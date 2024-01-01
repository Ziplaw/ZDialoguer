using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using ZDialoguer.Localization;

namespace ZGraph.DialogueSystem
{
    [ZNodeDisplay("#9991F5","Entry")]
    public class GraphStartDialogueNodeObject : DialogueNode
    {
        [Output(PortOptions.Single)]
        public ZNode Out(EdgeData edgeData)
        {
            var node = graph.GetNode(edgeData.inputPortViewDataKey);
            Debug.Log($"Out: {node}");
            return node;
        }
        public string GetNextText()
        {
            return null;
        }
    }

    public static class Extensions
    {
        public static string ParseFacts(this LocalisedString text, ZDialogueGraph graph)
        {
            string _text = text;
            // int index = 0;
            while (_text.Contains("<") /*&& index < 1000*/)
            {
                int start = _text.IndexOf('<');
                int end = _text.IndexOf('>');

                string tag = _text.Substring(start, end - start + 1);
                string factName = tag.Replace("<", "").Replace(">", "");
                
                try
                {
                    _text = _text.Replace(tag, graph.localFacts.First(f => f.nameID == factName).Value.ToString());
                }
                catch (InvalidOperationException)
                {
                    Debug.LogWarning(
                        $"You defined a Fact Tag ({tag}) in your Dialogue Text that doesn't exist in the current Dialogue Graph");
                    break;
                }
            }

            return _text;
        }
    }
}