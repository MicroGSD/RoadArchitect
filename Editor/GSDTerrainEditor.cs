using UnityEngine;
using UnityEditor;
using GSD;
[CustomEditor(typeof(GSDTerrain))] 
public class GSDTerrainEditor : Editor{
	protected GSDTerrain GSDT { get { return (GSDTerrain) target; } }

	//Serialized properties:
	SerializedProperty tSplatImageWidth;
	SerializedProperty tSplatImageHeight;
	SerializedProperty tSplatBackgroundColor;
	SerializedProperty tSplatForegroundColor;
	SerializedProperty tSplatWidth;
	SerializedProperty tSkipBridges;
	SerializedProperty tSkipTunnels;
	SerializedProperty tSplatSingleRoad;
	SerializedProperty tSplatSingleChoiceIndex;
	SerializedProperty tRoadSingleChoiceUID;

	public enum SplatImageResoMatchingEnum{None,
		Match512x512, 
		Match1024x1024, 
		Match2048x2048, 
		Match4096x4096, 
		MatchHeightmapResolution, 
		MatchDetailResolution, 
		MatchTerrainSize
	};
	SplatImageResoMatchingEnum tSplatReso = SplatImageResoMatchingEnum.None;
	
	private static string[] TheSplatResoOptions = new string[]{
		"Select option to match resolution",
		"512 x 512",
		"1024 x 1024",
		"2048 x 2048",
		"4096 x 4096", 
		"Match heightmap resolution",
		"Match detail resolution",
		"Match terrain size"
	};
	
	//Editor only variables:
	string[] tRoads = null;
	string[] tRoadsString = null;
	Texture btnRefreshText = null;
	GUIStyle GSDImageButton = null;
	Texture2D LoadBtnBG = null;
	Texture2D LoadBtnBGGlow = null;
	GUIStyle GSDLoadButton = null;

	private void OnEnable() {
		tSplatImageWidth = serializedObject.FindProperty("SplatResoWidth");
		tSplatImageHeight = serializedObject.FindProperty("SplatResoHeight");
		tSplatBackgroundColor = serializedObject.FindProperty("SplatBackground");
		tSplatForegroundColor = serializedObject.FindProperty("SplatForeground");
		tSplatWidth = serializedObject.FindProperty("SplatWidth");
		tSkipBridges = serializedObject.FindProperty("SplatSkipBridges");
		tSkipTunnels = serializedObject.FindProperty("SplatSkipTunnels");
		tSplatSingleRoad = serializedObject.FindProperty("SplatSingleRoad");
		tSplatSingleChoiceIndex = serializedObject.FindProperty("SplatSingleChoiceIndex");
		tRoadSingleChoiceUID = serializedObject.FindProperty("RoadSingleChoiceUID");
	}
	
	public override void OnInspectorGUI(){
		serializedObject.Update();
		InitNullChecks();
		
		Line ();
		EditorGUILayout.BeginHorizontal();
		//Main label:
		EditorGUILayout.LabelField("Splat map generation:", EditorStyles.boldLabel);
		//Online manual button:
		if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
			Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(6f);
		
		//Splat Resolution input:
		tSplatImageWidth.intValue = GSDT.SplatResoWidth;
		tSplatImageHeight.intValue = GSDT.SplatResoHeight;
		EditorGUILayout.BeginHorizontal();
		tSplatReso = (SplatImageResoMatchingEnum)EditorGUILayout.Popup("Match resolutions:",(int)tSplatReso,TheSplatResoOptions);
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			tSplatImageWidth.intValue = 1024;
			tSplatImageHeight.intValue = 1024;
		}
		EditorGUILayout.EndHorizontal();
		
