using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioLib.Data
{
    public class Country
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("iso_3166_1")]
        public string? Code { get; set; }

        [JsonProperty("stationcount")]
        public int StationCount { get; set; }

        public override string ToString()
        {
            return $"{Name} : {Code} ({StationCount})";
        }
    }
}
