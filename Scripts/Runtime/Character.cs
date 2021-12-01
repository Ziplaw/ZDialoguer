using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZDialoguer
{
    [Serializable]
    public class Character : DialogueData
    {
        [Serializable]
        public struct Name
        {
            private Fact _factName;
            private string _strName;

            public string Value
            {
                get => _factName != null ? _factName.Value.ToString() : _strName;
                set
                {
                    if (_factName != null) _factName.Value = value;
                    else _strName = value;
                }
            }
        }

        public List<Texture2D> characterSprites = new List<Texture2D>();
        public List<Name> nameList = new List<Name>();
    }
}