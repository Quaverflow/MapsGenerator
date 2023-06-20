using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.DTOs;

public class ProfileDefinition
{
    public MapInfo[] Maps { get; set; }
    public ClassDeclarationSyntax Profile { get; set; }

    public ProfileDefinition(MapInfo[] maps, ClassDeclarationSyntax profile)
    {
        Maps = maps;
        Profile = profile;
    }
}