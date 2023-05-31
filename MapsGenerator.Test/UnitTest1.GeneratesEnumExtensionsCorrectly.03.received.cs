//HintName: IMapGenerator.cs
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        void Map(somenamespace.Employee source,  out somenamespace.PersonDto destination);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(somenamespace.Employee source,  out somenamespace.PersonDto? destination, Action<Exception>? onError = null);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        void Map(somenamespace.Address source,  out somenamespace.AddressDto destination);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(somenamespace.Address source,  out somenamespace.AddressDto? destination, Action<Exception>? onError = null);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        void Map(somenamespace.Company source,  out somenamespace.CompanyDto destination);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(somenamespace.Company source,  out somenamespace.CompanyDto? destination, Action<Exception>? onError = null);
    }
}
