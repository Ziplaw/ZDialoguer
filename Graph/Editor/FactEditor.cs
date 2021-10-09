using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Instrumentation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Fact))]
public class FactEditor : Editor
{
    private Fact manager;
    private VisualElement root;

    private void OnEnable()
    {
        manager = target as Fact;
        root = new VisualElement();

        manager.OnFactTypeChange += FactChange;
    }

    private void OnDisable()
    {
        manager.OnFactTypeChange -= FactChange;
    }

    public void FactChange(Fact.FactType newFactType)
    {
        manager.factType = newFactType;
    }

    public override VisualElement CreateInspectorGUI()
    {
        root.Add(new Label(manager.nameID));
        var enumField = new EnumField("Fact Type", Fact.FactType.Float);
        enumField.RegisterValueChangedCallback(manager.FactTypeChange);
        root.Add(enumField);
        
        root.Add(new IMGUIContainer(() =>
        {
            using (var change = new EditorGUI.ChangeCheckScope())
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
            
                if (change.changed)
                {
                    EditorUtility.SetDirty(manager);
                    AssetDatabase.SaveAssets();       
                }
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
