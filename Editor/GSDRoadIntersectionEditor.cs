using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GSD;

[CustomEditor(typeof(GSDRoadIntersection))] 
public class GSDRoadIntersectionEditor : Editor {
	protected GSDRoadIntersection tInter { get { return (GSDRoadIntersection) target; } }
	
	SerializedProperty t_opt_AutoUpdateIntersections;
	SerializedProperty t_bDrawGizmo;
	SerializedProperty t_bLeftTurnYieldOnGreen;
	SerializedProperty t_opt_FixedTime_RegularLightLength;
	SerializedProperty t_opt_FixedTime_LeftTurnLightLength;
	SerializedProperty t_opt_FixedTime_AllRedLightLength;
	SerializedProperty t_opt_FixedTime_YellowLightLength;
	SerializedProperty t_bTrafficPoleStreetLight;
	SerializedProperty t_bTrafficLightGray;
	SerializedProperty t_bRegularPoleAlignment;
	SerializedProperty t_bLightsEnabled;
	SerializedProperty t_StreetLight_Range;
	SerializedProperty t_StreetLight_Intensity;
	SerializedProperty t_StreetLight_Color;
	SerializedProperty t_ScalingSense;
	SerializedProperty t_GradeMod;
	SerializedProperty t_bUseDefaultMaterials;
	SerializedProperty t_MarkerCenter1;
	SerializedProperty t_MarkerCenter2;
	SerializedProperty t_MarkerCenter3;
	SerializedProperty t_MarkerExt_Stretch1;
	SerializedProperty t_MarkerExt_Stretch2;
	SerializedProperty t_MarkerExt_Stretch3;
	SerializedProperty t_MarkerExt_Tiled1;
	SerializedProperty t_MarkerExt_Tiled2;
	SerializedProperty t_MarkerExt_Tiled3;
	SerializedProperty t_Lane0Mat1;
	SerializedProperty t_Lane0Mat2;
	SerializedProperty t_Lane1Mat1;
	SerializedProperty t_Lane1Mat2;
	SerializedProperty t_Lane2Mat1;
	SerializedProperty t_Lane2Mat2;
	SerializedProperty t_Lane3Mat1;
	SerializedProperty t_Lane3Mat2;
	SerializedProperty t_rType;
	SerializedProperty t_iStopType;
	SerializedProperty t_lType;
	
	#region "Editor only variables"
	//Editor only variables
	const bool bDebug = false;
	bool bShowMarkerStretch = false;
	bool bShowMarkerTiled = false;
	bool bShowMarkerCenter = false;
	bool bShowTLSense = false;
	bool bShowTLPole = false;
	bool bShowLightHelp = false;
	bool bShowLanes = false;
	bool bGradeCorrect = false;
	bool bShowDefaultMatHelp = false;
	bool bShowHelpLeftTurnGreen = false;
	string status = "Show help";
	
	GUIStyle GSDImageButton = null;
	Texture btnRefreshText = null;
	Texture btnDeleteText = null;
	
	private static string[] rTypeDescriptions = new string[]{
		"No turn lanes",
		"Left turn lane only",
		"Both left and right turn lanes"
	};
	
	private static string[] rTypeDescriptions_3Way = new string[]{
		"No turn lanes",
		"Left turn lane only"
	};
	
	private static string[] iStopTypeEnumDescriptions = new string[]{
		"Stop signs",
		"Traffic lights",
		"None"
//		"Traffic lights #2"
	};
	
	private static string[] iTrafficLightSequenceTypeDesc = new string[]{
		"Fixed time",
		"Other"
	};
	
	const string HelpText1 = "Each material added is rendered on top of the previous. Combine with transparent shaders which accept shadows to allow for easy marking.";
	
	//Checkers:
	Texture2D LoadBtnBG = null;
	Texture2D LoadBtnBGGlow = null;
	
	GUIStyle GSDLoadButton = null;
	bool bHasInit = false;
	#endregion
	
