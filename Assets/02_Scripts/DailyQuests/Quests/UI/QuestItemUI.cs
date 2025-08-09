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
    // Update 루프 제거 - 이벤트 기반으로 변경

    public void Bind(QuestData data)
    {
        if (data == null)
        {
            Debug.LogError("[QuestItemUI] Bind: data가 null입니다!");
            return;
        }
        
        questId = data.id;
        currentQuestData = data;
        lastKnownStatus = data.status; // 초기 상태 저장
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
    
    // 디버깅용 메서드 - 현재 퀘스트 상태 출력
    [ContextMenu("Debug/Print Quest Status")]
    private void DebugPrintQuestStatus()
    {
        Debug.Log("=== 퀘스트 상태 디버깅 ===");
        Debug.Log($"Quest ID: {questId ?? "null"}");
        Debug.Log($"Last Known Status: {lastKnownStatus}");
        Debug.Log($"Current Quest Data Status: {currentQuestData?.status ?? QuestStatus.Pending}");
        Debug.Log($"Button Interactable: {completeButton?.interactable ?? false}");
        Debug.Log($"Button Text: '{buttonText?.text ?? "null"}'");
        
        // DailyQuestManager에서의 실제 상태도 확인
        if (!string.IsNullOrEmpty(questId) && DailyQuestManager.Instance != null)
        {
            var allQuests = DailyQuestManager.Instance.GetQuests();
            var actualQuest = allQuests?.FirstOrDefault(q => q.id == questId);
            Debug.Log($"Actual Status in Manager: {actualQuest?.status ?? QuestStatus.Pending}");
            Debug.Log($"Manager Quest Title: {actualQuest?.title ?? "Not Found"}");
        }
        else
        {
            Debug.LogWarning("Cannot check manager status - questId or manager is null");
        }
        Debug.Log("=== 디버깅 끝 ===");
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
        
        // 클릭 전 상태 로그
        Debug.Log($"📊 [QuestItemUI] 클릭 전 상태 - LastKnown: {lastKnownStatus}, Button: {completeButton?.interactable}, Text: '{buttonText?.text}'");
        
        // 안전성 체크
        if (string.IsNullOrEmpty(questId) || DailyQuestManager.Instance == null)
        {
            Debug.LogError($"❌ [QuestItemUI] 초기화 오류: questId 또는 DailyQuestManager가 없습니다!");
            return;
        }
        
        if (lastKnownStatus == QuestStatus.Completed)
        {
            Debug.LogWarning($"⚠️ [QuestItemUI] 이미 완료된 퀘스트입니다: {questId}");
            return;
        }
        
        // 1단계: 퀘스트 완료 처리 (DailyQuestManager에서 상태 변경)
        Debug.Log($"🔄 [QuestItemUI] DailyQuestManager.CompleteQuest 호출 중...");
        bool success = DailyQuestManager.Instance.CompleteQuest(questId);
        Debug.Log($"🔄 [QuestItemUI] CompleteQuest 결과: {success}");
        
        if (success)
        {
            // 2단계: 로컬 상태 업데이트 (즉시 반영)
            Debug.Log($"🔄 [QuestItemUI] 로컬 상태 업데이트 중...");
            lastKnownStatus = QuestStatus.Completed;
            if (currentQuestData != null)
            {
                currentQuestData.status = QuestStatus.Completed;
                Debug.Log($"🔄 [QuestItemUI] currentQuestData 상태 업데이트 완료");
            }
            
            // 3단계: UI 업데이트 (버튼 텍스트 변경)
            Debug.Log($"🔄 [QuestItemUI] UI 업데이트 호출 중...");
            UpdateButtonState(QuestStatus.Completed);
            
            // 업데이트 후 상태 확인
            Debug.Log($"📊 [QuestItemUI] 업데이트 후 상태 - Button: {completeButton?.interactable}, Text: '{buttonText?.text}'");
            Debug.Log($"✅ [QuestItemUI] 퀘스트 완료 처리 완료: {questId}");
            
            // 강제로 다시 한 번 UI 업데이트 시도
            StartCoroutine(ForceUIUpdateNextFrame());
        }
        else
        {
            Debug.LogError($"❌ [QuestItemUI] 퀘스트 완료 실패: {questId}");
        }
    }
    
    // 강제 UI 업데이트 코루틴 (문제 해결용)
    private System.Collections.IEnumerator ForceUIUpdateNextFrame()
    {
        yield return null; // 다음 프레임 대기
        
        Debug.Log($"🔄 [QuestItemUI] 다음 프레임 강제 UI 업데이트: {questId}");
        
        // 상태 재확인
        if (lastKnownStatus == QuestStatus.Completed)
        {
            // 강제로 버튼 상태 다시 설정
            if (completeButton != null)
            {
                completeButton.interactable = false;
                Debug.Log($"🔄 [QuestItemUI] 버튼 비활성화 강제 적용");
            }
            
            if (buttonText != null)
            {
                buttonText.text = "완료됨";
                buttonText.SetLayoutDirty();
                buttonText.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
                Debug.Log($"🔄 [QuestItemUI] 버튼 텍스트 강제 업데이트: '{buttonText.text}'");
            }
            
            Debug.Log($"✅ [QuestItemUI] 다음 프레임 강제 업데이트 완료: {questId}");
        }
    }
}
