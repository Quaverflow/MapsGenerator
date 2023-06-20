
using AutoFixture;
using AutoMapper;
using MapsGenerator.Tests.Common;
using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;
using System.Linq;

namespace MapsGenerator.POC.Tests;

public class MappingTests
{
    private readonly IMapper _autoMapper;
    private readonly Company _company;
    private readonly MapGenerator _generator;

    public MappingTests()
    {
        _autoMapper = new MapperConfiguration(x => x.AddProfile(new AutoMapperProfile())).CreateMapper();
        _generator = new MapGenerator();
        
        var fixture = new Fixture();

        _company = fixture.Create<Company>();
    }

    [Fact]
    public void AutoMapper_Test()
    {
        var result = _autoMapper.Map<CompanyDto>(_company);
        AssertCompanyMap(_company, result);
    }
    
    [Fact]
    public void Generator_Test()
    {
        var result =_generator.Map<CompanyDto>(_company);
        AssertCompanyMap(_company, result);
    }

    private static void AssertCompanyMap(Company company, CompanyDto result)
    {
        Assert.Equal(company.Name, result.TradingName);

        Assert.Equal(company.Address.City, result.Address.City);
        Assert.Equal(company.Address.Street, result.Address.Street);

        AssertEmployeesToPersonDto(company.Employees, result.Workers);

        var ids = result.Bees.Keys.Order().ToArray();
        var orderIds = result.OrderIds.Order().ToArray();
        var aliases = result.Aliases.Order().ToArray();
        var alternativeNames = result.AlternativeNames.Order().ToArray();
        var carpetsIdsCollected = result.CarpetsIdsCollected.Order().ToArray();
        var petsNames = result.PetsNames.Order().ToArray();

        Assert.True(ids.SequenceEqual(company.Employees.Select(x => x.Id).Order()));
        Assert.True(orderIds.SequenceEqual(company.OrderIds.Order()));
        Assert.True(aliases.SequenceEqual(company.Aliases.Order()));
        Assert.True(alternativeNames.SequenceEqual(company.AlternativeNames.Order()));
        Assert.True(carpetsIdsCollected.SequenceEqual(company.CarpetsIdsCollected.Order()));
        Assert.True(petsNames.SequenceEqual(company.PetsNames.Order()));

        var employees = result.Bees.Values.ToArray();
        AssertEmployeesToPersonDto(company.Employees, employees);

    }

    private static void AssertEmployeesToPersonDto(Employee[] employees, PersonDto[] result)
    {
        foreach (var employee in result)
        {
            var matching = employees.First(x => x.Id == employee.Id);

            Assert.Equal(matching.PersonalDetails.Address.City, employee.Address.City);
            Assert.Equal(matching.PersonalDetails.Address.Street, employee.Address.Street);
            Assert.Equal(matching.PersonalDetails.LastName, employee.LastName);
            Assert.Equal("hello", employee.FirstName);
            Assert.Equal(3, employee.Age);
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