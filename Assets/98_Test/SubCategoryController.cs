using UnityEngine;
using UnityEngine.UI;

public class SubCategoryController : MonoBehaviour
{
    [System.Serializable]
    public class SubCategoryTab
    {
        public Toggle subToggle;  // 작은 카테고리 토글
        public string categoryId; // 상점 로직에 넘길 ID
    }

    public SubCategoryTab[] subCategories;
    //public ShopUI shopUI; // 상점 아이템 필터 담당

    void Start()
    {
        foreach (var sub in subCategories)
        {
            sub.subToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    // 상점 아이템 필터 변경
                    //shopUI.SetSubCategory(sub.categoryId);
                }
            });
        }
    }
}