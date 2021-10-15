using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguerEditor;
using Object = UnityEngine.Object;
using PointerType = UnityEngine.PointerType;

namespace ZDialoguer
{
    public abstract class NodeView : Node
    {
        public Action<NodeView> OnNodeSelected;
        public NodeObject NodeObject;
        protected static ZDialoguerGraphView currentGraphView;

        public virtual void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
        {
        }

        public abstract void OnConnectEdgeToInputPort(Edge edge);
        public abstract void OnConnectEdgeToOutputPort(Edge edge);
        public abstract void OnDisconnectEdgeFromInputPort(Edge edge);
        public abstract void OnDisconnectEdgeFromOutputPort(Edge edge);

        public static NodeView CreateNodeView(NodeObject nodeObject, ZDialoguerGraphView graphView)
        {
            currentGraphView = graphView;
            NodeView nodeView = nodeViewMap[nodeObject.GetType()].Invoke();

            nodeView.NodeObject = nodeObject;
            nodeView.viewDataKey = nodeObject.guid;
            nodeView.BuildNodeView(nodeObject, graphView.graph);
            nodeView.SetPosition(new Rect(new Vector2(nodeObject.position.x, nodeObject.position.y),
                new Vector2(100, 100)));
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
                { typeof(ChoiceNodeObject), () => new ChoiceNodeView() },
                { typeof(SwitchNodeObject), () => new SwitchNodeView() },
                { typeof(FactNodeObject), () => new FactNodeView() },
            };

        private string nodeName;
        private bool collapsed;

        protected void RepopulateGraph()
        {
            var view = currentGraphView;
            view.PopulateView(view.graph);
        }

        protected override void ToggleCollapse()
        {
            base.ToggleCollapse();
            if (!collapsed)
            {
                nodeName = title;
                if (title.Contains("Node")) title = nodeName.Substring(0, title.Length - 5);
            }
            else
            {
                title = nodeName;
            }


            collapsed = !collapsed;
        }


        protected Port CreateInputPort(Type type, string portName, VisualElement container, NodeObject nodeObject,
            ref int index,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
            input.portName = portName;
            Color.RGBToHSV(colorMap[type], out float H, out float S, out float B);
            // S *= .5f;
            input.Q<Label>().style.color = Color.HSVToRGB(H, S, B);
            input.portColor = colorMap[type];
            input.viewDataKey = nodeObject.guid + " " + index;
            index++;
            container?.Add(input);
            return input;
        }

        protected Port CreateOutputPort(Type type, string portName, VisualElement container, NodeObject nodeObject,
            ref int index,
            Port.Capacity portCapacity = Port.Capacity.Multi, Orientation orientation = Orientation.Horizontal)
        {
            var output = InstantiatePort(orientation, Direction.Output, portCapacity, type);
            Color.RGBToHSV(colorMap[type], out float H, out float S, out float B);
            // S *= .5f;
            output.Q<Label>().style.color = Color.HSVToRGB(H, S, B);
            output.portName = portName;
            output.portColor = colorMap[type];
            output.viewDataKey = nodeObject.guid + " " + index;
            index++;
            container?.Add(output);
            return output;
        }

        protected Dictionary<Type, Color> colorMap = new Dictionary<Type, Color>()
        {
            { typeof(Fact), new Color(1f, 0.65f, 0f) },
            { typeof(SequentialNodeObject), new Color(0.55f, 0.42f, 1f) },
            { typeof(bool), new Color(0.25f, 0.88f, 1f) }
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

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("Log Node Ports", LogNodePorts);
        }

        private void LogNodePorts(DropdownMenuAction action)
        {
            this.Query<Port>().ForEach(x =>
            {
                Debug.Log($"[<color=#{ColorUtility.ToHtmlStringRGB(x.portColor)}>{x.portName}</color>]" + " " +
                          x.viewDataKey);
            });
        }
    }

    public static class Extensions
    {
        public static bool IsOutputKey(this Edge edge, string key, Action action)
        {
            if (edge.output.viewDataKey.Split(' ').Last() == key)
            {
                action.Invoke();
                return true;
            }

            return false;
        }

        public static bool IsInputKey(this Edge edge, string key, Action action)
        {
            if (edge.input.viewDataKey.Split(' ').Last() == key)
            {
                action.Invoke();
                return true;
            }

            return false;
        }

        public static string WithColor(this string str, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
        }
    }
}