"""
Cumulative Damage Factor (CDF) computation for NanoFlex.

Port of FaarFieldAnalysis/modCDF.vb — damage models, coverage-to-pass
via Gaussian wander integration, and CDF accumulation across aircraft
and lateral offsets.
"""

from __future__ import annotations

import math
from enum import Enum
from dataclasses import dataclass

import numpy as np
from numpy.typing import NDArray

# ═══════════════════════════════════════════════════════════════════════════════
#  Constants (matching modCDF.vb)
# ═══════════════════════════════════════════════════════════════════════════════

NOFF = 41                    # lateral offsets for CDF sweep
OFFSET_INC = 10.0            # inches between offsets
LOG10 = math.log(10.0)       # ln(10), used in base-10 log conversions
SIGMA_W = 30.435             # wander σ for 70-inch wander width
N_NODES_LONG = 1800          # longitudinal nodes for tandem CDF scan

# ── Gauss area integration constants ──────────────────────────────────────────
_INTMUL = 0.3989423          # 1/√(2π)
_N_GAUSS = 4
_HALF = 0.5
_CORREC = 1.0 / 24.0


# ═══════════════════════════════════════════════════════════════════════════════
#  Damage model selection
# ═══════════════════════════════════════════════════════════════════════════════

class SubgradeDamageModel(Enum):
    STANDARD = "standard"
    STRAIGHT_LINE = "straight_line"
    BLEASDALE = "bleasdale"


# ═══════════════════════════════════════════════════════════════════════════════
#  Gaussian wander distribution (GaussArea from modCDF.vb)
# ═══════════════════════════════════════════════════════════════════════════════

def gauss_area(ap: float, bp: float, sigma: float) -> float:
    """Area under Gaussian between ap and bp with std dev sigma.

    Uses Euler–McLaurin 4-point quadrature matching Numerical.vb exactly.
    Returns the probability that a wandering wheel covers this interval.
    """
    if sigma < 1e-6:
        return 1.0 if ap <= 0.0 <= bp else 0.0

    a, b = ap / sigma, bp / sigma
    if a > b:
        a, b = b, a

    ha, hb = a / _N_GAUSS, b / _N_GAUSS
    za, zb = -ha * _HALF, -hb * _HALF
    inta, intb = 0.0, 0.0

    for _ in range(_N_GAUSS):
        za += ha;  zb += hb
        inta += math.exp(-_HALF * za * za)
        intb += math.exp(-_HALF * zb * zb)

    za += ha * _HALF;  zb += hb * _HALF
    inta = ha * (inta - ha * za * math.exp(-_HALF * za * za) * _CORREC)
    intb = hb * (intb - hb * zb * math.exp(-_HALF * zb * zb) * _CORREC)

    if abs(a) > 5.0:
        inta = _HALF * (1 if a >= 0 else -1) / _INTMUL
    if b > 5.0:
        intb = _HALF / _INTMUL

    return (intb - inta) * _INTMUL


# ═══════════════════════════════════════════════════════════════════════════════
#  Subgrade damage models
# ═══════════════════════════════════════════════════════════════════════════════

def subgrade_n_to_fail(
    strain_max: float,
    subgrade_modulus: float,
    model: SubgradeDamageModel = SubgradeDamageModel.STANDARD,
) -> float:
    """Allowable load repetitions for subgrade vertical strain.

    Parameters
    ----------
    strain_max : absolute value of max compressive vertical strain
    subgrade_modulus : subgrade E in psi
    model : damage model to use

    Returns
    -------
    Number of load repetitions to failure (coverages).
    """
    strain = max(abs(strain_max), 1e-4)  # clamp to avoid log(0)

    if model == SubgradeDamageModel.STANDARD:
        aa = 0.000247 + 0.000245 * math.log(subgrade_modulus) / LOG10
        bb = 0.0658 * subgrade_modulus ** 0.559
        return 10000.0 * (aa / strain) ** bb

    # Straight-line and Bleasdale share the high-departure branch
    sub_mod_sl = 15000.0  # fixed modulus for large departure levels
    aa_orig = 0.000247 + 0.000245 * math.log(sub_mod_sl) / LOG10
    bb_orig = 0.0658 * sub_mod_sl ** 0.559
    aa_orig *= 10000.0 ** (1.0 / bb_orig)
    bb_sl = 8.1
    aa_sl = 0.004

    strain_break = 10.0 ** (
        (bb_sl * math.log(aa_sl) / LOG10 - bb_orig * math.log(aa_orig) / LOG10)
        / (bb_sl - bb_orig)
    )

    if model == SubgradeDamageModel.STRAIGHT_LINE:
        if strain > strain_break:
            return (aa_sl / strain) ** bb_sl
        return (aa_orig / strain) ** bb_orig

    # Bleasdale model (Bleasdale 2 coefficients)
    if model == SubgradeDamageModel.BLEASDALE:
        strain = max(strain, 0.001)
        a11 = -0.163768916705
        b11 = 185.192806802
        c11 = 1.65054449461
        if strain <= 0.001765093:
            return 10.0 ** ((a11 + b11 * strain) ** (-1.0 / c11))
        return (0.00414131183 / strain) ** 8.1

    return 1e15  # fallback


