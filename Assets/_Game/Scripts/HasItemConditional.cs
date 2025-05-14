using UnityEngine;

public class HasItemConditional : Conditional
{
    [SerializeField] private string itemName;
    
    public override bool TestCondition()
    {
        return LevelManager.Current.HasItemInInventory(itemName);
    }
}
