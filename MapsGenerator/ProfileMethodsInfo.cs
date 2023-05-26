namespace MapsGenerator;

public class ProfileMethodsInfo
{
    public ProfileMethodsInfo(string[] methodNames, string documentation)
    {
        MethodNames = methodNames;
        Documentation = documentation;
    }

    public string[] MethodNames { get; }
    public string Documentation { get; }
}