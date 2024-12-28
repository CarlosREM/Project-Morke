using System;
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

    private void OnEnable()
    {
        CurrentEnergy = 100;
        CanRecharge = false;
        IsRecharging = false;
        TurnOff();
    }

    private void OnDisable()
    {
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
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, value), 5);
    }
}
