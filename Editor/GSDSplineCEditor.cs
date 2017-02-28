#region "Imports"
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(GSDSplineC))] 
#endregion
public class GSDSplineCEditor : Editor {
	protected GSDSplineC tSpline { get { return (GSDSplineC) target; } }
	
	public override void OnInspectorGUI(){
		
	}
}