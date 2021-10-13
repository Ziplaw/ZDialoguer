using System.Linq;
using UnityEditor;
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
            wnd.titleContent = new GUIContent("Dialogue Graph");
            var resolution = Screen.currentResolution;

            if (float.IsNaN(wnd.position.height) ||
                float.IsNaN(wnd.position.width) ||
                float.IsNaN(wnd.position.x) ||
                float.IsNaN(wnd.position.y))
            {
                wnd.position = new Rect(
                    new Vector2(resolution.width * .5f - 1440 * .5f, resolution.height * .5f - 512 * .5f),
                    new Vector2(1440, 512));
            }
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/com.Ziplaw.ZDialoguer/Scripts/Editor/ZDialogueGraphEditorWindow.uxml");
            visualTree.CloneTree(root);
            // _styleSheet =
            //     AssetDatabase.LoadAssetAtPath<StyleSheet>(
            //         "Assets/com.Ziplaw.ZDialoguer/Scripts/Editor/ZDialogueGraphEditorWindow.uss");
            graphView = root.Q<ZDialoguerGraphView>();
            graphView._editorWindow = this;
            inspectorView = root.Q<InspectorView>();
            PopupField<string> languagePopup = new PopupField<string>(LocalizationSettings.Instance.languages,
                LocalizationSettings.Instance.selectedLanguage);
            languagePopup.RegisterValueChangedCallback(e =>
            {
                LocalizationSettings.Instance.selectedLanguage =
                    LocalizationSettings.Instance.languages.IndexOf(e.newValue);

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