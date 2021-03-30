using System;
using System.IO;
using System.Threading.Tasks;
using HW4AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HW4AzureFunctions
{
    public static class ImageStatusUpdaterFailed
    {
        const string ImagesToConvertRoute = "failedimages/{name}";
        //ImageStatusUpdaterFailed/{name}

        [FunctionName("ImageStatusUpdaterFailed")]
        public static async void Run([BlobTrigger(ImagesToConvertRoute, Connection = ConfigSettings.STORAGE_CONNECTIONSTRING_NAME)] CloudBlockBlob cloudBlockBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n ContentType: {cloudBlockBlob.Properties.ContentType} Bytes");

            /*
             * This azure function will be triggered when images are uploaded into the failedimages container. Update the job with the status of 4. Also update the
             * statusDescription with the human readable description of the status as defined in the job status table definition. Update the imageResult property with
             * the Azure public url to the failed image.
             */


            var jobId = Guid.NewGuid().ToString();
            await UpdateJobTableWithStatus(log, jobId, 4, "Image failed during image conversion process", cloudBlockBlob.Uri.AbsoluteUri);

        }

        /// <summary>
        /// Updates the job table with status.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        private static async Task UpdateJobTableWithStatus(ILogger log, string jobId, int status, string statusDescription, string imageResult)
        {
            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            await jobTable.InsertOrReplaceFailureJobEntity(jobId, status, statusDescription, imageResult);
        }
    }
}
