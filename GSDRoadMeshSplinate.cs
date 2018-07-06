#region "Imports"
using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using GSD;
using System.IO;
#endif
#endregion
namespace GSD.Roads.Splination{
	public enum AxisTypeEnum {X,Z};
	public enum CollisionTypeEnum {None,SimpleMeshTriangle,SimpleMeshTrapezoid,MeshCollision,BoxCollision};
	public enum RepeatUVTypeEnum {None,X,Y};
	
	#if UNITY_EDITOR
	
	[System.Serializable]
	public class SplinatedMeshMaker{
		public bool bNeedsUpdate = false;
		public string UID = "";
		public bool bIsGSD = false;
		
		public Transform MasterObjTrans = null;
		public GameObject CurrentSplination = null;
		public string CurrentSplinationString = "";
		public GameObject CurrentSplinationCap1 = null;
		public string CurrentSplinationCap1String = "";
		public GameObject CurrentSplinationCap2 = null;
		public string CurrentSplinationCap2String = "";
		public float CapHeightOffset1 = 0f;
		public float CapHeightOffset2 = 0f;
		
		public GameObject Output = null;
		public Mesh tMesh = null;
		public bool bMaterialOverride = false;
		public Material SplinatedMaterial1 = null;
		public Material SplinatedMaterial2 = null;
		public string SplinatedMaterial1String = "";
		public string SplinatedMaterial2String = "";
		public float MinMaxMod = 0.002f;
		public float VertexMatchingPrecision = 0.005f;
		
		public bool bExactSplination = false;
		public bool bMatchRoadDefinition = false;
		public bool bMatchRoadIncrements = true;
		public bool bTrimEnd = false;
		public bool bTrimStart = false;
		public bool bToggle = false;
		public bool bMatchTerrain = false;
		public bool bIsBridge = false;
		
		public bool bIsStretch = false;
		public bool bStretchLocOffset = false;
		public bool bStretchSize = false;
		public Vector3 StretchBC_LocOffset = default(Vector3);
		public Vector3 StretchBC_Size = default(Vector3);
		public float Stretch_UVThreshold = 0.05f;
		public bool bStraightLineMatchStartEnd = false;
		public bool bBCFlipX = false;
		public bool bBCFlipZ = false;

		//Horizontal offsets:
		public float HorizontalSep = 0f;
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
		//Vertical cutoff
		public float VerticalCutoff = 0f;
		public bool bVerticalCutoff = false;
		public bool bVerticalCutoffDownwards = false;
		public bool bVerticalMeshCutoff_OppositeDir = false;
		public float VerticalMeshCutoffOffset = 0.04f;
		public bool bVerticalCutoff_MatchZero = false;

		public float RoadRaise = 0f;
		public Vector3 CustomRotation = default(Vector3);
		public bool bFlipRotation = false;
		public bool bStatic = true;
		public Vector3 StartPos = default(Vector3);
		public Vector3 EndPos = default(Vector3);
		public float StartTime = 0f;
		public float EndTime = 1f;
		public GSDSplineC tSpline = null;
		public GSDSplineN tNode = null;
		public AxisTypeEnum Axis = AxisTypeEnum.X;
		
		public float mMaxX = -1f;
		public float mMinX = -1f;
		public float mMaxY = -1f;
		public float mMinY = -1f;
		public float mMaxZ = -1f;
		public float mMinZ = -1f;
		
		public RepeatUVTypeEnum RepeatUVType = RepeatUVTypeEnum.None;
		public bool bNoCenterMode = true;
		
		//End objects:
		public GameObject EndCapStart = null;
		public GameObject EndCapEnd = null;
		public GameObject EndCapStartOutput = null;
		public GameObject EndCapEndOutput = null;
		public string EndCapStartString = "";
		public string EndCapEndString = "";
		public bool bEndCapCustomMatchStart = true;
		public bool bEndObjectsMatchGround = false;
		public Vector3 EndCapCustomOffsetStart = default(Vector3);
		public Vector3 EndCapCustomOffsetEnd = default(Vector3);
		public Vector3 EndCapCustomRotOffsetStart = default(Vector3);
		public Vector3 EndCapCustomRotOffsetEnd = new Vector3(0f,180f,0f);
		//Endings down:
		public bool bStartDown = false;
		public bool bStartTypeDownOverride = false;
		public float StartTypeDownOverride = 0f;
		public bool bEndDown = false;
		public bool bEndTypeDownOverride = false;
		public float EndTypeDownOverride = 0f;
		
		//Collision:
		public CollisionTypeEnum CollisionType = CollisionTypeEnum.SimpleMeshTriangle;
		public bool bCollisionConvex = false;
		public bool bSimpleCollisionAutomatic = true;
		public bool bCollisionExtrude = false;
		public bool bCollisionTrigger = false;
		
		public Vector3 CollisionBoxBL = default(Vector3);
		public Vector3 CollisionBoxBR = default(Vector3);
		public Vector3 CollisionBoxTL = default(Vector3);
		public Vector3 CollisionBoxTR = default(Vector3);

		public Vector3 CollisionTriBL = default(Vector3);
		public Vector3 CollisionTriBR = default(Vector3);
		public Vector3 CollisionTriT = default(Vector3);
		
		public string tName = "ExtrudedObject";
		public string ThumbString = "";
		public string Desc = "";
		public string DisplayName = "";
		
		public SplinatedMeshEditorMaker EM = null;
		
		public void Init(GSDSplineC _tSpline, GSDSplineN _tNode, Transform tTrans){
			tSpline = _tSpline;
			tNode = _tNode;
			MasterObjTrans = tTrans;
			SetupUniqueIdentifier();
		}
		
		public SplinatedMeshMaker Copy(){
			SplinatedMeshMaker SMM = new SplinatedMeshMaker();
			SMM.Init(tSpline,tNode,MasterObjTrans);
			SMM.MasterObjTrans = MasterObjTrans;
			SMM.bIsGSD = bIsGSD;
			
			SMM.CurrentSplination = CurrentSplination;
			SMM.CurrentSplinationString = CurrentSplinationString;
			
			SMM.CurrentSplinationCap1 = CurrentSplinationCap1;
			SMM.CurrentSplinationCap1String = CurrentSplinationCap1String;
			SMM.CurrentSplinationCap2 = CurrentSplinationCap2;
			SMM.CurrentSplinationCap2String = CurrentSplinationCap2String;
			SMM.CapHeightOffset1 = CapHeightOffset1;
			SMM.CapHeightOffset1 = CapHeightOffset2;

			SMM.Output = Output;
			SMM.tMesh = tMesh;
			SMM.bMaterialOverride = bMaterialOverride;
			SMM.SplinatedMaterial1 = SplinatedMaterial1;
			SMM.SplinatedMaterial2 = SplinatedMaterial2;
			SMM.SplinatedMaterial1String = SplinatedMaterial1String;
			SMM.SplinatedMaterial2String = SplinatedMaterial2String;
			SMM.bExactSplination = bExactSplination;
			SMM.bMatchRoadDefinition = bMatchRoadDefinition;
			SMM.bMatchRoadIncrements = bMatchRoadIncrements;
			SMM.bTrimStart = bTrimStart;
			SMM.bTrimEnd = bTrimEnd;
			SMM.bMatchTerrain = bMatchTerrain;
			SMM.MinMaxMod = MinMaxMod;
			SMM.VertexMatchingPrecision = VertexMatchingPrecision;
			
			SMM.bIsStretch = bIsStretch;
			SMM.bStretchLocOffset = bStretchLocOffset;
			SMM.bStretchSize = bStretchSize;
			SMM.StretchBC_LocOffset = StretchBC_LocOffset;
			SMM.StretchBC_Size = StretchBC_Size;
			SMM.Stretch_UVThreshold = Stretch_UVThreshold;
			SMM.bStraightLineMatchStartEnd = bStraightLineMatchStartEnd;
			SMM.bBCFlipX = bBCFlipX;
			SMM.bBCFlipZ = bBCFlipZ;

			//Horizontal offsets:
			SMM.HorizontalSep = HorizontalSep;
			SMM.HorizontalCurve = new AnimationCurve();
			if(HorizontalCurve != null && HorizontalCurve.keys.Length > 0){
				for(int i=0;i<HorizontalCurve.keys.Length;i++){
					SMM.HorizontalCurve.AddKey(HorizontalCurve.keys[i]);
				}
			}
			//Vertical offset:
			SMM.VerticalRaise = VerticalRaise;
			SMM.VerticalCurve = new AnimationCurve();
			if(VerticalCurve != null && VerticalCurve.keys.Length > 0){
				for(int i=0;i<VerticalCurve.keys.Length;i++){
					SMM.VerticalCurve.AddKey(VerticalCurve.keys[i]);
				}
			}
			
			//Vertical cutoff:
			SMM.bVerticalCutoff = bVerticalCutoff;
			SMM.VerticalCutoff = VerticalCutoff;
			SMM.bVerticalCutoffDownwards = bVerticalCutoffDownwards;
			SMM.bVerticalMeshCutoff_OppositeDir = bVerticalMeshCutoff_OppositeDir;
			SMM.VerticalMeshCutoffOffset = VerticalMeshCutoffOffset;
			SMM.bVerticalCutoff_MatchZero = bVerticalCutoff_MatchZero;

			SMM.RoadRaise = RoadRaise;
			SMM.CustomRotation = CustomRotation;
			SMM.bFlipRotation = bFlipRotation;
			SMM.bStatic = bStatic;
			SMM.StartTime = StartTime;
			SMM.EndTime = EndTime;
			SMM.StartPos = StartPos;
			SMM.EndPos = EndPos;
			SMM.Axis = Axis;
			
			SMM.RepeatUVType = RepeatUVType;
			
			SMM.mMaxX = mMaxX;
			SMM.mMinX = mMinX;
			SMM.mMaxY = mMaxY;
			SMM.mMinY = mMinY;
			SMM.mMaxZ = mMaxZ;
			SMM.mMinZ = mMinZ;
			
			//End objects:
			SMM.EndCapStart = EndCapStart;
			SMM.EndCapStartString = EndCapStartString;
			SMM.EndCapEnd = EndCapEnd;
			SMM.EndCapEndString = EndCapEndString;
			SMM.bEndCapCustomMatchStart = bEndCapCustomMatchStart;
			SMM.EndCapCustomOffsetStart = EndCapCustomOffsetStart;
			SMM.EndCapCustomOffsetEnd = EndCapCustomOffsetEnd;
			SMM.EndCapCustomRotOffsetStart = EndCapCustomRotOffsetStart;
			SMM.EndCapCustomRotOffsetEnd = EndCapCustomRotOffsetEnd;
			SMM.bEndObjectsMatchGround = bEndObjectsMatchGround;
			SMM.bIsBridge = bIsBridge;
			//End down:
			SMM.bStartDown = bStartDown;
			SMM.bStartTypeDownOverride = bStartTypeDownOverride;
			SMM.StartTypeDownOverride = StartTypeDownOverride;
			SMM.bEndDown = bEndDown;
			SMM.bEndTypeDownOverride = bEndTypeDownOverride;
			SMM.EndTypeDownOverride = EndTypeDownOverride;
			
			//Collision:
			SMM.CollisionType = CollisionType;
			SMM.bCollisionConvex = bCollisionConvex;
			SMM.bSimpleCollisionAutomatic = bSimpleCollisionAutomatic;
			SMM.bCollisionTrigger = bCollisionTrigger;
			
			SMM.CollisionBoxBL = CollisionBoxBL;
			SMM.CollisionBoxBR = CollisionBoxBR;
			SMM.CollisionBoxTL = CollisionBoxTL;
			SMM.CollisionBoxTR = CollisionBoxTR;
	
			SMM.CollisionTriBL = CollisionTriBL;
			SMM.CollisionTriBR = CollisionTriBR;
			SMM.CollisionTriT = CollisionTriT;
			
			SMM.tName = tName;
			SMM.ThumbString = ThumbString;
			SMM.Desc = Desc;	
			SMM.DisplayName = DisplayName;
			
			SMM.SetupUniqueIdentifier();
			
			return SMM;
		}
		
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
		
		public void UpdatePositions(){
			StartPos = tSpline.GetSplineValue(StartTime);
			EndPos = tSpline.GetSplineValue(EndTime);
		}
		
		public void SaveToLibrary(string fName = "", bool bIsDefault = false){
			SplinatedMeshLibraryMaker SLM = new SplinatedMeshLibraryMaker();
			SLM.Setup(this);
			GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			string tPath = xPath + "ESO" + tName + ".gsd";
			if(fName.Length > 0){
				if(bIsDefault){
					tPath = xPath + "Q/ESO" + fName + ".gsd";
				}else{
					tPath = xPath + "ESO" + fName + ".gsd";	
				}
			}
			GSDRootUtil.CreateXML<SplinatedMeshLibraryMaker>(ref tPath,SLM);
		}

		public void LoadFromLibrary(string xName, bool bIsQuickAdd = false){
			string xPath = GSDRootUtil.Dir_GetLibrary();
			string tPath = xPath + "ESO" + xName + ".gsd";
			if(bIsQuickAdd){
				GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
				tPath = xPath + "Q/ESO" + xName + ".gsd";
			}
			SplinatedMeshLibraryMaker SLM = (SplinatedMeshLibraryMaker)GSDRootUtil.LoadXML<SplinatedMeshLibraryMaker>(ref tPath);
			SLM.LoadToSMM(this);
			bNeedsUpdate = true;
		}
		
		public void LoadFromLibraryWizard(string xName){
			GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			string tPath = xPath + "W/" + xName + ".gsd";
			SplinatedMeshLibraryMaker SLM = (SplinatedMeshLibraryMaker)GSDRootUtil.LoadXML<SplinatedMeshLibraryMaker>(ref tPath);
			SLM.LoadToSMM(this);
			bNeedsUpdate = true;
		}
		
		public void LoadFromLibraryBulk(ref SplinatedMeshLibraryMaker SLM){
			SLM.LoadToSMM(this);
//			bNeedsUpdate = true;
		}
		
		public static SplinatedMeshLibraryMaker SLMFromData(string tData){
			try{
				SplinatedMeshLibraryMaker SLM = (SplinatedMeshLibraryMaker)GSDRootUtil.LoadData<SplinatedMeshLibraryMaker>(ref tData);	
				return SLM;
			}catch{
				return null;	
			}
		}

		public string ConvertToString(){
			SplinatedMeshLibraryMaker SLM = new SplinatedMeshLibraryMaker();
			SLM.Setup(this);
			return GSDRootUtil.GetString<SplinatedMeshLibraryMaker>(SLM);
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
				if(tInfo.Name.Contains("ESO") && tInfo.Extension.ToLower().Contains("gsd")){
					tCount+=1;
				}
			}
			
			tNames = new string[tCount];
			tPaths = new string[tCount];
			tCount = 0;
			foreach(FileInfo tInfo in fileInfo){
				if(tInfo.Name.Contains("ESO") && tInfo.Extension.ToLower().Contains("gsd")){
					tNames[tCount] = tInfo.Name.Replace(".gsd","").Replace("ESO","");
					tPaths[tCount] = tInfo.FullName;
					tCount+=1;
				}
			}
			#endif
		}

		public void Kill(){
			if(Output != null){
				Object.DestroyImmediate(Output);
			}
			if(EndCapStartOutput != null){
				Object.DestroyImmediate(EndCapStartOutput);
			}
			if(EndCapEndOutput != null){
				Object.DestroyImmediate(EndCapEndOutput);
			}
		}
		
		[System.Serializable]
		public class SplinatedMeshLibraryMaker{
			public string CurrentSplinationString = "";
			public string CurrentSplinationCap1String = "";
			public string CurrentSplinationCap2String = "";
			public bool bIsGSD = false;
			
			public bool bMaterialOverride = false;
			public string SplinatedMaterial1String = "";
			public string SplinatedMaterial2String = "";
			public float CapHeightOffset1 = 0f;
			public float CapHeightOffset2 = 0f;
			public bool bExactSplination = false;
			public bool bMatchRoadDefinition = false;
			public bool bMatchRoadIncrements = true;
			public bool bTrimStart = false;
			public bool bTrimEnd = false;
			public bool bToggle = false;
			public bool bMatchTerrain = false;
			public float MinMaxMod = 0.002f;
			public float VertexMatchingPrecision = 0.005f;
			
			public bool bIsStretch = false;
			public bool bStretchLocOffset = false;
			public bool bStretchSize = false;
			public Vector3 StretchBC_LocOffset = default(Vector3);
			public Vector3 StretchBC_Size = default(Vector3);
			public float Stretch_UVThreshold = 0.05f;
			public bool bStraightLineMatchStartEnd = false;
			public bool bBCFlipX = false;
			public bool bBCFlipZ = false;
			
			//Horizontal offsets:
			public float HorizontalSep = 5f;
			public AnimationCurve HorizontalCurve;
			//Vertical offsets:
			public float VerticalRaise = 0f;
			public AnimationCurve VerticalCurve;
			//Vertical cutoff:
			public float VerticalCutoff = 0f;
			public bool bVerticalCutoff = false;
			public bool bVerticalCutoffDownwards = false;
			public bool bVerticalMeshCutoff_OppositeDir = false;
			public float VerticalMeshCutoffOffset = 0.04f;
			public bool bVerticalCutoff_MatchZero = false;

