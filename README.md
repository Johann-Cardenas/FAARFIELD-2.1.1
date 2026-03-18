# FAARFIELD 2.1.1 — Source (FAA)

> **Official source notice:** This repository contains the source code of **FAARFIELD 2.1.1** as published by the Federal Aviation Administration: https://www.airporttech.tc.faa.gov/Products/Airport-Safety-Papers-Publications/Airport-Safety-Detail/ArtMID/3682/ArticleID/2841/FAARFIELD-20
>
> This copy is provided for inspection, study, and archival purposes only. It does not represent a reproduction or transfer of copyright. All rights and ownership remain with the Federal Aviation Administration.

---

## Table of contents

- [Overview](#overview)
- [Repository structure](#repository-structure)
- [Core computational modules](#core-computational-modules)
  - [LEAF — Layered Elastic Analysis](#leaf--layered-elastic-analysis-leafclasslib)
  - [ACR — Aircraft Classification Rating](#acr--aircraft-classification-rating-acnclasslib)
  - [FEM — Finite-Element Solver](#fem--finite-element-solver-femclasslib)
  - [CDF — Cumulative Damage Factor](#cdf--cumulative-damage-factor-faarfieldanalysis)
  - [Thickness design](#thickness-design-faarfieldanalysis)
  - [ACN — Flexible & rigid computation](#acn--flexible--rigid-computation-faarfieldanalysis)
  - [H-51 edge-stress](#h-51-edge-stress-faarfieldanalysis)
  - [Failure models — rigid pavement](#failure-models--rigid-pavement-faarfieldanalysis)
- [Domain model & factories](#domain-model--factories-faarfieldmodel)
- [User interfaces](#user-interfaces)
- [Reports](#reports)
- [Software stack](#software-stack)
- [Quick start (build)](#quick-start-build)
- [Unit tests](#unit-tests)
- [Project dependency graph](#project-dependency-graph)

---

## Overview

**FAARFIELD** (FAA Rigid and Flexible Iterative Elastic Layered Design) is a VB.NET desktop application for airfield pavement thickness design and evaluation. It implements:

- **Layered elastic theory** (LEAF) for computing deflections, strains, and stresses in multi-layer flexible pavements.
- **3-D finite-element analysis** (FAASR / NIKE3D-based) for rigid and composite pavement response.
- **ACN/PCN classification** per ICAO standards for pavement strength reporting.
- **Cumulative damage factor (CDF)** integration for mixed-traffic thickness design.
- **Overlay design** for flexible-on-flexible, PCC-on-rigid, HMA-on-rigid, and unbonded overlays.

---

## Repository structure

```
FAARFIELD-2.1.1/
│
├── FAARFIELD.sln                        Root Visual Studio solution
├── README.md                            This file
│
│  ── Computational libraries ───────────────────────────────────
├── LEAFClassLib/                        Layered Elastic Analysis Foundation
│   ├── clsLEAF.vb                       LEAF solver (3 994 lines)
│   └── Numerical.vb                     Gaussian quadrature & matrix ops
│
├── ACNClassLib/                         ACN / PCN calculation engine
│   ├── clsACR.vb                        ACR calculator (4 711 lines)
│   ├── clsACNsub.vb                     ACN sub-calculations
│   ├── Numerical.vb                     Numerical helpers
│   ├── Set_Eval.vb                      Evaluation-point setup
│   ├── Z_Eval.vb                        Evaluation-zone calculations
│   └── TwoGears.vb                      Dual-gear configuration
│
├── FEMClassLib/                         Finite-element analysis (120+ files)
│   ├── FAASR/                           FAASR 3-D structural solver
│   │   ├── clsFAASR3D.vb               Main 3-D interface
│   │   ├── clsWinYield.vb              Yield-criterion calculations
│   │   └── clsSetStep1.vb / 2.vb       Load-stepping setup
│   ├── Solve/                           Core solver (96 files)
│   │   ├── clsSolveMain.vb              Solution driver
│   │   ├── clsLudcmp.vb                LU decomposition
│   │   ├── clsChsky06.vb               Cholesky factorisation
│   │   ├── clsBfgs.vb                  BFGS optimisation
│   │   ├── clsQuasin.vb                Quasi-Newton iteration
│   │   └── clsFstif0–2.vb              Stiffness-matrix formation
│   ├── Initial/                         Element initialisation & basis functions
│   ├── Input/                           Mesh, material & BC input (33 files)
│   └── PrintOut/                        Result output & Tecplot export
│
├── FAAMeshClassLib/                     Mesh generation utilities
│   ├── clsMesh.vb                       Mesh builder
│   └── ModFunction.vb                   Mesh helper functions
│
│  ── Application layer ─────────────────────────────────────────
├── FaarFieldAnalysis/                   Windows Forms analysis host
│   ├── clsDetailedReportData.vb         Data structures for Detailed Computation Report
│   ├── modCDF.vb                        CDF calculations (flexible & rigid)
│   ├── modStrDesignFlex.vb              Flexible pavement design
│   ├── modStrDesign13.vb                Structural design method 13
│   ├── modDesignP209.vb                 P-209 aggregate-base design
│   ├── modDesignRigid_Adj.vb            Rigid pavement design & overlay
│   ├── modFAILURE_MODEL_NP.vb          Rigid / overlay failure models
│   ├── modPCN_ThicknessDesign.vb        Iterative thickness optimisation
│   ├── modPCN_ACNMain.vb                ACN calculation orchestration
│   ├── modPCN_ACNflexICAO.vb            ICAO flexible ACN
│   ├── modPCN_ACNRigComp.vb             Rigid ACN computation
│   ├── modPCN_H51inVB.vb               H-51 edge-stress method
│   ├── modPCN_Alpha.vb                  Alpha-factor curves
│   ├── modPCN_Nonstandard.vb            Non-standard aircraft handling
│   ├── modAdvisoryCircularRq.vb         Advisory Circular requirements
│   └── FormPCN.vb                       Main results form
│
├── FaarFieldModel/                      Domain model & factories (60+ files)
│   ├── Interfaces/                      35 interface contracts (IAircraft, IMaterial …)
│   ├── Aircraft.vb, AirplaneInfo.vb     Aircraft data representation
│   ├── Section.vb, Material.vb          Pavement section & material props
│   ├── Thickness.vb, Modulus.vb         Dimensional properties
│   ├── DesignOptions.vb                 Design configuration
│   ├── FaarFieldModelFactory.vb         Factory for model creation
│   └── Lcca.vb                          Life-cycle cost analysis
│
├── ACClassLib/                          Aircraft base library (clsAC.vb, modAC.vb)
├── AMClassLib/                          Aircraft-matching & gear editing
│
│  ── User interfaces ───────────────────────────────────────────
├── FF2/                                 Modern WPF application (MVVM)
│   ├── Views/MainWindow.xaml            Primary window
│   ├── ViewModels/                      50+ view-model classes
│   ├── Converters/                      52 UI value converters
│   ├── Models/RunAnalysis.vb            Analysis execution model
│   ├── Libs/AircraftLibrary.vb          Aircraft database manager
│   ├── Libs/HtmlUtils.vb               HTML/PDF report generation helpers
│   ├── Resources/Reports.css            Embedded CSS for all reports
│   └── Defaults/Aircraft/aircraft.xml   Aircraft library (1.9 MB, XML)
│
│  ── Supporting ─────────────────────────────────────────────────
├── CreateSignedAircraftLibrary/         Utility: signed aircraft libraries
├── FAARFIELDUnitTests/                  MSTest unit tests
├── FAARFIELD.Installer/                 WiX installer project
├── lib/                                 Third-party assemblies (Telerik)
└── packages/                            NuGet packages
```

---

## Core computational modules

The sections below describe each computational engine, the key files to inspect, and the most important functions and data structures within them. Line numbers are approximate and refer to the current source.

---

### LEAF — Layered Elastic Analysis (`LEAFClassLib/`)

The LEAF solver computes the pavement response (deflections, strains, stresses) at arbitrary depths through a multi-layer elastic half-space subjected to circular surface loads. This is the fundamental engine behind flexible pavement design in FAARFIELD.

| File | Purpose |
|------|---------|
| [clsLEAF.vb](LEAFClassLib/clsLEAF.vb) | Full LEAF solver (3 994 lines) |
| [Numerical.vb](LEAFClassLib/Numerical.vb) | Gaussian quadrature integration & matrix ops |

**Key functions:**

| Function | Location | Description |
|----------|----------|-------------|
| `ComputeResponse()` | [clsLEAF.vb:213](LEAFClassLib/clsLEAF.vb#L213) | Primary entry point — computes the selected response type for one or more aircraft on a given pavement structure. Populates the `AllResps` array with all 23 response components. |
| `ComputeResponse2()` | [clsLEAF.vb:487](LEAFClassLib/clsLEAF.vb#L487) | Extended version with tandem-gear lateral-offset logic for CDF calculations. |

**Key data structures (defined at the top of `clsLEAF.vb`):**

| Structure | Line | Description |
|-----------|------|-------------|
| `LEAFAllResponses` | [clsLEAF.vb:65](LEAFClassLib/clsLEAF.vb#L65) | 23-field structure: 3 deflections (X/Y/Z), 6 strains, 6 stresses, 3 principal strains, 3 principal stresses, max shear, octahedral normal & shear. |
| `LEAFACParms` | [clsLEAF.vb:98](LEAFClassLib/clsLEAF.vb#L98) | Aircraft parameters passed to the solver: gear load, tire count, tire pressures & positions, evaluation points. |
| `LEAFStrParms` | [clsLEAF.vb:115](LEAFClassLib/clsLEAF.vb#L115) | Pavement structure: number of layers, thickness, modulus, Poisson's ratio, and interface parameters for each layer. |

**Constants:** `NOFF = 41` offsets for CDF calculations, `NNodesLong = 1800` longitudinal nodes for tandem analysis, `OFFSETINC = 10.0` inches between offsets.

**Mathematical basis:** The solver uses the Hankel transform over a multi-layer elastic half-space. Response at radial distance *r* and depth *z* takes the form ∫ K(α,z)·J_n(α·r)·α dα, evaluated via 500-point Gauss-Laguerre quadrature. A 1-inch dummy top layer of surface material is inserted for numerical stability. Each tire is modelled as uniform circular pressure with contact radius a = √(W_wheel/(π·p_tire)); superposition handles multiple tires.

---

### ACR — Aircraft Classification Rating (`ACNClassLib/`)

Computes Aircraft Classification Ratings (ACR/ACN) for flexible and rigid pavements, including overlay scenarios. This module drives the LEAF solver and applies coverage-to-pass and fatigue models.

| File | Purpose |
|------|---------|
| [clsACR.vb](ACNClassLib/clsACR.vb) | ACR/ACN calculator (4 711 lines) |
| [clsACNsub.vb](ACNClassLib/clsACNsub.vb) | Sub-calculation support routines |
| [Set_Eval.vb](ACNClassLib/Set_Eval.vb) | Evaluation-point configuration |
| [Z_Eval.vb](ACNClassLib/Z_Eval.vb) | Depth-zone evaluation loops |
| [TwoGears.vb](ACNClassLib/TwoGears.vb) | Dual-gear geometry handling |

**Key function:**

| Function | Location | Description |
|----------|----------|-------------|
| `Z_Evaluation_Loop999()` | [clsACR.vb:3541](ACNClassLib/clsACR.vb#L3541) | Main evaluation loop — iterates over depth zones for a given `PavementType` (Flexible or Rigid) and accumulates damage. |

**ACR/PCR algorithms:** ACR — for each subgrade category (A/B/C/D), design reference base thickness with subject aircraft traffic, then find DSWL (Design Single Wheel Load) producing 36,500 coverages; ACR = 2×DSWL_kg/100. PCR — elimination algorithm: each round finds critical aircraft, computes MGW (CDF=1.0), then ACR at MGW; early exit when critical aircraft has max ACR.

**Design-type constants** (defined at the top of [clsACR.vb](ACNClassLib/clsACR.vb)):

| Constant | Value | Meaning |
|----------|-------|---------|
| `NewFlex` | 1 | New flexible pavement |
| `FlexOnFlex` | 2 | Flexible overlay on flexible |
| `PCCOnFlex` | 3 | PCC overlay on flexible |
| `NewRigid` | 10 | New rigid pavement |
| `UnbondOnRigid` | 11 | Unbonded overlay on rigid |
| `PartBondOnRigid` | 12 | Partially bonded overlay on rigid |
| `FlexOnRigid` | 13 | Flexible overlay on rigid |

**Enumerations:** `PavementType` — `Flexible = 1`, `Rigid = 2`.

---

### FEM — Finite-Element Solver (`FEMClassLib/`)

A full 3-D finite-element engine (based on FAASR / NIKE3D heritage) used for rigid pavement response calculations. The solver supports 4-node and 8-node brick elements, non-linear material behaviour, and iterative solution algorithms.

| Subdirectory | Files | Purpose |
|--------------|-------|---------|
| `FAASR/` | 13 | 3-D structural interface, load stepping, yield criteria |
| `Solve/` | 96 | Matrix solution, stiffness assembly, stress recovery |
| `Initial/` | 10 | Basis functions (`Basis4`, `Basis8`), element initialisation |
| `Input/` | 33 | Mesh, material, boundary-condition input |
| `PrintOut/` | 17 | Result formatting, Tecplot export |
| `Com/` | 10 | Shared variables and communication |

**Key entry points:**

| Function | File | Description |
|----------|------|-------------|
| `solve()` | [clsSolveMain.vb:77](FEMClassLib/Solve/clsSolveMain.vb#L77) | Main FEM solution driver — orchestrates load stepping, stiffness assembly, matrix solution, and stress recovery. Accepts a `CancellationToken` for async execution. |
| `clsFAASR3D` | [FAASR/clsFAASR3D.vb](FEMClassLib/FAASR/clsFAASR3D.vb) | Top-level FAASR 3-D solver class — sets up the 3-D model, applies loads/constraints, and calls the solution sequence. |
| `clsWinYield` | [FAASR/clsWinYield.vb](FEMClassLib/FAASR/clsWinYield.vb) | Yield-criterion calculations for non-linear material response. |

**Solver algorithms available in `Solve/`:**

| Algorithm | File |
|-----------|------|
| LU decomposition | `clsLudcmp.vb`, `clsLufwbk.vb` |
| Cholesky factorisation | `clsChsky06.vb` |
| BFGS optimisation | `clsBfgs.vb` |
| Quasi-Newton | `clsQuasin.vb` |
| BiCG iterative | `clsBdbic1.vb`, `clsBdbic2.vb` |

---

### CDF — Cumulative Damage Factor (`FaarFieldAnalysis/`)

Computes cumulative damage factors for flexible, rigid, and overlay pavements under mixed traffic. Integrates over lateral wander using a Gaussian probability distribution.

| File | Purpose |
|------|---------|
| [modCDF.vb](FaarFieldAnalysis/modCDF.vb) | CDF calculation module |

**Key functions:**

| Function | Line | Description |
|----------|------|-------------|
| `LeafCDFFlex()` | [modCDF.vb:128](FaarFieldAnalysis/modCDF.vb#L128) | Computes the maximum CDF for a flexible pavement section; calls the LEAF solver at each offset and sums damage over all aircraft. |
| `CoverageToPassFlexible()` | [modCDF.vb:1346](FaarFieldAnalysis/modCDF.vb#L1346) | Converts coverages to passes for a given aircraft on flexible pavement. |
| `CoverageToPassRigidSingleAC()` | [modCDF.vb:2067](FaarFieldAnalysis/modCDF.vb#L2067) | Coverage-to-pass conversion for rigid pavement (single aircraft). |
| `GaussArea()` | [modCDF.vb:757](FaarFieldAnalysis/modCDF.vb#L757) | Gauss probability integral for lateral-wander distribution. |
| `FAAModulusThick()` | [modCDF.vb:806](FaarFieldAnalysis/modCDF.vb#L806) | Computes FAA modulus and sub-layer thickness for stabilised bases. |
| `FAAModulus()` | [modCDF.vb:1083](FaarFieldAnalysis/modCDF.vb#L1083) | Sets layer moduli following FAA standards. |
| `CompforStab()` | [modCDF.vb:706](FaarFieldAnalysis/modCDF.vb#L706) | Computes F-slope for stabilised layers. |

**Damage models:** Subgrade — Standard (AA/BB from E_subgrade), Straight-Line (dual-branch), Bleasdale (three-parameter). Asphalt — AI (Asphalt Institute) and RDEC fatigue models. **Tandem CDF (gTandemFnew):** Two-pass LEAF finds critical lateral offset, then 1800 longitudinal strain points; peak/valley scanning accumulates signed damage. CDF is swept over 41 lateral offsets (0–400 in, 10-in steps).

---

### Thickness design (`FaarFieldAnalysis/`)

Iterative pavement thickness design routines that drive LEAF and FEM to converge on a section that satisfies the target design life.

| File | Key function(s) | Description |
|------|-----------------|-------------|
| [modPCN_ThicknessDesign.vb](FaarFieldAnalysis/modPCN_ThicknessDesign.vb) | `ThicknessDesign()` (line 13), `InitVar()` (line 448) | Top-level iterative thickness optimiser; sets up the section and calls the appropriate design sub for flexible or rigid. |
| [modStrDesignFlex.vb](FaarFieldAnalysis/modStrDesignFlex.vb) | `LeafDesignFlex()` (line 57), `LeafDesignFlex2()` (line 63), `LeafDesignFlexOFlex()` (line 770) | Flexible pavement design — Newton-Raphson on design layer thickness targeting CDF = 1.0 (convergence |ln(CDF)| < 0.005). Aggregate sublayer modulus refinement (WES formula); sublayer counts frozen near convergence. Also handles flex-on-flex overlay. |
| [modDesignRigid_Adj.vb](FaarFieldAnalysis/modDesignRigid_Adj.vb) | `pre_DesignRigid_NP()` (line 69), `pre_DesignRigidOverlay_NP()` (line 502), `pre_LifeTotal_PCConRigid2014()` (line 815) | Rigid pavement and overlay thickness design with 2014 fatigue model. |
| [modDesignP209.vb](FaarFieldAnalysis/modDesignP209.vb) | `SetData_DesignBase_SubgadeCBR20_4Layers()` (line 73), `CheckMinThickness()` (line 15) | P-209 crushed-aggregate base design data and minimum-thickness checks. |
| [modFAILURE_MODEL_NP.vb](FaarFieldAnalysis/modFAILURE_MODEL_NP.vb) | `DesignRigid_NP()` (line 554), `DesignRigidOverlay_NP()` (line 39), `LifeTotal_PCConRigid2014()` (line 1204), `NtoFail1()` (line 3022) | Rigid pavement failure models — PCC fatigue life, HMA-on-rigid life, and the general `NtoFail` allowable-repetitions function. |

---

### ACN — Flexible & rigid computation (`FaarFieldAnalysis/`)

ICAO ACN/PCN computation routines for pavement strength reporting.

| File | Key function(s) | Description |
|------|-----------------|-------------|
| [modPCN_ACNflexICAO.vb](FaarFieldAnalysis/modPCN_ACNflexICAO.vb) | `ACNFlexComp()` (line 229), `AlphaCurves()` (line 170), `AlphaFactorFromCurve()` (line 213) | ICAO flexible ACN: alpha-factor curves, spline interpolation, and the main `ACNFlexComp` convergence loop. |
| [modPCN_ACNRigComp.vb](FaarFieldAnalysis/modPCN_ACNRigComp.vb) | `ACNRigComp()` (line 79), `CACN()` (line 670) | Rigid ACN: iterates slab thickness to match the target ACN using Westergaard-type response. |

---

### H-51 edge-stress (`FaarFieldAnalysis/`)

Implements the FAA H-51 method (Westergaard edge-stress) for computing the critical stress in rigid pavement slabs under complex gear configurations.

| File | Key function(s) | Description |
|------|-----------------|-------------|
| [modPCN_H51inVB.vb](FaarFieldAnalysis/modPCN_H51inVB.vb) | `EdgeStress()` (line 77), `GEOM()` (line 215), `PROBRD()` (line 533), `CURVE()` (line 796) | `EdgeStress` is the main entry point; `GEOM` sets up tire geometry; `PROBRD` computes the Westergaard response; `CURVE` interpolates load-response curves. |

---

### Failure models — rigid pavement (`FaarFieldAnalysis/`)

Separate from the CDF module, these routines implement the FAA's rigid pavement and overlay fatigue failure models including the 2014 calibration, FEM-based stress computation, and overlay-thickness adjustment.

See [modFAILURE_MODEL_NP.vb](FaarFieldAnalysis/modFAILURE_MODEL_NP.vb) and [modDesignRigid_Adj.vb](FaarFieldAnalysis/modDesignRigid_Adj.vb) in the thickness-design table above.

---

## Domain model & factories (`FaarFieldModel/`)

The `FaarFieldModel` project defines the data layer used by both user interfaces and the analysis engine. It follows a factory pattern with 35 interface contracts.

| File | Description |
|------|-------------|
| [Aircraft.vb](FaarFieldModel/Aircraft.vb) | Aircraft data model |
| [AirplaneInfo.vb](FaarFieldModel/AirplaneInfo.vb) | Detailed aircraft info (gear geometry, gross weight, tire pressure) |
| [Section.vb](FaarFieldModel/Section.vb) | Pavement section definition |
| [Material.vb](FaarFieldModel/Material.vb) | Material properties (modulus, Poisson's ratio, layer code) |
| [Thickness.vb](FaarFieldModel/Thickness.vb) | Layer thickness with validation |
| [Modulus.vb](FaarFieldModel/Modulus.vb) | Elastic modulus with validation |
| [DesignOptions.vb](FaarFieldModel/DesignOptions.vb) | Design configuration (analysis type, solver options) |
| [FaarFieldModelFactory.vb](FaarFieldModel/FaarFieldModelFactory.vb) | Factory for creating validated model objects |
| [AnalysisType.vb](FaarFieldModel/AnalysisType.vb) | Enumeration: Flexible / Rigid / Overlay |
| [Lcca.vb](FaarFieldModel/Lcca.vb) | Life-cycle cost analysis data |

Validation classes (`ThicknessValidationRange.vb`, `ModulusValidationRange.vb`, etc.) enforce FAA-standard limits on all inputs.

---

## User interfaces

FAARFIELD ships with two UI front-ends:

| Project | Technology | Entry point |
|---------|-----------|-------------|
| **FF2/** | WPF + MVVM | [Application.xaml](FF2/Application.xaml) / [MainWindow.xaml](FF2/Views/MainWindow.xaml) |
| **FaarFieldAnalysis/** | Windows Forms | [FormPCN.vb](FaarFieldAnalysis/FormPCN.vb) |

The **FF2** (WPF) project is the primary user interface and includes:
- 50+ view-model classes in `ViewModels/`
- 52 value converters in `Converters/` for unit systems, visibility, and validation
- An embedded aircraft library at `Defaults/Aircraft/aircraft.xml` (1.9 MB)
- Material imagery in `Defaults/Materials/`
- Multiple report types (Structure, CDF Graph, PCR, Airport Master Record, Summary, and **Detailed Computation Report**)

---

## Reports

FAARFIELD generates several report types, accessible from the section tree in the FF2 UI. Each report can be viewed in the built-in HTML viewer and exported to PDF.

| Report | Description |
|--------|-------------|
| **Summary Report** | Job-level overview of all structures and design results |
| **Structure Report** | Pavement cross-section, layer properties, and analysis summary per structure |
| **CDF Graph** | Cumulative damage factor vs. lateral offset visualization |
| **PCR Report** | Pavement Classification Rating results and aircraft-by-aircraft breakdown |
| **PCR Graph** | PCR vs. aircraft chart |
| **Airport Master Record** | ICAO-compliant pavement strength record |
| **Detailed Computation Report** | Full computational trace for flexible pavement design (see below) |

### Detailed Computation Report

The **Detailed Computation Report** is a comprehensive technical report that documents the full computational workflow of thickness design, CDF accumulation, and ACR/PCR analysis. It is populated automatically when you run **Thickness Design** or **Life Analysis** on a flexible pavement section. Access it from the section tree under **Reports → Detailed Computation Report**.

**Key features:**

- **Section A — Pavement Structure Summary:** Design layers and expanded sublayer structure (after modulus adjustment), evaluation depth at subgrade.
- **Section B — Design Equations:** Rendered equations for the subgrade strain failure model (AA, BB, N_fail), CDF formula, coverage-to-pass (Gaussian wander model), and convergence criterion.
- **Section C — Coverage-to-Pass (C/P) Concept:** Educational diagram explaining how C/P is computed from Gaussian lateral wander (σ = 30.435 in.), multi-wheel superposition, and the 41 evaluation strips.
- **Section D — Fatigue Characterization:** Subgrade fatigue curve plot with aircraft operating points, N_fail, repetitions, and model parameters.
- **Section E — Per-Aircraft Detailed Breakdown:** For each aircraft: pavement cross-section with tire projection, gear parameters (load, tire pressure, contact area, tandem spacing), CDF vs. offset chart, wheel-level C/P decomposition, and a step-by-step computation walkthrough.
- **Section F — C/P Distribution:** C/P ratio vs. lateral offset for all aircraft.
- **Section G — CDF Sweep Table:** Full 41-offset table of C/P and CDF per aircraft and total CDF; critical offset identification.
- **Section H — CDF Distribution Across Pavement Width:** Composite CDF chart and contribution summary at the critical strip.
- **Section I — Newton-Raphson Convergence:** Dual-axis convergence plot (|ln(CDF)| and thickness vs. iteration), iteration log, and convergence summary.
- **Section J — ACR Details:** Reference structure, designed base thickness, DSWL iteration log, and final ACR per subgrade category.
- **Section K — PCR Elimination Rounds:** Critical aircraft per round, MGW iteration, round PCR, and early-exit flag.
- **Section L — ACR vs. Damage Per Departure:** Chart relating ACR to normalized CDF per departure; bubble size proportional to annual departures.

The report data is captured during analysis by `clsDetailedReportData` (in `FaarFieldAnalysis/clsDetailedReportData.vb`) and rendered in `MainWindowViewModel.refreshDetailedReport()`.

**Rendering architecture:**

The report is generated as a self-contained HTML string with inline base64-encoded PNG charts. The pipeline is:

1. **Data capture** — During thickness design or life analysis, intermediate computational values are stored in `clsDetailedReportData` (aircraft strains, CDF arrays, iteration logs, ACR/PCR results).
2. **HTML generation** — `refreshDetailedReport()` in `MainWindowViewModel.vb` (~line 8336) builds the complete HTML document using `HtmlUtils` helper methods (`wrap_p`, `wrap_table`, `wrap_bmp_img`, etc.).
3. **Chart rendering** — 12 private GDI+ drawing functions produce `System.Drawing.Bitmap` objects at 2x or 3x resolution via `ScaleTransform()`, then downscale with bicubic interpolation (`SupersampleBitmap()`). Each bitmap is PNG-encoded to base64 and embedded inline.
4. **Styling** — `Reports.css` (embedded resource) provides all styling including dashboard cards, table-of-contents, section headers, equation boxes, and chart containers.
5. **Display** — The HTML string is bound to a WPF `WebBrowser` control via `BrowserBehavior` (attached behavior). PDF export uses SelectPdf's `HtmlToPdf` converter.

**Chart functions** (all in `MainWindowViewModel.vb`):

| Function | Description |
|----------|-------------|
| `DrawEquationImage()` | Equation text as styled bitmap (3x supersample, Cambria Math font) |
| `DrawSingleAircraftCDFChart()` | CDF vs. lateral offset for one aircraft |
| `DrawCompositeCDFChart()` | All aircraft CDF curves + cumulative CDF overlay |
| `DrawPavementCrossSection()` | Layer stack diagram + tire stress projection |
| `DrawFatigueCurve()` | Log-log strain vs. allowable repetitions with model curve |
| `DrawConvergencePlot()` | Dual-axis convergence history (error + thickness vs. iteration) |
| `DrawCoveragePlot()` | C/P ratio distribution for all aircraft |
| `DrawCoverageConceptDiagram()` | 4-panel educational diagram on Gaussian wander C/P computation |
| `DrawWheelCPVisualization()` | Per-aircraft C/P with inferred wheel-level contributions |
| `DrawACRDamageChart()` | ACR vs. CDF-per-departure bubble chart |
| `DrawCDFContributionChart()` | Horizontal bar chart of CDF % contribution per aircraft |
| `DrawLifeRatioChart()` | Diverging bar chart of fatigue life reserve ratio |

---

## Software stack

| Component | Detail |
|-----------|--------|
| **Language** | Visual Basic .NET (VB.NET) |
| **Framework** | .NET Framework 4.8 |
| **IDE / Build** | Visual Studio 2019+ / MSBuild (ToolsVersion 15+) |
| **UI** | WPF (FF2) and Windows Forms (FaarFieldAnalysis) |
| **Third-party** | Telerik UI for WPF (`lib/RCWPF/`), iTextSharp (PDF), Select.HtmlToPdf |
| **Testing** | MSTest 2.1.1 |
| **Installer** | WiX Toolset (`FAARFIELD.Installer/`) |

---

## Quick start (build)

1. **Prerequisites** — Visual Studio 2019 or later with the **.NET desktop development** workload and .NET Framework 4.8 targeting pack.
2. **Open** `FAARFIELD.sln` in Visual Studio.
3. **Restore NuGet packages** (Visual Studio does this automatically on first build).
4. **Build** the solution (`Ctrl+Shift+B`). The startup project is **FF2**.
5. **Run** — press `F5` to launch the WPF application.

> The `lib/` folder contains pre-packaged Telerik assemblies; no separate Telerik license is needed to build.

---

## Unit tests

Test files live in `FAARFIELDUnitTests/` and use the MSTest framework.

| File | Scope |
|------|-------|
| [AircraftUnitTests.vb](FAARFIELDUnitTests/AircraftUnitTests.vb) | Aircraft model validation |
| [UnitTest1.vb](FAARFIELDUnitTests/UnitTest1.vb) | General functionality checks |

Run tests from the Visual Studio Test Explorer or via `dotnet test` (if the .NET CLI is configured for .NET Framework projects).

---

## Project dependency graph

```
FAARFIELD.sln
│
├── FF2  (WPF UI)
│   └──► FaarFieldModel
│
├── FaarFieldAnalysis  (WinForms UI)
│   └──► AMClassLib, ACNClassLib, LEAFClassLib, FEMClassLib
│
├── FaarFieldModel  (domain model)
│
├── ACNClassLib  (ACN/PCN engine)
│   └──► LEAFClassLib, FEMClassLib
│
├── LEAFClassLib  (layered elastic solver)
│
├── FEMClassLib  (finite-element solver)
│
├── FAAMeshClassLib  (mesh utilities)
│
├── ACClassLib  (aircraft base library)
│
├── AMClassLib  (aircraft-matching / gear editing)
│
├── CreateSignedAircraftLibrary  (signing utility)
│
├── FAARFIELDUnitTests  (tests)
│   └──► FaarFieldModel
│
└── FAARFIELD.Installer  (WiX packaging)
```

---

## Modifications from original FAARFIELD 2.1.1 source

This section documents all changes made to this codebase relative to the unmodified FAA-published FAARFIELD 2.1.1 source code.

### 1. Detailed Computation Report (new feature)

Added a comprehensive HTML report that documents the full computational trace of flexible pavement thickness design. The report is generated automatically after running Thickness Design or Life Analysis and is accessible from the section tree under **Reports > Detailed Computation Report**.

**Files added:**
- `FaarFieldAnalysis/clsDetailedReportData.vb` — Data collection classes populated during analysis (aircraft details, iteration records, CDF sweep data, sublayer info, ACR/PCR results).

**Files modified:**
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `refreshDetailedReport()` and 12 GDI+ chart-drawing functions for inline bitmap rendering.
- `FF2/ViewModels/DetailedReportViewModel.vb` — Tree view item that triggers report generation.
- `FF2/Libs/HtmlUtils.vb` — HTML helper class for report assembly and PDF export.
- `FF2/Resources/Reports.css` — Stylesheet for report layout (dashboard cards, tables, chart containers, equation blocks, step lists).
- `FF2/Converters/BrowserBehavior.vb` — Attached behavior binding the HTML string to the WPF WebBrowser control.
- `FaarFieldAnalysis/modCDF.vb` — Instrumented to populate `clsDetailedReportData` during CDF computation.
- `FaarFieldAnalysis/modStrDesignFlex.vb` — Instrumented to capture iteration records and sublayer data during flexible design.

The report includes 15 sections (A through L) covering pavement structure, design equations, coverage-to-pass concepts, fatigue characterization, per-aircraft breakdowns, CDF sweep tables, convergence history, and ACR/PCR details. All charts are rendered as supersampled GDI+ bitmaps encoded as inline base64 PNG.

### 2. Annual departure limit increased to 500,000

Raised the maximum allowable annual departures per aircraft from 100,000 to 500,000.

**Files modified:**
- `FF2/ValidationRules/AnnualDepartureValidationRule.vb` — Changed validation cap from 100,000 to 500,000.
- `FF2/Models/AircraftList.vb` — Changed error-list validation cap from 100,000 to 500,000.
- `FF2/ViewModels/MainWindowViewModel.vb` — Changed MessageBox validation cap from 100,000 to 500,000.

The original 100,000 limit was an engineering reasonableness bound with no regulatory citation in the code. This change allows analysis of higher-traffic scenarios. All other input limits (design life 1–50 years per AC 150/5320-6D §302.a, growth rate ±10%) remain unchanged.

### 3. Native HTML Report with SVG Charts ("Open in Browser")

Added an **Open in Browser** button next to the existing "Save As PDF" button on the Detailed Computation Report pane. This generates a **completely independent HTML report** (not a re-export of the PDF) using a parallel rendering pipeline with native browser technologies:

- **Inline SVG charts** — All visualizations (fatigue curve, convergence plot, C/P distribution, composite CDF, CDF contribution bars, life ratio bars, ACR bubble chart, per-aircraft CDF, pavement cross-section, C/P concept diagram) are rendered as scalable vector graphics directly in the HTML, replacing the GDI+ bitmap pipeline. Charts scale perfectly at any zoom level and look crisp in print.
- **Bleasdale piecewise visualization** — When the Bleasdale subgrade model is used, the fatigue curve SVG shows three color-coded zones (endurance limit, Bleasdale curve, power-law tail) with transition markers, dual-color curve segments, and an equation info box.
- **Clean Unicode** — HTML entities (`&epsilon;`, `&sigma;`, `&times;`, subscripts/superscripts) render natively in any modern browser, eliminating the GDI+ text-to-bitmap Unicode corruption.
- **Modern CSS** — CSS Grid dashboard, CSS variables for theming, responsive layout, professional typography, hover effects, print-optimized styles.
- **Clickable table of contents** — All 12 sections (A–L) have anchor links for instant navigation.
- **Collapsible data tables** — Large per-offset tables use HTML `<details>` elements to keep the report compact.
- **Self-contained** — The output `.html` file has zero external dependencies (all CSS and SVG are inline).

The existing PDF report (GDI+ bitmaps → SelectPdf) remains completely untouched.

**Files added:**
- `FF2/Libs/HtmlReportGenerator.vb` — New `HtmlReportGenerator` class (~1400 lines) with `Generate()` method, 10 SVG chart functions, CSS stylesheet, and helper utilities.

**Files modified:**
- `FF2/Libs/HtmlUtils.vb` — Added `HtmlToFile()` method (saves HTML with UTF-8 encoding, launches default browser).
- `FF2/Views/MainWindow.xaml` — Added "Open in Browser" button bound to `OnSectionReportOpenHtml` command.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `OnSectionReportOpenHtml` command property and `SectionReportOpenHtml` handler. The handler calls `HtmlReportGenerator.Generate()` (not `refreshDetailedReport()`).
- `FF2/FF2.vbproj` — Added `HtmlReportGenerator.vb` to compilation.

### 4. CM Report rename and gear configuration visualization

Renamed **"Detailed Computation Report"** to **"CM Report"** (Computational Mechanics) across all user-facing strings. Internal property names (`DetailedReportHtml`, `DetailedReportIsHidden`, `SerializationTag="DetailedReport"`) remain unchanged for serialization compatibility.

**UI changes:**
- Report tab now displays "CM Report" with a SemiBold font, an info icon (ⓘ in FAA blue), and a multi-line tooltip explaining the report's purpose.
- PDF and HTML default filenames use "CM Report" instead of "Detailed Computation Report".

**Gear configuration visualization** added to Section E of both the PDF (GDI+ bitmap) and HTML (native SVG) report pipelines:
- Plan view of wheel positions with tire contact patches drawn as semi-transparent circles.
- CDF offset strips (41 dashed vertical lines at 10-inch intervals) with the critical strip highlighted in red.
- Gaussian lateral wander overlay (σ=30.435 in.) as a translucent filled bell curve.
- Dimension annotations for dual spacing, tandem spacing, and contact area.
- Coordinate labels at each wheel showing (X, Y) in the gear coordinate system.

**Data model extensions** (`clsAircraftDetail` in `clsDetailedReportData.vb`):
- `WheelX()`, `WheelY()` — lateral/longitudinal position of each wheel (from `libTX`/`libTY`)
- `NWheels` — number of tires (from `libNTires`)
- `DualSpacing`, `GearSpacing` — gear geometry (from `libB`/`libTG`)
- Fixed population of `TandemSpacing` (from `libTS`) and `ContactArea` (computed from gross load, tire count, contact pressure)

**Files modified:**
- `FaarFieldAnalysis/clsDetailedReportData.vb` — Added 5 fields to `clsAircraftDetail`, updated doc comment.
- `FaarFieldAnalysis/modCDF.vb` — Populated `WheelX`/`WheelY`, `DualSpacing`, `GearSpacing`, `TandemSpacing`, `ContactArea` during CDF computation.
- `FF2/Views/MainWindow.xaml` — Replaced RadPane `Header` attribute with StackPanel content (CM Report + ⓘ icon + tooltip).
- `FF2/ViewModels/DetailedReportViewModel.vb` — Renamed tree node to "CM Report".
- `FF2/ViewModels/MainWindowViewModel.vb` — Renamed user-facing strings (5 locations), added `DrawGearConfiguration()` (900×600 GDI+, 2x supersampling), inserted gear chart in Section E.
- `FF2/Libs/HtmlReportGenerator.vb` — Renamed user-facing strings (2 locations), added `AppendGearConfigSVG()`, inserted SVG gear chart in Section E.

### 5. About window

Added an **About** button to the main toolbar (left of the Help button) that opens a custom borderless dialog window with a teal gradient header. The window provides:

- **Beta disclosure** — Prominent amber notice stating this is a customized beta version, not the official FAA release, with guidance that results should be independently verified.
- **Credits** — Customization by Johann Cardenas for computational mechanics research; original FAA authors acknowledged (Dr. Izydor Kawa, Y. G. Chen, Qiang Wang, Kairat Assemblayev).
- **License** — Permissive use with full liability disclaimer; original FAARFIELD IP remains with the FAA.
- **Version display** — Shows `v2.1.1-CM` and the build date from `BuildDate.txt`.
- **Draggable borderless chrome** — Custom window style with drop shadow, rounded corners, and teal close button.

**Files added:**
- `FF2/Views/AboutWindow.xaml` — WPF window with teal gradient header, beta disclosure, credits, license, and styled close button.
- `FF2/Views/AboutWindow.xaml.vb` — Code-behind: build date loading, close handler, drag support.

**Files modified:**
- `FF2/Application.xaml` — Added `AboutButton` resource (teal circle icon with "About" text).
- `FF2/Views/MainWindow.xaml` — Added About button to toolbar with `ItemAlignment="Right"`.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `OnAbout_Command` property and `OnAbout` handler.
- `FF2/FF2.vbproj` — Added `AboutWindow.xaml` and `AboutWindow.xaml.vb` to compilation.

### 6. Analysis progress banner and auto-tab switching

Added a visual progress banner to the **Status tab** in the Structure window's right panel. When the user clicks **Run** (for any analysis module — Thickness Design, Life, PCR, etc.), the UI now:

1. **Automatically switches** to the Status tab regardless of which tab (Status/Gear/Structure) was active. (The `SelectedTabIndex = 0` switch already existed in `RunAnalysis.RunOrCancel()`; no change needed.)
2. **Shows an indeterminate progress bar** in a blue-themed banner with an hourglass icon, the elapsed time (HH:MM:SS in Consolas), and a sub-text line that updates every second with the latest analysis status from `MessageText`.
3. **Transitions to a green "completed" banner** when the analysis finishes — full progress bar, check mark icon, and "Completed in HH:MM:SS — results are ready for review."
4. **Shows an amber "canceled" banner** if the user clicks Cancel during analysis.

The legacy overlay TextBoxes (RunningTime, StopWatch, CrossSection) are preserved in the XAML at zero opacity for data-binding compatibility but are no longer visually displayed — their information is now integrated into the progress banner.

**Files modified:**
- `FF2/Views/MainWindow.xaml` — Redesigned Status tab with Grid layout, progress banner (icon + status text + elapsed timer + ProgressBar + sub-text), and repositioned message display. Legacy TextBoxes hidden at 1x1px with Opacity=0.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added 11 progress banner properties (`ProgressBannerVisibility`, `ProgressBannerBackground`, `ProgressIcon`, `ProgressStatusText`, `ProgressTextBrush`, `ProgressIsIndeterminate`, `ProgressValue`, `ProgressBarBrush`, `ProgressSubText`, `ProgressSubTextVisibility`, `ProgressSubTextBrush`) and 4 helper methods (`ShowProgressRunning`, `ShowProgressCompleted`, `ShowProgressCanceled`, `HideProgressBanner`). Modified `RunButton_Click` to activate running/canceled states and `dispatcherTimer_Tick` to update sub-text during analysis and show completion state.

### 7. Gear tab visualization modernization

Rewrote the `PaintGear()` method in `FF2/Libs/ModuleDrawProfile.vb` with modern GDI+ rendering techniques. The gear configuration drawing in the right-panel Gear tab now features:

- **Anti-aliased rendering** with ClearType text hints and high-quality bicubic interpolation.
- **Teal gradient header bar** (#004D40 → #00796B) displaying the aircraft name, gear type badge (e.g., "Dual Tandem"), and wheel/tire summary.
- **Dot grid background** on warm gray (#FCFCFA) for a clean, modern appearance.
- **Modernized axes** with labeled "Lateral" / "Longitudinal" headings and unit annotations (in. or mm).
- **Rounded-rectangle tire imprints** with gradient fills (deep gray → medium gray), replacing the original flat rectangles.
- **Numbered wheel labels** (white text centered on each tire) for easy identification.
- **Coordinate annotations** in Consolas 6.5pt showing (X, Y) values at each wheel position.
- **Mirrored wheels** rendered with softer transparency to visually distinguish the mirror plane.
- **Evaluation point markers** as red dots with stroke, preserving original evaluation-point display.
- **Dimension annotations** with dashed lines for dual spacing (B) and tandem spacing (Ts).
- **Legend box** (bottom-left) with entries for tire imprint, mirrored wheel, and evaluation point.

The original coordinate transform logic is fully preserved — scale, scaleRatio, ScaledPictureBox, B-52/A380 special cases, US Customary/Metric branching, and mirrored-wheel rendering all function identically.

**Files modified:**
- `FF2/Libs/ModuleDrawProfile.vb` — Complete rewrite of `PaintGear()` (lines 44–370). Original `PaintUserDefinedGear()` unchanged.

### 8. Aggregate sublayer modulus documentation in CM Report

Added a comprehensive explanation of the **unbound aggregate sublayering procedure** to Section A of both the PDF (GDI+ bitmap) and HTML (native SVG) CM Report pipelines. When aggregate base (P-209) or subbase (P-154) layers are present, the report now shows:

- **Explanation note** — Why aggregate layers don't have a single fixed modulus and how FAARFIELD subdivides them into sublayers with depth-dependent moduli computed bottom-up.
- **Mathematical formula** — The empirical sublayer modulus reduction formula: E_i = E_{i-1} × (f1 - f2), where f1 and f2 depend on sublayer thickness and the C/D coefficients.
- **Parameters table** — C, D coefficients, modulus of layer below, and sublayer count for each aggregate type present (P-209: C=10.52, D=2.0; P-154: C=6.88, D=1.56).
- **Sublayer detail tables** — Individual sublayer thickness and computed modulus, showing the modulus gradient from bottom to top, with the reference "layer below" modulus highlighted.
- **Modulus-depth profile chart** — Visual step chart with left panel showing stacked layer bars (colored by type, aggregate layers shaded by modulus magnitude) and right panel showing the teal step-line tracing modulus at each depth. Aggregate sublayers highlighted with translucent fill.

**Data model extensions** (`clsSublayerData` in `clsDetailedReportData.vb`):
- `HasAggregateSublayers`, `BaseCoeffC`, `BaseCoeffD`, `SubbaseCoeffC`, `SubbaseCoeffD` — Sublayering formula coefficients.
- `BaseModUnder`, `SubbaseModUnder` — Modulus of the layer below each aggregate layer.
- `BaseSublayerCount`, `SubbaseSublayerCount` — Number of sublayers per aggregate type.
- `BaseSublayers`, `SubbaseSublayers` — `List(Of clsLayerInfo)` with individual sublayer thickness/modulus.

**Files modified:**
- `FaarFieldAnalysis/clsDetailedReportData.vb` — Extended `clsSublayerData` with 10 new fields for sublayering parameters and per-sublayer data.
- `FaarFieldAnalysis/modStrDesignFlex.vb` — Populated sublayering parameters (C, D, ModUnder, sublayer lists) from `BaseMod()`, `SubbaseMod()`, `TSS_P209()`, `TSS_P154()` during report data capture.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `DrawModulusDepthProfile()` (850×500 GDI+, 2x supersampling). Inserted explanation, formula, parameter tables, sublayer detail tables, and chart in Section A of the PDF report.
- `FF2/Libs/HtmlReportGenerator.vb` — Added `AppendSublayerModulusSection()`, `AppendModulusDepthSVG()`, `SvgDepthToY()`, `SvgModToX()`, `IsAggregateSublayer()`, `Fmt()`. Inserted explanation, formula, parameter tables, sublayer detail tables, and SVG chart in Section A of the HTML report. Added CSS for `.sublayer-modulus-section`, `.modulus-depth-svg`, `.sublayer-main-eq`, `.sublayer-detail`, `.ref-row`, `.mod-label`, `.fig-caption`.

### 9. Gross weight guardrail override

Changed the gross weight validation in `FaarFieldModel/AirplaneInfo.vb` (line ~505) from a hard block to a user-overridable warning. Previously, entering a gross taxi weight outside the 0.6×–1.25× default range triggered a `MessageBox.Show` that silently reverted the value. Now the dialog shows:

- The allowed range (min/max in both lb and kg)
- The entered value that is out of range
- **Yes** to override the limit and continue with the entered value (for research purposes)
- **No** to revert to the previous value

**Files modified:**
- `FaarFieldModel/AirplaneInfo.vb` — Replaced two `MessageBox.Show` + hard revert blocks with a single `MessageBoxButtons.YesNo` dialog that allows the user to proceed.

---

### 10. Asphalt (HMA) CDF documentation in CM Report

Added asphalt layer fatigue characterization to both the PDF (GDI+) and HTML (SVG) CM Report pipelines. FAARFIELD computes asphalt CDF in parallel with subgrade CDF using horizontal tensile strain at the bottom of the HMA layer. Two fatigue models are supported:

- **RDEC model** — Rate of Dissipated Energy Change: `PV = 44.422 × ε^5.14 × (E×0.0068948)^2.993 × VP^1.85 × GP^(-0.4063)`; `N_fail = 0.4801 × PV^(-0.90074)`. Uses mix-specific volumetric and gradation parameters (air voids, asphalt content, PNMS, PPCS, P200).
- **AI model** — Asphalt Institute: `AA = 2.68 - 5.0×log10(ε)`; `BB = 2.665×log10(E_asp)`; `N_fail = 10^(AA-BB)`.

The report now includes in Section D (Fatigue Characterization):
- RDEC or AI equation rendering (bitmap in PDF, styled HTML in browser report)
- RDEC mix parameters table (flexural modulus, air voids, asphalt content, void/gradation parameters)
- Per-aircraft asphalt CDF table (HMA strain, N_fail_HMA, CDF_HMA vs CDF_Subgrade, governing indicator)
- CDF comparison summary (total asphalt CDF vs total subgrade CDF with governing mode)
- Explanatory note on the role of asphalt CDF in the design process

**Data model extensions** (`FaarFieldAnalysis/clsDetailedReportData.vb`):
- `clsDetailedReportData`: `AsphaltCDFTotal`, `AsphaltCDFComputed`, `AsphaltModel`, RDEC parameters (`RdecFlexuralMod`, `RdecAirVoids`, `RdecAsphaltContent`, `RdecVoidParameter`, `RdecPNMS`, `RdecPPCS`, `RdecP200`, `RdecGradationParameter`)
- `clsAircraftDetail`: `AsphaltCDF`, `AsphaltNtoFail`, `AsphaltStrain`

**Data capture** (`FaarFieldAnalysis/modStrDesignFlex.vb`):
- Per-aircraft asphalt N_fail and strain captured immediately after `LeafCDFFlex` with `Overflow=False` (before subgrade computation overwrites `gNtoFail()`)
- Section-level CDFAsp total and RDEC parameters captured at the report data finalization point

**Files modified:**
- `FaarFieldAnalysis/clsDetailedReportData.vb` — Added 11 section-level and 3 per-aircraft fields
- `FaarFieldAnalysis/modStrDesignFlex.vb` — Added asphalt data capture at two pipeline stages
- `FF2/ViewModels/MainWindowViewModel.vb` — Added D.2 Asphalt (HMA) Layer Fatigue subsection in Section D
- `FF2/Libs/HtmlReportGenerator.vb` — Added D.2 subsection with equation cards, RDEC parameters table, per-aircraft CDF table, CDF comparison cards, and new CSS classes

---

### 11. High-quality vector PDF export

Rerouted the CM Report PDF export from the GDI+ bitmap pipeline to the SVG-based HTML pipeline (`HtmlReportGenerator.Generate()`). Previously, "Save As PDF" rendered all charts as raster bitmaps (GDI+ → base64 PNG → SelectPdf), producing blurry charts especially at zoom. Now the PDF is generated from the same HTML/SVG source as "Open in Browser", giving:

- **Vector SVG charts** — infinitely sharp at any zoom level, no pixelation
- **Native HTML text** — crisp labels and annotations instead of text-rendered-to-pixels
- **Consistent output** — PDF and HTML reports are now identical in content and visual quality

Additional improvements for all report types:
- Increased SelectPdf web page width from 1024px to 1400px for higher-fidelity rendering of all reports (Summary, Structure, CDF Graph, PCR, etc.)
- Enhanced print/PDF media queries: `page-break-inside: avoid` on figures, tables, equations, CDF comparison cards; `shape-rendering: geometricPrecision` on SVGs; `color-adjust: exact` for backgrounds

**Files modified:**
- `FF2/ViewModels/MainWindowViewModel.vb` — `SectionReportCreatePdf()`: CM Report now calls `HtmlReportGenerator.Generate()` instead of `refreshDetailedReport()`
- `FF2/Libs/HtmlUtils.vb` — `HtmltoPdf()`: web page width 1024→1400
- `FF2/Libs/HtmlReportGenerator.vb` — Enhanced `@media print` CSS rules for PDF optimization

---

### 12. Visual enhancement of all FAARFIELD reports

Improved visual quality and PDF rendering fidelity across all 7 report types.

**CM Report (SVG pipeline fix + polish):**
- Added explicit `width`/`height` attributes to all 12 SVG elements in the HTML report. SelectPdf's WebKit engine cannot infer dimensions from `viewBox` alone, causing SVGs to render as ~50px thumbnails in PDF. With explicit dimensions, SVGs render at full size in PDF while CSS `width:100%; max-width` keeps them responsive in the browser.
- Changed dashboard CSS from `grid-template-columns: repeat(auto-fit, ...)` to `display: flex; flex-wrap: wrap` for SelectPdf WebKit compatibility.
- Added `<title>` child elements to all SVG charts for browser tooltip accessibility.
- Added consistent `#FAFBFC` plot-area backgrounds to Life Ratio and CDF Contribution bar charts.
- Added CSS hover interactivity on chart data points and bars (`.chart-svg circle:hover`, `.chart-svg rect.bar:hover`).
- Enhanced print media queries: `page-break-inside: avoid` on `svg` and `figure` elements.

**CDF Graph report:**
- Re-enabled Y-axis gridlines (were commented out).
- Changed chart rendering from 96 DPI to 192 DPI (2x supersampling) for crisper text and lines.
- Changed image encoding from BMP to PNG (smaller file size, alpha channel support).
- Added descriptive chart caption below the graph.

**PCR Graph report:**
- Changed chart rendering from 96 DPI to 192 DPI (2x supersampling).
- Changed image encoding from BMP to PNG.
- Added descriptive chart caption below the graph.

**Structure Report:**
- Changed pavement profile bitmap encoding from BMP to PNG in `BitmapImage2Bitmap()`.

**All tabular reports (Summary, Structure, PCR, Airport Master Record):**
- Increased zebra-stripe contrast from `#F8F9FA` to `#EEF2F8` in `Reports.css`.

**Files modified:**
- `FF2/Libs/HtmlReportGenerator.vb` — SVG width/height attributes, `<title>` elements, dashboard flexbox, plot backgrounds, hover CSS, print CSS
- `FF2/ViewModels/MainWindowViewModel.vb` — CDF/PCR graph 2x supersampling, BMP→PNG encoding (3 locations), chart captions, re-enabled gridlines
- `FF2/Resources/Reports.css` — Improved table zebra-stripe contrast

---

## Backlog — Engineering reasonableness guardrails to review

The original FAARFIELD code contains numerous hard-coded limits and engineering reasonableness checks. As the codebase is customized, each of these assumptions should be reviewed to determine whether it should be retained, relaxed, or made configurable. The list below catalogues every guardrail identified in the source.

### Input validation limits

| # | Parameter | Limit | Location | Status | Notes |
|---|-----------|-------|----------|--------|-------|
| 1 | **Annual departures** | Max 100,000 per aircraft | `FaarFieldModel/AirplaneInfo.vb` ~L439 | Raised to 500,000 | Original FAA limit; no regulatory citation in code |
| 2 | **Annual growth rate** | –10% to +10% | `FaarFieldModel/AirplaneInfo.vb` ~L461 | Active | Resets to 0% if exceeded; user warned via MessageBox |
| 3 | **Max wheels per gear** | 56 | `AMClassLib/frmGear.vb` L15 (`NMaxWheels`) | Active | Data-structure hard limit |

### Minimum layer thickness — new structures (design mode)

These are enforced in `FF2/ViewModels/MainWindowViewModel.vb` ~L5636–5785 and vary by material type, design type, and aircraft weight category.

| # | Material | Design type | Aircraft category | Min thickness (in) | Notes |
|---|----------|-------------|-------------------|--------------------|-------|
| 4 | P-401/P-403 HMA Surface | New Flexible / HMA on Aggregate | Light (≤12,500 lb) | 3 | |
| 5 | P-401/P-403 HMA Surface | New Flexible / HMA on Aggregate | Heavy (>12,500 lb) | 4 | |
| 6 | P-401/P-403 HMA Surface | Overlay variants | — | 2–3 | Varies by overlay type |
| 7 | P-401/P-403 Stabilized / P-304 / P-306 | New Flexible (light) | Light | 5 | |
| 8 | P-401/P-403 Stabilized / P-304 / P-306 | New Rigid | — | 5 | |
| 9 | P-401/P-403 Stabilized / P-304 / P-306 | Other | — | 2–3 | |
| 10 | P-209 Crushed Aggregate / P-211 Lime Rock / P-154 | New Flexible / New Rigid (thickness design) | — | 6 | Also enforced post-design in `UpdateManager.vb` L19 |
| 11 | P-209 / P-211 / P-154 | Other | — | 4 | |
| 12 | P-208 Crushed Aggregate / P-219 Recycled Concrete | New Flexible (thickness design) | — | 6 | |
| 13 | P-208 / P-219 | New Rigid (light) | Light | 3 | |
| 14 | P-208 / P-219 | New Rigid (heavy) | Heavy (>12,500 lb) | 6 | |
| 15 | P-208 / P-219 | Overlay | — | 4 | |
| 16 | PCC Surface | When `g3inchPCC` flag enabled | — | 3 | `modFedfaaGbl.vb` ~L1465 |
| 17 | HMA Surface | When `g2inchHMA` flag enabled | — | 2 | `modFedfaaGbl.vb` ~L1461 |

### Sublayer thickness — WES formula (during sublayering)

Enforced in `FaarFieldAnalysis/modCDF.vb` ~L879–896.

| # | Material | Min per sublayer (in) | Notes |
|---|----------|-----------------------|-------|
| 18 | P-209 Crushed Aggregate | 10 | WES formula sublayering |
| 19 | P-154 Uncrushed Aggregate | 8 | WES formula sublayering |

### Aircraft weight category thresholds

Defined in `FaarFieldAnalysis/modFedfaaGbl.vb` ~L1319–1344. These thresholds trigger different minimum-thickness requirements and Advisory Circular notes.

| # | Category | Weight threshold (lb) | Effect |
|---|----------|-----------------------|--------|
| 20 | Light aircraft | ≤12,500 | Allows thinner minimums (3 in HMA, 3 in P-208 base) |
| 21 | Heavy aircraft | >60,000 | Triggers heavier base requirements |
| 22 | Very heavy aircraft | ≥100,000 | Triggers AC Note 320 (new flexible) / Note 328 (new rigid) |

### Computational strain floors and ceilings

These prevent numerical instability (divide-by-zero, log of zero) in fatigue calculations.

| # | Parameter | Floor/Ceiling | Location | Notes |
|---|-----------|---------------|----------|-------|
| 23 | Asphalt strain | Floor: 1×10⁻⁶ | `modCDF.vb` ~L286 | Prevents log/divide-by-zero in AI/RDEC fatigue |
| 24 | Subgrade strain | Floor: 1×10⁻⁴ | `modCDF.vb` ~L325 | Triggers overflow flag if below; halves thickness |
| 25 | PCC subgrade critical strain | Ceiling: 0.001765093 | `modCDF.vb` ~L388 | Threshold for foundation vs. PCC failure mode |
| 26 | Equivalent thickness | Floor: 0.4 in | `modCDF.vb` ~L797 | Minimum for aggregate equivalent thickness calc |

### Modulus bounds

| # | Parameter | Range | Location | Notes |
|---|-----------|-------|----------|-------|
| 27 | Base/subbase modulus (equiv. thickness calc) | 200,000–700,000 psi | `modCDF.vb` ~L787 | Clamped for WES equivalent thickness formula |

### Convergence and solver controls

| # | Parameter | Value | Location | Notes |
|---|-----------|-------|----------|-------|
| 28 | CDF convergence tolerance | \|ln(CDF)\| < 0.005 | `modCDF.vb` ~L99 (`CDFExitErr`) | Newton-Raphson exit criterion |
| 29 | Sublayer activation threshold | \|ln(CDF)\| < 0.69 (CDF 0.5–2.0) | `modCDF.vb` ~L103 (`CDFErrCntrl`) | Triggers aggregate sublayering |

### Interface bonding bounds

| # | Parameter | Range | Location | Notes |
|---|-----------|-------|----------|-------|
| 30 | Interface bonding parameter | 0.001–0.99 (scaled to 0.001–100 stiffness) | `AMClassLib/modPG.vb` ~L157 | Bounds penalty stiffness for layer interfaces |

### Discretization constants

These are fixed grid sizes in the LEAF/CDF solver, not strictly "guardrails" but hard-coded assumptions that limit resolution.

| # | Parameter | Value | Location | Notes |
|---|-----------|-------|----------|-------|
| 31 | Lateral offsets for CDF sweep | 41 offsets | `ACNClassLib/clsLEAF.vb` L61 (`NOFF`) | 0–400 in at 10-in steps |
| 32 | Offset spacing | 10 inches | `ACNClassLib/clsLEAF.vb` L63 (`OFFSETINC`) | Fixed increment |
| 33 | Longitudinal nodes (tandem) | 1,800 | `ACNClassLib/clsLEAF.vb` L62 (`NNodesLong`) | For tandem gear CDF |
| 34 | Max layers (flexible) | 24 | `ACNClassLib/clsLEAF.vb` (`MaxNPLayers`) | Structural layer limit |
| 35 | Max layers (modulus tracking) | 32 | `ACNClassLib/clsLEAF.vb` (`MaxModulusNPLayers`) | Including sublayers |

### Material property bounds

| # | Parameter | Range | Location | Notes |
|---|-----------|-------|----------|-------|
| 36 | PCC flexural strength (R) | 500–1,000 psi (3.45–6.9 MPa) | `Material.vb` ~L299 | Default 650 psi; hard reset to default if exceeded |
| 37 | Subgrade modulus | 1,000–50,000 psi (6.89–344.74 MPa) | `Material.vb` ~L434 | Default 15,000 psi |
| 38 | Variable (Flexible) modulus | 150,000–400,000 psi | `Material.vb` ~L416 | Stabilized base range |
| 39 | Variable (Rigid) modulus | 250,000–700,000 psi | `Material.vb` ~L425 | Lean concrete/econocrete |
| 40 | Subgrade K-value | 20.9–440.4 pci (5.7–119.5 MN/m³) | `Material.vb` ~L344 | Default 172.4 pci |

### Solver/design controls

| # | Parameter | Value | Location | Notes |
|---|-----------|-------|----------|-------|
| 41 | Max design iterations | 25 loops | `modStrDesignFlex.vb` ~L516 | Abort with warning if not converged |
| 42 | Max thickness delta/iteration | 50 in | `modStrDesignFlex.vb` ~L509 | Prevents wild oscillation |
| 43 | CDF reduction threshold | CDF < 0.3 → 80% thickness | `modStrDesignFlex.vb` ~L186 | Not applied during life computation |
| 44 | Compaction departure threshold | 6,000 | `modCDF.vb` ~L28 | gCompactionDeparture |

### Capacity limits

| # | Parameter | Value | Location | Notes |
|---|-----------|-------|----------|-------|
| 45 | Max aircraft per section | 80 | `modFedfaaGbl.vb` ~L229 | MaxSectAC; historical limit (was 10, then 30, then 40, now 80) |
| 46 | Max sections per job | 100 | `modFedfaaGbl.vb` ~L228 | MaxSects |
| 47 | Max jobs | 100 | `modFedfaaGbl.vb` ~L227 | MaxJobs |

### Gross weight validation

| # | Parameter | Range | Location | Notes |
|---|-----------|-------|----------|-------|
| 48 | Gross weight | 0.6×–1.25× default weight | `AirplaneInfo.vb` ~L492 | Dynamic per-aircraft |

### How to use this list

Each item above should be evaluated for one of three dispositions:

1. **Retain** — The limit reflects a genuine physical or regulatory constraint (e.g., AC 150/5320-6 requirements). Document the citation.
2. **Make configurable** — The limit is reasonable but should be user-adjustable for research or non-standard applications. Move to a settings/options panel.
3. **Remove** — The limit was a legacy software constraint (e.g., array sizing) with no engineering basis and can be eliminated.
