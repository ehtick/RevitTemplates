using ModelessModule.Services;
using Nice3point.TUnit.Revit.Executors;
using RevitAddin.Tests.DataSources;
using TUnit.Core.Executors;

namespace RevitAddin.Tests;

[DependencyInjectionDataSource]
public sealed class ElementMetadataExtractionServiceTests(ElementMetadataExtractionService extractionService) : RevitSamplesSourceTest
{
    [Test]
    public async Task ExtractMetadata_WhenElementIsNull_ReturnsNull()
    {
        var result = extractionService.ExtractMetadata(null);

        await Assert.That(result).IsNull();
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task ExtractMetadata_WithValidElements_ReturnsNonNullResult(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var elements = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .Take(10);

            foreach (var element in elements)
            {
                var result = extractionService.ExtractMetadata(element);

                await Assert.That(result).IsNotNull();
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
    public async Task ExtractMetadata_ElementsWithNullCategory_ReturnsEmptyCategoryName(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var elementsWithoutCategory = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .Where(element => element.Category is null)
                .Take(5)
                .ToArray();

            if (elementsWithoutCategory.Length == 0)
            {
                Skip.Test("No elements with null category found");
                return;
            }

            foreach (var element in elementsWithoutCategory)
            {
                var result = extractionService.ExtractMetadata(element);

                await Assert.That(result).IsNotNull();
                await Assert.That(result!.CategoryName).IsEqualTo(string.Empty);
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
    public async Task ExtractMetadata_ElementsWithCategory_ReturnsCategoryName(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var elementsWithCategory = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .Where(element => element.Category is not null)
                .Take(10);

            foreach (var element in elementsWithCategory)
            {
                var result = extractionService.ExtractMetadata(element);

                await Assert.That(result).IsNotNull();
                await Assert.That(result!.CategoryName).IsNotEmpty();
            }
        }
        finally
        {
            document?.Close(false);
        }
    }
}