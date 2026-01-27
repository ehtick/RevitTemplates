using System.ComponentModel.DataAnnotations;

namespace Build.Options;

/// <summary>
///     Build configuration options.
/// </summary>
[Serializable]
public sealed record BuildOptions
{
    /// <summary>
    ///     Path to build output
    /// </summary>
    [Required] public string OutputDirectory { get; init; } = null!;
}