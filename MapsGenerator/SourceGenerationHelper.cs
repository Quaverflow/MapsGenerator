namespace MapsGenerator;

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
        public void Exclude(Action<TDestination> destinationProperty)
        {
        }

        /// <summary>
        /// For properties with mismatching names
        /// </summary>
        /// <typeparam name=""T""></typeparam>
        /// <param name=""sourceProperty""></param>
        /// <param name=""destinationProperty""></param>
        public void MapFrom(Action<TDestination> sourceProperty, Action<TDestination> destinationProperty)
        {
        }   
        
        /// <summary>
        /// Completely custom mapping. Will not attempt to match nested property if it's a complex object
        /// </summary>
        /// <typeparam name=""T""></typeparam>
        /// <param name=""source""></param>
        /// <param name=""destinationProperty""></param>
        public void CustomMap<T>(T source, Action<TDestination> destinationProperty)
        {
        }
    }
}";
}