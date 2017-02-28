#region "Imports"
using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using GSD;
using System.IO;
#endif
#endregion
namespace GSD.Roads.EdgeObjects{
	#if UNITY_EDITOR
	
	[System.Serializable]
	public class EdgeObjectMaker{
		public bool bNeedsUpdate = false;
		public string UID = "";
		public GSDSplineN tNode = null;
		public bool bIsGSD = false;
		public GameObject EdgeObject = null;
		public string EdgeObjectString = "";
		public bool bMaterialOverride = false;
		public Material EdgeMaterial1 = null;
		public Material EdgeMaterial2 = null;
		public string EdgeMaterial1String = null;
		public string EdgeMaterial2String = null;
		public bool bMatchTerrain = true;
		
		//Temp editor buffers:
		public bool bEdgeSignLabelInit = false;
		public bool bEdgeSignLabel = false;
		public string EdgeSignLabel = "";
		
		public bool bCombineMesh = false;
		public bool bCombineMeshCollider = false;
		public GameObject MasterObj = null;
		public List<Vector3> EdgeObjectLocations;
		public List<Vector3> EdgeObjectRotations;
		public List<GameObject> EdgeObjects;
		public GSD.Roads.SignPlacementSubTypeEnum SubType = GSD.Roads.SignPlacementSubTypeEnum.Right;
		public float MeterSep = 5f;
		public bool bToggle = false;
		public bool bIsBridge = false;
		
		//Horizontal offsets:
		public float HorizontalSep = 5f;
		public AnimationCurve HorizontalCurve;
		public float HorizCurve_tempchecker1 = 0f;
		public float HorizCurve_tempchecker2 = 0f;
		public float HorizCurve_tempchecker3 = 0f;
		public float HorizCurve_tempchecker4 = 0f;
		public float HorizCurve_tempchecker5 = 0f;
		public float HorizCurve_tempchecker6 = 0f;
		public float HorizCurve_tempchecker7 = 0f;
		public float HorizCurve_tempchecker8 = 0f;
		//Vertical offsets:
		public float VerticalRaise = 0f;
		public AnimationCurve VerticalCurve;
		public float VerticalCurve_tempchecker1 = 0f;
		public float VerticalCurve_tempchecker2 = 0f;
		public float VerticalCurve_tempchecker3 = 0f;
		public float VerticalCurve_tempchecker4 = 0f;
		public float VerticalCurve_tempchecker5 = 0f;
		public float VerticalCurve_tempchecker6 = 0f;
		public float VerticalCurve_tempchecker7 = 0f;
		public float VerticalCurve_tempchecker8 = 0f;
		
		public Vector3 CustomRotation = default(Vector3);
		public bool bOncomingRotation = true;
		public bool bStatic = true;
		
		public float StartTime = 0f;
		public float EndTime = 1f;
		public float SingleOnlyBridgePercent = 0f;
		public Vector3 StartPos = default(Vector3);
		public Vector3 EndPos = default(Vector3);
		public bool bSingle = false;
		public float SinglePosition;
		
		public bool bStartMatchRoadDefinition = false;
		public float StartMatchRoadDef = 0f;
		
		public string tName = "EdgeObject";
		public string ThumbString = "";
		public string Desc = "";
		public string DisplayName = "";
		
		public EdgeObjectEditorMaker EM;
		
		public EdgeObjectMaker Copy(){
			EdgeObjectMaker EOM = new EdgeObjectMaker();
			
			EOM.EdgeObjectString = EdgeObjectString;
			#if UNITY_EDITOR
			EOM.EdgeObject = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(EdgeObjectString,typeof(GameObject));
			#endif
			EOM.bIsGSD = bIsGSD;
			
			EOM.bCombineMesh = bCombineMesh;
			EOM.bCombineMeshCollider = bCombineMeshCollider;
			EOM.SubType = SubType;
			EOM.MeterSep = MeterSep;
			EOM.bToggle = bToggle;
			EOM.bMatchTerrain = bMatchTerrain;
			
			EOM.bMaterialOverride = bMaterialOverride;
			EOM.EdgeMaterial1 = EdgeMaterial1;
			EOM.EdgeMaterial2 = EdgeMaterial2;
			
			EOM.MasterObj = MasterObj;
			EOM.EdgeObjectLocations = EdgeObjectLocations;
			EOM.EdgeObjectRotations = EdgeObjectRotations;
			EOM.tNode = tNode;
			EOM.StartTime = StartTime;
			EOM.EndTime = EndTime;
			EOM.StartPos = StartPos;
			EOM.EndPos = EndPos;
			EOM.SingleOnlyBridgePercent = SingleOnlyBridgePercent;
			EOM.bIsBridge = bIsBridge;

			EOM.HorizontalSep = HorizontalSep;
			EOM.HorizontalCurve = new AnimationCurve();
			if(HorizontalCurve != null && HorizontalCurve.keys.Length > 0){
				for(int i=0;i<HorizontalCurve.keys.Length;i++){
					EOM.HorizontalCurve.AddKey(HorizontalCurve.keys[i]);
				}
			}
			
			EOM.VerticalRaise = VerticalRaise;
			EOM.VerticalCurve = new AnimationCurve();
			if(VerticalCurve != null && VerticalCurve.keys.Length > 0){
				for(int i=0;i<VerticalCurve.keys.Length;i++){
					EOM.VerticalCurve.AddKey(VerticalCurve.keys[i]);
				}
			}

			EOM.CustomRotation = CustomRotation;
			EOM.bOncomingRotation = bOncomingRotation;
			EOM.bStatic = bStatic;
			EOM.bSingle = bSingle;
			EOM.SinglePosition = SinglePosition;
			
			EOM.bStartMatchRoadDefinition = bStartMatchRoadDefinition;
			EOM.StartMatchRoadDef = StartMatchRoadDef;
			
			EOM.SetupUniqueIdentifier();
			
			EOM.tName = tName;
			EOM.ThumbString = ThumbString;
			EOM.Desc = Desc;
			EOM.DisplayName = DisplayName;
			
			return EOM;
		}
		
