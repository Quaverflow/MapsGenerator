//HintName: MapperBase.g.cs

namespace MapsGenerator
{
    internal abstract class MapperBase
    {
        protected readonly MapGenerator Mapper = new();

        protected void Map<TSource, TDestination>()
        {
        }   

        protected void Map<TSource, TDestination>(Action<MapsGeneratorOptions<TSource, TDestination>> options)
        {
        }
    }
}