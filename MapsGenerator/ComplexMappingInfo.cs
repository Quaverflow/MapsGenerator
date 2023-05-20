namespace MapsGenerator;

public class ComplexMappingInfo
{
    public string Invocation { get; }
    public string Variable { get; }
    public string Destination { get; }
    public ComplexMappingInfo(string invocation, string variable, string destination)
    {
        Invocation = invocation;
        Variable = variable;
        Destination = destination;
    }
}