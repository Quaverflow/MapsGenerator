using MapsGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    public bool EnsureAllDestinationPropertiesAreMapped { get; }
    public bool IsEnum { get; }
    public InvocationExpressionSyntax InvocationExpressionSyntax { get; }
    public List<string> ExcludedProperties { get; }
    public List<PropertyMapFromPair> MapFromProperties { get; }
    public List<EnumValueMap> MapFromEnums { get; }
    public List<PropertyInfo> MapFromParameterProperties { get; }

    public MappingInfo(TypeSyntax source, TypeSyntax destination, string sourceName, string destinationName,
        string sourceFullName, string destinationFullName, InvocationExpressionSyntax invocationExpressionSyntax,
        Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(destination.SyntaxTree);
        var type = semanticModel.GetSymbolInfo(destination).Symbol as INamedTypeSymbol;
        IsEnum = type?.TypeKind is TypeKind.Enum;
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
        MapFromEnums = GetMapFromEnums();
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

    private bool CheckIsValidMapFrom(out BlockSyntax? body)
    {
        body = null;
        if (InvocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }
        var argument = InvocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is not SimpleLambdaExpressionSyntax { Body: BlockSyntax blockSyntax })
        {
            return false;
        }

        body = blockSyntax;
        return true;
    }

    private static bool IsTargetForMapFrom(StatementSyntax? statement, string expressionName, int argsCount, out InvocationExpressionSyntax? expression)
    {
        expression = null;
        if (statement is not ExpressionStatementSyntax
            {
                Expression: InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax innerExpression,
                } validExpression
            })
        {
            return false;
        }
        expression = validExpression;

        return expression.ArgumentList.Arguments.Count == argsCount ||
               innerExpression.Name.Identifier.Text == expressionName;
    }

    private List<EnumValueMap> GetMapFromEnums()
    {
        var mapFromEnums = new List<EnumValueMap>();
        if (!CheckIsValidMapFrom(out var body) || body == null)
        {
            return mapFromEnums;
        }

        foreach (var statement in body.Statements)
        {
            if (!IsTargetForMapFrom(statement, "MapFromEnum", 2, out var expression))
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
    private List<PropertyMapFromPair> GetMapFromProperties()
    {
        var mappedProperties = new List<PropertyMapFromPair>();

        if (!CheckIsValidMapFrom(out var body) || body == null)
        {
            return mappedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (!IsTargetForMapFrom(statement, "MapFrom", 2, out var expression))
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
                else if (expression.ArgumentList.Arguments[1].Expression is SimpleLambdaExpressionSyntax
                         {
                             Body: InvocationExpressionSyntax invocationExpression
                         })
                {
                    AddExpressionBodySource(destinationPropertyAccess, invocationExpression.ToString(), mappedProperties);
                }
                else if(expression.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax
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
            }
        }

        return mappedProperties;
    }

    private static void AddExpressionBodySource(MemberAccessExpressionSyntax destinationPropertyAccess,
        string invocationExpression, List<PropertyMapFromPair> mappedProperties, string identifier = "")
    {
        var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);
        var source = invocationExpression.Substring(invocationExpression.IndexOf('.') + 1);
        var destinationPropertyName = destinationPropertyAccess.Name.Identifier.Text;

        mappedProperties.Add(new PropertyMapFromPair(source, destinationAccessName, destinationPropertyName, identifier));
    }
    
    private static void AddBlockBodySource(MemberAccessExpressionSyntax destinationPropertyAccess,
        string invocationExpression, List<PropertyMapFromPair> mappedProperties, string identifier = "")
    {
        var destinationAccessName = GetNestedMemberAccessName(destinationPropertyAccess);
        var destinationPropertyName = destinationPropertyAccess.Name.Identifier.Text;

        mappedProperties.Add(new PropertyMapFromPair(invocationExpression, destinationAccessName, destinationPropertyName, identifier));
    }

    private List<PropertyInfo> GetMapFromParameterProperties(Compilation compilation)
    {
        var mappedProperties = new List<PropertyInfo>();
        if (!CheckIsValidMapFrom(out var body) || body == null)
        {
            return mappedProperties;
        }

        foreach (var statement in body.Statements)
        {
            if (!IsTargetForMapFrom(statement, "MapFromParameter", 1, out var expression))
            {
                continue;
            }

            if (expression?.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax
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