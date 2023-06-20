namespace MapsGenerator.Tests.Common.Models.Source;

public class Company
{
    public  string Name { get; set; }
    public Employee[] Employees { get; set; }
    public List<Guid> OrderIds { get; set; }
    public Queue<int> CarpetsIdsCollected { get; set; }
    public SortedSet<string> AlternativeNames { get; set; }
    public HashSet<string> Aliases { get; set; }
    public Stack<string> PetsNames { get; set; }
    public  Address Address { get; set; }
}