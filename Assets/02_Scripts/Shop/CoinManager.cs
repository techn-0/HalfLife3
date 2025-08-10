// CoinManager.cs

using System;
using UnityEngine;
using _02_Scripts.Common;

namespace _02_Scripts.Shop
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance { get; private set; }

        // ===== Public API =====
        public static long Balance => Instance?._balance ?? 0;

        /// <summary>코인을 증가시킵니다 (increase).</summary>
        public static void AddCoins(long amount) => Instance?.Add(amount);

        /// <summary>코인을 소모 시도합니다 (consume). 성공 시 true.</summary>
        public static bool TrySpendCoins(long amount) => Instance && Instance.TrySpend(amount);

        /// <summary>코인을 특정 값으로 설정합니다 (치트/테스트용).</summary>
        public static void SetCoins(long amount) => Instance?.SetBalance(amount);

        /// <summary>코인을 0으로 초기화합니다 (치트/테스트용).</summary>
        public static void ResetCoins() => Instance?.SetBalance(0);

        /// <summary>잔액 변경 시 호출되는 콜백 (English: balance changed event)</summary>
        public event Action<long> OnBalanceChanged;

        // ===== Config =====
        [SerializeField] private string saveFileName = "coin.json"; // 파일명 변경 가능
        private const int SAVE_VERSION = 1;

        // ===== State =====
        private long _balance;
        private string _savePath;

        [Serializable]
        private struct SaveData
        {
            public int ver;
            public long coin;
        }

        // ===== Lifecycle =====
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
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) Save(); // 백그라운드로 갈 때 저장
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        // ===== Core =====
        private void Add(long amount)
        {
            if (amount <= 0) return; // 음수/0 무시
            try
            {
                checked
                {
                    _balance += amount;
                }
            }
            catch (OverflowException)
            {
                _balance = long.MaxValue; // 오버플로 방지
            }

            OnBalanceChanged?.Invoke(_balance);
            Save();
        }

        private bool TrySpend(long amount)
        {
            if (amount <= 0) return false;
            if (_balance < amount) return false;

            _balance -= amount;
            OnBalanceChanged?.Invoke(_balance);
            Save();
            return true;
        }

        private void SetBalance(long amount)
        {
            if (amount < 0) return; // 음수는 거부
            _balance = amount;
            OnBalanceChanged?.Invoke(_balance);
            Save();
        }

        // ===== Persistence =====
        private void Load()
        {
            var data = JsonStorage.LoadOrDefault(_savePath, () => new SaveData { ver = SAVE_VERSION, coin = 0 });

            if (data.ver != SAVE_VERSION)
            {
                // 버전 규칙이 바뀌면 여기서 마이그레이션 처리 가능
                _balance = data.coin;
            }
            else
            {
                _balance = data.coin;
            }
        }

        private void Save()
        {
            var data = new SaveData
            {
                ver = SAVE_VERSION,
                coin = _balance
            };

            JsonStorage.Save(data, _savePath);
        }
    }
}