#region "Imports"
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using GSD;
[CustomEditor(typeof(GSDSplineN))] 
#endregion

//=====================================================
//==   NOTE THAT CUSTOM SERIALIZATION IS USED HERE   ==
//==     SOLELY TO COMPLY WITH UNDO REQUIREMENTS 	 ==
//=====================================================

public class GSDSplineNEditor : Editor {
	#region "Vars"
	protected GSDSplineN tNode { get { return (GSDSplineN) target; } }
	const string tOnlineHelpDesc = "Visit the online manual for the most effective help.";
	bool bMouseDragHasProcessed = true;
	int eCount = -1;
	int currentCount = 0;
	public bool bSplinatedObjectHelp = false;
	public bool bEdgeObjectHelp = false;
	bool bRemoveAll = false;
	float HorizRoadMax = 0;
	
	//Button icons:
	Texture btnDeleteText = null; 
	Texture btnCopyText = null;
	Texture btnSaveText = null;
	Texture btnLoadText = null;
	Texture btnExtrudeText = null;
	Texture btnEdgeText = null;
	Texture btnHelpText = null;
	Texture btnRefreshText = null;
	Texture btnDefaultText = null;
	Texture2D LoadBtnBG = null;
	Texture2D GSDTextAreaBG = null;
	Texture2D LoadBtnBGGlow = null;
	Texture2D ManualBG = null;
	
	public bool bLoadingEOS = false;
	public int LoadingEOSIndex = 0;
	public List<string> LoadingEOSNames = null;
	public List<string> LoadingEOSPaths = null;
	
	public bool bLoadingEOM = false;
	public int LoadingEOMIndex = 0;  
	public List<string> LoadingEOMNames = null;
	public List<string> LoadingEOMPaths = null;
	
	//Checkers:
//	float ChangeChecker = -1f;
//	bool bChangeChecker = false;
//	Vector3 vChangeChecker = default(Vector3);
//	GameObject tObj = null;
//	Material tMat = null;
	GSD.Roads.Splination.SplinatedMeshMaker SMM = null;
	


			
	public enum EndObjectsDefaultsEnum{None,
		WarningSign1_Static,
		WarningSign2_Static, 
		Atten_Static, 
		Barrel1_Static, 
		Barrel1_Rigid, 
		Barrel3_Static, 
		Barrel3_Rigid, 
		Barrel7_Static, 
		Barrel7_Rigid 
	};
	EndObjectsDefaultsEnum tEndObjectAdd = EndObjectsDefaultsEnum.None;
	
	private static string[] EndObjectsDefaultsEnumDesc = new string[]{
		"Quick add",
		"WarningSign1",
		"WarningSign2", 
		"Attenuator", 
		"1 Sand barrel Static", 
		"1 Sand barrel Rigid", 
		"3 Sand barrels Static", 
		"3 Sand barrels Rigid", 
		"7 Sand barrels Static", 
		"7 Sand barrels Rigid" 
	};
	
	public enum SMMDefaultsEnum{None,Custom,
		KRail,
		WBeamR,
		WBeamL, 
		Railing1,
		Railing2,
		Railing3,
		Railing4,
		RailingBase05m,
		RailingBase1m
	};
	SMMDefaultsEnum tSMMQuickAdd = SMMDefaultsEnum.None;
	
	public enum BridgeTopBaseDefaultsEnum{None,
		BaseExact, 
		Base1MOver, 
		Base2MOver, 
		Base3MDeep, 
	};
	BridgeTopBaseDefaultsEnum tBridgeTopBaseQuickAdd = BridgeTopBaseDefaultsEnum.None;
	
	public enum BridgeBottomBaseDefaultsEnum{None,
		BridgeBase6, 
		BridgeBase7, 
		BridgeBase8, 
		BridgeBaseGrid,
		BridgeSteel,
		BridgeBase2,
		BridgeBase3,
        BridgeBase4, 
		BridgeBase5, 
	};
	BridgeBottomBaseDefaultsEnum tBridgeBottomBaseQuickAdd = BridgeBottomBaseDefaultsEnum.None;
	
	public enum BridgeWizardDefaultsEnum{None,
		ArchBridge12m, 
		ArchBridge24m, 
		ArchBridge48m, 
		SuspensionBridgeSmall, 
		SuspensionBridgeLarge, 
		CausewayBridge1, 
		CausewayBridge2, 
		CausewayBridge3, 
		CausewayBridge4, 
		ArchBridge1, 
		ArchBridge2, 
		ArchBridge3,
		GridBridge,
		SteelBeamBridge
	};
//	BridgeWizardDefaultsEnum tBridgeWizardQuickAdd = BridgeWizardDefaultsEnum.None;
	
	
	public enum HorizMatchingDefaultsEnum{None,
		MatchCenter, 
		MatchRoadLeft, 
		MatchShoulderLeft, 
		MatchRoadRight, 
		MatchShoulderRight
	};
	HorizMatchingDefaultsEnum tHorizMatching = HorizMatchingDefaultsEnum.None;
	
	public enum EOMDefaultsEnum{None,Custom,StreetLightSingle,StreetLightDouble};

	//GSD.Roads.Splination.CollisionTypeEnum tCollisionTypeSpline = GSD.Roads.Splination.CollisionTypeEnum.SimpleMeshTriangle;
	//GSD.Roads.Splination.RepeatUVTypeEnum tRepeatUVType = GSD.Roads.Splination.RepeatUVTypeEnum.None;
	GSD.Roads.EdgeObjects.EdgeObjectMaker EOM = null;

	private static string[] TheAxisDescriptions_Spline = new string[]{
		"X axis",
		"Z axis"
	};
	
	private static string[] RepeatUVTypeDescriptions_Spline = new string[]{
		"None",
		"X axis",
		"Y axis"
	};
	
	private static string[] TheCollisionTypeEnumDescSpline = new string[]{
		"None",
		"Simple triangle",
		"Simple trapezoid",
		"Meshfilter collision mesh",
		"Straight line box collider"
	};

	private string[] HorizMatchSubTypeDescriptions;
	#endregion
	
	GUIStyle GSDImageButton = null;
	GUIStyle GSDLoadButton = null;
	GUIStyle GSDManualButton = null;
	GUIStyle GSDUrl = null;

	bool bSceneRectSet = false;
	Rect tSceneRect = default(Rect);
	bool bHasInit = false;
	
		//Buffers:
//	bool t_opt_GizmosEnabled = false;
	bool t_opt_GizmosEnabled = false;
	bool t_bIsBridgeStart = false;
	bool t_bIsBridgeEnd = false;
	bool t_bRoadCut = false;
	
	void Init(){
		bHasInit = true;
		EditorStyles.label.wordWrap = true;
		EditorStyles.miniLabel.wordWrap = true;
		
		if(btnDeleteText == null){
			btnDeleteText = (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/delete.png",typeof(Texture)) as Texture;	
		}
		if(btnCopyText == null){
			btnCopyText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/copy.png",typeof(Texture)) as Texture;	
		}
		if(btnLoadText == null){
			btnLoadText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/load.png",typeof(Texture)) as Texture;	
		}
		if(btnSaveText == null){
			btnSaveText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/save.png",typeof(Texture)) as Texture;	
		}
		if(btnExtrudeText == null){
			btnExtrudeText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/extrude.png",typeof(Texture)) as Texture;	
		}
		if(btnEdgeText == null){
			btnEdgeText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/edge.png",typeof(Texture)) as Texture;	
		}
		if(btnHelpText == null){
			btnHelpText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/help.png",typeof(Texture)) as Texture;	
		}
		if(GSDTextAreaBG == null){
			GSDTextAreaBG = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/popupbg.png",typeof(Texture2D)) as Texture2D;	
		}
		if(LoadBtnBG == null){
			LoadBtnBG = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/loadbg.png",typeof(Texture2D)) as Texture2D;	
		}
		if(LoadBtnBGGlow == null){
			LoadBtnBGGlow = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/loadbgglow.png",typeof(Texture2D)) as Texture2D;	
		}
		if(ManualBG == null){
			ManualBG = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/manualbg.png",typeof(Texture2D)) as Texture2D;	
		}
		if(btnRefreshText == null){
			btnRefreshText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/refresh.png",typeof(Texture)) as Texture;	
		}
		if(btnDefaultText == null){
			btnDefaultText	= (Texture)AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Editor/Icons/refresh2.png",typeof(Texture)) as Texture;	
		}
		
		if(GSDImageButton == null){
	    	GSDImageButton = new GUIStyle(GUI.skin.button);
	    	GSDImageButton.contentOffset = new Vector2(0f,-2f);
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
			GSDLoadButton.padding = new RectOffset(0,35,0,0);
		}
		
		if(GSDManualButton == null){
	    	GSDManualButton = new GUIStyle(GUI.skin.button);
			GSDManualButton.contentOffset = new Vector2(0f,1f);
			GSDManualButton.normal.textColor = new Color(1f,1f,1f,1f);
			GSDManualButton.normal.background = ManualBG;
			GSDManualButton.fixedHeight = 16f;
			GSDManualButton.fixedWidth = 128f;
		}
		
		if(GSDUrl == null){
	    	GSDUrl = new GUIStyle(GUI.skin.button);
			GSDUrl.normal.textColor = new Color(0.5f,1f,0.5f,1f);
		}
		
		float tRoadWidthHalf = tNode.GSDSpline.tRoad.RoadWidth()*0.5f;
		HorizMatchSubTypeDescriptions = new string[6];
		HorizMatchSubTypeDescriptions[0] = "Select preset";
		HorizMatchSubTypeDescriptions[1] = "Match center: 0 meters";
		HorizMatchSubTypeDescriptions[2] = "Match road left edge: -" + tRoadWidthHalf.ToString("F1") + " meters";
		HorizMatchSubTypeDescriptions[4] = "Match road right edge: " + tRoadWidthHalf.ToString("F1") + " meters";
		
		if(tNode.GSDSpline.tRoad.opt_bShouldersEnabled){
			HorizMatchSubTypeDescriptions[3] = "Match shoulder left edge: -" + (tRoadWidthHalf + tNode.GSDSpline.tRoad.opt_ShoulderWidth).ToString("F1") + " meters";
			HorizMatchSubTypeDescriptions[5] = "Match shoulder right edge: " + (tRoadWidthHalf + tNode.GSDSpline.tRoad.opt_ShoulderWidth).ToString("F1") + " meters";
		}else{
			HorizMatchSubTypeDescriptions[2] = "Match shoulder left edge: -" + tRoadWidthHalf.ToString("F1") + " meters";
			HorizMatchSubTypeDescriptions[4] = "Match shoulder right edge: " + tRoadWidthHalf.ToString("F1") + " meters";
		}
		
		HorizRoadMax = tNode.GSDSpline.tRoad.RoadWidth()*20;
	}
	
	GSDSplineN iNode1 = null;
	GSDSplineN iNode2 = null;
	bool bCreateIntersection = false;
	public override void OnInspectorGUI(){
		if(Event.current.type == EventType.ValidateCommand){
			switch (Event.current.commandName){
			case "UndoRedoPerformed":
				UpdateSplineObjects_OnUndo();
				break;
			}
		}

		if(Event.current.type != EventType.Layout && bCreateIntersection){
			bCreateIntersection = false;
			Selection.activeGameObject = GSD.Roads.GSDIntersections.CreateIntersection(iNode1,iNode2);
			return;
		}
		
		
		if(Event.current.type != EventType.Layout && tNode.bQuitGUI){
			tNode.bQuitGUI = false;
			return;
		}

		//Graphic null checks:
		if(!bHasInit){ Init(); }

		Line();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(tNode.EditorDisplayString,EditorStyles.boldLabel);
		
		if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(128f))){
			Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
		}
		EditorGUILayout.EndHorizontal();
		