def asphalt_n_to_fail(
    strain_max: float,
    asphalt_modulus: float,
) -> float:
    """Standard asphalt fatigue model (AI-style).

    Returns allowable load repetitions for horizontal tensile strain
    at the bottom of the HMA layer.
    """
    strain = max(abs(strain_max), 1e-6)
    aa = 2.68 - 5.0 * math.log(strain) / LOG10
    bb = 2.665 * math.log(asphalt_modulus) / LOG10
    return 10.0 ** (aa - bb)


@dataclass
class RDECParams:
    """Mix-design parameters for the RDEC asphalt fatigue model."""
    flexural_modulus_psi: float = 600_000.0
    air_voids: float = 3.5           # percent
    asphalt_content_vol: float = 12.0  # percent by volume
    pnms: float = 95.0               # percent passing nominal max sieve
    ppcs: float = 58.0               # percent passing primary control sieve
    p200: float = 4.5                # percent passing No. 200 sieve


def rdec_n_to_fail(
    strain_max: float,
    params: RDECParams | None = None,
) -> float:
    """RDEC (Rate of Dissipated Energy Change) asphalt fatigue model.

    Port of the RDEC path in modCDF.vb LeafCDFFlex.  Used for P-209
    flexible base design in FAARFIELD.

    Parameters
    ----------
    strain_max : absolute tensile strain at bottom of HMA
    params : mix-design parameters (defaults match FAARFIELD defaults)

    Returns
    -------
    Allowable load repetitions to failure.
    """
    if params is None:
        params = RDECParams()

    strain = max(abs(strain_max), 1e-6)

    void_par = params.air_voids / (params.air_voids + params.asphalt_content_vol)
    grad_par = (params.pnms - params.ppcs) / max(params.p200, 0.01)

    # Flexural modulus converted to MPa (psi * 0.0068948)
    e_flex_mpa = params.flexural_modulus_psi * 0.0068948

    pv = (44.422
          * strain ** 5.14
          * e_flex_mpa ** 2.993
          * void_par ** 1.85
          * grad_par ** (-0.4063))

    return 0.4801 * pv ** (-0.90074)


# ═══════════════════════════════════════════════════════════════════════════════
#  Coverage-to-pass (simplified general model)
# ═══════════════════════════════════════════════════════════════════════════════

def coverage_to_pass_flex(
    wheel_x: list[float],
    tire_width: float,
    depth: float,
    offset: float,
    sigma: float = SIGMA_W,
) -> float:
    """Coverage-to-pass ratio for a set of wheels at a lateral offset.

    Integrates the Gaussian wander probability that wheels pass over
    the evaluation point at the given lateral offset.

    Parameters
    ----------
    wheel_x : lateral (X) coordinates of the southernmost row of wheels (inches)
    tire_width : effective tire contact width (inches)
    depth : evaluation depth (inches) — affects effective footprint
    offset : lateral offset from gear centerline (inches)
    sigma : standard deviation of lateral wander (inches), default 30.435 (70" wander)

    Returns
    -------
    Coverage-to-pass ratio (0–1 range per wheel row).
    """
    gp = abs(depth) + tire_width  # effective tire pass width
    n_w = len(wheel_x)
    if n_w == 0:
        return 0.0

    # Centre the gear around the evaluation line
    x_center = sum(wheel_x) / n_w

    # Left/right integration limits per wheel
    lefts = [gp / 2.0] * n_w
    rights = [gp / 2.0] * n_w
    for i in range(n_w - 1):
        gap = abs(wheel_x[i] - wheel_x[i + 1])
        if gap < gp:
            rights[i] = gap / 2.0
            lefts[i + 1] = gap / 2.0

    total = 0.0
    for i in range(n_w):
        yoff = offset + (-x_center + wheel_x[i])
        a = yoff - lefts[i]
        b = yoff + rights[i]
        total += gauss_area(a, b, sigma)

    return total


