using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using GSD.Roads;
using GSD;
using UnityEditor;
#endif
[ExecuteInEditMode]
public class GSDRoad : MonoBehaviour{
	#if UNITY_EDITOR
	
	public GameObject MainMeshes;
	public GameObject MeshRoad;
	public GameObject MeshShoR;
	public GameObject MeshShoL;
	public GameObject MeshiLanes;
	public GameObject MeshiLanes0;
	public GameObject MeshiLanes1;
	public GameObject MeshiLanes2;
	public GameObject MeshiLanes3;
	public GameObject MeshiMainPlates;
	public GameObject MeshiMarkerPlates;
	[System.NonSerialized] 
	public string EditorTitleString = "";

	public GSDSplineC GSDSpline;
	public int MostRecentNodeCount = -1;
//	private bool bMostRecentCheck = false;
	public GameObject GSDSplineObj;
	public GSDRoadSystem GSDRS;
	public GSDSplineC[] PiggyBacks = null; 
	public bool bEditorProgressBar = false;
	public string UID; //Unique ID
	
	[SerializeField]
	public List<GSDTerrainHistoryMaker> TerrainHistory;
	public string TerrainHistoryByteSize = "";
	
	[System.NonSerialized]
	public bool bUpdateSpline = false;

	//Road editor options: 
	public float	opt_LaneWidth = 5f;					//Done.
	public bool 	opt_bShouldersEnabled = true;		//Disabled for now. Comprimises integrity of roads.
	public float 	opt_ShoulderWidth = 3f;				//Done.
	public int 		opt_Lanes = 2;						//Done.
	public float 	opt_RoadDefinition = 5f;			//Done.
	public bool 	opt_RoadCornerDefinition = false;   //Disable for now. No point.
	public bool 	opt_bRoadCuts = true;
	public bool 	opt_bShoulderCuts = true;
	public bool 	opt_bDynamicCuts = false;
	public bool		opt_bMaxGradeEnabled = true;
	public float	opt_MaxGrade = 0.08f;
	public bool 	opt_UseDefaultMaterials = true;
	public bool 	opt_AutoUpdateInEditor = true;
	
	public float 	opt_TerrainSubtract_Match = 0.01f;
	public bool		opt_bGSDRoadRaise = false;
	
	public float 	opt_MatchHeightsDistance = 50f;
	public float 	opt_ClearDetailsDistance = 30f;
	public float 	opt_ClearDetailsDistanceHeight = 5f;
	public float 	opt_ClearTreesDistance = 30f;
	public float 	opt_ClearTreesDistanceHeight = 50f;
	
	public bool		opt_HeightModEnabled = true;
	public bool		opt_DetailModEnabled = true;
	public bool		opt_TreeModEnabled = true;
	
	public bool		opt_SaveTerrainHistoryOnDisk = true;
	public float	opt_MagnitudeThreshold = 300f;
	public bool		opt_GizmosEnabled = true;
	public bool 	opt_bMultithreading = true;
	public bool		opt_bSaveMeshes = false;
    public bool     opt_bUseMeshColliders = true;
    public bool     opt_bIsStatic = false;
    public bool     opt_bIsLightmapped = false;

    public enum RoadMaterialDropdownEnum {
        Asphalt,
        Dirt,
        Brick,
        Cobblestone
    };
    public RoadMaterialDropdownEnum opt_tRoadMaterialDropdown = RoadMaterialDropdownEnum.Asphalt;
    public RoadMaterialDropdownEnum tRoadMaterialDropdownOLD = RoadMaterialDropdownEnum.Asphalt;

	
	public Material RoadMaterial1;
	public Material RoadMaterial2;
	public Material RoadMaterial3;
	public Material RoadMaterial4;
	public Material RoadMaterialMarker1;
	public Material RoadMaterialMarker2;
	public Material RoadMaterialMarker3;
	public Material RoadMaterialMarker4;
	public Material ShoulderMaterial1;
	public Material ShoulderMaterial2;
	public Material ShoulderMaterial3;
	public Material ShoulderMaterial4;
	public Material ShoulderMaterialMarker1;
	public Material ShoulderMaterialMarker2;
	public Material ShoulderMaterialMarker3;
	public Material ShoulderMaterialMarker4;
	
	public PhysicMaterial RoadPhysicMaterial;
	public PhysicMaterial ShoulderPhysicMaterial;
	
	#region "Road Construction"
	[System.NonSerialized]
	public GSD.Threaded.TerrainCalcs TerrainCalcsJob;
	[System.NonSerialized]
	public GSD.Threaded.RoadCalcs1 RoadCalcsJob1;
	[System.NonSerialized]
	public GSD.Threaded.RoadCalcs2 RoadCalcsJob2;
	[System.NonSerialized]
	public RoadConstructorBufferMaker RCS;

	public string tName = "";
	public bool bProfiling = false;
	public bool bSkipStore = true;
	[System.NonSerialized]
	public float EditorConstructionStartTime = 0f;

	void CleanRunTime(){
		//Make sure unused items are not using memory space in runtime:
		TerrainHistory = null;
		RCS = null;
	}

	public bool bEditorError = false;
	public System.Exception tError = null;
	void OnEnable(){
		if(!Application.isEditor){ return; }
//		if(Application.isEditor && !UnityEditor.EditorApplication.isPlaying){
			Editor_bIsConstructing = false;
			UnityEditor.EditorApplication.update += delegate { EditorUpdate(); };
#if UNITY_2018_1_OR_NEWER
        UnityEditor.EditorApplication.hierarchyChanged += delegate { hWindowChanged(); };
#else
        UnityEditor.EditorApplication.hierarchyWindowChanged += delegate { hWindowChanged(); };
#endif
        //		}
        if (GSDSpline == null || GSDSpline.mNodes == null){
			MostRecentNodeCount = 0;
		}else{
			MostRecentNodeCount = GSDSpline.GetNodeCount();
		}
        tRoadMaterialDropdownOLD = opt_tRoadMaterialDropdown;
		CheckMats();
	}

	public void Awake() {
		if(GSDSpline == null || GSDSpline.mNodes == null){
			MostRecentNodeCount = 0;
		}else{
			MostRecentNodeCount = GSDSpline.GetNodeCount();	
		}
	}

