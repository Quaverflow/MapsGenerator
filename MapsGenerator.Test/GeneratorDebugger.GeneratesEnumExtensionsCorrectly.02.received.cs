//HintName: MapGenerator.cs
using somenamespace;
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public somenamespace.PersonDto Map<T>(somenamespace.Employee source) where T : somenamespace.PersonDto
        {
            return new somenamespace.PersonDto
            {
                Seniority = (source.Seniority) switch
                            {
                                somenamespace.Seniority.Junior => somenamespace.SeniorityDto.Starter,
                                somenamespace.Seniority.Intermediate => somenamespace.SeniorityDto.Intermediate,
                                somenamespace.Seniority.Senior => somenamespace.SeniorityDto.Senior,

                                _ => throw new ArgumentOutOfRangeException(nameof(source.Seniority), source.Seniority, null)
                            },
                Id = source.Id,
                Role = source.Role,
                LastName = source.PersonalDetails.LastName,
                Address = Map<somenamespace.AddressDto>(source.PersonalDetails.Address),
                Height = source.PersonalDetails.Height,
                FirstName = "hello",
                Age = 3,
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.PersonProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Employee source, out somenamespace.PersonDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<somenamespace.PersonDto>(source);
                return true;
            }
            catch(Exception e)
            {
                destination = null;
                onError?.Invoke(e);
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
        public bool TryMap(somenamespace.Address source, out somenamespace.AddressDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<somenamespace.AddressDto>(source);
                return true;
            }
            catch(Exception e)
            {
                destination = null;
                onError?.Invoke(e);
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
                Address = Map<somenamespace.AddressDto>(source.Address),
                TradingName = source.Name,
                Sector = source.Name,
                OrderIds = source.OrderIds,
                CarpetsIdsCollected = source.CarpetsIdsCollected,
                AlternativeNames = source.AlternativeNames,
                Aliases = source.Aliases,
                PetsNames = source.PetsNames,
                Workers = MapWorkersFromCollection(source.Employees),
                Bees = MapBeesFromExpression(source),
            };
            
            somenamespace.PersonDto[] MapWorkersFromCollection(somenamespace.Employee[] sourceCollection)
            {
                var results = new somenamespace.PersonDto[sourceCollection.Count()];
                for (int i = 0; i < sourceCollection.Count(); i++)
                {
                    var item = sourceCollection[i];
                    var mappedItem = Map<somenamespace.PersonDto>(item);
                    results[i] = mappedItem;
                }
                return results;
            }
            
            Dictionary<System.Guid, somenamespace.PersonDto> MapBeesFromExpression(somenamespace.Company s)
=> s.Employees.ToDictionary(a => a.Id, a => Map<PersonDto>(a));

        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Company source, out somenamespace.CompanyDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<somenamespace.CompanyDto>(source);
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
