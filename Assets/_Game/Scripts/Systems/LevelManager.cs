using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Current { get; private set; }
    
    [SerializeField] private GameLoopManager gameLoopManagerPrefab;
    [SerializeField] private CinematicsManager levelCinematics;
    [field: SerializeField] public bool firstSpawnWithoutFlashlight { get; private set; }
    [SerializeField] private bool startWithCinematic;
     
    [SerializeField] List<Transform> levelCheckpoints;

    private IEnumerator Start()
    {
        Debug.Log("<color=white>[Level Manager]</color> <color=yellow>Level Setup starting...</color>", this);
        
        Current = this;
        if (!GameLoopManager.Instance)
        {
            Instantiate(gameLoopManagerPrefab);
            // initialize is called on game loop manager start
        }
        
        yield return null;
        
        GameLoopManager.InitializeLevel();

        yield return null;
        
        GameInputManager.ChangeInputMap("Gameplay");
        Checkpoint.OnCheckpointActivated += OnCheckpointReached;
        
        if (firstSpawnWithoutFlashlight)
        {
            GameLoopManager.Instance.PlayerRef.FlashlightActive = false;
        }
        
        yield return null;
        
        TransitionManager.TransitionFadeOut();

        Debug.Log("<color=white>[Level Manager]</color> <color=green>Setup Complete, Level starting</color>", this);
        
        if (startWithCinematic)
        {
            GameLoopManager.Instance.PlayerRef.SetActive(false);
            levelCinematics.PlayCinematic(0);
            yield return new WaitUntil(() => !CinematicsManager.IsPlayingCinematic);
            GameLoopManager.Instance.PlayerRef.SetActive(true);
        }
        
    }

    private void OnDestroy()
    {
        Current = null;
    }

    public Transform GetLevelCheckpoint(int index)
    {
        Assert.IsTrue(index >= 0 && index < levelCheckpoints.Count, "Invalid level checkpoint index");
        return levelCheckpoints[index];
    }

    private void OnCheckpointReached(Checkpoint checkpoint)
    {
        int i = levelCheckpoints.IndexOf(checkpoint.transform);
        Assert.IsTrue(i >= 0, "Invalid checkpoint, needs to be added to Checkpoint list first"); 
        GameLoopManager.CheckpointIndex = i;
    }
    
    #region REMOVE THIS
    
    [Header("(Placeholder) Win Conditions")]
    [SerializeField] private int totalBreakers = 2;
    private int _currentBreakers = 0;
    [SerializeField] private GameObject livingRoomLight;
    [SerializeField] private GameObject livingRoomDoor;
    [SerializeField] private FMODUnity.StudioEventEmitter sfxKnock;
    [SerializeField, EditorScene] private int menuScene;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
    
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
        Checkpoint.OnCheckpointActivated -= OnCheckpointReached;

        TransitionManager.onTransitionInComplete += EndingTransition;
        TransitionManager.TransitionFadeIn();

        void EndingTransition()
        {
            TransitionManager.onTransitionInComplete -= EndingTransition;
            StartCoroutine(EndingCutscene());
        }
    }

    IEnumerator EndingCutscene()
    {
        yield return null;
        
        sfxKnock.Play();

        yield return ShowText("Papa, are you here...?", 2, 3, 0.1f);

        yield return FadeOutText(1f);
        
        yield return ShowText("... Papa...?", 2, 3, 0.2f);
        
        yield return FadeOutText(1f);

        yield return new WaitForSeconds(1f);
        
        SceneManager.LoadSceneAsync(menuScene);
    }

    IEnumerator ShowText(string text, float delayBefore = 0, float delayAfter = 0, float delayPerLetter = 0.2f)
    {
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = text;
        dialogueText.color = Color.white;
        
        if (delayBefore > 0)
            yield return new WaitForSeconds(delayBefore);
        

        while (dialogueText.maxVisibleCharacters < text.Length)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(delayPerLetter);
        }
        
        if (delayAfter > 0)
            yield return new WaitForSeconds(delayAfter);
    }

    IEnumerator FadeOutText(float duration)
    {
        float delta = 0;
        Color textColor = dialogueText.color;
        while (textColor.a > 0)
        {
            textColor.a = Mathf.Lerp(1, 0, delta/duration);
            dialogueText.color = textColor;
            
            yield return null;
            
            delta += Time.deltaTime;
        }
    }
    
    #endregion
}
