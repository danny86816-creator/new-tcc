using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Tcc.Presentation.Contracts.Theme;

/// <summary>Strict descriptive result transport. Deserialization never issues trusted runtime objects.</summary>
public static class ThemeCompatibilityJsonV2
{
    public static string SerializeResult(ThemeCompatibilityResultV2 result)
    {
        ArgumentNullException.ThrowIfNull(result);
        ValidateResult(result);
        using MemoryStream buffer = new();
        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("contract_version", result.ContractVersion);
            writer.WriteString("status", Wire(result.Status));
            writer.WriteString("evaluation_state", Wire(result.EvaluationState));
            writer.WriteString("theme_id", result.ThemeId?.Value);
            writer.WriteString("theme_version", result.ThemeVersion?.Value);
            writer.WriteString("package_hash", result.PackageHash);
            writer.WriteString("selected_core_version", result.SelectedCoreVersion);
            writer.WriteString("selected_theme_api_version", result.SelectedThemeApiVersion);
            writer.WriteString("selected_ux_contract_version", result.SelectedUxContractVersion);
            writer.WriteString("selected_manifest_schema_version", result.SelectedManifestSchemaVersion);
            writer.WriteString("runtime_validation_status", Wire(result.RuntimeValidationStatus));
            writer.WriteString("capability_evidence_status", Wire(result.CapabilityEvidenceStatus));
            writer.WriteStartArray("authorized_capabilities");
            foreach (string item in result.AuthorizedCapabilities)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
            writer.WriteStartArray("enabled_capabilities");
            foreach (string item in result.EnabledCapabilities)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
            writer.WriteStartArray("disabled_capabilities");
            foreach (string item in result.DisabledCapabilities)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
            writer.WriteStartArray("notices");
            foreach (ThemeCompatibilityNoticeV2 item in result.Notices)
            {
                writer.WriteStartObject();
                writer.WriteString("kind", Wire(item.Kind));
                writer.WriteString("dimension", Wire(item.Dimension));
                writer.WriteNumber("sequence", item.Sequence);
                writer.WriteNull("diagnostic_code");
                writer.WriteString("message", item.Message);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteStartArray("failures");
            foreach (ThemeCompatibilityFailureV2 item in result.Failures)
            {
                writer.WriteStartObject();
                writer.WriteString("kind", Wire(item.Kind));
                writer.WriteString("dimension", Wire(item.Dimension));
                writer.WriteNumber("sequence", item.Sequence);
                writer.WriteNull("diagnostic_code");
                writer.WriteString("message", item.Message);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteString("accessibility_validation_status", Wire(result.AccessibilityValidationStatus));
            writer.WriteString("safety_invariants_status", Wire(result.SafetyInvariantsStatus));
            writer.WriteStartArray("safety_failures");
            foreach (ThemeCompatibilitySafetyStatusV2 item in result.SafetyFailures)
            {
                writer.WriteStringValue(Wire(item));
            }
            writer.WriteEndArray();
            writer.WriteString("migration_evidence_status", Wire(result.MigrationEvidenceStatus));
            if (result.MigrationRequired is bool migrationRequired) writer.WriteBoolean("migration_required", migrationRequired);
            else writer.WriteNull("migration_required");
            if (result.MigrationReady is bool migrationReady) writer.WriteBoolean("migration_ready", migrationReady);
            else writer.WriteNull("migration_ready");
            writer.WriteString("rollback_evidence_status", Wire(result.RollbackEvidenceStatus));
            if (result.RollbackRequired is bool rollbackRequired) writer.WriteBoolean("rollback_required", rollbackRequired);
            else writer.WriteNull("rollback_required");
            if (result.RollbackAvailable is bool rollbackAvailable) writer.WriteBoolean("rollback_available", rollbackAvailable);
            else writer.WriteNull("rollback_available");
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    public static ThemeCompatibilityResultV2 DeserializeResult(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement value = document.RootElement;
        RequireProperties(value,
            "contract_version",
            "status",
            "evaluation_state",
            "theme_id",
            "theme_version",
            "package_hash",
            "selected_core_version",
            "selected_theme_api_version",
            "selected_ux_contract_version",
            "selected_manifest_schema_version",
            "runtime_validation_status",
            "capability_evidence_status",
            "authorized_capabilities",
            "enabled_capabilities",
            "disabled_capabilities",
            "notices",
            "failures",
            "accessibility_validation_status",
            "safety_invariants_status",
            "safety_failures",
            "migration_evidence_status",
            "migration_required",
            "migration_ready",
            "rollback_evidence_status",
            "rollback_required",
            "rollback_available");
        ImmutableArray<ThemeCompatibilityFailureV2>.Builder failures = ImmutableArray.CreateBuilder<ThemeCompatibilityFailureV2>();
        foreach (JsonElement item in Array(value, "failures"))
        {
            RequireProperties(item, "kind", "dimension", "sequence", "diagnostic_code", "message");
            try
            {
                failures.Add(new(ReadEnum<ThemeCompatibilityFailureKindV2>(item.GetProperty("kind")),
                    ReadEnum<ThemeCompatibilityDimensionV2>(item.GetProperty("dimension")),
                    Integer(item.GetProperty("sequence")), Text(item, "diagnostic_code", true), Text(item, "message", false)!));
            }
            catch (ArgumentException)
            {
                throw new JsonException("Invalid compatibility detail.");
            }
        }
        ImmutableArray<ThemeCompatibilityNoticeV2>.Builder notices = ImmutableArray.CreateBuilder<ThemeCompatibilityNoticeV2>();
        foreach (JsonElement item in Array(value, "notices"))
        {
            RequireProperties(item, "kind", "dimension", "sequence", "diagnostic_code", "message");
            try
            {
                notices.Add(new(ReadEnum<ThemeCompatibilityNoticeKindV2>(item.GetProperty("kind")),
                    ReadEnum<ThemeCompatibilityDimensionV2>(item.GetProperty("dimension")),
                    Integer(item.GetProperty("sequence")), Text(item, "diagnostic_code", true), Text(item, "message", false)!));
            }
            catch (ArgumentException)
            {
                throw new JsonException("Invalid compatibility detail.");
            }
        }
        ThemeCompatibilityResultV2 result = new(
            Text(value, "contract_version", false)!,
            ReadEnum<ThemeCompatibilityStatusV2>(value.GetProperty("status")),
            ReadEnum<ThemeCompatibilityEvaluationStateV2>(value.GetProperty("evaluation_state")),
            Text(value, "theme_id", true) is string themeId ? new ThemeId(themeId) : null,
            Text(value, "theme_version", true) is string themeVersion ? new ThemeVersion(themeVersion) : null,
            Text(value, "package_hash", true),
            Text(value, "selected_core_version", true),
            Text(value, "selected_theme_api_version", true),
            Text(value, "selected_ux_contract_version", true),
            Text(value, "selected_manifest_schema_version", true),
            ReadEnum<ThemeCompatibilityEvidenceStatusV2>(value.GetProperty("runtime_validation_status")),
            ReadEnum<ThemeCompatibilityEvidenceStatusV2>(value.GetProperty("capability_evidence_status")),
            Strings(value, "authorized_capabilities"),
            Strings(value, "enabled_capabilities"),
            Strings(value, "disabled_capabilities"),
            notices.ToImmutable(),
            failures.ToImmutable(),
            ReadEnum<ThemeCompatibilityAccessibilityStatusV2>(value.GetProperty("accessibility_validation_status")),
            ReadEnum<ThemeCompatibilitySafetyStatusV2>(value.GetProperty("safety_invariants_status")),
            SafetyValues(value, "safety_failures"),
            ReadEnum<ThemeCompatibilityEvidenceStatusV2>(value.GetProperty("migration_evidence_status")),
            Boolean(value, "migration_required"),
            Boolean(value, "migration_ready"),
            ReadEnum<ThemeCompatibilityEvidenceStatusV2>(value.GetProperty("rollback_evidence_status")),
            Boolean(value, "rollback_required"),
            Boolean(value, "rollback_available"));
        ValidateResult(result);
        return result;
    }

    private static string Wire<T>(T value) where T : struct, Enum =>
        Enum.IsDefined(value) ? JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString())
            : throw new JsonException("Undefined compatibility enum.");

    private static T ReadEnum<T>(JsonElement value) where T : struct, Enum
    {
        if (value.ValueKind == JsonValueKind.String)
        {
            foreach (T candidate in Enum.GetValues<T>())
            {
                if (string.Equals(value.GetString(), Wire(candidate), StringComparison.Ordinal)) return candidate;
            }
        }
        throw new JsonException("Expected a canonical compatibility enum string.");
    }

    private static void RequireProperties(JsonElement value, params string[] names)
    {
        if (value.ValueKind != JsonValueKind.Object) throw new JsonException("Expected object.");
        HashSet<string> remaining = new(names, StringComparer.Ordinal);
        foreach (JsonProperty property in value.EnumerateObject())
        {
            if (!remaining.Remove(property.Name)) throw new JsonException("Unknown or duplicate property.");
        }
        if (remaining.Count != 0) throw new JsonException("Missing required property.");
    }

    private static string? Text(JsonElement value, string name, bool nullable)
    {
        JsonElement field = value.GetProperty(name);
        if (nullable && field.ValueKind == JsonValueKind.Null) return null;
        if (field.ValueKind != JsonValueKind.String) throw new JsonException("Expected string.");
        return field.GetString();
    }

    private static int Integer(JsonElement value) => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int result)
        ? result : throw new JsonException("Expected integer.");

    private static bool? Boolean(JsonElement value, string name) => value.GetProperty(name).ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => throw new JsonException("Expected nullable Boolean."),
    };

