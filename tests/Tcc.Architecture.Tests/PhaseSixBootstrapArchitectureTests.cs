using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Fallback;
using Tcc.Windows.Monitors;
using Tcc.Windows.Startup;

namespace Tcc.Architecture.Tests;

public sealed class PhaseSixBootstrapArchitectureTests
{
    private static readonly string HostOutput = Path.Combine(
        RepositoryPaths.Root,
        "src",
        "Tcc.DesktopHost",
        "bin",
        "x64",
        "Release",
        "net10.0-windows");

    // Phase5 identities are sealed at HEAD; Phase6 identities are locked by ADR-0005 §3.
    // This list is deliberately independent of the assembly being inspected.
    private static readonly Dictionary<string, string[]> ApprovedTopLevelTypes =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Tcc.Themes"] = Lines("""
                Tcc.Themes.AssemblyMarker
                Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot
                Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder
                Tcc.Themes.Compatibility.Binding.ThemeCompatibilityManifestMaterializer
                Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind
                Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation
                Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator
                Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityAccessibilityEvidenceV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityCapabilityEvidenceV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityEvidenceV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityMigrationEvidenceV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityRequestV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityRollbackEvidenceV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2
                Tcc.Themes.Compatibility.V2.ThemeCompatibilitySafetyEvidenceV2
                Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot
                Tcc.Themes.Fallback.BuiltInThemePresentationSource
                Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary
                Tcc.Themes.Integrity.ThemeIntegrityVerifier
                Tcc.Themes.Integrity.ThemeMetadataSchemaValidator
                Tcc.Themes.Integrity.ThemePackageInventoryEvaluation
                Tcc.Themes.Integrity.ThemePackageInventoryEvaluator
                Tcc.Themes.Integrity.ThemePackageMetadataEvaluation
                Tcc.Themes.Integrity.ThemePackageMetadataEvaluator
                Tcc.Themes.Integrity.ThemePackageSignatureEvaluation
                Tcc.Themes.Integrity.ThemePackageSignatureEvaluator
                Tcc.Themes.Manifests.ThemeManifestValidator
                """),
            ["Tcc.Windows"] = Lines("""
                Tcc.Windows.AssemblyMarker
                Tcc.Windows.Monitors.WindowMonitorAdapter
                Tcc.Windows.Monitors.WindowMonitorFacts
                Tcc.Windows.Startup.WindowsStartupFacts
                Tcc.Windows.Startup.WindowsStartupMode
                Tcc.Windows.Startup.WindowsStartupPathResolver
                """),
            ["Tcc.DesktopHost"] = Lines("""
                Tcc.DesktopHost.App
                Tcc.DesktopHost.EmbeddedSafeTheme.SafeThemeResourceAdapter
                Tcc.DesktopHost.MainWindow
                Tcc.DesktopHost.MainWindowViewModel
                Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap
                """),
        };

    private static readonly Dictionary<string, string[]> ApprovedNestedProductionTypes =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Tcc.Themes"] =
            [
                "Tcc.Themes.Manifests.ThemeManifestValidator+DiagnosticCodes",
            ],
            ["Tcc.Windows"] =
            [
                "Tcc.Windows.Monitors.WindowMonitorAdapter+NativeMonitorInfo",
                "Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect",
            ],
            ["Tcc.DesktopHost"] = [],
        };

    // These identities are sealed compiler output from approved sources. Exact identity,
    // CompilerGenerated metadata, declaring ownership and generated shape/provenance are
    // all required; a compiler-looking name or attribute alone is never an exemption.
    private static readonly Dictionary<string, string[]> ApprovedCompilerGeneratedTypes =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Tcc.Themes"] = Lines("""
                <>z__ReadOnlyArray`1
                Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot+<CaptureAsync>d__4
                Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder+<VerifyAndBindAsync>d__1
                Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary+<>c
                Tcc.Themes.Integrity.ThemeIntegrityVerifier+<VerifyAsync>d__0
                Tcc.Themes.Integrity.ThemePackageInventoryEvaluator+<EvaluateAsync>d__0
                Tcc.Themes.Integrity.ThemePackageMetadataEvaluator+<EvaluateAsync>d__2
                Tcc.Themes.Manifests.ThemeManifestValidator+<>O
                Tcc.Themes.Manifests.ThemeManifestValidator+<>c
                Tcc.Themes.Manifests.ThemeManifestValidator+<>c__DisplayClass24_0
                Tcc.Themes.Manifests.ThemeManifestValidator+<>c__DisplayClass25_0
                """),
            ["Tcc.Windows"] = [],
            ["Tcc.DesktopHost"] = Lines("""
                Tcc.DesktopHost.App+<OnExit>d__2
                Tcc.DesktopHost.App+<OnStartup>d__1
                """),
        };

    private static readonly Dictionary<string, string[]> ApprovedPhaseSixMembers =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot"] = Lines("""
                C internal(Tcc.Presentation.Contracts.Theme.ThemeVariantId,Tcc.Presentation.Contracts.Theme.ThemeTokenBundle,Tcc.Presentation.Contracts.Theme.ThemeFocusStyles,Tcc.Presentation.Contracts.Theme.ThemeAccessibilityContract)
                P public AccessibilityRequirements:Tcc.Presentation.Contracts.Theme.ThemeAccessibilityContract
                P public FocusStyles:Tcc.Presentation.Contracts.Theme.ThemeFocusStyles
                P public Tokens:Tcc.Presentation.Contracts.Theme.ThemeTokenBundle
                P public VariantId:Tcc.Presentation.Contracts.Theme.ThemeVariantId
                """),
            ["Tcc.Themes.Fallback.BuiltInThemePresentationSource"] = Lines("""
                C public()
                M public GetPresentation(Tcc.Presentation.Contracts.Theme.ThemeVariantId):Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot
                """),
            ["Tcc.Windows.Startup.WindowsStartupMode"] = Lines("""
                F public Installer:Tcc.Windows.Startup.WindowsStartupMode
                F public Portable:Tcc.Windows.Startup.WindowsStartupMode
                F public Unknown:Tcc.Windows.Startup.WindowsStartupMode
                F public value__:System.Int32
                """),
            ["Tcc.Windows.Startup.WindowsStartupFacts"] = Lines("""
                C internal(Tcc.Windows.Startup.WindowsStartupMode,System.String,System.String,System.String)
                P public DeviceStateRoot:System.String
                P public DiagnosticCode:System.String
                P public Mode:Tcc.Windows.Startup.WindowsStartupMode
                P public UserPreferencesRoot:System.String
                """),
            ["Tcc.Windows.Startup.WindowsStartupPathResolver"] = Lines("""
                C public()
                M public Resolve(System.Collections.Generic.IReadOnlyList<System.String>):Tcc.Windows.Startup.WindowsStartupFacts
                """),
            ["Tcc.Windows.Monitors.WindowMonitorFacts"] = Lines("""
                C internal(System.String,System.Int32,System.Int32,System.Int32,System.Int32,System.UInt32)
                M public FitBounds(System.Int32,System.Int32,System.Int32,System.Int32):System.ValueTuple<System.Int32,System.Int32,System.Int32,System.Int32>
                P public DeviceName:System.String
                P public Dpi:System.UInt32
                P public WorkAreaHeight:System.Int32
                P public WorkAreaWidth:System.Int32
                P public WorkAreaX:System.Int32
                P public WorkAreaY:System.Int32
                """),
            ["Tcc.Windows.Monitors.WindowMonitorAdapter"] = Lines("""
                C public()
                M public Capture(System.IntPtr):Tcc.Windows.Monitors.WindowMonitorFacts
                M public EnsureVisible(System.IntPtr):System.Boolean
                """),
            ["Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap"] = Lines("""
                M internal Configure(Microsoft.Extensions.Hosting.HostApplicationBuilder,System.String[]):System.Void
                """),
            ["Tcc.DesktopHost.EmbeddedSafeTheme.SafeThemeResourceAdapter"] = Lines("""
                M internal Create(Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot):System.Windows.ResourceDictionary
                """),
            ["Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect"] = Lines("""
                F internal Bottom:System.Int32
                F internal Left:System.Int32
                F internal Right:System.Int32
                F internal Top:System.Int32
                """),
            ["Tcc.Windows.Monitors.WindowMonitorAdapter+NativeMonitorInfo"] = Lines("""
                F internal DeviceName:System.String
                F internal Flags:System.UInt32
                F internal Monitor:Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect
                F internal Size:System.UInt32
                F internal Work:Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect
                """),
            ["Tcc.DesktopHost.App"] = Lines("""
                C public()
                M protected OnExit(System.Windows.ExitEventArgs):System.Void
                M protected OnStartup(System.Windows.StartupEventArgs):System.Void
                M public InitializeComponent():System.Void
                M public Main():System.Void
                """),
            ["Tcc.DesktopHost.MainWindow"] = Lines("""
                C public(Tcc.DesktopHost.MainWindowViewModel,Tcc.Windows.Monitors.WindowMonitorAdapter)
                M public InitializeComponent():System.Void
                """),
            ["Tcc.DesktopHost.MainWindowViewModel"] = Lines("""
                C public(Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot,System.String)
                P public Presentation:Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot
                P public Status:System.String
                """),
        };

    // ADR-0005 §3 locks parameter names for the listed callable boundary. Keeping this
    // authority separate from the inspected metadata prevents parameter-only mutants
    // from defining their own expected contract.
    private static readonly Dictionary<string, string[]> ApprovedCallableParameterMetadata =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot"] = Lines("""
                C(Tcc.Presentation.Contracts.Theme.ThemeVariantId variantId,Tcc.Presentation.Contracts.Theme.ThemeTokenBundle tokens,Tcc.Presentation.Contracts.Theme.ThemeFocusStyles focusStyles,Tcc.Presentation.Contracts.Theme.ThemeAccessibilityContract accessibilityRequirements)
                """),
            ["Tcc.Themes.Fallback.BuiltInThemePresentationSource"] = Lines("""
                C()
                M GetPresentation(Tcc.Presentation.Contracts.Theme.ThemeVariantId variantId)
                """),
            ["Tcc.Windows.Startup.WindowsStartupFacts"] = Lines("""
                C(Tcc.Windows.Startup.WindowsStartupMode mode,System.String deviceStateRoot,System.String userPreferencesRoot,System.String diagnosticCode)
                """),
            ["Tcc.Windows.Startup.WindowsStartupPathResolver"] = Lines("""
                C()
                M Resolve(System.Collections.Generic.IReadOnlyList<System.String> startupArguments)
                """),
            ["Tcc.Windows.Monitors.WindowMonitorFacts"] = Lines("""
                C(System.String deviceName,System.Int32 workAreaX,System.Int32 workAreaY,System.Int32 workAreaWidth,System.Int32 workAreaHeight,System.UInt32 dpi)
                M FitBounds(System.Int32 x,System.Int32 y,System.Int32 width,System.Int32 height)
                """),
            ["Tcc.Windows.Monitors.WindowMonitorAdapter"] = Lines("""
                C()
                M Capture(System.IntPtr windowHandle)
                M EnsureVisible(System.IntPtr windowHandle)
                """),
            ["Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap"] = Lines("""
                M Configure(Microsoft.Extensions.Hosting.HostApplicationBuilder builder,System.String[] args)
                """),
            ["Tcc.DesktopHost.EmbeddedSafeTheme.SafeThemeResourceAdapter"] = Lines("""
                M Create(Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot presentation)
                """),
            ["Tcc.DesktopHost.App"] = Lines("""
                C()
                M InitializeComponent()
                M Main()
                M OnExit(System.Windows.ExitEventArgs e)
                M OnStartup(System.Windows.StartupEventArgs e)
                """),
            ["Tcc.DesktopHost.MainWindow"] = Lines("""
                C(Tcc.DesktopHost.MainWindowViewModel viewModel,Tcc.Windows.Monitors.WindowMonitorAdapter monitorAdapter)
                M InitializeComponent()
                """),
            ["Tcc.DesktopHost.MainWindowViewModel"] = Lines("""
                C(Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot presentation,System.String startupNotice)
                """),
        };

    private static readonly Dictionary<string, string[]> ApprovedHostInterfaces =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Tcc.DesktopHost.App"] = Lines("""
                System.Windows.Markup.IHaveResources
                System.Windows.Markup.IQueryAmbient
                """),
            ["Tcc.DesktopHost.MainWindow"] = Lines("""
                System.ComponentModel.ISupportInitialize
                System.Windows.IFrameworkInputElement
                System.Windows.IInputElement
                System.Windows.IWindowService
                System.Windows.Markup.IAddChild
                System.Windows.Markup.IComponentConnector
                System.Windows.Markup.IHaveResources
                System.Windows.Markup.IQueryAmbient
                System.Windows.Media.Animation.IAnimatable
                System.Windows.Media.Composition.DUCE+IResource
                """),
        };

    private static readonly HashSet<string> ApprovedPhaseSixPublicTypes =
    [
        "Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot",
        "Tcc.Themes.Fallback.BuiltInThemePresentationSource",
        "Tcc.Windows.Startup.WindowsStartupMode",
        "Tcc.Windows.Startup.WindowsStartupFacts",
        "Tcc.Windows.Startup.WindowsStartupPathResolver",
        "Tcc.Windows.Monitors.WindowMonitorFacts",
        "Tcc.Windows.Monitors.WindowMonitorAdapter",
        "Tcc.DesktopHost.App",
        "Tcc.DesktopHost.MainWindow",
        "Tcc.DesktopHost.MainWindowViewModel",
    ];

    // ADR-0005 §3.3 fixes all native authority independently of the inspected assembly.
    // Every metadata field below is security-relevant: owner, method, library, entry point,
    // calling/marshalling semantics and the complete managed signature must all match.
    private static readonly HashSet<string> ApprovedNativeImports = Lines("""
        assembly=Tcc.Windows|owner=Tcc.Windows.Monitors.WindowMonitorAdapter|access=private|static=True|method=GetDpiForWindow|return=System.UInt32|returnMarshal=None|library=user32.dll|entry=GetDpiForWindow|calling=Winapi|charSet=None|setLastError=False|exactSpelling=False|preserveSig=True|libraryImport=False|parameters=value:System.IntPtr:marshal=None
        assembly=Tcc.Windows|owner=Tcc.Windows.Monitors.WindowMonitorAdapter|access=private|static=True|method=GetMonitorInfo|return=System.Boolean|returnMarshal=Bool|library=user32.dll|entry=GetMonitorInfoW|calling=Winapi|charSet=Unicode|setLastError=False|exactSpelling=False|preserveSig=True|libraryImport=False|parameters=value:System.IntPtr:marshal=None,ref:Tcc.Windows.Monitors.WindowMonitorAdapter+NativeMonitorInfo&:marshal=None
        assembly=Tcc.Windows|owner=Tcc.Windows.Monitors.WindowMonitorAdapter|access=private|static=True|method=GetWindowRect|return=System.Boolean|returnMarshal=Bool|library=user32.dll|entry=GetWindowRect|calling=Winapi|charSet=None|setLastError=False|exactSpelling=False|preserveSig=True|libraryImport=False|parameters=value:System.IntPtr:marshal=None,out:Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect&:marshal=None
        assembly=Tcc.Windows|owner=Tcc.Windows.Monitors.WindowMonitorAdapter|access=private|static=True|method=IsWindow|return=System.Boolean|returnMarshal=Bool|library=user32.dll|entry=IsWindow|calling=Winapi|charSet=None|setLastError=False|exactSpelling=False|preserveSig=True|libraryImport=False|parameters=value:System.IntPtr:marshal=None
        assembly=Tcc.Windows|owner=Tcc.Windows.Monitors.WindowMonitorAdapter|access=private|static=True|method=MonitorFromWindow|return=System.IntPtr|returnMarshal=None|library=user32.dll|entry=MonitorFromWindow|calling=Winapi|charSet=None|setLastError=False|exactSpelling=False|preserveSig=True|libraryImport=False|parameters=value:System.IntPtr:marshal=None,value:System.UInt32:marshal=None
        assembly=Tcc.Windows|owner=Tcc.Windows.Monitors.WindowMonitorAdapter|access=private|static=True|method=SetWindowPos|return=System.Boolean|returnMarshal=Bool|library=user32.dll|entry=SetWindowPos|calling=Winapi|charSet=None|setLastError=False|exactSpelling=False|preserveSig=True|libraryImport=False|parameters=value:System.IntPtr:marshal=None,value:System.IntPtr:marshal=None,value:System.Int32:marshal=None,value:System.Int32:marshal=None,value:System.Int32:marshal=None,value:System.Int32:marshal=None,value:System.UInt32:marshal=None
        """).ToHashSet(StringComparer.Ordinal);

    [Fact]
    public void ActualCompiledProductionSurfaceMatchesApprovedPhaseFiveAndPhaseSixAuthority()
    {
        foreach (Assembly assembly in new[] { typeof(BuiltInThemePresentationSource).Assembly,
                     typeof(WindowsStartupPathResolver).Assembly, LoadHostAssembly() })
        {
            Assert.Empty(GetCompiledBoundaryViolations(assembly, null));
        }
    }

    [Fact]
    public void CompiledMainWindowXamlFieldsExactlyMatchDeclaredNames()
    {
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        string[] declaredNames = XDocument.Load(Path.Combine(
                RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml"))
            .Descendants()
            .Select(element => element.Attribute(xaml + "Name")?.Value)
            .Where(name => name is not null)
            .Cast<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Type mainWindow = LoadHostAssembly().GetType("Tcc.DesktopHost.MainWindow", throwOnError: true)!;
        string[] compiledNames = mainWindow.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(IsGeneratedXamlBackingField)
            .Select(field => field.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(declaredNames, compiledNames);
    }

    [Fact]
    public void ProductionTypesHaveExactAssemblyVisibilityAndDeclaredSurface()
    {
        AssertClass(typeof(BuiltInThemePresentationSnapshot), "Tcc.Themes", isPublic: true, isStatic: false);
        AssertSingleConstructor(typeof(BuiltInThemePresentationSnapshot), isPublic: false,
            typeof(ThemeVariantId), typeof(ThemeTokenBundle), typeof(ThemeFocusStyles), typeof(ThemeAccessibilityContract));
        AssertProperties(typeof(BuiltInThemePresentationSnapshot),
            ("VariantId", typeof(ThemeVariantId)),
            ("Tokens", typeof(ThemeTokenBundle)),
            ("FocusStyles", typeof(ThemeFocusStyles)),
            ("AccessibilityRequirements", typeof(ThemeAccessibilityContract)));
        AssertNoCallableDeclaredMethods(typeof(BuiltInThemePresentationSnapshot));

        AssertClass(typeof(BuiltInThemePresentationSource), "Tcc.Themes", isPublic: true, isStatic: false);
        AssertSingleConstructor(typeof(BuiltInThemePresentationSource), isPublic: true);
        MethodInfo sourceMethod = Assert.Single(CallableDeclaredMethods(typeof(BuiltInThemePresentationSource)));
        Assert.Equal("GetPresentation", sourceMethod.Name);
        Assert.Equal(typeof(BuiltInThemePresentationSnapshot), sourceMethod.ReturnType);
        Assert.Equal([typeof(ThemeVariantId)], sourceMethod.GetParameters().Select(parameter => parameter.ParameterType));

        AssertClass(typeof(WindowsStartupFacts), "Tcc.Windows", isPublic: true, isStatic: false);
        AssertSingleConstructor(typeof(WindowsStartupFacts), isPublic: false,
            typeof(WindowsStartupMode), typeof(string), typeof(string), typeof(string));
        AssertProperties(typeof(WindowsStartupFacts),
            ("Mode", typeof(WindowsStartupMode)),
            ("DeviceStateRoot", typeof(string)),
            ("UserPreferencesRoot", typeof(string)),
            ("DiagnosticCode", typeof(string)));
        AssertNoCallableDeclaredMethods(typeof(WindowsStartupFacts));

        AssertClass(typeof(WindowMonitorFacts), "Tcc.Windows", isPublic: true, isStatic: false);
        AssertSingleConstructor(typeof(WindowMonitorFacts), isPublic: false,
            typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(uint));
        AssertProperties(typeof(WindowMonitorFacts),
            ("DeviceName", typeof(string)),
            ("WorkAreaX", typeof(int)),
            ("WorkAreaY", typeof(int)),
            ("WorkAreaWidth", typeof(int)),
            ("WorkAreaHeight", typeof(int)),
            ("Dpi", typeof(uint)));
        MethodInfo fitBounds = Assert.Single(CallableDeclaredMethods(typeof(WindowMonitorFacts)));
        Assert.Equal("FitBounds", fitBounds.Name);

        AssertConcreteService(typeof(WindowsStartupPathResolver), "Resolve", typeof(WindowsStartupFacts), typeof(IReadOnlyList<string>));
        AssertConcreteService(typeof(WindowMonitorAdapter), "Capture", typeof(WindowMonitorFacts), typeof(nint));
        MethodInfo ensureVisible = Assert.Single(CallableDeclaredMethods(typeof(WindowMonitorAdapter)), method => method.Name == "EnsureVisible");
        Assert.Equal(typeof(bool), ensureVisible.ReturnType);
        Assert.Equal([typeof(nint)], ensureVisible.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void NativeMonitorBoundaryHasExactImportsAndPrivateSequentialBuffers()
    {
        Type adapter = typeof(WindowMonitorAdapter);
        string[] importNames = adapter.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttribute<DllImportAttribute>() is not null)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            ["GetDpiForWindow", "GetMonitorInfo", "GetWindowRect", "IsWindow", "MonitorFromWindow", "SetWindowPos"],
            importNames);
        Assert.All(adapter.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttribute<DllImportAttribute>() is not null), method =>
            Assert.Equal("user32.dll", method.GetCustomAttribute<DllImportAttribute>()!.Value, ignoreCase: true));
        Assert.True(ApprovedNativeImports.SetEquals(
            GetNativeImportIdentities(adapter.Assembly, "Tcc.Windows")));

        Type rectangle = Assert.Single(adapter.GetNestedTypes(BindingFlags.NonPublic), type => type.Name == "NativeRect");
        Type monitorInfo = Assert.Single(adapter.GetNestedTypes(BindingFlags.NonPublic), type => type.Name == "NativeMonitorInfo");
        Assert.True(rectangle.IsNestedPrivate);
        Assert.True(monitorInfo.IsNestedPrivate);
        AssertNativeStructLayout(rectangle, CharSet.Ansi, 16,
            ("Left", typeof(int), 0),
            ("Top", typeof(int), 4),
            ("Right", typeof(int), 8),
            ("Bottom", typeof(int), 12));
        AssertNativeStructLayout(monitorInfo, CharSet.Unicode, 104,
            ("Size", typeof(uint), 0),
            ("Monitor", rectangle, 4),
            ("Work", rectangle, 20),
            ("Flags", typeof(uint), 36),
            ("DeviceName", typeof(string), 40));
        FieldInfo deviceName = monitorInfo.GetField("DeviceName", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MarshalAsAttribute marshal = Assert.IsType<MarshalAsAttribute>(deviceName.GetCustomAttribute<MarshalAsAttribute>());
        Assert.Equal(UnmanagedType.ByValTStr, marshal.Value);
        Assert.Equal(32, marshal.SizeConst);
    }

    [Fact]
    public void HostHasExactInternalOrchestrationAndExistingHostSignatures()
    {
        Assembly host = LoadHostAssembly();
        Type bootstrap = host.GetType("Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap", throwOnError: true)!;
        Type adapter = host.GetType("Tcc.DesktopHost.EmbeddedSafeTheme.SafeThemeResourceAdapter", throwOnError: true)!;
        Assert.True(bootstrap.IsNotPublic && bootstrap.IsAbstract && bootstrap.IsSealed);
        Assert.True(adapter.IsNotPublic && adapter.IsAbstract && adapter.IsSealed);
        Assert.Equal("Configure", Assert.Single(bootstrap.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)).Name);
        Assert.Equal("Create", Assert.Single(
            adapter.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly),
            method => method.IsAssembly).Name);

        Type viewModel = host.GetType("Tcc.DesktopHost.MainWindowViewModel", throwOnError: true)!;
        AssertSingleConstructor(viewModel, isPublic: true, typeof(BuiltInThemePresentationSnapshot), typeof(string));
        AssertProperties(viewModel,
            ("Status", typeof(string)),
            ("Presentation", typeof(BuiltInThemePresentationSnapshot)));

        Type mainWindow = host.GetType("Tcc.DesktopHost.MainWindow", throwOnError: true)!;
        AssertSingleConstructor(mainWindow, isPublic: true, viewModel, typeof(WindowMonitorAdapter));
        Assert.DoesNotContain(
            mainWindow.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => (method.IsPublic || method.IsAssembly)
                && !method.IsSpecialName
                && method.Name is not ("InitializeComponent" or "System.Windows.Markup.IComponentConnector.Connect"));
        Assert.NotNull(mainWindow.GetMethod(
            "InitializeComponent",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));

        Type app = host.GetType("Tcc.DesktopHost.App", throwOnError: true)!;
        Assert.NotNull(app.GetMethod("OnStartup", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        Assert.NotNull(app.GetMethod("OnExit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
    }

    [Fact]
    public void ConfigureCreatesExactRegistrationsAndOneImmutableSelectedSnapshot()
    {
        Assembly host = LoadHostAssembly();
        Assembly hosting = Assembly.LoadFrom(Path.Combine(HostOutput, "Microsoft.Extensions.Hosting.dll"));
        Type hostFactory = hosting.GetType("Microsoft.Extensions.Hosting.Host", throwOnError: true)!;
        object builder = hostFactory.GetMethod("CreateApplicationBuilder", [typeof(string[])])!
            .Invoke(null, [Array.Empty<string>()])!;
        Type bootstrap = host.GetType("Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap", throwOnError: true)!;
        string[] bootstrapArguments = ["--startup-variant=light", "--startup-mode=portable"];
        bootstrap.GetMethod("Configure", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [builder, bootstrapArguments]);

        IEnumerable descriptors = Assert.IsAssignableFrom<IEnumerable>(builder.GetType().GetProperty("Services")!.GetValue(builder));
        List<object> tccDescriptors = [];
        foreach (object descriptor in descriptors)
        {
            Type? serviceType = descriptor.GetType().GetProperty("ServiceType")!.GetValue(descriptor) as Type;
            if (serviceType?.FullName?.StartsWith("Tcc.", StringComparison.Ordinal) == true)
            {
                tccDescriptors.Add(descriptor);
            }
        }

        Assert.Equal(7, tccDescriptors.Count);
        AssertDescriptor(tccDescriptors, typeof(WindowsStartupPathResolver), "Singleton");
        object facts = AssertDescriptor(tccDescriptors, typeof(WindowsStartupFacts), "Singleton");
        Assert.Equal(WindowsStartupMode.Portable, facts.GetType().GetProperty("Mode")!.GetValue(facts));
        object source = AssertDescriptor(tccDescriptors, typeof(BuiltInThemePresentationSource), "Singleton");
        object presentation = AssertDescriptor(tccDescriptors, typeof(BuiltInThemePresentationSnapshot), "Singleton");
        Assert.Equal("light", ((ThemeVariantId)presentation.GetType().GetProperty("VariantId")!.GetValue(presentation)!).Value);
        Assert.Same(presentation, source.GetType().GetMethod("GetPresentation")!.Invoke(source, [new ThemeVariantId("light")]));
        AssertDescriptor(tccDescriptors, typeof(WindowMonitorAdapter), "Transient", requireInstance: false);

        object viewModel = AssertDescriptor(tccDescriptors,
            host.GetType("Tcc.DesktopHost.MainWindowViewModel", throwOnError: true)!, "Singleton");
        Assert.Same(presentation, viewModel.GetType().GetProperty("Presentation")!.GetValue(viewModel));
        AssertDescriptor(tccDescriptors,
            host.GetType("Tcc.DesktopHost.MainWindow", throwOnError: true)!, "Singleton", requireInstance: false);
    }

    [Fact]
    public void HomeWatchlistPresentationStaysLocalHonestAndWithinTheApprovedThemeContract()
    {
        XDocument markup = XDocument.Load(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml"));
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

        string[] requiredAutomationIds =
        [
            "MainContentScroller", "NavHome", "NavMarkets", "NavPlanning", "NavRisk",
            "NavPositions", "NavReview", "NavSettings", "Region.HeaderIdentity",
            "Region.HomeSafetyCore", "Region.MarketOverview", "Region.Watchlist",
            "Region.Priorities", "Region.MentalState", "Region.RecentActivity",
            "Editorial.RightMaxim", "TabBtcUsdt", "TabEthUsdt", "TabSolUsdt",
            "TabBnbUsdt", "TabXrpUsdt", "TabAddSymbol", "Timeframe1H",
            "Timeframe4H", "Timeframe1D", "Timeframe1W", "WindowMinimizeButton",
            "WindowMaximizeRestoreButton", "WindowCloseButton",
        ];
        foreach (string automationId in requiredAutomationIds)
        {
            Assert.Single(markup.Descendants(), element => element.Attributes().Any(attribute =>
                attribute.Name.LocalName == "AutomationProperties.AutomationId" &&
                attribute.Value == automationId));
        }

        string[] disabledControls =
        [
            "NavHome", "NavMarkets", "NavPlanning", "NavRisk", "NavPositions", "NavReview", "NavSettings",
            "TabBtcUsdt", "TabEthUsdt", "TabSolUsdt", "TabBnbUsdt", "TabXrpUsdt", "TabAddSymbol",
            "Timeframe1H", "Timeframe4H", "Timeframe1D", "Timeframe1W",
        ];
        foreach (string automationId in disabledControls)
        {
            XElement control = Assert.Single(markup.Descendants(), element => element.Attributes().Any(attribute =>
                attribute.Name.LocalName == "AutomationProperties.AutomationId" &&
                attribute.Value == automationId));
            Assert.Equal("False", control.Attribute("IsEnabled")?.Value);
        }

        XElement resources = Assert.Single(markup.Descendants(presentation + "Window.Resources"));
        string[] mergedResourceSources = resources.Descendants(presentation + "ResourceDictionary")
            .Select(element => element.Attribute("Source")?.Value)
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
        string[] expectedResourceSources =
        [
            "Resources/StrataObservatory.Design.xaml",
            "Resources/StrataObservatory.Iconography.xaml",
        ];
        Assert.Equal(expectedResourceSources, mergedResourceSources);

        HashSet<string> approvedResources = mergedResourceSources
            .Select(source => XDocument.Load(Path.Combine(
                RepositoryPaths.Root,
                "src",
                "Tcc.DesktopHost",
                source.Replace('/', Path.DirectorySeparatorChar))))
            .SelectMany(document => document.Root!.Elements())
            .Select(element => element.Attribute(xaml + "Key")?.Value)
            .Where(value => value is not null)
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
        approvedResources.UnionWith(resources.Descendants()
            .Select(element => element.Attribute(xaml + "Key")?.Value)
            .Where(value => value is not null)
            .Cast<string>());
        approvedResources.UnionWith([
            "Tcc.BuiltIn.Window", "color.background.base", "color.background.surface",
            "color.text.primary", "color.text.secondary", "focus.ring.color", "focus.ring.thickness",
            "Tcc.BuiltIn.Focus",
        ]);
        IEnumerable<string> dynamicResources = markup.Root!.DescendantsAndSelf()
            .SelectMany(element => element.Attributes())
            .Select(attribute => attribute.Value)
            .Where(value => value.StartsWith("{DynamicResource ", StringComparison.Ordinal))
            .Select(value => value[17..^1]);
        Assert.All(dynamicResources, resource => Assert.Contains(resource, approvedResources));

        XDocument typography = XDocument.Load(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "Resources", "StrataObservatory.Design.xaml"));
        XElement[] fontSetters = typography.Descendants(presentation + "Setter")
            .Where(setter => setter.Attribute("Property")?.Value == "FontSize").ToArray();
        Assert.True(fontSetters.Length >= 3);
        Assert.All(fontSetters, setter => Assert.StartsWith("{DynamicResource Tcc.ReferenceMaster.FontSize.",
            setter.Attribute("Value")?.Value, StringComparison.Ordinal));

        string source = markup.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("UNKNOWN", source, StringComparison.Ordinal);
        Assert.Contains("DATA UNAVAILABLE", source, StringComparison.Ordinal);
        Assert.Contains("Awaiting sync", source, StringComparison.Ordinal);
        Assert.Contains("No priorities loaded", source, StringComparison.Ordinal);
        Assert.Contains("Read-only", source, StringComparison.Ordinal);
        Assert.DoesNotContain("broker", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("automatic trading", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HomeM13AssetDetailsRemainTraceableWhileP3AddsOnlyTheAcceptedBackground()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(PhaseSixBootstrapArchitectureTests), nameof(HomeM13AssetDetailsRemainTraceableWhileP3AddsOnlyTheAcceptedBackground));
    }

    [Fact]
    public void HomeM14ConvergenceAndP3BackgroundUseSharedEvidenceWithoutGeometryDrift()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(PhaseSixBootstrapArchitectureTests), nameof(HomeM14ConvergenceAndP3BackgroundUseSharedEvidenceWithoutGeometryDrift));
    }

    [Fact]
    public void HomeM145AssetsAndP3BackgroundUseTraceableResources()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(PhaseSixBootstrapArchitectureTests), nameof(HomeM145AssetsAndP3BackgroundUseTraceableResources));
    }

    [Fact]
    public void HomeB3ArtworkRemainsHistoricalWhileP3LoadsOnlyTheAcceptedBackground()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(PhaseSixBootstrapArchitectureTests), nameof(HomeB3ArtworkRemainsHistoricalWhileP3LoadsOnlyTheAcceptedBackground));
    }

    [Fact]
    public void HomeB2R2FrozenSceneHasExactApprovedCustody()
    {
        string source = Path.Combine(RepositoryPaths.Root, "automation", "tcc_master_pipeline", "work", "m1_4_6_b2r1", "assets", "m1_4_6_b2r1_locked_scene.png");
        string production = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost", "Assets", "Home", "home-scene-b2r1-locked.png");
        const string expected = "DDEB846F17EA696072B5715F01B4524C5B2B5214F526B75EAF69432B66586A03";
        Assert.Equal(expected, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source))));
        Assert.Equal(expected, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(production))));
    }

    [Fact]
    public void HomeB2R2FormalManifestPreservesLogicalLayerOrder()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "automation", "tcc_master_pipeline", "work", "m1_4_6_b2r2", "m1_4_6_b2r2_layer_manifest.json")));
        JsonElement root = document.RootElement;
        Assert.Equal("PASS", root.GetProperty("status").GetString());
        Assert.Equal(3, root.GetProperty("physical_runtime_layer_count").GetInt32());
        JsonElement logical = root.GetProperty("logical_layers");
        Assert.Equal(10, logical.EnumerateObject().Count());
        Assert.Equal(Enumerable.Range(0, 10).Select(index => $"L{index}"), logical.EnumerateObject().Select(row => row.Name));
        Assert.False(root.GetProperty("visual_order_changed").GetBoolean());
        Assert.False(root.GetProperty("is_hit_test_visible").GetBoolean());
        Assert.False(root.GetProperty("animation").GetBoolean());
    }

    [Fact]
    public void HomeB2R2CoverageHonorsAllEightSelectionCapsAndProtections()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "automation", "tcc_master_pipeline", "work", "m1_4_6_b2r2", "m1_4_6_b2r2_artwork_coverage.json")));
        JsonElement root = document.RootElement;
        Assert.Equal("PASS", root.GetProperty("status").GetString());
        JsonElement[] rows = root.GetProperty("selection_rows").EnumerateArray().ToArray();
        Assert.Equal(Enumerable.Range(101, 8).Select(index => $"S{index}"), rows.Select(row => row.GetProperty("selection_id").GetString()));
        Assert.All(rows, row =>
        {
            Assert.True(row.GetProperty("artwork_coverage_ratio").GetDouble() <= row.GetProperty("maximum_ratio").GetDouble());
            Assert.True(row.GetProperty("actual_opacity").GetDouble() <= row.GetProperty("opacity_limit").GetDouble());
            Assert.Equal(0, row.GetProperty("hard_protection_overlap_px").GetInt32());
            Assert.Equal(0, row.GetProperty("critical_ui_overlap_px").GetInt32());
            Assert.Equal(0, row.GetProperty("readability_violations").GetInt32());
            Assert.False(row.GetProperty("hit_test_visible").GetBoolean());
        });
        Assert.True(root.GetProperty("s108_lower_than_s106_s107").GetBoolean());
        Assert.Equal(0, root.GetProperty("market_overview_core_artwork_px").GetInt32());
    }

    [Fact]
    public void HomeB2R2ProvenanceRecordsOfflineDeterministicDerivation()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "automation", "tcc_master_pipeline", "work", "m1_4_6_b2r2", "m1_4_6_b2r2_scene_provenance.json")));
        JsonElement root = document.RootElement;
        Assert.Equal("PASS", root.GetProperty("status").GetString());
        Assert.Equal(0, root.GetProperty("model_calls").GetInt32());
        Assert.Equal(0, root.GetProperty("image_generation_calls").GetInt32());
        Assert.False(root.GetProperty("art_regeneration").GetBoolean());
        Assert.False(root.GetProperty("character_regeneration").GetBoolean());
        Assert.False(root.GetProperty("manual_dragging").GetBoolean());
        Assert.True(root.GetProperty("production_base").GetProperty("byte_identical").GetBoolean());
    }

    [Theory]
    [InlineData(320d, 240d, "deep")]
    [InlineData(960d, 600d, "deep")]
    [InlineData(1400d, 900d, "deep")]
    [InlineData(320d, 240d, "light")]
    [InlineData(960d, 600d, "light")]
    [InlineData(1400d, 900d, "light")]
    public void BoundDiagnosticAndVariantAreAccessibleAndWrapAtApprovedWindowSizes(
        double width, double height, string selectedVariant)
    {
        string diagnostic = string.Join(" ", Enumerable.Repeat(
            "BOOTSTRAP_VARIANT_INVALID；已使用 Deep。 BOOTSTRAP_MODE_UNKNOWN；儲存路徑目前不可用。", 8));
        Exception? failure = null;
        Thread thread = new(() =>
        {
            object? window = null;
            try
            {
                Assembly host = LoadHostAssembly();
                BuiltInThemePresentationSnapshot presentation = new BuiltInThemePresentationSource()
                    .GetPresentation(new ThemeVariantId(selectedVariant));
                Type viewModelType = host.GetType("Tcc.DesktopHost.MainWindowViewModel", throwOnError: true)!;
                object viewModel = Activator.CreateInstance(viewModelType, presentation, diagnostic)!;
                // testhost.dll owns WPF ResourceAssembly, so generated InitializeComponent
                // cannot resolve the apphost BAML here. Parse the actual product XAML instead.
                XDocument markup = XDocument.Load(Path.Combine(
                    RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml"));
                markup.Root!.Attribute(XName.Get("Class", "http://schemas.microsoft.com/winfx/2006/xaml"))!.Remove();
                foreach (XAttribute fieldModifier in markup.Descendants().Attributes(
                    XName.Get("FieldModifier", "http://schemas.microsoft.com/winfx/2006/xaml")).ToArray())
                    fieldModifier.Remove();
                // XamlReader.Parse has no product code-behind instance after x:Class is removed.
                // Product-level geometry tests separately assert the exact caption-button handlers.
                foreach (XAttribute clickHandler in markup.Descendants().Attributes("Click").ToArray())
                    clickHandler.Remove();
                XNamespace xamlPresentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                // The loose-XAML test has no app ResourceAssembly, so materialize the product's
                // merged dictionaries in the same declared order before invoking XamlReader.
                XElement windowResources = Assert.Single(markup.Descendants(xamlPresentation + "Window.Resources"));
                XElement dictionary = Assert.Single(windowResources.Elements(xamlPresentation + "ResourceDictionary"));
                XElement merged = Assert.Single(dictionary.Elements(xamlPresentation + "ResourceDictionary.MergedDictionaries"));
                XElement[] materializedResources = merged.Elements(xamlPresentation + "ResourceDictionary")
                    .Select(item => item.Attribute("Source")!.Value)
                    .Select(source => XDocument.Load(Path.Combine(
                        RepositoryPaths.Root,
                        "src",
                        "Tcc.DesktopHost",
                        source.Replace('/', Path.DirectorySeparatorChar))))
                    .SelectMany(document => document.Root!.Elements())
                    .Select(element => new XElement(element))
                    .ToArray();
                merged.Remove();
                dictionary.Add(materializedResources);
                string hostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
                foreach (XAttribute assetReference in markup.Descendants()
                    .Attributes()
                    .Where(attribute => attribute.Name.LocalName is "Source" or "UriSource" or "ImageSource")
                    .Where(attribute => attribute.Value.StartsWith("Assets/", StringComparison.Ordinal))
                    .ToArray())
                {
                    assetReference.Value = new Uri(Path.Combine(
                        hostRoot,
                        assetReference.Value.Replace('/', Path.DirectorySeparatorChar))).AbsoluteUri;
                }
                Type xamlReader = Type.GetType("System.Windows.Markup.XamlReader, PresentationFramework", throwOnError: true)!;
                window = xamlReader.GetMethod("Parse", [typeof(string)])!
                    .Invoke(null, [markup.ToString(SaveOptions.DisableFormatting)])!;
                SetProperty(window, "DataContext", viewModel);
                SetProperty(window, "Width", width);
                SetProperty(window, "Height", height);
                Invoke(window, "Show");
                Invoke(window, "UpdateLayout");

                object scrollViewer = FindByAutomationId(window, "MainContentScroller");

                double viewportWidth = (double)GetProperty(scrollViewer, "ViewportWidth");
                double extentWidth = (double)GetProperty(scrollViewer, "ExtentWidth");
                Assert.True(viewportWidth > 0d);
                Assert.True(extentWidth >= viewportWidth - 0.5d);
                bool usesMinimumWidthFallback = extentWidth > viewportWidth + 0.5d;
                Assert.Equal(
                    usesMinimumWidthFallback ? "Visible" : "Collapsed",
                    GetProperty(scrollViewer, "ComputedHorizontalScrollBarVisibility")!.ToString());
                Assert.True((bool)GetProperty(scrollViewer, "Focusable"));
                Assert.True(GetKeyboardNavigationIsTabStop(scrollViewer));

                if (height == 240d && (double)GetProperty(scrollViewer, "ExtentHeight")
                    > (double)GetProperty(scrollViewer, "ViewportHeight"))
                {
                    Assert.True((bool)Invoke(scrollViewer, "Focus")!);
                    double before = (double)GetProperty(scrollViewer, "VerticalOffset");
                    Invoke(scrollViewer, "PageDown");
                    Invoke(window, "UpdateLayout");
                    Assert.True((double)GetProperty(scrollViewer, "VerticalOffset") > before,
                        "Keyboard-focused Home content scroller did not move on PageDown.");
                    Invoke(scrollViewer, "ScrollToTop");
                    Invoke(window, "UpdateLayout");
                    Assert.Equal(0d, (double)GetProperty(scrollViewer, "VerticalOffset"));
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                if (window is not null)
                    Invoke(window, "Close");
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }

    [Theory]
    [InlineData("deep", false)]
    [InlineData("deep", false, "--startup-variant=deep")]
    [InlineData("light", false, "--startup-variant=light")]
    [InlineData("deep", true, "--startup-variant=")]
    [InlineData("deep", true, "--startup-variant=Deep")]
    [InlineData("deep", true, "--startup-variant=unknown")]
    [InlineData("deep", true, "--startup-variant=deep", "--startup-variant=light")]
    public void ConfigureAppliesExactStartupVariantSemantics(
        string expectedVariant,
        bool expectedInvalidNotice,
        params string[] variantArguments)
    {
        List<string> arguments = [.. variantArguments, "--startup-mode=installer"];
        (Assembly Host, List<object> Descriptors) configured = ConfigureHost(arguments.ToArray());
        object presentation = AssertDescriptor(
            configured.Descriptors,
            typeof(BuiltInThemePresentationSnapshot),
            "Singleton");
        object viewModel = AssertDescriptor(
            configured.Descriptors,
            configured.Host.GetType("Tcc.DesktopHost.MainWindowViewModel", throwOnError: true)!,
            "Singleton");

        ThemeVariantId variant = (ThemeVariantId)presentation.GetType().GetProperty("VariantId")!.GetValue(presentation)!;
        string notice = (string)viewModel.GetType().GetProperty("Status")!.GetValue(viewModel)!;
        Assert.Equal(expectedVariant, variant.Value);
        Assert.Equal(expectedInvalidNotice, notice.Contains("BOOTSTRAP_VARIANT_INVALID", StringComparison.Ordinal));
    }

    [Fact]
    public void ResourceProjectionIsDetachedTypedAndDoesNotMutateSnapshot()
    {
        BuiltInThemePresentationSnapshot presentation = new BuiltInThemePresentationSource()
            .GetPresentation(new ThemeVariantId("deep"));
        Assembly host = LoadHostAssembly();
        MethodInfo create = host.GetType("Tcc.DesktopHost.EmbeddedSafeTheme.SafeThemeResourceAdapter", true)!
            .GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic)!;
        object? first = null;
        object? second = null;
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                first = create.Invoke(null, [presentation]);
                second = create.Invoke(null, [presentation]);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
        IDictionary dictionary = Assert.IsAssignableFrom<IDictionary>(first);
        Assert.Equal(14, dictionary.Count);
        object brush = dictionary["color.background.base"]!;
        Assert.Equal("SolidColorBrush", brush.GetType().Name);
        Assert.True((bool)brush.GetType().GetProperty("IsFrozen")!.GetValue(brush)!);
        dictionary["color.background.base"] = new object();
        Assert.Equal("#111827", presentation.Tokens.Tokens["color.background.base"].Value.GetString());
    }

    [Theory]
    [InlineData("deep", "installer")]
    [InlineData("deep", "portable")]
    [InlineData("light", "installer")]
    [InlineData("light", "portable")]
    public void ActualAppHostStartsThroughRealWpfWindow(string variant, string mode)
    {
        string executable = Path.Combine(HostOutput, "Tcc.DesktopHost.exe");
        Assert.True(File.Exists(executable), $"Build the Release x64 apphost first: {executable}");
        ProcessStartInfo startInfo = new(executable)
        {
            WorkingDirectory = HostOutput,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add($"--startup-variant={variant}");
        startInfo.ArgumentList.Add($"--startup-mode={mode}");

        using Process process = Process.Start(startInfo)!;
        try
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(12);
            while (!process.HasExited && process.MainWindowHandle == 0 && DateTime.UtcNow < deadline)
            {
                Thread.Sleep(50);
                process.Refresh();
            }

            Assert.False(process.HasExited, "Apphost exited before creating its real WPF window.");
            Assert.NotEqual(0, process.MainWindowHandle);
            Assert.StartsWith("TCC — Strata Observatory", process.MainWindowTitle, StringComparison.Ordinal);
            Assert.True(process.CloseMainWindow());
            Assert.True(process.WaitForExit(10_000));
            Assert.Equal(0, process.ExitCode);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
            }
        }
    }

    [Fact]
    public void ManifestProjectAndXamlStayInsideTheApprovedBoundary()
    {
        string manifestPath = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost", "app.manifest");
        XDocument manifest = XDocument.Load(manifestPath);
        string text = File.ReadAllText(manifestPath);
        Assert.Contains("asInvoker", text, StringComparison.Ordinal);
        Assert.Contains("uiAccess=\"false\"", text, StringComparison.Ordinal);
        Assert.Contains(">true/pm<", text, StringComparison.Ordinal);
        Assert.Contains(">PerMonitorV2,PerMonitor<", text, StringComparison.Ordinal);
        Assert.Equal("assembly", manifest.Root!.Name.LocalName);

        XDocument project = XDocument.Load(Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost", "Tcc.DesktopHost.csproj"));
        Assert.Equal("app.manifest", Assert.Single(project.Descendants("ApplicationManifest")).Value);

        XDocument appXaml = XDocument.Load(Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost", "App.xaml"));
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        string[] keys = appXaml.Descendants()
            .Attributes(x + "Key")
            .Select(attribute => attribute.Value)
            .ToArray();
        Assert.Equal(["Tcc.BuiltIn.Window", "Tcc.BuiltIn.Text", "Tcc.BuiltIn.Focus"], keys);
    }

    [Theory]
    [MemberData(nameof(CompiledMutants))]
    public void CompiledBoundaryMutantsAreRejected(
        string mutantId,
        string attack,
        string source,
        string projectItems,
        string expectedRejection)
    {
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-83a-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string projectPath = Path.Combine(root, $"{mutantId}.csproj");
            File.WriteAllText(projectPath, CreateMutantProject(mutantId, projectItems));
            File.WriteAllText(Path.Combine(root, "Mutant.cs"), source);
            if (projectItems.Contains("ProjectReference", StringComparison.Ordinal))
            {
                File.WriteAllText(Path.Combine(root, "Unauthorized.Dependency.csproj"),
                    CreateMutantProject("Unauthorized.Dependency", string.Empty));
                File.WriteAllText(Path.Combine(root, "UnauthorizedDependency.cs"),
                    "namespace Unauthorized.Dependency; public sealed class Marker { }");
            }

            ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q"]);
            Assert.True(build.ExitCode == 0,
                $"{mutantId} ({attack}) must compile before its guard is evidence. Exit={build.ExitCode}{Environment.NewLine}{build.Output}");

            string assemblyPath = Path.Combine(root, "bin", "Release", "net10.0-windows", $"{mutantId}.dll");
            Assert.True(File.Exists(assemblyPath), $"Compiled mutant assembly missing: {assemblyPath}");
            Assembly mutant = Assembly.LoadFrom(assemblyPath);
            string[] violations = GetCompiledBoundaryViolations(mutant, XDocument.Load(projectPath));

            Assert.Contains(expectedRejection, violations);
            Assert.True(violations.Length > 0, $"{mutantId} bypassed the compiled architecture guard.");
        }
        finally
        {
            Assert.StartsWith(
                Path.GetFullPath(Path.Combine(Path.GetTempPath(), "tcc-p6-83a-mutants")),
                Path.GetFullPath(root),
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void CompiledBoundaryPositiveControlIsAccepted()
    {
        const string source = """
            namespace Tcc.Themes.Fallback;
            public sealed class BuiltInThemePresentationSource
            {
                private static string HarmlessDetail() => "Home Trading AI";
            }
            """;
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-83a-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string projectPath = Path.Combine(root, "P683ALegalControl.csproj");
            File.WriteAllText(projectPath, CreateMutantProject("P683ALegalControl", string.Empty));
            File.WriteAllText(Path.Combine(root, "LegalControl.cs"), source);

            ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q"]);
            Assert.True(build.ExitCode == 0, build.Output);
            Assembly control = Assembly.LoadFrom(Path.Combine(
                root, "bin", "Release", "net10.0-windows", "P683ALegalControl.dll"));
            Assert.Empty(GetCompiledBoundaryViolations(control, XDocument.Load(projectPath)));
        }
        finally
        {
            Assert.StartsWith(
                Path.GetFullPath(Path.Combine(Path.GetTempPath(), "tcc-p6-83a-mutants")),
                Path.GetFullPath(root),
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("business-public")]
    [InlineData("arbitrary-public")]
    [InlineData("arbitrary-internal")]
    public void FreshUnanticipatedCompiledTypeIsRejectedByTheRealGuard(string shape)
    {
        string name = "Unanticipated" + Guid.NewGuid().ToString("N");
        string declaration = shape == "arbitrary-internal" ? "internal" : "public";
        string body = shape == "business-public"
            ? "public static bool Permitted(decimal exposure, decimal ceiling, bool alert) => exposure < ceiling && !alert;"
            : "private static int Detail() => 1;";
        string source = $"namespace Tcc.Windows.Startup; {declaration} sealed class {name} {{ {body} }}";
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-84r4-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string projectPath = Path.Combine(root, $"{name}.csproj");
        string windowsSourceRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.Windows");
        string linkedProductionSources = string.Join(Environment.NewLine,
            Directory.GetFiles(windowsSourceRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                    && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .Select(path => $"<Compile Include=\"{System.Security.SecurityElement.Escape(path)}\" />"));
        File.WriteAllText(projectPath, CreateMutantProject(name, linkedProductionSources));
        File.WriteAllText(Path.Combine(root, "FreshMutant.cs"), source);
        ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q"]);
        Assert.True(build.ExitCode == 0, build.Output);
        Assembly mutant = Assembly.LoadFrom(Path.Combine(root, "bin", "Release", "net10.0-windows", $"{name}.dll"));
        string[] violations = GetCompiledBoundaryViolations(mutant, XDocument.Load(projectPath), "Tcc.Windows");
        Assert.Contains(shape == "arbitrary-internal"
            ? "unauthorized internal production type" : "unauthorized public production type", violations);
    }

    [Theory]
    [InlineData("N1", "private-class", "WindowsStartupPathResolver")]
    [InlineData("N2", "private-struct", "WindowsStartupPathResolver")]
    [InlineData("N3", "private-interface", "WindowsStartupPathResolver")]
    [InlineData("N4", "private-enum", "WindowsStartupPathResolver")]
    [InlineData("N5", "private-delegate", "WindowsStartupPathResolver")]
    [InlineData("N6", "internal-class", "WindowsStartupPathResolver")]
    [InlineData("N7", "protected-class", "WindowsStartupPathResolver")]
    [InlineData("N8", "public-class", "WindowsStartupPathResolver")]
    [InlineData("N9", "private-policy", "WindowsStartupPathResolver")]
    [InlineData("N10", "private-class", "WindowMonitorAdapter")]
    [InlineData("N11", "protected-internal-class", "WindowsStartupPathResolver")]
    [InlineData("N12", "private-protected-class", "WindowsStartupPathResolver")]
    [InlineData("N13", "compiler-looking", "WindowsStartupPathResolver")]
    public void CompiledProductionCopyRejectsUnauthorizedNestedTypeIdentity(
        string attackId,
        string shape,
        string owner)
    {
        string randomIdentity = (shape == "compiler-looking" ? "GeneratedClosure_" : "QuartzNode_")
            + Guid.NewGuid().ToString("N");
        string nestedDeclaration = shape switch
        {
            "private-class" => $"private sealed class {randomIdentity} {{ }}",
            "private-struct" => $"private struct {randomIdentity} {{ public int Value; }}",
            "private-interface" => $"private interface {randomIdentity} {{ bool Check(int value); }}",
            "private-enum" => $"private enum {randomIdentity} {{ Unknown, Active }}",
            "private-delegate" => $"private delegate bool {randomIdentity}(int value);",
            "internal-class" => $"internal sealed class {randomIdentity} {{ }}",
            "protected-class" => $"protected sealed class {randomIdentity} {{ }}",
            "public-class" => $"public sealed class {randomIdentity} {{ }}",
            "private-policy" => $"private static class {randomIdentity} {{ internal static bool EvaluateQuota(decimal limit, decimal used, decimal proposed) => proposed > 0m && used + proposed <= limit; }}",
            "protected-internal-class" => $"protected internal sealed class {randomIdentity} {{ }}",
            "private-protected-class" => $"private protected sealed class {randomIdentity} {{ }}",
            "compiler-looking" => $"[System.Runtime.CompilerServices.CompilerGenerated] private sealed class {randomIdentity} {{ }}",
            _ => throw new InvalidOperationException(shape),
        };

        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-84r5-nested-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string windowsSourceRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.Windows");
        string[] sourcePaths = Directory.GetFiles(windowsSourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
        string targetPath = Assert.Single(sourcePaths, path => Path.GetFileNameWithoutExtension(path) == owner);
        string original = File.ReadAllText(targetPath);
        string ownerDeclaration = $"public sealed class {owner}\n{{";
        string replacementOwner = shape == "protected-class"
            ? $"public class {owner}\n{{"
            : ownerDeclaration;
        string mutated = original.Replace(
            ownerDeclaration,
            $"{replacementOwner}\n    {nestedDeclaration}",
            StringComparison.Ordinal);
        Assert.NotEqual(original, mutated);

        string mutatedPath = Path.Combine(root, Path.GetFileName(targetPath));
        File.WriteAllText(mutatedPath, mutated);
        string projectItems = string.Join(Environment.NewLine,
            sourcePaths.Select(path => $"<Compile Include=\"{System.Security.SecurityElement.Escape(path == targetPath ? mutatedPath : path)}\" />"));
        string assemblyName = $"NestedMutation{attackId}{Guid.NewGuid():N}";
        string projectPath = Path.Combine(root, $"{assemblyName}.csproj");
        File.WriteAllText(projectPath, CreateMutantProject(assemblyName, projectItems)
            .Replace("<Nullable>enable</Nullable>",
                "<EnableDefaultCompileItems>false</EnableDefaultCompileItems><Nullable>enable</Nullable>", StringComparison.Ordinal));

        ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q"]);
        Assert.True(build.ExitCode == 0,
            $"{attackId} ({shape}) must compile before its guard is evidence. Exit={build.ExitCode}{Environment.NewLine}{build.Output}");
        Assembly mutant = Assembly.LoadFrom(Path.Combine(root, "bin", "Release", "net10.0-windows", $"{assemblyName}.dll"));
        string[] violations = GetCompiledBoundaryViolations(mutant, XDocument.Load(projectPath), "Tcc.Windows");
        string expectedViolation = shape switch
        {
            "internal-class" => "unauthorized internal production type",
            "protected-class" => "unauthorized protected nested production type",
            "public-class" => "unauthorized public production type",
            "protected-internal-class" => "unauthorized protected internal nested production type",
            "private-protected-class" => "unauthorized private protected nested production type",
            _ => "unauthorized private nested production type",
        };
        Assert.Contains(expectedViolation, violations);
        Assert.True(violations.Length > 0, $"{attackId} ({shape}) bypassed the compiled architecture guard.");
    }

    [Theory]
    [InlineData("extra-member", "unauthorized Phase6 member surface")]
    [InlineData("public-constructor", "unauthorized Phase6 member surface")]
    [InlineData("mutable-setter", "unauthorized Phase6 member surface")]
    [InlineData("interface-leak", "unauthorized Phase6 interface authority")]
    [InlineData("nested-visibility", "unauthorized Phase6 type visibility")]
    [InlineData("native-import", "unauthorized native import")]
    [InlineData("native-charset", "unauthorized native buffer layout")]
    [InlineData("native-pack", "unauthorized native buffer layout")]
    [InlineData("native-buffer-shape", "unauthorized native buffer layout")]
    [InlineData("native-marshal", "unauthorized native buffer layout")]
    [InlineData("monitor-state-resolver-private", "global mutable monitor state")]
    [InlineData("monitor-state-resolver-internal", "global mutable monitor state")]
    [InlineData("monitor-state-adapter-public", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-fact", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-fact-resolver", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-list", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-list-resolver", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-enumerable", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-readonly-list", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-immutable-array", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-interface", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-collection", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-dictionary", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-array", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-nested", "global mutable monitor state")]
    [InlineData("monitor-state-readonly-nested-facade", "global mutable monitor state")]
    [InlineData("parameter-name", "unauthorized Phase6 parameter metadata")]
    [InlineData("legal-private-detail", "")]
    [InlineData("legal-readonly-immutable-helper", "")]
    [InlineData("legal-readonly-immutable-array-int", "")]
    public void CompiledProductionCopySurfaceMutationIsRejected(string mutation, string expectedViolation)
    {
        string name = "SurfaceMutation" + Guid.NewGuid().ToString("N");
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-84r4-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string windowsSourceRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.Windows");
        string[] sourcePaths = Directory.GetFiles(windowsSourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)).ToArray();
        string targetFile = mutation is "extra-member" or "interface-leak" or "public-constructor" or "mutable-setter"
            or "parameter-name" or "monitor-state-resolver-private" or "monitor-state-resolver-internal"
            or "monitor-state-readonly-list-resolver" or "monitor-state-readonly-fact-resolver"
            ? "WindowsStartupPathResolver.cs" : "WindowMonitorAdapter.cs";
        string targetPath = Assert.Single(sourcePaths, path => Path.GetFileName(path) == targetFile);
        string source = File.ReadAllText(targetPath);
        source = mutation switch
        {
            "extra-member" => source.Replace("public sealed class WindowsStartupPathResolver\n{",
                "public sealed class WindowsStartupPathResolver\n{\n    public int AdditionalAuthority() => 1;", StringComparison.Ordinal),
            "public-constructor" => source.Replace("internal WindowsStartupFacts(", "public WindowsStartupFacts(", StringComparison.Ordinal),
            "mutable-setter" => source.Replace("public WindowsStartupMode Mode { get; }", "public WindowsStartupMode Mode { get; set; }", StringComparison.Ordinal),
            "interface-leak" => source.Replace("public sealed class WindowsStartupPathResolver\n{",
                "public sealed class WindowsStartupPathResolver : IDisposable\n{\n    public void Dispose() { }", StringComparison.Ordinal),
            "nested-visibility" => source.Replace("private struct NativeRect", "public struct NativeRect", StringComparison.Ordinal),
            "native-import" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    [DllImport(\"kernel32.dll\")] private static extern nint GetCurrentProcess();", StringComparison.Ordinal),
            "native-charset" => source.Replace(
                "[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]",
                "[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]", StringComparison.Ordinal),
            "native-pack" => source.Replace(
                "[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]",
                "[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]", StringComparison.Ordinal),
            "native-buffer-shape" => source.Replace("SizeConst = 32", "SizeConst = 31", StringComparison.Ordinal),
            "native-marshal" => source.Replace("UnmanagedType.ByValTStr", "UnmanagedType.LPWStr", StringComparison.Ordinal),
            "monitor-state-resolver-private" => source.Replace("public sealed class WindowsStartupPathResolver\n{",
                "public sealed class WindowsStartupPathResolver\n{\n    private static Tcc.Windows.Monitors.WindowMonitorFacts? CurrentMonitor;", StringComparison.Ordinal),
            "monitor-state-resolver-internal" => source.Replace("public sealed class WindowsStartupPathResolver\n{",
                "public sealed class WindowsStartupPathResolver\n{\n    internal static Tcc.Windows.Monitors.WindowMonitorFacts? CurrentMonitor;", StringComparison.Ordinal),
            "monitor-state-adapter-public" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    public static WindowMonitorFacts? CurrentMonitor;", StringComparison.Ordinal),
            "monitor-state-readonly-fact" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly WindowMonitorFacts? RetainedMonitor;", StringComparison.Ordinal),
            "monitor-state-readonly-fact-resolver" => source.Replace("public sealed class WindowsStartupPathResolver\n{",
                "public sealed class WindowsStartupPathResolver\n{\n    private static readonly Tcc.Windows.Monitors.WindowMonitorFacts? RetainedMonitor;", StringComparison.Ordinal),
            "monitor-state-readonly-list" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly List<WindowMonitorFacts> RetainedMonitors = new();", StringComparison.Ordinal),
            "monitor-state-readonly-list-resolver" => source.Replace("public sealed class WindowsStartupPathResolver\n{",
                "public sealed class WindowsStartupPathResolver\n{\n    private static readonly List<Tcc.Windows.Monitors.WindowMonitorFacts> RetainedMonitors = new();", StringComparison.Ordinal),
            "monitor-state-readonly-enumerable" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly IEnumerable<WindowMonitorFacts> RetainedMonitors = new List<WindowMonitorFacts>();", StringComparison.Ordinal),
            "monitor-state-readonly-readonly-list" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly IReadOnlyList<WindowMonitorFacts> RetainedMonitors = new List<WindowMonitorFacts>();", StringComparison.Ordinal),
            "monitor-state-readonly-immutable-array" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly System.Collections.Immutable.ImmutableArray<WindowMonitorFacts> RetainedMonitors = System.Collections.Immutable.ImmutableArray<WindowMonitorFacts>.Empty;", StringComparison.Ordinal),
            "monitor-state-readonly-interface" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly IList<WindowMonitorFacts> RetainedMonitors = new List<WindowMonitorFacts>();", StringComparison.Ordinal),
            "monitor-state-readonly-collection" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly System.Collections.ObjectModel.Collection<WindowMonitorFacts> RetainedMonitors = new();", StringComparison.Ordinal),
            "monitor-state-readonly-dictionary" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly Dictionary<string, WindowMonitorFacts> RetainedMonitors = new();", StringComparison.Ordinal),
            "monitor-state-readonly-array" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly WindowMonitorFacts[] RetainedMonitors = new WindowMonitorFacts[1];", StringComparison.Ordinal),
            "monitor-state-readonly-nested" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly Dictionary<string, List<WindowMonitorFacts>> RetainedMonitors = new();", StringComparison.Ordinal),
            "monitor-state-readonly-nested-facade" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly Tuple<string, IReadOnlyCollection<WindowMonitorFacts[]>>? RetainedMonitors;", StringComparison.Ordinal),
            "parameter-name" => source.Replace("startupArguments", "renamedArguments", StringComparison.Ordinal),
            "legal-private-detail" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static string HarmlessDetail() => \"Home Trading AI\";", StringComparison.Ordinal),
            "legal-readonly-immutable-helper" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly string HarmlessDetail = \"presentation only\";", StringComparison.Ordinal),
            "legal-readonly-immutable-array-int" => source.Replace("public sealed class WindowMonitorAdapter\n{",
                "public sealed class WindowMonitorAdapter\n{\n    private static readonly System.Collections.Immutable.ImmutableArray<int> HarmlessValues = System.Collections.Immutable.ImmutableArray<int>.Empty;", StringComparison.Ordinal),
            _ => throw new InvalidOperationException(mutation),
        };
        Assert.NotEqual(File.ReadAllText(targetPath), source);
        string mutatedPath = Path.Combine(root, targetFile);
        File.WriteAllText(mutatedPath, source);
        string projectItems = string.Join(Environment.NewLine,
            sourcePaths.Select(path => $"<Compile Include=\"{System.Security.SecurityElement.Escape(path == targetPath ? mutatedPath : path)}\" />"));
        string projectPath = Path.Combine(root, $"{name}.csproj");
        File.WriteAllText(projectPath, CreateMutantProject(name, projectItems)
            .Replace("<Nullable>enable</Nullable>",
                "<EnableDefaultCompileItems>false</EnableDefaultCompileItems><Nullable>enable</Nullable>", StringComparison.Ordinal));
        ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q"]);
        Assert.True(build.ExitCode == 0, build.Output);
        Assembly mutant = Assembly.LoadFrom(Path.Combine(root, "bin", "Release", "net10.0-windows", $"{name}.dll"));
        string[] violations = GetCompiledBoundaryViolations(mutant, XDocument.Load(projectPath), "Tcc.Windows");
        if (mutation.StartsWith("legal-", StringComparison.Ordinal)) Assert.Empty(violations);
        else Assert.Contains(expectedViolation, violations);
    }

    [Theory]
    [InlineData("viewmodel-public", "unauthorized Phase6 member surface")]
    [InlineData("mainwindow-internal", "unauthorized Phase6 member surface")]
    [InlineData("mainwindow-protected", "unauthorized Phase6 member surface")]
    [InlineData("mainwindow-protected-internal", "unauthorized Phase6 member surface")]
    [InlineData("mainwindow-private-protected", "unauthorized Phase6 member surface")]
    [InlineData("runtime", "unauthorized Host authority dependency")]
    [InlineData("package", "unauthorized Host authority dependency")]
    [InlineData("resolver", "unauthorized Host authority dependency")]
    [InlineData("service-provider", "service locator dependency")]
    [InlineData("service-provider-call", "service locator dependency")]
    [InlineData("home", "unauthorized Phase6 member surface")]
    [InlineData("trading", "unauthorized Phase6 member surface")]
    [InlineData("ai", "unauthorized Phase6 member surface")]
    [InlineData("legal-private-helper", "")]
    public void CompiledHostProductionCopyRejectsUnauthorizedCallableAndOwnershipSurface(
        string mutation,
        string expectedViolation)
    {
        string randomMember = "Authority_" + Guid.NewGuid().ToString("N");
        string viewModelSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindowViewModel.cs"));
        string mainWindowSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs"));
        if (mutation.StartsWith("mainwindow-", StringComparison.Ordinal))
        {
            string accessibility = mutation["mainwindow-".Length..].Replace('-', ' ');
            mainWindowSource = mainWindowSource.Replace("public partial class MainWindow : Window\n{",
                $"public partial class MainWindow : Window\n{{\n    {accessibility} int {randomMember}() => 1;",
                StringComparison.Ordinal);
        }
        else
        {
            string declaration = mutation switch
            {
                "viewmodel-public" => $"public int {randomMember}() => 1;",
                "runtime" => $"internal Tcc.Presentation.Contracts.Theme.IThemeRuntime? {randomMember}() => null;",
                "package" => $"internal Tcc.Presentation.Contracts.Theme.IThemePackage? {randomMember}() => null;",
                "resolver" => $"internal Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2? {randomMember}() => null;",
                "service-provider" => "private System.IServiceProvider? _unauthorizedLocator;",
                "service-provider-call" => $"private object? {randomMember}() => ((System.IServiceProvider)null!).GetService(typeof(object));",
                "home" => $"internal bool Home{randomMember}() => true;",
                "trading" => $"protected bool Trading{randomMember}() => true;",
                "ai" => $"private protected bool AI{randomMember}() => true;",
                "legal-private-helper" => $"private string {randomMember}() => \"presentation only\";",
                _ => throw new InvalidOperationException(mutation),
            };
            viewModelSource = mutation is "trading" or "ai"
                ? viewModelSource.Replace("public sealed class MainWindowViewModel", "public class MainWindowViewModel", StringComparison.Ordinal)
                : viewModelSource;
            viewModelSource = viewModelSource.Replace("public sealed class MainWindowViewModel\n{",
                $"public sealed class MainWindowViewModel\n{{\n    {declaration}", StringComparison.Ordinal);
            if (mutation is "trading" or "ai")
            {
                viewModelSource = viewModelSource.Replace("public class MainWindowViewModel\n{",
                    $"public class MainWindowViewModel\n{{\n    {declaration}", StringComparison.Ordinal);
            }
        }

        (Assembly Assembly, AssemblyLoadContext Context) compiled = CompileHostProductionCopy(
            viewModelSource, mainWindowSource, mutation);
        try
        {
            string[] violations = GetCompiledBoundaryViolations(compiled.Assembly, null, "Tcc.DesktopHost");
            if (mutation == "legal-private-helper") Assert.Empty(violations);
            else Assert.Contains(expectedViolation, violations);
        }
        finally
        {
            compiled.Context.Unload();
        }
    }

    [Theory]
    [InlineData("async-resolver", "unauthorized Host authority dependency")]
    [InlineData("async-runtime-package", "unauthorized Host authority dependency")]
    [InlineData("generic-runtime", "unauthorized Host authority dependency")]
    [InlineData("generic-package", "unauthorized Host authority dependency")]
    [InlineData("generic-nested-runtime", "unauthorized Host authority dependency")]
    [InlineData("generic-package-array", "unauthorized Host authority dependency")]
    [InlineData("generic-resolver", "unauthorized Host authority dependency")]
    [InlineData("async-service-locator", "service locator dependency")]
    [InlineData("legal-generated-body", "")]
    [InlineData("legal-generic-int", "")]
    [InlineData("legal-generic-presentation", "")]
    public void CompiledAsyncHostBodyIsInspectedThroughItsApprovedOrigin(
        string mutation,
        string expectedViolation)
    {
        string appSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "App.xaml.cs"));
        string injectedBody = mutation switch
        {
            "async-resolver" => "GC.KeepAlive(new Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2());",
            "async-runtime-package" => "GC.KeepAlive(typeof(Tcc.Presentation.Contracts.Theme.IThemeRuntime));\n        GC.KeepAlive(typeof(Tcc.Presentation.Contracts.Theme.IThemePackage));",
            "generic-runtime" => "GC.KeepAlive(System.Runtime.CompilerServices.Unsafe.SizeOf<Tcc.Presentation.Contracts.Theme.IThemeRuntime>());",
            "generic-package" => "GC.KeepAlive(Array.Empty<Tcc.Presentation.Contracts.Theme.IThemePackage>());",
            "generic-nested-runtime" => "GC.KeepAlive(Array.Empty<List<Tcc.Presentation.Contracts.Theme.IThemeRuntime>>());",
            "generic-package-array" => "GC.KeepAlive(Array.Empty<Tcc.Presentation.Contracts.Theme.IThemePackage[]>());",
            "generic-resolver" => "GC.KeepAlive(Array.Empty<Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2>());",
            "async-service-locator" => "_ = ((System.IServiceProvider)null!).GetService(typeof(object));",
            "legal-generated-body" => "GC.KeepAlive(typeof(string));",
            "legal-generic-int" => "GC.KeepAlive(Array.Empty<int>());",
            "legal-generic-presentation" => "GC.KeepAlive(Array.Empty<Tcc.Presentation.Contracts.Theme.ThemeVariantId>());",
            _ => throw new InvalidOperationException(mutation),
        };
        appSource = appSource.Replace(
            "        base.OnStartup(e);",
            $"        base.OnStartup(e);\n        {injectedBody}",
            StringComparison.Ordinal);
        Assert.NotEqual(File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "App.xaml.cs")), appSource);

        string viewModelSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindowViewModel.cs"));
        string mainWindowSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs"));
        (Assembly Assembly, AssemblyLoadContext Context) compiled = CompileHostProductionCopy(
            viewModelSource, mainWindowSource, mutation, appSource);
        try
        {
            string[] violations = GetCompiledBoundaryViolations(compiled.Assembly, null, "Tcc.DesktopHost");
            if (mutation.StartsWith("legal-", StringComparison.Ordinal)) Assert.Empty(violations);
            else Assert.Contains(expectedViolation, violations);
        }
        finally
        {
            compiled.Context.Unload();
        }
    }

    [Theory]
    [InlineData("on-exit")]
    [InlineData("other-lifecycle")]
    [InlineData("duplicate-on-startup")]
    [InlineData("wrong-service")]
    [InlineData("generated-helper")]
    [InlineData("different-extension")]
    public void ServiceResolutionAuthorizationPreservesOriginCallsiteAndCardinality(string mutation)
    {
        string originalAppSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "App.xaml.cs"));
        string appSource = mutation switch
        {
            "on-exit" => originalAppSource.Replace(
                "            await _host.StopAsync();",
                "            await _host.StopAsync();\n            GC.KeepAlive(_host.Services.GetRequiredService<MainWindow>());",
                StringComparison.Ordinal),
            "other-lifecycle" => originalAppSource.Replace(
                "    protected override async void OnExit(ExitEventArgs e)",
                "    protected override void OnActivated(EventArgs e)\n    {\n        base.OnActivated(e);\n        GC.KeepAlive(_host!.Services.GetRequiredService<MainWindow>());\n    }\n\n    protected override async void OnExit(ExitEventArgs e)",
                StringComparison.Ordinal),
            "duplicate-on-startup" => originalAppSource.Replace(
                "            MainWindow = _host.Services.GetRequiredService<MainWindow>();",
                "            MainWindow = _host.Services.GetRequiredService<MainWindow>();\n            GC.KeepAlive(_host.Services.GetRequiredService<MainWindow>());",
                StringComparison.Ordinal),
            "wrong-service" => originalAppSource.Replace(
                "            MainWindow = _host.Services.GetRequiredService<MainWindow>();",
                "            MainWindow = _host.Services.GetRequiredService<MainWindow>();\n            GC.KeepAlive(_host.Services.GetRequiredService<MainWindowViewModel>());",
                StringComparison.Ordinal),
            "generated-helper" => originalAppSource.Replace(
                "    protected override async void OnExit(ExitEventArgs e)",
                "    private async Task ResolveFromGeneratedHelperAsync()\n    {\n        await Task.Yield();\n        GC.KeepAlive(_host!.Services.GetRequiredService<MainWindow>());\n    }\n\n    protected override async void OnExit(ExitEventArgs e)",
                StringComparison.Ordinal),
            "different-extension" => originalAppSource.Replace(
                "            MainWindow = _host.Services.GetRequiredService<MainWindow>();",
                "            MainWindow = _host.Services.GetRequiredService<MainWindow>();\n            GC.KeepAlive(ServiceProviderServiceExtensions.GetService<MainWindow>(_host.Services));",
                StringComparison.Ordinal),
            _ => throw new InvalidOperationException(mutation),
        };
        Assert.NotEqual(originalAppSource, appSource);

        string viewModelSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindowViewModel.cs"));
        string mainWindowSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs"));
        (Assembly Assembly, AssemblyLoadContext Context) compiled = CompileHostProductionCopy(
            viewModelSource, mainWindowSource, "service-provenance-" + mutation, appSource);
        try
        {
            string[] violations = GetCompiledBoundaryViolations(compiled.Assembly, null, "Tcc.DesktopHost");
            Assert.Contains("service locator dependency", violations);
        }
        finally
        {
            compiled.Context.Unload();
        }
    }

    [Fact]
    public void CurrentHostHasExactlyOneApprovedStartupResolutionPath()
    {
        string[] violations = GetCompiledBoundaryViolations(LoadHostAssembly(), null, "Tcc.DesktopHost");
        Assert.DoesNotContain("service locator dependency", violations);
    }

    [Fact]
    public void FreshRandomGuardFamilyVariantsAreRejectedThreeOfThree()
    {
        string randomIdentity = "Quartz_" + Guid.NewGuid().ToString("N");
        string originalAppSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "App.xaml.cs"));
        string viewModelSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindowViewModel.cs"));
        string mainWindowSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs"));

        string genericAppSource = originalAppSource.Replace(
            "        base.OnStartup(e);",
            $"        base.OnStartup(e);\n        object {randomIdentity} = Array.Empty<Dictionary<Guid, List<Tcc.Presentation.Contracts.Theme.IThemeRuntime[]>>>();\n        GC.KeepAlive({randomIdentity});",
            StringComparison.Ordinal);
        (Assembly Assembly, AssemblyLoadContext Context) generic = CompileHostProductionCopy(
            viewModelSource, mainWindowSource, "fresh-generic-" + randomIdentity, genericAppSource);
        try
        {
            Assert.Contains("unauthorized Host authority dependency",
                GetCompiledBoundaryViolations(generic.Assembly, null, "Tcc.DesktopHost"));
        }
        finally
        {
            generic.Context.Unload();
        }

        string serviceAppSource = originalAppSource.Replace(
            "    protected override async void OnExit(ExitEventArgs e)",
            $"    protected override void OnActivated(EventArgs e)\n    {{\n        base.OnActivated(e);\n        object? {randomIdentity} = ServiceProviderServiceExtensions.GetService<MainWindow>(_host!.Services);\n        GC.KeepAlive({randomIdentity});\n    }}\n\n    protected override async void OnExit(ExitEventArgs e)",
            StringComparison.Ordinal);
        (Assembly Assembly, AssemblyLoadContext Context) service = CompileHostProductionCopy(
            viewModelSource, mainWindowSource, "fresh-service-" + randomIdentity, serviceAppSource);
        try
        {
            Assert.Contains("service locator dependency",
                GetCompiledBoundaryViolations(service.Assembly, null, "Tcc.DesktopHost"));
        }
        finally
        {
            service.Context.Unload();
        }

        (Assembly Assembly, AssemblyLoadContext Context) monitor = CompileWindowsProductionCopy(
            "WindowsStartupPathResolver.cs",
            "fresh-monitor-" + randomIdentity,
            source => source.Replace(
                "public sealed class WindowsStartupPathResolver\n{",
                $"public sealed class WindowsStartupPathResolver\n{{\n    private static readonly Tuple<Guid, IReadOnlyDictionary<string, Tcc.Windows.Monitors.WindowMonitorFacts[]>>? {randomIdentity};",
                StringComparison.Ordinal));
        try
        {
            Assert.Contains("global mutable monitor state",
                GetCompiledBoundaryViolations(monitor.Assembly, null, "Tcc.Windows"));
        }
        finally
        {
            monitor.Context.Unload();
        }
    }

    [Theory]
    [InlineData("N1-runtime", "private static int ConstraintRuntime<T>() where T : Tcc.Presentation.Contracts.Theme.IThemeRuntime => 71;", true)]
    [InlineData("N2-package", "private static int ConstraintPackage<T>() where T : Tcc.Presentation.Contracts.Theme.IThemePackage => 71;", true)]
    [InlineData("N3-resolver-graph", "private static int ConstraintResolver<T>() where T : IEnumerable<Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2> => 71;", true)]
    [InlineData("N4-multiple", "private static int ConstraintMultiple<T>() where T : class, Tcc.Presentation.Contracts.Theme.IThemeRuntime, IDisposable => 71;", true)]
    [InlineData("N5-unused-parameter", "private static string ConstraintUnused<TValue, TAuthority>(TValue value) where TAuthority : Tcc.Presentation.Contracts.Theme.IThemeRuntime => value?.ToString() ?? string.Empty;", true)]
    [InlineData("P1-class", "private static int ConstraintClass<T>() where T : class => 71;", false)]
    [InlineData("P2-harmless", "private static int ConstraintHarmless<T>() where T : IDisposable => 71;", false)]
    [InlineData("P3-presentation", "private static int ConstraintPresentation<T>() where T : IEnumerable<Tcc.Presentation.Contracts.Theme.ThemeVariantId> => 71;", false)]
    public void DeclaredMethodGenericParametersAreIndependentAuthorityRoots(
        string control,
        string declaration,
        bool expectRejection)
    {
        (Assembly Assembly, AssemblyLoadContext Context) compiled = CompileHostConstraintControl(
            control,
            declaration);
        try
        {
            Assert.Equal("Tcc.DesktopHost", compiled.Assembly.GetName().Name);
            string[] violations = GetCompiledBoundaryViolations(compiled.Assembly, null);
            if (expectRejection)
                Assert.Contains("unauthorized Host authority dependency", violations);
            else
                Assert.Empty(violations);
        }
        finally
        {
            compiled.Context.Unload();
        }
    }

    [Fact]
    public void FreshRandomConstraintOnlyVariantsAreRejectedTwoOfTwo()
    {
        string firstMethod = "ConstraintProbe_" + Guid.NewGuid().ToString("N");
        string firstParameter = "TAuthority_" + Guid.NewGuid().ToString("N");
        string secondMethod = "ConstraintCarrier_" + Guid.NewGuid().ToString("N");
        string secondParameter = "TPackage_" + Guid.NewGuid().ToString("N");
        string[] declarations =
        [
            $"private static int {firstMethod}<{firstParameter}>() where {firstParameter} : Tcc.Presentation.Contracts.Theme.IThemeRuntime, IDisposable => 71;",
            $"private static int {secondMethod}<{secondParameter}>() where {secondParameter} : IReadOnlyDictionary<string, IReadOnlyCollection<Tcc.Presentation.Contracts.Theme.IThemePackage>> => 71;",
        ];

        foreach ((string declaration, int index) in declarations.Select((value, index) => (value, index)))
        {
            (Assembly Assembly, AssemblyLoadContext Context) compiled = CompileHostConstraintControl(
                $"fresh-constraint-{index}-{Guid.NewGuid():N}",
                declaration);
            try
            {
                Assert.Equal("Tcc.DesktopHost", compiled.Assembly.GetName().Name);
                Assert.Contains("unauthorized Host authority dependency",
                    GetCompiledBoundaryViolations(compiled.Assembly, null));
            }
            finally
            {
                compiled.Context.Unload();
            }
        }
    }

    [Theory]
    [InlineData("P1")]
    [InlineData("P2")]
    [InlineData("P3")]
    [InlineData("P4")]
    [InlineData("P5")]
    [InlineData("P6")]
    [InlineData("P7")]
    [InlineData("P8")]
    [InlineData("P9")]
    [InlineData("P10")]
    [InlineData("P11")]
    [InlineData("P12")]
    [InlineData("P13")]
    public void CompiledNativeImportMutantsAreRejectedByGlobalExactAuthority(string attackId)
    {
        string randomMethod = "NativeProbe_" + Guid.NewGuid().ToString("N");
        string randomEntryPoint = "NativeEntry_" + Guid.NewGuid().ToString("N");
        string assemblyName = attackId switch
        {
            "P4" => "Tcc.DesktopHost",
            "P5" => "Tcc.Themes",
            _ => "Tcc.Windows",
        };
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-84r6-native-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string projectPath = Path.Combine(root, $"NativeMutation{attackId}.csproj");

        if (assemblyName != "Tcc.Windows")
        {
            string owner = assemblyName == "Tcc.DesktopHost"
                ? "Tcc.DesktopHost.MainWindowViewModel"
                : "Tcc.Themes.Fallback.BuiltInThemePresentationSource";
            int separator = owner.LastIndexOf('.');
            string source = $$"""
                using System.Runtime.InteropServices;
                namespace {{owner[..separator]}};
                public sealed class {{owner[(separator + 1)..]}}
                {
                    [DllImport("{{(attackId == "P4" ? "user32.dll" : "kernel32.dll")}}", EntryPoint = "{{randomEntryPoint}}")]
                    private static extern nint {{randomMethod}}();
                }
                """;
            File.WriteAllText(Path.Combine(root, "NativeMutation.cs"), source);
            File.WriteAllText(projectPath, CreateMutantProject(assemblyName, string.Empty));
        }
        else
        {
            string windowsSourceRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.Windows");
            string[] sourcePaths = Directory.GetFiles(windowsSourceRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                    && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .ToArray();
            string targetFile = attackId switch
            {
                "P1" or "P2" or "P10" or "P11" or "P13" => "WindowsStartupPathResolver.cs",
                _ => "WindowMonitorAdapter.cs",
            };
            string targetPath = Assert.Single(sourcePaths, path => Path.GetFileName(path) == targetFile);
            string original = File.ReadAllText(targetPath);
            string mutated = CreateNativeMutation(attackId, original, randomMethod, randomEntryPoint);
            Assert.NotEqual(original, mutated);
            string mutatedPath = Path.Combine(root, targetFile);
            File.WriteAllText(mutatedPath, mutated);
            string projectItems = string.Join(Environment.NewLine,
                sourcePaths.Select(path => $"<Compile Include=\"{System.Security.SecurityElement.Escape(path == targetPath ? mutatedPath : path)}\" />"));
            string projectText = CreateMutantProject(assemblyName, projectItems)
                .Replace("<Nullable>enable</Nullable>",
                    "<EnableDefaultCompileItems>false</EnableDefaultCompileItems><Nullable>enable</Nullable>", StringComparison.Ordinal);
            if (attackId == "P13")
                projectText = projectText.Replace("<Nullable>enable</Nullable>",
                    "<AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>", StringComparison.Ordinal);
            File.WriteAllText(projectPath, projectText);
        }

        ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q"]);
        Assert.True(build.ExitCode == 0,
            $"{attackId} must compile before its guard is evidence. Exit={build.ExitCode}{Environment.NewLine}{build.Output}");
        string assemblyPath = Path.Combine(root, "bin", "Release", "net10.0-windows", $"{assemblyName}.dll");
        Assert.True(File.Exists(assemblyPath), $"Compiled native mutant assembly missing: {assemblyPath}");
        AssemblyLoadContext loadContext = new($"native-{attackId}-{Guid.NewGuid():N}", isCollectible: true);
        Assembly mutant = loadContext.LoadFromAssemblyPath(assemblyPath);
        string[] violations = GetCompiledBoundaryViolations(mutant, XDocument.Load(projectPath));
        Assert.Contains("unauthorized native import", violations);
        Assert.True(violations.Length > 0, $"{attackId} bypassed global native authority.");
    }

    [Fact]
    public void FreshRandomNativeAttacksAndCountPreservingReplacementAreRejected()
    {
        string[] attacks = ["P3", "P4", "P9", "P12"];
        foreach (string attack in attacks)
        {
            CompiledNativeImportMutantsAreRejectedByGlobalExactAuthority(attack);
        }
    }

    [Theory]
    [InlineData("builder.Services.AddSingleton<WindowMonitorAdapter>();", "wrong DI lifetime")]
    [InlineData("builder.Services.AddTransient<WindowMonitorAdapter>(_ => new WindowMonitorAdapter());", "wrong DI descriptor form")]
    [InlineData("builder.Services.AddTransient<WindowMonitorAdapter>(new Func<IServiceProvider, WindowMonitorAdapter>(_ => new WindowMonitorAdapter()));", "wrong DI descriptor form")]
    public void CompiledRealBootstrapRegistrationMutationIsRejected(
        string replacementRegistration,
        string expectedViolation)
    {
        string name = "RegistrationMutation" + Guid.NewGuid().ToString("N");
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-84r4-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string original = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost",
            "ThemeBootstrap", "ThemeBootstrap.cs"));
        string mutated = original.Replace("builder.Services.AddTransient<WindowMonitorAdapter>();",
            replacementRegistration, StringComparison.Ordinal);
        Assert.NotEqual(original, mutated);
        File.WriteAllText(Path.Combine(root, "ThemeBootstrap.cs"), mutated);
        File.WriteAllText(Path.Combine(root, "HostStubs.cs"), """
            namespace Tcc.DesktopHost;
            public sealed class MainWindowViewModel
            {
                public MainWindowViewModel(Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot presentation, string notice) { }
            }
            public sealed class MainWindow { }
            """);
        File.WriteAllText(Path.Combine(root, "Program.cs"), """
            using Microsoft.Extensions.Hosting;

            HostApplicationBuilder builder = Host.CreateApplicationBuilder([]);
            Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap.Configure(
                builder, ["--startup-mode=installer"]);
            foreach (var descriptor in builder.Services.Where(item =>
                         item.ServiceType.FullName?.StartsWith("Tcc.", StringComparison.Ordinal) == true))
            {
                Console.WriteLine($"{descriptor.ServiceType.FullName}|{descriptor.Lifetime}|{descriptor.ImplementationType?.FullName ?? "<null>"}|{descriptor.ImplementationInstance?.GetType().FullName ?? "<null>"}|{descriptor.ImplementationFactory is not null}|{descriptor.IsKeyedService}|{descriptor.ServiceKey?.ToString() ?? "<null>"}");
            }
            """);
        string[] projects = ["Tcc.Presentation.Contracts", "Tcc.Themes", "Tcc.Windows"];
        string items = "<PackageReference Include=\"Microsoft.Extensions.Hosting\" Version=\"10.0.11\" />"
            + string.Join(string.Empty, projects
                .Select(project => $"<ProjectReference Include=\"{System.Security.SecurityElement.Escape(Path.Combine(RepositoryPaths.Root, "src", project, project + ".csproj"))}\" />"));
        string projectPath = Path.Combine(root, $"{name}.csproj");
        File.WriteAllText(projectPath, CreateMutantProject(name, items)
            .Replace("<TargetFramework>", "<OutputType>Exe</OutputType><TargetFramework>", StringComparison.Ordinal));
        ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q"]);
        Assert.True(build.ExitCode == 0, build.Output);
        ProcessResult execution = RunProcess("dotnet", ["run", "--project", projectPath, "-c", "Release", "--no-build", "--no-restore"]);
        Assert.True(execution.ExitCode == 0, execution.Output);
        Assert.Contains(expectedViolation, GetRegistrationViolations(execution.Output.Split(Environment.NewLine)));
    }

    [Fact]
    public void ActualBootstrapSourceHasNoForbiddenAuthorityOrContainerBypass()
    {
        string source = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "ThemeBootstrap", "ThemeBootstrap.cs"));
        Assert.DoesNotContain("BuildServiceProvider", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IServiceProvider", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeCompatibility", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeIntegrity", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IThemeRuntime", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IThemePackage", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ResourceDictionary.Source", source, StringComparison.Ordinal);
    }

    public static IEnumerable<object[]> CompiledMutants()
    {
        yield return Mutant("M01", "extra public production type", """
            namespace Tcc.Themes.Fallback;
            public sealed class ExtraPresentationAuthority { }
            """, "unauthorized public production type");
        yield return Mutant("M02", "extra internal production type", """
            namespace Tcc.Themes.Fallback;
            internal sealed class HiddenPresentationAuthority { }
            """, "unauthorized internal production type");
        yield return Mutant("M03", "extra public/internal member", """
            namespace Tcc.Themes.Fallback;
            public sealed class BuiltInThemePresentationSource { public void ExtraMember() { } }
            """, "extra production member");
        yield return Mutant("M04", "public snapshot constructor", """
            namespace Tcc.Themes.Fallback;
            public sealed class BuiltInThemePresentationSnapshot { public BuiltInThemePresentationSnapshot() { } }
            """, "public snapshot constructor");
        yield return Mutant("M05", "init setter / mutable snapshot", """
            namespace Tcc.Themes.Fallback;
            public sealed class BuiltInThemePresentationSnapshot { internal BuiltInThemePresentationSnapshot() { } public string VariantId { get; init; } = "deep"; }
            """, "mutable snapshot setter");
        yield return Mutant("M06", "mutable dictionary leakage", """
            namespace Tcc.Themes.Fallback;
            public sealed class BuiltInThemePresentationSnapshot { internal BuiltInThemePresentationSnapshot() { } public Dictionary<string, string> Tokens { get; } = []; }
            """, "mutable dictionary leakage");
        yield return Mutant("M07", "theme presentation package/path injection", """
            namespace Tcc.Themes.Fallback;
            public sealed class BuiltInThemePresentationSource { public BuiltInThemePresentationSource(string packagePath) { } }
            """, "theme path/package injection");
        yield return Mutant("M08", "IThemeRuntime implementation", """
            namespace Tcc.Presentation.Contracts.Theme { public interface IThemeRuntime { } }
            namespace Tcc.Themes.Fallback { public sealed class BuiltInThemePresentationSource : Tcc.Presentation.Contracts.Theme.IThemeRuntime { } }
            """, "runtime/package authority");
        yield return Mutant("M09", "IThemePackage implementation", """
            namespace Tcc.Presentation.Contracts.Theme { public interface IThemePackage { } }
            namespace Tcc.Themes.Fallback { public sealed class BuiltInThemePresentationSource : Tcc.Presentation.Contracts.Theme.IThemePackage { } }
            """, "runtime/package authority");
        yield return Mutant("M10", "compatibility/evidence receipt construction leakage", """
            namespace Tcc.Themes.Compatibility.V2 { public sealed class ThemeCompatibilityEvidenceV2 { } public sealed class ThemeCompatibilityReceiptV2 { } }
            namespace Tcc.DesktopHost { public sealed class MainWindowViewModel { public MainWindowViewModel(Tcc.Themes.Compatibility.V2.ThemeCompatibilityEvidenceV2 evidence) { Receipt = new(); } public Tcc.Themes.Compatibility.V2.ThemeCompatibilityReceiptV2 Receipt { get; } } }
            """, "compatibility/evidence construction leakage");
        yield return Mutant("M11", "ViewModel ThemeCompatibilityResolverV2 dependency", """
            namespace Tcc.Themes.Compatibility.V2 { public sealed class ThemeCompatibilityResolverV2 { } }
            namespace Tcc.DesktopHost { public sealed class MainWindowViewModel { public MainWindowViewModel(Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2 resolver) { } } }
            """, "view-model resolver dependency");
        yield return Mutant("M12", "ViewModel IServiceProvider dependency", """
            namespace Tcc.DesktopHost;
            public sealed class MainWindowViewModel { public MainWindowViewModel(IServiceProvider services) { } }
            """, "service locator dependency");
        yield return Mutant("M14", "global/static mutable monitor", """
            namespace Tcc.Windows.Monitors;
            public sealed class WindowMonitorFacts { }
            public static class MonitorState { public static WindowMonitorFacts? Current; }
            """, "global mutable monitor state");
        yield return Mutant("M15", "external Theme ResourceDictionary URI", """
            namespace Tcc.DesktopHost.EmbeddedSafeTheme;
            internal static class SafeThemeResourceAdapter { public const string ExternalThemeUri = "pack://application:,,,/Untrusted.Theme;component/Deep.xaml"; }
            """, "external theme resource authority");
        yield return Mutant("M16", "unauthorized native import", """
            using System.Runtime.InteropServices;
            namespace Tcc.Windows.Monitors;
            public sealed class WindowMonitorAdapter { [DllImport("kernel32.dll")] private static extern nint GetCurrentProcess(); }
            """, "unauthorized native import");
        yield return Mutant("M17", "new ProjectReference", "namespace Tcc.Themes.Fallback; public sealed class BuiltInPresentationProjection { }",
            "new ProjectReference", "<ProjectReference Include=\"Unauthorized.Dependency.csproj\" />");
        yield return Mutant("M18", "new PackageReference", "namespace Tcc.Themes.Fallback; public sealed class BuiltInPresentationProjection { }",
            "new PackageReference", "<PackageReference Include=\"xunit\" Version=\"2.9.3\" />");
        yield return Mutant("M19", "new friend assembly", "namespace Tcc.Themes.Fallback; internal sealed class BuiltInPresentationProjection { }",
            "new friend assembly", "<InternalsVisibleTo Include=\"Unauthorized.Friend\" />");
        yield return Mutant("M20", "Home business leakage", "namespace Tcc.Themes.Home; public sealed class HomeSafetyPolicy { }",
            "unauthorized public production type");
        yield return Mutant("M21", "Trading / AI leakage", "namespace Tcc.DesktopHost.Trading { public sealed class TradingEngine { } } namespace Tcc.DesktopHost.AI { public sealed class AiScorer { } }",
            "unauthorized public production type");
    }

    private static object[] Mutant(string id, string attack, string source, string rejection, string projectItems = "") =>
        [id, attack, source, projectItems, rejection];

    private static string CreateMutantProject(string assemblyName, string projectItems) => $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0-windows</TargetFramework>
            <AssemblyName>{{assemblyName}}</AssemblyName>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <RestorePackagesWithLockFile>false</RestorePackagesWithLockFile>
            <NuGetAudit>false</NuGetAudit>
          </PropertyGroup>
          <ItemGroup>{{projectItems}}</ItemGroup>
        </Project>
        """;

    private static string CreateNativeMutation(
        string attackId,
        string source,
        string randomMethod,
        string randomEntryPoint)
    {
        string Insert(string owner, string declaration)
        {
            string marker = $"public sealed class {owner}\n{{";
            string mutated = source.Replace(marker, $"{marker}\n    {declaration}", StringComparison.Ordinal);
            if (mutated == source)
                throw new InvalidOperationException($"Native mutation owner not found: {owner}");
            return mutated;
        }

        return attackId switch
        {
            "P1" => Insert("WindowsStartupPathResolver",
                $"[System.Runtime.InteropServices.DllImport(\"kernel32.dll\", EntryPoint = \"{randomEntryPoint}\")] private static extern uint {randomMethod}();"),
            "P2" => Insert("WindowsStartupPathResolver",
                $"[System.Runtime.InteropServices.DllImport(\"user32.dll\", EntryPoint = \"{randomEntryPoint}\")] private static extern nint {randomMethod}();"),
            "P3" => Insert("WindowMonitorFacts",
                $"[DllImport(\"kernel32.dll\", EntryPoint = \"{randomEntryPoint}\")] private static extern uint {randomMethod}();"),
            "P6" => Insert("WindowMonitorAdapter",
                $"[DllImport(\"user32.dll\")] private static extern nint {randomMethod}();"),
            "P7" => Insert("WindowMonitorAdapter",
                $"[DllImport(\"user32.dll\", EntryPoint = \"{randomEntryPoint}\")] private static extern nint {randomMethod}();"),
            "P8" => Insert("WindowMonitorAdapter",
                "[DllImport(\"user32.dll\", EntryPoint = \"GetDpiForWindow\")] private static extern uint GetDpiForWindow(nint windowHandle, int unauthorizedParameter);"),
            "P9" => source.Replace(
                "[DllImport(\"user32.dll\")]\n    private static extern uint GetDpiForWindow(nint windowHandle);",
                "[DllImport(\"kernel32.dll\")]\n    private static extern uint GetDpiForWindow(nint windowHandle);",
                StringComparison.Ordinal),
            "P10" => Insert("WindowsStartupPathResolver", $$"""
                private static class {{randomMethod}}Owner
                {
                    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "{{randomEntryPoint}}")]
                    private static extern nint {{randomMethod}}();
                }
                """),
            "P11" => Insert("WindowsStartupFacts",
                $"[System.Runtime.InteropServices.DllImport(\"user32.dll\", EntryPoint = \"{randomEntryPoint}\")] private static extern nint {randomMethod}();"),
            "P12" => Insert("WindowMonitorFacts",
                $"[DllImport(\"kernel32.dll\", EntryPoint = \"GetCurrentThreadId\")] private static extern uint {randomMethod}();"),
            "P13" => Insert("WindowsStartupPathResolver",
                $"[System.Runtime.InteropServices.LibraryImport(\"kernel32.dll\", EntryPoint = \"{randomEntryPoint}\")] private static partial uint {randomMethod}();")
                .Replace("public sealed class WindowsStartupPathResolver", "public sealed partial class WindowsStartupPathResolver", StringComparison.Ordinal),
            _ => throw new InvalidOperationException(attackId),
        };
    }

    private static string[] GetCompiledBoundaryViolations(Assembly assembly, XDocument? project, string? expectedAssemblyName = null)
    {
        Type[] types = assembly.GetTypes();
        List<string> violations = [];
        string assemblyName = expectedAssemblyName ?? assembly.GetName().Name!;
        bool productionAssembly = ApprovedTopLevelTypes.TryGetValue(assemblyName, out string[]? approvedTypes);
        HashSet<string> allowed = productionAssembly
            ? approvedTypes!.Concat(ApprovedNestedProductionTypes[assemblyName]).ToHashSet(StringComparer.Ordinal)
            : ApprovedTopLevelTypes.Values.SelectMany(values => values)
                .Concat(ApprovedNestedProductionTypes.Values.SelectMany(values => values))
                .ToHashSet(StringComparer.Ordinal);
        foreach (Type type in types)
        {
            bool approvedIdentity = allowed.Contains(type.FullName!);
            bool approvedCompilerArtifact = IsApprovedCompilerGeneratedArtifact(type, assemblyName);
            if (!approvedIdentity && !approvedCompilerArtifact)
            {
                violations.Add(UnauthorizedTypeIdentityViolation(type));
                continue;
            }

            if (approvedCompilerArtifact)
                continue;

            if (type.IsNested && ApprovedNestedProductionTypes.Values.SelectMany(values => values).Contains(type.FullName!, StringComparer.Ordinal)
                && !type.IsNestedPrivate)
                violations.Add("unauthorized approved nested production type visibility");

            if (!ApprovedPhaseSixMembers.TryGetValue(type.FullName!, out string[]? expectedMembers))
                continue;
            bool expectedPublic = ApprovedPhaseSixPublicTypes.Contains(type.FullName!);
            bool approvedVisibility = type.IsNested
                ? type.IsNestedPrivate && !expectedPublic
                : expectedPublic ? type.IsPublic : type.IsNotPublic;
            if (!approvedVisibility)
                violations.Add("unauthorized Phase6 type visibility");
            string[] actualMembers = VisibleDeclaredMembers(type);
            if (actualMembers.Except(expectedMembers, StringComparer.Ordinal).Any()
                || (productionAssembly && expectedMembers.Except(actualMembers, StringComparer.Ordinal).Any()))
                violations.Add("unauthorized Phase6 member surface");
            if (ApprovedCallableParameterMetadata.TryGetValue(type.FullName!, out string[]? expectedParameterMetadata))
            {
                string[] actualParameterMetadata = CallableParameterMetadata(type);
                if (actualParameterMetadata.Except(expectedParameterMetadata, StringComparer.Ordinal).Any()
                    || (productionAssembly
                        && expectedParameterMetadata.Except(actualParameterMetadata, StringComparer.Ordinal).Any()))
                    violations.Add("unauthorized Phase6 parameter metadata");
            }
            string[] actualInterfaces = type.GetInterfaces().Select(TypeIdentity).Order(StringComparer.Ordinal).ToArray();
            if (ApprovedHostInterfaces.TryGetValue(type.FullName!, out string[]? expectedInterfaces))
            {
                if (!actualInterfaces.SequenceEqual(expectedInterfaces, StringComparer.Ordinal))
                    violations.Add("unauthorized Phase6 interface authority");
            }
            else if (!type.IsEnum && actualInterfaces.Length != 0)
            {
                violations.Add("unauthorized Phase6 interface authority");
            }
        }

        HashSet<string> actualNativeImports = GetNativeImportIdentities(assembly, assemblyName)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> approvedNativeImportsForAssembly = ApprovedNativeImports
            .Where(identity => identity.StartsWith($"assembly={assemblyName}|", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
        if (actualNativeImports.Except(approvedNativeImportsForAssembly, StringComparer.Ordinal).Any()
            || (productionAssembly
                && approvedNativeImportsForAssembly.Except(actualNativeImports, StringComparer.Ordinal).Any()))
            violations.Add("unauthorized native import");

        if (assemblyName == "Tcc.Windows")
            violations.AddRange(GetNativeBufferLayoutViolations(types));

        if (types.SelectMany(type => type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Any(IsUnauthorizedStaticMonitorAuthority))
            violations.Add("global mutable monitor state");

        if (assemblyName == "Tcc.DesktopHost")
        {
            Type[] hostOwners = types.Where(type => type.FullName is
                    "Tcc.DesktopHost.App"
                    or "Tcc.DesktopHost.MainWindow"
                    or "Tcc.DesktopHost.MainWindowViewModel"
                    or "Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap"
                    or "Tcc.DesktopHost.EmbeddedSafeTheme.SafeThemeResourceAdapter")
                .ToArray();
            Type[] hostCompiledBodies = hostOwners.SelectMany(GetCompiledBodyOwners).Distinct().ToArray();
            if (hostCompiledBodies.SelectMany(GetSurfaceTypes)
                .Any(type => TypeContains(type, IsForbiddenHostAuthorityType)))
                violations.Add("unauthorized Host authority dependency");
            if (hostCompiledBodies.SelectMany(GetReferencedMethodTypes)
                .Any(type => TypeContains(type, IsForbiddenHostAuthorityType)))
                violations.Add("unauthorized Host authority dependency");
            Type[] serviceLocatorOwners = types
                .Where(type => type.FullName is "Tcc.DesktopHost.MainWindowViewModel" or "Tcc.DesktopHost.MainWindow")
                .SelectMany(GetCompiledBodyOwners).Distinct().ToArray();
            if (serviceLocatorOwners
                .SelectMany(GetSurfaceTypes)
                .Any(type => TypeContains(type, candidate => candidate.FullName == typeof(IServiceProvider).FullName)))
                violations.Add("service locator dependency");
            if (serviceLocatorOwners
                .SelectMany(GetReferencedMethodTypes)
                .Any(type => TypeContains(type, candidate => candidate.FullName == typeof(IServiceProvider).FullName)))
                violations.Add("service locator dependency");
            if (HasUnapprovedServiceResolutionProvenance(hostOwners))
                violations.Add("service locator dependency");
        }

        if (productionAssembly)
        {
            foreach (string missing in approvedTypes!.Except(
                         types.Where(type => !type.IsNested).Select(type => type.FullName!),
                         StringComparer.Ordinal))
                violations.Add($"missing approved production type: {missing}");
        }

        if (!productionAssembly)
        {
        Type? source = types.SingleOrDefault(type => type.Name == "BuiltInThemePresentationSource");
        if (source?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Any(method => !method.IsSpecialName && (method.IsPublic || method.IsAssembly)) == true)
            violations.Add("extra production member");

        Type? snapshot = types.SingleOrDefault(type => type.Name == "BuiltInThemePresentationSnapshot");
        if (snapshot?.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length > 0)
            violations.Add("public snapshot constructor");
        if (snapshot?.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(property => property.SetMethod is not null) == true)
            violations.Add("mutable snapshot setter");
        if (snapshot?.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(property => property.PropertyType.IsGenericType
                && property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) == true)
            violations.Add("mutable dictionary leakage");

        IEnumerable<ParameterInfo> parameters = types.SelectMany(type =>
            type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(constructor => constructor.GetParameters())
            .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters())));
        if (parameters.Any(parameter => parameter.Name?.Contains("packagePath", StringComparison.OrdinalIgnoreCase) == true))
            violations.Add("theme path/package injection");

        if (types.SelectMany(type => type.GetInterfaces()).Any(type => type.Name is "IThemeRuntime" or "IThemePackage"))
            violations.Add("runtime/package authority");

        Type[] surfaceTypes = types.SelectMany(GetSurfaceTypes).ToArray();
        if (surfaceTypes.Any(type => type.Name is "ThemeCompatibilityEvidenceV2" or "ThemeCompatibilityReceiptV2"))
            violations.Add("compatibility/evidence construction leakage");
        if (types.Where(type => type.Name == "MainWindowViewModel").SelectMany(GetSurfaceTypes)
            .Any(type => type.Name == "ThemeCompatibilityResolverV2"))
            violations.Add("view-model resolver dependency");
        if (types.Where(type => type.Name == "MainWindowViewModel").SelectMany(GetSurfaceTypes)
            .Any(type => TypeContains(type, candidate => candidate.FullName == typeof(IServiceProvider).FullName)))
            violations.Add("service locator dependency");
        if (types.SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => field.GetRawConstantValue() as string)
            .Any(value => value?.Contains(";component/", StringComparison.OrdinalIgnoreCase) == true))
            violations.Add("external theme resource authority");
        }

        if (project?.Descendants("ProjectReference").Any() == true) violations.Add("new ProjectReference");
        if (project?.Descendants("PackageReference").Any() == true) violations.Add("new PackageReference");
        if (assembly.GetCustomAttributesData().Any(attribute =>
            attribute.AttributeType == typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute)
            && (!productionAssembly || attribute.ConstructorArguments[0].Value as string != "Tcc.Architecture.Tests")))
            violations.Add("new friend assembly");

        return violations.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IEnumerable<string> GetNativeImportIdentities(Assembly assembly, string assemblyName)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        return assembly.GetTypes()
            .SelectMany(type => type.GetMethods(flags))
            .Where(IsNativeImport)
            .Select(method => NativeImportIdentity(method, assemblyName))
            .Order(StringComparer.Ordinal);
    }

    private static IEnumerable<string> GetNativeBufferLayoutViolations(IEnumerable<Type> types)
    {
        Type? rectangle = types.SingleOrDefault(type =>
            type.FullName == "Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect");
        Type? monitorInfo = types.SingleOrDefault(type =>
            type.FullName == "Tcc.Windows.Monitors.WindowMonitorAdapter+NativeMonitorInfo");
        if (rectangle is null || monitorInfo is null)
            return [];

        try
        {
            bool rectangleLayout = HasExactNativeLayout(rectangle, CharSet.Ansi, 16,
                ("Left", "System.Int32", 0),
                ("Top", "System.Int32", 4),
                ("Right", "System.Int32", 8),
                ("Bottom", "System.Int32", 12));
            bool monitorLayout = HasExactNativeLayout(monitorInfo, CharSet.Unicode, 104,
                ("Size", "System.UInt32", 0),
                ("Monitor", "Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect", 4),
                ("Work", "Tcc.Windows.Monitors.WindowMonitorAdapter+NativeRect", 20),
                ("Flags", "System.UInt32", 36),
                ("DeviceName", "System.String", 40));
            FieldInfo? deviceName = monitorInfo.GetField(
                "DeviceName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            MarshalAsAttribute? marshal = deviceName?.GetCustomAttribute<MarshalAsAttribute>();
            if (rectangleLayout && monitorLayout && marshal?.Value == UnmanagedType.ByValTStr && marshal.SizeConst == 32)
                return [];
        }
        catch (Exception exception) when (exception is ArgumentException or TypeLoadException)
        {
            // Invalid interop metadata is itself a structural violation.
        }

        return ["unauthorized native buffer layout"];
    }

    private static bool HasExactNativeLayout(
        Type type,
        CharSet expectedCharSet,
        int expectedSize,
        params (string Name, string Type, int Offset)[] expectedFields)
    {
        StructLayoutAttribute? layout = type.StructLayoutAttribute;
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .OrderBy(field => field.MetadataToken)
            .ToArray();
        return type.IsValueType
            && layout is not null
            && layout.Value == LayoutKind.Sequential
            && layout.CharSet == expectedCharSet
            && layout.Pack == 0
            && layout.Size == 0
            && Marshal.SizeOf(type) == expectedSize
            && fields.Length == expectedFields.Length
            && fields.Zip(expectedFields).All(pair =>
                pair.First.IsAssembly
                && pair.First.Name == pair.Second.Name
                && pair.First.FieldType.FullName == pair.Second.Type
                && Marshal.OffsetOf(type, pair.First.Name).ToInt32() == pair.Second.Offset);
    }

    private static bool IsNativeImport(MethodInfo method) =>
        (method.Attributes & MethodAttributes.PinvokeImpl) != 0
        || method.GetCustomAttribute<DllImportAttribute>() is not null
        || method.GetCustomAttribute<LibraryImportAttribute>() is not null;

    private static string NativeImportIdentity(MethodInfo method, string assemblyName)
    {
        DllImportAttribute? import = method.GetCustomAttribute<DllImportAttribute>();
        string access = method.IsPrivate ? "private"
            : method.IsAssembly ? "internal"
            : method.IsFamily ? "protected"
            : method.IsFamilyOrAssembly ? "protected internal"
            : method.IsFamilyAndAssembly ? "private protected"
            : method.IsPublic ? "public" : "unknown";
        string library = import?.Value.ToLowerInvariant() ?? "<missing>";
        string entryPoint = import?.EntryPoint ?? method.Name;
        string returnMarshal = method.ReturnParameter.GetCustomAttribute<MarshalAsAttribute>()?.Value.ToString() ?? "None";
        string parameters = string.Join(",", method.GetParameters().Select(parameter =>
        {
            string direction = parameter.ParameterType.IsByRef
                ? parameter.IsOut ? "out" : parameter.IsIn ? "in" : "ref"
                : "value";
            string marshal = parameter.GetCustomAttribute<MarshalAsAttribute>()?.Value.ToString() ?? "None";
            return $"{direction}:{TypeIdentity(parameter.ParameterType)}:marshal={marshal}";
        }));
        bool preserveSig = (method.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != 0;
        bool libraryImport = method.GetCustomAttribute<LibraryImportAttribute>() is not null;
        return $"assembly={assemblyName}|owner={method.DeclaringType!.FullName}|access={access}|static={method.IsStatic}"
            + $"|method={method.Name}|return={TypeIdentity(method.ReturnType)}|returnMarshal={returnMarshal}"
            + $"|library={library}|entry={entryPoint}|calling={import?.CallingConvention.ToString() ?? "None"}"
            + $"|charSet={import?.CharSet.ToString() ?? "None"}|setLastError={import?.SetLastError ?? false}"
            + $"|exactSpelling={import?.ExactSpelling ?? false}|preserveSig={preserveSig}|libraryImport={libraryImport}"
            + $"|parameters={parameters}";
    }

    private static string UnauthorizedTypeIdentityViolation(Type type)
    {
        if (type.IsPublic || type.IsNestedPublic) return "unauthorized public production type";
        if (type.IsNotPublic || type.IsNestedAssembly) return "unauthorized internal production type";
        if (type.IsNestedPrivate) return "unauthorized private nested production type";
        if (type.IsNestedFamily) return "unauthorized protected nested production type";
        if (type.IsNestedFamORAssem) return "unauthorized protected internal nested production type";
        if (type.IsNestedFamANDAssem) return "unauthorized private protected nested production type";
        return "unauthorized production type identity";
    }

    private static bool IsApprovedCompilerGeneratedArtifact(Type type, string assemblyName)
    {
        if (!ApprovedCompilerGeneratedTypes.TryGetValue(assemblyName, out string[]? approvedArtifacts)
            || !approvedArtifacts.Contains(type.FullName!, StringComparer.Ordinal)
            || !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
        {
            return false;
        }

        if (string.Equals(type.FullName, "<>z__ReadOnlyArray`1", StringComparison.Ordinal))
        {
            Type[] genericArguments = type.GetGenericArguments();
            FieldInfo? items = type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            return type.DeclaringType is null
                && type.Namespace is null
                && type.IsNotPublic
                && type.IsClass
                && type.IsSealed
                && type.IsGenericTypeDefinition
                && genericArguments.Length == 1
                && items is not null
                && items.IsPrivate
                && items.IsInitOnly
                && items.FieldType.IsArray
                && items.FieldType.GetElementType() == genericArguments[0]
                && typeof(System.Collections.IList).IsAssignableFrom(type);
        }

