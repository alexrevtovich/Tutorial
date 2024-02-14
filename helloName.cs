using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json; 
using System.IO; 

namespace TutorialNamespace 
{
    public static class Hello
    {
        [FunctionName("HelloName")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Read the request body and deserialize the JSON to a dynamic object
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Extract the 'name' value from the JSON object
            string name = data?.name;

            // Check if the 'name' value is present and return "Hello <name>"
            if (!string.IsNullOrEmpty(name))
            {
                return new OkObjectResult($"Hello {name}");
            }
            else
            {
                // If no name is provided, return a generic "Hello" message
                return new OkObjectResult("Hello");
            }
        }
    }
}
