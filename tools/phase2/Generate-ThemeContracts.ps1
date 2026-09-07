[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$contractRoot = Join-Path $repositoryRoot 'contracts\theme'
$schemaRoot = Join-Path $contractRoot 'schemas'

@(
    $contractRoot,
    $schemaRoot,
    (Join-Path $contractRoot 'page-contracts'),
    (Join-Path $contractRoot 'module-contracts'),
    (Join-Path $contractRoot 'zone-contracts'),
    (Join-Path $contractRoot 'state-contracts')
) | ForEach-Object { New-Item -ItemType Directory -Force -Path $_ | Out-Null }

function Write-JsonFile {
    param(
        [Parameter(Mandatory)] [string] $Path,
        [Parameter(Mandatory)] [object] $Value
    )

    $json = $Value | ConvertTo-Json -Depth 100
    [System.IO.File]::WriteAllText($Path, "$json`n", [System.Text.UTF8Encoding]::new($false))
}

function Convert-RegistryRows {
    param(
        [Parameter(Mandatory)] [string] $Rows,
        [Parameter(Mandatory)] [string] $SourceReference
    )

    return @($Rows.Trim().Split("`n", [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
        $parts = $_.Trim().Split('|')
        [ordered]@{
            id = $parts[0]
            name = $parts[1]
            semantics = $parts[2]
            source_reference = $SourceReference
        }
    })
}

$domains = Convert-RegistryRows -SourceReference 'TCC Information & UX Architecture v1.1 — APPROVED §3.1' -Rows @'
D-HOME|Command Center Home|Integrated safety, status, daily priorities, positions, and alerts
D-PLAN|Planning|Trade plans, strategy templates, checklists, quality score, and hard gates
D-RISK|Risk & Permission|Trading permission, Risk Profiles, account/group rules, and risk simulation
D-POSITION|Positions|Active position management, focus mode, updates, and risk-sensitive edits
D-CLOSE|Closing|Configurable closing flow, quick close, and delayed review
D-REVIEW|Review & Learning|Decision Timeline, trade history, reports, mental state, and AI coach
D-CALENDAR|P&L Calendar & Analytics|Monthly calendar, quarterly/yearly heatmaps, and analytics builder
D-TEAM|Team Command Center|Shared plans, team workspaces, approvals, activity, and rooms
D-COLLAB|Collaboration Rooms|Dedicated collaboration spaces with shared artifacts and permissions
D-AI|GPT / AI Workflows|Manual GPT export/import, privacy preview, optional scoring, and AI coach
D-DATA|Data, Recovery & Migration|Recovery Center, backups, migration, and import/export
D-SETTINGS|Settings & Control|Settings, security, devices, accessibility, and shortcut profiles
D-THEME|Theme Management|Theme library, switching, personalization, and rollback
D-EXT|Extensions & Connectors|Plugins/extensions and future read-only connector management
D-UTILITY|Global Utilities|Search, Command Palette, notifications, and floating workspace
'@

$workspaces = Convert-RegistryRows -SourceReference 'TCC Information & UX Architecture v1.1 — APPROVED §7.1' -Rows @'
WS-PERSONAL|Personal Command Center|Personal trading operations
WS-TEAM|Team Command Center|Team-level trading collaboration
WS-DAILY|Daily Workspace|Default day overview
WS-PREMARKET|Pre-Market Workspace|Start-of-day preparation
WS-TRADING|Trading / Position Workspace|Active trade monitoring and management
WS-REVIEW|Review Workspace|Post-trade review and improvement
WS-COLLAB|Team Workspace|Shared plans, approvals, and rooms
WS-FLOAT|Floating Reminder Workspace|Always-on-top compact workspace
WS-CUSTOM-*|Custom Saved Workspace|User or team configured layout pattern
'@

$modules = Convert-RegistryRows -SourceReference 'TCC Information & UX Architecture v1.1 — APPROVED §8.2' -Rows @'
MOD-HOME-COMMAND|Integrated Command Center Home|Unified safety, status, and work overview
MOD-RISK-PERMISSION|Trading Permission|Tradable, Warning, and Blocked state with evidence and resolution paths
MOD-PLAN-WORKBENCH|Trade Plan Workbench|Build and formalize trade plans
MOD-PLAN-CHECKLIST-GATES|Checklist / Quality Score / Hard Gates|Validate plan quality and enforce hard gates
MOD-PLAN-STRATEGY-TEMPLATES|Strategy Templates|Versioned plan structures, conditions, scoring, and gates
MOD-RISK-PROFILES|Risk Profiles|Configure, simulate, apply, share, and version risk rules
MOD-RISK-ACCOUNTS-GROUPS|Accounts / Account Groups|Account and group context with separated rules and statistics
MOD-POSITION-MANAGEMENT|Position Management|Manage current positions and risk-sensitive edits
MOD-CLOSING|Closing|Quick and full closing workflows with audit
MOD-REVIEW-TIMELINE|Decision Timeline / Review|Compare plan and execution through ordered evidence and diffs
MOD-REVIEW-TRADE-HISTORY|Trade History|Searchable past trades and saved views
MOD-EVIDENCE-ATTACHMENTS-MARKUP|Attachments / Markup|Versioned evidence, snapshots, and annotations
MOD-CALENDAR-PL|P&L Calendar|Comparable month, quarter, and year performance views
MOD-ANALYTICS-BUILDER|Analytics Builder|Custom analytics with stable numeric meaning
MOD-REVIEW-REPORT-LIBRARY|Report Library|Searchable evidence-linked reports
MOD-REVIEW-MENTAL-STATE|Mental State|Optional privacy-sensitive pre/during/post state context
MOD-AI-IMPROVEMENT-COACH|Improvement Loop / AI Coach|Evidence-linked optional improvement workflow
MOD-AI-GPT-MANUAL|GPT Manual Export / Import|Manual optional GPT workflow with privacy preview
MOD-COLLAB-PERSONAL-TEAM|Personal / Team Collaboration|Shared plans, comments, presence, and permissions
MOD-COLLAB-ROOMS|Collaboration Rooms|Dedicated permissioned team spaces
MOD-UTILITY-ACTIVITY-FEED|Activity Feed|Permission-filtered relevant activity with critical alerts separated
MOD-UTILITY-COMMAND-PALETTE|Search / Command Palette|Permission-aware search and direct actions
MOD-UTILITY-NOTIFICATIONS|Notifications|Categorized actionable alerts with critical risk highest priority
MOD-UTILITY-FLOATING-WORKSPACE|Floating Reminder Workspace|Compact always-on-top monitoring with safety meaning
MOD-THEME-MANAGEMENT|Theme Management|Theme packages, compatibility, preview, switching, and rollback without data mutation
MOD-THEME-PERSONALIZATION|Theme Personalization Studio|Safe-range presentation personalization
MOD-SETTINGS-CENTER|Settings|Searchable professional settings with change history
MOD-DATA-RECOVERY|Recovery Center|Recover state with diff, confirmation, and audit
MOD-DATA-MIGRATION|Migration|Previewed, backed-up, no-overwrite migration
MOD-EXT-PLUGIN-MANAGEMENT|Plugin / Extension Management|Sandboxed extension control with explicit permissions
MOD-EXT-READONLY-CONNECTORS|Future Read-Only Connector Management|Read-only sync, reconciliation, and monitoring only
MOD-SETTINGS-SECURITY-DEVICES|Security / Devices|2FA, device management, App Lock, and step-up
MOD-SETTINGS-ACCESSIBILITY|Accessibility|Full Q93 accessibility profile
MOD-ONBOARDING|Quick Start / Full Setup Onboarding|Resumable safe product setup
MOD-AUTH-SECURITY|Login / App Lock / 2FA / Step-Up|Multi-user authentication and protected app access
MOD-DATA-ARCHIVE-SOFT-DELETE|Archive & Soft-Delete|Data-type-specific archive, soft-delete, restore, permission, and audit rules
MOD-LIFECYCLE-ORDER-TRACKING|Manual Order / Execution Tracking|Manual lifecycle recording only; no connector execution
MOD-LIFECYCLE-REPORTS|Closing Reports / Post-Trade Review Reports|Versioned evidence-linked lifecycle reports
MOD-REVIEW-MILESTONES|Growth & Discipline Milestones|Restrained, disableable progress without celebrating losses
MOD-DATA-EXPORT-PORTABLE-ARCHIVE|Export & Portable Archive|Open formats with private/shared separation
MOD-SETTINGS-UPDATES|Updates & Release Channels|Stable/Beta/Developer update safety with backup and rollback
'@

$zones = Convert-RegistryRows -SourceReference 'TCC Information & UX Architecture v1.1 — APPROVED §10.4' -Rows @'
ZONE-SAFETY-CORE|Safety Core|Trading Permission, total risk, current positions, and major alerts; always visible on Home and globally accessible
ZONE-CONTEXT-HEADER|Context Header|Current object, account/group, mode, and sync state
ZONE-STATUS-STRIP|Status Strip|Source, freshness, sync, and permissions
ZONE-PRIMARY-ACTION|Primary Action Area|Canonical actions with risk-sensitive confirmation
ZONE-EVIDENCE|Evidence Panel|Attachments, charts, notes, citations, and linkage
ZONE-RISK|Risk Panel|Current risk, violations, limits, impact, and corrective path
ZONE-DECISION|Decision Panel|Thesis, choices, and rationale
ZONE-TIMELINE|Timeline|Ordered event and diff history
ZONE-CHECKLIST|Checklist Zone|Required checks, gates, and score
ZONE-APPROVAL|Approval Zone|Approval requests, approvers, status, and path
ZONE-COLLABORATION|Collaboration Zone|Comments, presence, annotations, privacy, and visibility labels
ZONE-ATTACHMENTS|Attachments Zone|Files, markup, snapshots, version, and source
ZONE-AUDIT-HISTORY|Audit / History Zone|Diff, history, and restore without hiding audit
ZONE-SECONDARY-UTILITY|Secondary Utility Zone|Keyboard-accessible filters, sort, and saved views
ZONE-RECOVERY|Recovery Zone|Autosave, retry, rollback, and clear recovery action
ZONE-PRIVACY|Privacy Zone|Included data and private/shared visibility for AI/export/share
ZONE-CONFIRMATION|Confirmation Zone|Old, New, Risk Impact, required approval or validation, and explicit Confirm
'@

$states = Convert-RegistryRows -SourceReference 'TCC Information & UX Architecture v1.1 — APPROVED §36.1' -Rows @'
STATE-NORMAL|Normal|Data and actions are available
STATE-LOADING|Loading|Fetch or computation is in progress
STATE-EMPTY|Empty|No data exists or matches and a next action is available
STATE-SUCCESS|Success|An action completed with a clear result and next step
STATE-WARNING|Warning|A risk or validation issue requires cause and impact visibility
STATE-BLOCKED|Blocked|Action is not allowed and exact blocker plus resolution path are required
STATE-ERROR|Error|Operation failed with cause and retry or recovery path
STATE-OFFLINE|Offline|Network is unavailable while local-safe workflows remain available
STATE-STALE-DATA|Stale Data|Freshness is uncertain and source plus last update must be shown
STATE-SYNCING|Syncing|Sync is in progress with source and progress visible
STATE-CONFLICT|Conflict|Values disagree and diff/source plus manual resolution are required
STATE-PERMISSION-DENIED|Permission Denied|User is not authorized and a safe access path may be shown
STATE-APPROVAL-PENDING|Approval Pending|A required approval is outstanding
STATE-APPROVAL-REJECTED|Approval Rejected|A request was denied and cannot be applied
STATE-RECOVERY-AVAILABLE|Recovery Available|A recoverable state exists and restore impact can be inspected
STATE-SAFE-MODE|Safe Mode|Minimal accessible repair state with normal themes/plugins disabled
STATE-DEGRADED-PERFORMANCE|Degraded Performance|Presentation was downgraded without reducing safety or accessibility
STATE-REDUCED-MOTION|Reduced Motion|Motion is minimized and all functions remain available
STATE-REDUCED-TRANSPARENCY|Reduced Transparency|Transparency is minimized while hierarchy and readability remain
'@

$flows = Convert-RegistryRows -SourceReference 'TCC Information & UX Architecture v1.1 — APPROVED §37.1' -Rows @'
FLOW-A-PREMARKET|Start-of-Day / Pre-Market Flow|Prepare the day and evaluate readiness
FLOW-B-CREATE-PLAN|Create Trade Plan|Create a trade plan
FLOW-C-VALIDATE-PLAN|Complete Technical/Mindset/Risk Validation|Validate plan evidence, checklist, and risk
FLOW-D-PERMISSION-EVAL|Trading Permission Evaluation|Evaluate Tradable, Warning, or Blocked
FLOW-E-HARD-BLOCK-RECOVERY|Hard-Block and Recovery|Resolve a hard blocker safely
FLOW-F-FORMALIZE-OPEN|Formalize / Open a Trade|Create a formal snapshot and manually record opening
FLOW-G-MANAGE-POSITION|Manage Active Position|Monitor and manage an active position
FLOW-H-RISK-SENSITIVE-UPDATE|Update Risk-Sensitive Trade Values|Confirm Old, New, Risk Impact, and apply
FLOW-I-CLOSE-TRADE|Close Trade|Manually record a close with audit
FLOW-J-QUICK-CLOSE|Quick Close Capture|Capture immediate close data for later review
FLOW-K-DELAYED-DEEP-REVIEW|Delayed Deep Review|Complete post-trade deep review
FLOW-L-DECISION-TIMELINE|Decision Timeline Review|Compare plan, decisions, and execution
FLOW-M-CALENDAR-INVESTIGATION|P&L Calendar Investigation|Investigate performance from calendar views
FLOW-N-STRATEGY-TEMPLATE|Strategy Template Creation/Editing|Create and version strategy templates
FLOW-O-RISK-PROFILE|Risk Profile Creation/Editing/Simulation/Application|Edit, simulate, confirm, and apply risk profile changes
FLOW-P-FOLLOW-SHARED-TRADE|Follow a Shared Trade|Follow shared truth with a personal alternative view
FLOW-Q-TEAM-COLLAB-PLAN|Team Collaborative Plan|Collaborate on a permissioned shared plan
FLOW-R-TEAM-APPROVAL|Team Approval|Review evidence and decide a gated approval
FLOW-S-TRADE-FIELD-CONFLICT|Resolve Realtime Trade-Field Conflict|Manually resolve core trade-field conflict
FLOW-T-GPT-EXPORT|GPT Manual Export|Prepare and export privacy-reviewed context manually
FLOW-U-GPT-IMPORT|GPT Result Import|Validate and import optional GPT results without silent mutation
FLOW-V-CRASH-RECOVERY|Recovery After Crash|Inspect and safely recover autosaved state
FLOW-W-MIGRATION|Legacy-Data Migration|Preview, back up, validate, migrate, retry, or roll back
FLOW-X-THEME-SWITCH|Theme Switching|Preview, confirm, and switch presentation safely
FLOW-Y-THEME-ROLLBACK|Theme Failure Rollback|Fallback and roll back a failed theme
FLOW-Z-PLUGIN-PERMISSION|Plugin Enable / Permission Workflow|Review sandbox permissions before enabling
FLOW-AA-ONBOARDING|Quick Start / Full Setup Onboarding|Complete or resume safe setup
FLOW-AB-AUTH-SECURITY|Login / App Lock / 2FA / Step-Up Verification|Authenticate and verify sensitive actions
FLOW-AC-ARCHIVE-SOFT-DELETE|Archive / Soft-Delete / Restore Data|Apply data-type lifecycle rules with confirmation and audit
FLOW-AD-LIFECYCLE-REPORTS|Manual Order Tracking / Closing Report / Post-Trade Review Report|Record manual lifecycle and reports
FLOW-AE-READONLY-CONNECTOR|Read-Only Connector Enable / Reconcile / Monitor|Enable only read-only sync, reconciliation, and monitoring
FLOW-AF-COMMAND-PALETTE-DIRECT-ACTION|Command Palette Direct Action|Run permission-aware direct actions with required confirmation
'@

function Convert-PageRows {
    param([Parameter(Mandatory)] [string] $Rows)

    return @($Rows.Trim().Split("`n", [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
        $parts = $_.Trim().Split('|')
        [ordered]@{
            id = $parts[0]
            name = $parts[1]
            required_theme_contract = $parts[2]
            safety_constraints = @($parts[3].Split(';', [System.StringSplitOptions]::RemoveEmptyEntries))
            theme_freedom = $parts[4]
            required_theme_tests = @($parts[5].Split(';', [System.StringSplitOptions]::RemoveEmptyEntries))
            source_references = @(
                'TCC Information & UX Architecture v1.1 — APPROVED §9.1',
                'TCC Theme Architecture v1.2 — APPROVED §30.2'
            )
        }
    })
}

$pages = Convert-PageRows -Rows @'
UX-HOME-001|Command Center Home|Home command surface|ZONE-SAFETY-CORE visible;Trading Permission, total risk, current positions, and major alerts preserved|high_outside_safety_core|safety_core;deep_light;reduced_modes
UX-HOME-002|Daily Brief|Daily brief surface|Alerts and risk visible;No hidden blockers|high|daily_brief_state;accessibility
UX-ONB-001|Quick Start Onboarding|Quick Start onboarding|Safety Core meaning preserved;Accessibility entry available|medium|keyboard_wizard;screen_reader
UX-ONB-002|Full Setup Onboarding|Full Setup onboarding|High-impact setup confirmation clarity|medium|setup_confirmation;reduced_modes
UX-ONB-003|Onboarding Resume|Onboarding resume|Progress visible and accessible;No data loss on resume|medium|resume_state
UX-RISK-001|Trading Permission Overview|Trading Permission overview|Tradable, Warning, and Blocked semantics fixed|medium|permission_state_non_color_only
UX-RISK-002|Risk Profile Library|Risk Profile library|Version and active-profile clarity|medium|version_labels;contrast
UX-RISK-003|Risk Profile Editor|Risk Profile editor|Rule-edit impact not obscured|low_medium|forms;keyboard;confirmation
UX-RISK-004|Risk Simulation|Risk simulation|Old, New, and Risk Impact visible;Pre-apply simulation required|low|simulation_tables_accessible
UX-RISK-005|Accounts & Groups|Accounts and groups|Account separation visible|medium|account_labels
UX-PLAN-001|Trade Plan Dashboard|Plan dashboard|Draft and formal distinction preserved|high|list_grid_states
UX-PLAN-002|Trade Plan Workbench|Plan workbench|Risk, template, and account context visible;Formal snapshot rule preserved|medium|forms;evidence;accessibility
UX-PLAN-003|Checklist & Gates|Checklist and gates|Gate semantics fixed;Non-color-only state|medium|gate_status
UX-PLAN-004|Strategy Template Library|Template library|Source and version visible|high|template_list
UX-PLAN-005|Strategy Template Editor|Template editor|Required fields, gates, and conditions clear|medium|rule_editor_keyboard
UX-POS-001|Positions Overview|Positions overview|Current positions and risk visible|medium|dense_table_accessibility
UX-POS-002|Position Detail|Position detail|Risk-sensitive edits require confirmation|low_medium|diff;confirmation
UX-POS-003|Position Focus Mode|Focus mode|Critical alerts interrupt focus mode|medium|focus_mode_alerts
UX-CLOSE-001|Close Dashboard|Close dashboard|Close status and risk context visible|high|close_actions
UX-CLOSE-002|Quick Close|Quick close|No celebration on loss|medium|loss_feedback_negative
UX-CLOSE-003|Full Close|Full close|Material close confirmation preserved|medium|guided_flow
UX-ORDER-001|Manual Order Tracking|Manual order tracking|Manual V1 only;No connector execution|medium|read_only_boundary_labels
UX-CLOSE-004|Closing Report|Closing report|Report visibility and close values preserved|high|report_structure
UX-REVIEW-001|Review Dashboard|Review dashboard|Optional AI;Review status visible|high|optional_ai_absence
UX-REVIEW-002|Decision Timeline|Decision timeline|Diff and audit order preserved|medium|timeline_semantics
UX-REVIEW-003|Trade History|Trade history|Saved views do not alter data|high|table_list
UX-REVIEW-004|Report Library|Report library|Evidence trace preserved|high|report_cards
UX-REVIEW-005|Mental State|Mental state|Privacy labels visible;Context remains optional|medium|sensitive_labels
UX-REVIEW-006|Post-Trade Review|Post-trade review|Rule, self, team, and GPT scores side-by-side;GPT optional and non-official|medium|gpt_optional_non_official
UX-MILE-001|Growth & Discipline Milestones|Milestones|Disableable;No loss celebration|high|disable;no_loss_celebration
UX-CAL-001|Monthly P&L Calendar|Monthly calendar|Numeric meaning and source preserved|high|heatmap_non_color_values
UX-CAL-002|Quarterly Heatmap|Quarterly heatmap|Comparability preserved|high|scale;contrast
UX-CAL-003|Yearly Heatmap|Yearly heatmap|Strong comparability preserved|high|screen_reader_values
UX-ANALYTICS-001|Analytics Builder|Analytics builder|Numeric semantics fixed|high|chart_alternatives
UX-ANALYTICS-002|Historical Search|Historical search|Permission-filtered results|high|search_accessibility
UX-TEAM-001|Team Command Center|Team command center|Shared, private, and team boundaries visible|high|boundary_labels
UX-TEAM-002|Shared Trades|Shared trades|Shared truth visible|high|shared_status_labels
UX-TEAM-003|Team Approvals|Team approvals|Old, New, Risk Impact, and evidence visible|low|approval_confirmation
UX-TEAM-004|Activity Feed|Activity feed|Critical risk separated from ordinary feed|high|feed_filtering
UX-ROOM-001|Collaboration Room List|Room list|Access boundaries visible|high|room_permissions
UX-ROOM-002|Collaboration Room Workspace|Room workspace|Conflict and approval handling preserved|high|collaboration_conflict
UX-COLLAB-001|Comments & Annotations|Comments and annotations|Q34 taxonomy fixed|high_style_only|status_taxonomy
UX-AI-001|GPT Export Center|GPT export center|Manual and optional export;Privacy preview required|medium|privacy_export_labels
UX-AI-002|GPT Privacy Preview|Privacy preview|Included data and privacy boundary visible|low|critical_privacy_screen_reader
UX-AI-003|GPT Import|GPT import|Optional analysis;No silent changes|medium|import_validation
UX-AI-004|AI Coach|AI coach|Evidence-linked and optional;No silent mutation|medium|no_silent_mutation
UX-DATA-001|Data Center|Data center|Local-first and source state visible|medium|data_actions
UX-ARCH-001|Archive & Soft-Delete Center|Archive and soft-delete|Destructive warning plain;Data-type rules fixed|low|old_new_impact_confirm
UX-DATA-002|Export & Portable Archive|Export and portable archive|Private and shared separation visible|low_medium|export_preview
UX-REC-001|Recovery Center|Recovery Center|Diff and restore clarity;Overwrite confirmation required|low|recovery_confirmation
UX-MIG-001|Migration Start|Migration start|No-overwrite and backup visible|medium|migration_scan
UX-MIG-002|Migration Preview & Validation|Migration preview|Validation, retry, and rollback visible;No overwrite|low|migration_validation
UX-THEME-001|Theme Library|Theme Library|Compatibility, integrity, and accessibility visible;No data mutation|high|install_switch_states
UX-THEME-002|Theme Preview|Theme Preview|Preview cannot mutate authoritative data;Audio controls visible;Exit always available|high|preview_audio;no_data_mutation
UX-THEME-003|Personalization Studio|Personalization Studio|Safe ranges and reset preserved|high|clamp;accessibility
UX-SET-001|Settings Center|Settings Center|High-impact settings marked|high|settings_search
UX-SET-002|Accessibility Settings|Accessibility Settings|Full Q93 controls available|medium|full_accessibility_profile
UX-SET-003|Security & Devices|Security and devices|Step-up and security wording plain|low_medium|secure_forms
UX-SET-004|Notifications & Sound|Notifications and sound|Critical priority fixed;Sound remains supplementary|high|audio_disabled;night_mode
UX-SET-005|Keyboard Shortcuts|Keyboard shortcuts|Shortcut conflict checks visible|medium|keyboard_native
UX-SET-006|Updates & Release Channels|Updates and channels|Backup, compatibility, and rollback visible|medium|breaking_notice
UX-AUTH-001|Login / User Selection|Login and user selection|Protected data hidden before authentication|low|secure_labels
UX-AUTH-002|App Lock / Unlock|App Lock and unlock|Locked data hidden;Unlock accessible|low|unlock_accessibility
UX-AUTH-003|Verification / 2FA / Step-Up|2FA and Step-Up|Action reason clear;Cancellation safe|low|cancel_safety
UX-EXT-001|Plugin Manager|Plugin manager|Sandbox and Safe Mode semantics fixed|medium|plugin_coexistence
UX-EXT-002|Plugin Permission Review|Plugin permission review|Permission impact plain;Explicit confirmation|low|permission_labels
UX-CONN-001|Read-Only Connector Manager|Read-only connector manager|No write or execution capability|low_medium|connector_boundary_negative
UX-NOTIF-001|Notification Center|Notification center|Critical risk highest priority;Redundant alert cues|high|alert_redundancy
UX-CMD-001|Command Palette|Command palette|Permission and risk confirmation preserved|high|direct_action_safety
UX-FLOAT-001|Floating Workspace|Floating Workspace|Compact safety visible;Multi-monitor and DPI safe|medium|dpi_multi_monitor
UX-SAFE-001|Safe Mode Home|Safe Mode Home|Minimal accessible presentation;No decoration dependency|low_visual|safe_mode_recovery
'@

$baseStates = @(
    'STATE-NORMAL', 'STATE-LOADING', 'STATE-EMPTY', 'STATE-SUCCESS',
    'STATE-WARNING', 'STATE-BLOCKED', 'STATE-ERROR', 'STATE-OFFLINE'
)

function Get-SupportedStates {
    param([Parameter(Mandatory)] [string] $PageId)

    $additional = switch -Regex ($PageId) {
        '^UX-HOME-001$' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-PENDING','STATE-APPROVAL-REJECTED','STATE-RECOVERY-AVAILABLE','STATE-SAFE-MODE','STATE-DEGRADED-PERFORMANCE','STATE-REDUCED-MOTION','STATE-REDUCED-TRANSPARENCY'); break }
        '^UX-REVIEW-006$' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-PENDING','STATE-RECOVERY-AVAILABLE'); break }
        '^UX-(HOME|ONB)-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-DEGRADED-PERFORMANCE','STATE-REDUCED-MOTION','STATE-REDUCED-TRANSPARENCY'); break }
        '^UX-RISK-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-PENDING','STATE-APPROVAL-REJECTED','STATE-RECOVERY-AVAILABLE'); break }
        '^UX-PLAN-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-PENDING','STATE-RECOVERY-AVAILABLE'); break }
        '^UX-(POS|ORDER)-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-PENDING','STATE-RECOVERY-AVAILABLE'); break }
        '^UX-CLOSE-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-PENDING','STATE-RECOVERY-AVAILABLE'); break }
        '^UX-(REVIEW|MILE)-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE'); break }
        '^UX-(CAL|ANALYTICS)-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-DEGRADED-PERFORMANCE'); break }
        '^UX-(TEAM|ROOM|COLLAB)-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-PENDING','STATE-APPROVAL-REJECTED','STATE-RECOVERY-AVAILABLE'); break }
        '^UX-AI-' { @('STATE-STALE-DATA','STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-DEGRADED-PERFORMANCE'); break }
        '^UX-(DATA|ARCH|REC|MIG)-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-SAFE-MODE'); break }
        '^UX-THEME-' { @('STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-SAFE-MODE','STATE-DEGRADED-PERFORMANCE','STATE-REDUCED-MOTION','STATE-REDUCED-TRANSPARENCY'); break }
        '^UX-(SET|AUTH)-' { @('STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-SAFE-MODE','STATE-DEGRADED-PERFORMANCE','STATE-REDUCED-MOTION','STATE-REDUCED-TRANSPARENCY'); break }
        '^UX-(EXT|CONN)-' { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-SAFE-MODE'); break }
        default { @('STATE-STALE-DATA','STATE-SYNCING','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-RECOVERY-AVAILABLE','STATE-SAFE-MODE','STATE-DEGRADED-PERFORMANCE','STATE-REDUCED-MOTION','STATE-REDUCED-TRANSPARENCY') }
    }

    return @($baseStates + $additional | Select-Object -Unique)
}