		public void UpdatePositions(){
			StartPos = tNode.GSDSpline.GetSplineValue(StartTime);
			EndPos = tNode.GSDSpline.GetSplineValue(EndTime);
		}
		
		#region "Library"
		public void SetupUniqueIdentifier(){
			if(UID == null || UID.Length < 4){
				UID = System.Guid.NewGuid().ToString();
			}
		}
		
		public void SaveToLibrary(string fName = "", bool bIsDefault = false){
			EdgeObjectLibraryMaker ELM = new EdgeObjectLibraryMaker();
			ELM.Setup(this);
			GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			string tPath = xPath + "EOM" + tName + ".gsd";
			if(fName.Length > 0){
				if(bIsDefault){
					tPath = xPath + "Q/EOM" + fName + ".gsd";
				}else{
					tPath = xPath + "EOM" + fName + ".gsd";	
				}
			}
			GSDRootUtil.CreateXML<EdgeObjectLibraryMaker>(ref tPath,ELM);
		}

		public void LoadFromLibrary(string xName, bool bIsQuickAdd = false){
			GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			string tPath = xPath + "EOM" + xName + ".gsd";
			if(bIsQuickAdd){
				tPath = xPath + "Q/EOM" + xName + ".gsd";
			}
			EdgeObjectLibraryMaker ELM = (EdgeObjectLibraryMaker)GSDRootUtil.LoadXML<EdgeObjectLibraryMaker>(ref tPath);
			ELM.LoadTo(this);
			bNeedsUpdate = true;
		}
		
		public void LoadFromLibraryWizard(string xName){
			GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			string tPath = xPath + "W/" + xName + ".gsd";
			EdgeObjectLibraryMaker ELM = (EdgeObjectLibraryMaker)GSDRootUtil.LoadXML<EdgeObjectLibraryMaker>(ref tPath);
			ELM.LoadTo(this);
			bNeedsUpdate = true;
		}
		
		
		public string ConvertToString(){
			EdgeObjectLibraryMaker ELM = new EdgeObjectLibraryMaker();
			ELM.Setup(this);
			return GSDRootUtil.GetString<EdgeObjectLibraryMaker>(ELM);
		}
		
		public void LoadFromLibraryBulk(ref EdgeObjectLibraryMaker ELM){
			ELM.LoadTo(this);
		}
		
		public static EdgeObjectLibraryMaker ELMFromData(string tData){
			try{
				EdgeObjectLibraryMaker ELM = (EdgeObjectLibraryMaker)GSDRootUtil.LoadData<EdgeObjectLibraryMaker>(ref tData);	
				return ELM;
			}catch{
				return null;	
			}
		}
		
		public static void GetLibraryFiles(out string[] tNames, out string[] tPaths, bool bIsDefault = false){
			#if UNITY_WEBPLAYER
			tNames = null;
			tPaths = null;
			return;
			#else
			
			tNames = null;
			tPaths = null;
			DirectoryInfo info;
			string xPath = GSDRootUtil.Dir_GetLibrary();
			if(bIsDefault){
				info = new DirectoryInfo(xPath + "Q/");
			}else{
				info = new DirectoryInfo(xPath);
			}

			FileInfo[] fileInfo = info.GetFiles();
			int tCount = 0;
			
			
			foreach(FileInfo tInfo in fileInfo){
				if(tInfo.Name.Contains("EOM") && tInfo.Extension.ToLower().Contains("gsd")){
					tCount+=1;
				}
			}
			
			tNames = new string[tCount];
			tPaths = new string[tCount];
			tCount=0;
			foreach(FileInfo tInfo in fileInfo){
				if(tInfo.Name.Contains("EOM") && tInfo.Extension.ToLower().Contains("gsd")){
					tNames[tCount] = tInfo.Name.Replace(".gsd","").Replace("EOM","");
					tPaths[tCount] = tInfo.FullName;
					tCount+=1;
				}
			}
			#endif
		}
		