    private static JsonElement.ArrayEnumerator Array(JsonElement value, string name)
    {
        JsonElement field = value.GetProperty(name);
        if (field.ValueKind != JsonValueKind.Array) throw new JsonException("Expected array.");
        return field.EnumerateArray();
    }

    private static ImmutableArray<string> Strings(JsonElement value, string name)
    {
        ImmutableArray<string>.Builder result = ImmutableArray.CreateBuilder<string>();
        foreach (JsonElement item in Array(value, name))
        {
            if (item.ValueKind != JsonValueKind.String) throw new JsonException("Expected string array item.");
            result.Add(item.GetString()!);
        }
        return result.ToImmutable();
    }

    private static ImmutableArray<ThemeCompatibilitySafetyStatusV2> SafetyValues(JsonElement value, string name)
    {
        ImmutableArray<ThemeCompatibilitySafetyStatusV2>.Builder result = ImmutableArray.CreateBuilder<ThemeCompatibilitySafetyStatusV2>();
        foreach (JsonElement item in Array(value, name)) result.Add(ReadEnum<ThemeCompatibilitySafetyStatusV2>(item));
        return result.ToImmutable();
    }

    // Fixed templates prevent raw package, exception, version, or machine data from entering messages.
    internal static void ValidateNotice(ThemeCompatibilityNoticeKindV2 kind, ThemeCompatibilityDimensionV2 dimension,
        int sequence, string? diagnosticCode, string message)
    {
        bool pair = kind switch
        {
            ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled => dimension == ThemeCompatibilityDimensionV2.Capability,
            ThemeCompatibilityNoticeKindV2.DecorativePresentationDegradation => dimension is ThemeCompatibilityDimensionV2.Runtime or ThemeCompatibilityDimensionV2.Capability,
            ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback => dimension == ThemeCompatibilityDimensionV2.Runtime,
            _ => false,
        };
        if (!pair || sequence < 0 || diagnosticCode is not null || string.IsNullOrEmpty(message)
            || !string.Equals(message, $"{kind} in {dimension}.", StringComparison.Ordinal))
            throw new ArgumentException("Invalid compatibility notice.");
    }

