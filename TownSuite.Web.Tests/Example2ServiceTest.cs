using Newtonsoft.Json;
using NUnit.Framework;
using TownSuite.Web.Example.ServiceStackExample;
using TownSuite.Web.SSV3Facade;

namespace TownSuite.Web.Tests;

[TestFixture]
public class Example2ServiceTest
{
    [Test]
    public async Task HappyPathTest()
    {
        var options = Settings.GetSettings();
        var serviceProvider = Settings.GetServiceProvider();

        var path = "https://localhost/Example2";

        var value = JsonConvert.SerializeObject(
            new Example2
            {
                Number1 = 4.4m,
                Number2 = 10003.99m,
                Model = new ComplexModel
                {
                    Message = "Test message"
                }
            });


        var facade = new ServiceStackFacade(options, serviceProvider);
        var results = await facade.Post(path, value, "post");

        Assert.That(results.json,
            Is.EqualTo(
                "{\"Calculated\":10008.39,\"Model\":{\"Message\":\"Hello world\"},\"TestMultiClassUsage\":{\"Message\":\"Swagger generation test\"}}"));
        Assert.That(results.statusCode == 200);
    }
}