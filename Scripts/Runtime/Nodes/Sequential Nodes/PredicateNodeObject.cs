using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZDialoguer;
using ZDialoguer.Localization;


namespace ZDialoguer
{
    public class PredicateNodeObject : SequentialNodeObject
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

        public int factIndex = -1;
        public Operation operation;
        [SerializeField] float floatValue;
        [SerializeField] string stringValue;
        [SerializeField] internal int secondFactIndex = -1;

        public object Value
        {
            get
            {
                switch (GlobalData.Instance.facts[factIndex].factType)
                {
                    case Fact.FactType.Float: return floatValue;
                    case Fact.FactType.String: return stringValue;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (GlobalData.Instance.facts[factIndex].factType)
                {
                    case Fact.FactType.Float:
                        floatValue = (float)value;
                        break;
                    case Fact.FactType.String:
                        stringValue = (string)value;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public NodeObject childIfTrue;
        public NodeObject childIfFalse;

        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            return SequenceChild.OnRetrieve();
        }

        bool errorPrinted = false;

        public bool GetPredicate()
        {
            var fact = factIndex == -1 ? Fact.Null : GlobalData.Instance.facts[factIndex];
            var secondFact = secondFactIndex == -1 ? Fact.Null :GlobalData.Instance.facts[secondFactIndex];
            
            if (fact.initialized)
            {
                if (secondFact.initialized)
                {
                    if (secondFact.factType != fact.factType)
                    {
                        if(errorPrinted) return false;
                        
                        errorPrinted = true;
                        throw new ArgumentException($"{fact.nameID} and {secondFact.nameID} are not of the same type");
                    }

                    errorPrinted = false;

                    switch (fact.factType)
                    {
                        case Fact.FactType.Float: return MatchValues((float)fact.Value, (float)secondFact.Value);
                        case Fact.FactType.String: return MatchValues((string)fact.Value, (string)secondFact.Value);
                    }
                }
                else
                {
                    switch (fact.factType)
                    {
                        case Fact.FactType.Float: return MatchValues((float)fact.Value, (float)Value);
                        case Fact.FactType.String: return MatchValues((string)fact.Value, (string)Value);
                    }
                }
            }
            return true;
        }

        bool MatchValues(string value1, string value2)
        {
            switch (operation)
            {
                case Operation.Equals: return value1 == value2;
                case Operation.Not: return value1 != value2;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        bool MatchValues(float value1, float value2)
        {
            switch (operation)
            {
                case Operation.Equals: return Mathf.Approximately(value1, value2);
                case Operation.Greater: return value1 > value2;
                case Operation.Lower: return value1 < value2;
                case Operation.GreaterEqual: return value1 >= value2;
                case Operation.LowerEqual: return value1 <= value2;
                case Operation.Not: return !Mathf.Approximately(value1, value2);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override SequentialNodeObject SequenceChild => GetPredicate()
            ? childIfTrue as SequentialNodeObject
            : childIfFalse as SequentialNodeObject;

        public override NodeObject DeepClone()
        {
            PredicateNodeObject instance = (PredicateNodeObject)graph.GetOrCreateNodeInstance(this);
            instance.factIndex = factIndex;
            instance.secondFactIndex = secondFactIndex;
            instance.childIfTrue = graph.GetOrCreateNodeInstance(childIfTrue);
            instance.childIfFalse = graph.GetOrCreateNodeInstance(childIfFalse);
            return instance;
        }
    }
}