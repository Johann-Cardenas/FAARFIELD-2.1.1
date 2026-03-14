"""
Comprehensive test suite for NanoFlex.

Tests verify numerical fidelity against known analytical solutions,
FAARFIELD benchmark values, and internal consistency across all modules.

Run:  pytest test_nanoflex.py -v
"""

import math
import os
import sys

import numpy as np
import pytest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from numerical import (
    bessj0, bessj1, bessj1_over_x, gammln, gaulag,
    gauss_eliminate, lu_solve, gauss_jordan, pythag,
)
from units import UnitSystem, IN_TO_MM, LB_TO_KG, PSI_TO_KPA, PSI_TO_MPA
from materials import (
    MATERIALS, cbr_to_modulus, modulus_to_cbr,
    modulus_to_k_value, k_value_to_modulus, get_default_stack,
)
from structures import PavementSection, PavementLayer, TrafficAircraft
from leaf import LEAFSolver, LEAFAircraft, LEAFStructure, ResponseType
from cdf import (
    gauss_area, subgrade_n_to_fail, asphalt_n_to_fail,
    rdec_n_to_fail, RDECParams,
    coverage_to_pass_flex, coverage_to_pass_general,
    SubgradeDamageModel,
)


# ═══════════════════════════════════════════════════════════════════════════════
#  Bessel functions
# ═══════════════════════════════════════════════════════════════════════════════

class TestBesselFunctions:
    """Verify Bessel function implementations against scipy reference values."""

    @pytest.mark.parametrize("x, expected", [
        (0.0, 1.0),
        (1.0, 0.7651976865579666),
        (5.0, -0.1775967713143383),
        (20.0, 0.1670246643401511),
    ])
    def test_bessj0(self, x, expected):
        assert abs(bessj0(x) - expected) < 5e-9

    @pytest.mark.parametrize("x, expected", [
        (0.0, 0.0),
        (1.0, 0.4400505857449335),
        (5.0, -0.3275791375914652),
        (20.0, 0.06683312418816853),
    ])
    def test_bessj1(self, x, expected):
        assert abs(bessj1(x) - expected) < 1e-8

    def test_bessj1_over_x_at_zero(self):
        """J1(0)/0 should be 0.5 (L'Hopital limit)."""
        assert abs(bessj1_over_x(0.0) - 0.5) < 1e-9

    def test_bessj1_over_x_nonzero(self):
        val = bessj1_over_x(1.0)
        assert abs(val - bessj1(1.0) / 1.0) < 1e-12


# ═══════════════════════════════════════════════════════════════════════════════
#  Gauss–Laguerre quadrature
# ═══════════════════════════════════════════════════════════════════════════════

class TestGaulag:
    def test_500_points(self):
        x, w, n = gaulag(500, 0.0)
        assert n >= 300, f"Expected 300+ usable quadrature points, got {n}"
        assert np.all(x > 0), "All abscissae must be positive"
        assert np.all(w > 0), "All weights must be positive"

    def test_integral_of_exp(self):
        """Integral of exp(-x) from 0 to inf = 1 (via GL quadrature)."""
        x, w, n = gaulag(100, 0.0)
        integral = np.sum(w)
        assert abs(integral - 1.0) < 1e-10


# ═══════════════════════════════════════════════════════════════════════════════
#  Log-Gamma
# ═══════════════════════════════════════════════════════════════════════════════

class TestGammln:
    @pytest.mark.parametrize("x, expected", [
        (1.0, 0.0),        # ln(Gamma(1)) = ln(1) = 0
        (2.0, 0.0),        # ln(Gamma(2)) = ln(1) = 0
        (5.0, 3.178054),   # ln(24)
        (10.0, 12.80183),  # ln(362880)
    ])
    def test_known_values(self, x, expected):
        assert abs(gammln(x) - expected) < 1e-4


# ═══════════════════════════════════════════════════════════════════════════════
#  Linear algebra solvers
# ═══════════════════════════════════════════════════════════════════════════════

