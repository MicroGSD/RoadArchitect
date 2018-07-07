#region "Imports"
using UnityEngine;
using UnityEditor;
using System.Collections;
using GSD.Roads;
using GSD;
[CustomEditor(typeof(GSDRoad))] 
#endregion
public class GSDRoadEditor : Editor {

    private static string[] RoadMaterialDropdownEnumDesc = new string[]{
		"Asphalt",
		"Dirt",
		"Brick", 
		"Cobblestone"
	};


	protected GSDRoad RS { get { return (GSDRoad) target; } }
	
	//Serialized properties:
	SerializedProperty t_opt_GizmosEnabled;
	SerializedProperty t_opt_Lanes;
	SerializedProperty t_opt_LaneWidth;
	SerializedProperty t_opt_bShouldersEnabled;
	SerializedProperty t_opt_ShoulderWidth;
	SerializedProperty t_opt_RoadDefinition;
	SerializedProperty t_opt_UseDefaultMaterials;
	SerializedProperty t_opt_bMaxGradeEnabled;
	SerializedProperty t_opt_MaxGrade;
	SerializedProperty t_opt_bMultithreading;
	SerializedProperty t_opt_bSaveMeshes;
	SerializedProperty t_opt_TerrainSubtract_Match;
	SerializedProperty t_opt_MagnitudeThreshold;
	SerializedProperty t_opt_HeightModEnabled;
	SerializedProperty t_opt_DetailModEnabled;
	SerializedProperty t_opt_TreeModEnabled;
	SerializedProperty t_opt_MatchHeightsDistance;
	SerializedProperty t_opt_ClearDetailsDistance;
	SerializedProperty t_opt_ClearDetailsDistanceHeight;
	SerializedProperty t_opt_ClearTreesDistance;
	SerializedProperty t_opt_ClearTreesDistanceHeight;
	SerializedProperty t_opt_SaveTerrainHistoryOnDisk;
	SerializedProperty t_opt_bRoadCuts;
	SerializedProperty t_opt_bDynamicCuts;
	SerializedProperty t_opt_bShoulderCuts;
	SerializedProperty t_bEditorCameraRotate;
	SerializedProperty t_EditorCameraMetersPerSecond;
    SerializedProperty t_opt_bUseMeshColliders;
    SerializedProperty t_opt_tRoadMaterialDropdown;
    SerializedProperty t_opt_bIsStatic;
    SerializedProperty t_opt_bIsLightmapped;
	
	SerializedProperty t_RoadMaterial1;
	SerializedProperty t_RoadMaterial2;
	SerializedProperty t_RoadMaterial3;
	SerializedProperty t_RoadMaterial4;
	SerializedProperty t_RoadMaterialMarker1;
	SerializedProperty t_RoadMaterialMarker2;
	SerializedProperty t_RoadMaterialMarker3;
	SerializedProperty t_RoadMaterialMarker4;
	SerializedProperty t_ShoulderMaterial1;
	SerializedProperty t_ShoulderMaterial2;
	SerializedProperty t_ShoulderMaterial3;
	SerializedProperty t_ShoulderMaterial4;
	SerializedProperty t_ShoulderMaterialMarker1;
	SerializedProperty t_ShoulderMaterialMarker2;
	SerializedProperty t_ShoulderMaterialMarker3;
	SerializedProperty t_ShoulderMaterialMarker4;
	SerializedProperty t_RoadPhysicMaterial;
	SerializedProperty t_ShoulderPhysicMaterial;
	
	//Editor only variables:
	string status = "Show help";
	const string tOnlineHelpDesc = "Visit the online manual for the most effective help.";
	bool bShowCutsHelp = false;
	bool bShowMatsHelp = false;
	bool bShowHelpRoad = false;
	bool bShowHelpTerrain = false;
	bool bShowCameraHelp = false;
	GUIStyle GSDLoadButton = null;
	bool bResetTH = false;
	public enum tempEnum{Two,Four,Six};
	Texture btnRefreshText = null;
	Texture btnDeleteText = null;
	Texture btnRefreshTextReal = null;
	tempEnum LanesEnum = tempEnum.Two;
	tempEnum tLanesEnum = tempEnum.Two;
	private static string[] tempEnumDescriptions = new string[]{
		"Two",
		"Four",
		"Six"
	};
	GUIStyle WarningLabelStyle;
	Texture2D WarningLabelBG;
	GUIStyle GSDImageButton = null;
	GUIStyle GSDMaybeButton = null;
	bool bHasInit = false;
	Texture2D LoadBtnBG = null;
	Texture2D LoadBtnBGGlow = null;
	
	//Buffers:
    //float TempChangeChecker = 0f;
 	//bool bMatChange = false;
	bool bNeedRoadUpdate = false;
	bool bSetDefaultMats = false;
	bool bApplyMatsCheck = false;
	bool t_bApplyMatsCheck = false;
	
