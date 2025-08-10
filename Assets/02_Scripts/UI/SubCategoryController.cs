using UnityEngine;
using UnityEngine.UI;

public class SubCategoryController : MonoBehaviour
{
    [System.Serializable]
    public class SubCategoryTab
    {
        public Toggle subToggle;  // 작은 카테고리 토글
        public ShopTab Tab; // 탭
    }

    public SubCategoryTab[] subCategories;
    //public ShopUI shopUI; // 상점 아이템 필터 담당

    public void Init()
    {
        EventManager.Instance.Subscribe("UpdateUI", UpdateUI);
        foreach (var sub in subCategories)
        {
            sub.Tab.Init();
            sub.subToggle.onValueChanged.AddListener(isOn =>
            {
                sub.Tab.gameObject.SetActive(isOn);
                if (isOn)
                {
                    sub.Tab.UpdateUI();
                }
            });
        }
    }

    public void UpdateUI()
    {
        Debug.Log("Event");
        foreach (var sub in subCategories)
        {
            sub.Tab.UpdateUI();
        }
    }
}