using System.Reflection;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class DependencyBoundaryTests
{
    private const string PresentationContractsProject = "src/Tcc.Presentation.Contracts/Tcc.Presentation.Contracts.csproj";
    private const string ThemesProject = "src/Tcc.Themes/Tcc.Themes.csproj";
    private const string ThemeFeatureProject = "src/Tcc.Features.Themes/Tcc.Features.Themes.csproj";
    private const string WindowsProject = "src/Tcc.Windows/Tcc.Windows.csproj";
    private const string DesktopHostProject = "src/Tcc.DesktopHost/Tcc.DesktopHost.csproj";
    private const string ArchitectureTestsProject = "tests/Tcc.Architecture.Tests/Tcc.Architecture.Tests.csproj";

    private static readonly string[] RuntimeForbiddenReferences =
    [
        "Tcc.Domain",
        "Tcc.Application",
        "Tcc.Application.Abstractions",
        "Tcc.Persistence",
        "Tcc.Risk",
        "Tcc.Permissions",
        "Tcc.Audit",
        "Tcc.Sync",
        "Tcc.Recovery",
        "Tcc.Connectors.ReadOnly",
        "Tcc.AI",
        "Tcc.Plugins",
        "Tcc.Features.Themes",
    ];

    private static readonly string[] FeatureForbiddenImplementationReferences =
    [
        "Tcc.Domain",
        "Tcc.Application",
        "Tcc.Persistence",
        "Tcc.Security",
        "Tcc.Recovery",
        "Tcc.Connectors.ReadOnly",
        "Tcc.AI",
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> ApprovedProjectReferenceAllowlist =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [PresentationContractsProject] = CreateReferenceSet(),
            [ThemesProject] = CreateReferenceSet(PresentationContractsProject),
            [ThemeFeatureProject] = CreateReferenceSet(PresentationContractsProject, ThemesProject),
            [WindowsProject] = CreateReferenceSet(),
            [DesktopHostProject] = CreateReferenceSet(
                ThemeFeatureProject,
                PresentationContractsProject,
                ThemesProject,
                WindowsProject),
            [ArchitectureTestsProject] = CreateReferenceSet(
                ThemeFeatureProject,
                PresentationContractsProject,
                ThemesProject,
                WindowsProject),
        };

    [Fact]
    public void RepositoryProjectReferencesMatchApprovedAllowlist()
    {
        string repositoryRoot = FindRepositoryRoot();
        Dictionary<string, IReadOnlySet<string>> dependencyGraph =
            LoadProjectDependencyGraph(repositoryRoot);

        string[] violations = FindDependencyViolations(
            dependencyGraph,
            ApprovedProjectReferenceAllowlist);

        Assert.Empty(violations);
    }

    [Fact]
    public void ThemeRuntimeDoesNotReferenceThemeFeatureProject()
    {
        Dictionary<string, IReadOnlySet<string>> dependencyGraph =
            LoadProjectDependencyGraph(FindRepositoryRoot());

        Assert.DoesNotContain(
            dependencyGraph[ThemesProject],
            dependency => StringComparer.OrdinalIgnoreCase.Equals(dependency, ThemeFeatureProject));
    }

    [Fact]
    public void UnusedForbiddenProjectReferenceIsRejected()
    {
        string fixtureRoot = Path.Combine(
            Path.GetTempPath(),
            $"tcc-project-reference-fixture-{Guid.NewGuid():N}");

        try
        {
            string themesDirectory = Path.Combine(fixtureRoot, "src", "Tcc.Themes");
            string featureDirectory = Path.Combine(fixtureRoot, "src", "Tcc.Features.Themes");
            Directory.CreateDirectory(themesDirectory);
            Directory.CreateDirectory(featureDirectory);

            File.WriteAllText(
                Path.Combine(themesDirectory, "Tcc.Themes.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <ProjectReference Include="../Tcc.Features.Themes/Tcc.Features.Themes.csproj" />
                  </ItemGroup>
                </Project>
                """);
            File.WriteAllText(
                Path.Combine(featureDirectory, "Tcc.Features.Themes.csproj"),
                "<Project Sdk=\"Microsoft.NET.Sdk\" />");

            Dictionary<string, IReadOnlySet<string>> dependencyGraph =
                LoadProjectDependencyGraph(fixtureRoot);
            IReadOnlyDictionary<string, IReadOnlySet<string>> fixtureAllowlist =
                new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    [ThemesProject] = CreateReferenceSet(),
                    [ThemeFeatureProject] = CreateReferenceSet(),
                };

            string[] violations = FindDependencyViolations(dependencyGraph, fixtureAllowlist);

            Assert.Contains(
                violations,
                violation => violation.Contains(
                    $"{ThemesProject} -> {ThemeFeatureProject}",
                    StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(fixtureRoot))
            {
                Directory.Delete(fixtureRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void ThemeRuntimeDoesNotReferenceForbiddenCoreAssemblies()
    {
        AssertDoesNotReference(
            typeof(Tcc.Themes.AssemblyMarker).Assembly,
            RuntimeForbiddenReferences);
    }

    [Fact]
    public void ThemeFeatureDoesNotReferenceForbiddenImplementationAssemblies()
    {
        AssertDoesNotReference(
            typeof(Tcc.Features.Themes.AssemblyMarker).Assembly,
            FeatureForbiddenImplementationReferences);
    }

    private static void AssertDoesNotReference(Assembly assembly, string[] forbiddenReferences)
    {
        string[] actualReferences = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(
            actualReferences,
            reference => forbiddenReferences.Contains(reference, StringComparer.Ordinal));
    }

    private static Dictionary<string, IReadOnlySet<string>> LoadProjectDependencyGraph(
        string repositoryRoot)
    {
        string canonicalRoot = Path.GetFullPath(repositoryRoot);
        string[] projectFiles = Directory
            .EnumerateFiles(canonicalRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .Where(projectPath => !ContainsBuildOutputSegment(canonicalRoot, projectPath))
            .OrderBy(projectPath => projectPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Dictionary<string, string> repositoryProjectsByCanonicalPath = projectFiles.ToDictionary(
            projectPath => projectPath,
            projectPath => NormalizeRelativePath(canonicalRoot, projectPath),
            StringComparer.OrdinalIgnoreCase);
        Dictionary<string, IReadOnlySet<string>> graph =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (string projectPath in projectFiles)
        {
            XDocument projectDocument = XDocument.Load(projectPath, LoadOptions.None);
            string projectDirectory = Path.GetDirectoryName(projectPath)
                ?? throw new InvalidOperationException($"Project directory is unavailable: {projectPath}");

            HashSet<string> references = new(StringComparer.OrdinalIgnoreCase);
            foreach (XElement projectReference in projectDocument
                         .Descendants()
                         .Where(element => element.Name.LocalName == "ProjectReference"))
            {
                string? include = projectReference.Attribute("Include")?.Value;
                if (string.IsNullOrWhiteSpace(include))
                {
                    throw new InvalidOperationException(
                        $"ProjectReference without Include in {projectPath}");
                }

                string canonicalReferencePath = Path.GetFullPath(
                    Path.Combine(projectDirectory, include));
                string normalizedReference = repositoryProjectsByCanonicalPath.TryGetValue(
                    canonicalReferencePath,
                    out string? repositoryRelativeReference)
                    ? repositoryRelativeReference
                    : canonicalReferencePath.Replace('\\', '/');

                references.Add(normalizedReference);
            }

            graph.Add(repositoryProjectsByCanonicalPath[projectPath], references);
        }

        return graph;
    }

    private static string[] FindDependencyViolations(
        IReadOnlyDictionary<string, IReadOnlySet<string>> dependencyGraph,
        IReadOnlyDictionary<string, IReadOnlySet<string>> allowlist)
    {
        List<string> violations = [];

        foreach (string project in dependencyGraph.Keys.OrderBy(
                     project => project,
                     StringComparer.OrdinalIgnoreCase))
        {
            if (!allowlist.TryGetValue(project, out IReadOnlySet<string>? allowedReferences))
            {
                violations.Add($"Unapproved project discovered: {project}");
                continue;
            }

            foreach (string reference in dependencyGraph[project].OrderBy(
                         reference => reference,
                         StringComparer.OrdinalIgnoreCase))
            {
                if (!allowedReferences.Contains(reference))
                {
                    violations.Add($"Unauthorized ProjectReference: {project} -> {reference}");
                }
            }
        }

        foreach (string missingProject in allowlist.Keys
                     .Except(dependencyGraph.Keys, StringComparer.OrdinalIgnoreCase)
                     .OrderBy(project => project, StringComparer.OrdinalIgnoreCase))
        {
            violations.Add($"Approved project is missing: {missingProject}");
        }

        return [.. violations];
    }

    private static HashSet<string> CreateReferenceSet(params string[] references) =>
        new HashSet<string>(references, StringComparer.OrdinalIgnoreCase);

    private static bool ContainsBuildOutputSegment(string repositoryRoot, string projectPath)
    {
        string relativePath = Path.GetRelativePath(repositoryRoot, projectPath);
        return relativePath
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment =>
                segment.Equals("bin", StringComparison.OrdinalIgnoreCase)
                || segment.Equals("obj", StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeRelativePath(string repositoryRoot, string path) =>
        Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Tcc.slnx"))
                && File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            {
                return Path.GetFullPath(directory.FullName);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from {AppContext.BaseDirectory}.");
    }
}
