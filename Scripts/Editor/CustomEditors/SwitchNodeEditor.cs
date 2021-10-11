using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

[CustomEditor(typeof(SwitchNodeObject))]
public class SwitchNodeEditor : Editor
{
    private SwitchNodeObject manager;
    private VisualElement root;

    private void OnEnable()
    {
        root = new VisualElement();

        manager = target as SwitchNodeObject;
    }

    public override VisualElement CreateInspectorGUI()
    {
        ZDialogueGraphEditorWindow graphWindow = EditorWindow.GetWindow<ZDialogueGraphEditorWindow>();
        
        root.Add(InspectorView.GetNodeLabel("Switch Node", new Color(163 / 255f, 245 / 255f, 224 / 255f)));
        IMGUIContainer factElementContainer = new IMGUIContainer(() =>
        {
            GUILayout.Label(
                $"<color=#FFA600>Selected Fact: </color>{(manager.fact != null ? manager.fact.ToString() : "None")}",
                new GUIStyle("label") { richText = true });

            if (manager.fact)
                for (int i = 0; i < manager.outputEntries.Count; i++)
                {
                    if (manager.outputEntries[i].output)
                    {
                        string stringOutput;
                        var output = manager.outputEntries[i].output;
                        if (output is DialogueNodeObject _dialogueNodeObject)
                        {
                            string parsedText =
                                _dialogueNodeObject.text.ParseFacts(graphWindow.graphView.graph);
                            var split = parsedText.Split('\n')[0];
                            stringOutput = split.Substring(0, Mathf.Min(30, split.Length)) +
                                           "(...)";
                        }
                        else
                        {
                            stringOutput = output.ToString();
                        }
                        
                        GUILayout.Label(
                            $"<color=#9991F5>{manager.outputEntries[i].GetValue(manager.fact.factType)}</color> <color=#94F5AD>→</color> {stringOutput}",
                            new GUIStyle("label") { richText = true });
                    }
                }
        }) { style = { marginTop = 7, flexDirection = FlexDirection.Row } };

        // factElementContainer.usageHints;


        root.Add(factElementContainer);
        root.Add(new IMGUIContainer(OnInspectorGUI));

        return root;
    }
}