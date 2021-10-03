using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using ZDialoguer;

namespace ZDialoguerEditor
{
    public class ZDialogueGraphEditorWindow : EditorWindow
    {
        internal ZDialoguerGraphView graphView;
        private InspectorView inspectorView;

        
        [MenuItem("Tools/ZDialoguer/Graph")]
        public static void OpenWindow()
        {
            ZDialogueGraphEditorWindow wnd = GetWindow<ZDialogueGraphEditorWindow>();
            wnd.titleContent = new GUIContent("ZDialogueGraphEditorWindow");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/com.Ziplaw.ZDialoguer/Scripts/Editor/ZDialogueGraphEditorWindow.uxml");
            visualTree.CloneTree(root);
            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/com.Ziplaw.ZDialoguer/Scripts/Editor/ZDialogueGraphEditorWindow.uss");
            root.styleSheets.Add(styleSheet);
            graphView = root.Q<ZDialoguerGraphView>();
            inspectorView = root.Q<InspectorView>();
            
            graphView.OnNodeSelected = OnNodeSelectionChanged;
            graphView.OnBlackboardFactSelected = OnFactSelectionChanged;
            OnSelectionChange();

        }

        private void OnSelectionChange()
        {
            var graph = Selection.activeObject as ZDialogueGraph;
            if (graph)
            {
                graphView.PopulateView(graph);
            }
        }

        void OnNodeSelectionChanged(NodeView nodeView)
        {
            inspectorView.UpdateSelection(Editor.CreateEditor(nodeView.NodeObject));
        }
        
        void OnFactSelectionChanged(Fact fact)
        {
            inspectorView.UpdateSelection(Editor.CreateEditor(fact));
        }
    }
}