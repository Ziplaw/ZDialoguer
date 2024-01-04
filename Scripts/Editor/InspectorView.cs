using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguerEditor;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
    {
    }

    private Editor editor;

    public void UpdateSelection(ZGraph.Graph graph, Editor _editor)
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

    public static VisualElement GetNodeLabel(string labelText, Color color)
    {
        VisualElement labelContainer = new VisualElement
        {
            style =
            {
                backgroundColor = color, height = 40, borderTopLeftRadius = 10,
                borderTopRightRadius = 10, borderBottomLeftRadius = 10, borderBottomRightRadius = 10
            }
        };

        var font = Resources.Load<Font>("Fonts/FugazOne");
        
        var label = new Label()
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleCenter, fontSize = 30,
                unityFont = font,
                unityFontDefinition = new StyleFontDefinition(font)
            }
        };

    #if UNITY_2021_1_OR_NEWER
        label.text = $"<color=black>{labelText}</color>";
        label.enableRichText = true;

    #elif UNITY_2020_3_OR_NEWER
        label.text = labelText;
    #endif

        labelContainer.Add(label);
        return labelContainer;
    }
}