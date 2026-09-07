# ADR-0002: Phase 2 Schemas & Public Contracts Scope

**Status:** Accepted
**Decision date:** 2026-09-07
**Decision authority:** Explicit later user requirement
**Decision type:** Implementation sequencing; does not replace Frozen Architecture

## Context

The Frozen Theme Architecture describes a high-level implementation sequence in §33.1, a complete schema/contract file surface in §33.2, and required public interfaces in §33.3. Phase 2 needs a deterministic boundary that creates contracts without starting production Theme runtime behavior.

## Decision

Phase 2 is **Schemas & Public Contracts**.

The complete Phase 2 scope is:

- Theme Architecture §33.1 steps 1–3.
- All unique required contract/schema files named in Theme Architecture §33.2. The repeated `ThemeCopyResources.schema.json` and `ThemeCriticalCopyRules.schema.json` entries denote the same required files and are materialized once each.
- All required C# public interfaces named in Theme Architecture §33.3.
- Stable UX contract JSON derived only from approved UX IDs and their Frozen semantics, with provenance and duplicate-ID rejection.
- Contract, schema, serialization, negative, prohibited-surface, and architecture tests in the existing `tests/Tcc.Architecture.Tests` project.

Production runtime implementation is deferred. Phase 2 defines future responsibilities through interfaces and declarative contracts only.

## Semantic invariants

- Product semantics are unchanged.
- UX semantics are unchanged.
- System semantics are unchanged.
- Theme semantics are unchanged.
- The Frozen System Architecture SHA-256 remains `7BAF7ABEBEBFFE77F20DF964E89E7845AA54E8DE810CF6FB8C6B500881275B6F`.
- The Frozen Theme Architecture SHA-256 remains `960CCCB69AFDBE006CA12BECB86A1E3E6C96C6C59500CE943F176162D2B33856`.
- Missing standalone hashes for upstream Questionnaire, Product Constitution, or UX artifacts do not authorize inventing requirements and do not by themselves stop Phase 2.
- Any necessary UX semantic that cannot be traced to Frozen sources is a stop condition; it must not be guessed.

## Ownership and testing

- C# Theme public contracts are owned by `Tcc.Presentation.Contracts` under namespace `Tcc.Presentation.Contracts.Theme`.
- JSON contract and schema artifacts are stored under `contracts/theme`.
- The existing `tests/Tcc.Architecture.Tests` project owns Phase 2 architecture, schema, contract, negative, and serialization tests.
- No project or dependency direction is added or changed.

## Explicit non-goals

This ADR does not authorize a Theme loader, runtime, manager, switch implementation, package installer/extractor, signature verifier, storage implementation, asset/cache runtime, lifecycle orchestration, rollback engine, accessibility evaluator, audio/motion/personalization engine, Default Safe Theme, Gu Qinghan package/assets, WPF Theme UI, connector/plugin/persistence/sync/domain/risk/permission runtime, installer, Portable packaging, or any Phase 3+ behavior.

## Consequences

- Later runtime phases may implement these interfaces only after their own phase gates.
- Contract compatibility and forbidden business-mutation surfaces can be mechanically tested before runtime exists.
- This ADR coexists with the earlier `ADR-0002-theme-feature-owner.md`; its filename is preserved because it was explicitly authorized for this Phase 2 sequencing decision.
