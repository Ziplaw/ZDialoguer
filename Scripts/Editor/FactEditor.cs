using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Fact))]
public class FactEditor : Editor
{
    private Fact manager;

    private void OnEnable()
    {
        manager = target as Fact;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.Label(manager.nameID);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
        serializedObject.ApplyModifiedProperties();
    }
}
