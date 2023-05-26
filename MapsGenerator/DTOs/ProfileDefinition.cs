using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.DTOs;

public class ProfileDefinition
{
    public MappingInfo[] Maps { get; set; }
    public ClassDeclarationSyntax Profile { get; set; }

    public ProfileDefinition(MappingInfo[] maps, ClassDeclarationSyntax profile)
    {
        Maps = maps;
        Profile = profile;
    }
}