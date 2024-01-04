using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace ZGraph
{
    public abstract class Node : ScriptableObject
    {
        public Vector2 position;
        public string guid;
        public Graph graph;

        internal bool OnCreate(Vector2 position, Graph graph)
        {
            this.graph = graph;
            this.position = position;
            name = GetType().Name;
            guid = GUID.Generate().ToString();
            try
            {
                OnCreate();
            }
            catch (NodeNotCreatedException e)
            {
                Debug.LogWarning($"{GetType()} couldn't be created!: {e.Reason}");
                return false;
            }

            graph.Nodes.Add(this);

#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(this, graph);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
#endif

            return true;
        }

        [CanBeNull]
        public T GetConnectedNode<T>(string portName) where T : Node
        {
            var portID = $"{guid}_{portName}";
            var edgeDataIfInputPort = graph.edgeDatas.FirstOrDefault(edgeData => edgeData.inputPortID == portID);
            if (!edgeDataIfInputPort.Equals(default))
            {
                return graph.GetNodeFromPortID<T>(edgeDataIfInputPort.outputPortID);
            }
            
            var edgeDataIfOutputPort = graph.edgeDatas.FirstOrDefault(edgeData => edgeData.outputPortID == portID);
            if (!edgeDataIfOutputPort.Equals(default))
            {
                return graph.GetNodeFromPortID<T>(edgeDataIfOutputPort.inputPortID);
            }

            return null;
        }
        
        [CanBeNull]
        public T GetValueConnectedTo<T>(Node node, string portName)
        {
            var edgeDataIfInputPort = graph.edgeDatas.Find(edgeData => edgeData.inputPortID == $"{node.guid}_{portName}");
            if (!edgeDataIfInputPort.Equals(default))
            {
                return (T)GetType().GetField(edgeDataIfInputPort.outputPortID.Split("_")[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            }
            
            var edgeDataIfOutputPort = graph.edgeDatas.Find(edgeData => edgeData.outputPortID == $"{node.guid}_{portName}");
            if (!edgeDataIfOutputPort.Equals(default))
            {
                return (T)GetType().GetField(edgeDataIfOutputPort.inputPortID.Split("_")[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            }

            return default;
        }
        
        public IEnumerable<Node> GetConnectedNodes<T>(string portName) where T : Node
        {
            var portID = $"{guid}_{portName}";
            var edgeDataIfInputPort = graph.edgeDatas.Where(edgeData => edgeData.inputPortID == portID);
            bool found = false;
            
            foreach (var edgeData in edgeDataIfInputPort)
            {
                yield return graph.GetNodeFromPortID<T>(edgeData.outputPortID);
                found = true;
            }

            if (found) yield break;
            
            var edgeDataIfOutputPort = graph.edgeDatas.Where(edgeData => edgeData.outputPortID == portID);
            foreach (var edgeData in edgeDataIfOutputPort)
            {
                yield return graph.GetNodeFromPortID<T>(edgeData.inputPortID);
            }
        }
        
        protected virtual void OnCreate()
        {
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute : Attribute
    {
        public readonly PortOptions PortOptions;

        public OutputAttribute(PortOptions portOptions)
        {
            PortOptions = portOptions;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InputAttribute : Attribute
    {
        public readonly PortOptions PortOptions;

        public InputAttribute(PortOptions portOptions)
        {
            PortOptions = portOptions;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ZNodeDisplay : Attribute
    {
        public readonly Color Color;
        public readonly string DisplayName;

        public ZNodeDisplay(string color, string displayName)
        {
            if (ColorUtility.TryParseHtmlString(color, out var _color))
            {
                Color = _color;
            }
            else
            {
                Color = Color.white;
            }
            DisplayName = displayName;
        }
    }

    public class NodeNotCreatedException : Exception
    {
        public readonly string Reason;

        public NodeNotCreatedException(string reason)
        {
            Reason = reason;
        }
    }
}