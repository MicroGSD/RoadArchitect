#region "Imports"
//using System.Collections;                                 // Unused
//using System.Collections.Generic;                 // Unused
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion


[ExecuteInEditMode]
public class GSDRoadConnector : MonoBehaviour
{
    public GSDSplineN connectedNode;
    [HideInInspector]
    public GSDOffRoadObject obj { get { return transform.parent.GetComponent<GSDOffRoadObject>(); } }


#if UNITY_EDITOR
    #region "Gizmos"
    private void OnDrawGizmos()
    {
        Gizmos.color = GSDOffRoadObject.Color_NodeOffRoadColor;
        Gizmos.DrawCube(transform.position + new Vector3(0f, 6f, 0f), new Vector3(2f, 11f, 2f));
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = GSDOffRoadObject.Color_NodeOffRoadSelectedColor;
        Gizmos.DrawCube(transform.position + new Vector3(0f, 6.25f, 0f), new Vector3(3.5f, 12.5f, 3.5f));
    }
    #endregion


    public void ConnectToNode(GSDSplineN node)
    {
        Debug.Log("Would connect to " + node);
        connectedNode = node;
        connectedNode.transform.position = transform.position;
        connectedNode.GSDSpline.tRoad.UpdateRoad();
    }


    // Update is called once per frame
    private void Update()
    {
        if (connectedNode != null)
        {
            if (obj == null)
            {
                Debug.LogError("Parent should have GSDOffRoadObject component attached");
            }
            if (connectedNode.transform.position != transform.position)
            {
                connectedNode.transform.position = transform.position;
                connectedNode.GSDSpline.tRoad.UpdateRoad();
            }
        }
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(GSDRoadConnector))]
public class GSDRoadConnectorEditor : Editor
{
    public GSDRoadConnector tConnector { get { return (GSDRoadConnector) target; } }


    public override void OnInspectorGUI()
    {
        if (tConnector.connectedNode != null)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Off-road connection:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(tConnector.connectedNode.GSDSpline.tRoad.name + " to " + tConnector.obj.name);
            if (GUILayout.Button("Break connection"))
            {
                tConnector.connectedNode = null;
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif