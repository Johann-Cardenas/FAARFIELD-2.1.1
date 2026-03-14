"""
Material library and default layer stacks for NanoFlex.

All values are extracted from FF2/Libs/MaterialLibrary.vb,
FF2/Libs/ProgramDefaults.vb, and FaarFieldAnalysis/modINITLIBS.vb.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Optional


@dataclass
class MaterialDef:
    """A material definition from the FAARFIELD library."""
    name: str
    category: str
    layer_code: int
    default_thickness: float      # inches
    default_modulus: float         # psi
    default_poisson: float        # dimensionless
    default_cbr: Optional[float] = None       # %
    default_k_value: Optional[float] = None   # pci
    default_rupture: Optional[float] = None   # psi (PCC only)
    modulus_editable: bool = False
    thickness_range: tuple[float, float] = (1.0, 500.0)
    modulus_range: Optional[tuple[float, float]] = None


# ── Poisson's ratio by layer code ─────────────────────────────────────────────
# Source: FaarFieldAnalysis/modINITLIBS.vb DefaultPoisson()

POISSON_BY_CODE: dict[int, float] = {
    0: 0.35,   # User Defined
    1: 0.35,   # P-401/P-403 HMA Surface
    4: 0.35,   # Subgrade (flexible; 0.40 for rigid — handled at design time)
    5: 0.15,   # P-501 PCC Surface
    6: 0.35,   # P-209 Crushed Aggregate
    7: 0.20,   # Variable Stabilized (rigid)
    8: 0.35,   # P-154 Uncrushed Aggregate
    9: 0.35,   # Variable Stabilized (flexible)
    10: 0.35,  # P-401/P-403 HMA Overlay
    11: 0.15,  # PCC Overlay Unbonded
    12: 0.15,  # PCC Overlay Partially Bonded
    13: 0.15,  # PCC Overlay on Flexible
    14: 0.35,  # P-401/P-403 HMA Stabilized
    15: 0.20,  # P-301 Soil Cement Base
    16: 0.20,  # P-304 Cement Treated Base
    17: 0.20,  # P-306 Lean Concrete
    18: 0.35,  # P-208 Crushed Aggregate
    19: 0.35,  # P-219 Recycled Concrete Aggregate
    21: 0.35,  # P-211 Lime Rock
}

POISSON_SUBGRADE_FLEX = 0.35
POISSON_SUBGRADE_RIGID = 0.40

# ── Material catalogue ────────────────────────────────────────────────────────

MATERIALS: dict[str, MaterialDef] = {}

def _reg(m: MaterialDef):
    MATERIALS[m.name] = m

# General
_reg(MaterialDef("User Defined", "General", 0, 6, 100_000, 0.35,
                 default_cbr=66.6666, default_k_value=755.5,
                 modulus_editable=True,
                 modulus_range=(1_000, 4_000_000)))
_reg(MaterialDef("Subgrade", "General", 4, 12, 15_000, 0.35,
                 default_cbr=10.0, default_k_value=172.4,
                 modulus_editable=True,
                 modulus_range=(1_000, 50_000)))

# Aggregates
_reg(MaterialDef("P-154 Uncrushed Aggregate", "Aggregate", 8, 6, 40_000, 0.35))
_reg(MaterialDef("P-208 Crushed Aggregate", "Aggregate", 18, 6, 75_000, 0.35))
_reg(MaterialDef("P-209 Crushed Aggregate", "Aggregate", 6, 6, 75_000, 0.35))
_reg(MaterialDef("P-211 Lime Rock", "Aggregate", 21, 6, 60_000, 0.35))
_reg(MaterialDef("P-219 Recycled Concrete Aggregate", "Aggregate", 19, 6, 75_000, 0.35))

# HMA
_reg(MaterialDef("P-401/P-403 HMA Surface", "P-401/P-403 HMA", 1, 4, 200_000, 0.35))
_reg(MaterialDef("P-401/P-403 HMA Overlay", "P-401/P-403 HMA", 10, 6, 200_000, 0.35))

# PCC
_reg(MaterialDef("P-501 PCC Surface", "P-501 PCC", 5, 12, 4_000_000, 0.15,
                 default_rupture=650, modulus_editable=True,
                 modulus_range=(300_000, 5_000_000)))
_reg(MaterialDef("P-501 PCC Overlay (unbonded)", "P-501 PCC", 11, 12, 4_000_000, 0.15,
                 default_rupture=650, modulus_editable=True,
                 modulus_range=(300_000, 5_000_000)))
_reg(MaterialDef("P-501 PCC Overlay (partially bonded)", "P-501 PCC", 12, 12, 4_000_000, 0.15,
                 default_rupture=650, modulus_editable=True,
                 modulus_range=(300_000, 5_000_000)))
_reg(MaterialDef("P-501 PCC Overlay on Flexible", "P-501 PCC", 13, 15, 4_000_000, 0.15,
                 default_rupture=650, modulus_editable=True,
                 modulus_range=(300_000, 5_000_000)))

# Stabilized
_reg(MaterialDef("P-301 Soil Cement Base", "Stabilized", 15, 6, 250_000, 0.20,
                 modulus_editable=True))
_reg(MaterialDef("P-304 Cement Treated Base", "Stabilized", 16, 5, 500_000, 0.20,
                 modulus_editable=True))
_reg(MaterialDef("P-306 Lean Concrete", "Stabilized", 17, 5, 700_000, 0.20,
                 modulus_editable=True))
_reg(MaterialDef("P-401/P-403 HMA Stabilized", "Stabilized", 14, 5, 400_000, 0.35))
_reg(MaterialDef("Variable (flexible)", "Stabilized", 9, 5, 150_000, 0.35,
                 modulus_editable=True, modulus_range=(150_000, 400_000)))
_reg(MaterialDef("Variable (rigid)", "Stabilized", 7, 5, 250_000, 0.20,
                 modulus_editable=True, modulus_range=(250_000, 700_000)))


# ── CBR / Modulus / K-value conversions ───────────────────────────────────────

def cbr_to_modulus(cbr: float) -> float:
    """CBR → subgrade modulus (psi).  Default: E = CBR × 1500."""
    return cbr * 1500.0


def modulus_to_cbr(modulus: float) -> float:
    """Inverse of cbr_to_modulus."""
    return modulus / 1500.0


def cbr_to_modulus_nchrp(cbr: float) -> float:
    """NCHRP method: E = 2555 × CBR^0.64."""
    return 2555.0 * cbr ** 0.64


def modulus_to_k_value(modulus: float) -> float:
    """Modulus (psi) → subgrade reaction k (pci).  k = (E/20.15)^(1/1.28405)."""
    return (modulus / 20.15) ** (1.0 / 1.28405)


def k_value_to_modulus(k: float) -> float:
    """Inverse: E = k^1.28405 × 20.15."""
    return k ** 1.28405 * 20.15


def modulus_to_k_value_pca(modulus: float) -> float:
    """PCA method: k = 0.8155 × E^0.5719."""
    return 0.8155 * modulus ** 0.5719


def k_value_to_modulus_pca(k: float) -> float:
    """Inverse PCA: E = (k/0.8155)^(1/0.5719)."""
    return (k / 0.8155) ** (1.0 / 0.5719)


# ── Default layer stacks per analysis type ────────────────────────────────────

@dataclass
class LayerSpec:
    """A layer in a default pavement cross-section."""
    material_name: str
    thickness: float     # inches (0 = semi-infinite subgrade)
    modulus: float       # psi

    @property
    def material(self) -> MaterialDef:
        return MATERIALS[self.material_name]


# Only flexible-relevant types are included for NanoFlex
DEFAULT_STACKS: dict[str, list[LayerSpec]] = {
    "New Flexible": [
        LayerSpec("P-401/P-403 HMA Surface", 4, 200_000),
        LayerSpec("P-401/P-403 HMA Stabilized", 5, 400_000),
        LayerSpec("P-209 Crushed Aggregate", 10, 75_000),
        LayerSpec("Subgrade", 0, 15_000),
    ],
    "HMA on Aggregate": [
        LayerSpec("P-401/P-403 HMA Surface", 4, 200_000),
        LayerSpec("P-209 Crushed Aggregate", 10, 75_000),
        LayerSpec("P-154 Uncrushed Aggregate", 6, 40_000),
        LayerSpec("Subgrade", 0, 15_000),
    ],
    "HMA Overlay on Flexible": [
        LayerSpec("P-401/P-403 HMA Overlay", 4, 200_000),
        LayerSpec("P-401/P-403 HMA Surface", 4, 200_000),
        LayerSpec("Variable (flexible)", 5, 150_000),
        LayerSpec("P-209 Crushed Aggregate", 6, 75_000),
        LayerSpec("Subgrade", 0, 15_000),
    ],
}


def get_default_stack(analysis_type: str) -> list[LayerSpec]:
    """Return a copy of the default layer stack for the given analysis type."""
    if analysis_type not in DEFAULT_STACKS:
        raise ValueError(f"Unknown analysis type: {analysis_type}")
    return [LayerSpec(ls.material_name, ls.thickness, ls.modulus)
            for ls in DEFAULT_STACKS[analysis_type]]
