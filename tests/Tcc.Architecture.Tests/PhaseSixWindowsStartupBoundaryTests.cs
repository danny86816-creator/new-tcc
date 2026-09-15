using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Tcc.Windows.Monitors;
using Tcc.Windows.Startup;

namespace Tcc.Architecture.Tests;

public sealed class PhaseSixWindowsStartupBoundaryTests
{
    [Fact]
    public void StartupModeValuesAndUnknownSemanticsAreExact()
    {
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(WindowsStartupMode)));
        Assert.Equal(["Unknown", "Installer", "Portable"], Enum.GetNames<WindowsStartupMode>());
        Assert.Equal([0, 1, 2], Enum.GetValues<WindowsStartupMode>().Select(value => (int)value));

        WindowsStartupPathResolver resolver = new();
        AssertUnknown(resolver.Resolve([]), "BOOTSTRAP_MODE_UNKNOWN");
        AssertUnknown(resolver.Resolve(["--startup-mode=unknown"]), "BOOTSTRAP_MODE_UNKNOWN");
        AssertUnknown(resolver.Resolve(["--startup-mode=Installer"]), "BOOTSTRAP_MODE_UNKNOWN");
        AssertUnknown(
            resolver.Resolve(["--startup-mode=portable", "--startup-mode=portable"]),
            "BOOTSTRAP_MODE_UNKNOWN");
        AssertUnknown(
            resolver.Resolve(["--startup-mode=installer", "--startup-mode=installer"]),
            "BOOTSTRAP_MODE_UNKNOWN");
        AssertUnknown(
            resolver.Resolve(["--startup-mode=installer", "--startup-mode=portable"]),
            "BOOTSTRAP_MODE_UNKNOWN");
        AssertUnknown(resolver.Resolve(["--bad=value"]), "BOOTSTRAP_ARGUMENTS_INVALID");
        AssertUnknown(resolver.Resolve(["--portable-data-root"]), "BOOTSTRAP_ARGUMENTS_INVALID");
        Assert.Throws<ArgumentNullException>(() => resolver.Resolve(null!));
        Assert.Throws<ArgumentNullException>(() => resolver.Resolve([null!]));
    }

    [Fact]
    public void InstallerAndPortablePathsAreCanonicalAndStorageFree()
    {
        WindowsStartupPathResolver resolver = new();
        WindowsStartupFacts installer = resolver.Resolve(["--startup-mode=installer"]);
        Assert.Equal(WindowsStartupMode.Installer, installer.Mode);
        Assert.Null(installer.DiagnosticCode);
        string installerDeviceRoot = Assert.IsType<string>(installer.DeviceStateRoot);
        string installerPreferencesRoot = Assert.IsType<string>(installer.UserPreferencesRoot);
        Assert.True(Path.IsPathFullyQualified(installerDeviceRoot));
        Assert.True(Path.IsPathFullyQualified(installerPreferencesRoot));
        Assert.EndsWith(Path.Combine("TCC", "Themes", "DeviceState"), installerDeviceRoot, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(Path.Combine("TCC", "Themes", "UserPreferences"), installerPreferencesRoot, StringComparison.OrdinalIgnoreCase);

        WindowsStartupFacts portable = resolver.Resolve(["--startup-mode=portable"]);
        Assert.Equal(WindowsStartupMode.Portable, portable.Mode);
        Assert.Null(portable.DiagnosticCode);
        Assert.Equal(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "data", "themes")), portable.DeviceStateRoot);
        Assert.Equal(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "data", "themes", "user-state")), portable.UserPreferencesRoot);
        Assert.False(Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data")));
    }

    [Fact]
    public void PortableSelectedRootRejectsInvalidShapesAndAcceptsExistingLocalDirectory()
    {
        WindowsStartupPathResolver resolver = new();
        string root = Path.Combine(Path.GetTempPath(), $"tcc-p6-83-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            WindowsStartupFacts accepted = resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={root}"]);
            Assert.Equal(WindowsStartupMode.Portable, accepted.Mode);
            Assert.Null(accepted.DiagnosticCode);
            Assert.Equal(Path.Combine(root, "themes"), accepted.DeviceStateRoot);
            Assert.Equal(Path.Combine(root, "themes", "user-state"), accepted.UserPreferencesRoot);
            Assert.False(Directory.Exists(Path.Combine(root, "themes")));
            Assert.False(Directory.Exists(Path.Combine(root, "themes", "user-state")));

            AssertUnavailable(resolver.Resolve(["--startup-mode=portable", "--portable-data-root=relative"]));
            AssertUnavailable(resolver.Resolve(["--startup-mode=portable", "--portable-data-root=\\\\server\\share"]));
            AssertUnavailable(resolver.Resolve(["--startup-mode=portable", "--portable-data-root=\\\\?\\C:\\data"]));
            AssertUnavailable(resolver.Resolve(["--startup-mode=portable", "--portable-data-root=\\\\.\\C:\\data"]));
            AssertUnavailable(resolver.Resolve(["--startup-mode=portable", "--portable-data-root=C:\\definitely-missing-tcc-root"]));
            AssertUnknown(
                resolver.Resolve(["--startup-mode=installer", $"--portable-data-root={root}"]),
                "BOOTSTRAP_ARGUMENTS_INVALID");
            AssertUnknown(
                resolver.Resolve([
                    "--startup-mode=portable",
                    $"--portable-data-root={root}",
                    $"--portable-data-root={root}"
                ]),
                "BOOTSTRAP_ARGUMENTS_INVALID");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PortableVolumeRootIsAcceptedWithFrozenLayoutAndDoesNotCreateDirectories()
    {
        WindowsStartupPathResolver resolver = new();
        string volumeRoot = Assert.IsType<string>(Path.GetPathRoot(Path.GetTempPath()));
        Assert.Equal(DriveType.Fixed, new DriveInfo(volumeRoot).DriveType);

        string themes = Path.GetFullPath(Path.Combine(volumeRoot, "themes"));
        string userState = Path.GetFullPath(Path.Combine(themes, "user-state"));
        bool themesExisted = Directory.Exists(themes);
        bool userStateExisted = Directory.Exists(userState);

        WindowsStartupFacts canonical = resolver.Resolve(
            ["--startup-mode=portable", $"--portable-data-root={volumeRoot}"]);
        WindowsStartupFacts caseVariation = resolver.Resolve(
            ["--startup-mode=portable", $"--portable-data-root={volumeRoot.ToLowerInvariant()}"]);

        Assert.Equal(WindowsStartupMode.Portable, canonical.Mode);
        Assert.Null(canonical.DiagnosticCode);
        Assert.Equal(themes, canonical.DeviceStateRoot, ignoreCase: true);
        Assert.Equal(userState, canonical.UserPreferencesRoot, ignoreCase: true);
        Assert.Equal(WindowsStartupMode.Portable, caseVariation.Mode);
        Assert.Null(caseVariation.DiagnosticCode);
        Assert.Equal(themes, caseVariation.DeviceStateRoot, ignoreCase: true);
        Assert.Equal(userState, caseVariation.UserPreferencesRoot, ignoreCase: true);
        Assert.Equal(themesExisted, Directory.Exists(themes));
        Assert.Equal(userStateExisted, Directory.Exists(userState));
    }

    [Fact]
    public void PortableOrdinaryRootAcceptsWithOrWithoutTrailingSeparator()
    {
        WindowsStartupPathResolver resolver = new();
        string root = Path.Combine(Path.GetTempPath(), $"tcc-p6-84r2-trailing-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            string rootWithSeparator = root + Path.DirectorySeparatorChar;
            WindowsStartupFacts withoutSeparator = resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={root}"]);
            WindowsStartupFacts withSeparator = resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={rootWithSeparator}"]);

            Assert.Equal(WindowsStartupMode.Portable, withoutSeparator.Mode);
            Assert.Null(withoutSeparator.DiagnosticCode);
            Assert.Equal(Path.Combine(root, "themes"), withoutSeparator.DeviceStateRoot);
            Assert.Equal(Path.Combine(root, "themes", "user-state"), withoutSeparator.UserPreferencesRoot);
            Assert.Equal(withoutSeparator.DeviceStateRoot, withSeparator.DeviceStateRoot);
            Assert.Equal(withoutSeparator.UserPreferencesRoot, withSeparator.UserPreferencesRoot);
            Assert.Null(withSeparator.DiagnosticCode);
            Assert.False(Directory.Exists(Path.Combine(root, "themes")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PortableSelectedRootRejectsExternalJunctionEscapesAndAcceptsInternalJunction()
    {
        WindowsStartupPathResolver resolver = new();
        string fixture = Path.Combine(Path.GetTempPath(), $"tcc-p6-84r-{Guid.NewGuid():N}");
        string outside = Path.Combine(fixture, "outside");
        string rootLink = Path.Combine(fixture, "root-link");
        string themesEscapeRoot = Path.Combine(fixture, "themes-escape");
        string themesEscape = Path.Combine(themesEscapeRoot, "themes");
        string preferencesEscapeRoot = Path.Combine(fixture, "preferences-escape");
        string preferencesThemes = Path.Combine(preferencesEscapeRoot, "themes");
        string preferencesEscape = Path.Combine(preferencesThemes, "user-state");
        string internalRoot = Path.Combine(fixture, "internal");
        string internalTarget = Path.Combine(internalRoot, "internal-target");
        string internalThemes = Path.Combine(internalRoot, "themes");

        Directory.CreateDirectory(outside);
        Directory.CreateDirectory(themesEscapeRoot);
        Directory.CreateDirectory(preferencesThemes);
        Directory.CreateDirectory(internalTarget);
        try
        {
            CreateJunction(rootLink, outside);
            AssertUnavailable(resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={rootLink}"]));

            CreateJunction(themesEscape, outside);
            AssertUnavailable(resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={themesEscapeRoot}"]));

            CreateJunction(preferencesEscape, outside);
            AssertUnavailable(resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={preferencesEscapeRoot}"]));

            CreateJunction(internalThemes, internalTarget);
            WindowsStartupFacts accepted = resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={internalRoot}"]);
            Assert.Equal(WindowsStartupMode.Portable, accepted.Mode);
            Assert.Null(accepted.DiagnosticCode);
            Assert.Equal(internalThemes, accepted.DeviceStateRoot);
            Assert.Equal(Path.Combine(internalThemes, "user-state"), accepted.UserPreferencesRoot);
            Assert.False(Directory.Exists(Path.Combine(internalTarget, "user-state")));
        }
        finally
        {
            DeleteJunction(preferencesEscape);
            DeleteJunction(themesEscape);
            DeleteJunction(internalThemes);
            DeleteJunction(rootLink);
            Directory.Delete(fixture, recursive: true);
        }
    }

    [Fact]
    public void PortableDefaultRootRejectsExternalDataJunction()
    {
        WindowsStartupPathResolver resolver = new();
        string defaultData = Path.Combine(AppContext.BaseDirectory, "data");
        string fixture = Path.Combine(Path.GetTempPath(), $"tcc-p6-84r-default-{Guid.NewGuid():N}");
        string outside = Path.Combine(fixture, "outside");

        Assert.False(Directory.Exists(defaultData));
        Directory.CreateDirectory(outside);
        try
        {
            CreateJunction(defaultData, outside);
            AssertUnavailable(resolver.Resolve(["--startup-mode=portable"]));
        }
        finally
        {
            DeleteJunction(defaultData);
            Directory.Delete(fixture, recursive: true);
        }
    }

    [Fact]
    public void PortablePrefixCollisionAndReparseCycleAreRejected()
    {
        WindowsStartupPathResolver resolver = new();
        string fixture = Path.Combine(Path.GetTempPath(), $"tcc-p6-84r2-security-{Guid.NewGuid():N}");
        string root = Path.Combine(fixture, "foo");
        string prefixCollision = Path.Combine(fixture, "foobar");
        string themes = Path.Combine(root, "themes");
        string cycleRoot = Path.Combine(fixture, "cycle-root");
        string cycleThemes = Path.Combine(cycleRoot, "themes");
        string cycleB = Path.Combine(cycleRoot, "cycle-b");

        Directory.CreateDirectory(root);
        Directory.CreateDirectory(prefixCollision);
        Directory.CreateDirectory(cycleRoot);
        Directory.CreateDirectory(cycleThemes);
        try
        {
            CreateJunction(themes, prefixCollision);
            AssertUnavailable(resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={root}"]));

            CreateJunction(cycleB, cycleThemes);
            Directory.Delete(cycleThemes);
            CreateJunction(cycleThemes, cycleB);
            AssertUnavailable(resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={cycleRoot}"]));
        }
        finally
        {
            DeleteJunction(cycleThemes);
            DeleteJunction(cycleB);
            DeleteJunction(themes);
            Directory.Delete(fixture, recursive: true);
        }
    }

    [Fact]
    public void PortableVolumeRootRejectsExternalDescendantJunction()
    {
        WindowsStartupPathResolver resolver = new();
        string fixture = Path.Combine(Path.GetTempPath(), $"tcc-p6-84r2-volume-{Guid.NewGuid():N}");
        string volumeBacking = Path.Combine(fixture, "volume");
        string outside = Path.Combine(fixture, "outside");
        Directory.CreateDirectory(volumeBacking);
        Directory.CreateDirectory(outside);

        string drive = CreateSubstDrive(volumeBacking);
        string volumeRoot = drive + Path.DirectorySeparatorChar;
        string themes = Path.Combine(volumeRoot, "themes");
        try
        {
            Assert.Equal(DriveType.Fixed, new DriveInfo(volumeRoot).DriveType);
            CreateJunction(themes, outside);
            AssertUnavailable(resolver.Resolve(
                ["--startup-mode=portable", $"--portable-data-root={volumeRoot}"]));
        }
        finally
        {
            DeleteJunction(themes);
            DeleteSubstDrive(drive);
            Directory.Delete(fixture, recursive: true);
        }
    }

    [Fact]
    public void WindowMonitorFactsFitBoundsHandlesNegativeAndExtremeCoordinates()
    {
        ConstructorInfo constructor = typeof(WindowMonitorFacts).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
        WindowMonitorFacts facts = (WindowMonitorFacts)constructor.Invoke(["DISPLAY", -1920, -200, 1920, 1080, 144u]);

        Assert.Equal((-1920, -200, 1920, 1080), facts.FitBounds(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue));
        Assert.Equal((-800, 100, 800, 600), facts.FitBounds(-100, 100, 800, 600));
        Assert.Equal((-800, 280, 800, 600), facts.FitBounds(int.MaxValue, int.MaxValue, 800, 600));
        Assert.Throws<ArgumentOutOfRangeException>(() => facts.FitBounds(0, 0, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => facts.FitBounds(0, 0, 1, 0));
    }

    [Fact]
    public void AdapterUsesARealHwndAndReturnsUnknownForInvalidHandles()
    {
        WindowMonitorAdapter adapter = new();
        Assert.Null(adapter.Capture(0));
        Assert.False(adapter.EnsureVisible(0));

        nint window = CreateWindowEx(0, "STATIC", "TCC monitor probe", 0x00CF0000,
            20, 20, 320, 200, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        try
        {
            WindowMonitorFacts? facts = adapter.Capture(window);
            Assert.NotNull(facts);
            Assert.NotEmpty(facts.DeviceName);
            Assert.True(facts.WorkAreaWidth > 0);
            Assert.True(facts.WorkAreaHeight > 0);
            Assert.True(facts.Dpi > 0);
            Assert.True(adapter.EnsureVisible(window));
        }
        finally
        {
            Assert.True(DestroyWindow(window));
        }

        Assert.Null(adapter.Capture(window));
    }

    private static void AssertUnknown(WindowsStartupFacts facts, string code)
    {
        Assert.Equal(WindowsStartupMode.Unknown, facts.Mode);
        Assert.Null(facts.DeviceStateRoot);
        Assert.Null(facts.UserPreferencesRoot);
        Assert.Equal(code, facts.DiagnosticCode);
    }

    private static void AssertUnavailable(WindowsStartupFacts facts)
    {
        Assert.Equal(WindowsStartupMode.Portable, facts.Mode);
        Assert.Null(facts.DeviceStateRoot);
        Assert.Null(facts.UserPreferencesRoot);
        Assert.Equal("BOOTSTRAP_PATH_UNAVAILABLE", facts.DiagnosticCode);
    }

    private static void CreateJunction(string junction, string target)
    {
        ProcessStartInfo startInfo = new("cmd.exe")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("mklink");
        startInfo.ArgumentList.Add("/J");
        startInfo.ArgumentList.Add(junction);
        startInfo.ArgumentList.Add(target);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the Windows junction utility.");
        string standardOutput = process.StandardOutput.ReadToEnd();
        string standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.True(
            process.ExitCode == 0,
            $"Could not create junction. stdout: {standardOutput} stderr: {standardError}");
        Assert.True((File.GetAttributes(junction) & FileAttributes.ReparsePoint) != 0);
    }

    private static void DeleteJunction(string junction)
    {
        try
        {
            if ((File.GetAttributes(junction) & FileAttributes.ReparsePoint) != 0)
            {
                Directory.Delete(junction);
            }
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
    }

    private static string CreateSubstDrive(string target)
    {
        HashSet<string> existing = DriveInfo.GetDrives()
            .Select(drive => drive.Name[..2])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        string drive = Enumerable.Range('D', 'Z' - 'D' + 1)
            .Reverse()
            .Select(value => $"{(char)value}:")
            .First(candidate => !existing.Contains(candidate));

        RunSubst(drive, target);
        Assert.True(Directory.Exists(drive + Path.DirectorySeparatorChar));
        return drive;
    }

    private static void DeleteSubstDrive(string drive)
    {
        RunSubst(drive, "/D");
        Assert.False(Directory.Exists(drive + Path.DirectorySeparatorChar));
    }

    private static void RunSubst(string drive, string argument)
    {
        ProcessStartInfo startInfo = new("subst.exe")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(drive);
        startInfo.ArgumentList.Add(argument);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the Windows subst utility.");
        string standardOutput = process.StandardOutput.ReadToEnd();
        string standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.True(
            process.ExitCode == 0,
            $"Could not update substituted drive. stdout: {standardOutput} stderr: {standardError}");
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
    private static extern nint CreateWindowEx(
        uint extendedStyle,
        string className,
        string windowName,
        uint style,
        int x,
        int y,
        int width,
        int height,
        nint parent,
        nint menu,
        nint instance,
        nint parameter);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint windowHandle);
}
