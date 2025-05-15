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
            Debug.Log("[CustomMarkerReceiver] Notification received (Player Dialogue)");
            onPlayerDialogueNotification?.Invoke(playerDialogueNotif.dialogueMessage, playerDialogueNotif.dialogueDuration);
        }
        else if (notification is PlayerControlMarker playerControlNotif)
        {
            Debug.Log("[CustomMarkerReceiver] Notification received (Player Control)");
            onPlayerControlNotification?.Invoke(playerControlNotif.newControlStatus, playerControlNotif.playerActive);
        }
        else
        {
            Debug.Log("[CustomMarkerReceiver] Notification received (Default)");
            onBaseNotification?.Invoke();
        }
    }
}
