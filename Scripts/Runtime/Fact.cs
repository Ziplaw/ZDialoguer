using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZDialoguer
{
    [Serializable]
    public class Fact : DialogueData
    {
        public Fact()
        {
            nameID = "New Fact";
        }

        public Fact(Fact fact)
        {
            nameID = fact.nameID;
            factType = fact.factType;
            stringValue = fact.stringValue;
            floatValue = fact.floatValue;
        }

        public enum FactType
        {
            Float,
            String
        }

        // public Action<FactType> OnFactTypeChange;
        [SerializeField] public bool initialized;
        [SerializeField, HideInInspector] string stringValue = "New Fact";
        [SerializeField, HideInInspector] float floatValue = 0f;
        public FactType factType;

        public object Value
        {
            get
            {
                switch (factType)
                {
                    case FactType.Float: return floatValue;
                    case FactType.String: return stringValue;
                    default: throw new InvalidCastException("Specified value is neither string nor float");
                }
            }
            set
            {
                switch (value)
                {
                    case float _floatValue:
                        floatValue = _floatValue;
                        break;
                    case string _stringValue:
                        stringValue = _stringValue;
                        break;
                    default: throw new InvalidCastException("Specified value is neither string nor float");
                }
            }
        }

        public static Fact Null => _null ??= new Fact();
        private static Fact _null;
    }
}