$accessibilityRequirements = [ordered]@{
    keyboard = $true
    screen_reader_labels = $true
    screen_reader_announcements = $true
    focus_visible = $true
    full_keyboard_operation = $true
    text_scaling = $true
    zoom = $true
    contrast = $true
    color_vision = $true
    reduced_motion = $true
    reduced_transparency = $true
    sound_controls = $true
    critical_alert_redundancy = $true
}

$invariants = @(
    'core_ia_meaning',
    'home_safety_core_visible',
    'critical_alert_highest_priority',
    'trading_permission_tradable_warning_blocked',
    'risk_permission_semantics_fixed',
    'old_new_risk_impact_confirm',
    'manual_core_conflict_resolution',
    'gpt_optional_and_non_official',
    'connector_strictly_read_only',
    'accessibility_q93_required',
    'installer_portable_core_workflow_parity'
)

$uxContract = [ordered]@{
    '$schema' = 'https://schemas.tcc.local/theme/ux-contract.v1.1.schema.json'
    schema_version = '1.0'
    ux_contract_version = '1.1.0'
    ux_architecture_display_label = 'v1.1'
    theme_architecture_version = '1.0.0'
    minimum_theme_api_version = '1.0.0'
    provenance = [ordered]@{
        authority = 'TCC Information & UX Architecture v1.1 — APPROVED'
        derived_contract_source = 'TCC Theme Architecture v1.2 — APPROVED §§6, 30'
        generation_rule = 'Preserve exact approved stable IDs and semantics; reject duplicate or unknown IDs.'
    }
    invariants = $invariants
    domains = $domains
    workspaces = $workspaces
    modules = $modules
    zones = $zones
    states = $states
    flows = $flows
    surfaces = @($pages | ForEach-Object {
        $surface = [ordered]@{}
        foreach ($key in $_.Keys) { $surface[$key] = $_[$key] }
        $surface.supported_states = @(Get-SupportedStates -PageId $_.id)
        $surface.accessibility_obligations = $accessibilityRequirements
        $surface.visual_regression_coverage = @('declared_variants','applicable_states','accessibility_modes','dpi_buckets','critical_alert_overlay')
        $surface.performance_coverage = @('safety_core_continuity','non_blocking_theme_work','deterministic_degradation')
        $surface
    })
}

