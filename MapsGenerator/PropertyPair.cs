using Microsoft.CodeAnalysis;

namespace MapsGenerator;

public class PropertyPair
{
    public IPropertySymbol SourceProperty { get; set; }
    public IPropertySymbol DestinationProperty { get; set; }

    public PropertyPair(IPropertySymbol sourceProperty, IPropertySymbol destinationProperty)
    {
        SourceProperty = sourceProperty;
        DestinationProperty = destinationProperty;
    }
}