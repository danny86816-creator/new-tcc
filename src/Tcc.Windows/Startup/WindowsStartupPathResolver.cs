using System.IO;

namespace Tcc.Windows.Startup;

public enum WindowsStartupMode : int
{
    Unknown = 0,
    Installer = 1,
    Portable = 2,
}

public sealed class WindowsStartupFacts
{
    internal WindowsStartupFacts(
        WindowsStartupMode mode,
        string? deviceStateRoot,
        string? userPreferencesRoot,
        string? diagnosticCode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        bool hasRoots = deviceStateRoot is not null && userPreferencesRoot is not null;
        bool hasNoRoots = deviceStateRoot is null && userPreferencesRoot is null;
        if ((!hasRoots && !hasNoRoots)
            || (hasRoots && diagnosticCode is not null)
            || (hasNoRoots && string.IsNullOrWhiteSpace(diagnosticCode))
            || (mode == WindowsStartupMode.Unknown && hasRoots))
        {
            throw new ArgumentException("Windows startup facts are contradictory.");
        }

        if (hasRoots
            && (mode == WindowsStartupMode.Unknown
                || !Path.IsPathFullyQualified(deviceStateRoot!)
                || !Path.IsPathFullyQualified(userPreferencesRoot!)
                || !string.Equals(Path.GetFullPath(deviceStateRoot!), deviceStateRoot, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(Path.GetFullPath(userPreferencesRoot!), userPreferencesRoot, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Windows startup roots must be canonical absolute paths.");
        }

        bool validDiagnostic = diagnosticCode is null
            || diagnosticCode is "BOOTSTRAP_MODE_UNKNOWN" or "BOOTSTRAP_PATH_UNAVAILABLE" or "BOOTSTRAP_ARGUMENTS_INVALID";
        if (!validDiagnostic
            || mode == WindowsStartupMode.Unknown && diagnosticCode == "BOOTSTRAP_PATH_UNAVAILABLE"
            || mode != WindowsStartupMode.Unknown && diagnosticCode is "BOOTSTRAP_MODE_UNKNOWN" or "BOOTSTRAP_ARGUMENTS_INVALID")
        {
            throw new ArgumentException("Windows startup diagnostic is invalid.", nameof(diagnosticCode));
        }

        Mode = mode;
        DeviceStateRoot = deviceStateRoot;
        UserPreferencesRoot = userPreferencesRoot;
        DiagnosticCode = diagnosticCode;
    }

    public WindowsStartupMode Mode { get; }

    public string? DeviceStateRoot { get; }

    public string? UserPreferencesRoot { get; }

    public string? DiagnosticCode { get; }
}

public sealed class WindowsStartupPathResolver
{
    public WindowsStartupPathResolver()
    {
    }

    public WindowsStartupFacts Resolve(IReadOnlyList<string> startupArguments)
    {
        _ = GetType();
        ArgumentNullException.ThrowIfNull(startupArguments);

        string? modeValue = null;
        string? selectedPortableRoot = null;
        int modeCount = 0;
        int rootCount = 0;
        bool invalidArguments = false;

        for (int index = 0; index < startupArguments.Count; index++)
        {
            string argument = startupArguments[index]
                ?? throw new ArgumentNullException(nameof(startupArguments));
            if (argument.StartsWith("--startup-mode=", StringComparison.Ordinal))
            {
                modeCount++;
                modeValue = argument["--startup-mode=".Length..];
            }
            else if (argument.StartsWith("--portable-data-root=", StringComparison.Ordinal))
            {
                rootCount++;
                selectedPortableRoot = argument["--portable-data-root=".Length..];
            }
            else
            {
                invalidArguments = true;
            }
        }

        if (invalidArguments || rootCount > 1 || (rootCount == 1 && modeValue != "portable"))
        {
            return new WindowsStartupFacts(WindowsStartupMode.Unknown, null, null, "BOOTSTRAP_ARGUMENTS_INVALID");
        }

        if (modeCount != 1 || modeValue is not ("installer" or "portable"))
        {
            return new WindowsStartupFacts(WindowsStartupMode.Unknown, null, null, "BOOTSTRAP_MODE_UNKNOWN");
        }

        try
        {
            if (modeValue == "installer")
            {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(roaming))
                {
                    return new WindowsStartupFacts(WindowsStartupMode.Installer, null, null, "BOOTSTRAP_PATH_UNAVAILABLE");
                }

                string device = Path.GetFullPath(Path.Combine(local, "TCC", "Themes", "DeviceState"));
                string preferences = Path.GetFullPath(Path.Combine(roaming, "TCC", "Themes", "UserPreferences"));
                return new WindowsStartupFacts(WindowsStartupMode.Installer, device, preferences, null);
            }

            string authorizedRoot;
            string productRoot;
            if (rootCount == 0)
            {
                if (!IsUsableSelectedRoot(AppContext.BaseDirectory))
                {
                    return new WindowsStartupFacts(WindowsStartupMode.Portable, null, null, "BOOTSTRAP_PATH_UNAVAILABLE");
                }

                authorizedRoot = Path.GetFullPath(AppContext.BaseDirectory);
                productRoot = Path.GetFullPath(Path.Combine(authorizedRoot, "data"));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(selectedPortableRoot)
                    || !IsUsableSelectedRoot(selectedPortableRoot))
                {
                    return new WindowsStartupFacts(WindowsStartupMode.Portable, null, null, "BOOTSTRAP_PATH_UNAVAILABLE");
                }

                productRoot = Path.GetFullPath(selectedPortableRoot);
                authorizedRoot = productRoot;
            }

            string themes = Path.GetFullPath(Path.Combine(productRoot, "themes"));
            string userState = Path.GetFullPath(Path.Combine(themes, "user-state"));
            if (!IsContainedOrEqual(authorizedRoot, productRoot)
                || !IsContainedOrEqual(authorizedRoot, themes)
                || !IsContainedOrEqual(authorizedRoot, userState)
                || !IsPhysicallyContained(authorizedRoot, productRoot)
                || !IsPhysicallyContained(authorizedRoot, themes)
                || !IsPhysicallyContained(authorizedRoot, userState))
            {
                return new WindowsStartupFacts(WindowsStartupMode.Portable, null, null, "BOOTSTRAP_PATH_UNAVAILABLE");
            }

            return new WindowsStartupFacts(WindowsStartupMode.Portable, themes, userState, null);
        }
        catch (Exception exception) when (exception is ArgumentException
            or IOException
            or NotSupportedException
            or UnauthorizedAccessException)
        {
            return new WindowsStartupFacts(
                modeValue == "portable" ? WindowsStartupMode.Portable : WindowsStartupMode.Installer,
                null,
                null,
                "BOOTSTRAP_PATH_UNAVAILABLE");
        }
    }

    private static bool IsUsableSelectedRoot(string path)
    {
        if (!IsLocalAbsolutePath(path))
        {
            return false;
        }

        string fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(fullPath))
        {
            return false;
        }

        DirectoryInfo? current = new(fullPath);
        while (current is not null)
        {
            if ((current.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                return false;
            }

            current = current.Parent;
        }

        return true;
    }

    private static bool IsLocalAbsolutePath(string path)
    {
        if (!Path.IsPathFullyQualified(path)
            || path.StartsWith("\\\\", StringComparison.Ordinal)
            || Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) && !uri.IsFile)
        {
            return false;
        }

        string fullPath = Path.GetFullPath(path);
        string? root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrEmpty(root))
        {
            return false;
        }