Write-JsonFile -Path (Join-Path $contractRoot 'ux-contract.v1.1.json') -Value $uxContract

foreach ($page in $uxContract.surfaces) {
    $pageContract = [ordered]@{
        '$schema' = 'https://schemas.tcc.local/theme/UxSurfaceContract.schema.json'
        schema_version = '1.0'
        ux_contract_version = '1.1.0'
        surface_id = $page.id
        surface_type = 'page'
        surface_name = $page.name
        required_theme_contract = $page.required_theme_contract
        supported_states = $page.supported_states
        invariants = $invariants
        accessibility_requirements = $accessibilityRequirements
        safety_constraints = $page.safety_constraints
        theme_freedom = $page.theme_freedom
        required_theme_tests = $page.required_theme_tests
        provenance = $page.source_references
    }
    Write-JsonFile -Path (Join-Path $contractRoot "page-contracts\$($page.id).json") -Value $pageContract
}

foreach ($module in $modules) {
    $moduleContract = [ordered]@{
        schema_version = '1.0'
        ux_contract_version = '1.1.0'
        module_id = $module.id
        module_name = $module.name
        semantics = $module.semantics
        source_reference = $module.source_reference
        theme_must_not_change = @('meaning','business_semantics','risk_semantics','permission_semantics','accessibility')
    }
    Write-JsonFile -Path (Join-Path $contractRoot "module-contracts\$($module.id).json") -Value $moduleContract
}

