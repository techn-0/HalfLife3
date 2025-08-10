#nullable enable

using System.Collections.Generic;
using System.Net.Http;

namespace _02_Scripts.Http.Components
{
    public sealed class ApiRequest
    {
        public HttpMethod Method { get; }
        public string BaseUrl { get; }
        public string Path { get; }
        public IDictionary<string,string>? Query { get; }
        public object? JsonBody { get; }
        public IDictionary<string, string>? Headers { get; }

        public ApiRequest(HttpMethod method, string baseUrl, string path, IDictionary<string, string>? query = null, object? jsonBody = null, IDictionary<string, string>? headers = null)
        {
            Method = method;
            BaseUrl = baseUrl;
            Path = path;
            Query = query;
            JsonBody = jsonBody;
            Headers = headers;
        }
    }

}