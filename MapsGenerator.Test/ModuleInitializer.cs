using System.Runtime.CompilerServices;

namespace MapsGenerator.Test;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}