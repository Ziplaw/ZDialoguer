using System;
using System.Linq;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZDialoguer
{
    public class GraphStartNodeObject : SequentialNodeObject
    {
        public override SequentialNodeObject SequenceChild => childNodeObject;
        public SequentialNodeObject childNodeObject;
        [SerializeField] SequentialNodeObject _current;
        
        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            return SequenceChild.OnRetrieve();
        }

        public string GetNextText(ZDialogueGraph _graph)
        {
            if (_current)
            {
                var tuple = _current.OnRetrieve();
                _current = tuple.Item2;
                return tuple.Item1.ParseFacts(graph);
            }
            return null;
        }

        public SequentialNodeObject Next
        {
            get
            {
                _current = _current.SequenceChild;
                return _current;
            }
            internal set => _current = value;
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
                    _text = _text.Replace(tag, graph.facts.First(f => f.nameID == factName).Value.ToString());
                }
                catch (InvalidOperationException)
                {
                    Debug.LogWarning(
                        $"You defined a Fact Tag ({tag}) in your Dialogue Text that doesn't exist in the current Dialogue Graph");
                    break;
                }

                // index++;
                // if (index == 1000)
                // {
                //     Debug.LogError("Whew");
                // }
            }

            return _text;
        }
    }
}