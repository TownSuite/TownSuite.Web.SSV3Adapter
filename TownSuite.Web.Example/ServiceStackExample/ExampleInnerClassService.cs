namespace TownSuite.Web.Example.ServiceStackExample;

[ExampleAttribute]
public class ExampleInnerClassService : BaseServiceExample
{
    public async Task<ExampleResponse> Any(ExampleOuterClass.ExampleInnerClass request)
    {
        // demonstrate that it works with async methods
        await Task.Delay(100);

        return new ExampleResponse
        {
            FirstName = "Hello",
            LastName = "World"
        };
    }

    public void SomeOtherExampleMethod()
    {
        Console.WriteLine("SomeOtherExampleMethod called");
    }
}