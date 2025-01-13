using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManagerPrefab;
     
    [SerializeField] List<Transform> levelCheckpoints;

    private IEnumerator Start()
    {
        GameLoopManager.CurrentLevelManager = this;
        if (!GameLoopManager.Instance)
        {
            Instantiate(gameLoopManagerPrefab);
            // initialize is called on game loop manager start
        }
        else
        {
            GameLoopManager.InitializeLevel();
        }
        
        yield return null;
        
        GameInputManager.ChangeInputMap("Gameplay");
        
        yield return null;
        
        TransitionManager.TransitionFadeOut();
    }

    public Transform GetLevelCheckpoint(int index)
    {
        Assert.IsTrue(index >= 0 && index < levelCheckpoints.Count, "Invalid level checkpoint index");
        return levelCheckpoints[index];
    }

    public void OnCheckpointReached(int checkpointIndex)
    {
        GameLoopManager.CheckpointIndex = checkpointIndex;
    }
    
    [Header("(Placeholder) Win Conditions")]
    [SerializeField] private int totalBreakers = 2;
    private int _currentBreakers = 0;
    [SerializeField] private GameObject livingRoomLight;
    [SerializeField] private GameObject livingRoomDoor;
    [SerializeField] private FMODUnity.StudioEventEmitter sfxKnock;
    [SerializeField, EditorScene] private int menuScene;
    
    public void OnBreakerEnabled(int index)
    {
        _currentBreakers++;
        GameLoopManager.Instance.HudRef.DisableObjective(index);
        if (_currentBreakers == totalBreakers)
        {
            EnableLivingRoom();
            GameLoopManager.Instance.HudRef.EnableObjective(2);
        }
    }
    
    public void EnableLivingRoom()
    {
        livingRoomLight.SetActive(true);
        livingRoomDoor.SetActive(true);
    }

    public void EndLevel()
    {
        GameLoopManager.Instance.PlayerRef.enabled = false;
        TransitionManager.onTransitionInComplete += EndingTransition;
        TransitionManager.TransitionFadeIn();

        void EndingTransition()
        {
            TransitionManager.onTransitionInComplete -= EndingTransition;
            StartCoroutine(EndingCoroutine());
        }
    }

    IEnumerator EndingCoroutine()
    {
        yield return null;
        
        sfxKnock.Play();

        yield return new WaitForSeconds(2);
        
        SceneManager.LoadSceneAsync(menuScene);
    }
}
