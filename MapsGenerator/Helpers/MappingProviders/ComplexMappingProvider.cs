using System.Text;
using MapsGenerator.DTOs;
using MapsGenerator.Helpers.Extensions;
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
        var parametersList = new List<string>();
        var complexPropertyName = complexProperty.DestinationProperty.Name;
        var maps = context.ProfileDefinitions.SelectMany(x => x.Maps).ToArray();
        var existingMap = maps.First(x => x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString());
        var parameters = existingMap.MapFromParameterProperties.ToDictionary(
            x => $"{complexPropertyName.FirstCharToLower()}_{x.Name.FirstCharToUpper()}", x => $"{x.Type}");

        if (!context.CurrentParametersRequiredFromProperties.ContainsKey(complexPropertyName))
        {
            context.CurrentParametersRequiredFromProperties.Add(complexPropertyName, new(complexProperty.DestinationProperty, parameters));
        }

        var parameterBuilderString = parameters.Select(x => x.Key).ToString();

        var invocation = string.Empty;
        if (string.IsNullOrWhiteSpace(parameterBuilderString))
        {
            invocation = $"Map<{complexProperty.DestinationProperty.Type}>(source.{sourceName ?? complexProperty.SourceProperty.Name})";
            context.CurrentMappings.MapFromParameter.Add($"{complexProperty.DestinationProperty.Name} = {invocation},");
        }
        else
        {


            invocation = $"Map<{complexProperty.DestinationProperty.Type}>(source.{sourceName ?? complexProperty.SourceProperty.Name}, {parameterBuilderString})";
            context.CurrentMappings.MapFromParameter.Add($"{complexProperty.DestinationProperty.Name} = {invocation},");
        }
    }

    private static bool ComplexPropertyMapExists(SourceWriterContext context, PropertyPair complexProperty)
    {
        return context.CurrentProfile.Maps.FirstOrDefault(x =>
            x.SourceFullName == complexProperty.SourceProperty.Type.ToString() &&
            x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString()) != null;
    }

}