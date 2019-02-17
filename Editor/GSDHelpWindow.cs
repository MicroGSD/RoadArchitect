#region "Imports"
using UnityEngine;
using UnityEditor;
#endregion


public class GSDHelpWindow : EditorWindow
{
    private void OnGUI()
    {
        EditorStyles.label.wordWrap = true;
        EditorStyles.miniLabel.wordWrap = true;

        EditorGUILayout.LabelField("Road Architect Help", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Please visit the online manual for help.");
        GUILayout.Space(4f);

        if (GUILayout.Button("Click here to open online manual", EditorStyles.miniButton, GUILayout.Width(300f)))
        {
            Application.OpenURL("https://github.com/MicroGSD/RoadArchitect/wiki");
        }

        EditorGUILayout.LabelField("https://github.com/MicroGSD/RoadArchitect/wiki", EditorStyles.miniLabel);
        GUILayout.Space(4f);
        EditorGUILayout.LabelField("Please visit the online manual for help.", EditorStyles.miniLabel);
        GUILayout.Space(4f);
        EditorGUILayout.LabelField("Please visit our unity forum thread or contact support@microgsd.com with any questions or comments.", EditorStyles.miniLabel);
        // Maybe remove the line above by this? "Please visit our Github with any questions or comments"
    }


    #region "Init"
    public void Initialize()
    {
        Rect fRect = new Rect(340, 170, 420, 180);
        position = fRect;
        Show();
        titleContent.text = "Help Info";
    }
    #endregion
}