namespace TownSuite.Web.Example.ServiceStackExample;

[ExampleAttribute(ExampleProperty = ["TEST"])]
public class Example2Service : BaseServiceExample
{
    public async Task<Example2Response> Any(Example2 request)
    {
        // demonstrate that it works with async methods
        await Task.Delay(100);

        return new Example2Response
        {
            Calculated = request.Number1 + request.Number2,
            Model = new ComplexModel
            {
                Message = "Hello world"
            },
            TestMultiClassUsage = new ComplexModel
            {
                Message = "Swagger generation test"
            }
        };
    }

    public void SomeOtherExampleMethod()
    {
        Console.WriteLine("SomeOtherExampleMethod called");
    }
}