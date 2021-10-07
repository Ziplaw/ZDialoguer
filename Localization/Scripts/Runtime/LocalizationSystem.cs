using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationSystem
{
    public static string GetString(TextAsset csvFile, int id)
    {
        return GetTable(csvFile)[id].entry[LocalizationSettings.Instance.selectedLanguage];
    }
    internal static TableEntry[] GetTable(TextAsset csvFile)
    {
        // if (Application.isPlaying)
        // {
        //     Debug.LogWarning("Localization tables shouldn't be generated at runtime! Generate them on the editor first.");
        //     return null;
        // }

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
