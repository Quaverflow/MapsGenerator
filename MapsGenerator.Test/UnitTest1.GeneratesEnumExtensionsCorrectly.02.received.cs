//HintName: MapGenerator.cs
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public void Map(? addressCity,  out somenamespace.PersonDto destination)
        {
            destination = new somenamespace.PersonDto
            {
                FirstName = source.FirstName,
                Age = source.Age,
                Height = source.Height,
                Zodiac = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
                Address = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
                //LastName was manually excluded
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public bool Map(? addressCity, , out somenamespace.PersonDto destination, Action<Exception>? onError = null)
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
    }
}
