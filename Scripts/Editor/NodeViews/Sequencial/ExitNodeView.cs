using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZGraph.DialogueSystem
{
    public class ExitZNodeView : SequentialZNodeView
    {
        public override void BuildNodeView(ZNode Node, ZGraph graph)
        {
            base.BuildNodeView(Node, graph);

            capabilities ^= Capabilities.Collapsible;
            
            int index = 0;
            titleContainer.style.backgroundColor = new Color(153 / 255f, 145 / 255f, 245 / 255f);
            title = "Exit";
            CreateInputPort(typeof(SequentialDialogueNodeObject), "", mainContainer, Node, ref index, Port.Capacity.Multi);
        }

        // public override void OnConnectEdgeToInputPort(Edge edge) { }
        // public override void OnConnectEdgeToOutputPort(Edge edge) { }
        // public override void OnDisconnectEdgeFromInputPort(Edge edge) { }
        // public override void OnDisconnectEdgeFromOutputPort(Edge edge) { }
    }
}