# TCC Plan Dashboard WPF Visual Alignment R2

**Status:** IMPLEMENTED / VALIDATED / HUMAN VISUAL ACCEPTANCE PENDING
**Authority:** Human accepted the HOME-tone low-complexity visual master and then used standalone `1` to authorize this exact R2 WPF implementation and validation plan on 2026-10-02. After read-only focus audits and an isolated exact-input prototype proved that the existing outward focus adorner disappears at the minimum viewport, Human used standalone `1` again to authorize the six-file viewport-safe focus refinement. A later read-only actual-Windows-High-Contrast audit established `DTK-A11Y-007`: Windows replaced the root `MainContentScroller` focus adorner with a clipped one-pixel edge. Human then used standalone `1` to authorize the exact five-file High-Contrast focus-equivalence refinement recorded by Decision229. On 2026-10-03, Human explicitly required the Plan WPF text to use Chinese; Decision230 authorizes the bounded Traditional-Chinese copy refinement without changing product behavior or the shared HOME shell. Decision233 separately authorizes the exact local commit after the completed Runtime gate; it does not name Phase7 or authorize push, tag, merge, release, deploy, or branch deletion.
**Visual authority:** external preview SHA-256 `8C59FE204B615DE494649EBC141D5C63DA31F53E1EB7C7C0ED116A5D416E89AF`; layout, palette, material hierarchy, and relative emphasis only. The preview is not a Runtime asset.
**Product authority:** `UX-PLAN-001`, `MOD-PLAN-WORKBENCH`, Decisions225–226, Approved Product Constitution／UX／System／Theme authorities, and retained Decisions223–224 custody boundaries.

## Objective

Align the existing presentation-only Plan candidate with the accepted HOME night-ink visual direction while keeping implementation complexity at the HOME tier: one existing decorative scene, true WPF controls, four reusable rectangular surface types, and Grid／StackPanel layout. Preserve every existing product, safety, preview-state, accessibility, and dependency boundary.

## Included scope

- Recompose `PlanDashboardView.xaml` into one continuous Home Safety Core strip, one dominant Plan Workbench, one narrower Safety Review／Source column, and one explicitly read-only Next Action direction.
- Keep Draft and Formal Snapshot visibly distinct.
- Keep the current → proposed → risk impact → confirm sequence visible with truthful `UNKNOWN`／`UNAVAILABLE` values.
- Preserve Empty／Loading／Offline／Error／Blocked preview states and their existing live-region code path.
- Use exactly four Plan-local reusable surface styles: Primary, Secondary, Step, and Action.
- Use existing DynamicResource-backed Strata materials and the existing Plan-only decorative scene.
- Keep the Next Action direction visibly unavailable, non-focusable, non-hit-testable, and free of command／click authority; it must not resemble an enabled CTA.
- Give both required scroll containers inward two-DIP viewport-safe focus visuals that reuse the existing Strata Ice semantic brush and remain visible when either control meets or exceeds the real window bounds. `PlanDashboardScroller` retains its local focus template; the root `MainContentScroller` uses a non-hit-testable visual-tree overlay keyed only to its exact keyboard-focus state so actual Windows High Contrast cannot replace or clip the boundary.
- Add focused structural tests and synchronize this specification, Decision ledger, and project status after final validation.
- Present all Plan-owned visible copy, tooltips, UIA Name／HelpText, live-region state messages, and the Plan fixture window title in Traditional Chinese. Keep protocol identifiers, AutomationIds, fixture codes, `GPT`／`AI`, `PageDown`／`Tab`, and authority IDs unchanged where translation would damage technical identity.

## Explicit exclusions