foreach ($zone in $zones) {
    $isSafetyCore = $zone.id -eq 'ZONE-SAFETY-CORE'
    $semanticChildren = @(
        if ($isSafetyCore) {
            'trading_permission'
            'total_risk'
            'current_positions'
            'major_alerts'
        }
    )
    $zoneContract = [ordered]@{
        '$schema' = 'https://schemas.tcc.local/theme/FunctionalZoneContract.schema.json'
        schema_version = '1.0'
        zone_id = $zone.id
        zone_name = $zone.name
        semantics = $zone.semantics
        priority = if ($isSafetyCore) { 'critical' } else { 'contextual' }
        rearrangeability = if ($isSafetyCore) { 'limited' } else { 'contract_defined' }
        required_visibility = if ($isSafetyCore) { 'always_on_home_and_globally_accessible' } else { 'when_applicable' }
        may_be_collapsed = -not $isSafetyCore
        semantic_children = $semanticChildren
        theme_may_change = @('visual_style','component_skin','iconography','motion_supplement','sound_supplement')
        theme_must_not_change = @('presence_when_required','meaning','priority','labels','programmatic_state','accessibility')
        provenance = @($zone.source_reference)
    }
    Write-JsonFile -Path (Join-Path $contractRoot "zone-contracts\$($zone.id).json") -Value $zoneContract
}

foreach ($state in $states) {
    $stateContract = [ordered]@{
        '$schema' = 'https://schemas.tcc.local/theme/GlobalStatePresentation.schema.json'
        schema_version = '1.0'
        state_id = $state.id
        required_semantics = [ordered]@{
            meaning = $state.semantics
            must_show_reason = $state.id -in @('STATE-WARNING','STATE-BLOCKED','STATE-ERROR','STATE-CONFLICT','STATE-PERMISSION-DENIED','STATE-APPROVAL-REJECTED','STATE-SAFE-MODE')
            must_show_exit_or_resolution_path = $state.id -notin @('STATE-NORMAL','STATE-SUCCESS','STATE-REDUCED-MOTION','STATE-REDUCED-TRANSPARENCY')
            may_offer_corrective_action = $true
        }
        required_redundancy = [ordered]@{
            text = $true
            icon_or_structure = $true
            programmatic_state = $true
            screen_reader_announcement = $true
            color_only_forbidden = $true
            motion_only_forbidden = $true
            sound_only_forbidden = $true
            transparency_only_forbidden = $true
        }
        theme_freedom = [ordered]@{
            color = 'allowed_if_contrast_valid'
            animation = 'supplementary_only'
            sound = 'supplementary_only'
            metaphor = 'allowed_if_plain_label_preserved'
        }
        provenance = @($state.source_reference)
    }
    Write-JsonFile -Path (Join-Path $contractRoot "state-contracts\$($state.id).json") -Value $stateContract
}

Write-JsonFile -Path (Join-Path $contractRoot 'accessibility-contract.v1.json') -Value ([ordered]@{
    schema_version = '1.0'
    q93_required = $true
    requirements = $accessibilityRequirements
    text_contrast_minimum = 4.5
    large_text_contrast_minimum = 3.0
    non_text_contrast_minimum = 3.0
    focus_indicator_contrast_minimum = 3.0
    text_scale_test_percentages = @(100,125,150,175,200)
    zoom_test_percentages = @(100,125,150,175,200)
    combined_profiles_required = $true
    source_references = @('TCC Theme Architecture v1.2 — APPROVED §§21–23','TCC Information & UX Architecture v1.1 — APPROVED §34')
})

Write-JsonFile -Path (Join-Path $contractRoot 'motion-contract.v1.json') -Value ([ordered]@{
    schema_version = '1.0'
    page_transitions_interruptible = $true
    page_transitions_skippable = $true
    reduced_motion_mandatory = $true
    parallax_static_fallback = $true
    motion_is_supplementary = $true
    critical_alert_preempts_motion = $true
    loss_celebration_forbidden = $true
    source_reference = 'TCC Theme Architecture v1.2 — APPROVED §§19, 21'
})

Write-JsonFile -Path (Join-Path $contractRoot 'audio-contract.v1.json') -Value ([ordered]@{
    schema_version = '1.0'
    presentation_only = $true
    supplementary_only = $true
    one_click_disable_required = $true
    independent_categories = @('master_theme_audio','bgm','ambient','ui_sound','alert_supplement')
    normalized_volume_range = [ordered]@{ minimum = 0.0; maximum = 1.0 }
    night_mode_required = $true
    ducking_required = $true
    critical_alert_visual_text_redundancy_required = $true
    loss_celebration_forbidden = $true
    source_reference = 'TCC Theme Architecture v1.2 — APPROVED §20'
})

function New-StringSchema {
    param([string] $Pattern, [string[]] $Enum)
    $schema = [ordered]@{ type = 'string'; minLength = 1 }
    if ($Pattern) { $schema.pattern = $Pattern }
    if ($Enum) { $schema.enum = $Enum }
    return $schema
}

function New-BooleanSchema { return [ordered]@{ type = 'boolean' } }

function New-NumberSchema {
    param([decimal] $Minimum = 0, [decimal] $Maximum = [decimal]::MaxValue)
    $schema = [ordered]@{ type = 'number'; minimum = $Minimum }
    if ($Maximum -ne [decimal]::MaxValue) { $schema.maximum = $Maximum }
    return $schema
}

function New-IntegerSchema {
    param([long] $Minimum = 0)
    return [ordered]@{ type = 'integer'; minimum = $Minimum }
}

function New-ArraySchema {
    param([Parameter(Mandatory)] [object] $Items, [int] $MinItems = 0, [bool] $UniqueItems = $false)
    return [ordered]@{ type = 'array'; items = $Items; minItems = $MinItems; uniqueItems = $UniqueItems }
}

