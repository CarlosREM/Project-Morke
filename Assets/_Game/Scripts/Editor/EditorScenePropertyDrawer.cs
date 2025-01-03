using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EditorSceneAttribute))]
public class EditorScenePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.BeginProperty(position, label, property);
            var attrib = this.attribute as EditorSceneAttribute;
            int propertyIndex = property.intValue;

            List<string> sceneNameList = new List<string>();

            int sceneIndex = 0;
            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                if (!editorScene.enabled)
                    continue;
                
                var scenePath = editorScene.path;
                var scenePathSplit = scenePath.Split('/');
                var sceneName = scenePathSplit[^1].Split('.')[0];

                if (sceneName.Contains('_'))
                    sceneName = sceneName.TrimStart('_');
                
                sceneNameList.Add($"[{sceneIndex}] {sceneName}");
                sceneIndex++;
            }
            
            int index = -1;
            if (propertyIndex >= 0)
            {
                index = propertyIndex;
            }
            
            //Draw the popup box with the current selected index
            index = EditorGUI.Popup(position, label.text, index, sceneNameList.ToArray());
 
            //Adjust the actual string value of the property based on the selection
            if (index >= 0)
            {
                property.intValue = index;
            }
            else
            {
                property.intValue = -1;
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
