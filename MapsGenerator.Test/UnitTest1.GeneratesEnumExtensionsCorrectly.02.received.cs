//HintName: MapGenerator.cs
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public somenamespace.PersonDto Map(somenamespace.Employee source,  out somenamespace.PersonDto destination)
        {
            Map(source.PersonalDetails.Address, out var address);
            destination = new somenamespace.PersonDto
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
                Address = address,
                FirstName = source.PersonalDetails.FirstName,
                LastName = source.PersonalDetails.LastName,
                Age = source.PersonalDetails.Age,
                Height = source.PersonalDetails.Height,
            };
            return destination;
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Employee source,  out somenamespace.PersonDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                Map(source, out destination);
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
        public somenamespace.AddressDto Map(somenamespace.Address source,  out somenamespace.AddressDto destination)
        {
            destination = new somenamespace.AddressDto
            {
                Street = source.Street,
                City = source.City,
            };
            return destination;
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Address source,  out somenamespace.AddressDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                Map(source, out destination);
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
        public somenamespace.CompanyDto Map(somenamespace.Company source,  out somenamespace.CompanyDto destination)
        {
            Map(source.Address, out var address);
            destination = new somenamespace.CompanyDto
            {
                Address = address,
                TradingName = source.Name,
                Workers = MapWorkersFromCollection(source.Employees),
                Bees = MapBeesFromCollection(source),
                Sector = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
                Bees = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
            };
            
            somenamespace.PersonDto[] MapWorkersFromCollection(somenamespace.Employee[] sourceCollection)
            {
                var results = new List<somenamespace.PersonDto>();
                foreach(var item in sourceCollection)
                {
                    var mappedItem = Map(item, out var _);
                    results.Add(mappedItem);
                }

                return results.ToArray();
            }
            
            Dictionary<System.Guid, somenamespace.PersonDto> MapBeesFromCollection(somenamespace.Company s)
{ 
                    return s.Employees.ToDictionary(a => a.Id, a => Map(a, out _));
                }

            return destination;
        }
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public bool TryMap(somenamespace.Company source,  out somenamespace.CompanyDto? destination, Action<Exception>? onError = null)
        {
            try
            {
                Map(source, out destination);
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
