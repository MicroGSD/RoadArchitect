#region "Imports"
using UnityEngine;
using UnityEditor;
#endregion
public class GSDHelpWindow : EditorWindow{
	void OnGUI() {
		EditorStyles.label.wordWrap = true;
		EditorStyles.miniLabel.wordWrap = true;
		
		EditorGUILayout.LabelField("Road Architect Help",EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Please visit the online manual for help.");
		GUILayout.Space(4f);
		
		if(GUILayout.Button("Click here to open online manual",EditorStyles.miniButton,GUILayout.Width(300f))){
			Application.OpenURL("http://microgsd.com/Support.aspx");
		}

		EditorGUILayout.LabelField("http://microgsd.com/Support.aspx",EditorStyles.miniLabel);
		GUILayout.Space(4f);
		EditorGUILayout.LabelField("Please visit the online manual for help.",EditorStyles.miniLabel);
		GUILayout.Space(4f); 
		EditorGUILayout.LabelField("Please visit our unity forum thread or contact support@microgsd.com with any questions or comments.",EditorStyles.miniLabel);
	}
	
	#region "Init"
	public void Initialize(){
		Rect fRect = new Rect(340,170,420,180);
		position = fRect;
        Show();
        titleContent.text = "Help Info";
    }
    #endregion
}