namespace TownSuite.Web.Example.ServiceStackExample
{
    [ExamplePropertySet("TEST", "Test2")]
    public class ExampleAttributePropertyService : BaseServiceExample
    {
        public ExampleAttributePropertyResponse Any(ExampleAttributeProperty request)
        {
            return new ExampleAttributePropertyResponse
            {
                AttributeValue = request.AttributeValue,
            };
        }
    }
}
