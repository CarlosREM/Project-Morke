using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraManager : MonoBehaviour
{
    private struct ComposerParams
    {
        public Vector3 TargetOffset;
        public Vector3 Damping;
    }
    
    private Transform _playerRef, _cursorRef;

    [SerializeField] private CinemachineCamera cameraRef;
    [SerializeField] private CinemachinePositionComposer camComposer;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    private CameraTarget _defaultTarget, _playerTarget;
    
    private ComposerParams _defaultComposerParams;

    public void Start()
    {
        _defaultTarget = new CameraTarget()
        {
            CustomLookAtTarget = false,
            TrackingTarget = targetGroup.Transform,
            LookAtTarget = targetGroup.Transform,
        };
        
        _playerTarget = new CameraTarget()
        {
            CustomLookAtTarget = false,
            TrackingTarget = _playerRef,
            LookAtTarget = _playerRef,
        };

        _defaultComposerParams = new()
        {
            Damping = camComposer.Damping,
            TargetOffset = camComposer.TargetOffset,
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
        
        camComposer.TargetOffset = _defaultComposerParams.TargetOffset;
        camComposer.Damping = _defaultComposerParams.Damping;
    }

    public void FocusPlayer()
    {
        cameraRef.Target = _playerTarget;
        
        SetTargetOffset(Vector3.zero);
    }

    public void SetTarget(Transform target)
    {
        var newTarget = new CameraTarget()
        {
            CustomLookAtTarget = false,
            TrackingTarget = target,
            LookAtTarget = target,
        };
        
        cameraRef.Target = newTarget;
        
        SetTargetOffset(Vector3.zero);
    }
    
    public void SetTargetOffset(Vector3 value)
    {
        camComposer.TargetOffset = value;
    }
    
    public void SetDamping(Vector3 value)
    {
        camComposer.Damping = value;
    }

    public Rect GetCameraBounds()
    {
        Rect output = new();
        output.height = cameraRef.Lens.OrthographicSize * 2;
        output.width = output.height * cameraRef.Lens.Aspect;
        output.center = cameraRef.transform.position;
        return output;
    }
}
