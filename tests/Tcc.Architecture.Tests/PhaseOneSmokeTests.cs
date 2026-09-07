using System.Reflection;
using System.Runtime.InteropServices;

namespace Tcc.Architecture.Tests;

public sealed class PhaseOneSmokeTests
{
    [Fact]
    public void ApprovedPhaseOneAssembliesLoad()
    {
        Assembly[] assemblies =
        [
            typeof(Tcc.Presentation.Contracts.AssemblyMarker).Assembly,
            typeof(Tcc.Themes.AssemblyMarker).Assembly,
            typeof(Tcc.Features.Themes.AssemblyMarker).Assembly,
            typeof(Tcc.Windows.AssemblyMarker).Assembly,
        ];

        Assert.All(assemblies, assembly => Assert.StartsWith("Tcc.", assembly.GetName().Name));
    }

    [Fact]
    public void TestRunnerUsesApprovedX64Architecture()
    {
        Assert.Equal(
            System.Runtime.InteropServices.Architecture.X64,
            RuntimeInformation.ProcessArchitecture);
    }
}
