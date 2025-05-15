using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public abstract class CustomMarker : Marker, INotification
{
    public PropertyName id => new();
    
    [Space(10)] 
    [SerializeField] private bool retroactive;
    [SerializeField] private bool emitOnce;

    public NotificationFlags flags =>
        (retroactive ? NotificationFlags.Retroactive : default) |
        (emitOnce ? NotificationFlags.TriggerOnce : default);
}


public class PlayerDialogueMarker : CustomMarker
{
    [field: Space(10)] 
    [field: SerializeField, TextArea] public string dialogueMessage { get; private set; }
    [field: SerializeField] public float dialogueDuration { get; private set; } = 2.5f;
}


public class PlayerControlMarker : CustomMarker
{
    [field: Space(10)] 
    [field: SerializeField] public bool newControlStatus { get; private set; }

    [field: SerializeField] public bool playerActive { get; private set; } = true;
}
