using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
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
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = route)] HttpRequest req, string id, string code,
            ILogger log)
        {
            ;
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

            if (code == null)
            {
                var error = new ErrorResponse();
                error.ErrorNumber = 4;
                error.ParameterName = "code";
                error.ParameterValue = code;
                error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(4);

                return new BadRequestObjectResult(error);
            }

            if (id == string.Empty)
            {
                var error = new ErrorResponse();
                error.ErrorNumber = 2;
                error.ParameterName = "id";
                error.ParameterValue = id;
                error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(2);

                return new BadRequestObjectResult(error);
            }

            if (code == string.Empty)
            {
                var error = new ErrorResponse();
                error.ErrorNumber = 2;
                error.ParameterName = "code";
                error.ParameterValue = code;
                error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(2);

                return new BadRequestObjectResult(error);
            }




            //string name = req.Query["id"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            var result = GetJobById(log, id, code);

            if (result.Result == null)
            {
                var error = new ErrorResponse();
                error.ErrorNumber = 3;
                error.ParameterName = "id";
                error.ParameterValue = id;
                error.ErrorDescription = ErrorResponsesInformation.ErrorMessages.GetValueOrDefault(3);

                return new NotFoundObjectResult(error);
            }

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Get the jobid information.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        private static async Task<JobEntity> GetJobById(ILogger log, string jobId, string code)
        {
            log.LogInformation(string.Format("Gathering request for jobId: [0]", jobId));

            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY, code);
            return await jobTable.RetrieveJobEntity(jobId);
        }
    }
}

