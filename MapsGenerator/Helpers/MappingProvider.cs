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
        AddSimpleProperties(sourceProperties, destinationProperties, context);
        AddComplexProperties(sourceProperties, destinationProperties, context);

        foreach (var customMap in context.CurrentMap.MapFromProperties)
        {
            context.Mappings.MapFrom.Add($"{customMap.Destination} = source.{customMap.Source},");
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

    private static void InvokeExistingComplexPropertyMap(SourceWriterContext context, PropertyPair complexProperty)
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
        var invocation = $"Map(source.{complexProperty.SourceProperty.Name}, {parametersBuilder}out var {variable});";

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