    internal static void ValidateFailure(ThemeCompatibilityFailureKindV2 kind, ThemeCompatibilityDimensionV2 dimension,
        int sequence, string? diagnosticCode, string message)
    {
        bool pair = kind switch
        {
            ThemeCompatibilityFailureKindV2.MissingTrustedContext or ThemeCompatibilityFailureKindV2.InvalidRuntimeContext
                or ThemeCompatibilityFailureKindV2.ContextBindingMismatch => dimension == ThemeCompatibilityDimensionV2.Precondition,
            ThemeCompatibilityFailureKindV2.ContentSnapshotUnavailable or ThemeCompatibilityFailureKindV2.IntegrityNotVerified
                or ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch => dimension == ThemeCompatibilityDimensionV2.Integrity,
            ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema or ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid
                or ThemeCompatibilityFailureKindV2.InvalidSchemaSupportInput => dimension == ThemeCompatibilityDimensionV2.ManifestSchema,
            ThemeCompatibilityFailureKindV2.InvalidVersionInput or ThemeCompatibilityFailureKindV2.UnsatisfiableVersionRange
                or ThemeCompatibilityFailureKindV2.ConflictingVersionDeclaration => dimension == ThemeCompatibilityDimensionV2.VersionDeclarations,
            ThemeCompatibilityFailureKindV2.CoreVersionIncompatible => dimension == ThemeCompatibilityDimensionV2.CoreVersion,
            ThemeCompatibilityFailureKindV2.ThemeApiNoCompatibleVersion => dimension == ThemeCompatibilityDimensionV2.ThemeApiVersion,
            ThemeCompatibilityFailureKindV2.UxContractNoCompatibleVersion => dimension == ThemeCompatibilityDimensionV2.UxContractVersion,
            ThemeCompatibilityFailureKindV2.PlatformUnsupported or ThemeCompatibilityFailureKindV2.InstallationModeUnsupported => dimension == ThemeCompatibilityDimensionV2.PlatformMode,
            ThemeCompatibilityFailureKindV2.RuntimeValidationFailed => dimension == ThemeCompatibilityDimensionV2.Runtime,
            ThemeCompatibilityFailureKindV2.AccessibilityValidationFailed => dimension == ThemeCompatibilityDimensionV2.Accessibility,
            ThemeCompatibilityFailureKindV2.SafetyValidationFailed or ThemeCompatibilityFailureKindV2.AssetInventoryInvalid
                or ThemeCompatibilityFailureKindV2.MotionSafetyFailed or ThemeCompatibilityFailureKindV2.AudioSafetyFailed => dimension == ThemeCompatibilityDimensionV2.Safety,
            ThemeCompatibilityFailureKindV2.CapabilityBlocked => dimension == ThemeCompatibilityDimensionV2.Capability,
            ThemeCompatibilityFailureKindV2.MigrationNotReady => dimension == ThemeCompatibilityDimensionV2.Migration,
            ThemeCompatibilityFailureKindV2.RollbackUnavailable => dimension == ThemeCompatibilityDimensionV2.Rollback,
            ThemeCompatibilityFailureKindV2.EvidenceMissing or ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated
                or ThemeCompatibilityFailureKindV2.EvidenceScopeMismatch or ThemeCompatibilityFailureKindV2.EvidenceMalformed =>
                dimension is >= ThemeCompatibilityDimensionV2.Runtime and <= ThemeCompatibilityDimensionV2.Rollback,
            _ => false,
        };
        if (!pair || sequence < 0 || diagnosticCode is not null || string.IsNullOrEmpty(message)
            || !string.Equals(message, $"{kind} in {dimension}.", StringComparison.Ordinal))
            throw new ArgumentException("Invalid compatibility failure.");
    }

