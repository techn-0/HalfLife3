using System;
using Newtonsoft.Json;

namespace _02_Scripts.Http.Components
{
    [Serializable]
    public class Issue
    {
        [JsonProperty("number")]
        public int Number { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("state")]
        public string State { get; set; }
        
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
        
        [JsonProperty("body")]
        public string Body { get; set; }
        
        [JsonProperty("user")]
        public User User { get; set; }
        
        [JsonProperty("pull_request")]
        public object PullRequest { get; set; } // PR 구분용 (null이면 일반 이슈)

        // 편의 속성들
        [JsonIgnore]
        public bool IsPullRequest => PullRequest != null;
    }

    [Serializable]
    public class User
    {
        [JsonProperty("login")]
        public string Login { get; set; }
        
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
    }

    [Serializable]
    public class NewIssue
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("body")]
        public string Body { get; set; }

        public NewIssue(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }

    [Serializable]
    public class RateLimit
    {
        [JsonProperty("resources")]
        public RateLimitResources Resources { get; set; }
    }

    [Serializable]
    public class RateLimitResources
    {
        [JsonProperty("core")]
        public RateLimitInfo Core { get; set; }
    }

    [Serializable]
    public class RateLimitInfo
    {
        [JsonProperty("limit")]
        public int Limit { get; set; }
        
        [JsonProperty("remaining")]
        public int Remaining { get; set; }
        
        [JsonProperty("reset")]
        public long Reset { get; set; } // Unix timestamp
        
        [JsonProperty("used")]
        public int Used { get; set; }
    }

    [Serializable]
    public class RepositoryInfo
    {
        [JsonProperty("name")] 
        public string Name { get; set; }
        
        [JsonProperty("full_name")] 
        public string FullName { get; set; }

        // JSON 키 "private" -> C# 프로퍼티는 예약어 피해서 IsPrivate로
        [JsonProperty("private")] 
        public bool IsPrivate { get; set; }

        [JsonProperty("default_branch")] 
        public string DefaultBranch { get; set; }
    }
}
