using NUnit.Framework;
using TownSuite.Web.SSV3Facade;

namespace TownSuite.Web.Tests;

[TestFixture]
public class ExampleServiceTest
{

    [Test]
    public async Task HappyPathTest()
    {
        var options = Settings.GetSettings();
        var serviceProvider = Settings.GetServiceProvider();

        string path = "https://localhost/Example";

        string value="";
   

        var facade = new ServiceStackFacade(options, serviceProvider);
        var results = await facade.Post(path, value);

        Assert.That(results.json, Is.EqualTo("{\"FirstName\":\"Hello\",\"LastName\":\"World\"}"));
        Assert.That(results.statusCode == 200);
    }

}

