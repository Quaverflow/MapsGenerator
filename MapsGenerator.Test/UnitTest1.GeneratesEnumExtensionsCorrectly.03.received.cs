//HintName: IMapGenerator.cs
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        void Map(somenamespace.Person person, ? addressCity,  out somenamespace.PersonDto destination)
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        bool Map(somenamespace.Person person, ? addressCity, , out somenamespace.PersonDto destination, Action<Exception>? onError = null);
    }
}