        Type? owner = type.DeclaringType;
        if (owner is null
            || !type.IsNestedPrivate
            || !ApprovedTopLevelTypes[assemblyName].Contains(owner.FullName!, StringComparer.Ordinal))
        {
            return false;
        }

        if (typeof(IAsyncStateMachine).IsAssignableFrom(type))
        {
            return owner.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Any(method => method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == type);
        }

        FieldInfo[] fields = type.GetFields(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        if (string.Equals(type.Name, "<>O", StringComparison.Ordinal))
            return type.IsAbstract && type.IsSealed && fields.Length > 0
                && fields.All(field => field.IsStatic && field.Name.Contains(">__", StringComparison.Ordinal))
                && methods.Length == 0;
        if (string.Equals(type.Name, "<>c", StringComparison.Ordinal))
            return type.IsClass && type.IsSealed && !type.IsAbstract
                && fields.Any(field => field.IsStatic && field.Name == "<>9" && field.FieldType == type)
                && methods.Length > 0 && methods.All(method => method.Name.Contains(">b__", StringComparison.Ordinal));
        if (type.Name.StartsWith("<>c__DisplayClass", StringComparison.Ordinal))
            return type.IsClass && type.IsSealed && !type.IsAbstract
                && fields.Any(field => !field.IsStatic)
                && methods.Length > 0 && methods.All(method => method.Name.Contains(">b__", StringComparison.Ordinal));
        return false;
    }

