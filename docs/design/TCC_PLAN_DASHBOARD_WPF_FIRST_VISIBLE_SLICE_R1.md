# TCC Plan Dashboard WPF First Visible Slice R1

**Status:** IMPLEMENTED / VALIDATED / HUMAN VISUAL ACCEPTED / LOCAL COMMIT EXISTS
**Authority:** Human instruction `那就坐下一個板塊的WPF`, followed by standalone `1` accepting the proposed `UX-PLAN-001 — Trade Plan Dashboard` scope; later instructions `生成一個適合這個板塊的主題背景吧` and `風格要跟HOME一致`, followed by standalone `1` authorizing integration and validation on 2026-10-02. Decision233 separately authorizes the exact product local commit after the completed Runtime gate. Human standalone `1` after that delivery formally accepts the visual result under Decision234, and the later explicit `123都做吧 開始` authorizes Decision235's exact five-file acceptance-record local commit. Neither authority permits push or publication.
**Formal sources:** Approved UX Architecture `UX-PLAN-001`, `MOD-PLAN-WORKBENCH`; Product Constitution safety and Q93 requirements; approved System/Theme Architecture; repository Decisions 223–224 as retained gate/custody boundaries and Decisions 225–226 as this candidate's implementation authority.

## Objective

Deliver the smallest coherent WPF presentation slice for the next formally ordered navigation destination, `Plan` / `UX-PLAN-001`, including its accepted HOME-style decorative background, without adding business, persistence, connector, execution, or navigation-routing behavior.

## Included scope

- A compiled `PlanDashboardView` in `Tcc.DesktopHost`.
- Explicit Draft versus Formal Snapshot distinction.
- Read-only plan dashboard presentation for normal/empty, loading, offline, error, and blocked states.
- Visible Home Safety Core semantics: Trading Permission, Total Risk, Current Positions, and Major Alerts.
- Static high-impact review sequence: current → proposed → risk impact → confirm.
- Source/freshness, GPT-optional, and read-only connector truth.
- One Plan-owned 1672×941 HOME-style moonlit mountain background, rendered as a noninteractive decorative layer behind existing semantic surfaces; accepted SHA-256 `19BC4132539F7614843F5B0955364DE8FC717B903F6C227651D9F786A40F3F05`.
- Runtime evidence activation through the already-existing `TCC_VISUAL_FIXTURE` mechanism using `PLAN_R1` variants.
- Focused architecture/presentation tests, status/decision synchronization, and required repository gates.

## Explicitly excluded scope

- Real navigation or enabling the existing left-rail Plan button.
- Plan creation, editing, validation, formalization, autosave, persistence, sharing, approval, recovery, or conflict resolution behavior.
- Domain/application/persistence models or services.
- Broker, exchange, prop-firm, order, or execution APIs.
- Connector writes, live market data, GPT dependency, or official GPT scoring.
- New NuGet packages, project references, public APIs, migrations, or architecture direction changes.
- Changes to APPROVED/FROZEN artifacts, HOME scene/atlas bytes, or existing HOME business semantics.
- Any character, UI text, data visualization, animation, parallax, or semantic content embedded in the background asset.
- Commit, push, tag, merge, release, deploy, or branch deletion.

## Files

- `src/Tcc.DesktopHost/PlanDashboardView.xaml`
- `src/Tcc.DesktopHost/PlanDashboardView.xaml.cs`
- `src/Tcc.DesktopHost/Tcc.DesktopHost.csproj`
- `src/Tcc.DesktopHost/Assets/Plan/Tcc.PlanDashboard.HomeStyle.Background.R1.png`
- `src/Tcc.DesktopHost/App.xaml`
- `src/Tcc.DesktopHost/MainWindow.xaml`
- `src/Tcc.DesktopHost/MainWindow.xaml.cs`
- `tests/Tcc.Architecture.Tests/PlanDashboardPresentationTests.cs`
- `tests/Tcc.Architecture.Tests/PhaseSixBootstrapArchitectureTests.cs`
- `tests/Tcc.Architecture.Tests/CurrentHomeAuthorityContract.cs`
- `docs/CODEX_DECISIONS.md`
- `docs/CODEX_PROJECT_STATUS.md`
- this phase specification

## Dependencies and architecture boundary

- Reuse only the existing DesktopHost WPF resources and the current composition root.
- Package the accepted PNG as an existing WPF `Resource`; no image library, loader service, or new runtime dependency is introduced.
- No project/dependency graph change.
- No direct dependency from Theme runtime/features into Domain, Persistence, Connector, AI, Security, or Recovery assemblies.
- The view owns presentation state only; it does not become a composition root or introduce product data authority.

## Risks

- Hidden duplicate UIA content when PLAN overlays HOME; mitigate by hiding the HOME page layer while preserving shell chrome/navigation.
- Text clipping at Windows Text Scale and constrained viewport; mitigate with an independently keyboard-scrollable plan surface, wrapping copy, and minimum hit-target sizing.
- Color-only safety meaning; mitigate with persistent textual state labels and redundant symbols/borders.
- A preview fixture being mistaken for production navigation; mitigate with explicit presentation-slice/offline labels and disabled navigation/actions.
- Historical HOME regression; mitigate with focused HOME/current-authority and full regression gates.
- Background detail reducing foreground contrast; mitigate with the existing high-opacity Plan panels, dark scene veil, protected low-detail left workspace, and actual Runtime inspection.
- Decorative pixels leaking into High Contrast or the accessibility tree; mitigate through existing `Tcc.Strata.SceneOpacity=0` High Contrast behavior, no AutomationId/Name/x:Name, and non-focusable/non-hit-testable image configuration.

