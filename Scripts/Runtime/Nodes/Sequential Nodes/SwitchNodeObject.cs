using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZDialoguer
{
    public class SwitchNodeObject : SequentialNodeObject
    {
        public override SequentialNodeObject SequenceChild
        {
            get
            {
                if (!fact) return null;
                switch (fact.factType)
                {
                    case Fact.FactType.Float:
                        return outputEntries.FirstOrDefault(o => o.floatValue == (float)fact.Value)?.output as
                            SequentialNodeObject;
                    case Fact.FactType.String:
                        return outputEntries.FirstOrDefault(o => o.stringValue == (string)fact.Value)?.output as
                            SequentialNodeObject;
                    default: return null;
                }
            }
        }

        public Fact fact;
        [SerializeField] internal List<OutputEntry> outputEntries = new List<OutputEntry>();

        [Serializable]
        public class OutputEntry
        {
            public float floatValue;
            public string stringValue = "New Entry";
            public NodeObject output;

            public void SetValue(object value, Fact.FactType factFactType)
            {
                switch (factFactType)
                {
                    case Fact.FactType.Float:
                        floatValue = (float)value;
                        break;
                    case Fact.FactType.String:
                        stringValue = (string)value;
                        break;
                }
            }

            public object GetValue(Fact.FactType factFactType)
            {
                switch (factFactType)
                {
                    case Fact.FactType.Float: return floatValue;
                    case Fact.FactType.String: return stringValue;
                    default: return null;
                }
            }
        }

        public object GetValue(out int position)
        {
            if (fact)
            {
                var value = outputEntries.FirstOrDefault(e => e.GetValue(fact.factType).Equals(fact.Value));
                position = outputEntries.IndexOf(value);
                    
                return value?.GetValue(fact.factType);
            }

            position = -1;
            return null;
        }

        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            return SequenceChild.OnRetrieve();
        }
    }
}