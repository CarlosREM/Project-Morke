using UnityEngine;
using UnityEngine.Timeline;

[TrackClipType(typeof(CustomMarkerReceiver))]
[TrackBindingType(typeof(CustomMarkerReceiver))]
[TrackColor(255f, 255f, 255f)]
public class CustomMarkerTrack : PlayableTrack
{
}
