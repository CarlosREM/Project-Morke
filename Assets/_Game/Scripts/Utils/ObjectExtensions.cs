using UnityEngine;

public static class ObjectExtensions
{
    public static void SetActive(this Component component, bool isActive)
    {
        component.gameObject.SetActive(isActive);
    }

    public static bool IsInRange(this int value, int min, int max)
    {
        return value >= min && value <= max;
    }
}
