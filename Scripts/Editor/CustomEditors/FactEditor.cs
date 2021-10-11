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

        manager.OnFactTypeChange += FactChange;
    }

    private void OnDisable()
    {
        if (manager) manager.OnFactTypeChange -= FactChange;
    }

    public void FactChange(Fact.FactType newFactType)
    {
        manager.factType = newFactType;
    }

    public override VisualElement CreateInspectorGUI()
    {
        root.style.backgroundColor = new Color(40 / 255f, 40 / 255f, 40 / 255f);

        VisualElement labelContainer = new VisualElement
        {
            style =
            {
                backgroundColor = new Color(1f, 0.65f, 0f), height = 40, borderTopLeftRadius = 10,
                borderTopRightRadius = 10, borderBottomLeftRadius = 10, borderBottomRightRadius = 10
            }
        };
        var label = new Label("<color=black>Fact</color>")
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleCenter, fontSize = 30,
                unityFont = Resources.Load<Font>("Fonts/FugazOne")
            }
        };
        label.enableRichText = true;

        labelContainer.Add(label);

        root.Add(labelContainer);
        var enumField = new EnumField("Fact Type", manager.factType);
        enumField.RegisterValueChangedCallback((e) =>
        {
            manager.FactTypeChange(e);
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