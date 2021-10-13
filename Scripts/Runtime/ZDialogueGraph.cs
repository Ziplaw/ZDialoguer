using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZDialoguer;

namespace ZDialoguer
{
    [CreateAssetMenu]
    public class ZDialogueGraph : ScriptableObject
    {
#if UNITY_EDITOR
        public bool debugMode;
#endif
        public bool initialized;
        public List<NodeObject> nodes = new List<NodeObject>();
        public List<Fact> facts = new List<Fact>();
        public List<EdgeData> edgeDatas = new List<EdgeData>();
        public TextAsset dialogueText;
#if UNITY_EDITOR


        public void Init(Vector2 graphEntryPosition)
        {
            var start = CreateInstance<GraphStartNodeObject>();
            start.Init(graphEntryPosition, this);
            initialized = true;
        }

        public Fact CreateFact(string nameID)
        {
            Fact fact = CreateInstance<Fact>();
            fact.name = nameID;
            fact.nameID = nameID;
            fact.Value = default(float);
            facts.Add(fact);
            AssetDatabase.AddObjectToAsset(fact, this);
            AssetDatabase.SaveAssets();

            return fact;
        }

        public void DeleteNode(NodeObject nodeObject)
        {
            nodes.Remove(nodeObject);
            AssetDatabase.RemoveObjectFromAsset(nodeObject);
            AssetDatabase.SaveAssets();
        }

        public void DeleteFact(Fact fact)
        {
            facts.Remove(fact);
            AssetDatabase.RemoveObjectFromAsset(fact);
            AssetDatabase.SaveAssets();
        }
#endif
        [Serializable]
        public class EdgeData
        {
            public string outputPortViewDataKey;
            public string inputPortViewDataKey;
        }

        private void OnEnable()
        {
            var entryNode = GetEntryNode();
            entryNode.Next = entryNode;
        }

        public GraphStartNodeObject GetEntryNode()
        {
            return (GraphStartNodeObject)nodes[0];
        }
    }
}

public static class Extensions
{
    public static void RemoveEdge(this List<ZDialogueGraph.EdgeData> list, Edge edge)
    {
        list.Remove(list.FirstOrDefault(e =>
            e.inputPortViewDataKey == edge.input.viewDataKey && e.outputPortViewDataKey == edge.output.viewDataKey));
    }

    public static void AddEdge(this List<ZDialogueGraph.EdgeData> list, Edge edge)
    {
        if (!list.Any(e =>
            e.inputPortViewDataKey == edge.input.viewDataKey && e.outputPortViewDataKey == edge.output.viewDataKey))
            list.Add(new ZDialogueGraph.EdgeData
                { inputPortViewDataKey = edge.input.viewDataKey, outputPortViewDataKey = edge.output.viewDataKey });
    }
}