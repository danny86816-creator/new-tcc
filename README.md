# Trading Command Center

This repository contains the approved Phase 1 foundation for the Trading Command Center rebuild.

## Phase 1 scope

- Governance baseline record and accepted architecture decisions.
- .NET 10 / WPF solution skeleton for Windows x64.
- Package boundaries for presentation contracts, the Theme runtime, Theme feature UI, Windows adapters, and the desktop composition root.
- Minimal xUnit architecture test runner.

No Theme schemas, Theme API contracts, validators, persistence models, package lifecycle, preview, switching, accessibility runtime, or other Phase 2–8 implementation is included.

## Prerequisites

- Windows x64.
- .NET 10 SDK. `global.json` accepts the latest installed .NET 10 feature band starting from 10.0.100.

## Build and test

Generate or intentionally update lock files only when dependencies change:

```powershell
dotnet restore Tcc.slnx --force --no-cache
```

Formal validation and CI must use the committed lock files and the explicit x64 solution platform:

```powershell
dotnet restore Tcc.slnx --locked-mode --force --no-cache
dotnet build Tcc.slnx --configuration Release --no-restore -p:Platform=x64
dotnet test Tcc.slnx --configuration Release --no-build -p:Platform=x64
```

The approved Theme Architecture remains immutable in the external governance factory. Its identity and approval interpretation are recorded in `docs/governance/TCC_THEME_ARCHITECTURE_BASELINE_APPROVAL.md`.
