namespace MapsGenerator;

public class PropertyMapFromPair
{
    public string Source { get; set; }
    public string Destination { get; set; }

    public PropertyMapFromPair(string source, string destination)
    {
        Source = source;
        Destination = destination;
    }
}