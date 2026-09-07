# AI Trading Intelligence Baseline v1

**Artifact type:** Controlled planning, governance, and architecture-integration authority

**Integration status:** COMPLETE — awaiting 05A validation

**Production implementation status:** NOT STARTED

**Implementation authority:** NONE

**Last updated:** 2026-09-07

## 1. Authority and Change Control

This artifact records the latest explicit requirements for the future Trading Intelligence domain. It is additive repository governance and does not edit, replace, or silently reinterpret any approved or Frozen source artifact.

Authority is applied in this order:

1. Approved Questionnaire Q1–Q110.
2. Explicit later user requirements.
3. This AI Trading Intelligence Baseline v1 for the Trading Intelligence domain.
4. Approved Product Constitution, UX Architecture, System Architecture, and Theme Architecture.
5. Approved ADRs and phase scope plans.
6. Phase 1 and Phase 2 approved baselines.
7. Legacy TCC material for inventory, migration, and reference only.
8. Agent assumptions.

The baseline refines future Trading Intelligence policy while preserving the Questionnaire's fixed safety core, GPT optionality, local-first authority, permission/risk/confirmation/audit boundaries, Q93 accessibility, and read-only connector restriction.

### Architecture Amendment Required

The current Frozen System Architecture authorizes only manual V1 execution tracking and read-only broker/exchange/prop-firm connectors. Therefore:

- `Prepared Order` is a future local, non-executed artifact only.
- AI-F terminates at human confirmation of a prepared-order artifact; it does not transmit, place, modify, cancel, close, or execute an order.
- Any future broker/exchange write or execution path requires a separately approved architecture amendment and explicit implementation authorization.
- No Frozen document is modified by this integration.

## 2. Three-Layer Authority Boundary

### A. Deterministic Engine

The deterministic engine is the final system authority for:

- Risk arithmetic and position sizing.
- Hard constraints, permissions, execution gates, and lifecycle/state transitions.
- Setup identity, deterministic scoring, versioning, and data-freshness validation.

Its decisions must be reproducible, versioned, fail closed where required, and auditable. An LLM cannot override them.

### B. AI Layer

The AI layer may provide:

- Contextual and market-state interpretation.
- Confluence judgment and evidence synthesis.
- Explanation, similarity retrieval, recommendations, learning proposals, and candidate-rule proposals.

The AI layer must not modify hard risk, approve a strategy, raise execution permission, bypass a deterministic gate, autonomously place an order, or treat inferred data as observed market data.

### C. Human Governance

Human authority retains:

- Strategy, candidate-rule, trial, and model-promotion approval.
- Execution authority, overrides, AI mode control, and final permission escalation.

Human confirmation does not bypass deterministic risk, permission, freshness, or audit gates. An LLM is never the final authority for risk, permission, hard gates, or execution.

## 3. Future Capability Groups

These are planning groupings only. They do not authorize projects, interfaces, schemas, services, providers, or runtime implementations.

| Group | Planned capabilities |
|---|---|
| Trading Observation & Setup | Market Observation, Candidate Pool, Setup Lifecycle, Setup Identity, Multi-Timeframe Setup, Setup Expiration |
| Deterministic Trading Rules | Strategy Rule, Risk Policy, Setup Quality, Hard Constraint, Strategy Constraint, AI Advisory |
| Decision & Evidence | Decision Snapshot, Decision Logger, Evidence, Conflicting Evidence, Missing Conditions, Human Override, Version Provenance |
| Position Intelligence | Market State, Structural Health, Position Health, Position Timeline, Protective Stop Advice, Reduction Advice, TP Extension Advice |
| AI Governance | AI Recommendation, Shadow Mode, Candidate Rules, Historical Validation, Trial, Model Governance, Learning Proposal |
| Data & Safety Infrastructure | Market Data Quality, Provenance, Freshness, Degraded Mode, Safe Mode, Prepared Order Boundary, Permission Gate |

## 4. Rule Classification

| Classification | Authority and behavior | Examples |
|---|---|---|
| Hard Constraint | Deterministic blocking. AI and human confirmation cannot silently bypass it. | Absolute/aggregate risk caps, maximum positions, daily-loss block, permissions, critical market-data absence, execution gate |
| Strategy Constraint | Affects setup eligibility, quality, confidence, waiting, or downgrade; not every rule is necessarily a hard block. | Strategy-specific confluence, timing, structure, quality thresholds |
| AI Advisory | Non-authoritative interpretation or recommendation; never auto-executed. | Trend weakening, protective stop, reduction, early exit, TP extension, market-state interpretation |

Classification, rule version, effective version, evidence, and outcome must remain auditable.

