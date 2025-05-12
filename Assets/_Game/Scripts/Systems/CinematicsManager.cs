using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

public class CinematicsManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector[] cinematicList;

    public static bool IsPlayingCinematic { get; private set; }

    public static event Action OnCinematicPlay;
    public static event Action OnCinematicStop;

    public void PlayCinematic(int index)
    {
        Assert.IsTrue(index.IsInRange(0, cinematicList.Length-1), "Invalid Cinematic index");
        
        cinematicList[index].SetActive(true);
        cinematicList[index].Play();
        
        IsPlayingCinematic = true;
        OnCinematicPlay?.Invoke();
        StartCoroutine(CinematicCoroutine(cinematicList[index]));
    }

    private IEnumerator CinematicCoroutine(PlayableDirector currentCinematic)
    {
        while (currentCinematic.duration - currentCinematic.time > 0.005)
            yield return null;
        
        currentCinematic.Stop();
        currentCinematic.SetActive(false);
        IsPlayingCinematic = false;
        OnCinematicStop?.Invoke();
    }
}