# ═══════════════════════════════════════════════════════════════════════════════
#  Coverage-to-pass for general gear (CoverageToPassFlexGeneral13B)
# ═══════════════════════════════════════════════════════════════════════════════

def coverage_to_pass_general(
    wheel_x: list[float],
    wheel_y: list[float],
    tire_width: float,
    depth: float,
    offset: float,
    sigma: float = SIGMA_W,
) -> float:
    """Coverage-to-pass for general gear type "X" with tandem handling.

    Port of CoverageToPassFlexGeneral13B from modCDF.vb.  Identifies the
    southernmost (lowest-Y) row of wheels, computes a tandem multiplier
    based on depth and inter-axle spacing, and integrates the Gaussian
    wander probability.

    Parameters
    ----------
    wheel_x, wheel_y : coordinates of all tires (inches)
    tire_width : effective tire contact width (inches)
    depth : evaluation depth (inches)
    offset : cumulative lateral offset (inches, incremented externally)
    sigma : wander standard deviation (inches)
    """
    n_tires = len(wheel_x)
    if n_tires == 0:
        return 0.0

    # Sort tires by Y coordinate (ascending) to identify bottom row
    order = sorted(range(n_tires), key=lambda i: wheel_y[i])
    sx = [wheel_x[i] for i in order]
    sy = [wheel_y[i] for i in order]

    tw = tire_width

    # Identify bottom wheels: a wheel is "bottom" if no other wheel in the
    # same X-column (within tw/2) has a smaller Y.
    is_bottom = [True] * n_tires
    col_parent = [-1] * n_tires  # index of the bottom wheel in same column
    for i in range(n_tires):
        for j in range(i + 1, n_tires):
            if abs(sx[j] - sx[i]) <= tw / 2.0:
                if is_bottom[i]:
                    is_bottom[j] = False
                    col_parent[j] = i
                else:
                    is_bottom[j] = False
                    col_parent[j] = col_parent[i] if col_parent[i] >= 0 else i

    # For each bottom wheel, collect the tandem spacing chain
    td_multiplier: dict[int, float] = {}
    for i in range(n_tires):
        if not is_bottom[i]:
            continue
        # Gather Y values of wheels in this column (sorted ascending)
        col_ys = sorted(
            sy[j] for j in range(n_tires)
            if j != i and abs(sx[j] - sx[i]) <= tw / 2.0
        )
        mult = 1.0
        prev_y = sy[i]
        for cy in col_ys:
            td = cy - prev_y
            gap = td - tw
            if gap <= 0:
                gap = 0.001
            if depth > 2.0 * gap:
                pass  # fully overlapping — no multiplier change
            elif depth > gap:
                mult += 2.0 - depth / gap
            else:
                mult += 1.0
            prev_y = cy
        td_multiplier[i] = mult

    # Extract bottom wheels sorted by X
    bottom_indices = sorted(
        [i for i in range(n_tires) if is_bottom[i]], key=lambda i: sx[i]
    )
    bottom_x = [sx[i] for i in bottom_indices]
    bottom_td = [td_multiplier[i] for i in bottom_indices]
    n_bw = len(bottom_x)
    if n_bw == 0:
        return 0.0

    # Build left/right integration limits (same as simplified version)
    gp = abs(depth) + tw
    lefts = [0.0] * n_bw
    rights = [0.0] * n_bw
    lefts[0] = bottom_x[0] - gp / 2.0
    for i in range(n_bw - 1):
        mid = (bottom_x[i] + bottom_x[i + 1]) / 2.0
        if bottom_x[i] + gp / 2.0 < mid:
            rights[i] = bottom_x[i] + gp / 2.0
            lefts[i + 1] = bottom_x[i + 1] - gp / 2.0
        else:
            rights[i] = mid
            lefts[i + 1] = mid
    rights[n_bw - 1] = bottom_x[n_bw - 1] + gp / 2.0

    # Apply lateral offset (cumulative shift)
    for i in range(n_bw):
        lefts[i] += offset
        rights[i] += offset

    # Sum Gaussian area weighted by tandem multiplier
    ctp = 0.0
    for i in range(n_bw):
        if lefts[i] < 0 and rights[i] < 0:
            area = gauss_area(abs(lefts[i]), abs(rights[i]), sigma)
        else:
            area = gauss_area(lefts[i], rights[i], sigma)
        ctp += area * bottom_td[i]

    return ctp


