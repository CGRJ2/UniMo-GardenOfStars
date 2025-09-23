using System.Collections.Generic;
using UnityEngine;

public class SkinPanel : MonoBehaviour
{
    [SerializeField] private GameObject _skinButtonPrefab;
    [SerializeField] private SkinTypes _type;

    [SerializeField] private SkinViewPanel _viewPanel;

    private List<SkinButton> _buttonList = new List<SkinButton>();

    private void Awake()
    {
        Dictionary<string, SkinDataCsv> dict;

        if (_type == SkinTypes.Character)
        {
            dict = Manager.data.CharacterSkin.Values;
        }
        else
        {
            dict = Manager.data.EquipSkin.Values;
        }

        foreach (var skin in dict)
        {
            SkinButton button = Instantiate(_skinButtonPrefab, transform).GetComponent<SkinButton>();

            button.SetInfo(skin.Key, _type, this);

            _buttonList.Add(button);
        }
    }

    public void SetButtonsInfo()
    {
        foreach(SkinButton button in _buttonList)
        {
            button.UpdateInfo();
        }
    }

    public void SetView(string id)
    {
        _viewPanel.SetInfo(id, _type);
    }
}