			public float RoadRaise = 0f;
			public Vector3 CustomRotation = default(Vector3);
			public bool bFlipRotation = false;
			public bool bStatic = true;
			public float StartTime = 0f;
			public float EndTime = 1f;
			public int Axis = 0;
			public bool bIsBridge = false;
			
			public float mMaxX = -1f;
			public float mMinX = -1f;
			public float mMaxY = -1f;
			public float mMinY = -1f;
			public float mMaxZ = -1f;
			public float mMinZ = -1f;
			
			public int RepeatUVType = 0;
			public bool bNoCenterMode = true;
			
			//End objects:
			public string EndCapStartString = "";
			public string EndCapEndString = "";
			public bool bEndCapCustomMatchStart = true;
			public Vector3 EndCapCustomOffsetStart = default(Vector3);
			public Vector3 EndCapCustomOffsetEnd = default(Vector3);
			public Vector3 EndCapCustomRotOffsetStart = default(Vector3);
			public Vector3 EndCapCustomRotOffsetEnd = default(Vector3);
			public bool bEndObjectsMatchGround = false;
			//Endings down:
			public bool bStartDown = false;
			public bool bStartTypeDownOverride = false;
			public float StartTypeDownOverride = 0f;
			public bool bEndDown = false;
			public bool bEndTypeDownOverride = false;
			public float EndTypeDownOverride = 0f;
			
			//Collision:
			public int CollisionType = 0;
			public bool bCollisionConvex = false;
			public bool bSimpleCollisionAutomatic = true;
			public bool bCollisionTrigger = false;
			
			public Vector3 CollisionBoxBL = default(Vector3);
			public Vector3 CollisionBoxBR = default(Vector3);
			public Vector3 CollisionBoxTL = default(Vector3);
			public Vector3 CollisionBoxTR = default(Vector3);
	
			public Vector3 CollisionTriBL = default(Vector3);
			public Vector3 CollisionTriBR = default(Vector3);
			public Vector3 CollisionTriT = default(Vector3);
			
			public string tName = "ExtrudedObject";
			public string ThumbString = "";
			public string Desc = "";
			public string DisplayName = "";
			
			public void Setup(SplinatedMeshMaker SMM){
				CurrentSplinationString = SMM.CurrentSplinationString;
				if(SMM.CurrentSplinationCap1 == null){
					CurrentSplinationCap1String = "";
				}else{
					CurrentSplinationCap1String = SMM.CurrentSplinationCap1String;
				}
				
				if(SMM.CurrentSplinationCap2 == null){
					CurrentSplinationCap2String = "";
				}else{
					CurrentSplinationCap2String = SMM.CurrentSplinationCap2String;
				}
				bIsGSD = SMM.bIsGSD;
				
				CapHeightOffset1 = SMM.CapHeightOffset1;
				CapHeightOffset2 = SMM.CapHeightOffset2;

				bMaterialOverride = SMM.bMaterialOverride;
				SplinatedMaterial1String = SMM.SplinatedMaterial1String;
				SplinatedMaterial2String = SMM.SplinatedMaterial2String;
				bExactSplination = SMM.bExactSplination;
				bMatchRoadDefinition = SMM.bMatchRoadDefinition;
				bMatchRoadIncrements = SMM.bMatchRoadIncrements;
				bTrimStart = SMM.bTrimStart;
				bTrimEnd = SMM.bTrimEnd;
				bMatchTerrain = SMM.bMatchTerrain;
				MinMaxMod = SMM.MinMaxMod;
				bIsBridge = SMM.bIsBridge;
				VertexMatchingPrecision = SMM.VertexMatchingPrecision;
				
				bIsStretch = SMM.bIsStretch;
				bStretchLocOffset = SMM.bStretchLocOffset;
				bStretchSize = SMM.bStretchSize;
				StretchBC_LocOffset = SMM.StretchBC_LocOffset;
				StretchBC_Size = SMM.StretchBC_Size;
				Stretch_UVThreshold = SMM.Stretch_UVThreshold;
				bStraightLineMatchStartEnd = SMM.bStraightLineMatchStartEnd;
				bBCFlipX = SMM.bBCFlipX;
				bBCFlipZ = SMM.bBCFlipZ;

				//Horizontal offsets:
				HorizontalSep = SMM.HorizontalSep;
				HorizontalCurve = SMM.HorizontalCurve;
				//Vertical offset:
				VerticalRaise = SMM.VerticalRaise;
				VerticalCurve = SMM.VerticalCurve;
				//Vertical cutoff
				VerticalCutoff = SMM.VerticalCutoff;
				bVerticalCutoff = SMM.bVerticalCutoff;
				bVerticalCutoffDownwards = SMM.bVerticalCutoffDownwards;
				bVerticalMeshCutoff_OppositeDir = SMM.bVerticalMeshCutoff_OppositeDir;
				VerticalMeshCutoffOffset = SMM.VerticalMeshCutoffOffset;
				bVerticalCutoff_MatchZero = SMM.bVerticalCutoff_MatchZero;
				
				RoadRaise = SMM.RoadRaise;
				CustomRotation = SMM.CustomRotation;
				bFlipRotation = SMM.bFlipRotation;
				bStatic = SMM.bStatic;
				StartTime = SMM.StartTime;
				EndTime = SMM.EndTime;
				Axis = (int)SMM.Axis;
				
				RepeatUVType = (int)SMM.RepeatUVType;
				
				mMaxX = SMM.mMaxX;
				mMinX = SMM.mMinX;
				mMaxY = SMM.mMaxY;
				mMinY = SMM.mMinY;
				mMaxZ = SMM.mMaxZ;
				mMinZ = SMM.mMinZ;
				
				//End objects:
				if(SMM.EndCapStart == null){
					EndCapStartString = "";
				}else{
					EndCapStartString = SMM.EndCapStartString;
				}
				if(SMM.EndCapEnd == null){
					EndCapEndString = "";
				}else{
					EndCapEndString = SMM.EndCapEndString;
				}
				bEndCapCustomMatchStart = SMM.bEndCapCustomMatchStart;
				EndCapCustomOffsetStart = SMM.EndCapCustomOffsetStart;
				EndCapCustomOffsetEnd = SMM.EndCapCustomOffsetEnd;
				EndCapCustomRotOffsetStart = SMM.EndCapCustomRotOffsetStart;
				EndCapCustomRotOffsetEnd = SMM.EndCapCustomRotOffsetEnd;
				bEndObjectsMatchGround = SMM.bEndObjectsMatchGround;
				//Endings down:
				bStartDown = SMM.bStartDown;
				bStartTypeDownOverride = SMM.bStartTypeDownOverride;
				StartTypeDownOverride = SMM.StartTypeDownOverride;
				bEndDown = SMM.bEndDown;
				bEndTypeDownOverride = SMM.bEndTypeDownOverride;
				EndTypeDownOverride = SMM.EndTypeDownOverride;

				//Collision:
				CollisionType = (int)SMM.CollisionType;
				bCollisionConvex = SMM.bCollisionConvex;
				bSimpleCollisionAutomatic = SMM.bSimpleCollisionAutomatic;
				bCollisionTrigger = SMM.bCollisionTrigger;
				
				CollisionBoxBL = SMM.CollisionBoxBL;
				CollisionBoxBR = SMM.CollisionBoxBR;
				CollisionBoxTL = SMM.CollisionBoxTL;
				CollisionBoxTR = SMM.CollisionBoxTR;
		
				CollisionTriBL = SMM.CollisionTriBL;
				CollisionTriBR = SMM.CollisionTriBR;
				CollisionTriT = SMM.CollisionTriT;
				
				tName = SMM.tName;
				ThumbString = SMM.ThumbString;
				Desc = SMM.Desc;
				DisplayName = SMM.DisplayName;
			}
			
			public void LoadToSMM(SplinatedMeshMaker SMM){
				#if UNITY_EDITOR
				SMM.CurrentSplinationString = CurrentSplinationString;
				SMM.CurrentSplination = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(CurrentSplinationString,typeof(GameObject));

				SMM.CurrentSplinationCap1String = CurrentSplinationCap1String;
				if(CurrentSplinationCap1String != null && CurrentSplinationCap1String.Length > 1){
					SMM.CurrentSplinationCap1 = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(CurrentSplinationCap1String,typeof(GameObject));
				}
				
				SMM.CurrentSplinationCap2String = CurrentSplinationCap2String;
				if(CurrentSplinationCap2String != null && CurrentSplinationCap2String.Length > 1){
					SMM.CurrentSplinationCap2 = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(CurrentSplinationCap2String,typeof(GameObject));
				}

				SMM.CapHeightOffset1 = CapHeightOffset1;
				SMM.CapHeightOffset2 = CapHeightOffset2;

				SMM.bMaterialOverride = bMaterialOverride;
				SMM.SplinatedMaterial1String = SplinatedMaterial1String;
				SMM.SplinatedMaterial2String = SplinatedMaterial2String;
				
				if(bMaterialOverride){
					if(SplinatedMaterial1String != null && SplinatedMaterial1String.Length > 0){
						SMM.SplinatedMaterial1 = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(SplinatedMaterial1String,typeof(Material));
					}
					if(SplinatedMaterial2String != null && SplinatedMaterial2String.Length > 0){
						SMM.SplinatedMaterial2 = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(SplinatedMaterial2String,typeof(Material));
					}
				}
				
				SMM.bIsGSD = bIsGSD;
				SMM.bExactSplination = bExactSplination;
				SMM.bMatchRoadDefinition = bMatchRoadDefinition;
				SMM.bMatchRoadIncrements = bMatchRoadIncrements;
				SMM.bTrimStart = bTrimStart;
				SMM.bTrimEnd = bTrimEnd;
				SMM.bMatchTerrain = bMatchTerrain;
				SMM.MinMaxMod = MinMaxMod;
				SMM.bIsBridge = bIsBridge;
				SMM.VertexMatchingPrecision = VertexMatchingPrecision;

				SMM.bIsStretch = bIsStretch;
				SMM.bStretchLocOffset = bStretchLocOffset;
				SMM.bStretchSize = bStretchSize;
				SMM.StretchBC_LocOffset = StretchBC_LocOffset;
				SMM.StretchBC_Size = StretchBC_Size;
				SMM.Stretch_UVThreshold = Stretch_UVThreshold;
				SMM.bStraightLineMatchStartEnd = bStraightLineMatchStartEnd;
				SMM.bBCFlipX = bBCFlipX;
				SMM.bBCFlipZ = bBCFlipZ;
				
				//Horizontal offsets:
				SMM.HorizontalSep = HorizontalSep;
				SMM.HorizontalCurve = HorizontalCurve;
				//Vertical offset:
				SMM.VerticalRaise = VerticalRaise;
				SMM.VerticalCurve = VerticalCurve;
				//Vertical cutoff:
				SMM.VerticalCutoff = VerticalCutoff;
				SMM.bVerticalCutoff = bVerticalCutoff;
				SMM.bVerticalCutoffDownwards = bVerticalCutoffDownwards;
				SMM.bVerticalMeshCutoff_OppositeDir = bVerticalMeshCutoff_OppositeDir;
				SMM.VerticalMeshCutoffOffset = VerticalMeshCutoffOffset;
				SMM.bVerticalCutoff_MatchZero = bVerticalCutoff_MatchZero;
				
				SMM.RoadRaise = RoadRaise;
				SMM.CustomRotation = CustomRotation;
				SMM.bFlipRotation = bFlipRotation;
				SMM.bStatic = bStatic;
				SMM.StartTime = StartTime;
				SMM.EndTime = EndTime;
				SMM.Axis = (AxisTypeEnum)Axis;
				
				SMM.RepeatUVType = (RepeatUVTypeEnum)RepeatUVType;
				
				SMM.mMaxX = mMaxX;
				SMM.mMinX = mMinX;
				SMM.mMaxY = mMaxY;
				SMM.mMinY = mMinY;
				SMM.mMaxZ = mMaxZ;
				SMM.mMinZ = mMinZ;
				
				//Ending objects:
				SMM.EndCapStartString = EndCapStartString;
				SMM.EndCapEndString = EndCapEndString;
				if(EndCapStartString != null && EndCapStartString.Length > 0){
					SMM.EndCapStart = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(EndCapStartString,typeof(GameObject));
				}
				if(EndCapEndString != null && EndCapEndString.Length > 0){
					SMM.EndCapEnd = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(EndCapEndString,typeof(GameObject));
				}
				SMM.bEndCapCustomMatchStart = bEndCapCustomMatchStart;
				SMM.EndCapCustomOffsetStart = EndCapCustomOffsetStart;
				SMM.EndCapCustomOffsetEnd = EndCapCustomOffsetEnd;
				SMM.EndCapCustomRotOffsetStart = EndCapCustomRotOffsetStart;
				SMM.EndCapCustomRotOffsetEnd = EndCapCustomRotOffsetEnd;
				SMM.bEndObjectsMatchGround = bEndObjectsMatchGround;

				//Endings down:
				SMM.bStartDown = bStartDown;
				SMM.bStartTypeDownOverride = bStartTypeDownOverride;
				SMM.StartTypeDownOverride = StartTypeDownOverride;
				SMM.bEndDown = bEndDown;
				SMM.bEndTypeDownOverride = bEndTypeDownOverride;
				SMM.EndTypeDownOverride = EndTypeDownOverride;
				
				//Collision:
				SMM.CollisionType = (CollisionTypeEnum)CollisionType;
				SMM.bCollisionConvex = bCollisionConvex;
				SMM.bSimpleCollisionAutomatic = bSimpleCollisionAutomatic;
				SMM.bCollisionTrigger = bCollisionTrigger;
				
				SMM.CollisionBoxBL = CollisionBoxBL;
				SMM.CollisionBoxBR = CollisionBoxBR;
				SMM.CollisionBoxTL = CollisionBoxTL;
				SMM.CollisionBoxTR = CollisionBoxTR;
		
				SMM.CollisionTriBL = CollisionTriBL;
				SMM.CollisionTriBR = CollisionTriBR;
				SMM.CollisionTriT = CollisionTriT;
				
				SMM.tName = tName;
				SMM.ThumbString = ThumbString;
				SMM.Desc = Desc;
				SMM.DisplayName = DisplayName;
				#endif
			}
		}
	
	
		public class SplinatedMeshEditorMaker{
			public GameObject CurrentSplination = null;
			public GameObject CurrentSplinationCap1 = null;
			public GameObject CurrentSplinationCap2 = null;

			public bool bMaterialOverride = false;
			public Material SplinatedMaterial1 = null;
			public Material SplinatedMaterial2 = null;
			public float CapHeightOffset1 = 0f;
			public float CapHeightOffset2 = 0f;
			public bool bExactSplination = false;
			public bool bMatchRoadDefinition = false;
			public bool bMatchRoadIncrements = true;
			public bool bTrimStart = false;
			public bool bTrimEnd = false;
			public bool bToggle = false;
			public bool bMatchTerrain = false;
			public float MinMaxMod = 0.002f;
			public float VertexMatchingPrecision = 0.005f;
			
			public bool bIsStretch = false;
			public bool bStretchLocOffset = false;
			public bool bStretchSize = false;
			public Vector3 StretchBC_LocOffset = default(Vector3);
			public Vector3 StretchBC_Size = default(Vector3);
			public float Stretch_UVThreshold = 0.05f;
			public bool bStraightLineMatchStartEnd = false;
			public bool bBCFlipX = false;
			public bool bBCFlipZ = false;
			
			//Horizontal offsets:
			public float HorizontalSep = 5f;
			public AnimationCurve HorizontalCurve;
			//Vertical offsets:
			public float VerticalRaise = 0f;
			public AnimationCurve VerticalCurve;
			//Vertical cutoff:
			public float VerticalCutoff = 0f;
			public bool bVerticalCutoff = false;
			public bool bVerticalCutoffDownwards = false;
			public bool bVerticalMeshCutoff_OppositeDir = false;
			public float VerticalMeshCutoffOffset = 0.04f;
			public bool bVerticalCutoff_MatchZero = false;

			public float RoadRaise = 0f;
			public Vector3 CustomRotation = default(Vector3);
			public bool bFlipRotation = false;
			public bool bStatic = true;
			public float StartTime = 0f;
			public float EndTime = 1f;
			public AxisTypeEnum Axis = AxisTypeEnum.X;
			public bool bIsBridge = false;
			
			public RepeatUVTypeEnum RepeatUVType = RepeatUVTypeEnum.None;
			public bool bNoCenterMode = true;
			
			//End objects:
			public GameObject EndCapStart = null;
			public GameObject EndCapEnd = null;
			public bool bEndCapCustomMatchStart = true;
			public Vector3 EndCapCustomOffsetStart = default(Vector3);
			public Vector3 EndCapCustomOffsetEnd = default(Vector3);
			public Vector3 EndCapCustomRotOffsetStart = default(Vector3);
			public Vector3 EndCapCustomRotOffsetEnd = default(Vector3);
			public bool bEndObjectsMatchGround = false;
			//Endings down:
			public bool bStartDown = false;
			public bool bStartTypeDownOverride = false;
			public float StartTypeDownOverride = 0f;
			public bool bEndDown = false;
			public bool bEndTypeDownOverride = false;
			public float EndTypeDownOverride = 0f;
			
			//Collision:
			public CollisionTypeEnum CollisionType = CollisionTypeEnum.SimpleMeshTriangle;
			public bool bCollisionConvex = false;
			public bool bSimpleCollisionAutomatic = true;
			public bool bCollisionTrigger = false;
			
