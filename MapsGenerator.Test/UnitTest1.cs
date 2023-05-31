namespace MapsGenerator.Test;

[UsesVerify]
public class UnitTest1
{
    [Fact]
    public Task GeneratesEnumExtensionsCorrectly()
    {
        // The source code to test
        var source = @"
using MapsGenerator;
using System;

namespace somenamespace
{

    internal class GeneratorProfile : MapperBase
    {
        public GeneratorProfile()
        {
            Map<Employee, PersonDto>(x =>
            {
                x.MapFrom(d => d.FirstName, s => s.PersonalDetails.FirstName);
                x.MapFrom(d => d.LastName, s => s.PersonalDetails.LastName);
                x.MapFrom(d => d.Age, s => s.PersonalDetails.Age);
                x.MapFrom(d => d.Address, s => s.PersonalDetails.Address);
                x.MapFrom(d => d.Height, s => s.PersonalDetails.Height);
                x.EnsureAllDestinationPropertiesAreMapped();
            });

            Map<Address, AddressDto>(x => {x.EnsureAllDestinationPropertiesAreMapped()});

            Map<Company,CompanyDto>(x =>
            {
                x.MapFrom(d => d.TradingName, s => s.Name);
                x.MapFrom(d => d.Workers, s => s.Employees);
                x.EnsureAllDestinationPropertiesAreMapped();
            });

            Map<Seniority, SeniorityDto>(x =>
            {
                //todo sort out enums
                //x.MapFromEnum(SeniorityDto.Starter, Seniority.Junior);
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
        public  Employee[] Employees { get; set; }
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