		if(tSplatReso != SplatImageResoMatchingEnum.None){
			if(tSplatReso == SplatImageResoMatchingEnum.MatchHeightmapResolution){
				tSplatImageWidth.intValue = GSDT.tTerrain.terrainData.heightmapResolution;
				tSplatImageHeight.intValue = GSDT.tTerrain.terrainData.heightmapResolution;
			}else if(tSplatReso == SplatImageResoMatchingEnum.MatchDetailResolution){
				tSplatImageWidth.intValue = GSDT.tTerrain.terrainData.detailResolution;
				tSplatImageHeight.intValue = GSDT.tTerrain.terrainData.detailResolution;
			}else if(tSplatReso == SplatImageResoMatchingEnum.MatchTerrainSize){
				tSplatImageWidth.intValue = (int)GSDT.tTerrain.terrainData.size.x;
				tSplatImageHeight.intValue = (int)GSDT.tTerrain.terrainData.size.z;
			}else if(tSplatReso == SplatImageResoMatchingEnum.Match512x512){
				tSplatImageWidth.intValue = 512;
				tSplatImageHeight.intValue = 512;
			}else if(tSplatReso == SplatImageResoMatchingEnum.Match1024x1024){
				tSplatImageWidth.intValue = 1024;
				tSplatImageHeight.intValue = 1024;
			}else if(tSplatReso == SplatImageResoMatchingEnum.Match2048x2048){
				tSplatImageWidth.intValue = 2048;
				tSplatImageHeight.intValue = 2048;
			}else if(tSplatReso == SplatImageResoMatchingEnum.Match4096x4096){
				tSplatImageWidth.intValue = 4096;
				tSplatImageHeight.intValue = 4096;
			}
			tSplatReso = SplatImageResoMatchingEnum.None;
		}

		//Splat image width input:
		tSplatImageWidth.intValue = EditorGUILayout.IntField("Splat image width:", tSplatImageWidth.intValue);
		//Splat image height input:
		tSplatImageHeight.intValue = EditorGUILayout.IntField("Splat image height:", tSplatImageHeight.intValue);
		

