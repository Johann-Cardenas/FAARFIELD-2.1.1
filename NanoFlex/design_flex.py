"""
Iterative flexible pavement thickness design for NanoFlex.

Port of FaarFieldAnalysis/modStrDesignFlex.vb — Newton-Raphson iteration
on the design layer thickness until CDF_subgrade ≈ 1.0.
"""

from __future__ import annotations

import math
from dataclasses import dataclass, field
from typing import Optional

import numpy as np
from numpy.typing import NDArray

from leaf import LEAFSolver, ResponseType
from cdf import (
    leaf_cdf_flex, CDFResult, SubgradeDamageModel,
    NOFF, SIGMA_W, asphalt_n_to_fail,
    compute_tandem_strains,
)
from structures import PavementSection, PavementLayer
from materials import POISSON_BY_CODE

# ═══════════════════════════════════════════════════════════════════════════════
#  Constants
# ═══════════════════════════════════════════════════════════════════════════════

CDF_EXIT_ERR = 0.005        # |ln(CDF) - 0| < 0.005 → 0.995 < CDF < 1.005
CDF_ERR_CTRL = 0.69         # ln(2), switch point for sublayer refinement
MAX_ITERATIONS = 50
MIN_THICK_DEFAULT = 2.0     # inches, fallback minimum layer thickness

# Minimum thicknesses by material name
MIN_THICKNESS: dict[str, float] = {
    "P-401/P-403 HMA Surface": 2.0,
    "P-401/P-403 HMA Overlay": 2.0,
    "P-401/P-403 HMA Stabilized": 4.0,
    "P-209 Crushed Aggregate": 6.0,
    "P-208 Crushed Aggregate": 6.0,
    "P-154 Uncrushed Aggregate": 6.0,
    "P-211 Lime Rock": 6.0,
    "P-219 Recycled Concrete Aggregate": 6.0,
    "P-306 Lean Concrete": 4.0,
    "P-304 Cement Treated Base": 4.0,
    "P-301 Soil Cement Base": 4.0,
    "Variable (flexible)": 4.0,
    "Variable (rigid)": 4.0,
}


@dataclass
class DesignResult:
    """Output of a flexible pavement thickness design."""
    converged: bool
    iterations: int
    layer_thicknesses: list[float]      # final thicknesses (inches)
    cdf_subgrade: float                 # CDF at top of subgrade
    cdf_asphalt: Optional[float]        # CDF at bottom of asphalt (if computed)
    subgrade_strain: NDArray            # max strain per aircraft
    n_to_fail_subgrade: NDArray         # NtoFail per aircraft (subgrade)
    n_to_fail_asphalt: Optional[NDArray]
    design_layer_index: int             # 0-based layer that was iterated
    message: str = ""


# ═══════════════════════════════════════════════════════════════════════════════
#  Aggregate sublayer modulus refinement
# ═══════════════════════════════════════════════════════════════════════════════

def faa_sublayer_modulus(
    thickness: float, layer_code: int, modulus_below: float
) -> tuple[int, list[float], list[float]]:
    """Compute sublayer thicknesses and moduli for aggregate layers.

    Implements the WES modulus procedure from FAAModulusThick in
    modCDF.vb.  Each sublayer modulus is computed from the layer below
    using: E_i = E_{i+1} * (1 + C*log10(t) - D*log10(E_{i+1})*log10(t))

    Returns (n_sub, thicknesses[], moduli[]).
    """
    AGGREGATE_BASES = {6, 18, 19, 21}  # P-209, P-208, P-219, P-211
    AGGREGATE_SUBBASES = {8}            # P-154
    LOG10 = math.log(10.0)

    if layer_code in AGGREGATE_BASES:
        C, D = 10.52, 2.1
        thick_min = 10.0
    elif layer_code in AGGREGATE_SUBBASES:
        C, D = 7.18, 1.56
        thick_min = 8.0
    else:
        return 1, [thickness], [modulus_below]

    def _wes_modulus(sub_thick: float, e_below: float) -> float:
        """WES logarithmic modulus: E = E_below * (1 + C*lg(t) - D*lg(E_below)*lg(t))"""
        if sub_thick <= 0 or e_below <= 0:
            return e_below
        lg_t = math.log(sub_thick) / LOG10
        lg_e = math.log(e_below) / LOG10
        return e_below * (1.0 + C * lg_t - D * lg_e * lg_t)

    # Determine number of sublayers
    ns = 1
    if thickness > thick_min:
        ns = int(thickness / thick_min)
        if thickness / thick_min - ns != 0.0:
            ns += 1
    ts = thickness / ns

    if ns == 1:
        mod = _wes_modulus(thickness, modulus_below)
        return 1, [thickness], [mod]

    # Build sublayer moduli from bottom to top
    thicknesses = [ts] * ns
    moduli = [0.0] * ns
    m_below = modulus_below
    for i in range(ns - 1, -1, -1):
        moduli[i] = _wes_modulus(ts, m_below)
        m_below = moduli[i]

    return ns, thicknesses, moduli


