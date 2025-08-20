using UnityEngine;
using UnityEngine.UI;

public class UIFollowWorldTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform target;               // buildingOrigin ���� ���� Ÿ��
    [SerializeField] RectTransform uiRect;           // ��ư/�г� RectTransform
    [SerializeField] Canvas canvas;                  // �� UI�� ���� ĵ����
    [Header("Offsets")]
    [SerializeField] Vector2 screenSpaceOffset = new Vector2(0, 20f); // �Ӹ� ���� �ø���
    [SerializeField] Vector3 worldSpaceOffset = Vector3.zero;         // World Space Canvas��
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
