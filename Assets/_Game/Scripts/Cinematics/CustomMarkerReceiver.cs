using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class CustomMarkerReceiver : MonoBehaviour, INotificationReceiver
{
    [SerializeField] private UnityEvent<string, float> onPlayerDialogueNotification;
    [SerializeField] private UnityEvent<bool, bool> onPlayerControlNotification;
    [SerializeField] private UnityEvent onBaseNotification;
    
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is PlayerDialogueMarker playerDialogueNotif)
        {
            onPlayerDialogueNotification?.Invoke(playerDialogueNotif.dialogueMessage, playerDialogueNotif.dialogueDuration);
        }
        else if (notification is PlayerControlMarker playerControlNotif)
        {
            onPlayerControlNotification?.Invoke(playerControlNotif.newControlStatus, playerControlNotif.playerActive);
        }
        else
        {
            onBaseNotification?.Invoke();
        }
    }
}
