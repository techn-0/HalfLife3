using System.Collections.Generic;
using UnityEngine;

public class QuestListUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;   // ScrollView의 Content
    [SerializeField] private QuestItemUI itemPrefab;  // 퀘스트 카드 프리팹

    private void OnEnable()
    {
        Debug.Log("[QuestListUI] OnEnable 호출됨");
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestsGenerated += Refresh;
            Debug.Log("[QuestListUI] OnQuestsGenerated 이벤트 구독 완료");
        }
        else
        {
            Debug.LogWarning("[QuestListUI] DailyQuestManager.Instance가 null입니다!");
        }
    }
    private void OnDisable()
    {
        Debug.Log("[QuestListUI] OnDisable 호출됨");
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestsGenerated -= Refresh;
            Debug.Log("[QuestListUI] OnQuestsGenerated 이벤트 구독 해제 완료");
        }
    }

    private void Start()
    {
        Debug.Log("[QuestListUI] Start 호출됨");
        if (DailyQuestManager.Instance != null)
        {
            // OnEnable에서 구독에 실패했을 수 있으니 Start에서도 구독 시도
            DailyQuestManager.Instance.OnQuestsGenerated -= Refresh; // 중복 구독 방지
            DailyQuestManager.Instance.OnQuestsGenerated += Refresh;
            Debug.Log("[QuestListUI] Start에서 OnQuestsGenerated 이벤트 구독 완료");
            
            var quests = DailyQuestManager.Instance.GetQuests();
            Debug.Log($"[QuestListUI] Start에서 퀘스트 {quests.Count}개 가져와서 Refresh 호출");
            Refresh(quests);
        }
        else
        {
            Debug.LogWarning("[QuestListUI] Start에서 DailyQuestManager.Instance가 null입니다!");
        }
    }

    private void Refresh(IReadOnlyList<QuestData> list)
    {
        Debug.Log($"[QuestListUI] Refresh 호출됨 - 퀘스트 개수: {list?.Count ?? 0}");
        
        foreach (Transform child in content) Destroy(child.gameObject);
        if (list == null) 
        {
            Debug.LogWarning("[QuestListUI] Refresh - list가 null입니다!");
            return;
        }

        foreach (var q in list)
        {
            var ui = Instantiate(itemPrefab, content);
            ui.Bind(q);
            Debug.Log($"[QuestListUI] UI 아이템 생성됨 - Quest ID: {q.id}");
        }
        
        Debug.Log($"[QuestListUI] Refresh 완료 - 총 {list.Count}개 UI 아이템 생성");
    }
}
