using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common.Models.Destination;

public class PersonDto
{
    public Guid Id { get; set; }
    public  string FirstName { get; set; }
    public  string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public  AddressDto Address { get; set; }
    public  string Role { get; set; }
    public SeniorityDto Seniority { get; set; }
}