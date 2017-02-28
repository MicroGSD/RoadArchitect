#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Used for progress information for other areas of RA.
/// </summary>
public class GSDEditorProgressWindow : EditorWindow {
	float secs = 10.0f;
	float startVal = 0f;
	float progress = 0f;
	
	#if UNITY_EDITOR
	static void Init(){
		GSDEditorProgressWindow window = (GSDEditorProgressWindow)EditorWindow.GetWindow (typeof (GSDEditorProgressWindow));
		window.Show();
	}
	#endif
	
	#if UNITY_EDITOR
	void OnGUI(){
		secs = EditorGUILayout.FloatField("Time to wait:", secs);
		if(GUILayout.Button("Display bar")) {
			if(secs < 1) {
				Debug.LogError("Seconds should be bigger than 1");
				return;
			}
			startVal = (float)EditorApplication.timeSinceStartup;
		}

		if(progress < secs)
			EditorUtility.DisplayProgressBar(
				"Simple Progress Bar",
				"Shows a progress bar for the given seconds",
				progress/secs);
		else
			EditorUtility.ClearProgressBar();

		progress = (float)(EditorApplication.timeSinceStartup - startVal);
	}
	

	void OnInspectorUpdate(){
		Repaint();
	}
	#endif
}