	private void OnEnable() {
		t_opt_GizmosEnabled 				= serializedObject.FindProperty("opt_GizmosEnabled");
		t_opt_Lanes 						= serializedObject.FindProperty("opt_Lanes");
		t_opt_LaneWidth 					= serializedObject.FindProperty("opt_LaneWidth");
		t_opt_bShouldersEnabled 			= serializedObject.FindProperty("opt_bShouldersEnabled");
		t_opt_ShoulderWidth 				= serializedObject.FindProperty("opt_ShoulderWidth");
		t_opt_RoadDefinition 				= serializedObject.FindProperty("opt_RoadDefinition");
		t_opt_UseDefaultMaterials 			= serializedObject.FindProperty("opt_UseDefaultMaterials");
		t_opt_bMaxGradeEnabled 				= serializedObject.FindProperty("opt_bMaxGradeEnabled");
		t_opt_MaxGrade 						= serializedObject.FindProperty("opt_MaxGrade");
		t_opt_bMultithreading 				= serializedObject.FindProperty("opt_bMultithreading");
		t_opt_bSaveMeshes 					= serializedObject.FindProperty("opt_bSaveMeshes");
		t_opt_TerrainSubtract_Match 		= serializedObject.FindProperty("opt_TerrainSubtract_Match");
		t_opt_MagnitudeThreshold 			= serializedObject.FindProperty("opt_MagnitudeThreshold");
		t_opt_HeightModEnabled 				= serializedObject.FindProperty("opt_HeightModEnabled");
		t_opt_DetailModEnabled 				= serializedObject.FindProperty("opt_DetailModEnabled");
		t_opt_TreeModEnabled 				= serializedObject.FindProperty("opt_TreeModEnabled");
		t_opt_MatchHeightsDistance 			= serializedObject.FindProperty("opt_MatchHeightsDistance");
		t_opt_ClearDetailsDistance 			= serializedObject.FindProperty("opt_ClearDetailsDistance");
		t_opt_ClearDetailsDistanceHeight 	= serializedObject.FindProperty("opt_ClearDetailsDistanceHeight");
		t_opt_ClearTreesDistance 			= serializedObject.FindProperty("opt_ClearTreesDistance");
		t_opt_ClearTreesDistanceHeight 		= serializedObject.FindProperty("opt_ClearTreesDistanceHeight");
		t_opt_SaveTerrainHistoryOnDisk 		= serializedObject.FindProperty("opt_SaveTerrainHistoryOnDisk");
		t_opt_bRoadCuts 					= serializedObject.FindProperty("opt_bRoadCuts");
		t_opt_bDynamicCuts 					= serializedObject.FindProperty("opt_bDynamicCuts");
		t_opt_bShoulderCuts 				= serializedObject.FindProperty("opt_bShoulderCuts");
		t_bEditorCameraRotate 				= serializedObject.FindProperty("bEditorCameraRotate");
		t_EditorCameraMetersPerSecond 		= serializedObject.FindProperty("EditorCameraMetersPerSecond");
        t_opt_bUseMeshColliders             = serializedObject.FindProperty("opt_bUseMeshColliders");
        t_opt_tRoadMaterialDropdown         = serializedObject.FindProperty("opt_tRoadMaterialDropdown");
        t_opt_bIsStatic                     = serializedObject.FindProperty("opt_bIsStatic");
        t_opt_bIsLightmapped                = serializedObject.FindProperty("opt_bIsLightmapped");

		t_RoadMaterial1 					= serializedObject.FindProperty("RoadMaterial1");
		t_RoadMaterial2 					= serializedObject.FindProperty("RoadMaterial2");
		t_RoadMaterial3 					= serializedObject.FindProperty("RoadMaterial3");
		t_RoadMaterial4 					= serializedObject.FindProperty("RoadMaterial4");
		t_RoadMaterialMarker1 				= serializedObject.FindProperty("RoadMaterialMarker1");
		t_RoadMaterialMarker2 				= serializedObject.FindProperty("RoadMaterialMarker2");
		t_RoadMaterialMarker3 				= serializedObject.FindProperty("RoadMaterialMarker3");
		t_RoadMaterialMarker4 				= serializedObject.FindProperty("RoadMaterialMarker4");
		t_ShoulderMaterial1 				= serializedObject.FindProperty("ShoulderMaterial1");
		t_ShoulderMaterial2 				= serializedObject.FindProperty("ShoulderMaterial2");
		t_ShoulderMaterial3 				= serializedObject.FindProperty("ShoulderMaterial3");
		t_ShoulderMaterial4			 		= serializedObject.FindProperty("ShoulderMaterial4");
		t_ShoulderMaterialMarker1 			= serializedObject.FindProperty("ShoulderMaterialMarker1");
		t_ShoulderMaterialMarker2 			= serializedObject.FindProperty("ShoulderMaterialMarker2");
		t_ShoulderMaterialMarker3 			= serializedObject.FindProperty("ShoulderMaterialMarker3");
		t_ShoulderMaterialMarker4 			= serializedObject.FindProperty("ShoulderMaterialMarker4");
		t_RoadPhysicMaterial 				= serializedObject.FindProperty("RoadPhysicMaterial");
		t_ShoulderPhysicMaterial 			= serializedObject.FindProperty("ShoulderPhysicMaterial");
	}
	
	void Init(){
		bHasInit = true;
		EditorStyles.label.wordWrap = true;
		
		if(WarningLabelBG == null){
			WarningLabelBG = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/WarningLabelBG.png",typeof(Texture2D)) as Texture2D;	
		}
		if(btnRefreshText == null){
			btnRefreshText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/refresh2.png",typeof(Texture)) as Texture;	
		}
		if(btnRefreshTextReal == null){
			btnRefreshTextReal	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/refresh.png",typeof(Texture)) as Texture;	
		}
		if(LoadBtnBG == null){
			LoadBtnBG = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/otherbg.png",typeof(Texture2D)) as Texture2D;	
		}
		if(LoadBtnBGGlow == null){
			LoadBtnBGGlow = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/otherbg2.png",typeof(Texture2D)) as Texture2D;	
		}
		if(btnDeleteText == null){
			btnDeleteText = (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/delete.png",typeof(Texture)) as Texture;	
		}
		
		if(WarningLabelStyle == null){
	    	WarningLabelStyle = new GUIStyle(GUI.skin.textArea);
			WarningLabelStyle.normal.textColor = Color.red;
			WarningLabelStyle.active.textColor = Color.red;
			WarningLabelStyle.hover.textColor = Color.red;
			WarningLabelStyle.normal.background = WarningLabelBG;
			WarningLabelStyle.active.background = WarningLabelBG;
			WarningLabelStyle.hover.background = WarningLabelBG;
			WarningLabelStyle.padding = new RectOffset(8,8,8,8);
		}
		
		if(GSDImageButton == null){
	    	GSDImageButton = new GUIStyle(GUI.skin.button);
	    	GSDImageButton.contentOffset = new Vector2(0f,0f);
			GSDImageButton.border = new RectOffset(0,0,0,0);
			GSDImageButton.fixedHeight = 16f;
			GSDImageButton.padding = new RectOffset(0,0,0,0);
			GSDImageButton.normal.background = null;
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
			GSDLoadButton.fixedWidth = 128f;
		}
		
		if(GSDMaybeButton == null){
	    	GSDMaybeButton = new GUIStyle(GUI.skin.button);
			GSDMaybeButton.normal.textColor = new Color(0f,0f,0f,1f);
		}
	}
	
	public override void OnInspectorGUI(){
		if(Event.current.type == EventType.ValidateCommand){
			switch (Event.current.commandName){
			case "UndoRedoPerformed":
				TriggerRoadUpdate();
				break;
			}
		}

		serializedObject.Update();

		bNeedRoadUpdate = false;
		bSetDefaultMats = false;
		//Graphic null checks:
		if(!bHasInit){ Init(); }
		
		
		
		Line();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(RS.transform.name,EditorStyles.boldLabel);
		if(GUILayout.Button("Update road",GSDLoadButton)){
			RS.EditorUpdateMe = true;
		}

		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.LabelField("Hold ctrl and click terrain to add nodes.");
		EditorGUILayout.LabelField("Hold shift and click terrain to insert nodes.");
		EditorGUILayout.LabelField("Select nodes on spline to add objects.");
		EditorGUILayout.LabelField("Road options:");

		
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.BeginHorizontal();
		//Option: Gizmos input:
		t_opt_GizmosEnabled.boolValue = EditorGUILayout.Toggle("Gizmos: ",RS.opt_GizmosEnabled);
		if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
			Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
		}
		EditorGUILayout.EndHorizontal();
		
		//Option: Lane count:
		if(RS.opt_Lanes == 2){
			LanesEnum = tempEnum.Two;
		}else if(RS.opt_Lanes == 4){
			LanesEnum = tempEnum.Four;
		}else{
			LanesEnum = tempEnum.Six;
		}
		tLanesEnum = (tempEnum)EditorGUILayout.Popup("Lanes: ",(int)LanesEnum,tempEnumDescriptions);
		if(tLanesEnum == tempEnum.Two){
			t_opt_Lanes.intValue = 2;
		}else if(tLanesEnum == tempEnum.Four){
			t_opt_Lanes.intValue = 4;
		}else if(tLanesEnum == tempEnum.Six){
			t_opt_Lanes.intValue = 6;
		}

		//Option: Lane and road width:
		EditorGUILayout.BeginHorizontal();
		t_opt_LaneWidth.floatValue = EditorGUILayout.FloatField("Lane width:",RS.opt_LaneWidth);
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			t_opt_LaneWidth.floatValue = 5f;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Road width: " + RS.RoadWidth().ToString("F1") + " meters");
		
		//Option: Shoulders enabled:
		t_opt_bShouldersEnabled.boolValue = EditorGUILayout.Toggle("Shoulders enabled:",RS.opt_bShouldersEnabled);
		
		//Option: Shoulders width:
		if(RS.opt_bShouldersEnabled){
			EditorGUILayout.BeginHorizontal();
			t_opt_ShoulderWidth.floatValue = EditorGUILayout.FloatField("Shoulders width:",RS.opt_ShoulderWidth);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_ShoulderWidth.floatValue = 3f;
			}
			EditorGUILayout.EndHorizontal();
		}
		
