using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button questListButton;
    [SerializeField] private QuestPopupUI questPopupUI;
    [SerializeField] private QuestBar questBar;
    
    private void Start()
    {
        // 버튼 이벤트 연결
        questListButton.onClick.AddListener(OnQuestListButtonClicked);
    }

    private void OnQuestListButtonClicked()
    {
        if (questPopupUI != null)
        {
            questPopupUI.ShowQuestPopup();
        }

        questBar.UpdateUI();

    }
}
