"""
Aircraft data loader for NanoFlex.

Reads the FAA-curated aircraft library from aircraft.xml (DataContract format)
and converts to TrafficAircraft instances for use in pavement design.
"""

from __future__ import annotations

import os
from dataclasses import dataclass
from typing import Optional
from xml.etree import ElementTree as ET

from structures import TrafficAircraft

# ── XML namespaces used by DataContractSerializer ─────────────────────────────

NS_DC = "http://schemas.datacontract.org/2004/07/FaarFieldModel"
NS_ARR = "http://schemas.microsoft.com/2003/10/Serialization/Arrays"
NS_XSI = "http://www.w3.org/2001/XMLSchema-instance"


@dataclass
class AircraftRecord:
    """Raw record from aircraft.xml before conversion to TrafficAircraft."""
    manufacturer: str
    name: str
    gross_weight_lbs: float
    mg_percent: float
    tire_pressure_psi: float
    n_wheels: int
    n_gears: int
    gear_type: str
    gear_orientation: int
    wheel_x: list[float]       # inches, per wheel
    wheel_y: list[float]       # inches, per wheel
    eval_x: list[float]        # inches
    eval_y: list[float]        # inches
    is_belly: bool
    deprecated: bool

    @property
    def display_name(self) -> str:
        return f"{self.manufacturer} {self.name}"

    def validate(self) -> list[str]:
        """Return warnings about data consistency."""
        warnings: list[str] = []
        if self.n_wheels < 1:
            warnings.append(f"{self.name}: n_wheels < 1")
        if len(self.wheel_x) != self.n_wheels:
            warnings.append(
                f"{self.name}: wheel_x count ({len(self.wheel_x)}) "
                f"!= n_wheels ({self.n_wheels})")
        if len(self.wheel_x) != len(self.wheel_y):
            warnings.append(
                f"{self.name}: wheel_x/wheel_y length mismatch")
        if not self.eval_x:
            warnings.append(f"{self.name}: no evaluation points")
        if self.gross_weight_lbs <= 0:
            warnings.append(f"{self.name}: gross weight <= 0")
        if self.tire_pressure_psi <= 0:
            warnings.append(f"{self.name}: tire pressure <= 0")
        return warnings

    def to_traffic_aircraft(
        self, annual_departures: int = 1200, annual_growth: float = 0.0
    ) -> TrafficAircraft:
        """Convert to a TrafficAircraft ready for pavement design."""
        return TrafficAircraft(
            name=self.name,
            gross_weight=self.gross_weight_lbs,
            mg_percent=self.mg_percent,
            tire_pressure=self.tire_pressure_psi,
            n_wheels=self.n_wheels,
            wheel_x=list(self.wheel_x),
            wheel_y=list(self.wheel_y),
            eval_x=list(self.eval_x),
            eval_y=list(self.eval_y),
            annual_departures=annual_departures,
            annual_growth=annual_growth,
            gear_type=self.gear_type,
            gear_orientation=self.gear_orientation,
        )


def _find_text(elem: ET.Element, tag: str, ns: str = NS_DC) -> str:
    """Extract text from a child element, returning '' if missing."""
    child = elem.find(f"{{{ns}}}{tag}")
    return child.text.strip() if child is not None and child.text else ""


def _find_float(elem: ET.Element, tag: str, ns: str = NS_DC) -> float:
    txt = _find_text(elem, tag, ns)
    return float(txt) if txt else 0.0


def _find_int(elem: ET.Element, tag: str, ns: str = NS_DC) -> int:
    txt = _find_text(elem, tag, ns)
    return int(float(txt)) if txt else 0


def _find_bool(elem: ET.Element, tag: str, ns: str = NS_DC) -> bool:
    return _find_text(elem, tag, ns).lower() == "true"


def _extract_us(elem: ET.Element, tag: str) -> float:
    """Extract the US Customary value from a dual-unit element."""
    parent = elem.find(f"{{{NS_DC}}}{tag}")
    if parent is None:
        return 0.0
    us = parent.find(f"{{{NS_DC}}}us")
    return float(us.text) if us is not None and us.text else 0.0


