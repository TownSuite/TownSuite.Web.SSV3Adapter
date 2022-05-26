using System;
namespace TownSuite.Web.Example.ServiceStackExample
{

	[NestedExample()]
	public class ValidNestedExampleService : BaseServiceExample
	{
		public ValidNestedExampleResponse Any(ValidNestedExample request)
        {
	        return new ValidNestedExampleResponse()
			{
				 Output = "All good"
			};
        }
		
	}
}

