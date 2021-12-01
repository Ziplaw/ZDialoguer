using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZDialoguer
{


    public class FactBlackboardField : BlackboardField
    {
        public int factIndex;
        public Action<Fact> OnBlackboardFactSelected;

        public FactBlackboardField(int factIndex)
        {
            this.factIndex = factIndex;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnBlackboardFactSelected?.Invoke(GlobalData.Instance.facts[factIndex]);
        }
    }
}