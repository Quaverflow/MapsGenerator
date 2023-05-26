//HintName: MapGenerator.cs
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public void Map(? addressCity,  out somenamespace.PersonDto)
        {
            Map(source.Address, addressCity, out var address);
            destination = new somenamespace.PersonDto
            {
                FirstName = source.FirstName,
                Age = source.Age,
                Height = source.Height,
                Address = address,
                Zodiac = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
                //LastName was manually excluded
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public bool Map(? addressCity, , out somenamespace.PersonDto, Action<Exception>? onError = null)
        {
            try
            {
                Map(source, addressCity, out destination);
                return true;
            }
            catch(Exception e)
            {
                destination = null;
                if(onError != null) { onError(e); }
                return false;
            }
        }
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public void Map( out somenamespace.AddressDto)
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
        public bool Map(, out somenamespace.AddressDto, Action<Exception>? onError = null)
        {
            try
            {
                Map(source, out destination);
                return true;
            }
            catch(Exception e)
            {
                destination = null;
                if(onError != null) { onError(e); }
                return false;
            }
        }
    }
}
