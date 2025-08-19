using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpCanvas_Building : MonoBehaviour
{
    [SerializeField] Button btn_Info;
    public Transform buildingOrigin;
    public void Awake()
    {
        

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, buildingOrigin.position);
        RectTransform canvasRectTransForm = GetComponent<RectTransform>();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransForm, screenPoint, Camera.main, out localPoint))
        {
            //�ｺ���� ������ġ�� ��������Ʈ�� �̵�
            btn_Info.transform.GetComponent<RectTransform>().localPosition = localPoint;
        }
    }
}
