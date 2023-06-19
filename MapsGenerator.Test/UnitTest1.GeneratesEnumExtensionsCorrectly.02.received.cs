//HintName: MapGenerator.cs
using ;
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public PersonDto Map<T>(Employee source) where T : PersonDto
        {
            return new PersonDto
            {
                Seniority = (source.Seniority) switch
                            {
                                Seniority.Intermediate => SeniorityDto.Intermediate,
                                Seniority.Senior => SeniorityDto.Senior,
                                /*THIS VALUE DOESN'T HAVE A MAPPING*/ => SeniorityDto.Starter,
                                _ => throw new ArgumentOutOfRangeException(nameof(source.Seniority), source.Seniority, null)
                            },
                Id = source.Id,
                Role = source.Role,
                LastName = source.PersonalDetails.LastName,
                Age = source.PersonalDetails.Age,
                Address = Map<AddressDto>(source.PersonalDetails.Address),
                Height = source.PersonalDetails.Height,
                FirstName = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(Employee source, out PersonDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<PersonDto>(source);
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
        public AddressDto Map<T>(Address source) where T : AddressDto
        {
            return new AddressDto
            {
                Street = source.Street,
                City = source.City,
            };
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(Address source, out AddressDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<AddressDto>(source);
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
        public CompanyDto Map<T>(Company source) where T : CompanyDto
        {
            return new CompanyDto
            {
                Address = Map<AddressDto>(source.Address),
                TradingName = source.Name,
                Workers = MapWorkersFromCollection(source.Employees),
                Bees = MapBeesFromExpression(source),
            };
            
            PersonDto[] MapWorkersFromCollection(Employee[] sourceCollection)
            {
                var results = new PersonDto[sourceCollection.Count()];
                for (int i = 0; i < sourceCollection.Count(); i++)
                {
                    var item = sourceCollection[i];
                    var mappedItem = Map<PersonDto>(item);
                    results[i] = mappedItem;
                }
                return results;
            }
            
            Dictionary<System.Guid, PersonDto> MapBeesFromExpression(Company s)
=> s =>
                
                    s.Employees.ToDictionary(a => a.Id, a => Map(a, out _));

        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(Company source, out CompanyDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                destination = Map<CompanyDto>(source);
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
