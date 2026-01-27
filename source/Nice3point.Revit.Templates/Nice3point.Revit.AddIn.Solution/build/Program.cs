using Build.Modules;
using Build.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines;
using ModularPipelines.Extensions;

var builder = Pipeline.CreateBuilder();

builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();
#if (hasArtifacts)

builder.Services.AddOptions<BuildOptions>().Bind(builder.Configuration.GetSection("Build"));
builder.Services.AddOptions<BundleOptions>().Bind(builder.Configuration.GetSection("Bundle"));
#endif
#if (isGitHubCi && hasArtifacts)
builder.Services.AddOptions<PublishOptions>().Bind(builder.Configuration.GetSection("Publish"));
#endif

if (args.Length == 0)
{
    builder.Services.AddModule<CompileProjectsModule>();
}
#if (hasArtifacts)

if (args.Contains("pack"))
{
    builder.Services.AddModule<CleanProjectsModule>();
#if (includeBundle)
    builder.Services..AddModule<CreateBundleModule>();
#endif
#if (includeInstaller)
    builder.Services..AddModule<CreateInstallerModule>();
#endif
}
#endif
#if (isGitHubCi && hasArtifacts)
if (args.Contains("publish"))
{
    builder.Services.AddModule<PublishGithubModule>();
}
#endif

await builder.Build().RunAsync();