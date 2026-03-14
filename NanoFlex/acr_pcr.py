"""
ACR/PCR computation engine for NanoFlex.

Port of ACNClassLib/clsACR.vb and FaarFieldAnalysis/modFedfaaGbl.vb.

- ACR (Aircraft Classification Rating): DSWL-based rating on ICAO reference structures
- PCR (Pavement Classification Rating): ACR of the critical aircraft at max allowable weight
"""

from __future__ import annotations

import math
from copy import deepcopy
from dataclasses import dataclass
from typing import Optional

import numpy as np
from numpy.typing import NDArray

from leaf import LEAFSolver, LEAFAircraft, LEAFStructure, ResponseType
from cdf import (
    leaf_cdf_flex, SubgradeDamageModel, asphalt_n_to_fail,
    NOFF, SIGMA_W, compute_tandem_strains,
)
from structures import PavementSection, PavementLayer, TrafficAircraft
from design_flex import design_flex, compute_life

# ═══════════════════════════════════════════════════════════════════════════════
#  Constants (from ACNClassLib/Print.vb and clsACR.vb)
# ═══════════════════════════════════════════════════════════════════════════════

LBS_TO_KG = 0.453592
MPA_TO_PSI = 145.03773773
C1000 = 100                          # ACR denominator (per ICAO definition)
STRESS_ACN = 2.75 * MPA_TO_PSI       # 398.85 psi — rigid target stress
COV_ACN = 36500                      # flexible target coverages

# ICAO subgrade moduli (MPa converted to psi)
SUBGRADE_MODULI_MPA = {"A": 200, "B": 120, "C": 80, "D": 50}
SUBGRADE_MODULI_PSI = {k: v * MPA_TO_PSI for k, v in SUBGRADE_MODULI_MPA.items()}

# Reference SWL tire pressure: 1.5 MPa = 217.56 psi
SWL_TIRE_PRESSURE = 1.5 * MPA_TO_PSI

# Reference HMA thickness: 76mm (≤2 wheels) or 127mm (>2 wheels) → inches
HMA_THICK_LE2 = 76.0 / 25.4    # ≈ 2.992 in
HMA_THICK_GT2 = 127.0 / 25.4   # ≈ 5.0 in

# Reference moduli
HMA_MODULUS = 1379.0 * MPA_TO_PSI    # ≈ 200,000 psi
BASE_MODULUS = 400.0 * MPA_TO_PSI    # ≈ 58,015 psi
PCC_MODULUS = 27579.0 * MPA_TO_PSI   # ≈ 4,000,000 psi
BASE_MODULUS_RIGID = 500.0 * MPA_TO_PSI  # ≈ 72,519 psi

# Strain target for flexible ACR
STRAIN_ACN = 0.00132524078262523

PI = 3.14159265359


# ═══════════════════════════════════════════════════════════════════════════════
#  Data structures
# ═══════════════════════════════════════════════════════════════════════════════

@dataclass
class ACRResult:
    """Result of an ACR computation for one aircraft."""
    acr: dict[str, float]                # ACR per subgrade category {A,B,C,D}
    dswl_lbs: dict[str, float]           # DSWL in pounds per category
    reference_thickness: dict[str, float]  # base thickness per category (inches)
    aircraft_name: str = ""


@dataclass
class PCRResult:
    """Result of a PCR computation for a pavement section."""
    pcr: float                           # the PCR value
    critical_aircraft: str               # name of the critical aircraft
    max_gross_weight: float              # MGW in lbs
    subgrade_category: str               # A/B/C/D
    life_years: float                    # structural life in years
    acr_at_mgw: float                    # ACR of critical aircraft at MGW


# ═══════════════════════════════════════════════════════════════════════════════
#  ICAO reference structure builders
# ═══════════════════════════════════════════════════════════════════════════════

