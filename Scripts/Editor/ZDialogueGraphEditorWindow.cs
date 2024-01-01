using System;
using System.Linq;
using Codice.Client.ChangeTrackerService.Serialization;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using ZGraph.DialogueSystem;
using ZGraph;

namespace ZDialoguerEditor
{
    public class MyAssetHandler
    {
        [OnOpenAsset(OnOpenAssetAttributeMode.Execute)]
        public static bool OpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is ZDialogueGraph graph)
            {
                Selection.activeObject = obj;
                ZDialogueGraphEditorWindow.OpenWindow(graph);
                return true;
            }

            return false;
        }
    }

    public class ZDialogueGraphEditorWindow : EditorWindow
    {
        public ZDialogueGraph graph;
        public ZGraphView graphView;
        private InspectorView inspectorView;

        public static void OpenWindow(ZDialogueGraph graph)
        {
            ZDialogueGraphEditorWindow wnd = GetWindow<ZDialogueGraphEditorWindow>();
            wnd.titleContent = new GUIContent(graph.GraphTypeName);
            wnd.graph = graph;
            wnd.graphView.PopulateView(wnd.graph);
            var resolution = Screen.currentResolution;


            if (float.IsNaN(wnd.position.height) ||
                float.IsNaN(wnd.position.width) ||
                float.IsNaN(wnd.position.x) ||
                float.IsNaN(wnd.position.y))
            {
                wnd.position = new Rect(
                    new Vector2(resolution.width * .5f - 1440 * .5f, resolution.height * .5f - 512 * .5f), //
                    new Vector2(1440, 512));
            }
            // wnd.Close();
        }

        public static void TryRepopulate()
        {
            if (HasOpenInstances<ZDialogueGraphEditorWindow>())
            {
                var wnd = GetWindow<ZDialogueGraphEditorWindow>().graphView;
                wnd.PopulateView(wnd.graph);
            }
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("UXML/ZDialogueGraphEditorWindow");
            visualTree.CloneTree(root);
            graphView = root.Q<ZGraphView>();
            graphView._editorWindow = this;
            inspectorView = root.Q<InspectorView>();
            PopupField<string> languagePopup = new PopupField<string>(LocalizationSettings.Instance.languages,
                LocalizationSettings.Instance.selectedLanguage) { name = "LanguagePopup" };

            LocalizationSettings.Instance.OnLanguageChange += s => languagePopup.SetValueWithoutNotify(s); //

            languagePopup.RegisterValueChangedCallback(e =>
            {
                LocalizationSettings.Instance.selectedLanguage =
                    LocalizationSettings.Instance.languages.IndexOf(e.newValue);

                TryRepopulate();

                EditorUtility.SetDirty(LocalizationSettings.Instance);
                AssetDatabase.SaveAssets();
            });
            root.Q<VisualElement>("LanguageSelectElement").Add(languagePopup);

            graphView.OnNodeSelected = OnNodeSelectionChanged;
            graphView.OnNodeDeselected = OnNodeDeselection;

            AssetDeleter.window = this;

            if (graph)
                graphView.PopulateView(graph);
        }

        void OnNodeSelectionChanged(ZNodeView zNodeView)
        {
            inspectorView.UpdateSelection(graphView.graph, Editor.CreateEditor(zNodeView.Node));
            Selection.activeObject = zNodeView.Node;
        }
        
        void OnNodeDeselection(ZNodeView zNodeView)
        {
            inspectorView.UpdateSelection(graphView.graph, Editor.CreateEditor(zNodeView.Node));
            Selection.activeObject = graph;
        }

        void OnFactSelectionChanged(Fact fact)
        {
            // inspectorView.UpdateSelection(graphView.graph, Editor.CreateEditor(fact));
        }
    }

    public class AssetDeleter : AssetModificationProcessor
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