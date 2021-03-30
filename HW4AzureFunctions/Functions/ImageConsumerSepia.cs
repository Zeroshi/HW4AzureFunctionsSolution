using System;
using System.IO;
using System.Threading.Tasks;
using HW4AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HW4AzureFunctions
{
    public static class ImageConsumerSepia
    {
        //ImageConsumerSepia/{name}
        const string ImagesToConvertRoute = "converttosepia/{name}";
        private static string _jobId;
        private static string _convertedBlobName;
        private const string ConversionType = "Sepia";

        [FunctionName("ImageConsumerSepia")]
        public static async void Run([BlobTrigger(ImagesToConvertRoute, Connection = ConfigSettings.STORAGE_CONNECTIONSTRING_NAME)] CloudBlockBlob cloudBlockBlob, string name, ILogger log)
        {

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n ContentType: {cloudBlockBlob.Properties.ContentType} Bytes");

            /*
             * This azure function will be triggered when images are uploaded into the converttosepia container. It will then write out a record to the imageconversionjobs
             * with a status of 1 and an imageconversionmode of Sepia. It will then convert the image to a sepia image and store the resultant image as a blob in the
             * convertedimages container if the conversion was a success or the failedimages container if conversion failed. When your code enters the stage where it's
             * about to convert the image, change the status in the jobs table to 2. When initially setting or updating the status ensure that you also update the
             * statusDescription with the human readable description of the status as defined in the job status table definition.
                Assign the blob a unique ID, with the original blob name appended, using the .NET class Guid. This is the id of the blob that will be put into the convertedimages or 
                failedimages depending on success or failure of conversion. The format is <guid>-<original blobId>
                For example, if the original blob id was dogs.jpg, the resultant converted image blob id would be e55286cd-9464-47d8-b399-62dd0355fa93-dogs.jpg.
                Assign the job a different unique ID using the .NET class Guid.
                Assign a partition key of: imageconversions
                Ensure that the job record contains the Azure public url to the uploaded image in the imageSource column.
                Ensure that the job record contains the imageConversionMode set to Sepia
             */

            using (Stream blobStream = await cloudBlockBlob.OpenReadAsync())
            {
                // Get the storage account
                string storageConnectionString = Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                // Create a blob client so blobs can be retrieved and created
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                //Set up Names
                _convertedBlobName = $"{Guid.NewGuid()}-{name}";
                _jobId = Guid.NewGuid().ToString();

                //Populate Job Table
                await InsertJobTableWithStatus(log, _jobId, 1, ConversionType, cloudBlockBlob.Uri.AbsoluteUri);

                // Create or retrieve a reference to the converted images container
                CloudBlobContainer convertedImagesContainer = blobClient.GetContainerReference(ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME);
                bool created = await convertedImagesContainer.CreateIfNotExistsAsync();
                log.LogInformation($"[{ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME}] Container needed to be created: {created}");

                CloudBlobContainer failedImagesContainer = blobClient.GetContainerReference(ConfigSettings.FAILED_IMAGES_CONTAINERNAME);
                created = await failedImagesContainer.CreateIfNotExistsAsync();
                log.LogInformation($"[{ConfigSettings.FAILED_IMAGES_CONTAINERNAME}] Container needed to be created: {created}");


                await ConvertAndStoreImage(log, blobStream, convertedImagesContainer, name, failedImagesContainer);
            }
        }



        /// <summary>
        /// Updates the job table with status.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        private static async Task InsertJobTableWithStatus(ILogger log, string jobId, int status,
            string imageConversionMode, string imageSource)
        {
            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            await jobTable.InsertOrReplaceJobEntity(jobId, status, imageConversionMode, imageSource);
        }

        /// <summary>
        /// Updates the job table with status.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        private static async Task UpdateJobTableWithStatus(ILogger log, string jobId, int status,
            string imageConversionMode, string statusDescription)
        {
            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            await jobTable.UpdateJobEntityStatus(jobId, status, imageConversionMode, statusDescription);
        }


        /// <summary>
        /// Stores the failed image.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="uploadedImage">The uploaded image.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <param name="failedImagesContainer">The failed images container.</param>
        /// <param name="convertedBlobName">Name of the converted BLOB.</param>
        /// <param name="jobId">The job identifier.</param>
        private static async Task StoreFailedImage(ILogger log, Stream uploadedImage, string blobName,
            CloudBlobContainer failedImagesContainer, string convertedBlobName, string jobId)
        {
            try
            {
                log.LogInformation(
                    $"[+] Storing failed image {blobName} into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");

                CloudBlockBlob failedBlockBlob = failedImagesContainer.GetBlockBlobReference(convertedBlobName);
                failedBlockBlob.Metadata.Add(ConfigSettings.JOBID_METADATA_NAME, jobId);

                uploadedImage.Seek(0, SeekOrigin.Begin);
                await failedBlockBlob.UploadFromStreamAsync(uploadedImage);

                log.LogInformation(
                    $"[+] Stored failed image {blobName} into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");
            }
            catch (Exception ex)
            {
                log.LogError(
                    $"Failed to store a blob called {blobName} that failed conversion into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME}. Exception ex {ex.Message}");
            }
        }

        /// <summary>
        /// Converts the and store image.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="uploadedImagesContainer">The uploaded images container.</param>
        /// <param name="convertedImagesContainer">The converted images container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        private static async Task ConvertAndStoreImage(ILogger log,
            Stream uploadedImage,
            CloudBlobContainer convertedImagesContainer,
            string blobName,
            CloudBlobContainer failedImagesContainer)
        {
            try
            {
                await UpdateJobTableWithStatus(log, _jobId, 2, "Sepia", "Converting to Sepia");

                uploadedImage.Seek(0, SeekOrigin.Begin);

                using (MemoryStream convertedMemoryStream = new MemoryStream())
                using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(uploadedImage))
                {
                    log.LogInformation($"[+] Starting conversion of image {blobName}");

                    image.Mutate(x => x.Sepia());
                    image.SaveAsJpeg(convertedMemoryStream);

                    convertedMemoryStream.Seek(0, SeekOrigin.Begin);
                    log.LogInformation($"[-] Completed conversion of image {blobName}");

                    log.LogInformation(
                        $"[+] Storing converted image {blobName} into {ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                    CloudBlockBlob convertedBlockBlob =
                        convertedImagesContainer.GetBlockBlobReference(_convertedBlobName);

                    convertedBlockBlob.Metadata.Add(ConfigSettings.JOBID_METADATA_NAME, _jobId);

                    convertedBlockBlob.Properties.ContentType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    await convertedBlockBlob.UploadFromStreamAsync(convertedMemoryStream);

                    // Save the job id in blob meta data for later retrieval
                    //convertedBlockBlob.SetMetadata(null);

                    log.LogInformation(
                        $"[-] Stored converted image {_convertedBlobName} into {ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to convert blob {blobName} Exception ex {ex.Message}");
                await StoreFailedImage(log, uploadedImage, blobName, failedImagesContainer,
                    convertedBlobName: _convertedBlobName, jobId: _jobId);
            }
        }
    }
}
