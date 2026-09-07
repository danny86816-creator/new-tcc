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
