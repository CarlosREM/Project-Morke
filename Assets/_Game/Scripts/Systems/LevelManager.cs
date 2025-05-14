using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Serializable]
    public struct LevelObjective
    {
        public string objective;
        [Min(0)] public int maxProgress;
        [HideInInspector] public int currentProgress;

        [SerializeField] UnityEvent onStarted;
        [SerializeField] UnityEvent onComplete;
        
        public void ExecuteOnStarted() => onStarted?.Invoke();
        public void ExecuteOnComplete() => onComplete?.Invoke();
    }
    
    public static LevelManager Current { get; private set; }
    
    [SerializeField] private GameLoopManager gameLoopManagerPrefab;
    [SerializeField] private CinematicsManager levelCinematics;
    [field: SerializeField] public bool firstSpawnWithoutFlashlight { get; private set; }
    [SerializeField] private bool startWithCinematic;
     
    [SerializeField] List<Transform> levelCheckpoints;

    public static event Action<string> OnNotificationSent;
    
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
        else
        {
            UpdateObjective();
        }
        
    }

    private void OnDestroy()
    {
        Current = null;
    }

    #region Checkpoints
    
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
        
        OnNotificationSent?.Invoke("<color=yellow>Checkpoint reached</color>");
    }
    
    #endregion
    
    #region Inventory
    
    private readonly List<string> _currentInventory = new();


    public void AddToInventory(string item)
    {
        Assert.IsTrue(!HasItemInInventory(item), "Invalid item, already in inventory");
        _currentInventory.Add(item);
        
        OnNotificationSent?.Invoke($"<color=yellow>Item found:</color> \"{item}\"");
    }

    public bool HasItemInInventory(string item)
    {
        return _currentInventory.Contains(item);
    }
    
    #endregion
    
    #region Objective System

    [SerializeField] private List<LevelObjective> levelObjectives;
    public int CurrentObjectiveIndex { get; private set; } = -1;
    public LevelObjective CurrentObjectiveInfo => levelObjectives[CurrentObjectiveIndex];
    public static event Action<int, LevelObjective> OnObjectiveProgress;
    public static event Action<LevelObjective> OnNewObjective;
    
    public void ProgressObjective()
    {
        var objective = CurrentObjectiveInfo;
        objective.currentProgress += 1;

        if (objective.currentProgress >= objective.maxProgress)
        {
            objective.ExecuteOnComplete();
        }
        else
        {
            levelObjectives[CurrentObjectiveIndex] = objective;
            OnObjectiveProgress?.Invoke(CurrentObjectiveIndex, objective);
        }
        
    }

    public void UpdateObjective()
    {
        CurrentObjectiveIndex++;
        if (CurrentObjectiveIndex < levelObjectives.Count)
        {
            CurrentObjectiveInfo.ExecuteOnStarted();
            OnNewObjective?.Invoke(CurrentObjectiveInfo);
        }
        else
        {
            // all objectives completed, end level
            SceneManager.LoadSceneAsync(menuScene);
        }
    }
    
    #endregion
    
    #region Miscellaneous Actions
    
    public void PlayerDialogue(string dialogue)
    {
        GameLoopManager.Instance.PlayerRef.PlayerDialogue.SetDialogue(dialogue);
    }
    public void PlayerDialogue(string dialogue, float duration)
    {
        GameLoopManager.Instance.PlayerRef.PlayerDialogue.SetDialogue(dialogue, duration);
    }

    public void SetPlayerFlashlightStatus(bool newStatus)
    {
        GameLoopManager.Instance.PlayerRef.FlashlightActive = newStatus;
    }

    public void SetPlayerControlStatus(bool newStatus, bool playerActive)
    {
        GameLoopManager.Instance.PlayerRef.enabled = newStatus;
        GameLoopManager.Instance.PlayerRef.SetActive(playerActive);
    }

    public void ShowTutorial()
    {
        GameLoopManager.Instance.HudRef.ShowTutorialHints();
    }
    
    #endregion
    
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

        TransitionManager.OnTransitionInComplete += EndingTransition;
        TransitionManager.TransitionFadeIn();

        void EndingTransition()
        {
            TransitionManager.OnTransitionInComplete -= EndingTransition;
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