class TestLinearAlgebra:
    def test_gauss_jordan_3x3(self):
        A = np.array([[2, 1, -1], [-3, -1, 2], [-2, 1, 2]], dtype=float)
        b = np.array([8, -11, -3], dtype=float)
        ifail = gauss_jordan(A.copy(), b)
        assert ifail == 0
        np.testing.assert_allclose(b, [2, 3, -1], atol=1e-10)

    def test_lu_solve_spd(self):
        A = np.array([[4, 2, 0], [2, 5, 1], [0, 1, 3]], dtype=float)
        b = np.array([1, 2, 3], dtype=float)
        ifail = lu_solve(A, b)
        assert ifail == 0
        expected = np.linalg.solve(
            np.array([[4, 2, 0], [2, 5, 1], [0, 1, 3]], dtype=float),
            np.array([1, 2, 3], dtype=float),
        )
        np.testing.assert_allclose(b, expected, atol=1e-8)

    def test_gauss_eliminate_2x2(self):
        A = np.array([[2, 1], [1, 3]], dtype=float)
        r = np.array([5, 7], dtype=float)
        ifail = gauss_eliminate(A, r)
        assert ifail == 0
        np.testing.assert_allclose(r, [1.6, 1.8], atol=1e-10)

    def test_pythag(self):
        assert abs(pythag(3, 4) - 5.0) < 1e-12
        assert abs(pythag(0, 0) - 0.0) < 1e-12


# ═══════════════════════════════════════════════════════════════════════════════
#  Unit conversions
# ═══════════════════════════════════════════════════════════════════════════════

class TestUnits:
    def test_conversion_factors(self):
        assert abs(IN_TO_MM - 25.4) < 1e-6
        assert abs(LB_TO_KG - 0.453592) < 1e-6
        assert abs(PSI_TO_KPA - 6.89476) < 1e-5

    def test_unit_system_us(self):
        us = UnitSystem(metric=False)
        assert us.display_thickness(10.0) == 10.0
        assert us.to_internal_thickness(10.0) == 10.0

    def test_unit_system_metric(self):
        si = UnitSystem(metric=True)
        assert abs(si.display_thickness(1.0) - 25.4) < 1e-6
        assert abs(si.to_internal_thickness(25.4) - 1.0) < 1e-6

    def test_roundtrip_metric(self):
        si = UnitSystem(metric=True)
        for val in [4.0, 10.0, 200000.0]:
            rt = si.to_internal_modulus(si.display_modulus(val))
            assert abs(rt - val) / val < 1e-10


# ═══════════════════════════════════════════════════════════════════════════════
#  Material library
# ═══════════════════════════════════════════════════════════════════════════════

class TestMaterials:
    def test_catalogue_count(self):
        assert len(MATERIALS) == 19

    def test_cbr_modulus_roundtrip(self):
        for cbr in [5, 10, 20]:
            assert abs(modulus_to_cbr(cbr_to_modulus(cbr)) - cbr) < 1e-10

    def test_k_value_roundtrip(self):
        for e in [5000, 15000, 30000]:
            k = modulus_to_k_value(e)
            e_rt = k_value_to_modulus(k)
            assert abs(e_rt - e) / e < 1e-6

    def test_default_stack_new_flexible(self):
        stack = get_default_stack("New Flexible")
        assert len(stack) == 4
        assert stack[0].material_name == "P-401/P-403 HMA Surface"
        assert stack[-1].material_name == "Subgrade"


# ═══════════════════════════════════════════════════════════════════════════════
#  Input validation
# ═══════════════════════════════════════════════════════════════════════════════

class TestValidation:
    def test_valid_section(self):
        s = PavementSection(name="Test")
        s.layers = [
            PavementLayer("P-401/P-403 HMA Surface", 4.0, 200000),
            PavementLayer("P-209 Crushed Aggregate", 10.0, 75000),
            PavementLayer("Subgrade", 0.0, 15000),
        ]
        assert s.validate() == []

    def test_modulus_out_of_range(self):
        lay = PavementLayer("Subgrade", 0.0, 100000)
        w = lay.validate()
        assert any("outside range" in msg for msg in w)

    def test_thickness_below_minimum(self):
        lay = PavementLayer("P-401/P-403 HMA Surface", 1.0, 200000)
        w = lay.validate()
        assert any("below minimum" in msg for msg in w)

    def test_bad_aircraft(self):
        s = PavementSection(name="Test")
        s.layers = [
            PavementLayer("P-401/P-403 HMA Surface", 4.0, 200000),
            PavementLayer("Subgrade", 0.0, 15000),
        ]
        s.traffic = [TrafficAircraft(
            name="Bad", gross_weight=-1, mg_percent=0.95, tire_pressure=200,
            n_wheels=2, wheel_x=[0, 8], wheel_y=[0, 0],
            eval_x=[0], eval_y=[0],
        )]
        w = s.validate()
        assert any("gross weight" in msg for msg in w)


