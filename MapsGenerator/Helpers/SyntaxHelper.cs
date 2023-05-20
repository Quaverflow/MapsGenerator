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
            map);
        return mappingInfo;
    }

    public static List<PropertyPair> GetSimpleMatchingProperties(IEnumerable<IPropertySymbol> sourceProperties,
        IPropertySymbol[] destinationProperties)
    {
        var matchingProperties = new List<PropertyPair>();

        foreach (var sourceProperty in sourceProperties.Where(IsSimplePropertySymbol))
        {
            var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);

            if (destinationProperty != null && sourceProperty.Type.Equals(destinationProperty.Type))
            {
                matchingProperties.Add(new PropertyPair(sourceProperty, destinationProperty));
            }
        }

        return matchingProperties;
    }

    public static List<PropertyPair> GetComplexMatchingProperties(IEnumerable<IPropertySymbol> sourceProperties,
        IEnumerable<IPropertySymbol> destinationProperties)
    {
        var matchingProperties = new List<PropertyPair>();
        var complexDestinationProperties = destinationProperties.Where(symbol => !IsSimplePropertySymbol(symbol)).ToArray();

        foreach (var sourceProperty in sourceProperties.Where(symbol => !IsSimplePropertySymbol(symbol)))
        {
            var destinationProperty = complexDestinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);

            if (destinationProperty != null)
            {
                matchingProperties.Add(new PropertyPair(sourceProperty, destinationProperty));
            }
        }

        return matchingProperties;
    }

    public static bool IsSimplePropertySymbol(IPropertySymbol property)
        => property.Type.TypeKind != TypeKind.Class || property.Type.SpecialType == SpecialType.System_String;

    public static IEnumerable<IPropertySymbol> GetProperties(ExpressionSyntax typeSyntax, Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

        return semanticModel.GetSymbolInfo(typeSyntax).Symbol is not INamedTypeSymbol typeSymbol
            ? Enumerable.Empty<IPropertySymbol>()
            : typeSymbol.GetMembers().OfType<IPropertySymbol>();
    }

    public static string GetTypeSyntaxName(TypeSyntax typeSyntax)
        => (typeSyntax as IdentifierNameSyntax)?.Identifier.Text ?? throw new InvalidOperationException("typeSyntax is not an Identifier");

    public static string GetTypeSyntaxFullName(CSharpSyntaxNode typeSyntax, Compilation compilation)
        => compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetSymbolInfo(typeSyntax).Symbol?.ToString()
           ?? throw new InvalidOperationException("typeSyntax is not an Identifier");

    public static string GetTypeSyntaxFullName(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var namespaceDeclaration = classDeclarationSyntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().First().Name.ToString();
        var className = classDeclarationSyntax.Identifier.Text;
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
}