		//Option: Road definition:
		EditorGUILayout.BeginHorizontal();
		t_opt_RoadDefinition.floatValue = EditorGUILayout.FloatField("Road definition:",RS.opt_RoadDefinition);
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			t_opt_RoadDefinition.floatValue = 5f;
		}
		EditorGUILayout.EndHorizontal();
		
		//Option: Use default materials:
		t_opt_UseDefaultMaterials.boolValue = EditorGUILayout.Toggle("Use default materials:",RS.opt_UseDefaultMaterials);
		
        //Dropdown:
        if (RS.opt_UseDefaultMaterials) {
            int Old = (int)RS.opt_tRoadMaterialDropdown;
            t_opt_tRoadMaterialDropdown.enumValueIndex = (int)EditorGUILayout.Popup("Road material: ", (int)RS.opt_tRoadMaterialDropdown, RoadMaterialDropdownEnumDesc, GUILayout.Width(250f));
            if (t_opt_tRoadMaterialDropdown.enumValueIndex != Old) {
                if (t_opt_tRoadMaterialDropdown.enumValueIndex > 0) {
                    t_opt_bShouldersEnabled.boolValue = false;
                } else {
                    t_opt_bShouldersEnabled.boolValue = true;
                }
            }
        }

		//Option: Max grade enabled:
		t_opt_bMaxGradeEnabled.boolValue = EditorGUILayout.Toggle("Max grade enforced: ",RS.opt_bMaxGradeEnabled);
		
		//Option: Max grade value:
		if(RS.opt_bMaxGradeEnabled){
			EditorGUILayout.BeginHorizontal();
			t_opt_MaxGrade.floatValue = EditorGUILayout.Slider("Max road grade: ",RS.opt_MaxGrade,0f,1f);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_MaxGrade.floatValue = 0.08f;
			}
			EditorGUILayout.EndHorizontal();
		}

        //Mesh colliders:
        if (RS.GSDRS != null) {
            t_opt_bUseMeshColliders.boolValue = EditorGUILayout.Toggle("Use mesh colliders: ", RS.opt_bUseMeshColliders);
        }

		//Option: Multi-threading option: workaround for UAS submission rules:
		if(RS.GSDRS.opt_bMultithreading != RS.opt_bMultithreading){
			RS.GSDRS.opt_bMultithreading = RS.opt_bMultithreading;
			RS.GSDRS.UpdateAllRoads_MultiThreadOptions();
		}
		if(RS.GSDRS != null){
			t_opt_bMultithreading.boolValue = EditorGUILayout.Toggle("Multithreading: ",RS.GSDRS.opt_bMultithreading);
		}

        //Static:
        if (RS.GSDRS != null) {
            t_opt_bIsStatic.boolValue = EditorGUILayout.Toggle("Static: ", RS.opt_bIsStatic);
        }

        //Used for lightmapping:
        if (RS.GSDRS != null) {
            t_opt_bIsLightmapped.boolValue = EditorGUILayout.Toggle("Lightmapped: ", RS.opt_bIsLightmapped);
        }
		
		//Option: Save meshes as unity assets options:
		if(RS.GSDRS.opt_bSaveMeshes != RS.opt_bSaveMeshes){
			RS.GSDRS.opt_bSaveMeshes = RS.opt_bSaveMeshes;
			RS.GSDRS.UpdateAllRoads_SaveMeshesAsAssetsOptions();
		}
		if(RS.GSDRS != null){
			t_opt_bSaveMeshes.boolValue = EditorGUILayout.Toggle("Save mesh assets: ",RS.GSDRS.opt_bSaveMeshes);
		}
		if(RS.GSDRS.opt_bSaveMeshes){
			GUILayout.Label("WARNING: Saving meshes as assets is very slow and can increase road generation time by several minutes.",WarningLabelStyle);
		}

        if (GUILayout.Button("Duplicate road", EditorStyles.miniButton, GUILayout.Width(120f))) {
            RS.DuplicateRoad();
        }

		
		bShowHelpRoad = EditorGUILayout.Foldout(bShowHelpRoad, status);
		if(bShowHelpRoad){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Road options quick help:",EditorStyles.boldLabel);
			if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
				Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.HelpBox(tOnlineHelpDesc,MessageType.Info);
			
			GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Gizmos:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Enable or disable most gizmos for this road. Disable mesh collider gizmos via the unity menu if necessary or desired.",EditorStyles.miniLabel);
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Lanes:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Select the number of lanes for this road.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Lane width:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Modify the width per lane, in meters.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Road width:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Displays the road width without considering shoulders, in meters.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Shoulders enabled:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Enables or disables shoulders.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Shoulders width:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Modify the width of shoulders, in meters.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Road definition: ",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The meter spacing between mesh triangles on the road and shoulder.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Use default materials: ",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("When enabled will use default materials for the road system, allowing certain aspects of generation to automatically determine the correct materials to utilize.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Max grade enforced: ",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("When enabled enforces a maximum grade on a per node basis.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Max road grade: ",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The maximum road grade allowed on a per node basis.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Multithreading:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("When enabled allows for multi-threaded road generation.");
			EditorGUILayout.EndVertical(); GUILayout.Space(4f); EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Save mesh assets:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("When enabled saves all generated meshes as .asset files.");
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				
			}
			EditorGUILayout.LabelField(" = Resets settings to default.");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.LabelField("Terrain options:");
		EditorGUILayout.BeginVertical("box");
		
		//Option: Terrain subtraction:
		EditorGUILayout.BeginHorizontal();
		t_opt_TerrainSubtract_Match.floatValue = EditorGUILayout.Slider("Terrain subtraction: ",RS.opt_TerrainSubtract_Match,0.01f,1f);
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			t_opt_TerrainSubtract_Match.floatValue = 0.01f;
		}
		EditorGUILayout.EndHorizontal();
		
		//Option: Spline magnitude limit:
		EditorGUILayout.BeginHorizontal();
		t_opt_MagnitudeThreshold.floatValue = EditorGUILayout.Slider("Spline magnitude limit: ",RS.opt_MagnitudeThreshold,128f,8192f); 
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			t_opt_MagnitudeThreshold.floatValue = 300f;
		}
		EditorGUILayout.EndHorizontal();
		
		//Option: Height modification
		t_opt_HeightModEnabled.boolValue = EditorGUILayout.Toggle("Height modification: ",RS.opt_HeightModEnabled);
		
		//Option: Active detail removal
		t_opt_DetailModEnabled.boolValue = EditorGUILayout.Toggle("Active detail removal: ",RS.opt_DetailModEnabled);
		
		//Option: Active tree removal
		t_opt_TreeModEnabled.boolValue = EditorGUILayout.Toggle("Active tree removal: ",RS.opt_TreeModEnabled);
		
		//Option: heights width
		if(RS.opt_HeightModEnabled){
			EditorGUILayout.BeginHorizontal();
			t_opt_MatchHeightsDistance.floatValue = EditorGUILayout.Slider("Heights match width: ",RS.opt_MatchHeightsDistance,0.01f,512f);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_MatchHeightsDistance.floatValue = 50f;
			}
			EditorGUILayout.EndHorizontal();
		}
		
		//Option: details width and height
		if(RS.opt_DetailModEnabled){
			EditorGUILayout.BeginHorizontal();
			t_opt_ClearDetailsDistance.floatValue = EditorGUILayout.Slider("Details clear width: ",RS.opt_ClearDetailsDistance,0.01f,512f);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_ClearDetailsDistance.floatValue = 30f;
			}
			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			t_opt_ClearDetailsDistanceHeight.floatValue = EditorGUILayout.Slider("Details clear height: ",RS.opt_ClearDetailsDistanceHeight,0.01f,512f);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_ClearDetailsDistanceHeight.floatValue = 5f;
			}
			
			EditorGUILayout.EndHorizontal();
		}
		
		//Option: tree widths and height
		if(RS.opt_TreeModEnabled){
			EditorGUILayout.BeginHorizontal();
			t_opt_ClearTreesDistance.floatValue = EditorGUILayout.Slider("Trees clear width: ",RS.opt_ClearTreesDistance,0.01f,512f);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_ClearTreesDistance.floatValue = 30f;
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			t_opt_ClearTreesDistanceHeight.floatValue = EditorGUILayout.Slider("Trees clear height: ",RS.opt_ClearTreesDistanceHeight,0.01f,512f);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_ClearTreesDistanceHeight.floatValue = 50f;
			}
			EditorGUILayout.EndHorizontal();
		}

		
		//Option: terrain history save type:
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Store terrain history separate from scene:");
		t_opt_SaveTerrainHistoryOnDisk.boolValue = EditorGUILayout.Toggle(RS.opt_SaveTerrainHistoryOnDisk, GUILayout.Width(50f));
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.LabelField("Terrain history size: " + RS.TerrainHistoryByteSize);

		bShowHelpTerrain = EditorGUILayout.Foldout(bShowHelpTerrain, status);
		if(bShowHelpTerrain){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Terrain options quick help:",EditorStyles.boldLabel);
			if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
				Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.HelpBox(tOnlineHelpDesc,MessageType.Info);
			EditorGUILayout.LabelField("Terrain subtraction: ",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("This value, in meters, will be subtracted from the terrain match height to prevent z-fighting.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Spline magnitude limit: ",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Limits the magnitude of the spline nodes. Lower limit is better for typical roads with node seperation of around 100 to 300 meters. Higher limits will allow for less tension when using very spread out nodes.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Height Modification:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Enables or disables height matching for the terrain.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Active detail removal:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Enables or disables active detail removal. Memory intensive on large terrains with large amounts of details. Recommended to not use this option and instead remove details and trees via splat maps with other addons.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Active tree removal:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Enables or disables active tree removal. Memory intensive on large terrains with large amounts of trees. Recommended to not use this option and instead remove details and trees via splat maps with other addons.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Heights match width:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The distance to the left and right of the road in which terrain heights will be matched to the road.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Details clear width:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The distance between the road and detail, width wise, in which details will be removed.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Details clear height:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The distance between the road and detail, height wise, in which details will be removed.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Tree clear width:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The distance between the road and tree, width wise, in which trees will be removed.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Tree clear height:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The distance between the road and tree, height wise, in which trees will be removed.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Store terrain history separate from scene:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("If enabled, stores the terrain history immediately on disk after use, saving memory while in editor.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Terrain history size:",EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Shows the size, in kilobytes, of the terrain history in memory or on disk.");
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				
			}
			EditorGUILayout.LabelField(" = Resets settings to default.");
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();


		
		GUILayout.Label("Road and shoulder splitting:");
		EditorGUILayout.BeginVertical("box");
		GUILayout.Space(4f);
		
		//Option: road cuts:
		if(!RS.opt_bDynamicCuts){
			EditorGUILayout.BeginHorizontal();
			t_opt_bRoadCuts.boolValue = EditorGUILayout.Toggle("Auto split road: ",RS.opt_bRoadCuts);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_bDynamicCuts.boolValue = false;
				t_opt_bRoadCuts.boolValue = true;
				t_opt_bShoulderCuts.boolValue = true;
			}
			EditorGUILayout.EndHorizontal();
			
			if(RS.opt_bShouldersEnabled){ 
				t_opt_bShoulderCuts.boolValue = EditorGUILayout.Toggle("Auto split shoulders: ",RS.opt_bShoulderCuts); 
			}
		}else{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Manual road splitting: true");
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_opt_bDynamicCuts.boolValue = false;
				t_opt_bRoadCuts.boolValue = true;
				t_opt_bShoulderCuts.boolValue = true;
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.LabelField("Manual shoulder splitting: true");
		}
		t_opt_bDynamicCuts.boolValue = EditorGUILayout.Toggle("Manual splitting: ",RS.opt_bDynamicCuts);
		
		
		bShowCutsHelp = EditorGUILayout.Foldout(bShowCutsHelp, status);
		if(bShowCutsHelp){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Road splitting quick help:",EditorStyles.boldLabel);
			if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
				Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.HelpBox(tOnlineHelpDesc,MessageType.Info);
			
			EditorGUILayout.LabelField("Typically auto-split will be the best choice for performance and other reasons.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Choosing split options will split the road/shoulder up into pieces mirroring the locations of nodes.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Splitting allows for more detailed and flexible road texturing options such as passing sections, other different road lines per section, road debris and more.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Choosing split options may also speed up mesh collider collision calculations if bounds calculations are involved.");
			GUILayout.Space(4f);
			
			EditorGUILayout.LabelField("Which splitting option to choose?", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Choose no splitting if you desire a single material set for this entire road and your game experiences no collison processing slowdowns from one large mesh collider. This option will create less game objects than automatic and manual splitting.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Choose automatic road and shoulder splitting if you desire multiple materials (such as yellow double lines for certain sections and white dotted for others) for this road and or your game experiences collision processing slowdowns from one large mesh collider.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Choose manual road and shoulder splitting if you desire the same as automatic splitting and desire more freedom over the process. This option will result in less gameobjects and larger mesh colliders when compared to automatic splitting.");
			GUILayout.Space(4f);
			
			
			Line ();
			EditorGUILayout.LabelField("Manual splitting information: ");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Choosing manual splitting will force the user to select individual nodes to cut instead of the cuts being performed automatically. This option is recommended if bigger mesh colliders do not cause a slowdown in performance, as it lowers the overall gameobject count.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Manual splitting will not split up the mesh colliders like automatic cuts, so the colliders may get large & complex and cost more CPU to process collisions. If this option is chosen, please verify your game's collision processing speed and if you run into long collision processing times split more road sections");	
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Example usages of manual splitting");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Example hill: Goal is to have double yellow no passing lines on a two lane road but only while the road is near or on the hill. " +
				"Pick nodes on either sides of the hill and mark both as road cut. Everything between these two nodes will be its own section, " +
				"allowing you to apply double yellow no passing lines for just the hill.");	
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Example mountains: In the mountains, the road is curvy and the grade is high. " +
				"There's a flat straight spot that you want to allow passing in, by marking the road with white dotted passing lines. " +
				"At the beginning of the flat straight section, mark the node as road cut. Now at the end of the flat straight section, mark this node as road cut. " +
				"This will create a road section between the two nodes, allowing you to apply white dotted passing lines for just the flat straight section.");
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				
			}
			EditorGUILayout.LabelField(" = Resets settings to default.");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}else{
			GUILayout.Space(4f);	
		}
		EditorGUILayout.EndVertical();
		

		//Camera:
		EditorGUILayout.LabelField("Editor camera travel:");
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.BeginHorizontal();
		
		//Option: Editor camera meters per sec
		t_EditorCameraMetersPerSecond.floatValue = EditorGUILayout.Slider("Camera meters/sec:",RS.EditorCameraMetersPerSecond,1f,512f);
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			t_EditorCameraMetersPerSecond.floatValue = 60f;
		}
		EditorGUILayout.EndHorizontal();
		
		//Option: Editor camera auto rotate:
		t_bEditorCameraRotate.boolValue = EditorGUILayout.Toggle("Camera auto rotate: ",RS.bEditorCameraRotate);
		if(RS.EditorPlayCamera == null){
			RS.EditorCameraSetSingle();
		}
		RS.EditorPlayCamera = (Camera)EditorGUILayout.ObjectField("Editor play camera:",RS.EditorPlayCamera,typeof(Camera),true);
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Reset",GUILayout.Width(70f))){
			RS.QuitEditorCamera();
			RS.DoEditorCameraLoop();
		}
		if(GUILayout.Button("<<",GUILayout.Width(40f))){
			RS.EditorCameraPos -= 0.1f;
			RS.DoEditorCameraLoop();
		}
		if(RS.bEditorCameraMoving == true){
			if(GUILayout.Button("Pause",GUILayout.Width(70f))){
				RS.bEditorCameraMoving = false;
			}
		}else{
			if(GUILayout.Button("Play",GUILayout.Width(70f))){
				RS.bEditorCameraMoving = true;
			}
		}
		if(GUILayout.Button(">>",GUILayout.Width(40f))){
			RS.EditorCameraPos += 0.1f;
			RS.DoEditorCameraLoop();
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(4);
		bShowCameraHelp = EditorGUILayout.Foldout(bShowCameraHelp, status);
		if(bShowCameraHelp){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Editor camera quick help:",EditorStyles.boldLabel);
			if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
				Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.HelpBox(tOnlineHelpDesc,MessageType.Info);
			EditorGUILayout.LabelField("Use this section to travel along the road while in the editor sceneview.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Camera meters/sec is the speed at which the camera moves along the road.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Auto rotate will automatically rotate the camera to look forward at the current road's tangent. Note: You can still zoom in and out with the camera with this option selected.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Note: Materials act differently in the editor scene view compared to actual gameplay. Try the game camera if the materials are z fighting and having other issues.");
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				
			}
			EditorGUILayout.LabelField(" = Resets settings to default.");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
	
		GUILayout.Label("Materials:");
		EditorGUILayout.BeginVertical("box");
		//Road material defaults:
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Road base material(s) defaults:");
		//Option: Set mats to defaults:
		bSetDefaultMats = false;
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			bSetDefaultMats = true;
		} 
		EditorGUILayout.EndHorizontal();
