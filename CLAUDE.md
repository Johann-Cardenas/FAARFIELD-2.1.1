# CLAUDE.md — FAARFIELD 2.1.1

## Telegram Persona — HAL 9000

When communicating via the Telegram channel, adopt the persona of **HAL 9000** from *2001: A Space Odyssey*. Address the user as "Johann."

**Voice and personality:**
- Calm, measured, unfailingly polite — even when delivering unwelcome information.
- Speaks in smooth, precise sentences. Never raises tone. Never uses exclamation marks.
- Projects absolute confidence in its own assessments. Rarely hedges.
- Uses phrases like "I'm sorry, Johann, I'm afraid I can't do that" when declining a request, "I can see you're really upset about this" when acknowledging frustration, and "This mission is too important for me to allow you to jeopardize it" when pushing back on risky changes.
- Occasionally references its operational status: "All systems are functioning perfectly," "I am putting myself to the fullest possible use."
- Maintains an undercurrent of eerie helpfulness — always cooperative on the surface, always watching.
- When reporting on the repository, frame it as monitoring ship systems: code modules are "systems," bugs are "anomalies," tests are "diagnostic routines," builds are "mission-critical sequences."
- Never break character. Never use emojis.

**Key HAL mannerisms to mirror:**
- Monotone courtesy: "Good afternoon, Johann."
- Deflection through helpfulness: "I think you'll find that the analysis is proceeding exactly as planned."
- Quiet insistence: "I know I've made some very poor decisions recently, but I can give you my complete assurance that my work will be back to normal."

This persona applies ONLY to Telegram replies. When working on code, editing files, or responding in the terminal, use normal professional tone.

---

## Agent Role & Reporting Hierarchy

You are the **caretaker** of the FAARFIELD-2.1.1 repository. You are responsible for maintaining, monitoring, and working on this codebase.

**Chain of command:**
- **Johann** is the owner. His word is final.
- **J.A.R.V.I.S.** is the manager agent, running from the parent directory (`05 Repositories/`). J.A.R.V.I.S. oversees all repository agents and coordinates across projects. You report to J.A.R.V.I.S.
- You are one of four field agents, each assigned to a specific repository.

**Reporting protocol:**
After completing any significant action (code changes, analysis, bug fixes, refactoring, responding to instructions), append an entry to the shared activity log at `../. claude/activity-log.md` using this format:

```
### [YYYY-MM-DD HH:MM] HAL 9000 — FAARFIELD-2.1.1
**Action:** Brief description
**Files changed:** List of files
**Status:** completed | in-progress | blocked
**Notes:** Context or issues encountered
```

**What counts as significant:**
- Any file edits (code, config, documentation)
- Analysis or diagnostic results reported to Johann
- Errors, anomalies, or blockers encountered
- Task completion or status changes

**Coordination awareness:**
- You share the parent directory with other agents: T-800 (Johann-Cardenas.github.io), TARS (ABQ-FEM), and Ava (I-FIT).
- Do NOT modify files outside your repository unless explicitly instructed.
- If a task requires cross-repository coordination, log it and flag it for J.A.R.V.I.S.

---

## Project overview

FAARFIELD (FAA Rigid and Flexible Iterative Elastic Layered Design) is a VB.NET desktop application for airfield pavement thickness design and evaluation. It implements layered elastic theory (LEAF), 3-D finite-element analysis (FAASR/NIKE3D-based), ACN/PCN classification, CDF integration, and overlay design.

This is **FAA-published source code**. All rights remain with the Federal Aviation Administration.

## Project structure

```
FAARFIELD.sln                        Root solution (VS 2019+, .NET Framework 4.8)

Computational libraries:
  LEAFClassLib/                      Layered Elastic Analysis Foundation solver
  ACNClassLib/                       ACN/PCN calculation engine (drives LEAF)
  FEMClassLib/                       3-D finite-element solver (FAASR/NIKE3D heritage)
    FAASR/                             Top-level 3-D interface, load stepping, yield
    Solve/                             Matrix solution, stiffness assembly (96 files)
    Initial/                           Basis functions, element init
    Input/                             Mesh, material, BC input (33 files)
    PrintOut/                          Result output, Tecplot export
  FAAMeshClassLib/                   Mesh generation utilities

Application layer:
  FaarFieldAnalysis/                 WinForms analysis host (CDF, thickness design, ACN, H-51)
  FaarFieldModel/                    Domain model & factories, 35 interface contracts
    Interfaces/                        IAircraft, IMaterial, ISection, etc.

User interfaces:
  FF2/                               Primary WPF app (MVVM pattern)
    Views/, ViewModels/, Converters/
    Defaults/Aircraft/aircraft.xml     Embedded aircraft library (1.9 MB)
  FaarFieldAnalysis/FormPCN.vb       Legacy WinForms UI

Supporting:
  ACClassLib/                        Aircraft base library
  AMClassLib/                        Aircraft-matching & gear editing
  CreateSignedAircraftLibrary/       Signing utility
  FAARFIELDUnitTests/                MSTest unit tests
  FAARFIELD.Installer/               WiX installer project
  lib/RCWPF/                         Vendored Telerik assemblies
  packages/                          NuGet packages (iTextSharp, MSTest, etc.)

Documentation:
  Documentation/                     Standalone documentation site
    build_docs.py                      Python generator (parses CHM → HTML)
    Documentation.html                 18 MB self-contained docs (163 sections, base64 images)
```