def _build_flex_reference(
    subgrade_cat: str, n_wheels: int, base_thickness: float
) -> PavementSection:
    """Build the ICAO flexible reference structure."""
    hma_thick = HMA_THICK_LE2 if n_wheels <= 2 else HMA_THICK_GT2
    sg_mod = SUBGRADE_MODULI_PSI[subgrade_cat]

    section = PavementSection(name=f"Flex Ref {subgrade_cat}")
    section.layers = [
        PavementLayer("P-401/P-403 HMA Surface", hma_thick, HMA_MODULUS, poisson=0.35),
        PavementLayer("P-209 Crushed Aggregate", base_thickness, BASE_MODULUS, poisson=0.35),
        PavementLayer("Subgrade", 0.0, sg_mod, poisson=0.35),
    ]
    return section


def _build_rigid_reference(
    subgrade_cat: str, pcc_thickness: float
) -> PavementSection:
    """Build the ICAO rigid reference structure."""
    sg_mod = SUBGRADE_MODULI_PSI[subgrade_cat]
    base_thick = 200.0 / 25.4  # 200mm → inches

    section = PavementSection(name=f"Rigid Ref {subgrade_cat}")
    section.layers = [
        PavementLayer("P-501 PCC Surface", pcc_thickness, PCC_MODULUS,
                      poisson=0.15, interface_bond=0.0),
        PavementLayer("P-209 Crushed Aggregate", base_thick, BASE_MODULUS_RIGID, poisson=0.35),
        PavementLayer("Subgrade", 0.0, sg_mod, poisson=0.40),
    ]
    return section


# ═══════════════════════════════════════════════════════════════════════════════
#  DSWL iteration for flexible
# ═══════════════════════════════════════════════════════════════════════════════

def _compute_flex_coverages(
    gear_load_lbs: float,
    section: PavementSection,
    solver: LEAFSolver,
) -> float:
    """Compute allowable coverages (NtoFail) for a single wheel on a structure.

    Uses horizontal strain at bottom of HMA → AI fatigue model.
    """
    # Single-wheel aircraft on reference structure
    tp = np.array([0.0, SWL_TIRE_PRESSURE])
    tx = np.array([0.0, 0.0]);  ty = np.array([0.0, 0.0])
    ex = np.array([0.0, 0.0]);  ey = np.array([0.0, 0.0])

    ac = LEAFAircraft(
        name="SWL", gear_load=gear_load_lbs, n_tires=1,
        tire_press=tp, tire_x=tx, tire_y=ty,
        n_eval_points=1, eval_x=ex, eval_y=ey,
    )

    eval_depth = section.layers[0].thickness
    leaf_struct = section.to_leaf_structure(eval_depth, 1)

    resp = solver.compute_response(
        ResponseType.HORIZONTAL_STRESS, [ac], leaf_struct)

    strain_max = abs(resp[1, 1])
    if strain_max < 1e-10:
        return 1e15
    return asphalt_n_to_fail(strain_max, HMA_MODULUS)


def _dswl_flex(
    ref_section: PavementSection,
    solver: LEAFSolver,
    initial_load: float = 50000.0,
) -> float:
    """Find the SWL that produces exactly COV_ACN coverages on the reference.

    Uses bisection with linear interpolation in strain space.
    """
    target = COV_ACN

    # Bracketing phase
    load_lo = initial_load * 0.1
    load_hi = initial_load * 3.0
    cov_lo = _compute_flex_coverages(load_lo, ref_section, solver)
    cov_hi = _compute_flex_coverages(load_hi, ref_section, solver)

    # Adjust brackets: higher load → lower coverages
    for _ in range(20):
        if cov_lo > target > cov_hi:
            break
        if cov_lo <= target:
            load_lo *= 0.5
            cov_lo = _compute_flex_coverages(load_lo, ref_section, solver)
        if cov_hi >= target:
            load_hi *= 2.0
            cov_hi = _compute_flex_coverages(load_hi, ref_section, solver)

    # Bisection with interpolation
    for _ in range(50):
        if abs(cov_lo - cov_hi) < 1.0:
            break
        # Log-linear interpolation
        if cov_lo > 0 and cov_hi > 0:
            frac = (math.log(target) - math.log(cov_hi)) / (
                    math.log(cov_lo) - math.log(cov_hi))
        else:
            frac = 0.5
        frac = max(0.01, min(0.99, frac))
        load_mid = load_hi + frac * (load_lo - load_hi)
        cov_mid = _compute_flex_coverages(load_mid, ref_section, solver)

        if abs(cov_mid - target) / target < 0.001:
            return load_mid

        if cov_mid > target:
            load_lo, cov_lo = load_mid, cov_mid
        else:
            load_hi, cov_hi = load_mid, cov_mid

    return (load_lo + load_hi) / 2.0


