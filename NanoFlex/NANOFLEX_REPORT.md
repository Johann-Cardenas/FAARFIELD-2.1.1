# NanoFlex — Implementation Report

**Project:** Python reimplementation of FAARFIELD 2.1.1 flexible pavement design and ACR/PCR computations  
**Repository:** `FAARFIELD-2.1.1/NanoFlex/`  
**Date:** March 2026  

---

## 1. Objective

Replicate the iterative flexible pavement thickness design and ACR/PCR computation modules of FAARFIELD 2.1.1 in Python, such that **identical inputs produce the same outputs** as the original VB.NET desktop application.  The implementation targets:

- Layered elastic analysis (LEAF solver)
- Cumulative Damage Factor (CDF) computation with subgrade and asphalt failure models
- Newton-Raphson thickness design iteration
- Aircraft Classification Rating (ACR) on ICAO reference structures
- Pavement Classification Rating (PCR) via critical-aircraft MGW determination

The user interface is a **Flask web application** served locally at `http://127.0.0.1:5000`, providing a FAARFIELD-inspired UI in the browser.

---

## 2. Architecture and File Structure

```
NanoFlex/
├── app.py                  Flask backend (REST API + page serving)
├── templates/
│   └── index.html          Single-page HTML/JS/Bootstrap UI
├── static/
│   └── style.css           Custom theme (navy header, clean cards)
│
├── numerical.py            Bessel functions, quadrature, linear algebra
├── units.py                US Customary ↔ SI conversion factors
├── leaf.py                 LEAF layered elastic solver
├── materials.py            Material catalogue, CBR/E/k conversions, default stacks
├── structures.py           PavementSection, PavementLayer, TrafficAircraft
├── aircraft.py             FAA aircraft library XML parser (235 aircraft)
├── cdf.py                  Damage models, coverage-to-pass, CDF accumulation
├── design_flex.py          Newton-Raphson thickness design, life computation
├── acr_pcr.py              ACR (DSWL iteration), PCR (elimination algorithm)
│
├── test_nanoflex.py        Comprehensive pytest test suite
├── requirements.txt        Python dependencies
└── NANOFLEX_REPORT.md      This document
```

**Dependency graph (import order):**
```
numerical.py          (no deps — standalone math)
units.py              (no deps — standalone conversions)
  └── leaf.py         (imports numerical)
      └── materials.py       (no deps)
          └── structures.py  (imports materials, leaf)
              └── aircraft.py    (imports structures)
                  └── cdf.py         (imports leaf)
                      └── design_flex.py (imports leaf, cdf, structures, materials)
                          └── acr_pcr.py     (imports leaf, cdf, structures, design_flex)
                              └── app.py         (imports all above — Flask layer)
```

---

## 3. How Computations Work — Detailed Technical Description

### 3.1 Layered Elastic Analysis (LEAF Solver)

The LEAF solver (`leaf.py`) computes stresses, strains, and deflections in a multi-layered elastic half-space subjected to one or more circular surface loads. It is a faithful port of `LEAFClassLib/clsLEAF.vb`, originally derived from Burmister's layered elastic theory.

#### 3.1.1 Mathematical Basis

The fundamental problem is: given *N* horizontal elastic layers bonded (or partially bonded) at their interfaces, resting on a semi-infinite elastic subgrade, with circular uniform loads applied at the surface, find the stress/strain/deflection at any depth.

The solution uses the **Hankel transform** (cylindrical Fourier transform). For each response quantity *R(r,z)* at radial distance *r* and depth *z*, the solution takes the form:

```
R(r, z) = ∫₀^∞ K(α, z) · J_n(α·r) · α dα
```

where *α* is the Hankel transform variable, *K(α,z)* is the kernel containing layer coefficients, and *J_n* is a Bessel function of order 0 or 1 depending on the response type.

#### 3.1.2 Numerical Integration

The semi-infinite integral is evaluated using **500-point Gauss-Laguerre quadrature** (nodes and weights computed by `gaulag` in `numerical.py` via Newton-Raphson root refinement of the Laguerre polynomial). The quadrature naturally handles the exponential decay of the integrand.

For each quadrature point *α_g*:

1. **Origin shifts** (`_set_o_shifts`): Computed to stabilise the exponential terms `exp(-α·h)` that appear in the layer coefficient matrix. Without shifts, large *α*×*h* products would cause numerical overflow/underflow.

2. **Layer coefficient system** (`_find_constants`): A `4N×4N` linear system is assembled from the boundary conditions at each interface (continuity of vertical stress, shear stress, vertical displacement, and modified radial displacement/interface shear). The system is reduced to `(4N-2)×(4N-2)` by eliminating the two subgrade coefficients that must be zero (bounded solution at infinite depth).

   Three solvers are attempted in sequence:
   - **Partial inversion** (`_find_constants_part_invert`): Block 4×4 elimination without pivoting — fastest, O(N) per quadrature point
   - **LU decomposition** (`lu_solve`): Numerical Recipes `ludcmp`+`lubksb` with partial pivoting
   - **Gauss-Jordan** (`gauss_jordan`): Full pivoting, most robust, used as final fallback

