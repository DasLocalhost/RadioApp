using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Radio.Data;
using Radio.Api;
using RadioLib.Data;

namespace Radio
{
    /// <summary>
    /// API client for radio browser API
    /// </summary>
    public class RadioClient
    {
        private string _apiUrl;
        private RestClient _client;

        private readonly string _countriesApi = "json/countries";
        private readonly string _statesApi = "json/states";
        private readonly string _stationsApi = "json/stations";

        // TODO : add offsets / limits to API calls
        public RadioClient()
        {
            _apiUrl = GetApiUrl();
            
            _client = new RestClient(_apiUrl);
        }

        private string GetApiUrl()
        {
            // Get fastest ip of dns
            string baseUrl = @"all.api.radio-browser.info";
            var ips = Dns.GetHostAddresses(baseUrl);
            long lastRoundTripTime = long.MaxValue;
            string searchUrl = @"de1.api.radio-browser.info"; // Fallback
            foreach (IPAddress ipAddress in ips)
            {
                var reply = new Ping().Send(ipAddress);
                if (reply != null &&
                    reply.RoundtripTime < lastRoundTripTime)
                {
                    lastRoundTripTime = reply.RoundtripTime;
                    searchUrl = ipAddress.ToString();
                }
            }

            // Get clean name
            IPHostEntry hostEntry = Dns.GetHostEntry(searchUrl);
            if (!string.IsNullOrEmpty(hostEntry.HostName))
            {
                searchUrl = hostEntry.HostName;
            }

            return "https:\\\\" + searchUrl;
        }

        public State[]? GetStates()
        {
            return RequestBuilder.Make(_client, _statesApi).Get<State[]>();
        }

        public State[]? GetStates(string byCountry)
        {
            return RequestBuilder.Make(_client, _statesApi + $"/{byCountry}/").Get<State[]>();
        }

        public Country[]? GetCountries()
        {
            return RequestBuilder.Make(_client, _countriesApi).Get<Country[]>();
        }

        public Station[]? GetStations()
        {
            return RequestBuilder.Make(_client, _stationsApi).Get<Station[]>();
        }

        public Station[]? GetStations(string byState)
        {
            return RequestBuilder.Make(_client, _stationsApi + $"/bystate/{byState}").Get<Station[]>();
        }

        public async Task<Station[]?> GetStationsAsync(string byState)
        {
            return await RequestBuilder.Make(_client, _stationsApi + $"/bystate/{byState}").GetAsync<Station[]>();
        }
    }
}
