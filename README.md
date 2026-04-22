# FAARFIELD 2.1.1 — Source (FAA)

> [!IMPORTANT]
> **Official source notice:** This repository contains the source code of **FAARFIELD 2.1.1** as published by the Federal Aviation Administration: https://www.airporttech.tc.faa.gov/Products/Airport-Safety-Papers-Publications/Airport-Safety-Detail/ArtMID/3682/ArticleID/2841/FAARFIELD-20
>
> This copy is provided for inspection, study, and archival purposes only. It does not represent a reproduction or transfer of copyright. All rights and ownership remain with the Federal Aviation Administration.

> [!CAUTION]
> **Beta version.** This repository contains customizations to the original FAA-published source (documented in [Modifications](#modifications-from-original-faarfield-211-source)). Results from this version should be independently verified against the official FAARFIELD release before use in any production or regulatory context.

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
- [Build setup & compilation](#build-setup--compilation)
  - [Prerequisites](#prerequisites)
  - [Installation walkthrough](#installation-walkthrough)
  - [Opening and building the solution](#opening-and-building-the-solution)
  - [Expected build output](#expected-build-output)
  - [Running the application](#running-the-application)
  - [Troubleshooting](#troubleshooting)
- [Unit tests](#unit-tests)
- [Project dependency graph](#project-dependency-graph)
- [Modifications from original FAARFIELD 2.1.1 source](#modifications-from-original-faarfield-211-source)
- [Backlog — Engineering reasonableness guardrails](#backlog--engineering-reasonableness-guardrails-to-review)

---

## Overview

**FAARFIELD** (FAA Rigid and Flexible Iterative Elastic Layered Design) is a VB.NET desktop application for airfield pavement thickness design and evaluation. It implements:

- **Layered elastic theory** (LEAF) for computing deflections, strains, and stresses in multi-layer flexible pavements.
- **3-D finite-element analysis** (FAASR / NIKE3D-based) for rigid and composite pavement response.
- **ACN/PCN classification** per ICAO standards for pavement strength reporting.
- **Cumulative damage factor (CDF)** integration for mixed-traffic thickness design.
- **Overlay design** for flexible-on-flexible, PCC-on-rigid, HMA-on-rigid, and unbonded overlays.

> [!NOTE]
> The computational engines in this project were ported from Fortran to VB.NET. Variable names and algorithm structure deliberately mirror the original Fortran source for traceability to FAA technical reports. Do not rename variables for "clarity" — they map to published equations and documentation.

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
│   ├── Libs/HtmlReportGenerator.vb      SVG-based HTML report generator
│   ├── Resources/Reports.css            Embedded CSS for all reports
│   └── Defaults/Aircraft/aircraft.xml   Aircraft library (1.9 MB, XML)
│
│  ── Supporting ─────────────────────────────────────────────────
├── CreateSignedAircraftLibrary/         Utility: signed aircraft libraries
├── FAARFIELDUnitTests/                  MSTest unit tests
├── FAARFIELD.Installer/                 WiX installer project
├── lib/                                 Third-party assemblies (Telerik)
├── packages/                            NuGet packages
│
│  ── Documentation ─────────────────────────────────────────────
└── Documentation/                       Standalone documentation site
    ├── build_docs.py                     CHM → HTML generator
    └── Documentation.html                Self-contained docs (163 sections, 18 MB)
```

> [!TIP]
> The `lib/` folder contains pre-packaged Telerik UI for WPF assemblies. No separate Telerik license is needed to build or run the solution.

---

## Core computational modules

The sections below describe each computational engine, the key files to inspect, and the most important functions and data structures within them. Line numbers are approximate and refer to the current source.

> [!WARNING]
> Many `Public` variables in `modCDF.vb`, `modFedfaaGbl.vb`, and similar modules are **shared mutable state** used across the entire analysis pipeline. Renaming or retyping them will break callers across multiple projects. The FEM solver (`FEMClassLib/Solve/`) has 96 files with heavily interrelated state — changes require running the full unit test suite and verifying against known benchmark results.

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

<details>
<summary><strong>Mathematical basis</strong></summary>

The solver uses the Hankel transform over a multi-layer elastic half-space. Response at radial distance *r* and depth *z* takes the form ∫ K(α,z)·J_n(α·r)·α dα, evaluated via 500-point Gauss-Laguerre quadrature. A 1-inch dummy top layer of surface material is inserted for numerical stability. Each tire is modelled as uniform circular pressure with contact radius a = √(W_wheel/(π·p_tire)); superposition handles multiple tires.

</details>

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
- Multiple report types (Structure, CDF Graph, PCR, Airport Master Record, Summary, and **CM Report**)

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
| **CM Report** | Full computational trace for flexible pavement design (see below) |

### CM Report (Computational Mechanics)

The **CM Report** is a comprehensive technical report that documents the full computational workflow of thickness design, CDF accumulation, and ACR/PCR analysis. It is populated automatically when you run **Thickness Design** or **Life Analysis** on a flexible pavement section. Access it from the section tree under **Reports > CM Report**.

> [!NOTE]
> The CM Report was originally named "Detailed Computation Report." Internal property names (`DetailedReportHtml`, `DetailedReportIsHidden`, `SerializationTag="DetailedReport"`) retain the original naming for serialization compatibility.

**Key features:**

- **Section A — Pavement Structure Summary:** Design layers and expanded sublayer structure (after modulus adjustment), evaluation depth at subgrade, aggregate sublayering procedure with modulus-depth profile chart.
- **Section B — Design Equations:** Rendered equations for the subgrade strain failure model (AA, BB, N_fail), CDF formula, coverage-to-pass (Gaussian wander model), and convergence criterion.
- **Section C — Coverage-to-Pass (C/P) Concept:** Educational diagram explaining how C/P is computed from Gaussian lateral wander (σ = 30.435 in.), multi-wheel superposition, and the 41 evaluation strips.
- **Section D — Fatigue Characterization:** Subgrade fatigue curve plot with aircraft operating points, plus asphalt (HMA) layer fatigue when applicable (RDEC or AI models).
- **Section E — Per-Aircraft Detailed Breakdown:** For each aircraft: gear configuration plan view, pavement cross-section with tire projection, gear parameters, CDF vs. offset chart, wheel-level C/P decomposition, and step-by-step computation walkthrough.
- **Section F — C/P Distribution:** C/P ratio vs. lateral offset for all aircraft.
- **Section G — CDF Sweep Table:** Full 41-offset table of C/P and CDF per aircraft and total CDF; critical offset identification.
- **Section H — CDF Distribution Across Pavement Width:** Composite CDF chart and contribution summary at the critical strip.
- **Section I — Newton-Raphson Convergence:** Dual-axis convergence plot and iteration log.
- **Section J — ACR Details** *(conditional)*: Reference structure, DSWL iterations, final ACR per subgrade category.
- **Section K — PCR Elimination Rounds** *(conditional)*: Critical aircraft per round, MGW iteration, round PCR.
- **Section L — ACR vs. Damage Per Departure** *(conditional)*: Bubble chart relating ACR to normalized CDF per departure.

<details>
<summary><strong>Rendering architecture</strong></summary>

The report data is captured during analysis by `clsDetailedReportData` (in `FaarFieldAnalysis/clsDetailedReportData.vb`) and rendered via two parallel pipelines:

**PDF pipeline (GDI+ bitmaps):**
```
Analysis Engine → clsDetailedReportData
  → MainWindowViewModel.refreshDetailedReport()
    → 12 GDI+ chart functions (2x/3x supersampled bitmaps)
    → HtmlUtils (base64 PNG embedding)
    → Reports.css
    → BrowserBehavior (WebBrowser display)
    → SelectPdf HtmlToPdf (PDF export)
```

**HTML pipeline (native SVG):**
```
HtmlReportGenerator.Generate()
  → Complete HTML5 document via StringBuilder
  → Inline SVG charts (viewBox-based, responsive)
  → CSS inlined in <style> block
  → Self-contained .html with zero external dependencies
  → Opens in default browser
```

The PDF export now uses the SVG pipeline for the CM Report, producing vector-quality charts.

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
| `DrawGearConfiguration()` | Plan view of wheel positions with CDF strips and Gaussian wander |
| `DrawACRDamageChart()` | ACR vs. CDF-per-departure bubble chart |
| `DrawCDFContributionChart()` | Horizontal bar chart of CDF % contribution per aircraft |
| `DrawLifeRatioChart()` | Diverging bar chart of fatigue life reserve ratio |
| `DrawModulusDepthProfile()` | Modulus vs. depth step chart for sublayered sections |

</details>

---

## Software stack

| Component | Detail |
|-----------|--------|
| **Language** | Visual Basic .NET (VB.NET) |
| **Framework** | .NET Framework 4.8 |
| **IDE / Build** | Visual Studio 2022 (Community or higher) / MSBuild |
| **UI** | WPF (FF2) and Windows Forms (FaarFieldAnalysis) |
| **Third-party** | Telerik UI for WPF (`lib/RCWPF/`), iTextSharp (PDF), Select.HtmlToPdf |
| **Testing** | MSTest 2.1.1 |
| **Installer** | WiX Toolset (`FAARFIELD.Installer/`) |

---

## Build setup & compilation

This section provides step-by-step instructions for setting up a development environment capable of building and running FAARFIELD from source.

### Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| **Windows** | 10 or 11 (64-bit) | Required — this is a WPF desktop application |
| **Visual Studio** | 2022 Community (free) or higher | [Download here](https://visualstudio.microsoft.com/vs/community/) |
| **.NET Framework** | 4.8 Targeting Pack | Installed via Visual Studio workloads |
| **Disk space** | ~3 GB | For Visual Studio + workloads; the FAARFIELD source itself is ~150 MB |

> [!NOTE]
> Visual Studio **2019** also works but is no longer receiving feature updates. Visual Studio 2022 Community Edition is free for individual developers, open-source projects, academic research, and small teams (up to 5 users).

### Installation walkthrough

#### Step 1 — Download Visual Studio 2022 Community

Download the installer from https://visualstudio.microsoft.com/vs/community/ and run it. The Visual Studio Installer will launch and present a list of **Workloads**.

#### Step 2 — Select the required workload

In the **Workloads** tab, check:

- [x] **.NET desktop development**

This single workload installs everything you need: the VB.NET compiler, .NET Framework 4.8 targeting pack, WPF designer, Windows Forms designer, NuGet package manager, and MSBuild.

> [!TIP]
> You do **not** need the "ASP.NET and web development", "Azure development", or ".NET Multi-platform App UI" workloads. Only ".NET desktop development" is required. Keeping the install minimal saves disk space and download time.

#### Step 3 — Verify .NET Framework 4.8 targeting pack

In the **Individual components** tab (next to Workloads), search for `.NET Framework 4.8` and confirm these are checked:

- [x] .NET Framework 4.8 SDK
- [x] .NET Framework 4.8 targeting pack

These are normally included with the ".NET desktop development" workload, but it's worth confirming.

> [!WARNING]
> If the .NET Framework 4.8 targeting pack is missing, the solution will open but every project will show **"The reference assemblies for .NETFramework,Version=v4.8 were not found"** and the build will fail with hundreds of errors. This is the single most common setup problem.

#### Step 4 — Install

Click **Install** (or **Modify** if Visual Studio is already installed). The download is typically 2–3 GB. Once complete, launch Visual Studio.

### Opening and building the solution

1. **Clone or download** this repository to a local folder (e.g., `C:\Repos\FAARFIELD-2.1.1`).

2. **Open the solution** — Double-click `FAARFIELD.sln` or use **File > Open > Project/Solution** in Visual Studio.

3. **NuGet restore** — Visual Studio automatically restores NuGet packages on the first build. You should see a brief "Restoring NuGet packages..." notification in the status bar. If prompted, click **Restore**.

4. **Set the startup project** — In Solution Explorer, right-click the **FF2** project and select **Set as Startup Project**. The project name should appear in **bold**.

5. **Build the solution** — Press `Ctrl+Shift+B` or go to **Build > Build Solution**.

> [!TIP]
> **Command-line build** (optional): If you prefer building from the command line or need to automate builds:
> ```
> "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" FAARFIELD.sln -p:Configuration=Debug
> ```
> If you installed VS Build Tools instead of the full IDE, the path is:
> ```
> "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" FAARFIELD.sln -p:Configuration=Debug
> ```

### Expected build output

A successful build produces output similar to:

```
Build started...
1>------ Build started: Project: FaarFieldModel, Configuration: Debug Any CPU ------
2>------ Build started: Project: LEAFClassLib, Configuration: Debug Any CPU ------
...
========== Build: 12 succeeded, 0 failed, 0 up-to-date, 1 skipped ==========
```

> [!NOTE]
> **Expected warnings.** The build produces several pre-existing warnings that are safe to ignore:
> - **BC42024** — Unused local variables (`ac7`, `_factory`, etc.). Dead code, no runtime impact.
> - **BC42105** — Anonymous `Function` lambdas in `InvokeAsync` without explicit return. Return value is discarded by the dispatcher.
> - **MSB3270** — `FAAMeshClassLib` processor architecture mismatch. Works at runtime because the app targets x86.
> - **MSB3187** — Referenced assembly version mismatch. Resolved at runtime by binding redirects.
>
> These warnings have been evaluated and determined to pose no runtime risk. See [Modification 14](#14-build-warning-cleanup) for details.

> [!CAUTION]
> If you see **errors** (not warnings), the most common causes are:
> 1. Missing .NET Framework 4.8 targeting pack — see [Step 3](#step-3--verify-net-framework-48-targeting-pack)
> 2. Failed NuGet restore — right-click the solution in Solution Explorer and select **Restore NuGet Packages**
> 3. The FAARFIELD.Installer project may fail if you don't have the WiX Toolset installed — this is safe to ignore (it only builds the `.msi` installer, not the application itself). You can right-click it in Solution Explorer and select **Unload Project** to suppress its errors.

### Running the application

1. Press **F5** (Start Debugging) or **Ctrl+F5** (Start Without Debugging).
2. The FAARFIELD WPF window will open with the main job/section tree on the left and the working area on the right.
3. To verify everything works, create a new job, add a flexible section with a few aircraft, and run **Thickness Design**.

> [!TIP]
> The first analysis run may take 10–30 seconds depending on the number of aircraft and your hardware. The status tab shows a progress banner with elapsed time. If the progress bar appears stuck, the solver is likely in the FEM or LEAF computation loop — let it finish.

### Troubleshooting

<details>
<summary><strong>"The reference assemblies for .NETFramework,Version=v4.8 were not found"</strong></summary>

**Cause:** The .NET Framework 4.8 targeting pack is not installed.

**Fix:** Open Visual Studio Installer → Modify → Individual Components → check ".NET Framework 4.8 targeting pack" and ".NET Framework 4.8 SDK" → click Modify.

</details>

<details>
<summary><strong>Build fails with "Could not copy file... because it is being used by another process"</strong></summary>

**Cause:** A previous instance of FAARFIELD is still running and has locked the output DLL/EXE files in `bin/Debug/`.

**Fix:** Close all running instances of FAARFIELD (check the system tray), then rebuild. If the problem persists, use Task Manager to end any `FF2.exe` processes.

</details>

<details>
<summary><strong>NuGet packages fail to restore</strong></summary>

**Cause:** Network issues or a corrupted package cache.

**Fix:**
1. Right-click the solution in Solution Explorer → **Restore NuGet Packages**
2. If that fails, delete the `packages/` folder and rebuild — NuGet will re-download everything
3. Check that `nuget.org` is listed under **Tools > NuGet Package Manager > Package Sources**

</details>

<details>
<summary><strong>FAARFIELD.Installer project errors (WiX)</strong></summary>

**Cause:** The WiX Toolset v3.x Visual Studio extension is not installed. This project builds the `.msi` installer and is not required for development.

**Fix:** Right-click `FAARFIELD.Installer` in Solution Explorer → **Unload Project**. The rest of the solution will build and run normally without it.

</details>

<details>
<summary><strong>Telerik-related warnings or missing references</strong></summary>

**Cause:** Telerik assemblies in `lib/RCWPF/` may not be loading correctly.

**Fix:** The `lib/` folder must be present at the repository root with all `.dll` files. If you cloned with a shallow checkout or the folder is empty, do a full clone. The assemblies are checked into the repository — no Telerik license or NuGet feed is required.

</details>

<details>
<summary><strong>Application launches but shows blank/white window</strong></summary>

**Cause:** Typically a XAML binding error or missing resource. Check the **Output** window in Visual Studio (View > Output, select "Debug" in the dropdown) for binding errors.

**Fix:** Ensure you are running the **FF2** project (not FaarFieldAnalysis) as the startup project. Clean and rebuild the solution (**Build > Clean Solution**, then **Build > Rebuild Solution**).

</details>

---

## Unit tests

Test files live in `FAARFIELDUnitTests/` and use the MSTest framework.

| File | Scope |
|------|-------|
| [AircraftUnitTests.vb](FAARFIELDUnitTests/AircraftUnitTests.vb) | Aircraft model validation |
| [UnitTest1.vb](FAARFIELDUnitTests/UnitTest1.vb) | General functionality checks |

Run tests from the Visual Studio **Test Explorer** (`Ctrl+E, T`) or via command line:

```
vstest.console FAARFIELDUnitTests\bin\Debug\FAARFIELDUnitTests.dll
```

> [!WARNING]
> Numerical results must match FAA-published verification cases. Do not "fix" floating-point tolerances that look loose — they reflect validated engineering accuracy.

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

> [!IMPORTANT]
> These modifications are for computational mechanics research purposes. The original FAARFIELD computational engines (LEAF, FEM, CDF, ACN/PCN) have **not** been altered — all changes are in the UI layer, reporting pipeline, and input validation bounds.

---

### 1. Detailed Computation Report (new feature)

Added a comprehensive HTML report that documents the full computational trace of flexible pavement thickness design. The report is generated automatically after running Thickness Design or Life Analysis and is accessible from the section tree under **Reports > CM Report**.

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

---

### 2. Annual departure limit increased to 500,000

Raised the maximum allowable annual departures per aircraft from 100,000 to 500,000.

**Files modified:**
- `FF2/ValidationRules/AnnualDepartureValidationRule.vb` — Changed validation cap from 100,000 to 500,000.
- `FF2/Models/AircraftList.vb` — Changed error-list validation cap from 100,000 to 500,000.
- `FF2/ViewModels/MainWindowViewModel.vb` — Changed MessageBox validation cap from 100,000 to 500,000.

The original 100,000 limit was an engineering reasonableness bound with no regulatory citation in the code. This change allows analysis of higher-traffic scenarios. All other input limits (design life 1–50 years per AC 150/5320-6D §302.a, growth rate ±10%) remain unchanged.

---

### 3. Native HTML Report with SVG Charts ("Open in Browser")

Added an **Open in Browser** button next to the existing "Save As PDF" button on the CM Report pane. This generates a **completely independent HTML report** (not a re-export of the PDF) using a parallel rendering pipeline with native browser technologies:

- **Inline SVG charts** — All visualizations are rendered as scalable vector graphics directly in the HTML, replacing the GDI+ bitmap pipeline. Charts scale perfectly at any zoom level and look crisp in print.
- **Bleasdale piecewise visualization** — When the Bleasdale subgrade model is used, the fatigue curve SVG shows three color-coded zones with transition markers and equation info box.
- **Clean Unicode** — HTML entities render natively in any modern browser, eliminating GDI+ text-to-bitmap Unicode issues.
- **Modern CSS** — CSS Grid dashboard, CSS variables for theming, responsive layout, professional typography, hover effects, print-optimized styles.
- **Clickable table of contents** — All 12 sections (A–L) have anchor links for instant navigation.
- **Collapsible data tables** — Large per-offset tables use HTML `<details>` elements to keep the report compact.
- **Self-contained** — The output `.html` file has zero external dependencies (all CSS and SVG are inline).

The existing PDF report (GDI+ bitmaps → SelectPdf) remains completely untouched.

**Files added:**
- `FF2/Libs/HtmlReportGenerator.vb` — New `HtmlReportGenerator` class (~2600 lines) with `Generate()` method, 10+ SVG chart functions, CSS stylesheet, and helper utilities.

**Files modified:**
- `FF2/Libs/HtmlUtils.vb` — Added `HtmlToFile()` method (saves HTML with UTF-8 encoding, launches default browser).
- `FF2/Views/MainWindow.xaml` — Added "Open in Browser" button bound to `OnSectionReportOpenHtml` command.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `OnSectionReportOpenHtml` command property and handler.
- `FF2/FF2.vbproj` — Added `HtmlReportGenerator.vb` to compilation.

---

### 4. CM Report rename and gear configuration visualization

Renamed **"Detailed Computation Report"** to **"CM Report"** (Computational Mechanics) across all user-facing strings. Internal property names remain unchanged for serialization compatibility.

**UI changes:**
- Report tab now displays "CM Report" with a SemiBold font, an info icon, and a multi-line tooltip.
- PDF and HTML default filenames use "CM Report" instead of "Detailed Computation Report".

**Gear configuration visualization** added to Section E of both report pipelines:
- Plan view of wheel positions with tire contact patches drawn as semi-transparent circles.
- CDF offset strips (41 dashed vertical lines at 10-inch intervals) with the critical strip highlighted in red.
- Gaussian lateral wander overlay (σ=30.435 in.) as a translucent filled bell curve.
- Dimension annotations for dual spacing, tandem spacing, and contact area.
- Coordinate labels at each wheel showing (X, Y) in the gear coordinate system.

**Data model extensions** (`clsAircraftDetail` in `clsDetailedReportData.vb`):
- `WheelX()`, `WheelY()` — lateral/longitudinal position of each wheel
- `NWheels` — number of tires
- `DualSpacing`, `GearSpacing` — gear geometry dimensions
- Fixed population of `TandemSpacing` and `ContactArea`

**Files modified:**
- `FaarFieldAnalysis/clsDetailedReportData.vb` — Added 5 fields to `clsAircraftDetail`.
- `FaarFieldAnalysis/modCDF.vb` — Populated new gear geometry fields during CDF computation.
- `FF2/Views/MainWindow.xaml` — Replaced header with styled StackPanel content.
- `FF2/ViewModels/DetailedReportViewModel.vb` — Renamed tree node.
- `FF2/ViewModels/MainWindowViewModel.vb` — Renamed strings, added `DrawGearConfiguration()`.
- `FF2/Libs/HtmlReportGenerator.vb` — Added `AppendGearConfigSVG()`.

---

### 5. About window

Added an **About** button to the main toolbar that opens a custom borderless dialog with:

- **Beta disclosure** — Prominent amber notice that this is a customized version, not the official FAA release.
- **Credits** — Customization by Johann Cardenas; original FAA authors acknowledged.
- **License** — Permissive use with full liability disclaimer; original FAARFIELD IP remains with the FAA.
- **Version display** — Shows `v2.1.1-CM` and the build date.

**Files added:**
- `FF2/Views/AboutWindow.xaml` — WPF window with styled layout.
- `FF2/Views/AboutWindow.xaml.vb` — Code-behind for build date loading and drag support.

**Files modified:**
- `FF2/Application.xaml` — Added `AboutButton` resource.
- `FF2/Views/MainWindow.xaml` — Added About button to toolbar.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `OnAbout_Command` and handler.
- `FF2/FF2.vbproj` — Added `AboutWindow.xaml` and code-behind to compilation.

---

### 6. Analysis progress banner and auto-tab switching

Added a visual progress banner to the **Status tab** that shows:

1. **Indeterminate progress bar** in blue with hourglass icon, elapsed time (HH:MM:SS), and live status text.
2. **Green "completed" banner** when analysis finishes — check mark icon, final elapsed time, and "results are ready for review."
3. **Amber "canceled" banner** if the user clicks Cancel during analysis.

Legacy overlay TextBoxes are preserved at zero opacity for data-binding compatibility.

**Files modified:**
- `FF2/Views/MainWindow.xaml` — Redesigned Status tab with Grid layout and progress banner.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added 11 progress banner properties and 4 helper methods.

---

### 7. Gear tab visualization modernization

Rewrote the `PaintGear()` method in `FF2/Libs/ModuleDrawProfile.vb` with modern GDI+ rendering:

- Anti-aliased rendering with ClearType text hints
- Teal gradient header bar with aircraft name and gear type badge
- Dot grid background on warm gray
- Rounded-rectangle tire imprints with gradient fills
- Numbered wheel labels and coordinate annotations
- Mirrored wheels rendered with softer transparency
- Dimension annotations for dual spacing and tandem spacing
- Legend box with entries for tire imprint, mirrored wheel, and evaluation point

Original coordinate transform logic (scale, scaleRatio, B-52/A380 special cases, unit branching) is fully preserved.

**Files modified:**
- `FF2/Libs/ModuleDrawProfile.vb` — Complete rewrite of `PaintGear()` (lines 44–370).

---

### 8. Aggregate sublayer modulus documentation in CM Report

Added a comprehensive explanation of the **unbound aggregate sublayering procedure** to Section A of both report pipelines:

- Explanation of why aggregate layers require sublayering and depth-dependent moduli
- Mathematical formula: E_i = E_{i-1} × (f1 - f2)
- Parameters table with C, D coefficients per aggregate type (P-209: C=10.52, D=2.0; P-154: C=6.88, D=1.56)
- Sublayer detail tables showing the modulus gradient
- Modulus-depth profile chart (step chart with stacked layer bars)

**Files modified:**
- `FaarFieldAnalysis/clsDetailedReportData.vb` — Extended `clsSublayerData` with 10 new fields.
- `FaarFieldAnalysis/modStrDesignFlex.vb` — Populated sublayering parameters during data capture.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added `DrawModulusDepthProfile()` and Section A content.
- `FF2/Libs/HtmlReportGenerator.vb` — Added `AppendSublayerModulusSection()`, `AppendModulusDepthSVG()`, and related helpers.

---

### 9. Gross weight guardrail override

Changed the gross weight validation in `FaarFieldModel/AirplaneInfo.vb` from a hard block to a user-overridable warning. The dialog now shows the allowed range and offers **Yes** (override for research) / **No** (revert to previous value).

**Files modified:**
- `FaarFieldModel/AirplaneInfo.vb` — Replaced `MessageBox.Show` + hard revert with `MessageBoxButtons.YesNo` dialog.

---

### 10. Asphalt (HMA) CDF documentation in CM Report

Added asphalt layer fatigue characterization to both report pipelines. Two fatigue models:

- **RDEC model** — Rate of Dissipated Energy Change with mix-specific volumetric parameters.
- **AI model** — Asphalt Institute simplified model.

The report now includes in Section D: equation rendering, RDEC mix parameters table, per-aircraft asphalt CDF table, and CDF comparison summary (subgrade vs. asphalt, with governing mode indicator).

**Files modified:**
- `FaarFieldAnalysis/clsDetailedReportData.vb` — Added 11 section-level and 3 per-aircraft fields.
- `FaarFieldAnalysis/modStrDesignFlex.vb` — Added asphalt data capture at two pipeline stages.
- `FF2/ViewModels/MainWindowViewModel.vb` — Added D.2 Asphalt subsection in Section D.
- `FF2/Libs/HtmlReportGenerator.vb` — Added D.2 subsection with equation cards, tables, and CSS.

---

### 11. High-quality vector PDF export

Rerouted the CM Report PDF export from the GDI+ bitmap pipeline to the SVG-based HTML pipeline (`HtmlReportGenerator.Generate()`). The PDF now contains vector SVG charts instead of raster bitmaps.

Additional improvements:
- Increased SelectPdf web page width from 1024px to 1400px for all other report types.
- Enhanced print/PDF media queries for background preservation and page-break control.

**Files modified:**
- `FF2/ViewModels/MainWindowViewModel.vb` — CM Report PDF now calls `HtmlReportGenerator.Generate()`.
- `FF2/Libs/HtmlUtils.vb` — `HtmltoPdf()`: web page width 1024→1400.
- `FF2/Libs/HtmlReportGenerator.vb` — Enhanced `@media print` CSS rules.

---

### 12. Visual enhancement of all FAARFIELD reports

Improved visual quality and PDF rendering fidelity across all 7 report types.

**CM Report (SVG pipeline):**
- Added explicit `width`/`height` attributes to all SVG elements (SelectPdf's WebKit cannot infer from `viewBox` alone).
- Changed dashboard CSS from CSS Grid to flexbox for SelectPdf compatibility.
- Added `<title>` child elements to SVGs for accessibility.
- Added plot-area backgrounds and CSS hover interactivity.

**CDF Graph & PCR Graph reports:**
- 2x supersampling (96 DPI → 192 DPI).
- BMP → PNG encoding (smaller files, alpha support).
- Added descriptive chart captions.

**Structure Report:**
- BMP → PNG encoding for pavement profile bitmaps.

**All tabular reports:**
- Increased zebra-stripe contrast from `#F8F9FA` to `#EEF2F8` in `Reports.css`.

**Files modified:**
- `FF2/Libs/HtmlReportGenerator.vb` — SVG dimensions, dashboard flexbox, hover CSS, print CSS.
- `FF2/ViewModels/MainWindowViewModel.vb` — 2x supersampling, BMP→PNG, chart captions, gridlines.
- `FF2/Resources/Reports.css` — Improved table contrast.

---

### 13. CM Report PDF quality overhaul

Addressed multiple quality issues that made the CM Report PDF difficult to read at printed size.

> [!NOTE]
> The root cause was a mismatch between the SelectPdf web page width (1400px) and the HTML body max-width (1100px), wasting 300px and scaling all content down to ~0.44pt per CSS pixel — making 9px SVG fonts render at an illegible ~4pt.

**SelectPdf rendering settings** (`FF2/Libs/HtmlUtils.vb`):
- `webPageWidth` 1400 → 1100, matching CSS `max-width`. New scale: 0.556pt per CSS-px (27% increase).
- Added `CssMediaType = Screen` to prevent background-color stripping.

**Table header visibility** — Hardcoded `#1a3c6e !important` with `print-color-adjust: exact` (CSS `var()` may not resolve in SelectPdf's WebKit).

**Header metadata spacing** — Added `border-left` separator as fallback for CSS `gap` (not supported in SelectPdf's WebKit).

**SVG font sizes** — All fonts below 10px increased to 10–14px range. Minimum font size rule: no SVG text below 10px.

| CSS Class | Before | After |
|-----------|--------|-------|
| `.chart-svg .chart-title` | bold 12px | bold 14px |
| `.chart-svg .axis-label` | 11px | 600 12px |
| `.chart-svg .tick` | 9px | 10px |
| `.chart-svg .label` | 10px | 11px |
| `.chart-svg .legend-text` | 10px | 11px |

**Stroke widths** — Grid lines 0.5 → 0.8; data curves 1.5 → 2.

**CDF Y-axis** — Added `FmtCDFSvg()` helper: values ≥ 0.001 use fixed notation; smaller values use scientific notation with SVG `<tspan>` superscripts.

**Page breaks** — Each aircraft section starts on a new page; sub-elements (figures, tables) use `break-inside: avoid`.

**Chart left margins** — Increased by 10–15px across CDF and gear config charts.

**Files modified:**
- `FF2/Libs/HtmlUtils.vb` — webPageWidth 1400→1100, CssMediaType=Screen.
- `FF2/Libs/HtmlReportGenerator.vb` — CSS overhaul, inline font sizes, stroke widths, FmtCDFSvg(), margins.

---

### 14. Build warning cleanup

Fixed 8 pre-existing compiler warnings that represented potential runtime risks. No functional behavior was changed.

**Uninitialized variable warnings (BC42104):**
- `MainWindowViewModel.vb` — `FrostDepthReading` given explicit `= Nothing` initializer.
- `RunAnalysis.vb` — `S1`–`S5` split into individual declarations with `= ""` initializers.

**Function missing return on all paths (BC42105):**
- `ThicknessConverter.vb` — Added `Return ""` for unset `DimensionalProperty`.
- `AircraftLibrary.vb` — Added `Return Nothing` when save dialog is canceled.
- `MainWindowViewModel.vb` — `PCRReportPage()` empty stub given `Return Nothing`.

**Duplicate XML doc comment (BC42305):**
- `MainWindowViewModel.vb` — Removed stale duplicate `''' <summary>` block.

> [!NOTE]
> **Intentionally not fixed** (low risk, no runtime impact):
> - BC42024 — Unused local variables (dead code)
> - BC42105 — Anonymous `Function` lambdas in `InvokeAsync` (return value discarded by dispatcher)
> - MSB3270/MSB3187 — Processor architecture mismatch (works at runtime targeting x86)

**Files modified:**
- `FF2/ViewModels/MainWindowViewModel.vb`
- `FF2/Models/RunAnalysis.vb`
- `FF2/Converters/ThicknessConverter.vb`
- `FF2/Libs/AircraftLibrary.vb`

---

### 15. UI/UX modernization (41 improvements)

Comprehensive visual and interaction refresh of the WPF interface, guided by a 48-item audit (`UI_UX_Audit.md`). All changes are in the presentation layer — no computational code was modified.

**Theme system** (`FF2/Themes/ModernTheme.xaml`):
- 16 named `SolidColorBrush` resources (FAA Blue palette, grays, semantic colors).
- Implicit styles for `Button` (rounded, animated hover/press), `TextBox` (animated focus border), `DataGridRow` (animated hover + selection), `DataGridColumnHeader` (blue header), `GroupBox` (14pt SemiBold header, 2px left blue accent border via ControlTemplate), `Label` (secondary text).
- Keyed styles: `PrimaryButton` (blue bg, white text, animated), `WatermarkTextBox` (placeholder via Tag), `NumericCellStyle` (right-aligned), `ToolbarShadowBorder`, `AnimatedPaneStyle`.
- 4 typography heading styles (`TypeHeadingLarge`/`Medium`/`Small`/`Title`).

**MainWindow.xaml changes:**
- Replaced all hardcoded colors (`WhiteSmoke`, `Blue`, `DarkGray`, `White`, `Black`, `Red`, `Gray`, `#e6e6e6`) with `StaticResource` references.
- Keyboard shortcuts: `Ctrl+N` (new), `Ctrl+O` (open), `Ctrl+S` (save), `F1` (help).
- Status bar at bottom with job name, structure name, analysis type.
- Traffic DataGrid: `FrozenColumnCount="1"`, `AlternatingRowBackground`, `NumericCellStyle` on 12 numeric columns, `Width="Auto"` with `MinWidth` for numeric columns.
- Explorer/Material `RadTreeView`: hover + selection backgrounds, padding, Unicode node icons via `TreeNodeIconConverter`.
- Aircraft and material filter `TextBox` fields with watermark placeholders and live filtering.
- Toolbar group labels ("AIRCRAFT", "BATCH") in 9pt SemiBold secondary color.
- `PrimaryButton` style on Run button.
- `DropShadowEffect` on cross-section panel.
- Progress banner with color-coded states (blue=running, green=success, amber=warning, red=failure) and left accent border.
- Toast notification overlay (auto-dismissing) for save/export actions.

**GDI+ rendering** (`FF2/Libs/ModuleDrawProfile.vb`):
- Anti-aliasing, ClearTypeGridFit, HighQualityBicubic rendering.
- `MeasureString`-sized semi-transparent label boxes (replaced fixed-width green).
- Design layer highlight: FAA blue (replaced bright green), width 8 → 6.
- Dotted grid lines at tick intervals.
- Axis label font: 7.5pt → 9.0pt.
- Legend: bottom-right with rounded rectangle background, dynamic sizing.
- Gear badge: pill-shape using `GraphicsPath` arcs.

**ViewModel** (`FF2/ViewModels/MainWindowViewModel.vb`):
- Aircraft filter: `AircraftFilterText` property with `ApplyAircraftFilter()`.
- Material filter: `MaterialFilterText` property with `ApplyMaterialFilter()` and category auto-expand.
- Toast notification: `ShowToast()` with auto-dismiss via `DispatcherTimer`, success/error styles.
- Progress states: `ShowProgressFailed()` for red error banner.

**DPI awareness** (`FF2/My Project/app.manifest`):
- `PerMonitorV2` DPI awareness enabled.
- Windows 10 `supportedOS` declared.

**Files added:**
- `FF2/Themes/ModernTheme.xaml` — Central theme resource dictionary.
- `FF2/Converters/TreeNodeIconConverter.vb` — Maps ViewModel types to Unicode icon characters.

**Files modified:**
- `FF2/Views/MainWindow.xaml` — All visual changes listed above.
- `FF2/ViewModels/MainWindowViewModel.vb` — Filter, toast, progress properties.
- `FF2/Libs/ModuleDrawProfile.vb` — GDI+ rendering improvements.
- `FF2/Application.xaml` — Theme dictionary merge, converter registration, window font.
- `FF2/My Project/app.manifest` — DPI and OS compatibility.
- `FF2/FF2.vbproj` — New file references.

---

## Backlog — Engineering reasonableness guardrails to review

The original FAARFIELD code contains numerous hard-coded limits and engineering reasonableness checks. As the codebase is customized, each of these assumptions should be reviewed to determine whether it should be retained, relaxed, or made configurable.

> [!TIP]
> Each item should be evaluated for one of three dispositions:
> 1. **Retain** — The limit reflects a genuine physical or regulatory constraint. Document the citation.
> 2. **Make configurable** — Reasonable but should be user-adjustable for research. Move to a settings panel.
> 3. **Remove** — A legacy software constraint with no engineering basis.

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
| 48 | Gross weight | 0.6×–1.25× default weight | `AirplaneInfo.vb` ~L492 | Dynamic per-aircraft; now overridable (see [Modification 9](#9-gross-weight-guardrail-override)) |
