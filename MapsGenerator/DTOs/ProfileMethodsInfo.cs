namespace MapsGenerator.DTOs;

public class ProfileMethodsInfo
{
    public ProfileMethodsInfo(string documentation)
    {
        Documentation = documentation;
    }

    public string Documentation { get; }
}