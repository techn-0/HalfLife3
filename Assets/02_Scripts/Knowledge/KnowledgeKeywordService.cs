using System;
using System.Collections.Generic;
using System.Linq;
using _02_Scripts.Common;
using UnityEngine;

namespace _02_Scripts.Knowledge
{
    /// <summary>
    ///     키워드 학습 서비스
    ///     - 키워드 등록/삭제
    ///     - 복습 스케줄 관리 (1, 3, 7, 14, 30일)
    ///     - 오늘의 복습 키워드 조회
    /// </summary>
    public class KnowledgeKeywordService : MonoBehaviour
    {
        private const int SAVE_VERSION = 1;

        // 복습 스케줄 (일 단위)
        private static readonly int[] REVIEW_SCHEDULE = { 1, 3, 7, 14, 30 };

        // ===== Config =====
        [SerializeField] private string saveFileName = "keywords.json";

        // ===== State =====
        private readonly Dictionary<string, KeywordData> _keywords = new Dictionary<string, KeywordData>();
        private string _savePath;
        public static KnowledgeKeywordService Instance { get; private set; }

        // ===== Unity Lifecycle =====
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _savePath = JsonStorage.GetSavePath(saveFileName);
            Load();
            
            // DateManager의 새로운 날 이벤트 구독
            DateManager.Instance.OnNewDayFirstLogin += OnNewDayFirstLogin;
        }

        private void OnDestroy()
        {
            Save();
            
            // 이벤트 구독 해제
            if (DateManager.Instance != null)
            {
                DateManager.Instance.OnNewDayFirstLogin -= OnNewDayFirstLogin;
            }
        }
        
        /// <summary>새로운 날 첫 접속 시 호출되는 메서드</summary>
        private void OnNewDayFirstLogin(DateTime loginDate)
        {
            Debug.Log($"[KnowledgeKeywordService] 새로운 날 첫 접속 감지: {loginDate:yyyy-MM-dd}");
            ProcessOverdueKeywords();
        }

        // ===== Public API =====

        /// <summary>키워드 등록</summary>
        /// 외부에서 값이 들어왔는지 체크하고 false면 중복
        public bool AddKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return false;

            keyword = keyword.Trim();
            if (_keywords.ContainsKey(keyword))
            {
                Debug.LogWarning($"[KnowledgeKeywordService] 키워드 '{keyword}'는 이미 등록되어 있습니다.");
                return false;
            }

            KeywordData keywordData = new KeywordData(keyword, DateManager.Instance.CurrentSessionDate);
            _keywords[keyword] = keywordData;

            Save();
            Debug.Log($"[KnowledgeKeywordService] 키워드 '{keyword}' 등록 완료. 첫 복습일: {keywordData.reviewDates[0]}");