		private void SaveMesh(Mesh tMesh, bool bIsCollider){
			#if UNITY_EDITOR
			if(!tNode.GSDSpline.tRoad.GSDRS.opt_bSaveMeshes){ return; }

			//string tSceneName = System.IO.Path.GetFileName(UnityEditor.EditorApplication.currentScene).ToLower().Replace(".unity","");
            string tSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;


            tSceneName = tSceneName.Replace("/","");
			tSceneName = tSceneName.Replace(".","");
			string tFolderName = "Assets/RoadArchitect/Mesh/Generated/CombinedEdgeObj/";
			string tRoadName = tNode.GSDSpline.tRoad.transform.name;
			string FinalName = tFolderName + tSceneName + "-" + tRoadName + "-" + tName + ".asset";
			if(bIsCollider){
				FinalName = tFolderName + tSceneName + "-" + tRoadName + "-" + tName + "-collider.asset";	
			}
			
			string xPath = Application.dataPath.Replace("/Assets","/" + tFolderName);
			if(!System.IO.Directory.Exists(xPath)){
				System.IO.Directory.CreateDirectory(xPath);
			}

			UnityEditor.AssetDatabase.CreateAsset(tMesh,FinalName);
			UnityEditor.AssetDatabase.SaveAssets();
			#endif
		}
		
		#region "Library object"
		[System.Serializable]
		public class EdgeObjectLibraryMaker{
			public string EdgeObjectString = "";
			public bool bCombineMesh = false;
			public bool bCombineMeshCollider = false;
			public List<Vector3> EdgeObjectLocations;
			public List<Vector3> EdgeObjectRotations;
			public GSD.Roads.SignPlacementSubTypeEnum SubType = GSD.Roads.SignPlacementSubTypeEnum.Right;
			public float MeterSep = 5f;
			public bool bToggle = false;
			public bool bIsBridge = false;
			public bool bIsGSD = false;
			
			public bool bMaterialOverride = false;
			public string EdgeMaterial1String = "";
			public string EdgeMaterial2String = "";
			
			//Horizontal offsets:
			public float HorizontalSep = 5f;
			public AnimationCurve HorizontalCurve;
			//Vertical offsets:
			public float VerticalRaise = 0f;
			public AnimationCurve VerticalCurve;
			
			public Vector3 CustomRotation = default(Vector3);
			public bool bOncomingRotation = true;
			public bool bStatic = true;
			public bool bMatchTerrain = true;
			
			public float StartTime = 0f;
			public float EndTime = 1f;
			public float SingleOnlyBridgePercent = 0f;
			public bool bSingle = false;
			public float SinglePosition;
			
			public bool bStartMatchRoadDefinition = false;
			public float StartMatchRoadDef = 0f;
			
			public string tName = "EdgeObject";
			public string ThumbString = "";
			public string Desc = "";
			public string DisplayName = "";
			
			public void Setup(EdgeObjectMaker EOM){
				EdgeObjectString = EOM.EdgeObjectString;
				bCombineMesh = EOM.bCombineMesh;
				bCombineMeshCollider = EOM.bCombineMeshCollider;
//				GSD.Roads.SignPlacementSubTypeEnum SubType = EOM.SubType;
				MeterSep = EOM.MeterSep;
				bToggle = EOM.bToggle;
				bIsGSD = EOM.bIsGSD;
				
				bMaterialOverride = EOM.bMaterialOverride;
				EdgeMaterial1String = EOM.EdgeMaterial1String;
				EdgeMaterial2String = EOM.EdgeMaterial2String;
				
				HorizontalSep = EOM.HorizontalSep;
				HorizontalCurve = EOM.HorizontalCurve;
				VerticalRaise = EOM.VerticalRaise;
				VerticalCurve = EOM.VerticalCurve;
				bMatchTerrain = EOM.bMatchTerrain;
				
				CustomRotation = EOM.CustomRotation;
				bOncomingRotation = EOM.bOncomingRotation;
				bStatic = EOM.bStatic;
				bSingle = EOM.bSingle;
				SinglePosition = EOM.SinglePosition;
				tName = EOM.tName;
				SingleOnlyBridgePercent = EOM.SingleOnlyBridgePercent;
				bStartMatchRoadDefinition = EOM.bStartMatchRoadDefinition;
				StartMatchRoadDef = EOM.StartMatchRoadDef;
				ThumbString = EOM.ThumbString;
				Desc = EOM.Desc;
				bIsBridge = EOM.bIsBridge;
				DisplayName = EOM.DisplayName;
			}
			
			public void LoadTo(EdgeObjectMaker EOM){
				EOM.EdgeObjectString = EdgeObjectString;
				#if UNITY_EDITOR
				EOM.EdgeObject = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(EdgeObjectString,typeof(GameObject));
				#endif
				EOM.bMaterialOverride = bMaterialOverride;
				#if UNITY_EDITOR
				if(EdgeMaterial1String.Length > 0){
					EOM.EdgeMaterial1 = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(EdgeMaterial1String,typeof(Material));
				}
				if(EdgeMaterial2String.Length > 0){
					EOM.EdgeMaterial2 = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(EdgeMaterial2String,typeof(Material));
				}
				#endif

				EOM.bCombineMesh = bCombineMesh;
				EOM.bCombineMeshCollider = bCombineMeshCollider;
				EOM.SubType = SubType;
				EOM.MeterSep = MeterSep;
				EOM.bToggle = bToggle;
				EOM.bIsGSD = bIsGSD;

				EOM.HorizontalSep = HorizontalSep;
				EOM.HorizontalCurve = HorizontalCurve;
				EOM.VerticalRaise = VerticalRaise;
				EOM.VerticalCurve = VerticalCurve;
				EOM.bMatchTerrain = bMatchTerrain;

				EOM.CustomRotation = CustomRotation;
				EOM.bOncomingRotation = bOncomingRotation;
				EOM.bStatic = bStatic;
				EOM.bSingle = bSingle;
				EOM.SinglePosition = SinglePosition;
				EOM.tName = tName;
				EOM.SingleOnlyBridgePercent = SingleOnlyBridgePercent;
				EOM.bStartMatchRoadDefinition = bStartMatchRoadDefinition;
				EOM.StartMatchRoadDef = StartMatchRoadDef;
				EOM.ThumbString = ThumbString;
				EOM.Desc = Desc;
				EOM.bIsBridge = bIsBridge;
				EOM.DisplayName = DisplayName;
			}
		}
		#endregion
		#endregion
		
		
		
