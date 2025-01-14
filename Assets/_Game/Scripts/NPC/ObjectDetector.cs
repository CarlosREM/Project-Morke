using System;
using UnityEngine;

public class ObjectDetector : MonoBehaviour
{
    [SerializeField, TagSelector] private string objectTag;

    [SerializeField] private float detectionDelay;
    
    public bool HasDetectedObj { get; private set; }

    private GameObject _currentTarget;
    private float _currentDelay;
    
}
