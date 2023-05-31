using MapsGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace MapsGenerator.DTOs;

public class MappingInfo
{
    public TypeSyntax Source { get; }
    public TypeSyntax Destination { get; }
    public string SourceName { get; }
    public string DestinationName { get; }
    public string SourceFullName { get; }
    public string DestinationFullName { get; }
    public bool EnsureAllDestinationPropertiesAreMapped { get; }
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
        EnsureAllDestinationPropertiesAreMapped = GetEnsureAllDestinationPropertiesAreMapped();
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

    private bool GetEnsureAllDestinationPropertiesAreMapped()
    {
        
        if (InvocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        var argument = InvocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax body })
        {
            return false;
        }

        foreach (var statement in body.Statements)
        {
            if (statement is ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Name.Identifier.Text: "EnsureAllDestinationPropertiesAreMapped"
                        },
                        ArgumentList.Arguments.Count: 0
                    }
                })
            {
                return true;
            }
        }

        return false;
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
                var destinationPropertyName = destinationPropertyAccess.Name.Identifier.Text;

                mappedProperties.Add(new(sourceAccessName, destinationAccessName, destinationPropertyName));
            }
        }

        return mappedProperties;
    }

    private SimpleNameSyntax GetIdentifier(MemberAccessExpressionSyntax sourcePropertyAccess)
    {
        var exp = sourcePropertyAccess;
        while (exp.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            exp = memberAccessExpressionSyntax;
        }

        return exp.Name;
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

                var variableName = destinationAccessName.Replace(".", string.Empty).FirstCharToLower();
                var nestedPropertyQueue = new Queue<string>();
                foreach (var item in destinationAccessName.Split('.'))
                {
                    nestedPropertyQueue.Enqueue(item);
                }

                mappedProperties.Add(new PropertyInfo(destinationAccessName, returnType, variableName, nestedPropertyQueue));
            }
        }

        return mappedProperties;
    }

    private static string GetNestedMemberAccessName(MemberAccessExpressionSyntax memberAccess)
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