    private static ThemeCompatibilityStatusV2 Primary(ThemeCompatibilityFailureKindV2 kind) => kind switch
    {
        ThemeCompatibilityFailureKindV2.MissingTrustedContext or ThemeCompatibilityFailureKindV2.InvalidRuntimeContext
            or ThemeCompatibilityFailureKindV2.ContextBindingMismatch or ThemeCompatibilityFailureKindV2.InvalidSchemaSupportInput
            or ThemeCompatibilityFailureKindV2.EvidenceScopeMismatch or ThemeCompatibilityFailureKindV2.EvidenceMalformed => ThemeCompatibilityStatusV2.RefusedPrecondition,
        ThemeCompatibilityFailureKindV2.ContentSnapshotUnavailable or ThemeCompatibilityFailureKindV2.IntegrityNotVerified
            or ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch => ThemeCompatibilityStatusV2.TrustedEvidenceFailure,
        ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema => ThemeCompatibilityStatusV2.UnsupportedManifestSchema,
        ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid => ThemeCompatibilityStatusV2.ManifestSchemaInvalid,
        ThemeCompatibilityFailureKindV2.InvalidVersionInput => ThemeCompatibilityStatusV2.InvalidVersionInput,
        ThemeCompatibilityFailureKindV2.UnsatisfiableVersionRange => ThemeCompatibilityStatusV2.UnsatisfiableVersionRange,
        ThemeCompatibilityFailureKindV2.ConflictingVersionDeclaration => ThemeCompatibilityStatusV2.ConflictingVersionDeclaration,
        ThemeCompatibilityFailureKindV2.CoreVersionIncompatible => ThemeCompatibilityStatusV2.IncompatibleCoreVersion,
        ThemeCompatibilityFailureKindV2.ThemeApiNoCompatibleVersion => ThemeCompatibilityStatusV2.IncompatibleThemeApiVersion,
        ThemeCompatibilityFailureKindV2.UxContractNoCompatibleVersion => ThemeCompatibilityStatusV2.IncompatibleUxContractVersion,
        ThemeCompatibilityFailureKindV2.PlatformUnsupported => ThemeCompatibilityStatusV2.UnsupportedPlatform,
        ThemeCompatibilityFailureKindV2.InstallationModeUnsupported => ThemeCompatibilityStatusV2.UnsupportedInstallationMode,
        ThemeCompatibilityFailureKindV2.RuntimeValidationFailed => ThemeCompatibilityStatusV2.RuntimeValidationFailed,
        ThemeCompatibilityFailureKindV2.AccessibilityValidationFailed => ThemeCompatibilityStatusV2.AccessibilityValidationFailed,
        ThemeCompatibilityFailureKindV2.SafetyValidationFailed or ThemeCompatibilityFailureKindV2.AssetInventoryInvalid
            or ThemeCompatibilityFailureKindV2.MotionSafetyFailed or ThemeCompatibilityFailureKindV2.AudioSafetyFailed => ThemeCompatibilityStatusV2.SafetyValidationFailed,
        ThemeCompatibilityFailureKindV2.CapabilityBlocked => ThemeCompatibilityStatusV2.BlockedCapability,
        ThemeCompatibilityFailureKindV2.MigrationNotReady => ThemeCompatibilityStatusV2.MigrationNotReady,
        ThemeCompatibilityFailureKindV2.RollbackUnavailable => ThemeCompatibilityStatusV2.RollbackUnavailable,
        _ => ThemeCompatibilityStatusV2.EvidenceUnavailable,
    };

