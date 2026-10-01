using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP3ArchitectureSystemV2Tests
{
    private const string AssetRelativePath = "Assets/Scene/TCC_P3_RIGHT_ARCHITECTURAL_SYSTEM_V2_R2_CANDIDATE_02.png";
    private const string PriorR2CandidateName = "TCC_P3_RIGHT_ARCHITECTURAL_SYSTEM_V2_R2_CANDIDATE_01.png";
    private const string PriorCandidateName = "TCC_P3_RIGHT_ARCHITECTURAL_SYSTEM_V2_CANDIDATE_01.png";
    private const string RejectedAssetName = "TCC_P3_RIGHT_VERTICAL_SCENIC_STRUCTURE_V1_CANDIDATE_01.png";
    private const string ExpectedHash = "0BE25DF21F564BD5382205CEE95AADE6FFD96AE4F49C0521A1C2946643CAA399";
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string MainWindowPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ProjectPath = Path.Combine(HostRoot, "Tcc.DesktopHost.csproj");
    private static readonly string ProductionAssetPath = Path.Combine(
        HostRoot, AssetRelativePath.Replace('/', Path.DirectorySeparatorChar));
    private static readonly string EvidenceAssetPath = Path.Combine(
        RepositoryPaths.Root, "uiux_cleanroom", "reference_master_rebase", "wpf_p3", "p3_c",
        "new_architecture_system", "TCC_P3_RIGHT_ARCHITECTURAL_SYSTEM_V2_R2_CANDIDATE_02.png");

    [Fact]
    public void NewArchitectureAssetIsByteIdenticalRgbaAndTheRejectedAssetIsNotCompiled()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3ArchitectureSystemV2Tests), nameof(NewArchitectureAssetIsByteIdenticalRgbaAndTheRejectedAssetIsNotCompiled));
    }

    [Fact]
    public void OneCoherentArchitectureSystemSpansBehindAndInFrontOfTheCharacter()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3ArchitectureSystemV2Tests), nameof(OneCoherentArchitectureSystemSpansBehindAndInFrontOfTheCharacter));
    }

    [Fact]
    public void CharacterAuthorityGeometryRemainsUnchangedByTheArchitectureReplacement()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3ArchitectureSystemV2Tests), nameof(CharacterAuthorityGeometryRemainsUnchangedByTheArchitectureReplacement));
    }

    private static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private static XElement GetByTag(XDocument document, string tag) =>
        Assert.Single(document.Descendants(), element => element.Attribute("Tag")?.Value == tag);
}
