using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestListUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;   // ScrollView의 Content
    [SerializeField] private QuestItemUI itemPrefab;  // 퀘스트 카드 프리팹
    
    // 생성된 UI 아이템들을 관리하기 위한 딕셔너리
    private Dictionary<string, QuestItemUI> questUIItems = new Dictionary<string, QuestItemUI>();

    private void OnEnable()
    {
        // DailyQuestManager가 준비될 때까지 기다리는 코루틴 시작
        StartCoroutine(WaitForManagerAndSubscribe());
    }
    
    private System.Collections.IEnumerator WaitForManagerAndSubscribe()
    {
        // DailyQuestManager.Instance가 준비될 때까지 대기
        while (DailyQuestManager.Instance == null)
        {
            yield return null;
        }
        
        // 추가로 한 프레임 더 대기 (Manager가 완전히 초기화되도록)
        yield return null;
        
        Debug.Log($"[QuestListUI] Manager 준비 완료, 이벤트 구독 시작");
        
        // 이벤트 구독
        DailyQuestManager.Instance.OnQuestsGenerated += Refresh;
        DailyQuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        
        // 이미 생성된 퀘스트가 있다면 로드
        var quests = DailyQuestManager.Instance.GetQuests();
        Debug.Log($"[QuestListUI] 기존 퀘스트 확인: {quests.Count}개");
        
        if (quests.Count > 0)
        {
            Debug.Log($"[QuestListUI] 기존 퀘스트 로드 시작");
            Refresh(quests);
        }
        else
        {
            Debug.Log($"[QuestListUI] 기존 퀘스트 없음");
        }
    }
    
    private void OnDisable()
    {
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestsGenerated -= Refresh;
            DailyQuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    private void Start()
    {
        // OnEnable에서 코루틴으로 이미 처리하므로 Start에서는 추가 작업 없음
        Debug.Log("📋 [QuestListUI] Start 호출됨 - OnEnable 코루틴이 이벤트 구독 처리 중");
    }

    public void Refresh(IReadOnlyList<QuestData> list)
    {
        // 기존 UI 아이템들과 딕셔너리 정리
        questUIItems.Clear();
        foreach (Transform child in content) 
        {
            Destroy(child.gameObject);
        }
        
        if (list == null) 
        {
            Debug.LogWarning("⚠️ [QuestListUI] 퀘스트 목록이 null입니다!");
            return;
        }

        // 새로운 UI 아이템들 생성 및 딕셔너리에 등록
        foreach (var q in list)
        {
            var ui = Instantiate(itemPrefab, content);
            ui.Bind(q, this); // this 참조 전달
            questUIItems[q.id] = ui; // 딕셔너리에 등록
        }
        
        Debug.Log($"📋 [QuestListUI] UI 새로고침 완료 - {list.Count}개 퀘스트");
    }
    
    // 퀘스트 완료 이벤트 처리 (이벤트 기반 업데이트)
    private void OnQuestCompleted(QuestData completedQuest)
    {
        // 해당 퀘스트의 UI 아이템 찾아서 상태 업데이트
        if (questUIItems.TryGetValue(completedQuest.id, out var questUI))
        {
            if (questUI != null)
            {
                // 이벤트 기반으로 즉시 UI 업데이트
                questUI.UpdateQuestStatus(QuestStatus.Completed);
                
                // 강제로 다음 프레임에서도 한 번 더 업데이트
                StartCoroutine(ForceUIUpdateNextFrame(questUI, completedQuest.id));
            }
            else
            {
                Debug.LogError($"[QuestListUI] UI 아이템이 null입니다: {completedQuest.id}");
            }
        }
        else
        {
            Debug.LogError($"[QuestListUI] UI를 찾을 수 없습니다: {completedQuest.id}");
        }
    }
    
    // 강제 UI 업데이트 (GitHub 검증 후 UI 반영을 위해)
    private System.Collections.IEnumerator ForceUIUpdateNextFrame(QuestItemUI questUI, string questId)
    {
        yield return null; // 다음 프레임 대기
        
        questUI.UpdateQuestStatus(QuestStatus.Completed);
    }
    
    // 퀘스트 완료 처리 (QuestItemUI에서 이동)
    public void CompleteQuest(string questId)
    {
        Debug.Log($"🎯 [QuestListUI] 퀘스트 완료 요청: {questId}");
        
        // 안전성 체크
        if (string.IsNullOrEmpty(questId) || DailyQuestManager.Instance == null)
        {
            Debug.LogError($"❌ [QuestListUI] 초기화 오류: questId 또는 DailyQuestManager가 없습니다!");
            return;
        }
        
        // 해당 UI 아이템 찾기
        if (!questUIItems.TryGetValue(questId, out var questUI) || questUI == null)
        {
            Debug.LogError($"❌ [QuestListUI] UI 아이템을 찾을 수 없습니다: {questId}");
            return;
        }
        
        // 이미 완료된 퀘스트 체크
        var questData = DailyQuestManager.Instance.GetQuests().FirstOrDefault(q => q.id == questId);
        if (questData?.status == QuestStatus.Completed)
        {
            Debug.LogWarning($"⚠️ [QuestListUI] 이미 완료된 퀘스트입니다: {questId}");
            return;
        }
        
        // 1단계: 퀘스트 완료 처리 (DailyQuestManager에서 상태 변경)
        Debug.Log($"🔄 [QuestListUI] DailyQuestManager.CompleteQuest 호출 중...");
        bool success = DailyQuestManager.Instance.CompleteQuest(questId);
        Debug.Log($"🔄 [QuestListUI] CompleteQuest 결과: {success}");
        
        if (success)
        {
            // 2단계: UI 업데이트는 OnQuestCompleted 이벤트에서 자동 처리됨
            Debug.Log($"✅ [QuestListUI] 퀘스트 완료 처리 완료: {questId}");
        }
        else
        {
            Debug.LogError($"❌ [QuestListUI] 퀘스트 완료 실패: {questId}");
        }
    }
    
    // 전체 퀘스트 상태 디버깅 (QuestItemUI에서 이동 및 확장)
    [ContextMenu("Debug/Print All Quest Status")]
    public void DebugPrintAllQuestStatus()
    {
        Debug.Log("=== 전체 퀘스트 상태 디버깅 ===");
        Debug.Log($"UI 아이템 개수: {questUIItems.Count}");
        
        if (DailyQuestManager.Instance == null)
        {
            Debug.LogError("DailyQuestManager.Instance가 null입니다!");
            return;
        }
        
        var allQuests = DailyQuestManager.Instance.GetQuests();
        Debug.Log($"Manager의 퀘스트 개수: {allQuests?.Count ?? 0}");
        
        foreach (var kvp in questUIItems)
        {
            string questId = kvp.Key;
            QuestItemUI questUI = kvp.Value;
            
            Debug.Log($"--- 퀘스트 ID: {questId} ---");
            
            if (questUI != null)
            {
                Debug.Log($"UI 존재: ✓");
                // UI의 상태는 QuestItemUI의 public 메서드로 접근하거나 Manager에서 확인
            }
            else
            {
                Debug.Log($"UI 존재: ✗ (null)");
            }
            
            // Manager에서의 실제 상태 확인
            var actualQuest = allQuests?.FirstOrDefault(q => q.id == questId);
            if (actualQuest != null)
            {
                Debug.Log($"Manager 상태: {actualQuest.status}");
                Debug.Log($"Manager 제목: {actualQuest.title}");
            }
            else
            {
                Debug.Log($"Manager에서 찾을 수 없음");
            }
        }
        Debug.Log("=== 디버깅 끝 ===");
    }
    
    // 퀘스트 초기화 (UI 클리어)
    public void ClearQuests()
    {
        questUIItems.Clear();
        foreach (Transform child in content) 
        {
            Destroy(child.gameObject);
        }
    }
    
    // 퀘스트 완전 초기화 (데이터 + UI)
    public void ClearAllQuests()
    {
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.ClearAllQuests();
        }
    }
}