function New-ObjectSchema {
    param(
        [string[]] $Required,
        [Parameter(Mandatory)] [System.Collections.IDictionary] $Properties,
        [object] $AdditionalProperties = $false
    )
    return [ordered]@{
        type = 'object'
        required = @($Required | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        properties = $Properties
        additionalProperties = $AdditionalProperties
    }
}

function New-RootSchema {
    param(
        [Parameter(Mandatory)] [string] $Name,
        [Parameter(Mandatory)] [string[]] $Required,
        [Parameter(Mandatory)] [System.Collections.IDictionary] $Properties
    )
    $schema = New-ObjectSchema -Required $Required -Properties $Properties
    $schema = [ordered]@{
        '$schema' = 'https://json-schema.org/draft/2020-12/schema'
        '$id' = "https://schemas.tcc.local/theme/$Name.schema.json"
        title = $Name
        type = $schema.type
        required = $schema.required
        properties = $schema.properties
        additionalProperties = $schema.additionalProperties
    }
    return $schema
}

$semver = '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$'
$schemaVersion = [ordered]@{ type = 'string'; const = '1.0' }
$themeId = New-StringSchema -Pattern '^[a-z0-9]+(?:[.-][a-z0-9]+)+$'
$semverString = New-StringSchema -Pattern $semver
$surfaceId = New-StringSchema -Enum @($pages.id)
$zoneId = New-StringSchema -Enum @($zones.id)
$globalStateId = New-StringSchema -Enum @($states.id)
$fixedWorkspaceIds = @($workspaces.id | Where-Object { $_ -ne 'WS-CUSTOM-*' })
$workspaceId = [ordered]@{
    anyOf = @(
        (New-StringSchema -Enum $fixedWorkspaceIds),
        (New-StringSchema -Pattern '^WS-CUSTOM-[A-Z0-9][A-Z0-9-]*$')
    )
}
$nonEmptyString = New-StringSchema
$stringArray = New-ArraySchema -Items $nonEmptyString -UniqueItems $true
$requiredStringArray = New-ArraySchema -Items $nonEmptyString -MinItems 1 -UniqueItems $true
$allowedCapabilities = @(
    'presentation.tokens','presentation.layout','presentation.components','presentation.icons',
    'presentation.copy.noncritical','presentation.motion','presentation.parallax',
    'presentation.cursor.showcase_limited','presentation.audio.ui','presentation.audio.bgm',
    'presentation.audio.ambient','presentation.loading_empty_error','presentation.time_of_day_scene',
    'presentation.trading_state_overlay','presentation.workspace_appearance',
    'presentation.floating_workspace_appearance','presentation.personalization'
)

$keyboardAndFocus = New-ObjectSchema -Required @(
    'focus_order_policy','keyboard_activation_policy','modal_focus_trap_preserved',
    'focus_restoration_target','shortcut_conflict_policy','custom_composite_keyboard_pattern',
    'hidden_element_focus_policy','preview_keyboard_exit_required','escape_cancel_behavior'
) -Properties ([ordered]@{
    focus_order_policy = $nonEmptyString
    keyboard_activation_policy = $nonEmptyString
    modal_focus_trap_preserved = [ordered]@{ const = $true }
    focus_restoration_target = $nonEmptyString
    shortcut_conflict_policy = $nonEmptyString
    custom_composite_keyboard_pattern = $nonEmptyString
    hidden_element_focus_policy = $nonEmptyString
    preview_keyboard_exit_required = [ordered]@{ const = $true }
    escape_cancel_behavior = $nonEmptyString
})

$themeManifestSchema = New-RootSchema -Name 'ThemeManifest' -Required @(
    'schema_version','theme_api_version','package','compatibility','variants','capabilities',
    'feature_flags','degraded_mode','accessibility','assets','motion','integrity'
) -Properties ([ordered]@{
    schema_version = $schemaVersion
    theme_api_version = [ordered]@{ type = 'string'; const = '1.0.0' }
    package = New-ObjectSchema -Required @('theme_id','package_id','name','publisher','publisher_id','version','channel','description','license','tags') -Properties ([ordered]@{
        theme_id = $themeId
        package_id = $themeId
        name = $nonEmptyString
        publisher = $nonEmptyString
        publisher_id = New-StringSchema -Pattern '^[a-z0-9]+(?:[.-][a-z0-9]+)*$'
        version = $semverString
        channel = New-StringSchema -Enum @('stable','beta','developer')
        description = $nonEmptyString
        homepage = [ordered]@{ type = @('string','null') }
        support_url = [ordered]@{ type = @('string','null') }
        license = $nonEmptyString
        tags = $stringArray
    })
    compatibility = New-ObjectSchema -Required @('required_core_version','required_theme_api_version','required_ux_contract_version','supported_platforms','portable_supported','minimum_dpi_scale','maximum_tested_dpi_scale') -Properties ([ordered]@{
        required_core_version = $nonEmptyString
        required_theme_api_version = $nonEmptyString
        required_ux_contract_version = $nonEmptyString
        supported_platforms = New-ArraySchema -Items (New-StringSchema -Enum @('windows')) -MinItems 1 -UniqueItems $true
        portable_supported = [ordered]@{ const = $true }
        minimum_dpi_scale = New-NumberSchema -Minimum 1.0
        maximum_tested_dpi_scale = New-NumberSchema -Minimum 1.0
    })
    variants = New-ArraySchema -MinItems 1 -UniqueItems $true -Items (New-ObjectSchema -Required @('variant_id','display_name','type','default','token_file','accessibility_overrides') -Properties ([ordered]@{
        variant_id = New-StringSchema -Pattern '^[a-z0-9][a-z0-9._-]*$'
        display_name = $nonEmptyString
        type = New-StringSchema -Enum @('deep','light','high_contrast','custom')
        default = New-BooleanSchema
        token_file = $nonEmptyString
        accessibility_overrides = $stringArray
        fallback_variant_id = $nonEmptyString
    }))
    capabilities = New-ArraySchema -Items (New-StringSchema -Enum $allowedCapabilities) -MinItems 1 -UniqueItems $true
    feature_flags = New-ObjectSchema -Required @('presentation_only') -Properties ([ordered]@{
        presentation_only = New-ArraySchema -Items (New-ObjectSchema -Required @('flag_id','default_enabled','description','cannot_affect') -Properties ([ordered]@{
            flag_id = New-StringSchema -Pattern '^presentation\.'
            default_enabled = New-BooleanSchema
            description = $nonEmptyString
            cannot_affect = $requiredStringArray
        }))
    })
    degraded_mode = New-ObjectSchema -Required @('supported','levels','preserves','default_fallback_level') -Properties ([ordered]@{
        supported = [ordered]@{ const = $true }
        levels = New-ArraySchema -Items (New-StringSchema -Enum @('none','reduced_decoration','minimal_decoration','safe_presentation_only')) -MinItems 4 -UniqueItems $true
        preserves = $requiredStringArray
        default_fallback_level = [ordered]@{ const = 'safe_presentation_only' }
    })
    accessibility = New-ObjectSchema -Required @(
        'q93_compliant_claim','text_scaling_supported','zoom_supported','contrast_supported',
        'color_vision_modes_supported','reduced_motion_supported','reduced_transparency_supported',
        'sound_controls_supported','keyboard_focus_visible','full_keyboard_operation',
        'screen_reader_labels_preserved','critical_alert_redundancy_supported'
    ) -Properties ([ordered]@{
        q93_compliant_claim = New-BooleanSchema
        text_scaling_supported = [ordered]@{ const = $true }
        zoom_supported = [ordered]@{ const = $true }
        contrast_supported = [ordered]@{ const = $true }
        color_vision_modes_supported = [ordered]@{ const = $true }
        reduced_motion_supported = [ordered]@{ const = $true }
        reduced_transparency_supported = [ordered]@{ const = $true }
        sound_controls_supported = [ordered]@{ const = $true }
        keyboard_focus_visible = [ordered]@{ const = $true }
        full_keyboard_operation = [ordered]@{ const = $true }
        screen_reader_labels_preserved = [ordered]@{ const = $true }
        critical_alert_redundancy_supported = [ordered]@{ const = $true }
    })
    assets = New-ObjectSchema -Required @('inventory','tiers','total_declared_size_bytes','hash_algorithm') -Properties ([ordered]@{
        inventory = $nonEmptyString
        tiers = New-ArraySchema -Items (New-StringSchema -Enum @('tier0','tier1','tier2','tier3','audio')) -MinItems 1 -UniqueItems $true
        total_declared_size_bytes = New-IntegerSchema
        hash_algorithm = [ordered]@{ const = 'sha256' }
    })
    audio = New-ObjectSchema -Required @('sound_pack','bgm','ambient','optional','one_click_disable_supported','night_mode_supported','ducking_supported') -Properties ([ordered]@{
        sound_pack = $nonEmptyString
        bgm = $nonEmptyString
        ambient = $nonEmptyString
        optional = [ordered]@{ const = $true }
        one_click_disable_supported = [ordered]@{ const = $true }
        night_mode_supported = [ordered]@{ const = $true }
        ducking_supported = [ordered]@{ const = $true }
    })
    motion = New-ObjectSchema -Required @('profiles','reduced_motion','interruptible_transitions','skippable_transitions','parallax_static_fallback') -Properties ([ordered]@{
        profiles = $nonEmptyString
        reduced_motion = $nonEmptyString
        interruptible_transitions = [ordered]@{ const = $true }
        skippable_transitions = [ordered]@{ const = $true }
        parallax_static_fallback = [ordered]@{ const = $true }
    })
    personalization = New-ObjectSchema -Required @('presets','safe_ranges','migrations','reset_supported') -Properties ([ordered]@{
        presets = $nonEmptyString
        safe_ranges = $nonEmptyString
        migrations = $nonEmptyString
        reset_supported = [ordered]@{ const = $true }
    })
    integrity = New-ObjectSchema -Required @('integrity_manifest','signature_required_for_channel','hash_algorithm') -Properties ([ordered]@{
        integrity_manifest = $nonEmptyString
        signature = $nonEmptyString
        signature_required_for_channel = $nonEmptyString
        hash_algorithm = [ordered]@{ const = 'sha256' }
    })
    rollback = New-ObjectSchema -Required @('rollback_manifest','state_migration_supported','previous_version_compatibility') -Properties ([ordered]@{
        rollback_manifest = $nonEmptyString
        state_migration_supported = New-BooleanSchema
        previous_version_compatibility = $stringArray
    })
})
Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeManifest.schema.json') -Value $themeManifestSchema

$integrityFile = New-ObjectSchema -Required @('path','sha256','size_bytes','required') -Properties ([ordered]@{
    path = New-StringSchema -Pattern '^(?![A-Za-z]:|/|\\|.*\.\.[/\\]).+$'
    sha256 = New-StringSchema -Pattern '^[A-Fa-f0-9]{64}$'
    size_bytes = New-IntegerSchema
    required = New-BooleanSchema
})
Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeIntegrity.schema.json') -Value (New-RootSchema -Name 'ThemeIntegrity' -Required @('schema_version','theme_id','version','hash_algorithm','files','package_hash','created_at') -Properties ([ordered]@{
    schema_version = $schemaVersion
    theme_id = $themeId
    version = $semverString
    hash_algorithm = [ordered]@{ const = 'sha256' }
    files = New-ArraySchema -Items $integrityFile -MinItems 1 -UniqueItems $true
    package_hash = New-StringSchema -Pattern '^[A-Fa-f0-9]{64}$'
    created_at = [ordered]@{ type = 'string'; format = 'date-time' }
}))

