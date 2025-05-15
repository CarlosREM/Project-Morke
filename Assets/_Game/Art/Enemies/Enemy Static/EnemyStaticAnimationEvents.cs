using FMODUnity;
using UnityEngine;

public class EnemyStaticAnimationEvents : MonoBehaviour
{
    [SerializeField] private EnemyStaticBehavior enemyControl;
    [SerializeField] private StudioEventEmitter sfxOnFlap;
    
    public void Dead()
    {
        enemyControl.SetActive(false);
    }

    public void BlinkOpen()
    {
        enemyControl.AfterBlink();
    }
    
    public void BlinkClosed()
    {
        enemyControl.OnBlink();
    }

    public void OnFlap()
    {
        enemyControl.OnFlap();
        sfxOnFlap?.Play();
    }
}
