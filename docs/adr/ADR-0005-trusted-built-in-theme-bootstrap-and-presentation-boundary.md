# ADR-0005 — Trusted Built-In Theme Bootstrap and Presentation Boundary

**Status:** ADDED / CANDIDATE — TCC-P6-82 BOUNDARY / CONTRACT APPROVAL CANDIDATE.
**Date:** 2026-09-14.
**Authority:** explicit TCC-P6-82; TCC-DEC-2026-09-14-020.
**Implementation:** IMPLEMENTED / UNVALIDATED / UNSEALED. TCC-P6-83B clarifies the foundation-vs-UI validation split; this candidate is not independent approval or a seal.
**Scope:** Phase6A trusted built-in foundation → TCC-P6-84 independent validation → separately authorized TCC-P6-85 first real UI vertical slice and its UI/release validation matrix.

## 1. Context, authority and source audit

Source precedence: Approved Questionnaire Q1–Q110; later explicit requirements; approved Product Constitution; approved UX; Frozen System/Theme; sealed decisions/ADRs/baselines; legacy reference; assumptions last. A substantive conflict stops work.

Reviewed authority:
- Governance factory `config/SOURCE_BUNDLE.md`: SOURCE A Approved Questionnaire Q1–Q110; SOURCE B Product Constitution v1.0; SOURCE C approved Information & UX Architecture v1.1. Relevant traceability: Q3/Q5/Q6/Q38/Q39/Q60/Q65/Q80/Q81/Q87/Q89/Q91/Q93/Q96/Q100/Q103/Q106/Q110; Constitution §§16,19,24,28; UX §§11,34,38.
- Frozen System v1.1: §§7.4,16.4,31,34 ownership/composition and Windows startup; SHA-256 `7BAF7ABEBEBFFE77F20DF964E89E7845AA54E8DE810CF6FB8C6B500881275B6F`.
- Frozen Theme v1.2: §§3–5,11,21–25,29,31; especially §24.4 embedded trusted default, §24.4.1 startup, §25.2 path ownership, §25.3 unavailable Portable storage, §25.5–6 DPI/monitor recovery. SHA-256 `960CCCB69AFDBE006CA12BECB86A1E3E6C96C6C59500CE943F176162D2B33856`.
- ADR-0001; both existing ADR-0002 files (technology/scope and Theme feature ownership); ADR-0004; Decisions001–019; repository AGENTS/README/status; sealed Theme contracts and current project graph.

Source findings:
1. The default safe theme is embedded trusted platform presentation, never a removable Theme Package. An external sample mirror has no fallback authority. Safe shell startup does not await package validation.
2. Full startup authentication, Recovery, Application Services and Safety evaluation are later owners. The Phase6A minimal shell must not claim these services ran, authorize access to protected data, or open authenticated Home workflows.
3. Current `App.OnStartup` composes only singleton MainWindowViewModel/MainWindow. MainWindow receives its ViewModel by constructor and binds a Phase1 status string. This actual acquisition path must be extended; a disconnected bootstrap demo is insufficient.
4. `Tcc.Windows` and `Tcc.Features.Themes` currently contain only AssemblyMarker. No mode detector, path resolver, monitor implementation, configuration persistence, launcher/installer marker, or deployment receipt is available.
5. `Tcc.Themes` owns manifest/integrity/compatibility code, has one existing test friend, and references only Presentation.Contracts. DesktopHost alone composes concrete implementations. No new dependency is needed.
6. Reuse `ThemeVariantId`, `ThemeTokenBundle`, `ThemeTokenValue`, `ThemeSemanticBinding`, `ThemeFocusStyles`, `ThemeAccessibilityContract`. Their record DTOs do not alone guarantee deep collection immutability; snapshot construction must copy/freeze dictionaries and clone JsonElement values.
7. Reject reuse of `IThemePackage`, `IThemeRuntime`, `ThemeRuntimeState`, `ThemeRuntimeSnapshot`, compatibility environment/results or owner receipts for bootstrap. They convey package/lifecycle or validation semantics absent here. TradingPermissionPresentationState has no Unknown member and must never be defaulted to Tradable for missing data.
8. Frozen examples use machine IDs `deep`/`light`, display names Deep/Light. Those names are retained. This ADR chooses explicit bootstrap inputs, not an invented saved-preference subsystem.

## 2. Decision and scope

Establish only the immutable built-in presentation catalog/snapshot, embedded Deep/Light resources, narrow consumption, minimal DesktopHost orchestration, Windows startup-mode/path facts and per-window monitor/DPI facts, plus architecture guards.

External Theme admission remains fail-closed under sealed V2 owners. No bootstrap object is a trust token, evidence, admission candidate, compatibility success, activation permission or package identity. Internal snapshot construction protects ordinary callers from replacing the product catalog; it is not a public security credential. Assembly replacement, reflection/unsafe tampering and OS compromise are not newly solved by this contract.

Explicit exclusions: IThemeRuntime implementation; package discovery/filesystem acquisition/install/uninstall/activation/switch/preview; rollback/migration execution; package persistence/cache/registry; six production owner evidence issuers; resolver invocation; fake Compatibility/Safety/Q93/user/account/login/synchronization success; Trading engine, Market Data, Risk/Positions business services, Kraken/USD.PM, liquidity, AI Trading Intelligence, formal Features.Home and Gu Qinghan package. No release/installer construction, authentication bypass or Home business-policy ownership.

## 3. Exact type and member budget

This is the FINAL proposed type table for separately authorized TCC-P6-83. It locks every public/internal callable member, constructor, property, parameter name/type/nullability and handwritten type identity. No implicit public default constructors, init/set accessors, records, inherited custom base types, public fields, factories, overloads, events or extension methods unless listed. Object-inherited methods are not additions. Private helper methods/readonly backing fields may implement the listed algorithms within the same types; they may not introduce another type or callable boundary. Native layout fields are pinned below.

All new types use existing assemblies. **New public types: 7; new handwritten non-public types: 4 (2 top-level and 2 private nested); total 11. New interfaces: 0. New public types in Presentation.Contracts: 0.** Existing host type amendments are separately listed and do not count as new types.