$versionRange = New-ObjectSchema -Required @('required','tested') -Properties ([ordered]@{
    required = $nonEmptyString
    tested = New-ArraySchema -Items $semverString -MinItems 1 -UniqueItems $true
})
Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeCompatibility.schema.json') -Value (New-RootSchema -Name 'ThemeCompatibility' -Required @('schema_version','theme_id','version','core','theme_api','ux_contract','windows','accessibility_matrix') -Properties ([ordered]@{
    schema_version = $schemaVersion
    theme_id = $themeId
    version = $semverString
    core = $versionRange
    theme_api = $versionRange
    ux_contract = $versionRange
    windows = New-ObjectSchema -Required @('installer_supported','portable_supported','multi_monitor_supported','dpi_ranges_tested') -Properties ([ordered]@{
        installer_supported = [ordered]@{ const = $true }
        portable_supported = [ordered]@{ const = $true }
        multi_monitor_supported = [ordered]@{ const = $true }
        dpi_ranges_tested = New-ArraySchema -Items (New-StringSchema -Pattern '^[0-9]+%$') -MinItems 1 -UniqueItems $true
    })
    accessibility_matrix = New-ObjectSchema -Required @('deep','light','reduced_motion','reduced_transparency','high_contrast','color_vision','keyboard','screen_reader') -Properties ([ordered]@{
        deep = [ordered]@{ const = 'pass_required' }
        light = [ordered]@{ const = 'pass_required' }
        reduced_motion = [ordered]@{ const = 'pass_required' }
        reduced_transparency = [ordered]@{ const = 'pass_required' }
        high_contrast = [ordered]@{ const = 'pass_required' }
        color_vision = [ordered]@{ const = 'pass_required' }
        keyboard = [ordered]@{ const = 'pass_required' }
        screen_reader = [ordered]@{ const = 'pass_required' }
    })
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeRollback.schema.json') -Value (New-RootSchema -Name 'ThemeRollback' -Required @('schema_version','theme_id','version','rollback_supported','rollback_targets','non_migratable_state_policy') -Properties ([ordered]@{
    schema_version = $schemaVersion
    theme_id = $themeId
    version = $semverString
    rollback_supported = [ordered]@{ const = $true }
    rollback_targets = New-ArraySchema -MinItems 1 -Items (New-ObjectSchema -Required @('from_version','to_version_range','state_migration','cache_invalidation') -Properties ([ordered]@{
        from_version = $semverString
        to_version_range = $nonEmptyString
        state_migration = $nonEmptyString
        cache_invalidation = $requiredStringArray
    }))
    non_migratable_state_policy = [ordered]@{ const = 'reset_theme_owned_state_only' }
}))

$assetBudgets = New-ObjectSchema -Required @(
    'max_total_package_bytes','max_extracted_package_bytes','max_tier0_bytes','max_tier0_decoded_memory_bytes',
    'max_tier1_bytes','max_tier2_bytes','max_tier3_bytes','max_audio_total_bytes','max_bgm_total_bytes',
    'max_preview_assets_bytes','max_single_asset_bytes','max_single_asset_width_px','max_single_asset_height_px',
    'max_single_asset_decoded_memory_bytes','max_gpu_texture_memory_bytes','max_cache_bytes_per_theme','max_cache_bytes_global'
) -Properties ([ordered]@{
    max_total_package_bytes = New-IntegerSchema
    max_extracted_package_bytes = New-IntegerSchema
    max_tier0_bytes = New-IntegerSchema
    max_tier0_decoded_memory_bytes = New-IntegerSchema
    max_tier1_bytes = New-IntegerSchema
    max_tier2_bytes = New-IntegerSchema
    max_tier3_bytes = New-IntegerSchema
    max_audio_total_bytes = New-IntegerSchema
    max_bgm_total_bytes = New-IntegerSchema
    max_preview_assets_bytes = New-IntegerSchema
    max_single_asset_bytes = New-IntegerSchema
    max_single_asset_width_px = New-IntegerSchema
    max_single_asset_height_px = New-IntegerSchema
    max_single_asset_decoded_memory_bytes = New-IntegerSchema
    max_gpu_texture_memory_bytes = New-IntegerSchema
    max_cache_bytes_per_theme = New-IntegerSchema
    max_cache_bytes_global = New-IntegerSchema
})
Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeAssets.schema.json') -Value (New-RootSchema -Name 'ThemeAssets' -Required @('schema_version','theme_id','version','assets','asset_budgets') -Properties ([ordered]@{
    schema_version = $schemaVersion
    theme_id = $themeId
    version = $semverString
    assets = New-ArraySchema -Items (New-ObjectSchema -Required @('asset_id','path','type','tier','variant_scope','surface_scope','required','fallback_asset_id','sha256','size_bytes','dpi_policy','load_policy','dispose_policy') -Properties ([ordered]@{
        asset_id = $nonEmptyString
        path = New-StringSchema -Pattern '^(?![A-Za-z]:|/|\\|.*\.\.[/\\]).+$'
        type = New-StringSchema -Enum @('image','font','audio','vector','animation','data')
        tier = New-StringSchema -Enum @('tier0','tier1','tier2','tier3','audio')
        variant_scope = $stringArray
        surface_scope = New-ArraySchema -Items $surfaceId -UniqueItems $true
        required = New-BooleanSchema
        fallback_asset_id = $nonEmptyString
        sha256 = New-StringSchema -Pattern '^[A-Fa-f0-9]{64}$'
        size_bytes = New-IntegerSchema
        dimensions = New-ObjectSchema -Required @('width','height') -Properties ([ordered]@{ width = New-IntegerSchema -Minimum 1; height = New-IntegerSchema -Minimum 1 })
        dpi_policy = $nonEmptyString
        load_policy = New-StringSchema -Enum @('startup','immediate','lazy','opportunistic','preview_only')
        dispose_policy = $nonEmptyString
    }))
    asset_budgets = $assetBudgets
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeTokens.schema.json') -Value (New-RootSchema -Name 'ThemeTokens' -Required @('schema_version','tokens','semantic_bindings') -Properties ([ordered]@{
    schema_version = $schemaVersion
    tokens = New-ObjectSchema -Properties ([ordered]@{}) -AdditionalProperties (New-ObjectSchema -Required @('type','value') -Properties ([ordered]@{
        type = New-StringSchema -Enum @('color','duration','number','string','font','dimension','opacity','boolean')
        value = [ordered]@{}
    }))
    semantic_bindings = New-ObjectSchema -Properties ([ordered]@{}) -AdditionalProperties (New-ObjectSchema -Required @('token','requires_text_label','requires_icon_or_structure') -Properties ([ordered]@{
        token = $nonEmptyString
        requires_text_label = New-BooleanSchema
        requires_icon_or_structure = New-BooleanSchema
    }))
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeLayoutAdapter.schema.json') -Value (New-RootSchema -Name 'ThemeLayoutAdapter' -Required @('schema_version','surface_id','zones','keyboard_and_focus') -Properties ([ordered]@{
    schema_version = $schemaVersion
    surface_id = $surfaceId
    zones = New-ArraySchema -MinItems 1 -Items (New-ObjectSchema -Required @('zone_id','placement','visibility','rearrangeability','collapse_allowed') -Properties ([ordered]@{
        zone_id = $zoneId
        placement = $nonEmptyString
        visibility = $nonEmptyString
        rearrangeability = $nonEmptyString
        collapse_allowed = New-BooleanSchema
    }))
    keyboard_and_focus = $keyboardAndFocus
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeComponentAdapter.schema.json') -Value (New-RootSchema -Name 'ThemeComponentAdapter' -Required @('schema_version','component_id','supported_surface_ids','preserved_semantics','keyboard_and_focus') -Properties ([ordered]@{
    schema_version = $schemaVersion
    component_id = $nonEmptyString
    supported_surface_ids = New-ArraySchema -Items $surfaceId -MinItems 1 -UniqueItems $true
    preserved_semantics = $requiredStringArray
    keyboard_and_focus = $keyboardAndFocus
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeCopyResources.schema.json') -Value (New-RootSchema -Name 'ThemeCopyResources' -Required @('schema_version','locale','non_critical_resources','approved_critical_fallbacks') -Properties ([ordered]@{
    schema_version = $schemaVersion
    locale = New-StringSchema -Pattern '^[A-Za-z]{2,3}(?:-[A-Za-z0-9]{2,8})*$'
    non_critical_resources = New-ObjectSchema -Properties ([ordered]@{}) -AdditionalProperties $nonEmptyString
    approved_critical_fallbacks = New-ObjectSchema -Properties ([ordered]@{}) -AdditionalProperties $nonEmptyString
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeCriticalCopyRules.schema.json') -Value (New-RootSchema -Name 'ThemeCriticalCopyRules' -Required @('schema_version','core_owned_keys','plain_language_required_categories','reject_ambiguous_critical_copy','reject_missing_critical_fallback') -Properties ([ordered]@{
    schema_version = $schemaVersion
    core_owned_keys = $requiredStringArray
    plain_language_required_categories = $requiredStringArray
    reject_ambiguous_critical_copy = [ordered]@{ const = $true }
    reject_missing_critical_fallback = [ordered]@{ const = $true }
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeFocusStyles.schema.json') -Value (New-RootSchema -Name 'ThemeFocusStyles' -Required @('schema_version','minimum_contrast_ratio','visible_on_all_interactive_controls','programmatic_order_preserved','modal_focus_trap_preserved','focus_restoration_required') -Properties ([ordered]@{
    schema_version = $schemaVersion
    minimum_contrast_ratio = New-NumberSchema -Minimum 3.0
    visible_on_all_interactive_controls = [ordered]@{ const = $true }
    programmatic_order_preserved = [ordered]@{ const = $true }
    modal_focus_trap_preserved = [ordered]@{ const = $true }
    focus_restoration_required = [ordered]@{ const = $true }
}))

