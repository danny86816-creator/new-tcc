namespace Tcc.Presentation.Contracts.Theme;

/// <summary>
/// Frozen machine-readable contract identifiers used by the Phase 2 Theme surface.
/// </summary>
public static class ContractVersions
{
    public const string ThemeApi = "1.0.0";
    public const string ThemeArchitecture = "1.0.0";
    public const string UxContract = "1.1.0";
    public const string UxArchitectureDisplay = "v1.1";
    public const string Schema = "1.0";
    public const string ThemeIntegritySchemaV2 = "2.0";
    public const string ThemeSignatureEnvelopeSchemaV1 = "1.0";
}

public readonly record struct ThemeId(string Value);

public readonly record struct ThemeVersion(string Value);

public readonly record struct ThemeApiVersion(string Value);

public readonly record struct UxContractVersion(string Value);

public readonly record struct UxSurfaceId(string Value);

public readonly record struct GlobalStateId(string Value);

public readonly record struct ThemeVariantId(string Value);

public readonly record struct ThemeAssetId(string Value);

public readonly record struct ThemePreviewSessionId(string Value);

public readonly record struct ThemeCorrelationId(string Value);

/// <summary>
/// Opaque logical identity for a Theme package. It is not a filesystem path.
/// </summary>
public readonly record struct ThemePackageRef(string Value);
