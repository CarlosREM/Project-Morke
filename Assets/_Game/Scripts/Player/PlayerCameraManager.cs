using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraManager : MonoBehaviour
{
    private Transform _playerRef, _cursorRef;

    [SerializeField] private CinemachineCamera cameraRef;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    private CameraTarget _defaultTarget;

    public void Start()
    {
        _defaultTarget = new CameraTarget()
        {
            CustomLookAtTarget = false,
            TrackingTarget = targetGroup.Transform,
            LookAtTarget = targetGroup.Transform,
        };
    }

    public void Initialize(PlayerControl player)
    {
        _playerRef = player.CamTargetPlayer;
        _cursorRef = player.CamTargetCursor;
        
        targetGroup.AddMember(_playerRef, 1f, 0.5f);
        targetGroup.AddMember(_cursorRef, 1f, 0.5f);
    }

    public void ResetFocus()
    {
        cameraRef.Target = _defaultTarget;
    }
}
