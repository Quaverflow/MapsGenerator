using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;

namespace MapsGenerator.Helpers.MappingProviders;

public static class EnumMappingProvider
{
    public static void AddEnums(IPropertySymbol[] sourceProperties, IPropertySymbol[] destinationProperties, SourceWriterContext context)
    {
        var enumPropertiesMatchingByName = SyntaxHelper.GetEnumMatchingProperties(
            sourceProperties,
            destinationProperties,
            context);

        foreach (var enumProperty in enumPropertiesMatchingByName)
        {
            if (CommonMappingProvider.UseCommonMappings(context, enumProperty))
            {
                continue;
            }

            BuildEnumMapping(context, enumProperty);
        }
    }

    private static void BuildEnumMapping(SourceWriterContext context, PropertyPair enumProperty)
    {
        var sourceValues = GetValues(enumProperty.SourceProperty);
        var destinationValues = GetValues(enumProperty.DestinationProperty);

        var matchingEnumNames = new List<string>();
        var unmatchedEnumNames = new List<string>();
        foreach (var destinationValue in destinationValues)
        {
            if (IsEnumMapDefined(context, destinationValue, out var manuallyMapped))
            {
                var mappedSourceValue = sourceValues.First(x => x.Name == manuallyMapped?.Source);
                var mappedDestinationValue = destinationValues
                    .First(x => x.Name == manuallyMapped?.Destination);

                matchingEnumNames.Add($"{mappedSourceValue} => {mappedDestinationValue},");
            }
            else
            {
                PopulateMatchingAndUnmatchingValues(sourceValues, destinationValue, matchingEnumNames, unmatchedEnumNames);
            }
        }

        if (matchingEnumNames.Any())
        {
            CreateEnumMapSwitch(context, enumProperty, matchingEnumNames, unmatchedEnumNames);
        }
        else
        {
            context.CurrentNotMappedProperties.Add(enumProperty.DestinationProperty);
        }
    }

    private static IFieldSymbol[] GetValues(IPropertySymbol property) 
        => property.Type.GetMembers().OfType<IFieldSymbol>().ToArray();

    private static void CreateEnumMapSwitch(SourceWriterContext context, PropertyPair enumProperty, List<string> matchingEnumNames,
        List<string> unmatchedEnumNames)
    {
        var switchExpression = @$"(source.{enumProperty.SourceProperty.Name}) switch
                            {{
                                {CreateEnumMapRows(matchingEnumNames)}
                                {CreateEnumMapRows(unmatchedEnumNames)}
                                _ => throw new ArgumentOutOfRangeException(nameof(source.{enumProperty.SourceProperty.Name}), source.{enumProperty.SourceProperty.Name}, null)
                            }}";
        context.CurrentMappings.MatchingByName.Add($"{enumProperty.DestinationProperty.Name} = {switchExpression},");
        context.CurrentNotMappedProperties.Remove(enumProperty.DestinationProperty);
    }

    private static string CreateEnumMapRows(IReadOnlyCollection<string> names) 
        => names.Any() ? string.Join("\n", names) : string.Empty;

    private static void PopulateMatchingAndUnmatchingValues(IFieldSymbol[] sourceValues, IFieldSymbol destinationValue,
        ICollection<string> matchingEnumNames, ICollection<string> unmatchedEnumNames)
    {
        if (HasMatchingValue(sourceValues, destinationValue, out var matching))
        {
            matchingEnumNames.Add($"{matching} => {destinationValue},");
        }
        else
        {
            unmatchedEnumNames.Add($"/*THIS VALUE DOESN'T HAVE A MAPPING*/ => {destinationValue},");
        }
    }

    private static bool HasMatchingValue(IEnumerable<IFieldSymbol> sourceValues, ISymbol destinationValue, out IFieldSymbol? fieldSymbol)
    {
        if (sourceValues.FirstOrDefault(x => x.Name == destinationValue.Name) is { } matching)
        {
            fieldSymbol = matching;
            return true;
        }

        fieldSymbol = null;
        return false;
    }

    private static bool IsEnumMapDefined(SourceWriterContext context, ISymbol destinationValue, out EnumValueMap? enumValueMap)
    {
        var mapFromEnums = context.ProfileDefinitions
            .SelectMany(x => x.Maps
                .SelectMany(y => y.MapFromEnums));
        if (
            mapFromEnums.FirstOrDefault(x => x.Destination == destinationValue.Name) is { } manuallyMapped)
        {
            enumValueMap = manuallyMapped;
            return true;
        }

        enumValueMap = null;
        return false;
    }
}