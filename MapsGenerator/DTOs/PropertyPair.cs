using Microsoft.CodeAnalysis;

namespace MapsGenerator.DTOs;

public class PropertyPair
{
    public IPropertySymbol SourceProperty { get; }
    public PropertyPair DestinationProperty { get; }

    public PropertyPair(IPropertySymbol sourceProperty, IPropertySymbol destinationProperty)
    {
        SourceProperty = sourceProperty;
        DestinationProperty = destinationProperty;
    }
}