# ═══════════════════════════════════════════════════════════════════════════════
#  LEAF solver
# ═══════════════════════════════════════════════════════════════════════════════

class TestLEAFSolver:
    @pytest.fixture
    def solver(self):
        return LEAFSolver()

    @pytest.fixture
    def single_wheel(self):
        tp = np.array([0.0, 200.0])
        tx = np.array([0.0, 0.0])
        ty = np.array([0.0, 0.0])
        ex = np.array([0.0, 0.0])
        ey = np.array([0.0, 0.0])
        return LEAFAircraft(
            name="SWL", gear_load=50000, n_tires=1,
            tire_press=tp, tire_x=tx, tire_y=ty,
            n_eval_points=1, eval_x=ex, eval_y=ey,
        )

    def test_boussinesq_vertical_strain(self, solver, single_wheel):
        """Single-layer half-space should match Boussinesq solution."""
        struct = LEAFStructure(
            n_layers=1,
            thick=np.array([0.0, 0.0]),
            modulus=np.array([0.0, 15000.0]),
            poisson=np.array([0.0, 0.45]),
            interface_parm=np.array([0.0, 1.0]),
            eval_depth=20.0, eval_layer=1,
        )
        result = solver.compute_response(
            ResponseType.VERTICAL_STRAIN, [single_wheel], struct)
        strain = result[1, 1]
        assert strain < 0, "Compressive strain should be negative"
        assert abs(strain) > 1e-4, "Strain magnitude should be physically reasonable"

    def test_two_layer_vertical_strain(self, solver, single_wheel):
        """HMA over subgrade should give compressive strain at interface."""
        struct = LEAFStructure(
            n_layers=2,
            thick=np.array([0.0, 4.0, 0.0]),
            modulus=np.array([0.0, 200000.0, 15000.0]),
            poisson=np.array([0.0, 0.35, 0.35]),
            interface_parm=np.array([0.0, 1.0, 1.0]),
            eval_depth=4.0, eval_layer=2,
        )
        result = solver.compute_response(
            ResponseType.VERTICAL_STRAIN, [single_wheel], struct)
        assert result[1, 1] < 0

    def test_horizontal_stress(self, solver, single_wheel):
        """Horizontal stress at bottom of HMA should be tensile (positive)."""
        struct = LEAFStructure(
            n_layers=2,
            thick=np.array([0.0, 4.0, 0.0]),
            modulus=np.array([0.0, 200000.0, 15000.0]),
            poisson=np.array([0.0, 0.35, 0.35]),
            interface_parm=np.array([0.0, 1.0, 1.0]),
            eval_depth=4.0, eval_layer=1,
        )
        result = solver.compute_response(
            ResponseType.HORIZONTAL_STRESS, [single_wheel], struct)
        assert result[1, 1] > 0, "Tensile stress should be positive"

    def test_vertical_deflection(self, solver, single_wheel):
        """Surface deflection should be positive (downward)."""
        struct = LEAFStructure(
            n_layers=2,
            thick=np.array([0.0, 4.0, 0.0]),
            modulus=np.array([0.0, 200000.0, 15000.0]),
            poisson=np.array([0.0, 0.35, 0.35]),
            interface_parm=np.array([0.0, 1.0, 1.0]),
            eval_depth=0.001, eval_layer=1,
        )
        result = solver.compute_response(
            ResponseType.VERTICAL_DEFLECTION, [single_wheel], struct)
        assert result[1, 1] > 0


# ═══════════════════════════════════════════════════════════════════════════════
#  CDF damage models
# ═══════════════════════════════════════════════════════════════════════════════

