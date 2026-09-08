using System.Globalization;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Architecture.Tests;

/// <summary>
/// Test-only executable reference semantics for the Phase 4 contract amendment.
/// This type is intentionally outside every production assembly and is never a runtime verifier.
/// </summary>
internal static class ThemeIntegrityContractConformanceOracle
{
    internal static ThemeIntegrityVerificationResultV2 Evaluate(
        ThemeIntegrityVerificationRequestV2 request,
        IReadOnlyList<ThemePackageContentEntryV1> entries,
        IReadOnlyDictionary<string, byte[]> contentByLogicalPath,
        Func<string, JsonElement, JsonElement>? schemaMutation = null)
    {
        List<ThemeIntegrityDiagnosticV1> diagnostics = [];
        if (request is null || entries is null || contentByLogicalPath is null
            || request.IntegrityManifest?.Files is null || request.Policy is null
            || request.TrustSnapshot is null || request.TrustSnapshot.TrustedSigners.IsDefault
            || request.IntegrityManifest.Files.Any(file => file is null)
            || request.TrustSnapshot.TrustedSigners.Any(signer => signer is null)
            || entries.Any(entry => entry is null))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.MissingSecurityField, null,
                "A required security object, collection, or collection item is absent.");
            return Failure(diagnostics);
        }

        // Own the bytes used by schema validation, materialization, hashing and signature checks.
        Dictionary<string, byte[]> contents = contentByLogicalPath.ToDictionary(
            pair => pair.Key, pair => pair.Value is null ? null! : (byte[])pair.Value.Clone(), StringComparer.Ordinal);
        ThemePackageContentEntryV1[] inventory = entries.ToArray();
        ValidateDirectInputs(request, inventory, diagnostics);
        ThemeIntegrityManifestV2? manifest = ReadMetadata<ThemeIntegrityManifestV2>(
            request.IntegrityManifestPath?.Value, "ThemeIntegrity.v2.schema.json",
            ThemeIntegrityDiagnosticCodes.IntegrityManifestHashUnavailable, contents, diagnostics, schemaMutation);
        ThemeManifest? theme = ReadMetadata<ThemeManifest>(
            request.ThemeManifestPath?.Value, "ThemeManifest.schema.json",
            ThemeIntegrityDiagnosticCodes.PackageReadFailure, contents, diagnostics, schemaMutation);
        ThemeSignatureEnvelopeV1? envelope = null;
        bool rawSignaturePresent = request.SignatureEnvelopePath is not null
            && contents.ContainsKey(request.SignatureEnvelopePath.Value);
        if (rawSignaturePresent || request.SignatureEnvelope is not null)
        {
            envelope = ReadMetadata<ThemeSignatureEnvelopeV1>(
                request.SignatureEnvelopePath?.Value, "ThemeSignatureEnvelope.v1.schema.json",
                ThemeIntegrityDiagnosticCodes.MissingSignature, contents, diagnostics, schemaMutation);
        }

        if (manifest is not null && !SemanticEquals(manifest, request.IntegrityManifest)
            || !SemanticEquals(envelope, request.SignatureEnvelope))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch, null,
                "Caller metadata DTO differs from the DTO materialized from validated raw bytes.");
        }

        if (theme is not null)
        {
            using JsonDocument themeSchema = JsonDocument.Parse(File.ReadAllBytes(
                Path.Combine(RepositoryPaths.ThemeContracts, "schemas", "ThemeManifest.schema.json")));
            HashSet<string> knownCapabilities = themeSchema.RootElement.GetProperty("properties")
                .GetProperty("capabilities").GetProperty("items").GetProperty("enum")
                .EnumerateArray().Select(value => value.GetString()!).ToHashSet(StringComparer.Ordinal);
            ThemeManifestValidationResult semantic = new Tcc.Themes.Manifests.ThemeManifestValidator().Validate(
                theme, new ThemeManifestValidationContext(new HashSet<string>([ContractVersions.Schema], StringComparer.Ordinal),
                    knownCapabilities, request.Policy.DistributionChannel is ThemeDistributionChannel.Stable or ThemeDistributionChannel.Store));
            if (!semantic.IsValid)
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, request.ThemeManifestPath?.Value,
                    "Raw Theme Manifest violates the sealed Theme Manifest semantic contract.");
            if (theme.Package.ThemeId != request.ThemeId.Value)
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch, null,
                    "Theme Manifest identity differs from the verification request.");
            if (theme.Package.Version != request.Version.Value)
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch, null,
                    "Theme Manifest version differs from the verification request.");
        }

        if (manifest is null || theme is null || diagnostics.Count != 0)
            return Failure(diagnostics);

        // Only the materialized raw metadata enters the semantic/hash/crypto decision.
        return EvaluateBound(request with { IntegrityManifest = manifest, SignatureEnvelope = envelope }, inventory, contents);
    }

    private static ThemeIntegrityVerificationResultV2 Failure(IEnumerable<ThemeIntegrityDiagnosticV1> diagnostics) =>
        new(false, null, null, null, [], [], ThemeSignatureVerificationStatus.NotEvaluated,
            null, null, OrderDiagnostics(diagnostics));

    private static bool SemanticEquals<T>(T? left, T? right)
    {
        try
        {
            JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
            return JsonNode.DeepEquals(JsonSerializer.SerializeToNode(left, options), JsonSerializer.SerializeToNode(right, options));
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

    private static T? ReadMetadata<T>(string? path, string schemaName, string unavailableCode,
        Dictionary<string, byte[]> contents, ICollection<ThemeIntegrityDiagnosticV1> diagnostics,
        Func<string, JsonElement, JsonElement>? schemaMutation) where T : class
    {
        if (path is null || !contents.TryGetValue(path, out byte[]? bytes) || bytes is null)
        {
            Add(diagnostics, unavailableCode, path, "Required raw metadata bytes are unavailable.");
            return null;
        }

        // Repository schemas are always loaded. The optional test hook mutates that actual
        // document in memory to prove a schema change reaches the executable decision.
        using JsonDocument schema = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(RepositoryPaths.ThemeContracts, "schemas", schemaName)));
        try
        {
            using JsonDocument document = JsonDocument.Parse(bytes);
            JsonElement actualSchema = schemaMutation?.Invoke(schemaName, schema.RootElement) ?? schema.RootElement;
            if (HasDuplicateProperties(document.RootElement)
                || JsonSchemaSubsetValidator.Validate(actualSchema, document.RootElement).Count != 0)
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, path,
                    "Raw metadata does not conform to its authoritative schema.");
                return null;
            }

            T? value = JsonSerializer.Deserialize<T>(bytes, ThemeContractJson.CreateSerializerOptions());
            if (value is not null) return value;
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException
            or FormatException or OverflowException)
        {
            // Only expected metadata parsing/conversion failures are contained here.
            // Cancellation and unrelated programming failures are not intercepted.
        }

        Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, path,
            "Raw metadata cannot be materialized by the authoritative serializer.");
        return null;
    }

    private static bool HasDuplicateProperties(JsonElement node) => node.ValueKind switch
    {
        JsonValueKind.Object => node.EnumerateObject().Select(property => property.Name).Distinct(StringComparer.Ordinal).Count()
            != node.EnumerateObject().Count() || node.EnumerateObject().Any(property => HasDuplicateProperties(property.Value)),
        JsonValueKind.Array => node.EnumerateArray().Any(HasDuplicateProperties),
        _ => false,
    };

    private static void ValidateDirectInputs(ThemeIntegrityVerificationRequestV2 request,
        IReadOnlyList<ThemePackageContentEntryV1> entries, ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        ValidateSecurityFields(request, diagnostics);
        ValidateContractConstants(request, diagnostics);
        ValidateIdentityBinding(request, diagnostics);
        ValidateTrustSnapshot(request, diagnostics);
        ValidatePathSet(request.IntegrityManifest.Files.Select(file => file.CanonicalPath), diagnostics);
        ValidatePathSet(entries.Select(entry => entry.LogicalPath), diagnostics);
        if (!ValidIdentity(request.ThemeId.Value) || !ValidIdentity(request.IntegrityManifest.ThemeId)
            || !ValidVersion(request.Version.Value) || !ValidVersion(request.IntegrityManifest.Version)
            || request.IntegrityManifest.Files.Any(file => !Enum.IsDefined(file.EntryKind)
                || file.LengthBytes < 0 || !ValidHash(file.Sha256))
            || !ValidHash(request.IntegrityManifest.PackageHash)
            || request.TrustSnapshot.TrustedSigners.Any(signer => !Enum.IsDefined(signer.TrustState)
                || !Enum.IsDefined(signer.Algorithm) || !Enum.IsDefined(signer.PublicKeyEncoding)
                || string.IsNullOrWhiteSpace(signer.PublisherId) || string.IsNullOrWhiteSpace(signer.KeyId)
                || string.IsNullOrWhiteSpace(signer.PublicKey)))
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSecurityValue, null,
                "A security field has an invalid value or undefined enum member.");

        if (entries.Any(entry => entry.EntryKind != ThemePackageContentEntryKind.File))
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedPackageEntry, null,
                "Only regular file entries are supported by the integrity contract.");
        if (request.SignatureEnvelope is { } envelope)
        {
            if (!Enum.IsDefined(envelope.Algorithm))
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm, null, "Signature algorithm is unsupported.");
            if (!Enum.IsDefined(envelope.SignatureEncoding))
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedSignatureEncoding, null, "Signature encoding is unsupported.");
            if (!Enum.IsDefined(envelope.SignedPayloadType))
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSecurityValue, null, "Signed payload type is unsupported.");
            if (!IsCanonicalSignature(envelope.Signature))
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                    "Signature must be canonical Base64 encoding of exactly 64 P1363 bytes.");
        }
    }

    private static bool ValidIdentity(string? value) => value is not null
        && Regex.IsMatch(value, "\\A[a-z0-9]+(?:[.-][a-z0-9]+)+\\z", RegexOptions.CultureInvariant);
    private static bool ValidVersion(string? value) => value is not null
        && Regex.IsMatch(value, "\\A(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\z", RegexOptions.CultureInvariant);
    private static bool ValidHash(string? value) => value is not null
        && Regex.IsMatch(value, "\\A[a-f0-9]{64}\\z", RegexOptions.CultureInvariant);
    private static bool IsCanonicalSignature(string? value)
    {
        if (value is null || value.Length != 88) return false;
        Span<byte> decoded = stackalloc byte[64];
        return Convert.TryFromBase64String(value, decoded, out int count) && count == 64
            && Convert.ToBase64String(decoded) == value;
    }

    private static ThemeIntegrityVerificationResultV2 EvaluateBound(
        ThemeIntegrityVerificationRequestV2 request,
        IReadOnlyList<ThemePackageContentEntryV1> entries,
        Dictionary<string, byte[]> contentByLogicalPath)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(contentByLogicalPath);

        List<ThemeIntegrityDiagnosticV1> diagnostics = [];
        List<ThemeIntegrityFileEvidenceV1> evidence = [];

        if (request.IntegrityManifest is null
            || request.Policy is null
            || request.TrustSnapshot is null
            || request.IntegrityManifest.Files is null
            || request.TrustSnapshot.TrustedSigners.IsDefault)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.MissingSecurityField, null,
                "A required security contract object or collection is absent.");
            return new ThemeIntegrityVerificationResultV2(
                false,
                null,
                null,
                null,
                [],
                [],
                ThemeSignatureVerificationStatus.NotEvaluated,
                request.SignatureEnvelope?.PublisherId,
                request.SignatureEnvelope?.KeyId,
                OrderDiagnostics(diagnostics));
        }

        ValidateSecurityFields(request, diagnostics);
        ValidateContractConstants(request, diagnostics);
        ValidateIdentityBinding(request, diagnostics);
        ValidateTrustSnapshot(request, diagnostics);

        string? themeManifestPath = request.ThemeManifestPath?.Value;
        string? integrityManifestPath = request.IntegrityManifestPath?.Value;
        string? signatureEnvelopePath = request.SignatureEnvelopePath?.Value;

        List<CanonicalEntry> canonicalEntries = CanonicalizeEntries(entries, diagnostics);
        ValidatePathSet(
            request.IntegrityManifest.Files.Select(file => file.CanonicalPath),
            diagnostics);
        ValidatePathSet(canonicalEntries.Select(entry => entry.CanonicalPath), diagnostics);

        Dictionary<string, List<CanonicalEntry>> entriesByPath = canonicalEntries
            .GroupBy(entry => entry.CanonicalPath, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        HashSet<string> declaredPaths = [];

        foreach (ThemeIntegrityFileV2 declared in request.IntegrityManifest.Files)
        {
            if (!ThemeCanonicalPath.TryCreate(declared.CanonicalPath, out ThemeCanonicalPath? declaredPath))
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath, declared.CanonicalPath,
                    "Declared inventory path is not canonical.");
                continue;
            }

            declaredPaths.Add(declaredPath!.Value);
            entriesByPath.TryGetValue(declaredPath.Value, out List<CanonicalEntry>? matches);
            CanonicalEntry? present = matches?.Count == 1 ? matches[0] : null;
            if (present is null)
            {
                ThemeIntegrityEvidenceStatus status = declared.Required
                    ? ThemeIntegrityEvidenceStatus.MissingRequired
                    : ThemeIntegrityEvidenceStatus.MissingAllowed;
                evidence.Add(new ThemeIntegrityFileEvidenceV1(
                    declaredPath.Value,
                    declared.EntryKind,
                    declared.Required,
                    false,
                    declared.LengthBytes,
                    null,
                    declared.Sha256,
                    null,
                    status));
                if (declared.Required)
                {
                    Add(diagnostics, ThemeIntegrityDiagnosticCodes.MissingRequiredFile, declaredPath.Value,
                        "Required inventory entry is absent.");
                }

                continue;
            }

            if (!contentByLogicalPath.TryGetValue(present.Source.LogicalPath, out byte[]? bytes) || bytes is null)
            {
                AddReadFailure(diagnostics, declaredPath.Value);
                evidence.Add(new ThemeIntegrityFileEvidenceV1(
                    declaredPath.Value,
                    declared.EntryKind,
                    declared.Required,
                    true,
                    declared.LengthBytes,
                    present.Source.LengthBytes,
                    declared.Sha256,
                    null,
                    ThemeIntegrityEvidenceStatus.Unavailable));
                continue;
            }

            string actualHash = Sha256Hex(bytes);
            ThemeIntegrityEvidenceStatus evidenceStatus = ThemeIntegrityEvidenceStatus.Verified;
            if (declared.LengthBytes != bytes.LongLength || present.Source.LengthBytes != bytes.LongLength)
            {
                evidenceStatus = ThemeIntegrityEvidenceStatus.LengthMismatch;
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.FileLengthMismatch, declaredPath.Value,
                    "Declared or enumerated file length does not match content length.");
            }
            else if (!string.Equals(declared.Sha256, actualHash, StringComparison.Ordinal))
            {
                evidenceStatus = ThemeIntegrityEvidenceStatus.HashMismatch;
                string code = declared.EntryKind == ThemeIntegrityEntryKind.ThemeManifest
                    ? ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch
                    : ThemeIntegrityDiagnosticCodes.FileHashMismatch;
                Add(diagnostics, code, declaredPath.Value, "Declared file hash does not match raw content bytes.");
            }

            evidence.Add(new ThemeIntegrityFileEvidenceV1(
                declaredPath.Value,
                declared.EntryKind,
                declared.Required,
                true,
                declared.LengthBytes,
                bytes.LongLength,
                declared.Sha256,
                actualHash,
                evidenceStatus));
        }

        foreach (CanonicalEntry entry in canonicalEntries)
        {
            if (entry.Source.EntryKind != ThemePackageContentEntryKind.File)
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedPackageEntry, entry.CanonicalPath,
                    "Only regular file entries are supported by the integrity contract.");
            }

            bool isReservedMetadata = string.Equals(entry.CanonicalPath, integrityManifestPath, StringComparison.Ordinal)
                                      || (signatureEnvelopePath is not null
                                          && string.Equals(entry.CanonicalPath, signatureEnvelopePath, StringComparison.Ordinal));
            if (!declaredPaths.Contains(entry.CanonicalPath) && !isReservedMetadata)
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UndeclaredFile, entry.CanonicalPath,
                    "Present package entry is not declared by the exhaustive inventory.");
            }
        }

        string? packageHash = TryComputePackageHash(
            canonicalEntries,
            integrityManifestPath,
            signatureEnvelopePath,
            contentByLogicalPath,
            diagnostics);
        if (packageHash is not null
            && !string.Equals(packageHash, request.IntegrityManifest.PackageHash, StringComparison.Ordinal))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.PackageHashMismatch, null,
                "Canonical package tree hash does not match the integrity manifest.");
        }

        string? themeManifestHash = TryHashRequiredContent(
            themeManifestPath,
            ThemeIntegrityDiagnosticCodes.PackageReadFailure,
            "Theme Manifest raw bytes are unavailable.",
            contentByLogicalPath,
            diagnostics);
        string? integrityManifestHash = TryHashRequiredContent(
            integrityManifestPath,
            ThemeIntegrityDiagnosticCodes.IntegrityManifestHashUnavailable,
            "Integrity Manifest raw bytes are unavailable.",
            contentByLogicalPath,
            diagnostics);

        ThemeSignatureVerificationStatus signatureStatus = ValidateSignature(
            request,
            packageHash,
            themeManifestHash,
            integrityManifestHash,
            diagnostics);

        ThemeIntegrityDiagnosticV1[] orderedDiagnostics = OrderDiagnostics(diagnostics).ToArray();
        ThemeIntegrityFileEvidenceV1[] orderedEvidence = evidence
            .OrderBy(item => item.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        return new ThemeIntegrityVerificationResultV2(
            orderedDiagnostics.Length == 0,
            packageHash,
            themeManifestHash,
            integrityManifestHash,
            orderedEvidence,
            orderedEvidence.Where(item => item.CanonicalPath.StartsWith("assets/", StringComparison.Ordinal)).ToArray(),
            signatureStatus,
            request.SignatureEnvelope?.PublisherId,
            request.SignatureEnvelope?.KeyId,
            orderedDiagnostics);
    }

    internal static IReadOnlyList<ThemeIntegrityDiagnosticV1> OrderDiagnostics(
        IEnumerable<ThemeIntegrityDiagnosticV1> diagnostics) =>
        diagnostics
            .Select(item => item ?? new ThemeIntegrityDiagnosticV1(
                ThemeIntegrityDiagnosticCodes.MissingSecurityField, ThemeDiagnosticsSeverity.Error, null,
                "A required diagnostic collection item is absent."))
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.CanonicalPath, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToArray();

    internal static byte[] BuildSignaturePayload(
        string themeId,
        string themeVersion,
        string packageHash,
        string themeManifestHash,
        string integrityManifestHash,
        string publisherId,
        string keyId)
    {
        string[] fields =
        [
            ThemeIntegrityContractSemantics.SignaturePayloadDomain,
            themeId,
            themeVersion,
            packageHash,
            themeManifestHash,
            integrityManifestHash,
            publisherId,
            keyId,
        ];
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(string.Join('\0', fields));
    }

    internal static string ComputePackageHash(
        IEnumerable<(string CanonicalPath, byte[] Bytes)> entries)
    {
        StringBuilder records = new();
        foreach ((string path, byte[] bytes) in entries.OrderBy(item => item.CanonicalPath, StringComparer.Ordinal))
        {
            records.Append(path);
            records.Append('\0');
            records.Append(bytes.LongLength.ToString(CultureInfo.InvariantCulture));
            records.Append('\0');
            records.Append(Sha256Hex(bytes));
            records.Append('\n');
        }

        return Sha256Hex(Encoding.UTF8.GetBytes(records.ToString()));
    }

    internal static string Sha256Hex(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void ValidateSecurityFields(
        ThemeIntegrityVerificationRequestV2 request,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        string?[] requiredValues =
        [
            request.ThemeId.Value,
            request.Version.Value,
            request.PackageRef.Value,
            request.ThemeManifestPath?.Value,
            request.IntegrityManifestPath?.Value,
            request.IntegrityManifest.SchemaVersion,
            request.IntegrityManifest.ThemeId,
            request.IntegrityManifest.Version,
            request.IntegrityManifest.HashAlgorithm,
            request.IntegrityManifest.PackageHash,
            request.Policy.PolicyId,
            request.Policy.PolicyVersion,
            request.Policy.ExpectedTrustPolicyId,
            request.Policy.ExpectedTrustPolicyVersion,
            request.TrustSnapshot.PolicyId,
            request.TrustSnapshot.PolicyVersion,
        ];
        if (requiredValues.Any(string.IsNullOrWhiteSpace)
            || (request.SignatureEnvelope is not null && request.SignatureEnvelopePath is null))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.MissingSecurityField, null,
                "A required security contract field is absent or empty.");
        }
    }

    private static void ValidateContractConstants(
        ThemeIntegrityVerificationRequestV2 request,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        if (!string.Equals(request.IntegrityManifest.SchemaVersion, ContractVersions.ThemeIntegritySchemaV2, StringComparison.Ordinal))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedSchemaVersion, null,
                "Integrity Manifest schema version is unsupported.");
        }

        if (!Enum.IsDefined(request.IntegrityManifest.InventoryMode)
            || request.IntegrityManifest.InventoryMode != ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload
            || !string.Equals(request.IntegrityManifest.HashAlgorithm, ThemeIntegrityContractSemantics.HashAlgorithm, StringComparison.Ordinal))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSecurityValue, null,
                "Integrity Manifest security constants are invalid.");
        }
    }

    private static void ValidateIdentityBinding(
        ThemeIntegrityVerificationRequestV2 request,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        if (!string.Equals(request.ThemeId.Value, request.IntegrityManifest.ThemeId, StringComparison.Ordinal))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch, null,
                "Verification request theme_id does not match the Integrity Manifest theme_id.");
        }

        if (!string.Equals(request.Version.Value, request.IntegrityManifest.Version, StringComparison.Ordinal))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch, null,
                "Verification request version does not match the Integrity Manifest version.");
        }
    }

    private static void ValidateTrustSnapshot(
        ThemeIntegrityVerificationRequestV2 request,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        if (!string.Equals(request.Policy.ExpectedTrustPolicyId, request.TrustSnapshot.PolicyId, StringComparison.Ordinal)
            || !string.Equals(request.Policy.ExpectedTrustPolicyVersion, request.TrustSnapshot.PolicyVersion, StringComparison.Ordinal))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.TrustPolicyMismatch, null,
                "Verification policy expected trust-policy identity/version does not match the trust snapshot.");
        }

        foreach (IGrouping<(string PublisherId, string KeyId), ThemeTrustedSignerV1> duplicate in request.TrustSnapshot.TrustedSigners
                     .GroupBy(signer => (signer.PublisherId, signer.KeyId))
                     .Where(group => group.Count() > 1))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.DuplicateTrustedSignerIdentity, null,
                $"Duplicate trusted signer identity '{duplicate.Key.PublisherId}/{duplicate.Key.KeyId}'.");
        }
    }

    private static List<CanonicalEntry> CanonicalizeEntries(
        IReadOnlyList<ThemePackageContentEntryV1> entries,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        List<CanonicalEntry> canonical = [];
        foreach (ThemePackageContentEntryV1 entry in entries)
        {
            if (!ThemeCanonicalPath.TryCreate(entry.LogicalPath, out ThemeCanonicalPath? path))
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath, entry.LogicalPath,
                    "Enumerated package path is not canonical.");
                continue;
            }

            canonical.Add(new CanonicalEntry(entry, path!.Value));
        }

        return canonical;
    }

    private static void ValidatePathSet(
        IEnumerable<string> paths,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        List<(string Source, string Canonical)> canonical = [];
        foreach (string path in paths)
        {
            if (!ThemeCanonicalPath.TryCreate(path, out ThemeCanonicalPath? canonicalPath))
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath, path,
                    "Path is not a valid TCC Package Canonical Path v1 value.");
                continue;
            }

            canonical.Add((path, canonicalPath!.Value));
        }

        foreach (IGrouping<string, (string Source, string Canonical)> duplicate in canonical
                     .GroupBy(item => item.Canonical, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.DuplicateNormalizedPath, duplicate.Key,
                "Multiple paths normalize to the same canonical path.");
        }

        foreach (IGrouping<string, (string Source, string Canonical)> collision in canonical
                     .GroupBy(item => item.Canonical, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Select(item => item.Canonical).Distinct(StringComparer.Ordinal).Count() > 1))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.CaseInsensitivePathCollision, collision.Key,
                "Canonical paths collide under Windows ordinal-ignore-case comparison.");
        }
    }

    private static string? TryComputePackageHash(
        IEnumerable<CanonicalEntry> entries,
        string? integrityManifestPath,
        string? signatureEnvelopePath,
        Dictionary<string, byte[]> contentByLogicalPath,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        List<(string CanonicalPath, byte[] Bytes)> included = [];
        foreach (CanonicalEntry entry in entries
                     .Where(item => !string.Equals(item.CanonicalPath, integrityManifestPath, StringComparison.Ordinal)
                                    && !string.Equals(item.CanonicalPath, signatureEnvelopePath, StringComparison.Ordinal)))
        {
            if (!contentByLogicalPath.TryGetValue(entry.Source.LogicalPath, out byte[]? bytes) || bytes is null)
            {
                AddReadFailure(diagnostics, entry.CanonicalPath);
                return null;
            }

            included.Add((entry.CanonicalPath, bytes));
        }

        return ComputePackageHash(included);
    }

    private static string? TryHashRequiredContent(
        string? path,
        string diagnosticCode,
        string message,
        Dictionary<string, byte[]> contentByLogicalPath,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        if (path is null || !contentByLogicalPath.TryGetValue(path, out byte[]? bytes))
        {
            Add(diagnostics, diagnosticCode, path, message);
            return null;
        }

        return Sha256Hex(bytes);
    }

    private static ThemeSignatureVerificationStatus ValidateSignature(
        ThemeIntegrityVerificationRequestV2 request,
        string? packageHash,
        string? themeManifestHash,
        string? integrityManifestHash,
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        bool enumPolicyValid = Enum.IsDefined(request.Policy.DistributionChannel)
                               && Enum.IsDefined(request.Policy.SignatureRequirement);
        bool stableOrStore = request.Policy.DistributionChannel is ThemeDistributionChannel.Stable or ThemeDistributionChannel.Store;
        if (!enumPolicyValid
            || (stableOrStore && request.Policy.SignatureRequirement != ThemeSignatureRequirement.Required))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.ChannelPolicyViolation, null,
                "Distribution channel and signature requirement are inconsistent.");
        }

        if (request.Policy.DeveloperExceptionAuthorized
            && request.Policy.DistributionChannel != ThemeDistributionChannel.Developer)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnauthorizedDeveloperException, null,
                "Developer signature exception is authorized outside the Developer channel.");
        }

        bool signatureRequired = stableOrStore
                                 || request.Policy.SignatureRequirement == ThemeSignatureRequirement.Required;
        if (request.SignatureEnvelope is null)
        {
            if (signatureRequired)
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.MissingSignature, null,
                    "A signature is required by channel policy.");
                return ThemeSignatureVerificationStatus.Missing;
            }

            if (request.Policy.DistributionChannel == ThemeDistributionChannel.Developer)
            {
                if (!request.Policy.DeveloperExceptionAuthorized)
                {
                    Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnauthorizedDeveloperException, null,
                        "Unsigned Developer content requires an explicit authorized exception.");
                    return ThemeSignatureVerificationStatus.Missing;
                }

                return ThemeSignatureVerificationStatus.DeveloperExceptionAccepted;
            }

            return ThemeSignatureVerificationStatus.NotRequired;
        }

        ThemeSignatureEnvelopeV1 envelope = request.SignatureEnvelope;
        if (!Enum.IsDefined(envelope.Algorithm)
            || envelope.Algorithm != ThemeSignatureAlgorithm.EcdsaP256Sha256)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm, null,
                "Signature algorithm is unsupported.");
            return ThemeSignatureVerificationStatus.UnsupportedAlgorithm;
        }

        if (!Enum.IsDefined(envelope.SignatureEncoding)
            || envelope.SignatureEncoding != ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedSignatureEncoding, null,
                "Signature encoding is unsupported.");
            return ThemeSignatureVerificationStatus.UnsupportedAlgorithm;
        }

        if (!Enum.IsDefined(envelope.SignedPayloadType)
            || envelope.SignedPayloadType != ThemeSignedPayloadType.TccThemePackageSignatureV1
            || !string.Equals(envelope.EnvelopeVersion, ContractVersions.ThemeSignatureEnvelopeSchemaV1, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(envelope.PublisherId)
            || string.IsNullOrWhiteSpace(envelope.KeyId))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.MissingSecurityField, null,
                "Signature envelope constants or required identity fields are invalid.");
            return ThemeSignatureVerificationStatus.Invalid;
        }

        byte[] signature;
        try
        {
            signature = Convert.FromBase64String(envelope.Signature);
        }
        catch (FormatException)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                "Signature is not valid Base64.");
            return ThemeSignatureVerificationStatus.Invalid;
        }

        if (signature.Length != 64
            || !string.Equals(Convert.ToBase64String(signature), envelope.Signature, StringComparison.Ordinal))
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                "ECDSA P-256 IEEE P1363 signature must use canonical Base64 and decode to exactly 64 bytes.");
            return ThemeSignatureVerificationStatus.Invalid;
        }

        ThemeTrustedSignerV1[] signerMatches = request.TrustSnapshot.TrustedSigners
            .Where(candidate => string.Equals(candidate.PublisherId, envelope.PublisherId, StringComparison.Ordinal)
                                && string.Equals(candidate.KeyId, envelope.KeyId, StringComparison.Ordinal))
            .ToArray();
        if (signerMatches.Length == 0)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnknownSigner, null,
                "Publisher/key identity is not present in the trust snapshot.");
            return ThemeSignatureVerificationStatus.UnknownSigner;
        }

        if (signerMatches.Length != 1)
        {
            return ThemeSignatureVerificationStatus.UnknownSigner;
        }

        ThemeTrustedSignerV1 signer = signerMatches[0];
        if (signer.TrustState == ThemeTrustState.Revoked)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.RevokedSigner, null,
                "Publisher/key identity is revoked by the trust snapshot.");
            return ThemeSignatureVerificationStatus.RevokedSigner;
        }

        if (!Enum.IsDefined(signer.TrustState)
            || signer.TrustState != ThemeTrustState.Trusted
            || signer.Algorithm != ThemeSignatureAlgorithm.EcdsaP256Sha256
            || signer.PublicKeyEncoding != ThemePublicKeyEncoding.SubjectPublicKeyInfo
            || packageHash is null
            || themeManifestHash is null
            || integrityManifestHash is null)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                "Trusted signer material or signed evidence is invalid.");
            return ThemeSignatureVerificationStatus.Invalid;
        }

        try
        {
            byte[] publicKey = Convert.FromBase64String(signer.PublicKey);
            if (!HasApprovedNamedCurve(publicKey))
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm, null,
                    "SPKI must declare the approved NIST P-256 named curve OID.");
                return ThemeSignatureVerificationStatus.UnsupportedAlgorithm;
            }
            using ECDsa verifier = ECDsa.Create();
            verifier.ImportSubjectPublicKeyInfo(publicKey, out int bytesRead);
            ECCurve actualCurve = verifier.ExportParameters(false).Curve;
            if (!actualCurve.IsNamed || actualCurve.Oid.Value != "1.2.840.10045.3.1.7")
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm, null,
                    "Imported public key curve identity is not the approved NIST P-256 OID.");
                return ThemeSignatureVerificationStatus.UnsupportedAlgorithm;
            }
            byte[] payload = BuildSignaturePayload(
                request.ThemeId.Value,
                request.Version.Value,
                packageHash,
                themeManifestHash,
                integrityManifestHash,
                envelope.PublisherId,
                envelope.KeyId);
            if (bytesRead != publicKey.Length
                || !verifier.VerifyData(
                    payload,
                    signature,
                    HashAlgorithmName.SHA256,
                    DSASignatureFormat.IeeeP1363FixedFieldConcatenation))
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                    "Signature does not verify against the canonical payload and trusted key.");
                return ThemeSignatureVerificationStatus.Invalid;
            }
        }
        catch (CryptographicException)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                "Trusted public key or signature material is malformed.");
            return ThemeSignatureVerificationStatus.Invalid;
        }
        catch (FormatException)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                "Trusted public key is not valid Base64.");
            return ThemeSignatureVerificationStatus.Invalid;
        }
        catch (ArgumentException)
        {
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                "Trusted public key material is invalid.");
            return ThemeSignatureVerificationStatus.Invalid;
        }
        catch (PlatformNotSupportedException exception) when (exception.InnerException is CryptographicException)
        {
            // Windows CNG reports invalid EC point parameters through this wrapper.
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSignature, null,
                "Trusted public key parameters are invalid for the cryptographic provider.");
            return ThemeSignatureVerificationStatus.Invalid;
        }

        return ThemeSignatureVerificationStatus.Valid;
    }

    private static bool HasApprovedNamedCurve(byte[] spki)
    {
        try
        {
            AsnReader reader = new(spki, AsnEncodingRules.DER);
            AsnReader sequence = reader.ReadSequence();
            AsnReader algorithm = sequence.ReadSequence();
            if (algorithm.ReadObjectIdentifier() != "1.2.840.10045.2.1"
                || algorithm.ReadObjectIdentifier() != "1.2.840.10045.3.1.7") return false;
            algorithm.ThrowIfNotEmpty();
            sequence.ReadBitString(out int unusedBits);
            sequence.ThrowIfNotEmpty();
            reader.ThrowIfNotEmpty();
            return unusedBits == 0;
        }
        catch (AsnContentException)
        {
            return false;
        }
    }

    private static void AddReadFailure(
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics,
        string path) =>
        Add(diagnostics, ThemeIntegrityDiagnosticCodes.PackageReadFailure, path,
            "Package content bytes are unavailable for an enumerated file.");

    private static void Add(
        ICollection<ThemeIntegrityDiagnosticV1> diagnostics,
        string code,
        string? canonicalPath,
        string message)
    {
        ThemeIntegrityDiagnosticV1 diagnostic = new(code, ThemeDiagnosticsSeverity.Error, canonicalPath, message);
        if (!diagnostics.Contains(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    private sealed record CanonicalEntry(ThemePackageContentEntryV1 Source, string CanonicalPath);
}
