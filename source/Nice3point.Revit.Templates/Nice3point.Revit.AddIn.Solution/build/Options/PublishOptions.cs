namespace Build.Options;

/// <summary>
///     Release publishing options.
/// </summary>
[Serializable]
public sealed record PublishOptions
{
    /// <summary>
    ///     Release version.
    /// </summary>
    /// <remarks>
    ///     This will override the version determined by GitVersion.Tool. <br/>
    /// </remarks>
    /// <example>
    ///     1.0.0-alpha.1.250101 <br/>
    ///     1.0.0-beta.2.250101 <br/>
    ///     1.0.0
    /// </example>
    public string? Version { get; init; }
    
    /// <summary>
    ///     Path to the changelog file.
    /// </summary>
    public string? ChangelogFile { get; init; }
}