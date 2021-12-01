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
                if (FactInstance == null) return null;
                switch (FactInstance.factType)
                {
                    case Fact.FactType.Float:
                        return outputEntries.FirstOrDefault(o => o.floatValue == (float)FactInstance.Value)?.output as
                            SequentialNodeObject;
                    case Fact.FactType.String:
                        return outputEntries.FirstOrDefault(o => o.stringValue == (string)FactInstance.Value)?.output as
                            SequentialNodeObject;
                    default: return null;
                }
            }
        }

        public Fact FactInstance => factIndex == -1? Fact.Null : GlobalData.Instance.facts[factIndex];

        public int factIndex = -1;
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
            if (FactInstance != null)
            {
                var value = outputEntries.FirstOrDefault(e => e.GetValue(FactInstance.factType).Equals(FactInstance.Value));
                position = outputEntries.IndexOf(value);
                    
                return value?.GetValue(FactInstance.factType);
            }

            position = -1;
            return null;
        }

        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            return SequenceChild.OnRetrieve();
        }

        public override NodeObject DeepClone()
        {
            SwitchNodeObject instance = (SwitchNodeObject)graph.GetOrCreateNodeInstance(this);
            instance.outputEntries.ForEach(outputEntry => outputEntry.output = graph.GetOrCreateNodeInstance(outputEntry.output));
            return instance;
        }
    }
}