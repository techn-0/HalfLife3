using System;
using UnityEngine;

/// <summary>
/// 게임 접속 날짜 정보를 전역으로 관리하는 싱글톤 클래스
/// 접속 날짜, 게임 시간 등을 관리하며 차후 출석 보상 및 자정 갱신 기능의 기반이 됨
/// 
/// ========== 사용법 가이드 ==========
/// 
/// 1. 기본 사용법:
///    var dateManager = DateManager.Instance;
///    string today = dateManager.GetCurrentDateString(); // "2025-08-09" 형식
///    bool isNewDay = dateManager.IsNewDay; // 새로운 날 여부
/// 
/// 2. 날짜 확인:
///    - dateManager.CurrentSessionDate : 현재 접속 날짜 (DateTime)
///    - dateManager.LastLoginDate : 마지막 접속 날짜 (DateTime)
///    - dateManager.SessionStartTime : 게임 시작 시간 (DateTime)
/// 
/// 3. 유틸리티 메서드:
///    - GetCurrentDateString() : 현재 날짜를 "yyyy-MM-dd" 형식으로 반환
///    - GetLastLoginDateString() : 마지막 접속 날짜를 문자열로 반환
///    - GetSessionDuration() : 게임 플레이 시간 반환
///    - IsToday(DateTime date) : 특정 날짜가 오늘인지 확인
///    - GetDaysBetween(date1, date2) : 두 날짜 사이의 일수 차이
/// 
/// 4. 이벤트 구독 (차후 확장용):
///    dateManager.OnNewDayFirstLogin += (date) => { /* 새로운 날 첫 접속 시 */ };
///    dateManager.OnDateChanged += (date) => { /* 날짜 변경 시 */ };
/// 
/// 5. 테스트 방법 (에디터 전용):
///    - Inspector에서 DateManager 컴포넌트 찾기
///    - 우클릭 → Debug 메뉴에서 테스트 기능 사용
///    - "Set Last Login to Yesterday" : 새로운 날 테스트
///    - "Print Date Info" : 현재 상태 출력
/// 
/// 주의사항:
/// - 자동으로 싱글톤 생성되므로 별도 생성 불필요
/// - DontDestroyOnLoad로 씬 전환 시에도 유지됨
/// - PlayerPrefs로 접속 날짜 영구 저장됨
/// 
/// =====================================
/// </summary>
public class DateManager : MonoBehaviour
{
    #region Singleton
    private static DateManager _instance;
    public static DateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DateManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("DateManager");
                    _instance = obj.AddComponent<DateManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Events
    /// <summary>
    /// 날짜가 변경되었을 때 발생하는 이벤트 (차후 자정 갱신에 활용)
    /// 
    /// 사용 예시:
    /// DateManager.Instance.OnDateChanged += (newDate) => {
    ///     Debug.Log($"날짜가 {newDate:yyyy-MM-dd}로 변경되었습니다!");
    ///     RefreshDailyContents();
    /// };
    /// 
    /// 활용: 자정 갱신, 일일 컨텐츠 리셋 등에 사용 (차후 구현 예정)
    /// </summary>
    public event System.Action<DateTime> OnDateChanged;
    
    /// <summary>
    /// 새로운 날에 첫 접속했을 때 발생하는 이벤트 (차후 출석 보상에 활용)
    /// 
    /// 사용 예시:
    /// DateManager.Instance.OnNewDayFirstLogin += (loginDate) => {
    ///     ShowAttendanceReward();
    ///     UpdateConsecutiveDays();
    ///     Debug.Log($"{loginDate:yyyy-MM-dd} 첫 접속!");
    /// };
    /// 
    /// 활용: 출석 보상, 연속 접속일 업데이트, 환영 메시지 등에 사용
    /// </summary>
    public event System.Action<DateTime> OnNewDayFirstLogin;
    #endregion

    #region Properties
    /// <summary>
    /// 현재 게임의 접속 날짜 (시간 제거된 DateTime)
    /// 
    /// 사용 예시:
    /// DateTime today = DateManager.Instance.CurrentSessionDate;
    /// if (someQuest.createdDate.Date == today) { /* 오늘 생성된 퀘스트 */ }
    /// 
    /// 주의: 시간 정보는 제거되고 날짜만 포함 (00:00:00)
    /// </summary>
    public DateTime CurrentSessionDate { get; private set; }
    
    /// <summary>
    /// 마지막으로 저장된 접속 날짜 (PlayerPrefs에서 로드)
    /// 
    /// 사용 예시:
    /// DateTime lastLogin = DateManager.Instance.LastLoginDate;
    /// int daysSince = (DateTime.Now.Date - lastLogin).Days;
    /// 
    /// 활용: 연속 접속일, 복귀 유저 판별 등에 사용
    /// </summary>
    public DateTime LastLoginDate { get; private set; }
    
