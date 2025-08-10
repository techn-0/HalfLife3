using UnityEngine;

public class ShopTab : MonoBehaviour
{
    [SerializeField] private ShopElement[] shopElements;

    [SerializeField]
    private Transform shopElementParent;

    private bool isInit = false;

    public void Init()
    {
        isInit = true;
        shopElements = shopElementParent.GetComponentsInChildren<ShopElement>();
        foreach (var shopElement in shopElements)
        {
            shopElement.Init();
        }
    }
    public void UpdateUI()
    {
        foreach (ShopElement shopElement in shopElements)
        {
            shopElement.UpdateUI();
        }
    }
}