	int EditorTimer=0;
	int EditorTimerMax=0;
	int EditorTimerSpline = 0;
	const int EditorTimerSplineMax = 2;
	[System.NonSerialized]
	public int EditorProgress = 0;
	const int GizmoNodeTimerMax = 2;
	public bool EditorUpdateMe = false;
	public bool bTriggerGC = false;
	bool bTriggerGC_Happening;
	float TriggerGC_End = 0f;
	private void EditorUpdate(){
		if(!Application.isEditor){
			UnityEditor.EditorApplication.update -= delegate { EditorUpdate(); };
		}
		
		if(this == null){ 
			UnityEditor.EditorApplication.update -= delegate { EditorUpdate(); };
			Editor_bIsConstructing = false; 
			EditorUtility.ClearProgressBar(); 
			return; 
		}
		
		//Custom garbage collection demands for editor:
		if(bTriggerGC){
			bTriggerGC = false;
			TriggerGC_End = Time.realtimeSinceStartup + 1f;
			bTriggerGC_Happening = true;
		}
		if(bTriggerGC_Happening){
			if(Time.realtimeSinceStartup > TriggerGC_End){
				bTriggerGC_Happening = false;
				GSDRootUtil.ForceCollection();
				TriggerGC_End = 200000f;
			}
		}
		
		if(Editor_bIsConstructing){ // && !Application.isPlaying && !UnityEditor.EditorApplication.isPlaying){
			if(GSDRS != null){
				if(GSDRS.opt_bMultithreading){
					EditorTimer+=1;
					if(EditorTimer > EditorTimerMax){
						if((Time.realtimeSinceStartup - EditorConstructionStartTime) > 180f){
							Editor_bIsConstructing = false;
							EditorUtility.ClearProgressBar();	
							Debug.Log ("Update shouldn't take longer than 180 seconds. Aborting update.");
						}
						
						EditorTimer=0;
						if(bEditorError){
							Editor_bIsConstructing = false;
							EditorUtility.ClearProgressBar();	
							bEditorError = false;
							if(tError != null){
								throw tError;
							}
						}
						
						if(TerrainCalcsJob != null && TerrainCalcsJob.Update()){
							ConstructRoad2();
						}else if(RoadCalcsJob1 != null && RoadCalcsJob1.Update()){
							ConstructRoad3();
						}else if(RoadCalcsJob2 != null && RoadCalcsJob2.Update()){
							ConstructRoad4();
						}
					}
				}
			}
		}else{
			if(EditorUpdateMe && !Editor_bIsConstructing){
				EditorUpdateMe = false;
				GSDSpline.Setup_Trigger();
			}
		}
		
		if(Editor_bIsConstructing){
			RoadUpdateProgressBar();	
		}else if(bEditorProgressBar){
			RoadUpdateProgressBar();	
		}

		if(!Application.isPlaying && bUpdateSpline && !UnityEditor.EditorApplication.isPlaying){
			EditorTimerSpline += 1;
			if(EditorTimerSpline > EditorTimerSplineMax){
				EditorTimerSpline = 0;	
				bUpdateSpline = false;
				GSDSpline.Setup_Trigger();
				MostRecentNodeCount = GSDSpline.mNodes.Count;
			}
		}
		
		if(bEditorCameraMoving && EditorCameraNextMove < EditorApplication.timeSinceStartup){
			EditorCameraNextMove = (float)EditorApplication.timeSinceStartup+EditorCameraTimeUpdateInterval;
			DoEditorCameraLoop();
		}
	}
	
	[System.NonSerialized]
	public bool bEditorCameraMoving = false;
	[System.NonSerialized]
	public float EditorCameraPos = 0f;
//	float EditorCameraPos_Full = 0f;
	const float EditorCameraTimeUpdateInterval = 0.015f;
	float EditorCameraNextMove = 0f;
	bool bEditorCameraSetup = false;
	float EditorCameraStartPos = 0f;
	float EditorCameraEndPos = 1f;
	float EditorCameraIncrementDistance = 0f;
	float EditorCameraIncrementDistance_Full = 0f;
	public float EditorCameraMetersPerSecond = 60f;
	public bool bEditorCameraRotate = false;
	Vector3 EditorCameraV1 = default(Vector3);
	Vector3 EditorCameraV2 = default(Vector3);
	[System.NonSerialized]
	public Vector3 EditorCameraOffset = new Vector3(0f,5f,0f);
	[System.NonSerialized]
	public Camera EditorPlayCamera = null;
	Vector3 EditorCameraBadVec = default(Vector3);
		
	public void DoEditorCameraLoop(){
		if(!bEditorCameraSetup){
			bEditorCameraSetup = true;
			if(GSDSpline.bSpecialEndControlNode){	//If control node, start after the control node:
				EditorCameraEndPos = GSDSpline.mNodes[GSDSpline.GetNodeCount()-2].tTime;
			}
			if(GSDSpline.bSpecialStartControlNode){	//If ends in control node, end construction before the control node:
				EditorCameraStartPos = GSDSpline.mNodes[1].tTime;
			}
//			EditorCameraPos_Full = 0f;
			ChangeEditorCameraMetersPerSec();
		}
		
		if(!Selection.Contains(this.transform.gameObject)){
			QuitEditorCamera();
			return; 
		}
		
//		EditorCameraPos_Full+=EditorCameraIncrementDistance_Full;
//		if(EditorCameraPos_Full > GSDSpline.distance){ EditorCameraPos = EditorCameraStartPos; bEditorCameraMoving = false; bEditorCameraSetup = false; EditorCameraPos_Full = 0f; return; }
//		EditorCameraPos = GSDSpline.TranslateDistBasedToParam(EditorCameraPos_Full);
		
		EditorCameraPos += EditorCameraIncrementDistance;
		if(EditorCameraPos > EditorCameraEndPos){ 
			QuitEditorCamera();
			return; 
		}
		if(EditorCameraPos < EditorCameraStartPos){
			EditorCameraPos	= EditorCameraStartPos;
		}
		
		GSDSpline.GetSplineValue_Both(EditorCameraPos,out EditorCameraV1, out EditorCameraV2);

		if(EditorApplication.isPlaying){
			if(EditorPlayCamera != null){
				EditorPlayCamera.transform.position = EditorCameraV1;
				if(bEditorCameraRotate){
					EditorPlayCamera.transform.position += EditorCameraOffset;
					if(EditorCameraV2 != EditorCameraBadVec){
						EditorPlayCamera.transform.rotation = Quaternion.LookRotation(EditorCameraV2);
					}
				}
			}
		}else{
			SceneView.lastActiveSceneView.pivot = EditorCameraV1;
			if(bEditorCameraRotate){
				SceneView.lastActiveSceneView.pivot += EditorCameraOffset;
				if(EditorCameraV2 != EditorCameraBadVec){
					SceneView.lastActiveSceneView.rotation = Quaternion.LookRotation(EditorCameraV2);
				}
			}
			SceneView.lastActiveSceneView.Repaint();
		}
	}
	public void EditorCameraSetSingle(){
		if(EditorPlayCamera == null){
			Camera[] EditorCams = (Camera[])GameObject.FindObjectsOfType(typeof(Camera));
			if(EditorCams != null && EditorCams.Length == 1){
				EditorPlayCamera = EditorCams[0];
			}
		}
	}
	public void QuitEditorCamera(){
		EditorCameraPos = EditorCameraStartPos; 
		bEditorCameraMoving = false; 
		bEditorCameraSetup = false; 
//		EditorCameraPos_Full = 0f;
	}
	public void ChangeEditorCameraMetersPerSec(){
		EditorCameraIncrementDistance_Full = (EditorCameraMetersPerSecond/60);
		EditorCameraIncrementDistance = (EditorCameraIncrementDistance_Full/GSDSpline.distance);	
	}

