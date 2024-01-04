using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZDialoguer;
using ZDialoguerEditor;

namespace ZGraph.DialogueSystem
{
    [CustomEditor(typeof(DialogueGraph))]
    public class ZDialogueGraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Graph"))
            {
                ZDialogueGraphEditorWindow.OpenWindow(target as DialogueGraph);
            }

            base.OnInspectorGUI();
        }
    }
}
