using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Fact : ScriptableObject
{
    public enum FactType {Float, String}

    public Action<FactType> OnFactTypeChange;
    public string nameID;
    [SerializeField,HideInInspector] string stringValue = "New Fact";
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
                default: throw new InvalidCastException("Specified value is neither string nor float");
            }
        }
        set
        {
            switch (value)
            {
                case float _floatValue: floatValue = _floatValue; break;
                case string _stringValue: stringValue = _stringValue; break;
                default: throw new InvalidCastException("Specified value is neither string nor float");
            }
        }
    }

#if UNITY_EDITOR
    public void FactTypeChange(ChangeEvent<Enum> evt)
    {
        OnFactTypeChange((FactType)evt.newValue);
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
    
}

