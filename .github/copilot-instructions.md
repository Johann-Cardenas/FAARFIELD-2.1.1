# Copilot Instructions for FAARFIELD 2.1.1

## Project Overview

FAARFIELD is an FAA-published VB.NET desktop application for airfield pavement thickness design and evaluation. It implements layered elastic theory (LEAF), 3-D finite-element analysis (FAASR/NIKE3D-based), ACN/PCN classification (ICAO), and cumulative damage factor (CDF) integration for mixed-traffic design.

**Framework:** .NET Framework 4.8 (locked -- no .NET Core/5+)
**Language:** VB.NET only -- do not introduce C# projects
**IDE:** Visual Studio 2019+
**Startup project:** FF2 (WPF)

## Project Structure

```
FAARFIELD.sln                 Solution file (do not hand-edit)
FF2/                          Primary WPF UI (MVVM pattern)
  Views/                        XAML views
  ViewModels/                   ViewModel classes (22 files)
  Converters/                   Value converters (52 files)
  Defaults/Aircraft/            FAA-curated aircraft data (XML/CSV)
FaarFieldModel/               Domain model with interfaces
  Interfaces/                   23 interface contracts (IAircraft, IMaterial, ISection, etc.)
  FaarFieldModelFactory.vb      Factory for creating domain objects
FaarFieldAnalysis/            Procedural analysis engine (WinForms legacy)
  modCDF.vb                     Cumulative damage integration (~2,100 lines)
  modFedfaaGbl.vb               Global constants and shared state
  modStrDesignFlex.vb           Flexible pavement design
  modDesignRigid_Adj.vb         Rigid pavement design
  modFAILURE_MODEL_NP.vb       Failure models (2014 FAA calibration)
  modPCN_H51inVB.vb            H-51 edge-stress method
LEAFClassLib/                 Layered elastic solver (clsLEAF.vb, ~3,994 lines)
ACNClassLib/                  ACN/PCN calculation (clsACR.vb, ~4,711 lines)
FEMClassLib/                  Finite-element engine (120+ files)
  Solve/                        Matrix solution routines (75 files)
  FAASR/                        3-D FEM interface
FAAMeshClassLib/              Mesh generation
ACClassLib/                   Aircraft classification helpers
AMClassLib/                   Aircraft mix helpers
FAARFIELDUnitTests/           MSTest unit tests
FAARFIELD.Installer/          WiX Toolset 3.10 installer (.wixproj)
lib/RCWPF/                    Vendored Telerik UI assemblies
packages/                     NuGet packages (managed by NuGet restore)
```

### Dependency Build Order

```
LEAFClassLib, FEMClassLib           (foundational, no deps)
ACNClassLib        --> LEAF + FEM
FaarFieldModel                      (no deps)
AMClassLib         --> FaarFieldModel
FaarFieldAnalysis  --> ACN + LEAF + FEM + AMClassLib
FF2                --> FaarFieldModel
FAARFIELDUnitTests --> FaarFieldModel
```

## Build Commands

```shell
# Command-line build
msbuild FAARFIELD.sln /p:Configuration=Release

# Run tests
vstest.console FAARFIELDUnitTests\bin\Debug\FAARFIELDUnitTests.dll
```

In Visual Studio: `Ctrl+Shift+B` to build, `F5` to run (FF2 startup project).

## Coding Conventions

### Naming Patterns

| Element | Convention | Examples |
|---------|-----------|----------|
| Computational classes | `cls` prefix + PascalCase | `clsLEAF`, `clsACR`, `clsSolve` |
| Model/UI classes | PascalCase (no prefix) | `Aircraft`, `Material`, `Section` |
| Procedural modules | `mod` prefix + PascalCase | `modCDF`, `modFedfaaGbl`, `modStrDesignFlex` |
| Interfaces | `I` prefix + PascalCase | `IAircraft`, `IMaterial`, `ISection` |
| ViewModels | PascalCase + `ViewModel` suffix | `MainWindowViewModel` |
| Forms (legacy) | `frm` or `Form` prefix | `FormPCN`, `frmGear` |
| Global module vars | `g` prefix + camelCase | `gFirstIter`, `gSolverType`, `gSlabMeshSize` |
| Aircraft struct fields | `lib` prefix | `libACName`, `libGL`, `libCP` |

