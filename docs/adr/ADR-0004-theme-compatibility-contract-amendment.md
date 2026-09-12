# ADR-0004: Theme Compatibility Contract Amendment

**Status:** Authorized design / UNSEALED CANDIDATE
**Decision date:** 2026-09-10
**Authority:** TCC-P5B-66, approved TCC-P5B-65 and supplemental S1–S9; Decision019
**Scope:** Candidate A contracts only

## Context and decision

Ordinary caller-constructible result DTOs, IsVerified flags and matching hash/identity strings cannot represent construction provenance. Approve the narrow additive ownership exception: Tcc.Themes owns IThemeCompatibilityResolverV2, ThemeCompatibilityRequestV2, ThemeCompatibilityContextV2 and ThemeCompatibilityEvidenceV2 in Tcc.Themes.Compatibility.V2. Stable environment, target, result, failure, notice and enum contracts remain in Tcc.Presentation.Contracts.Theme.

Dependency remains **Tcc.Themes → Tcc.Presentation.Contracts**. No reverse dependency, new friend assembly, project, package or reference. Earlier ADRs and all V1 contracts/serialization remain unchanged. DesktopHost remains the sole composition root.

## Trust and composition

Context/evidence are sealed non-record classes with internal construction, internal get-only trusted state, and no public mutation/import/clone/factory. Ordinary external API consumers cannot manufacture them. Tcc.Themes and the existing test friend are trusted code; reflection, unsafe memory and assembly replacement are outside the supported external-caller threat model.

Future sole production context issuer is the internal trusted binder. It owns one immutable content image, calls the sealed V2 verifier once, materializes both DTOs from the same verified bytes and checks real file evidence, hashes, lengths and identities. It accepts all legitimately verified channels, including authorized unsigned cases. It reuses PackageHash and does not duplicate integrity cryptography. The additive Compatibility materializer will use the unchanged schema; its future EmbeddedResource belongs to separately authorized Candidate B. Context retains immutable evidence, not raw bytes or ongoing filesystem custody. Changed content requires re-verification by its owner.

Six internal owner receipts explicitly bind Context and Environment. Runtime, capability, accessibility, Safety, migration and rollback owners produce them; the future resolver only composes them. Missing evidence prevents success. Q93 declarations, file hashes and tested DPI coverage are not runtime validation. MigrationRequired/Ready and RollbackRequired/Available remain independent nullable facts, with unknown and inapplicable semantics defined in Decision019.

## Version, outcome and determinism

ThemeCompatibilityContractV2="2.0" identifies only the new runtime/result contract, not any package schema or Theme API upgrade. All existing constants remain unchanged. Four selected versions are nullable; no empty string, unknown or fake version fills an absent selection. The sealed Phase5A algorithm is preserved and remains the sole future version negotiation owner; generic failures do not invent source dimensions. Original failures and occurrences retain their order.

Failure precedence: structural preconditions, integrity/binding, schema, sealed Phase5A declaration/Core/API/UX sequence, platform/mode, runtime, accessibility, Safety, capability, migration, rollback. Precondition/integrity failures refuse; schema failures stop schema-dependent interpretation. Other independent valid dimensions may collect failures. The full fixed enum/status mapping and owner semantics in Decision019 are normative here.

Schema support authority remains {"1.0"}, with canonical ASCII major.minor support tokens; caller claims cannot widen it. Theme API {"1.0.0"} and UX {"1.1.0"} remain sealed. Windows/x64 and Installer/Portable parity remain. Untested DPI needs positively validated scalable fallback. All consumed collections are eagerly copied to immutable snapshots; acquisition requires stable caller sources. No ambient state, cache, registry, clock or RNG supplies facts.

## Serialization

Only SerializeResult/DeserializeResult transport canonical snake_case result data. Property order and enum strings are fixed; nullable fields serialize explicitly. Require version "2.0", every field, no unknown/duplicate properties, no numeric/unknown enums, valid ordered failures/notices, legal nullable facts and consistent status. Deserialization rejects contradictions without repair. Data import grants no trusted context/evidence or installation/activation authority. V1 ThemeContractJson is unchanged.

## Supplemental contract closure S1–S6

The explicit later Supervisor supplement to TCC-P5B-66 closes the five missing definitions without creating Decision020.

- `ThemeCompatibilityInstallationModeV2`: `NotSpecified=0`, `Installer=1`, `Portable=2`; wire strings `not_specified`, `installer`, `portable`. No additional members. NotSpecified is invalid for actual evaluation; Installer and Portable share all mandatory requirements.
- `ThemeCompatibilityOperationV2`: `NotSpecified=0`, `PackageEvaluation=1`, `TransitionEvaluation=2`; wire strings `not_specified`, `package_evaluation`, `transition_evaluation`. No additional members. PackageEvaluation requires CurrentThemeId, CurrentThemeVersion and ThemeStateRevision all null. At DTO construction, source identity/version are both present or both absent. Valid TransitionEvaluation composition requires both present; revision remains nullable and the later owner must refuse if applicable state requires it. Operation grants no execution authority.
- `ThemeCompatibilityNoticeKindV2`: `NotSpecified=0`, `OptionalCapabilityDisabled=1`, `DecorativePresentationDegradation=2`, `UntestedDpiScalableFallback=3`; wire strings `not_specified`, `optional_capability_disabled`, `decorative_presentation_degradation`, `untested_dpi_scalable_fallback`. NotSpecified is invalid in a final result. OptionalCapabilityDisabled requires Capability dimension; DecorativePresentationDegradation allows Runtime or Capability; UntestedDpiScalableFallback requires Runtime.
- All enum values are explicit and sequential in the listed order. Numeric and unknown enum input is rejected by canonical transport. Descriptive environment enum transport uses these exact strings with strict string-enum settings; the canonical V2 API transports results only.

