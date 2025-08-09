#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json; // JsonUtility 대신 Newtonsoft.Json 사용

namespace _02_Scripts.Http.Components
{
    public static class UnityHttpClient
    {
        public static async Awaitable<ApiResponse<T?>> SendAsync<T>(ApiRequest req, CancellationToken ct)
        {
            // 바로 취소된 토큰이면 즉시 중단
            ct.ThrowIfCancellationRequested();

            var url = BuildUrl(req.BaseUrl, req.Path, req.Query);

            using var uwr = new UnityWebRequest(url, req.Method.Method);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            // JSON body 처리
            if (req.JsonBody != null)
            {
                var json = JsonConvert.SerializeObject(req.JsonBody); // JsonUtility.ToJson 대신 JsonConvert 사용
                uwr.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                uwr.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            }

            // 헤더 설정
            if (req.Headers != null)
            {
                foreach (var kv in req.Headers)
                {
                    uwr.SetRequestHeader(kv.Key, kv.Value);
                }
            }

            var op = uwr.SendWebRequest();

            // CancellationToken 등록 - 취소 시 실제 요청도 중단
            // uwr 대신 op를 통해 안전하게 취소 처리
            using var reg = ct.Register(() =>
            {
                try
                {
                    if (!op.isDone && op.webRequest != null)
                    {
                        op.webRequest.Abort();
                    }
                }
                catch
                {
                    /* 예외 무시 */
                }
            });

            // Unity 2023.1+의 표준 방식: CancellationToken과 함께 대기
            await Awaitable.FromAsyncOperation(op, ct);

            var raw = uwr.downloadHandler?.text ?? string.Empty;
            var headers = uwr.GetResponseHeaders() ?? new Dictionary<string, string>();

            // 응답 처리
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                T? data = typeof(T) == typeof(string)
                    ? (T)(object)raw
                    : JsonConvert.DeserializeObject<T>(raw); // JsonUtility.FromJson<T> 대신 JsonConvert 사용
                return new ApiResponse<T?>(uwr.responseCode, data, raw, headers);
            }
            else
            {
                return new ApiResponse<T?>(uwr.responseCode, default(T), raw, headers);
            }
        }

        private static string BuildUrl(string baseUrl, string path, IDictionary<string, string>? query)
        {
            var ub = new UriBuilder($"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}");

            if (query != null && query.Count > 0)
            {
                var queryString = string.Join("&", System.Linq.Enumerable.Select(query,
                    kv => $"{UnityWebRequest.EscapeURL(kv.Key)}={UnityWebRequest.EscapeURL(kv.Value)}"));
                ub.Query = queryString;
            }

            return ub.Uri.ToString();
        }
    }
}