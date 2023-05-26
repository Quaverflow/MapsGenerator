namespace MapsGenerator.DTOs;

public class ProfileMethodsInfo
{
    public ProfileMethodsInfo(string parameters, string documentation)
    {
        Parameters = parameters;
        Documentation = documentation;
    }

    public string Parameters { get; }
    public string Documentation { get; }
}