### Fortran-Heritage Numerical Code

The computational libraries (LEAFClassLib, ACNClassLib, FEMClassLib, FaarFieldAnalysis) contain dense numerical code ported from Fortran. Variable names are deliberately terse (e.g., `a0`, `xx1()`, `hh()`, `n1`) and map directly to published FAA technical reports and the original Fortran source.

**Do NOT rename these variables for clarity.** The naming is preserved for traceability against FAA documentation.

### Author and Change-Tracking Comments

Preserve comments like `'ikawa`, `'YGC 061113`, `'kairat replace tandem`. These reference original FAA developers and change dates. Do not remove or reformat them.

### Architectural Patterns

- **FF2 (WPF):** Strict MVVM. Views bind to ViewModels. Value converters in `FF2/Converters/`. No business logic in code-behind.
- **FaarFieldModel:** Domain model with factory pattern. New types must implement matching `I*` interface and use `FaarFieldModelFactory`.
- **FaarFieldAnalysis:** Procedural modules with shared `Public` module-level variables representing global state. Be aware that these variables are mutated across the entire analysis pipeline.

### Comments

Use comments sparingly. Only comment complex code -- do not add obvious or redundant comments, docstrings, or XML documentation to code you didn't write.

### Known Intentional Quirks

- `IMeasurmentSystem` interface is intentionally misspelled -- do not rename.
- `Option Strict Off` is intentional on most projects (legacy Fortran-origin late-binding). Do not add `Option Strict On` to files that lack it.

## Do Not Touch

These files are auto-generated or externally managed. Do not manually edit them:

| Category | Files/Paths |
|----------|-------------|
| WinForms/WPF designers | All `*.Designer.vb` files |
| VS project internals | All `My Project/` folders (`AssemblyInfo.vb`, `*.Designer.vb`, etc.) |
| WPF code-behind (auto-wired) | Files containing only `InitializeComponent()` |
| Vendored Telerik assemblies | `lib/RCWPF/` (pre-compiled, used as-is) |
| NuGet packages | `packages/` directory |
| FAA aircraft data | `FF2/Defaults/Aircraft/aircraft.xml` and `aircraft.csv` |
| Solution file | `FAARFIELD.sln` (use Visual Studio to modify) |
| Installer project | `FAARFIELD.Installer/` (modify only when changing deployed artifacts) |

## Shared Mutable State Warning

Files like `modCDF.vb` and `modFedfaaGbl.vb` contain dozens of `Public` variables (`gFirstIter`, `gParamA/B/C/D`, `gFSlope`, `gTandemFnew`, etc.) shared across FaarFieldAnalysis, FEMClassLib, ACNClassLib, and LEAFClassLib. Renaming, retyping, or removing these variables will break callers across multiple projects.

The FEM solver (`FEMClassLib/Solve/`) has ~96 heavily interrelated files. Changes there require running the full unit test suite and verifying results match FAA-published verification cases.

## Environment Context

- **No Fortran compiler needed:** All original Fortran code is fully ported to VB.NET.
- **No Abaqus dependency:** The FEM solver is self-contained (FAASR/NIKE3D heritage).
- **Units:** Internal calculations use US customary (inches, psi, pounds). Metric conversion happens at the UI layer via `IMeasurmentSystem`.
- **Compiler settings across projects:** `Option Explicit On`, `Option Strict Off`, `Option Infer On`, `OptionCompare Binary`.
- **No linters or formatters are configured.** Do not introduce `.editorconfig`, StyleCop, or code-analysis rulesets.
