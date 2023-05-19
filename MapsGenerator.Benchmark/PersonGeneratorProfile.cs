using MapsGenerator.Benchmark.Models;

namespace MapsGenerator.Benchmark;

internal class PersonGeneratorProfile : MapperBase
{
    public PersonGeneratorProfile()
    {
        Map<Person, PersonDto>(x =>
        { 
            x.Exclude(y => y.LastName);
            x.MapFrom(d => d.Zodiac, s => s.Traits.Zodiac);
        });
        Map<Address, AddressDto>();
    }
}