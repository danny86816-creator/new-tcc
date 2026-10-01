using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP3SceneContactIntegrationTests
{
    private const string BehindRelativePath = "Assets/Scene/TCC_P3_SCENE_DECOR_BEHIND_CHARACTER_V1.png";
    private const string ForegroundRelativePath = "Assets/Scene/TCC_P3_SCENE_FOREGROUND_ACCENT_V1.png";
    private const string StructureRelativePath = "Assets/Scene/TCC_P3_RIGHT_ARCHITECTURAL_SYSTEM_V2_R2_CANDIDATE_02.png";
    private const string BehindHash = "C4DC3E27D039A5D6C135299B60AEDADD6AAC0E8AA1163BDCF526E67C12B1C225";
    private const string ForegroundHash = "1E6B2540B0119CEC26E8C16A3800A0E534581E61DEB96C77034C3C350D119CF8";
    private const string StructureHash = "0BE25DF21F564BD5382205CEE95AADE6FFD96AE4F49C0521A1C2946643CAA399";
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string MainWindowPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ProjectPath = Path.Combine(HostRoot, "Tcc.DesktopHost.csproj");

    [Fact]
    public void AcceptedSceneContactAssetsAreByteIdenticalCompiledResources()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3SceneContactIntegrationTests), nameof(AcceptedSceneContactAssetsAreByteIdenticalCompiledResources));
    }

    [Fact]
    public void SceneContactAssetsKeepTheAuthorizedSplitDepthOrder()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3SceneContactIntegrationTests), nameof(SceneContactAssetsKeepTheAuthorizedSplitDepthOrder));
    }

    [Fact]
    public void AuthorityBackedRightVerticalCopyAndCandidateStructureRemainDecorative()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3SceneContactIntegrationTests), nameof(AuthorityBackedRightVerticalCopyAndCandidateStructureRemainDecorative));
    }

    [Fact]
    public void SceneAccessoriesUseBoundedDepthClassesAndLocalEnvironmentalContact()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3SceneContactIntegrationTests), nameof(SceneAccessoriesUseBoundedDepthClassesAndLocalEnvironmentalContact));
    }

    private static void AssertAsset(XDocument project, string relativePath, string expectedHash)
    {
        string path = Path.Combine(HostRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path));
        Assert.Equal(expectedHash, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));
        byte[] png = File.ReadAllBytes(path);
        Assert.Equal(1672, ReadBigEndianInt32(png, 16));
        Assert.Equal(941, ReadBigEndianInt32(png, 20));
        Assert.Equal(6, png[25]);
        Assert.Single(project.Descendants("Resource"), item =>
            item.Attribute("Include")?.Value == relativePath);
    }

    private static void AssertReferencePlaneImage(XElement image, string expectedLeft)
    {
        Assert.Equal(expectedLeft, image.Attribute("Canvas.Left")?.Value);
        Assert.Equal("0", image.Attribute("Canvas.Top")?.Value);
        Assert.Equal("1672", image.Attribute("Width")?.Value);
        Assert.Equal("941", image.Attribute("Height")?.Value);
        Assert.Equal("Fill", image.Attribute("Stretch")?.Value);
        AssertDecorative(image);
    }

    private static void AssertDepthImage(XDocument window, string tag, string expectedOpacity)
    {
        XElement image = GetByTag(window, tag);
        Assert.Equal(expectedOpacity, image.Attribute("Opacity")?.Value);
        Assert.Single(image.Elements(Presentation + "Image.OpacityMask"));
        Assert.Single(image.Descendants(Presentation + "LinearGradientBrush"));
    }

    private static void AssertAlphaContamination(
        XDocument window,
        string tag,
        string expectedSource,
        string expectedFill,
        string expectedOpacity)
    {
        XElement overlay = GetByTag(window, tag);
        Assert.Equal(expectedFill, overlay.Attribute("Fill")?.Value);
        Assert.Equal(expectedOpacity, overlay.Attribute("Opacity")?.Value);
        Assert.Equal(expectedSource,
            Assert.Single(overlay.Descendants(Presentation + "ImageBrush")).Attribute("ImageSource")?.Value);
        AssertDecorative(overlay);
    }

    private static void AssertRegionalImage(
        XElement image,
        string expectedSource,
        string expectedLeft,
        string expectedTop,
        string expectedWidth,
        string expectedHeight)
    {
        Assert.Equal(expectedSource, image.Attribute("Source")?.Value);
        Assert.Equal(expectedLeft, image.Attribute("Canvas.Left")?.Value);
        Assert.Equal(expectedTop, image.Attribute("Canvas.Top")?.Value);
        Assert.Equal(expectedWidth, image.Attribute("Width")?.Value);
        Assert.Equal(expectedHeight, image.Attribute("Height")?.Value);
        Assert.Equal("Fill", image.Attribute("Stretch")?.Value);
        AssertDecorative(image);
    }

    private static void AssertDecorative(XElement element)
    {
        Assert.Equal("False", element.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("False", element.Attribute("Focusable")?.Value);
        Assert.Equal("False", element.Attribute("KeyboardNavigation.IsTabStop")?.Value);
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private static XElement GetByTag(XDocument document, string tag) =>
        Assert.Single(document.Descendants(), element => element.Attribute("Tag")?.Value == tag);
}
