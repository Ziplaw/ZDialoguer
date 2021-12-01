using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Codice.CM.SEIDInfo;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguer.Localization;
using ZDialoguer.Localization.Editor;

public class GlobalDataSearchWindow : EditorWindow
{
    private IEnumerable<DialogueData> itemList;
    private Action<int> OnSelectOption;
    private Action<IEnumerable<DialogueData>> OnAddItem;
    private Action<int> OnRemoveItem;
    private ZDialogueGraph graph;
    private Type itemListType;
 
    public static void Open<T>(ZDialogueGraph graph, List<T> list, Action<int> OnSelectOption,
        Action<IEnumerable<DialogueData>> OnAddItem, Action<int> OnRemoveItem, Vector2 position) where T : DialogueData
    {
        GlobalDataSearchWindow window = CreateInstance<GlobalDataSearchWindow>();
        window.itemList = list;
        window.OnSelectOption = OnSelectOption;
        window.OnAddItem = OnAddItem;
        window.OnRemoveItem = OnRemoveItem;
        window.itemListType = typeof(T);
        window.graph = graph;
        Rect r = new Rect(position, new Vector2(10, 10));
        window.ShowAsDropDown(r, new Vector2(500, 300));
    }


    private void CreateGUI()
    {
        rootVisualElement.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
        rootVisualElement.style.marginBottom = 5;
        rootVisualElement.style.marginRight = 5;
        rootVisualElement.style.marginLeft = 5;
        rootVisualElement.style.marginTop = 5;

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
            string[] nameIDs = null;

            switch (itemListType.Name)
            {
                case "Fact":
                    nameIDs = graph.facts.Select(f => GlobalData.Instance.facts[f].nameID).ToArray();
                    break;
                case "Character":
                    nameIDs = graph.characters.Select(c => GlobalData.Instance.characters[c].nameID).ToArray();
                    break;
            }

            foreach (var item in itemList.Select(e => e).Where(e =>
                !nameIDs.Contains(e.nameID) && e.nameID.ToLower().Contains(searchField.value.ToLower())))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(item.nameID,
                        new GUIStyle("helpbox") { richText = true, wordWrap = true, stretchHeight = true },
                        GUILayout.MinHeight(24), GUILayout.MaxHeight(24));
                    if (GUILayout.Button("-", GUILayout.Width(24), GUILayout.MinHeight(24)))
                    {
                        OnRemoveItem.Invoke(itemList.ToList().IndexOf(item));
                        itemList = (IEnumerable<DialogueData>) typeof(GlobalData).GetFields(BindingFlags.Public | BindingFlags.Instance)
                            .First(f => f.FieldType.GenericTypeArguments[0] == itemListType)
                            .GetValue(GlobalData.Instance);
                        return;
                    }
                    if (GUILayout.Button(selectIcon, GUILayout.Width(24), GUILayout.MinHeight(24)))
                    {
                        SelectValue(item);
                    }
                }
            }
        }));
        
        root.Add(new Button(() => OnAddItem.Invoke(itemList)){text = "+"});

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

    void SelectValue(DialogueData item)
    {
        OnSelectOption?.Invoke(itemList.ToList().IndexOf(item));
    }
}