			public Vector3 CollisionBoxBL = default(Vector3);
			public Vector3 CollisionBoxBR = default(Vector3);
			public Vector3 CollisionBoxTL = default(Vector3);
			public Vector3 CollisionBoxTR = default(Vector3);
	
			public Vector3 CollisionTriBL = default(Vector3);
			public Vector3 CollisionTriBR = default(Vector3);
			public Vector3 CollisionTriT = default(Vector3);
			
			public string tName = "ExtrudedObject";
			
			public void Setup(SplinatedMeshMaker SMM){
				CurrentSplination = SMM.CurrentSplination;
				CurrentSplinationCap1 = SMM.CurrentSplinationCap1;
				CurrentSplinationCap2 = SMM.CurrentSplinationCap2;
				
				CapHeightOffset1 = SMM.CapHeightOffset1;
				CapHeightOffset2 = SMM.CapHeightOffset2;

				bMaterialOverride = SMM.bMaterialOverride;
				SplinatedMaterial1 = SMM.SplinatedMaterial1;
				SplinatedMaterial2 = SMM.SplinatedMaterial2;
				bExactSplination = SMM.bExactSplination;
				bMatchRoadDefinition = SMM.bMatchRoadDefinition;
				bMatchRoadIncrements = SMM.bMatchRoadIncrements;
				bTrimStart = SMM.bTrimStart;
				bTrimEnd = SMM.bTrimEnd;
				bMatchTerrain = SMM.bMatchTerrain;
				MinMaxMod = SMM.MinMaxMod;
				bIsBridge = SMM.bIsBridge;
				VertexMatchingPrecision = SMM.VertexMatchingPrecision;
				
				bIsStretch = SMM.bIsStretch;
				bStretchLocOffset = SMM.bStretchLocOffset;
				bStretchSize = SMM.bStretchSize;
				StretchBC_LocOffset = SMM.StretchBC_LocOffset;
				StretchBC_Size = SMM.StretchBC_Size;
				Stretch_UVThreshold = SMM.Stretch_UVThreshold;
				bStraightLineMatchStartEnd = SMM.bStraightLineMatchStartEnd;
				bBCFlipX = SMM.bBCFlipX;
				bBCFlipZ = SMM.bBCFlipZ;

				//Horizontal offsets:
				HorizontalSep = SMM.HorizontalSep;
				HorizontalCurve = SMM.HorizontalCurve;
				//Vertical offset:
				VerticalRaise = SMM.VerticalRaise;
				VerticalCurve = SMM.VerticalCurve;
				//Vertical cutoff
				VerticalCutoff = SMM.VerticalCutoff;
				bVerticalCutoff = SMM.bVerticalCutoff;
				bVerticalCutoffDownwards = SMM.bVerticalCutoffDownwards;
				bVerticalMeshCutoff_OppositeDir = SMM.bVerticalMeshCutoff_OppositeDir;
				VerticalMeshCutoffOffset = SMM.VerticalMeshCutoffOffset;
				bVerticalCutoff_MatchZero = SMM.bVerticalCutoff_MatchZero;
				
				RoadRaise = SMM.RoadRaise;
				CustomRotation = SMM.CustomRotation;
				bFlipRotation = SMM.bFlipRotation;
				bStatic = SMM.bStatic;
				StartTime = SMM.StartTime;
				EndTime = SMM.EndTime;
				Axis = SMM.Axis;
				
				RepeatUVType = SMM.RepeatUVType;
				
				//End objects:
				EndCapStart = SMM.EndCapStart;
				EndCapEnd = SMM.EndCapEnd;
				
				bEndCapCustomMatchStart = SMM.bEndCapCustomMatchStart;
				EndCapCustomOffsetStart = SMM.EndCapCustomOffsetStart;
				EndCapCustomOffsetEnd = SMM.EndCapCustomOffsetEnd;
				EndCapCustomRotOffsetStart = SMM.EndCapCustomRotOffsetStart;
				EndCapCustomRotOffsetEnd = SMM.EndCapCustomRotOffsetEnd;
				bEndObjectsMatchGround = SMM.bEndObjectsMatchGround;
				//Endings down:
				bStartDown = SMM.bStartDown;
				bStartTypeDownOverride = SMM.bStartTypeDownOverride;
				StartTypeDownOverride = SMM.StartTypeDownOverride;
				bEndDown = SMM.bEndDown;
				bEndTypeDownOverride = SMM.bEndTypeDownOverride;
				EndTypeDownOverride = SMM.EndTypeDownOverride;

				//Collision:
				CollisionType = SMM.CollisionType;
				bCollisionConvex = SMM.bCollisionConvex;
				bSimpleCollisionAutomatic = SMM.bSimpleCollisionAutomatic;
				bCollisionTrigger = SMM.bCollisionTrigger;
				
				CollisionBoxBL = SMM.CollisionBoxBL;
				CollisionBoxBR = SMM.CollisionBoxBR;
				CollisionBoxTL = SMM.CollisionBoxTL;
				CollisionBoxTR = SMM.CollisionBoxTR;
		
				CollisionTriBL = SMM.CollisionTriBL;
				CollisionTriBR = SMM.CollisionTriBR;
				CollisionTriT = SMM.CollisionTriT;
				
				tName = SMM.tName;
			}
			
			public void LoadToSMM(SplinatedMeshMaker SMM){
				#if UNITY_EDITOR
				SMM.CurrentSplination = CurrentSplination; // (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(CurrentSplinationString,typeof(GameObject));
				SMM.CurrentSplinationCap1 = CurrentSplinationCap1;// (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(CurrentSplinationCap1String,typeof(GameObject));
				SMM.CurrentSplinationCap2 = CurrentSplinationCap2;// (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(CurrentSplinationCap2String,typeof(GameObject));
			
				SMM.CapHeightOffset1 = CapHeightOffset1;
				SMM.CapHeightOffset2 = CapHeightOffset2;

				SMM.bMaterialOverride = bMaterialOverride;
				
				SMM.SplinatedMaterial1 = SplinatedMaterial1;
				SMM.SplinatedMaterial2 = SplinatedMaterial2;

				SMM.bExactSplination = bExactSplination;
				SMM.bMatchRoadDefinition = bMatchRoadDefinition;
				SMM.bMatchRoadIncrements = bMatchRoadIncrements;
				SMM.bTrimStart = bTrimStart;
				SMM.bTrimEnd = bTrimEnd;
				SMM.bMatchTerrain = bMatchTerrain;
				SMM.MinMaxMod = MinMaxMod;
				SMM.bIsBridge = bIsBridge;
				SMM.VertexMatchingPrecision = VertexMatchingPrecision;

				SMM.bIsStretch = bIsStretch;
				SMM.bStretchLocOffset = bStretchLocOffset;
				SMM.bStretchSize = bStretchSize;
				SMM.StretchBC_LocOffset = StretchBC_LocOffset;
				SMM.StretchBC_Size = StretchBC_Size;
				SMM.Stretch_UVThreshold = Stretch_UVThreshold;
				SMM.bStraightLineMatchStartEnd = bStraightLineMatchStartEnd;
				SMM.bBCFlipX = bBCFlipX;
				SMM.bBCFlipZ = bBCFlipZ;
				
				//Horizontal offsets:
				SMM.HorizontalSep = HorizontalSep;
				SMM.HorizontalCurve = HorizontalCurve;
				//Vertical offset:
				SMM.VerticalRaise = VerticalRaise;
				SMM.VerticalCurve = VerticalCurve;
				//Vertical cutoff:
				SMM.VerticalCutoff = VerticalCutoff;
				SMM.bVerticalCutoff = bVerticalCutoff;
				SMM.bVerticalCutoffDownwards = bVerticalCutoffDownwards;
				SMM.bVerticalMeshCutoff_OppositeDir = bVerticalMeshCutoff_OppositeDir;
				SMM.VerticalMeshCutoffOffset = VerticalMeshCutoffOffset;
				SMM.bVerticalCutoff_MatchZero = bVerticalCutoff_MatchZero;
				
				SMM.RoadRaise = RoadRaise;
				SMM.CustomRotation = CustomRotation;
				SMM.bFlipRotation = bFlipRotation;
				SMM.bStatic = bStatic;
				SMM.StartTime = StartTime;
				SMM.EndTime = EndTime;
				SMM.Axis = Axis;
				
				SMM.RepeatUVType = RepeatUVType;
				
				//Ending objects:
				SMM.EndCapStart = EndCapStart;
				SMM.EndCapEnd = EndCapEnd;

				SMM.bEndCapCustomMatchStart = bEndCapCustomMatchStart;
				SMM.EndCapCustomOffsetStart = EndCapCustomOffsetStart;
				SMM.EndCapCustomOffsetEnd = EndCapCustomOffsetEnd;
				SMM.EndCapCustomRotOffsetStart = EndCapCustomRotOffsetStart;
				SMM.EndCapCustomRotOffsetEnd = EndCapCustomRotOffsetEnd;
				SMM.bEndObjectsMatchGround = bEndObjectsMatchGround;

				//Endings down:
				SMM.bStartDown = bStartDown;
				SMM.bStartTypeDownOverride = bStartTypeDownOverride;
				SMM.StartTypeDownOverride = StartTypeDownOverride;
				SMM.bEndDown = bEndDown;
				SMM.bEndTypeDownOverride = bEndTypeDownOverride;
				SMM.EndTypeDownOverride = EndTypeDownOverride;
				
				//Collision:
				SMM.CollisionType = CollisionType;
				SMM.bCollisionConvex = bCollisionConvex;
				SMM.bSimpleCollisionAutomatic = bSimpleCollisionAutomatic;
				SMM.bCollisionTrigger = bCollisionTrigger;
				
				SMM.CollisionBoxBL = CollisionBoxBL;
				SMM.CollisionBoxBR = CollisionBoxBR;
				SMM.CollisionBoxTL = CollisionBoxTL;
				SMM.CollisionBoxTR = CollisionBoxTR;
		
				SMM.CollisionTriBL = CollisionTriBL;
				SMM.CollisionTriBR = CollisionTriBR;
				SMM.CollisionTriT = CollisionTriT;
				
				SMM.tName = tName;
				#endif
			}
			
			public bool IsEqualToSMM(SplinatedMeshMaker SMM){
				if(SMM.CurrentSplination != CurrentSplination){ return false; } 
				if(SMM.CurrentSplinationCap1 != CurrentSplinationCap1){ return false; }
				if(SMM.CurrentSplinationCap2 != CurrentSplinationCap2){ return false; }
			
				if(!GSDRootUtil.IsApproximately(SMM.CapHeightOffset1,CapHeightOffset1,0.0001f)){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.CapHeightOffset2,CapHeightOffset2,0.0001f)){ return false; }

				if(SMM.bMaterialOverride != bMaterialOverride){ return false; }
				
				if(SMM.SplinatedMaterial1 != SplinatedMaterial1){ return false; }
				if(SMM.SplinatedMaterial2 != SplinatedMaterial2){ return false; }

