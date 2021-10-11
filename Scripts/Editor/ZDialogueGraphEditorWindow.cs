using System;
using System.Linq;
using Codice.CM.SEIDInfo;
using PlasticPipe.PlasticProtocol.Client.Proxies;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using ZDialoguer;
using AssetModificationProcessor = UnityEditor.AssetModificationProcessor;

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
            wnd.titleContent = new GUIContent("Dialogue Graph");
            var resolution = Screen.currentResolution;
            Debug.Log(wnd.position);

            if (float.IsNaN(wnd.position.height) ||
                float.IsNaN(wnd.position.width) ||
                float.IsNaN(wnd.position.x) ||
                float.IsNaN(wnd.position.y))
            {
                wnd.position = new Rect(new Vector2(resolution.width * .5f - 1440 * .5f, resolution.height * .5f - 512 * .5f),
                    new Vector2(1440, 512));
            }
        }

        public void CreateGUI()
        {
            // if (float.IsNaN(rootVisualElement.layout.width))
            // {
            //     var evt = GeometryChangedEvent.GetPooled(rootVisualElement.layout,
            //         new Rect(new Vector2(rootVisualElement.layout.x, rootVisualElement.layout.y),
            //             new Vector2(500, 300)));
            //     rootVisualElement.schedule.Execute(() => rootVisualElement.SendEvent(evt));
            //     Debug.Log(rootVisualElement.layout);
            //     evt.Dispose();
            // }

            // Close();
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
            graphView._editorWindow = this;
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

            AssetDeleter.window = this;

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
            inspectorView.UpdateSelection(graphView.graph, Editor.CreateEditor(nodeView.NodeObject));
        }
        
        void OnFactSelectionChanged(Fact fact)
        {
            inspectorView.UpdateSelection(graphView.graph, Editor.CreateEditor(fact));
        }
    }
    
    public class AssetDeleter : UnityEditor.AssetModificationProcessor
    {
        public static ZDialogueGraphEditorWindow window;
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
        {
            if (window)
            {
                if (AssetDatabase.GetAssetPath(window.graphView.graph) == path)
                {
                    window.Close();
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}