using MapsGenerator.ExternalAssembly.TestData;

namespace MapsGenerator.Tests.Common.Models.Destination;

public class PersonDto
{
    public required string IP { get; set; }
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public  string? LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public required AddressDto Address { get; set; }
    public required ExternalAssemblyAddressDto ExternalAddress { get; set; }
    public required string Role { get; set; }
    public SeniorityDto Seniority { get; set; }
}