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

    public void PlayCinematic(int index)
    {
        Assert.IsTrue(index.IsInRange(0, cinematicList.Length-1), "Invalid Cinematic index");
        
        cinematicList[index].cinematicSequence.SetActive(true);
        cinematicList[index].cinematicSequence.Play();
        
        IsPlayingCinematic = true;
        OnCinematicPlay?.Invoke();
        StartCoroutine(CinematicCoroutine(cinematicList[index]));
    }

    private IEnumerator CinematicCoroutine(GameCinematic currentCinematic)
    {
        var cinematic = currentCinematic.cinematicSequence;
        while (cinematic.duration - cinematic.time > 0.01)
            yield return null;
        
        cinematic.Stop();
        cinematic.SetActive(false);
        IsPlayingCinematic = false;
        OnCinematicStop?.Invoke();
        currentCinematic.onCinematicEnd?.Invoke();
    }
}
