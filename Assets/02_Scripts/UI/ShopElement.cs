using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopElement : MonoBehaviour
{
    [Serializable]
    public struct ActiveObject
    {
        public GameObject[] Objects;
    }
    
    [SerializeField] private string id;
    [SerializeField] private int price;
    [SerializeField] private Sprite sprite;
    
    [SerializeField] private ActiveObject[] ActiveObj = new  ActiveObject[2];
    [SerializeField] private ActiveObject[] PriceObj = new  ActiveObject[2];
    
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI PriceTMP;

    private bool isActive = false;
    private bool isOpen = false;

    public void Init()
    {
        button?.onClick.AddListener(ClickBtn);
        PriceTMP.text = price.ToString();
        
        UpdateUI();
    }

    public void ClickBtn()
    {
        if (isOpen)
        {
                // 이미 구매된 경우: 켜기/끄기 토글
                isActive = !isActive;
                PlayerPrefs.SetInt($"{id}_isActive", isActive ? 1 : 0);
                PlayerPrefs.Save();
        }
        else
        {
                // 구매 로직
                if (GameManager.instance != null && GameManager.instance.Gold >= price)
                {
                    // 금액 차감
                    GameManager.instance.Gold -= price;

                    // 구매 상태 저장
                    isOpen = true;
                    PlayerPrefs.SetInt($"{id}_isOpen", 1);
                    PlayerPrefs.SetInt($"{id}_isActive", 1); // 구매하면 자동 활성화
                    PlayerPrefs.Save();

                    isActive = true;

                    EventManager.Instance.Publish("UpdateUI");
                }
                else
                {
                    // 돈 부족 → 무반응
                    Debug.Log("Gold가 부족합니다.");
                }
        }
    }
    public void UpdateUI()
    {
        isOpen = PlayerPrefs.HasKey($"{id}_isOpen");
        if (isOpen)
        {
            isActive = PlayerPrefs.GetInt($"{id}_isActive", 0) == 1;
            if (isActive)
            {
                foreach (GameObject obj in ActiveObj[0].Objects)
                {
                    obj.SetActive(false);
                }
                foreach (GameObject obj in ActiveObj[1].Objects)
                {
                    obj.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject obj in ActiveObj[0].Objects)
                {
                    obj.SetActive(true);
                }
                foreach (GameObject obj in ActiveObj[1].Objects)
                {
                    obj.SetActive(false);
                }
            }
        }
        else
        {
            if (GameManager.instance != null && GameManager.instance.Gold >= price)
            {
                foreach (GameObject obj in PriceObj[0].Objects)
                {
                    obj.SetActive(false);
                }
                foreach (GameObject obj in PriceObj[1].Objects)
                {
                    obj.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject obj in PriceObj[0].Objects)
                {
                    obj.SetActive(true);
                }
                foreach (GameObject obj in PriceObj[1].Objects)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
