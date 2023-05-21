//HintName: IMapGenerator.cs
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        void Map(somenamespace.Person source, out somenamespace.PersonDto destination);
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        bool TryMap(somenamespace.Person source, out somenamespace.PersonDto destination);
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        void Map(somenamespace.Address source, out somenamespace.AddressDto destination);
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        bool TryMap(somenamespace.Address source, out somenamespace.AddressDto destination);
    }
}
