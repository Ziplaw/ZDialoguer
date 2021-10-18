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

                for (var i = 0; i < tableEntry.entry.Length; i++)
                {
                    int j = i;
                    var entry = tableEntry.entry[i];
                    // var editButtonTemplateContainer = tree.Instantiate();
                    var textField = new TextField
                    {
                        multiline = true,

                        style =
                        {
                            flexGrow = 1,
                            minHeight = 24,
                            width = 0,
                            whiteSpace = WhiteSpace.Normal,
                        }
                    };
                    textField.Q("unity-text-input").style.unityTextAlign = TextAnchor.UpperLeft;
                    textField.RegisterValueChangedCallback(e => { tableEntry.entry[j] = e.newValue; });
                    textField.RegisterCallback<BlurEvent>(e =>
                    {
                        var _table = new List<LocalizationSystem.TableEntry>(table);
                        _table.RemoveAt(0);
                        LocalizationSystem.SetTable(csvFileAssetPath, _table);
                    });
                    textField.RegisterCallback<KeyDownEvent>(e =>
                    {
                        if (e.keyCode == KeyCode.RightArrow)
                        {
                            if (j != LocalizationSettings.Instance.languages.Count - 1)
                                rowContainer.ElementAt(j + 1).Focus();
                        }

                        if (e.keyCode == KeyCode.LeftArrow)
                        {
                            if (j != 0)
                                rowContainer.ElementAt(j - 1).Focus();
                        }

                        if (e.keyCode == KeyCode.UpArrow)
                        {
                            if (tableEntryIndex != 1)
                                container.Q($"row{tableEntryIndex - 1}")?.ElementAt(j).Focus();
                        }

                        if (e.keyCode == KeyCode.DownArrow)
                        {
                            if (tableEntryIndex != table.Count-1)
                                container.Q($"row{tableEntryIndex + 1}")?.ElementAt(j).Focus();
                        }
                    });
                    textField.SetValueWithoutNotify(entry);
                    rowContainer.Add(textField);
                }

                if (tableEntryIndex == 0)
                {
                    rowContainer.Query<TextField>().ToList().ForEach(t =>
                    {
                        t.Q("unity-text-input").style.unityTextAlign = TextAnchor.MiddleCenter;
                        t.SetEnabled(false);
                    });

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
                        table = LocalizationSystem.GetTable(csvFileAssetPath);
                        table.RemoveAt(tableEntryIndex - 1);
                        LocalizationSystem.SetTable(csvFileAssetPath, table);
                        GenerateTableMenu(csvFileAssetPath, container);
                    }) { text = "-" });
                }

                container.Add(rowContainer);
            }

            if (rootVisualElement.Q("addEntryButton") == null)
            {
                rootVisualElement.Add(new Button(() =>
                {
                    var localTable = LocalizationSystem.GetTable(csvFileAssetPath);
                    localTable.Add(new LocalizationSystem.TableEntry
                        { entry = LocalizationSettings.Instance.languages.Select(s => "").ToArray() });
                    LocalizationSystem.SetTable(csvFileAssetPath, localTable);
                    GenerateTableMenu(csvFileAssetPath, container);
                }) { name = "addEntryButton", text = "+", style = { fontSize = 24, height = 24 } });

                // rootVisualElement.Add(new Button(() =>
                //     {
                //         // SubmitEntryAt();
                //     })
                //     { text = "Save & Apply", name = "saveAndApplyButton", style = { fontSize = 16, height = 24 } });
            }
        }
        else
        {
            rootVisualElement.Add(generateButton);
        }
    }

    private List<LocalizationSystem.TableEntry> AddEntry(List<LocalizationSystem.TableEntry> table)
    {
        return table;
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