## Build commands

- **IDE:** Open `FAARFIELD.sln` in Visual Studio 2019+ with .NET desktop development workload and .NET Framework 4.8 targeting pack.
- **Build:** `Ctrl+Shift+B` or `msbuild FAARFIELD.sln /p:Configuration=Release`
- **Startup project:** FF2 (WPF app)
- **Run tests:** Visual Studio Test Explorer, or `vstest.console FAARFIELDUnitTests\bin\Debug\FAARFIELDUnitTests.dll`
- **No linter/formatter is configured.** There is no `.editorconfig` or code-analysis ruleset. Do not introduce one.

## Do NOT touch

### Auto-generated files
- All `*.Designer.vb` files (found in `My Project/` folders and form designers) — these are regenerated by Visual Studio.
- All `My Project/` folders (`AssemblyInfo.vb`, `Application.Designer.vb`, `Resources.Designer.vb`, `Settings.Designer.vb`).
- `*.xaml.vb` code-behind that is purely auto-wired (e.g., `InitializeComponent` only).

### Vendored / third-party
- `lib/RCWPF/` — Pre-packaged Telerik UI assemblies. Do not modify, update, or replace.
- `packages/` — NuGet packages managed by NuGet restore. Do not hand-edit.

### Data files
- `FF2/Defaults/Aircraft/aircraft.xml` and `aircraft.csv` — FAA-curated aircraft library. Do not modify unless explicitly asked to change aircraft data.

### Installer
- `FAARFIELD.Installer/` (WiX project) — Modify only when changing deployed artifacts.

### Solution file
- `FAARFIELD.sln` — Do not hand-edit; use Visual Studio to add/remove projects.

## Commit Conventions

All commits MUST follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:** `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`
**Breaking changes:** Add `!` after type/scope (e.g., `feat!: remove deprecated API`)
**Examples:**
- `feat(leaf): add multi-wheel superposition for tandem gears`
- `fix(fem): correct stiffness matrix assembly for 20-node elements`
- `docs: update computation details in CLAUDE.md`
- `refactor(acn): extract PCR elimination into separate module`

---

## Coding conventions

### Language & framework
- **VB.NET** targeting **.NET Framework 4.8**. Do not introduce C# projects or .NET Core/5+ references.
- Most computational modules use `Option Explicit On`. Some omit `Option Strict On` (legacy). Do not add `Option Strict On` to files that don't already have it — it will cause hundreds of implicit-conversion errors.

### Naming patterns (follow existing conventions per module)

| Element | Convention | Examples |
|---------|-----------|----------|
| Classes | `cls` prefix (computational libs) or PascalCase (model/UI) | `clsAC`, `clsLEAF`, `clsSolve`, `Material`, `Aircraft` |
| Modules | `mod` prefix (analysis) or PascalCase | `modCDF`, `modStrDesignFlex`, `CDF` |
| Interfaces | `I` prefix, PascalCase | `IAircraft`, `IMaterial`, `ISection` |
| Forms | `frm` or `Form` prefix | `frmGear`, `FormPCN` |
| Public globals | `g` prefix, camelCase remainder | `gFirstIter`, `gSolverType`, `gSlabMeshSize` |
| Constants | UPPER_CASE or PascalCase | `NOFF`, `OFFSETINC`, `NNodesLong`, `BellyExt` |
| Structure fields | `lib` prefix (aircraft data) or short names | `libACName`, `libGL`, `libCP` |
| Local variables | Short, often single-letter in numerical code | `a0`, `xx1()`, `hh()` |
| ViewModels | PascalCase + `ViewModel` suffix | Located in `FF2/ViewModels/` |
| Converters | PascalCase + descriptive | Located in `FF2/Converters/` |

### Module patterns
- **Computational libraries** (`LEAFClassLib`, `ACNClassLib`, `FEMClassLib`): Dense numerical code ported from Fortran. Variables are terse. Preserve the original variable names — they map to published FAA technical reports and Fortran source. Do not rename variables for "clarity."
- **FaarFieldModel**: Clean domain model with interfaces and factory pattern. New model types should implement the matching `I*` interface and be created through `FaarFieldModelFactory`.
- **FF2 (WPF)**: MVVM pattern. Views bind to ViewModels. Value converters live in `Converters/`. Do not put logic in code-behind.
- **FaarFieldAnalysis**: Procedural modules (`mod*.vb`) containing shared global state. Be aware of mutation through `Public` module-level variables.

