using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOff : MonoBehaviour
{
    public TMPro.TMP_Text nameDisplayText;

    public void OnClickChange()
    {
        if (nameDisplayText.text == "ON")
        {
            nameDisplayText.text = "OFF";
        } else {
            nameDisplayText.text = "ON";
        }
    }
}
