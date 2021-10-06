using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZDialoguer;
using ZDialoguer.Localization;

public class DialogueDirector : MonoBehaviour
{
    public List<ZDialogueGraph> graphs;
    public LocalisedString text;
    
    private void Start()
    {
        Print();
    }

    void Print()
    {
        graphs.ForEach(g => g.facts.ForEach(f => Debug.LogError(f.value)));
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Modify"))
        {
            graphs.FirstOrDefault().facts.FirstOrDefault().value = 10;
            Print();
        }
    }
}
