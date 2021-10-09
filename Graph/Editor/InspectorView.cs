using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
    {
    }

    private Editor editor;

    public InspectorView()
    {
    }

    public void UpdateSelection(Editor _editor)
    {
        Clear();

        Object.DestroyImmediate(editor);
        editor = _editor;
        VisualElement uiElementsEditor = editor.CreateInspectorGUI();
        if (uiElementsEditor != null) Add(uiElementsEditor);
        else
            Add(new IMGUIContainer(() => editor.OnInspectorGUI())
                { style = { borderBottomWidth = 2, borderBottomColor = new Color(.25f, .25f, .25f) } });
    }
}