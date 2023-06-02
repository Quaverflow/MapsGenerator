using System.Text;
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

            context.NotMappedProperties.Add(complexProperty.DestinationProperty);
        }
    }

    public static void InvokeExistingComplexPropertyMap(SourceWriterContext context, PropertyPair complexProperty, string? sourceName = null)
    {
        var parametersBuilder = new StringBuilder();
        var complexPropertyName = complexProperty.DestinationProperty.Name;
        foreach (var mappedParameter in context.CurrentMap.MapFromParameterProperties.Where(x =>
                     x.NestedPropertyInvocationQueue.Peek() == complexPropertyName))
        {
            mappedParameter.NestedPropertyInvocationQueue.Dequeue();
            parametersBuilder.Append($"{mappedParameter.VariableName}, ");
        }

        var parameterBuilderString = parametersBuilder.ToString();
        var parameters = string.IsNullOrWhiteSpace(parameterBuilderString)
            ? null
            : $", {parameterBuilderString}";

        var invocation = $"Map<{complexProperty.DestinationProperty.Type}>(source.{sourceName ?? complexProperty.SourceProperty.Name}{parameters})";
        context.Mappings.MapFromParameter.Add($"{complexProperty.DestinationProperty.Name} = {invocation},");
    }

    private static bool ComplexPropertyMapExists(SourceWriterContext context, PropertyPair complexProperty)
    {
        return context.CurrentProfile.Maps.FirstOrDefault(x =>
            x.SourceFullName == complexProperty.SourceProperty.Type.ToString() &&
            x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString()) != null;
    }

}