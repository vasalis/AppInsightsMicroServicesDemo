using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace MicroDevices
{
    public class RestApi
    {
        private readonly ILogger mLogger;
        private readonly IConfiguration mConfig;
        private Container mContainer;
        private readonly ITopicClient _topicClient;

        public RestApi(ILogger<RestApi> logger,
            IConfiguration config,
            Container aDbContainer,
            ITopicClient aQueueClient)
        {
            mLogger = logger;
            mConfig = config;
            mContainer = aDbContainer;
            _topicClient = aQueueClient;
        }

        [FunctionName("Devices")]
        public async Task<IActionResult> Devices(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Devices trigger function processed a request.");

            IActionResult returnValue = null;

            try
            {
                List<DeviceEntity> lResults = new List<DeviceEntity>();

                string lCategory = "all";

                QueryDefinition queryDefinition = new QueryDefinition("select TOP 100 * from Devices t order by t._ts desc");

                using (FeedIterator<DeviceEntity> feedIterator = this.mContainer.GetItemQueryIterator<DeviceEntity>(
                    queryDefinition,
                    null,
                    new QueryRequestOptions() { PartitionKey = new PartitionKey(lCategory) }))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        foreach (var item in await feedIterator.ReadNextAsync())
                        {
                            lResults.Add(item);
                        }
                    }

                    mLogger.LogInformation($"Got {lResults.Count} for category: {lCategory}");

                    returnValue = new OkObjectResult(lResults);
                }

                // For demo - broadcast a new device on each call
                DeviceEntity lNewD = new DeviceEntity() { Id = Guid.NewGuid().ToString(), Added = DateTime.UtcNow, Category = "all", Name = $"Device {Guid.NewGuid().ToString().Substring(0, 3)}" };
                await this.BroadcastNewDeviceAdded(lNewD);
            }
            catch (Exception ex)
            {
                mLogger.LogError($"Could not GetDevices. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }

        [FunctionName("AddNewDevice")]
        public async Task<IActionResult> AddNewDevice(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AddNewDevice trigger function processed a request.");

            IActionResult returnValue = null;

            try
            {
                // Persist to this Microservice (CosmosDB).
                var input = await this.WriteToCosmosDb(req);

                // Inform other Microservices that might be listening.
                await this.BroadcastNewDeviceAdded(input);
                
                returnValue = new OkObjectResult(input);
            }
            catch (Exception ex)
            {
                mLogger.LogError($"Could not insert item. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }

        private async Task<DeviceEntity> WriteToCosmosDb(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DeviceEntity input = JsonConvert.DeserializeObject<DeviceEntity>(requestBody);

            input.Id = Guid.NewGuid().ToString();
            input.Added = DateTime.UtcNow;
            input.Category = "all";            

            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                ItemResponse<DeviceEntity> lUpdatedItem = await mContainer.UpsertItemAsync(input);

                mLogger.LogInformation("Item updated or inserted");
                mLogger.LogInformation($"This query cost: {lUpdatedItem.RequestCharge} RU/s");
            }

            return input;
        }

        private async Task BroadcastNewDeviceAdded(DeviceEntity aDevice)
        {
            string messageBody = $"Added device: {JsonConvert.SerializeObject(aDevice)}";
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // Write the body of the message to the App Insights
            mLogger.LogInformation($"[Function output] Sending message to Service Bus: {messageBody}");

            // Send the message to the queue.
            await _topicClient.SendAsync(message);
        }        
    }
}
