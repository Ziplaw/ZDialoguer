using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZDialoguer
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        internal ZDialoguerGraphView graphView;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Nodes")),
                new SearchTreeGroupEntry(new GUIContent("Static Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Fact Node")) { userData = typeof(FactNodeObject), level = 2 },
                new SearchTreeEntry(new GUIContent("Exit Node")) { userData = typeof(ExitNodeObject), level = 2 },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Dialogue Node")) { userData = typeof(DialogueNodeObject), level = 2 },
                new SearchTreeEntry(new GUIContent("Choice Node")) { userData = typeof(ChoiceNodeObject), level = 2 },
                new SearchTreeGroupEntry(new GUIContent("Logic Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Predicate Node")) { userData = typeof(PredicateNodeObject), level = 2 },
                new SearchTreeEntry(new GUIContent("Switch Node")) { userData = typeof(SwitchNodeObject), level = 2 },
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = graphView._editorWindow.rootVisualElement.ChangeCoordinatesTo(
                graphView._editorWindow.rootVisualElement.parent,
                context.screenMousePosition - graphView._editorWindow.position.position);
            var localMousePos = graphView.contentContainer.WorldToLocal(worldMousePosition);

            graphView.CreateNode(SearchTreeEntry.userData as Type, graphView.TransformMousePosition(localMousePos));
            return true;
        }
    }
}