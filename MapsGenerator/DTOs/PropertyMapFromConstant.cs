namespace MapsGenerator.DTOs;

public class PropertyMapFromConstant
{
    public PropertyMapFromConstant(string destination, string value)
    {
        Destination = destination;
        Value = value;
    }

    public string Destination { get; }
    public string Value { get; }
}