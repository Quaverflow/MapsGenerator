using MapsGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.DTOs;

public class MappingInfo
{
    public TypeSyntax Source { get; }
    public TypeSyntax Destination { get; }
    public string SourceName { get; }
    public string DestinationName { get; }
    public string SourceFullName { get; }
    public string DestinationFullName { get; }
    public InvocationExpressionSyntax InvocationExpressionSyntax { get; }
    public List<string> ExcludedProperties { get; }
    public List<PropertyMapFromPair> MapFromProperties { get; }
    public List<PropertyInfo> MapFromParameterProperties { get; }

    public MappingInfo(TypeSyntax source, TypeSyntax destination, string sourceName, string destinationName,
        string sourceFullName, string destinationFullName, InvocationExpressionSyntax invocationExpressionSyntax,
        Compilation compilation)
    {
        Source = source;
        Destination = destination;
        SourceName = sourceName;
        DestinationName = destinationName;
        SourceFullName = sourceFullName;
        DestinationFullName = destinationFullName;
        InvocationExpressionSyntax = invocationExpressionSyntax;
        ExcludedProperties = GetExcludedProperties();
        MapFromProperties = GetMapFromProperties();
        MapFromParameterProperties = GetMapFromParameterProperties(compilation);
    }

    private List<string> GetExcludedProperties()
    {
        var excludedProperties = new List<string>();
        if (InvocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return excludedProperties;
        }

        var argument = InvocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax body })
        {
            return excludedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Name.Identifier.Text: "Exclude"
                        },
                        ArgumentList.Arguments.Count: 1
                    } expression
                })
            {
                continue;
            }

            if (expression.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax
                {
                    Body: MemberAccessExpressionSyntax propertyAccess
                })
            {
                excludedProperties.Add(propertyAccess.Name.Identifier.Text);
            }
        }

        return excludedProperties;
    }

    private List<PropertyMapFromPair> GetMapFromProperties()
    {
        var mappedProperties = new List<PropertyMapFromPair>();
        if (InvocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return mappedProperties;
        }

        var argument = InvocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax body })
        {
            return mappedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Name.Identifier.Text: "MapFrom"
                        },
                        ArgumentList.Arguments.Count: 2
                    } expression
                })
            {
                continue;
            }

            if (expression.ArgumentList.Arguments[1].Expression is SimpleLambdaExpressionSyntax
                {
                    Body: MemberAccessExpressionSyntax sourcePropertyAccess
                }
                && expression.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax
                {
                    Body: MemberAccessExpressionSyntax destinationPropertyAccess
                })
            {
                var sourceAccessName = GetNestedMemberAccessName(sourcePropertyAccess);
                var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);

                mappedProperties.Add(new(sourceAccessName, destinationAccessName));
            }
        }

        return mappedProperties;
    }

    private List<PropertyInfo> GetMapFromParameterProperties(Compilation compilation)
    {
        var mappedProperties = new List<PropertyInfo>();
        if (InvocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return mappedProperties;
        }

        var argument = InvocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax body })
        {
            return mappedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Name.Identifier.Text: "MapFromParameter"
                        },
                        ArgumentList.Arguments.Count: 1
                    } expression
                })
            {
                continue;
            }

            if (expression.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax
                {
                    Body: MemberAccessExpressionSyntax destinationPropertyAccess
                })
            {
                var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);
                var innerExpression = expression.ArgumentList.Arguments[0].Expression;
                var syntaxTree = innerExpression.SyntaxTree;
                var returnType = (compilation.GetSemanticModel(syntaxTree)
                        .GetSymbolInfo(innerExpression).Symbol as IMethodSymbol)?.ReturnType
                    .ToString();
                if (returnType == null)
                {
                    throw new InvalidOperationException("Not a valid symbol");
                }
                //todo protect from variable name duplication
                mappedProperties.Add(new PropertyInfo(destinationAccessName, returnType, destinationAccessName.FirstCharToLower()));
            }
        }

        return mappedProperties;
    }

    private string GetNestedMemberAccessName(MemberAccessExpressionSyntax memberAccess)
    {
        var name = memberAccess.Name.Identifier.Text;

        return memberAccess.Expression switch
        {
            IdentifierNameSyntax => name,
            MemberAccessExpressionSyntax nestedMemberAccess => GetNestedMemberAccessName(nestedMemberAccess) + "." + name,
            _ => throw new ArgumentException("Unexpected expression type in member access chain.")
        };
    }
}