using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs.Description;

namespace HW4AzureFunctions.Entities
{
    public class JobTable
    {
        private CloudTableClient _tableClient;
        private CloudTable _table;
        private string _partitionKey;
        private ILogger _log;

        /// <summary>
        /// Create Job Table without SAS
        /// </summary>
        /// <param name="log"></param>
        /// <param name="partitionKey"></param>
        public JobTable(ILogger log, string partitionKey)
        {

            string storageConnectionString =
                Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the table client.
            _tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "jobentity" table.
            _table = _tableClient.GetTableReference(ConfigSettings.JOBS_TABLENAME);

            _table.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            _partitionKey = partitionKey;

        }

        ///// <summary>
        ///// Create job table with SAS
        ///// </summary>
        ///// <param name="log"></param>
        ///// <param name="partitionKey"></param>
        ///// <param name="sharedAccessSignature"></param>
        //public JobTable(ILogger log, string partitionKey, string sharedAccessSignature)
        //{
        //    string storageConnectionString = Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

        //    //convert to credentials
        //    StorageCredentials sasCredentials = new StorageCredentials(sharedAccessSignature);

        //    // Create the table client.
        //    _tableClient = new CloudTableClient(new Uri("https://azurestorageassignment3.azurewebsites.net", false), sasCredentials);

        //    // Create the CloudTable object that represents the "jobentity" table.
        //    _table = _tableClient.GetTableReference(ConfigSettings.JOBS_TABLENAME);

        //    _table.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        //    _partitionKey = partitionKey;
        //}

        /// <summary>
        /// Clear Sepia and GreyScale containers
        /// </summary>
        /// <param name="log"></param>
        public async void ClearContainers(ILogger log)
        {
            string storageConnectionString =
                Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);

            BlobContainerClient greyscaleContainerClient =
                new BlobContainerClient(storageConnectionString, "converttogreyscale");
            greyscaleContainerClient.GetBlobs();


            var blobs = greyscaleContainerClient.GetBlobs();
            foreach (BlobItem blobItem in blobs)
            {
                await greyscaleContainerClient.DeleteBlobIfExistsAsync(blobItem.Name);
                log.LogInformation($"GreyScale Blob: {blobItem.Name} was deleted successfully.");
            }

            BlobContainerClient sepiaContainerClient =
                new BlobContainerClient(storageConnectionString, "converttosepia");
            sepiaContainerClient.GetBlobs();

            blobs = sepiaContainerClient.GetBlobs();
            foreach (BlobItem blobItem in blobs)
            {
                await sepiaContainerClient.DeleteBlobIfExistsAsync(blobItem.Name);
                log.LogInformation($"Sepia Blob: {blobItem.Name} was deleted successfully.");
            }
        }


        /// <summary>
        /// Retrieves the job entity.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>JobEntity.</returns>
        public async Task<JobEntity> RetrieveJobEntity(string jobId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<JobEntity>(_partitionKey, jobId);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            return retrievedResult.Result as JobEntity;
        }


        /// <summary>
        /// Returns all job entities
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobEntity>> RetrieveJobEntities()
        {
            TableContinuationToken token = null;
            var result = new List<JobEntity>();

            do
            {
                var q = new TableQuery<JobEntity>();
                var queryResult = Task.Run(() => _table.ExecuteQuerySegmentedAsync(q, token)).GetAwaiter().GetResult();
                foreach (var item in queryResult.Results)
                {
                    var blob = new JobEntity();
                    blob.JobId = item.JobId;
                    blob.ImageConversionMode = item.ImageConversionMode;
                    blob.Status = item.Status;
                    blob.StatusDescription = item.StatusDescription;
                    blob.ImageSource = item.ImageSource;
                    blob.ImageResult = item.ImageResult;
                    result.Add(blob);
                }
                token = queryResult.ContinuationToken;
            } while (token != null);

            return result;
        }

        /// <summary>
        /// Updates the job entity.
        /// </summary>
        /// <param name="jobEntity">The job entity.</param>
        public async Task<bool> UpdateJobEntity(JobEntity jobEntity)
        {
            TableOperation replaceOperation = TableOperation.Replace(jobEntity);
            TableResult result = await _table.ExecuteAsync(replaceOperation);

            if (result.HttpStatusCode > 199 && result.HttpStatusCode < 300)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the job entity status.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        public async Task UpdateJobEntityStatus(string jobId, int status, string imageConversionMode, string statusDescription)
        {
            JobEntity jobEntityToReplace = await RetrieveJobEntity(jobId);
            if (jobEntityToReplace != null)
            {
                jobEntityToReplace.Status = status;
                jobEntityToReplace.ImageConversionMode = imageConversionMode;
                jobEntityToReplace.StatusDescription = statusDescription;
                await UpdateJobEntity(jobEntityToReplace);
            }
        }



        /// <summary>
        /// Inserts the or replace job entity.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        public async Task InsertOrReplaceJobEntity(string jobId, int status, string imageConversionMode, string imageSource)
        {

            JobEntity jobEntityToInsertOrReplace = new JobEntity();
            jobEntityToInsertOrReplace.RowKey = jobId;
            jobEntityToInsertOrReplace.PartitionKey = _partitionKey;
            jobEntityToInsertOrReplace.Status = status;
            jobEntityToInsertOrReplace.ImageConversionMode = imageConversionMode;
            jobEntityToInsertOrReplace.ImageSource = imageSource;

            TableOperation insertReplaceOperation = TableOperation.InsertOrReplace(jobEntityToInsertOrReplace);
            TableResult result = await _table.ExecuteAsync(insertReplaceOperation);

        }

        /// <summary>
        /// Inserts the or replace job entity.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        public async Task InsertOrReplaceFailureJobEntity(string jobId, int status, string statusDescription, string imageResult)
        {

            JobEntity jobEntityToInsertOrReplace = new JobEntity();
            jobEntityToInsertOrReplace.RowKey = jobId;
            jobEntityToInsertOrReplace.PartitionKey = _partitionKey;
            jobEntityToInsertOrReplace.Status = status;
            jobEntityToInsertOrReplace.StatusDescription = statusDescription;
            jobEntityToInsertOrReplace.ImageResult = imageResult;

            TableOperation insertReplaceOperation = TableOperation.InsertOrReplace(jobEntityToInsertOrReplace);
            TableResult result = await _table.ExecuteAsync(insertReplaceOperation);

        }

    }
}
