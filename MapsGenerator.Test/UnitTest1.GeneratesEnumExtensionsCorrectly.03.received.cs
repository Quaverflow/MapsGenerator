//HintName: IMapGenerator.cs
namespace MapsGenerator
{
    public interface IMapGenerator
    {
        void Map(somenamespace.Person source, out somenamespace.PersonDto destination);
        bool TryMap(somenamespace.Person source, out somenamespace.PersonDto destination);
        void Map(somenamespace.Address source, out somenamespace.AddressDto destination);
        bool TryMap(somenamespace.Address source, out somenamespace.AddressDto destination);
    }
}
