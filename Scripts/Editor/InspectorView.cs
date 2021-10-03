using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView,VisualElement.UxmlTraits> {}

    private Editor editor;
    
    public InspectorView()
    {
    }

    public void UpdateSelection(Editor _editor)
    {
        Clear();

        Object.DestroyImmediate(editor);
        editor = _editor;
        IMGUIContainer container = new IMGUIContainer(() => editor.OnInspectorGUI());
        Add(container);
    }
}
