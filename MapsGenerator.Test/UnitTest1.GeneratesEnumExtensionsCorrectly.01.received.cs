//HintName: MapperOptions.g.cs

namespace MapsGenerator
{

    internal class MapsGeneratorOptions<TSource, TDestination>
    {
        public void Exclude<TProperty>(Func<TDestination, TProperty> destinationProperty)
        {
        }

        public void MapFrom<TSourceProperty, TDestinationProperty>(Func<TDestination, TDestinationProperty> destinationProperty, Func<TSource, TSourceProperty> sourceProperty)
        {
        }   
        
        public void MapFromEnum<TSourceProperty, TDestinationProperty>(TDestinationProperty destinationProperty, TSourceProperty sourceProperty) 
            where TDestinationProperty : Enum where TSourceProperty : Enum
        {
        }   

        public void MapFromParameter<TProperty>(Func<TDestination, TProperty> destinationProperty)
        {
        }       

        public void EnsureAllDestinationPropertiesAreMapped()
        {
        }
    }
}