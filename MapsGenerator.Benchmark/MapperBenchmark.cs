//using AutoMapper;
//using BenchmarkDotNet.Attributes;
//using MapsGenerator.Tests.Common.Models;
//using MapsGenerator.Tests.Common.Models.Destination;
//using MapsGenerator.Tests.Common.Models.Source;

//namespace MapsGenerator.Tests.Common;

//[MemoryDiagnoser]
//[RankColumn]
//public class MapperBenchmark
//{
//    private readonly Person _person;
//    private readonly IMapper _autoMapper;
//    private readonly MapGenerator _generatedMapper;

//    public MapperBenchmark()
//    {
//        _person = new Person
//        {
//            FirstName = "abc",
//            LastName = "cede",
//            Age = 10,
//            Address = new Address
//            {
//                Street = "adfawe",
//                City = "ffff"
//            },
//            Traits = new Traits
//            {
//                Zodiac = "sotk"
//            }
//        };

//        _autoMapper = new MapperConfiguration(x => x.AddProfile(new PersonProfile())).CreateMapper();
//        _generatedMapper = new MapGenerator();
//    }

//    [Benchmark]
//    public PersonDto UsingMapGenerator()
//    {
//        _generatedMapper.Map(_person, out var result);
//        return result;
//    }

//    [Benchmark]
//    public PersonDto UsingTryMapGenerator()
//    {
//        _generatedMapper.TryMap(_person, out var result);
//        return result;
//    }

//    [Benchmark]
//    public PersonDto UsingAutoMapper()
//    {
//        return _autoMapper.Map<PersonDto>(_person);
//    }
//}