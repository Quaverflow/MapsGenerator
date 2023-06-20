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
            //x.MapFrom(d => d.LastName, s => s.PersonalDetails.LastName);
            x.Exclude(d => d.LastName);
        });
    }
}
    public class PersonalDetails
    {
        public string LastName { get; set; }
    }
    public class Employee
    {
        public PersonalDetails PersonalDetails { get; set; }
    }

    public class PersonDto
    {
        public  string LastName { get; set; }
    }


}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}