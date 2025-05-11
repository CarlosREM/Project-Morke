using UnityEngine;
using UnityEngine.Tilemaps;

public class StairsOccluder : MonoBehaviour
{
    [SerializeField] private TilemapRenderer stairsStepsRenderer;
    [SerializeField] private SpriteRenderer[] stairsRailRenderer;
    
    public void ToggleStairsLayer(bool isOn)
    {
        stairsStepsRenderer.sortingLayerName = (isOn) ? "Foreground" : "Background";
        
        foreach (var railSprite in stairsRailRenderer)
        {
            railSprite.sortingLayerName = (isOn) ? "Foreground" : "Background";
        }
    }
}
