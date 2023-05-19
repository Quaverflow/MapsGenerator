//HintName: MapperBase.g.cs

namespace MapsGenerator
{
    internal abstract classDeclarationSyntax MapperBase
    {
        protected void Map<TSource, TDestination>()
        {
        }   

        protected void Map<TSource, TDestination>(Action<MapsGeneratorOptions<TSource, TDestination>> options)
        {
        }
    }
}