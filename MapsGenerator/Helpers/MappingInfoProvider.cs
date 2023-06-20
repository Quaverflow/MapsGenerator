using MapsGenerator.DTOs;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator.Helpers;

public static class MappingInfoProvider
{
    private static bool IsMarchingExpression(StatementSyntax statement, string name, int arguments, out InvocationExpressionSyntax invocationExpression)
    {
        if (statement is ExpressionStatementSyntax
            {
                Expression: InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax innerExpression
                } expression
            })
        {
            if (innerExpression.Name.Identifier.Text == name && expression.ArgumentList.Arguments.Count == arguments)
            {
                invocationExpression = expression;
                return true;
            }
        }

        invocationExpression = null!;
        return false;
    }

    public static List<string> GetExcludedProperties(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        var excludedProperties = new List<string>();
        if (invocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return excludedProperties;
        }

        var argument = invocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax body })
        {
            return excludedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (!IsMarchingExpression(statement, "Exclude", 1, out var expression))
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

    public static bool GetEnsureAllDestinationPropertiesAreMapped(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        if (invocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        var argument = invocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax body })
        {
            return false;
        }

        return body.Statements.Any(statement => 
                !IsMarchingExpression(statement, "EnsureAllDestinationPropertiesAreMapped", 0, out _));
    }

    private static bool CheckIsValidMapFrom(InvocationExpressionSyntax invocationExpressionSyntax, out BlockSyntax? body)
    {
        body = null;
        if (invocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }
        var argument = invocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax blockSyntax })
        {
            return false;
        }

        body = blockSyntax;
        return true;
    }

    public static List<EnumValueMap> GetMapFromEnums(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        var mapFromEnums = new List<EnumValueMap>();
        if (!CheckIsValidMapFrom(invocationExpressionSyntax, out var body) || body == null)
        {
            return mapFromEnums;
        }

        foreach (var statement in body.Statements)
        {
            if (!IsMarchingExpression(statement, "MapFromEnum", 2, out var expression))
            {
                continue;
            }

            if (expression?.ArgumentList.Arguments[1].Expression is MemberAccessExpressionSyntax sourcePropertyAccess
                && expression.ArgumentList.Arguments[0].Expression is MemberAccessExpressionSyntax destinationPropertyAccess)
            {
                var sourceAccessName = GetNestedMemberAccessName(sourcePropertyAccess);
                var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);

                mapFromEnums.Add(new(sourceAccessName, destinationAccessName, expression.ArgumentList.Arguments[1].ToString(), expression.ArgumentList.Arguments[0].ToString()));
            }
        }

        return mapFromEnums;
    }

    public static List<PropertyMapFromPair> GetMapFromProperties(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        var mappedProperties = new List<PropertyMapFromPair>();

        if (!CheckIsValidMapFrom(invocationExpressionSyntax, out var body) || body == null)
        {
            return mappedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (!IsMarchingExpression(statement, "MapFrom", 2, out var expression))
            {
                continue;
            }

            if (expression?.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax
                {
                    Body: MemberAccessExpressionSyntax destinationPropertyAccess
                })
            {
                if (expression.ArgumentList.Arguments[1].Expression is SimpleLambdaExpressionSyntax
                    {
                        Body: MemberAccessExpressionSyntax sourcePropertyAccess
                    })
                {
                    var sourceAccessName = GetNestedMemberAccessName(sourcePropertyAccess);
                    var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);
                    var destinationPropertyName = destinationPropertyAccess.Name.Identifier.Text;

                    mappedProperties.Add(new(sourceAccessName, destinationAccessName, destinationPropertyName));
                }
                else if (expression.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax
                {
                    Body: BlockSyntax innerExpressionBody
                })
                {
                    var parameterIdentifier = expression
                        .ArgumentList.Arguments[1].Expression
                        .DescendantNodes()
                        .OfType<ParameterSyntax>()
                        .FirstOrDefault()
                        ?.Identifier.ValueText;

                    AddBlockBodySource(destinationPropertyAccess, innerExpressionBody.ToString(), mappedProperties, parameterIdentifier ?? throw new InvalidOperationException());
                }
                else if (expression.ArgumentList.Arguments[1].Expression is SimpleLambdaExpressionSyntax invocationExpression)
                {
                    var parameterIdentifier = expression
                        .ArgumentList.Arguments[1].Expression
                        .DescendantNodes()
                        .OfType<ParameterSyntax>()
                        .FirstOrDefault()
                        ?.Identifier.ValueText;

                    AddExpressionBodySource(destinationPropertyAccess, invocationExpression.ToString(), mappedProperties, parameterIdentifier ?? throw new InvalidOperationException());
                }
            }
        }

        return mappedProperties;
    }

    public static List<PropertyMapFromConstant> GetMapFromConstantProperties(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        var mappedProperties = new List<PropertyMapFromConstant>();

        if (!CheckIsValidMapFrom(invocationExpressionSyntax, out var body) || body == null)
        {
            return mappedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (!IsMarchingExpression(statement, "MapFromConstantValue", 2, out var expression))
            {
                continue;
            }

            if (expression?.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax
                {
                    Body: MemberAccessExpressionSyntax destinationPropertyAccess
                } && expression.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax constantValue)
            {
                var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);

                mappedProperties.Add(new(destinationAccessName, constantValue.ToString()));

            }
        }

        return mappedProperties;
    }

    private static void AddExpressionBodySource(MemberAccessExpressionSyntax destinationPropertyAccess,
        string invocationExpression, List<PropertyMapFromPair> mappedProperties, string identifier = "")
    {
        var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);
        var methodBody = invocationExpression.Substring(invocationExpression.IndexOf('.') + 1);
        var source = $"=> {identifier}.{methodBody};";
        var destinationPropertyName = destinationPropertyAccess.Name.Identifier.Text;

        mappedProperties.Add(new PropertyMapFromPair(source, destinationAccessName, destinationPropertyName, identifier));
    }

    public static void AddBlockBodySource(MemberAccessExpressionSyntax destinationPropertyAccess,
        string invocationExpression, List<PropertyMapFromPair> mappedProperties, string identifier = "")
    {
        var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);
        var destinationPropertyName = destinationPropertyAccess.Name.Identifier.Text;

        mappedProperties.Add(new PropertyMapFromPair(invocationExpression, destinationAccessName, destinationPropertyName, identifier));
    }

    public static string GetNestedMemberAccessName(MemberAccessExpressionSyntax memberAccess)
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