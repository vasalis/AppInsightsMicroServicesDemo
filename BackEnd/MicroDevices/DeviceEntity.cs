using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroDevices
{
    public class DeviceEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "added")]
        public DateTime Added { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }       

    }
}
