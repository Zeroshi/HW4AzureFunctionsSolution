using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureStorage.Entities;
using HW4AzureFunctions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace HW4AzureFunctions
{
    public static class ConversionJobStatus
    {
        const string route = "v1/jobs";

        [FunctionName("ConversionJobStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = route)] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                //return error if null
                //if (code == null)
                //{
                //    var error = new ErrorResponse();
                //    error.ErrorNumber = 4;
                //    error.ParameterName = "code";
                //    error.ParameterValue = code;
                //    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(4);

                //    return new BadRequestObjectResult(error);
                //}

                ////return error if blank
                //if (code == string.Empty)
                //{
                //    var error = new ErrorResponse();
                //    error.ErrorNumber = 2;
                //    error.ParameterName = "code";
                //    error.ParameterValue = code;
                //    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(2);

                //    return new BadRequestObjectResult(error);
                //}

                //string name = req.Query["name"];

                //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                //dynamic data = JsonConvert.DeserializeObject(requestBody);
                //name = name ?? data?.name;

                //string responseMessage = string.IsNullOrEmpty(name)
                //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

                //query

                //get all of the rows
                var results = GetJobs(log);

                //return all rows to client
                return new OkObjectResult(results.Result);
            }
            catch (Exception ex)
            {
                var error = new ErrorResponse();
                error.ErrorNumber = 3;
                error.ParameterName = "";
                error.ParameterValue = "";
                error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(3);

                return new NotFoundObjectResult(error);
            }
        }

        /// <summary>
        /// Get all of the jobs
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        private static async Task<List<JobEntity>> GetJobs(ILogger log)
        {
            log.LogInformation(string.Format("Gathering all request"));

            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            return await jobTable.RetrieveJobEntities();
        }
    }
}

