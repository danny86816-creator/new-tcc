# Codex Decision Ledger

This file records explicit human decisions that constrain future Codex construction. It is a ledger, not a substitute for the approved source artifacts or ADRs.

## Accepted decisions

| ID | Date | Decision | Implementation effect |
|---|---|---|---|
| `TCC-DEC-2026-09-07-001` | 2026-09-07 | `TCC Theme Architecture v1.2 — APPROVED.md`, with SHA-256 `960CCCB69AFDBE006CA12BECB86A1E3E6C96C6C59500CE943F176162D2B33856`, is the formal approved Theme baseline. | Candidate wording in the body is historical and does not revoke external approval. The source file remains immutable. |
| `TCC-DEC-2026-09-07-002` | 2026-09-07 | Use C#/.NET 10 LTS, WPF, MVVM, Generic Host/DI, SQLite/EF Core, xUnit, Windows x64, Installer, and self-contained Portable delivery. | Establishes the repository and build platform. See ADR-0001. |
| `TCC-DEC-2026-09-07-003` | 2026-09-07 | Approve creation of `Tcc.Features.Themes` with presentation-only Theme management ownership. | Establishes the feature package and its prohibited dependencies. See ADR-0002. |
| `TCC-DEC-2026-09-07-004` | 2026-09-07 | Execute Phase 1 only and stop after build and baseline tests. | Phase 2–8 implementation remains prohibited until separately approved. |
| `TCC-DEC-2026-09-07-005` | 2026-09-07 | Phase 2 implementation is authorized on `phase2-foundation` and is defined as Theme Architecture §33.1 steps 1–3 plus all §33.2 schemas/contracts and all §33.3 public interfaces. | Production runtime is deferred; existing `Tcc.Architecture.Tests` is used; commit and Phase 3 remain prohibited. See `ADR-0002-phase2-schemas-public-contracts-scope.md`. |
| `TCC-DEC-2026-09-07-006` | 2026-09-07 | Phase 2 stable-ID schemas fail closed from the approved registries, while the explicitly approved `WS-CUSTOM-*` workspace family remains patterned; Theme API compatibility is fixed to `1.0.0` until a later approved range exists. | Generator-owned enums/patterns and actual-schema negative tests are the contract boundary; DTO wire names and optionality must remain mechanically consistent with the Frozen JSON/type semantics. Decision source: explicit Phase 2 surgical remediation requirement. |
| `TCC-DEC-2026-09-07-007` | 2026-09-07 | Adopt AI Trading Intelligence Baseline v1 as the latest domain-specific planning authority for future Trading Intelligence, with deterministic engine, advisory AI, and human-governance authority kept separate. | Establishes the AI-A–AI-F Architecture Amendment Track as roadmap only; keeps GPT optional, forbids AI authority over risk/permission/hard gates/execution, preserves read-only connectors, and leaves Phase 3 Theme scope unchanged. Decision source: explicit 05A user requirement. |
| `TCC-DEC-2026-09-07-008` | 2026-09-07 | Supersede the historical future Trading Risk Policy values `Total Risk = 2%` and `Maximum Positions = 2` with target single-trade risk 1.0%, absolute single-trade hard cap 1.2%, maximum aggregate position risk 3.0%, maximum simultaneous positions 3, daily-loss block at -3%, two-loss warning, three-loss entry block, and a required consecutive-loss protection master toggle. | Supersession applies only to future Trading Risk Policy authority; historical requirements remain recorded and no Risk engine/runtime implementation is authorized. Decision source: explicit 05A user requirement. |
| `TCC-DEC-2026-09-07-009` | 2026-09-07 | Approve the Phase 4 Theme Integrity Contract Amendment: additive V2 evidence contracts, exhaustive inventory, TCC Package Canonical Path v1, Canonical Package Tree Hash v1, application-managed signer registry with immutable caller trust snapshot, explicit channel policy, and ECDSA P-256/SHA-256 using IEEE P1363 signatures and SPKI public keys. | Sealed V1 contracts/schemas and Phase 2/3 tags remain unchanged; contract/schema/tests only are authorized. Phase 4 production implementation stays locked pending independent amendment audit and later explicit authorization. See `ADR-0003-theme-integrity-contract-amendment.md`. |
| `TCC-DEC-2026-09-08-010` | 2026-09-08 | `ThemePackageRef` is an opaque verification input source/context and audit-provenance handle, not publisher-signed content identity. It must not be included in the canonical signature payload or make identical content cryptographically location-dependent. | Phase 4 signatures remain bound only to Theme ID/version, package hash, Theme Manifest hash, Integrity Manifest hash, publisher ID, and key ID. Decision source: explicit Phase 4 Contract Amendment surgical-remediation instruction. |

