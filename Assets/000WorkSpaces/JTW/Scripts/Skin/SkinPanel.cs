using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkinPanel : MonoBehaviour
{
    [SerializeField] private GameObject _skinButtonPrefab;
    [SerializeField] private SkinTypes _type;

    [SerializeField] private SkinViewPanel _viewPanel;

    private List<SkinButton> _buttonList = new List<SkinButton>();

    private void Start()
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

        var sortDict = dict.OrderBy(pair => pair.Value.Cost);

        foreach (var skin in sortDict)
        {
            SkinButton button = Instantiate(_skinButtonPrefab, transform).GetComponent<SkinButton>();

            button.SetInfo(skin.Key, _type, this);

            _buttonList.Add(button);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
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
