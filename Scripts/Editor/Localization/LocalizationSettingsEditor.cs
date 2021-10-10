using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationSettings))]
public class LocalizationSettingsEditor : Editor
{
    private LocalizationSettings manager;

    private void OnEnable()
    {
        manager = target as LocalizationSettings;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Localization Table"))
        {
            string path = Path.Combine(Application.dataPath, "ZResources/ZDialoguer/LocalizationTable.cs");
            if(!Directory.Exists(Path.Combine(Application.dataPath, "ZResources/ZDialoguer")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "ZResources/ZDialoguer"));
            }
    
            string script = "";
            script += 
@"using LINQtoCSV;
using System;
using System.Collections;
using System.Collections.Generic;

public class LocalizationTable 
{
";
            for (var i = 0; i < manager.languages.Count; i++)
            {
                var language = manager.languages[i];
                script += $"    [CsvColumn(Name = \"{language}\", FieldIndex = {i+1})] public string {language}" + " { get; set; }\n";
            }

            script += @"
    public static List<LocalizationSystem.TableEntry> GenerateConvertedTable(List<LocalizationTable> table) 
    {
        List<LocalizationSystem.TableEntry> tableConverted = new List<LocalizationSystem.TableEntry>();
        
        foreach (var tableEntry in table)
        {
            string[] entries = { ";
            
            foreach (var managerLanguage in manager.languages)
            {
                script += $"tableEntry.{managerLanguage}, ";
            }

            script += @"};
            
            tableConverted.Add(new LocalizationSystem.TableEntry {entry = entries});
        }

        return tableConverted;
    }


    public static List<LocalizationTable> GenerateTable(List<LocalizationSystem.TableEntry> table) 
    {
        List<LocalizationTable> tableConverted = new List<LocalizationTable>();

        foreach (var tableEntry in table)
        {
            tableConverted.Add(new LocalizationTable {  ";
            /*ID = tableEntry.entry[0], Spanish = tableEntry.entry[1] ... */
            for (var i = 0; i < manager.languages.Count; i++)
            {
                var managerLanguage = manager.languages[i];
                script += $"{managerLanguage} = tableEntry.entry[{i}], ";
            }

            script +=@"});
        }

        return tableConverted;
    }
";


            script += "}";
            
            File.WriteAllText(path, script);
            AssetDatabase.Refresh();
        }
    }
}
