using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
[ExecuteInEditMode]
public class GSDSplineRepair : MonoBehaviour {

	public void RemoveLastNode()
    {
        GSDSplineC spline = GetComponent<GSDSplineC>();
        spline.mNodes.RemoveAt(spline.mNodes.Count - 1);
        spline.tRoad.UpdateRoad();
    }
}


[CustomEditor(typeof(GSDSplineRepair))]
public class GSDSplineRepairEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GSDSplineRepair repair = (GSDSplineRepair)target;
        if (GUILayout.Button("Remove last node"))
        {
            repair.RemoveLastNode();
        }
    }
}
#endif