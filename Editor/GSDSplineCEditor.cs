#if UNITY_EDITOR
#region "Imports"
using UnityEngine;
using UnityEditor;
#endregion


[CustomEditor(typeof(GSDSplineC))]
public class GSDSplineCEditor : Editor
{
    protected GSDSplineC thisSpline { get { return (GSDSplineC) target; } }
    private int browseNode = 0;


    public override void OnInspectorGUI()
    {
        #region NodeBrowser
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Browse to node:", EditorStyles.boldLabel);
        browseNode = EditorGUILayout.IntField(browseNode);
        if (GUILayout.Button("Browse"))
        {
            if (browseNode < thisSpline.mNodes.Count)
            {
                Selection.objects = new Object[1] { thisSpline.mNodes[browseNode] };
            }
        }
        EditorGUILayout.EndHorizontal();
        #endregion
    }
}
#endif