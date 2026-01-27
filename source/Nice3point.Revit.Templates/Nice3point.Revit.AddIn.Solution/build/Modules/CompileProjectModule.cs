using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using Sourcy.DotNet;

namespace Build.Modules;

/// <summary>
///     Compile the add-in for each supported Revit configuration.
/// </summary>
[DependsOn<ResolveVersioningModule>]
[DependsOn<ResolveConfigurationsModule>]
public sealed class CompileProjectModule : Module
{
    protected override void ExecuteModule(IModuleContext context, CancellationToken cancellationToken)
    {
        var versioningResult = await context.GetModule<ResolveVersioningModule>();
        var configurationsResult = await context.GetModule<ResolveConfigurationsModule>();
        var versioning = versioningResult.ValueOrDefault!;
        var configurations = configurationsResult.ValueOrDefault!;

        foreach (var configuration in configurations)
        {
            await context.SubModule(configuration, async () => await CompileAsync(context, versioning, configuration, cancellationToken));
        }
    }

    /// <summary>
    ///     Compile the add-in project for the specified configuration.
    /// </summary>
    private static async Task CompileAsync(
        IModuleContext context,
        ResolveVersioningResult versioning,
        string configuration,
        CancellationToken cancellationToken)
    {
        await context.DotNet().Build(new DotNetBuildOptions
        {
            ProjectSolution = Solutions.Nice3point.Revit.AddIn.FullName,
            Configuration = configuration,
            Properties =
            [
                ("VersionPrefix", versioning.VersionPrefix),
                ("VersionSuffix", versioning.VersionSuffix!)
            ]
        }, cancellationToken: cancellationToken);
    }
}