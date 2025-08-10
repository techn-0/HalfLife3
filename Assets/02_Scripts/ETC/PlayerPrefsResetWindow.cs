#if UNITY_EDITOR_WIN
using _02_Scripts.Shop;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsResetWindow : EditorWindow
{
    [MenuItem("Window/PlayerPrefs Reset")]
    public static void ShowWindow()
    {
        GetWindow<PlayerPrefsResetWindow>("PlayerPrefs Reset");
    }

    private void OnGUI()
    {
        GUILayout.Label("PlayerPrefs 초기화", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Reset PlayerPrefs", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("PlayerPrefs Reset",
                    "모든 PlayerPrefs 데이터를 삭제하시겠습니까?",
                    "삭제", "취소"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("모든 PlayerPrefs가 초기화되었습니다.");
            }
        }
        
        if (GUILayout.Button("AddGold", GUILayout.Height(40)))
        {
                CoinManager.AddCoins(1000);
                Debug.Log("돈 증가됨.");
        }
    }
}
#endif