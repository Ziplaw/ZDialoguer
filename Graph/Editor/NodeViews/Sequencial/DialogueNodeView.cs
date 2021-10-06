using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

public class DialogueNodeView : SequencialNodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph, ref int index)
    {
        base.BuildNodeView(nodeObject, graph, ref index);
        CreateInputPort(typeof(SequencialNodeObject), "►", inputContainer, nodeObject, ref index);
        title = "Dialogue Node";
        CreateOutputPort(typeof(SequencialNodeObject), "►",outputContainer, nodeObject, ref index, Port.Capacity.Single);
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.58f, 0.96f, 0.68f));

        TextField textField = new TextField(int.MaxValue, true, false, '*');
        textField.style.flexWrap = Wrap.Wrap;
        textField.style.alignContent = Align.Center;
        textField.style.maxHeight = 150;
        textField.SetValueWithoutNotify((nodeObject as DialogueNodeObject).text);
        textField.Q<TextInputBaseField<string>>().style.flexWrap = Wrap.Wrap;
        textField.Q<TextInputBaseField<string>>().style.width = 170;
        textField.Q<TextInputBaseField<string>>().style.alignSelf = Align.Center;
        textField.Q<TextInputBaseField<string>>().style.whiteSpace = WhiteSpace.Normal;
        textField.Q<TextInputBaseField<string>>().style.maxHeight = 150;
        // textField.RegisterValueChangedCallback<LocalisedString>((evt =>
        // {
        //     (nodeObject as DialogueNodeObject).text = evt.newValue;
        // }));
        mainContainer.Add(textField);
    }
    public override void OnConnectEdgeToOutputPort(Edge edge) => edge.IsInputKey('0', () => (NodeObject as DialogueNodeObject).connectedChild = (edge.input.node as NodeView).NodeObject as SequencialNodeObject);
    public override void OnDisconnectEdgeFromOutputPort(Edge edge) => edge.IsInputKey('0', () => (NodeObject as DialogueNodeObject).connectedChild = null);
    public override void OnConnectEdgeToInputPort(Edge edge) { }
    public override void OnDisconnectEdgeFromInputPort(Edge edge) { }
}