# ═══════════════════════════════════════════════════════════════════════════════
#  Tandem CDF: longitudinal strain scanning (gTandemFnew path)
# ═══════════════════════════════════════════════════════════════════════════════

def compute_tandem_strains(
    solver,
    aircraft_list,
    structure,
    n_nodes_long: int = N_NODES_LONG,
):
    """Two-pass LEAF computation for the tandem CDF method.

    Port of ComputeResponse2 from clsLEAF.vb.

    Pass 1: standard eval points → locate the transverse offset where
            vertical strain is most compressive for each aircraft.
    Pass 2: generate *n_nodes_long* eval points along the longitudinal (Y)
            direction at that offset and re-run the LEAF integration.

    Parameters
    ----------
    solver : LEAFSolver instance (reusable quadrature weights)
    aircraft_list : list of LEAFAircraft (original eval points)
    structure : LEAFStructure
    n_nodes_long : number of longitudinal eval nodes (default 1800)

    Returns
    -------
    NDArray[nac+1, n_nodes_long+1] — longitudinal strain profiles (1-based).
    """
    from leaf import LEAFAircraft, ResponseType

    # Pass 1: standard computation at original eval points
    strain1 = solver.compute_response(
        ResponseType.VERTICAL_STRAIN, aircraft_list, structure)

    modified_ac: list[LEAFAircraft] = []
    for idx, ac in enumerate(aircraft_list):
        iac = idx + 1

        # Find X offset where strain is most compressive (most negative)
        response_max = 10.0  # sentinel (positive → less than any compressive)
        offset_max = ac.eval_x[1] if ac.n_eval_points >= 1 else 0.0
        for ie in range(1, ac.n_eval_points + 1):
            if strain1[iac, ie] < response_max:
                response_max = strain1[iac, ie]
                offset_max = ac.eval_x[ie]

        # Tire Y range with padding (matching FAARFIELD's ±160 inches)
        tire_y_min = min(ac.tire_y[it] for it in range(1, ac.n_tires + 1))
        tire_y_max = max(ac.tire_y[it] for it in range(1, ac.n_tires + 1))
        tire_y_min -= 160.0
        tire_y_max += 160.0

        # Build longitudinal eval grid
        new_eval_x = np.zeros(n_nodes_long + 1)
        new_eval_y = np.zeros(n_nodes_long + 1)
        for i in range(1, n_nodes_long + 1):
            new_eval_x[i] = offset_max
            new_eval_y[i] = (tire_y_min
                             + i * (tire_y_max - tire_y_min) / n_nodes_long)

        modified_ac.append(LEAFAircraft(
            name=ac.name, gear_load=ac.gear_load, n_tires=ac.n_tires,
            tire_press=ac.tire_press.copy(), tire_x=ac.tire_x.copy(),
            tire_y=ac.tire_y.copy(),
            n_eval_points=n_nodes_long, eval_x=new_eval_x, eval_y=new_eval_y,
            gear=ac.gear, gear_orientation=ac.gear_orientation,
        ))

    # Pass 2: LEAF at longitudinal eval points
    return solver.compute_response(
        ResponseType.VERTICAL_STRAIN, modified_ac, structure)


