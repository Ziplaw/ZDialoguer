using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
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
        [SerializeField] internal List<LocalizationSystem.TableEntry> table;
        [SerializeField] internal string output;
        [SerializeField] internal string csvFileFullAssetPath;

#if UNITY_EDITOR
        
        #endif
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
                if (textField.table == null || textField.table.Count == 0)
                {
                    textField.table = LocalizationSystem.GetTable(textField.csvFileFullAssetPath);
                }
                if (textField.table != null && textField.table.Count > 0 && textField.table.Count > textField.value && textField.table[textField.value].entry != null)
                {
                    textField.output = textField.table[textField.value].entry[LocalizationSettings.Instance.selectedLanguage];
                }
            }
            return textField.output;
        }

        public void Reset()
        {
            value = default;
            table = null;
            output = string.Empty;
        }

        public static string GetFullAssetPath(TextAsset textCsvFile)
        {
            return Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6),
                AssetDatabase.GetAssetPath(textCsvFile));
        }
    }

    
}