		public class EdgeObjectEditorMaker{
			public GameObject EdgeObject = null;
			public bool bCombineMesh = false;
			public bool bCombineMeshCollider = false;
			public List<Vector3> EdgeObjectLocations;
			public List<Vector3> EdgeObjectRotations;
			public GSD.Roads.SignPlacementSubTypeEnum SubType = GSD.Roads.SignPlacementSubTypeEnum.Right;
			public float MeterSep = 5f;
			public bool bToggle = false;
			public bool bIsBridge = false;
			public bool bIsGSD = false;
			
			public bool bMaterialOverride = false;
			public Material EdgeMaterial1;
			public Material EdgeMaterial2;
			
			//Horizontal offsets:
			public float HorizontalSep = 5f;
			public AnimationCurve HorizontalCurve;
			//Vertical offsets:
			public float VerticalRaise = 0f;
			public AnimationCurve VerticalCurve;
			
			public Vector3 CustomRotation = default(Vector3);
			public bool bOncomingRotation = true;
			public bool bStatic = true;
			public bool bMatchTerrain = true;
			
			public float StartTime = 0f;
			public float EndTime = 1f;
			public float SingleOnlyBridgePercent = 0f;
			public bool bSingle = false;
			public float SinglePosition;
			public string tName;
			
			public bool bStartMatchRoadDefinition = false;
			public float StartMatchRoadDef = 0f;

			
			public void Setup(EdgeObjectMaker EOM){
				EdgeObject = EOM.EdgeObject;
				bCombineMesh = EOM.bCombineMesh;
				bCombineMeshCollider = EOM.bCombineMeshCollider;
				MeterSep = EOM.MeterSep;
				bToggle = EOM.bToggle;
				
				bMaterialOverride = EOM.bMaterialOverride;
				EdgeMaterial1 = EOM.EdgeMaterial1;
				EdgeMaterial2 = EOM.EdgeMaterial2;
				
				HorizontalSep = EOM.HorizontalSep;
				HorizontalCurve = EOM.HorizontalCurve;
				VerticalRaise = EOM.VerticalRaise;
				VerticalCurve = EOM.VerticalCurve;
				bMatchTerrain = EOM.bMatchTerrain;
				
				CustomRotation = EOM.CustomRotation;
				bOncomingRotation = EOM.bOncomingRotation;
				bStatic = EOM.bStatic;
				bSingle = EOM.bSingle;
				SinglePosition = EOM.SinglePosition;
				tName = EOM.tName;
				
				StartTime = EOM.StartTime;
				EndTime = EOM.EndTime;
				SingleOnlyBridgePercent = EOM.SingleOnlyBridgePercent;
				bStartMatchRoadDefinition = EOM.bStartMatchRoadDefinition;
				StartMatchRoadDef = EOM.StartMatchRoadDef;
			}
			
			public void LoadTo(EdgeObjectMaker EOM){
				EOM.EdgeObject = EdgeObject;
				EOM.bMaterialOverride = bMaterialOverride;
				EOM.EdgeMaterial1 = EdgeMaterial1;
				EOM.EdgeMaterial2 = EdgeMaterial2;
			
				EOM.bCombineMesh = bCombineMesh;
				EOM.bCombineMeshCollider = bCombineMeshCollider;
				EOM.SubType = SubType;
				EOM.MeterSep = MeterSep;
				EOM.bToggle = bToggle;

				EOM.HorizontalSep = HorizontalSep;
				EOM.HorizontalCurve = HorizontalCurve;
				EOM.VerticalRaise = VerticalRaise;
				EOM.VerticalCurve = VerticalCurve;
				EOM.bMatchTerrain = bMatchTerrain;

				EOM.CustomRotation = CustomRotation;
				EOM.bOncomingRotation = bOncomingRotation;
				EOM.bStatic = bStatic;
				EOM.bSingle = bSingle;
				EOM.StartTime = StartTime;
				EOM.EndTime = EndTime;
				EOM.SinglePosition = SinglePosition;
				EOM.tName = tName;
				EOM.SingleOnlyBridgePercent = SingleOnlyBridgePercent;
				EOM.bStartMatchRoadDefinition = bStartMatchRoadDefinition;
				EOM.StartMatchRoadDef = StartMatchRoadDef;
			}
			
