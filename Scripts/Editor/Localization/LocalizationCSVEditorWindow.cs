using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using ZDialoguerEditor;
using Object = UnityEngine.Object;


public class LocalizationCSVEditorWindow : EditorWindow
{
    [MenuItem("Tools/ZDialoguer/LocalizationCSVEditorWindow")]
    public static void Open()
    {
        var window = GetWindow<LocalizationCSVEditorWindow>();
        window.titleContent = new GUIContent("Localization Editor");
        // window.Close();
    }

    private TextAsset csvFile;
    internal string csvFileAssetPath;
    private Button generateButton;

    internal string GetTextAssetFullPath(TextAsset textAsset)
    {
        if (textAsset != null)
        {
            return Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6),
                AssetDatabase.GetAssetPath(textAsset));
        }

        return null;
    }

    public void CreateGUI()
    {
        // Show();
        // Open();
        // Close();
        rootVisualElement.Clear();
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/com.Ziplaw.ZDialoguer/Scripts/Editor/Localization/LocalizationCSVEditorWindow.uxml");
        VisualElement staticVisualElement = visualTree.Instantiate();
        root.Add(staticVisualElement);
        var scrollView = root.Q<ScrollView>();
        
        root.style.marginBottom = 5;
        root.style.marginLeft = 5;
        root.style.marginRight = 5;
        root.style.marginTop = 5;
        root.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
        scrollView.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
        scrollView.style.flexGrow = 1;
        scrollView.style.flexDirection = FlexDirection.Column;
        
        // scrollView.Q("unity-content-viewport").style.flexGrow = 1;
        // scrollView.Q("unity-content-viewport").style.flexDirection = FlexDirection.Column;
        //
        // scrollView.Q("unity-content-container").style.flexGrow = 1;
        // scrollView.Q("unity-content-container").style.flexDirection = FlexDirection.Column;
        
        var assetField = root.Q<ObjectField>();
        assetField.RegisterValueChangedCallback(e =>
        {
            csvFileAssetPath = GetTextAssetFullPath(e.newValue as TextAsset);

            GenerateTableMenu(csvFileAssetPath, scrollView);
        });
        generateButton = root.Q<Button>("GenerateLocalizationAsset");
        generateButton.clicked += () => GenerateAndOpenTextAsset(assetField);
    }

    internal void GenerateAndOpenTextAsset(ObjectField assetField)
    {
        var path = EditorUtility.SaveFilePanel("Create Localization Table Asset", "", "Table", "csv");
        path = path.Substring(Application.dataPath.Length - 6);
        File.WriteAllText(path,
            String.Join(LocalizationSettings.Instance.separator.ToString(), LocalizationSettings.Instance.languages) +
            "\n");
        AssetDatabase.Refresh();

        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        assetField.SetValueWithoutNotify(textAsset);
        GenerateTableMenu(csvFileAssetPath, rootVisualElement.Q<ScrollView>());
    }

    internal void GenerateTableMenu(string csvFilePath, VisualElement container)
    {
        container.Clear();
        if (!string.IsNullOrEmpty(csvFilePath))
        {
            generateButton.RemoveFromHierarchy();
            var table = LocalizationSystem.GetTable(csvFilePath);
            table.InsertRange(0,
                new List<LocalizationSystem.TableEntry>()
                {
                    new LocalizationSystem.TableEntry { entry = LocalizationSettings.Instance.languages.ToArray() }
                });
            foreach (var tableEntry in table)
            {
                int tableEntryIndex = table.IndexOf(tableEntry);
                VisualElement rowContainer = new VisualElement
                    { name = $"row{tableEntryIndex}", style = { flexDirection = FlexDirection.Row } };
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/com.Ziplaw.ZDialoguer/Scripts/Editor/Localization/SelectorButton.uxml");

                for (var i = 0; i < tableEntry.entry.Length; i++)
                {
                    int j = i;
                    var entry = tableEntry.entry[i];
                    var editButtonTemplateContainer = tree.Instantiate();

                    editButtonTemplateContainer.Q<Button>().name = j.ToString();
                    editButtonTemplateContainer.style.flexGrow = 1;
                    editButtonTemplateContainer.style.width =
                        new StyleLength(new Length(100f / LocalizationSettings.Instance.languages.Count,
                            LengthUnit.Percent));
                    editButtonTemplateContainer.style.minHeight = 24;
                    var label = editButtonTemplateContainer.Q<Label>();
                    label.text = entry;
                    editButtonTemplateContainer.Q<Button>().clicked += () => EditButton(editButtonTemplateContainer,
                        tableEntryIndex, j);
                    editButtonTemplateContainer.Q<Button>().style.flexDirection = FlexDirection.Column;
                    editButtonTemplateContainer.Q<Button>().style.alignItems = i == 0 ? Align.Center : Align.FlexStart;
                    rowContainer.Add(editButtonTemplateContainer);

                    // if (tableEntryIndex == (table.Count - 1) && j == 0)
                    // {
                    //     editButtonTemplateContainer.name = "editing";
                    // }
                }

                if (tableEntryIndex == 0)
                {
                    rowContainer.Add(new Button(() =>
                    {
                        Selection.activeObject = LocalizationSettings.Instance;
                        EditorGUIUtility.PingObject(LocalizationSettings.Instance);
                    })
                    {
                        style =
                        {
                            backgroundImage = Resources.Load<Texture2D>("Icons/selectInProject"), height = 20,
                            width = 20
                        }
                    });
                }
                else
                {
                    rowContainer.Add(new Button(() =>
                    {
                        table.RemoveAt(tableEntryIndex);
                        table.RemoveAt(0);
                        LocalizationSystem.SetTable(csvFileAssetPath, table);
                        GenerateTableMenu(csvFileAssetPath, container);
                    }) { text = "-" });
                }

                container.Add(rowContainer);
            }

            container.Add(new Button(() =>
            {
                table = AddEntry(table);
                GenerateTableMenu(csvFileAssetPath, container);
                var editContainer = rootVisualElement.Q<ScrollView>().Q<VisualElement>("unity-content-viewport")
                    .Q<VisualElement>("unity-content-container").Q<VisualElement>($"row{table.Count - 1}")
                    .Q<TemplateContainer>();
                EditButton(editContainer, table.Count - 1, 0);
            }) { text = "+", style = { fontSize = 24, height = 24 } });
        }
        else
        {
            rootVisualElement.Add(generateButton);
        }
    }

    private List<LocalizationSystem.TableEntry> AddEntry(List<LocalizationSystem.TableEntry> table)
    {
        table.Add(new LocalizationSystem.TableEntry
            { entry = LocalizationSettings.Instance.languages.Select(s => "").ToArray() });
        var tableNoLanguages = new List<LocalizationSystem.TableEntry>(table);
        tableNoLanguages.RemoveAt(0);
        LocalizationSystem.SetTable(csvFileAssetPath, tableNoLanguages);
        return table;
    }

    private void EditButton(VisualElement editButtonTemplateContainer, int tableIndex, int entryIndex)
    {
        var table = LocalizationSystem.GetTable(csvFileAssetPath);
        var label = editButtonTemplateContainer.Q<Label>();
        if (label == null) return;
        label.RemoveFromHierarchy();

        var textField = new TextField()
        {
            multiline = true,
            style =
            {
                width = new StyleLength(new Length(98, LengthUnit.Percent)),
                unityTextAlign = TextAnchor.UpperLeft,
                flexDirection = FlexDirection.Row,
                whiteSpace = WhiteSpace.Normal,
                flexShrink = 1,
                flexGrow = 1
            },
            value = label.text
        };
        textField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                GenerateTableMenu(csvFileAssetPath, rootVisualElement.Q<ScrollView>());
            }
        });
        var textElement = textField.Q<VisualElement>("unity-text-input");
        textElement.style.unityTextAlign = TextAnchor.UpperLeft;
        textElement.style.whiteSpace = WhiteSpace.Normal;
        textElement.RegisterCallback<KeyDownEvent>(e =>
        {
            if (e.keyCode == KeyCode.Return)
            {
                if (!e.shiftKey)
                {
                    SubmitEntryAt(table, tableIndex, entryIndex, textField.value);
                }
                else
                {
                    // ((e.target as VisualElement).parent as TextField).SetValueWithoutNotify(((e.target as VisualElement).parent as TextField).value + "\n");

                    rootVisualElement.schedule.Execute(() =>
                    {
                        textField.SetValueWithoutNotify(textField.value + "\n");
                        textElement.Focus();
                        rootVisualElement.schedule.Execute(() =>
                        {
                            var evt = MouseDownEvent.GetPooled(
                                new Vector2(textElement.worldBound.x + textElement.worldBound.width-1,
                                    textElement.worldBound.y + textElement.worldBound.height-1), 0,
                                1,
                                Vector2.zero);
                            textElement.SendEvent(evt);
                            evt.Dispose();
                        });
                    });
                }
            }
        });

        rootVisualElement.schedule.Execute(() => { textElement.Focus(); });
        // textElement.Focus();

        var color = new Color(40 / 255f, 40 / 255f, 40 / 255f);
        editButtonTemplateContainer.Q<Button>().Add(textField);

        var submit = new Button(() => SubmitEntryAt(table, tableIndex, entryIndex, textField.value))
        {
            style =
            {
                height = 24,
                width = new StyleLength(new Length(98, LengthUnit.Percent)), backgroundColor = color,
                unityBackgroundScaleMode = ScaleMode.ScaleToFit,
                backgroundImage = Resources.Load<Texture2D>("Icons/select"),
            }
        };
        editButtonTemplateContainer.Q<Button>().Add(submit);
    }

    private void SubmitEntryAt(List<LocalizationSystem.TableEntry> table, int tableIndex, int entryIndex,
        string newText)
    {
        try
        {
            table[tableIndex - 1].entry[entryIndex] = newText;
        }
        catch (IndexOutOfRangeException)
        {
        }

        LocalizationSystem.SetTable(csvFileAssetPath, table);

        rootVisualElement.Q<ScrollView>().Clear();
        GenerateTableMenu(csvFileAssetPath, rootVisualElement.Q<ScrollView>());
    }

    private void OnDestroy()
    {
        if (HasOpenInstances<ZDialogueGraphEditorWindow>())
        {
            var window = GetWindow<ZDialogueGraphEditorWindow>();
            var view = window.graphView;
            view.PopulateView(view.graph);
        }
    }
}