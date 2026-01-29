using Build.Options;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using Sourcy.DotNet;

namespace Build.Modules;

/// <summary>
///     Pack the SDK NuGet package.
/// </summary>
[DependsOn<CleanProjectsModule>]
[DependsOn<ResolveVersioningModule>]
[DependsOn<UpdateTemplatesReadmeModule>(Optional = true)]
[DependsOn<CleanProjectsModule>(Optional = true)]
[DependsOn<GenerateNugetChangelogModule>(Optional = true)]
public sealed class PackSdkModule(IOptions<BuildOptions> buildOptions) : Module
{
    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var changelogModule = context.GetModuleIfRegistered<GenerateNugetChangelogModule>();

        var versioningResult = await context.GetModule<ResolveVersioningModule>();
        var changelogResult = changelogModule is null ? null : await changelogModule;

        var versioning = versioningResult.ValueOrDefault!;
        var changelog = changelogResult?.ValueOrDefault ?? string.Empty;
        var outputFolder = context.Git().RootDirectory.GetFolder(buildOptions.Value.OutputDirectory);

        await context.DotNet().Pack(new DotNetPackOptions
        {
            ProjectSolution = Projects.Nice3point_Revit_Sdk.FullName,
            Configuration = "Release",
            Output = outputFolder,
            Properties = new List<KeyValue>
            {
                ("VersionPrefix", versioning.VersionPrefix),
                ("VersionSuffix", versioning.VersionSuffix!),
                ("PackageReleaseNotes", changelog)
            }
        }, cancellationToken: cancellationToken);
    }
}