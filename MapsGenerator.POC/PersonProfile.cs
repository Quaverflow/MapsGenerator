using MapsGenerator.POC.Models;

namespace MapsGenerator.POC;

internal class PersonProfile : MapperBase
{
    public PersonProfile()
    {
        Map<Person, PersonDto>(x =>
        {  
            x.Exclude(y => y.LastName); 
            x.MapFrom(d => d.Zodiac, s => s.Traits.Zodiac);
            x.MapFrom(d => d.StreamingPlatform, s => s.Lineage);
            x.MapFromParameter(d => d.FirstName);
            x.EnsureAllDestinationPropertiesAreMapped();
            //x.MapFromParameter(d => d.Address.City);
        });
        Map<Address, AddressDto>();

    }
}