$requiredSemantics = New-ObjectSchema -Required @('meaning','must_show_reason','must_show_exit_or_resolution_path','may_offer_corrective_action') -Properties ([ordered]@{
    meaning = $nonEmptyString
    must_show_reason = New-BooleanSchema
    must_show_exit_or_resolution_path = New-BooleanSchema
    may_offer_corrective_action = New-BooleanSchema
})
$requiredRedundancy = New-ObjectSchema -Required @('text','icon_or_structure','programmatic_state','screen_reader_announcement','color_only_forbidden','motion_only_forbidden','sound_only_forbidden','transparency_only_forbidden') -Properties ([ordered]@{
    text = [ordered]@{ const = $true }
    icon_or_structure = [ordered]@{ const = $true }
    programmatic_state = [ordered]@{ const = $true }
    screen_reader_announcement = [ordered]@{ const = $true }
    color_only_forbidden = [ordered]@{ const = $true }
    motion_only_forbidden = [ordered]@{ const = $true }
    sound_only_forbidden = [ordered]@{ const = $true }
    transparency_only_forbidden = [ordered]@{ const = $true }
})
$stateFreedom = New-ObjectSchema -Required @('color','animation','sound','metaphor') -Properties ([ordered]@{
    color = $nonEmptyString
    animation = [ordered]@{ const = 'supplementary_only' }
    sound = [ordered]@{ const = 'supplementary_only' }
    metaphor = $nonEmptyString
})

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeStatePresentation.schema.json') -Value (New-RootSchema -Name 'ThemeStatePresentation' -Required @('schema_version','state_id','required_semantics','required_redundancy','theme_freedom') -Properties ([ordered]@{
    schema_version = $schemaVersion
    state_id = $globalStateId
    required_semantics = $requiredSemantics
    required_redundancy = $requiredRedundancy
    theme_freedom = $stateFreedom
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeMotionProfile.schema.json') -Value (New-RootSchema -Name 'ThemeMotionProfile' -Required @('schema_version','profiles','reduced_motion') -Properties ([ordered]@{
    schema_version = $schemaVersion
    profiles = New-ArraySchema -MinItems 1 -Items (New-ObjectSchema -Required @('profile_id','intensity','transitions_interruptible','transitions_skippable','parallax_enabled','parallax_reduced_fallback','critical_alert_motion_supplementary_only','loss_celebration_forbidden') -Properties ([ordered]@{
        profile_id = $nonEmptyString
        intensity = New-StringSchema -Enum @('none','subtle','moderate','rich')
        transitions_interruptible = [ordered]@{ const = $true }
        transitions_skippable = [ordered]@{ const = $true }
        parallax_enabled = New-BooleanSchema
        parallax_reduced_fallback = [ordered]@{ const = 'static' }
        critical_alert_motion_supplementary_only = [ordered]@{ const = $true }
        loss_celebration_forbidden = [ordered]@{ const = $true }
    }))
    reduced_motion = New-ObjectSchema -Required @('disable_nonessential_motion','replace_transitions_with','preserve_state_textually','disable_parallax','disable_looping_decorative_motion') -Properties ([ordered]@{
        disable_nonessential_motion = [ordered]@{ const = $true }
        replace_transitions_with = New-StringSchema -Enum @('instant','short_fade','instant_or_short_fade')
        preserve_state_textually = [ordered]@{ const = $true }
        disable_parallax = [ordered]@{ const = $true }
        disable_looping_decorative_motion = [ordered]@{ const = $true }
    })
}))

$soundControls = New-ObjectSchema -Required @('one_click_disable','independent_bgm','independent_ambient','independent_ui','independent_alert_supplement','night_mode','ducking') -Properties ([ordered]@{
    one_click_disable = [ordered]@{ const = $true }
    independent_bgm = [ordered]@{ const = $true }
    independent_ambient = [ordered]@{ const = $true }
    independent_ui = [ordered]@{ const = $true }
    independent_alert_supplement = [ordered]@{ const = $true }
    night_mode = [ordered]@{ const = $true }
    ducking = [ordered]@{ const = $true }
})
Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeSoundPack.schema.json') -Value (New-RootSchema -Name 'ThemeSoundPack' -Required @('schema_version','sound_pack_id','version','controls','events','bgm','ambient') -Properties ([ordered]@{
    schema_version = $schemaVersion
    sound_pack_id = $themeId
    version = $semverString
    controls = $soundControls
    events = New-ArraySchema -Items (New-ObjectSchema -Required @('event_id','asset_id','priority','supplementary_only','duck_bgm','respects_sound_disabled','requires_visual_and_text_alert','celebratory') -Properties ([ordered]@{
        event_id = $nonEmptyString
        asset_id = $nonEmptyString
        priority = New-StringSchema -Enum @('critical','high','medium','low')
        supplementary_only = [ordered]@{ const = $true }
        duck_bgm = New-BooleanSchema
        respects_sound_disabled = [ordered]@{ const = $true }
        requires_visual_and_text_alert = New-BooleanSchema
        celebratory = New-BooleanSchema
    }))
    bgm = New-ArraySchema -Items (New-ObjectSchema -Required @('track_id','path','loop','contextual','respects_one_click_disable','respects_night_mode') -Properties ([ordered]@{
        track_id = $nonEmptyString
        path = $nonEmptyString
        loop = New-BooleanSchema
        contextual = New-BooleanSchema
        respects_one_click_disable = [ordered]@{ const = $true }
        respects_night_mode = [ordered]@{ const = $true }
    }))
    ambient = New-ArraySchema -Items (New-ObjectSchema -Required @('ambient_id','path','loop','contextual','surface_scope','respects_one_click_disable','respects_night_mode','duckable','default_volume') -Properties ([ordered]@{
        ambient_id = $nonEmptyString
        path = $nonEmptyString
        loop = New-BooleanSchema
        contextual = New-BooleanSchema
        surface_scope = New-ArraySchema -Items $surfaceId -UniqueItems $true
        respects_one_click_disable = [ordered]@{ const = $true }
        respects_night_mode = [ordered]@{ const = $true }
        duckable = [ordered]@{ const = $true }
        default_volume = New-NumberSchema -Minimum 0 -Maximum 1
    }))
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemePersonalizationSafeRanges.schema.json') -Value (New-RootSchema -Name 'ThemePersonalizationSafeRanges' -Required @('schema_version','theme_id','controls') -Properties ([ordered]@{
    schema_version = $schemaVersion
    theme_id = $themeId
    controls = New-ArraySchema -MinItems 1 -UniqueItems $true -Items (New-ObjectSchema -Required @('control_id','display_name','type','default','accessibility_overrides','cannot_affect') -Properties ([ordered]@{
        control_id = $nonEmptyString
        display_name = $nonEmptyString
        type = New-StringSchema -Enum @('number','boolean','enum','color','string')
        default = [ordered]@{}
        min = New-NumberSchema
        max = New-NumberSchema
        step = New-NumberSchema
        accessibility_overrides = New-ObjectSchema -Properties ([ordered]@{}) -AdditionalProperties ([ordered]@{})
        cannot_affect = New-ArraySchema -Items (New-StringSchema -Enum @('risk_semantics','permission_semantics','audit_semantics','recovery_semantics','critical_alert_visibility','home_safety_core_visibility','confirmation_semantics','keyboard_focus_visibility','screen_reader_labels','accessibility_availability','connector_scope','gpt_optionality','authoritative_data')) -MinItems 1 -UniqueItems $true
    }))
}))

