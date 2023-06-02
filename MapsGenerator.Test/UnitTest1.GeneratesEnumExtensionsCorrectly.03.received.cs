//HintName: IMapGenerator.cs
using somenamespace;
using somenamespace;
using somenamespace;
using somenamespace;
using somenamespace;
using somenamespace;
using somenamespace;
using somenamespace;
using somenamespace;
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        somenamespace.PersonDto Map<T>(somenamespace.Employee source) where T : somenamespace.PersonDto;
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(somenamespace.Employee source,  out somenamespace.PersonDto? destination, Action<Exception>? onError = null);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        somenamespace.AddressDto Map<T>(somenamespace.Address source) where T : somenamespace.AddressDto;
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(somenamespace.Address source,  out somenamespace.AddressDto? destination, Action<Exception>? onError = null);
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        somenamespace.CompanyDto Map<T>(somenamespace.Company source) where T : somenamespace.CompanyDto;
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        bool TryMap(somenamespace.Company source,  out somenamespace.CompanyDto? destination, Action<Exception>? onError = null);
    }
}
