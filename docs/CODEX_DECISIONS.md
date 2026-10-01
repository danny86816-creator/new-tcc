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

## TCC-DEC-2026-09-08-012 — Reserved integrity metadata cannot be payload declarations

- Date: 2026-09-08.
- Decision source: explicit human-approved GPT Supervisor resolution in task 25, resolving the Phase 4B reserved metadata declaration gap and authorizing continuation of implementation.
- Decision: `IntegrityManifest.Files` is payload inventory only. `IntegrityManifestPath` and non-null `SignatureEnvelopePath` identify reserved integrity metadata and may not be declared as ordinary payload files. Validate and normalize the declaration using TCC Package Canonical Path v1 first; an exact NFC canonical match using `StringComparer.Ordinal` rejects with `P4I029`, severity Error, and `CanonicalPath = null`. The fixed diagnostic message is: `A payload file declaration must not target a reserved integrity metadata path.`
- Evidence and reads: an invalid reserved declaration produces no file evidence, regardless of Required/Optional or present/absent state. Phase 4B does not read reserved metadata as payload or perform payload length/SHA-256 verification on it. Raw reserved metadata processing remains deferred to approved later stages, including Phase 4C raw metadata validation.
- Present inventory: exact reserved metadata remains excluded from ordinary payload exhaustive inventory comparison. Similar names and case variants are not automatically reserved; normal path, collision, and inventory rules still apply. Decomposed declarations that normalize exactly to a reserved identity are invalid reserved declarations. Present reserved entries still undergo canonical path, duplicate/NFC/case collision, and physical entry-kind safety checks.
- Related approved clarification: malformed logical paths use `P4I002` and the original offending logical path without host/exception/reader details. For a read failure after enumeration, `Unavailable` evidence carries enumerated `LengthBytes` and null `ActualSha256`; that length is only an enumeration metadata claim and never means content bytes or content length were verified.
- Reason and impact: prevents integrity metadata self-reference and accidental payload treatment; preserves consistent diagnostic, evidence, and read semantics across Phase 4B through Phase 4E.
- Effect on earlier authority: resolves the previously unspecified reserved-declaration case; no historical decision is rewritten. No V1/V2/schema amendment or ADR-0003 edit is introduced. The sealed oracle's generic declaration fall-through is not authority to treat reserved metadata as payload.

## TCC-DEC-2026-09-08-013 — Phase 4C Raw Metadata Custody and Runtime Validation Boundary

- Date: 2026-09-08.
- Decision source: explicit GPT Supervisor Resolution supplied by the user in `31—Phase 4C Supervisor Planning`. Current authorization is this append-only governance clarification and Phase 4C planning closure/re-derivation only; it does not authorize implementation, production/test/project/schema/status edits, build, commit, tag, or push.
- Reason and impact: defines cross-stage raw-byte custody, production schema authority, metadata read diagnostics, and non-cryptographic binding for Phase 4C while preserving sealed Phase 4B and the later Phase 4D/4E boundaries.
- Sealed boundary: Phase 4B remains sealed. Its outcome is not extended to carry raw bytes. Phase 4C may execute only after `ThemePackageInventoryEvaluation.CanContinue == true`; unsuccessful inventory outcomes must not enter raw metadata or tree-hash processing. Tree records use present `Verified` evidence; `MissingAllowed` is legal absence and produces no record.
- Theme Manifest custody: Phase 4B reads it at most once; Phase 4C may re-read it exactly once when processing that metadata, for at most two reads across Phase 4A–4C. Before JSON parsing, schema validation, materialization, semantic validation, or identity/version checks, require an exact canonical-path match to Phase 4B Verified Theme Manifest evidence and compare the second-read byte length and SHA-256 with its `ActualLengthBytes` and `ActualSha256`. Length or hash incoherence fails closed with `CanContinue = false`, `P4I010`, and the Theme Manifest canonical path. The fixed message is: `Theme Manifest raw bytes do not match the content verified by the package inventory stage.` Incoherent bytes must not be parsed.
- Reader contract: the caller reader must still satisfy the sealed immutable-content contract. The second-read proof is defense in depth, not permission for mutable content. Other ordinary payload files are neither re-read nor cached by Phase 4C; no whole-package byte cache is introduced.
- Reserved metadata custody: Phase 4C owns the first raw read of the Integrity Manifest, at most once. Phase 4C owns the first raw read of the Signature Envelope, at most once and only when `SignatureEnvelopePath` is non-null. A null path causes no read, no fabricated envelope, and no `P4I012`; final unsigned-channel acceptance remains Phase 4D.
- Read diagnostics: Theme Manifest second-read `FileNotFoundException` maps to `P4I021` at its canonical path, meaning previously verified package content is now unavailable. Integrity Manifest first-read `FileNotFoundException` maps to `P4I011` at `IntegrityManifestPath`. Referenced Signature Envelope first-read `FileNotFoundException` maps to `P4I012` at `SignatureEnvelopePath`. Other approved `IOException`, `InvalidDataException`, and `UnauthorizedAccessException` failures map to `P4I021` at the affected safe canonical path. Handle the specific FileNotFound mappings before general I/O handling. Do not expose exception text, host paths, or reader implementation details. `OperationCanceledException` always propagates; cancellation requested during an I/O-failure race takes precedence over ordinary diagnostics.
- Production schema mechanism: use a BCL-based constrained JSON Schema validator that actually loads and executes the exact sealed files `contracts/theme/schemas/ThemeManifest.schema.json`, `contracts/theme/schemas/ThemeIntegrity.v2.schema.json`, and `contracts/theme/schemas/ThemeSignatureEnvelope.v1.schema.json`. Future authorized implementation may minimally edit `src/Tcc.Themes/Tcc.Themes.csproj` solely to embed those exact source files with stable logical resource names. Preserve their sealed SHA-256 values; no copied/generated replacement schema, network loading, or mutable external runtime source is allowed.
- Schema capability gate: inventory every keyword, format, reference, type, and numeric/array/object/string constraint actually used by the three schemas before implementation edits; establish an exact supported-keyword allowlist. Unknown or unsupported keywords fail closed and stop implementation as `PHASE 4C UNSUPPORTED SCHEMA KEYWORD GAP`; no keyword may be silently ignored. A missing or malformed embedded authoritative schema is a programmer/deployment integrity fault, not an ordinary hostile-package diagnostic. Production must not reference the test assembly or call its `JsonSchemaSubsetValidator`; test differential comparison is permitted, with the sealed schemas remaining authority. No new JSON Schema NuGet dependency is authorized.
- Strict raw JSON: before DTO materialization, use `Utf8JsonReader` or equivalent BCL low-level processing for strict UTF-8, one root, no trailing tokens, and recursive per-object duplicate rejection, including objects inside arrays. Compare decoded property names exactly; differently cased names are not duplicates but must satisfy case-sensitive schema/property binding. Do not use regex JSON scanning, last-write-wins, case-insensitive enum/property binding, or lossy numeric conversions.
- Integrity Manifest binding: compare the raw-materialized DTO with `request.IntegrityManifest` across every contract-defined field. Object property order, whitespace, and formatting do not affect equality; `Files` list order and every list-item field do. Strings, including hashes and untyped canonical-path strings, compare ordinally without extra normalization. Enums must be defined and exactly equal. Null/non-null and absent/null semantics follow each sealed schema/contract field, never a generalized deserializer-default equivalence. `LengthBytes` uses direct exact Int64 parsing without floating-point/decimal intermediaries, overflow wrapping, or lossy conversion. Binding mismatch is `P4I028` at `IntegrityManifestPath`; the caller DTO must not replace the validated raw DTO.
- Theme Manifest binding: after the cross-stage proof, execute strict raw JSON, the actual schema, `ThemeContractJson` materialization, the sealed `ThemeManifestValidator`, and request/Integrity Manifest identity/version binding. Do not invent equality against a nonexistent full caller Theme Manifest DTO. Identity and version mismatches use `P4I024` and `P4I025`; other raw-document/schema/materialization/sealed-semantic failures use `P4I027` under the sealed diagnostic semantics.
- Signature ownership: Phase 4C owns exact-path raw reading, strict JSON, duplicate rejection, authoritative schema validation, materialization, caller DTO binding, and deterministic non-cryptographic inputs for the envelope. ECDSA P-256/SHA-256, IEEE P1363 cryptographic verification, SPKI and exact curve OID validation, publisher/key lookup, unknown/revoked trust, trust snapshots, channel policy, Developer exceptions, and final signature verdict remain Phase 4D. No new `SignatureEnvelopeHash` public/security field is introduced.
- Canonical Package Tree Hash v1: use present Phase 4B Verified payload evidence, including the Theme Manifest; omit MissingAllowed and exactly exclude `IntegrityManifestPath` and non-null `SignatureEnvelopePath`. Preserve case in NFC canonical paths and sort by `StringComparer.Ordinal`. Each record is `canonical_path NUL decimal_length NUL lowercase_sha256_hex LF`, with invariant decimal length, UTF-8 without BOM, separators 0x00 and terminator 0x0A; `entry_kind` is not serialized. SHA-256 the concatenated records and compare ordinally with raw-materialized `IntegrityManifest.PackageHash`; mismatch is `P4I009` with null canonical path. No ordinary payload reread is permitted.
- Raw top-level hashes: `ThemeManifestHash` and `IntegrityManifestHash` are SHA-256 of their exact owned raw bytes, never DTO reserialization. The Theme Manifest hash must first pass the Phase 4B proof and bind to the sealed Integrity Manifest's corresponding expected file hash; no nonexistent top-level expected-hash field is invented. Integrity Manifest actual-hash binding follows the sealed signature payload contract, with cryptographic acceptance deferred to Phase 4D. Computing a hash does not establish document validity or stage continuation.
- Outcome and memory: the proposed internal Phase 4C outcome may carry `CanContinue`, deterministic diagnostics, actual package/Theme Manifest/Integrity Manifest hashes, and validated materialized metadata needed by Phase 4D/4E. Retain raw metadata bytes only when a downstream requirement genuinely needs them. It must not carry `IsVerified`, trusted publisher/key, channel approval, signature verification, or a final package verdict.
- Dependencies and phase boundary: no new project, ProjectReference, NuGet dependency, or friend assembly is authorized. Phase 4D retains crypto/trust/channel policy; Phase 4E retains public composition and final result. Production `ThemeIntegrityVerifier` count must remain zero through Phase 4C. No V1/V2/schema amendment or public verifier is introduced.
- Effect on earlier authority: this later explicit resolution clarifies Phase 4C custody and diagnostics without rewriting Decisions 011/012, ADR-0003, or any sealed artifact. It does not itself declare the planning gate passed: any remaining authority/input contradiction must be reported to Supervisor before implementation authorization.

## TCC-DEC-2026-09-08-014 — Phase 4C Verified Theme Manifest Evidence Entry Precondition

- Date: 2026-09-08.
- Decision source: explicit GPT Supervisor Resolution supplied by the user in `31—Phase 4C Supervisor Planning`, resolving `PHASE 4C VERIFIED THEME MANIFEST EVIDENCE PRECONDITION GAP`. This round authorizes only this append-only governance decision and the final Phase 4C planning closure report, not implementation, production/test/project/schema/status edits, restore/build/test, staging, commit, tag, or push.
- Reason and sealed preservation: Phase 4B remains sealed and unchanged. Its `CanContinue == true` alone is insufficient to enter Phase 4C. Preserve the legitimate sealed Phase 4B behavior in which an unrelated Verified payload, such as `assets/icon.png`, can produce `CanContinue == true` even when `request.ThemeManifestPath` is `theme.json` and no corresponding Theme Manifest evidence exists. Rejecting that input for metadata processing is now explicitly Phase 4C's responsibility, not a Phase 4B defect or reopening.
- Hard entry precondition: first require `ThemePackageInventoryEvaluation.CanContinue == true`; otherwise Phase 4C must not execute. Then establish the canonical logical identity of `request.ThemeManifestPath` using the same sealed path semantics as Phase 4B: Unicode NFC, case preserved, and Ordinal identity. No filesystem resolution, host-path conversion, or case-insensitive matching is permitted. Require exactly one FileEvidence item matching that canonical identity; it must have `IsPresent == true`, `Status == Verified`, non-null `ActualLengthBytes`, and non-null `ActualSha256` in the sealed lowercase SHA-256 format (exactly 64 ASCII characters from `0-9` and `a-f`).
- Execution order: this entry precondition must pass before any Phase 4C package/metadata reader call, Theme Manifest second read, Integrity Manifest or referenced Signature Envelope first read, tree-hash computation, raw hash, JSON parse, schema validation, materialization, semantic check, or other Phase 4C processing. It is an entry gate before all such work, not a compensating validation after bytes have already been read.
- Failure outcome: when Phase 4B `CanContinue == true` but the required evidence precondition fails, return a deterministic package metadata structural/semantic failure with `CanContinue = false`, `P4I027`, and `CanonicalPath` equal to the canonical Theme Manifest path. The fixed message is: `Theme Manifest must be present as verified package content before metadata validation.` Phase 4C reader calls are zero; tree and raw metadata hashes are not computed; parsing, schema validation, materialization, and `ThemeManifestValidator` are not run. Do not throw merely because the required evidence is missing.
- Failure cases: absence of a matching item; matching `MissingAllowed`, `MissingRequired`, `LengthMismatch`, `HashMismatch`, `Unavailable`, `UnsupportedEntry`, or any other non-Verified status; `IsPresent != true`; null actual length; null or invalid actual hash; and any matching count other than exactly one all fail this gate with `P4I027`. A differently cased path does not match. NFC-equivalent identities follow the same canonical semantics; multiple matches after canonical identity comparison fail rather than selecting any one item.
- No evidence repair: do not synthesize evidence, select the first or last matching item, reread content to compensate for missing evidence, infer evidence from caller DTO/raw package data, or claim that Phase 4B verified content it did not verify.
- Failure separation: missing or otherwise unsuitable Verified Theme evidence is `P4I027`, not `P4I005` (Phase 4B declared-required-payload absence), `P4I010` (a later raw-content comparison), or `P4I021` (a later availability failure). After the entry gate passes, Theme Manifest second-read `FileNotFoundException`, `IOException`, `InvalidDataException`, or `UnauthorizedAccessException` remains `P4I021` at its canonical path under Decision 013. A successful second read whose byte length or SHA-256 differs from the admitted Phase 4B evidence remains `P4I010` at that path and must not be parsed. Cancellation continues to propagate with the existing precedence.
- Fault distinction: an incomplete/package-reachable evidence precondition failure is a deterministic `P4I027` package outcome, not a deployment/programmer exception. Contradictory duplicate Verified evidence that cannot originate from lawful Phase 4B execution is distinguishable as an internal-invariant corruption scenario, but it still must fail closed under the matching-count gate; no first/last-item selection or new public diagnostic is authorized. Genuine programmer/deployment faults such as missing or malformed embedded authoritative schemas retain their separate Decision 013 treatment.
- Tree-hash consequence: Canonical Package Tree Hash v1 is unchanged. Before tree computation, Phase 4C must already have exactly one present Verified evidence item for the canonical Theme Manifest path with the required actual fields. Theme Manifest remains a required Verified tree input; all other present Verified payload evidence remains included under the existing exact reserved-metadata exclusions, while MissingAllowed contributes no record. No ordinary payload reread is authorized.
- Required planned coverage: `CanContinue == true` with no Theme evidence; unrelated Verified payload only reproducing the sealed Phase 4B output; MissingAllowed; every other non-Verified status; false presence; absent actual fields; invalid lowercase hash format; a valid exact match permitting only subsequent second-read coherence; case-different rejection; NFC-equivalent matching; and multiple matches rejected without selection. Failure cases must prove zero Phase 4C reader calls and no tree/raw hash or downstream metadata processing. These are future implementation tests, not authorization to edit or execute tests this round.
- Effect on earlier authority: closes the previously unspecified Phase 4C entry-evidence branch and replaces the earlier inference that Phase 4B success alone guarantees Verified Theme Manifest evidence. Decisions 011, 012, and 013 remain byte-preserved; Decision 013's admitted-evidence custody, schema, binding, read diagnostics, and Phase 4D/4E boundaries remain applicable. No ADR edit, Phase 4B reopening, V1/V2/schema amendment, new dependency, or public verifier is introduced. Planning readiness is not implementation authorization.

## TCC-DEC-2026-09-09-015 — Phase 4D Signature Identity, SPKI, Channel Policy, Diagnostic and Evaluation Precedence

- Date: 2026-09-09.
- Decision source: explicit GPT Supervisor — Phase 4D Governance Resolution supplied by the user in `38—Phase 4D Scope Planning`. This decision records that supplied resolution; it does not itself declare planning PASS or silently resolve contradictions discovered during planning closure.
- Authorized scope: append this Decision015 only to `docs/CODEX_DECISIONS.md`, then perform read-only Phase4D planning closure in the current conversation. No implementation, build, test, status edit, staging, commit, tag, push, or new conversation is authorized. Decisions011–014, ADR-0003, V1/V2 contracts, schemas, Frozen System/Theme, Phase4A/B/C production/tests, and project files remain unchanged. No further decision may be created automatically.
- Reason and effect: clarify Phase4D signing identities, trust consumption, cryptographic representation, channel policy, deterministic diagnostics, and internal outcome while preserving sealed signed-payload bytes, upstream custody, and the Phase4E public-composition boundary. These later explicit rules supply the Phase4D production policy; existing oracle code order is not a substitute for this decision. Remaining contradictions must be reported to Supervisor, not repaired by assumption or forced into PASS.

### Signed payload and signing identities

- Preserve exactly `UTF8_NO_BOM("TCC-THEME-PACKAGE-SIGNATURE-V1" + NUL + ThemeId + NUL + ThemeVersion + NUL + PackageHash + NUL + ThemeManifestHash + NUL + IntegrityManifestHash + NUL + PublisherId + NUL + KeyId)`. Existing golden bytes remain authoritative. Do not change the discriminator, add field names, length prefixes, escaping, LF, or trailing NUL; do not switch to JSON or normalize fields before signing. Preserve the sealed lowercase hash representation and the exclusion of `ThemePackageRef` from publisher-signed content.
- Apply the signing-ID profile to `SignatureEnvelope.PublisherId`, `SignatureEnvelope.KeyId`, and every `TrustSnapshot.TrustedSigners[]` publisher/key ID. Each must be non-null, non-empty, consist of valid Unicode scalar values, encode to UTF-8 successfully, and occupy 1–256 UTF-8 bytes inclusive on the exact acquired string without normalization.
- Reject all C0/C1 control scalar values `U+0000..U+001F` and `U+007F..U+009F`, including NUL. This closes the signed-payload NUL framing ambiguity. No Trim, case conversion, OrdinalIgnoreCase, culture comparison, NFC/NFD, or compatibility normalization is permitted. Equality is `StringComparer.Ordinal` on exact validated strings; case variants, visually similar Unicode, and composed/decomposed forms remain distinct.
- For signed packages, validated `ThemeManifest.Package.PublisherId` must Ordinal-equal `SignatureEnvelope.PublisherId` before trust lookup. `KeyId` has no invented ThemeManifest binding. Invalid envelope/snapshot signer IDs or publisher mismatch produce `P4I029` with the fixed message below, never interpolating identities or raw Unicode.
- Resolve only the exact `(PublisherId, KeyId)` tuple using Ordinal + Ordinal. No global KeyId search, normalized match, closest match, or first/last selection.

### Signature and public-key representation

- Signature algorithm remains ECDSA P-256 with SHA-256 and IEEE P1363 fixed-field concatenation: exactly 64 bytes, 32-byte `r || s`, each unsigned fixed-width big-endian. DER signatures and alternate-encoding fallback are forbidden.
- Signature text must be exactly 88 ASCII characters of canonical standard Base64, with exact `==` padding, canonical pad bits, no whitespace, URL-safe alphabet, or omitted padding. Perform strict lexical check, decode, require 64 bytes, re-encode with standard Base64, and compare Ordinal-exactly to input. Any failure is `P4I013`.
- Phase4D v1 has no low-S-only restriction. Do not normalize `s`, reject high-S explicitly, or add a diagnostic. If BCL verifies both mathematically valid `s` and `n-s`, both are acceptable. Future low-S-only policy requires separate governance. Plan both forms; unexpected supported-provider incompatibility must be reported before implementation completion, not hidden by custom normalization. The low-S policy gap is closed for v1 by no explicit restriction.
- `ThemeTrustedSignerV1.PublicKey` must be canonical standard Base64 containing exactly one DER SubjectPublicKeyInfo value. Require exactly 124 ASCII characters and exactly 91 decoded DER bytes; re-encoding must Ordinal-equal the original. Reject whitespace, URL-safe form, missing/noncanonical padding or pad bits, trailing text, and wrong decoded length. Malformed public-key representation is `P4I013`.
- SPKI algorithm is exactly `id-ecPublicKey`, OID `1.2.840.10045.2.1`; parameters are named-curve OID only, exactly `1.2.840.10045.3.1.7` (P-256 / secp256r1 / prime256v1). Explicit, absent, NULL, or other curve parameters are rejected, including secp256k1 and brainpool.
- Adopt exact DER and full consumption: the outer SPKI, AlgorithmIdentifier, parameter encoding, BIT STRING, and outer reader must be fully consumed (`ThrowIfNotEmpty` equivalent). `ImportSubjectPublicKeyInfo` bytesRead must equal decoded SPKI length. Trailing bytes are rejected.
- The subjectPublicKey BIT STRING has exactly zero unused bits and exactly 65 EC-point bytes with first byte `0x04`: only uncompressed SEC1 is accepted. Reject compressed `0x02`/`0x03`, hybrid, point-at-infinity, and non-65-byte point representations.
- After structural DER validation, import using BCL `ECDsa.ImportSubjectPublicKeyInfo`, require full consumption, and require `ExportParameters(false).Curve` to be named with exact OID `1.2.840.10045.3.1.7`. KeySize should be consistent with P-256; size or successful import alone is never curve-identity authority.
- Malformed material maps to `P4I013`: noncanonical PublicKey Base64, wrong decoded length, malformed DER, invalid BIT STRING encoding, trailing malformed DER, invalid EC point material, and candidate-key-caused BCL import failure.
- Structurally identifiable material violating the algorithm/profile maps to `P4I016`: wrong algorithm OID, wrong named curve, explicit parameters, or unsupported point encoding including compressed/hybrid form. The exact stage order below also applies. Planning must report any unresolved interaction between these supplied classifications and earlier representation gates rather than silently changing them.

### Trust snapshot and resources

- Consume existing `ThemeTrustSnapshotV1` with PolicyId, PolicyVersion, and `ImmutableArray<ThemeTrustedSignerV1>`. No registry, CA, network lookup, key download, mutable global cache, or trust-membership decision is introduced. Preserve the immutable caller-provided snapshot model.
- Expected trust-policy identity/version must equal snapshot PolicyId/PolicyVersion using sealed semantics; mismatch is `P4I023` with fixed text below.
- Validate every signer PublisherId/KeyId against this decision. Any invalid signer identity produces `P4I029`; do not select the target first and ignore invalid registry identities.
- Duplicate means the same exact publisher/key pair using Ordinal + Ordinal. Scan the entire immutable signer array. Any duplicate, including byte-identical records or Trusted/Revoked ambiguity, fails with `P4I022`. Never first-wins or last-wins. Different KeyIds under one publisher or identical KeyIds under different publishers are not duplicate pairs.
- After global validation, no exact envelope pair produces `P4I014`. Matching `TrustState.Revoked` produces `P4I015`; Trusted may continue. No clock, NotBefore/NotAfter, historical acceptance, or revocation timestamp is introduced.
- Do not add an arbitrary signer-count limit: the snapshot is application-managed rather than package-controlled raw input. Use one complete O(n) deterministic scan for signer ID profile, duplicates, and target resolution; registry resource limits remain outside Phase4D v1. Preserve stage precedence: invalid snapshot ID beats duplicate, and duplicate beats target lookup. No cache, network, or package reread.
- Signing IDs are bounded to 256 UTF-8 bytes each, PublicKey to the exact 124-character/91-byte profile, and signature to 88 characters/64 bytes.

### Authoritative channel policy

| Channel / requirement | Exception flag legality | Absent envelope after legal policy | Present envelope |
|---|---|---|---|
| Stable / Required | must be false; true is P4I018 | P4I012 | full signature/trust verification |
| Store / Required | must be false; true is P4I018 | P4I012 | full signature/trust verification |
| Stable or Store / Optional | Optional is P4I017; illegal exception flag is checked first | no absence evaluation after policy failure | no verification after policy failure |
| Beta / Required | must be false; true is P4I018 | P4I012 | full verification |
| Beta / Optional | must be false; true is P4I018 | authorized unsigned-by-policy may continue | full verification |
| Developer / Required | must be false; true is P4I018 | P4I012 | full verification |
| Developer / Optional | either flag value allowed with present envelope | true: authorized unsigned exception; false: P4I018 | full verification regardless of flag |

- `DeveloperExceptionAuthorized` is existing explicit caller authorization, not permission evaluation by Phase4D. It never overrides Required. A true flag outside Developer is always invalid, never ignored. A present Developer signature that is invalid, malformed, unknown, or revoked must fail; no fallback to unsigned acceptance.

### Diagnostics and exact evaluation precedence

- Phase4D v1 is fail-fast: at most one new diagnostic per evaluation, exactly one for a Phase4D security failure. Do not accumulate Phase4D failures or conflate this with Phase4A/B/C collections. Phase4D must not run when Phase4C CanContinue=false.
- Every Phase4D diagnostic has Severity=Error, CanonicalPath=null, and failure CanContinue=false. Do not add SignatureEnvelopePath just for diagnostics; Phase4C owns metadata-file-path diagnostics.
- Messages are exactly the table below. Never interpolate PublisherId, KeyId, raw Unicode, public key/SPKI, signature/Base64, registry entries, host paths, provider messages/types, or stack details. Identities may appear only in the internal signed success outcome for Phase4E.

| Code | Condition | Exact message |
|---|---|---|
| P4I012 | required signature absent after valid policy | A signature is required by the active theme integrity policy. |
| P4I013 | invalid signature representation/verification or malformed signer public-key material | The signature or signer public-key material is invalid. |
| P4I014 | no exact publisher/key pair | No trusted signer entry matches the envelope publisher/key identity. |
| P4I015 | matching signer revoked | The matching signer entry is revoked. |
| P4I016 | structurally identifiable unsupported algorithm/curve/SPKI profile | The signer public key does not match the approved ECDSA P-256 SPKI profile. |
| P4I017 | Stable/Store with SignatureRequirement.Optional | The signature requirement is inconsistent with the selected theme release channel. |
| P4I018 | non-Developer exception=true; Developer Required exception=true; Developer Optional absent without exception | The Developer unsigned-signature exception is not authorized for this policy state. |
| P4I022 | duplicate exact signer pair | The trust snapshot contains duplicate publisher/key signer identities. |
| P4I023 | expected trust-policy identity/version mismatch | The supplied trust snapshot does not match the expected trust policy identity or version. |
| P4I029 | invalid signing ID or ThemeManifest/envelope publisher mismatch | A signing identity field is invalid or inconsistent. |

Adopt exactly these stages; no later stage runs after a Phase4D failure:

| Stage | Evaluation |
|---|---|
| 0 | Cancellation / internal entry invariant |
| 1 | Trust policy identity/version binding |
| 2 | DeveloperExceptionAuthorized legality |
| 3 | Channel × SignatureRequirement consistency |
| 4 | Envelope presence / authorized unsigned decision |
| 5 | Envelope signing-ID profile |
| 6 | ThemeManifest PublisherId ↔ Envelope PublisherId binding |
| 7 | Signature canonical Base64 / 64-byte P1363 representation |
| 8 | Trust snapshot signer-ID profile |
| 9 | Trust snapshot duplicate pair detection |
| 10 | Exact publisher/key pair resolution |
| 11 | Revocation state |
| 12 | Signer PublicKey canonical Base64 |
| 13 | SPKI structural profile |
| 14 | BCL key import / exact exported curve identity |
| 15 | Signed payload construction |
| 16 | Cancellation checkpoint |
| 17 | ECDSA P-256/SHA-256/P1363 verification |
| 18 | Post-verification cancellation checkpoint |
| 19 | Success outcome |

- Consequences: policy mismatch beats missing envelope; illegal exception flag beats channel inconsistency; channel inconsistency beats absence; missing required envelope beats later trust/crypto; invalid signing identity beats signature representation; representation beats trust lookup; snapshot invalid identity beats duplicate; duplicate beats target lookup; unknown pair beats revocation/SPKI/crypto; revoked pair beats SPKI/crypto; wrong SPKI profile beats signature verification. Verify only with an approved Trusted signer key.
- Upstream-only failures: P4I026 unsupported signature encoding is normally impossible after valid Phase4C success. Do not re-run Phase4A/4C raw enum/schema validation. An impossible internal Algorithm, SignatureEncoding, or SignedPayloadType contradicting validated Phase4C input is an internal invariant/programmer fault, not a newly reachable package diagnostic.

### Operation-local exceptions, cancellation, and BCL

- Narrow hostile-input handling: Signature/PublicKey Base64 FormatException, candidate-SPKI ASN.1 malformed-input exception, candidate-key-caused ImportSubjectPublicKeyInfo CryptographicException, and signature-verification CryptographicException after structural/key checks map to P4I013. Recognized valid DER with unsupported algorithm/curve/explicit parameters/compressed or hybrid point follows P4I016.
- Do not convert ObjectDisposedException, PlatformNotSupportedException, unexpected ArgumentException/InvalidOperationException, wrong fixed-API usage, or internal state corruption into package diagnostics. Propagate/fail-fast. No catch(Exception), and no whole-evaluator CryptographicException catch; catches must be operation-local.
- OperationCanceledException propagates; no cancellation P4I. Check at least before policy/trust work, before SPKI parse/import, immediately before VerifyData/VerifyHash, and immediately after verification before returning success/failure. Cancellation observed after crypto and before outcome return discards the result and propagates. Synchronous BCL crypto does not promise mid-call cancellation.
- BCL only: ECDsa, SHA256, System.Formats.Asn1, standard Base64 conversion, and DSASignatureFormat.IeeeP1363FixedFieldConcatenation. No new crypto NuGet. Planning must choose exactly one of VerifyData(payload, signature, SHA256, P1363) or SHA256(payload) plus VerifyHash(hash, signature, P1363), and plan exact golden-vector checks. No double hashing.

### Inputs, internal outcome, and downstream boundary

- Do not reopen Phase4C. Consume sealed ThemePackageMetadataEvaluation plus existing ThemeIntegrityVerificationPolicyV1, ThemeTrustSnapshotV1, and CancellationToken. No package-derived input is missing. Package-content reads=0: no ordinary payload, ThemeManifest, IntegrityManifest, or SignatureEnvelope reread.
- Approve only the minimal internal concept in Tcc.Themes.Integrity: internal static ThemePackageSignatureEvaluator and internal sealed record ThemePackageSignatureEvaluation. No extra handwritten helper type unless planning proves unavoidable. Evaluator has metadata, policy, snapshot, cancellation inputs; no reader, filesystem/path input, or required async.
- Outcome may contain only CanContinue, Diagnostics, existing ThemeSignatureVerificationStatus SignatureStatus, string? PublisherId, and string? KeyId. No new status enum.
- Signed cryptographically valid Trusted success carries exact validated envelope PublisherId/KeyId. Authorized unsigned Beta Optional absence or Developer Optional authorized absence has CanContinue=true, empty diagnostics, and null PublisherId/KeyId. Planning must derive the exact existing unsigned status members without inventing semantics; otherwise report PHASE4D OUTCOME REPRESENTABILITY GAP and block.
- Any Phase4D security failure has CanContinue=false, exactly one Phase4D diagnostic, and null PublisherId/KeyId. Planning must derive the applicable existing sealed failure status. Inspect and report every existing ThemeSignatureVerificationStatus member; if a precise mapping is impossible, block without adding enum members or silently expanding their meaning.
- No ThemeIntegrityVerificationResultV2, public IsVerified, public verification verdict, installation/activation/cache permission, ThemeIntegrityVerifier, or IThemeIntegrityVerifierV2 implementation belongs to this stage. Phase4E retains coherent public composition. Public ThemeIntegrityVerifier count remains zero.
- Evaluator is stateless; use and dispose an independent ECDsa instance per call. No static mutable crypto object, global mutable registry, key cache, or cross-request state.

### Planning closure and future-only implementation/test surface

- Re-evaluate all gaps A–N: signed payload framing, signature encoding, low-S, SPKI/curve, Phase4C input capability, trust, time/revocation, channel, Developer exception, Beta, diagnostics, precedence, BCL, and outcome representability. Do not force PASS. Return exact remaining contradictions to Supervisor; no automatic additional decision or implementation.
- Only if planning PASS, derive the minimal future surface: new src/Tcc.Themes/Integrity/ThemePackageSignatureEvaluator.cs containing both approved internal types; new tests/Tcc.Architecture.Tests/PhaseFourThemePackageSignatureEvaluatorTests.cs; exact approved-type-only update to PhaseThreeScopeBoundaryTests.cs; and CODEX_PROJECT_STATUS.md update only after future implementation validation PASS. No Tcc.Themes.csproj, schema, contract, ADR, or Phase4C change is authorized here.
- Future planned coverage must include literal signed-payload golden bytes; NUL collision/control rejection; 256-byte ID boundaries; Ordinal/case/NFC distinctions; manifest publisher binding; canonical signature Base64/P1363/DER rejection; low/high-S policy; canonical PublicKey Base64/91-byte DER/OIDs/full consumption/BIT STRING/uncompressed-versus-compressed/invalid point/imported-curve checks; policy identity/version; all signer IDs, duplicates, unknown/revoked pairs; exhaustive channel/requirement/exception matrix; present-signature no unsigned fallback; exact diagnostics and mixed-failure precedence; sensitive-message leakage; cancellation checkpoints/races; snapshot permutations, concurrency and zero reads; Phase4A/B/C regression; exact architecture surface; no Phase4E/public verifier leakage; and future Release x64/full required gates. These are plans only, not authorization to create or run tests.
- Completion of this governance append does not authorize implementation, status changes, build/test, stage, commit, tag, push, or Phase4E. If planning PASS, return to GPT Supervisor for separate Phase4D Implementation Authorization; otherwise return remaining gaps without creating another decision.

## TCC-DEC-2026-09-09-016 — Phase 4D Deterministic P-256 Public Point Validation and Platform Exception Boundary

- Date: 2026-09-09.
- Decision source: explicit GPT Supervisor EC Point / Platform Exception Compatibility Resolution supplied by the user in `39—Phase 4D Signature & Trust Implementation` after the Windows/.NET 10.0.11 invalid-point import reproduction.
- Authorization: append this decision only, perform a narrow compatibility proof outside the repository, and resume the already-authorized Phase4D implementation if and only if that proof passes. Decision011–015 remain byte-unchanged. No Phase4E, public ThemeIntegrityVerifier, Trading/Market Data, stage, commit, tag, or push is authorized.
- Reason: Windows BCL can wrap an invalid EC point import failure in PlatformNotSupportedException with an inner CryptographicException. Phase4D must establish mathematical point validity before provider import, preserving the distinction between invalid candidate material and platform capability failure.
- Stage refinement: Decision015 Stage13 becomes Stage13A SPKI structural/profile parsing followed by Stage13B deterministic affine P-256 public-point validation. Stage14 remains BCL ImportSubjectPublicKeyInfo and exact exported curve confirmation. All other stage ordering, channel, identity, signature, diagnostic, and outcome rules remain unchanged.
- Stage13A precondition: exact fully consumed DER SubjectPublicKeyInfo, id-ecPublicKey OID `1.2.840.10045.2.1`, named P-256 OID `1.2.840.10045.3.1.7`, zero unused BIT STRING bits, exactly 65 point bytes, and uncompressed prefix `0x04`. Stage13B reads the following 32-byte X and 32-byte Y as unsigned big-endian integers with BCL System.Numerics.BigInteger only.
- Exact field prime p: `FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF`. Exact curve b: `5AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B`. Require `0 <= X < p`, `0 <= Y < p`, and `Y * Y mod p == (X * X * X - 3 * X + b) mod p`, normalizing modulo results to non-negative form. Use exact integer arithmetic without floating point.
- Invalid candidate point: out-of-field X/Y or an off-curve point, including X=0/Y=0, fails at Stage13B before any provider import with P4I013, SignatureStatus=Invalid, Severity=Error, CanonicalPath=null, and exact message `The signature or signer public-key material is invalid.` Standard SEC1 infinity `0x00` is rejected by the existing point profile; do not invent another infinity representation.
- No subgroup multiplication check or custom scalar multiplication: P-256 has cofactor 1; a finite affine point on the exact curve plus existing profile and BCL import is sufficient for Phase4D v1. This arithmetic operates on public, fixed-size 256-bit coordinates, requires no constant-time implementation, and must not introduce secret-key cryptography or work/allocation proportional to package size.
- Stage14 exception rule: a PlatformNotSupportedException from import or a crypto provider always propagates, including when its InnerException is CryptographicException. Never unwrap or convert that platform wrapper. A CryptographicException directly thrown by the candidate ImportSubjectPublicKeyInfo call after Stage13B may map to P4I013 only in an operation-local catch attributable to that specific candidate import. No whole-evaluator catch, provider-message leakage, or broad exception conversion is authorized.
- Compatibility proof before implementation: golden valid point must pass both Stage13 substeps and import with bytesRead=91 and exact exported P-256 OID; X=0/Y=0, X=p, Y=p, and known off-curve single-bit X/Y mutations must fail before import; an independent known valid P-256 point must pass. Expected results must not be computed by calling a copy of the production point validator. PlatformNotSupportedException after valid-point proof must remain propagation-only, including its inner-CryptographicException form.
- Approved implementation surface remains exactly five candidate files: this existing ledger candidate; new `src/Tcc.Themes/Integrity/ThemePackageSignatureEvaluator.cs`; new `tests/Tcc.Architecture.Tests/PhaseFourThemePackageSignatureEvaluatorTests.cs`; exact two-type allowance in `tests/Tcc.Architecture.Tests/PhaseThreeScopeBoundaryTests.cs`; and `docs/CODEX_PROJECT_STATUS.md` only after all implementation validation passes. Point helpers are private static methods in the evaluator; exactly two handwritten production types remain approved. No new NuGet, project, ProjectReference, contract/schema/ADR change, or Phase4C modification.
- Tests must cover affine validity, X/Y bounds, off-curve mutations, the Windows zero-point reproduction, and provider avoidance where a direct seam permits. Do not add architecture complexity merely to mock ECDsa. If a provider-exception seam cannot be injected while preserving the two-type boundary, use a narrow external compatibility harness and disclose that limitation.
- Preservation and completion: record ledger before/after SHA-256 and Decision015 byte-range hash; require exactly one Decision016 heading. Frozen System/Theme, V1/V2, schemas, ADR, Decision011–015, Phase4A/B/C, phase4c-approved, and ThemeManifestValidator remain unchanged. Full prior implementation tests/build/regression gates plus compatibility closure are required before Phase4D Implementation PASS and Independent Validation readiness. This decision refines only Decision015 point-validation and candidate/platform exception separation; all other approved rules remain in force.

## TCC-DEC-2026-09-09-017 — Phase 4E Asset Evidence Projection and Phase 4F Independent Validation Boundary

- Date: 2026-09-09.
- Decision source: explicit GPT Supervisor Governance Gap Resolution + Planning Closure supplied by the user in `48—Phase 4E Scope Planning`.
- Current authorization: append exactly this one decision to this ledger, then perform Phase4E planning closure in the same conversation. No production, test, status, contract, schema, ADR, project, or Frozen artifact change; no restore/build/test, stage, commit, tag, push, or implementation is authorized by this task.
- Reason and effect: resolve the public AssetEvidence mapping gap and the Phase4E/Phase4F ownership and acceptance boundary. Decisions011–016 and all earlier ledger bytes remain unchanged. This decision supplies the previously missing rules without replacing sealed upstream stage semantics; it does not itself force planning PASS or authorize Phase4E implementation.

### Asset evidence projection

- `ThemeIntegrityVerificationResultV2.AssetEvidence` is derived only from `ThemePackageInventoryEvaluation.FileEvidence` produced by Phase4B. Membership is exactly `item.CanonicalPath.StartsWith("assets/", StringComparison.Ordinal)`. Include `assets/icon.png` and `assets/ui/panel.png`; exclude `Assets/icon.png`, `assets2/icon.png`, and `asset/icon.png`. Preserve the upstream canonical path without case folding, culture comparison, additional Unicode normalization, glob matching, or extension filtering.
- Preserve each matching complete `ThemeIntegrityFileEvidenceV1` item regardless of its existing lawful Phase4B evidence status, including where applicable Verified, MissingAllowed, MissingRequired, Unavailable, HashMismatch, LengthMismatch, and UnsupportedEntry. No Verified-only projection, new asset status, or reconstructed evidence. Use the exact existing V2 element type; no new evidence contract is introduced.
- AssetEvidence is a stable subsequence of B.FileEvidence. Preserve B ordering exactly; do not independently sort by path, filename, status, extension, or EntryKind. Do not apply a second filter using EntryKind, Theme Manifest asset declarations, metadata location, required/optional state, or hash verification result.
- If B was not executed, AssetEvidence is empty. If B returned evidence, project the evidence actually returned even when B.CanContinue=false. Preserve that same projection if C or D subsequently fails; later failure must not erase already-produced B evidence. Public FileEvidence retains the complete B collection; AssetEvidence neither replaces it nor removes entries from it.
- Eagerly materialize the projection as an immutable/stable collection using existing framework facilities, preferably `ImmutableArray<ThemeIntegrityFileEvidenceV1>`, without changing public contract types or adding dependencies. AssetEvidence package reads are zero: no package enumeration, asset read/hash, Theme Manifest reparsing, asset-inventory read, or filesystem discovery.

### Phase4E ownership and planning closure

- Phase4E owns the public `Tcc.Themes.Integrity.ThemeIntegrityVerifier`, its V2-only implementation, A-to-B-to-C-to-D orchestration and short-circuit composition, final public V2 result and IsVerified, diagnostics/evidence materialization, cancellation/exception composition, preservation of byte custody, direct/focused composition tests, real public V2 integration tests, architecture surface evolution, and full repository implementation gates. It does not independently approve its own candidate.
- Adopt the planning expression `IsVerified = Aaccepted && Baccepted && Caccepted && Daccepted` for the currently sealed lawful pipeline, including signed Trusted, Beta Optional authorized unsigned, and Developer Optional authorized unsigned success. Do not additionally require SignatureStatus.Valid or use an empty diagnostic collection as the sole verdict. Pass D.SignatureStatus through when D executes; otherwise use NotEvaluated. Preserve the approved early-result mappings, executed-stage diagnostic collection without E deduplication, Code/CanonicalPath/Message Ordinal ordering with null path first, and existing upstream custody. E uses the same caller reader for B/C and adds no direct package reads or catch blocks. The original-token terminal ThrowIfCancellationRequested before each public result return is approved. Other A/B/C/D behavior remains governed by the existing sealed sources.
- The public verifier remains a public sealed, parameterless, stateless V2-only class with the existing `ValueTask<ThemeIntegrityVerificationResultV2> VerifyAsync(ThemeIntegrityVerificationRequestV2 request, IThemePackageContentReader contentReader, CancellationToken cancellationToken = default)` contract. No V1 implementation, adapter, dual runtime, additional service dependency, parallel stage execution, or Task.Run.
- Only the compiler-generated async state machine of that exact public VerifyAsync may be introduced. Validate exact owner/method/signature, AsyncStateMachineAttribute target, declaring type, NestedPrivate, CompilerGenerated, and IAsyncStateMachine provenance. No unstable d__ ordinal hardcoding, namespace wildcard, or broad generated-artifact allowance.

### Phase4F independent validation and sealing

- Phase4F is separately authorized independent full integration/security validation of the exact Phase4E implementation candidate, not a production implementation phase. Normal authorization is READ, BUILD, TEST, ATTACK, REPORT only, with no repository modification.
- Independent coverage must include exact public V2 surface and real A/B/C/D composition; final verdict and signed/Beta/Developer successes; all early-failure families and public field/evidence mappings; diagnostics/order; reads and TOCTOU/custody; cancellation/exception propagation; determinism/concurrency; generated provenance; V1=0 and exact V2=1; full build/test/architecture gates; Frozen/sealed preservation; and candidate scope/Git hygiene. Validation artifacts must not silently enter the repository candidate.
- On a Phase4F defect, stop and report the defect, severity, evidence, and minimal remediation direction. Do not silently fix it. A separately authorized remediation task performs any fix, followed by separately authorized re-validation to close the defect.
- Phase4E may report IMPLEMENTED / VALIDATED only after its authorized implementation tests and mandatory repository gates pass. It remains an unsealed implementation candidate, not SEALED / APPROVED. Only after independent Phase4F PASS may Supervisor authorize a Phase4E Baseline Staging Audit and then a separately authorized Phase4E Baseline Seal. Do not seal before Phase4F PASS.

### Future-only exact implementation surface

- Adopt exactly nine repository files for the future Phase4E implementation candidate; this governance append is separate and does not become a tenth implementation file:
  1. NEW `src/Tcc.Themes/Integrity/ThemeIntegrityVerifier.cs`.
  2. NEW `tests/Tcc.Architecture.Tests/PhaseFourThemeIntegrityVerifierCompositionTests.cs`.
  3. MODIFY `tests/Tcc.Architecture.Tests/PhaseThreeScopeBoundaryTests.cs`.
  4. MODIFY `tests/Tcc.Architecture.Tests/PhaseTwoContractCompletenessTests.cs`.
  5. MODIFY `tests/Tcc.Architecture.Tests/PhaseFourContractAmendmentTests.cs`.
  6. MODIFY `tests/Tcc.Architecture.Tests/PhaseFourThemeIntegrityVerifierTests.cs`.
  7. MODIFY `tests/Tcc.Architecture.Tests/PhaseFourThemePackageInventoryEvaluatorTests.cs`.
  8. MODIFY `tests/Tcc.Architecture.Tests/PhaseFourThemePackageSignatureEvaluatorTests.cs`.
  9. MODIFY `docs/CODEX_PROJECT_STATUS.md`.
- The six existing test files may change only during separately authorized future implementation to evolve obsolete phase-scoped V2=0 guards into exact approved V2=1, preserving V1=0, other V2=0, no adapter/fallback, internal A/B/C/D shapes, generated provenance, dependency/Frozen/contract invariants, and no final-verdict ownership in earlier stages. Do not delete/skip tests, broadly weaken assertions, or allow arbitrary verifier types.
- No tenth implementation file, contract/schema/ADR/csproj change, new project/reference/NuGet dependency, or DI registration. If any becomes necessary, stop and return a governance/capability gap. Status may change only after future implementation validation passes; then correct stale Phase4D seal-in-progress wording to the actual SEALED / APPROVED state and existing phase4d-approved tag. Do not change status in this planning task.
- Re-evaluate planning gaps A–P against this resolution and the sealed sources; A and O are resolved by the rules above, but remaining correctness/security gaps must still block. Planning PASS only permits return to Supervisor for explicit Phase4E Implementation Authorization. No automatic Decision018, implementation, Phase4F execution, Trading/Market Data work, or Git publication is authorized.

## TCC-DEC-2026-09-09-018 — Phase 5 Compatibility Negotiation Scope Split and Deterministic Version Semantics

- Date: 2026-09-09.
- Decision source: explicit GPT Supervisor rulings supplied by the user in `54—Phase 5 Scope Planning`, including the Phase5 scope-split instruction and the later `PLANNING CLOSURE — RESOLVE G1/G2/G3 AND APPEND DECISION018` instruction.
- Current authorization: append exactly Decision018 to this ledger on the already-created `codex/phase5-theme-compatibility-resolver` branch and complete planning closure. The entry HEAD is `3799cc6f90f6d4068e3dec39167612513d76dc43`; annotated `phase4e-approved` object is `21d048a57125bc3ac38b959f456b10517efabae0`, peeled to that same HEAD. Do not recreate the branch, modify the old Phase4 branch, change status/source/tests/contracts/schemas/ADRs/csproj, stage, commit, tag, push, or implement Phase5A in this task. Decisions001–017 remain byte-preserved; Decision019 is not authorized.
- Reason and effect: close the previously reported G1 required-range semantics, G2 canonical request-version grammar, and G3 UX display/machine normalization boundary. These are explicit later human rulings, not claims that unconstrained sealed string fields already supplied the complete grammar. No approved/Frozen artifact or sealed contract is rewritten. The ruling resolves the Phase5A planning proposals; it does not silently revise earlier production behavior or force a planning PASS if another material gap is found.

### Workstream and phase split

- Phase5 is the Theme Compatibility Resolver workstream. Approved Theme Architecture section 33.1 sequencing remains `ThemeManifestValidator -> ThemeIntegrityVerifier -> ThemeCompatibilityResolver -> ThemeCapabilityGate -> Theme Library metadata store`. The sealed public Theme Integrity pipeline therefore precedes Phase5. Theme Runtime and Trading Intelligence remain separate tracks.
- Phase5A is **Theme Compatibility Version Negotiation Foundation**, an internal deterministic calculation layer only. It owns the input validation needed for version negotiation, Manifest Schema compatibility, current Core version compatibility, Theme API and UX Contract negotiation, numeric comparison, accepted-set intersection, deterministic version failures, and a stable immutable internal outcome.
- Phase5A implements no `IThemeCompatibilityResolver`, returns no public `ThemeCompatibilityResult`, and adds no public type or public enum. Public compatibility resolver implementations remain exactly zero during 5A. Its result must not declare a package Compatible, Installable, Activatable, Safe, Accessible, MigrationReady, or RollbackReady.
- Phase5B is **Full Public ThemeCompatibilityResolver Composition**, deferred to a separate planning/authorization gate. It later owns `IThemeCompatibilityResolver` and the complete public result only after integrity evidence linkage, platform/mode, capability and accessibility evidence, migration/rollback evidence, full failure mapping, and admission semantics have explicit approval. Do not automatically start 5B after 5A.
- Phase5C is **Independent Phase5 Validation**. Implementation and self-tests are not independent approval. Independent audit, separately authorized remediation if needed, re-validation, staging audit, and separately authorized seal remain distinct gates.
- Phase5A requires zero public contract, schema, ADR, csproj, project, ProjectReference, or dependency changes. `Tcc.Themes` owns the implementation and continues to depend only on `Tcc.Presentation.Contracts`; `Tcc.DesktopHost` remains the sole composition root. Existing friend access to `Tcc.Architecture.Tests` is sufficient. If implementation proves an additional boundary/change necessary, stop and report a governance gap rather than silently amending it.

### Canonical numeric versions and required-range grammar

The following six fields use exactly the same grammar:

1. `ThemeManifest.compatibility.required_core_version`.
2. `ThemeManifest.compatibility.required_theme_api_version`.
3. `ThemeManifest.compatibility.required_ux_contract_version`.
4. `ThemeCompatibilityManifest.core.required`.
5. `ThemeCompatibilityManifest.theme_api.required`.
6. `ThemeCompatibilityManifest.ux_contract.required`.

```text
number   := "0" | [1-9][0-9]*
version  := number "." number "." number
operator := ">=" | "<=" | ">" | "<" | "="
clause   := operator? version
range    := SPACE* clause (SPACE+ clause)* SPACE*
SPACE    := U+0020 only
```

- All clauses have logical AND semantics. A clause without an operator means exact equality. `1.0.0`, `=1.0.0`, `>=1.0.0 <2.0.0`, and `>=1.1.0 <=1.9.9` are syntactically valid. Operators are adjacent to their version token. Leading/trailing/repeated ASCII spaces are allowed only by the range grammar; an empty or all-space range is invalid.
- Reject prerelease, build metadata, wildcards, caret, tilde, OR, hyphen ranges, commas, tabs, newlines, Unicode whitespace/digits, numeric sign characters, v-prefixes, two/four-component versions, and leading-zero components. Do not normalize them into acceptance. Rejected examples include `v1.0.0`, `1.0`, `1.0.0-alpha`, `1.0.0+build`, `^1.0.0`, `~1.0.0`, `1.*`, `>=1.0.0 || <2.0.0`, `1.0.0 - 2.0.0`, and `01.0.0`.
- `ThemeCompatibilityRequest.CoreVersion` and every element of `SupportedThemeApiVersions` and `SupportedUxContractVersions` must be exact canonical `X.Y.Z` machine versions. No leading/trailing whitespace, Trim, alias expansion, v-prefix handling, or automatic component padding occurs inside Phase5A.
- Validate numeric-token grammar before numeric conversion. Use BCL `System.Numerics.BigInteger` for components, with no artificial Int32/Int64 maximum and no new dependency. Compare major, then minor, then patch numerically; never use floating point, decimal, full-version string lexical ordering, or culture-sensitive parsing. `1.10.0 > 1.9.0` and `18446744073709551616.0.0 > 18446744073709551615.0.0`.
- Required/optional status remains that of the applicable sealed schema and the version inputs consumed by this primitive. Missing required version data fails; optional unrelated data is not promoted into a new Phase5A requirement. This is not authority to validate platform/accessibility admission or reimplement the full Manifest Validator.

### Accepted sets, satisfiability, and declaration consistency

- A valid range denotes the set of canonical nonnegative integer triples satisfying every clause. Normalize its meaning using tightest lower/upper bounds and inclusivity plus any equality constraint. Repeated constraints, clause ordering, and legal ASCII-space repetition do not change the accepted set.
- Equality and emptiness concern this discrete version domain, not a dense real-number interval. A useful derived implementation model is the half-open interval `[L,U)`, with minimum `0.0.0`, optional unbounded upper endpoint, and successor of `(a,b,c)` equal to `(a,b,c+1)`. Strict lower bounds and inclusive upper bounds can use that successor; equality becomes `[v,successor(v))`. This is a calculation representation of the approved accepted-set rule, not an added version grammar or resource limit. In particular, `>1.0.0 <1.0.1` is empty, `>1.0.0` equals `>=1.0.1`, and `<0.0.0` is empty.
- A syntactically valid but empty accepted set produces internal `UnsatisfiableVersionRange`, distinct from lexical/grammar `InvalidVersionInput`. Required examples are `>=2.0.0 <1.0.0`, `>1.0.0 <1.0.0`, `=1.0.0 =2.0.0`, and `=1.0.0 >1.0.0`.
- For each Core/Theme API/UX dimension, both document declarations must parse and be individually satisfiable before comparing them. A malformed declaration produces `InvalidVersionInput`; an unsatisfiable declaration produces `UnsatisfiableVersionRange`. Do not additionally classify that invalid pair as a declaration conflict.
- Two valid satisfiable declarations must denote **exactly the same accepted version set**. An intersection or subset/superset relationship is insufficient. Text equality is unnecessary. `>=1.0.0 <2.0.0` equals `<2.0.0 >=1.0.0`; it does not equal `>=1.5.0 <2.0.0`. Non-equivalence produces `ConflictingVersionDeclaration`. Never prefer one document or silently narrow/merge conflicting ranges.

### Display, machine, and schema identifiers

- Display Version is not Machine Compatibility Version. Approved UX display mapping `v1.1 -> 1.1.0` belongs exclusively to an explicit presentation/import/serialization boundary before the request or manifest machine value enters Phase5A. Phase5A directly rejects both `v1.1` and `1.1` with `InvalidVersionInput`. Do not Trim, strip/prepend v, append `.0`, or generalize the mapping to arbitrary two-component values. This task creates no such boundary implementation.
- Both Theme Manifest and Compatibility Manifest schema identifiers remain exactly `"1.0"`; these are not canonical numeric machine versions. Do not pass them through the X.Y.Z parser or convert them to `1.0.0`. An invalid/unsupported schema is a hard stop before interpreting schema-dependent compatibility declarations. Caller support claims cannot enable unknown schema semantics.

### Negotiation and production support authority

- CoreVersion is one exact current version, which must satisfy the agreed Core range. Otherwise emit `CoreVersionIncompatible`. There is no nearest-version, tested-version, upgrade, downgrade, or other fallback.
- For Theme API and UX, validate every application-supported machine token, deterministically remove numerically equivalent exact tokens, evaluate against the agreed range and applicable sealed support authority, then select the numerically highest common supported version. Input enumeration order never affects the result. An empty common set yields null for that selected dimension, `CanContinue=false`, and `ThemeApiNoCompatibleVersion` or `UxContractNoCompatibleVersion`.
- Decision006 and `ContractVersions.ThemeApi` fix currently authorized production Theme API support to `{ "1.0.0" }`. Caller input cannot widen it. Planning treatment is to validate all caller tokens first, then intersect canonical caller Theme API tokens with that sealed set before range selection; unsupported claims are never selected, and no surviving common version produces `ThemeApiNoCompatibleVersion`. Malformed tokens must not be silently discarded by this intersection.
- UX support inputs are canonical machine versions subject to all approved architecture/ContractVersions constraints; the current sealed UX contract identifier is `1.1.0`. Display copy does not authorize support. Algorithmic multiple-version fixtures are not production support registration or permission to add a new UX contract. No production support configuration changes are authorized by this decision.
- `tested[]` is informational compatibility evidence only. It does not add versions to either application support set or widen a required range. Never fall back to tested versions to rescue a failed negotiation.

### Failures, internal surface, and snapshots

- Failure precedence is: (1) Manifest Schema; (2) malformed/unsatisfiable declarations and valid dual-document conflicts; (3) Core Version; (4) Theme API; (5) UX Contract. Invalid/unsupported schema hard-short-circuits. Where valid inputs allow safe independent evaluation, collect all independent Core/API/UX failures in that order rather than arbitrarily stopping after a Core failure. Never evaluate a dimension through its own malformed/empty/conflicting requirement.
- Authorized internal failure kinds are `InvalidVersionInput`, `UnsatisfiableVersionRange`, `UnsupportedManifestSchema`, `ConflictingVersionDeclaration`, `CoreVersionIncompatible`, `ThemeApiNoCompatibleVersion`, and `UxContractNoCompatibleVersion`. No public enum changes and no P3M/P4I diagnostic reuse.
- Target exactly three handwritten internal top-level types in `Tcc.Themes.Compatibility`: static `ThemeCompatibilityVersionNegotiator`, sealed record `ThemeCompatibilityVersionNegotiation`, and enum `ThemeCompatibilityNegotiationFailureKind`. No additional service/factory type is needed. The planning entry signature is `internal static ThemeCompatibilityVersionNegotiation Negotiate(ThemeCompatibilityRequest request)`. It is synchronous; no Task.Run, worker, async service, or DI registration is introduced.
- The minimal internal outcome holds only `bool CanContinue`, nullable `SelectedThemeApiVersion`, nullable `SelectedUxContractVersion`, and `ImmutableArray<ThemeCompatibilityNegotiationFailureKind> Failures`. Use existing framework tuple/private-method facilities for numeric/range computations rather than introducing speculative public abstractions. Additional Core/Manifest version evidence is permitted only if demonstrably required for deterministic testing; no admission, accessibility, migration, or rollback flags are allowed.
- Phase5A eagerly snapshots caller-owned supported-version collections and any other collections it actually consumes. Do not reread them after acquisition. Caller collections must remain stable during acquisition; the implementation does not promise safety against concurrent mutation while copying. Output collections are immutable/stable. No dependency is added for snapshots.
- Require stateless operation, no mutable static state/cache/clock/RNG/locale dependency, same semantic input to same semantic output, and independent concurrent calls without state leakage. Semantic failure order must not depend on collection enumeration; internal repeated failure kinds must not obscure the requirement to collect independent dimension failures.

### Security, deferred responsibilities, and future implementation files

- Phase5A does not call `ThemeIntegrityVerifier`, use a package reader, read/extract/install/promote/register/activate packages, persist Library/state, or acquire filesystem custody. `CanContinue` means version negotiation only, not `IsVerified`, Compatible, Installed, or Activatable.
- Future Phase5B/lifecycle admission requires the real sealed V2 `ThemeIntegrityVerifier` result with `IsVerified=true` and controlled custody linking that same verified content through compatibility to promotion/registration. Phase4 evidence is not a filesystem lock. Changed underlying content requires the lifecycle owner to rerun the sealed verifier. Do not implement V1 adapters, duplicate integrity logic, mutate trust snapshots, or bypass the existing gate.
- Platform/Installer/Portable admission, DPI/text scale/zoom, capability policy, accessibility/Safety Core validation, asset/font/audio/runtime behavior, migration/rollback availability or execution, Safe Mode Core, Windows integration, persistence, Trading, Market Data, and AI are excluded from 5A. Theme presentation cannot own business, permission, risk, audit, or AI decision authority. Gu Qinghan remains a downstream Theme Package; Q93, Home Safety Core, GPT optionality, and Installer/Portable parity remain mandatory.
- Preserve future BreakoutProp original coin set and `OKX / USDT.P -> Kraken / USD.PM` migration without changing that set, with per-coin liquidity line charts at 15M/30M/1H/4H/1D/1W/1M. None is implemented here. Trading Intelligence remains independent and cannot introduce autonomous execution or broker/exchange/prop-firm write APIs.
- Proposed future Phase5A candidate is exactly five files: NEW `src/Tcc.Themes/Compatibility/ThemeCompatibilityVersionNegotiator.cs`; NEW `tests/Tcc.Architecture.Tests/PhaseFiveThemeCompatibilityNegotiationTests.cs`; MODIFY `tests/Tcc.Architecture.Tests/PhaseThreeScopeBoundaryTests.cs`; MODIFY `docs/CODEX_PROJECT_STATUS.md`; plus this existing governance candidate `docs/CODEX_DECISIONS.md`. Do not create or modify the first four in this task.
- Future guards must allow only the exact approved internal surface and preserve public compatibility implementations=0, integrity V1=0 and exact V2=1, sealed production/contract/schema/Frozen boundaries, no filesystem/Windows/package-reader/Trading/AI/DI leakage, and the existing 6-project/11-ProjectReference/4-PackageReference graph. Do not remove/skip tests or broadly weaken generated-code/type guards.
- Future tests must cover exact schema IDs, canonical/invalid tokens, operator/range boundaries, discrete empty ranges, semantic equality/conflict, highest authorized overlap/no-overlap, tested non-rescue, multi-dimension failures, BigInteger values, en-US/tr-TR/zh-TW, permutations, immutable snapshots, concurrency, no side effects, and compiled public resolver count zero. Pure numeric multi-version test vectors must not widen actual production support.
- Future implementation gates remain focused tests, required architecture/integration/regression tests, locked restore, Release x64 build, all tests, Frozen hash verification, sealed/scope preservation, and Git hygiene. The current ledger-only closure uses append-prefix/hash/diff/scope/baseline verification; it does not claim a newly executed production build or runtime validation.
- Planning PASS permits return to Supervisor for explicit `55—Phase 5A Compatibility Negotiation Implementation` authorization only. It does not start implementation, self-approve a candidate, authorize 5B/5C execution, or authorize commit/tag/push/seal.

## TCC-DEC-2026-09-10-019 — Additive Phase5B Compatibility V2 Contracts and Trusted Evidence Composition

- Date: 2026-09-10.
- Decision source: approved TCC-P5B-65 design, explicit TCC-P5B-66 Candidate A implementation authorization, and its later Supervisor Supplemental Contract Authority S1–S9.
- Status: APPENDED / UNSEALED CANDIDATE. The authorized design is recorded here; independent contract validation and sealing remain separate gates.
- Reason: preserve V1 while describing missing/failed compatibility facts truthfully and preventing ordinary callers from fabricating trusted runtime evidence.
- Scope: additive V2 runtime API ownership in Tcc.Themes and portable description/result ownership in Tcc.Presentation.Contracts.
- Supersedes: no historical decision text or Phase5A semantics; this narrowly supplements Phase2 ownership and Decision018's deferred Phase5B design. Decisions001–018 remain byte-preserved.

### Locked Supervisor Decisions

A:
V1 完整保留；新增獨立 V2 family。

B:
Caller-constructible IsVerified=true record 不構成 provenance。
必須經過受控 binder 及不透明 context。

C:
未選定、未評估、不適用或未知值使用明確狀態及 null；
不使用空字串、假版本或 caller backfill。

D:
Resolver 組合 owner evidence；
不執行 capability、accessibility、Safety、migration、rollback policy。

E:
Phase5A 三個 internal algorithm types、演算法及 Decision018 保留。
V1 resolver implementations=0。
V2 contract amendment 階段 implementations=0。
後續批准的 resolver implementation candidate 完成時：
V2 implementations=1 exact；第二個 implementation 拒絕。

Conformance: PASS

### Recommended Architecture

Single recommended design: B。

公開 V2 runtime API、request及不透明 trust objects：
Tcc.Themes.Compatibility.V2，assembly Tcc.Themes。

穩定 environment/result/failure/notice DTO與 enums：
Tcc.Presentation.Contracts.Theme，assembly Tcc.Presentation.Contracts。

受控內容取得、verifier composition及 materialization：
Tcc.Themes.Compatibility.Binding，internal。

依賴維持：
Tcc.Themes → Tcc.Presentation.Contracts。

Why:

1. 不透明 context 的 internal constructor 位於 verifier/binder 所在 assembly。
2. Contracts 不需要反向引用 Tcc.Themes。
3. Theme Architecture §§3.1、3.4、4.2 允許 Theme runtime/API
   由 Tcc.Themes 擁有，穩定 presentation data 留在 Contracts。
4. 保留原 Tcc.Themes.Compatibility namespace，
   可完整保留 Phase5A 原本的三型別及功能測試。
5. 不需新 project、PackageReference、ProjectReference 或 friend assembly。

治理說明：

Phase2 ADR 將當時的公開 Theme contracts 放在 Presentation.Contracts。
本設計需要 Decision019及新的 additive ADR，明確批准
「具有受控建構要求的 V2 runtime API」的窄例外。
這不是宣稱原 ADR 已經包含該例外，也不修改原 ADR。

### Trusted Evidence Boundary

Type:
Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2

Owner:
Tcc.Themes。

Visibility/kind:
public sealed class；非 record；無 public/protected constructor；
無 public setter/init、clone、with、token-import或 FromResult 方法。

Construction:
internal constructor，只有受信任 assembly 程式可呼叫。
Production 唯一建構 owner：
Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder。

Context 代表「受控取得及綁定作業的結果」。
它可以攜帶成功內容，也可以攜帶真實 acquisition/refusal/schema failure；
其存在本身不等於 Compatible，亦不保證 IsVerified=true。

Context 的 internal、get-only內容：

- ThemePackageRef PackageRef
- ThemeIntegrityVerificationResultV2? Integrity
- ThemeManifest? Manifest
- ThemeCompatibilityManifest? Compatibility
- string? CompatibilityManifestHash
- ImmutableArray<ThemeCompatibilityFailureV2> BindingFailures

成功完整綁定時：
Integrity、兩份 manifests、CompatibilityManifestHash 均非 null，
Integrity.IsVerified=true，BindingFailures 為空。

失敗時：
只保留實際已取得的證據，未產生資料為 null；
BindingFailures 說明真正失敗階段。
不得構造不一致的成功內容。

Fabrication resistance:

普通外部 caller 無法：
- new context；
- 繼承 context；
- 將 V1或自行建立的 V2 result 轉成 context；
- 透過 public factory提交 hash/DTO以取得信任；
- 反序列化成合法可消費的 trusted context。

Threat model:

支持的防護目標是一般受支持 API 使用、惡意 package data、
外部 caller 自行建立 DTO及混接合法物件。

不涵蓋：
reflection/private-member access、unsafe memory mutation、
runtime patching、惡意替換已載入 assembly、
冒充現有 test friend assembly或任意受信任程式碼執行。

Tcc.Themes及既有 Tcc.Architecture.Tests friend 是明確 TCB。
本設計不新增 InternalsVisibleTo，也不把 internal 宣稱為進程隔離。

Lifecycle relationship:

未来 ThemeApplicationService/lifecycle orchestration，在 Tcc.Themes 內
呼叫 binder；DesktopHost 仍是唯一組態/DI composition root。

Binder 是固定資料流程元件，不能註冊服務、查 service locator、
選取任意 verifier/provider或成為第二個 composition root。

目前尚無公開 lifecycle issuance入口。
契約階段不可新增接受任意 DTO的捷徑來補上它。

### Same-Content Binding

唯一 production 路徑：

1. 可信 orchestration 提供 Phase4 request及 package reader。
   Policy、TrustSnapshot 必須由其核准 owner提供；
   不接受 Theme package自行指定信任根。

2. 深複製 request 的 nested inventory、policy、trust collections。
   以一次作業專用內容快照取得 package bytes。

3. 新增 internal ThemeCompatibilityContentSnapshot：
   - 實作既有 IThemePackageContentReader。
   - 保存本次取得的完整 enumeration records。
   - 對合法 regular-file logical paths取得並複製 bytes。
   - duplicate/unsupported enumeration資訊不可正規化或刪除。
   - 不合法 path不得送入底層 ReadContentAsync。
   - 取得失敗即產生 acquisition failure。
   - bytes及 lookup structure 完成後不可修改。
   - 不暴露 backing byte[]。

4. 同一份 immutable snapshot reader交給真正的、
   封存 ThemeIntegrityVerifier.VerifyAsync，恰好一次。
   不接受 caller提交的 verification result替代此呼叫。

5. 只有真實 IsVerified=true才繼續綁定。
   Beta及 Developer合法 unsigned成功仍接受，
   不額外要求 SignatureStatus.Valid。

6. Theme Manifest從快照內同一份 raw bytes materialize。
   比對該 bytes的 SHA-256、長度、ThemeManifestHash及對應 FileEvidence。

7. Compatibility Manifest固定 logical path為 compatibility.json：
   - 必須有唯一、存在且 Verified的實際 FileEvidence。
   - ActualLengthBytes及 ActualSha256必須對應快照 bytes。
   - 執行既有 schema後，從相同 bytes materialize。
   - ThemeId/Version與已綁定 Theme Manifest必須一致。

8. PackageHash直接沿用這次真實 verifier result。
   Binder不另寫 Canonical Package Tree Hash演算法。

9. Context保存深度不可變 DTO/evidence，不保存 raw package bytes。
   作業完成後，snapshot不由 resolver或全域狀態持有。

重要區別：

- Hash comparison只用來核對 binder自行保管、
  已送入真實 verifier的 bytes。
- 不是比較兩組 caller自行提供的 hash字串。
- ThemeId/Version equality是附加一致性檢查，不是信任根。
- PackageRef是來源脈絡，不是 proof。
- 此作業專用 immutable content image不是 provenance registry/cache。

Package A＋Package B DTO攻擊：

不存在公開 DTO輸入槽。
兩份 DTO均由 binder從同一份被 verifier消費的快照產生，
普通受支持 API无法混入 caller的 Package B DTO。

內容變更：

Context證明特定歷史快照。
不表示來源路徑被永久鎖定或目前仍是同一內容。
後續 lifecycle若要使用不同/變更內容，必須重建快照並重新驗證。

### Compatibility Manifest Materialization

Owner:
internal static ThemeCompatibilityManifestMaterializer，
namespace Tcc.Themes.Compatibility.Binding。

Raw bytes:
只使用 ThemeCompatibilityContentSnapshot 中的 compatibility.json。
不另開 filesystem，不向原 reader再次讀取。

Schema validation:
呼叫既有 ThemeMetadataSchemaValidator.Validate，
新增 embedded resource：
Tcc.Themes.Schemas.ThemeCompatibility.schema.json。

Schema source保持原檔：
contracts\theme\schemas\ThemeCompatibility.schema.json

SHA-256:
3278CC92A93AF636380739A34063F2A889E63AB89E0A56132F768836E126B2BC

現有 validator已提供指定 resource name的入口；
無須修改 sealed ThemeMetadataSchemaValidator。

Deserialization:
使用 ThemeContractJson既有設定，
schema成功後 materialize既有 ThemeCompatibilityManifest。
拒絕重複 JSON properties、unknown properties、缺少欄位及型別錯誤。

版本 range語意仍交給 Phase5A；
materializer不複製 required-range parser。

固定分類：
- 文件不存在/無 Verified evidence/bytes不符：trusted-evidence failure。
- 可辨識但不支援的 schema identifier：UnsupportedManifestSchema。
- 支援 schema之 JSON/結構驗證失敗：ManifestSchemaInvalid。
- materialization失敗：ManifestSchemaInvalid。
- identity/version與同一 package不符：trusted-evidence failure。
- required range語法錯誤：後續 Phase5A InvalidVersionInput。

若 Theme Manifest已在 Phase4失敗，維持 integrity refusal；
不跳過 Phase4以改判一個較漂亮的 compatibility schema狀態。

Required changes:
- New internal production types: YES，於後續 implementation candidate。
- New public manifest DTO: NO。
- New schema contract: NO。
- JSON schema changes: NO。
- Tcc.Themes.csproj新增一項 EmbeddedResource: YES。
- PackageReference/ProjectReference changes: NO。

### V2 Public Surface

下列為完整新型別清單。

共同規則：

C namespace = Tcc.Presentation.Contracts.Theme
C assembly = Tcc.Presentation.Contracts

R namespace = Tcc.Themes.Compatibility.V2
R assembly = Tcc.Themes

所有 DTO constructor皆要求完整欄位；
集合須 eager immutable snapshot，不提供成功預設值。
可序列化資料不具有 trusted provenance。

| Type | Location | Visibility/kind | Construction | Purpose |
|---|---|---|---|---|
| IThemeCompatibilityResolverV2 | R | public interface | 無 | 同步 V2 API |
| ThemeCompatibilityRequestV2 | R | public sealed class | public完整 constructor | 明確 request |
| ThemeCompatibilityContextV2 | R | public sealed class | internal | acquisition/content provenance |
| ThemeCompatibilityEvidenceV2 | R | public sealed class | internal | owner evidence bundle |
| ThemeCompatibilityEnvironmentV2 | C | public sealed class | public完整 constructor | 不可變 target environment |
| ThemeCompatibilityRuntimeTargetV2 | C | public sealed record | public完整 constructor | 單一 surface/profile/viewport target |
| ThemeCompatibilityResultV2 | C | public sealed class | public完整 constructor | 穩定結果資料 |
| ThemeCompatibilityFailureV2 | C | public sealed record | public完整 constructor | 固定 failure detail |
| ThemeCompatibilityNoticeV2 | C | public sealed record | public完整 constructor | 非阻擋降級資訊 |
| ThemeCompatibilityStatusV2 | C | public enum | 不適用 | primary status |
| ThemeCompatibilityFailureKindV2 | C | public enum | 不適用 | failure kind |
| ThemeCompatibilityDimensionV2 | C | public enum | 不適用 | failure category |
| ThemeCompatibilityEvaluationStateV2 | C | public enum | 不適用 | 未評估/拒絕/已評估 |
| ThemeCompatibilityEvidenceStatusV2 | C | public enum | 不適用 | owner evidence狀態 |
| ThemeCompatibilityAccessibilityStatusV2 | C | public enum | 不適用 | accessibility狀態 |
| ThemeCompatibilitySafetyStatusV2 | C | public enum | 不適用 | sealed Safety語意＋未評估 |
| ThemeCompatibilityInstallationModeV2 | C | public enum | 不適用 | Installer/Portable |
| ThemeCompatibilityOperationV2 | C | public enum | 不適用 | package/transition評估目的 |
| ThemeCompatibilityNoticeKindV2 | C | public enum | 不適用 | 固定 notice語彙 |
| ThemeCompatibilityJsonV2 | C | public static class | 無 instance | V2 result專用序列化 |

Internal evidence types：

以下六個型別均為 R內的 internal sealed class，
internal完整 constructor、get-only fields；
僅由該責任的核准 production owner或 trusted test fixture建構：

- ThemeCompatibilityRuntimeEvidenceV2
- ThemeCompatibilityCapabilityEvidenceV2
- ThemeCompatibilityAccessibilityEvidenceV2
- ThemeCompatibilitySafetyEvidenceV2
- ThemeCompatibilityMigrationEvidenceV2
- ThemeCompatibilityRollbackEvidenceV2

每份 receipt都有：
- ThemeCompatibilityContextV2 Context
- ThemeCompatibilityEnvironmentV2 Environment

Reference binding為精確同一 context/environment物件。
這是明確物件資料流，不查 token table。

後續 implementation types：

- Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder
  internal static class。
- Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot
  internal sealed class，private constructor。
- Tcc.Themes.Compatibility.Binding.ThemeCompatibilityManifestMaterializer
  internal static class。
- Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2
  public sealed class，public parameterless constructor。

除編譯器產生且經精確來源驗證的 artifacts外，
不新增其他 production helper types。

### V2 Request

精確 API：

ThemeCompatibilityResultV2 Resolve(
    ThemeCompatibilityRequestV2 request);

Request只有三個 get-only properties：

| Field | Type | 必要性/nullable | Owner |
|---|---|---|---|
| Context | ThemeCompatibilityContextV2? | 必要；null可表示缺少輸入 | trusted binder |
| Environment | ThemeCompatibilityEnvironmentV2? | 必要；null導致 refusal | DesktopHost及 lifecycle/context owners |
| Evidence | ThemeCompatibilityEvidenceV2? | 最終成功必要；null表示尚未取得 | trusted owner-evidence composition |

request本身為 null：
ArgumentNullException，屬 API呼叫錯誤。
缺少其可空欄位則回傳結構化結果，不丟一般驗證例外。

Environment精確 fields：

| Field | Type | Nullability/規則 |
|---|---|---|
| CoreVersion | string? | 必要；null由 Phase5A判 invalid |
| SupportedThemeApiVersions | ImmutableArray<string?>? | 必要；保留 null及 malformed tokens |
| SupportedUxContractVersions | ImmutableArray<string?>? | 同上 |
| SupportedManifestSchemaVersions | ImmutableArray<string?>? | 必要；依本決策專屬規則 |
| Platform | string? | 必要；production canonical value為 windows |
| Architecture | string? | 必要；目前只支援 x64 |
| InstallationMode | ThemeCompatibilityInstallationModeV2 | 必要；不得為 NotSpecified |
| RuntimeBuildId | string | 必要；host受控 build identity |
| ValidationProfileVersion | string | 必要；核准驗證 profile版本 |
| RuntimeTargets | ImmutableArray<ThemeCompatibilityRuntimeTargetV2> | 必要；非空 |
| Operation | ThemeCompatibilityOperationV2 | 必要；不得為 NotSpecified |
| CurrentThemeId | ThemeId? | transition需要時由 lifecycle提供 |
| CurrentThemeVersion | ThemeVersion? | 與 CurrentThemeId成對 |
| ThemeStateRevision | string? | 涉及既有 theme-owned state時必要 |

此 environment是評估目標資料，不是驗證證據。
外部 caller可以建立另一個 environment，
但不能因此取得與它相符的 trusted receipts。

RuntimeTarget精確 fields：

- string TargetId
- ThemeVariantId Variant
- UxSurfaceId Surface
- decimal DpiScale
- decimal ViewportWidthDip
- decimal ViewportHeightDip
- ThemeAccessibilityProfile AccessibilityProfile

DpiScale、viewport dimensions、profile.TextScale/profile.Zoom必須 >0。
TargetId唯一且只為 logical identifier。
Profile沿用既有全部欄位，包括 contrast、color vision、
reduced modes、audio controls、keyboard、screen reader。

Snapshot semantics：

- Environment constructor深複製所有集合及 runtime targets。
- Request捕捉的是不可變 context/environment/evidence references。
- Receipt產生後不得重新指向別的 environment。
- Evidence bundle內所有 receipts必須指向相同 Context/Environment。
- Request與 bundle不一致：RefusedPrecondition。
- 不依當前 ambient DPI、profile、theme state或全域設定補值。

### V2 Result

精確 fields：

- string ContractVersion
- ThemeCompatibilityStatusV2 Status
- ThemeCompatibilityEvaluationStateV2 EvaluationState
- ThemeId? ThemeId
- ThemeVersion? ThemeVersion
- string? PackageHash

- string? SelectedCoreVersion
- string? SelectedThemeApiVersion
- string? SelectedUxContractVersion
- string? SelectedManifestSchemaVersion

- ThemeCompatibilityEvidenceStatusV2 RuntimeValidationStatus
- ThemeCompatibilityEvidenceStatusV2 CapabilityEvidenceStatus
- ImmutableArray<string> AuthorizedCapabilities
- ImmutableArray<string> EnabledCapabilities
- ImmutableArray<string> DisabledCapabilities
- ImmutableArray<ThemeCompatibilityNoticeV2> Notices
- ImmutableArray<ThemeCompatibilityFailureV2> Failures

- ThemeCompatibilityAccessibilityStatusV2 AccessibilityValidationStatus
- ThemeCompatibilitySafetyStatusV2 SafetyInvariantsStatus
- ImmutableArray<ThemeCompatibilitySafetyStatusV2> SafetyFailures

- ThemeCompatibilityEvidenceStatusV2 MigrationEvidenceStatus
- bool? MigrationRequired
- bool? MigrationReady

- ThemeCompatibilityEvidenceStatusV2 RollbackEvidenceStatus
- bool? RollbackRequired
- bool? RollbackAvailable

Identity/hash只有在可信內容已建立時輸出，
不從 caller填回，也不回顯不可信 raw值。

Capabilities集合可為空，但必須結合 CapabilityEvidenceStatus理解：
NotEvaluated＋空集合表示未取得判定，不能解讀為已授權或全部禁用。

Selected-version規則：

1. 硬 refusal或 schema gate失敗：
   四個 selected versions全部 null。

2. Schema gate成功：
   SelectedManifestSchemaVersion="1.0"。
   後續版本或 owner evidence失敗不抹除這個真實選定事實。

3. Theme API/UX：
   逐字沿用 Phase5A的 nullable selections。
   不以 caller值、tested[]或預設版本補 null。

4. Core：
   只有 Phase5A outcome能證明 Core輸入/宣告/相容性成立才輸出。
   若出現任一 InvalidVersionInput、UnsatisfiableVersionRange、
   ConflictingVersionDeclaration、UnsupportedManifestSchema或
   CoreVersionIncompatible，SelectedCoreVersion=null。
   因 generic failures沒有 dimension，採保守 null，
   不重跑 parser來猜測 Core其實成功。
   若只存在 API/UX no-compatible failures，Core通過可被證明，
   此時可輸出 canonical request.CoreVersion。

5. 已選定版本不代表整體 Compatible。

Migration/rollback未評估為 null；
不適用的 Ready可為 null；
不使用 false同時表示「不知道」與「確定不需要」。

### Status Model

ThemeCompatibilityEvaluationStateV2：

- NotEvaluated = 0
- RefusedPrecondition = 1
- Evaluated = 2

ThemeCompatibilityStatusV2，按以下列序固定數值，自0起：

0. NotEvaluated
1. RefusedPrecondition
2. TrustedEvidenceFailure
3. UnsupportedManifestSchema
4. ManifestSchemaInvalid
5. InvalidVersionInput
6. UnsatisfiableVersionRange
7. ConflictingVersionDeclaration
8. IncompatibleCoreVersion
9. IncompatibleThemeApiVersion
10. IncompatibleUxContractVersion
11. UnsupportedPlatform
12. UnsupportedInstallationMode
13. RuntimeValidationFailed
14. AccessibilityValidationFailed
15. SafetyValidationFailed
16. BlockedCapability
17. MigrationNotReady
18. RollbackUnavailable
19. EvidenceUnavailable
20. Compatible
21. CompatibleWithDegradation
22. CompatibleUntestedDpiWithScalableFallback

Default status不是成功。

NotEvaluated：
尚無已執行評估的描述性狀態；
正常 Resolve不以它作為缺少必要輸入的模糊替代品。

硬前提不足：
EvaluationState=RefusedPrecondition；
Status依原因為 RefusedPrecondition或 TrustedEvidenceFailure。

Context來源可靠、schema/內容可評估之後的失敗：
EvaluationState=Evaluated。
未完成的 owner維度保持 NotEvaluated。

Compatibility Manifest schema失敗：
來源可靠，已執行 schema stage，
但未執行 negotiation；EvaluationState=Evaluated，
selected versions全部 null。

### Failure Model

ThemeCompatibilityDimensionV2：

Precondition
Integrity
ManifestSchema
VersionDeclarations
CoreVersion
ThemeApiVersion
UxContractVersion
PlatformMode
Runtime
Accessibility
Safety
Capability
Migration
Rollback

ThemeCompatibilityFailureKindV2：

MissingTrustedContext
InvalidRuntimeContext
ContextBindingMismatch
ContentSnapshotUnavailable
IntegrityNotVerified
ContentEvidenceMismatch
UnsupportedManifestSchema
ManifestSchemaInvalid
InvalidSchemaSupportInput
InvalidVersionInput
UnsatisfiableVersionRange
ConflictingVersionDeclaration
CoreVersionIncompatible
ThemeApiNoCompatibleVersion
UxContractNoCompatibleVersion
PlatformUnsupported
InstallationModeUnsupported
RuntimeValidationFailed
AccessibilityValidationFailed
SafetyValidationFailed
AssetInventoryInvalid
MotionSafetyFailed
AudioSafetyFailed
CapabilityBlocked
MigrationNotReady
RollbackUnavailable
EvidenceMissing
EvidenceNotEvaluated
EvidenceScopeMismatch
EvidenceMalformed

上述 failure enum固定順序、自0起；數值不是排序權威。

ThemeCompatibilityFailureV2：

- ThemeCompatibilityFailureKindV2 Kind
- ThemeCompatibilityDimensionV2 Dimension
- int Sequence
- string? DiagnosticCode
- string Message

Sequence為最終輸出位置，從0開始連續。
DiagnosticCode在初版一律 null；Kind已提供穩定 machine identity。
未來若新增診斷碼，須另行治理。

Message：
由 Kind/Dimension對應的固定文字模板產生；
不插入 filesystem paths、raw JSON、caller version text、
secret資料或 exception.Message。

不重用 P3M/P4I碼。
原 Phase4 result仍保存在 context內作為 upstream evidence；
不把 P4I碼冒充 Phase5B自己的語義。

相同 kind可能代表不同 upstream failure occurrence；
不得去重而丟失 Phase5A evidence。

### Phase5A Mapping

| Internal kind | V2 Kind | V2 Dimension | Primary Status |
|---|---|---|---|
| InvalidVersionInput | InvalidVersionInput | VersionDeclarations | InvalidVersionInput |
| UnsatisfiableVersionRange | UnsatisfiableVersionRange | VersionDeclarations | UnsatisfiableVersionRange |
| UnsupportedManifestSchema | UnsupportedManifestSchema | ManifestSchema | UnsupportedManifestSchema |
| ConflictingVersionDeclaration | ConflictingVersionDeclaration | VersionDeclarations | ConflictingVersionDeclaration |
| CoreVersionIncompatible | CoreVersionIncompatible | CoreVersion | IncompatibleCoreVersion |
| ThemeApiNoCompatibleVersion | ThemeApiNoCompatibleVersion | ThemeApiVersion | IncompatibleThemeApiVersion |
| UxContractNoCompatibleVersion | UxContractNoCompatibleVersion | UxContractVersion | IncompatibleUxContractVersion |

前三種 generic version/declaration failures不附 source field。
VersionDeclarations是計算類別，不宣称必然來自 package某一欄位；
InvalidVersionInput也可能來自 caller Core/support tokens。

保留 Phase5A Failures原順序及重複 occurrence。
不從 occurrence index猜測來源維度。

Resolver只呼叫一次封存 Negotiate。
為呼叫舊 internal signature，建立短生命週期 V1 request projection：
- manifests來自 context；
-版本/集合來自 immutable environment；
- accessibility若尚無驗證，使用誠實的 Unvalidated表示。
Phase5A不讀取其未使用欄位。

這不是 V1 resolver implementation、public adapter或 V1成功結果。

### Failure Precedence

精確順序：

0. Context/environment/bundle identity及結構前提。
1. 真實 integrity verdict與same-content binding。
2. Schema identifiers、caller schema support、raw schema/materialization。
3. Phase5A完整 Failures序列：
   declarations → Core → Theme API → UX，
   逐項保留封存實際順序。
4. Platform、architecture、Installer/Portable declarations。
5. Runtime/DPI/profile validation evidence。
6. Accessibility evidence。
7. Safety evidence：
   Home Safety Core → critical alerts → risk/permission clarity
   → confirmation semantics → accessibility
   → asset inventory → motion safety → audio safety。
8. Capability evidence。
9. Migration evidence。
10. Rollback evidence。

硬 refusal：

0或1失敗時停止；
不執行 negotiation、platform或其他 owner evidence組合。
可回報同一安全前提階段已知的多項失敗；
不得穿越失敗階段補做推論。

Schema失敗：

停止 schema-dependent negotiation及後續判斷；
只保留該階段真正取得的 failures。

其餘：

在共同 context/environment可信且可獨立評估時，
收集所有獨立失敗，不因 Core失敗而丟棄有效 owner evidence。

同一 owner維度內：
Missing → ScopeMismatch → Malformed → NotEvaluated → Failed。
只有實際適用的情況才輸出，不為一個 missing receipt重複附加
scope/malformed/failed等推測。

非 Phase5A同階 failures依：
固定 Kind順序 →固定 safety suborder →核准 logical requirement ID
作 Ordinal排序，再指派 Sequence。

Primary status：
取上述最先的 failure所對應 status。
EvidenceMissing/EvidenceNotEvaluated映射 EvidenceUnavailable；
scope/malformed跨物件前提問題映射 RefusedPrecondition。

無 failure時：
- 有已驗證的 untested-DPI scalable-fallback notice：
  CompatibleUntestedDpiWithScalableFallback。
- 否則有核准 decoration degradation：
  CompatibleWithDegradation。
- 否則：Compatible。

不以 enum數值排序替代上述 precedence。

### Compatible Semantics

Status=Compatible當且僅當：

- Context來自核准 binder且完整 verified content binding成功。
- schema支援與materialization成功。
- Phase5A CanContinue=true。
- 四個 selected versions均有合法值。
- platform/architecture/mode符合支援條件。
- runtime receipt Passed。
- accessibility receipt Validated。
- Safety receipt Passed且其必要子驗證已通過。
- capability receipt Passed且無blocking decisions。
- migration/rollback necessity已有適用owner判定。
- Required=true的 migration/rollback能力證據成功。
- 所有receipt指向相同context/environment。
- Failures為空。
- 無需另標示的degradation/fallback notice。

CompatibleWithDegradation及
CompatibleUntestedDpiWithScalableFallback也必須滿足全部安全條件，
只允許已核准的可用性保持/裝飾降級。

Compatible certifies：
該內容快照對該environment及指定operation，
本契約所要求的相容性證據完整且無阻擋。

Does not certify：
Installed、Enabled、Active、Installable、Activatable、
Migration executed、Rollback executed、
current filesystem custody、永久有效性、全面產品安全認證。

它不執行任何上述狀態轉換。

### Capability Evidence

Producer：
ThemeCapabilityGate及其核准的 Theme capability composition owner。
目前 producer尚未實作，不在Phase5B內代建policy engine。

ThemeCompatibilityCapabilityEvidenceV2 fields：

- Context
- Environment
- ThemeCompatibilityEvidenceStatusV2 Status
- ImmutableArray<string> AuthorizedCapabilities
- ImmutableArray<string> EnabledCapabilities
- ImmutableArray<string> DisabledCapabilities
- ImmutableArray<ThemeCompatibilityNoticeV2> Notices
- ImmutableArray<ThemeCompatibilityFailureV2> Failures

Producer obligations：

- Allowed不自動等於Enabled。
- Enabled必須是authorized且declared的capability。
- 未宣告與unknown capability不得授權。
- 只能disable optional presentation capability。
- 不得degrade Safety、Q93、critical alerts或其他Core invariants。
- Policy判斷、optional判斷、degradation是否合法均由producer完成。

Resolver：
只驗證receipt形狀/綁定並投影結果；
不從manifest宣告推導policy，不呼叫ThemeCapabilityGate。

Missing：
CapabilityEvidenceStatus=NotEvaluated；
failure=EvidenceMissing/Capability；
絕不產生任何compatible成功status。

### Accessibility Evidence

Producer：
Tcc.Themes的核准accessibility validation owner，
配合AccessibilityRuntime及既有IThemeAccessibilityValidator。
不得只包裝caller提交的ThemeAccessibilityValidationResult。

Receipt fields：

- Context
- Environment
- ThemeCompatibilityAccessibilityStatusV2 Status
- ImmutableArray<ThemeCompatibilityFailureV2> Failures

Statuses：
NotEvaluated=0
Validated=1
Failed=2

Runtime binding：

必須涵蓋environment的全部runtime targets、
DPI、viewport、text scale、zoom、variant、surface及完整profile；
並完成ValidationProfileVersion要求的Q93矩陣、
deep/light及必要reduced/keyboard/screen-reader驗證。

變更任一target/profile/build/context，需要新的receipt。
ProfileId字串相等不足以替代完整snapshot identity。

q93_compliant_claim、pass_required及matrix存在只屬宣告。
不構成Validated。

Missing：
NotEvaluated＋EvidenceMissing/Accessibility。
有receipt但未執行：
NotEvaluated＋EvidenceNotEvaluated/Accessibility。
任一必要項失敗：
Failed＋AccessibilityValidationFailed。

### Safety Evidence

Producer：
Tcc.Themes中核准的Theme Safety validation orchestration，
組合各驗證owner的實際結果；
不引用Risk、Permission、Domain或Recovery implementation。

ThemeCompatibilitySafetyStatusV2：

NotEvaluated
Passed
FailedHomeSafetyCore
FailedCriticalAlerts
FailedRiskPermissionClarity
FailedConfirmationSemantics
FailedAccessibility

其serialization保留對應意義：

not_evaluated
passed
failed_home_safety_core
failed_critical_alerts
failed_risk_permission_clarity
failed_confirmation_semantics
failed_accessibility

Receipt fields：

- Context
- Environment
- ThemeCompatibilitySafetyStatusV2 Status
- ImmutableArray<ThemeCompatibilitySafetyStatusV2> Failures
- ThemeCompatibilityEvidenceStatusV2 AssetInventoryStatus
- ThemeCompatibilityEvidenceStatusV2 MotionSafetyStatus
- ThemeCompatibilityEvidenceStatusV2 AudioSafetyStatus
- ImmutableArray<ThemeCompatibilityFailureV2> Details

Passed必要條件：
SafetyFailures空；
asset inventory及motion safety Passed；
audio safety Passed，或owner已證明不適用。

如此保留Theme §10.5的asset/motion/audio驗證責任，
不把Phase4 FileEvidence當成這些語意驗證。

EvidenceStatusV2：
NotEvaluated=0、Passed=1、Failed=2、NotApplicable=3。

NotApplicable只可用於確實無適用audio功能等明確可選子項；
不得用於整體runtime、capability、accessibility或Safety。

Missing required Safety evidence：
NotEvaluated＋EvidenceMissing/Safety。
不得填passed。
Resolver不執行Safety validation。

### Migration / Rollback

Migration receipt：

- Context
- Environment
- ThemeCompatibilityEvidenceStatusV2 Status
- bool? MigrationRequired
- bool? MigrationReady
- ImmutableArray<ThemeCompatibilityFailureV2> Failures

Producer：
ThemeApplicationService下的theme-owned state migration owner。

MigrationRequired：
針對指定source theme/state revision與target package，
是否需要theme-owned state轉換的相容性事實。

MigrationReady：
適用計畫與必要前提是否已被其owner確認。
不是「已執行」。

規則：
- 未評估：Required=null，Ready=null。
- 已確認不需要：Required=false，Ready=null。
- 需要：Required=true，Ready必須有明確true/false。
- Required=true且Ready非true：阻擋。
- 不從MAJOR版本或manifest宣告單獨推論Ready。

Rollback receipt：

- Context
- Environment
- ThemeCompatibilityEvidenceStatusV2 Status
- bool? RollbackRequired
- bool? RollbackAvailable
- ImmutableArray<ThemeCompatibilityFailureV2> Failures

Producer：
ThemeRollbackManager及其核准lifecycle owner。

RollbackRequired：
指定operation是否要求具備可回復路徑的前提事實。

RollbackAvailable：
當次owner確認存在適用、具體且已驗證的rollback選項。
不是version range存在，也不是已執行rollback。

規則：
- 未評估：Required=null，Available=null。
- 已確認不要求：Required=false；
  Available保留實際true/false；未檢查則null。
- Required=true且Available非true：阻擋。

兩份receipt均為最終成功必要輸入；
「已證明不需要」也是有效的owner判定，
不能由resolver填false代替。

Source state、operation、target或environment變更後，重新取得receipt。

Resolver：
零state migration、零backup、零rollback execution。

### Platform / Portable / DPI

Inputs：
Environment.Platform、Architecture、InstallationMode、
RuntimeBuildId及RuntimeTargets。

Authority：
目前production windows/x64。
Installer與Portable保持同一核心功能及驗證要求。

Declaration checks：
- Manifest.SupportedPlatforms包含windows。
- Manifest.PortableSupported保持核准語意。
- Compatibility.Windows的installer/portable/multi-monitor宣告成立。
- 不把其他platform aliases自動正規化成windows。

三種資訊必須區分：

1. Declared support：package宣告。
2. Tested coverage：maximum_tested_dpi_scale、
   dpi_ranges_tested及tested[]。
3. Runtime validated compatibility：owner receipt。

maximum_tested_dpi_scale不是硬支援上限。
超過tested bucket不自動失敗，也不自動成功。
只有實際scalable layout/reflow/scroll及安全fallback驗證成功，
才可輸出CompatibleUntestedDpiWithScalableFallback。

minimum_dpi_scale不作為忽略Q93或OS-supported scale的藉口。
Resolver不新增未核准的數值admission閾值；
實際可用性由runtime/accessibility owner驗證。

Runtime receipt：

- Context
- Environment
- ThemeCompatibilityEvidenceStatusV2 Status
- ImmutableArray<ThemeCompatibilityNoticeV2> Notices
- ImmutableArray<ThemeCompatibilityFailureV2> Failures

Producer：
Theme runtime/platform validation owner；
Windows資料經DesktopHost注入的既有boundary取得，
不新增Tcc.Themes → Tcc.Windows reference。

DPI/monitor/profile變更：
重新取得environment及相應證據；
Resolver不查當前桌面狀態。

### Manifest Schema Support

Sealed authority:
{"1.0"}

Collection rules：

| Input | Result |
|---|---|
| null collection | InvalidSchemaSupportInput，schema gate停止 |
| empty collection | UnsupportedManifestSchema |
| 缺少"1.0" | UnsupportedManifestSchema |
| {"1.0","2.0"} | 有效交集僅{"1.0"}；2.0不授予支援 |
| 只有有效但未知identifier | UnsupportedManifestSchema |
| 含malformed/null元素 | InvalidSchemaSupportInput，不能靜默丟棄 |
| duplicates | Ordinal去重，語意不變 |
| enumeration order不同 | 結果相同 |

Schema support token grammar：
canonical非負十進位major.minor：
(0|[1-9][0-9]*) "." (0|[1-9][0-9]*)

這是新V2 caller schema-support collection的詞法規則，
不是修改Theme Package schema或Phase5A版本語法。

拒絕：
空字串、whitespace、"v1.0"、"1.0.0"、"01.0"、Unicode數字。
不Trim、不補版本段。

先驗證全部tokens，再与{"1.0"}取交集。
Caller不能擴張sealed schema authority。
InvalidSchemaSupportInput對應：
Status=RefusedPrecondition，Dimension=ManifestSchema。

### Determinism

Snapshots：

必須eager snapshot：
- Phase4 request inventory/trust nested collections。
- package enumeration records及raw byte buffers。
- ThemeManifest全部nested lists。
- Compatibility manifest tested arrays、DPI arrays、accessibility dictionary。
- Integrity FileEvidence、AssetEvidence、Diagnostics。
- 所有support collections。
- environment runtime targets及profiles。
- 每個owner receipt的lists/notices/failures。
- 最終result collections。

Caller須在acquisition期間保持來源穩定；
不承諾安全複製正在被同時修改的集合。

取得後caller mutation不影響result。
不使用caller comparer決定語意。
識別、集合及文字排序使用Ordinal。
版本計算保持Decision018及BigInteger語意。

State：
無clock、RNG、全域registry、AsyncLocal、ThreadLocal、
nonce table、service locator、mutable provenance cache。

Concurrency：
Resolver無instance fields及mutable static state；
同一instance可處理獨立concurrent calls。

Sync：
Resolve保持同步。
不使用Task.Run、sync-over-async或在Resolve內等待verifier。

Constructor：
public ThemeCompatibilityResolverV2()

DI：
未來由DesktopHost註冊時建議Singleton。
Request/context/evidence為每次作業的immutable data；
不得存成singleton mutable state。
契約修訂不建立DI infrastructure。

### V1 Preservation

完全保留：

- IThemeCompatibilityResolver
- ThemeCompatibilityRequest
- ThemeCompatibilityResult
- ThemeCompatibilityStatus
- ThemeFailureReason
- ThemeAccessibilityStatus
- 其餘V1 enums、DTO、serialization semantics

V1 implementation count: 0。

不增加V1 adapter、dual-interface implementation或fallback。
V2 family不改Theme API production support "1.0.0"。

新增identifier：
ContractVersions.ThemeCompatibilityContractV2 = "2.0"

含義：
V2 runtime composition/result contract identifier。

不是：
Theme API version、ThemeManifest schema、
Compatibility Manifest schema或Integrity schema升版。

### Guard Evolution

核准將新V2 types放在：
Tcc.Themes.Compatibility.V2

將binding types放在：
Tcc.Themes.Compatibility.Binding

原namespace：
Tcc.Themes.Compatibility
仍精確只有三個Phase5A handwritten top-level types。

因此，不必為新增V2去削弱Phase5A既有namespace-wide限制。

實際需演進：

1. File:
tests\Tcc.Architecture.Tests\PhaseThreeScopeBoundaryTests.cs

Test:
ActualCompiledThemeAssemblyMatchesExactPhaseThreeTypeSurface

Helper:
GetCompiledSurfaceViolations

Current:
ApprovedTopLevelTypes固定清單；
新API、context、binder、resolver等均會被拒絕。
Nested artifacts也必須列入精確來源規則。

Smallest change:
分A/B候選階段，加入本決策逐名列出的新型別及精確shape。
保留原Phase5A type/dependency/outcome/exception-handler guards全部原樣。
新增獨立V2 resolver guard及binder的窄依賴guard。

2. File:
tests\Tcc.Architecture.Tests\PhaseTwoContractCompletenessTests.cs

Test:
RequiredInterfacesHaveOnlyPhaseApprovedProductionImplementations

Helper:
FindUnauthorizedImplementationViolations

Current:
IThemePackageContentReader的production implementation未獲准。

Conflict:
後續ThemeCompatibilityContentSnapshot需要實作該既有reader介面。

Smallest change:
只允許精確internal sealed snapshot type。
不允許其他reader、不放寬resolver/capability/lifecycle implementations。

不需改動的歷史測試：

3. File:
tests\Tcc.Architecture.Tests\PhaseFiveThemeCompatibilityNegotiationTests.cs

Test:
CompiledPhaseFiveSurfaceIsInternalSynchronousAndStateless

原assertion：
原compatibility namespace精確三個internal types、
negotiator同步stateless、assembly內V1 implementations=0。

本核准設計完全符合：
V2在獨立子namespace且只實作V2 interface。
因此整檔可byte-preserve，不需刪除或弱化該test。

4. File:
tests\Tcc.Architecture.Tests\PhaseFourThemePackageMetadataEvaluatorTests.cs

Test:
EmbeddedSchemasHaveExactNamesAndSourceBytes

實際assertion：
逐一驗證原三份resource存在及bytes/hash；
不是禁止assembly增加第四份resource的exact-set assertion。

因此不修改。
新增Compatibility resource另由新binding tests驗證。

5. File:
tests\Tcc.Architecture.Tests\PhaseFourContractAmendmentTests.cs

Test:
AmendmentAddsNoVerifierImplementationOrPhaseFiveOrTradingLeakage

實際scope：
驗證原integrity implementation規則及固定四個integrity amendment檔案。
本設計不修改該四檔，不因method名稱包含PhaseFive就假定需放寬。

V2 count：

A契約候選完成：
V1=0、V2=0。

B實作候選完成：
V1=0、V2=1，唯一允許：
Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2。

Second resolver attack：
掃描全部production types；
abstract、generic、nested、generated、wrong namespace、
dual V1/V2或第二個implementation皆不得繞過。
不要只計算public concrete types。

Resolver dependencies：
固定closed allowlist；
涵蓋signatures、fields、locals、IL calls、generic arguments/
constraints、typed catch metadata及filter IL。

Binder限定可依賴sealed verifier、content reader、
JSON/schema、SHA256及immutable snapshot所需BCL。
不得把binder的I/O/crypto allowlist套用到resolver或Phase5A。

### Serialization

Stable serialization required：

- ThemeCompatibilityResultV2。
- 其nested failures/notices/enums。
- V2 environment/runtime target可作描述性資料使用，
  不構成trust proof。

ThemeCompatibilityJsonV2 public methods：

- string SerializeResult(ThemeCompatibilityResultV2 result)
- ThemeCompatibilityResultV2 DeserializeResult(string json)

規則：
- snake_case。
- 固定property order及enum string values。
- 所有selected null明確序列化為null。
- 拒絕unknown fields、unknown/numeric enums、缺required members、
  duplicate JSON properties及矛盾狀態。
- 驗證ContractVersion精確"2.0"。
- 失敗排序及Sequence必须合法。
- 不修改ThemeContractJson既有V1/Phase4設定。

Opaque runtime objects：

ThemeCompatibilityContextV2、ThemeCompatibilityEvidenceV2、
request及internal receipts不屬portable serialized trust artifacts。
Canonical V2 serializer不提供它們的serialize/import入口。
一般serializer可能輸出描述或空object，
但不能藉此取得合法trusted instance。

Public result即使反序列化成功仍只是資料，
不能用來建立context/evidence，也不能作為activation authorization。

### Supplemental contract closure S1–S6

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

### Current authorization and phase gates

Only the ten Candidate A files explicitly listed by TCC-P5B-66 are authorized. Candidate A defines data shapes, immutable snapshots, internal construction and strict result transport only. V1 resolver implementations=0 and Candidate A V2 resolver implementations=0. Future Candidate B resolver implementations must equal exactly one after separate authorization. The binder, content snapshot reader, compatibility materializer, evidence producers, lifecycle issuance, DI registration and resolver remain absent. All binding/materialization/owner behavior described above is normative future work, not a claim of implemented or validated runtime trust.

No changes to Phase5A production or functional tests, V1 source/semantics, package schemas, csproj/lock files, project/package references, friend assemblies or earlier ADRs. No Trading, Market Data, AI, execution connector or business policy ownership. No filesystem, reader, crypto, lifecycle or raw-content authority enters Resolve. The existing six-project/eleven-ProjectReference/four-PackageReference graph and Frozen sources remain unchanged.

Required gates: focused contract/scope/Phase2/Phase5A/dependency tests, normal and locked restore with NuGet audit, single-node Release x64 build, fresh full tests with zero failures/skips, Frozen/sealed preservation, exact scope, Git hygiene and truthful status synchronization. Implementation self-validation does not grant independent approval, resolver authorization or seal. No stage/commit/tag/push is authorized.

## TCC-DEC-2026-09-14-020 — Trusted Built-In Theme Bootstrap and First UI Boundary

- Date: 2026-09-14.
- Status: **ADDED / CANDIDATE — BOUNDARY / CONTRACT APPROVAL CANDIDATE** under TCC-P6-82. This is an approved-for-review boundary candidate, not a production implementation, independent approval or seal.
- Decision source: explicit later user requirements in TCC-P6-82, following TCC-P6-81 PLANNING PASS; Frozen Theme §§24.4–24.4.1,25; Frozen System §§7.4,16.4,31; approved Questionnaire Q1–Q110, Constitution and UX; ADR-0001/0002/0004.
- Reason: establish the smallest complete trusted built-in startup foundation without blocking first UI on external Theme admission or fabricating package evidence.
- Impact: future Phase6A built-in presentation in Tcc.Themes, Windows startup/monitor facts in Tcc.Windows and composition in Tcc.DesktopHost; exact future tests/guards and handoff.
- Supersedes: **NONE**. Decision019, ADR-0004, sealed Phase5A/5B, Candidate A contracts and Frozen artifacts remain unchanged.

### Locked decisions

1. Strategy is **minimal trusted built-in runtime foundation → independent validation → first UI vertical slice**. Full Theme Runtime first and stub-heavy UI first are rejected.
2. The default safe presentation is trusted embedded product-owned platform presentation. It is not an installable/uninstallable Theme Package, an admission candidate, a Compatibility V2 success substitute or a source of receipts. Safe minimal shell startup does not wait for external packages.
3. Phase6A excludes IThemeRuntime implementation and all external package acquisition/discovery/install/uninstall/activation/live switch/preview/rollback/migration/persistence/cache/registry. Sealed resolver/verifier implementations remain intact but are not registered or invoked by bootstrap.
4. No owner evidence fabrication. No fake Compatibility, Safety, Q93, user/account/login/synchronization success. Runtime/capability/accessibility/Safety/migration/rollback receipt producers remain absent; Decision019 trust/evidence semantics are unchanged.
5. Narrow additive boundary: public sealed BuiltInThemePresentationSource and internally constructed immutable BuiltInThemePresentationSnapshot owned by Tcc.Themes.Fallback. UI consumes only snapshot presentation facts. Reuse ThemeVariantId, ThemeTokenBundle, ThemeTokenValue, ThemeSemanticBinding, ThemeFocusStyles and ThemeAccessibilityContract with deep copies/frozen dictionaries/JsonElement clones. No package identity, receipt/verdict, lifecycle, path authority, principal/account, HWND or ResourceDictionary in snapshot.
6. Exact startup variants are machine IDs **deep/light**, display names **Deep/Light**. Explicit --startup-variant selects once per process; absent selects Deep; invalid/duplicate selects Deep with BOOTSTRAP_VARIANT_INVALID notice. Direct source invalid variant throws. This is Startup Variant Selection, never Theme Variant Switching.
7. Bootstrap state is immutable in-memory presentation only; no saved preference, per-theme/user/account persistence, transition audit, migration or rollback. The existing full ThemeRuntimeState is not reused for bootstrap.
8. Installer/Portable use identical built-in presentation behavior. Explicit --startup-mode supplies startup path policy only; it does not attest packaging provenance. Missing/invalid mode is Unknown, roots absent and notice visible; no guessed Installer default. No persistent/core-store operations until a future owner establishes actual deployment authority.
9. WindowsStartupPathResolver owns raw Windows argument cardinality, mode and canonical Known Folder/Portable data-root resolution. Paths stay with composition, never Theme/UI/ViewModel. No package discovery/intake, directory writes, network discovery, trust decision, marker/lock/persistence subsystem. Unavailable/read-only paths cannot prevent the embedded shell.
10. WindowMonitorAdapter/WindowMonitorFacts own actual HWND-derived work area/DPI and native visible-bound enforcement. Facts are immutable per-window captures; no global monitor singleton, fake 96-DPI fallback or placement persistence. Host responds to actual window/DPI/display events and releases hooks.
11. DesktopHost alone owns DI and startup ordering. Theme owns presentation state; Windows owns platform facts. Host does not own compatibility evidence, package trust/lifecycle, business Safety, authentication or Home business logic. Actual App→DI→MainWindow→ViewModel acquisition must consume the approved snapshot.
12. Exact registrations: singleton WindowsStartupPathResolver, WindowsStartupFacts instance, BuiltInThemePresentationSource, BuiltInThemePresentationSnapshot instance, MainWindowViewModel and MainWindow; transient WindowMonitorAdapter captured per window. ThemeBootstrap and SafeThemeResourceAdapter are internal static and unregistered. No alternate provider/service locator; no operation-local future receipt/state captured by singleton.
13. Public/internal budget: **7 new public types, 2 internal top-level types, 2 private native nested structs; 11 total; 0 new interfaces**. Existing host constructors/properties change only as specified in ADR-0005 §3. No new public type in Presentation.Contracts. The planning-only IBuiltInThemePresentationSource interface is rejected as unnecessary.
14. Dependency budget remains **6 projects / 11 ProjectReferences / 4 PackageReferences / 1 existing test friend**; all new project/reference/package/friend counts zero. WPF remains in DesktopHost; platform code is in Windows; no architecture direction is added.
15. First future UI is native WPF Command Center with left navigation, top app/status area, four-part Home Safety Core, mental-state area, BTC boundary, KPI/overview, Deep/Light, resize/DPI/monitor-safe layout and keyboard/focus/screen-reader baseline. Home-shaped shell does not create Features.Home business ownership. P6-83 only adapts the existing minimal window; full UI regions await independent validation and separate authorization.
16. Allowed placeholders: BTC unavailable, KPI/positions/risk unknown, disabled future navigation, isolated clearly labelled presentation fixtures. Forbidden: fake price, permission, zero risk/positions, no-alert, login/sync success or receipts. Unknown remains unknown.
17. Q93 is real applicable behavior, not a package receipt prerequisite for built-in UI. Provide contrast, text/zoom scaling, focus, high contrast, keyboard, screen-reader semantics and reduced motion/transparency support. Existing accessibility bools express requirements, never validated status. Safety truth is supplied only by future business owners; presentation must preserve Trading Permission, Total Risk, Current Positions and Major Alerts.
18. Exact type/member table, resource/token values, mode/path/DPI algorithms, seven-service DI/lifetime budget, and **22-path maximum TCC-P6-83 file allowlist** are normative in [ADR-0005](adr/ADR-0005-trusted-built-in-theme-bootstrap-and-presentation-boundary.md) §§3–9. They are the final proposed implementation boundary; any additional type/member/path/dependency requires STOP, not implementation improvisation.
19. Architecture guards must evolve by exact identity/shape/ownership/provenance, never namespace wildcard or count-only API approval. Protect all old Phase5 guards and new no-runtime/no-package/no-evidence/no-UI-resolver/no-path/no-business/no-Trading/AI boundaries. Positive and compiled negative mutants must exercise real guards.
20. P6-83 acceptance requires deep immutability, both variant/resource paths, invalid selection, Installer/Portable/unknown facts, path/native DPI boundaries, real App/MainWindow bootstrap without packages, absence of resolver/evidence calls, DI/architecture mutants, normal/locked restore with audit, Release x64, full tests, real applicable minimal-shell accessibility checks, Frozen/sealed preservation and truthful status/Git hygiene. Missing runtime/hardware validation cannot be reported as PASS.
21. TCC-P6-84 is read-only independent validation of fake-source/mutable-snapshot/trust/path/mode/monitor/resource/DI/lifetime/runtime/evidence/variant/parity attacks with a real positive startup control. Fixes require a separate authorization; no same-task repair or automatic UI start.

### TCC-P6-83B acceptance-scope clarification

This subsection is an additive clarification of the still-unsealed Decision020 candidate under the explicit later TCC-P6-83B requirement. It is not Decision021, a quality waiver, an independent validation result or a seal. Decision019, ADR-0004 and the product/architecture semantics above are unchanged.

Historical results remain exact: TCC-P6-83 is BLOCKED and TCC-P6-83A is BLOCKED. Their missing-evidence findings are not rewritten as PASS. TCC-P6-83A nevertheless established 21/21 compiled mutants rejected with zero bypasses and a legal control accepted; a corrupted embedded-resource real-apphost exit code 1 with an accessible explicit failure, no fallback and no fake shell; two real monitors; real 100% and 125% DPI; cross-monitor refresh, resize and off-screen recovery; real WPF UI Automation/name/focus evidence; current non-default 152% Windows text scaling; focused 49/49 PASS; full 1582/1582 PASS; and Release with zero warnings/errors.

For the Phase6A bootstrap foundation, physical/accessibility acceptance is limited to the mechanisms and minimal shell that now exist. It requires: at least two actual physical display/DPI contexts when available; real HWND acquisition; real GetDpiForWindow behavior; real cross-monitor refresh when multiple monitors exist; real resize and off-screen recovery; real WPF UI Automation discovery; accessible names and focus evidence for the minimal shell; a non-default real Windows text scaling profile when available; automated High Contrast resource projection; and no critical-state dependence on animation or transparency. The accepted TCC-P6-83A evidence satisfies this foundation scope.

The following checks remain mandatory but are explicitly **DEFERRED / NOT YET EXECUTED**, not PASS and not Phase6A bootstrap blockers: physical 150%, 175% and 200% DPI; Windows text-size 100%, 125%, 150%, 175% and 200%; active High Contrast physical validation; and interactive Narrator reading/order validation. Their owner is TCC-P6-85 first real UI vertical-slice validation and/or its independent validation task, with release validation repeating applicable profiles. They must remain open in governance until executed.

The split exists because those deferred checks primarily validate the final Command Center presentation layout, readability, information survival and assistive-technology behavior. Requiring the complete matrix against the intentionally minimal bootstrap shell would not validate the later UI and would require the same validation again after UI implementation. Phase6A has already exercised the underlying Windows monitor/DPI/UIA foundation on real hardware.

Under this clarified scope: TCC-P6-83B is PASS; the Combined Phase6A Implementation Gate is PASS; Phase6A remains IMPLEMENTED / UNVALIDATED / UNSEALED; Ready for TCC-P6-84: YES; Ready for UI: NO; and the deferred physical UI matrix remains OPEN / REQUIRED FOR UI VALIDATION.

### Current authorization and handoff

TCC-P6-82 authorizes only creation of codex/phase6-theme-bootstrap-foundation at sealed HEAD 05a116d41c8a96b7c8d591ddbe212d1e18cf6fa9 and exactly three governance mutations: append this Decision020, add ADR-0005, update CODEX_PROJECT_STATUS. No production/test/contract/csproj/solution/package/Frozen changes; no git add, commit, tag, push, merge, rebase or amend.

At the historical TCC-P6-82 handoff, Phase5B stayed SEALED / APPROVED, TCC-P6-81 PLANNING PASS was carried from explicit task authority, and Phase6A production, runtime wiring and first UI were NOT IMPLEMENTED. That Boundary PASS made TCC-P6-83 ready for Supervisor authorization only. Ready for UI and external Theme runtime remained NO.

TCC-P6-83B later authorizes only this Decision020 clarification, the corresponding ADR-0005 clarification and truthful project-status synchronization. No production or test mutation and no technical rerun is authorized when candidate bytes remain unchanged. After the governance clarification passes, the exact next action is return to GPT Supervisor, who may issue TCC-P6-84. UI remains separately unauthorized.

## TCC-DEC-2026-09-15-021 — Fixed Design System, Module-Specific Layout Strategies

- Date: 2026-09-15.
- Decision source: explicit later user binding product-wide design clarification during TCC-P6-85 Stage B.
- Decision: TCC shares a stable design system for typography, spacing, semantic color and Deep/Light semantics, separators, focus, keyboard/navigation behavior, accessibility, safety hierarchy, icons, interaction quality, honest unavailable-data treatment, Theme Contract and architectural boundaries. Page skeleton, density, visual rhythm and workspace strategy remain module-specific.
- Home scope: A — Command Ledger is the Home / Overview baseline; only C's table/workstation language is imported. Home KPI/Safety geometry, table proportions, density and responsive rearrangement must not become universal product requirements. Task85 implements no future module and introduces no universal dashboard template.
- Reuse: styles may be shared only when they are true design-system primitives; layout components may be shared only where the semantic obligation is genuinely common. Future module examples are directional guidance, not Frozen specifications.
- Skill boundary: Taste may choose module-appropriate visual strategy; UI/UX Pro Max may review module usability and information architecture. Neither may impose one module's layout strategy globally. Future Hallmark may affect brand/theme expression, not replace product UX obligations.
- Reason / impact: preserve future Markets, Trade, Positions, Risk, Journal, Analytics, Intelligence, Settings and Theme Library layout flexibility without global-design-system rewrites or premature abstraction.
- Supersedes: NONE. This records product-wide UI design guidance only, not an architecture amendment. Decision020, ADR-0005, Frozen System/Theme, Phase6A and Theme Contract are unchanged.

## TCC-DEC-2026-09-15-022 — Home Watchlist-First Visual Direction

- Date: 2026-09-15.
- Decision source: explicit later user TCC-P6-85R2 Home reset and authorized scope expansion.
- Decision: Home / Overview uses a Watchlist-first workstation, with an immediate compact four-obligation Home Safety Core and subordinate BTC, Positions and Mental State context. Showcase/V2-family references guide clarity, simplicity, hierarchy, primary-workstation dominance and visual quality; they do not lock geometry or density. Legacy Task85 Command Ledger visuals are deprecated as a Home target.
- Reason: the previous stacked Command Ledger, KPI and multi-timeframe presentation missed the selected Home direction; the approved physical Windows Text Size check also exposed fixed-size Home text.
- Impact: Home presentation and its local tests/status only. Preserve honest unknown/unavailable data, Q93, Deep/Light Theme Contract, keyboard/UIA, Installer/Portable parity and all business/architecture boundaries. Future modules retain their module-specific layout strategies.
- Supersedes: only Decision021's Home-specific `A — Command Ledger` baseline and C-import sentence. Decision021's product-wide design-system and module-specific-layout guidance remains in force; Decision020, ADR-0005, Phase6A, Frozen artifacts and approved product semantics are unchanged.

## TCC-DEC-2026-09-16-023 — Home Production Visual Master Replacement and M0 Baseline

- Date: 2026-09-16.
- Decision source: explicit later user requirement and the supplied `錄製內容 2026-09-15 151055.mp4` NEW MASTER recording, SHA-256 `A36A5ECCB2E99CCD02DB6AE11005CCB7CE470DBF355D9E1D5D9320CEAF099BC1`.
- Decision: the supplied NEW MASTER is the Home / Command Center Production Visual Master. It determines global composition, layout, grid, panel geometry, navigation structure, information hierarchy, character zone, UI density, component placement, workspace proportions and interaction-surface placement. The OLD MASTER is limited to Brand / Art Direction Reference: ice-and-snow atmosphere, landscape language, plum blossom, Eastern architecture, Gu Qinghan world, brand finish and material inspiration. OLD MASTER scene composition cannot override NEW MASTER geometry or components.
- M0 baseline: `docs/design/master/TCC_HOME_PRODUCTION_MASTER.md`, `TCC_HOME_MASTER_GEOMETRY.md` and `TCC_HOME_MASTER_OPEN_QUESTIONS.md` record 25 visible regions, the measured 2142×1196 grid/alignment model, L0-L8 layers, component families, fidelity risks and unresolved issues. M0 is documentation-only and creates no implementation authority.
- Production impact: no production code, XAML, test, contract, Theme resource, feature, dependency or architecture boundary changed. The existing uncommitted TCC-P6-85R2 implementation and its self-validation remain historical evidence, but its Watchlist-first visual target is not the current Production Visual Master and cannot be sealed as such.
- Semantic preservation: Home Safety Core, honest unavailable/unknown states, GPT optionality, read-only future connectors, Q93 accessibility, Deep/Light semantics, Installer/Portable parity, Theme isolation and all approved architecture boundaries remain unchanged.
- Open evidence: the single supplied dark reference does not define responsive layouts, character crop limits, exact clean panel tracks, asset decomposition, full interaction/state coverage, exact typography/colors, motion, localization, DPI/text scaling or Light/High Contrast treatment. These are explicitly `[UNDEFINED]` and cannot be guessed during implementation.
- Supersedes: Decision022's Home Watchlist-first visual direction and Showcase/V2-family target. Decision022's factual historical implementation/remediation record remains. Decision021's product-wide fixed-design-system/module-specific-layout principle, Decision020, ADR-0005, Phase6A, Frozen artifacts and approved product/business semantics remain unchanged.


## TCC-DEC-2026-09-16-024 — M0.2 Host-owned Measurement and Compact Revision Contract

- Date: 2026-09-16; source: explicit later M0.2 user repair authorization.
- Decision: model outputs primitive geometry and short evidence IDs; host owns hashes, right/bottom and normalized 0..1 fractions. Central registry and local line-supported proposals replace repeated EvidenceRef and coordinate guessing. Unreliable geometry remains UNDEFINED/null.
- Execution: A draft → Host Guard → B with every host finding → A patch only → Host atomic patch/schema/Guard → final B. Prior-value canonical JSON hash required. Missing B response is HIGH AUDITOR_MISS, retained across rounds. Provider caps 14000/8000/10000/8000, max four calls, no paid retry or ceiling increase.
- Scope/impact: pipeline-only contracts/rendering/measurement and metadata; no production, Theme/Core semantics, dependency boundary or formal-spec rewrite. Replaces M0.1 model-generated derived values and full-bundle A3 only for future runs; both failed runs and old schema regression data remain immutable.
- Status: 228 tests PASS and offline fixture reconstruction PASS; four detected rectangles are all ambiguous. Does not approve Run003, freeze M0 or authorize M1.

## TCC-DEC-2026-09-16-025 — M0.3 Deterministic Required-Region Coverage Gate

- Date: 2026-09-16; source: explicit later M0.3 offline measurement repair authorization.
- Decision: separate measurement execution, required-region coverage, unresolved ambiguity and formal geometry gates. Command completion alone cannot claim measurement PASS. A future paid run must stop before lock creation unless the saved formal geometry gate passes.
- Measurement contract: original 2142×1196 PNG pixels are canonical; normalized coordinates are host-derived. Every one of 23 required regions has a controlled status, bbox or truthful null, evidence IDs, hierarchy and provenance. Geometry-critical regions require supported geometry; explicit nonrectangular art may remain bbox-free.
- Evidence architecture: multi-scale edges, row/column color and luminance projections, connected components, fixed negative-space gaps, crop envelopes, coarse anchors preserved as search-prior provenance, deduplicated candidates and hierarchical partitions. No historical coordinates, guessed bbox or ambiguous promotion is accepted.
- Scope/status: pipeline-only. Original 228/228 and new 20/20 offline tests PASS; records 23/23 and critical geometry 21/21. Model calls=0; no Run003, production C#/XAML, formal-spec, M1, commit, push or deploy change. This makes the evidence ready for Supervisor review only and does not authorize a paid run or freeze M0.

## TCC-DEC-2026-09-16-026 — M0.4 Host-Owned Candidate Construction

- Date: 2026-09-16; source: explicit M0.4 offline repair authorization after accepted M0-RUN-003 failure evidence.
- Decision: Agent A emits semantic interpretation only. The Host-owned Candidate Compiler injects canonical pixel/normalized geometry, image dimensions, required-region presence, measurement status/provenance/evidence, hierarchy, proposal selections, deterministic alignments and spacing. Model output and revision patches cannot override those fields.
- Audit flow: PASS1 compiles before Host Guard. Any BLOCKER fails PRE_AUDIT_GATE and stops before Agent B without adding a model call. Host findings use compact ID/root-cause/evidence references; CompactAuditV4 retains independent findings and can confirm, dispute or add defects without copying full evidence prose. An incomplete audit is never auditor PASS.
- Evidence: immutable Run003 replay groups 187 findings into 6 root causes; Host canonicalization removes deterministic fan-out and one proven NON_RECTANGULAR legacy-adapter false positive while retaining 2 genuine HIGH semantic/specification defects. Worst-case allowed Audit fixture is estimated at 7,158 tokens under the unchanged 8,000 ceiling.
- Scope/status: pipeline-only; original 228/228, M0.3 20/20 and M0.4 18/18 tests PASS. Model calls=0; Run001/002/003 custody PASS; production C#/XAML and formal specs unchanged; no Run004 or M1. Ready for Supervisor review does not authorize M0-RUN-004, M0 Freeze or M1.

## TCC-DEC-2026-09-16-027 — M0.4.1 PASS2 Audit Capacity Contract

- Date: 2026-09-16; source: explicit M0.4.1 offline capacity-hardening authorization after Supervisor acceptance of M0.4.
- Decision: CompactAuditV4 uses concise ID-backed findings and batched Host responses. It retains independent Agent B findings, severity, required action, Host confirmation/dispute/undefined handling, multiple region/evidence references and AUDITOR_MISS detection while excluding duplicated geometry, coordinates and evidence prose already held by Host artifacts.
- Capacity contract: the deterministic schema-maximum fixture covers 23 region audits, 40 independent findings, 200 Host finding dispositions, maximum references and maximum UTF-8 text. Exact serialization is 19,193 bytes / 5,998 estimated tokens using `ceil(bytes / 3.2)`, within the 6,000-token exit gate and unchanged 8,000-token PASS2 ceiling; safety margin is 2,002 tokens / 25.02%.
- Prompt contract: PASS2 input is reduced from 202,414 to 118,693 UTF-8 bytes by replacing duplicate Candidate geometry, registry records and Host prose with lossless R/E/H lookup maps. All 23 regions, semantic claims/families/questions/verifications, cited evidence source/image/bbox/kind data, Host findings, eight image payloads and full read-only Taste remain available.
- Scope/status: pipeline-only; original 228/228, M0.3 20/20, M0.4 18/18 and M0.4.1 16/16 tests PASS. Both remaining Run003 HIGH findings are Agent-owned semantic/specification defects for PASS2 detection and PASS3 revision. Model calls=0; custody PASS; production C#/XAML and formal specifications unchanged; no Run004 or M1. `READY_FOR_SUPERVISOR_REVIEW` does not authorize M0-RUN-004, M0 Freeze or M1.

## TCC-DEC-2026-09-16-028 — M0.4.2 Audit Equivalence and Host Revision Authority

- Date: 2026-09-16; source: explicit M0.4.2 offline repair authorization after accepted M0-RUN-004 failure evidence.
- Decision: classify Host HIGHs by deterministic root cause and substantive Agent B equivalence, not raw finding-ID overlap. Agent B must explicitly CONFIRM, REJECT or UNRESOLVED each Host root with controlled reason/evidence and still perform an independent per-region audit. Contradictory or missing dispositions remain auditor misses; equivalent independent evidence may cover a Host cause without concealing its own finding.
- Revision contract: Host creates the revision-base manifest, canonical field hashes and typed Agent-owned patch-target registry. Agent supplies target IDs and semantic intent, never previous-value hashes or Host-owned geometry. Host validates exact base, ownership, type and concurrency codes, applies atomically to SemanticDraft, then recompiles Candidate and reruns Guard.
- Historical diagnosis: Run004's 23 PASS3 patches selected nested `evidence_ids` fields whose actual Host hashes were absent from the prompt catalog; the Agent fabricated a repeated hash. Host snapshot and canonicalization were correct. This is a field-catalog omission plus Agent hash fabrication, addressed by removing hash authority from the model output.
- Evidence/status: immutable Run004 replay groups 45 Host HIGHs into three genuine roots and 42 fan-out instances, with one equivalent-covered and two genuine auditor-miss roots; all 23 historical intents map and apply offline. Recompiled Candidate and Host Guard 3 execute with 0 BLOCKER / 24 HIGH, so this is pipeline readiness only. Worst-case PASS2 estimates 5,895 tokens under the unchanged 8,000 ceiling. Original 228/228, M0.3 20/20, M0.4 18/18, M0.4.1 16/16 and M0.4.2 27/27 tests PASS. Model calls=0, custody PASS, production C#/XAML and formal specifications unchanged; no Run005, M0 Freeze or M1. `READY_FOR_SUPERVISOR_REVIEW` does not authorize a paid run.
- Supersedes: Decision027's PASS2 output contract for future execution only; its historical M0.4.1 result and all higher-authority product/architecture decisions remain unchanged.

## TCC-DEC-2026-09-16-029 — M0.4.3 Canonical PASS2 Schema and Raw-Before-Validation Custody

- Date: 2026-09-16; source: explicit M0.4.3 offline schema-conformance hardening authorization after accepted M0-RUN-005 failure evidence.
- Decision: `CompactAuditV43` and `M0.4.3_AUDIT` are the single active PASS2 contract authority. The same controlled enum constants generate Pydantic parsing, strict provider JSON Schema and prompt instructions. All fields are required and non-null, repeated empty values use `[]`, and extra fields are forbidden.
- Validation boundary: Stage 1 validates structural schema only. Stage 2 is a separate Host-owned reference-integrity gate for region, finding and evidence IDs and cross-record constraints. Either stage fails closed; no salvage, silent fill, schema relaxation or automatic paid retry is allowed.
- Custody: future provider/raw structured output must be durably written before Pydantic validation, with any validation error captured separately. Run005 did not preserve its raw PASS2 payload, Pydantic `errors()` details or traceback, so the exact historical failing field path and failed constraint cannot be reconstructed. Its truthful replay status is `NOT_POSSIBLE_RAW_OUTPUT_NOT_CAPTURED`; adversarial fixtures test the repaired contract without fabricating historical output.
- Evidence/status: provider compatibility, canonical contract, two-stage validation, adversarial fixtures, traceability and custody pass. Worst-case PASS2 is 18,837 UTF-8 bytes / 5,887 estimated tokens under the unchanged 8,000 ceiling, leaving 2,113 tokens / 26.41%. Original 228/228, M0.3 20/20, M0.4 18/18, M0.4.1 16/16, M0.4.2 27/27 and M0.4.3 40/40 tests pass; locked restore 6/6, Release x64 build 0 warnings / 0 errors and full repository tests 1700/1700 pass.
- Gate/scope: `M0.4.3 GATE FAIL` because the exact historical ValidationError path/root cause requirement cannot be proven from preserved evidence. Model calls=0; Run001–Run005 and production C#/XAML remain unchanged; no Run006, M0 Freeze, M1, commit, push or deploy. Ready for Supervisor to consider Run006: NO.
- Supersedes: Decision028's active PASS2 schema/output mechanics for future execution only. Decision028's historical M0.4.2 result, Run005 history and all higher-authority product/architecture decisions remain unchanged.

## TCC-DEC-2026-09-16-030 — Run005 Historical-Evidence Exception and M0-RUN-006 Preflight Stop

- Date: 2026-09-16; source: explicit Supervisor authorization for M0-RUN-006.
- Decision: M0.4.3 is accepted with one narrow historical-evidence exception: Run005's exact field-level PASS2 `ValidationError` cause cannot be reconstructed because its raw output was not captured. This exception does not allow invalid schema output and does not weaken raw-before-validation custody, canonical `M0.4.3_AUDIT`, fail-closed validation, two-stage references, enum/null semantics, traceability or the 8,000-token capacity ceiling.
- Run006 forward controls: preflight now verifies all six accepted Python partitions, the canonical contract and no-drift manifest, worst-case output at most 6,000, raw-before-validation enablement, traceability and Run001–Run005/production custody. PASS2 writes an immutable raw artifact plus `pass2_raw_custody.json` before validation; a structural failure additionally writes exact paths, expected/received values and types, constraint, raw/schema/prompt hashes without retry.
- Historical execution result: the refreshed M0.4.3 suite passed 40/40. The required Original suite then failed 227/228 because `test_run002_length_fixture_and_immutable_evidence` used the Windows CP950 default decoder for a UTF-8 fixture and raised `UnicodeDecodeError`. The explicit stop rule ended execution before the Run006 lock and before any paid call; remaining preflight partitions and PASS1–PASS4 were not executed.
- Custody/status: model calls=0; Run001–Run005, formal artifacts, HEAD and index preserved; production C#/XAML unchanged; no commit/push/deploy, Run007, M0 Freeze or M1. `M0-RUN-006 FAIL`; ready for M0 FINAL FREEZE REVIEW: NO.
- Supersedes: NONE. Decision029's repaired forward contract remains active; only its Run006-readiness conclusion is replaced by the explicit Supervisor exception and this actual preflight outcome.

## TCC-DEC-2026-09-16-031 — Explicit UTF-8 Repository Text Boundaries

- Date: 2026-09-16; source: explicit M0.4.3.1 offline repair authorization after accepted Run006 attempt-001 preflight abort.
- Decision: every repository-owned pipeline artifact contract defined as JSON, TXT, YAML, Markdown, fixture or report metadata must use explicit UTF-8 for reads and writes. Platform defaults, Windows ANSI code pages and global encoding monkey patches are forbidden. Intentional binary reads, including `PIL.Image.open`, remain byte/binary operations.
- Root cause: `test_run002_length_fixture_and_immutable_evidence` read the UTF-8 `m0_run002_length.json` fixture through `Path.read_text(encoding=None)`. On the active Windows host, `TextIOWrapper` selected CP950 and failed on byte `0x8C` at offset 226. The fixture was valid UTF-8 and its SHA-256 remained `1302354F6B5FB7A1B61F503085D3287F0F66D0D67EFBD08D2F9B472BE50E62E9`.
- Repair: the failing fixture read, its Run002 custody read and one same-class gate JSON read now use the shared `read_utf8_json` boundary. An AST-backed audit scanned 111 sites, records three repaired implicit text sites, nine intentional binary sites and zero remaining implicit UTF-8 text sites. Cross-locale tests cover CP950 simulation, CP950 rejection, non-ASCII round trip, locale-invariant JSON, immutable fixture hash and historical custody.
- Evidence/status: Original 228/228, M0.3 20/20, M0.4 18/18, M0.4.1 16/16, M0.4.2 27/27, M0.4.3 40/40 and M0.4.3.1 7/7 pass. The isolated offline Run006 preflight replay passes all existing gates with model_calls=0. Run001–Run005 and attempt-001 are unchanged; production C#/XAML unchanged; no Run006 paid directory, Run007, commit/push/deploy, M0 Freeze or M1.
- Gate: M0.4.3.1 `READY_FOR_SUPERVISOR_REVIEW`; ready for Supervisor to consider a new M0-RUN-006 paid execution authorization: YES. This is not paid execution authority.
- Supersedes: NONE. Decision030 remains the immutable record of Run006 attempt-001 preflight failure.

## TCC-DEC-2026-09-16-032 — M0-RUN-006 Attempt-002 PASS1 Length Stop

- Date: 2026-09-16; source: explicit Supervisor reauthorization of M0-RUN-006 with a new isolated attempt.
- Authorization: `attempt-002`, `openai/gpt-4.1`, fixed A → B → A → B order, at most four calls, no paid retry, no model replacement, no Run007, no M0 Freeze and no M1. Attempt-001 remains immutable preflight-abort evidence.
- Preflight: fresh Original/M0.3/M0.4/M0.4.1/M0.4.2/M0.4.3/M0.4.3.1 suites passed 228/228, 20/20, 18/18, 16/16, 27/27, 40/40 and 7/7. Eight image payloads, 2142×1196 master, Taste, formal geometry, zero remaining implicit UTF-8 text sites, Run001–Run005 custody, attempt-001 custody and production preservation passed.
- Execution result: PASS1 made one model call, resolved to `gpt-4.1-2025-04-14`, and reached the unchanged 14,000 completion-token ceiling with `finish_reason=length`. Raw output was captured before validation, but the truncated output was not parsed or accepted as a SemanticDraft; PASS1 schema is invalid/unavailable and executed passes remain 0/4.
- Stop/custody: the required fail-fast rule stopped before Host Guard 1, Pre-Audit, PASS2, PASS3 and PASS4. No retry occurred. Model calls=1; source/scope and historical custody passed during execution; production C#/XAML unchanged; no commit/push/deploy, Run007, M0 Freeze or M1. `M0-RUN-006 FAIL`; ready for M0 FINAL FREEZE REVIEW: NO.
- Supersedes: NONE. Decision031 remains the accepted offline repair and Decision030 remains immutable attempt-001 history.

## TCC-DEC-2026-09-16-033 — M0.4.4 Bounded PASS1 Semantic Ownership

- Date: 2026-09-16; source: explicit M0.4.4 offline PASS1 capacity-hardening authorization after accepted Run006 attempt-002 failure evidence.
- Root cause: the immutable PASS1 response consumed 14,000 tokens and 41,507 raw bytes. Insignificant whitespace accounted for 26,674 bytes; meaningful bytes were 14,833. All 23 regions completed, then JSON degraded at `open_questions[7].impact` and stopped with `finish_reason=length`. The old schema also permitted verbose claim fan-out, 30 three-field questions and repeated semantics, while the old prompt carried the full registry.
- Semantic ownership: active future PASS1 output is strict `M0.4.4_SEMANTIC`. Agent A owns bounded visual/content roles, short implementation notes, semantic relationships, component-family interpretation, fidelity risks, 30 uncertainty assessments and evidence IDs. Canonical/normalized geometry, dimensions, measurement status/provenance, hierarchy, spacing, full evidence records and required-region metadata remain Host-owned and absent from the Agent schema.
- Reference/compiler contract: PASS1 receives Host-resolvable Rxx/Exxx IDs, compact observations and Host relationship IDs instead of full evidence prose or registry duplication. The Host compiler resolves IDs, injects M0.3 geometry/evidence authority and creates the Candidate without fabricating missing Agent semantics. Future PASS3 manifests and atomic apply support the new bounded schema while historical SemanticDraft replay remains explicit and read-only.
- Capacity: theoretical maximum uses four-byte UTF-8 code points and exact compact serialization: 33,422 bytes / 10,445 estimated tokens. The unchanged 14,000 ceiling retains 3,555 tokens / 25.39%. PASS1 prompt is 78,843 → 16,032 bytes. Run006 captured semantic compaction replay estimates 2,429 tokens with no loss among complete captured items; missing truncated items are not fabricated. PASS2 remains 5,887 / 8,000.
- Evidence/status: existing partitions pass 228/228, 20/20, 18/18, 16/16, 27/27, 40/40 and 7/7; M0.4.4 passes 28/28. Raw-before-validation custody remains generic across PASS1–PASS4; strict gpt-4.1 schema compatibility, traceability, UTF-8 and historical custody pass. Model calls=0; production unchanged; no Run007, M0 Freeze, M1, commit, push or deploy. `READY_FOR_SUPERVISOR_REVIEW` is readiness evidence only and does not authorize another paid attempt.
- Supersedes: Decision024's active future PASS1 SemanticDraft shape only. Historical Run003–Run006 evidence and all later PASS2/patch/custody decisions remain unchanged.

## TCC-DEC-2026-09-16-034 — M0-RUN-006 Attempt-003 Preflight Authorization Stop

- Date: 2026-09-16; source: explicit Supervisor reauthorization of `M0-RUN-006` as isolated `attempt-003`.
- Preflight evidence: fresh Original/M0.3/M0.4/M0.4.1/M0.4.2/M0.4.3/M0.4.3.1/M0.4.4 suites passed 228/228, 20/20, 18/18, 16/16, 27/27, 40/40, 7/7 and 28/28.
- Stop reason: the live `authorized_run` entry still hard-codes `M0-RUN-006` to `attempt-002`. It rejected `attempt-003` before forward controls, lock creation, runner construction or provider execution. The explicit fail-fast rule prohibited repair or retry in the paid-execution turn.
- Custody/status: PASS1–PASS4 were not attempted; model calls=0; no raw response exists; production C#/XAML unchanged by this attempt; no commit/push/deploy, Run007, M0 Freeze or M1. `M0-RUN-006 FAIL`; ready for M0 FINAL FREEZE REVIEW: NO.
- Supersedes: NONE. Decision033 remains accepted offline repair evidence; this decision records the actual attempt-003 execution result.

## TCC-DEC-2026-09-16-035 — Run/Attempt/Execution Authorization Authority

- Date: 2026-09-16; source: explicit M0.4.4.1 offline repair authorization after accepted attempt-003 precheck failure evidence.
- Decision: `automation/tcc_master_pipeline/run_authorization.json` is the sole Run006 authorization authority. It binds `M0-RUN-006`, `attempt-003`, `execution-002`, paid permission, source/time, model and immutable history. CLI values must match it exactly; attempt numbers and execution instances are never inferred or incremented.
- Lifecycle/lock: legal startup is `AUTHORIZED → STARTING → LOCKED → RUNNING`; terminal `FAILED`, `CANDIDATE_PASS` and `ABORTED_PRECHECK` states cannot resume. The lock contains run, attempt and execution identities plus the manifest hash. Authorization and complete preflight precede lock acquisition; pre-lock failure evidence is retained with model_calls=0.
- Custody: attempt-001, attempt-002 and attempt-003 precheck evidence are hashed. A later paid execution uses `attempt-003/execution-002`, so it cannot overwrite the accepted precheck abort. Historical/unauthorized/duplicate identities fail closed.
- Evidence/status: existing partitions pass 228/228, 20/20, 18/18, 16/16, 27/27, 40/40, 7/7 and 28/28; M0.4.4.1 passes 18/18. Offline attempt-003 authorization/lifecycle/lock replay passes; model calls=0; production unchanged; no Run007, M0 Freeze or M1. `READY_FOR_SUPERVISOR_REVIEW` does not itself authorize paid execution.
- Supersedes: Decision032's execution-specific attempt authority for future Run006 starts only. Its historical attempt-002 outcome remains immutable.

## TCC-DEC-2026-09-16-036 — M0-RUN-006 Attempt-003 Execution-002 PASS3 Provider Stop

- Date: 2026-09-16; source: explicit paid authorization for `M0-RUN-006 / attempt-003 / execution-002`.
- Preflight/identity: all nine regression partitions passed; the manifest, attempt and execution identity matched; lifecycle advanced `AUTHORIZED → STARTING → LOCKED → RUNNING`. No duplicate identity or historical custody defect was found.
- Execution: PASS1 and PASS2 returned `gpt-4.1-2025-04-14`, `finish_reason=stop`, valid schemas and raw custody. Host Guard 1 reported 0 BLOCKER / 5 HIGH across three roots. Pre-Audit, reference integrity and Audit Coverage passed; Agent B supplied 23 coverage rows, zero independent findings and three CONFIRM root dispositions with zero auditor misses. Revision Base Manifest and 439-target Patch Target Registry were created.
- Stop: PASS3 was the third model call and failed with provider `BadRequestError` before a provider response/raw payload was available. The run retained `response_pass3.json` but correctly has no `raw_response_pass3.json`, patch set or atomic apply. No retry or PASS4 occurred.
- Custody/status: model calls=3; lifecycle `FAILED`; source, historical and production custody pass; production C#/XAML unchanged; no commit/push/deploy, Run007, M0 Freeze or M1. `M0-RUN-006 FAIL`; ready for M0 FINAL FREEZE REVIEW: NO.
- Supersedes: NONE. Decision035 remains the authorization authority; this decision records the consumed execution-002 outcome.

## TCC-DEC-2026-09-17-037 — PASS3 Provider Request Contract and Pre-Send Custody

- Date: 2026-09-17; source: explicit M0.4.5 offline repair authorization after accepted Run006 attempt-003/execution-002 failure evidence.
- Historical evidence boundary: the immutable run recorded `BadRequestError`, timestamp, no provider response and zero output, but did not retain exception repr/traceback, HTTP status, provider type/code/param/message, request ID, headers or error body. Those fields remain `UNAVAILABLE`; the historical request is marked `EXACT_REQUEST_NOT_FULLY_RECONSTRUCTABLE` because CrewAI's final composed system/message serialization and transport metadata were not captured.
- Root cause: installed `openai==2.54.0` deterministically serializes historical `SemanticPatchIntent.new_value: Any` as `{\"title\":\"New Value\"}`. This node has no type, reference, enum, const or union and is invalid at `response_format.json_schema.schema.$defs.SemanticPatchIntent.properties.new_value`. `STRUCTURED_OUTPUT_SCHEMA_INVALID` is the primary cause. Redundant full Registry plus target-catalog input is a measured secondary `PATCH_TARGET_PAYLOAD_OVEREXPANSION`, not the BadRequest cause.
- Repair: active `SemanticRevisionPatchSet` keeps only patch target, operation, bounded string/string-array value, finding IDs and reason. Host retains path, previous hash, base/concurrency authority and atomic apply. The complete 439-target Host registry stays local; a deterministic 416-target Agent catalog excludes 23 Host-owned objects, canonical paths and previous hashes. No arbitrary dict, unstructured output, schema relaxation, retry, model swap or higher token ceiling was introduced.
- Request boundary: every future PASS validates a canonical request and strict schema, writes secret-free request payload/custody plus request/schema/input hashes before the SDK send, then separately records response and validation/error evidence. `model_calls`, `provider_request_attempts`, `provider_responses_received` and `model_outputs_received` are independent counters. Live duplicate-execution rejection remains enabled; historical preflight replay explicitly bypasses only the existence check.
- Capacity/evidence: deterministic offline PASS3 replay passes request build, serialization, schema compatibility, references, custody and dry-send eligibility with provider_call=0. PASS3 theoretical maximum is 7,046 / 10,000 tokens, leaving 2,954 / 29.54%; PASS1 remains 10,445 / 14,000 and PASS2 5,887 / 8,000. Regression partitions pass 228/228, 20/20, 18/18, 16/16, 27/27, 40/40, 7/7, 28/28, 18/18 and M0.4.5 31/31.
- Gate/scope: `READY_FOR_SUPERVISOR_REVIEW`; model_calls=0, custody and traceability pass, production C#/XAML unchanged, Run007 absent and M1 not entered. This does not authorize paid execution, M0 Freeze, commit, push or deploy.
- Supersedes: Decision036's failed request-contract mechanics for future PASS3 only. Decision036 remains immutable execution history; Decision035 remains historical authorization authority until a new explicit authorization is issued.

## TCC-DEC-2026-09-17-038 — M0-RUN-006 Attempt-003 Execution-003 PASS1 Structural Stop

- Date: 2026-09-17; source: explicit paid reauthorization for `M0-RUN-006 / attempt-003 / execution-003` after acceptance of M0.4.5.
- Authorization/preflight: the manifest explicitly bound execution-003 and retained execution-002 by tree hash. Fresh Original/M0.3/M0.4/M0.4.1/M0.4.2/M0.4.3/M0.4.3.1/M0.4.4/M0.4.4.1/M0.4.5 partitions passed 228/228, 20/20, 18/18, 16/16, 27/27, 40/40, 7/7, 28/28, 18/18 and 31/31. Request-before-send custody, response-before-validation custody, budgets, UTF-8, traceability, historical custody and production preservation passed before the lock/provider boundary.
- Execution: PASS1 made one provider request and received `gpt-4.1-2025-04-14`, `finish_reason=stop`, 13,647 prompt tokens and 4,086 completion tokens. The provider response was valid JSON and was durably captured, but strict Host validation rejected the `M0.4.4_SEMANTIC` payload with `Composition and layering semantics are required`; the output contained only ten relationship rows and did not satisfy the complete composition/layering invariant.
- Stop/custody: fail-fast stopped before Candidate Compiler/Host Guard 1 and PASS2–PASS4. No retry, repair, model switch, ceiling increase or new execution occurred. Counters are model_calls=1, provider_request_attempts=1, provider_responses_received=1 and model_outputs_received=1. Source/scope/custody passed; production C#/XAML and formal specs were unchanged; no commit/push/deploy, Run007, M0 Freeze or M1.
- Gate: `M0-RUN-006 FAIL`; ready for M0 FINAL FREEZE REVIEW: NO.
- Supersedes: NONE. Decision037 remains the accepted M0.4.5 repair evidence; this decision records the consumed execution-003 result.

## TCC-DEC-2026-09-17-039 — PASS1 Semantic Completeness Contract

- Date: 2026-09-17; source: explicit M0.4.6 offline repair authorization after accepted Run006 attempt-003/execution-003 evidence.
- Root cause: the failure occurred in layer D, the cross-field semantic validator. M0.4.4 removed first-class composition and layering fields in favor of generic `relationships[2..10]`; its prompt did not require a LAYERING row, while the after-validator still required both COMPOSITION and LAYERING. The immutable execution-003 output therefore supplied six COMPOSITION rows and zero LAYERING rows despite passing provider and structural schema validation.
- Decision: active future PASS1 output is strict `SemanticDraftV46`. Composition and layering are separate required, non-null, non-empty Agent-owned sections. Composition contains exactly overall role, major grouping, primary emphasis and spatial relationship; layering contains exactly foreground, middle and background roles with stacking semantics. Each item carries resolvable Rxx/Exxx/Hxx evidence. Insufficient visual support uses an explicit uncertainty path and never omission, null, empty sections or misplaced summary prose.
- Alignment and ownership: one canonical semantic-completeness manifest defines required sections, minimum cardinality, evidence, uncertainty and validator rules. Prompt, schema and validator are checked for equality. Host geometry, bounding boxes, measurements and z-coordinates remain absent from Agent output and continue to be injected from accepted Host evidence.
- Capacity/evidence: PASS1 theoretical maximum is 32,420 bytes / 10,132 estimated tokens under the unchanged 14,000 ceiling, leaving 3,868 / 27.63%. PASS2 remains 5,887 / 8,000 and PASS3 7,046 / 10,000. The immutable raw execution-003 response replays to the same expected failure without modification; a complete V46 fixture and explicit uncertainty fixture pass. All eleven partitions pass 464/464.
- Gate/scope: `READY_FOR_SUPERVISOR_REVIEW`; model_calls=0; request/response/traceability/history custody pass; production C#/XAML unchanged; no Run007, M0 Freeze, M1, commit, push or deploy. This readiness result does not authorize paid execution.
- Supersedes: Decision033's active PASS1 schema shape for future executions only. Decision033's historical capacity evidence and Decision038's immutable execution-003 result remain authoritative history.

## TCC-DEC-2026-09-17-040 — M0-RUN-006 Attempt-003 Execution-004 PASS2 Structural Stop

- Date: 2026-09-17; source: explicit paid reauthorization for `M0-RUN-006 / attempt-003 / execution-004` after acceptance of M0.4.6.
- Authorization/preflight: the authorization manifest explicitly bound execution-004 and retained execution-002/execution-003 by tree hash. Fresh Original/M0.3/M0.4/M0.4.1/M0.4.2/M0.4.3/M0.4.3.1/M0.4.4/M0.4.4.1/M0.4.5/M0.4.6 partitions passed 228/228, 20/20, 18/18, 16/16, 27/27, 40/40, 7/7, 28/28, 18/18, 31/31 and 31/31. Eight image payloads, 2142×1196 master, Taste, M0.4.6 contract, request/response custody, traceability, history and production preservation passed before provider execution.
- PASS1: one request returned `gpt-4.1-2025-04-14`, `finish_reason=stop`, 14,532 prompt and 4,191 completion tokens. Strict `SemanticDraftV46` structural and semantic-completeness validation passed with 23 regions, four composition aspects and three layering roles. Host Guard 1 found 0 BLOCKER and one HIGH Character Zone first-level hierarchy root; the Pre-Audit Gate correctly allowed Agent B.
- PASS2 stop: the second request returned the same resolved snapshot with `finish_reason=stop`, 39,500 prompt and 1,042 completion tokens. Raw output and response custody were captured before validation. Strict `M0.4.3_AUDIT` validation rejected `findings.0.short_defect` because the supplied Chinese text exceeded the 56 UTF-8 byte maximum. The raw audit contained 23 coverage rows, one HIGH finding and one CONFIRM disposition, but it is not an accepted audit and Reference Integrity/Audit Coverage were not executed.
- Fail-fast/custody: no retry or automatic repair occurred. Revision Base, PASS3, Atomic Apply, Host Guard 3 and PASS4 were not executed. Counters are completed passes 1/4, model calls=2, provider requests/responses/outputs=2/2/2. Source and historical custody pass; production C#/XAML and formal specs unchanged; no commit/push/deploy, Run007, M0 Freeze or M1.
- Gate: `M0-RUN-006 FAIL`; ready for M0 FINAL FREEZE REVIEW: NO.
- Supersedes: NONE. Decision039 remains the accepted M0.4.6 contract; this decision records the consumed execution-004 outcome.

## TCC-DEC-2026-09-17-041 — PASS2 Multilingual Field-Bound Contract

- Date: 2026-09-17; source: explicit M0.4.7 offline repair authorization after accepted execution-004 failure evidence.
- Root cause: `CompactAuditFindingV43` exposed `maxLength=28` to the provider but applied a hidden Host-side maximum of 56 UTF-8 bytes. The capacity fixture used 28 copies of two-byte `é`, proving the byte limit was a compactness proxy. Agent B was never told the byte rule, and JSON Schema cannot express UTF-8 byte length through `maxLength`.
- Decision: active `CompactAuditV47` defines `audit_summary` as 8–48 and `short_defect`/`required_action` as 8–28 Unicode code points across the canonical contract, prompt, provider schema and Pydantic. Host post-validation only rejects ill-formed Unicode. Runtime truncation, byte slicing and automatic semantic repair remain prohibited and invalid output fails closed.
- Capacity: the active maximum is derived as 30 independent findings. A worst case using four-byte supplementary code points, 23 coverage rows and six dispositions/240 Host IDs is 19,029 serialized bytes / 5,947 estimated tokens under the unchanged 8,000 ceiling, leaving 2,053 / 25.66%; 31 findings estimates 6,084 and exceeds the 6,000 exit gate. PASS1 remains 10,132/14,000 and PASS3 remains 7,046/10,000.
- Replay/evidence: immutable execution-004 raw PASS2 fails V43 and validates under V47 without modification. Reference Integrity passes; RC05↔A001 is equivalent by root, R19, CHARACTER_FIRST_CLASS semantics and E022→EV_M03_REGION_CHARACTER_ZONE; independent HIGH A001 and one CONFIRM disposition remain; Audit Coverage is 23/23 and genuine AUDITOR_MISS roots are 0. This does not retroactively promote the raw audit or change execution-004 FAIL.
- Gate/scope: all twelve partitions pass 495/495. `READY_FOR_SUPERVISOR_REVIEW`; model_calls=0, request/response custody, traceability and historical custody pass; production C#/XAML unchanged; no Run007, M0 Freeze, M1, commit, push or deploy.
- Supersedes: Decision032/Decision040 PASS2 bound mechanics for future executions only. All historical execution results remain immutable.

## TCC-DEC-2026-09-17-042 — M0 Final Freeze

- Date: 2026-09-17; source: explicit Supervisor M0 FINAL FREEZE authorization after accepting `M0-RUN-006 / attempt-003 / execution-005`.
- Winning evidence: execution-005 completed all four fixed Passes with four requests/responses/outputs, valid schemas and normal finish reasons. Host Guard 3 is 0 BLOCKER / 0 HIGH; PASS4 unresolved HIGH and Final AUDITOR_MISS HIGH are zero; required regions are 23/23 with no AMBIGUOUS or geometry-critical UNDEFINED regions.
- Frozen truth: `work/m0_freeze/m0_frozen_candidate.json` and `m0_frozen_semantic_draft.json` are exact byte copies of the accepted revised artifacts. Host geometry authority, final Audit manifest, eight-image provenance chain and `M0_FREEZE_MANIFEST.json` preserve source hashes. `M0_FREEZE_LOCK.json` requires formal UNFREEZE/change control; M1 must create downstream derivatives and may not overwrite M0 truth.
- Residuals: PASS4 LOW A001 and A002 remain open in `m0_residual_findings.json` for the M1 backlog. They concern unsupported character/application-chrome rounded-corner or shadow claims and do not violate Safety, canonical geometry, traceability or semantic correctness gates.
- Transition/scope: M0 is FROZEN. M1 is AUTHORIZED but NOT STARTED; a separate Supervisor execution authorization is required. Freeze model_calls=0; historical execution custody passes; production C#/XAML unchanged; no Run007, commit, push or deploy.
- Supersedes: NONE. All historical Run001–Run006 outcomes and M0.4.x repair decisions remain immutable evidence.

## TCC-DEC-2026-09-17-043 — M1.1 Frozen-to-WPF Structural Mapping

- Date: 2026-09-17; source: explicit Supervisor authorization for `M1.1 — FROZEN SPEC TO WPF STRUCTURAL IMPLEMENTATION`.
- Decision: production uses a `ScrollViewer → Viewbox → 2142×1196 Canvas` canonical design space. Frozen pixel geometry is represented exactly inside the Canvas, while the Viewbox provides deterministic uniform scaling without changing composition.
- Hierarchy: all 23 required regions have unique production controls and traceability. Character Zone is a direct Viewport-level region. SafetyMetricCard and SecondaryInfoPanel remain separate WPF style families. Background Landscape and Plum Blossom/Snow decorations are Path-based artwork slots with no invented rectangular authority.
- Residuals/assets: A001 and A002 remain `DEFER_VISUAL_EVIDENCE`; no speculative Character Zone or Application Chrome shadow/rounding was added. The production master remains verification-only and was not copied into production assets; approved artwork remains pending for the three art slots.
- Evidence: deterministic validation reports 23/23 implemented, missing/duplicate/hierarchy/geometry critical errors 0, Character Zone PASS and non-rectangular exceptions 2. Locked restore passes; Release x64 build is 0 warnings / 0 errors; full tests pass 1700/1700; real app launch, XAML render and 2142×1196 client capture pass.
- Scope: M0 frozen artifacts are unchanged; no business, trading, risk or data-layer logic changed; no commit, push, deploy or M1.2 work occurred.
- Supersedes: the TCC-P6-85R2 Watchlist-first visual composition as the active production Home layout only. Its historical evidence remains preserved.

## TCC-DEC-2026-09-17-044 — M1.2 Evidence-Backed Visual Fidelity Layer

- Date: 2026-09-17; source: explicit Supervisor authorization for `M1.2 — VISUAL FIDELITY IMPLEMENTATION` after accepting M1.1.
- Decision: apply a reusable cold-dark WPF visual layer sampled from the provenance-locked master while retaining M0/M1.1 geometry and hierarchy. Local Home resources own background, surfaces, text, accent, borders, separators, state emphasis, opacity, radii, typography scale, line heights and spacing; High Contrast replaces these brushes with system colors.
- Evidence boundary: panel and control radii are limited to visually evidenced families. Character Zone and Application Chrome receive no speculative corner or shadow effects; A001/A002 remain `DEFER_VISUAL_EVIDENCE`. Exact font identity, final character/background/decoration art and icon family remain explicit M1.3 gaps.
- Regression: Visual Token Map contains 38 accounted decisions; Typography Map and Visual Coverage account for 23/23 regions with unknown states 0. M1.1 structural regression remains 23/23 regions, 21/21 rectangular geometry, Character Zone PASS and two non-rectangular exceptions. One Master comparison baseline was captured without iterative correction.
- Evidence: locked restore passes; Release x64 build is 0 warnings / 0 errors; full tests pass 1700/1700; real app launch, XAML render and independent 2142×1196 M1.2 capture pass. M0 frozen and M1.1 accepted baseline hashes remain unchanged.
- Scope: no business/trading/risk/data logic, package, project reference or dependency changed; no commit, push, deploy, M0 run, M1.3 or automated visual-regression loop occurred.
- Supersedes: Decision043's statement that visual fidelity remained M1.2 work. Decision043's accepted structural authority remains unchanged.

## TCC-DEC-2026-09-17-045 — M1.3 Evidence-Bounded Asset and Icon Integration

- Date: 2026-09-17; source: explicit Supervisor authorization for `M1.3 — ASSET / ICON / TYPOGRAPHY DETAIL INTEGRATION` after accepting M1.2.
- Decision: use one local, filled, 24-unit WPF Geometry family reconstructed from direct Master evidence for brand, navigation, command, safety, panel and empty-state icons. Keep M1.2 token ownership and M1.1 canonical region geometry/hierarchy unchanged. Visible implementation labels are replaced by production-safe unavailable states; no market/account data is fabricated.
- Asset boundary: the repository has no independent clean character, landscape, plum/snow, logo raster or authoritative font file. The composed Master/crops contain UI or playback-overlay contamination and are not promoted to production artwork. Character, landscape, decoration and exact font identity remain explicit deferred assets/change requests; no AI-generated or inpainted substitute is introduced. A001/A002 remain `DEFER_VISUAL_EVIDENCE`.
- Typography: retain the approved `font.family.ui` fallback because no authoritative font identity exists. Add pixel rounding, Display formatting, ClearType rendering and bounded compact/safety metrics to prevent clipping in locked geometry. Master title-case copy is used where directly visible.
- Evidence: Asset Inventory is 26 complete with 22 implemented and four deferred; Icon Map is 18/18; placeholder inventory improves 27→3 with owner/reason/future phase; traceability covers 23/23 regions. M0 8/8, M1.1 7/7 and M1.2 6/6 custody checks pass. One Master comparison improves mean absolute delta 29.782→29.299 and edge delta 16.025→15.645 with zero unexplained region regressions.
- Validation/scope: 23/23 regions, 21/21 rectangular geometry, two nonrectangular exceptions and Character Zone hierarchy pass. Locked restore and Release x64 build pass at 0 warnings / 0 errors; repository tests pass 1701/1701; real runtime and 2142×1196 M1.3 capture pass with XAML runtime errors 0. No M0 mutation, dependency change, commit, push, deploy, M1.4 or automated visual-regression loop occurred.
- Supersedes: Decision044's statement that final icon-family and typography-detail integration remained M1.3 work. Decision044's accepted visual-token architecture and baseline remain unchanged.

## TCC-DEC-2026-09-17-046 — M1.4 Evidence-Bounded Visual Regression Convergence

- Date: 2026-09-17; source: explicit Supervisor authorization for `M1.4 — VISUAL REGRESSION & FIDELITY CONVERGENCE` after accepting M1.3.
- Decision: compare the provenance-locked 2142×1196 Master with real Release production captures through a deterministic Pillow-based harness. Fidelity reporting keeps RAW MASTER DELTA separate from a foreground/edge-based FIXABLE-EVIDENCE DELTA and measures all 23 canonical regions across pixel, RMS, threshold, edge, perceptual, color, typography, spacing, asset, decoration and geometry dimensions.
- Implementation: three bounded repair iterations changed only shared WPF tokens/styles and observed content inside locked regions. Top-bar/status/search treatment, navigation rhythm and observed Calendar entry, header copy inset, shared Safety icon/text alignment and compact secondary-panel typography converged. A primary-command gradient experiment regressed its regional metric and was reverted. SafetyValue settled at 14/24 after the final runtime capture proved that 15 still clipped the longest mixed-language statement.
- Evidence boundary: M1-CR-001 through M1-CR-003 remain open. M1-CR-004 records the missing authority for the primary-command icy surface/halo. No Master crop became a production asset; no character, landscape, plum/snow, font, trading data, effect or Hallmark detail was invented. A001/A002 remain `DEFER_VISUAL_EVIDENCE`.
- Result: canonical alignment passes. M1.3→M1.4 Master mean delta improves 29.299→28.629, threshold coverage 57.909%→56.839%, edge delta 15.645→15.316 and perceptual similarity 0.898002→0.901080. Region classification is 19 improved, four stable, zero regressed; final fixable BLOCKER/HIGH are 0/0. M0 8/8, M1.1 7/7, M1.2 6/6 and M1.3 8/8 custody pass; geometry is 21/21 plus two nonrectangular exceptions; Character Zone hierarchy passes.
- Validation/scope: locked restore 6/6; Release x64 build 0 warnings / 0 errors; focused .NET 3/3, harness unit 4/4 and repository tests 1702/1702 pass; real final runtime and 2142×1196 capture pass with XAML errors 0. Final render SHA-256 is `3ED78F6DC336108F90B012A94D51CAFDC5A7C739690DA2D214814A3D11CCCF6E`. M0 is unchanged; no commit, push, deploy or M1.5 work occurred.
- Supersedes: Decision045's statement that formal automated visual-regression convergence remained M1.4 work. Decisions043–045 remain authoritative for accepted structure, visual-token architecture and asset/evidence boundaries.

## TCC-DEC-2026-09-17-047 — M1.4.5R Unified Style-Matched Scene Integration

- Date: 2026-09-17; source: explicit Supervisor authorization for `M1.4.5R — STYLE-MATCHED ASSET GENERATION / INTEGRATION` plus later direction that character facial features, clothing and art style follow the Production Master or supplied Gu Qinghan pack, that character/background be one coherent scene, and that the face remain unobstructed by UI controls.
- Strategy: the Production Master remains composition, region proportion, visual weight and UI-integration authority. The 24-image Gu Qinghan pack is face, costume, sword, material, atmosphere and painterly-style authority. The generated artwork may be new; it may not impersonate an original clean source or directly copy one reference image.
- Integration: one production raster paints character, moonlit winter landscape, architecture, mist, snow and restrained plum together under a single lighting direction, depth model, color grade and brush treatment. This replaces separate background/character/ornament layers that looked composited. Frozen Character Zone remains a direct Viewport child and all canonical geometry remains unchanged.
- Character clearance: the observed canonical face bbox is `x=1872..2042, y=202..427`. It has zero overlap with New Trade Plan, Review Checklist and Major Alerts, with 322 px, 38 px and 43 px horizontal clearance respectively. Hair/fur may pass behind translucent surfaces; facial features may not.
- Assets/provenance: controlled registry is 30 total: 23 `VECTOR_RECONSTRUCTED`, one `STYLE_MATCHED_GENERATED`, three `COMPOSITION_MATCHED_GENERATED`, three `DEFERRED`, and zero placeholder/unknown. One unified raster is compiled as explicit WPF `Resource`; intermediate generations remain isolated evidence. M1-CR-001, M1-CR-002 and M1-CR-004 are resolved; M1-CR-003 exact licensed font identity remains open.
- Result: M1.4→M1.4.5R mean delta improves 28.629→24.380, threshold coverage 56.839%→56.104% and perceptual similarity 0.901080→0.918300. Edge delta rises 15.316→16.702 because real painted detail replaces placeholders; 22 regions improve, one remains stable and zero regress. Frozen structural regression passes 23/23 regions, 21/21 geometry, two nonrectangular exceptions and Character hierarchy.
- Validation/scope: locked restore passes; Release x64 build is 0 warnings / 0 errors; repository tests pass 1703/1703; asset tests pass 6/6; runtime and 2142×1196 capture pass with XAML errors 0. M0 is unchanged; no commit, push, deploy or M1.5 work occurred. `ACTUAL_MODEL_IDENTIFIER_UNAVAILABLE` is recorded for image generation.
- Supersedes: Decision045/046 statements that character/background/plum artwork must remain deferred because no independent clean source exists. Their accepted structure, visual tokens, deterministic assets and M1.4 baseline remain authoritative.

## TCC-DEC-2026-09-18-048 — M1.4.6-B1 Complete Coexistence Scene Strategy

- Date: 2026-09-18; source: explicit Supervisor cancellation of `M1.4.6-B — direct layered collage integration` and authorization of `M1.4.6-B1 — B character-anchored complete coexistence scene generation`.
- Decision: stop generating character, background, foreground and atmosphere independently for later visual assembly. Candidate B remains the character identity authority, while the accepted M1.4.5R HOME render and M1.4.6-A safety map constrain composition. Each B1 candidate must jointly generate character, ground contact, rock, multi-depth plum, ice/snow, fog, landscape and one moonlight system as a single complete scene before any layer extraction is considered.
- Result: B1-A/B/C complete scenes and 2142×1196 review previews exist. Candidate B identity, plum, rocks, ice/snow, fog, unified moonlight and near/mid/far depth pass 3/3. Exact critical UI pixel protection and face-clearance checks report zero conflicts. No candidate is selected by Codex.
- Scope: review evidence only. Production, M0 and M1.4.5R remain byte-identical; no formal layer extraction, WPF integration, commit, push or deploy occurred. M1.4.6-B2 and M1.5 require separate authorization.
- Supersedes: the unexecuted direct layered-collage M1.4.6-B route. M1.4.6-A remains accepted historical exploration and reference evidence.

## TCC-DEC-2026-09-18-049 — M1.4.6-B2 B-Base Character Scale Candidates

- Date: 2026-09-18; source: explicit Supervisor authorization for `M1.4.6-B2 — B 基底人物尺度微調候選`, followed by binding corrections that the lower hand must grip the sword hilt and both feet must follow ergonomic standing direction and ground contact.
- Decision: use the selected B1-B complete coexistence scene as the sole base direction and vary only character presence across three review candidates: B2-A approximately +6%, B2-B approximately +10% and B2-C approximately +14%. Mountain, frozen lake, fog, snow, moonlight, plum, rocks, depth, identity, costume, weapon family and UI-safe composition remain invariant.
- Anatomy correction: every accepted candidate shows a closed lower-hand grip around the sword hilt with the guard below the fist. The weight-bearing front boot points screen-left/front with aligned leg direction and visible ground contact; the rear foot remains slightly behind with a stable outward angle. These are direct visual checks, not biomechanical simulation claims.
- Result: three normalized 2142×1196 scenes and UI previews plus one overview are complete. Exact class-A UI pixels are preserved, conservative face boxes have zero forbidden intersections, critical UI conflicts are 0 and the required scene elements pass 3/3. B2-B is recommended because it balances increased presence, scene depth and UI clearance; this recommendation is not a Supervisor selection.
- Scope: review evidence only. Production, M0, M1.4.5R and B1-B custody pass with zero drift. No production integration, commit, push, deploy or later-stage execution occurred.
- Supersedes: NONE. Decision048 remains the B1 complete-scene authority; this decision adds the authorized B2 scale exploration.

## TCC-DEC-2026-09-18-050 — M1.4.6-B2 Face-Style and Lower-Body Correction

- Date: 2026-09-18; source: Supervisor rejection of the first B2 delivery because its facial rendering drifted from Gu Qinghan/Candidate B and some candidates exposed feet in implausible anatomical positions.
- Decision: reject the first B2 A/B/C images and their anatomy acceptance. Use the original Candidate B face crop as the sole facial identity and rendering-style authority. Lock the refined oval proportions, eyebrow/eye/nose/lip relationships, mature expression, cool skin material and semi-realistic 3D-painterly Chinese fantasy key-art treatment. Background and body scale do not authorize facial reinterpretation.
- Lower-body rule: exactly one weight-bearing boot may be visible, connected naturally beneath the robe and pointing screen-left/front with full snow contact. The other leg and foot are fully concealed by the long robe and foreground depth. Any second boot, toe, heel or footwear-like fragment is a rejection condition.
- Result: corrected B2-A/B/C scenes and 2142×1196 UI previews replace the rejected delivery. Face-style comparison and lower-body comparison boards provide direct review evidence. Face authority checks pass 3/3, visible boot count is exactly one for 3/3, duplicate/misplaced-foot count is 0, class-A UI conflicts and face/forbidden intersections are 0. B2-B remains a recommendation only.
- Scope: isolated candidate evidence only. Production, M0, M1.4.5R and B1-B remain unchanged; no commit, push, deploy, integration or later stage occurred.
- Supersedes: Decision049's acceptance of the first B2 candidate images and two-foot anatomy result. Decision049's authorization, scale targets and review-only boundary remain in force.

## TCC-DEC-2026-09-18-051 — M1.4.6-B2R Integrated Full-Scene Regeneration

- Date: 2026-09-18; source: Supervisor rejection of both facial drift and the visibly composited character/background result, followed by direction to generate the entire visual set together while prioritizing character fidelity.
- Decision: reject the character-locked layered composite. Generate character, landscape, rocks, plum, ice, snow, fog and moonlight together as one complete scene. Candidate B's full RGBA artwork and original face crop remain the identity/style authorities, but no character cutout is pasted onto the resulting environment.
- Composition: the active candidate retains one coherent light, atmosphere, material response and edge treatment. A 24 px whole-scene reframe moves the already integrated scene as one raster so the face clears Review Checklist and Major Alerts; it does not separate or redraw the character.
- Result: one 2142×1196 integrated scene and HOME UI preview are review-ready. Critical UI conflicts and face/forbidden intersections are 0. Exactly one grounded boot is visible; duplicate/misplaced-foot count is 0. A direct original-face/generated-face comparison is included so Supervisor can judge remaining identity variance rather than relying on a generic PASS statement.
- Scope: isolated candidate evidence only. Production, M0, M1.4.5R and B1-B remain unchanged; no commit, push, deploy, WPF integration or later-stage execution occurred.
- Supersedes: Decision050's corrected B2 images as the active review candidate and the rejected B2R layered composite. The identity, UI-safety and anatomy constraints remain authoritative.

## TCC-DEC-2026-09-18-052 — M1.4.6-B2R.1 Same-Source WPF Layer Feasibility

- Date: 2026-09-18; source: explicit Supervisor authorization for `M1.4.6-B2R.1 — 共生場景分層可行性硬化`.
- Character freeze: the accepted B2R scene is the immutable character authority. Face, features, hair, identity, clothing, weapon, pose, scale, principal position and calm tone may not be regenerated, replaced or retouched. Exact source-to-candidate face comparison reports zero changed pixels.
- Layer strategy: derive every visual layer from the same accepted B2R scene. The logical ten-layer order is implemented with four runtime rasters: the exact B2R base preserves background, character and grounded scene; the UI plane preserves the accepted interface; a cropped same-source foreground overlay supplies local UI interleaving; and a cropped same-source atmosphere overlay supplies light snow/mist. The clean plate is engineering evidence for feasibility only and does not replace the frozen runtime art.
- Safety contract: class A excludes all crossing; class B permits only bounded low-opacity atmosphere with protected text/control interiors; class C permits expressive foreground overlap. All artwork layers are `IsHitTestVisible=false`. Critical overlaps and readability violations are 0; eight representative WPF controls remain hit-testable.
- Evidence: isolated WPF runtime, XAML and resources pass with 0 errors. 100%/125%/150% DPI render at 2142×1196, 2678×1495 and 3213×1794 respectively and pass 3/3. Alpha QA passes. Four runtime rasters consume an estimated 33.104 MiB, classified MEDIUM and within the stage gate.
- Scope: the harness remains outside the repository and the review artifacts remain under `automation/tcc_master_pipeline/work/m1_4_6_b2r1`. Production, M0 and the B2R source custody all pass. No production WPF integration, character modification, image generation, commit, push, deploy, M1.5 or Production Freeze occurred.
- Status: `M1.4.6-B2R.1 CANDIDATE PASS / AWAITING SUPERVISOR REVIEW`. Formal WPF layer integration requires separate authorization.
- Supersedes: Decision051 only for the active review stage. Decision051's accepted integrated scene remains the frozen visual source and character authority.

## TCC-DEC-2026-09-18-053 — M1.4.6-B2R.1a Supervisor-Addressable Coordinate System

- Date: 2026-09-18; source: explicit Supervisor authorization for `M1.4.6-B2R.1a — 人工可指定穿插區域座標系統` after accepting and freezing B2R.1 as the integration baseline.
- Coordinate authority: keep 2142×1196 as the sole engineering coordinate space. Partition it into 32 columns and 18 rows with `round(i×2142/32)` and `round(j×1196/18)` boundaries. Every cell is left/top inclusive and right/bottom exclusive; the 576 cells cover all 2,561,832 pixels with zero gap and zero overlap. Normalized bounds are stored beside pixel bounds.
- Semantic identity: preserve B2R.1's A/B/C rules while assigning stable individual identities A01–A15, B01–B04 and C01–C03. Each region stores pixel/normalized geometry, intersecting cells, default artwork policy, intrusion limit and critical/interactive flags. Region→Grid and Grid→Region indexes are both complete.
- Selection contract: Sxxx supports one cell, rectangular ranges, disjoint unions, semantic regions and region/grid intersections. Rules store included/excluded cells, pixel/normalized geometry, zone override, element allow/deny lists, intrusion, opacity and hit-test state. Critical UI Hard Protection remains priority one and unsafe relaxation returns `HARD_PROTECTION_CONFLICT`; it is never silently applied.
- Evidence: all ten required artifacts exist, including four visually inspected reference images and a Traditional-Chinese legend with six examples. Focused tests pass 4/4; locked restore passes 6/6; Release x64 build passes with 0 warnings/0 errors; full .NET tests pass 1703/1703. An additional non-gating legacy automation sweep passes 515/518; its three failures are pre-existing environment/pre-M1 custody assumptions and did not originate in B2R.1a.
- Scope: B2R.1's 28-file baseline and manifest hashes have zero drift; current Production and M0 custody checks pass. No model or image-generation call, artwork modification, MainWindow/resource change, commit, push, deploy or B2R.2 work occurred.
- Status: `M1.4.6-B2R.1a PASS`. Supervisor may now address `Bxx`/`Cxx`, `XnnYnn` ranges or request a saved `Sxxx` selection.
- Supersedes: NONE. Decision052 remains the frozen integration baseline and layer-feasibility authority.

## TCC-DEC-2026-09-18-054 — M1.4.6-B2R.1b Limited Intrusion Pilot v1

- Date: 2026-09-18; source: explicit Supervisor authorization for `M1.4.6-B2R.1b — LIMITED INTRUSION PILOT v1`.
- Rule model: keep A01, A02, A03, A05, A10, A13, A14 and A15 as their original A-class semantic regions. Add S101–S108 as bounded child selections defined in each source region's local normalized coordinate space. Convert local geometry deterministically to 2142×1196 authority pixels, global normalized geometry and intersecting 32×18 cells.
- Safety operation: compute `Requested Geometry - Hard Protection = Effective Allowed Geometry`. Text, values, icons, input, chart core and principal click content are removed before a selection becomes effective. The result is stored as exact non-overlapping pixel rectangles; all selections have `hit_test_visible=false` and zero remaining Critical UI overlap.
- Element policy: level-one mist/snow/cold-light elements are allowed within per-selection constraints. Level-two single petals or faint plum shadows are limited to the specifically authorized selections. Thick/complete plum branches, character body/hair, cape, sleeve and large rocks remain forbidden throughout Pilot v1.
- Overconstraint: S101 removes 72.392% and S102 removes 64.780% of requested geometry; both exceed the 60% threshold and are flagged `SELECTION_OVERCONSTRAINED` without discarding their remaining safe geometry. S103–S108 do not cross the threshold. S104 is the most conservative by depth/opacity policy; S105 is the most open by area, corner depth and 0.42 opacity.
- Evidence: all eight required artifacts and three visually inspected maps exist. Focused tests pass 4/4; locked restore passes 6/6; Release x64 build passes with 0 warnings/0 errors; full .NET tests pass 1703/1703. Model and image-generation calls are 0.
- Scope: no actual artwork is placed into UI. B2R, character, B2R.1a, Production and M0 custody have zero drift. No MainWindow/resource change, commit, push, deploy or B2R.2 execution occurred.
- Status: `M1.4.6-B2R.1b PASS / AWAITING SUPERVISOR REVIEW`. Review is specifically required for S101/S102 overconstraint and S105 openness before later artwork placement.
- Supersedes: NONE. Decisions052–053 remain the frozen visual/layer baseline and coordinate authority.

## TCC-DEC-2026-09-18-055 — M1.4.6-B2R.2 Formal HOME WPF Scene Integration

- Date: 2026-09-18; source: explicit Supervisor authorization for `M1.4.6-B2R.2 — HOME 正式 WPF 場景融合`, including formal acceptance of S101–S108 and the retained overconstraint of S101/S102.
- Production authority: the exact B2R.1 locked scene becomes the HOME base raster without changing the person, face, pose, scale, position, costume, weapon or complete scene. The production copy retains SHA-256 `DDEB846F17EA696072B5715F01B4524C5B2B5214F526B75EAF69432B66586A03`.
- Layer strategy: production uses three physical runtime layers while preserving the logical L0–L9 order: the frozen scene, the existing WPF UI tree and one static non-interactive intrusion bitmap. The intrusion bitmap is deterministically derived from heavily blurred scene-native pixels, clipped to the exact B2R.1b Effective Allowed Geometry and placed above UI only where authorized. It contains no distinguishable character/body/branch/rock detail and has no animation.
- Coverage: S101–S108 actual artwork coverage is 2.997%, 3.999%, 7.997%, 2.998%, 9.999%, 8.000%, 7.997% and 4.999% respectively. All remain under the approved maxima; S108 stays below S106/S107. Critical UI overlap, readability violations, hard-protection overlap and Market Overview core artwork are all 0.
- Runtime result: the formal production MainWindow renders at 2142×1196, 2678×1495 and 3213×1794 for 100/125/150% DPI. Navigation, Search, New Trade Plan, Review Checklist, top bar, all four Safety cards and the four workspace panels pass 13/13 hit testing because artwork remains `IsHitTestVisible=false`. XAML and resource errors are 0.
- Performance: two runtime rasters decode to 19.546 MiB, with a 9.773 MiB decoded integration increment over one base raster. The first formal HOME show measured 727.330 ms in the isolated validation process. Static rendering and no per-frame bitmap work keep the formal risk at MEDIUM.
- Validation/scope: locked restore passes 6/6; Release build passes with 0 warnings/0 errors; focused checks pass 7/7; full repository tests pass 1708/1708. M0 Frozen hashes pass. No model/image-generation call, material/font/button/blur/glass/frost change, animation, other page, M1.4.7/M1.5 work, commit, push or deploy occurred.
- Status: `M1.4.6-B2R.2 PASS / AWAITING SUPERVISOR REVIEW`. Later work requires separate authorization.
- Supersedes: Decision054 only for the active stage and its prohibition on artwork insertion. Decisions051–054 remain authorities for the frozen scene, character identity, layer feasibility, coordinate system and intrusion geometry.

## TCC-DEC-2026-09-18-056 — M1.4.6-B2R.2A Scene-First Semantic Authority

- Date: 2026-09-18; source: explicit Supervisor authorization for `M1.4.6-B2R.2A — 背景場景語意與穿插潛力分析`.
- Analysis authority: the byte-identical 2142×1196 B2R frozen complete scene is the sole visual source. UI whitespace may not justify artwork. Every later intrusion claim must trace to an existing `SCxxx` element, its observed location, spatial depth, physical form and direction.
- Scene model: 40 stable elements cover sky, moon/light, clouds, mountain/forest/building systems, lake/reflection/cracks, four fog roles, snow/rock/plum/particle systems and subdivided character/body/material/weapon/ground-contact roles. Spatial counts are far 8, mid 8, near 9 and foreground 15. Density combines local luminance variation, edge quantity, color-channel variation and semantic detail rather than brightness alone.
- Intrusion model: 13 candidates retain source IDs, source positions, direction, maximum extension, opacity, density, UI suitability, foreground-layer need and form. Twelve scene-naturality forbidden zones remain 0% artwork even if later UI safety permits placement. S101–S108 are reference geometry only and are neither modified nor redefined.
- Grid authority: the existing 32×18 coordinate system remains unchanged. All 576/576 cells store primary and secondary scene semantics, depth, multi-factor density, brightness, direction, natural potential and recommended/prohibited forms. Full-frame snow, haze and moonlight remain traceable secondary atmosphere but are capped during per-cell potential classification so they cannot falsely make every cell a high-potential insertion source.
- Evidence and scope: five reviewed maps, six required JSON datasets, naturality rules, validation evidence and HTML/TXT one-click reports are isolated under `automation/tcc_master_pipeline/work/m1_4_6_b2r2a`. Formal HOME, MainWindow, production assets, character, B2R and S101–S108 custody are unchanged; model/image-generation calls are 0. No Background × UI joint analysis, artwork edit, commit, push or deploy occurred.
- Status: `M1.4.6-B2R.2A PASS / AWAITING SUPERVISOR REVIEW`. Background × UI joint analysis requires separate explicit authorization.
- Supersedes: NONE. Decision055 remains the formal production integration authority; Decisions051–054 remain the frozen scene, character, coordinate and selection authorities.

## TCC-DEC-2026-09-18-057 — M1.4.6-B2R.2B Scene × UI Causal Fusion Plan

- Date: 2026-09-18; source: explicit Supervisor authorization for `M1.4.6-B2R.2B — 場景 × UI 聯合融合規劃`.
- Causal contract: every fusion proposal must form `SC source → natural direction → narrow extension corridor → UI contact → fusion form`. Existing SC recognition bounding boxes are never valid masks. UI whitespace and S-selection capacity do not create an artistic reason, and 0% remains a valid preferred result.
- Safety-container meaning: S101–S108 remain unchanged upper-bound containers. They say where artwork may reach after Hard Protection; they do not require artwork, minimum coverage or per-component effects. The minimum effective set fits existing containers, so no S container is added or removed.
- Classification: A is true foreground intrusion, B is environmental influence recorded as future lighting/material rules only, C is conditional edge intrusion and D is prohibited. Moonlight (`SC003`) and lake reflection (`SC013`) remain B-class and cannot be pasted onto cards. Hair, cape and hem remain C-class or prohibited; low-confidence character candidates are not auto-approved.
- Joint evidence: all 13 high/medium-potential elements have distinct directional corridors with source anchors, directions, maximum distance/width, depth, opacity, density, forbidden direction and termination conditions. The complete 13×8 matrix records 104 SC→S relations: 21 natural, 11 conditional, 11 environmental, 3 prohibited and 58 unrelated. A direct 247-entry SC→UI mapping covers 19 named UI/whitespace regions.
- Priority and confidence: 38 bounded candidates classify as P1 8, P2 11, P3 9 and P4 10; confidence is high 22, medium 12 and low 4. Low-confidence auto-approvals are 0. Eighteen 0% zones protect identity, Safety, primary actions, status/table/chart content, character anatomy and lake perspective.
- Minimum effective set: five proposed A-class contacts form the conservative next construction boundary: `SC017→S105/Market`, `SC018→S106/Risk`, `SC018→S107/Open`, `SC017→S108/Activity` and `SC030→S103/Page Title outer safety band`. Each contact intersects existing Effective Allowed Geometry and retains 0% as the fallback if runtime evidence fails.
- Evidence and scope: four visually reviewed maps, seven required JSON datasets, validation evidence and HTML/TXT one-click reports are isolated under `automation/tcc_master_pipeline/work/m1_4_6_b2r2b`. Formal HOME, MainWindow, production assets, character, B2R and S101–S108 custody are unchanged; model/image-generation calls are 0. No material, animation, artwork generation, fusion construction, commit, push or deploy occurred.
- Status: `M1.4.6-B2R.2B PASS / AWAITING SUPERVISOR REVIEW`. Second formal fusion construction requires separate explicit authorization and is limited to the approved minimum set.
- Supersedes: NONE. Decisions055–056 remain the production and scene-semantic authorities; Decisions051–054 remain the frozen scene, character, coordinate and safety-container authorities.

## TCC-DEC-2026-09-18-058 — M1.4.6-B2R.2C 第二版融合靜態預演

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B2R.2C — 第二版融合靜態預演` 的明確授權。
- 預演方法：以正式 HOME 與 byte-identical B2R 凍結場景為唯一來源，從原場景採樣湖霧、地霧與飄雪像素，沿 B2R.2B 核准的來源、方向、走廊、接觸窗與安全遮罩作確定性延伸。甲／乙／丙僅調整透明度、局部密度、有效延伸量與柔化程度；沒有生成或重畫藝術元素。
- 安全裁決：第一輪離線 Guard 在 FC004 `SC017→Activity` 找到 775 個效果像素進入凍結人物保護區，因此沒有放寬保護，並將 FC004 在甲、乙、丙與核心三組版全部設為 0%。最終四版的 Critical UI overlap、可讀性違規與人物保護區像素漂移均為 0。
- 視覺結果：甲版、乙版與核心三組診斷版可接受；乙版是 Supervisor 優先檢視候選，但不是正式選定。丙版僅保留為上限反例，因 FC003 在局部對照中開始暴露矩形來源亮塊與輕微貼圖風險。FC005 只允許甲／乙極少量；FC004 不建議保留。
- 證據：甲／乙／丙、核心三組、四版總覽、五張局部比較、四份結構化 JSON 及具一鍵複製功能的 HTML/TXT 報告均位於 `automation/tcc_master_pipeline/work/m1_4_6_b2r2c`。5/5 關係可追溯；model calls 與 image-generation calls 均為 0。
- 範圍：正式 HOME、人物、B2R、S101–S108、production C#/XAML/asset 皆未修改；沒有材質、動畫、commit、push 或 deploy。正式 WPF 融合仍需 Supervisor 選定候選／強度並另行授權。
- 狀態：`M1.4.6-B2R.2C CANDIDATE VALIDATION COMPLETE / AWAITING SUPERVISOR REVIEW`。
- 取代：NONE。Decision057 仍是因果走廊與最小集合權威；Decisions051–056 仍是凍結場景、人物、座標、安全容器、production 與場景語意權威。

## TCC-DEC-2026-09-18-059 — M1.4.6-B2R.2D 人物邊緣共生聯合驗證

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B2R.2D — 人物邊緣共生聯合驗證` 的明確授權。
- 方法：沿用 B2R.2C 核心三組基底，只從 byte-identical B2R 凍結場景擷取同座標人物外緣像素；沒有平移、拉伸、複製、生成或重畫人物／衣料。A 為 0% 控制組，B 為輕量，C 為平衡上限。
- 合格來源：CE001 只使用與 Today's Priorities 右界相鄰的 SC034 披風／薄紗外緣；CE002 只使用與 Activity 右界相鄰的 SC034／SC036 薄紗與衣擺外緣。SC033 頭髮、SC035 實體衣袖與 Open Positions 因距離或空間連續性不足維持 0%，不因 UI 留白而強行穿插。
- 安全結果：人物臉部與人物核心像素差異均為 0；人物尺寸、姿勢、位置、服裝、武器、足部與接地區未變。Critical UI overlap 與可讀性違規均為 0，Alpha 白／黑邊、異色光暈、透明 RGB 污染及來源 RGB 漂移未檢出；候選層為靜態非互動證據。
- 視覺裁決：A、B、C 均可接受；B 在可感知共生與克制之間最佳，列為 Supervisor 優先檢視候選但未正式選定。C 僅代表可接受上限，禁止再提高強度。人物薄紗／衣擺外緣在右側局部形成的前後關係，比單純霧／雪更具體。
- 證據與範圍：三版成品、總覽、四張局部比較、凍結比較、來源追蹤、Alpha QA、六份 JSON 與 HTML/TXT 一鍵複製報告共 19 項，位於 `automation/tcc_master_pipeline/work/m1_4_6_b2r2d`。model calls 與 image-generation calls 均為 0；正式 HOME、人物、B2R、S101–S108 與 production C#/XAML/assets 未修改；沒有材質、動畫、commit、push 或 deploy。
- 狀態：`M1.4.6-B2R.2D CANDIDATE VALIDATION COMPLETE / AWAITING SUPERVISOR REVIEW`。正式 WPF 人物邊緣施工需 Supervisor 選定候選並另行授權。
- 取代：NONE。Decision058 仍是自然霧／雪融合基底權威；Decisions051–057 仍保有各自的凍結場景、人物、座標、安全容器、production、語意與因果走廊權威。

## TCC-DEC-2026-09-18-060 — M1.4.6-B2R.2E 梅枝／梅花前景共生壓力測試

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B2R.2E — 梅枝／梅花前景共生壓力測試` 的明確授權。
- 方法：A 精確沿用 B2R.2D 乙版並將紅梅前景設為 0%；B/C 只在原座標前置 B2R 同源紅梅像素；D 提高同源強度並加入 SC025 中景紅梅作景深上限測試；Diagnostic 改用 B2R.2C 核心三組基底，關閉人物薄紗增益而保留 C 強度紅梅。沒有生成、平移、縮放、複製或內容感知補圖。
- 來源裁決：SC027 右下前景梅枝／梅花是唯一主要來源，因其前景深度、右下→左上方向及與 Activity／右下卡片群的同座標接觸完整。SC026 右上前景只允許微量末端作陪襯。SC025 只屬 D 壓力來源；SC024 遠景與 SC028 左下模糊梅維持 0%。
- 視覺結果：B、C、Diagnostic 都產生可感知增益；Diagnostic 證明紅梅前景是獨立有效變量。C 在整頁融合、景深與可讀性間最佳，列為 Supervisor 優先檢視候選但未正式選定。D 的 SC025 前置造成可辨識的景深跳躍與貼圖／人工風險，因此只保留為非產品上限反例。
- 安全與量測：五版 byte-distinct，B<C<D 平均通道差異順序通過；相對 A 的變更像素分別為 B 12,264、C 12,882、D 15,627。Critical UI overlap、可讀性違規、人物保護重疊、臉部／人物核心差異及來源連續性違規皆為 0；Alpha QA 與 custody 均通過。
- 證據與範圍：五版成品、總覽、五張局部比較、人物凍結、來源追蹤、Alpha QA、六份 JSON、摘要與 HTML/TXT 一鍵複製報告共 23 項，位於 `automation/tcc_master_pipeline/work/m1_4_6_b2r2e`。model calls 與 image-generation calls 均為 0；正式 HOME、WPF、人物、B2R、S101–S108 及 production assets 未修改；沒有材質、互動、動畫、第二場景、commit、push 或 deploy。
- 狀態：`M1.4.6-B2R.2E CANDIDATE VALIDATION COMPLETE / AWAITING SUPERVISOR REVIEW`。正式梅枝前景 WPF 施工需 Supervisor 選定 C 或指定修正並另行授權。
- 取代：NONE。Decision059 仍是人物薄紗基準權威；Decisions051–058 仍保有凍結場景、人物、座標、安全容器、production、語意、因果走廊與自然霧／雪融合權威。

## TCC-DEC-2026-09-18-061 — M1.4.6-B3.0 新主底圖與參考影片雙重視覺權威

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B3.0 — 新主底圖接管與參考影片標準建立` 的明確授權。
- 權威分工：新主底圖是場景、人物、景深、月光、環境色與自然前景來源的唯一新權威；參考影片是 UI/UX 完成度、材質、資訊層級、空間關係、動態與互動原則的權威。影片提供原理，不要求逐像素複製，也不得改寫 canonical TCC 功能。
- 座標與語意：新圖建立 `B3SC001–B3SC018`，涵蓋遠景 3、中景 6、近景 4、前景 5，並採 2142×1196 工作座標。舊 `SC001–SC040`、SC025/026/027、人物／梅枝遮罩、延伸走廊、像素座標與景深區域均廢棄為新圖權威；只繼承來源連續性、0% 合法、中景不硬前置、人物核心保護、零 Critical overlap 與 Alpha 品質等方法。
- 人物與前景：臉、頭、身體、手、武器及來源未完整揭露的足部／接地是永久保護區。髮絲／薄紗／衣擺只允許同源同位低強度共生；左梅枝與右下梅枝是最有價值的強候選，但仍以可讀性與 Critical overlap 0 為必要條件。
- 方向裁決：方向 A 用於檢查參考影片的高密度完成度；方向 B 將 Safety Core 整合為治理帶、放大 Market 決策畫布、群組 Risk／Positions／Activity，並保留右側約 26–30% 人物／場景空間。Supervisor 應優先審查 B，再用 A 檢查完成度遺漏；B 目前只是候選，並非 frozen production。
- 證據與範圍：來源 manifest、10 張影片關鍵影格、A–T 設計語言、八層景深、光線、人物保護、前景候選、G1–G12 差距、兩張方向稿、比較圖、結構化 JSON 與一鍵複製報告位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_0`。model calls 與 image-generation calls 均為 0；來源、新人物、新背景、production WPF/assets 皆未修改；沒有 commit、push 或 deploy。
- 狀態：`M1.4.6-B3.0 PASS / AWAITING SUPERVISOR REVIEW`。B3.1 需另行核准／授權，未自動開始。
- 取代：Decision060 的舊底圖視覺候選不再是正式主視覺方向；Decision060 的方法驗證結論仍被繼承。Decisions051–059 保留為歷史證據，不得提供新圖像素權威。

## TCC-DEC-2026-09-18-062 — M1.4.6-B3.0R 高精度基線與 GOLD TARGET 收斂

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B3.0R — 高精度基線恢復與 HOME GOLD TARGET 收斂` 的明確授權。
- 權威順序：canonical TCC 功能需求優先，其次是 M1.1 已驗證 HOME 幾何、M1.2／現行 XAML 基礎視覺、B3 新場景、參考影片品質語言與 Taste。B3.0 A／B 只保留比較與產品思考，不提供幾何權威。
- 幾何裁決：21 個矩形元件在 2142×1196 座標中以 ΔX／ΔY／ΔW／ΔH 全 0 恢復；Safety Core 維持 Trading Permission、Total Risk、Current Positions、Major Alerts，Market Overview 與 Top/Side Bar 均採既有權威。三項概念幾何只列為 `NOT_APPLIED_AWAITING_SUPERVISOR` 提案。
- 基線與材質：先完成 `B3_HOME_BASELINE_RESTORED.png` Gate，再建立 `B3_HOME_GOLD_TARGET.png`。GOLD 沒有幾何改動；11 項玻璃透射、霜邊、環境反射、按鈕冷光與同源前景差異全部可逆並記入 `b3_style_delta_ledger.json`。
- 未知值政策：精確 FontFamily、LetterSpacing、三類 Shadow、Panel Blur、圖表 live line／point 與 Pressed-state 共九項沒有核准 Authority，維持 `UNKNOWN_NO_AUTHORITY`，沒有以目測或影片推算補值。
- 人物與前景：人物、新底圖與來源 hash 保持不變。只以前景層使用 B3SC018 同源同位髮絲／薄紗／衣擺外緣，以及 B3SC014／B3SC016、少量 B3SC015 的原像素梅枝；人物核心永久排除。Critical UI overlap 與可讀性違規均為 0，Alpha QA 通過。
- Runtime 證據：隔離 WPF Visual Tree 在 100／125／150% DPI 產出 2142×1196、2678×1495、3213×1794 原始截圖；位置尺寸漂移 0。前景藝術層 `IsHitTestVisible=false`，14/14 互動代理命中通過。
- Repository Gate：locked restore 6/6 projects；Release x64 build 0 warnings／0 errors；完整回歸 1708/1708 PASS，failed 0，skipped 0。
- 範圍與狀態：24 項強制產物與支援證據位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_0r`。額外模型與圖像生成呼叫皆為 0；production HOME／WPF、新底圖、人物未修改；未進入 B3.1；沒有 commit、push 或 deploy。狀態為 `PASS / AWAITING SUPERVISOR REVIEW`。
- 取代：Decision061 的 B3.0 A／B 概念方向不再能充當 HOME 幾何候選；Decision061 的新場景／影片雙重權威分工繼續有效。正式 B3.1 WPF 施工仍需 Supervisor 另行授權。

## TCC-DEC-2026-09-18-063 — M1.4.6-B3.1 HOME 靜態視覺系統正式 WPF 落地

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B3.1 — HOME 靜態視覺系統正式 WPF 落地` 的明確授權與對 B3.0R 的 ACCEPTED 裁決。
- 幾何裁決：B3.0R 的 21 個矩形元件是本階段不可變更的 Geometry Authority。正式 compiled production MainWindow Runtime 量測結果為 21/21，ΔX／ΔY／ΔW／ΔH 非零數全部為 0；沒有用 Margin、Padding、Grid、Span 或 Alignment 偷改版面。
- 材質裁決：B3.0R 的 Panel 216、PanelDeep 222、PanelInner 230、Border 184、Separator 85、Primary Shell 56 全數維持。正式 WPF 以集中可重用 Resource 建立 Panel、Deep／Inner／Safety／Main Workspace、Primary／Secondary Button、Chart、方向霜邊、低強度雪地冷反射與局部暖窗次光；Runtime 不需額外有限微調，Ledger 仍完整記錄可回滾值。
- 場景與工程裁決：production 使用 byte-identical B3 source 與五張局部 component-aware crop；沒有一張 2142×1196 全畫布前景 overlay。B3SC018 人物薄紗／少量髮絲／衣擺外緣，以及 B3SC014／015／016 梅枝，只取原圖同座標 RGB 並以 deterministic alpha 呈現。人物核心、來源 RGB、Critical UI 與可讀性均保持 0 差異／0 違規。
- Runtime 結果：100／125／150% 正式截圖為 2142×1196、2678×1495、3213×1794；14/14 HitTest 通過，L7–L9 藝術層全數 `IsHitTestVisible=false`。觀看順序為交易資訊、冰雪場景、人物外緣／梅枝；沒有均勻霓虹框或黑板化。
- 成本與 Gate：六張 production rasters 解碼總量 9.617 MiB，較先前兩張 full-canvas B2R.2 rasters 淨減 9.929 MiB，risk LOW。Focused 8/8、locked restore 6/6、Release 0 warnings／0 errors、完整測試 1711/1711 PASS。
- 範圍與狀態：17 張強制視覺產物、9 份機器 JSON、完整繁中 HTML/TXT 與一鍵複製報告位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_1`。額外模型與圖像生成呼叫均為 0；功能／交易／資料模型未修改；沒有動畫、Hover／Focus、Scene Dynamics、Micro Interaction、commit、push 或 deploy。狀態為 `PASS / AWAITING SUPERVISOR REVIEW`。
- 取代：Decision062 的 GOLD TARGET 由離線靜態候選提升為 B3.1 production WPF 實作參照；Decision062 的 Geometry Authority 與未知值政策繼續有效。任何後續動態或新階段仍需 Supervisor 另行授權。

## TCC-DEC-2026-09-18-064 — M1.4.6-B3.1R 人物深度穿插與完整梅枝候選

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B3.1R — 人物深度穿插 + 完整梅枝連續性修正` 的明確授權。
- 深度結構：Major Alerts、Today's Priorities 與 Activity 拆為 `Card Material z0 → same-source Artwork z1 → Critical Content z2`。藝術層可以遮蔽玻璃與穿越處 Border，但 Icon、標題、狀態、Chevron、Checkbox 與列表內容維持最前；ART_OVER_MATERIAL 為 10,385 px，ART_OVER_CRITICAL_CONTENT 為 0 px。
- 人物裁決：只從 byte-identical B3 權威圖擷取同座標頭／臉／主髮量及髮絲；沒有生成、補畫、Face Swap、位移、縮放、Warp 或人物內容變更，person pixel delta 0。Major Alerts 測整個頭／臉／髮穿插；Today's Priorities 只測髮絲穿插。
- 梅枝裁決：A 使用 B3SC016 原座標可辨識連續主枝、分枝與梅花；B 將 Activity 前景梅枝設為 0%。先前造成白色斷帶的手繪 alpha skeleton 已移除，改採來源暗枝／紅花像素與窄來源走廊；不重排花朵、不移動枝條。
- 候選治理：三組 production candidate layers 預設 `Opacity=0`，保留 B3.1 正式預設；隔離 Runtime harness 才啟用六個交付組合。Supervisor 尚未選 A 或 B，也可選 C 退回人物穿插或 D 有界修正。
- 工程 Gate：21/21 幾何 ΔX／ΔY／ΔW／ΔH 全 0；B3.1 material token delta 0；Alpha、Border Occlusion、梅枝連續性、可讀性、HitTest 兩模式各 14/14、DPI 100/125/150% 皆通過。三張局部 crop 增量 1.875 MiB，無新全畫布前景 Overlay，risk LOW。Release build 0 warnings／0 errors，完整測試 1715/1715 PASS。
- 範圍與狀態：19 張報告影像、13 份 JSON、完整繁中 HTML/TXT 與一鍵複製報告位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_1r`。model calls 與 image-generation calls 均為 0；沒有動畫、Hover／Focus／Pressed、responsive refactor、新功能、commit、push 或 deploy。狀態為 `PASS / AWAITING SUPERVISOR DECISION`。
- 取代：NONE。Decision063 保持正式 B3.1 預設權威，直到 Supervisor 明確選定 B3.1R 候選；Decision062 的 Geometry Authority 與未知值政策繼續有效。

## TCC-DEC-2026-09-18-065 — M1.4.6-B3.1D 深度權威與 B3.1 正式基線恢復

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B3.1D — 場景深度權威建置 + B3.1R 實驗回退` 的明確授權。
- Production 裁決：B3.1R 的 Major Alerts 人物核心硬穿插、Today's Priorities 大量髮絲硬穿插、Activity 候選切換與三個 Runtime 候選資源已精確撤回。正式 HOME 回復 Decision063 的 B3.1 Authority；逐像素比較 changed pixels 0、MAE 0、max channel delta 0。B3.1R 報告、圖像、遮罩、JSON、腳本與三層架構研究完整保留在隔離工作目錄。
- 深度架構：Scene Depth Authority 使用 D0–D6 描述空間位置，Depth Confidence 使用 HIGH／MEDIUM／LOW／UNKNOWN 描述證據品質，Occlusion Relationship 記錄遮擋證據。這三者不直接決定 Z-order；UI Fusion Policy 另以六個受控值決定能否硬前置、柔性融合、Glass Fade、留在背景、禁止或未知。
- 人物裁決：臉、五官、頭、主髮量、軀幹、手、武器與實體服裝一律禁止硬穿 UI。Character Distance Field 由實際 silhouette 的 L2 distance transform 產生；A 線性、B SmoothStep、C ease-out 只存在隔離預覽，並同步衰減 Glass、Border、Frost、Reflection 與 Inner Highlight。沒有 radial／circular gradient；B 僅是審查推薦，沒有正式選定。
- 梅枝裁決：B3SC014 與 B3SC016 只有在 HIGH confidence、來源連續且主枝／次枝／花朵／原始方向全部成立時，才能作完整前景穿插。碎花方案禁止；無法成立時採 0%。Critical Content 永遠保持最上層，隔離候選 overlap 為 0。
- Gate：21/21 Geometry ΔX／ΔY／ΔW／ΔH 全 0、人物 RGB delta 0、B3 source hash exact、HitTest 14/14、DPI 100/125/150% PASS；focused 5/5、locked restore 6/6、Release build 0 warnings／0 errors、full regression 1716/1716 PASS。model calls 與 image-generation calls 均為 0。
- 範圍與狀態：17 張必交影像、11 份必交 JSON、三張 3× 局部證據與一鍵複製 HTML/TXT 報告位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_1d`。沒有候選寫入 Production，沒有動畫、commit、push 或 deploy。狀態為 `PASS / AWAITING SUPERVISOR REVIEW`。
- 取代：Decision064 不再提供正式 Runtime 視覺候選；其研究證據與 `Material → Artwork → Critical Content` 工程能力繼續保留。Decision063 再次成為目前正式 HOME Production Authority；Decision062 的 Geometry Authority 與未知值政策繼續有效。

## TCC-DEC-2026-09-18-066 — M1.4.6-B3.1D.1 可見表面人物語意與局部深度拓撲

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B3.1D.1 — 人物語意分層 × 局部深度拓撲權威建置` 的明確授權，以及對 B3.1D 的 ACCEPTED 裁決。
- 能力邊界：本 Authority 是 `2D Visible-Surface Semantic Decomposition + Local Depth Topology`，不是 3D reconstruction、character mesh 或 full depth recovery。只處理 B3 權威圖中真正可見且已包含於 B3.1D CHARACTER_VISIBLE_UNION 的像素；被遮蔽的髮、身體、披風、武器與背面內容一律不補猜。
- 語意裁決：Primary Labels 為 CH01 臉／頭核心、CH02 主髮量、CH03 游離髮絲、CH04 毛領、CH05 實體衣服、CH06 半透明薄紗、CH07 柔性衣擺、CH08 手、CH09 武器、CH10 飾品與 CH99 未知。439,602 px union 的 created outside、lost 與 multi-primary 均為 0。CH99 為 106,310 px／24.183%；這是對既有 union 內建築／前景交纏與低證據區的保守表達，不設降低 KPI。
- 證據分離：Semantic、Semantic Confidence、Depth／Depth Confidence 與 Fusion Policy 分別儲存。白色不自動等於薄紗、深色不自動等於頭髮、膚色不單獨決定臉；每個分類同時使用形狀、連續性、紋理、人體結構、透明證據、邊界與 source continuity。
- 局部拓撲：建立 10 筆 bounded relationships，涵蓋髮際線、臉側游離髮、髮／毛領、毛領／衣身、薄紗／毛領與衣身、手／握柄、武器／衣身、衣擺／衣身及髮飾／主髮。禁止全人物固定排序；每個 local graph 均為 DAG，cycle 0。
- 融合候選：CH01／02／04／05／10 為 GLASS_FADE_ONLY；CH03／06／07 為 SOFT_FOREGROUND_ALLOWED；CH08／09 為 PROTECTED；CH99 為 FORBIDDEN。因 CH06 目前僅 MEDIUM semantic/depth confidence，本階段 HARD_FOREGROUND_CANDIDATE 為 0。SmoothStep 仍是未來 Glass Fade 首選候選，但沒有寫入 Production。
- 距離與邊界：八個 silhouette-derived L2 channels 分別對 core、main hair、free hair、fur、solid garment、veil、hands、weapon 建立；不使用 radial gradient。Boundary QA 的 white／black halo、hard cut、rectangular crop artifact 與 source-edge drift 均為 0；低信心邊緣保留 CH99。
- Production 與 Gate：MainWindow.xaml、MainWindow.xaml.cs、DesktopHost project 與 B3 production assets 的 stage-entry SHA-256 全數不變；B3.1 Runtime changed pixels 0，梅枝 Authority 未修改。Focused 6/6、locked restore 6/6、Release x64 0 warnings／0 errors、full regression 1722/1722 PASS；model calls 與 image-generation calls 均為 0。
- 範圍與狀態：11 張 masks、11 張視覺交付、9 份 JSON、semantic-distance NPZ 與一鍵複製 HTML/TXT 報告位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_1d1`。沒有 Production 視覺施工、SmoothStep 套用、人物／梅枝強化、commit、push 或 deploy；沒有進入 B3.1S。狀態為 `PASS / AWAITING SUPERVISOR REVIEW`。
- 取代：NONE。Decision065 的 B3.1D Scene Depth／Confidence／Distance／Fusion 分離仍為上位深度權威；Decision063 仍是正式 HOME Production Authority。

## TCC-DEC-2026-09-18-067 — M1.4.6-B3.1D.2 人物 Union 純化與可合成性權威

- 日期：2026-09-18；來源：Supervisor 對 `M1.4.6-B3.1D.2 — 人物 Union 純化 × 可合成性權威建置` 的明確授權。
- Union 裁決：舊 `LEGACY_CHARACTER_CANDIDATE_UNION` 439,602 px 守恆分割為 Confirmed 305,668 px、Uncertain 15,083 px 與 External Scene 118,851 px；created、lost、multi-label 與所有 pairwise intersection 均為 0。新 `CHARACTER_AUTHORITY` 只包含 Confirmed。
- 純度裁決：CH01–CH10 只保留 Confirmed membership，CH99 只保留 Uncertain。明顯建築、梅枝、積雪與岩石由人物 Authority 外部化；B3SC014／B3SC016 梅枝 Authority 不刪除、不改寫。
- 可合成性裁決：Semantic、membership、depth、confidence、compositability 與 fusion policy 分離。CH03 依連通區分為 Edge Alpha、Glass Fade 或禁止；目前沒有 OPAQUE_CUTOUT_OK 或可直接重組區。手與武器維持 PROTECTED。
- Matting 裁決：CH06 是 binary semantic mask，不是真實 alpha；來源 RGB 已與舊背景合成。前景色與 alpha 從單張 RGB 不可唯一恢復，因此全部列為 `MATTING_UNDERDETERMINED`，只能維持 Glass Fade policy，禁止聲稱 true alpha 已恢復。
- Distance 裁決：乾淨 Distance Field 由 Confirmed Character 與 CH01–CH10 重建，Uncertain 獨立通道，External Scene fade weight 為 0；共 10 個 L2 silhouette channels，不使用 radial gradient。
- Production 與 Gate：Production stage-entry hashes 全數一致，B3.1 Runtime changed pixels 0，Geometry 21/21、Material Tokens 與 Assets 不變。Focused 8/8、locked restore 6/6、Release x64 0 warnings／0 errors、full regression 1730/1730 PASS；model／paid-model／image-generation calls 均為 0。
- 範圍與狀態：18 張必交視覺、11 張乾淨 masks、12 份 JSON、distance NPZ 與一鍵複製 HTML/TXT 報告位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_1d2`。未施工融合、未修改 Production HOME、未進入 B3.1S、未 commit／push／deploy；狀態為 `PASS / AWAITING SUPERVISOR REVIEW`。
- 取代：Decision066 的 `LEGACY_CHARACTER_CANDIDATE_UNION` 不再是正式人物 membership Authority；其可見表面語意與局部深度研究仍保留。Decision065 的深度／信心／Fusion 分離與 Decision063 的正式 HOME Production Authority 繼續有效。

## TCC-DEC-2026-09-19-068 — M1.4.6-B3.1F 前景交錯可行性實證

- 日期：2026-09-19；來源：Supervisor 對 `M1.4.6-B3.1F — 前景交錯可行性實證` 的明確授權，以及對 B3.1D／D.2 的 ACCEPTED 與 D.1 的 CONDITIONAL ACCEPT 裁決。
- Runtime 裁決：所有能力結論來自 compiled production MainWindow 的隔離 WPF Visual Tree。實驗使用 WPF Layer、Image、Brush、OpacityMask、Border、Z-order 與 IsHitTestVisible；Python 只準備同源測試資產及製作報告板，不充當 Runtime 證據。
- 梅枝裁決：B3SC014 左下與 B3SC016 右下均在原始座標保留可辨識的主枝、次枝、細枝、梅花與方向，採 `card material z0 → source plum z1 → original critical content z2`。Market／Activity Border 在交會處確實被枝系遮蔽，Critical Content 遮擋 0，全頁與 3× 審查通過，因此 `PLUM_HARD_INTERLEAVE=PASS`。
- 髮絲裁決：CH03_R003 與 CH03_R007 在原始座標都不接觸任何授權測試卡片，分別判 `NOT_APPLICABLE`。未平移、縮放、旋轉、Warp、補畫、猜 alpha 或攜入舊背景；因沒有成功的可適用 Region，`HAIR_SOFT_INTERLEAVE=FAIL`，Combo 2 禁止建立。
- 薄紗裁決：沿用 D.2 的 source limitation；CH06 沒有 independent alpha，RGB 已與舊背景合成，matting 不可唯一求解。本階段不重測硬交錯，`VEIL_INTERLEAVE=NOT_AUTHORIZED`。
- 透明退讓裁決：只使用 Confirmed Character 與 CH01／02／04／05 語意距離；External Scene 與 Uncertain Character 權重 0，CH08／09 保護。SmoothStep 同步衰減 Glass、Border、Frost、Reflection 與 Inner Highlight；人物與 Critical Content 物件不變，全頁／3× 無 Halo、人工邊界或挖洞感，因此 `CHARACTER_PROXIMITY_FADE=PASS`。
- 後續策略：能力矩陣證明可用策略為「完整梅枝強穿插 + 人物鄰近透明退讓」。人物核心硬切穿插已由既有證據否決；髮絲未證明；薄紗未授權。Combo 1 PASS，Combo 2 不存在。這只是後續 B3.1S 的條件基礎，不構成 B3.1S 授權。
- Gate 與範圍：Geometry 21/21、HitTest 14/14；Focused 8/8、locked restore 6/6、Release x64 0 warnings／0 errors、full regression 1738/1738 PASS。19 張視覺、9 份必要 JSON、Actual WPF Runtime、支援 manifest 與一鍵複製 HTML/TXT 位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_1f`。model／paid-model／image-generation calls 均為 0；正式 HOME、Material Token、Production Assets 未修改；未進入 B3.1S、動畫或下一階段；未 commit／push／deploy。
- 狀態：`M1.4.6-B3.1F PASS / AWAITING SUPERVISOR REVIEW`。
- 取代：NONE。Decision067 的純化 Character Authority 與 matting 限制、Decision065 的深度／Fusion 分離、Decision063 的正式 B3.1 Production Authority 繼續有效。

## TCC-DEC-2026-09-19-069 — 禁止拼貼式人物右移與 M1.4.6-B3.1G-R 一體化候選

- 日期：2026-09-19；來源：Supervisor 對 B3.1G 的人工中止、人物右移策略修正，以及對 `M1.4.6-B3.1G-R — 一體化人物右移重建修正版` 的明確授權。
- 策略裁決：`背景底板 + 平移人物 PNG + UI` 即使 Alpha 與邊界乾淨，仍會因光照、空氣透視、雪霧、筆觸、景深與共同成像關係不一致形成拼貼感。後續禁止把純人物直接作最終拼貼層；純人物只保留 `CHARACTER VISUAL AUTHORITY` 身份。中止的 B3.1G 圖只可作 `NEGATIVE EXAMPLE`。
- 重建裁決：人物右移必須使用原始 B3 場景與純人物身份 Authority 進行一體化影像編修，並同時處理原位置、新位置與必要融合區。生成式結果不要求 Pixel Delta 0，改以人物身份、畫風、場景融合、背景修補與 UI 相容性五項 Gate 判定；任一 FAIL 即不得升格，也不得用原人物 PNG 覆蓋補救。
- 候選結果：建立 A／B／C 三張 1678×937 一體化場景。A 約向右 38 source px，五項 Gate 全 PASS；B 只有約 6 px 右移且身份／UI Gate FAIL；C 約 61 px 右移但身份與尺度漂移，身份 Gate FAIL。A 是唯一 `GENERATED DERIVED VISUAL AUTHORITY CANDIDATE`，只供 Supervisor Review。
- UI 預覽：純場景 A 通過後，才以 compiled production MainWindow 的隔離 Visual Tree 建立 A／B／C 預覽。Geometry 21/21、HitTest 14/14；人物距離按候選重新計算。梅枝標為 `GENERATED_DERIVED_SCENE_PLUM_WITH_B3SC_LOCATION_PRIOR`，未冒充舊 B3SC source-pixel Authority。策略仍限人物鄰近 SmoothStep 退讓 + 完整梅枝交錯，沒有硬穿人臉、主髮或薄紗。
- Production 與成本：正式 B3.1 HOME、MainWindow、Material Token 與 Production Assets hashes 不變。本階段圖像生成／編修呼叫 3，實際模型識別無法由平台驗證；沒有進入正式融合、動畫、commit、push 或 deploy。
- 產物與狀態：純場景、3× 身份／材質／融合證據、反例比較、Actual WPF 預覽、區域比較、Final Review Board、機器 JSON 與一鍵複製 HTML/TXT 位於 `automation/tcc_master_pipeline/work/m1_4_6_b3_1gr`。狀態為 `PASS / NEW_VISUAL_AUTHORITY_CANDIDATE A / AWAITING SUPERVISOR REVIEW`。
- 下一步邊界：A 尚未成為 Production Authority。只有 Supervisor 明確接受後，才能另行授權「以 A 建立正式新權威，再進行 UI 融合施工」。
- 取代：先前任何「純人物剛性平移後貼回背景」策略。Decision068 的完整梅枝交錯與人物鄰近透明退讓能力、Decision063 的 B3.1 Production Authority 繼續有效。

## TCC-DEC-2026-09-19-070 — B3.1G-R A 正式接受為新的生成式衍生主視覺權威

- 日期：2026-09-19；來源：Supervisor 明確決策 `M1.4.6-B3.1G-R: ACCEPTED WITH CANDIDATE A`。
- Authority 裁決：A 正式升格為 `NEW GENERATED DERIVED VISUAL AUTHORITY`。A 仍不是原始 B3 真實像素，也尚未替換 Production；正式 HOME 繼續維持 B3.1。
- 接受理由：A 無明顯 PNG 拼貼感；人物與月光、雪霧、空氣、景深及筆觸共同成像；人眼審查認定人物身份、神韻、表情與主要特徵一致；右移降低 Major Alerts、Today's Priorities 與 Activity 的人物核心衝突；產品品質高於中止的拼貼方案。
- 人物一致性：生成式一體化主視覺不再要求 Character／Face Pixel Delta 0。最高標準改為五官結構、臉型、表情、眼神、年齡感、神韻、髮型、服裝、武器、姿態、身體比例語言與畫風在人眼審查下仍屬同一人物。可接受有助融合的小幅尺寸、邊界、光影、雪霧與細節重新成像；禁止為追求像素一致退回拼貼。
- 變化分帳：A 的 X／Y registration delta 約 +37.848／+15.768 source px，畫寬 X 約 +2.2567%；尺寸比例約 0.9461，等同縮小約 5.4%，已由 Supervisor 人工接受。姿態沒有構成身份改變的實質差異；臉部 Pixel Delta 非 0，但視覺身份一致。後續必須持續分別記錄位置、尺寸、姿態與臉部身份差異。
- 舊 Authority 降級：B3SC014／015／016／018、舊 Character Union、Distance Field、Semantic Masks、人物座標與梅枝 Alpha Mask 對 A 只能作 `LOCATION / SEMANTIC / DESIGN PRIOR`，不得宣稱像素級延續。
- H 要求：正式融合前，M1.4.6-B3.1H 必須從 A 重建人物可見範圍、Confirmed／Uncertain／External Scene、人物語意、人物距離場、場景深度、深度信心、UI Fusion Policy、左右真前景梅枝、梅枝來源連續性與 Critical Content 保護區。禁止直接平移舊 B3 Mask；新場景物件應建立 `A_SCxxx` ID。
- 融合策略：Decision068 已證明的「人物鄰近透明退讓 + 完整梅枝交錯」繼續有效；髮絲仍為 FAIL／未證明，薄紗硬交錯仍 NOT AUTHORIZED，手／武器保護，Critical Content 永遠在最前層。
- 階段邊界：本決策沒有授權替換 Production，也沒有授權 B3.1H 或 B3.1S。必須另行授權 H；H 通過 Supervisor Review 後，才能另行授權 S。沒有 commit、push 或 deploy。
- 取代：Decision069 中 A 仍為「等待 Supervisor Review 的候選」狀態。Decision069 的禁止拼貼策略、Decision068 的融合能力與 Decision063 的現行 Production Authority 繼續有效。

## TCC-DEC-2026-09-19-071 — B3.1H 完整梅枝前景與禁止碎片裁切

- 日期：2026-09-19；來源：Supervisor 的 `完整梅枝前景原則 — 禁止卡片式碎片裁切` 與 B3.1H 追加 Requirement。
- 定位裁決：梅枝是 A 場景中原本存在的完整真前景物件，不是 Activity 或其他卡片的裝飾。右下完整枝系是主要前景，左側／左下是次要前景；不得對稱、鏡像、擴增或追求覆蓋率。
- Authority 裁決：B3.1H 必須從 A 重新建立單一完整右下梅枝 Authority Object 與新 `A_SCxxx` ID。B3SC016 只能作位置／語意先驗。Authority Bounds 必須由枝系可見範圍決定，禁止依 Activity、Open Positions 或其他 Card Bounds 裁成多個 fragments。
- 結構裁決：Authority 必須保留 A 中可確認的主枝、主要次枝、結構性細枝、花簇、生長方向、可見起終點、原有遮擋與人物前後關係。原本被人物、雪岩、建築遮住的部分只記錄 Occlusion，不得補猜；A 中連續的可見主枝不得在 UI 邊界新增中斷。
- 像素裁決：所有可見枝條 RGB 必須來自 A，選定 Source Pixel RGB drift 0。禁止重上色、生成新花、重畫／延長枝條或使用舊 B3 像素冒充 A。Mask 必須清楚顯示主枝；只剩花點或短枝即 `FAIL_PLUM_STRUCTURE_FRAGMENTED`。
- Runtime 裁決：隔離 WPF 中採 `Scene → Glass／Border／Frost／Reflection／Inner Highlight → Full Plum Foreground → Critical Content`。整枝為單一邏輯物件，可遮材質，Critical Content overlap 必須為 0；不得重寫人物／梅枝既有前後關係。
- Visual Gate：白邊、黑邊、Halo、矩形 Crop Edge、背景／人物污染、per-card crops、碎花貼片、Card Bounds 裁斷、虛構枝花或 Critical Content 遮擋任一成立即 Visual FAIL；工程 Gate 不得覆蓋此裁決。
- 必交證據：完整 Authority、Mask、Structure Graph、Occlusion Map、UI Path、Actual WPF Full Preview、Activity 3×、Cross-card 3×、Source QA 與同時呈現八類證據的 `A_HOME_FINAL_REVIEW_BOARD.png`。
- 階段邊界：本決策只固化 B3.1H 施工要求，不構成 B3.1H 執行授權。A 已接受為新生成式衍生主視覺權威，Production 仍維持 B3.1；H／S 均未開始，未 commit／push／deploy。
- 取代：所有較寬鬆、允許按卡片裁切或以碎片梅花代表完整枝系的舊描述。Decision070 的 A Authority 與 Decision068 的融合能力繼續有效。


## TCC-DEC-2026-09-19-072 — M1.4.6-B3.1H A 專屬場景融合 Authority 候選

- 日期：2026-09-19；來源：Supervisor 對 `M1.4.6-B3.1H — A 專屬場景融合 Authority 重建` 的正式授權。
- A 人物 Authority：從 Candidate A 重建 215,698 px Confirmed Character 與 CH01–CH10／CH99 互斥語意；重建 A 專屬 Distance Field。舊 B3 Character Union、Distance Field、Semantic Masks 與座標只作先驗，未作 A 像素 Authority。
- Depth／Fusion 裁決：A Scene Depth、Confidence 與 Fusion Policy 分離。人物以 SmoothStep 讓 Glass／Border／Frost／Reflection／Inner Highlight 最多退讓 30%；人物與 Critical Content 不變，沒有 radial gradient、Halo 或挖洞。臉、主髮、薄紗維持禁止硬穿插。
- 梅枝 Authority：右下 `A_SC031_RIGHT_PLUM_COMPLETE_VISIBLE_SYSTEM` 為 15,240 px 的單一完整可見枝系物件；來源遮擋保持透明，31 px 間隙不是 Card Boundary 裁切。主枝、次枝、結構細枝與花簇共同管理；Card-boundary gaps 0、per-card crops 0、generated branches／flowers 0／0、Source RGB drift 0。左側 `A_SC032_LEFT_PLUM_SECONDARY_VISIBLE_SYSTEM` 維持次要前景。
- Runtime 裁決：compiled production MainWindow 隔離驗證採 `Scene → Material → Full Plum → Critical Content`；Geometry 21/21、ΔXYWH 0、HitTest 14/14、DPI 100/125/150、ART_OVER_CRITICAL_CONTENT 0。Runtime 沒有 2142×1196 全畫布透明 Overlay。
- Gate 與範圍：20/20 必交圖與 Final Review Board 完成；Locked restore 6/6、Release x64 0 warnings／0 errors、Focused 8/8、full regression 1738/1738 PASS。Production HOME／Material Token／Assets custody hashes 不變；model／image calls 0；未 commit／push／deploy。
- 狀態與邊界：`PASS / AWAITING SUPERVISOR REVIEW`。本決策記錄的是 H 候選 Authority 與自驗證結果，不授權替換 Production，也不授權進入 B3.1S；須由 Supervisor 審查後另行決策。
- 取代：Decision071 的 `H NOT STARTED` 執行狀態；其完整梅枝原則本身繼續有效。Decision070 的 A 主視覺權威、Decision068 的可用融合能力與 Decision063 的現行 Production Authority 繼續有效。


## TCC-DEC-2026-09-19-073 — B3.1H 梅枝／梅花裁剪潔淨化

- 日期：2026-09-19；來源：Supervisor 的 `M1.4.6-B3.1H 追加 Supervisor Requirement — 梅枝／梅花裁剪潔淨化`。
- 雙 Gate 裁決：梅枝前景必須同時具備完整枝系與乾淨語意。完整性不得掩蓋背景、人物、建築、雪岩、UI 殘影、孤立黑線／紅點、白黑邊、Halo 或矩形裁切痕；乾淨化也不得補畫或改寫 A 的可見遮擋。
- 右下結果：清理前 13,853 px，正式 Confirmed 4,937 px，Uncertain 2,493 px，Rejected Noise 6,423 px。Uncertain／Rejected 不進正式前景；孤立碎片 0、最大孤立面積 0、fragment-only 0；人物／背景雪岩／建築／UI 污染均 0，白邊／黑邊／Halo／矩形痕均 0，Source RGB drift 0。
- 結構裁決：主枝、主要次枝與 34 個可證明附著花簇保留；30 個無法充分證明的花簇候選剔除，1 個孤立 component 已移除。清理後仍是單一 `A_SC031_RIGHT_PLUM_COMPLETE_VISIBLE_SYSTEM` Authority Object，per-card crops 0。
- 左側裁決：來源無法同時證明乾淨與完整，因此正式 Confirmed／Runtime foreground 降為 0；3,744 px 標為 Uncertain、6,474 px 剔除。這是依「寧可 0%，不得製造散花」原則，不構成能力退化。
- 證據與 Runtime：新增 10 張 cleanup／noise／component／edge／contamination 圖，Final Review Board 擴充為 18 格；必交圖 30/30。compiled WPF 仍為 Geometry 21/21、HitTest 14/14、DPI 100/125/150、Critical Content overlap 0。
- Gate 與邊界：Production custody hashes 不變；本次只修改離線 Authority、報告與狀態文件，model／image calls 0，未 commit／push／deploy，未進入 B3.1S。狀態 `PASS / AWAITING SUPERVISOR REVIEW`。
- 取代：Decision072 中右下 15,240 px 初版裁切與左側啟用策略；Decision071 的完整梅枝原則繼續有效，但判斷順序更新為自然 → 乾淨 → 完整 → 效果強度。

## TCC-DEC-2026-09-19-074 — M1.4.6-B3.1H.1 右下梅枝合法結構回補候選

- 日期：2026-09-19；來源：Supervisor 對 `M1.4.6-B3.1H.1 — 右下梅枝邊界精修 × 合法結構回補` 的正式授權。
- 問題裁決：Decision073 的潔淨化結果沒有重新帶回污染，但 4,937 px Confirmed 在實際 WPF 中出現細碎、稀疏與過度瘦化風險。H.1 不以像素數作目標，而是直接回到 Candidate A，依 Source、既有枝系方向與 Connected Component 證據回補合法暗枝、花梗／花萼與附著花簇。
- 像素結果：H core 4,937 px 全數鎖定；H.1 Confirmed 8,183 px，新增 3,246 px，其中 Uncertain 升級 282 px、Rejected 有限回查恢復 2,964 px；剩餘 Uncertain／Rejected 為 2,211／3,459 px。22 個可見回補群組均保留逐項原因與來源證據。
- 可視 Gate：主枝與主要次枝 `HUMAN_VISIBLE_CONTINUITY_GATE` 均 PASS；人工 Mask 造成的最大可視斷裂為 0，沒有人工橋接。54 個來源附著花簇 Confirmed、6 個無充分依據的候選拒絕；合法花梗／花萼保留，無禿花或 fragment-only 正式區。
- 品質與 Runtime：Source RGB max drift 0、changed pixels 0、generated pixels 0；人物／背景／建築／雪岩／UI 污染、矩形痕、白黑邊與 Halo 均 0。compiled WPF Geometry 21/21、HitTest 14/14、DPI 100/125/150、ART_OVER_CRITICAL_CONTENT 0、per-card crops 0。
- 範圍：人物、CH01–CH10／CH99、Character Distance Field、Fade、左側梅枝、UI Geometry、Material Token 與 Production 均未修改。model／image-generation calls 0；未 commit／push／deploy，未進入 B3.1S。
- 狀態：`PASS / AWAITING SUPERVISOR REVIEW`。本決策只記錄 H.1 候選與自驗證結果；是否接受 H.1、是否授權 B3.1S 仍需 Supervisor 後續明確裁決。
- 取代：Decision073 的右下 4,937 px 正式候選值；Decision073 的污染 Gate、左側 0 foreground、Decision071 的完整枝系原則與自然 → 乾淨 → 完整 → 效果強度排序繼續有效。

## TCC-DEC-2026-09-19-075 — M1.4.6-B3.1H.1 完整真前景與三候選驗證

- 日期：2026-09-19；來源：Supervisor 的 `M1.4.6-B3.1H.1 — 真前景物件純化 × 完整前置驗證` 正式授權與同名 Supervisor Context。
- 右下梅枝：沿用 Decision074 已驗證的 8,183 px Confirmed，保留 2,211 px Uncertain、3,459 px Rejected；新增 `LEGAL_FOREGROUND_OMISSION_GATE`，所有來源追蹤合法枝條與附著花簇皆已進正式前景，遺漏 0，主枝 3×／6×與主要次枝連續性 PASS。
- 左下完整近景：Decision074 的「左側 0 formal foreground」只適用於單獨梅枝。完整 H.1 改以 A 中 frame-connected 雪岩＋梅枝原生遮擋群建立 `A_SC041_LEFT_LOWER_SNOW_ROCK_PLUM_COMPLETE_VISIBLE_GROUP`；Confirmed／Uncertain／Rejected 為 84,354／11,550／112,107 px，單一物件、Source RGB drift 0、背景／人物／UI 污染 0。
- 人物前側衣物：只建立 CH07 Soft Hem 不透明來源子區候選，不使用 CH06 薄紗、不宣稱 true alpha、不改臉或人物身份。100% 全頁沒有明顯貼圖，3× 邊緣仍須 Supervisor 人工覆核，未升格為正式人物穿插能力。
- 左導覽：Candidate 3 只降低 Navigation material opacity（0.773–0.812），Selected State、文字與 Icon 內容維持 1.0；沒有 radial hole 或局部挖洞。
- 三候選：Candidate 1 為完整環境前景基線；Candidate 2 加人物衣物候選；Candidate 3 再加左導覽材質退讓。自驗證只建議 Supervisor 先看 Candidate 1，不自動選案。
- Gate：38/38 圖、14/14 JSON、Geometry 21/21、HitTest 14/14、DPI 100/125/150、Focused 30/30、Full 1738/1738、Release build 0 warnings／0 errors、Source RGB drift 0、Critical overlap 0；Production custody 4/4 unchanged。
- 狀態與邊界：`PASS / AWAITING SUPERVISOR REVIEW`。未修改 Production HOME／Material Token／凍結 Geometry，model／image calls 0，未 commit／push／deploy，未進入 B3.1S。
- 取代：Decision074 的 H.1 僅右下窄版任務範圍與左側 0 formal foreground 結論；Decision074 的右下 8,183 px Authority 與污染／連續性結果保留。Decision071 的完整物件原則、Decision073 的潔淨 Gate 及 Decision070 的 A Authority 繼續有效。

## TCC-DEC-2026-09-21-076 — Style Pack 正式接受與 Authority 邊界

- 日期：2026-09-21；來源：Supervisor 對 `STYLE_PACK_ACCEPT` 的正式授權。
- 接受結果：`STYLE_PACK_STATUS=ACCEPTED`。既有分析結果維持 6 STYLE_RULE、2 LOCAL_REFERENCE、3 NOT_PROVEN、27 APPROVED_VISUAL_ASSET、2 RESTRICTED_UI_REFERENCE、0 UNCLASSIFIED；Geometry／Function Authority promotions 皆為 0，HPA-001／HPA-002 guards PASS，conflicts 0。
- Authority 順序：Accepted Design System V1 是產品視覺系統最高規則 Authority；Style Pack 只具 Taste／Visual Language／Approved Asset Authority，不是 Geometry Authority，也不是 Function Authority。
- Duplicate Evidence：跨 Pack、路徑或 Registry 的相同 SHA256 只算一份 unique evidence，不得增加 evidence_count、confidence、repeated evidence 或 cross-Pack support。已知同 SHA256 影片 `A36A5ECCB2E99CCD02DB6AE11005CCB7CE470DBF355D9E1D5D9320CEAF099BC1` 依此去重；`DUPLICATE_EVIDENCE_DEDUP=PASS`。
- PACK A UI 限制：PACK A 的 whole-Pack Style Authority 不得提升 Dashboard、Command Center、Navigation、Panel、Card Layout 或 Product UI。這些內容只能作 LOCAL／RESTRICTED VISUAL REFERENCE，僅供氛圍、材質、光影、Scene/UI Fusion 與視覺語言；禁止 Geometry、Layout、Panel Count、Navigation、Function、Workflow 與 Information Architecture；`PACK_A_UI_RESTRICTION_GUARD=PASS`。
- 範圍與成本：本階段 model／Vision／image API calls 均為 0；未重新分析素材，未修改 Case A 或 Accepted Design System，未讀取或修改 Production，未進入 Case B，未 commit／push／deploy。
- 下一步：`CASE_B_WITH_ACCEPTED_STYLE_PACK`，需另行明確授權後才能開始。

## TCC-DEC-2026-09-21-077 — Case B 舊方向否決與 Accepted Style Pack 全面重建

- 日期：2026-09-21；來源：Supervisor 對 `CASE_B_REBUILD_FROM_ACCEPTED_STYLE_PACK` 的正式授權，以及對既有兩張 Case B 視覺方向的人工否決。
- 舊圖裁決：`CASE_B_FOCUS_WORKSPACE.png` 與 `CASE_B_FOCUS_WORKSPACE_REFINED.png` 均為 `REJECTED_BY_USER`，只保留為 rejected／historical validation evidence；不得再 refinement、image edit、trace、延續巨大冰月牙／雪花 centerpiece 或 loading-screen composition。
- 重建裁決：Case B 身份仍為 `SYNTHETIC_VALIDATION_ARCHETYPE`、`FORMAL_PRODUCT_MODULE=NO`；本次以全新 Focus Workspace composition 驗證低資訊密度、單一強主焦點、大量負空間、成熟產品 UI identity、Scene／UI Fusion 與相對 HOME／Case A 的 Geometry independence。
- Reference 治理：實際查看並使用 Approved `ASSET-005`、`ASSET-014`、`ASSET-027` 作 scene／material／taste reference；`RUI-001` 只供 visual language、material、lighting、environmental mood 與 Scene／UI relationship，Geometry／Layout／Panel Count／Navigation／Function／Workflow／Information Architecture authority 均維持 NO。
- 生成結果：單一 from-scratch image-generation call 產生 `CASE_B_REBUILD_V2.png`，有效 PNG 1672×941。指定契約為 `gpt-image-2.5-sunburst`／`xhigh`；平台未回報 runtime model／quality metadata，因此 Gate 明確區分 requested contract 與 runtime provenance。
- Gate：`CASE_B_REBUILD_V2_GATE.json` 為 PASS；IMAGE_API_CALLS=1，舊兩張 Case B edit base、HOME／Case A Geometry base、正式功能發明與 Product Constraint 均為 NO／0，HPA-001／HPA-002 guards PASS，FAILED_CHECKS=NONE，Production 未讀取／未修改。
- 邊界：Generation Gate 不構成視覺品質驗收；`VISUAL_QUALITY_AUDITED=NO`，Case B 未 Accept，未執行 visual audit、WPF、Production、Design System、Style Pack 或 Case A 修改，未 commit／push／deploy。
- 下一步：`CASE_B_REBUILD_V2_VISUAL_AUDIT`，必須另行執行，不得由本 Gate 自動宣告 Visual Quality PASS。
- 取代：Decision076 的下一步 `CASE_B_WITH_ACCEPTED_STYLE_PACK` 已由本次授權執行完成；Decision076 的 Style Pack Authority 與限制繼續有效。

## TCC-DEC-2026-09-21-078 — Case B Product Context Authority 與內容計畫

- 日期：2026-09-21；來源：Supervisor 對 Case B Image Generation 的停止指示，以及新增 `TCC Product Constitution v1.0 — APPROVED` 作 Product Function／Information Authority 的明確授權。
- Authority 分權：Product Constitution 只決定產品功能與可呈現資訊；Accepted Design System 決定視覺行為；Accepted Style Pack 決定 Taste／Material／Atmosphere；Case B Brief 決定本次低資訊密度 Focus Workspace 驗證目標。四者均不互相取得未授權的 Geometry、Style、Product 或 Composition authority。
- Content Plan：Case B 選定 `TRADING_PLAN_PREPARATION`。Primary Workspace 呈現一個 current Trading Plan；固定 Safety Core 保留 Trading Permission、Total Risk、Current Positions、Major Alerts；Supporting Information 選用 Strategy Template、Risk Profile、Evidence／Team Approval。
- Density 邊界：以單一計畫、單一 active region、固定 Safety Core compact summary 與三個 supporting summaries 保留產品語義；不展示 position table、calendar、analytics charts、history feeds、module overview 或完整 dashboard matrix。
- V1 Safety：禁止 broker／exchange control、自動下單、自動平倉與 API trading。Manual Trade Execution Tracking 保持手動執行後的追蹤語意。
- 既有 V2：本次停止指示到達前已產生的 `CASE_B_REBUILD_V2.png`、Prompt、pipeline 與 generation Gate 保留為 pre-product-context historical artifacts；不得進 Visual Audit、不得 Accept、不得作 edit 或 Geometry base。未授權刪除或覆寫。
- 本階段：只建立 `CASE_B_PRODUCT_CONTENT_PLAN.md` 並更新狀態／決策；MODEL_CALLS=0、IMAGE_API_CALLS=0，未執行 Image Generation、Visual Audit、WPF、Production、Design System、Style Pack 或 Case A 修改，未 commit／push／deploy。
- 下一步：`CASE_B_REBUILD_V2_AFTER_PRODUCT_CONTEXT`，必須另行授權。
- 取代：Decision077 的下一步 `CASE_B_REBUILD_V2_VISUAL_AUDIT`；Decision077 的舊 Case B rejected 狀態、Style Pack 使用邊界與 historical generation facts 保留。

## TCC-DEC-2026-09-21-079 — Case B Product Context V2 從零生成候選

- 日期：2026-09-21；來源：Supervisor 對 `CASE_B_REBUILD_V2_AFTER_PRODUCT_CONTEXT` 的正式授權。
- Authority 分權：沿用 Decision078。Product Content Plan 只決定資訊；Accepted Design System 只決定相對視覺結構；Accepted Style Pack 只決定 Taste／Material／Atmosphere；Case B Brief 只決定低資訊密度 Focus Workspace 驗證目標。四者都不取得固定 Geometry Authority。
- 新候選：以一次 from-scratch Image Generation Call 產生 `CASE_B_PRODUCT_CONTEXT_V2.png`（1672×941 PNG）。Primary Workspace 明確呈現 `TRADING PLAN PREPARATION`、選取中的 `RISK & EVIDENCE` 與 `CONTINUE PREPARATION`；Safety Core 4/4 與 Supporting Information 3/3 均進 prompt 並出現在圖片中。
- Reference 治理：Approved `ASSET-005`、`ASSET-014`、`ASSET-027` 只作 scene／material／taste reference；Restricted UI images 0。舊 Case B、pre-context V2、HOME 與 Case A 均未作 edit 或 Geometry base；新 composition 獨立。
- 產品安全：沒有 broker／exchange execution、自動交易、自動下單、自動平倉或 API execution；沒有價格、商品代號、帳戶資料、交易 ticket 或偽造市場數據。
- Gate：`CASE_B_PRODUCT_CONTEXT_V2_GATE.json` 為 PASS；HPA-001／HPA-002 PASS，IMAGE_API_CALLS=1，FAILED_CHECKS=NONE，Production 未讀取／未修改。指定契約為 `gpt-image-2.5-sunburst`／`xhigh`；平台仍未回報 runtime model／quality metadata，因此 requested contract 與 runtime provenance 保持分離。
- 邊界：Generation Gate 不構成 Visual Acceptance；`VISUAL_QUALITY_AUDITED=NO`。沒有執行 Visual Audit、WPF、Production、Case A、Design System 或 Style Pack 修改，沒有 commit／push／deploy。
- 下一步：`CASE_B_PRODUCT_CONTEXT_V2_VISUAL_AUDIT`，必須在新對話中獨立授權與執行；不得由本 Gate 自動接受 Case B。
- 取代：Decision078 的下一步 `CASE_B_REBUILD_V2_AFTER_PRODUCT_CONTEXT` 已完成；Decision078 的 authority 分離、產品邊界與舊 V2 historical-only 裁決繼續有效。

## TCC-DEC-2026-09-21-080 — Case B Product Context V2 否決與 V3 構圖計畫

- 日期：2026-09-21；來源：Supervisor 的 `USER_VISUAL_DECISION=REJECTED` 與 `CASE_B_PRODUCT_CONTEXT_V3` Composition Plan 授權。
- V2 裁決：`CASE_B_PRODUCT_CONTEXT_V2` 正式否決。禁止 Audit、Refinement、Image Edit；V2 只能作 negative evidence，不得作 Geometry／Composition base。Decision079 的 generation facts 與 Gate custody 保留，但其 `CASE_B_PRODUCT_CONTEXT_V2_VISUAL_AUDIT` 下一步取消。
- 失敗分類：產品語義與資訊量已足夠；失敗在 UI Composition、Surface Strategy、Scene/UI Integration 與 Visual Taste。具體禁止巨大置中 Glass Modal、四張 KPI Cards、三列設定清單、Setup Wizard／Enterprise SaaS、Wallpaper + HUD、每項資訊皆包矩形、Workspace 退化為 CTA，以及 Header→KPI Row→Big Card→List→Button 結構。
- V3 產品範圍：`TRADING_PLAN_PREPARATION`、Safety Core 4/4 與 Supporting Context 3/3 全數保留，不新增功能；broker／exchange／automatic execution 邊界不變。
- V3 構圖：選定 `Architectural Planning Field`。一個大型 off-center open Planning Field 與建築柱體、黑玻璃桌面、透明冰玻璃結構共構；一條 continuous Safety Spine 承載四項狀態；Strategy、Risk Profile、Evidence／Approval 成為場內 contextual anchors。`MAJOR_SURFACES=2`，硬上限 3。
- Scene/UI 裁決：Scene 必須透過建築邊界、桌面平面、冰玻璃、月光方向、反射與冷霧參與 UI Geometry；不得再用中央浮空 Panel 疊在 Wallpaper 上。構圖必須 asymmetrical，保留場景側與 Planning Field 內部的功能性 Negative Space。
- Style Pack：已重新查看 Approved `ASSET-005/010/014/018/027`；只吸收空間層次、冷暗高級感、黑玻璃、低亮度 Surface、有限 Frost/Cyan、月光穿透、雪霧深度及古風建築/科技共存。不得複製其人物、物件或 Composition Geometry；Restricted UI images 不使用。
- Plan Gate：十個必答問題 10/10 PASS；Safety 4/4、Supporting 3/3、Surface 2/3；MODEL_CALLS=0、IMAGE_API_CALLS=0。未讀取或修改 Production，V2 圖片／Prompt／Gate 未修改，未 commit／push／deploy。
- 下一步：`CASE_B_V3_GENERATE_AFTER_COMPOSITION_REVIEW`。本決策不授權 Image Generation；必須先完成 Composition Review 並取得新的明確授權。
- 取代：Decision079 的 V2 候選與 Visual Audit 下一步；Decision078 的 Product Context、authority separation 與 automatic-execution boundary 繼續有效。

## TCC-DEC-2026-09-21-081 — Case B Product Context V3 從零生成候選

- 日期：2026-09-21；來源：Supervisor 對 `CASE_B_V3_GENERATE_AFTER_COMPOSITION_REVIEW` 的正式授權。
- 構圖 custody：Decision080 的 `Architectural Planning Field` 與 `MAJOR_SURFACES=2` 原樣執行，`CASE_B_V3_COMPOSITION_PLAN.md` 未改寫。Primary Planning Field、continuous Safety Spine、三個 contextual anchors、非對稱空間與 Scene/UI 結構整合均進入 V3 prompt。
- 新候選：以唯一一次 from-scratch Image Generation Call 產生 `CASE_B_PRODUCT_CONTEXT_V3.png`，有效 PNG 1672×941，SHA-256 `C01E946798F639A06D61CF19D0100EE15A0EE4F171ADB2775F6B83BFAF23281B`。
- 產品範圍：`TRADING_PLAN_PREPARATION`、Safety Core 4/4 與 Supporting Context 3/3 全數保留；沒有 broker／exchange／live execution、自動交易、自動下單、自動平倉、API execution、價格、商品代號或偽造市場資料。
- Reference 治理：Approved `ASSET-005/010/014/018/027` 只作 taste／material／scene reference；Restricted UI images 0。舊 Case B、V2、HOME 與 Case A 均未作 edit 或 Geometry base；V3 是獨立 composition。
- Gate：`CASE_B_V3_VISUAL_GATE.json` PASS；Composition Plan used YES／rewritten NO，HPA-001／HPA-002 PASS，MAJOR_SURFACES=2，IMAGE_API_CALLS=1，FAILED_CHECKS=NONE，Production 未讀取／未修改。指定契約為 `gpt-image-2.5-sunburst`／`xhigh`；平台未回報 runtime model／quality metadata，因此 requested contract 與 runtime provenance 分離。
- Repository 驗證：Python compile 與 generation preflight PASS；locked restore 6/6、Release x64 build 0 warnings／0 errors、full tests 1738/1738 PASS，failed 0、skipped 0；M0 Frozen 8/8、V2 custody、generated-source copy custody、task scope 7/7、diff／secret scan 與 staged 0 均 PASS。沒有 commit／push／deploy。
- 邊界：Generation Gate 不構成 Visual Audit 或 Acceptance；`VISUAL_QUALITY_AUDITED=NO`。本次未執行 Audit、Refinement、WPF、Production、Design System、Style Pack 或 Case A 修改。
- 下一步：`CASE_B_V3_VISUAL_REVIEW`，必須以獨立審查權限執行；不得由本決策自行宣布 Case B Accepted。
- 取代：Decision080 的 generation next step 已完成；Decision080 的 V2 rejection、negative-evidence-only、構圖與產品邊界繼續有效。

## TCC-DEC-2026-09-21-082 — Case B V3 否決與 V4 Deterministic Geometry Wireframe

- 日期：2026-09-21；來源：Supervisor 的 `USER_VISUAL_DECISION=REJECTED` 與 `CASE_B_V4_GEOMETRY_WIREFRAME` 正式授權。
- V3 裁決：`CASE_B_PRODUCT_CONTEXT_V3` 正式否決。禁止 Audit、Refinement、Image Edit；V3 只作 historical negative evidence，不得作 Geometry／Composition base。Decision081 的生成事實與 custody 保留，但其 `CASE_B_V3_VISUAL_REVIEW` 下一步取消。
- Root cause：V3 仍為 Scene Wallpaper + Single Giant Glass Dashboard；`GIANT_CENTERED_GLASS_PANEL=YES`、`BACKGROUND_PLUS_FLOATING_UI=YES`、`SCENE_PARTICIPATES_IN_GEOMETRY=NO`、`ASYMMETRICAL_PRODUCT_COMPOSITION=FAIL`。僅靠文字 prompt 不再具有主要 Geometry 決定權。
- V4 Geometry：使用 deterministic Pillow 產生 1600×900 灰階 Wireframe。Primary Workspace 固定為 `(96,165)–(800,669)`，寬 44%、高 56%、相對 canvas 中心左移 352 px，右側為 open edge；Scene Negative Space `(940,80)–(1540,820)` 占 30.83%。
- Scene integration：Primary Workspace 與左側 architectural column 及 `y=669` desk／floor plane 幾何接觸；右側 Scene zone 禁止大型 UI Surface。因此 `ENVIRONMENTAL_ANCHOR=YES`、`WALLPAPER_PLUS_UI=NO`。
- Product geometry：`MAJOR_SURFACES=2`。Surface 1 是 open Primary Workspace；Surface 2 是沿下緣附著的 continuous Safety Rail。Safety Core 4/4 共用一條 Rail；Supporting Context 3/3 僅為場內 markers/links，不形成個別 cards。
- Gate：`CASE_B_V4_GEOMETRY_GATE.json` PASS；Primary width ratio 0.4400、height ratio 0.5600、Scene Negative Space ratio 0.3083；Giant Centered Modal、Dashboard Layout、Supporting Cards 均為 NO，FAILED_CHECKS=NONE。Python compile、deterministic re-render、16/16 Geometry assertions、Plan/Output hash、V3 custody、M0 Frozen 8/8、scope 7/7、secret/diff scan 與 staged 0 均 PASS。
- 範圍：`IMAGE_API_CALLS=0`，未生成 V4 Final Image；無 Frost、Glow、雪景、角色、Style Pack 美術或 cinematic render；未讀取／修改 Production，未 commit／push／deploy。
- 下一步：`CASE_B_V4_GEOMETRY_REVIEW`。Wireframe 完成後停止；只有 Geometry Review 通過與另行授權後，未來 Image Model 才可作 Style／Material／Scene render，且不得重新解釋主要 Layout。
- 取代：Decision081 的 V3 Visual Review next step；Decision078 的 Product Context 與 automatic-execution boundary、Decision080 的 V2 rejection 繼續有效。

## TCC-DEC-2026-09-21-083 — Case B V4 Geometry 接受與 Style Render 候選

- 日期：2026-09-21；來源：Supervisor 的 `CASE_B_V4_GEOMETRY_REVIEW=PASS`、`GEOMETRY_ACCEPTED=YES` 與 `CASE_B_V4_STYLE_RENDER` 正式授權。
- Geometry custody：`CASE_B_V4_GEOMETRY_WIREFRAME.png` 與 `CASE_B_V4_GEOMETRY_PLAN.md` 成為本次唯一 Geometry／Composition Authority，SHA-256 分別維持 `F9F2B88B…C96D96` 與 `ECC2D8AC…D936`。Image Model 不得重新設計 Layout，只能處理 Style、Material、Lighting、Scene、Depth 與 Rendering。
- Locked contract：Primary Workspace 保持左偏 44% screen width／56% screen height、右側真正 open edge、Scene Negative Space target 0.30、`MAJOR_SURFACES=2`、一條 shared Safety Rail、三個 contextual anchors、architectural-column 與 desk/floor-plane contacts。禁止 full rectangle closure、uniform glass slab、第三個大型 surface 與 protected scene zone 內的大型 UI。
- Reference 治理：實際輸入 Image 1 為 accepted wireframe；Approved `ASSET-005/010/014/027` 只作 Taste／Material／Lighting／Scene／Depth reference。沒有舊 Case B edit／geometry／composition base，沒有 Restricted UI image。因工具最多接受五張 reference，`ASSET-018` 未進正式生成輸入；一次六張 reference 的嘗試在生成前被工具拒絕，未產生影像。
- 新候選：唯一一次成功 Image Generation Call 產生 `CASE_B_V4_STYLE_RENDER.png`，有效 PNG 1672×941，SHA-256 `4FE3747FB1137F1E057A49EC1A4B3B9C9ACE79659358A9DB7A21D4A9FC36EACA`。
- Gate：`CASE_B_V4_STYLE_RENDER_GATE.json` PASS；Geometry Review PASS、Wireframe used YES、reinterpretation requested NO、Open Edge required YES、uniform glass fill requested NO、HPA-001／HPA-002 PASS、IMAGE_API_CALLS=1、FAILED_CHECKS=NONE，Production 未讀取／未修改。
- 驗證與邊界：Python compile、preflight、PNG integrity、authority custody 與 call accounting PASS。未重跑 .NET build/tests，因 Production、tests 與 dependencies 未變，沿用緊鄰前次 Release build 0 warnings／0 errors、full tests 1738/1738 PASS。Generation Gate 不構成 Visual Audit 或 Acceptance；`VISUAL_QUALITY_AUDITED=NO`。未執行 Audit、Accept、Refinement、WPF、Production、commit、push 或 deploy。
- 下一步：`CASE_B_V4_HUMAN_VISUAL_REVIEW`。需另行人工視覺裁決，不得由本決策自動接受或進入 Refinement／WPF／Production。
- 取代：Decision082 的 `CASE_B_V4_GEOMETRY_REVIEW` next step 已完成；Decision082 的 V3 rejection、deterministic geometry custody 與 Decision078 的產品／execution 邊界繼續有效。

## TCC-DEC-2026-09-21-084 — Case B Reference Panel From-Scratch Rebuild

- 日期：2026-09-21；來源：Supervisor 對重新生成 Case B 的後續明確授權，以及外部參考面板 `ChatGPT Image 2026年9月15日 下午09_51_14.png`。
- Authority 更新：本次參考面板是 Product Information／Layout／Composition Authority，決定資訊層級、功能區塊、工作區比例、導覽、Safety Core、主／輔助資訊、面板關係與 Command Center 密度。此後續明確需求取代 Decision080–083 中「Case B 必須極低資訊密度、單一 focus/open field、巨大 protected negative space」作為本次 rebuild 的構圖假設；Accepted Style Pack 與 Accepted Design System V1 仍分別只決定 Taste／Material／Atmosphere 與 Frost／Glow／Hierarchy／Surface behavior。
- Generation：完全 from scratch，沒有 edit、refinement 或 Wireframe。唯一一次成功 built-in Image Generation Call 使用外部參考面板作產品介面權威，並使用 Approved `ASSET-005/010/014/027` 作風格素材；所有歷史 Case B 圖與 V4 Wireframe 均未作 edit、Geometry 或 Composition base。
- 新候選：產生 `CASE_B_REFERENCE_PANEL_REBUILD.png`，有效 PNG 1672×941，SHA-256 `EFE9CB7ABCC848B0AF2B2D868D747C15AA6B5A57B88D72CA7F70D2CBE5D55A23`。原始 generated image 與 workspace copy hash 一致。
- 產品結構：畫面包含 global top bar、左側 navigation、Command Center header、四項 Safety Core、Observation Worktable／Watchlist、1H／4H／1D Market Anchor、Capital／Risk Terrain、Positions Snapshot、Today's Priorities、Mental State、optional AI Summary 與 Activity；資訊密度明顯接近參考面板，不再是低資訊量 key art。
- 產品安全：Unavailable／Unknown／Read-only 狀態如實呈現；未聲稱真實價格、資產或帳戶資料；AI 維持 optional；沒有 broker／exchange write、automatic trading、auto-order／auto-close、order submission、BUY／SELL control 或 execution API。
- 驗證與邊界：生成前 output absent、reference custody 與 forbidden-reference preflight PASS；成功呼叫 1 次；PNG size/hash 與 generated-source copy custody PASS。未重跑 .NET build/tests，因 Production、tests、dependencies 未變；沿用最近 Release build 0 warnings／0 errors、full tests 1738/1738 PASS。`VISUAL_QUALITY_AUDITED=NO`，未執行 Audit、Accept、Refinement、WPF、Production、commit、push 或 deploy。
- 下一步：`CASE_B_REFERENCE_PANEL_HUMAN_REVIEW`。本決策只記錄生成事實，不構成視覺接受。
- 取代：Decision083 的 `CASE_B_V4_HUMAN_VISUAL_REVIEW` 作為目前 next step；Decision083 的 V4 生成事實保留為 historical non-accepted candidate。Decision078 的 product safety／execution boundary 繼續有效。

## TCC-DEC-2026-09-21-085 — Case B Reference Panel Character-Required Rebuild

- 日期：2026-09-21；來源：Supervisor 對 Case B 的後續人物必要條件。
- 人物語意：新的 Case B 必須呈現「有角色存在的 Trading Command Center」。人物用於強化 Accepted Style Pack 世界觀、平衡高資訊密度、增加場景深度與辨識度；視覺權重固定為 `主要交易工作區 > 人物 > 純背景裝飾`，不得成為人物海報、角色立繪主視覺或遊戲登入畫面。
- Placement boundary：人物置於畫面右側窄幅環境帶，與雪夜、古風建築、冰霜、月光與霧氣融合；生成 prompt 明確禁止人物遮擋 Safety Core、主要交易介面、Watchlist、Market Anchor、Risk Terrain、底部支援模組、導覽、header 或關鍵標籤。
- Generation：完全 from scratch，沒有 edit 或 refinement。外部參考面板繼續作 Product Information／Layout／Composition／Character Placement Authority；Approved `ASSET-005/010/018/014` 只作人物世界觀、材質、光影與場景 reference。所有歷史 Case B、V4 Wireframe 與前一張 `CASE_B_REFERENCE_PANEL_REBUILD.png` 均未進入輸入。
- 新候選：唯一一次成功 built-in Image Generation Call 產生 `CASE_B_REFERENCE_PANEL_REBUILD_WITH_CHARACTER.png`，有效 PNG 1672×941，SHA-256 `C9E300A724D6B43EB9081C9D9D01CD9D200004E424DC46D519BBD7B55D230927`；generated source 與 workspace copy hash 一致。
- 產品與安全：Reference Panel 的 global top bar、左側 navigation、四項 Safety Core、Observation Worktable、Market Anchor、Capital／Risk Terrain 與底部支援模組繼續保留；Unknown／Unavailable／Read-only 語意不變，沒有 broker／exchange write、automatic trading、order submission、BUY／SELL control 或 execution API。
- 驗證與邊界：preflight 確認新 output absent、人物必要條件與 forbidden-reference 規則；成功呼叫 1 次；PNG size/hash 與 copy custody PASS。未重跑 .NET build/tests，因 Production、tests、dependencies 未變；沿用最近 Release build 0 warnings／0 errors、full tests 1738/1738 PASS。`VISUAL_QUALITY_AUDITED=NO`，未執行 Audit、Accept、Refinement、WPF、Production、commit、push 或 deploy。
- 下一步：`CASE_B_REFERENCE_PANEL_CHARACTER_HUMAN_REVIEW`。本決策只記錄生成事實與人物契約，不構成視覺接受。
- 取代：Decision084 的 `CASE_B_REFERENCE_PANEL_HUMAN_REVIEW` 作為目前 next step；Decision084 的第一張 reference-panel rebuild 保留為 historical non-accepted candidate，未被覆寫。

## TCC-DEC-2026-09-21-086 — Case B High-Fidelity Character Reference Rebuild

- 日期：2026-09-21；來源：Supervisor 提供人物參考 `exec-6c6a6c34-2bcf-4141-96b5-bc2df4e118ae.png`，並明確要求「以參考圖高精度還原、不要複製貼上、生成一體的圖」。
- Character authority：人物參考為 Character Appearance／Costume／Material Authority，SHA-256 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`。生成需高精度保留臉部側面輪廓、黑色長髮與高髻、金色髮飾、白色毛領、半透明白袍金線、深色護臂、流蘇與克制紅寶石細節；不得擷取、貼合、描摹、photobash 或把人物原圖當 edit base。
- Authority 分工：外部參考面板繼續作 Product Information／Layout／Composition Authority；Approved Style Pack 三張資產只作雪夜世界觀、冰霜／黑玻璃材質、古風建築、月光與景深參考。人物參考只決定角色外觀，不得改寫產品版型。
- Generation：完全 from scratch。唯一一次成功 built-in Image Generation Call 同時使用人物參考、產品面板與三張 Style assets；所有歷史 Case B、Decision084／085 rebuild、V4 Wireframe 均未進入輸入，沒有 Image Edit 或 refinement。
- 新候選：產生 `CASE_B_REFERENCE_PANEL_REBUILD_CHARACTER_REFERENCE.png`，有效 PNG 1672×941，SHA-256 `431CBA2F62B9A0D162D9B4FFE7EF456F2237AD70CDF256658D8E18CB4AE7A812`；generated source 與 workspace copy hash 一致。
- 構圖結果：人物重新渲染於最右側窄幅環境帶並共享雪霧、月光、反射與建築景深；主要交易工作區保持第一視覺權重，人物未遮擋左側導覽、四項 Safety Core、Watchlist、Market Anchor、Capital／Risk Terrain 或底部支援模組。產品維持高資訊密度 Trading Command Center，而非人物海報或登入畫面。
- 產品安全：Unknown／Unavailable／Read-only／Optional 語意維持；沒有 broker／exchange write、automatic trading、order submission、BUY／SELL control、execution API 或偽造帳戶數據。
- 驗證與邊界：output-absent preflight、PNG integrity、1672×941 dimensions、SHA-256 與 generated-source copy custody PASS。未重跑 .NET build/tests，因 Production、tests 與 dependencies 未變；沿用最近 Release build 0 warnings／0 errors、full tests 1738/1738 PASS。`VISUAL_QUALITY_AUDITED=NO`，未執行 Audit、Accept、Refinement、WPF、Production、commit、push 或 deploy。
- 下一步：`CASE_B_REFERENCE_PANEL_CHARACTER_REFERENCE_HUMAN_REVIEW`。本決策只記錄生成事實與 reference custody，不構成視覺接受。
- 取代：Decision085 的 `CASE_B_REFERENCE_PANEL_CHARACTER_HUMAN_REVIEW` 作為目前 next step；Decision085 的候選保留為 historical non-accepted candidate，未被覆寫。

## TCC-DEC-2026-09-21-087 — Case B Two-Reference Soft Ethereal Character Rebuild

- 日期：2026-09-21；來源：Supervisor 後續明確要求「不要看之前的圖，就看這兩張」，並指出人物必須空靈、高度還原，前一個兩圖候選「不像原參考圖這麼柔」。
- Reference boundary：正式生成輸入嚴格限制為兩張：`ChatGPT Image 2026年9月15日 下午09_51_14.png`（Product UI／Layout／Composition／Scene Authority，SHA-256 `30308D0704F3E605AB5979BCDDF1A81B766147DFD9B2F5811FED68339DF94588`）與 `exec-6c6a6c34-2bcf-4141-96b5-bc2df4e118ae.png`（Character Appearance／Identity／Softness Authority，SHA-256 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`）。歷史 Case B、先前生成圖、V4 Wireframe 與 Style Pack 圖均未進入輸入。
- Character correction：高精度保留人物柔和橢圓臉、圓潤但纖細的面部轉折、平靜清澈眼神、細緻鼻唇、瓷白半透明膚質、黑髮高髻、金飾、白色毛領、半透明白袍金線、深色護臂與紅寶石細節；以低對比月光、柔和雪地補光、薄霧與細髮絲呈現 `空靈／溫柔／清冷／安靜／純淨`，並明確禁止尖銳下顎、凹陷臉頰、凌厲眼線、濃妝、嚴厲目光與成熟冷艷化。
- Generation：完全 from scratch，沒有 edit base、copy-paste、cutout、tracing、photobash 或 previous-output inheritance。唯一一次成功 built-in Image Generation Call 只使用上述兩張 reference。
- 新候選：`CASE_B_TWO_REFERENCE_SOFT_ETHEREAL_CHARACTER_REBUILD.png`，有效 PNG 1672×941，SHA-256 `33CEB998FA27778A9A60C04E8D858CADBA0ADB034D0488904E4F77F7408A7940`；generated source 與 workspace copy hash 一致。
- 構圖與產品：人物位於最右側雪夜環境帶，視覺權重低於主交易工作區且未遮擋 Safety Core、Watchlist、Market Anchor、Risk Terrain 或底部支援模組。外部面板的高資訊密度 Trading Command Center 結構、Unknown／Unavailable／Read-only／Optional 語意與 execution safety boundary 均保留。
- 驗證與邊界：two-reference preflight、output absent、PNG integrity、1672×941 dimensions、SHA-256 與 generated-source copy custody PASS。未重跑 .NET build/tests，因 Production、tests 與 dependencies 未變；沿用最近 Release build 0 warnings／0 errors、full tests 1738/1738 PASS。`VISUAL_QUALITY_AUDITED=NO`，未執行 Audit、Accept、Refinement、WPF、Production、commit、push 或 deploy。
- 下一步：`CASE_B_TWO_REFERENCE_SOFT_CHARACTER_HUMAN_REVIEW`。本決策不構成視覺接受。
- 取代：Decision086 的 current next step 與使用三張 Style assets 的 reference scope；Decision086 的生成事實保留為 historical non-accepted candidate。

## TCC-DEC-2026-09-21-088 — Case B Three Softer Two-Reference Variants

- 日期：2026-09-21；來源：Supervisor 要求「再柔一點看看，給我三版」。
- Reference boundary：三版正式輸入都嚴格限制為兩張原始 reference：外部 Command Center 面板負責 Product UI／Layout／Composition／Scene，人物圖負責 Character Identity／Appearance／Softness。所有歷史 Case B、先前生成圖、V4 Wireframe 與 Style Pack 圖均未進入輸入。
- Variant design：V1 `Natural Softness` 使用柔和雪地補光、低對比睫毛與自然膚質；V2 `Ethereal Soft Mist` 使用珍珠月光、細薄雪霧與絲綢式明暗轉折；V3 `Pure Gentle Serenity` 使用圓潤面部轉折、平靜眼神與克制暖冷平衡。三版只在人物柔和表現上形成受控差異，產品方向與 execution safety boundary 不變。
- Generation：三個獨立 built-in Image Generation Calls，全部 FROM SCRATCH；沒有 edit base、copy-paste、cutout、tracing、photobash 或 previous-output inheritance。
- Outputs：`CASE_B_TWO_REFERENCE_SOFTER_V1.png`（SHA-256 `6857B1E0E38881A851C7BB3392F837AB03FD3BAD10D3B8FD4ADA8FB9CB61BF11`）、`CASE_B_TWO_REFERENCE_SOFTER_V2.png`（`1DEAB45D931ACE817571273D73E955800C561076F7247967FE87D6E265C92006`）、`CASE_B_TWO_REFERENCE_SOFTER_V3.png`（`3EBD26EACB8A099A318AD3066D6791C3D0AC15E06C91922B022BD16298B45181`）；全部為有效 1672×941 PNG，generated source 與 workspace copy hash 一致。
- 驗證與邊界：three-output-absent preflight、3/3 PNG integrity、dimensions、SHA-256 與 copy custody PASS。未重跑 .NET build/tests，因 Production、tests 與 dependencies 未變；沿用最近 Release build 0 warnings／0 errors、full tests 1738/1738 PASS。`VISUAL_QUALITY_AUDITED=NO`，未自動選案，未執行 Audit、Accept、Refinement、WPF、Production、commit、push 或 deploy。
- 下一步：`CASE_B_TWO_REFERENCE_SOFTER_VARIANT_SELECTION`。等待 Supervisor 選 V1／V2／V3 或全部退回。
- 取代：Decision087 的單一候選 human-review next step；Decision087 的生成事實保留為 historical non-accepted candidate。

## TCC-DEC-2026-09-21-089 — Case B Selected-Base Three Regenerated Variants

- 日期：2026-09-21；來源：Supervisor 貼回並指定 SHA-256 `3EBD26EACB8A099A318AD3066D6791C3D0AC15E06C91922B022BD16298B45181` 的 V3 圖，要求「以這版，再生三版看看」。
- Reference boundary：該 selected image 是本次唯一 visual reference；沒有使用原始面板、原始人物圖、其他 V1／V2／V3、歷史 Case B、V4 Wireframe 或 Style Pack 圖作額外輸入。
- Preservation contract：三版都要求高密度 Trading Command Center 結構、上方工具列、左側導覽、四項 Safety Core、主／輔助模組、右側人物 footprint、手部靠胸姿態、月夜雪景、梅枝、燈籠與霜玻璃系統維持接近 selected image；產品工作區持續高於人物視覺權重。
- Variant design：V1 `Closest Regeneration` 僅強化自然柔和、髮絲、毛領與薄紗細節；V2 `Soft Luminous` 增加珍珠雪地補光、低臉部對比與細薄霧氣；V3 `Quiet Cinematic` 加深藍夜、克制燈籠反光與更平靜神情。
- Generation：三個獨立 reference-guided built-in Image Generation Calls，輸入皆只有 selected image。沒有增加第四版、沒有其他 reference、沒有 Production 修改。
- Outputs：`CASE_B_SELECTED_BASE_REGENERATED_V1.png`（SHA-256 `6B5B54D2F18DB629B4761A101049233ED430287A2E0FE563781245180C3194A9`）、`CASE_B_SELECTED_BASE_REGENERATED_V2.png`（`B44CD72630678C5DEE0C9551EC9A0185DF8526B068F0931B748D2D079B9EBF39`）、`CASE_B_SELECTED_BASE_REGENERATED_V3.png`（`5A0AEB7D1E4E567AFC8BE4E10BEBAC26B953F351688AB1DE7A6DD9F11EDC2CD2`）；3/3 為有效 1672×941 PNG，generated source 與 workspace copy hash 一致。
- 驗證與邊界：selected-reference custody、three-output-absent preflight、3/3 PNG dimensions／SHA-256／copy custody PASS。未重跑 .NET build/tests，因 Production、tests 與 dependencies 未變；沿用最近 Release build 0 warnings／0 errors、full tests 1738/1738 PASS。`VISUAL_QUALITY_AUDITED=NO`，未自動選案，未執行 Audit、Accept、Refinement、WPF、Production、commit、push 或 deploy。
- 下一步：`CASE_B_SELECTED_BASE_VARIANT_SELECTION`。等待 Supervisor 選 V1／V2／V3 或全部退回。
- 取代：Decision088 的 three-softer-variant selection next step；Decision088 的生成事實保留為 historical non-accepted candidates。

## TCC-DEC-2026-09-21-090 — Case B Specification Input Pack Prepared

- 日期：2026-09-21；來源：Supervisor 選定 SHA-256 `3EBD26EACB8A099A318AD3066D6791C3D0AC15E06C91922B022BD16298B45181` 的 Case B 圖並正式授權 `CASE_B_SPEC_INPUT_PACK_PREP`。
- Reference Panel Authority：選定圖以 `authority/CASE_B_REFERENCE_PANEL_AUTHORITY.png` 獨立保存，角色為 `PRODUCT_LAYOUT_INFORMATION_DENSITY_AUTHORITY`；它約束 Layout direction、Information density、Navigation、Main workspace、Safety Core grouping、Supporting modules、Character relationship 與 Scene/UI balance，但不直接決定正式產品功能、exact pixel geometry、exact colors 或 exact WPF values。
- Pack contents：建立 `uiux_cleanroom/case_b_spec_input_pack/` 的 authority／product／design_system／style_pack／validation／negative_evidence／visual_assets／governance／output 九區。Product Content Plan、Accepted Design System、Accepted Style Pack 正式檔與 Case B validation authority 均以原內容複製；新增的 summary、authority map、rejected directions 與 CrewAI task 只整理 Supervisor 已授權內容。
- Visual custody：Approved Registry 的 27/27 assets 全數物化；其中 16 個 Character assets 另作 unchanged index copies。`ASSET-009` 在 cleanroom Pack B snapshot 中缺少，但已從 `STYLE_PACK_MANIFEST.json` 宣告的精確 Pack B source path 取回同名原檔，沒有猜測或替換。Restricted UI references 2/2 未被 promoted。
- Negative evidence：只收錄可明確確認為 user-rejected 的 `CASE_B_FOCUS_WORKSPACE.png`、`CASE_B_FOCUS_WORKSPACE_REFINED.png`、`CASE_B_PRODUCT_CONTEXT_V2.png`、`CASE_B_PRODUCT_CONTEXT_V3.png`；全部標記 `NEGATIVE_EVIDENCE_ONLY` 且禁止作 Geometry／Layout／Style Authority、Edit／Generation Base 或 Positive Reference。
- Contact sheets：deterministic 產生 3 張 Approved Style Assets 聯絡表與 1 張 Character 聯絡表；每格標示 Asset ID／檔名，未使用 OCR，原圖未修改。權威面板保持獨立，未混入 Style contact sheets。
- Manifest／Gate：`CASE_B_SPEC_INPUT_MANIFEST.json` 收錄 74 個 payload 檔案，Authority Role 僅使用八個允許值；Manifest 自身依 conventional self-hash exclusion 不列入自身 entries。74/74 hashes、16/16 duplicate custody、Restricted promotion 0、Rejected promotion 0、required counts 與 Gate 全部 PASS。
- 邊界：MODEL_CALLS=0、VISION_API_CALLS=0、IMAGE_API_CALLS=0；未呼叫 CrewAI、未生成正式 Case B 規格書、未生圖、未分析 rejected images、未讀取／修改 Production、未修改 Design System、Style Pack、Case A 或 Product Context，未 commit／push／deploy。
- 下一步：`CREWAI_CASE_B_SPEC_SYNTHESIS`，只有 Supervisor 確認輸入包後才能另行授權。
- 取代：Decision089 的 variant-selection next step；Decision089 的圖像生成事實保留。選定圖成為本輸入包的最新 Reference Panel Authority，但本決策不構成 Production acceptance 或 WPF implementation authority。

## TCC-DEC-2026-09-22-091 — Case B Wireframe V2 凍結為正式渲染骨架

- 日期：2026-09-22；來源：Supervisor 對 `CASE_B_VISUAL_RENDER_PREP` 的正式授權，以及對 `CASE_B_WIREFRAME_V2.png` 的明確人工通過。
- Geometry custody：`uiux_cleanroom/module_validation/output/CASE_B_WIREFRAME_V2.png`（1600×900；SHA-256 `2FEB8E8CD50E298FBBF51A3CEBC9881A6A44A9C7B6511CBC8F980E69954642A6`）凍結為下一張 Case B 正式視覺候選的唯一 Geometry／Composition Authority。左導覽、頂部共享 Safety Core、中央主要工作區、三項附著式 Supporting Context、右側人物／場景區、開放／階梯右邊界與 scene intrusion 均不得由 Image Model 重新解釋。
- Authority 分工：Accepted Case B Spec 只決定八項產品功能與語意；Accepted Design System 決定層級、Surface、Frost／Glow 與 Scene/UI integration；Accepted Style Pack 只決定 Taste／Material／Atmosphere；最新 Reference Panel 只提供資訊密度、層級與 Scene/UI balance。所有幾何衝突均由 V2 優先。
- Reference boundary：所有歷史 Case B、被否決候選與 A／B／C variants 均禁止作修改底圖、構圖來源、幾何來源或正向參考。下一階段只允許 V2、最新 Reference Panel 及 Approved `ASSET-005/014/019` 作受限輸入；三個 Style assets 沒有 Geometry／Function Authority。
- Product／visual lock：只允許 `TRADING_PLAN_PREPARATION`、Safety Core 4/4 與 Supporting Context 3/3；主要工作區保持第一焦點，人物只作右側場景錨點。四個硬護欄、禁止方向、產品內容保留與 `CBN-006`／`CBN-010` 人工視覺驗收均已寫入計畫及提示詞。
- Generation boundary：本次只完成凍結、計畫與提示詞，`IMAGE_API_CALLS=0`。下一步 `CASE_B_VISUAL_RENDER_V1` 僅可生成一張第一候選；候選落盤後立即停止，交 Supervisor 人工看圖，不得自動接受、Refinement、WPF 或 Production。
- 驗證：Focused prep/custody 21/21、Accepted Spec 9/9、Input Pack 74/74、Production/Test 18/18、locked restore 6/6、Release x64 build 0 warnings／0 errors、full tests 1738/1738 全數 PASS；HEAD 未改變且既有 dirty baseline 未擴大。
- 取代：Decision090 的 `CREWAI_CASE_B_SPEC_SYNTHESIS` next step 已由後續完成的 Specification Audit／Repair／Reaudit／Acceptance／Implementation Prep／Wireframe 工作取代；Decision090 的 Input Pack custody 與最新 Reference Panel 權威邊界持續有效。

## TCC-DEC-2026-09-22-092 — TCC Reference Master Rebase

- 日期：2026-09-22；來源：Supervisor 正式決策 `TCC_REFERENCE_MASTER_REBASE` 與本次明確指定的兩張圖片。
- 最高 UI Authority：`TCC_REFERENCE_MASTER_AUTHORITY.png` 是新的 UI／layout／composition／information-density／visual-language／scene-integration 母版，1672×941 RGB，SHA-256 `30308D0704F3E605AB5979BCDDF1A81B766147DFD9B2F5811FED68339DF94588`。
- Character Authority：`TCC_CHARACTER_IDENTITY_AUTHORITY.png` 是獨立人物 identity／appearance／hair／clothing／accessories／transparent-silhouette authority，1024×1536 RGBA，SHA-256 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`。人物圖不取得 UI geometry 或產品功能權威。
- Authority order：兩張凍結圖片依上述分工優先；本次從圖片與產品約束萃取的九份規格居次；既有 Accepted Design System／Style Pack 只作 compatibility constraint，不得反向改寫新母版。
- Historical boundary：舊 Case B、其 wireframe、reference panel、variants、render candidates 與衍生驗證資料全部保留，但自本決策起只標示為 `HISTORICAL_REFERENCE_ONLY`，不得作新母版、正向 reference、幾何或視覺語言 authority。
- Synthesis：沿用 repository 既有 CrewAI sequential architecture，由 CrewAI 實際調用 Vision；成功 Vision API 1 次、同時檢視 2 張圖片，Image API 0 次。首次超出模型 completion limit 的拒絕請求亦保留在 call accounting，不隱藏技術失敗。
- Outputs：建立 `TCC_REFERENCE_MASTER_SPEC.md`、`TCC_REFERENCE_LAYOUT_SPEC.md`、`TCC_REFERENCE_CONTENT_SPEC.md`、`TCC_REFERENCE_VISUAL_LANGUAGE_SPEC.md`、`TCC_REFERENCE_CHARACTER_SPEC.md`、`TCC_REFERENCE_SCENE_SPEC.md`、`TCC_REFERENCE_COMPONENT_SPEC.md`、`TCC_REFERENCE_ACCEPTANCE_CRITERIA.md` 與 `TCC_REFERENCE_MACHINE_RULES.json`，共 9/9。
- Validation：authority／input hashes、10/10 manifest、九份輸出、Navigation 8/8、Safety 4/4、Modules 8/8、24 項 acceptance、protected custody 1675/1675、old Case B function leaks 0 均 PASS；locked restore、Release x64 build 與 full tests 1738/1738 PASS。
- Scope：本決策不授權或完成 image generation、WPF、Production 修改、dependency 變更、commit、push、deploy 或規格接受。`specification_accepted=false`。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_SPEC_INDEPENDENT_AUDIT`。
- 取代：本決策取代 Decision091 與所有 Case B 決策作為 current cleanroom authority／next-step 的效力；其已發生事實與 custody 紀錄仍作 historical evidence 保留。

## TCC-DEC-2026-09-22-093 — Reference Master Specs 正式接受與凍結

- 日期：2026-09-22；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_SPEC_ACCEPTANCE`，且先決條件 `TCC_REFERENCE_MASTER_SPEC_REAUDIT=PASS`、`RMR_MEDIUM_001=VERIFIED_RESOLVED`。
- 決策：`TCC_REFERENCE_MASTER_SPEC.md`、Layout、Content、Visual Language、Character、Scene、Component、Acceptance Criteria 與 Machine Rules 共九份規格，以 `TCC_REFERENCE_MASTER_SPEC_ACCEPTED_HASHES.json` 登錄的精確 SHA-256 正式成為 `ACCEPTED / FROZEN`。為保持 hash custody，不回寫九份規格內 pre-acceptance provenance；Acceptance 文件、hash registry 與 PASS Gate 是目前接受狀態的治理來源。
- Authority stack：UI Master 圖片繼續是 layout、content、composition、information density、visual language、scene integration 與 character placement 的最高權威；Character Identity 圖片繼續是 identity、face、hair、costume、accessories 與 transparent silhouette 的最高權威；九份 Accepted Specs 將兩張圖轉成可施工、可驗收規則。
- Evidence：九份 specs 9/9 present 且 SHA-256 與 repaired/re-audited final bytes 一致；兩張 Authority 2/2 hash exact match；Original Findings 11/11 closed、`RMR-MEDIUM-001` independently verified resolved；new BLOCKER/HIGH/MEDIUM/LOW 全 0；Machine Rules valid/consistent；Acceptance Criteria executable；Constructability PASS。
- Revision policy：後續若需改動任何 accepted spec，必須另開明確 Spec Revision 並重做 hash、audit、repair（如需要）、Re-Audit 與 Acceptance；不得 silent modification。
- Visual acceptance boundary：本決策只接受規格，不接受任何未來 rendered candidate、wireframe、WPF 或 Production 視覺成品；12 項 `HUMAN_VISUAL_REVIEW_REQUIRED` criteria 仍須以最終候選與 Authority 圖片並排人工驗收。
- Historical boundary：Old Case B 與其衍生資料繼續為 `HISTORICAL_REFERENCE_ONLY`，active leaks 0，不得重新取得 geometry、layout、content、visual-language、function 或 positive-reference authority。
- Scope／custody：Production/Test 1342/1342 unchanged；九份 accepted specs 未修改；未執行 CrewAI、Vision、Image API、wireframe、WPF、Production、Tests 修改、commit、push 或 deploy。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_IMPLEMENTATION_PREP`；不得直接進入 WPF、Production 或最終生圖。
- 取代：本決策取代 Decision092 的 `specification_accepted=false` 與 `TCC_REFERENCE_MASTER_SPEC_INDEPENDENT_AUDIT` next step；Decision092 的 Authority 分工、生成歷程與 historical boundary 繼續有效。

## TCC-DEC-2026-09-22-094 — Reference Master Visual V1 正式接受與凍結

- 日期：2026-09-22；來源：Supervisor 正式人工視覺決策 `TCC_REFERENCE_MASTER_VISUAL_V1_HUMAN_REVIEW`。
- 決策：`TCC_REFERENCE_MASTER_VISUAL_V1.png` 以 SHA-256 `5BC1C464A04A8D79D854B099468DBB2D6DDB71BCB5FE33DD8E701EB9F54B6BF7` 正式接受並凍結為 `ACCEPTED_BASELINE`；`FINAL_VISUAL_ACCEPTED=YES`。
- Gate：候選實際 SHA-256 與既有 Visual V1 Gate 相符；既有 deterministic acceptance 保持 `16/16 PASS`。Supervisor 將候選與正式 UI Master Authority、Character Identity Authority 並排實際檢視後，12 項人工視覺驗收全部 PASS。
- 視覺判定：人物身份替換成功，並保留正式 UI 母版的構圖、內容、模組、資訊密度、場景、冬夜氣質、材質與光影；人物維持右側場景角色且未壓過主要工作區。臉部與白色毛領相較環境略乾淨、略亮，但不構成缺陷或 acceptance blocker。
- Regeneration boundary：不得生成 V2。重新生成的母版漂移風險高於上述非阻擋差異的價值。
- Scope／custody：本階段 MODEL_CALLS=0、VISION_API_CALLS=0、IMAGE_API_CALLS=0；候選圖、九份 Accepted Specs、Authority、Production 與 Tests 均未修改；未執行 WPF、commit、push 或 deploy。
- 驗證：Focused acceptance 11/11、Accepted Spec hashes 9/9、Authority hashes 2/2、locked restore、Release x64 build 0 warnings／0 errors、full tests 1738/1738 全數 PASS。
- 下一步：`TCC_REFERENCE_MASTER_WPF_IMPLEMENTATION_PREP`。下一階段只規劃如何把已接受的視覺母版實作成 WPF，不是重新設計；本決策本身不授權 Production 修改。
- 取代：本決策取代 Visual V1 Render Gate 的 `READY_FOR_HUMAN_REVIEW`／`PENDING` current-state 與 `TCC_REFERENCE_MASTER_VISUAL_V1_HUMAN_REVIEW` next step；原 Render Report、Render Metadata 與 deterministic Gate 保留為生成及驗證歷史證據。

## TCC-DEC-2026-09-23-095 — Reference Master WPF Implementation Prep

- 日期：2026-09-23；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_WPF_IMPLEMENTATION_PREP`。
- 決策：Accepted Visual、九份 Accepted Specs 與既有 Implementation Prep 已轉成 12 份 WPF/.NET 10 工程規劃；本階段只規劃，不建立正式 XAML/ViewModel，不修改 Production／Tests。
- 架構接入：保留 `Tcc.DesktopHost` 唯一 composition root、Host lifecycle、constructor injection、trusted built-in Theme projection、monitor/DPI/accessibility hooks 與 architecture-test discipline。Reference Master 主 layout 採 responsive Grid；Canvas 僅限 noninteractive scene/character/decorative placement；禁止全畫面 baseline raster UI 與 global Viewbox scaling。
- Components/Data：TopBar、SideNavigation、CommandHeader、SafetyStatusTile、8 個主模組、SceneLayer、CharacterLayer 均有正式 View/ViewModel/test boundary 規劃。顯示狀態分為 Content/Access/Configuration/Optionality dimensions；`Unknown`、`Unavailable`、`Empty`、`NotSet`、`ReadOnly` 不得混用或硬寫成真實資料。
- Resources：背景、人物與裝飾可作 approved raster derivatives；Navigation、Top Bar、按鈕、表格、清單、文字、圖例、圖示與狀態必須是真正 WPF controls/resources。Character 優先使用已凍結透明 Authority；無 UI clean scene 若不存在，必須另行授權重建／清除，不得在 Prep 執行。
- Scaling/validation：1672×941 為 reference measurement；runtime 使用 WPF DIPs、star/minmax 與 1280×720 candidate minimum effective viewport。後續 screenshot pipeline 必須 build/launch/fixed viewport/capture/compare/finding/repair/repeat，並保留 human visual review。
- Sequence：正式施工分 WPF-P1 結構與幾何、P2 材質與視覺、P3 人物與場景融合、P4 資料狀態與互動、P5 最終視覺校準；一次只允許一個 phase `IN PROGRESS`。
- Gate：Accepted Visual 1/1、Accepted Specs 9/9、Authority 2/2、12/12 prep files、Layers 7/7、Layout Regions 7/7、Components 14/14、Display States 8/8、Implementation Steps 19/19、JSON 3/3、Old Case B active leaks 0；Production／Tests digests unchanged；Image API calls 0。
- 下一步：唯一允許的 next step 是另行授權 `TCC_REFERENCE_MASTER_WPF_P1`，且 P1 只做 Shell + Geometry + Major Regions；不得自動擴張到完整視覺、人物融合、資料互動、commit、push 或 deploy。
- 取代：本決策取代 Decision094 的 `TCC_REFERENCE_MASTER_WPF_IMPLEMENTATION_PREP` next step；Decision094 的 Accepted Visual freeze、V2 禁令與 Authority custody 持續有效。

## TCC-DEC-2026-09-23-096 — Reference Master WPF P1 Geometry Authority Migration

- 日期：2026-09-23；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_WPF_P1`，範圍嚴格限於 Shell + Geometry + Major Regions。
- 決策：目前 WPF runtime 的 UI 幾何正式由 Accepted Visual、九份 Accepted Specs、Reference Master Geometry Map 與 WPF Layout Map 控制。1672×941 是 design-reference measurement；runtime 採 responsive Grid、WPF DIP、star/minimum sizing 與 ScrollViewer fallback，不採 global Viewbox、巨型 Canvas 或全畫面 raster UI。
- Geometry contract：根布局使用 `[8.5,24.5,31.5,21,14.5]` 五欄與 `[5.5,14,12,62.5,6]` 五列；保留 Top Bar、Side Navigation、Command Header、四格 Safety Core、Main/Lower Workspace、8 個正式 module bounds、右側 14.5% Character reservation 與 full-root Scene reservation。
- Legacy migration：Phase6 的 compiled-surface、DI、startup、native boundary、DPI、High Contrast、text-scale、semantic honesty 與 negative gates 繼續有效。舊 B3 的 2142×1196 Canvas/Viewbox current-runtime geometry 與研究階段永久 production-byte lock，經正式報告 supersede 為 Accepted Reference Master 的 structured geometry、manifest、no-raster、Release、full-test 與 screenshot gates；舊資產／研究證據仍保留。Test removed 0、skipped 0，`COVERAGE_WEAKENED=NO`。
- Architecture：`Tcc.DesktopHost` 保持唯一 composition root；沒有新增 project、package、dependency、DI registration、ViewModel、compiled type、public API 或第二套 navigation/MVVM。結構元素不使用 `x:Name`，避免擴大 locked compiled member surface。
- Evidence：Accepted Visual 1/1、Accepted Specs 9/9、Authority 2/2、WPF Prep Gate 均 PASS；focused P1/B3 37/37、Phase6 160/160、full tests 1745/1745 PASS；locked restore PASS；Release x64 build 0 warnings／0 errors；Old Case B active runtime leaks 0。
- Screenshot：實際 WPF process 在 120 DPI 以 2090×1176 physical capture 後正規化為 1672×941 reference-DIP review artifact；SHA-256 `1D731E9FB1B4E0FED045C55D75312ED502B0909864CE246EE73906239FB067F9`。幾何 deterministic comparison PASS，但人工 geometry review 必須保持 `PENDING`。
- Scope：P2–P5 最終材質、人物、場景、圖示、字體校準、圖表、完整 binding、hover/focus 與動畫均未實作；Accepted Visual/Specs/Authority 未修改；未 commit、push、deploy。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_WPF_P1_HUMAN_GEOMETRY_REVIEW`；人工通過前不得進入 P2。
- 取代：本決策取代 Decision095 的 `TCC_REFERENCE_MASTER_WPF_P1` next step，並只 supersede 舊 B3 作為 current runtime geometry authority 的效力；Decision095 的架構、scaling、phase sequence 與 custody 邊界持續有效。

## TCC-DEC-2026-09-23-097 — Reference Master WPF P1 Geometry Repair R1

- 日期：2026-09-23；來源：Supervisor 正式 Human Geometry Review `REVISE`，指定 HGR-001、HGR-002 與追加 HGR-003。
- Window Chrome：正式移除原生 Windows Title Bar，採 `WindowStyle=None` 與 WPF `WindowChrome`；App Top Bar 位於視窗最頂，Win32 runtime 測得 client origin 與 window top 差值 0。保留 8 DIP resize border、Minimize／Maximize-Restore／Close 真實 controls、keyboard focus、UI Automation Name／InvokePattern 與 resize 行為。
- Navigation rhythm：Navigation item height 從 44 調整為 52 DIP，保留原字級與 2 DIP vertical margin；修復範圍是 item/container geometry，不以字體放大替代。
- Right-edge composition：移除可見的 `CHARACTER RESERVED` 卡片與文字，但保留透明、空白、noninteractive `Region.CharacterReserved` layout region。Safety Core、Risk Terrain 與 AI/Activity 以不同右側 extent 建立 stepped/open edge，為未來場景／人物穿插保留空間。
- Coverage：新增三項 P1 geometry tests；舊 Phase6 asset test 改為要求 character region transparent／empty／noninteractive 且禁止可見 label。Classless XAML runtime test 僅在 test copy 移除 code-behind event attribute；正式 XAML handler 由 P1 test 精確驗證。Tests removed 0、skipped 0，`COVERAGE_WEAKENED=NO`。
- Evidence：Locked restore PASS；Release x64 build 0 warnings／0 errors；focused regression 17/17；full tests 1748/1748 PASS；實際 Release WPF UIA 驗證三個 window controls 3/3 enabled／keyboard-focusable／invokable，maximize／restore／resize PASS。
- Screenshot：`TCC_WPF_P1_GEOMETRY_SCREENSHOT_R1.png`，1672×941，SHA-256 `58F0369F7DF9F066E1E0F95FEC63F0E2CA170EFD06FF0EFC00107C3F80CF7FE6`。Repair Gate 為 PASS，但視覺最終判定仍須由 Supervisor 執行 R1 review。
- Scope：已通過的其他 geometry 不重設計；未進入 P2、Material、Frost、Blur、Scene artwork、Character、Typography calibration、Final icons 或完整資料綁定；未新增 dependency；未 commit、push 或 deploy。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_WPF_P1_HUMAN_GEOMETRY_REVIEW_R1`；不得直接進 P2。
- 取代：本決策取代 Decision096 的 `TCC_REFERENCE_MASTER_WPF_P1_HUMAN_GEOMETRY_REVIEW` current next step；Decision096 的 P1 architecture、legacy migration、accepted geometry 與 custody 邊界持續有效。

## TCC-DEC-2026-09-23-098 — Reference Master WPF P1 Geometry 正式接受與凍結

- 日期：2026-09-23；來源：Supervisor 正式人工幾何決策 `TCC_REFERENCE_MASTER_WPF_P1_HUMAN_GEOMETRY_REVIEW_R1=PASS`。
- 決策：`TCC_WPF_P1_GEOMETRY_SCREENSHOT_R1.png`（SHA-256 `58F0369F7DF9F066E1E0F95FEC63F0E2CA170EFD06FF0EFC00107C3F80CF7FE6`）經 Supervisor 與正式 Accepted Visual Baseline 比較後，P1 Geometry 正式成為 `ACCEPTED_BASELINE`；`TCC_REFERENCE_MASTER_WPF_P1=ACCEPTED`。
- Finding closure：HGR-001、HGR-002、HGR-003 均為 `VERIFIED_RESOLVED`。Window Chrome、App Top Bar、Side Navigation width／vertical rhythm、Command Header、Safety Core、Observation、BTC、Capital/Risk、Lower Modules、stepped right edge、Character/Scene interpenetration space、Scenic Reveal、major alignment 與 overall fidelity 共 15/15 PASS。
- Freeze boundary：不得再修改 P1 已接受幾何，除非後續實作發現真正的結構性阻擋問題。P2 只能在既有幾何上完成 Material、Visual Surfaces、Typography 與 Visual Component Language，不得重新設計 Geometry，也不得進入 P3 Character／Scene Integration。
- Non-blocking：`SCENE RESERVED`、`SCENIC REVEAL RESERVED`、工程輔助輪廓與 Structural Placeholder 是 P1 工程占位內容，不構成 Finding；依正式施工順序於後續階段移除／替換。像素級校準留待 WPF-P5，不重新開啟 P1 Geometry。
- Custody：本次只建立 acceptance record／Accepted Gate 並更新 Status／Decisions。Production、Tests、Accepted Visual、Accepted Specs 與 Authority 均未修改；Production／Tests digests 與 P1 Repair Gate 精確一致。
- Call accounting：MODEL_CALLS=0、VISION_API_CALLS=0、IMAGE_API_CALLS=0。未執行 build/test，因本次為 acceptance-only 且沒有 Production／Test byte change；P1 Repair Gate 的 1748/1748 PASS 工程證據持續有效。
- 下一步：唯一允許的 next step 是另行授權 `TCC_REFERENCE_MASTER_WPF_P2`；本決策不構成 P2 施工授權。
- 取代：本決策取代 Decision097 的 `TCC_REFERENCE_MASTER_WPF_P1_HUMAN_GEOMETRY_REVIEW_R1` next step 與 P1 human-review pending 狀態；Decision097 的 repair facts、runtime evidence、coverage 與 custody 保持有效。

## TCC-DEC-2026-09-23-099 — Reference Master WPF P2 Implementation Spec

- 日期：2026-09-23；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_WPF_P2_IMPLEMENTATION_SPEC`。
- 決策：P2 的 Material、Visual Surfaces、Typography 與 Visual Component Language 已轉為六份可直接施工的規格；本階段只建立規格與 Gate，不修改 Production、Tests 或 Accepted P1 Geometry。
- Candidate-token policy：所有精確 HEX、opacity、字型、字級、line-height、border、radius、icon size、risk-ring thickness 與 effect 值一律是 `IMPLEMENTATION_CANDIDATE`，不得宣稱為 `SOURCE-PROVEN TOKEN`；後續只能以 Accepted Visual screenshot calibration 集中調整。
- Resource ownership：下一階段採 trusted `Tcc.DesktopHost` ResourceDictionary，依 Brushes → Materials → Typography → Icons → Controls → ModuleStyles 合併；禁止 component-local magic color，且 Theme Package 不得改變 Core semantics。
- Surface language：Top Chrome、Navigation、Header Open、Standard、Elevated、Deep、Quiet、Selected、Hover、Warning、Critical、Disabled/Unavailable 均有不同角色；Safety、Observation、Market Anchor、Risk、Positions、Priorities、Mental、AI、Activity、Navigation、Top Bar 與 Command Header 不得退化為同一套 generic cards。
- Accessibility／performance：focus 必須獨立於 selected/hover，狀態不得只靠顏色，High Contrast 採 system resources，文字支援 100–200% scaling；repeated `BlurEffect`／`DropShadowEffect` 為 0，frost 優先使用 bounded border/gradient/texture。
- Geometry freeze：P2 不得改 Width、Height、Grid、Row、Column、Span、major Margin／alignment 或 accepted region topology；若視覺需求無法在現有 bounds 內完成，必須標記 `P2_GEOMETRY_CONFLICT` 並停止該修復。
- Evidence：Accepted Visual 1/1、Accepted Specs 9/9、Authority 2/2、P1 review artifact hash 均 PASS；六份 artifacts 6/6、JSON 3/3、required sections 16/16、surface families 12/12、typography roles 12/12、acceptance checklist 110 items。Locked restore PASS、Release x64 build 0 warnings／0 errors、full tests 1748/1748 PASS。
- Custody：Production 60-file digest 與 Tests 39-file digest前後一致；Geometry、Accepted Visual、Accepted Specs 與 Authority 未修改；MODEL/VISION/IMAGE calls 皆 0；未 commit、push 或 deploy。
- 下一步：唯一允許的 next step 是另行授權 `TCC_REFERENCE_MASTER_WPF_P2_IMPLEMENTATION`；施工仍限 P2 視覺層，不得進 P3 Character／Scene Integration。
- 取代：本決策取代 Decision098 的 `TCC_REFERENCE_MASTER_WPF_P2` generic next step，將其具體化為先完成的 P2 Spec Gate 與後續需另行授權的 `TCC_REFERENCE_MASTER_WPF_P2_IMPLEMENTATION`；Decision098 的 Geometry freeze 持續有效。

## TCC-DEC-2026-09-23-100 — Reference Master WPF P2 Implementation

- 日期：2026-09-23；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_WPF_P2_IMPLEMENTATION`。
- 決策：P2 Material、Visual Surfaces、Typography 與 Visual Component Language 已在 Accepted P1 Geometry 上完成；狀態為 `READY_FOR_HUMAN_VISUAL_REVIEW`。不得因本次 Codex self-review PASS 自動進入 P3。
- Resource architecture：`MainWindow` 依 Brushes → Materials → Typography → Icons → Controls → ModuleStyles 合併六個集中式 ResourceDictionary。Exact visual values 仍為 `IMPLEMENTATION_CANDIDATE`，不升格為 source-proven tokens；MainWindow 不散落 inline HEX。
- Visual language：實作冷灰藍、低飽和、低成本 Frost hierarchy；Observation/BTC 為 primary workspace，Risk 為 elevated analysis，Safety 有獨立 tile treatment，lower modules 使用 Standard/Quiet/Deep variants。Navigation 有 default/hover/pressed/selected/focus/disabled；Cyan 僅限小型 selected/focus/data emphasis。`BlurEffect=0`、`DropShadowEffect=0`。
- Typography/accessibility：13 組 roles 各有集中式 FontSize 與 LineHeight，Windows text size 100–200% 同步縮放；大字級保留 P1 比例並透過既有 root ScrollViewer 提供可捲動畫布。High Contrast 以 `SystemColors` 覆寫候選 brushes，狀態仍由文字與圖示共同表達。
- Geometry custody：五欄／五列比例、major spans、module bounds、Safety/Risk/AI-Activity 三段右緣、Character/Scene reservation 均保留；`Region.CharacterReserved` 為 transparent/empty/noninteractive。`P2_GEOMETRY_CONFLICT=NONE`。
- Evidence：Accepted Visual 1/1、Accepted Specs 9/9、P2 specs 6/6、P1 screenshot 1/1 hashes MATCH；VI-001～VI-012 為 12/12 PASS。真實 WPF 1672×941 screenshot SHA-256 `0069714833216D4D36836E8E5752CC15078598A1AF779E10A942825DA7D83689`；100/125/150/152/175/200 runtime text-scale matrix PASS。
- Tests/gates：baseline 1748、added 8、removed 0、after 1756；focused 26/26、full 1756/1756 PASS，failed/skipped 0；locked restore PASS；Release x64 build 0 warnings／0 errors；`COVERAGE_WEAKENED=NO`。
- Scope：未加入人物、雪景、紅梅、建築、燈籠或其他 P3/P4/P5 內容；`IMAGE_API_CALLS=0`；未新增 dependency；未 commit、push 或 deploy。既有 dirty/untracked baseline 原樣保留。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW`；Human review status `PENDING`，通過前不得進 P3。
- 取代：本決策取代 Decision099 的 `TCC_REFERENCE_MASTER_WPF_P2_IMPLEMENTATION` next step；Decision098/099 的 Geometry freeze、Authority custody 與 candidate-token policy 持續有效。

## TCC-DEC-2026-09-23-101 — Reference Master WPF P2 Human Visual Repair R1

- 日期：2026-09-23；來源：Supervisor 正式決策 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW=REVISE`，並追加 `TCC_REFERENCE_MASTER_WPF_P1_RETROSPECTIVE_VISUAL_AUDIT`。
- P1 回溯裁決：Accepted Visual、P1 R1 與 P2 pre-repair runtime 經 16 項重新比對；`NEW_P1_HIGH=0`、Medium/Low 亦為 0，沒有 geometry drift 或新的非重複 invariant。P1 Geometry 仍為 `ACCEPTED_BASELINE`，未重開、未修改。
- P2 修復裁決：P2-HVR-001–006 全數 `RESOLVED`。PanelTitle 改為 14/19 Medium 且 NoWrap；新增 Workspace Primary/Secondary、Safety、Auxiliary、Action 等 scene-compatible gradient surface roles；Navigation selected 改為細 edge + 輕 lift、inactive 提高可讀性；Safety/Risk/Mental footprint 放大；AI/Activity 與 Command Header finish 完成。
- Visual Invariants：重新獨立審核 VI-001–012，並加入 Supervisor 指定 VI-013–016；R1 實際 runtime 證據為 16/16 PASS。主要證據是 1672×941 `TCC_WPF_P2_SCREENSHOT_R1.png`，SHA-256 `114F049E5ED3D3D913A971B7DFA9CB47616E45CEABF90BBE52360AB60A40D278`；152% 展開畫布證據 SHA-256 `D00559D486EA6B3111596A2C5DD4AE7FC0657251C87D9204296EDD7501709B94`。
- 測試相容性：舊 B3 test 的「只允許單一 gradient」假設改為直接禁止任何非 `Tcc.ReferenceMaster.*`、`Tcc.Home.*` 或 B3 gradient authority leakage；P2 對新增 semantic gradients 另有正向 parsed-XAML guards。Focused 15/15、locked restore PASS、Release x64 0 warnings/0 errors、full 1760/1760 PASS，coverage 未弱化。
- Scope：Accepted Specs 9/9 與 authority images 2/2 hashes 不變；Production 66 files digest `25F100D2EC6E8AE2692D2CDA553C25E9F416792B2B26C9ECE0AABCE6A7FC8537`；Tests 40 files digest `B1D74AAF134F8C0C3B6F88A0A693259C3DD07E46DE41765394FB70146F9269FC`。未加入 P3 scene/character/plum/building/lantern，未 commit／push／deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P2_VISUAL_REPAIR=READY_FOR_HUMAN_REVIEW`；下一步固定為 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R1`，不得直接進 P3。
- 取代：Decision100 的 `READY_FOR_HUMAN_VISUAL_REVIEW / PENDING` 施工狀態與其 12-invariant 初版自評；Decision100 的集中式資源與 P1 custody 成果、Decision099 的 P2 spec authority、Decision098 的 P1 Accepted Geometry 與 Decision094 的 Accepted Visual authority 繼續有效。

## TCC-DEC-2026-09-23-102 — Reference Master WPF P1/P2 Visual Repair R2

- 日期：2026-09-23；來源：Supervisor 正式裁決 `P1_RETROSPECTIVE_VISUAL_AUDIT=REVISE`、`P2_HUMAN_VISUAL_REVIEW_R1=REVISE`，並授權 bounded surgical repair。
- P1 修復：`P1-RVA-HIGH-001` 與 `P1-RVA-MEDIUM-001` 已 `RESOLVED`。Command Header 採 Page Title／Identity／internal breathing／Quote／Action／right-scene breathing 六段 anchor topology；Top Bar 重新分配 Brand、status、utility、Search 與 window controls，使 Search 維持次要比例並消除內部擁擠。Safety/Main/Lower major geometry、stepped right edge 與 scene reveal 未重開或漂移。
- P2 精修：Safety icons 改用 semantic semi-fill 並提高 stroke weight；PageTitle 改為 36/42 Medium；Identity plaque 增加 frost fragments 與 seal weight；Action 使用獨立 inner-frost bordered finish。沒有擴張至其他 P2 surface family。
- Visual Invariants：保留 VI-001–016 並新增 VI-017–019；R2 結果 19/19 PASS。實際 UIA：Identity 33.91–42.52%、Quote start 62.80%、Action 80.74–87.20%、Action 後 scene breathing 12.80%、Search x 70.99%／width 13.82%；11/11 required elements present and actionable。
- Runtime evidence：`TCC_WPF_P2_SCREENSHOT_R2.png` 為實際 1672×941 WPF runtime，SHA-256 `0D3218AAE64C1ABA2580D181203BA794ECB865E9A92CE73D50371E6D35BDDDDC`。初次 capture 發現 Brand 截斷並於 final capture 前修正；Windows Text Size 已還原 152%。
- Tests/gates：focused 25/25、locked restore PASS、Release x64 build 0 warnings／0 errors、full tests 1763/1763 PASS；Accepted Specs 9/9 與 authority images 2/2 SHA-256 MATCH；dependency mutations 0、P3 asset markers 0、staged 0。
- Scope：未加入 Character、Snow、Plum、Architecture、Lantern 或其他 P3 art；未 image generation、commit、push 或 deploy。既有無關 dirty/untracked baseline 保留。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P2_VISUAL_REPAIR_R2=READY_FOR_HUMAN_VISUAL_REVIEW_R2`；下一步固定為 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R2`，通過前不得進 P3。
- 取代：Decision101 的 R1 human-review next step；Decision101 的已完成修復事實、Decision100 的集中式資源與 P1 custody、Decision099 的 P2 spec authority、Decision098 的 Accepted Geometry 與 Decision094 的 Accepted Visual authority 繼續有效。

## TCC-DEC-2026-09-23-103 — Reference Master WPF P2 Visual Repair R3

- 日期：2026-09-23；來源：Supervisor 正式裁決 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R2=REVISE`，並確認 `P1_RETROSPECTIVE_VISUAL_AUDIT=PASS`、`NEW_P1_HIGH=0`、`NEW_P1_MEDIUM=0`。
- P1 custody：P1 Geometry 維持 `ACCEPTED_BASELINE` 且本輪不得再修改。Command Header anchors、Top Bar distribution、stepped right edge、scene breathing、Observation、BTC、Lower bounds、Typography single-line、Identity plaque、PageTitle、Navigation rhythm 與 information density 均未回歸。
- P2 修復：`P2-R2-001`–`004` 全數 `RESOLVED`。Action 在既有 108×46 bounds 內改為更明亮的 ice-white/cyan frost、bounded luminous edge、dark readable label 與 restrained inner highlight；Safety icons 保持 42×42 並提高 semantic fill mass 與 2.2 stroke；Mental center icon 改為 meditation/calm-state vector；Risk/Mental rings 分別校準為 156×156 與 120×120。
- 視覺審查：Accepted Visual vs R3 實際 runtime 比較後，UI/UX Pro Max PASS、Taste audit-only PASS、Hallmark bounded audit `0 critical／0 major／0 minor`；沒有 neon、strong glow、HUD 化或 ordinary WPF regression。
- Runtime evidence：`TCC_WPF_P2_SCREENSHOT_R3.png` 為 HWND-bound `PrintWindow` 實際 1672×941 WPF runtime，SHA-256 `8E8D8BFC0077AD6E1F7D01C336A85F62F4C1A5CCB65F55B4479DBFDC03C7C505`；UIA 11/11 PASS；Windows Text Size 還原 152%。
- Tests/gates：focused 26/26、locked restore PASS、Release x64 build 0 warnings／0 errors、full tests 1764/1764 PASS；Accepted Specs 9/9 與 authority images 2/2 SHA-256 MATCH；dependency mutations 0、P3 content added NO、staged 0。
- Scope：未加入 Character、Snow、Plum、Architecture、Lantern 或完整 scene integration；未 image generation、commit、push 或 deploy。既有無關 dirty/untracked baseline 保留。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P2_VISUAL_REPAIR_R3=READY_FOR_HUMAN_VISUAL_REVIEW_R3`；下一步固定為 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R3`，通過前不得進 P3。
- 取代：Decision102 的 R2 human-review next step；Decision102 的已完成修復事實、Decision100 的集中式資源與 P1 custody、Decision099 的 P2 spec authority、Decision098 的 Accepted Geometry 與 Decision094 的 Accepted Visual authority繼續有效。

## TCC-DEC-2026-09-23-104 — Reference Master WPF P2 Visual Repair R4

- 日期：2026-09-23；來源：Supervisor 正式裁決 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R3=REVISE`，並追加 `P2-R3-003 Identity Plaque Red Seal`；P1 retrospective 保持 PASS。
- P1 custody：Header 六段 column topology、Action 起點與 Y、Identity Plaque anchor/geometry、Mental module/ring center、Top Bar、Safety、Observation、BTC、Lower modules、Navigation、stepped right edge、scene reservations 與 information density 均未修改。
- P2 修復：`P2-R3-001`–`003` 全數 `RESOLVED`。Action 由 108×46 校準為 168×46 DIP，採 22 DIP horizontal padding 與 16 DIP text-arrow gap，起點仍為 x=80.74%；Mental symbol 維持 46×46／120 ring bounds，改為 filled seated meditation silhouette；Identity Seal 維持 18×18 與既有 12 DIP gap，改為 restrained red fill、方印 silhouette 與 internal seal-cut geometry。
- 視覺不變條件：新增 VI-020、VI-021 並 PASS；VI-001–019 全數保留，合計 21/21 PASS。Identity Plaque geometry 另由 parsed-XAML test 與 runtime screenshot 確認未漂移。
- 視覺審查：Accepted Visual vs R4 actual runtime 比較後，UI/UX Pro Max PASS、Taste audit-only PASS、Hallmark bounded audit `0 critical／0 major／0 minor`；沒有 neon、glow、generic UI icon、thin yoga glyph 或 ordinary narrow-button regression。
- Runtime evidence：`TCC_WPF_P2_SCREENSHOT_R4.png` 為 HWND-bound `PrintWindow` 實際 1672×941 WPF runtime，SHA-256 `4CA7CA50C805D2E64AEEF32E2BC22DC87F3C48207B176974CBBC47045EFF1DDB`；UIA 11/11 PASS；Action bounds `[1350,96,168,46]`，右側 scene breathing 9.21%；Windows Text Size 還原 152%。
- Tests/gates：focused 27/27、locked restore PASS、Release x64 build 0 warnings／0 errors、full tests 1765/1765 PASS；Accepted Specs 9/9 與 authority images 2/2 SHA-256 MATCH；dependency mutations 0、P3 content added NO、staged 0。
- Scope：未修改 Accepted Visual／Specs／Authority，未加入 Character、Snow、Plum、Architecture、Lantern 或 scene integration；未 image generation、commit、push 或 deploy。既有無關 dirty/untracked baseline 保留。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P2_VISUAL_REPAIR_R4=READY_FOR_HUMAN_VISUAL_REVIEW_R4`；下一步固定為 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R4`，通過前不得進 P3。
- 取代：Decision103 的 R3 human-review next step；Decision103 的已完成修復事實、Decision100 的集中式資源與 P1 custody、Decision099 的 P2 spec authority、Decision098 的 Accepted Geometry 與 Decision094 的 Accepted Visual authority繼續有效。

## TCC-DEC-2026-09-23-105 — Reference Master WPF P2 Visual Repair R5

- 日期：2026-09-23；來源：Supervisor 正式裁決 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R4=REVISE`，只授權 `P2-R4-001` 與 `P2-R4-002`；P1 Geometry 保持 PASS / LOCKED。
- Runtime-bounds 決策：重要視覺元件以實際渲染為最終證據。R4 Action 雖宣告 168 DIP，實際 frost surface 受 109.09 px 單欄 allocation 裁切；R5 不移動 x/y anchor、不改六欄 topology，只設定 `Grid.ColumnSpan=2` 並以 200×46 DIP 覆蓋既有 Action + scene-breathing allocation。UIA/DPI 實測 `[1350,96,200,46]`、parent allocation 305.45 px、Header 內剩餘 106 px，VI-022 PASS。
- Seal 決策：Identity Plaque 的 18×18 bounds、12 DIP gap 與 anchor/geometry 不變；只把內部 mark 改為專用朱紅 token、微不規則實心印面與 EvenOdd 負形篆刻 channels，移除 outlined badge language，無 glow/neon。
- Preservation：Mental State 列為 P5 calibration candidate，本輪未修改。Top Bar、Header anchors、Safety、Observation、BTC、Capital/Risk、lower modules、navigation、stepped right edge、typography、information density 與 scene reservation 全部保持。
- Runtime/evidence：`TCC_WPF_P2_SCREENSHOT_R5.png` 為 1672×941 HWND-bound PrintWindow，SHA-256 `99E579F88E13824D8DCBC26D52BECE17872DA0AE637D2CCD6FAC6E38090DCE4E`；UIA 8/8；Windows Text Size 恢復 152%。
- Gates：focused 27/27、locked restore PASS、Release x64 build 0 warnings/0 errors、full tests 1765/1765 PASS；Accepted Specs 9/9、Authority 2/2、P1 Accepted Screenshot hash MATCH；dependency mutations 0、P3 content NO、staged 0。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P2_VISUAL_REPAIR_R5=READY_FOR_HUMAN_VISUAL_REVIEW_R5`；下一步固定 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R5`；P3 仍禁止。
- 取代：Decision104 的 R4 human-review next step；Decision104 的既有修復事實、Decision098 的 P1 Accepted Geometry 與 Decision094 的 Accepted Visual authority繼續有效。

## TCC-DEC-2026-09-23-106 — Reference Master WPF P2 R5 正式接受與凍結

- 日期：2026-09-23；來源：Supervisor 正式人工視覺決策 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R5=PASS`。
- 決策：`TCC_WPF_P2_SCREENSHOT_R5.png`（SHA-256 `99E579F88E13824D8DCBC26D52BECE17872DA0AE637D2CCD6FAC6E38090DCE4E`）正式成為 P2 `ACCEPTED_BASELINE`；`TCC_REFERENCE_MASTER_WPF_P2_ACCEPTANCE=PASS`、`TCC_REFERENCE_MASTER_WPF_P2=ACCEPTED`。
- Acceptance scope：P1 Geometry、Header anchor topology、Top Bar distribution、stepped right edge、scene interpenetration、navigation、Safety、material direction、surface／typography hierarchy、information density、Risk Ring、Mental State、Primary Action visual/runtime footprint、Identity Red Seal、anti-SaaS 與 anti-esports 全部 PASS；previously accepted regression 為 NO。
- Finding closure：`P2_R4_001=VERIFIED_RESOLVED`、`P2_R4_002=VERIFIED_RESOLVED`、`VI_022=VERIFIED_PASS`；R5 Repair Gate PASS，Visual Invariants VI-001～VI-022 為 22/22 PASS。
- Freeze boundary：R5 screenshot 與 P2 accepted visual state 凍結。不得以 P5 calibration candidates 重開 P2；不得破壞 stepped right edge、no character card、scene interpenetration、surface hierarchy、cyan/frost restraint、anti-SaaS／anti-esports、runtime visual bounds、action footprint fidelity 或 semantic icon mass fidelity。
- P5 candidates：Primary Action frost character、Mental State meditation symbol weight、panel transparency／glass depth 只在 P3 scene 存在後由 P5 校準；三者均不阻擋 P2 Acceptance。
- Custody：本次只建立 Acceptance 文件／Accepted Gate 並更新 Status／Decisions。Production、Tests、P1 Geometry、P2 Styles、R5 screenshot、Accepted Visual、Accepted Specs 與 Authority 均未修改；MODEL_CALLS=0、VISION_API_CALLS=0、IMAGE_API_CALLS=0；未 commit、push 或 deploy。
- 下一步：唯一允許的 next step 是另行授權 `TCC_REFERENCE_MASTER_WPF_P3_IMPLEMENTATION_PREP`。本決策只提供合法前置條件，不授權開始 P3 implementation。
- 取代：本決策取代 Decision105 的 `TCC_REFERENCE_MASTER_WPF_P2_HUMAN_VISUAL_REVIEW_R5` pending next step；Decision105 的 R5 repair evidence、Decision098 的 P1 Accepted Geometry 與 Decision094 的 Accepted Visual authority 繼續有效。

## TCC-DEC-2026-09-23-107 — Reference Master WPF P3 Implementation Prep

- 日期：2026-09-23；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_WPF_P3_IMPLEMENTATION_PREP`。
- 決策：Character + Scene + Environmental Integration 已建立 13 份可直接施工的 P3 planning artifacts；本階段狀態為 `PASS`，但只授權 specification/preparation，沒有開始 P3 Production implementation。
- Authority chain：Accepted Questionnaire／Constitution／UX／System／Theme／Reference Master specs、Accepted Visual、P1 Accepted Geometry、P2 Accepted Visual Language 與 Character Identity Authority 依既定優先序共同約束；P1/P2 不得因場景需求而漂移。
- Layering：L0–L6 為視覺語意，不機械等同 WPF ZIndex。實作規格以 fallback、far、mid、rear atmosphere、behind-UI decoration、accepted UI、character、foreground、interaction planes 建立明確排序，並以 Interpenetration Map 定義 protected/allowed overlap。
- Character integration：只使用既有透明 Character Identity Authority，不重新生成角色；以 normalized footprint、face/hair clearance、scene contact、moonlight、rim、mist、white-fabric clipping 與 alpha-edge rules 控制整合品質。
- Asset decision：未來背景資產明確標示 `IMAGE_GENERATION_REQUIRED=YES`；現有 baked/composite scene assets 不符合可分層、可縮放、可保護 UI 的 P3 需求，只能作 reference 或 rejected direct-use evidence。本輪沒有建立或修改實際人物／場景資產，`IMAGE_API_CALLS=0`。
- Glass compatibility：P3 Prep 未發現阻擋 Accepted P2 glass language 的結構性 blocker；場景亮度、mist、snow、plum、architecture 與 character edge 都必須接受 P2 readability/contrast gate，不能反向重開 P2。
- Quality gates：P3-VI-001–032、decoded raster target 56 MiB／hard stop 72 MiB、1672×941／1920×1080／2560×1440、125/150% DPI 與 HWND runtime + human visual validation plan 均已定義。Critical UI occlusion 要求固定為 0。
- Custody：P1 Geometry、P2 Visual Language、Production、Tests、Accepted Visual／Specs／Authority 未因本 Prep 任務修改；依明確授權不重跑 Build 或完整 Tests；未 commit、push 或 deploy。
- 下一步：唯一允許的 next step 是另行授權 `TCC_REFERENCE_MASTER_WPF_P3_ASSET_PREPARATION`；該步仍不得直接修改 Production WPF，除非後續另有明確 implementation authorization。
- 取代：本決策取代 Decision106 的 P3 Prep pending next step；Decision106 的 P2 accepted freeze、Decision098 的 P1 accepted geometry 與 Decision094 的 Accepted Visual authority 繼續有效。

## TCC-DEC-2026-09-23-108 — Reference Master WPF P3 Asset Preparation Candidates

- 日期：2026-09-23；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_WPF_P3_ASSET_PREPARATION`，並明確允許 Image API，但禁止 Production WPF／Tests 修改與 P3-A integration。
- 決策：以 Accepted Visual 作唯一 Scene Composition Authority，採 reference-preserving edit 移除 UI／文字／人物並補景；正式 staging 保留兩張 Background 與兩張 RGBA Foreground Overlay 候選，不採文字從零重新設計世界。
- 內部篩選：Background 01 因新增 central-right cliff/waterfall focal mass 淘汰；Background 02 經單點移除 right-side roof/head tangency 後進入 Human Review。Overlay 01 因底部雪堆與粒子過重淘汰；Overlay 02 以 1.2077% high-chroma red、0.3568% warm coverage、black/white/scene alpha QA 進入 Human Review。
- Character custody：`TCC_P3_CHARACTER_WORKING_SOURCE.png` 是 Character Identity Authority 的 byte-identical working copy，SHA-256 同為 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`；人物生成、臉／髮／服裝修改、裁切與 resize 均為 0。
- Layer decision：Foreground Overlay `REQUIRED=YES`，因 Accepted Visual 的四向梅枝與右下燈籠承擔 Scene/UI interpenetration；Character Lighting Reference `NOT_REQUIRED`，後續光線整合只能用 non-destructive masks/grade，不能取代 Identity Authority。
- Gate：Accepted Visual／Character hashes valid；Manifest valid；32 項 P3 invariants + 10 項 Asset invariants 為 42/42 `PASS_AT_ASSET_SCOPE`；Human-only acceptance 保持 `PENDING`；所有 assets `accepted=false`。
- Validation/custody：Manifest 7/7 hashes/dimensions/alpha、JSON 4/4、Frozen Specs 9/9、Authority 2/2 與 Accepted Visual 1/1 PASS；locked restore PASS；Release x64 build 0 warnings／0 errors；full tests 1765/1765 PASS。`IMAGE_API_CALLS=5`；Production／Tests 無本任務新增修改；沒有 runtime integration、commit、push 或 deploy。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_WPF_P3_HUMAN_ASSET_REVIEW`。不得直接進入 P3-A 或 WPF Integration。
- 取代：本決策取代 Decision107 的 Asset Preparation pending next step；Decision107 的 P3 planning authority、Decision106 的 P2 freeze、Decision098 的 P1 Geometry 與 Decision094 的 Accepted Visual authority 繼續有效。

## TCC-DEC-2026-09-23-109 — Reference Master WPF P3 Asset Repair R1 Depth Separation

- 日期：2026-09-23；來源：Supervisor 正式裁決 `TCC_REFERENCE_MASTER_WPF_P3_HUMAN_ASSET_REVIEW=REVISE`，並授權限定於既有 Overlay 02 的 deterministic asset repair。
- Human Review 裁決：Background 01 REJECT；Background 02 PASS/FROZEN；Character Working Source PASS/FROZEN；Overlay 01 REJECT；Overlay 02 REVISE/SOURCE ONLY。Background 02、Character Working Source／Authority 與 Overlay 02 source hashes 均保持不變。
- Depth repair：Overlay 02 的 433 個完整 8-connected alpha components 依 scene depth 分配為 `TCC_P3_SCENE_DECOR_BEHIND_CHARACTER_V1.png`（planned z=40）與 `TCC_P3_SCENE_FOREGROUND_ACCENT_V1.png`（planned z=80）；split components 0、layer alpha overlap 0、source alpha union exact、recomposition RGB max delta 0。禁止用矩形切線截斷粒子或枝葉。
- Lantern decision：燈籠與相鄰右下梅枝／雪石在 Accepted Visual 中共享 foreground relationship，因此不建立第三張 lantern-only layer；此決策避免不必要 fragmentation，仍保留 WPF 對 character-front／character-behind 環境元素的獨立排序能力。
- Plum grade：只對 Overlay 02 既有紅色像素作 bounded、feathered、non-destructive environment grade；平均 saturation 157.067→150.927、value 108.557→103.061、luma 56.839→56.012，並略增冷灰藍污染；未重新生成或重畫枝／花／燈籠。
- Gate：P3-AI-011–013 新增並 PASS；32 項 P3 + 13 項 Asset Invariants 為 45/45 `PASS_AT_REPAIR_ASSET_SCOPE`。Manifest 14/14、A/B/C composites、alpha QA、Frozen Specs 9/9、Authority 2/2、Accepted Visual 1/1、locked restore、Release x64 0 warnings／0 errors與 full tests 1765/1765 均 PASS。
- Custody：`IMAGE_API_CALLS=0`；Production 66 files 與 Tests 40 files 的 start/final digests 分別一致；未開始 runtime integration、未修改 Production WPF／Tests、未 commit／push／deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_ASSET_REPAIR_R1=READY_FOR_HUMAN_ASSET_REVIEW_R1`；`P3_A_ALLOWED=NO`；唯一下一步是 `TCC_REFERENCE_MASTER_WPF_P3_HUMAN_ASSET_REVIEW_R1`。
- 取代：本決策取代 Decision108 的初次 Human Asset Review pending 狀態與 Overlay 02 candidate disposition；Decision108 的 Preparation provenance、Decision107 的 P3 planning authority、Decision106 的 P2 freeze、Decision098 的 P1 Geometry 與 Decision094 的 Accepted Visual authority 繼續有效。

## TCC-DEC-2026-09-23-110 — P3 Runtime Character Environmental Integration Invariants

- 日期：2026-09-23；來源：使用者明確追加正式規則 `P3-VI-033`～`P3-VI-037`，其權威高於先前 P3 preparation assumptions。
- 新增契約：P3 Visual Invariants 由 32 擴充為 37。033 禁止 transparent PNG pasted-on appearance 及其六類症狀；034 要求人物與 Scene 共享冷環境光、月光方向、局部暖污染、陰影語言與 haze；035 禁止完整乾淨 360-degree alpha silhouette；036 要求白毛／白衣不比 Accepted Visual 更亮或更純白並接受 cold blue-gray contamination；037 要求臉部保留身份但消除棚拍貼圖感。
- Stage routing：五項規則屬 final integrated Runtime acceptance，正式併入 P3-C Character/Scene Integration 與 P3-F Human Visual Calibration；P3-B placement-only screenshot 只是中間狀態，不可作最終 PASS 證據。
- Evidence：必須使用 HWND-bound Release Runtime pixels、face/fur/hair crops、silhouette/occlusion overlay、Accepted Visual 與 Character Identity Authority side-by-side review；source inspection、clean PNG、asset board、XAML declaration 或 UIA bounds 均不可單獨通過。
- Current status：`P3-VI-033`～`037=PENDING_RUNTIME_VALIDATION`。尚未開始 P3 Runtime，因此不得提前宣告 PASS；未修改或重開既有 R1 Asset Repair Gate 的 45/45 repair-asset-scope 歷史結論。
- Custody：本決策只更新 P3 Visual Invariants、Character Integration Rules、Runtime Visual Validation Plan、Implementation Plan、Status 與 Decisions。Production、Tests、Assets、Accepted Visual／Specs／Authority 均未修改；`IMAGE_API_CALLS=0`；未 commit／push／deploy。
- 下一步：仍為 `TCC_REFERENCE_MASTER_WPF_P3_HUMAN_ASSET_REVIEW_R1`；P3-A 未授權且不得開始。
- 取代：本決策擴充 Decision107 中 P3 Visual Invariants 32 的總數與 Character integration acceptance；Decision109 的 R1 asset repair evidence、Decision108 的 asset provenance、Decision106 的 P2 freeze、Decision098 的 P1 Geometry 與 Decision094 的 Accepted Visual authority 繼續有效。

## TCC-DEC-2026-09-23-111 — P3 Human Asset Review R1 Acceptance

- 日期：2026-09-23；來源：正式授權 `TCC_REFERENCE_MASTER_WPF_P3_HUMAN_ASSET_REVIEW_R1`，並明確分離 Asset suitability 與 final Runtime integration。
- Human Asset Review：Background 02、Character Working Source、Decor Behind Character 與 Foreground Accent 全部 `ACCEPTED/FROZEN`。Background 的 moon／mountain／architecture／horizon／density／winter-night palette／Scenic Reveal compatibility 無新重大問題；Character 與 Authority byte-identical；兩張 overlay alpha、edge、branch continuity 與獨立 WPF control 均符合。
- Depth decision：`Background → Decor Behind Character → Character → Foreground Accent` 可獨立排序；`DEPTH_LAYER_CAPABILITY=PASS`、`CHARACTER_Z_ORDER_CAPABILITY=PASS`。此結論只證明 capability，不等於 final visual integration。
- Runtime separation：`CHARACTER_SCENE_INTEGRATION=NOT_STARTED`、`CHARACTER_CUTOUT_APPEARANCE=CURRENTLY_PRESENT`、`PLACEMENT_ONLY_ACCEPTANCE=FORBIDDEN`。P3-VI-033～037 全部保持 `PENDING_RUNTIME_VALIDATION`，只允許在 P3-C／P3-F 由 integrated HWND Release Runtime 正式驗證。
- Deferred calibration：上方梅枝 footprint、左下延伸、紅梅亮／淨／飽和與 Character Authority 偏亮列為 Runtime calibration candidates，不構成 Asset blocker。
- Evidence：Manifest 14/14 hashes/dimensions PASS；Character byte-identical；alpha components 433、split 0、overlap 0、union exact；A/B/C 與 black/white alpha QA PASS；R1 Repair Gate SHA-256 `783BE80555A83466A6623EE74DB77300E5205BA9C094812ED8C40C78418481D3` 且未修改。
- Custody：只新增 Asset Acceptance、Accepted Gate、Status、Decision111 與分離的 SKILL checkpoint。Production、Tests、accepted binary assets 與歷史 R1 Gate 均未修改；依授權未 Build／run full tests；MODEL／VISION／IMAGE API calls 0；未 commit／push／deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_HUMAN_ASSET_REVIEW_R1=PASS`、`ASSET_ACCEPTED=YES`、`P3_A_ALLOWED=YES`。
- 下一步：`TCC_REFERENCE_MASTER_WPF_P3_A_BACKGROUND_SCENE_INTEGRATION`，只允許 Background Scene Integration；Character Placement 屬 P3-B，Character/Scene Integration 屬 P3-C。
- 取代：本決策取代 Decision109 的 pending Human Asset Review R1 與 `P3_A_ALLOWED=NO` 狀態；Decision110 的 Runtime-only invariants、Decision109 的 repair evidence、Decision108 的 asset provenance、Decision106 的 P2 freeze、Decision098 的 P1 Geometry 與 Decision094 的 Accepted Visual authority繼續有效。

## TCC-DEC-2026-09-23-112 — P3-A Background Scene Integration

- 日期：2026-09-23；來源：正式授權 `TCC_REFERENCE_MASTER_WPF_P3_A_BACKGROUND_SCENE_INTEGRATION`，限定 Background Scene only。
- Asset custody：Accepted Background 02 以 byte-identical copy 部署為 `Assets/Scene/TCC_P3_SCENE_BACKGROUND_PLATE.png`，source/Production SHA-256 均為 `4384664A6016564F6C47E56FA554F6B10B1BA855499742ECFEF000267A4E2771`；accepted source 未修改。
- Runtime architecture：DesktopHost 擁有單一靜態非互動 Background `Image`，z=10、`UniformToFill`、HighQuality；UI z=60，並保留 z=40/70/80 空插槽供未來 Decor Behind/Character/Foreground。沒有新 dependency、ProjectReference、public API、Viewbox、Canvas、blur、shadow、animation 或 particle system。
- P1/P2 custody：五欄五列 geometry、major spans/margins、stepped right edge、Action footprint、P2 ResourceDictionary order、brushes/styles/typography 全數保持；只調整頂層 UI z-index 以騰出正式 Scene planes。
- Runtime evidence：1672×941 / DPI 96 / Text Size 100% HWND-bound PrintWindow screenshot SHA-256 `4415020422CD69D6AEE263940141B64CFBB7E373D276D8845C170F468C88A3F4`；UIA 8/8、0 offscreen；Text Size 已回復原值 152%。右側開放場景區與 accepted background 完全相同像素 98.433%，證明 Runtime 實際載入且無幾何變形。
- Visual result：Moon、Mountain、Architecture、Water/Ice Horizon、Scenic Reveal、Stepped Right Edge 與 UI Readability 全數 PASS。P3-A 無 Character 與 Foreground 屬預期狀態，不作失敗。
- Gates：focused 35/35、locked restore PASS、Release x64 0 warnings/0 errors、full 1770/1770 PASS、Frozen Specs 9/9、Authority 2/2、Accepted Visual/P1/P2/Background hashes 4/4 MATCH；failed checks 0。
- Character boundary：Character Working Source、Decor Behind Character 與 Foreground Accent 均未加入 Production/runtime；P3-VI-033～037 維持 `PENDING_RUNTIME_VALIDATION`。
- 狀態：`READY_FOR_HUMAN_VISUAL_REVIEW_A`；`P3_B_ALLOWED=NO`；下一步唯一為 `TCC_REFERENCE_MASTER_WPF_P3_A_HUMAN_VISUAL_REVIEW`。未 commit、tag、push、release 或 deploy；`IMAGE_API_CALLS=0`。
- 取代：本決策取代 Decision111 的 P3-A pending next step；Decision111 的 Asset Acceptance、Decision110 的 Runtime-only Character invariants、Decision109 的 repair evidence、Decision106 的 P2 freeze、Decision098 的 P1 Geometry 與 Decision094 的 Accepted Visual authority繼續有效。

## TCC-DEC-2026-09-23-113 — P3-A Glass / Scene Compatibility Repair R1 and Character Contract Hardening

- 日期：2026-09-23；來源：Supervisor 正式授權 `TCC_REFERENCE_MASTER_WPF_P3_A_GLASS_COMPATIBILITY_REPAIR_R1 + P3_CHARACTER_INTEGRATION_CONTRACT_HARDENING`，並明確禁止 P3-B/P3-C Runtime implementation。
- Human findings：P3-A V1 的 Background composition 保持 PASS，但 `P3A-HVR-001/002` 確認 Dense information surfaces 抑制不足且 Open/Low-density/Dense 差異不足。R1 以 centralized cold-dark brushes 與 existing style mapping 解決，沒有修改 Background pixels、P1 bounds、module positions、header anchors 或 P2 component hierarchy。
- Surface contract：正式建立 `Open Scenic Reveal > Low-density Glass > Dense Information Glass > Dense Inner Surface`。Safety Core 與 Capital/Risk Terrain 保留較高 Scene contact；Observation、BTC、Positions、Priorities、Mental State、AI Summary、Activity 與其 table/chart/inner surfaces 增加局部 attenuation。
- Runtime evidence：最新 Release apphost SHA-256 `DBB92113F6EB5317CAEF6ECCB4C4670EBE16189BD84ABD4003DED563CA9801C9`；1672×941 / DPI 96 / Text Size 100% HWND-bound screenshot SHA-256 `C946879C627D90707F4802FB7BB9365B24F39B7E8337717217EFDDB90AEB7132`；UIA 8/8、0 offscreen；Text Size 已恢復 152%，process 殘留 0。
- Pixel result：Open right 對 V1 MAE 0.0、99.999% exact Background；Safety/Risk 只小幅變暗；Observation luminance 41.609→31.762、BTC 32.618→27.531、Lower modules 49.763→37.233。P3A-HVR-001/002 `RESOLVED`，P3-VI-038/039/040 `PASS`，沒有全畫面 mask、黑牆或 Scene 消失。
- Character contract：P3-B 僅可驗收 placement/crop/scale/overlap/Z-order/clipping，Screenshot 只是 `PLACEMENT_EVIDENCE`；P3-C 才擁有 environmental grade、exposure、shared lighting、edge、silhouette breakup、scene contact、face/white-material integration。新增 P3-VI-041 `No Detectable Character Cutout`，總數 41；P3-VI-033～037/041 全部維持 `PENDING_RUNTIME_VALIDATION`。
- Escalation：若 bounded WPF grading/masks/shadow/atmosphere/occlusion 仍無法消除 cutout，必須設定 `CHARACTER_DERIVED_INTEGRATION_ASSET_REQUIRED=YES` 並停止，另請 Supervisor 授權；不得以堆疊廉價 effects 假裝 PASS，且任何 derived asset 不得改 identity。
- Gates：focused 33/33、locked restore PASS、Release 0 warnings/0 errors、full 1771/1771、Frozen Specs 9/9、Authority 2/2、Background source/Production hash match；failed checks 0；`IMAGE_API_CALLS=0`。
- Custody：Character、Decor Behind Character、Foreground Accent 均未進 Production/runtime；未 commit、tag、push、release 或 deploy；`P3_B_ALLOWED=NO`、`P3_C_ALLOWED=NO`。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_WPF_P3_A_HUMAN_VISUAL_REVIEW_R1`。
- 取代：本決策取代 Decision112 的初次 P3-A Human Review pending 狀態；Decision112 的 byte-identical Background integration、Decision111 的 Asset Acceptance、Decision110 的 Runtime-only Character rules、Decision106 的 P2 freeze、Decision098 的 P1 Geometry 與 Decision094 的 Accepted Visual authority繼續有效。

## TCC-DEC-2026-09-24-114 — P3-A Glass / Scene Compatibility Repair R2

- 日期：2026-09-24；來源：Supervisor Human Visual Review R1 正式裁決 `TCC_REFERENCE_MASTER_WPF_P3_A_HUMAN_VISUAL_REVIEW_R1=REVISE`，只授權 Observation table body 與 Positions／Priorities／Mental content interiors 的 R2 局部修復。
- Retained-PASS custody：Header、Safety Core、BTC chart、Capital/Risk Terrain、AI Summary、Activity、Open Scenic Reveal 與 Stepped Right Edge 不得修改。R2 Runtime 量測確認 Header／Safety／Observation tabs-header／BTC／Risk／AI／Activity／Open Right 對 R1 均為 100% pixel-identical。
- Local repair：Observation 只加深 `DenseRow`；Positions empty-state、Priorities five-row body、Mental centered body 各加入獨立 nested translucent blue-black content surface。Priorities 採最高 local attenuation，Mental 保持比 BTC 更輕。未改 outer glass、module bounds、row geometry、padding、typography或全域 panel brush。
- Pixel result：Observation rows luma 29.620→26.104；Positions 32.337→26.844；Priorities 34.401→26.213；Mental 39.480→34.830。Open Right 對 R1 與 Background 均 100% exact，無 opaque black wall、global mask 或 Scene erasure。
- Runtime evidence：1672×941／DPI 96／Text Size 100% HWND-bound screenshot SHA-256 `3D7A9032C1DD8612103FFD056313FBEA2B8FF226EE00CA3870134BE2FCAA2200`；UIA 8/8、offscreen 0；Text Size 恢復 152%；process 殘留 0。
- Gates：focused P1/P2/P3 34/34、locked restore PASS、Release 0 warnings／0 errors、full 1772/1772、Frozen Specs 9/9、Authority 2/2、Background source/Production hash match；failed checks 0；`IMAGE_API_CALLS=0`。
- Scope：P3-VI-038/039/040 為 PASS；P3-VI-033～037/041 仍為 `PENDING_RUNTIME_VALIDATION`。Character、Decor Behind Character、Foreground Accent 未加入；`P3_B_ALLOWED=NO`、`P3_C_ALLOWED=NO`；未 commit、tag、push、release 或 deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_A_GLASS_COMPATIBILITY_REPAIR_R2=READY_FOR_HUMAN_VISUAL_REVIEW_A_R2`；下一步唯一為 `TCC_REFERENCE_MASTER_WPF_P3_A_HUMAN_VISUAL_REVIEW_R2`。
- 取代：本決策取代 Decision113 的 R1 Human Review pending 狀態；Decision113 的 Character contract hardening、Decision112 的 Background integration、Decision111 的 Asset Acceptance、Decision110 的 Runtime-only Character rules與既有 P1/P2 frozen authority 繼續有效。

## TCC-DEC-2026-09-24-115 — P3-A R2 Human Visual Acceptance

- 日期：2026-09-24；來源：Supervisor 正式裁決 `TCC_REFERENCE_MASTER_WPF_P3_A_HUMAN_VISUAL_REVIEW_R2=PASS`，本階段限 Audit／Acceptance 落盤，不得重新施工。
- Acceptance：R2 Runtime screenshot SHA-256 `3D7A9032C1DD8612103FFD056313FBEA2B8FF226EE00CA3870134BE2FCAA2200` 驗證吻合；R2 Repair Gate SHA-256 `D28595644008F28E911AE953318F8878B6DC5D7BC767BE6C45A4BF8D342018A6` 且 failed checks 0。P3A-HVR-001/002 `VERIFIED_RESOLVED`；P3-VI-038～040、Surface/Scene hierarchy、UI readability、Scenic Reveal、Background/anchors、P1/P2 custody全部 PASS。
- Frozen baseline：`TCC_REFERENCE_MASTER_WPF_P3_A=ACCEPTED`、`P3_A_STATUS=ACCEPTED_BASELINE`。Background integration、scaling/crop、Scene coordinate relationship、Glass/Scene hierarchy、Scenic Reveal 與 Dense information attenuation 正式凍結；除 genuine cross-phase blocker 外不得重開 P3-A。
- P5 candidates：Positions/Priorities/Mental 略偏實，只能在 Character、Foreground、Lantern 全部整合後重評；Observation rows 的少量 Scene texture 已 `READABILITY=PASS`。兩者皆不阻擋且不得重開 P3-A。
- Character boundary：Character/Foreground 尚未整合，`CHARACTER_SCENE_INTEGRATION=NOT_STARTED`；P3-VI-033～037/041 維持 `PENDING_RUNTIME_VALIDATION`。`P3_B_ALLOWED=YES` 只表示可在另行授權後做 placement/crop/scale/overlap/Z-order/clipping，不能宣告 Character integration；P3-C 才擁有 final environmental integration。
- Custody：本 Acceptance 僅新增 Acceptance、Accepted Gate 並更新 Status/Decision；Production 67 files 與 Tests 41 files 的 pre/post digest 保持一致。依授權未重跑 Build/Tests，MODEL/VISION/IMAGE API calls 0；未開始 P3-B/P3-C，未 commit、push 或 deploy。
- 下一步：唯一允許的 next step 是另行授權 `TCC_REFERENCE_MASTER_WPF_P3_B_CHARACTER_PLACEMENT`；本輪停止，不直接施工。
- 取代：本決策取代 Decision114 的 R2 Human Review pending 狀態與 `P3_B_ALLOWED=NO`；Decision114 的 R2 evidence、Decision113 的 Character contract、Decision112 的 Background integration、Decision111 的 Asset Acceptance、Decision110 的 Runtime-only Character rules與既有 P1/P2 frozen authority繼續有效。

## TCC-DEC-2026-09-24-116 — P3-B Character Placement Runtime Authority

- 日期：2026-09-24；來源：正式授權 `TCC_REFERENCE_MASTER_WPF_P3_B_CHARACTER_PLACEMENT`，限定 placement/crop/scale/overlap/Z-order/clipping，不含 Character environmental integration。
- Asset custody：Accepted `TCC_P3_CHARACTER_WORKING_SOURCE.png` 以 byte-identical copy 部署到 Production；source/Production SHA-256 均為 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`。沒有生成、recompress、grade、alpha、face、hair、outfit 或 identity 修改。
- Placement architecture：Character 是 direct Scene composition 的非互動 `Image`，`Stretch=Uniform`、HighQuality、無 Automation element/card/frame/panel；1672×941 local reference plane 只用於 artwork scaling，不縮放 functional UI。正式 z-order 保留 Background 10 → Decor Behind 40 → UI 60 → Character 70 → Foreground 80；本輪 z=40/80 slots仍空。
- Runtime authority：Release HWND-bound 1672×941 / DPI 96 / Text Size 100% screenshot SHA-256 `D7264F4AB82B7A32B6EF3D9AF29FC1848AB87C906703D44B9015AF88E0A90E19`；相對 Frozen P3-A R2 的 pixel-diff mask bounds為 x=1404–1671、y=169–930（268×762），right edge=1672、bottom edge=931；head 1549,255、face ref 1506,281、shoulder/fur 1429,389。UIA 8/8、offscreen 0、Text Size恢復152%、process殘留0。
- Placement verdict：position/scale/crop/footprint/head/shoulder/lower-body/right-edge/UI-overlap全部 PASS；Character card absent、Character不是第一焦點、Stepped Right Edge與Scenic Reveal保留。P3-A resource hashes、Background、P1 geometry、P2 component language保持；Character footprint外 Runtime pixel changes=0。
- Integration boundary：`CHARACTER_SCENE_INTEGRATION=NOT_STARTED`、`CHARACTER_CUTOUT_APPEARANCE=CURRENTLY_PRESENT_OR_UNVALIDATED`。White/face exposure、environment tint、hard alpha edge、shared lighting、atmospheric occlusion、silhouette breakup只記為 P3-C pending；P3-VI-033～037/041皆維持 `PENDING_RUNTIME_VALIDATION`，不得由本決策推導 final Character PASS。
- Gates：locked restore PASS；Release x64 build 0 warnings/0 errors；focused P3-A/P3-B 12/12；legacy phase migrations 8/8；full 1777/1777；failed checks 0；`IMAGE_API_CALLS=0`。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_B_CHARACTER_PLACEMENT=READY_FOR_HUMAN_PLACEMENT_REVIEW_B`；`P3_C_ALLOWED=NO`。未 commit、tag、push、release 或 deploy。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_WPF_P3_B_HUMAN_PLACEMENT_REVIEW`。
- 取代：本決策取代 Decision115 的 P3-B not-started／awaiting-authorization 狀態；Decision115 的 P3-A Accepted Baseline freeze、Decision114 的 R2 evidence、Decision113 的 Character integration contract與既有 P1/P2 authority繼續有效。

## TCC-DEC-2026-09-24-117 — P3-B Character Placement Repair R1

- 日期：2026-09-24；來源：Supervisor 正式裁決 `TCC_REFERENCE_MASTER_WPF_P3_B_HUMAN_PLACEMENT_REVIEW=REVISE`，限定修復 scale、crop、footprint、head/fur weight、lower-body crop、right-edge relation與focal hierarchy。
- Placement decision：V1 uniform scale 0.512 乘以 0.8203125，R1 scale=0.420；visible left保持x=1404，Runtime diff bounds由 x=1404–1671／y=169–930（268×762）修復為 x=1404–1623／y=175–799（220×625）。人物不重新置中，main lower footprint停於y=800，右側保留x=1624–1671 scene band。
- Visual result：position、scale、crop、footprint、head、shoulder/fur、lower-body crop、right-edge relation、UI overlap與secondary focal hierarchy全部PASS；Character card absent、first focal point NO、poster/hero effect NO。Accepted Visual vs R1 direct board包含 full/right/head/shoulder crops。
- Scope：只修改 Uniform placement/crop；Accepted Character source/Production bytes、P3-A Background/resources、UI geometry/style、P1/P2、z-order與future slots保持。沒有 exposure/color/light/blur/shadow/alpha/haze/foreground/plum/lantern；P3-VI-033～037/041仍 pending，P3-C/P3-D未開始。
- P3-D finding：`P3D_PENDING_FINDING_001` 記錄 Accepted Visual 右緣 dark vertical architecture/calligraphy/plum/lantern/snow foreground 關係；本輪只保留空間，禁止施工。
- Runtime evidence：1672×941／DPI96／Text Size100% HWND-bound screenshot SHA-256 `914B3BDD6A5080AF6176242130659ED09F9F9D94C4B61789ACFE890AC360DE91`；UIA8/8、offscreen0、Text Size恢復152%、process殘留0；comparison board SHA-256 `4E8944575234363896460EC8D4E76C644304F65F5C55735871E486C93B1F0898`。
- Gates：focused P3-A/P3-B 12/12、locked restore PASS、Release x64 0 warnings/0 errors、full 1777/1777、Frozen resources 6/6、Character hash match、failed checks 0、`IMAGE_API_CALLS=0`。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_B_CHARACTER_PLACEMENT_REPAIR_R1=READY_FOR_HUMAN_PLACEMENT_REVIEW_B_R1`；`P3_C_ALLOWED=NO`。未 commit、tag、push、release或deploy。
- 下一步：唯一允許的 next step 是 `TCC_REFERENCE_MASTER_WPF_P3_B_HUMAN_PLACEMENT_REVIEW_R1`。
- 取代：本決策取代 Decision116 的 V1 placement PASS／awaiting Human Review狀態；Decision116 的 asset/runtime architecture與P3-C boundary、Decision115 的P3-A freeze及既有P1/P2 authority繼續有效。

## TCC-DEC-2026-09-24-118 — P3-B Human Placement Review R1 Acceptance

- 日期：2026-09-24；來源：Supervisor 正式裁決 `TCC_REFERENCE_MASTER_WPF_P3_B_HUMAN_PLACEMENT_REVIEW_R1=PASS`，本階段限Audit／Acceptance落盤，不得修改Production、Tests、Character、Background、Glass或進行P3-C施工。
- Evidence：1672×941 Runtime screenshot SHA-256 `914B3BDD6A5080AF6176242130659ED09F9F9D94C4B61789ACFE890AC360DE91`驗證吻合；Repair R1 Gate SHA-256 `B5081AB99C4B4FBF9E93B55EF397B4F13D5E0CAED5A154376F996CDF975F2BA0`，status `READY_FOR_HUMAN_PLACEMENT_REVIEW_B_R1`，failed checks `NONE`。
- Accepted baseline：x=1404–1623、y=175–799、220×625正式凍結；position、scale、crop、footprint、head／shoulder anchors、lower-body crop、right-edge relation與UI overlap全部PASS；Character card absent、first focal point NO；Stepped Right Edge、Scenic Reveal、P3-A、P1、P2全部preserved。
- Placement/Integration separation：`CHARACTER_PLACEMENT_STATUS=ACCEPTED_BASELINE`不等於final Character acceptance。`CHARACTER_SCENE_INTEGRATION=NOT_STARTED`、`CHARACTER_CUTOUT_APPEARANCE=DETECTABLE`；P3-VI-033～037與041全部保持`PENDING_RUNTIME_VALIDATION`；placement-only final acceptance為`FORBIDDEN`。
- P3-C rule：P3-C須以Accepted Placement geometry為基線，除genuine integration blocker外不得重做scale/move/crop；必須處理environment contamination、white/face exposure、shared moonlight、hair rim、local shadow、alpha edge、atmospheric occlusion、silhouette breakup、depth/sharpness與foreground interaction。一般Human Reviewer若不放大即可辨識透明PNG貼圖，P3-C必須REVISE，不得進P3-F。
- P3-D custody：`P3D_PENDING_FINDING_001`持續保留，涵蓋Accepted Visual右側dark architecture/scenic element、vertical calligraphy、plum、lantern及foreground snow/rock；本Acceptance未施工。
- Scope custody：Production 68 files digest `A45B018991DD366BB6FD304976072AD4C9652655F0DB3B9F6ED27256C4D100EA`與Tests 42 files digest `FEA5215EE3B3906A984D6A0E3303A4765296BD63B102DA0600306B0ECB50E97E`前後一致。依授權未Build、未跑Tests；MODEL/VISION/IMAGE API calls 0；未commit、push或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_B_ACCEPTANCE=PASS`、`ACCEPTED=YES`、`P3_C_ALLOWED=YES`。P3-C本輪未開始，仍需另行正式授權。
- 下一步：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_SCENE_INTEGRATION`。
- 取代：本決策取代Decision117的awaiting Human Placement Review R1狀態；Decision117的R1 Runtime authority與P3-D pending finding、Decision116的asset/runtime architecture、Decision115的P3-A freeze及既有P1/P2 authority繼續有效。

## TCC-DEC-2026-09-24-119 — P3-C Bounded Character Integration Escalation

- 日期：2026-09-24；來源：正式授權 `TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_SCENE_INTEGRATION`，限定先以WPF-native/non-generative方法完成最多C-V1與一輪C-R1，仍可辨識cutout時必須停止並請求derived-asset授權。
- Implementation：Accepted Character bitmap保持byte-identical，SHA-256 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`；Accepted placement保持x=1404–1623、y=175–799、220×625。五張static technical masks與四個同幾何non-interactive WPF overlays處理cool environment、shadow side、moonlight edge、local atmosphere及edge transition；無blur、DropShadow、shader、animation、timer或Image API。
- Runtime verdict：C-R1的shared lighting、white material、face integration及luminance hierarchy改善並PASS，但一般Human Reviewer在1672×941不放大仍可直接讀到完整透明PNG貼層。因此P3-VI-033/035/041 FAIL，P3-VI-034/036/037/042/043/044 PASS；總P3 invariants為44，P3-C required set 6/9 PASS。
- Escalation：依授權上限停止，不做第三輪effect-stack camouflage。`CHARACTER_DERIVED_INTEGRATION_ASSET_REQUIRED=YES`、`CHARACTER_CUTOUT_APPEARANCE=DETECTABLE`。不得自行生成derived Character asset，且P3-D不得救援P3-C；`P3D_PENDING_FINDING_001`保持記錄但未施工。
- Evidence：R1 Runtime screenshot SHA-256 `B005811FC7196F02989C24E079E84ED16F1BEF214D6FC58A53B6040190CC9051`；comparison board `886A294E...F622`；ROI metrics `2DC9D644...39C9`；UIA 8/8、offscreen 0、Text Size 100% capture後恢復152%、process殘留0。
- Gates：focused 17/17、locked restore PASS、Release x64 0 warnings/0 errors、full 1781/1781、Image API calls 0。P3-A/P3-B authority preserved；未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_SCENE_INTEGRATION=BLOCKED_PENDING_DERIVED_ASSET_AUTHORIZATION`；`P3_D_ALLOWED=NO`。
- 下一步：唯一允許的next step是另行授權 `TCC_REFERENCE_MASTER_WPF_P3_C_DERIVED_CHARACTER_ASSET_PREP_AUTHORIZATION`。
- 取代：本決策取代Decision118的P3-C not-started／allowed狀態；Decision118的P3-B Accepted Placement、Decision115的P3-A Accepted Baseline、Decision113的Character contract及既有P1/P2 authority繼續有效。

## TCC-DEC-2026-09-24-120 — P3-C Derived Character Asset Preparation Candidates

- 日期：2026-09-24；來源：正式授權 `TCC_REFERENCE_MASTER_WPF_P3_C_DERIVED_CHARACTER_ASSET_PREP_AUTHORIZATION`，限 cleanroom 產生最多兩張 reference-preserving image-to-image候選，不得替換Runtime、修改Production/Tests或進入P3-D。
- Generation：使用 Character Identity Authority 作 primary edit target，Accepted Visual只作light/luminance/mood reference，Accepted Background 02只作environment reference；共2次built-in imagegen呼叫，runtime model identifier未暴露。兩張raw輸出各自保留後，final候選以deterministic post-process重套Character Authority exact Alpha並清除Alpha外RGB。
- Candidate disposition：Candidate 01 SHA-256 `9C66C4DEFAABCE977A03B890F07F631AE564AAA632A66C12A1B19EC0554CD222`，`PASS_ASSET_QA_NOT_PREFERRED`；Candidate 02 SHA-256 `F2ABBD4778C017DED49416EACD4F29F88631362769D8869030C3A54CCD8FDAE4`，`PASS_ASSET_QA_RECOMMENDED_FOR_HUMAN_REVIEW`。兩者均1024×1536 RGBA，Alpha與Authority byte-exact，identity、geometry與environment-only transformation QA PASS。
- Visual decision：Candidate 02相較Candidate 01具較高face/hair correlation與較低非必要藍紫飽和，保留同一人物、臉、表情、pose、髮型、飾品、白毛、服裝、雙手與配件，因此推薦人工審查；本決策不構成接受或Production authority。
- Contract：新增P3-VI-045 Derived Identity Preservation、046 Geometry Preservation、047 Environment-Only Transformation，P3 invariant總數由44增至47。045/046/047在asset candidate scope PASS；033/035/041只記`LIKELY_RESOLVED_CANDIDATE`並維持`PENDING_RUNTIME_REVALIDATION`。
- Scope/custody：Character Authority、Accepted Visual、Background 02與Accepted P3-B placement hashes/geometry有效；MainWindow、Production Character、Background、Glass、Tests與Runtime均未由本任務修改；P3-D assets未整合；`IMAGE_API_CALLS=2`；未commit、push或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_DERIVED_CHARACTER_ASSET_PREP=READY_FOR_HUMAN_DERIVED_CHARACTER_ASSET_REVIEW`；`DERIVED_CHARACTER_ASSET_ACCEPTED=NO`、`P3_D_ALLOWED=NO`。
- 下一步：唯一允許的next step是另行授權 `TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_DERIVED_CHARACTER_ASSET_REVIEW`。
- 取代：本決策解除Decision119的derived-asset preparation authorization blocker，但不取代其Runtime FAIL/PASS證據；Decision119的P3-C Runtime baseline、Decision118的P3-B Accepted Placement、Decision115的P3-A Accepted Baseline與既有P1/P2 authority繼續有效。

## TCC-DEC-2026-09-24-121 — P3-C Derived Asset Acceptance and Runtime Revalidation V2

- 日期：2026-09-24；來源：Supervisor正式裁決`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_DERIVED_CHARACTER_ASSET_REVIEW=PASS`並授權Derived Asset Acceptance + Runtime Revalidation V2。Candidate01因environmental cold saturation過強REJECT；Candidate02接受作P3-C Runtime baseline。
- Asset custody：Candidate02與Production derived copy SHA-256均為`F2ABBD4778C017DED49416EACD4F29F88631362769D8869030C3A54CCD8FDAE4`，1024×1536 RGBA且byte-identical。Character Identity Authority與原Production source均保持`A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`；derived asset不成為新Identity Authority。
- Minimal Runtime：V2 Base只使用Accepted P3-A Background、Derived Candidate02、既有Canvas／clip／placement；移除C-R1 clip mask、environment grade、shadow side、local atmosphere與moonlight edge stack。Accepted placement保持x=1404–1623、y=175–799、220×625；P3-D assets、Blur、DropShadow、cyan glow、whole-body tint、Opacity trick及重複人物均不存在。
- Runtime evidence：current-binary 1672×941／DPI96／Text Size100% HWND-bound V2 Base screenshot SHA-256為`C418281C6348B4182E75632068524438C0F623B43F6E0E952B09955D1B7C02CE`；apphost`DBB92113...01C9`、DesktopHost DLL`1DC53455...C041`；UIA8/8、offscreen0，Text Size恢復152%。相較C-R1，人物cold contamination與局部exposure有部分改善，但完整透明人物輪廓仍可立即辨識。
- Calibration decision：授權只允許在V2 Base已顯著改善且僅剩minor edge/exposure/atmosphere mismatch時執行一次bounded V2-R1；本次剩餘的是完整silhouette/cutout failure而非minor residual，故V2-R1未執行。不得以另一次atmospheric/effect stack、Candidate03、Image Generation或P3-D掩蓋問題。
- Invariants：P3-VI-034/036/037/042/043/044/045/046/047 PASS；P3-VI-033/035/041 FAIL。`CHARACTER_CUTOUT_APPEARANCE=DETECTABLE`，`P3_C_RUNTIME_REVALIDATION_V2=REVISE_REQUIRES_SUPERVISOR_DECISION`，`P3_D_ALLOWED=NO`。
- Gates：locked restore PASS；focused 187/187；Release x64 0 warnings/0 errors；full 1781/1781；P3-A frozen resources 6/6；Background source/Production與Derived accepted/Production hashes match；`IMAGE_API_CALLS=0`。未commit、tag、push、release或deploy。
- 下一步：依正式契約只能進行`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_VISUAL_REVIEW_V2`，由Supervisor決定後續；不得自行繼續施工。
- 取代：本決策完成Decision120的Human Derived Asset Review pending狀態，但不推翻Decision119的C-R1 Runtime FAIL；Decision118的P3-B Accepted Placement、Decision115的P3-A Accepted Baseline與既有P1/P2 authority繼續有效。

## TCC-DEC-2026-09-24-122 — P3-C Selective Edge Matte Is Technically Valid but Insufficient

- 日期：2026-09-24；來源：正式授權 `TCC_REFERENCE_MASTER_WPF_P3_C_SELECTIVE_EDGE_MATTE_INTEGRATION`，限定Candidate02 RGB、P3-B placement與P3-A frozen baseline下，以deterministic 4–12 source-pixel material matte修復Alpha boundary；禁止Candidate03/04、Image Generation、全人物調色/blur/opacity、effect stack與P3-D。
- Architecture：identity/geometry authority與Runtime compositing matte正式拆開。Candidate02 RGB SHA-256維持`F2AB...FAE4`；Layer1=Candidate02×Core Mask、Layer2=Candidate02×Selective Edge Mask，幾何均為`1310,155,430.08×645.12`、clip`94,0,220,645.12`。六張1024×1536 RGBA masks由distance transform、fixed material polygons、smoothstep與deterministic modulation產生；Image API calls 0。
- Alpha custody：Hair/Fur/Cloth/Hard source bands分別10/10/8/4 px，約映射1.68–4.2 Runtime px；deep-core Alpha不變，outside-source新增像素0，source/target nonzero Alpha pixels皆1,078,083；57,484 pixels／可見source 4.2279%受transition影響。P3-VI-046補充geometry preservation不等於Runtime Alpha byte identity；P3-VI-048～051新增，P3 invariant總數51。
- Runtime：current-binary 1672×941／DPI96／Text Size100% HWND screenshot SHA-256`3FAF8050...4FED`；UIA8/8、offscreen0、Text Size恢復152%、process殘留0。Candidate02 shared lighting、white material、face與luminance hierarchy保持；無halo、uniform feather、full blur、glow或DropShadow。
- Visual verdict：局部hair/fur/cloth boundary較V2柔和且材質差異有效，但一般Human Reviewer在full-frame正常尺寸仍立即讀到一整張透明PNG。P3-VI-034/036/037/042～051 PASS；P3-VI-033/035/041 FAIL；`CHARACTER_CUTOUT_APPEARANCE=DETECTABLE`、`P3_C_EDGE_MATTE_RESULT=INSUFFICIENT`。剩餘問題是whole-subject topology／scene interaction，不是單一材質minor defect，因此optional V3-R1未執行。
- Gates：locked restore PASS；Release x64 0 warnings/0 errors；focused 51/51；full 1782/1782；P3-A frozen resources 6/6；Background與Candidate02 custody PASS；P3-D assets 0。未commit、tag、push、release或deploy。
- 下一步：唯一允許的next step為`TCC_REFERENCE_MASTER_WPF_P3_C_SUPERVISOR_ARCHITECTURE_DECISION`，由Supervisor裁決scene-conditioned baked interaction patch或P3-C/P3-D責任調整；P3-D仍`NO`，不得自行續作。
- 取代：本決策完成Decision121等待Supervisor後的selective-matte授權，但不推翻Decision121的V2比較證據、Decision120的Derived Candidate custody、Decision118的P3-B Accepted Placement、Decision115的P3-A Accepted Baseline與既有P1/P2 authority。

## TCC-DEC-2026-09-25-123 — Option B Responsibility Split and P3-D Asset-Gap Stop

- 日期：2026-09-25；來源：Supervisor正式架構裁決`TCC_REFERENCE_MASTER_WPF_P3_C_SUPERVISOR_ARCHITECTURE_DECISION`，採Option B重新定義P3-C/P3-D責任，並授權P3-D只整合accepted/authority-backed scene-contact elements。
- Responsibility：C-V3正式定義為`P3_C_INTRINSIC_CHARACTER_BASELINE=ACCEPTED_FOR_SCENE_CONTACT_VALIDATION`，不等於final Character acceptance；P3-C擁有RGB/exposure/shared lighting/face/white material/intrinsic Alpha與material edge，P3-D擁有Behind/Foreground/梅枝/燈籠/雪岩/right scenic structure/vertical detail及external silhouette interruption。
- Runtime：accepted Behind Decor與Foreground Accent byte-identical整合於z40/z80，C-V3 Character保持z70；right vertical maxim `雪落無聲 而市有道`依Content/Visual Language authority加入。Candidate02 RGB、masks、edge matte、scale/position/crop皆不變；Image API calls 0。
- Visual disposition：available-authority combined Runtime以梅枝及雪/岩前景中斷right arm/waist/lower contour，不再呈現完整360-degree clean silhouette；P3-VI-033/035/041為`PASS_CANDIDATE_AVAILABLE_AUTHORITY_ASSETS`，P3-VI-042為`PASS_WITH_REVISED_SEMANTICS`，053/054 PASS。此candidate不等於final P3-D Human acceptance。
- Asset gap：Accepted Visual所需far-right dark vertical scenic/architectural structure沒有accepted raster asset且無explicit vector reconstruction authority。禁止任意fog、branch、black concealment、vector invention或P3-D用來掩蓋intrinsic Character failure；`P3D_PENDING_FINDING_001=BLOCKED_BY_ASSET_GAP`，P3-VI-052/055 blocked。
- Evidence：1672×941/DPI96/Text Size100% HWND screenshot SHA-256`570262A36CCD9280C2408DF6850304AC6C2676B065FA6A32B44F806C61207C40`；comparison board`F6F2D843E7A0F2CBD52B08CD7D8CFBD8C07412028BE815C85D60B90107E9D60F`；UIA8/8、offscreen0、Text Size恢復152%。
- Gates：P3 invariant catalog 55 unique/contiguous；focused 210/210、locked restore PASS、Release x64 0 warnings/0 errors、full 1785/1785；P3-A/P3-B/C-V3 custody preserved；staged files 0。未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_D_SCENE_CONTACT_INTEGRATION=BLOCKED_PENDING_P3D_ASSET_PREP`；不得宣告`READY_FOR_HUMAN_COMBINED_VISUAL_REVIEW_D`。
- 下一步：唯一允許的next step為另行授權`TCC_REFERENCE_MASTER_WPF_P3_D_ASSET_PREP_AUTHORIZATION`。
- 取代：本決策取代Decision122的P3-D prohibited／Supervisor architecture decision pending狀態；Decision122的C-V3 technical evidence、Decision121的Candidate02 custody、Decision118的P3-B Accepted Placement、Decision115的P3-A Accepted Baseline與既有P1/P2 authority繼續有效。

## TCC-DEC-2026-09-25-124 — P3-D R1 Right Scenic Support and Footprint Calibration

- 日期：2026-09-25；來源：正式授權 `TCC_REFERENCE_MASTER_WPF_P3_D_ASSET_PREP_AND_SCENE_CONTACT_CALIBRATION_R1`，限定Character/P3-A凍結、Accepted scene raster pixels不變，只允許缺失right vertical scenic structure的reference-preserving reconstruction與Runtime transform calibration。
- Asset decision：Preflight確認repository無可直接使用之right-side dark vertical wooden/architectural structure。以Accepted Visual為唯一視覺uauthority產生1張1672×941 RGBA Candidate 01，Candidate/Production SHA-256皆`3DABB2CA8E8547F342F511719D80F24EDF9727BB3D2EFF60025DD5389AB24E24`；無baked text、無人物/梅/燈籠/背景/新建築，狀態僅`PREFERRED_FOR_RUNTIME_VALIDATION`，不自行Final Accept。`IMAGE_API_CALLS=1`。
- Runtime decision：Right structure置z50於Character後方，現有live vertical calligraphy保持於foreground層。Accepted Behind/Foreground assets均byte-identical，以兩個regional clip/placement分別校準left top/right top與left bottom/right contact；不以asset canvas bounds作Runtime footprint authority。
- Freeze：Candidate02 RGB、lighting、exposure、face、fur、hair、selective edge matte、material masks、placement、scale、crop全部不變；P3-A Background/Glass/UI resources不變。Character derived hash保持`F2ABBD...FAE4`，visible bounds保持`1404,175-1623,799`。
- Visual decision：HVR-001=`RESOLVED_CANDIDATE`，HVR-002/003 PASS；left top/bottom plum、right top plum、right foreground及lantern footprint PASS。`P3D_PENDING_FINDING_001=RESOLVED`；P3-VI目錄由55增至59，001–059 unique/contiguous；033/035/041/052–059均PASS，Workspace priority PASS，cutout appearance `NOT_DETECTABLE`。
- Evidence：1672×941/DPI96/HWND-bound Runtime screenshot SHA-256`A491E9022F6E58AE294150186CD5C6F4377198920EAD4ADC58A8570400A84C0C`；Accepted vs Current vs R1 comparison SHA-256`DEF70E56FA69E3B54E700C739D50F04358D4B0B500D7712D4A1500F4AFFA8BD6`；UIA8/8、offscreen0、Text Size100%擷取後恢復152%。
- Gates：focused 210/210、locked restore 6/6、Release x64 0 warnings/0 errors、full 1785/1785，failed/skipped 0；P3-A/P3-B/P3-C baseline custody PASS，staged 0。未commit、tag、push、release或deploy。
- 狀態：`READY_FOR_HUMAN_COMBINED_VISUAL_REVIEW_D_R1`。這是review readiness，不是Final Combined Acceptance；不得進入下一phase。
- 下一步：唯一允許的next step為`TCC_REFERENCE_MASTER_WPF_P3_D_HUMAN_COMBINED_VISUAL_REVIEW_R1`。
- 取代：本決策解除Decision123的P3-D asset-gap blocker，不取代Decision123的Option B責任邊界、Decision122的C-V3 technical evidence、Decision121的Candidate02 custody、Decision118的P3-B Accepted Placement或Decision115的P3-A Accepted Baseline。

## TCC-DEC-2026-09-25-125 — P3-D R2 Optical Depth and Local Light Contact

- 日期：2026-09-25；來源：Supervisor Human Combined Visual Review R1正式裁決`REVISE`，限定修復scene accessory optical integration，Character與P3-A完全凍結。
- Optical decision：R1幾何、scale、clip與Z-order保持；Depth A左/右上梅以`0.58/0.48` opacity、directional taper與較強cool contamination退入背景，Depth B右木構以`0.70`與edge taper接軌背景，Depth C左/右前景以`0.74/0.82`及較弱cool contamination保持清晰但不自成PNG組。
- Local light：Lantern保留warm accent，新增僅由Foreground Alpha約束、中心`0.88,0.80`的克制amber contact，影響immediate snow/wood/branch/rock；不照亮Character、不形成全域orange block。無Gaussian/BlurEffect/DropShadow/new fog/new decor。
- Character/P3-A custody：Candidate02 SHA-256維持`F2ABBD...FAE4`，Core/Edge mask hashes與`1310,155,430.08×645.12`/clip`94,0,220,645.12`完全不變；Background02、Glass、UI geometry/opacity與accepted scene rasters未修改。`IMAGE_API_CALLS=0`。
- Invariants：新增P3-VI-060～064；catalog為64 unique/contiguous、gaps/duplicates 0。HVR-001～005均RESOLVED；060～064 PASS；033/035/041與052～059保持PASS，Character cutout regression NO，Workspace priority PASS。
- Evidence：1672×941/DPI96/HWND-bound screenshot SHA-256`5FCA4C044984149614215CD7E0B546EC054BBD1AA6C817C48F5A44C2D851B619`；Accepted/R1/R2 board SHA-256`8EC4217E5DAA935F6E043208B190A3C7D079F848F7E23997DB735D576137432B`；UIA8/8、offscreen0、Text Size恢復152%。
- Gates：locked restore 6/6；P3 focused 21/21；affected regressions 7/7；Release x64 0 warnings/0 errors；full 1786/1786，failed/skipped 0；staged 0。Phase2 literal-color fixture精準排除四個正式P3 scene slots，loose-XAML fixture只中和`Assets/Scene` ImageBrush URI，未放寬產品/UI契約。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_D_SCENE_ACCESSORY_INTEGRATION_R2=READY_FOR_HUMAN_COMBINED_VISUAL_REVIEW_D_R2`；此為review readiness，不是Final Combined Acceptance。未commit、tag、push、release或deploy。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_D_HUMAN_COMBINED_VISUAL_REVIEW_R2`。
- 取代：本決策取代Decision124的R1 Human Review pending狀態與59-invariant count；不取代Decision124的asset/footprint evidence、Decision123的Option B責任邊界、Decision122的C-V3 technical evidence、Decision121的Candidate02 custody、Decision118的P3-B Accepted Placement或Decision115的P3-A Accepted Baseline。

## TCC-DEC-2026-09-25-126 — P3-D R3 Bounded Authority Accent Weight Calibration

- 日期：2026-09-25；來源：Supervisor Human Combined Visual Review R2正式裁決`REVISE`，確認R2 optical depth方向正確但發生`AUTHORITY_ACCENT_WEIGHT_OVER_SUPPRESSION`，限定只修right-top plum、right foreground/lantern、left-bottom plum與right-structure footprint；Character與P3-A完全凍結。
- Calibration decision：left-top保持R2不變；right-top image/cold overlay由`0.48/0.24`調為`0.56/0.18`；left-bottom由`0.74/0.16`調為`0.80/0.12`且footprint不變；right foreground以右下錨定放大約3%至`432,243.15,1240×697.85`，image/cold/warm由`0.82/0.14/0.62`調為`0.86/0.12/0.70`。所有既有directional taper、Alpha-bound contamination與local-light topology保留。
- Structure decision：right structure opacity、色彩與mask不變，只將image右移28 px，並clip於`x=1518–1672`形成Accepted Visual近似的窄edge support；live calligraphy保持原位置且仍有結構承載。
- Character/P3-A custody：Candidate02 SHA-256維持`F2ABBD...FAE4`，Character RGB/exposure/lighting/face/fur/hair/matte/masks/Alpha/X/Y/scale/crop均未修改；P3-A Background hash`438466...2771`與Glass/UI不變。Accepted Behind/Foreground/Structure rasters維持`C4DC3E...C225`、`1E6B25...9CF8`、`3DABB2...E24`；`IMAGE_API_CALLS=0`。
- Invariants：新增P3-VI-065 Authority Accent Weight Fidelity、066 Right Vertical Structure Footprint Fidelity、067 Scene Contact Continuity；catalog為67 unique/contiguous、gaps/duplicates 0。P3D-R2-HVR-001～004 RESOLVED；P3-VI-060～067 PASS；R2 optical depth與Character cutout/no-regression PASS保持。
- Evidence：1672×941/DPI96/HWND-bound screenshot SHA-256`8DE97343A889192F7FCE0C798C0F579A92743E007B61D154ECF46CFC04DE6DF7`；Accepted/R2/R3 board SHA-256`B3CB90E1016D437F5BF9179C99AE122D7389C76882A0FBA95B83008484C7DBF2`；UIA8/8、offscreen0、Text Size恢復152%。
- Gates：locked restore 6/6；P3 focused 21/21；Release x64 0 warnings/0 errors；full 1786/1786，failed/skipped 0；staged 0；無BlurEffect/DropShadowEffect/glow/new scene object。未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_D_SCENE_ACCESSORY_INTEGRATION_R3=READY_FOR_HUMAN_COMBINED_VISUAL_REVIEW_D_R3`；此為review readiness，不是Final Combined Acceptance。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_D_HUMAN_COMBINED_VISUAL_REVIEW_R3`。
- 取代：本決策取代Decision125的R2 Human Review pending狀態與64-invariant count；不取代Decision125的optical integration evidence、Decision124的asset authority、Decision123的Option B責任邊界、Decision122的C-V3 evidence、Decision121的Candidate02 custody、Decision118的P3-B placement或Decision115的P3-A Accepted Baseline。

## TCC-DEC-2026-09-25-127 - P3-D R4 Accent Weight and Character Lower Termination Repair

- 日期：2026-09-25；來源：Supervisor Human Combined Visual Review R3正式裁決`REVISE`，限定修復四區梅枝Authority weight與Character bottom hard horizontal termination，禁止修改Character intrinsic baseline、P3-A、lantern、right structure及R3 optical depth。
- Root cause：accepted Core/Edge均位於`1310,155,430.08x645.12`且clip為`94,0,220,645.12`，因此兩層在Runtime y=`800.12`同時結束；z80 foreground asset在該列無法跨Character完整寬度提供opaque scene contact，opacity-only校正不足以移除水平資產底邊。
- Character decision：原Core/Edge source、RGB、masks、Alpha、X/Y、scale、clip完全不變。R4新增兩個同源、同Core/Edge mask的lower-continuation presentation，只從accepted bottom y=`800.12`開始，使用來源最後`140.88` Runtime px的垂直反射延伸並保持於z80 foreground後方；沒有fade、fog、black mask、blur、new asset或Image API call。此hidden continuation不重開P3-B accepted upper placement。
- Accent decision：left-top image/cold由`0.58/0.20`調為`0.76/0.12`；right-top由`0.56/0.18`調為`0.80/0.12`並左移55 px；left-bottom由`0.80/0.12`調為`0.90/0.08`；right foreground由`0.86/0.12`調為`0.96/0.08`。方向性taper、lantern warm contact、right structure、scene rasters與Depth A/B/C topology保持。
- Visual decision：P3D-R3-HVR-001～005均RESOLVED；right/left top、right foreground與left-bottom plum weight PASS；Character bottom hard clip NO、scene occlusion PASS、hidden continuation VALID；lantern/right structure/optical depth/Workspace priority無回歸。
- Invariants：新增P3-VI-068 Character Lower Termination Through Scene Occlusion；catalog為68 unique/contiguous、gaps/duplicates 0；P3-VI-065～068 PASS，其餘R3/P3-C accepted results保持。
- Evidence：1672x941／DPI96／HWND-bound screenshot SHA-256`7119269D09013572289AD14C45612B9DDE0A733DF12BB84B134D7BF90C6BB5DB`；Accepted/R3/R4 board SHA-256`2500A4DFE4DE24E70CA1FD50A483D82F4BFF5AD94531EEBA62AB1DE1542868B2`；UIA8/8、offscreen0、Text Size恢復152%、process殘留0。
- Gates：focused 22/22；locked restore 6/6；Release x64 0 warnings/0 errors；full 1787/1787，failed/skipped 0；staged 0；`IMAGE_API_CALLS=0`。未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_D_SCENE_ACCENT_AND_CHARACTER_BOTTOM_OCCLUSION_REPAIR_R4=READY_FOR_HUMAN_COMBINED_VISUAL_REVIEW_D_R4`；這是review readiness，不是Final Combined Acceptance。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_D_HUMAN_COMBINED_VISUAL_REVIEW_R4`。
- 取代：本決策取代Decision126的R3 Human Review pending狀態與67-invariant count；不取代Decision126的right-structure authority、Decision125的optical integration、Decision124的asset authority、Decision123的Option B責任邊界、Decision122的C-V3 evidence、Decision121的Candidate02 custody、Decision118的P3-B accepted upper placement或Decision115的P3-A Accepted Baseline。

## TCC-DEC-2026-09-25-128 - P3-B Clean Runtime Evidence Supersedes Polluted Candidate

- 日期：2026-09-25；來源：Supervisor正式裁決`TCC_REFERENCE_MASTER_WPF_P3_B_HUMAN_PLACEMENT_REVIEW=INVALID_CANDIDATE`並授權`TCC_REFERENCE_MASTER_WPF_P3_B_CLEAN_RUNTIME_REPAIR_R1`。
- Stage purity：P3-B Runtime只顯示P3-A Accepted Background、P3-A Accepted UI/Glass及Accepted Character Working Source。Behind Decor、Foreground Accent、Lantern、Right Architecture、Plum、vertical calligraphy、scene occlusion、Derived Character、Edge Matte與lower continuation均不渲染；其已接受資產仍保留custody，舊gate維持歷史紀錄。
- Character decision：Runtime恢復單一`TCC_P3_CHARACTER_WORKING_SOURCE.png`，geometry維持`1310,155,430.08×645.12`、clip`94,0,220,645.12`及visible bounds`1404,175-1623,799`。Character置z55、Accepted UI置z60，保留人物位置且AI Summary／Activity全部文字可讀；`UI_CRITICAL_TEXT_OCCLUSION=NO`。
- Runtime evidence：compiled WPF visual-tree probe確認Background／Character `IsVisible=true`，三個later-scene parent slots `Visibility=Collapsed`／`IsVisible=false`。1672×941／DPI96／HWND-bound screenshot SHA-256`D8469943F603D8847B7471B896AD3DA5F91C29A4DCDECF7514B4002D611D44DA`；Accepted vs Clean board SHA-256`8504AAC93AE2E079DCAC55675F9D63874F651220984EC3815E0A150387654BDA`；UIA8/8、offscreen0、Text Size恢復152%。
- Invariants：stage-specific catalog為43；P3-VI-042 Stage-Pure Placement Evidence與P3-VI-043 No Critical UI Content Occlusion均PASS。舊P3-C/P3-D implementation/review evidence保留為歷史，但不得作本次P3-B Runtime證據。
- Gates：focused 22/22；locked restore 6/6；Release x64 0 warnings/0 errors；full 1787/1787，failed/skipped 0；P3-A frozen resources 6/6；staged 0；`IMAGE_API_CALLS=0`。未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_B_CLEAN_RUNTIME_REPAIR_R1=READY_FOR_HUMAN_PLACEMENT_REVIEW_CLEAN_R1`；`P3_C_ALLOWED=NO`。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_B_HUMAN_PLACEMENT_REVIEW_CLEAN_R1`。
- 取代：本決策以Supervisor later authority取代Decision127作為current runtime track，並撤銷Decision118的P3-B accepted placement verdict；不修改P3-A Accepted Baseline或已接受binary asset custody。

## TCC-DEC-2026-09-25-129 - Clean P3-B Placement Accepted With Aspect-Safe Review Evidence

- 日期：2026-09-25；來源：Supervisor正式授權`TCC_REFERENCE_MASTER_WPF_P3_B_HUMAN_PLACEMENT_REVIEW_CLEAN_R1_ACCEPTANCE`並裁決Clean R1 `PASS`。
- Acceptance：Clean Runtime僅包含P3-A Accepted Baseline與Accepted Character Working Source；stage purity、position、scale、crop、footprint、head/shoulder/lower-body anchors、right-edge relation、UI overlap均PASS；critical UI text occlusion、Character card與first-focal-point均為NO。P3-B正式凍結為`ACCEPTED_BASELINE`。
- Scale authority：撤銷由stage-polluted evidence推導的`overscale approximately 20%`與`CURRENT_SCALE × 0.82–0.84`；不得再將其作placement authority。Clean Runtime的現有X/Y、uniform scale、crop、anchors、footprint、right-edge/UI relation與z-slot成為正式P3-B authority。
- P3-VI-044：正式新增Review Evidence Aspect-Ratio Integrity。原comparison crops因固定矩形填滿而非等比例，不得作geometry判斷；正式board改為Uniform fit＋letterbox。兩個full frames與每側五個ROI共12/12 placements之`scale_x=scale_y`，non-uniform failures 0。Board SHA-256`B03EB2C191404569D642648328EBEB496FCA43E5F8922A8495CDD99A61D1BE60`；audit SHA-256`1667538BB0104BA9A7B1BBFA5B4D7DF46F58FDC7E4F894148454B1C995DCDB2D`。
- Invariants：Active P3-B catalog為44。P3-VI-042/043 PASS，P3-VI-044 `DEFINED_AND_APPLIED`；歷史post-P3-B draft 045–068不屬於目前accepted baseline，需後續重新授權才可使用。
- Integration boundary：Placement Acceptance不等於Character Integration Acceptance。`CHARACTER_SCENE_INTEGRATION=NOT_STARTED`、`CHARACTER_CUTOUT_APPEARANCE=CURRENTLY_PRESENT`；P3-VI-033–037與041保持`PENDING_RUNTIME_VALIDATION`。P3C-FINDING-001～008與P3D_PENDING_FINDING_001正式保留。
- Custody：Acceptance/Audit only；Production 83 files digest`3EE8F855D8825CC2C0FCD899C6C404C43011FF6D30F8F35F6FBF111B6B8839BD`、Tests 44 files digest`28492CE2FC16551DFF89C5402DA85EF1EC211B01FC64DD12C8EB1B10CCB353CC`前後不變。依授權未build/test；model/vision/image calls 0；未commit/push/deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_B_ACCEPTANCE=PASS`、`P3_B_PLACEMENT_STATUS=ACCEPTED_BASELINE`、`ACCEPTED=YES`、`P3_C_ALLOWED=YES`。
- 下一步：唯一為另行授權`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_SCENE_INTEGRATION`；本輪不得直接施工P3-C。
- 取代：本決策完成Decision128的Clean R1 Human Review pending狀態，並以P3-VI-044撤銷失真review crops的幾何證據資格；不修改P3-A Accepted Baseline、Production Runtime或binary asset custody。

## TCC-DEC-2026-09-25-130 - P3-C Bounded WPF Integration Stops at Derived-Asset Boundary

- 日期：2026-09-25；來源：正式授權`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_SCENE_INTEGRATION`，以Decision129的Clean P3-B Accepted Placement、P3-A Accepted Baseline與Accepted Character Working Source為凍結輸入。
- Implementation decision：Character source pixels與`1310,155,430.08×645.12`／clip`94,0,220,645.12`幾何不變；以既有deterministic masks建立region/material-specific cool contamination、shadow-side、moonlight rim、face/fur/cloth/warm contact及atmospheric presentation layers。沒有BlurEffect、DropShadowEffect、animation、P3-D scene rescue或Image API call。
- Bounded repair：初版在1672×941正常顯示尺度仍讀為完整PNG，故只執行授權的一次bounded repair，增加局部污染與atmospheric veil強度；不以全域tint、blur或第三套effect stack掩蓋問題。
- Visual decision：P3-VI-034 Shared Lighting、036 White Material Exposure、037 Face Integration、044 Aspect-Ratio Integrity、045 No Global Tint Shortcut、046 No Blur-Based Integration、047 Lighting Direction Coherence、048 Identity Preservation PASS；P3-VI-033 Environmental Integration、035 Silhouette Breakup、041 No Detectable Character Cutout FAIL。Character identity保持，但完整輪廓在final display scale仍可辨識。
- Evidence：1672×941／DPI96／HWND-bound screenshot SHA-256`F3B0B917BFC66508BFD6C887EE1852BE9C64C8A6ACBE75764CD3F725258DF416`；Accepted Visual／Clean P3-B／P3-C Runtime comparison SHA-256`73569C37CD817CDECD5AAF87C7A8462453C1681D0F3554B1E6373ADB0D417702`；24/24 review placements使用native-size letterbox且無非等比例縮放；UIA8/8、offscreen0、Text Size擷取後恢復152%。
- Gates：P3 invariant catalog為48 unique/contiguous；focused 19/19、locked restore 6/6、Release x64 0 warnings/0 errors、full 1788/1788、Frozen P3-A resource hashes 6/6；staged 0。未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_SCENE_INTEGRATION=BLOCKED_DERIVED_ASSET_REQUIRED`、`CHARACTER_DERIVED_INTEGRATION_ASSET_REQUIRED=YES`、`P3_D_ALLOWED=NO`。依授權停止，不能宣告`READY_FOR_HUMAN_VISUAL_REVIEW_C`。
- 下一步：唯一允許的next step為另行授權`TCC_REFERENCE_MASTER_WPF_P3_C_DERIVED_CHARACTER_ASSET_AUTHORIZATION`。
- 取代：本決策完成Decision129的P3-C not-started狀態；不取代Decision129的P3-B Accepted Placement、Decision115的P3-A Accepted Baseline或既有Accepted binary asset custody。

## TCC-DEC-2026-09-26-131 - P3-C Derived Character Candidate04 Prepared for Human Review

- 日期：2026-09-26；來源：正式授權`TCC_REFERENCE_MASTER_WPF_P3_C_DERIVED_CHARACTER_ASSET_AUTHORIZATION`，限定cleanroom asset preparation，不得修改Production、Tests、Runtime layers、Accepted authorities或開始P3-D。
- Candidate decision：先完成2組deterministic candidates；Candidate01仍明顯cutout，Candidate02有fog-sticker／連續halo風險，故依授權使用2次reference-preserving ImageGen。Candidate03身份安全但breakup不足；Candidate04以較強冷灰雪夜污染、authority-derived material-aware Alpha feather、分離的raw atmosphere residual與crossing wisps成為唯一Human Review推薦。
- Identity/geometry custody：四組皆為1024×1536 RGBA；Candidate04 face RGB correlation `0.96558716`、source topology retained `0.99999258`、zero-Alpha外RGB samples `0`；臉、髮型、髮飾、服裝結構、身材、姿勢、手部與P3-B anchor未重設。完整Scene、山、月、湖、建築、UI、梅枝、燈籠均未烘入資產。
- Asset architecture：base、atmosphere與integration mask分離；later Runtime integration必須重評並移除重複WPF moonlight/shadow/tint/veil，防止double rim、double shadow、cyan halo、over-dark face或over-blue cloth。
- Invariants/evidence：正式啟用P3-VI-049 Derived Character Identity Lock、050 No Baked Scene Background、051 Placement Anchor Preservation、052 No Double-Lighting Artifact；current P3 catalog為52。comparison 54/54 native-size crop placements aspect-safe，non-uniform failures 0；board SHA-256`F853C9DE...2864C`，Candidate04 composite SHA-256`460B80D3...75A1`。
- Gates/scope：manifest assets 12/12、aspect audit 54/54、P3 catalog 52/52 unique/contiguous、locked restore 6/6、Release x64 0 warnings/errors、full tests 1788/1788；task-start後Production/Test source修改數0/0，staged 0。`IMAGE_API_CALLS=2`；未替換Character、未改`MainWindow.xaml`、未開始Runtime integration或P3-D，未commit、tag、push、release或deploy。
- 狀態：`READY_FOR_HUMAN_DERIVED_ASSET_REVIEW`；`DERIVED_ASSET_ACCEPTED=NO`、`P3_D_ALLOWED=NO`。P3-VI-033/035/041仍是Human-owned Runtime gates，不能由asset preparation自動宣告PASS。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_DERIVED_ASSET_REVIEW`。
- 取代：本決策解除Decision130的asset-authorization pending狀態，但不取代其WPF-only失敗證據、Decision129的Clean P3-B Accepted Placement、Decision115的P3-A Accepted Baseline或任何Accepted binary authority。

## TCC-DEC-2026-09-26-132 - P3-VI-053 Forbids Placement Viewport Cropping

- 日期：2026-09-26；來源：使用者明確後續要求`P3-VI-053 — No Artificial Character Viewport Clipping`，其Authority高於先前P3-B對固定viewport的placement acceptance。
- Contract：Character placement/integration不得以固定矩形viewport裁掉Authority可見的hair、fur、sleeve、translucent cloth、ribbons或garment silhouette。唯一允許crop來源為Window physical boundary、Accepted Visual明確scene occlusion、或正式foreground occlusion/mask；source不存在的straight vertical/horizontal cutoff一律使`CHARACTER_INTEGRATION=FAIL`。
- Current evidence：Runtime Character為`1310,155,430.08×645.12`，主影像與多個integration layers反覆使用clip`94,0,220,645.12`，形成x=1404/1624直切。Window右界為x=1672，因此右切提前48px，左切無physical-boundary理由。Authority Alpha在viewport左側仍有87,039 pixels，viewport右側至Window boundary仍有131,424 pixels；左右cut columns各有706–1,225 nonzero rows。
- Decision：current Runtime `P3_VI_053=FAIL`、`CHARACTER_INTEGRATION=FAIL`。既有clipped P3-B/P3-C/Candidate04 composites不得用於placement geometry、silhouette completeness或final integration判斷。P3-VI-044 aspect-ratio PASS不抵銷P3-VI-053；等比例裁切仍可能是非法viewport。
- Asset custody：Candidate04 1024×1536 base/atmosphere/mask未被裁切，仍可保留為intrinsic asset preference；但其formal Human Derived Asset Review gate降為`BLOCKED_CHARACTER_VIEWPORT_CLIPPING_REPAIR_REQUIRED`，不得部署或宣告Accepted。
- Scope：本輪只更新規格、finding、evidence、gate與governance；未修改`MainWindow.xaml`、Production、Tests、Character assets或Runtime，未呼叫Image API，未commit/push/deploy。
- 狀態：P3 catalog為53；P3-VI-053 current Runtime FAIL，P3-D仍禁止。
- 下一步：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_VIEWPORT_CLIPPING_REPAIR_AUTHORIZATION`。
- 取代：本決策以explicit later authority取代Decision129中固定`94,0,220,645.12` clip可作Accepted Placement viewport的部分，以及Decision131的unconditional Human Derived Asset Review readiness；不取代P3-B scale/head/shoulder anchors、Candidate04 intrinsic asset QA、P3-A baseline或Character Identity Authority。

## TCC-DEC-2026-09-26-133 - P3-C Full-Alpha Viewport Clipping Repair R1

- 日期：2026-09-26；來源：正式授權`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_VIEWPORT_CLIPPING_REPAIR_AUTHORIZATION`與P3-VI-053 explicit later authority。
- Root cause：base Character與full-character integration layers重複使用固定`Rect=94,0,220,645.12`，在Runtime x=1404與x=1624形成Authority source不存在的垂直截斷；base opacity mask進一步把viewport bounds誤作silhouette authority。
- Repair decision：移除base Character的`Image.Clip`及`Image.OpacityMask`，並移除Environment Contamination、Shadow Side、Moonlight Rim、Atmospheric Depth四個full-character layers的相同固定clip。Character與所有full-source layers仍共用`1310,155,430.08×645.12`、`Stretch=Uniform`座標；face/fur/cloth/warm clips只限region-local effect，不裁切base visibility。唯一完整畫面crop為1672×941 physical Window boundary。
- Placement decision：不假設舊geometry。Accepted Visual與Authority source的robust feature alignment得到uniform scale`0.4229199885`、translation`1310.1124,157.8900`、225 good matches／189 inliers，因此保留Runtime uniform scale`0.42`與`1310,155`。Alpha>0實際footprint為`1310,173-1671,799`；原cut columns x=1404／1624分別含302／510個nonzero Authority rows。
- Visual evidence：正式Runtime screenshot為1672×941、DPI96、HWND-bound PrintWindow；SHA-256`E96FEA5BADB2DABBCCD176314244B7FC99226C30293748F066C7B574AF05A54E`。Accepted Visual與Runtime full frames及八個必查region全部使用等比例fit，16/16 placements aspect-safe，non-uniform failures 0；evidence SHA-256`1B55B61A6B6A574D5C196614DB96E20CA28AE3552D6493542E0C0428816F1337`。
- Invariants：P3 catalog增為55 unique/contiguous。P3-VI-053 No Artificial Character Viewport Clipping、054 Alpha-Silhouette Placement Authority、055 Window-Boundary Crop Legitimacy均PASS；P3-VI-044為16/16 PASS。P3-VI-033–037/041仍保留Human-owned final judgment，不由本技術gate越權接受。
- Custody/scope：Character source與Production SHA-256均為`A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`；Candidate04未修改；P3-A Frozen resources 6/6 MATCH；P3-D parent slots保持Collapsed；`IMAGE_API_CALLS=0`。未新增tint、light、atmosphere、mask、blur或shadow，未commit、tag、push、release或deploy。
- Gates：focused`12/12`；locked restore`6/6`；Release x64`0 warnings / 0 errors`；full tests`1788/1788`、failed/skipped 0；UIA`8/8`、offscreen 0；Text Size擷取後恢復152%；staged 0。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_VIEWPORT_CLIPPING_REPAIR=READY_FOR_HUMAN_VIEWPORT_REVIEW_R1`。這是implementation-side review readiness，不是Candidate04 acceptance或final P3-C acceptance。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_VIEWPORT_REVIEW_R1`。
- 取代：本決策完成Decision132的repair-pending狀態，並取代Decision129/130中固定Character viewport作為current placement/integration authority的部分；不取代Decision129的Accepted scale/head/shoulder anchors、Decision131的Candidate04 intrinsic asset QA、Decision115的P3-A baseline或Character Identity Authority。

## TCC-DEC-2026-09-26-134 - Human Viewport Review R1 Revises Intrinsic Bottom-Edge Status

- 日期：2026-09-26；來源：正式Supervisor Human Viewport Review裁決`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_VIEWPORT_REVIEW_R1=REVISE`，並授權唯一的offline／cleanroom Accepted Foreground Occlusion Validation。
- Human verdict：Decision133修復的left artificial cut與right pre-window artificial cut均`VERIFIED_ABSENT`，full Authority silhouette可用，因此P3-VI-053與054維持PASS。另確認High-severity `P3C-VIEWPORT-HVR-002`：Character Authority非透明內容接觸intrinsic 1024×1536 source bottom，Runtime約y=800直接裸露straight horizontal canvas termination。
- Placement custody：Character X/Y、uniform scale、head anchor與shoulder anchor不重開；不得移動人物以藏edge。Character、Character Authority、Candidate04及P3-A均未修改；禁止生成lower body、延伸服裝、非等比例scale、blur、fade或重新引入rectangular viewport。
- Offline validation：Accepted Foreground Accent `1E6B2540...19CF8`為原生1672×941 Accepted Visual partition，精確以`0,0,1672,941`、無move/scale/crop/opacity change疊於current full-silhouette Runtime。346個Authority bottom-edge x-columns中只有121個在y=794–806取得Foreground Alpha>32覆蓋，coverage 34.97%；225 columns未覆蓋，決定性連續裸露區為x=1327–1550。Human-visible horizontal termination仍可辨識。
- Decision：`FOREGROUND_OCCLUSION_VALIDATION=FAIL`、`CHARACTER_INTRINSIC_EDGE_BLOCKER=UNRESOLVED`。不得把Accepted Foreground移到非Authority位置作弊，也不得宣告`PASS_WITH_FORMAL_FOREGROUND_DEPENDENCY`。P3-VI-055降為`PENDING_OCCLUSION_VALIDATION`；新增P3-VI-056 Intrinsic Character Canvas Edge Concealment，current status `FAIL_ACCEPTED_FOREGROUND_INSUFFICIENT`；active catalog為56。
- Evidence：review board SHA-256`182CF8D051A97010BD3BFC1BCEA00F6D1F68DD18256962149B34087384AE0D1E`；offline composite SHA-256`5D4537EC4CE5935F08250C00855DEFF50C612F2A8A33D8C137127E5A9495240E`；6/6 placements aspect-safe、nonuniform failures 0。Hallmark audit為1 critical、0 major、0 minor，critical finding即exposed rectangular Character asset boundary。
- Scope：Review-only cleanroom output；`MainWindow.xaml`、Production、Tests、Character placement/assets及Candidate04皆未修改；`src`／`tests` tree digests維持`88FCD8...EFB0`／`76AF36...EA6F`；`P3D_PRODUCTION_INTEGRATION=NO`、`P3_D_ALLOWED=NO`、`IMAGE_API_CALLS=0`。依scope不執行build/tests，未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_INTRINSIC_EDGE_OCCLUSION_VALIDATION=BLOCKED_UNRESOLVED`。Candidate04 Human Derived Asset Review仍blocked。
- 下一步：`SUPERVISOR_DECISION_REQUIRED`，由Supervisor決定placement、asset canvas或derived-asset處理策略；本輪停止。
- 取代：本決策取代Decision133的P3-VI-055完整PASS與Human Viewport Review pending狀態；不取代其P3-VI-053/054 PASS、accepted X/Y/scale/anchors、Candidate04 intrinsic asset custody、P3-A baseline或Character Identity Authority。

## TCC-DEC-2026-09-26-135 - P3-C Derived Canvas Bottom Extension R1 Prepared

- 日期：2026-09-26；來源：正式Supervisor Decision `DERIVED_CANVAS_EXTENSION_REQUIRED`，以及使用者後續明確新增P3-VI-061 Local Garment Extrapolation Only與P3-VI-062 Silhouette Derivative Continuity。
- Asset decision：建立1024×2048 RGBA Derived Character。`x=0..1023,y=0..1535`最後以Character Identity Authority原像素覆蓋，RGBA pixel hash `869A23B4...C3E71`完全相符；只有`y=1536..2047`為bottom garment continuation。沒有生成新人體、肢體、臉、頭髮、姿勢、主要裙片、主要ribbon或新服裝silhouette language。
- Generation record：正式Image API calls 3；另有1次本機reference-count preflight rejection，未送出API。full-canvas attempt未選用；第一個local output整體因large flare、major ribbon與finished hem而拒絕；受P3-VI-061/062約束的第三次local output為主要延伸來源。為消除source seam，僅使用前一local output的首96 extension rows作局部cloth continuity bridge，再以96 rows smoothstep過渡；原Authority region不受影響。
- Seam/derivative evidence：source與extension boundary各982 Alpha contact columns，Alpha continuity 100%；boundary left/right position delta 0；前64 extension rows總width growth 15px；left slope由`-0.246518`連續到`-0.238636`px/row，right slope保持0；正常尺度未見source/generated horizontal seam或突然外擴。P3-VI-057–062 implementation-side皆PASS。
- Runtime projection：保留Accepted P3-B projection X `1310.1124`、Y `157.89`、uniform scale `0.4229199885`。Derived canvas bottom落在Runtime y `1024.0301`，低於941px physical Window達83.0301px；Character placement與Accepted Foreground position皆未修改。
- Evidence/custody：Extended Canvas SHA-256 `C743D17C...88AE4`；Review Board SHA-256 `1F82E27C...FC101`；6/6 review placements aspect-safe。Character Authority、Candidate04、Accepted Background、Accepted Foreground、Production與Tests保持不變；Image model未公開，記錄為`UNKNOWN_NOT_EXPOSED`。
- Scope/gates：asset preparation only；依授權不執行build/tests。Task-start/final `src`/`tests` tree digests一致，分別為`A959FE7A...19EB7`/`503608B7...414B`；未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_CANVAS_EXTENSION=READY_FOR_HUMAN_EXTENSION_REVIEW_R1`；`EXTENDED_CHARACTER_ACCEPTED=NO`、`CANDIDATE_04_ACCEPTED=NO`、`P3_D_ALLOWED=NO`。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_EXTENSION_REVIEW_R1`。
- 取代：本決策解除Decision134的Supervisor-decision blocker，並以Derived Canvas修復候選取代Accepted Foreground作為intrinsic edge唯一補救路徑；不取代Decision134的Human evidence、Decision133的Accepted placement anchors、Candidate04 custody、P3-A baseline或Character Identity Authority。

## TCC-DEC-2026-09-26-136 - Extension Accepted and Candidate04 Extended Composite Prepared

- 日期：2026-09-26；來源：正式Supervisor Human Decision `TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_EXTENSION_REVIEW_R1=PASS`與授權`TCC_REFERENCE_MASTER_WPF_P3_C_EXTENSION_ACCEPTANCE_AND_DERIVED_COMPOSITE_PREP`。
- Extension acceptance：正式接受`TCC_P3_CHARACTER_EXTENDED_CANVAS_V1_CANDIDATE_01.png`作為Accepted Character Canvas Extension Asset；SHA-256 `C743D17C...88AE4`，Bottom Extension `6E04D397...6FA7`，Character Authority `A42A885A...385D`，dimensions 1024×2048 RGBA。P3-VI-055 PASS、056 `PASS_BY_DERIVED_CANVAS_EXTENSION`、057–062 PASS。此接受不等於Final Character Integration Acceptance。
- Canvas waiver：先前1024×1920僅為generation-risk preference；Human Review確認1024×2048沒有garment redesign、flare、new anatomy或silhouette discontinuity，因此`CANVAS_SIZE_VARIANCE_FROM_SUPERVISOR_PREFERENCE=ACCEPTED`、`RERENDER_REQUIRED=NO`，後續不得僅因1920/2048差異重開此Asset。
- Composite architecture：Candidate04 base `D7059FE3...5848`與atmosphere `EBBDBA07...0A3B`保持byte-identical source files，依原座標在`0,0`作標準Alpha composite；Accepted Extension只供應`y=1536..2047`。Extension Alpha逐像素保持，RGB以Candidate04 `y=1280..1535`建立per-channel gain/bias，另以128-row boundary residual decay消除色調接縫；沒有resize、shift、warp、placement change或Production WPF effect。
- Output/evidence：Extended Candidate SHA-256 `E5B2DE32...18C9`；comparison board SHA-256 `4EFA7B02...2F88`；16/16 placements aspect-safe。Boundary 982 overlap columns，RGB MAE `2.302444`，小於source／extension neighbor約`3.218`；Alpha MAE `2.662933`。Candidate04 integrated top region exact，transparent RGB leakage 0，no Scene raster baked in。
- Offline verdict：identity、face、hair、outfit、pose、garment/color/Alpha continuity、P3-VI-044、049–052與057–062 preparation gates PASS。P3-VI-033–037/041不由offline asset prep宣告PASS，仍需Integrated HWND Runtime Human Review；later Runtime必須移除或重評重複WPF moonlight、shadow、tint與veil以避免double lighting。
- Scope：Production、Tests、Candidate04 sources、Accepted Extension、placement與Foreground皆未修改；`IMAGE_API_CALLS=0`。依asset-only scope不執行build/tests；未commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_DERIVED_CHARACTER_EXTENDED_PREP=READY_FOR_HUMAN_DERIVED_ASSET_REVIEW_R1`；`CANDIDATE_04_ACCEPTED=NO`、`CHARACTER_SCENE_INTEGRATION=NOT_ACCEPTED`、`P3_D_ALLOWED=NO`。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_DERIVED_ASSET_REVIEW_R1`。
- 取代：本決策完成Decision135的Human Extension Review pending狀態，並解除Decision134的intrinsic-bottom-edge blocker；不取代Decision133的Accepted placement anchors、Candidate04 identity custody、P3-A baseline、Character Identity Authority或P3-VI-033–037/041 Runtime ownership。

## TCC-DEC-2026-09-26-137 - R1 Extension Reopened and Semantic Continuity R2 Prepared

- 日期：2026-09-26；來源：正式Supervisor Human Derived Asset Review `TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_DERIVED_ASSET_REVIEW_R1=REVISE`。
- Gate separation：R1的Authority RGBA、pixel seam與Alpha continuity證據仍成立，但不再推導garment visual continuity。原R1 Acceptance與Gate保留為歷史，狀態改為`HISTORICAL_ACCEPTED_THEN_REOPENED`；目前`EXTENDED_CHARACTER_STATUS=REOPENED_FOR_VISUAL_CONTINUITY_REPAIR`。
- R2 construction：以Character Authority底部320 rows作唯一context、下方384 rows作透明generation region，執行1次maximum-conservative ImageGen local fabric continuation。raw output以225個特徵匹配、217個RANSAC inliers回到Authority座標；只保留延伸區，最後將原1024×1536 Authority逐像素覆蓋到1024×1920 canvas。
- Semantic result：R2-01在不知道y座標時無法指出generated boundary，且未出現簡化白裙、換裙、新大面積白布或單體skirt fill。既有left/center/right cloth、diagonal translucent ribbon、gold embroidery、fold complexity與Alpha overlap rhythm持續；R2-02因此不生成。
- Evidence：Extended Canvas SHA-256 `683E7410...ABC93`；Bottom Extension `45F1B528...AE68`；Review Board `5CF027F3...9C1A`。Source region RGBA match YES；seam RGB MAE `3.564494`、Alpha MAE `0.478615`；source/extension boundary width `982/986px`；12/12 evidence placements aspect-safe。
- Placement/scope：P3-B X `1310.1124`、Y `157.89`、uniform scale `0.4229199885`不變；1920 canvas bottom投影Runtime y `969.8964`，低於941px Window `28.8964px`。Candidate04三項hash不變，Production、Tests、Foreground、Background與P3-B placement均未修改；Image API calls `1`；未commit、tag、push、release或deploy。
- Invariants：新增P3-VI-063 Fabric Component Lineage、064 Garment Complexity Continuity、065 No Monolithic Skirt Fill、066 Embroidery Rhythm Continuity、067 Translucency Rhythm Continuity；目前總數67。P3-VI-057–067僅為internal preparation PASS，正式接受仍由Human Extension Review R2持有。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_CANVAS_EXTENSION_R2=READY_FOR_HUMAN_EXTENSION_REVIEW_R2`；`EXTENDED_CHARACTER_ACCEPTED=NO`、`CANDIDATE_04_ACCEPTED=NO`、`P3_D_ALLOWED=NO`。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_EXTENSION_REVIEW_R2`。
- 取代：本決策重開Decision136的R1 current-acceptance interpretation，但不刪除其歷史Acceptance、Gate或技術證據；不取代Character Authority、Accepted P3-B placement、Candidate04 custody、P3-A baseline或P3-VI-033–037/041 Runtime ownership。

## TCC-DEC-2026-09-26-138 - Garment Extension Abandoned, Formal Scene Occlusion Bridge Prepared

- 日期：2026-09-26；來源：正式Supervisor Decision `TCC_REFERENCE_MASTER_WPF_P3_C_GARMENT_EXTENSION_STRATEGY=ABANDONED`。
- Strategy decision：Character Bottom Extension、Canvas Extension與Garment Continuation全部取消。R1/R2 extension assets、extended Characters與review candidates保留但標記`HISTORICAL_REJECTED`；歷史Gates不覆寫。`EXTENSION_ASSETS_ALLOWED_IN_PRODUCTION=NO`、`EXTENDED_CHARACTER_ACCEPTED=NO`。
- Authority decision：Character Identity Authority及byte-identical 1024×1536 Character Working Source重新成為Active Character Geometry Authority。Candidate04三項source保持unmodified且`CANDIDATE_04_ACCEPTED=NO`。P3-B X `1310.1124`、Y `157.89`、uniform scale `0.4229199885`不變。
- Invariant decision：P3-VI-057–067保留歷史文字但active status為`RETIRED_WITH_ABANDONED_STRATEGY`，未來Production/P3-C/P3-D Gates不得要求。P3-VI-053–056保持active；056改由Formal Scene Occlusion解決。新增068 Low-Frequency Foreground Occlusion、069 Foreground Occlusion Must Read As Scene、070 No Density Compensation、071 Character Lower-Body Deemphasis。
- Existing asset proof：Accepted Foreground native authority transform只覆蓋重建後126/344 projected source-bottom columns（36.6279%），x=1328–1545仍暴露，因此符合新low-frequency foreground ImageGen授權條件。
- Candidate decision：ImageGen Candidate01因右側高岩重量與hard-cover風險拒絕；Candidate02降低岩體、拉平輪廓，以單一connected cold gray-blue snow/stone mass進入Human Review。Candidate02 covers 344/344 bottom columns，high-chroma red/pure-white pixels 0/0；intended scene-mask＋UI-safe visible-layer模型在AI Summary/Activity critical rectangles的visible asset Alpha pixels為0/0。
- Evidence：A–E comparison及三個bottom close-ups共9/9 placements aspect-safe，nonuniform failures 0。Candidate02與comparison/evidence/plan/review均為cleanroom artifacts，不是Production deployment；Image API calls `2`。
- Scope：Production、Tests、`MainWindow.xaml`、Character assets、Candidate04、P3-A、P3-B與歷史Gates皆未修改。沒有commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_SCENE_OCCLUSION_BRIDGE_PREP=READY_FOR_HUMAN_SCENE_OCCLUSION_REVIEW`；`NEW_FOREGROUND_ACCEPTED=NO`、`P3_C_FINAL_ACCEPTED=NO`、`P3_D_ALLOWED=NO`。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_SCENE_OCCLUSION_REVIEW`。
- 取代：本決策取代Decision137的Human Extension Review R2 pending與active garment-extension strategy，不刪除或改寫Decision135–137及其歷史evidence/Gates；不取代Character Authority、P3-B placement、P3-A baseline或Candidate04 custody。

## TCC-DEC-2026-09-26-139 - Scene Occlusion Coverage Is Diagnostic and Terrain Topology Is Human-Owned

- 日期：2026-09-26；來源：正式Supervisor Human Scene Occlusion Review `TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_SCENE_OCCLUSION_REVIEW=REVISE`。
- Review finding：Candidate02雖以344/344 column coverage消除source edge，單一平滑連續雪脊仍讀為專用bug-cover；P3-VI-069因此FAIL。100% coverage不得再作Human PASS要求，正式主判準改為`CHARACTER_INTRINSIC_BOTTOM_EDGE=NOT_HUMAN_DETECTABLE`。
- Repair decision：R1-01因仍有連續斜脊內部拒絕；R1-02為最後允許候選。其保留Candidate02右下、冷藍灰、低頻、UI-safe方向，以三個不等高upper terrain masses、兩個深度凹口、暗岩/雪底陰影及23,997px Accepted Foreground overlap建立燈籠基座地形連接。
- Human QA：source bottom edge正常尺度及F close-up不可辨；horizontal snow wall absent、lantern relation、dark rock breaks、red density、UI critical text、Accepted depth rhythm皆PASS。diagnostic coverage為215/344（62.5%），未回填到100%。
- Invariants：新增P3-VI-072 No Coverage-Optimized Foreground Wall及P3-VI-073 Foreground Terrain Attachment，總數73；P3-VI-068–073 R1 preparation皆PASS。
- Evidence：R1-02 SHA-256 `D4A657D8...30E31`；A–I board SHA-256 `27618415...4712`；9/9 placements aspect-safe；new red/pure-white pixels 0/0，critical UI visible Alpha 0/0；Image API calls 2/2。
- Scope：Original Character Authority、P3-B placement、Candidate04、Accepted Background/Foreground、Production、Tests及P3-D均未修改；沒有commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_SCENE_OCCLUSION_BRIDGE_REPAIR_R1=READY_FOR_HUMAN_SCENE_OCCLUSION_REVIEW_R1`；`NEW_FOREGROUND_ACCEPTED=NO`、`CANDIDATE04_ACCEPTED=NO`、`P3_C_FINAL_ACCEPTED=NO`、`P3_D_ALLOWED=NO`。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_SCENE_OCCLUSION_REVIEW_R1`。
- 取代：本決策取代Decision138中Candidate02可直接進入接受審查的視覺判定與100% coverage正向解讀，但不取代garment path retirement、Character Authority、Accepted P3-B placement或歷史asset custody。

## TCC-DEC-2026-09-26-140 - Character Garment Mass Requires an Independent Scene-Composition Gate

- 日期：2026-09-26；來源：正式Supervisor授權`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_GARMENT_MASS_RECOMPOSITION_PREP`，承接Human Scene Occlusion Review R1的`REVISE`與`P3C-GM-HVR-001`。
- Visual decision：R1-02的intrinsic-edge concealment方向可保留，但不得將其等同Character scene integration完成。Character下半部仍有過大的白色／半透明visual mass與完整裙擺trace，因此新增P3-VI-074 Lower Garment Visual Mass Restraint、075 No Full-Garment Read、076 Upper-Body Focal Dominance、077 Occlusion Density Restraint；P3 catalog增至77。
- Composition decision：D candidate只使用P3-A baseline、byte-identical original Character、Accepted Foreground、R1-02 terrain與既有UI panel depth。Terrain以bottom-right為錨點作單一uniform scale 1.45，無micro-repair；AI Summary與Activity panel恢復於Character上方。沒有改Character pixels/Alpha/scale/X/Y/identity/outfit，也沒有生成或設計新garment/scene element。
- Evidence：D candidate SHA-256`CEC23F74...3207A6`；A–J board SHA-256`52DCA940...E8743DD`；10/10 placements aspect-safe，non-uniform failures 0。Upper visible Alpha retained 92.8029%；lower white visual mass reduced 78.6876%；full skirt trace `BROKEN`；intrinsic bottom edge `NOT_HUMAN_DETECTABLE`；critical UI terrain Alpha 0/0。
- Gates：deterministic replay PASS；locked restore 6/6；Release x64 0 warnings/0 errors；full tests 1788/1788，failed/skipped 0；Frozen P3-A resources 6/6；P3 invariant catalog 77 unique/contiguous；staged 0。
- Scope：Garment extension path維持retired；Character Authority、P3-B placement、Candidate04、Accepted Background/Foreground、Production、Tests及P3-D均未修改；`IMAGE_API_CALLS=0`。沒有commit、tag、push、release或deploy。
- 狀態：`TCC_REFERENCE_MASTER_WPF_P3_C_CHARACTER_GARMENT_MASS_RECOMPOSITION_PREP=READY_FOR_HUMAN_GARMENT_MASS_REVIEW`。這是review readiness，不是New Foreground/Terrain/Candidate04/P3-C Final acceptance；上述接受狀態全部為NO，P3-D仍禁止。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_HUMAN_GARMENT_MASS_REVIEW`。
- 取代：本決策取代Decision139的Human Scene Occlusion Review R1 pending狀態與73-invariant count，但不取代其coverage-is-diagnostic原則、garment-extension retirement、Character Authority、Accepted P3-B placement、P3-A baseline或任何歷史asset custody。

## TCC-DEC-2026-09-26-141 - Old Right-Side Building System Rejected for Future Repair

- 日期：2026-09-26；來源：使用者後續明確要求「不要用以前的建築物」。此要求高於先前以舊右側柱體及其材質衍生扶手作bounded depth repair的假設。
- Asset disposition：`TCC_P3_RIGHT_VERTICAL_SCENIC_STRUCTURE_V1_CANDIDATE_01.png`與`SceneArchitecturalBarrierSlot`衍生扶手皆標記為`REJECTED_FOR_FUTURE_REPAIR_EVIDENCE_ONLY`。檔案與Runtime capture保留作`P3C-FVI-HIGH-ARCH-001`稽核證據，不刪除、不偽裝成Accepted authority。
- Prohibition：後續不得透過重用、調色、加粗、補雪、reskin、trace或衍生方式延續舊建築系統；不得把舊柱或舊扶手當新方案的母版。
- Future boundary：若後續正式授權修復，必須建立一套新且一致的建築authority，同時滿足P3-VI-078、079、082、083；但本決策本身不授權ImageGen、Scene generation、Production repair或Runtime wiring。
- Preservation：P3-B Character placement、face/hair focal area、P3-A Glass hierarchy、stepped right edge、Header breathing space、garment-mass reduction、AI Summary／Activity readability、Lantern secondary accent與Plum restraint均不得因未來替換而破壞。
- Status：`OLD_BUILDING_REUSE=FORBIDDEN`、`P3C-FVI-HIGH-ARCH-001=OPEN`、`OVERALL_VISUAL_INTEGRATION_STATUS=REVISE`、`P3_D_ALLOWED=NO`。
- 下一步：`SUPERVISOR_FULL_VISUAL_INTEGRATION_REVIEW`，由Supervisor另行決定新建築authority與施工授權。
- 取代：本決策取代任何將舊右側建築候選或其衍生扶手視為可繼續修補來源的假設；不取代Decision140的garment-mass成果、Character/P3-A/P3-B custody或歷史evidence。

## TCC-DEC-2026-09-27-142 - New Coherent Right-Side Architecture System V2 Prepared

- 日期：2026-09-27；來源：使用者要求「重做一次」，並以推薦範圍確認只重做整套右側建築，保留Character placement、UI、Background、Plum、Lantern、Snowbank。
- New authority：建立`TCC_P3_RIGHT_ARCHITECTURAL_SYSTEM_V2_CANDIDATE_01.png`，1672×941 RGBA，SHA-256 `B5E642A4...A5E10`。其檐、柱、木欄杆、石基、積雪、材質、透視與冷月光屬同一物理系統；不是舊建築的recolour、reskin、trace或derivative。
- Runtime topology：同一透明來源以互斥clip分成rear architecture z50及front barrier z56，Character維持z55，後續terrain z58、plum z59、UI z60、lantern z80不變。此設計使建築可同時位於人物前後，而不以兩個不相干素材冒充同一結構。
- Old-source custody：舊`TCC_P3_RIGHT_VERTICAL_SCENIC_STRUCTURE_V1_CANDIDATE_01.png`仍留作歷史稽核證據，但已從DesktopHost `.csproj`及Runtime XAML引用移除。Architecture tests禁止其重新進入compiled resources或Runtime。
- Preservation：Character source／placement `1310,155,430.08×645.12`、P3-A Glass/UI、Background 02、Accepted Foreground、Plum、Lantern及garment-mass terrain均未改動。
- Evidence：HWND Runtime 1672×941、DPI96、UIA8/8、offscreen0；12/12 comparison placements aspect-safe；asset/runtime/comparison hashes已記錄。`IMAGE_API_CALLS=2`。
- Gates：focused P3 30/30；P1 focused 1/1；locked restore PASS；Release x64 0 warnings/0 errors；full tests 1795/1795。未commit、tag、push、release或deploy。
- Status：`P3C-FVI-HIGH-ARCH-001=REPAIRED_PENDING_SUPERVISOR_VISUAL_REVIEW`；P3-VI-078–083為`CANDIDATE_PASS_PENDING_SUPERVISOR_VISUAL_REVIEW`；`P3_D_ALLOWED=NO`。
- 下一步：唯一為`TCC_REFERENCE_MASTER_WPF_P3_C_NEW_ARCHITECTURE_SYSTEM_V2_HUMAN_VISUAL_REVIEW_R1`。
- 取代：本決策完成Decision141等待replacement authority的狀態，但不撤銷舊建築reuse prohibition、不覆寫歷史audit evidence，也不取代Character/P3-A/P3-B及garment-mass custody。

## TCC-DEC-2026-09-27-143 - Architecture V2-R2 Must Be Space-Planned Before Generation

- 日期：2026-09-27；來源：使用者要求先考慮建築可放入空間、素材前後排版、整個畫面融合度與協調性，再製作建築素材。
- Root cause：V2-R1素材本身有完整右柱，但Runtime把柱體留在Character後層；由於Accepted Character抵達Window右邊界，柱體被hair/clothing/plum/lantern遮蔽。問題是space/depth contract，不是只靠重畫柱子可修復。
- Planning-first rule：建立1672×941 space map，先鎖定face／primary hair與white-fur protected zones，再定義rear eave／inner post、front outer column、lower railing與stone plinth可用區。Asset generation只能在該contract後執行。
- User priority refinement：人物為唯一場景主體；五官身份與冷靜空靈氣質同級最高。使用者提供參考與Production `TCC_P3_CHARACTER_WORKING_SOURCE.png` byte-identical，SHA-256皆為`A42A885A...B6385D`，故禁止以生成母版人物取代正式人物。
- Reconstructable master workflow：先建立整張1672×941構圖母版，再依同一座標拆成透明建築層，最後接回鎖定UI/Character Runtime。母版必須同時考慮Alpha可拆性、Z-order、UI protected zones及Runtime還原，不得只追求單張好看。
- New asset：`TCC_P3_RIGHT_ARCHITECTURAL_SYSTEM_V2_R2_CANDIDATE_02.png`，1672×941 RGBA，SHA-256 `0BE25DF2...A399`；Image API calls 3。它由最新整張母版同座標拆分，未沿用舊建築。
- Runtime split：rear architecture z50 opacity0.62；Character z55 unchanged；compact snow-rock foundation z56 opacity0.56；edge-only column reinforcement z57 opacity0.25；terrain z58、plum z59、UI z60、lantern z80。大型扶手已移除。
- Whole-frame calibration：Candidate01因建築量體與扶手比例過大而淘汰；Candidate02只保留極右屋簷/細柱/雪岩基座。禁止為節省額度或工時翻用舊建築；舊素材只作稽核，設計判斷只看人物參考、Accepted Visual、最新母版與Runtime。
- Evidence：Runtime SHA `FA0B63CD...A22A`；comparison SHA `DD097D9C...740DC`；12/12 aspect-safe；1672×941、DPI96、UIA8/8、offscreen0。
- Gates：focused P3 30/30；locked restore PASS；Release x64 0 warnings/0 errors；full tests 1795/1795。未commit、tag、push、release或deploy。
- Status：`READY_FOR_SUPERVISOR_VISUAL_REVIEW_R2`；formal Human acceptance pending；`P3_D_ALLOWED=NO`。
- 下一步：`TCC_REFERENCE_MASTER_WPF_P3_C_NEW_ARCHITECTURE_SYSTEM_V2_R2_HUMAN_VISUAL_REVIEW`。
- 取代：本決策取代Decision142的V2-R1 review candidate，但不撤銷old-building prohibition、不改Character/P3-A/P3-B custody，也不刪除V2-R1與R2 calibration evidence。

## TCC-DEC-2026-09-27-144 - Latest Master and Exact Character Rebase the WPF Visual Foundation

- 日期：2026-09-27；來源：使用者明確要求以最新 1672×941 圖作母版、先重新製作 WPF 基礎定位，再加入素材，並將 `codex-clipboard-1bcccbae-619d-4d92-adf4-a977967e26eb.png` 綁死為唯一 Character 素材。
- Authority：最新母版 SHA-256 `5C43B4B...566412`；Character user input／cleanroom／Production SHA-256 均為 `17A888CC...258225`，確認 byte-identical。Character 不得重新生成、重繪、替換、任意裁切或非等比例縮放。
- Rebuild boundary：`MainWindow.xaml` 視覺內容與 resource dictionary 從零重建；DesktopHost 只編譯 `Assets/RebuildR1` 的 Scene、Character、Foreground 三項 raster。舊 Scene／Character／Home asset 留存稽核，但不再編譯或引用。
- Runtime topology：new Scene z0；exact Character z10；native WPF glass shells z20；single full-canvas Foreground z35；semantic workspace z49；critical UI content z50。Character 使用 `334.4,188.2,1337.6,752.8` 與 `Stretch=Uniform`，無 Clip／OpacityMask。
- Verification：locked restore PASS；new Foundation／Home／diagnostic focused tests 25/25；Release build 0 warnings/errors；Runtime evidence 1672×941。Full suite 1749/1805 PASS，56 FAIL，故狀態只能是`FOUNDATION_IMPLEMENTED_PENDING_HUMAN_REVIEW_AND_LEGACY_TEST_MIGRATION`。
- Legacy gate：舊 P1/P2/P3 測試仍要求已被本次正式重建取代的節點與資產；另有 dirty-baseline `MainWindow.xaml.cs` 的 Phase6 mutation failure。禁止以 skip、刪測試或降標處理，必須另行授權後遷移／修復。
- Scope：沒有 commit、tag、push、release 或 deploy；staged files 0。舊檔沒有刪除，避免破壞稽核鏈。
- 下一步：Human visual review；若接受，再正式授權 legacy-test migration 與 dirty-baseline Phase6 repair。
- 取代：本決策取代 Decision143 作為 current visual implementation track，但保留 Decision141 的 old-building reuse prohibition 與所有歷史 evidence；不改變 Phase6A sealed business／safety semantics。

## TCC-DEC-2026-09-27-145 - Master Image Reopens Character Facial and Temperament Authority

- 日期：2026-09-27；來源：使用者明確指出目前人物素材的氣質與五官和母版有落差，要求重新生成直到 Human 明確說過關，並指定 `C:/Users/danny/OneDrive/桌面/母.png` 為母版。
- Authority：`母.png` SHA-256 `5C43B4B...566412` 成為本輪人物五官、表情、氣質、月光色調與場景內比例的直接 Human authority。Decision144 的 byte-identical custody仍是既有Production事實，但不再代表Character Human acceptance。
- Acceptance rule：每輪候選只作Human review；在使用者明確說「過關」前，禁止替換Production人物、修改WPF引用或宣告Character accepted。五官身份與空靈氣質同級最高，姿勢／解剖其次，服裝微細節再其次。
- Candidate R1：以母版為唯一reference生成同座標透明人物，SHA-256 `4FE2C3F7...4A4C`。本輪Image API calls 1；檔案位於cleanroom character regeneration區，未進Production。
- Scope：Production、WPF XAML、compiled resources與tests均未修改；build/tests不執行。沒有commit、tag、push、release或deploy；staged files 0。
- Status：`READY_FOR_HUMAN_FACE_AND_ETHEREAL_REVIEW_R1`。
- 下一步：使用者接受Candidate R1，或指出單一最重要落差後生成R2。
- 取代：本決策重開Decision144的Character acceptance interpretation，但不改其WPF foundation、asset custody或legacy-test evidence。

## TCC-DEC-2026-09-27-146 - User-Selected Transparent Character Is Structural Authority; Master Controls Face and Mood

- 日期：2026-09-27；來源：使用者明確要求以 `codex-clipboard-cfa1d03d-f2b7-4ca1-b79d-ed3c1b25bf32.png` 修正唇色與妝，並再次要求參考 `母.png` 生成。
- Authority split：透明人物 SHA-256 `4FE2C3F7...4A4C` 為姿勢、解剖、服裝、Alpha silhouette、畫面位置與右邊界關係的結構權威；`母.png` SHA-256 `5C43B4B...566412` 只作五官、冷玫瑰唇色、淡紅眼妝、低飽和月光膚色與空靈氣質參考。
- Candidate R4：同時引用兩項 authority 重新生成，正式 review 檔 SHA-256 `6CDD54F6...FB52`。Image API 原始輸出為 1671×941；只在左側補 1 px 純透明畫布正規化成 1672×941 RGBA，未縮放人物，右邊界關係不變。原始生成檔 SHA-256 `27637C39...0D93` 另存稽核。
- Acceptance rule：R3/R4皆為cleanroom Human Review candidates；使用者未明確說「過關」前，禁止替換Production、修改WPF reference或宣告accepted。
- Scope：Production、WPF XAML、compiled resources與tests未修改；build/tests不執行。沒有commit、tag、push、release或deploy。
- Status：`READY_FOR_HUMAN_MAKEUP_AND_ETHEREAL_REVIEW_R4`。
- 下一步：使用者接受R4，或指出下一個單一最重要落差。
- 取代：本決策細化Decision145的authority分工與current candidate；不取代Decision144的WPF foundation及歷史custody/evidence。

## TCC-DEC-2026-09-27-147 - R2 Replaces the Background While Locking the Accepted Whole-Frame Baseline

- 日期：2026-09-27；來源：使用者明確指出 R1 Runtime 已大致成立，要求不要把背景重製擴張成 UI／卡片／人物／前景的全面改版，並要求後續所有施工都先考慮整體畫面。
- Baseline authority：使用者提供的 R1 Runtime SHA-256 `7C146140...7B12` 鎖定卡片座標／透明度、Risk 內容配置、人物 0.8 等比例幾何與左右下方前景重量。Global design thinking 定義為所有層一起評估，不代表所有層一起改動。
- R2 mutable plane：主要可變深度面只有 Scene Background。新 Scene `TCC_WPF_REBUILD_SCENE_BASE_R2.png` SHA-256 `D27895EB...0E54`，重建山水與右側木構；右上梅枝必須由建築簷架／支架物理承接，不得從畫框邊緣無根出現。
- Foreground correction：R2 Foreground SHA-256 `5F35A6CE...5B1` 保留基準左右下方雪梅／燈籠量體，只移除舊右上無根梅枝並新增小型左上梅枝。它仍是單一 full-canvas RGBA foreground plane。
- Character custody：使用者指定 `download.png` 與 Production Character SHA-256 均為 `4FE2C3F7...4A4C`，byte-identical；Runtime 保持 `334.4,188.2,1337.6,752.8`、`Stretch=Uniform`、無 Clip／OpacityMask。
- Runtime/evidence：R2 HWND-bound screenshot 1672×941／DPI96／Text Size100% SHA-256 `8F4E1AC1...09E0`；Text Size 已恢復 152%；UIA 8/8、offscreen 0；Scene／Character／Foreground cleanroom-production hashes 全 MATCH。
- Verification：locked restore PASS；R2 focused 12/12；Release x64 0 warnings／0 errors。Full suite 1750/1807 PASS、57 FAIL，集中於 superseded P1/P2/P3 visual contracts 與 dirty-baseline Phase6 mutation surface；禁止以刪測試、skip 或降標處理。
- Status：`BACKGROUND_REPLACED_READY_FOR_HUMAN_WHOLE_FRAME_REVIEW_AND_LEGACY_TEST_MIGRATION`。Human visual acceptance 與 legacy migration 均未自行宣告完成。
- Scope：沒有 commit、tag、push、release 或 deploy；staged files 0。舊資產保留稽核，不再由 R2 Runtime 引用。
- 下一步：`TCC_WPF_REBUILD_FOUNDATION_R2_HUMAN_WHOLE_FRAME_REVIEW`。
- 取代：本決策取代 Decision144 的 R1 current visual implementation interpretation，並結束 Decision145/146 的未接入 Candidate 狀態；不撤銷 old-building reuse prohibition，也不改 Phase6A sealed business／safety semantics。

## TCC-DEC-2026-09-27-148 - Latest Human Correction Reopens Scale and Card Geometry

- 日期：2026-09-27；來源：使用者明確指出「素材、人物都太大，卡片位置也沒修正」，因此 Decision147 的人物／前景比例與卡片幾何鎖定不再成立。
- Character correction：Production 仍使用 byte-identical `download.png`，但 Runtime registration 改為 `521.3,188.7,1153.3,649.1`、`Stretch=Uniform`、無 Clip／OpacityMask，以回到母版可視 footprint。
- Foreground correction：Foreground 改採 compact full-canvas RGBA，SHA-256 `EFEE4302...BF41`；保留左上梅枝與左右下雪梅／燈籠點綴，但降低占比。右上梅枝仍由 Scene 建築簷架物理承接。
- Card correction：12 個 shell 與內容 module 依母版像素邊界同步校正；卡片維持錯落分組，不強制切齊。Risk shell 改為上半部左圓餅／右參數，下半部垂直淡出，右下框線釋放並讓山水連續。
- Evidence：HWND Runtime 1672×941、DPI96、Text Size100% screenshot SHA-256 `A5860879...FEAC`；Text Size 恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；R2 focused 12/12；Release x64 0 warnings／0 errors；full tests 1750/1807 PASS、57 FAIL，仍為 superseded P1/P2/P3 visual contracts 與 dirty-baseline Phase6 mutation surface。
- Status：`VISUAL_SCALE_AND_CARD_GEOMETRY_REPAIRED_READY_FOR_HUMAN_WHOLE_FRAME_REVIEW`。Human acceptance 與 legacy-test migration 仍未自行宣告完成。
- Scope：沒有 commit、tag、push、release 或 deploy；未刪測試、未 skip、未降標；既有 unrelated dirty baseline 保留。
- 取代：本決策只取代 Decision147 的 character geometry、foreground weight、card/Risk preserved interpretation；不取代其 Scene authority、right-plum building anchor、Character byte custody、layer topology或歷史稽核證據。

## TCC-DEC-2026-09-27-149 - Character Continuation and Rooted Plum Accent Refine Whole-Frame Integration

- 日期：2026-09-27；來源：使用者要求右側岩石／梅枝收窄以露出右手與佩劍、人物衣服向下延伸、卡片透明度依母版漸層、右側卡片不得垂直切齊，並連續校正左上梅枝必須由 Trading Permission 卡片長向 Home／Market、具足夠花量與輕微分枝。
- Character custody：正式人物仍使用 SHA-256 `4FE2C3F7...4A4C` 的 accepted RGBA，Runtime geometry `521.3,188.7,1153.3,649.1`、`Stretch=Uniform`、無 Clip／OpacityMask。新增 `TCC_WPF_REBUILD_GARMENT_CONTINUATION_R3.png` SHA-256 `B589FCC8...8273`，只位於 z9、正式人物 z10 後方，用於視窗下緣的局部服裝延續，不覆寫人物五官、肢體或原始 Alpha silhouette。
- Foreground decision：`TCC_WPF_REBUILD_FOREGROUND_R3.png` SHA-256 `FEF558B6...1AC` 以三個互斥可見區域註冊；左右下方維持 compact accent，右側收窄以露出手、腕甲與佩劍。左上卡片梅枝採獨立 Matrix registration，使枝根落在 Trading Permission 卡片、枝向 Home／Market，並在 Runtime 尺寸保留主枝、上方短分枝、下方細枝及增加後的花量。
- Card material：新增 Safety、Primary、Secondary、Auxiliary 四種冷藍灰透明漸層；Glass 保留背景山水可讀性但維持文字對比。Risk 左界為1020、Mental左界977、AI／Activity左界1119，明確保留母版錯落，不以同一直線強制切齊。
- Runtime/evidence：HWND-bound screenshot 1672×941／DPI96／Text Size100% SHA-256 `A0BE5D0D...350A`；Text Size恢復152%；UIA8/8、offscreen0。Image API calls累計11。
- Verification：locked restore PASS；direct R2/R3 contracts 13/13 PASS；Release x64 0 warnings／0 errors；full tests 1750/1807 PASS、57 FAIL，回到既有 superseded P1/P2/P3 visual contracts 與 dirty-baseline Phase6 mutation surface基線。未刪測試、未skip、未降標。
- Status：`VISUAL_INTEGRATION_R3_READY_FOR_HUMAN_WHOLE_FRAME_REVIEW`。Human visual acceptance 與 legacy-test migration仍未自行宣告完成。
- Scope：沒有commit、tag、push、release或deploy；unrelated dirty baseline保留。
- 取代：本決策只取代Decision148的Foreground資產、無garment-continuation、單一面板材質與左上梅枝幾何描述；不取代Scene authority、右上建築錨定、exact Character custody、card master geometry或歷史稽核證據。

## TCC-DEC-2026-09-27-150 - Original Character Authority Enters Runtime by Geometry Only

- 日期：2026-09-27；來源：使用者明確指定直接將 `exec-6c6a6c34-2bcf-4141-96b5-bc2df4e118ae.png` 接入 WPF，並要求先思考自然版面位置，再調整素材。
- Identity authority：使用者指定檔、Production `TCC_P3_CHARACTER_WORKING_SOURCE.png` 與 `TCC_CHARACTER_IDENTITY_AUTHORITY.png` 均為 1024×1536、SHA-256 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D`，byte-identical。故本輪不修改任何人物像素，也不呼叫 Image API。
- Placement decision：採單一 uniform scale `0.52`，Runtime geometry `1200,174,532.48,798.72`。臉部落在右側卡片外、人物保持場景主體但不遮蔽交易資訊；右手與佩劍可辨，衣袖只在外側模組後方形成輕微交錯。
- Clipping rule：Character Image 無 `Clip`、無 `OpacityMask`、無固定矩形 viewport；只允許 1672×941 實體 Window boundary 自然裁切，符合 P3-VI-053。舊 garment continuation 不再註冊或載入 Runtime。
- Depth decision：Scene z0、Character z10、Glass z20、Foreground z35、UI z50。Foreground R3 只作正式前景遮擋與景深收尾，不重畫人物輪廓。
- Evidence：HWND-bound Runtime 1672×941、DPI96、Text Size100% screenshot SHA-256 `9ABE89786D908382EFF8848D302AF261ACBF3023BB481A4A44A9438D88A38F`；Text Size 已恢復 152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；direct-character focused 12/12 PASS；Release x64 0 warnings／0 errors；full tests 1748/1807 PASS、59 FAIL。失敗屬 superseded P1/P2/P3 visual contracts 與既存 Phase6 production/mutation authority surface；未刪測試、未 skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；不得在 Human verdict 前宣告 final accepted。
- Scope：沒有 commit、tag、push、release 或 deploy；未刪除歷史素材，舊 transformed character／garment continuation只保留稽核、不進 Runtime。
- 取代：本決策取代 Decision149 的 garment-continuation Runtime 與 `4FE2C3F7...4A4C` 人物註冊；保留 Scene、Foreground R3、card geometry/material 與梅枝錨定決策。

## TCC-DEC-2026-09-27-151 - Three Independent Clean Foreground Assets Replace R3

- 日期：2026-09-27；來源：使用者判定右下、左下、左上素材髒污，明確要求砍掉重做並參考母版；另要求左上梅枝必須具些微分枝且不得修改前一生成圖，須重新生成。
- Generation rule：三區均以 `母.png` 作構圖、色調、材質參考，從零生成 1672×941 32bpp ARGB 透明素材。左上第一次生成作廢；正式 R4 左上為第二次獨立生成，未編修或沿用第一次生成像素。
- Assets：Left SHA-256 `5A992FA8718BABE3AD25107BC9398D7152360D09B31051CAD50E09796CD8DDC4`；Right `0EEFE22D9A63520547EC2206D7F60895D2F8133F9E3C377919E7EA8E5EB67757`；Top-left `49C3BC16D588BC91DF9CF0A0CAB52112DF85DB5816CB650BDB6418075AD041C6`。
- Runtime placement：Left `0,603.32,600,337.68`；Right `1252,704.62,420,236.38`；Top-left `65,95,170,95.68`。左下保持低矮，右下收窄並露出人物右手／佩劍，左上枝根位於 Trading Permission 左上框線並以主枝加三條短分枝朝 Home／Market 延伸。
- Alpha/depth rule：三張素材各自原生透明，不使用 Runtime `Clip`、`OpacityMask` 或舊 Foreground R3 visibility-region 拼接。三者位於 z35，UI文字保持 z50；人物與卡片幾何不變。
- Evidence：HWND-bound Runtime 1672×941、DPI96、Text Size100% screenshot SHA-256 `949BE933528FBE0510E1473700F7F6042B87BFBD6375F18A9B22EECE3C85B557`；Text Size已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；focused 12/12 PASS；Release x64 0 warnings／0 errors；full tests 1748/1807 PASS、59 FAIL。失敗仍屬 superseded P1/P2/P3 visual contracts 與既存 Phase6 production/mutation authority surface；未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；Image API calls本輪4次，其中正式使用3張，左上第一候選作廢。
- Scope：舊 Foreground R3保留稽核但不再編譯或引用；沒有commit、tag、push、release或deploy。
- 取代：本決策取代Decision150保留Foreground R3的部分；不改其exact Character Authority、placement、physical-window-only clipping、Scene、card geometry/material或right-upper building plum。

## TCC-DEC-2026-09-27-152 - Top-left Plum R5 Is Regenerated from Scratch with Controlled Visual Weight

- 日期：2026-09-27；來源：使用者要求左上角梅枝砍掉重做，先審視視覺重心與延伸比例，禁止以修改舊素材處理。
- Generation rule：正式 R5 只引用 `母.png` 作構圖、色調與材質參考，從零生成新的 1672×941 32bpp ARGB 透明素材；R4 左上資產不作輸入、不編修、不拼補、不延伸。Image API calls 本 R5 為 1。
- Asset：`TCC_WPF_REBUILD_FOREGROUND_TOPLEFT_R5.png` SHA-256 `0F3D4E0760D4D8BE30D4DA58D56ADC856F7E9F4F0DB1EC1F8E36200E478663A0`；R4 左上退出 compiled/runtime resource，保留歷史稽核。
- Composition decision：Runtime placement `25,95,210,118.20`。根部與中段承擔主要重量並位於 Trading Permission 左上；單一 S 形主枝向 Home／Market 延伸且逐步變細；上、下短分枝與末端細枝保持次要，不形成第二大型輪廓；Command Center 主標不被遮擋。
- Scope：左下 R4、右下 R4、Character、Scene、卡片幾何／材質與 right-upper building plum 均未變更；新素材無 Runtime `Clip` 或 `OpacityMask`。
- Evidence：HWND-bound Runtime 1672×941、DPI96、Text Size100% screenshot SHA-256 `879390731719543D1B3E74724AEB6B48BFC05702A0A6DFDE77E296ED6687AD0B`；Text Size 已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；focused 12/12 PASS；Release x64 0 warnings／0 errors；full tests 1748/1807 PASS、59 FAIL，維持既有 superseded P1/P2/P3 visual contracts 與 Phase6 authority baseline；未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；不得在 Human verdict 前宣告 final accepted。
- Scope guard：沒有 commit、tag、push、release或deploy；staged files 0；unrelated dirty baseline 保留。
- 取代：本決策只取代 Decision151 的左上 R4 資產、hash 與 placement；其左下／右下資產、Character／Scene／card 決策繼續有效。

## TCC-DEC-2026-09-27-153 - Compact R6 Plum Keeps Its Mass on the Trading Permission Card

- 日期：2026-09-27；來源：使用者判定 R5 左上梅枝過大，要求砍掉重新生成；之後撤回對 R6「一樣太大」的暫時判斷並要求繼續現有工作。
- Generation rule：R6 只引用 `母.png`，從零生成新的 1672×941 32bpp ARGB 透明素材；R5 不作輸入、不縮放、不修改、不拼補。Image API calls 本 R6 為 1。
- Asset：`TCC_WPF_REBUILD_FOREGROUND_TOPLEFT_R6.png` SHA-256 `88FC5510F21A16E441FAC8167E93CC08D5C1CA762151720FD8C8EE26C86709C8`；R5 左上退出 compiled/runtime resource並保留歷史稽核。
- Composition decision：Image geometry `80,75,650,365.88` 保持 1672:941 aspect ratio；Alpha>32 的來源枝體 bounds 為 226×195，換算 Runtime 可見 footprint 約 88×76 px。主要量體留在 Trading Permission 左上，只有細端稍微延伸至 Home／Market；Command Center 主標不受遮擋。
- Reduction：R5 Runtime 可見 footprint 約194×99；R6可見面積約降低65%，由跨區枝條降為第三層級卡片點綴。
- Scope：左下 R4、右下 R4、Character、Scene、卡片幾何／材質與 right-upper building plum 均未變更；新素材無 Runtime `Clip` 或 `OpacityMask`。
- Evidence：HWND-bound Runtime 1672×941、DPI96、Text Size100% screenshot SHA-256 `112BEC344F55787F20FA70874DFDC69C725786C0CFB40E3762D5E8E34F58AE0D`；Text Size 已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；focused 12/12 PASS；Release x64 0 warnings／0 errors；full tests 1748/1807 PASS、59 FAIL，失敗集合與 R5 相同；未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；不得在 Human verdict 前宣告 final accepted。
- Scope guard：沒有 commit、tag、push、release或deploy；staged files 0；unrelated dirty baseline 保留。
- 取代：本決策只取代 Decision152 的 R5 左上資產、hash、placement與visual mass；其左下／右下、Character／Scene／card決策繼續有效。

## TCC-DEC-2026-09-27-154 - Upper-left Accent Is Removed and Right-bottom Scenery Is Rebuilt from Scratch

- 日期：2026-09-27；來源：使用者明確要求拿掉左上點綴，並依母版尺寸與景深重做右下造景；後續再明確要求「直接重做，不要用改的」。
- Generation rule：右下 R5 只引用 `母.png` 作視覺比例、材質、色調與景深參考，從零生成新的 1672×941 32bpp ARGB 透明素材。右下 R4 不作生成輸入、不編修、不延伸、不拼補。Image API calls 本 R7 為 1，cleanroom 累計 7。
- Asset：`TCC_WPF_REBUILD_FOREGROUND_RIGHT_R5.png` SHA-256 `1F164687D977962DB30D8ACC74A37E79350EE710663F151283F635C3757A060F`；右下 R4 與左上 R6 均退出 compiled/runtime resources並保留歷史稽核。
- Composition decision：Image geometry `869.44,489.32,802.56,451.68`，維持 1672:941 aspect ratio；Alpha>32 的來源 bounds 為 `928,189-1671,940`，換算 Runtime 可見 footprint 約 `1315,580-1672,941`。量體依母版限制在右下角，不擴張成大型基座。
- Depth decision：最靠鏡頭的雪梅／積雪保持柔焦，中景暖燈籠為清晰焦點，後方枝條與岩面降低對比並受冷霧污染；人物右手與劍柄完整可見，下段佩劍只由正式前景自然遮擋。
- Scope：左下 R4、Character、Scene、卡片幾何／材質與 right-upper building plum 均未變更；右下新素材無 Runtime `Clip` 或 `OpacityMask`；左上點綴為 `NOT_PRESENT`。
- Evidence：HWND-bound Runtime 1672×941／DPI96／Text Size100% screenshot SHA-256 `7975233FB55DF19D62B05FBF2A0EB7A867E0C8F9C4AC23C358A5516919EA978A`；Text Size 已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；focused 12/12 PASS；Release x64 0 warnings／0 errors；full tests 1748/1807 PASS、59 FAIL，維持既有 superseded P1/P2/P3 visual contracts 與 Phase6 authority baseline；未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；不得在 Human verdict 前宣告 final accepted。
- Scope guard：沒有 commit、tag、push、release或deploy；staged files 0；unrelated dirty baseline 保留。
- 取代：本決策移除 Decision153 的左上 R6 Runtime 並取代 Decision151 的右下 R4 資產、hash、placement與depth描述；其左下、exact Character Authority、Scene與card決策繼續有效。

## TCC-DEC-2026-09-27-155 - Right-side Cards Restore the Master's Stepped Extents

- 日期：2026-09-27；來源：使用者指出最右側四張卡片不應垂直切齊，要求參考母版修正。
- Geometry decision：`Major Alerts` 維持 x-right 1318；`Capital / Risk Terrain` 由 width 299 擴至336、x-right 1356；`AI Summary` 與 `Activity` 由 width 200 擴至247、x-right 1366。右緣關係為 `Safety < Risk < AI/Activity`，不再形成全高單一直線。
- Risk continuity：Risk 的 top/right partial outline 同步由299改為336；下半部 Scene blend與右下框線釋放仍保留。圓餅、參數與文案的內部錨點不移動。
- Depth decision：Character 由 z10 改為 z30，只調整前後關係，不改任何人物像素、Alpha、位置、尺寸或等比例縮放。卡片材質 shell 保持 z20，人物自然遮住伸入接觸區的框線／玻璃；Foreground 保持 z35，UI critical content 保持 z50，確保內容可讀。
- Preserved scope：Scene、Character Authority、左右下前景、卡片 Y／高度、其他卡片幾何、透明材質 token與功能語意均未修改；Image API calls 0。
- Evidence：HWND-bound Runtime 1672×941／DPI96／Text Size100% screenshot SHA-256 `2D423BDA5C921FB953656A9072CAC749DAFF8839CB9B5B373FD76C4B7A69B106`；Text Size已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；focused 13/13 PASS；Release x64 0 warnings／0 errors；full tests1749/1808 PASS、59 FAIL，失敗集合維持既有 superseded visual contracts與Phase6 authority baseline；新增right-edge test PASS。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；沒有commit、tag、push、release或deploy，staged files 0。
- 取代：本決策取代 Decision154「卡片幾何不變」的描述；Decision154的right-bottom scenery、top-left removal、Character pixel custody與其他scope繼續有效。

## TCC-DEC-2026-09-27-156 - R9 Trials All Card Layers in Front of the Character

- 日期：2026-09-27；來源：使用者明確要求「卡片都壓在人物前面試試」。
- Trial decision：Character `Panel.ZIndex` 由30退至10；所有 card material shells 維持 z20，Foreground維持z35，card content維持z50。因此卡片材質與文字都位於人物前方。
- Geometry custody：Decision155建立的三段式右緣完全保留：Major Alerts 1318、Risk Terrain 1356、AI Summary／Activity 1366；沒有重新垂直切齊。
- Character custody：人物 source、SHA、Alpha、`1200,174,532.48,798.72`、`Stretch=Uniform`、無Clip／OpacityMask全部不變；本輪只改Z-order。
- Evidence：HWND-bound Runtime 1672×941／DPI96／Text Size100% screenshot SHA-256 `5DB4C49500CEF95F9C9962FE42A56C6F9F5C27435600A9D89651CB4ECC98F0E1`；Text Size已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；focused13/13 PASS；Release x64 0 warnings／0 errors；full tests1749/1808 PASS、59 FAIL，維持既有baseline failure集合；Image API calls0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；R8人物在材質前方證據保留供比較，R9為目前Runtime。沒有commit、tag、push、release或deploy，staged files0。
- 取代：本決策只取代Decision155的Character/Card shell前後關係；其right-edge geometry與其他custody繼續有效。

## TCC-DEC-2026-09-27-157 - R10 Calibrates Every Card Independently and Nudges the Character Right

- 日期：2026-09-27；來源：使用者要求逐一分析母版每張卡片透明度、修正 WPF，並補充人物只需稍微向右。
- Analysis rule：`母.png` 為扁平合成 PNG，沒有可直接讀取的原始 Alpha；因此透明度以背景紋理保留量、局部對比衰減、文字密度及上下退場速度推估有效 WPF overlay strength，沒有把估算冒充設計檔原值。
- Material decision：Trading Permission、Total Risk、Current Positions、Major Alerts、Observation、Market Anchor、Positions、Priorities、Mental State、AI Summary、Activity 各自使用獨立 vertical gradient；Risk Terrain 使用 `83.1% → 74.9% → 40.8% → 0%` 四段消隱。主卡加深、下排保持 secondary、Risk 右下與底邊繼續釋放。
- Border decision：Safety、Primary、Secondary、Auxiliary 四種框線依卡片角色分流；Risk 只保留上、左與右側前224px框線，不恢復完整矩形。
- Character decision：Character `Canvas.Left` 由1200微移至1212；Top174、532.48×798.72、Uniform、source hash、Alpha、無Clip／OpacityMask均不變。只允許Window physical boundary自然裁切。
- Preserved scope：R9卡片位於人物前方、R8三段式右緣、Scene、左右下前景、上右建築梅枝、卡片內容與功能語意全部保留；Image API calls 0。
- Evidence：HWND-bound Runtime 1672×941／DPI96／Text Size100% screenshot SHA-256 `234AC8CBBD6E13FFD8600D60AA56BA24F4AB193453505070FD757162F09153CA`；Text Size已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore PASS；focused13/13 PASS；Release x64 0 warnings／0 errors；full tests1749/1808 PASS、59 FAIL，維持既有 superseded visual contracts 與 Phase6 authority baseline；未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；沒有commit、tag、push、release或deploy，staged files0。
- 取代：本決策只取代Decision156之前的共用四組 card material 與人物x1200幾何；Decision156的卡片／人物深度關係及Decision155的right-edge geometry繼續有效。

## TCC-DEC-2026-09-27-158 - R11 Restores Full-Surface Opacity Hierarchy and Low-Alpha Card Junctions

- 日期：2026-09-27；來源：使用者要求重新仔細分析置頂列、側邊欄、全部卡片的透明度，並明確指出母版卡片連接處也保留透明材質。
- Analysis rule：母版是扁平 PNG，沒有原始 layer Alpha；R11 仍以背景紋理保留、局部對比衰減、邊框強度與退場方向推估有效 WPF overlay，不把估算冒充設計檔原值。
- Chrome/navigation/header：Top chrome 改為80.8%→76.1%→71.4%，Navigation 82.4%→76.9%→69.8%，Header horizontal scrim 46.3%→29.0%→8.6%；移除 Header 額外 `Opacity=0.48`，避免 alpha 重複相乘。Workspace selector、Command search、selected Home、chart inset 分別採27.5%、33.3%、36.1%、35.3%獨立 token。
- Card material：11 張 resource card gradients 再微調；Risk 保持四段式84.7%→76.9%→44.7%→0%消隱。卡片位置、尺寸、R8三段式右緣與 R9 card-in-front depth均不變。
- Junction decision：新增11個低 Alpha connector，填入原本7–8px的卡片接縫。Safety、Primary、Secondary、Row connectors中心最低 alpha依序為21.2%、18.0%、15.7%、17.3%，兩端再回升以銜接鄰卡；`Panel.ZIndex=19`，只恢復母版的連續玻璃，不覆蓋 z20 card shell 或 z50 content。
- Character custody：人物仍為 x1212、y174、532.48×798.72、Uniform、無Clip／OpacityMask；source與authority SHA `A42A885A...B6385D` byte-identical，沒有修改任何人物像素或Alpha。
- Evidence：HWND-bound Runtime 1672×941／DPI96／Text Size100% screenshot SHA-256 `669499D4A20B39F381EB06EDB01A46C93A9215638F4FAC850BECF0B62F429B37`；Text Size已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore6/6 PASS；focused15/15 PASS；Release x64 0 warnings／0 errors；full tests1751/1810 PASS、59 FAIL。59項仍屬既有 superseded visual contracts與Phase6 dirty-baseline authority surface，R11沒有新增 regression；未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；Image API calls本任務0；沒有commit、tag、push、release或deploy，staged files0。
- 取代：本決策取代Decision157的R10透明度數值與「接縫為 scene-clear gutter」的隱含狀態；Decision157的人物x1212、Decision156的深度關係及Decision155的right-edge geometry繼續有效。

## TCC-DEC-2026-09-27-159 - R12 Reconstructs Card-Specific Internal Glass Topologies

- 日期：2026-09-27；來源：使用者指出母版部分卡片具有雙層／多層結構與多變透明度，要求先嚴密分析母版再施工。
- Analysis rule：母版為扁平PNG，R12以內部邊界連續性、背景紋理衰減、局部對比與功能分組逐卡判讀，不宣稱可還原原始設計圖層或Alpha。先建立正式R12 analysis artifact後才修改WPF。
- Topology decision：四張Safety卡保持單層。Observation新增4層；Market新增4個shell-level層並保留既有chart inset作第五層；Risk、Positions、Priorities、Mental、AI、Activity各新增2層，共20個新的z21 internal surfaces。每張卡使用符合自身功能的tab/data/footer/analysis/focus/timeline topology，不套用共同雙層模板。
- Opacity decision：Header、Control、Data、Footer、Selected、Analysis、Focus、SceneTransition分別使用獨立brush；internal strength低於parent shell。第一次Runtime發現Risk與Mental硬矩形過度明顯，正式R12移除其inner border，只保留分析密度與radial focus漸層。
- Depth decision：Scene z0 → Character z10 → Connector z19 → Card shell z20 → Internal layer z21 → Foreground z35 → Card content z50。Internal layers全部`IsHitTestVisible=False`，不取代live controls或更改產品語意。
- Custody：所有card geometry、R11接縫、R8三段式右緣、Character `1212,174,532.48,798.72`、source pixels、Alpha、無Clip／OpacityMask全部不變；Image API calls本任務0。
- Evidence：HWND-bound Runtime 1672×941／DPI96／Text Size100% screenshot SHA-256 `3B1BF5835606F04F28F41991198CE5984FEAD56D341B0DD5CAE508904C37B0D9`；Text Size已恢復152%；UIA8/8、offscreen0。
- Verification：locked restore6/6 PASS；focused16/16 PASS；Release x64 0 warnings／0 errors；full tests1752/1811 PASS、59 FAIL。59項仍為既有superseded visual contracts與Phase6 dirty-baseline authority surface；R12未新增regression，未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW`；沒有commit、tag、push、release或deploy，staged files0。
- 取代：本決策擴充Decision158只處理outer surface與junction的材料模型；Decision158的R11透明度、connector system，Decision157的人物x1212及Decision155的right-edge geometry繼續有效。

## TCC-DEC-2026-09-27-160 - Strata R1 Replaces the Prior Runtime with a Two-authority From-scratch System

- 日期：2026-09-27；來源：使用者明確指出不得偷用舊資產，並補充唯一應使用的是指定人物圖與冰階水墨圖。
- Authority decision：人物圖只定義角色身份、五官、收斂空靈氣質、白衣金紋與佩劍語言；冰階水墨圖只定義冰黑岩層、水墨負空間、霧氣、景深與少量紅梅色彩。舊 WPF Runtime、舊 master、舊卡片拓撲、舊背景、舊前景及其他歷史資產都不是設計或生成輸入。
- Gate decision：先完成零 raster image 的 Asset-free Runtime，鎖定 floating rail、single control spine、dominant observatory、continuous decision ledger、borderless risk lens 與右側角色走廊，通過 Runtime 視覺檢查後才允許生成場景。
- Generation decision：以兩張 authority reference 一次生成新的 1672×941 全畫面場景；角色與冰岩共用光線、透視、接觸陰影與霧氣，禁止 collage、cutout、halo、UI、文字、建築與舊 dashboard 構圖。正式資產 SHA-256 `C71D4A0CFECD01553128BFE3E7E6DB8DE1550FDB5D36F2E613B6D0957C039EE2`。
- Runtime decision：DesktopHost 只編譯一張 `Assets/StrataR1/TCC_STRATA_R1_MASTER_SCENE_V1.png`；舊 RebuildR2、Character 與 AutonomousR1 scene resource declarations 全部退出 project。正式 WPF 使用一張底層 Image，不使用人物／造景拼接層。
- Validation：locked restore PASS；focused 9/9 PASS；Release x64 0 warnings／0 errors；Frozen P3-A resources 6/6 MATCH；Runtime 1672×941、DPI96、UIA8/8、offscreen0；maximize/restore PASS；Text Size152% 為可捲動 conditional PASS。完整 regression 1734/1820 PASS、86 FAIL，失敗集中於被本決策取代的歷史 P1/P2/P3/RebuildR2 視覺與 asset custody contracts。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；未刪測試、未skip、未降標；沒有 commit、tag、push、release或deploy。
- 取代：本決策取代 Decision151–159 對目前 Runtime 視覺、資產、卡片幾何、人物位置、透明度、接縫、內部分層與深度關係的所有描述；這些紀錄保留為歷史稽核，不再是現行 Runtime authority。

## TCC-DEC-2026-09-27-161 - The User-selected Strata Observatory Frame Replaces Strata R1 as Visual Authority

- 日期：2026-09-27；來源：使用者明確要求「全部砍掉重來」，先完美復刻新提供的 1672×941 WPF，並逐輪自主驗收。
- Authority decision：最新提供的 Strata Observatory 畫面是目前唯一視覺權威，管轄 top chrome、98px navigation rail、header、Safety Core、Market Overview、Watchlist、底部三欄、透明度、框線、LOGO、字體層級與角色走廊。Decision160 的場景、版型與 Home 專屬資產不再是 Runtime 輸入。
- Asset decision：從新參考圖生成一張無 UI clean plate，只保留雪山、月色、紅梅與右側人物；正式資產 `Assets/StrataObservatory/StrataObservatory.CleanPlate.png` SHA-256 `5020FEFB89FD9EECB03BF58816002A5D6B748A541F4DEC7971B662434482ADD1`。所有 UI、文字、向量圖示、LOGO、框線與圖表空狀態都由 WPF 原生繪製，不把扁平截圖當 UI 背景。
- Geometry decision：採用參考座標系：top chrome 52px；rail `6,52,98,889`；header `104,52,1154,186`；Market Overview `126,239,870,404`；Watchlist `1001,239,251,404`；底部三欄 `126/492/879,654`。Safety Core 為 640×101 並落在 header 右側。
- Material and type decision：panel `#BF0B1821`、border `#72829BA9`；Georgia 負責品牌與 editorial headings，Segoe UI Variable Text 負責介面文字，Cascadia Mono 負責 numeric/status。高對比模式隱藏場景並映射到 SystemColors。
- Truth decision：只復刻參考圖的視覺結構，不採用其中 ENABLED、即時價格、風險百分比或成功宣稱。Runtime 維持 OFFLINE／UNKNOWN／Read only／No execution API。
- Iteration evidence：Iteration01 因 125% Windows DPI 造成 25% 放大與裁切而退回；移除外層 MinWidth/MinHeight 後，Iteration02 完整映射；Iteration03 校正 top-bar 起點與 HOME 116px selected field 後通過自主幾何驗收。正式 Runtime screenshot SHA-256 `F868B87153175D2566770B2C0ED61AE5C4C97EA2781AFE4A1A17E4F266D18493`。
- Verification：locked restore PASS；focused 6/6 PASS；Release x64 0 warnings／0 errors；UIA 11/11、offscreen 0；完整 regression 1729/1825 PASS、96 FAIL。96項集中於被此決策取代的歷史視覺／asset custody contracts及既有 Phase 6 dirty-baseline authority surface；未刪測試、未skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release或deploy，staged files 0。
- 取代：本決策取代 Decision160 對現行 Runtime 場景、版型、資產與視覺 QA 的描述；Decision160 保留為歷史稽核。

## TCC-DEC-2026-09-27-162 - Two-source Background Replaces the Screenshot-derived Clean Plate

- 日期：2026-09-27；來源：使用者明確要求以本輪指定的人物圖與冰階水墨圖重新生成適合現行 WPF 的背景，並禁止使用任何舊資產。
- Authority decision：背景生成只允許兩張使用者指定圖像：人物圖定義角色身份、白衣金紋、長黑髮與兵器語言；冰階水墨圖定義黑墨負空間、冰岩、霧氣、紅梅與冷色景深。舊 clean plate、歷史 Runtime screenshot、其他背景或前景資產都不得成為生成或合成輸入。
- Iteration decision：第一版雖為本輪全新生成，但人物從約 x1040 開始、占用 WPF 區域過多，因此退回。第二版僅使用第一版新生成結果與本輪人物 authority 做定向修正，將人物縮至約原比例 65% 並移入最右側走廊，保留左側與中央的低對比 UI-safe 冰墨空間。
- Asset decision：現行產品資產為 `Assets/StrataObservatory/StrataObservatory.TwoSource.R1.png`，SHA-256 `1E357CB26F5DC06A8F7EB9DAB1537E5ECDF5F5E8696594E0C1F3CB5A6DD8B3F9`。舊 `StrataObservatory.CleanPlate.png` 已刪除，並從 XAML、project resource declaration 與正向測試契約退出。
- Runtime decision：Strata Observatory WPF 的 top chrome、navigation rail、Safety Core、Market Overview、Watchlist、底部三欄、原生文字、圖示、LOGO、框線與功能語意均不變；本決策只替換最底層 raster scene。
- Evidence：1672×941 Runtime screenshot SHA-256 `5CE364E59E651BA644CEEB198DF9131B02FDD9EB62673EFA9D859F1D22CF83E1`；人物完整落在右側角色走廊，左側主要工作區維持低對比可讀性。
- Verification：locked restore PASS；focused 6/6 PASS；DesktopHost 與 solution Release x64 build 均為 0 warnings／0 errors；full regression 1729/1825 PASS、96 FAIL。96 項維持 superseded historical visual/resource contracts 與既有 Phase 6 authority surface；未刪測試、未 skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy，staged files 0。
- 取代：本決策只取代 Decision161 的 clean-plate 資產來源、檔名、hash 與背景 Runtime evidence；Decision161 的 WPF 幾何、材質、字體、功能真值與 visual authority 規則繼續有效。

## TCC-DEC-2026-09-27-163 - R3 Restores the Master-scale Character and Protects the Face Corridor

- 日期：2026-09-27；來源：使用者退回 Decision162／R1，明確指出人物太小並要求重來。
- Authority preservation：本輪仍只使用兩張使用者指定 authority image。R2 是從兩張原始 authority image 全新生成；R3 的唯一 edit inputs 是本輪新生成的 R2 與同一人物 authority。歷史 clean plate、R1、其他專案背景、Runtime screenshot 或舊前景都不是生成輸入。
- Scale decision：人物恢復為母版級右側主視覺，約占畫面右側 28–32%，頭頂接近 y=80–100，衣袍自然延伸到底部與右邊界；不再沿用 R1 的小人物尺度。
- Runtime rejection：R2 單獨底圖的尺寸雖符合要求，但 1672×941 Runtime 顯示人物偏左約 80–110px，Safety／Watchlist 的 x=1252 邊界壓到臉部，因此 R2 自主驗收不通過，沒有成為最終資產。
- R3 correction：維持 R2 的人物尺寸、姿勢、服裝、光線與場景，只將完整人物向右移並自然補齊空出的山霧。R3 Runtime 中臉部完整位於 x=1252 右側，Watchlist 只覆蓋肩衣／髮絲，主 Market 區沒有被人物侵入。
- Asset decision：現行產品只引用 `Assets/StrataObservatory/StrataObservatory.TwoSource.R3.png`，SHA-256 `CDF464FD58AFEE165352C74DD79BE8BD799045620792CAB9724BADF24EC86911`。R1、R2 及 `StrataObservatory.CleanPlate.png` 均已從產品資產目錄移除。
- Evidence：HWND-bound Runtime 1672×941，xaml runtime errors 0，screenshot SHA-256 `04D68AEFED83A330F0A5166EFC5040C23C97DDD6A12162689FC862F02171DC53`。
- Verification：locked restore PASS；focused R3 identity 6/6 PASS；DesktopHost 與 solution Release x64 build 均為 0 warnings／0 errors；full regression 1729/1825 PASS、96 FAIL；broad Frozen-name filter 5/10 PASS，5 項為 superseded P3 character/placement/garment Runtime locks。R3 自身 hash lock 通過；未刪測試、未 skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy，staged files 0。
- 取代：本決策取代 Decision162 對現行背景資產、人物尺度、Runtime evidence 與 next action 的描述；Decision161 的 WPF 幾何、材質、字體及功能真值繼續有效。

## TCC-DEC-2026-09-27-164 - R4 Slightly Reduces Character Occupancy and Moves Her Right

- 日期：2026-09-27；來源：使用者指出 R3 人物占比仍太大，要求「縮小一點點不要太多，往右移一些」。
- Transform decision：以 R3 為當輪 edit authority，完整人物群組（臉、髮、髮飾、飄帶、白裘、雙手、衣袍、流蘇與兵器）等比例縮至約 92%，再整體向右平移約 55px；頭頂仍維持約 y=85–100，沒有向下沉或改變視線、姿勢、服裝與身份。
- Scene custody：雪山、月色、雲霧、河谷、岩台、紅梅、冷色光線及左中 UI-safe 空間保持原構圖；人物左側騰出的窄帶以同一場景山霧自然補齊。WPF 幾何、透明度、字體、框線、LOGO 與所有功能真值均未修改。
- Asset decision：現行產品只引用 `Assets/StrataObservatory/StrataObservatory.TwoSource.R4.png`，1672×941，SHA-256 `A5279D85EB7324982703BD757556938D3DFE8267E62AC9E893963156878A0804`。R3 已從產品資產目錄移除，但原生成來源仍可復原。
- Runtime evidence：HWND-bound Runtime 1672×941，xaml runtime errors 0，screenshot SHA-256 `7FC432A043F086BFD73B17655EB1A726525413D7B9452DDD71E749ACE03E3681`。人物臉部與 x=1252 Watchlist 邊界保持清楚間距，人物仍保有右側主視覺份量。
- Verification：locked restore PASS；focused R4 identity 6/6 PASS；DesktopHost 與 solution Release x64 build 均為 0 warnings／0 errors；full regression 1729/1825 PASS、96 FAIL；broad Frozen-name filter 5/10 PASS。失敗仍為 superseded historical visual/resource contracts 與既有 Phase 6 authority surface；未刪測試、未 skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy，staged files 0。
- 取代：本決策只取代 Decision163 對現行背景資產、人物尺度／位置、hash、Runtime evidence 與 next action 的描述；Decision161 的 WPF 幾何、材質、字體及功能真值繼續有效。

## TCC-DEC-2026-09-28-165 - Master-detail R3 Refines Native WPF Materials Without Moving the Accepted Scene

- 日期：2026-09-28；來源：使用者在接受 R4 人物尺度與位置後，指定以最新母版繼續精修全部 WPF 細節並開始施工。
- Locked scope：`StrataObservatory.TwoSource.R4.png`、人物占比／位置、1672×941 外框幾何、卡片座標與 OFFLINE／UNKNOWN／Read only／No execution API 功能真值全部鎖定。本輪不得重新生成背景、恢復舊資產或複製母版中的虛構即時市場狀態。
- Material decision：top chrome、navigation、primary、secondary、safety、header field、chart inset、selected surface 與 footer band 改為各自的原生 WPF gradient/token；主卡、次卡與 Safety 卡使用不同 surface style，框線與文字色階同步收斂，避免所有面板共用單一半透明黑底。
- Component decision：Mental State 由規則虛線圓改為三段不規則 cubic hand-brush path 加低對比內環；三張底部卡 footer row 由 34px 調為 44px 並上移 6px，footer band 降低對比。R2 曾把底部標題縮得過小，Runtime 退回後 R3 恢復 `Strata.PanelTitle` 正常層級。
- Accessibility decision：把原先不合法的 raw `ControlTemplate` focus resource 改為以 `Control` 為 TargetType 的 `Style`，保留鍵盤焦點框；disabled navigation/tab opacity 提升至 0.68，改善離線狀態辨識，不解鎖任何功能。
- Runtime evidence：R1 因 title/footer/ring 比例退回；R2 因底部標題過小退回；R3 1672×941 Runtime screenshot SHA-256 `466D91891FD1B323D9898B8980A4BB0960291A28F4E02FBA98194968ED967E35`，1280×720 screenshot SHA-256 `49437AFEA9D87E902655903A2A1F652E75074982198B4B29C3A4AF58B4279E9D`，兩者 xaml runtime errors 0。R4 raster SHA-256 仍為 `A5279D85EB7324982703BD757556938D3DFE8267E62AC9E893963156878A0804`。
- Verification：locked restore PASS；focused 6/6 PASS；DesktopHost 與 solution Release x64 build 均 0 warnings／0 errors；full regression 1729/1825 PASS、96 FAIL；Frozen-name filter 5/10 PASS。96／5 項失敗維持 superseded historical visual/resource contracts 與既有 Phase 6 authority surface，未刪測試、未 skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy，staged files 0。
- 取代：本決策擴充 Decision161 的初始材質／字體實作細節，但不取代其幾何與產品真值；Decision164 的 R4 背景、人物尺度／位置與資產 custody 完整保留。

## TCC-DEC-2026-09-28-166 - Master-detail R4 Adds Residual Hierarchy Without Fabricating Market Data

- 日期：2026-09-28；來源：使用者授權第二次精修、補足或其他必要細節。
- Locked scope：Decision164 的 R4 raster、人物尺度／位置、外框座標與 Decision165 的 operational-truth boundary 全部保留；不以假價格、假時間或假健康狀態填補母版資訊密度。
- Typography decision：新增獨立 `Strata.SectionTitle`，Market Overview 使用 21.5px／30px line-height；下排 `Strata.PanelTitle` 微調為 14.5px／21px，使主區標題與工作卡標題不再共用同一角色。
- Selection decision：BTC／1D 使用專屬 disabled-selected template，以暗色 selected surface、安靜外框及 2px 冰藍底線表達目前視圖；停用狀態仍不提供任何互動或交易能力。Watchlist 分頁同步加入 selected surface 與底線。
- Detail decision：主卡、Safety 與底部三卡增加低 Alpha 頂緣 glint；Watchlist 五列增加低 Alpha 分隔線。Pass1 曾加入完整垂直 chart grid，但 Runtime 顯示其工程感偏離母版的水墨留白，因此正式 R4 撤回，只保留低對比水平網格與誠實離線狀態。
- Runtime evidence：1672×941 screenshot SHA-256 `849DB8ECFC61AE8FCC0448BB37ADAE7F7C7FDE9000EA86368A71220AA4CA3724`；1280×720 screenshot SHA-256 `EDD6651549270BE3B30CD39EBCE3B2A5AF1A4F369EA1F9E43BE6E8AD568FCB0A`。兩者在 Windows Text Scale 152% 下 main window render PASS、xaml runtime errors 0。R4 raster SHA-256 仍為 `A5279D85EB7324982703BD757556938D3DFE8267E62AC9E893963156878A0804`。
- Verification：locked restore PASS；focused 6/6 PASS；DesktopHost 與 solution Release x64 build 均 0 warnings／0 errors；full regression 1729/1825 PASS、96 FAIL；Frozen-name filter 5/10 PASS。失敗數與 Decision165 完全一致，沒有新增 regression，未刪測試、未 skip、未降標。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy，staged files 0。
- 取代：本決策只取代 Decision165 對現行字體角色、選取狀態與 Runtime evidence 的描述；Decision165 的材質／focus／mental-ring／footer 決策與 Decision164 的 R4 asset custody 繼續有效。

## TCC-DEC-2026-09-28-167 - Cross-scale Art Matrix Owns Typography, Accessibility Palette and Runtime Material Fidelity

- 日期：2026-09-28；來源：使用者要求以 UI/UX 所有美術指標 × 所有美術結構尺度逐格審核並修正。
- Audit decision：以 16 項指標 × token、glyph、control、item、component、module、region、page、viewport、scene 10 尺度建立 160 格矩陣。Baseline `P80 / F68 / N12` 的 68 格失敗收斂為五個根因，禁止以逐元件 one-off 色碼或字級 workaround 修補；正式矩陣為 `docs/design/TCC_STRATA_UI_ART_MATRIX_R1.md`。
- Token/scale decision：`MainWindow.xaml` 可見層不再包含 raw hex、literal FontSize、literal Georgia 或 literal CJK family。新增 CJK、BrandMark、UtilityGlyph、MarketValue、StatusValue、RailFooter 等角色，並將 SectionTitle、全部新增字級與 line-height 納入 `_baseTypographyMetrics`，使 152% Windows Text Scale 對整個 hierarchy 生效。
- Accessibility/material decision：High Contrast override keys 擴充到 SceneVeil、WorkspaceScrim、Secondary/Safety/Header/Chart、selection、footer、glint、grid、row separator 與 mental-ring。Normal mode 不再用 code-behind solid brush 壓掉 Design XAML gradients；只有 High Contrast 建立 local palette override。主色對深色角色的最低 WCAG 對比實測為 4.77:1。
- Affordance/UIA decision：無 handler 的搜尋改為明示 offline unavailable，快捷鍵 pill 改為 OFF；Watchlist menu 降權並曝露 unavailable semantics；Activity caret 改為靜態 LOCAL EVENTS。Main ScrollViewer 移除無作用 focus target；20 個 button 全部具 AutomationId 與 explicit accessible name。
- Runtime decision：第一次 152% Runtime 因 Safety 第三行與 rail footer clipping 正式退回。第二輪只縮短 Safety 內部節奏並新增 rail-footer 尺度，沒有移動任何外框。1672×941 與 1280×720 pass2 在 DPI96／Text Scale152% 均 21/21 UIA visible、20/20 buttons named、offscreen 0；screenshots SHA-256 分別為 `6713BD1EEF76C0FAADE94C315C79AA110A297951E5566E393F2E206C3FC2B4D0`、`A04B44A56D07DBEE05304AAAD49C191F09027488E9D47E90D41C71395BD96706`。
- Custody/truth decision：R4 raster SHA-256 保持 `A5279D85EB7324982703BD757556938D3DFE8267E62AC9E893963156878A0804`；人物、scene、七個外框與 OFFLINE／UNKNOWN／Read only／No execution API 完全不變。產品資產目錄仍只有 R4 一個檔案。
- Verification：locked restore PASS；focused 7/7 PASS；DesktopHost／solution Release x64 均 0 warnings/errors；full regression 1730/1826 PASS、96 FAIL；Frozen filter 5/10 PASS；`git diff --check` PASS；staged files 0。相較 Decision166，失敗數完全不變，新增 focused test 只使 passed/total 各增加一。
- Limitation：目前環境沒有 `winapp` CLI，改由專案既有 HWND `PrintWindow` + UIAutomationClient 流程產生實證。High Contrast 程式路徑已完整 token 化並被 focused test 鎖定，但本輪未擅自切換使用者的 Windows system theme，因此 actual High Contrast screenshot 仍待另行授權。
- 取代：本決策取代 Decision166 的現行 Runtime evidence 與文字縮放／材質執行方式；Decision164 的 R4 asset custody、Decision165/166 的幾何與 operational truth 繼續有效。

## TCC-DEC-2026-09-28-168 - Source-specific Identity Marks Must Be Reconstructed as Geometry

- 日期：2026-09-28；來源：使用者開始第三輪並明確指出「朱印沒有還原」。
- Identity decision：母版標題旁是上下雙層直印，右側 editorial 是獨立方印；兩者不得再共用一般 CJK 字型 `承`。左上 `T` 同樣視為 drawn monogram，不再讓 Windows Text Scale 改變其圖形比例。
- Vector decision：新增 `Strata.Seal.Vertical`、`Strata.Seal.Square` 與 `Strata.BrandMonogram` 三套原生 WPF path template；外框使用分段、不完全閉合的刻印邊緣，內部使用各自的方折 stroke topology。所有 identity control 均不可 focus、不可 hit-test，actual copy 的 Windows Text Scale 路徑維持不變。
- Token decision：新增 muted-vermilion `Tcc.Strata.Brush.Seal`，與功能危險狀態的 `Danger` 分離，並納入 normal reset 及 High Contrast override。品牌字級、microcopy 及右側 editorial 另以角色 token 校正，不使用 one-off 字級。
- Locked scope：Decision164 的 R4 raster／人物位置，Decision165/166 的七個外框，Decision167 的 High Contrast／UIA／operational-truth boundary 全部保留；沒有生成新 raster、沒有恢復舊資產、沒有加入虛構市場值。
- Runtime evidence：1672×941 與 1280×720 均在 DPI96／Text Scale152% 通過，21/21 UIA visible、20/20 buttons named、offscreen0；screenshots SHA-256 分別為 `CD7C159E9228C71E8AECDC76B1754285355849A822D5AA4EB197C0557FB607D8` 與 `1182CB6E9A955CF0FDD7588A2107991B086516C5102109120BEB1264C4461953`。
- Verification：locked restore PASS；focused7/7 PASS；DesktopHost／solution Release x64 0 warnings/errors；full regression1730/1826 PASS、96 FAIL；Frozen filter5/10 PASS；raw color/font-size/font-family/legacy-seal scans0/0/0/0；R4 hash unchanged；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策只取代 Decision167 中仍以一般字體代理 identity marks 的實作與現行 Runtime evidence；Decision167 的 matrix/token/accessibility ownership 及 Decision164–166 的 scene、geometry、material、truth decisions 繼續有效。

## TCC-DEC-2026-09-28-169 - Fourth-round Craft Refinement Uses Honest Absence and One Vector Language

- 日期：2026-09-28；來源：使用者接受第四輪建議並指示開始施工。
- Locked scope：Decision164 的 R4 raster／人物尺度位置、Decision165–168 的七個外框、材質、文字縮放、High Contrast、identity geometry 與 operational-truth boundary 全部保留；不新增功能、不生成 raster、不用歷史資產、不編造價格、走勢、時間或健康狀態。
- Empty-state decision：Market chart 移除單一大破折號與水平假折線，改為三層誠實語意：`NO LIVE MARKET DATA`、`MARKET FEED OFFLINE`、`NO SERIES TO DISPLAY`；保留低對比水平 guide 與軸刻度以維持母版資訊密度，但不表示任何實際資料。
- Vector-system decision：minimize／maximize／close 與 Home／Market／Plan／Risk／Positions／Review／Settings 全部改為共用 1.45px、round-cap／round-join 的原生 vector family；window glyph 維持 square-cap 的 chrome 語彙。既有 AutomationId、accessible name、enabled/offline semantics 不變。
- Craft decision：兩枚朱印改為更不對稱且分段的刻印筆畫；Mental State 改為六段不同粗細、透明度與 dash rhythm 的乾筆 path；新增 reusable `Strata.CraftFrame` 角部線、lower-panel local scrim、right-editorial scrim，並把 brand display weight 由 SemiBold 降為 Normal。所有新增 brush 皆納入 normal reset 與 High Contrast transparent override。
- Runtime evidence：1672×941 screenshot SHA-256 `EF8C0B81EEEBDD1B188B798DD8948A3CBA9DB4EFAB75EB1210E0DEF520D0FFCC`；1280×720 screenshot SHA-256 `64841CA8BAAC1F5B58FEFA95A3A9F22738324AC404C3176C2E68A1E6952455AF`。兩者均為 DPI96／Windows Text Scale152%，21/21 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore 6/6 PASS；focused 8/8 PASS；DesktopHost／solution Release x64 0 warnings/errors；full regression 1731/1827 PASS、96 FAIL；Frozen-name filter 5/10 PASS；raw color/font-size/font-family/legacy-seal scans 0/0/0/0；R4 hash unchanged；`git diff --check` PASS；staged files0。相較 Decision168，唯一數量變更為新增一個 passing focused test，失敗集合保持 96／5。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision168 的現行 Runtime evidence，並補充 chart absence、icon family、dry-brush ring、scrim 與 craft-frame 語彙；Decision164–168 的 scene、geometry、accessibility、identity 與 product truth 仍有效。

## TCC-DEC-2026-09-28-170 - Fifth-round Luxury Fidelity Uses Restrained Signals and Layered Ink Material

- 日期：2026-09-28；來源：使用者要求第五次精修，強調高品質、高質感並加倍仔細分析母版。
- Locked scope：Decision164 的 R4 raster／人物尺度位置、Decision165–169 的七個外框、字體尺度、High Contrast、向量 icon family、identity geometry 與 operational-truth boundary 全部保留；本輪不新增功能、不生成 raster、不恢復舊資產，也不以虛構市場值提高資訊密度。
- Status-hierarchy decision：Market 離線區把整行 danger mono headline 拆為 7px danger signal、低階 `OFFLINE` 與中性 `NO LIVE MARKET DATA`，保持狀態明確但不讓長期離線狀態成為全頁視覺主角；中央空態仍明示 `MARKET FEED OFFLINE`／`NO SERIES TO DISPLAY`。
- Material decision：primary／secondary／safety 面板由垂直三段平均漸層改為雙向兩段材質，selected surface 與 footer band 改為低對比方向性漸層；`Strata.CraftFrame` 使用不對稱 pale-gold／ice micro-edge，不新增 shadow、glow 或玻璃卡片層級。
- Identity decision：vertical／square seals 在既有 carved geometry 下增加同 token 的寬筆、低透明、次像素 offset bleed layer，High Contrast 仍由同一 `Seal` resource 接管；不是字型、raster 或新資產。
- Brushwork decision：Pass1 的無 dash 圓環仍因過度平滑被退回。Pass2／Pass3 以兩道寬墨底、四道不等長連續 sweep、內圈刻痕與七段不重複 bristle fragment 重建 Mental State；正式輸出不再含任何 `StrokeDashArray`。
- Runtime evidence：最終 1672×941 screenshot SHA-256 `416B019436A9488B61112C42AC0F52BE3B52401B4A2625EFF4626BCA1D7DDCFA`；1280×720 screenshot SHA-256 `3CD5782F3F660980ACA5833360A759E8FC477B5424ACB62F9926827A1B3CB452`。兩者均為 DPI96／Windows Text Scale152%，21/21 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore 6/6 PASS；focused 9/9 PASS；DesktopHost Platform=x64／solution Release x64 0 warnings／0 errors；full regression 1732/1828 PASS、96 FAIL；Frozen-name filter 5/10 PASS；與既有 TRX baseline 比較，兩組 failure names 均 new0／resolved0。raw color／font-size／font-family／legacy-seal／dash scans 0/0/0/0/0；R4 hash unchanged；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision169 的現行 Runtime evidence 與 Mental State／seal／panel-material 執行細節；Decision164–169 的 scene、geometry、accessibility、icon family、identity topology 與 product truth 繼續有效。

## TCC-DEC-2026-09-28-171 - Sixth-round Refinement Makes Unknown and Dormant States Visually Honest

- 日期：2026-09-28；來源：使用者要求第六輪精修，並指定抓出先前沒有找到的問題。
- Locked scope：Decision164 的 R4 raster／人物尺度位置、Decision165–170 的七個外框、材質、字體尺度、High Contrast、identity geometry 與 operational-truth boundary 全部保留；不新增功能、不生成 raster、不恢復舊資產、不編造市場或心理狀態。
- Honest-state decision：Watchlist 五個紅／青漲跌箭頭在價格未知時仍會暗示趨勢，Mental State 四個實心點在評估未設時仍會暗示已確認；兩者改用共用中性空心 unset marker，不再以形狀編造方向或完成度。
- Affordance decision：HOME 保留 selected-current-page 外觀但改為 disabled，UIA name 為 `Home selected, current page`；空 priorities 首列移除 gold stripe 與 selected surface，避免把「無資料」呈現為可執行選項。
- Scale decision：頁面 subtitle 移除手工空格 tracking，改回正常文字並由共用 7.5px role 接管；Safety cell 只縮短內部水平節奏，修復 `Read only · Not tradable` 在 152% Text Scale 的字形裁切，不移動任何外框。
- Runtime evidence：Pass1 因 Safety 仍裁切及 subtitle 層級退回；Pass2 因空 priority 假選取退回；Pass3 1672×941 screenshot SHA-256 `60CB9CA46059E0175FF2CB936CAB3B990FD6E29DBFB3636F02658A86F1B0422D`，1280×720 screenshot SHA-256 `760F3FA1CD2859C143FEF9BA190EF4E43367773E94B77EE00481601EF7089741`。兩者均為 DPI96／Windows Text Scale152%，21/21 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore PASS；focused 10/10 PASS；DesktopHost／solution Release x64 0 warnings/errors；full regression 1733/1829 PASS、96 FAIL；Frozen-name filter 5/10 PASS；與 Decision170 baseline 比較，兩組 failure names 均 new0／resolved0。raw color／font-size／font-family／legacy-seal／dash／old-trend scans 0/0/0/0/0/0；R4 hash unchanged；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision170 的現行 Runtime evidence，並補充 unknown-state geometry、current-page affordance、microcopy tracking 與 empty-row selection 語意；Decision164–170 的 scene、geometry、material、accessibility、identity 與 product truth 繼續有效。

## TCC-DEC-2026-09-28-172 - Lower-right Scenery Uses an Independent Alpha Layer and Measured Luminance Lift

- 日期：2026-09-28；來源：使用者要求參考母版在右下加入造景，先製作新素材，並同步修正整體畫面明亮度。
- Asset decision：以母版與現行 Runtime 作為 reference-only input，透過 built-in ImageGen 新生成 `StrataObservatory.ForegroundPlum.R1.png`；1254×1254 Format32bppArgb，alpha 0–255，visible bounds x285–1253／y548–1253，SHA-256 `AE55504A8DDFA173425FCB2D8DF18931CCF1533D6BA936B5A645F5E4DC280793`。資產只含積雪老梅枝、朱紅花朵、霜與局部冷霧，沒有 UI、人物、文字或完整背景。
- Composition decision：前景作為獨立 non-interactive WPF Image，Z40、opacity 0.9。Pass1 x1100/y470、620×620 因 nominal canvas 大量透明邊界使造景被藏在 viewport 外而退回；Pass2 x1010/y300、760×760 建立清楚前景；Pass3 依 Human feedback 縮小約 10.5% 並右移 60px／下移 50px，最終 x1070/y350、680×680，使造景從畫面外自然進場且不形成厚重底座。
- Accessibility decision：新前景 `Focusable=False`、`IsHitTestVisible=False`，並以 `Tcc.Strata.ForegroundSceneryOpacity` 集中控制；High Contrast 顯式將其設為 0，不干擾 system palette 與控制順序。
- Luminance decision：SceneVeil alpha 由 `#35` 收敛至 `#21`，WorkspaceScrim 由 `#94/#82/#42` 收敛至 `#86/#74/#36`；1672×941 工作區 mean luminance 由 31.613 提升至 33.764（+6.8%）。面板、文字、框線、狀態與業務真值不變。
- Runtime evidence：1672×941 screenshot SHA-256 `CAD3FCC767D615417F6C35D9DA06E3CF539904BB3857E1450DE0F89F1A3A51BB`；1280×720 screenshot SHA-256 `D00E04D2303A274D66DCADD323FDEFD8B96E3F794004BFE65C3747F7957D1F5D`。兩者均為 DPI96／Windows Text Scale152%，21/21 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore PASS；focused 11/11 PASS；DesktopHost／solution Release x64 0 warnings/errors；full regression 1734/1830 PASS、96 FAIL；Frozen-name filter 5/10 PASS；與 Decision171 baseline 比較，兩組 failure names 均 new0／resolved0。R4 hash unchanged；沒有刪測試、skip 或降低驗證標準。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision171 的現行 Runtime evidence 與 product-asset-count 敘述；Decision164 的 R4 asset custody、Decision165–171 的 geometry、material、accessibility、identity 與 product truth 繼續有效。

## TCC-DEC-2026-09-28-173 - Edge Material Is Card-specific Physical Buildup, Not a Repeated Crack Template

- 日期：2026-09-28；來源：使用者退回先前卡片冰紋施工並要求以矩陣重新分析母版後開始施工。
- Scope decision：本輪只在 Market Overview 與 Priorities 驗證材質語彙；Watchlist、Mental State、Recent Activity、Safety Core、人物、R4 場景、右下前景、七個外框與產品真值全部保持不變。POC 未經 Human acceptance 不得擴張到其他卡片。
- Material decision：母版邊緣拆為結構 1px 框線、攝影式霜雪／礦物堆積、場景接觸與極少暖金礦脈；禁止四角同構、細線蛛網、霓虹青與幾何裂紋模板。第一次生成因仍呈現四角線稿與右上蛛網感正式退回，不進入交付。
- Density decision：第二次全新生成兩張獨立透明資產。Market 採低密度、主要沿左／底邊的 `StrataObservatory.MarketEdgeMaterial.PocR2.png`，opacity 0.10；Priorities 採左下／底部較重的 `StrataObservatory.PrioritiesEdgeMaterial.PocR2.png`，opacity 0.16，且只有一條上右暖金礦脈。兩者中心保持透明，不攔截 hit test 或 focus。
- Accessibility decision：兩張 overlay 均為 `Focusable=False`、`IsHitTestVisible=False`、HighQuality bitmap scaling；Normal mode 使用集中 opacity token，High Contrast 將兩個 token 顯式設為0。沒有新增鍵盤順序、功能狀態或可操作 affordance。
- Runtime evidence：1672×941 screenshot SHA-256 `35CE77334A173474E8BC595271DEE8CC06C39363EE399D9C58A82D55EF01F00C`；1280×720 screenshot SHA-256 `A7C6B07EE9EA407E558642DC066E293DCEB4AD875C6DF0D90C57A258CF18FA56`。兩者均為 DPI96／Windows Text Scale152%，21/21 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore PASS；focused HOME 12/12 PASS；DesktopHost／solution Release x64 0 warnings/errors；full regression1735/1831 PASS、96 FAIL；Frozen-name filter5/10 PASS。相較 Decision172，失敗數維持96／5，新測試只使 passed/total 各增加一。scope scan 確認 edge overlays2、其餘四區0、raw hex/font-size/old-frost0/0/0；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策只取代所有未接受的舊卡片冰紋／裂紋施工與現行 Runtime evidence；Decision164–172 的場景、幾何、前景、accessibility、identity 與 operational truth 繼續有效。

## TCC-DEC-2026-09-28-174 - Edge Material Restraint Is Controlled by Broken Topology Before Opacity

- 日期：2026-09-28；來源：使用者判定現行冰紋太搶戲，暫停後續擴張並以 `0` 指示繼續修正。
- Scope decision：本輪只修正 Decision173 已存在的 Market Overview 與 Priorities 兩張 overlay；Watchlist、Mental State、Recent Activity、Safety Core、人物、R4 場景、右下前景、七個外框及所有產品真值保持不變。原定延伸正式暫停，待 R3 Human review 後另行決定。
- Material decision：R2 問題不是只有透明度，而是完整四邊周界形成高權重閉合輪廓。R3 重新生成兩張獨立透明素材，先移除連續頂／右／底邊、重複角落與 Priorities 金裂紋，再以 runtime opacity 微調；不得用單純降低 alpha 掩飾完整框 topology。
- Density decision：Market R3 非透明覆蓋 5.974%、mean alpha 4.40、opacity 0.12；Priorities R3 非透明覆蓋 4.581%、mean alpha 3.09、opacity 0.14。對照 R2 分別為 10.670%／mean alpha12.30／opacity0.10 與 18.665%／mean alpha32.29／opacity0.16。R3 只保留斷續局部沉積，中心資料區維持透明。
- Asset custody：`StrataObservatory.MarketEdgeMaterial.R3.png` SHA-256 `D93C8CD0A481981E64DB00BF072B40A39E336C27CB0B1643661D41BC1EACBF87`；`StrataObservatory.PrioritiesEdgeMaterial.R3.png` SHA-256 `7CE5796FAD283D3216F89510A6E9AC21F17CDCE163A4DE9AE373827E766D37B3`。R2 product references 為0；R4 raster 與 foreground raster hashes 不變。
- Accessibility decision：overlay 仍為 `Focusable=False`、`IsHitTestVisible=False`、HighQuality scaling；High Contrast 仍將兩個 opacity token 設為0。沒有新增控制、焦點、功能狀態或資料語意。
- Runtime evidence：1672×941 screenshot SHA-256 `CEF784A28D7A6B2A4DECA8D64261DFFDF781A4477BA6B639CAD995DC781E6D17`；1280×720 screenshot SHA-256 `5D7DFC8021995C8980CB749C77EBDB9DB1244B05C9145C0D951FF0AD07160E14`。兩者均為 DPI96／Windows Text Scale152%，21/21 UIA visible、20/20 buttons named、offscreen0，且經人工畫面檢查未見裁切、重疊、意外 scrollbar 或材質壓過內容。
- Verification：locked restore PASS；focused HOME 12/12 PASS；DesktopHost exact Release x64／solution Release x64 0 warnings/errors；full regression1735/1831 PASS、96 FAIL；Frozen-name filter5/10 PASS。相較既有 matrix TRX baseline，兩組 failure names 均 new0／resolved0。scope scan edge Image overlays2、其餘四區0、R2 product references／raw hex／literal font-size0/0/0；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision173 的 R2 asset、密度、opacity 與 Runtime evidence；Decision173 的 card-specific、non-interactive、High Contrast 與 limited-scope principles，以及 Decision164–172 的場景、幾何、前景、accessibility、identity 與 operational truth 繼續有效。

## TCC-DEC-2026-09-28-175 - Edge Material Extension Is Governed by a Card-specific Art and Scale Matrix

- 日期：2026-09-28；來源：使用者以 `0` 指示延續已收斂方向，並補充「這個也施工，剛剛的矩陣」。
- Scope decision：保留 Decision174 的 Market／Priorities R3，新增 Watchlist、Mental State、Recent Activity 三張各自生成的透明材質；Safety Core、人物、R4 場景、右下前景、七個外框與所有產品真值不變。
- Matrix decision：正式建立 `docs/design/TCC_STRATA_EDGE_MATERIAL_EXTENSION_R1_MATRIX.md`，逐卡交叉驗證素材比例、拓撲、alpha 覆蓋、runtime opacity、內容碰撞、視覺層級、互動穿透、高對比停用與雙 viewport 表現。素材比例相對卡片比例誤差均低於 0.8%，不以同一模板換尺寸。
- Material decision：Watchlist 只保留左下短痕與極短底邊微痕，alpha 覆蓋2.42%、opacity0.11；Mental State 只保留右上石屑與底邊短痕，alpha 覆蓋2.68%、opacity0.10；Recent Activity 只保留分離的左上粉塵／右下沉積，alpha 覆蓋4.29%、opacity0.08。五張現行材質資產 SHA-256 全部唯一，且不形成第二套閉合框線。
- Accessibility decision：三張新 overlay 均為 `Focusable=False`、`IsHitTestVisible=False`、HighQuality scaling；Normal mode 由 card-specific token 控制，High Contrast 明確設為0。Safety Core 不新增 overlay。
- Runtime evidence：1672×941 screenshot SHA-256 `0BC7F23BE6595D4AFEA39E4607061996916A4371764AEC00041790B2C9966AEB`；1280×720 screenshot SHA-256 `93AEF67D2FC0E87719CA45E040C7321E33A0B523C097F28C13A074003D2B5F69`。兩者均為 DPI96／Windows Text Scale152%，21/21 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore PASS；focused HOME13/13 PASS；DesktopHost exact Release x64／solution Release x64 0 warnings/errors；full regression1736/1832 PASS、96 FAIL；Frozen-name filter5/10 PASS。與 matrix TRX baseline 比較，兩組 failure names 均 new0／resolved0。scope scan edge overlays5、Safety overlay0、R2 product references／raw hex／literal font-size0/0/0；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision174「不得擴張至其他卡片」的暫停狀態與現行 Runtime evidence；Decision174 的 broken-topology-first 原則、既有 R3 資產與 Decision164–173 的場景、幾何、前景、accessibility、identity、Safety Core 與 operational truth 繼續有效。

## TCC-DEC-2026-09-28-176 - Decorative Material Must Have an Explicit Content Layer and Runtime-discoverable Region Peer

- 日期：2026-09-28；來源：使用者以 `0` 指示繼續下一輪，並要求沿用先前矩陣施工。
- Layer decision：五張裝飾卡片不得依靠素材中央透明避免壓字。直接內容明確置於 Z5、材質維持 Z4、CraftFrame 維持 Z3；這將視覺層級從素材偶然性改為 XAML 契約。Native 舊／新比較只有0.45%像素改變，平均 changed-channel delta1.83，外框、人物、背景與所有內容座標不變。
- Region decision：原先 `Region.HomeSafetyCore` 錯掛 Market Overview；現改為 Safety 的 `Trading Permission` heading 承載，並新增 Market、Watchlist、Priorities、Mental State、Recent Activity 五個正確區域 ID 與 honest accessible name。
- Runtime-peer decision：Pass1 將 AutomationProperties 放在 Border，靜態 XAML 測試通過但 Runtime 只有21/27 UIA，六區全數 missing，正式退回。Border 不會因 attached metadata 自動建立 automation peer；Pass2 改由可見 TextBlock heading 承載，兩個 viewport 均27/27、offscreen0。後續 landmark gate 必須以 Runtime AutomationId 查找，不得只驗證 XAML 字串。
- Runtime evidence：1672×941 screenshot SHA-256 `2A0F3005003F46127F10F594477F87B2D8E71398353F418D74488FEA5B154065`；1280×720 screenshot SHA-256 `E6F5B05040418D536007501E13E65BCAE2966B8D27A95E8A6E18A70FC2D71027`。兩者均DPI96／Windows Text Scale152%、27/27 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore PASS；focused HOME14/14 PASS；DesktopHost exact Release x64／solution Release x64 0 warnings/errors；full regression1737/1833 PASS、96 FAIL；Frozen-name filter5/10 PASS；相較 Decision175 baselines failure names new0／resolved0；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision175 的現行 Runtime evidence、21/21 UIA 基準與 implicit card-content layer；Decision175 的五素材矩陣、Decision174 的 broken topology 及 Decision164–173 的場景、幾何、前景、identity、High Contrast、Safety Core 與 operational truth 繼續有效。

## TCC-DEC-2026-09-28-177 - Edge-material Visibility Is Calibrated by Effective Alpha

- 日期：2026-09-28；來源：Human 明確指出「冰紋沒加上去」。
- Diagnosis：五張透明 PNG、product reference、Z4 material layer 與 High Contrast override 均存在；缺陷不是安裝或 topology，而是 sparse intrinsic alpha 再乘上低 runtime opacity 後，主要像素在完整 Runtime 畫面中近似不可見。第一個修正截圖也揭露 build output 與 capture executable 路徑不同，依 Observation064 退回並以 exact Platform=x64 binary 重取證據。
- Visibility decision：素材、alpha coverage、位置、卡片幾何、content Z5／material Z4／frame Z3 及 Safety Core clean contract 全部不變。只把 Market／Priorities／Watchlist／Mental State／Recent Activity opacity 分別定為0.50／0.55／0.65／0.65／0.70；控制目標是局部沉積可直接辨識，而非形成完整冰框。
- Quantitative evidence：相對 Decision176 native baseline，以2px步距取樣393756 pixels，changed5513（1.400%）、mean RGB delta0.373、max215。這證明修正仍為局部邊緣材質，不是整頁亮度或場景替換。
- Runtime evidence：1672×941 screenshot SHA-256 `CE43B2405BB9AA53D9CC5F882F9C3DF9138B0B2D1DB62805F051E842F193B2BA`；1280×720 screenshot SHA-256 `B5EFE44A63E599497171AF37A24A7112569B1D8F2197541991993FEAF3CD7DCC`。兩者均DPI96／Windows Text Scale152%、27/27 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore PASS；focused HOME14/14 PASS；DesktopHost exact Platform=x64／solution Release x64 builds0 warnings/errors；full regression1737/1833 PASS、96 FAIL；Frozen-name filter5/10 PASS；相較 Decision176 baselines failure names new0／resolved0；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision176 的現行 runtime opacity 與 Runtime evidence；Decision176 的 layer／UIA peer contract、Decision175 的五素材矩陣、Decision174 的 broken-topology-first 原則，以及 Decision164–173 的場景、幾何、前景、identity、High Contrast、Safety Core 與 operational truth 繼續有效。

## TCC-DEC-2026-09-28-178 - Seventh-round Refinement Uses Reproducible Matrix Gates and a Single-alert Offline Hierarchy

- 日期：2026-09-28；來源：使用者要求提升矩陣品質與細節後執行第七輪修正。
- Matrix decision：新增 `TCC_STRATA_SEVENTH_REFINEMENT_R1_MATRIX.md`。每個美術指標必須對應 token／control／card／region／viewport 尺度，並記錄 P0/P1/P2、S/R/U/T/H evidence、量測門檻與退回動作；只列 PASS 而沒有證據與 rollback 的表格不再視為 gate。
- Locked scope：Decision164–177 的 R4 scene、人物、前景、五張冰紋資產、卡片幾何、layer order、region peer、identity、High Contrast 與 operational truth 全部保留；本輪不生成或修改 raster，不新增功能或虛構資料。
- Safety hierarchy decision：Trading Permission 的 danger `OFFLINE` 是唯一主警示；Total Risk／Current Positions／Major Alerts 的 `UNKNOWN` 不是數值，改用 `Strata.StatusUnknown`（Interface SecondaryStatus）與 neutral unset ring，保留文字真值但降低競爭權重。
- Empty-state decision：Market header 已說明 `OFFLINE · NO LIVE MARKET DATA`，中央不再重複 `MARKET FEED OFFLINE`；改為結果導向的 `NO SERIES TO DISPLAY`／`LIVE DATA REQUIRED`，glyph30→24、empty-state box92→82，維持 chart grid 語境而不形成第二張 error card。
- Runtime evidence：1672×941 screenshot SHA-256 `AD8407818179E0ED4E477F1231A9F4C72A420BADC31C57BAD41E197806F10AAA`；1280×720 screenshot SHA-256 `17EC01463316D0B6E48DDDA3675C43A78E2E3489D9D142F9AC9707735B27C018`。兩者均DPI96／Windows Text Scale152%、27/27 UIA visible、20/20 buttons named、offscreen0。相對 Decision177 native 只變動7113 pixels／0.4521%，mean RGB delta0.3744、max channel239。
- Verification：locked restore PASS；focused HOME15/15 PASS；DesktopHost exact Platform=x64／solution Release x64 builds0 warnings/errors；full1738/1834、96 FAIL；Frozen5/10、5 FAIL；相較 Decision177 failure names new0／resolved0。raw hex／literal FontSize／literal FontFamily／ellipsis0/0/0/0；全部產品 raster hash 不變；Hallmark pre-emit P5/H5/E5/S5/R5/V5 與適用 WPF gates PASS。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision177 的現行 Runtime evidence，並新增 matrix-gate、single-alert offline hierarchy 與 result-oriented chart empty state；Decision164–177 其餘有效鎖定全部延續。

## TCC-DEC-2026-09-28-179 - Eighth-round Graphics Fidelity Uses Native Geometry and Optical Calibration

- 日期：2026-09-28；來源：使用者在完成全介面圖形類別矩陣後要求進行下一次修正。
- Locked scope：Decision164–178 的 R4 scene、人物、右下前景、五張冰紋資產、卡片幾何、layer order、region peers、High Contrast、Safety Core、離線／unknown 真值全部保留；本輪不生成或修改 raster，不新增功能或虛構資料。
- Glyph decision：Home selected 改為 23px 填色屋形與透明門洞；Market／Plan／Risk／Positions／Review／Settings 改為 22px、1.3px 等級並逐枚校正路徑，不使用單一機械縮放。Watchlist `⋮` 字型符號改為三個獨立 Ellipse 的 `Strata.KebabGlyph`，避免字型 baseline 與 fallback 漂移。
- Identity decision：Vertical／Square seal 尺寸與位置不變，只提高核心刻線與偏移墨暈權重；Mental State 保持 NOT SET／NO SELF-CHECK 真值，以10條不等粗細、偏心、斷續 Path 建立墨刷而非科技 HUD。舊輪次鎖定的三段 brush path 與 foreground marker 保留，沒有放寬既有 contract。
- Quantitative evidence：相較 Decision178 native baseline，以2px步距取樣393756 pixels，changed over RGB sum3 為2587（0.6570%）、mean RGB absolute delta0.2367、max RGB sum delta543；差異維持在 glyph、圓環與朱印局部。
- Runtime evidence：1672×941 screenshot SHA-256 `CE2ADAB6E38428FC77784F3DA9FE7F93A1A062E185571ACDCAB4A9E4A038C738`；1280×720 screenshot SHA-256 `084CE5079F780746FE04651D96027164E60010896E03995E685169034DB1DE55`。兩者均DPI96／Windows Text Scale152%、27/27 UIA visible、20/20 buttons named、offscreen0。
- Verification：locked restore PASS；focused HOME16/16 PASS；DesktopHost exact Platform=x64／solution Release builds0 warnings/errors；full1739/1835、96 FAIL；Frozen5/10、5 FAIL；相較第六／七輪 baseline 兩組 failure names new0／resolved0。raw hex／literal FontSize／literal FontFamily／text kebab0/0/0/0；七個 product raster references 及全部 raster SHA-256不變；Hallmark pre-emit P5/H5/E5/S5/R5/V5 與適用 WPF gates PASS；`git diff --check` PASS；staged files0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision178 的現行 Runtime evidence，並新增 native glyph、explicit kebab、brush-ring 與 stronger seal fidelity；Decision164–178 其餘有效鎖定全部延續。

## TCC-DEC-2026-09-28-180 - Theme Iconography Is Derived from Product Semantics Without Mother-reference Input

- 日期：2026-09-28；來源：使用者明確要求「矩陣生成主題化圖標素材，不用參考母版」。
- Source decision：本輪不得讀取、分析或量測母版；圖標唯一來源為 TCC 功能語意、現行 Strata theme token、WPF 控制尺度、狀態語意與 accessibility contract。既有背景、人物、前景、冰紋、卡片幾何與產品真值全部鎖定。
- System decision：建立獨立 `StrataObservatory.Iconography.xaml`，以「寒鋒刻痕（Fractured Strata）」統一 24×24 navigation／command／state 與 16×16 window chrome。素材庫固定為17個唯一 Geometry／16個唯一 Style，且不得包含 raw hex、FontFamily、reference／mother 依賴或 Design dictionary shadow key。
- Semantic decision：Navigation、Command、Window、State 四類以功能語意而非裝飾裂紋區分；Home 使用 Ice＋Gold keystone，其餘 inactive 使用 TextSecondary，unavailable 以 Danger slash 疊加。圖標保持 Focusable=False／IsHitTestVisible=False，操作名稱與狀態仍由外層 control 承擔。
- Runtime decision：第一輪 Risk 的水平／垂直交叉結構容易被讀成醫療十字，正式退回；final 改成盾牌＋縱向限制刻痕。1672×941／1280×720 均 DPI96／Text Scale152%、27/27 UIA visible、20/20 buttons named、offscreen0；SHA-256 分別為 `FE7F67BD01E64A3B32BF8489A67C464410096B8CBB2076A71A81CC6784F3B65C`／`5B38EE02D1A758632D96ED3E92CE999172F818693BBDF7CDBA38AA840C0FC42D`。
- Quantitative evidence：相較 Decision179 native baseline，以2px步距取樣393756 pixels，changed615（0.1562%）、mean RGB absolute delta0.0859；全部 product raster hashes 不變。
- Verification：locked restore PASS；focused HOME17/17 PASS；DesktopHost exact Platform=x64／solution Release builds0 warnings/errors；full1740/1836、96 FAIL；Frozen5/10、5 FAIL；相較 Decision179 failure names new0／resolved0；icon scan Styles16／Geometries17／duplicate0／raw hex0／font0／reference0；`git diff --check` PASS；staged0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision179 的現行 icon implementation 與 Runtime evidence；Decision164–179 對場景、幾何、材質、前景、High Contrast、Safety Core 與 operational truth 的其餘有效鎖定繼續成立。

## TCC-DEC-2026-09-29-181 - Theme Icon Assets Require Standalone Human Approval Before Product Integration

- 日期：2026-09-29；來源：使用者指出 R1 未真正融合主題，並明確要求先生成圖標素材包、過審後才放入 WPF。
- Workflow decision：後續主題圖標分為兩個獨立 gate。Phase A 只產生隔離的 production-format candidate assets、manifest 與多尺度 contact sheet；Phase B 必須在 Human 明確 approve 後才可修改 `src/Tcc.DesktopHost`、建立 Style／ControlTemplate、執行 Runtime 與 accessibility gate。
- Candidate decision：R2 採「寒鋒層印（Frostcut Sigil）」語彙，以冰銀 semantic silhouette、金色 pressure-point facet、岩冰斜切與朱紅 danger cut 組成。15枚 semantic icons 分為 Navigation7／Command3／Window3／State2，共30個唯一 WPF Geometry。
- Isolation decision：候選包只存在 `output/strata-observatory-themed-icon-pack-r2/`；product source／tests 對 `Tcc.Frostcut.*` references 為0。本階段不得修改 `MainWindow.xaml`、production resources、AutomationId、控制狀態或產品資產。
- Evidence：candidate XAML parse PASS；Geometry30／unique30／duplicates0；1600×1000 contact sheet SHA-256 `37532D420659EC886EE3023270FC40FAB6E36D8DCC9C96C9D5A6BB3C54D774E9`；manifest 明確標記 `CANDIDATE_REVIEW_ONLY`／`integrated:false`。
- Status：`CANDIDATE_ASSET_REVIEW_NOT_INTEGRATED`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策不移除 R1 現行 Runtime；它取代「新圖標可直接接線再驗收」的工作順序。R1 保持現狀，直到 R2 Human approve 後才允許另開 integration phase。

## TCC-DEC-2026-09-29-182 - Premium Icon Quality Requires Layered Material and Independent Optical Geometry

- 日期：2026-09-29；來源：Human 對 R2 候選包的 verdict 為「質感不夠」，並以 `0` 指示繼續重製。
- Rejection decision：R2 的 semantic silhouette、金色 facet 與家族一致性不足以建立高質感；它仍被讀成加上主題色的工程線稿。R2 保留為 rejected candidate evidence，不得整合產品。
- Material decision：R3 採「玄冰鎏金（Obsidian Frost Inlay）」四層 display stack：3.4px carved relief、1.28px gradient ice blade、0.42px specular glint、sparse gold pressure-point inlay；Unavailable 的第四層改為 vermilion cut。材質深度由 Drawing layer 形成，不生成 raster、不使用大面積 glow。
- Optical-size decision：每枚 icon 不得以同一路徑從24px縮至16px。15枚 icon 各有 display Main／Inlay 與獨立 Compact geometry，共45 Geometry；display15與compact15共30 DrawingImage。16px compact 刻意移除 relief／glint，降低視覺泥化。
- Isolation decision：全部 R3 檔案只存在 `output/strata-observatory-themed-icon-pack-r3/`；`src/`／`tests/` 對 `Tcc.ObsidianFrost.*` references0；未修改 `MainWindow.xaml` 或 production resource wiring。
- Evidence：WPF XamlReader86/86 resources PASS；Geometry45／DrawingImage30／Brush5／Pen6／unique keys86；1600×1000 contact sheet SHA-256 `62DA7FCB309244DFC559E740576ACA709D73FA50224EFC81DFECAEF3431ACF48`；candidate XAML SHA-256 `4885BCC8FC163EBC2A84E3ADB6E6E2977B5515009EF3FE9BD828795BA3367833`。
- Status：`CANDIDATE_ASSET_REVIEW_NOT_INTEGRATED`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision181 的 R2 candidate 作為目前審核標的，但保留 Decision181 的 approval-before-integration workflow。R1 production 現況與 Decision164–180 的有效產品鎖定不變。

## TCC-DEC-2026-09-29-183 - Whole-set Regeneration Requires New Silhouettes, Not Revision of Rejected Geometry

- 日期：2026-09-29；來源：Human 判定 R3「玩具感太重、主題性不足、冰雪與紅梅元素不足」，並進一步明確要求「不要用修改的，整套重新生成」。
- Rejection decision：R3 的描線／浮雕／金屬節點 grammar 與中止的 R4 冰骨線框均不得再修改為下一候選。R3、R4 保留為 rejected evidence，不得接入產品。
- Clean-room decision：R5 另建 `Tcc.FrostboundPlum.*` namespace 與 `Frostbound Plum Seal／冰魄梅篆` grammar。15枚 semantic silhouettes 全部從零重畫，R4／R5 normalized Geometry string intersection為0；不讀取母版、不使用舊 raster、不引用舊 candidate resources。
- Theme decision：功能主體由不對稱實心冰片構成；積雪以獨立白色 fill plane 表現；寒刻內紋保存功能辨識；暗朱梅枝穿入主輪廓；紅梅使用圓潤五瓣與獨立花苞，不以金色寶石或通用角落貼花代替主題。
- Presentation decision：contact sheet 取消菱形底座與卡牌容器，改用5×3無框標本格；每枚同時顯示大尺寸材質、actual24px與independent16px proof，避免 specimen 包裝本身製造玩具感。
- Isolation decision：R5 全部檔案只存在 `output/strata-observatory-themed-icon-pack-r5/`；`src/`／`tests/` 對 `Tcc.FrostboundPlum.*` references0；未修改 `MainWindow.xaml` 或 production resource wiring。
- Evidence：WPF XamlReader160/160 resources PASS；Geometry120／DrawingImage30／Brush5／Pen5／unique keys160；1600×1000 specimen SHA-256 `233F1C3886C858A682B6FA8BDBAA6608080C8FC541E5531200DE70DB92805F98`；candidate XAML SHA-256 `C60B10CA34625E3DFA0290D4CE21752E61F27C128326A16C0D980926F9DB4FAC`；review ZIP SHA-256 `C9674F08AB08048AE09913E1A750F22D7C058D3F4E3700B567B0235D1FE36CD4`。
- Status：`CANDIDATE_ASSET_REVIEW_NOT_INTEGRATED`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision182 的 R3 candidate 作為目前審核標的，但保留 Decision181 的 approval-before-integration workflow。R1 production 現況與 Decision164–180 的有效產品鎖定不變。

## TCC-DEC-2026-09-29-184 - Rime Plum Engraving R6 Replaces the Rejected Filled-Ice Candidate

- 日期：2026-09-29；來源：Human 在取得專門 SVG／icon Skill 後再次明確要求「重新製作ICON，不要用修改的」。
- Rejection decision：R5「冰魄梅篆」作為 rejected candidate evidence 保留，但不得修改或接入產品；R6 生成器不得包含 `FrostboundPlum`、`Obsidian` 或 `Frostcut` identifier，R5 僅能在 R6 完成後參與 exact geometry intersection 驗證。
- Clean-room decision：R6 另建 `Tcc.RimePlumEngraving.*` namespace 與 `Rime Plum Engraving／霜梅鏤影` grammar。15枚 semantic icons 各自擁有獨立24px與16px optical master，共30個 SVG、150個 StreamGeometry與30個 DrawingImage；R5／R6 exact normalized Geometry intersection為0。
- Theme decision：R6 不沿用 R5 的實心冰片／積雪面。功能骨架改為窄暗槽、冰釉主筆與微型冷光稜線構成的連續霜鋼鏤刻；紅梅必須具備可辨認的曲線枝幹、分岔、花苞及五瓣花，並落在每枚 icon 的 semantic pressure point，不得退化為統一角落貼花或狀態斜線。
- Optical decision：16px 使用另繪 compact geometry，不從24px線性縮放；display undercut由首輪4.15縮至3.35、ice enamel由2.55縮至2.25、ridge由0.48縮至0.38，以排除 sticker／toy outline；compact undercut／ice分別為2.38／1.58。
- Presentation decision：specimen sheet 維持5×3無框檢視結構，同時呈現104px material、actual24px dark、independent16px dark與actual24px pale proofs；重複 inline SVG 全部使用唯一 gradient ID 並標成decorative，避免審核板本身產生結構缺陷。
- Isolation decision：R6 全部檔案只存在 `output/strata-observatory-themed-icon-pack-r6/`；`src/`／`tests/` 對 `Tcc.RimePlumEngraving.*` references0；未修改 `MainWindow.xaml` 或 production resource wiring。
- Evidence：WPF XamlReader195/195 resources PASS；StreamGeometry150／DrawingImage30／unique keys195；SVG masters30/30；R5／R6 shared geometry strings0；HTML IDs120／duplicates0／aria-hidden60／transition-all0；contrast text17.06:1／muted8.45:1／plum6.71:1／ice14.13:1；hash ledger39/39；ZIP entries40；preview1600×1200 SHA-256 `41A8AEF261BA2AE96165E15966D86D757F98D4DF20FCD6D417223C7A590BAE31`；candidate XAML SHA-256 `D64E2AFD8B55F8E59DA0A975C4431D255251BB0EF6163CEE6C7F1C7C291F4157`；review ZIP SHA-256 `F9DEC3871BAA3D1BF1D97636CF6650D7893407C704A1966F01F16D78245DA44B`。
- Status：`CANDIDATE_ASSET_REVIEW_NOT_INTEGRATED`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代 Decision183 的 R5 candidate 作為目前審核標的，但保留 approval-before-integration workflow。R1 production 現況與 Decision164–180 的有效產品鎖定不變。

## TCC-DEC-2026-09-29-185 - Free-direction R7 Uses a Precision-instrument Grammar

- 日期：2026-09-29；來源：Human 明確表示「不限定主題，你自由發揮，做一套試試」。
- Direction decision：R7 採 `Nocturne Meridian／夜航子午儀`，以夜間天文儀器與精密製圖為核心語彙；不沿用 R6 冰雪、紅梅、植物枝法或既有主題色分工。色材限定為氧化暗金屬陰槽、象牙刻線、少量老黃銅校準刻度與青綠琺瑯索引，避免遊戲戰利品、卡牌底座與裝飾貼花感。
- Semantic decision：15枚 icons 保留 Home／Market／Plan／Risk／Positions／Review／Settings／Search／Add／More／Minimize／Maximize／Close／Unavailable／Unset 的產品語意，但以觀測穹頂、航圖折面、限位環、軌道儀、回程刻度、調校輪、觀測鏡、測量架及狀態儀盤重新造形。
- Optical decision：每枚 icon 擁有獨立24px與16px master，共30 SVG；24px 使用 Core／Fine／Accent 三層，16px 使用重新簡化的 Core／Accent 兩層，不以線性縮放代替 optical drawing。WPF 包含4 Brush、5 Pen、75 StreamGeometry與30 DrawingImage，共114 resources。
- Clean-room decision：R7 generator 不包含 `RimePlumEngraving`、`FrostboundPlum`、`Obsidian` 或 `Frostcut` identifiers；R6 僅在生成後參與 exact normalized geometry comparison，R6／R7 intersection為0。R6 保留為上一個候選，不被修改。
- Isolation decision：R7 全部檔案只存在 `output/strata-observatory-themed-icon-pack-r7/`；`src/`／`tests/` 對 `Tcc.NocturneMeridian.*` references0；未修改 `MainWindow.xaml` 或 production resource wiring。
- Evidence：WPF XamlReader114/114 resources PASS；StreamGeometry75／DrawingImage30／unique keys114；SVG masters30/30；HTML duplicate IDs0／aria-hidden60／transition-all0；contrast ivory14.91:1／brass8.26:1／teal9.43:1／muted7.56:1；hash ledger39/39；ZIP entries40；preview1600×1200 SHA-256 `1DAC80C937C2E7D2A1970889EA2C8471079CA067B32770115354D3B412637C63`；candidate XAML SHA-256 `8487592F8259126EC498CA982E243C6B49E7DD39A37D853AE767DF54F09D5B8D`；review ZIP SHA-256 `E26E4194BD5E5AF58CAB6BDC1A99CE70D3A50CA20075B5232F6E49E88204CB5A`。
- Status：`CANDIDATE_ASSET_REVIEW_NOT_INTEGRATED`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策將 R7 設為目前 Human review target；R6 保留為先前候選證據，Decision181 的 approval-before-integration workflow 與 R1 production 現況繼續有效。

## TCC-DEC-2026-09-29-186 - Nocturne Meridian R7 Is Installed Through Stable Styles and Dynamic Theme Tokens

- 日期：2026-09-29；來源：Human 在審閱 R7 素材後明確要求「安裝看看」。
- Scope decision：本輪授權從 candidate review 進入 bounded product integration。只允許替換 icon artwork、接線與直接相關測試；背景、人物、前景、卡片、Safety Core、Automation、資料真值與功能行為不得改變。
- Resource decision：產品 `StrataObservatory.Iconography.xaml` 直接承載75個 StreamGeometry、30個 DrawingImage、5個 Pen與16個 Style，共126個唯一 resources。現有 `Strata.*Glyph` Style keys 保持不變，因此 `MainWindow.xaml` 的控制語意與 Automation contract 不需重構。
- Accessibility decision：候選板的4個固定色 Brush 不進產品。Shadow／Ivory／Brass／Enamel 分別改接 `Tcc.Strata.Brush.Background`／`Text`／`Gold`／`Ice` DynamicResource；raw hex0、ReferenceMaster0、FontFamily0，沿用現行 High Contrast palette replacement。Watchlist More 由4×16調為16×16以容納橫向三環語意，其餘 control dimensions不變。
- Runtime evidence：exact Release x64於1672×941及1280×720、DPI96／Windows Text Scale152%各27/27 UIA visible、20/20 buttons named、offscreen0；screenshots SHA-256 `3530995D3CAA4A4CF582031246A75FDBC9454018AE5F41BA875967753DEC6A35`／`9CFDEA93F3AD007DEA6B8B82B34BD1B0C42BAAAFA5421F03DCB56DBA4FC192BD`。Iconography SHA-256 `66A6C43E8A80AB233421D8DC17F8547BF8906C480A755CC925CA1F5453298E06`。
- Verification：locked restore PASS；focused HOME17/17 PASS；DesktopHost exact Release x64／solution Release x64 builds0 warnings/errors；full1740/1836 PASS、96 historical FAIL；Frozen-name filter5/10 PASS、5 historical FAIL；XamlReader126/126；keys126 unique／StreamGeometry75／DrawingImage30／Styles16；raw hex／ReferenceMaster／old geometry0/0/0。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策把 Decision185 的 R7 candidate 升為目前產品 icon implementation；Decision181 的 standalone approval gate 已履行。R6及所有舊候選保留為歷史證據，Decision164–180 對場景、材質、Safety Core與 operational truth 的有效鎖定繼續成立。

## TCC-DEC-2026-09-29-187 - Application Identity Uses a Dedicated Meridian Monogram with Optical Masters

- 日期：2026-09-29；來源：Human 明確要求「程式的icon也設計一個」。
- Identity decision：App Icon 不重用既有 Home／Navigation glyph；另建 `TCC T-monogram × meridian instrument` 品牌章，以夜色場、象牙 T、老黃銅量測環與單一青綠琺瑯索引構成，與 Nocturne Meridian 同源但具獨立品牌辨識。
- Optical decision：256px、32px、16px 各自擁有獨立 SVG master；微型版本主動移除細刻度並加粗 frame／monogram，不以單一大圖縮放代替 optical drawing。三份 SVG 均含 title／description、無 text element；輸出16／20／24／32／40／48／64／128／256px 九層32-bit PNG frame。
- Packaging decision：九層 PNG 以 ICO directory 封裝為 `Tcc.NocturneMeridian.AppIcon.ico` 並透過 `ApplicationIcon` 嵌入 EXE；WPF `Window.Icon` 使用同設計256px PNG pack resource。原因是 Windows Shell 能讀取全 PNG-frame ICO，但 WPF XAML decoder 對該容器回報 `0x88982F60`；視窗圖示因此在 `InitializeComponent()` 後由 absolute pack URI 載入，亦避免 detached XAML tests 需要 package resource context。
- Scope decision：只新增 `Assets/Brand`、EXE／Window wiring、focused contract、review outputs 與 governance records；現行 iconography、背景、人物、前景、卡片、Safety Core、Automation 與產品資料語意不變。
- Evidence：ICO frames9/9／PNG signatures9/9；compiled EXE associated icon32×32可抽取；focused HOME18/18 PASS；locked restore PASS；DesktopHost／solution Release x64 build0 warnings/errors；full1741/1837 PASS、96 historical FAIL；Frozen5/10 PASS、5 historical FAIL；Runtime1672×941、DPI96／Text Scale152%、27/27 UIA visible、20/20 buttons named、offscreen0；Runtime hash仍為 `3530995D3CAA4A4CF582031246A75FDBC9454018AE5F41BA875967753DEC6A35`。
- Asset hashes：ICO `F43179378B6E2B1B5366E8349B9917E7B61BE59D5C5A9F90069A1D5C02FA9B9E`；256 SVG `A33A5C1910BC36821C14D7EAF9FF00F5DD9382A02B017C61F6C94953E5C540E3`；Window PNG `FEC6FEDE9B47A57B8F4946D916A1CD766951AFB0C7EE64AF0EE410F075A308C1`；512 preview `0EBE732D917BE5B67A00F6A4DE72DE00B1A9248C5CF76B5D25B1EDDF8DFA2634`。
- Status：`READY_FOR_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有 commit、tag、push、release 或 deploy。
- 取代：本決策新增 App identity，不取代 Decision186 的 product iconography；Decision164–186 的其餘有效鎖定繼續成立。

## TCC-DEC-2026-09-29-188 - Character Generations Use One Permanent Identity Master

- 日期：2026-09-29；來源：Human 明確要求「臉跑了，之後生成都看這張」。
- Identity authority：所有後續含人物場景的生成，必須直接引用 `C:\Users\danny\.codex\generated_images\01a0a657-c32a-7812-a485-9fa95fc669d5\精選素材\exec-6c6a6c34-2bcf-4141-96b5-bc2df4e118ae.png`；任何生成圖、母版截圖或替代肖像不得供應或混合人物身份。
- Face reinforcement：允許使用 `output/master-fidelity-r1/guides/IdentityMaster.FaceAnchor.SourceCrop.png`，但它只能是上述同一母圖的無損提示裁切，不是新身份來源，也不得作為產品資產。
- Geometry separation：人物尺寸、位置與山腰景深只能由不含人臉的幾何 guide 或文字座標約束；guide 沒有風格或身份權威，且不得出現在成品。
- Generation rule：失敗版本不得換臉、修臉或局部補丁；必須保留固定身份母圖，調整生成條件後整張重生。
- Status：`ACTIVE_IDENTITY_LOCK`；R10 仍為待 Human 審核候選，尚未接入 WPF，沒有 commit、tag、push、release 或 deploy。
- 取代：本決策取代任何允許母版人物或生成圖參與身份推導的暫時做法；不取代既有功能、Safety Core、accessibility 或主題架構決策。

## TCC-DEC-2026-09-29-189 - HOME Scene Uses Independently Approved Landscape, Character and Depth-Plum Layers

- 日期：2026-09-29；來源：Human 明確改定「1.生成景觀底圖、2.人物圖、3.景深紅梅、最後合併；景觀先3張」。
- Workflow decision：停止要求生成器在同一張圖同時解決景觀、人物身份與近景紅梅；改由景觀底圖、人物圖、景深紅梅三個結構單位各自生成、人工過審，最後才合併並統一光向、色溫與景深。
- Current landscape gate：A／B／C 均為 `1672×941`、無人物、無紅梅、無UI的standalone candidates；Human 選定景觀前，不得開始人物圖或景深紅梅正式候選，也不得接入WPF。
- Layout decision：全畫面左側79%保留為卡片安全區，右側21%為人物主體區；透明髮絲／衣帶的跨界只在後續合併驗收時判定。
- Identity continuity：Decision188 的永久人物身份母圖保持有效；景觀候選不得供應或混合人物身份。
- Status：`LANDSCAPE_SELECTION_PENDING`；沒有commit、tag、push、release或deploy。
- 取代：本決策取代R10–R19期間的單次完整場景生成與臨時兩階段整圖做法；不取代Decision188人物身份鎖或任何產品功能／Safety／accessibility決策。

## TCC-DEC-2026-09-29-190 - Character Layer Receives Three Identity-Locked Transparent Candidates

- 日期：2026-09-29；來源：Human 在景觀候選仍未選定時明確要求「生成人物三張」。
- Authorization decision：本次明確指令解除 Decision189「必須先選定景觀才能生成正式人物候選」的順序限制，但不代表任何景觀或人物已過審；景深紅梅、合併與WPF接入仍受人工選版 gate 約束。
- Identity decision：三張人物候選都只直接引用 Decision188 的永久身份母圖；不使用景觀、母版截圖、舊人物或其他生成圖混合身份。A為最接近母圖的冷靜神情，B只稍微柔化眼神與嘴角，C只略降低眼瞼並增加衣帶動勢。
- Asset decision：`Character.A.ReferenceCalm.png`、`Character.B.SofterDreamlike.png`、`Character.C.ContemplativeFlow.png` 均為 `1024×1536 Format32bppArgb` standalone transparent PNG；無景觀、月亮、紅梅、建築、UI或文字。Hash 分別為 `BE737FEB12516F67D28AD541C3701F4C2985A672980FAA70CD91B79D64DD6659`、`781B8A29F028ECC4D444467B5CD59A3D86E3F05F4E79EE8E3274B6018CCBC01D`、`B2AF4D05C48657D4453C24455C2DD8F18A15BF5A8D7BB07F0A75D6C3D5ED2722`。
- Status：`CHARACTER_A_B_C_AWAITING_HUMAN_SELECTION`；三張均未進入產品資產、XAML、Build或Runtime。
- 取代：僅取代 Decision189 的候選生成順序限制；Decision188身份鎖與Decision189分層過審／最後合併規則保持有效。

## TCC-DEC-2026-09-29-191 - Composite Preview Uses the Exact Identity Authority with Deterministic Scene Grading

- 日期：2026-09-29；來源：Human 指定「你用這張就好了，先放進去看看」，並要求調整人物素材亮度與質感以融入背景。
- Character decision：停止採用 Decision190 的生成候選；改用 `TCC_CHARACTER_IDENTITY_AUTHORITY.png`，其 SHA-256 `A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D` 與 Human 指定檔完全一致。人物不重新生成、不換臉、不局部重畫。
- Preview decision：以 Landscape A 作測試底圖；人物固定放置 `x=1180, y=81, width=573, height=860`。R1 因人物來源下緣停在畫面內而出現水平接縫，未列為候選；R2 將來源下緣貼齊941px視窗底部，消除該接縫。
- Grading decision：R3 保持 R2 幾何、Alpha、臉與人物內容，只套用確定性 ColorMatrix：R `0.68`、G `0.75`、B `0.86`、Alpha `0.97`，並加入極小冷藍 offset，使白裘、膚色、黑髮與護腕共享背景月夜的曝光與色溫。R3 SHA-256 `D99ED78A1209F3B9921689AB8211F1856032FE9AF26136814D36A61CF04BF923`。
- Status：`COMPOSITE_PREVIEW_R3_AWAITING_HUMAN_REVIEW`；景深紅梅、產品資產替換、XAML、Build與Runtime均未開始。
- 取代：本決策取代 Decision190 的人物候選選版路徑；Decision188身份鎖與Decision189分層過審／最後合併規則保持有效。

## TCC-DEC-2026-09-29-192 - R20 Returns to Two-Reference Unified Generation with Restrained Depth Plum

- 日期：2026-09-29；來源：Human 退回 deterministic composite 並明確要求「拿這兩張去生成吧，再加入紅梅」。
- Input authority：本輪只使用 Landscape A 與 Decision188 永久人物母圖兩張原始參考；不使用 R1–R3 deterministic composites、Character A／B／C、第一個失敗兩參考輸出、舊WPF資產或其他歷史場景作生成輸入。
- Generation decision：整張1672×941重新共同生成，使人物、雪山、河谷、岩台、霧、月光與紅梅共享同一光向、色溫、材質與景深。第一個輸出因人物約占右側三分之一、錯成偏坐姿／橫臂且右下紅梅過重而退回，禁止繼續修改。
- R20 decision：第二次從原始兩參考重生；恢復一手扶領、一手自然垂下持劍，人物核心留在最右側，左79%維持深色卡片安全區。紅梅只保留左上雪枝與右下淺景深枝兩處，禁止中央散花或花牆。輸出 `1672×941 Format24bppRgb`，SHA-256 `A8D05D7C99A59C2AEF865EF0451F958681DE427FFFCE655021955572F8DD2875`。
- Status：`R20_TWO_REFERENCE_GENERATED_SCENE_AWAITING_HUMAN_REVIEW`；product／test references為0，沒有產品資產替換、XAML、Build、Runtime、commit、push或deploy。
- 取代：本決策取代 Decision191 的 deterministic composite 路徑與 Decision189 的獨立景深紅梅生成順序；Decision188人物身份來源與未過審不得接入WPF的gate保持有效。

## TCC-DEC-2026-09-29-193 - R21 Is an Independent Regeneration from the Same Two Original References

- 日期：2026-09-29；來源：Human 要求「這兩張再生成一次」。
- Input custody：R21 只使用 Landscape A 與 Decision188 永久人物母圖；未讀取、修改或引用 R20 作生成輸入，因此不是 R20 的 edit／repair。
- Variation decision：維持右側站姿、一手扶領／一手垂下持劍、左79%卡片安全區與左上／右下兩處節制紅梅；相較 R20，R21 人物更直立、神情更柔，右下紅梅更靠邊，左側山谷更暗。
- Asset：`StrataObservatory.HomeBackground.R21.TwoReference.GeneratedWithDepthPlum.Candidate.png`；`1672×941 Format24bppRgb`；SHA-256 `CB286F743F7EDE9CAA629983F5287BD40CE7869E68DDFD79DF63A7BCDC5C97A2`。
- Status：`R20_R21_COMPARISON_AWAITING_HUMAN_REVIEW`；product／test references為0，沒有產品資產替換、XAML、Build、Runtime、commit、push或deploy。
- 取代：NONE。R20 保留為並列候選；Decision188人物身份來源與未過審不得接入WPF的gate保持有效。

## TCC-DEC-2026-09-29-194 - R22 and R23 Explore Temperament without Changing Input Authority

- 日期：2026-09-29；來源：Human 指出「氣質差了一點」並要求再生成兩個版本。
- Input custody：R22／R23 均只使用 Landscape A 與 Decision188 永久人物母圖；沒有將 R20、R21 或彼此作為生成／修改輸入。
- Temperament split：R22 以放鬆眉間與下顎、平視遠谷、柔和半垂眼瞼呈現更輕、更遠、更安靜的空靈感；R23 僅略降下巴、保留可見瞳孔，以較內省、夢幻但不憂鬱的神情呈現第二條氣質方向。兩版都禁止嚴肅、戰鬥、傲慢、媚態、娃娃感與動漫大眼。
- Assets：R22 `1672×941 Format24bppRgb`，SHA-256 `47C1E54DAEF2F82233BEF2F781480132BD94844111B62853C5F9F2AC9F7D030E`；R23 `1672×941 Format24bppRgb`，SHA-256 `0470CD6D5B0F7B5AD03C121C7C02850C9A12AC3689300E6DB0668D2AA08A0F91`。
- Status：`R20_R23_COMPARISON_AWAITING_HUMAN_REVIEW`；product／test references為0，沒有產品資產替換、XAML、Build、Runtime、commit、push或deploy。
- 取代：NONE。R20／R21 保留為並列候選；Decision188人物身份來源與未過審不得接入WPF的gate保持有效。

## TCC-DEC-2026-09-29-195 - Human-Selected R21 Becomes One Integrated WPF Scene

- 日期：2026-09-29；來源：Human 對 R21 明確指示「採用這版 繼續施工吧」。
- Selection decision：R21 成為目前 HOME 場景的 Human-authorized product asset；來源候選與產品副本 SHA-256 均為 `CB286F743F7EDE9CAA629983F5287BD40CE7869E68DDFD79DF63A7BCDC5C97A2`，尺寸 `1672×941`。
- Runtime topology：WPF 以單一 `MasterFidelitySceneImage` 載入完整場景。舊 `MasterR1.Background.png` 與 `MasterR1.Character.png` 不再由 `MainWindow.xaml` 或 `Tcc.DesktopHost.csproj` 引用，避免第二層人物素材覆蓋已過審的臉、姿態、光線與景深紅梅。
- Scope decision：本結構單位只替換 master scene 的產品載入拓撲；卡片尺度、字體、圖標、互動、Safety Core、Automation 與離線 truth 不變。沒有刪除、skip 或弱化測試。
- Evidence：focused R21 contract2/2 PASS；locked restore PASS；solution／DesktopHost exact Release x64 builds0 warnings／0 errors；Frozen hashes8/8；舊split product references0；diff check PASS；staged0。雙視窗 Runtime 在DPI96／Text Scale152%均27/27 UIA visible、20/20 buttons named、offscreen0；截圖hash為 `772257AE9367700E609768AF637FDE91B7118DBC785E2328CAD813869FF01080` 與 `527315B64773C4D487827D1530877056599750561325194B13A1A7E49D73A700`。
- Gate conflict：full regression為1738/1839 PASS、101 FAIL；相較已記錄的96個歷史失敗基線，新增5個仍要求舊背景／人物分層拓撲的契約衝突。依規則不自行改寫或弱化歷史契約；正式全綠完成仍需 Human 明確授權 contract migration 或指示回滾。
- Status：`R21_INTEGRATED_AWAITING_HUMAN_RUNTIME_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有commit、tag、push、release或deploy。
- 取代：本決策將 Decision193 的 R21 候選升格為目前產品 scene，並取代 Decision194 的 R20–R23 待選狀態；Decision188 的身份來源紀錄仍作為生成 provenance 保留。

## TCC-DEC-2026-09-29-196 - R21 Runtime Is Rejected and R24 Restores the Card Breathing Corridor

- 日期：2026-09-29；來源：Human 對實機結果指出「人物太靠左了，跟卡片很擠壓」。
- Verdict：R21 standalone 圖片曾獲准接入，但 WPF Runtime 構圖驗收失敗；失敗點是白裘／衣袍主體在卡片右界前後形成擠壓，缺乏暗色呼吸帶。Decision195 的技術驗證證據保留，但其 Human Runtime acceptance 被本決策撤回。
- Regeneration rule：禁止把 R21 平移、裁切、補洞、局部重畫或修臉。R24 從 Landscape A 與 Decision188 永久人物母圖兩個原始 authority 重新完整生成；R21 不作生成輸入。
- Geometry decision：左75%保留暗色WPF工作區；卡片右界外建立約35–50px暗色緩衝；人物實體白裘／衣袍主體約由全寬77–78%後開始，只有少量髮絲或透明飄帶可輕微跨界。人物不硬鎖舊尺寸，改為略縮小並自然收進右側。
- Asset：`StrataObservatory.HomeBackground.R24.RightBreathingCorridor.Candidate.png`；`1672×941 Format24bppRgb`；SHA-256 `0919779DEA767660EB44AF57F9E353D3547A1FEC4745DD8B18A2F2ACD0973BEB`。
- Status：`R24_AWAITING_HUMAN_VISUAL_REVIEW`；R24尚未接入產品。R21暫留現行WPF資產，等待Human選擇R24接受、再生或回滾；沒有commit、tag、push、release或deploy。
- 取代：撤回Decision195的Human Runtime acceptance，不抹除其產品接入與驗證史實；Decision188人物身份來源與失敗版本不得局部修改的規則繼續有效。

## TCC-DEC-2026-09-29-197 - R24 Face Failure Is Rejected and Repeated Scale Drift Stops Blind Regeneration

- 日期：2026-09-29；來源：Human 指出R24「臉歪了，重來」。
- Rejection：R24臉部眼軸、鼻樑與下顎透視不自然，已移入rejected；禁止修臉、扶正、局部補繪或把R24作後續生成來源。
- Identity reinforcement：後續重生仍只以Decision188人物母圖為身份 authority，另使用同一母圖的無損臉部裁切加強骨相；Landscape A提供環境，不含身份的抽象guide只提供右側包絡與卡片呼吸帶。
- Repeated-error diagnosis：當提示提高臉部可讀性時，生成器反覆放大人物並向左展示完整衣袍；加強出框要求又導致巨型肖像。依規範停止盲目retry，不把任何自動退回輸出複製進專案。
- R25：唯一同時達到自然臉軸與右側間距的輸出為 `StrataObservatory.HomeBackground.R25.FaceAxis.RightCorridor.Candidate.png`，SHA-256 `17B1CCE17BB16ED6995340C36D1250BE696898934C9DA2233CCB713A47765699`；但尺寸為 `1671×941`，不符合locked `1672×941`，只能供Human判斷臉與氣質，禁止接入WPF。
- Status：`R25_VISUAL_REVIEW_WITH_DIMENSION_BLOCKER`；R21仍暫留產品；沒有commit、tag、push、release或deploy。
- 取代：R24的候選狀態被撤回；Decision196的R21 Runtime rejection與Decision188身份來源規則保持有效。

## TCC-DEC-2026-09-29-198 - Two Human-Supplied Scenes Receive Isolated WPF Runtime Comparison

- 日期：2026-09-29；來源：Human 明確要求「這兩版分別接入WPF看看」。
- Comparison scope：Variant A／B僅逐一暫時覆蓋現有單一scene資源，使用同一份XAML、字體、卡片、icon、Safety Core、離線狀態與Automation結構擷取Runtime；這不是任何一版的產品接受或持久接入授權。
- Input custody：A為`1672×941`、SHA-256 `3EFA71B8CCEC0DDAA8D45FDFF5F9C290F2426029936CD656FF59BC7D0190805E`；B為`1672×941`、SHA-256 `B4C4B7225D75C5A3408E8BF683EEDA7756C6E4398560021BC1F4FE2996C385C2`。
- Runtime evidence：A／B都在相同`1672×941`視窗、DPI96、Windows Text Scale152%下擷取；A screenshot SHA-256 `8BF58435B0D2832FF467433E26419E70A9A4B3B974FE78974B8C33B215BFEA3A`，B screenshot SHA-256 `4796EE5A6AB4A3D1C08A172B5191EFB5CA56E051600850153788FC417D24B6A8`；兩者皆27/27 UIA visible、20/20 buttons named、offscreen0。
- Resource correctness：首次B增量建置沿用舊內嵌資源，因截圖hash與A相同而被判無效；正式B證據改用`Rebuild`後重新擷取，證明A／B Runtime hash不同。往後更換WPF內嵌scene做比較時必須強制Rebuild，不得只依賴incremental build。
- Restoration：比較完成後，產品資源已回復R21 exact hash `CB286F743F7EDE9CAA629983F5287BD40CE7869E68DDFD79DF63A7BCDC5C97A2`，Release執行檔亦完成Rebuild。沒有commit、tag、push、release或deploy。
- Status：`TWO_VERSION_WPF_RUNTIME_COMPARISON_AWAITING_HUMAN_SELECTION`；Human可選A、B或兩版都退回。
- 取代：NONE。Decision196的R21 Runtime rejection仍有效；本決策只建立可比較證據，不宣告新產品scene。

## TCC-DEC-2026-09-29-199 - Human Selects Variant B as the R26 Product Scene

- 日期：2026-09-29；來源：Human在A／B實際WPF比較後明確選擇「B」。
- Selection：Variant B升格為`StrataObservatory.MasterR1.Scene.R26.png`；來源與產品副本皆為`1672×941`，SHA-256 `B4C4B7225D75C5A3408E8BF683EEDA7756C6E4398560021BC1F4FE2996C385C2`，byte-identical。
- Product wiring：`MainWindow.xaml`與`Tcc.DesktopHost.csproj`各只引用R26一次；R21與舊MasterR1 background／character分層產品引用均為0。R21檔案只保留歷史追溯，不再是Runtime資源。
- Contract：原R21精準契約改名為`MasterFidelityR26SceneTests`，鎖定R26路徑、雜湊、1672×941框架與單一完整場景拓撲；未刪除、skip或弱化測試。
- Evidence：focused2/2 PASS；locked restore PASS；DesktopHost exact Release x64 Rebuild與solution Release x64 Build均0 warnings／0 errors；Frozen8/8；正式Runtime1672×941 screenshot SHA-256 `4796EE5A6AB4A3D1C08A172B5191EFB5CA56E051600850153788FC417D24B6A8`，與B比較證據完全一致；minimum viewport1280×720 screenshot SHA-256 `7308DC5807F0DC61211EC5E11574543359AED96CBE050CD3DCE7C9FA811CBBD3`。兩者均DPI96、Text Scale152%、27/27 UIA visible、20/20 buttons named、offscreen0；1280×720無卡片裁斷、人物錯位、截字或區塊溢出。
- Gate conflict：full regression為`1738/1839 PASS`、`101 FAIL`，與R26接入前基線完全相同；這101項仍包含Phase6 authority與被取代的P1-P3／split-scene歷史契約。依規則不自行移除或弱化，故Repository正式全綠完成與commit仍不允許。
- Status：`R26_SELECTED_B_INTEGRATED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；沒有commit、tag、push、release或deploy。
- 取代：Variant B不再只是Decision198比較稿；它取代R21作為目前產品scene。Decision196對R21 Runtime的退回維持歷史有效。

## TCC-DEC-2026-09-29-200 - R26 Offline Market Retains a Quiet Analytical Scaffold

- 日期：2026-09-29；來源：Human在R26 B接入後要求繼續施工，並沿用先前的矩陣式逐單位精修規則。
- Scope：R26場景、人物、21%人物區、七個外框、五張冰紋、圖示、文字、互動、UIA及offline／unknown真值全部鎖定；本輪只允許Market chart grid的單一normal-mode token校正。
- Decision：`Tcc.Strata.Brush.ChartGrid`由`#285D7481`改為`#345D7481`；只提高alpha，RGB、1px線寬、五條水平線、ChartAxis及High Contrast system-color override不變。禁止新增價格、日期、走勢或任何暗示live data的內容。
- Matrix／rollback：正式矩陣為`docs/design/TCC_STRATA_R26_ANALYTICAL_SCAFFOLD_R1_MATRIX.md`。若格線成為視覺焦點，整個單位回復`#285D7481`，不得在失敗版本上疊加局部裝飾。
- Evidence：五條格線平均局部對比由6.596增至9.105，增益2.509；native／minimum Runtime hash為`19CFF01B...6047`／`9A8DEB72...FE25`，兩者DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0。focused1/1與R26 custody2/2 PASS；locked restore PASS；兩個Release builds0 warnings／0 errors；Frozen8/8；full1739/1840 PASS、101個既有FAIL。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；沒有commit、tag、push、release或deploy。
- 取代：NONE。Decision199的R26 scene acceptance與Decision164–180的geometry／material／truth／accessibility locks全部繼續有效。

## TCC-DEC-2026-09-29-201 - R26 Cards Remove Rejected Ice Ornament and Restore a Two-Pixel Turn

- 日期：2026-09-29；來源：Human明確指出母版卡片只有些微圓角，且現行冰紋不如母版，要求先移除。
- Scope：只處理卡片外觀結構單位。R26場景／人物、五張卡片與Safety Core的矩形、內容、間距、文字、互動、UIA與offline真值全部鎖定。
- Decision：`Strata.Panel`基底改用共享`Tcc.Strata.CardCornerRadius=2`，Primary／Secondary／Safety繼承；五個EdgeMaterial Image、csproj Resource、opacity token與High Contrast override完整移除。冰紋PNG保留為未引用歷史素材，不刪除使用者工作樹內容。
- Visual rationale：母版的輪廓近乎方角，只在轉折處略微收邊；2px足以避免完全銳角，又不引入現代大圓角卡片語言。卡片質感回到邊框、頂部glint與半透明深色底面本身。
- Matrix／rollback：正式矩陣為`docs/design/TCC_STRATA_R26_CARD_MATERIAL_CORRECTION_R1_MATRIX.md`。若Human退回，整個卡片材質單位回退；禁止在被退回的冰紋或圓角版本上繼續疊補丁。
- Evidence：focused3/3與R26 custody2/2 PASS；locked restore PASS；DesktopHost Rebuild與solution Release x64 build均0 warnings／0 errors；Frozen8/8；產品EdgeMaterial引用0；雙Runtime hash `E277226C...0069`／`EE5D4983...8F3A`，均DPI96、Text Scale152%、UIA27/27、20/20 buttons named、offscreen0。full仍為1739/1840 PASS、101個既有FAIL。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；沒有commit、tag、push、release或deploy。
- 取代：Decision200中「五張冰紋鎖定」被Human最新明確要求取代；其餘R26 scene、幾何、truth與accessibility locks繼續有效。

## TCC-DEC-2026-09-30-202 - Mental State Replaces the Mechanical Vector Ring with One Approved Brush Asset

- 日期：2026-09-30；來源：Human明確退回舊Mental State圓圖，並指定透明冰墨筆觸圓環作為適合版本。
- Scope：只重建Mental State圓環結構單位。卡片位置／尺寸／圓角、中央狀態文字、輔助文字、其他模組、R26場景與產品真值全部鎖定。
- Decision：刪除十段機械式向量弧與三個`Tcc.Strata.Brush.MentalRing.*` brushes；將Human指定來源逐位元組接入為單一1254×1254 RGBA資產，於既有145×145舞台以HighQuality／Uniform呈現，透明度0.62且不接受焦點或命中測試。High Contrast只將裝飾圖透明度設為0，live center text繼續可見。
- Semantic boundary：正式規格目前只定義`NOT SET`，configured-state display semantics仍是`UNDEFINED`；因此本輪只採共用外環，不把測試fixture中的`CALM`推導成產品狀態分類，也不虛構每種狀態的圖案。狀態圖示家族須等待獨立核准的狀態詞彙與對應矩陣。
- Matrix／rollback：正式矩陣為`docs/design/TCC_STRATA_R26_MENTAL_STATE_BRUSH_RING_R1_MATRIX.md`。若Human退回，整個圓環單位重生或回退，禁止在失敗圖樣上局部補丁。
- Evidence：asset SHA-256 `EC78867EDDAFDD62D4E3F1269555E68C4E68F21A383002CE242CCEB3E78D54FB`；focused5/5 PASS；HOME17/20且只有三個既有舊視覺契約失敗；locked restore6/6 PASS；DesktopHost Release x64 rebuild與solution Release build均0 warnings／0 errors；Frozen11/11 MATCH；full1740/1841 PASS、101個既有FAIL；雙Runtime hash `F59B043D...D645`／`651C065B...352`，均DPI96、Text Scale152%、UIA27/27、20/20 buttons named、offscreen0。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；沒有commit、tag、push、release或deploy。
- 取代：先前第八輪以向量Path構成Mental State圓環的局部決策；Decision201卡片材質、R26 scene、truth與accessibility locks繼續有效。

## TCC-DEC-2026-09-30-203 - Seven Mental-State Concepts Become an Isolated 64-Grid Vector Candidate

- 日期：2026-09-30；來源：Human接受「以概念母版筆觸語言、以辨識邏輯從零重建七張獨立透明圖標，並在32／48／64px驗收」的建議。
- Scope boundary：本輪只建立`output/strata-mental-state-vector-r1/`候選包；禁止修改`src/`、`tests/`或目前產品中的共同筆觸圓環。Decision202所記錄的正式產品truth與configured-state semantics `UNDEFINED`維持不變。
- Construction decision：七枚圖示共用64×64網格與8–56安全區；使用8px深色undercut、4.4px冰墨主筆、1.25px冷光脊。NOT SET的兩筆完全分離；STRESSED採開放單一路徑以降低黑團；IMPULSIVE只保留短促爆發並獨占單一朱紅種子；其餘狀態禁止朱紅。所有狀態避免依賴共同圓框。
- Semantics boundary：NOT SET／CALM／FOCUSED／CAUTIOUS／STRESSED／FATIGUED／IMPULSIVE均是候選視覺語意，不構成核准的產品狀態列舉或mapping。正式接入前必須另有Human核准的vocabulary／mapping與獨立integration phase。
- Assets：7個standalone SVG、31個WPF ResourceDictionary資源、1600×1200 HTML／PNG對照板、manifest、設計矩陣、review notes、hash ledger及生成／驗證腳本。Preview SHA-256 `5F391C4655C15216BBFC2E663AE14FC7C0A7BDE2EB7A07E407E3BAF3EB092113`；XAML SHA-256 `B1E561FD3C10ED47DD3CC4AE0098972076D7A7075F9ED1780947D7E924EC13DE`。
- Evidence：SVG7/7；WPF XamlReader31/31、StreamGeometry14、EllipseGeometry2、DrawingImage7、unique keys31/31；96／120／144／192 DPI共28/28 WPF render非空；32／48／64px深淺底視覺證據完整；hash ledger16/16；production／test references0。
- Status：`CANDIDATE_VECTOR_FAMILY_AWAITING_HUMAN_ART_REVIEW`；沒有產品整合、Build／Runtime語意變更、commit、tag、push、release或deploy。
- 取代：NONE。Decision202仍是目前產品Mental State視覺與語意authority。

## TCC-DEC-2026-09-30-204 - NOT SET Receives an Isolated WPF Runtime Preview without State Mapping

- 日期：2026-09-30；來源：Human在退回整套向量方案、改以材質與輪廓分離生成四張獨立raster後，明確要求「好 實裝看看」。
- Scope：只將獨立生成的`NOT SET`候選放入現有Mental State145×145舞台；FOCUSED／STRESSED／IMPULSIVE與未重新生成的CALM／CAUTIOUS／FATIGUED均不接入產品，不建立enum、persistence、mapping或runtime切換。
- Product wiring：新增`StrataObservatory.MentalState.NotSetGlyph.R1.png`，1254×1254 RGBA，SHA-256`830FC8361EF1731BE4631B4466957EAC3E7DA0831F8C97F9736CE84CF1AF3341`。XAML／csproj各1個引用；Decision202 common brush ring產品引用歸零但歷史素材不刪除。Image及opacity token改用`MentalStateGlyph`語意；High Contrast仍只隱藏裝飾圖，`NOT SET`／`NO SELF-CHECK`保留。
- Runtime finding：在來源尺寸可見的兩個分離墨塊，縮至145px後外輪廓聚合成近似完整圓環。文字、卡片、縮放、UIA均正常，但此版本只取得技術預覽資格，未自動取得Human美術接受。
- Evidence：focused1/1 PASS；HOME17/20且只有三個既有舊視覺契約失敗；locked restore6/6 PASS；兩個Release builds0 warnings／0 errors；Frozen authority／accepted-spec hashes11/11 MATCH；full1740/1841 PASS、101個既有FAIL；雙Runtime hash`B2793BB9...FFA5`／`7FF3BF95...BC8F`，均DPI96、Text Scale152%、UIA27/27、20/20 buttons named、offscreen0；new refs1/1、old refs0/0、other-state refs0、staged0、diff check PASS。
- Status：`RUNTIME_PREVIEW_TECHNICAL_PASS_AWAITING_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有commit、tag、push、release或deploy。
- 取代：Decision202的common brush ring目前不再是active產品引用，但仍是本預覽的完整rollback baseline；Decision203的向量候選已被Human視覺退回，維持隔離且不得接入。

## TCC-DEC-2026-09-30-205 - NOT SET R2 Breaks the Shared Circular Rhythm at the Actual 145-Pixel Stage

- 日期：2026-09-30；來源：Human同意依「強化斷口、擴大中央負空間、直接以145px驗收、禁止修改失敗圖」重生並實裝。
- Scope：只替換active `NOT SET`裝飾glyph。145×145舞台、`NOT SET`／`NO SELF-CHECK` live truth、Mental State rows、卡片、R26場景、High Contrast與UIA全部鎖定；不建立其他狀態mapping。
- Generation decision：所有R2嘗試均從零生成且不提供參考圖／edit base。第一張雖有大斷口，兩段仍共享隱形圓周，於145px仍可能補成破圓，因此未接入。第二張改用不同軸線、長度與曲率的兩筆冰墨殘片，通過145px preflight後才進入WPF。
- Product wiring：active資產為`StrataObservatory.MentalState.NotSetGlyph.R2.png`，1254×1254 RGBA、SHA-256`319800BCCFC17A0AA0A12F88699715D52AC323F4FD4C0C89C558DD1386909C14`。XAML／csproj各1個引用；R1保留但active引用0，common ring與其他state raster引用0。
- Runtime finding：兩筆在1672×941及1280×720 Runtime仍明確分離，不再形成完整或破圓。live copy與四列狀態無裁切／重疊，UIA27/27、buttons20/20 named、offscreen0。
- Cache discipline：第一次1672擷取得到R1完全相同hash，判定為舊WPF embedded resource快取並作廢；強制Rebuild實際`bin/x64`擷取路徑後，R2有效hash為`36E0870D...FDFD`／`4174E491...21F0`。只要替換embedded bitmap，Runtime證據前必須重建實際啟動路徑。
- Evidence：focused1/1 PASS；HOME17/20且仍只有三個既有失敗；locked restore6/6；Release builds0 warnings／0 errors；Frozen11/11 MATCH；full TRX1740/1841 PASS、101個既有FAIL、error／timeout／aborted0；R2 refs1/1、R1／common ring／other-state refs0；`git diff --check` PASS、staged0、HEAD`d8756402e7a5`。
- Status：`IMPLEMENTED_RUNTIME_PASS_AWAITING_HUMAN_VISUAL_REVIEW_WITH_LEGACY_GATE_CONFLICT`；沒有commit、tag、push、release或deploy。
- 取代：Decision204的R1 active preview與其近圓美術方向；Decision202 common ring及R1均保留歷史／rollback provenance，不再是active產品引用。

## TCC-DEC-2026-09-30-206 - Human Accepts NOT SET R2 as the Active Mental-State Visual

- 日期：2026-09-30；來源：在assistant明確詢問是否將R2視為通過並繼續下一個結構單位後，Human回答「好」。
- Acceptance：`StrataObservatory.MentalState.NotSetGlyph.R2.png`取得Human美術接受，維持active `NOT SET`資產；既有145px、雙viewport Runtime、High Contrast、UIA與focused evidence繼續有效。
- Boundary：此接受只涵蓋目前正式產品唯一存在的`NOT SET`視覺，不批准其他狀態的產品詞彙、enum、persistence、mapping或runtime切換。
- Status：`HUMAN_ACCEPTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；完整repository仍有101個已揭露歷史失敗，因此不宣告全綠、不允許commit或下一implementation phase。
- 取代：Decision205中的「awaiting Human visual review」狀態；其餘scope、cache discipline、資產hash與rollback規則不變。

## TCC-DEC-2026-09-30-207 - FOCUSED R1 Uses an Open Four-Facet Gate as an Isolated Candidate

- 日期：2026-09-30；來源：Human接受R2後同意繼續下一個結構單位；施工規則仍為失敗禁止修改、從零重生、先以實際145px驗收。
- Scope：只生成`FOCUSED`候選資產與145px預覽，全部留在`output/`；禁止修改`src/`、`tests/`或active WPF資源，禁止建立產品狀態mapping。
- Rejection record：R1A在145px像隨機碎冰；R1B的單側匯聚形成箭頭；兩者均完整退回且不作局部修補。
- Candidate：R1C由四枚不相連、方向內收的冰墨刻面包圍中央負空間，形成開放式聚焦門；1254×1254 RGBA，SHA-256`51BFD837D2E3227F731F90959A8BCBFA3F6A6FB0FC7B349EE40F9995F5381074`，145px preview SHA-256`B306666BC5D7C82195210F1BA2B1B365C90C818A86D8028204AFD1BF0461234B`。
- Status：`ISOLATED_CANDIDATE_AWAITING_HUMAN_ART_REVIEW`；產品／測試引用0，沒有Build／Runtime語意變更、commit、tag、push、release或deploy。
- 取代：NONE。Decision206的accepted R2仍是唯一active product glyph；R1C只有候選資格。

## TCC-DEC-2026-09-30-208 - Human Accepts FOCUSED R1C as an Isolated Art Candidate

- 日期：2026-09-30；來源：Human檢視R1C的145×145實際尺寸預覽後明確回答「好 通過」。
- Acceptance：R1C的四枚內收冰墨刻面、中央負空間與開放式聚焦門語意取得Human美術接受；R1A／R1B仍維持退回，不得作為後續修改來源。
- Boundary：本次只接受隔離的美術候選。未授權WPF接入、產品狀態詞彙、enum、persistence、mapping、runtime切換或其他state glyph施工。
- Evidence：1254×1254 RGBA候選SHA-256`51BFD837D2E3227F731F90959A8BCBFA3F6A6FB0FC7B349EE40F9995F5381074`；145px preview SHA-256`B306666BC5D7C82195210F1BA2B1B365C90C818A86D8028204AFD1BF0461234B`；產品／測試引用0。
- Status：`HUMAN_ACCEPTED_ISOLATED_CANDIDATE`；沒有產品原始碼變更、Build／Runtime語意變更、commit、tag、push、release或deploy。
- 取代：Decision207中的「awaiting Human art review」狀態；其餘隔離、rollback與semantic boundary繼續有效。

## TCC-DEC-2026-09-30-209 - Full Mental-State WPF Integration Stops at Asset and Mapping Authority Preflight

- 日期：2026-09-30；來源：Human明確要求「整套接入wpf」。
- Authorized objective：建立完整Mental State WPF視覺資源與選擇邊界；不得因「整套」而復活已退回素材或靜默發明Core產品語意。
- Preflight finding：七態中只有`NOT SET` R2與`FOCUSED` R1C取得Human接受。CALM／CAUTIOUS／STRESSED／FATIGUED／IMPULSIVE沒有核准raster；舊七枚Vector R1整套已被Human退回，禁止接入。Runtime正式真值仍只有`NOT SET`，`ApplyReferenceFixture()`中的`CALM`／`FOCUSED`是fixture資料，不是產品enum／mapping authority。
- Gate：Phase 0在任何`src/`／`tests/`修改前停止。必須先取得五枚缺少資產的Human authority，以及preview-only或product-state mapping的明確邊界，才能開始Phase 1 Resource custody。
- Plan：正式分期與rollback矩陣為`docs/design/TCC_STRATA_R26_MENTAL_STATE_SUITE_WPF_R1_PLAN.md`；同一時間只有Phase 0生效。
- Status：`BLOCKED_AT_ASSET_AND_MAPPING_AUTHORITY_PREFLIGHT`；未修改產品原始碼／測試、未執行Build／Runtime、沒有commit、tag、push、release或deploy。
- 取代：Decision208中「等待WPF preview或下一單位」的待辦狀態；Decision206／208的Human接受與Decision203對被退回Vector R1的隔離規則維持有效。

## TCC-DEC-2026-09-30-210 - Seven Mental-State Glyphs Are Generated Once as One Clean-Room Atlas

- 日期：2026-09-30；來源：Human針對Phase0 blocker明確指示「待七枚全部通過後再一次接入WPF，你一次生一套」。
- Generation boundary：不再逐枚生成；七態在同一次built-in ImageGen呼叫中從零生成為一張透明4×2 atlas，上排NOT SET／CALM／FOCUSED／CAUTIOUS，下排STRESSED／FATIGUED／IMPULSIVE／空白。未提供任何舊raster、Vector R1或WPF geometry作參考／edit base。
- Candidate：`StrataObservatory.MentalStateSuite.R1.Candidate.png`，1774×887 Format32bppArgb，SHA-256`EA15A6E24FEE80A3395DDE775E52F340FC2A64AFF6DA40EC1C08C2FCBDFE20C1`。七枚共用冰墨／石墨礦物材質，以輪廓與負空間區分；只有IMPULSIVE含一枚朱紅種子。
- Whole-unit rule：Human以整套為單位接受或退回；退回時整張從零重生，禁止局部修補、裁切重組或只保留部分cell作下一版來源。
- Product boundary：candidate只存在`output/`，產品／測試引用0；active NOT SET R2與accepted standalone FOCUSED R1C不被自動取代。整套通過前禁止WPF接入。
- Status：`COMPLETE_SUITE_R1_AWAITING_HUMAN_ART_REVIEW`；沒有產品原始碼／測試變更、Build／Runtime語意變更、commit、tag、push、release或deploy。
- 取代：Decision209中「缺少五枚候選資產」的阻擋；Human整套接受與產品mapping authority仍待完成。

## TCC-DEC-2026-10-01-211 - Human-Accepted Mental-State Atlas Becomes One Presentation-Only WPF Suite

- 日期：2026-10-01；來源：Human在要求「待七枚全部通過後再一次接入WPF，你一次生一套」後，以`通過`接受Complete Suite R1並授權繼續接入。
- Art authority：`StrataObservatory.MentalStateSuite.R1.Candidate.png`整套升格為Human接受的七態presentation authority；產品副本`StrataObservatory.MentalStateSuite.R1.png`與來源byte-identical，1774×887 RGBA，SHA-256`EA15A6E24FEE80A3395DDE775E52F340FC2A64AFF6DA40EC1C08C2FCBDFE20C1`。
- Packaging decision：保留單一atlas，不做影像重採樣或分檔改圖。因尺寸不能整除4×2，WPF以columns`444／443／444／443`、rows`444／443`切成七個精確整數`CroppedBitmap` presentation resources；空白第八格不註冊。
- Semantic boundary：production預設只顯示NOT SET；既有reference fixture只切換CALM。FOCUSED／CAUTIOUS／STRESSED／FATIGUED／IMPULSIVE只註冊為presentation resources，不新增enum、persistence、Domain／Core mapping或public API。live text仍是狀態truth；High Contrast仍隱藏裝飾glyph。
- Rollback：舊NOT SET R2與standalone FOCUSED R1C保留在磁碟供provenance／rollback，但active references為0。若整套日後退回，整個atlas單位回復或從零重生，禁止局部修補個別cell。
- Evidence：focused1/1 PASS；HOME17/20且仍只有三個既有舊視覺契約失敗；locked restore6/6 PASS；DesktopHost exact Release x64與solution Release x64 builds0 warnings／0 errors；Frozen accepted-specification／authority hashes11/11 MATCH；full1740/1841 PASS、101個既有FAIL，與接入前記錄數量相同。production NOT SET與fixture CALM各在1672×941及1280×720完成Runtime，四張皆DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0。
- Status：`HUMAN_ACCEPTED_WPF_INTEGRATED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`。完整repository仍非全綠；沒有commit、tag、push、release或deploy。
- 取代：Decision210的`AWAITING_HUMAN_ART_REVIEW`與Decision206的active NOT SET R2產品引用；Decision209禁止虛構產品mapping的邊界仍持續有效。

## TCC-DEC-2026-10-01-212 - Offline Market Overview Uses an Explicit Non-Inference Hierarchy

- 日期：2026-10-01；來源：Human開啟`TCC—HOME UI 精修 Phase 2`並建議先完成Market Overview主卡，要求精修標題、頁籤、空態、座標層級與留白，同時維持offline truth、不虛構行情資料。
- 決策：Market Overview保留既有870×404外框與五條主要水平格線；新增低權重垂直guides及左／下axis，只表達分析座標結構。畫面明示`NO VALUES ARE INFERRED`與`PRICES · DATES · TRENDS REMAIN BLANK`，不顯示任何價格、日期、趨勢線或推導值。
- Interaction boundary：symbol／range controls仍全部disabled且保留既有UIA名稱；`SYMBOL`／`RANGE`只改善群組掃讀，不建立可用market interaction或connector behavior。
- 影響範圍：只限Market Overview卡片內部XAML、專用tab styles與focused contracts；Safety Core、導航、Watchlist、其他卡片、R26 scene、Mental Suite、產品語意及dependency graph不變。
- Evidence：focused3/3；HOME18/21且只剩三個既有失敗；R26／Mental Suite custody4/4；locked restore與兩個Release builds通過；Runtime1672×941／1280×720皆DPI96、Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1741/1842 PASS、101個既有FAIL。
- 取代：只取代先前Market Overview卡內的tab spacing、空態輔助文案與單向座標骨架；Decision211及所有Safety／offline／read-only邊界維持有效。

## TCC-DEC-2026-10-01-213 - Safety Core Uses One Primary Offline Alert and Three Neutral Unknown States

- 日期：2026-10-01；來源：Human以「好 進行下一步」接受Market Overview並授權下一個結構單位；額度刷新後依建議的Safety優先層級繼續施工。
- Hierarchy decision：Safety Core維持一條連續640×101 ink strip及四項正式語意，不拆成四張等權卡。Trading Permission取得190px較寬欄位，並以2px Danger導引線、空心marker及`OFFLINE`成為唯一高注意狀態。
- Unknown-state decision：Total Risk、Current Positions與Major Alerts維持中性unset marker及`UNKNOWN`，不得使用Danger／Success色、零值、無警示或任何推導資訊。三個非互動標題移除underline，四欄統一label／value／helper三列baseline。
- Typography／accessibility：新增token-owned `Strata.SafetyLabel`及`Strata.SafetyLabel.Primary`；`Region.HomeSafetyCore`與既有accessible name不變。雙viewport在DPI96／Text Scale152%為UIA27/27、buttons20/20 named、offscreen0，無裁切、重疊或溢出。
- Scope：Safety外框、Header、Market Overview、navigation、scene、raster assets、business semantics、dependency graph及interaction均不變；未新增broker／exchange／execution行為或假資料。
- Evidence：focused3/3；HOME19/22且只剩三個既有失敗；R26／Mental Suite custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；Runtime hashes`6BCF7AAB...E59CB`／`5E1CA6CF...6188D`；full1742/1843 PASS、101個既有FAIL，failure-name delta new0／resolved0。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前Safety Core內部的等距padding、三個underlined label與未明確分層的status節奏；所有Home Safety Core、offline truth、Q93及Decision212邊界持續有效。

## TCC-DEC-2026-10-01-214 - Left Navigation Distinguishes Current Location from Unavailable Destinations

- 日期：2026-10-01；來源：Human明確要求「精修左側導航列」。
- Current-location decision：HOME維持`IsEnabled=False`及`IsTabStop=False`，保留selected surface與2px aged-gold leading border，並新增可見`CURRENT`微文案；目前位置不再只靠色彩表達，也不建立重複導向目前頁的假互動。
- Unavailable-destination decision：Market／Planning／Risk／Positions／Review／Settings維持disabled且command／click為0；原UIA名稱不變，另提供`ItemStatus=Unavailable`、誠實`HelpText`與disabled tooltip。offline與presentation unavailable不得被重新呈現為可進入或即將可用。
- Typography／structure：七個label統一使用navigation-only interface typography；HOME仍使用23px accepted glyph，其餘六個仍為22px；98×889 rail、九列topology、primary／utility divider與Nocturne Meridian R7資源不變，footer改為置中而不提高視覺權重。
- Scope：只限左側導航內容XAML、navigation-only styles、focused contract與治理／Runtime evidence；Safety Core、Market Overview、其他卡片、R26 scene、Mental Suite、business semantics、dependency graph及navigation behavior不變。
- Evidence：focused3/3；HOME20/23且只剩三個既有失敗；R26／Mental Suite custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；Runtime hashes`5F2FFF90...63A4A1`／`3F1D893C...38CD11`，兩者均DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1743/1844 PASS、101個既有FAIL，failure-name delta new0／resolved0。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前左側導航混用display typography、目前頁缺乏非色彩狀態文案、停用目的地只靠UIA Name說明及footer偏左的呈現；Decision212／213與所有offline truth、Q93及產品邊界持續有效。

## TCC-DEC-2026-10-01-215 - Offline Watchlist Uses Equal-Weight Symbols and Neutral Unavailable Values

- 日期：2026-10-01；來源：Human接受Left Navigation後指示「做下一個板塊」；依既有Phase 2相鄰結構順序進入Watchlist。
- Title／state decision：`Watchlist`是卡片標題而非selected tab；移除dormant `Top Movers`與kebab視覺控制，改用靜態`READ ONLY`狀態。UIA Name／HelpText明示五個symbols的price／24H在offline時不可用且不得推導。
- Row-truth decision：BTCUSDT／ETHUSDT／SOLUSDT／BNBUSDT／XRPUSDT維持原順序與等權symbol typography；每列保留neutral unset marker，Price／24H共十格只顯示中性em dash。production不再顯示selected surface、紅色`OFF`、數值、百分比、方向箭頭或success／danger movement。
- Footer／fixture boundary：footer為`DATA UNAVAILABLE · READ ONLY`。`FixtureWatchlistRows`繼續Collapsed，其既有reference selected row、43,287.62及+1.26%等值保持不變且不得成為production truth。
- Scope：只限Watchlist內部XAML、三個Watchlist-only typography styles、focused contracts及治理／Runtime evidence；251×404外框、four-row topology、Market Overview、Safety Core、navigation、其他卡片、R26 scene、Mental Suite、business semantics、dependency graph與connector behavior不變。
- Evidence：focused3/3；HOME21/24且只剩三個既有失敗；custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；Runtime hashes`0C12E70E...B812`／`A371C99E...5A41`，兩者均DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1744/1845 PASS、101個既有FAIL，failure-name delta new0／resolved0；scope scan及Git hygiene PASS。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前Watchlist把標題呈現成selected tab、顯示dormant overflow control、production首列假選取與紅色`OFF`的呈現；Decision212／213／214與所有offline truth、Q93及產品邊界持續有效。

## TCC-DEC-2026-10-01-216 - None-Loaded Priorities Preserve List Rhythm without Checkbox Affordance

- 日期：2026-10-01；來源：Human接受Watchlist R1並同意建議的下一個Priorities結構單位。
- Truth boundary：production仍只有`Priorities, no items loaded, not set`，不得從Reference fixture或approved visual copy推導正式task。Header新增靜態`NOT SET`，UIA HelpText明示priority slots為read-only且editing／completion unavailable。
- List decision：保留現有363×260卡片、three-row shell及01–04四個production slots。第一列只顯示`No priorities loaded`，其餘三列為中性em dash；四個空checkbox輪廓改成neutral unset marker，避免暗示可勾選、完成或persistence。
- Fixture boundary：`PriorityFirstRow`與`PriorityText1`–`PriorityText4` named bindings保留，既有Reference fixture仍可填入四列並標示第一列；fixture資料及style mutation不成為production truth或新產品功能。
- Typography／footer：新增token-owned `Strata.PriorityIndex`、`Strata.PriorityLabel`及`Strata.PriorityPlaceholder`；footer由人工逐字spacing改為`SMALL STEPS · COMPOUNDED`，維持editorial role且降低152% Text Scale裁切風險。
- Scope：Priorities以外的HOME cards、navigation、R26 scene、Mental Suite、business semantics、dependency graph與interaction behavior不變；未新增task creation、editing、completion、selection、reorder、persistence、command或click handler。
- Evidence：focused3/3及被取代contract3/3；HOME22/25且只剩三個既有失敗；custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；Runtime hashes`A89A7716...D0E8C`／`360B8CF1...71B51`，兩者均DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1745/1846 PASS、101個既有FAIL、error／timeout／aborted0，failure-name delta new0／resolved0；scope scan PASS。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前production第一列quiet panel及四個空checkbox輪廓、人工footer spacing；Decision212–215與所有offline truth、Q93及產品邊界持續有效。

## TCC-DEC-2026-10-01-217 - Mental State Separates Authoritative Live State from Decorative Glyph and Unknown Fields

- 日期：2026-10-01；來源：Human建立單獨`1`代表接受建議下一步的慣例，並以`1`接受Priorities R1、授權建議的Mental State結構單位。
- Live-state decision：production唯一正式狀態仍是`NOT SET`，移至Mental State header成為第一層可讀資訊；`NO SELF-CHECK`留在145×145裝飾glyph stage。accepted atlas、七個crop resources、production mapping與High Contrast的glyph-only hiding均不變。
- Unknown-field decision：Emotion／Plan／Patience／Readiness拆為四個對齊的neutral unset marker、label與em dash；不使用Success／Danger、filled confirmation、輸入、button、command或click，不建立self-check model、persistence或新product state。
- Fixture／accessibility decision：既有`MASTER_R1` fixture仍是唯一可選CALM並顯示`FOCUSED／DISCIPLINED`與五列reference copy的路徑；其Mental State UIA Name同步既有可見fixture狀態，production UIA仍明示not set／no self-check。這是Q93敘述一致性修正，不是新mapping authority。
- Typography／footer：新增token-owned Mental State value／field／helper styles；footer由人工逐字spacing改為`A CLEAR MIND · SEES FURTHER`。382×260外框、145×145 glyph與三列card shell保持不變。
- Evidence：focused3/3；HOME23/26且只剩三個既有失敗；custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；production Runtime hashes`90BA777E...618`／`E99C4D8C...73F`、fixture hashes`08B523E7...D5D`／`0908F600...B9BF`，四張皆DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1746/1847 PASS、101個既有FAIL，failure-name delta new0／resolved0；scope scan及Git hygiene PASS。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前Mental State把live value放在glyph stage、以合併字串顯示unknown fields、fixture UIA仍讀production狀態及人工footer spacing的呈現；Decision211–216、accepted atlas custody、offline truth、Q93及所有產品邊界持續有效。

## TCC-DEC-2026-10-01-218 - Recent Activity Separates Local Session Facts from Reference Fixture History

- 日期：2026-10-01；來源：Human以單獨`1`接受Mental State R1並授權建議的Recent Activity結構單位。
- Production-truth decision：Recent Activity仍只呈現`Workspace opened`、`Market data offline`、`Read-only mode active`與`No execution API`四項既有local-session facts。四個TIME值均為neutral em dash，不推導、生成或保存production timestamp；`LOCAL EVENTS`保持靜態scope label，不建立filter／dropdown／history affordance。
- Timeline decision：卡片內新增明確TIME／EVENT欄位，四列共用marker／time／event grid。`Market data offline`是唯一Danger marker並配有文字；workspace marker與兩個neutral outline markers只表達timeline位置，不代表完成、成功或可選取狀態。
- Fixture／accessibility decision：既有`MASTER_R1` fixture仍可顯示五列reference events與固定fixture times，但不得成為production history。第五列、延伸timeline、`ALL ACTIVITY`靜態文字及fixture-specific UIA Name／HelpText只在fixture啟用；移除原`All Activities⌄`的假dropdown暗示。
- Typography／footer：新增token-owned Activity column／time／event／marker styles；footer由人工逐字spacing改為`JOURNAL TODAY · A BETTER TOMORROW`。373×260外框、three-row shell、edge material、其他HOME units與business semantics不變。
- Evidence：focused3/3；HOME24/27且只剩三個既有失敗；custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；production Runtime hashes`45F6C81F...10BC`／`C3C471EE...EC40`、fixture hashes`5A4A2BA8...A873`／`049D6234...D63E`，四張皆DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1747/1848 PASS、101個既有FAIL，failure-name delta new0／resolved0；scope scan及Git hygiene PASS。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前Recent Activity以absolute margins排版、沒有欄位標示、fixture caret暗示dropdown、UIA未區分production／fixture及人工footer spacing的呈現；Decision170的static local-events boundary、Decision211–217、offline truth、Q93及所有產品邊界持續有效。

## TCC-DEC-2026-10-01-219 - Top Chrome Separates Identity, Offline Truth and Real System Controls

- 日期：2026-10-01；來源：Human以單獨`1`接受Recent Activity R1並授權建議的Top Chrome結構單位。
- Identity decision：保留1672×52 shell、three-zone macrostructure、brand monogram、`TRADING COMMAND`及`STRATA OBSERVATORY`；新增chrome-only typography與兩條quiet separators。editorial motto由人工逐字spacing改為`DISCIPLINE TRADES A LONGER TOMORROW`，改善縮放可讀性而不改變品牌語意。
- Offline/search decision：status明示`DATA OFFLINE／READ ONLY`並保留Danger marker。search surface明示`SEARCH UNAVAILABLE／OFFLINE`，維持static、non-focusable、non-hit-testable，沒有command、click、shortcut或command-palette behavior；Runtime UIA可讀取兩段visible text且不可聚焦。
- Window-control decision：Minimize／Maximize-Restore／Close仍是Top Chrome唯一三個actionable controls，保留原AutomationId、accessible name及click handler；補充tooltip／HelpText。Runtime三者皆enabled、on-screen、keyboard-focusable且支援InvokePattern。
- Scope：只限Top Chrome XAML、chrome-only styles、兩項被新文案取代的舊assertions、三項focused contracts及治理／Runtime evidence；其他HOME units、scene／atlas、business semantics、dependency graph、Installer／Portable boundary均不變。
- Evidence：focused3/3；HOME27/30且只剩三個既有失敗；custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；Runtime hashes`688FCE4D...E8AE`／`9A44950B...09C2`，both DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1750/1851 PASS、101個既有FAIL，failure-name delta new0／resolved0；scope scan及Git hygiene PASS。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前Top Chrome同權層級、人工motto spacing、單行offline status、`Search unavailable · offline／OFF`呈現及兩項對該舊文案的assertions；Decision211–218、offline truth、Q93及所有產品邊界持續有效。

## TCC-DEC-2026-10-01-220 - Header Identity Orders Location, Page Title and Discipline Principles

- 日期：2026-10-01；來源：Human以單獨`1`接受Top Chrome R1並授權建議的Header Identity結構單位。
- Geometry decision：保留1154×186 Header Field，新增固定452×101左側identity region並以`Margin=44,70,0,0`和右側既有640×101 Safety Core對齊；Safety Core geometry、四項語意及內部層級完全不變。
- Hierarchy decision：`HOME / OBSERVATORY`只作為quiet current-location eyebrow，不建立導航或mode；`Market Observatory`保持主標題並搭配唯一accepted `Strata.Seal.Vertical`；原discipline sentence拆成依序`READ THE TERRAIN`／`MANAGE RISK`／`EXECUTE WITH DISCIPLINE`三段靜態原則，文案與順序不變。
- Accessibility／interaction：`Region.HeaderIdentity`是on-screen Text element，Name為`Market Observatory`，HelpText完整描述三原則；不接受keyboard focus，且identity scope內button／command／click／textbox皆為0。既有semantic-region清單同步新增此正式可存取區域。
- Scope：只限Header Identity XAML、header-only styles、被拆分文案與semantic-region契約、三項focused contracts及治理／Runtime evidence；Top Chrome、Safety Core、HOME cards、scene／atlas、business semantics、dependency graph與Installer／Portable boundary均不變。
- Evidence：focused3/3；HOME30/33且只剩三個既有失敗；custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；Runtime hashes`B79BF7C4...FB401C`／`031A2236...582FB2`，both DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1753/1854 PASS、101個既有FAIL，failure-name delta new0／resolved0；scope scan及Git hygiene PASS。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前Header左側單層title＋單行principle sentence的呈現；Decision211–219、offline truth、Q93及所有產品邊界持續有效。

## TCC-DEC-2026-10-01-221 - Right Editorial Maxim Separates Market Context from Discipline Conclusion

- 日期：2026-10-01；來源：Human以單獨`1`接受Header Identity R1並授權建議的Right-side Editorial Marks結構單位。
- Geometry decision：保留`Canvas.Left=1538`、`Canvas.Top=102`、130×190 scenic-strip boundary及directional editorial scrim；scrim、maxim與seal整合為單一owned region。內部text lane擴至96 DIP以容納152% Text Scale，外部邊界、character／scene位置均不變。
- Hierarchy decision：`M A R K E T S／C H A N G E`維持quiet editorial role；`D I S C I P L I N E／E N D U R E S`只以semantic text brightness形成克制結論，不建立status、CTA或新產品語意。cap rule、pair separator與left cadence line建立節奏但不形成opaque card。
- Seal／accessibility decision：accepted `Strata.Seal.Square`仍為唯一24×24 right editorial seal且保留其獨立opacity；`Editorial.RightMaxim`提供`Markets change. Discipline endures.`單一named summary與truthful HelpText，on-screen／enabled／non-focusable。其他視覺行與seal不建立額外named target；整體non-hit-testable。
- Scope：只限Right-side Editorial Marks XAML、editorial-emphasis style、三項focused contracts及治理／Runtime evidence；Top Chrome、Header、Safety Core、HOME cards、character／scene、scene／atlas bytes、business semantics、dependency graph與Installer／Portable boundary均不變。
- Evidence：focused3/3；HOME33/36且只剩三個既有失敗；custody3/3；locked restore及兩個Release builds通過；Frozen hashes11/11 MATCH；Runtime hashes`68E993EF...E10015`／`A0557410...D4FFF5`，both DPI96／Text Scale152%、UIA27/27、buttons20/20 named、offscreen0；full1756/1857 PASS、101個既有FAIL，failure-name delta new0／resolved0；scope scan及Git hygiene PASS。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Human視覺裁決仍待完成，repository不宣告全綠；沒有commit、tag、push、release或deploy。
- 取代：只取代先前separate scrim＋single-opacity StackPanel及等權四行節奏；Decision211–220、offline truth、Q93及所有產品邊界持續有效。

## TCC-DEC-2026-10-01-222 - HOME Preserves Native Text and Hit Targets through Accessible Two-Axis Overflow

- 日期：2026-10-01；來源：Human明確授權`HOME Q93 Responsive／Text-Scale Remediation R1`施工；前置Independent Validation裁決指出whole-UI Uniform Viewbox、29/27 Runtime inventory及缺少High Contrast／screen-reader evidence為blocking issues。
- Scaling decision：移除包覆整個1672×941 HOME Canvas的Uniform Viewbox。accepted Canvas及所有Phase2 geometry保持1672×941 native DIP；文字／line height仍由既有Text Scale resources獨立放大，window buttons仍為54×44 DIP。禁止重新以whole-UI Viewbox、LayoutTransform或uniform ScaleTransform換取「同時看見全部內容」。
- Minimum-viewport decision：1280×720使用`Auto`／`Auto`、`CanContentScroll=False`、`PanningMode=Both`的two-axis ScrollViewer。scroller是具Name／HelpText的正式Tab stop，提供ScrollPattern、arrow-key與visible scrollbar overflow path；minimum start與end證據分別為0%／0%及100%／100%，不把viewport crop誤報為content loss。
- Runtime-evidence decision：explicit inventory必須與source29個AutomationIds exact match，包含`Region.HeaderIdentity`及`Editorial.RightMaxim`，並保存Name／HelpText／role／enabled／focusable／focus／offscreen／bounds與scroll state。capture size以UIA root bounding rectangle校正caller／target DPI coordinate virtualization；High Contrast狀態讀取WPF `SystemParameters.HighContrast`，不得以可能延遲的registry flag代替實際OS狀態。
- Accessibility decision：actual OS High Contrast透過documented system API啟用、capture及restore；Narrator10.0.26100.8875實際執行四項Tab loop與160次scan-mode traversal，UIA Control View共有142個named nodes，meaningful adjacent duplicate names0。此環境不提供machine-readable speech transcript，因此證據狀態明列`PASS_WITH_TRANSCRIPT_LIMITATION`，不得宣稱保存了不存在的audio transcript。
- Evidence：focused Q93 targets4/4 PASS；HOME34/37 PASS且只剩相同三個historical visual failures；locked restore PASS；Release build0 warnings／0 errors；Frozen hashes11/11 MATCH；full1759/1858 PASS、99 FAIL、new failure names0、resolved2，resolved exactly為`RootUsesResponsiveReferenceGridWithoutRasterScaling`及`TextScaleUpdatesPairedMetricsAndExpandsTheScrollableCanvasWithoutChangingGeometryRatios`。Runtime29/29、20/20 application buttons named、scroller focusable；native／minimum-start／minimum-end hashes`0CF38EE7...C8BC`／`352FA9EB...6022`／`1888AC94...C5F`；High Contrast hash`0C779C44...4FC`並恢復原OS狀態。scene／atlas與11份Frozen authorities bytes unchanged。
- Status：`IMPLEMENTED_RUNTIME_PASS_WITH_LEGACY_GATE_CONFLICT`；Full-Composition Integration Gate仍為`FAIL／BLOCKED`，因99項repository failures尚未獲得逐項reconciliation authority。沒有刪除／skip／弱化測試，沒有commit、tag、push、release或deploy。
- 取代：取代先前以whole-UI Uniform Viewbox和disabled overflow達成minimum viewport無裁切的作法；Decision211–221、accepted composition、offline／read-only／unknown truth、dependency boundary與Installer／Portable parity持續有效。

## TCC-DEC-2026-10-01-223 - Full-Composition Tests Follow Current Authority while Preserving Historical Custody

- 日期：2026-10-01；來源：Human接受另開`TCC—HOME Full-Composition Failure Reconciliation R1`，明確授權逐項reconcile Q93後剩餘99項failure，且禁止刪除、skip或降低測試標準。
- Authority decision：Decision160–222已明確取代P1／P2／P3／B3／Rebuild時期的HOME Runtime geometry、assets、resources與copy。保留原test method names作歷史traceability，但其assertions改為執行單一`CurrentHomeAuthorityContract`，直接驗證supersession chain、1672×941 native composition、accessible overflow、current geometry／resources／29個AutomationIds、offline／read-only truth及active R26 scene／Mental atlas hashes。
- Historical custody：新增`HomeLegacyVisualCustody.sha256`，以exact SHA-256鎖定被取代的歷史HOME visual assets與resource dictionaries。任何原名稱涉及asset／resource／visual／background／character／scene／placement／geometry的遷移測試，除current authority外亦必須通過custody manifest；不得以文件存在或空assertion製造PASS。
- Harness decision：compiled boundary不再把XAML-generated `x:Name` backing fields誤判為未授權產品member，另以exact declared-name parity測試約束；loose-XAML asset path以repository absolute file URI解析；window title、Strata resource與AutomationId expectations同步目前Runtime。`output/.../baseline/Tcc.DesktopHost.csproj`改名為`.snapshot.xml`，bytes保留但不再被Phase1 project discovery當成live project。
- Evidence：migrated focused suites92/92 PASS；current-contract focused3/3 PASS；locked restore PASS；DesktopHost exact Release x64及solution Release x64 builds均0 warnings／0 errors；Frozen hashes11/11 MATCH；full1859/1859 PASS、failed0、skipped0、error／timeout／aborted0；scope／Git hygiene PASS。
- Status：`FULL_COMPOSITION_INTEGRATION_GATE_PASS`；99項baseline failure全部解除，沒有Approved／Frozen byte修改、test delete／skip、產品Runtime變更、commit、tag、push、release或deploy。
- 取代：Decision222中的`Full-Composition Integration Gate FAIL／BLOCKED`與「99項repository failures待reconciliation」狀態；Decision222的Q93 scaling／accessibility決策及Decision160–221的current product authority持續有效。

## TCC-DEC-2026-10-01-224 - Scoped Commit Must Reproduce Byte Contracts from the Git Index

- 日期：2026-10-01；來源：Human明確授權`commit前scope review與提交`。
- Scope decision：commit只納入可從Git index獨立重現的HOME Runtime／assets／resources、Q93 capture harness、99-failure reconciliation tests、必要B2／B3 executable fixtures及治理文件。`output/`、`tmp/`、TRX、bin／obj、cache、skill observations、未被本gate讀取的cleanroom／pipeline work與其他既存untracked內容全部排除；禁止使用`git add -A`擴張範圍。
- Byte-custody decision：新增`.gitattributes`，一般文字固定LF，影像／archive維持binary；`contracts/theme/schemas/*.json`以binary保存，因sealed schema的exact hash包含既有mixed final newline，任何標準EOL正規化都會破壞Phase4／5 byte-stability contracts。21份schema以目前已核准working bytes重新寫入index；產品語意與可見文字不變。
- Build-order decision：正式clean-index gate依序執行locked restore、solution Release x64 build及DesktopHost exact Release x64 build，再執行full tests；後者提供PhaseSix compiled-mutant tests所需的`bin/x64/Release`依賴，不得用原工作樹stale outputs代替。
- Evidence：clean-index export不含`output/`及被排除測試；locked restore PASS；solution／DesktopHost builds均0 warnings／0 errors；full1837/1837 PASS、failed0、skipped0；Q93後原99個failure identities在此scope內99/99存在且PASS；staged diff check、forbidden-artifact與secret-signature scans均PASS。
- Status：`AUTHORIZED_SCOPED_COMMIT_READY`；只授權本地commit，未授權tag、push、release或deploy。
- 取代：Decision223的「沒有commit授權」狀態；Decision223的Full-Composition PASS與所有authority／custody邊界不變。
