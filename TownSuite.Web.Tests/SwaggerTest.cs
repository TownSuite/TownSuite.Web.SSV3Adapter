using NUnit.Framework;
using TownSuite.Web.SSV3Facade;

namespace TownSuite.Web.Tests;

[TestFixture]
public class SwaggerTest
{
    [Test]
    public async Task HappyPathTest()
    {
        var options = Settings.GetSettings();
        var serviceProvider = Settings.GetServiceProvider();

        var swag = new Swagger(options, serviceProvider,
            "description", "title", "1.1.1");
        var results = await swag.Generate("localhost");

        Assert.That(results.json,
            Is.EqualTo(
                "{\"swagger\":\"2.0\",\"info\":{\"title\":\"title\",\"description\":\"description\",\"version\":\"1.1.1\"},\"host\":\"localhost\",\"basePath\":\"/service/json/syncreply/{name}\",\"schemes\":[\"https\"],\"paths\":{\"/Example\":{\"post\":{\"summary\":\"\",\"parameters\":[{\"in\":\"body\",\"name\":\"Example\",\"description\":\"\",\"schema\":{\"type\":\"object\",\"properties\":{}}}],\"consumes\":[\"application/json\"],\"description\":\"\",\"produces\":[\"application/json\"],\"responses\":{\"200\":{\"description\":\"OK\",\"schema\":{\"type\":\"object\",\"properties\":{\"FirstName\":{\"type\":\"string\"},\"LastName\":{\"type\":\"string\"}}}}}}},\"/Example2\":{\"post\":{\"summary\":\"\",\"parameters\":[{\"in\":\"body\",\"name\":\"Example2\",\"description\":\"\",\"schema\":{\"type\":\"object\",\"properties\":{\"Number1\":{\"type\":\"number\",\"format\":\"double\",\"x-nullable\":\"false\"},\"Number2\":{\"type\":\"number\",\"format\":\"double\",\"x-nullable\":\"false\"},\"Model\":{\"$ref\":\"#/definitions/ComplexModel\"}}}}],\"consumes\":[\"application/json\"],\"description\":\"\",\"produces\":[\"application/json\"],\"responses\":{\"200\":{\"description\":\"OK\",\"schema\":{\"type\":\"object\",\"properties\":{\"Calculated\":{\"type\":\"number\",\"format\":\"double\",\"x-nullable\":\"false\"},\"Model\":{\"$ref\":\"#/definitions/ComplexModel\"},\"TestMultiClassUsage\":{\"$ref\":\"#/definitions/ComplexModel\"}}}}}}},\"/Example3\":{\"post\":{\"summary\":\"\",\"parameters\":[{\"in\":\"body\",\"name\":\"Example3\",\"description\":\"\",\"schema\":{\"type\":\"object\",\"properties\":{}}}],\"consumes\":[\"application/json\"],\"description\":\"\",\"produces\":[\"application/json\"],\"responses\":{\"200\":{\"description\":\"OK\",\"schema\":{\"type\":\"object\",\"properties\":{\"FirstName\":{\"type\":\"string\"},\"LastName\":{\"type\":\"string\"}}}}}}},\"/ExampleDataProfiling\":{\"post\":{\"summary\":\"\",\"parameters\":[{\"in\":\"body\",\"name\":\"ExampleDataProfiling\",\"description\":\"\",\"schema\":{\"type\":\"object\",\"properties\":{\"Number1\":{\"type\":\"number\",\"format\":\"double\",\"x-nullable\":\"false\"},\"Number2\":{\"type\":\"number\",\"format\":\"double\",\"x-nullable\":\"false\"},\"Model\":{\"$ref\":\"#/definitions/ComplexModel\"}}}}],\"consumes\":[\"application/json\"],\"description\":\"\",\"produces\":[\"application/json\"],\"responses\":{\"200\":{\"description\":\"OK\",\"schema\":{\"type\":\"object\",\"properties\":{\"Calculated\":{\"type\":\"number\",\"format\":\"double\",\"x-nullable\":\"false\"},\"Model\":{\"$ref\":\"#/definitions/ComplexModel\"}}}}}}},\"/ValidNestedExample\":{\"post\":{\"summary\":\"\",\"parameters\":[{\"in\":\"body\",\"name\":\"ValidNestedExample\",\"description\":\"\",\"schema\":{\"type\":\"object\",\"properties\":{\"Input\":{\"type\":\"string\"}}}}],\"consumes\":[\"application/json\"],\"description\":\"\",\"produces\":[\"application/json\"],\"responses\":{\"200\":{\"description\":\"OK\",\"schema\":{\"type\":\"object\",\"properties\":{}}}}}}},\"definitions\":{\"ComplexModel\":{\"type\":\"object\",\"properties\":{\"Message\":{\"type\":\"string\"}}}}}"));
        Assert.That(results.statusCode == 200);
    }
}