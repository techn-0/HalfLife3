using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using _02_Scripts.Shop;

namespace _02_Scripts.Reward
{
    /// <summary>
    /// 출석 보상 관리 클래스
    /// DateManager와 연동하여 새로운 날 첫 접속 시 출석 보상을 자동으로 지급
    /// 상단 팝업으로 보상 지급 알림 표시
    /// </summary>
    public class AttendanceRewardManager : MonoBehaviour
    {
        [Header("출석 보상 설정")]
        [SerializeField] private long dailyAttendanceCoins = 1000;
        [SerializeField] private string attendanceMessage = "출석 보상!";

        [Header("팝업 설정")]
        [SerializeField] private float popupDisplayDuration = 5f;
        [SerializeField] private GameObject attendancePopupPrefab;
        [SerializeField] private Transform popupParent;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = true;

        // 현재 활성화된 팝업 인스턴스
        private GameObject currentPopupInstance;

        // 오늘 출석 보상 수령 여부 저장 키
        private string TodayAttendanceKey => $"AttendanceReward_{DateManager.Instance.GetCurrentDateString()}";

        #region Unity Lifecycle
        private void Start()
        {
            // DateManager의 새로운 날 이벤트 구독
            DateManager.Instance.OnNewDayFirstLogin += HandleNewDayFirstLogin;

            if (enableDebugLogs)
            {
                Debug.Log("[AttendanceRewardManager] 출석 보상 시스템 초기화 완료");
                Debug.Log($"[AttendanceRewardManager] 오늘 출석 보상 수령 여부: {HasReceivedTodayAttendance()}");
            }
        }

        private void Update()
        {
            // ---------------------------------------------------------초기화 디버깅---------------------------------------------------------
            // 키보드 1을 눌렀을 때 오늘 출석 기록 초기화 (새로운 Input System 사용)
            if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                // 출석 보상 수령 기록 초기화
                CM_ResetTodayAttendance();

                // 구매목록, 배치 초기화
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();

                // 퀘스트 초기화
                if (DailyQuestManager.Instance != null)
                {
                    DailyQuestManager.Instance.ClearAllQuests();
                    Debug.Log("[AttendanceRewardManager] 퀘스트 초기화 완료");
                }
                else
                {
                    Debug.LogWarning("[AttendanceRewardManager] DailyQuestManager.Instance가 null입니다.");
                }

                // 코인 초기화
                CoinManager.ResetCoins();
                
                // RewardManager 초기화 일일 관련
                RewardManager.Instance.ResetToday();

                // 씬 초기화
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.name);
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (DateManager.Instance != null)
            {
                DateManager.Instance.OnNewDayFirstLogin -= HandleNewDayFirstLogin;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 새로운 날 첫 접속 시 호출되는 이벤트 핸들러
        /// </summary>
        private void HandleNewDayFirstLogin(DateTime loginDate)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[AttendanceRewardManager] 새로운 날 첫 접속 감지: {loginDate:yyyy-MM-dd}");
            }

            CheckAndGiveAttendanceReward();
        }
        #endregion

        #region Attendance Reward Logic
        /// <summary>
        /// 출석 보상 확인 및 지급
        /// </summary>
        public void CheckAndGiveAttendanceReward()
        {
            if (HasReceivedTodayAttendance())
            {
                if (enableDebugLogs)
                {
                    Debug.Log("[AttendanceRewardManager] 오늘 이미 출석 보상을 받았습니다.");
                }
                return;
            }

            // 출석 보상 지급
            GiveAttendanceReward();
        }

        /// <summary>
        /// 출석 보상 지급 실행
        /// </summary>
        private void GiveAttendanceReward()
        {
            // 코인 지급
            CoinManager.AddCoins(dailyAttendanceCoins);

            // 출석 보상 수령 표시
            MarkTodayAttendanceAsReceived();

            // 팝업 표시
            ShowAttendancePopup();

            if (enableDebugLogs)
            {
                Debug.Log($"[AttendanceRewardManager] 출석 보상 지급 완료! 코인 {dailyAttendanceCoins} 지급됨");
            }
        }

        /// <summary>
        /// 오늘 출석 보상 수령 여부 확인
        /// </summary>
        public bool HasReceivedTodayAttendance()
        {
            return PlayerPrefs.GetInt(TodayAttendanceKey, 0) == 1;
        }

        /// <summary>
        /// 오늘 출석 보상 수령 표시
        /// </summary>
        private void MarkTodayAttendanceAsReceived()
        {
            PlayerPrefs.SetInt(TodayAttendanceKey, 1);
            PlayerPrefs.Save();

            if (enableDebugLogs)
            {
                Debug.Log($"[AttendanceRewardManager] 출석 보상 수령 기록 저장: {TodayAttendanceKey}");
            }
        }
        #endregion

