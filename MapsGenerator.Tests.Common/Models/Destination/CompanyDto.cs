namespace MapsGenerator.Tests.Common.Models.Destination;

public class CompanyDto
{
    public  string TradingName { get; set; }
    public  string Sector { get; set; }
    public  PersonDto[] Workers { get; set; }
    public Queue<Guid> OrderIds { get; set; }
    public Stack<int> CarpetsIdsCollected { get; set; }
    public List<string> AlternativeNames { get; set; }
    public SortedSet<string> Aliases { get; set; }
    public HashSet<string> PetsNames { get; set; }
    public  Dictionary<Guid, PersonDto> Bees { get; set; }
    public  AddressDto Address { get; set; }
}