using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Animator))]
public class PlayerHudManager : MonoBehaviour
{
    [SerializeField] private PlayerControl playerRef;
    private Animator _hudAnimator;

    [SerializeField] private float speedUpPerMissingHealth = 1.5f;
    
    [SerializeField] private float deathAnimDuration = 2f;
    [SerializeField] private float postDeathDelay = 1f;

    [SerializeField] private Transform worldCanvas;
    [SerializeField] private PauseMenuController pauseMenu;
    
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
        pauseMenu.gameObject.SetActive(true);
    }

    public void HidePauseMenu()
    {
        playerRef.enabled = true;
        pauseMenu.gameObject.SetActive(false);
    }

    #endregion

    public void SetWorldCanvasPosition(Vector3 worldCanvasPosition)
    {
        worldCanvas.position = worldCanvasPosition;
    }
}
