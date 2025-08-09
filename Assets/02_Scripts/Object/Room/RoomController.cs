using UnityEngine;
using AYellowpaper.SerializedCollections;

public class RoomController : MonoBehaviour
{
    [SerializedDictionary] public SerializedDictionary<string, RoomObjectBase> objectsDictionary;

    public void Init()
    {
        UpdateRoomState();
        EventManager.Instance.Subscribe("UpdateRoom", UpdateRoomState);
    }

    public void UpdateRoomState()
    {
        foreach (var kv in objectsDictionary)
        {
            bool flag = PlayerPrefs.GetInt($"{kv.Key}_isActive", 0) == 1;
            bool alreadySpawn = kv.Value.gameObject.activeSelf;
            kv.Value.gameObject.SetActive(flag);
            if (flag && !alreadySpawn)
            {
                kv.Value.ShowInstantiateEffect();
            }
            
        }
    }
}
