using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Tcc.Architecture.Tests;

public sealed class MasterFidelityR26SceneTests
{
    private const string RelativeAssetPath = "Assets/MasterFidelity/StrataObservatory.MasterR1.Scene.R26.png";
    private const string ExpectedSha256 = "B4C4B7225D75C5A3408E8BF683EEDA7756C6E4398560021BC1F4FE2996C385C2";
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string MainWindowPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "MainWindow.xaml");
    private static readonly string ProjectPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Tcc.DesktopHost.csproj");
    private static readonly string AssetPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", RelativeAssetPath.Replace('/', Path.DirectorySeparatorChar));

    [Fact]
    public void SelectedR26SceneHasApprovedCustodyAndFrame()
    {
        Assert.True(File.Exists(AssetPath));
        byte[] png = File.ReadAllBytes(AssetPath);

        Assert.True(png.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.Equal(1672, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)));
        Assert.Equal(941, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
        Assert.Equal(ExpectedSha256, Convert.ToHexString(SHA256.HashData(png)));
    }

    [Fact]
    public void RuntimeUsesOneIntegratedMasterFidelitySceneWithoutSplitCharacter()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string project = File.ReadAllText(ProjectPath);

        Assert.Contains(RelativeAssetPath, xaml, StringComparison.Ordinal);
        Assert.Equal(
            xaml.IndexOf(RelativeAssetPath, StringComparison.Ordinal),
            xaml.LastIndexOf(RelativeAssetPath, StringComparison.Ordinal));
        Assert.Contains(RelativeAssetPath, project, StringComparison.Ordinal);
        Assert.Equal(
            project.IndexOf(RelativeAssetPath, StringComparison.Ordinal),
            project.LastIndexOf(RelativeAssetPath, StringComparison.Ordinal));
        Assert.Contains("x:Name=\"MasterFidelitySceneImage\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MasterR1.Background.png", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MasterR1.Character.png", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MasterR1.Background.png", project, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MasterR1.Character.png", project, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MasterR1.Scene.R21.png", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MasterR1.Scene.R21.png", project, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Tcc.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
