using MapsGenerator.ExternalAssembly.TestData;

namespace MapsGenerator.Tests.Common.Models.Source;

public class Person
{
    public  string FirstName { get; set; }
    public  string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public Address ExternalAddress { get; set; }
    public Address Address { get; set; }
}