- No real navigation, plan CRUD, validation, formalization, autosave, persistence, approval, recovery, or conflict-resolution behavior.
- No Domain／Application／Persistence service, connector write, order, broker, exchange, prop-firm, or execution API.
- No new code-behind behavior, `x:Name`, compiled type, public API, NuGet package, ProjectReference, migration, project Resource, or dependency direction.
- No modification of shared App, DesktopHost project, HOME authority contract, PhaseSix compiled boundary, HOME scene／atlas, or the Plan background asset. `MainWindow.xaml` is included only for the `MainContentScroller` viewport-safe focus behavior; no other shared-shell byte is authorized.
- No localization of the shared HOME top chrome, navigation rail, editorial marks, or HOME page copy in the Decision230 refinement; that would require separate cross-surface authority.
- No character, animation, parallax, irregular clipping, ornate mask, embedded UI raster, or master-image inclusion.
- No APPROVED／FROZEN byte modification and no Git publication operation.

## Files

- `src/Tcc.DesktopHost/PlanDashboardView.xaml`
- `src/Tcc.DesktopHost/MainWindow.xaml`
- `tests/Tcc.Architecture.Tests/PlanDashboardPresentationTests.cs`
- `docs/design/TCC_PLAN_DASHBOARD_WPF_VISUAL_ALIGNMENT_R2.md`
- `docs/CODEX_DECISIONS.md`
- `docs/CODEX_PROJECT_STATUS.md`

All other candidate and user-owned files are out of scope. If another product or test file becomes necessary, stop and request new authority.

Decision229's High-Contrast focus-equivalence refinement is narrower than the full R2 file set: it authorizes only `MainWindow.xaml`, `PlanDashboardPresentationTests.cs`, this specification, the Decision ledger, and project status. `PlanDashboardView.xaml` is custody-only for that refinement and must not change.

Decision230's Traditional-Chinese copy refinement authorizes exactly seven repository paths: `PlanDashboardView.xaml`, `PlanDashboardView.xaml.cs`, the Plan-only strings in `MainWindow.xaml.cs`, `PlanDashboardPresentationTests.cs`, this specification, the Decision ledger, and project status. Layout geometry, styles, assets, `MainWindow.xaml`, other HOME copy, public APIs, dependencies, and business semantics are excluded.

## Architecture and asset boundary

- `Tcc.DesktopHost` remains the sole composition root.
- `PlanDashboardView` remains presentation-only and owns no business authority.
- The accepted full-screen preview is an authority map, not a shippable bitmap.
- Production continues to use `Assets/Plan/Tcc.PlanDashboard.HomeStyle.Background.R1.png`, SHA-256 `19BC4132539F7614843F5B0955364DE8FC717B903F6C227651D9F786A40F3F05`.
- All text, state, step, source, safety, and next-action content remains native WPF.
- The decorative Image remains non-focusable, non-hit-testable, unnamed in UIA, and removed through `Tcc.Strata.SceneOpacity=0` in High Contrast.

## Accessibility requirements

- Keyboard-operable two-axis overflow at 1672×941 and 1280×720; the minimum-viewport path must cover both vertical PageDown and horizontal Left／Right Arrow input on the outer page scroller.
- At 1280×720, UIA HelpText must explain the bounded two-stage keyboard path: PageDown in `PlanDashboardScroller`, then Tab to `MainContentScroller`, PageDown again, and use Left／Right Arrow to inspect the full width. UIA `IsOffscreen` alone is not sufficient evidence when an ancestor scroller can still clip the descendant.
- No Uniform Viewbox, LayoutTransform, or ScaleTransform for whole-UI scaling.
- Text Scale enlarges text independently and must not clip or hide required meaning.
- Every interactive element exposes truthful UIA Name／Role／Value／State; focusable unnamed controls remain zero.
- Visible Plan copy and its corresponding UIA Name／HelpText／live-region messages use the same Traditional-Chinese meaning. Technical keyboard keys remain written as `PageDown`／`Tab`／Left／Right Arrow equivalents so instructions match the physical platform input.
- Focus indication reuses HOME's two-DIP Strata Ice treatment. `PlanDashboardScroller` uses its local inward-margin template rather than the shared outward `Tcc.Strata.FocusVisual`, whose negative margin is clipped at the minimum viewport. `MainContentScroller` sets `FocusVisualStyle={x:Null}` and uses a sibling root overlay with `Margin=2`, `BorderThickness=2`, `Tcc.Strata.Brush.Ice`, `IsHitTestVisible=False`, and `Focusable=False`; its visibility follows only `MainContentScroller.IsKeyboardFocused`, never descendant focus. This preserves distinct Plan versus outer focus, normal pixels, Tab order, UIA, and actual Windows High Contrast. State meaning is never color-only.
- Action styling must remain truthful: the unavailable Next Action direction is explicitly labelled and visually quieter than an enabled control.
- High Contrast removes the decorative scene and preserves system-color text, boundaries, and safety labels.
- Actual Narrator focus traversal is required; lack of a machine-readable transcript must be reported as a limitation, never fabricated.

