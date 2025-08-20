using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

public class JoystickPanel : MonoBehaviour
{
    void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();

        Rect safeArea = Screen.safeArea;

        rect.anchorMin = new Vector2(0, safeArea.yMin / Screen.height);
        rect.anchorMin = new Vector2(0, safeArea.yMin / Screen.height);
    }
}
