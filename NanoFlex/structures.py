"""
Pavement structure and traffic mix data model for NanoFlex.

Mirrors the domain model from FaarFieldModel (ISection, IMaterial, IAirplaneInfo)
in a lightweight Python form suitable for the LEAF solver and design modules.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Optional

import numpy as np
from numpy.typing import NDArray

from materials import MATERIALS, POISSON_BY_CODE, MaterialDef
from leaf import LEAFStructure, LEAFAircraft

# ── Validation limits (matching FAARFIELD's UI constraints) ────────────────
_MODULUS_LIMITS: dict[int, tuple[float, float]] = {
    1:  (100_000, 400_000),    # P-401/P-403 HMA Surface
    10: (100_000, 400_000),    # P-401/P-403 HMA Overlay
    14: (100_000, 400_000),    # P-401/P-403 HMA Stabilized
    4:  (1_000, 50_000),       # Subgrade
    5:  (300_000, 5_000_000),  # P-501 PCC Surface
    6:  (20_000, 100_000),     # P-209 Crushed Aggregate
    18: (20_000, 100_000),     # P-208 Crushed Aggregate
    8:  (10_000, 60_000),      # P-154 Uncrushed Aggregate
    19: (20_000, 100_000),     # P-219 Recycled Concrete Aggregate
    21: (20_000, 100_000),     # P-211 Lime Rock
    15: (100_000, 500_000),    # P-301 Soil Cement Base
    16: (200_000, 700_000),    # P-304 Cement Treated Base
    17: (300_000, 1_000_000),  # P-306 Lean Concrete
    9:  (150_000, 400_000),    # Variable (flexible)
    7:  (250_000, 700_000),    # Variable (rigid)
}

_MIN_THICKNESS: dict[int, float] = {
    1: 2.0, 10: 2.0, 14: 4.0, 5: 6.0, 11: 6.0, 12: 6.0, 13: 6.0,
    6: 6.0, 18: 6.0, 8: 6.0, 19: 6.0, 21: 6.0,
    15: 4.0, 16: 4.0, 17: 4.0, 9: 4.0, 7: 4.0,
}


@dataclass
class PavementLayer:
    """Single layer in a pavement cross-section."""
    material_name: str
    thickness: float          # inches (0 for semi-infinite subgrade)
    modulus: float            # psi
    poisson: Optional[float] = None
    layer_code: Optional[int] = None
    interface_bond: float = 1.0   # 1=fully bonded, 0=unbonded

    def __post_init__(self):
        mat = MATERIALS.get(self.material_name)
        if mat and self.layer_code is None:
            self.layer_code = mat.layer_code
        if self.poisson is None:
            code = self.layer_code if self.layer_code is not None else 0
            self.poisson = POISSON_BY_CODE.get(code, 0.35)

    def validate(self) -> list[str]:
        """Return a list of validation warnings (empty if OK)."""
        warnings: list[str] = []
        code = self.layer_code if self.layer_code is not None else 0
        is_subgrade = (code == 4)

        if not is_subgrade and self.thickness <= 0:
            warnings.append(f"{self.material_name}: thickness must be > 0")

        min_t = _MIN_THICKNESS.get(code, 1.0)
        if not is_subgrade and self.thickness < min_t:
            warnings.append(
                f"{self.material_name}: thickness {self.thickness:.1f} in "
                f"below minimum {min_t:.1f} in")

        if self.modulus <= 0:
            warnings.append(f"{self.material_name}: modulus must be > 0")

        limits = _MODULUS_LIMITS.get(code)
        if limits and not (limits[0] <= self.modulus <= limits[1]):
            warnings.append(
                f"{self.material_name}: modulus {self.modulus:,.0f} psi "
                f"outside range [{limits[0]:,.0f}, {limits[1]:,.0f}]")

        if self.poisson is not None and not (0.0 < self.poisson < 0.5):
            warnings.append(
                f"{self.material_name}: Poisson's ratio {self.poisson} "
                f"outside valid range (0, 0.5)")

        if not (0.0 <= self.interface_bond <= 1.0):
            warnings.append(
                f"{self.material_name}: interface bond {self.interface_bond} "
                f"must be in [0, 1]")

        return warnings


@dataclass
class TrafficAircraft:
    """An aircraft in the traffic mix."""
    name: str
    gross_weight: float       # lbs
    mg_percent: float         # fraction on main gear (0–1)
    tire_pressure: float      # psi
    n_wheels: int             # wheels per main gear
    wheel_x: list[float]      # x-coordinates (inches), per wheel
    wheel_y: list[float]      # y-coordinates (inches), per wheel
    eval_x: list[float]       # evaluation point x-coordinates
    eval_y: list[float]       # evaluation point y-coordinates
    annual_departures: int = 1200
    annual_growth: float = 0.0   # percent per year
    gear_type: str = ""
    gear_orientation: int = 0

    @property
    def gear_load(self) -> float:
        """Total load on one main gear assembly (lbs)."""
        return self.gross_weight * self.mg_percent

    def total_departures(self, design_life: int) -> float:
        """Total departures over the design life with compound growth."""
        if self.annual_growth == 0.0:
            return self.annual_departures * design_life
        r = self.annual_growth / 100.0
        return self.annual_departures * ((1 + r) ** design_life - 1) / r


@dataclass
class PavementSection:
    """Complete pavement cross-section with traffic mix."""
    name: str = "New Section"
    layers: list[PavementLayer] = field(default_factory=list)
    traffic: list[TrafficAircraft] = field(default_factory=list)
    design_life: int = 20             # years
    cdf_tolerance: float = 0.005      # convergence criterion for design

    @property
    def n_layers(self) -> int:
        return len(self.layers)

    @property
    def total_thickness(self) -> float:
        """Sum of all finite layer thicknesses (inches)."""
        return sum(l.thickness for l in self.layers if l.thickness > 0)

    @property
    def subgrade_depth(self) -> float:
        """Depth to top of subgrade (inches)."""
        return sum(l.thickness for l in self.layers[:-1])

    def validate(self) -> list[str]:
        """Validate the section and return a list of warnings (empty if OK)."""
        warnings: list[str] = []
        if len(self.layers) < 2:
            warnings.append("Section must have at least 2 layers (surface + subgrade)")
        for layer in self.layers:
            warnings.extend(layer.validate())
        if self.layers and self.layers[-1].layer_code != 4:
            warnings.append("Last layer should be subgrade (layer code 4)")
        if self.design_life < 1:
            warnings.append("Design life must be >= 1 year")
        for ac in self.traffic:
            if ac.gross_weight <= 0:
                warnings.append(f"{ac.name}: gross weight must be > 0")
            if ac.tire_pressure <= 0:
                warnings.append(f"{ac.name}: tire pressure must be > 0")
            if ac.n_wheels < 1:
                warnings.append(f"{ac.name}: must have at least 1 wheel")
            if not (0.0 < ac.mg_percent <= 1.0):
                warnings.append(f"{ac.name}: mg_percent must be in (0, 1]")
            if ac.annual_departures < 0:
                warnings.append(f"{ac.name}: annual departures must be >= 0")
            if len(ac.wheel_x) != ac.n_wheels:
                warnings.append(
                    f"{ac.name}: wheel_x count ({len(ac.wheel_x)}) "
                    f"!= n_wheels ({ac.n_wheels})")
        return warnings

    def to_leaf_structure(self, eval_depth: float, eval_layer: int) -> LEAFStructure:
        """Convert to LEAF solver input format (1-based arrays)."""
        nl = self.n_layers
        thick = np.zeros(nl + 1)
        modulus = np.zeros(nl + 1)
        poisson = np.zeros(nl + 1)
        interface_parm = np.zeros(nl + 1)
        for i, lay in enumerate(self.layers):
            thick[i + 1] = lay.thickness
            modulus[i + 1] = lay.modulus
            poisson[i + 1] = lay.poisson if lay.poisson is not None else 0.35
            interface_parm[i + 1] = lay.interface_bond
        return LEAFStructure(
            n_layers=nl, thick=thick, modulus=modulus, poisson=poisson,
            interface_parm=interface_parm, eval_depth=eval_depth,
            eval_layer=eval_layer,
        )

    def to_leaf_aircraft(self) -> list[LEAFAircraft]:
        """Convert traffic mix to LEAF aircraft format (1-based arrays)."""
        result = []
        for ac in self.traffic:
            nw = ac.n_wheels
            ne = len(ac.eval_x)
            tp = np.zeros(nw + 1)
            tx = np.zeros(nw + 1)
            ty = np.zeros(nw + 1)
            ex = np.zeros(ne + 1)
            ey = np.zeros(ne + 1)
            for j in range(nw):
                tp[j + 1] = ac.tire_pressure
                tx[j + 1] = ac.wheel_x[j]
                ty[j + 1] = ac.wheel_y[j]
            for j in range(ne):
                ex[j + 1] = ac.eval_x[j]
                ey[j + 1] = ac.eval_y[j]
            result.append(LEAFAircraft(
                name=ac.name, gear_load=ac.gear_load, n_tires=nw,
                tire_press=tp, tire_x=tx, tire_y=ty,
                n_eval_points=ne, eval_x=ex, eval_y=ey,
                gear=ac.gear_type, gear_orientation=ac.gear_orientation,
            ))
        return result
