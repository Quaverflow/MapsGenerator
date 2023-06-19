﻿using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

internal class PersonProfile : MapperBase
{
    public PersonProfile()
    {
        Map<Employee, PersonDto>(x =>
        {
            x.MapFromConstantValue(d => d.FirstName, "hello");
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