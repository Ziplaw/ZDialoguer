using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

public abstract class SequentialNodeView : NodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        base.BuildNodeView(nodeObject, graph);
        var ghostPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, null);
        ghostPort.style.display = DisplayStyle.None;
        ghostPort.viewDataKey = "⌂";
        inputContainer.Add(ghostPort);
    }

    public abstract override void OnConnectEdgeToInputPort(Edge edge);
    public abstract override void OnConnectEdgeToOutputPort(Edge edge);
    public abstract override void OnDisconnectEdgeFromInputPort(Edge edge);
    public abstract override void OnDisconnectEdgeFromOutputPort(Edge edge);
}
