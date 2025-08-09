using System;
using System.Text.RegularExpressions;
using System.Threading;
using _02_Scripts.Http.Components;
using Newtonsoft.Json.Linq;
using UnityEngine;
// 0/1건 판별용

namespace _02_Scripts.Http // example.Services에서 Services로 변경
{
    public sealed class GithubService
    {
        private readonly GithubClient _client;

        public GithubService(GithubClient client) => _client = client;

        // getTodayCommitCount
        public async Awaitable<int> GetTodayCommitCountAsync(
            string owner, string repo, string author /* null/empty면 전체 */, CancellationToken ct)
        {
            // Asia/Seoul의 "오늘" 경계 계산 → UTC ISO-8601
            var seoul = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
            var nowUtc = DateTimeOffset.UtcNow;
            var nowSeoul = TimeZoneInfo.ConvertTime(nowUtc, seoul);
            var startLocal = new DateTime(nowSeoul.Year, nowSeoul.Month, nowSeoul.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var startSeoul = new DateTimeOffset(startLocal, seoul.GetUtcOffset(startLocal));
            var endSeoul = startSeoul.AddDays(1);
            var sinceUtc = startSeoul.ToUniversalTime();
            var untilUtc = endSeoul.ToUniversalTime();

            // per_page=1 → Link rel="last"의 page=N == 총 개수
            var resp = await _client.ListCommitsRawAsync(owner, repo, sinceUtc, untilUtc, author, perPage: 1, ct);
            if (!resp.Ok) {
                Debug.LogWarning($"GitHub commits 실패: {resp.Status}\n{resp.Raw}");
                return 0;
            }

            if (resp.Headers.TryGetValue("Link", out var link))
            {
                // ...<...page=N>; rel="last"
                var m = Regex.Match(link, @"[?&]page=(\d+)>;\s*rel=""last""");
                if (m.Success && int.TryParse(m.Groups[1].Value, out var n))
                    return n;
            }

            // Link 없으면 0 또는 1건: Raw 배열 길이로 판정
            try {
                var arr = JArray.Parse(resp.Raw);
                return arr.Count; // 0 or 1
            } catch {
                return 0;
            }
        }
    }
}
