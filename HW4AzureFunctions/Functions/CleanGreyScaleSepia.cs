using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HW4AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace HW4AzureFunctions.Functions
{
    public static class CleanGreyScaleSepia
    {
        [FunctionName("CleanGreyScaleSepia")]
        public static void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            CleanContainers(log);
            log.LogInformation($"Containers completed cleaning: {DateTime.Now}");
        }

        /// <summary>
        /// Gets the jobid information.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        private static async void CleanContainers(ILogger log)
        {
            log.LogInformation(string.Format("Gathering all request"));

            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            jobTable.ClearContainers(log);
        }
    }
}
