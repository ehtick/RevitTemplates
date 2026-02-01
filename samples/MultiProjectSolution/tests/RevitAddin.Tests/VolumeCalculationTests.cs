using ModelessModule.Services;
using Nice3point.TUnit.Revit.Executors;
using RevitAddin.Tests.DataSources;
using TUnit.Core.Executors;

namespace RevitAddin.Tests;

[DependencyInjectionDataSource]
public sealed class VolumeCalculationTests(ElementMetadataExtractionService extractionService) : RevitSamplesSourceTest
{
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task CalculateVolume_WhenElementIsNull_ReturnsZero(string filePath)
    {
        var result = extractionService.CalculateVolume(null);

        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task CalculateVolume_ElementsWithGeometry_ReturnsVolume(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var elementsWithGeometry = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .Where(element => element.get_Geometry(new Options()) is not null)
                .Take(10);

            foreach (var element in elementsWithGeometry)
            {
                var result = extractionService.CalculateVolume(element);

                await Assert.That(result).IsGreaterThanOrEqualTo(0);
            }
        }
        finally
        {
            document?.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task CalculateVolume_SolidElements_ReturnsPositiveVolume(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var solidElements = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .Where(element =>
                {
                    var geometry = element.get_Geometry(new Options());
                    return geometry is not null && geometry.OfType<Solid>().Any(solid => solid.Volume > 0);
                })
                .Take(5)
                .ToArray();

            if (solidElements.Length == 0)
            {
                Skip.Test("No solid elements found");
                return;
            }

            foreach (var element in solidElements)
            {
                var result = extractionService.CalculateVolume(element);

                await Assert.That(result).IsGreaterThan(0);
            }
        }
        finally
        {
            document?.Close(false);
        }
    }
}