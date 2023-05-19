namespace MapsGenerator.POC;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public Address Address { get; set; }
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
    public AddressDto Address { get; set; }
}

public class AddressDto
{
    public string Street { get; set; }
    public string City { get; set; }
}

[Mapper]
internal class PersonProfile : MapperBase
{
    public PersonProfile()
    {
        Map<Person, PersonDto>(options =>
        {
            options.Exclude(x => x.FirstName)
        });
        Map<Address, AddressDto>();
    }
}