using MapsGenerator.ExternalAssembly.TestData;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

internal class ExternalAddressProfile : MapperBase
{
    public ExternalAddressProfile()
    {
        Map<Address, ExternalAssemblyAddressDto>(x 
            => x.EnsureAllDestinationPropertiesAreMapped());
    }
}