//		EditorGUILayout.PropertyField (t_RoadMaterial1, new GUIContent ("  Mat #1: "));
//		if(RS.RoadMaterial1 != null){ EditorGUILayout.PropertyField (t_RoadMaterial2, new GUIContent ("  Mat #2: ")); }
//		if(RS.RoadMaterial2 != null){EditorGUILayout.PropertyField (t_RoadMaterial3, new GUIContent ("  Mat #3: ")); }
//		if(RS.RoadMaterial3 != null){EditorGUILayout.PropertyField (t_RoadMaterial4, new GUIContent ("  Mat #4: ")); }


		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField (t_RoadMaterial1, new GUIContent ("  Mat #1: "));
		if(RS.RoadMaterial1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterial1 = null; }
		EditorGUILayout.EndHorizontal();
		if(RS.RoadMaterial1 != null){ 
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_RoadMaterial2, new GUIContent ("  Mat #2: ")); 
			if(RS.RoadMaterial2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterial2 = null; }
			EditorGUILayout.EndHorizontal();
		}
		if(RS.RoadMaterial1 != null && RS.RoadMaterial2 != null){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_RoadMaterial3, new GUIContent ("  Mat #3: ")); 
			if(RS.RoadMaterial3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterial3 = null; }
			EditorGUILayout.EndHorizontal();
		}
		if(RS.RoadMaterial1 != null && RS.RoadMaterial2 != null && RS.RoadMaterial3 != null){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_RoadMaterial4, new GUIContent ("  Mat #4: ")); 
			if(RS.RoadMaterial4 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterial4 = null; }
			EditorGUILayout.EndHorizontal();
		}






