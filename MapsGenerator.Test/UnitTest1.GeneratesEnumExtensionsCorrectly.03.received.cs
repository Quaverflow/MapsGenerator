//HintName: IMapGenerator.cs
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        void Map(? addressCity,  out somenamespace.PersonDto)
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        bool Map(? addressCity, , out somenamespace.PersonDto, Action<Exception>? onError = null);
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        void Map( out somenamespace.AddressDto)
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        bool Map(, out somenamespace.AddressDto, Action<Exception>? onError = null);
    }
}
