#if UNITY_EDITOR
#region "Imports"
//using UnityEngine;                // Unused
using UnityEditor;
#endregion


[CustomEditor(typeof(GSDSplineI))]
public class GSDSplineIEditor : Editor
{
    protected GSDSplineI thisSpline { get { return (GSDSplineI) target; } }


    public override void OnInspectorGUI()
    {
        //Intentionally left empty.
    }
}
#endif