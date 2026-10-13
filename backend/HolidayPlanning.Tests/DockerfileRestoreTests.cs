using System.Text.RegularExpressions;
using Xunit;

namespace HolidayPlanning.Tests;

/// <summary>
/// Guards the one build defect that CI is structurally incapable of catching.
///
/// <para>
/// <c>backend/Dockerfile</c> copies each project's <c>.csproj</c> individually (for layer
/// caching) and then runs a bare <c>dotnet restore</c>, which restores
/// <c>HolidayPlanning.slnx</c> — so every project listed in the solution must have a matching
/// COPY line. CI restores from a full checkout and is therefore always green; only the image
/// build sees the truncated context, and only on a push to the deploy branch, after the merge,
/// where nobody is looking.
/// </para>
/// <para>
/// This has already taken a production site down once: a test project was added to the solution
/// and the image build failed with <c>MSB3202</c> for two days behind green checks and two
/// merged promotions.
/// </para>
/// </summary>
public class DockerfileRestoreTests
{
    private static readonly Regex SolutionProject =
        new(@"<Project\s+Path=""(?<path>[^""]+\.csproj)""", RegexOptions.Compiled);

    // Matches `COPY HolidayPlanning.Foo/*.csproj ./HolidayPlanning.Foo/` and the explicit-filename form.
    private static readonly Regex DockerfileCopy =
        new(@"^\s*COPY\s+(?<dir>[^\s/]+)/[^\s]*\.csproj\s", RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>Walks up from the test assembly to the directory holding the solution file.</summary>
    private static DirectoryInfo BackendDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "HolidayPlanning.slnx")))
        {
            dir = dir.Parent;
        }

        Assert.True(dir is not null, "Could not locate HolidayPlanning.slnx by walking up from " + AppContext.BaseDirectory);
        return dir!;
    }

    /// <summary>Project directories listed in the solution, e.g. "HolidayPlanning.WebApi".</summary>
    internal static IReadOnlyList<string> SolutionProjectDirectories(string slnxContent)
    {
        var dirs = SolutionProject.Matches(slnxContent)
            .Select(m => m.Groups["path"].Value.Replace('\\', '/').Split('/')[0])
            .Distinct()
            .ToList();

        // A parse that finds nothing must fail loudly rather than vacuously pass: "no projects"
        // and "the format changed and I could not read it" are the same value otherwise.
        Assert.True(dirs.Count > 0, "Parsed zero projects out of HolidayPlanning.slnx — the format changed?");
        return dirs;
    }

    /// <summary>Project directories whose .csproj the Dockerfile copies before restoring.</summary>
    internal static IReadOnlyList<string> DockerfileCopiedDirectories(string dockerfileContent)
    {
        var dirs = DockerfileCopy.Matches(dockerfileContent)
            .Select(m => m.Groups["dir"].Value)
            .Distinct()
            .ToList();

        Assert.True(dirs.Count > 0, "Parsed zero csproj COPY lines out of backend/Dockerfile — the format changed?");
        return dirs;
    }

    [Fact]
    public void Dockerfile_copies_a_csproj_for_every_project_in_the_solution()
    {
        var backend = BackendDirectory();
        var slnx = File.ReadAllText(Path.Combine(backend.FullName, "HolidayPlanning.slnx"));
        var dockerfile = File.ReadAllText(Path.Combine(backend.FullName, "Dockerfile"));

        var inSolution = SolutionProjectDirectories(slnx);
        var copied = DockerfileCopiedDirectories(dockerfile);

        var missing = inSolution.Except(copied).ToList();

        Assert.True(
            missing.Count == 0,
            $"backend/Dockerfile does not COPY the .csproj for: {string.Join(", ", missing)}. "
            + "`dotnet restore` in the image restores HolidayPlanning.slnx, so the build will fail with "
            + "MSB3202 — but only in the image build on the deploy branch, never in CI. "
            + $"Add: COPY {missing.FirstOrDefault()}/*.csproj ./{missing.FirstOrDefault()}/");
    }

    [Fact]
    public void Dockerfile_does_not_copy_a_csproj_for_a_project_that_left_the_solution()
    {
        var backend = BackendDirectory();
        var slnx = File.ReadAllText(Path.Combine(backend.FullName, "HolidayPlanning.slnx"));
        var dockerfile = File.ReadAllText(Path.Combine(backend.FullName, "Dockerfile"));

        var stale = DockerfileCopiedDirectories(dockerfile)
            .Except(SolutionProjectDirectories(slnx))
            .ToList();

        Assert.True(
            stale.Count == 0,
            $"backend/Dockerfile copies a .csproj for {string.Join(", ", stale)}, which is not in "
            + "HolidayPlanning.slnx. The COPY will fail the build outright once the directory goes.");
    }

    // --- The parsers are asserted against known-bad inputs, so the guard above cannot go green
    // --- by quietly failing to read either file. Each case is a real defect that has shipped.

    [Fact]
    public void Guard_detects_the_defect_that_took_the_portfolio_down()
    {
        // The exact pre-fix pair: Tests is in the solution, absent from the Dockerfile.
        const string slnx = """
            <Solution>
              <Project Path="HolidayPlanning.WebApi/HolidayPlanning.WebApi.csproj" />
              <Project Path="HolidayPlanning.Tests/HolidayPlanning.Tests.csproj" />
            </Solution>
            """;
        const string dockerfile = "COPY HolidayPlanning.slnx .\nCOPY HolidayPlanning.WebApi/*.csproj ./HolidayPlanning.WebApi/\nRUN dotnet restore\n";

        var missing = SolutionProjectDirectories(slnx).Except(DockerfileCopiedDirectories(dockerfile));

        Assert.Equal(new[] { "HolidayPlanning.Tests" }, missing);
    }

    [Fact]
    public void Guard_detects_a_newly_added_project_such_as_a_source_generator()
    {
        // web-template#83 (the source-generator spike) adds a 9th project to the solution and no
        // COPY line — it would re-break the image build the moment it merged.
        const string slnx = """
            <Solution>
              <Project Path="HolidayPlanning.WebApi/HolidayPlanning.WebApi.csproj" />
              <Project Path="HolidayPlanning.SourceGenerators/HolidayPlanning.SourceGenerators.csproj" />
            </Solution>
            """;
        const string dockerfile = "COPY HolidayPlanning.WebApi/*.csproj ./HolidayPlanning.WebApi/\nRUN dotnet restore\n";

        var missing = SolutionProjectDirectories(slnx).Except(DockerfileCopiedDirectories(dockerfile));

        Assert.Equal(new[] { "HolidayPlanning.SourceGenerators" }, missing);
    }

    [Fact]
    public void Guard_accepts_a_correct_pair()
    {
        const string slnx = """
            <Solution>
              <Project Path="HolidayPlanning.WebApi/HolidayPlanning.WebApi.csproj" />
              <Project Path="HolidayPlanning.Tests/HolidayPlanning.Tests.csproj" />
            </Solution>
            """;
        const string dockerfile = """
            COPY HolidayPlanning.slnx .
            COPY HolidayPlanning.WebApi/*.csproj ./HolidayPlanning.WebApi/
            COPY HolidayPlanning.Tests/*.csproj ./HolidayPlanning.Tests/
            RUN dotnet restore
            """;

        Assert.Empty(SolutionProjectDirectories(slnx).Except(DockerfileCopiedDirectories(dockerfile)));
    }
}
