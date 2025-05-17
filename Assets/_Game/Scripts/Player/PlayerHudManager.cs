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
    [SerializeField] private FMODUnity.StudioEventEmitter sfxHeart;

    [SerializeField] private RectTransform batteryIndicator;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private RectTransform batteryRechargeIcon;
    
    [SerializeField] private TMPro.TextMeshProUGUI objectiveText;

    [SerializeField] private TMPro.TextMeshProUGUI notificationText;
    
    [SerializeField] private PauseMenuController pauseMenu;
    [SerializeField] private TMPro.TextMeshProUGUI pauseMenuObjective;
    
    [Header("World Canvas Parameters")]
    [SerializeField] private Transform worldCanvas;
    [SerializeField] private float deathAnimDuration = 2f;
    [SerializeField] private float postDeathDelay = 1f;

    public static bool CanPause { get; set; }
    public bool IsCoverOn { get; private set; }

    #region Initialization
    
    private void Awake()
    {
        _hudAnimator = GetComponent<Animator>();
        pauseMenu.HudRef = this;

        pauseMenuObjective.text = "";
    }

    private void Start()
    {
        // disable/enable HUD when a cinematic plays/stops, respectively
        CinematicsManager.OnCinematicPlay += OnCinematicStart;
        CinematicsManager.OnCinematicStop += OnCinematicStop;
        
        // on screen text notifs
        LevelManager.OnNewObjective += OnNewObjective;
        LevelManager.OnNotificationSent += OnNotification;

        if (playerRef && playerRef.health)
        {
            playerRef.health.OnHurt += OnPlayerHurt;
            playerRef.health.OnHeal += OnPlayerHeal;
        }
        
        _hudAnimator.SetFloat("HP Speed", 1);
        
        Debug.Log("<color=white>[Player HUD Manager]</color> <color=green>Ready</color>", this);
    }

    private void OnDestroy()
    {
        CinematicsManager.OnCinematicPlay -= OnCinematicStart;
        CinematicsManager.OnCinematicStop -= OnCinematicStop;
        
        LevelManager.OnNewObjective -= OnNewObjective;
        LevelManager.OnNotificationSent -= OnNotification;

        if (playerRef && playerRef.health)
        {
            playerRef.health.OnHurt -= OnPlayerHurt;
            playerRef.health.OnHeal -= OnPlayerHeal;
        }
        
        Debug.Log("<color=white>[Game Input Manager]</color> <color=red>Destroyed</color>", this);
    }

    private void Update()
    {
        if (playerRef.FlashlightActive != batteryIndicator.IsActive())
        {
            if (playerRef.FlashlightActive)
                batteryIndicator.SetActive(true);
            else
            {
                batteryIndicator.SetActive(false);
                return;
            }
                
        }
        
        float energy = playerRef.Flashlight.CurrentEnergy;
        if (energy > 80)
            batterySlider.value = 3;
        else if (energy > 40)
            batterySlider.value = 2;
        else if (energy > 0)
            batterySlider.value = 1;
        else
            batterySlider.value = 0;

        if (playerRef.Flashlight.IsRecharging != batteryRechargeIcon.gameObject.activeSelf)
            batteryRechargeIcon.gameObject.SetActive(playerRef.Flashlight.IsRecharging);
        
        // tutorial fade out
        if (tutorialHints.IsActive() && _currentTutorialDuration < tutorialDuration + tutorialFadeDuration)
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
    }

    private void OnCinematicStart()
    {
        gameOverlay.SetActive(false);
    }

    private void OnCinematicStop()
    {
        gameOverlay.SetActive(true);
    }
    
    #endregion
    
    #region UI

    public void ShowTutorialHints()
    {
        tutorialHints.SetActive(true);
    }
    
    private void OnPlayerHurt(int hurtAmount)
    {
        float animSpeed = _hudAnimator.GetFloat("HP Speed");
        
        animSpeed += speedUpPerMissingHealth * hurtAmount;
        
        _hudAnimator.SetFloat("HP Speed", animSpeed);
        sfxHeart.Params[0].Value = animSpeed;
    }
    
    private void OnPlayerHeal(int healAmount)
    {
        float animSpeed = _hudAnimator.GetFloat("HP Speed");
        
        animSpeed -= speedUpPerMissingHealth * healAmount;
        
        _hudAnimator.SetFloat("HP Speed", animSpeed);
        sfxHeart.Params[0].Value = animSpeed;
    }

    public void EnableObjective(int idx)
    {
        //objectives[idx].gameObject.SetActive(true);
    }
    public void DisableObjective(int idx)
    {
        //objectives[idx].gameObject.SetActive(false);
    }

    private void OnNewObjective(LevelManager.LevelObjective objective)
    {
        objectiveText.text = $"> {objective.objective}";
        pauseMenuObjective.text = objective.objective;
        _hudAnimator.SetTrigger("Show Objective");
    }

    private void OnNotification(string notification)
    {
        notificationText.text = notification;
        _hudAnimator.SetTrigger("Show Notification");
    }
    
    #region Pause Menu

    private bool _previousOverlayStatus;
    
    public void ShowPauseMenu()
    {
        if (!CanPause)
            return;
        
        playerRef.enabled = false;
        _previousOverlayStatus = gameOverlay.IsActive();
        gameOverlay.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        playerRef.enabled = true;
        pauseMenu.SetActive(false);
        gameOverlay.SetActive(_previousOverlayStatus);
    }

    #endregion
    
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
            sfxHeart.Params[0].Value = hpParamDelta;
            currentDuration += Time.deltaTime;
            yield return null;
            
        } while (currentDuration < deathAnimDuration);
        
        _hudAnimator.SetFloat("HP Speed", 1);
        sfxHeart.Params[0].Value = 1;
        
        yield return new WaitForSeconds(postDeathDelay);
        
        // turn cover off
        _hudAnimator.SetTrigger("Cover Trigger");
    }

    #endregion


    public void SetWorldCanvasPosition(Vector3 worldCanvasPosition)
    {
        worldCanvas.position = worldCanvasPosition;
    }
}
