using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField, EditorScene] private int playScene; 
    
    public IEnumerator Start()
    {
        GameInputManager.ChangeInputMap("UI");
        
        yield return null; // wait for everything to load before fade out, looks better
        
        TransitionManager.TransitionFadeOut();
    }

    public void OnPlayPressed()
    {
        TransitionManager.OnTransitionInComplete += GoToLevel;
        TransitionManager.TransitionFadeIn();

        void GoToLevel()
        {
            TransitionManager.OnTransitionInComplete -= GoToLevel;
            SceneManager.LoadSceneAsync(playScene);
        }
    }
    
    public void OnQuitPressed()
    {
        TransitionManager.OnTransitionInComplete += QuitAfterTransition;
        TransitionManager.TransitionFadeIn();

        void QuitAfterTransition()
        {
            TransitionManager.OnTransitionInComplete -= QuitAfterTransition;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
    }

}
