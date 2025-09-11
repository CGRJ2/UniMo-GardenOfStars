using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lamp_QuestContent : MonoBehaviour
{
    [SerializeField] Image image_Default;
    [SerializeField] Image image_Clear;

    public void UpdateView(bool isClear)
    {
        if (isClear)
        {
            image_Clear.gameObject.SetActive(true);
            image_Default.gameObject.SetActive(false);
        }
        else
        {
            image_Clear.gameObject.SetActive(false);
            image_Default.gameObject.SetActive(true);
        }
    }
}
