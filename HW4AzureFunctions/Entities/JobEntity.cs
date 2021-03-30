using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace HW4AzureFunctions.Entities
{
    public class JobEntity : TableEntity
    {
        [JsonProperty("jobId")]
        [MaxLength(36)]
        public string JobId { get; set; }
        [JsonProperty("imageConversionMode")]
        public string ImageConversionMode { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("statusDescription")]
        [MaxLength(512)]
        public string StatusDescription { get; set; }
        [JsonProperty("imageSource")]
        [MaxLength(512)]
        public string ImageSource { get; set; }
        [JsonProperty("imageResult")]
        [MaxLength(512)]
        public string ImageResult { get; set; }
    }
}
