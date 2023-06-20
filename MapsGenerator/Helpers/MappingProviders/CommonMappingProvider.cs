using MapsGenerator.DTOs;

namespace MapsGenerator.Helpers.MappingProviders;

public static class CommonMappingProvider
{
    public static bool UseCommonMappings(SourceWriterContext context, PropertyPair property) 
        => TryExclude(context.CurrentMap, property, context) || IsDefinedAsMapFrom(context.CurrentMap, property);

    private static bool IsDefinedAsMapFrom(MapInfo mappingInfo, PropertyPair property)
        => mappingInfo.MapFromProperties.FirstOrDefault(x =>
            x.Destination == property.DestinationProperty.Name) is not null;

    public static void AddRemainingExcludedMaps(SourceWriterContext context)
    {
        var propertiesToExclude = context.CurrentNotMappedProperties
            .Where(y => context.CurrentMap.ExcludedProperties
                .Any(x => x == y.Name))
            .ToArray();

        foreach (var property in propertiesToExclude)
        {
            context.CurrentMappings.Excluded.Add($"//{property.Name} was manually excluded");
            context.CurrentNotMappedProperties.Remove(property);
        }
    }

    private static bool TryExclude(MapInfo mappingInfo, PropertyPair property, SourceWriterContext context)
    {
        if (mappingInfo.ExcludedProperties.All(x => x != property.DestinationProperty.Name))
        {
            return false;
        }

        context.CurrentMappings.Excluded.Add($"//{property.DestinationProperty.Name} was manually excluded");
        return true;
    }
}