		//Option: Gizmo options, Convoluted due to submission compliance for undo rules:
		if(tNode.GSDSpline.tRoad.opt_GizmosEnabled != tNode.opt_GizmosEnabled){
			tNode.GSDSpline.tRoad.opt_GizmosEnabled = tNode.opt_GizmosEnabled;
			tNode.GSDSpline.tRoad.UpdateGizmoOptions();
			tNode.GSDSpline.tRoad.Wireframes_Toggle();
		}
		t_opt_GizmosEnabled = EditorGUILayout.Toggle("Gizmos: ",tNode.GSDSpline.tRoad.opt_GizmosEnabled);
		
		//Option: Manual road cut:
		if(tNode.idOnSpline > 0 && tNode.idOnSpline < (tNode.GSDSpline.GetNodeCount()-1) && !tNode.bIsIntersection && !tNode.bSpecialEndNode){ // && !cNode.bIsBridge_PreNode && !cNode.bIsBridge_PostNode){
			if(tNode.GSDSpline.tRoad.opt_bDynamicCuts){
				Line();
				t_bRoadCut = EditorGUILayout.Toggle("Cut road at this node: ",tNode.bRoadCut);
			}
			Line();
		}
		
		//Option: Bridge options
		bool bDidBridge = false;
		if(!tNode.bIsEndPoint){
			//Bridge start:
			if(!tNode.bIsBridgeEnd && tNode.CanBridgeStart()){
				t_bIsBridgeStart = EditorGUILayout.Toggle(" Bridge start",tNode.bIsBridgeStart);
				bDidBridge = true;
			}
			//Bridge end:
			if(!tNode.bIsBridgeStart && tNode.CanBridgeEnd()){
				t_bIsBridgeEnd = EditorGUILayout.Toggle(" Bridge end",tNode.bIsBridgeEnd);
				bDidBridge = true;
			}
			
			if(bDidBridge){
				Line();
			}
		}
		
		//Do extrusion and edge objects overview:
		DoExtAndEdgeOverview();
		
		if(tNode.bSpecialRoadConnPrimary){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Road connection:",EditorStyles.boldLabel);
			if(GUILayout.Button("Break road connection")){
				tNode.SpecialNodeCounterpart.BreakConnection();
			}
			EditorGUILayout.EndHorizontal();
			if(tNode.SpecialNodeCounterpart != null){
				EditorGUILayout.LabelField(tNode.SpecialNodeCounterpart.GSDSpline.tRoad.transform.name + " to " + tNode.SpecialNodeCounterpart.SpecialNodeCounterpart.GSDSpline.tRoad.transform.name);
			}
			EditorGUILayout.LabelField("To break this road connection, click the \"Break road connection\" button.");
			Line();
		}

		//Statistics:
		DoStats();
		EditorGUILayout.EndVertical();
		
