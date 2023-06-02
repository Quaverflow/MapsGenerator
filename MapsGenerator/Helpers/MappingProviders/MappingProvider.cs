using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;

namespace MapsGenerator.Helpers.MappingProviders;

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

        GetStandardMappings(context, sourceProperties, destinationProperties);

        GetMapFromMappings(context, destinationProperties, sourceProperties);

        foreach (var unmappedProperty in context.NotMappedProperties)
        {
            context.Mappings.UnmappedProperties.Add($"{unmappedProperty.Name} = /*MISSING MAPPING FOR TARGET PROPERTY.*/ ,");
        }
    }

    private static void GetMapFromMappings(SourceWriterContext context, IPropertySymbol[] destinationProperties,
        IPropertySymbol[] sourceProperties)
    {
        foreach (var customMap in context.CurrentMap.MapFromProperties)
        {
            if (customMap.IsExpressionMap)
            {
                GetExpressionMapping(context, destinationProperties, customMap);
            }
            else
            {
                var innerSourceProperty = SyntaxHelper.GetInnerProperty(context, sourceProperties, customMap.Source);
                var innerDestinationProperty = destinationProperties.First(x => x.Name == customMap.Destination);

                if (innerSourceProperty.IsComplexPropertySymbol())
                {
                    ComplexMappingProvider.InvokeExistingComplexPropertyMap(context,
                        new PropertyPair(innerSourceProperty, innerDestinationProperty), customMap.Source);
                }
                else if (innerSourceProperty.Type.IsCollectionSymbol())
                {
                    GetMapFromCollectionMapping(context, customMap, innerDestinationProperty, innerSourceProperty);
                }
                else
                {
                    context.Mappings.MapFrom.Add($"{customMap.Destination} = source.{customMap.Source},");
                }
            }

            if (context.NotMappedProperties.FirstOrDefault(x => x.Name == customMap.DestinationSimpleName) is { } notMapped)
            {
                context.NotMappedProperties.Remove(notMapped);
            }
        }
    }

    private static void GetExpressionMapping(SourceWriterContext context, IPropertySymbol[] destinationProperties,
        PropertyMapFromPair customMap)
    {
        var innerDestinationProperty = SyntaxHelper.GetInnerProperty(context, destinationProperties, customMap.Destination);
        var functionName = $"Map{customMap.Destination}FromCollection";
        var localFunction = @$"
            {innerDestinationProperty.Type} {functionName}({context.CurrentMap.SourceFullName} {customMap.LambdaIdentifier})
{customMap.Source}
";

        context.Mappings.LocalFunctions.Add(localFunction.Replace("Mapper.", string.Empty));
        context.Mappings.MapFrom.Add($"{customMap.Destination} = {functionName}(source),");
    }

    private static void GetMapFromCollectionMapping(SourceWriterContext context, PropertyMapFromPair customMap,
        IPropertySymbol innerDestinationProperty, IPropertySymbol innerSourceProperty)
    {
        var functionName = $"Map{customMap.Destination}FromCollection";
        var localFunction = CollectionMappingProvider.GetLocalFunctionForCollection(innerDestinationProperty,
            customMap, innerSourceProperty, functionName);
        context.Mappings.LocalFunctions.Add(localFunction);
        context.Mappings.MapFrom.Add($"{customMap.Destination} = {functionName}(source.{customMap.Source}),");
    }

    private static void GetStandardMappings(SourceWriterContext context, IPropertySymbol[] sourceProperties,
        IPropertySymbol[] destinationProperties)
    {
        EnumMappingProvider.AddEnums(sourceProperties, destinationProperties, context);
        SimpleMappingProvider.AddSimpleProperties(sourceProperties, destinationProperties, context);
        ComplexMappingProvider.AddComplexProperties(sourceProperties, destinationProperties, context);
        CollectionMappingProvider.AddCollectionProperties(sourceProperties, destinationProperties, context);
    }
}