using _02_Scripts.Shop;
using TMPro;
using UnityEngine;

public class Overlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldTMP;

    public void Init()
    {
        UpdateGoldTMPValue(CoinManager.Balance);
        CoinManager.Instance.OnBalanceChanged += UpdateGoldTMPValue;
    }

    public void UpdateGoldTMPValue(long gold)
    {
        goldTMP.text = gold.ToString();
    }
}
