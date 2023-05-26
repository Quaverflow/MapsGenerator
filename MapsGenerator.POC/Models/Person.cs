namespace MapsGenerator.POC.Models;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Lineage { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public Address Address { get; set; }
    public Traits Traits { get; set; }
}