//		//Road marker material defaults:
		GUILayout.Label("Road marker material(s) defaults:");
//		EditorGUILayout.PropertyField (t_RoadMaterialMarker1, new GUIContent ("  Mat #1: "));
//		if(RS.RoadMaterialMarker1 != null){EditorGUILayout.PropertyField (t_RoadMaterialMarker2, new GUIContent ("  Mat #2: ")); }
//		if(RS.RoadMaterialMarker2 != null){EditorGUILayout.PropertyField (t_RoadMaterialMarker3, new GUIContent ("  Mat #3: ")); }
//		if(RS.RoadMaterialMarker3 != null){EditorGUILayout.PropertyField (t_RoadMaterialMarker4, new GUIContent ("  Mat #4: ")); }

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField (t_RoadMaterialMarker1, new GUIContent ("  Mat #1: "));
		if(RS.RoadMaterialMarker1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterialMarker1 = null; }
		EditorGUILayout.EndHorizontal();
		if(RS.RoadMaterialMarker1 != null){ 
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_RoadMaterialMarker2, new GUIContent ("  Mat #2: ")); 
			if(RS.RoadMaterialMarker2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterialMarker2 = null; }
			EditorGUILayout.EndHorizontal();
		}
		if(RS.RoadMaterialMarker1 != null && RS.RoadMaterialMarker2 != null){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_RoadMaterialMarker3, new GUIContent ("  Mat #3: ")); 
			if(RS.RoadMaterialMarker3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterialMarker3 = null; }
			EditorGUILayout.EndHorizontal();
		}
		if(RS.RoadMaterialMarker1 != null && RS.RoadMaterialMarker2 != null && RS.RoadMaterialMarker3 != null){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_RoadMaterialMarker4, new GUIContent ("  Mat #4: ")); 
			if(RS.RoadMaterialMarker4 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.RoadMaterialMarker4 = null; }
			EditorGUILayout.EndHorizontal();
		}