# ═══════════════════════════════════════════════════════════════════════════════
#  ACR computation
# ═══════════════════════════════════════════════════════════════════════════════

def compute_acr(
    aircraft: TrafficAircraft,
    categories: list[str] | None = None,
    verbose: bool = False,
) -> ACRResult:
    """Compute ACR for an aircraft across subgrade categories.

    Parameters
    ----------
    aircraft : aircraft definition (gear geometry, weight, tire pressure)
    categories : list of subgrade categories to compute (default all A–D)
    verbose : print progress

    Returns
    -------
    ACRResult with ACR values per subgrade category.
    """
    if categories is None:
        categories = ["A", "B", "C", "D"]

    solver = LEAFSolver()
    result_acr: dict[str, float] = {}
    result_dswl: dict[str, float] = {}
    result_thick: dict[str, float] = {}

    for cat in categories:
        if verbose:
            print(f"  Computing ACR for category {cat}...")

        # Step 1: Design the reference structure
        ref = _build_flex_reference(cat, aircraft.n_wheels, 10.0)

        # Traffic for design: single aircraft, target coverages
        ref.traffic = [deepcopy(aircraft)]
        ref.traffic[0].annual_departures = int(COV_ACN / 20)
        ref.design_life = 20

        # Design the base thickness
        design_result = design_flex(
            ref, design_layer_index=1,
            compute_asphalt_cdf=False,
            verbose=verbose,
        )
        base_thick = ref.layers[1].thickness

        if verbose:
            print(f"    Reference base thickness: {base_thick:.2f} in")

        # Step 2: Find DSWL
        dswl = _dswl_flex(ref, solver, initial_load=aircraft.gear_load)

        # Step 3: Compute ACR
        acr = 2.0 * dswl * LBS_TO_KG / C1000

        if verbose:
            print(f"    DSWL = {dswl:.0f} lbs, ACR = {acr:.1f}")

        result_acr[cat] = acr
        result_dswl[cat] = dswl
        result_thick[cat] = base_thick

    return ACRResult(
        acr=result_acr,
        dswl_lbs=result_dswl,
        reference_thickness=result_thick,
        aircraft_name=aircraft.name,
    )


# ═══════════════════════════════════════════════════════════════════════════════
#  PCR computation
# ═══════════════════════════════════════════════════════════════════════════════

def _classify_subgrade(modulus_psi: float) -> str:
    """Classify subgrade modulus into ICAO category A–D."""
    if modulus_psi >= 200.0 * MPA_TO_PSI:
        return "A"
    elif modulus_psi >= 120.0 * MPA_TO_PSI:
        return "B"
    elif modulus_psi >= 80.0 * MPA_TO_PSI:
        return "C"
    else:
        return "D"


@dataclass
class _RoundResult:
    """Internal result from one PCR elimination round."""
    crit_ac_name: str
    crit_ac_original_idx: int
    mgw: float
    pcr: float


