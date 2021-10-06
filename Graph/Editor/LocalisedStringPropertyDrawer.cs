using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.PlayerLoop;
using ZDialoguer.Localization;

namespace ZDialoguer.Localization.Editor
{
    public class LocalisedStringPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var node = property.serializedObject.targetObject as DialogueNodeObject;
            node.text.output = string.Empty;
            node.text.table = null;
            
            var root = new VisualElement();
            var rowContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            var currentText =
                new HelpBox((property.serializedObject.targetObject as DialogueNodeObject).text,
                    HelpBoxMessageType.None) { style = { flexGrow = 1 ,maxHeight = 150, maxWidth = 150, minHeight = 24} };
            
            var searchButton = new Button(() => SearchButton(property, currentText))
                { style = { alignSelf = Align.FlexEnd, flexWrap = Wrap.Wrap, flexGrow = 0, height = 24, width = 24 } };
            searchButton.style.backgroundImage = Resources.Load<Texture2D>("Icons/search");

            rowContainer.Add(currentText);
            rowContainer.Add(searchButton);
            root.Add(rowContainer);
            return root;
        }

        void SearchButton(SerializedProperty property, HelpBox currentText)
        {
            LocalizationSearchWindow.Open(property, currentText);
        }
    }

    public class LocalizationSearchWindow : EditorWindow
    {
        private static SerializedProperty _property;
        private static HelpBox _localisedTextBox;
        int indexToSelect;
        private bool hasSelected;

        public static void Open(SerializedProperty property, HelpBox localisedTextBox)
        {
            LocalizationSearchWindow window = CreateInstance<LocalizationSearchWindow>();
            window.titleContent = new GUIContent("Localisation Search");
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
            
            var node = _property.serializedObject.targetObject as DialogueNodeObject;
            var text = node.text;
            var table = LocalizationSystem.GetTable(text.csvFile);
            VisualElement root = new ScrollView()
                { style = {flexDirection = FlexDirection.Column} };
            var searchField = new ToolbarSearchField(){contentContainer = { style = { maxWidth = 1000}},style = { maxWidth = 1000}};
            rootVisualElement.Add(searchField);
            Texture2D selectIcon = Resources.Load<Texture2D>("Icons/select");
            
            root.Add(new IMGUIContainer((() =>
            {
                foreach (var tableEntry in table.Where(e => e.entry[LocalizationSettings.Instance.selectedLanguage].ToLower().Contains(searchField.value.ToLower())))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        string label = Array.IndexOf(table, tableEntry) == text.value
                            ? $"<color=#94F5AD>{tableEntry.entry[LocalizationSettings.Instance.selectedLanguage]}</color>"
                            : tableEntry.entry[LocalizationSettings.Instance.selectedLanguage];
                        int height =(int)( 24 * (1 + tableEntry.entry[LocalizationSettings.Instance.selectedLanguage].Length / 144f));
                        GUILayout.Label(tableEntry.entry[0],new GUIStyle("helpbox"){richText = true}, GUILayout.Width(75), GUILayout.MinHeight(height));
                        GUILayout.Label(label,new GUIStyle("helpbox"){richText = true, wordWrap = true, stretchHeight = true}, GUILayout.MinHeight(24), GUILayout.MaxHeight(height));
                        if (GUILayout.Button(selectIcon, GUILayout.Width(24), GUILayout.MinHeight(height)))
                        {
                            SelectValue(tableEntry,table,text,node);
                        }
                    }
                }
            })));
            // foreach (var tableEntry in table)
            // {
            //     VisualElement rower = new VisualElement () {style = {flexDirection = FlexDirection.Row, maxHeight = 1000, flexGrow = 1, backgroundColor = new Color(0.17f, 0.17f, 0.17f)}};
            //     rower.Add(new HelpBox(){text = tableEntry.entry[LocalizationSettings.Instance.selectedLanguage], style = {flexDirection = FlexDirection.Row, maxHeight = 1000, flexGrow = 1, flexWrap = Wrap.Wrap}});
            //     rower.Add(new Button(() =>
            //     {
            //         SelectValue(tableEntry,table,text, node);
            //     }){contentContainer = { style = { unitySliceBottom = -20, unitySliceLeft = -20, unitySliceRight = -20, unitySliceTop = -20}},style = { backgroundImage = Resources.Load<Texture2D>("Icons/select"), unityBackgroundScaleMode = ScaleMode.ScaleToFit}});
            //     root.Add(rower);
            // }
            
            

            rootVisualElement.style.borderBottomColor = new Color(.35f,.35f,.35f);
            rootVisualElement.style.borderBottomWidth = 1;
            rootVisualElement.style.borderTopColor = new Color(.35f,.35f,.35f);
            rootVisualElement.style.borderTopWidth = 1;
            rootVisualElement.style.borderLeftColor = new Color(.35f,.35f,.35f);
            rootVisualElement.style.borderLeftWidth = 1;
            rootVisualElement.style.borderRightColor = new Color(.35f,.35f,.35f);
            rootVisualElement.style.borderRightWidth = 1;
            rootVisualElement.Add(root);
        }

        void SelectValue(LocalizationSystem.TableEntry entry, LocalizationSystem.TableEntry[] table, LocalisedString text, NodeObject node)
        {
            text.value = Array.IndexOf(table, entry);
            text.Reset();
            _localisedTextBox.text = text;
            EditorUtility.SetDirty(node);
            AssetDatabase.SaveAssets();
            Close();
        }
    }
}