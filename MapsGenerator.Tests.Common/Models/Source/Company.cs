namespace MapsGenerator.Tests.Common.Models.Source;

public class Company
{
    public required string Name { get; set; }
    public required Employee[] Employees { get; set; }
    public required Address Address { get; set; }
}