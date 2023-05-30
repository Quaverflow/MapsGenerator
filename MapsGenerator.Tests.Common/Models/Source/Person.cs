namespace MapsGenerator.Tests.Common.Models.Source;

public class Person
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public required Address Address { get; set; }
}