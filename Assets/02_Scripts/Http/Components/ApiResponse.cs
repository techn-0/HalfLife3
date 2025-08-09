using System.Collections.Generic;
#nullable enable

namespace _02_Scripts.Http.Components
{
    public class ApiResponse<T>
    {
        public long Status { get; }
        public T? Data { get; }
        public string Raw { get; }
        public IDictionary<string, string> Headers { get; }
        public bool Ok => Status >= 200 && Status <= 299;

        public ApiResponse(long status, T? data, string raw, IDictionary<string, string> headers)
        {
            Status = status;
            Data = data;
            Raw = raw;
            Headers = headers;
        }
    }
}