_AGG_BASE_CODES = {6, 18, 19, 21}   # P-209, P-208, P-219, P-211
_AGG_SUB_CODES = {8}                 # P-154
_AGG_CODES = _AGG_BASE_CODES | _AGG_SUB_CODES


def _has_aggregate_layers(layers: list[PavementLayer]) -> bool:
    """True if the section has aggregate layers that can be sublayered."""
    return any(
        (l.layer_code or 0) in _AGG_CODES
        for l in layers[:-1]  # exclude subgrade
    )


def _build_sublayered_structure(
    section: PavementSection,
    eval_depth: float,
    eval_layer: int,
    frozen_ns: dict[int, int] | None = None,
) -> 'LEAFStructure':
    """Build a LEAF structure with aggregate layers expanded into sublayers.

    Each aggregate layer is split using faa_sublayer_modulus (WES formula).
    Non-aggregate layers pass through unchanged.  The eval_layer is
    adjusted to account for any inserted sublayers above the evaluation
    point.

    Parameters
    ----------
    section : the pavement section (logical layers)
    eval_depth : depth to evaluation point (inches)
    eval_layer : 1-based logical layer index of the evaluation layer
    frozen_ns : when provided, forces each aggregate layer (by 0-based
                index) to keep a fixed sublayer count, preventing
                oscillation near convergence
    """
    from leaf import LEAFStructure as _LS

    layers = section.layers
    exp_thick: list[float] = []
    exp_mod: list[float] = []
    exp_poi: list[float] = []
    exp_ip: list[float] = []
    adj_eval_layer = 0

    for i, lay in enumerate(layers):
        code = lay.layer_code if lay.layer_code is not None else 0
        is_subgrade = (i == len(layers) - 1)
        poi = lay.poisson if lay.poisson is not None else 0.35
        ip = lay.interface_bond

        if not is_subgrade and code in _AGG_CODES:
            mod_below = layers[i + 1].modulus if i + 1 < len(layers) else 15000.0
            ns_natural, _, _ = faa_sublayer_modulus(lay.thickness, code, mod_below)

            if frozen_ns is not None and i in frozen_ns:
                ns = frozen_ns[i]
            else:
                ns = ns_natural

            # Recompute sublayer moduli with the (possibly frozen) count
            ts = lay.thickness / max(ns, 1)
            sub_mods = [0.0] * ns
            m_below = mod_below
            LOG10 = math.log(10.0)
            agg_c = 10.52 if code in _AGG_BASE_CODES else 7.18
            agg_d = 2.1 if code in _AGG_BASE_CODES else 1.56
            for j in range(ns - 1, -1, -1):
                if ts > 0 and m_below > 0:
                    lg_t = math.log(ts) / LOG10
                    lg_e = math.log(m_below) / LOG10
                    sub_mods[j] = m_below * (1.0 + agg_c * lg_t - agg_d * lg_e * lg_t)
                else:
                    sub_mods[j] = m_below
                m_below = sub_mods[j]

            for j in range(ns):
                exp_thick.append(ts)
                exp_mod.append(sub_mods[j])
                exp_poi.append(poi)
                exp_ip.append(ip)
            if i + 1 <= eval_layer:
                adj_eval_layer += ns
        else:
            exp_thick.append(lay.thickness)
            exp_mod.append(lay.modulus)
            exp_poi.append(poi)
            exp_ip.append(ip)
            if i + 1 <= eval_layer:
                adj_eval_layer += 1

    nl = len(exp_thick)
    thick = np.zeros(nl + 1)
    modulus = np.zeros(nl + 1)
    poisson = np.zeros(nl + 1)
    interface_parm = np.zeros(nl + 1)
    for k in range(nl):
        thick[k + 1] = exp_thick[k]
        modulus[k + 1] = exp_mod[k]
        poisson[k + 1] = exp_poi[k]
        interface_parm[k + 1] = exp_ip[k]

    return _LS(
        n_layers=nl, thick=thick, modulus=modulus, poisson=poisson,
        interface_parm=interface_parm, eval_depth=eval_depth,
        eval_layer=adj_eval_layer,
    )


