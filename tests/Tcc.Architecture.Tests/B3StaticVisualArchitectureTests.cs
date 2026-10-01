using System.Buffers.Binary;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class B3StaticVisualArchitectureTests
{
    private static readonly string Root = FindRoot();
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void B3RuntimeGeometryIsFormallySupersededByReferenceMasterP1()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(B3StaticVisualArchitectureTests), nameof(B3RuntimeGeometryIsFormallySupersededByReferenceMasterP1));
    }

    [Fact]
    public void B3MaterialsRemainHistoricalAndAreNotCurrentRuntimeAuthority()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(B3StaticVisualArchitectureTests), nameof(B3MaterialsRemainHistoricalAndAreNotCurrentRuntimeAuthority));
    }

    [Fact]
    public void HistoricalB3RasterCustodyRemainsIntactAndP3LoadsOnlyTheAcceptedBackground()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(B3StaticVisualArchitectureTests), nameof(HistoricalB3RasterCustodyRemainsIntactAndP3LoadsOnlyTheAcceptedBackground));
    }

    private static string MainWindowXaml() => Path.Combine(Root, "src", "Tcc.DesktopHost", "MainWindow.xaml");

    private static string MigrationReport() => Path.Combine(
        Root, "uiux_cleanroom", "reference_master_rebase", "wpf_p1", "TCC_WPF_P1_LEGACY_GATE_MIGRATION.md");

    private static (int Width, int Height) ReadPngSize(string path)
    {
        byte[] header = File.ReadAllBytes(path)[..24];
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, header[..8]);
        return (
            BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16, 4)),
            BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20, 4)));
    }

    private static string FindRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Tcc.slnx")))
            current = current.Parent;
        return current?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
