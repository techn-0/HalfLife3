using System;
using _02_Scripts.Shop;
using UnityEngine;

public class GameManager : BaseSingleton<GameManager>
{
    // 정적(static) 변수로 유일한 인스턴스를 저장
    public static GameManager instance = null;
    
    public DecoPopUp decoPopUp;
    public RoomController roomController;
    public Overlay overlay;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Start()
    {
        if (!PlayerPrefs.HasKey("Human1_isOpen"))
        {
            PlayerPrefs.SetInt("Human1_isOpen", 1);
            PlayerPrefs.SetInt("Human1_isActive", 1);
        }
        
        decoPopUp.Init();
        roomController.Init();
        overlay.Init();
    }
}