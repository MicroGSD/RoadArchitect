#if UNITY_EDITOR
#region "Imports"
//using UnityEngine;                    // Unused
using UnityEditor;
#endregion


[CustomEditor(typeof(GSDSplineF))]
public class GSDSplineFEditor : Editor
{
    protected GSDSplineF thisSpline { get { return (GSDSplineF) target; } }


    public override void OnInspectorGUI()
    {
        //Intentionally left empty.
    }
}
#endif