def _extract_coordinates(elem: ET.Element, tag: str) -> tuple[list[float], list[float]]:
    """Extract X, Y coordinate lists from a WheelCoordinates or EvaluationPoints element."""
    xs, ys = [], []
    parent = elem.find(f"{{{NS_DC}}}{tag}")
    if parent is None:
        return xs, ys
    for pt in parent.findall(f"{{{NS_ARR}}}anyType"):
        x_elem = pt.find(f"{{{NS_DC}}}X")
        y_elem = pt.find(f"{{{NS_DC}}}Y")
        if x_elem is not None and y_elem is not None:
            x_us = x_elem.find(f"{{{NS_DC}}}us")
            y_us = y_elem.find(f"{{{NS_DC}}}us")
            xs.append(float(x_us.text) if x_us is not None and x_us.text else 0.0)
            ys.append(float(y_us.text) if y_us is not None and y_us.text else 0.0)
    return xs, ys


def load_aircraft_library(
    xml_path: Optional[str] = None,
    include_belly: bool = False,
    include_deprecated: bool = False,
) -> list[AircraftRecord]:
    """Load the FAA aircraft library from aircraft.xml.

    Parameters
    ----------
    xml_path : path to aircraft.xml.  Defaults to the embedded library
               at FF2/Defaults/Aircraft/aircraft.xml relative to the repo root.
    include_belly : if True, include belly-gear variants.
    include_deprecated : if True, include deprecated aircraft.

    Returns
    -------
    List of AircraftRecord sorted by manufacturer then name.
    """
    if xml_path is None:
        base = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
        xml_path = os.path.join(base, "FF2", "Defaults", "Aircraft", "aircraft.xml")

    tree = ET.parse(xml_path)
    root = tree.getroot()

    airplanes_elem = root.find(f"{{{NS_DC}}}Airplanes")
    if airplanes_elem is None:
        raise ValueError("No <Airplanes> element found in aircraft.xml")

    records: list[AircraftRecord] = []

    for ac_elem in airplanes_elem.findall(f"{{{NS_ARR}}}anyType"):
        is_belly = _find_bool(ac_elem, "IsBelly")
        deprecated = _find_bool(ac_elem, "Deprecated")

        if not include_belly and is_belly:
            continue
        if not include_deprecated and deprecated:
            continue

        name = _find_text(ac_elem, "Name")
        manufacturer = _find_text(ac_elem, "Manufacturer")
        gw = _extract_us(ac_elem, "_GrossWeight")
        mg = _find_float(ac_elem, "MgPercent")
        cp = _extract_us(ac_elem, "Cp")
        nw = _find_int(ac_elem, "NumberWheels")
        ng = _find_int(ac_elem, "NumberGear")
        gear = _find_text(ac_elem, "Gear")
        gear_orient = _find_int(ac_elem, "GearOrientation")

        # FAARFIELD post-load: swap Tt↔B, replace "N" with "X"
        if gear == "N":
            gear = "X"

        wx, wy = _extract_coordinates(ac_elem, "WheelCoordinates")
        ex, ey = _extract_coordinates(ac_elem, "EvaluationPoints")

        if not ex:
            ex, ey = [0.0], [0.0]

        # Reconcile NumberWheels with actual coordinate count.
        # For gear type "X" (general gear), FAARFIELD may mirror
        # coordinates during post-load; the XML may already contain the
        # full set.  Use the actual coordinate count as the truth.
        actual_nw = len(wx)
        if actual_nw > 0 and actual_nw != nw:
            nw = actual_nw

        # For belly-gear variants, evaluation points sometimes reference
        # a combined main+belly gear.  Clamp to the actual wheel set.
        if is_belly and len(ex) == 0:
            ex, ey = [0.0], [0.0]

        # Truncate coordinates if more than n_wheels are present
        # (some XML entries have trailing zeros)
        if len(wx) > nw:
            wx = wx[:nw]
            wy = wy[:nw]

        records.append(AircraftRecord(
            manufacturer=manufacturer,
            name=name,
            gross_weight_lbs=gw,
            mg_percent=mg,
            tire_pressure_psi=cp,
            n_wheels=nw,
            n_gears=ng,
            gear_type=gear,
            gear_orientation=gear_orient,
            wheel_x=wx,
            wheel_y=wy,
            eval_x=ex,
            eval_y=ey,
            is_belly=is_belly,
            deprecated=deprecated,
        ))

    records.sort(key=lambda r: (r.manufacturer, r.name))
    return records


def find_aircraft(records: list[AircraftRecord], query: str) -> list[AircraftRecord]:
    """Search aircraft by name or manufacturer (case-insensitive substring)."""
    q = query.lower()
    return [r for r in records if q in r.name.lower() or q in r.manufacturer.lower()]
