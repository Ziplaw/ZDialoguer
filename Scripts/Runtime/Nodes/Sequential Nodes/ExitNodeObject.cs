using System;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZGraph.DialogueSystem
{
    [ZNodeDisplay("#9991F5","Exit")]
    public class ExitDialogueNodeObject : SequentialDialogueNodeObject
    {
        [Input(PortOptions.Multiple)]
        public ZNode Input(EdgeData edgeData)
        {
            var node = graph.GetNode(edgeData.outputPortViewDataKey);
            Debug.Log($"Input: {node}");
            return node;
        }
    }
}