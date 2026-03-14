"""
NanoFlex Web Application — Flask backend.

Serves a FAARFIELD-inspired web UI for flexible pavement design,
life computation, ACR and PCR analysis.  All computation uses the
existing NanoFlex Python engine (leaf.py, cdf.py, design_flex.py, etc.).

Usage:
    cd NanoFlex
    pip install -r requirements.txt
    python app.py
    # Open http://127.0.0.1:5000
"""

import sys
import os
import traceback

import numpy as np
from flask import Flask, render_template, request, jsonify

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from materials import MATERIALS, DEFAULT_STACKS, get_default_stack
from structures import PavementSection, PavementLayer, TrafficAircraft
from aircraft import load_aircraft_library, find_aircraft
from design_flex import design_flex, design_flex_overlay, compute_life
from acr_pcr import compute_acr, compute_pcr

app = Flask(__name__)

_aircraft_library = None


def _get_library():
    global _aircraft_library
    if _aircraft_library is None:
        _aircraft_library = load_aircraft_library()
    return _aircraft_library


def _to_python(obj):
    """Recursively convert numpy types to builtin Python types for JSON."""
    if isinstance(obj, dict):
        return {k: _to_python(v) for k, v in obj.items()}
    if isinstance(obj, (list, tuple)):
        return type(obj)(_to_python(v) for v in obj)
    if isinstance(obj, np.ndarray):
        return obj.tolist()
    if isinstance(obj, (np.floating,)):
        return float(obj)
    if isinstance(obj, (np.integer,)):
        return int(obj)
    if isinstance(obj, np.bool_):
        return bool(obj)
    return obj


def _build_section(data: dict) -> PavementSection:
    """Construct a PavementSection from the JSON request body."""
    layers = []
    for ld in data["layers"]:
        layers.append(PavementLayer(
            material_name=ld["material_name"],
            thickness=float(ld["thickness"]),
            modulus=float(ld["modulus"]),
            poisson=ld.get("poisson"),
            layer_code=ld.get("layer_code"),
        ))
    traffic = []
    for td in data.get("traffic", []):
        traffic.append(TrafficAircraft(
            name=td["name"],
            gross_weight=float(td["gross_weight"]),
            mg_percent=float(td["mg_percent"]),
            tire_pressure=float(td["tire_pressure"]),
            n_wheels=int(td["n_wheels"]),
            wheel_x=td["wheel_x"],
            wheel_y=td["wheel_y"],
            eval_x=td["eval_x"],
            eval_y=td["eval_y"],
            annual_departures=int(td.get("annual_departures", 1200)),
            annual_growth=float(td.get("annual_growth", 0.0)),
            gear_type=td.get("gear_type", ""),
            gear_orientation=int(td.get("gear_orientation", 0)),
        ))
    return PavementSection(
        name=data.get("name", "Section"),
        layers=layers,
        traffic=traffic,
        design_life=int(data.get("design_life", 20)),
    )


# ── Routes ────────────────────────────────────────────────────────────────────

@app.route("/")
def index():
    return render_template("index.html")


@app.route("/api/materials")
def api_materials():
    result = []
    for name, m in MATERIALS.items():
        result.append({
            "name": m.name,
            "category": m.category,
            "layer_code": m.layer_code,
            "default_thickness": m.default_thickness,
            "default_modulus": m.default_modulus,
            "default_poisson": m.default_poisson,
            "modulus_editable": m.modulus_editable,
        })
    return jsonify(result)


@app.route("/api/stacks")
def api_stacks():
    return jsonify(list(DEFAULT_STACKS.keys()))


@app.route("/api/stack/<path:name>")
def api_stack(name):
    try:
        stack = get_default_stack(name)
    except ValueError as exc:
        return jsonify({"error": str(exc)}), 400
    layers = []
    for ls in stack:
        m = MATERIALS[ls.material_name]
        layers.append({
            "material_name": ls.material_name,
            "thickness": ls.thickness,
            "modulus": ls.modulus,
            "poisson": m.default_poisson,
            "layer_code": m.layer_code,
        })
    return jsonify({"layers": layers})