    private static void Require(bool condition)
    {
        if (!condition) throw new JsonException("Contradictory compatibility result.");
    }

    private static int Precedence(ThemeCompatibilityFailureV2 failure) => failure.Kind is
        ThemeCompatibilityFailureKindV2.EvidenceScopeMismatch or ThemeCompatibilityFailureKindV2.EvidenceMalformed
            ? 0 : (int)failure.Dimension;

    private static void Owner(ThemeCompatibilityResultV2 result, ThemeCompatibilityDimensionV2 dimension,
        bool notEvaluated, bool failed)
    {
        bool missing = result.Failures.Any(f => f.Dimension == dimension
            && f.Kind is ThemeCompatibilityFailureKindV2.EvidenceMissing or ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated);
        bool failure = result.Failures.Any(f => f.Dimension == dimension
            && f.Kind is not ThemeCompatibilityFailureKindV2.EvidenceMissing and not ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated);
        Require(notEvaluated == missing && failed == failure);
    }

    // Lexical transport validation only; the sealed negotiator owns range parsing and selection.
    private static bool Version(string? value, int components)
    {
        if (value is null) return true;
        string[] parts = value.Split('.');
        if (parts.Length != components) return false;
        foreach (string part in parts)
        {
            if (part.Length == 0 || (part.Length > 1 && part[0] == '0')) return false;
            foreach (char c in part) if (c is < '0' or > '9') return false;
        }
        return true;
    }

