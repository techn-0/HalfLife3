using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ToggleTabController : MonoBehaviour
{
    [System.Serializable]
    public class TabData
    {
        public Toggle toggle;             // 연결된 토글
        public GameObject linkedObject;   // 켜졌을 때 활성화할 오브젝트 (없으면 무시)
        public string categoryId;         // 상점 필터용 ID (없으면 무시)
    }

    public TabData[] tabs;
    public UnityEvent<string> onCategorySelected; // 카테고리 선택 이벤트 (상점 연결)

    void Start()
    {
        foreach (var tab in tabs)
        {
            tab.toggle.onValueChanged.AddListener(isOn =>
            {
                if (tab.linkedObject != null)
                    tab.linkedObject.SetActive(isOn);

                if (isOn && !string.IsNullOrEmpty(tab.categoryId))
                    onCategorySelected?.Invoke(tab.categoryId);
            });

            // 초기 상태 반영
            if (tab.linkedObject != null)
                tab.linkedObject.SetActive(tab.toggle.isOn);
        }
    }
}