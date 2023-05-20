using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator;

public class SourceWriter
{
    private readonly MappingInfo[] _maps;
    private readonly Compilation _compilation;
    private readonly List<string> _mapMethodsDefinitions = new();
    public SourceWriter(IEnumerable<InvocationExpressionSyntax> maps, Compilation compilation)
    {
        _maps = maps.Select(x => SyntaxHelper.GetMappingInfo(x, compilation)).ToArray();
        _compilation = compilation;
    }

    public (string contract, string implementation)  GenerateSource()
    {
        var implementationBuilder = new StringBuilder();
        AddNamespace(implementationBuilder, 0, AddClass);
        var implementation = implementationBuilder.ToString();

        var contractBuilder = new StringBuilder();
        AddNamespace(contractBuilder, 0, AddInterface);
        var contract = contractBuilder.ToString();

        return (contract, implementation);
    }

    private void AddInterface(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("public interface IMapGenerator", indent);
        builder.AppendLine("{", indent);
        AddInterfaceMethodsDeclaration(builder, indent);
        builder.AppendLine("}", indent);
    }

    private void AddInterfaceMethodsDeclaration(StringBuilder builder, int indent)
    {
        indent++;
        foreach (var definition in _mapMethodsDefinitions)
        {
            builder.AppendLine(definition, indent);
        }

    }

    private void AddNamespace(StringBuilder builder, int indent, Action<StringBuilder, int> addBody)
    {
        builder.AppendLine("namespace MapsGenerator", indent);
        builder.AppendLine("{", indent);
        addBody(builder, indent);
        builder.AppendLine("}", indent);
    }

    private void AddClass(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("public class MapGenerator : IMapGenerator", indent);
        builder.AppendLine("{", indent);
        AddClassMethodsDeclaration(builder, indent);
        builder.AppendLine("}", indent);
    }

    private void AddClassMethodsDeclaration(StringBuilder builder, int indent)
    {
        indent++;
        foreach (var map in _maps)
        {
            var methodName = $"Map({map.SourceFullName} source, out {map.DestinationFullName} destination)";
            _mapMethodsDefinitions.Add($"void {methodName};");
            _mapMethodsDefinitions.Add($"bool Try{methodName};");
            builder.AppendLine($"public void {methodName}", indent);
            builder.AppendLine("{", indent);
            AddMapAddMethodBody(builder, map, indent);
            builder.AppendLine("}", indent);
            builder.AppendLine();

            builder.AppendLine($"public bool Try{methodName}", indent);
            builder.AppendLine("{", indent);
            AddTryMethodBody(builder, indent);
            builder.AppendLine("}", indent);
            builder.AppendLine();

        }
    }

    private static void AddTryMethodBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("try", indent);
        builder.AppendLine("{", indent);
        AddTryBody(builder, indent);
        builder.AppendLine("}", indent);
        builder.AppendLine("catch", indent);
        builder.AppendLine("{", indent);
        AddCatchBody(builder, indent);
        builder.AppendLine("}", indent);

    }

    private static void AddTryBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine($"Map(source, out destination);", indent);
        builder.AppendLine("return true;", indent);
    }

    private static void AddCatchBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("destination = null;", indent);
        builder.AppendLine("return false;", indent);
    }

    private void AddMapAddMethodBody(StringBuilder builder, MappingInfo mappingInfo, int indent)
    {
        indent++;
        var mappings = MappingProvider.GetMappings(mappingInfo, _maps, _compilation);
        foreach (var mapping in mappings.ComplexMappingInfo)
        {
            builder.AppendLine(mapping.Invocation, indent);
        }

        builder.AppendLine($"destination = new {mappingInfo.DestinationFullName}", indent);
        builder.AppendLine("{", indent);
        AddClassInitializationBody(builder, mappings, indent);
        builder.AppendLine("};", indent);
    }

    private static void AddClassInitializationBody(StringBuilder builder, Mappings mappings, int indent)
    {
        indent++;
        foreach (var matchingByName in mappings.MatchingByName)
        {
            builder.AppendLine(matchingByName, indent);
        }

        foreach (var complexMappingInfo in mappings.ComplexMappingInfo)
        {
            builder.AppendLine($"{complexMappingInfo.Destination} = {complexMappingInfo.Variable},", indent);
        }

        foreach (var mapFrom in mappings.MapFrom)
        {
            builder.AppendLine(mapFrom, indent);
        }

        foreach (var excludedProperty in mappings.Excluded)
        {
            builder.AppendLine(excludedProperty, indent);
        }
    }
}