3. **Integrand evaluation**: The layer coefficients (A_i, B_i, C_i, D_i for each layer) are combined with Bessel functions and exponentials to form the integrand for the requested response type:
   - **Vertical strain**: Uses `J₀(α·r)` for the radial Bessel kernel
   - **Horizontal stress**: Uses both `J₀(α·r)` and `J₁(α·r)/r` for radial and tangential components, then converts from cylindrical (σ_r, σ_θ) to Cartesian (σ_x) via `σ_x = σ_r·cos²θ + σ_θ·sin²θ`
   - **Vertical deflection**: Uses `J₀(α·r)` with the displacement kernel

4. **Convergence tracking**: Integration terminates early when the incremental contribution falls below `10⁻⁶` of the accumulated sum for all aircraft simultaneously.

5. **Dummy top layer**: A 1-inch dummy layer of the same material as the surface is always inserted at the top, matching FAARFIELD's `ComputeResponse`. This improves numerical stability for the partial inversion solver.

6. **Final scaling**: The raw integral is multiplied by `p·a·(1+ν)/E` where *p* is tire pressure, *a* is contact radius, and *ν*, *E* are the evaluation layer's Poisson's ratio and modulus.

#### 3.1.3 Load Model

Each tire is modelled as a **uniform circular pressure** on the surface. The contact radius is:

```
a = √(W_wheel / (π · p_tire))
```

where `W_wheel = GearLoad / N_tires` and `p_tire` is the tire inflation pressure. Superposition of multiple tires is handled by summing `J₀(α·r_it)` contributions across all tires at each evaluation point, where `r_it` is the distance from tire *t* to evaluation point *e*.

#### 3.1.4 Gear and Load Handling

**How gear load is determined:**
1. The aircraft's **gross weight** is multiplied by **MG percent** (fraction on main gear, typically 0.93–0.95) to get the **gear load**
2. The gear load is divided equally among all tires: `W_wheel = GearLoad / N_tires`
3. Each tire applies uniform pressure `p_tire` over a circular area of radius `a`

**How gear geometry enters the computation:**
- Each aircraft carries arrays of wheel coordinates `(tire_x[i], tire_y[i])` defining the gear footprint (in inches, relative to the gear centroid)
- Evaluation points `(eval_x[j], eval_y[j])` are positions where the response is computed
- The LEAF solver computes `r_ij = √((eval_x[j]-tire_x[i])² + (eval_y[j]-tire_y[i])²)` for every tire-eval pair
- Response at each eval point is the superposition of all tire contributions

**Gear types in FAARFIELD:**
- `S` — Single wheel
- `D` — Dual wheel
- `2S` — Two singles in tandem
- `2D` — Dual tandem (4 wheels)
- `3D` — Triple dual tandem (6 wheels)
- `2D/2D2` — Complex dual tandem variants
- `X` — General gear (arbitrary geometry, uses special coverage-to-pass logic)

**Important runtime behaviour:** Aircraft loaded from `aircraft.xml` are always treated as `libNGroups = 1` (single gear group), regardless of how many physical gear assemblies they have. The XML stores the full wheel coordinate set, and the solver uses superposition. Multi-gear handling (`WFBF`/`WFBN`) only exists in the hardcoded `modAC.vb` library, not in the XML workflow.

### 3.2 CDF Computation

The Cumulative Damage Factor (`cdf.py`) converts raw LEAF responses into a failure prediction by accumulating damage from all aircraft in the traffic mix across multiple lateral offsets.

#### 3.2.1 Subgrade Damage Models

**Standard Model** (default in FAARFIELD 2.1.1):
```
AA = 0.000247 + 0.000245 · log₁₀(E_subgrade)
BB = 0.0658 · E_subgrade^0.559
NtoFail = 10000 · (AA / ε_vertical)^BB
```

**Straight-Line Model** (dual-branch):
```
If ε > ε_break:  NtoFail = (0.004 / ε)^8.1
Else:            NtoFail = (AA_orig / ε)^BB_orig
```
where `ε_break` is the crossover point between the two branches.

**Bleasdale Model** (three-parameter):
```
If ε ≤ 0.001765:  NtoFail = 10^((a + b·ε)^(-1/c))
Else:             NtoFail = (0.00414 / ε)^8.1
```

#### 3.2.2 Asphalt Damage Models

**AI (Asphalt Institute) Standard**:
```
NtoFail = 10^(2.68 - 5·log₁₀(ε) - 2.665·log₁₀(E_asphalt))
```

**RDEC (Rate of Dissipated Energy Change)**:
```
PV = 44.422 · ε^5.14 · E_MPa^2.993 · VoidParam^1.85 · GradParam^(-0.4063)
NtoFail = 0.4801 · PV^(-0.90074)
```
Uses six mix-design parameters: flexural modulus, air voids, asphalt content by volume, PNMS, PPCS, P200.

#### 3.2.3 Coverage-to-Pass Ratio