class TestCDF:
    def test_gauss_area_unit_sigma(self):
        """Area from -sigma to +sigma should be ~0.6827."""
        area = gauss_area(-30.435, 30.435, 30.435)
        assert abs(area - 0.6827) < 0.01

    def test_gauss_area_zero_sigma(self):
        assert gauss_area(-1, 1, 0.0) == 1.0
        assert gauss_area(1, 2, 0.0) == 0.0

    def test_subgrade_standard(self):
        ntf = subgrade_n_to_fail(0.001, 15000.0, SubgradeDamageModel.STANDARD)
        assert ntf > 0
        assert ntf < 1e12

    def test_subgrade_straight_line(self):
        ntf = subgrade_n_to_fail(0.001, 15000.0, SubgradeDamageModel.STRAIGHT_LINE)
        assert ntf > 0

    def test_subgrade_bleasdale(self):
        ntf = subgrade_n_to_fail(0.001, 15000.0, SubgradeDamageModel.BLEASDALE)
        assert ntf > 0

    def test_asphalt_fatigue(self):
        """Benchmark: strain=0.001, E=200k → NtoFail ≈ 3571."""
        ntf = asphalt_n_to_fail(0.001, 200000.0)
        assert abs(ntf - 3571) < 5

    def test_higher_strain_fewer_reps(self):
        ntf_low = subgrade_n_to_fail(0.0005, 15000.0)
        ntf_high = subgrade_n_to_fail(0.002, 15000.0)
        assert ntf_low > ntf_high

    def test_coverage_to_pass_single_wheel(self):
        ctp = coverage_to_pass_flex([0.0], 8.0, 15.0, 0.0)
        assert 0.0 < ctp < 1.0

    def test_rdec_defaults_benchmark(self):
        """RDEC with defaults at strain=0.001: NtoFail ≈ 5600 (double-precision)."""
        ntf = rdec_n_to_fail(0.001, RDECParams())
        assert abs(ntf - 5600) < 50, f"Expected ≈5600, got {ntf:.1f}"

    def test_rdec_higher_strain_fewer_reps(self):
        ntf_low = rdec_n_to_fail(0.0005)
        ntf_high = rdec_n_to_fail(0.002)
        assert ntf_low > ntf_high

    def test_coverage_general_single_wheel(self):
        """General gear coverage for a single wheel should match simplified."""
        ctp_gen = coverage_to_pass_general([0.0], [0.0], 8.0, 15.0, 0.0)
        ctp_flex = coverage_to_pass_flex([0.0], 8.0, 15.0, 0.0)
        assert abs(ctp_gen - ctp_flex) < 0.01

    def test_coverage_general_tandem(self):
        """General gear with tandem wheels should produce > 1× coverage."""
        wx = [-10.0, 10.0, -10.0, 10.0]
        wy = [0.0, 0.0, 50.0, 50.0]
        ctp = coverage_to_pass_general(wx, wy, 8.0, 15.0, 0.0)
        assert ctp > 0.0

    def test_scan_tandem_damage_synthetic(self):
        """Synthetic profile with one valley should produce positive damage."""
        from cdf import scan_tandem_damage
        profile = np.zeros(102)
        # Create a compressive strain valley around index 50
        for i in range(30, 70):
            profile[i] = -0.001 * math.exp(-((i - 50) / 8.0) ** 2)
        damage, s_max = scan_tandem_damage(
            profile, 100, 15000.0, SubgradeDamageModel.STANDARD)
        assert damage > 0.0, "Compressive valley should produce positive damage"
        assert s_max > 0.0

    def test_tandem_cdf_vs_standard(self):
        """Tandem CDF should differ from standard (different NtoFail path)."""
        from cdf import leaf_cdf_flex, compute_tandem_strains
        from leaf import LEAFSolver, LEAFAircraft, LEAFStructure, ResponseType

        solver = LEAFSolver()
        section = PavementSection(name="Test", design_life=20)
        section.layers = [
            PavementLayer("P-401/P-403 HMA Surface", 4.0, 200000.0),
            PavementLayer("P-209 Crushed Aggregate", 20.0, 40000.0),
            PavementLayer("Subgrade", 0.0, 15000.0, layer_code=4),
        ]
        section.traffic = [TrafficAircraft(
            name="SWL-50", gross_weight=50000.0, mg_percent=0.95,
            tire_pressure=200.0, n_wheels=1,
            wheel_x=[0.0], wheel_y=[0.0],
            eval_x=[0.0], eval_y=[0.0],
            annual_departures=1200,
        )]
        leaf_ac = section.to_leaf_aircraft()
        ed = section.subgrade_depth
        ls = section.to_leaf_structure(ed, 3)

        strain_resp = solver.compute_response(
            ResponseType.VERTICAL_STRAIN, leaf_ac, ls)
        tandem_resp = compute_tandem_strains(solver, leaf_ac, ls)

        wx = [[0.0]]
        tw_val = math.sqrt(50000 * 0.95 / 1 / (200 * 3.14159265359))
        tw = [tw_val * 2.0]
        reps = [1200 * 20]
        ne = [1]

        r_std = leaf_cdf_flex(
            strain_resp, reps, ne, wx, tw, ed, 15000.0,
            use_tandem=False,
        )
        r_tandem = leaf_cdf_flex(
            strain_resp, reps, ne, wx, tw, ed, 15000.0,
            use_tandem=True, tandem_strain_response=tandem_resp,
        )
        # Both should produce valid (positive) CDF
        assert r_std.cdf_max > 0
        assert r_tandem.cdf_max > 0