## 5. Risk Policy Authority Supersession

This supersession applies only to **Future Trading Risk Policy Authority**. Historical requirements and documents remain unchanged.

| Policy | Historical authority — superseded for future Trading Risk Policy | Current future authority |
|---|---:|---:|
| Total / aggregate position risk | 2% | 3.0% maximum aggregate position risk |
| Maximum simultaneous positions | 2 | 3 |
| Target single-trade risk | Not established by the historical values above | 1.0% |
| Absolute single-trade hard cap | Not established by the historical values above | 1.2% |
| Daily loss | Not established by the historical values above | -3% blocks new entries |
| Consecutive losses | Not established by the historical values above | 2 warns; 3 blocks new entries |
| Consecutive-loss protection | Not established by the historical values above | Master toggle required |

The master toggle controls the consecutive-loss protection policy only. It cannot disable unrelated hard constraints. Exact runtime arithmetic, reset windows, persistence, and UI behavior require future contract and architecture approval; no `RiskPolicyEngine` is authorized here.

## 6. Trading Intelligence Invariants

### 6.1 Setup Identity

- Every setup has an independent identity.
- Same symbol plus different timeframe means different setup; for example, `ETH 4H Long` and `ETH 1H Short` may coexist.
- Identity, lifecycle, evidence, state, history, and quality remain separate.
- Position and account risk aggregation must prevent double counting across related setups.

### 6.2 Setup Lifecycle

```text
Observed
→ Candidate
→ Near Entry
→ Tradable
→ Entered
→ Position
→ Closed
```

Terminal/alternate states include `Invalidated`, `Expired`, and `Abandoned`. An invalidated or expired setup cannot revive merely because price returns to a prior area; a new opportunity receives a new setup identity.

### 6.3 Decision Audit

An AI decision must preserve at least:

- Market data, source identity, timestamp, freshness, and quality state.
- Model, rule, strategy, prompt, and configuration versions.
- Evidence, conflicting evidence, missing evidence, and confidence.
- Reasons for, reasons against, recommendation, human override, and override reason.

`LONG`, `SHORT`, or `WAIT` alone is not an auditable decision record.

### 6.4 Data Trust

Market data must identify source/exchange, timestamp, freshness, and quality. The minimum quality vocabulary is `Fresh`, `Delayed`, `Stale`, `Missing`, `Abnormal`, `Conflicting`, and `Degraded`.

- Stale data cannot be silently treated as live.
- Inferred or generated values cannot masquerade as observed market data.
- Missing critical market data prevents a high-confidence entry recommendation.
- Degraded behavior and its effect on risk/permission/recommendation must be explicit.

### 6.5 Position Health

Position Health uses this governed composition:

```text
Deterministic Rule Base Score
+ AI Context Adjustment
= Final Interpreted Health (0–100)
```

The record preserves base score, adjustment, reasons, rule version, and model version. The LLM cannot assign an arbitrary final score or change the deterministic base.

### 6.6 Shadow and Real Separation

`Shadow Trade != Real Trade`.

- Records, lifecycle, execution authority, and metrics are separate.
- Comparison is allowed, but conversion is not implicit.
- Shadow Mode cannot produce any real execution side effect.

### 6.7 Prepared Order Boundary

```text
AI Recommendation
→ Prepared Order
→ Deterministic Risk Gate
→ Permission Gate
→ Human Confirmation
→ STOP (non-executed artifact)
```

A Prepared Order is not an Executed Order. `Prepared → automatic send` is prohibited. Autonomous execution and exchange write integration are not authorized.

### 6.8 Learning Governance

```text
Observation
→ Learning
→ Candidate Rule
→ Historical Validation
→ Shadow
→ Comparison
→ Human Review
→ Trial
→ Human Approval
→ Production
```

AI learning cannot mutate production strategy directly and can never modify hard risk.

### 6.9 Model Governance

```text
New Model
→ Shadow
→ Compare
→ Trial
→ Human Approval
→ Production
```

Automatic production replacement is prohibited. Model promotion remains auditable and reversible through separately approved governance.

### 6.10 Safe Mode

Future Trading Intelligence Safe Mode stops prepared orders and execution-related AI actions, and may stop new recommendations according to approved policy. It preserves existing-position visibility, SL/TP awareness, risk and market monitoring, and critical alerts. It is not a total system shutdown and does not weaken the existing Frozen Recovery/Safe Mode boundary.

## 7. Frozen Compatibility and Impact Matrix

