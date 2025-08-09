using UnityEngine;
using UnityEngine.UI;

public class MainCategoryController : MonoBehaviour
{
    [System.Serializable]
    public class MainCategoryTab
    {
        public Toggle mainToggle;       // 큰 카테고리 토글
        public GameObject subGroup;     // 연결된 작은 카테고리 그룹
    }

    public MainCategoryTab[] categories;

    void Start()
    {
        foreach (var cat in categories)
        {
            cat.mainToggle.onValueChanged.AddListener(isOn =>
            {
                cat.subGroup.SetActive(isOn);
            });

            // 시작 시 토글 상태에 맞춰 활성화
            cat.subGroup.SetActive(cat.mainToggle.isOn);
        }
    }
}