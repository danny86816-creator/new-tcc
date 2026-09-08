using System.Security.Cryptography;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Integrity;

/// <summary>
/// Evaluates logical inventory and individual file bytes after request preflight.
/// This stage cannot establish package, metadata, signature, or trust verification.
/// </summary>
internal static class ThemePackageInventoryEvaluator
{
    internal static async ValueTask<ThemePackageInventoryEvaluation> EvaluateAsync(
        ThemeIntegrityVerificationRequestV2 request,
        IThemePackageContentReader contentReader,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<ThemeIntegrityDiagnosticV1> diagnostics = [];
        List<ThemeIntegrityFileEvidenceV1> evidence = [];
        ThemePackageContentEntryV1[] snapshot;
        try
        {
            IReadOnlyList<ThemePackageContentEntryV1> entries = await contentReader
                .EnumerateEntriesAsync(request.PackageRef, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            snapshot = entries.ToArray();
        }
        catch (Exception exception) when (IsExpectedReaderFailure(exception))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.PackageReadFailure, null,
                "Package inventory could not be read.");
            return Complete(evidence, diagnostics);
        }

        List<(string Path, ThemeIntegrityFileV2 File)> declared = [];
        HashSet<string> invalidDeclarations = new(StringComparer.Ordinal);
        foreach (ThemeIntegrityFileV2 file in request.IntegrityManifest.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ThemeCanonicalPath.TryCreate(file.CanonicalPath, out ThemeCanonicalPath? path))
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath, file.CanonicalPath,
                    "Declared inventory path is not canonical.");
                continue;
            }

            string canonical = path!.Value;
            declared.Add((canonical, file));
        }

        List<(string Path, ThemePackageContentEntryV1 Entry)> present = [];
        foreach (ThemePackageContentEntryV1 entry in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool validPath = ThemeCanonicalPath.TryCreate(entry.LogicalPath, out ThemeCanonicalPath? path);
            if (!validPath)
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath, entry.LogicalPath,
                    "Enumerated package path is not canonical.");
            }

            if (entry.EntryKind != ThemePackageContentEntryKind.File)
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UnsupportedPackageEntry, path?.Value,
                    "Only regular file entries are supported by the integrity contract.");
            }

            if (validPath)
            {
                present.Add((path!.Value, entry));
            }
        }

        // Inspect both complete path sets before excluding reserved metadata or ineligible declarations.
        // An ambiguous path on either side must never resolve to a readable entry.
        List<string> declaredPathList = [];
        HashSet<string> declaredPaths = new(StringComparer.Ordinal);
        foreach ((string path, _) in declared)
        {
            declaredPathList.Add(path);
            declaredPaths.Add(path);
        }

        List<string> presentPathList = [];
        foreach ((string path, _) in present)
        {
            presentPathList.Add(path);
        }

        HashSet<string> ambiguous = FindAmbiguousPaths(declaredPathList, diagnostics);
        ambiguous.UnionWith(FindAmbiguousPaths(presentPathList, diagnostics));

        foreach ((string path, ThemeIntegrityFileV2 file) in declared)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsReserved(request, path))
            {
                invalidDeclarations.Add(path);
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSecurityValue, null,
                    "A payload file declaration must not target a reserved integrity metadata path.");
                continue;
            }

            if (!Enum.IsDefined(file.EntryKind) || file.LengthBytes < 0 || !IsSha256(file.Sha256))
            {
                invalidDeclarations.Add(path);
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.InvalidSecurityValue, null,
                    "A declared file entry kind, length, or SHA-256 value is invalid.");
            }
        }

        Dictionary<string, ThemePackageContentEntryV1> readableInventory = new(StringComparer.Ordinal);
        foreach ((string path, ThemePackageContentEntryV1 entry) in present)
        {
            if (!ambiguous.Contains(path))
            {
                readableInventory.Add(path, entry);
            }
        }

        foreach ((string path, _) in present)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ambiguous.Contains(path) && !declaredPaths.Contains(path) && !IsReserved(request, path))
            {
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.UndeclaredFile, path,
                    "Present package entry is not declared by the exhaustive inventory.");
            }
        }

        SortDeclarations(declared);
        foreach ((string path, ThemeIntegrityFileV2 file) in declared)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ambiguous.Contains(path) || invalidDeclarations.Contains(path))
            {
                continue;
            }

            if (!readableInventory.TryGetValue(path, out ThemePackageContentEntryV1? entry))
            {
                if (file.Required)
                {
                    Add(diagnostics, ThemeIntegrityDiagnosticCodes.MissingRequiredFile, path,
                        "Required inventory entry is absent.");
                }

                evidence.Add(CreateEvidence(file, path, false, null, null, file.Required
                    ? ThemeIntegrityEvidenceStatus.MissingRequired
                    : ThemeIntegrityEvidenceStatus.MissingAllowed));
                continue;
            }

            if (entry.EntryKind != ThemePackageContentEntryKind.File)
            {
                evidence.Add(CreateEvidence(file, path, true, null, null,
                    ThemeIntegrityEvidenceStatus.UnsupportedEntry));
                continue;
            }

            ReadOnlyMemory<byte> bytes;
            try
            {
                bytes = await contentReader.ReadContentAsync(request.PackageRef, path, cancellationToken)
                    .ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception exception) when (IsExpectedReaderFailure(exception))
            {
                cancellationToken.ThrowIfCancellationRequested();
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.PackageReadFailure, path,
                    "Package content bytes are unavailable for an enumerated file.");
                // Sealed evidence semantics: this length is only an enumeration claim.
                // Unavailable never means content bytes or their length were verified.
                evidence.Add(CreateEvidence(file, path, true, entry.LengthBytes, null,
                    ThemeIntegrityEvidenceStatus.Unavailable));
                continue;
            }

            string hash = Convert.ToHexStringLower(SHA256.HashData(bytes.Span));
            ThemeIntegrityEvidenceStatus status = ThemeIntegrityEvidenceStatus.Verified;
            if (file.LengthBytes != bytes.Length || entry.LengthBytes != bytes.Length)
            {
                status = ThemeIntegrityEvidenceStatus.LengthMismatch;
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.FileLengthMismatch, path,
                    "Declared or enumerated file length does not match content length.");
            }
            else if (!string.Equals(file.Sha256, hash, StringComparison.Ordinal))
            {
                status = ThemeIntegrityEvidenceStatus.HashMismatch;
                Add(diagnostics, file.EntryKind == ThemeIntegrityEntryKind.ThemeManifest
                        ? ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch
                        : ThemeIntegrityDiagnosticCodes.FileHashMismatch,
                    path, "Declared file hash does not match raw content bytes.");
            }

            evidence.Add(CreateEvidence(file, path, true, bytes.Length, hash, status));
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Complete(evidence, diagnostics);
    }

    private static bool IsReserved(ThemeIntegrityVerificationRequestV2 request, string path) =>
        string.Equals(path, request.IntegrityManifestPath.Value, StringComparison.Ordinal)
        || string.Equals(path, request.SignatureEnvelopePath?.Value, StringComparison.Ordinal);

    private static bool IsSha256(string? value)
    {
        if (value is not { Length: 64 })
        {
            return false;
        }

        foreach (char character in value)
        {
            if (character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsExpectedReaderFailure(Exception exception) =>
        exception is IOException or InvalidDataException or UnauthorizedAccessException;

    private static HashSet<string> FindAmbiguousPaths(
        IReadOnlyList<string> paths,
        List<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        HashSet<string> ambiguous = new(StringComparer.Ordinal);
        Dictionary<string, int> exactCounts = new(StringComparer.Ordinal);
        Dictionary<string, HashSet<string>> caseGroups = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in paths)
        {
            exactCounts[path] = exactCounts.TryGetValue(path, out int count) ? count + 1 : 1;
            if (!caseGroups.TryGetValue(path, out HashSet<string>? spellings))
            {
                spellings = new HashSet<string>(StringComparer.Ordinal);
                caseGroups.Add(path, spellings);
            }

            spellings.Add(path);
        }

        foreach (KeyValuePair<string, int> exact in exactCounts)
        {
            if (exact.Value > 1)
            {
                ambiguous.Add(exact.Key);
                Add(diagnostics, ThemeIntegrityDiagnosticCodes.DuplicateNormalizedPath, exact.Key,
                    "Multiple paths normalize to the same canonical path.");
            }
        }

        foreach (HashSet<string> group in caseGroups.Values)
        {
            if (group.Count <= 1)
            {
                continue;
            }

            List<string> spellings = [.. group];
            spellings.Sort(StringComparer.Ordinal);
            ambiguous.UnionWith(spellings);
            Add(diagnostics, ThemeIntegrityDiagnosticCodes.CaseInsensitivePathCollision, spellings[0],
                "Canonical paths collide under Windows ordinal-ignore-case comparison.");
        }

        return ambiguous;
    }

    private static void SortDeclarations(List<(string Path, ThemeIntegrityFileV2 File)> declarations)
    {
        for (int index = 1; index < declarations.Count; index++)
        {
            (string Path, ThemeIntegrityFileV2 File) current = declarations[index];
            int target = index;
            while (target > 0
                   && StringComparer.Ordinal.Compare(declarations[target - 1].Path, current.Path) > 0)
            {
                declarations[target] = declarations[target - 1];
                target--;
            }

            declarations[target] = current;
        }
    }

    private static ThemeIntegrityFileEvidenceV1 CreateEvidence(
        ThemeIntegrityFileV2 file, string path, bool present, long? length, string? hash,
        ThemeIntegrityEvidenceStatus status) =>
        new(path, file.EntryKind, file.Required, present, file.LengthBytes, length, file.Sha256, hash, status);

    private static void Add(List<ThemeIntegrityDiagnosticV1> diagnostics, string code, string? path, string message)
    {
        ThemeIntegrityDiagnosticV1 diagnostic = new(code, ThemeDiagnosticsSeverity.Error, path, message);
        if (!diagnostics.Contains(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static ThemePackageInventoryEvaluation Complete(
        List<ThemeIntegrityFileEvidenceV1> evidence,
        List<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        for (int index = 1; index < evidence.Count; index++)
        {
            ThemeIntegrityFileEvidenceV1 current = evidence[index];
            int target = index;
            while (target > 0
                   && StringComparer.Ordinal.Compare(evidence[target - 1].CanonicalPath, current.CanonicalPath) > 0)
            {
                evidence[target] = evidence[target - 1];
                target--;
            }

            evidence[target] = current;
        }

        for (int index = 1; index < diagnostics.Count; index++)
        {
            ThemeIntegrityDiagnosticV1 current = diagnostics[index];
            int target = index;
            while (target > 0 && CompareDiagnostics(diagnostics[target - 1], current) > 0)
            {
                diagnostics[target] = diagnostics[target - 1];
                target--;
            }

            diagnostics[target] = current;
        }

        return new ThemePackageInventoryEvaluation(
            diagnostics.Count == 0,
            evidence.ToArray(),
            diagnostics.ToArray());
    }

    private static int CompareDiagnostics(ThemeIntegrityDiagnosticV1 left, ThemeIntegrityDiagnosticV1 right)
    {
        int result = StringComparer.Ordinal.Compare(left.Code, right.Code);
        if (result == 0)
        {
            result = StringComparer.Ordinal.Compare(left.CanonicalPath, right.CanonicalPath);
        }

        return result == 0 ? StringComparer.Ordinal.Compare(left.Message, right.Message) : result;
    }
}

/// <summary>An internal stage outcome, not a complete package verification result.</summary>
internal sealed record ThemePackageInventoryEvaluation(
    bool CanContinue,
    IReadOnlyList<ThemeIntegrityFileEvidenceV1> FileEvidence,
    IReadOnlyList<ThemeIntegrityDiagnosticV1> Diagnostics);
