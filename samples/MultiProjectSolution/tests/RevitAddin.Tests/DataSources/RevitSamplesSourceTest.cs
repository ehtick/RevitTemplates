using Nice3point.TUnit.Revit;

namespace RevitAddin.Tests.DataSources;

public class RevitSamplesSourceTest : RevitApiTest
{
    private static readonly string SamplesPath = $@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples";

    [Before(Class)]
    public static void ValidateSamples()
    {
        if (!Directory.Exists(SamplesPath))
        {
            Skip.Test($"Samples folder not found at {SamplesPath}");
            return;
        }

        if (!Directory.EnumerateFiles(SamplesPath, "*.rfa").Any())
        {
            Skip.Test($"No .rfa files found in {SamplesPath}");
        }
    }

    public static IEnumerable<string> GetSampleFiles()
    {
        if (!Directory.Exists(SamplesPath))
        {
            yield return string.Empty;
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(SamplesPath, "*.rfa")) yield return file;
    }
}