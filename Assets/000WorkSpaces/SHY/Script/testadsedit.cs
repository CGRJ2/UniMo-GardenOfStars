using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(testads))]
public class TestAdsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        testads ads = (testads)target;

        EditorGUILayout.LabelField("광고 상태 관리", EditorStyles.boldLabel);

        ads.adRemoved = EditorGUILayout.Toggle("광고 제거 구매", ads.adRemoved);

        if (GUILayout.Button("현재 광고 상태 적용해보기"))
        {
            ads.ApplyBannerState();
        }

        DrawDefaultInspector(); // UnityEvent 등 기본 필드도 표시
    }
}
