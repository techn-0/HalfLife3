// CoinManager.cs

using System;
using System.IO;
using UnityEngine;

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

        /// <summary>잔액 변경 시 호출되는 콜백 (English: balance changed event)</summary>
        public event Action<long> OnBalanceChanged;

        // ===== Config =====
        [SerializeField] private string saveFileName = "coin.json"; // 파일명 변경 가능
        private const int SAVE_VERSION = 1;

        // ===== State =====
        private long _balance;
        private string _savePath;
        private readonly object _ioLock = new object();

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

            _savePath = Path.Combine(Application.persistentDataPath, saveFileName);
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

        // ===== Persistence =====
        private void Load()
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    _balance = 0;
                    return;
                }

                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
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
            catch
            {
                // 손상된 파일 등: 안전하게 초기화
                _balance = 0;
            }
        }

        private void Save()
        {
            // 크래시 안전성을 위해 임시 파일에 먼저 기록 후 교체
            lock (_ioLock)
            {
                try
                {
                    var data = new SaveData { ver = SAVE_VERSION, coin = _balance };
                    var json = JsonUtility.ToJson(data);

                    var dir = Path.GetDirectoryName(_savePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    var tmpPath = _savePath + ".tmp";
                    File.WriteAllText(tmpPath, json);

                    // 기존 파일 교체
                    if (File.Exists(_savePath))
                        File.Delete(_savePath);
                    File.Move(tmpPath, _savePath);
                }
                catch
                {
                    // 저장 실패는 조용히 무시 (다음 기회에 재시도)
                }
            }
        }
    }
}