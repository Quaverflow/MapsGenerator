namespace MapsGenerator.POC.Models;

public class PersonDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public string Zodiac { get; set; }
    public AddressDto Address { get; set; }
}