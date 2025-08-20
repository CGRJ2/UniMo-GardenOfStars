using UnityEngine;
using UnityEngine.UI;

public class UIFollowWorldTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform target;               // buildingOrigin 같은 월드 타겟
    [SerializeField] RectTransform uiRect;           // 버튼/패널 RectTransform
    [SerializeField] Canvas canvas;                  // 이 UI가 속한 캔버스
    [Header("Offsets")]
    [SerializeField] Vector2 screenSpaceOffset = new Vector2(0, 20f); // 머리 위로 올리기
    [SerializeField] Vector3 worldSpaceOffset = Vector3.zero;         // World Space Canvas용
    RectTransform canvasRect;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        if (!uiRect) uiRect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (!target || !uiRect || !canvas) return;

        var cam = Camera.main;
        if (!cam) return;

        switch (canvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
                {
                    Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, target.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, null, out var lp);
                    uiRect.anchoredPosition = lp + screenSpaceOffset;
                    break;
                }
            case RenderMode.ScreenSpaceCamera:
                {
                    Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, target.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, canvas.worldCamera, out var lp);
                    uiRect.anchoredPosition = lp + screenSpaceOffset;
                    break;
                }
            case RenderMode.WorldSpace:
                {
                    Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, target.position);
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, sp, canvas.worldCamera, out var wp);
                    uiRect.position = wp + worldSpaceOffset;
                    break;
                }
        }
    }
}
