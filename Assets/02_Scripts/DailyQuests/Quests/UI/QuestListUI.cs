using System.Collections.Generic;
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
            ui.Bind(q);
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