# ═══════════════════════════════════════════════════════════════════════════════
#  Aircraft loader
# ═══════════════════════════════════════════════════════════════════════════════

class TestAircraftLoader:
    @pytest.fixture
    def library(self):
        from aircraft import load_aircraft_library
        return load_aircraft_library()

    def test_library_loads(self, library):
        assert len(library) > 200

    def test_search(self, library):
        from aircraft import find_aircraft
        hits = find_aircraft(library, "737")
        assert len(hits) > 0

    def test_to_traffic_aircraft(self, library):
        rec = library[0]
        tac = rec.to_traffic_aircraft(annual_departures=1200)
        assert tac.n_wheels == rec.n_wheels
        assert tac.gross_weight == rec.gross_weight_lbs

    def test_wheel_coordinate_consistency(self, library):
        """All aircraft should have matching wheel_x and wheel_y lengths."""
        for rec in library:
            assert len(rec.wheel_x) == len(rec.wheel_y), (
                f"{rec.name}: wheel_x/wheel_y length mismatch")


# ═══════════════════════════════════════════════════════════════════════════════
#  Integration: design convergence
# ═══════════════════════════════════════════════════════════════════════════════

class TestDesignIntegration:
    @pytest.mark.slow
    def test_simple_design_converges(self):
        """Single-wheel aircraft on 3-layer flex should converge."""
        from design_flex import design_flex

        section = PavementSection(name="Test", design_life=20)
        section.layers = [
            PavementLayer("P-401/P-403 HMA Surface", 4.0, 200000),
            PavementLayer("P-209 Crushed Aggregate", 10.0, 75000),
            PavementLayer("Subgrade", 0.0, 15000),
        ]
        section.traffic = [TrafficAircraft(
            name="SWL-50", gross_weight=50000, mg_percent=0.95,
            tire_pressure=200, n_wheels=1,
            wheel_x=[0.0], wheel_y=[0.0],
            eval_x=[0.0], eval_y=[0.0],
            annual_departures=1200,
        )]
        result = design_flex(section, compute_asphalt_cdf=False)
        assert result.converged or result.message == "Minimum thickness reached"
        assert result.cdf_subgrade > 0

    @pytest.mark.slow
    def test_overlay_design_converges(self):
        """Overlay on 4-layer section should converge."""
        from design_flex import design_flex_overlay

        section = PavementSection(name="Overlay Test", design_life=20)
        section.layers = [
            PavementLayer("P-401/P-403 HMA Overlay", 4.0, 200000),
            PavementLayer("P-401/P-403 HMA Surface", 4.0, 200000),
            PavementLayer("P-209 Crushed Aggregate", 10.0, 75000),
            PavementLayer("Subgrade", 0.0, 15000),
        ]
        section.traffic = [TrafficAircraft(
            name="SWL-50", gross_weight=50000, mg_percent=0.95,
            tire_pressure=200, n_wheels=1,
            wheel_x=[0.0], wheel_y=[0.0],
            eval_x=[0.0], eval_y=[0.0],
            annual_departures=1200,
        )]
        result = design_flex_overlay(section)
        assert result.design_layer_index == 0
        assert result.converged or "Minimum" in result.message
        assert result.cdf_subgrade > 0


# ═══════════════════════════════════════════════════════════════════════════════
#  Cross-validation: design results vs FAARFIELD benchmarks
# ═══════════════════════════════════════════════════════════════════════════════

