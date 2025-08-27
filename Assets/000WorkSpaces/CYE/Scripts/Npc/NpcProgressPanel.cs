using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcProgressPanel : MonoBehaviour
{
    public string _itemId;
    [SerializeField] private TMP_Text _targetIdText;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _currentCountText;
    [SerializeField] private TMP_Text _targetCountText;

    public void UpdateItemImage(Sprite itemImage)
    {
        _itemImage.sprite = itemImage;
    }
    public void UpdateItemId(string itemId)
    {
        _targetIdText.text = itemId;
    }
    public void UpdateCurrentCountText(int currentCount)
    {
        _currentCountText.text = currentCount.ToString();
    }
    public void UpdateTargetCountText(int targetCount)
    {
        _targetCountText.text = targetCount.ToString();
    }
}
