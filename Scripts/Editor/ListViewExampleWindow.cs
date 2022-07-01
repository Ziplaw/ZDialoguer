using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ListViewExampleWindow : EditorWindow
{
    [MenuItem("Window/ListViewExampleWindow")]
    public static void OpenDemoManual()
    {
        GetWindow<ListViewExampleWindow>().Show();
    }

    public void OnEnable()
    {
        
    }
}