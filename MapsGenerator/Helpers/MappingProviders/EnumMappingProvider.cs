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

            var sourceValues = enumProperty.SourceProperty.Type.GetMembers().OfType<IFieldSymbol>().ToArray();
            var destinationValues = enumProperty.DestinationProperty.Type.GetMembers().OfType<IFieldSymbol>().ToArray();

            var matchingEnumNames = new List<string>();
            var unmatchedEnumNames = new List<string>();
            foreach (var destinationValue in destinationValues)
            {
                if (context.ProfileDefinitions.SelectMany(x => x.Maps.SelectMany(y => y.MapFromEnums))
                    .FirstOrDefault(x => x.Destination == destinationValue.Name) is { } manuallyMapped)
                {
                    var mappedSourceValue = sourceValues.First(x => x.Name == manuallyMapped.Source);
                    var mappedDestinationValue = destinationValues.First(x => x.Name == manuallyMapped.Destination);
                    matchingEnumNames.Add($"                                {mappedSourceValue} => {mappedDestinationValue},");
                }
                else
                {
                    if (sourceValues.FirstOrDefault(x => x.Name == destinationValue.Name) is { } matching)
                    {
                        matchingEnumNames.Add($"                                {matching} => {destinationValue},");
                    }
                    else
                    {
                        unmatchedEnumNames.Add($"                                /*THIS VALUE DOESN'T HAVE A MAPPING*/ => {destinationValue},");
                    }
                }
            }

            if (matchingEnumNames.Any())
            {
                var switchExpression = @$"(source.{enumProperty.SourceProperty.Name}) switch
                            {{
{string.Join("\n", matchingEnumNames)}
{string.Join("\n", unmatchedEnumNames)}
                                _ => throw new ArgumentOutOfRangeException(nameof(source.{enumProperty.SourceProperty.Name}), source.{enumProperty.SourceProperty.Name}, null)
                            }}";
                context.CurrentMappings.MatchingByName.Add($"{enumProperty.DestinationProperty.Name} = {switchExpression},");
                context.CurrentNotMappedProperties.Remove(enumProperty.DestinationProperty);
            }
            else
            {
                context.CurrentNotMappedProperties.Add(enumProperty.DestinationProperty);
            }
        }
    }

}