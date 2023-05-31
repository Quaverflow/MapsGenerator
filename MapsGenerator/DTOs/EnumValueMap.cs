namespace MapsGenerator.DTOs;

public class EnumValueMap
{
    public EnumValueMap(string source, string destination, string sourceFullName, string destinationFullName)
    {
        Source = source;
        Destination = destination;
        SourceFullName = sourceFullName;
        DestinationFullName = destinationFullName;
    }

    public string Source { get; }
    public string Destination { get; }    
    public string SourceFullName { get; }
    public string DestinationFullName { get; }
}