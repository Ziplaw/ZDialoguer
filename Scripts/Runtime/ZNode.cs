using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZGraph
{
    public abstract class ZNode : ScriptableObject
    {
        public Vector2 position;
        public string guid;
        public ZGraph graph;

        internal bool OnCreate(Vector2 position, ZGraph graph)
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

            graph.nodes.Add(this);

#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(this, graph);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
#endif

            return true;
        }

        protected virtual void OnCreate()
        {
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class OutputAttribute : Attribute
    {
        public readonly PortOptions PortOptions;

        public OutputAttribute(PortOptions portOptions)
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
    
    [AttributeUsage(AttributeTargets.Method)]
    public class InputAttribute : Attribute
    {
        public readonly PortOptions PortOptions;

        public InputAttribute(PortOptions portOptions)
        {
            PortOptions = portOptions;
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