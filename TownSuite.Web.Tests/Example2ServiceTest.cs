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

        string path = "https://localhost/Example2";

        string value = JsonConvert.SerializeObject(
            new Example2() {
                 Number1 = 4.4m,
                 Number2 = 10003.99m,
                 Model = new ComplexModel()
                 {
                      Message = "Test message"
                 }
            });
   

        var facade = new ServiceStackFacade(options, serviceProvider);
        var results = await facade.Post(path, value);

        Assert.That(results.json, Is.EqualTo("{\"Calculated\":10008.39,\"Model\":{\"Message\":\"Hello world\"}}"));
        Assert.That(results.statusCode == 200);
    }

}