| Authority / surface | Impact | Required treatment |
|---|---|---|
| Phase 1 baseline | No change | Preserve commit/tag and dependency allowlist |
| Phase 2 Theme contracts | No change | Sealed schemas and public contracts remain untouched |
| Phase 3 Theme Manifest Validator | No production change | Scope remains deterministic manifest validation only |
| Frozen System Architecture | No source edit | Compatible safety/AI boundaries retained; any execution/write path is `Architecture Amendment Required` |
| Frozen Theme Architecture | No source edit | Theme remains presentation-only and independent of Trading Intelligence |
| Future Trading contracts | Additive, deferred | Plan and approve in AI-A before implementation |
| Future Trading schemas | Additive, deferred | Plan and approve in AI-A before implementation |
| Risk authority | Updated for future policy | Apply the supersession table; preserve history |
| Theme sequencing | No immediate change | Phase 3 and later Theme runtime sequencing remain independent |
| AI roadmap | New Architecture Amendment Track | AI-A through AI-F are roadmap only |
| Exchange execution | Not authorized | Read-only connector boundary remains in force |
| LLM production | Not authorized | GPT remains optional; no production provider/runtime in 05A |

## 8. AI Trading Architecture Amendment Track

This track does not consume or renumber the Theme Runtime Track. Every stage requires separate scope approval and validation before implementation.

### AI-A — Contracts & State Foundation

Plan additive contracts for setup identity/lifecycle, market observation, decision snapshots, evidence, human override, version provenance, market state, and position state. **Non-goal: no LLM.**

### AI-B — Deterministic Trading Core

Plan Risk Policy, Setup Quality, rule classification, deterministic risk arithmetic, state transitions, setup expiration, and position risk. **Non-goal: no AI execution.**

### AI-C — Observation & Position Intelligence

Plan multi-timeframe hierarchy, structure, support/resistance, Fibonacci context, liquidity, trend strength, Market State, Structural Health, Position Health, and timeline.

### AI-D — Recommendation & Explainability

Plan recommendation, reasons for/against, missing conditions, confidence, suggested entry/SL/TP/size, and human feedback. The result remains recommendation-only.

### AI-E — Shadow & Learning

Plan Shadow Trading, human-versus-AI comparison, Candidate Rules, Historical Validation, Trial, and Model Governance with human approval gates.

### AI-F — Prepared Order

Plan a non-executed Prepared Order artifact, deterministic gate, permission gate, and human confirmation. The track ends before transmission or execution; autonomous execution remains prohibited.

## 9. Theme / Trading Intelligence Separation

```text
Theme Runtime Track
||
Trading Intelligence Track
```

- Trading/AI contracts must be additive and must not enter the Theme contract family.
- Theme manifests, validators, runtime packages, feature packages, and UI adapters cannot depend on Trading AI, recommendations, observations, risk policy, position intelligence, shadow/learning, prepared orders, market data, or execution capabilities.
- Theme Manifest Validator public contracts remain unchanged.
- Theme Packages cannot change Trading Intelligence semantics, risk, permissions, hard gates, data authority, or execution authority.

## 10. Future Validation Gates

Before any AI-A–AI-F implementation is authorized, its approved phase plan must define and validate:

- Additive contract/schema ownership and dependency direction.
- Deterministic-versus-AI-versus-human authority tests.
- Risk arithmetic, hard-cap, aggregate-risk, maximum-position, daily-loss, and consecutive-loss boundaries.
- Setup identity/lifecycle and no-resurrection tests.
- Decision provenance, evidence completeness, and override auditability.
- Market-data source/freshness/quality fail-closed behavior.
- Position Health base/adjustment/version traceability.
- Shadow/real storage, lifecycle, metric, and side-effect isolation.
- Prepared-order non-execution and connector read-only guards.
- Learning/model promotion gates and prohibition of automatic production mutation.
- Safe Mode preservation of position visibility, monitoring, and critical alerts.
- Phase 1 dependency, Frozen integrity, Theme isolation, GPT optionality, Q93, Installer/Portable parity, build, test, and Git hygiene gates.

## 11. Deferred / Non-Goals

05A does not authorize or create:

- Production code, tests, projects, public interfaces, schemas, migrations, or package dependencies.
- `RiskPolicyEngine`, Theme Manifest Validator implementation, AI runtime, LLM provider, vector database, ML pipeline, background worker, message bus, or event-sourcing infrastructure.
- Broker/exchange/prop-firm write APIs, order transmission, trade execution, or autonomous execution.
- Strategy approval, rule promotion, model promotion, or production learning mutation without human governance.
- Changes to approved/Frozen artifacts, Phase 1/2 baselines, or Phase 3 production scope.

## 12. Exact Next Action

Return to GPT Supervisor for 05A validation.

Do not start Phase 3 implementation.
