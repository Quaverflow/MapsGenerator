namespace MapsGenerator.Tests.Common.Models.Destination;

public class CompanyDto
{
    public  string TradingName { get; set; }
    public  string Sector { get; set; }
    public  PersonDto[] Workers { get; set; }
    public  Dictionary<Guid, PersonDto> Bees { get; set; }
    public  AddressDto Address { get; set; }
}