				if(SMM.bExactSplination != bExactSplination){ return false; }
				if(SMM.bMatchRoadDefinition != bMatchRoadDefinition){ return false; }
				if(SMM.bMatchRoadIncrements != bMatchRoadIncrements){ return false; }
				if(SMM.bTrimStart != bTrimStart){ return false; }
				if(SMM.bTrimEnd != bTrimEnd){ return false; }
				if(SMM.bMatchTerrain != bMatchTerrain){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.MinMaxMod,MinMaxMod,0.0001f)){ return false; }
				if(SMM.bIsBridge != bIsBridge){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.VertexMatchingPrecision,VertexMatchingPrecision,0.0001f)){ return false; }

				if(SMM.bIsStretch != bIsStretch){ return false; }
				if(SMM.bStretchLocOffset != bStretchLocOffset){ return false; }
				if(SMM.bStretchSize != bStretchSize){ return false; }
				if(SMM.StretchBC_LocOffset != StretchBC_LocOffset){ return false; }
				if(SMM.StretchBC_Size != StretchBC_Size){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.Stretch_UVThreshold,Stretch_UVThreshold,0.0001f)){ return false; }
				if(SMM.bStraightLineMatchStartEnd != bStraightLineMatchStartEnd){ return false; }
				if(SMM.bBCFlipX != bBCFlipX){ return false; }
				if(SMM.bBCFlipZ != bBCFlipZ){ return false; }
				
				//Horizontal offsets:
				if(!GSDRootUtil.IsApproximately(SMM.HorizontalSep,HorizontalSep,0.0001f)){ return false; }
				if(SMM.HorizontalCurve != HorizontalCurve){ return false; }
				//Vertical offset:
				if(!GSDRootUtil.IsApproximately(SMM.VerticalRaise,VerticalRaise,0.0001f)){ return false; }
				if(SMM.VerticalCurve != VerticalCurve){ return false; }
				//Vertical cutoff:
				if(!GSDRootUtil.IsApproximately(SMM.VerticalCutoff,VerticalCutoff,0.0001f)){ return false; }
				if(SMM.bVerticalCutoff != bVerticalCutoff){ return false; }
				if(SMM.bVerticalCutoffDownwards != bVerticalCutoffDownwards){ return false; }
				if(SMM.bVerticalMeshCutoff_OppositeDir != bVerticalMeshCutoff_OppositeDir){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.VerticalMeshCutoffOffset,VerticalMeshCutoffOffset,0.0001f)){ return false; }
				if(SMM.bVerticalCutoff_MatchZero != bVerticalCutoff_MatchZero){ return false; }
				
				if(!GSDRootUtil.IsApproximately(SMM.RoadRaise,RoadRaise,0.0001f)){ return false; }
				if(SMM.CustomRotation != CustomRotation){ return false; }
				if(SMM.bFlipRotation != bFlipRotation){ return false; }
				if(SMM.bStatic != bStatic){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.StartTime,StartTime,0.0001f)){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.EndTime,EndTime,0.0001f)){ return false; }
				if(SMM.Axis != Axis){ return false; }
				
				if(SMM.RepeatUVType != RepeatUVType){ return false; }

				//Ending objects:
				if(SMM.EndCapStart != EndCapStart){ return false; }
				if(SMM.EndCapEnd != EndCapEnd){ return false; }

				if(SMM.bEndCapCustomMatchStart != bEndCapCustomMatchStart){ return false; }
				if(SMM.EndCapCustomOffsetStart != EndCapCustomOffsetStart){ return false; }
				if(SMM.EndCapCustomOffsetEnd != EndCapCustomOffsetEnd){ return false; }
				if(SMM.EndCapCustomRotOffsetStart != EndCapCustomRotOffsetStart){ return false; }
				if(SMM.EndCapCustomRotOffsetEnd != EndCapCustomRotOffsetEnd){ return false; }
				if(SMM.bEndObjectsMatchGround != bEndObjectsMatchGround){ return false; }

				//Endings down:
				if(SMM.bStartDown != bStartDown){ return false; }
				if(SMM.bStartTypeDownOverride != bStartTypeDownOverride){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.StartTypeDownOverride,StartTypeDownOverride,0.0001f)){ return false; }
				if(SMM.bEndDown != bEndDown){ return false; }
				if(SMM.bEndTypeDownOverride != bEndTypeDownOverride){ return false; }
				if(!GSDRootUtil.IsApproximately(SMM.EndTypeDownOverride,EndTypeDownOverride,0.0001f)){ return false; }
				
				//Collision:
				if(SMM.CollisionType != CollisionType){ return false; }
				if(SMM.bCollisionConvex != bCollisionConvex){ return false; }
				if(SMM.bSimpleCollisionAutomatic != bSimpleCollisionAutomatic){ return false; }
				if(SMM.bCollisionTrigger != bCollisionTrigger){ return false; }
				
				if(SMM.CollisionBoxBL != CollisionBoxBL){ return false; }
				if(SMM.CollisionBoxBR != CollisionBoxBR){ return false; }
				if(SMM.CollisionBoxTL != CollisionBoxTL){ return false; }
				if(SMM.CollisionBoxTR != CollisionBoxTR){ return false; }
		
				if(SMM.CollisionTriBL != CollisionTriBL){ return false; }
				if(SMM.CollisionTriBR != CollisionTriBR){ return false; }
				if(SMM.CollisionTriT != CollisionTriT){ return false; }
				
				if(string.CompareOrdinal(SMM.tName,tName) != 0){ return false; }
				
				return true;
			}
		}
		
		
		#region "Static util"
		public void SetupUniqueIdentifier(){
			if(UID == null || UID.Length < 4){
				UID = System.Guid.NewGuid().ToString();
			}
		}
		
		public static Vector3 GetVector3Average(Vector3[] tVects){
			int tCount = tVects.Length;
			Vector3 mVect = default(Vector3);
			for(int i=0;i<tCount;i++){
				mVect += tVects[i];
			}
			mVect /= tCount;
			return mVect;
		}
		
		private static bool FloatsNear(float tNear, float tVal1,float tVal2){
			if(GSDRootUtil.IsApproximately(tVal1,tVal2,tNear)){ return true; }
			
			if(tVal1 < (tVal2 + tNear) && tVal1 > (tVal2 - tNear)){
				return true;	
			}
			if(tVal2 < (tVal1 + tNear) && tVal2 > (tVal1 - tNear)){
				return true;	
			}
			return false;
		}
	
		private static int[] GetCollisionTris_Tri(int MeshCount, int cTriCount, int cCount){
			int tCounter = 0;
			int[] tTris = new int[cTriCount*3];
			
			//Front side: **
			tTris[tCounter] = 0; tCounter+=1;
			tTris[tCounter] = 2; tCounter+=1;
			tTris[tCounter] = 1; tCounter+=1;
			int tMod = -1;
			for(int i=0;i<(MeshCount);i++){
				tMod = (i*3);
				//Bottom side: ***
				tTris[tCounter] = 1+tMod; tCounter+=1;
				tTris[tCounter] = 4+tMod; tCounter+=1;
				tTris[tCounter] = 0+tMod; tCounter+=1;
				tTris[tCounter] = 4+tMod; tCounter+=1;
				tTris[tCounter] = 3+tMod; tCounter+=1;
				tTris[tCounter] = 0+tMod; tCounter+=1;
				//Left side: ***
				tTris[tCounter] = 3+tMod; tCounter+=1;
				tTris[tCounter] = 5+tMod; tCounter+=1;
				tTris[tCounter] = 0+tMod; tCounter+=1;
				tTris[tCounter] = 5+tMod; tCounter+=1;
				tTris[tCounter] = 2+tMod; tCounter+=1;
				tTris[tCounter] = 0+tMod; tCounter+=1;
				//Right side: ***
				tTris[tCounter] = 1+tMod; tCounter+=1;
				tTris[tCounter] = 2+tMod; tCounter+=1;
				tTris[tCounter] = 4+tMod; tCounter+=1;
				tTris[tCounter] = 2+tMod; tCounter+=1;
				tTris[tCounter] = 5+tMod; tCounter+=1;
				tTris[tCounter] = 4+tMod; tCounter+=1;
			}
			//Back side: **
			tTris[tCounter] = cCount-2; tCounter+=1;
			tTris[tCounter] = cCount-1; tCounter+=1;
			tTris[tCounter] = cCount-3; tCounter+=1;
			
			return tTris;
		}
		
		private static int[] GetCollisionTris_Box(int MeshCount, int cTriCount, int cCount){
			int tCounter = 0;
			int[] tTris = new int[cTriCount*3];
			
			//Front side: ***
			tTris[tCounter] = 0; tCounter+=1;
			tTris[tCounter] = 2; tCounter+=1;
			tTris[tCounter] = 1; tCounter+=1;
			tTris[tCounter] = 2; tCounter+=1;
			tTris[tCounter] = 3; tCounter+=1;
			tTris[tCounter] = 1; tCounter+=1;
			
			int tMod = -1;
			for(int i=0;i<(MeshCount);i++){
				tMod = (i*4);
				//Bottom side: ***
				tTris[tCounter] = tMod+1; tCounter+=1;
				tTris[tCounter] = tMod+5; tCounter+=1;
				tTris[tCounter] = tMod+0; tCounter+=1;
				tTris[tCounter] = tMod+5; tCounter+=1;
				tTris[tCounter] = tMod+4; tCounter+=1;
				tTris[tCounter] = tMod+0; tCounter+=1;
				//Top side: ***
				tTris[tCounter] = tMod+2; tCounter+=1;
				tTris[tCounter] = tMod+6; tCounter+=1;
				tTris[tCounter] = tMod+3; tCounter+=1;
				tTris[tCounter] = tMod+6; tCounter+=1;
				tTris[tCounter] = tMod+7; tCounter+=1;
				tTris[tCounter] = tMod+3; tCounter+=1;
				//Left side: ***
				tTris[tCounter] = tMod+4; tCounter+=1;
				tTris[tCounter] = tMod+6; tCounter+=1;
				tTris[tCounter] = tMod+0; tCounter+=1;
				tTris[tCounter] = tMod+6; tCounter+=1;
				tTris[tCounter] = tMod+2; tCounter+=1;
				tTris[tCounter] = tMod+0; tCounter+=1;
				//Right side: ***
				tTris[tCounter] = tMod+1; tCounter+=1;
				tTris[tCounter] = tMod+3; tCounter+=1;
				tTris[tCounter] = tMod+5; tCounter+=1;
				tTris[tCounter] = tMod+3; tCounter+=1;
				tTris[tCounter] = tMod+7; tCounter+=1;
				tTris[tCounter] = tMod+5; tCounter+=1;
			}
			
			//Back side: ***
			tTris[tCounter] = cCount-3; tCounter+=1;
			tTris[tCounter] = cCount-1; tCounter+=1;
			tTris[tCounter] = cCount-4; tCounter+=1;
			tTris[tCounter] = cCount-1; tCounter+=1;
			tTris[tCounter] = cCount-2; tCounter+=1;
			tTris[tCounter] = cCount-4; tCounter+=1;
			
			return tTris;
		}
		
		private static bool IsApproxTwoThirds(ref Vector3 V1, Vector3 V2, float Precision = 0.005f){
			int cCount = 0;
			if(GSDRootUtil.IsApproximately(V1.x,V2.x,Precision)){
				cCount+=1;
			}
			if(GSDRootUtil.IsApproximately(V1.y,V2.y,Precision)){
				cCount+=1;
			}
			if(GSDRootUtil.IsApproximately(V1.z,V2.z,Precision)){
				cCount+=1;
			}
		
			if(cCount == 2){
				return true;
			}else{
				return false;
			}
		}
		
		private static bool IsApproxWithNeg(ref Vector3 V1,ref Vector3 V2){
			int cCount = 0;
			bool bXMatch = false;
			bool bYMatch = false;
			bool bZMatch = false;
			
			if(GSDRootUtil.IsApproximately(V1.x,V2.x,0.02f)){
				cCount+=1;
				bXMatch = true;
			}
			if(GSDRootUtil.IsApproximately(V1.y,V2.y,0.02f)){
				cCount+=1;
				bYMatch = true;
			}
			if(GSDRootUtil.IsApproximately(V1.z,V2.z,0.02f)){
				cCount+=1;
				bZMatch = true;
			}
			
			if(cCount == 2){
				if(!bXMatch && GSDRootUtil.IsApproximately(V1.x,V2.x * -1f,0.02f)){
					return true;
				}else if(!bYMatch && GSDRootUtil.IsApproximately(V1.y,V2.y * -1f,0.02f)){
					return true;
				}else if(!bZMatch && GSDRootUtil.IsApproximately(V1.z,V2.z * -1f,0.02f)){
					return true;
				}else{
					return false;	
				}
			}else{
				return false;	
			}
		}
		
		private static bool V3EqualToNone(Vector3 V1){
			if(!GSDRootUtil.IsApproximately(V1.x,0f,0.0001f)){
				return false;
			}
			if(!GSDRootUtil.IsApproximately(V1.y,0f,0.0001f)){
				return false;
			}
			if(!GSDRootUtil.IsApproximately(V1.z,0f,0.0001f)){
				return false;
			}
			return true;
		}
		
		private static bool V3EqualNormal(Vector3 V1,Vector3 V2){
			if(!GSDRootUtil.IsApproximately(V1.x,V2.x,0.01f)){
				return false;
			}
			if(!GSDRootUtil.IsApproximately(V1.y,V2.y,0.01f)){
				return false;
			}
			if(!GSDRootUtil.IsApproximately(V1.z,V2.z,0.01f)){
				return false;
			}
			return true;
		}
		
		private static bool IsApproxExtruded(ref Vector3 V1,ref Vector3 V2, bool bIsZAxis){
			if(!GSDRootUtil.IsApproximately(V1.y,V2.y,0.02f)){
				return false;	
			}
			
			if(bIsZAxis){
				if(!GSDRootUtil.IsApproximately(V1.x,V2.x,0.02f)){
					return false;	
				}
			}else{
				if(!GSDRootUtil.IsApproximately(V1.z,V2.z,0.02f)){
					return false;	
				}
			}
			
			return true;
		}
		
		private static float GetVHeightAtXY(ref Vector3 tVect1, ref Vector3 tVect2, ref Vector3 tVect3){
			Vector2 tVect2D1 = new Vector2(tVect1.x,tVect1.z);
			Vector2 tVect2D2 = new Vector2(tVect2.x,tVect2.z);
			Vector2 tVect2D3 = new Vector2(tVect3.x,tVect3.z);
			
			float tDist1 = Vector2.Distance(tVect2D1,tVect2D3);
			float tDist2 = Vector2.Distance(tVect2D2,tVect2D3);
			float tDistSum = tDist1+tDist2;
			
			float CloseTo1 = (tDist1/tDistSum);

			Vector3 tVect = ((tVect2-tVect1)*CloseTo1)+tVect1;

			return tVect.y;
		}	
		#endregion	
	
		
		public void Setup(bool bGetStrings = false, bool bCollect = true){
			#if UNITY_EDITOR
			GameObject[] tObj = new GameObject[5];
			try{
				SplinateMesh_Do(bGetStrings, ref tObj,bCollect);
			}catch(System.Exception e){
				if(tObj != null){
					for(int i=0;i<5;i++){
						if(tObj[i] != null){
							Object.DestroyImmediate(tObj[i]);
						}
					}
				}
				throw e;
			}
			#endif
		}
		private void SplinateMesh_Do(bool bGetStrings, ref GameObject[] ErrortObj, bool bCollect){
			#if UNITY_EDITOR
			bNeedsUpdate = false;
			SetupUniqueIdentifier();

			//Buffers:
			Vector3 tVect1 = default(Vector3);
			Vector3 tVect2 = default(Vector3);
			Vector3 tDir = default(Vector3);
			Vector3 xVect = default(Vector3);
//			Vector3 oVect = default(Vector3);
//			Quaternion tRot = default(Quaternion);
			float tFloat1 = default(float);
//			float tFloat2 = default(float);
//			float tFloat3 = default(float);
//			int tCount = -1;

			StartTime = tSpline.GetClosestParam(StartPos);
			EndTime = tSpline.GetClosestParam(EndPos);
			
			if(EndTime < StartTime){
				EndTime = tNode.NextTime;
				EndPos = tSpline.GetSplineValue(EndTime,false);
			}
			if(EndTime > 0.99995f){
				EndTime = 0.99995f;	
				EndPos = tSpline.GetSplineValue(EndTime,false);
			}

			Kill();
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

//			bool bIsLeft = false;
//			if(HorizontalSep < 0f){
//				bIsLeft = true;
//			}

			//Setup strings:
			if(bGetStrings){
				CurrentSplinationString = GSDRootUtil.GetPrefabString(CurrentSplination);
				if(CurrentSplinationCap1 != null){ CurrentSplinationCap1String = GSDRootUtil.GetPrefabString(CurrentSplinationCap1); }
				if(CurrentSplinationCap2 != null){ CurrentSplinationCap2String = GSDRootUtil.GetPrefabString(CurrentSplinationCap2); }
				if(EndCapStart != null){ EndCapStartString = GSDRootUtil.GetPrefabString(EndCapStart); }
				if(EndCapEnd != null){ EndCapEndString = GSDRootUtil.GetPrefabString(EndCapEnd); }
				if(SplinatedMaterial1 != null){ SplinatedMaterial1String = UnityEditor.AssetDatabase.GetAssetPath(SplinatedMaterial1); }
				if(SplinatedMaterial2 != null){ SplinatedMaterial2String = UnityEditor.AssetDatabase.GetAssetPath(SplinatedMaterial2); }
			}
	
			if(CurrentSplination == null){ return; }
			GameObject tObj = (GameObject)GameObject.Instantiate(CurrentSplination);
			ErrortObj[0] = tObj;
			
			GameObject EndCapStartObj = null;
			GameObject EndCapEndObj = null;
			if(EndCapStart != null){
				EndCapStartObj = (GameObject)GameObject.Instantiate(EndCapStart);
				ErrortObj[1] = EndCapStartObj;
			}
			if(EndCapEnd != null){
				EndCapEndObj = 	(GameObject)GameObject.Instantiate(EndCapEnd);
				ErrortObj[2] = EndCapEndObj;
			}

			GameObject Cap1 = null;
			GameObject Cap2 = null;
			if(bFlipRotation){
				if(CurrentSplinationCap2 != null){
					Cap1 = (GameObject)GameObject.Instantiate(CurrentSplinationCap2);
					ErrortObj[3] = Cap1;
				}
				if(CurrentSplinationCap1 != null){
					Cap2 = (GameObject)GameObject.Instantiate(CurrentSplinationCap1);
					ErrortObj[4] = Cap2;	
				}
			}else{
				if(CurrentSplinationCap1 != null){
					Cap1 = (GameObject)GameObject.Instantiate(CurrentSplinationCap1);
					ErrortObj[3] = Cap1;
				}
				if(CurrentSplinationCap2 != null){
					Cap2 = (GameObject)GameObject.Instantiate(CurrentSplinationCap2);
					ErrortObj[4] = Cap2;	
				}
			}
			
			MeshFilter MF = null;
			Mesh CapMesh1 = null;
			Mesh CapMesh2 = null;
			HashSet<int> tCapMatchIndices1 = new HashSet<int>();
			HashSet<int> tCapMatchIndices2 = new HashSet<int>();
			if(Cap1 != null){
				MF = Cap1.GetComponent<MeshFilter>();
				CapMesh1 = MF.sharedMesh;
			}
			if(Cap2 != null){
				MF = Cap2.GetComponent<MeshFilter>();
				CapMesh2 = MF.sharedMesh;
			}
			
			MF = tObj.GetComponent<MeshFilter>();
			tMesh = MF.sharedMesh;
			
//			Quaternion OrigRot = tObj.transform.rotation;
			if(bFlipRotation){
				tVect1 = new Vector3(0f,180f,0f);
				tObj.transform.Rotate(tVect1,Space.World);
				if(Cap1 != null){ Cap1.transform.Rotate(tVect1,Space.World); }
				if(Cap2 != null){ Cap2.transform.Rotate(tVect1,Space.World); }
			}
			tObj.transform.Rotate(CustomRotation,Space.World);
			if(Cap1 != null){ Cap1.transform.Rotate(CustomRotation,Space.World); }
			if(Cap2 != null){ Cap2.transform.Rotate(CustomRotation,Space.World); }
			
			if(tMesh == null){
				GameObject.DestroyImmediate(tObj);
				Debug.LogError ("Mesh was null");
				return;
			}
			
			Vector3[] CapOrigVerts1 = null;
			Vector3[] CapOrigVerts2 = null;
			int CapOrigMVL1 = 0;
			int CapOrigMVL2 = 0;
			if(CapMesh1 != null){
				CapOrigVerts1 = CapMesh1.vertices;
				CapOrigMVL1 = CapOrigVerts1.Length;
			}
			if(CapMesh2 != null){
				CapOrigVerts2 = CapMesh2.vertices;
				CapOrigMVL2 = CapOrigVerts2.Length;
			}
			
			Vector3[] OrigVerts = tMesh.vertices;
			int OrigMVL = OrigVerts.Length;

			//Transform vertices:
			Vector3[] OrigNormals = tMesh.normals;
			bool bCheckingNormal = true;
			for(int i=0;i<OrigMVL;i++){
				OrigVerts[i] = tObj.transform.TransformPoint(OrigVerts[i]);
				if(bCheckingNormal){
					if(!V3EqualToNone(OrigNormals[i])){
						bCheckingNormal = false;
					}
				}
			}
			//If no normals on base mesh, recalc them
			if(bCheckingNormal){
				tMesh.RecalculateNormals();
				OrigNormals = tMesh.normals;
			}
			//Cap mesh:
			Vector3[] CapOrigNormals1 = null;
			Vector3[] CapOrigNormals2 = null;
			int[] CapOrigTris1= null;
			int[] CapOrigTris2 = null;
			Vector2[] CapOrigUV1 = null;
			Vector2[] CapOrigUV2 = null;
			int CapTriCount1 = 0;
			int CapTriCount2 = 0;
			if(CapMesh1 != null){
				for(int i=0;i<CapOrigMVL1;i++){
					CapOrigVerts1[i] = Cap1.transform.TransformPoint(CapOrigVerts1[i]);
				}
				
				float[] oMinMaxX = new float[CapOrigMVL1];
				float[] oMinMaxY = new float[CapOrigMVL1];
				float[] oMinMaxZ = new float[CapOrigMVL1];
				for(int i=0;i<CapOrigMVL1;i++){
					oMinMaxX[i] = CapOrigVerts1[i].x;	
					oMinMaxY[i] = CapOrigVerts1[i].y;
					oMinMaxZ[i] = CapOrigVerts1[i].z;
				}
//				float oMinX = Mathf.Min(oMinMaxX);
				float oMaxX = Mathf.Max(oMinMaxX);
//				float oMinY = Mathf.Min(oMinMaxY);
//				float oMaxY = Mathf.Max(oMinMaxY);
//				float oMinZ = Mathf.Min(oMinMaxZ);
				float oMaxZ = Mathf.Max(oMinMaxZ);
				
				for(int i=0;i<CapOrigMVL1;i++){
					if(Axis == AxisTypeEnum.Z){
						if(GSDRootUtil.IsApproximately(CapOrigVerts1[i].z,oMaxZ,MinMaxMod)){
							tCapMatchIndices1.Add(i);
						}
					}else{
						if(GSDRootUtil.IsApproximately(CapOrigVerts1[i].x,oMaxX,MinMaxMod)){
							tCapMatchIndices1.Add(i);
						}
					}
				}

				CapMesh1.RecalculateNormals();
				CapOrigTris1 = CapMesh1.triangles;
				CapOrigUV1 = CapMesh1.uv;
				CapTriCount1 = CapOrigTris1.Length;
				CapOrigNormals1 = CapMesh1.normals;
			}
			if(CapMesh2 != null){
				for(int i=0;i<CapOrigMVL2;i++){
					CapOrigVerts2[i] = Cap2.transform.TransformPoint(CapOrigVerts2[i]);
				}
				
				float[] oMinMaxX = new float[CapOrigMVL2];
				float[] oMinMaxY = new float[CapOrigMVL2];
				float[] oMinMaxZ = new float[CapOrigMVL2];
				for(int i=0;i<CapOrigMVL2;i++){
					oMinMaxX[i] = CapOrigVerts2[i].x;	
					oMinMaxY[i] = CapOrigVerts2[i].y;
					oMinMaxZ[i] = CapOrigVerts2[i].z;
				}
				float oMinX = Mathf.Min(oMinMaxX);
//				float oMaxX = Mathf.Max(oMinMaxX);
//				float oMinY = Mathf.Min(oMinMaxY);
//				float oMaxY = Mathf.Max(oMinMaxY);
				float oMinZ = Mathf.Min(oMinMaxZ);
//				float oMaxZ = Mathf.Max(oMinMaxZ);
				
				for(int i=0;i<CapOrigMVL2;i++){
					if(Axis == AxisTypeEnum.Z){
						if(GSDRootUtil.IsApproximately(CapOrigVerts2[i].z,oMinZ,MinMaxMod)){
							tCapMatchIndices2.Add(i);
						}
					}else{
						if(GSDRootUtil.IsApproximately(CapOrigVerts2[i].x,oMinX,MinMaxMod)){
							tCapMatchIndices2.Add(i);
						}
					}
				}
				
				CapMesh2.RecalculateNormals();
				CapOrigTris2 = CapMesh2.triangles;
				CapOrigUV2 = CapMesh2.uv;
				CapTriCount2 = CapOrigTris2.Length;
				CapOrigNormals2 = CapMesh2.normals;
			}
			
			int[] OrigTris = tMesh.triangles;
			int OrigTriCount = OrigTris.Length;
			Vector2[] OrigUV = tMesh.uv;
			float[] tMinMax = new float[OrigMVL]; 
			float[] tMinMaxX = new float[OrigMVL];
			float[] tMinMaxY = new float[OrigMVL];
			float[] tMinMaxZ = new float[OrigMVL];
			float[] tMinMaxUV = null;
			if(RepeatUVType != RepeatUVTypeEnum.None){
				tMinMaxUV = new float[OrigMVL];
			}
			for(int i=0;i<OrigMVL;i++){
				if(Axis == AxisTypeEnum.X){
					tMinMax[i] = OrigVerts[i].x;
				}else{
					tMinMax[i] = OrigVerts[i].z;
				}
				tMinMaxX[i] = OrigVerts[i].x;
				tMinMaxY[i] = OrigVerts[i].y;
				tMinMaxZ[i] = OrigVerts[i].z;
				if(RepeatUVType == RepeatUVTypeEnum.X){
					tMinMaxUV[i] = OrigUV[i].x;
				}else if(RepeatUVType == RepeatUVTypeEnum.Y){
					tMinMaxUV[i] = OrigUV[i].y;
				}
			}
			
			float mMax = Mathf.Max(tMinMax);
			float mMin = Mathf.Min(tMinMax);
			float mMaxX = Mathf.Max(tMinMaxX);
			float mMinX = Mathf.Min(tMinMaxX);
			float mMaxY = Mathf.Max(tMinMaxY);
			float mMinY = Mathf.Min(tMinMaxY);
			float mMaxZ = Mathf.Max(tMinMaxZ);
			float mMinZ = Mathf.Min(tMinMaxZ);
			float mMinUV = -1f;
			float mMaxUV = -1f;
			float mUVDiff = -1f;
			if(RepeatUVType != RepeatUVTypeEnum.None){
				mMinUV = Mathf.Min(tMinMaxUV);
				mMaxUV = Mathf.Max(tMinMaxUV);
				mUVDiff = mMaxUV - mMinUV;
			}
			float mMaxDiff = mMax - mMin;
			float mMaxHeight = mMaxY - mMinY;
			float mMaxThreshold = mMax-MinMaxMod;
			float mMinThreshold = mMin+MinMaxMod;
			List<int> MinVectorIndices = new List<int>();
			List<int> MaxVectorIndices = new List<int>();
			List<int> MiddleVectorIndicies = new List<int>();
			float tBuffer = 0f;
			for(int i=0;i<OrigMVL;i++){
				if(Axis == AxisTypeEnum.X){
					tBuffer = OrigVerts[i].x;
				}else{
					tBuffer = OrigVerts[i].z;
				}
	
				if(tBuffer > mMaxThreshold){
					MaxVectorIndices.Add(i);
				}else if(tBuffer < mMinThreshold){
					MinVectorIndices.Add(i);
				}else{
					MiddleVectorIndicies.Add(i);	
				}
			}
			int MiddleCount = MiddleVectorIndicies.Count;

			//Match up min/max vertices:
			Dictionary<int,int> MatchingIndices = new Dictionary<int, int>();
			Dictionary<int,int> MatchingIndices_Min = new Dictionary<int, int>();
			Dictionary<int,List<int>> MatchingIndices_Min_Cap = new Dictionary<int, List<int>>();
			Dictionary<int,List<int>> MatchingIndices_Max_Cap = new Dictionary<int, List<int>>();
			int tCount1 = MaxVectorIndices.Count;
			int tCount2 = MinVectorIndices.Count;
			int tIntBuffer1 = -1;
			int tIntBuffer2 = -1;
			int tIntBuffer3 = -1;
			int tIntBuffer4 = -1;
//			Dictionary<int,float> UVStep = null;
//			if(RepeatUVType != RepeatUVTypeEnum.None){
//				UVStep = new Dictionary<int, float>();
//			}
			List<int> AlreadyAddedList = new List<int>();
			for(int i=0;i<tCount1;i++){
				tIntBuffer1 = MaxVectorIndices[i];
				tVect1 = OrigVerts[tIntBuffer1];

				bool bAdded = false;
				for(int j=0;j<OrigTriCount;j+=3){
					if(OrigTris[j] == tIntBuffer1){
						tIntBuffer3 = OrigTris[j+1];
						tIntBuffer4 = OrigTris[j+2];
					}else if(OrigTris[j+1] == tIntBuffer1){
						tIntBuffer3 = OrigTris[j];
						tIntBuffer4 = OrigTris[j+2];
					}else if(OrigTris[j+2] == tIntBuffer1){
						tIntBuffer3 = OrigTris[j];
						tIntBuffer4 = OrigTris[j+1];
					}else{
						continue;	
					}
					if(MinVectorIndices.Contains(tIntBuffer3)){
						for(int k=0;k<tCount2;k++){
							tIntBuffer2 = MinVectorIndices[k];
							if(tIntBuffer2 == tIntBuffer3){
								if(AlreadyAddedList.Contains(tIntBuffer2)){ break; }
								if(IsApproxTwoThirds(ref tVect1,OrigVerts[tIntBuffer2],VertexMatchingPrecision)){
									MatchingIndices.Add(tIntBuffer1, tIntBuffer2);
									AlreadyAddedList.Add(tIntBuffer2);
									MatchingIndices_Min.Add(tIntBuffer2,tIntBuffer1);
									bAdded = true;
									break;
								}
							}
						}
					}
					if(!bAdded && MinVectorIndices.Contains(tIntBuffer4)){
						for(int k=0;k<tCount2;k++){
							tIntBuffer2 = MinVectorIndices[k];
							if(tIntBuffer2 == tIntBuffer4){
								if(AlreadyAddedList.Contains(tIntBuffer2)){ break; }
								if(IsApproxTwoThirds(ref tVect1,OrigVerts[tIntBuffer2],VertexMatchingPrecision)){
									MatchingIndices.Add(tIntBuffer1, tIntBuffer2);
									AlreadyAddedList.Add(tIntBuffer2);
									MatchingIndices_Min.Add(tIntBuffer2,tIntBuffer1);
									bAdded = true;
									break;
								}
							}
						}
					}
					if(bAdded){ break; }
				}
			}
			
			//Tris don't match, so need further refinement:
			if(MatchingIndices.Count < MaxVectorIndices.Count){
				bool bIsZAxis = (Axis == AxisTypeEnum.Z);
				for(int i=0;i<tCount1;i++){
					tIntBuffer1 = MaxVectorIndices[i];
					if(MatchingIndices.ContainsKey(tIntBuffer1)){ continue; }
					tVect1 = OrigVerts[tIntBuffer1];
					if(Axis == AxisTypeEnum.Z){
						for(int j=0;j<tCount2;j++){
							tIntBuffer2 = MinVectorIndices[j];
							if(!AlreadyAddedList.Contains(tIntBuffer2)){
								tVect2 = OrigVerts[tIntBuffer2];
								if(IsApproxExtruded(ref tVect1,ref tVect2,bIsZAxis) && V3EqualNormal(OrigNormals[tIntBuffer1],OrigNormals[tIntBuffer2])){
									MatchingIndices.Add(tIntBuffer1, tIntBuffer2);
									AlreadyAddedList.Add(tIntBuffer2);
									MatchingIndices_Min.Add(tIntBuffer2,tIntBuffer1);
									break;
								}
							}
						}
					}
				}
			}
			
			//Caps:
			if(CapMesh1 != null){
				bool bDidAdd = false;
				foreach(KeyValuePair<int,int> KVP in MatchingIndices_Min){
					List<int> tList = new List<int>();
					tVect1 = OrigVerts[KVP.Key];
					for(int i=0;i<CapOrigMVL1;i++){
						if(tCapMatchIndices1.Contains(i) && IsApproxTwoThirds(ref tVect1, CapOrigVerts1[i],VertexMatchingPrecision)){
							tList.Add(i);	
							bDidAdd = true;
						}
					}
					MatchingIndices_Min_Cap.Add(KVP.Key,tList);
				}
				if(!bDidAdd){
					try{
						Debug.LogWarning("Start cap error (still processing extrusion, ignoring start cap). No matching vertices found for start cap. Most likely the cap mesh is aligned improperly or along the wrong axis relative to the main mesh.");
					}catch{
						
					}
					if(Cap1 != null){
						Object.DestroyImmediate(Cap1);
					}
					CapMesh1 = null;
					CapOrigMVL1 = 0;
					CapTriCount1 = 0;
				}
			}
			if(CapMesh2 != null){
				bool bDidAdd = false;
				foreach(KeyValuePair<int,int> KVP in MatchingIndices){
					List<int> tList = new List<int>();
					tVect1 = OrigVerts[KVP.Key];
					for(int i=0;i<CapOrigMVL2;i++){
						if(tCapMatchIndices2.Contains(i) && IsApproxTwoThirds(ref tVect1, CapOrigVerts2[i],VertexMatchingPrecision)){
							tList.Add(i);
							bDidAdd = true;
						}
					}
					MatchingIndices_Max_Cap.Add(KVP.Key,tList);
				}
				if(!bDidAdd){
					try{
						Debug.LogError("End cap error (still processing extrusion, ignoring end cap). No matching vertices found for end cap. Most likely the cap mesh is aligned improperly or along the wrong axis relative to the main mesh.");
					}catch{
						
					}
					if(Cap2 != null){ Object.DestroyImmediate(Cap2); }
					CapMesh2 = null;
					CapOrigMVL2 = 0;
					CapTriCount2 = 0;
				}
			}
			
			//Road definition matching:
			if(bMatchRoadDefinition){
				float RoadDefStart = (tSpline.tRoad.opt_RoadDefinition / 2f) * -1;
				float UVChange = tSpline.tRoad.opt_RoadDefinition / mMaxDiff;
				foreach(KeyValuePair<int,int> KVP in MatchingIndices){
					//Vertex change:
					if(Axis == AxisTypeEnum.X){
						OrigVerts[KVP.Value].x = RoadDefStart;
						OrigVerts[KVP.Key].x = (OrigVerts[KVP.Value].x + tSpline.tRoad.opt_RoadDefinition);
					}else if(Axis == AxisTypeEnum.Z){
						OrigVerts[KVP.Value].z = RoadDefStart;
						OrigVerts[KVP.Key].z = (OrigVerts[KVP.Value].z + tSpline.tRoad.opt_RoadDefinition);
					}
					//UV Change:
					if(RepeatUVType == RepeatUVTypeEnum.X){
						OrigUV[KVP.Key].x *= UVChange;
					}else if(RepeatUVType == RepeatUVTypeEnum.Y){
						OrigUV[KVP.Key].y *= UVChange;
					}
				}
				
				//Settings:
				tMinMaxUV = new float[OrigMVL];
				tMinMax = new float[OrigMVL];
				tMinMaxX = new float[OrigMVL];
				tMinMaxY = new float[OrigMVL];
				tMinMaxZ = new float[OrigMVL];
				for(int i=0;i<OrigMVL;i++){
					if(Axis == AxisTypeEnum.X){
						tMinMax[i] = OrigVerts[i].x;
					}else{
						tMinMax[i] = OrigVerts[i].z;
					}
					tMinMaxX[i] = OrigVerts[i].x;
					tMinMaxY[i] = OrigVerts[i].y;
					tMinMaxZ[i] = OrigVerts[i].z;
					if(RepeatUVType == RepeatUVTypeEnum.X){
						tMinMaxUV[i] = OrigUV[i].x;
					}else if(RepeatUVType == RepeatUVTypeEnum.Y){
						tMinMaxUV[i] = OrigUV[i].y;
					}
				}
				//UV Changes:
				mMax = Mathf.Max(tMinMax);
				mMin = Mathf.Min(tMinMax);
				mMaxX = Mathf.Max(tMinMaxX);
				mMinX = Mathf.Min(tMinMaxX);
				mMaxY = Mathf.Max(tMinMaxY);
				mMinY = Mathf.Min(tMinMaxY);
				mMaxZ = Mathf.Max(tMinMaxZ);
				mMinZ = Mathf.Min(tMinMaxZ);
				mMinUV = -1f;
				mMaxUV = -1f;
				mUVDiff = -1f;
				if(RepeatUVType != RepeatUVTypeEnum.None){
					mMinUV = Mathf.Min(tMinMaxUV);
					mMaxUV = Mathf.Max(tMinMaxUV);
					mUVDiff = mMaxUV - mMinUV;
				}
				mMaxDiff = mMax - mMin;
				mMaxHeight = mMaxY - mMinY;
				mMaxThreshold = mMax-MinMaxMod;
				mMinThreshold = mMin+MinMaxMod;
			}
			
			//For vert reverse cut:
			int VertCutTriIndex1 = -1;
			int VertCutTriIndex2 = -1;
			if(bVerticalMeshCutoff_OppositeDir){
				float[] tMatchingMaxY = new float[MatchingIndices.Count];
				int tempcount141 = 0;
				foreach(KeyValuePair<int,int> KVP in MatchingIndices){
					tMatchingMaxY[tempcount141] = OrigVerts[KVP.Key].y;
					tempcount141+=1;
				}
				
				float tMatchingMaxY_f = Mathf.Max(tMatchingMaxY);
				foreach(KeyValuePair<int,int> KVP in MatchingIndices){
					if(GSDRootUtil.IsApproximately(OrigVerts[KVP.Key].y,tMatchingMaxY_f,0.0001f)){
						VertCutTriIndex1 = KVP.Key;
						VertCutTriIndex2 = KVP.Value;
						break;
					}
				}
			}
			

			//Set auto simple collision points: 
			if(bSimpleCollisionAutomatic){
				if(Axis == AxisTypeEnum.X){
					CollisionTriBL = new Vector3(mMinX,mMinY,mMinZ);
					CollisionTriBR = new Vector3(mMinX,mMinY,mMaxZ);
					CollisionTriT = new Vector3(mMinX,mMaxY,((mMaxZ-mMinZ)*0.5f)+mMinZ);
				}else if(Axis == AxisTypeEnum.Z){
					CollisionTriBL = new Vector3(mMinX,mMinY,mMinZ);
					CollisionTriBR = new Vector3(mMaxX,mMinY,mMinZ);
					CollisionTriT = new Vector3(((mMaxX-mMinX)*0.5f)+mMinX,mMaxY,mMinZ);
				}
				
				if(Axis == AxisTypeEnum.X){
					CollisionBoxBL = new Vector3(mMinX,mMinY,mMinZ);
					CollisionBoxBR = new Vector3(mMinX,mMinY,mMaxZ);
					CollisionBoxTL = new Vector3(mMinX,mMaxY,mMinZ);
					CollisionBoxTR = new Vector3(mMinX,mMaxY,mMaxZ);
				}else if(Axis == AxisTypeEnum.Z){
					CollisionBoxBL = new Vector3(mMinX,mMinY,mMinZ);
					CollisionBoxBR = new Vector3(mMaxX,mMinY,mMinZ);
					CollisionBoxTL = new Vector3(mMinX,mMaxY,mMinZ);
					CollisionBoxTR = new Vector3(mMaxX,mMaxY,mMinZ);
				}
			}
			
			Vector3[] tVerts = null;
			Vector2[] tUV = null;

			//Get the vector series that this mesh is interpolated on:
			List<float> tTimes = new List<float>();
			float cTime = StartTime;
			
			
			tTimes.Add(cTime);
			int SpamGuard = 5000;
			int SpamGuardCounter = 0;
			float pDiffTime = EndTime - StartTime;
			float CurrentH = 0f;
			float fHeight = 0f;
//			Vector3 tStartPos = tSpline.GetSplineValue(StartTime);
//			Vector3 tEndPos = tSpline.GetSplineValue(EndTime);
			
			while(cTime < EndTime && SpamGuardCounter < SpamGuard){
				tSpline.GetSplineValue_Both(cTime,out tVect1,out tDir);
				fHeight = HorizontalCurve.Evaluate((cTime-StartTime) / pDiffTime);
				CurrentH = fHeight * HorizontalSep;
	
				if(CurrentH < 0f){
					CurrentH *= -1f;
					tVect1 = (tVect1 + new Vector3(CurrentH*-tDir.normalized.z,0,CurrentH*tDir.normalized.x));
				}else if(CurrentH > 0f){
					tVect1 = (tVect1 + new Vector3(CurrentH*tDir.normalized.z,0,CurrentH*-tDir.normalized.x));
				}
				
				xVect = (tDir.normalized * mMaxDiff) + tVect1;
				
				cTime = tSpline.GetClosestParam(xVect,false,false);
				if(cTime > EndTime){
					cTime = EndTime;
				}
				tTimes.Add(cTime);
				SpamGuardCounter+=1;
			}
			if(bTrimStart){
				tTimes.RemoveAt(0);	
			}else if(bTrimEnd){
				tTimes.RemoveAt(tTimes.Count-1);
			}
			int vSeriesCount = tTimes.Count;
			
			//Dynamic vertical and horiz:
			List<float> DynamicVerticalRaise = null;
			List<float> DynamicHoriz = null;
			DynamicVerticalRaise = new List<float>();
			DynamicHoriz = new List<float>();
			float tStartTime = tTimes[0];
			float tEndTime = tTimes[vSeriesCount-1];
//			float tDiffTime = tEndTime - tStartTime;
//			float cDiff = 0f;
			
			float jDistance = 0f;
			float jStartDistance = tSpline.TranslateParamToDist(tStartTime);
			float jEndDistance = tSpline.TranslateParamToDist(tEndTime);
			float jDistanceDiff = jEndDistance - jStartDistance;
//			float jLastTime = 0f;
			float jCurrTime = 0f;
//			float jStep = 0.02f / tSpline.distance;
//			Vector3 jVect1 = default(Vector3);
//			Vector3 jVect2 = default(Vector3);
//			float prevFHeight = 0f;
//			bool basfsafa = false;
			for(int i=0;i<vSeriesCount;i++){
//				cDiff = tTimes[i] - tStartTime;
//				cDiff = cDiff / tDiffTime;
				
				//Vertical curve:
				if(VerticalCurve.keys == null || VerticalCurve.length < 1){
					fHeight = 1f;
				}else{
					jDistance = tSpline.TranslateParamToDist(tTimes[i]);
					jCurrTime = (jDistance - jStartDistance) / jDistanceDiff;
					fHeight = VerticalCurve.Evaluate(jCurrTime);
				}
				DynamicVerticalRaise.Add(fHeight);
				
				//Horizontal curve:
				if(HorizontalCurve.keys == null || HorizontalCurve.length < 1){
					fHeight = 1f;
				}else{
					fHeight = HorizontalCurve.Evaluate(jCurrTime);
				}
				DynamicHoriz.Add(fHeight);
			}
			
			Vector3[] VectorSeries = new Vector3[vSeriesCount];
			Vector3[] VectorSeriesTangents = new Vector3[vSeriesCount];
//			bool bIsCenter = GSDRootUtil.IsApproximately(HorizontalSep,0f,0.02f);
			float tIntStrength = 0f;
			float tIntHeight = 0f;
			GSDRoadIntersection GSDRI = null;
			bool bIsPastInter = false;
			GSDSplineN xNode = null;
			List<float> tOrigHeights = new List<float>();
			
//			List<Terrain> xTerrains = null;
//			List<GSD.Roads.GSDRoadUtil.Construction2DRect> tTerrainRects = null;
//			int TerrainCount = 0;
//			if(bMatchTerrain){
//				tTerrainRects = new List<GSD.Roads.GSDRoadUtil.Construction2DRect>();
//				xTerrains = new List<Terrain>();
//				Object[] tTerrains = GameObject.FindObjectsOfType(typeof(Terrain));
//				GSD.Roads.GSDRoadUtil.Construction2DRect tTerrainRect = null;
//				Vector2 tPos2D = default(Vector2);
//				Vector2 P1,P2,P3,P4;
//				foreach(Terrain xTerrain in tTerrains){
//					tPos2D = new Vector2(xTerrain.transform.position.x,xTerrain.transform.position.z);
//					P1 = new Vector2(0f,0f) + tPos2D;
//					P2 = new Vector2(0f,xTerrain.terrainData.size.y) + tPos2D;
//					P3 = new Vector2(xTerrain.terrainData.size.x,xTerrain.terrainData.size.y) + tPos2D;
//					P4 = new Vector2(xTerrain.terrainData.size.x,0f) + tPos2D;
//					tTerrainRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(P1,P2,P3,P4,xTerrain.transform.position.y);
//					tTerrainRects.Add(tTerrainRect);
//					xTerrains.Add(xTerrain);
//					TerrainCount+=1;
//				}
//			}
			
//			Vector2 temp2DVect = default(Vector2);
			Ray tRay = default(Ray);
			RaycastHit[] tRayHit = null;
			float[] tRayYs = null;
			for(int i=0;i<vSeriesCount;i++){
				cTime = tTimes[i];
				tSpline.GetSplineValue_Both(cTime,out tVect1,out tVect2);
				tOrigHeights.Add(tVect1.y);
				
				//Horizontal offset:
				CurrentH = DynamicHoriz[i] * HorizontalSep;
				
				if(CurrentH < 0f){
					CurrentH *= -1f;
					tVect1 = (tVect1 + new Vector3(CurrentH*-tVect2.normalized.z,0,CurrentH*tVect2.normalized.x));
				}else if(CurrentH > 0f){
					tVect1 = (tVect1 + new Vector3(CurrentH*tVect2.normalized.z,0,CurrentH*-tVect2.normalized.x));
				}

				tIntStrength = tSpline.IntersectionStrength(ref tVect1,ref tIntHeight, ref GSDRI, ref bIsPastInter, ref cTime, ref xNode);
				
				if(GSDRootUtil.IsApproximately(tIntStrength,1f,0.0001f)){ 
					tVect1.y = tIntHeight;
				}else if(!GSDRootUtil.IsApproximately(tIntStrength,0f,0.001f)){
					tVect1.y = (tIntStrength*tIntHeight) + ((1-tIntStrength)*tVect1.y); 
				}
				
				//Terrain matching:
				if(bMatchTerrain){
//					temp2DVect = new Vector2(tVect1.x,tVect1.z);
//					for(int j=0;j<TerrainCount;j++){
//						if(tTerrainRects[j].Contains(ref temp2DVect)){
//							tVect1.y = xTerrains[j].SampleHeight(tVect1);
//							break;
//						}
//					}
					
		
					tRay = new Ray(tVect1+new Vector3(0f,1f,0f),Vector3.down);
					tRayHit = Physics.RaycastAll(tRay);
					if(tRayHit.Length > 0){
						tRayYs = new float[tRayHit.Length];
						for(int g=0;g<tRayHit.Length;g++){
							tRayYs[g] = tRayHit[g].point.y;	
						}
						tVect1.y = Mathf.Max(tRayYs);
					}
				}
				
				tVect1.y += (DynamicVerticalRaise[i]*VerticalRaise);

				VectorSeries[i] = tVect1;
				VectorSeriesTangents[i] = tVect2;
			}
			int MeshCount = (vSeriesCount-1);
			
//			float yDiff = 0f;
//			float tDistance = 0f;
			int MVL = MeshCount * OrigMVL;
			#if UNITY_2017_3_OR_NEWER
			if(MVL > 4000000){
				throw new System.Exception("Over 4000000 vertices detected, exiting extrusion. Try switching splination axis and make sure your imported FBX file has proper import scale. Make sure the mesh isn't too small and make sure the distance isn't too large.");
			}
			#else
			if(MVL > 64900){
				throw new System.Exception("Over 65000 vertices detected, exiting extrusion. Try switching splination axis and make sure your imported FBX file has proper import scale. Make sure the mesh isn't too small and make sure the distance isn't too large.");
			}
			#endif
			int MaxCount = MaxVectorIndices.Count;
			int MinCount = MinVectorIndices.Count;
			int TriCount = MeshCount * OrigTriCount;
//			int MatchCount = MatchingIndices.Count;
			tVerts = new Vector3[MVL];
			tUV = new Vector2[MVL];
			int[] tTris = new int[TriCount];
			Vector3[] tNormals = new Vector3[MVL];
			int vManuver = 0;
			int vManuver_Prev = 0;
			int TriManuver = 0;
			Vector3[] cVerts = null;
			int[] cTris = null;
			int cCount = -1;
			int cTriCount = -1;
			bool bSimpleCollisionOn = false;
			float tOrigHeightBuffer = 0f;
			float tFloat5 = 0f;
			if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
				cVerts = new Vector3[3*(MeshCount+1)];
				cCount = cVerts.Length;
				cTriCount = (6*cCount)+2;
				bSimpleCollisionOn = true;
			}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
				cVerts = new Vector3[4*(MeshCount+1)];
				cCount = cVerts.Length;
				cTriCount = (8*cCount)+4;
				bSimpleCollisionOn = true;
			}
			