	private void hWindowChanged(){
		if(!Application.isEditor){
#if UNITY_2018_1_OR_NEWER
            UnityEditor.EditorApplication.hierarchyChanged -= delegate { hWindowChanged(); };
#else
            UnityEditor.EditorApplication.hierarchyWindowChanged -= delegate { hWindowChanged(); };
#endif
        }
        if (Application.isPlaying || !Application.isEditor){ return; }
		if(Application.isEditor && UnityEditor.EditorApplication.isPlaying){ return; }
		if(Application.isEditor && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode){ return; }
		
		int tCount = 0;
		if(GSDSpline != null && GSDSpline.mNodes != null){
			tCount = GSDSpline.GetNodeCountNonNull();	
		}
		if(tCount != MostRecentNodeCount){
			bUpdateSpline = true;
		}
	}
	
	void RoadUpdateProgressBar(){
		if(Editor_bIsConstructing){
			EditorUtility.DisplayProgressBar(
				"GSD Road Update",
				EditorTitleString,
				((float)EditorProgress/100f));
		}else if(bEditorProgressBar){
			bEditorProgressBar = false;
			EditorUtility.ClearProgressBar();	
		}
	}	
	
	public void UpdateRoad(RoadUpdateTypeEnum tUpdateType = RoadUpdateTypeEnum.Full){
        if (!GSDRS.opt_bAllowRoadUpdates) {
            GSDSpline.Setup();
            Editor_bIsConstructing = false;
            return;
        }

		if(Editor_bIsConstructing){
			return;	
		}

		SetupUniqueIdentifier();



		if(bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("UpdateRoadPrelim"); }
		opt_RoadDefinition = Mathf.Clamp(opt_RoadDefinition,1f,50f);
		opt_LaneWidth = Mathf.Clamp(opt_LaneWidth,0.2f,500f);
		EditorConstructionStartTime = Time.realtimeSinceStartup;
		EditorTitleString = "Updating " + transform.name + "...";
		System.GC.Collect();
		
		if(opt_SaveTerrainHistoryOnDisk){
			ConstructRoad_LoadTerrainHistory();
		}
		
		CheckMats();

		EditorUtility.ClearProgressBar();
		
		bProfiling = true;
		if(GSDRS.opt_bMultithreading){ bProfiling = false; }
		
        //Set all terrains to height 0:
        GSD.Roads.GSDTerraforming.CheckAllTerrainsHeight0();

		EditorProgress = 20;
		bEditorProgressBar = true;
		if(Editor_bIsConstructing){
			if(TerrainCalcsJob != null){ TerrainCalcsJob.Abort(); TerrainCalcsJob = null; }
			if(RoadCalcsJob1 != null){ RoadCalcsJob1.Abort(); RoadCalcsJob1 = null; }
			if(RoadCalcsJob2 != null){ RoadCalcsJob2.Abort(); RoadCalcsJob2 = null; }
			Editor_bIsConstructing = false;
		}  
	
//		if(Application.isPlaying || !Application.isEditor){ return; }
//		if(Application.isEditor && UnityEditor.EditorApplication.isPlaying){ return; }
//		if(Application.isEditor && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode){ return; }

		//In here for intersection patching purposes:
		int mCount = GSDSpline.GetNodeCount();
		GSDSplineN tNode = null;
		GSDSplineN tNode1 = null;
		GSDSplineN tNode2 = null;
		if(GSDSpline.CheckInvalidNodeCount()){
			GSDSpline.Setup();	
			mCount = GSDSpline.GetNodeCount();
		}
		if(mCount > 1){
			for(int i=0;i<mCount;i++){
//				try{
					tNode = GSDSpline.mNodes[i];
//				}catch{
//					Editor_bIsConstructing = false;
//					EditorUpdateMe = true;
//					return;	
//				}
				
				//If node is intersection with an invalid GSDRI, mark it at non-intersection. Just-in-case.
				if(tNode.bIsIntersection && tNode.GSDRI == null){
					tNode.bIsIntersection = false;
					tNode.id_intersection_othernode = -1;
					tNode.Intersection_OtherNode = null;
				}
				//If node is intersection, re-setup:
				if(tNode.bIsIntersection && tNode.GSDRI != null){
					tNode1 = tNode.GSDRI.Node1;
					tNode2 = tNode.GSDRI.Node2;
					tNode.GSDRI.Setup(tNode1,tNode2);
					tNode.GSDRI.DeleteRelevantChildren(tNode, tNode.GSDSpline.tRoad.transform.name);
					//If primary node on intersection, do more re-setup:
					if(tNode.GSDRI.Node1 == tNode){
						tNode.GSDRI.Lanes = opt_Lanes;
						tNode.GSDRI.name = tNode.GSDRI.transform.name;
					}
					//Setup construction objects:
					tNode.GSDRI.Node1.iConstruction = new GSD.Roads.GSDIntersections.iConstructionMaker();
					tNode.GSDRI.Node2.iConstruction = new GSD.Roads.GSDIntersections.iConstructionMaker();
				}

				//Store materials and physical materials for road and or shoulder cuts on each node, if necessary:
				tNode.StoreCuts();
			}
		}
		name = transform.name;
		

		
		GSDSpline.RoadWidth = RoadWidth();
//		if(bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("SplineSetup"); }
		GSDSpline.Setup();
//		if(bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
		mCount = GSDSpline.GetNodeCount();

		if(GSDSpline == null || GSDSpline.mNodes == null){
			MostRecentNodeCount = 0;
		}else{
			MostRecentNodeCount = GSDSpline.GetNodeCount();	
		}
		
		if(opt_UseDefaultMaterials){
			SetDefaultMats();	
		}
		
		if(opt_UseDefaultMaterials){
			if(DetectInvalidDefaultMatsForUndo()){
				SetAllCutsToCurrentMaterials();	
			}
		}
		
		//Hiding in hierarchy:
		for(int i=0;i<mCount;i++){
			tNode = GSDSpline.mNodes[i];
			if(tNode != null){
				if(tNode.bIsIntersection || tNode.bSpecialEndNode){
					tNode.ToggleHideFlags(true);
				}else{
					tNode.ToggleHideFlags(false);
				}
			}
		}
		
		int cCount = transform.childCount;
		GameObject tMainMeshes = null;
		List<GameObject> tObjs = new List<GameObject>();
		for(int i=0;i<cCount;i++){
			if(transform.GetChild(i).transform.name.ToLower().Contains("mainmeshes")){
				tMainMeshes = transform.GetChild(i).transform.gameObject;
				tObjs.Add(tMainMeshes);
			}
		}
		for(int i=(tObjs.Count-1);i>=0;i--){
			tMainMeshes = tObjs[i];
			Object.DestroyImmediate(tMainMeshes);
		}
		
		if(mCount < 2){
			//Delete old objs and return:
			if(MainMeshes != null){ Object.DestroyImmediate(MainMeshes); }
			if(MeshRoad != null){ Object.DestroyImmediate(MeshRoad); }
			if(MeshShoR != null){ Object.DestroyImmediate(MeshShoR); }
			if(MeshShoL != null){ Object.DestroyImmediate(MeshShoL); }
			if(MeshiLanes != null){ Object.DestroyImmediate(MeshiLanes); }
			if(MeshiLanes0 != null){ Object.DestroyImmediate(MeshiLanes0); }
			if(MeshiLanes1 != null){ Object.DestroyImmediate(MeshiLanes1); }
			if(MeshiLanes2 != null){ Object.DestroyImmediate(MeshiLanes2); }
			if(MeshiLanes3 != null){ Object.DestroyImmediate(MeshiLanes3); }
			if(MeshiMainPlates != null){ Object.DestroyImmediate(MeshiMainPlates); }
			if(MeshiMarkerPlates != null){ Object.DestroyImmediate(MeshiMarkerPlates); }
			if(bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			return;
		}
		
		GSDSpline.HeightHistory = new List<KeyValuePair<float, float>>();
		if(GSDRS == null){ GSDRS = transform.parent.GetComponent<GSDRoadSystem>(); } //Compatibility update.
		
		if(GSDRS.opt_bMultithreading){
			Editor_bIsConstructing = true;
		}else{
			Editor_bIsConstructing = false;
		}
		Editor_bConstructionID = 0;
		
		
		
		//Check if road takes place on only 1 terrain:
		Terrain tTerrain = GSD.Roads.GSDRoadUtil.GetTerrain(GSDSpline.mNodes[0].pos);
		bool bSameTerrain = true;
		for(int i=1;i<mCount;i++){
			if(tTerrain != GSD.Roads.GSDRoadUtil.GetTerrain(GSDSpline.mNodes[0].pos)){
				bSameTerrain = false;
				break;
			}
		}

		RCS = new RoadConstructorBufferMaker(this, tUpdateType);

		if(bSameTerrain){
			RCS.tTerrain = tTerrain;	
		}else{
			RCS.tTerrain = null;
		}
		tTerrain = null;
		
		if(bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
		
		if(GSDRS.opt_bMultithreading){
			if(RCS.bTerrainOn || TerrainHistory == null){
				GSDTerraforming.ProcessRoad_Terrain_Hook1(GSDSpline,this);
			}else{
				ConstructRoad2();
			}
		}else{
			UpdateRoad_NoMultiThreading();
		}
	}
	
            #region "Terrain history"
	public void ConstructRoad_StoreTerrainHistory(bool bDiskOnly = false){
		if(!bDiskOnly){
        	GSDRoad tRoad = this;
        	GSDRoadUtil.ConstructRoad_StoreTerrainHistory(ref tRoad);
		}

		if(opt_SaveTerrainHistoryOnDisk && TerrainHistory != null && TerrainHistory.Count > 0){
			if(bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("TerrainHistory_Save"); }
			GSDGeneralEditor.TerrainHistory_Save(TerrainHistory,this);
			if(bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			TerrainHistory.Clear();
			TerrainHistory = null;
		}else{
			if(TerrainHistory != null && TerrainHistory.Count > 0){
				int tSize = 0;
				for(int i=0;i<TerrainHistory.Count;i++){
					tSize += TerrainHistory[i].GetSize();
				}
				TerrainHistoryByteSize = (tSize*0.001f).ToString("n0") + " kb";	
			}else{
				TerrainHistoryByteSize = "0 bytes";	
			}
		}
	}
	public void ConstructRoad_ResetTerrainHistory(){
        GSDRoad tRoad = this;
		if(opt_SaveTerrainHistoryOnDisk && TerrainHistory != null){
			GSDGeneralEditor.TerrainHistory_Delete(this);
		}else{
			GSDRoadUtil.ConstructRoad_ResetTerrainHistory(ref tRoad);
		}
	}
	public void ConstructRoad_LoadTerrainHistory(bool bForce = false){
		if(opt_SaveTerrainHistoryOnDisk || bForce){
			if(TerrainHistory != null){
				TerrainHistory.Clear();
				TerrainHistory = null;
			}
			TerrainHistory = GSDGeneralEditor.TerrainHistory_Load(this);
		}
		if(bForce){
			GSDGeneralEditor.TerrainHistory_Delete(this);	
		}
	}
            #endregion
	
            #region "Construction process"
            #region "No multithread"
	private void UpdateRoad_NoMultiThreading(){
		if(opt_HeightModEnabled || opt_DetailModEnabled || opt_TreeModEnabled){
			if(bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("RoadCon_Terrain"); }
			if(RCS.bTerrainOn || TerrainHistory == null){
				GSDTerraforming.ProcessRoad_Terrain_Hook1(GSDSpline,this,false);
				GSDTerraforming.ProcessRoad_Terrain_Hook2(GSDSpline,ref EditorTTDList);
				ConstructRoad_StoreTerrainHistory();//Store history.
				int EditorTTDListCount = EditorTTDList.Count;
				for(int i=0;i<EditorTTDListCount;i++){
					EditorTTDList[i] = null;	
				}
				EditorTTDList = null;
				System.GC.Collect();
			}
			if(bProfiling){
                UnityEngine.Profiling.Profiler.EndSample();
			}
		}
		
		EditorProgress = 50;
		GSDRoad tRoad = this;
		if(bProfiling){
            UnityEngine.Profiling.Profiler.BeginSample("RoadCon_RoadPrelim");
		}
	
		EditorProgress = 80;	
		GSD.Threaded.GSDRoadCreationT.RoadJob_Prelim(ref tRoad);
		if(bProfiling){
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("RoadCon_Road1");
		}
		EditorProgress = 90;
		GSD.Threaded.RoadCalcs1_static.RunMe(ref RCS);
		if(bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
		if(bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("MeshSetup1"); }
		EditorProgress = 92;
		RCS.MeshSetup1();
		if(bProfiling){
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("RoadCon_Road2");
		}
		EditorProgress = 94;
		GSD.Threaded.RoadCalcs2_static.RunMe(ref RCS);
		if(bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
		if(bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("MeshSetup2"); }
		EditorProgress = 96;
		RCS.MeshSetup2();
		if(bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
		Construction_Cleanup();
	}
            #endregion
	
	private void ConstructRoad2(){
		EditorProgress = 40;
		if(RCS.bTerrainOn){
			//Store history:
			GSDTerraforming.ProcessRoad_Terrain_Hook2(GSDSpline,ref EditorTTDList);
			ConstructRoad_StoreTerrainHistory();
			int EditorTTDListCount = EditorTTDList.Count;
			for(int i=0;i<EditorTTDListCount;i++){
				EditorTTDList[i] = null;	
			}
			EditorTTDList = null;
			System.GC.Collect();
		} EditorProgress = 60;
		
		if(TerrainCalcsJob != null){ TerrainCalcsJob.Abort(); TerrainCalcsJob = null; }
		GSDRoad tRoad = this;
		EditorProgress = 72;
		RoadCalcsJob1 = new GSD.Threaded.RoadCalcs1();
		RoadCalcsJob1.Setup(ref RCS, ref tRoad);
		RoadCalcsJob1.Start();
	}
	
	private void ConstructRoad3(){
		EditorProgress = 84;
		RCS.MeshSetup1();
		EditorProgress = 96;
		if(RoadCalcsJob1 != null){ RoadCalcsJob1.Abort(); RoadCalcsJob1 = null; }
		RoadCalcsJob2 = new GSD.Threaded.RoadCalcs2();
		RoadCalcsJob2.Setup(ref RCS);
		RoadCalcsJob2.Start();
		EditorProgress = 98;
	}
	
	private void ConstructRoad4(){
		RCS.MeshSetup2();
		Construction_Cleanup();
	}
            #endregion
	
	private void Construction_Cleanup(){
        FixZ();

		if(TerrainCalcsJob != null){ TerrainCalcsJob.Abort(); TerrainCalcsJob = null; }
		if(RoadCalcsJob1 != null){ RoadCalcsJob1.Abort(); RoadCalcsJob1 = null; }
		if(RoadCalcsJob2 != null){ RoadCalcsJob2.Abort(); RoadCalcsJob2 = null; }
		Editor_bIsConstructing = false;
		int mCount = GSDSpline.GetNodeCount();
		GSDSplineN tNode;
		for(int i=0;i<mCount;i++){
			tNode = GSDSpline.mNodes[i];
			if(tNode.bIsIntersection){
				if(tNode.iConstruction != null){
					tNode.iConstruction.Nullify();
					tNode.iConstruction = null;
				}
			}
			tNode.SetupSplinationLimits();
			tNode.SetupEdgeObjects(false);
			tNode.SetupSplinatedMeshes(false);
		}
		if(GSDSpline.HeightHistory != null){ GSDSpline.HeightHistory.Clear(); GSDSpline.HeightHistory = null; }
		if(RCS != null){ 
			RCS.Nullify(); 
			RCS = null; 
		}
		
		if(GSDRS.opt_bSaveMeshes){
			UnityEditor.AssetDatabase.SaveAssets();
		}
		bEditorProgressBar = false;
		EditorUtility.ClearProgressBar();
		//Make sure terrain history out of memory if necessary (redudant but keep):
		if(opt_SaveTerrainHistoryOnDisk && TerrainHistory != null){
			TerrainHistory.Clear();
			TerrainHistory = null;
		}
		
		//Collect:
		bTriggerGC = true;

        if (tRoadMaterialDropdownOLD != opt_tRoadMaterialDropdown) {
            tRoadMaterialDropdownOLD = opt_tRoadMaterialDropdown;
            SetAllCutsToCurrentMaterials();
        }

		if(PiggyBacks != null && PiggyBacks.Length > 0){
			for(int i=0;i<PiggyBacks.Length;i++){
				if(PiggyBacks[i] == null){	
					PiggyBacks = null;
					break;
				}
			}
				
			if(PiggyBacks != null){
				GSDSplineC tPiggy = PiggyBacks[0];
				GSDSplineC[] NewPiggys = null;
				
				PiggyBacks[0] = null;
				if(PiggyBacks.Length > 1){
					NewPiggys = new GSDSplineC[PiggyBacks.Length-1];	
					for(int i=1;i<PiggyBacks.Length;i++){
						NewPiggys[i-1] = PiggyBacks[i];
					}
				}
				
				if(NewPiggys != null){
					tPiggy.tRoad.PiggyBacks = NewPiggys;
				}
				NewPiggys = null;
				tPiggy.Setup_Trigger();
			}
		}
	}
	
	public List<GSDTerraforming.TempTerrainData> EditorTTDList;
	public void EditorTerrainCalcs(ref List<GSDTerraforming.TempTerrainData> tList){
		EditorTTDList = tList;
	}
#endregion
	
            #region "Gizmos"
	public bool Editor_bIsConstructing = false;
	public int Editor_bConstructionID = 0;
	public bool Editor_bSelected = false;
	public bool Editor_MouseTerrainHit = false;
	public Vector3 Editor_MousePos = new Vector3(0f,0f,0f);
	public readonly Color Color_NodeDefaultColor = new Color(0f,1f,1f,0.75f);
	public readonly Color Color_NodeConnColor = new Color(0f,1f,0f,0.75f);
	public readonly Color Color_NodeInter = new Color(0f,1f,0f,0.75f);
	void OnDrawGizmosSelected(){
		if(Editor_MouseTerrainHit){
			Gizmos.color = Color.red;
			Gizmos.DrawCube(Editor_MousePos, new Vector3(10f,4f,10f));
		}
	}
            #endregion
	
	public float RoadWidth(){
		return (opt_LaneWidth * (float)opt_Lanes);
	}

	public float EditorCameraTimer = 0f;
	float EditorTestTimer = 0f;
	bool bEditorTestTimer = true;
	void Update(){
		if(Application.isEditor && bEditorCameraMoving){
			EditorCameraTimer+=Time.deltaTime;
			if(EditorCameraTimer > EditorCameraTimeUpdateInterval){
				EditorCameraTimer = 0f;
				DoEditorCameraLoop();
			}
		}
		
		if(bEditorTestTimer){
			if(transform.name == "Road1"){
				EditorTestTimer += Time.deltaTime;
				if(EditorTestTimer > 2f){
//					UpdateRoad(RoadUpdateTypeEnum.Full);
	//				akjsdfkajlgffdghfsdghsdf();
					bEditorTestTimer = false;
				}
			}else{
				bEditorTestTimer = false;
			}
		}
	}
	
	
	
		
	static void akjsdfkajlgffdghfsdghsdf(){
		int LoopMax = 1000;
		DoShort(LoopMax);
		DoInt(LoopMax);
		DoLong(LoopMax);
	}
	
	
	static void DoShort(int LoopMax){
		ushort[] tSubject = new ushort[25000];
//		int tInt = 0;
		for(int i=0;i<LoopMax;i++){
			for(int j=0;j<25000;j++){
				tSubject[j] = (ushort)(j+1);
//				int xTemp = (int)tSubject[j];
			}
		}
	}
	
	static void DoInt(int LoopMax){
		int[] tSubject = new int[25000];
//		int tInt = 0;
		for(int i=0;i<LoopMax;i++){
			for(int j=0;j<25000;j++){
				tSubject[j] = j+1;
//				int xTemp = tSubject[j];
			}
		}
	}
	
	static void DoLong(int LoopMax){
		long[] tSubject = new long[25000];
//		int tInt = 0;
		for(int i=0;i<LoopMax;i++){
			for(int j=0;j<25000;j++){
				tSubject[j] = (long)(j+1);
//				int xTemp = (int)tSubject[j];
			}
		}
	}
	
            #region "Default materials retrieval"
	public bool DetectInvalidDefaultMatsForUndo(){
		string tNameLower = "";
		int tCounter = 0;
		if(!MeshRoad){ return false; }
		
		MeshRenderer[] MRs = MeshRoad.GetComponentsInChildren<MeshRenderer>();
		Material tMat2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble.mat");
		Material tMat4 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble-4L.mat");
		Material tMat6 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble-6L.mat");
		foreach(MeshRenderer MR in MRs){
			tNameLower = MR.transform.name.ToLower();
			if(tNameLower.Contains("marker")){
				if(opt_Lanes == 2){
					if(MR.sharedMaterials[0] == tMat4){
						tCounter+=1;
					}else if(MR.sharedMaterials[0] == tMat6){
						tCounter+=1;
					}
				}else if(opt_Lanes == 4){
					if(MR.sharedMaterials[0] == tMat2){
						tCounter+=1;
					}else if(MR.sharedMaterials[0] == tMat6){
						tCounter+=1;
					}
				}else if(opt_Lanes == 6){
					if(MR.sharedMaterials[0] == tMat2){
						tCounter+=1;
					}else if(MR.sharedMaterials[0] == tMat4){
						tCounter+=1;
					}
				}
			}
			if(tCounter > 1){
				return true;	
			}
		}
		return false;
	}
	
	public void SetAllCutsToCurrentMaterials(){
		string tNameLower = "";
		if(!MeshRoad){ return; }
		
		MeshRenderer[] MRs = MeshRoad.GetComponentsInChildren<MeshRenderer>();
		Material[] tMats_World = GetMaterials_RoadWorld();
		Material[] tMats_Marker = GetMaterials_RoadMarker();
		foreach(MeshRenderer MR in MRs){
			tNameLower = MR.transform.name.ToLower();
			if(tNameLower.Contains("marker")){
				if(tMats_Marker != null){
					MR.sharedMaterials = tMats_Marker;
				}
			}else if(tNameLower.Contains("cut")){
				if(tMats_World != null){
					MR.sharedMaterials = tMats_World;
				}
			}
		}

		if(opt_bShouldersEnabled && MeshShoL != null){
			MRs = MeshShoL.GetComponentsInChildren<MeshRenderer>();
			tMats_World = GetMaterials_ShoulderWorld();
			tMats_Marker = GetMaterials_ShoulderMarker();
			foreach(MeshRenderer MR in MRs){
				tNameLower = MR.transform.name.ToLower();
				if(tNameLower.Contains("marker")){
					if(tMats_Marker != null){
						MR.sharedMaterials = tMats_Marker;
					}
				}else if(tNameLower.Contains("cut")){
					if(tMats_World != null){
						MR.sharedMaterials = tMats_World;
					}
				}
			}
		}

		if(opt_bShouldersEnabled && MeshShoR != null){
			MRs = MeshShoR.GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer MR in MRs){
				tNameLower = MR.transform.name.ToLower();
				if(tNameLower.Contains("marker")){
					if(tMats_Marker != null){
						MR.sharedMaterials = tMats_Marker;
					}
				}else if(tNameLower.Contains("cut")){
					if(tMats_World != null){
						MR.sharedMaterials = tMats_World;
					}
				}
			}
		}
	}

	public Material[] GetMaterials_RoadWorld(){
		int mCounter = 0;
		if(RoadMaterial1 != null){
			mCounter+=1;
			if(RoadMaterial2 != null){
				mCounter+=1;
				if(RoadMaterial3 != null){
					mCounter+=1;
					if(RoadMaterial4 != null){
						mCounter+=1;
					}
				}
			}
		}
		if(mCounter > 0){
			Material[] tMats = new Material[mCounter];
			if(RoadMaterial1 != null){
				tMats[0] = RoadMaterial1;
				if(RoadMaterial2 != null){
					tMats[1] = RoadMaterial2;	
					if(RoadMaterial3 != null){
						tMats[2] = RoadMaterial3;	
						if(RoadMaterial4 != null){
							tMats[3] = RoadMaterial4;	
						}
					}
				}
			}
			return tMats;
		}else{
			return null;	
		}
	}
	
	public Material[] GetMaterials_RoadMarker(){
		int mCounter = 0;
		if(RoadMaterialMarker1 != null){
			mCounter+=1;
			if(RoadMaterialMarker2 != null){
				mCounter+=1;
				if(RoadMaterialMarker3 != null){
					mCounter+=1;
					if(RoadMaterialMarker4 != null){
						mCounter+=1;
					}
				}
			}
		}
		if(mCounter > 0){
			Material[] tMats = new Material[mCounter];
			if(RoadMaterialMarker1 != null){
				tMats[0] = RoadMaterialMarker1;
				if(RoadMaterialMarker2 != null){
					tMats[1] = RoadMaterialMarker2;	
					if(RoadMaterialMarker3 != null){
						tMats[2] = RoadMaterialMarker3;	
						if(RoadMaterialMarker4 != null){
							tMats[3] = RoadMaterialMarker4;	
						}
					}
				}
			}
			return tMats;
		}else{
			return null;	
		}
	}
	
	public Material[] GetMaterials_ShoulderWorld(){
		if(!opt_bShouldersEnabled){
			return null;
		}

		int mCounter = 0;
		if(ShoulderMaterial1 != null){
			mCounter+=1;
			if(ShoulderMaterial2 != null){
				mCounter+=1;
				if(ShoulderMaterial3 != null){
					mCounter+=1;
					if(ShoulderMaterial4 != null){
						mCounter+=1;
					}
				}
			}
		}
		if(mCounter > 0){
			Material[] tMats = new Material[mCounter];
			if(ShoulderMaterial1 != null){
				tMats[0] = ShoulderMaterial1;
				if(ShoulderMaterial2 != null){
					tMats[1] = ShoulderMaterial2;	
					if(ShoulderMaterial3 != null){
						tMats[2] = ShoulderMaterial3;	
						if(ShoulderMaterial4 != null){
							tMats[3] = ShoulderMaterial4;	
						}
					}
				}
			}
			return tMats;
		}else{
			return null;	
		}
	}
	
	public Material[] GetMaterials_ShoulderMarker(){
		if(!opt_bShouldersEnabled){
			return null;
		}

		int mCounter = 0;
		if(ShoulderMaterialMarker1 != null){
			mCounter+=1;
			if(ShoulderMaterialMarker2 != null){
				mCounter+=1;
				if(ShoulderMaterialMarker3 != null){
					mCounter+=1;
					if(ShoulderMaterialMarker4 != null){
						mCounter+=1;
					}
				}
			}
		}
		if(mCounter > 0){
			Material[] tMats = new Material[mCounter];
			if(ShoulderMaterialMarker1 != null){
				tMats[0] = ShoulderMaterialMarker1;
				if(ShoulderMaterialMarker2 != null){
					tMats[1] = ShoulderMaterialMarker2;	
					if(ShoulderMaterialMarker3 != null){
						tMats[2] = ShoulderMaterialMarker3;	
						if(ShoulderMaterialMarker4 != null){
							tMats[3] = ShoulderMaterialMarker4;	
						}
					}
				}
			}
			return tMats;
		}else{
			return null;	
		}
	}
            #endregion
	
            #region "Materials"
	void CheckMats(){
		if(!opt_UseDefaultMaterials){
			return;
		}
		
		if(!RoadMaterial1){
			RoadMaterial1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDRoad1.mat");
			RoadMaterial2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadDetailOverlay1.mat");
		}
		if(!RoadMaterialMarker1){
			if(opt_Lanes == 2){
				RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble.mat");
			}else if(opt_Lanes == 4){
				RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble-4L.mat");
			}else if(opt_Lanes == 6){
				RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble-6L.mat");
			}else{
				RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble.mat");
			}
			
			if(opt_Lanes == 2){
				RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks.mat");
			}else if(opt_Lanes == 4){
				RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks-4L.mat");
			}else if(opt_Lanes == 6){
				RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks-6L.mat");
			}else{
				RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks.mat");
			}
		}
		if(opt_bShouldersEnabled && !ShoulderMaterial1){ 
			ShoulderMaterial1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDShoulder1.mat");
			ShoulderMaterial2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadDetailOverlay1.mat");
		}
		
		if(opt_bShouldersEnabled && !RoadPhysicMaterial){
			RoadPhysicMaterial = GSD.Roads.GSDRoadUtilityEditor.GivePhysicsMaterial("Assets/RoadArchitect/Physics/GSDPavement.physicMaterial");
		}
		if(opt_bShouldersEnabled && !ShoulderPhysicMaterial){
			ShoulderPhysicMaterial = GSD.Roads.GSDRoadUtilityEditor.GivePhysicsMaterial("Assets/RoadArchitect/Physics/GSDDirt.physicMaterial");
		}	
	}
	
	public void SetDefaultMats(){
        if (opt_tRoadMaterialDropdown == RoadMaterialDropdownEnum.Asphalt) {
		    RoadMaterial1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDRoad1.mat");
		    RoadMaterial2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadDetailOverlay1.mat");
	
		    if(opt_Lanes == 2){
			    RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble.mat");
		    }else if(opt_Lanes == 4){
			    RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble-4L.mat");
		    }else if(opt_Lanes == 6){
			    RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble-6L.mat");
		    }else{
			    RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDWhiteYellowDouble.mat");
		    }
		
		    if(opt_Lanes == 2){
			    RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks.mat");
		    }else if(opt_Lanes == 4){
			    RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks-4L.mat");
		    }else if(opt_Lanes == 6){
			    RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks-6L.mat");
		    }else{
			    RoadMaterialMarker2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDTireMarks.mat");
		    }
	
		    ShoulderMaterial1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDShoulder1.mat");
		    ShoulderMaterial2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadDetailOverlay1.mat");

		    RoadPhysicMaterial = GSD.Roads.GSDRoadUtilityEditor.GivePhysicsMaterial("Assets/RoadArchitect/Physics/GSDPavement.physicMaterial");
		    ShoulderPhysicMaterial = GSD.Roads.GSDRoadUtilityEditor.GivePhysicsMaterial("Assets/RoadArchitect/Physics/GSDDirt.physicMaterial");
        } else if (opt_tRoadMaterialDropdown == RoadMaterialDropdownEnum.Dirt) {
            RoadMaterial1 = null;
            RoadMaterial2 = null;
            RoadMaterial3 = null;
            RoadMaterial4 = null;
            RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDDirtRoad.mat");
            RoadMaterialMarker2 = null;
            RoadMaterialMarker3 = null;
            RoadMaterialMarker4 = null;
        } else if (opt_tRoadMaterialDropdown == RoadMaterialDropdownEnum.Brick) {
            RoadMaterial1 = null;
            RoadMaterial2 = null;
            RoadMaterial3 = null;
            RoadMaterial4 = null;
            RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDBrickRoad.mat");
            RoadMaterialMarker2 = null;
            RoadMaterialMarker3 = null;
            RoadMaterialMarker4 = null;
        } else if (opt_tRoadMaterialDropdown == RoadMaterialDropdownEnum.Cobblestone) {
            RoadMaterial1 = null;
            RoadMaterial2 = null;
            RoadMaterial3 = null;
            RoadMaterial4 = null;
            RoadMaterialMarker1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDCobblestoneRoad.mat");
            RoadMaterialMarker2 = null;
            RoadMaterialMarker3 = null;
            RoadMaterialMarker4 = null;
        }
		
		int mCount = GSDSpline.GetNodeCount();
		for(int i=0;i<mCount;i++){
			if(GSDSpline.mNodes[i] && GSDSpline.mNodes[i].bIsIntersection && GSDSpline.mNodes[i].GSDRI != null && GSDSpline.mNodes[i].GSDRI.bUseDefaultMaterials){
				GSDSpline.mNodes[i].GSDRI.ResetMaterials_All();
			}
		}
	}
            #endregion
	
	public void Wireframes_Toggle(){
		MeshRenderer[] tMRs = transform.GetComponentsInChildren<MeshRenderer>();
		Wireframes_Toggle_Help(ref tMRs);
		
		if(GSDSpline != null){
			tMRs = GSDSpline.transform.GetComponentsInChildren<MeshRenderer>();
			Wireframes_Toggle_Help(ref tMRs);	
		}
	}
	void Wireframes_Toggle_Help(ref MeshRenderer[] tMRs){
		int tCount = tMRs.Length;
		for(int i=0;i<tCount;i++){
			//EditorUtility.SetSelectedWireframeHidden(tMRs[i], !opt_GizmosEnabled);
            EditorUtility.SetSelectedRenderState(tMRs[i],opt_GizmosEnabled ? EditorSelectedRenderState.Wireframe : EditorSelectedRenderState.Hidden);
		}
	}
	
#endif

            void Start(){
		#if UNITY_EDITOR
			if(Application.isPlaying){
				CleanRunTime();
			}
		#else
			this.enabled = false;
		#endif
	}
	
	#if UNITY_EDITOR
	//For compliance on submission rules:
	public void UpdateGizmoOptions(){
		if(GSDSpline == null){ return; }
		GSDSplineN tNode = null;
		
		int mCount = GSDSpline.GetNodeCount();
		for(int i=0;i<mCount;i++){
			tNode = GSDSpline.mNodes[i];
			if(tNode != null){
				tNode.opt_GizmosEnabled = opt_GizmosEnabled;	
			}
		}
	}
	
	public void SetupUniqueIdentifier(){
		if(UID == null || UID.Length < 4){
			UID = System.Guid.NewGuid().ToString();}
	}

    public void DuplicateRoad() {
        GameObject tRoadObj = GSDRS.AddRoad();
        UnityEditor.Undo.RegisterCreatedObjectUndo(tRoadObj, "Duplicate");

        GSDRoad xRoad = tRoadObj.GetComponent<GSDRoad>();
        if (xRoad == null) { return; }

        //Road editor options: 
        xRoad.opt_LaneWidth = opt_LaneWidth;					//Done.
        xRoad.opt_bShouldersEnabled = opt_bShouldersEnabled;		//Disabled for now. Comprimises integrity of roads.
        xRoad.opt_ShoulderWidth = opt_ShoulderWidth;				//Done.
        xRoad.opt_Lanes = opt_Lanes;						//Done.
        xRoad.opt_RoadDefinition = opt_RoadDefinition;			//Done.
        xRoad.opt_RoadCornerDefinition = opt_RoadCornerDefinition;   //Disable for now. No point.
        xRoad.opt_bRoadCuts = opt_bRoadCuts;
        xRoad.opt_bShoulderCuts = opt_bShoulderCuts;
        xRoad.opt_bDynamicCuts = opt_bDynamicCuts;
        xRoad.opt_bMaxGradeEnabled = opt_bMaxGradeEnabled;
        xRoad.opt_MaxGrade = opt_MaxGrade;
        xRoad.opt_UseDefaultMaterials = opt_UseDefaultMaterials;
        xRoad.opt_AutoUpdateInEditor = opt_AutoUpdateInEditor;

        xRoad.opt_TerrainSubtract_Match = opt_TerrainSubtract_Match;
        xRoad.opt_bGSDRoadRaise = opt_bGSDRoadRaise;

        xRoad.opt_MatchHeightsDistance = opt_MatchHeightsDistance;
        xRoad.opt_ClearDetailsDistance = opt_ClearDetailsDistance;
        xRoad.opt_ClearDetailsDistanceHeight = opt_ClearDetailsDistanceHeight;
        xRoad.opt_ClearTreesDistance = opt_ClearTreesDistance;
        xRoad.opt_ClearTreesDistanceHeight = opt_ClearTreesDistanceHeight;

        xRoad.opt_HeightModEnabled = opt_HeightModEnabled;
        xRoad.opt_DetailModEnabled = opt_DetailModEnabled;
        xRoad.opt_TreeModEnabled = opt_TreeModEnabled;

        xRoad.opt_SaveTerrainHistoryOnDisk = opt_SaveTerrainHistoryOnDisk;
        xRoad.opt_MagnitudeThreshold = opt_MagnitudeThreshold;
        xRoad.opt_GizmosEnabled = opt_GizmosEnabled;
        xRoad.opt_bMultithreading = opt_bMultithreading;
        xRoad.opt_bSaveMeshes = opt_bSaveMeshes;
        xRoad.opt_bUseMeshColliders = opt_bUseMeshColliders;

        xRoad.opt_tRoadMaterialDropdown = opt_tRoadMaterialDropdown;
        xRoad.tRoadMaterialDropdownOLD = tRoadMaterialDropdownOLD;
	
	    xRoad.RoadMaterial1 = RoadMaterial1;
	    xRoad.RoadMaterial2 = RoadMaterial2;
	    xRoad.RoadMaterial3 = RoadMaterial3;
	    xRoad.RoadMaterial4 = RoadMaterial4;
	    xRoad.RoadMaterialMarker1 = RoadMaterialMarker1;
	    xRoad.RoadMaterialMarker2 = RoadMaterialMarker2;
	    xRoad.RoadMaterialMarker3 = RoadMaterialMarker3;
	    xRoad.RoadMaterialMarker4 = RoadMaterialMarker4;
	    xRoad.ShoulderMaterial1 = ShoulderMaterial1;
	    xRoad.ShoulderMaterial2 = ShoulderMaterial2;
	    xRoad.ShoulderMaterial3 = ShoulderMaterial3;
	    xRoad.ShoulderMaterial4 = ShoulderMaterial4;
	    xRoad.ShoulderMaterialMarker1 = ShoulderMaterialMarker1;
	    xRoad.ShoulderMaterialMarker2 = ShoulderMaterialMarker2;
	    xRoad.ShoulderMaterialMarker3 = ShoulderMaterialMarker3;
	    xRoad.ShoulderMaterialMarker4 = ShoulderMaterialMarker4;
	
	    xRoad.RoadPhysicMaterial = RoadPhysicMaterial;
        xRoad.ShoulderPhysicMaterial = ShoulderPhysicMaterial;

        xRoad.GSDSpline.Setup_Trigger();

        Selection.activeGameObject = xRoad.transform.gameObject;
    }

    private void FixZ() {
        #if UNITY_ANDROID
        FixZ_Mobile();
        #elif  UNITY_IPHONE
        FixZ_Mobile();
        #elif UNITY_STANDALONE_WIN
        FixZ_Win();
        #elif UNITY_STANDALONE
        FixZ_Mobile();
        #elif UNITY_WEBPLAYER
        FixZ_Mobile();
        #else
        FixZ_Mobile();
        #endif
    }

    private void FixZ_Mobile() {
        //This road:
        Object[] tMarkerObjs = transform.GetComponentsInChildren<MeshRenderer>();
        Vector3 tVect = default(Vector3);
        foreach (MeshRenderer MR in tMarkerObjs) {
            if (MR.transform.name.Contains("Marker")) {
                tVect = new Vector3(0f, 0.02f, 0f);
                MR.transform.localPosition = tVect;
            } else if (MR.transform.name.Contains("SCut")) {
                tVect = MR.transform.position;
                tVect.y += 0.01f;
                MR.transform.position = tVect;
            } else if (MR.transform.name.Contains("RoadCut")) {
                tVect = MR.transform.position;
                tVect.y += 0.01f; 
                MR.transform.position = tVect;
            }
        }

        //Intersections (all):
        tMarkerObjs = GSDRS.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer MR in tMarkerObjs) {
            if (MR.transform.name.Contains("CenterMarkers")) {
                tVect = new Vector3(0f, 0.02f, 0f);
                MR.transform.localPosition = tVect;
            } else if (MR.transform.name.Contains("-Inter") && MR.transform.name.Contains("-Lane")) {
                tVect = new Vector3(0f, 0.02f, 0f);
                MR.transform.localPosition = tVect;
            } else if (MR.transform.name.Contains("-Inter") && MR.transform.name.Contains("-Stretch")) {
                tVect = new Vector3(0f, 0.02f, 0f);
                MR.transform.localPosition = tVect;
            } else if (MR.transform.name.Contains("-Inter") && MR.transform.name.Contains("-Tiled")) {
                tVect = new Vector3(0f, 0.01f, 0f);
                MR.transform.localPosition = tVect;
            }
        }
    }

    private void FixZ_Win() {
        //This road:
        Object[] tMarkerObjs = transform.GetComponentsInChildren<MeshRenderer>();
        Vector3 tVect = default(Vector3);
        foreach (MeshRenderer MR in tMarkerObjs) {
            if (MR.transform.name.Contains("Marker")) {
                tVect = new Vector3(0f, 0.01f, 0f);
                MR.transform.localPosition = tVect;
            }
        }

        //Intersections (all):
        tMarkerObjs = Object.FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer MR in tMarkerObjs) {
            if (MR.transform.name.Contains("-Inter") && MR.transform.name.Contains("-Lane")) {
                tVect = new Vector3(0f, 0.01f, 0f);
                MR.transform.localPosition = tVect;
            } else if (MR.transform.name.Contains("-Inter") && MR.transform.name.Contains("-Stretch")) {
                tVect = new Vector3(0f, 0.01f, 0f);
                MR.transform.localPosition = tVect;
            }
        }
    }
	#endif
}