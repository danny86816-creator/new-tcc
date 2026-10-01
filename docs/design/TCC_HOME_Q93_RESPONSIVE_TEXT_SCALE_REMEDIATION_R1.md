# TCC HOME Q93 Responsive／Text-Scale Remediation R1

**Status:** IMPLEMENTED — FULL-COMPOSITION BLOCKER RESOLVED BY RECONCILIATION R1
**Authorized by:** Human explicit authorization on 2026-10-01
**Phase boundary:** HOME full-composition Q93 remediation only

**Integration follow-up:** Decision223 reconciled all 99 named failures; the scoped clean-index commit gate passes 1837/1837 with all 99 original identities present and passing.

## Goal

Remove whole-UI raster-like scaling from the accepted 1672×941 HOME composition. At the minimum 1280×720 window and enlarged Windows text, preserve real text sizes and control hit targets, provide a keyboard-operable two-axis overflow path, and make Runtime UIA evidence cover every current AutomationId.

## Files in Scope

- `src/Tcc.DesktopHost/MainWindow.xaml`
- `src/Tcc.DesktopHost/MainWindow.xaml.cs` only if text-scale plumbing requires correction
- `automation/capture_strata_matrix_r1.ps1`
- focused Q93 contracts in `tests/Tcc.Architecture.Tests`
- this plan, `docs/CODEX_PROJECT_STATUS.md` and `docs/CODEX_DECISIONS.md`
- new Runtime evidence under `output/tcc-home-q93-responsive-text-scale-remediation-r1/`

## Dependencies and Locked Boundaries

- Preserve the accepted 1672×941 composition, all Phase 2 structural-unit geometry, exact visible product copy and offline／read-only／unknown semantics.
- Preserve scene, character and Mental State atlas bytes; do not edit accepted or Frozen authorities.
- Preserve the three real window actions and all unavailable navigation／market controls.
- Keep `Tcc.DesktopHost` as the sole composition root; do not add dependencies, public APIs, migrations or package references.
- Q93 and `TCC_WPF_SCALING_STRATEGY.md` are authoritative: no full-UI `Viewbox`, `LayoutTransform` or uniform `ScaleTransform`; text and hit targets follow DPI／Text Scale independently.

## Implementation

1. Replace the outer whole-composition `Viewbox` with a direct fixed-reference Canvas inside an `Auto`／`Auto` `ScrollViewer`.
2. Make the overflow shell keyboard-focusable and panning-capable without adding product interaction or a new AutomationId.
3. Keep the composition Canvas at 1672×941 so the minimum viewport reveals it through overflow instead of shrinking it.
4. Update focused architecture contracts to enforce the current accepted Canvas composition, forbid whole-UI scaling, and require the accessible overflow path.
5. Update the Runtime capture harness so its explicit UIA inventory equals all 29 current source AutomationIds and records focusability／control type plus scroll-pattern evidence.

## Risks

- Auto scrollbars reduce the initially visible minimum-viewport area and require proof that right／bottom content remains reachable.
- A fixed composition may expose existing text clipping that the outer `Viewbox` previously concealed.
- High Contrast changes the real OS theme and must be restored after evidence capture.
- Narrator can verify traversal and announcements, but this environment may not provide a machine-readable speech transcript; any limitation must be disclosed rather than replaced with UIA-only claims.

## Acceptance

- No whole-UI `Viewbox`, `LayoutTransform` or uniform `ScaleTransform` remains around the HOME composition.
- At 1280×720, text and the 54×44 window-button hit targets are not geometrically reduced; horizontal and vertical overflow are keyboard reachable.
- Focused Q93 tests pass, including the two previously failing named contracts.
- Runtime capture discovers 29/29 AutomationIds. `Region.HeaderIdentity` and `Editorial.RightMaxim` are independently present, named and non-focusable.
- Native and minimum Runtime screenshots are visually inspected; minimum evidence includes overflow reachability rather than claiming the whole 1672×941 composition is simultaneously on-screen.
- Actual OS High Contrast is captured and restored. Narrator or NVDA is exercised when available, with Name／Role／State and duplicate-announcement findings recorded honestly.
- Locked restore, Release x64 build, full tests, Frozen hash verification, scope scan and Git hygiene complete; status files match actual results.

## Rollback

Revert only this remediation's outer-shell XAML, focused contracts, capture-harness changes, plan／governance updates and new Runtime evidence. Do not revert or overwrite the pre-existing dirty HOME implementation.

## Result

- The whole-composition `Viewbox` is removed. The accepted 1672×941 Canvas now remains at native geometry inside an `Auto`／`Auto`, two-axis, keyboard-focusable `ScrollViewer`; 1280×720 exposes real overflow instead of shrinking typography or hit targets.
- Runtime evidence discovers 29/29 explicit HOME AutomationIds. `MainContentScroller` is keyboard-focusable; `Region.HeaderIdentity` and `Editorial.RightMaxim` are named, enabled and non-focusable.
- Native Runtime remains 1672×941. Minimum Runtime proves two-axis `ScrollPattern` at 0%／0% and 100%／100%; all 20 application buttons remain named, and the eight framework scroll-part buttons are non-focusable.
- Actual OS High Contrast capture passed and the original High Contrast state was restored. Narrator 10.0.26100.8875 completed the four-item Tab loop and a 160-command scan traversal over 142 named Control View nodes; no meaningful adjacent duplicate names were found. Audio speech is not machine-transcribed, so the saved evidence explicitly retains that limitation.
- Focused Q93 contracts pass, including both previously failing named contracts. Full regression improved from 1756/1857 with 101 failures to 1759/1858 with 99 failures: one new coverage test passed and two Q93 failures resolved, with no new failure names.
- Repository completion and commit remain blocked because 99 disclosed historical／authority conflicts still fail. No test was deleted, skipped or weakened to hide those failures.

## Verification

- Locked restore: PASS.
- Release build: 0 warnings／0 errors.
- Full tests: 1759/1858 PASS; 99 FAIL; skipped／error／timeout／aborted 0.
- HOME tests: 34/37 PASS; the same three historical visual-contract failures remain.
- Frozen accepted specifications／authority images: 11/11 SHA-256 MATCH.
- Scene SHA-256: `B4C4B7225D75C5A3408E8BF683EEDA7756C6E4398560021BC1F4FE2996C385C2`.
- Mental State atlas SHA-256: `EA15A6E24FEE80A3395DDE775E52F340FC2A64AFF6DA40EC1C08C2FCBDFE20C1`.
- Git: branch `codex/phase6-first-visible-ui`; HEAD `d8756402e7a5dfbfa2b42711c7e23a4d34c1675f`; staged 0; pre-existing dirty baseline preserved; no commit／push／release／deploy.
