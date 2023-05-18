using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MapsGenerator;

public static class SourceGenerationHelper
{
    public const string Mapper = @"
namespace MapsGenerator
{
    internal abstract classDeclarationSyntax MapperBase
    {
        protected void Map<TSource, TDestination>()
        {
        }   

        protected void Map<TSource, TDestination>(Action<TSource, TDestination> options)
        {
        }
    }
}";
}

[Generator]
public class MappingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("MapperBase.g.cs", SourceText.From(SourceGenerationHelper.Mapper, Encoding.UTF8));
        });

        var declarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => Filter.IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => Filter.GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // Combine the selected classes with the `Compilation`
        var compilationAndEnums
            = context.CompilationProvider.Combine(declarations.Collect());

        // Generate the source using the compilation and classes
        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctClasses = classes
            .OfType<ClassDeclarationSyntax>() //to avoid annoying nullability issues
            .Distinct()
            .ToArray();

        var allMaps = new List<InvocationExpressionSyntax>();
        foreach (var classDeclarationSyntax in distinctClasses)
        {
            if (Filter.TryFindMapsInvocations(classDeclarationSyntax, out var maps))
            {
                allMaps.AddRange(maps);
            }
        }
        context.AddSource("MapperImplementation", new SourceWriter(allMaps, compilation).GenerateSource());
    }


}

public class SourceWriter
{
    private readonly MappingInfo[] _maps;
    private readonly Compilation _compilation;

    public SourceWriter(IEnumerable<InvocationExpressionSyntax> maps, Compilation compilation)
    {
        _maps = maps.Select(x => SyntaxHelper.GetMappingInfo(x, compilation)).ToArray();
        _compilation = compilation;
    }

    public string GenerateSource()
    {
        var stringBuilder = new StringBuilder();

        AddNamespace(stringBuilder, 0);

        return stringBuilder.ToString();
    }

    private void AddNamespace(StringBuilder builder, int indent)
    {
        builder.AppendLine("namespace MapsGenerator", indent);
        builder.AppendLine("{", indent);
        AddClass(builder, indent);
        builder.AppendLine("}", indent);
    }

    private void AddClass(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("public classDeclarationSyntax MapperImplementation", indent);
        builder.AppendLine("{", indent);
        AddMethodsDeclaration(builder, indent);
        builder.AppendLine("}", indent);
    }

    private void AddMethodsDeclaration(StringBuilder builder, int indent)
    {
        indent++;
        foreach (var map in _maps)
        {
            builder.AppendLine($"public {map.DestinationFullName} {map.MappingName}({map.SourceFullName} source)", indent);
            builder.AppendLine("{", indent);
            AddMethodBody(builder, map, indent);
            builder.AppendLine("}", indent);
            builder.AppendLine();
        }
    }

    private void AddMethodBody(StringBuilder builder, MappingInfo mappingInfo, int indent)
    {
        indent++;
        builder.AppendLine($"return new {mappingInfo.DestinationFullName}", indent);
        builder.AppendLine("{", indent);
        AddClassInitializationBody(builder, mappingInfo, indent);
        builder.AppendLine("}", indent);
    }

    private void AddClassInitializationBody(StringBuilder builder, MappingInfo mappingInfo, int indent)
    {
        indent++;
        var sourceProperties = SyntaxHelper.GetProperties(mappingInfo.Source, _compilation).ToArray();
        var destinationProperties = SyntaxHelper.GetProperties(mappingInfo.Destination, _compilation).ToArray();

        var simplePropertiesMatchingByName = SyntaxHelper.GetSimpleMatchingProperties(
            sourceProperties,
            destinationProperties);

        foreach (var simpleProperty in simplePropertiesMatchingByName)
        {
            builder.AppendLine($"{simpleProperty.DestinationProperty.Name} = source.{simpleProperty.SourceProperty.Name},", indent);
        }

        var complexPropertiesMatchingByName = SyntaxHelper.GetComplexMatchingProperties(
            sourceProperties,
            destinationProperties);

        foreach (var complexProperty in complexPropertiesMatchingByName)
        {
            if (_maps.FirstOrDefault(x =>
                    x.SourceFullName == complexProperty.SourceProperty.Type.ToString() &&
                    x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString()) is {} map)
            {
                builder.AppendLine($"{complexProperty.DestinationProperty.Name} = {map.MappingName}(source.{complexProperty.SourceProperty.Name})", indent);
            }
            else
            {
                builder.AppendLine($"//{complexProperty.DestinationProperty.Name} = source.{complexProperty.SourceProperty.Name} these property have matching name but no map has been defined", indent);
            }
        }
    }
}

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

    public static string GetTypeSyntaxFullName(ExpressionSyntax typeSyntax, Compilation compilation)
        => compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetSymbolInfo(typeSyntax).Symbol?.ToString()
           ?? throw new InvalidOperationException("typeSyntax is not an Identifier");

    public static SeparatedSyntaxList<TypeSyntax> GetTypeArguments(InvocationExpressionSyntax map)
    {
        if (map.Expression is GenericNameSyntax genericMethodName)
        {
            return genericMethodName.TypeArgumentList.Arguments;
        }

        throw new InvalidOperationException("couldn't find type arguments");
    }
}

public static class StringBuilderExtensions
{
    private const string Space = "    ";

    public static void AppendLine(this StringBuilder builder, string text, int indent)
        => builder.AppendLine(Space.Repeat(indent) + text);
}

public static class StringExtensions
{
    public static string Repeat(this string text, int n)
    {
        var textAsSpan = text.AsSpan();
        var span = new Span<char>(new char[textAsSpan.Length * n]);
        for (var i = 0; i < n; i++)
        {
            textAsSpan.CopyTo(span.Slice(i * textAsSpan.Length, textAsSpan.Length));
        }

        return span.ToString();
    }
}
public class PropertyPair
{
    public IPropertySymbol SourceProperty { get; set; }
    public IPropertySymbol DestinationProperty { get; set; }

    public PropertyPair(IPropertySymbol sourceProperty, IPropertySymbol destinationProperty)
    {
        SourceProperty = sourceProperty;
        DestinationProperty = destinationProperty;
    }
}

public class MappingInfo
{
    public TypeSyntax Source { get; }
    public TypeSyntax Destination { get; }
    public string SourceName { get; }
    public string DestinationName { get; }
    public string SourceFullName { get; }
    public string DestinationFullName { get; }
    public InvocationExpressionSyntax InvocationExpressionSyntax { get; }
    public string MappingName { get; }

    public MappingInfo(TypeSyntax source, TypeSyntax destination, string sourceName, string destinationName,
        string sourceFullName, string destinationFullName, InvocationExpressionSyntax invocationExpressionSyntax)
    {
        Source = source;
        Destination = destination;
        SourceName = sourceName;
        DestinationName = destinationName;
        SourceFullName = sourceFullName;
        DestinationFullName = destinationFullName;
        InvocationExpressionSyntax = invocationExpressionSyntax;
        MappingName = $"{SourceName}_To_{DestinationFullName.Replace(".", string.Empty)}";
    }
}