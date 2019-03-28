#if UNITY_EDITOR
#region "Imports"
//using UnityEngine;                // Unused
using UnityEditor;
#endregion


[CustomEditor(typeof(GSDSplineI))]
public class GSDSplineIEditor : Editor
{
    protected GSDSplineI tSpline { get { return (GSDSplineI) target; } }


    public override void OnInspectorGUI()
    {
        //Intentionally left empty.
    }
}
#endif