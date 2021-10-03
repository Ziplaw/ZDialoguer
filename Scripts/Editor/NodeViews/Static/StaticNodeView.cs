using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZDialoguer;

public abstract class StaticNodeView : NodeView
{
    public override NodeView BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph, ref int index)
    {
        return this;
    }

    public abstract override void OnConnectEdgeToInputPort(Edge edge);
    public abstract override void OnConnectEdgeToOutputPort(Edge edge);
    public abstract override void OnDisconnectEdgeFromInputPort(Edge edge);
    public abstract override void OnDisconnectEdgeFromOutputPort(Edge edge);
}
