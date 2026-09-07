# Theme Contracts

This directory contains the Phase 2 declarative Theme schema and stable UX contract surface. It is derived from the Frozen UX and Theme Architecture; it does not replace either source.

## Contents

- `schemas/`: the 21 unique §33.2 JSON Schema files. The two repeated names in the source list are materialized once each.
- `ux-contract.v1.1.json`: complete stable ID registries and per-surface traceability with provenance.
- `page-contracts/`, `module-contracts/`, `zone-contracts/`, `state-contracts/`: deterministic generated contract projections.
- `accessibility-contract.v1.json`, `motion-contract.v1.json`, `audio-contract.v1.json`: cross-surface invariant contracts.
- `theme-manifest.schema.json`: the §6.7 generated-path projection of the canonical `schemas/ThemeManifest.schema.json`.

Regenerate deterministically from the repository root:

```powershell
./tools/phase2/Generate-ThemeContracts.ps1
```

The generator contains only approved IDs and explicit provenance. It rejects duplicate IDs and does not depend on a user-machine absolute path.

No file in this directory implements Theme runtime behavior.
