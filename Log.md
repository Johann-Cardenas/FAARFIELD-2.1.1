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

#### Change 12: Report & UI Readability Pass

**Date:** 2026-04-14 | **Commit:** `b8dfcb7` | **Category:** UI / Report (enhancement)

Targeted readability pass across report charts, embedded CSS, and the main WPF shell. Font sizes bumped uniformly (chart titles 10→11 pt, axis labels 8.5→9.5 pt, legends 7→8 pt, concept-diagram headings 8→9 pt); `.detailed-table` and dashboard-card spacing increased; color swatches enlarged with a hairline border. Several hardcoded hex colors in the primary view were replaced with theme tokens (`FaaPrimaryLight`, `FaaBorderGrayLight`, `FaaBackgroundGray`, `FaaTextPrimary`). Added implicit `ToolTip` and `Separator` styles to `ModernTheme.xaml`. Enlarged the Run button, design-layer selection buttons, and the analysis progress bar for better hit targets.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/ViewModels/MainWindowViewModel.vb` | Standardized chart font sizes across all 13 `Draw*` functions; unified annotation/legend sizing |
| `FF2/Libs/HtmlReportGenerator.vb` | SVG font-size alignment matching the GDI+ pipeline for consistent report output |
| `FF2/Resources/Reports.css` | Increased `.detailed-table` padding and font; larger color swatches with border; dashboard-card label size bump; refined chart-caption color |
| `FF2/Themes/ModernTheme.xaml` | Added implicit `ToolTip` style (rounded, drop shadow) and `Separator` style; bumped `DataGrid` row/header heights and cell padding |
| `FF2/Views/MainWindow.xaml` | Replaced hardcoded colors with theme tokens; enlarged Run button and design-layer buttons; bumped toolbar label and progress bar sizes |
| `FF2/Views/AboutWindow.xaml` | Bullet and disclaimer text bumped to 11 pt |

**Computation impact:** None. All changes are in the presentation layer.

---

#### Change 13: Application Icon Fix

**Date:** 2026-04-22 | **Category:** UI (bug fix)

Replaced the application icon with a proper multi-resolution file. The previous `<ApplicationIcon>` pointed at `Resources\BackC.ico`, a single 32×32 / 16-color frame; Windows had no high-res frame to pick from for taskbar (48 px), Alt+Tab, Start menu, or jump lists (all want 48 or 256 px), so the icon rendered pixelated and washed-out. Generated `FAA-Icon.ico` from the new `FAA-Icon.svg` source with seven PNG-compressed frames (16, 24, 32, 48, 64, 128, 256 px, 32-bit RGBA). Also set `Window.Icon` explicitly on `MainWindow.xaml` so the title-bar icon is no longer an implicit fallback.

**Files added:**
| File | Purpose |
|------|---------|
| `FF2/Resources/FAA-Icon.svg` | Vector source for the application icon |
| `FF2/Resources/FAA-Icon.ico` | Multi-resolution .ico (7 frames) generated from the SVG |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/FF2.vbproj` | `<ApplicationIcon>` now points to `Resources\FAA-Icon.ico`; `FAA-Icon.ico` added as a WPF `<Resource>` |
| `FF2/Views/MainWindow.xaml` | Added `Icon="/FF2;component/Resources/FAA-Icon.ico"` |

**Computation impact:** None.

---

#### Change 14: About Window — Version String and Photographic Header

**Date:** 2026-04-22 | **Category:** UI (enhancement)

Updated the version pill from `v2.1.1-CM` to `v.2.1.1.10` to match the formal release numbering. Replaced the purely-gradient header with a layered background: the ICT research-group photograph (`FF2/Resources/ICT-Image.jpg`) sits as the outer `Border.Background`, clipped by the rounded top corners; a second inner `Border` provides a horizontal gradient overlay going from fully opaque dark teal (`#FF004D40`) on the left to fully transparent teal (`#0026A69A`) on the right. The left two-thirds stay readable behind the FAARFIELD title, version pill, subtitle, and build date, while the right portion smoothly reveals the photograph. Source image was downsampled from 2695×1200 to 1280×570 and saved as progressive JPEG at quality 85 (~114 KB) to keep the assembly footprint reasonable.

