using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZGraph
{
    public class ZNodeView : Node
    {
        public ZNode Node;
        protected static ZGraphView currentGraphView;

        public virtual void BuildNodeView(ZNode Node, ZGraph graph)
        {
            int index = 0;

            inputContainer.Clear();
            outputContainer.Clear();

            foreach (var method in Node.GetType().GetMethods())
            {
                var outputAttribute = method.GetCustomAttribute<OutputAttribute>();
                Port port = null;
                if (outputAttribute != null)
                {
                    port = CreateOutputPort(method.ReturnType, method.Name, outputContainer, Node, ref index,
                        outputAttribute.PortOptions == PortOptions.Single ? Port.Capacity.Single : Port.Capacity.Multi);
                }

                if (port != null)
                {
                    var edgeDatas = graph.GetInputConnectionsTo(port.viewDataKey);
                    foreach (var edgeData in edgeDatas)
                    {
                        var button = new Button();
                        button.text = method.Name;
                        button.clicked += () => method.Invoke(Node, new object[] { edgeData });
                        contentContainer.Add(button);
                    }
                }

                var inputAttribute = method.GetCustomAttribute<InputAttribute>();
                port = null;
                if (inputAttribute != null)
                {
                    port = CreateInputPort(method.ReturnType, method.Name, inputContainer, Node, ref index,
                        inputAttribute.PortOptions == PortOptions.Single ? Port.Capacity.Single : Port.Capacity.Multi);
                }

                if (port != null)
                {
                    var edgeDatas = graph.GetOutputConnectionsTo(port.viewDataKey);
                    foreach (var edgeData in edgeDatas)
                    {
                        var button = new Button();
                        button.text = method.Name;
                        button.clicked += () => method.Invoke(Node, new object[] { edgeData });
                        contentContainer.Add(button);
                    }
                }
            }

            capabilities ^= Capabilities.Deletable;
            capabilities ^= Capabilities.Collapsible;
            var nodeDisplayAttribute = Node.GetType().GetCustomAttribute<ZNodeDisplay>();
            if (nodeDisplayAttribute != null)
            {
                title = nodeDisplayAttribute.DisplayName;
                titleContainer.style.backgroundColor = nodeDisplayAttribute.Color;
            }
            else
            {
                title = Node.GetType().Name;
                titleContainer.style.backgroundColor = new StyleColor(Color.white);
            }

            // titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.57f, 0.96f));
            mainContainer.style.alignItems = Align.Center;
        }

        protected void ForceCollapsable()
        {
            var ghostPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, null);
            ghostPort.style.display = DisplayStyle.None;
            ghostPort.viewDataKey = "⌂";
            inputContainer.Add(ghostPort);
        }

        public void OnConnectEdgeToInputPort(Edge edge, bool populating)
        {
            
        }

        public void OnConnectEdgeToOutputPort(Edge edge, bool populating)
        {
            // if (populating) return;
            //
            // var outputNodeView = (ZNodeView)edge.input.node;
            // var inputNodeView = (ZNodeView)edge.input.node;
            //
            // outputNodeView.BuildNodeView(outputNodeView.Node, currentGraphView.graph);
            // inputNodeView.BuildNodeView(inputNodeView.Node, currentGraphView.graph);
        }

        public void OnDisconnectEdgeFromInputPort(Edge edge, bool populating)
        {
            
        }

        public void OnDisconnectEdgeFromOutputPort(Edge edge, bool populating)
        {
            // if (populating) return;
            //
            // var outputNodeView = (ZNodeView)edge.input.node;
            // var inputNodeView = (ZNodeView)edge.input.node;
            //
            // outputNodeView.BuildNodeView(outputNodeView.Node, currentGraphView.graph);
            // inputNodeView.BuildNodeView(inputNodeView.Node, currentGraphView.graph);
        }

        public static ZNodeView CreateNodeView(ZNode zNode, ZGraphView graphView)
        {
            currentGraphView = graphView;
            ZNodeView zNodeView = new ZNodeView();

            zNodeView.Node = zNode;
            zNodeView.viewDataKey = zNode.guid;
            zNodeView.BuildNodeView(zNode, graphView.graph);
            var font = Resources.Load<Font>("Fonts/FugazOne");
            zNodeView.Q<Label>("title-label").style.unityFont = font;
            zNodeView.Q<Label>("title-label").style.unityFontDefinition = new StyleFontDefinition(font);

            zNodeView.SetPosition(new Rect(new Vector2(zNode.position.x, zNode.position.y),
                new Vector2(100, 100)));
            zNodeView.style.left = zNode.position.x;
            zNodeView.style.top = zNode.position.y;
            zNodeView.mainContainer.style.backgroundColor = new StyleColor(new Color(0.17f, 0.17f, 0.17f));
            zNodeView.RefreshExpandedState();
            zNodeView.RefreshPorts();

            return zNodeView;
        }

        private static Dictionary<Type, Func<ZNodeView>> nodeViewMap =
            new Dictionary<Type, Func<ZNodeView>>
            {
                // { typeof(GraphStartDialogueNodeObject), () => new GraphStartZNodeView() },
                // { typeof(PredicateDialogueNodeObject), () => new PredicateZNodeView() },
                // { typeof(DialogueNodeObject), () => new DialogueZNodeView() },
                // { typeof(ChoiceDialogueNodeObject), () => new ChoiceZNodeView() },
                // { typeof(SwitchDialogueNodeObject), () => new SwitchZNodeView() },
                // { typeof(FactDialogueNode), () => new FactZNodeView() },
                // { typeof(ExitDialogueNodeObject), () => new ExitZNodeView() },
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


        protected Port CreateInputPort(Type type, string portName, VisualElement container, ZNode zNode,
            ref int index,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
            input.portName = portName;

            Color color = Color.white;
            if (colorMap.TryGetValue(type, out var _color))
            {
                color = _color;
            }

            Color.RGBToHSV(color, out float H, out float S, out float B);
            // S *= .5f;
            input.Q<Label>().style.color = Color.HSVToRGB(H, S, B);
            input.portColor = color;
            input.viewDataKey = zNode.guid + " " + index;
            index++;
            container?.Add(input);
            return input;
        }

        protected Port CreateOutputPort(Type type, string portName, VisualElement container, ZNode zNode,
            ref int index,
            Port.Capacity portCapacity = Port.Capacity.Multi, Orientation orientation = Orientation.Horizontal)
        {
            var output = InstantiatePort(orientation, Direction.Output, portCapacity, type);

            Color color = Color.white;
            if (colorMap.TryGetValue(type, out var _color))
            {
                color = _color;
            }


            Color.RGBToHSV(color, out float H, out float S, out float B);
            // S *= .5f;
            output.Q<Label>().style.color = Color.HSVToRGB(H, S, B);
            output.portName = portName;
            output.portColor = color;
            output.viewDataKey = zNode.guid + " " + index;
            index++;
            container?.Add(output);
            return output;
        }

        protected Dictionary<Type, Color> colorMap = new Dictionary<Type, Color>()
        {
            // { typeof(Fact), new Color(1f, 0.65f, 0f) },
            // { typeof(SequentialDialogueNodeObject), new Color(0.55f, 0.42f, 1f) },
            // { typeof(bool), new Color(0.25f, 0.88f, 1f) }
        };

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Node.position.x = newPos.xMin;
            Node.position.y = newPos.yMin;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            currentGraphView.OnNodeSelected.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            currentGraphView.OnNodeDeselected.Invoke(this);
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
        public static bool IsOutputKey(this Edge edge, int key, Action action = null)
        {
            if (edge.output.GetID(Direction.Output) == key)
            {
                action?.Invoke();
                return true;
            }

            return false;
        }

        public static bool IsInputKey(this Edge edge, int key, Action action = null)
        {
            if (edge.input.GetID(Direction.Input) == key)
            {
                action?.Invoke();
                return true;
            }

            return false;
        }

        public static int GetID(this Port port, Direction portDirection)
        {
            switch (portDirection)
            {
                case Direction.Input: return Convert.ToInt32(port.viewDataKey.Split(' ').First());
                case Direction.Output: return Convert.ToInt32(port.viewDataKey.Split(' ').Last());
                default: return -1;
            }
        }

        public static string WithColor(this string str, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
        }
    }
}