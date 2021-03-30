using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using AzureStorage.Entities;
using HW4AzureFunctions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HW4AzureFunctions
{
    public static class ConversionJobStatusById
    {
        const string route = "v1/jobs";
        //"v1/jobs?id={id}code={xxxxx}" 

        [FunctionName("ConversionJobStatusById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = route)] HttpRequest req, string id,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                if (id == null)
                {
                    var error = new ErrorResponse();
                    error.ErrorNumber = 4;
                    error.ParameterName = "id";
                    error.ParameterValue = id;
                    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(4);

                    return new BadRequestObjectResult(error);
                }

                //if (code == null)
                //{
                //    var error = new ErrorResponse();
                //    error.ErrorNumber = 4;
                //    error.ParameterName = "code";
                //    error.ParameterValue = code;
                //    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(4);

                //    return new BadRequestObjectResult(error);
                //}

                if (id == string.Empty)
                {
                    var error = new ErrorResponse();
                    error.ErrorNumber = 2;
                    error.ParameterName = "id";
                    error.ParameterValue = id;
                    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(2);

                    return new BadRequestObjectResult(error);
                }

                //if (code == string.Empty)
                //{
                //    var error = new ErrorResponse();
                //    error.ErrorNumber = 2;
                //    error.ParameterName = "code";
                //    error.ParameterValue = code;
                //    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(2);

                //    return new BadRequestObjectResult(error);
                //}

                if (id.Length > 37) //key is 37 characters, directions need to be updated : example: 4f864935-a5a9-4976-89c5-fb792a02c6a4
                {
                    var error = new ErrorResponse();
                    error.ErrorNumber = 5;
                    error.ParameterName = "id";
                    error.ParameterValue = id;
                    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(5);

                    return new NotFoundObjectResult(error);
                }



                //string name = req.Query["id"];

                //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                //dynamic data = JsonConvert.DeserializeObject(requestBody);
                //name = name ?? data?.name;

                //string responseMessage = string.IsNullOrEmpty(name)
                //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

                var result = GetJobById(log, id);

                if (result.Result == null)
                {
                    var error = new ErrorResponse();
                    error.ErrorNumber = 3;
                    error.ParameterName = "id";
                    error.ParameterValue = id;
                    error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(3);

                    return new NotFoundObjectResult(error);
                }

                return new OkObjectResult((JobEntity)result.Result);
            }
            catch (Exception ex)
            {
                var error = new ErrorResponse();
                error.ErrorNumber = 3;
                error.ParameterName = "id";
                error.ParameterValue = id;
                error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(3);

                return new NotFoundObjectResult(error);
            }
        }

        /// <summary>
        /// Get the jobid information.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        private static async Task<JobEntity> GetJobById(ILogger log, string jobId)
        {
            log.LogInformation(string.Format("Gathering request for jobId: [0]", jobId));

            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            return await jobTable.RetrieveJobEntity(jobId);
        }
    }
}

