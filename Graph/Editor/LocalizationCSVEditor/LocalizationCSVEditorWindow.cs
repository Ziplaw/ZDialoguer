using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class LocalizationCSVEditorWindow : EditorWindow
{
    [MenuItem("Tools/ZDialoguer/LocalizationCSVEditorWindow")]
    public static void ShowExample()
    {
        LocalizationCSVEditorWindow wnd = GetWindow<LocalizationCSVEditorWindow>();
        wnd.titleContent = new GUIContent("LocalizationCSVEditorWindow");
    }

    private bool editMode;
    

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/com.Ziplaw.ZDialoguer/Graph/Editor/LocalizationCSVEditor/LocalizationCSVEditorWindow.uxml");
        VisualElement staticVisualElement = visualTree.Instantiate();
        root.Add(staticVisualElement);
        var container = root.Q<ScrollView>();
        root.Q<ObjectField>().RegisterValueChangedCallback(e => GenerateTableMenu(e, container));

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.

        // root.Add(labelWithStyle);
    }

    private void GenerateTableMenu(ChangeEvent<Object> evt, VisualElement container)
    {
        container.Clear();
        if (evt.newValue)
        {
            var table = LocalizationSystem.GetTable(evt.newValue as TextAsset).ToList();
            table.InsertRange(0,
                new List<LocalizationSystem.TableEntry>()
                {
                    new LocalizationSystem.TableEntry() { entry = LocalizationSettings.Instance.languages.ToArray() }
                });
            foreach (var tableEntry in table)
            {
                VisualElement rowContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/com.Ziplaw.ZDialoguer/Graph/Editor/LocalizationCSVEditor/SelectorButton.uxml");

                foreach (var entry in tableEntry.entry)
                {
                    var element = tree.Instantiate();
                    element.style.flexGrow = 1;
                    element.style.width =
                        new StyleLength(new Length(100f / LocalizationSettings.Instance.languages.Count, LengthUnit.Percent));
                    var label = element.Q<Label>();
                    label.text = entry;
                    element.Q<Button>().clicked += () => EditButton(element, table.IndexOf(tableEntry), tableEntry.entry.ToList().IndexOf(entry));
                    rowContainer.Add(element);
                }

                container.Add(rowContainer);
            }
        }
    }

    private void EditButton(VisualElement element, int tableIndex, int entryIndex)
    {
        var label = element.Q<Label>();
        label.RemoveFromHierarchy();
        var textField = new TextField()
        {
            multiline = true,
            style =
            {
                // width = new StyleLength(new Length(98, LengthUnit.Percent)),
                unityTextAlign = TextAnchor.UpperLeft,
                flexDirection = FlexDirection.Row,
                flexGrow = 1,
                whiteSpace = WhiteSpace.Normal,
                alignSelf = Align.FlexStart,
                alignContent = Align.FlexStart,
                alignItems = Align.FlexStart,
            },
            value = label.text
        };
        textField.SelectAll();
        var textElement = textField.Q<VisualElement>("unity-text-input");
        textElement.style.unityTextAlign = TextAnchor.UpperLeft;
        textElement.style.whiteSpace = WhiteSpace.Normal;
        var color = new Color(40 / 255f, 40 / 255f, 40 / 255f);
        element.Q<Button>().Add(textField);
        var submit = new Button((() => SubmitEntryAt(tableIndex, entryIndex)))
        {
            style =
            {
                width = new StyleLength(new Length(98, LengthUnit.Percent)), backgroundColor = color,
                unityBackgroundScaleMode = ScaleMode.ScaleToFit,
                backgroundImage = Resources.Load<Texture2D>("Icons/select")
            }
        };
        element.Q<Button>().Add(submit);
    }

    private void SubmitEntryAt(int tableIndex, int entryIndex)
    {
        throw new System.NotImplementedException();
    }
}