        #region Popup Management
        /// <summary>
        /// 출석 보상 팝업 표시
        /// </summary>
        private void ShowAttendancePopup()
        {
            if (attendancePopupPrefab == null)
            {
                Debug.LogWarning("[AttendanceRewardManager] 출석 팝업 프리팹이 설정되지 않았습니다.");
                return;
            }

            // 기존 팝업이 있다면 제거
            if (currentPopupInstance != null)
            {
                Destroy(currentPopupInstance);
            }

            // 팝업 생성
            Transform parent = popupParent != null ? popupParent : transform;
            currentPopupInstance = Instantiate(attendancePopupPrefab, parent);

            // 팝업 활성화 (Coroutine 실행을 위해 필요)
            currentPopupInstance.SetActive(true);

            // AttendancePopupUI 컴포넌트가 있다면 설정
            var popupUI = currentPopupInstance.GetComponent<AttendancePopupUI>();
            if (popupUI != null)
            {
                popupUI.Initialize(attendanceMessage, dailyAttendanceCoins, popupDisplayDuration);
            }
            else
            {
                Debug.LogWarning("[AttendanceRewardManager] AttendancePopupUI 컴포넌트를 찾을 수 없습니다.");
            }

            // 일정 시간 후 자동 제거
            Destroy(currentPopupInstance, popupDisplayDuration);

            if (enableDebugLogs)
            {
                Debug.Log($"[AttendanceRewardManager] 출석 보상 팝업 표시: {popupDisplayDuration}초 후 자동 제거");
            }
        }
        #endregion

        #region Debug Methods
        [ContextMenu("Debug/Check Today Attendance")]
        private void CM_CheckTodayAttendance()
        {
            bool received = HasReceivedTodayAttendance();
            Debug.Log($"[AttendanceRewardManager] 오늘 출석 보상 수령 여부: {received}");
            Debug.Log($"[AttendanceRewardManager] 저장 키: {TodayAttendanceKey}");
        }

        [ContextMenu("Debug/Force Give Attendance Reward")]
        private void CM_ForceGiveAttendanceReward()
        {
            GiveAttendanceReward();
            Debug.Log("[AttendanceRewardManager] 강제 출석 보상 지급 실행");
        }

        [ContextMenu("Debug/Reset Today Attendance")]
        private void CM_ResetTodayAttendance()
        {
            PlayerPrefs.DeleteKey(TodayAttendanceKey);
            PlayerPrefs.Save();
            Debug.Log("[AttendanceRewardManager] 오늘 출석 보상 수령 기록 초기화");
        }

        [ContextMenu("Debug/Show Test Popup")]
        private void CM_ShowTestPopup()
        {
            ShowAttendancePopup();
            Debug.Log("[AttendanceRewardManager] 테스트 팝업 표시");
        }

        [ContextMenu("Debug/Test Full Attendance Flow")]
        private void CM_TestFullAttendanceFlow()
        {
            // 1. 먼저 오늘 출석 기록 초기화
            PlayerPrefs.DeleteKey(TodayAttendanceKey);
            PlayerPrefs.Save();

            // 2. DateManager를 어제로 설정
            DateManager.Instance.DebugSetLastLoginDate(DateTime.Now.AddDays(-1));

            // 3. 출석 보상 확인 실행
            CheckAndGiveAttendanceReward();

            Debug.Log("[AttendanceRewardManager] 전체 출석 보상 플로우 테스트 완료");
        }

        [ContextMenu("Debug/Simulate New Day Login")]
        private void CM_SimulateNewDayLogin()
        {
            // DateManager 어제로 설정 후 새로운 날 이벤트 시뮬레이션
            DateManager.Instance.DebugSetLastLoginDate(DateTime.Now.AddDays(-1));
            HandleNewDayFirstLogin(DateTime.Now);
            Debug.Log("[AttendanceRewardManager] 새로운 날 접속 시뮬레이션 완료");
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// 수동으로 출석 보상 확인 (다른 스크립트에서 호출 가능)
        /// </summary>
        public void ManualCheckAttendance()
        {
            CheckAndGiveAttendanceReward();
        }

        /// <summary>
        /// 현재 설정된 일일 출석 보상 코인 수량 반환
        /// </summary>
        public long GetDailyAttendanceCoins()
        {
            return dailyAttendanceCoins;
        }

        /// <summary>
        /// 현재 설정된 팝업 표시 시간 반환
        /// </summary>
        public float GetPopupDisplayDuration()
        {
            return popupDisplayDuration;
        }
        #endregion
    }
}
