using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
[assembly: InternalsVisibleTo("com.Ziplaw.ZDialoguer.Editor")]

namespace ZDialoguer
{
    public abstract class NodeObject : ScriptableObject
    {
        public Vector2 position;
        public string guid;
        protected ZDialogueGraph graph;

        #if UNITY_EDITOR
        public virtual void Init(Vector2 position, ZDialogueGraph graph)
        {
            this.graph = graph;
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
        public abstract SequencialNodeObject SequenceChild { get; }
    }
    
    public abstract class StaticNodeObject : NodeObject
    {
        
    }
    
}