		if(GUI.changed){
			//Set snapshot for undo:
	
			Undo.RecordObject(tNode,"Modify node");
			
			//Option: Gizmo options, Convoluted due to submission compliance for undo rules:
			if(t_opt_GizmosEnabled != tNode.GSDSpline.tRoad.opt_GizmosEnabled){
				tNode.GSDSpline.tRoad.opt_GizmosEnabled = t_opt_GizmosEnabled;
				tNode.GSDSpline.tRoad.UpdateGizmoOptions();
				tNode.GSDSpline.tRoad.Wireframes_Toggle();
				SceneView.RepaintAll();
			}
			
			//Option: Manual cut:
			if(tNode.idOnSpline > 0 && tNode.idOnSpline < (tNode.GSDSpline.GetNodeCount()-1) && !tNode.bIsIntersection && !tNode.bSpecialEndNode){ // && !cNode.bIsBridge_PreNode && !cNode.bIsBridge_PostNode){
				if(tNode.GSDSpline.tRoad.opt_bDynamicCuts){
					if(t_bRoadCut != tNode.bRoadCut){
						tNode.bRoadCut = t_bRoadCut;	
					}
				}
			}
			
			//Option: Bridge options
				//Bridge start:
			if(!tNode.bIsEndPoint){
				if(!tNode.bIsBridgeEnd && tNode.CanBridgeStart()){
					if(t_bIsBridgeStart != tNode.bIsBridgeStart){
						tNode.bIsBridgeStart = t_bIsBridgeStart;
						tNode.BridgeToggleStart();	
					}
				}
			}
				//Bridge end:
			if(!tNode.bIsEndPoint){
				if(!tNode.bIsBridgeStart && tNode.CanBridgeEnd()){
					if(t_bIsBridgeEnd != tNode.bIsBridgeEnd){
						tNode.bIsBridgeEnd = t_bIsBridgeEnd;
						tNode.BridgeToggleEnd();	
					}
				}
			}
			
			UpdateSplineObjects();
			UpdateEdgeObjects();
			
			EditorUtility.SetDirty(target);
		}
	}
	
	void OnSelectionChanged(){
		Repaint();	
	}
	
	//GUIStyle SectionBG;
	
	void DoExtAndEdgeOverview(){
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Extrusion & edge objects",EditorStyles.boldLabel);
		EditorGUILayout.LabelField("");
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
				if(bEdgeObjectHelp){
			bEdgeObjectHelp = EditorGUILayout.Foldout(bEdgeObjectHelp, "Hide quick help");
		}else{
			bEdgeObjectHelp = EditorGUILayout.Foldout(bEdgeObjectHelp, "Show quick help");
		}
		EditorGUILayout.LabelField("");
		
		if(GUILayout.Button("Save group",EditorStyles.miniButton,GUILayout.Width(108f)) || GUILayout.Button(btnSaveText,GSDImageButton,GUILayout.Width(16f))){
			GSDSaveWindow tSave = EditorWindow.GetWindow<GSDSaveWindow>();
			if(tNode.bIsBridge){
				tSave.Initialize(ref tSceneRect,GSDSaveWindow.WindowTypeEnum.BridgeWizard,tNode);
			}else{
				tSave.Initialize(ref tSceneRect,GSDSaveWindow.WindowTypeEnum.BridgeWizard,tNode);
			}
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(4f);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("");
		if(GUILayout.Button("Open Wizard",GSDLoadButton,GUILayout.Width(128f))){// || GUILayout.Button(btnLoadText,GSDImageButton,GUILayout.Width(16f))){
			GSDWizard tWiz = EditorWindow.GetWindow<GSDWizard>();
			if(tSceneRect.x < 0){ tSceneRect.x = 0f; }
			if(tSceneRect.y < 0){ tSceneRect.y = 0f; }
			tWiz.xRect = tSceneRect;
			if(tNode.bIsBridgeStart){
				tWiz.Initialize(GSDWizard.WindowTypeEnum.BridgeComplete,tNode);
			}else{
				tWiz.Initialize(GSDWizard.WindowTypeEnum.Extrusion,tNode);	
			}
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(4f);
		
		if(bEdgeObjectHelp){
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnExtrudeText,GSDImageButton,GUILayout.Width(32f))){ }
			EditorGUILayout.LabelField("= Extrusion objects", EditorStyles.miniLabel);
			EditorGUILayout.LabelField("");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Railings, bridge pieces, center dividers and other connected objects.", EditorStyles.miniLabel);
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnEdgeText,GSDImageButton,GUILayout.Width(32f))){ }
			EditorGUILayout.LabelField("= Edge objects", EditorStyles.miniLabel);
			EditorGUILayout.LabelField("");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Signs, street lights, bridge pillars and other unconnected road objects.", EditorStyles.miniLabel);
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnSaveText,GSDImageButton,GUILayout.Width(16f))){ }
			EditorGUILayout.LabelField("= Saves object config to library for use on other nodes.", EditorStyles.miniLabel);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnCopyText,GSDImageButton,GUILayout.Width(16f))){ }
			EditorGUILayout.LabelField("= Duplicates object onto current node.", EditorStyles.miniLabel);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){ }
			EditorGUILayout.LabelField("= Deletes object.", EditorStyles.miniLabel);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){ }
			EditorGUILayout.LabelField("= Refreshes object.", EditorStyles.miniLabel);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){ }
			EditorGUILayout.LabelField("= Resets setting(s) to default.", EditorStyles.miniLabel);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			Line();
		}
		currentCount = 0;

		GUILayout.Space(2f);
		
		
		
		//Splinated objects:
		DoSplineObjects();
		
		//Edge Objects:
		DoEdgeObjects();

		GUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("");
		if(GUILayout.Button("Add custom extrusion object", EditorStyles.miniButton)){
			tNode.AddSplinatedObject();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("");
		if(GUILayout.Button("Add custom edge object", EditorStyles.miniButton)){
			tNode.AddEdgeObject();
		}
		EditorGUILayout.EndHorizontal();
		
		if(tNode.SplinatedObjects.Count > 20 || tNode.EdgeObjects.Count > 20){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("");
			bRemoveAll = EditorGUILayout.Toggle(bRemoveAll, GUILayout.Width(20f));
			if(GUILayout.Button("Remove all", EditorStyles.miniButton, GUILayout.Width(100f))){
				if(bRemoveAll){ 
					tNode.RemoveAllSplinatedObjects();
					tNode.RemoveAllEdgeObjects();
					bRemoveAll = false;
				}
			}
			
			EditorGUILayout.EndHorizontal();
		}
		Line();	
	}
	
	void DoStats(){
		EditorGUILayout.LabelField("Statistics:");
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Grade to next node: " + tNode.GradeToNext);
		EditorGUILayout.LabelField("Grade to prev node: " + tNode.GradeToPrev);
		EditorGUILayout.LabelField("Distance from start: " + tNode.tDist.ToString("F3") + " meters");
		EditorGUILayout.LabelField("% of spline: " + ((tNode.tDist / tNode.GSDSpline.distance)*100f).ToString("F2") + "%");
		EditorGUILayout.LabelField("Parameter: " + tNode.tTime);
		EditorGUILayout.LabelField("Tangent: " + tNode.tangent);
		EditorGUILayout.LabelField("POS: " + tNode.pos);
		EditorGUILayout.LabelField("ID on spline: " + tNode.idOnSpline);
		EditorGUILayout.LabelField("Is intersection node: " + tNode.bIsIntersection);
		EditorGUILayout.LabelField("Is end node: " + tNode.bIsEndPoint);
		EditorGUILayout.LabelField("Is bridge start: " + tNode.bIsBridgeStart);
		EditorGUILayout.LabelField("Is bridge end: " + tNode.bIsBridgeEnd);
		EditorGUILayout.LabelField("Road: " + tNode.GSDSpline.tRoad.transform.name);
		EditorGUILayout.LabelField("System: " + tNode.GSDSpline.tRoad.GSDRS.transform.name);
		EditorGUILayout.SelectableLabel("UID: " + tNode.UID);
	}
	
	public void DoSplineObjects(){
		if(!tNode.CanSplinate()){ return; }
		if(tNode.SplinatedObjects == null){ tNode.SplinatedObjects = new List<GSD.Roads.Splination.SplinatedMeshMaker>(); }
		eCount = tNode.SplinatedObjects.Count;

		SMM = null;
		eCount = tNode.SplinatedObjects.Count;
		if(eCount == 0){ }
		
		for(int i=0;i<tNode.SplinatedObjects.Count;i++){
			currentCount +=1;
			SMM = tNode.SplinatedObjects[i];
			if(SMM.EM == null){
				SMM.EM = new GSD.Roads.Splination.SplinatedMeshMaker.SplinatedMeshEditorMaker();
			}
			SMM.EM.Setup(SMM);
			
			//GSD.Roads.Splination.AxisTypeEnum tAxisTypeSpline = GSD.Roads.Splination.AxisTypeEnum.Z;
			
			EditorGUILayout.BeginVertical("TextArea");

			if(SMM.bNeedsUpdate){ SMM.Setup(true); }

			
			EditorGUILayout.BeginHorizontal();
			
			SMM.bToggle = EditorGUILayout.Foldout(SMM.bToggle,"#"+ currentCount.ToString() + ": " + SMM.tName);
		
			if(GUILayout.Button(btnExtrudeText,GSDImageButton,GUILayout.Width(32f))){
				
			}
			
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				SMM.Setup();
			}
			if(GUILayout.Button(btnSaveText,GSDImageButton,GUILayout.Width(16f))){
				GSDSaveWindow tSave = EditorWindow.GetWindow<GSDSaveWindow>();
				tSave.Initialize(ref tSceneRect,GSDSaveWindow.WindowTypeEnum.Extrusion,tNode,SMM);
			}
			if(GUILayout.Button(btnCopyText,GSDImageButton,GUILayout.Width(16f))){
				Undo.RecordObject(tNode,"Copy");
				tNode.CopySplinatedObject(ref SMM);
				EditorUtility.SetDirty(tNode);
			}
			if(GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){
				Undo.RecordObject(tNode,"Delete");
				tNode.RemoveSplinatedObject(i);
				EditorUtility.SetDirty(tNode);
			}
			EditorGUILayout.EndHorizontal();
			if(!SMM.bToggle){ EditorGUILayout.EndVertical(); continue; }
			
			GUILayout.Space(8f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("General options:");
			if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
				Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginVertical("box");
			
			//Name:
			SMM.EM.tName = EditorGUILayout.TextField("Name:",SMM.tName);

			//Game object (prefab):
		  	SMM.EM.CurrentSplination = (GameObject)EditorGUILayout.ObjectField("Prefab:",SMM.CurrentSplination,typeof(GameObject), false);
			
			//Game object (prefab start cap):
		  	SMM.EM.CurrentSplinationCap1 = (GameObject)EditorGUILayout.ObjectField("Prefab start cap:",SMM.CurrentSplinationCap1,typeof(GameObject), false);
			//Prefab start cap height offset:
			if(SMM.CurrentSplinationCap1 != null){
				SMM.EM.CapHeightOffset1 = EditorGUILayout.FloatField("  Height offset:",SMM.CapHeightOffset1); 
			}
			
			//Game object (prefab end cap):
		  	SMM.EM.CurrentSplinationCap2 = (GameObject)EditorGUILayout.ObjectField("Prefab end cap:",SMM.CurrentSplinationCap2,typeof(GameObject), false);
			//Prefab end cap height offset:
			if(SMM.CurrentSplinationCap2 != null){
				SMM.EM.CapHeightOffset2 = EditorGUILayout.FloatField("  Height offset:",SMM.CapHeightOffset2); 
			}
			
			//Material overrides:
			SMM.EM.bMaterialOverride = EditorGUILayout.Toggle("Material override: ",SMM.bMaterialOverride); 
			if(SMM.bMaterialOverride){
				SMM.EM.SplinatedMaterial1 = (Material)EditorGUILayout.ObjectField("Override mat #1: ",SMM.SplinatedMaterial1,typeof(Material), false);
				SMM.EM.SplinatedMaterial2 = (Material)EditorGUILayout.ObjectField("Override mat #2: ",SMM.SplinatedMaterial2,typeof(Material), false);
			}
			
			//Axis:
			SMM.EM.Axis = (GSD.Roads.Splination.AxisTypeEnum)EditorGUILayout.Popup("Extrusion axis: ",(int)SMM.Axis,TheAxisDescriptions_Spline,GUILayout.Width(250f));
			
			//Start time:
			if(SMM.StartTime < tNode.MinSplination){ SMM.StartTime = tNode.MinSplination; }
			if(SMM.EndTime > tNode.MaxSplination){ SMM.EndTime = tNode.MaxSplination; }
			EditorGUILayout.BeginHorizontal();
			SMM.EM.StartTime = EditorGUILayout.Slider("Start param: ",SMM.StartTime,tNode.MinSplination,tNode.MaxSplination-0.01f);	
			if(GUILayout.Button("match node",EditorStyles.miniButton, GUILayout.Width(80f))){
				SMM.EM.StartTime = tNode.tTime;
			}
			if(SMM.EM.StartTime >= SMM.EM.EndTime){ SMM.EM.EndTime = (SMM.EM.StartTime + 0.01f); }
			EditorGUILayout.EndHorizontal();
			
			//End time:
			EditorGUILayout.BeginHorizontal();
			SMM.EM.EndTime = EditorGUILayout.Slider("End param: ",SMM.EndTime,SMM.StartTime,tNode.MaxSplination);	
			if(GUILayout.Button("match next",EditorStyles.miniButton, GUILayout.Width(80f))){
				SMM.EM.EndTime = tNode.NextTime;
			}
			if(SMM.EM.StartTime >= SMM.EM.EndTime){ SMM.EM.EndTime = (SMM.EM.StartTime + 0.01f); }
			EditorGUILayout.EndHorizontal();
			
			//Straight line options:
			if(tNode.IsStraight()){
				if(!SMM.bIsStretch){
					SMM.EM.bIsStretch = EditorGUILayout.Toggle("Straight line stretch:",SMM.bIsStretch); 
				}else{
					EditorGUILayout.BeginVertical("box");
					SMM.EM.bIsStretch = EditorGUILayout.Toggle("Straight line stretch:",SMM.bIsStretch); 
		
					//Stretch_UVThreshold:
					SMM.EM.Stretch_UVThreshold = EditorGUILayout.Slider("UV stretch threshold:",SMM.Stretch_UVThreshold,0.01f,0.5f);
					
					//UV repeats:
					SMM.EM.RepeatUVType = (GSD.Roads.Splination.RepeatUVTypeEnum)EditorGUILayout.Popup("UV stretch axis: ",(int)SMM.RepeatUVType,RepeatUVTypeDescriptions_Spline,GUILayout.Width(250f));
					EditorGUILayout.EndVertical();
				}
			}else{
				SMM.EM.bIsStretch = false;
			}
			
			
			SMM.EM.bTrimStart = EditorGUILayout.Toggle("Trim start:",SMM.bTrimStart); 
			SMM.EM.bTrimEnd = EditorGUILayout.Toggle("Trim end:",SMM.bTrimEnd); 

			//Static option:
			SMM.EM.bStatic = EditorGUILayout.Toggle("Static: ",SMM.bStatic); 
			
			//Splination method
//			SMM.EM.bMatchRoadIncrements = EditorGUILayout.Toggle("Match road increments: ",SMM.bMatchRoadIncrements); 
			SMM.EM.bMatchTerrain = EditorGUILayout.Toggle("Match ground: ",SMM.bMatchTerrain); 
			
			//Vector min/max threshold: 
			EditorGUILayout.BeginHorizontal();
			SMM.EM.MinMaxMod = EditorGUILayout.Slider("Vertex min/max threshold: ",SMM.MinMaxMod,0.01f,0.2f);	
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				SMM.EM.MinMaxMod = 0.05f;
			}
			EditorGUILayout.EndHorizontal();
			
			//Vertex matching precision:
			EditorGUILayout.BeginHorizontal();
			SMM.EM.VertexMatchingPrecision = EditorGUILayout.Slider("Vertex matching precision: ",SMM.VertexMatchingPrecision,0f,0.01f);	
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				SMM.EM.VertexMatchingPrecision = 0.005f;
			}
			EditorGUILayout.EndHorizontal();
			
			//UV repeats:
			if(!SMM.bIsStretch){
				SMM.EM.RepeatUVType = (GSD.Roads.Splination.RepeatUVTypeEnum)EditorGUILayout.Popup("UV repeat axis: ",(int)SMM.RepeatUVType,RepeatUVTypeDescriptions_Spline,GUILayout.Width(250f));
			}
			
			if(SMM.bMatchRoadDefinition){
				EditorGUILayout.BeginVertical("TextArea");
				EditorGUILayout.BeginHorizontal();
				SMM.EM.bMatchRoadDefinition = EditorGUILayout.Toggle("Match road definition: ",SMM.bMatchRoadDefinition); 
				if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
					SMM.EM.bMatchRoadDefinition = false;
				}
				EditorGUILayout.EndHorizontal();
				if(SMM.bMatchRoadDefinition){
					EditorGUILayout.LabelField("  Only use this option if object length doesn't match the road definition.", EditorStyles.miniLabel);	
					EditorGUILayout.LabelField("  Matching road definition requires a UV repeat type.", EditorStyles.miniLabel);	
					EditorGUILayout.LabelField("  If the material fails to scale properly, try flipping the Y rotation.", EditorStyles.miniLabel);	
				}
				//Flip rotation option:
				SMM.EM.bFlipRotation = EditorGUILayout.Toggle("  Flip Y rotation: ",SMM.bFlipRotation);
				EditorGUILayout.EndVertical();
			}else{
				EditorGUILayout.BeginHorizontal();
				SMM.EM.bMatchRoadDefinition = EditorGUILayout.Toggle("Match road definition: ",SMM.bMatchRoadDefinition); 
				if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
					SMM.EM.bMatchRoadDefinition = false;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
			
			//Vertical offset:
			EditorGUILayout.LabelField("Vertical options:");
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			SMM.EM.VerticalRaise = EditorGUILayout.Slider("Vertical raise magnitude:",SMM.VerticalRaise,-512f,512f); 
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				SMM.EM.VerticalRaise = 0f; 
			}
			EditorGUILayout.EndHorizontal();
			
			//Vertical curve:
			if(SMM.VerticalCurve == null || SMM.VerticalCurve.keys.Length < 2){ EnforceCurve(ref SMM.VerticalCurve); }
			EditorGUILayout.BeginHorizontal();
			SMM.EM.VerticalCurve = EditorGUILayout.CurveField("Curve: ",SMM.VerticalCurve);	
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				ResetCurve(ref SMM.EM.VerticalCurve);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			//Horizontal offsets:
			SMM.EM.HorizontalSep = SMM.HorizontalSep;
			EditorGUILayout.LabelField("Horizontal offset options:");
			EditorGUILayout.BeginVertical("box");
			tHorizMatching = HorizMatchingDefaultsEnum.None;
			tHorizMatching = (HorizMatchingDefaultsEnum)EditorGUILayout.Popup((int)tHorizMatching,HorizMatchSubTypeDescriptions, GUILayout.Width(100f));
			if(tHorizMatching != HorizMatchingDefaultsEnum.None){
				if(tHorizMatching == HorizMatchingDefaultsEnum.MatchCenter){
					SMM.EM.HorizontalSep = 0f;
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchRoadLeft){
					SMM.EM.HorizontalSep = (tNode.GSDSpline.tRoad.RoadWidth()/2)*-1;
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchShoulderLeft){
					SMM.EM.HorizontalSep = ((tNode.GSDSpline.tRoad.RoadWidth()/2) + tNode.GSDSpline.tRoad.opt_ShoulderWidth)*-1;
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchRoadRight){
					SMM.EM.HorizontalSep = (tNode.GSDSpline.tRoad.RoadWidth()/2);
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchShoulderRight){
					SMM.EM.HorizontalSep = (tNode.GSDSpline.tRoad.RoadWidth()/2) + tNode.GSDSpline.tRoad.opt_ShoulderWidth;
				}
				tHorizMatching = HorizMatchingDefaultsEnum.None;
			}
			EditorGUILayout.BeginHorizontal();
			SMM.EM.HorizontalSep = EditorGUILayout.Slider("Horiz offset magnitude:",SMM.HorizontalSep,(-1f*HorizRoadMax),HorizRoadMax);	
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				SMM.EM.HorizontalSep = 0f;
			}
			EditorGUILayout.EndHorizontal();
			
			//Horizontal curve:
			if(SMM.HorizontalCurve == null || SMM.HorizontalCurve.keys.Length < 2){ EnforceCurve(ref SMM.HorizontalCurve); }

			EditorGUILayout.BeginHorizontal();
			SMM.EM.HorizontalCurve = EditorGUILayout.CurveField("Curve: ",SMM.HorizontalCurve);
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				ResetCurve(ref SMM.EM.HorizontalCurve);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			//Vertical cutoff:
			EditorGUILayout.LabelField("Vertical cutoff:");
			EditorGUILayout.BeginVertical("box");
			SMM.EM.bVerticalCutoff = EditorGUILayout.Toggle("Height cutoff enabled:",SMM.bVerticalCutoff); 
			if(SMM.bVerticalCutoff){
				SMM.EM.bVerticalCutoff_MatchZero = EditorGUILayout.Toggle("Match spline height:",SMM.bVerticalCutoff_MatchZero); 
				SMM.EM.bVerticalCutoffDownwards = EditorGUILayout.Toggle("Cut direction toggle:",SMM.bVerticalCutoffDownwards);
				SMM.EM.VerticalCutoff = EditorGUILayout.Slider("Height cut offset: ",SMM.VerticalCutoff,-50f,50f); 
				SMM.EM.bVerticalMeshCutoff_OppositeDir = EditorGUILayout.Toggle("Opposite dir mesh cut:",SMM.bVerticalMeshCutoff_OppositeDir); 
				SMM.EM.VerticalMeshCutoffOffset = EditorGUILayout.Slider("Mesh cut offset: ",SMM.VerticalMeshCutoffOffset,-5f,5f);
			}
			EditorGUILayout.EndVertical();
			
			//End type:
			EditorGUILayout.LabelField("Extrusion ending options:");
			EditorGUILayout.BeginVertical("box");
			SMM.EM.bStartDown = EditorGUILayout.Toggle("Push start down:",SMM.bStartDown); 
			SMM.EM.bEndDown = EditorGUILayout.Toggle("Push end down:",SMM.bEndDown); 
			if(SMM.bStartDown){
				SMM.EM.bStartTypeDownOverride = EditorGUILayout.Toggle("Override start down value: ",SMM.bStartTypeDownOverride); 
				if(SMM.bStartTypeDownOverride){
					SMM.EM.StartTypeDownOverride = EditorGUILayout.Slider("Downward movement: ",SMM.StartTypeDownOverride,-10f,10f);	
				}
			}
			if(SMM.bEndDown){
				SMM.EM.bEndTypeDownOverride = EditorGUILayout.Toggle("Override end down value: ",SMM.bEndTypeDownOverride); 
				if(SMM.bEndTypeDownOverride){
					SMM.EM.EndTypeDownOverride = EditorGUILayout.Slider("Downward movement: ",SMM.EndTypeDownOverride,-10f,10f);	
				}
			}
			EditorGUILayout.EndVertical();

			//Start and end objects:
			EditorGUILayout.LabelField("Start & end objects:");
			EditorGUILayout.BeginVertical("box");
			//End cap custom match start:
			SMM.EM.bEndCapCustomMatchStart = EditorGUILayout.Toggle("Match objects to ends:",SMM.bEndCapCustomMatchStart);
			
			//End objects match ground:
			SMM.EM.bEndObjectsMatchGround = EditorGUILayout.Toggle("Force origins to ground:",SMM.bEndObjectsMatchGround); 
			
			//Start cap:
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Start object:");
			tEndObjectAdd = (EndObjectsDefaultsEnum)EditorGUILayout.Popup((int)tEndObjectAdd,EndObjectsDefaultsEnumDesc);
			if(tEndObjectAdd != EndObjectsDefaultsEnum.None){
				SMM.EM.EndCapStart = GetEndObjectQuickAdd();
				tEndObjectAdd = EndObjectsDefaultsEnum.None;	
			}
			EditorGUILayout.EndHorizontal();
			
			
			SMM.EM.EndCapStart = (GameObject)EditorGUILayout.ObjectField("Prefab:",SMM.EndCapStart,typeof(GameObject), false);
			if(SMM.EndCapStart != null){
				SMM.EM.EndCapCustomOffsetStart = EditorGUILayout.Vector3Field("Position offset:",SMM.EndCapCustomOffsetStart);
				SMM.EM.EndCapCustomRotOffsetStart = EditorGUILayout.Vector3Field("Rotation offset:",SMM.EndCapCustomRotOffsetStart);
			}
			EditorGUILayout.EndVertical();

			//End cap:
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("End object:");
			tEndObjectAdd = (EndObjectsDefaultsEnum)EditorGUILayout.Popup((int)tEndObjectAdd,EndObjectsDefaultsEnumDesc);
			if(tEndObjectAdd != EndObjectsDefaultsEnum.None){
				SMM.EM.EndCapEnd = GetEndObjectQuickAdd();
				SMM.EM.EndCapCustomRotOffsetEnd = new Vector3(0f,180f,0f);
				tEndObjectAdd = EndObjectsDefaultsEnum.None;	
			}
			EditorGUILayout.EndHorizontal();
			
			
			SMM.EM.EndCapEnd = (GameObject)EditorGUILayout.ObjectField("Prefab:",SMM.EndCapEnd,typeof(GameObject), false);
			if(SMM.EndCapEnd != null){
				SMM.EM.EndCapCustomOffsetEnd = EditorGUILayout.Vector3Field("Position offset:",SMM.EndCapCustomOffsetEnd); 
				SMM.EM.EndCapCustomRotOffsetEnd = EditorGUILayout.Vector3Field("Rotation offset:",SMM.EndCapCustomRotOffsetEnd);
			}
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndVertical();

			//Collision:
			EditorGUILayout.LabelField("Collision options:");
			EditorGUILayout.BeginVertical("box");
			SMM.EM.CollisionType = (GSD.Roads.Splination.CollisionTypeEnum)EditorGUILayout.Popup("Collision type: ",(int)SMM.CollisionType,TheCollisionTypeEnumDescSpline,GUILayout.Width(320f));
			//Mesh collison convex option
			if(SMM.CollisionType != GSD.Roads.Splination.CollisionTypeEnum.None && SMM.CollisionType != GSD.Roads.Splination.CollisionTypeEnum.BoxCollision){
				SMM.EM.bCollisionConvex = EditorGUILayout.Toggle(" Convex: ",SMM.bCollisionConvex); 
				SMM.EM.bCollisionTrigger = EditorGUILayout.Toggle(" Trigger: ",SMM.bCollisionTrigger); 
			}
			
			if(SMM.CollisionType == GSD.Roads.Splination.CollisionTypeEnum.SimpleMeshTriangle || SMM.CollisionType == GSD.Roads.Splination.CollisionTypeEnum.SimpleMeshTrapezoid){
				SMM.EM.bSimpleCollisionAutomatic = EditorGUILayout.Toggle(" Automatic simple collision: ", SMM.bSimpleCollisionAutomatic); 
			}
			//If not automatic simple collisions:
			if(!SMM.bSimpleCollisionAutomatic){
				if(SMM.CollisionType == GSD.Roads.Splination.CollisionTypeEnum.SimpleMeshTriangle){
					SMM.EM.CollisionTriBL = SMM.CollisionTriBL;
					SMM.EM.CollisionTriBR = SMM.CollisionTriBR;
					SMM.EM.CollisionTriT = SMM.CollisionTriT;
					
					EditorGUILayout.LabelField("Bottom left:");
					SMM.EM.CollisionTriBL.x = EditorGUILayout.Slider(" x-axis: ",SMM.CollisionTriBL.x,SMM.mMinX-5f,SMM.mMaxX+5f);
					SMM.EM.CollisionTriBL.y = EditorGUILayout.Slider(" y-axis: ",SMM.CollisionTriBL.y,SMM.mMinY-5f,SMM.mMaxY+5f);
					SMM.EM.CollisionTriBL.z = EditorGUILayout.Slider(" z-axis: ",SMM.CollisionTriBL.z,SMM.mMinZ-5f,SMM.mMaxZ+5f);

					EditorGUILayout.LabelField("Bottom right:");
					SMM.EM.CollisionTriBR.x = EditorGUILayout.Slider(" x-axis: ",SMM.CollisionTriBR.x,SMM.mMinX-5f,SMM.mMaxX+5f);
					SMM.EM.CollisionTriBR.y = EditorGUILayout.Slider(" y-axis: ",SMM.CollisionTriBR.y,SMM.mMinY-5f,SMM.mMaxY+5f);
					SMM.EM.CollisionTriBR.z = EditorGUILayout.Slider(" z-axis: ",SMM.CollisionTriBR.z,SMM.mMinZ-5f,SMM.mMaxZ+5f);
					
					EditorGUILayout.LabelField("Top:");
					SMM.EM.CollisionTriT.x = EditorGUILayout.Slider(" x-axis: ",SMM.CollisionTriT.x,SMM.mMinX-5f,SMM.mMaxX+5f);
					SMM.EM.CollisionTriT.y = EditorGUILayout.Slider(" y-axis: ",SMM.CollisionTriT.y,SMM.mMinY-5f,SMM.mMaxY+5f);
					SMM.EM.CollisionTriT.z = EditorGUILayout.Slider(" z-axis: ",SMM.CollisionTriT.z,SMM.mMinZ-5f,SMM.mMaxZ+5f);

				}else if(SMM.CollisionType == GSD.Roads.Splination.CollisionTypeEnum.SimpleMeshTrapezoid){
					SMM.EM.CollisionBoxBL = EditorGUILayout.Vector3Field(" Bottom left:",SMM.CollisionBoxBL);
					SMM.EM.CollisionBoxBR = EditorGUILayout.Vector3Field(" Bottom right:",SMM.CollisionBoxBR);
					SMM.EM.CollisionBoxTL = EditorGUILayout.Vector3Field(" Top left:",SMM.CollisionBoxTL);
					SMM.EM.CollisionBoxTR = EditorGUILayout.Vector3Field(" Top right:",SMM.CollisionBoxTR);
				}
			}
			
			if(SMM.CollisionType == GSD.Roads.Splination.CollisionTypeEnum.BoxCollision){
				SMM.EM.StretchBC_LocOffset = EditorGUILayout.Vector3Field("Box collider center offset:",SMM.StretchBC_LocOffset);
				SMM.EM.bBCFlipX = EditorGUILayout.Toggle("Flip center X:", SMM.bBCFlipX);
				SMM.EM.bBCFlipZ = EditorGUILayout.Toggle("Flip center Z:", SMM.bBCFlipZ);

				
				SMM.EM.bStretchSize = EditorGUILayout.Toggle("Box collider size edit:",SMM.bStretchSize); 
				if(SMM.bStretchSize){ 
					SMM.EM.StretchBC_Size = EditorGUILayout.Vector3Field("Size:",SMM.StretchBC_Size);
				}else{
					EditorGUILayout.LabelField("Size:",SMM.StretchBC_Size.ToString());
				}	
			}
			EditorGUILayout.EndVertical();
			
			
			EditorGUILayout.LabelField("Rotation options:");
			EditorGUILayout.BeginVertical("box");

			//Custom rotation:
			SMM.EM.CustomRotation = SMM.CustomRotation;
			//EOM.CustomRotation = EditorGUILayout.Vector3Field("Custom rotation: ",EOM.CustomRotation);
			EditorGUILayout.BeginHorizontal();
			//Flip rotation option:
			if(SMM.EM.bFlipRotation != SMM.bFlipRotation){
				SMM.EM.bFlipRotation = EditorGUILayout.Toggle("Flip Y rotation: ",SMM.EM.bFlipRotation); 
			}else{
				SMM.EM.bFlipRotation = EditorGUILayout.Toggle("Flip Y rotation: ",SMM.bFlipRotation); 
			}
			
			
