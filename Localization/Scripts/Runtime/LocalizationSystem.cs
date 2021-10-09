using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using LINQtoCSV;
using UnityEngine;

public class LocalizationSystem
{
    public static string GetString(string textAssetFullPath, int id)
    {
        return GetTable(textAssetFullPath)[id].entry[LocalizationSettings.Instance.selectedLanguage];
    }

#if UNITY_EDITOR
    public static void SetTable(string textAssetFullPath, List<TableEntry> table)
    {
        var localizationTableType = Assembly.Load("Assembly-CSharp").GetType("LocalizationTable");
        
        CsvFileDescription outputFileDescription = new CsvFileDescription
        {
            SeparatorChar = LocalizationSettings.Instance.separator,
            FirstLineHasColumnNames = true, 
        };

        var cc = new CsvContext();
        
        var list = localizationTableType.GetMethod("GenerateTable").Invoke(null, new object[] {table});
        var method = typeof(CsvContext)
            .GetMethods().First(m =>
                m.Name == "Write" &&
                m.GetParameters().Length == 3 /*&& m.GetParameters()[0].ParameterType == typeof(IEnumerable<>)*/ &&
                m.GetParameters()[1].ParameterType == typeof(string) &&
                m.GetParameters()[2].ParameterType == typeof(CsvFileDescription));

        method = method.MakeGenericMethod(new Type[] { localizationTableType });
        
        method.Invoke(cc, new object[] { list, textAssetFullPath, outputFileDescription });
        UnityEditor.AssetDatabase.Refresh();
    }
#endif

    internal static List<TableEntry> GetTable(string textAssetFullPath)
    {
        // string[] internalLineBreakSplits = csvFile.text.Split( new [] {$"{LocalizationSettings.Instance.separator}\"", $"\"{LocalizationSettings.Instance.separator}", "\n\"","\"\n"}, StringSplitOptions.None);

        CsvFileDescription inputFileDescription = new CsvFileDescription
        {
            SeparatorChar = LocalizationSettings.Instance.separator,
            FirstLineHasColumnNames = true
        };

        CsvContext cc = new CsvContext();

        // Debug.Log(localizationTableType);
        // object table = Activator.CreateInstance(typeof(IEnumerable<>).MakeGenericType(new Type[] { localizationTableType }));

        // Debug.Log(table);

        // List<LocalizationTable> table = cc.Read<LocalizationTable>(textAssetFullPath, inputFileDescription).ToList();

        var localizationTableType = Assembly.Load("Assembly-CSharp").GetType("LocalizationTable");
        
        return localizationTableType.GetMethod("GenerateConvertedTable").Invoke(null, new object[]{
            typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(localizationTableType).Invoke(
                null, new object[]
                {
                    typeof(CsvContext).GetMethod("Read", new []{typeof(string), typeof(CsvFileDescription)})
                        .MakeGenericMethod(new Type[] { localizationTableType })
                        .Invoke(cc, new object[] { textAssetFullPath, inputFileDescription })
                })}) as List<TableEntry>;

        // Debug.Log(table);

        // IEnumerable<DataTestClass> products =
        //     cc.Read<DataTestClass>(Path.Combine(Application.dataPath, "A.csv"), inputFileDescription);
        //
        // foreach (var dataTestClass in products)
        // {
        //     Debug.Log(dataTestClass.German);
        // }

        return null;

        // string[] csvData = csvFile.text.Split('\n');
        // TableEntry[] csvTable = new TableEntry[csvData.Length - 2];
        //
        // for (int i = 1; i < csvData.Length - 1; i++)
        // {
        //     string[] csvInfo = csvData[i].Split(LocalizationSettings.Instance.separator);
        //     csvTable[i - 1].entry = new string[LocalizationSettings.Instance.languages.Count];
        //
        //     for (int j = 0; j < csvInfo.Length; j++)
        //     {
        //         csvTable[i - 1].entry[j] = csvInfo[j];
        //     }
        // }
        //
        // return csvTable;
    }


    [Serializable]
    public struct TableEntry
    {
        public string[] entry;
    }
}