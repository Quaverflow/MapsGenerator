namespace MapsGenerator.DTOs;

public class PropertyMapFromPair
{
    public string Source { get; }
    public string Destination { get; }

    public PropertyMapFromPair(string source, string destination)
    {
        Source = source;
        Destination = destination;
    }
}