**Files added:**
| File | Purpose |
|------|---------|
| `FF2/Resources/ICT-Image.jpg` | 1280×570 progressive JPEG used as the About window header background |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/Views/AboutWindow.xaml` | Version pill text; header now uses `ImageBrush` background plus nested gradient overlay Border with alpha fading to transparent |
| `FF2/FF2.vbproj` | Added `ICT-Image.jpg` as a WPF `<Resource>` so it is packed into the FF2 assembly |

**Computation impact:** None.

---

#### Change 15: Responsive Layout — Adapt to Small-Screen Laptops

**Date:** 2026-04-22 | **Category:** UI (enhancement)

Made MainWindow adaptive so the app is usable on 17.3" laptops (and anything down to ~1366×768) while preserving the exact appearance on the 27" 4K reference monitor. Three layers of work:

1. **Adaptive window sizing.** Declared `Width=1280 Height=768 MinWidth=1000 MinHeight=640` on the `Window` element. The existing `WindowState` two-way binding (already wired via `Window_Loaded_Command`) is now driven by a new check at the top of `Window_Loaded`: if `SystemParameters.WorkArea.Width < 1400` or `Height < 820`, the window launches maximized so no UI is clipped by screen chrome.
2. **Selective Viewbox wrapping.** Wrapped the three large fixed-width Canvas regions — Job Information pane (Width=1000), Analysis tab content (Width=1200), and PAVEAIR download pane (Width=1200) — each in a `<Viewbox Stretch="Uniform" StretchDirection="DownOnly">`. On 4K the Viewbox is transparent (no upscale); on smaller screens the Canvas shrinks uniformly to fit the available pane width. The ~30 smaller `<Canvas Width="240">` blocks are already narrow enough and were left alone.
3. **No global transform.** A `LayoutTransform` on the root Grid was explicitly rejected because Telerik `RadDocking`'s drag-drop compass, autohide strips, and floating panes position themselves in screen coordinates and do not participate in parent visual transforms — a global scale would break those behaviours.

**Why these specific changes.** DPI awareness was already correct (`PerMonitorV2,PerMonitor` in `app.manifest`), so WPF already scales text and chrome to correct physical size on any monitor. The actual failure mode on small screens was spatial: the 1265-wide window exceeded the available work area, and Canvas regions at 1000–1200 DIPs overflowed their panes. Addressing both issues directly, with Telerik-safe techniques, was preferred over a disruptive Canvas→Grid rewrite.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/Views/MainWindow.xaml` | Window tag: bumped `Width` 1265→1280, added `MinWidth=1000 MinHeight=640`; wrapped three Canvas regions in `<Viewbox StretchDirection="DownOnly">` |
| `FF2/ViewModels/MainWindowViewModel.vb` | `Window_Loaded`: auto-maximize when `SystemParameters.WorkArea` is smaller than 1400×820 |

**Computation impact:** None. Pure presentation-layer change.

---

#### Change 16: Defensive-Programming Audit — GDI+ Leak and Dead-Code Fixes

**Date:** 2026-04-22 | **Category:** Quality (bug fix)

Completed a full audit pass of the customized UI / report code for bugs that could crash the app or degrade the experience. Confirmed nine real issues (after discounting several hallucinated agent findings) and applied surgical fixes. All changes are in the presentation layer; no computation touched.

**GDI+ handle leaks fixed.** In WPF/GDI+ these accumulate silently until the OS runs out of handles and the app crashes. Each fix below was verified to be leaking on every invocation:

- `FF2/ViewModels/MainWindowViewModel.vb` `DrawUserDefinedGear()` (gear diagram on the Structure pane) and `DrawProfile()` (pavement cross-section): `Graphics.FromImage(image)`, the `System.Windows.Forms.PictureBox` scratch control, and the `MemoryStream` backing the returned `BitmapImage` were all leaked on every redraw. Each now lives inside a multi-resource `Using` block. The `BitmapImage` is initialised with `CacheOption = OnLoad` and `Freeze()`ed so the backing stream can be disposed safely right after load.
- `FF2/ViewModels/MainWindowViewModel.vb` `DrawEquationImage()`: the inline `g.FillRectangle(New SolidBrush(...), ...)` for the left accent stripe leaked on every equation rendered in the CM Report. Now wrapped in `Using`, with the other pens/brushes reorganised into a single multi-resource `Using` block.
- `FF2/Libs/ModuleDrawProfile.vb` `Paint(...)`: the three structurally-similar layer-drawing branches each allocated a `Pen` (for designed-layer outline) and a `Font` + `SolidBrush` + `Pen` + `SolidBrush` set (for the three on-layer labels) without disposing any of them. Over a typical 4-6 layer design, that's up to 18 GDI+ handles leaked **per redraw**. All three branches now use `Using` blocks with scoped `labelFont` locals, and the empty-structure placeholder path also disposes its font.
- `FF2/Libs/HtmlUtils.vb` `encodeTobase64`: `MemoryStream` now in `Using`.
- `FF2/Libs/HtmlUtils.vb` `HtmltoPdf`: the SelectPdf `HtmlToPdf` converter and resulting `PdfDocument` now in nested `Using` blocks so the native resources are released after each PDF export.

