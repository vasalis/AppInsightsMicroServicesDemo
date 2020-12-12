using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MicroAlerts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlertsController : ControllerBase
    {
        private static readonly string[] Names = new[]
        {
            "First Floor", "Second Floor", "Garage", "Basement", "Attic"
        };

        private static readonly string[] Descriptions = new[]
        {
            "High temp", "Very high temp", "Low temp", "Very low temp", "No power"
        };

        private readonly ILogger<AlertsController> _logger;
        private readonly ITopicClient _topicClient;
        private readonly AlertEntityDbContext _alertsDb;

        public AlertsController(ILogger<AlertsController> logger, ITopicClient aTopicClient, AlertEntityDbContext aDb)
        {
            _logger = logger;
            _topicClient = aTopicClient;
            _alertsDb = aDb;
        }

        [HttpGet]
        public async Task<IEnumerable<AlertEntity>> Get()
        {
            if (!this.DoCompute())
            {
                // Broadcast to Service bus - Demo purpose
                await this.BroadcastNewAlertAdded(Guid.NewGuid().ToString());
            }


            var lExit = _alertsDb.getAlerts();

            if (lExit.Count == 0)
            {
                var rng = new Random();
                lExit = Enumerable.Range(1, 5).Select(index => new AlertEntity
                {
                    ID = Guid.NewGuid().ToString(),
                    When = DateTime.Now.AddDays(index),
                    Name = Names[rng.Next(Names.Length)],
                    Decription = Descriptions[rng.Next(Names.Length)]
                }).ToList();

                // Save to db
                _alertsDb.AddRange(lExit);

                _alertsDb.SaveChanges();
            }

            return lExit;
        }

        [HttpPost]
        public async Task Post([FromBody] string value)
        {
            this.WriteToSqlDb(value);
            await this.BroadcastNewAlertAdded(value);
        }

        private void WriteToSqlDb(string aAlert)
        {
            AlertEntity lNewAlert = JsonConvert.DeserializeObject<AlertEntity>(aAlert);

            _alertsDb.Add(lNewAlert);

            _alertsDb.SaveChanges();
        }

        private async Task BroadcastNewAlertAdded(string aAlert)
        {
            // Create a new message to send to the queue.
            string messageBody = $"New Alert: {aAlert}";
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // Write the body of the message to the App Insights
            _logger.LogInformation($"Sending message to Service Bus: {messageBody}");

            // Send the message to the queue.
            await _topicClient.SendAsync(message);
        }

        private bool DoCompute()
        {
            var lBaseStr = Environment.GetEnvironmentVariable("CPUIntenseBase");

            if (!string.IsNullOrEmpty(lBaseStr))
            {
                int lBase = Convert.ToInt32(lBaseStr);

                _logger.LogInformation($"DoCompute base: {lBase}");

                double result = 0;
                for (var i = Math.Pow(lBase, 8); i >= 0; i--)
                {
                    result += Math.Atan(i) * Math.Tan(i);
                }

                return true;
            }

            return false;
        }        
    }
}
