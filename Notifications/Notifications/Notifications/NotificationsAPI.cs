using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace Notifications
{
    public static class NotificationsAPI
    {
        [FunctionName("NotificationsAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ExecutionContext ctx,
            ILogger log)
        {
            string lAlertBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation($"NotificationsAPI called with payload: {lAlertBody}");

            string ServiceBusConnectionString = "<TODO: Service Bus Connection String>";
            string TopicName = "sensors";
            TopicClient _topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

            SensorEntity lSensor = new SensorEntity();
            lSensor.Name = $"Sensor {lAlertBody}";
            lSensor.Temperature = 98;

            string messageBody = JsonConvert.SerializeObject(lSensor);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            await _topicClient.SendAsync(message);

            return new OkObjectResult(lAlertBody);
        }
    }
}
