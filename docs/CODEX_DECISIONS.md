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