### 3.1 Public contract types

| Exact type / namespace | Project / owner | Visibility / shape / construction | Exact properties and methods | Lifetime / consumers / why internal is insufficient |
|---|---|---|---|---|
| `Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot` | Tcc.Themes / built-in presentation | public sealed non-record class; sole internal ctor `(ThemeVariantId variantId, ThemeTokenBundle tokens, ThemeFocusStyles focusStyles, ThemeAccessibilityContract accessibilityRequirements)` | public get-only `ThemeVariantId VariantId`, `ThemeTokenBundle Tokens`, `ThemeFocusStyles FocusStyles`, `ThemeAccessibilityContract AccessibilityRequirements`; no declared public/internal methods | immutable process snapshot; DesktopHost ViewModel/resource adapter consume it across assembly boundary |
| `Tcc.Windows.Startup.WindowsStartupMode` | Tcc.Windows / Windows facts | public enum, int underlying; exact `Unknown=0, Installer=1, Portable=2`; no ctor | exactly these constants | value; DesktopHost uses explicit/unknown mode without depending on compatibility contracts |
| `Tcc.Windows.Startup.WindowsStartupFacts` | Tcc.Windows / Windows facts | public sealed non-record class; sole internal ctor `(WindowsStartupMode mode, string? deviceStateRoot, string? userPreferencesRoot, string? diagnosticCode)` | public get-only `WindowsStartupMode Mode`, `string? DeviceStateRoot`, `string? UserPreferencesRoot`, `string? DiagnosticCode`; no declared public/internal methods | immutable per process; DesktopHost orchestration only, never ViewModel/XAML/Theme; public because resolver output crosses assembly |
| `Tcc.Windows.Monitors.WindowMonitorFacts` | Tcc.Windows / Windows facts | public sealed non-record class; sole internal ctor `(string deviceName, int workAreaX, int workAreaY, int workAreaWidth, int workAreaHeight, uint dpi)` | public get-only `string DeviceName`, `int WorkAreaX`, `int WorkAreaY`, `int WorkAreaWidth`, `int WorkAreaHeight`, `uint Dpi`; public `(int X, int Y, int Width, int Height) FitBounds(int x, int y, int width, int height)` | immutable capture per window/event; MainWindow consumes it; native physical facts must cross assembly |

All reference parameters/properties without `?` are non-null; reject null with ArgumentNullException. Snapshot constructor rejects non-canonical variant, missing/extra/malformed tokens or contradictory focus/requirement values with ArgumentException; unsupported source variant uses ArgumentOutOfRangeException. It eagerly freezes dictionaries with ordinal keys, clones every JsonElement out of its document, copies nested token/binding records, and reconstructs focus/accessibility records. Mutation of input collections, disposed JsonDocument, downcasts to mutable collection interfaces, or `with` on returned records cannot alter snapshot-reachable state. No mutable ResourceDictionary, brush, path, HWND, principal, account, result, receipt or lifecycle state enters it.

WindowsStartupFacts permits Unknown only with both roots null and a diagnostic. Mode must be a defined enum; roots are either both non-null canonical local paths with no diagnostic or both null with a diagnostic and a known/unknown mode. It never states writability or installation verification. WindowMonitorFacts requires nonempty device name, positive dimensions and DPI; checked integer extent arithmetic rejects overflow. FitBounds rejects nonpositive requested dimensions; shrinks to work area, clamps both axes using widened checked arithmetic and preserves negative virtual-screen coordinates. It does not persist or move a window itself.

### 3.2 Public implementations required across existing assemblies

| Exact type / namespace | Project / owner | Exact shape / construction / methods | Lifetime / consumers / reason |
|---|---|---|---|
| `Tcc.Themes.Fallback.BuiltInThemePresentationSource` | Tcc.Themes | public sealed non-record class, no interfaces; sole public parameterless ctor; sole public method `BuiltInThemePresentationSnapshot GetPresentation(ThemeVariantId variantId)`; no public/internal properties | singleton immutable catalog of two snapshots; DesktopHost bootstrap only; concrete cross-assembly constructor injection avoids public factory/interface proliferation |
| `Tcc.Windows.Startup.WindowsStartupPathResolver` | Tcc.Windows | public sealed non-record class, no interfaces; sole public parameterless ctor; sole public method `WindowsStartupFacts Resolve(IReadOnlyList<string> startupArguments)`; no public/internal properties | stateless singleton; DesktopHost bootstrap only; owns cardinality/mode parsing, Known Folder and canonicalization calls |
| `Tcc.Windows.Monitors.WindowMonitorAdapter` | Tcc.Windows | public sealed non-record class, no interfaces; sole public parameterless ctor; public `WindowMonitorFacts? Capture(nint windowHandle)` and `bool EnsureVisible(nint windowHandle)`; no public/internal properties | one transient instance captured by each MainWindow; no global monitor state; MainWindow invokes capture and actual platform placement across assembly boundary |

No public bootstrap-source interface is added: no independent implementation consumer exists, and DI can register the sealed concrete source. This rejects the planning candidate `IBuiltInThemePresentationSource`. All other candidate names are retained with exact owner namespaces above/below. Source substitution by a caller is unsupported; UI receives only the snapshot, never a source selector or package path.

### 3.3 Internal orchestration and native layout types

| Exact type | Project / namespace | Shape / exact public or internal members | Lifetime / owner / consumers |
|---|---|---|---|
| `Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap` | Tcc.DesktopHost / Tcc.DesktopHost.ThemeBootstrap | internal static class; no ctor/properties; internal static `void Configure(HostApplicationBuilder builder, string[] args)` | one startup call; DesktopHost composition; App only |
| `Tcc.DesktopHost.EmbeddedSafeTheme.SafeThemeResourceAdapter` | Tcc.DesktopHost / Tcc.DesktopHost.EmbeddedSafeTheme | internal static class; no ctor/properties; internal static `ResourceDictionary Create(BuiltInThemePresentationSnapshot presentation)` | new detached dictionary per window/accessibility refresh; MainWindow only |
| `Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect` | Tcc.Windows | private nested sequential-layout struct; exactly internal int fields `Left, Top, Right, Bottom`; no declared ctor/properties/methods | native-call local buffer; WindowMonitorAdapter only |
| `Tcc.Windows.Monitors.WindowMonitorAdapter+NativeMonitorInfo` | Tcc.Windows | private nested sequential-layout Unicode struct; exactly internal uint `Size`, internal NativeRect `Monitor, Work`, internal uint `Flags`, internal string `DeviceName` with ByValTStr SizeConst=32; no declared ctor/properties/methods | native-call local buffer; WindowMonitorAdapter only |

