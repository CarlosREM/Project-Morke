using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PlayerHudManager : MonoBehaviour
{
    [SerializeField] private PlayerControl playerRef;
    private Animator _hudAnimator;

    [Header("HUD Parameters")] 
    [SerializeField] private RectTransform gameOverlay;
    [SerializeField] private CanvasGroup tutorialHints;
    [SerializeField] private float tutorialDuration;
    [SerializeField] private float tutorialFadeDuration;
                     private float _currentTutorialDuration;
    [SerializeField] private float speedUpPerMissingHealth = 1.5f;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private RectTransform batteryRechargeIcon;
    [SerializeField] private PauseMenuController pauseMenu;
    
    [Header("World Canvas Parameters")]
    [SerializeField] private Transform worldCanvas;
    [SerializeField] private float deathAnimDuration = 2f;
    [SerializeField] private float postDeathDelay = 1f;

    
    public bool IsCoverOn { get; private set; }

    #region Initialization
    
    private void Awake()
    {
        _hudAnimator = GetComponent<Animator>();
        pauseMenu.HudRef = this;
    }

    private void OnEnable()
    {
        if (!playerRef || !playerRef.health)
            return;
        
        playerRef.health.OnHurt += OnPlayerHurt;
        playerRef.health.OnHeal += OnPlayerHeal;
        
        _hudAnimator.SetFloat("HP Speed", 1);
    }

    private void OnDisable()
    {
        if (!playerRef || !playerRef.health)
            return;
        
        playerRef.health.OnHurt -= OnPlayerHurt;
        playerRef.health.OnHeal -= OnPlayerHeal;
    }

    private void Update()
    {
        float energy = playerRef.flashlight.CurrentEnergy;
        if (energy > 80)
            batterySlider.value = 3;
        else if (energy > 40)
            batterySlider.value = 2;
        else if (energy > 0)
            batterySlider.value = 1;
        else
            batterySlider.value = 0;

        if (playerRef.flashlight.IsRecharging != batteryRechargeIcon.gameObject.activeSelf)
            batteryRechargeIcon.gameObject.SetActive(playerRef.flashlight.IsRecharging);
        
        // tutorial fade out
        if (_currentTutorialDuration < tutorialDuration + tutorialFadeDuration)
        {
            _currentTutorialDuration += Time.deltaTime;
            if (_currentTutorialDuration > tutorialDuration)
            {
                tutorialHints.alpha = Mathf.Lerp(1, 0, (_currentTutorialDuration-tutorialDuration) / tutorialFadeDuration);
                if (tutorialHints.alpha <= 0)
                    tutorialHints.gameObject.SetActive(false);
            }
        }
    }

    public void Initialize(PlayerControl player)
    {
        playerRef = player;
        player.OnPauseTriggered += ShowPauseMenu;
        this.OnEnable();
    }

    #endregion
    
    #region HP widget
    
    private void OnPlayerHurt(int hurtAmount)
    {
        float animSpeed = _hudAnimator.GetFloat("HP Speed");
        
        animSpeed += speedUpPerMissingHealth * hurtAmount;
        
        _hudAnimator.SetFloat("HP Speed", animSpeed);
    }
    
    private void OnPlayerHeal(int healAmount)
    {
        float animSpeed = _hudAnimator.GetFloat("HP Speed");
        
        animSpeed -= speedUpPerMissingHealth * healAmount;
        
        _hudAnimator.SetFloat("HP Speed", animSpeed);
    }

    #endregion
    
    #region On Player Death

    public void SetCoverOn(int value)
    {
        Assert.IsTrue(value == 0 || value == 1, "SetCoverOn value should be 0 or 1");
        IsCoverOn = value == 1;
        
        //Debug.Log($"Set Cover On = {IsCoverOn} ({value})");
    }

    public void PlayerDeathAnim()
    {
        StartCoroutine(PlayerDeathAnimCoroutine());
    }
    
    private IEnumerator PlayerDeathAnimCoroutine()
    {
        // trigger anim, wait til cover is on
        _hudAnimator.SetTrigger("Cover Trigger");
        yield return new WaitUntil(() => IsCoverOn);

        // slow down heart beat anim speed until its set to 1
        float hpParamValue = _hudAnimator.GetFloat("HP Speed"),
                currentDuration = 0,
                hpParamDelta;
        do
        {
            hpParamDelta = Mathf.Lerp(hpParamValue, 1, currentDuration / deathAnimDuration);
            _hudAnimator.SetFloat("HP Speed", hpParamDelta);
            currentDuration += Time.deltaTime;
            yield return null;
        } while (currentDuration < deathAnimDuration);
        
        
        yield return new WaitForSeconds(postDeathDelay);
        
        // turn cover off
        _hudAnimator.SetTrigger("Cover Trigger");
    }

    #endregion

    #region Pause Menu
    
    public void ShowPauseMenu()
    {
        playerRef.enabled = false;
        gameOverlay.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(true);
    }

    public void HidePauseMenu()
    {
        playerRef.enabled = true;
        pauseMenu.gameObject.SetActive(false);
        gameOverlay.gameObject.SetActive(true);
    }

    #endregion

    public void SetWorldCanvasPosition(Vector3 worldCanvasPosition)
    {
        worldCanvas.position = worldCanvasPosition;
    }
}