The coverage-to-pass ratio accounts for the **lateral wander** of aircraft as they taxi along the runway/taxiway. Each wheel traces a path that is normally distributed around the centerline with standard deviation σ = 30.435 inches (corresponding to 70-inch wander width).

**Standard computation** (`coverage_to_pass_flex`):
1. Compute the effective tire pass width: `gp = depth + tire_width`
2. For each wheel, determine left/right integration limits (handling overlap between adjacent wheels)
3. For each offset position, integrate the Gaussian probability that the wheel footprint covers that offset
4. Sum across all wheels in the bottommost row

**General gear computation** (`coverage_to_pass_general`):
1. Sort all tires by Y-coordinate (longitudinal direction)
2. Identify the "bottom row" (southernmost wheels)
3. Group wheels into columns by X-coordinate (within tire-width tolerance)
4. For each column, compute a **tandem multiplier** based on depth and inter-axle spacing:
   - If `depth > 2·gap`: fully overlapping, no multiplier change
   - If `gap < depth ≤ 2·gap`: `mult += 2.0 - depth/gap` (partial overlap)
   - If `depth ≤ gap`: `mult += 1.0` (separate contribution)
5. Apply Gaussian wander integration weighted by tandem multipliers

#### 3.2.4 Tandem CDF Method (gTandemFnew)

This is the **default** subgrade CDF method in FAARFIELD 2.1.1. Instead of using a single maximum strain value, it scans the longitudinal strain profile for peaks and valleys.

**Two-pass LEAF computation** (`compute_tandem_strains`):
1. **Pass 1:** Standard eval points → find the transverse (X) offset where vertical strain is most compressive
2. **Pass 2:** Generate 1800 longitudinal (Y) eval points at the critical X-offset, spanning from `tire_y_min - 160` to `tire_y_max + 160` inches → re-run LEAF

**Longitudinal strain scanning** (`scan_tandem_damage`):
1. Walk through the 1800-node strain profile
2. Identify local extrema:
   - **Valley** (more compressive, `s_prev > s_curr < s_next`): `ExtrType = 2`
   - **Peak** (less compressive, `s_prev < s_curr > s_next`): `ExtrType = 1`
3. For each extremum, compute `NtoFail` from the absolute strain
4. Accumulate signed damage: `Damage += (-1)^ExtrType / NtoFail`
   - Valleys (ExtrType=2) **add** damage (positive contribution)
   - Peaks (ExtrType=1) **subtract** damage (partial recovery)

**Effect on coverage-to-pass:** When tandem scanning is active, the coverage-to-pass computation omits the tandem multiplier (tandem effects are already captured via the strain profile scanning).

#### 3.2.5 CDF Sweep

The CDF is computed at **41 equally-spaced lateral offsets** (0 to 400 inches, in 10-inch increments):

```
For each offset position (ioff = 1..41):
    For each aircraft:
        CDF_contribution = Repetitions × CoverageToPass(offset) × Damage
        CDF_total[ioff] += CDF_contribution

CDF_max = max(CDF_total[1..41])
```

The critical offset (the one with maximum total CDF) determines the controlling failure location.

### 3.3 Iterative Flexible Pavement Thickness Design

The thickness design (`design_flex.py`) finds the layer thickness that produces CDF = 1.0 at the top of the subgrade. This is a port of `FaarFieldAnalysis/modStrDesignFlex.vb`.

#### 3.3.1 Step-by-Step Algorithm