            return true;
        }

        /// <summary>키워드 삭제</summary>
        public bool RemoveKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return false;

            keyword = keyword.Trim();
            if (!_keywords.ContainsKey(keyword))
            {
                Debug.LogWarning($"[KnowledgeKeywordService] 키워드 '{keyword}'를 찾을 수 없습니다.");
                return false;
            }

            _keywords.Remove(keyword);
            Save();
            Debug.Log($"[KnowledgeKeywordService] 키워드 '{keyword}' 삭제 완료.");

            return true;
        }

        /// <summary>모든 키워드 삭제 (주로 테스트용)</summary>
        public void ClearAllKeywords()
        {
            int count = _keywords.Count;
            _keywords.Clear();
            Save();
            Debug.Log($"[KnowledgeKeywordService] 모든 키워드 삭제 완료 ({count}개).");
        }

        /// <summary>오늘의 복습 키워드 목록 조회</summary>
        public List<string> GetTodayReviewKeywords()
        {
            DateTime todayDate = DateManager.Instance.CurrentSessionDate;
            List<string> todayKeywords = new List<string>();
            bool hasChanges = false;

            // 하나의 반복문으로 복습일 처리와 오늘의 키워드 조회를 동시에 처리
            foreach (KeyValuePair<string, KeywordData> kvp in _keywords.ToList()) // ToList()로 복사본을 만들어 안전하게 수정
            {
                string keyword = kvp.Key;
                KeywordData keywordData = kvp.Value;

                // 완료된 키워드는 제외
                if (keywordData.isCompleted) continue;

                if (keywordData.currentReviewIndex < keywordData.reviewDates.Count)
                {
                    DateTime reviewDate = DateTime.Parse(keywordData.reviewDates[keywordData.currentReviewIndex]);

                    // 복습일을 넘긴 경우 (오늘보다 과거)
                    if (todayDate > reviewDate)
                    {
                        // 복습일을 오늘로 설정
                        keywordData.reviewDates[keywordData.currentReviewIndex] = DateManager.Instance.GetCurrentDateString();
                        _keywords[keyword] = keywordData;
                        hasChanges = true;

                        string nextReviewDate = keywordData.currentReviewIndex < keywordData.reviewDates.Count
                            ? keywordData.reviewDates[keywordData.currentReviewIndex]
                            : "완료";

                        Debug.Log($"[KnowledgeKeywordService] 키워드 '{keyword}' 복습일 초과로 인해 오늘로 설정. " +
                                  $"현재 단계: {keywordData.currentReviewIndex + 1}/{REVIEW_SCHEDULE.Length}, 복습일: {nextReviewDate}");
                    }

                    // 현재 복습 단계의 날짜가 오늘 또는 이전인지 확인 (복습일을 넘긴 경우 포함)
                    DateTime currentReviewDate = DateTime.Parse(keywordData.reviewDates[keywordData.currentReviewIndex]);
                    if (currentReviewDate <= todayDate)
                    {
                        todayKeywords.Add(keywordData.keyword);
                    }
                }
            }

            if (hasChanges)
            {
                Save();
            }

            Debug.Log($"[KnowledgeKeywordService] 오늘의 복습 키워드: {todayKeywords.Count}개");
            return todayKeywords;
        }

        /// <summary>복습일을 넘긴 키워드들을 처리 (내부적으로 GetTodayReviewKeywords에서 처리됨)</summary>
        private void ProcessOverdueKeywords()
        {
            // 이제 GetTodayReviewKeywords()에서 통합 처리되므로 별도 호출 불필요
            // 하지만 기존 호출 코드 호환성을 위해 유지
            DateTime todayDate = DateManager.Instance.CurrentSessionDate;
            bool hasChanges = false;

            foreach (KeyValuePair<string, KeywordData> kvp in _keywords.ToList())
            {
                string keyword = kvp.Key;
                KeywordData keywordData = kvp.Value;

                if (keywordData.isCompleted) continue;

                if (keywordData.currentReviewIndex < keywordData.reviewDates.Count)
                {
                    DateTime reviewDate = DateTime.Parse(keywordData.reviewDates[keywordData.currentReviewIndex]);

                    if (todayDate > reviewDate)
                    {
                        SetKeywordReviewToday(keyword);
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                Save();
            }
        }

        /// <summary>키워드의 복습일을 오늘로 설정 (등급은 그대로 유지)</summary>
        private void SetKeywordReviewToday(string keyword)
        {
            if (!_keywords.TryGetValue(keyword, out KeywordData keywordData)) return;

            // 등급은 그대로 두고 현재 복습일만 오늘로 설정
            if (keywordData.currentReviewIndex < keywordData.reviewDates.Count)
            {
                keywordData.reviewDates[keywordData.currentReviewIndex] = DateManager.Instance.GetCurrentDateString();
            }

            _keywords[keyword] = keywordData;

            string nextReviewDate = keywordData.currentReviewIndex < keywordData.reviewDates.Count
                ? keywordData.reviewDates[keywordData.currentReviewIndex]
                : "완료";

            Debug.Log($"[KnowledgeKeywordService] 키워드 '{keyword}' 복습일 초과로 인해 오늘로 설정. " +
                      $"현재 단계: {keywordData.currentReviewIndex + 1}/{REVIEW_SCHEDULE.Length}, 복습일: {nextReviewDate}");
        }

        /// <summary>복습일을 넘긴 키워드 수동 초기화 (디버그용)</summary>
        [ContextMenu("Debug - Reset Overdue Keywords")]
        private void DebugResetOverdueKeywords()
        {
            ProcessOverdueKeywords();
            Debug.Log("[KnowledgeKeywordService] 복습일 초과 키워드 초기화 완료");
        }

        /// <summary>키워드 복습 완료 처리</summary>
        public bool CompleteReview(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return false;

            keyword = keyword.Trim();
            if (!_keywords.TryGetValue(keyword, out KeywordData keywordData))
            {
                Debug.LogWarning($"[KnowledgeKeywordService] 키워드 '{keyword}'를 찾을 수 없습니다.");
                return false;
            }

            if (keywordData.isCompleted)
            {
                Debug.LogWarning($"[KnowledgeKeywordService] 키워드 '{keyword}'는 이미 모든 복습이 완료되었습니다.");
                return false;
            }

            // 다음 복습 단계로 진행
            keywordData.currentReviewIndex++;

            // 모든 복습 완료 체크
            if (keywordData.currentReviewIndex >= REVIEW_SCHEDULE.Length)
            {
                keywordData.isCompleted = true;
                Debug.Log($"[KnowledgeKeywordService] 키워드 '{keyword}' 모든 복습 완료!");
            }
            else
            {
                string nextReviewDate = keywordData.reviewDates[keywordData.currentReviewIndex];
                Debug.Log($"[KnowledgeKeywordService] 키워드 '{keyword}' 복습 완료. 다음 복습일: {nextReviewDate}");
            }

            _keywords[keyword] = keywordData;
            Save();

            return true;
        }

        /// <summary>전체 키워드 목록 조회</summary>
        public List<KeywordData> GetAllKeywords()
        {
            return _keywords.Values.ToList();
        }

        /// <summary>완료된 키워드 목록 조회 (5단계 모두 완료한 키워드)</summary>
        public List<KeywordData> GetCompletedKeywords()
        {
            return _keywords.Values.Where(k => k.isCompleted).ToList();
        }

        /// <summary>진행중인 키워드 목록 조회 (아직 완료되지 않은 키워드)</summary>
        public List<KeywordData> GetInProgressKeywords()
        {
            return _keywords.Values.Where(k => !k.isCompleted).ToList();
        }

        /// <summary>키워드 상세 정보 조회</summary>
        public KeywordData? GetKeywordData(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return null;

            keyword = keyword.Trim();
            return _keywords.TryGetValue(keyword, out KeywordData data)
                ? data
                : null;
        }

        /// <summary>복습 통계 조회</summary>
        public (int total, int completed, int inProgress) GetStatistics()
        {
            int total = _keywords.Count;
            int completed = _keywords.Values.Count(k => k.isCompleted);
            int inProgress = total - completed;

            return (total, completed, inProgress);
        }

        /// <summary>오늘 등록된 키워드 목록 조회</summary>
        public List<KeywordData> GetTodayRegisteredKeywords()
        {
            return _keywords.Values.Where(k => DateManager.Instance.IsToday(DateTime.Parse(k.registrationDate))).ToList();
        }

        /// <summary>특정 날짜에 등록된 키워드 수 조회</summary>
        public int GetKeywordCountByDate(DateTime date)
        {
            string dateString = date.ToString("yyyy-MM-dd");
            return _keywords.Values.Count(k => k.registrationDate == dateString);
        }

        /// <summary>최근 N일간 등록된 키워드 목록 조회</summary>
        public List<KeywordData> GetRecentKeywords(int days)
        {
            DateTime cutoffDate = DateManager.Instance.CurrentSessionDate.AddDays(-days);
            return _keywords.Values.Where(k => DateTime.Parse(k.registrationDate) >= cutoffDate).ToList();
        }

        // ===== Persistence =====
        private void Load()
        {
            SaveData data = JsonStorage.LoadOrDefault(_savePath, () => new SaveData
            {
                ver = SAVE_VERSION,
                keywords = new List<KeywordData>()
            });

            if (data.ver != SAVE_VERSION)
            {
                Debug.LogWarning($"[KnowledgeKeywordService] 버전 불일치. 현재: {SAVE_VERSION}, 파일: {data.ver}");
            }

            _keywords.Clear();
            if (data.keywords != null)
            {
                foreach (KeywordData keyword in data.keywords)
                {
                    _keywords[keyword.keyword] = keyword;
                }
            }

            Debug.Log($"[KnowledgeKeywordService] 키워드 데이터 로드 완료: {_keywords.Count}개");
        }

        private void Save()
        {
            SaveData data = new SaveData
            {
                ver = SAVE_VERSION,
                keywords = _keywords.Values.ToList()
            };

            JsonStorage.Save(data, _savePath);
        }


        // ===== Data Structures =====
        [Serializable]
        private struct SaveData
        {
            public int ver;
            public List<KeywordData> keywords;
        }

        [Serializable]
        public struct KeywordData
        {
            public string keyword;
            public string registrationDate; // yyyy-MM-dd 형식
            public List<string> reviewDates; // yyyy-MM-dd 형식 리스트
            public int currentReviewIndex; // 현재 복습 단계 (0~4)
            public bool isCompleted; // 모든 복습 완료 여부

            public KeywordData(string keyword, DateTime registrationDate)
            {
                this.keyword = keyword;
                this.registrationDate = registrationDate.ToString("yyyy-MM-dd");
                reviewDates = CalculateReviewDates(registrationDate);
                currentReviewIndex = 0;
                isCompleted = false;
            }

            private static List<string> CalculateReviewDates(DateTime registrationDate)
            {
                List<string> dates = new List<string>();
                foreach (int days in REVIEW_SCHEDULE)
                {
                    dates.Add(registrationDate.AddDays(days).ToString("yyyy-MM-dd"));
                }

                return dates;
            }
        }
    }
}
