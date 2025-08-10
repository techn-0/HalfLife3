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
        [SerializeField] private string branchOrSha = ""; // 빈 값이면 default branch 사용

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

            // 레포 메타 정보 조회
            var (exists, isPrivate, defaultBranch) = await service.GetRepositoryMetaAsync(owner, repo, CancellationToken.None);
            Debug.Log($"exists={exists}, isPrivate={isPrivate}, defaultBranch={defaultBranch ?? "(unknown)"}");

            if (!exists) return;

            var branchToUse = string.IsNullOrEmpty(branchOrSha)
                ? (defaultBranch ?? "")
                : branchOrSha;

            // 오늘 커밋 수 조회 (브랜치 지정)
            var count = await service.GetTodayCommitCountAsync(owner, repo, author: "", branchToUse, CancellationToken.None);
            Debug.Log($"[{(string.IsNullOrEmpty(branchToUse) ? "default" : branchToUse)}] 오늘 커밋 수: {count}");
        }
    }
}