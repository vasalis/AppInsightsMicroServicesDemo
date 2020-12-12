using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace Notifications
{
    public static class NotificationsSB
    {
        [FunctionName("NotificationsSB")]
        public static void Run([ServiceBusTrigger("notifications", "all", Connection = "ServiceBusConnection")]string mySbMsg,
            [SendGrid(ApiKey = "SGridKey")] out SendGridMessage message,            
            ExecutionContext ctx,
            ILogger log)
        {
            message = new SendGridMessage();

            try
            {
                log.LogInformation($"NotificationsSB received message: {mySbMsg}");

                // TODO
                // Handle Notification Entity (Type, Audience etc etc)                
                // Handle state
                // [maybe] Handle ticketing system update

                string lAlertMSg = $"New Alert Notification: {mySbMsg}";

                // Send Email
                // SendEmail(message, lAlertMSg);

                // Send to Teams Channel
                SendToTeams(ctx, lAlertMSg);

                // Send to other services
                SendToNotificationHub(lAlertMSg);
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to proccess message: {mySbMsg}", ex);
            }
        }

        private static void SendEmail(SendGridMessage message, string aMsg)
        {
            message.AddTo("<TODO: email address>");
            message.AddContent("text/html", aMsg);
            message.SetFrom(new EmailAddress("<TODO: Sender email address>", "<TODO: Sender name>"));
            message.SetSubject("New Alert Notification");
        }

        private static void SendToTeams(ExecutionContext aExContext, string aMsg)
        {
            using (var lHttp = new HttpClient())
            {
                string lEndPoint = "<TODO: Teams channel webhook End Point>";

                var lTeamsCard = GetCardForTeams(aExContext, aMsg);
                var content = new StringContent(lTeamsCard, Encoding.UTF8, "application/json");
                var result = lHttp.PostAsync(lEndPoint, content).Result;
            }
        }

        private static void SendToNotificationHub(string aMsg)
        {
            // TODO
        }

        private static string GetCardForTeams(ExecutionContext aExContext, string aMsg)
        {
            // Default approach.
            string ltemplate = $"{{'text':'{aMsg}'}}";

            try
            {
                var filePath = Path.Combine(aExContext.FunctionAppDirectory, "ModelsNHelpers", "TeamsCardTemplate.json");

                string fileContent = System.IO.File.ReadAllText(filePath);
                ltemplate = fileContent.Replace("TextPlaceholder", aMsg);
            }
            catch (Exception ex)
            {
                // TODO: Log this.   
            }

            return ltemplate;
        }
    }
}