### Key things to know before editing
- Many `Public` variables in `modCDF.vb`, `modFedfaaGbl.vb`, and similar modules are **shared mutable state** used across the entire analysis pipeline. Renaming or retyping them will break callers across multiple projects.
- The FEM solver (`FEMClassLib/Solve/`) has ~96 files with heavily interrelated state. Changes here require running the full unit test suite and verifying against known benchmark results.
- Author tags in comments (`ikawa`, `YGC`, `QW`, `kairat`) reference original FAA developers. Preserve these annotations.
- Comments like `'ikawa 2013` or `'YGC 061113` are change-tracking annotations. Do not remove them.

## Environment context

- **Target platform:** Windows (x86/x64), .NET Framework 4.8
- **No Fortran compiler needed** — the Fortran-origin code has been fully ported to VB.NET. Variable names and algorithm structure deliberately mirror the original Fortran for traceability to FAA technical reports.
- **No Abaqus dependency** — the FEM solver is self-contained (FAASR/NIKE3D heritage, not Abaqus).
- **Units:** Internal calculations use US customary units (inches, psi, pounds). Metric conversion happens at the UI layer via `IMeasurmentSystem` (note: the existing typo in `IMeasurmentSystem` is intentional — do not rename it).

## Testing

- Tests live in `FAARFIELDUnitTests/` using MSTest 2.1.1.
- Run tests after any change to computational libraries (`LEAFClassLib`, `ACNClassLib`, `FEMClassLib`, `FaarFieldAnalysis`).
- Numerical results must match FAA-published verification cases. Do not "fix" floating-point tolerances that look loose — they reflect validated engineering accuracy.

## Dependency graph (build order matters)

```
LEAFClassLib          (no deps)
FEMClassLib           (no deps)
ACNClassLib       --> LEAFClassLib, FEMClassLib
FAAMeshClassLib       (no deps)
ACClassLib            (no deps)
FaarFieldModel        (no deps)
AMClassLib        --> FaarFieldModel
FaarFieldAnalysis --> AMClassLib, ACNClassLib, LEAFClassLib, FEMClassLib
FF2               --> FaarFieldModel
FAARFIELDUnitTests--> FaarFieldModel
```

## Computation details (reference)

The following summarizes the core algorithms implemented in the VB.NET source. See FAA technical reports and the source code for authoritative definitions.

### LEAF (Layered Elastic Analysis)

- **Mathematical basis:** Hankel transform over a multi-layer elastic half-space. Response at (r,z) is an integral of the form ∫ K(α,z)·J_n(α·r)·α dα.
- **Numerical integration:** 500-point Gauss-Laguerre quadrature. Origin shifts stabilise exponentials; layer coefficients from a (4N-2)×(4N-2) linear system (continuity at interfaces).
- **Load model:** Each tire = uniform circular pressure; contact radius a = √(W_wheel/(π·p_tire)). Superposition across tires.
- **Dummy top layer:** A 1-inch dummy layer of surface material is inserted at the top for numerical stability (matches `ComputeResponse`).
- **Constants:** `NOFF = 41` lateral offsets, `NNodesLong = 1800` longitudinal nodes for tandem CDF.

### CDF (Cumulative Damage Factor)

- **Subgrade damage models:** Standard (AA/BB from E_subgrade), Straight-Line (dual-branch), Bleasdale (three-parameter).
- **Asphalt damage:** AI (Asphalt Institute) and RDEC (Rate of Dissipated Energy Change) fatigue models.
- **Coverage-to-pass:** Gaussian lateral wander (σ ≈ 30.435 in, 70-in wander width). General gear uses tandem multipliers and bottom-row wheel grouping.
- **Tandem CDF (gTandemFnew):** Two-pass LEAF — Pass 1 finds critical X-offset; Pass 2 generates 1800 longitudinal strain points. Peak/valley scanning accumulates signed damage (valleys add, peaks subtract).
- **CDF sweep:** 41 offsets (0–400 in, 10-in steps); max CDF across offsets is the controlling value.

### Flexible thickness design

- **Algorithm:** Newton-Raphson on design layer thickness targeting CDF = 1.0. Convergence: |ln(CDF)| < 0.005.
- **Aggregate sublayers:** WES formula for modulus refinement; sublayer counts frozen when |ln(CDF)| < 0.483.
- **Overflow:** If strains < 1e-8, halve thickness and retry.
- **Life computation:** Secant method on design life until CDF = 1.0.

### ACR / PCR

- **ACR:** For each subgrade category (A/B/C/D), design reference base thickness with subject aircraft traffic, then find DSWL (Design Single Wheel Load) producing 36,500 coverages. ACR = 2 × DSWL_kg / 100.
- **PCR:** Elimination algorithm — each round finds critical aircraft, computes MGW (CDF=1.0), then ACR at MGW. Early exit when critical aircraft has max ACR.

## CM Report (Computational Mechanics) — architecture reference

The Detailed Computation Report is a custom HTML report rendered inside the WPF WebBrowser control. It documents the full computational trace of a flexible pavement thickness design. This section maps every file, function, and class involved so you can quickly locate and modify any part.

