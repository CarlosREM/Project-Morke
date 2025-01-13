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
        TransitionManager.onTransitionInComplete += GoToLevel;
        TransitionManager.TransitionFadeIn();

        void GoToLevel()
        {
            TransitionManager.onTransitionInComplete -= GoToLevel;
            SceneManager.LoadSceneAsync(playScene);
        }
    }
    
    public void OnQuitPressed()
    {
        TransitionManager.onTransitionInComplete += QuitAfterTransition;
        TransitionManager.TransitionFadeIn();

        void QuitAfterTransition()
        {
            TransitionManager.onTransitionInComplete -= QuitAfterTransition;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
    }

}
