using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp1 : MonoBehaviour
{
    public Light spotlight;

    private bool isLightOn = false;

    void Start()
    {
        if (spotlight == null)
        {
            spotlight = GetComponent<Light>();
        }
    }

    void OnMouseDown()
    {
        isLightOn = !isLightOn;

        spotlight.enabled = isLightOn;
    }
}
