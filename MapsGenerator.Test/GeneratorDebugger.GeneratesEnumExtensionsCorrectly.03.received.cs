//HintName: IMapGenerator.cs
using somenamespace;
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        somenamespace.PersonDto MapTosomenamespacePersonDto(somenamespace.Employee source);
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        bool TryMapTosomenamespacePersonDto(somenamespace.Employee source, out somenamespace.PersonDto? destination, Action<Exception>? onError = null);
    }
}
