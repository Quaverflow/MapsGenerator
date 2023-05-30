namespace MapsGenerator.Tests.Common.Models.Source;

public class Employee
{
    public Guid Id { get; set; }
    public required Person PersonalDetails { get; set; }
    public required string Role { get; set; }
    public Seniority Seniority { get; set; }
}