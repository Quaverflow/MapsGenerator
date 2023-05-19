using AutoMapper;
using MapsGenerator.Benchmark.Models;

namespace MapsGenerator.Benchmark;

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<Person, PersonDto>()
            .ForMember(x => x.Zodiac, y => y.MapFrom(t => t.Traits.Zodiac))
            .ForMember(x => x.FirstName, y=> y.Ignore());

        CreateMap<Address, AddressDto>();
    }
}