# ═══════════════════════════════════════════════════════════════════════════════
#  Stabilized base compensation factor
# ═══════════════════════════════════════════════════════════════════════════════

def comp_for_stab(layers: list[PavementLayer], design_layer_idx: int) -> float:
    """Compensation factor FSlope for stabilized base.

    Port of CompforStab in modCDF.vb.  In FAARFIELD this is used
    exclusively in the **rigid** design paths (LeafCDFRigid13,
    overlay-on-rigid, HMA-on-rigid) to modify the PCC fatigue slope.
    It is NOT called during flexible pavement design (LeafCDFFlex /
    modStrDesignFlex.vb).  Retained here for future rigid design
    implementation.
    """
    AGG_EQUIV_FACTOR = 0.5
    AGG_CODES = {6, 8, 18, 19, 21}

    equiv_thick = 0.0
    for i in range(design_layer_idx, len(layers) - 1):
        lay = layers[i]
        code = lay.layer_code
        if code is None:
            continue
        if code in AGG_CODES:
            equiv_thick += AGG_EQUIV_FACTOR * lay.thickness / 8.0
        else:
            mod = max(200_000.0, min(700_000.0, lay.modulus))
            slope = (1.0 - AGG_EQUIV_FACTOR) / (700_000.0 - 200_000.0)
            equiv_factor = AGG_EQUIV_FACTOR + slope * (mod - 200_000.0)
            equiv_thick += equiv_factor * lay.thickness / 8.0

    equiv_thick = max(equiv_thick, 0.4)
    return 0.25 * 10.0 ** (1.2 * (1.0 - equiv_thick))


# ═══════════════════════════════════════════════════════════════════════════════
#  Main design iteration
# ═══════════════════════════════════════════════════════════════════════════════