    private static string[] Lines(string value) => value.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Trim()).Where(line => line.Length > 0).Order(StringComparer.Ordinal).ToArray();

    private static string TypeIdentity(Type type)
    {
        if (type.IsByRef) return TypeIdentity(type.GetElementType()!) + "&";
        if (type.IsArray) return TypeIdentity(type.GetElementType()!) + "[]";
        if (!type.IsGenericType) return type.FullName!;
        return type.GetGenericTypeDefinition().FullName!.Split('`')[0] + "<"
            + string.Join(",", type.GetGenericArguments().Select(TypeIdentity)) + ">";
    }

    private static string[] VisibleDeclaredMembers(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        List<string> members = [];
        members.AddRange(type.GetConstructors(flags).Where(Visible)
            .Select(ctor => $"C {CallableAccess(ctor)}({string.Join(",", ctor.GetParameters().Select(p => TypeIdentity(p.ParameterType)))})"));
        members.AddRange(type.GetProperties(flags).Where(property =>
                (property.GetMethod is not null && Visible(property.GetMethod))
                || (property.SetMethod is not null && Visible(property.SetMethod)))
            .Select(property => $"P {CallableAccess((MethodBase?)property.GetMethod ?? property.SetMethod!)} {property.Name}:{TypeIdentity(property.PropertyType)}"
                + (property.SetMethod is not null ? " SET" : "")));
        HashSet<MethodInfo> accessors = type.GetProperties(flags)
            .SelectMany(property => new[] { property.GetMethod, property.SetMethod })
            .Concat(type.GetEvents(flags).SelectMany(evt => new[] { evt.AddMethod, evt.RemoveMethod, evt.RaiseMethod }))
            .OfType<MethodInfo>().ToHashSet();
        members.AddRange(type.GetMethods(flags).Where(method => Visible(method) && !accessors.Contains(method))
            .Select(method => $"M {CallableAccess(method)} {method.Name}({string.Join(",", method.GetParameters().Select(p => TypeIdentity(p.ParameterType)))}):{TypeIdentity(method.ReturnType)}"));
        members.AddRange(type.GetFields(flags).Where(field => Visible(field) && !IsGeneratedXamlBackingField(field))
            .Select(field => $"F {FieldAccess(field)} {field.Name}:{TypeIdentity(field.FieldType)}"));
        members.AddRange(type.GetEvents(flags).Where(evt => evt.AddMethod is not null && Visible(evt.AddMethod))
            .Select(evt => $"E {evt.Name}:{TypeIdentity(evt.EventHandlerType!)}"));
        return members.Order(StringComparer.Ordinal).ToArray();
    }

    private static bool Visible(MethodBase method) => method.IsPublic || method.IsAssembly
        || method.IsFamily || method.IsFamilyOrAssembly || method.IsFamilyAndAssembly;

    private static bool Visible(FieldInfo field) => field.IsPublic || field.IsAssembly
        || field.IsFamily || field.IsFamilyOrAssembly || field.IsFamilyAndAssembly;

    private static bool IsGeneratedXamlBackingField(FieldInfo field) =>
        field.IsAssembly
        && field.DeclaringType?.FullName == "Tcc.DesktopHost.MainWindow"
        && typeof(System.Windows.DependencyObject).IsAssignableFrom(field.FieldType);

    private static string CallableAccess(MethodBase method) => method.IsPublic ? "public"
        : method.IsAssembly ? "internal"
        : method.IsFamily ? "protected"
        : method.IsFamilyOrAssembly ? "protected internal"
        : method.IsFamilyAndAssembly ? "private protected"
        : "private";

    private static string FieldAccess(FieldInfo field) => field.IsPublic ? "public"
        : field.IsAssembly ? "internal"
        : field.IsFamily ? "protected"
        : field.IsFamilyOrAssembly ? "protected internal"
        : field.IsFamilyAndAssembly ? "private protected"
        : "private";

    private static string[] CallableParameterMetadata(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        List<string> metadata = [];
        metadata.AddRange(type.GetConstructors(flags).Where(Visible)
            .Select(constructor => $"C({ParameterMetadata(constructor.GetParameters())})"));
        HashSet<MethodInfo> accessors = type.GetProperties(flags)
            .SelectMany(property => new[] { property.GetMethod, property.SetMethod })
            .Concat(type.GetEvents(flags).SelectMany(evt => new[] { evt.AddMethod, evt.RemoveMethod, evt.RaiseMethod }))
            .OfType<MethodInfo>().ToHashSet();
        metadata.AddRange(type.GetMethods(flags)
            .Where(method => Visible(method) && !method.IsSpecialName && !accessors.Contains(method))
            .Select(method => $"M {method.Name}({ParameterMetadata(method.GetParameters())})"));
        return metadata.Order(StringComparer.Ordinal).ToArray();
    }

    private static string ParameterMetadata(IEnumerable<ParameterInfo> parameters) =>
        string.Join(",", parameters.Select(parameter =>
            $"{TypeIdentity(parameter.ParameterType)} {parameter.Name ?? "<missing>"}"));

    private static IEnumerable<Type> GetSurfaceTypes(Type type) =>
        type.GetGenericArguments()
            .Concat(type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType)))
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(property => property.PropertyType))
            .Concat(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(field => field.FieldType))
            .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
                    .Append(method.ReturnType)
                    .Concat(method.GetGenericArguments())))
            .Concat(type.GetInterfaces())
            .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetMethodBody()?.LocalVariables.Select(local => local.LocalType) ?? []));

    private static IEnumerable<Type> GetCompiledBodyOwners(Type owner)
    {
        yield return owner;
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (MethodInfo method in owner.GetMethods(flags))
        {
            foreach (StateMachineAttribute stateMachine in method
                         .GetCustomAttributes(inherit: false).OfType<StateMachineAttribute>())
            {
                Type generatedBody = stateMachine.StateMachineType;
                if (generatedBody.Assembly == owner.Assembly
                    && generatedBody.DeclaringType == owner
                    && generatedBody.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                {
                    yield return generatedBody;
                }
            }
        }
    }

    private static bool TypeContains(Type type, Func<Type, bool> predicate) =>
        GetTypeGraph(type).Any(predicate);

    private static IEnumerable<Type> GetTypeGraph(Type root)
    {
        HashSet<Type> visited = [];
        Stack<Type> pending = new();
        pending.Push(root);
        while (pending.TryPop(out Type? type))
        {
            if (!visited.Add(type)) continue;
            yield return type;
            if (type.HasElementType && type.GetElementType() is Type element)
                pending.Push(element);
            foreach (Type argument in type.GetGenericArguments())
                pending.Push(argument);
            if (type.IsGenericParameter)
            {
                foreach (Type constraint in type.GetGenericParameterConstraints())
                    pending.Push(constraint);
            }
        }
    }

    private static bool IsUnauthorizedStaticMonitorAuthority(FieldInfo field) =>
        TypeContains(field.FieldType, candidate =>
            candidate.FullName == "Tcc.Windows.Monitors.WindowMonitorFacts");

    private static bool IsForbiddenHostAuthorityType(Type type) => type.FullName is
        "Tcc.Presentation.Contracts.Theme.IThemeRuntime"
        or "Tcc.Presentation.Contracts.Theme.IThemePackage"
        or "Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2"
        || type.Namespace?.StartsWith("Tcc.Features.Home", StringComparison.Ordinal) == true
        || type.Namespace?.StartsWith("Tcc.DesktopHost.Home", StringComparison.Ordinal) == true
        || type.Namespace?.StartsWith("Tcc.DesktopHost.Trading", StringComparison.Ordinal) == true
        || type.Namespace?.StartsWith("Tcc.DesktopHost.AI", StringComparison.Ordinal) == true;

    private static readonly Dictionary<short, OpCode> IlOpCodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode))
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opcode => opcode.Value);

    private static IEnumerable<Type> GetReferencedMethodTypes(Type owner)
    {
        foreach (MemberInfo member in GetReferencedMembers(owner))
        {
            foreach (Type root in GetReferencedTypeRoots(member))
            {
                foreach (Type type in GetTypeGraph(root))
                    yield return type;
            }
        }
    }

    private static IEnumerable<Type> GetReferencedTypeRoots(MemberInfo member)
    {
        if (member.DeclaringType is Type declaringType) yield return declaringType;
        if (member is Type referencedType) yield return referencedType;
        if (member is FieldInfo field) yield return field.FieldType;
        if (member is MethodBase method)
        {
            foreach (ParameterInfo parameter in method.GetParameters())
                yield return parameter.ParameterType;
            if (method is MethodInfo calledMethod)
            {
                foreach (Type argument in calledMethod.GetGenericArguments())
                    yield return argument;
                yield return calledMethod.ReturnType;
            }
        }
    }

    private static bool HasUnapprovedServiceResolutionProvenance(IEnumerable<Type> owners)
    {
        ReferencedCallsite[] serviceCallsites = owners
            .SelectMany(GetReferencedCallsites)
            .Where(callsite => callsite.Member is MethodBase method && CarriesServiceProviderAuthority(method))
            .ToArray();
        ReferencedCallsite[] approvedOriginCalls = serviceCallsites
            .Where(callsite => IsApprovedStartupOrigin(callsite.Origin))
            .ToArray();
        ReferencedCallsite[] serviceGetters = approvedOriginCalls
            .Where(callsite => callsite.Member is MethodBase method && IsHostServicesGetter(method))
            .ToArray();
        ReferencedCallsite[] windowResolutions = approvedOriginCalls
            .Where(callsite => callsite.Member is MethodBase method && IsMainWindowResolution(method))
            .ToArray();

        bool exactApprovedPath = serviceCallsites.Length == 2
            && approvedOriginCalls.Length == 2
            && serviceGetters.Length == 1
            && windowResolutions.Length == 1
            && serviceGetters[0].Body == windowResolutions[0].Body
            && serviceGetters[0].IlOffset < windowResolutions[0].IlOffset;
        return !exactApprovedPath;
    }

    private static bool CarriesServiceProviderAuthority(MethodBase method) =>
        method.DeclaringType?.FullName == typeof(IServiceProvider).FullName
        || method.GetParameters().Any(parameter =>
            TypeContains(parameter.ParameterType, candidate => candidate.FullName == typeof(IServiceProvider).FullName))
        || method is MethodInfo info && TypeContains(info.ReturnType,
            candidate => candidate.FullName == typeof(IServiceProvider).FullName);

    private static bool IsApprovedStartupOrigin(MethodBase method) =>
        method.DeclaringType?.FullName == "Tcc.DesktopHost.App"
        && method.Name == "OnStartup"
        && MethodReturnType(method) == typeof(void)
        && method.GetParameters().Select(parameter => parameter.ParameterType.FullName)
            .SequenceEqual(["System.Windows.StartupEventArgs"], StringComparer.Ordinal);

    private static bool IsHostServicesGetter(MethodBase method) =>
        method.DeclaringType?.FullName == "Microsoft.Extensions.Hosting.IHost"
        && method.Name == "get_Services"
        && method.GetParameters().Length == 0
        && method is MethodInfo info
        && info.ReturnType.FullName == typeof(IServiceProvider).FullName;

    private static bool IsMainWindowResolution(MethodBase method) =>
        method.DeclaringType?.FullName
            == "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions"
        && method.Name == "GetRequiredService"
        && method is MethodInfo resolution
        && resolution.IsGenericMethod
        && resolution.GetGenericArguments().Length == 1
        && resolution.GetGenericArguments()[0].FullName == "Tcc.DesktopHost.MainWindow";

    private static Type MethodReturnType(MethodBase method) =>
        method is MethodInfo info ? info.ReturnType : typeof(void);

    private static IEnumerable<ReferencedCallsite> GetReferencedCallsites(Type owner)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        IEnumerable<MethodBase> origins = owner.GetMethods(flags).Cast<MethodBase>()
            .Concat(owner.GetConstructors(flags));
        foreach (MethodBase origin in origins)
        {
            foreach ((int Offset, MemberInfo Member) reference in GetReferencedMembers(origin))
                yield return new ReferencedCallsite(origin, origin, reference.Offset, reference.Member);
            foreach (StateMachineAttribute stateMachine in origin
                         .GetCustomAttributes(inherit: false).OfType<StateMachineAttribute>())
            {
                Type bodyOwner = stateMachine.StateMachineType;
                if (bodyOwner.Assembly != owner.Assembly
                    || bodyOwner.DeclaringType != owner
                    || !bodyOwner.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                    continue;
                IEnumerable<MethodBase> bodies = bodyOwner.GetMethods(flags).Cast<MethodBase>()
                    .Concat(bodyOwner.GetConstructors(flags));
                foreach (MethodBase body in bodies)
                {
                    foreach ((int Offset, MemberInfo Member) reference in GetReferencedMembers(body))
                        yield return new ReferencedCallsite(origin, body, reference.Offset, reference.Member);
                }
            }
        }
    }

    private sealed record ReferencedCallsite(
        MethodBase Origin,
        MethodBase Body,
        int IlOffset,
        MemberInfo Member);

    private static IEnumerable<MemberInfo> GetReferencedMembers(Type owner)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        IEnumerable<MethodBase> methods = owner.GetMethods(flags).Cast<MethodBase>()
            .Concat(owner.GetConstructors(flags));
        foreach (MethodBase method in methods)
            foreach ((int Offset, MemberInfo Member) reference in GetReferencedMembers(method))
                yield return reference.Member;
    }

    private static IEnumerable<(int Offset, MemberInfo Member)> GetReferencedMembers(MethodBase method)
    {
        byte[]? il = method.GetMethodBody()?.GetILAsByteArray();
        if (il is null) yield break;
        for (int offset = 0; offset < il.Length;)
        {
            int instructionOffset = offset;
            short code = il[offset++] == 0xFE
                ? unchecked((short)(0xFE00 | il[offset++]))
                : il[offset - 1];
            OpCode opcode = IlOpCodes[code];
            int operandStart = offset;
            int operandLength = opcode.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, offset),
                OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI
                    or OperandType.InlineMethod or OperandType.InlineSig or OperandType.InlineString
                    or OperandType.InlineTok or OperandType.InlineType or OperandType.ShortInlineR => 4,
                _ => throw new InvalidOperationException($"Unknown IL operand type: {opcode.OperandType}"),
            };
            offset += operandLength;
            if (opcode.OperandType is not (OperandType.InlineMethod or OperandType.InlineField
                or OperandType.InlineTok or OperandType.InlineType)) continue;
            int token = BitConverter.ToInt32(il, operandStart);
            Type? declaringType = method.DeclaringType;
            Type[] declaringGenerics = declaringType?.IsGenericType == true ? declaringType.GetGenericArguments() : [];
            Type[] methodGenerics = method.IsGenericMethod ? method.GetGenericArguments() : [];
            MemberInfo member = method.Module.ResolveMember(token, declaringGenerics, methodGenerics)!;
            yield return (instructionOffset, member);
        }
    }

    private static ProcessResult RunProcess(string fileName, IReadOnlyList<string> arguments)
    {
        ProcessStartInfo startInfo = new(fileName) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (string argument in arguments) startInfo.ArgumentList.Add(argument);
        using Process process = Process.Start(startInfo)!;
        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        Task.WhenAll(standardOutput, standardError).GetAwaiter().GetResult();
        return new ProcessResult(process.ExitCode, standardOutput.Result + standardError.Result);
    }

    private sealed record ProcessResult(int ExitCode, string Output);

    private static (Assembly Assembly, AssemblyLoadContext Context) CompileWindowsProductionCopy(
        string targetFile,
        string mutation,
        Func<string, string> mutate)
    {
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-84r9-windows-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string windowsSourceRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.Windows");
        string[] sourcePaths = Directory.GetFiles(windowsSourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
        string targetPath = Assert.Single(sourcePaths, path => Path.GetFileName(path) == targetFile);
        string original = File.ReadAllText(targetPath);
        string mutated = mutate(original);
        Assert.NotEqual(original, mutated);
        string mutatedPath = Path.Combine(root, targetFile);
        File.WriteAllText(mutatedPath, mutated);
        string compileItems = string.Join(Environment.NewLine, sourcePaths.Select(path =>
            $"<Compile Include=\"{System.Security.SecurityElement.Escape(path == targetPath ? mutatedPath : path)}\" />"));
        string projectPath = Path.Combine(root, $"WindowsMutation{mutation}.csproj");
        File.WriteAllText(projectPath, CreateMutantProject("Tcc.Windows", compileItems).Replace(
            "<Nullable>enable</Nullable>",
            "<EnableDefaultCompileItems>false</EnableDefaultCompileItems><Nullable>enable</Nullable>",
            StringComparison.Ordinal));
        ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q", "-m:1"]);
        Assert.True(build.ExitCode == 0,
            $"Windows {mutation} mutant must compile before guard evidence.{Environment.NewLine}{build.Output}");
        string output = Path.Combine(root, "bin", "Release", "net10.0-windows");
        string assemblyPath = Path.Combine(output, "Tcc.Windows.dll");
        AssemblyLoadContext context = new($"windows-{mutation}-{Guid.NewGuid():N}", isCollectible: true);
        return (context.LoadFromAssemblyPath(assemblyPath), context);
    }

    private static (Assembly Assembly, AssemblyLoadContext Context) CompileHostProductionCopy(
        string viewModelSource,
        string mainWindowSource,
        string mutation,
        string? appSource = null,
        bool useProductionAssemblyIdentity = false)
    {
        string root = Path.Combine(Path.GetTempPath(), "tcc-p6-84r7-host-mutants", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string viewModelPath = Path.Combine(root, "MainWindowViewModel.cs");
        string mainWindowPath = Path.Combine(root, "MainWindow.xaml.cs");
        File.WriteAllText(viewModelPath, viewModelSource);
        File.WriteAllText(mainWindowPath, mainWindowSource);

        string hostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
        string appPath = Path.Combine(hostRoot, "App.xaml.cs");
        if (appSource is not null)
        {
            appPath = Path.Combine(root, "App.xaml.cs");
            File.WriteAllText(appPath, appSource);
        }
        string generatedRoot = Path.Combine(hostRoot, "obj", "Release", "net10.0-windows");
        string[] compilePaths =
        [
            appPath,
            mainWindowPath,
            viewModelPath,
            .. Directory.GetFiles(Path.Combine(hostRoot, "ThemeBootstrap"), "*.cs", SearchOption.AllDirectories),
            .. Directory.GetFiles(Path.Combine(hostRoot, "EmbeddedSafeTheme"), "*.cs", SearchOption.AllDirectories),
            Path.Combine(generatedRoot, "App.g.cs"),
            Path.Combine(generatedRoot, "MainWindow.g.cs"),
            Path.Combine(generatedRoot, "Tcc.DesktopHost.GlobalUsings.g.cs"),
        ];
        Assert.All(compilePaths, path => Assert.True(File.Exists(path), $"Host production-copy input missing: {path}"));
        string compileItems = string.Join(Environment.NewLine, compilePaths.Select(path =>
            $"<Compile Include=\"{System.Security.SecurityElement.Escape(path)}\" />"));
        string projectPath = Path.Combine(root, $"HostMutation{mutation}.csproj");
        string assemblyName = useProductionAssemblyIdentity ? "Tcc.DesktopHost" : $"HostMutation{mutation}";
        File.WriteAllText(projectPath, $$"""
            <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
              <PropertyGroup>
                <TargetFramework>net10.0-windows</TargetFramework>
                <AssemblyName>{{assemblyName}}</AssemblyName>
                <UseWPF>true</UseWPF>
                <OutputType>Library</OutputType>
                <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <RestorePackagesWithLockFile>false</RestorePackagesWithLockFile>
                <NuGetAudit>false</NuGetAudit>
              </PropertyGroup>
              <ItemGroup>
                {{compileItems}}
                <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.11" />
                <Reference Include="Tcc.Presentation.Contracts"><HintPath>{{System.Security.SecurityElement.Escape(Path.Combine(HostOutput, "Tcc.Presentation.Contracts.dll"))}}</HintPath></Reference>
                <Reference Include="Tcc.Themes"><HintPath>{{System.Security.SecurityElement.Escape(Path.Combine(HostOutput, "Tcc.Themes.dll"))}}</HintPath></Reference>
                <Reference Include="Tcc.Windows"><HintPath>{{System.Security.SecurityElement.Escape(Path.Combine(HostOutput, "Tcc.Windows.dll"))}}</HintPath></Reference>
              </ItemGroup>
            </Project>
            """);
        ProcessResult build = RunProcess("dotnet", ["build", projectPath, "-c", "Release", "--nologo", "-v:q", "-m:1"]);
        Assert.True(build.ExitCode == 0,
            $"Host {mutation} mutant must compile before guard evidence.{Environment.NewLine}{build.Output}");
        string output = Path.Combine(root, "bin", "Release", "net10.0-windows");
        string assemblyPath = Path.Combine(output, $"{assemblyName}.dll");
        AssemblyLoadContext context = new($"host-{mutation}-{Guid.NewGuid():N}", isCollectible: true);
        context.Resolving += (loadContext, name) =>
        {
            string dependency = Path.Combine(output, name.Name + ".dll");
            if (!File.Exists(dependency)) dependency = Path.Combine(HostOutput, name.Name + ".dll");
            return File.Exists(dependency) ? loadContext.LoadFromAssemblyPath(dependency) : null;
        };
        return (context.LoadFromAssemblyPath(assemblyPath), context);
    }

    private static (Assembly Assembly, AssemblyLoadContext Context) CompileHostConstraintControl(
        string control,
        string declaration)
    {
        string viewModelSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindowViewModel.cs"));
        viewModelSource = viewModelSource.Replace(
            "public sealed class MainWindowViewModel\n{",
            $"public sealed class MainWindowViewModel\n{{\n    {declaration}",
            StringComparison.Ordinal);
        string mainWindowSource = File.ReadAllText(Path.Combine(
            RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs"));
        return CompileHostProductionCopy(
            viewModelSource,
            mainWindowSource,
            control,
            useProductionAssemblyIdentity: true);
    }

    private static Assembly LoadHostAssembly()
    {
        string path = Path.Combine(HostOutput, "Tcc.DesktopHost.dll");
        Assert.True(File.Exists(path), $"Build the Release x64 host first: {path}");
        return Assembly.LoadFrom(path);
    }

    private static (Assembly Host, List<object> Descriptors) ConfigureHost(string[] arguments)
    {
        Assembly host = LoadHostAssembly();
        Assembly hosting = Assembly.LoadFrom(Path.Combine(HostOutput, "Microsoft.Extensions.Hosting.dll"));
        Type hostFactory = hosting.GetType("Microsoft.Extensions.Hosting.Host", throwOnError: true)!;
        object builder = hostFactory.GetMethod("CreateApplicationBuilder", [typeof(string[])])!
            .Invoke(null, [Array.Empty<string>()])!;
        Type bootstrap = host.GetType("Tcc.DesktopHost.ThemeBootstrap.ThemeBootstrap", throwOnError: true)!;
        bootstrap.GetMethod("Configure", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [builder, arguments]);

        IEnumerable descriptors = Assert.IsAssignableFrom<IEnumerable>(builder.GetType().GetProperty("Services")!.GetValue(builder));
        List<object> tccDescriptors = [];
        foreach (object descriptor in descriptors)
        {
            Type? serviceType = descriptor.GetType().GetProperty("ServiceType")!.GetValue(descriptor) as Type;
            if (serviceType?.FullName?.StartsWith("Tcc.", StringComparison.Ordinal) == true)
            {
                tccDescriptors.Add(descriptor);
            }
        }

        return (host, tccDescriptors);
    }

    private static string[] GetRegistrationViolations(IEnumerable<string> registrations)
    {
        Dictionary<string, (string Lifetime, string Form)> expected = new(StringComparer.Ordinal)
        {
            ["Tcc.Windows.Startup.WindowsStartupPathResolver"] = ("Singleton", "instance"),
            ["Tcc.Windows.Startup.WindowsStartupFacts"] = ("Singleton", "instance"),
            ["Tcc.Themes.Fallback.BuiltInThemePresentationSource"] = ("Singleton", "instance"),
            ["Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot"] = ("Singleton", "instance"),
            ["Tcc.Windows.Monitors.WindowMonitorAdapter"] = ("Transient", "type"),
            ["Tcc.DesktopHost.MainWindowViewModel"] = ("Singleton", "instance"),
            ["Tcc.DesktopHost.MainWindow"] = ("Singleton", "type"),
        };
        List<string> violations = [];
        foreach (string registration in registrations.Where(line => line.StartsWith("Tcc.", StringComparison.Ordinal)))
        {
            string[] parts = registration.Split('|');
            if (parts.Length != 7 || !expected.TryGetValue(parts[0], out var approved))
            {
                violations.Add("wrong DI descriptor form");
                continue;
            }
            if (parts[1] != approved.Lifetime)
                violations.Add("wrong DI lifetime");
            bool isInstance = parts[2] == "<null>" && parts[3] == parts[0] && parts[4] == "False";
            bool isType = parts[2] == parts[0] && parts[3] == "<null>" && parts[4] == "False";
            if ((approved.Form == "instance" && !isInstance)
                || (approved.Form == "type" && !isType)
                || parts[5] != "False" || parts[6] != "<null>")
                violations.Add("wrong DI descriptor form");
            expected.Remove(parts[0]);
        }
        if (expected.Count != 0) violations.Add("missing DI registration");
        return violations.ToArray();
    }

    private static object AssertDescriptor(
        IEnumerable<object> descriptors,
        Type serviceType,
        string lifetime,
        bool requireInstance = true)
    {
        object descriptor = Assert.Single(descriptors, item =>
            item.GetType().GetProperty("ServiceType")!.GetValue(item) as Type == serviceType);
        Assert.Equal(lifetime, descriptor.GetType().GetProperty("Lifetime")!.GetValue(descriptor)!.ToString());
        Type descriptorType = descriptor.GetType();
        Assert.False((bool)descriptorType.GetProperty("IsKeyedService")!.GetValue(descriptor)!);
        Assert.Null(descriptorType.GetProperty("ServiceKey")!.GetValue(descriptor));
        object? implementationType = descriptorType.GetProperty("ImplementationType")!.GetValue(descriptor);
        object? instance = descriptorType.GetProperty("ImplementationInstance")!.GetValue(descriptor);
        object? factory = descriptorType.GetProperty("ImplementationFactory")!.GetValue(descriptor);
        if (requireInstance)
        {
            Assert.Null(implementationType);
            Assert.Null(factory);
            Assert.NotNull(instance);
            Assert.Equal(serviceType, instance.GetType());
            return instance;
        }

        Assert.Equal(serviceType, implementationType);
        Assert.Null(instance);
        Assert.Null(factory);
        return descriptor;
    }

    private static void AssertNativeStructLayout(
        Type type,
        CharSet expectedCharSet,
        int expectedSize,
        params (string Name, Type Type, int Offset)[] expectedFields)
    {
        StructLayoutAttribute layout = Assert.IsType<StructLayoutAttribute>(type.StructLayoutAttribute);
        Assert.Equal(LayoutKind.Sequential, layout.Value);
        Assert.Equal(expectedCharSet, layout.CharSet);
        Assert.Equal(0, layout.Pack);
        Assert.Equal(0, layout.Size);
        Assert.Equal(expectedSize, Marshal.SizeOf(type));
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .OrderBy(field => field.MetadataToken)
            .ToArray();
        Assert.Equal(expectedFields.Select(field => field.Name), fields.Select(field => field.Name));
        for (int index = 0; index < fields.Length; index++)
        {
            Assert.True(fields[index].IsAssembly);
            Assert.Equal(expectedFields[index].Type, fields[index].FieldType);
            Assert.Equal(expectedFields[index].Offset, Marshal.OffsetOf(type, fields[index].Name).ToInt32());
        }
    }

    private static object GetProperty(object instance, string propertyName) =>
        instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!
            .GetValue(instance)!;

    private static object FindByAutomationId(object root, string automationId)
    {
        Type automationProperties = Type.GetType(
            "System.Windows.Automation.AutomationProperties, PresentationCore", true)!;
        MethodInfo getAutomationId = automationProperties.GetMethod(
            "GetAutomationId", BindingFlags.Public | BindingFlags.Static)!;
        Type visualTreeHelper = Type.GetType(
            "System.Windows.Media.VisualTreeHelper, PresentationCore", true)!;
        MethodInfo getChildrenCount = visualTreeHelper.GetMethod(
            "GetChildrenCount", BindingFlags.Public | BindingFlags.Static)!;
        MethodInfo getChild = visualTreeHelper.GetMethod(
            "GetChild", BindingFlags.Public | BindingFlags.Static)!;
        Stack<object> pending = new();
        pending.Push(root);

        while (pending.Count != 0)
        {
            object current = pending.Pop();
            if ((string)getAutomationId.Invoke(null, [current])! == automationId)
                return current;

            int childCount = (int)getChildrenCount.Invoke(null, [current])!;
            for (int index = childCount - 1; index >= 0; index--)
                pending.Push(getChild.Invoke(null, [current, index])!);
        }

        throw new InvalidOperationException($"AutomationId '{automationId}' was not found.");
    }

    private static void SetProperty(object instance, string propertyName, object value) =>
        instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(instance, value);

    private static object? Invoke(object instance, string methodName) =>
        instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes)!
            .Invoke(instance, null);

    private static string GetAutomationPeerName(object element)
    {
        Type peerFactoryType = Type.GetType(
            "System.Windows.Automation.Peers.UIElementAutomationPeer, PresentationCore",
            throwOnError: true)!;
        object peer = peerFactoryType.GetMethod("CreatePeerForElement", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, [element])
            ?? Activator.CreateInstance(
                Type.GetType("System.Windows.Automation.Peers.TextBlockAutomationPeer, PresentationFramework", true)!,
                element)!;
        Type automationPeerType = Type.GetType(
            "System.Windows.Automation.Peers.AutomationPeer, PresentationCore", true)!;
        return (string)automationPeerType.GetMethod("GetName", BindingFlags.Public | BindingFlags.Instance)!
            .Invoke(peer, null)!;
    }

    private static bool GetKeyboardNavigationIsTabStop(object element)
    {
        Type navigation = Type.GetType("System.Windows.Input.KeyboardNavigation, PresentationFramework", true)!;
        return (bool)navigation.GetMethod("GetIsTabStop", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, [element])!;
    }

    private static void AssertConcreteService(Type type, string methodName, Type returnType, params Type[] parameterTypes)
    {
        AssertClass(type, "Tcc.Windows", isPublic: true, isStatic: false);
        AssertSingleConstructor(type, isPublic: true);
        MethodInfo method = Assert.Single(CallableDeclaredMethods(type), candidate => candidate.Name == methodName);
        Assert.Equal(returnType, Nullable.GetUnderlyingType(method.ReturnType) ?? method.ReturnType);
        Assert.Equal(parameterTypes, method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    private static void AssertClass(Type type, string assemblyName, bool isPublic, bool isStatic)
    {
        Assert.Equal(assemblyName, type.Assembly.GetName().Name);
        Assert.Equal(isPublic, type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Equal(isStatic, type.IsAbstract && type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Null(type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
    }

    private static void AssertSingleConstructor(Type type, bool isPublic, params Type[] parameterTypes)
    {
        ConstructorInfo constructor = Assert.Single(type.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(isPublic, constructor.IsPublic);
        Assert.Equal(!isPublic, constructor.IsAssembly);
        Assert.Equal(parameterTypes, constructor.GetParameters().Select(parameter => parameter.ParameterType));
    }

    private static void AssertProperties(Type type, params (string Name, Type Type)[] expected)
    {
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(expected.Select(item => item.Name), properties.Select(property => property.Name));
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index].Type, properties[index].PropertyType);
            Assert.True(properties[index].GetMethod!.IsPublic);
            Assert.Null(properties[index].SetMethod);
        }
    }

    private static MethodInfo[] CallableDeclaredMethods(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => (method.IsPublic || method.IsAssembly) && !method.IsSpecialName)
            .ToArray();

    private static void AssertNoCallableDeclaredMethods(Type type) => Assert.Empty(CallableDeclaredMethods(type));
}
