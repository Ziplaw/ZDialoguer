using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[Obsolete]
public class ExtendedEdgeManipulator : IEdgeConnectorListener
{
    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrop(GraphView graphView, Edge edge)
    {
        
    }
}
