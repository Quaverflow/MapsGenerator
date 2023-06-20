using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;

namespace MapsGenerator.Helpers.MappingProviders;

public static class ComplexMappingProvider
{
    public static void AddComplexProperties(
        IPropertySymbol[] sourceProperties,
        IEnumerable<IPropertySymbol> destinationProperties,
        SourceWriterContext context)
    {
        var complexPropertiesMatchingByName = SyntaxHelper.GetComplexMatchingProperties(
            sourceProperties,
            destinationProperties,
            context);

        foreach (var complexProperty in complexPropertiesMatchingByName)
        {
            if (CommonMappingProvider.UseCommonMappings(context, complexProperty))
            {
                continue;
            }

            if (ComplexPropertyMapExists(context, complexProperty))
            {
                InvokeExistingComplexPropertyMap(context, complexProperty);
                continue;
            }

            context.CurrentNotMappedProperties.Add(complexProperty.DestinationProperty);
        }
    }

    public static void InvokeExistingComplexPropertyMap(SourceWriterContext context, PropertyPair complexProperty, string? sourceName = null)
    {
        var invocation = $"MapTo{complexProperty.DestinationProperty.Type.ToString().Replace(".", string.Empty)}(source.{sourceName ?? complexProperty.SourceProperty.Name})";
        context.CurrentMappings.MapFrom.Add($"{complexProperty.DestinationProperty.Name} = {invocation},");
    }

    private static bool ComplexPropertyMapExists(SourceWriterContext context, PropertyPair complexProperty)
        => context.AllMappings.FirstOrDefault(x =>
            x.SourceFullName == complexProperty.SourceProperty.Type.ToString() &&
            x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString()) != null;
}