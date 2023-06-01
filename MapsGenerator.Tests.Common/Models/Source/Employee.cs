namespace MapsGenerator.Tests.Common.Models.Source;

public class Employee
{
    public Guid Id { get; set; }
    public Person PersonalDetails { get; set; }
    public string Role { get; set; }
    public Seniority Seniority { get; set; }
}