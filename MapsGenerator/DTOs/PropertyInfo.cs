namespace MapsGenerator.DTOs;

public class PropertyInfo
{
    public string Name { get; }
    public string VariableName { get; }
    public string Type { get; }
    public Queue<string> NestedPropertyInvocationQueue { get; }
    public Queue<string> NestedPropertyDefinitionQueue { get; }

    public PropertyInfo(string name, string type, string variableName, Queue<string> nestedPropertyInvocationQueue)
    {
        Name = name;
        Type = type;
        VariableName = variableName;
        NestedPropertyInvocationQueue = nestedPropertyInvocationQueue;
        NestedPropertyDefinitionQueue = nestedPropertyInvocationQueue;
    }
}