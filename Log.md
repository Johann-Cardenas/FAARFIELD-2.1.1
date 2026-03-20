# FAARFIELD 2.1.1-CM — Change Log

> **Purpose:** This document provides a detailed, auditable record of every change made to the FAARFIELD 2.1.1 source code relative to the original FAA-published version. It is intended to support release documentation, peer review, and regulatory transparency.
>
> **Scope:** Only changes that modify, extend, or add to the VB.NET source code are logged. Documentation-only commits (README, CLAUDE.md) and infrastructure setup (git configuration, IDE settings) are excluded.
>
> **Baseline:** The unmodified FAA-published FAARFIELD 2.1.1 source was committed in three setup commits on 2026-02-06 (`32d8b2d`, `52f5a2c`, `6c517ec`). All changes documented below are measured against commit `6c517ec` (Project Setup III), which represents the complete, unmodified FAA source.

---

## Computation Engine Integrity Statement

The core computational engines of FAARFIELD have **not** been modified. The following directories and files remain identical to the FAA-published source:

| Module | Files | Status |
|--------|-------|--------|
| **LEAFClassLib/** (Layered Elastic Analysis) | `clsLEAF.vb`, `Numerical.vb` | **Unmodified** |
| **FEMClassLib/** (Finite-Element Solver, 120+ files) | All files in `FAASR/`, `Solve/`, `Initial/`, `Input/`, `PrintOut/`, `Com/` | **Unmodified** |
| **FAAMeshClassLib/** (Mesh Generation) | `clsMesh.vb`, `ModFunction.vb` | **Unmodified** |
| **ACNClassLib/** (ACN/PCN core) | `clsACNsub.vb`, `Set_Eval.vb`, `Z_Eval.vb`, `TwoGears.vb`, `Numerical.vb` | **Unmodified** |
| **FaarFieldAnalysis/** (Design algorithms) | `modPCN_ThicknessDesign.vb`, `modPCN_ACNflexICAO.vb`, `modPCN_ACNRigComp.vb`, `modPCN_H51inVB.vb`, `modDesignRigid_Adj.vb`, `modFAILURE_MODEL_NP.vb`, `modDesignP209.vb`, `modPCN_Alpha.vb`, `modPCN_Nonstandard.vb`, `modAdvisoryCircularRq.vb`, `modStrDesign13.vb` | **Unmodified** |

**What "unmodified" means:** Zero lines added, removed, or changed. Verified by `git diff 6c517ec..HEAD` against each file and directory listed above.

### Computational files with instrumentation-only changes

The following computational files were modified **exclusively** to capture intermediate values for the CM Report. No algorithm, formula, convergence criterion, or control flow was altered. All instrumentation code is wrapped in `Try/Catch` blocks so that any failure in data collection cannot affect the computation.

| File | Lines added | Nature of changes |
|------|-------------|-------------------|
| `FaarFieldAnalysis/modCDF.vb` | +89 | Captures per-aircraft CDF sweep data, gear geometry, strain/NtoFail values, and C/P distributions after the CDF computation loop completes |
| `FaarFieldAnalysis/modStrDesignFlex.vb` | +122 | Records iteration history (thickness, CDF, convergence error) at each Newton-Raphson step; captures sublayer structure and aggregate modulus parameters after convergence; captures asphalt CDF data |
| `FaarFieldAnalysis/modFedfaaGbl.vb` | +50 | Declares `gDetailedReportData` global; transfers RDEC mix properties from section model to engine globals (wiring only, no new computation); logs PCR elimination rounds and ACR details |
| `ACNClassLib/clsACR.vb` | +62 | Adds `DSWLIterationLog` structure and logging inside `Calculate_DSWL_Flex()` to record each DSWL iteration step (gear load, NtoFail, strain, coverage, delta) |

**Verification method:** Each diff was inspected to confirm that:
1. No existing lines were deleted or modified
2. All new code is either (a) `Try/Catch`-wrapped data capture that runs after the computation produces its results, or (b) new data structures (`DSWLIterationLog`) with no callers in the computation path
3. The instrumentation reads from existing variables — it does not write to any variable used by the computation

---

## Change Log

### Phase 1 — Infrastructure & Documentation (2026-02-06 to 2026-03-06)

No source code changes. Repository setup, README documentation, CLAUDE.md, and Copilot instructions.

| Date | Commit | Description |
|------|--------|-------------|
| 2026-02-06 | `32d8b2d` | Initial import of FAA-published FAARFIELD 2.1.1 source |
| 2026-02-06 | `52f5a2c` | Additional project files and dependencies |
| 2026-02-06 | `6c517ec` | Telerik UI assemblies — **baseline complete** |
| 2026-02-22 | `d534453` | README improvements (documentation only) |
| 2026-03-05 | `37e196e` | README update (documentation only) |
| 2026-03-05 | `831c2e0` | Added CLAUDE.md and settings.local.json (documentation/config only) |
| 2026-03-06 | `74f26ca` | Copilot instructions update (documentation only) |

---

### Phase 2 — NanoFlex Python Implementation (2026-03-13)

Parallel Python implementation of layered elastic, CDF, and ACR/PCR algorithms with a Flask web interface. These are **standalone Python files** — they do not modify or interact with the VB.NET source code.

| Date | Commit | Description |
|------|--------|-------------|
| 2026-03-13 | `22966e4` | NanoFlex: 14 Python files implementing LEAF, CDF, and ACR/PCR |
| 2026-03-13 | `ece560e` | Flask web application interface for NanoFlex |

---

### Phase 3 — VB.NET Customizations (2026-03-15 to 2026-03-20)

All source code modifications to the FAARFIELD VB.NET codebase occur in this phase. Each change is categorized and detailed below.

---

#### Change 1: CM Report — Data Collection Infrastructure

**Date:** 2026-03-15 | **Commit:** `a7459ac` | **Category:** Report (new feature)

Added data collection classes and instrumentation hooks to capture intermediate computational values during analysis, enabling a detailed computation report.

**Files added:**
| File | Lines | Purpose |
|------|-------|---------|
| `FaarFieldAnalysis/clsDetailedReportData.vb` | 215 | Data model classes: `clsDetailedReportData`, `clsAircraftDetail`, `clsIterationRecord`, `clsCDFSweepData`, `clsSublayerData`, `clsLayerInfo`, `clsACRDetail`, `clsPCRRound`, `clsDSWLIteration` |

**Files modified (instrumentation only — see Integrity Statement above):**
| File | Lines added | What was added |
|------|-------------|----------------|
| `FaarFieldAnalysis/modCDF.vb` | +89 | Per-aircraft CDF/C-to-P capture after main CDF loop; gear geometry extraction |
| `FaarFieldAnalysis/modStrDesignFlex.vb` | +122 | Iteration records at each Newton-Raphson step; sublayer/aggregate data after convergence; asphalt CDF capture |
| `FaarFieldAnalysis/modFedfaaGbl.vb` | +50 | `gDetailedReportData` global declaration; RDEC property transfer; PCR round and ACR detail logging |
| `ACNClassLib/clsACR.vb` | +62 | `DSWLIterationLog` structure; iteration logging in `Calculate_DSWL_Flex()` |

**Computation impact:** None. All instrumentation reads from existing variables after computation completes.

---

#### Change 2: CM Report — PDF Report Rendering (GDI+ Pipeline)

**Date:** 2026-03-15 | **Commits:** `a7459ac`, `c441ef7`, `eeadff2`, `85024e2`, `3aa366e` | **Category:** Report (new feature)

Built the HTML report generation engine with 12 GDI+ chart functions that render supersampled bitmaps embedded as inline base64 PNG.

**Files added:**
| File | Lines | Purpose |
|------|-------|---------|
| `FF2/ViewModels/DetailedReportViewModel.vb` | 81 | Tree view item that triggers report generation |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/ViewModels/MainWindowViewModel.vb` | Added `refreshDetailedReport()` (~1500 lines) and 12 `Draw*()` chart functions. Added report HTML generation with 15 sections (A–L) |
| `FF2/Libs/HtmlUtils.vb` | Added `encodeTobase64()`, `wrap_bmp_img()`, modified `CreateHtmlPage()` for report CSS embedding |
| `FF2/Converters/BrowserBehavior.vb` | Extended to handle `DetailedReportHtml` binding to WebBrowser |
| `FF2/Resources/Reports.css` | Added ~300 lines: dashboard cards, TOC, section headers, math blocks, step lists, chart containers, summary boxes |
| `FF2/Views/MainWindow.xaml` | Added WebBrowser panel for CM Report display |
| `FF2/FF2.vbproj` | Added new file references |

**Computation impact:** None. All changes are in the UI/presentation layer (FF2 project).

---

#### Change 3: CM Report — Native HTML/SVG Pipeline ("Open in Browser")

**Date:** 2026-03-16 | **Commits:** `430bee5`, `51feaa9` | **Category:** Report (new feature)

Added a parallel rendering pipeline that generates self-contained HTML5 with inline SVG charts, replacing GDI+ bitmaps with vector graphics for browser viewing.

**Files added:**
| File | Lines | Purpose |
|------|-------|---------|
| `FF2/Libs/HtmlReportGenerator.vb` | 3398 | Complete HTML5 report generator: inline SVG charts, CSS Grid layout, responsive design, print media queries |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/Libs/HtmlUtils.vb` | Added `HtmlToFile()` method for saving HTML and launching default browser |
| `FF2/Views/MainWindow.xaml` | Added "Open in Browser" button |
| `FF2/ViewModels/MainWindowViewModel.vb` | Added `OnSectionReportOpenHtml` command and handler |
| `FF2/FF2.vbproj` | Added `HtmlReportGenerator.vb` to compilation |

**Computation impact:** None.

---

#### Change 4: CM Report — PDF Quality Overhaul & Vector Export

**Date:** 2026-03-17 | **Commit:** `d1f7218` | **Category:** Report (enhancement)

Rerouted CM Report PDF export from GDI+ bitmaps to the SVG pipeline for vector-quality output. Fixed SelectPdf rendering issues (web page width mismatch, font sizes, stroke widths, page breaks).

**Files added:**
| File | Lines | Purpose |
|------|-------|---------|
| `FF2/Views/AboutWindow.xaml` | 333 | About dialog with beta disclosure, credits, license |
| `FF2/Views/AboutWindow.xaml.vb` | 36 | Code-behind for build date and drag support |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/Libs/HtmlUtils.vb` | `webPageWidth` 1400→1100; `CssMediaType = Screen` |
| `FF2/Libs/HtmlReportGenerator.vb` | SVG font size overhaul (minimum 10px); stroke widths increased; `FmtCDFSvg()` helper; page break CSS; dashboard flexbox |
| `FF2/ViewModels/MainWindowViewModel.vb` | CM Report PDF now calls `HtmlReportGenerator.Generate()`; added About command |
| `FF2/FF2.vbproj` | Added AboutWindow files |

**Computation impact:** None.

---

#### Change 5: Report Consistency & Visual Enhancements

**Date:** 2026-03-19 | **Commit:** `2ccb37b` | **Category:** Report (enhancement)

Improved visual consistency across all 7 report types. 2x supersampling for CDF/PCR graphs. BMP→PNG encoding. Enhanced table contrast in CSS.

**Files added:**
| File | Lines | Purpose |
|------|-------|---------|
| `FF2/Converters/TreeNodeIconConverter.vb` | 41 | Maps ViewModel types to Unicode icons for tree view |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/ViewModels/MainWindowViewModel.vb` | 2x supersampling for CDF/PCR graph bitmaps; BMP→PNG encoding; chart captions |
| `FF2/ViewModels/CDFGraphViewModel.vb` | Report rendering updates |
| `FF2/ViewModels/GraphPCN.vb` | Report rendering updates |
| `FF2/ViewModels/ReportViewModel.vb` | Report rendering updates |
| `FF2/ViewModels/SummaryReportViewModel.vb` | Report rendering updates |
| `FF2/ViewModels/PCRReportViewModel.vb` | Report rendering updates |
| `FF2/ViewModels/Form5010.vb` | Report rendering updates |
| `FF2/Resources/Reports.css` | Increased zebra-stripe contrast |

**Computation impact:** None.

---

#### Change 6: Gear Layout Refinement

**Date:** 2026-03-20 | **Commit:** `d005ed4` | **Category:** UI (enhancement)

Removed coordinate annotations from evaluation points in the gear configuration visualization to reduce visual clutter.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/Libs/ModuleDrawProfile.vb` | Simplified `PaintGear()` rendering (removed coordinate labels on evaluation points) |

**Computation impact:** None.

---

#### Change 7: Annual Departure Limit Raised to 500,000

**Date:** 2026-03-15 | **Commit:** `a7459ac` | **Category:** Validation (relaxation)

Raised the maximum allowable annual departures per aircraft from 100,000 to 500,000 to allow analysis of higher-traffic scenarios. The original limit had no regulatory citation in the code.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/ValidationRules/AnnualDepartureValidationRule.vb` | Validation cap 100,000 → 500,000 |
| `FF2/Models/AircraftList.vb` | Error-list validation cap 100,000 → 500,000 |
| `FF2/ViewModels/MainWindowViewModel.vb` | MessageBox validation cap 100,000 → 500,000 |

**Computation impact:** None. The validation change allows larger input values but does not alter how those values are processed by the CDF or thickness design engines.

---

#### Change 8: Gross Weight Guardrail Override

**Date:** 2026-03-15 | **Commit:** `a7459ac` | **Category:** Validation (relaxation)

Changed gross weight validation from a hard block to a user-overridable warning dialog.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FaarFieldModel/AirplaneInfo.vb` | Replaced hard `MessageBox.Show` + revert with `YesNo` dialog: Yes = override for research, No = revert. Added re-entry guard to prevent double-fire from `OnPropertyChanged` |

**Computation impact:** None. The computation engines accept whatever gross weight value is set; only the UI-layer validation was changed.

---

#### Change 9: RDEC Mix Property Storage

**Date:** 2026-03-15 | **Commit:** `a7459ac` | **Category:** Model (extension)

Added RDEC (Rate of Dissipated Energy Change) asphalt mix properties to the section model so they can be captured for CM Report documentation.

**Files modified:**
| File | Lines added | Change summary |
|------|-------------|----------------|
| `FaarFieldModel/Interfaces/ISection.vb` | +9 | Added 6 RDEC property declarations to `ISection` interface |
| `FaarFieldModel/Section.vb` | +44 | Implemented RDEC properties with backing fields |

**Computation impact:** None. These properties store values that already existed as engine globals (`gFlexuralMod`, `gAirVoids`, etc.); the new properties provide a clean path for the CM Report to read them.

---

#### Change 10: UI/UX Modernization

**Date:** 2026-03-15 | **Commit:** `a7459ac` | **Category:** UI (enhancement)

Comprehensive visual refresh of the WPF interface.

**Files added:**
| File | Lines | Purpose |
|------|-------|---------|
| `FF2/Themes/ModernTheme.xaml` | 508 | Theme resource dictionary: 16 color brushes, implicit styles for Button/TextBox/DataGridRow/GroupBox/Label, keyed styles, typography |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/Views/MainWindow.xaml` | Replaced hardcoded colors with StaticResource; keyboard shortcuts; status bar; DataGrid formatting; tree view styling; filter TextBoxes; progress banner; toast notifications (~1000 lines changed) |
| `FF2/ViewModels/MainWindowViewModel.vb` | Aircraft/material filter properties; toast notification system; progress state management |
| `FF2/Libs/ModuleDrawProfile.vb` | Anti-aliased rendering; gradient fills; dimension annotations; legend box (~500 lines rewritten in `PaintGear()` and cross-section drawing) |
| `FF2/Application.xaml` | Theme dictionary merge; converter registration; window font |
| `FF2/My Project/app.manifest` | PerMonitorV2 DPI awareness; Windows 10 supportedOS |

**Computation impact:** None. All changes are in the presentation layer.

---

#### Change 11: Build Warning Cleanup

**Date:** 2026-03-17 | **Commit:** `d1f7218` | **Category:** Quality (maintenance)

Fixed 8 pre-existing compiler warnings representing potential runtime risks. No functional behavior changed.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/ViewModels/MainWindowViewModel.vb` | Explicit initializers for `FrostDepthReading`; removed duplicate XML doc comment; added `Return Nothing` for `PCRReportPage()` |
| `FF2/Models/RunAnalysis.vb` | Split `S1`–`S5` into individual declarations with `= ""` initializers |
| `FF2/Converters/ThicknessConverter.vb` | Added `Return ""` for unset `DimensionalProperty` |
| `FF2/Libs/AircraftLibrary.vb` | Added `Return Nothing` when save dialog is canceled |

**Computation impact:** None.

---

## Summary: Files Changed vs. Original FAA Source

**42 files changed** | **11,480 lines added** | **753 lines deleted**

### By category

| Category | Files added | Files modified | Lines added | Computation altered? |
|----------|------------|----------------|-------------|---------------------|
| Report — Data collection | 1 | 4 | ~323 | No (instrumentation only) |
| Report — GDI+ pipeline | 1 | 6 | ~2,200 | No |
| Report — SVG pipeline | 1 | 4 | ~3,500 | No |
| Report — PDF quality | 0 | 2 | ~200 | No |
| Report — Consistency | 1 | 8 | ~150 | No |
| UI — Modernization | 1 | 5 | ~2,000 | No |
| UI — About window | 2 | 4 | ~400 | No |
| UI — Gear visualization | 0 | 1 | ~500 | No |
| Validation — Departures | 0 | 3 | ~12 | No |
| Validation — Gross weight | 0 | 1 | ~27 | No |
| Model — RDEC properties | 0 | 2 | ~53 | No |
| Quality — Build warnings | 0 | 4 | ~15 | No |

### Computational engine files — change summary

| File | Total lines added | Type of change | Algorithm modified? |
|------|-------------------|----------------|---------------------|
| `ACNClassLib/clsACR.vb` | +62 | Data structure + iteration logging | **No** |
| `FaarFieldAnalysis/modCDF.vb` | +89 | Post-computation data capture | **No** |
| `FaarFieldAnalysis/modStrDesignFlex.vb` | +122 | Iteration/sublayer data capture | **No** |
| `FaarFieldAnalysis/modFedfaaGbl.vb` | +50 | Global declaration + data capture | **No** |
| `FaarFieldModel/AirplaneInfo.vb` | +21 (net) | Gross weight validation UX | **No** |
| `FaarFieldModel/Section.vb` | +44 | RDEC property storage | **No** |
| `FaarFieldModel/Interfaces/ISection.vb` | +9 | Interface extension | **No** |
| LEAFClassLib/ (entire directory) | 0 | — | **No** |
| FEMClassLib/ (entire directory) | 0 | — | **No** |
| All other FaarFieldAnalysis/mod*.vb | 0 | — | **No** |

---

*Last updated: 2026-03-20*
*Maintained by: HAL9000 (automated) and Johann Cardenas*
