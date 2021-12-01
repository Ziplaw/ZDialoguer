using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

public class FactNodeView : StaticNodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        this.Q("title-button-container").RemoveFromHierarchy();
        
        int index = 0;
        FactNodeObject factNodeObject = nodeObject as FactNodeObject;

        base.BuildNodeView(nodeObject, graph);
        this.Q("title-label").style.color = colorMap[typeof(Fact)];
        var port = CreateOutputPort(typeof(Fact), "", titleContainer, nodeObject, ref index);
        port.style.alignSelf = Align.Center;

        title = GlobalData.Instance.facts[factNodeObject.factIndex].nameID;
    }

    public override void OnConnectEdgeToInputPort(Edge edge)
    {
    }

    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
    }
}