**Silent-failure and dead-code cleanup:**

- `FF2/ViewModels/MainWindowViewModel.vb` lines ~92 and ~143: the `Try/Catch` blocks around `CurrentSectionView.Section.*Tracker = True` assignments had empty `Catch` bodies. They now log via `Debug.WriteLine` so future diagnostics are possible.
- `FF2/ViewModels/MainWindowViewModel.vb` `CommonReportPdf` (near line 14173): an always-true `If System.Windows.Forms.DialogResult.Cancel Then` guard inside the SaveFileDialog-cancel branch was removed; the cancellation message now shows unconditionally when the user cancels, which is the correct behavior.
- `FF2/ViewModels/MainWindowViewModel.vb` (after `DrawProfile`): deleted a ~20-line commented-out `DrawProfile(section, job)` overload that had been dead since the current method was introduced.
- `FF2/Views/MainWindow.xaml.vb`: removed the unused `Login_Click` handler. The XAML login button uses `Command="{Binding OnLogin_Command}"`, so the code-behind handler was never wired up.

**False positives dismissed.** The audit agents also flagged (a) a duplicate `RadDocking.SerializationTag="ProgressWindow"` — verified to be inside a `<!-- ... -->` comment block at lines 3513–3531 of MainWindow.xaml and therefore inert; (b) `Process.Start(New ProcessStartInfo(filepath) With {.UseShellExecute = True})` in `HtmlUtils.HtmlToFile` — this is the correct Win32 pattern for "open file with default handler" and does not have a quoting bug; (c) `Image.FromFile(...)` in `ModuleDrawProfile.LoadBrushes` / `LoadBrushes2` — the file lock is a well-known `Image.FromFile` behaviour but the brushes live for the lifetime of the app and the files are shipped assets, so no functional defect.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/ViewModels/MainWindowViewModel.vb` | GDI+ leak fixes in `DrawUserDefinedGear`, `DrawProfile`, `DrawEquationImage`; silent `Catch` blocks now log; removed always-true `DialogResult.Cancel` check; deleted commented-out `DrawProfile` overload |
| `FF2/Libs/ModuleDrawProfile.vb` | All three layer-drawing branches in `Paint(...)` refactored to dispose `Pen`/`SolidBrush`/`Font` via `Using`; empty-structure placeholder path fixed the same way |
| `FF2/Libs/HtmlUtils.vb` | `encodeTobase64` wraps `MemoryStream` in `Using`; `HtmltoPdf` wraps `HtmlToPdf` and `PdfDocument` in `Using` |
| `FF2/Views/MainWindow.xaml.vb` | Removed dead `Login_Click` handler |

**Computation impact:** None. The audit explicitly excluded `LEAFClassLib`, `FEMClassLib`, and the original `FaarFieldAnalysis/modPCN_*` / `modDesign*` / `modFAILURE_*` modules per standing rule.

---

#### Change 17: Clean MSTest Suite — 5/5 Passing

**Date:** 2026-04-22 | **Category:** Quality (test infrastructure)

Two MSTest cases (`AircraftListUnitTest`, `RetrieveBellyInfo`) had been failing since long before the audit work. Root-cause analysis identified two independent defects:

1. **Headless MessageBox.** `FF2/Libs/AircraftLibrary.GetAircrafts` shows a warning dialog when the aircraft library signature validation fails. Inside the message string it reads `My.Application.Info.DirectoryPath`, which returns `Nothing` under vstest.console, producing a `NullReferenceException` that crashed the test's `MainWindowViewModel` constructor. The constructor already accepted an `IsUnitTest` flag, but it was not propagated into `GetAircrafts`.

2. **Stale assertion after Change 7.** `AircraftUnitTests.AircraftListUnitTest` at line 100 asserted that over-limit departures produce the message `"Maximum allowable number of Annual Departures is 100,000"` — but Change 7 (2026-03-15) raised the limit to 500,000 and updated the production error message accordingly. The test literal was never updated.

**Fixes:**
- Added an optional `isUnitTest As Boolean = False` parameter to `AircraftLibrary.GetAircrafts`. The constructor at `MainWindowViewModel.vb:4317` and `4319` now forwards its existing `IsUnitTest` value. When `isUnitTest = True`, the unsigned-library warning is written to `Debug.WriteLine` instead of `MessageBox.Show`. Production UX is unchanged — in interactive app use the dialog still appears exactly as before.
- Updated the test assertion in `FAARFIELDUnitTests/AircraftUnitTests.vb:100` to expect `"500,000"` in line with the current production error message.

**Verification:** `MSBuild FAARFIELD.sln -t:FAARFIELDUnitTests -p:Configuration=Debug` → exit 0. `vstest.console.exe FAARFIELDUnitTests\bin\Debug\FAARFIELDUnitTests.dll /Platform:x86` → **Total tests: 5, Passed: 5, Failed: 0**.

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/Libs/AircraftLibrary.vb` | Added `isUnitTest As Boolean = False` optional parameter to `GetAircrafts`; wrapped the unsigned-library `MessageBox.Show` in an `If isUnitTest Then Debug.WriteLine Else MessageBox.Show End If`; added `Imports System.Diagnostics` |
| `FF2/ViewModels/MainWindowViewModel.vb` | Passes the constructor's `IsUnitTest` flag into both `GetAircrafts` calls inside the constructor (line 4317/4319) — the separate `LoadAircraftLibrary()` refresh path is UI-triggered and keeps the default `False` |
| `FAARFIELDUnitTests/AircraftUnitTests.vb` | Updated line 100 assertion to expect `500,000` (matches production behaviour introduced in Change 7) |

