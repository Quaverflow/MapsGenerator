using AutoFixture;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using MapsGenerator.Tests.Common;
using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Benchmark;

[MemoryDiagnoser]
[RankColumn]
public class MapperBenchmark
{
    private readonly IMapper _autoMapper;
    private readonly Company _company;
    private readonly MapGenerator _generator;

    public MapperBenchmark()
    {
        var fixture = new Fixture();
        _company = fixture.Create<Company>();

        _autoMapper = new MapperConfiguration(x => x.AddProfile(new AutoMapperProfile())).CreateMapper();
        _generator = new MapGenerator();

    }

    [Benchmark]
    public CompanyDto UsingMapGenerator()
    {
        return _generator.MapToMapsGeneratorTestsCommonModelsDestinationCompanyDto(_company);
    }

    [Benchmark]
    public CompanyDto UsingTryMapGenerator()
    {
        _generator.TryMapToMapsGeneratorTestsCommonModelsDestinationCompanyDto(_company, out var result);
        return result!;
    }

    [Benchmark]
    public CompanyDto UsingAutoMapper()
    {
        return _autoMapper.Map<CompanyDto>(_company);
    }
}