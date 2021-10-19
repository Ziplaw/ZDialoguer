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

        public Dictionary<string, Fact> FactMap => _factMap ??= facts.ToDictionary(f => f.nameID, f => f);
        private Dictionary<string, Fact> _factMap;

        public Dictionary<NodeObject, NodeObject> nodeObjectMap = new Dictionary<NodeObject, NodeObject>();
        public Dictionary<Fact, Fact> factInstanceMap = new Dictionary<Fact, Fact>();

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

        // private void OnEnable()
        // {
        //     var entryNode = GetEntryNode();
        //     entryNode.Current = entryNode;
        // }

        public GraphStartNodeObject GetEntryNode()
        {
            return (GraphStartNodeObject)nodes[0];
        }

        public void Setup()
        {
            nodes.ForEach(n => n.graph = this);
        }

        public void InitializeFacts(FactData[] factDatas)
        {
            foreach (var factData in factDatas)
            {
                FactMap[factData.nameID].Value = factData.value;
            }
        }

        internal NodeObject GetOrCreateNodeInstance(NodeObject current)
        {
            if (current == null) return null;
            if (!nodeObjectMap.ContainsKey(current))
                nodeObjectMap[current] = Instantiate(current);
            return nodeObjectMap[current];
        }
        
        internal Fact GetOrCreateFactInstance(Fact current)
        {
            if (current == null) return null;
            if (!factInstanceMap.ContainsKey(current))
                factInstanceMap[current] = Instantiate(current);
            return factInstanceMap[current];
        }

        public void Dispose()
        {
            nodes.ForEach(n => Destroy(n));
            facts.ForEach(n => Destroy(n));
            Destroy(this);
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