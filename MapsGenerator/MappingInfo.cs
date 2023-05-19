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
    }

    private List<string> GetExcludedProperties()
    {
        var excludedProperties = new List<string>();
        if (InvocationExpressionSyntax.ArgumentList.Arguments.Count != 1)
        {
            return excludedProperties;
        }

        var argument = InvocationExpressionSyntax.ArgumentList.Arguments[0];
        if (argument.Expression is SimpleLambdaExpressionSyntax lambda)
        {
            if (lambda.Body is not BlockSyntax body)
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

        }

        return excludedProperties;
    }
}