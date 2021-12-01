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
        public List<int> facts = new List<int>();
        public List<int> characters = new List<int>();
        public List<EdgeData> edgeDatas = new List<EdgeData>();
        public TextAsset dialogueText;


        public Dictionary<NodeObject, NodeObject> nodeObjectMap = new Dictionary<NodeObject, NodeObject>();

#if UNITY_EDITOR

        public void Init(Vector2 graphEntryPosition)
        {
            var start = CreateInstance<GraphStartNodeObject>();
            start.Init(graphEntryPosition, this);
            initialized = true;
        }

        public void DeleteNode(NodeObject nodeObject)
        {
            nodes.Remove(nodeObject);
            AssetDatabase.RemoveObjectFromAsset(nodeObject);
            AssetDatabase.SaveAssets();
        }

        public void DeleteFact(int factIndex)
        {
            facts.Remove(factIndex);
            AssetDatabase.SaveAssets();
        }

#endif
        [Serializable]
        public class EdgeData
        {
            public string outputPortViewDataKey;
            public string inputPortViewDataKey;
        }

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
                GlobalData.Instance.facts.First(f => f.nameID == factData.nameID).Value = factData.value;
            }
        }

        internal NodeObject GetOrCreateNodeInstance(NodeObject current)
        {
            if (current == null) return null;
            if (!nodeObjectMap.ContainsKey(current))
                nodeObjectMap[current] = Instantiate(current);
            return nodeObjectMap[current];
        }

        public void Dispose()
        {
            nodes.ForEach(n => Destroy(n));
            facts.Clear();
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