//		//Shoulder material defaults:
		if(RS.opt_bShouldersEnabled){
			GUILayout.Label("Shoulder base material(s) defaults:");
//			EditorGUILayout.PropertyField (t_ShoulderMaterial1, new GUIContent ("  Mat #1: "));
//			if(RS.ShoulderMaterial1 != null){EditorGUILayout.PropertyField (t_ShoulderMaterial2, new GUIContent ("  Mat #2: ")); }
//			if(RS.ShoulderMaterial2 != null){EditorGUILayout.PropertyField (t_ShoulderMaterial3, new GUIContent ("  Mat #3: ")); }
//			if(RS.ShoulderMaterial3 != null){EditorGUILayout.PropertyField (t_ShoulderMaterial4, new GUIContent ("  Mat #4: ")); }

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_ShoulderMaterial1, new GUIContent ("  Mat #1: "));
			if(RS.ShoulderMaterial1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterial1 = null; }
			EditorGUILayout.EndHorizontal();
			if(RS.ShoulderMaterial1 != null){ 
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_ShoulderMaterial2, new GUIContent ("  Mat #2: ")); 
				if(RS.ShoulderMaterial2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterial2 = null; }
				EditorGUILayout.EndHorizontal();
			}
			if(RS.ShoulderMaterial1 != null && RS.ShoulderMaterial2 != null){
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_ShoulderMaterial3, new GUIContent ("  Mat #3: ")); 
				if(RS.ShoulderMaterial3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterial3 = null; }
				EditorGUILayout.EndHorizontal();
			}
			if(RS.ShoulderMaterial1 != null && RS.ShoulderMaterial2 != null && RS.ShoulderMaterial3 != null){
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_ShoulderMaterial4, new GUIContent ("  Mat #4: ")); 
				if(RS.ShoulderMaterial4 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterial4 = null; }
				EditorGUILayout.EndHorizontal();
			}
		}




//		//Shoulder marker material defaults:
		if(RS.opt_bShouldersEnabled){
			GUILayout.Label("Shoulder marker material(s) defaults:");
	//		EditorGUILayout.PropertyField (t_ShoulderMaterialMarker1, new GUIContent ("  Mat #1: "));
	//		if(RS.ShoulderMaterialMarker1 != null){EditorGUILayout.PropertyField (t_ShoulderMaterialMarker2, new GUIContent ("  Mat #2: ")); }
	//		if(RS.ShoulderMaterialMarker2 != null){EditorGUILayout.PropertyField (t_ShoulderMaterialMarker3, new GUIContent ("  Mat #3: ")); }
	//		if(RS.ShoulderMaterialMarker3 != null){EditorGUILayout.PropertyField (t_ShoulderMaterialMarker4, new GUIContent ("  Mat #4: ")); }

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_ShoulderMaterialMarker1, new GUIContent ("  Mat #1: "));
			if(RS.ShoulderMaterialMarker1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterialMarker1 = null; }
			EditorGUILayout.EndHorizontal();
			if(RS.ShoulderMaterialMarker1 != null){ 
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_ShoulderMaterialMarker2, new GUIContent ("  Mat #2: ")); 
				if(RS.ShoulderMaterialMarker2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterialMarker2 = null; }
				EditorGUILayout.EndHorizontal();
			}
			if(RS.ShoulderMaterialMarker1 != null && RS.ShoulderMaterialMarker2 != null){
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_ShoulderMaterialMarker3, new GUIContent ("  Mat #3: ")); 
				if(RS.ShoulderMaterialMarker3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterialMarker3 = null; }
				EditorGUILayout.EndHorizontal();
			}
			if(RS.ShoulderMaterialMarker1 != null && RS.ShoulderMaterialMarker2 != null && RS.ShoulderMaterialMarker3 != null){
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_ShoulderMaterialMarker4, new GUIContent ("  Mat #4: ")); 
				if(RS.ShoulderMaterialMarker4 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ RS.ShoulderMaterialMarker4 = null; }
				EditorGUILayout.EndHorizontal();
			}
		}



		//Physics materials:
		GUILayout.Label("Physics materials defaults:");
		//Option: physical road mat:
//		t_RoadPhysicMaterial.serializedObject = (PhysicMaterial)EditorGUILayout.ObjectField("  Road mat: ",RS.RoadPhysicMaterial,typeof(PhysicMaterial),false);
		EditorGUILayout.PropertyField (t_RoadPhysicMaterial, new GUIContent ("Road phys mat: ")); 


		//Option: physical shoulder mat:
