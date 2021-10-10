using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FactBlackboardField : BlackboardField
{
    public Fact fact;
    public Action<Fact> OnBlackboardFactSelected; 
    
    public FactBlackboardField(Fact fact)
    {
        this.fact = fact;
    }

    public override void OnSelected()
    {
        base.OnSelected();
        OnBlackboardFactSelected?.Invoke(fact);
    }
}
