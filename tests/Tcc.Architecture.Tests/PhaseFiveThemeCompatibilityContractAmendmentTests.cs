using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Compatibility.V2;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFiveThemeCompatibilityContractAmendmentTests
{
    private const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    private static readonly string[] SerializerMethods = ["DeserializeResult", "SerializeResult"];
    private static readonly string[] DetailWireProperties = ["kind", "dimension", "sequence", "diagnostic_code", "message"];
    private static readonly string[] VersionFields = ["Schema", "ThemeApi", "ThemeArchitecture", "ThemeCompatibilityContractV2", "ThemeIntegritySchemaV2", "ThemeSignatureEnvelopeSchemaV1", "UxArchitectureDisplay", "UxContract"];
    private static readonly HashSet<Type> RuntimeDependencies =
    [
        typeof(object), typeof(void), typeof(string), typeof(bool), typeof(int), typeof(Nullable<>),
        typeof(ArgumentNullException), typeof(ArgumentException), typeof(StringComparer),
        typeof(IEnumerable<>), typeof(IEnumerator<>), typeof(IDisposable), typeof(KeyValuePair<,>),
        typeof(System.Collections.IEnumerable), typeof(System.Collections.IEnumerator),
        typeof(ImmutableArray), typeof(ImmutableArray<>), typeof(ImmutableArray<>.Builder),
        typeof(ImmutableDictionary), typeof(ImmutableDictionary<,>), typeof(Enumerable),
        typeof(IReadOnlyList<>), typeof(IReadOnlyDictionary<,>), typeof(IEqualityComparer<>),
        typeof(ThemePackageRef), typeof(ThemeIntegrityVerificationResultV2), typeof(ThemeIntegrityFileEvidenceV1),
        typeof(ThemeIntegrityDiagnosticV1), typeof(ThemeManifest), typeof(ThemeCompatibilityManifest),
        typeof(ThemePackageIdentity), typeof(ThemeCompatibilityDeclaration), typeof(ThemeVariant),
        typeof(ThemeFeatureFlags), typeof(ThemePresentationFeatureFlag), typeof(ThemeDegradedModeDeclaration),
        typeof(ThemeAssetDeclaration), typeof(ThemeRollbackDeclaration), typeof(ThemeVersionRange),
        typeof(ThemeWindowsCompatibility), typeof(ThemeCompatibilityEnvironmentV2), typeof(ThemeCompatibilityResultV2),
        typeof(ThemeCompatibilityFailureV2), typeof(ThemeCompatibilityNoticeV2),
        typeof(ThemeCompatibilityEvidenceStatusV2), typeof(ThemeCompatibilityAccessibilityStatusV2),
        typeof(ThemeCompatibilitySafetyStatusV2),
    ];

    internal static bool RuntimeDependencyAllowed(Type type, Assembly owner)
    {
        Type definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        return RuntimeDependencies.Contains(definition)
            || (definition.Assembly == owner && RuntimeTypeNames.Contains(definition.FullName, StringComparer.Ordinal));
    }
    internal static readonly string[] RuntimeTypeNames =
    [
        "Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRequestV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityCapabilityEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityAccessibilityEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilitySafetyEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityMigrationEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRollbackEvidenceV2",
    ];
    internal const string ResolverTypeName =
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2";

    private static readonly Dictionary<Type, (string Name, Type Type)[]> Shapes = new()
    {
        [typeof(ThemeCompatibilityEnvironmentV2)] = [("CoreVersion", typeof(string)), ("SupportedThemeApiVersions", typeof(ImmutableArray<string?>?)), ("SupportedUxContractVersions", typeof(ImmutableArray<string?>?)), ("SupportedManifestSchemaVersions", typeof(ImmutableArray<string?>?)), ("Platform", typeof(string)), ("Architecture", typeof(string)), ("InstallationMode", typeof(ThemeCompatibilityInstallationModeV2)), ("RuntimeBuildId", typeof(string)), ("ValidationProfileVersion", typeof(string)), ("RuntimeTargets", typeof(ImmutableArray<ThemeCompatibilityRuntimeTargetV2>)), ("Operation", typeof(ThemeCompatibilityOperationV2)), ("CurrentThemeId", typeof(ThemeId?)), ("CurrentThemeVersion", typeof(ThemeVersion?)), ("ThemeStateRevision", typeof(string))],
        [typeof(ThemeCompatibilityRuntimeTargetV2)] = [("TargetId", typeof(string)), ("Variant", typeof(ThemeVariantId)), ("Surface", typeof(UxSurfaceId)), ("DpiScale", typeof(decimal)), ("ViewportWidthDip", typeof(decimal)), ("ViewportHeightDip", typeof(decimal)), ("AccessibilityProfile", typeof(ThemeAccessibilityProfile))],
        [typeof(ThemeCompatibilityResultV2)] = [("ContractVersion", typeof(string)), ("Status", typeof(ThemeCompatibilityStatusV2)), ("EvaluationState", typeof(ThemeCompatibilityEvaluationStateV2)), ("ThemeId", typeof(ThemeId?)), ("ThemeVersion", typeof(ThemeVersion?)), ("PackageHash", typeof(string)), ("SelectedCoreVersion", typeof(string)), ("SelectedThemeApiVersion", typeof(string)), ("SelectedUxContractVersion", typeof(string)), ("SelectedManifestSchemaVersion", typeof(string)), ("RuntimeValidationStatus", typeof(ThemeCompatibilityEvidenceStatusV2)), ("CapabilityEvidenceStatus", typeof(ThemeCompatibilityEvidenceStatusV2)), ("AuthorizedCapabilities", typeof(ImmutableArray<string>)), ("EnabledCapabilities", typeof(ImmutableArray<string>)), ("DisabledCapabilities", typeof(ImmutableArray<string>)), ("Notices", typeof(ImmutableArray<ThemeCompatibilityNoticeV2>)), ("Failures", typeof(ImmutableArray<ThemeCompatibilityFailureV2>)), ("AccessibilityValidationStatus", typeof(ThemeCompatibilityAccessibilityStatusV2)), ("SafetyInvariantsStatus", typeof(ThemeCompatibilitySafetyStatusV2)), ("SafetyFailures", typeof(ImmutableArray<ThemeCompatibilitySafetyStatusV2>)), ("MigrationEvidenceStatus", typeof(ThemeCompatibilityEvidenceStatusV2)), ("MigrationRequired", typeof(bool?)), ("MigrationReady", typeof(bool?)), ("RollbackEvidenceStatus", typeof(ThemeCompatibilityEvidenceStatusV2)), ("RollbackRequired", typeof(bool?)), ("RollbackAvailable", typeof(bool?))],
        [typeof(ThemeCompatibilityFailureV2)] = [("Kind", typeof(ThemeCompatibilityFailureKindV2)), ("Dimension", typeof(ThemeCompatibilityDimensionV2)), ("Sequence", typeof(int)), ("DiagnosticCode", typeof(string)), ("Message", typeof(string))],
        [typeof(ThemeCompatibilityNoticeV2)] = [("Kind", typeof(ThemeCompatibilityNoticeKindV2)), ("Dimension", typeof(ThemeCompatibilityDimensionV2)), ("Sequence", typeof(int)), ("DiagnosticCode", typeof(string)), ("Message", typeof(string))],
        [typeof(ThemeCompatibilityRequestV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Evidence", typeof(ThemeCompatibilityEvidenceV2))],
        [typeof(ThemeCompatibilityContextV2)] = [("PackageRef", typeof(ThemePackageRef)), ("Integrity", typeof(ThemeIntegrityVerificationResultV2)), ("Manifest", typeof(ThemeManifest)), ("Compatibility", typeof(ThemeCompatibilityManifest)), ("CompatibilityManifestHash", typeof(string)), ("BindingFailures", typeof(ImmutableArray<ThemeCompatibilityFailureV2>))],
        [typeof(ThemeCompatibilityEvidenceV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Runtime", typeof(ThemeCompatibilityRuntimeEvidenceV2)), ("Capability", typeof(ThemeCompatibilityCapabilityEvidenceV2)), ("Accessibility", typeof(ThemeCompatibilityAccessibilityEvidenceV2)), ("Safety", typeof(ThemeCompatibilitySafetyEvidenceV2)), ("Migration", typeof(ThemeCompatibilityMigrationEvidenceV2)), ("Rollback", typeof(ThemeCompatibilityRollbackEvidenceV2))],
        [typeof(ThemeCompatibilityRuntimeEvidenceV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Status", typeof(ThemeCompatibilityEvidenceStatusV2)), ("Notices", typeof(ImmutableArray<ThemeCompatibilityNoticeV2>)), ("Failures", typeof(ImmutableArray<ThemeCompatibilityFailureV2>))],
        [typeof(ThemeCompatibilityCapabilityEvidenceV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Status", typeof(ThemeCompatibilityEvidenceStatusV2)), ("AuthorizedCapabilities", typeof(ImmutableArray<string>)), ("EnabledCapabilities", typeof(ImmutableArray<string>)), ("DisabledCapabilities", typeof(ImmutableArray<string>)), ("Notices", typeof(ImmutableArray<ThemeCompatibilityNoticeV2>)), ("Failures", typeof(ImmutableArray<ThemeCompatibilityFailureV2>))],
        [typeof(ThemeCompatibilityAccessibilityEvidenceV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Status", typeof(ThemeCompatibilityAccessibilityStatusV2)), ("Failures", typeof(ImmutableArray<ThemeCompatibilityFailureV2>))],
        [typeof(ThemeCompatibilitySafetyEvidenceV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Status", typeof(ThemeCompatibilitySafetyStatusV2)), ("Failures", typeof(ImmutableArray<ThemeCompatibilitySafetyStatusV2>)), ("AssetInventoryStatus", typeof(ThemeCompatibilityEvidenceStatusV2)), ("MotionSafetyStatus", typeof(ThemeCompatibilityEvidenceStatusV2)), ("AudioSafetyStatus", typeof(ThemeCompatibilityEvidenceStatusV2)), ("Details", typeof(ImmutableArray<ThemeCompatibilityFailureV2>))],
        [typeof(ThemeCompatibilityMigrationEvidenceV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Status", typeof(ThemeCompatibilityEvidenceStatusV2)), ("MigrationRequired", typeof(bool?)), ("MigrationReady", typeof(bool?)), ("Failures", typeof(ImmutableArray<ThemeCompatibilityFailureV2>))],
        [typeof(ThemeCompatibilityRollbackEvidenceV2)] = [("Context", typeof(ThemeCompatibilityContextV2)), ("Environment", typeof(ThemeCompatibilityEnvironmentV2)), ("Status", typeof(ThemeCompatibilityEvidenceStatusV2)), ("RollbackRequired", typeof(bool?)), ("RollbackAvailable", typeof(bool?)), ("Failures", typeof(ImmutableArray<ThemeCompatibilityFailureV2>))],
    };

    [Fact]
    public void ExactDataShapesConstructorsAndNullability()
    {
        NullabilityInfoContext nullability = new();
        foreach ((Type type, (string Name, Type Type)[] shape) in Shapes)
        {
            bool publicType = type.Namespace == "Tcc.Presentation.Contracts.Theme" || type == typeof(ThemeCompatibilityRequestV2)
                || type == typeof(ThemeCompatibilityContextV2) || type == typeof(ThemeCompatibilityEvidenceV2);
            Assert.Equal(publicType, type.IsPublic);
            Assert.True(type.IsSealed && !type.IsAbstract && !type.IsGenericType && !type.IsNested);
            PropertyInfo[] properties = type.GetProperties(Declared).Where(p => p.Name != "EqualityContract").ToArray();
            Assert.Equal(shape.Select(p => p.Name).Order(StringComparer.Ordinal), properties.Select(p => p.Name).Order(StringComparer.Ordinal));
            foreach ((string name, Type expected) in shape)
            {
                PropertyInfo property = Assert.Single(properties, p => p.Name == name);
                Assert.Equal(expected, property.PropertyType);
                Assert.Null(property.SetMethod);
                bool internalState = type.Namespace == "Tcc.Themes.Compatibility.V2" && type != typeof(ThemeCompatibilityRequestV2);
                Assert.Equal(internalState, property.GetMethod!.IsAssembly);
            }
            ConstructorInfo constructor = Assert.Single(type.GetConstructors(Declared), c => c.GetParameters().Length != 1 || c.GetParameters()[0].ParameterType != type);
            Type[] expectedParameters = shape.Select(p => ParameterType(p.Type)).ToArray();
            Assert.Equal(expectedParameters, constructor.GetParameters().Select(p => p.ParameterType));
            Assert.Equal(type.Namespace == "Tcc.Themes.Compatibility.V2" && type != typeof(ThemeCompatibilityRequestV2), constructor.IsAssembly);
        }
        foreach (string name in new[] { "SelectedCoreVersion", "SelectedThemeApiVersion", "SelectedUxContractVersion", "SelectedManifestSchemaVersion" })
            Assert.Equal(NullabilityState.Nullable, nullability.Create(typeof(ThemeCompatibilityResultV2).GetProperty(name)!).ReadState);
        foreach (PropertyInfo property in typeof(ThemeCompatibilityRequestV2).GetProperties())
            Assert.Equal(NullabilityState.Nullable, nullability.Create(property).ReadState);
        Assert.Equal(typeof(bool?), typeof(ThemeCompatibilityResultV2).GetProperty("MigrationRequired")!.PropertyType);
        Assert.Equal(typeof(bool?), typeof(ThemeCompatibilityResultV2).GetProperty("RollbackAvailable")!.PropertyType);
    }

    private static Type ParameterType(Type type)
    {
        Type? nullable = Nullable.GetUnderlyingType(type);
        if (nullable?.IsGenericType == true && nullable.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
            return typeof(IEnumerable<>).MakeGenericType(nullable.GetGenericArguments());
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>)
            ? typeof(IEnumerable<>).MakeGenericType(type.GetGenericArguments()) : type;
    }

    internal static IEnumerable<string> RuntimeShapeViolations(Type type)
    {
        if (!RuntimeTypeNames.Contains(type.FullName, StringComparer.Ordinal)) yield break;
        Type expected = typeof(ThemeCompatibilityContextV2).Assembly.GetType(type.FullName!)!;
        if (type.IsNested || type.IsGenericType || type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
            yield return "Candidate A runtime type has unauthorized nesting, generic, or generated shape.";
        if (expected.IsInterface)
        {
            MethodInfo[] methods = type.GetMethods(Declared);
            MethodInfo? resolve = methods.Length == 1 ? methods[0] : null;
            ParameterInfo[] parameters = resolve?.GetParameters() ?? [];
            if (!type.IsInterface || !type.IsPublic || type.GetInterfaces().Length != 0
                || type.GetFields(Declared).Length != 0 || type.GetProperties(Declared).Length != 0
                || type.GetEvents(Declared).Length != 0 || type.GetConstructors(Declared).Length != 0
                || type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Length != 0
                || resolve is null || resolve.Name != "Resolve" || !resolve.IsPublic || resolve.IsStatic
                || !resolve.IsAbstract || !resolve.IsVirtual || resolve.IsFinal
                || resolve.IsGenericMethod || resolve.IsGenericMethodDefinition || resolve.GetMethodBody() is not null
                || resolve.ReturnType != typeof(ThemeCompatibilityResultV2) || parameters.Length != 1
                || parameters[0].ParameterType.FullName != typeof(ThemeCompatibilityRequestV2).FullName
                || parameters[0].ParameterType.IsByRef || parameters[0].ParameterType.IsPointer
                || parameters[0].IsOut || parameters[0].IsOptional || parameters[0].HasDefaultValue
                || parameters[0].IsDefined(typeof(ParamArrayAttribute), inherit: false))
                yield return "Candidate A resolver interface shape is invalid.";
            yield break;
        }
        bool publicType = expected == typeof(ThemeCompatibilityRequestV2) || expected == typeof(ThemeCompatibilityContextV2)
            || expected == typeof(ThemeCompatibilityEvidenceV2);
        if (!type.IsClass || !type.IsSealed || type.IsAbstract || type.IsPublic != publicType || type.GetInterfaces().Length != 0)
            yield return "Candidate A runtime class shape is invalid.";
        (string Name, Type Type)[] shape = Shapes[expected];
        PropertyInfo[] properties = type.GetProperties(Declared);
        if (properties.Length != shape.Length) yield return "Candidate A property inventory is invalid.";
        foreach ((string name, Type dataType) in shape)
        {
            PropertyInfo? property = properties.SingleOrDefault(p => p.Name == name);
            if (property is null || property.PropertyType.FullName != dataType.FullName || property.SetMethod is not null
                || property.GetMethod!.IsStatic || property.GetMethod.IsPublic != (expected == typeof(ThemeCompatibilityRequestV2)))
                yield return "Candidate A get-only property shape is invalid.";
        }
        ConstructorInfo[] constructors = type.GetConstructors(Declared);
        if (constructors.Length != 1 || constructors[0].IsPublic != (expected == typeof(ThemeCompatibilityRequestV2))
            || (expected != typeof(ThemeCompatibilityRequestV2) && !constructors[0].IsAssembly)
            || !constructors[0].GetParameters().Select(p => p.ParameterType.FullName).SequenceEqual(shape.Select(p => ParameterType(p.Type).FullName)))
            yield return "Candidate A constructor boundary is invalid.";
        foreach (MethodInfo method in type.GetMethods(Declared))
            if (!method.IsSpecialName && !(expected == typeof(ThemeCompatibilityContextV2) && method.Name == "Snapshot"
                && method.IsPrivate && method.IsStatic && method.ReturnType == typeof(ThemeManifest)
                && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(ThemeManifest)))
                yield return "Candidate A has an unauthorized factory, clone, or method.";
        if (type.GetFields(Declared).Length != shape.Length || type.GetFields(Declared).Any(f => !f.IsPrivate || !f.IsInitOnly || f.IsStatic
            || !shape.Any(p => f.Name == $"<{p.Name}>k__BackingField" && f.FieldType.FullName == p.Type.FullName)))
            yield return "Candidate A has unauthorized mutable or hidden state.";
    }

    internal static IEnumerable<string> ResolverShapeViolations(Type type)
    {
        if (!type.GetInterfaces().Any(contract => string.Equals(
                contract.FullName,
                "Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2",
                StringComparison.Ordinal))
            && !string.Equals(type.FullName, ResolverTypeName, StringComparison.Ordinal))
        {
            yield break;
        }

        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        ConstructorInfo[] constructors = type.GetConstructors(declared);
        MethodInfo[] methods = type.GetMethods(declared);
        MethodInfo[] publicMethods = methods.Where(method => method.IsPublic).ToArray();
        MethodInfo? resolve = publicMethods.Length == 1 ? publicMethods[0] : null;
        ParameterInfo[] parameters = resolve?.GetParameters() ?? [];
        bool exactInterface = type.GetInterfaces().Length == 1
            && string.Equals(type.GetInterfaces()[0].FullName,
                "Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2", StringComparison.Ordinal);
        if (!string.Equals(type.FullName, ResolverTypeName, StringComparison.Ordinal)
            || !string.Equals(type.Assembly.GetName().Name, "Tcc.Themes", StringComparison.Ordinal)
            || !type.IsPublic || !type.IsClass || !type.IsSealed || type.IsAbstract
            || type.IsNested || type.IsGenericType || type.BaseType != typeof(object)
            || type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
            || !exactInterface
            || constructors.Length != 1 || !constructors[0].IsPublic || constructors[0].IsStatic
            || constructors[0].GetParameters().Length != 0
            || type.GetFields(declared).Length != 0 || type.GetProperties(declared).Length != 0
            || type.GetEvents(declared).Length != 0 || type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Length != 0
            || methods.Any(method => !method.IsPublic && !method.IsPrivate)
            || resolve is null || resolve.Name != "Resolve" || !resolve.IsPublic || resolve.IsStatic
            || resolve.IsAbstract || resolve.IsGenericMethod || resolve.ReturnType != typeof(ThemeCompatibilityResultV2)
            || parameters.Length != 1
            || parameters[0].ParameterType.FullName != typeof(ThemeCompatibilityRequestV2).FullName
            || parameters[0].ParameterType.IsByRef || parameters[0].ParameterType.IsPointer
            || parameters[0].IsOut || parameters[0].IsOptional || parameters[0].HasDefaultValue
            || parameters[0].IsDefined(typeof(ParamArrayAttribute), false))
        {
            yield return $"Illegal V2 resolver implementation shape or identity: {type.FullName}.";
        }
    }

    [Fact]
    public void ExactPublicAndInternalInventoriesAndAuthorizedImplementations()
    {
        Assembly runtime = typeof(ThemeCompatibilityContextV2).Assembly;
        string[] expectedRuntimeTypes = RuntimeTypeNames.Append(ResolverTypeName).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expectedRuntimeTypes, runtime.GetTypes().Where(t => t.Namespace == "Tcc.Themes.Compatibility.V2").Select(t => t.FullName).Order(StringComparer.Ordinal));
        foreach (Type type in runtime.GetTypes()) Assert.Empty(RuntimeShapeViolations(type));
        Assembly[] assemblies = [runtime, typeof(ContractVersions).Assembly, typeof(Tcc.Windows.AssemblyMarker).Assembly, typeof(Tcc.Features.Themes.AssemblyMarker).Assembly];
        Type[] productionTypes = assemblies.SelectMany(assembly => assembly.GetTypes()).ToArray();
        Assert.DoesNotContain(productionTypes, type => !type.IsInterface && type.GetInterfaces().Contains(typeof(IThemeCompatibilityResolver)));
        Type resolver = Assert.Single(productionTypes, type => !type.IsInterface
            && type.GetInterfaces().Contains(typeof(IThemeCompatibilityResolverV2)));
        Assert.Equal(ResolverTypeName, resolver.FullName);
        Assert.Empty(ResolverShapeViolations(resolver));
        string[] expected = Shapes.Keys.Where(t => t.Namespace == "Tcc.Presentation.Contracts.Theme").Select(t => t.Name)
            .Concat(EnumCases().Select(c => ((Type)c[0]).Name)).Append(nameof(ThemeCompatibilityJsonV2)).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expected, typeof(ContractVersions).Assembly.GetExportedTypes().Where(t => t.Name.StartsWith("ThemeCompatibility", StringComparison.Ordinal) && t.Name.EndsWith("V2", StringComparison.Ordinal)).Select(t => t.Name).Order(StringComparer.Ordinal));
        Assert.Equal(SerializerMethods, typeof(ThemeCompatibilityJsonV2).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(m => m.Name).Order(StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(EnumCases))]
    public void ExactEnumNamesValuesAndWireStrings(Type type, string names, string wires)
    {
        string[] expected = names.Split(' ');
        Assert.Equal(expected, Enum.GetNames(type));
        JsonSerializerOptions options = new();
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false));
        string[] values = wires.Split(' ');
        for (int i = 0; i < expected.Length; i++)
        {
            object value = Enum.Parse(type, expected[i]);
            Assert.Equal(i, Convert.ToInt32(value, CultureInfo.InvariantCulture));
            Assert.Equal(JsonSerializer.Serialize(values[i]), JsonSerializer.Serialize(value, type, options));
            Assert.Equal(value, JsonSerializer.Deserialize(JsonSerializer.Serialize(values[i]), type, options));
        }
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("0", type, options));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize("\"unknown\"", type, options));
    }

    public static IEnumerable<object[]> EnumCases()
    {
        yield return [typeof(ThemeCompatibilityEvaluationStateV2), "NotEvaluated RefusedPrecondition Evaluated", "not_evaluated refused_precondition evaluated"];
        yield return [typeof(ThemeCompatibilityStatusV2), "NotEvaluated RefusedPrecondition TrustedEvidenceFailure UnsupportedManifestSchema ManifestSchemaInvalid InvalidVersionInput UnsatisfiableVersionRange ConflictingVersionDeclaration IncompatibleCoreVersion IncompatibleThemeApiVersion IncompatibleUxContractVersion UnsupportedPlatform UnsupportedInstallationMode RuntimeValidationFailed AccessibilityValidationFailed SafetyValidationFailed BlockedCapability MigrationNotReady RollbackUnavailable EvidenceUnavailable Compatible CompatibleWithDegradation CompatibleUntestedDpiWithScalableFallback", "not_evaluated refused_precondition trusted_evidence_failure unsupported_manifest_schema manifest_schema_invalid invalid_version_input unsatisfiable_version_range conflicting_version_declaration incompatible_core_version incompatible_theme_api_version incompatible_ux_contract_version unsupported_platform unsupported_installation_mode runtime_validation_failed accessibility_validation_failed safety_validation_failed blocked_capability migration_not_ready rollback_unavailable evidence_unavailable compatible compatible_with_degradation compatible_untested_dpi_with_scalable_fallback"];
        yield return [typeof(ThemeCompatibilityDimensionV2), "Precondition Integrity ManifestSchema VersionDeclarations CoreVersion ThemeApiVersion UxContractVersion PlatformMode Runtime Accessibility Safety Capability Migration Rollback", "precondition integrity manifest_schema version_declarations core_version theme_api_version ux_contract_version platform_mode runtime accessibility safety capability migration rollback"];
        yield return [typeof(ThemeCompatibilityFailureKindV2), "MissingTrustedContext InvalidRuntimeContext ContextBindingMismatch ContentSnapshotUnavailable IntegrityNotVerified ContentEvidenceMismatch UnsupportedManifestSchema ManifestSchemaInvalid InvalidSchemaSupportInput InvalidVersionInput UnsatisfiableVersionRange ConflictingVersionDeclaration CoreVersionIncompatible ThemeApiNoCompatibleVersion UxContractNoCompatibleVersion PlatformUnsupported InstallationModeUnsupported RuntimeValidationFailed AccessibilityValidationFailed SafetyValidationFailed AssetInventoryInvalid MotionSafetyFailed AudioSafetyFailed CapabilityBlocked MigrationNotReady RollbackUnavailable EvidenceMissing EvidenceNotEvaluated EvidenceScopeMismatch EvidenceMalformed", "missing_trusted_context invalid_runtime_context context_binding_mismatch content_snapshot_unavailable integrity_not_verified content_evidence_mismatch unsupported_manifest_schema manifest_schema_invalid invalid_schema_support_input invalid_version_input unsatisfiable_version_range conflicting_version_declaration core_version_incompatible theme_api_no_compatible_version ux_contract_no_compatible_version platform_unsupported installation_mode_unsupported runtime_validation_failed accessibility_validation_failed safety_validation_failed asset_inventory_invalid motion_safety_failed audio_safety_failed capability_blocked migration_not_ready rollback_unavailable evidence_missing evidence_not_evaluated evidence_scope_mismatch evidence_malformed"];
        yield return [typeof(ThemeCompatibilityEvidenceStatusV2), "NotEvaluated Passed Failed NotApplicable", "not_evaluated passed failed not_applicable"];
        yield return [typeof(ThemeCompatibilityAccessibilityStatusV2), "NotEvaluated Validated Failed", "not_evaluated validated failed"];
        yield return [typeof(ThemeCompatibilitySafetyStatusV2), "NotEvaluated Passed FailedHomeSafetyCore FailedCriticalAlerts FailedRiskPermissionClarity FailedConfirmationSemantics FailedAccessibility", "not_evaluated passed failed_home_safety_core failed_critical_alerts failed_risk_permission_clarity failed_confirmation_semantics failed_accessibility"];
        yield return [typeof(ThemeCompatibilityInstallationModeV2), "NotSpecified Installer Portable", "not_specified installer portable"];
        yield return [typeof(ThemeCompatibilityOperationV2), "NotSpecified PackageEvaluation TransitionEvaluation", "not_specified package_evaluation transition_evaluation"];
        yield return [typeof(ThemeCompatibilityNoticeKindV2), "NotSpecified OptionalCapabilityDisabled DecorativePresentationDegradation UntestedDpiScalableFallback", "not_specified optional_capability_disabled decorative_presentation_degradation untested_dpi_scalable_fallback"];
    }

    private static ThemeCompatibilityContextV2 Context() => new(new ThemePackageRef("logical"), null, null, null, null, []);
    private static ThemeAccessibilityProfile Profile() => new("standard", 1m, 1m, false, null, false, false, false, false, false);
    private static ThemeCompatibilityEnvironmentV2 Environment(IEnumerable<string?>? support = null,
        IEnumerable<ThemeCompatibilityRuntimeTargetV2>? targets = null,
        ThemeCompatibilityOperationV2 operation = ThemeCompatibilityOperationV2.PackageEvaluation,
        ThemeId? current = null, ThemeVersion? version = null, string? revision = null) =>
        new("1.5.0", support ?? ImmutableArray.Create<string?>("1.0.0"), ["1.1.0"], ["1.0"], "windows", "x64",
            ThemeCompatibilityInstallationModeV2.Installer, "build", "profile", targets ?? [new("main", new("deep"), new("home"), 1m, 800m, 600m, Profile())], operation, current, version, revision);

    [Fact]
    public void EnvironmentSnapshotsAndTargetInvariants()
    {
        List<string?> support = ["1.0.0", null, "invalid"];
        List<ThemeCompatibilityRuntimeTargetV2> targets = [new("main", new("deep"), new("home"), 1, 800, 600, Profile())];
        ThemeCompatibilityEnvironmentV2 value = Environment(support, targets);
        support.Clear(); targets.Clear();
        Assert.Equal(new string?[] { "1.0.0", null, "invalid" }, value.SupportedThemeApiVersions!.Value);
        Assert.Single(value.RuntimeTargets);
        Assert.Throws<ArgumentException>(() => Environment(targets: []));
        Assert.Throws<ArgumentException>(() => Environment(targets: [value.RuntimeTargets[0], value.RuntimeTargets[0]]));
        Assert.Throws<ArgumentException>(() => new ThemeCompatibilityRuntimeTargetV2("main", new("deep"), new("home"), 0, 1, 1, Profile()));
        Assert.Throws<ArgumentException>(() => new ThemeCompatibilityRuntimeTargetV2("main", new("deep"), new("home"), 1, 1, 1, Profile() with { Zoom = 0 }));
        Assert.Throws<ArgumentException>(() => Environment(current: new("source")));
        Assert.Throws<ArgumentException>(() => Environment(current: new("source"), version: new("1.0.0")));
        Assert.Throws<ArgumentException>(() => Environment(revision: "state"));
        Assert.Null(Environment(operation: ThemeCompatibilityOperationV2.TransitionEvaluation).CurrentThemeId);
    }

    [Fact]
    public void EvidenceAbsenceIsPreservedAndNullParentsRejected()
    {
        ThemeCompatibilityEvidenceV2 value = new(Context(), Environment(), null, null, null, null, null, null);
        foreach (PropertyInfo p in typeof(ThemeCompatibilityEvidenceV2).GetProperties(Declared).Where(p => p.Name is not "Context" and not "Environment")) Assert.Null(p.GetValue(value));
        Assert.Throws<ArgumentNullException>(() => new ThemeCompatibilityEvidenceV2(null!, Environment(), null, null, null, null, null, null));
        Assert.Throws<ArgumentNullException>(() => new ThemeCompatibilityEvidenceV2(Context(), null!, null, null, null, null, null, null));
    }

    [Theory]
    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, false)]
    [InlineData(3, false)] [InlineData(4, false)] [InlineData(5, false)]
    [InlineData(0, true)] [InlineData(1, true)] [InlineData(2, true)]
    [InlineData(3, true)] [InlineData(4, true)] [InlineData(5, true)]
    public void EveryReceiptRejectsContextAndEnvironmentMismatch(int slot, bool differentEnvironment)
    {
        ThemeCompatibilityContextV2 context = Context(); ThemeCompatibilityEnvironmentV2 environment = Environment();
        object?[] arguments = [context, environment, null, null, null, null, null, null];
        Type receipt = typeof(ThemeCompatibilityEvidenceV2).GetConstructors(Declared)[0].GetParameters()[slot + 2].ParameterType;
        ConstructorInfo receiptConstructor = receipt.GetConstructors(Declared)[0];
        object?[] receiptArguments = receiptConstructor.GetParameters().Select(p => Empty(p.ParameterType)).ToArray();
        receiptArguments[0] = differentEnvironment ? context : Context();
        receiptArguments[1] = differentEnvironment ? Environment() : environment;
        arguments[slot + 2] = receiptConstructor.Invoke(receiptArguments);
        ConstructorInfo bundle = typeof(ThemeCompatibilityEvidenceV2).GetConstructors(Declared)[0];
        Assert.IsType<ArgumentException>(Assert.Throws<TargetInvocationException>(() => bundle.Invoke(arguments)).InnerException);
        receiptArguments[0] = context; receiptArguments[1] = environment;
        arguments[slot + 2] = receiptConstructor.Invoke(receiptArguments);
        Assert.IsType<ThemeCompatibilityEvidenceV2>(bundle.Invoke(arguments));
    }

    private static object? Empty(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return Array.CreateInstance(type.GetGenericArguments()[0], 0);
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static ThemeCompatibilityNoticeV2 Notice(ThemeCompatibilityNoticeKindV2 kind, int sequence = 0) =>
        new(kind, kind == ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled ? ThemeCompatibilityDimensionV2.Capability : ThemeCompatibilityDimensionV2.Runtime,
            sequence, null, $"{kind} in {(kind == ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled ? ThemeCompatibilityDimensionV2.Capability : ThemeCompatibilityDimensionV2.Runtime)}.");

    [Theory]
    [InlineData(0)] [InlineData(4)] [InlineData(-1)]
    public void InvalidNoticeKindsRejected(int kind) => Assert.Throws<ArgumentException>(() => Notice((ThemeCompatibilityNoticeKindV2)kind));

    [Fact]
    public void NoticeConstructorRejectsInvalidDataAndMessages()
    {
        Assert.Throws<ArgumentException>(() => Notice(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled, -1));
        Assert.Throws<ArgumentException>(() => new ThemeCompatibilityNoticeV2(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled, ThemeCompatibilityDimensionV2.Runtime, 0, null, "OptionalCapabilityDisabled in Runtime."));
        foreach (string? text in new string?[] { null, "", " ", "raw caller text", "{\"secret\":true}", "System.Exception: failure" })
            Assert.Throws<ArgumentException>(() => new ThemeCompatibilityNoticeV2(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled, ThemeCompatibilityDimensionV2.Capability, 0, null, text!));
        Assert.Throws<ArgumentException>(() => new ThemeCompatibilityNoticeV2(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled, ThemeCompatibilityDimensionV2.Capability, 0, "P4I001", "OptionalCapabilityDisabled in Capability."));
    }

    private static ThemeCompatibilityResultV2 Result(bool success = false, ThemeCompatibilityStatusV2 status = ThemeCompatibilityStatusV2.Compatible,
        IEnumerable<ThemeCompatibilityNoticeV2>? notices = null) => new(
        "2.0", success ? status : ThemeCompatibilityStatusV2.NotEvaluated,
        success ? ThemeCompatibilityEvaluationStateV2.Evaluated : ThemeCompatibilityEvaluationStateV2.NotEvaluated,
        success ? new ThemeId("theme") : null, success ? new ThemeVersion("1.0.0") : null, success ? new string('a', 64) : null,
        success ? "1.5.0" : null, success ? "1.0.0" : null, success ? "1.1.0" : null, success ? "1.0" : null,
        success ? ThemeCompatibilityEvidenceStatusV2.Passed : ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
        success ? ThemeCompatibilityEvidenceStatusV2.Passed : ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
        [], [], [], notices ?? [], [],
        success ? ThemeCompatibilityAccessibilityStatusV2.Validated : ThemeCompatibilityAccessibilityStatusV2.NotEvaluated,
        success ? ThemeCompatibilitySafetyStatusV2.Passed : ThemeCompatibilitySafetyStatusV2.NotEvaluated, [],
        success ? ThemeCompatibilityEvidenceStatusV2.Passed : ThemeCompatibilityEvidenceStatusV2.NotEvaluated, success ? false : null, null,
        success ? ThemeCompatibilityEvidenceStatusV2.Passed : ThemeCompatibilityEvidenceStatusV2.NotEvaluated, success ? false : null, null);

    private static JsonObject Json(bool success = false) => JsonNode.Parse(ThemeCompatibilityJsonV2.SerializeResult(Result(success)))!.AsObject();

    [Theory]
    [InlineData("invalid_version_input", "version_declarations", "invalid_version_input")]
    [InlineData("unsatisfiable_version_range", "version_declarations", "unsatisfiable_version_range")]
    [InlineData("conflicting_version_declaration", "version_declarations", "conflicting_version_declaration")]
    [InlineData("core_version_incompatible", "core_version", "incompatible_core_version")]
    [InlineData("theme_api_no_compatible_version", "theme_api_version", "incompatible_theme_api_version")]
    [InlineData("ux_contract_no_compatible_version", "ux_contract_version", "incompatible_ux_contract_version")]
    [InlineData("platform_unsupported", "platform_mode", "unsupported_platform")]
    [InlineData("installation_mode_unsupported", "platform_mode", "unsupported_installation_mode")]
    [InlineData("runtime_validation_failed", "runtime", "runtime_validation_failed")]
    [InlineData("accessibility_validation_failed", "accessibility", "accessibility_validation_failed")]
    [InlineData("safety_validation_failed", "safety", "safety_validation_failed")]
    [InlineData("capability_blocked", "capability", "blocked_capability")]
    [InlineData("migration_not_ready", "migration", "migration_not_ready")]
    [InlineData("rollback_unavailable", "rollback", "rollback_unavailable")]
    public void EvaluatedFailuresRoundTripWithTruthfulFacts(string kind, string dimension, string status)
    {
        JsonObject node = Json(true);
        node["status"] = status;
        node["failures"] = new JsonArray(FailureJson(kind, dimension));
        if (dimension is "version_declarations" or "core_version") node["selected_core_version"] = null;
        if (dimension == "theme_api_version") node["selected_theme_api_version"] = null;
        if (dimension == "ux_contract_version") node["selected_ux_contract_version"] = null;
        if (dimension == "runtime") node["runtime_validation_status"] = "failed";
        if (dimension == "capability") node["capability_evidence_status"] = "failed";
        if (dimension == "accessibility") node["accessibility_validation_status"] = "failed";
        if (dimension == "safety")
        {
            node["safety_invariants_status"] = "failed_home_safety_core";
            node["safety_failures"] = new JsonArray("failed_home_safety_core");
        }
        if (dimension is "migration" or "rollback")
        {
            node[dimension + "_evidence_status"] = "failed";
            node[dimension + "_required"] = true;
            node[dimension == "migration" ? "migration_ready" : "rollback_available"] = false;
        }
        ThemeCompatibilityResultV2 result = ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString());
        Assert.Equal(status, JsonNode.Parse(ThemeCompatibilityJsonV2.SerializeResult(result))!["status"]!.GetValue<string>());
        Assert.Single(result.Failures);
        node["status"] = "compatible";
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
    }

    private static JsonObject FailureJson(string kind, string dimension, int sequence = 0)
    {
        string kindName = Enum.GetNames<ThemeCompatibilityFailureKindV2>().Single(n => JsonNamingPolicy.SnakeCaseLower.ConvertName(n) == kind);
        string dimensionName = Enum.GetNames<ThemeCompatibilityDimensionV2>().Single(n => JsonNamingPolicy.SnakeCaseLower.ConvertName(n) == dimension);
        return new JsonObject { ["kind"] = kind, ["dimension"] = dimension, ["sequence"] = sequence,
            ["diagnostic_code"] = null, ["message"] = $"{kindName} in {dimensionName}." };
    }

    [Theory]
    [InlineData("missing_trusted_context", "precondition", "refused_precondition", "refused_precondition")]
    [InlineData("integrity_not_verified", "integrity", "trusted_evidence_failure", "refused_precondition")]
    [InlineData("unsupported_manifest_schema", "manifest_schema", "unsupported_manifest_schema", "evaluated")]
    [InlineData("manifest_schema_invalid", "manifest_schema", "manifest_schema_invalid", "evaluated")]
    [InlineData("invalid_schema_support_input", "manifest_schema", "refused_precondition", "refused_precondition")]
    public void RefusalAndSchemaStatesHaveNoSelectedVersions(string kind, string dimension, string status, string evaluation)
    {
        JsonObject node = Json(); node["status"] = status; node["evaluation_state"] = evaluation;
        node["failures"] = new JsonArray(FailureJson(kind, dimension));
        ThemeCompatibilityResultV2 result = ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString());
        Assert.Null(result.SelectedManifestSchemaVersion);
        node["selected_core_version"] = "1.5.0";
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
    }

    [Fact]
    public void StoppedStagesCannotImportFailuresFromLaterStages()
    {
        JsonObject node = Json(); node["status"] = "refused_precondition"; node["evaluation_state"] = "refused_precondition";
        node["failures"] = new JsonArray(FailureJson("missing_trusted_context", "precondition"),
            FailureJson("integrity_not_verified", "integrity", 1));
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
        node["status"] = "trusted_evidence_failure";
        node["failures"] = new JsonArray(FailureJson("integrity_not_verified", "integrity"),
            FailureJson("manifest_schema_invalid", "manifest_schema", 1));
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
    }

    [Theory]
    [InlineData("runtime", "runtime_validation_status")]
    [InlineData("capability", "capability_evidence_status")]
    [InlineData("accessibility", "accessibility_validation_status")]
    [InlineData("safety", "safety_invariants_status")]
    [InlineData("migration", "migration_evidence_status")]
    [InlineData("rollback", "rollback_evidence_status")]
    public void MissingOwnerEvidenceRemainsNotEvaluated(string dimension, string field)
    {
        JsonObject node = Json(true); node["status"] = "evidence_unavailable"; node[field] = "not_evaluated";
        node["failures"] = new JsonArray(FailureJson("evidence_missing", dimension));
        if (dimension is "migration" or "rollback") node[dimension + "_required"] = null;
        Assert.Single(ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()).Failures);
        node["status"] = "compatible";
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
    }

    [Fact]
    public void MigrationAndRollbackTruthStatesRemainDistinct()
    {
        foreach (bool migration in new[] { false, true })
        foreach (bool required in new[] { false, true })
        {
            JsonObject node = Json(true); string prefix = migration ? "migration" : "rollback";
            node[prefix + "_required"] = required;
            node[migration ? "migration_ready" : "rollback_available"] = required ? true : null;
            ThemeCompatibilityResultV2 value = ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString());
            Assert.Equal(required, migration ? value.MigrationRequired : value.RollbackRequired);
            if (required)
            {
                node[migration ? "migration_ready" : "rollback_available"] = null;
                Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
            }
        }
        JsonObject optional = Json(true); optional["rollback_available"] = false;
        Assert.False(ThemeCompatibilityJsonV2.DeserializeResult(optional.ToJsonString()).RollbackAvailable);
        optional["rollback_available"] = true;
        Assert.True(ThemeCompatibilityJsonV2.DeserializeResult(optional.ToJsonString()).RollbackAvailable);
    }

    [Fact]
    public void IndependentFailureAndNoticeSequencesPreserveOccurrences()
    {
        JsonObject node = Json(true); node["status"] = "invalid_version_input"; node["selected_core_version"] = null;
        node["failures"] = new JsonArray(FailureJson("invalid_version_input", "version_declarations"), FailureJson("invalid_version_input", "version_declarations", 1));
        JsonObject noticeResult = JsonNode.Parse(ThemeCompatibilityJsonV2.SerializeResult(Result(true,
            ThemeCompatibilityStatusV2.CompatibleWithDegradation, [Notice(ThemeCompatibilityNoticeKindV2.DecorativePresentationDegradation)])))!.AsObject();
        node["notices"] = noticeResult["notices"]!.DeepClone();
        ThemeCompatibilityResultV2 result = ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString());
        Assert.Equal(2, result.Failures.Length); Assert.Single(result.Notices);
        Assert.Equal(0, result.Notices[0].Sequence); Assert.Equal(1, result.Failures[1].Sequence);
        node["failures"]![1]!["sequence"] = 0;
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
    }

    [Fact]
    public void ContextAndReceiptCollectionsAreImmutableSnapshotsOnly()
    {
        List<string> tested = ["1.0.0"]; Dictionary<string, string> matrix = new(StringComparer.Ordinal) { ["standard"] = "required" };
        ThemeCompatibilityManifest manifest = new("1.0", "theme", "1.0.0", new("1.0.0", tested), new("1.0.0", tested), new("1.1.0", tested), new(true, true, true, tested), matrix);
        List<ThemeIntegrityFileEvidenceV1> files = [];
        ThemeIntegrityVerificationResultV2 integrity = new(false, null, null, null, files, files, ThemeSignatureVerificationStatus.NotEvaluated, null, null, []);
        List<ThemeCompatibilityFailureV2> failures = [new(ThemeCompatibilityFailureKindV2.IntegrityNotVerified, ThemeCompatibilityDimensionV2.Integrity, 0, null, "IntegrityNotVerified in Integrity.")];
        ThemeCompatibilityContextV2 context = new(new("logical"), integrity, null, manifest, null, failures);
        tested.Clear(); matrix.Clear(); failures.Clear();
        Assert.Single(context.Compatibility!.Core.Tested); Assert.Single(context.Compatibility.AccessibilityMatrix);
        Assert.Single(context.BindingFailures); Assert.IsType<ImmutableArray<ThemeIntegrityFileEvidenceV1>>(context.Integrity!.FileEvidence);
        List<ThemeCompatibilityNoticeV2> notices = [Notice(ThemeCompatibilityNoticeKindV2.DecorativePresentationDegradation)];
        ThemeCompatibilityRuntimeEvidenceV2 receipt = new(context, Environment(), ThemeCompatibilityEvidenceStatusV2.NotEvaluated, notices, []);
        notices.Clear(); Assert.Single(receipt.Notices);
        Assert.False(context.Integrity.IsVerified); // Contract fixture makes no runtime provenance claim.
    }

    [Theory]
    [InlineData("public sealed class ThemeCompatibilityContextV2 { public ThemeCompatibilityContextV2() {} }", "constructor boundary")]
    [InlineData("internal sealed class ThemeCompatibilityContextV2 { internal ThemeCompatibilityContextV2() {} }", "class shape")]
    [InlineData("public sealed class ThemeCompatibilityContextV2 { public System.IO.FileInfo Probe() => null!; }", "System.IO.FileInfo")]
    public void CandidateAShapeAndDependencyAttacksAreCompiledAndRejected(string declaration, string finding)
    {
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(null, "namespace Tcc.Themes.Compatibility.V2 { " + declaration + " }", includeCandidateAContracts: false);
        Assert.Contains(PhaseThreeScopeBoundaryTests.GetCompiledSurfaceViolations(fixture), v => v.Contains(finding, StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(ResolverInterfaceExactnessMutants))]
    public void ResolverInterfaceExactnessMutantsCompileAndAreRejected(string caseName, string members)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(null, ResolverInterfaceSource(members), includeCandidateAContracts: false);
        Type resolver = fixture.GetType("Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2", throwOnError: true)!;
        Assert.Contains(RuntimeShapeViolations(resolver), violation => violation.Contains("resolver interface shape", StringComparison.Ordinal));
        Assert.Contains(PhaseThreeScopeBoundaryTests.GetCompiledSurfaceViolations(fixture), violation => violation.Contains("resolver interface shape", StringComparison.Ordinal));
    }

    public static IEnumerable<object[]> ResolverInterfaceExactnessMutants()
    {
        const string approved = "ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request);";
        yield return ["mutable static field", "public static int AuditState; " + approved];
        yield return ["static readonly field", "public static readonly int AuditState; " + approved];
        yield return ["static Resolve with body", "public static ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!;"];
        yield return ["default interface instance Resolve body", "public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!;"];
        yield return ["extra instance method", approved + " void Audit();"];
        yield return ["extra static method", approved + " public static void Audit() { }"];
        yield return ["generic Resolve", "ThemeCompatibilityResultV2 Resolve<T>(ThemeCompatibilityRequestV2 request);"];
        yield return ["Resolve overload", approved + " ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request, int mode);"];
        yield return ["extra property", approved + " int Audit { get; }"];
        yield return ["extra event", approved + " event System.Action? Audit;"];
        yield return ["nested interface", approved + " interface Audit { }"];
    }

    [Theory]
    [InlineData("P5B-IV-001", "public static int AuditState; ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request);")]
    [InlineData("P5B-IV-002", "ThemeCompatibilityResultV2 Resolve<T>(ThemeCompatibilityRequestV2 request);")]
    public void OriginalIndependentValidationDefectsCompileAndAreRejected(string defect, string members)
    {
        Assert.StartsWith("P5B-IV-00", defect, StringComparison.Ordinal);
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(null, ResolverInterfaceSource(members), includeCandidateAContracts: false);
        Type resolver = fixture.GetType("Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2", throwOnError: true)!;
        Assert.Contains(RuntimeShapeViolations(resolver), violation => violation.Contains("resolver interface shape", StringComparison.Ordinal));
    }

    [Fact]
    public void FreshApprovedResolverInterfaceCompilesAndIsAccepted()
    {
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(null,
            ResolverInterfaceSource("ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request);"), includeCandidateAContracts: false);
        Type resolver = fixture.GetType("Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2", throwOnError: true)!;
        Assert.Empty(RuntimeShapeViolations(resolver));
        Assert.DoesNotContain(PhaseThreeScopeBoundaryTests.GetCompiledSurfaceViolations(fixture),
            violation => violation.Contains("resolver interface shape", StringComparison.Ordinal));
    }

    private static string ResolverInterfaceSource(string members) =>
        "using Tcc.Presentation.Contracts.Theme; namespace Tcc.Themes.Compatibility.V2 { "
        + "public sealed class ThemeCompatibilityRequestV2 { } public interface IThemeCompatibilityResolverV2 { "
        + members + " } }";

    [Theory]
    [InlineData("public abstract class Alternate : IThemeCompatibilityResolverV2 { public abstract ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request); }")]
    [InlineData("public sealed class Alternate<T> : IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; }")]
    public void CandidateBRejectsAbstractAndGenericV2Implementations(string declaration)
    {
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(null,
            "using Tcc.Presentation.Contracts.Theme; namespace Tcc.Themes.Compatibility.V2 { "
            + "public sealed class ThemeCompatibilityRequestV2 {} public interface IThemeCompatibilityResolverV2 { ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request); } "
            + declaration + " }", includeCandidateAContracts: false);
        Assert.Contains(PhaseThreeScopeBoundaryTests.GetCompiledSurfaceViolations(fixture),
            v => v.Contains("Illegal V2 resolver implementation shape or identity", StringComparison.Ordinal));
    }

    [Fact]
    public void NullableFactsRoundTripAndWireOrderAreStable()
    {
        string json = ThemeCompatibilityJsonV2.SerializeResult(Result());
        ThemeCompatibilityResultV2 roundtrip = ThemeCompatibilityJsonV2.DeserializeResult(json);
        Assert.Equal(json, ThemeCompatibilityJsonV2.SerializeResult(roundtrip));
        Assert.Equal(ThemeCompatibilityStatusV2.NotEvaluated, default);
        Assert.Null(roundtrip.SelectedCoreVersion); Assert.Null(roundtrip.SelectedThemeApiVersion);
        Assert.Null(roundtrip.SelectedUxContractVersion); Assert.Null(roundtrip.SelectedManifestSchemaVersion);
        Assert.Null(roundtrip.MigrationRequired); Assert.Null(roundtrip.MigrationReady);
        Assert.Null(roundtrip.RollbackRequired); Assert.Null(roundtrip.RollbackAvailable);
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal(Shapes[typeof(ThemeCompatibilityResultV2)].Select(p => JsonNamingPolicy.SnakeCaseLower.ConvertName(p.Name)), document.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.Contains("\"selected_core_version\":null", json, StringComparison.Ordinal);
        Assert.Equal("2.0", roundtrip.ContractVersion);
    }

    [Theory]
    [InlineData(ThemeCompatibilityStatusV2.CompatibleWithDegradation, ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled)]
    [InlineData(ThemeCompatibilityStatusV2.CompatibleWithDegradation, ThemeCompatibilityNoticeKindV2.DecorativePresentationDegradation)]
    [InlineData(ThemeCompatibilityStatusV2.CompatibleUntestedDpiWithScalableFallback, ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback)]
    public void NoticeSuccessRoundTrip(ThemeCompatibilityStatusV2 status, ThemeCompatibilityNoticeKindV2 kind)
    {
        string json = ThemeCompatibilityJsonV2.SerializeResult(Result(true, status, [Notice(kind)]));
        Assert.Equal(json, ThemeCompatibilityJsonV2.SerializeResult(ThemeCompatibilityJsonV2.DeserializeResult(json)));
        using JsonDocument doc = JsonDocument.Parse(json);
        Assert.Equal(DetailWireProperties, doc.RootElement.GetProperty("notices")[0].EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public void NoticeSequencesAndSuccessPrecedenceAreEnforced()
    {
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.SerializeResult(Result(true, notices: [Notice(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled)])));
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.SerializeResult(Result(true, ThemeCompatibilityStatusV2.CompatibleWithDegradation)));
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.SerializeResult(Result(true, ThemeCompatibilityStatusV2.CompatibleWithDegradation, [Notice(ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback)])));
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.SerializeResult(Result(true, ThemeCompatibilityStatusV2.CompatibleUntestedDpiWithScalableFallback, [Notice(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled)])));
        List<ThemeCompatibilityNoticeV2> notices = [Notice(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled), Notice(ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback, 1)];
        ThemeCompatibilityResultV2 result = Result(true, ThemeCompatibilityStatusV2.CompatibleUntestedDpiWithScalableFallback, notices);
        notices.Clear(); Assert.Equal(2, result.Notices.Length);
        string json = ThemeCompatibilityJsonV2.SerializeResult(result);
        foreach (int sequence in new[] { -1, 0, 2 })
        {
            JsonObject node = JsonNode.Parse(json)!.AsObject(); node["notices"]![1]!["sequence"] = sequence;
            Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
        }
    }

    [Theory]
    [MemberData(nameof(InvalidResultFields))]
    public void InvalidResultStateIsRejected(string field, string value)
    {
        JsonObject node = Json(true); node[field] = JsonNode.Parse(value);
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
    }

    public static IEnumerable<object[]> InvalidResultFields()
    {
        yield return ["extra", "true"];
        yield return ["contract_version", "\"1.0\""];
        yield return ["status", "20"];
        yield return ["status", "\"unknown\""];
        yield return ["status", "\"Compatible\""];
        yield return ["status", "\"compatible_with_degradation\""];
        yield return ["evaluation_state", "\"not_evaluated\""];
        yield return ["runtime_validation_status", "\"failed\""];
        yield return ["capability_evidence_status", "\"not_evaluated\""];
        yield return ["accessibility_validation_status", "\"failed\""];
        yield return ["safety_invariants_status", "\"not_evaluated\""];
        yield return ["migration_required", "null"];
        yield return ["migration_required", "true"];
        yield return ["migration_ready", "false"];
        yield return ["rollback_required", "true"];
        yield return ["theme_id", "null"];
        yield return ["package_hash", "\"unknown\""];
        yield return ["enabled_capabilities", "[\"unapproved\"]"];
        foreach (string name in new[] { "selected_core_version", "selected_theme_api_version", "selected_ux_contract_version", "selected_manifest_schema_version" })
            foreach (string token in new[] { "", "unknown", "v1.0", "01.0.0", "1.0.0-alpha", "１.0.0", " 1.0.0" })
                yield return [name, JsonSerializer.Serialize(token)];
    }

    [Fact]
    public void MissingDuplicateAndMalformedNestedFieldsAreRejected()
    {
        JsonObject original = Json();
        foreach (string key in original.Select(p => p.Key).ToArray())
        {
            JsonObject missing = original.DeepClone().AsObject(); missing.Remove(key);
            Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(missing.ToJsonString()));
            string duplicated = original.ToJsonString().Insert(1, JsonSerializer.Serialize(key) + ":null,");
            Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(duplicated));
        }
        string baseline = ThemeCompatibilityJsonV2.SerializeResult(Result(true, ThemeCompatibilityStatusV2.CompatibleWithDegradation, [Notice(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled)]));
        foreach (string key in new[] { "kind", "dimension", "sequence", "diagnostic_code", "message" })
        {
            JsonObject node = JsonNode.Parse(baseline)!.AsObject(); node["notices"]![0]!.AsObject().Remove(key);
            Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
        }
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(baseline.Replace("\"kind\":", "\"kind\":null,\"kind\":", StringComparison.Ordinal)));
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(baseline.Replace("optional_capability_disabled", "not_specified", StringComparison.Ordinal)));
    }

    [Fact]
    public void FailureRoundTripDoesNotInventVersionsOrHideOccurrences()
    {
        JsonObject node = Json(); node["status"] = "refused_precondition"; node["evaluation_state"] = "refused_precondition";
        node["failures"] = JsonNode.Parse("""[{"kind":"missing_trusted_context","dimension":"precondition","sequence":0,"diagnostic_code":null,"message":"MissingTrustedContext in Precondition."}]""");
        string valid = node.ToJsonString();
        ThemeCompatibilityResultV2 value = ThemeCompatibilityJsonV2.DeserializeResult(valid);
        Assert.Null(value.SelectedCoreVersion); Assert.Single(value.Failures);
        node["failures"]![0]!["sequence"] = 1;
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
        node = JsonNode.Parse(valid)!.AsObject(); node["failures"]![0]!["kind"] = 0;
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
        node = JsonNode.Parse(valid)!.AsObject(); node["failures"]![0]!["diagnostic_code"] = "P3M001";
        Assert.Throws<JsonException>(() => ThemeCompatibilityJsonV2.DeserializeResult(node.ToJsonString()));
    }

    [Fact]
    public async Task SerializationIsCultureIndependentAndConcurrent()
    {
        ThemeCompatibilityResultV2 result = Result(true);
        string expected = ThemeCompatibilityJsonV2.SerializeResult(result);
        Task<string>[] calls = Enumerable.Range(0, 60).Select(i => Task.Run(() =>
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(new[] { "en-US", "tr-TR", "zh-TW" }[i % 3]);
            return ThemeCompatibilityJsonV2.SerializeResult(ThemeCompatibilityJsonV2.DeserializeResult(expected));
        })).ToArray();
        Assert.All(await Task.WhenAll(calls), actual => Assert.Equal(expected, actual));
    }

    [Fact]
    public void JsonCannotReconstructTrustObjects()
    {
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<ThemeCompatibilityContextV2>("{}"));
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<ThemeCompatibilityEvidenceV2>("{}"));
        Assert.Empty(typeof(ThemeCompatibilityContextV2).GetConstructors());
        Assert.Empty(typeof(ThemeCompatibilityEvidenceV2).GetConstructors());
    }

    [Theory]
    [InlineData("public class Attack : ThemeCompatibilityContextV2 { }", "CS0509")]
    [InlineData("public class Attack { object Run() => new ThemeCompatibilityContextV2(); }", "CS1729")]
    [InlineData("public class Attack { object Run() => new ThemeCompatibilityEvidenceV2(); }", "CS1729")]
    [InlineData("public class Attack { object Run(ThemeCompatibilityContextV2 c) => c with { }; }", "CS8858")]
    [InlineData("public class Attack { void Run(ThemeCompatibilityContextV2 c) { c.Manifest = null; } }", "CS1061")]
    [InlineData("public class Attack { object Run(ThemeCompatibilityResultV2 r) => ThemeCompatibilityContextV2.FromResult(r); }", "CS0117")]
    [InlineData("public class Attack { object Run() => new ThemeCompatibilityRuntimeEvidenceV2(); }", "CS0122")]
    [InlineData("public class Attack { object Run() => ThemeCompatibilityJsonV2.DeserializeContext(\"{}\"); }", "CS0117")]
    public void OrdinaryExternalConsumerAttacksFailCompilation(string source, string diagnostic)
    {
        (int exit, string output) = CompileExternal(source);
        Assert.NotEqual(0, exit); Assert.Contains(diagnostic, output, StringComparison.Ordinal);
    }

    [Fact]
    public void OrdinaryExternalConsumerPositiveControlCompiles()
    {
        (int exit, string output) = CompileExternal("public class Consumer { public ThemeCompatibilityRequestV2 Create() => new(null, null, null); }");
        Assert.True(exit == 0, output);
    }

    private static (int Exit, string Output) CompileExternal(string source)
    {
        string path = Path.Combine(Path.GetTempPath(), "tcc-contract-consumer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        try
        {
            XDocument project = new(new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0-windows"), new XElement("Nullable", "enable"), new XElement("NuGetAudit", "true")),
                new XElement("ItemGroup", new[] { typeof(ThemeCompatibilityContextV2).Assembly, typeof(ThemeCompatibilityResultV2).Assembly }
                    .Select(a => new XElement("Reference", new XAttribute("Include", a.GetName().Name!), new XElement("HintPath", a.Location))))));
            project.Save(Path.Combine(path, "Consumer.csproj"));
            File.WriteAllText(Path.Combine(path, "Consumer.cs"), "using Tcc.Presentation.Contracts.Theme; using Tcc.Themes.Compatibility.V2; " + source);
            ProcessStartInfo info = new("dotnet") { WorkingDirectory = path, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            foreach (string arg in new[] { "build", "Consumer.csproj", "--configuration", "Release", "-m:1", "--nologo" }) info.ArgumentList.Add(arg);
            using Process process = Process.Start(info)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync(); Task<string> stderr = process.StandardError.ReadToEndAsync();
            Assert.True(process.WaitForExit(120000), "External compile timed out.");
            return (process.ExitCode, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
        }
        finally { Directory.Delete(path, true); }
    }

    [Fact]
    public void SealedBytesAndAdditiveVersionArePreserved()
    {
        Assert.Equal("EFD30B00E8AB3B72933F1C9B9C85FF5DD2EB22A06AEE4AADF3734956BDFF39E3", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(RepositoryPaths.Root, "src/Tcc.Themes/Compatibility/ThemeCompatibilityVersionNegotiator.cs")))));
        Assert.Equal("7B806344157027A8242181BB7A8A493F8778A817410B81139B007FB88865C3D5", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(RepositoryPaths.Root, "tests/Tcc.Architecture.Tests/PhaseFiveThemeCompatibilityNegotiationTests.cs")))));
        Assert.Equal("FF13BE048200E1ADB37B83BDBB16E9D4908F4B110FCD8EFD2FB5D656FA7CFDF2", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(RepositoryPaths.Root, "src/Tcc.Presentation.Contracts/Theme/ThemeInterfaces.cs")))));
        Assert.Equal("7EF30138A2DC1A563AB88E7ABF17359FFAD3B143242A183C2053C10D07615E0F", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(RepositoryPaths.Root, "src/Tcc.Presentation.Contracts/Theme/ThemeEnums.cs")))));
        Assert.Equal("EA08B4F59C396D95F6B08395AA8F1CD5967D868BE420D4203DC283624A4C7365", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(RepositoryPaths.Root, "src/Tcc.Presentation.Contracts/Theme/ThemeModels.cs")))));
        Assert.Equal("F94B6F7DFCB1CAA1E07E7096ADBB4843457B6BA4B4F7EE1A6F0AD29149ACC358", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(RepositoryPaths.Root, "src/Tcc.Presentation.Contracts/Theme/ThemeContractJson.cs")))));
        Assert.Equal("2.0", ContractVersions.ThemeCompatibilityContractV2);
        Assert.Equal(VersionFields, typeof(ContractVersions).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.Name).Order(StringComparer.Ordinal));
        Assert.Equal("1.0.0", ContractVersions.ThemeApi); Assert.Equal("1.1.0", ContractVersions.UxContract);
        Assert.Equal("1.0.0", ContractVersions.ThemeArchitecture); Assert.Equal("v1.1", ContractVersions.UxArchitectureDisplay);
        Assert.Equal("1.0", ContractVersions.Schema); Assert.Equal("2.0", ContractVersions.ThemeIntegritySchemaV2);
        Assert.Equal("1.0", ContractVersions.ThemeSignatureEnvelopeSchemaV1);
    }
}
