using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPurc : MonoBehaviour
{
    public void ClickBtn()
    {
        Debug.LogError("구매 버튼 누름");
    }

    public void Fail()
    {
        Debug.LogError("눌렀는데 실패함");
    }

    public void Complete()
    {
        Debug.LogError("결제 완료");
    }

    
}
