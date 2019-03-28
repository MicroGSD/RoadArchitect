#if UNITY_EDITOR
#region "Imports"
using UnityEngine;
using UnityEditor;
//using System.Collections;               // Unused
#endregion


/// <summary>
/// Used for notifications in other areas of RA.
/// </summary>
public class GSDNotification : EditorWindow
{
    private string notification = "This is a Notification";


    private static void Initialize()
    {
        GSDNotification window = EditorWindow.GetWindow<GSDNotification>();
        window.Show();
    }


    private void OnGUI()
    {
        notification = EditorGUILayout.TextField(notification);
        if (GUILayout.Button("Show Notification"))
        {
            this.ShowNotification(new GUIContent(notification));
        }
        if (GUILayout.Button("Remove Notification"))
        {
            this.RemoveNotification();
        }
    }
}
#endif