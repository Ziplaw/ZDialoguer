using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

[CustomEditor(typeof(FactNodeObject))]
public class FactNodeEditor : Editor
{
    
    private FactNodeObject manager;
    private VisualElement root;

    private void OnEnable()
    {
        manager = target as FactNodeObject;
        root = new VisualElement();
    }

    public override VisualElement CreateInspectorGUI()
    {
        Editor editor = CreateEditor(manager.fact);
        root.Add(editor.CreateInspectorGUI());
        return root;
    }
}
