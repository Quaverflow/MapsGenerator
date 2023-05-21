using System.Collections.Immutable;
using System.Text;
using MapsGenerator.DTOs;
using MapsGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MapsGenerator;

[Generator]
public class MappingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("MapperBase.g.cs", SourceText.From(SourceGenerationHelper.Mapper, Encoding.UTF8));
            ctx.AddSource("MapperOptions.g.cs", SourceText.From(SourceGenerationHelper.MapperOptions, Encoding.UTF8));
        });

        var declarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => Filter.IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => Filter.GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // Combine the selected classes with the `Compilation`
        var compilationAndEnums
            = context.CompilationProvider.Combine(declarations.Collect());

        // Generate the source using the compilation and classes
        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctClasses = classes
            .OfType<ClassDeclarationSyntax>() //to avoid annoying nullability issues
            .Distinct()
            .ToArray();

        var profileDefinitions = new List<ProfileDefinition>();
        foreach (var classDeclarationSyntax in distinctClasses)
        {
            if (Filter.TryFindMapsInvocations(classDeclarationSyntax, out var maps))
            {
                var mapsInfo = maps.Select(x => SyntaxHelper.GetMappingInfo(x, compilation)).ToArray();
                profileDefinitions.Add(new ProfileDefinition(mapsInfo, classDeclarationSyntax));
            }
        }

        var (contract, implementation) = new MapsGeneratorSourceWriter(profileDefinitions, compilation).GenerateSource();
        context.AddSource("MapGenerator", implementation);
        context.AddSource("IMapGenerator", contract);
    }
}

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