Exact notice shape, public sealed record, five public get-only properties:

```csharp
public ThemeCompatibilityNoticeV2(
    ThemeCompatibilityNoticeKindV2 kind,
    ThemeCompatibilityDimensionV2 dimension,
    int sequence,
    string? diagnosticCode,
    string message)
```

Properties and wire order: `Kind/kind`, `Dimension/dimension`, `Sequence/sequence`, `DiagnosticCode/diagnostic_code`, `Message/message`.
Kind must not be NotSpecified; Sequence must be nonnegative; Message must be non-null and non-empty; DiagnosticCode must be null in Candidate A. No caller text, raw JSON, exception, version token, secret, credential, filesystem path or machine data may enter Message. Contract implementation fixes the detail template to `{Kind} in {Dimension}.` using defined enum names only, for both failures and notices. This selects the fixed descriptive template required by the approved design without adding diagnostic codes or policy ownership.

Evidence bundle: public sealed non-record `ThemeCompatibilityEvidenceV2`, with exactly eight internal get-only properties and this sole internal constructor:

```csharp
internal ThemeCompatibilityEvidenceV2(
    ThemeCompatibilityContextV2 context,
    ThemeCompatibilityEnvironmentV2 environment,
    ThemeCompatibilityRuntimeEvidenceV2? runtime,
    ThemeCompatibilityCapabilityEvidenceV2? capability,
    ThemeCompatibilityAccessibilityEvidenceV2? accessibility,
    ThemeCompatibilitySafetyEvidenceV2? safety,
    ThemeCompatibilityMigrationEvidenceV2? migration,
    ThemeCompatibilityRollbackEvidenceV2? rollback)
```

Properties: `Context`, `Environment`, `Runtime`, `Capability`, `Accessibility`, `Safety`, `Migration`, `Rollback`, matching these parameter types and nullabilities exactly. No additional public data properties, token, nonce or registry key. Null context/environment throw ArgumentNullException. All six slots may be absent. Each present receipt must reference the exact same context and environment; all mismatches consistently throw ArgumentException. Never drop, rebind or manufacture receipts. Future Resolve checks request/bundle reference identity and returns RefusedPrecondition/ContextBindingMismatch; Candidate A does not implement that resolver behavior.

No public/protected constructor, factory, clone/with, mutable setter/init, FromResult, FromDto, import or deserialize path exists for context/evidence. Canonical serializer has only SerializeResult and DeserializeResult. A result is descriptive data only, never a portable proof or activation/installation authority.

Notice and failure arrays independently use contiguous Sequence 0..N-1 in supplied order. Never reorder or repair deserialized input. Compatible requires no failures and no notices. CompatibleWithDegradation requires no failures, one or more notices and no UntestedDpiScalableFallback notice. CompatibleUntestedDpiWithScalableFallback requires no failures and at least one such notice, and may also carry ordinary degradation. All three require complete successful mandatory evidence. Notices cannot rescue failure. Q93, Home Safety Core, critical alerts, risk/permission clarity, confirmation semantics and required accessibility are never degraded.

## Current authorization and phase gates

Only the ten Candidate A files explicitly listed by TCC-P5B-66 are authorized. Candidate A defines data shapes, immutable snapshots, internal construction and strict result transport only. V1 resolver implementations=0 and Candidate A V2 resolver implementations=0. Future Candidate B resolver implementations must equal exactly one after separate authorization. The binder, content snapshot reader, compatibility materializer, evidence producers, lifecycle issuance, DI registration and resolver remain absent. All binding/materialization/owner behavior described above is normative future work, not a claim of implemented or validated runtime trust.

No changes to Phase5A production or functional tests, V1 source/semantics, package schemas, csproj/lock files, project/package references, friend assemblies or earlier ADRs. No Trading, Market Data, AI, execution connector or business policy ownership. No filesystem, reader, crypto, lifecycle or raw-content authority enters Resolve. The existing six-project/eleven-ProjectReference/four-PackageReference graph and Frozen sources remain unchanged.

Required gates: focused contract/scope/Phase2/Phase5A/dependency tests, normal and locked restore with NuGet audit, single-node Release x64 build, fresh full tests with zero failures/skips, Frozen/sealed preservation, exact scope, Git hygiene and truthful status synchronization. Implementation self-validation does not grant independent approval, resolver authorization or seal. No stage/commit/tag/push is authorized.
