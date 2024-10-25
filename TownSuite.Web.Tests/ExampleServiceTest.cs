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

    [Test]
    public async Task HappyPathTest_inner_class()
    {
        var options = Settings.GetSettings();
        var serviceProvider = Settings.GetServiceProvider();

        var path = "https://localhost/ExampleOuterClass+ExampleInnerClass";

        var value = "";

        var adapter = new ServiceStackAdapter(options, serviceProvider);
        var results = await adapter.Post(path, value, "any");

        Assert.That(results.json, Is.EqualTo("{\"FirstName\":\"Hello\",\"LastName\":\"World\"}"));
        Assert.That(results.statusCode == 200);
    }

    [Test]
    public async Task Should_handle_exceptions_outside_of_direct_service()
    {
        var expectedException = "My Exception";
        string? exceptionMessage = null;
        var options = Settings.GetSettings();
        options.CustomCallBack = (args) => throw new Exception(expectedException);
        var serviceProvider = Settings.GetServiceProvider();
        var path = "https://localhost/Example";
        var value = "";

        options.OtherExceptionCallback =
            ex =>
            {
                exceptionMessage = ex.Message;
                return (418, null);
            };
        var adapter = new ServiceStackAdapter(options, serviceProvider);
        var results = await adapter.Post(path, value, "any");

        Assert.That(exceptionMessage, Is.EqualTo(expectedException));
        Assert.That(results.statusCode == 418);
    }
}