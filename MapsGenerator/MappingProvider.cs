using Microsoft.CodeAnalysis;

namespace MapsGenerator;

public static class MappingProvider
{
    public static Mappings GetMappings(MappingInfo mappingInfo, MappingInfo[] maps, Compilation compilation)
    {
        var mappings = new Mappings();
        var sourceProperties = SyntaxHelper.GetProperties(mappingInfo.Source, compilation).ToArray();
        var destinationProperties = SyntaxHelper.GetProperties(mappingInfo.Destination, compilation).ToArray();

        AddSimpleProperties(mappingInfo, sourceProperties, destinationProperties, mappings);
        AddComplexProperties(mappingInfo, maps, sourceProperties, destinationProperties, mappings);

        foreach (var customMap in mappingInfo.MapFromProperties)
        {
            mappings.MapFrom.Add($"{customMap.Destination} = source.{customMap.Source},");
        }

        return mappings;
    }

    private static void AddComplexProperties(
        MappingInfo mappingInfo, 
        MappingInfo[] maps,
        IEnumerable<IPropertySymbol> sourceProperties, 
        IEnumerable<IPropertySymbol> destinationProperties, 
        Mappings mappings)
    {
        var complexPropertiesMatchingByName = SyntaxHelper.GetComplexMatchingProperties(
            sourceProperties,
            destinationProperties);

        foreach (var complexProperty in complexPropertiesMatchingByName)
        {
            if (UseCommonMappings(mappingInfo, mappings, complexProperty))
            {
                continue;
            }

            if (maps.FirstOrDefault(x =>
                    x.SourceFullName == complexProperty.SourceProperty.Type.ToString() &&
                    x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString()) != null)
            {
                var variable = complexProperty.DestinationProperty.Name.FirstCharToLower();
                var invocation = $"Map(source.{complexProperty.SourceProperty.Name}, out var {variable});";

                //todo add a check for duplication
                mappings.ComplexMappingInfo.Add(new ComplexMappingInfo(invocation, variable,
                    complexProperty.DestinationProperty.Name));
                continue;
            }

            mappings.MatchingByName.Add($"//{complexProperty.DestinationProperty.Name} = source.{complexProperty.SourceProperty.Name} these property have matching name but no map has been defined");
        }
    }

    private static void AddSimpleProperties(MappingInfo mappingInfo, IEnumerable<IPropertySymbol> sourceProperties,
        IPropertySymbol[] destinationProperties, Mappings mappings)
    {
        var simplePropertiesMatchingByName = SyntaxHelper.GetSimpleMatchingProperties(
            sourceProperties,
            destinationProperties);

        foreach (var simpleProperty in simplePropertiesMatchingByName)
        {
            if (UseCommonMappings(mappingInfo, mappings, simpleProperty))
            {
                continue;
            }

            mappings.MatchingByName.Add($"{simpleProperty.DestinationProperty.Name} = source.{simpleProperty.SourceProperty.Name},");
        }
    }

    private static bool UseCommonMappings(MappingInfo mappingInfo, Mappings mappings, PropertyPair simpleProperty)
    {
        if (IsExcluded(mappingInfo, simpleProperty))
        {
            mappings.Excluded.Add($"//{simpleProperty.DestinationProperty.Name} was manually excluded");
            return true;
        }

        return IsDefinedAsMapFrom(mappingInfo, simpleProperty);
    }

    private static bool IsDefinedAsMapFrom(MappingInfo mappingInfo, PropertyPair complexProperty)
        => mappingInfo.MapFromProperties.FirstOrDefault(x =>
            x.Destination == complexProperty.DestinationProperty.Name) is not null;

    private static bool IsExcluded(MappingInfo mappingInfo, PropertyPair simpleProperty) 
        => mappingInfo.ExcludedProperties.Any(x => x == simpleProperty.DestinationProperty.Name);
}