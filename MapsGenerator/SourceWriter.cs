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
            var methodName = $"Map({map.SourceFullName} source, out {map.DestinationFullName} destination)";
            builder.AppendLine($"public void {methodName}", indent);
            builder.AppendLine("{", indent);
            AddMethodBody(builder, map, indent);
            builder.AppendLine("}", indent);
            builder.AppendLine();

            builder.AppendLine($"public bool Try{methodName}", indent);
            builder.AppendLine("{", indent);
            AddTryMethodBody(builder, indent);
            builder.AppendLine("}", indent);
            builder.AppendLine();

        }
    }

    private void AddTryMethodBody(StringBuilder builder, int indent)
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

    private void AddTryBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine($"Map(source, out destination);", indent);
        builder.AppendLine("return true;", indent);
    }
    
    private void AddCatchBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("destination = null;", indent);
        builder.AppendLine("return false;", indent);
    }

    private void AddMethodBody(StringBuilder builder, MappingInfo mappingInfo, int indent)
    {
        indent++;
        var mappings = GetMappings(mappingInfo);
        foreach (var mapping in mappings.ComplexMappingInfo)
        {
            builder.AppendLine(mapping.Invocation, indent);
        }

        builder.AppendLine($"destination = new {mappingInfo.DestinationFullName}", indent);
        builder.AppendLine("{", indent);
        AddClassInitializationBody(builder, mappings, indent);
        builder.AppendLine("};", indent);
    }

    private void AddClassInitializationBody(StringBuilder builder, Mappings mappings, int indent)
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

    //todo remove duplication
    private Mappings GetMappings(MappingInfo mappingInfo)
    {
        var sourceProperties = SyntaxHelper.GetProperties(mappingInfo.Source, _compilation).ToArray();
        var destinationProperties = SyntaxHelper.GetProperties(mappingInfo.Destination, _compilation).ToArray();

        var simplePropertiesMatchingByName = SyntaxHelper.GetSimpleMatchingProperties(
            sourceProperties,
            destinationProperties);

        var mappings = new Mappings();

        foreach (var simpleProperty in simplePropertiesMatchingByName)
        {
            if (mappingInfo.ExcludedProperties.Any(x => x == simpleProperty.DestinationProperty.Name))
            {
                mappings.Excluded.Add("//{simpleProperty.DestinationProperty.Name} was manually excluded");
                continue;
            }

            if (mappingInfo.MapFromProperties.FirstOrDefault(x =>
                    x.Destination == simpleProperty.DestinationProperty.Name) is not null)
            {
                continue;
            }

            mappings.MatchingByName.Add($"{simpleProperty.DestinationProperty.Name} = source.{simpleProperty.SourceProperty.Name},");
        }


        var complexPropertiesMatchingByName = SyntaxHelper.GetComplexMatchingProperties(
            sourceProperties,
            destinationProperties);

        foreach (var complexProperty in complexPropertiesMatchingByName)
        {
            if (mappingInfo.ExcludedProperties.Any(x => x == complexProperty.DestinationProperty.Name))
            {
                mappings.MapFrom.Add($"//{complexProperty.DestinationProperty.Name} was manually excluded");

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

                var variable = complexProperty.DestinationProperty.Name.FirstCharToLower();
                var invocation = $"Map(source.{complexProperty.SourceProperty.Name}, out var {variable});";

                //todo add a check for duplication
                mappings.ComplexMappingInfo.Add(new ComplexMappingInfo(invocation, variable, complexProperty.DestinationProperty.Name));
            }
            else
            {
                mappings.MatchingByName.Add($"//{complexProperty.DestinationProperty.Name} = source.{complexProperty.SourceProperty.Name} these property have matching name but no map has been defined");
            }
        }

        foreach (var customMap in mappingInfo.MapFromProperties)
        {
            mappings.MapFrom.Add($"{customMap.Destination} = source.{customMap.Source},");
        }

        return mappings;
    }
}

public class Mappings
{
    public List<string> MatchingByName { get; } = new();
    public List<string> MapFrom { get; } = new();
    public List<string> Excluded { get; } = new();
    public List<ComplexMappingInfo> ComplexMappingInfo { get; } = new();
}

public class ComplexMappingInfo
{
    public string Invocation { get; }
    public string Variable { get; }
    public string Destination { get; }
    public ComplexMappingInfo(string invocation, string variable, string destination)
    {
        Invocation = invocation;
        Variable = variable;
        Destination = destination;
    }
}