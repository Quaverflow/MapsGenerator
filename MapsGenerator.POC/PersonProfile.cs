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
            x.MapFromParameter(d => d.FirstName);
            x.MapFromParameter(d => d.Address.City);
        });
        Map<Address, AddressDto>();

    }
}