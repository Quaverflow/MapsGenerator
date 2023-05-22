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
        mapper.Map(person, "John", "London", out var personDto);

        Assert.Equal("John", personDto.FirstName);
        Assert.Equal("London", personDto.Address.City);
        Assert.Equal(person.Address.Street, personDto.Address.Street);
        Assert.Equal(person.Age, personDto.Age);
        Assert.Equal(person.Traits.Zodiac, personDto.Zodiac);
        Assert.Null(personDto.LastName);
    }
}