using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Throwable : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed;
    public float height;

    public Vector3 startPosition;
    public float progress;
}
