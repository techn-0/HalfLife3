using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject trackSelectionPanel;
    [SerializeField] private Toggle knowledgeToggle;
    [SerializeField] private Toggle portfolioToggle;
    [SerializeField] private Toggle jobHuntToggle;
    [SerializeField] private Button confirmButton;
    
    [Header("Connected UI")]
    [SerializeField] private GameObject mainPanel;
    
    private void Start()
    {
        // 버튼 이벤트 연결
        confirmButton.onClick.AddListener(OnConfirmSelection);
        
        // 트랙 선택이 필요한지 확인
        CheckIfTrackSelectionNeeded();
    }
    
    private void CheckIfTrackSelectionNeeded()
    {
        if (DailyQuestManager.Instance == null)
        {
            // Manager가 준비될 때까지 대기
            StartCoroutine(WaitForManagerAndCheck());
            return;
        }
        
        // 이미 오늘 퀘스트가 있다면 트랙 선택 화면 숨기기
        var existingQuests = DailyQuestManager.Instance.GetQuests();
        if (existingQuests.Count > 0)
        {
            ShowMainPanel();
        }
        else
        {
            ShowTrackSelection();
        }
    }
    
    private System.Collections.IEnumerator WaitForManagerAndCheck()
    {
        while (DailyQuestManager.Instance == null)
        {
            yield return null;
        }
        
        CheckIfTrackSelectionNeeded();
    }
    
    private void ShowTrackSelection()
    {
        trackSelectionPanel.SetActive(true);
        mainPanel.SetActive(false);
        
        // 토글은 인스펙터 설정을 따라감 (강제 변경하지 않음)
    }
    
    private void ShowMainPanel()
    {
        trackSelectionPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    
    private void OnConfirmSelection()
    {
        Debug.Log("[TrackSelectionUI] OnConfirmSelection 시작");
        
        // 토글 상태 확인
        Debug.Log($"[TrackSelectionUI] 토글 상태 - Knowledge: {knowledgeToggle.isOn}, Portfolio: {portfolioToggle.isOn}, JobHunt: {jobHuntToggle.isOn}");
        
        // 선택된 트랙들 수집
        List<TrackType> selectedTracks = new List<TrackType>();
        
        if (knowledgeToggle.isOn)
            selectedTracks.Add(TrackType.Knowledge);
        if (portfolioToggle.isOn)
            selectedTracks.Add(TrackType.Portfolio);
        if (jobHuntToggle.isOn)
            selectedTracks.Add(TrackType.JobHunt);
        
        Debug.Log($"[TrackSelectionUI] 선택된 트랙: [{string.Join(", ", selectedTracks)}] (총 {selectedTracks.Count}개)");
        
        // 최소 1개는 선택해야 함
        if (selectedTracks.Count == 0)
        {
            Debug.LogWarning("[TrackSelectionUI] 최소 1개의 트랙을 선택해주세요!");
            return;
        }
        
        // DailyQuestManager에 선택된 트랙 설정
        if (DailyQuestManager.Instance != null)
        {
            Debug.Log("[TrackSelectionUI] DailyQuestManager.SetActiveTracks 호출");
            DailyQuestManager.Instance.SetActiveTracks(selectedTracks);
            Debug.Log("[TrackSelectionUI] DailyQuestManager.GenerateQuestsForSelectedTracks 호출");
            DailyQuestManager.Instance.GenerateQuestsForSelectedTracks();
        }
        else
        {
            Debug.LogError("[TrackSelectionUI] DailyQuestManager.Instance가 null입니다!");
        }
        
        // 메인 화면으로 전환
        ShowMainPanel();
    }
}