## Gates

1. **Baseline custody:** capture exact pre-R2 hashes outside the repository; the viewport-focus refinement additionally uses `C:\Users\danny\.codex\rollback-snapshots\20261002-plan-r2-viewport-focus-pre`; the High-Contrast focus-equivalence refinement uses `C:\Users\danny\.codex\rollback-snapshots\20261003-plan-hc-focus-equivalence-pre`; the Traditional-Chinese copy refinement uses `C:\Users\danny\.codex\rollback-snapshots\20261003-plan-zh-tw-copy-pre`; staged zero; user untracked files preserved.
2. **Structure:** the full R2 delivery stays within the six listed paths, and Decision229 stays within its exact five-file subset. The sole current shared-shell edit replaces the root scroller's local focus adorner with the visual-tree overlay described above. No new `x:Name`, code-behind, resource registration, dependency, compiled type, or other shared-shell edit.
3. **Focused tests:** Plan presentation, Current HOME authority, and PhaseSix compiled-boundary suites pass.
4. **Runtime:** actual Release x64 WPF at native and minimum viewport; inspect normal five preview states, overflow, text scale, UIA, focus, High Contrast, and Narrator.
5. **Repository:** locked restore, exact DesktopHost and solution Release x64 builds, full tests, Frozen 11/11 hashes, scope scan, secret／artifact scan, `git diff --check`, staged and untracked hygiene.

## Rollback

For full-R2 rollback, restore only the exact pre-R2 bytes recorded under `C:\Users\danny\.codex\rollback-snapshots\20261002-plan-r2-pre` and remove only this R2 specification and its Decision／Status additions. For the first viewport-focus refinement, restore the six exact pre-refinement bytes under `C:\Users\danny\.codex\rollback-snapshots\20261002-plan-r2-viewport-focus-pre`. For Decision229 only, restore the five exact pre-refinement bytes under `C:\Users\danny\.codex\rollback-snapshots\20261003-plan-hc-focus-equivalence-pre`. For Decision230 only, restore the seven exact pre-localization bytes under `C:\Users\danny\.codex\rollback-snapshots\20261003-plan-zh-tw-copy-pre`. Do not use destructive Git commands and do not alter the Decision225–226 candidate, `f6bac94` baseline, existing Plan asset, or user-owned untracked files.

## Validation result

- Final focused Plan presentation tests: 9/9 PASS; the focus-refinement expanded Plan／Current HOME／PhaseSix selection is 296/296 PASS. An earlier broader R2 selection was 311/311 PASS; exact-byte closure is the final full regression. All reported selections have 0 failed and 0 skipped.
- Decision229 closure: focused Plan 9/9 and the current broad-name Plan／HOME／PhaseSix selection 225/225 PASS; locked restore PASS; solution Release and exact DesktopHost Release x64 builds 0 warnings／0 errors; full regression 1868/1868 PASS with 0 failed and 0 skipped; Frozen authorities 11/11 MATCH; exact five-file scope, secret, forbidden-artifact, diff and Git-hygiene gates PASS.
- Locked restore PASS; solution Release and exact DesktopHost Release x64 builds: 0 warnings／0 errors.
- Final repository full regression: 1868/1868 PASS; 0 failed, 0 skipped.
- Actual Release x64 Runtime at DPI96／Windows Text Size152%:
  - 1672×941 native SHA-256 `4B9645CB9E8F88E4F8BCB3F761842EE63FEF5D7560E6CF55D082154BDC8B336C`; native width has no unnecessary horizontal overflow.
  - 1280×720 minimum SHA-256 `D3B647FB7BF813EC5519860DA410546EBA6D6F862C87DB07B0435202D9DF0C5A`; two-axis overflow remains keyboard-focusable and preserves all content.
  - Actual Windows High Contrast SHA-256 `6CCB0882CBE1877D74B905BF34040CED6D09A108B826B34049A7557B1B7756D8`; decorative scene removed and the original OS setting restored.
