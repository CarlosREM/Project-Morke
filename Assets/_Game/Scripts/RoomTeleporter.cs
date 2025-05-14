using System.Collections;
using UnityEngine;

public class RoomTeleporter : MonoBehaviour
{
    public void Teleport()
    {
        GameLoopManager.Instance.PlayerRef.enabled = false;
        
        TransitionManager.OnTransitionInComplete += OnTransitionComplete;
        TransitionManager.TransitionFadeIn();

        void OnTransitionComplete()
        {
            TransitionManager.OnTransitionInComplete -= OnTransitionComplete;
            StartCoroutine(TeleportAfterTransition());
        }
    }

    private IEnumerator TeleportAfterTransition()
    {
        yield return null;
        
        // lock camera so it goes with us
        GameLoopManager.Instance.CamRef.SetDamping(Vector3.zero);

        // teleport to position
        GameLoopManager.Instance.PlayerRef.transform.position = transform.position;

        yield return new WaitForSeconds(0.5f);
        
        GameLoopManager.Instance.CamRef.ResetFocus();
        
        TransitionManager.TransitionFadeOut();
        TransitionManager.OnTransitionOutComplete += OnTransitionComplete;
        
        void OnTransitionComplete()
        {
            // return control to player until it finishes the fade out
            TransitionManager.OnTransitionOutComplete -= OnTransitionComplete;
            GameLoopManager.Instance.PlayerRef.enabled = true;
        }
    }
}
