using NUnit.Framework;
using TownSuite.Web.SSV3Adapter;

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
                @"{
  ""openapi"": ""3.0.1"",
  ""info"": {
    ""title"": ""TownSuite.SSV3Adapter"",
    ""version"": ""v1""
  },
  ""paths"": {
    ""/service/json/syncreply/{name}/Example3"": {
      ""post"": {
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.Example3""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.Example3Response""
                }
              }
            }
          },
          ""299"": {
            ""description"": ""Partial Success""
          },
          ""400"": {
            ""description"": ""Bad Request""
          },
          ""401"": {
            ""description"": ""Unauthorized""
          },
          ""403"": {
            ""description"": ""Forbidden""
          },
          ""429"": {
            ""description"": ""Too Many Requests""
          },
          ""500"": {
            ""description"": ""Internal Server Error""
          },
          ""502"": {
            ""description"": ""Bad Gateway""
          },
          ""504"": {
            ""description"": ""Gateway Timeout""
          }
        }
      }
    },
    ""/service/json/syncreply/{name}/Example"": {
      ""post"": {
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.Example""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ExampleResponse""
                }
              }
            }
          },
          ""299"": {
            ""description"": ""Partial Success""
          },
          ""400"": {
            ""description"": ""Bad Request""
          },
          ""401"": {
            ""description"": ""Unauthorized""
          },
          ""403"": {
            ""description"": ""Forbidden""
          },
          ""429"": {
            ""description"": ""Too Many Requests""
          },
          ""500"": {
            ""description"": ""Internal Server Error""
          },
          ""502"": {
            ""description"": ""Bad Gateway""
          },
          ""504"": {
            ""description"": ""Gateway Timeout""
          }
        }
      }
    },
    ""/service/json/syncreply/{name}/ValidNestedExample"": {
      ""post"": {
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ValidNestedExample""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ValidNestedExampleResponse""
                }
              }
            }
          },
          ""299"": {
            ""description"": ""Partial Success""
          },
          ""400"": {
            ""description"": ""Bad Request""
          },
          ""401"": {
            ""description"": ""Unauthorized""
          },
          ""403"": {
            ""description"": ""Forbidden""
          },
          ""429"": {
            ""description"": ""Too Many Requests""
          },
          ""500"": {
            ""description"": ""Internal Server Error""
          },
          ""502"": {
            ""description"": ""Bad Gateway""
          },
          ""504"": {
            ""description"": ""Gateway Timeout""
          }
        }
      }
    },
    ""/service/json/syncreply/{name}/Example2"": {
      ""post"": {
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.Example2""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.Example2Response""
                }
              }
            }
          },
          ""299"": {
            ""description"": ""Partial Success""
          },
          ""400"": {
            ""description"": ""Bad Request""
          },
          ""401"": {
            ""description"": ""Unauthorized""
          },
          ""403"": {
            ""description"": ""Forbidden""
          },
          ""429"": {
            ""description"": ""Too Many Requests""
          },
          ""500"": {
            ""description"": ""Internal Server Error""
          },
          ""502"": {
            ""description"": ""Bad Gateway""
          },
          ""504"": {
            ""description"": ""Gateway Timeout""
          }
        }
      }
    },
    ""/service/json/syncreply/{name}/ExampleDataProfiling"": {
      ""post"": {
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ExampleDataProfiling""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ExampleDataProfilingResponse""
                }
              }
            }
          },
          ""299"": {
            ""description"": ""Partial Success""
          },
          ""400"": {
            ""description"": ""Bad Request""
          },
          ""401"": {
            ""description"": ""Unauthorized""
          },
          ""403"": {
            ""description"": ""Forbidden""
          },
          ""429"": {
            ""description"": ""Too Many Requests""
          },
          ""500"": {
            ""description"": ""Internal Server Error""
          },
          ""502"": {
            ""description"": ""Bad Gateway""
          },
          ""504"": {
            ""description"": ""Gateway Timeout""
          }
        }
      }
    },
    ""/service/json/syncreply/{name}/InterfaceEndpointExample"": {
      ""post"": {
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.InterfaceEndpointExample""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Success"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""object""
                }
              }
            }
          },
          ""299"": {
            ""description"": ""Partial Success""
          },
          ""400"": {
            ""description"": ""Bad Request""
          },
          ""401"": {
            ""description"": ""Unauthorized""
          },
          ""403"": {
            ""description"": ""Forbidden""
          },
          ""429"": {
            ""description"": ""Too Many Requests""
          },
          ""500"": {
            ""description"": ""Internal Server Error""
          },
          ""502"": {
            ""description"": ""Bad Gateway""
          },
          ""504"": {
            ""description"": ""Gateway Timeout""
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {
      ""TownSuite.Web.Example.ServiceStackExample.Example3"": {
        ""type"": ""object"",
        ""description"": """"
      },
      ""TownSuite.Web.Example.ServiceStackExample.Example3Response"": {
        ""type"": ""object"",
        ""properties"": {
          ""FirstName"": {
            ""type"": ""string""
          },
          ""LastName"": {
            ""type"": ""string""
          }
        }
      },
      ""TownSuite.Web.Example.ServiceStackExample.Example"": {
        ""type"": ""object"",
        ""description"": """"
      },
      ""TownSuite.Web.Example.ServiceStackExample.ExampleResponse"": {
        ""type"": ""object"",
        ""properties"": {
          ""FirstName"": {
            ""type"": ""string""
          },
          ""LastName"": {
            ""type"": ""string""
          }
        }
      },
      ""TownSuite.Web.Example.ServiceStackExample.ValidNestedExample"": {
        ""type"": ""object"",
        ""properties"": {
          ""Input"": {
            ""type"": ""string""
          }
        },
        ""description"": """"
      },
      ""TownSuite.Web.Example.ServiceStackExample.ValidNestedExampleResponse"": {
        ""type"": ""object""
      },
      ""TownSuite.Web.Example.ServiceStackExample.ComplexModel"": {
        ""type"": ""object"",
        ""properties"": {
          ""Message"": {
            ""type"": ""string""
          }
        }
      },
      ""TownSuite.Web.Example.ServiceStackExample.Example2"": {
        ""type"": ""object"",
        ""properties"": {
          ""Number1"": {
            ""type"": ""number""
          },
          ""Number2"": {
            ""type"": ""number""
          },
          ""Model"": {
            ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ComplexModel""
          }
        },
        ""description"": """"
      },
      ""TownSuite.Web.Example.ServiceStackExample.Example2Response"": {
        ""type"": ""object"",
        ""properties"": {
          ""Calculated"": {
            ""type"": ""number""
          },
          ""Model"": {
            ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ComplexModel""
          },
          ""TestMultiClassUsage"": {
            ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ComplexModel""
          }
        }
      },
      ""TownSuite.Web.Example.ServiceStackExample.ExampleDataProfiling"": {
        ""type"": ""object"",
        ""properties"": {
          ""Number1"": {
            ""type"": ""number""
          },
          ""Number2"": {
            ""type"": ""number""
          },
          ""Model"": {
            ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ComplexModel""
          }
        },
        ""description"": """"
      },
      ""TownSuite.Web.Example.ServiceStackExample.ExampleDataProfilingResponse"": {
        ""type"": ""object"",
        ""properties"": {
          ""Calculated"": {
            ""type"": ""number""
          },
          ""Model"": {
            ""$ref"": ""#/components/schemas/TownSuite.Web.Example.ServiceStackExample.ComplexModel""
          }
        }
      },
      ""TownSuite.Web.Example.ServiceStackExample.InterfaceEndpointExample"": {
        ""type"": ""object"",
        ""description"": """"
      }
    }
  }
}"));
        Assert.That(results.statusCode == 200);
    }
}