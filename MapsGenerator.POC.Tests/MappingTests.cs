
using AutoFixture;
using Xunit;
using AutoMapper;
using MapsGenerator.Tests.Common;
using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

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
        var result =_generator.Map(_company, out _);
        AssertCompanyMap(_company, result);
    }

    private static void AssertCompanyMap(Company company, CompanyDto result)
    {
        Assert.Equal(company.Name, result.TradingName);

        Assert.Equal(company.Address.City, result.Address.City);
        Assert.Equal(company.Address.Street, result.Address.Street);

        AssertEmployeesToPersonDto(company.Employees, result.Workers);

        var ids = result.Bees.Keys.ToArray();
        Assert.True(ids.OrderBy(x => x).SequenceEqual(company.Employees.Select(x => x.Id).OrderBy(x => x)));

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