using System.Text;
using MapsGenerator.DTOs;
using MapsGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator;

public class MapsGeneratorSourceWriter
{
    private readonly List<ProfileDefinition> _profileDefinitions;
    private readonly Compilation _compilation;
    private readonly List<MethodDefinition> _mapMethodsDefinitions = new();
    public MapsGeneratorSourceWriter(List<ProfileDefinition> profileDefinitions, Compilation compilation)
    {
        _profileDefinitions = profileDefinitions;
        _compilation = compilation;
    }

    public (string contract, string implementation) GenerateSource()
    {
        var implementationBuilder = new StringBuilder();
        AddNamespace(implementationBuilder, 0, AddMapGeneratorClass);
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
            builder.AppendLine(definition.ProfileDocumentation, indent);
            builder.AppendLine(definition.Name, indent);
        }

    }

    private static void AddNamespace(StringBuilder builder, int indent, Action<StringBuilder, int> addBody)
    {
        builder.AppendLine("namespace MapsGenerator", indent);
        builder.AppendLine("{", indent);
        addBody(builder, indent);
        builder.AppendLine("}", indent);
    }

    private void AddMapGeneratorClass(StringBuilder builder, int indent)
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
        foreach (var definition in _profileDefinitions)
        {
            foreach (var map in definition.Maps)
            {
                var parameters = string.Join("", map.MapFromParameterProperties
                    .Select(x => x.Type + " " + x.VariableName + ", "));

                var methodName = $"Map({map.SourceFullName} source, {parameters}out {map.DestinationFullName} destination)";
                var profileName = SyntaxHelper.GetTypeSyntaxFullName(definition.Profile);
                var profileDocumentation = @$"
/// <summary>
/// Profile <see cref=""{profileName}""/>
/// </summary>";

                _mapMethodsDefinitions.Add(new MethodDefinition($"void {methodName};", profileDocumentation));
                _mapMethodsDefinitions.Add(new MethodDefinition($"bool Try{methodName};", profileDocumentation));

                builder.AppendLine(profileDocumentation, indent);
                builder.AppendLine($"public void {methodName}", indent);
                builder.AppendLine("{", indent);
                AddMapMethodBody(builder, map, definition, indent);
                builder.AppendLine("}", indent);

                builder.AppendLine(profileDocumentation, indent);
                builder.AppendLine($"public bool Try{methodName}", indent);
                builder.AppendLine("{", indent);
                AddTryMethodBody(builder, map, indent);
                builder.AppendLine("}", indent);
            }
        }
    }

    private static void AddTryMethodBody(StringBuilder builder, MappingInfo map, int indent)
    {
        indent++;
        builder.AppendLine("try", indent);
        builder.AppendLine("{", indent);
        AddTryBody(builder, map, indent);
        builder.AppendLine("}", indent);
        builder.AppendLine("catch", indent);
        builder.AppendLine("{", indent);
        AddCatchBody(builder, indent);
        builder.AppendLine("}", indent);

    }

    private static void AddTryBody(StringBuilder builder, MappingInfo map, int indent)
    {
        var parameters = string.Join("", map.MapFromParameterProperties
            .Select(x => x.VariableName + ", "));

        indent++;
        builder.AppendLine($"Map(source, {parameters}out destination);", indent);
        builder.AppendLine("return true;", indent);
    }

    private static void AddCatchBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("destination = null;", indent);
        builder.AppendLine("return false;", indent);
    }

    private void AddMapMethodBody(StringBuilder builder, MappingInfo mappingInfo, ProfileDefinition profileDefinition, int indent)
    {
        indent++;
        var mappings = MappingProvider.GetMappings(mappingInfo, profileDefinition.Maps, _compilation);
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

        foreach (var mapFrom in mappings.MapFromParameter)
        {
            builder.AppendLine(mapFrom, indent);
        }

        foreach (var excludedProperty in mappings.Excluded)
        {
            builder.AppendLine(excludedProperty, indent);
        }
    }
}
