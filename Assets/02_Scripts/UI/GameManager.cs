using UnityEngine;

public class GameManager : BaseSingleton<GameManager>
{
    // 정적(static) 변수로 유일한 인스턴스를 저장
    public static GameManager instance = null;
    
    public DecoPopUp decoPopUp;

    public int Gold;
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
        decoPopUp.Init();   
    }
}