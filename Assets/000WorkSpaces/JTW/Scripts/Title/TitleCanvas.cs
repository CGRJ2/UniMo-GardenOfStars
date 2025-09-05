using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleCanvas : KYS.BaseUI
{
    private void OnEnable()
    {
        GetEvent("Panel").Click += OnClick;
    }

    private void OnDisable()
    {
        GetEvent("Panel").Click -= OnClick;
    }

    private void OnClick(PointerEventData data)
    {
        Debug.Log("스테이지 씬 로드 ");
    }
}
