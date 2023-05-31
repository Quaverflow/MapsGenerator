﻿using MapsGenerator.Tests.Common.Models;
using MapsGenerator.Tests.Common.Models.Destination;
using MapsGenerator.Tests.Common.Models.Source;

namespace MapsGenerator.Tests.Common;

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

        Map<Address, AddressDto>(x => x.EnsureAllDestinationPropertiesAreMapped());

        Map<Company,CompanyDto>(x =>
        {
            x.MapFrom(d => d.TradingName, s => s.Name);
            x.MapFrom(d => d.Workers, s => s.Employees);
            x.EnsureAllDestinationPropertiesAreMapped();
        });

        Map<Seniority, SeniorityDto>(x =>
        {
            //todo sort out enums
            x.MapFrom(_ => SeniorityDto.Starter, _ => Seniority.Junior);
        });
    }
}