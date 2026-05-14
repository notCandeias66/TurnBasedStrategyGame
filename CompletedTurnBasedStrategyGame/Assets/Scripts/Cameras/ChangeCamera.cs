using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public Camera newCamera;

    public void CameraChange()
    {
        Camera currentCamera = Camera.main;
        if (currentCamera != null)
        {
            currentCamera.gameObject.tag = "Untagged";
            currentCamera.gameObject.SetActive(false);
        }

        newCamera.gameObject.tag = "MainCamera";
        newCamera.gameObject.SetActive(true);
    }
}