namespace MapsGenerator.Tests.Common.Models.Destination;

public class CompanyDto
{
    public required string TradingName { get; set; }
    public required string Sector { get; set; }
    public required PersonDto[] Workers { get; set; }
    public required AddressDto Address { get; set; }
}