namespace MapsGenerator.Helpers;

public static class SourceGenerationHelper
{
    public const string Mapper = @"
namespace MapsGenerator
{
    internal abstract class MapperBase
    {
        protected void Map<TSource, TDestination>()
        {
        }   

        protected void Map<TSource, TDestination>(Action<MapsGeneratorOptions<TSource, TDestination>> options)
        {
        }
    }
}";

    public const string MapperOptions = @"
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
        /// <param name=""destinationProperty""></param>
        /// <param name=""sourceProperty""></param>
        public void MapFrom<TSourceProperty, TDestinationProperty>(Func<TDestination, TDestinationProperty> destinationProperty, Func<TSource, TSourceProperty> sourceProperty)
        {
        }   
        
        /// <summary>
        /// Completely custom mapping. Will not attempt to match nested property if it's a complex object
        /// </summary>
        /// <typeparam name=""T""></typeparam>
        /// <param name=""destinationProperty""></param>
        /// <param name=""source""></param>
        public void CustomMap<T>(Action<TDestination> destinationProperty, T source)
        {
        }
    }
}";
}