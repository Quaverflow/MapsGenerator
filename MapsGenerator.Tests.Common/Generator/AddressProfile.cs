using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

internal class AddressProfile : MapperBase
{
    public AddressProfile()
    {
        Map<Address, AddressDto>(x => x.EnsureAllDestinationPropertiesAreMapped());
    }
}