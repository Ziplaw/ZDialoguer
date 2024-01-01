using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

namespace ZGraph.DialogueSystem
{
    public class GraphStartZNodeView : SequentialZNodeView
    {
        public override void BuildNodeView(ZNode Node, ZGraph graph)
        {
            base.BuildNodeView(Node, graph);
            int index = 0;
            GraphStartDialogueNodeObject startDialogueNodeObject = Node as GraphStartDialogueNodeObject;
            capabilities ^= Capabilities.Deletable;
            capabilities ^= Capabilities.Collapsible;
            title = "Entry";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.57f, 0.96f));
            mainContainer.style.alignItems = Align.Center;

            CreateOutputPort(typeof(SequentialDialogueNodeObject), "", mainContainer, Node, ref index,
                Port.Capacity.Single,
                Orientation.Vertical);
        }

        // public override void OnConnectEdgeToInputPort(Edge edge)
        // {
        // }
        //
        // public override void OnConnectEdgeToOutputPort(Edge edge)
        // {
        //     
        // }
        //
        // public override void OnDisconnectEdgeFromInputPort(Edge edge)
        // {
        //     
        // }
        //
        // public override void OnDisconnectEdgeFromOutputPort(Edge edge)
        // {
        //     
        // }
    }
}