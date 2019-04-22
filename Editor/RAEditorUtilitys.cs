using UnityEngine;


public class RAEditorUtilitys : MonoBehaviour
{


    public static void DrawLine()
    {
        GUILayout.Space(4f);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); //Horizontal bar
    }


    public static void Line(float _spacing = 4f, float _size = 1f)
    {
        GUILayout.Space(_spacing);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(_size)); //Horizontal bar
        GUILayout.Space(_spacing);
    }
}