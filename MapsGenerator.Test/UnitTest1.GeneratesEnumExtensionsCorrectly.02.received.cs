//HintName: MapGenerator.cs
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public void Map(somenamespace.Person source, out somenamespace.PersonDto destination)
        {
            Map(source.Address, out var address);
            destination = new somenamespace.PersonDto
            {
                FirstName = source.FirstName,
                Age = source.Age,
                Height = source.Height,
                Address = address,
                //FirstName - ?
                //LastName was manually excluded
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
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
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public void Map(somenamespace.Address source, out somenamespace.AddressDto destination)
        {
            destination = new somenamespace.AddressDto
            {
                Street = source.Street,
                City = source.City,
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
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