def scan_tandem_damage(
    strain_profile: NDArray,
    n_nodes_long: int,
    subgrade_modulus: float,
    damage_model: SubgradeDamageModel = SubgradeDamageModel.STANDARD,
) -> tuple[float, float]:
    """Scan longitudinal strain profile for peaks/valleys.

    Port of the ``kairat replace tandem`` block in LeafCDFFlex (modCDF.vb).
    Identifies local extrema in the compressive strain profile along the
    longitudinal direction and accumulates signed damage:

    - Local valley (more compressive, ExtrType=2): damage += 1/NtoFail
    - Local peak  (less compressive, ExtrType=1): damage -= 1/NtoFail

    Parameters
    ----------
    strain_profile : 1-based array of vertical strain values [1..n_nodes_long]
    n_nodes_long : number of longitudinal nodes
    subgrade_modulus : subgrade E (psi)
    damage_model : failure criterion

    Returns
    -------
    (damage, strain_max_abs) — total signed damage and the maximum
    absolute strain encountered among the extrema.
    """
    damage = 0.0
    strain_max_abs = 0.0

    for i in range(2, n_nodes_long):
        if i >= strain_profile.shape[0]:
            break
        if strain_profile[i] >= 0:
            continue  # only compressive (negative) strains

        s_prev = strain_profile[i - 1]
        s_curr = strain_profile[i]
        s_next = strain_profile[i + 1] if i + 1 < strain_profile.shape[0] else 0.0

        extr_type = 0
        # Local maximum (peak — less compressive)
        if s_prev < s_curr and s_curr > s_next:
            extr_type = 1
        # Local minimum (valley — more compressive)
        elif s_prev > s_curr and s_curr < s_next:
            extr_type = 2

        if extr_type == 0:
            continue

        sm = abs(s_curr)
        if sm > strain_max_abs:
            strain_max_abs = sm
        ntf = subgrade_n_to_fail(sm, subgrade_modulus, damage_model)
        # (-1)^ExtrType: peak subtracts, valley adds
        damage += ((-1) ** extr_type) * (1.0 / ntf)

    return damage, strain_max_abs


# ═══════════════════════════════════════════════════════════════════════════════
#  CDF accumulation across aircraft and offsets
# ═══════════════════════════════════════════════════════════════════════════════

@dataclass
class CDFResult:
    """Results of a CDF computation."""
    cdf_max: float                       # maximum CDF across all offsets
    cdf_by_offset: NDArray[np.float64]   # CDF at each of 41 offsets
    cdf_by_aircraft: NDArray[np.float64] # CDF per aircraft at the critical offset
    critical_offset_index: int           # 1-based index of max-CDF offset
    n_to_fail: NDArray[np.float64]       # NtoFail per aircraft
    strain_max: NDArray[np.float64]      # max strain per aircraft


