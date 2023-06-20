using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Employee, PersonDto>()
            .ForMember(x => x.Height, y => y.MapFrom(z => z.PersonalDetails.Height))
            .ForMember(x => x.FirstName, y => y.MapFrom(z => z.PersonalDetails.FirstName))
            .ForMember(x => x.LastName, y => y.MapFrom(z => z.PersonalDetails.LastName))
            .ForMember(x => x.Age, y => y.MapFrom(z => z.PersonalDetails.Age))
            .ForMember(x => x.Address, y => y.MapFrom(z => z.PersonalDetails.Address))
            .AfterMap((s,d) =>
            {
                d.FirstName = "hello";
                d.Age = 3;
            });
        
        CreateMap<Address, AddressDto>();

        CreateMap<Company, CompanyDto>()
            .ForMember(x => x.TradingName, y => y.MapFrom(z => z.Name))
            .ForMember(x => x.Aliases, y => y.MapFrom(z => new SortedSet<string>(z.Aliases)))
            .ForMember(x => x.CarpetsIdsCollected, y => y.MapFrom(z =>new Stack<int>( z.CarpetsIdsCollected)))
            .ForMember(x => x.OrderIds, y => y.MapFrom(z => new Queue<Guid>(z.OrderIds)))
            .ForMember(x => x.AlternativeNames, y => y.MapFrom(z => new List<string>(z.AlternativeNames)))
            .ForMember(x => x.PetsNames, y => y.MapFrom(z => new HashSet<string>(z.PetsNames)))
            .ForMember(x => x.Workers, y => y.MapFrom(z => z.Employees))
            .AfterMap((s, d, r) =>
            {
                d.Bees = s.Employees.ToDictionary(a => a.Id, a => r.Mapper.Map<PersonDto>(a));
            });

        CreateMap<Seniority, SeniorityDto>()
            .ConvertUsingEnumMapping(x => x.MapValue(Seniority.Junior, SeniorityDto.Starter));
    }
}