		//Splat background color input:
		EditorGUILayout.BeginHorizontal();
		tSplatBackgroundColor.colorValue = EditorGUILayout.ColorField("Splat background:", GSDT.SplatBackground);
			//Default button:
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			tSplatBackgroundColor.colorValue = new Color(0f,0f,0f,1f);
		}
		EditorGUILayout.EndHorizontal();
		
		//Splat foreground color input:
		EditorGUILayout.BeginHorizontal();
		tSplatForegroundColor.colorValue = EditorGUILayout.ColorField("Splat foreground:", GSDT.SplatForeground);
			//Default button:
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				tSplatForegroundColor.colorValue = new Color(1f,1f,1f,1f);
			}
		EditorGUILayout.EndHorizontal();
		
		//Splat width (meters) input:
		EditorGUILayout.BeginHorizontal();
		tSplatWidth.floatValue = EditorGUILayout.Slider("Splat width (meters):",GSDT.SplatWidth,0.02f,256f);
			//Default button:
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			tSplatWidth.floatValue = 30f;
		}
		EditorGUILayout.EndHorizontal();
		
		//Skip bridges:
		tSkipBridges.boolValue = EditorGUILayout.Toggle("Skip bridges: ",GSDT.SplatSkipBridges);
		
		//Skip tunnels:
		tSkipTunnels.boolValue = EditorGUILayout.Toggle("Skip tunnels: ",GSDT.SplatSkipTunnels);
				
		//Splat single road bool input:
		EditorGUILayout.BeginHorizontal();
		tSplatSingleRoad.boolValue = EditorGUILayout.Toggle("Splat a single road: ",GSDT.SplatSingleRoad);

		//Splat single road , road input:
		if(GSDT.SplatSingleRoad){
			LoadSplatSingleChoice();
			tSplatSingleChoiceIndex.intValue = EditorGUILayout.Popup(GSDT.SplatSingleChoiceIndex, tRoadsString, GUILayout.Width(150f));
			tRoadSingleChoiceUID.stringValue = tRoads[tSplatSingleChoiceIndex.intValue];
		}

		EditorGUILayout.EndHorizontal();
		
		//Generate splatmap button:
		GUILayout.Space(8f);
		if(GUILayout.Button("Generate splatmap for this terrain")){
			GenerateSplatMap();
		}
		GUILayout.Space(10f);
	
		if(GUI.changed){
			serializedObject.ApplyModifiedProperties();
//			EditorUtility.SetDirty(target); //Necessary?
		}
	}
	
	void InitNullChecks(){
		if(btnRefreshText == null){
			btnRefreshText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/refresh2.png",typeof(Texture)) as Texture;	
		}
		if(GSDImageButton == null){
	    	GSDImageButton = new GUIStyle(GUI.skin.button);
	    	GSDImageButton.contentOffset = new Vector2(0f,0f);
			GSDImageButton.border = new RectOffset(0,0,0,0);
			GSDImageButton.fixedHeight = 16f;
			GSDImageButton.padding = new RectOffset(0,0,0,0);
			GSDImageButton.normal.background = null;
		}
		if(LoadBtnBG == null){
			LoadBtnBG = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/FlexBG.png",typeof(Texture2D)) as Texture2D;	
		}
		if(LoadBtnBGGlow == null){
			LoadBtnBGGlow = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/FlexBG.png",typeof(Texture2D)) as Texture2D;	
		}
		if(GSDLoadButton == null){
	    	GSDLoadButton = new GUIStyle(GUI.skin.button);
			GSDLoadButton.contentOffset = new Vector2(0f,1f);
			GSDLoadButton.normal.textColor = new Color(1f,1f,1f,1f);
			GSDLoadButton.normal.background = LoadBtnBG;
			GSDLoadButton.active.background = LoadBtnBGGlow;
			GSDLoadButton.focused.background = LoadBtnBGGlow;
			GSDLoadButton.hover.background = LoadBtnBGGlow;
			GSDLoadButton.fixedHeight = 16f;
			GSDLoadButton.padding = new RectOffset(0,0,0,0);
		}
	}
	
	private void LoadSplatSingleChoice(){
		tRoads = null;
		tRoadsString = null;
		Object[] xRoads = GameObject.FindObjectsOfType(typeof(GSDRoad));	
		int xRoadsCount = xRoads.Length;
		tRoads = new string[xRoadsCount];
		tRoadsString = new string[xRoadsCount];
		int xCounter = 0;
		foreach(GSDRoad tRoad in xRoads){
			tRoads[xCounter] = tRoad.UID;
			tRoadsString[xCounter] = tRoad.transform.name;
			xCounter+=1;
		}	
	}
	
	private void GenerateSplatMap(){
		byte[] tBytes = null;
		if(GSDT.SplatSingleRoad && GSDT.RoadSingleChoiceUID != ""){
			tBytes = GSD.Roads.GSDRoadUtil.MakeSplatMap(GSDT.tTerrain,GSDT.SplatBackground,GSDT.SplatForeground,GSDT.SplatResoWidth,GSDT.SplatResoHeight,GSDT.SplatWidth,GSDT.SplatSkipBridges,GSDT.SplatSkipTunnels,GSDT.RoadSingleChoiceUID);
		}else{
			tBytes = GSD.Roads.GSDRoadUtil.MakeSplatMap(GSDT.tTerrain,GSDT.SplatBackground,GSDT.SplatForeground,GSDT.SplatResoWidth,GSDT.SplatResoHeight,GSDT.SplatWidth,GSDT.SplatSkipBridges,GSDT.SplatSkipTunnels);
		}
		
		if(tBytes != null && tBytes.Length > 3){
			string tPath = UnityEditor.EditorUtility.SaveFilePanel("Save splat map",Application.dataPath,"Splat","png");
			if(tPath != null && tPath.Length > 3){
	       		System.IO.File.WriteAllBytes(tPath, tBytes);
			}
			tBytes = null;
		}
	}
	
	void Line(){
		GUILayout.Space(4f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); //Horizontal bar
		GUILayout.Space(4f);
	}
}