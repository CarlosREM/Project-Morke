using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneUtility : MonoBehaviour
{
    [SerializeField, EditorScene] private int nextScene;
    [SerializeField] private float delay;

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(delay);
        
        TransitionManager.TransitionFadeIn();
        TransitionManager.onTransitionInComplete += LoadAfterTransition;
        
        void LoadAfterTransition()
        {
            TransitionManager.onTransitionInComplete -= LoadAfterTransition;
            SceneManager.LoadScene(nextScene);
        }
    }
}
