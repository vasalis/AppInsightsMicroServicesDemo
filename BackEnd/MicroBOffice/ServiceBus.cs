using System;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MicroBOffice
{
    public static class ServiceBus
    {
        public class BackOfficeLog
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Text { get; set; }
        }

        [FunctionName("BOfficeSBus")]
        [return: Table("BackOfficeLogs", Connection = "StorageConnectionString")]
        public static BackOfficeLog Run([ServiceBusTrigger("%ServiceBusTopicName%", "all", Connection = "ServiceBusConnectionString")]string mySbMsg, ILogger log)
        {
            TelemetryClient lClient = new TelemetryClient();

            lClient.TrackEvent("Custom Event BOfficeSBus. Enabled logging to Storage.");

            log.LogInformation($"BOfficeSBus processed message: {mySbMsg}");

            return new BackOfficeLog { PartitionKey = "Http", RowKey = Guid.NewGuid().ToString(), Text = mySbMsg };
        }
    }
}