def design_flex(
    section: PavementSection,
    design_layer_index: int = -1,
    damage_model: SubgradeDamageModel = SubgradeDamageModel.STANDARD,
    compute_asphalt_cdf: bool = True,
    verbose: bool = False,
) -> DesignResult:
    """Iterate on thickness of the design layer until CDF_subgrade ≈ 1.0.

    Parameters
    ----------
    section : the pavement section (layers + traffic mix)
    design_layer_index : 0-based index of the layer to iterate.
                         Default -1 selects the last layer before subgrade.
    damage_model : subgrade failure criterion
    compute_asphalt_cdf : if True, compute asphalt CDF after convergence
    verbose : print iteration progress

    Returns
    -------
    DesignResult with final thicknesses, CDF values, and convergence info.
    """
    layers = section.layers
    nlay = len(layers)
    if nlay < 2:
        raise ValueError("Need at least 2 layers (surface + subgrade)")

    # Design layer: last layer before subgrade
    if design_layer_index < 0:
        design_layer_index = nlay - 2
    dl = design_layer_index

    min_thick = MIN_THICKNESS.get(layers[dl].material_name, MIN_THICK_DEFAULT)
    subgrade_modulus = layers[-1].modulus
    asphalt_modulus = layers[0].modulus

    # LEAF solver instance (reuse quadrature weights)
    solver = LEAFSolver()

    # Prepare aircraft data (convert once)
    leaf_aircraft = section.to_leaf_aircraft()

    # Precompute wheel X and tire widths for CDF
    wheel_x_per_ac: list[list[float]] = []
    tire_width_per_ac: list[float] = []
    reps_list: list[float] = []
    n_eval_list: list[int] = []
    for ac in section.traffic:
        wheel_x_per_ac.append(list(ac.wheel_x))
        tw = math.sqrt(ac.gear_load / ac.n_wheels / (ac.tire_pressure * 3.14159265359))
        tire_width_per_ac.append(tw * 2.0)  # diameter as width
        reps_list.append(ac.total_departures(section.design_life))
        n_eval_list.append(len(ac.eval_x))

    # ── Iteration loop ────────────────────────────────────────────────────
    cdf_err = 10.0
    cdf_m1 = 0.0
    t_m1 = 0.0
    iloop = 0
    cdf_max = 0.0
    cdf_result: Optional[CDFResult] = None
    overflow = True

    has_agg = _has_aggregate_layers(layers)
    layer_switch = False
    frozen_ns: dict[int, int] | None = None

    while iloop < MAX_ITERATIONS:
        iloop += 1

        # Build current LEAF structure (with or without sublayer expansion)
        eval_depth = sum(l.thickness for l in layers[:-1])
        if layer_switch and has_agg:
            leaf_struct = _build_sublayered_structure(
                section, eval_depth, nlay, frozen_ns)
        else:
            leaf_struct = section.to_leaf_structure(eval_depth, nlay)

        # Tandem CDF: two-pass LEAF computation for longitudinal strain scan
        # (matching FAARFIELD's gTandemFnew = True default for subgrade)
        tandem_resp = compute_tandem_strains(
            solver, leaf_aircraft, leaf_struct)
        # Standard strain for overflow check and asphalt CDF
        strain_resp = solver.compute_response(
            ResponseType.VERTICAL_STRAIN, leaf_aircraft, leaf_struct)

        # Compute CDF with tandem scanning
        cdf_result = leaf_cdf_flex(
            strain_resp, reps_list, n_eval_list,
            wheel_x_per_ac, tire_width_per_ac,
            eval_depth, subgrade_modulus, asphalt_modulus,
            is_subgrade=True, damage_model=damage_model,
            use_tandem=True, tandem_strain_response=tandem_resp,
        )
        cdf_max = cdf_result.cdf_max

        if verbose:
            print(f"  Iter {iloop:3d}: t[{dl}]={layers[dl].thickness:8.3f} in"
                  f"  CDF={cdf_max:.6f}  |ln(CDF)|={abs(math.log(max(cdf_max,1e-15))):.5f}")

        # Handle overflow (strains too small)
        if all(s < 1e-8 for s in cdf_result.strain_max):
            overflow = True
            layers[dl].thickness = max(layers[dl].thickness * 0.5, min_thick)
            if layers[dl].thickness <= min_thick:
                break
            continue
        else:
            overflow = False

        cdf_err = abs(math.log(max(cdf_max, 1e-15)))

        # Activate sublayer expansion when CDF is in the vicinity of 1.0
        if cdf_err < CDF_ERR_CTRL and has_agg and not layer_switch:
            layer_switch = True
            cdf_err = 10.0  # force re-evaluation with sublayers
            t_m1 = 0.0      # reset Newton-Raphson state
            cdf_m1 = 0.0
            if verbose:
                print("    -> Switching to aggregate sublayer mode")
            continue

        # Freeze sublayer counts when approaching convergence to prevent
        # oscillation from changing NS across the CDF=1.0 boundary
        if layer_switch and has_agg and frozen_ns is None and cdf_err < CDF_ERR_CTRL * 0.7:
            frozen_ns = {}
            for i_lay, lay in enumerate(layers[:-1]):
                code = lay.layer_code if lay.layer_code is not None else 0
                if code in _AGG_CODES:
                    mod_below = layers[i_lay + 1].modulus if i_lay + 1 < len(layers) else 15000.0
                    ns, _, _ = faa_sublayer_modulus(lay.thickness, code, mod_below)
                    frozen_ns[i_lay] = ns

        # Check convergence
        if cdf_err < CDF_EXIT_ERR:
            break

        # Check minimum thickness
        if layers[dl].thickness <= min_thick and cdf_max < 1.0:
            layers[dl].thickness = min_thick
            break

        # Newton-Raphson on ln(CDF) vs thickness
        if t_m1 == 0.0 and cdf_m1 == 0.0:
            t_m1 = layers[dl].thickness
            cdf_m1 = math.log(max(cdf_max, 1e-15))
            delt = layers[dl].thickness * 0.01
            layers[dl].thickness += delt
            continue

        # Second evaluation for gradient
        log_cdf = math.log(max(cdf_max, 1e-15))
        del_cdf = log_cdf - cdf_m1
        delt = layers[dl].thickness - t_m1

        if abs(del_cdf) < 1e-12:
            # No gradient — perturb and retry
            t_m1 = layers[dl].thickness
            cdf_m1 = log_cdf
            layers[dl].thickness *= 1.1
            continue

        if del_cdf > 0.0 and cdf_max > 1e-6:
            # CDF increased with thickness — gradient is wrong.
            # Retry with a larger perturbation to get a better estimate.
            t_m1 = layers[dl].thickness
            cdf_m1 = log_cdf
            if cdf_max > 1.0:
                layers[dl].thickness *= 2.0
            else:
                layers[dl].thickness *= 1.1
            continue

        # Overshoot control (from FAARFIELD)
        if -CDF_ERR_CTRL < cdf_m1 < CDF_ERR_CTRL:
            factor = 1.0
        elif -(CDF_ERR_CTRL + 1) < cdf_m1 < (CDF_ERR_CTRL + 1):
            factor = 0.95
        else:
            factor = 0.6

        # Newton step
        new_delt = (-cdf_m1 * delt / del_cdf) * factor
        new_delt = max(-50.0, min(50.0, new_delt))

        new_t = t_m1 + new_delt
        if new_t < min_thick or cdf_max < 1e-6:
            new_t = min_thick

        t_m1 = layers[dl].thickness
        cdf_m1 = log_cdf
        layers[dl].thickness = new_t

    # ── Post-convergence: asphalt CDF ─────────────────────────────────────
    cdf_asphalt = None
    ntf_asphalt = None
    if compute_asphalt_cdf and cdf_result is not None:
        # Evaluate horizontal stress at bottom of asphalt (layer 1)
        eval_depth_asp = layers[0].thickness
        leaf_struct_asp = section.to_leaf_structure(eval_depth_asp, 1)
        h_resp = solver.compute_response(
            ResponseType.HORIZONTAL_STRESS, leaf_aircraft, leaf_struct_asp)

        cdf_asp_result = leaf_cdf_flex(
            h_resp, reps_list, n_eval_list,
            wheel_x_per_ac, tire_width_per_ac,
            eval_depth_asp, subgrade_modulus, asphalt_modulus,
            is_subgrade=False, damage_model=damage_model,
        )
        cdf_asphalt = cdf_asp_result.cdf_max
        ntf_asphalt = cdf_asp_result.n_to_fail

    converged = cdf_err < CDF_EXIT_ERR
    msg = "Design converged" if converged else (
        "Minimum thickness reached" if layers[dl].thickness <= min_thick
        else "Max iterations reached"
    )

    return DesignResult(
        converged=converged, iterations=iloop,
        layer_thicknesses=[l.thickness for l in layers],
        cdf_subgrade=cdf_max,
        cdf_asphalt=cdf_asphalt,
        subgrade_strain=cdf_result.strain_max if cdf_result else np.array([]),
        n_to_fail_subgrade=cdf_result.n_to_fail if cdf_result else np.array([]),
        n_to_fail_asphalt=ntf_asphalt,
        design_layer_index=dl,
        message=msg,
    )


