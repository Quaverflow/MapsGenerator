using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.Helpers;

public static class Filter
{
    //predicate. Should be fast and simple in order to avoid slowing down the IDE
    public static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 };

    //transform. Second layer of filtering, should still prefer performance over everything.
    public static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        return IsMapperChild(classDeclarationSyntax) ? classDeclarationSyntax : null;
    }

    private static bool IsMapperChild(BaseTypeDeclarationSyntax classDeclarationSyntax)
    {
        if (classDeclarationSyntax.BaseList?.Types is null)
        {
            return false;
        }

        //no linq for performance.
        foreach (var item in classDeclarationSyntax.BaseList.Types)
        {
            if (item.Type is IdentifierNameSyntax { Identifier.Text: "MapperBase" })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// tries to find all defined maps.
    /// </summary>
    /// <param name="classDeclaration"></param>
    /// <param name="mapInvocations"></param>
    /// <returns></returns>
    public static bool TryFindMapsInvocations(ClassDeclarationSyntax classDeclaration, out InvocationExpressionSyntax[] mapInvocations)
    {
        var constructor = GetParameterlessConstructor(classDeclaration);
        if (constructor?.Body == null)
        {
            mapInvocations = Array.Empty<InvocationExpressionSyntax>();
            return false;
        }

        mapInvocations = constructor.Body.Statements
            .Select(x => (x as ExpressionStatementSyntax)?.Expression)
            .Where(FilterMapInvocations)
            .OfType<InvocationExpressionSyntax>()
            .ToArray();

        return true;
    }

    /// <summary>
    /// Verify name, parameters and number of type parameters.
    /// </summary>
    /// <param name="expressionSyntax"></param>
    /// <returns></returns>
    private static bool FilterMapInvocations(ExpressionSyntax? expressionSyntax) 
        => expressionSyntax is InvocationExpressionSyntax
        {
            Expression: GenericNameSyntax
            {
                Identifier.Text: "Map",
                TypeArgumentList.Arguments.Count: 2
            },
            ArgumentList.Arguments.Count: 0 or 1
        };

    private static ConstructorDeclarationSyntax? GetParameterlessConstructor(TypeDeclarationSyntax classDeclaration)
    => classDeclaration.Members
        .OfType<ConstructorDeclarationSyntax>()
        .FirstOrDefault(x => !x.ParameterList.Parameters.Any());

}