using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZDialoguer
{
    // [CreateAssetMenu]
    public class GlobalData : ScriptableObject
    {
        public static GlobalData Instance => _instance ??= Resources.Load<GlobalData>("Global Dialogue Data");
        private static GlobalData _instance;

        //[SerializeField]
        private List<Fact> copiedFacts = new List<Fact>();
        public List<Fact> facts = new List<Fact>();
        public List<Character> characters = new List<Character>();

#if UNITY_EDITOR
        public GlobalData()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    copiedFacts = facts.Select(f => new Fact(f)).ToList();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    for (var i = 0; i < facts.Count; i++)
                    {
                        facts[i].Value = copiedFacts[i].Value;
                    }
                    copiedFacts.Clear();
                    break;
            }
        }
        
        public void Create<T>(string newName) where T : new()
        {
            T obj = new T();
            switch (obj)
            {
                case Fact fact: 
                    facts.Add(fact);
                    fact.nameID = newName;
                    fact.Value = default(float);
                    fact.initialized = true;
                    break;
                case Character character: characters.Add(character);
                    break;
            }
            
            // AssetDatabase.AddObjectToAsset(obj, Instance);
            AssetDatabase.SaveAssets();
        }

        public void Delete<T>(T obj)
        {
            switch (obj)
            {
                case Fact fact : facts.Remove(fact);
                    break;
                case Character character: characters.Remove(character);
                    break;
            }
            
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}