using NUnit.Framework;
using TownSuite.Web.SSV3Adapter;

namespace TownSuite.Web.Tests;

[TestFixture]
public class ExampleServiceTest
{
    [Test]
    public async Task HappyPathTest()
    {
        var options = Settings.GetSettings();
        var serviceProvider = Settings.GetServiceProvider();

        var path = "https://localhost/Example";

        var value = "";


        var adapter = new ServiceStackAdapter(options, serviceProvider);
        var results = await adapter.Post(path, value, "any");

        Assert.That(results.json, Is.EqualTo("{\"FirstName\":\"Hello\",\"LastName\":\"World\"}"));
        Assert.That(results.statusCode == 200);
    }
}