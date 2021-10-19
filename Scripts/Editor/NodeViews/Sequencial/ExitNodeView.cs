using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZDialoguer
{
    public class ExitNodeView : SequentialNodeView
    {
        public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
        {
            // base.BuildNodeView(nodeObject, graph);
            int index = 0;
            titleContainer.style.backgroundColor = new Color(153 / 255f, 145 / 255f, 245 / 255f);
            title = "Exit";
            CreateInputPort(typeof(SequentialNodeObject), "", extensionContainer, nodeObject, ref index, Port.Capacity.Multi);
        }

        public override void OnConnectEdgeToInputPort(Edge edge) { }
        public override void OnConnectEdgeToOutputPort(Edge edge) { }
        public override void OnDisconnectEdgeFromInputPort(Edge edge) { }
        public override void OnDisconnectEdgeFromOutputPort(Edge edge) { }
    }
}