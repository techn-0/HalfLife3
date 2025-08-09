using UnityEngine;
using UnityEngine.UI;

public class QuestPopupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPopup;
    [SerializeField] private Button closeButton;
    [SerializeField] private QuestListUI questListUI;
    
    private void Start()
    {
        Debug.Log("[QuestPopupUI] Start 호출됨");
        
        // Inspector 참조 검증
        if (questPopup == null)
        {
            Debug.LogError("[QuestPopupUI] questPopup이 null입니다! Inspector에서 할당해주세요.");
        }
        
        if (closeButton == null)
        {
            Debug.LogError("[QuestPopupUI] closeButton이 null입니다! Inspector에서 할당해주세요.");
        }
        else
        {
            // 버튼 이벤트 연결
            closeButton.onClick.AddListener(ClosePopup);
            Debug.Log("[QuestPopupUI] closeButton 이벤트 연결 완료");
        }
        
        if (questListUI == null)
        {
            Debug.LogError("[QuestPopupUI] questListUI가 null입니다! Inspector에서 할당해주세요.");
        }
        
        // 중요: 팝업이 이미 활성화되어 있다면 비활성화하지 않음
        if (questPopup != null)
        {
            bool isCurrentlyActive = questPopup.activeSelf;
            Debug.Log($"[QuestPopupUI] Start 시점의 questPopup 상태: {isCurrentlyActive}");
            
            if (!isCurrentlyActive)
            {
                // 팝업이 비활성화 상태일 때만 비활성화 상태 유지
                questPopup.SetActive(false);
                Debug.Log("[QuestPopupUI] 초기 팝업 비활성화 완료");
            }
            else
            {
                // 팝업이 이미 활성화되어 있으면 그대로 유지
                Debug.Log("[QuestPopupUI] 팝업이 이미 활성화되어 있어 상태 유지");
            }
        }
    }
    
    public void ShowQuestPopup()
    {
        try
        {
            if (questPopup == null)
            {
                Debug.LogError("[QuestPopupUI] questPopup이 null입니다! Inspector에서 할당해주세요.");
                return;
            }
            
            Debug.Log("[QuestPopupUI] ShowQuestPopup 시작");
            
            // 먼저 DailyQuestManager 상태 확인
            if (DailyQuestManager.Instance == null)
            {
                Debug.LogError("[QuestPopupUI] DailyQuestManager.Instance가 null입니다!");
                return;
            }
            
            var quests = DailyQuestManager.Instance.GetQuests();
            Debug.Log($"[QuestPopupUI] 현재 퀘스트 개수: {quests?.Count ?? 0}");
            
            if (quests != null && quests.Count > 0)
            {
                foreach (var quest in quests)
                {
                    Debug.Log($"[QuestPopupUI] 퀘스트: {quest.id} - {quest.title} ({quest.status})");
                }
            }
            else
            {
                Debug.LogWarning("[QuestPopupUI] 퀘스트가 없습니다! 트랙 선택을 완료했는지 확인하세요.");
            }
            
            questPopup.SetActive(true);
            Debug.Log("[QuestPopupUI] questPopup 활성화 완료");
            
            // 팝업을 UI 계층의 최상단으로 이동 (다른 UI보다 앞에 렌더링)
            questPopup.transform.SetAsLastSibling();
            Debug.Log("[QuestPopupUI] SetAsLastSibling 완료");
            
            // 즉시 한번 새로고침 시도
            RefreshQuestListSafely();
            
            // 한 프레임 지연 후 퀘스트 목록 새로고침 (UI가 완전히 활성화된 후)
            StartCoroutine(RefreshQuestListNextFrame());
            Debug.Log("[QuestPopupUI] RefreshQuestListNextFrame 코루틴 시작");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestPopupUI] ShowQuestPopup 오류: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    private System.Collections.IEnumerator RefreshQuestListNextFrame()
    {
        Debug.Log("[QuestPopupUI] RefreshQuestListNextFrame 시작");
        
        // 한 프레임 대기
        yield return null;
        
        Debug.Log("[QuestPopupUI] 한 프레임 대기 완료");
        
        // 안전한 참조 체크 및 새로고침 실행
        RefreshQuestListSafely();
    }
    
    private void RefreshQuestListSafely()
    {
        try
        {
            // 안전한 참조 체크
            if (questListUI == null)
            {
                Debug.LogError("[QuestPopupUI] questListUI가 null입니다! Inspector에서 할당해주세요.");
                return;
            }
            
            if (DailyQuestManager.Instance == null)
            {
                Debug.LogError("[QuestPopupUI] DailyQuestManager.Instance가 null입니다!");
                return;
            }
            
            // 퀘스트 목록 명시적 새로고침
            var quests = DailyQuestManager.Instance.GetQuests();
            Debug.Log($"[QuestPopupUI] 퀘스트 목록 새로고침 (지연 후) - {quests?.Count ?? 0}개 퀘스트");
            
            if (quests != null)
            {
                // QuestListUI의 Refresh 메서드를 직접 호출
                questListUI.Refresh(quests);
                Debug.Log("[QuestPopupUI] questListUI.Refresh 호출 완료");
            }
            else
            {
                Debug.LogWarning("[QuestPopupUI] 퀘스트 목록이 null입니다!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[QuestPopupUI] RefreshQuestListSafely 오류: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    public void ClosePopup()
    {
        questPopup.SetActive(false);
    }
}
