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
    
    private int maxValue => RewardManager.Instance.GetProgress(rewardType).Goal;
    private int currentValue => RewardManager.Instance.GetProgress(rewardType).Count;

    void Start()
    {
        UpdateUI();
    }

    // UI 갱신
    private void UpdateUI()
    {
        foreground.fillAmount = (float)currentValue / maxValue;
        valueText.text = $"{currentValue}/{maxValue}";
    }
}
