using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

    [CustomEditor(typeof(GlobalData))]
    public class GlobalDataCustomEditor : Editor
    {
        private GlobalData manager;

        private void OnEnable()
        {
            manager = target as GlobalData;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            var factListView = CreateFactListView(manager.facts);

            container.Add(new IMGUIContainer(() => base.OnInspectorGUI()));
            container.Add(factListView);
            
            return container;
        }

        private VisualElement CreateFactListView(List<Fact> list)
        {
            Func<VisualElement> makeItem = () =>
            {
                var root = new VisualElement();
                EnumField enumField = new EnumField(Fact.FactType.Float);
                root.Add(enumField);
                return root;
            };

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.Q<EnumField>().value = list[i].factType;
            };

            const int itemHeight = 48;

            var listView = new ListView(list, itemHeight, makeItem, bindItem);

            listView.headerTitle = "Facts";
            listView.showFoldoutHeader = true;
            listView.showAddRemoveFooter = true;
            listView.showBoundCollectionSize = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.reorderable = true;
            listView.showBorder = true;

            listView.selectionType = SelectionType.Multiple;

            // listView.onItemsChosen += objects => Debug.Log(objects);
            // listView.onSelectionChange += objects => Debug.Log(objects);
            listView.onSelectedIndicesChange += ints => Debug.Log(ints.Count());

            listView.style.flexGrow = 0f;

            return listView;
        }
    }