	private void OnEnable() {
		t_opt_AutoUpdateIntersections 		= serializedObject.FindProperty("opt_AutoUpdateIntersections");
		t_bDrawGizmo 						= serializedObject.FindProperty("bDrawGizmo");
		t_bLeftTurnYieldOnGreen 			= serializedObject.FindProperty("bLeftTurnYieldOnGreen");
		t_opt_FixedTime_RegularLightLength 	= serializedObject.FindProperty("opt_FixedTime_RegularLightLength");
		t_opt_FixedTime_LeftTurnLightLength = serializedObject.FindProperty("opt_FixedTime_LeftTurnLightLength");
		t_opt_FixedTime_AllRedLightLength	= serializedObject.FindProperty("opt_FixedTime_AllRedLightLength");
		t_opt_FixedTime_YellowLightLength 	= serializedObject.FindProperty("opt_FixedTime_YellowLightLength");
		t_bTrafficPoleStreetLight 			= serializedObject.FindProperty("bTrafficPoleStreetLight");
		t_bTrafficLightGray 		= serializedObject.FindProperty("bTrafficLightGray");
		t_bRegularPoleAlignment 	= serializedObject.FindProperty("bRegularPoleAlignment");
		t_bLightsEnabled 			= serializedObject.FindProperty("bLightsEnabled");
		t_StreetLight_Range 		= serializedObject.FindProperty("StreetLight_Range");
		t_StreetLight_Intensity 	= serializedObject.FindProperty("StreetLight_Intensity");
		t_StreetLight_Color 		= serializedObject.FindProperty("StreetLight_Color");
		t_ScalingSense 				= serializedObject.FindProperty("ScalingSense");
		t_GradeMod 					= serializedObject.FindProperty("GradeMod");
		t_bUseDefaultMaterials 		= serializedObject.FindProperty("bUseDefaultMaterials");
		t_MarkerCenter1				= serializedObject.FindProperty("MarkerCenter1");
		t_MarkerCenter2				= serializedObject.FindProperty("MarkerCenter2");
		t_MarkerCenter3				= serializedObject.FindProperty("MarkerCenter3");
		t_MarkerExt_Stretch1		= serializedObject.FindProperty("MarkerExt_Stretch1");
		t_MarkerExt_Stretch2		= serializedObject.FindProperty("MarkerExt_Stretch2");
		t_MarkerExt_Stretch3		= serializedObject.FindProperty("MarkerExt_Stretch3");
		t_MarkerExt_Tiled1			= serializedObject.FindProperty("MarkerExt_Tiled1");
		t_MarkerExt_Tiled2			= serializedObject.FindProperty("MarkerExt_Tiled2");
		t_MarkerExt_Tiled3			= serializedObject.FindProperty("MarkerExt_Tiled3");
		t_Lane0Mat1					= serializedObject.FindProperty("Lane0Mat1");
		t_Lane0Mat2					= serializedObject.FindProperty("Lane0Mat2");
		t_Lane1Mat1					= serializedObject.FindProperty("Lane1Mat1");
		t_Lane1Mat2					= serializedObject.FindProperty("Lane1Mat2");
		t_Lane2Mat1					= serializedObject.FindProperty("Lane2Mat1");
		t_Lane2Mat2					= serializedObject.FindProperty("Lane2Mat2");
		t_Lane3Mat1					= serializedObject.FindProperty("Lane3Mat1");
		t_Lane3Mat2					= serializedObject.FindProperty("Lane3Mat2");
		t_rType						= serializedObject.FindProperty("rType");
		t_iStopType					= serializedObject.FindProperty("iStopType");
		t_lType						= serializedObject.FindProperty("lType");
	}
	
	public override void OnInspectorGUI(){
		if(Event.current.type == EventType.ValidateCommand){
			switch (Event.current.commandName){
			case "UndoRedoPerformed":
				TriggerRoadUpdate(true);
				break;
			}
		}

		serializedObject.Update();
		
		//Graphic null checks:
		if(!bHasInit){ Init(); }

		Line ();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Intersection options",EditorStyles.boldLabel);
		if(GUILayout.Button("Update intersection",GSDLoadButton)){
			TriggerRoadUpdate(true);
		}
		EditorGUILayout.EndHorizontal();

		//Option: Auto update:
		t_opt_AutoUpdateIntersections.boolValue = EditorGUILayout.Toggle("Auto-update:",tInter.opt_AutoUpdateIntersections);
		
		//Option: Gizmo:
		t_bDrawGizmo.boolValue = EditorGUILayout.Toggle("Gizmo:",tInter.bDrawGizmo);

		//UI:
		EditorGUILayout.BeginHorizontal();
		if(tInter.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){
			EditorGUILayout.LabelField("Intersection type: 3 way");
		}else{
			EditorGUILayout.LabelField("Intersection type: 4 way");	
		}
		if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
			Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
		}
		EditorGUILayout.EndHorizontal();
		
		//Option: Intersection turn lane options
		Line();
		EditorGUILayout.LabelField("Intersection turn lane options:");
		if(tInter.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){
			t_rType.enumValueIndex = (int)EditorGUILayout.Popup((int)tInter.rType, rTypeDescriptions_3Way);
		}else{
			t_rType.enumValueIndex = (int)EditorGUILayout.Popup((int)tInter.rType, rTypeDescriptions);
		}
		
		//Option: Left yield on green:
		if(tInter.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
			t_bLeftTurnYieldOnGreen.boolValue = EditorGUILayout.Toggle("Left yield on green: ",tInter.bLeftTurnYieldOnGreen);

			bShowHelpLeftTurnGreen = EditorGUILayout.Foldout(bShowHelpLeftTurnGreen, status);
			if(bShowHelpLeftTurnGreen){
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("Left yield on green: If checked, replaces the standard 3-light left turn light with a five-light yield on green left turn signal structure and sign.");
				EditorGUILayout.EndVertical();
			}
		}
		Line();
		
		
		//Option: Intersection stop type:
		t_iStopType.enumValueIndex = (int)EditorGUILayout.Popup("Intersection stop type:",(int)tInter.iStopType, iStopTypeEnumDescriptions);
		
		
		if(tInter.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || tInter.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
			//Option: Traffic light timing type:
			t_lType.enumValueIndex = (int)EditorGUILayout.Popup("Traffic light timing:",(int)tInter.lType, iTrafficLightSequenceTypeDesc);
			
			//Options: Traffic fixed light timings:
			if(tInter.lType == GSDRoadIntersection.LightTypeEnum.Timed){
				EditorGUILayout.LabelField("Traffic light fixed time lengths (in seconds):");
				EditorGUILayout.BeginHorizontal();
				t_opt_FixedTime_RegularLightLength.floatValue = EditorGUILayout.Slider("Green length: ",tInter.opt_FixedTime_RegularLightLength,0.1f,180f);
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){ t_opt_FixedTime_RegularLightLength.floatValue = 30f; }
				EditorGUILayout.EndHorizontal();
				
				if(tInter.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
					EditorGUILayout.BeginHorizontal();
					t_opt_FixedTime_LeftTurnLightLength.floatValue = EditorGUILayout.Slider("Left turn only length: ",tInter.opt_FixedTime_LeftTurnLightLength,0.1f,180f);
					if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){ t_opt_FixedTime_LeftTurnLightLength.floatValue = 10f; }
					EditorGUILayout.EndHorizontal();
				}
				
