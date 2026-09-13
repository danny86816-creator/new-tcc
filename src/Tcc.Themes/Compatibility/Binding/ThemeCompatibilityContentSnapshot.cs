using System.Collections.Immutable;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Compatibility.Binding;

internal sealed class ThemeCompatibilityContentSnapshot : IThemePackageContentReader
{
    private readonly ThemePackageRef _packageRef;
    private readonly ImmutableArray<ThemePackageContentEntryV1> _entries;
    private readonly IReadOnlyDictionary<string, byte[]> _content;

    private ThemeCompatibilityContentSnapshot(
        ThemePackageRef packageRef,
        ImmutableArray<ThemePackageContentEntryV1> entries,
        IReadOnlyDictionary<string, byte[]> content)
    {
        _packageRef = packageRef;
        _entries = entries;
        _content = content;
    }

    internal static async ValueTask<ThemeCompatibilityContentSnapshot> CaptureAsync(
        ThemePackageRef packageRef,
        IThemePackageContentReader source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<ThemePackageContentEntryV1> sourceEntries = await source
            .EnumerateEntriesAsync(packageRef, cancellationToken)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceEntries is null)
        {
            throw new InvalidDataException("Package inventory is unavailable.");
        }

        ImmutableArray<ThemePackageContentEntryV1>.Builder entries =
            ImmutableArray.CreateBuilder<ThemePackageContentEntryV1>(sourceEntries.Count);
        HashSet<string> readablePaths = new(StringComparer.Ordinal);
        foreach (ThemePackageContentEntryV1? entry in sourceEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry is null)
            {
                throw new InvalidDataException("Package inventory contains an unavailable entry.");
            }

            entries.Add(entry with { });
            if (entry.EntryKind == ThemePackageContentEntryKind.File
                && ThemeCanonicalPath.TryCreate(entry.LogicalPath, out ThemeCanonicalPath? path))
            {
                readablePaths.Add(path!.Value);
            }
        }

        Dictionary<string, byte[]> content = new(StringComparer.Ordinal);
        List<string> orderedPaths = [.. readablePaths];
        orderedPaths.Sort(StringComparer.Ordinal);
        foreach (string path in orderedPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadOnlyMemory<byte> bytes = await source
                .ReadContentAsync(packageRef, path, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            content.Add(path, bytes.ToArray());
        }

        return new ThemeCompatibilityContentSnapshot(packageRef, entries.ToImmutable(), content);
    }

    public ValueTask<IReadOnlyList<ThemePackageContentEntryV1>> EnumerateEntriesAsync(
        ThemePackageRef packageRef,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsurePackage(packageRef);
        return ValueTask.FromResult<IReadOnlyList<ThemePackageContentEntryV1>>(_entries);
    }

    public ValueTask<ReadOnlyMemory<byte>> ReadContentAsync(
        ThemePackageRef packageRef,
        string canonicalPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsurePackage(packageRef);
        if (!ThemeCanonicalPath.TryCreate(canonicalPath, out ThemeCanonicalPath? path)
            || !_content.TryGetValue(path!.Value, out byte[]? bytes))
        {
            throw new InvalidDataException("Snapshot content is unavailable.");
        }

        return ValueTask.FromResult<ReadOnlyMemory<byte>>(bytes.ToArray());
    }

    internal bool TryRead(string canonicalPath, out ReadOnlyMemory<byte> bytes)
    {
        if (_content.TryGetValue(canonicalPath, out byte[]? stored))
        {
            bytes = stored.ToArray();
            return true;
        }

        bytes = default;
        return false;
    }

    private void EnsurePackage(ThemePackageRef packageRef)
    {
        if (packageRef != _packageRef)
        {
            throw new InvalidDataException("Snapshot package identity does not match.");
        }
    }
}
