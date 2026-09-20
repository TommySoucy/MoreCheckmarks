using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace MoreCheckmarks.Tests;

public class PackagingTests
{
    private const string TestVersion = "1.2.3";
    private static readonly string[] RequiredFiles =
    {
        "BepInEx/plugins/MoreCheckmarks/MoreCheckmarks.dll",
        "BepInEx/plugins/MoreCheckmarks/MoreCheckmarksAssets",
        "SPT_Runtime/user/mods/MoreCheckmarksBackend/MoreCheckmarksBackend.dll",
        "SPT_Runtime/user/mods/MoreCheckmarksBackend/MoreCheckmarksBackend.deps.json",
        "SPT_Runtime/user/mods/MoreCheckmarksBackend/quest-id-reference.txt"
    };

    [Theory]
    [InlineData("Release")]
    [InlineData("Debug")]
    public void ArchiveContainsOnlyCurrentDistributionFiles(string configuration)
    {
        using var fixture = new PackageFixture();
        fixture.WriteDist("SPT/user/mods/MoreCheckmarksBackend/old.dll", "old backend");
        fixture.WriteDist("BepInEx/plugins/MoreCheckmarks/obsolete.dll", "old plugin");
        fixture.WriteDist("SPT_Runtime/user/mods/MoreCheckmarksBackend/config.json", "local settings");

        // Package twice: stale files in both dist and the previous staging directory must be excluded.
        for (var run = 0; run < 2; run++)
        {
            var result = fixture.Package(configuration);
            Assert.True(result.exitCode == 0, result.output);
            var zipName = $"MoreCheckmarks-{(configuration == "Debug" ? "Debug-" : "")}{TestVersion}.zip";
            using var archive = ZipFile.OpenRead(Path.Combine(fixture.Root, zipName));
            Assert.Equal(RequiredFiles.OrderBy(x => x), archive.Entries.Select(x => x.FullName.Replace('\\', '/')).OrderBy(x => x));
            foreach (var entry in archive.Entries)
            {
                using var reader = new StreamReader(entry.Open());
                Assert.Equal("current", reader.ReadToEnd());
            }

            var staleStageFile = Path.Combine(fixture.Root, "obj", configuration, "package", "obsolete.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(staleStageFile));
            File.WriteAllText(staleStageFile, "old staging file");
        }
    }

    [Theory]
    [InlineData("BepInEx/plugins/MoreCheckmarks/MoreCheckmarks.dll")]
    [InlineData("BepInEx/plugins/MoreCheckmarks/MoreCheckmarksAssets")]
    [InlineData("SPT_Runtime/user/mods/MoreCheckmarksBackend/MoreCheckmarksBackend.dll")]
    public void MissingRequiredFileFailsInsteadOfPublishingPartialArchive(string missingFile)
    {
        using var fixture = new PackageFixture();
        File.Delete(Path.Combine(fixture.Root, "dist", missingFile));
        var result = fixture.Package("Release");
        Assert.NotEqual(0, result.exitCode);
        Assert.Contains("Missing package file", result.output);
        Assert.False(File.Exists(Path.Combine(fixture.Root, $"MoreCheckmarks-{TestVersion}.zip")));
    }

    private sealed class PackageFixture : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "MoreCheckmarks-packaging-" + Guid.NewGuid());

        public PackageFixture()
        {
            foreach (var file in RequiredFiles)
                WriteDist(file, "current");
        }

        public void WriteDist(string relativePath, string contents)
        {
            var path = Path.Combine(Root, "dist", relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, contents);
        }

        public (int exitCode, string output) Package(string configuration)
        {
            var repo = new DirectoryInfo(AppContext.BaseDirectory);
            while (repo != null && !File.Exists(Path.Combine(repo.FullName, "MoreCheckmarks.sln")))
                repo = repo.Parent;
            Assert.NotNull(repo);

            // Execute the real packaging target without compiling or deploying either mod.
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = repo.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            foreach (var argument in new[] {
                "msbuild", "Server/MoreCheckmarksBackend.csproj", "-nologo", "-t:PackageMod",
                "-p:Version=" + TestVersion,
                "-p:Configuration=" + configuration,
                "-p:DistDir=" + Path.Combine(Root, "dist"),
                "-p:PackageOutputDir=" + Root,
                "-p:BaseIntermediateOutputPath=" + Path.Combine(Root, "obj") + Path.DirectorySeparatorChar
            })
                start.ArgumentList.Add(argument);
            using var process = Process.Start(start);
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(30000))
            {
                process.Kill(entireProcessTree: true);
                throw new TimeoutException("Packaging target did not finish within 30 seconds.");
            }
            return (process.ExitCode, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
