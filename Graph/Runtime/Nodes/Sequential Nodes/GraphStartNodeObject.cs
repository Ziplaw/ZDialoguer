using System.Linq;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZDialoguer
{
    public class GraphStartNodeObject : SequencialNodeObject
    {
        public override SequencialNodeObject SequenceChild => childNodeObject;
        public SequencialNodeObject childNodeObject;
        [SerializeField] SequencialNodeObject _next;//
        
        public SequencialNodeObject Next
        {
            get
            {
                _next = _next.SequenceChild;
                return _next;
            }
            internal set => _next = value;
        }
    }

    public static class Extensions
    {
        public static string ParseFacts(this LocalisedString text, ZDialogueGraph graph)
        {
            string _text = text;
            int index = 0;
            while (_text.Contains("<"))
            {
                int start = _text.IndexOf('<');
                int end = _text.IndexOf('>');

                string tag = _text.Substring(start, end-start+1);
                string factName = tag.Replace("<","").Replace(">","");

                _text = _text.Replace(tag, graph.facts.First(f => f.nameID == factName).value.ToString());
            }

            return _text;
        }
    }
}