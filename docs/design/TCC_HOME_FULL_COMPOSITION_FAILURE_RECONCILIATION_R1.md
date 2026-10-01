# TCC HOME Full-Composition Failure Reconciliation R1

**Status:** COMPLETE — FULL-COMPOSITION GATE PASS
**Authorized by:** Human explicit authorization on 2026-10-01
**Phase boundary:** Reconcile the 99 failures recorded by `full-after-fix.trx`; no new product phase

## Goal

Reconcile every remaining full-suite failure against the current authority chain. Correct real implementation or test-harness defects, migrate assertions that still treat superseded HOME visual systems as current Runtime authority, and preserve historical evidence without deleting, skipping or weakening architecture coverage.

## Phase 1 — Baseline and authority classification (COMPLETE)

- Files: `full-after-fix.trx`, applicable approved decisions/specifications, failing test sources, current HOME Runtime sources.
- Dependencies: Decision160–222, Q93 scaling strategy, R26 scene authority, Phase1 dependency allowlist.
- Risks: treating obsolete visual locks as current authority; hiding a real product regression inside a broad test migration.
- Acceptance: all 99 failures assigned to a documented root-cause class; current-vs-historical authority is explicit.
- Rollback: remove this plan/status update only; no Runtime change in this phase.

## Phase 2 — Current-contract and harness remediation (COMPLETE)

- Files: only directly failing current-contract tests/harnesses and, if required, the smallest coherent Runtime source change.
- Dependencies: Phase 1 classification.
- Risks: loosening compiled-boundary checks or admitting generated evidence projects into the Phase1 project graph.
- Acceptance: focused current-contract gates pass; mutation/negative tests still reject their prohibited cases.
- Rollback: revert only this phase's focused edits and restore any renamed evidence snapshot to its original name.

## Phase 3 — Superseded visual-contract migration (COMPLETE)

- Files: historical P1/P2/P3/B3/Rebuild test suites plus a current-authority replacement contract if needed.
- Dependencies: explicit supersession decisions and passing Phase 2 gates.
- Risks: vacuous PASS, duplicate coverage, loss of historical asset custody.
- Acceptance: each migrated assertion validates either immutable historical custody or the named current replacement authority; no test deletion/skip and no reduction of negative checks.
- Rollback: revert only migrated test assertions; production assets and Frozen/Approved artifacts remain untouched.

## Phase 4 — Full validation and governance sync (COMPLETE)

- Files: `docs/CODEX_PROJECT_STATUS.md`, `docs/CODEX_DECISIONS.md` only if a long-lived decision is established, and new reconciliation evidence.
- Dependencies: Phases 1–3 complete.
- Risks: stale build output or dirty-tree artifacts producing misleading counts.
- Acceptance: locked restore, Release x64 build, focused gates, full tests, Frozen hashes, scope scan and Git hygiene all recorded truthfully; any residual blocker is named.
- Rollback: remove only this task's evidence/governance delta; preserve the pre-existing dirty baseline.

## Locked boundaries

- Do not modify Approved/Frozen authorities or accepted scene/atlas bytes.
- Do not change business semantics, dependency direction, Installer/Portable parity or Q93 accessibility.
- Do not delete, skip, soften or replace a failing test with a document-only/vacuous assertion.
- Do not commit, push, release or deploy.

## Result

- The baseline 99 failures are reconciled with no deleted or skipped tests and no Approved/Frozen byte changes.
- Current-contract defects were corrected: generated XAML fields are validated separately from authored member boundaries; loose-XAML asset URIs resolve against the repository; current title, resources, AutomationIds and truth copy are asserted; the build-shaped evidence snapshot no longer participates in project discovery.
- Superseded P1/P2/P3/B3/Rebuild Runtime assertions now execute a shared current-authority contract while `HomeLegacyVisualCustody.sha256` preserves exact historical asset/resource custody.
- Focused migrated suites pass 92/92; locked restore passes; Release x64 builds report 0 warnings/0 errors; Frozen hashes match 11/11; full tests pass 1859/1859 with 0 failed and 0 skipped.
- Full-Composition Integration Gate is `PASS`. No commit, push, release or deploy was performed.
