using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadioLib.Data
{
    public class Station
    {
        [JsonProperty("stationuuid")]
        public Guid StationId { get; set; }

        [JsonProperty("name")]
        public string? StationName { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("url_resolved")]
        public string? UrlResolved { get; set; }

        [JsonProperty("homepage")]
        public string? HomePage { get; set; }

        [JsonProperty("favicon")]
        public string? Icon { get; set; }

        [JsonProperty("tags")]
        public string? Tags { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("countrycode")]
        public string? CountryCode { get; set; }

        [JsonProperty("codec")]
        public string? Codec { get; set; }

        [JsonProperty("votes")]
        public string? Votes { get; set; }
    }
}

/*
 * 
   {
    "changeuuid": "02711df1-f2e3-461d-9736-5d320cd21c54",
    "stationuuid": "ab0fc687-7a04-4fa8-8af9-9b71607dd7ae",
    "serveruuid": null,
    "name": "103.7 Da Beat",
    "url": "http://s1.voscast.com:8580/;",
    "url_resolved": "http://s1.voscast.com:8580/;",
    "homepage": "https://www.1037dabeat.com/",
    "favicon": "https://storage.googleapis.com/wzukusers/user-34985523/images/fav-179d7c0d161c40789abc441d6ca3f809/apple-touch-icon-120x120.png?v=fav-179d7c0d161c40789abc441d6ca3f809",
    "tags": "hip-hop,rap,rap hiphop rnb",
    "country": "Ukraine",
    "countrycode": "UA",
    "iso_3166_2": null,
    "state": "",
    "language": "english/russian",
    "languagecodes": "",
    "votes": 112,
    "lastchangetime": "2024-04-02 21:47:26",
    "lastchangetime_iso8601": "2024-04-02T21:47:26Z",
    "codec": "MP3",
    "bitrate": 128,
    "hls": 0,
    "lastcheckok": 1,
    "lastchecktime": "2024-08-03 12:56:05",
    "lastchecktime_iso8601": "2024-08-03T12:56:05Z",
    "lastcheckoktime": "2024-08-03 12:56:05",
    "lastcheckoktime_iso8601": "2024-08-03T12:56:05Z",
    "lastlocalchecktime": "2024-08-03 05:36:46",
    "lastlocalchecktime_iso8601": "2024-08-03T05:36:46Z",
    "clicktimestamp": "2024-08-03 17:54:23",
    "clicktimestamp_iso8601": "2024-08-03T17:54:23Z",
    "clickcount": 31,
    "clicktrend": -3,
    "ssl_error": 0,
    "geo_lat": null,
    "geo_long": null,
    "has_extended_info": false
  },
 * 
 */