using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZDialoguer;

public abstract class SequencialNodeView : NodeView
{
    public override NodeView BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph, ref int index)
    {
        CreateInputPort(typeof(NodeObject), "►", inputContainer, nodeObject, ref index);
        CreateOutputPort(typeof(NodeObject), "True ►", nodeObject, ref index);
        CreateOutputPort(typeof(NodeObject), "False ►", nodeObject, ref index);
        return this;
    }

    public abstract override void OnConnectEdgeToInputPort(Edge edge);
    public abstract override void OnConnectEdgeToOutputPort(Edge edge);
    public abstract override void OnDisconnectEdgeFromInputPort(Edge edge);
    public abstract override void OnDisconnectEdgeFromOutputPort(Edge edge);
}
