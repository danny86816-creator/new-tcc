namespace Tcc.Presentation.Contracts.Theme;

/// <summary>
/// Verifies the additive V2 Theme package integrity contract without mutating package or trust state.
/// </summary>
public interface IThemeIntegrityVerifierV2
{
    ValueTask<ThemeIntegrityVerificationResultV2> VerifyAsync(
        ThemeIntegrityVerificationRequestV2 request,
        IThemePackageContentReader contentReader,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Supplies read-only access to an opaque package reference. Implementations must not expose arbitrary filesystem paths,
/// extract, install, move, delete, promote, activate, persist, or cache package content through this contract.
/// </summary>
public interface IThemePackageContentReader
{
    ValueTask<IReadOnlyList<ThemePackageContentEntryV1>> EnumerateEntriesAsync(
        ThemePackageRef packageRef,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads immutable content for an already canonical logical entry path.
    /// </summary>
    ValueTask<ReadOnlyMemory<byte>> ReadContentAsync(
        ThemePackageRef packageRef,
        string canonicalPath,
        CancellationToken cancellationToken = default);
}
