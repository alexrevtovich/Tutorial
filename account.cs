using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System;


namespace TutorialNamespace
{
    public static class AccountFunction
    {
        // Retrieve the MongoDB connection string from environment variables for security and flexibility.
        private static readonly string connectionString = Environment.GetEnvironmentVariable("MongoDbConnectionString");

        // Constants for the MongoDB database and collection names.
        private static readonly string databaseName = "tutorial";
        private static readonly string collectionName = "users";

        // Class representing the expected structure of the request payload.
        public class UserInput
        {
            public string Name { get; set; }
            public string Password { get; set; }
        }

        // Azure Function to add a new user to the MongoDB collection.
        [FunctionName("addUser")]
        public static async Task<IActionResult> AddUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function 'addUser' processed a request.");

            // Read and deserialize the request body to a UserInput object.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UserInput>(requestBody);

            // Validate the request payload to ensure required fields are provided.
            if (string.IsNullOrEmpty(data?.Name))
            {
                return new BadRequestObjectResult(new { error_message = "name is not provided" });
            }

            if (string.IsNullOrEmpty(data?.Password))
            {
                return new BadRequestObjectResult(new { error_message = "password is not provided" });
            }

            try
            {
                // Initialize the MongoDB client, database, and collection.
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                // Create a new BSON document from the user input and insert it into the collection.
                var newUser = new BsonDocument { { "name", data.Name }, { "password", data.Password } };
                await collection.InsertOneAsync(newUser);

                // Return the inserted user data as a successful response.
                return new OkObjectResult(new { data.Name, data.Password });
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during database insertion and return a server error response.
                log.LogError($"An error occurred while inserting the user into the database: {ex.Message}");
                return new StatusCodeResult(500); // Internal Server Error
            }
        }
    }
}
