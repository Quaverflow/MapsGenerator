using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;

namespace MapsGenerator.Helpers.MappingProviders;

public static class CollectionMappingProvider
{
    private static readonly IReadOnlyDictionary<string, string> SupportedCollections =
        new Dictionary<string, string>()
        {
            {"Queue", "Enqueue"},
            {"Stack", "Push"},
            {"List", "Add"},
            { "HashSet", "Add"},
            { "SortedSet", "Add"}
        };

    public static void AddCollectionProperties(
        IPropertySymbol[] sourceProperties,
        IEnumerable<IPropertySymbol> destinationProperties,
        SourceWriterContext context)
    {
        var collectionPropertiesMatchingByName = SyntaxHelper.GetCollectionProperties(
            sourceProperties,
            destinationProperties,
            context);

        foreach (var collectionProperty in collectionPropertiesMatchingByName)
        {
            if (!CommonMappingProvider.UseCommonMappings(context, collectionProperty))
            {
                context.CurrentNotMappedProperties.Add(collectionProperty.DestinationProperty);
            }
        }
    }

    public static string GetLocalFunctionForCollection(IPropertySymbol destination,
        PropertyMapFromPair customMap, IPropertySymbol source, string functionName) 
        => destination.Type switch
        {
            IArrayTypeSymbol arrayType => BuildArrayLocalFunction(destination, source, functionName, arrayType),
            INamedTypeSymbol namedType => BuildSupportedCollectionLocalFunction(destination, customMap, source, namedType),
            _ => string.Empty
        };

    private static string BuildSupportedCollectionLocalFunction(IPropertySymbol innerDestinationProperty,
        PropertyMapFromPair customMap, IPropertySymbol innerSourceProperty, INamedTypeSymbol namedType)
    {
        //currently we only support single type collections
        var collectionArgumentType = namedType.TypeArguments[0];

        ThrowIfNotCollection(namedType);
        var genericType = namedType.ConstructedFrom;

        return @$"
            {innerDestinationProperty.Type} Map{customMap.Destination}FromCollection({innerSourceProperty.Type} sourceCollection)
            {{
                var results = new {InitializeCollection(genericType, collectionArgumentType.Name)};
                foreach(var item in sourceCollection)
                {{
                    var mappedItem = {GetMappingExpression(collectionArgumentType)}
                    results.{SupportedCollections[genericType.Name]}(mappedItem);
                }}

                return results;
            }}";
    }

    private static string BuildArrayLocalFunction(IPropertySymbol innerDestinationProperty,
        IPropertySymbol innerSourceProperty, string functionName, IArrayTypeSymbol arrayType) 
        => @$"
            {innerDestinationProperty.Type} {functionName}({innerSourceProperty.Type} sourceCollection)
            {{
                var results = new {arrayType.ElementType}[sourceCollection.Count()];
                for (int i = 0; i < sourceCollection.Count(); i++)
                {{
                    var item = sourceCollection[i];
                    var mappedItem = {GetMappingExpression(arrayType.ElementType)}
                    results[i] = mappedItem;
                }}
                return results;
            }}";

    private static string GetMappingExpression(ITypeSymbol symbol) 
        => symbol.IsSimpleTypeSymbol() 
            ? "item;" 
            : $"MapTo{symbol.ToString().Replace(".", string.Empty)}(item);";

    private static string InitializeCollection(ISymbol genericType, string collectionArgumentType)
    {
        if (SupportedCollections.Keys.FirstOrDefault(x => x == genericType.Name)
            is { } validCollection)
        {
            return $"{validCollection}<{collectionArgumentType}>()";
        }

        return $"{genericType.Name} is not a supported collection type";
    }

    private static void ThrowIfNotCollection(ISymbol genericType)
    {
        if (genericType.ContainingNamespace.ToString() is not ("System.Collections" or "System.Collections.Generic"))
        {
            throw new InvalidOperationException($"{genericType} is not a collection");
        }
    }
}