//			if(GUILayout.Button("Reset custom rotation",EditorStyles.miniButton,GUILayout.Width(160f))){
//				SMM.CustomRotation = new Vector3(0f,0f,0f);
//			}
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				SMM.EM.CustomRotation = new Vector3(0f,0f,0f);
			}
			EditorGUILayout.EndHorizontal();
//			SMM.EM.CustomRotation = EditorGUILayout.Vector3Field("",SMM.CustomRotation);
//			SMM.EM.CustomRotation.x = EditorGUILayout.Slider("x-axis: ",SMM.CustomRotation.x,-360f,360f);
//			SMM.EM.CustomRotation.y = EditorGUILayout.Slider("y-axis: ",SMM.CustomRotation.y,-360f,360f);
//			SMM.EM.CustomRotation.z = EditorGUILayout.Slider("z-axis: ",SMM.CustomRotation.z,-360f,360f);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();	
			
			
			EditorGUILayout.LabelField("Deprecated options:");
			EditorGUILayout.BeginVertical("box");
			SMM.EM.bExactSplination = EditorGUILayout.Toggle("Directional extrusion: ",SMM.bExactSplination);
			
			EditorGUILayout.EndVertical();	
			BigLine();
			BigLine();
		}
	}
	
	public void UpdateSplineObjects(){
		if(!tNode.CanSplinate()){ return; }
		if(tNode.SplinatedObjects == null){ tNode.SplinatedObjects = new List<GSD.Roads.Splination.SplinatedMeshMaker>(); }
		eCount = tNode.SplinatedObjects.Count;
		for(int i=0;i<eCount;i++){	
			SMM = tNode.SplinatedObjects[i];
			if(SMM.EM != null){
				if(!SMM.EM.IsEqualToSMM(SMM)){
					SMM.EM.LoadToSMM(SMM);

					SMM.UpdatePositions();
					if(SMM.EM.bIsStretch != SMM.bIsStretch){ 
						if(SMM.bIsStretch){
							SMM.CollisionType = GSD.Roads.Splination.CollisionTypeEnum.BoxCollision;
							SMM.bMatchRoadDefinition = false;
							SMM.bMatchTerrain = false;
							SMM.bCollisionConvex = false;
							SMM.bStartDown = false;
							SMM.bEndDown = false;
							SMM.bVerticalCutoff = false;
							SMM.bExactSplination = false;
							SMM.bEndTypeDownOverride = false;
						}
					}
										
					SMM.Setup(true);
//					Debug.Log ("Setup SMM");
				}
			}
		}
	}
	
	public void UpdateSplineObjects_OnUndo(){
		if(!tNode.CanSplinate()){ return; }
		if(tNode.SplinatedObjects == null){ tNode.SplinatedObjects = new List<GSD.Roads.Splination.SplinatedMeshMaker>(); }
		
		//Destroy all children:
    	for(int i=tNode.transform.childCount-1;i>=0;i--){
	        Object.DestroyImmediate(tNode.transform.GetChild(i).gameObject);
	    }
		
		//Re-setup the SMM:
		eCount = tNode.SplinatedObjects.Count;
		for(int i=0;i<eCount;i++){	
			SMM = tNode.SplinatedObjects[i];
			SMM.UpdatePositions();
			//if(SMM.bIsStretch != SMM.bIsStretch){ 
				if(SMM.bIsStretch){
					SMM.CollisionType = GSD.Roads.Splination.CollisionTypeEnum.BoxCollision;
					SMM.bMatchRoadDefinition = false;
					SMM.bMatchTerrain = false;
					SMM.bCollisionConvex = false;
					SMM.bStartDown = false;	
					SMM.bEndDown = false;
					SMM.bVerticalCutoff = false;
					SMM.bExactSplination = false;
					SMM.bEndTypeDownOverride = false;
				}
			//}				
			SMM.Setup(true);
		}
		
		UpdateEdgeObjects_OnUndo();
	}
	
	public void DoEdgeObjects(){
		if(!tNode.CanSplinate()){ return; }
		
		if(tNode.EdgeObjects == null){ 
			tNode.EdgeObjects = new List<GSD.Roads.EdgeObjects.EdgeObjectMaker>(); 
		}
		eCount = tNode.EdgeObjects.Count;

		EOM = null;

		for(int i=0;i<tNode.EdgeObjects.Count;i++){
			EOM = tNode.EdgeObjects[i];
			if(EOM.EM == null){
				EOM.EM = new GSD.Roads.EdgeObjects.EdgeObjectMaker.EdgeObjectEditorMaker();	
			}
			EOM.EM.Setup(EOM);
			
			currentCount +=1;
			EditorGUILayout.BeginVertical("TextArea");
			

			if(EOM.bNeedsUpdate){ EOM.Setup(); }
			EOM.bNeedsUpdate = false;
			
			EditorGUILayout.BeginHorizontal();

			EOM.bToggle = EditorGUILayout.Foldout(EOM.bToggle,"#" + currentCount.ToString() + ": " + EOM.tName);
			
			if(GUILayout.Button(btnEdgeText,GSDImageButton,GUILayout.Width(32f))){
				
			}
			if(GUILayout.Button(btnRefreshText,GSDImageButton,GUILayout.Width(16f))){
				EOM.Setup();
			}
			if(GUILayout.Button(btnSaveText,GSDImageButton,GUILayout.Width(16f))){
				GSDSaveWindow tSave = EditorWindow.GetWindow<GSDSaveWindow>();
				tSave.Initialize(ref tSceneRect,GSDSaveWindow.WindowTypeEnum.Edge,tNode,null,EOM);
			}
			
			if(GUILayout.Button(btnCopyText,GSDImageButton,GUILayout.Width(16f))){
				Undo.RecordObject(tNode,"Copy");
				tNode.CopyEdgeObject(i);
				EditorUtility.SetDirty(tNode);
			}
			if(GUILayout.Button(btnDeleteText,GSDImageButton,GUILayout.Width(16f))){
				Undo.RecordObject(tNode,"Delete");
				tNode.RemoveEdgeObject(i);
				EditorUtility.SetDirty(tNode);
			}
			EditorGUILayout.EndHorizontal();
			
			if(!EOM.bToggle){ EditorGUILayout.EndVertical(); continue; }
			
			GUILayout.Space(8f);
			EditorGUILayout.BeginHorizontal(); 
			EditorGUILayout.LabelField("General options:");
			if(GUILayout.Button("Online manual",EditorStyles.miniButton,GUILayout.Width(120f))){
				Application.OpenURL("http://microgsd.com/Support/RoadArchitectManual.aspx");
			}
			EditorGUILayout.EndHorizontal(); 
			
			EditorGUILayout.BeginVertical("box");
			//Name:
			EOM.EM.tName = EditorGUILayout.TextField("Name: ",EOM.tName);

			//Edge object:
		    EOM.EM.EdgeObject = (GameObject)EditorGUILayout.ObjectField("Edge object: ",EOM.EdgeObject,typeof(GameObject), false);
			if(EOM.EM.EdgeObject != EOM.EdgeObject){ 
				EOM.bEdgeSignLabelInit = false;
				EOM.bEdgeSignLabel = false;
			}

			//Material override:
			EOM.EM.bMaterialOverride = EditorGUILayout.Toggle("Material override: ",EOM.bMaterialOverride);
			if(!EOM.bMaterialOverride){
				EOM.EM.EdgeMaterial1 = null;
				EOM.EM.EdgeMaterial2 = null;
			}

			if(!EOM.bEdgeSignLabelInit){
				EOM.bEdgeSignLabel = false;
				if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSignDiamond") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-diamond";
					
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSignSquare-Small") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-Square";
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSignSquare") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-Square";
					
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign988-Small") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-988";
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign988") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-988";
					
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign861-Small") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-861";
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign861") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-861";
					
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign617-Small") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-617";
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign617") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-617";
					
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign396") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-396";
					
				}else if(string.CompareOrdinal(EOM.EM.EdgeObject.name,"GSDSign330") == 0){
					EOM.bEdgeSignLabel = true;
					EOM.EdgeSignLabel = "GSDFedSign-330";
				}
			}

			if(EOM.bMaterialOverride){
				if(EOM.bEdgeSignLabel){
					EditorGUILayout.TextField("Material search term: ",EOM.EdgeSignLabel);
				}
			
				EOM.EM.EdgeMaterial1 = (Material)EditorGUILayout.ObjectField("Override mat #1: ",EOM.EdgeMaterial1,typeof(Material), false);
				EOM.EM.EdgeMaterial2 = (Material)EditorGUILayout.ObjectField("Override mat #2: ",EOM.EdgeMaterial2,typeof(Material), false);
			}

			if(EOM.bSingle){
				EOM.EM.bCombineMesh = false;
			}else{
				EOM.EM.bCombineMesh = EditorGUILayout.Toggle("Combine meshes: ",EOM.bCombineMesh);

				if(EOM.bCombineMesh){
					EOM.EM.bCombineMeshCollider = EditorGUILayout.Toggle("Combined mesh collider: ",EOM.bCombineMeshCollider);
				}
			}
			
			EOM.EM.bSingle = EditorGUILayout.Toggle("Single object only: ",EOM.bSingle);
			if(EOM.EM.bSingle != EOM.bSingle){ 
				EOM.EM.EndTime = tNode.NextTime;
//				EOM.EM.EndPos = tNode.GSDSpline.GetSplineValue(EOM.EM.EndTime,false);
				EOM.EM.SinglePosition = tNode.tTime+0.025f;
				if(EOM.EM.bSingle){
					EOM.EM.bCombineMesh = false;
				}
			}
			
			if(EOM.bSingle){
				EOM.EM.SinglePosition = EditorGUILayout.Slider("Single location: ",EOM.SinglePosition,tNode.tTime,1f);

				if(tNode.bIsBridgeStart && tNode.bIsBridgeMatched){					
					EOM.EM.SingleOnlyBridgePercent = EditorGUILayout.Slider("Bridge %: ",EOM.SingleOnlyBridgePercent,0f,1f);
					if(!GSDRootUtil.IsApproximately(EOM.SingleOnlyBridgePercent,EOM.EM.SingleOnlyBridgePercent,0.001f)){
						EOM.EM.SingleOnlyBridgePercent = Mathf.Clamp(EOM.EM.SingleOnlyBridgePercent,0f,1f);
						float tDist = (EOM.EM.SingleOnlyBridgePercent * (tNode.BridgeCounterpartNode.tDist - tNode.tDist) + tNode.tDist); 
						EOM.EM.SinglePosition = tNode.GSDSpline.TranslateDistBasedToParam(tDist);
					}
				}
			}

			EOM.EM.bStatic = EditorGUILayout.Toggle("Static: ",EOM.bStatic);
			EOM.EM.bMatchTerrain = EditorGUILayout.Toggle("Match ground height: ",EOM.bMatchTerrain);

			if(!EOM.bSingle){
				EOM.EM.MeterSep = EditorGUILayout.Slider("Dist between objects: ",EOM.MeterSep,1f,256f);
			}
			
			EOM.EM.bStartMatchRoadDefinition = EditorGUILayout.Toggle("Match road definition: ",EOM.bStartMatchRoadDefinition); 
			if(EOM.bStartMatchRoadDefinition){
				EOM.EM.StartMatchRoadDef = EditorGUILayout.Slider("Position fine tuning: ",EOM.StartMatchRoadDef,0f,1f);
				if(!GSDRootUtil.IsApproximately(EOM.EM.StartMatchRoadDef,EOM.StartMatchRoadDef,0.001f)){
					EOM.EM.StartMatchRoadDef = Mathf.Clamp(EOM.EM.StartMatchRoadDef,0f,1f);
				}
			}
			
			if(!EOM.bSingle){
				if(EOM.EM.StartTime < tNode.MinSplination){ EOM.EM.StartTime = tNode.MinSplination; }
				if(EOM.EM.EndTime > tNode.MaxSplination){ EOM.EM.EndTime = tNode.MaxSplination; }

				EditorGUILayout.BeginHorizontal();
				EOM.EM.StartTime = EditorGUILayout.Slider("Start param: ",EOM.StartTime,tNode.MinSplination,EOM.EndTime);
				if(EOM.EM.EndTime < EOM.EM.StartTime){
					EOM.EM.EndTime = Mathf.Clamp(EOM.StartTime+0.01f,0f,1f);
				}
				if(GUILayout.Button("match node",EditorStyles.miniButton, GUILayout.Width(80f))){
					EOM.EM.StartTime = tNode.tTime;
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EOM.EM.EndTime = EditorGUILayout.Slider("End param: ",EOM.EndTime,EOM.StartTime,tNode.MaxSplination);
				if(EOM.EM.StartTime > EOM.EM.EndTime){
					EOM.EM.StartTime = Mathf.Clamp(EOM.EndTime-0.01f,0f,1f);
				}
				if(GUILayout.Button("match next",EditorStyles.miniButton, GUILayout.Width(80f))){
					EOM.EM.EndTime = tNode.NextTime;
				}
				EditorGUILayout.EndHorizontal();
			}
		
			EditorGUILayout.EndVertical();
			
			//Vertical offset:
			EditorGUILayout.LabelField("Vertical options:");
			EditorGUILayout.BeginVertical("box");
			
			EditorGUILayout.BeginHorizontal();
			EOM.EM.VerticalRaise = EditorGUILayout.Slider("Vertical raise magnitude:",EOM.VerticalRaise,-512f,512f); 
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				EOM.EM.VerticalRaise = 0f;
			}
			EditorGUILayout.EndHorizontal();
			
			if(EOM.VerticalCurve == null || EOM.VerticalCurve.keys.Length < 2){ EnforceCurve(ref EOM.VerticalCurve); }
			EditorGUILayout.BeginHorizontal();
			EOM.EM.VerticalCurve = EditorGUILayout.CurveField("Curve: ",EOM.VerticalCurve);
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				ResetCurve(ref EOM.EM.VerticalCurve);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			//Horizontal offsets:
			EditorGUILayout.LabelField("Horizontal offset options:");
			EditorGUILayout.BeginVertical("box");
			tHorizMatching = HorizMatchingDefaultsEnum.None;
			tHorizMatching = (HorizMatchingDefaultsEnum)EditorGUILayout.Popup((int)tHorizMatching,HorizMatchSubTypeDescriptions, GUILayout.Width(100f));
			if(tHorizMatching != HorizMatchingDefaultsEnum.None){
				if(tHorizMatching == HorizMatchingDefaultsEnum.MatchCenter){
					EOM.EM.HorizontalSep = 0f;
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchRoadLeft){
					EOM.EM.HorizontalSep = (tNode.GSDSpline.tRoad.RoadWidth()*0.5f)*-1;
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchShoulderLeft){
					if(tNode.GSDSpline.tRoad.opt_bShouldersEnabled){
						EOM.EM.HorizontalSep = ((tNode.GSDSpline.tRoad.RoadWidth()*0.5f) + tNode.GSDSpline.tRoad.opt_ShoulderWidth)*-1;
					}else{
						EOM.EM.HorizontalSep = ((tNode.GSDSpline.tRoad.RoadWidth()*0.5f))*-1;
					}
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchRoadRight){
					EOM.EM.HorizontalSep = (tNode.GSDSpline.tRoad.RoadWidth()*0.5f);
				}else if(tHorizMatching == HorizMatchingDefaultsEnum.MatchShoulderRight){
					if(tNode.GSDSpline.tRoad.opt_bShouldersEnabled){
						EOM.EM.HorizontalSep = (tNode.GSDSpline.tRoad.RoadWidth()*0.5f) + tNode.GSDSpline.tRoad.opt_ShoulderWidth;
					}else{
						EOM.EM.HorizontalSep = (tNode.GSDSpline.tRoad.RoadWidth()*0.5f);
					}
				}
				tHorizMatching = HorizMatchingDefaultsEnum.None;
			}
			if(!GSDRootUtil.IsApproximately(EOM.EM.HorizontalSep,EOM.HorizontalSep)){
				EOM.EM.HorizontalSep = Mathf.Clamp(EOM.EM.HorizontalSep,(-1f*HorizRoadMax),HorizRoadMax);
			}
			
			
			EditorGUILayout.BeginHorizontal();
			EOM.EM.HorizontalSep = EditorGUILayout.Slider("Horiz offset magnitude:",EOM.EM.HorizontalSep,(-1f*HorizRoadMax),HorizRoadMax);	
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				EOM.EM.HorizontalSep = 0f;
			}
			if(!GSDRootUtil.IsApproximately(EOM.EM.HorizontalSep,EOM.HorizontalSep)){
				EOM.EM.HorizontalSep = Mathf.Clamp(EOM.EM.HorizontalSep,(-1f*HorizRoadMax),HorizRoadMax);
			}
			EditorGUILayout.EndHorizontal();
			if(EOM.HorizontalCurve == null || EOM.HorizontalCurve.keys.Length < 2){ EnforceCurve(ref EOM.HorizontalCurve); }
			EditorGUILayout.BeginHorizontal();
			EOM.EM.HorizontalCurve = EditorGUILayout.CurveField("Curve: ",EOM.HorizontalCurve);
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				ResetCurve(ref EOM.EM.HorizontalCurve);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("Rotation options:");
			EditorGUILayout.BeginVertical("box");
			if(EOM.HorizontalSep < 0f){
				EOM.EM.bOncomingRotation = EditorGUILayout.Toggle("Auto rotate oncoming objects: ",EOM.bOncomingRotation);
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Custom rotation: ");
			if(GUILayout.Button(btnDefaultText,GSDImageButton,GUILayout.Width(16f))){
				EOM.EM.CustomRotation = new Vector3(0f,0f,0f);
			}
			EditorGUILayout.EndHorizontal();

			EOM.EM.CustomRotation.x = EditorGUILayout.Slider("x-axis: ",EOM.CustomRotation.x,-360f,360f);
			EOM.EM.CustomRotation.y = EditorGUILayout.Slider("y-axis: ",EOM.CustomRotation.y,-360f,360f);
			EOM.EM.CustomRotation.z = EditorGUILayout.Slider("z-axis: ",EOM.CustomRotation.z,-360f,360f);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
		}
	}
	
	public void UpdateEdgeObjects(){
		if(!tNode.CanSplinate()){ return; }
		eCount = tNode.EdgeObjects.Count;
		for(int i=0;i<tNode.EdgeObjects.Count;i++){
			EOM = tNode.EdgeObjects[i];
			if(EOM.EM != null){
				if(!EOM.EM.IsEqual(EOM)){
					EOM.EM.LoadTo(EOM);
					EOM.UpdatePositions();
					EOM.Setup(); 
//					Debug.Log ("Setup EOM"); 
				}
			}
		}
	}
	
	public void UpdateEdgeObjects_OnUndo(){
		if(!tNode.CanSplinate()){ return; }
		eCount = tNode.EdgeObjects.Count;
		for(int i=0;i<tNode.EdgeObjects.Count;i++){
			EOM = tNode.EdgeObjects[i];
			EOM.Setup();
		}
	}
	
	#region "Quick adds"
	private void BridgeAdd_TopBase(float tHorizSep = 0f, float tVertRaise = -0.01f, string tMat = "Assets/RoadArchitect/Materials/GSDConcrete2.mat", bool bOverridePrefab = false, string OverridePrefab = ""){
		SMM = tNode.AddSplinatedObject();
		string tBridgeTopBaseToAdd = "";
		string tName = "";
		if(tNode.GSDSpline.tRoad.opt_Lanes == 2){
			if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base1MOver){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-19w-5l-1d.fbx";
				tName = "BridgeTop1M-1M";
			}else if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base2MOver){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-20w-5l-1d.fbx";
				tName = "BridgeTop2M-1M";
			}else if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base3MDeep){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-18w-5l-3d.fbx";
				tName = "BridgeTop0M-3M";
			}else{
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-18w-5l-1d.fbx";
				tName = "BridgeTop0M-1M";
			}
		}else if(tNode.GSDSpline.tRoad.opt_Lanes == 4){
			if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base1MOver){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-29w-5l-1d.fbx";
				tName = "BridgeTop1M-1M";
			}else if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base2MOver){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-30w-5l-1d.fbx";
				tName = "BridgeTop2M-1M";
			}else if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base3MDeep){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-28w-5l-3d.fbx";
				tName = "BridgeTop0M-3M";
			}else{
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-28w-5l-1d.fbx";
				tName = "BridgeTop0M-1M";
			}
		}else{
			if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base1MOver){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-39w-5l-1d.fbx";
				tName = "BridgeTop1M-1M";
			}else if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base2MOver){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-40w-5l-1d.fbx";
				tName = "BridgeTop2M-1M";
			}else if(tBridgeTopBaseQuickAdd == BridgeTopBaseDefaultsEnum.Base3MDeep){
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-38w-5l-3d.fbx";
				tName = "BridgeTop0M-3M";
			}else{
				tBridgeTopBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase-38w-5l-1d.fbx";
				tName = "BridgeTop0M-1M";
			}
		}
		
		if(bOverridePrefab){ tBridgeTopBaseToAdd = OverridePrefab; }
		
		SMM.tName = tName;
		SMM.CurrentSplination = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(tBridgeTopBaseToAdd, typeof(GameObject));
		SMM.HorizontalSep = tHorizSep;
		SMM.VerticalRaise = tVertRaise;
		SMM.bMaterialOverride = true;
		SMM.SplinatedMaterial1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);	
		SMM.Axis = GSD.Roads.Splination.AxisTypeEnum.Z;
		
		tBridgeTopBaseQuickAdd = BridgeTopBaseDefaultsEnum.None;
		if(SMM.StartTime < tNode.MinSplination){ SMM.StartTime = tNode.MinSplination; }
		if(SMM.EndTime > tNode.MaxSplination){ SMM.EndTime = tNode.MaxSplination; }	
	}

	private void BridgeAdd_BottomBase(float tHorizSep = 0f, float tVertRaise = -1.01f, string tMat = "Assets/RoadArchitect/Materials/GSDConcrete2.mat", bool bOverridePrefab = false, string OverridePrefab = ""){
		SMM = tNode.AddSplinatedObject();
		string tBridgeBottomBaseToAdd = "";
		string tName = "";
		if(tNode.GSDSpline.tRoad.opt_Lanes == 2){
			if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase2){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase2-18w-5l-3d.fbx";
				tName = "BridgeBase2";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase3){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase3-18w-5l-5d.fbx";
				tName = "BridgeBase3";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase4){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase4-18w-5l-5d.fbx";
				tName = "BridgeBase4";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase5){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase5-18w-5l-5d.fbx";
				tName = "BridgeBase5";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase6){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase6-2L.fbx";
				tName = "BridgeArchBeam80";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase7){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase7-2L.fbx";
				tName = "BridgeArchSolid80";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase8){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase8-2L.fbx";
				tName = "BridgeArchSolid180";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBaseGrid){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBaseGrid-18w-5l-5d.fbx";
				tName = "BridgeGrid";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeSteel){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBaseSteelBeam-18w-20l-3d.fbx";
				tName = "BridgeSteelBeams";
			}
		}else if(tNode.GSDSpline.tRoad.opt_Lanes == 4){
			if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase2){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase2-28w-5l-3d.fbx";
				tName = "BridgeBase2";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase3){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase3-28w-5l-5d.fbx";
				tName = "BridgeBase3";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase4){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase4-28w-5l-5d.fbx";
				tName = "BridgeBase4";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase5){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase5-28w-5l-5d.fbx";
				tName = "BridgeBase5";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase6){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase6-4L.fbx";
				tName = "BridgeArchBeam80";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase7){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase7-4L.fbx";
				tName = "BridgeArchSolid80";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase8){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase8-4L.fbx";
				tName = "BridgeArchSolid180";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBaseGrid){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBaseGrid-28w-5l-5d.fbx";
				tName = "BridgeGrid";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeSteel){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBaseSteelBeam-28w-20l-3d.fbx";
				tName = "BridgeSteelBeams";
			}
		}else{
			if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase2){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase2-38w-5l-3d.fbx";
				tName = "BridgeBase2";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase3){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase3-38w-5l-5d.fbx";
				tName = "BridgeBase3";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase4){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase4-38w-5l-5d.fbx";
				tName = "BridgeBase4";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase5){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase5-38w-5l-5d.fbx";
				tName = "BridgeBase5";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase6){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase6-6L.fbx";
				tName = "BridgeArchBeam80";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase7){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase7-6L.fbx";
				tName = "BridgeArchSolid80";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase8){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBase8-6L.fbx";
				tName = "BridgeArchSolid180";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBaseGrid){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBaseGrid-38w-5l-5d.fbx";
				tName = "BridgeGrid";
			}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeSteel){
				tBridgeBottomBaseToAdd = "Assets/RoadArchitect/Mesh/RoadObj/Bridges/BridgeBaseSteelBeam-38w-20l-3d.fbx";
				tName = "BridgeBeams";
			}
		}
		
		if(bOverridePrefab){ tBridgeBottomBaseToAdd = OverridePrefab; }
		
		SMM.CurrentSplination = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(tBridgeBottomBaseToAdd, typeof(GameObject));
		SMM.HorizontalSep = tHorizSep;
		SMM.VerticalRaise = tVertRaise;
		SMM.bMaterialOverride = true;
		SMM.tName = tName;
		
		if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase2){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase3){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase4){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase5){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase6){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase7){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBase8){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial(tMat);
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeBaseGrid){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDSteel7.mat");
		}else if(tBridgeBottomBaseQuickAdd == BridgeBottomBaseDefaultsEnum.BridgeSteel){
			SMM.SplinatedMaterial1 =  GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDSteel7.mat");
		}
		
		SMM.Axis = GSD.Roads.Splination.AxisTypeEnum.Z;
		
		tBridgeTopBaseQuickAdd = BridgeTopBaseDefaultsEnum.None;
		if(SMM.StartTime < tNode.MinSplination){ SMM.StartTime = tNode.MinSplination; }
		if(SMM.EndTime > tNode.MaxSplination){ SMM.EndTime = tNode.MaxSplination; }
	}
	
	private void ExtrusionQuickAdd(bool bHorizOverride = false, float tHorizSep = 0f, bool bVertOverride = false, float tVertRaise = 0f){
		try{
			ExtrusionQuickAdd_Do();
		}catch(System.Exception e){
			tSMMQuickAdd = SMMDefaultsEnum.None;
			throw e;
		}
	}
	
	private void ExtrusionQuickAdd_Do(){
		if(tSMMQuickAdd == SMMDefaultsEnum.KRail){
			tNode.SplinatedObjectQuickAdd("KRail");
		}
	}
	
	private void ExtrudeHelper(string tPath, string tName, float DefaultHoriz, GSD.Roads.Splination.AxisTypeEnum tAxis = GSD.Roads.Splination.AxisTypeEnum.Z, bool bHorizOverride = false, float tHorizSep = 0f, bool bVertOverride = false, float tVertRaise = 0f, bool bFlipRot = false){
		SMM = tNode.AddSplinatedObject();
		SMM.CurrentSplination = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(tPath, typeof(GameObject));
		
		if(bHorizOverride){
			SMM.HorizontalSep = tHorizSep;
		}else{
			SMM.HorizontalSep = ((tNode.GSDSpline.tRoad.RoadWidth()/2) + tNode.GSDSpline.tRoad.opt_ShoulderWidth)*-1f;
		}
		
		if(bVertOverride){
			SMM.VerticalRaise = tVertRaise;
		}else{
			if(tNode.bIsBridgeStart){ SMM.VerticalRaise = -0.01f; }
		}
		
		SMM.bFlipRotation = bFlipRot;
		SMM.Axis = tAxis;
		if(SMM.StartTime < tNode.MinSplination){ SMM.StartTime = tNode.MinSplination; }
		if(SMM.EndTime > tNode.MaxSplination){ SMM.EndTime = tNode.MaxSplination; }
		SMM.tName = tName;
	}
	#endregion

    public void OnSceneGUI() {
		Event current = Event.current;
		int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
		bool bUsed = false;

		if(!bSceneRectSet){
			try{
				tSceneRect = EditorWindow.GetWindow<SceneView>().position;	
			}catch{
				tSceneRect = EditorWindow.GetWindow<EditorWindow>().position;	
			}
			bSceneRectSet = true;
		}
			
		if(!tNode.bEditorSelected){
			tNode.bEditorSelected = true;	
		}
		
		if(current.type == EventType.ValidateCommand){
		    switch (current.commandName){
			    case "UndoRedoPerformed":
					UpdateSplineObjects_OnUndo();
			    break;
		    }
	    }
		
		if(controlID != tNode.GetHashCode()){ tNode.bEditorSelected = false; }
		
		//Drag with left click:
		if (Event.current.type == EventType.MouseDrag && Event.current.button == 0){
			bMouseDragHasProcessed = false;
			tNode.bGizmoDrawIntersectionHighlight = true;
		}
		//Drag with left click release:
		if(Event.current.type == EventType.MouseUp && Event.current.button == 0){
			Object[] xNodeObjects = GameObject.FindObjectsOfType(typeof(GSDSplineN));
			foreach(GSDSplineN xNode in xNodeObjects){
				if(Vector3.Distance(xNode.transform.position,tNode.transform.position) < 33f){
					if(xNode == tNode){ continue; }
					if(tNode.bSpecialEndNode || xNode.bSpecialEndNode){ continue; }
					if(xNode.bIsEndPoint && tNode.bIsEndPoint){ 
						//End point connection.
						tNode.transform.position = xNode.transform.position;
						//Activate special end node for tnode
						TriggerRoadConnection(tNode,xNode);
						bUsed = true;
						break;
					}
					if(xNode.bIsIntersection){ continue; }
					if(xNode.bNeverIntersect){ continue; }
					if(tNode.bIsEndPoint && xNode.bIsEndPoint){ continue; }
					if(xNode.GSDSpline == tNode.GSDSpline){ //Don't let intersection be created on consecutive nodes:
						if((tNode.idOnSpline+1) == xNode.idOnSpline || (tNode.idOnSpline-1) == xNode.idOnSpline){
							continue;	
						}
					}
					tNode.transform.position = xNode.transform.position;
					TriggerIntersection(tNode,xNode);
					bUsed = true;
					break;
				}else{
					continue;	
				}
			}
			
			if(!bMouseDragHasProcessed){
				//Enforce maximum road grade:
				if(tNode.IsLegitimate() && tNode.GSDSpline.tRoad.opt_bMaxGradeEnabled){
					tNode.EnsureGradeValidity();
				}
				TriggerRoadUpdate();
				bUsed = true;
			} 
			bMouseDragHasProcessed = true;
			tNode.bGizmoDrawIntersectionHighlight = false;
			bUsed = true;
		}
		
		//Enforce maximum road grade:
		if(bMouseDragHasProcessed){
			Vector3 vChangeChecker = tNode.transform.position;
			if(VectorDiff(vChangeChecker,tNode.pos)){
				tNode.pos = vChangeChecker;
				if(tNode.IsLegitimate() && tNode.GSDSpline.tRoad.opt_bMaxGradeEnabled){
					tNode.EnsureGradeValidity();
				}
				TriggerRoadUpdate();
			}
			bUsed= true;
		}
		
		if(Selection.activeGameObject == tNode.transform.gameObject){
			if(current.keyCode == KeyCode.F5){
				TriggerRoadUpdate();
			}
		}
		
		if(bUsed){
//			switch(current.type){
//				case EventType.layout:
//			        HandleUtility.AddDefaultControl(controlID);
//			    break;
//			}
		}

		if(GUI.changed){ 
			EditorUtility.SetDirty(tNode); 
		}
    }
	
	private bool VectorDiff(Vector3 tVect1, Vector3 tVect2){
		if(!GSDRootUtil.IsApproximately(tVect1.x,tVect2.x,0.0001f)){
			return true;
		}
		if(!GSDRootUtil.IsApproximately(tVect1.y,tVect2.y,0.0001f)){
			return true;
		}
		if(!GSDRootUtil.IsApproximately(tVect1.z,tVect2.z,0.0001f)){
			return true;
		}
		return false;
	}
	
	private void TriggerRoadConnection(GSDSplineN tNode1, GSDSplineN tNode2){
    	tNode.GSDSpline.ActivateEndNodeConnection(tNode1,tNode2);
	}
	
	private void TriggerIntersection(GSDSplineN tNode1, GSDSplineN tNode2){
		bCreateIntersection = true;
		iNode1 = tNode1;
		iNode2 = tNode2;
		Selection.activeGameObject = GSD.Roads.GSDIntersections.CreateIntersection(tNode1,tNode2);
	}
	
	private void TriggerRoadUpdate(){
		if(tNode != null){
    		tNode.GSDSpline.tRoad.EditorUpdateMe = true;
		}
	}
	
	void Line(){
		GUILayout.Space(4f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f)); //Horizontal bar
		GUILayout.Space(4f);
	}
	
	void LineSmall(){
		GUILayout.Space(2f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f)); //Horizontal bar
		GUILayout.Space(2f);
	}
	
	void BigLine(){
		GUILayout.Space(4f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4f)); //Horizontal bar
		GUILayout.Space(4f);
	}
	
	void ResetCurve(ref AnimationCurve tCurve){
		tCurve = null;
		tCurve = new AnimationCurve();
		EnforceCurve(ref tCurve);
	}
	
	bool V3Equal(ref Vector3 V1,ref Vector3 V2){
		if(!GSDRootUtil.IsApproximately(V1.x,V2.x,0.001f)){
			return false;
		}
		if(!GSDRootUtil.IsApproximately(V1.y,V2.y,0.001f)){
			return false;
		}
		if(!GSDRootUtil.IsApproximately(V1.z,V2.z,0.001f)){
			return false;
		}
		return true;
	}
	
	void EnforceCurve(ref AnimationCurve tCurve){
		if(tCurve.keys.Length == 0){
			tCurve.AddKey(0f,1f);
			tCurve.AddKey(1f,1f);
		}else if(tCurve.keys.Length == 1){
			tCurve.keys[0].time = 0f;
			tCurve.AddKey(1f,1f);
		}
	}
	
	GameObject GetEndObjectQuickAdd(){
		string tPath = "";
		if(tEndObjectAdd == EndObjectsDefaultsEnum.WarningSign1_Static){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDWarningSign_Static.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.WarningSign2_Static){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDWarningSign2_Static.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.Atten_Static){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDAtten_Static.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.Barrel1_Static){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDRoadBarrel_Static.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.Barrel1_Rigid){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDRoadBarrel_Rigid.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.Barrel3_Static){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDRoadBarrel3_Static.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.Barrel3_Rigid){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDRoadBarrel3_Rigid.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.Barrel7_Static){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDRoadBarrel7_Static.prefab";
		}else if(tEndObjectAdd == EndObjectsDefaultsEnum.Barrel7_Rigid){
			tPath = "Assets/RoadArchitect/Mesh/RoadObj/Interactive/GSDRoadBarrel7_Rigid.prefab";
		}else{
			return null;	
		}
		
		return (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(tPath, typeof(GameObject)) as GameObject;
	}
}
