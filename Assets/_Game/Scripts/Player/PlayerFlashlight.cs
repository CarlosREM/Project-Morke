using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject lightObject;
    
    [Header("Parameters")]
    [SerializeField, Min(0)] private float timeForFullRecharge;
    [SerializeField, Min(0)] private float energyDecayPerSec;
    
    public float CurrentEnergy { get; private set; } = 100;
    public bool IsFlashlightOn { get; private set; } = false;
    
    public bool CanRecharge { get; private set; } = false;
    public bool IsRecharging { get; set; }

    private float _currentRechargeTime;

    private float _rotateTowards;

    [SerializeField] private RectTransform rechargeHint;
    private bool _hintShowed;

    private void OnEnable()
    {
        CurrentEnergy = 100;
        CanRecharge = false;
        IsRecharging = false;
        TurnOff();
    }

    private void OnDisable()
    {
        transform.localRotation = Quaternion.identity;
        TurnOff();
    }

    private void Update()
    {
        if (IsFlashlightOn)
        {
            CurrentEnergy -= energyDecayPerSec * Time.deltaTime;
            
            if (CurrentEnergy < 0)
            {
                CurrentEnergy = 0;
                TurnOff();
                CanRecharge = true;
            }
        }

        if (CanRecharge && IsRecharging)
        {
            CurrentEnergy = Mathf.Lerp(0, 100, _currentRechargeTime / timeForFullRecharge);
            
            _currentRechargeTime += Time.deltaTime;
            
            // stop recharging if flashlight is full
            if (CurrentEnergy >= 100)
            {
                IsRecharging = false;
                CanRecharge = false;
                _currentRechargeTime = 0;
            }
        }
        
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, _rotateTowards), 2.5f);

        // show hint only once
        if (!_hintShowed)
        {
            if (rechargeHint.gameObject.activeSelf != CanRecharge)
            {
                rechargeHint.gameObject.SetActive(CanRecharge);
                if (!CanRecharge)
                    _hintShowed = true;
            }
        }
    }

    public void ToggleFlashlight()
    {
        if (IsFlashlightOn)
            TurnOff();
        else
            TurnOn();
    }

    public void TurnOn()
    {
        if (CurrentEnergy <= 0)
            return;
        
        IsFlashlightOn = true;
        
        lightObject.SetActive(true);
    }

    public void TurnOff()
    {
        IsFlashlightOn = false;

        lightObject.SetActive(false);
    }

    public void SetRechargeStatus(bool value)
    {
        IsRecharging = value && CanRecharge;

        if (CanRecharge && !IsRecharging)
        {
            CanRecharge = false;
            _currentRechargeTime = 0;
        }
    }

    public void SetRotation(float value)
    {
        _rotateTowards = value;
    }

    public void AddRotation(float value)
    {
        _rotateTowards += value;
    }
}
