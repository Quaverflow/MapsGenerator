using System.Text;
using MapsGenerator.DTOs;
using MapsGenerator.Helpers;
using MapsGenerator.Helpers.Extensions;
using MapsGenerator.Helpers.MappingProviders;

namespace MapsGenerator;

public class MapsGeneratorSourceWriter
{
    private readonly SourceWriterContext _context;

    public MapsGeneratorSourceWriter(SourceWriterContext context)
    {
        _context = context;
        MappingProvider.GetAllTypeProperties(context);
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
        foreach (var definition in _context.MapMethodsDefinitions)
        {
            builder.AppendLine(definition.ProfileDocumentation, indent);
            builder.AppendLine(definition.Name, indent);
        }
    }

    private void AddNamespace(StringBuilder builder, int indent, Action<StringBuilder, int> addBody)
    {
        foreach (var usingStatement in _context.TypesProperties.Select(x =>
                     $"using {x.Value.Type.GetFullNamespace()};").Distinct())
        {
            builder.AppendLine(usingStatement, indent);
        }
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
        foreach (var currentProfile in _context.ProfileDefinitions)
        {
            foreach (var currentMap in currentProfile.Maps.Where(x => !x.IsEnum))
            {
                _context.Reset();
                _context.CurrentProfile = currentProfile;
                _context.CurrentMap = currentMap;
                AddClassMethodsDeclaration(builder, indent);
            }
        }

        builder.AppendLine("}", indent);
    }

    private void AddClassMethodsDeclaration(StringBuilder builder, int indent)
    {
        indent++;

        var profileMethodsInfo = new ProfileMethodsInfo(BuildDocumentation(_context.CurrentProfile));

        _context.ProfileMethodsInfo.Add(profileMethodsInfo);
        var source = $"{_context.CurrentMap.SourceFullName} source";
        var mapDeclaration = $"{_context.CurrentMap.DestinationFullName} Map<T>({source}) where T : {_context.CurrentMap.DestinationFullName}";
        var tryMapDeclaration = $"bool TryMap({source}, out {_context.CurrentMap.DestinationFullName}? destination, Action<Exception>? onError = null)";

        _context.MapMethodsDefinitions.Add(new MethodDefinition($"{mapDeclaration};", profileMethodsInfo.Documentation));
        _context.MapMethodsDefinitions.Add(new MethodDefinition($"{tryMapDeclaration};", profileMethodsInfo.Documentation));

        builder.AppendLine(profileMethodsInfo.Documentation, indent);
        builder.AppendLine($"public {mapDeclaration}", indent);
        builder.AppendLine("{", indent);
        AddMapMethodBody(builder, indent);
        builder.AppendLine("}", indent);

        builder.AppendLine(profileMethodsInfo.Documentation, indent);
        builder.AppendLine($"public {tryMapDeclaration}", indent);
        builder.AppendLine("{", indent);
        AddTryMethodBody(builder, indent);
        builder.AppendLine("}", indent);
    }

    private static string BuildDocumentation(ProfileDefinition definition)
    {
        var profileName = SyntaxHelper.GetTypeSyntaxFullName(definition.Profile);
        var profileDocumentation = @$"
/// <summary>
/// Profile <see cref=""{profileName}""/>
/// </summary>";
        return profileDocumentation;
    }

    private void AddTryMethodBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("try", indent);
        builder.AppendLine("{", indent);
        AddTryBody(builder, indent);
        builder.AppendLine("}", indent);
        builder.AppendLine("catch(Exception e)", indent);
        builder.AppendLine("{", indent);
        AddCatchBody(builder, indent);
        builder.AppendLine("}", indent);

    }

    private void AddTryBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine($"destination = Map<{_context.CurrentMap.DestinationFullName}>(source);", indent);
        builder.AppendLine("return true;", indent);
    }

    private static void AddCatchBody(StringBuilder builder, int indent)
    {
        indent++;
        builder.AppendLine("destination = null;", indent);
        builder.AppendLine("onError?.Invoke(e);", indent);
        builder.AppendLine("return false;", indent);
    }

    private void AddMapMethodBody(StringBuilder builder, int indent)
    {
        indent++;
        MappingProvider.GetMappings(_context);

        builder.AppendLine($"return new {_context.CurrentMap.DestinationFullName}", indent);
        builder.AppendLine("{", indent);
        AddClassInitializationBody(builder, _context.CurrentMappings, indent);
        builder.AppendLine("};", indent);

        foreach (var function in _context.CurrentMappings.LocalFunctions)
        {
            builder.AppendLine(function, indent);
        }
    }

    private static void AddClassInitializationBody(StringBuilder builder, Mapping mappings, int indent)
    {
        indent++;
        foreach (var matchingByName in mappings.MatchingByName)
        {
            builder.AppendLine(matchingByName, indent);
        }

        foreach (var mapFrom in mappings.MapFrom)
        {
            builder.AppendLine(mapFrom, indent);
        }

        foreach (var mapFrom in mappings.UnmappedProperties)
        {
            builder.AppendLine(mapFrom, indent);
        }

        foreach (var excludedProperty in mappings.Excluded)
        {
            builder.AppendLine(excludedProperty, indent);
        }
    }
}
