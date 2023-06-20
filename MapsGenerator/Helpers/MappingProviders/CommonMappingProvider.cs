using MapsGenerator.DTOs;

namespace MapsGenerator.Helpers.MappingProviders;

public static class CommonMappingProvider
{
    public static bool UseCommonMappings(SourceWriterContext context, PropertyPair property)
    {
        if (IsExcluded(context.CurrentMap, property))
        {
            context.CurrentMappings.Excluded.Add($"//{property.DestinationProperty.Name} was manually excluded");
            return true;
        }

        return IsDefinedAsMapFrom(context.CurrentMap, property);
    }

    private static bool IsDefinedAsMapFrom(MapInfo mappingInfo, PropertyPair property)
        => mappingInfo.MapFromProperties.FirstOrDefault(x =>
            x.Destination == property.DestinationProperty.Name) is not null;

    private static bool IsExcluded(MapInfo mappingInfo, PropertyPair simpleProperty)
        => mappingInfo.ExcludedProperties.Any(x => x == simpleProperty.DestinationProperty.Name);
}