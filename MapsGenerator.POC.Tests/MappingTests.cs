using MapsGenerator.POC.Models;

namespace MapsGenerator.POC.Tests;

public class MappingTests
{
    [Fact]
    public void TestValidMapping()
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
            },
            Traits = new Traits
            {
                Zodiac = "sotk"
            }
        };
        var mapper = new MapGenerator();
        mapper.Map(person, out var personDto);

        Assert.Equal(person.FirstName, personDto.FirstName);
        Assert.Equal(person.Address.City, personDto.Address.City);
        Assert.Equal(person.Address.Street, personDto.Address.Street);
        Assert.Equal(person.Age, personDto.Age);
        Assert.Equal(person.Traits.Zodiac, personDto.Zodiac);
        Assert.Null(personDto.LastName);
    }
}