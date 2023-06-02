//HintName: MapGenerator.cs
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
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public somenamespace.PersonDto Map<T>(somenamespace.Employee source) where T : somenamespace.PersonDto
        {
            return new somenamespace.PersonDto
            {
                Seniority = (source.Seniority) switch
                            {
                                somenamespace.Seniority.Intermediate => somenamespace.SeniorityDto.Intermediate,
                                somenamespace.Seniority.Senior => somenamespace.SeniorityDto.Senior,
                                /*THIS VALUE DOESN'T HAVE A MAPPING*/ => somenamespace.SeniorityDto.Starter,
                                _ => throw new ArgumentOutOfRangeException(nameof(source.Seniority), source.Seniority, null)
                            },
                Id = source.Id,
                Role = source.Role,
                FirstName = source.PersonalDetails.FirstName,
                LastName = source.PersonalDetails.LastName,
                Age = source.PersonalDetails.Age,
                Height = source.PersonalDetails.Height,
                Address = Map<somenamespace.AddressDto>(source.PersonalDetails.Address),
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Employee source,  out somenamespace.PersonDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<somenamespace.PersonDto>(source );
                return true;
            }
            catch(Exception e)
            {
                destination = null;
                if(onError != null) { onError(e); }
                return false;
            }
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public somenamespace.AddressDto Map<T>(somenamespace.Address source) where T : somenamespace.AddressDto
        {
            return new somenamespace.AddressDto
            {
                Street = source.Street,
                City = source.City,
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Address source,  out somenamespace.AddressDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<somenamespace.AddressDto>(source );
                return true;
            }
            catch(Exception e)
            {
                destination = null;
                if(onError != null) { onError(e); }
                return false;
            }
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public somenamespace.CompanyDto Map<T>(somenamespace.Company source) where T : somenamespace.CompanyDto
        {
            return new somenamespace.CompanyDto
            {
                TradingName = source.Name,
                Workers = MapWorkersFromCollection(source.Employees),
                Bees = MapBeesFromCollection(source),
                Address = Map<somenamespace.AddressDto>(source.Address),
                Sector = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
            };
            
            somenamespace.PersonDto[] MapWorkersFromCollection(somenamespace.Employee[] sourceCollection)
            {
                var results = new List<somenamespace.PersonDto>();
                foreach(var item in sourceCollection)
                {
                    var mappedItem = Map<somenamespace.PersonDto>(item);
                    results.Add(mappedItem);
                }

                return results.ToArray();
            }
            
            Dictionary<System.Guid, somenamespace.PersonDto> MapBeesFromCollection(somenamespace.Company s)
{ 
                    return s.Employees.ToDictionary(a => a.Id, a => Map(a, out _));
                }

        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Company source,  out somenamespace.CompanyDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<somenamespace.CompanyDto>(source );
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