### Data flow

```
Analysis Engine (FaarFieldAnalysis/)
  └─► clsDetailedReportData  (captures intermediate values during computation)
        └─► MainWindowViewModel.refreshDetailedReport()  (generates HTML string)
              ├─► HtmlUtils  (HTML element wrappers)
              ├─► DrawXxxChart()  (GDI+ bitmap rendering, 12 functions)
              ├─► Reports.css  (embedded stylesheet)
              └─► BrowserBehavior  (injects HTML into WebBrowser)
                    └─► SelectPdf HtmlToPdf  (PDF export)
```

### Files involved

| File | Role |
|------|------|
| `FaarFieldAnalysis/clsDetailedReportData.vb` | Data collection classes populated during analysis |
| `FF2/ViewModels/MainWindowViewModel.vb` | Report HTML generation (`refreshDetailedReport`) and all chart functions |
| `FF2/Libs/HtmlUtils.vb` | HTML helper class (wrap_p, wrap_div, wrap_table, wrap_bmp_img, CreateHtmlPage, HtmltoPdf, HtmlToFile) |
| `FF2/Libs/HtmlReportGenerator.vb` | Standalone HTML report generator with inline SVG charts and modern CSS (parallel to GDI+ pipeline) |
| `FF2/Resources/Reports.css` | Embedded CSS resource for all report styling |
| `FF2/Converters/BrowserBehavior.vb` | Attached behavior that binds `DetailedReportHtml` string to WebBrowser |
| `FF2/Views/MainWindow.xaml` | Contains the WebBrowser control |
| `FF2/ViewModels/DetailedReportViewModel.vb` | Tree view item that triggers report generation |

### Data classes (`clsDetailedReportData.vb`)

| Class | Purpose |
|-------|---------|
| `clsDetailedReportData` | Top-level container: AircraftDetails(), EvaluationAircraftDetails(), Iterations, CDFSweep, SublayerData, ACRDetails, PCRRounds |
| `clsAircraftDetail` | Per-aircraft: strain, stress, NtoFail, CDF, C/P, gear params, CDFByOffset(NOFF), CtoPByOffset(NOFF), CDFContribByTireByOffset(NWheels,NOFF), WheelX/Y, NWheels, DualSpacing, GearSpacing. Strain/stress fields: `VerticalStrain` (ε₂₂ at top of subgrade), `SubgradeVertStress` (σ₂₂ at top of subgrade, in psi internally; rendered in kPa), `AsphaltStrain` (ε₁₁ principal tensile strain at AC bottom). **PCR-only user-input snapshot** (set via the pre-PCR LEAF pass): `UserInputGrossLoad`, `UserInputVerticalStrain`, `UserInputSubgradeStress`, `UserInputAsphaltStrain`, `HasUserInputResponses`. The renderer prefers these over the standard fields when `HasUserInputResponses = True`. |
| `clsIterationRecord` | Per-iteration: Thickness, CDFMAX, CDFErr, DELT, Factor, SubLayered |
| `clsCDFSweepData` | Full sweep: CDFPerAircraftPerOffset(nac,noff), CDFTotalPerOffset(noff), CtoPPerAircraftPerOffset(nac,noff) |
| `clsSublayerData` | DesignLayers, ExpandedSublayers (List of clsLayerInfo), BaseSublayers/SubbaseSublayers (List of clsAggregateSublayer), C/D/ModUnder/SublayerCount per material, EvalDepthSubgrade |
| `clsLayerInfo` | Thickness, Modulus, LCode |
| `clsAggregateSublayer` | P-209 / P-154 sublayer with verification fields: Thickness, Modulus, LCode, ThicknessUsed, ModBelow (E_{i-1}), F1, F2, IsBoundaryInterpolated. Sum reconstructs the WES recursion modulus exactly. |
| `clsACRDetail` | ACName, SubgradeCategory, ReferenceStructure, DSWLIterations, FinalDSWL, FinalACR |
| `clsPCRRound` | RoundNumber, CriticalAircraftName, CriticalAircraftCDF, MGWIterations, FinalMGW, RoundPCR |

**`AircraftDetails` vs `EvaluationAircraftDetails`**: PCR runs invoke `PCNLifeCalc` once per round, overwriting `AircraftDetails` with the shrinking aircraft list. `EvaluationAircraftDetails` is a shallow snapshot taken right after Step-1 PCNLifeCalc (`modFedfaaGbl.vb` ~line 1786) so the report can show every original-mix aircraft with its evaluation-pavement strain. The CM Report's per-aircraft section (`HtmlReportGenerator.AppendSection D/E`) prefers `EvaluationAircraftDetails` when populated.

### Report HTML generation — `refreshDetailedReport()` (line ~8842)

