using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Playables;

public class CinematicsManager : MonoBehaviour
{
    [Serializable]
    public struct GameCinematic
    {
        public PlayableDirector cinematicSequence;
        public UnityEvent onCinematicEnd;
    }
    
    [SerializeField] private GameCinematic[] cinematicList;

    public static bool IsPlayingCinematic { get; private set; }

    public static event Action OnCinematicPlay;
    public static event Action OnCinematicStop;

    private GameCinematic _currentGameCinematic;
    
    public void PlayCinematic(int index)
    {
        Assert.IsTrue(index.IsInRange(0, cinematicList.Length-1), "Invalid Cinematic index");
        
        _currentGameCinematic = cinematicList[index];
        
        _currentGameCinematic.cinematicSequence.SetActive(true);
        _currentGameCinematic.cinematicSequence.Play();
        
        IsPlayingCinematic = true;
        OnCinematicPlay?.Invoke();
    }
    
    public void StopCinematic()
    {
        var cinematic = _currentGameCinematic.cinematicSequence;
        cinematic.Stop();
        cinematic.SetActive(false);
        IsPlayingCinematic = false;
        OnCinematicStop?.Invoke();
        _currentGameCinematic.onCinematicEnd?.Invoke();
    }
}
