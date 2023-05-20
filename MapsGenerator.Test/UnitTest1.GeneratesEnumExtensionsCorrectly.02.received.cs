//HintName: MapGenerator.cs
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        public void Map(somenamespace.Person source, out somenamespace.PersonDto destination)
        {
            Map(source.Address, out var address);
            destination = new somenamespace.PersonDto
            {
                FirstName = source.FirstName,
                Age = source.Age,
                Height = source.Height,
                Address = address,
                Zodiac = source.Traits.Zodiac,
                //LastName was manually excluded
            };
        }

        public bool TryMap(somenamespace.Person source, out somenamespace.PersonDto destination)
        {
            try
            {
                Map(source, out destination);
                return true;
            }
            catch
            {
                destination = null;
                return false;
            }
        }

        public void Map(somenamespace.Address source, out somenamespace.AddressDto destination)
        {
            destination = new somenamespace.AddressDto
            {
                Street = source.Street,
                City = source.City,
            };
        }

        public bool TryMap(somenamespace.Address source, out somenamespace.AddressDto destination)
        {
            try
            {
                Map(source, out destination);
                return true;
            }
            catch
            {
                destination = null;
                return false;
            }
        }

    }
}