def design_flex_overlay(
    section: PavementSection,
    damage_model: SubgradeDamageModel = SubgradeDamageModel.STANDARD,
    verbose: bool = False,
) -> DesignResult:
    """Iterate on HMA overlay thickness until CDF_subgrade ≈ 1.0.

    Port of LeafDesignFlexOFlex from modStrDesignFlex.vb.  The overlay
    layer (index 0) is iterated while all underlying layers are held
    fixed at their existing thicknesses.

    Parameters
    ----------
    section : pavement section — layer 0 must be HMA Overlay, layer 1
              must be HMA Surface (existing pavement).
    damage_model : subgrade failure criterion
    verbose : print iteration progress

    Returns
    -------
    DesignResult with final overlay thickness and CDF values.
    """
    layers = section.layers
    nlay = len(layers)
    if nlay < 3:
        raise ValueError("Overlay design needs >= 3 layers (overlay + surface + subgrade)")

    dl = 0  # design layer is the overlay
    min_thick = MIN_THICKNESS.get(layers[dl].material_name, 2.0)
    subgrade_modulus = layers[-1].modulus
    asphalt_modulus = layers[0].modulus

    solver = LEAFSolver()
    leaf_aircraft = section.to_leaf_aircraft()

    wheel_x_per_ac: list[list[float]] = []
    tire_width_per_ac: list[float] = []
    reps_list: list[float] = []
    n_eval_list: list[int] = []
    for ac in section.traffic:
        wheel_x_per_ac.append(list(ac.wheel_x))
        tw = math.sqrt(ac.gear_load / ac.n_wheels / (ac.tire_pressure * 3.14159265359))
        tire_width_per_ac.append(tw * 2.0)
        reps_list.append(ac.total_departures(section.design_life))
        n_eval_list.append(len(ac.eval_x))

    cdf_err = 10.0
    cdf_m1 = 0.0
    t_m1 = 0.0
    iloop = 0
    cdf_max = 0.0
    cdf_result: Optional[CDFResult] = None

    while iloop < MAX_ITERATIONS:
        iloop += 1

        eval_depth = sum(l.thickness for l in layers[:-1])
        leaf_struct = section.to_leaf_structure(eval_depth, nlay)

        tandem_resp = compute_tandem_strains(
            solver, leaf_aircraft, leaf_struct)
        strain_resp = solver.compute_response(
            ResponseType.VERTICAL_STRAIN, leaf_aircraft, leaf_struct)

        cdf_result = leaf_cdf_flex(
            strain_resp, reps_list, n_eval_list,
            wheel_x_per_ac, tire_width_per_ac,
            eval_depth, subgrade_modulus, asphalt_modulus,
            is_subgrade=True, damage_model=damage_model,
            use_tandem=True, tandem_strain_response=tandem_resp,
        )
        cdf_max = cdf_result.cdf_max

        if verbose:
            print(f"  Iter {iloop:3d}: overlay={layers[dl].thickness:8.3f} in"
                  f"  CDF={cdf_max:.6f}")

        # When CDF is very low and thickness is above minimum, reduce overlay
        if cdf_max < 0.01 and layers[dl].thickness > min_thick:
            layers[dl].thickness *= 0.9
            if layers[dl].thickness < min_thick:
                layers[dl].thickness = min_thick
            continue

        cdf_err = abs(math.log(max(cdf_max, 1e-15)))

        if cdf_err < CDF_EXIT_ERR:
            break

        # Minimum thickness reached
        if layers[dl].thickness <= min_thick and cdf_max < 1.0:
            layers[dl].thickness = min_thick
            break

        # Newton-Raphson on ln(CDF) vs overlay thickness
        if iloop == 1 or (t_m1 == 0.0 and cdf_m1 == 0.0):
            t_m1 = layers[dl].thickness
            cdf_m1 = math.log(max(cdf_max, 1e-15))
            layers[dl].thickness += layers[dl].thickness * 0.01
            continue

        log_cdf = math.log(max(cdf_max, 1e-15))
        del_cdf = log_cdf - cdf_m1
        delt = layers[dl].thickness - t_m1

        if abs(del_cdf) < 1e-12:
            # No gradient — perturb
            t_m1 = layers[dl].thickness
            cdf_m1 = log_cdf
            layers[dl].thickness *= 1.1
            continue

        factor = 0.6 if abs(cdf_m1) > CDF_ERR_CTRL + 1 else (
            0.95 if abs(cdf_m1) > CDF_ERR_CTRL else 1.0)

        new_delt = (-cdf_m1 * delt / del_cdf) * factor
        new_delt = max(-50.0, min(50.0, new_delt))

        new_t = t_m1 + new_delt
        if new_t < min_thick:
            new_t = min_thick

        t_m1 = layers[dl].thickness
        cdf_m1 = log_cdf
        layers[dl].thickness = new_t

    # Asphalt CDF at bottom of overlay and bottom of existing surface
    cdf_asphalt = None
    ntf_asphalt = None
    if cdf_result is not None:
        # Evaluate at bottom of overlay (layer 0 → layer 1 interface)
        asp_depth = layers[0].thickness
        leaf_struct_asp = section.to_leaf_structure(asp_depth, 1)
        h_resp = solver.compute_response(
            ResponseType.HORIZONTAL_STRESS, leaf_aircraft, leaf_struct_asp)
        cdf_asp = leaf_cdf_flex(
            h_resp, reps_list, n_eval_list,
            wheel_x_per_ac, tire_width_per_ac,
            asp_depth, subgrade_modulus, asphalt_modulus,
            is_subgrade=False, damage_model=damage_model,
        )
        cdf_asphalt = cdf_asp.cdf_max
        ntf_asphalt = cdf_asp.n_to_fail

    converged = cdf_err < CDF_EXIT_ERR
    msg = "Overlay design converged" if converged else (
        "Minimum overlay thickness reached" if layers[dl].thickness <= min_thick
        else "Max iterations reached"
    )

    return DesignResult(
        converged=converged, iterations=iloop,
        layer_thicknesses=[l.thickness for l in layers],
        cdf_subgrade=cdf_max,
        cdf_asphalt=cdf_asphalt,
        subgrade_strain=cdf_result.strain_max if cdf_result else np.array([]),
        n_to_fail_subgrade=cdf_result.n_to_fail if cdf_result else np.array([]),
        n_to_fail_asphalt=ntf_asphalt,
        design_layer_index=dl,
        message=msg,
    )


