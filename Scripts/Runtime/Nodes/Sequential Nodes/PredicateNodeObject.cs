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

        public Fact fact;
        public Operation operation;
        [SerializeField] float floatValue;
        [SerializeField] string stringValue;
        [SerializeField] internal Fact secondFact;

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
                    case Fact.FactType.Float:
                        floatValue = (float)value;
                        break;
                    case Fact.FactType.String:
                        stringValue = (string)value;
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }

        public NodeObject childIfTrue;
        public NodeObject childIfFalse;

        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            return SequenceChild.OnRetrieve();
        }

        public bool GetPredicate()
        {
            if (fact)
            {
                if (secondFact)
                {
                    if (secondFact.factType != fact.factType)
                        throw new ArgumentException($"{fact.nameID} and {secondFact.nameID} are not of the same type");

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
                default: throw new NotImplementedException();
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
                default: throw new NotImplementedException();
            }
        }

        public override SequentialNodeObject SequenceChild => GetPredicate()
            ? childIfTrue as SequentialNodeObject
            : childIfFalse as SequentialNodeObject;
    }
}