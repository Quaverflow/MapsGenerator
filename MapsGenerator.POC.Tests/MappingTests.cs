
using AutoFixture;
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
        Assert(_company, result);
    }
    
    [Fact]
    public void Generator_Test()
    {
        var result =_generator.Map(_company, out _);
        Assert(_company, result);
    }

    private static void Assert(Company company, CompanyDto result)
    {
        Xunit.Assert.Equal(company.Name, result.TradingName);

        Xunit.Assert.Equal(company.Address.City, result.Address.City);
        Xunit.Assert.Equal(company.Address.Street, result.Address.Street);

        foreach (var employee in result.Workers)
        {
            var matching = company.Employees.First(x => x.Id == employee.Id);

            Xunit.Assert.Equal(matching.PersonalDetails.Address.City, employee.Address.City);
            Xunit.Assert.Equal(matching.PersonalDetails.Address.Street, employee.Address.Street);
            Xunit.Assert.Equal(matching.PersonalDetails.FirstName, employee.FirstName);
            Xunit.Assert.Equal(matching.PersonalDetails.LastName, employee.LastName);
            Xunit.Assert.Equal(matching.PersonalDetails.Age, employee.Age);
            Xunit.Assert.Equal(matching.PersonalDetails.Height, employee.Height);
            if (matching.Seniority == Seniority.Junior)
            {
                Xunit.Assert.Equal(SeniorityDto.Starter, employee.Seniority);
            }
            else
            {
                Xunit.Assert.Equal(matching.Seniority.ToString(), employee.Seniority.ToString());
            }

            Xunit.Assert.Equal(matching.Role, employee.Role);
        }
    }
}