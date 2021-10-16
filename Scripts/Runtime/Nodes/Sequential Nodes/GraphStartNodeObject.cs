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
        [SerializeField] SequentialNodeObject _next; //

        public string GetNextText(ZDialogueGraph _graph)
        {
            string dialogueText;
            
            var current = Next;
            while (!(current as DialogueNodeObject))
            {
                switch (current)
                {
                    case IEventNodeObject eventNodeObject:
                        eventNodeObject.Execute();
                        switch (eventNodeObject)
                        {
                            case ChoiceNodeObject choiceNodeObject:
                                Next = choiceNodeObject;
                                break;

                        }
                        break;
                    case null:
                        // Next = this;
                        return null;
                }

                current = Next;
            }

            Debug.Log((current as DialogueNodeObject).text.ParseFacts(_graph));
            return (current as DialogueNodeObject).text.ParseFacts(_graph);
        }

        public SequentialNodeObject Next
        {
            get
            {
                _next = _next != null ? _next.SequenceChild : null;
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