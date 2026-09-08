# ADR-0003: Theme Integrity Contract Amendment

**Status:** Accepted  
**Decision date:** 2026-09-07  
**Decision authority:** Explicit human approval of the Phase 4 Contract Gap Resolution Proposal  
**Decision type:** Additive, versioned contract amendment; does not replace Frozen Architecture or sealed V1 contracts

## Context

The sealed Phase 2 integrity surface establishes `IThemeIntegrityVerifier`, `ThemeIntegrityVerificationRequest`,
`ThemeIntegrityVerificationResult`, `ThemeIntegrityManifest`, `ThemeIntegrityFile`, and
`ThemeIntegrity.schema.json`. It does not fully determine the package trust boundary needed before the Phase 4
Theme Integrity Verifier can be implemented. The unresolved areas were signature authority, distribution-channel
policy, canonical package hashing, exhaustive inventory semantics, canonical path handling, structured evidence,
and stable integrity diagnostics.

Phase 2 and Phase 3 are sealed baselines. Rewriting their artifacts would invalidate their reviewed serialization,
schema, implementation, and tag identities. The gap is therefore resolved through an additive contract family.

## Decision

Add versioned V2 integrity contracts and V1 supporting evidence, signature, policy, and trust contracts in
`Tcc.Presentation.Contracts`, plus versioned JSON schemas. Existing V1 types, wire semantics, and schema remain
unchanged. The amendment authorizes contracts and contract-level tests only; it does not authorize a production
`ThemeIntegrityVerifier` implementation.

### Package integrity boundary

`PackageRef` remains an opaque logical identity. `IThemePackageContentReader` is read-only and is limited to
enumerating logical entries, reporting entry length/type, and opening non-writable content streams. The contract
does not expose an arbitrary filesystem trust path and does not permit extraction, installation, movement,
deletion, promotion, activation, persistence, or caching.

`ThemePackageRef` identifies verification input source/context only. It is transport and audit provenance, not
publisher-signed content identity, so changing a package's source handle must not invalidate an otherwise identical
publisher signature. It is therefore intentionally excluded from the canonical signature payload and may be
retained separately by orchestration/audit records.

Symbolic links, reparse points, directories where a file is required, and unsupported entry kinds fail closed at
the future verifier boundary. Archive extraction and containment are outside this amendment.

### TCC Package Canonical Path v1

Canonical paths:

- use `/` as the only separator and remain relative;
- are normalized to Unicode NFC for validation and comparison while retaining display case;
- reject empty segments, `.`, `..`, absolute paths, drive prefixes, UNC paths, backslashes, colons, NUL/control
  characters, trailing dots, trailing spaces, and Windows reserved device names;
- reject both normalized duplicates and `OrdinalIgnoreCase` collisions;
- use `Ordinal` ordering and never culture-dependent comparison.

Reserved Theme Manifest, Integrity Manifest, and optional Signature Envelope paths use the typed
`ThemeCanonicalPath` contract. Schema patterns enforce representable lexical constraints; decoded/normalized path
semantics, cross-entry uniqueness, Unicode-normalized collisions, and Windows case collisions are semantic
contract invariants.

### Exhaustive inventory

V2 `files` is the exhaustive allowed payload inventory. A declared required file that is absent fails. A declared
optional file may be absent. Every present declared file is verified. Every present undeclared file fails.
Only the exact Integrity Manifest and Signature Envelope entries identified by the request are reserved metadata
excluded from inventory/package-hash comparison; broad glob exclusions are forbidden.

### Canonical Package Tree Hash v1

The package hash is independent of raw ZIP/archive bytes. For every included payload entry, create this UTF-8
record:

```text
canonical_path NUL decimal_length NUL lowercase_sha256_hex LF
```

Sort records by canonical path using `Ordinal`, concatenate them, and compute SHA-256. The Integrity Manifest and
Signature Envelope themselves are excluded exactly to avoid self-reference. No Merkle tree or content-addressable
store is introduced.

`ThemeManifestHash` and `IntegrityManifestHash` are separate V2 evidence fields and are SHA-256 over the actual raw
bytes of those package files. JSON must not be reserialized before hashing. The existing V1
`ThemeIntegrityVerificationResult.ManifestHash` is not reinterpreted.

### Signature and trust

`ThemeSignatureEnvelopeV1` uses ECDSA P-256 with SHA-256. Signatures use IEEE P1363 fixed-field concatenation.
Trusted public keys are SubjectPublicKeyInfo bytes. The signature canonical payload is UTF-8 without BOM:

```text
TCC-THEME-PACKAGE-SIGNATURE-V1
NUL
ThemeId
NUL
ThemeVersion
NUL
PackageHash
NUL
ThemeManifestHash
NUL
IntegrityManifestHash
NUL
PublisherId
NUL
KeyId
```

Hash values in this payload are lowercase hexadecimal.

The SPKI algorithm must be `id-ecPublicKey` (`1.2.840.10045.2.1`) with named-curve parameters
`1.2.840.10045.3.1.7` (NIST P-256 / secp256r1 / prime256v1). The imported BCL curve identity must also
be that exact OID. Key size, signature length, successful import, cryptographic verification, and localized
friendly names are not curve-identity proofs. Explicit parameters, unknown curves, secp256k1, brainpool,
and all other curves fail closed even when their signatures verify cryptographically.

Trust flows from an application-managed, versioned signer registry into an immutable caller-provided
`ThemeTrustSnapshotV1`, then into the verifier. The verifier must not download keys, perform cloud lookup, create a
certificate authority, decide trust membership, or mutate the registry. A signer is matched by publisher ID and key
ID and is explicitly `Trusted` or `Revoked`.

