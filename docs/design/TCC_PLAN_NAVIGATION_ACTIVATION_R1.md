# TCC Plan Navigation Activation R1

**Status:** IMPLEMENTED / REPOSITORY VALIDATED / RUNTIME UI VERIFIED / LOCAL COMMIT AUTHORIZED
**Authority:** Human standalone `1` on 2026-10-03 accepted the proposed `UX-PLAN-001 Plan Navigation Activation R1` next action. After a standalone `2` review exposed a child-label visual-state gap, Human standalone `1` on 2026-10-04 authorized the `UX-PLAN-001 Plan Navigation State Closure R1` refinement recorded as Decision232. Decision233 separately authorizes the exact local commit after the completed Runtime gate. This work is not named Phase7 and does not authorize push or publication.
**Formal sources:** Approved Questionnaire; Approved Product Constitution; Approved UX Architecture `UX-PLAN-001`; Approved System Architecture; Approved Theme Architecture; Decisions225–232; current uncommitted Plan presentation candidate.

## Objective

Activate the smallest coherent, real Home ↔ Plan shell navigation path while keeping the Plan page read-only, offline/empty-truthful, and free of product-data behavior.

## Included files

- `src/Tcc.DesktopHost/MainWindow.xaml`
- `src/Tcc.DesktopHost/MainWindow.xaml.cs`
- `tests/Tcc.Architecture.Tests/PlanDashboardPresentationTests.cs`
- `tests/Tcc.Architecture.Tests/CurrentHomeAuthorityContract.cs`
- `tests/Tcc.Architecture.Tests/PhaseSixBootstrapArchitectureTests.cs`
- `tests/Tcc.Architecture.Tests/HomeStrataObservatoryTests.cs`
- `docs/design/TCC_PLAN_NAVIGATION_ACTIVATION_R1.md`
- `docs/CODEX_DECISIONS.md`
- `docs/CODEX_PROJECT_STATUS.md`

## Dependencies

- Reuse the existing `Tcc.DesktopHost` composition root, `MainWindow`, and compiled `PlanDashboardView`.
- No new project, package, `ProjectReference`, public API, migration, service, or dependency direction.
- The current Plan preview-state mechanism remains presentation-only and does not become data authority.

## Explicit exclusions

- No list/filter/open/resume workflow.
- No create, edit, delete, validation, formalization, autosave, persistence, sharing, approval, recovery, or conflict-resolution behavior.
- No `UX-PLAN-002` through `UX-PLAN-005` implementation.
- No Domain, Application, Persistence, connector-write, broker, exchange, prop-firm, order, or execution implementation.
- No change to Home Safety Core semantics, GPT optionality, official-score boundaries, Theme Package semantics, or Gu Qinghan packaging.
- No edit to any `APPROVED`, `FINAL`, `LOCKED`, or `FROZEN` artifact.
- No commit, tag, push, merge, release, deploy, or branch deletion.

## Risks and controls

- **False current-page state:** one private shell-state method updates visibility, selected style, current marker, tooltip, UIA Name/ItemStatus/HelpText, scroller identity, and title together.
- **Stale child-label emphasis:** the HOME and PLAN labels bind their foreground to their existing parent navigation buttons, so the selected/unselected foreground follows the same button-style swap without new names, generated fields, or code-behind state.
- **Keyboard or screen-reader dead end:** Home and Plan remain ordinary focusable WPF buttons with `Click` activation; focus stays on the invoking control and the page change is announced through a polite live region.
- **Hidden duplicate HOME semantics:** exactly one of `HomePageLayer` and `PlanDashboardSurface` is visible.
- **Plan behavior expansion:** navigation always enters the existing truthful offline preview and introduces no data operation.
- **Historical HOME regression:** focused Plan/HOME/current-authority tests precede full repository gates.

## Acceptance gates

1. **Contract gate:** Home and Plan are enabled, tab-focusable, named buttons with real `Click` handlers; all other unavailable navigation stays disabled.
2. **State gate:** mouse/keyboard activation switches exactly one page surface and synchronizes button and child-label visual emphasis, current markers, tooltip, UIA state, scroller name/help, and window title.
3. **Safety gate:** Plan opens in the existing offline read-only presentation; no product-data or execution behavior is added.
4. **Accessibility/runtime gate:** actual Release x64 UIA verifies Name/role/state, focus continuity, Home ↔ Plan activation, normal and constrained layouts, text scale, High Contrast where safe, and no focusable unnamed controls.
5. **Repository gate:** focused tests, locked restore, Release x64 builds, full tests, Frozen hash verification, scope scan, `git diff --check`, staged/untracked/secret/artifact hygiene.

## Decision232 state-closure refinement

- Exact scope: `MainWindow.xaml`, `PlanDashboardPresentationTests.cs`, `HomeStrataObservatoryTests.cs`, this specification, Decision ledger, and project status.
- HOME and PLAN label foregrounds bind to the existing `NavHome` and `NavPlanning` button foregrounds by `ElementName`; there is no new `x:Name`, generated field, public type, code-behind branch, resource, dependency, or product state.
- Focused contracts now lock both label bindings plus the complete existing state tuple: surfaces, button styles, CURRENT markers, tooltip, UIA Name/ItemStatus/HelpText, outer scroller identity, title, offline Plan state, and live-region event.
- Final repository evidence: focused10/10, single compiled-surface1/1, clean expanded207/207, full1868/1868, failed0/skipped0; locked restore PASS; solution Release and exact DesktopHost Release x64 builds 0 warnings/0 errors; Frozen authority hashes11/11 MATCH.
- The first full run was stopped after the compiled-surface gate exposed an unauthorized generated public type while evaluating the initial ancestor-type binding candidate. The final implementation uses existing element names; exact clean/rebuild restored the approved compiled surface without an allowlist change, skip, or weakened assertion.
- Runtime remains blocked because `winapp ui` is unavailable and the current Computer Use runtime exposes no native app/window APIs. Source, contract, and earlier fixture evidence do not substitute for the new navigation Runtime gate.

## Rollback

For the Decision232 refinement, restore only the six exact pre-refinement files in `C:\Users\danny\.codex\rollback-snapshots\20261004-plan-navigation-state-closure-r1-pre`. The original Decision231 rollback remains `C:\Users\danny\.codex\rollback-snapshots\20261003-plan-navigation-activation-r1-pre`. Do not use destructive Git commands and do not touch unrelated user-owned files.
