using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
    {
    }

    private Editor editor;

    public InspectorView()
    {
    }

    public void UpdateSelection(ZDialogueGraph graph, Editor _editor)
    {
        Clear();
        var root = new ScrollView();
        Add(root);

        Object.DestroyImmediate(editor);
        editor = _editor;
        VisualElement uiElementsEditor = editor.CreateInspectorGUI();
        if (uiElementsEditor != null) root.Add(uiElementsEditor);
        else root.Add(new IMGUIContainer(() => editor.OnInspectorGUI())
                { style = { borderBottomWidth = 2, borderBottomColor = new Color(.25f, .25f, .25f) } });
        root.Add(new Button(() =>
        {
            if (EditorWindow.HasOpenInstances<ZDialogueGraphEditorWindow>())
            {
                var graphView = EditorWindow.GetWindow<ZDialogueGraphEditorWindow>().graphView;
                graphView.PopulateView(graphView.graph);
            }
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();   
        }){text = "Update Graph"});
    }
}