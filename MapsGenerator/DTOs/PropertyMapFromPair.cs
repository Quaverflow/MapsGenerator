using Microsoft.CodeAnalysis;

namespace MapsGenerator.DTOs;

public class PropertyMapFromPair
{
    public string Source { get; }
    public string Destination { get; }
    public string DestinationSimpleName { get; }

    public PropertyMapFromPair(string source, string destination, string destinationSimpleName)
    {
        Source = source;
        Destination = destination;
        DestinationSimpleName = destinationSimpleName;
    }
}