using System;
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
        [SerializeField] float floatValue;
        [SerializeField] string stringValue;

        public object Value
        {
            get
            {
                switch (fact.factType)
                {
                    case Fact.FactType.Float: return floatValue;
                    case Fact.FactType.String: return stringValue;
                    default: throw new NotImplementedException();
                }
            }
            set
            {
                switch (fact.factType)
                {
                    case Fact.FactType.Float: floatValue = (float)value; break;
                    case Fact.FactType.String: stringValue = (string)value; break;
                    default: throw new NotImplementedException();
                }
            }
        }
        
        public NodeObject childIfTrue;
        public NodeObject childIfFalse;

        public override void Init(Vector2 _position, ZDialogueGraph graph)
        {
            base.Init(_position, graph);
        }


        public bool GetPredicate()
        {
            if (fact)
                switch (fact.factType)
                {
                    case Fact.FactType.Float:

                        float _valueFloat = (float)Value;
                        float _factValueFloat = (float)fact.Value;
                        
                        switch (operation)
                        {
                            case Operation.Equals:
                                return _factValueFloat == _valueFloat;
                            case Operation.Greater:
                                return _factValueFloat > _valueFloat;
                            case Operation.Lower:
                                return _factValueFloat < _valueFloat;
                            case Operation.GreaterEqual:
                                return _factValueFloat >= _valueFloat;
                            case Operation.LowerEqual:
                                return _factValueFloat <= _valueFloat;
                            case Operation.Not:
                                return _factValueFloat != _valueFloat;
                            default:
                                throw new NotImplementedException();
                        }
                    case Fact.FactType.String:
                        
                        switch (operation)
                        {
                            case Operation.Equals:
                                return (string)fact.Value == (string)Value;
                            case Operation.Not:
                                return (string)fact.Value != (string)Value;
                            default:
                                throw new NotImplementedException();
                        }
                }
                
                
            return true;
        }

        public override SequencialNodeObject SequenceChild => GetPredicate() ? (childIfTrue as SequencialNodeObject) : (childIfFalse as SequencialNodeObject);
    }
}