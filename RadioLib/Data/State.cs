using Newtonsoft.Json;
using Radio.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Radio.Data
{
    public class State
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("stationcount")]
        public int? StationCount { get; set; }

        public override string ToString()
        {
            return $"{Name} - {Country} ({StationCount})";
        }
    }
}