using Shouldly;

namespace TechXYZ.GymXYZ.WebApp.Tests;

/// <summary>
/// Reaches the web app's source files from the test binaries.
/// <para>
/// Stylesheets and <c>.razor</c> files are not copied to the test output, so
/// the tests that read them as text — theme blocks, markup hygiene — have to
/// walk back up to the repository. That walk lived in
/// <see cref="Features.BrandThemeTests"/> until a second and third test needed
/// it; it is here so all of them break in the same place if the layout moves.
/// </para>
/// </summary>
internal static class RepositoryFiles
{
    /// <summary>The folder holding the solution file.</summary>
    internal static DirectoryInfo Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "TechXyz.GymXyz.sln")))
        {
            directory = directory.Parent;
        }

        directory.ShouldNotBeNull("Could not find the repository root from the test binaries.");

        return directory;
    }

    /// <summary>The web app project folder.</summary>
    internal static DirectoryInfo WebApp() =>
        new(Path.Combine(Root().FullName, "TechXyz.GymXyz.WebApp"));

    /// <summary>Reads a file under the web app project, by path segments.</summary>
    internal static string ReadWebAppFile(params string[] segments)
    {
        var path = Path.Combine([WebApp().FullName, .. segments]);

        File.Exists(path).ShouldBeTrue($"Expected a file at {path}.");

        return File.ReadAllText(path);
    }

    /// <summary>
    /// Every file of the web app matching a search pattern, excluding the build
    /// folders — <c>obj</c> holds generated copies of the very files being
    /// scanned, and finding the same fault twice helps nobody.
    /// </summary>
    internal static IEnumerable<FileInfo> WebAppFiles(string searchPattern) =>
        WebApp()
            .EnumerateFiles(searchPattern, SearchOption.AllDirectories)
            .Where(file => !file.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                           && !file.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

    /// <summary>The path of a file relative to the repository root, for messages.</summary>
    internal static string RelativePath(FileInfo file) =>
        Path.GetRelativePath(Root().FullName, file.FullName);
}
