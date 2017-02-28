#region "Imports"
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(GSDSplineI))] 
#endregion
public class GSDSplineIEditor : Editor {
	protected GSDSplineI tSpline { get { return (GSDSplineI) target; } }
	
	public override void OnInspectorGUI(){
		//Intentionally left empty.
	}
}