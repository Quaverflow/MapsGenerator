using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common.Models.Destination;

public class PersonDto
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public required AddressDto Address { get; set; }
    public required string Role { get; set; }
    public SeniorityDto Seniority { get; set; }
}