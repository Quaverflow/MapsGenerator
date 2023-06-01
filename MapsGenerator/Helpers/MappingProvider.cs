using System.Reflection.PortableExecutable;
using System.Text;
using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.Helpers;

public static class MappingProvider
{
    public static void GetAllTypeProperties(SourceWriterContext context)
    {
        foreach (var map in context.ProfileDefinitions.SelectMany(x => x.Maps))
        {
            var semanticModel = context.Compilation.GetSemanticModel(map.Source.SyntaxTree);
            var sourceType = semanticModel.GetTypeInfo(map.Source).Type;
            var destinationType = semanticModel.GetTypeInfo(map.Destination).Type;

            var sourceProperties = SyntaxHelper.GetProperties(map.Source, semanticModel).ToArray();
            var destinationProperties = SyntaxHelper.GetProperties(map.Destination, semanticModel).ToArray();

            if (sourceType != null && !context.TypesProperties.ContainsKey(sourceType.ToString()))
            {
                context.TypesProperties.Add(sourceType.ToString(), new TypeProperties(sourceProperties, sourceType));
            }
            if (destinationType != null && !context.TypesProperties.ContainsKey(destinationType.ToString()))
            {
                context.TypesProperties.Add(destinationType.ToString(), new TypeProperties(destinationProperties, destinationType));
            }
        }
    }

    public static void GetMappings(SourceWriterContext context)
    {
        var sourceProperties = context.TypesProperties[context.CurrentMap.SourceFullName].Properties;
        var destinationProperties = context.TypesProperties[context.CurrentMap.DestinationFullName].Properties;
        AddEnums(sourceProperties, destinationProperties, context);
        AddSimpleProperties(sourceProperties, destinationProperties, context);
        AddComplexProperties(sourceProperties, destinationProperties, context);

        foreach (var customMap in context.CurrentMap.MapFromProperties)
        {
            var innerSourceProperty = GetInnerProperty(context, sourceProperties, customMap.Source);

            if (innerSourceProperty.IsComplexPropertySymbol())
            {
                var innerDestinationProperty = GetInnerProperty(context, destinationProperties, customMap.Destination);
                InvokeExistingComplexPropertyMap(context, new PropertyPair(innerSourceProperty, innerDestinationProperty), customMap.Source);
            }
            else
            {
                context.Mappings.MapFrom.Add($"{customMap.Destination} = source.{customMap.Source},");
            }
           
            if (context.NotMappedProperties.FirstOrDefault(x => x.Name == customMap.DestinationSimpleName) is { } notMapped)
            {
                context.NotMappedProperties.Remove(notMapped);
            }
        }

        foreach (var unmappedProperty in context.NotMappedProperties)
        {
            context.Mappings.UnmappedProperties.Add($"{unmappedProperty.Name} = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,");
        }
    }

    private static IPropertySymbol GetInnerProperty(SourceWriterContext context, IPropertySymbol[] currentType, string nestedProperty)
    {
        var propertyNesting = nestedProperty.Split('.').ToArray();
        var result = currentType.First(x => x.Name == propertyNesting[0]);

        foreach (var property in propertyNesting.Skip(1))
        {
            if (context.TypesProperties.TryGetValue(result.Type.Name, out var value))
            {
                currentType = value.Properties;
            }
            else
            {
                var properties = result.Type.GetMembers().OfType<IPropertySymbol>().ToArray();
                context.TypesProperties.Add(result.Type.Name,new TypeProperties(properties, result.Type));
                currentType = properties;
            }

            result = currentType.First(x => x.Name == property);
        }

        return result;
    }

    private static void AddEnums(IPropertySymbol[] sourceProperties, IPropertySymbol[] destinationProperties, SourceWriterContext context)
    {
        var enumPropertiesMatchingByName = SyntaxHelper.GetEnumMatchingProperties(
            sourceProperties,
            destinationProperties,
            context);

        foreach (var enumProperty in enumPropertiesMatchingByName)
        {
            if (UseCommonMappings(context, enumProperty))
            {
                continue;
            }

            var sourceValues = enumProperty.SourceProperty.Type.GetMembers().OfType<IFieldSymbol>().ToArray();
            var destinationValues = enumProperty.DestinationProperty.Type.GetMembers().OfType<IFieldSymbol>().ToArray();

            var matchingEnumNames = new List<string>();
            var unmatchedEnumNames = new List<string>();
            foreach (var destinationValue in destinationValues)
            {
                if (context.ProfileDefinitions.SelectMany(x => x.Maps.SelectMany(y => y.MapFromEnums)).FirstOrDefault(x => x.Destination == destinationValue.Name) is { } manuallyMapped)
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
                    else if (context.CurrentMap.EnsureAllDestinationPropertiesAreMapped)
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
                context.Mappings.MatchingByName.Add($"{enumProperty.DestinationProperty.Name} = {switchExpression},");
                context.NotMappedProperties.Remove(enumProperty.DestinationProperty);
            }
            else
            {
                context.NotMappedProperties.Add(enumProperty.DestinationProperty);
            }
        }
    }

    private static void AddComplexProperties(
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
            if (UseCommonMappings(context, complexProperty))
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

    private static void InvokeExistingComplexPropertyMap(SourceWriterContext context, PropertyPair complexProperty, string? sourceName = null)
    {
        var parametersBuilder = new StringBuilder();
        var complexPropertyName = complexProperty.DestinationProperty.Name;
        foreach (var mappedParameter in context.CurrentMap.MapFromParameterProperties.Where(x =>
                     x.NestedPropertyInvocationQueue.Peek() == complexPropertyName))
        {
            mappedParameter.NestedPropertyInvocationQueue.Dequeue();
            parametersBuilder.Append($"{mappedParameter.VariableName}, ");
        }

        var variable = complexPropertyName.FirstCharToLower();
        var invocation = $"Map(source.{sourceName ?? complexProperty.SourceProperty.Name}, {parametersBuilder}out var {variable});";

        //todo add a check for duplication
        context.Mappings.ComplexMappingInfo.Add(new ComplexMappingInfo(invocation, variable,
            complexProperty.DestinationProperty.Name));
    }

    private static bool ComplexPropertyMapExists(SourceWriterContext context, PropertyPair complexProperty)
    {
        return context.CurrentProfile.Maps.FirstOrDefault(x =>
            x.SourceFullName == complexProperty.SourceProperty.Type.ToString() &&
            x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString()) != null;
    }

    private static void AddSimpleProperties(IPropertySymbol[] sourceProperties,
        IPropertySymbol[] destinationProperties, SourceWriterContext context)
    {
        var simplePropertiesMatchingByName = SyntaxHelper.GetSimpleMatchingProperties(
            sourceProperties,
            destinationProperties,
            context);

        foreach (var simpleProperty in simplePropertiesMatchingByName)
        {
            if (UseCommonMappings(context, simpleProperty))
            {
                continue;
            }

            context.Mappings.MatchingByName.Add($"{simpleProperty.DestinationProperty.Name} = source.{simpleProperty.SourceProperty.Name},");
        }
    }

    private static bool UseCommonMappings(SourceWriterContext context, PropertyPair simpleProperty)
    {
        if (IsExcluded(context.CurrentMap, simpleProperty))
        {
            context.Mappings.Excluded.Add($"//{simpleProperty.DestinationProperty.Name} was manually excluded");
            return true;
        }

        if (context.CurrentMap.MapFromParameterProperties.FirstOrDefault(
                  x => x.Name == simpleProperty.DestinationProperty.Name) is { } property)
        {
            context.Mappings.MapFromParameter.Add($"{property.Name} = {property.VariableName},");
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