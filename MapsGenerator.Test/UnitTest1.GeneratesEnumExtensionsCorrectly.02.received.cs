//HintName: MapperImplementation.cs
namespace MapsGenerator
{
    public class MapperImplementation
    {
        public void Map(somenamespace.Person source, out somenamespace.PersonDto destination)
        {
            Map(source.Address, out var Address address);
            destination = new somenamespace.PersonDto
            {
                FirstName = source.FirstName,
                Age = source.Age,
                Height = source.Height,
                Address = address,
                Zodiac = source.Traits.Zodiac,
                //{simpleProperty.DestinationProperty.Name} was manually excluded
            };
        }

        public void Map(somenamespace.Address source, out somenamespace.AddressDto destination)
        {
            destination = new somenamespace.AddressDto
            {
                Street = source.Street,
                City = source.City,
            };
        }

    }
}
