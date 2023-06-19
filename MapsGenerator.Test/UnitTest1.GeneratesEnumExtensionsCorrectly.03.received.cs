//HintName: IMapGenerator.cs
using ;
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        PersonDto Map<T>(Employee source) where T : PersonDto;
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(Employee source, out PersonDto? destination, Action<Exception>? onError = null);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        AddressDto Map<T>(Address source) where T : AddressDto;
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(Address source, out AddressDto? destination, Action<Exception>? onError = null);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        CompanyDto Map<T>(Company source) where T : CompanyDto;
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(Company source, out CompanyDto? destination, Action<Exception>? onError = null);
    }
}
