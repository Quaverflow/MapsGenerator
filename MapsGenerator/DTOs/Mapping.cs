namespace MapsGenerator.DTOs;

public class Mapping
{
    public List<string> MatchingByName { get; } = new();
    public List<string> MapFrom { get; } = new();
    public List<string> MapFromConstant { get; } = new();
    public List<string> LocalFunctions { get; } = new();
    public List<string> Excluded { get; } = new();
    public List<string> UnmappedProperties { get; } = new();

    public void Reset()
    {
        MatchingByName.Clear();
        MapFrom.Clear();
        LocalFunctions.Clear();
        Excluded.Clear();
        UnmappedProperties.Clear();
        MapFromConstant.Clear();
    }
}