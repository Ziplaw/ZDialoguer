using System;
using System.Linq;
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
            // DestroyImmediate(wnd);
            wnd.titleContent = new GUIContent("ZDialogueGraphEditorWindow");
        }

        public void CreateGUI()
        {
            // Close();
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/com.Ziplaw.ZDialoguer/Graph/Editor/ZDialogueGraphEditorWindow.uxml");
            visualTree.CloneTree(root);
            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/com.Ziplaw.ZDialoguer/Graph/Editor/ZDialogueGraphEditorWindow.uss");
            root.styleSheets.Add(styleSheet);
            graphView = root.Q<ZDialoguerGraphView>();
            inspectorView = root.Q<InspectorView>();
            PopupField<string> languagePopup = new PopupField<string>(LocalizationSettings.Instance.languages, LocalizationSettings.Instance.selectedLanguage);
            languagePopup.RegisterValueChangedCallback(e =>
            {
                LocalizationSettings.Instance.selectedLanguage = LocalizationSettings.Instance.languages.IndexOf(e.newValue);
                
                foreach (var node in graphView.nodes.ToList().Where(n => n is DialogueNodeView))
                {
                    var nodeObject = (node as DialogueNodeView).NodeObject as DialogueNodeObject;
                    nodeObject.text.Reset();
                    (node as DialogueNodeView).Q<HelpBox>().text = nodeObject.text;
                }

                EditorUtility.SetDirty(LocalizationSettings.Instance);
                AssetDatabase.SaveAssets();
            });
            root.Q<VisualElement>("LanguageSelectElement").Add(languagePopup);
            
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