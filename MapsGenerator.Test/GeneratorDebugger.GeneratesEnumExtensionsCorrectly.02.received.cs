//HintName: MapGenerator.cs
using somenamespace;
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public somenamespace.PersonDto MapTosomenamespacePersonDto(somenamespace.Employee source)
        {
            return new somenamespace.PersonDto
            {
                //LastName was manually excluded
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public bool TryMapTosomenamespacePersonDto(somenamespace.Employee source, out somenamespace.PersonDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = MapTosomenamespacePersonDto(source);
                return true;
            }
            catch(Exception e)
            {
                destination = null;
                onError?.Invoke(e);
                return false;
            }
        }
    }
}
