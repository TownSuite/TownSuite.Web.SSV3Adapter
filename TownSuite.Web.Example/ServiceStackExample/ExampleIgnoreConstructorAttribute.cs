using TownSuite.Web.SSV3Adapter.Interfaces;

namespace TownSuite.Web.Example.ServiceStackExample
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ExampleIgnoreConstructorAttribute: Attribute, IIgnoreConstructorAttribute
    {
    }
}
