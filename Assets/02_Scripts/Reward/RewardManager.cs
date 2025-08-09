using System;
using System.Collections.Generic;
using UnityEngine;
using _02_Scripts.Shop;

namespace _02_Scripts.Reward
{
    /// <summary>
    /// RewardManager
    /// - 진행(Progress), 목표(Goal), 수령 여부(Claimed)를 관리.
    /// - DailyQuestManager는 "수행"을 위한 것, 본 매니저는 "보상"을 위한 것.
    /// - 외부에서 SetTrackCount 호출로 각 RewardType별 목표를 설정합니다.
    /// </summary>
    public sealed class RewardManager : MonoBehaviour
    {
        // ==== Singleton ====
        private static RewardManager _instance;
        public static RewardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RewardManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("RewardManager");
                        _instance = go.AddComponent<RewardManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // ==== State ====
        // Count와 Received를 튜플로 통합 관리 (Dictionary로 변경)
        private readonly Dictionary<RewardType, (int count, bool received)> _rewards = new();
        private readonly Dictionary<RewardType, int> _trackCounts = new(); // Track Count (목표)

        // Coin reward per track
        private const int COIN_REWARD_PER_TRACK = 1000;

        // TODO: 코인 관련
        // TODO: 트랙 관련 provider

        // ===== Constructor (Private) =====
        private RewardManager()
        {
            // 생성자에서 초기화 작업
            Debug.Log("[RewardManager] 싱글턴 인스턴스 생성됨");
        }

        // ===== Unity Lifecycle =====
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[RewardManager] 싱글턴 인스턴스 생성됨");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 특정 RewardType의 Track Count(목표)를 설정합니다.
        /// Daily의 경우 _rewards에도 초기 상태로 등록합니다.
        /// </summary>
        public void SetTrackCount(RewardType type, int trackCount)
        {
            _trackCounts[type] = Math.Max(1, trackCount);

            // Daily인 경우 _rewards에 초기 상태로 등록
            if (type == RewardType.Daily)
            {
                // 이미 존재하지 않는 경우만 초기 상태로 추가
                if (!_rewards.ContainsKey(type))
                {
                    _rewards[type] = (0, false); // count: 0, received: false
                    Debug.Log($"[Daily] _rewards에 초기 상태로 등록됨 - Count: 0, Received: false");
                }
            }
        }

        /// <summary>
        /// Track Count(목표) 조회
        /// </summary>
        public int GetTrackCount(RewardType type) =>
            _trackCounts.GetValueOrDefault(type, 1);

        /// <summary>해당 미션의 현재 카운트</summary>
        public int GetCount(RewardType type) =>
            _rewards.TryGetValue(type, out var reward)
                ? reward.count
                : 0;

        /// <summary>해당 미션의 수령 여부</summary>
        public bool IsReceived(RewardType type) =>
            _rewards.TryGetValue(type, out var reward) && reward.received;

        /// <summary>
        /// RewardType에 따른 목표값을 계산합니다.
        /// Daily인 경우: Track Count 사용
        /// 다른 타입인 경우: 기본값 1 사용 (향후 확장 가능)
        /// </summary>
        private int GetGoal(RewardType type)
        {
            return type switch
            {
                RewardType.Daily => Math.Max(1, GetTrackCount(type)),
                _ => 1 // 기본값, 향후 다른 타입별로 확장 가능
            };
        }

        /// <summary>해당 미션의 진행도 스냅샷(Count/Goal/Completed/Received/Receivable)</summary>
        public RewardProgress GetProgress(RewardType type)
        {
            int cnt = GetCount(type);
            int goal = GetGoal(type);
            bool received = IsReceived(type);
            return new RewardProgress(type, cnt, goal, received);
        }

        /// <summary>여러 미션의 진행도 스냅샷을 한 번에</summary>
        public RewardProgress[] GetAllProgress(params RewardType[] types)
        {
            if (types == null || types.Length == 0) return Array.Empty<RewardProgress>();
            var list = new List<RewardProgress>(types.Length);
            foreach (var t in types) list.Add(GetProgress(t));
            return list.ToArray();
        }

