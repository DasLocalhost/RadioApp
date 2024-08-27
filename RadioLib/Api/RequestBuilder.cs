using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace Radio.Api
{
    /// <summary>
    /// Builder to create a standard Pest API call
    /// </summary>
    public class RequestBuilder
    {
        #region Fields

        private RestClient _client;
        private RestRequest? _request;

        #endregion

        #region JSON

        private static readonly DefaultContractResolver resolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        private static readonly JsonSerializerSettings? serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = resolver,
            NullValueHandling = NullValueHandling.Ignore
        };

        #endregion

        private RequestBuilder(RestClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Init builder with ready-to-use rest client and API url
        /// </summary>
        /// <param name="client">Rest client, ready to use</param>
        /// <param name="api">API to call</param>
        /// <returns>Self</returns>
        public static RequestBuilder Make(RestClient client, string api)
        {
            var builder = new RequestBuilder(client);
            builder._request = new RestRequest(api);

            return builder;
        }

        /// <summary>
        /// Add HTTP parameters to the request
        /// </summary>
        /// <param name="parameters">List of parameters</param>
        /// <returns>Self</returns>
        public RequestBuilder WithParams(params KeyValuePair<string, string?>[] parameters)
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");

            if (parameters != null)
            {
                foreach (var param in parameters)
                    _request.AddOrUpdateParameter(param.Key, param.Value);
            }

            return this;
        }

        public RequestBuilder WithBody(object? body)
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");

            if (body != null)
                _request.AddBody(body);

            return this;
        }

        public T? Post<T>() where T : IBaseResponse
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");

            var response = _client.Post(_request);
            var content = response.Content;

            var respObj = DeserializeResponse<T>(content);

            return respObj;
        }

        public string? Post()
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");

            var response = _client.Post(_request);
            var content = response.Content;

            return content;
        }

        /// <summary>
        /// Simple get request to check HTTP status only.
        /// </summary>
        /// <returns>True if 200 status message received</returns>
        public bool Get()
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");

            var response = _client.Get(_request);

            return response.IsSuccessful;
        }

        public T? Get<T>()
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");
            var response = _client.Get(_request);

            var respObj = DeserializeResponse<T>(response.Content);

            return respObj;
        }

        public async Task<T?> GetAsync<T>()
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");

            var response = await _client.GetAsync(_request);

            var respObj = DeserializeResponse<T>(response.Content);

            return respObj;
        }

        /// <summary>
        /// Simple get request to check HTTP status and some additional data in response. 
        /// </summary>
        /// <returns>True if 200 status message received and needed data is in headers</returns>
        public bool Get(Predicate<RestResponse> predicate)
        {
            if (_client == null) throw new ArgumentNullException("client");
            if (_request == null) throw new ArgumentNullException("request");

            var response = _client.Get(_request);

            if (!response.IsSuccessful)
                return false;

            return predicate(response);
        }

        private T? DeserializeResponse<T>(string? json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException($"{nameof(json)}");

            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }
    }
}
