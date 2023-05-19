using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapsGenerator;

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
    public string MappingName { get; }

    public MappingInfo(TypeSyntax source, TypeSyntax destination, string sourceName, string destinationName,
        string sourceFullName, string destinationFullName, InvocationExpressionSyntax invocationExpressionSyntax)
    {
        Source = source;
        Destination = destination;
        SourceName = sourceName;
        DestinationName = destinationName;
        SourceFullName = sourceFullName;
        DestinationFullName = destinationFullName;
        InvocationExpressionSyntax = invocationExpressionSyntax;
        MappingName = $"{SourceName}_To_{DestinationFullName.Replace(".", string.Empty)}";
        ExcludedProperties = GetExcludedProperties();
        MapFromProperties = GetMapFromProperties();
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

public class PropertyMapFromPair
{
    public string Source { get; set; }
    public string Destination { get; set; }

    public PropertyMapFromPair(string source, string destination)
    {
        Source = source;
        Destination = destination;
    }
}
