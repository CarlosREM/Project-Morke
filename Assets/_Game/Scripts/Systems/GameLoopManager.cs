using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("Game Prefabs")]
    [SerializeField] private PlayerControl playerPrefab;
    [SerializeField] private PlayerCameraManager cameraPrefab;
    [SerializeField] private PlayerHudManager playerHudPrefab;
    
    private LevelManager _currentLevelManager;
    private int _currentCheckpointIndex = 0;

    private PlayerControl _playerRef;
    private PlayerCameraManager _camRef;
    private PlayerHudManager _hudRef;
    
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

    private void Start()
    {
        InitializeLevel();
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(PlayerDeathSequenceCoroutine());
    }

    private IEnumerator PlayerDeathSequenceCoroutine()
    {
        _playerRef.health.enabled = false; // can no longer be hurt, just in case
        _camRef.FocusPlayer();
        _hudRef.PlayerDeathAnim();
        
        // dont move player until transition panel is fully active
        yield return new WaitUntil( ()=> _hudRef.IsCoverOn);
        
        _camRef.SetDamping(Vector3.zero); // remove damping so camera remains static when teleporting the player
        
        yield return null;
        
        // reset player position to the last checkpoint
        var checkpoint = Instance._currentLevelManager.GetLevelCheckpoint(Instance._currentCheckpointIndex);
        _playerRef.transform.position = checkpoint.position;

        // TODO: reset all enemies
        
        // wait til transition panel fades out to return control
        yield return new WaitUntil( ()=> !_hudRef.IsCoverOn);

        _playerRef.GetComponentInChildren<PlayerAnimation>().Reset();
        
        yield return new WaitForSeconds(1f);
        
        _playerRef.health.enabled = true;
        _playerRef.enabled = true; // component disables on death
        
        _camRef.ResetFocus();
    }

    #region Static Methods
    
    public static void InitializeLevel()
    {
        if (!Instance)
            return;
        
        if (!Instance._currentLevelManager)
        {
            // a level manager is REQUIRED in every playable level
            Instance._currentLevelManager = GameObject.FindWithTag("Level Manager").GetComponent<LevelManager>();
        }
        
        Assert.IsNotNull(Instance._currentLevelManager, "No level manager found");

        var checkpoint = Instance._currentLevelManager.GetLevelCheckpoint(Instance._currentCheckpointIndex);
        
        var player = Instantiate(Instance.playerPrefab, checkpoint.transform.position, Quaternion.identity);
        player.health.OnDeath += Instance.OnPlayerDeath;
        
        var playerCamera = Instantiate(Instance.cameraPrefab, checkpoint.transform.position, Quaternion.identity);
        playerCamera.Initialize(player);
        
        var playerHud = Instantiate(Instance.playerHudPrefab, Vector3.zero, Quaternion.identity);
        playerHud.Initialize(player.health);

        Instance._playerRef = player;
        Instance._camRef = playerCamera;
        Instance._hudRef = playerHud;
    }
    

    public static void SetCheckpointIndex(int index)
    {
        if (!Instance)
            return;
        
        Instance._currentCheckpointIndex = index;
    }
    
    #endregion
}
