using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGraph.DialogueSystem
{
    public class FactDialogueNode : PureDialogueNode
    {
        [FormerlySerializedAs("factIndex")] public Fact fact;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            DialogueGraph dialogueGraph = graph as DialogueGraph;
            
            if (dialogueGraph.localFacts.Count > 0)
            {
                fact = dialogueGraph.localFacts[0];
            }
            throw new NodeNotCreatedException("There's no Facts in the current graph!");
        }
    }
}