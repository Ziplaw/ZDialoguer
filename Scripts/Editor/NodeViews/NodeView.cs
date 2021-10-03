using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguerEditor;

namespace ZDialoguer
{
    public abstract class NodeView : Node
    {
        public Action<NodeView> OnNodeSelected;
        public NodeObject NodeObject;
        public abstract NodeView BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph, ref int index);
        public abstract void OnConnectEdgeToInputPort(Edge edge);
        public abstract void OnConnectEdgeToOutputPort(Edge edge);
        public abstract void OnDisconnectEdgeFromInputPort(Edge edge);
        public abstract void OnDisconnectEdgeFromOutputPort(Edge edge);

        public static NodeView CreateNodeView(NodeObject nodeObject, ZDialogueGraph graph)
        {
            int index = 0;
            
            NodeView nodeView = nodeViewMap[nodeObject.GetType()].Invoke(nodeObject, graph)
                .BuildNodeView(nodeObject, graph, ref index);
            nodeView.NodeObject = nodeObject;
            nodeView.viewDataKey = nodeObject.guid;
            nodeView.style.left = nodeObject.position.x;
            nodeView.style.top = nodeObject.position.y;
            nodeView.mainContainer.style.backgroundColor = new StyleColor(new Color(0.17f, 0.17f, 0.17f));
            nodeView.RefreshPorts();

            return nodeView;
        }

        private static Dictionary<Type, Func<NodeObject, ZDialogueGraph, NodeView>> nodeViewMap =
            new Dictionary<Type, Func<NodeObject, ZDialogueGraph, NodeView>>()
            {
                { typeof(FactNodeObject), (nodeObject, graph) => new FactNodeView() },
                { typeof(PredicateNodeObject), (nodeObject, graph) => new PredicateNodeView() },
            };


        protected void CreateInputPort(Type type, string portName, VisualElement container, NodeObject nodeObject, ref int index,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
            input.portName = portName;
            input.portColor = colorMap[type];
            input.viewDataKey = nodeObject.guid + " " + index;
            index++;
            container.Add(input);
        }

        protected void CreateOutputPort(Type type, string portName, NodeObject nodeObject, ref int index)
        {
            var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
            output.portName = portName;
            output.portColor = colorMap[type];
            output.viewDataKey = nodeObject.guid + " " + index;
            index++;
            outputContainer.Add(output);
        }
        
        protected Dictionary<Type, Color> colorMap = new Dictionary<Type, Color>()
        {
            { typeof(Fact), new Color(1f, 0.65f, 0f) },
            { typeof(NodeObject), new Color(0.55f, 0.42f, 1f) }
        };

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            NodeObject.position.x = newPos.xMin;
            NodeObject.position.y = newPos.yMin;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }
    }
}