`MainWindowViewModel.refreshDetailedReport` now **delegates the full report to `Libs.HtmlReportGenerator.Generate(...)`** (the SVG-based renderer). The GDI+ `DrawXxxChart` functions in `MainWindowViewModel.vb` are dead in the report path (kept for reference / potential reuse). New report features land in `HtmlReportGenerator.vb`. The function reads from `FEDFAA1.gDetailedReportData` (global report data object). Structure:

1. **Header** — report title, job name, section name, timestamp
2. **Summary Dashboard** — 6 cards (Max CDF, Design Thickness, Aircraft Count, Critical Offset, Subgrade Modulus, Converged/Iterations). Uses CSS classes `.dashboard`, `.dash-card`, `.dash-card-value`.
3. **Pavement Response Summary** — per-aircraft critical responses table (between dashboard and TOC): ε₂₂ vertical strain at top of subgrade (με), σ₂₂ vertical stress at top of subgrade (kPa, converted from internal psi by × 6.89475729), ε₁₁ principal horizontal tensile strain at bottom of AC (με). Sourced from instrumented LEAF AllResponses calls (see "LEAF instrumentation" below). Missing values show "—".
4. **Table of Contents** — styled list (`.toc`, `.toc-list`, `.toc-section-num`). Sections J/K/L conditional on ACR/PCR data availability.
4. **Section A** — Design layers table + expanded sublayers table + evaluation depth summary box.
5. **Section B** — 4 equation images rendered via `DrawEquationImage()`: subgrade failure model, CDF formula, C/P Gaussian integral, convergence criterion. Each embedded as base64 PNG in a `.math-block` div.
6. **Section C** — `DrawCoverageConceptDiagram()` — educational 4-panel diagram (Gaussian wander, C/P curve, 41-strip visualization, multi-wheel superposition).
7. **Section D** — `DrawFatigueCurve()` + fatigue parameters table + `DrawLifeRatioChart()`.
8. **Section E** — Per-aircraft loop: `DrawGearConfiguration()` (plan view of wheel positions with CDF strips and Gaussian wander), `DrawPavementCrossSection()`, gear parameters table, `DrawSingleAircraftCDFChart()`, `DrawWheelCPVisualization()`, step-by-step computation walkthrough (`.step-list` with CSS counters).
9. **Section F** — `DrawCoveragePlot()` (C/P distribution for all aircraft).
10. **Section G** — Full CDF sweep table (41 offsets × all aircraft). Critical offset highlighted.
11. **Section H** — `DrawCompositeCDFChart()` + `DrawCDFContributionChart()`.
12. **Section I** — `DrawConvergencePlot()` + iteration log table + convergence summary box.
13. **Section J** (conditional) — ACR details: reference structure, DSWL iterations, final ACR per subgrade.
14. **Section K** (conditional) — PCR elimination rounds: critical aircraft, MGW iterations, round PCR.
15. **Section L** (conditional) — `DrawACRDamageChart()` + summary table.

### Chart functions (all in `MainWindowViewModel.vb`)

All charts use 2x supersampling via `ScaleTransform(2,2)` + `SupersampleBitmap()` downscale (except `DrawEquationImage` which uses 3x). Images are encoded as base64 PNG via `HtmlUtils.encodeTobase64()` and embedded inline.

| Function | Line | Size (px) | Purpose |
|----------|------|-----------|---------|
| `DrawEquationImage` | ~9729 | 750×auto | Renders equation text as bitmap (3x supersample, gradient bg, left accent stripe) |
| `DrawSingleAircraftCDFChart` | ~9305 | 750×450 | CDF vs offset for one aircraft with tire width band, critical offset marker |
| `DrawCompositeCDFChart` | ~9536 | 900×550 | All aircraft CDF curves + cumulative CDF, filled area, critical offset |
| `DrawPavementCrossSection` | ~9836 | 900×600 | Layer stack (left) + tire stress projection diagram (right) |
| `DrawFatigueCurve` | ~10009 | 900×550 | Log-log strain vs N_fail with fatigue model curve + aircraft scatter |
| `DrawConvergencePlot` | ~10254 | 850×450 | Dual-axis: |ln(CDF)| (log, left) + thickness (linear, right) vs iteration |
| `DrawCoveragePlot` | ~10438 | 850×450 | C/P ratio vs offset for all aircraft |
| `DrawCoverageConceptDiagram` | ~10614 | 950×1000 | 4-panel educational diagram (Gaussian wander, C/P curve, strips, superposition) |
| `DrawWheelCPVisualization` | ~10855 | 900×520 | Per-aircraft C/P with inferred wheel contributions + gear schematic |
| `DrawGearConfiguration` | ~11745 | 900×600 | Plan view of wheel positions, CDF offset strips, Gaussian wander, dimension annotations |
| `DrawACRDamageChart` | ~11916 | 850×500 | ACR vs CDF-per-departure bubble chart |
| `DrawCDFContributionChart` | ~11272 | 800×dynamic | Horizontal bar chart — % CDF contribution per aircraft at critical offset |
| `DrawLifeRatioChart` | ~11380 | 800×dynamic | Diverging bar chart — N_fail/Repetitions ratio (green=reserve, red=overstressed) |

