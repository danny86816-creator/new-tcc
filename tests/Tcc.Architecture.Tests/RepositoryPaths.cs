namespace Tcc.Architecture.Tests;

internal static class RepositoryPaths
{
    public static string Root { get; } = FindRepositoryRoot();

    public static string ThemeContracts => Path.Combine(Root, "contracts", "theme");

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
