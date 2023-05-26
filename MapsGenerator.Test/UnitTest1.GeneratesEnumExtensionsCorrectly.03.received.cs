//HintName: IMapGenerator.cs
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        void Map(? addressCity,  out somenamespace.PersonDto destination)
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        bool Map(? addressCity, , out somenamespace.PersonDto destination, Action<Exception>? onError = null);
    }
}