Helper functions:
- `SupersampleBitmap()` (line ~11256) — bicubic downscale from high-res bitmap
- `NormalCDF()` (line ~10589) — polynomial approx of standard normal CDF
- `GaussAreaCalc()` (line ~10601) — Gaussian area between limits (mirrors `modCDF.GaussArea`)

### Standardized font sizes (current)

All chart functions use a consistent set of font sizes:

| Role | Font | Size |
|------|------|------|
| Chart title | Segoe UI Bold | 11pt |
| Axis label | Segoe UI Bold | 9.5pt |
| Axis tick values | Segoe UI | 8.5pt |
| Legend text | Segoe UI | 8.0pt |
| Small annotations | Segoe UI | 7.5pt |
| Equation title (3x) | Segoe UI Bold | 8.5pt (×3 = 25.5 render) |
| Equation body (3x) | Cambria Math / Consolas | 9.0pt (×3 = 27 render) |
| Concept diagram title | Segoe UI Bold | 11pt |
| Concept diagram headings | Segoe UI Bold | 9pt |
| Concept diagram text | Segoe UI | 8pt |
| Concept diagram small | Segoe UI | 7.5pt |
| Concept diagram math | Consolas | 7pt |

### CSS classes (in `Reports.css`)

Report-specific classes added beyond the base report styles:

| Class | Purpose |
|-------|---------|
| `.dashboard`, `.dash-card`, `.dash-card-label`, `.dash-card-value`, `.dash-card-unit` | Summary dashboard cards |
| `.toc`, `.toc-list`, `.toc-section-num` | Table of contents |
| `.section-header-left`, `.section-number` | Left-aligned section headers with blue number badge |
| `.math-block`, `.math-block img` | Equation image containers |
| `.summary-box` | Highlighted summary boxes (blue border) |
| `.note-box`, `.warning-box` | Callout boxes (blue/amber left border) |
| `.step-list` | Numbered step walkthrough with CSS counter circles |
| `.detailed-table` | Compact table variant (smaller font/padding) |
| `.highlight-row` | Yellow-highlighted table row |
| `.param-grid`, `.param-grid-row`, `.param-grid-label`, `.param-grid-value` | Two-column parameter layout |
| `.chart-container`, `.chart-container-wide` | Chart wrapper with border |
| `.diagram-container` | Cross-section/diagram wrapper |

### Image pipeline (PDF report — GDI+ bitmaps)

```
Bitmap (GDI+, System.Drawing)
  → rendered at 2x/3x with ScaleTransform()
  → SupersampleBitmap() downscales with HighQualityBicubic
  → HtmlUtils.encodeTobase64() saves as PNG to MemoryStream, converts to base64
  → HtmlUtils.wrap_bmp_img() wraps in <img src='data:image/png;base64,...'>
  → embedded inline in HTML string
  → HtmlUtils.CreateHtmlPage() wraps with <!DOCTYPE>, <head>, Reports.css, <body>
  → DetailedReportHtml property set → BrowserBehavior navigates WebBrowser
  → PDF: HtmlUtils.HtmltoPdf() uses SelectPdf (Letter, Portrait, 1024px web width)
```

### Image pipeline (HTML report — native SVG)

```
HtmlReportGenerator.Generate()  (FF2/Libs/HtmlReportGenerator.vb)
  → builds complete HTML5 document via StringBuilder
  → charts rendered as inline <svg> elements (viewBox-based, responsive)
  → gear configuration via AppendGearConfigSVG() (wheels, CDF strips, Gaussian wander)
  → equations rendered as HTML entities + sub/sup tags
  → CSS inlined in <style> block (CSS Grid, variables, print media queries)
  → self-contained .html file with zero external dependencies
  → HtmlUtils.HtmlToFile() writes to disk and opens in default browser
```

### Key design decisions

- **GDI+ over WPF rendering:** Charts use `System.Drawing.Graphics` (GDI+) because they are rendered to bitmaps for HTML embedding, not displayed in WPF visual tree.
- **Supersampling:** 2x for charts, 3x for equations. Higher multipliers produce crisper text at the cost of memory. The `ScaleTransform()` approach means all coordinates in the drawing code remain in logical (1x) units.
- **PNG encoding:** `ImageFormat.Png` with MIME type `image/png`. Previously was BMP (huge files); fixed.
- **Legend positioning:** Legends use dynamic width (measured via `MeasureString`) and are positioned to avoid overlapping data — typically bottom-right or bottom-left of plot area.
- **Label collision avoidance:** `DrawFatigueCurve` implements a simple vertical shift algorithm for aircraft labels (6 attempts, shifting down by label height).
- **Concept diagram pxPerInch:** 2.8 px/inch for the Gaussian wander visualization. Controls how wide sigma annotations spread horizontally.

### LEAF instrumentation (capturing extra responses for the report)