def compute_life(
    section: PavementSection,
    damage_model: SubgradeDamageModel = SubgradeDamageModel.STANDARD,
) -> float:
    """Compute pavement structural life in years (CDF=1.0).

    Uses Newton-Raphson iteration on the design life until CDF ≈ 1.0.
    """
    solver = LEAFSolver()
    leaf_aircraft = section.to_leaf_aircraft()

    nlay = len(section.layers)
    eval_depth = sum(l.thickness for l in section.layers[:-1])
    leaf_struct = section.to_leaf_structure(eval_depth, nlay)

    tandem_resp = compute_tandem_strains(solver, leaf_aircraft, leaf_struct)
    strain_resp = solver.compute_response(
        ResponseType.VERTICAL_STRAIN, leaf_aircraft, leaf_struct)

    subgrade_modulus = section.layers[-1].modulus
    asphalt_modulus = section.layers[0].modulus

    wheel_x_per_ac = [list(ac.wheel_x) for ac in section.traffic]
    tire_width_per_ac = []
    n_eval_list = [len(ac.eval_x) for ac in section.traffic]
    for ac in section.traffic:
        tw = math.sqrt(ac.gear_load / ac.n_wheels / (ac.tire_pressure * 3.14159265359))
        tire_width_per_ac.append(tw * 2.0)

    # Secant method on life
    life_m1 = section.design_life * 1.0
    life_str = section.design_life * 1.1

    def _cdf_at_life(years: float) -> float:
        reps = [ac.total_departures(int(max(1, years))) for ac in section.traffic]
        r = leaf_cdf_flex(
            strain_resp, reps, n_eval_list,
            wheel_x_per_ac, tire_width_per_ac,
            eval_depth, subgrade_modulus, asphalt_modulus,
            is_subgrade=True, damage_model=damage_model,
            use_tandem=True, tandem_strain_response=tandem_resp,
        )
        return r.cdf_max

    cdf_m1 = _cdf_at_life(life_m1)

    for _ in range(30):
        cdf_str = _cdf_at_life(life_str)
        if abs(cdf_str - 1.0) < 0.001:
            return life_str
        if abs(cdf_str - cdf_m1) < 1e-12:
            break
        delt = (1.0 - cdf_m1) * (life_str - life_m1) / (cdf_str - cdf_m1)
        life_m1, cdf_m1 = life_str, cdf_str
        life_str = life_m1 + delt

    return life_str