def compute_pcr(
    section: PavementSection,
    verbose: bool = False,
) -> PCRResult:
    """Compute PCR for a pavement section with its traffic mix.

    Port of the full FAARFIELD iterative elimination algorithm from
    modFedfaaGbl.vb (StepsToDoM loop).

    Algorithm per round:
      1. Compute CDF distribution for current traffic mix.
      2. Find the critical aircraft (largest CDF at the critical offset).
      3. Isolate critical aircraft → adjust departures so CDF matches
         its contribution → iterate gross weight until CDF = 1.0 → MGW.
      4. Compute ACR at MGW → PCR for this round.
      5. Remove the critical aircraft and repeat with the reduced mix.

    The reported PCR is the maximum across all rounds.  Early exit
    occurs when the round's critical aircraft is the one with the
    highest ACR in the original mix.

    Parameters
    ----------
    section : pavement section with layers and traffic mix
    verbose : print progress

    Returns
    -------
    PCRResult with PCR value and supporting information.
    """
    if not section.traffic:
        raise ValueError("No traffic mix defined")

    solver = LEAFSolver()
    subgrade_modulus = section.layers[-1].modulus
    sg_cat = _classify_subgrade(subgrade_modulus)

    # Pre-compute ACR for every aircraft to find gACRmaxIndex
    acr_values: list[float] = []
    for ac in section.traffic:
        ar = compute_acr(ac, categories=[sg_cat], verbose=False)
        acr_values.append(ar.acr[sg_cat])
    acr_max_original_idx = int(np.argmax(acr_values))

    if verbose:
        print(f"  Max ACR aircraft: {section.traffic[acr_max_original_idx].name}"
              f" (ACR={acr_values[acr_max_original_idx]:.1f})")

    # Build a working copy of the traffic mix with original indices
    remaining: list[tuple[int, TrafficAircraft]] = [
        (i, deepcopy(ac)) for i, ac in enumerate(section.traffic)
    ]

    best_pcr = 0.0
    best_result: _RoundResult | None = None
    round_results: list[_RoundResult] = []

    for round_num in range(len(section.traffic)):
        if not remaining:
            break

        # Build a section copy with the remaining traffic
        sec_round = deepcopy(section)
        sec_round.traffic = [ac for _, ac in remaining]

        # Step 1–2: Compute CDF and find critical aircraft
        cdf_result = _compute_cdf_for_section(sec_round, solver)

        crit_local_idx = int(np.argmax(cdf_result.cdf_by_aircraft))
        crit_original_idx = remaining[crit_local_idx][0]
        crit_ac = remaining[crit_local_idx][1]
        crit_cdf = cdf_result.cdf_by_aircraft[crit_local_idx]

        if verbose:
            print(f"  Round {round_num+1}: critical = {crit_ac.name}"
                  f" (CDF={crit_cdf:.4f})")

        # Step 3: Find MGW for the critical aircraft
        mgw = _find_mgw(sec_round, crit_local_idx, solver, verbose)

        # Step 4: Compute ACR at MGW → PCR
        ac_at_mgw = deepcopy(crit_ac)
        ac_at_mgw.gross_weight = mgw
        acr_result = compute_acr(ac_at_mgw, categories=[sg_cat], verbose=False)
        pcr_value = acr_result.acr[sg_cat]

        rr = _RoundResult(
            crit_ac_name=crit_ac.name,
            crit_ac_original_idx=crit_original_idx,
            mgw=mgw, pcr=pcr_value,
        )
        round_results.append(rr)

        if pcr_value > best_pcr:
            best_pcr = pcr_value
            best_result = rr

        if verbose:
            print(f"    MGW={mgw:.0f} lbs, PCR={pcr_value:.1f}")

        # Early exit: critical aircraft is the max-ACR aircraft
        if crit_original_idx == acr_max_original_idx:
            if verbose:
                print("  Early exit: critical aircraft = max ACR aircraft")
            break

        # Step 5: Remove the critical aircraft from remaining
        remaining.pop(crit_local_idx)

    if best_result is None:
        raise RuntimeError("PCR computation produced no results")

    life_years = compute_life(section)

    return PCRResult(
        pcr=best_pcr,
        critical_aircraft=best_result.crit_ac_name,
        max_gross_weight=best_result.mgw,
        subgrade_category=sg_cat,
        life_years=life_years,
        acr_at_mgw=best_pcr,
    )