				EditorGUILayout.BeginHorizontal();
				t_opt_FixedTime_AllRedLightLength.floatValue = EditorGUILayout.Slider("All red length: ",tInter.opt_FixedTime_AllRedLightLength,0.1f,180f);
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){ t_opt_FixedTime_AllRedLightLength.floatValue = 1f; }
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				t_opt_FixedTime_YellowLightLength.floatValue = EditorGUILayout.Slider("Yellow light length: ",tInter.opt_FixedTime_YellowLightLength,0.1f,180f);
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){ t_opt_FixedTime_YellowLightLength.floatValue = 3f; }
				EditorGUILayout.EndHorizontal();
			}
		}
		
		
		if(tInter.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || tInter.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
			//Option: Traffic light poles:
			Line();
			EditorGUILayout.LabelField("Traffic light poles:");
			t_bTrafficPoleStreetLight.boolValue = EditorGUILayout.Toggle("Street lights: ", tInter.bTrafficPoleStreetLight);
			
			//Option: Traffic light pole gray color:
			t_bTrafficLightGray.boolValue = EditorGUILayout.Toggle("Gray color: ", tInter.bTrafficLightGray);
			
			//Option: Normal pole alignment:
			t_bRegularPoleAlignment.boolValue = EditorGUILayout.Toggle("Normal pole alignment: ",tInter.bRegularPoleAlignment);
			bShowTLPole = EditorGUILayout.Foldout(bShowTLPole, status);
			if(bShowTLPole){
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("Street lights: If checked, attaches street lights to each intersection pole. Point lights optional and can be manipulated in the next option segment.");
				GUILayout.Space(4f);
				EditorGUILayout.LabelField("Gray color: If checked, sets the traffic light pole base materials to gray galvanized steel. If unchecked the material used is black metal paint.");
				GUILayout.Space(4f);
				EditorGUILayout.LabelField("Normal pole alignment: Recommended to keep this option on unless abnormal twisting of light objects is occurring. Turn this option off if the roads immediately surrounding your intersection are curved at extreme angles and cause irregular twisting of the traffic light objects. Turning this option off will attempt to align the poles perpendicular to the adjacent relevant road without any part existing over the main intersection bounds.");
				EditorGUILayout.EndVertical();
			}
			
			//Option: Point lights enabled:
			Line();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Point lights:");
			EditorGUILayout.EndHorizontal();
			t_bLightsEnabled.boolValue = EditorGUILayout.Toggle("  Point lights enabled: ", tInter.bLightsEnabled);
			
			//Options: Street point light options:
			if(tInter.bTrafficPoleStreetLight){
				//Option: Street light range:
				EditorGUILayout.BeginHorizontal();
				t_StreetLight_Range.floatValue = EditorGUILayout.Slider("  Street light range: ",tInter.StreetLight_Range,1f,128f);
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
					t_StreetLight_Range.floatValue = 30f;
				}
				EditorGUILayout.EndHorizontal();
				
				//Option: Street light intensity:
				EditorGUILayout.BeginHorizontal();
				t_StreetLight_Intensity.floatValue = EditorGUILayout.Slider("  Street light intensity: ",tInter.StreetLight_Intensity,0f,8f);
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
					t_StreetLight_Intensity.floatValue = 1f;
				}
				EditorGUILayout.EndHorizontal();
				
				//Option: Street light color:
				EditorGUILayout.BeginHorizontal();
				t_StreetLight_Color.colorValue = EditorGUILayout.ColorField("  Street light color: ",tInter.StreetLight_Color);
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
					t_StreetLight_Color.colorValue = new Color(1f,0.7451f,0.27451f,1f);;
				}
				EditorGUILayout.EndHorizontal();
			}
			bShowLightHelp = EditorGUILayout.Foldout(bShowLightHelp, status);
			if(bShowLightHelp){
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("Point lights: Enabled means that point lights for the traffic lights (and street lights, if enabled) will be turned on. This is accessible via script \"bLightsEnabled\"");
			
				GUILayout.Space(4f);
				EditorGUILayout.LabelField("If street pole lights enabled: Street light range, intensity and color: These settings directly correlate to the standard point light settings.");
				
				GUILayout.Space(4f);
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
					
				}
				EditorGUILayout.LabelField(" = Resets settings to default.");
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
			Line();
			
			//Option: Traffic light scaling sensitivity:
			EditorGUILayout.LabelField("Traffic light scaling sensitivity: *Does not auto-update");
			EditorGUILayout.BeginHorizontal();
			t_ScalingSense.floatValue = EditorGUILayout.Slider(tInter.ScalingSense,0f,200f);
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				t_ScalingSense.floatValue = 170f;
			}
			EditorGUILayout.EndHorizontal(); GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			bShowTLSense = EditorGUILayout.Foldout(bShowTLSense, status);
			if(GUILayout.Button("Manually update intersection",EditorStyles.miniButton,GUILayout.Width(170f))){
				TriggerRoadUpdate(true);	
			}
			EditorGUILayout.EndHorizontal();
			if(bShowTLSense){
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("Increasing this value will increase the scaling sensitivity relative to the size of the intersection. Higher scaling value = bigger traffic lights at further distances. Default value is 170.");
				GUILayout.Space(4f);
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
					
				}
				EditorGUILayout.LabelField(" = Resets settings to default.");
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
			GUILayout.Space(4f);
		}
	
		//Option: Grade correction modifier:
		Line();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Grade correction factor: *Does not auto-update");
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		t_GradeMod.floatValue = EditorGUILayout.Slider(tInter.GradeMod,0.01f,2f);
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			t_GradeMod.floatValue = 0.375f;	
		}
		EditorGUILayout.EndHorizontal(); 
		EditorGUILayout.BeginHorizontal();
		bGradeCorrect = EditorGUILayout.Foldout(bGradeCorrect, status);
		if(GUILayout.Button("Manually update intersection",EditorStyles.miniButton,GUILayout.Width(170f))){
			tInter.UpdateRoads();
		}
		EditorGUILayout.EndHorizontal(); 
		if(bGradeCorrect){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("If using extreme road grades immediately surrounding the intersection, terrain height matching errors may occur at the point of road expansion leading to the intersection.");
			EditorGUILayout.LabelField("Raise this value if you see terrain poking through the road at the road expansion areas.");
			EditorGUILayout.LabelField("Lower this value if you are not using road mesh colliders and notice dips at the expansion points.");
			EditorGUILayout.LabelField("Default value is 0.375 meters, which is the maximum value for a linear range of 0% to 20% grade.");
			EditorGUILayout.LabelField("Recommended to keep grades and angles small leading up to intersections.");
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				
			}
			EditorGUILayout.LabelField(" = Resets settings to default.");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		
		//Option: Use default materials:
		Line();
		t_bUseDefaultMaterials.boolValue = EditorGUILayout.Toggle("Use default materials:",tInter.bUseDefaultMaterials);
		bShowDefaultMatHelp = EditorGUILayout.Foldout(bShowDefaultMatHelp, status);
		if(bShowDefaultMatHelp){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Checking this option will reset all materials for this intersection and use the default intersection materials that come with this addon.");
			EditorGUILayout.EndVertical();
		}
		
		
		Line();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Center marker material(s):");
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			tInter.ResetMaterials_Center();	
		}
		EditorGUILayout.EndHorizontal(); GUILayout.Space(4f);
		//Options: Center materials:
		//		EditorGUILayout.BeginHorizontal();
		//		EditorGUILayout.PropertyField (t_MarkerCenter1, new GUIContent ("Center material #1:"));
		//		if(tInter.MarkerCenter1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ t_MarkerCenter1 = null; }
		//		EditorGUILayout.EndHorizontal();
		//		if(tInter.MarkerCenter1 != null){
		//			EditorGUILayout.BeginHorizontal();
		//			EditorGUILayout.PropertyField (t_MarkerCenter2, new GUIContent ("Center material #2:"));
		//			if(tInter.MarkerCenter2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ t_MarkerCenter2 = null; }
		//			EditorGUILayout.EndHorizontal();
		//			if(tInter.MarkerCenter2 != null){
		//				EditorGUILayout.BeginHorizontal();
		//				EditorGUILayout.PropertyField (t_MarkerCenter3, new GUIContent ("Center material #3:"));
		//				if(tInter.MarkerCenter3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ t_MarkerCenter3 = null; }
		//				EditorGUILayout.EndHorizontal();
		//			}
		//		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField (t_MarkerCenter1, new GUIContent ("  Mat #1: "));
		if(tInter.MarkerCenter1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerCenter1 = null; }
		EditorGUILayout.EndHorizontal();
		
		if(tInter.MarkerCenter1 != null){ 
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_MarkerCenter2, new GUIContent ("  Mat #2: ")); 
			if(tInter.MarkerCenter2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerCenter2 = null; }
			EditorGUILayout.EndHorizontal();
		}
		if(tInter.MarkerCenter2 != null && tInter.MarkerCenter1 != null){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_MarkerCenter3, new GUIContent ("  Mat #3: ")); 
			if(tInter.MarkerCenter3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerCenter3 = null; }
			EditorGUILayout.EndHorizontal();
		}
		
		bShowMarkerCenter = EditorGUILayout.Foldout(bShowMarkerCenter, status);
		if(bShowMarkerCenter){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Center marker materials require transparent shaders. Covers the center area of the intersection. Displayed in order #1 on bottom to #4 on top. Combine with transparent shaders which accept shadows to allow for easy marking.");
			DoDefaultHelpMat();
			DoDeleteHelpMat();
			EditorGUILayout.EndVertical();
		}
		Line();
		
		//Options: Marker ext stretched materials:
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Exterior fitted marker material(s):");
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			tInter.ResetMaterials_Ext_Stretched();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField (t_MarkerExt_Stretch1, new GUIContent ("  Mat #1: "));
		if(tInter.MarkerExt_Stretch1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerExt_Stretch1 = null; }
		EditorGUILayout.EndHorizontal();
		
		if(tInter.MarkerExt_Stretch1 != null){ 
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_MarkerExt_Stretch2, new GUIContent ("  Mat #2: ")); 
			if(tInter.MarkerExt_Stretch2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerExt_Stretch2 = null; }
			EditorGUILayout.EndHorizontal();
		}
		if(tInter.MarkerExt_Stretch2 != null && tInter.MarkerExt_Stretch1 != null){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_MarkerExt_Stretch3, new GUIContent ("  Mat #3: ")); 
			if(tInter.MarkerExt_Stretch3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerExt_Stretch3 = null; }
			EditorGUILayout.EndHorizontal();
		}
		
		bShowMarkerStretch = EditorGUILayout.Foldout(bShowMarkerStretch, status);
		if(bShowMarkerStretch){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Fitted marker materials require transparent shaders. Covers the exterior area of the intersection with the UV's stretched to match at a 1:1 ratio. Should be use for intersection markings and any visual effects like dirt." + HelpText1);
			DoDefaultHelpMat();
			DoDeleteHelpMat();
			EditorGUILayout.EndVertical();
		}
		Line();
		
		//Options: Marker ext tiled materials:
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Exterior tiled marker material(s):");
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			tInter.ResetMaterials_Ext_Tiled();
		} 
		EditorGUILayout.EndHorizontal(); GUILayout.Space(4f);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField (t_MarkerExt_Tiled1, new GUIContent ("  Mat #1: "));
		if(tInter.MarkerExt_Tiled1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerExt_Tiled1 = null; }
		EditorGUILayout.EndHorizontal();
		
		if(tInter.MarkerExt_Tiled1 != null){ 
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_MarkerExt_Tiled2, new GUIContent ("  Mat #2: ")); 
			if(tInter.MarkerExt_Tiled2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerExt_Tiled2 = null; }
			EditorGUILayout.EndHorizontal();
		}
		if(tInter.MarkerExt_Tiled2 != null && tInter.MarkerExt_Tiled1 != null){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_MarkerExt_Tiled3, new GUIContent ("  Mat #3: ")); 
			if(tInter.MarkerExt_Tiled3 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.MarkerExt_Tiled3 = null; }
			EditorGUILayout.EndHorizontal();
		}
		
		bShowMarkerTiled = EditorGUILayout.Foldout(bShowMarkerTiled, status);
		if(bShowMarkerTiled){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Covers the exterior area of the intersection with the UV's tiled matching world coordinates. Tiled and used for road pavement textures. UV coordinates will match up seamlessly with road pavement." + HelpText1);
			DoDefaultHelpMat();
			DoDeleteHelpMat();
			EditorGUILayout.EndVertical();
		}
		Line();
		
		//Option: Lane section 0:
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Lanes marker materials:");
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			tInter.ResetMaterials_Lanes();
		} 
		EditorGUILayout.EndHorizontal(); GUILayout.Space(4f);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField (t_Lane0Mat1, new GUIContent ("Lane section 0 mat #1:"));
		if(tInter.Lane0Mat1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane0Mat1 = null; }
		EditorGUILayout.EndHorizontal();
		if(tInter.Lane0Mat1 != null){ 
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_Lane0Mat2, new GUIContent ("Lane section 0 mat #2:")); 
			if(tInter.Lane0Mat2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane0Mat2 = null; }
			EditorGUILayout.EndHorizontal();
		}
		
		
		//Option: Lane section 1:
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField (t_Lane1Mat1, new GUIContent ("Lane section 1 mat #1:"));
		if(tInter.Lane1Mat1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane1Mat1 = null; }
		EditorGUILayout.EndHorizontal();
		if(tInter.Lane1Mat1 != null){ 
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_Lane1Mat2, new GUIContent ("Lane section 1 mat #2:")); 
			if(tInter.Lane1Mat2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane1Mat2 = null; }
			EditorGUILayout.EndHorizontal();
		}
		
		//Option: Lane section 2:
		if(tInter.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes || tInter.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_Lane2Mat1, new GUIContent ("Lane section 2 mat #1:"));
			if(tInter.Lane2Mat1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane2Mat1 = null; }
			EditorGUILayout.EndHorizontal();
			if(tInter.Lane2Mat1 != null){ 
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_Lane2Mat2, new GUIContent ("Lane section 2 mat #2:")); 
				if(tInter.Lane2Mat2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane2Mat2 = null; }
				EditorGUILayout.EndHorizontal();
			}
		} GUILayout.Space(4f);
		
		//Option: Lane section 3:
		if(tInter.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (t_Lane3Mat1, new GUIContent ("Lane section 3 mat #1:"));
			if(tInter.Lane3Mat1 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane3Mat1 = null; }
			EditorGUILayout.EndHorizontal();
			if(tInter.Lane3Mat1 != null){ 
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField (t_Lane3Mat2, new GUIContent ("Lane section 3 mat #2:")); 
				if(tInter.Lane3Mat2 != null && GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ tInter.Lane3Mat2 = null; }
				EditorGUILayout.EndHorizontal();
			}
		}
		bShowLanes = EditorGUILayout.Foldout(bShowLanes, status);
		if(bShowLanes){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Covers individual lane sections in the intersection. Used for high-definition line marking.");
			EditorGUILayout.LabelField("Lane section 0 = Left side of the intersection where oncoming traffic travels. Use lane texture which matches the number of lanes used on the road system, with a white left line and a yellow right line.");
			
			if(tInter.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				EditorGUILayout.LabelField("Lane section 1 = Left turn lane. Use a single lane texture with a yellow left line and a white right line.");
				EditorGUILayout.LabelField("Lane section 2 = Right outgoing traffic lane. Use lane texture which matches the number of lanes used on the road system with a white right line.");
				EditorGUILayout.LabelField("Lane section 3 = Right turn lane. Use a single lane texture with a white right line.");
			}else if(tInter.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				EditorGUILayout.LabelField("Lane section 1 = Left turn lane. Use a single lane texture with a yellow left line and a white right line.");
				EditorGUILayout.LabelField("Lane section 2 = Right outgoing traffic lane. Use lane texture which matches the number of lanes used on the road system with a white right line.");
			}else if(tInter.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
				EditorGUILayout.LabelField("Lane section 1 = Right outgoing traffic lane. Use lane texture which matches the number of lanes used on the road system with a white right line.");
			}
			
			EditorGUILayout.LabelField(HelpText1);
			DoDefaultHelpMat();
			DoDeleteHelpMat();
			EditorGUILayout.EndVertical();
		}
		Line();

