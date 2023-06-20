using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

internal class PersonProfile : MapperBase
{
    public PersonProfile()
    {
        Map<Employee, PersonDto>(x =>
        {
            x.MapFromConstantValue(d => d.FirstName, "hello");
            x.MapFromConstantValue(d => d.Age, 3);
            x.MapFrom(d => d.Address, s => s.PersonalDetails.Address);
            x.MapFrom(d => d.ExternalAddress, s => s.PersonalDetails.ExternalAddress);
            x.MapFrom(d => d.Height, s => s.PersonalDetails.Height);
            x.Exclude(d => d.LastName);

            x.EnsureAllDestinationPropertiesAreMapped();
        });
    }
}