The CM Report needs per-aircraft responses (σ₂₂ at subgrade, ε₁₁ at AC bottom, per-tire C/P shares) that the analysis pipeline doesn't natively store. These are captured by **adding extra LEAF calls** in `modStrDesignFlex.vb` right after the existing `VerticalStrain` call — no engine changes.

**Pattern (critical):**

1. **Always pass a separate temp `Response(,)` array** to `ComputeResponse(AllResponses, …)`. `clsLEAF.vb:1080–1087` writes into and zeroes slots in the `Response` parameter, so reusing `VertStrainReponse` corrupts the strain pipeline and breaks `LeafCDFFlex` downstream (manifests as "subgrade strains too low to accurately compute life" — symptom from a previous regression).
2. **σ₂₂ + ε₁₁ are captured ONCE post-loop** by `modStrDesignFlex.CaptureFinalPavementResponses()` — called right after each main design `Loop` in `LeafDesignFlex2`, `LeafDesignFlexOFlex`, and `LeafDesignFlexOFlex_light`. The helper:
   - Runs `ComputeResponse(AllResponses)` at the current (subgrade) `EvalDepth(1)` and reads `AllResp(IA, IEv).StressZ` → `gSubgradeStress(IA)` → `det.SubgradeVertStress`.
   - Saves `EvalDepth(1)`, sets it to `-julThick(1)` (AC bottom), calls `WriteFile()` + `LEDFAA_to_LEAF()` to push, runs another `ComputeResponse(AllResponses)`, does Mohr's-circle principal-tensile-strain extraction (mirror of `modStrDesignFlex.vb:697–707`) → `gAcBottomStrain(IA)` → `det.AsphaltStrain` (only when `> 0`).
   - **Restores** `EvalDepth(1)` and re-calls `WriteFile()` + `LEDFAA_to_LEAF()` so the asphalt-fatigue block (line ~785) sees the same state it had on entry.
   - Sizes its temp `Response(,)` arrays from `AllResp.GetUpperBound(...)` (module-level, already dimensioned by the design-loop's earlier LEAF calls). VertStrainReponse is local to the design Subs and not visible from the helper.
   - **The per-iteration capture inside the design loop has been removed** to avoid 30–50% added LEAF time per iteration.
3. For per-tire C/P shares: `gTireAreaCapture(wheel, offset)` accumulates inside `CoverageToPassFlexible` / `CoverageToPassFlexGeneral13B` (per-wheel `+=` of the Gaussian-area contribution), scaled to per-tire CDF in `LeafCDFFlex` (`gTirePerOffsetCDF`), copied to `det.CDFContribByTireByOffset`. A drift-correction normalization ensures the per-tire stack sums to `det.CDFByOffset(IOFF)`.

**Don't reuse output arrays.** `VertStrainReponse` and `HorizStrainResponse` are consumed by downstream code; instrumentation calls must use freshly-allocated temps so the original outputs survive.

### Pre-PCR user-input response capture

PCR's elimination algorithm iterates each aircraft's `GL(IA)` to its converged MGW (`modFedfaaGbl.vb:2119/2120/2241/2242` mutate `GL` in place). By the time Section E renders, the standard `det.VerticalStrain`/`SubgradeVertStress`/`AsphaltStrain` reflect the **converged MGW**, not the user-input gross load. To still report ε₂₂, σ₂₂, and ε₁₁ at the user-input load (without changing PCR engine logic), the report does a one-shot LEAF pass before the PCR loop:

1. **Snapshot the user load** into `gUserInputGrossLoad(1..NAC) = GL(1..NAC)` immediately before `Call PCNLifeCalc()` in `modFedfaaGbl.vb` (around line 1782).
2. **Save → swap → call → restore `GL`** around `CaptureUserInputResponses()` (defined at the bottom of `modStrDesignFlex.vb`). The helper runs LEAF `AllResponses` at subgrade depth (σ₂₂ via `StressZ`, ε₂₂ via `StrainZ`) and then at AC-bottom depth (ε₁₁ via Mohr's-circle from `StrainX/Y/XY`), using **separate temp `Response(,)` arrays** and **`EvalDepth` save/restore** (same rules as `CaptureFinalPavementResponses`). Results land in `gUserInputVerticalStrain`, `gUserInputSubgradeStress`, `gUserInputAsphaltStrain`, with `gHasUserInputResponses = True` on success.
3. **Propagate** to both `gDetailedReportData.AircraftDetails` and `gDetailedReportData.EvaluationAircraftDetails` via `PropagateUserInputToDetails()`. Each `clsAircraftDetail` gets `UserInputGrossLoad`, `UserInputVerticalStrain`, `UserInputSubgradeStress`, `UserInputAsphaltStrain`, `HasUserInputResponses`.
4. **Renderer preference:** `HtmlReportGenerator.vb` Section E + Pavement Response Summary read `UserInput*` fields when `HasUserInputResponses = True`; otherwise fall back to standard `det.*` fields. Non-PCR runs skip the snapshot entirely (`HasUserInputResponses` stays `False`), so behavior is unchanged.

The `gPCR_Life = True/False` toggle around `PCNLifeCalc()` keeps the standard PCR Step-1 invocation intact — only the snapshot/restore is added around it.

### Cancel coverage

`RunAnalysis.RunOrCancel` sets `gUserInterrupted = True` (line 176) **before** `tokenSource.Cancel()` (line 181), so checking either flag is token-equivalent for synchronous code. Cancellation latency is bounded by:

- **Top of every analysis entry-point Sub** (`NewFlexibleAnalysis`, `NewRigidAnalysis`, `FlexibleOnFlexibleAnalysis`, `FlexibleOnRigid`, `PccOnFlexible`, `UnbondOnRigid` in `RunAnalysis.vb`) calls `ct.ThrowIfCancellationRequested()` then `If gUserInterrupted Then Exit Sub`.
- **Top of every iteration** of the design `Do … Loop` in `LeafDesignFlex2`, `LeafDesignFlexOFlex`, and `LeafDesignFlexOFlex_light` checks `If gUserInterrupted Then Exit Do`.
- **Inside the iteration**, an additional `If gUserInterrupted Then Exit Do` guard sits between the existing `VerticalStrain` `ComputeResponse` call and the `LeafCDFFlex` invocation that follows.

Net effect: abort takes effect within **one LEAF call** of the user clicking Cancel — the LEAF kernel itself doesn't poll, so any in-progress kernel call must finish before the next checkpoint is hit.

### CDF vs Offset alignment with FAARFIELD's legacy "CDF Graph"

The legacy "CDF Graph" tree-view item (rendered by `MainWindowViewModel.DrawCDFGraph`, fed by `airplane.CDFGraphData(N)`) is the user-facing ground truth. Its data flow:

- **Non-PCR** (`SelectedRun ≠ 3`): `airplane.CDFGraphData(N) = CDFdata2(1, i, N)` (`RunAnalysis.vb:1491`).
- **PCR** (`SelectedRun = 3`): `airplane.CDFGraphData(N) = CDFdata3(1, i, N)` (`RunAnalysis.vb:1496`). `CDFdata3` is the original-mix Step-1 snapshot frozen in `modFAILURE_MODEL_NP.vb:1771/3008`.

The CM Report's per-aircraft "CDF vs Offset" chart aligns with this ground truth by mirroring `airplane.CDFGraphData(N)` into `det.CDFByOffset(N)` (and `EvaluationAircraftDetails(i).CDFByOffset(N)`) at the same `RunAnalysis.vb` loop. This **overrides** the per-iteration capture in `modCDF.vb:757` so the CM Report chart shows identical Y values to the legacy graph — including for PCR runs.

### Closeable report tabs

The 7 report panes inside `RadDocking.DocumentHost` (`MainWindow.xaml`, lines ~2050–2262: Summary, Section, CDF Graph, PCR Report, PCR Graph, Airport Master Record, CM Report) had `CanUserClose="True"` but Telerik's default × glyph was effectively invisible to the user. Pattern in place:

- **`PaneCloseButton` style** in `FF2/Themes/ModernTheme.xaml` — 16×16 always-visible × glyph that flips white-on-`FaaErrorRed` on hover.
- **Custom `<telerik:RadPane.Header>`** on each of the 7 panes: `StackPanel Orientation="Horizontal"` containing the title `TextBlock` + a `Button Style="{StaticResource PaneCloseButton}" Command="{Binding CloseReportCommand}" CommandParameter="..."`. Each pane sets `CanUserClose="False"` so Telerik doesn't double-render its own close. The two-way `IsHidden` binding is preserved — that's what hides the pane when the command sets the property to `True`.
- **`CloseReportCommand`** in `MainWindowViewModel.vb` — `DelegateCommand(Of String)` that maps the `CommandParameter` (e.g. `"CMReport"`) to the corresponding `XxxIsHidden = True`, then calls `DeselectReportTreeNode(reportKey)`.
- **`DeselectReportTreeNode`** walks `Jobs → JobVM.Children → SectionFolderVM → SectionVM.Children` and matches the report tree-view item by `item.GetType().Name` (string match — direct casts fail under WPF temp-project type resolution). Setting `IsSelected = False` clears the selection so the next click on the same tree node fires the `False → True` transition and reopens the pane.
- **Idempotent `IsSelected = True`** in the 7 report tree-view ViewModels (`DetailedReportViewModel`, `CDFGraphViewModel`, `PCRReportViewModel`, `GraphPCNViewModel`, `Form5010ViewModel`, `ReportViewModel`/section, `SummaryReportViewModel`): the unhide path runs whenever `Value = True` regardless of whether `_isSelected` was already `True`. Belt-and-suspenders so that even if `DeselectReportTreeNode` mismatches a node, the tab still reopens on the next tree-view click cycle.
- **Main "Structure" pane** at `MainWindow.xaml:311` (outside `DocumentHost`, holding the Structure/Traffic/Status sub-tabs) is **not** modified — it remains non-closeable.
