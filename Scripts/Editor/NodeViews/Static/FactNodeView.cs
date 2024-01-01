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

namespace ZGraph.DialogueSystem
{
    public class FactZNodeView : StaticZNodeView
    {
        public override void BuildNodeView(ZNode Node, ZGraph graph)
        {
            this.Q("title-button-container").RemoveFromHierarchy();

            int index = 0;
            FactDialogueNode factDialogueNode = Node as FactDialogueNode;

            base.BuildNodeView(Node, graph);
            this.Q("title-label").style.color = colorMap[typeof(Fact)];
            var port = CreateOutputPort(typeof(Fact), "", titleContainer, Node, ref index);
            port.style.alignSelf = Align.Center;

            title = factDialogueNode.fact.nameID;
        }

        // public override void OnConnectEdgeToInputPort(Edge edge)
        // {
        // }
        //
        // public override void OnConnectEdgeToOutputPort(Edge edge)
        // {
        // }
        //
        // public override void OnDisconnectEdgeFromInputPort(Edge edge)
        // {
        // }
        //
        // public override void OnDisconnectEdgeFromOutputPort(Edge edge)
        // {
        // }
    }
}