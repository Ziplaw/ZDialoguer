using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LINQtoCSV;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.PlayerLoop;
using ZDialoguer.Localization;
using ZGraph;
using ZGraph.DialogueSystem;
using ZDialoguerEditor;

namespace ZDialoguer.Localization.Editor
{
    public class LocalisedStringPropertyDrawer : PropertyDrawer
    {
        internal ZNodeView ZNodeView;
        internal int indexPosition = 0;
        internal bool oneLine;
        internal bool stretch;
        private bool editingText;
        public VisualElement _container;
        public int _containerPosition;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var self = GetTextFromNode(property.serializedObject.targetObject as ZNode, indexPosition);

            self.table = null;

            var root = new VisualElement { name = "localisedStringContainer" };
            root.style.marginBottom = oneLine ? 0 : 5;
            root.style.marginLeft = oneLine ? 0 : 5;
            root.style.marginRight = oneLine ? 0 : 5;
            root.style.marginTop = oneLine ? 0 : 5;

            var rowContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row, alignItems = Align.Center /*, maxHeight = oneLine?0:1000*/
                }
            };

            var currentText =
                new HelpBox(self,
                    HelpBoxMessageType.None)
                {
                    style =
                    {
                        flexGrow = 1, maxHeight = 150,
                        maxWidth = oneLine ? 70 : stretch ? new StyleLength(StyleKeyword.None) : 150,
                        minHeight = oneLine ? 24 : 56, minWidth = 70
                    }
                };
            var label = currentText.Q<Label>();
            label.style.overflow = Overflow.Hidden;
            label.style.whiteSpace = oneLine ? WhiteSpace.NoWrap : WhiteSpace.Normal;

            var textField = new TextField
            {
                value = self,
                multiline = true,
                style =
                {
                    flexGrow = 1, maxHeight = 150,
                    maxWidth = oneLine ? 70 : stretch ? new StyleLength(StyleKeyword.None) : 150, 
                    minHeight = oneLine ? 24 : 56,
                    minWidth = 70,
                    whiteSpace = oneLine ? WhiteSpace.NoWrap : WhiteSpace.Normal,
                }
            };

            //overflow = Overflow.Hidden, whiteSpace = oneLine ? WhiteSpace.NoWrap : WhiteSpace.Normal}

            //align items center max height 0

            VisualElement columnContainer = new VisualElement { style = { alignSelf = Align.FlexEnd, flexDirection = oneLine ? FlexDirection.Row : FlexDirection.Column} };

            var searchIcon = Resources.Load<Texture2D>("Icons/search");
            var editIcon = Resources.Load<Texture2D>(editingText ? "Icons/select" : "Icons/edit");
            

            var searchButton = new Button(() => SearchButton(property, currentText))
                { style = { alignSelf = Align.FlexEnd, flexWrap = Wrap.Wrap, flexGrow = 0, height = 24, width = 24 } };
            searchButton.style.backgroundImage = searchIcon;

            var editButton = new Button(() => EditButton(property, self, textField))
                { style = { alignSelf = Align.FlexEnd, flexWrap = Wrap.Wrap, flexGrow = 0, height = 24, width = 24 } };
            editButton.style.backgroundImage = editIcon;

            columnContainer.Add(editButton);
            columnContainer.Add(searchButton);

            
            if (editingText)
                rowContainer.Add(textField);
            else
                rowContainer.Add(currentText);

            rowContainer.Add(columnContainer);
            root.Add(rowContainer);
            return root;
        }

        void SearchButton(SerializedProperty property, HelpBox currentText)
        {
            LocalizationSearchWindow.Open(property, currentText, indexPosition);
        }

        void EditButton(SerializedProperty property, LocalisedString localisedString, TextField textField)
        {
            editingText = !editingText;

            if (!editingText)
            {
                localisedString.table[localisedString.value].entry[LocalizationSettings.Instance.selectedLanguage] =
                    textField.value;
                LocalizationSystem.SetTable(localisedString.csvFileFullAssetPath, localisedString.table);
            }

            _container.Q("localisedStringContainer")?.RemoveFromHierarchy();
            
            _container.Insert(_containerPosition,CreatePropertyGUI(property) );
        }

        internal static LocalisedString GetTextFromNode(ZNode zNode, int indexPosition)
        {
            switch (zNode)
            {
                case DialogueNodeObject dialogueNodeObject: return dialogueNodeObject.text;
                case ChoiceDialogueNodeObject choiceNodeObject:
                    return indexPosition == -1
                        ? choiceNodeObject.dialogueText
                        : choiceNodeObject.choices[indexPosition].choiceText;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class LocalizationSearchWindow : EditorWindow
    {
        private static SerializedProperty _property;
        private static HelpBox _localisedTextBox;
        int indexToSelect;
        private static int choiceNodeIndexPos;
        private bool hasSelected;

        public static void Open(SerializedProperty property, HelpBox localisedTextBox, int indexPosition)
        {
            LocalizationSearchWindow window = CreateInstance<LocalizationSearchWindow>();
            window.titleContent = new GUIContent("Localisation Search");
            choiceNodeIndexPos = indexPosition;
            _property = property;
            _localisedTextBox = localisedTextBox;
            Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Rect r = new Rect(mouse.x - 450, mouse.y + 10, 10, 10);
            window.ShowAsDropDown(r, new Vector2(500, 300));
        }


        private void CreateGUI()
        {
            rootVisualElement.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            rootVisualElement.style.marginBottom = 5;
            rootVisualElement.style.marginRight = 5;
            rootVisualElement.style.marginLeft = 5;
            rootVisualElement.style.marginTop = 5;

            LocalisedString text =
                LocalisedStringPropertyDrawer.GetTextFromNode(_property.serializedObject.targetObject as ZNode,
                    choiceNodeIndexPos);

            // var node = _property.serializedObject.targetObject as DialogueNodeObject;
            // var text = node.text;
            var table = LocalizationSystem.GetTable(text.csvFileFullAssetPath);
            VisualElement root = new ScrollView
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 5,
                    marginRight = 5,
                    marginLeft = 5,
                    marginTop = 5
                }
            };
            var searchField = new ToolbarSearchField
                { contentContainer = { style = { maxWidth = 1000 } }, style = { maxWidth = 1000 } };
            rootVisualElement.Add(searchField);
            Texture2D selectIcon = Resources.Load<Texture2D>("Icons/select");

            root.Add(new IMGUIContainer(() =>
            {
                foreach (var tableEntry in table.Where(e =>
                    e.entry.Any(s => s.ToLower().Contains(searchField.value.ToLower()))))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        string label = Array.IndexOf(table.ToArray(), tableEntry) == text.value
                            ? $"<color=#94F5AD>{tableEntry.entry[LocalizationSettings.Instance.selectedLanguage]}</color>"
                            : tableEntry.entry[LocalizationSettings.Instance.selectedLanguage];
                        int height = (int)(24 *
                                           (1 + tableEntry.entry[LocalizationSettings.Instance.selectedLanguage]
                                               .Length / 144f));
                        GUILayout.Label(tableEntry.entry[0], new GUIStyle("helpbox") { richText = true },
                            GUILayout.Width(75), GUILayout.MinHeight(height));
                        GUILayout.Label(label,
                            new GUIStyle("helpbox") { richText = true, wordWrap = true, stretchHeight = true },
                            GUILayout.MinHeight(24), GUILayout.MaxHeight(height));
                        if (GUILayout.Button(selectIcon, GUILayout.Width(24), GUILayout.MinHeight(height)))
                        {
                            SelectValue(tableEntry, table, text, _property.serializedObject.targetObject as ZNode);
                        }
                    }
                }
            }));


            rootVisualElement.style.borderBottomColor = new Color(.35f, .35f, .35f);
            rootVisualElement.style.borderBottomWidth = 1;
            rootVisualElement.style.borderTopColor = new Color(.35f, .35f, .35f);
            rootVisualElement.style.borderTopWidth = 1;
            rootVisualElement.style.borderLeftColor = new Color(.35f, .35f, .35f);
            rootVisualElement.style.borderLeftWidth = 1;
            rootVisualElement.style.borderRightColor = new Color(.35f, .35f, .35f);
            rootVisualElement.style.borderRightWidth = 1;
            rootVisualElement.Add(root);
        }

        void SelectValue(LocalizationSystem.TableEntry entry, List<LocalizationSystem.TableEntry> table,
            LocalisedString text, ZNode zNode)
        {
            text.Reset();
            text.value = Array.IndexOf(table.ToArray(), entry);
            _localisedTextBox.text = text;
            EditorUtility.SetDirty(zNode);
            AssetDatabase.SaveAssets();
            Close();
        }
    }
}