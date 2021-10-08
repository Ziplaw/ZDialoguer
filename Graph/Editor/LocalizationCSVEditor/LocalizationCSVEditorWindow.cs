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
using Object = UnityEngine.Object;


public class LocalizationCSVEditorWindow : EditorWindow
{
    [MenuItem("Tools/ZDialoguer/LocalizationCSVEditorWindow")]
    public static void ShowExample()
    {
        LocalizationCSVEditorWindow wnd = GetWindow<LocalizationCSVEditorWindow>();
        wnd.titleContent = new GUIContent("Localization Editor");
    }

    private bool editMode;
    private TextAsset csvFile;

    private Button generateButton;


    public void CreateGUI()
    {
        rootVisualElement.Clear();
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/com.Ziplaw.ZDialoguer/Graph/Editor/LocalizationCSVEditor/LocalizationCSVEditorWindow.uxml");
        VisualElement staticVisualElement = visualTree.Instantiate();
        root.Add(staticVisualElement);
        var container = root.Q<ScrollView>();
        var assetField = root.Q<ObjectField>();
        assetField.RegisterValueChangedCallback(e => GenerateTableMenu(e.newValue as TextAsset, container));
        generateButton = root.Q<Button>("GenerateLocalizationAsset");
        generateButton.clicked += () => GenerateAndOpenTextAsset(assetField);
    }

    private void GenerateAndOpenTextAsset(ObjectField assetField)
    {
        var path = EditorUtility.SaveFilePanel("Create Localization Table Asset", "", "Table", "csv");
        path = path.Substring(Application.dataPath.Length - 6);
        File.WriteAllText(path,
            String.Join(LocalizationSettings.Instance.separator.ToString(), LocalizationSettings.Instance.languages) +
            "\n");
        AssetDatabase.Refresh();

        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        assetField.SetValueWithoutNotify(textAsset);
        GenerateTableMenu(textAsset, rootVisualElement.Q<ScrollView>());
    }

    private void GenerateTableMenu(TextAsset csvFile, VisualElement container)
    {
        container.Clear();
        if (csvFile)
        {
            generateButton.RemoveFromHierarchy();
            this.csvFile = csvFile;
            var table = LocalizationSystem.GetTable(csvFile).ToList();
            // TemplateContainer lastTemplateContainer = null;
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
                    "Assets/com.Ziplaw.ZDialoguer/Graph/Editor/LocalizationCSVEditor/SelectorButton.uxml");

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

                    if (tableEntryIndex == (table.Count - 1) && j == 0)
                    {
                        editButtonTemplateContainer.name = "editing";
                        // lastTemplateContainer = editButtonTemplateContainer;
                    }

                    // Debug.Log(tableEntryIndex + " " + (table.Count - 1) + " " + j);
                    // Debug.Log(lastTemplateContainer);
                    // Debug.Log(rootVisualElement.Q<TemplateContainer>("editing"));
                    // Debug.Log(rootVisualElement.Q<ScrollView>().Q<VisualElement>("row3"));
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
                        LocalizationSystem.SetTable(csvFile, table.ToArray());
                        GenerateTableMenu(csvFile, container);
                    }) { text = "-" });
                }


                // Debug.Log(rootVisualElement.Q<ScrollView>());
                // Debug.Log(rootVisualElement.Q<ScrollView>().Q<VisualElement>($"row{table.Count-2}").Q<TemplateContainer>());

                container.Add(rowContainer);
            }

            container.Add(new Button(() =>
            {
                table = AddEntry(table);
                // Debug.Log(lastTemplateContainer.Q<Button>());
                
                // lastTemplateContainer.Q<Button>().style.borderBottomColor = Color.red;
                // lastTemplateContainer.Q<Button>().style.borderBottomWidth = 20;
                GenerateTableMenu(csvFile, container);
                var editContainer = rootVisualElement.Q<ScrollView>().Q<VisualElement>("unity-content-viewport")
                    .Q<VisualElement>("unity-content-container").Q<VisualElement>($"row{table.Count - 1}")
                    .Q<TemplateContainer>();
                // Debug.Log(lastTemplateContainer.Q<Button>());
                // lastTemplateContainer.Q<Button>().style.borderBottomColor = Color.red;
                // lastTemplateContainer.Q<Button>().style.borderBottomWidth = 20;
                EditButton(editContainer, table.Count - 1, 0);
                // Debug.Log(typeof(Clickable).GetMethod("Invoke", BindingFlags.NonPublic | BindingFlags.Instance));
                // Debug.Log(container.Q<VisualElement>("row0"));
                // Debug.Log(container.Q<VisualElement>("row0").Q<TemplateContainer>().Q<Button>("0"));
                // Debug.Log(container.Q<VisualElement>("row0").Q<Button>().clickable);
                // typeof(Clickable).GetMethod("Invoke", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(container.Q<VisualElement>("row0").Q<TemplateContainer>().Q<Button>("0").clickable, new object[] {null});
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
        LocalizationSystem.SetTable(csvFile, tableNoLanguages.ToArray());
        return table;
    }

    private void EditButton(VisualElement editButtonTemplateContainer, int tableIndex, int entryIndex)
    {
        var table = LocalizationSystem.GetTable(csvFile);
        var label = editButtonTemplateContainer.Q<Label>();
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
                flexShrink = 1
            },
            value = label.text
        };
        textField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                GenerateTableMenu(csvFile, rootVisualElement.Q<ScrollView>());
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
                    
                    rootVisualElement.schedule.Execute(() => {
                        textField.SetValueWithoutNotify(textField.value + "\n");
                        textElement.Focus();
                        rootVisualElement.schedule.Execute(() =>
                        {
                            Debug.Log(textElement.worldBound + " " + textElement.localBound);
                            var evt = MouseDownEvent.GetPooled(
                                new Vector2(textElement.worldBound.x + textElement.worldBound.width - 2, textElement.worldBound.y + textElement.worldBound.height - 2), 0,
                                1,
                                Vector2.zero);
                            textElement.SendEvent(evt);
                            evt.Dispose();
                        });
                    });
                }
            }
        });
        
        rootVisualElement.schedule.Execute(() => {
            textElement.Focus();
        });
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

    private void SubmitEntryAt(LocalizationSystem.TableEntry[] table, int tableIndex, int entryIndex, string newText)
    {
        Debug.Log(tableIndex + " " + entryIndex);
        try
        {
            table[tableIndex - 1].entry[entryIndex] = newText;
        }
        catch (IndexOutOfRangeException)
        {
        }

        LocalizationSystem.SetTable(csvFile, table);

        rootVisualElement.Q<ScrollView>().Clear();
        GenerateTableMenu(csvFile, rootVisualElement.Q<ScrollView>());
    }
}