using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZDialoguer;


namespace ZDialoguer
{
    public class PredicateNodeObject : SequencialNodeObject
    {
        public enum Operation
        {
            Equals,
            Greater,
            Lower,
            GreaterEqual,
            LowerEqual,
            Not
        };

        public Fact fact;
        public Operation operation;
        public float value;
        
        public NodeObject childIfTrue;
        public NodeObject childIfFalse;

        public override void Init(Vector2 _position, ZDialogueGraph graph)
        {
            base.Init(_position, graph);
        }


        public bool GetPredicate()
        {
            if (fact)
                switch (operation)
                {
                    case Operation.Equals:
                        return fact.value == value;
                    case Operation.Greater:
                        return fact.value > value;
                    case Operation.Lower:
                        return fact.value < value;
                    case Operation.GreaterEqual:
                        return fact.value >= value;
                    case Operation.LowerEqual:
                        return fact.value <= value;
                    case Operation.Not:
                        return fact.value != value;
                    default:
                        return false;
                }
            return true;
        }

        public override SequencialNodeObject SequenceChild => GetPredicate() ? (childIfTrue as SequencialNodeObject) : (childIfFalse as SequencialNodeObject);
    }
}