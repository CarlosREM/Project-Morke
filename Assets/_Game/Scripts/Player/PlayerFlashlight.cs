using System;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject lightObject;

    [Header("Parameters")] 
    [SerializeField] private bool DEBUG_decayDisabled;
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

    [SerializeField] private FMODUnity.StudioEventEmitter sfxRefLoop;
    [SerializeField] private FMODUnity.StudioEventEmitter sfxRefOff;
    [SerializeField] private float lowEnergyThreshold = 40;
    private bool _lowEnergyEnabled;
    
    private void OnEnable()
    {
        CurrentEnergy = 100;
        CanRecharge = false;
        IsRecharging = false;
        IsFlashlightOn = false;
        lightObject.SetActive(false);

        _lowEnergyEnabled = false;
        sfxRefLoop.Params[0].Value = 0;
        sfxRefOff.Params[0].Value = 0;
    }

    private void OnDisable()
    {
        transform.localRotation = Quaternion.identity;
        
        IsFlashlightOn = false;
        lightObject.SetActive(false);
        sfxRefLoop.Stop();
    }

    private void Update()
    {
        if (IsFlashlightOn && !DEBUG_decayDisabled)
        {
            CurrentEnergy -= energyDecayPerSec * Time.deltaTime;
            
            if (CurrentEnergy < 0)
            {
                CurrentEnergy = 0;
                sfxRefOff.Params[0].Value = 1;
                TurnOff();
                CanRecharge = true;
            }
            else if (!_lowEnergyEnabled && CurrentEnergy < lowEnergyThreshold)
            {
                _lowEnergyEnabled = true;
                sfxRefLoop.SetParameter(sfxRefLoop.Params[0].ID, 1);
            }
        }

        if (CanRecharge && IsRecharging)
        {
            CurrentEnergy = Mathf.Lerp(0, 100, _currentRechargeTime / timeForFullRecharge);
            
            _currentRechargeTime += Time.deltaTime;
            
            // stop recharging if flashlight is full
            if (CurrentEnergy >= 100)
            {
                SetRechargeStatus(false);
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
        {
            sfxRefOff.Play();
            return;
        }

        IsFlashlightOn = true;
        
        lightObject.SetActive(true);
        sfxRefLoop.Play();
    }

    public void TurnOff()
    {
        IsFlashlightOn = false;

        lightObject.SetActive(false);
        sfxRefLoop.Stop();
        sfxRefOff.Play();
        _lowEnergyEnabled = false;
    }

    public void SetRechargeStatus(bool value)
    {
        if (!CanRecharge)
            return;
        
        IsRecharging = value;

        if (!IsRecharging)
        {
            CanRecharge = false;
            _currentRechargeTime = 0;
            
            sfxRefLoop.Params[0].Value = 0;
            sfxRefOff.Params[0].Value = 0;
        }
    }

    public void SetRotation(float value)
    {
        _rotateTowards = value;
    }
    
    public void FlipRotation(bool isRight)
    {
        _rotateTowards = 180 - _rotateTowards;
        transform.localRotation = Quaternion.Euler(0, 0, _rotateTowards);
    }
}
