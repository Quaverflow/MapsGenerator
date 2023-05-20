namespace MapsGenerator.DTOs;

public class Mappings
{
    public List<string> MatchingByName { get; } = new();
    public List<string> MapFrom { get; } = new();
    public List<string> Excluded { get; } = new();
    public List<ComplexMappingInfo> ComplexMappingInfo { get; } = new();
}