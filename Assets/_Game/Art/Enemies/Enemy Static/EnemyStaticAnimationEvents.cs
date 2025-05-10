using UnityEngine;

public class EnemyStaticAnimationEvents : MonoBehaviour
{
    [SerializeField] private EnemyStaticBehavior enemyControl;
    
    public void Dead()
    {
        enemyControl.enabled = false;
    }
}
