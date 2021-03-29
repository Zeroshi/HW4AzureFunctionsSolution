using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace AzureStorage.Entities
{
    /// <summary>
    /// error model
    /// </summary>
    public class ErrorResponse 
    {
        /// <summary>
        /// internal error number
        /// </summary>
        [JsonPropertyName("errorNumber")]
        public int ErrorNumber { get; set; }
        /// <summary>
        /// perameter name
        /// </summary>
        [JsonPropertyName("parameterName")]
        public string ParameterName { get; set; }
        /// <summary>
        /// value of paramater
        /// </summary>
        [JsonPropertyName("parameterValue")]
        public string ParameterValue { get; set; }
        /// <summary>
        /// internal error description 
        /// </summary>
        [JsonPropertyName("errorDescription")]
        public string ErrorDescription { get; set; }
    }
}