Native imports are private static extern methods on WindowMonitorAdapter, limited to user32: `nint MonitorFromWindow(nint windowHandle, uint flags)`; Unicode EntryPoint GetMonitorInfoW `bool GetMonitorInfo(nint monitorHandle, ref NativeMonitorInfo monitorInfo)`; `uint GetDpiForWindow(nint windowHandle)`; `bool IsWindow(nint windowHandle)`; `bool GetWindowRect(nint windowHandle, out NativeRect rectangle)`; `bool SetWindowPos(nint windowHandle, nint insertAfter, int x, int y, int width, int height, uint flags)`. All bool returns use BOOL marshaling. No other native import, native wrapper type, source-generated import family or callback delegate type. DllImport is deliberate to keep generated/native surface bounded. NativeRect/NativeMonitorInfo are mutable interop buffers only; they are never returned or stored globally.

### 3.4 Existing host type amendments

- `Tcc.DesktopHost.App`: retain public WPF Application partial shape and existing OnStartup/OnExit signatures. OnStartup calls Configure exactly once before builder.Build/StartAsync and then resolves MainWindow once. No new public/internal member.
- `Tcc.DesktopHost.MainWindowViewModel`: retain public sealed class and public get-only `string Status`; replace implicit parameterless ctor with sole public `MainWindowViewModel(BuiltInThemePresentationSnapshot presentation, string startupNotice)`; add sole public get-only `BuiltInThemePresentationSnapshot Presentation`. Store constructor inputs after null validation; Status is the supplied plain bootstrap notice, never Safety or authentication truth.
- `Tcc.DesktopHost.MainWindow`: retain WPF Window partial shape; sole public user-authored ctor becomes `MainWindow(MainWindowViewModel viewModel, WindowMonitorAdapter monitorAdapter)`. No new public/internal member. Private instance handlers apply detached resources before Show, observe SourceInitialized/DpiChanged/Closed and display-change window messages; release HwndSource hook and SystemParameters.StaticPropertyChanged handler at close. These are framework delegates, not new handwritten delegate types. No service-provider lookup in view or ViewModel.
- WPF-generated InitializeComponent/IComponentConnector/fields and existing App async state machines require exact declaring-source/method provenance; compiler-generated naming is not a wildcard allowance. New Theme/Windows code is synchronous and must introduce no async/iterator/closure type. Prefer loops/static helpers; re-audit any unexpected generated type and stop if a new type allowance would be needed.

## 4. Resources, snapshot and Deep / Light semantics

Source eagerly reads exactly two neutral embedded .resx resources in its own assembly:
- `Tcc.Themes.Fallback.Deep.resources`
- `Tcc.Themes.Fallback.Light.resources`

Each has exactly one string entry `Tokens`, containing the unchanged ThemeTokens schema v1.0 JSON shape. ResourceManager uses invariant culture, exact base name and source assembly; no satellite/localization fallback, disk probing, network, caller path, manifest/package reader or runtime download. Manifest-resource identity tests require the four sealed JSON schema resources plus these exact two resource names. The constructor validates both catalog entries; broken Light must not hide behind a working Deep.

Exact startup token set (each JSON type and value is locked here; all colors opaque):
| Token | Type | Deep | Light |
|---|---|---|---|
| color.background.base | color | #111827 | #FFFFFF |
| color.background.surface | color | #1F2937 | #F3F4F6 |
| color.text.primary | color | #F9FAFB | #111827 |
| color.text.secondary | color | #D1D5DB | #374151 |
| color.state.tradable | color | #86EFAC | #166534 |
| color.state.warning | color | #FDE68A | #854D0E |
| color.state.blocked | color | #FCA5A5 | #991B1B |
| color.state.unknown | color | #D1D5DB | #374151 |
| focus.ring.color | color | #FDE047 | #1D4ED8 |
| focus.ring.thickness | dimension | 2 | 2 |
| font.family.ui | font | Segoe UI | Segoe UI |
| font.size.body | dimension | 16 | 16 |
| motion.duration.short | duration | 0ms | 0ms |
| surface.opacity | opacity | 1 | 1 |

SchemaVersion is "1.0". Exact semantic bindings: `trading_permission.tradable → color.state.tradable`, `trading_permission.warning → color.state.warning`, `trading_permission.blocked → color.state.blocked`, `presentation.unknown → color.state.unknown`; every binding RequiresTextLabel=true and RequiresIconOrStructure=true. These are palette semantics, never actual permission/risk values.

FocusStyles: SchemaVersion="1.0", MinimumContrastRatio=3m, VisibleOnAllInteractiveControls/ProgrammaticOrderPreserved/ModalFocusTrapPreserved/FocusRestorationRequired=true. AccessibilityRequirements uses the existing eleven ThemeAccessibilityContract bools, all true as requirements, not evidence or an evaluated status. No ThemeAccessibilityValidationResult is created.

Startup selection is explicit `--startup-variant=deep` or `--startup-variant=light`, case-sensitive ordinal ASCII. Absence selects product-owned Deep as the documented built-in default. Duplicate, empty, whitespace, different case or unknown values select Deep with diagnostic notice `BOOTSTRAP_VARIANT_INVALID`; they never silently succeed as the requested variant. Direct GetPresentation accepts only exact deep/light IDs; default/unknown/null Value throws ArgumentOutOfRangeException. Source performs no fallback itself.

