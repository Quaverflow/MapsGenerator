namespace MapsGenerator.POC.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var person = new Person
        {
            FirstName = "abc",
            LastName ="cede",
            Age = 10,
            Address = new Address
            {
                Street = "adfawe",
                City = "ffff"
            }
        };
        var mapper = new MapsGenerator.MapperImplementation();
        var personDto = mapper.Person_To_MapsGeneratorPOCPersonDto(person);

        Assert.Equal(person.FirstName, personDto.FirstName);
        Assert.Equal(person.Address.City, personDto.Address.City);
        Assert.Equal(person.Address.Street, personDto.Address.Street);
        Assert.Equal(person.Age, personDto.Age);
        Assert.Null(personDto.LastName);
    }
}