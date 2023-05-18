//HintName: MapperBase.g.cs

namespace MapsGenerator
{
    internal abstract classDeclarationSyntax MapperBase
    {
        protected void Map<TSource, TDestination>()
        {
        }   

        protected void Map<TSource, TDestination>(MapsGeneratorOptions<TSource, TDestination> options)
        {
        }
    }
}