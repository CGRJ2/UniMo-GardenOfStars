using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSwitchPanel : KYS.BaseUI
{
    [SerializeField] private GameObject _characterSkinPanel;
    [SerializeField] private GameObject _equipSkinPanel;

    [SerializeField] private SkinViewPanel _viewPanel;

    private Button _characterButton => GetUI<Button>("CharacterSkinButton");
    private Button _equipButton => GetUI<Button>("EquipSkinButton");

    protected override void Awake()
    {
        base.Awake();
        _characterButton.onClick.AddListener(() =>
        {
            _viewPanel.ClearAvatar();

            _characterSkinPanel.SetActive(true);
            _equipSkinPanel.SetActive(false);
        });

        _equipButton.onClick.AddListener(() =>
        {
            _viewPanel.ClearAvatar();

            _characterSkinPanel.SetActive(false);
            _equipSkinPanel.SetActive(true);
        });
    }
}
