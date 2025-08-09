using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private Button completeButton;
    [SerializeField] private TextMeshProUGUI buttonText; // 버튼 텍스트 컴포넌트

    private string questId;
    private QuestData currentQuestData;
    private QuestStatus lastKnownStatus = QuestStatus.Pending; // 마지막으로 알려진 상태
    private QuestListUI parentQuestListUI; // 부모 QuestListUI 참조
    // Update 루프 제거 - 이벤트 기반으로 변경

    public void Bind(QuestData data, QuestListUI questListUI = null)
    {
        if (data == null)
        {
            Debug.LogError("[QuestItemUI] Bind: data가 null입니다!");
            return;
        }
        
        questId = data.id;
        currentQuestData = data;
        lastKnownStatus = data.status; // 초기 상태 저장
        parentQuestListUI = questListUI; // 부모 참조 저장
        title.text = data.title;
        desc.text = data.description;
        
        Debug.Log($"[QuestItemUI] Bind 호출됨 - ID: {questId}, 제목: {data.title}, 상태: {data.status}");
        
        // buttonText가 할당되지 않은 경우 자동으로 찾아보기
        if (buttonText == null && completeButton != null)
        {
            buttonText = completeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                Debug.LogError($"[QuestItemUI] Button에서 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다! Inspector에서 Button Text를 수동으로 할당해주세요. (퀘스트: {questId})");
            }
            else
            {
                Debug.Log($"[QuestItemUI] buttonText 자동 할당 성공: {questId}");
            }
        }
        
        // 컴포넌트 유효성 검사
        if (completeButton == null)
        {
            Debug.LogError($"[QuestItemUI] completeButton이 할당되지 않았습니다! (퀘스트: {questId})");
        }
        
        UpdateButtonState(data.status);
        
        Debug.Log($"[QuestItemUI] Bind 완료 - ID: {questId}, 버튼 활성화: {completeButton?.interactable}, 텍스트: '{buttonText?.text}'");
    }
    
    // Update 루프 및 CheckQuestStatusChange 제거 - 이벤트 기반으로 변경하여 성능 향상 및 로그 스팸 방지
    
    // 디버깅용 메서드 - 현재 퀘스트 상태 출력 (간소화됨)
    [ContextMenu("Debug/Print Quest Status")]
    private void DebugPrintQuestStatus()
    {
        Debug.Log("=== 개별 퀘스트 상태 디버깅 ===");
        Debug.Log($"Quest ID: {questId ?? "null"}");
        Debug.Log($"Last Known Status: {lastKnownStatus}");
        Debug.Log($"Current Quest Data Status: {currentQuestData?.status ?? QuestStatus.Pending}");
        Debug.Log($"Button Interactable: {completeButton?.interactable ?? false}");
        Debug.Log($"Button Text: '{buttonText?.text ?? "null"}'");
        Debug.Log("=== 디버깅 끝 ===");
        Debug.Log("💡 전체 퀘스트 디버깅은 QuestListUI의 컨텍스트 메뉴를 사용하세요");
    }
    
    private void UpdateButtonState(QuestStatus status)
    {
        Debug.Log($"🔧 [QuestItemUI] UpdateButtonState 시작 - Quest: {questId}, Status: {status}");
        
        if (completeButton == null)
        {
            Debug.LogError($"❌ [QuestItemUI] completeButton이 null입니다! ({questId})");
            return;
        }
        
        if (buttonText == null)
        {
            Debug.LogError($"❌ [QuestItemUI] buttonText가 null입니다! ({questId})");
            return;
        }
        
        // 이전 상태 로그
        Debug.Log($"📊 [QuestItemUI] 변경 전 - Interactable: {completeButton.interactable}, Text: '{buttonText.text}'");
        
        switch (status)
        {
            case QuestStatus.Pending:
                completeButton.interactable = true;
                buttonText.text = "완료하기";
                Debug.Log($"🔧 [QuestItemUI] Pending 상태로 설정");
                break;
                
            case QuestStatus.Completed:
                completeButton.interactable = false;
                buttonText.text = "완료됨";
                Debug.Log($"🔧 [QuestItemUI] Completed 상태로 설정");
                break;
        }
        
        // UI 강제 리프레시 (더 강력한 업데이트)
        try
        {
            buttonText.SetLayoutDirty();
            buttonText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            
            // 버튼 컴포넌트 강제 활성화/비활성화로 리프레시
            completeButton.enabled = false;
            completeButton.enabled = true;
            
            Debug.Log($"🔧 [QuestItemUI] UI 강제 리프레시 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ [QuestItemUI] UI 리프레시 중 오류: {ex.Message}");
        }
        
        // 변경 후 상태 확인
        Debug.Log($"📊 [QuestItemUI] 변경 후 - Interactable: {completeButton.interactable}, Text: '{buttonText.text}'");
        
        // 완료 상태로 변경될 때 특별 로그
        if (status == QuestStatus.Completed)
        {
            Debug.Log($"✅ [QuestItemUI] 버튼 완료 상태 적용 완료: {questId}");
        }
    }
    
    // 외부에서 퀘스트 상태가 변경되었을 때 UI 업데이트 (이벤트 기반)
    public void UpdateQuestStatus(QuestStatus newStatus)
    {
        Debug.Log($"🎉 [QuestItemUI] UpdateQuestStatus 호출 - Quest: {questId}, New Status: {newStatus}");
        Debug.Log($"📊 [QuestItemUI] 이전 상태 - LastKnown: {lastKnownStatus}, Data: {currentQuestData?.status}");
        
        if (currentQuestData != null)
        {
            currentQuestData.status = newStatus;
            Debug.Log($"🔄 [QuestItemUI] currentQuestData 상태 업데이트 완료");
        }
        
        // 상태 추적 업데이트
        lastKnownStatus = newStatus;
        Debug.Log($"🔄 [QuestItemUI] lastKnownStatus 업데이트 완료: {lastKnownStatus}");
        
        // UI 강제 업데이트
        UpdateButtonState(newStatus);
        
        // Canvas 강제 리프레시 (즉시 반영을 위해)
        if (completeButton != null)
        {
            completeButton.gameObject.SetActive(false);
            completeButton.gameObject.SetActive(true);
            Debug.Log($"🔄 [QuestItemUI] GameObject 강제 리프레시 완료");
        }
        
        // 중요한 상태 변경만 로그로 기록
        if (newStatus == QuestStatus.Completed)
        {
            Debug.Log($"🎉 [QuestItemUI] 퀘스트 상태 업데이트 최종 완료: {questId} → {newStatus}");
            
            // 최종 상태 확인
            Debug.Log($"📊 [QuestItemUI] 최종 확인 - Button: {completeButton?.interactable}, Text: '{buttonText?.text}'");
        }
    }

    public void OnClickComplete()
    {
        Debug.Log($"🎯 [QuestItemUI] 완료 버튼 클릭: {questId}");
        
        // QuestListUI로 처리 위임 (참조 우선, fallback으로 Find 사용)
        var questListUI = parentQuestListUI ?? FindFirstObjectByType<QuestListUI>();
        if (questListUI != null)
        {
            questListUI.CompleteQuest(questId);
        }
        else
        {
            Debug.LogError($"❌ [QuestItemUI] QuestListUI를 찾을 수 없습니다!");
        }
    }
}
