using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.DTOs;

public class ProfileDefinition
{
    public ProfileInfo[] Maps { get; set; }
    public ClassDeclarationSyntax Profile { get; set; }

    public ProfileDefinition(ProfileInfo[] maps, ClassDeclarationSyntax profile)
    {
        Maps = maps;
        Profile = profile;
    }
}