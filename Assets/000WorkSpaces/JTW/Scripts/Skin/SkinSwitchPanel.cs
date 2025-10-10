using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSwitchPanel : KYS.BaseUI
{
    [SerializeField] private Sprite _switchImage;
    [SerializeField] private Sprite _switchActiveImage;

    [SerializeField] private GameObject _characterSkinPanel;
    [SerializeField] private GameObject _equipSkinPanel;

    [SerializeField] private SkinViewPanel _viewPanel;

    private Button _characterButton => GetUI<Button>("CharacterSkinButton");
    private Button _equipButton => GetUI<Button>("EquipSkinButton");

    private TextMeshProUGUI _characterText => GetUI<TextMeshProUGUI>("CharacterSkinButtonText");
    private TextMeshProUGUI _equipText => GetUI<TextMeshProUGUI>("EquipSkinButtonText");

    protected override void Awake()
    {
        base.Awake();
        _characterButton.onClick.AddListener(() =>
        {
            _viewPanel.ClearAvatar();

            _characterSkinPanel.SetActive(true);
            _equipSkinPanel.SetActive(false);

            _characterButton.image.sprite = _switchActiveImage;
            _equipButton.image.sprite = _switchImage;

            _characterButton.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            _equipButton.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            _characterText.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            _equipText.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        });

        _equipButton.onClick.AddListener(() =>
        {
            _viewPanel.ClearAvatar();

            _characterSkinPanel.SetActive(false);
            _equipSkinPanel.SetActive(true);

            _characterButton.image.sprite = _switchImage;
            _equipButton.image.sprite = _switchActiveImage;

            _characterButton.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
            _equipButton.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));

            _characterText.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
            _equipText.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
        });
    }
}
