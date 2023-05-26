namespace MapsGenerator.Test;

[UsesVerify]
public class UnitTest1
{
    [Fact]
    public Task GeneratesEnumExtensionsCorrectly()
    {
        // The source code to test
        var source = @"
using MapsGenerator;

namespace somenamespace
{
   
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public Address Address { get; set; }
    public Traits Traits { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class PersonDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public string Zodiac { get; set; }
    public AddressDto Address { get; set; }
}

public class AddressDto
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class Traits
{
    public string Zodiac { get; set; }
}


internal class PersonProfile : MapperBase
{
    public PersonProfile()
    {
        Map<Person, PersonDto>(x =>
        { 
            x.Exclude(y => y.LastName);
            //x.MapFrom(d => d.AddressDto, s => s.Address);
            x.MapFromParameter(d => d.Address.City);
            x.EnsureAllDestinationPropertiesAreMapped();
            
        });
        //Map<Address, AddressDto>();

    }
}}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}