        DriveInfo drive = new(root);
        return drive.DriveType is not (DriveType.Network or DriveType.NoRootDirectory);
    }

    private static bool IsPhysicallyContained(string authorizedRoot, string candidate)
    {
        HashSet<string> visitedReparsePoints = new(StringComparer.OrdinalIgnoreCase);
        return TryResolveWithinAuthorizedRoot(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(authorizedRoot)),
            Path.GetFullPath(candidate),
            visitedReparsePoints,
            out _);
    }

    private static bool TryResolveWithinAuthorizedRoot(
        string authorizedRoot,
        string candidate,
        HashSet<string> visitedReparsePoints,
        out string resolvedPath)
    {
        resolvedPath = candidate;
        if (!IsContainedOrEqual(authorizedRoot, candidate))
        {
            return false;
        }

        string relative = Path.GetRelativePath(authorizedRoot, candidate);
        if (relative == ".")
        {
            resolvedPath = authorizedRoot;
            return true;
        }

        string[] components = relative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
        string current = authorizedRoot;

        for (int index = 0; index < components.Length; index++)
        {
            string next = Path.GetFullPath(Path.Combine(current, components[index]));
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(next);
            }
            catch (FileNotFoundException)
            {
                return AppendMissingComponents(current, components, index, authorizedRoot, out resolvedPath);
            }
            catch (DirectoryNotFoundException)
            {
                return AppendMissingComponents(current, components, index, authorizedRoot, out resolvedPath);
            }

            if ((attributes & FileAttributes.Directory) == 0)
            {
                return false;
            }

            if ((attributes & FileAttributes.ReparsePoint) == 0)
            {
                current = next;
                continue;
            }

            if (!visitedReparsePoints.Add(next))
            {
                return false;
            }

            FileSystemInfo? target = new DirectoryInfo(next).ResolveLinkTarget(returnFinalTarget: false);
            if (target is null
                || !IsLocalAbsolutePath(target.FullName)
                || !IsContainedOrEqual(authorizedRoot, target.FullName)
                || !TryResolveWithinAuthorizedRoot(
                    authorizedRoot,
                    target.FullName,
                    visitedReparsePoints,
                    out current))
            {
                return false;
            }
        }

        resolvedPath = current;
        return IsContainedOrEqual(authorizedRoot, resolvedPath);
    }

    private static bool AppendMissingComponents(
        string current,
        string[] components,
        int startIndex,
        string authorizedRoot,
        out string resolvedPath)
    {
        for (int index = startIndex; index < components.Length; index++)
        {
            current = Path.GetFullPath(Path.Combine(current, components[index]));
        }

        resolvedPath = current;
        return IsContainedOrEqual(authorizedRoot, resolvedPath);
    }

    private static bool IsContainedOrEqual(string root, string candidate)
    {
        string normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        string normalizedCandidate = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
        string prefix = Path.EndsInDirectorySeparator(normalizedRoot)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        return string.Equals(normalizedRoot, normalizedCandidate, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }
}
