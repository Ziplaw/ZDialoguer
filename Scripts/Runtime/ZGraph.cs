using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZDialoguer;

namespace ZGraph
{
    public abstract class ZGraph : ScriptableObject
    {
        internal virtual string GraphTypeName => "Generic Graph";
        
        public List<ZNode> nodes = new();
        public List<EdgeData> edgeDatas = new();
        
        public virtual void OnPopulate(Vector2 size)
        {
        }

        public void CreateNode<T>(Vector2 nodePosition) where T : ZNode
        {
            var node = CreateInstance<T>();
            node.OnCreate(nodePosition, this);
        }

        public ZNode GetNode(string portID)
        {
            return nodes.Find(n => n.guid == GetPortInfo(portID).guid);
        }

        public IEnumerable<EdgeData> GetInputConnectionsTo(string portID)
        {
            return edgeDatas.Where(e => e.outputPortViewDataKey == portID);
        }
        
        public IEnumerable<EdgeData> GetOutputConnectionsTo(string portID)
        {
            return edgeDatas.Where(e => e.inputPortViewDataKey == portID);
        }
        
        (string guid, int index) GetPortInfo(string portID)
        {
            var split = portID.Split(" ");

            return (split[0], int.Parse(split[1]));
        }
    }


    public static class Extensions
    {
        public static void RemoveEdge(this List<EdgeData> list, Edge edge)
        {
            list.Remove(list.FirstOrDefault(e =>
                e.inputPortViewDataKey == edge.input.viewDataKey &&
                e.outputPortViewDataKey == edge.output.viewDataKey));
        }

        public static void AddEdge(this List<EdgeData> list, Edge edge)
        {
            if (!list.Any(e =>
                    e.inputPortViewDataKey == edge.input.viewDataKey &&
                    e.outputPortViewDataKey == edge.output.viewDataKey))
                list.Add(new EdgeData
                    { inputPortViewDataKey = edge.input.viewDataKey, outputPortViewDataKey = edge.output.viewDataKey });
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