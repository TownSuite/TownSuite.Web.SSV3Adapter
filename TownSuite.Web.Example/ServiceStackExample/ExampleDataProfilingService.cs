using Dapper;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace TownSuite.Web.Example.ServiceStackExample
{

    [ExampleAttribute()]
    public class ExampleDataProfilingService : BaseServiceExample
    {
        readonly IConfiguration _configuration;
        public ExampleDataProfilingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ExampleDataProfilingResponse> Any(ExampleDataProfiling request)
        {
            string cnStr = _configuration.GetConnectionString("TestDb");

            // using var cn = new System.Data.SqlClient.SqlConnection(cnStr);
            using var cn = new Microsoft.Data.SqlClient.SqlConnection(cnStr);

            await cn.OpenAsync();
            var data = await cn.QueryAsync("SELECT test_column, test_column2 FROM test_table");

            return new ExampleDataProfilingResponse()
            {
                Calculated = request.Number1 + request.Number2,
                Model = new ComplexModel()
                {
                    Message = "Hello world"
                }
            };
        }

        public void SomeOtherExampleMethod()
        {
            Console.WriteLine("SomeOtherExampleMethod called");
        }
    }
}

