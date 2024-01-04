using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZGraph.DialogueSystem;

namespace ZGraph
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Node Node;
        protected static ZGraphView currentGraphView;

        public virtual void BuildNodeView(Node node, Graph graph)
        {
            foreach (var field in node.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var outputAttribute = field.GetCustomAttribute<OutputAttribute>();
                Port port = null;
                if (outputAttribute != null)
                {
                    port = CreateOutputPort(field.FieldType, field.Name, outputContainer, node,
                        outputAttribute.PortOptions == PortOptions.Single ? Port.Capacity.Single : Port.Capacity.Multi);
                }

                var inputAttribute = field.GetCustomAttribute<InputAttribute>();
                port = null;
                if (inputAttribute != null)
                {
                    port = CreateInputPort(field.FieldType, field.Name, inputContainer, node,
                        inputAttribute.PortOptions == PortOptions.Single ? Port.Capacity.Single : Port.Capacity.Multi);
                }
            }

            RebuildContentContainer(node, graph);

            capabilities ^= Capabilities.Deletable;
            capabilities ^= Capabilities.Collapsible;
            var nodeDisplayAttribute = node.GetType().GetCustomAttribute<ZNodeDisplay>();
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
        
        private void RebuildContentContainer(Node node, Graph graph)
        {
            extensionContainer.Clear();
            
            foreach (var field in Node.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<InputAttribute>() != null)
                {
                    var edgeDatas = graph.GetInputConnectionsTo($"{node.guid}_{field.Name}");
                    foreach (var edgeData in edgeDatas)
                    {
                        var button = new Button();
                        button.text = edgeData.outputPortID;
                        button.clicked += () =>
                        {
                            ((UnnamedNode)Node).Execute();
                            Debug.Log(field.GetValue(Node));
                        };
                        extensionContainer.Add(button);
                    }
                }

                if (field.GetCustomAttribute<OutputAttribute>() != null)
                {
                    var edgeDatas = graph.GetOutputConnectionsTo($"{node.guid}_{field.Name}");
                    foreach (var edgeData in edgeDatas)
                    {
                        var button = new Button();
                        button.text = edgeData.inputPortID;
                        button.clicked += () =>
                        {
                            ((UnnamedNode)Node).Execute();
                            Debug.Log(field.GetValue(Node));
                        };
                        extensionContainer.Add(button);
                    }
                }
            }
        }

        public void OnConnectEdgeToInputPort(Edge edge, bool populating)
        {
            if (populating) return;
            
            var inputNodeView = (NodeView)edge.input.node;

            inputNodeView.RebuildContentContainer(inputNodeView.Node, currentGraphView.graph);
        }

        public void OnConnectEdgeToOutputPort(Edge edge, bool populating)
        {
            if (populating) return;
            
            var outputNodeView = (NodeView)edge.output.node;

            outputNodeView.RebuildContentContainer(outputNodeView.Node, currentGraphView.graph);
        }

        public void OnDisconnectEdgeFromInputPort(Edge edge, bool populating)
        {
            if (populating) return;
            
            var inputNodeView = (NodeView)edge.input.node;

            inputNodeView.RebuildContentContainer(inputNodeView.Node, currentGraphView.graph);
        }

        public void OnDisconnectEdgeFromOutputPort(Edge edge, bool populating)
        {
            if (populating) return;
            
            var outputNodeView = (NodeView)edge.output.node;

            outputNodeView.RebuildContentContainer(outputNodeView.Node, currentGraphView.graph);
        }
        
        protected void ForceCollapsable()
        {
            var ghostPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, null);
            ghostPort.style.display = DisplayStyle.None;
            ghostPort.viewDataKey = "⌂";
            inputContainer.Add(ghostPort);
        }

        public static NodeView CreateNodeView(Node node, ZGraphView graphView)
        {
            currentGraphView = graphView;
            NodeView nodeView = new NodeView();

            nodeView.Node = node;
            nodeView.viewDataKey = node.guid;
            nodeView.BuildNodeView(node, graphView.graph);
            var font = Resources.Load<Font>("Fonts/FugazOne");
            nodeView.Q<Label>("title-label").style.unityFont = font;
            nodeView.Q<Label>("title-label").style.unityFontDefinition = new StyleFontDefinition(font);
            nodeView.titleContainer.parent.style.alignItems = Align.Stretch;

            nodeView.SetPosition(new Rect(new Vector2(node.position.x, node.position.y),
                new Vector2(100, 100)));
            nodeView.style.left = node.position.x;
            nodeView.style.top = node.position.y;
            nodeView.mainContainer.style.backgroundColor = new StyleColor(new Color(0.17f, 0.17f, 0.17f));
            nodeView.RefreshExpandedState();
            nodeView.RefreshPorts();

            return nodeView;
        }

        
        
        private string nodeName;
        private bool collapsed;

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


        protected Port CreateInputPort(Type type, string portName, VisualElement container, Node node,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
            input.portName = portName;

            Color color = Color.white;
            if (colorMap.TryGetValue(type, out var _color))
            {
                color = _color;
            }
            else
            {
                var parsed = ColorUtility.TryParseHtmlString($"#{GetRandomColor(type.AssemblyQualifiedName)}FF", out _color);
                if(parsed) color = _color;
            }
            
            Color.RGBToHSV(color, out float H, out float S, out float B);
            // S *= .5f;
            input.Q<Label>().style.color = Color.HSVToRGB(H, S, B);
            input.portColor = color;
            input.viewDataKey = $"{node.guid}_{portName}";
            container?.Add(input);
            return input;
        }

        protected Port CreateOutputPort(Type type, string portName, VisualElement container, Node node,
            Port.Capacity portCapacity = Port.Capacity.Multi, Orientation orientation = Orientation.Horizontal)
        {
            var output = InstantiatePort(orientation, Direction.Output, portCapacity, type);

            Color color = Color.white;
            if (colorMap.TryGetValue(type, out var _color))
            {
                color = _color;
            }
            else
            {
                var parsed = ColorUtility.TryParseHtmlString($"#{GetRandomColor(type.AssemblyQualifiedName)}FF", out _color);
                if(parsed) color = _color;
            }


            Color.RGBToHSV(color, out float H, out float S, out float B);
            // S *= .5f;
            output.Q<Label>().style.color = Color.HSVToRGB(H, S, B);
            output.portName = portName;
            output.portColor = color;
            output.viewDataKey = $"{node.guid}_{portName}";
            container?.Add(output);
            return output;
        }

        protected Dictionary<Type, Color> colorMap = new Dictionary<Type, Color>()
        {
            // { typeof(Fact), new Color(1f, 0.65f, 0f) },
            // { typeof(SequentialDialogueNodeObject), new Color(0.55f, 0.42f, 1f) },
            // { typeof(bool), new Color(0.25f, 0.88f, 1f) }
        };
        
        static string GetRandomColor(string raw)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(raw));
                return BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 6);
            }
        }

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