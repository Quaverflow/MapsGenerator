//HintName: MapperImplementation.cs
namespace MapsGenerator
{
    public classDeclarationSyntax MapperImplementation
    {
        public somenamespace.PersonDto Person_To_somenamespacePersonDto(somenamespace.Person source)
        {
            return new somenamespace.PersonDto
            {
                //FirstName was manually excluded
                LastName = source.LastName,
                Age = source.Age,
                Address = Address_To_somenamespaceAddressDto(source.Address)
            }
        }

        public somenamespace.AddressDto Address_To_somenamespaceAddressDto(somenamespace.Address source)
        {
            return new somenamespace.AddressDto
            {
                Street = source.Street,
                City = source.City,
            }
        }

    }
}
