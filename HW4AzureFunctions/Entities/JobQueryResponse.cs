using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;

namespace HW4AzureFunctions.Entities
{
    public class JobQueryResponse
    {
        [Required]
        [JsonProperty("id")]
        public string ID { get; set; }
        [Required]
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
