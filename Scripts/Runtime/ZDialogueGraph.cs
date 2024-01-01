using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZGraph.DialogueSystem
{
    [CreateAssetMenu]
    public class ZDialogueGraph : ZGraph
    {
        internal override string GraphTypeName => "Dialogue Graph";

        public List<Fact> localFacts = new List<Fact>();
        public List<int> characters = new List<int>();
        public TextAsset dialogueText;
        
        public override void OnPopulate(Vector2 size)
        {
            if(!GetEntryNode()) CreateNode<GraphStartDialogueNodeObject>(Vector2.zero);
        }

        public GraphStartDialogueNodeObject GetEntryNode()
        {
            return nodes.Count > 0 ? (GraphStartDialogueNodeObject)nodes[0] : null;
        }

        public void InitializeFacts(FactData[] factDatas)
        {
            foreach (var factData in factDatas)
            {
                GlobalData.Instance.facts.First(f => f.nameID == factData.nameID).Value = factData.value;
            }
        }
    }
}