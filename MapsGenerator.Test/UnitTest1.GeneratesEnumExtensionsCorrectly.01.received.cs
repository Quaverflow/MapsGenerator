//HintName: MapperImplementation.cs
namespace MapsGenerator
{
    public classDeclarationSyntax MapperImplementation
    {
        public somenamespace.PersonDto Person_To_PersonDto(somenamespace.Person source)
        {
            return new somenamespace.PersonDto
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Age = source.Age,
            }
        }
    }
}