    /// <summary>
    /// 오늘이 새로운 날인지 (이전 접속과 날짜가 다른지)
    /// 
    /// 사용 예시:
    /// if (DateManager.Instance.IsNewDay) {
    ///     // 새로운 날 첫 접속 시 처리
    ///     ShowWelcomeMessage();
    ///     ResetDailyContents();
    /// }
    /// 
    /// 활용: 일일 보상, 출석 체크, 컨텐츠 초기화 등에 사용
    /// </summary>
    public bool IsNewDay { get; private set; }
    
    /// <summary>
    /// 게임 시작 시간 (정확한 시분초 포함)
    /// 
    /// 사용 예시:
    /// DateTime startTime = DateManager.Instance.SessionStartTime;
    /// TimeSpan playTime = DateTime.Now - startTime;
    /// 
    /// 활용: 플레이 시간 추적, 세션 분석 등에 사용
    /// </summary>
    public DateTime SessionStartTime { get; private set; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSession();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log($"[DateManager] 게임 시작 - 날짜: {CurrentSessionDate:yyyy-MM-dd}, 새로운 날: {IsNewDay}");
    }
    #endregion

    #region Initialization
    private void InitializeSession()
    {
        SessionStartTime = DateTime.Now;
        CurrentSessionDate = SessionStartTime.Date; // 시간 제거하고 날짜만
        
        // 마지막 접속 날짜 로드
        LoadLastLoginDate();
        
        // 새로운 날인지 확인
        CheckIfNewDay();
        
        // 현재 날짜를 마지막 접속 날짜로 저장
        SaveCurrentDateAsLastLogin();
        
        Debug.Log($"[DateManager] 날짜 시스템 초기화 완료");
        Debug.Log($"[DateManager] 현재 접속 날짜: {CurrentSessionDate:yyyy-MM-dd}");
        Debug.Log($"[DateManager] 마지막 접속 날짜: {LastLoginDate:yyyy-MM-dd}");
        Debug.Log($"[DateManager] 새로운 날 여부: {IsNewDay}");
    }
    #endregion

    #region Date Management
    private void LoadLastLoginDate()
    {
        string lastLoginString = PlayerPrefs.GetString("LastLoginDate", "");
        
        if (string.IsNullOrEmpty(lastLoginString))
        {
            // 첫 접속 - 어제 날짜로 설정하여 새로운 날로 처리
            LastLoginDate = CurrentSessionDate.AddDays(-1);
            Debug.Log("[DateManager] 첫 접속 감지");
        }
        else
        {
            if (DateTime.TryParse(lastLoginString, out DateTime savedDate))
            {
                LastLoginDate = savedDate.Date;
                Debug.Log($"[DateManager] 마지막 접속 날짜 로드: {LastLoginDate:yyyy-MM-dd}");
            }
            else
            {
                // 파싱 실패 시 어제 날짜로 설정
                LastLoginDate = CurrentSessionDate.AddDays(-1);
                Debug.LogWarning("[DateManager] 저장된 날짜 파싱 실패, 새로운 날로 처리");
            }
        }
    }

    private void CheckIfNewDay()
    {
        IsNewDay = CurrentSessionDate.Date != LastLoginDate.Date;
        
        if (IsNewDay)
        {
            Debug.Log($"[DateManager] 새로운 날 감지! 이전: {LastLoginDate:yyyy-MM-dd}, 현재: {CurrentSessionDate:yyyy-MM-dd}");
            
            // 새로운 날 첫 접속 이벤트 발생 (차후 출석 보상에 활용)
            OnNewDayFirstLogin?.Invoke(CurrentSessionDate);
        }
        else
        {
            Debug.Log($"[DateManager] 같은 날 재접속: {CurrentSessionDate:yyyy-MM-dd}");
        }
    }