$audioState = New-ObjectSchema -Required @('master_theme_audio_enabled','master_theme_audio_volume','bgm_enabled','bgm_volume','ambient_enabled','ambient_volume','ui_sound_enabled','ui_sound_volume','alert_supplement_enabled','alert_supplement_volume','night_mode','ducking_enabled') -Properties ([ordered]@{
    master_theme_audio_enabled = New-BooleanSchema
    master_theme_audio_volume = New-NumberSchema -Minimum 0 -Maximum 1
    bgm_enabled = New-BooleanSchema
    bgm_volume = New-NumberSchema -Minimum 0 -Maximum 1
    ambient_enabled = New-BooleanSchema
    ambient_volume = New-NumberSchema -Minimum 0 -Maximum 1
    ui_sound_enabled = New-BooleanSchema
    ui_sound_volume = New-NumberSchema -Minimum 0 -Maximum 1
    alert_supplement_enabled = New-BooleanSchema
    alert_supplement_volume = New-NumberSchema -Minimum 0 -Maximum 1
    night_mode = New-BooleanSchema
    ducking_enabled = New-BooleanSchema
})
Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeRuntimeState.schema.json') -Value (New-RootSchema -Name 'ThemeRuntimeState' -Required @('schema_version','state_id','user_id','device_id','theme_id','theme_version','variant_id','personalization','audio','motion','automation','updated_at') -Properties ([ordered]@{
    schema_version = $schemaVersion
    state_id = $nonEmptyString
    user_id = $nonEmptyString
    device_id = $nonEmptyString
    theme_id = $themeId
    theme_version = $semverString
    variant_id = $nonEmptyString
    workspace_state = New-ObjectSchema -Required @('workspace_id','layout_ref','monitor_binding','window_bounds') -Properties ([ordered]@{
        workspace_id = $workspaceId
        layout_ref = $nonEmptyString
        monitor_binding = $nonEmptyString
        window_bounds = New-ObjectSchema -Required @('x','y','width','height','dpi_scale') -Properties ([ordered]@{
            x = [ordered]@{ type = 'number' }
            y = [ordered]@{ type = 'number' }
            width = New-NumberSchema -Minimum 1
            height = New-NumberSchema -Minimum 1
            dpi_scale = New-NumberSchema -Minimum 1
        })
    })
    personalization = New-ObjectSchema -Required @('preset_id','values') -Properties ([ordered]@{
        preset_id = $nonEmptyString
        values = New-ObjectSchema -Properties ([ordered]@{}) -AdditionalProperties ([ordered]@{})
    })
    audio = $audioState
    motion = New-ObjectSchema -Required @('intensity','parallax_enabled') -Properties ([ordered]@{
        intensity = New-NumberSchema -Minimum 0 -Maximum 1
        parallax_enabled = New-BooleanSchema
    })
    automation = New-ObjectSchema -Required @('time_of_day_scene_enabled','trading_state_overlay_enabled','automation_pinned_or_disabled') -Properties ([ordered]@{
        time_of_day_scene_enabled = New-BooleanSchema
        trading_state_overlay_enabled = New-BooleanSchema
        automation_pinned_or_disabled = New-StringSchema -Enum @('default','pinned','disabled')
    })
    updated_at = [ordered]@{ type = 'string'; format = 'date-time' }
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'UxSurfaceContract.schema.json') -Value (New-RootSchema -Name 'UxSurfaceContract' -Required @('schema_version','ux_contract_version','surface_id','surface_type','surface_name','required_theme_contract','supported_states','invariants','accessibility_requirements','safety_constraints','theme_freedom','required_theme_tests','provenance') -Properties ([ordered]@{
    '$schema' = $nonEmptyString
    schema_version = $schemaVersion
    ux_contract_version = [ordered]@{ const = '1.1.0' }
    surface_id = $surfaceId
    surface_type = [ordered]@{ const = 'page' }
    surface_name = $nonEmptyString
    required_theme_contract = $nonEmptyString
    supported_states = New-ArraySchema -Items $globalStateId -MinItems 1 -UniqueItems $true
    invariants = $requiredStringArray
    accessibility_requirements = New-ObjectSchema -Required @('keyboard','screen_reader_labels','screen_reader_announcements','focus_visible','full_keyboard_operation','text_scaling','zoom','contrast','color_vision','reduced_motion','reduced_transparency','sound_controls','critical_alert_redundancy') -Properties ([ordered]@{
        keyboard = [ordered]@{ const = $true }
        screen_reader_labels = [ordered]@{ const = $true }
        screen_reader_announcements = [ordered]@{ const = $true }
        focus_visible = [ordered]@{ const = $true }
        full_keyboard_operation = [ordered]@{ const = $true }
        text_scaling = [ordered]@{ const = $true }
        zoom = [ordered]@{ const = $true }
        contrast = [ordered]@{ const = $true }
        color_vision = [ordered]@{ const = $true }
        reduced_motion = [ordered]@{ const = $true }
        reduced_transparency = [ordered]@{ const = $true }
        sound_controls = [ordered]@{ const = $true }
        critical_alert_redundancy = [ordered]@{ const = $true }
    })
    safety_constraints = $requiredStringArray
    theme_freedom = $nonEmptyString
    required_theme_tests = $requiredStringArray
    provenance = $requiredStringArray
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'FunctionalZoneContract.schema.json') -Value (New-RootSchema -Name 'FunctionalZoneContract' -Required @('schema_version','zone_id','zone_name','semantics','priority','rearrangeability','required_visibility','may_be_collapsed','semantic_children','theme_may_change','theme_must_not_change','provenance') -Properties ([ordered]@{
    '$schema' = $nonEmptyString
    schema_version = $schemaVersion
    zone_id = $zoneId
    zone_name = $nonEmptyString
    semantics = $nonEmptyString
    priority = $nonEmptyString
    rearrangeability = $nonEmptyString
    required_visibility = $nonEmptyString
    may_be_collapsed = New-BooleanSchema
    semantic_children = $stringArray
    theme_may_change = $requiredStringArray
    theme_must_not_change = $requiredStringArray
    provenance = $requiredStringArray
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'GlobalStatePresentation.schema.json') -Value (New-RootSchema -Name 'GlobalStatePresentation' -Required @('schema_version','state_id','required_semantics','required_redundancy','theme_freedom','provenance') -Properties ([ordered]@{
    '$schema' = $nonEmptyString
    schema_version = $schemaVersion
    state_id = $globalStateId
    required_semantics = $requiredSemantics
    required_redundancy = $requiredRedundancy
    theme_freedom = $stateFreedom
    provenance = $requiredStringArray
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'ThemeDiagnosticsEvent.schema.json') -Value (New-RootSchema -Name 'ThemeDiagnosticsEvent' -Required @('schema_version','event_id','event_type','theme_id','theme_version','severity','message','correlation_id','timestamp','privacy') -Properties ([ordered]@{
    schema_version = $schemaVersion
    event_id = $nonEmptyString
    event_type = New-StringSchema -Pattern '^theme\.'
    theme_id = $themeId
    theme_version = $semverString
    variant_id = $nonEmptyString
    surface_id = $surfaceId
    severity = New-StringSchema -Enum @('information','warning','error','critical')
    message = $nonEmptyString
    asset_id = $nonEmptyString
    correlation_id = $nonEmptyString
    timestamp = [ordered]@{ type = 'string'; format = 'date-time' }
    privacy = New-ObjectSchema -Required @('contains_trading_data','contains_private_data','contains_team_data') -Properties ([ordered]@{
        contains_trading_data = [ordered]@{ const = $false }
        contains_private_data = [ordered]@{ const = $false }
        contains_team_data = [ordered]@{ const = $false }
    })
}))

Write-JsonFile -Path (Join-Path $schemaRoot 'CopyFallbackResult.schema.json') -Value (New-RootSchema -Name 'CopyFallbackResult' -Required @('schema_version','key','locale','value','source','is_critical','used_core_fallback','is_valid') -Properties ([ordered]@{
    schema_version = $schemaVersion
    key = $nonEmptyString
    locale = New-StringSchema -Pattern '^[A-Za-z]{2,3}(?:-[A-Za-z0-9]{2,8})*$'
    value = $nonEmptyString
    source = New-StringSchema -Enum @('theme_noncritical','theme_approved_critical','core_fallback')
    is_critical = New-BooleanSchema
    used_core_fallback = New-BooleanSchema
    is_valid = [ordered]@{ const = $true }
}))

Copy-Item -Force -LiteralPath (Join-Path $schemaRoot 'ThemeManifest.schema.json') -Destination (Join-Path $contractRoot 'theme-manifest.schema.json')

$requiredSchemaFiles = @(
    'ThemeManifest.schema.json','ThemeIntegrity.schema.json','ThemeCompatibility.schema.json',
    'ThemeRollback.schema.json','ThemeAssets.schema.json','ThemeTokens.schema.json',
    'ThemeLayoutAdapter.schema.json','ThemeComponentAdapter.schema.json','ThemeCopyResources.schema.json',
    'ThemeCriticalCopyRules.schema.json','ThemeFocusStyles.schema.json','ThemeStatePresentation.schema.json',
    'ThemeMotionProfile.schema.json','ThemeSoundPack.schema.json','ThemePersonalizationSafeRanges.schema.json',
    'ThemeRuntimeState.schema.json','UxSurfaceContract.schema.json','FunctionalZoneContract.schema.json',
    'GlobalStatePresentation.schema.json','ThemeDiagnosticsEvent.schema.json','CopyFallbackResult.schema.json'
)

$actualSchemaFiles = @(Get-ChildItem -LiteralPath $schemaRoot -Filter '*.schema.json' -File | Select-Object -ExpandProperty Name | Sort-Object)
$missingSchemaFiles = @($requiredSchemaFiles | Where-Object { $_ -notin $actualSchemaFiles })
if ($missingSchemaFiles.Count -gt 0) {
    throw "Missing generated schema files: $($missingSchemaFiles -join ', ')"
}

$allStableIds = @($domains.id + $workspaces.id + $modules.id + $zones.id + $states.id + $flows.id + $pages.id)
$duplicateIds = @($allStableIds | Group-Object | Where-Object Count -gt 1 | Select-Object -ExpandProperty Name)
if ($duplicateIds.Count -gt 0) {
    throw "Duplicate stable UX IDs: $($duplicateIds -join ', ')"
}

Write-Output "Generated $($requiredSchemaFiles.Count) schemas and $($pages.Count) page contracts under contracts/theme."
