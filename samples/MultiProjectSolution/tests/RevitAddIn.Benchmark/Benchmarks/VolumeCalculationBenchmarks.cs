using BenchmarkDotNet.Attributes;
using Nice3point.BenchmarkDotNet.Revit;

namespace RevitAddIn.Benchmark.Benchmarks;

[PublicAPI]
[MemoryDiagnoser]
public class VolumeCalculationBenchmarks : RevitApiBenchmark
{
    private Document? _document;
    private Element[] _geometryElements = [];

    [Params(0, 1)]
    public int ElementSizeIndex { get; set; }
    private Element GeometryElement => _geometryElements[ElementSizeIndex];

    protected sealed override void OnSetup()
    {
        var samplesPath = $@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples";
        var rvtFile = Directory.EnumerateFiles(samplesPath, "*.rvt")
            .OrderBy(file => new FileInfo(file).Length)
            .FirstOrDefault();

        if (rvtFile is null) throw new InvalidOperationException("No .rvt files found in samples folder");

        _document = Application.OpenDocumentFile(rvtFile);

        var elementsWithSolids = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .Where(element =>
            {
                var geometry = element.get_Geometry(new Options());
                return geometry is not null && geometry.OfType<Solid>().Any(solid => solid.Volume > 0);
            })
            .ToArray();

        var largestElement = elementsWithSolids
            .OrderByDescending(GetDiagonal)
            .First();

        var smallestElement = elementsWithSolids
            .Where(element => GetDiagonal(element) > 1e-1)
            .OrderBy(GetDiagonal)
            .First();

        _geometryElements = [smallestElement, largestElement];
    }

    protected sealed override void OnCleanup()
    {
        _document?.Close(false);
    }

    [Benchmark(Baseline = true)]
    public double Coarse_SolidsOnly_Foreach()
    {
        var totalVolume = 0d;
        var geometry = GeometryElement.get_Geometry(new Options { DetailLevel = ViewDetailLevel.Coarse });

        foreach (var geometryObject in geometry)
        {
            if (geometryObject is Solid { Volume: > 0 } solid)
            {
                totalVolume += solid.Volume;
            }
        }

        return totalVolume;
    }

    [Benchmark]
    public double Medium_SolidsOnly_Foreach()
    {
        var totalVolume = 0d;
        var geometry = GeometryElement.get_Geometry(new Options { DetailLevel = ViewDetailLevel.Medium });

        foreach (var geometryObject in geometry)
        {
            if (geometryObject is Solid { Volume: > 0 } solid)
            {
                totalVolume += solid.Volume;
            }
        }

        return totalVolume;
    }

    [Benchmark]
    public double Fine_SolidsOnly_Foreach()
    {
        var totalVolume = 0d;
        var geometry = GeometryElement.get_Geometry(new Options { DetailLevel = ViewDetailLevel.Fine });

        foreach (var geometryObject in geometry)
        {
            if (geometryObject is Solid { Volume: > 0 } solid)
            {
                totalVolume += solid.Volume;
            }
        }

        return totalVolume;
    }

    [Benchmark]
    public double Fine_SolidsOnly_Linq()
    {
        var geometry = GeometryElement.get_Geometry(new Options { DetailLevel = ViewDetailLevel.Fine });
        return geometry?.OfType<Solid>().Where(solid => solid.Volume > 0).Sum(solid => solid.Volume) ?? 0d;
    }

    [Benchmark]
    public double Fine_WithInstances_Recursive()
    {
        var geometry = GeometryElement.get_Geometry(new Options { DetailLevel = ViewDetailLevel.Fine });
        return CalculateVolumeRecursive(geometry);
    }

    [Benchmark]
    public double Fine_WithInstances_Stack()
    {
        var geometry = GeometryElement.get_Geometry(new Options { DetailLevel = ViewDetailLevel.Fine });
        return CalculateVolumeIterative(geometry);
    }

    private static double GetDiagonal(Element element)
    {
        var boundingBox = element.get_BoundingBox(null);
        return boundingBox?.Max.DistanceTo(boundingBox.Min) ?? double.MinValue;
    }

    private static double CalculateVolumeRecursive(GeometryElement geometryElement)
    {
        var totalVolume = 0d;

        foreach (var geometryObject in geometryElement)
        {
            totalVolume += geometryObject switch
            {
                Solid { Volume: > 0 } solid => solid.Volume,
                GeometryInstance instance => CalculateVolumeRecursive(instance.GetInstanceGeometry()),
                GeometryElement nested => CalculateVolumeRecursive(nested),
                _ => 0d
            };
        }

        return totalVolume;
    }

    private static double CalculateVolumeIterative(GeometryElement geometryElement)
    {
        var totalVolume = 0d;
        var stack = new Stack<GeometryElement>();
        stack.Push(geometryElement);

        while (stack.Count > 0)
        {
            var currentGeometry = stack.Pop();
            foreach (var geometryObject in currentGeometry)
            {
                switch (geometryObject)
                {
                    case Solid { Volume: > 0 } solid:
                        totalVolume += solid.Volume;
                        break;
                    case GeometryInstance instance:
                        stack.Push(instance.GetInstanceGeometry());
                        break;
                    case GeometryElement nestedGeometry:
                        stack.Push(nestedGeometry);
                        break;
                }
            }
        }

        return totalVolume;
    }
}