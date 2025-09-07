using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBarUI : MonoBehaviour
{
    public Oxygen oxygen;       // Assign your Oxygen script here
    public Image fillImage;           // The white bar

    void Update()
    {
        if (oxygen != null && fillImage != null)
        {
            fillImage.fillAmount = oxygen.currentOxygen / oxygen.maxOxygen;
        }
    }
}