This is **Startup Variant Selection**, not **Theme Variant Switching**. Freeze one selected snapshot per process. OS high-contrast refresh changes derived WPF resources only; it does not replace VariantId or invoke any theme switch transition. No saved preference, registry setting, per-theme/user/account state, transition audit, migration, rollback or live switch is created. Future saved selection must be supplied by an approved settings owner through a separately reviewed contract.

Resource adapter returns a new non-authoritative WPF ResourceDictionary. Exact keys are the fourteen token keys above; colors become frozen SolidColorBrush, font family becomes FontFamily, duration becomes TimeSpan.Zero and dimensions/opacity become double. Under SystemParameters.HighContrast, use SystemColors WindowColor/WindowTextColor for base/surface/text/state and HighlightColor for focus; keep semantic text/structure requirements. Re-evaluate on system accessibility changes; never mutate catalog/snapshot. No external XAML, pack URI from a caller, ObjectDataProvider, executable markup, animation/audio or transparency effect. App.xaml may define only the static style keys `Tcc.BuiltIn.Window`, `Tcc.BuiltIn.Text`, `Tcc.BuiltIn.Focus`, using DynamicResource token keys. MainWindow.xaml changes only resource/style consumption in the existing minimal shell. No new navigation or business regions in P6-83.

## 5. Windows mode and path boundary

A packaging detector/marker is not currently implemented. Do not infer mode from directory names, registry, working directory, elevated state or existence of a writable folder.

Phase6A accepts a documented explicit launch input `--startup-mode=installer|portable`. This selects process startup path policy; it is **not proof that an installer ran or that a package is authentic**. Host passes an eagerly copied list of the raw Windows bootstrap arguments to WindowsStartupPathResolver, preserving duplicates. Resolve rejects a null list/null item with ArgumentNullException and parses exact `--startup-mode=`/`--portable-data-root=` keys itself; unrecognized/malformed entries in this dedicated list yield BOOTSTRAP_ARGUMENTS_INVALID with Unknown/null roots. With absent/invalid/duplicate mode, return Unknown, roots null, diagnostic `BOOTSTRAP_MODE_UNKNOWN`; run built-in presentation without storage. No Installer default. Both explicit modes execute the same presentation path. Future packaging must supply/verify a deployment fact before persistent/core-store operations are authorized; Phase6A does none.

Optional `--portable-data-root=<absolute-local-directory>` is accepted only once, only for explicit portable mode. It is an explicit user-selected data root, never a Theme Package path. Empty/relative/UNC/device/extended-device/network-drive path, URI, invalid path, file rather than directory, inaccessible selection or a reparse-point component produces roots null and `BOOTSTRAP_PATH_UNAVAILABLE`. An option used outside portable mode or duplicated produces Unknown/null roots with `BOOTSTRAP_ARGUMENTS_INVALID`. Non-bootstrap arguments remain with existing Generic Host; exact prefix parsing must not reinterpret arbitrary keys as privileged facts. The three bootstrap keys are removed before Generic Host command-line parsing; no environment/config source can override them.

Dedicated resolver is required because Tcc.Windows has no dependency on Theme/UI and owns Windows Known Folder behavior. Resolve uses:
- Installer: Environment.GetFolderPath(LocalApplicationData) and GetFolderPath(ApplicationData), equivalent trusted platform abstraction; Path.Combine + Path.GetFullPath produce `TCC/Themes/DeviceState` and `TCC/Themes/UserPreferences`. Never concatenate environment-variable text. Missing/empty Known Folder results do not fall back to cwd/temp.
- Portable default: canonical AppContext.BaseDirectory + `data`; selected root: canonical supplied absolute local data directory. Both derive `themes/user-state` for device/preferences roots, matching Frozen portable layout. Their physical location is shared; no persistence or marker content is created.
- Canonicalization is Windows case-insensitive, separator-aware containment, rooted on the selected product root. Existing ancestors are checked for reparse/network redirection and accessibility only as facts needed to resolve a local path. No directory enumeration, package probing, network discovery, write probe, create/delete, registry access or trust decision.
- If a root is read-only, missing, unavailable or otherwise cannot be established, built-in startup remains available. Successfully deriving a path is **not a writability guarantee**. No paths authorize later writes or bypass future handle/containment/lock checks. Selected existing read-only roots may be reported as locations; write capability remains unknown. A missing default data subfolder may be lexically derived without creation; this also makes no existence/writability claim.
- Diagnostic codes are finite, machine-readable and path-free: BOOTSTRAP_MODE_UNKNOWN, BOOTSTRAP_PATH_UNAVAILABLE, BOOTSTRAP_ARGUMENTS_INVALID. Catch only expected path/platform/access exceptions; programming defects propagate to controlled startup failure. Never catch everything and manufacture facts.

DesktopHost receives WindowsStartupFacts; stores no paths in ViewModel/status/log text/resources/Theme. UI receives a plain unavailable/unknown notice only. Theme layer receives no Windows facts because embedded tokens need none. No source graph change, package directories, stable portable instance marker, collision handling, settings store or core-data root authority is implemented here. Frozen marker/single-writer rules remain required before any future persistent Portable operation.

## 6. Monitor / DPI boundary

Tcc.Windows owns Capture and FitBounds. MainWindow owns per-window lifecycle and WPF/native coordinate conversion. Capture requires a real nonzero valid HWND; invalid/destroyed handle returns null. It uses MonitorFromWindow nearest monitor, then primary fallback if necessary, GetMonitorInfoW for device/work area, and GetDpiForWindow for actual DPI. Failed OS calls, zero DPI or invalid extents return null; never invent 96 DPI or a primary monitor identity.

