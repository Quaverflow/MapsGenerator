using MapsGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.DTOs;

public class ProfileInfo
{
    public TypeSyntax Source { get; }
    public TypeSyntax Destination { get; }
    public string SourceName { get; }
    public string DestinationName { get; }
    public string SourceFullName { get; }
    public string DestinationFullName { get; }
    public bool EnsureAllDestinationPropertiesAreMapped { get; }
    public bool IsEnum { get; }
    public InvocationExpressionSyntax InvocationExpressionSyntax { get; }
    public IReadOnlyList<string> ExcludedProperties { get; }
    public IReadOnlyList<PropertyMapFromConstant> MapFromConstantProperties { get; }
    public IReadOnlyList<PropertyMapFromPair> MapFromProperties { get; }
    public IReadOnlyList<EnumValueMap> MapFromEnums { get; }

    public ProfileInfo(TypeSyntax source, TypeSyntax destination, string sourceName, string destinationName,
        string sourceFullName, string destinationFullName, InvocationExpressionSyntax invocationExpressionSyntax,
        Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(destination.SyntaxTree);
        var type = semanticModel.GetSymbolInfo(destination).Symbol as INamedTypeSymbol;
        IsEnum = type?.TypeKind is TypeKind.Enum;
        Source = source;
        Destination = destination;
        SourceName = sourceName;
        DestinationName = destinationName;
        SourceFullName = sourceFullName;
        DestinationFullName = destinationFullName;
        InvocationExpressionSyntax = invocationExpressionSyntax;
        ExcludedProperties = MappingInfoProvider.GetExcludedProperties(InvocationExpressionSyntax);
        MapFromProperties = MappingInfoProvider.GetMapFromProperties(InvocationExpressionSyntax);
        MapFromConstantProperties = MappingInfoProvider.GetMapFromConstantProperties(InvocationExpressionSyntax);
        MapFromEnums = MappingInfoProvider.GetMapFromEnums(InvocationExpressionSyntax);
        EnsureAllDestinationPropertiesAreMapped = MappingInfoProvider.GetEnsureAllDestinationPropertiesAreMapped(InvocationExpressionSyntax);
    }


}