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

### 3. "Open in Browser" HTML export for Detailed Computation Report

Added an **Open in Browser** button next to the existing "Save As PDF" button on the Detailed Computation Report pane. This saves the report as a standalone `.html` file and opens it in the default browser, bypassing the IE-based WPF WebBrowser control and SelectPdf renderer limitations.

**Files modified:**
- `FF2/Libs/HtmlUtils.vb` — Added `HtmlToFile()` method (saves HTML with UTF-8 encoding, launches default browser).
- `FF2/Views/MainWindow.xaml` — Added "Open in Browser" button bound to `OnSectionReportOpenHtml` command.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `OnSectionReportOpenHtml` command property and `SectionReportOpenHtml` handler (SaveFileDialog for `.html`, calls `HtmlToFile`).
