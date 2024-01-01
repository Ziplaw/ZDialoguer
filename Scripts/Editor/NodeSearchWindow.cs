using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZGraph
{
    public class ZNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        internal ZGraphView graphView;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Nodes")),
                // new SearchTreeGroupEntry(new GUIContent("Static Nodes"), 1),
                // new SearchTreeEntry(new GUIContent("Fact Node")) { userData = typeof(FactDialogueNode), level = 2 },
                new SearchTreeEntry(new GUIContent("Exit Node")) { userData = typeof(DialogueSystem.ExitDialogueNodeObject), level = 2 },
                // new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),
                // new SearchTreeEntry(new GUIContent("Dialogue Node")) { userData = typeof(DialogueDialogueNodeObject), level = 2 },
                // new SearchTreeEntry(new GUIContent("Choice Node")) { userData = typeof(ChoiceDialogueNodeObject), level = 2 },
                // new SearchTreeGroupEntry(new GUIContent("Logic Nodes"), 1),
                // new SearchTreeEntry(new GUIContent("Predicate Node")) { userData = typeof(PredicateDialogueNodeObject), level = 2 },
                // new SearchTreeEntry(new GUIContent("Switch Node")) { userData = typeof(SwitchDialogueNodeObject), level = 2 },
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