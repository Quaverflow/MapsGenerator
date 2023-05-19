//HintName: MapperOptions.g.cs

namespace MapsGenerator
{
    internal class MapsGeneratorOptions<TSource, TDestination>
    {
        public void Exclude<TProperty>(Func<TDestination, TProperty> destinationProperty)
        {
        }

        /// <summary>
        /// For properties with mismatching names
        /// </summary>
        /// <param name="destinationProperty"></param>
        /// <param name="sourceProperty"></param>
        public void MapFrom<TSourceProperty, TDestinationProperty>(Func<TDestination, TDestinationProperty> destinationProperty, Func<TSource, TSourceProperty> sourceProperty)
        {
        }   
        
        /// <summary>
        /// Completely custom mapping. Will not attempt to match nested property if it's a complex object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destinationProperty"></param>
        /// <param name="source"></param>
        public void CustomMap<T>(Action<TDestination> destinationProperty, T source)
        {
        }
    }
}