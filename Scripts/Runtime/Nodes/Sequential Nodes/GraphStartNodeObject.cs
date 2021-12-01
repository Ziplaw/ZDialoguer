using System;
using System.Collections.Generic;
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

        public string GetNextText()
        {
            if (_current)
            {
                var tuple = _current.OnRetrieve();
                _current = tuple.Item2;
                if (tuple.Item1 == null) return null;
                return tuple.Item1.ParseFacts(graph);
            }
            return null;
        }
        public SequentialNodeObject Current
        {
            set => _current = value;
        }

        public override NodeObject DeepClone()
        {
            GraphStartNodeObject instance = (GraphStartNodeObject)graph.GetOrCreateNodeInstance(this);
            instance.childNodeObject = (SequentialNodeObject)graph.GetOrCreateNodeInstance(childNodeObject);
            instance._current = (SequentialNodeObject)graph.GetOrCreateNodeInstance(_current);
            return instance;
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
                    _text = _text.Replace(tag, graph.facts.Select(i => GlobalData.Instance.facts[i]).First(f => f.nameID == factName).Value.ToString());
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