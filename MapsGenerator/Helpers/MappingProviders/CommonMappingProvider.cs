using MapsGenerator.DTOs;

namespace MapsGenerator.Helpers.MappingProviders;

public static class CommonMappingProvider
{
    public static bool UseCommonMappings(SourceWriterContext context, PropertyPair simpleProperty)
    {
        if (IsExcluded(context.CurrentMap, simpleProperty))
        {
            context.CurrentMappings.Excluded.Add($"//{simpleProperty.DestinationProperty.Name} was manually excluded");
            return true;
        }

        return IsDefinedAsMapFrom(context.CurrentMap, simpleProperty);
    }

    private static bool IsDefinedAsMapFrom(MappingInfo mappingInfo, PropertyPair complexProperty)
        => mappingInfo.MapFromProperties.FirstOrDefault(x =>
            x.Destination == complexProperty.DestinationProperty.Name) is not null;

    private static bool IsExcluded(MappingInfo mappingInfo, PropertyPair simpleProperty)
        => mappingInfo.ExcludedProperties.Any(x => x == simpleProperty.DestinationProperty.Name);
}