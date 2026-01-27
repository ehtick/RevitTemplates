#if (hasArtifacts)
using Build.Options;
#endif
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Conditions;
using ModularPipelines.Context;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;
using Sourcy.DotNet;

namespace Build.Modules;

/// <summary>
///     Clean projects and artifact directories.
/// </summary>
[SkipIf<IsCI>]
#if (hasArtifacts)
public sealed class CleanProjectModule(IOptions<BuildOptions> buildOptions) : Module
#else
public sealed class CleanProjectModule() : Module
#endif
{
    protected override void ExecuteModule(IModuleContext context, CancellationToken cancellationToken)
    {
        var rootDirectory = context.Git().RootDirectory;
#if (hasArtifacts)
        var outputDirectory = rootDirectory.GetFolder(buildOptions.Value.OutputDirectory);
#endif
        var buildOutputDirectories = rootDirectory
            .GetFolders(folder => folder.Name is "bin" or "obj")
            .Where(folder => folder.Parent != Projects.Build.Directory);

        foreach (var buildFolder in buildOutputDirectories)
        {
            buildFolder.Clean();
        }
#if (hasArtifacts)

        if (outputDirectory.Exists)
        {
            outputDirectory.Clean();
        }
#endif
    }
}