using UnityEditor;
using UnityEngine;

public class MorkelEditorMenus
{
    [MenuItem("GameObject/Morkel (Editor)/Create Prefab/Level Manager")]
    private static void CreateLevelManager()
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Systems/Level Manager.prefab");
        PrefabUtility.InstantiatePrefab(prefabAsset);
    }
    
    [MenuItem("GameObject/Morkel (Editor)/Create Prefab/Game Loop Manager")]
    private static void CreateGameLoopManager()
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Systems/Game Loop Manager.prefab");
        PrefabUtility.InstantiatePrefab(prefabAsset);
    }
    
    [MenuItem("GameObject/Morkel (Editor)/Create Prefab/Input Manager")]
    private static void CreateInputManager()
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Systems/Input Manager.prefab");
        PrefabUtility.InstantiatePrefab(prefabAsset);
    }
    
    [MenuItem("GameObject/Morkel (Editor)/Create Prefab/Transition Manager")]
    private static void CreateTransitionManager()
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Systems/Transition Manager.prefab");
        PrefabUtility.InstantiatePrefab(prefabAsset);
    }
    
    [MenuItem("GameObject/Morkel (Editor)/Create Prefab/Placeholder Camera")]
    private static void CreatePlaceholderCamera()
    {
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Utils/PlaceholderCamera.prefab");
        PrefabUtility.InstantiatePrefab(prefabAsset);    
    }
}
