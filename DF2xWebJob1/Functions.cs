
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DF2xWebJobsdk3x
{
    public static class Functions
    {
        [FunctionName(nameof(Functions))]
        public static async Task CronJob(
            [TimerTrigger("0 */2 * * * *")] TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger logger)
        {
            logger.LogInformation("Cron job fired!");

            string instanceId = await client.StartNewAsync<string>(nameof(HelloSequence), input: null);
            logger.LogInformation($"Started new instance with ID = {instanceId}.");

            DurableOrchestrationStatus status;
            while (true)
            {
                status = await client.GetStatusAsync(instanceId);
                logger.LogInformation($"Status: {status.RuntimeStatus}, Last update: {status.LastUpdatedTime}.");

                if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed ||
                    status.RuntimeStatus == OrchestrationRuntimeStatus.Failed ||
                    status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            logger.LogInformation($"Output: {status.Output}");
        }

        public static async Task<List<string>> HelloSequence(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        public static string SayHello([ActivityTrigger] string name, ILogger logger)
        {
            string greeting = $"Hello {name}!";
            logger.LogInformation(greeting);
            Thread.Sleep(5000);
            return greeting;
        }
    }
}
