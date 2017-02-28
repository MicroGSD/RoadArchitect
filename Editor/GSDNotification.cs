using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Used for notifications in other areas of RA.
/// </summary>
public class GSDNotification : EditorWindow{
    string notification = "This is a Notification";
    static void Initialize() {
       	GSDNotification window = EditorWindow.GetWindow<GSDNotification>();
        window.Show();
    }
    
    void OnGUI() {
        notification = EditorGUILayout.TextField(notification);
        if(GUILayout.Button("Show Notification")){
            this.ShowNotification(new GUIContent(notification));
        }
        if(GUILayout.Button("Remove Notification")) {
            this.RemoveNotification();
        }
    }
}