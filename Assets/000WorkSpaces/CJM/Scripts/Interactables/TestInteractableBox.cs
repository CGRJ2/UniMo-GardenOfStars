using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractableBox : InteractableBase
{
    public override void Interact()
    {
        base.Interact();
        Debug.Log("테스트 박스 상호작용");
    }

    public override void ShowInteractableUI()
    {
        base.ShowInteractableUI();
        Debug.Log("테스트 박스 UI 활성화");
    }

    public override void CloseInteractableUI()
    {
        base.CloseInteractableUI();
        Debug.Log("테스트 박스 UI 비활성화");
    }
}
