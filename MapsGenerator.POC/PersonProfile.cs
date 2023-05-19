namespace MapsGenerator.POC;

internal class PersonProfile : MapperBase
{
    public PersonProfile()
    {
        Map<Person, PersonDto>(x =>
        { 
            x.Exclude(y => y.LastName);
            x.MapFrom(d => d.Zodiac, s => s.Traits.Zodiac);
        });
        Map<Address, AddressDto>();

    }
}