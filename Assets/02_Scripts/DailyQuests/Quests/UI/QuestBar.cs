using UnityEngine;
using TMPro;
using _02_Scripts.Reward;

// 사용 예:
// var g = GetComponent<QuestBar>();
// g.Initialize(20, 5);     // [5/20]
// g.Add(3);                // [8/20]
//
// // 나중에 외부 소스 연결:
// g.Bind(mySource);        // mySource.Changed에서 UpdateUI 호출

public interface IProgressSource
{
    int Current { get; }
    int Max { get; }
    event System.Action Changed;
}

public class QuestBar : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image background;
    [SerializeField] UnityEngine.UI.Image foreground;   // fillAmount만 설정
    [SerializeField] TMP_Text valueText;
    [SerializeField] RewardType rewardType;
    [SerializeField] UnityEngine.UI.Button rewardButton;

    private int maxValue => RewardManager.Instance.GetProgress(rewardType).Goal;
    private int currentValue => RewardManager.Instance.GetProgress(rewardType).Count;

    void Start()
    {
        UpdateUI();
        
        // 보상 버튼 클릭 이벤트 연결
        if (rewardButton != null)
        {
            rewardButton.onClick.AddListener(OnRewardButtonClicked);
        }
    }

    // UI 갱신
    public void UpdateUI()
    {
        float fillValue = maxValue > 0 ? (float)currentValue / maxValue : 0f;
        foreground.fillAmount = fillValue;
        valueText.text = $"{currentValue}/{maxValue}";
        Debug.Log($"QuestBar UpdateUI: current={currentValue}, max={maxValue}, fillAmount={fillValue}");
        
        // 보상 버튼 활성화 조건: 완료됨 && 아직 수령하지 않음
        UpdateRewardButton();
    }
    
    private void UpdateRewardButton()
    {
        bool isReceived = RewardManager.Instance.IsReceived(rewardType);
        
        // 이미 수령했다면 모든 UI 요소 비활성화
        if (isReceived)
        {
            background.enabled = false;
            foreground.enabled = false;
            valueText.enabled = false;
            if (rewardButton != null)
                rewardButton.gameObject.SetActive(false);
            return;
        }
        
        // 수령하지 않았다면 정상 로직 실행
        background.enabled = true;
        foreground.enabled = true;
        valueText.enabled = true;
        if (rewardButton != null)
        {
            rewardButton.gameObject.SetActive(true);
            bool isCompleted = currentValue >= maxValue && maxValue > 0;
            rewardButton.interactable = isCompleted;
        }
    }
    
    private void OnRewardButtonClicked()
    {
        if (RewardManager.Instance.TryReceive(rewardType, out var progress))
        {
            Debug.Log($"보상 수령 성공: {rewardType}");
            UpdateUI(); // UI 갱신하여 버튼 비활성화
        }
        else
        {
            Debug.Log($"보상 수령 실패: {rewardType}");
        }
    }
    
}
