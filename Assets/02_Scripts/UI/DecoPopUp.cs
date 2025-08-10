using UnityEngine;

public class DecoPopUp : MonoBehaviour
{
    [SerializeField] private SubCategoryController decoCategoryController;

    public void Init()
    {
        decoCategoryController.Init();
    }
}
