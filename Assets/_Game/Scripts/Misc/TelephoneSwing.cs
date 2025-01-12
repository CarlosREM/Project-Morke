using System;
using UnityEngine;

public class TelephoneSwing : MonoBehaviour
{
    [SerializeField] private float timeForSwing;
    private float _currentSwingTime;

    private Vector3 _startRotation, _currentRotation;

    private void Start()
    {
        _startRotation = transform.eulerAngles;
        _currentRotation = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float t = _currentSwingTime / timeForSwing;
        t = t * t * (3f - 2f * t);

        _currentRotation.z = Mathf.Lerp(_startRotation.z, -_startRotation.z, t);
        transform.rotation = Quaternion.Euler(_currentRotation);

        _currentSwingTime += Time.deltaTime;
        if (_currentSwingTime >= timeForSwing)
        {
            _currentSwingTime = 0;
            _startRotation.z = -_startRotation.z;
        }
    }
}
