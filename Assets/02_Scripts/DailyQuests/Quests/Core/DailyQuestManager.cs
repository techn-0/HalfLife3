using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// DailyQuestManager
/// - 매일 08:00(로컬) 트랙당 1개 퀘스트 자동 생성(00:00으로 자동생성 시간을 변경하는게 좋을까요)
/// - 완료 조건: 버튼 클릭(검증 없음)
/// - 보상: 트랙 수별 총합 → 퀘스트당 균등 분배
/// - 저장: 날짜별 JSON (Application.persistentDataPath/DailyQuests/)
/// - UI/네트워크와 분리: 이벤트(Event) + 퍼블릭 API만 제공
/// </summary>
public sealed class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    [Header("Active Tracks (인스펙터에서 원하는 만큼 추가 가능)")]
    [SerializeField] private List<TrackType> activeTracks = new();   // 인스펙터에서 선택

    [Header("Generation Time (Local)")]
    [Range(0,23)] public int generateHour = 8; // 08:00

    [Header("Total Rewards by Track Count (3개 이상은 자동 확장)")]
    public int totalXp1 = 100;   public float totalCoin1 = 1f;
    public int totalXp2 = 220;   public float totalCoin2 = 2.2f;
    public int totalXp3 = 360;   public float totalCoin3 = 3.6f;

    // 이벤트 — UI가 구독해서 갱신
    public event Action<IReadOnlyList<QuestData>> OnQuestsGenerated;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<int,float> OnRewardGranted;
    public event Action OnPerfectDay;

    private readonly List<QuestData> todayQuests = new();
    private DailySave save = new DailySave();
    private string todayStr;
    private DateTime nextGenTimeLocal;

    private string BasePath =>
        Path.Combine(Application.persistentDataPath, "DailyQuests");

    // ===== Unity lifecycle =====
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Directory.CreateDirectory(BasePath);
        todayStr = DateTime.Now.ToString("yyyy-MM-dd");
        LoadOrInit();
        SetupNextGenTime();
        StartCoroutine(Scheduler());
    }

    // ===== Public API =====
    public IReadOnlyList<QuestData> GetQuests() 
    {
        Debug.Log($"[DailyQuestManager] GetQuests() 호출됨 - 현재 퀘스트 개수: {todayQuests.Count}");
        for (int i = 0; i < todayQuests.Count; i++)
        {
            Debug.Log($"[DailyQuestManager] Quest[{i}] - ID: {todayQuests[i].id}, Status: {todayQuests[i].status}");
        }
        return todayQuests;
    }

    /// <summary>완료(검증 없음, 버튼으로 호출)</summary>
    public bool CompleteQuest(string questId)
    {
        var q = todayQuests.Find(x => x.id == questId);
        if (q == null || q.status == QuestStatus.Completed) return false;

        q.status = QuestStatus.Completed;
        OnQuestCompleted?.Invoke(q);

        save.xpTotal += q.reward.xp;
        save.coinTotal += q.reward.coin;
        OnRewardGranted?.Invoke(q.reward.xp, q.reward.coin);

        // 모두 완료 시 Perfect Day
        bool allDone = true;
        foreach (var it in todayQuests)
            if (it.status != QuestStatus.Completed) { allDone = false; break; }
        if (allDone)
        {
            save.streak += 1;
            save.gachaTickets += 1;
            OnPerfectDay?.Invoke();
        }

        SaveToday();
        return true;
    }

    public DateTime GetNextGenerationTimeLocal() => nextGenTimeLocal;

    // ===== Internals =====
    private void LoadOrInit()
    {
        Debug.Log($"[DailyQuestManager] LoadOrInit() 시작 - todayStr: {todayStr}");
        
        var path = PathFor(todayStr);
        Debug.Log($"[DailyQuestManager] 저장 파일 경로 확인: {path}");
        
        if (File.Exists(path))
        {
            Debug.Log("[DailyQuestManager] 오늘 날짜 저장 파일이 존재합니다. 로드합니다.");
            save = JsonUtility.FromJson<DailySave>(File.ReadAllText(path)) ?? new DailySave();
            todayQuests.Clear();
            if (save.quests != null) 
            {
                todayQuests.AddRange(save.quests);
                Debug.Log($"[DailyQuestManager] 저장된 퀘스트 {save.quests.Length}개 로드 완료");
            }
            OnQuestsGenerated?.Invoke(todayQuests);
        }
        else
        {
            Debug.Log("[DailyQuestManager] 오늘 날짜 저장 파일이 없습니다.");
            
            // 현재 시간이 생성 시간 이후인지 확인
            var now = DateTime.Now;
            var todayGenTime = new DateTime(now.Year, now.Month, now.Day, generateHour, 0, 0);
            
            Debug.Log($"[DailyQuestManager] 현재 시간: {now:HH:mm:ss}");
            Debug.Log($"[DailyQuestManager] 오늘 생성 시간: {todayGenTime:HH:mm:ss}");
            
            if (now >= todayGenTime)
            {
                Debug.Log("[DailyQuestManager] 생성 시간이 지났으므로 퀘스트를 생성합니다.");
                GenerateForToday();
            }
            else
            {
                Debug.Log($"[DailyQuestManager] 아직 생성 시간이 아닙니다. {todayGenTime:HH:mm:ss}에 생성됩니다.");
                // 빈 상태로 유지
                todayQuests.Clear();
                OnQuestsGenerated?.Invoke(todayQuests);
            }
        }
    }

    private void GenerateForToday()
    {
        Debug.Log("[DailyQuestManager] GenerateForToday() 시작");
        
        // 이미 오늘 퀘스트가 생성되었는지 확인 (중복 생성 방지)
        var path = PathFor(todayStr);
        if (File.Exists(path))
        {
            Debug.LogWarning($"[DailyQuestManager] 오늘({todayStr}) 퀘스트가 이미 생성되어 있습니다. 중복 생성을 방지합니다.");
            // 기존 파일을 로드
            save = JsonUtility.FromJson<DailySave>(File.ReadAllText(path)) ?? new DailySave();
            todayQuests.Clear();
            if (save.quests != null) 
            {
                todayQuests.AddRange(save.quests);
                Debug.Log($"[DailyQuestManager] 기존 퀘스트 {save.quests.Length}개를 로드했습니다.");
            }
            OnQuestsGenerated?.Invoke(todayQuests);
            return;
        }
        
        int n = activeTracks.Count; // 제한 제거 - 인스펙터에서 설정한 만큼 생성
        Debug.Log($"[DailyQuestManager] 생성할 퀘스트 개수 (n): {n}");
        
        if (n <= 0)
        {
            Debug.LogWarning("[DailyQuestManager] activeTracks가 비어있습니다. 퀘스트를 생성할 수 없습니다.");
            return;
        }
        
        // 동적 보상 계산 - 퀘스트 개수에 따라 적절히 분배
        (int txp, float tcoin) totals = n switch
        {
            1 => (totalXp1, totalCoin1),
            2 => (totalXp2, totalCoin2),
            3 => (totalXp3, totalCoin3),
            _ => (totalXp3 + (n - 3) * 120, totalCoin3 + (n - 3) * 1.2f) // 3개 이상일 때 확장
        };
        Debug.Log($"[DailyQuestManager] 총 보상 - XP: {totals.txp}, Coin: {totals.tcoin}");
        
        int perXp = n > 0 ? Mathf.RoundToInt(totals.txp / (float)n) : 0;
        float perCoin = n > 0 ? totals.tcoin / n : 0f;
        Debug.Log($"[DailyQuestManager] 퀘스트당 보상 - XP: {perXp}, Coin: {perCoin}");

        todayQuests.Clear();
        Debug.Log("[DailyQuestManager] todayQuests 리스트 초기화 완료");
        
        int idx = 1;
        foreach (var t in activeTracks)
        {
            var questData = new QuestData {
                id = $"{todayStr}-{t}-{idx++:000}",
                track = t,
                title = TitleOf(t),
                description = DescOf(t),
                status = QuestStatus.Pending,
                reward = new Reward { xp = perXp, coin = perCoin }
            };
            
            todayQuests.Add(questData);
            Debug.Log($"[DailyQuestManager] 퀘스트 생성됨 - ID: {questData.id}, Track: {questData.track}, Title: {questData.title}");
        }

        Debug.Log($"[DailyQuestManager] 총 {todayQuests.Count}개 퀘스트 생성 완료");

        save = new DailySave {
            date = todayStr,
            quests = todayQuests.ToArray(),
            xpTotal = 0,
            coinTotal = 0f,
            streak = save?.streak ?? 0,
            gachaTickets = save?.gachaTickets ?? 0
        };

        Debug.Log($"[DailyQuestManager] DailySave 객체 생성 완료 - date: {save.date}, quests 배열 길이: {save.quests.Length}");

        SaveToday();
        Debug.Log("[DailyQuestManager] SaveToday() 호출 완료");
        
        // 이벤트 구독자 정보 확인
        if (OnQuestsGenerated != null)
        {
            var delegates = OnQuestsGenerated.GetInvocationList();
            Debug.Log($"[DailyQuestManager] OnQuestsGenerated 이벤트 구독자 수: {delegates.Length}");
            for (int i = 0; i < delegates.Length; i++)
            {
                Debug.Log($"[DailyQuestManager] 구독자[{i}]: {delegates[i].Target?.GetType().Name}.{delegates[i].Method.Name}");
            }
            OnQuestsGenerated.Invoke(todayQuests);
            Debug.Log("[DailyQuestManager] OnQuestsGenerated 이벤트 발생 완료");
        }
        else
        {
            Debug.LogWarning("[DailyQuestManager] OnQuestsGenerated 이벤트에 구독자가 없습니다!");
        }
    }

    private void SaveToday()
    {
        Debug.Log("[DailyQuestManager] SaveToday() 시작");
        
        save.quests = todayQuests.ToArray();
        Debug.Log($"[DailyQuestManager] save.quests 배열 업데이트 - 길이: {save.quests.Length}");
        
        string filePath = PathFor(todayStr);
        Debug.Log($"[DailyQuestManager] 저장할 파일 경로: {filePath}");
        
        string jsonData = JsonUtility.ToJson(save, true);
        Debug.Log($"[DailyQuestManager] JSON 데이터 생성 완료 - 길이: {jsonData.Length} characters");
        Debug.Log($"[DailyQuestManager] JSON 내용: {jsonData}");
        
        try
        {
            File.WriteAllText(filePath, jsonData);
            Debug.Log("[DailyQuestManager] 파일 저장 성공");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DailyQuestManager] 파일 저장 실패: {ex.Message}");
        }
    }

    private string PathFor(string date) => Path.Combine(BasePath, $"{date}.json");

    private void SetupNextGenTime()
    {
        var now = DateTime.Now;
        var today8 = new DateTime(now.Year, now.Month, now.Day, generateHour, 0, 0);
        nextGenTimeLocal = (now >= today8) ? today8.AddDays(1) : today8;
    }

    private IEnumerator Scheduler()
    {
        while (true)
        {
            if (DateTime.Now >= nextGenTimeLocal)
            {
                Debug.Log("[DailyQuestManager] Scheduler - 퀘스트 생성 시간이 되었습니다.");
                
                var newTodayStr = DateTime.Now.ToString("yyyy-MM-dd");
                Debug.Log($"[DailyQuestManager] Scheduler - 새로운 날짜: {newTodayStr}, 기존: {todayStr}");
                
                // 날짜가 바뀐 경우에만 새로운 퀘스트 생성
                if (newTodayStr != todayStr)
                {
                    Debug.Log("[DailyQuestManager] Scheduler - 날짜가 바뀌었으므로 새로운 퀘스트를 생성합니다.");
                    todayStr = newTodayStr;
                    
                    // 새 날짜의 저장 파일이 있는지 확인
                    var newPath = PathFor(todayStr);
                    if (!File.Exists(newPath))
                    {
                        Debug.Log("[DailyQuestManager] Scheduler - 새 날짜의 저장 파일이 없으므로 퀘스트를 생성합니다.");
                        GenerateForToday();
                    }
                    else
                    {
                        Debug.Log("[DailyQuestManager] Scheduler - 새 날짜의 저장 파일이 이미 존재합니다. 로드합니다.");
                        LoadOrInit();
                    }
                }
                else
                {
                    Debug.Log("[DailyQuestManager] Scheduler - 같은 날짜이므로 퀘스트 생성을 건너뜁니다.");
                }
                
                SetupNextGenTime();
            }
            yield return new WaitForSeconds(5f);
        }
    }

    private string TitleOf(TrackType t) => t switch
    {
        TrackType.Portfolio => "오늘의 커밋/PR",
        TrackType.Knowledge => "지식 카드 1개",
        TrackType.JobHunt   => "구직 진행 1회",
        TrackType.Free      => "자유 퀘스트",
        _ => "퀘스트"
    };

    private string DescOf(TrackType t) => t switch
    {
        TrackType.Portfolio => "깃 커밋 또는 PR 1건",
        TrackType.Knowledge => "요약 카드 적기",
        TrackType.JobHunt   => "이력서 수정/공고 기록 등",
        TrackType.Free      => "자유롭게 목표 설정",
        _ => ""
    };

    // --- 에디터 편의(선택). 요구 명세에 영향 없음 ---
    [ContextMenu("Quests/Force Generate For Today")]
    private void CM_ForceGenerate()
    {
        Debug.Log("[DailyQuestManager] Force Generate For Today 시작");
        Debug.Log($"[DailyQuestManager] 현재 activeTracks 개수: {activeTracks.Count}");
        
        for (int i = 0; i < activeTracks.Count; i++)
        {
            Debug.Log($"[DailyQuestManager] activeTracks[{i}]: {activeTracks[i]}");
        }
        
        todayStr = DateTime.Now.ToString("yyyy-MM-dd");
        Debug.Log($"[DailyQuestManager] todayStr: {todayStr}");
        
        GenerateForToday();
        
        // Force Generate 후 UI가 반영되지 않는 경우를 위한 추가 조치
        Debug.Log("[DailyQuestManager] Force Generate 완료 후 UI 강제 갱신 시도");
        
        // 혹시 UI가 아직 이벤트를 구독하지 않았을 경우를 대비해 잠시 후 다시 시도
        StartCoroutine(DelayedUIRefresh());
    }
    
    private System.Collections.IEnumerator DelayedUIRefresh()
    {
        yield return new WaitForSeconds(0.1f); // 짧은 지연
        
        Debug.Log("[DailyQuestManager] 지연된 UI 갱신 시도");
        Debug.Log($"[DailyQuestManager] 현재 todayQuests 개수: {todayQuests.Count}");
        
        if (OnQuestsGenerated != null)
        {
            var delegates = OnQuestsGenerated.GetInvocationList();
            Debug.Log($"[DailyQuestManager] 지연된 UI 갱신 - 구독자 수: {delegates.Length}");
            OnQuestsGenerated.Invoke(todayQuests);
            Debug.Log("[DailyQuestManager] 지연된 UI 갱신 이벤트 발생 완료");
        }
        else
        {
            Debug.LogWarning("[DailyQuestManager] 지연된 UI 갱신 시도 실패 - 구독자 없음");
            
            // 더 긴 지연 후 다시 한 번 시도
            yield return new WaitForSeconds(0.5f);
            Debug.Log("[DailyQuestManager] 두 번째 지연된 UI 갱신 시도");
            
            if (OnQuestsGenerated != null)
            {
                var delegates = OnQuestsGenerated.GetInvocationList();
                Debug.Log($"[DailyQuestManager] 두 번째 시도 - 구독자 수: {delegates.Length}");
                OnQuestsGenerated.Invoke(todayQuests);
                Debug.Log("[DailyQuestManager] 두 번째 시도 이벤트 발생 완료");
            }
            else
            {
                Debug.LogError("[DailyQuestManager] 두 번째 시도도 실패 - UI가 이벤트를 구독하지 않았거나 Script Execution Order 문제입니다!");
            }
        }
    }

    [ContextMenu("Quests/Clear Today Save")]
    private void CM_ClearToday()
    {
        var p = PathFor(DateTime.Now.ToString("yyyy-MM-dd"));
        if (File.Exists(p)) File.Delete(p);
        todayQuests.Clear();
        OnQuestsGenerated?.Invoke(todayQuests);
    }
    
    // === 테스트용 메서드 ===
    [ContextMenu("Test/Generate 10 Test Quests")]
    private void CM_GenerateTestQuests()
    {
        GenerateTestQuests(10);
    }
    
    private void GenerateTestQuests(int count)
    {
        Debug.Log($"[DailyQuestManager] 테스트용 {count}개 퀘스트 생성 시작");
        
        todayStr = DateTime.Now.ToString("yyyy-MM-dd");
        todayQuests.Clear();
        
        // 테스트용 고정 보상 (개수에 상관없이)
        int perXp = 100;
        float perCoin = 1f;
        
        var allTracks = System.Enum.GetValues(typeof(TrackType)) as TrackType[];
        
        for (int i = 0; i < count; i++)
        {
            // 트랙 타입을 순환하면서 할당
            var trackType = allTracks[i % allTracks.Length];
            
            var questData = new QuestData {
                id = $"{todayStr}-{trackType}-{(i + 1):000}",
                track = trackType,
                title = TitleOf(trackType) + $" #{i + 1}",
                description = DescOf(trackType) + $" (테스트 #{i + 1})",
                status = QuestStatus.Pending,
                reward = new Reward { xp = perXp, coin = perCoin }
            };
            
            todayQuests.Add(questData);
            Debug.Log($"[DailyQuestManager] 테스트 퀘스트 생성됨 - ID: {questData.id}");
        }
        
        Debug.Log($"[DailyQuestManager] 테스트용 {count}개 퀘스트 생성 완료");
        
        // 저장
        save = new DailySave {
            date = todayStr,
            quests = todayQuests.ToArray(),
            xpTotal = 0,
            coinTotal = 0f,
            streak = save?.streak ?? 0,
            gachaTickets = save?.gachaTickets ?? 0
        };
        
        SaveToday();
        
        // UI 갱신
        if (OnQuestsGenerated != null)
        {
            OnQuestsGenerated.Invoke(todayQuests);
            Debug.Log($"[DailyQuestManager] 테스트 퀘스트 UI 갱신 완료 - {count}개");
        }
        else
        {
            Debug.LogWarning("[DailyQuestManager] 테스트 퀘스트 생성 완료, 하지만 UI 구독자 없음");
            // 지연된 갱신 시도
            StartCoroutine(DelayedUIRefresh());
        }
    }
}
