using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.Helpers;

public static class SyntaxHelper
{
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
                if (sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name) is { } sourceProperty
                    && destinationProperty.Type.Equals(destinationProperty.Type, SymbolEqualityComparer.Default))
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
            if (sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name) is { } sourceProperty)
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
            if (sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name) is { } sourceProperty)
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
        => (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Array) || type.SpecialType == SpecialType.System_String;

    public static bool IsEnum(this IPropertySymbol property)
        => property.Type.TypeKind is TypeKind.Enum;

    public static IEnumerable<IPropertySymbol> GetProperties(ExpressionSyntax typeSyntax, SemanticModel semanticModel)
    {
        return semanticModel.GetSymbolInfo(typeSyntax).Symbol is not INamedTypeSymbol typeSymbol
            ? Enumerable.Empty<IPropertySymbol>()
            : typeSymbol.GetMembers().OfType<IPropertySymbol>();
    }

    public static string GetTypeSyntaxName(TypeSyntax typeSyntax)
        => (typeSyntax as IdentifierNameSyntax)?.Identifier.Text ?? throw new InvalidOperationException("typeSyntax is not an Identifier");

    public static string GetTypeSyntaxFullName(CSharpSyntaxNode typeSyntax, Compilation compilation)
        => compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetSymbolInfo(typeSyntax).Symbol?.ToString()
           ?? throw new InvalidOperationException($"{typeSyntax} is not an Identifier");

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
                if (sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name) is { } sourceProperty
                    && destinationProperty.Type.Equals(destinationProperty.Type, SymbolEqualityComparer.Default))
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
        var enumerableType = typeSymbol.AllInterfaces
            .FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_IEnumerable);


        if (enumerableType != null)
        {
            if (enumerableType is { TypeArguments.Length: 1 })
            {
                return true;
            }

            if (enumerableType.TypeArguments.Length == 0)
            {
                return true;
            }
        }

        var collectionType = typeSymbol.AllInterfaces
            .FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_ICollection_T);

        return collectionType is { };
    }
}