using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator;

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
        builder.AppendLine("public class MapperImplementation", indent);
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
        builder.AppendLine("};", indent);
    }

    private void AddClassInitializationBody(StringBuilder builder, MappingInfo mappingInfo, int indent)
    {
        indent++;
        var sourceProperties = SyntaxHelper.GetProperties(mappingInfo.Source, _compilation).ToArray();
        var destinationProperties = SyntaxHelper.GetProperties(mappingInfo.Destination, _compilation).ToArray();

        var simplePropertiesMatchingByName = SyntaxHelper.GetSimpleMatchingProperties(
            sourceProperties,
            destinationProperties);

        var complexPropertiesMatchingByName = SyntaxHelper.GetComplexMatchingProperties(
            sourceProperties,
            destinationProperties);

        foreach (var simpleProperty in simplePropertiesMatchingByName)
        {
            if (mappingInfo.ExcludedProperties.Any(x => x == simpleProperty.DestinationProperty.Name))
            {
                builder.AppendLine($"//{simpleProperty.DestinationProperty.Name} was manually excluded", indent);
                continue;
            }

            if (mappingInfo.MapFromProperties.FirstOrDefault(x =>
                    x.Destination == simpleProperty.DestinationProperty.Name) is not null)
            {
                continue;
            }

            builder.AppendLine($"{simpleProperty.DestinationProperty.Name} = source.{simpleProperty.SourceProperty.Name},", indent);
        }

        foreach (var complexProperty in complexPropertiesMatchingByName)
        {
            if (mappingInfo.ExcludedProperties.Any(x => x == complexProperty.DestinationProperty.Name))
            {
                builder.AppendLine($"//{complexProperty.DestinationProperty.Name} was manually excluded", indent);
                continue;
            }

            if (mappingInfo.MapFromProperties.FirstOrDefault(x =>
                    x.Destination == complexProperty.DestinationProperty.Name) is not null)
            {
                continue;
            }

            if (_maps.FirstOrDefault(x =>
                    x.SourceFullName == complexProperty.SourceProperty.Type.ToString() &&
                    x.DestinationFullName == complexProperty.DestinationProperty.Type.ToString()) is { } map)
            {
                builder.AppendLine($"{complexProperty.DestinationProperty.Name} = {map.MappingName}(source.{complexProperty.SourceProperty.Name}),", indent);
            }
            else
            {
                builder.AppendLine($"//{complexProperty.DestinationProperty.Name} = source.{complexProperty.SourceProperty.Name} these property have matching name but no map has been defined", indent);
            }
        }

        foreach (var customMap in mappingInfo.MapFromProperties)
        {
            builder.AppendLine($"{customMap.Destination} = source.{customMap.Source},", indent);
        }
    }
}