//			List<GSD.Roads.GSDRoadUtil.Construction3DTri> tTriList = null;
//			GSD.Roads.GSDRoadUtil.Construction3DTri VertOppCutTri = null;
//			int VertCutBufferIndex1 = -1;
//			int VertCutBufferIndex2 = -1;
			Vector3 VertCutBuffer1 = default(Vector3);
			Vector3 VertCutBuffer2 = default(Vector3);
			Vector3 VertCutBuffer3 = default(Vector3);
			float tOrigHeightBuffer_Orig = 0f;
//			if(bVerticalMeshCutoff_OppositeDir){
//				tTriList = new List<GSD.Roads.GSDRoadUtil.Construction3DTri>();
//			}
			
			if(bIsStretch){
				DoStretch(ref OrigVerts, ref OrigUV, ref OrigTris,ref MaxVectorIndices,ref MinVectorIndices, mMaxDiff,out tVerts,out tUV, out tNormals, out tTris);
				goto StretchSkip;
			}
			
			//Main loop:
			Matrix4x4 tMatrix = new Matrix4x4();
			for(int j=0;j<MeshCount;j++){
				TriManuver = j*OrigTriCount;
				vManuver = j*OrigMVL;
				vManuver_Prev = (j-1)*OrigMVL;

				if(!bIsStretch){
					tVect1 = VectorSeries[j];
					tVect2 = VectorSeries[j+1];	
				}
				
//				yDiff = tVect2.y - tVect1.y;
//				tDistance = Vector3.Distance(tVect1,tVect2);

//				if(j==0){ tStartPos = tVect1; }
//				if(j==(MeshCount-1)){ tEndPos = tVect1; }
				
				if(bExactSplination && MiddleCount < 2){
					tDir = (tVect2 - tVect1).normalized;
				}else{
					tDir = VectorSeriesTangents[j].normalized;
				}
				
				tOrigHeightBuffer = tOrigHeights[j]+VerticalCutoff;
				tOrigHeightBuffer_Orig = tOrigHeights[j];
				tMatrix.SetTRS(tVect1,Quaternion.LookRotation(tDir),new Vector3(1f,1f,1f));
				
				//Rotate and set vertex positions:
				for(int i=0;i<OrigMVL;i++){
					xVect = OrigVerts[i];
					tVerts[vManuver+i] = tMatrix.MultiplyPoint3x4(xVect);
//					tVerts[vManuver+i] = (Quaternion.LookRotation(tDir)*xVect) + tVect1;

					//UV:
					tUV[vManuver+i] = OrigUV[i];
					
					//Vertical cutoff:
					if(bVerticalCutoff){
						if(MiddleVectorIndicies.Contains(i)){
							tFloat5 = tVerts[vManuver+i].y;
							if(bVerticalCutoffDownwards){
								if(bVerticalCutoff_MatchZero){
									if(tFloat5 < tOrigHeightBuffer_Orig){
										tVerts[vManuver+i].y = tOrigHeightBuffer_Orig;
									}
								}else{
									if(tFloat5 < tOrigHeightBuffer){
										tVerts[vManuver+i].y = tOrigHeightBuffer;
									}
								}
								
								tFloat1 = (tOrigHeightBuffer_Orig-tOrigHeightBuffer)/mMaxHeight;
								tUV[vManuver+i].x *= tFloat1;
								
							}else{
								if(bVerticalCutoff_MatchZero){
									if(tFloat5 > tOrigHeightBuffer_Orig){
										tVerts[vManuver+i].y = tOrigHeightBuffer_Orig;
									}
								}else{
									if(tFloat5 > tOrigHeightBuffer){
										tVerts[vManuver+i].y = tOrigHeightBuffer;
									}
								}
								
								tFloat1 = (tOrigHeightBuffer-tOrigHeightBuffer_Orig)/mMaxHeight;
								tUV[vManuver+i].x *= tFloat1;
							}
						}
					}
				}

				if(RepeatUVType != RepeatUVTypeEnum.None){
					for(int i=0;i<MaxCount;i++){
						tIntBuffer1 = MaxVectorIndices[i];
						if(RepeatUVType == RepeatUVTypeEnum.X){
							tUV[vManuver+tIntBuffer1].x = mUVDiff*(j+1);
						}else{
							tUV[vManuver+tIntBuffer1].y = mUVDiff*(j+1);	
						}
					}
					for(int i=0;i<MinCount;i++){
						tIntBuffer1 = MinVectorIndices[i];
						if(RepeatUVType == RepeatUVTypeEnum.X){
							tUV[vManuver+tIntBuffer1].x = mUVDiff*j;
						}else{
							tUV[vManuver+tIntBuffer1].y = mUVDiff*j;
						}
					}
				}
				
				//Simple collision (triangle or trap):
				if(bSimpleCollisionOn){
					if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
						cVerts[0+(j*3)] = tMatrix.MultiplyPoint3x4(CollisionTriBL);
						cVerts[1+(j*3)] = tMatrix.MultiplyPoint3x4(CollisionTriBR);
						cVerts[2+(j*3)] = tMatrix.MultiplyPoint3x4(CollisionTriT);
						
//						cVerts[0+(j*3)] = (Quaternion.LookRotation(tDir)*CollisionTriBL) + tVect1;
//						cVerts[1+(j*3)] = (Quaternion.LookRotation(tDir)*CollisionTriBR) + tVect1;
//						cVerts[2+(j*3)] = (Quaternion.LookRotation(tDir)*CollisionTriT) + tVect1;
					}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
						cVerts[0+(j*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxBL);
						cVerts[1+(j*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxBR);
						cVerts[2+(j*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxTL);
						cVerts[3+(j*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxTR);
						
//						cVerts[0+(j*4)] = (Quaternion.LookRotation(tDir)*CollisionBoxBL) + tVect1;
//						cVerts[1+(j*4)] = (Quaternion.LookRotation(tDir)*CollisionBoxBR) + tVect1;
//						cVerts[2+(j*4)] = (Quaternion.LookRotation(tDir)*CollisionBoxTL) + tVect1;
//						cVerts[3+(j*4)] = (Quaternion.LookRotation(tDir)*CollisionBoxTR) + tVect1;
					}	
					
					if(j==(MeshCount-1)){
						Vector3 tAdd = default(Vector3);
						if(Axis == AxisTypeEnum.X){
							tAdd = new Vector3(mMaxDiff*-1f,0f,0f);
						}else{
							tAdd = new Vector3(0f,0f,mMaxDiff);
						}
						
						if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
							cVerts[0+((j+1)*3)] = tMatrix.MultiplyPoint3x4(CollisionTriBL + tAdd);
							cVerts[1+((j+1)*3)] = tMatrix.MultiplyPoint3x4(CollisionTriBR + tAdd);
							cVerts[2+((j+1)*3)] = tMatrix.MultiplyPoint3x4(CollisionTriT + tAdd);
							
//							cVerts[0+((j+1)*3)] = (Quaternion.LookRotation(tDir)*(CollisionTriBL + tAdd)) + tVect1;
//							cVerts[1+((j+1)*3)] = (Quaternion.LookRotation(tDir)*(CollisionTriBR + tAdd)) + tVect1;
//							cVerts[2+((j+1)*3)] = (Quaternion.LookRotation(tDir)*(CollisionTriT + tAdd)) + tVect1;
						}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
							cVerts[0+((j+1)*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxBL + tAdd);
							cVerts[1+((j+1)*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxBR + tAdd);
							cVerts[2+((j+1)*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxTL + tAdd);
							cVerts[3+((j+1)*4)] = tMatrix.MultiplyPoint3x4(CollisionBoxTR + tAdd);
							
//							cVerts[0+((j+1)*4)] = (Quaternion.LookRotation(tDir)*(CollisionBoxBL + tAdd)) + tVect1;
//							cVerts[1+((j+1)*4)] = (Quaternion.LookRotation(tDir)*(CollisionBoxBR + tAdd)) + tVect1;
//							cVerts[2+((j+1)*4)] = (Quaternion.LookRotation(tDir)*(CollisionBoxTL + tAdd)) + tVect1;
//							cVerts[3+((j+1)*4)] = (Quaternion.LookRotation(tDir)*(CollisionBoxTR + tAdd)) + tVect1;
						}		
					}
				}

				//If j > 0, the previous max vects need to match current min vects:
				Vector3 mVect = default(Vector3);
				if(j>0){
//					foreach(KeyValuePair<int,int> KVP in MatchingIndices){
//						tNormals[vManuver+KVP.Key] = tNormals[KVP.Value];
//					}
					foreach(KeyValuePair<int,int> KVP in MatchingIndices_Min){
						mVect = tVerts[vManuver+KVP.Key] - tVerts[vManuver_Prev+KVP.Value];
						tVerts[vManuver+KVP.Key] = tVerts[vManuver_Prev+KVP.Value];
					}
					
					for(int g=0;g<MinVectorIndices.Count;g++){
						if(!MatchingIndices_Min.ContainsKey(MinVectorIndices[g])){
							tVerts[vManuver+MinVectorIndices[g]] -= mVect;
						}
					}
					
					//Simple collision (triangle or trap):
					if(bSimpleCollisionOn){
						if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
							cVerts[0+(j*3)] -= mVect;
							cVerts[1+(j*3)] -= mVect;
							cVerts[2+(j*3)] -= mVect;

						}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
							cVerts[0+(j*4)] -= mVect;
							cVerts[1+(j*4)] -= mVect;
							cVerts[2+(j*4)] -= mVect;
							cVerts[3+(j*4)] -= mVect;
						}	
					}
				}

				//Triangles:
				for(int i=0;i<OrigTriCount;i++){
					tTris[i+TriManuver] = OrigTris[i]+vManuver;	
				}
				
				//Vert cut reverse:
				if(bVerticalCutoff){
					if(bVerticalMeshCutoff_OppositeDir){
						VertCutBuffer1 = tVerts[vManuver+VertCutTriIndex1];
						VertCutBuffer2 = tVerts[vManuver+VertCutTriIndex2];
	
						for(int i=0;i<MiddleCount;i++){
							VertCutBuffer3 = tVerts[vManuver+MiddleVectorIndicies[i]];
							
							if(!bVerticalCutoffDownwards){
								tBuffer = GetVHeightAtXY(ref VertCutBuffer1, ref VertCutBuffer2, ref VertCutBuffer3) + VerticalMeshCutoffOffset;
								if(VertCutBuffer3.y < tBuffer){
									tVerts[vManuver+MiddleVectorIndicies[i]].y = tBuffer;
								}
							}else{
								tBuffer = GetVHeightAtXY(ref VertCutBuffer1, ref VertCutBuffer2, ref VertCutBuffer3) - VerticalMeshCutoffOffset;
								if(VertCutBuffer3.y > tBuffer){
									tVerts[vManuver+MiddleVectorIndicies[i]].y = tBuffer;
								}
							}
						}
					}
				}
				
				
				//Ending push down:
				if(bStartDown){
					tFloat1 = mMaxHeight*1.05f;
					if(bStartTypeDownOverride){
						tFloat1 = StartTypeDownOverride;	
					}
					if(j==0){
						for(int i=0;i<MinCount;i++){
							tIntBuffer1 = MinVectorIndices[i];
							tVerts[vManuver+tIntBuffer1].y -= tFloat1;
						}

						float tTotalDistDown = 0f;
						Vector3 pVect1 = default(Vector3);
						Vector3 pVect2 = default(Vector3);
						foreach(KeyValuePair<int,int> KVP in MatchingIndices){
							pVect1 = tVerts[vManuver+KVP.Key];
							pVect2 = tVerts[vManuver+KVP.Value];
							tTotalDistDown = Vector3.Distance(pVect1,pVect2);
							break;
						}

						for(int i=0;i<MiddleCount;i++){
							tIntBuffer1 = MiddleVectorIndicies[i];
							float tDistTo1 = Vector3.Distance(tVerts[vManuver+tIntBuffer1],pVect1);
							tVerts[vManuver+tIntBuffer1].y -= (tFloat1*(tDistTo1/tTotalDistDown));
						}

						if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
							cVerts[0+(j*3)].y -= tFloat1;
							cVerts[1+(j*3)].y -= tFloat1;
							cVerts[2+(j*3)].y -= tFloat1;
						}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
							cVerts[0+(j*4)].y -= tFloat1;
							cVerts[1+(j*4)].y -= tFloat1;
							cVerts[2+(j*4)].y -= tFloat1;
							cVerts[3+(j*4)].y -= tFloat1;
						}	
					}
				}

				if(bEndDown){
					tFloat1 = mMaxHeight*1.05f;
					if(bEndTypeDownOverride){
						tFloat1 = EndTypeDownOverride;	
					}
					if(j==(MeshCount-1)){
						for(int i=0;i<MaxCount;i++){
							tIntBuffer1 = MaxVectorIndices[i];
							tVerts[vManuver+tIntBuffer1].y -= tFloat1;
						}

						float tTotalDistDown = 0f;
						Vector3 pVect1 = default(Vector3);
						Vector3 pVect2 = default(Vector3);
						foreach(KeyValuePair<int,int> KVP in MatchingIndices){
							pVect1 = tVerts[vManuver+KVP.Key];
							pVect2 = tVerts[vManuver+KVP.Value];
							tTotalDistDown = Vector3.Distance(pVect1,pVect2);
							break;
						}

						for(int i=0;i<MiddleCount;i++){
							tIntBuffer1 = MiddleVectorIndicies[i];
							float tDistTo1 = Vector3.Distance(tVerts[vManuver+tIntBuffer1],pVect2);
							tVerts[vManuver+tIntBuffer1].y -= (tFloat1*(tDistTo1/tTotalDistDown));
						}

						if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
							cVerts[0+((j+1)*3)].y -= tFloat1;
							cVerts[1+((j+1)*3)].y -= tFloat1;
							cVerts[2+((j+1)*3)].y -= tFloat1;
						}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
							cVerts[0+((j+1)*4)].y -= tFloat1;
							cVerts[1+((j+1)*4)].y -= tFloat1;
							cVerts[2+((j+1)*4)].y -= tFloat1;
							cVerts[3+((j+1)*4)].y -= tFloat1;
						}
					}
				}
				
				//Ending objects:
				if(j==0 && EndCapStartObj != null){
					if(bEndCapCustomMatchStart && MinVectorIndices.Count > 0){
						Vector3[] bVerts = new Vector3[MinVectorIndices.Count];
						for(int g=0;g<MinVectorIndices.Count;g++){
							bVerts[g] = tVerts[vManuver+MinVectorIndices[g]];
						}
						Vector3 tVect5 = GetVector3Average(bVerts);
						Vector3 tVect6 = tSpline.GetSplineValue(tSpline.GetClosestParam(tVect5,false,false),false);
						tVect5.y = tVect6.y;
						EndCapStartObj.transform.position = tVect5;
					}else{
						EndCapStartObj.transform.position = tVect1;
					}
					
					if(bEndObjectsMatchGround){
						tRay = default(Ray);
						tRayHit = null;
						float tHitY = 0f;
//						int tHitIndex = 0;
						Vector3 HitVect = EndCapStartObj.transform.position;
						tRay = new Ray(HitVect+new Vector3(0f,1f,0f),Vector3.down);
						tRayHit = Physics.RaycastAll(tRay);
						if(tRayHit.Length > 0){
							tRayYs = new float[tRayHit.Length];
							for(int g=0;g<tRayHit.Length;g++){
								if(g==0){ 
									tHitY = tRayHit[g].point.y; 
//									tHitIndex=0;
								}else{
									if(tRayHit[g].point.y > tHitY){
										tHitY = tRayHit[g].point.y;
//										tHitIndex = g;
									}
								}
							}
							HitVect.y = tHitY;
							EndCapStartObj.transform.position = HitVect;
						}
					}
					EndCapStartObj.transform.rotation = Quaternion.LookRotation(tDir);
					EndCapStartObj.transform.Rotate(EndCapCustomRotOffsetStart,Space.World);
					EndCapStartObj.transform.position += EndCapCustomOffsetStart;
					
				}else if(j==(MeshCount-1) && EndCapEndObj != null){
					if(bEndCapCustomMatchStart && MaxVectorIndices.Count > 0){
						Vector3[] bVerts = new Vector3[MaxVectorIndices.Count];
						for(int g=0;g<MaxVectorIndices.Count;g++){
							bVerts[g] = tVerts[vManuver+MaxVectorIndices[g]];
						}
						Vector3 tVect5 = GetVector3Average(bVerts);
						Vector3 tVect6 = tSpline.GetSplineValue(tSpline.GetClosestParam(tVect5,false,false),false);
						if(!float.IsNaN(tVect6.y)){
							tVect5.y = tVect6.y;	
						}
						EndCapEndObj.transform.position = tVect5;
					}else{
						EndCapEndObj.transform.position = tVect2;
					}
					
					if(bEndObjectsMatchGround){
						tRay = default(Ray);
						tRayHit = null;
						float tHitY = 0f;
//						int tHitIndex = 0;
						Vector3 HitVect = EndCapEndObj.transform.position;
						tRay = new Ray(HitVect+new Vector3(0f,1f,0f),Vector3.down);
						tRayHit = Physics.RaycastAll(tRay);

						if(tRayHit.Length > 0){
							tRayYs = new float[tRayHit.Length];
							for(int g=0;g<tRayHit.Length;g++){
								if(g==0){ 
									tHitY = tRayHit[g].point.y; 
//									tHitIndex=0;
								}else{
									if(tRayHit[g].point.y > tHitY){
										tHitY = tRayHit[g].point.y;
//										tHitIndex = g;
									}
								}
							}
							HitVect.y = tHitY;
							EndCapEndObj.transform.position = HitVect;
						}
					}
					EndCapEndObj.transform.rotation = Quaternion.LookRotation(tDir);	
					EndCapEndObj.transform.Rotate(EndCapCustomRotOffsetEnd,Space.World);
					EndCapEndObj.transform.position += EndCapCustomOffsetEnd;
				}
			}
			
		StretchSkip:
			if(bIsStretch){ vManuver = 0; }
			
			//End/Start for stretch:
			if(bIsStretch){
				//Ending objects:
				if(EndCapStartObj != null){
					tVect1 = tVerts[MinVectorIndices[0]];
					tFloat1 = tSpline.GetClosestParam(tVect1);
					tVect2 = tSpline.GetSplineValue(tFloat1,false);
					tVect1.y = tVect2.y;
					
					EndCapStartObj.transform.position = tVect1;
					EndCapStartObj.transform.rotation = Quaternion.LookRotation(tDir);
					EndCapStartObj.transform.Rotate(EndCapCustomRotOffsetStart,Space.World);
					EndCapStartObj.transform.position += EndCapCustomOffsetStart;
				}
				if(EndCapEndObj != null){
					tVect1 = tVerts[MaxVectorIndices[0]];
					tFloat1 = tSpline.GetClosestParam(tVect1);
					tVect2 = tSpline.GetSplineValue(tFloat1,false);
					tVect1.y = tVect2.y;
					
					EndCapEndObj.transform.position = tVect1;
					EndCapEndObj.transform.rotation = Quaternion.LookRotation(tDir);	
					EndCapEndObj.transform.Rotate(EndCapCustomRotOffsetEnd,Space.World);
					EndCapEndObj.transform.position += EndCapCustomOffsetEnd;
				}
			}
			
			if(bSimpleCollisionOn && !bIsStretch){
				if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
					cTris = GetCollisionTris_Tri(MeshCount,cTriCount,cCount);
				}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
					cTris = GetCollisionTris_Box(MeshCount,cTriCount,cCount);
				}
			}
			
			if(CapMesh1 != null){
				Vector3[] cap1_verts = new Vector3[CapOrigMVL1];	
				System.Array.Copy(CapOrigVerts1,cap1_verts,CapOrigMVL1);
				int[] cap1_tris = new int[CapTriCount1];
				System.Array.Copy(CapOrigTris1,cap1_tris,CapTriCount1);
				Vector2[] cap1_uv = new Vector2[CapOrigMVL1];
				System.Array.Copy(CapOrigUV1,cap1_uv,CapOrigMVL1);
				Vector3[] cap1_normals = new Vector3[CapOrigMVL1];
				System.Array.Copy(CapOrigNormals1,cap1_normals,CapOrigMVL1);
				bool[] cap1_hit = new bool[CapOrigMVL1];
				bool bcapstart = true;
				float tHeight = 0f;
				
				foreach(KeyValuePair<int,List<int>> KVP in MatchingIndices_Min_Cap){
					int wCount = KVP.Value.Count;
					for(int i=0;i<wCount;i++){
						if(bcapstart){
							tVect1 = cap1_verts[KVP.Value[i]] - tVerts[KVP.Key];
						}
						cap1_verts[KVP.Value[i]] = tVerts[KVP.Key];	
						cap1_hit[KVP.Value[i]] = true;
						if(bcapstart){
							tHeight = tSpline.GetSplineValue(tSpline.GetClosestParam(cap1_verts[KVP.Value[i]]),false).y;
							bcapstart = false;
						}
					}
				}
				
				float tParam  = 0f;
				for(int i=0;i<CapOrigMVL1;i++){
					if(!cap1_hit[i]){
						cap1_verts[i] -= tVect1;
						tParam = tSpline.GetClosestParam(cap1_verts[i]);
						tVect2 = tSpline.GetSplineValue(tParam,false);
						cap1_verts[i].y -= (tHeight-tVect2.y);
						cap1_verts[i].y += CapHeightOffset1;
					}
				}
				
				Vector3[] nVerts = new Vector3[CapOrigMVL1+tVerts.Length];
				Vector3[] nNormals = new Vector3[CapOrigMVL1+tNormals.Length];
				int[] nTris = new int[CapTriCount1+tTris.Length];
				Vector2[] nUV = new Vector2[CapOrigMVL1+tUV.Length];
				int OldTriCount = tTris.Length;
				int OldMVL = tVerts.Length;
				
				System.Array.Copy(cap1_verts,	nVerts,		CapOrigMVL1);
				System.Array.Copy(cap1_normals,	nNormals,	CapOrigMVL1);
				System.Array.Copy(cap1_tris,	nTris,		CapTriCount1);
				System.Array.Copy(cap1_uv,		nUV,		CapOrigMVL1);
				
				System.Array.Copy(tVerts,	0,nVerts,	CapOrigMVL1,	OldMVL);
				System.Array.Copy(tNormals,	0,nNormals,	CapOrigMVL1,	OldMVL);
				System.Array.Copy(tTris,	0,nTris,	CapTriCount1,	OldTriCount);
				System.Array.Copy(tUV,		0,nUV,		CapOrigMVL1,	OldMVL);

				for(int i=CapTriCount1;i<(CapTriCount1+OldTriCount);i++){
					nTris[i] += CapOrigMVL1;	
				}
				
				tVerts = nVerts;
				tTris = nTris;
				tNormals = nNormals;
				tUV = nUV;
			}
			
			if(CapMesh2 != null){
				Vector3[] cap2_verts = new Vector3[CapOrigMVL2];
				System.Array.Copy(CapOrigVerts2,cap2_verts,CapOrigMVL2);
				int[] cap2_tris = new int[CapTriCount2];
				System.Array.Copy(CapOrigTris2,cap2_tris,CapTriCount2);
				Vector2[] cap2_uv = new Vector2[CapOrigMVL2];
				System.Array.Copy(CapOrigUV2,cap2_uv,CapOrigMVL2);
				Vector3[] cap2_normals = new Vector3[CapOrigMVL2];
				System.Array.Copy(CapOrigNormals2,cap2_normals,CapOrigMVL2);
				bool[] cap2_hit = new bool[CapOrigMVL2];
				bool bcapstart = true;
				float tHeight = 0f;
				
				foreach(KeyValuePair<int,List<int>> KVP in MatchingIndices_Max_Cap){
					int wCount = KVP.Value.Count;
					for(int i=0;i<wCount;i++){
						if(bcapstart){
							tVect1 = cap2_verts[KVP.Value[i]] - tVerts[vManuver+KVP.Key+CapOrigMVL1];	
						}
						cap2_verts[KVP.Value[i]] = tVerts[vManuver+KVP.Key+CapOrigMVL1];	
						cap2_hit[KVP.Value[i]] = true;
						
						if(bcapstart){
							tHeight = tSpline.GetSplineValue(tSpline.GetClosestParam(cap2_verts[KVP.Value[i]]),false).y;
							bcapstart = false;
						}
					}
				}
				
				float tParam = 0f;
				for(int i=0;i<CapOrigMVL2;i++){
					
					if(!cap2_hit[i]){
						cap2_verts[i] -= tVect1;
						tParam = tSpline.GetClosestParam(cap2_verts[i]);
						tVect2 = tSpline.GetSplineValue(tParam,false);
						cap2_verts[i].y -= (tHeight-tVect2.y);
						cap2_verts[i].y += CapHeightOffset2;
					}
				}
				
				Vector3[] nVerts = new Vector3[CapOrigMVL2+tVerts.Length];
				Vector3[] nNormals = new Vector3[CapOrigMVL2+tNormals.Length];
				int[] nTris = new int[CapTriCount2+tTris.Length];
				Vector2[] nUV = new Vector2[CapOrigMVL2+tUV.Length];
				int OldTriCount = tTris.Length;
				int OldMVL = tVerts.Length;

				System.Array.Copy(tVerts,	0,nVerts,	0,	OldMVL);
				System.Array.Copy(tNormals,	0,nNormals,	0,	OldMVL);
				System.Array.Copy(tTris,	0,nTris,	0,	OldTriCount);
				System.Array.Copy(tUV,		0,nUV,		0,	OldMVL);
				
				System.Array.Copy(cap2_verts,	0,nVerts,	OldMVL,			CapOrigMVL2);
				System.Array.Copy(cap2_normals,	0,nNormals,	OldMVL,			CapOrigMVL2);
				System.Array.Copy(cap2_tris,	0,nTris,	OldTriCount,	CapTriCount2);
				System.Array.Copy(cap2_uv,		0,nUV,		OldMVL,			CapOrigMVL2);
				
				for(int i=OldTriCount;i<nTris.Length;i++){
					nTris[i] += OldMVL;	
				}
				
				tVerts = nVerts;
				tTris = nTris;
				tNormals = nNormals;
				tUV = nUV;
			}
			
			int tVertCount = tVerts.Length;
			for(int i=0;i<tVertCount;i++){
				tVerts[i] -= tNode.pos;	
			}
			if(cVerts != null){
				int cVertCount = cVerts.Length;
				for(int i=0;i<cVertCount;i++){
					cVerts[i] -= tNode.pos;	
				}
			}
			
			//Mesh creation:
			Mesh xMesh = new Mesh();
			xMesh.vertices = tVerts;
			xMesh.triangles = tTris;
			xMesh.normals = tNormals;
			xMesh.uv = tUV;
			xMesh.RecalculateNormals();
			tNormals = xMesh.normals;
			Vector3 tAvgNormal = default(Vector3);
			tIntBuffer1 = 0;
			if(!bIsStretch){
				for(int j=1;j<MeshCount;j++){
					vManuver = j*OrigMVL;
					vManuver_Prev = (j-1)*OrigMVL;
					if(CapMesh1 != null){ tIntBuffer1 = CapOrigMVL1; }
					foreach(KeyValuePair<int,int> KVP in MatchingIndices_Min){
						tAvgNormal = (tNormals[tIntBuffer1+vManuver+KVP.Key] + tNormals[tIntBuffer1+vManuver_Prev+KVP.Value]) * 0.5f;
						tNormals[tIntBuffer1+vManuver+KVP.Key] = tAvgNormal;
						tNormals[tIntBuffer1+vManuver_Prev+KVP.Key] = tAvgNormal;
					}
				}
				xMesh.normals = tNormals;
			}
			xMesh.tangents = GSDRootUtil.ProcessTangents(tTris,tNormals,tUV,tVerts);
			
			if(tName == null || tName.Length < 1){
				tName = "ExtrudedMesh";
			}

			Output = new GameObject(tName);
			Output.transform.position = tNode.pos;

			MF = Output.AddComponent<MeshFilter>();
			MF.sharedMesh = xMesh;
			
			if(tNode.GSDSpline.tRoad.GSDRS.opt_bSaveMeshes){
				SaveMesh(ref xMesh,false);
			}

			//Colliders:
			MeshCollider MC = null;
			if(CollisionType == CollisionTypeEnum.SimpleMeshTriangle){
				MC = Output.AddComponent<MeshCollider>();
				Mesh cMesh = new Mesh();
				cMesh.vertices = cVerts;
				cMesh.triangles = cTris;
				cMesh.normals = new Vector3[cVerts.Length];
				if(MC != null){ MC.sharedMesh = cMesh; }
				if(MC != null){
					MC.convex = bCollisionConvex;
					MC.isTrigger = bCollisionTrigger;
					if(tNode.GSDSpline.tRoad.GSDRS.opt_bSaveMeshes){
						cMesh.uv = new Vector2[cVerts.Length];
						cMesh.tangents = GSDRootUtil.ProcessTangents(cTris,cMesh.normals,cMesh.uv,cVerts);
						SaveMesh(ref cMesh,true);
					}
				}
			}else if(CollisionType == CollisionTypeEnum.SimpleMeshTrapezoid){
				MC = Output.AddComponent<MeshCollider>();
				Mesh cMesh = new Mesh();
				cMesh.vertices = cVerts;
				cMesh.triangles = cTris;
				cMesh.normals = new Vector3[cVerts.Length];
				if(MC != null){ MC.sharedMesh = cMesh; }
				if(MC != null){
					MC.convex = bCollisionConvex;
					MC.isTrigger = bCollisionTrigger;
					if(tNode.GSDSpline.tRoad.GSDRS.opt_bSaveMeshes){
						cMesh.uv = new Vector2[cVerts.Length];
						cMesh.tangents = GSDRootUtil.ProcessTangents(cTris,cMesh.normals,cMesh.uv,cVerts);
						SaveMesh(ref cMesh,true);
					}
				}
			}else if(CollisionType == CollisionTypeEnum.MeshCollision){
				MC = Output.AddComponent<MeshCollider>();
				if(MC != null){ MC.sharedMesh = xMesh; }
				if(MC != null){
					MC.convex = bCollisionConvex;
					MC.isTrigger = bCollisionTrigger;
				}
			}else if(CollisionType == CollisionTypeEnum.BoxCollision){
				//Primitive collider:
				GameObject BC_Obj = new GameObject("Primitive");
				BoxCollider BC = BC_Obj.AddComponent<BoxCollider>();
				BC_Obj.transform.position = tNode.pos;
				BC_Obj.transform.rotation = Quaternion.LookRotation(tNode.tangent);
				
				Vector3 BCCenter = default(Vector3);
//				if(bStraightLineMatchStartEnd){
//					if(tNode.bIsBridge && tNode.bIsBridgeMatched && tNode.BridgeCounterpartNode != null){
//						BCCenter = ((tNode.pos - tNode.BridgeCounterpartNode.pos)*0.5f)+tNode.BridgeCounterpartNode.pos;
//					}else if(tNode.idOnSpline < (tSpline.GetNodeCount()-1)){
//						BCCenter = ((tNode.pos - tSpline.mNodes[tNode.idOnSpline+1].pos)*0.5f)+tSpline.mNodes[tNode.idOnSpline+1].pos;
//					}else{
//						
//					}
//					BCCenter.y -= VerticalRaise;
//					BCCenter.y -= (mMaxHeight*0.5f);
//				}else{
					Vector3 POS = default(Vector3);
					tSpline.GetSplineValue_Both(StartTime,out tVect1, out POS);
					//Goes right if not neg:
					tVect1 = (tVect1 + new Vector3(HorizontalSep*POS.normalized.z,0,HorizontalSep*-POS.normalized.x));
					tSpline.GetSplineValue_Both(EndTime,out tVect2, out POS);
					tVect2 = (tVect2 + new Vector3(HorizontalSep*POS.normalized.z,0,HorizontalSep*-POS.normalized.x));
					tVect1.y += VerticalRaise;
					tVect2.y += VerticalRaise;
					BCCenter = ((tVect1 - tVect2)*0.5f)+tVect2;
					BCCenter.y += ((mMinY + mMaxY)*0.5f);
//				}
				
				BCCenter -= tNode.pos;
				BCCenter.z *= -1f;
				
				if(bBCFlipX){
					BCCenter.z *= -1f;
				}
				if(bBCFlipZ){
					BCCenter.x *= -1f;
				}
				
//				

				Vector3 BCCenter2 = new Vector3(BCCenter.z,BCCenter.y,BCCenter.x);
				
				
				BCCenter2 += StretchBC_LocOffset;
			
				
				BC.center = BCCenter2;
				
				tFloat1 = Vector3.Distance(tNode.pos,tNode.BridgeCounterpartNode.pos);
				
				if(bStretchSize){
					BC.size = StretchBC_Size;
				}else{
					if(Axis == AxisTypeEnum.X){
						BC.size = new Vector3(tFloat1,mMaxHeight,(mMaxZ-mMinZ));	
					}else{
						BC.size = new Vector3((mMaxX-mMinX),mMaxHeight,tFloat1);			
					}
					StretchBC_Size = BC.size;
				}
				BC_Obj.transform.parent = Output.transform;
			}
			
			
			//Use prefab mats if no material override:
			MeshRenderer MR = Output.AddComponent<MeshRenderer>();
			if(SplinatedMaterial1 == null && !bMaterialOverride){
				MeshRenderer PrefabMR = tObj.GetComponent<MeshRenderer>();
				if(PrefabMR != null && PrefabMR.sharedMaterials != null){
					MR.materials = PrefabMR.sharedMaterials;	
				}
			}else{
				//Else, use override mats:
				tIntBuffer1 = 0;
				if(SplinatedMaterial1 != null){
					tIntBuffer1+=1;
					if(SplinatedMaterial2 != null){
						tIntBuffer1+=1;	
					}
				}
				if(tIntBuffer1 > 0){
					Material[] tMats = new Material[tIntBuffer1];
					if(SplinatedMaterial1 != null){
						tMats[0] = SplinatedMaterial1;
						if(SplinatedMaterial2 != null){
							tMats[1] = SplinatedMaterial2;
						}
					}
					MR.materials = tMats;
				}
			}
			
			mMaxX = mMaxX*1.5f;
			mMinX = mMinX*1.5f;
			mMaxY = mMaxY*1.5f;
			mMinY = mMinY*1.5f;
			mMaxZ = mMaxZ*1.5f;
			mMinZ = mMinZ*1.5f;
			
			StartPos = tSpline.GetSplineValue(StartTime);
			EndPos = tSpline.GetSplineValue(EndTime);
			
			//Destroy the instantiated prefab:
			if(tObj != null){ Object.DestroyImmediate(tObj); }
			if(Cap1 != null){ Object.DestroyImmediate(Cap1); }
			if(Cap2 != null){ Object.DestroyImmediate(Cap2); }
			
			Material[] fMats = MR.sharedMaterials;
			
			//Set the new object with the specified vertical raise:
			Output.transform.name = tName;
			Output.transform.parent = MasterObjTrans;
			if(EndCapStartObj != null){
				EndCapStartObj.transform.parent = Output.transform;
				EndCapStartOutput = EndCapStartObj;
				
				MeshRenderer eMR = EndCapStartObj.GetComponent<MeshRenderer>();
				if(eMR == null){ eMR = EndCapStartObj.AddComponent<MeshRenderer>(); }
				if(eMR.sharedMaterials == null || (eMR.sharedMaterial != null && eMR.sharedMaterial.name.ToLower().Contains("default-diffuse"))){
					eMR.sharedMaterials = fMats;
				}
			}
			if(EndCapEndObj != null){
				EndCapEndObj.transform.parent = Output.transform;
				EndCapEndOutput = EndCapEndObj;
				MeshRenderer eMR = EndCapEndObj.GetComponent<MeshRenderer>();
				if(eMR == null){ eMR = EndCapEndObj.AddComponent<MeshRenderer>(); }
				if(eMR.sharedMaterials == null || (eMR.sharedMaterial != null && eMR.sharedMaterial.name.ToLower().Contains("default-diffuse"))){
					eMR.sharedMaterials = fMats;
				}
			}
			
			if(bCollect){
				tNode.GSDSpline.tRoad.bTriggerGC = true;
			}
			#endif
		}	
		
		private void SaveMesh(ref Mesh tMesh, bool bIsCollider){
			#if UNITY_EDITOR
			if(!tNode.GSDSpline.tRoad.GSDRS.opt_bSaveMeshes){ return; }
			//string tSceneName = System.IO.Path.GetFileName(UnityEditor.EditorApplication.currentScene).ToLower().Replace(".unity","");
            string tSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            tSceneName = tSceneName.Replace("/","");
			tSceneName = tSceneName.Replace(".","");
			string tFolderName = "Assets/RoadArchitect/Mesh/Generated/Extrusions/";
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
		
		void DoStretch(ref Vector3[] OrigVerts,ref Vector2[] OrigUV, ref int[] OrigTris,ref List<int> MaxVectorIndices,ref List<int> MinVectorIndices,float mMaxDiff, out Vector3[] tVerts, out Vector2[] tUV, out Vector3[] tNormals, out int[] tTris){
			Vector3 tVect1 = tNode.pos;
			Vector3 tVect2 = default(Vector3);
			
//			if(bStraightLineMatchStartEnd){
//				if(tNode.bIsBridge && tNode.bIsBridgeMatched && tNode.BridgeCounterpartNode != null){
//					tVect2 = tNode.BridgeCounterpartNode.pos;
//				}else if(tNode.idOnSpline < (tSpline.GetNodeCount()-1)){
//					tVect2 = tSpline.mNodes[tNode.idOnSpline+1].pos;	
//				}
//			}
			
			Vector3 POS = default(Vector3);
			Vector3 tDir = tNode.tangent;

//			if(!bStraightLineMatchStartEnd){
				tSpline.GetSplineValue_Both(StartTime,out tVect1, out POS);
				//Goes right if not neg:
				tVect1 = (tVect1 + new Vector3(HorizontalSep*POS.normalized.z,0,HorizontalSep*-POS.normalized.x));
				
				tSpline.GetSplineValue_Both(EndTime,out tVect2, out POS);
				tVect2 = (tVect2 + new Vector3(HorizontalSep*POS.normalized.z,0,HorizontalSep*-POS.normalized.x));
				
				tVect1.y += VerticalRaise;
				tVect2.y += VerticalRaise;

				tDir = tSpline.GetSplineValue(StartTime,true);
//			}
			
			Matrix4x4 tMatrixStart = new Matrix4x4();
			Matrix4x4 tMatrixEnd = new Matrix4x4();
			int OrigMVL = OrigVerts.Length;
			
			tVerts = new Vector3[OrigMVL];
			tUV = new Vector2[OrigMVL];
			tNormals = new Vector3[OrigMVL];
			tTris = new int[OrigTris.Length];
			System.Array.Copy(OrigVerts,tVerts,OrigMVL);
			System.Array.Copy(OrigTris,tTris,OrigTris.Length);
			System.Array.Copy(OrigUV,tUV,OrigMVL);

			tMatrixStart.SetTRS(tVect1,Quaternion.LookRotation(tDir),new Vector3(1f,1f,1f));
			tMatrixEnd.SetTRS(tVect2,Quaternion.LookRotation(tDir),new Vector3(1f,1f,1f));
				
			//Rotate and set vertex positions:
			float NewDiff = Vector3.Distance(tVect1,tVect2);
			float UVMod = NewDiff / mMaxDiff;	
			Vector3 xVect = default(Vector3);
			for(int i=0;i<OrigMVL;i++){
				xVect = OrigVerts[i];
				if(MaxVectorIndices.Contains(i)){
					tVerts[i] = tMatrixEnd.MultiplyPoint3x4(xVect);
				}else{
					tVerts[i] = tMatrixStart.MultiplyPoint3x4(xVect);
				}
				
				if(RepeatUVType == RepeatUVTypeEnum.X){
					if(OrigUV[i].x > Stretch_UVThreshold){
						tUV[i].x = OrigUV[i].x * UVMod;
					}
				}else{
					if(OrigUV[i].y > Stretch_UVThreshold){
						tUV[i].y = OrigUV[i].y * UVMod;
					}
				}
			}
		}
		
		Vector3 GetAverageNormalToGround(GameObject tObj){
			Ray tRay = default(Ray);
			RaycastHit[] tRayHit = null;
			float tHitY = 0f;
			int tHitIndex = 0;
			Vector3 tHitNormal = default(Vector3);
		
			Bounds tBounds = tObj.GetComponent<MeshFilter>().sharedMesh.bounds;

			Vector3[] tVects = new Vector3[8];
			tVects[0] = tBounds.min;
		    tVects[1] = tBounds.max;
		    tVects[2] = new Vector3(tVects[0].x, tVects[0].y, tVects[1].z);
		    tVects[3] = new Vector3(tVects[0].x, tVects[1].y, tVects[0].z);
		    tVects[4] = new Vector3(tVects[1].x, tVects[0].y, tVects[0].z);
		    tVects[5] = new Vector3(tVects[0].x, tVects[1].y, tVects[1].z);
		    tVects[6] = new Vector3(tVects[1].x, tVects[0].y, tVects[1].z);
		    tVects[7] = new Vector3(tVects[1].x, tVects[1].y, tVects[0].z);
			
			List<Vector3> xVects = new List<Vector3>();

			for(int i=0;i<8;i++){
				tRay = new Ray(tVects[i]+new Vector3(0f,1f,0f),Vector3.down);
				tRayHit = Physics.RaycastAll(tRay);
				tHitIndex = -1;
				tHitY = -1f;
				if(tRayHit.Length > 0){
					for(int g=0;g<tRayHit.Length;g++){
						if(g==0){ 
							tHitY = tRayHit[g].point.y; 
							tHitIndex=0;
						}else{
							if(tRayHit[g].point.y > tHitY){
								tHitY = tRayHit[g].point.y;
								tHitIndex = g;
							}
						}
					}
					xVects.Add(tRayHit[tHitIndex].normal);
				}
			}
			
			
			for(int i=0;i<xVects.Count;i++){
				tHitNormal += xVects[i];
			}
			tHitNormal /= xVects.Count;
			
			return tHitNormal;
		}
	}
	
	#endif
}
