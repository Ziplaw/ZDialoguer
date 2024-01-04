using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using ZDialoguer;

namespace ZGraph
{
    public abstract class Graph : ScriptableObject
    {
        internal virtual string GraphTypeName => "Generic Graph";
        
        [FormerlySerializedAs("nodes")] public List<Node> Nodes = new();
        public List<EdgeData> edgeDatas = new();
        public Dictionary<string, Node> NodeDictionary;

        public virtual void OnPopulate(Vector2 size)
        {
        }

        public void CreateNode<T>(Vector2 nodePosition) where T : Node
        {
            var node = CreateInstance<T>();
            node.OnCreate(nodePosition, this);
        }

        public Node GetNode(string portID)
        {
            return Nodes.Find(n => n.guid == GetPortInfo(portID).guid);
        }

        public IEnumerable<EdgeData> GetInputConnectionsTo(string portID)
        {
            return edgeDatas.Where(e => e.inputPortID == portID);
        }
        
        public IEnumerable<EdgeData> GetOutputConnectionsTo(string portID)
        {
            return edgeDatas.Where(e => e.outputPortID == portID);
        }
        
        (string guid, int index) GetPortInfo(string portID)
        {
            var split = portID.Split(" ");

            return (split[0], int.Parse(split[1]));
        }
        
        public void RemoveEdge(string inputPortID, string outputPortID)
        {
            edgeDatas.Remove(edgeDatas.FirstOrDefault(edgeData =>
                edgeData.inputPortID == inputPortID && edgeData.outputPortID == outputPortID));
        }

        public void AddEdge(string inputPortID, string outputPortID)
        {
            if (edgeDatas.Any(edgeData => edgeData.inputPortID == inputPortID && edgeData.outputPortID == outputPortID))
                return;
            
            edgeDatas.Add(new EdgeData(inputPortID, outputPortID));
        }

        public T GetNodeFromPortID<T>(string outputPortID) where T : Node
        {
            return (T)Nodes.Find(n => n is T && n.guid == outputPortID.Split("_")[0]);
        }
    }
    
    
    public enum PortOptions {Single, Multiple}

    [AttributeUsage(AttributeTargets.Field)]
    public class PortAttribute : Attribute
    {
        public readonly PortOptions PortOptions;

        public PortAttribute(PortOptions portOptions = PortOptions.Single)
        {
            PortOptions = portOptions;
        }
    }
}