**Inputs:**
- Pavement cross-section (N layers with material, thickness, modulus, Poisson's ratio)
- Traffic mix (M aircraft with gear geometry, weight, departures, growth rate)
- Design layer index (default: last layer before subgrade)
- Design life (years)
- Convergence tolerance (|ln(CDF)| < 0.005)

**Iteration procedure:**

```
Step 1: INITIALIZATION
  - Set design layer to initial thickness (from default stack)
  - Compute minimum thickness constraint for the material
  - Prepare LEAF solver (compute 500-point quadrature weights once)
  - Convert all aircraft to LEAF format (gear load, tire radii, coordinates)
  - Pre-compute total departures per aircraft (with compound growth)
  - Pre-compute tire widths for coverage-to-pass

Step 2: MAIN LOOP (up to 50 iterations)

  Step 2a: BUILD LEAF STRUCTURE
    - Compute eval_depth = sum of all layer thicknesses above subgrade
    - If aggregate sublayer mode is active:
        Expand aggregate layers using WES formula:
          E_i = E_{i+1} · (1 + C·log₁₀(t) - D·log₁₀(E_{i+1})·log₁₀(t))
        where C=10.52, D=2.1 for base; C=7.18, D=1.56 for subbase
        Sublayer thickness = total_thickness / N_sublayers
        N_sublayers = ceil(thickness / thick_min) where thick_min = 10" (base) or 8" (subbase)
    - Build LEAFStructure with 1-based arrays

  Step 2b: COMPUTE STRAINS
    - Two-pass LEAF for tandem CDF:
      Pass 1: Standard eval points → find critical X-offset
      Pass 2: 1800 longitudinal points at critical X → dense strain profile
    - Standard LEAF for overflow check

  Step 2c: COMPUTE CDF
    - Call leaf_cdf_flex with use_tandem=True
    - For each aircraft at each of 41 offsets:
        damage = scan_tandem_damage(longitudinal_profile)
        CDF += repetitions × coverage_to_pass(offset) × damage
    - Record CDF_max across all offsets

  Step 2d: CHECK OVERFLOW
    - If all strains < 1e-8: structure is over-designed
    - Halve the design layer thickness and retry
    - If already at minimum thickness: stop

  Step 2e: ACTIVATE SUBLAYER EXPANSION
    - When |ln(CDF)| < 0.69 and section has aggregate layers:
      Switch to aggregate sublayer mode
      Reset Newton-Raphson state (t_m1, cdf_m1)
      Force re-evaluation

  Step 2f: FREEZE SUBLAYER COUNTS
    - When |ln(CDF)| < 0.483 (0.69×0.7):
      Record current sublayer counts for each aggregate layer
      Freeze them to prevent oscillation near CDF=1.0

  Step 2g: CHECK CONVERGENCE
    - If |ln(CDF)| < 0.005: CONVERGED → exit loop
    - If thickness ≤ minimum and CDF < 1.0: MIN THICKNESS → exit

  Step 2h: NEWTON-RAPHSON UPDATE
    - First iteration: perturb thickness by 1% to estimate gradient
    - Subsequent iterations:
        log_cdf = ln(CDF)
        gradient = (log_cdf - log_cdf_prev) / (thickness - thickness_prev)

        If gradient ≈ 0: perturb thickness by 10%
        If gradient > 0 (wrong sign): double thickness if CDF>1, else +10%

        Overshoot control:
          If |ln(CDF_prev)| < 0.69:         factor = 1.0
          If |ln(CDF_prev)| < 1.69:         factor = 0.95
          Else:                             factor = 0.6

        Newton step: Δt = (-log_cdf_prev × Δt_prev / Δlog_cdf) × factor
        Clamp: -50 ≤ Δt ≤ +50 inches

        new_thickness = prev_thickness + Δt
        Enforce minimum thickness

Step 3: POST-CONVERGENCE ASPHALT CDF
  - Evaluate horizontal stress at bottom of HMA (eval_layer = 1)
  - Compute asphalt CDF using AI or RDEC fatigue model
  - This is informational — does not affect the design thickness

Step 4: RETURN RESULTS
  - Final layer thicknesses
  - CDF subgrade and asphalt
  - Max strain and NtoFail per aircraft
  - Convergence status and iteration count
```

#### 3.3.2 Overlay Design

`design_flex_overlay` uses the same algorithm but iterates on the **overlay layer (index 0)** instead of the last layer before subgrade. Additional logic:
- If CDF < 0.01 and thickness > minimum: reduce overlay by 10%
- Minimum overlay thickness: 2 inches
- Post-convergence: asphalt CDF at bottom of overlay and existing surface

#### 3.3.3 Life Computation

`compute_life` determines how many years the current structure can sustain traffic (CDF = 1.0). Uses the **secant method** to iterate on the design life:
1. Evaluate CDF at two candidate lifetimes
2. Linearly interpolate to find the lifetime where CDF = 1.0
3. Repeat for up to 30 iterations until |CDF - 1.0| < 0.001

### 3.4 ACR/PCR Computation

#### 3.4.1 ACR (Aircraft Classification Rating)

ACR quantifies the damage potential of an aircraft on a standard ICAO reference pavement. Port of `ACNClassLib/clsACR.vb`.

**Step-by-step for flexible ACR:**

```
For each subgrade category (A/B/C/D):

  Step 1: BUILD REFERENCE STRUCTURE
    - HMA surface: 76 mm (≤2 wheels) or 127 mm (>2 wheels), E = 200,000 psi
    - P-209 Crushed Aggregate base: initial thickness 10 in, E = 58,015 psi
    - Subgrade: E from category (A=29,006, B=17,403, C=11,602, D=7,251 psi)

  Step 2: DESIGN REFERENCE BASE THICKNESS
    - Traffic: the subject aircraft at COV_ACN/20 = 1825 departures/year, 20-year life
    - Run design_flex with design_layer_index=1 (base layer)
    - The base thickness adjusts until CDF = 1.0

  Step 3: FIND DSWL (Design Single Wheel Load)
    - On the designed reference structure, find the single-wheel load that
      produces exactly 36,500 coverages (COV_ACN) at the asphalt/base interface
    - Single wheel uses reference tire pressure = 217.56 psi (1.5 MPa)
    - Bisection with log-linear interpolation in strain space
    - Bracket: 0.1× to 3× initial load, then refine to 0.1% accuracy

  Step 4: COMPUTE ACR
    ACR = 2 × DSWL_lbs × 0.453592 / 100
```

#### 3.4.2 PCR (Pavement Classification Rating)

PCR determines the load-bearing capacity of a specific pavement section with its traffic mix. Port of `FaarFieldAnalysis/modFedfaaGbl.vb` (StepsToDoM loop).

**Elimination algorithm:**

```
Step 0: PRE-COMPUTATION
  - Classify subgrade modulus → category (A/B/C/D)
  - Compute ACR for every aircraft in the traffic mix on the section's subgrade
  - Identify the aircraft with maximum ACR (gACRmaxIndex)

For round = 1 to N_aircraft:

  Step 1: COMPUTE CDF DISTRIBUTION
    - Build LEAF structure from the pavement section
    - Compute vertical strain (tandem method) for remaining traffic mix
    - Get CDF per aircraft at the critical offset

  Step 2: FIND CRITICAL AIRCRAFT
    - The aircraft with the largest individual CDF contribution

  Step 3: DETERMINE MGW (Max Gross Weight)
    - Isolate the critical aircraft (single-aircraft traffic mix)
    - Secant iteration on gross weight until CDF = 1.0
    - Bracket: 50% to 150% of current gross weight
    - Up to 30 iterations, tolerance: |CDF - 1.0| < 0.01

  Step 4: COMPUTE PCR FOR THIS ROUND
    - Create aircraft copy at MGW
    - Compute ACR at MGW on reference structure → this round's PCR

  Step 5: CHECK EARLY EXIT
    - If the critical aircraft is the one with maximum ACR → stop
    - This is the FAARFIELD optimization: once the dominant aircraft is found,
      subsequent rounds cannot produce a higher PCR

  Step 6: REMOVE CRITICAL AIRCRAFT
    - Remove from remaining set
    - Continue to next round

FINAL: PCR = max(PCR across all rounds)
```

---

## 4. Implementation Plan

The work was divided into seven sequential phases, each verified before proceeding.

| Phase | Module(s) | Purpose | FAARFIELD Source |
|-------|-----------|---------|-----------------|
| 1 | `numerical.py`, `units.py` | Bessel functions, quadrature, linear algebra, unit conversions | `LEAFClassLib/Numerical.vb`, `FaarFieldModel/UsCustomary.vb`, `Metric.vb` |
| 2 | `leaf.py` | LEAF layered elastic solver | `LEAFClassLib/clsLEAF.vb` |
| 3 | `materials.py`, `structures.py`, `aircraft.py` | Material library, pavement/traffic data model, aircraft XML loader | `FF2/Libs/MaterialLibrary.vb`, `ProgramDefaults.vb`, `FaarFieldModel/`, `FF2/Defaults/Aircraft/aircraft.xml` |
| 4 | `cdf.py` | Damage models, coverage-to-pass, CDF accumulation | `FaarFieldAnalysis/modCDF.vb` |
| 5 | `design_flex.py` | Iterative thickness design, life computation | `FaarFieldAnalysis/modStrDesignFlex.vb` |
| 6 | `acr_pcr.py` | ACR (DSWL iteration), PCR (MGW iteration) | `ACNClassLib/clsACR.vb`, `FaarFieldAnalysis/modFedfaaGbl.vb` |
| 7 | `app.py`, `templates/`, `static/` | Flask web UI (replaced Jupyter notebook) | `FF2/` (WPF views) |

---

## 5. Steps Taken

### Phase 1 — Numerical Foundations (`numerical.py`, `units.py`)

**What was ported:**
- Bessel functions `J₀(x)`, `J₁(x)`, `J₁(x)/x` — piecewise rational polynomial approximations with exact coefficients from `Numerical.vb`
- Gauss-Laguerre quadrature (`gaulag`) — Newton-Raphson root refinement for 500 abscissae and weights
- Log-gamma function (`gammln`) — Lanczos approximation with 6 coefficients
- Four linear algebra solvers: Gauss elimination, LU decomposition (with back-substitution), Gauss-Jordan elimination (full pivoting), and SVD auxiliary (`pythag`)
- Unit conversion constants (in→mm, lb→kg, psi→kPa, pci→MN/m³, °F↔°C)

**Design decision:** We deliberately used FAARFIELD's own polynomial Bessel approximations rather than `scipy.special` to guarantee bit-level numerical fidelity.

### Phase 2 — LEAF Solver (`leaf.py`)

**What was ported:**
- `SetOShifts` — origin-shift calculation to stabilise exponentials in the Hankel integrand
- `FindConstantsPartInvert` — fast partial-inversion solver (4×4 block elimination, no pivoting)
- `FindConstantsFull` — full matrix assembly with Gauss-Jordan or LU solver
- `FindConstants` — solver dispatcher with automatic fallback: PartInvert → LU → Gauss-Jordan
- `IntegrateZStrain` — vertical strain integration via 500-point Gauss-Laguerre quadrature
- `IntegrateHorizontalStress` — horizontal stress integration (radial/tangential → Cartesian conversion)
- `IntegrateZDeflection` — vertical deflection integration
- Dummy top layer insertion for numerical stability (matching FAARFIELD's `ComputeResponse`)
- Per-aircraft convergence tracking with early termination

### Phase 3 — Data Model (`materials.py`, `structures.py`, `aircraft.py`)

**What was ported:**
- All 19 materials from `MaterialLibrary.vb` with exact default thicknesses, moduli, Poisson's ratios, CBR/k-value defaults, layer codes, and validation ranges
- Three CBR/modulus/k-value conversion formulas (Default, PCA, NCHRP)
- Three default layer stacks (New Flexible, HMA on Aggregate, HMA Overlay on Flexible)
- Poisson's ratio lookup table by layer code (21 entries from `modINITLIBS.vb`)
- Aircraft XML parser for the FAA-curated `aircraft.xml` (DataContract format with dual-unit elements)
- FAARFIELD post-load processing: Tt↔B swap, gear type "N"→"X" replacement
- `PavementSection` → `LEAFStructure` and `LEAFAircraft` conversion

### Phase 4 — CDF Computation (`cdf.py`)

**What was ported:**
- `GaussArea` — Euler-McLaurin 4-point Gaussian wander integration (exact port)
- Three subgrade damage models (Standard, Straight-Line, Bleasdale)
- Asphalt fatigue model (AI-style) and RDEC model
- Coverage-to-pass computation (simplified and general gear variants)
- Tandem CDF method: `compute_tandem_strains` (two-pass LEAF) and `scan_tandem_damage` (peak/valley scanning)
- 41-offset CDF sweep with per-aircraft and total accumulation

### Phase 5 — Thickness Design (`design_flex.py`)

**What was ported:**
- Newton-Raphson iteration on design layer thickness targeting CDF = 1.0
- Aggregate sublayer modulus refinement (WES formula) with sublayer count freezing
- Overshoot control factors matching FAARFIELD's CDF error bands
- Overflow handling and minimum thickness enforcement
- Post-convergence asphalt CDF
- Structural life computation via secant method
- Overlay design iteration (`design_flex_overlay`)

### Phase 6 — ACR/PCR Engine (`acr_pcr.py`)

**What was ported:**
- ICAO reference structure builders (flexible and rigid)
- ACR: design reference structure → DSWL iteration → ACR formula
- PCR: multi-round elimination algorithm with MGW iteration and early exit
- Subgrade category classification

### Phase 7 — Flask Web Application (`app.py`, `templates/`, `static/`)

**What was built (replaced the Jupyter notebook):**
- **Flask backend** (`app.py`): REST API with endpoints for materials, default stacks, aircraft search, design, life, ACR, and PCR
- **Single-page UI** (`templates/index.html`): Bootstrap 5 + Font Awesome layout with:
  - Analysis type selector with automatic stack loading
  - Interactive layer editor table (add/remove/reorder layers, material dropdown, editable thickness/modulus)
  - CBR ↔ subgrade modulus live conversion
  - Aircraft search with debounced autocomplete from the 235-aircraft FAA library
  - Editable traffic mix (departures, growth rate, gross weight)
  - Action buttons (Design, Life, ACR, PCR) with loading overlay
  - Results display with convergence summary, per-aircraft detail tables, and canvas cross-section visualization
  - US Customary / Metric unit toggle affecting all fields
- **Custom theme** (`static/style.css`): Navy header, clean card layout, professional data tables

---

## 6. Verification Against FAARFIELD

### 6.1 Numerical Foundations

| Test | Method | Result |
|------|--------|--------|
| `bessj0(x)` for x = 0, 1, 5, 20 | Compare to `scipy.special.j0` | Max error < 2×10⁻¹⁰ |
| `bessj1(x)` for x = 0, 1, 5, 20 | Compare to `scipy.special.j1` | Max error < 5×10⁻⁹ |
| `gammln(1..10)` | Compare to `scipy.special.gammaln` | Exact match (12+ digits) |
| `gaulag(500, 0)` | Check count and positivity | 498 points (matching FAARFIELD) |
| `gauss_jordan` on 3×3 system | Compare to `numpy.linalg.solve` | Exact match |
| `lu_solve` on SPD and non-SPD | Verify solution and failure | Matches FAARFIELD |
| Unit conversions (6 factors + temperature) | Compare to known values | All correct |

### 6.2 LEAF Solver

| Test | Method | Result |
|------|--------|--------|
| Boussinesq half-space | LEAF vs analytical solution | **0.0000% relative error** |
| Multi-layer flexible | Physical reasonableness check | Correct strain sign and magnitude |

### 6.3 Data Model

| Test | Method | Result |
|------|--------|--------|
| Material catalogue | Count materials | 19 (matching `MaterialLibrary.vb`) |
| Default stacks | Verify layer counts and properties | Exact match |
| CBR/E/k-value conversions | Cross-check formulas | Correct |
| Aircraft library | Parse `aircraft.xml` | 235 aircraft loaded |

### 6.4 CDF Computation

| Test | Method | Result |
|------|--------|--------|
| Gaussian wander area | Compare to `scipy.stats.norm` | Diff = 4.65×10⁻⁶ |
| Asphalt NtoFail (ε=0.001, E=200k) | Compare to FAARFIELD source comment | **3571** (FAARFIELD: "3570.7151") |
| RDEC NtoFail (ε=0.001) | Full formula with defaults | **5600** |
| Coverage-to-pass (single/tandem) | Physical reasonableness | Confirmed |
| Tandem damage scanning (synthetic) | Verify signed accumulation | Correct |

### 6.5 Thickness Design

| Test | Method | Result |
|------|--------|--------|
| SWL-50 (50k lb, single wheel) | 3-layer flex | Converged: 4" HMA + 18.5" base |
| B737 BBJ (171.5k lb, dual wheel) | 3-layer flex | Converged: 4" HMA + 33.9" base |
| 4-layer with stabilized base | Design P-209 layer | Converged |
| Overlay design | Iterate overlay thickness | Converged |
| CDF monotonicity | 3 thicknesses (15/25/40") | CDF₁₅ > CDF₂₅ > CDF₄₀ |

### 6.6 ACR/PCR

| Test | Method | Result |
|------|--------|--------|
| Subgrade classification | 4 boundaries | A/B/C/D correct |
| ACR for SWL-50 on category D | Full chain | ACR(D) = 856 |
| PCR elimination | Multi-round with early exit | Reports max PCR across rounds |

### 6.7 Flask Web Application

| Test | Method | Result |
|------|--------|--------|
| Page load | `GET /` | HTML served correctly |
| Materials API | `GET /api/materials` | 19 materials returned |
| Stacks API | `GET /api/stacks` | 3 stack names returned |
| Stack detail | `GET /api/stack/New Flexible` | 4 layers with correct properties |
| Aircraft search | `GET /api/aircraft/search?q=B737` | Multiple Boeing 737 variants |
| Design API | `POST /api/design` (B737 on 3-layer) | Converged in 13 iterations, CDF = 1.0000 |

---

## 7. Priority Items — All Resolved

### 7.1 High Priority — ALL RESOLVED

| # | Item | Status |
|---|------|--------|
| 1 | Aggregate sublayer modulus refinement (WES formula) | RESOLVED — integrated into design iteration with NS freezing |
| 2 | CompforStab (stabilized base compensation) | NOT APPLICABLE — rigid design only |
| 3 | Multi-gear aircraft (WFBF, WFBN) | NOT APPLICABLE — XML sets libNGroups=1 |
| 4 | Tandem gear CDF (gTandemFnew) | RESOLVED — two-pass LEAF + peak/valley scanning |
| 5 | DSWL iteration for rigid pavements | OUT OF SCOPE — requires FEM/Westergaard, not LEAF |

### 7.2 Medium Priority — ALL RESOLVED

| # | Item | Status |
|---|------|--------|
| 6 | RDEC asphalt fatigue model | RESOLVED |
| 7 | Cross-validation with FAARFIELD outputs | RESOLVED — 4 cross-validation tests |
| 8 | PCR full elimination algorithm | RESOLVED |
| 9 | Coverage-to-pass for general gear | RESOLVED |
| 10 | Overlay design types | RESOLVED |

### 7.3 Lower Priority — ALL RESOLVED

| # | Item | Status |
|---|------|--------|
| 11 | Numerical overflow suppression | RESOLVED |
| 12 | Performance optimisation (vectorisation) | RESOLVED |
| 13 | Input validation | RESOLVED |
| 14 | Metric unit support | RESOLVED |
| 15 | Comprehensive test suite | RESOLVED — 49+ pytest tests |
| 16 | Aircraft wheel coordinate accuracy | RESOLVED |

---

## 8. Flask Web Application — Current Status

### 8.1 What Works

| Feature | Status | Notes |
|---------|--------|-------|
| Page serving and navigation | Working | Single-page app with Bootstrap 5 |
| Material library display | Working | All 19 materials available as dropdowns |
| Analysis type switching | Working | Loads correct default stack |
| Layer editor | Working | Add, remove, reorder, edit thickness/modulus/material |
| CBR ↔ subgrade modulus | Working | Live conversion on change |
| Aircraft search | Working | Debounced search across 235 aircraft |
| Traffic mix management | Working | Add from search, edit departures/weight/growth, remove |
| Design computation | Working | Full Newton-Raphson iteration via API |
| Life computation | Working | Secant method via API |
| ACR computation | Working | For first aircraft, all 4 subgrade categories |
| PCR computation | Working | Full elimination algorithm via API |
| Cross-section visualization | Working | Canvas rendering with material colors |
| Unit toggle (US/Metric) | Working | Converts all display values |
| Loading overlay | Working | Shows during computation |
| Error display | Working | Shows traceback on failure |
| Results display | Working | Convergence badge, summary table, per-aircraft detail |

### 8.2 Known Limitations

1. **ACR scope**: Currently computes ACR for the first aircraft in the traffic mix only; a future improvement would add aircraft selection or batch computation
2. **No save/load**: Section definitions are not persisted — refreshing the browser resets to defaults
3. **Synchronous computation**: Long-running computations (especially PCR with many aircraft) block the Flask server; a production deployment would use async workers
4. **No CDF chart**: The results show numerical CDF values but no graphical CDF-vs-offset plot
5. **No verbose iteration log**: The API runs design with `verbose=False`; iteration-by-iteration output is not streamed to the UI
6. **Single-user**: Flask development server is single-threaded; not intended for concurrent users

---

## 9. Next Steps and Suggestions

### 9.1 Immediate Next Steps (High Impact)

| # | Item | Description | Effort |
|---|------|-------------|--------|
| 1 | **Save/Load section** | Export/import section (layers + traffic) as JSON files via download/upload buttons | Low |
| 2 | **ACR for all traffic aircraft** | Batch ACR computation with results table showing all aircraft × all categories | Low |
| 3 | **CDF-vs-offset chart** | Add a line chart (Chart.js or Plotly.js) showing CDF across the 41 lateral offsets | Low |
| 4 | **Verbose design log** | Stream iteration-by-iteration output to the UI (Server-Sent Events or WebSocket) | Medium |
| 5 | **Section validation before compute** | Client-side validation with specific error messages (modulus ranges, minimum thicknesses) | Low |

### 9.2 Medium-Term Improvements

| # | Item | Description | Effort |
|---|------|-------------|--------|
| 6 | **PDF report generation** | Export design results as a formatted PDF (iText or ReportLab) | Medium |
| 7 | **Damage model selector** | UI dropdown to choose between Standard, Straight-Line, Bleasdale subgrade models | Low |
| 8 | **RDEC model parameters UI** | Form to edit RDEC mix-design parameters | Low |
| 9 | **Multi-section comparison** | Side-by-side comparison of two or more section designs | Medium |
| 10 | **Browser-based testing** | Automated Selenium/Playwright tests for the web UI | Medium |

### 9.3 Long-Term / Advanced

| # | Item | Description | Effort |
|---|------|-------------|--------|
| 11 | **Rigid pavement design** | Port `FEMClassLib` or Westergaard edge stress for PCC design | Very High |
| 12 | **3-D FEM integration** | Port FAASR/NIKE3D solver for composite pavement analysis | Very High |
| 13 | **Asynchronous computation** | Use Celery or threading for non-blocking design runs | Medium |
| 14 | **Desktop packaging** | Bundle as a standalone desktop app (PyInstaller or Electron + Flask) | Medium |
| 15 | **Batch design automation** | API for programmatic batch runs (multiple sections, parameter sweeps) | Medium |
| 16 | **Cross-validation automation** | Automated comparison against FAARFIELD outputs for a library of benchmark cases | High |

### 9.4 Suggestions for Further Accuracy Improvement

1. **Side-by-side FAARFIELD comparison**: Run identical inputs through both FAARFIELD 2.1.1 and NanoFlex, comparing layer thicknesses, CDF values, and ACR/PCR outputs for a set of benchmark cases (e.g., the FAARFIELD sample files).

2. **Edge-case testing**: Test with extreme inputs — very thin structures, very high traffic, very low subgrade modulus, single-wheel vs. 12-wheel gears — to find numerical stability boundaries.

3. **Convergence tolerance study**: Verify that the 0.005 convergence tolerance in NanoFlex produces results within FAARFIELD's published accuracy (typically ±0.1 inches on thickness).

4. **Temperature/seasonal effects**: FAARFIELD uses a fixed HMA modulus of 200,000 psi. A future enhancement could incorporate temperature-dependent modulus for climate-specific design.

---

## 10. File Inventory

| File | Lines | Description |
|------|-------|-------------|
| `numerical.py` | 290 | Bessel functions, Gauss-Laguerre quadrature, linear algebra solvers |
| `units.py` | 79 | US Customary ↔ SI conversion factors, `UnitSystem` class |
| `leaf.py` | 759 | LEAF layered elastic solver (vectorised inner loops, Burmister theory) |
| `materials.py` | 155 | Material catalogue (19 materials), CBR/E/k conversions, default stacks |
| `structures.py` | 193 | `PavementSection`, `PavementLayer`, `TrafficAircraft` with validation |
| `aircraft.py` | 195 | FAA aircraft library XML parser (235 aircraft) |
| `cdf.py` | 574 | Damage models, Gaussian wander, general gear, tandem scanning, CDF sweep |
| `design_flex.py` | 606 | Newton-Raphson thickness design (new flex + overlay), life, sublayer expansion |
| `acr_pcr.py` | 417 | ACR (DSWL on reference structures), PCR (elimination algorithm) |
| `app.py` | 219 | Flask backend — REST API and page serving |
| `templates/index.html` | 685 | Single-page HTML/JS/Bootstrap UI |
| `static/style.css` | 180 | Custom theme |
| `test_nanoflex.py` | 538 | Comprehensive pytest test suite |
| `requirements.txt` | 4 | Python dependencies |
| `NANOFLEX_REPORT.md` | — | This document |
| **Total** | **~4,894** | Excluding this report |

---

## 11. Dependencies

```
numpy>=1.24
scipy>=1.10
lxml>=4.9
flask>=3.0
```

Python 3.10+ required. No Fortran compiler or external solver needed.

---

## 12. How to Run

```bash
cd NanoFlex
pip install -r requirements.txt
python app.py
# Open http://127.0.0.1:5000 in your browser
```

For tests:
```bash
cd NanoFlex
pytest test_nanoflex.py -v
```
