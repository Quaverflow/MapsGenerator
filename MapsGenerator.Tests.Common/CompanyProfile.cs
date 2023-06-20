using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

internal class CompanyProfile : MapperBase
{

    public CompanyProfile()
    {
        Map<Company, CompanyDto>(x =>
        {
            //x.MapFrom(d => d.Address, s => s.Address);
            x.MapFrom(d => d.TradingName, s => s.Name);
            x.MapFrom(d => d.Sector, s => s.Name);
            x.MapFrom(d => d.OrderIds, s => s.OrderIds);
            x.MapFrom(d => d.CarpetsIdsCollected, s => s.CarpetsIdsCollected);
            x.MapFrom(d => d.AlternativeNames, s => s.AlternativeNames);
            x.MapFrom(d => d.Aliases, s => s.Aliases);
            x.MapFrom(d => d.PetsNames, s => s.PetsNames);
            x.MapFrom(d => d.Workers, s => s.Employees);
            x.MapFrom(d => d.Bees, s => s.Employees.ToDictionary(a => a.Id, a
                => Mapper.MapToMapsGeneratorTestsCommonModelsDestinationPersonDto(a)));

            x.EnsureAllDestinationPropertiesAreMapped();
        });

    }
}