        /// <summary>완료 여부(Count >= Goal)</summary>
        public bool IsCompleted(RewardType type) => GetProgress(type).Completed;

        /// <summary>수령 가능 여부(Completed && !Received)</summary>
        public bool CanReceive(RewardType type) => GetProgress(type).Receivable;

        /// <summary>현재 전체 카운트의 복사본을 반환 (읽기 전용 스냅샷)</summary>
        public IReadOnlyDictionary<RewardType, int> GetCountsSnapshot()
        {
            var copy = new Dictionary<RewardType, int>(_rewards.Count);
            foreach (var kv in _rewards) copy[kv.Key] = kv.Value.count;
            return copy;
        }

        // =========================
        // Command (갱신)
        // =========================

        /// <summary>카운트를 특정 값으로 설정</summary>
        public int SetCount(RewardType type, int value)
        {
            int goal = GetGoal(type);
            int clampedValue = Math.Max(0, Math.Min(value, goal));

            if (_rewards.TryGetValue(type, out var current))
            {
                _rewards[type] = (clampedValue, current.received);
            }
            else
            {
                _rewards[type] = (clampedValue, false);
            }

            return clampedValue;
        }

        /// <summary>카운트를 증가</summary>
        public int Increase(RewardType type, int delta = 1)
        {
            int goal = GetGoal(type);

            if (_rewards.TryGetValue(type, out var current))
            {
                int newCount = Math.Max(0, Math.Min(current.count + delta, goal));
                _rewards[type] = (newCount, current.received);
                return newCount;
            }
            else
            {
                int newCount = Math.Max(0, Math.Min(delta, goal));
                _rewards[type] = (newCount, false);
                return newCount;
            }
        }

        /// <summary>
        /// 수령 시도(Receive). 
        /// - Completed가 아니면 false
        /// - 미수령 -> 수령 처리하고 true
        /// - 이미 수령됨 -> false
        /// </summary>
        public bool TryReceive(RewardType type, out RewardProgress progress)
        {
            progress = GetProgress(type);
            if (!progress.Completed) return false;

            // 현재 상태 확인
            if (_rewards.TryGetValue(type, out var current))
            {
                if (current.received) return false; // 이미 수령됨

                // 수령 처리
                _rewards[type] = (current.count, true);
                progress = GetProgress(type);
                
                // 코인 보상 지급 (Track Count * 1000)
                int trackCount = GetTrackCount(type);
                long coinReward = trackCount * COIN_REWARD_PER_TRACK;
                CoinManager.AddCoins(coinReward);
                
                Debug.Log($"[RewardManager] {type} 보상 수령 완료! 코인 {coinReward} 지급됨 (Track Count: {trackCount})");
                
                return true;
            }

            // 키가 없으면 완료된 상태로 수령 처리
            _rewards[type] = (progress.Count, true);
            progress = GetProgress(type);
            
            // 코인 보상 지급 (Track Count * 1000)
            int trackCountForNewKey = GetTrackCount(type);
            long coinRewardForNewKey = trackCountForNewKey * COIN_REWARD_PER_TRACK;
            CoinManager.AddCoins(coinRewardForNewKey);
            
            Debug.Log($"[RewardManager] {type} 보상 수령 완료! 코인 {coinRewardForNewKey} 지급됨 (Track Count: {trackCountForNewKey})");
            
            return true;
        }

        /// <summary>수령 상태 강제 설정(운영/테스트 용)</summary>
        public void SetReceived(RewardType type, bool value)
        {
            if (_rewards.TryGetValue(type, out var current))
            {
                _rewards[type] = (current.count, value);
            }
            else
            {
                _rewards[type] = (0, value);
            }
        }

        public void RewardReset(RewardType type)
        {
            _rewards.Remove(type);
        }

        /// <summary>하루 자정 타이머가 호출하는 전체 초기화</summary>
        [ContextMenu("Reset Today")]
        public void ResetToday()
        {
            _rewards.Clear();
        }
    }
}