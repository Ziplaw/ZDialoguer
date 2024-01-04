using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using ZDialoguer.Localization;

namespace ZGraph.DialogueSystem
{
    [ZNodeDisplay("#9991F5","Entry")]
    public class GraphStartDialogueNodeObject : UnnamedNode
    {
        [Output(PortOptions.Single)] int Out;
        
        public string GetNextText()
        {
            return null;
        }

        public override void Execute()
        {
            Out = 3;
        }
    }

    public static class Extensions
    {
        public static string ParseFacts(this LocalisedString text, DialogueGraph graph)
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