using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;

namespace MapsGenerator;

public class TypeProperties
{
    public TypeProperties(IPropertySymbol[] properties, ITypeSymbol type)
    {
        Properties = properties;
        Type = type;
    }

    public IPropertySymbol[] Properties { get; }
    public ITypeSymbol Type { get; }
}
public class SourceWriterContext
{
    public List<ProfileDefinition> ProfileDefinitions { get; }
    public Compilation Compilation { get; }
    public List<MethodDefinition> MapMethodsDefinitions { get; } = new();
    public List<ProfileMethodsInfo> ProfileMethodsInfo { get; } = new();
    public Dictionary<string, TypeProperties> TypesProperties { get; } = new();

    public List<IPropertySymbol> CurrentNotMappedProperties { get; } = new();
    public ProfileDefinition CurrentProfile { get; set; } = null!;
    public MappingInfo CurrentMap { get; set; } = null!;
    public Mappings CurrentMappings { get; } = new();
    public Dictionary<string, ParametersRequiredByProperty> CurrentParametersRequiredFromProperties { get; } = new();

    public SourceWriterContext(List<ProfileDefinition> profileDefinitions, Compilation compilation)
    {
        ProfileDefinitions = profileDefinitions;
        Compilation = compilation;
    }

    public void Reset()
    {
        CurrentMappings.Reset();
        CurrentNotMappedProperties.Clear();
        CurrentParametersRequiredFromProperties.Clear();
        CurrentMap = null!;
        CurrentProfile = null!;
    }
}