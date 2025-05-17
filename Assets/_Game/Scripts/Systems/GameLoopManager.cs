using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }
    
    [Header("Game Prefabs")]
    #if UNITY_EDITOR
    // editor only, since these should come from the very first scene
    [SerializeField] private GameInputManager gameInputManager; 
    [SerializeField] private TransitionManager transitionManager;
    #endif
    [SerializeField] private PlayerControl playerPrefab;
    [SerializeField] private PlayerCameraManager cameraPrefab;
    [SerializeField] private PlayerHudManager playerHudPrefab;

    public static Transform CurrentCheckpoint { get; set; }

    public PlayerControl PlayerRef { get; private set; }
    public PlayerCameraManager CamRef { get; private set; }
    public PlayerHudManager HudRef { get; private set; }

    [Header("SFX")] 
    [SerializeField] private FMODUnity.EventReference sfxOnDead;
    
    public static event Action OnPlayerDeadReset; 
    
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnPlayerDeath()
    {
        Debug.Log("<color=white>[Game Loop Manager]</color> <color=green>Playing death sequence...</color>", this);

        StartCoroutine(PlayerDeathSequenceCoroutine());
    }

    private IEnumerator PlayerDeathSequenceCoroutine()
    {
        PlayerRef.health.enabled = false; // can no longer be hurt, just in case
        CamRef.FocusPlayer();
        HudRef.PlayerDeathAnim();
        
        FMODUnity.RuntimeManager.PlayOneShot(sfxOnDead);
        
        HudRef.SetWorldCanvasPosition(PlayerRef.transform.position);
        
        // dont move player until transition panel is fully active
        yield return new WaitUntil( ()=> HudRef.IsCoverOn);
        
        CamRef.SetDamping(Vector3.zero); // remove damping so camera remains static when teleporting the player
        
        yield return null;
        
        // reset player position to the last checkpoint
        PlayerRef.transform.position = CurrentCheckpoint.position;
        HudRef.SetWorldCanvasPosition(CurrentCheckpoint.position); // don't let the hud stay behind or the illusion breaks!
        
        OnPlayerDeadReset?.Invoke();
        
        // wait til transition panel fades out to return control
        yield return new WaitUntil( ()=> !HudRef.IsCoverOn);

        PlayerRef.PlayerAnim.Reset();
        
        yield return new WaitForSeconds(1f);
        
        PlayerRef.health.enabled = true;
        PlayerRef.enabled = true; // component disables on death
        
        CamRef.ResetFocus();
    }

    #region Static Methods
    
    public static void InitializeLevel()
    {
        #if UNITY_EDITOR
        if (!GameInputManager.Instance)
        {
            // just making sure input object exists
            Instantiate(Instance.gameInputManager);
        }

        if (!TransitionManager.Instance)
        {
            Instantiate(Instance.transitionManager);
        }
        #endif
        
        Assert.IsNotNull(Instance, "Game Loop Manager instance hasn't been initialized.");
        
        // a level manager is REQUIRED in every playable level
        Assert.IsNotNull(LevelManager.Current, "Level Manager instance hasn't been initialized.");

        CurrentCheckpoint = LevelManager.Current.StartingCheckpoint;
        
        var player = Instantiate(Instance.playerPrefab, CurrentCheckpoint.position, Quaternion.identity);
        player.health.OnDeath += Instance.OnPlayerDeath;
        
        var playerCamera = Instantiate(Instance.cameraPrefab, CurrentCheckpoint.position, Quaternion.identity);
        playerCamera.Initialize(player);
        
        var playerHud = Instantiate(Instance.playerHudPrefab, Vector3.zero, Quaternion.identity);
        playerHud.Initialize(player);

        Instance.PlayerRef = player;
        Instance.CamRef = playerCamera;
        Instance.HudRef = playerHud;
        
        Debug.Log("<color=white>[Game Loop Manager]</color> <color=green>Level Initialized</color>", Instance);
    }
    
    public static void ExitGameLoop()
    {
        TransitionManager.OnTransitionInComplete += OnTransitionInComplete;

        TransitionManager.TransitionFadeIn();
        
        
        void OnTransitionInComplete()
        {
            Debug.Log("<color=white>[Game Loop Manager]</color> <color=yellow>Exiting game loop...</color>", Instance);

            TransitionManager.OnTransitionInComplete -= OnTransitionInComplete;

            SceneManager.LoadSceneAsync("MainMenu"); // 1 should be main menu
            Destroy(Instance.gameObject);
        }
        
    }
    
    #endregion
}