    private static void Transition(ThemeCompatibilityEvidenceStatusV2 status, bool? required, bool? ready, bool migration)
    {
        Require(Enum.IsDefined(status));
        if (status == ThemeCompatibilityEvidenceStatusV2.NotEvaluated) Require(required is null && ready is null);
        else
        {
            Require(required is not null);
            if (status == ThemeCompatibilityEvidenceStatusV2.NotApplicable) Require(required == false);
            if (required == true) Require(ready is not null && status != ThemeCompatibilityEvidenceStatusV2.NotApplicable);
            if (required == true && status == ThemeCompatibilityEvidenceStatusV2.Passed) Require(ready == true);
            if (migration && required == false) Require(ready is null);
        }
    }

    private static void ValidateResult(ThemeCompatibilityResultV2 result)
    {
        Require(result.ContractVersion == ContractVersions.ThemeCompatibilityContractV2);
        Require(Enum.IsDefined(result.Status) && Enum.IsDefined(result.EvaluationState)
            && Enum.IsDefined(result.RuntimeValidationStatus) && Enum.IsDefined(result.CapabilityEvidenceStatus)
            && Enum.IsDefined(result.AccessibilityValidationStatus) && Enum.IsDefined(result.SafetyInvariantsStatus));
        Require(result.RuntimeValidationStatus != ThemeCompatibilityEvidenceStatusV2.NotApplicable
            && result.CapabilityEvidenceStatus != ThemeCompatibilityEvidenceStatusV2.NotApplicable);
        Require(result.ThemeId.HasValue == result.ThemeVersion.HasValue);
        Require(result.ThemeId is null || !string.IsNullOrWhiteSpace(result.ThemeId.Value.Value));
        Require(result.ThemeVersion is null || !string.IsNullOrWhiteSpace(result.ThemeVersion.Value.Value));
        Require(result.PackageHash is null || (result.PackageHash.Length == 64 && result.PackageHash.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f')));
        Require(Version(result.SelectedCoreVersion, 3) && Version(result.SelectedThemeApiVersion, 3)
            && Version(result.SelectedUxContractVersion, 3) && Version(result.SelectedManifestSchemaVersion, 2));
        Require(result.SelectedThemeApiVersion is null or "1.0.0");
        Require(result.SelectedUxContractVersion is null or "1.1.0");
        Require(result.SelectedManifestSchemaVersion is null or "1.0");
        for (int i = 0; i < result.Failures.Length; i++)
        {
            ThemeCompatibilityFailureV2 f = result.Failures[i];
            Require(f is not null && f.Sequence == i);
            ValidateFailure(f!.Kind, f.Dimension, f.Sequence, f.DiagnosticCode, f.Message);
            if (i > 0)
            {
                ThemeCompatibilityFailureV2 previous = result.Failures[i - 1];
                Require(Precedence(previous) <= Precedence(f));
                // The sealed Phase5A sequence is evidence, including duplicates;
                // never sort or reinterpret that declaration sequence.
                if (previous.Dimension == f.Dimension && Precedence(previous) == Precedence(f)
                    && f.Dimension != ThemeCompatibilityDimensionV2.VersionDeclarations)
                    Require(previous.Kind <= f.Kind);
            }
        }
        for (int i = 0; i < result.Notices.Length; i++)
        {
            ThemeCompatibilityNoticeV2 n = result.Notices[i];
            Require(n is not null && n.Sequence == i);
            ValidateNotice(n!.Kind, n.Dimension, n.Sequence, n.DiagnosticCode, n.Message);
        }
        foreach (ThemeCompatibilitySafetyStatusV2 failure in result.SafetyFailures)
            Require(Enum.IsDefined(failure) && failure is not ThemeCompatibilitySafetyStatusV2.NotEvaluated and not ThemeCompatibilitySafetyStatusV2.Passed);
        Require(result.SafetyFailures.Distinct().Count() == result.SafetyFailures.Length);
        Require(result.SafetyFailures.SequenceEqual(result.SafetyFailures.Order()));
        if (result.SafetyInvariantsStatus is ThemeCompatibilitySafetyStatusV2.NotEvaluated or ThemeCompatibilitySafetyStatusV2.Passed)
            Require(result.SafetyFailures.IsEmpty);
        else Require(!result.SafetyFailures.IsEmpty && result.SafetyInvariantsStatus == result.SafetyFailures[0]);
        foreach (ImmutableArray<string> values in new[] { result.AuthorizedCapabilities, result.EnabledCapabilities, result.DisabledCapabilities })
        {
            Require(values.All(v => !string.IsNullOrWhiteSpace(v)) && values.Distinct(StringComparer.Ordinal).Count() == values.Length);
        }
        Require(result.EnabledCapabilities.All(v => result.AuthorizedCapabilities.Contains(v, StringComparer.Ordinal)));
        Require(!result.EnabledCapabilities.Intersect(result.DisabledCapabilities, StringComparer.Ordinal).Any());
        if (result.CapabilityEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated)
            Require(result.AuthorizedCapabilities.IsEmpty && result.EnabledCapabilities.IsEmpty && result.DisabledCapabilities.IsEmpty);
        Transition(result.MigrationEvidenceStatus, result.MigrationRequired, result.MigrationReady, true);
        Transition(result.RollbackEvidenceStatus, result.RollbackRequired, result.RollbackAvailable, false);
        bool success = result.Status is ThemeCompatibilityStatusV2.Compatible or ThemeCompatibilityStatusV2.CompatibleWithDegradation
            or ThemeCompatibilityStatusV2.CompatibleUntestedDpiWithScalableFallback;
        bool fallback = result.Notices.Any(n => n.Kind == ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback);
        if (success)
        {
            Require(result.EvaluationState == ThemeCompatibilityEvaluationStateV2.Evaluated && result.Failures.IsEmpty
                && result.ThemeId is not null && result.ThemeVersion is not null && result.PackageHash is not null
                && result.SelectedCoreVersion is not null && result.SelectedThemeApiVersion is not null
                && result.SelectedUxContractVersion is not null && result.SelectedManifestSchemaVersion is not null
                && result.RuntimeValidationStatus == ThemeCompatibilityEvidenceStatusV2.Passed
                && result.CapabilityEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.Passed
                && result.AccessibilityValidationStatus == ThemeCompatibilityAccessibilityStatusV2.Validated
                && result.SafetyInvariantsStatus == ThemeCompatibilitySafetyStatusV2.Passed
                && result.MigrationEvidenceStatus is ThemeCompatibilityEvidenceStatusV2.Passed or ThemeCompatibilityEvidenceStatusV2.NotApplicable
                && result.RollbackEvidenceStatus is ThemeCompatibilityEvidenceStatusV2.Passed or ThemeCompatibilityEvidenceStatusV2.NotApplicable);
            Require(result.Status == (fallback ? ThemeCompatibilityStatusV2.CompatibleUntestedDpiWithScalableFallback
                : result.Notices.IsEmpty ? ThemeCompatibilityStatusV2.Compatible : ThemeCompatibilityStatusV2.CompatibleWithDegradation));
        }
        else if (result.Status == ThemeCompatibilityStatusV2.NotEvaluated)
        {
            Require(result.EvaluationState == ThemeCompatibilityEvaluationStateV2.NotEvaluated && result.Failures.IsEmpty && result.Notices.IsEmpty
                && result.ThemeId is null && result.PackageHash is null);
        }
        else
        {
            Require(!result.Failures.IsEmpty && result.Status == Primary(result.Failures[0].Kind));
            bool refusal = result.Status is ThemeCompatibilityStatusV2.RefusedPrecondition or ThemeCompatibilityStatusV2.TrustedEvidenceFailure;
            Require(result.EvaluationState == (refusal ? ThemeCompatibilityEvaluationStateV2.RefusedPrecondition : ThemeCompatibilityEvaluationStateV2.Evaluated));
        }
        bool stopped = result.EvaluationState != ThemeCompatibilityEvaluationStateV2.Evaluated
            || result.Status is ThemeCompatibilityStatusV2.UnsupportedManifestSchema or ThemeCompatibilityStatusV2.ManifestSchemaInvalid;
        if (stopped)
        {
            Require(result.SelectedCoreVersion is null && result.SelectedThemeApiVersion is null
                && result.SelectedUxContractVersion is null && result.SelectedManifestSchemaVersion is null
                && result.RuntimeValidationStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated
                && result.CapabilityEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated
                && result.AccessibilityValidationStatus == ThemeCompatibilityAccessibilityStatusV2.NotEvaluated
                && result.SafetyInvariantsStatus == ThemeCompatibilitySafetyStatusV2.NotEvaluated
                && result.MigrationEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated
                && result.RollbackEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated && result.Notices.IsEmpty);
            Require(result.Failures.All(f => f.Dimension <= ThemeCompatibilityDimensionV2.ManifestSchema
                || f.Kind is ThemeCompatibilityFailureKindV2.EvidenceMalformed or ThemeCompatibilityFailureKindV2.EvidenceScopeMismatch));
            if (!result.Failures.IsEmpty)
                Require(result.Failures.All(f => Precedence(f) == Precedence(result.Failures[0])));
        }
        else
        {
            Require(result.SelectedManifestSchemaVersion == "1.0");
            Owner(result, ThemeCompatibilityDimensionV2.Runtime,
                result.RuntimeValidationStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
                result.RuntimeValidationStatus == ThemeCompatibilityEvidenceStatusV2.Failed);
            Owner(result, ThemeCompatibilityDimensionV2.Capability,
                result.CapabilityEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
                result.CapabilityEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.Failed);
            Owner(result, ThemeCompatibilityDimensionV2.Accessibility,
                result.AccessibilityValidationStatus == ThemeCompatibilityAccessibilityStatusV2.NotEvaluated,
                result.AccessibilityValidationStatus == ThemeCompatibilityAccessibilityStatusV2.Failed);
            Owner(result, ThemeCompatibilityDimensionV2.Safety,
                result.SafetyInvariantsStatus == ThemeCompatibilitySafetyStatusV2.NotEvaluated,
                result.SafetyInvariantsStatus is not ThemeCompatibilitySafetyStatusV2.NotEvaluated and not ThemeCompatibilitySafetyStatusV2.Passed);
            Owner(result, ThemeCompatibilityDimensionV2.Migration,
                result.MigrationEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
                result.MigrationEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.Failed);
            Owner(result, ThemeCompatibilityDimensionV2.Rollback,
                result.RollbackEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
                result.RollbackEvidenceStatus == ThemeCompatibilityEvidenceStatusV2.Failed);
        }
        foreach (ThemeCompatibilityFailureV2 f in result.Failures)
        {
            if (f.Kind is ThemeCompatibilityFailureKindV2.InvalidVersionInput or ThemeCompatibilityFailureKindV2.UnsatisfiableVersionRange
                or ThemeCompatibilityFailureKindV2.ConflictingVersionDeclaration or ThemeCompatibilityFailureKindV2.CoreVersionIncompatible)
                Require(result.SelectedCoreVersion is null);
            if (f.Kind == ThemeCompatibilityFailureKindV2.ThemeApiNoCompatibleVersion) Require(result.SelectedThemeApiVersion is null);
            if (f.Kind == ThemeCompatibilityFailureKindV2.UxContractNoCompatibleVersion) Require(result.SelectedUxContractVersion is null);
            if (f.Kind == ThemeCompatibilityFailureKindV2.MigrationNotReady)
                Require(result.MigrationRequired == true && result.MigrationReady != true);
            if (f.Kind == ThemeCompatibilityFailureKindV2.RollbackUnavailable)
                Require(result.RollbackRequired == true && result.RollbackAvailable != true);
        }
    }
}
