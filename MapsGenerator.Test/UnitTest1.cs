using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MapsGenerator.Test;
public static class TestHelper
{
    public static Task Verify(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<PortableExecutableReference> references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references); 

        var generator = new MappingGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver);
    }
}

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}

[UsesVerify]
public class UnitTest1
{
    //[Fact]
    //public void Test1()
    //{
    //    var person = new Person
    //    {
    //        FirstName = "John",
    //        LastName = "Doe",
    //        Age = 30,
    //        Address = new Address
    //        {
    //            Street = "123 Main St",
    //            City = "New York"
    //        }
    //    };

    //    PersonDto personDto = person.MapToPersonDto();

    //    Assert.Equal("John", personDto.FirstName);
    //    Assert.Equal("Doe", personDto.LastName);
    //    Assert.Equal(30, personDto.Age);
    //    Assert.Equal("123 Main St", personDto.Address.Street);
    //    Assert.Equal("New York", personDto.Address.City);
    //}
    [Fact]
    public Task GeneratesEnumExtensionsCorrectly()
    {
        // The source code to test
        var source = @"
using MapsGenerator;

namespace somenamespace
{
    internal class PersonProfile : MapperBase
    {
        public PersonProfile()
        {
            Map<Person, PersonDto>();
        }
    }

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class PersonDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public AddressDto Address { get; set; }
}

public class AddressDto
{
    public string Street { get; set; }
    public string City { get; set; }
}}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}