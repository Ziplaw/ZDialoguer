using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Fact : ScriptableObject
{
    public enum FactType {Float, String}

    public Action<FactType> OnFactTypeChange;
    public string nameID;
    [SerializeField,HideInInspector] string stringValue = "";
    [SerializeField,HideInInspector] float floatValue = 0f;
    public FactType factType;

    public object Value
    {
        get
        {
            switch (factType)
            {
                case FactType.Float: return floatValue;
                case FactType.String: return stringValue;
                default: throw new NotImplementedException();
            }
        }
        set
        {
            switch (factType)
            {
                case FactType.Float: floatValue = (float)value; break;
                case FactType.String: stringValue = (string)value; break;
                default: throw new NotImplementedException();
            }
        }
    }

    public void FactTypeChange(ChangeEvent<Enum> evt) => OnFactTypeChange((FactType)evt.newValue);
}