			public bool IsEqual(EdgeObjectMaker EOM){
				if(EOM.EdgeObject != EdgeObject){ return false; }
				if(EOM.bMaterialOverride != bMaterialOverride){ return false; }
				if(EOM.EdgeMaterial1 != EdgeMaterial1){ return false; }
				if(EOM.EdgeMaterial2 != EdgeMaterial2){ return false; }
			
				if(EOM.bCombineMesh != bCombineMesh){ return false; }
				if(EOM.bCombineMeshCollider != bCombineMeshCollider){ return false; }
				if(EOM.SubType != SubType){ return false; }
				if(!GSDRootUtil.IsApproximately(EOM.MeterSep,MeterSep,0.001f)){ return false; }
//				if(EOM.bToggle != bToggle){ return false; }

				if(!GSDRootUtil.IsApproximately(EOM.HorizontalSep,HorizontalSep,0.001f)){ return false; }
				if(EOM.HorizontalCurve != HorizontalCurve){ return false; }
				if(!GSDRootUtil.IsApproximately(EOM.VerticalRaise,VerticalRaise,0.001f)){ return false; }
				if(EOM.VerticalCurve != VerticalCurve){ return false; }
				if(EOM.bMatchTerrain != bMatchTerrain){ return false; }

				if(EOM.CustomRotation != CustomRotation){ return false; }
				if(EOM.bOncomingRotation != bOncomingRotation){ return false; }
				if(EOM.bStatic != bStatic){ return false; }
				if(EOM.bSingle != bSingle){ return false; }
				
				if(!GSDRootUtil.IsApproximately(EOM.SinglePosition,SinglePosition,0.001f)){ return false; }
				if(!GSDRootUtil.IsApproximately(EOM.StartTime,StartTime,0.001f)){ return false; }
				if(!GSDRootUtil.IsApproximately(EOM.EndTime,EndTime,0.001f)){ return false; }
				if(EOM.tName != tName){ return false; }
				if(!GSDRootUtil.IsApproximately(EOM.SingleOnlyBridgePercent,SingleOnlyBridgePercent,0.001f)){ return false; }
				if(EOM.bStartMatchRoadDefinition != bStartMatchRoadDefinition){ return false; }
				if(!GSDRootUtil.IsApproximately(EOM.StartMatchRoadDef,StartMatchRoadDef,0.001f)){ return false; }
				
				return true;
			}
		}
		
		
		#region "Setup and processing"
		public void Setup(bool bCollect = true){
			#if UNITY_EDITOR
			List<GameObject> tErrorObjs = new List<GameObject>();
			try{
				Setup_Do(bCollect, ref tErrorObjs);
			}catch(System.Exception e){
				if(tErrorObjs != null && tErrorObjs.Count > 0){
					int tCount = tErrorObjs.Count;
					for(int i=0;i<tCount;i++){
						if(tErrorObjs[i] != null){
							Object.DestroyImmediate(tErrorObjs[i]);
						}
					}
					throw e;
				}
			}
			#endif
		}
		private void Setup_Do(bool bCollect, ref List<GameObject> tErrorObjs){
			#if UNITY_EDITOR
			if(EdgeObjects == null){ EdgeObjects = new List<GameObject>(); }
			if(HorizontalCurve == null){ 
				HorizontalCurve = new AnimationCurve();
				HorizontalCurve.AddKey(0f,1f);
				HorizontalCurve.AddKey(1f,1f);
			}
			if(VerticalCurve == null){
				VerticalCurve = new AnimationCurve();
				VerticalCurve.AddKey(0f,1f);
				VerticalCurve.AddKey(1f,1f);
			}
			
			SetupUniqueIdentifier();

			SetupLocations();
			EdgeObjectString = GSDRootUtil.GetPrefabString(EdgeObject);
			if(EdgeMaterial1 != null){ EdgeMaterial1String = UnityEditor.AssetDatabase.GetAssetPath(EdgeMaterial1); }
			if(EdgeMaterial2 != null){ EdgeMaterial2String = UnityEditor.AssetDatabase.GetAssetPath(EdgeMaterial2); }
			EdgeObjects = new List<GameObject>();

			Quaternion xRot = default(Quaternion);
			xRot = Quaternion.identity;
			xRot.eulerAngles = CustomRotation;
			int lCount = EdgeObjectLocations.Count;
//			Quaternion OrigRot = Quaternion.identity;
			Material[] tMats = null;
			GameObject tObj = null;
			if(EdgeObject != null){
				GameObject mObj = new GameObject(EdgeObject.name); 
				MasterObj = mObj;
				tErrorObjs.Add(MasterObj);
				mObj.transform.position = tNode.transform.position;
				mObj.transform.parent = tNode.transform;
				mObj.name = tName;
				MeshRenderer OrigMR = EdgeObject.GetComponent<MeshRenderer>();
				for(int j=0;j<lCount;j++){
					if(EdgeObjectRotations[j] == default(Vector3)){
						tObj = (GameObject)GameObject.Instantiate(EdgeObject);	
						tErrorObjs.Add(tObj);
						tObj.transform.position = EdgeObjectLocations[j];
					}else{
						tObj = (GameObject)GameObject.Instantiate(EdgeObject,EdgeObjectLocations[j],Quaternion.LookRotation(EdgeObjectRotations[j]));	
						tErrorObjs.Add(tObj);
					}
//					OrigRot = tObj.transform.rotation;
					tObj.transform.rotation *= xRot;
						if(bOncomingRotation && SubType == GSD.Roads.SignPlacementSubTypeEnum.Left){
							Quaternion tRot = new Quaternion(0f,0f,0f,0f);
							tRot = Quaternion.identity;
							tRot.eulerAngles = new Vector3(0f,180f,0f);
							tObj.transform.rotation *= tRot;
						}
					tObj.isStatic = bStatic;
					tObj.transform.parent = mObj.transform;
					EdgeObjects.Add(tObj);
					
					MeshRenderer NewMR = tObj.GetComponent<MeshRenderer>();
					if(NewMR == null){
						NewMR = tObj.AddComponent<MeshRenderer>();	
					}
					
					if(!bMaterialOverride && OrigMR != null && OrigMR.sharedMaterials.Length > 0 && NewMR != null){
						NewMR.sharedMaterials = OrigMR.sharedMaterials;	
					}else{
						if(EdgeMaterial1 != null){
							if(EdgeMaterial2 != null){
								tMats = new Material[2];
								tMats[0] = EdgeMaterial1;
								tMats[1] = EdgeMaterial2;
							}else{
								tMats = new Material[1];
								tMats[0] = EdgeMaterial1;
							}
							NewMR.sharedMaterials = tMats;
						}
					}
				}	
			}
			
			lCount = EdgeObjects.Count;
			if(lCount > 1 && bCombineMesh){
				Material[] tMat = null;
				Mesh xMeshBuffer = null;
				xMeshBuffer = EdgeObject.GetComponent<MeshFilter>().sharedMesh;
				if(bMaterialOverride){
					if(EdgeMaterial1 != null){
						if(EdgeMaterial2 != null){
							tMat = new Material[2];
							tMat[0] = EdgeMaterial1;
							tMat[1] = EdgeMaterial2;
						}else{
							tMat = new Material[1];
							tMat[0] = EdgeMaterial1;
						}
					}
				}else{
					tMat = EdgeObject.GetComponent<MeshRenderer>().sharedMaterials;	
				}
				
				Vector3[] kVerts = xMeshBuffer.vertices;
				int[] kTris = xMeshBuffer.triangles;
				Vector2[] kUV = xMeshBuffer.uv;
				int OrigMVL = kVerts.Length;
				int OrigTriCount = xMeshBuffer.triangles.Length;
				
				List<Vector3[]> hVerts = new List<Vector3[]>();
				List<int[]> hTris = new List<int[]>();
				List<Vector2[]> hUV = new List<Vector2[]>();
				

				Transform tTrans;
				for(int j=0;j<lCount;j++){
					tTrans = EdgeObjects[j].transform;
					hVerts.Add(new Vector3[OrigMVL]);
					hTris.Add(new int[OrigTriCount]);
					hUV.Add(new Vector2[OrigMVL]);
					
					//Vertex copy:
					System.Array.Copy(kVerts,hVerts[j],OrigMVL);
					//Tri copy:
					System.Array.Copy(kTris,hTris[j],OrigTriCount);
					//UV copy:
					System.Array.Copy(kUV,hUV[j],OrigMVL);
		
					Vector3 tVect = default(Vector3);
					for(int i=0;i<OrigMVL;i++){
						tVect = hVerts[j][i];
						hVerts[j][i] = tTrans.rotation * tVect;
						hVerts[j][i] += tTrans.localPosition;
					}
				}
				
				GameObject xObj = new GameObject(tName);
				MeshRenderer MR = xObj.GetComponent<MeshRenderer>();
				if(MR == null){ MR = xObj.AddComponent<MeshRenderer>(); }
				xObj.isStatic = bStatic;
				xObj.transform.parent = MasterObj.transform;
				tErrorObjs.Add(xObj);
				xObj.transform.name = xObj.transform.name + "Combined";
				xObj.transform.name = xObj.transform.name.Replace("(Clone)","");
				MeshFilter MF = xObj.GetComponent<MeshFilter>();
				if(MF == null){ MF = xObj.AddComponent<MeshFilter>(); }
				MF.sharedMesh = GSDCombineMeshes(ref hVerts,ref hTris, ref hUV,OrigMVL,OrigTriCount);
				MeshCollider MC = xObj.GetComponent<MeshCollider>();
				if(MC == null){ MC = xObj.AddComponent<MeshCollider>(); }
				xObj.transform.position = tNode.transform.position;
				xObj.transform.rotation = Quaternion.identity;
	
				for(int j=(lCount-1);j>=0;j--){
					Object.DestroyImmediate(EdgeObjects[j]);
				}
				for(int j=0;j<EdgeObjects.Count;j++){
					EdgeObjects[j] = null;
				}
				EdgeObjects.RemoveRange(0,lCount);
				EdgeObjects.Add(xObj);
				
				if(tMat != null && MR != null){
					MR.sharedMaterials = tMat;
				}
				
				BoxCollider BC = xObj.GetComponent<BoxCollider>();
				if(BC != null){ Object.DestroyImmediate(BC); }
				int cCount = xObj.transform.childCount;
				int spamc = 0;
				while(cCount > 0 && spamc < 10){
					Object.DestroyImmediate(xObj.transform.GetChild(0).gameObject);
					cCount = xObj.transform.childCount;
					spamc+=1;
				}
				
				if(bCombineMeshCollider){
					if(MC == null){ MC = xObj.AddComponent<MeshCollider>(); }
					MC.sharedMesh = MF.sharedMesh;
				}else{
					if(MC != null){
						Object.DestroyImmediate(MC);	
						MC = null;
					}
				}
				
				if(tNode.GSDSpline.tRoad.GSDRS.opt_bSaveMeshes && MF != null && bCombineMesh){
					SaveMesh(MF.sharedMesh,false);
					if(MC != null){
						if(MF.sharedMesh != MC.sharedMesh){
							SaveMesh(MC.sharedMesh,true);
						}
					}
				}
				
//				tMesh = null;
			}
			
			//Zero these out, as they are not needed anymore:
			if(EdgeObjectLocations != null){ EdgeObjectLocations.Clear(); EdgeObjectLocations=null; }
			if(EdgeObjectRotations != null){ EdgeObjectRotations.Clear(); EdgeObjectRotations=null; }
			
			if(bCollect){
				tNode.GSDSpline.tRoad.bTriggerGC = true;
			}
			#endif
		}
	
