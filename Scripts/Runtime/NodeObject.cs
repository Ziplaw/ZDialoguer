using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace ZDialoguer
{
    public abstract class NodeObject : ScriptableObject
    {
        public Vector2 position;
        public string guid;

        #if UNITY_EDITOR
        
        protected void Init(Vector2 position, ZDialogueGraph graph)
        {
            this.position = position;
            name = GetType().Name;
            guid = GUID.Generate().ToString();
            graph.nodes.Add(this);
            
            AssetDatabase.AddObjectToAsset(this, graph);
            AssetDatabase.SaveAssets();
        }

#endif
    }
    
    public abstract class SequencialNodeObject : NodeObject
    {
        public abstract NodeObject SequenceChild { get; }
    }
    
    public abstract class StaticNodeObject : NodeObject
    {
        
    }
    
}
