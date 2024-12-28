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
        StartCoroutine(LoopResetCoroutine());
    }

    private IEnumerator LoopResetCoroutine()
    {
        yield return new WaitForSeconds(2);
        
        // fade in transition panel
        yield return null;

        // disable player objects so everything resets
        _playerRef.gameObject.SetActive(false);
        _hudRef.enabled = false;
        
        // reset player position to the last checkpoint
        var checkpoint = Instance._currentLevelManager.GetLevelCheckpoint(Instance._currentCheckpointIndex);
        _playerRef.transform.position = checkpoint.position;

        yield return null;
        
        _playerRef.gameObject.SetActive(true);
        _playerRef.enabled = true; // component disables on death
        _hudRef.enabled = true;
        
        // fade out transition panel
        yield return null;
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