//		if(bDebug){
//			Line();
//			EditorGUILayout.LabelField("Debug");
//			if(tInter.Node1 != null){ EditorGUILayout.LabelField("  Node1: " + tInter.Node1.transform.name); } else { EditorGUILayout.LabelField("  Node1: null"); }
//			if(tInter.Node2 != null){ EditorGUILayout.LabelField("  Node2: " + tInter.Node2.transform.name); } else { EditorGUILayout.LabelField("  Node2: null"); }
//			if(tInter.Node1 != null){ EditorGUILayout.LabelField("  UID1: " + tInter.Node1.UID); } else { EditorGUILayout.LabelField("  UID1: null"); }
//			if(tInter.Node2 != null){ EditorGUILayout.LabelField("  UID2: " + tInter.Node2.UID); } else { EditorGUILayout.LabelField("  UID2: null"); }
//			EditorGUILayout.LabelField("  Same spline: " + tInter.bSameSpline);
//			EditorGUILayout.LabelField("  bFlipped: " + tInter.bFlipped);
//			EditorGUILayout.LabelField("  IgnoreSide: " + tInter.IgnoreSide);
//			EditorGUILayout.LabelField("  IgnoreCorner: " + tInter.IgnoreCorner);
//			
//			if(tInter.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || tInter.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
//				if(tInter.LightsRR != null){ EditorGUILayout.LabelField("  LightsRR: " + tInter.LightsRR); } else { EditorGUILayout.LabelField("  LightsRR: null"); }
//				if(tInter.LightsRR != null || tInter.LightsRR.MR_Main != null){ EditorGUILayout.LabelField("   MR_Main: " + tInter.LightsRR.MR_Main); } else { EditorGUILayout.LabelField("  LightsRR.MR_Main: null"); }
//				if(tInter.LightsRR != null || tInter.LightsRR.MR_Left != null){ EditorGUILayout.LabelField("   MR_Left: " + tInter.LightsRR.MR_Left); } else { EditorGUILayout.LabelField("  LightsRR.MR_Left: null"); }
//				if(tInter.LightsRR != null || tInter.LightsRR.MR_Right != null){ EditorGUILayout.LabelField("   MR_Right: " + tInter.LightsRR.MR_Right); } else { EditorGUILayout.LabelField("  LightsRR.MR_Right: null"); }
//				if(tInter.LightsRL != null){ EditorGUILayout.LabelField("  LightsRL: " + tInter.LightsRL); } else { EditorGUILayout.LabelField("  LightsRL: null"); }
//				if(tInter.LightsLL != null){ EditorGUILayout.LabelField("  LightsLL: " + tInter.LightsLL); } else { EditorGUILayout.LabelField("  LightsLL: null"); }
//				if(tInter.LightsLR != null){ EditorGUILayout.LabelField("  LightsLR: " + tInter.LightsLR); } else { EditorGUILayout.LabelField("  LightsLR: null"); }
//			}
//			Line();
//		}
		
		
		//Set change bools:
		bool bToggleTrafficLightPoleColor = false;
		bool bTogglePointLights = false;
		bool bGizmoChange = false;
		bool bMatChange = false;
		bool bRoadUpdate = false;
		
		if(GUI.changed){
			//Option: Gizmo:
			if(t_bDrawGizmo.boolValue != tInter.bDrawGizmo){ 
				bGizmoChange = true;
			}
			
			//Option: Intersection turn lane options
			if((int)tInter.rType != t_rType.enumValueIndex){
				bMatChange = true;
				bRoadUpdate = true;
			}
			
			//Option: Left yield on green:
			if(t_bLeftTurnYieldOnGreen.boolValue != tInter.bLeftTurnYieldOnGreen){
				bRoadUpdate = true;
			}
			
			//Option: Intersection stop type:
			if(t_iStopType.enumValueIndex != (int)tInter.iStopType){
				bRoadUpdate = true;
			}

			//Option: Traffic light poles:
			if(t_bTrafficPoleStreetLight.boolValue != tInter.bTrafficPoleStreetLight){
				bRoadUpdate = true;
			}
			
			//Option: Traffic light pole gray color:
			if(t_bTrafficLightGray.boolValue != tInter.bTrafficLightGray){
				bToggleTrafficLightPoleColor = true;
			}
			
			//Option: Normal pole alignment:
			if(t_bRegularPoleAlignment.boolValue != tInter.bRegularPoleAlignment){
				bRoadUpdate = true;
			}
			
			//Option: Point lights enabled:
			if(t_bLightsEnabled.boolValue != tInter.bLightsEnabled){
				bTogglePointLights = true;
			}
			
			//Option: Street light range:
			if(!GSDRootUtil.IsApproximately(t_StreetLight_Range.floatValue,tInter.StreetLight_Range,0.01f)){ 
				bTogglePointLights = true;
			}
			
			//Option: Street light intensity:
			if(!GSDRootUtil.IsApproximately(t_StreetLight_Intensity.floatValue,tInter.StreetLight_Intensity,0.01f)){ 
				bTogglePointLights = true;
			}
			
			//Option: Street light color:
			if(t_StreetLight_Color.colorValue != tInter.StreetLight_Color){ 
				bTogglePointLights = true;
			}
			
			//Option: Use default materials:
			if(t_bUseDefaultMaterials.boolValue != tInter.bUseDefaultMaterials){
				bMatChange = true;
			}
			
			if(t_MarkerCenter1 == null || t_MarkerCenter1.objectReferenceValue == null){ tInter.MarkerCenter1 = null; }
			if(t_MarkerCenter2 == null || t_MarkerCenter2.objectReferenceValue == null){ tInter.MarkerCenter2 = null; }
			if(t_MarkerCenter3 == null || t_MarkerCenter3.objectReferenceValue == null){ tInter.MarkerCenter3 = null; }
			if(t_MarkerExt_Stretch1 == null || t_MarkerExt_Stretch1.objectReferenceValue == null){ tInter.MarkerExt_Stretch1 = null; }
			if(t_MarkerExt_Stretch2 == null || t_MarkerExt_Stretch2.objectReferenceValue == null){ tInter.MarkerExt_Stretch2 = null; }
			if(t_MarkerExt_Stretch3 == null || t_MarkerExt_Stretch3.objectReferenceValue == null){ tInter.MarkerExt_Stretch3 = null; }
			if(t_MarkerExt_Tiled1 == null || t_MarkerExt_Tiled1.objectReferenceValue == null){ tInter.MarkerExt_Tiled1 = null; }
			if(t_MarkerExt_Tiled2 == null || t_MarkerExt_Tiled2.objectReferenceValue == null){ tInter.MarkerExt_Tiled2 = null; }
			if(t_MarkerExt_Tiled3 == null || t_MarkerExt_Tiled3.objectReferenceValue == null){ tInter.MarkerExt_Tiled3 = null; }
			if(t_Lane0Mat1 == null || t_Lane0Mat1.objectReferenceValue == null){ tInter.Lane0Mat1 = null; }
			if(t_Lane0Mat2 == null || t_Lane0Mat2.objectReferenceValue == null){ tInter.Lane0Mat2 = null; }
			if(t_Lane1Mat1 == null || t_Lane1Mat1.objectReferenceValue == null){ tInter.Lane1Mat1 = null; }
			if(t_Lane1Mat2 == null || t_Lane1Mat2.objectReferenceValue == null){ tInter.Lane1Mat2 = null; }
			if(t_Lane2Mat1 == null || t_Lane2Mat1.objectReferenceValue == null){ tInter.Lane2Mat1 = null; }
			if(t_Lane2Mat2 == null || t_Lane2Mat2.objectReferenceValue == null){ tInter.Lane2Mat2 = null; }
			if(t_Lane3Mat1 == null || t_Lane3Mat1.objectReferenceValue == null){ tInter.Lane3Mat1 = null; }
			if(t_Lane3Mat2 == null || t_Lane3Mat2.objectReferenceValue == null){ tInter.Lane3Mat2 = null; }
			
			if(tInter.MarkerCenter1 != null && t_MarkerCenter1.objectReferenceValue != tInter.MarkerCenter1){ bMatChange = true; }
			if(tInter.MarkerCenter2 != null && t_MarkerCenter2.objectReferenceValue != tInter.MarkerCenter2){ bMatChange = true; }
			if(tInter.MarkerCenter3 != null && t_MarkerCenter3.objectReferenceValue != tInter.MarkerCenter3){ bMatChange = true; }
			if(tInter.MarkerExt_Stretch1 != null && t_MarkerExt_Stretch1.objectReferenceValue != tInter.MarkerExt_Stretch1){ bMatChange = true; }
			if(tInter.MarkerExt_Stretch2 != null && t_MarkerExt_Stretch2.objectReferenceValue != tInter.MarkerExt_Stretch2){ bMatChange = true; }
			if(tInter.MarkerExt_Stretch3 != null && t_MarkerExt_Stretch3.objectReferenceValue != tInter.MarkerExt_Stretch3){ bMatChange = true; }
			if(tInter.MarkerExt_Tiled1 != null && t_MarkerExt_Tiled1.objectReferenceValue != tInter.MarkerExt_Tiled1){ bMatChange = true; }
			if(tInter.MarkerExt_Tiled2 != null && t_MarkerExt_Tiled2.objectReferenceValue != tInter.MarkerExt_Tiled2){ bMatChange = true; }
			if(tInter.MarkerExt_Tiled3 != null && t_MarkerExt_Tiled3.objectReferenceValue != tInter.MarkerExt_Tiled3){ bMatChange = true; }
			if(tInter.Lane0Mat1 != null && t_Lane0Mat1.objectReferenceValue != tInter.Lane0Mat1){ bMatChange = true; }
			if(tInter.Lane0Mat2 != null && t_Lane0Mat2.objectReferenceValue != tInter.Lane0Mat2){ bMatChange = true; }
			if(tInter.Lane1Mat1 != null && t_Lane1Mat1.objectReferenceValue != tInter.Lane1Mat1){ bMatChange = true; }
			if(tInter.Lane1Mat2 != null && t_Lane1Mat2.objectReferenceValue != tInter.Lane1Mat2){ bMatChange = true; }
			if(tInter.Lane2Mat1 != null && t_Lane2Mat1.objectReferenceValue != tInter.Lane2Mat1){ bMatChange = true; }
			if(tInter.Lane2Mat2 != null && t_Lane2Mat2.objectReferenceValue != tInter.Lane2Mat2){ bMatChange = true; }
			if(tInter.Lane3Mat1 != null && t_Lane3Mat1.objectReferenceValue != tInter.Lane3Mat1){ bMatChange = true; }
			if(tInter.Lane3Mat2 != null && t_Lane3Mat2.objectReferenceValue != tInter.Lane3Mat2){ bMatChange = true; }
			
			
			//Apply changes:
			serializedObject.ApplyModifiedProperties();
			
			
			//Apply secondary effects:
			if(bGizmoChange){
				MeshRenderer[] tMRs = tInter.transform.GetComponentsInChildren<MeshRenderer>();
				int tCount = tMRs.Length;
				for(int i=0;i<tCount;i++){
					//EditorUtility.SetSelectedWireframeHidden(tMRs[i], !tInter.bDrawGizmo);
                    EditorUtility.SetSelectedRenderState(tMRs[i],tInter.bDrawGizmo ? EditorSelectedRenderState.Wireframe : EditorSelectedRenderState.Hidden);
                    
				}
				SceneView.RepaintAll();
			}
			
			if(bTogglePointLights){
				tInter.TogglePointLights(tInter.bLightsEnabled); 
			}
			
			if(bToggleTrafficLightPoleColor){
				tInter.ToggleTrafficLightPoleColor();
			}
			
			if(bMatChange){
				tInter.UpdateMaterials();
				if(tInter.bUseDefaultMaterials){
					tInter.ResetMaterials_All();	
				}
			}
			
			if(bRoadUpdate){
				TriggerRoadUpdate();	
			}
			
			EditorUtility.SetDirty(tInter);
		}
	}
		
    public void OnSceneGUI() {
		Event current = Event.current;
		int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
		
//		if (Event.current.type == EventType.MouseDrag && Event.current.button == 0){
//			//Update relevant splines:
//			tInter.Node1.GSDSpline.Setup();
//			if(!tInter.bSameSpline){
//				tInter.Node2.GSDSpline.Setup();
//			}
//			bMouseDragHasProcessed = false;
//		}
//		
//		if(Event.current.type == EventType.MouseUp && Event.current.button == 0){
//			if(!bMouseDragHasProcessed){
//				tInter.Node1.transform.position = tInter.transform.position;
//				tInter.Node2.transform.position = tInter.transform.position;
//				tInter.StartCoroutine(TriggerRoadUpdate());
//			} 
//			bMouseDragHasProcessed = true;
//		}
		
		if(Event.current.type == EventType.MouseUp && Event.current.button == 0){
			if(tInter.transform.position != tInter.Node1.transform.position){
				tInter.Node1.transform.position = tInter.transform.position;	
				tInter.Node2.transform.position = tInter.transform.position;
				tInter.Height = tInter.transform.position.y;
				TriggerRoadUpdate();
			}
		}

		
		
		if(current.type == EventType.ValidateCommand){
		    switch (current.commandName){
			    case "UndoRedoPerformed":
			   		TriggerRoadUpdate(true);
			    break;
		    }
	    }
		
		if(current.type == EventType.MouseDown){
			return;
		}
		
		if(Selection.activeGameObject == tInter.transform.gameObject){
			if(current.keyCode == KeyCode.F5){
				if(tInter != null && tInter.Node1 != null && tInter.Node2 != null){
					TriggerRoadUpdate();
				}
			}
		}
		
//		switch(current.type){
//			case EventType.layout:
////			if(current.type != EventType.MouseDown){
//		        HandleUtility.AddDefaultControl(controlID);
////			}
//		    break;
//				
//		}
		
		if(controlID == 1){
			//Do nothing	
		}
		if(GUI.changed){ EditorUtility.SetDirty(tInter); }
    }
	
	private void TriggerRoadUpdate(bool bForce = false){
		if(!bForce && !tInter.opt_AutoUpdateIntersections){ return; }
		
		if(tInter != null && tInter.Node1 != null && tInter.Node2 != null){
			if(!tInter.Node1.GSDSpline.tRoad.Editor_bIsConstructing && !tInter.Node2.GSDSpline.tRoad.Editor_bIsConstructing){
				if(!tInter.bSameSpline){
					tInter.Node1.GSDSpline.tRoad.PiggyBacks = new GSDSplineC[1];
					tInter.Node1.GSDSpline.tRoad.PiggyBacks[0] = tInter.Node2.GSDSpline;
				}
				tInter.Node1.GSDSpline.tRoad.EditorUpdateMe = true;
			}
		}
	}
	
	void Init(){
		EditorStyles.label.wordWrap = true;
		bHasInit = true;
		if(btnRefreshText == null){
			btnRefreshText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/refresh2.png",typeof(Texture)) as Texture;	
		}
		if(btnDeleteText == null){
			btnDeleteText = (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/delete.png",typeof(Texture)) as Texture;	
		}
		if(LoadBtnBG == null){
			LoadBtnBG = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/otherbg.png",typeof(Texture2D)) as Texture2D;	
		}
		if(LoadBtnBGGlow == null){
			LoadBtnBGGlow = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/otherbg2.png",typeof(Texture2D)) as Texture2D;	
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
	}
	
	void DoDefaultHelpMat(){
		GUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
			
		}
		EditorGUILayout.LabelField(" = Resets material(s) to default materials.");
		EditorGUILayout.EndHorizontal();	
	}
	
	void DoDeleteHelpMat(){
		GUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){
			
		}
		EditorGUILayout.LabelField(" = Removes material(s) from intersection.");
		EditorGUILayout.EndHorizontal();	
	}
	
	void Line(){
		GUILayout.Space(4f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); //Horizontal bar
		GUILayout.Space(4f);
	}
}