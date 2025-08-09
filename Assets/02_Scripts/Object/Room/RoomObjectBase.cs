using UnityEngine;

public class RoomObjectBase : MonoBehaviour
{
    [SerializeField] private float EffectScale = 1f;

    public void ShowInstantiateEffect()
    {
        GameObject go = Instantiate(EffectManager.Instance.smokeEffect, transform.position, Quaternion.identity);
        go.transform.localScale = go.transform.localScale * EffectScale;
    }
}
