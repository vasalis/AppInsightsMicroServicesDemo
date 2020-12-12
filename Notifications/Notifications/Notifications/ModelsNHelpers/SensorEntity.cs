using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications
{
    public class SensorEntity
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "temperature")]
        public Int32 Temperature { get; set; }
    }
}
