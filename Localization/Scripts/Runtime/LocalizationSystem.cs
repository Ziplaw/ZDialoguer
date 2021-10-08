using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LocalizationSystem
{
    public static string GetString(TextAsset csvFile, int id)
    {
        return GetTable(csvFile)[id].entry[LocalizationSettings.Instance.selectedLanguage];
    }

#if UNITY_EDITOR
    public static void SetTable(TextAsset csvFile, TableEntry[] table)
    {
        var newTable = table.ToList();
        newTable.InsertRange(0,
            new List<TableEntry>()
            {
                new TableEntry { entry = LocalizationSettings.Instance.languages.ToArray() }
            });

        string csvText = "";
        
        foreach (var tableEntry in newTable)
        {
            for (var i = 0; i < tableEntry.entry.Length; i++)
            {
                csvText += tableEntry.entry[i] + (i == tableEntry.entry.Length-1 ? "" : LocalizationSettings.Instance.separator.ToString());
            }
            
            foreach (var s in tableEntry.entry)
            {
            }

            csvText += "\n";
        }
        
        var path = UnityEditor.AssetDatabase.GetAssetPath(csvFile);
        File.WriteAllText(path, csvText);
        UnityEditor.AssetDatabase.Refresh();
    }
#endif
    
    internal static TableEntry[] GetTable(TextAsset csvFile)
    {
        if (csvFile)
        {
            string[] csvData = csvFile.text.Split('\n');
            TableEntry[] csvTable = new TableEntry[csvData.Length - 2];

            for (int i = 1; i < csvData.Length - 1; i++)
            {
                string[] csvInfo = csvData[i].Split(LocalizationSettings.Instance.separator);
                csvTable[i - 1].entry = new string[LocalizationSettings.Instance.languages.Count];

                for (int j = 0; j < csvInfo.Length; j++)
                {
                    csvTable[i - 1].entry[j] = csvInfo[j];
                }
            }
            return csvTable;
        }
        return null;
    }

    [Serializable]
    public struct TableEntry
    {
        public string[] entry;
    }
}
