using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using ZDialoguer.Localization;

[assembly: InternalsVisibleTo("com.Ziplaw.ZDialoguer.Editor")]

namespace ZDialoguer
{
    public abstract class NodeObject : ScriptableObject
    {
        public Vector2 position;
        public string guid;
        [SerializeField]protected ZDialogueGraph graph;


        #if UNITY_EDITOR
        public virtual bool Init(Vector2 position, ZDialogueGraph graph)
        {
            this.graph = graph;
            this.position = position;
            name = GetType().Name;
            guid = GUID.Generate().ToString();
            graph.nodes.Add(this);
            
            AssetDatabase.AddObjectToAsset(this, graph);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();

            return true;
        }

#endif
    }
    
    public abstract class SequentialNodeObject : NodeObject
    {
        public abstract SequentialNodeObject SequenceChild { get; }
        public abstract (LocalisedString, SequentialNodeObject) OnRetrieve();

    }
    
    public abstract class StaticNodeObject : NodeObject
    {
        
    }
    
}
