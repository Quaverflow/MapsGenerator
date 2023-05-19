//HintName: MapperImplementation.cs
namespace MapsGenerator
{
    public class MapperImplementation
    {
        public somenamespace.PersonDto Person_To_somenamespacePersonDto(somenamespace.Person source)
        {
            return new somenamespace.PersonDto
            {
                FirstName = source.FirstName,
                //LastName was manually excluded
                Age = source.Age,
                Height = source.Height,
                Address = Address_To_somenamespaceAddressDto(source.Address)
                Zodiac = source.Traits.Zodiac,
            };
        }

        public somenamespace.AddressDto Address_To_somenamespaceAddressDto(somenamespace.Address source)
        {
            return new somenamespace.AddressDto
            {
                Street = source.Street,
                City = source.City,
            };
        }

    }
}
