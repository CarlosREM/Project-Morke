using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlaceholderCamera : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
}
