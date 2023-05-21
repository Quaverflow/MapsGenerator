//HintName: MapperOptions.g.cs

namespace MapsGenerator
{
    internal class MapsGeneratorOptions<TSource, TDestination>
    {
        public void Exclude<TProperty>(Func<TDestination, TProperty> destinationProperty)
        {
        }

        /// <summary>
        /// For properties with mismatching names. Will attempt to match properties by name if it is a complex object
        /// </summary>
        /// <param name="destinationProperty"></param>
        /// <param name="sourceProperty"></param>
        public void MapFrom<TSourceProperty, TDestinationProperty>(Func<TDestination, TDestinationProperty> destinationProperty, Func<TSource, TSourceProperty> sourceProperty)
        {
        }   
        
        /// <summary>
        /// Will add a parameter to the map method with the same name and type of the destination property,
        /// </summary>
        /// <param name="destinationProperty"></param>
        public void MapFromParameter<TProperty>(Func<TDestination, TProperty> destinationProperty)
        {
        }
    }
}