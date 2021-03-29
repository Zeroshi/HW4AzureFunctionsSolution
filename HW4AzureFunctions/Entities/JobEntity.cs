using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace HW4AzureFunctions.Entities
{
    public class JobEntity : TableEntity
    {
        [JsonProperty("jobId")]
        public string JobId { get; set; }
        [JsonProperty("imageConversionMode")]
        public string ImageConversionMode { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }
        [JsonProperty("imageSource")]
        public string ImageSource { get; set; }
        [JsonProperty("imageResult")]
        public string ImageResult { get; set; }
    }
}
