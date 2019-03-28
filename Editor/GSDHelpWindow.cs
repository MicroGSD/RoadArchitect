#if UNITY_EDITOR
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

        if (GUILayout.Button("Click here to open online manual", EditorStyles.toolbarButton, GUILayout.Width(310f)))
        {
            Application.OpenURL("https://github.com/MicroGSD/RoadArchitect/wiki");
        }

        EditorGUILayout.LabelField("https://github.com/MicroGSD/RoadArchitect/wiki");
        GUILayout.Space(12f);
        EditorGUILayout.LabelField("Please visit the online manual for help.");
        GUILayout.Space(12f);
        EditorGUILayout.LabelField("Please visit our unity forum thread or reach out to us on Github (links below) with any questions or comments.");     
        // or contact support@microgsd.com // Since I do not know if this Email even exists anymore, I'll removed it // FH 20.02.19

        EditorGUILayout.LabelField("You can also check out the following Sites, for an Beta of RoadArchitect:");
        GUILayout.Space(4f);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("embeddedt's RoadArchitect Repository", EditorStyles.toolbarButton, GUILayout.Width(310f)))
        {
            Application.OpenURL("https://github.com/embeddedt/RoadArchitect/tree/master");
        }
        EditorGUILayout.LabelField("https://github.com/embeddedt/RoadArchitect/tree/master");

        EditorGUILayout.EndVertical();

        //GUILayout.Space(4f);

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("FritzsHero's RoadArchitect Repository", EditorStyles.toolbarButton, GUILayout.Width(310f)))
        {
            Application.OpenURL("https://github.com/FritzsHero/RoadArchitect/tree/master");
        }
        EditorGUILayout.LabelField("https://github.com/FritzsHero/RoadArchitect/tree/master");

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(12f);


        EditorGUILayout.LabelField("If you encounter Bugs or have a Feature Suggestion, you can submit them on one of the following sites:");
        GUILayout.Space(4f);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("MicroGSD's RoadArchitect Issues", EditorStyles.toolbarButton, GUILayout.Width(310f)))
        {
            Application.OpenURL("https://github.com/MicroGSD/RoadArchitect/issues");
        }
        EditorGUILayout.LabelField("https://github.com/MicroGSD/RoadArchitect/issues");

        EditorGUILayout.EndVertical();

        //GUILayout.Space(4f);

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("embeddedt's RoadArchitect Issues", EditorStyles.toolbarButton, GUILayout.Width(310f)))
        {
            Application.OpenURL("https://github.com/embeddedt/RoadArchitect/issues");
        }
        EditorGUILayout.LabelField("https://github.com/embeddedt/RoadArchitect/issues");

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        // Maybe remove the line above by this? "Please visit our Github with any questions or comments"
    }


    #region "Init"
    public void Initialize()
    {
        Rect fRect = new Rect(340, 170, 650, 350);
        position = fRect;
        Show();
        titleContent.text = "Help Info";
    }
    #endregion
}
#endif