using UnityEditor.Experimental.GraphView;

namespace ZDialoguer
{
    public class ChoiceNodeView : SequentialNodeView
    {
        public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
        {
            base.BuildNodeView(nodeObject, graph);
            int index = 0;
            CreateInputPort(typeof(SequentialNodeObject), "►", inputContainer, NodeObject, ref index);
            CreateOutputPort(typeof(SequentialNodeObject), "►", outputContainer, NodeObject, ref index);
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
}