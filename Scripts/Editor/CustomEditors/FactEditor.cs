using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Instrumentation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

[CustomEditor(typeof(Fact))]
public class FactEditor : Editor
{
    private Fact manager;
    private VisualElement root;

    private void OnEnable()
    {
        root = new VisualElement();

        switch (target)
        {
            case Fact fact:
                manager = fact;
                break;
            case FactNodeObject factNodeObject:
                manager = factNodeObject.fact;
                break;
            default:
                return;
        }
    }

    public override VisualElement CreateInspectorGUI()
    {
        root.style.backgroundColor = new Color(40 / 255f, 40 / 255f, 40 / 255f);


        root.Add(InspectorView.GetNodeLabel("Fact", new Color(1f, 0.65f, 0f)));
        var enumField = new EnumField("Fact Type", manager.factType);
        enumField.RegisterValueChangedCallback(e =>
        {
            manager.factType = (Fact.FactType)e.newValue;
            ZDialogueGraphEditorWindow.TryRepopulate();
            root.Clear();
            CreateInspectorGUI();
        });
        root.Add(enumField);

        root.Add(new IMGUIContainer(() =>
        {
            switch (manager.factType)
            {
                case Fact.FactType.Float:
                    manager.Value = EditorGUILayout.FloatField("value", (float)manager.Value);
                    break;
                case Fact.FactType.String:
                    manager.Value = EditorGUILayout.TextField("value", (string)manager.Value);
                    break;
            }
        }));

        return root;
    }

    // public override void OnInspectorGUI()
    // {
    //     GUILayout.Label(manager.nameID);
    //     
    //     serializedObject.Update();
    //
    //     EditorGUILayout.PropertyField(serializedObject.FindProperty("factType"));
    //
    //     serializedObject.ApplyModifiedProperties();
    //
    //     using (var change = new EditorGUI.ChangeCheckScope())
    //     {
    //         switch (manager.factType)
    //         {
    //             case Fact.FactType.Float:
    //                 manager.Value = EditorGUILayout.FloatField("value", (float)manager.Value);
    //                 break;
    //             case Fact.FactType.String:
    //                 manager.Value = EditorGUILayout.TextField("value", (string)manager.Value);
    //                 break;
    //         }
    //
    //         if (change.changed)
    //         {
    //             EditorUtility.SetDirty(manager);
    //             AssetDatabase.SaveAssets();       
    //         }
    //     }
    //    
    // }
}