- Runtime UIA: all 12 visible required Plan targets found, offscreen0, focusable unnamed controls0, Plan scroller focus verified. The retained legacy `Plan.Filter.Drafts` identity is intentionally on a collapsed compatibility element and is not represented as a visible Runtime target.
- Focus-visibility refinement: the pre-change 1280×720 Plan-focused screenshot was byte-identical to the unfocused baseline because the outward `Margin=-3` adorner was fully clipped. The authorized local inward templates use `Margin=2`, `BorderThickness=2`, and `Tcc.Strata.Brush.Ice`. At 1280×720, Plan focus SHA-256 is `3E07435E04991890E2E8071B7590D64A68690919BB28D5DFE8DCC8FCEA43F8BC`, outer focus SHA-256 is `E04161C2B6DF6BFEBD95C862E757669FF21BD22B6FC1F659EE792BA6B479D8A5`, and both remain visible even though the Plan bounds exceed the window right／bottom boundaries. Against the unfocused baseline, Plan changes 3,920／921,600 pixels (0.4253%), outer changes 7,952／921,600 (0.8628%), and the two focus states differ by 4,032／921,600 (0.4375%). At 1672×941, Plan focus SHA-256 is `D1A3D30E91DB33789559E57F1BD94B80A725A0545923207BDBC299465D1FE1BC` and outer focus SHA-256 is `5537D208864FA5A8BC3187DDE20EA60A68E20E6DDFEA2CEB11004412FF292903`. Actual High Contrast Plan／outer focus hashes are `D35565517F2C00EB702C336B41899F2574CDD6B3A6ACC377FBCAB68748A00F66`／`5A59B142AFAB45DB479715983463B1C6FD630B3B3D539DA39DE1695BDE662E5A`; both focus boundaries remain visible, `CAPTURE_SUCCEEDED=True`, and `HIGH_CONTRAST_RESTORED=True`.
- Decision229 High-Contrast focus-equivalence refinement: native and minimum normal-mode hashes remain exactly unchanged (`D1A3D30E...E1BC`／`5537D208...2903` and `3E07435E...8F8BC`／`E04161C2...8A5`). Actual High Contrast Plan／outer hashes are `971A82C37800170B76F652B888E986F6D0AF154C7C3CF5F85F6EA8A449A652C8`／`715525E8CAB87A7CDEDDCA1EE653FBC2D40C89ECE4957BC092924D73BE5CF2C5`. The outer screenshot contains exactly 10,404 `#8EE3F0` pixels in its outer eight-pixel band, equal to the expected complete two-pixel 1672×941 perimeter; Plan focus does not activate the outer overlay. Focus remains `PlanDashboardScroller` versus `MainContentScroller` in UIA, `CAPTURE_SUCCEEDED=True`, and `HIGH_CONTRAST_RESTORED=True`.
- Native refinement audit: one keyboard PageDown in `PlanDashboardScroller` moves its vertical scroll from 0% to 100% and exposes Next Action on-screen. Its exact visible text is `Review risk before formalizing · unavailable in this slice`; it is non-focusable, non-hit-testable, has no command／click target, and uses a standalone directional glyph rather than a button-like box. Final native PageDown screenshot SHA-256 is `FE3C8E100D51E768177B224AA1813C8B14A5336D8DFB00554B7BC6661EB977BB`.
- Minimum refinement audit: at 1280×720, inner Plan PageDown alone leaves the lower region clipped by the outer fixed composition even though UIA reports `IsOffscreen=false`; the verified keyboard path is inner PageDown → Tab to `MainContentScroller` → outer PageDown. This brings the full unavailable Next Action text into the actual viewport. Pixel screenshot SHA-256 is `F934002E428EF9553DE9C55924ED0C1313D81CF286E89797B928D3B7A08714A1`; screen-space bounds and native screenshot, not `IsOffscreen` alone, are the acceptance evidence. UIA HelpText uses the stable phrase `outer page scroller` because fixture mode changes the outer scroller's runtime Name.
- Minimum two-axis audit: with the outer scroller focused after the two-stage vertical path, platform-standard Right Arrow input moves horizontal scroll from 25.43% to 100% while vertical remains at 100%. Next Action bounds become fully inside the window (`left371`, `right1183`, `top646`, `bottom669`), and the complete Safety／Source column plus directional glyph are visible. Screenshot SHA-256 is `83BDCA4BFEDDC4095D2ABF749D797D392EE38BC064C058CC59D108DDC51CF052`; `Ctrl+End` was tested and correctly rejected as unsupported guidance because it did not change horizontal percent.
- Final High Contrast PageDown screenshot SHA-256 is `F16DB47B8A475D2C006C8171B4CF10336B89B7D843E686525D9DFA01B6FA82C9`; the unavailable label and unboxed direction remain visible, the decorative scene is removed, and the original OS setting was restored.
- All five Runtime fixtures (`PLAN_R1`, `PLAN_R1_BLOCKED`, `PLAN_R1_EMPTY`, `PLAN_R1_ERROR`, `PLAN_R1_LOADING`) expose their exact truthful `Plan.StateRegion` names on-screen.
- Actual Narrator 10.0.26100.8875 exercised the five-step named focus sequence `PlanDashboardScroller → MainContentScroller → Minimize → Maximize/Restore → Close`; Narrator was restored inactive. Spoken-output transcript is unavailable in this automation channel, so the result is `PASS_WITH_TRANSCRIPT_LIMITATION`.
- The accepted master remains external visual authority only; production continues to ship the existing Plan decorative background hash `19BC4132...3F05`, with no UI bitmap inclusion.
- Frozen authority custody is 11/11 MATCH. Exact six-file scope scan, `git diff --check`, staged-zero, untracked preservation, branch／HEAD／upstream／remote identity and final Git hygiene are recorded in `docs/CODEX_PROJECT_STATUS.md`.
- Decision230 Traditional-Chinese copy closure: focused Plan presentation tests 9/9 PASS；current broad-name Plan／HOME／PhaseSix selection 225/225 PASS after exact x64 clean removed a stale `XamlGeneratedNamespace.GeneratedInternalTypeHelper` output；full regression 1868/1868 PASS with 0 failed and 0 skipped. Locked restore PASS；solution Release and exact DesktopHost Release x64 builds both 0 warnings／0 errors；Frozen authorities 11/11 MATCH；exact seven-file localization scope, secret／forbidden-artifact scan, `git diff --check`, staged-zero and untracked preservation PASS. No allowlist expansion, test skip or reduced gate was used.
- Decision230 Runtime copy evidence: all five fixtures expose their exact Traditional-Chinese `Plan.StateRegion` Name；each fixture has Plan targets12/12、focusable unnamed0、offscreen0. The current normal Plan-focus capture SHA-256 is `D4827E3D786F3B06615662C6FD2828DFE2C1AA4E7AF16B220AB2522E6521BADE`；the 1280×720 outer-focus capture is `1D07EF434502A6D9250C2113DD5E7B443DCF9355AD07CC47447F428F5DB5890D`；actual High Contrast Plan／outer focus captures are `619100232EAE28D547295A01FB417C4D9C1ECE903A281B31E0397D8A8FB6910F`／`532FE6A78C911503B5246529123EC7E121367DBFE130588AAB808C893CE66EBA`，with correct focus identity, `CAPTURE_SUCCEEDED=True`, and `HIGH_CONTRAST_RESTORED=True`.
