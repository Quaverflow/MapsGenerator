using Microsoft.CodeAnalysis;

namespace MapsGenerator;

public class ParametersRequiredByProperty
{
    public ParametersRequiredByProperty( IPropertySymbol property, Dictionary<string, string> parametersByVariableName)
    {
        Property = property;
        ParametersByVariableName = parametersByVariableName;
    }

    public Dictionary<string, string> ParametersByVariableName { get; }
    public IPropertySymbol Property { get; }
}