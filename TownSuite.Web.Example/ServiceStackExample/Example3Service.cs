namespace TownSuite.Web.Example.ServiceStackExample;

[ExampleAttribute]
public class ExampleService3 : BaseServiceExample
{
    private readonly IDoStuff _stuff;

    public ExampleService3(IDoStuff stuff)
    {
        _stuff = stuff;
    }

    public async Task<Example3Response> Any(Example3 request)
    {
        // demonstrate that it works with async methods
        await Task.Delay(100);

        return new Example3Response
        {
            FirstName = "Hello",
            LastName = $"World_{_stuff.Test}"
        };
    }

    public void SomeOtherExampleMethod()
    {
        Console.WriteLine("SomeOtherExampleMethod called");
    }
}