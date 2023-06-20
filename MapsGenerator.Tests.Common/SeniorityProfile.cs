using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

internal class SeniorityProfile : MapperBase
{
    public SeniorityProfile()
    {
        Map<Seniority, SeniorityDto>(x =>
        {
            x.MapFromEnum(SeniorityDto.Starter, Seniority.Junior);
        });
    }
}