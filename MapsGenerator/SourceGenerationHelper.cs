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
}";
}