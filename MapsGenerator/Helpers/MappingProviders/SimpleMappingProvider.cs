using Microsoft.CodeAnalysis;

namespace MapsGenerator.Helpers.MappingProviders;

public static class SimpleMappingProvider
{
    public static void AddSimpleProperties(IPropertySymbol[] sourceProperties,
        IPropertySymbol[] destinationProperties, SourceWriterContext context)
    {
        var simplePropertiesMatchingByName = SyntaxHelper.GetSimpleMatchingProperties(
            sourceProperties,
            destinationProperties,
            context);

        foreach (var simpleProperty in simplePropertiesMatchingByName)
        {
            if (CommonMappingProvider.UseCommonMappings(context, simpleProperty))
            {
                continue;
            }

            context.Mappings.MatchingByName.Add($"{simpleProperty.DestinationProperty.Name} = source.{simpleProperty.SourceProperty.Name},");
        }
    }
}