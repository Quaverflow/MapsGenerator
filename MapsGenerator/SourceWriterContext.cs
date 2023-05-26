using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis;

namespace MapsGenerator;

public class SourceWriterContext
{
    public List<ProfileDefinition> ProfileDefinitions { get; }
    public Compilation Compilation { get; }
    public List<MethodDefinition> MapMethodsDefinitions { get; } = new();
    public List<ProfileMethodsInfo> ProfileMethodsInfo { get; } = new();
    public List<IPropertySymbol> NotMappedProperties { get; } = new();

    public ProfileDefinition CurrentProfile { get; set; } = null!;
    public MappingInfo CurrentMap { get; set; } = null!;
    public Mappings Mappings { get; } = new();

    public SourceWriterContext(List<ProfileDefinition> profileDefinitions, Compilation compilation)
    {
        ProfileDefinitions = profileDefinitions;
        Compilation = compilation;
    }

    public void Reset()
    {
        Mappings.Reset();
        NotMappedProperties.Clear();
        CurrentMap = null!;
        CurrentProfile = null!;
    }
}