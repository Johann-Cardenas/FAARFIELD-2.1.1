"""
Unit conversion utilities for NanoFlex.

All conversion factors match FAARFIELD's dimensional property classes exactly:
  Thickness.vb, Length.vb, Weight.vb, Pressure.vb, Modulus.vb,
  SubgradeReaction.vb, Area.vb, UsCustomary.vb, Metric.vb.

Internal calculations use US Customary (inches, psi, pounds).
"""

# ── Multiplicative factors: US Customary → SI ────────────────────────────────

IN_TO_MM = 25.4                   # inches → millimetres
LB_TO_KG = 0.453592               # pounds → kilograms
PSI_TO_KPA = 6.89476              # psi → kilopascals
PSI_TO_MPA = 0.00689476           # psi → megapascals
PCI_TO_MN_M3 = 0.271447           # pci → MN/m³
IN2_TO_MM2 = IN_TO_MM * IN_TO_MM  # in² → mm² (645.16)

# ── Inverse factors: SI → US Customary ────────────────────────────────────────

MM_TO_IN = 1.0 / IN_TO_MM
KG_TO_LB = 1.0 / LB_TO_KG
KPA_TO_PSI = 1.0 / PSI_TO_KPA
MPA_TO_PSI = 1.0 / PSI_TO_MPA
MN_M3_TO_PCI = 1.0 / PCI_TO_MN_M3
MM2_TO_IN2 = 1.0 / IN2_TO_MM2


def to_si(value: float, factor: float) -> float:
    """Convert a US Customary value to SI using the given factor."""
    return value * factor


def to_us(value: float, factor: float) -> float:
    """Convert an SI value to US Customary using the given factor."""
    return value / factor


def fahrenheit_to_celsius(f: float) -> float:
    """°F → °C  (matches UsCustomary.SetTemperatureValue)."""
    return (f - 32.0) / 1.8


def celsius_to_fahrenheit(c: float) -> float:
    """°C → °F  (matches Metric.SetTemperatureValue)."""
    return c * 1.8 + 32.0


# ── Metric display support ────────────────────────────────────────────────────

class UnitSystem:
    """Bidirectional unit converter with display labels.

    All NanoFlex internals use US Customary.  This class converts values
    for display/input in either system and provides the correct labels.
    """

    def __init__(self, metric: bool = False):
        self.metric = metric

    # ── Labels ─────────────────────────────────────────────────────────
    @property
    def thickness_label(self) -> str:
        return "mm" if self.metric else "in"

    @property
    def modulus_label(self) -> str:
        return "MPa" if self.metric else "psi"

    @property
    def pressure_label(self) -> str:
        return "kPa" if self.metric else "psi"

    @property
    def weight_label(self) -> str:
        return "kg" if self.metric else "lbs"

    @property
    def k_value_label(self) -> str:
        return "MN/m³" if self.metric else "pci"

    # ── Internal (US) → display ────────────────────────────────────────
    def display_thickness(self, inches: float) -> float:
        return inches * IN_TO_MM if self.metric else inches

    def display_modulus(self, psi: float) -> float:
        return psi * PSI_TO_MPA if self.metric else psi

    def display_pressure(self, psi: float) -> float:
        return psi * PSI_TO_KPA if self.metric else psi

    def display_weight(self, lbs: float) -> float:
        return lbs * LB_TO_KG if self.metric else lbs

    def display_k_value(self, pci: float) -> float:
        return pci * PCI_TO_MN_M3 if self.metric else pci

    # ── Display → internal (US) ────────────────────────────────────────
    def to_internal_thickness(self, value: float) -> float:
        return value * MM_TO_IN if self.metric else value

    def to_internal_modulus(self, value: float) -> float:
        return value * MPA_TO_PSI if self.metric else value

    def to_internal_pressure(self, value: float) -> float:
        return value * KPA_TO_PSI if self.metric else value

    def to_internal_weight(self, value: float) -> float:
        return value * KG_TO_LB if self.metric else value

    def to_internal_k_value(self, value: float) -> float:
        return value * MN_M3_TO_PCI if self.metric else value
