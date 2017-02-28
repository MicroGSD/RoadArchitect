#region "Imports"
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(GSDSplineF))] 
#endregion
public class GSDSplineFEditor : Editor {
	protected GSDSplineF tSpline { get { return (GSDSplineF) target; } }
	
	public override void OnInspectorGUI(){
		//Intentionally left empty.
	}
}