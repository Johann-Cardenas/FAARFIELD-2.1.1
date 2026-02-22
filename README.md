# FAARFIELD 2.1.1 — Source (FAA)

> **Official source notice:** This repository contains the source code of **FAARFIELD 2.1.1** as published by the Federal Aviation Administration: https://www.airporttech.tc.faa.gov/Products/Airport-Safety-Papers-Publications/Airport-Safety-Detail/ArtMID/3682/ArticleID/2841/FAARFIELD-20
>
> This copy is provided for inspection, study, and archival purposes only. It does not represent a reproduction or transfer of copyright. All rights and ownership remain with the Federal Aviation Administration.

---

## Table of contents

- [Overview](#overview)
- [Repository structure](#repository-structure)
- [Software stack](#software-stack)
- [Use cases](#use-cases)
- [Quick start (build)](#quick-start-build)

---

## Overview

- **Project:** FAARFIELD — a VB.NET solution for airfield pavement and aircraft load/structure analysis.
- **Purpose:** Implements models, analysis tools and libraries for PCN/ACN calculations, aircraft/gear modeling, mesh and FEM analysis, and related utilities.

## Repository structure

Root (key items)

- `FAARFIELD.sln` — Root Visual Studio solution containing multiple projects.
- `README.md` — This file.

Top-level projects and folders

- `FaarFieldAnalysis/` — Main application (Windows Forms). Project: `FaarFieldAnalysis.vbproj`.
- `FaarFieldModel/` — Core domain model, factories and shared types. Project: `FaarFieldModel.vbproj`.
- `ACClassLib/` — Aircraft class library and helpers.
- `ACNClassLib/` — ACN/PCN calculation library.
- `AMClassLib/` — Aircraft-matching / auxiliary models.
- `LEAFClassLib/` — LEAF-related models and utilities.
- `FAAMeshClassLib/` — Mesh utilities and helpers. Project: `FAAMeshClassLib.vbproj`.
- `FEMClassLib/` — Finite-element helper library.
- `CreateSignedAircraftLibrary/` — Utility app to create signed aircraft libraries.
- `FAARFIELDUnitTests/` — Unit tests for core logic.
- `FAARFIELD.Installer/` — WiX installer project used to create an installer (`Product.wxs`).

Supporting folders

- `lib/` — Third-party assemblies (example: Telerik controls under `lib/RCWPF/`).
- `packages/` — NuGet package folders (if packages are restored locally).

Notes

- Projects target .NET Framework 4.8; most code is Visual Basic (VB.NET).
- Many projects reference each other via `ProjectReference` entries in their `.vbproj` files; open the solution in Visual Studio for dependency visualization.

## Software stack

- **Language:** Visual Basic .NET (VB.NET).
- **Framework:** .NET Framework 4.8 (projects target `v4.8`). Example: [FaarFieldAnalysis.vbproj](FaarFieldAnalysis/FaarFieldAnalysis.vbproj#L1).
- **IDE / Build:** Visual Studio / MSBuild (ToolsVersion 15+).
- **UI:** Windows Forms and WPF components (projects reference `System.Windows.Forms`, `PresentationFramework`, etc.).
- **Third-party:** Telerik UI controls (referenced from `lib/RCWPF/...` in `FaarFieldModel`).
- **Installer:** WiX Toolset (used by `FAARFIELD.Installer`).

## Use cases

- **PCN / ACN calculations:** Compute pavement classification numbers and aircraft classification numbers for airfield pavement evaluation.
- **Aircraft & Gear Modeling:** Represent aircraft definitions, gear configurations and produce load distributions for pavement checks.
- **Structural Analysis:** Rigid and flexible pavement design and strength/fatigue assessments using included numerical and FEM utilities.
- **Mesh & FEM workflows:** Create meshes and run finite-element style calculations via `FAAMeshClassLib` and `FEMClassLib`.
- **Packaging & Deployment:** Build the installer package using the WiX project in `FAARFIELD.Installer`.

---
