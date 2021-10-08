using System;
using System.Runtime.CompilerServices;
using UnityEngine;
[assembly: InternalsVisibleTo("com.Ziplaw.ZDialoguer.Editor")]

namespace ZDialoguer.Localization
{
    [Serializable]
    public class LocalisedString
    {
        public int value;
        public TextAsset csvFile;
        [SerializeField, HideInInspector] private bool csvDictatedByDialogueGraph;
        [SerializeField] internal LocalizationSystem.TableEntry[] table;
        [SerializeField] internal string output;

        public LocalisedString(bool csvDictatedByDialogueGraph = false)
        {
            this.csvDictatedByDialogueGraph = csvDictatedByDialogueGraph;
            value = default;
            csvFile = default;
            table = default;
            output = default;
        }

        public static implicit operator string(LocalisedString textField)
        {
            if (string.IsNullOrEmpty(textField.output))
            {
                if (textField.table == null || textField.table.Length == 0)
                {
                    textField.table = LocalizationSystem.GetTable(textField.csvFile);
                }
                if (textField.table != null && textField.table.Length > 0 && textField.table.Length > textField.value && textField.table[textField.value].entry != null)
                {
                    textField.output = textField.table[textField.value].entry[LocalizationSettings.Instance.selectedLanguage];
                }
            }
            return textField.output;
        }

        public void Reset()
        {
            table = null;
            output = string.Empty;
        }
    }

    
}