		private void SetupLocations(){
			float OrigHeight = 0f;
			StartTime = tNode.GSDSpline.GetClosestParam(StartPos);
			EndTime = tNode.GSDSpline.GetClosestParam(EndPos);
			
			float FakeStartTime = StartTime;
			if(bStartMatchRoadDefinition){
				int tIndex = tNode.GSDSpline.GetClosestRoadDefIndex(StartTime,false,true);
				float jTime1 = tNode.GSDSpline.TranslateInverseParamToFloat(tNode.GSDSpline.RoadDefKeysArray[tIndex]);
				float jTime2 = jTime1;
				if(tIndex+1 < tNode.GSDSpline.RoadDefKeysArray.Length){
					jTime2 = tNode.GSDSpline.TranslateInverseParamToFloat(tNode.GSDSpline.RoadDefKeysArray[tIndex+1]);
				}
				FakeStartTime = jTime1 + ((jTime2 - jTime1) * StartMatchRoadDef);
			}
			
			
//			int eCount = EdgeObjects.Count;
//			Vector3 rVect = default(Vector3);
//			Vector3 lVect = default(Vector3);
//			float fTimeMax = -1f;
			int mCount = tNode.GSDSpline.GetNodeCount();
			if(tNode.idOnSpline >= mCount-1){ return; }
//			fTimeMax = tNode.GSDSpline.mNodes[tNode.idOnSpline+1].tTime;
//			float tStep = -1f;
			Vector3 tVect = default(Vector3);
			Vector3 POS = default(Vector3);
			
//			tStep = MeterSep/tNode.GSDSpline.distance;
			//Destroy old objects:
			ClearEOM();
			//Make sure old locs and rots are fresh:
			if(EdgeObjectLocations != null){ EdgeObjectLocations.Clear(); EdgeObjectLocations=null; }
			EdgeObjectLocations = new List<Vector3>();
			if(EdgeObjectRotations != null){ EdgeObjectRotations.Clear(); EdgeObjectRotations=null; }
			EdgeObjectRotations = new List<Vector3>();
			bool bIsCenter = GSDRootUtil.IsApproximately(HorizontalSep,0f,0.02f);
			
			
			//Set rotation and locations:
//			Vector2 temp2DVect = default(Vector2);
			Ray tRay = default(Ray);
			RaycastHit[] tRayHit = null;
			float[] tRayYs = null;
			if(bSingle){
				tNode.GSDSpline.GetSplineValue_Both(SinglePosition,out tVect,out POS);
				OrigHeight = tVect.y;
				
				//Horizontal offset:
				if(!bIsCenter){
//					if(HorizontalSep > 0f){
						tVect = (tVect + new Vector3(HorizontalSep*POS.normalized.z,0,HorizontalSep*-POS.normalized.x));
//					}else{
//						tVect = (tVect + new Vector3(HorizontalSep*-POS.normalized.z,0,HorizontalSep*POS.normalized.x));
//					}
				}
				
				
				
				//Vertical:
				if(bMatchTerrain){
					tRay = new Ray(tVect+new Vector3(0f,1f,0f),Vector3.down);
					tRayHit = Physics.RaycastAll(tRay);
					if(tRayHit.Length > 0){
						tRayYs = new float[tRayHit.Length];
						for(int g=0;g<tRayHit.Length;g++){
							tRayYs[g] = tRayHit[g].point.y;	
						}
						tVect.y = Mathf.Max(tRayYs);
					}
				}
				tVect.y += VerticalRaise;
				
				StartPos = tVect;
				EndPos = tVect;
				
				if(float.IsNaN(tVect.y)){
					tVect.y = OrigHeight;	
				}
				
				EdgeObjectLocations.Add(tVect);
				EdgeObjectRotations.Add(POS);
			}else{
							
				//Get the vector series that this mesh is interpolated on:
				List<float> tTimes = new List<float>();
				float cTime = FakeStartTime;
				tTimes.Add(cTime);
				int SpamGuard = 5000;
				int SpamGuardCounter = 0;
				float pDiffTime = EndTime - FakeStartTime;
				float CurrentH = 0f;
				float fHeight = 0f;
				Vector3 xVect = default(Vector3);
				while(cTime < EndTime && SpamGuardCounter < SpamGuard){
					tNode.GSDSpline.GetSplineValue_Both(cTime,out tVect,out POS);
					
					fHeight = HorizontalCurve.Evaluate((cTime-FakeStartTime) / pDiffTime);
					CurrentH = fHeight * HorizontalSep;
		
					if(CurrentH < 0f){
						CurrentH *= -1f;
						tVect = (tVect + new Vector3(CurrentH*-POS.normalized.z,0,CurrentH*POS.normalized.x));
					}else if(CurrentH > 0f){
						tVect = (tVect + new Vector3(CurrentH*POS.normalized.z,0,CurrentH*-POS.normalized.x));
					}
					
					xVect = (POS.normalized * MeterSep) + tVect;
					
					cTime = tNode.GSDSpline.GetClosestParam(xVect,false,false);
					
					if(cTime > EndTime){
						break;
					}
					tTimes.Add(cTime);
					SpamGuardCounter+=1;
				}
				int vSeriesCount = tTimes.Count;

				float mMin = FakeStartTime;
				float mMax = EndTime;
				float tPercent = 0;
				for(int i=0;i<vSeriesCount;i++){
					tNode.GSDSpline.GetSplineValue_Both(tTimes[i],out tVect,out POS);
					
					tPercent = ((tTimes[i] - mMin) / (mMax - mMin));

					//Horiz:
					CurrentH = (HorizontalCurve.Evaluate(tPercent) * HorizontalSep);
					if(CurrentH < 0f){
						CurrentH *= -1f;
						tVect = (tVect + new Vector3(CurrentH*-POS.normalized.z,0,CurrentH*POS.normalized.x));
					}else if(CurrentH > 0f){
						tVect = (tVect + new Vector3(CurrentH*POS.normalized.z,0,CurrentH*-POS.normalized.x));
					}
					
					//Vertical:
					if(bMatchTerrain){
						tRay = new Ray(tVect+new Vector3(0f,1f,0f),Vector3.down);
						tRayHit = Physics.RaycastAll(tRay);
						if(tRayHit.Length > 0){
							tRayYs = new float[tRayHit.Length];
							for(int g=0;g<tRayHit.Length;g++){
								tRayYs[g] = tRayHit[g].point.y;	
							}
							tVect.y = Mathf.Max(tRayYs);
						}
					}
					
					tVect.y += (VerticalCurve.Evaluate(tPercent) * VerticalRaise);
					
					EdgeObjectLocations.Add(tVect);
					EdgeObjectRotations.Add(POS);
				}
				StartPos = tNode.GSDSpline.GetSplineValue(StartTime);
				EndPos = tNode.GSDSpline.GetSplineValue(EndTime);
			}
		}

