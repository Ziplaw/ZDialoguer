using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguerEditor;
using PointerType = UnityEngine.PointerType;

namespace ZDialoguer
{
    public abstract class NodeView : Node
    {
        public Action<NodeView> OnNodeSelected;
        public NodeObject NodeObject;
        public abstract void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph, ref int index);
        public abstract void OnConnectEdgeToInputPort(Edge edge);
        public abstract void OnConnectEdgeToOutputPort(Edge edge);
        public abstract void OnDisconnectEdgeFromInputPort(Edge edge);
        public abstract void OnDisconnectEdgeFromOutputPort(Edge edge);

        public static NodeView CreateNodeView(NodeObject nodeObject, ZDialogueGraph graph)
        {
            int index = 0;

            NodeView nodeView = nodeViewMap[nodeObject.GetType()].Invoke();

            nodeView.NodeObject = nodeObject;
            nodeView.viewDataKey = nodeObject.guid;
            nodeView.BuildNodeView(nodeObject, graph, ref index);
            nodeView.style.left = nodeObject.position.x;
            nodeView.style.top = nodeObject.position.y;
            nodeView.mainContainer.style.backgroundColor = new StyleColor(new Color(0.17f, 0.17f, 0.17f));
            nodeView.RefreshExpandedState();
            nodeView.RefreshPorts();

            return nodeView;
        }

        private static Dictionary<Type, Func<NodeView>> nodeViewMap =
            new Dictionary<Type, Func<NodeView>>
            {
                { typeof(GraphStartNodeObject), () => new GraphStartNodeView() },
                { typeof(PredicateNodeObject), () => new PredicateNodeView() },
                { typeof(DialogueNodeObject), () => new DialogueNodeView() },
                { typeof(FactNodeObject), () => new FactNodeView() },
            };


        protected void CreateInputPort(Type type, string portName, VisualElement container, NodeObject nodeObject,
            ref int index,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
            input.portName = portName;
            input.portColor = colorMap[type];
            input.viewDataKey = nodeObject.guid + " " + index;
            index++;
            container.Add(input);
        }

        protected void CreateOutputPort(Type type, string portName, VisualElement container, NodeObject nodeObject, ref int index,
            Port.Capacity portCapacity = Port.Capacity.Multi, Orientation orientation = Orientation.Horizontal)
        {
            var output = InstantiatePort(orientation, Direction.Output, portCapacity, type);
            output.portName = portName;
            output.portColor = colorMap[type];
            output.viewDataKey = nodeObject.guid + " " + index;
            index++;
            container.Add(output);
        }

        protected Dictionary<Type, Color> colorMap = new Dictionary<Type, Color>()
        {
            { typeof(Fact), new Color(1f, 0.65f, 0f) },
            { typeof(SequencialNodeObject), new Color(0.55f, 0.42f, 1f) }
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

    public static class Extensions
    {
        public static bool IsOutputKey(this Edge edge, char key, Action action)
        {
            if (edge.output.viewDataKey.Last() == key)
            {
                action.Invoke();
                return true;
            }

            return false;
        }

        public static bool IsInputKey(this Edge edge, char key, Action action)
        {
            if (edge.input.viewDataKey.Last() == key)
            {
                action.Invoke();
                return true;
            }

            return false;
        }
    }
}