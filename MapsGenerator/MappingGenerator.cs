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

        var enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => Filter.IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => Filter.GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // Combine the selected classes with the `Compilation`
        var compilationAndEnums
            = context.CompilationProvider.Combine(enumDeclarations.Collect());

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

        foreach (var classDeclarationSyntax in distinctClasses)
        {
            context.AddSource("MapperImplementation", new SourceWriter(classDeclarationSyntax, compilation).GenerateSource());
        }
    }


}

public class SourceWriter
{
    private readonly ClassDeclarationSyntax _classDeclarationSyntax;
    private readonly Compilation _compilation;

    public SourceWriter(ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation)
    {
        _classDeclarationSyntax = classDeclarationSyntax;
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
        if (!Filter.TryFindMapsInvocations(_classDeclarationSyntax, out var maps))
        {
            builder.AppendLine("no maps were found.", indent);
            return;
        }

        foreach (var map in maps)
        {
            var typeArguments = GetTypeArguments(map).ToArray();
            var mappingInfo = new MappingInfo(
                typeArguments[0],
                typeArguments[1],
                GetTypeSyntaxName(typeArguments[0]),
                GetTypeSyntaxName(typeArguments[1]),
                GetTypeSyntaxFullName(typeArguments[0]),
                GetTypeSyntaxFullName(typeArguments[1]));

            builder.AppendLine($"public {mappingInfo.DestinationFullName} {mappingInfo.SourceName}_To_{mappingInfo.DestinationName}({mappingInfo.SourceFullName} source)", indent);
            builder.AppendLine("{", indent);
            AddMethodBody(builder, mappingInfo, indent);
            builder.AppendLine("}", indent);
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
        var sourceProperties = GetProperties(mappingInfo.Source);
        var destinationProperties = GetProperties(mappingInfo.Destination);

        var simplePropertiesMatchingByName = GetMatchingProperties(
            sourceProperties.Where(IsSimplePropertySymbol),
            destinationProperties.Where(IsSimplePropertySymbol).ToArray());

        foreach (var simpleProperty in simplePropertiesMatchingByName)
        {
            builder.AppendLine($"{simpleProperty} = source.{simpleProperty},", indent);
        }
    }
    private List<string> GetMatchingProperties(IEnumerable<IPropertySymbol> sourceProperties, IPropertySymbol[] destinationProperties)
    {
        var matchingProperties = new List<string>();

        foreach (var sourceProperty in sourceProperties)
        {
            var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);

            if (destinationProperty != null && sourceProperty.Type.Equals(destinationProperty.Type))
            {
                matchingProperties.Add(destinationProperty.Name);
            }
        }

        return matchingProperties;
    }

    private bool IsSimplePropertySymbol(IPropertySymbol property)
        => property.Type.TypeKind != TypeKind.Class || property.Type.SpecialType == SpecialType.System_String;

    private IEnumerable<IPropertySymbol> GetProperties(ExpressionSyntax typeSyntax)
    {
        var semanticModel = _compilation.GetSemanticModel(typeSyntax.SyntaxTree);

        return semanticModel.GetSymbolInfo(typeSyntax).Symbol is not INamedTypeSymbol typeSymbol
            ? Enumerable.Empty<IPropertySymbol>()
            : typeSymbol.GetMembers().OfType<IPropertySymbol>();
    }

    private static string GetTypeSyntaxName(TypeSyntax typeSyntax)
        => (typeSyntax as IdentifierNameSyntax)?.Identifier.Text ?? throw new InvalidOperationException("typeSyntax is not an Identifier");

    private string GetTypeSyntaxFullName(ExpressionSyntax typeSyntax)
        => _compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetSymbolInfo(typeSyntax).Symbol?.ToString()
           ?? throw new InvalidOperationException("typeSyntax is not an Identifier");

    private static SeparatedSyntaxList<TypeSyntax> GetTypeArguments(InvocationExpressionSyntax map)
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

public class MappingInfo
{
    public MappingInfo(TypeSyntax source, TypeSyntax destination, string sourceName, string destinationName, string sourceFullName, string destinationFullName)
    {
        Source = source;
        Destination = destination;
        SourceName = sourceName;
        DestinationName = destinationName;
        SourceFullName = sourceFullName;
        DestinationFullName = destinationFullName;
    }

    public TypeSyntax Source { get; set; }
    public TypeSyntax Destination { get; set; }
    public string SourceName { get; set; }
    public string DestinationName { get; set; }
    public string SourceFullName { get; set; }
    public string DestinationFullName { get; set; }
}