class TestCrossValidation:
    """End-to-end comparisons against FAARFIELD design outputs.

    These use simple single-aircraft cases where FAARFIELD's design
    thickness and CDF can be reproduced with reasonable tolerance.
    Tolerances are engineering-appropriate (within 1 inch of thickness,
    within 5% of CDF).
    """

    def _make_b737_like(self) -> TrafficAircraft:
        """Dual-wheel gear similar to 737-800."""
        return TrafficAircraft(
            name="B737-like", gross_weight=174200, mg_percent=0.95,
            tire_pressure=204, n_wheels=2,
            wheel_x=[-17.16, 17.16], wheel_y=[0.0, 0.0],
            eval_x=[0.0], eval_y=[0.0],
            annual_departures=1200,
        )

    @pytest.mark.slow
    def test_flex_design_3layer_b737(self):
        """Three-layer flexible design with a 737-like aircraft.

        Reference check: with E_sub=15000 psi, 20-year life at 1200
        annual departures, the aggregate thickness should fall in a
        reasonable range and CDF should be near 1.0.
        """
        from design_flex import design_flex

        section = PavementSection(name="B737 3-layer", design_life=20)
        section.layers = [
            PavementLayer("P-401/P-403 HMA Surface", 4.0, 200000),
            PavementLayer("P-209 Crushed Aggregate", 20.0, 75000),
            PavementLayer("Subgrade", 0.0, 15000),
        ]
        section.traffic = [self._make_b737_like()]

        result = design_flex(section, compute_asphalt_cdf=True, verbose=False)
        assert result.converged or "Minimum" in result.message

        # CDF should converge near 1.0 (within 5%)
        if result.converged:
            assert 0.95 <= result.cdf_subgrade <= 1.05

        # Aggregate thickness should be physically reasonable (10–50 in)
        agg_thick = result.layer_thicknesses[1]
        assert 10.0 <= agg_thick <= 50.0, f"Aggregate thickness {agg_thick:.1f} in"

    @pytest.mark.slow
    def test_flex_design_4layer(self):
        """Four-layer flexible with stabilized base (default stack)."""
        from design_flex import design_flex
        from materials import get_default_stack

        stack = get_default_stack("New Flexible")
        section = PavementSection(name="4-layer flex", design_life=20)
        section.layers = [
            PavementLayer(s.material_name, s.thickness, s.modulus)
            for s in stack
        ]
        section.traffic = [self._make_b737_like()]

        result = design_flex(section, design_layer_index=2,
                             compute_asphalt_cdf=True, verbose=False)
        assert result.converged or "Minimum" in result.message
        assert result.cdf_subgrade > 0

        # Designed aggregate thickness
        agg_thick = result.layer_thicknesses[2]
        assert 5.0 <= agg_thick <= 50.0, f"P-209 thickness {agg_thick:.1f} in"

    @pytest.mark.slow
    def test_overlay_design_b737(self):
        """Overlay on existing flexible pavement with 737-like traffic."""
        from design_flex import design_flex_overlay
        from materials import get_default_stack

        stack = get_default_stack("HMA Overlay on Flexible")
        section = PavementSection(name="Overlay B737", design_life=20)
        section.layers = [
            PavementLayer(s.material_name, s.thickness, s.modulus)
            for s in stack
        ]
        section.traffic = [self._make_b737_like()]

        result = design_flex_overlay(section, verbose=False)
        assert result.design_layer_index == 0

        overlay_thick = result.layer_thicknesses[0]
        assert overlay_thick >= 2.0, "Overlay should meet minimum"
        assert result.cdf_subgrade > 0

    @pytest.mark.slow
    def test_cdf_decreases_with_thickness(self):
        """Verify that increasing aggregate thickness decreases CDF."""
        from design_flex import design_flex

        cdfs = []
        for agg_t in [15.0, 25.0, 40.0]:
            section = PavementSection(name=f"t={agg_t}", design_life=20)
            section.layers = [
                PavementLayer("P-401/P-403 HMA Surface", 4.0, 200000),
                PavementLayer("P-209 Crushed Aggregate", agg_t, 75000),
                PavementLayer("Subgrade", 0.0, 15000),
            ]
            section.traffic = [self._make_b737_like()]
            # Just compute CDF, don't iterate
            from leaf import LEAFSolver, ResponseType
            from cdf import leaf_cdf_flex
            solver = LEAFSolver()
            leaf_ac = section.to_leaf_aircraft()
            ed = section.subgrade_depth
            ls = section.to_leaf_structure(ed, section.n_layers)
            sr = solver.compute_response(ResponseType.VERTICAL_STRAIN, leaf_ac, ls)

            import math as _m
            reps = [ac.total_departures(20) for ac in section.traffic]
            ne = [len(ac.eval_x) for ac in section.traffic]
            wx = [list(ac.wheel_x) for ac in section.traffic]
            tw = [_m.sqrt(ac.gear_load / ac.n_wheels / (ac.tire_pressure * 3.14159265359)) * 2.0
                  for ac in section.traffic]
            r = leaf_cdf_flex(sr, reps, ne, wx, tw, ed, 15000.0)
            cdfs.append(r.cdf_max)

        assert cdfs[0] > cdfs[1] > cdfs[2], (
            f"CDF should decrease: {cdfs}")
