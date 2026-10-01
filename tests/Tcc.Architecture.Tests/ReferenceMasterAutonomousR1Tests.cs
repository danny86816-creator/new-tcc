using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterAutonomousR1Tests
{
    private static readonly XNamespace P = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string XamlPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ResourcePath = Path.Combine(HostRoot, "Resources", "AutonomousR1.Design.xaml");

    [Fact]
    public void MainWindowUsesOnlyTheStrataDesignDictionary()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterAutonomousR1Tests), nameof(MainWindowUsesOnlyTheStrataDesignDictionary));
    }

    [Fact]
    public void StageTwoGeometryGateContainsNoRasterImage()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterAutonomousR1Tests), nameof(StageTwoGeometryGateContainsNoRasterImage));
    }

    [Fact]
    public void AssetFreeStageCannotPackageAnySceneOrCharacterRaster()
    {
        string project = File.ReadAllText(Path.Combine(HostRoot, "Tcc.DesktopHost.csproj"));
        Assert.DoesNotContain("Assets/StrataR1", project, StringComparison.Ordinal);
        Assert.DoesNotContain("Assets/RebuildR2", project, StringComparison.Ordinal);
        Assert.DoesNotContain("Assets/Character", project, StringComparison.Ordinal);
        Assert.DoesNotContain("Assets/AutonomousR1", project, StringComparison.Ordinal);
    }

    [Fact]
    public void CompositionUsesAControlSpineDominantInstrumentLedgerAndRiskLens()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterAutonomousR1Tests), nameof(CompositionUsesAControlSpineDominantInstrumentLedgerAndRiskLens));
    }

    [Fact]
    public void SceneCorridorIsProtectedBehindTheInterface()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterAutonomousR1Tests), nameof(SceneCorridorIsProtectedBehindTheInterface));
    }

    [Fact]
    public void AllEightOperationalModulesRemainPresent()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterAutonomousR1Tests), nameof(AllEightOperationalModulesRemainPresent));
    }

    [Fact]
    public void RequiredInteractiveAutomationContractRemainsPresent()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterAutonomousR1Tests), nameof(RequiredInteractiveAutomationContractRemainsPresent));
    }

    [Fact]
    public void StrataSystemUsesSemanticTypeSurfaceAndFocusTokens()
    {
        string resources = File.ReadAllText(ResourcePath);
        Assert.Contains("Tcc.Strata.Font.Display", resources, StringComparison.Ordinal);
        Assert.Contains("Tcc.Strata.Font.Body", resources, StringComparison.Ordinal);
        Assert.Contains("Tcc.Strata.Font.Numeric", resources, StringComparison.Ordinal);
        Assert.Contains("Tcc.Strata.Brush.LensFade", resources, StringComparison.Ordinal);
        Assert.Contains("Property=\"IsKeyboardFocused\"", resources, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Autonomous.Brush.LeftScrim", resources, StringComparison.Ordinal);
    }

    private static XElement ByAutomationId(XDocument document, string id) =>
        Assert.Single(document.Descendants(), item => item.Attributes().Any(attribute =>
            attribute.Name.LocalName == "AutomationProperties.AutomationId" && attribute.Value == id));
}
