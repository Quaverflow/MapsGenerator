using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;

namespace MapsGenerator.Helpers.MappingProviders;

public static class CollectionMappingProvider
{
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
            if (CommonMappingProvider.UseCommonMappings(context, collectionProperty))
            {
                continue;
            }

            context.NotMappedProperties.Add(collectionProperty.DestinationProperty);
        }

    }


    public static string GetLocalFunctionForCollection(IPropertySymbol innerDestinationProperty,
        PropertyMapFromPair customMap, IPropertySymbol innerSourceProperty, string functionName)
    {
        var localFunction = string.Empty;
        if (innerDestinationProperty.Type is IArrayTypeSymbol arrayType)
        {
            var collectionArgumentType = arrayType.ElementType.ToString();

            localFunction = @$"
            {innerDestinationProperty.Type} {functionName}({innerSourceProperty.Type} sourceCollection)
            {{
                var results = new List<{collectionArgumentType}>();
                foreach(var item in sourceCollection)
                {{
                    var mappedItem = Map<{collectionArgumentType}>(item);
                    results.Add(mappedItem);
                }}

                return results.ToArray();
            }}";
        }

        if (innerDestinationProperty.Type is INamedTypeSymbol namedType)
        {
            var collectionArgumentType = string.Join(", ", namedType.TypeArguments.Select(x => x.ToString()));
            var collectionType = $"{GetCollectionType(innerDestinationProperty.Type)}<{collectionArgumentType}>()";
            var action = GetAddToCollectionActionName(innerDestinationProperty.Type);
            localFunction = @$"
            {innerDestinationProperty.Type} Map{customMap.Destination}FromCollection({innerSourceProperty.Type} sourceCollection)
            {{
                var results = new {collectionType};
                foreach(var item in sourceCollection)
                {{
                    var mappedItem = Map<{collectionArgumentType}>(item);
                    results.{action}(mappedItem);
                }}

                return results;
            }}";
        }

        return localFunction;
    }

    private static string GetCollectionType(ITypeSymbol symbol)
    {
        var collectionTypeName = string.Empty;

        if (symbol is INamedTypeSymbol { IsGenericType: true } namedType)
        {
            var genericTypeDefinition = namedType.ConstructedFrom;

            var containingNamespace = genericTypeDefinition.ContainingNamespace.ToString();
            if (containingNamespace is "System.Collections" or "System.Collections.Generic")
            {
                collectionTypeName = genericTypeDefinition.Name switch
                {
                    "Queue" => "Queue",
                    "SortedList" => "UNSUPPORTED MAPPING",
                    "Stack" => "Stack",
                    "List" => "List",
                    "Dictionary" => "UNSUPPORTED MAPPING",
                    "HashSet" => "HashSet",
                    "SortedSet" => "SortedSet",
                    _ => "UnknownCollection"
                } ?? throw new InvalidOperationException();
            }
        }

        return collectionTypeName;
    }

    private static string GetAddToCollectionActionName(ITypeSymbol symbol)
    {
        var collectionTypeName = string.Empty;

        if (symbol is INamedTypeSymbol { IsGenericType: true } namedType)
        {
            var genericTypeDefinition = namedType.ConstructedFrom;

            var containingNamespace = genericTypeDefinition.ContainingNamespace.ToString();
            if (containingNamespace is "System.Collections" or "System.Collections.Generic")
            {
                collectionTypeName = genericTypeDefinition.Name switch
                {
                    "Queue" => "Enqueue",
                    "SortedList" => "Add",
                    "Stack" => "Push",
                    "List" => "Add",
                    "Dictionary" => "Add",
                    "HashSet" => "Add",
                    "SortedSet" => "Add",
                    _ => "UnknownCollection"
                } ?? throw new InvalidOperationException();
            }
        }

        return collectionTypeName;
    }

}