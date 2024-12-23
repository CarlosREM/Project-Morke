using UnityEngine;

public class TestInteract : Interactable
{
    public override bool Interact(GameObject source)
    {
        Debug.Log($"Interacted with {name}!");
        return true;
    }
}