		//ref hVerts,ref hTris, ref hNormals, ref hUV, ref hTangents
		private Mesh GSDCombineMeshes(ref List<Vector3[]> hVerts,ref List<int[]> hTris, ref List<Vector2[]> hUV, int OrigMVL, int OrigTriCount){
			int mCount = hVerts.Count;
			int NewMVL = OrigMVL * mCount;
			Vector3[] tVerts = new Vector3[NewMVL];
			int[] tTris = new int[OrigTriCount*mCount];
			Vector3[] tNormals = new Vector3[NewMVL];
			Vector2[] tUV = new Vector2[NewMVL];
			
			int CurrentMVLIndex = 0;
			int CurrentTriIndex = 0;
			for(int j=0;j<mCount;j++){
				CurrentMVLIndex = OrigMVL * j;
				CurrentTriIndex = OrigTriCount * j;

				if(j>0){
					for(int i=0;i<OrigTriCount;i++){
						hTris[j][i] += CurrentMVLIndex;
					}
				}

				System.Array.Copy(hVerts[j],0,tVerts,CurrentMVLIndex,OrigMVL);
				System.Array.Copy(hTris[j],0,tTris,CurrentTriIndex,OrigTriCount);
				System.Array.Copy(hUV[j],0,tUV,CurrentMVLIndex,OrigMVL);
			}
			
			Mesh tMesh = new Mesh();
			tMesh.vertices = tVerts;
			tMesh.triangles = tTris;
			tMesh.uv = tUV;
			tMesh.normals = tNormals;
			tMesh.RecalculateBounds();
			tMesh.RecalculateNormals();
			tMesh.tangents = GSDRootUtil.ProcessTangents(tTris,tNormals,tUV,tVerts);
			return tMesh;
		}
		
		public void ClearEOM(){
			if(EdgeObjects != null){
				int hCount = EdgeObjects.Count;
				for(int h=(hCount-1);h>=0;h--){
					if(EdgeObjects[h] != null){
						Object.DestroyImmediate(EdgeObjects[h].transform.gameObject);	
					}
				}
			}	
			if(MasterObj != null){
				Object.DestroyImmediate(MasterObj);
			}
		}
		#endregion
		
		
		
		public void SetDefaultTimes(bool bIsEndPoint, float tTime, float tTimeNext, int idOnSpline, float tDist){
			if(!bIsEndPoint){
				StartTime = tTime;
				EndTime = tTimeNext;
			}else{
				if(idOnSpline < 2){
					StartTime = tTime;
					EndTime = tTimeNext;
				}else{
					StartTime = tTime;
					EndTime = tTime-(125f/tDist);
				}
			}
		}
		
		
	}
	
	#endif	
}