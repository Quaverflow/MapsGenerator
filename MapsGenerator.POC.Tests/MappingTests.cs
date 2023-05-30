
using AutoFixture;
using AutoMapper;
using MapsGenerator.Tests.Common;
using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.POC.Tests;

public class MappingTests
{
    private readonly IMapper _autoMapper;

    public MappingTests()
    {
        _autoMapper = new MapperConfiguration(x => x.AddProfile(new AutoMapperProfile())).CreateMapper();

    }

    [Fact]
    public void TestValidMapping()
    {
        var fixture = new Fixture();

        var company = fixture.Create<Company>();
        var result = _autoMapper.Map<CompanyDto>(company);

        Assert.Equal(company.Name, result.TradingName);

        Assert.Equal(company.Address.City, result.Address.City);
        Assert.Equal(company.Address.Street, result.Address.Street);

        foreach (var employee in result.Workers)
        {
            var matching = company.Employees.First(x => x.Id == employee.Id);

            Assert.Equal(matching.PersonalDetails.Address.City, employee.Address.City);
            Assert.Equal(matching.PersonalDetails.Address.Street, employee.Address.Street);
            Assert.Equal(matching.PersonalDetails.FirstName, employee.FirstName);
            Assert.Equal(matching.PersonalDetails.LastName, employee.LastName);
            Assert.Equal(matching.PersonalDetails.Age, employee.Age);
            Assert.Equal(matching.PersonalDetails.Height, employee.Height);
            if (matching.Seniority == Seniority.Junior)
            {
                Assert.Equal(SeniorityDto.Starter, employee.Seniority);
            }
            else
            {
                Assert.Equal(matching.Seniority.ToString(), employee.Seniority.ToString());
            }
            Assert.Equal(matching.Role, employee.Role);
        }
    }
}