    private void SaveCurrentDateAsLastLogin()
    {
        string dateString = CurrentSessionDate.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString("LastLoginDate", dateString);
        PlayerPrefs.Save();
        
        Debug.Log($"[DateManager] 접속 날짜 저장: {dateString}");
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 현재 날짜를 문자열로 반환 (yyyy-MM-dd 형식)
    /// 
    /// 사용 예시:
    /// string today = DateManager.Instance.GetCurrentDateString(); // "2025-08-09"
    /// 
    /// 활용: 파일명, 데이터 키, UI 표시 등에 사용
    /// </summary>
    public string GetCurrentDateString()
    {
        return CurrentSessionDate.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 마지막 접속 날짜를 문자열로 반환 (yyyy-MM-dd 형식)
    /// 
    /// 사용 예시:
    /// string lastLogin = DateManager.Instance.GetLastLoginDateString();
    /// 
    /// 활용: 연속 접속일 계산, 휴식일 표시 등에 사용
    /// </summary>
    public string GetLastLoginDateString()
    {
        return LastLoginDate.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 게임 지속 시간을 반환
    /// 
    /// 사용 예시:
    /// TimeSpan playTime = DateManager.Instance.GetSessionDuration();
    /// string timeText = $"플레이 시간: {playTime.Hours}시간 {playTime.Minutes}분";
    /// 
    /// 활용: 플레이 타임 추적, 세션 관리 등에 사용
    /// </summary>
    public TimeSpan GetSessionDuration()
    {
        return DateTime.Now - SessionStartTime;
    }

    /// <summary>
    /// 특정 날짜가 오늘인지 확인
    /// 
    /// 사용 예시:
    /// DateTime someDate = DateTime.Parse("2025-08-09");
    /// bool isToday = DateManager.Instance.IsToday(someDate);
    /// 
    /// 활용: 퀘스트 날짜 검증, 이벤트 유효성 확인 등에 사용
    /// </summary>
    public bool IsToday(DateTime date)
    {
        return date.Date == CurrentSessionDate.Date;
    }

    /// <summary>
    /// 두 날짜 사이의 일수 차이를 반환
    /// 
    /// 사용 예시:
    /// int daysDiff = DateManager.Instance.GetDaysBetween(lastLogin, today);
    /// string message = $"{daysDiff}일 만에 접속하셨습니다!";
    /// 
    /// 활용: 연속 접속일, 휴식일, 이벤트 기간 계산 등에 사용
    /// </summary>
    public int GetDaysBetween(DateTime date1, DateTime date2)
    {
        return (int)(date2.Date - date1.Date).TotalDays;
    }
    #endregion

    #region Future Extensions
    // 차후 구현 예정 메서드들 (현재는 빈 구현)

    /// <summary>차후 자정 갱신 기능 구현 예정</summary>
    private void CheckMidnightUpdate()
    {
        // TODO: 자정 넘어갈 때 갱신 로직 구현 예정
    }

    /// <summary>차후 출석 보상 관련 기능 구현 예정</summary>
    private void HandleAttendanceReward()
    {
        // TODO: 출석 보상 로직 구현 예정
    }
    #endregion

    #region Debug Methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPrintDateInfo()
    {
        Debug.Log("=== Date Manager Info ===");
        Debug.Log($"현재 접속 날짜: {CurrentSessionDate:yyyy-MM-dd}");
        Debug.Log($"마지막 접속 날짜: {LastLoginDate:yyyy-MM-dd}");
        Debug.Log($"새로운 날 여부: {IsNewDay}");
        Debug.Log($"게임 시작 시간: {SessionStartTime:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"게임 지속 시간: {GetSessionDuration()}");
        Debug.Log("=============================");
    }

    /// <summary>테스트용: 마지막 접속 날짜를 강제로 설정</summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugSetLastLoginDate(DateTime date)
    {
        LastLoginDate = date.Date;
        string dateString = LastLoginDate.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString("LastLoginDate", dateString);
        PlayerPrefs.Save();
        
        Debug.Log($"[DateManager] 테스트용 마지막 접속 날짜 설정: {dateString}");
        
        // 새로운 날 여부 재확인
        CheckIfNewDay();
    }

    // ContextMenu 테스트 메서드들
    [ContextMenu("Debug/Print Date Info")]
    private void CM_PrintDateInfo()
    {
        DebugPrintDateInfo();
    }

    [ContextMenu("Debug/Set Last Login to Yesterday")]
    private void CM_SetLastLoginYesterday()
    {
        DebugSetLastLoginDate(DateTime.Now.AddDays(-1));
        Debug.Log("[DateManager] 마지막 접속을 어제로 설정 (새로운 날 테스트)");
    }

    [ContextMenu("Debug/Set Last Login to Today")]
    private void CM_SetLastLoginToday()
    {
        DebugSetLastLoginDate(DateTime.Now);
        Debug.Log("[DateManager] 마지막 접속을 오늘로 설정 (같은 날 테스트)");
    }

    [ContextMenu("Debug/Clear Login Data")]
    private void CM_ClearLoginData()
    {
        PlayerPrefs.DeleteKey("LastLoginDate");
        PlayerPrefs.Save();
        Debug.Log("[DateManager] 접속 데이터 초기화 (재시작하면 첫 접속으로 처리됨)");
    }
    #endregion
}
