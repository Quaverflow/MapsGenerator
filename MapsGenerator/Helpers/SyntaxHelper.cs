using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.Helpers;

public static class SyntaxHelper
{
    private static readonly Dictionary<CSharpSyntaxNode, string> TypeSyntaxFullNameCache = new();

    public static string GetFullNamespace(this ITypeSymbol typeSymbol)
    {
        var namespaceSymbol = typeSymbol.ContainingNamespace;
        var namespaceName = namespaceSymbol.Name;

        while (!namespaceSymbol.IsGlobalNamespace)
        {
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
            if (!namespaceSymbol.IsGlobalNamespace)
            {
                namespaceName = namespaceSymbol.Name + "." + namespaceName;
            }
        }

        return namespaceName;
    }

    public static MappingInfo GetMappingInfo(InvocationExpressionSyntax map, Compilation compilation)
    {
        var typeArguments = GetTypeArguments(map).ToArray();
        var mappingInfo = new MappingInfo(
            typeArguments[0],
            typeArguments[1],
            GetTypeSyntaxName(typeArguments[0]),
            GetTypeSyntaxName(typeArguments[1]),
            GetTypeSyntaxFullName(typeArguments[0], compilation),
            GetTypeSyntaxFullName(typeArguments[1], compilation),
            map,
            compilation);
        return mappingInfo;
    }

    public static List<PropertyPair> GetSimpleMatchingProperties(IPropertySymbol[] sourceProperties,
        IPropertySymbol[] destinationProperties, SourceWriterContext context)
    {
        var matchingProperties = new List<PropertyPair>();

        foreach (var destinationProperty in destinationProperties)
        {
            if (IsSimplePropertySymbol(destinationProperty))
            {
                var sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name);
                if (sourceProperty != null && destinationProperty.Type.Equals(destinationProperty.Type, SymbolEqualityComparer.Default))
                {
                    matchingProperties.Add(new PropertyPair(sourceProperty, destinationProperty));
                }
                else
                {
                    context.NotMappedProperties.Add(destinationProperty);
                }
            }
        }

        return matchingProperties;
    }

    public static List<PropertyPair> GetEnumMatchingProperties(IPropertySymbol[] sourceProperties,
        IEnumerable<IPropertySymbol> destinationProperties, SourceWriterContext context)
    {
        var matchingProperties = new List<PropertyPair>();
        var enumDestinationProperties = destinationProperties.Where(IsEnum).ToArray();

        foreach (var destinationProperty in enumDestinationProperties)
        {
            var sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name);
            if (sourceProperty != null)
            {
                matchingProperties.Add(new PropertyPair(sourceProperty, destinationProperty));
            }
            else
            {
                context.NotMappedProperties.Add(destinationProperty);
            }
        }

        return matchingProperties;
    }

    public static List<PropertyPair> GetComplexMatchingProperties(IPropertySymbol[] sourceProperties,
        IEnumerable<IPropertySymbol> destinationProperties, SourceWriterContext context)
    {
        var matchingProperties = new List<PropertyPair>();
        var complexDestinationProperties = destinationProperties.Where(IsComplexPropertySymbol).ToArray();

        foreach (var destinationProperty in complexDestinationProperties)
        {
            var sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name);
            if (sourceProperty != null)
            {
                matchingProperties.Add(new PropertyPair(sourceProperty, destinationProperty));
            }
            else
            {
                context.NotMappedProperties.Add(destinationProperty);
            }
        }

        return matchingProperties;
    }

    public static bool IsSimplePropertySymbol(this IPropertySymbol property)
        => property.Type.IsSimplePropertySymbol() && !property.IsEnum();

    public static bool IsComplexPropertySymbol(this IPropertySymbol property) =>
        !property.Type.IsSimplePropertySymbol()
        && !property.Type.IsCollectionSymbol()
        && !property.IsEnum();

    public static bool IsSimplePropertySymbol(this ITypeSymbol type)
        => type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Array || type.SpecialType == SpecialType.System_String;

    public static bool IsEnum(this IPropertySymbol property)
        => property.Type.TypeKind is TypeKind.Enum;

    public static IEnumerable<IPropertySymbol> GetProperties(ExpressionSyntax typeSyntax, SemanticModel semanticModel) 
        => semanticModel.GetSymbolInfo(typeSyntax).Symbol is not INamedTypeSymbol typeSymbol 
            ? Enumerable.Empty<IPropertySymbol>()
            : typeSymbol.GetMembers().OfType<IPropertySymbol>();

    public static string GetTypeSyntaxName(TypeSyntax typeSyntax)
        => (typeSyntax as IdentifierNameSyntax)?.Identifier.Text ?? throw new InvalidOperationException("typeSyntax is not an Identifier");

    public static string GetTypeSyntaxFullName(CSharpSyntaxNode typeSyntax, Compilation compilation)
    {
        if (TypeSyntaxFullNameCache.TryGetValue(typeSyntax, out var fullName))
        {
            return fullName;
        }

        fullName = compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetSymbolInfo(typeSyntax).Symbol?.ToString()
                   ?? throw new InvalidOperationException($"{typeSyntax} is not an Identifier");

        TypeSyntaxFullNameCache[typeSyntax] = fullName;
        return fullName;
    }

    public static string GetTypeSyntaxFullName(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var namespaceDeclaration = classDeclarationSyntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().First().Name.ToString();
        var className = classDeclarationSyntax.Identifier.ValueText;
        var qualifiedName = SyntaxFactory.QualifiedName(
            SyntaxFactory.IdentifierName(namespaceDeclaration),
            SyntaxFactory.IdentifierName(className));
        return qualifiedName.ToString();
    }

    public static SeparatedSyntaxList<TypeSyntax> GetTypeArguments(InvocationExpressionSyntax map)
    {
        if (map.Expression is GenericNameSyntax genericMethodName)
        {
            return genericMethodName.TypeArgumentList.Arguments;
        }

        throw new InvalidOperationException("couldn't find type arguments");
    }

    public static List<PropertyPair> GetCollectionProperties(IPropertySymbol[] sourceProperties, IEnumerable<IPropertySymbol> destinationProperties, SourceWriterContext context)
    {
        var matchingProperties = new List<PropertyPair>();

        foreach (var destinationProperty in destinationProperties)
        {
            if (destinationProperty.Type.IsCollectionSymbol())
            {
                var sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name);
                if (sourceProperty != null && destinationProperty.Type.Equals(destinationProperty.Type, SymbolEqualityComparer.Default))
                {
                    matchingProperties.Add(new PropertyPair(sourceProperty, destinationProperty));
                }
                else
                {
                    context.NotMappedProperties.Add(destinationProperty);
                }
            }
        }

        return matchingProperties;
    }

    public static bool IsCollectionSymbol(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.IsSimplePropertySymbol())
        {
            return false;
        }

        var enumerableType = typeSymbol.AllInterfaces.FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_IEnumerable);
        if (enumerableType is { TypeArguments.Length: 1 or 0 })
        {
            return true;
        }

        var collectionType = typeSymbol.AllInterfaces.FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_ICollection_T);
        return collectionType != null;
    }

    public static IPropertySymbol GetInnerProperty(SourceWriterContext context, IPropertySymbol[] currentType, string nestedProperty)
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
                context.TypesProperties.Add(result.Type.Name, new TypeProperties(properties, result.Type));
                currentType = properties;
            }

            result = currentType.First(x => x.Name == property);
        }

        return result;
    }
}