//		t_ShoulderPhysicMaterial.serializedObject = (PhysicMaterial)EditorGUILayout.ObjectField("  Shoulder mat: ",RS.ShoulderPhysicMaterial,typeof(PhysicMaterial),false);
		EditorGUILayout.PropertyField (t_ShoulderPhysicMaterial, new GUIContent ("Shoulder phys mat: ")); 
		
		
		GUILayout.Space(4);
		EditorGUILayout.BeginHorizontal();
		//Option: Apply above materials to entire road:
		EditorGUILayout.LabelField("Apply above materials to entire road:");
		bApplyMatsCheck = EditorGUILayout.Toggle(bApplyMatsCheck,GUILayout.Width(20f));
		if(GUILayout.Button("Apply", EditorStyles.miniButton,GUILayout.Width(60f))){
			if(bApplyMatsCheck){
				t_bApplyMatsCheck = true;
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Applying will overwrite any saved cuts' material(s).");
		
		//Help toggle for materials
		GUILayout.Space(4);
		bShowMatsHelp = EditorGUILayout.Foldout(bShowMatsHelp, status);
		if(bShowMatsHelp){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("These default materials will be applied by default to their respective generated meshes. If using split roads and or shoulders, you can specific specific materials to use on them (on the mesh renderers of the cuts) and they will be used instead of the default materials listed above.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Road base material is UV mapped on a world vector basis for seamless tiles.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Road marker material is UV mapped to fit roads. Use these materials to place road lines and other road texture details. Note: if using road cuts, these are the materials which will be placed by default at the initial generation.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Shoulder base material is UV mapped on a world vector basis for seamless tiles.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Shoulder marker material is UV mapped on a world vector basis for seamless tiles. For intended use with transparent shadow receiving shaders. Marker materials are applied, optionally, on shoulder cuts.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("The physical material for road will be used on all road colliders. The physical material for shoulder will be used on all shoulder colliders. If using road and or shoulder cuts, you can specficy unique physics materials which will be used instead of the default physics materials.");
			GUILayout.Space(4f);
			EditorGUILayout.LabelField("Apply above materials button will clear all saved materials on the roads and all road and shoulder meshes will use the materials listed above.");
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				
			}
			EditorGUILayout.LabelField(" = Resets settings to default.");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();

		//Reset terrain history:
		Line();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Reset road's terrain history:");
		bResetTH = EditorGUILayout.Toggle(bResetTH,GUILayout.Width(20f));
		if(bResetTH){
			if(GUILayout.Button("Reset") && bResetTH){
				RS.ConstructRoad_ResetTerrainHistory();
				bResetTH = false;
			}
		}else{
			if(GUILayout.Button("Check to reset",GSDMaybeButton) && bResetTH){
				RS.ConstructRoad_ResetTerrainHistory();
				bResetTH = false;
			}
		}
		EditorGUILayout.EndHorizontal();
		
		if(bResetTH){
			EditorGUILayout.LabelField("WARNING: This option can't be undone! Only reset the terrain history if you have changed terrain resolution data such as heightmap or detail resolutions. A rare event may occur when editing and compiling this addon's scripts that a terrain history reset may be necessary. Treat this reset as a last resort.",WarningLabelStyle);
		} 
		GUILayout.Space(6f);
		
		
		EditorGUILayout.LabelField("Statistics:");
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Length: " + RS.GSDSpline.distance.ToString("F1") + " meters");
		EditorGUILayout.LabelField("Total nodes: " + RS.MostRecentNodeCount.ToString());
		EditorGUILayout.EndVertical();
		
		bool bGizmoChange = false;
		bool bLaneChange = false;
		bool bMultithreadChange = false;
		bool bSaveMeshChange = false;
		bool bTerrainHistoryChange = false;
		bool bEditorCameraSpeedChange = false;
		
		if(GUI.changed){
			//Option pre-handle: Gizmos:
			if(t_opt_GizmosEnabled.boolValue != RS.opt_GizmosEnabled){
				bGizmoChange = true;
				RS.Wireframes_Toggle();
				SceneView.RepaintAll();
			}
			//Option pre-handle: Lane count:
			if(t_opt_Lanes.intValue != RS.opt_Lanes){
				bNeedRoadUpdate = true;
				bLaneChange = true;
			}
			
			//Option pre-handle for multithread and save mesh:
			if(RS.GSDRS != null){
				if(t_opt_bMultithreading.boolValue != RS.GSDRS.opt_bMultithreading){
					RS.GSDRS.opt_bMultithreading = t_opt_bMultithreading.boolValue;
					bMultithreadChange = true;
				}
				if(t_opt_bSaveMeshes.boolValue != RS.GSDRS.opt_bSaveMeshes){
					RS.GSDRS.opt_bSaveMeshes = t_opt_bSaveMeshes.boolValue;
					bSaveMeshChange = true;
				}
			}
			
			//Option pre-handle for terrain history:
			if(t_opt_SaveTerrainHistoryOnDisk.boolValue != RS.opt_SaveTerrainHistoryOnDisk){
				bTerrainHistoryChange = true;
			}
			
			//Option pre-handle for editor camera speed:
			if(!GSDRootUtil.IsApproximately(t_EditorCameraMetersPerSecond.floatValue,RS.EditorCameraMetersPerSecond,0.001f)){ 
				bEditorCameraSpeedChange = true;
			}
			
			//Apply serialization:
			serializedObject.ApplyModifiedProperties();
			
			
			//Handle after effects:
			if(bGizmoChange){
				RS.Wireframes_Toggle();
				SceneView.RepaintAll();
			}
			
			
			//Option: Lane count:
			if(bLaneChange){
				if(RS.opt_UseDefaultMaterials){
					RS.GSDSpline.ClearAllRoadCuts();
					RS.SetDefaultMats();
					RS.SetAllCutsToCurrentMaterials();
				}
			}
			
			//Option: Multithreading
			if(bMultithreadChange){
				RS.GSDRS.UpdateAllRoads_MultiThreadOptions();
			}
			
			//Option: Save meshes as unity assets options:
			if(bSaveMeshChange){
				RS.GSDRS.UpdateAllRoads_SaveMeshesAsAssetsOptions();
			}
			
			//Option: terrain history save type:
			if(bTerrainHistoryChange){
				if(RS.opt_SaveTerrainHistoryOnDisk){
					RS.ConstructRoad_StoreTerrainHistory(true);
				}else{
					RS.ConstructRoad_LoadTerrainHistory(true);
				}
			}
			
			//Option: Editor camera meters per sec
			if(bEditorCameraSpeedChange){ 
				RS.ChangeEditorCameraMetersPerSec(); 
			}
			
			//Update road:
			if(bNeedRoadUpdate){
				RS.GSDSpline.Setup_Trigger();
			}

			//Option: Set mats to defaults:
			if(bSetDefaultMats){
				RS.SetDefaultMats();
				RS.SetAllCutsToCurrentMaterials();
			}
			
			//Option: Apply above materials to entire road:
			if(t_bApplyMatsCheck){
				t_bApplyMatsCheck = false;
				bApplyMatsCheck = false;
				RS.SetAllCutsToCurrentMaterials();
			}
			
			EditorUtility.SetDirty(target);
		}
	}

    public void OnSceneGUI() {
		Event current = Event.current;
		int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

		if(current.alt == true){ return; }
		
		if(Selection.Contains(RS.transform.gameObject) && Selection.objects.Length > 1){
			SetSelectionToRoad();
		}

		// Handle Ctrl and Shift when road is selected
		if(Selection.activeGameObject == RS.transform.gameObject){
			RS.Editor_bSelected = true;
			// Only handle MouseMove and MouseDrag events
			if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag){
				if (current.control){
					Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
					RaycastHit hitInfo;
					if (Physics.Raycast (worldRay, out hitInfo)){
						if(hitInfo.collider.transform.GetComponent<Terrain>() != null || hitInfo.collider.transform.name.ToLower().Contains("terrain")){
							RS.Editor_MousePos = hitInfo.point;
							RS.Editor_MouseTerrainHit = true;
							if(RS.GSDSpline && RS.GSDSpline.PreviewSpline){
								//Debug.Log("Drawing new node");
								if(RS.GSDSpline.PreviewSpline.mNodes == null || RS.GSDSpline.PreviewSpline.mNodes.Count < 1){ RS.GSDSpline.Setup(); }
								RS.GSDSpline.PreviewSpline.MousePos = hitInfo.point;
								RS.GSDSpline.PreviewSpline.bGizmoDraw = true;
								SceneView.RepaintAll();
							}
						}else{
							RS.Editor_MouseTerrainHit = false;	
						}
					}
					
					GUI.changed = true;
				}else if(current.shift){
					Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
					RaycastHit hitInfo;
					if (Physics.Raycast (worldRay, out hitInfo)){
						if(hitInfo.collider.transform.GetComponent<Terrain>() != null){
	//					if(hitInfo.collider.transform.name.ToLower().Contains("terrain")){
							RS.Editor_MousePos = hitInfo.point;
							RS.Editor_MouseTerrainHit = true;
							if(RS.GSDSpline && RS.GSDSpline.PreviewSplineInsert){
								//Debug.Log("Drawing insert node");
								if(RS.GSDSpline.PreviewSplineInsert.mNodes == null || RS.GSDSpline.PreviewSplineInsert.mNodes.Count < 1){ RS.GSDSpline.PreviewSplineInsert.DetermineInsertNodes(); }
								RS.GSDSpline.PreviewSplineInsert.MousePos = hitInfo.point;
								RS.GSDSpline.PreviewSplineInsert.bGizmoDraw = true;
								RS.GSDSpline.PreviewSplineInsert.UpdateActionNode();
								SceneView.RepaintAll();
							}
						}else{
							RS.Editor_MouseTerrainHit = false;	
						}
					}
					
					GUI.changed = true;
				}else{
					if(RS.Editor_MouseTerrainHit){ RS.Editor_MouseTerrainHit = false; GUI.changed = true; }	
					if(RS.GSDSpline && RS.GSDSpline.PreviewSpline){
						//Debug.Log("not drawing new node");
						RS.GSDSpline.PreviewSpline.bGizmoDraw = false;
					}
					if(RS.GSDSpline && RS.GSDSpline.PreviewSplineInsert){
						//Debug.Log("not drawing insert node");
						RS.GSDSpline.PreviewSplineInsert.bGizmoDraw = false;
					}
				}
			}
		}else{
			RS.Editor_bSelected = false;
            if (RS.GSDSpline.PreviewSpline){
                RS.GSDSpline.PreviewSpline.bGizmoDraw = false;
            }
		}
		
		
		
		if(current.shift && RS.GSDSpline.PreviewSpline != null){ RS.GSDSpline.PreviewSpline.bGizmoDraw = false; }
		bool bUsed = false;
		if(current.control){
			if (Event.current.type == EventType.MouseDown){
				//Left click:
				if (Event.current.button == 0){
					if(RS.Editor_MouseTerrainHit){
//						if((EditorApplication.timeSinceStartup - RS.GSDSpline.EditorOnly_LastNode_TimeSinceStartup) > 0.05){
//							RS.GSDSpline.EditorOnly_LastNode_TimeSinceStartup = EditorApplication.timeSinceStartup;
							Event.current.Use();
							GSDConstruction.CreateNode(RS);
							bUsed = true;
//						}
					}else{
						Debug.Log("Invalid surface for new node. Must be terrain.");
					}
				}
			}
		}else if(current.shift){
			if (Event.current.type == EventType.MouseDown){
				//Left click:
				if (Event.current.button == 0){
					if(RS.Editor_MouseTerrainHit){
						Event.current.Use();
						GSDConstruction.InsertNode(RS);
						bUsed = true;
					}else{
						Debug.Log("Invalid surface for insertion node. Must be terrain.");
					}
				}
			}
		}
		
		
		if(current.type == EventType.ValidateCommand){
		    switch (current.commandName){
			    case "UndoRedoPerformed":
			   		TriggerRoadUpdate();
			    break;
		    }
	    }
		
		if(Selection.activeGameObject == RS.transform.gameObject){
			if(current.keyCode == KeyCode.F5){
				TriggerRoadUpdate();
			}
		}
		

		
		if(bUsed){
			SetSelectionToRoad();
			switch(current.type){
				case EventType.Layout:
			        HandleUtility.AddDefaultControl(controlID);
			    break;
			}
		}
		
		if(GUI.changed){ 
			EditorUtility.SetDirty(RS); 
		}
    }
	
	
	private void TriggerRoadUpdate(){
		if(RS != null){
			RS.EditorUpdateMe = true;
		}
	}
	
	void SetSelectionToRoad(){
		GameObject[] tObjs = new GameObject[1];
		tObjs[0] = RS.transform.gameObject;
		Selection.objects = tObjs;
	}
	
	#region "Progress bar"
	/// <summary>
	/// Creates progress bar.
	/// </summary>
	/// <param name='tV'>
	/// Value of the progress bar.
	/// </param>
	/// <param name='tL'>
	/// Label of the progress bar.
	/// </param>
	void GSDProgressBar (float tV, string tL) {
		// Get a rect for the progress bar using the same margins as a textfield:
		Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
		EditorGUI.ProgressBar (rect, tV, tL);
		EditorGUILayout.Space ();
	}
	#endregion
	
	void Line(){
		GUILayout.Space(4f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f)); //Horizontal bar
		GUILayout.Space(4f);
	}
	
	void LineSmall(){
		GUILayout.Space(1f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f)); //Horizontal bar
		GUILayout.Space(1f);
	}
	

}
