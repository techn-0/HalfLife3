using System.Threading;
using _02_Scripts.Http.Components;
using UnityEngine;

// 수정된 네임스페이스로 변경

namespace _02_Scripts.Http // Components 네임스페이스 추가
{
    public class GithubExampleSimple : MonoBehaviour
    {
        [SerializeField] private string token = ""; // GitHub PAT
        [SerializeField] private string userAgent = "Demo/1.0"; // 필수(User-Agent)
        [SerializeField] private string owner = "oak_oak-cassia";
        [SerializeField] private string repo = "RTW-Server";

        private GithubClient _gh;

        private async void Start() // async void: Unity lifecycle 메서드에서 OK
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("GitHub token이 비어있습니다.");
                return;
            }

            _gh = new GithubClient(token, userAgent);
            var service = new GithubService(_gh);

            // 오늘 커밋 수 조회
            var count = await service.GetTodayCommitCountAsync(owner, repo, author: "", CancellationToken.None);
            Debug.Log($"오늘 커밋 수: {count}");

            // 기존 이슈 조회도 함께 유지 (필요시)
            var resp = await _gh.ListIssuesAsync(owner, repo, "open", CancellationToken.None);
            if (resp.Ok && resp.Data != null)
            {
                Debug.Log($"오픈 이슈 수: {resp.Data.Length}");
                foreach (var issue in resp.Data)
                {
                    if (issue.IsPullRequest) continue; // PR 제외
                    Debug.Log($"#{issue.Number} {issue.Title} by {issue.User?.Login}");
                }
            }
        }
    }
}