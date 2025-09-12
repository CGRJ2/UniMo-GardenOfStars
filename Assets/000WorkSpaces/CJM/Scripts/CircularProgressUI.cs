using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularProgressUI : MonoBehaviour
{
    [SerializeField] Image image_Fill;

    public void SetValue(float value)
    {
        image_Fill.fillAmount = value;
    }
}
