using MapsGenerator.Tests.Common.Models;
using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

internal class PersonGeneratorProfile : MapperBase
{
    public PersonGeneratorProfile()
    {
        //Map<Person, PersonDto>(x =>
        //{ 
        //    x.Exclude(y => y.LastName);
        //    x.MapFrom(d => d.Zodiac, s => s.Traits.Zodiac);
        //});
        //Map<Address, AddressDto>();
    }
}