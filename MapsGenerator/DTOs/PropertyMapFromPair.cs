namespace MapsGenerator.DTOs;

public class PropertyMapFromPair
{
    public string Source { get; }
    public string Destination { get; }
    public string DestinationSimpleName { get; }
    public bool IsExpressionMap { get; }
    public string LambdaIdentifier { get; }

    public PropertyMapFromPair(string source, string destination, string destinationSimpleName, string lambdaIdentifier)
    {
        Source = source;
        Destination = destination;
        DestinationSimpleName = destinationSimpleName;
        LambdaIdentifier = lambdaIdentifier;
        IsExpressionMap = true;
    }
    public PropertyMapFromPair(string source, string destination, string destinationSimpleName)
    {
        Source = source;
        Destination = destination;
        DestinationSimpleName = destinationSimpleName;
        IsExpressionMap = false;
        LambdaIdentifier = string.Empty;
    }
}
