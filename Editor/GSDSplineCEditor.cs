#region "Imports"
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(GSDSplineC))] 
#endregion
public class GSDSplineCEditor : Editor {
	protected GSDSplineC tSpline { get { return (GSDSplineC) target; } }

    int browseNode = 0;
	public override void OnInspectorGUI(){
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Browse to node:", EditorStyles.boldLabel);
        browseNode = EditorGUILayout.IntField(browseNode);
        if (GUILayout.Button("Browse"))
        {
            if (browseNode < tSpline.mNodes.Count)
                Selection.objects = new Object[1] { tSpline.mNodes[browseNode] };
        }
        EditorGUILayout.EndHorizontal();
	}
}