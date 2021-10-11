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

public class DialogueNodeView : SequentialNodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        base.BuildNodeView(nodeObject, graph);
        int index = 0;
        var dialogueNodeObject = nodeObject as DialogueNodeObject;
        CreateInputPort(typeof(SequentialNodeObject), "►", inputContainer, nodeObject, ref index, Port.Capacity.Multi);
        title = "Dialogue Node";
        CreateOutputPort(typeof(SequentialNodeObject), "►",outputContainer, nodeObject, ref index, Port.Capacity.Single);
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.58f, 0.96f, 0.68f));
        // dialogueNodeObject.text = new LocalisedString(true);
        dialogueNodeObject.text.csvFile = graph.dialogueText;
        dialogueNodeObject.text.csvFileFullAssetPath = LocalisedString.GetFullAssetPath(dialogueNodeObject.text.csvFile);

        // Debug.Log(this.Q("title").Q<Button>());
        // var container = new VisualElement();
        // container.Add();

        var propertyDrawer =
            new LocalisedStringPropertyDrawer().CreatePropertyGUI(
                new SerializedObject(nodeObject).FindProperty("text"));

        propertyDrawer.style.marginBottom = 5;
        propertyDrawer.style.marginLeft = 5;
        propertyDrawer.style.marginRight = 5;
        propertyDrawer.style.marginTop = 5;
        extensionContainer.Add(propertyDrawer);

        var ghostPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, null);
        ghostPort.style.display = DisplayStyle.None;
        ghostPort.viewDataKey = "⌂";
        inputContainer.Add(ghostPort);
    }

    public override void OnConnectEdgeToOutputPort(Edge edge) => edge.IsInputKey('0', () => (NodeObject as DialogueNodeObject).connectedChild = (edge.input.node as NodeView).NodeObject as SequentialNodeObject);
    public override void OnDisconnectEdgeFromOutputPort(Edge edge) => edge.IsInputKey('0', () => (NodeObject as DialogueNodeObject).connectedChild = null);
    public override void OnConnectEdgeToInputPort(Edge edge) { }
    public override void OnDisconnectEdgeFromInputPort(Edge edge) { }
}