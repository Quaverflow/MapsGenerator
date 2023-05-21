namespace MapsGenerator.DTOs;

public class PropertyInfo
{
    public string Name { get; }
    public string VariableName { get; }
    public string Type { get; }
    public PropertyInfo(string name, string type, string variableName)
    {
        Name = name;
        Type = type;
        VariableName = variableName;
    }
}