Per-monitor awareness is an explicit future implementation prerequisite, not inferred from WPF or GetDpiForWindow. P6-83 adds `src/Tcc.DesktopHost/app.manifest` with asm.v1 manifestVersion=1.0, assemblyIdentity name=Tcc.DesktopHost version=1.0.0.0, asInvoker/uiAccess=false, and asm.v3 application/windowsSettings containing `dpiAware` (2005 namespace) value `true/pm` plus `dpiAwareness` (2016 namespace) value `PerMonitorV2,PerMonitor`. P6-83 may add only `<ApplicationManifest>app.manifest</ApplicationManifest>` to the existing DesktopHost PropertyGroup; no other project metadata/reference/package change. The resulting native apphost manifest and actual process/window awareness must be inspected, not merely source XML. Launch the built apphost for this gate, not `dotnet Tcc.DesktopHost.dll`. This follows [Microsoft's WPF per-monitor guidance](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/ui/using-the-visual-layer-with-wpf) and [application-manifest DPI declarations](https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests). These technical references do not override Frozen product authority. No process-wide DPI API call is added after WPF initializes.

WindowMonitorFacts work area/bounds are physical screen pixels; Dpi/96d is the derived scale. No stored monitor selection, global current-monitor singleton or floating-workspace implementation. On SourceInitialized, WM_DISPLAYCHANGE and after WPF DpiChanged handling, call EnsureVisible and acquire fresh per-window facts. EnsureVisible validates HWND, calls Capture and GetWindowRect, applies FitBounds to the actual outer pixel rectangle, and calls SetWindowPos only if a change is needed, with insertAfter=0 and SWP_NOZORDER|SWP_NOACTIVATE (0x0014). It returns true only for an already-contained or successfully constrained actual window, false for unknown/native failure. Re-entrant placement is avoided by the host's private per-window handler flag. There is no WPF DIP-to-global-pixel multiplication: native rectangles stay physical and WPF owns its normal DPI adjustment. Monitor removal recaptures current/primary work area; no off-screen remembered coordinate has authority.

If facts are unavailable or EnsureVisible fails, retain an opaque scalable shell with Windows-managed placement, expose monitor/DPI unavailable status, and retry only on the next real window/display/DPI event. The exact transient notice is the MainWindow title suffix ` — 顯示器／DPI 資訊暫不可用`; remove it only after a successful fresh capture/placement. This uses the existing Window.Title property and does not mutate the immutable ViewModel. Do not fabricate a validated placement or a numeric DPI. Failure must not crash a usable built-in shell; a manual DPI gate remains required to claim monitor-safe behavior. Release event hooks on close.

## 7. DesktopHost composition, lifetimes and startup flow

Exact conceptual registrations, all only in ThemeBootstrap.Configure:
1. WindowsStartupPathResolver singleton concrete.
2. WindowsStartupFacts immutable instance, resolved once by that concrete owner.
3. BuiltInThemePresentationSource singleton concrete, eagerly validated catalog.
4. BuiltInThemePresentationSnapshot immutable selected instance from that exact source.
5. WindowMonitorAdapter transient concrete, constructor-captured by MainWindow for its window lifetime.
6. MainWindowViewModel singleton via explicit factory passing selected snapshot and finite plain startup notice.
7. MainWindow singleton via constructor injection.

Configure creates the path resolver/source and their results explicitly before registration of those same instances; it does not BuildServiceProvider or resolve a temporary service container. Existing App builds/starts Generic Host once, then resolves MainWindow at the composition root only. Static resource adapter has no DI registration. No Theme service or ViewModel sees IServiceProvider. The source/Windows resolver have no operation-local receipt, HWND, stream, JsonDocument or mutable request retained.

Startup:
1. App separates only the three bootstrap arguments; creates HostApplicationBuilder with remaining arguments.
2. Configure validates argument cardinality, obtains mode/path facts, selects canonical startup variant and acquires the immutable source snapshot.
3. Register the exact seven services above; build/start host once.
4. MainWindow is acquired through the actual App → DI → constructor path; ViewModel already holds the selected snapshot.
5. MainWindow derives detached resources, initializes minimal XAML, sets DataContext, subscribes per-window events, then App shows it.
6. Display finite notices without paths: built-in presentation active; variant fallback if requested input invalid; startup mode/path/monitor facts unavailable as applicable. No login/account/Safety/compatibility success.
7. No package is discovered/read/validated, no resolver or owner evidence is invoked. Background external theme work remains absent.

Not registered: IThemeRuntime, IThemePackage, Compatibility Resolver V1/V2, Integrity Verifier V1/V2, six owner evidence producers, package content reader/binder/materializer, installation/activation/preview/rollback/migration/discovery/cache/registry services. Their sealed existing implementations remain in the assembly but unreferenced from the new bootstrap call graph.

Failure behavior: malformed selection falls back only as specified with a visible notice. Unavailable Windows path/mode/monitor facts preserve honest unknown and embedded presentation. Missing/corrupt embedded resources are a product defect: fail startup with a plain accessible OS message and nonzero exit; do not load disk content or issue a success receipt. Existing core Login/App Lock, Safe Mode, Recovery and Home services are not fabricated to mask missing owners.

## 8. Accessibility, Safety and first UI boundary

Phase6A supplies opaque scalable tokens, visible focus styles, immutable semantic requirements, live OS high-contrast overlay and per-window geometry support. All eleven accessibility flags describe obligations. They are not results. No package Accessibility receipt is required to start trusted built-in presentation; no package accessibility success is claimed.

Future native WPF Command Center slice must include left navigation, top application/status area, Home Safety Core, mental-state area, BTC data boundary, KPI/overview, both startup variants, resize/DPI/monitor-safe layout and keyboard/focus/screen-reader baseline. A Home-shaped shell in DesktopHost is presentation composition only; no Features.Home assembly/business service is created. Formal Home logic remains a later feature owner.

Safety Core must keep Trading Permission, Total Risk, Current Positions and Major Alerts visible, in plain text with programmatic labels and non-color encoding. Unknown remains unknown. Allowed: BTC unavailable, KPI unknown, positions unknown, risk unknown, disabled future navigation, production-isolated labelled presentation fixtures. Forbidden: invented BTC prices, trading permission, zero risk/positions, no-alert, login/account identity, sync success, compatibility/Safety/Q93 approval or owner receipts. Mental-state values are not inferred. No enum default is a substitute for unavailable facts.

Future UI acceptance must execute text/zoom at 100/125/150/175/200% and higher OS-supported maximum, DPI 100/125/150/175/200% and higher OS-supported maximum, combined profiles, active high contrast, reduced motion/transparency, keyboard, focus, UI Automation/Narrator and non-color state interpretation. Normal text ≥4.5:1; large text/non-text/focus ≥3:1. Critical information reflows/scrolls and cannot clip or become mouse-only. Phase6A validates only its actual minimal shell and underlying primitives; it does not claim the not-yet-created full Command Center or deferred UI/release matrix passes.

## 9. Exact TCC-P6-83 file allowlist

Maximum candidate paths **22**, counting the three governance files. NEW/MODIFY below is relative to sealed HEAD; Decision020 and ADR-0005 were carried unchanged through P6-83 implementation and were later clarified only under the authorized TCC-P6-83B governance refinement. The sole csproj edit is the manifest property in row22; no solution, lockfile, schema, friend or dependency change. A twenty-third path or additional type/member requires STOP and return to Supervisor.

| # | Action | Exact repository path | Purpose |
|---|---|---|---|
| 1 | NEW | src/Tcc.Themes/Fallback/BuiltInThemePresentationSource.cs | sealed catalog source |
| 2 | NEW | src/Tcc.Themes/Fallback/BuiltInThemePresentationSnapshot.cs | deep immutable presentation snapshot |
| 3 | NEW | src/Tcc.Themes/Fallback/Deep.resx | embedded Deep Tokens entry |
| 4 | NEW | src/Tcc.Themes/Fallback/Light.resx | embedded Light Tokens entry |
| 5 | NEW | src/Tcc.Windows/Startup/WindowsStartupPathResolver.cs | resolver plus WindowsStartupMode/WindowsStartupFacts |
| 6 | NEW | src/Tcc.Windows/Monitors/WindowMonitorAdapter.cs | per-window adapter, facts and two exact native nested structs |
| 7 | NEW | src/Tcc.DesktopHost/ThemeBootstrap/ThemeBootstrap.cs | sole bootstrap DI and selection orchestration |
| 8 | NEW | src/Tcc.DesktopHost/EmbeddedSafeTheme/SafeThemeResourceAdapter.cs | detached WPF resources/accessibility overlay |
| 9 | MODIFY | src/Tcc.DesktopHost/App.xaml.cs | actual startup and controlled failure path |
| 10 | MODIFY | src/Tcc.DesktopHost/App.xaml | exactly three built-in styles |
| 11 | MODIFY | src/Tcc.DesktopHost/MainWindow.xaml.cs | snapshot acquisition/resource/monitor event path |
| 12 | MODIFY | src/Tcc.DesktopHost/MainWindow.xaml | style consumption only in existing minimal shell |
| 13 | MODIFY | src/Tcc.DesktopHost/MainWindowViewModel.cs | immutable constructor-injected presentation/notice |
| 14 | NEW | tests/Tcc.Architecture.Tests/PhaseSixBuiltInThemeBootstrapTests.cs | contracts/resources/immutability/selection/startup integration |
| 15 | NEW | tests/Tcc.Architecture.Tests/PhaseSixWindowsStartupBoundaryTests.cs | mode/path/geometry and actual HWND positive controls |
| 16 | NEW | tests/Tcc.Architecture.Tests/PhaseSixBootstrapArchitectureTests.cs | exact multi-assembly surface/IL/DI/native boundaries and mutants |
| 17 | MODIFY | tests/Tcc.Architecture.Tests/PhaseThreeScopeBoundaryTests.cs | exact additive Theme two-type allowance and existing fixture evolution only |
| 18 | MODIFY (carry) | docs/CODEX_DECISIONS.md | Decision020 candidate from P6-82 plus TCC-P6-83B acceptance-scope clarification; Decision001–019 bytes unchanged |
| 19 | NEW (carry) | docs/adr/ADR-0005-trusted-built-in-theme-bootstrap-and-presentation-boundary.md | P6-82 boundary/type/file authority plus TCC-P6-83B foundation-vs-UI validation clarification |
| 20 | MODIFY | docs/CODEX_PROJECT_STATUS.md | truthful implementation/validation snapshot |
| 21 | NEW | src/Tcc.DesktopHost/app.manifest | explicit per-monitor DPI awareness, asInvoker only |
| 22 | MODIFY | src/Tcc.DesktopHost/Tcc.DesktopHost.csproj | only ApplicationManifest property pointing at row21 |

The FINAL allowlist is exactly rows 1–22. A twenty-third path is forbidden. No implementation is authorized by merely listing these paths. The three P6-82 governance paths are included in this twenty-two-path future candidate, not additional to it. P6-82 itself still prohibits any csproj/manifest implementation edit and has exactly three governance paths.

## 10. Architecture guard evolution

Guard authority must be explicit identities and signatures, never count-only or namespace-prefix acceptance.
- Extend existing PhaseThreeScopeBoundaryTests only for exact `Tcc.Themes.Fallback.BuiltInThemePresentationSource` and `BuiltInThemePresentationSnapshot`, with their exact visibility, non-record sealed shape and members. Keep every sealed type/interface/provenance/Phase5 dependency restriction. Update its positive fixture builder with these two exact shapes so existing negative tests continue attacking the intended boundary; no weakening or deleting old assertions.
- New PhaseSixBootstrapArchitectureTests checks exact seven public / four non-public additions across actual Theme/Windows/DesktopHost assemblies, exact existing-host signature changes, no extra public/internal constructor/member or mutable snapshot state. Owner assembly is part of identity. Exact native imports/layout fields and generated source provenance are checked.
- Inspect actual new/changed production method bodies, constructor calls, generic instantiations, locals, fields, properties, signatures and inherited interface closure. No IThemeRuntime or IThemePackage implementation, Compatibility V1 implementation, fake evidence issuer, new package admission service, trust factory, service-locator bypass or Trading/AI/business Safety/Home logic. Existing sealed V2 resolver/verifier remain exactly one each; no new bootstrap/UI dependency on them, their binder/reader/evidence or V1 services.
- Reachability matters: inspect App→Configure→MainWindow→ViewModel→adapter/source paths, not just source strings or class names. UI/ViewModel may consume only the snapshot; MainWindow alone may capture per-window Windows monitor facts. WindowsStartupFacts/resolver/paths stay in composition; forbid System.IO/registry/package path ownership in ViewModel/XAML/Theme source, and arbitrary XAML/external resource loaders in adapter.
- Verify DI descriptors through actual Configure execution: exact service identities/lifetimes/instances, no duplicate registration, no hidden service provider, no mutable/operation-local state captured by singleton. Frozen dictionaries/brushes are not replacements for field/member checks.
- Dependency gate parses every repository csproj ProjectReference using Windows canonical case-insensitive paths, retains exact six-project allowlist and all eleven edges, four exact package refs and one existing friend. No new projects/references/packages/friends; no namespace wildcard. No generic exemption for abstract, generated, approved-name, nested, record, interface, delegate or native types.
- Negative mutants must independently introduce: extra public method/type; public snapshot ctor; init setter; mutable dictionary; package-path argument; IThemeRuntime/IThemePackage/Compatibility V1 implementation (including abstract/inherited forms); owner-receipt construction; resolver call in VM; IServiceProvider field; extra DI registration; wrong singleton monitor lifetime; static monitor field; external ResourceDictionary URI; wrong native import; new reference/package/friend; Theme/Home business or Trading/AI dependency. Every mutant must be shown to fail the real guard with an approved positive control that passes.
- Tests may load the actual built DesktopHost assembly in an isolated child process/collectible context with framework dependency resolution and use reflection to invoke existing/internal host entry points. No new ProjectReference/friend or public test hook is permitted. Read generated runtimeconfig/deps from the real host output; use WindowsDesktop framework when executing WPF. Reflection-only shape checks do not count as runtime smoke.

## 11. TCC-P6-83 acceptance and execution plan

Phases are sequential; only one IN PROGRESS. Stop before crossing a failed gate.
| Stage | Goal / allowed files | Dependencies / risk | Acceptance / rollback |
|---|---|---|---|
| A | types/catalog/platform facts, rows 1–6 and focused tests 14–16 | ADR/Decision candidate confirmed; shallow immutability, fake mode and native layout risks | contract, resource, immutability, both variants and Windows facts tests; revert only current-stage task changes while preserving P6-82 governance |
| B | actual host composition/resource consumption, rows 7–13,21–22 | A PASS; ordering, resource mutation, DI/lifetime, DPI event risks | real App/MainWindow acquisition, apphost awareness, no-package smoke, both variants and unavailable facts; revert only B changes |
| C | exact architecture guards/full qualification/status, rows 14–17,20 | B PASS; weakened guard or false approval risk | mutants, locked restore/Release x64/full tests/Frozen/scope/Git; fix only authorized candidate or report blocker, no commit/next phase on failure |

Exact tests:
1. Constructor/property/method/nullability/enum/member identity and immutable object graph; dictionary mutation through original input and casts; disposed JsonDocument; altered returned record copies; null/default/undefined inputs.
2. Deep and Light source results; exact resource names, no satellite/filesystem dependency, exact fourteen typed tokens/four bindings/focus/all eleven requirements, finite numeric values; normal/non-text/focus contrast against both base and surface, and actual high-contrast derived WPF resources. Missing resource/key, extra key, wrong type, invalid JSON and incomplete second variant fail product validation.
3. Actual Configure invocation: explicit deep/light; absent variant→Deep; unknown/empty/case/duplicate variant→Deep with notice; direct invalid source call throws. Snapshot resolves identically by reference for source-selected registration and VM. Repeated source access stable; no runtime switching/persistence.
4. Mode inputs installer/portable/absent/invalid/duplicate; both modes share UI behavior, missing mode never Installer. Known Folder redirection/non-ASCII/long-path cases, relative/UNC/device/network/reparse/root escape inputs, selected root moved/read-only/unavailable, invalid option combination and no file writes. No fabricated installer/portable-instance identity. Geometry valid/negative coordinates/overflow/nonpositive/extreme sizes; valid actual HWND capture; destroyed/zero handle→unknown.
5. Actual desktop child-process smoke with no external package directory: WPF MainWindow displayed through App/DI path, actual ViewModel Presentation acquired, resource colors reflect each launch variant, controls/data context alive, events detach and process exits cleanly. Do not substitute a fixture window or unused helper. UI Automation or in-process inspection of the real window may observe it; no public test-only runtime switch, fake source or smoke-success API.
6. Runtime absence checks plus IL/registration proof: no external package reads, resolver/verifier calls, six receipts, IThemeRuntime, persistence writes or background package task. Force unavailable mode/path and still show safe minimal shell. Force corrupted embedded resource in an isolated mutant and observe accessible startup failure/nonzero exit.
7. Phase6A foundation hardware/accessibility validation: exercise at least two actual display/DPI contexts when available; real HWND and GetDpiForWindow; real cross-monitor refresh when multiple monitors exist; real resize and off-screen recovery; real WPF UI Automation discovery; accessible names/focus for the minimal shell; a non-default real Windows text scaling profile when available; automated High Contrast resource projection; and no critical-state dependence on animation/transparency. Evidence must be real and unavailable mechanisms must not be fabricated.
8. All architecture mutants and exact graph check above; sealed Phase5A/5B production and functional tests unchanged apart from row17's exact guard additions; old mutants must remain effective.

Required command gates (single node on this host): normal restore with NuGet audit; locked restore with audit; Release x64 build; focused tests; full repository tests with zero failures/skips. Commands retain README's solution/configuration/platform with `-m:1`, restore `--force --no-cache -p:RestoreDisableParallel=true`, locked restore additionally `--locked-mode`, build `--no-restore`, test `--no-build`. Audit must not be disabled or warnings suppressed. Report actual counts and warning/error totals.

Preservation: sealed HEAD/tag identities and ancestry; byte-preserved Decision019/ADR-0004/Candidate A contracts/Phase5A/Phase5B code and functional tests; Frozen hashes above; exact twenty-two-path budget; staged=0 and no unapproved artifact. The DesktopHost csproj differs only by ApplicationManifest; all graph and package metadata remain unchanged. Report inherited ignored build output separately from new artifacts; do not delete unrelated files to make a cleanliness claim.

### 11.1 TCC-P6-83B foundation gate clarification

This clarification is not a quality waiver. It separates the implemented Phase6A bootstrap foundation from the final real-UI accessibility/display acceptance that can only meaningfully validate the future Command Center layout.

Historical truth remains: TCC-P6-83 BLOCKED and TCC-P6-83A BLOCKED. TCC-P6-83A's successful technical evidence is accepted for foundation correctness without rewriting either historical verdict: compiled mutants 21/21 rejected, bypasses 0, positive control ACCEPT; corrupted embedded-resource real apphost exit code 1 with explicit accessible failure and no fallback/fake shell; two actual monitors; real 100%/125% DPI, cross-monitor refresh, resize and off-screen recovery; real WPF UI Automation/name/focus evidence; current 152% Windows text scaling; focused 49/49, full 1582/1582 and Release zero warnings/errors.

The Phase6A foundation gate is therefore satisfied. The following remains **DEFERRED / NOT YET EXECUTED**, not PASS: physical 150%/175%/200% DPI; target Windows text-size 100%/125%/150%/175%/200%; active High Contrast physical validation; and interactive Narrator reading/order validation. These checks are assigned to TCC-P6-85 first real UI vertical-slice validation and/or its independent validation task, and remain required for later release validation.

The deferred checks primarily test final presentation layout, readability, information survival and assistive-technology behavior. Running the complete matrix only against the minimal bootstrap shell would not validate the future UI and would need repetition. This allocation does not weaken Q93 or remove any future gate.

TCC-P6-83B result: PASS. Combined Phase6A Implementation Gate: PASS. Phase6A state: IMPLEMENTED / UNVALIDATED / UNSEALED. Ready for TCC-P6-84: YES. Ready for UI: NO. Deferred physical UI matrix: OPEN / REQUIRED FOR UI VALIDATION.

## 12. TCC-P6-84 independent validation authority

TCC-P6-84 is **READ-ONLY**, no fix, production/test/governance mutation, stage/commit/tag/push or automatic implementation continuation. Attack copies/ephemeral processes live outside repository and must not change the candidate; return findings to Supervisor.

| Attack | Expected defense / independent observation |
|---|---|
| Fake built-in source / public trust leakage | exact sealed concrete registration; no public snapshot issuance/constructor/factory/token or evidence semantics |
| Mutable snapshot / JSON document disposal / ResourceDictionary mutation | mutate inputs and output copies; original snapshot unchanged; detached mutable dictionary cannot authorize new state |
| Package-path injection / external resource replacement | no accepted package path/load URI; exact embedded assembly resource identities |
| Fake mode / Installer–Portable mismatch | missing/invalid remains Unknown; launch input is only startup policy, never installation proof; parity for same presentation |
| Global monitor leakage / wrong lifetime capture | multiple real windows on distinct monitors; independent captures, no static HWND/monitor/operation receipts |
| IThemeRuntime / V1 accidental implementation | full interface closure and abstract/generated mutants rejected |
| Resolver/evidence coupling | actual App call path/IL/runtime absence; zero new receipt construction or package calls |
| DI service locator / bypass | actual descriptor set and constructors, exactly one provider, no ViewModel lookup |
| Deep/Light incomplete resources | force each variant, mutate missing second resource/token; failure must be detected |
| No-package / invalid variant startup | real positive no-package launch for both variants; invalid selection visible fallback, no false success |
| DPI/monitor edge cases | actual HWND, disconnect/reconfigure/scale; null facts honest, bounds visible when actual data available |

Positive control: untouched P6-83 candidate completes the actual minimal-shell startup for both canonical variants and both explicit modes with no package and passes all tests. Mutants must fail specific guards, not unrelated compilation errors. Independent authority separately assesses evidence quality and all unexecuted hardware/accessibility gates. Phase6B PASS still does not authorize full external Theme runtime; first UI requires subsequent explicit scope.

## 13. Rejected alternatives and future migration

Rejected: full Theme runtime first; stub-heavy UI first; fake Compatibility/Safety/Q93 receipts; IThemePackage for embedded platform fallback; public source interface/factory/trust token with no consumer need; path/monitor state in Theme singletons; ViewModel service locator; live variant switching or saved per-theme bootstrap state; graph expansion; deriving Installer from absent facts; mandatory external package validation before shell.

Future: preserve embedded fallback permanently. Add real packaging mode provenance, settings persistence/Portable marker/locks, authenticated core services and formal Features.Home only under their owners and later contracts. Package admission later uses sealed V2 content binding and real six-owner evidence; live transition, migration/rollback and package UI remain separate. Do not convert startup snapshot into a receipt or enlarge these two Theme types silently. If a future design needs another interface/type/dependency, submit a separate additive governance amendment.

## 14. P6-82 governance gates and handoff

Current mutation budget is exactly Decision ledger append, this new ADR, status snapshot update. No source/test/contract/csproj/solution/lock/Frozen mutation. New branch starts at sealed `05a116d41c8a96b7c8d591ddbe212d1e18cf6fa9`; tag object `2ceff7fb024cbf03887f08425823af78b66e4f76`; content baseline `26dfd8a2178663b13a18660f3c40180a1e370128`. Decision019 and all earlier ledger bytes are preserved.

P6-82 phases: source/baseline audit (read-only, branch creation after exact identity); governance definition (these three paths only, risks unresolved boundary/member scope); preservation/build/tests/status closure (same paths, no production). Each stage depends on previous PASS. Rollback is limited to the task's own uncommitted three-file edits after explicit user instruction; no branch deletion or sealed branch edits.

Historically, the TCC-P6-82 Boundary PASS meant a complete reviewable contract candidate ready for Supervisor-issued TCC-P6-83; it did not approve implementation already performed. TCC-P6-83B now records the implemented foundation as IMPLEMENTED / UNVALIDATED / UNSEALED and ready for Supervisor-issued TCC-P6-84 independent validation. External Theme runtime and UI implementation readiness remain NO.
