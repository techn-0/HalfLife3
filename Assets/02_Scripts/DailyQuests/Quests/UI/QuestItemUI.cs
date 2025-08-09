using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private Button completeButton;

    private string questId;

    public void Bind(QuestData data)
    {
        questId = data.id;
        title.text = data.title;
        desc.text = data.description;
        completeButton.interactable = (data.status == QuestStatus.Pending);
    }

    public void OnClickComplete()
    {
        if (DailyQuestManager.Instance.CompleteQuest(questId))
            completeButton.interactable = false;
    }
}