def _compute_cdf_for_section(
    section: PavementSection, solver: LEAFSolver,
) -> 'CDFResult':
    """Compute CDF distribution for a section (helper for PCR rounds)."""
    from cdf import CDFResult as _CDFResult  # avoid circular at top level
    leaf_aircraft = section.to_leaf_aircraft()
    nlay = len(section.layers)
    eval_depth = sum(l.thickness for l in section.layers[:-1])
    leaf_struct = section.to_leaf_structure(eval_depth, nlay)

    strain_resp = solver.compute_response(
        ResponseType.VERTICAL_STRAIN, leaf_aircraft, leaf_struct)
    tandem_resp = compute_tandem_strains(solver, leaf_aircraft, leaf_struct)

    subgrade_modulus = section.layers[-1].modulus

    wheel_x_per_ac = [list(ac.wheel_x) for ac in section.traffic]
    tire_width_per_ac = []
    reps_list: list[float] = []
    n_eval_list: list[int] = []
    for ac in section.traffic:
        tw = math.sqrt(ac.gear_load / ac.n_wheels / (ac.tire_pressure * PI))
        tire_width_per_ac.append(tw * 2.0)
        reps_list.append(ac.total_departures(section.design_life))
        n_eval_list.append(len(ac.eval_x))

    return leaf_cdf_flex(
        strain_resp, reps_list, n_eval_list,
        wheel_x_per_ac, tire_width_per_ac,
        eval_depth, subgrade_modulus,
        use_tandem=True, tandem_strain_response=tandem_resp,
    )


def _find_mgw(
    section: PavementSection,
    crit_ac_idx: int,
    solver: LEAFSolver,
    verbose: bool = False,
) -> float:
    """Find max gross weight for CDF = 1.0 using secant method."""
    crit_ac = section.traffic[crit_ac_idx]

    def _cdf_at_weight(gw: float) -> float:
        sec_copy = deepcopy(section)
        sec_copy.traffic = [deepcopy(crit_ac)]
        sec_copy.traffic[0].gross_weight = gw

        leaf_ac = sec_copy.to_leaf_aircraft()
        nlay = len(sec_copy.layers)
        eval_depth = sum(l.thickness for l in sec_copy.layers[:-1])
        leaf_struct = sec_copy.to_leaf_structure(eval_depth, nlay)

        strain_resp = solver.compute_response(
            ResponseType.VERTICAL_STRAIN, leaf_ac, leaf_struct)
        tandem_resp = compute_tandem_strains(solver, leaf_ac, leaf_struct)

        wx = [list(sec_copy.traffic[0].wheel_x)]
        tw_val = math.sqrt(sec_copy.traffic[0].gear_load /
                           sec_copy.traffic[0].n_wheels /
                           (sec_copy.traffic[0].tire_pressure * PI))
        tw = [tw_val * 2.0]
        reps = [sec_copy.traffic[0].total_departures(sec_copy.design_life)]
        ne = [len(sec_copy.traffic[0].eval_x)]

        r = leaf_cdf_flex(
            strain_resp, reps, ne, wx, tw,
            eval_depth, sec_copy.layers[-1].modulus,
            use_tandem=True, tandem_strain_response=tandem_resp,
        )
        return r.cdf_max

    # Secant iteration
    gw1 = crit_ac.gross_weight * 0.5
    gw2 = crit_ac.gross_weight * 1.5
    cdf1 = _cdf_at_weight(gw1)
    cdf2 = _cdf_at_weight(gw2)

    for iteration in range(30):
        if abs(cdf2 - 1.0) < 0.01:
            break
        if abs(cdf2 - cdf1) < 1e-10:
            break
        gw_new = gw1 + (1.0 - cdf1) * (gw2 - gw1) / (cdf2 - cdf1)
        gw_new = max(1000.0, min(gw_new, crit_ac.gross_weight * 5.0))
        gw1, cdf1 = gw2, cdf2
        gw2 = gw_new
        cdf2 = _cdf_at_weight(gw2)
        if verbose:
            print(f"    MGW iter {iteration+1}: GW={gw2:.0f} lbs, CDF={cdf2:.4f}")

    return gw2
