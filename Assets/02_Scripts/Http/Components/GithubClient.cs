using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace _02_Scripts.Http.Components
{
    public sealed class GithubClient
    {
        private const string BASE = "https://api.github.com";

        private readonly string _token;
        private readonly string _userAgent;

        public GithubClient(string token, string userAgent)
        {
            _token = token;
            _userAgent = userAgent;
        }

        private Dictionary<string, string> CommonHeaders() => new()
        {
            ["Authorization"] = $"Bearer {_token}",
            ["Accept"] = "application/vnd.github+json", // GitHub 권장
            ["User-Agent"] = _userAgent, // 필수
            ["X-GitHub-Api-Version"] = "2022-11-28" // 명시 권장
        };

        // GET /repos/{owner}/{repo}/issues?state=...
        public Awaitable<ApiResponse<Issue[]>> ListIssuesAsync(
            string owner,
            string repo,
            string state,
            CancellationToken ct)
        {
            var req = new ApiRequest(
                HttpMethod.Get, BASE, $"/repos/{owner}/{repo}/issues",
                new Dictionary<string, string> { ["state"] = state },
                jsonBody: null,
                headers: CommonHeaders()
            );
            return UnityHttpClient.SendAsync<Issue[]>(req, ct);
        }

        // POST /repos/{owner}/{repo}/issues  (body: { title, body })
        public Awaitable<ApiResponse<Issue>> CreateIssueAsync(
            string owner,
            string repo,
            NewIssue body,
            CancellationToken ct)
        {
            var req = new ApiRequest(
                HttpMethod.Post, BASE, $"/repos/{owner}/{repo}/issues",
                query: null,
                jsonBody: body,
                headers: CommonHeaders()
            );
            return UnityHttpClient.SendAsync<Issue>(req, ct);
        }

        // GET /rate_limit (레이트리밋 확인)
        public Awaitable<ApiResponse<RateLimit>> GetRateLimitAsync(CancellationToken ct)
        {
            var req = new ApiRequest(
                HttpMethod.Get, BASE, "/rate_limit",
                query: null,
                jsonBody: null,
                headers: CommonHeaders()
            );
            return UnityHttpClient.SendAsync<RateLimit>(req, ct);
        }

        // GET /repos/{owner}/{repo} -> RepositoryInfo로 파싱
        public Awaitable<ApiResponse<RepositoryInfo>> GetRepositoryAsync(
            string owner,
            string repo,
            CancellationToken ct)
        {
            var req = new ApiRequest(
                HttpMethod.Get, BASE, $"/repos/{owner}/{repo}",
                query: null, jsonBody: null, headers: CommonHeaders()
            );
            return UnityHttpClient.SendAsync<RepositoryInfo>(req, ct);
        }

        // GET /repos/{owner}/{repo}/commits (기존 메서드 유지 - 호환용)
        public Awaitable<ApiResponse<string>> ListCommitsRawAsync(
            string owner,
            string repo,
            DateTimeOffset? sinceUtc,
            DateTimeOffset? untilUtc,
            string author,
            int perPage,
            CancellationToken ct)
        {
            return ListCommitsRawAsync(owner, repo, sinceUtc, untilUtc, author, perPage, branchOrSha: null, ct);
        }

        // GET /repos/{owner}/{repo}/commits (브랜치/커밋 SHA를 받을 수 있는 오버로드)
        public Awaitable<ApiResponse<string>> ListCommitsRawAsync(
            string owner,
            string repo,
            DateTimeOffset? sinceUtc,
            DateTimeOffset? untilUtc,
            string author,
            int perPage,
            string branchOrSha,
            CancellationToken ct)
        {
            var query = new Dictionary<string, string>();
            if (sinceUtc.HasValue) query["since"] = sinceUtc.Value.ToString("o"); // ISO-8601
            if (untilUtc.HasValue) query["until"] = untilUtc.Value.ToString("o"); // ISO-8601
            if (!string.IsNullOrEmpty(author)) query["author"] = author;
            if (!string.IsNullOrEmpty(branchOrSha)) query["sha"] = branchOrSha; // ★ 브랜치/커밋 선택
            query["per_page"] = Math.Max(1, perPage).ToString();

            var req = new ApiRequest(
                HttpMethod.Get, BASE, $"/repos/{owner}/{repo}/commits",
                query: query, jsonBody: null, headers: CommonHeaders()
            );
            return UnityHttpClient.SendAsync<string>(req, ct);
        }
    }
}