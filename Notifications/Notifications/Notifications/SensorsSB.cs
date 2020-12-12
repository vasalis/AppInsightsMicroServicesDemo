using System;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Notifications
{
    public static class SensorsSB
    {
        [FunctionName("SensorsSB")]
        public static void Run([ServiceBusTrigger("sensors", "all", Connection = "ServiceBusConnection")]string mySbMsg, ILogger log)
        {
            try
            {
                log.LogInformation($"SensorsSB received message: {mySbMsg}");

                SensorEntity lSensor = JsonConvert.DeserializeObject<SensorEntity>(mySbMsg);

                if (lSensor.Temperature > 10)
                {
                    string lMSg = $"Sensor {lSensor.Name} temp is {lSensor.Temperature} C - threshold is 10 C";

                    string ServiceBusConnectionString = "<TODO: Service Bus Connection String>";
                    string TopicName = "notifications";
                    TopicClient _topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

                    var message = new Message(Encoding.UTF8.GetBytes(lMSg));

                    // Send the message to the queue.
                    _topicClient.SendAsync(message);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to proccess message: {mySbMsg}", ex);
            }
        }
    }
}
