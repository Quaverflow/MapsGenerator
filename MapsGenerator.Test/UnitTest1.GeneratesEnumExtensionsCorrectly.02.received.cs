//HintName: MapGenerator.cs
namespace MapsGenerator
{
    public class MapGenerator : IMapGenerator
    {
        
/// <summary>
/// Profile <see cref="somenamespace.GeneratorProfile"/>
/// </summary>
        public void Map(somenamespace.Employee source,  out somenamespace.PersonDto destination)
        {
            destination = new somenamespace.PersonDto
            {
                Seniority = (source.Seniority) switch
                            {
                                somenamespace.Seniority.Intermediate => somenamespace.SeniorityDto.Intermediate,
                                somenamespace.Seniority.Senior => somenamespace.SeniorityDto.Senior,
                                /*THIS VALUE DOESN'T HAVE A MAPPING*/ => somenamespace.SeniorityDto.Starter,
                                _ => throw new ArgumentOutOfRangeException(nameof(va), va, null)
                            },
                Id = source.Id,
                Role = source.Role,
                FirstName = source.PersonalDetails.FirstName,
                LastName = source.PersonalDetails.LastName,
                Age = source.PersonalDetails.Age,
                Address = source.PersonalDetails.Address,
                Height = source.PersonalDetails.Height,
            };
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
        public void Map(somenamespace.Address source,  out somenamespace.AddressDto destination)
        {
            destination = new somenamespace.AddressDto
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
        public void Map(somenamespace.Company source,  out somenamespace.CompanyDto destination)
        {
            Map(source.Address, out var address);
            destination = new somenamespace.CompanyDto
            {
                Address = address,
                TradingName = source.Name,
                Workers = source.Employees,
                Sector = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,
            };
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
