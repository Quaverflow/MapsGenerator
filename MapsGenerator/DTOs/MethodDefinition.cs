namespace MapsGenerator.DTOs;

public class MethodDefinition
{
    public string Name { get; }
    public string ProfileDocumentation { get; }

    public MethodDefinition(string name, string profileDocumentation)
    {
        Name = name;
        ProfileDocumentation = profileDocumentation;
    }
}