`ThemeIntegrityVerificationPolicyV1` explicitly carries the expected trust-policy identity and version. These must
equal `ThemeTrustSnapshotV1.PolicyId` and `PolicyVersion`. Exact `(PublisherId, KeyId)` identities in a trust snapshot
are unique; duplicates, including trusted/revoked ambiguity, fail closed with a stable diagnostic.

### Channel policy

The caller supplies `ThemeIntegrityVerificationPolicyV1`; the verifier does not infer a channel.

- Stable requires a signature and forbids a developer exception.
- Store distribution requires a signature.
- Developer may accept unsigned content only when an explicit authorized exception is present.
- Beta follows the policy's explicit signature requirement.

### Evidence and diagnostics

V2 results separate package, Theme Manifest, and Integrity Manifest hashes; provide file and applicable asset
evidence; report signature status, publisher/key identity, and structured diagnostics. Unavailable hashes are null,
never empty strings. A deterministic verifier decision contains no `verified_at`; orchestration may add timestamped
records outside the deterministic core where required by Frozen evidence rules.

Integrity diagnostics use the `P4Ixxx` family. One semantic rule has one stable code. Repeated entry violations may
reuse that rule's code. Diagnostics order by code, canonical path, and message with `Ordinal` comparison.
`ThemeIntegrityDiagnosticVocabulary` is the authoritative active rule/code registry; unknown diagnostic codes are
invalid at the contract boundary.

Contract-only security semantics are backed by a test-only Contract Conformance Oracle and executable reference
vectors. The oracle consumes the public contracts and actual schemas, is not shipped in any production assembly,
and does not implement or substitute for the production `ThemeIntegrityVerifier`.

### Authoritative raw-metadata conformance boundary

The test-only pipeline owns a snapshot of the package bytes and executes:
raw metadata → repository actual schema validation → `ThemeContractJson` deserialization → public DTO →
semantic validation → inventory/hash/identity/signature/trust/policy checks. It directly loads
`ThemeIntegrity.v2.schema.json`, `ThemeSignatureEnvelope.v1.schema.json`, and the sealed
`ThemeManifest.schema.json`. The materialized Theme Manifest also passes the existing sealed
`ThemeManifestValidator`; this does not modify that implementation or authorize downstream runtime work.

Where the request also carries metadata DTOs, deterministic semantic equality with the materialized raw DTOs
is mandatory. Property ordering and whitespace do not change equality; collection ordering and field values do.
Only DTOs materialized from validated raw bytes enter the hash/signature decision. A valid caller DTO cannot
replace an invalid, missing, or different raw document. Duplicate JSON properties fail closed.
Request, Integrity Manifest, and Theme Manifest Theme ID/version must agree and satisfy their identity grammar.

`ThemeManifestHash` and `IntegrityManifestHash` hash the exact owned raw bytes that passed this pipeline,
including their original whitespace. The canonical signature payload references those same hashes. No JSON
reserialization is used to produce the hash inputs. `ThemePackageRef` remains excluded from signed content.

Direct public DTO construction does not bypass semantic checks: undefined security enums, invalid identities,
hash values, algorithms, encodings, policy values, trust values, malformed Unicode, and null security collection
items fail closed. `ThemeCanonicalPath.TryCreate` returns false for malformed surrogate input. A present envelope
with a null/malformed signature is invalid, including on Developer channel; only actual absence can use an
authorized unsigned exception. Parsing and normalization failures produce deterministic diagnostics without
exception details. The sealed cancellation contracts are unchanged.

The minimal additional diagnostic rules are `P4I027` (metadata document nonconformance, including schema,
materialization, or sealed manifest semantics), `P4I028` (raw/DTO binding mismatch), and `P4I029` (invalid security
field value without a more specific existing rule). Wrong curve uses `P4I016`; malformed path uses `P4I002`;
null required objects/items use `P4I020`; malformed signature values use `P4I013`. All active codes require an
executable emitting vector, registry equality, unknown-code rejection, and deterministic output tests.

### TH-OTD-002 resolution

The previously open signature-authority decision is resolved for Phase 4 as application-managed signer registry →
immutable caller trust snapshot → verifier, with ECDSA P-256/SHA-256, IEEE P1363 signatures, and SPKI public keys.
This does not create an online authority, CA, store backend, or key-distribution service.

## Theme and Trading separation

This amendment is Theme package integrity only. It adds no Risk, Market Observation, AI Recommendation, Position
Intelligence, Shadow Trading, Learning, Prepared Order, Market Data, or Trade Execution contract or behavior. The
Theme Runtime Track and Trading Intelligence Track remain hard-separated.

## Phase 4 unlock gate

Production Phase 4 remains locked. It may begin only after this amendment passes focused and full repository gates,
an independent Contract Amendment Audit passes, the amendment is separately authorized for commit/sealing, and a
subsequent explicit instruction authorizes the production branch and `ThemeIntegrityVerifier` implementation.

## Consequences

- V1 remains compatible and its sealed tag identities do not move.
- Phase 4 receives deterministic package, path, inventory, signature, trust, policy, evidence, and diagnostic
  boundaries before runtime work starts.
- The .NET BCL is sufficient for the selected cryptography; no NuGet dependency is authorized or required.
- Future production work must fail closed and implement these contracts without expanding into package lifecycle,
  Theme activation, UI, trading, or Phase 5+ behavior.