## Phased gates

1. **Authority/plan gate:** explicit Human authority, formal `UX-PLAN-001` traceability, included/excluded scope, rollback and file list recorded.
2. **Structure gate:** compiled view exists, MainWindow hosts it without enabling navigation, accepted PNG is packaged as a WPF Resource, dependency graph unchanged.
3. **Semantic/accessibility gate:** Draft/Formal, Safety Core, risk/permission, source/freshness, keyboard focus, UIA labels/live state, contrast-safe semantics and required state surfaces are present.
4. **Runtime gate:** actual Release x64 WPF process displays `PLAN_R1`; native and constrained/text-scale captures are inspected; UIA names, roles, focusability, state and overflow are checked.
5. **Repository gate:** locked restore, focused tests, exact DesktopHost and solution Release x64 builds, full tests, Frozen hashes, scope scan, `git diff --check`, staged/untracked hygiene.

## Focused and full validation

- Focused `PlanDashboardPresentationTests`.
- Relevant HOME/current-authority and dependency-boundary tests.
- `dotnet restore Tcc.slnx --locked-mode --force --no-cache`.
- Solution and exact DesktopHost Release x64 builds with 0 warnings / 0 errors.
- Full Release x64 test suite with no failed or skipped tests.
- Existing Frozen authority hash verification.

## UI, DPI, Text Scale, High Contrast and screen-reader requirements

- 1280×720 minimum viewport and native 1672×941 viewport must retain a keyboard-operable overflow path.
- Text Scale must enlarge text independently; no whole-UI raster scaling or uniform `Viewbox`.
- At least 100% and enlarged Text Scale runtime inspection; DPI-stable logical layout evidence.
- High Contrast must use current system-color remapping, set decorative scene opacity to zero, and retain textual safety meaning.
- UI Automation Name/Role/State/HelpText must be truthful; no focusable unnamed controls.
- Page-state changes use a live-region announcement; tab order follows visible shell/action/scroll order.
- Reduced motion/transparency remain informationally equivalent; this slice adds no motion, sound, or parallax.

## Frozen hash, scope scan and Git hygiene

- Approved/Frozen source bytes remain unchanged and must match the existing 11/11 SHA-256 manifest.
- Scope scan must contain only the listed phase files plus the already-authorized status synchronization baseline.
- Existing 4,035 user-owned untracked files must remain preserved and excluded.
- Staged changes must remain 0; no `git add`, commit, push, tag, merge, release, deploy, or branch deletion.

## Rollback

For background-only rollback, remove the Plan PNG `Resource` entry, the bottom-layer `Image`, the PNG asset, its focused assertions, and Decision226/background status text. For full-slice rollback, remove the new Plan view/test/spec files; remove only the Plan host/layer and fixture branches from `MainWindow`; remove this phase's Decision/Status entries. The prior `f6bac94` HOME baseline remains intact. No migration or external state requires rollback.

## Validation result

- Focused Plan／compiled-boundary／XAML-field tests: 9/9 PASS.
- PhaseSix architecture suite: 161/161 PASS.
- Locked restore: PASS.
- Solution Release build: 0 warnings / 0 errors.
- Exact DesktopHost Release x64 build: 0 warnings / 0 errors.
- Full Release regression: 1866/1866 PASS, failed 0, skipped 0.
- Runtime states: Offline／Empty／Loading／Error／Blocked all launched from the actual Release x64 executable and exposed truthful UIA live-state regions.
- Runtime background: accepted 1672×941 asset is visible behind Plan only; native screenshot SHA-256 `0382805BFB34857AE6329C27FEC7E076D363919FED4ECFC1CC196112E84502D`; 1280×720 screenshot SHA-256 `8F455A7E2E215822EB27327ACB648DE2E59B5AB7C29BC9DDA3BC53C2CEA18EF7`.
- Runtime accessibility: native and 1280×720 minimum viewport at Windows Text Scale 152%, DPI 96; keyboard-scrollable two-axis overflow; 13/13 expected Plan semantic targets; focusable unnamed controls 0; Plan scroller focus verified.
- Actual OS High Contrast: decorative background absent, readable system-color remapping verified, screenshot SHA-256 `42C753DA21B5D1752295CE2FF6C14C03A865F7800F17B7A431527B424DCDEECC`, and the original OS state restored.
- Actual Narrator: one actual Narrator process exercised the named five-step focus sequence `PlanDashboardScroller → MainContentScroller → Minimize → Maximize/Restore → Close`; machine-readable speech transcript is unavailable, so evidence is `PASS_WITH_TRANSCRIPT_LIMITATION` rather than a claimed transcript.
- Frozen accepted authorities: 11/11 SHA-256 MATCH.
- Git identity: branch `codex/phase6-first-visible-ui`; local HEAD, upstream and GitHub remote branch all remain `f6bac940604271977df031dcb00503a6f10d0871`.
- Git hygiene: `git diff --check` PASS; staged changes 0; dependency graph unchanged; 4,035 pre-existing user-owned untracked files preserved, with only five phase source／test／spec／asset files and six excluded prior Runtime evidence files added to the repository untracked inventory (total 4,046). New background Runtime evidence remains outside the repository.
- No commit, push, tag, merge, release, deploy or branch deletion was performed.