def leaf_cdf_flex(
    strain_response: NDArray[np.float64],
    reps: list[float],
    n_eval_pts: list[int],
    wheel_x_per_ac: list[list[float]],
    tire_width_per_ac: list[float],
    eval_depth: float,
    subgrade_modulus: float,
    asphalt_modulus: float = 200_000.0,
    is_subgrade: bool = True,
    damage_model: SubgradeDamageModel = SubgradeDamageModel.STANDARD,
    sigma_w: float = SIGMA_W,
    use_rdec: bool = False,
    rdec_params: RDECParams | None = None,
    gear_types: list[str] | None = None,
    wheel_y_per_ac: list[list[float]] | None = None,
    use_tandem: bool = False,
    tandem_strain_response: NDArray[np.float64] | None = None,
    n_nodes_long: int = N_NODES_LONG,
) -> CDFResult:
    """Compute CDF for flexible pavement (subgrade or asphalt).

    Parameters
    ----------
    strain_response : array[nac+1, n_eval_max+1] of strains (1-based).
                      Used for max strain (non-tandem) or asphalt CDF.
    reps : total departures per aircraft (0-based list, len=nac)
    n_eval_pts : eval points per aircraft (0-based list, len=nac)
    wheel_x_per_ac : wheel X-coordinates per aircraft (0-based list)
    tire_width_per_ac : tire contact width per aircraft (0-based list)
    eval_depth : depth of evaluation (inches)
    subgrade_modulus : subgrade E (psi)
    asphalt_modulus : asphalt E (psi), used if is_subgrade=False
    is_subgrade : True for subgrade criterion, False for asphalt
    damage_model : subgrade failure model selection
    sigma_w : wander sigma (inches)
    use_rdec : if True, use RDEC fatigue model for asphalt CDF
    rdec_params : mix-design parameters for RDEC (defaults used if None)
    gear_types : gear type per aircraft (if "X", use general gear coverage)
    wheel_y_per_ac : wheel Y-coordinates per aircraft (needed for general gear)
    use_tandem : if True, use longitudinal strain scanning (gTandemFnew)
    tandem_strain_response : longitudinal strain profiles from
                             compute_tandem_strains (required when use_tandem)
    n_nodes_long : longitudinal node count for tandem scanning

    Returns
    -------
    CDFResult with CDF values, max strain, and NtoFail per aircraft.
    """
    nac = len(reps)
    cdf_flex_val = np.zeros(NOFF + 1)  # 1-based
    cdf_by_ac = np.zeros(nac)
    n_to_fail_arr = np.zeros(nac)
    strain_max_arr = np.zeros(nac)
    damage_arr = np.zeros(nac)  # effective 1/NtoFail when tandem is active

    for ia_0 in range(nac):
        ia = ia_0 + 1  # 1-based index into strain_response

        if use_tandem and is_subgrade and tandem_strain_response is not None:
            # Tandem CDF: scan longitudinal strain profile for peaks/valleys
            damage, s_max = scan_tandem_damage(
                tandem_strain_response[ia, :], n_nodes_long,
                subgrade_modulus, damage_model,
            )
            damage_arr[ia_0] = damage
            strain_max_arr[ia_0] = s_max
            n_to_fail_arr[ia_0] = (1.0 / damage) if abs(damage) > 1e-30 else 1e15
        else:
            # Standard path: max strain across evaluation points
            strain_max = 0.0
            for ie in range(1, n_eval_pts[ia_0] + 1):
                if abs(strain_response[ia, ie]) > strain_max:
                    strain_max = abs(strain_response[ia, ie])

            strain_max_arr[ia_0] = strain_max

            if is_subgrade:
                ntf = subgrade_n_to_fail(strain_max, subgrade_modulus, damage_model)
            elif use_rdec:
                ntf = rdec_n_to_fail(strain_max, rdec_params)
            else:
                ntf = asphalt_n_to_fail(strain_max, asphalt_modulus)
            n_to_fail_arr[ia_0] = ntf
            damage_arr[ia_0] = 1.0 / ntf if ntf > 0 else 0.0

        # Choose coverage-to-pass function.
        # When tandem is active, coverage does NOT include the tandem
        # multiplier (tandem effects are captured via strain scanning).
        use_general = (
            gear_types is not None
            and ia_0 < len(gear_types)
            and gear_types[ia_0] == "X"
            and wheel_y_per_ac is not None
            and not use_tandem  # disable general-gear multiplier in tandem mode
        )

        # CDF across 41 lateral offsets
        offset = 0.0
        for ioff in range(1, NOFF + 1):
            if use_general:
                cov_to_pass = coverage_to_pass_general(
                    wheel_x_per_ac[ia_0], wheel_y_per_ac[ia_0],
                    tire_width_per_ac[ia_0], eval_depth, offset, sigma_w,
                )
            else:
                cov_to_pass = coverage_to_pass_flex(
                    wheel_x_per_ac[ia_0], tire_width_per_ac[ia_0],
                    eval_depth, offset, sigma_w,
                )
            accdf = reps[ia_0] * cov_to_pass * damage_arr[ia_0]
            cdf_flex_val[ioff] += accdf
            if ioff == 1 or accdf > cdf_by_ac[ia_0]:
                cdf_by_ac[ia_0] = accdf
            offset += OFFSET_INC

    # Find maximum CDF
    cdf_max = 0.0
    i_control = 1
    for ioff in range(1, NOFF + 1):
        if cdf_flex_val[ioff] > cdf_max:
            cdf_max = cdf_flex_val[ioff]
            i_control = ioff

    # CDF per aircraft at critical offset
    cdf_at_crit = np.zeros(nac)
    offset_crit = (i_control - 1) * OFFSET_INC
    for ia_0 in range(nac):
        use_general = (
            gear_types is not None
            and ia_0 < len(gear_types)
            and gear_types[ia_0] == "X"
            and wheel_y_per_ac is not None
            and not use_tandem
        )
        if use_general:
            cov = coverage_to_pass_general(
                wheel_x_per_ac[ia_0], wheel_y_per_ac[ia_0],
                tire_width_per_ac[ia_0], eval_depth, offset_crit, sigma_w,
            )
        else:
            cov = coverage_to_pass_flex(
                wheel_x_per_ac[ia_0], tire_width_per_ac[ia_0],
                eval_depth, offset_crit, sigma_w,
            )
        cdf_at_crit[ia_0] = reps[ia_0] * cov * damage_arr[ia_0]

    return CDFResult(
        cdf_max=cdf_max,
        cdf_by_offset=cdf_flex_val[1:],
        cdf_by_aircraft=cdf_at_crit,
        critical_offset_index=i_control,
        n_to_fail=n_to_fail_arr,
        strain_max=strain_max_arr,
    )
