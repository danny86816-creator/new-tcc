using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP3CharacterPlacementTests
{
    private const string ExpectedCharacterHash = "A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D";
    private const string ExpectedDerivedCharacterHash = "F2ABBD4778C017DED49416EACD4F29F88631362769D8869030C3A54CCD8FDAE4";
    private const string CharacterAssetRelativePath = "Assets/Character/TCC_P3_CHARACTER_WORKING_SOURCE.png";
    private const string DerivedCharacterAssetRelativePath = "Assets/Character/TCC_P3_CHARACTER_DERIVED_INTEGRATED.png";
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string MainWindowPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ProjectPath = Path.Combine(HostRoot, "Tcc.DesktopHost.csproj");
    private static readonly string ProductionCharacterPath = Path.Combine(
        HostRoot, CharacterAssetRelativePath.Replace('/', Path.DirectorySeparatorChar));
    private static readonly string ProductionDerivedCharacterPath = Path.Combine(
        HostRoot, DerivedCharacterAssetRelativePath.Replace('/', Path.DirectorySeparatorChar));
    private static readonly string AcceptedCharacterPath = Path.Combine(
        RepositoryPaths.Root, "uiux_cleanroom", "reference_master_rebase", "wpf_p3", "assets", "character",
        "TCC_P3_CHARACTER_WORKING_SOURCE.png");

    [Fact]
    public void ProductionCharacterIsTheByteIdenticalAcceptedRgbaAssetAndACompiledResource()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterPlacementTests), nameof(ProductionCharacterIsTheByteIdenticalAcceptedRgbaAssetAndACompiledResource));
    }

    [Fact]
    public void CharacterLayerIsDecorativeUniformAndUsesOnlyThePlacementCoordinatePlane()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterPlacementTests), nameof(CharacterLayerIsDecorativeUniformAndUsesOnlyThePlacementCoordinatePlane));
    }

    [Fact]
    public void FullAuthorityAlphaPlacementUsesOnlyThePhysicalWindowBoundary()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterPlacementTests), nameof(FullAuthorityAlphaPlacementUsesOnlyThePhysicalWindowBoundary));
    }

    [Fact]
    public void P3AAcceptedResourcesAndP1P2GeometryRemainFrozen()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterPlacementTests), nameof(P3AAcceptedResourcesAndP1P2GeometryRemainFrozen));
    }

    [Fact]
    public void AcceptedCharacterPlacementRemainsFrozenInsideTheActiveSceneDepthTopology()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterPlacementTests), nameof(AcceptedCharacterPlacementRemainsFrozenInsideTheActiveSceneDepthTopology));
    }

    private static void AssertDecorative(XElement element)
    {
        Assert.Equal("False", element.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("False", element.Attribute("Focusable")?.Value);
        Assert.Equal("False", element.Attribute("KeyboardNavigation.IsTabStop")?.Value);
    }

    private static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private static XElement GetByTag(XDocument document, string tag) =>
        Assert.Single(document.Descendants(), element => element.Attribute("Tag")?.Value == tag);

    private static XElement GetByAutomationId(XDocument document, string automationId) =>
        Assert.Single(document.Descendants(), element => element.Attributes().Any(attribute =>
            attribute.Name.LocalName == "AutomationProperties.AutomationId" && attribute.Value == automationId));
}