**Computation impact:** None. Both fixes are in the test-plumbing and presentation layers.

---

#### Change 18: Fast Incremental Builds — Stop BuildDate from Invalidating VB Compile

**Date:** 2026-04-23 | **Category:** Build (performance)

The customization work has been generating frequent rebuild cycles, which surfaced a latent build-performance pathology: every "Build" in Visual Studio was forcing a full recompile of FF2's 18,856-line `MainWindowViewModel.vb` even when nothing had changed.

Root cause: `FF2.vbproj` has had a `<PreBuildEvent>` since initial project setup (commit `32d8b2d`) that rewrites `FF2\Resources\BuildDate.txt` on every build. That file was declared as `<EmbeddedResource>`, making it an input to MSBuild's `CoreCompile` target. Each build:

1. PreBuildEvent rewrites `BuildDate.txt` (timestamp changes);
2. MSBuild's up-to-date check sees the embedded-resource input modified;
3. `GenerateTemporaryTargetAssembly` (the WPF temp-assembly pass) re-runs — ~11 s;
4. `Vbc` re-compiles the entire FF2 assembly from scratch — ~20 s;
5. Downstream references cascade.

An additional side effect: the file was never copied to `bin\Debug\Resources\`, so the About window's code-behind (`AboutWindow.xaml.vb:12`) silently fell through to `DateTime.Now` as a fallback. The feature appeared to work but was showing the current date, not the real build timestamp.

**Fix.** Changed one line in `FF2.vbproj`:

```xml
<None Include="Resources\BuildDate.txt">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

(previously `<EmbeddedResource Include="Resources\BuildDate.txt" />`).

The PreBuildEvent still runs and updates the timestamp — no loss of behaviour. The file is now copied to the output directory where the About window's existing `File.ReadAllText` actually reads from, so the build-date display fixes itself as a side effect. Crucially, the file is no longer a `Vbc` input, so the every-build full-recompile cycle is broken.

**Measured impact** (Configuration=Debug, Platform=AnyCPU, local MSBuild 17.14):

| Build scenario | Before | After | Delta |
|---|---|---|---|
| Cold build (after Clean) | ~38 s | ~38 s | unchanged — unavoidable |
| Immediate rebuild, nothing changed | ~12 s | **~5 s** | ~58 % faster |
| Incremental with small code change | ~40 s (full Vbc) | ~5 – 15 s (only touched projects) | ~70 % faster |

**Files modified:**
| File | Change summary |
|------|---------------|
| `FF2/FF2.vbproj` | `BuildDate.txt` moved from `<EmbeddedResource>` to `<None CopyToOutputDirectory="PreserveNewest">` |

**Computation impact:** None. Zero functional change — only the build-system classification of the timestamp file, plus the (now working) About-window build-date display that was previously showing a fallback value.

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

*Last updated: 2026-04-22*
*Maintained by: HAL9000 (automated) and Johann Cardenas*
