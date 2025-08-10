using UnityEngine;
using _02_Scripts.Reward;

/// <summary>
/// 게임 시작 시 출석 보상 시스템을 자동으로 초기화하는 헬퍼 클래스
/// Main Scene에 하나만 배치하면 자동으로 출석 보상 시스템이 동작함
/// </summary>
public class AttendanceRewardInitializer : MonoBehaviour
{
    [Header("자동 생성 설정")]
    [SerializeField] private bool autoCreateAttendanceManager = true;
    [SerializeField] private string attendanceManagerName = "AttendanceRewardManager";
    
    [Header("출석 보상 기본 설정")]
    [SerializeField] private long defaultDailyCoins = 100;
    [SerializeField] private string defaultMessage = "출석 보상이 지급!";
    [SerializeField] private float defaultPopupDuration = 5f;
    
    [Header("팝업 설정")]
    [SerializeField] private GameObject attendancePopupPrefab;
    [SerializeField] private Transform popupParent;
    
    private AttendanceRewardManager attendanceManager;
    
    private void Start()
    {
        InitializeAttendanceRewardSystem();
    }
    
    /// <summary>
    /// 출석 보상 시스템 초기화
    /// </summary>
    private void InitializeAttendanceRewardSystem()
    {
        // 기존 AttendanceRewardManager 찾기
        attendanceManager = FindObjectOfType<AttendanceRewardManager>();
        
        if (attendanceManager == null && autoCreateAttendanceManager)
        {
            // 자동으로 AttendanceRewardManager 생성
            CreateAttendanceRewardManager();
        }
        else if (attendanceManager != null)
        {
            Debug.Log("[AttendanceRewardInitializer] 기존 AttendanceRewardManager 발견");
        }
        
        if (attendanceManager != null)
        {
            Debug.Log("[AttendanceRewardInitializer] 출석 보상 시스템 초기화 완료");
            
            // 게임 시작 즉시 출석 보상 확인 (약간의 지연 후)
            Invoke(nameof(CheckInitialAttendance), 0.5f);
        }
        else
        {
            Debug.LogError("[AttendanceRewardInitializer] AttendanceRewardManager 초기화 실패");
        }
    }
    
    /// <summary>
    /// AttendanceRewardManager GameObject 생성 및 설정
    /// </summary>
    private void CreateAttendanceRewardManager()
    {
        GameObject managerObject = new GameObject(attendanceManagerName);
        attendanceManager = managerObject.AddComponent<AttendanceRewardManager>();
        
        // DontDestroyOnLoad 설정 (씬 전환 시에도 유지)
        DontDestroyOnLoad(managerObject);
        
        // Reflection을 사용하여 private 필드 설정
        SetAttendanceManagerFields();
        
        Debug.Log($"[AttendanceRewardInitializer] {attendanceManagerName} 자동 생성 완료");
    }
    
    /// <summary>
    /// AttendanceRewardManager의 필드들을 설정
    /// (SerializeField이므로 Inspector에서 설정하는 것과 같은 효과)
    /// </summary>
    private void SetAttendanceManagerFields()
    {
        if (attendanceManager == null) return;
        
        // Reflection을 사용하여 SerializeField 값들 설정
        var type = typeof(AttendanceRewardManager);
        
        // dailyAttendanceCoins 설정
        var coinsField = type.GetField("dailyAttendanceCoins", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        coinsField?.SetValue(attendanceManager, defaultDailyCoins);
        
        // attendanceMessage 설정
        var messageField = type.GetField("attendanceMessage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        messageField?.SetValue(attendanceManager, defaultMessage);
        
        // popupDisplayDuration 설정
        var durationField = type.GetField("popupDisplayDuration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        durationField?.SetValue(attendanceManager, defaultPopupDuration);
        
        // attendancePopupPrefab 설정
        var prefabField = type.GetField("attendancePopupPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        prefabField?.SetValue(attendanceManager, attendancePopupPrefab);
        
        // popupParent 설정
        var parentField = type.GetField("popupParent", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        parentField?.SetValue(attendanceManager, popupParent);
        
        // enableDebugLogs 설정
        var debugField = type.GetField("enableDebugLogs", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        debugField?.SetValue(attendanceManager, true);
        
        Debug.Log("[AttendanceRewardInitializer] AttendanceRewardManager 필드 설정 완료");
    }
    
    #region Public Interface
    /// <summary>
    /// 수동으로 출석 보상 확인 (버튼 등에서 호출 가능)
    /// </summary>
    public void ManualCheckAttendance()
    {
        if (attendanceManager != null)
        {
            attendanceManager.ManualCheckAttendance();
        }
        else
        {
            Debug.LogWarning("[AttendanceRewardInitializer] AttendanceRewardManager가 없습니다.");
        }
    }
    
    /// <summary>
    /// 현재 출석 보상 매니저 참조 반환
    /// </summary>
    public AttendanceRewardManager GetAttendanceManager()
    {
        return attendanceManager;
    }
    
    /// <summary>
    /// 게임 시작 시 초기 출석 보상 확인
    /// </summary>
    private void CheckInitialAttendance()
    {
        if (attendanceManager != null)
        {
            Debug.Log("[AttendanceRewardInitializer] 게임 시작 시 출석 보상 확인 중...");
            attendanceManager.ManualCheckAttendance();
        }
        else
        {
            Debug.LogWarning("[AttendanceRewardInitializer] AttendanceRewardManager가 없어서 출석 확인을 할 수 없습니다.");
        }
    }
    #endregion
    
    #region Debug Methods
    [ContextMenu("Debug/Manual Check Attendance")]
    private void CM_ManualCheckAttendance()
    {
        ManualCheckAttendance();
    }
    
    [ContextMenu("Debug/Reinitialize System")]
    private void CM_ReinitializeSystem()
    {
        InitializeAttendanceRewardSystem();
    }
    #endregion
}
