namespace MapsGenerator.Test;

[UsesVerify]
public class GeneratorDebugger
{
    //this test is only for debugging purposes
    [Fact]
    public Task GeneratesEnumExtensionsCorrectly()
    {
        // The source code to test
        var source = @"
using MapsGenerator;
using System;

namespace somenamespace
{

   
internal class PersonProfile : MapperBase
{
    public PersonProfile()
    {
        Map<Employee, PersonDto>(x =>
        {
            x.MapFromConstantValue(d => d.FirstName, ""hello"");
            x.MapFrom(d => d.LastName, s => s.PersonalDetails.LastName);
            x.Exclude(d => d.LastName);
            x.MapFromConstantValue(d => d.Age, 3);
            x.MapFrom(d => d.Address, s => s.PersonalDetails.Address);
            x.MapFrom(d => d.Height, s => s.PersonalDetails.Height);
            x.EnsureAllDestinationPropertiesAreMapped();
        });
    }
}
internal class GeneratorProfile : MapperBase
{
    public GeneratorProfile()
    {

        Map<Address, AddressDto>(x => x.EnsureAllDestinationPropertiesAreMapped());

        Map<Company, CompanyDto>(x =>
        {
            x.MapFrom(d => d.TradingName, s => s.Name);
            x.MapFrom(d => d.Sector, s => s.Name);
            x.MapFrom(d => d.OrderIds, s => s.OrderIds);
            x.MapFrom(d => d.CarpetsIdsCollected, s => s.CarpetsIdsCollected);
            x.MapFrom(d => d.AlternativeNames, s => s.AlternativeNames);
            x.MapFrom(d => d.Aliases, s => s.Aliases);
            x.MapFrom(d => d.PetsNames, s => s.PetsNames);
            x.MapFrom(d => d.Workers, s => s.Employees);
            x.MapFrom(d => d.Bees, s => s.Employees.ToDictionary(a => a.Id, a => Mapper.Map<PersonDto>(a)));

            x.EnsureAllDestinationPropertiesAreMapped();
        });

        Map<Seniority, SeniorityDto>(x =>
        {
            x.MapFromEnum(SeniorityDto.Starter, Seniority.Junior);
        });
    }
}
    public class Address
    {
        public  string Street { get; set; }
        public  string City { get; set; }
    }

    public class Company
    {
         public  string Name { get; set; }
    public Employee[] Employees { get; set; }
    public List<Guid> OrderIds { get; set; }
    public Queue<int> CarpetsIdsCollected { get; set; }
    public SortedSet<string> AlternativeNames { get; set; }
    public HashSet<string> Aliases { get; set; }
    public List<string> PetsNames { get; set; }
    public  Address Address { get; set; }
    }

    public class Employee
    {
        public Guid Id { get; set; }
        public Person PersonalDetails { get; set; }
        public  string Role { get; set; }
        public Seniority Seniority { get; set; }
    }

    public class Person
    {
        public  string FirstName { get; set; }
        public  string LastName { get; set; }
        public int Age { get; set; }
        public int Height { get; set; }
        public  Address Address { get; set; }
    }

    public enum Seniority
    {
        Junior,
        Intermediate,
        Senior
    }
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

    public class AddressDto
    {
        public  string Street { get; set; }
        public  string City { get; set; }
    }

    public class PersonDto
    {
    public Guid Id { get; set; }
    public  string FirstName { get; set; }
    public  string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public  AddressDto Address { get; set; }
    public  string Role { get; set; }
    public SeniorityDto Seniority { get; set; }
    }

    public enum SeniorityDto
    {
        Starter,
        Intermediate,
        Senior
    }

}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}