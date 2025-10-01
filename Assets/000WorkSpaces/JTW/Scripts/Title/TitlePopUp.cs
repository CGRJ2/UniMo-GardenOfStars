using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitlePopUp : KYS.BaseUI
{
    private TextMeshProUGUI _popUpText => GetUI<TextMeshProUGUI>("MessageText");

    private void OnEnable()
    {
        _popUpText.text = GetLocalizedText("ui_network_disconnected_message");
    }
}