## Standing boundaries

- Do not edit or overwrite approved governance artifacts.
- Do not copy core implementation from legacy TCC.
- Do not implement Phase 2–8 early.
- Do not place Theme metadata or Theme behavior in `Tcc.Domain`.
- Do not let `Tcc.Features.Themes` own Theme runtime, persistence, security validation, or Recovery Core.
- Preserve `Tcc.DesktopHost` as the composition root.
- Preserve the deterministic-engine / advisory-AI / human-governance authority separation defined by `docs/governance/AI_TRADING_INTELLIGENCE_BASELINE_V1.md`.
- Keep the Theme Runtime Track and Trading Intelligence Track separate; Trading/AI dependencies and contracts cannot enter Theme Manifest Validator or the Theme contract family.
- Treat Prepared Orders as non-executed artifacts. Broker/exchange write or execution integration requires a separately approved architecture amendment and explicit implementation authorization.

## Decisions still open after Phase 1

The Theme Architecture's enumerated open technical decisions remain open unless explicitly covered by ADR-0001 or ADR-0002. In particular, Phase 1 does not select the Theme archive format, signature authority, asset/audio codec policy, cache thresholds, visual regression tooling, Theme store protocol, quarantine design, installer builder, MVVM helper library, or SQLite protection mechanism.

## TCC-DEC-2026-09-08-011 — Phase 4 V2-only production runtime

- Date: 2026-09-08.
- Decision source: explicit human-approved GPT Supervisor decision in task 17, Phase 4 Theme Integrity Verifier Compatibility Resolution / Branch Setup.
- Decision: future Phase 4 production `Tcc.Themes.Integrity.ThemeIntegrityVerifier` in `Tcc.Themes` implements `IThemeIntegrityVerifierV2` only. It does not implement `IThemeIntegrityVerifier`; no dual-interface implementation or V1 adapter is authorized.
- Preservation: sealed V1 `IThemeIntegrityVerifier`, `ThemeIntegrityVerificationRequest`, `ThemeIntegrityVerificationResult`, related contracts and schemas remain unchanged. V1 compatibility means source / binary / contract preservation, not Phase 4 runtime usability. Phase 4 provides no V1 runtime implementation, registration, or composition binding. Sealed V2 contracts and schemas also remain unchanged; no breaking V1 change is authorized.
- Reason: V1 lacks security-critical V2 inputs and cannot losslessly construct a V2 request. Missing content source, raw metadata, trust snapshot, channel/policy, and Developer exception authorization must never be guessed or replaced with defaults that pretend to supply security information. V2 security semantics must not be weakened for compatibility.
- Future V1 runtime support: requires a separate approved compatibility amendment defining content source resolution, raw metadata acquisition, trust source, channel/policy mapping, Developer exception mapping, V2-to-V1 result projection, and failure semantics. Until then, no adapter may be created.
- Effect on earlier authority: clarifies the V1 compatibility meaning in `TCC-DEC-2026-09-07-009` and ADR-0003; resolves task 16's CONTRACT COMPATIBILITY GAP. No earlier decision or sealed artifact is rewritten or replaced. Frozen System and Theme Architecture remain unchanged; this explicit later requirement supplies the production interface strategy.
- Authorized scope this round: this decision, project-status synchronization, and creation of `phase4-theme-integrity-verifier` from `phase4-contract-amendment-approved`, peeled commit `4ab99f8a6e7743a2ef7f40cbf17e007d9adf0bdc`. Phase 4A–4F and all production implementation remain NOT STARTED and require subsequent explicit authorization. No new project, ProjectReference, or NuGet dependency is expected. Governance changes remain unstaged; git add, commit, tag, push, merge, rebase, and amend are prohibited this round.
