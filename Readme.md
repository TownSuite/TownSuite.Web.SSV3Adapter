
TownSuite.Web.SSV3Facade is a simple middleware implementation to help port old Service Stack version 3 .net 3.5 and 4.8 code to net6.0 and asp.net core.

The premise is, existing code that works can be upgraded to a new tech stack with minimal changes while permitting and encourging use of newer technologies as needed.

This facade permits hosting of both webapi controllers and older service stack v3 code in the same project.


The original ServiceStack V3 code can be found at https://github.com/ServiceStackV3/ServiceStackV3.

# Caveats

1. Only covers the basics of service stack v3
2. It is expected that the code being ported will need to hook into newer asp.net core features
3. The facade only returns json
4. Newtonsoft Json.NET is used

# Usage


Add nuget package [Add Name Here]

[Add nuget add command here]


Code example.  Add this to Program.cs for new net6.0 asp.net core or Startup.cs files.

```cs
// If a service has an attribute that implements TownSuite.Web.SSV3Facade.IExecutableAttribute
// its ExecuteAsync method will be invoked.  This is to help port over
// old code.  Create an attribute that also implmentats IExecutableAttribute.
// If this is not wanted, or some other action is desired then
// implement the CustomCallBack delegate and watch for
// callbackType == CustomCall.ServiceInstantiated.  This method
// will be called after the service is instantiated but before any methods
// of the service are invoked.
app.UseServiceStackV3Facade(new ServiceStackV3FacadeOptions(
    serviceTypes: new Type[] {
        typeof(TownSuite.Web.Example.ServiceStackExample.BaseServiceExample)
    })
{
    RoutePath = "/service/json/syncreply/{name}",
    SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
    {
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
        ContractResolver = new AllPropertiesResolver(),
        Converters = {
                        new BackwardsCompatStringConverter()
                    }
    },
    SearchAssemblies = new Assembly[]
     {
         Assembly.Load("TownSuite.Web.Example")
     },
    CustomCallBack = ((CustomCall callbackType, object serviceInstance,
          object? requestDto) args) =>
    {
        // Example to demonstrate custom callback hook types

        if (args.callbackType == CustomCall.ServiceInstantiated)
        {
            Console.WriteLine($"Service {args.serviceInstance.GetType().ToString()} initialized.");
            // This is the place to do custom authorization work or
            // other custom work that needs to take place before service methods are called
        }


        // Example of executing an extra method regardless based solely on serviceInstance type
        if (args.serviceInstance.GetType() == typeof(TownSuite.Web.Example.ServiceStackExample.ExampleService))
        {
            var serviceInstance = (args.serviceInstance as TownSuite.Web.Example.ServiceStackExample.ExampleService);
            serviceInstance?.SomeOtherExampleMethod();
        }

        return Task.CompletedTask;
    },
    CustomErrorHandler = async (
        (Type Service, MethodInfo Method, Type DtoType)? serviceInfo,
        object instance, Exception ex
        ) =>
    {
        // Example to demonstrate a custom error handling hook

        return await Task.FromResult<(string Output, bool ReThrow)>(
            (Output: "Demonstratate a custom result can be returned.",
            ReThrow: true));
    },

});
```

If swagger json can also be generated at runtime by adding the following.

```cs
app.UseServiceStackV3FacadeSwagger(new ServiceStackV3FacadeOptions(
    serviceTypes: new Type[] {
        typeof(TownSuite.Web.Example.ServiceStackExample.BaseServiceExample)
    })
{
    RoutePath = "/swag/swagger.json",
    SearchAssemblies = new Assembly[]
     {
         Assembly.Load("TownSuite.Web.Example")
     },
});
```


