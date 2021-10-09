using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguer.Localization;
using ZDialoguer.Localization.Editor;

public class DialogueNodeView : SequencialNodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        int index = 0;
        var dialogueNodeObject = nodeObject as DialogueNodeObject;
        base.BuildNodeView(nodeObject, graph);
        CreateInputPort(typeof(SequencialNodeObject), "►", inputContainer, nodeObject, ref index, Port.Capacity.Multi);
        title = "Dialogue Node";
        CreateOutputPort(typeof(SequencialNodeObject), "►",outputContainer, nodeObject, ref index, Port.Capacity.Single);
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.58f, 0.96f, 0.68f));
        // dialogueNodeObject.text = new LocalisedString(true);
        dialogueNodeObject.text.csvFile = graph.dialogueText;
        dialogueNodeObject.text.csvFileFullAssetPath = Path.Combine(Application.dataPath.Substring(0,Application.dataPath.Length-6),AssetDatabase.GetAssetPath(dialogueNodeObject.text.csvFile));

        mainContainer.Add(new LocalisedStringPropertyDrawer().CreatePropertyGUI(new SerializedObject(nodeObject).FindProperty("text")));
    }
    public override void OnConnectEdgeToOutputPort(Edge edge) => edge.IsInputKey('0', () => (NodeObject as DialogueNodeObject).connectedChild = (edge.input.node as NodeView).NodeObject as SequencialNodeObject);
    public override void OnDisconnectEdgeFromOutputPort(Edge edge) => edge.IsInputKey('0', () => (NodeObject as DialogueNodeObject).connectedChild = null);
    public override void OnConnectEdgeToInputPort(Edge edge) { }
    public override void OnDisconnectEdgeFromInputPort(Edge edge) { }
}