@app.route("/api/aircraft/search")
def api_aircraft_search():
    lib = _get_library()
    q = request.args.get("q", "").strip()
    if len(q) < 2:
        return jsonify([])
    hits = find_aircraft(lib, q)[:50]
    return jsonify([{
        "name": r.name,
        "manufacturer": r.manufacturer,
        "display_name": r.display_name,
        "gross_weight": r.gross_weight_lbs,
        "mg_percent": r.mg_percent,
        "tire_pressure": r.tire_pressure_psi,
        "n_wheels": r.n_wheels,
        "gear_type": r.gear_type,
        "wheel_x": r.wheel_x,
        "wheel_y": r.wheel_y,
        "eval_x": r.eval_x,
        "eval_y": r.eval_y,
        "gear_orientation": r.gear_orientation,
    } for r in hits])


@app.route("/api/design", methods=["POST"])
def api_design():
    try:
        data = request.get_json()
        section = _build_section(data)
        analysis_type = data.get("analysis_type", "New Flexible")
        if "Overlay" in analysis_type:
            result = design_flex_overlay(section, verbose=False)
        else:
            result = design_flex(section, verbose=False)
        return jsonify(_to_python({
            "converged": result.converged,
            "iterations": result.iterations,
            "layer_thicknesses": result.layer_thicknesses,
            "cdf_subgrade": result.cdf_subgrade,
            "cdf_asphalt": result.cdf_asphalt,
            "message": result.message,
            "subgrade_strain": result.subgrade_strain,
            "n_to_fail_subgrade": result.n_to_fail_subgrade,
            "n_to_fail_asphalt": result.n_to_fail_asphalt,
            "design_layer_index": result.design_layer_index,
        }))
    except Exception:
        return jsonify({"error": traceback.format_exc()}), 500


@app.route("/api/life", methods=["POST"])
def api_life():
    try:
        section = _build_section(request.get_json())
        life = compute_life(section)
        return jsonify({"life_years": float(life)})
    except Exception:
        return jsonify({"error": traceback.format_exc()}), 500


@app.route("/api/acr", methods=["POST"])
def api_acr():
    try:
        data = request.get_json()
        ad = data["aircraft"]
        ac = TrafficAircraft(
            name=ad["name"],
            gross_weight=float(ad["gross_weight"]),
            mg_percent=float(ad["mg_percent"]),
            tire_pressure=float(ad["tire_pressure"]),
            n_wheels=int(ad["n_wheels"]),
            wheel_x=ad["wheel_x"],
            wheel_y=ad["wheel_y"],
            eval_x=ad["eval_x"],
            eval_y=ad["eval_y"],
            gear_type=ad.get("gear_type", ""),
        )
        cats = data.get("categories", ["A", "B", "C", "D"])
        result = compute_acr(ac, categories=cats)
        return jsonify(_to_python({
            "aircraft_name": result.aircraft_name,
            "acr": result.acr,
            "dswl_lbs": result.dswl_lbs,
            "reference_thickness": result.reference_thickness,
        }))
    except Exception:
        return jsonify({"error": traceback.format_exc()}), 500


@app.route("/api/pcr", methods=["POST"])
def api_pcr():
    try:
        section = _build_section(request.get_json())
        result = compute_pcr(section)
        return jsonify(_to_python({
            "pcr": result.pcr,
            "critical_aircraft": result.critical_aircraft,
            "max_gross_weight": result.max_gross_weight,
            "subgrade_category": result.subgrade_category,
            "life_years": result.life_years,
            "acr_at_mgw": result.acr_at_mgw,
        }))
    except Exception:
        return jsonify({"error": traceback.format_exc()}), 500


if __name__ == "__main__":
    print("NanoFlex - Loading aircraft library...")
    _get_library()
    print(f"  {len(_aircraft_library)} aircraft loaded.")
    print()
    print("Starting NanoFlex at http://127.0.0.1:5000")
    app.run(debug=False, port=5000)
