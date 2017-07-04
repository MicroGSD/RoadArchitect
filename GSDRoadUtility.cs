using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
#endif
namespace GSD.Roads{
	//Generic http://www.fhwa.dot.gov/bridge/bridgerail/br053504.cfm
	public enum RailingTypeEnum{None,Generic1,Generic2,K_Rail,WBeam};
	public enum RailingSubTypeEnum{Both,Left,Right};
	public enum SignPlacementSubTypeEnum{Center,Left,Right};
	public enum CenterDividerTypeEnum{None,K_Rail,KRail_Blinds,Wire,Markers};
	public enum EndCapTypeEnum{None,WBeam,Barrels3Static,Barrels3Rigid,Barrels7Static,Barrels7Rigid};
	public enum RoadUpdateTypeEnum {Full,Intersection,Railing,CenterDivider,Bridges};
	public enum AxisTypeEnum {X,Y,Z};
	
	#if UNITY_EDITOR
	public static class GSDConstruction{
		private static bool IsApproximately(float a, float b){
	    	return IsApproximately(a, b, 0.01f);
	    }
	     
	    private static bool IsApproximately(float a, float b, float tolerance){
	   		return Mathf.Abs(a - b) < tolerance;
	    }
		
		public static GSDSplineN CreateNode(GSDRoad RS, bool bSpecialEndNode = false, Vector3 vSpecialLoc = default(Vector3), bool bInterNode = false){
			Object[] tWorldNodeCount = GameObject.FindObjectsOfType(typeof(GSDSplineN));
			GameObject tNodeObj = new GameObject("Node" + tWorldNodeCount.Length.ToString());
			if(!bInterNode){
				UnityEditor.Undo.RegisterCreatedObjectUndo(tNodeObj, "Created node");
			}
			GSDSplineN tNode = tNodeObj.AddComponent<GSDSplineN>();

			if(bSpecialEndNode){
				tNode.bSpecialEndNode = true;
				tNodeObj.transform.position = vSpecialLoc;
			}else{
				tNodeObj.transform.position = RS.Editor_MousePos;
				//This helps prevent double clicks:
				int mCount = RS.GSDSpline.GetNodeCount();
				for(int i=0;i<mCount;i++){
					if(Vector3.Distance(RS.Editor_MousePos,RS.GSDSpline.mNodes[i].pos) < 5f){
						Object.DestroyImmediate(tNodeObj);
						return null;
					}
				}
				//End double click prevention
			}
			Vector3 xVect = tNodeObj.transform.position;
			if(xVect.y < 0.03f){ xVect.y = 0.03f; }
			tNodeObj.transform.position = xVect;

			tNodeObj.transform.parent = RS.GSDSplineObj.transform;
			tNode.idOnSpline = RS.GSDSpline.GetNodeCount()+1;
			tNode.GSDSpline = RS.GSDSpline;
			
			//Enforce max road grade:
			if(RS.opt_bMaxGradeEnabled && !bSpecialEndNode){
				tNode.EnsureGradeValidity(-1,true);
			}
			
			if(!bInterNode && !bSpecialEndNode){
				RS.UpdateRoad();
			}
			return tNode;
		}
		//Insert
		//Detect closest node (if end node, auto select other node)
		//Determine which node is closest (up or down) on spline
		//Place node, adjust all id on splines
		//Setup spline
		public static GSDSplineN InsertNode(GSDRoad RS, bool bForcedLoc = false, Vector3 ForcedLoc = default(Vector3), bool bIsPreNode = false, int InsertIndex = -1, bool bSpecialEndNode = false, bool bInterNode = false){
			GameObject tNodeObj;
			Object[] tWorldNodeCount = GameObject.FindObjectsOfType(typeof(GSDSplineN));
			if(!bForcedLoc){
				tNodeObj = new GameObject("Node" + tWorldNodeCount.Length.ToString());	
			}else if(bForcedLoc && !bSpecialEndNode){
				tNodeObj = new GameObject("Node" + tWorldNodeCount.Length.ToString() + "Ignore");
			}else{
				tNodeObj = new GameObject("Node" + tWorldNodeCount.Length.ToString());	
			}
			if(!bInterNode){
				UnityEditor.Undo.RegisterCreatedObjectUndo(tNodeObj, "Inserted node");
			}
			
			if(!bForcedLoc){
				tNodeObj.transform.position = RS.Editor_MousePos;
				
				//This helps prevent double clicks:
				int mCount = RS.GSDSpline.GetNodeCount();
				for(int i=0;i<mCount;i++){
					if(Vector3.Distance(RS.Editor_MousePos,RS.GSDSpline.mNodes[i].pos) < 15f){
						Object.DestroyImmediate(tNodeObj);
						return null;
					}
				}
				//End double click prevention
			}else{
				tNodeObj.transform.position = ForcedLoc;
			}
			Vector3 xVect = tNodeObj.transform.position;
			if(xVect.y < 0.03f){ xVect.y = 0.03f; }
			tNodeObj.transform.position = xVect;
			tNodeObj.transform.parent = RS.GSDSplineObj.transform;
			
			int cCount = RS.GSDSpline.mNodes.Count;
//			float mDistance = 50000f;
//			float tDistance = 0f;
			
			float tParam; 
			if(!bForcedLoc){
				tParam = RS.GSDSpline.GetClosestParam(RS.Editor_MousePos,false,true);
			}else{
				tParam = RS.GSDSpline.GetClosestParam(ForcedLoc,false,true);
			}
			bool bEndInsert = false;
			bool bZeroInsert = false;
			int iStart = 0;
            if (GSDRootUtil.IsApproximately(tParam, 0f, 0.0001f)) {
				bZeroInsert = true;
				iStart = 0;
            } else if (GSDRootUtil.IsApproximately(tParam, 1f, 0.0001f)) {
				bEndInsert = true;
			}
			
			if(bForcedLoc){
				iStart = InsertIndex;
			}else{
				for(int i=0;i<cCount;i++){
					GSDSplineN xNode = RS.GSDSpline.mNodes[i];
					if(!bZeroInsert && !bEndInsert){
						if(tParam > xNode.tTime){
							iStart = xNode.idOnSpline+1;
						}
					}
				}
			}
	
			if(bEndInsert){
				iStart = RS.GSDSpline.mNodes.Count;
			}else{
				for(int i=iStart;i<cCount;i++){
					RS.GSDSpline.mNodes[i].idOnSpline+=1;
				}
			}
			
			GSDSplineN tNode = tNodeObj.AddComponent<GSDSplineN>();
			if(bForcedLoc && !bSpecialEndNode){
				tNode.bIsBridge = true;
				tNode.bIgnore = true;
//				tNode.bIsBridge_PreNode = bIsPreNode;
//				tNode.bIsBridge_PostNode = !bIsPreNode;	
			}
			tNode.GSDSpline = RS.GSDSpline;
			tNode.idOnSpline = iStart;
			tNode.bSpecialEndNode = bSpecialEndNode;
			if(!bForcedLoc){
				tNode.pos = RS.Editor_MousePos;
			}else{
				tNode.pos = ForcedLoc;
			}
			
			RS.GSDSpline.mNodes.Insert(iStart,tNode);
	
			//Enforce maximum road grade:
			if(!bForcedLoc && !bSpecialEndNode && RS.opt_bMaxGradeEnabled){
				tNode.EnsureGradeValidity(iStart);
			}

			if(!bInterNode && !bSpecialEndNode){
				if(!bForcedLoc){
					RS.UpdateRoad();
				}
			}
			
			return tNode;
		}
    }

	public static class GSDTerraforming{
		public class TempTerrainData{
			public int HM;
			public int HMHeight;
			public float[,] heights;
			public bool[,] tHeights;
	
			public float HMRatio;
			public float MetersPerHM = 0f;
				
			//Heights:
			public ushort[] cX;
			public ushort[] cY;
			public float[] cH;
			public float[] oldH;
			public int cI = 0;
			public int TerrainMaxIndex;
			
			//Details:
			public int DetailLayersCount;
			
			public List<ushort> MainDetailsX;
			public List<ushort> MainDetailsY;
			
			public List<List<ushort>> DetailsX;
			public List<List<ushort>> DetailsY;
			public List<List<ushort>> OldDetailsValue;
//			public Dictionary<int,int[,]> DetailValues;
			public int[] DetailsI;
			public float DetailToHeightRatio;
			
			
			
			
			
//			public Dictionary<int,bool[,]> DetailHasProcessed;
			
			public HashSet<int> DetailHasProcessed;
			
	
			
//			public List<List<bool>> OldDetailsValue;
			
			public int DetailMaxIndex;
			public HashSet<int> DetailLayersSkip;
			
			//Trees
			public List<TreeInstance> TreesCurrent;
			public List<TreeInstance> TreesOld;
			public int TreesI;
			public int TreeSize;
				
			public Vector3 TerrainSize;
			public Vector3 TerrainPos;
			public int GSDID;
			
			public void Nullify(){
				heights = null;
				tHeights = null;
				cX = null;
				cY = null;
				cH = null;
				oldH = null;
//				DetailsX = null;
//				DetailsY = null;
//				DetailValues = null;
				OldDetailsValue = null;
//				DetailsI = null;
				TreesCurrent = null;
				TreesOld = null;
			}
		}
		
		private static void CheckAllTerrains(){
			Object[] tTerrains = GameObject.FindObjectsOfType(typeof(Terrain));	
			GSDTerrain TID;
			GameObject tObj;
			foreach(Terrain tTerrain in tTerrains){
				tObj = tTerrain.transform.gameObject;
				TID = tObj.GetComponent<GSDTerrain>();
				if(TID == null){
					TID = tObj.AddComponent<GSDTerrain>();
				}
				TID.CheckID();
			}
		}

        public static void CheckAllTerrainsHeight0() {
            CheckAllTerrains();
            Object[] tTerrains = GameObject.FindObjectsOfType(typeof(Terrain));
            foreach (Terrain tTerrain in tTerrains) {
                if (!GSDRootUtil.IsApproximately(tTerrain.transform.position.y, 0f, 0.0001f)) {
                    Vector3 tVect = tTerrain.transform.position;
                    tVect.y = 0f;
                    tTerrain.transform.position = tVect;
                }
            }
        }

		public static void ProcessRoad_Terrain_Hook1(GSDSplineC tSpline, GSDRoad tRoad, bool bMultithreaded = true){		
			ProcessRoad_Terrain_Hook1_Do(ref tSpline, ref tRoad, bMultithreaded);
		}
		private static void ProcessRoad_Terrain_Hook1_Do(ref GSDSplineC tSpline,ref GSDRoad tRoad, bool bMultithreaded){			
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("ProcessRoad_Terrain_Hook1_Do"); }
			//First lets make sure all terrains have GSD shit on them:
			CheckAllTerrains();
			
			//Reset the terrain:
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("TerrainsReset"); }
			GSDTerraforming.TerrainsReset(tRoad);
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }

			float heightDistance = tRoad.opt_MatchHeightsDistance;
//			float treeDistance = tRoad.opt_ClearTreesDistance;
			float detailDistance = tRoad.opt_ClearDetailsDistance;
				
			Dictionary<Terrain,TempTerrainData> TempTerrainDict = new Dictionary<Terrain, TempTerrainData>();
			//Populate dictionary:
			Object[] tTerrains = GameObject.FindObjectsOfType(typeof(Terrain));
			GSDTerrain TID;
			int aSize = 0;
			int dSize = 0;
			TempTerrainData TTD;
			bool bContains = false;
			GSDRoadUtil.Construction2DRect tRect = null;
//			GSDRoadUtil.Construction2DRect rRect = null;
			foreach(Terrain tTerrain in tTerrains){
				tRect = GetTerrainBounds(tTerrain);
				bContains = false;
                //Debug.Log(tTerrain.transform.name + " bounds: " + tRect.ToStringGSD());
                //Debug.Log("  Road bounds: " + tSpline.RoadV0 + "," + tSpline.RoadV1 + "," + tSpline.RoadV2 + "," + tSpline.RoadV3);

                if (bContains != true && tRect.Contains(ref tSpline.RoadV0)) {
                    bContains = true;
                } else if (bContains != true && tRect.Contains(ref tSpline.RoadV1)) {
                    bContains = true;
                } else if (bContains != true && tRect.Contains(ref tSpline.RoadV2)) {
                    bContains = true;
                } else if (bContains != true && tRect.Contains(ref tSpline.RoadV3)) {
                    bContains = true;
                } else {
                    int mCount3 = tRoad.GSDSpline.GetNodeCount();
                    Vector2 tVect2D_321 = default(Vector2);
                    for (int i = 0; i < mCount3; i++) {
                        tVect2D_321 = new Vector2(tRoad.GSDSpline.mNodes[i].pos.x, tRoad.GSDSpline.mNodes[i].pos.z);
                        if (tRect.Contains(ref tVect2D_321)) {
                            bContains = true;
                            break;
                        }
                    }

                    if (!bContains) {
                        float tDef = 5f / tSpline.distance;
                        Vector2 x2D = default(Vector2);
                        Vector3 x3D = default(Vector3);
                        for (float i = 0f; i <= 1f; i += tDef) {
                            x3D = tSpline.GetSplineValue(i);
                            x2D = new Vector2(x3D.x, x3D.z);
                            if (tRect.Contains(ref x2D)) {
                                bContains = true;
                                break;
                            }
                        }
                    }
                }
				
//				rRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(tSpline.RoadV0,tSpline.RoadV1,tSpline.RoadV2,tSpline.RoadV3);
				
				
				if(bContains && !TempTerrainDict.ContainsKey(tTerrain)){
					TTD = new TempTerrainData();
					TTD.HM = tTerrain.terrainData.heightmapResolution;			
					TTD.HMHeight = tTerrain.terrainData.heightmapHeight;
					TTD.heights = tTerrain.terrainData.GetHeights(0,0,tTerrain.terrainData.heightmapWidth,tTerrain.terrainData.heightmapHeight);
					TTD.HMRatio = TTD.HM / tTerrain.terrainData.size.x;
					TTD.MetersPerHM = tTerrain.terrainData.size.x / tTerrain.terrainData.heightmapResolution;
					float DetailRatio = tTerrain.terrainData.detailResolution / tTerrain.terrainData.size.x;
					
					//Heights:
					if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("Heights"); }
					if(tRoad.opt_HeightModEnabled){
						aSize = (int)tSpline.distance * ((int)(heightDistance*1.65f*TTD.HMRatio)+2);
						if(aSize > (tTerrain.terrainData.heightmapResolution * tTerrain.terrainData.heightmapResolution)){
							aSize = tTerrain.terrainData.heightmapResolution * tTerrain.terrainData.heightmapResolution;	
						}
						TTD.cX = new ushort[aSize];
						TTD.cY = new ushort[aSize];
						TTD.oldH = new float[aSize];
						TTD.cH = new float[aSize];		
						TTD.cI = 0;
						TTD.TerrainMaxIndex = tTerrain.terrainData.heightmapResolution;
						TTD.TerrainSize = tTerrain.terrainData.size;
						TTD.TerrainPos = tTerrain.transform.position;
						TTD.tHeights = new bool[tTerrain.terrainData.heightmapWidth,tTerrain.terrainData.heightmapHeight];
						TID = tTerrain.transform.gameObject.GetComponent<GSDTerrain>();
						if(TID != null){
							TTD.GSDID = TID.GSDID;
							TempTerrainDict.Add(tTerrain,TTD);	
						}
					}
					if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			
					//Details:
					if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("Details"); }
					if(tRoad.opt_DetailModEnabled){
//						TTD.DetailValues = new Dictionary<int, int[,]>();
						TTD.DetailLayersCount = tTerrain.terrainData.detailPrototypes.Length;	
//						TTD.DetailHasProcessed = new Dictionary<int, bool[,]>();
						TTD.DetailHasProcessed = new HashSet<int>();
						TTD.MainDetailsX = new List<ushort>();
						TTD.MainDetailsY = new List<ushort>();
						TTD.DetailsI = new int[TTD.DetailLayersCount];
						TTD.DetailToHeightRatio = (float)((float)tTerrain.terrainData.detailResolution) / ((float)tTerrain.terrainData.heightmapResolution);
						TTD.DetailMaxIndex = tTerrain.terrainData.detailResolution;
						TTD.DetailLayersSkip = new HashSet<int>();
						
						// Get all of layer zero.
//						int[] mMinMaxDetailEntryCount = new int[TTD.DetailLayersCount];
//						if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("DetailValues"); }
//						Vector3 bVect = default(Vector3);
//						Vector2 bVect2D = default(Vector2);
//						int DetailRes = tTerrain.terrainData.detailResolution;
//						for(int i=0;i<TTD.DetailLayersCount;i++){
//							int[,] tInts = tTerrain.terrainData.GetDetailLayer(0,0,tTerrain.terrainData.detailWidth,tTerrain.terrainData.detailHeight,i);
//							int Length1 = tInts.GetLength(0);
//							int Length2 = tInts.GetLength(1);
//							for (int y=0;y < Length1;y++){
//								for (int x=0;x < Length2;x++){
//									if(tInts[x,y] > 0){
//										bVect = new Vector3(((float)y/(float)DetailRes) * TTD.TerrainSize.x,0f,((float)x/(float)DetailRes) * TTD.TerrainSize.z);
//										bVect = tTerrain.transform.TransformPoint(bVect);
//										bVect2D = new Vector2(bVect.z,bVect.x);
//										if(rRect.Contains(ref bVect2D)){
//											mMinMaxDetailEntryCount[i] += 1;
//										}
//									}
//								}
//							}
							
//							if(mMinMaxDetailEntryCount[i] < 1){
//								TTD.DetailLayersSkip.Add(i);
//								tInts = null;
//							}else{
//								TTD.DetailValues.Add(i,tInts);
//								TTD.DetailHasProcessed.Add(i,new bool[tTerrain.terrainData.detailWidth,tTerrain.terrainData.detailHeight]);
//							}
//						}
//						if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
						

						dSize = (int)tSpline.distance * ((int)(detailDistance*3f*DetailRatio)+2);
						if(dSize > (tTerrain.terrainData.detailResolution * tTerrain.terrainData.detailResolution)){
							dSize = tTerrain.terrainData.detailResolution * tTerrain.terrainData.detailResolution;	
						}
						
//						TTD.DetailsX = new List<ushort[]>();
//						TTD.DetailsY = new List<ushort[]>();
//						TTD.OldDetailsValue = new List<ushort[]>();
						TTD.DetailsX = new List<List<ushort>>();
						TTD.DetailsY = new List<List<ushort>>();
						TTD.OldDetailsValue = new List<List<ushort>>();
//						TTD.DetailHasProcessed = new List<List<bool>>();
						
						for(int i=0;i<TTD.DetailLayersCount;i++){
//							if(TTD.DetailLayersSkip.Contains(i)){ 
//								TTD.DetailsX.Add(new ushort[0]);
//								TTD.DetailsY.Add(new ushort[0]);
//								TTD.OldDetailsValue.Add(new ushort[0]);
//								continue; 
//							}
//							int detailentrycount = (int)((float)mMinMaxDetailEntryCount[i] * 1.5f);
//							int d_temp_Size = dSize;
//							if(d_temp_Size > detailentrycount){ d_temp_Size = detailentrycount; }
//							if(d_temp_Size < 1){ d_temp_Size = 1; }
//							if(d_temp_Size > (tTerrain.terrainData.detailResolution * tTerrain.terrainData.detailResolution)){
//								d_temp_Size = tTerrain.terrainData.detailResolution * tTerrain.terrainData.detailResolution;	
//							}
//
//							TTD.DetailsX.Add(new ushort[d_temp_Size]);
//							TTD.DetailsY.Add(new ushort[d_temp_Size]);
//							TTD.OldDetailsValue.Add(new ushort[d_temp_Size]);
							
							TTD.DetailsX.Add(new List<ushort>());
							TTD.DetailsY.Add(new List<ushort>());
							TTD.OldDetailsValue.Add(new List<ushort>());
						}
						
						
//						TTD.DetailsX = new ushort[TTD.DetailLayersCount,dSize];
//						TTD.DetailsY = new ushort[TTD.DetailLayersCount,dSize];
//						TTD.OldDetailsValue = new ushort[TTD.DetailLayersCount,dSize];
				

					}
					if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
					
					//Trees:
					if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("Trees"); }
					if(tRoad.opt_TreeModEnabled){
//						TTD.TreesCurrent = tTerrain.terrainData.treeInstances;
						TTD.TreesCurrent = new List<TreeInstance>(tTerrain.terrainData.treeInstances);
						TTD.TreeSize = TTD.TreesCurrent.Count;
						TTD.TreesI = 0;
						TTD.TreesOld = new List<TreeInstance>();
					}
					if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
				}
			}
				
			//Figure out relevant TTD to spline:
			List<TempTerrainData> EditorTTDList = new List<TempTerrainData>();
			if(TempTerrainDict != null){
				foreach(Terrain tTerrain in tTerrains){
					if(TempTerrainDict.ContainsKey(tTerrain)){
						EditorTTDList.Add(TempTerrainDict[tTerrain]);
					}
				}
			}
			
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
				
			//Start job now, for each relevant TTD:
			tRoad.EditorTerrainCalcs(ref EditorTTDList);
			if(bMultithreaded){
				GSD.Threaded.TerrainCalcs tJob = new GSD.Threaded.TerrainCalcs();
				tJob.Setup(ref EditorTTDList,tSpline,tRoad);
				tRoad.TerrainCalcsJob = tJob;
				tJob.Start();
			}else{
				GSD.Threaded.TerrainCalcs_Static.RunMe(ref EditorTTDList,tSpline,tRoad);
			}
		}
			
		public static GSDRoadUtil.Construction2DRect GetTerrainBounds(Terrain tTerrain){
			float terrainWidth = tTerrain.terrainData.size.x;
			float terrainLength = tTerrain.terrainData.size.z;
//			Vector3 tPos = tTerrain.transform.TransformPoint(tTerrain.transform.position);

            Vector3 X0 = new Vector3(0f, 0f, 0f);
            Vector3 X1 = new Vector3(terrainWidth, 0f, 0f);
            Vector3 X2 = new Vector3(terrainWidth, 0f, terrainLength);
            Vector3 X3 = new Vector3(0f, 0f, terrainLength);

            X0 = tTerrain.transform.TransformPoint(X0);
            X1 = tTerrain.transform.TransformPoint(X1);
            X2 = tTerrain.transform.TransformPoint(X2);
            X3 = tTerrain.transform.TransformPoint(X3);

            Vector2 P0 = new Vector2(X0.x,X0.z);
            Vector2 P1 = new Vector2(X1.x, X1.z);
            Vector2 P2 = new Vector2(X2.x, X2.z);
            Vector2 P3 = new Vector2(X3.x, X3.z);


            //OLD CODE:
            //Vector2 P0 = new Vector2(0f, 0f);
            //Vector2 P1 = new Vector2(terrainWidth, 0f);
            //Vector2 P2 = new Vector2(terrainWidth, terrainLength);
            //Vector2 P3 = new Vector2(0f, terrainLength);

            //P0 = tTerrain.transform.TransformPoint(P0);
            //P1 = tTerrain.transform.TransformPoint(P1);
            //P2 = tTerrain.transform.TransformPoint(P2);
            //P3 = tTerrain.transform.TransformPoint(P3);
			
			return new GSDRoadUtil.Construction2DRect(P0,P1,P2,P3,tTerrain.transform.position.y);
		}
		
		public static void ProcessRoad_Terrain_Hook2(GSDSplineC tSpline,ref List<TempTerrainData> TTDList){
			if(tSpline.tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("ProcessRoad_Terrain_Hook2"); }
			ProcessRoad_Terrain_Hook2_Do(ref tSpline, ref TTDList);
			if(tSpline.tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
		}
		private static void ProcessRoad_Terrain_Hook2_Do(ref GSDSplineC tSpline,ref List<TempTerrainData> TTDList){
			if(!tSpline.tRoad.opt_TreeModEnabled && !tSpline.tRoad.opt_HeightModEnabled && !tSpline.tRoad.opt_DetailModEnabled){
				//Exit if no mod taking place.
				return;
			}
			Object[] TIDs = GameObject.FindObjectsOfType(typeof(GSDTerrain));
			Terrain tTerrain;
			int[,] tDetails = null;
			int IntBufferX = 0;
			int IntBufferY = 0;
			int tVal = 0;
//			ushort Invalid = 16384;
			foreach(TempTerrainData TTD in TTDList){
				foreach(GSDTerrain TID in TIDs){
					if(TID.GSDID == TTD.GSDID){
						tTerrain = TID.transform.gameObject.GetComponent<Terrain>();
						if(tTerrain != null){
							//Details:
							if(tSpline.tRoad.opt_DetailModEnabled){
								for(int i=0;i<TTD.DetailLayersCount;i++){
//									if(TTD.DetailLayersSkip.Contains(i) || TTD.DetailValues[i] == null){ continue; }
//									if(TTD.DetailsI[i] > 0){
//										tTerrain.terrainData.SetDetailLayer(0,0,i,TTD.DetailValues[i]);	
//									}
									
									if(TTD.DetailLayersSkip.Contains(i) || TTD.MainDetailsX == null || TTD.MainDetailsX.Count < 1){ continue; }
									tDetails = tTerrain.terrainData.GetDetailLayer(0,0,TTD.DetailMaxIndex,TTD.DetailMaxIndex,i);
									
									int MaxCount = TTD.MainDetailsX.Count;
									for(int j=0;j<MaxCount;j++){
										IntBufferX = TTD.MainDetailsX[j];
										IntBufferY = TTD.MainDetailsY[j];
										tVal = tDetails[IntBufferX,IntBufferY];
										if(tVal > 0){
											TTD.DetailsX[i].Add((ushort)IntBufferX);
											TTD.DetailsY[i].Add((ushort)IntBufferY);
											TTD.OldDetailsValue[i].Add((ushort)tVal);
											tDetails[IntBufferX,IntBufferY] = 0;
										}
									}
									TTD.DetailsI[i] = TTD.DetailsX[i].Count;
									
									tTerrain.terrainData.SetDetailLayer(0,0,i,tDetails);
									tDetails = null;
									TTD.DetailHasProcessed = null;
								}
								TTD.MainDetailsX = null;
								TTD.MainDetailsY = null;
								System.GC.Collect();
							}
							//Trees:
							if(tSpline.tRoad.opt_TreeModEnabled && TTD.TreesCurrent != null && TTD.TreesI > 0){
								tTerrain.terrainData.treeInstances = TTD.TreesCurrent.ToArray();
							}
							//Heights:
							if(tSpline.tRoad.opt_HeightModEnabled && TTD.heights != null && TTD.cI > 0){
								//Do heights last to trigger collisions and stuff properly:
								tTerrain.terrainData.SetHeights(0,0,TTD.heights);
							}
						}
					}
				}
			}
		}
		
		public static void TerrainsReset(GSDRoad tRoad){
			TerrainsReset_Do(ref tRoad);
		}
		private static void TerrainsReset_Do(ref GSDRoad tRoad){
			if(tRoad.TerrainHistory == null){ return; }
			if(tRoad.TerrainHistory.Count < 1){ return; }
		
			Object[] TIDs = GameObject.FindObjectsOfType(typeof(GSDTerrain));
			float[,] heights;
			int[,] tDetails;
			int ArrayCount;
			foreach(GSDTerrainHistoryMaker TH in tRoad.TerrainHistory){
				Terrain tTerrain = null;
				foreach(GSDTerrain TID in TIDs){
					if(TID.GSDID == TH.TID){
						tTerrain = TID.tTerrain;
					}
				}
				if(!tTerrain){
					continue;		
				}
					
				//Heights:
				if(TH.x1 != null){
					heights = tTerrain.terrainData.GetHeights(0,0,tTerrain.terrainData.heightmapWidth,tTerrain.terrainData.heightmapHeight);
					ArrayCount = TH.cI;
					for(int i=0;i<ArrayCount;i++){
						heights[TH.x1[i],TH.y1[i]] = TH.h[i];
					}
					tTerrain.terrainData.SetHeights(0,0,heights);
				}
				//Details:
				if(TH.DetailsI != null && TH.DetailsX != null && TH.DetailsY != null && TH.DetailsOldValue != null){
					int RealLayerCount = tTerrain.terrainData.detailPrototypes.Length;
					int StartIndex = 0;
					int EndIndex = 0;
					for(int i=0;i<TH.DetailLayersCount;i++){
						if(i >= RealLayerCount){ break; }
						if(TH.DetailsX.Length <= i){ break; }
						if(TH.DetailsY.Length <= i){ break; }
						if(TH.DetailsX == null || TH.DetailsY == null || TH.DetailsI == null || TH.DetailsX.Length < 1){
							continue;
						}

						tDetails = tTerrain.terrainData.GetDetailLayer(0,0,tTerrain.terrainData.detailWidth,tTerrain.terrainData.detailHeight,i);
						ArrayCount = TH.DetailsI[i];
						if(ArrayCount == 0){ continue; }
						EndIndex += ArrayCount;
						for(int j=StartIndex;j<EndIndex;j++){
							tDetails[TH.DetailsX[j],TH.DetailsY[j]] = TH.DetailsOldValue[j];
						}
						StartIndex = EndIndex;
						tTerrain.terrainData.SetDetailLayer(0,0,i,tDetails);
						tDetails = null;
					}
				}
				//Trees:
				TreeInstance[] xTress = TH.MakeTrees();
				if(xTress != null){
					ArrayCount = xTress.Length;
					if(ArrayCount > 0 && TH.TreesOld != null){
						int TerrainTreeCount = tTerrain.terrainData.treeInstances.Length;;
						TreeInstance[] tTrees = new TreeInstance[ArrayCount+TerrainTreeCount];
						System.Array.Copy(tTerrain.terrainData.treeInstances,0,tTrees,0,TerrainTreeCount);
						System.Array.Copy(xTress,0,tTrees,TerrainTreeCount,ArrayCount);
						tTerrain.terrainData.treeInstances = tTrees;
					}
					xTress = null;
				}
			}
			System.GC.Collect();
		}	
	}

	[System.Serializable]
	public class GSDTerrainHistoryMaker{
		public int TID;
		//Heights:
		public int[] x1;
		public int[] y1;
		public float[] h;
		public int cI;
		public bool bHeightHistoryEnabled;
		//Details:
		public int DetailLayersCount;
		
		public int[] DetailsX;
		public int[] DetailsY;
		public int[] DetailsOldValue;
		public int[] DetailsI;
		
		public bool bDetailHistoryEnabled;
		//Trees:
		public GSDTreeInstance[] TreesOld;
		public int TreesI;
		public bool bTreeHistoryEnabled;
		public bool bDestroyMe = false;
		
		public void Nullify(){
			//Heights:
			x1 = null;
			y1 = null;
			h = null;
			DetailsX = null;
			DetailsY = null;
			DetailsOldValue = null;
			DetailsI = null;
			//Trees:
			TreesOld = null;
		}
		
		[System.Serializable]
		public class GSDTreeInstance{
			public float colorR;//4
			public float colorG;//8
			public float colorB;//12
			public float colorA;//16
			public float heightScale;//20
			public float lightmapColorR;//24
			public float lightmapColorG;//28
			public float lightmapColorB;//32
			public float lightmapColorA;//36
			public float positionX;//40
			public float positionY;//44
			public float positionZ;//48
			public int prototypeIndex;//52
			public float widthScale;//56
		}
		
		public void MakeGSDTrees(ref List<TreeInstance> tTrees){
			int tSize = tTrees.Count;	
			TreesOld = new GSDTreeInstance[tSize];
			GSDTreeInstance tTree = null;
			TreeInstance xTree;
			for(int i=0;i<tSize;i++){
				xTree = tTrees[i];
				tTree = new GSDTreeInstance();
				tTree.colorR = xTree.color.r;
				tTree.colorG = xTree.color.g;
				tTree.colorB = xTree.color.b;
				tTree.colorA = xTree.color.a;
				tTree.heightScale = xTree.heightScale;
				tTree.lightmapColorR = xTree.lightmapColor.r;
				tTree.lightmapColorG = xTree.lightmapColor.g;
				tTree.lightmapColorB = xTree.lightmapColor.b;
				tTree.lightmapColorA = xTree.lightmapColor.a;
				tTree.positionX = xTree.position.x;
				tTree.positionY = xTree.position.y;
				tTree.positionZ = xTree.position.z;
				tTree.prototypeIndex = xTree.prototypeIndex;
				tTree.widthScale = xTree.widthScale;
				TreesOld[i] = tTree;
			}
		}
		
		public TreeInstance[] MakeTrees(){
			if(TreesOld == null || TreesOld.Length < 1){ return null; }
			int tSize = TreesOld.Length;	
			TreeInstance[] tTrees = new TreeInstance[tSize];
			GSDTreeInstance tTree = null;
			TreeInstance xTree;
			for(int i=0;i<tSize;i++){
				tTree = TreesOld[i];
				xTree = new TreeInstance();
				xTree.color = new Color(tTree.colorR,tTree.colorG,tTree.colorB,tTree.colorA);
				xTree.heightScale = tTree.heightScale;
				xTree.lightmapColor = new Color(tTree.lightmapColorR,tTree.lightmapColorG,tTree.lightmapColorB,tTree.lightmapColorA);
				xTree.position = new Vector3(tTree.positionX,tTree.positionY,tTree.positionZ);
				xTree.prototypeIndex = tTree.prototypeIndex;
				xTree.widthScale = tTree.widthScale;
				tTrees[i] = xTree;
			}
			return tTrees;
		}
		
		public int GetSize(){
			int tSize = 4;
			if(x1 != null){ tSize += (x1.Length*4); tSize += 20; }
			if(y1 != null){ tSize += (y1.Length*4); tSize += 20; }
			if(h != null){ tSize += (h.Length*4); tSize += 20; }
			tSize += 4;
			tSize += 1;
			//Details:
			tSize += 4;
			if(DetailsX != null){ tSize += (DetailsX.Length*4); tSize += 20; }
			if(DetailsY != null){ tSize += (DetailsY.Length*4); tSize += 20; }
			if(DetailsOldValue != null){ tSize += (DetailsOldValue.Length*4); tSize += 20; }
			if(DetailsI != null){ tSize += (DetailsI.Length*4); tSize += 20; }
			tSize += 1;
			//Trees:
			if(TreesOld != null){ tSize += (TreesOld.Length * 56); tSize += 20; }
			tSize += 4;
			tSize += 1;
			tSize += 1;
			
			return tSize;
		}
	}

	public class RoadConstructorBufferMaker{
		public GSDRoad tRoad;
		
		public List<Vector3> RoadVectors;
		public List<Vector3> ShoulderR_Vectors;
		public List<Vector3> ShoulderL_Vectors;

		public int[] tris;
		public int[] tris_ShoulderR;
		public int[] tris_ShoulderL;

		public Vector3[] normals;
		public Vector3[] normals_ShoulderR;
		public Vector3[] normals_ShoulderL;
		public List<int> normals_ShoulderR_averageStartIndexes;
		public List<int> normals_ShoulderL_averageStartIndexes;
		
		public Vector2[] uv;
		public Vector2[] uv2;
		public Vector2[] uv_SR;
		public Vector2[] uv_SL;

		public Vector4[] tangents;
		public Vector4[] tangents2;
		public Vector4[] tangents_SR;
		public Vector4[] tangents_SL;
		
		public List<List<Vector3>> cut_RoadVectors;
		public List<Vector3> cut_RoadVectorsHome;
		public List<List<Vector3>> cut_ShoulderR_Vectors;
		public List<List<Vector3>> cut_ShoulderL_Vectors;
		public List<Vector3> cut_ShoulderR_VectorsHome;
		public List<Vector3> cut_ShoulderL_VectorsHome;
		public List<int[]> cut_tris;
		public List<int[]> cut_tris_ShoulderR;
		public List<int[]> cut_tris_ShoulderL;
		public List<Vector3[]> cut_normals;
		public List<Vector3[]> cut_normals_ShoulderR;
		public List<Vector3[]> cut_normals_ShoulderL;
		public List<Vector2[]> cut_uv;
		public List<Vector2[]> cut_uv_SR;
		public List<Vector2[]> cut_uv_SL;
		public List<Vector4[]> cut_tangents;
		public List<Vector4[]> cut_tangents_SR;
		public List<Vector4[]> cut_tangents_SL;
		
		public List<Vector2[]> cut_uv_world;
		public List<Vector2[]> cut_uv_SR_world;
		public List<Vector2[]> cut_uv_SL_world;
		public List<Vector4[]> cut_tangents_world;
		public List<Vector4[]> cut_tangents_SR_world;
		public List<Vector4[]> cut_tangents_SL_world;

		//Road connections:
		public List<Vector3[]> RoadConnections_verts;
		public List<int[]> RoadConnections_tris;
		public List<Vector3[]> RoadConnections_normals;
		public List<Vector2[]> RoadConnections_uv;
		public List<Vector4[]> RoadConnections_tangents;
		
		//Back lanes:
		public List<Vector3[]> iBLane0s;
		public List<Vector3[]> iBLane1s;
		public List<bool> iBLane1s_IsMiddleLane;
		public List<Vector3[]> iBLane2s;
		public List<Vector3[]> iBLane3s;
		//Front lanes:
		public List<Vector3[]> iFLane0s;
		public List<Vector3[]> iFLane1s;
		public List<bool> iFLane1s_IsMiddleLane;
		public List<Vector3[]> iFLane2s;
		public List<Vector3[]> iFLane3s;
		//Main plates:
		public List<Vector3[]> iBMainPlates;
		public List<Vector3[]> iFMainPlates;
		//Marker plates:
		public List<Vector3[]> iBMarkerPlates;
		public List<Vector3[]> iFMarkerPlates;
		
		//Back lanes:
		public List<int[]> iBLane0s_tris;
		public List<int[]> iBLane1s_tris;
		public List<int[]> iBLane2s_tris;
		public List<int[]> iBLane3s_tris;
		//Front lanes:
		public List<int[]> iFLane0s_tris;
		public List<int[]> iFLane1s_tris;
		public List<int[]> iFLane2s_tris;
		public List<int[]> iFLane3s_tris;
		//Main plates:
		public List<int[]> iBMainPlates_tris;
		public List<int[]> iFMainPlates_tris;
		//Marker plates:
		public List<int[]> iBMarkerPlates_tris;
		public List<int[]> iFMarkerPlates_tris;
		
		//Back lanes:
		public List<Vector3[]> iBLane0s_normals;
		public List<Vector3[]> iBLane1s_normals;
		public List<Vector3[]> iBLane2s_normals;
		public List<Vector3[]> iBLane3s_normals;
		//Front lanes:
		public List<Vector3[]> iFLane0s_normals;
		public List<Vector3[]> iFLane1s_normals;
		public List<Vector3[]> iFLane2s_normals;
		public List<Vector3[]> iFLane3s_normals;
		//Main plates:
		public List<Vector3[]> iBMainPlates_normals;
		public List<Vector3[]> iFMainPlates_normals;
		//Marker plates:
		public List<Vector3[]> iBMarkerPlates_normals;
		public List<Vector3[]> iFMarkerPlates_normals;
		
		//Back lanes:
		public List<GSDRoadIntersection> iBLane0s_tID;
		public List<GSDRoadIntersection> iBLane1s_tID;
		public List<GSDRoadIntersection> iBLane2s_tID;
		public List<GSDRoadIntersection> iBLane3s_tID;
		//Front lanes:
		public List<GSDRoadIntersection> iFLane0s_tID;
		public List<GSDRoadIntersection> iFLane1s_tID;
		public List<GSDRoadIntersection> iFLane2s_tID;
		public List<GSDRoadIntersection> iFLane3s_tID;
		//Main plates:
		public List<GSDRoadIntersection> iBMainPlates_tID;
		public List<GSDRoadIntersection> iFMainPlates_tID;
		//Marker plates:
		public List<GSDRoadIntersection> iBMarkerPlates_tID;
		public List<GSDRoadIntersection> iFMarkerPlates_tID;
		
		//Back lanes:
		public List<GSDSplineN> iBLane0s_nID;
		public List<GSDSplineN> iBLane1s_nID;
		public List<GSDSplineN> iBLane2s_nID;
		public List<GSDSplineN> iBLane3s_nID;
		//Front lanes:
		public List<GSDSplineN> iFLane0s_nID;
		public List<GSDSplineN> iFLane1s_nID;
		public List<GSDSplineN> iFLane2s_nID;
		public List<GSDSplineN> iFLane3s_nID;
		//Main plates:
		public List<GSDSplineN> iBMainPlates_nID;
		public List<GSDSplineN> iFMainPlates_nID;
		//Marker plates:
		public List<GSDSplineN> iBMarkerPlates_nID;
		public List<GSDSplineN> iFMarkerPlates_nID;
		
		//Back lanes:
		public List<Vector2[]> iBLane0s_uv;
		public List<Vector2[]> iBLane1s_uv;
		public List<Vector2[]> iBLane2s_uv;
		public List<Vector2[]> iBLane3s_uv;
		//Front lanes:
		public List<Vector2[]> iFLane0s_uv;
		public List<Vector2[]> iFLane1s_uv;
		public List<Vector2[]> iFLane2s_uv;
		public List<Vector2[]> iFLane3s_uv;
		//Main plates:
		public List<Vector2[]> iBMainPlates_uv;
		public List<Vector2[]> iFMainPlates_uv;
		public List<Vector2[]> iBMainPlates_uv2;
		public List<Vector2[]> iFMainPlates_uv2;
		//Marker plates:
		public List<Vector2[]> iBMarkerPlates_uv;
		public List<Vector2[]> iFMarkerPlates_uv;
		
		//Back lanes:
		public List<Vector4[]> iBLane0s_tangents;
		public List<Vector4[]> iBLane1s_tangents;
		public List<Vector4[]> iBLane2s_tangents;
		public List<Vector4[]> iBLane3s_tangents;
		//Front lanes:
		public List<Vector4[]> iFLane0s_tangents;
		public List<Vector4[]> iFLane1s_tangents;
		public List<Vector4[]> iFLane2s_tangents;
		public List<Vector4[]> iFLane3s_tangents;
		//Main plates:
		public List<Vector4[]> iBMainPlates_tangents;
		public List<Vector4[]> iFMainPlates_tangents;
		public List<Vector4[]> iBMainPlates_tangents2;
		public List<Vector4[]> iFMainPlates_tangents2;
		//Marker plates:
		public List<Vector4[]> iBMarkerPlates_tangents;
		public List<Vector4[]> iFMarkerPlates_tangents;
	
		public Terrain tTerrain;
		
		public List<GSD.Roads.GSDRoadUtil.Construction2DRect> tIntersectionBounds;
		public HashSet<Vector3> ImmuneVects;
	
		public Mesh tMesh;
		public Mesh tMesh_SR;
		public Mesh tMesh_SL;
		public bool tMeshSkip = false;
		public bool tMesh_SRSkip = false;
		public bool tMesh_SLSkip = false;
		public List<Mesh> tMesh_RoadCuts;
		public List<Mesh> tMesh_SRCuts;
		public List<Mesh> tMesh_SLCuts;
		public List<Mesh> tMesh_RoadCuts_world;
		public List<Mesh> tMesh_SRCuts_world;
		public List<Mesh> tMesh_SLCuts_world;
		
		public List<Mesh> tMesh_RoadConnections;

		public List<Mesh> tMesh_iBLanes0;
		public List<Mesh> tMesh_iBLanes1;
		public List<Mesh> tMesh_iBLanes2;
		public List<Mesh> tMesh_iBLanes3;
		public List<Mesh> tMesh_iFLanes0;
		public List<Mesh> tMesh_iFLanes1;
		public List<Mesh> tMesh_iFLanes2;
		public List<Mesh> tMesh_iFLanes3;
		public List<Mesh> tMesh_iBMainPlates;
		public List<Mesh> tMesh_iFMainPlates;
		public List<Mesh> tMesh_iBMarkerPlates;
		public List<Mesh> tMesh_iFMarkerPlates;

		public RoadUpdateTypeEnum tUpdateType;
		
		public bool bRoadOn = true;
		public bool bTerrainOn = true;
		public bool bBridgesOn = true;
		public bool bInterseOn = true;
		
		public List<int> RoadCuts;
		public List<GSDSplineN> RoadCutNodes;
		public List<int> ShoulderCutsR;
		public List<GSDSplineN> ShoulderCutsRNodes;
		public List<int> ShoulderCutsL;
		public List<GSDSplineN> ShoulderCutsLNodes;
		
		public enum SaveMeshTypeEnum {Road,Shoulder,Intersection,Railing,Center,Bridge,RoadCut,SCut,BSCut,RoadConn};
		
		public RoadConstructorBufferMaker(GSDRoad _tRoad, RoadUpdateTypeEnum _tUpdateType){
			tUpdateType = _tUpdateType;
			bRoadOn = (tUpdateType == RoadUpdateTypeEnum.Full || tUpdateType == RoadUpdateTypeEnum.Intersection || tUpdateType == RoadUpdateTypeEnum.Bridges);
			bTerrainOn = (tUpdateType == RoadUpdateTypeEnum.Full || tUpdateType == RoadUpdateTypeEnum.Intersection || tUpdateType == RoadUpdateTypeEnum.Bridges);
			bBridgesOn = (tUpdateType == RoadUpdateTypeEnum.Full || tUpdateType == RoadUpdateTypeEnum.Bridges);
			bInterseOn = (tUpdateType == RoadUpdateTypeEnum.Full || tUpdateType == RoadUpdateTypeEnum.Intersection);
			
			tRoad = _tRoad;
			Nullify();
			RoadVectors = new List<Vector3>();
			ShoulderR_Vectors = new List<Vector3>();
			ShoulderL_Vectors = new List<Vector3>();
			normals_ShoulderR_averageStartIndexes = new List<int>();
			normals_ShoulderL_averageStartIndexes = new List<int>();
			
			cut_RoadVectors = new List<List<Vector3>>();
			cut_RoadVectorsHome = new List<Vector3>();
			cut_ShoulderR_Vectors = new List<List<Vector3>>();
			cut_ShoulderL_Vectors = new List<List<Vector3>>();
			cut_ShoulderR_VectorsHome = new List<Vector3>();
			cut_ShoulderL_VectorsHome = new List<Vector3>();
			cut_tris = new List<int[]>();
			cut_tris_ShoulderR = new List<int[]>();
			cut_tris_ShoulderL = new List<int[]>();
			cut_normals = new List<Vector3[]>();
			cut_normals_ShoulderR = new List<Vector3[]>();
			cut_normals_ShoulderL = new List<Vector3[]>();
			cut_uv = new List<Vector2[]>();
			cut_uv_SR = new List<Vector2[]>();
			cut_uv_SL = new List<Vector2[]>();
			cut_tangents = new List<Vector4[]>();
			cut_tangents_SR = new List<Vector4[]>();
			cut_tangents_SL = new List<Vector4[]>();
			
			cut_uv_world = new List<Vector2[]>();
			cut_uv_SR_world = new List<Vector2[]>();
			cut_uv_SL_world = new List<Vector2[]>();
			cut_tangents_world = new List<Vector4[]>();
			cut_tangents_SR_world = new List<Vector4[]>();
			cut_tangents_SL_world = new List<Vector4[]>();
			
			RoadCutNodes = new List<GSDSplineN>();
			ShoulderCutsRNodes = new List<GSDSplineN>();
			ShoulderCutsLNodes = new List<GSDSplineN>();
			
			RoadConnections_verts = new List<Vector3[]>();
		 	RoadConnections_tris = new List<int[]>();
		 	RoadConnections_normals = new List<Vector3[]>();
		 	RoadConnections_uv = new List<Vector2[]>();
		 	RoadConnections_tangents = new List<Vector4[]>();
			
			RoadCuts = new List<int>();
			ShoulderCutsR = new List<int>();
			ShoulderCutsL = new List<int>();
			
			//if(bInterseOn){
				//Back lanes:
				iBLane0s = new List<Vector3[]>();
				iBLane1s = new List<Vector3[]>();
				iBLane2s = new List<Vector3[]>();
				iBLane3s = new List<Vector3[]>();
				//Front lanes:
				iFLane0s = new List<Vector3[]>();
				iFLane1s = new List<Vector3[]>();
				iFLane2s = new List<Vector3[]>();
				iFLane3s = new List<Vector3[]>();
				//Main plates:
				iBMainPlates = new List<Vector3[]>();
				iFMainPlates = new List<Vector3[]>();
				//Marker plates:
				iBMarkerPlates = new List<Vector3[]>();
				iFMarkerPlates = new List<Vector3[]>();
				
				//Back lanes:
				iBLane0s_tris = new List<int[]>();
				iBLane1s_tris = new List<int[]>();
				iBLane2s_tris = new List<int[]>();
				iBLane3s_tris = new List<int[]>();
				//Front lanes:
				iFLane0s_tris = new List<int[]>();
				iFLane1s_tris = new List<int[]>();
				iFLane2s_tris = new List<int[]>();
				iFLane3s_tris = new List<int[]>();
				//Main plates:
				iBMainPlates_tris = new List<int[]>();
				iFMainPlates_tris = new List<int[]>();
				//Marker plates:
				iBMarkerPlates_tris = new List<int[]>();
				iFMarkerPlates_tris = new List<int[]>();
				
				//Back lanes:
				iBLane0s_normals = new List<Vector3[]>();
				iBLane1s_normals = new List<Vector3[]>();
				iBLane2s_normals = new List<Vector3[]>();
				iBLane3s_normals = new List<Vector3[]>();
				//Front lanes:
				iFLane0s_normals = new List<Vector3[]>();
				iFLane1s_normals = new List<Vector3[]>();
				iFLane2s_normals = new List<Vector3[]>();
				iFLane3s_normals = new List<Vector3[]>();
				//Main plates:
				iBMainPlates_normals = new List<Vector3[]>();
				iFMainPlates_normals = new List<Vector3[]>();
				//Marker plates:
				iBMarkerPlates_normals = new List<Vector3[]>();
				iFMarkerPlates_normals = new List<Vector3[]>();
				
				//Back lanes:
				iBLane0s_uv = new List<Vector2[]>();
				iBLane1s_uv = new List<Vector2[]>();
				iBLane2s_uv = new List<Vector2[]>();
				iBLane3s_uv = new List<Vector2[]>();
				//Front lanes:
				iFLane0s_uv = new List<Vector2[]>();
				iFLane1s_uv = new List<Vector2[]>();
				iFLane2s_uv = new List<Vector2[]>();
				iFLane3s_uv = new List<Vector2[]>();
				//Main plates:
				iBMainPlates_uv = new List<Vector2[]>();
				iFMainPlates_uv = new List<Vector2[]>();
				iBMainPlates_uv2 = new List<Vector2[]>();
				iFMainPlates_uv2 = new List<Vector2[]>();
				//Marker plates:
				iBMarkerPlates_uv = new List<Vector2[]>();
				iFMarkerPlates_uv = new List<Vector2[]>();
				
				//Back lanes:
				iBLane0s_tangents = new List<Vector4[]>();
				iBLane1s_tangents = new List<Vector4[]>();
				iBLane2s_tangents = new List<Vector4[]>();
				iBLane3s_tangents = new List<Vector4[]>();
				//Front lanes:
				iFLane0s_tangents = new List<Vector4[]>();
				iFLane1s_tangents = new List<Vector4[]>();
				iFLane2s_tangents = new List<Vector4[]>();
				iFLane3s_tangents = new List<Vector4[]>();
				//Main plates:
				iBMainPlates_tangents = new List<Vector4[]>();
				iFMainPlates_tangents = new List<Vector4[]>();
				iBMainPlates_tangents2 = new List<Vector4[]>();
				iFMainPlates_tangents2 = new List<Vector4[]>();
				//Marker plates:
				iBMarkerPlates_tangents = new List<Vector4[]>();
				iFMarkerPlates_tangents = new List<Vector4[]>();
				
				iFLane1s_IsMiddleLane = new List<bool>();
				iBLane1s_IsMiddleLane = new List<bool>();
		
				//Back lanes:
				iBLane0s_tID = new List<GSDRoadIntersection>();
				iBLane1s_tID = new List<GSDRoadIntersection>();
				iBLane2s_tID = new List<GSDRoadIntersection>();
				iBLane3s_tID = new List<GSDRoadIntersection>();
				//Front lanes:
				iFLane0s_tID = new List<GSDRoadIntersection>();
				iFLane1s_tID = new List<GSDRoadIntersection>();
				iFLane2s_tID = new List<GSDRoadIntersection>();
				iFLane3s_tID = new List<GSDRoadIntersection>();
				//Main plates:
				iBMainPlates_tID = new List<GSDRoadIntersection>();
				iFMainPlates_tID = new List<GSDRoadIntersection>();
				//Marker plates:
				iBMarkerPlates_tID = new List<GSDRoadIntersection>();
				iFMarkerPlates_tID = new List<GSDRoadIntersection>();
			
				iBLane0s_nID = new List<GSDSplineN>();
				iBLane1s_nID = new List<GSDSplineN>();
				iBLane2s_nID = new List<GSDSplineN>();
				iBLane3s_nID = new List<GSDSplineN>();
				//Front lanes:
				iFLane0s_nID = new List<GSDSplineN>();
				iFLane1s_nID = new List<GSDSplineN>();
				iFLane2s_nID = new List<GSDSplineN>();
				iFLane3s_nID = new List<GSDSplineN>();
				//Main plates:
				iBMainPlates_nID = new List<GSDSplineN>();
				iFMainPlates_nID = new List<GSDSplineN>();
				//Marker plates:
				iBMarkerPlates_nID = new List<GSDSplineN>();
				iFMarkerPlates_nID = new List<GSDSplineN>();
			//}
			
			tTerrain = null;
			
			tMesh = new Mesh();
			tMesh_SR = new Mesh();
			tMesh_SL = new Mesh();
			tMesh_RoadCuts = new List<Mesh>();
			tMesh_SRCuts = new List<Mesh>();
			tMesh_SLCuts = new List<Mesh>();
			tMesh_RoadCuts_world = new List<Mesh>();
			tMesh_SRCuts_world = new List<Mesh>();
			tMesh_SLCuts_world = new List<Mesh>();
			
			tMesh_RoadConnections = new List<Mesh>();
			
			//if(bInterseOn){
				tMesh_iBLanes0 = new List<Mesh>();
				tMesh_iBLanes1 = new List<Mesh>();
				tMesh_iBLanes2 = new List<Mesh>();
				tMesh_iBLanes3 = new List<Mesh>();
				tMesh_iFLanes0 = new List<Mesh>();
				tMesh_iFLanes1 = new List<Mesh>();
				tMesh_iFLanes2 = new List<Mesh>();
				tMesh_iFLanes3 = new List<Mesh>();
				tMesh_iBMainPlates = new List<Mesh>();
				tMesh_iFMainPlates = new List<Mesh>();
				tMesh_iBMarkerPlates = new List<Mesh>();
				tMesh_iFMarkerPlates = new List<Mesh>();
				tIntersectionBounds = new List<GSD.Roads.GSDRoadUtil.Construction2DRect>();
				ImmuneVects = new HashSet<Vector3>();
			//}

			InitGameObjects();
		}
		
		#region "Init and nullify"
		private void InitGameObjects(){
			//Destry past objects:
			if(tRoad.MainMeshes != null){
				MeshFilter[] MFArray = tRoad.MainMeshes.GetComponentsInChildren<MeshFilter>();
				MeshCollider[] MCArray = tRoad.MainMeshes.GetComponentsInChildren<MeshCollider>();
				
				int MFArrayCount = MFArray.Length;
				int MCArrayCount = MCArray.Length;
				for(int i=(MFArrayCount-1);i>-1;i--){
					MFArray[i].sharedMesh = null;	
				}
				for(int i=(MCArrayCount-1);i>-1;i--){
					MCArray[i].sharedMesh = null;	
				}
				
				Object.DestroyImmediate(tRoad.MainMeshes);
			}

			//Main mesh object:
			tRoad.MainMeshes = new GameObject("MainMeshes");
			tRoad.MainMeshes.transform.parent = tRoad.transform;
			
			//Road and shoulders:
			tRoad.MeshRoad = new GameObject("RoadMesh");
			tRoad.MeshShoR = new GameObject("ShoulderR");
			tRoad.MeshShoL = new GameObject("ShoulderL");
			tRoad.MeshRoad.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshShoR.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshShoL.transform.parent = tRoad.MainMeshes.transform;
			
			//Intersections:
			tRoad.MeshiLanes = new GameObject("MeshiLanes");
			tRoad.MeshiLanes0 = new GameObject("MeshiLanes0");
			tRoad.MeshiLanes1 = new GameObject("MeshiLanes1");
			tRoad.MeshiLanes2 = new GameObject("MeshiLanes2");
			tRoad.MeshiLanes3 = new GameObject("MeshiLanes3");
			tRoad.MeshiMainPlates = new GameObject("MeshiMainPlates");
			tRoad.MeshiMarkerPlates = new GameObject("MeshiMarkerPlates");
			tRoad.MeshiLanes.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshiLanes0.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshiLanes1.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshiLanes2.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshiLanes3.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshiMainPlates.transform.parent = tRoad.MainMeshes.transform;
			tRoad.MeshiMarkerPlates.transform.parent = tRoad.MainMeshes.transform;
		}
		
		public void Nullify(){
			RoadVectors = null;
			ShoulderR_Vectors = null;
			ShoulderL_Vectors = null;
			tris = null;
			normals = null;
			uv = null;
			uv_SR = null;
			uv_SL = null;
			tangents = null;
			tangents_SR = null;
			tangents_SL = null;
			tTerrain = null;
			tIntersectionBounds = null;
			ImmuneVects = null;
			iBLane0s = null;
			iBLane1s = null;
			iBLane2s = null;
			iBLane3s = null;
			iFLane0s = null;
			iFLane1s = null;
			iFLane2s = null;
			iFLane3s = null;
			iBMainPlates = null;
			iFMainPlates = null;
			iBMarkerPlates = null;
			iFMarkerPlates = null;
			tMesh = null;
			tMesh_SR = null;
			tMesh_SL = null;
			if(tMesh_iBLanes0 != null){ tMesh_iBLanes0.Clear(); tMesh_iBLanes0 = null; }
			if(tMesh_iBLanes1 != null){ tMesh_iBLanes1.Clear(); tMesh_iBLanes1 = null; }
			if(tMesh_iBLanes2 != null){ tMesh_iBLanes2.Clear(); tMesh_iBLanes2 = null; }
			if(tMesh_iBLanes3 != null){ tMesh_iBLanes3.Clear(); tMesh_iBLanes3 = null; }
			if(tMesh_iFLanes0 != null){ tMesh_iFLanes0.Clear(); tMesh_iFLanes0 = null; }
			if(tMesh_iFLanes1 != null){ tMesh_iFLanes1.Clear(); tMesh_iFLanes1 = null; }
			if(tMesh_iFLanes2 != null){ tMesh_iFLanes2.Clear(); tMesh_iFLanes2 = null; }
			if(tMesh_iFLanes3 != null){ tMesh_iFLanes3.Clear(); tMesh_iFLanes3 = null; }
			if(tMesh_iBMainPlates != null){ tMesh_iBMainPlates.Clear(); tMesh_iBMainPlates = null; }
			if(tMesh_iFMainPlates != null){ tMesh_iFMainPlates.Clear(); tMesh_iFMainPlates = null; }
			if(tMesh_iBMarkerPlates != null){ tMesh_iBMarkerPlates.Clear(); tMesh_iBMarkerPlates = null; }
			if(tMesh_iFMarkerPlates != null){ tMesh_iFMarkerPlates.Clear(); tMesh_iFMarkerPlates = null; }
			tMesh_RoadConnections = null;
			
			iFLane1s_IsMiddleLane = null;
			iBLane1s_IsMiddleLane = null;
			
			RoadConnections_verts = null;
		 	RoadConnections_tris = null;
		 	RoadConnections_normals = null;
		 	RoadConnections_uv = null;
		 	RoadConnections_tangents = null;
			
			if(cut_uv_world != null){ cut_uv_world.Clear(); cut_uv_world = null; }
			if(cut_uv_SR_world != null){ cut_uv_SR_world.Clear(); cut_uv_SR_world = null; }
			if(cut_uv_SL_world != null){ cut_uv_SL_world.Clear(); cut_uv_SL_world = null; }
			if(cut_tangents_world != null){ cut_tangents_world.Clear(); cut_tangents_world = null; }
			if(cut_tangents_SR_world != null){ cut_tangents_SR_world.Clear(); cut_tangents_SR_world = null; }
			if(cut_tangents_SL_world != null){ cut_tangents_SL_world.Clear(); cut_tangents_SL_world = null; }
			
			
			tMesh = null; 
			tMesh_SR = null; 
			tMesh_SL = null; 
			tMesh_SR = null; 
			tMesh_SL = null; 
			tMesh_RoadCuts = null; 
			tMesh_SRCuts = null; 
			tMesh_SLCuts = null; 
			tMesh_RoadCuts_world = null; 
			tMesh_SRCuts_world = null; 
			tMesh_SLCuts_world = null; 
		}
		#endregion
		
		#region "Mesh Setup1"	
		public void MeshSetup1(){
			MeshSetup1_Do();
		}
	
		/// <summary>
		/// Creates meshes and assigns vertices, triangles and normals. If multithreading enabled, this occurs inbetween threaded jobs since unity library can't be used in threads.
		/// </summary>
		private void MeshSetup1_Do(){
			Mesh MeshBuffer = null;
			
			if(bInterseOn){
				MeshSetup1_IntersectionObjectsSetup();
			}
			
			if(bRoadOn){
				//Main road:
				if(RoadVectors.Count < 64000){
					if(tMesh == null){ tMesh = new Mesh(); }
					tMesh = MeshSetup1_Helper(ref tMesh,RoadVectors.ToArray(),ref tris, ref normals);
					tMeshSkip = false;
				}else{
					tMeshSkip = true;
				}

				//Right shoulder:
				if(ShoulderR_Vectors.Count < 64000){
					if(tMesh_SR == null){ tMesh_SR = new Mesh(); }
					tMesh_SR = MeshSetup1_Helper(ref tMesh_SR, ShoulderR_Vectors.ToArray(),ref tris_ShoulderR, ref normals_ShoulderR);
					tMesh_SRSkip = false;
				}else{
					tMesh_SRSkip = true;
				}
				
				//Left shoulder:
				if(ShoulderL_Vectors.Count < 64000){
					if(tMesh_SL == null){ tMesh_SL = new Mesh(); }
					tMesh_SL = MeshSetup1_Helper(ref tMesh_SL, ShoulderL_Vectors.ToArray(),ref tris_ShoulderL, ref normals_ShoulderL);
					tMesh_SLSkip = false;
				}else{
					tMesh_SLSkip = true;
				}
				
				if(RoadConnections_verts.Count > 0){
					Mesh qMesh = null;
					for(int i=0;i<RoadConnections_verts.Count;i++){
						qMesh = new Mesh();
						qMesh.vertices = RoadConnections_verts[i];
						qMesh.triangles = RoadConnections_tris[i];
						qMesh.normals = RoadConnections_normals[i];
						qMesh.uv = RoadConnections_uv[i];
						qMesh.RecalculateNormals();
						RoadConnections_normals[i] = qMesh.normals;
						tMesh_RoadConnections.Add(qMesh);
					}
				}
				
				
				if((tRoad.opt_bRoadCuts || tRoad.opt_bDynamicCuts) && RoadCuts.Count > 0){
					int[] tTris = null;
					Vector3[] tNormals = null;
					int cCount = cut_RoadVectors.Count;
					for(int i=0;i<cCount;i++){
						tTris = cut_tris[i];
						tNormals = cut_normals[i];
						MeshBuffer = new Mesh();
						tMesh_RoadCuts.Add(MeshSetup1_Helper(ref MeshBuffer, cut_RoadVectors[i].ToArray(),ref tTris, ref tNormals));
						MeshBuffer = new Mesh();
						tMesh_RoadCuts_world.Add(MeshSetup1_Helper(ref MeshBuffer, cut_RoadVectors[i].ToArray(),ref tTris, ref tNormals));
						cut_normals[i] = tNormals;
						tMeshSkip = true;
					}
				}
				if(tRoad.opt_bShoulderCuts || tRoad.opt_bDynamicCuts){
					int[] tTris = null;
					Vector3[] tNormals = null;
					int rCount = cut_ShoulderR_Vectors.Count;
					for(int i=0;i<rCount;i++){
						tTris = cut_tris_ShoulderR[i];
						tNormals = cut_normals_ShoulderR[i];
						MeshBuffer = new Mesh();
						tMesh_SRCuts.Add(MeshSetup1_Helper(ref MeshBuffer, cut_ShoulderR_Vectors[i].ToArray(),ref tTris, ref tNormals));
						MeshBuffer = new Mesh();
						tMesh_SRCuts_world.Add(MeshSetup1_Helper(ref MeshBuffer, cut_ShoulderR_Vectors[i].ToArray(),ref tTris, ref tNormals));
						cut_normals_ShoulderR[i] = tNormals;
						tMesh_SRSkip = true;
					}
					if(rCount <= 0){
						tMesh_SRSkip = false;	
					}
					int lCount = cut_ShoulderL_Vectors.Count;
					for(int i=0;i<lCount;i++){
						tTris = cut_tris_ShoulderL[i];
						tNormals = cut_normals_ShoulderL[i];
						MeshBuffer = new Mesh();
						tMesh_SLCuts.Add(MeshSetup1_Helper(ref MeshBuffer, cut_ShoulderL_Vectors[i].ToArray(),ref tTris, ref tNormals));
						MeshBuffer = new Mesh();
						tMesh_SLCuts_world.Add(MeshSetup1_Helper(ref MeshBuffer, cut_ShoulderL_Vectors[i].ToArray(),ref tTris, ref tNormals));
						cut_normals_ShoulderL[i] = tNormals;
						tMesh_SLSkip = true;
					}
					if(lCount <= 0){
						tMesh_SLSkip = false;	
					}
				}
			}

			if(bInterseOn){
				MeshSetup1_IntersectionParts();
			}
			
			MeshBuffer = null;
		}
		
		#region "Intersection for MeshSetup1"
		private void MeshSetup1_IntersectionObjectsSetup(){
			int mCount = tRoad.GSDSpline.GetNodeCount();
			List<GSDRoadIntersection> tGSDRIs = new List<GSDRoadIntersection>();
			for(int i=0;i<mCount;i++){
				if(tRoad.GSDSpline.mNodes[i].bIsIntersection){
					if(!tGSDRIs.Contains(tRoad.GSDSpline.mNodes[i].GSDRI)){
						tGSDRIs.Add(tRoad.GSDSpline.mNodes[i].GSDRI);	
					}
				}
			}
			
			//Cleanups:
			foreach(GSDRoadIntersection GSDRI in tGSDRIs){
				GSDIntersectionObjects.CleanupIntersectionObjects(GSDRI.transform.gameObject);
				if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.StopSign_AllWay){
					GSDIntersectionObjects.CreateStopSignsAllWay(GSDRI.transform.gameObject,true);
				}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1){
					GSDIntersectionObjects.CreateTrafficLightBases(GSDRI.transform.gameObject,true);
				}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
					
				}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.None){
					//Do nothing.
				}
			}
		}
		
		private void MeshSetup1_IntersectionParts(){
			int mCount = tRoad.GSDSpline.GetNodeCount();
			bool bHasInter = false;
			for(int i=0;i<mCount;i++){
				if(tRoad.GSDSpline.mNodes[i].bIsIntersection){
					bHasInter = true;
					break;
				}
			}
			if(!bHasInter){
				return;	
			}
			
			int vCount = -1;
			Mesh MeshBuffer = null;
			Vector3[] tNormals = null;
			int[] tTris = null;
			//Back lanes:
			vCount = iBLane0s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iBLane0s_normals[i];
				tTris = iBLane0s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iBLane0s[i],ref tTris, ref tNormals);
				tMesh_iBLanes0.Add(MeshBuffer);
			}
			vCount = iBLane1s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iBLane1s_normals[i];
				tTris = iBLane1s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iBLane1s[i],ref tTris, ref tNormals);
				tMesh_iBLanes1.Add(MeshBuffer);
			}
			vCount = iBLane2s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iBLane2s_normals[i];
				tTris = iBLane2s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iBLane2s[i],ref tTris, ref tNormals);
				tMesh_iBLanes2.Add(MeshBuffer);
			}
			vCount = iBLane3s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iBLane3s_normals[i];
				tTris = iBLane3s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iBLane3s[i],ref tTris, ref tNormals);
				tMesh_iBLanes3.Add(MeshBuffer);
			}
			//Front lanes:
			vCount = iFLane0s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iFLane0s_normals[i];
				tTris = iFLane0s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iFLane0s[i],ref tTris, ref tNormals);
				tMesh_iFLanes0.Add(MeshBuffer);
			}
			vCount = iFLane1s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iFLane1s_normals[i];
				tTris = iFLane1s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iFLane1s[i],ref tTris, ref tNormals);
				tMesh_iFLanes1.Add(MeshBuffer);
			}
			vCount = iFLane2s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iFLane2s_normals[i];
				tTris = iFLane2s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iFLane2s[i],ref tTris, ref tNormals);
				tMesh_iFLanes2.Add(MeshBuffer);
			}
			vCount = iFLane3s.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iFLane3s_normals[i];
				tTris = iFLane3s_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iFLane3s[i],ref tTris, ref tNormals);
				tMesh_iFLanes3.Add(MeshBuffer);
			}
			//Main plates:
			vCount = iBMainPlates.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iBMainPlates_normals[i];
				tTris = iBMainPlates_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iBMainPlates[i],ref tTris, ref tNormals);
				tMesh_iBMainPlates.Add(MeshBuffer);
			}
			vCount = iFMainPlates.Count;
			for(int i=0;i<vCount;i++){
				tNormals = iFMainPlates_normals[i];
				tTris = iFMainPlates_tris[i];
				MeshBuffer = new Mesh();
				MeshBuffer = MeshSetup1_Helper(ref MeshBuffer, iFMainPlates[i],ref tTris, ref tNormals);
				tMesh_iFMainPlates.Add(MeshBuffer);
			}
			
			MeshBuffer = null;
		}
		#endregion
		
		private Mesh MeshSetup1_Helper(ref Mesh xMesh, Vector3[] verts, ref int[] tris, ref Vector3[] normals){
			xMesh.vertices = verts;
			xMesh.triangles = tris;
			xMesh.normals = normals;
			xMesh.RecalculateNormals();
			normals = xMesh.normals;
//			xMesh.hideFlags = HideFlags.DontSave;
			return xMesh;
		}
		#endregion
		
		#region "Mesh Setup2"
		public void MeshSetup2(){
			MeshSetup2_Do();
		}
		
		/// <summary>
		/// Assigns UV and tangents to meshes. If multithreading enabled, this occurs after the last threaded job since unity library can't be used in threads.
		/// </summary>
		private void MeshSetup2_Do(){
			Mesh MeshMainBuffer = null;
			Mesh MeshMarkerBuffer = null;
			
			if(bRoadOn){
				//If road cuts is off, full size UVs:
				if((!tRoad.opt_bRoadCuts && !tRoad.opt_bDynamicCuts) || (RoadCuts == null || RoadCuts.Count <= 0)){
					if(tMesh != null){
						tMesh = MeshSetup2_Helper(ref tMesh, uv, tangents, ref tRoad.MeshRoad,true);
						SaveMesh(SaveMeshTypeEnum.Road,tMesh,tRoad,tRoad.MeshRoad.transform.name);
						
						Vector3[] ooVerts = new Vector3[tMesh.vertexCount];
						int[] ooTris = new int[tMesh.triangles.Length];
						Vector3[] ooNormals = new Vector3[tMesh.normals.Length];
						Vector2[] ooUV = new Vector2[uv2.Length];
						Vector4[] ooTangents = new Vector4[tangents2.Length];

						System.Array.Copy(tMesh.vertices,ooVerts,ooVerts.Length);
						System.Array.Copy(tMesh.triangles,ooTris,ooTris.Length);
						System.Array.Copy(tMesh.normals,ooNormals,ooNormals.Length);
						System.Array.Copy(uv2,ooUV,ooUV.Length);
						System.Array.Copy(tangents2,ooTangents,ooTangents.Length);
						
						Mesh pMesh = new Mesh();
						pMesh.vertices = ooVerts;
						pMesh.triangles = ooTris;
						pMesh.normals = ooNormals;
						pMesh.uv = ooUV;
						pMesh.tangents = ooTangents;
		
						GameObject gObj = new GameObject("Pavement");
						pMesh = MeshSetup2_Helper(ref pMesh, uv2, tangents2, ref gObj,false);
						gObj.transform.parent = tRoad.MeshRoad.transform;	//Road markers stored on parent "MeshRoad" game object, with a "Pavement" child game object storing the asphalt.
						SaveMesh(SaveMeshTypeEnum.Road,pMesh,tRoad,gObj.transform.name);
					}
				}else{
					//If road cuts, change it to one material (pavement) with world mapping
					int cCount = cut_RoadVectors.Count;
//					Vector2[] tUV;
					GameObject CreatedMainObj;
					GameObject CreatedMarkerObj;
					for(int i=0;i<cCount;i++){
						CreatedMainObj = null;
						MeshMainBuffer = tMesh_RoadCuts_world[i];
						if(MeshMainBuffer != null){
							MeshSetup2_Helper_RoadCuts(i,ref MeshMainBuffer, cut_uv_world[i], cut_tangents_world[i], ref tRoad.MeshRoad,false, out CreatedMainObj);
							SaveMesh(SaveMeshTypeEnum.RoadCut,MeshMainBuffer,tRoad,"RoadCut" + i.ToString());
						}
						
						CreatedMarkerObj = null;
						MeshMarkerBuffer = tMesh_RoadCuts[i];
						bool bHasMats = false;
						if(MeshMarkerBuffer != null){
					 		bHasMats = MeshSetup2_Helper_RoadCuts(i,ref MeshMarkerBuffer, cut_uv[i], cut_tangents[i], ref CreatedMainObj,true, out CreatedMarkerObj);
							if(bHasMats){
								SaveMesh(SaveMeshTypeEnum.RoadCut,MeshMarkerBuffer,tRoad,"RoadCutMarker" + i.ToString());
							}else{
								//Destroy if no marker materials:
								Object.DestroyImmediate(CreatedMarkerObj);
								Object.DestroyImmediate(MeshMarkerBuffer);
							}
						}
					}
					
					//Remove main mesh stuff if necessary:
					if(tRoad.MeshRoad != null){
						MeshCollider tMC = tRoad.MeshRoad.GetComponent<MeshCollider>();
						MeshRenderer tMR = tRoad.MeshRoad.GetComponent<MeshRenderer>();
						if(tMC != null){ 
							Object.DestroyImmediate(tMC); 
						}
						if(tMR != null){ 
							Object.DestroyImmediate(tMR); 
						}
					}
					if(tMesh != null){
						Object.DestroyImmediate(tMesh); 
					}
				}
				
				//Shoulders:
				if(tRoad.opt_bShouldersEnabled){
					if((!tRoad.opt_bShoulderCuts && !tRoad.opt_bDynamicCuts) || (ShoulderCutsL == null || cut_ShoulderL_Vectors.Count <= 0)){
						//Right road shoulder:
						if(tMesh_SR != null){
							tMesh_SR = MeshSetup2_Helper(ref tMesh_SR, uv_SR, tangents_SR, ref tRoad.MeshShoR,false,true);
							SaveMesh(SaveMeshTypeEnum.Shoulder,tMesh_SR,tRoad,tRoad.MeshShoR.transform.name); 
						}
						
						//Left road shoulder:
						if(tMesh_SL != null){
							tMesh_SL = MeshSetup2_Helper(ref tMesh_SL, uv_SL, tangents_SL, ref tRoad.MeshShoL,false,true);
							SaveMesh(SaveMeshTypeEnum.Shoulder,tMesh_SL,tRoad,tRoad.MeshShoL.transform.name); 
						}
					}else{
						GameObject CreatedMainObj;
						GameObject CreatedMarkerObj;
						int rCount = cut_ShoulderR_Vectors.Count;
						for(int i=0;i<rCount;i++){
							bool bHasMats = false;
							CreatedMainObj = null;
							MeshMainBuffer = tMesh_SRCuts_world[i];
							if(MeshMainBuffer != null){
								MeshSetup2_Helper_CutsShoulder(i,ref MeshMainBuffer, cut_uv_SR_world[i], cut_tangents_SR_world[i], ref tRoad.MeshShoR,false,false,out CreatedMainObj);
								SaveMesh(SaveMeshTypeEnum.SCut,MeshMainBuffer,tRoad,"SCutR" + i.ToString());
							}
	
							CreatedMarkerObj = null;
							MeshMarkerBuffer = tMesh_SRCuts[i]; 
							if(MeshMarkerBuffer != null){
								bHasMats = MeshSetup2_Helper_CutsShoulder(i,ref MeshMarkerBuffer, cut_uv_SR[i], cut_tangents_SR[i], ref CreatedMainObj,false,true,out CreatedMarkerObj);
								if(bHasMats){
									SaveMesh(SaveMeshTypeEnum.SCut,MeshMarkerBuffer,tRoad,"SCutRMarker" + i.ToString());
								}else{
									//Destroy if no marker materials:
									Object.DestroyImmediate(CreatedMarkerObj);
									Object.DestroyImmediate(MeshMarkerBuffer);
								}
							}
						}
						
						int lCount = cut_ShoulderL_Vectors.Count;
						for(int i=0;i<lCount;i++){
							bool bHasMats = false;
							CreatedMainObj = null;
							MeshMainBuffer = tMesh_SLCuts_world[i];
							if(MeshMainBuffer != null){
								MeshSetup2_Helper_CutsShoulder(i,ref MeshMainBuffer, cut_uv_SL_world[i], cut_tangents_SL_world[i], ref tRoad.MeshShoL,true,false,out CreatedMainObj);
								SaveMesh(SaveMeshTypeEnum.SCut,MeshMainBuffer,tRoad,"SCutL" + i.ToString());
							}
							
							
							CreatedMarkerObj = null;
							MeshMarkerBuffer = tMesh_SLCuts[i];
							if(MeshMarkerBuffer != null){
								bHasMats = MeshSetup2_Helper_CutsShoulder(i,ref MeshMarkerBuffer, cut_uv_SL[i], cut_tangents_SL[i], ref CreatedMainObj,true,true,out CreatedMarkerObj);
								if(bHasMats){
									SaveMesh(SaveMeshTypeEnum.SCut,MeshMarkerBuffer,tRoad,"SCutLMarker" + i.ToString());
								}else{
									//Destroy if no marker materials:
									Object.DestroyImmediate(CreatedMarkerObj);
									Object.DestroyImmediate(MeshMarkerBuffer);
								}
							}
						}

                        if (tRoad.opt_bUseMeshColliders) {
	//						MeshSetup2_Intersections_FixNormals();	
						}
						
						//Remove main mesh stuff if necessary:
						if(tMesh_SR != null){
							Object.DestroyImmediate(tMesh_SR); 
						}
						if(tMesh_SL != null){
							Object.DestroyImmediate(tMesh_SL); 
						}
						
						if(tRoad.MeshShoR != null){
							MeshCollider tMC = tRoad.MeshShoR.GetComponent<MeshCollider>();
							MeshRenderer tMR = tRoad.MeshShoR.GetComponent<MeshRenderer>();
							if(tMC != null){ Object.DestroyImmediate(tMC); }
							if(tMR != null){ Object.DestroyImmediate(tMR); }
						}
						if(tRoad.MeshShoL != null){
							MeshCollider tMC = tRoad.MeshShoL.GetComponent<MeshCollider>();
							MeshRenderer tMR = tRoad.MeshShoL.GetComponent<MeshRenderer>();
							if(tMC != null){ Object.DestroyImmediate(tMC); }
							if(tMR != null){ Object.DestroyImmediate(tMR); }
						}
					}
				}else{
					//Remove main mesh stuff if necessary:
					if(tMesh_SR != null){
						Object.DestroyImmediate(tMesh_SR); 
					}
					if(tMesh_SL != null){
						Object.DestroyImmediate(tMesh_SL); 
					}
					
					if(tRoad.MeshShoR != null){
						MeshCollider tMC = tRoad.MeshShoR.GetComponent<MeshCollider>();
						MeshRenderer tMR = tRoad.MeshShoR.GetComponent<MeshRenderer>();
						if(tMC != null){ Object.DestroyImmediate(tMC); }
						if(tMR != null){ Object.DestroyImmediate(tMR); }
					}
					if(tRoad.MeshShoL != null){
						MeshCollider tMC = tRoad.MeshShoL.GetComponent<MeshCollider>();
						MeshRenderer tMR = tRoad.MeshShoL.GetComponent<MeshRenderer>();
						if(tMC != null){ Object.DestroyImmediate(tMC); }
						if(tMR != null){ Object.DestroyImmediate(tMR); }
					}
					
					Mesh tBuffer = null;
					int xCount = tMesh_SRCuts_world.Count;
					for(int i=0;i<xCount;i++){
						tBuffer = tMesh_SRCuts_world[i];
						Object.DestroyImmediate(tBuffer);
						tMesh_SRCuts_world[i] = null;
					}
					xCount = tMesh_SRCuts.Count;
					for(int i=0;i<xCount;i++){
						tBuffer = tMesh_SRCuts[i];
						Object.DestroyImmediate(tBuffer);
						tMesh_SRCuts[i] = null;
					}
					xCount = tMesh_SLCuts_world.Count;
					for(int i=0;i<xCount;i++){
						tBuffer = tMesh_SLCuts_world[i];
						Object.DestroyImmediate(tBuffer);
						tMesh_SLCuts_world[i] = null;
					}
					xCount = tMesh_SLCuts.Count;
					for(int i=0;i<xCount;i++){
						tBuffer = tMesh_SLCuts[i];
						Object.DestroyImmediate(tBuffer);
						tMesh_SLCuts[i] = null;
					}
					
					if(tRoad.MeshShoR != null){
						Object.DestroyImmediate(tRoad.MeshShoR);
					}
					
					if(tRoad.MeshShoL != null){
						Object.DestroyImmediate(tRoad.MeshShoL);
					}
				}

				
				for(int i=0;i<RoadConnections_tangents.Count;i++){
					tMesh_RoadConnections[i].tangents = RoadConnections_tangents[i];
					GameObject tObj = new GameObject("RoadConnectionMarker");
					MeshFilter MF = tObj.AddComponent<MeshFilter>();
					MeshRenderer MR = tObj.AddComponent<MeshRenderer>();
					float fDist = Vector3.Distance(RoadConnections_verts[i][2],RoadConnections_verts[i][3]);
					fDist = Mathf.Round(fDist);
					if(tRoad.opt_Lanes == 2){
						if(fDist == Mathf.Round(tRoad.RoadWidth()*2f)){
							GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadConn-4L.mat",MR);
						}else if(fDist == Mathf.Round(tRoad.RoadWidth()*3f)){
							GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadConn-6L-2L.mat",MR);
						}
					}else if(tRoad.opt_Lanes == 4){
						if(fDist == Mathf.Round(tRoad.RoadWidth()*1.5f)){
							GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadConn-6L-4L.mat",MR);
						}
					}
					MF.sharedMesh = tMesh_RoadConnections[i];
					tObj.transform.parent = tRoad.MeshRoad.transform;
					
					Mesh vMesh = new Mesh();
					vMesh.vertices = RoadConnections_verts[i];
					vMesh.triangles = RoadConnections_tris[i];
					vMesh.normals = RoadConnections_normals[i];
					Vector2[] vUV = new Vector2[4];
					vUV[0] = new Vector2(RoadConnections_verts[i][0].x/5f,RoadConnections_verts[i][0].z/5f);
					vUV[1] = new Vector2(RoadConnections_verts[i][1].x/5f,RoadConnections_verts[i][1].z/5f);
					vUV[2] = new Vector2(RoadConnections_verts[i][2].x/5f,RoadConnections_verts[i][2].z/5f);
					vUV[3] = new Vector2(RoadConnections_verts[i][3].x/5f,RoadConnections_verts[i][3].z/5f);
					vMesh.uv = vUV;
					vMesh.RecalculateNormals();
					RoadConnections_normals[i] = vMesh.normals;
					vMesh.tangents = GSDRootUtil.ProcessTangents(vMesh.triangles,vMesh.normals,vMesh.uv,vMesh.vertices);
					tObj = new GameObject("RoadConnectionBase");
					MF = tObj.AddComponent<MeshFilter>();
					MR = tObj.AddComponent<MeshRenderer>();
					MeshCollider MC = tObj.AddComponent<MeshCollider>();
					MF.sharedMesh = vMesh;
					MC.sharedMesh = MF.sharedMesh;
					GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/GSDRoad6.mat",MR);
					tObj.transform.parent = tRoad.MeshRoad.transform;
					
					SaveMesh(SaveMeshTypeEnum.RoadConn,vMesh,tRoad,"RoadConn" + i.ToString());
				}
			}

			if(bInterseOn){
				MeshSetup2_Intersections();
			}
			if(tRoad.MeshiLanes != null){ Object.DestroyImmediate(tRoad.MeshiLanes); }
			if(tRoad.MeshiLanes0 != null){ Object.DestroyImmediate(tRoad.MeshiLanes0); }
			if(tRoad.MeshiLanes1 != null){ Object.DestroyImmediate(tRoad.MeshiLanes1); }
			if(tRoad.MeshiLanes2 != null){ Object.DestroyImmediate(tRoad.MeshiLanes2); }
			if(tRoad.MeshiLanes3 != null){ Object.DestroyImmediate(tRoad.MeshiLanes3); }
			if(tRoad.MeshiMainPlates != null){ Object.DestroyImmediate(tRoad.MeshiMainPlates); }
			if(tRoad.MeshiMarkerPlates != null){ Object.DestroyImmediate(tRoad.MeshiMarkerPlates); }
			
			//Updates the road and shoulder cut materials if necessary. Note: Cycling through all nodes in case the road cuts and shoulder cut numbers don't match.
			if(tRoad.opt_bRoadCuts || tRoad.opt_bShoulderCuts || tRoad.opt_bDynamicCuts){
				int mCount = tRoad.GSDSpline.GetNodeCount();
				for(int i=0;i<mCount;i++){
					tRoad.GSDSpline.mNodes[i].UpdateCuts();	
				}
			}
		}
		
		#region "MeshSetup2 - Intersections"
//		private void MeshSetup2_Intersections_FixNormals(){
//			int mCount = tRoad.GSDSpline.GetNodeCount();
//			GSDSplineN tNode = null;
//			GSDRoadIntersection GSDRI = null;
//			float MaxDist = 0f;
//			float[] tDists = new float[2];
//			Collider[] tColliders = null;
//			List<GameObject> tCuts = null;
//
//			for(int h=0;h<mCount;h++){
//				tNode=tRoad.GSDSpline.mNodes[h];
//				if(tNode.bIsIntersection){
//					GSDRI = tNode.GSDRI;
//					
//					
//					
//
//					tColliders = Physics.OverlapSphere(GSDRI.CornerRR_Outer,tRoad.opt_ShoulderWidth*1.25f);
//					tCuts = new List<GameObject>();
//					foreach(Collider tCollider in tColliders){
//						if(tCollider.transform.name.Contains("cut")){
//							tCuts.Add(tCollider.transform.gameObject);
//						}
//					}
//					
//					
//					
//					foreach(GameObject tObj in tCuts){
//						MeshFilter MF1 = tCuts[0].GetComponent<MeshFilter>();
//						if(MF1 == null){ continue; }
//						Mesh zMesh1 = MF1.sharedMesh;
//						Vector3[] tVerts1 = zMesh1.vertices;
//						Vector3[] tNormals1 = zMesh1.normals;
//						int MVL1 = tVerts1.Length;
//						for(int i=0;i<MVL1;i++){
//							if(tVerts1[i] == GSDRI.CornerRR){
//								tNormals1[i] = Vector3.up;
//							}else if(tVerts1[i] == GSDRI.CornerRR_Outer){
//								tNormals1[i] = Vector3.up;
//							}
//						}
//					}
//
//					
//				}
//			}
//		}
		
		private void MeshSetup2_Intersections(){
			int mCount = tRoad.GSDSpline.GetNodeCount();
			bool bHasInter = false;
			for(int i=0;i<mCount;i++){
				if(tRoad.GSDSpline.mNodes[i].bIsIntersection){
					bHasInter = true;
					break;
				}
			}
			if(!bHasInter){
				return;	
			}
			
			
			int vCount = -1;
			Mesh xMesh = null;
			Vector2[] tUV = null;
			Vector4[] tTangents = null;
			MeshFilter MF = null;
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane0 = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane1 = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane2 = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane3 = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_MainPlate = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_MainPlateM = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			HashSet<GSDRoadIntersection> UniqueGSDRI = new HashSet<GSDRoadIntersection>();
			
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane1_Disabled = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane2_Disabled = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane2_DisabledActive = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane2_DisabledActiveR = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane3_Disabled = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();
			Dictionary<GSDRoadIntersection, List<MeshFilter>> tCombineDict_Lane1_DisabledActive = new Dictionary<GSDRoadIntersection, List<MeshFilter>>();

			vCount = iBLane0s.Count;
			for(int i=0;i<vCount;i++){
				tUV = iBLane0s_uv[i];
				tTangents = iBLane0s_tangents[i];
				xMesh = tMesh_iBLanes0[i];
				MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes0, "Lane0B","Assets/RoadArchitect/Materials/Markers/GSDInterWhiteLYellowR.mat");
				if(!tCombineDict_Lane0.ContainsKey(iBLane0s_tID[i])){
					tCombineDict_Lane0.Add(iBLane0s_tID[i], new List<MeshFilter>());
				}
				tCombineDict_Lane0[iBLane0s_tID[i]].Add(MF);
			}
			vCount = iBLane1s.Count;
			for(int i=0;i<vCount;i++){
				bool bPrimaryNode = (iBLane1s_tID[i].Node1 == iBLane1s_nID[i]);
				if(!bPrimaryNode && iBLane1s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && (iBLane1s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.TurnLane || iBLane1s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes) && !iBLane1s_tID[i].bNode2B_LeftTurnLane){
					tUV = iBLane1s_uv[i];
					tTangents = iBLane1s_tangents[i];
					xMesh = tMesh_iBLanes1[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes1, "LaneD1B","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabled.mat");
					if(!tCombineDict_Lane1_Disabled.ContainsKey(iBLane1s_tID[i])){
						tCombineDict_Lane1_Disabled.Add(iBLane1s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane1_Disabled[iBLane1s_tID[i]].Add(MF);
				}else if(bPrimaryNode && iBLane1s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iBLane1s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					tUV = iBLane1s_uv[i];
					tTangents = iBLane1s_tangents[i];
					xMesh = tMesh_iBLanes1[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes1, "LaneDA1B","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuter.mat");
					if(!tCombineDict_Lane1_DisabledActive.ContainsKey(iBLane1s_tID[i])){
						tCombineDict_Lane1_DisabledActive.Add(iBLane1s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane1_DisabledActive[iBLane1s_tID[i]].Add(MF);
				}else{
					tUV = iBLane1s_uv[i];
					tTangents = iBLane1s_tangents[i];
					xMesh = tMesh_iBLanes1[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes1, "Lane1B","Assets/RoadArchitect/Materials/Markers/GSDInterYellowLWhiteR.mat");
					if(!tCombineDict_Lane1.ContainsKey(iBLane1s_tID[i])){
						tCombineDict_Lane1.Add(iBLane1s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane1[iBLane1s_tID[i]].Add(MF);
				}
			}
			vCount = iBLane2s.Count;
			for(int i=0;i<vCount;i++){
				bool bPrimaryNode = (iBLane2s_tID[i].Node1 == iBLane2s_nID[i]);
				if(!bPrimaryNode && iBLane2s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && (iBLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.TurnLane || iBLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes) && !iBLane2s_tID[i].bNode2B_LeftTurnLane){
					tUV = iBLane2s_uv[i];
					tTangents = iBLane2s_tangents[i];
					xMesh = tMesh_iBLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "LaneDA2B","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuter.mat");
					if(!tCombineDict_Lane2_DisabledActive.ContainsKey(iBLane2s_tID[i])){
						tCombineDict_Lane2_DisabledActive.Add(iBLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2_DisabledActive[iBLane2s_tID[i]].Add(MF);
				}else if(!bPrimaryNode && iBLane2s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iBLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes && !iBLane2s_tID[i].bNode2B_RightTurnLane){
					tUV = iBLane2s_uv[i];
					tTangents = iBLane2s_tangents[i];
					xMesh = tMesh_iBLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "LaneDA2B","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuterR.mat");
					if(!tCombineDict_Lane2_DisabledActiveR.ContainsKey(iBLane2s_tID[i])){
						tCombineDict_Lane2_DisabledActiveR.Add(iBLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2_DisabledActiveR[iBLane2s_tID[i]].Add(MF);
				}else if(bPrimaryNode && iBLane2s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iBLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					tUV = iBLane2s_uv[i];
					tTangents = iBLane2s_tangents[i];
					xMesh = tMesh_iBLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "LaneD2B","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabled.mat");
					if(!tCombineDict_Lane2_Disabled.ContainsKey(iBLane2s_tID[i])){
						tCombineDict_Lane2_Disabled.Add(iBLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2_Disabled[iBLane2s_tID[i]].Add(MF);
				}else{
					tUV = iBLane2s_uv[i];
					tTangents = iBLane2s_tangents[i];
					xMesh = tMesh_iBLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "Lane2B","Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR.mat");
					if(!tCombineDict_Lane2.ContainsKey(iBLane2s_tID[i])){
						tCombineDict_Lane2.Add(iBLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2[iBLane2s_tID[i]].Add(MF);
				}
			}
			vCount = iBLane3s.Count;
			for(int i=0;i<vCount;i++){
				bool bPrimaryNode = (iBLane3s_tID[i].Node1 == iBLane3s_nID[i]);
				if(!bPrimaryNode && iBLane3s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iBLane3s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes && !iBLane3s_tID[i].bNode2B_RightTurnLane){
					tUV = iBLane3s_uv[i];
					tTangents = iBLane3s_tangents[i];
					xMesh = tMesh_iBLanes3[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes3, "LaneD3B","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabled.mat");
					if(!tCombineDict_Lane3_Disabled.ContainsKey(iBLane3s_tID[i])){
						tCombineDict_Lane3_Disabled.Add(iBLane3s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane3_Disabled[iBLane3s_tID[i]].Add(MF);
				}else{
					tUV = iBLane3s_uv[i];
					tTangents = iBLane3s_tangents[i];
					xMesh = tMesh_iBLanes3[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes3, "Lane3B","Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR.mat");
					if(!tCombineDict_Lane3.ContainsKey(iBLane3s_tID[i])){
						tCombineDict_Lane3.Add(iBLane3s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane3[iBLane3s_tID[i]].Add(MF);
				}
			}
	
			//Front lanes:
			vCount = iFLane0s.Count;
			for(int i=0;i<vCount;i++){
				tUV = iFLane0s_uv[i];
				tTangents = iFLane0s_tangents[i];
				xMesh = tMesh_iFLanes0[i];
				MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes0, "Lane0F","Assets/RoadArchitect/Materials/Markers/GSDInterWhiteLYellowR.mat");
				if(!tCombineDict_Lane0.ContainsKey(iFLane0s_tID[i])){
					tCombineDict_Lane0.Add(iFLane0s_tID[i], new List<MeshFilter>());
				}
				tCombineDict_Lane0[iFLane0s_tID[i]].Add(MF);
			}
			vCount = iFLane1s.Count;
			for(int i=0;i<vCount;i++){
				bool bPrimaryNode = (iFLane1s_tID[i].Node1 == iFLane1s_nID[i]);
				if(!bPrimaryNode && iFLane1s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && (iFLane1s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes || iFLane1s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.TurnLane) && !iFLane1s_tID[i].bNode2F_LeftTurnLane){
					tUV = iFLane1s_uv[i];
					tTangents = iFLane1s_tangents[i];
					xMesh = tMesh_iFLanes1[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes1, "LaneD1F","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabled.mat");
					if(!tCombineDict_Lane1_Disabled.ContainsKey(iFLane1s_tID[i])){
						tCombineDict_Lane1_Disabled.Add(iFLane1s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane1_Disabled[iFLane1s_tID[i]].Add(MF);
				}else if(bPrimaryNode && iFLane1s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iFLane1s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					tUV = iFLane1s_uv[i];
					tTangents = iFLane1s_tangents[i];
					xMesh = tMesh_iFLanes1[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes1, "LaneDAR1F","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuterR.mat");
					if(!tCombineDict_Lane1_DisabledActive.ContainsKey(iFLane1s_tID[i])){
						tCombineDict_Lane1_DisabledActive.Add(iFLane1s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane1_DisabledActive[iFLane1s_tID[i]].Add(MF);
				}else{
					tUV = iFLane1s_uv[i];
					tTangents = iFLane1s_tangents[i];
					xMesh = tMesh_iFLanes1[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes1, "Lane1F","Assets/RoadArchitect/Materials/Markers/GSDInterYellowLWhiteR.mat");
					if(!tCombineDict_Lane1.ContainsKey(iFLane1s_tID[i])){
						tCombineDict_Lane1.Add(iFLane1s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane1[iFLane1s_tID[i]].Add(MF);
				}
			}
			vCount = iFLane2s.Count;
			for(int i=0;i<vCount;i++){
				bool bPrimaryNode = (iFLane2s_tID[i].Node1 == iFLane2s_nID[i]);
				if(!bPrimaryNode && iFLane2s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && (iFLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes || iFLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.TurnLane) && !iFLane2s_tID[i].bNode2F_LeftTurnLane){
					tUV = iFLane2s_uv[i];
					tTangents = iFLane2s_tangents[i];
					xMesh = tMesh_iFLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "LaneDA2F","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuter.mat");
					if(!tCombineDict_Lane2_DisabledActive.ContainsKey(iFLane2s_tID[i])){
						tCombineDict_Lane2_DisabledActive.Add(iFLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2_DisabledActive[iFLane2s_tID[i]].Add(MF);
				}else if(!bPrimaryNode && iFLane2s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iFLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes && !iFLane2s_tID[i].bNode2F_RightTurnLane){
					tUV = iFLane2s_uv[i];
					tTangents = iFLane2s_tangents[i];
					xMesh = tMesh_iFLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "LaneDAR2F","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuterR.mat");
					if(!tCombineDict_Lane2_DisabledActiveR.ContainsKey(iFLane2s_tID[i])){
						tCombineDict_Lane2_DisabledActiveR.Add(iFLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2_DisabledActiveR[iFLane2s_tID[i]].Add(MF);
				}else if(bPrimaryNode && iFLane2s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iFLane2s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					tUV = iFLane2s_uv[i];
					tTangents = iFLane2s_tangents[i];
					xMesh = tMesh_iFLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "LaneD2F","Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabled.mat");
					if(!tCombineDict_Lane2_Disabled.ContainsKey(iFLane2s_tID[i])){
						tCombineDict_Lane2_Disabled.Add(iFLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2_Disabled[iFLane2s_tID[i]].Add(MF);
				}else{
					tUV = iFLane2s_uv[i];
					tTangents = iFLane2s_tangents[i];
					xMesh = tMesh_iFLanes2[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes2, "Lane2F","Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR.mat");
					if(!tCombineDict_Lane2.ContainsKey(iFLane2s_tID[i])){
						tCombineDict_Lane2.Add(iFLane2s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane2[iFLane2s_tID[i]].Add(MF);
				}
			}
			vCount = iFLane3s.Count;
			for(int i=0;i<vCount;i++){
				bool bPrimaryNode = (iFLane3s_tID[i].Node1 == iFLane3s_nID[i]);
				if(!bPrimaryNode && iFLane3s_tID[i].iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay && iFLane3s_tID[i].rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes && !iFLane3s_tID[i].bNode2F_RightTurnLane){
					tUV = iFLane3s_uv[i];
					tTangents = iFLane3s_tangents[i];
					xMesh = tMesh_iFLanes3[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes3, "LaneD3F","Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR.mat");
					if(!tCombineDict_Lane3_Disabled.ContainsKey(iFLane3s_tID[i])){
						tCombineDict_Lane3_Disabled.Add(iFLane3s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane3_Disabled[iFLane3s_tID[i]].Add(MF);
				}else{
					tUV = iFLane3s_uv[i];
					tTangents = iFLane3s_tangents[i];
					xMesh = tMesh_iFLanes3[i];
					MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiLanes3, "Lane3F","Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR.mat");
					if(!tCombineDict_Lane3.ContainsKey(iFLane3s_tID[i])){
						tCombineDict_Lane3.Add(iFLane3s_tID[i], new List<MeshFilter>());
					}
					tCombineDict_Lane3[iFLane3s_tID[i]].Add(MF);
				}
			}
			
			//Main plates:
			vCount = iBMainPlates.Count;
			for(int i=0;i<vCount;i++){
				tUV = iBMainPlates_uv[i];
				tTangents = iBMainPlates_tangents[i];
				xMesh = tMesh_iBMainPlates[i];
				MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiMainPlates, "MainPlateB","Assets/RoadArchitect/Materials/GSDRoad6.mat", false);
				if(!tCombineDict_MainPlate.ContainsKey(iBMainPlates_tID[i])){
					tCombineDict_MainPlate.Add(iBMainPlates_tID[i], new List<MeshFilter>());
				}
				tCombineDict_MainPlate[iBMainPlates_tID[i]].Add(MF);
				
				Mesh fMesh = new Mesh();
				fMesh.vertices = iBMainPlates[i];
				fMesh.triangles = iBMainPlates_tris[i];
				fMesh.normals = iBMainPlates_normals[i];
				tUV = iBMainPlates_uv2[i];
				tTangents = iBMainPlates_tangents2[i];
				MF = MeshSetup2_Intersection_Helper(ref fMesh, ref tUV, ref tTangents, ref tRoad.MeshiMainPlates, "MainPlateBM","Assets/RoadArchitect/Materials/GSDInterMainPlate1.mat");
				if(!tCombineDict_MainPlateM.ContainsKey(iBMainPlates_tID[i])){
					tCombineDict_MainPlateM.Add(iBMainPlates_tID[i], new List<MeshFilter>());
				}
				tCombineDict_MainPlateM[iBMainPlates_tID[i]].Add(MF);
			}
			vCount = iFMainPlates.Count;
			for(int i=0;i<vCount;i++){
				tUV = iFMainPlates_uv[i];
				tTangents = iFMainPlates_tangents[i];
				xMesh = tMesh_iFMainPlates[i];
				MF = MeshSetup2_Intersection_Helper(ref xMesh, ref tUV, ref tTangents, ref tRoad.MeshiMainPlates, "MainPlateFM","Assets/RoadArchitect/Materials/GSDRoad6.mat", false);
				
				if(!tCombineDict_MainPlate.ContainsKey(iFMainPlates_tID[i])){
					tCombineDict_MainPlate.Add(iFMainPlates_tID[i], new List<MeshFilter>());
				}
				tCombineDict_MainPlate[iFMainPlates_tID[i]].Add(MF);
				
				Mesh tMesh = new Mesh();
				tMesh.vertices = iFMainPlates[i];
				tMesh.triangles = iFMainPlates_tris[i];
				tMesh.normals = iFMainPlates_normals[i];
				tUV = iFMainPlates_uv2[i];
				tTangents = iFMainPlates_tangents2[i];
				MF = MeshSetup2_Intersection_Helper(ref tMesh, ref tUV, ref tTangents, ref tRoad.MeshiMainPlates, "MainPlateFM","Assets/RoadArchitect/Materials/GSDInterMainPlate1.mat");
				if(!tCombineDict_MainPlateM.ContainsKey(iFMainPlates_tID[i])){
					tCombineDict_MainPlateM.Add(iFMainPlates_tID[i], new List<MeshFilter>());
				}
				tCombineDict_MainPlateM[iFMainPlates_tID[i]].Add(MF);
			}

			vCount = tRoad.GSDSpline.GetNodeCount();
			GSDSplineN tNode = null;
			for(int i=0;i<vCount;i++){
				tNode = tRoad.GSDSpline.mNodes[i];
				if(tNode.bIsIntersection && tNode.GSDRI != null && tNode.GSDRI.Node1 == tNode){
					//Create center plate
					Vector3[] xVerts = new Vector3[4];
					xVerts[0] = tNode.GSDRI.CornerLR;
					xVerts[1] = tNode.GSDRI.CornerRR;
					xVerts[2] = tNode.GSDRI.CornerLL;
					xVerts[3] = tNode.GSDRI.CornerRL;
            
					
					int[] xTris = new int[6];
					xTris[0] = 0;
					xTris[1] = 2;
					xTris[2] = 1;
					xTris[3] = 2;
					xTris[4] = 3;
					xTris[5] = 1;
					
					Vector2[] xUV = new Vector2[4];
					xUV[0] = new Vector2(xVerts[0].x/5f,xVerts[0].z/5f);
					xUV[1] = new Vector2(xVerts[1].x/5f,xVerts[1].z/5f);
					xUV[2] = new Vector2(xVerts[2].x/5f,xVerts[2].z/5f);
					xUV[3] = new Vector2(xVerts[3].x/5f,xVerts[3].z/5f);
					
					Vector2[] xUV2 = new Vector2[4];
					xUV2[0] = new Vector2(0f,0f);
					xUV2[1] = new Vector2(1f,0f);
					xUV2[2] = new Vector2(0f,1f);
					xUV2[3] = new Vector2(1f,1f);
					
					Mesh vMesh = new Mesh();
					vMesh.vertices = xVerts;
					vMesh.triangles = xTris;
					vMesh.normals = new Vector3[4];
					vMesh.uv = xUV;
                    vMesh.RecalculateBounds();
					vMesh.RecalculateNormals();
					vMesh.tangents = GSDRootUtil.ProcessTangents(xTris,vMesh.normals,xUV,xVerts);
                    if (tRoad.opt_bIsLightmapped) {
                        //UnityEditor.Unwrapping.GenerateSecondaryUVSet(vMesh);
                    }
					
					int cCount = tNode.GSDRI.transform.childCount;
					List<GameObject> GOToDelete = new List<GameObject>();
					for(int j=0;j<cCount;j++){
						if(tNode.GSDRI.transform.GetChild(j).name.ToLower() == "tcenter"){
							GOToDelete.Add(tNode.GSDRI.transform.GetChild(j).gameObject);
						}
						if(tNode.GSDRI.transform.GetChild(j).name.ToLower() == "markcenter"){
							GOToDelete.Add(tNode.GSDRI.transform.GetChild(j).gameObject);	
						}
					}
					for(int j=GOToDelete.Count-1;j>=0;j--){
						Object.DestroyImmediate(GOToDelete[j]);
					}
					
					GameObject tCenter = new GameObject("tCenter");
					MF = tCenter.AddComponent<MeshFilter>();
					MF.sharedMesh = vMesh;
					tCenter.transform.parent = tNode.GSDRI.transform;
                    if (tRoad.opt_bIsLightmapped) {
                        UnityEditor.GameObjectUtility.SetStaticEditorFlags(tCenter, UnityEditor.StaticEditorFlags.LightmapStatic);
                    }
                    if (tRoad.opt_bIsStatic) {
                        tCenter.isStatic = true;
                    }
                    
                    
					
					Mesh mMesh = new Mesh();
					Vector3[] bVerts = new Vector3[4];
                    mMesh.vertices = xVerts;
					mMesh.triangles = xTris;
					mMesh.normals = new Vector3[4];
					mMesh.uv = xUV2;
					mMesh.RecalculateBounds();
					mMesh.RecalculateNormals();
					mMesh.tangents = GSDRootUtil.ProcessTangents(xTris,vMesh.normals,xUV,xVerts);
                    

					GameObject tMarker = new GameObject("CenterMarkers");
					//tMarker.transform.localPosition = default(Vector3);
					MF = tMarker.AddComponent<MeshFilter>();
					MF.sharedMesh = mMesh;
					MeshRenderer MR = tMarker.AddComponent<MeshRenderer>();
                    MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    //					if(tNode.GSDRI.MarkerCenter != null){
                    ////						MR.material = tNode.GSDRI.MarkerCenter;
                    //					}
                    tMarker.transform.parent = tNode.GSDRI.transform;
                    if (tRoad.opt_bIsLightmapped) {
                        UnityEditor.GameObjectUtility.SetStaticEditorFlags(tMarker, UnityEditor.StaticEditorFlags.LightmapStatic);
                    }
                    if (tRoad.opt_bIsStatic) {
                        tMarker.isStatic = true;
                    }
                    
					bVerts = MF.sharedMesh.vertices;
					for(int j=0;j<4;j++){
						bVerts[j].y = tNode.GSDRI.SignHeight;
					}


                    int zCount = bVerts.Length;
                    for (int z = 0; z < zCount; z++) {
                        bVerts[z] -= tNode.transform.position;
                    }

					MF.sharedMesh.vertices = bVerts;



                    MR.transform.position = tNode.GSDRI.transform.position;

					mMesh.RecalculateBounds();
                    if (tRoad.opt_bIsLightmapped) {
                        //UnityEditor.Unwrapping.GenerateSecondaryUVSet(mMesh);
                    }
					SaveMesh(SaveMeshTypeEnum.Intersection,MF.sharedMesh,tRoad,tNode.GSDRI.transform.name + "-" + "CenterMarkers");
				}
			}
			
//			List<Mesh> MeshToDelete = new List<Mesh>();
			
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane0){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" + "Lane0");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane1){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"Lane1");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane2){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"Lane2");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane3){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"Lane3");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_MainPlate){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"TiledExt", true);
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_MainPlateM){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"StretchExt");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane1_Disabled){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"LaneD1");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane3_Disabled){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"LaneD3");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane2_DisabledActive){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"LaneDA2");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane2_DisabledActiveR){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"LaneDAR2");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane2_Disabled){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"LaneD2");
			}
			foreach (KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP in tCombineDict_Lane1_DisabledActive){
				if(!UniqueGSDRI.Contains(KVP.Key)){ UniqueGSDRI.Add(KVP.Key); }
				MeshSetup2_CombineIntersections(KVP,KVP.Key.transform.name + "-" +"LaneDA1");
			}
			
			foreach (GSDRoadIntersection GSDRI in UniqueGSDRI){
				GSDRI.UpdateMaterials();	
			}
		}
		
		private void MeshSetup2_CombineIntersections(KeyValuePair<GSDRoadIntersection, List<MeshFilter>> KVP, string tName, bool bMainPlates = false){
			int vCount = KVP.Value.Count;
			if(vCount < 1){ return; }
			tName = tRoad.name+"-"+tName;
			GameObject tCenter = null;
			int cCount = KVP.Key.transform.childCount;
			List<GameObject> GOToDelete = new List<GameObject>();
			for(int i=0;i<cCount;i++){
				if(KVP.Key.transform.GetChild(i).name.ToLower() == tName.ToLower()){
					GOToDelete.Add(KVP.Key.transform.GetChild(i).gameObject);
				}
				if(bMainPlates && KVP.Key.transform.GetChild(i).name.ToLower() == "tcenter"){
					tCenter = KVP.Key.transform.GetChild(i).gameObject;
				}
			}
			for(int i=GOToDelete.Count-1;i>=0;i--){
				Object.DestroyImmediate(GOToDelete[i]);
			}
			
			int CombineCount = vCount;
			if(tCenter != null){ CombineCount +=1; }
			CombineInstance[] combine = new CombineInstance[CombineCount];
			for(int i=0;i<vCount;i++){
				combine[i].mesh = KVP.Value[i].sharedMesh;
        		combine[i].transform = KVP.Value[i].transform.localToWorldMatrix;
			}
			
			int SpecialVertCount = 0;
			if(tCenter != null){
				for(int i=0;i<(CombineCount-1);i++){
					SpecialVertCount += combine[i].mesh.vertexCount;
				}
				MeshFilter tMF = tCenter.GetComponent<MeshFilter>();
				Vector3[] xVerts = tMF.sharedMesh.vertices;
				float xHeight = combine[0].mesh.vertices[combine[0].mesh.vertexCount-1].y;
				for(int i=0;i<xVerts.Length;i++){
					xVerts[i].y = xHeight;
				}
				tMF.sharedMesh.vertices = xVerts;
				combine[CombineCount-1].mesh = tMF.sharedMesh;
        		combine[CombineCount-1].transform = tMF.transform.localToWorldMatrix;	
			}
			
			GameObject tObj = new GameObject(tName);
			MeshFilter MF = tObj.AddComponent<MeshFilter>();
			Mesh tMesh = new Mesh(); 
    		tMesh.CombineMeshes(combine);
			MF.sharedMesh = tMesh;
			MeshRenderer MR = tObj.AddComponent<MeshRenderer>();
            MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tObj.transform.parent = KVP.Key.transform;
			Vector3[] tVerts = MF.sharedMesh.vertices;
			Vector3 tVect = tObj.transform.localPosition;
//			float tHeight = 0f;
			for(int i=0;i<tVerts.Length;i++){
				tVerts[i] += tVect;
			}
			MF.sharedMesh.vertices = tVerts;
			tObj.transform.localPosition = new Vector3(0f,0f,0f);
			MF.sharedMesh.RecalculateBounds();
			MF.sharedMesh.RecalculateNormals();
			MF.sharedMesh.tangents = GSDRootUtil.ProcessTangents(MF.sharedMesh.triangles,MF.sharedMesh.normals,MF.sharedMesh.uv,MF.sharedMesh.vertices);
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(MF.sharedMesh);
            }
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(tObj, UnityEditor.StaticEditorFlags.LightmapStatic);
            }
            if (tRoad.opt_bIsStatic) {
                tObj.isStatic = true;
            }
 

			if(bMainPlates){
				MeshCollider MC = tObj.AddComponent<MeshCollider>();
				MC.sharedMesh = MF.sharedMesh;
				MC.material = tRoad.RoadPhysicMaterial;
			}
			
			if(bMainPlates && tCenter != null){
				Object.DestroyImmediate(tCenter);	
			}

			SaveMesh(SaveMeshTypeEnum.Intersection,MF.sharedMesh,tRoad,tName);
		}
		
		private MeshFilter MeshSetup2_Intersection_Helper(ref Mesh xMesh,ref Vector2[] uv,ref Vector4[] tangents, ref GameObject MasterObj, string tName, string tMat, bool bCollider = false){
			if(xMesh == null){ return null; }
			xMesh.uv = uv;
            xMesh.tangents = tangents;
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(xMesh);
            }

			GameObject tObj = new GameObject(tName);
			tObj.transform.parent = MasterObj.transform;
			MeshFilter MF = tObj.AddComponent<MeshFilter>();
			MF.sharedMesh = xMesh;
			if(bCollider){
				MeshCollider MC = tObj.AddComponent<MeshCollider>();
				MC.sharedMesh = MF.sharedMesh;
			}
			if(tMat.Length < 1){ return null; }
			MeshRenderer MR = tObj.AddComponent<MeshRenderer>();
            MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial(tMat,MR);
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(tObj, UnityEditor.StaticEditorFlags.LightmapStatic);
            }
            if (tRoad.opt_bIsStatic) {
                tObj.isStatic = true;
            }

			return MF;
		}
		#endregion			
		
		private Mesh MeshSetup2_Helper(ref Mesh xMesh, Vector2[] uv, Vector4[] tangents, ref GameObject tObj, bool bMarker,bool bShoulder = false, bool bBridge = false){
			xMesh.uv = uv;
            xMesh.tangents = tangents;
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(xMesh);
            }
			MeshFilter MF = tObj.AddComponent<MeshFilter>();
			MF.sharedMesh = xMesh;
			MeshCollider MC = null;
            if (tRoad.opt_bUseMeshColliders) {
				MC = tObj.AddComponent<MeshCollider>();
				MC.sharedMesh = MF.sharedMesh;
			}
			MeshRenderer MR = tObj.AddComponent<MeshRenderer>();
			
			if(bShoulder){
				int mCounter = 0;
				if(tRoad.ShoulderMaterial1 != null){
					mCounter+=1;
					if(tRoad.ShoulderMaterial2 != null){
						mCounter+=1;
						if(tRoad.ShoulderMaterial3 != null){
							mCounter+=1;
							if(tRoad.ShoulderMaterial4 != null){
								mCounter+=1;
							}
						}
					}
				}
				
				if(mCounter > 0){
					Material[] tMats = new Material[mCounter];
					if(tRoad.ShoulderMaterial1 != null){
						tMats[0] = tRoad.ShoulderMaterial1;
						if(tRoad.ShoulderMaterial2 != null){
							tMats[1] = tRoad.ShoulderMaterial2;
							if(tRoad.ShoulderMaterial3 != null){
								tMats[2] = tRoad.ShoulderMaterial3;	
								if(tRoad.ShoulderMaterial4 != null){
									tMats[3] = tRoad.ShoulderMaterial4;	
								}
							}
						}
					}
					MR.materials = tMats;
				}
			}else{
				if(bMarker){
					int mCounter = 0;
					if(tRoad.RoadMaterialMarker1 != null){
						mCounter+=1;
						if(tRoad.RoadMaterialMarker2 != null){
							mCounter+=1;
							if(tRoad.RoadMaterialMarker3 != null){
								mCounter+=1;
								if(tRoad.RoadMaterialMarker4 != null){
									mCounter+=1;
								}
							}
						}
					}
					
					if(mCounter > 0){
						Material[] tMats = new Material[mCounter];
						if(tRoad.RoadMaterialMarker1 != null){
							tMats[0] = tRoad.RoadMaterialMarker1;
							if(tRoad.RoadMaterialMarker2 != null){
								tMats[1] = tRoad.RoadMaterialMarker2;	
								if(tRoad.RoadMaterialMarker3 != null){
									tMats[2] = tRoad.RoadMaterialMarker3;	
									if(tRoad.RoadMaterialMarker4 != null){
										tMats[3] = tRoad.RoadMaterialMarker4;	
									}
								}
							}
						}
						MR.materials = tMats;
					}
				}else{
					int mCounter = 0;
					if(tRoad.RoadMaterial1 != null){
						mCounter+=1;
						if(tRoad.RoadMaterial2 != null){
							mCounter+=1;
							if(tRoad.RoadMaterial3 != null){
								mCounter+=1;
								if(tRoad.RoadMaterial4 != null){
									mCounter+=1;
								}
							}
						}
					}
					if(mCounter > 0){
						Material[] tMats = new Material[mCounter];
						if(tRoad.RoadMaterial1 != null){
							tMats[0] = tRoad.RoadMaterial1;
							if(tRoad.RoadMaterial2 != null){
								tMats[1] = tRoad.RoadMaterial2;	
								if(tRoad.RoadMaterial3 != null){
									tMats[2] = tRoad.RoadMaterial3;	
									if(tRoad.RoadMaterial4 != null){
										tMats[3] = tRoad.RoadMaterial4;	
									}
								}
							}
						}
						MR.materials = tMats;
					}
					
					if(MC){ MC.sharedMaterial = tRoad.RoadPhysicMaterial; }
				}
			}

            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(tObj, UnityEditor.StaticEditorFlags.LightmapStatic);
            }
            if (tRoad.opt_bIsStatic) {
                tObj.isStatic = true;
            }

			return xMesh;
		}
		
		private bool MeshSetup2_Helper_RoadCuts(int i,ref Mesh zMesh, Vector2[] uv, Vector4[] tangents, ref GameObject MasterObj, bool bIsMarkers, out GameObject CreatedObj){
			string tName = "RoadCut" + i.ToString();
			if(bIsMarkers){
				tName = "Markers" + i.ToString();
			}
			CreatedObj = new GameObject(tName);
			
			if(!bIsMarkers){
				RoadCutNodes[i].RoadCut_world = CreatedObj;
			}else{
				RoadCutNodes[i].RoadCut_marker = CreatedObj;	
			}

			CreatedObj.transform.position = cut_RoadVectorsHome[i];
			zMesh.uv = uv;
            zMesh.tangents = tangents;
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(zMesh);
            }
			MeshFilter MF = CreatedObj.AddComponent<MeshFilter>();
			MF.sharedMesh = zMesh;

			MeshCollider MC = null;
            if (tRoad.opt_bUseMeshColliders && !bIsMarkers) {
				MC = CreatedObj.AddComponent<MeshCollider>();
				if(MC.sharedMesh == null){
					MC.sharedMesh = MF.sharedMesh;
				}
			}
			MeshRenderer MR = CreatedObj.AddComponent<MeshRenderer>();
            
            //Disable shadows for road cuts and markers:
            MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            int mCounter = 0;
			bool bHasMats = false;
			
			if(bIsMarkers){
                

                //Get the mat count:
                if(tRoad.RoadMaterialMarker1 != null){
					mCounter+=1;
					if(tRoad.RoadMaterialMarker2 != null){
						mCounter+=1;
						if(tRoad.RoadMaterialMarker3 != null){
							mCounter+=1;
							if(tRoad.RoadMaterialMarker4 != null){
								mCounter+=1;
							}
						}
					}
				}
				//Apply mats:
				if(mCounter > 0){
					Material[] tMats = new Material[mCounter];
					if(tRoad.RoadMaterialMarker1 != null){
						tMats[0] = tRoad.RoadMaterialMarker1;
						if(tRoad.RoadMaterialMarker2 != null){
							tMats[1] = tRoad.RoadMaterialMarker2;	
							if(tRoad.RoadMaterialMarker3 != null){
								tMats[2] = tRoad.RoadMaterialMarker3;	
								if(tRoad.RoadMaterialMarker4 != null){
									tMats[3] = tRoad.RoadMaterialMarker4;	
								}
							}
						}
					}
					MR.materials = tMats;
					bHasMats = true;
				}
			}else{
				//Get the mat count:
				if(tRoad.RoadMaterial1 != null){
					mCounter+=1;
					if(tRoad.RoadMaterial2 != null){
						mCounter+=1;
						if(tRoad.RoadMaterial3 != null){
							mCounter+=1;
							if(tRoad.RoadMaterial4 != null){
								mCounter+=1;
							}
						}
					}
				}
				//Apply mats:
				if(mCounter > 0){
					Material[] tMats = new Material[mCounter];
					if(tRoad.RoadMaterial1 != null){
						tMats[0] = tRoad.RoadMaterial1;
						if(tRoad.RoadMaterial2 != null){
							tMats[1] = tRoad.RoadMaterial2;	
							if(tRoad.RoadMaterial3 != null){
								tMats[2] = tRoad.RoadMaterial3;	
								if(tRoad.RoadMaterial4 != null){
									tMats[3] = tRoad.RoadMaterial4;	
								}
							}
						}
					}
					MR.materials = tMats;
					bHasMats = true;
				}	
			}
		
			CreatedObj.transform.parent = MasterObj.transform;
			if(!bIsMarkers && MC != null){ MC.sharedMaterial = tRoad.RoadPhysicMaterial; }
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(CreatedObj, UnityEditor.StaticEditorFlags.LightmapStatic);
            }
            if (tRoad.opt_bIsStatic) {
                CreatedObj.isStatic = true;
            }

			return bHasMats;
		}
		
		private bool MeshSetup2_Helper_CutsShoulder(int i,ref Mesh zMesh, Vector2[] uv, Vector4[] tangents, ref GameObject MasterObj, bool bIsLeft, bool bIsMarkers, out GameObject CreatedObj){
	
			string tName = "";
			if(bIsMarkers){
				if(bIsLeft){
					tName = "Markers" + i.ToString();
				}else{
					tName = "Markers" + i.ToString();
				}
			}else{
				if(bIsLeft){
					tName = "SCutL" + i.ToString();
				}else{
					tName = "SCutR" + i.ToString();
				}
			}
			
			CreatedObj = new GameObject(tName);
			if(bIsLeft){
				CreatedObj.transform.position = cut_ShoulderL_VectorsHome[i];
				if(!bIsMarkers){
					ShoulderCutsLNodes[i].ShoulderCutL_world = CreatedObj;
				}else{
					ShoulderCutsLNodes[i].ShoulderCutL_marker = CreatedObj;
				}
				
			}else{
				CreatedObj.transform.position = cut_ShoulderR_VectorsHome[i];
				if(!bIsMarkers){
					ShoulderCutsRNodes[i].ShoulderCutR_world = CreatedObj;
				}else{
					ShoulderCutsRNodes[i].ShoulderCutR_marker = CreatedObj;
				}
			}
			
			MeshCollider MC = null;
            if (tRoad.opt_bUseMeshColliders) {
				MC = CreatedObj.AddComponent<MeshCollider>();
			}
			
			zMesh.uv = uv;
            zMesh.tangents = tangents;
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(zMesh);
            }
			MeshFilter MF = CreatedObj.AddComponent<MeshFilter>();
			MF.sharedMesh = zMesh;

            if (tRoad.opt_bUseMeshColliders) {
				if(MC.sharedMesh == null){
					MC.sharedMesh = MF.sharedMesh;
				}
			}
			int mCounter = 0;
			bool bHasMats = false;

            //Disable shadows for road cuts and markers:
            MeshRenderer MR = CreatedObj.AddComponent<MeshRenderer>();
            MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if(bIsMarkers){
				if(tRoad.ShoulderMaterialMarker1 != null){
					mCounter+=1;
					if(tRoad.ShoulderMaterialMarker2 != null){
						mCounter+=1;
						if(tRoad.ShoulderMaterialMarker3 != null){
							mCounter+=1;
							if(tRoad.ShoulderMaterialMarker4 != null){
								mCounter+=1;
							}
						}
					}
				}
				if(mCounter > 0){
					Material[] tMats = new Material[mCounter];
					if(tRoad.ShoulderMaterialMarker1 != null){
						tMats[0] = tRoad.ShoulderMaterialMarker1;
						if(tRoad.ShoulderMaterialMarker2 != null){
							tMats[1] = tRoad.ShoulderMaterialMarker2;	
							if(tRoad.ShoulderMaterialMarker3 != null){
								tMats[2] = tRoad.ShoulderMaterialMarker3;	
								if(tRoad.ShoulderMaterialMarker4 != null){
									tMats[3] = tRoad.ShoulderMaterialMarker4;	
								}
							}
						}
					}
					
					MR.materials = tMats;
					bHasMats = true;
				}
			}else{
				if(tRoad.ShoulderMaterial1 != null){
					mCounter+=1;
					if(tRoad.ShoulderMaterial2 != null){
						mCounter+=1;
						if(tRoad.ShoulderMaterial3 != null){
							mCounter+=1;
							if(tRoad.ShoulderMaterial4 != null){
								mCounter+=1;
							}
						}
					}
				}
				if(mCounter > 0){
					Material[] tMats = new Material[mCounter];
					if(tRoad.ShoulderMaterial1 != null){
						tMats[0] = tRoad.ShoulderMaterial1;
						if(tRoad.ShoulderMaterial2 != null){
							tMats[1] = tRoad.ShoulderMaterial2;	
							if(tRoad.ShoulderMaterial3 != null){
								tMats[2] = tRoad.ShoulderMaterial3;	
								if(tRoad.ShoulderMaterial4 != null){
									tMats[3] = tRoad.ShoulderMaterial4;	
								}
							}
						}
					}
					MR.materials = tMats;
					MR = null;
					bHasMats = true;
				}
			}

			if(!bIsMarkers && MC != null){ MC.sharedMaterial = tRoad.ShoulderPhysicMaterial; }
			CreatedObj.transform.parent = MasterObj.transform;
            if (tRoad.opt_bIsLightmapped) {
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(CreatedObj, UnityEditor.StaticEditorFlags.LightmapStatic);
            }
            if (tRoad.opt_bIsStatic) {
                CreatedObj.isStatic = true;
            }
 
			MF = null;
			MC = null;
			
			return bHasMats;
		}
	  	#endregion
		
		private static void SaveMesh(SaveMeshTypeEnum SaveType, Mesh tMesh, GSDRoad tRoad, string tName){
			if(!tRoad.GSDRS.opt_bSaveMeshes){ return; }
			
			//string tSceneName = System.IO.Path.GetFileName(UnityEditor.EditorApplication.currentScene).ToLower().Replace(".unity","");
            string tSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            tSceneName = tSceneName.Replace("/","");
			tSceneName = tSceneName.Replace(".","");
			string tFolderName = "";
			if(SaveType == SaveMeshTypeEnum.Road){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/Roads/";
			}else if(SaveType == SaveMeshTypeEnum.Shoulder){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/Shoulders/";
			}else if(SaveType == SaveMeshTypeEnum.Intersection){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/Intersections/";
			}else if(SaveType == SaveMeshTypeEnum.Railing){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/Railings/";
			}else if(SaveType == SaveMeshTypeEnum.Center){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/CenterDividers/";
			}else if(SaveType == SaveMeshTypeEnum.RoadCut){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/Roads/Cuts/";
			}else if(SaveType == SaveMeshTypeEnum.SCut){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/Shoulders/Cuts/";
			}else if(SaveType == SaveMeshTypeEnum.RoadConn){
				tFolderName = "Assets/RoadArchitect/Mesh/Generated/RoadConn/";
			}
			
			string xPath = Application.dataPath.Replace("/Assets","/" + tFolderName);
			if(!System.IO.Directory.Exists(xPath)){
				System.IO.Directory.CreateDirectory(xPath);
			}
			
			string tRoadName = tRoad.transform.name;
			string FinalName = tFolderName + tSceneName + "-" + tRoadName + "-" + tName + ".asset";
			if(SaveType == SaveMeshTypeEnum.Intersection){
				FinalName = tFolderName + tSceneName + "-" + tName + ".asset";
			}
			
			UnityEditor.AssetDatabase.CreateAsset(tMesh,FinalName);
		}
	}

	public static class GSDGeneralEditor{
		#region "Terrain history serialization"
		//http://forum.unity3d.com/threads/32647-C-Sharp-Binary-Serialization
		//http://answers.unity3d.com/questions/363477/c-how-to-setup-a-binary-serialization.html
		
		// === This is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
	    // Do not change this
	    public sealed class VersionDeserializationBinder : SerializationBinder{
		    public override System.Type BindToType(string assemblyName, string typeName){
			    if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName)){
				    System.Type typeToDeserialize = null;
				    assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;
				    // The following line of code returns the type.
				    typeToDeserialize = System.Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
				    return typeToDeserialize;
			    }
			    return null;
		    }
	    }

	    public static void TerrainHistory_Save(List<GSDTerrainHistoryMaker> tObj, GSDRoad tRoad){
			string tPath = CheckNonAssetDirTH() + GetRoadTHFilename(ref tRoad);
			if(string.IsNullOrEmpty(tPath) || tPath.Length < 2){ return; }
		    Stream stream = File.Open(tPath, FileMode.Create);
		    BinaryFormatter bformatter = new BinaryFormatter();
		    bformatter.Binder = new VersionDeserializationBinder();
		    bformatter.Serialize(stream, tObj);
			tRoad.TerrainHistoryByteSize = (stream.Length*0.001f).ToString("n0") + " kb";
		    stream.Close();
		}
		
		public static void TerrainHistory_Delete(GSDRoad tRoad){
			string tPath = CheckNonAssetDirTH() + GetRoadTHFilename(ref tRoad);
			if(System.IO.File.Exists(tPath)){
				System.IO.File.Delete(tPath);	
			}
		}
		
		public static List<GSDTerrainHistoryMaker> TerrainHistory_Load(GSDRoad tRoad){
			string tPath = CheckNonAssetDirTH() + GetRoadTHFilename(ref tRoad);
			if(string.IsNullOrEmpty(tPath) || tPath.Length < 2){ return null; }
			if(!File.Exists(tPath)){ return null; }
		    List<GSDTerrainHistoryMaker> result;
            Stream stream = File.Open(tPath, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Binder = new VersionDeserializationBinder();
//			try{
            	result = (List<GSDTerrainHistoryMaker>)bFormatter.Deserialize(stream) as List<GSDTerrainHistoryMaker>;
//			}catch{
//				result = null;	
//			}
            stream.Close();
            return result;
		}
		
		private static string GetRoadTHFilename(ref GSDRoad tRoad){
			//string tSceneName = System.IO.Path.GetFileName(UnityEditor.EditorApplication.currentScene).ToLower().Replace(".unity","");
            string tSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            tSceneName = tSceneName.Replace("/","");
			tSceneName = tSceneName.Replace(".","");
			string tRoadName = tRoad.GSDRS.transform.name.Replace("RoadArchitectSystem","RAS") + "-" + tRoad.transform.name;
			return tSceneName + "-" + tRoadName + "-TH.gsd";
		}
		
		public static string CheckNonAssetDir(){
			string tPath = GSDRootUtil.Dir_GetBase();
			if(!System.IO.Directory.Exists(tPath)){
				System.IO.Directory.CreateDirectory(tPath);	
			}
			if(System.IO.Directory.Exists(tPath)){
				return tPath + "/";
			}else{
				return "";
			}
		}
		
		public static string CheckNonAssetDirTH(){
			CheckNonAssetDir();
			string xPath = GSDRootUtil.Dir_GetTH();
			if(!System.IO.Directory.Exists(xPath)){
				System.IO.Directory.CreateDirectory(xPath);	
			}
			if(System.IO.Directory.Exists(xPath)){
				return xPath;
			}else{
				return "";
			}
		}
		
		public static string CheckNonAssetDirLibrary(){
			CheckNonAssetDir();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			if(!System.IO.Directory.Exists(xPath)){
				System.IO.Directory.CreateDirectory(xPath);	
			}
			if(System.IO.Directory.Exists(xPath)){
				return xPath;
			}else{
				return "";
			}
		}
		
		public static void CheckNonAssetDirs(){
			CheckNonAssetDir();
			CheckNonAssetDirTH();
			CheckNonAssetDirLibrary();
		}
		#endregion
	}

	public static class GSDRoadUtil{
		private const string FileSepString = "\n!!!! MICROGSD !!!!\n";
        private const string FileSepStringCRLF = "\r\n!!!! MICROGSD !!!!\r\n";

        public static Terrain GetTerrain(Vector3 tVect){
			return GetTerrain_Do(ref tVect);
		}
		private static Terrain GetTerrain_Do(ref Vector3 tVect){
			//Sphere cast 5m first. Then raycast down 1000m, then up 1000m.
			Collider[] tColliders = Physics.OverlapSphere(tVect,10f);
			if(tColliders != null){
				int tCollidersLength = tColliders.Length;
				for(int i=0;i<tCollidersLength;i++){
					Terrain tTerrain = tColliders[i].transform.GetComponent<Terrain>();
					if(tTerrain){
						tColliders = null;
						return tTerrain;
					}
				}
				tColliders = null;
			}
			
			RaycastHit[] tHits;
			tHits = Physics.RaycastAll(tVect,Vector3.down,1000f);
			int tHitsLength=0;
			if(tHits != null){
				tHitsLength = tHits.Length;
				for(int i=0;i<tHitsLength;i++){
					Terrain tTerrain = tHits[i].collider.transform.GetComponent<Terrain>();
					if(tTerrain){
						tHits = null;
						return tTerrain;
					}
				}
				tHits = null;
			}
			
			tHits = Physics.RaycastAll(tVect,Vector3.up,1000f);
			if(tHits != null){
				tHitsLength = tHits.Length;
				for(int i=0;i<tHitsLength;i++){
					Terrain tTerrain = tHits[i].collider.transform.GetComponent<Terrain>();
					if(tTerrain){
						tHits = null;
						return tTerrain;
					}
				}
				tHits = null;
			}
			return null;	
		}
		
        #region "Terrain history"
        public static void ConstructRoad_StoreTerrainHistory(ref GSDRoad tRoad){
            ConstructRoad_DoStoreTerrainHistory(ref tRoad);
        }
        private static void ConstructRoad_DoStoreTerrainHistory(ref GSDRoad tRoad){
            Object[] TIDs = GameObject.FindObjectsOfType(typeof(GSDTerrain));
			
			HashSet<int> tTIDS = new HashSet<int>();
			foreach(GSDTerrain TID in TIDs){
				tTIDS.Add (TID.GSDID);
			}
			
			if(tRoad.TerrainHistory != null && tRoad.TerrainHistory.Count > 0){
				//Delete unnecessary terrain histories:
				foreach(GSDTerrainHistoryMaker THf in tRoad.TerrainHistory){
					if(!tTIDS.Contains(THf.TID)){
						THf.Nullify();
						THf.bDestroyMe = true;
					}
				}
				
				int hCount = tRoad.TerrainHistory.Count;
				for(int i=hCount-1;i>=0;i--){
					if(tRoad.TerrainHistory[i].bDestroyMe){
						GSDTerrainHistoryMaker THf = tRoad.TerrainHistory[i];
						tRoad.TerrainHistory.RemoveAt(i);
						if(THf != null){ THf = null; }
					}
				}
			}

            if (tRoad.TerrainHistory == null) { tRoad.TerrainHistory = new List<GSDTerrainHistoryMaker>(); }
            foreach (GSDTerraforming.TempTerrainData TTD in tRoad.EditorTTDList){
                GSDTerrainHistoryMaker TH = null;
                GSDTerrain TID = null;
                //Get TID:
                foreach(GSDTerrain _TID in TIDs){
                    if (_TID.GSDID == TTD.GSDID)
                    {
                        TID = _TID;
                    }
                }

				if(tRoad.TerrainHistory == null) { tRoad.TerrainHistory = new List<GSDTerrainHistoryMaker>(); }
				if(TID == null){ continue; }
				
				int THCount = tRoad.TerrainHistory.Count;
				bool bContainsTID = false;
				for(int i=0;i<THCount;i++){
					if(tRoad.TerrainHistory[i].TID == TID.GSDID){
						bContainsTID = true;
						break;
					}
				}
				
                if(!bContainsTID){
                    GSDTerrainHistoryMaker THx = new GSDTerrainHistoryMaker();
					THx.TID = TID.GSDID;
                    tRoad.TerrainHistory.Add(THx);
                }
				
				TH = null;
				for(int i=0;i<THCount;i++){
					if(tRoad.TerrainHistory[i].TID == TID.GSDID){
						TH = tRoad.TerrainHistory[i];
						break;
					}
				}
				if(TH == null){ continue; }

                //Heights:
                if(tRoad.opt_HeightModEnabled){
                    if(TTD.cX != null && TTD.cY != null){
	                    TH.x1 = new int[TTD.cI];
	                    System.Array.Copy(TTD.cX, 0, TH.x1, 0, TTD.cI);
	                    TH.y1 = new int[TTD.cI];
	                    System.Array.Copy(TTD.cY, 0, TH.y1, 0, TTD.cI);
	                    TH.h = new float[TTD.cI];
	                    System.Array.Copy(TTD.oldH, 0, TH.h, 0, TTD.cI);
	                    TH.cI = TTD.cI;
					}
                }else{
                    TH.x1 = null;
                    TH.y1 = null;
                    TH.h = null;
                    TH.cI = 0;
                }
                //Details:
                if(tRoad.opt_DetailModEnabled){
					int TotalSize = 0;
					for(int i=0;i<TTD.DetailLayersCount;i++){
						TotalSize += TTD.DetailsI[i];
					}
					
//					float tHalf = (float)TotalSize / 2f;
//					int IntHalf = Mathf.CeilToInt(tHalf);
					
					TH.DetailsX = new int[TotalSize];
					TH.DetailsY = new int[TotalSize];
					TH.DetailsOldValue = new int[TotalSize];
	
					int RunningIndex = 0;
					int cLength = 0;
					for(int i=0;i<TTD.DetailLayersCount;i++){
						cLength = TTD.DetailsI[i];
						if(cLength < 1){ continue; }
						System.Array.Copy(TTD.DetailsX[i].ToArray(),0,TH.DetailsX,RunningIndex,cLength);
						System.Array.Copy(TTD.DetailsY[i].ToArray(),0,TH.DetailsY,RunningIndex,cLength);
						System.Array.Copy(TTD.OldDetailsValue[i].ToArray(),0,TH.DetailsOldValue,RunningIndex,cLength);
						RunningIndex += TTD.DetailsI[i];
					}
					
//                    TH.DetailsX = TTD.DetailsX;
//                    TH.DetailsY = TTD.DetailsY;
//                    TH.DetailsOldValue = TTD.OldDetailsValue;
                    TH.DetailsI = TTD.DetailsI;
                    TH.DetailLayersCount = TTD.DetailLayersCount;
                }else{
                    TH.DetailsX = null;
                    TH.DetailsY = null;
                    TH.DetailsOldValue = null;
                    TH.DetailsI = null;
                    TH.DetailLayersCount = 0;
                }
                //Trees:
                if(tRoad.opt_TreeModEnabled){
					if(TTD.TreesOld != null){
	                    TH.MakeGSDTrees(ref TTD.TreesOld);
						TTD.TreesOld.Clear();
						TTD.TreesOld = null;
	                    TH.TreesI = TTD.TreesI;
					}
                }else{
                    TH.TreesOld = null;
                    TH.TreesI = 0;
                }
            }
			
//			//TerrainHistoryRaw
//			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("TerrainHistorySerialize"); }
//			TerrainHistorySerialize(ref tRoad);
//			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
        }
		
//		static void TerrainHistorySerialize(ref GSDRoad tRoad) {
//			MemoryStream ms = new MemoryStream();
//	        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//			formatter.Serialize(ms,tRoad.TerrainHistory);
//			ms.Close();
//			tRoad.TerrainHistoryRaw = ms.ToArray();
//	        ms = null;
//	    }
//		
//		static void TerrainHistoryDeserialize(ref GSDRoad tRoad) {
//			MemoryStream ms = new MemoryStream(tRoad.TerrainHistoryRaw);
//	        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//			tRoad.TerrainHistory = (List<GSDTerrainHistoryMaker>)formatter.Deserialize(ms);
//			ms.Close();
//	        ms = null;
//	    }

        public static void ConstructRoad_ResetTerrainHistory(ref GSDRoad tRoad)
        {
            ConstructRoad_DoResetTerrainHistory(ref tRoad);
        }

        private static void ConstructRoad_DoResetTerrainHistory(ref GSDRoad tRoad)
        {
            if (tRoad.TerrainHistory != null)
            {
                tRoad.TerrainHistory.Clear();
                tRoad.TerrainHistory = null;
            }
        }
        #endregion

		[System.Serializable]
		public class Construction3DTri{
			public Vector3 P1,P2,P3;	
			const float NearDist = 0.15f;
			const float NearDistSQ = 0.0225f;
			Vector2[] poly2D;
			Vector3[] poly3D;
			public float MaxDistance = 200f;
			public float MaxDistanceSq = 200f;
			public Vector3 normal = default(Vector3);
			public Vector3 pMiddle = default(Vector3);
			public float MinI=0f;
			public float MaxI=1f;
			
			public Construction3DTri(Vector3 _P1,Vector3 _P2,Vector3 _P3, float _MinI, float _MaxI){
				MinI = _MinI;
				MaxI = _MaxI;
				P1 = _P1;
				P2 = _P2;
				P3 = _P3;

				poly2D = new Vector2[3];
				poly2D[0] = new Vector2(P1.x,P1.z);
				poly2D[1] = new Vector2(P2.x,P2.z);
				poly2D[2] = new Vector2(P3.x,P3.z);
				
				poly3D = new Vector3[3];
				poly3D[0] = P1;
				poly3D[1] = P2;
				poly3D[2] = P3;
				
				float[] tMaxes = new float[3];
				tMaxes[0] = Vector3.Distance(P1,P2);
				tMaxes[1] = Vector3.Distance(P1,P3);
				tMaxes[2] = Vector3.Distance(P2,P3);
				MaxDistance = Mathf.Max(tMaxes) * 1.5f;
				
				float[] tMaxesSQ = new float[3];
				tMaxesSQ[0] = Vector3.SqrMagnitude(P1-P2);
				tMaxesSQ[1] = Vector3.SqrMagnitude(P1-P3);
				tMaxesSQ[2] = Vector3.SqrMagnitude(P2-P3);
				MaxDistanceSq = Mathf.Max(tMaxesSQ) * 1.5f;

				PlaneFrom3Points(out normal,out pMiddle,P1,P2,P3);
				
				normal = Vector3.Cross((P3-P1),(P2-P1));
				
////				//This creates middle point:
//				Vector3 tMiddle1 = ((P3-P1)*0.5f)+P1;
//				Vector3 tMiddle2 = ((P2-P1)*0.5f)+P1;
//				pMiddle = ((tMiddle2-tMiddle1)*0.5f)+tMiddle1;
			}
			
			//Get the intersection between a line and a plane. 
			//If the line and plane are not parallel, the function outputs true, otherwise false.
			public Vector3 LinePlaneIntersection(ref Vector3 F1){
				F1.y = 0f;
				
				//calculate the distance between the linePoint and the line-plane intersection point
				float dotNumerator = Vector3.Dot((pMiddle - F1), normal);
				float dotDenominator = Vector3.Dot(Vector3.up.normalized, normal);
		
				//line and plane are not parallel
				if(!IsApproximately(0f,dotDenominator,0.001f)){
					//get the coordinates of the line-plane intersection point
					return (F1 + (Vector3.up.normalized * (dotNumerator / dotDenominator)));
				}else{
					//output not valid
					return default(Vector3);
				}
			}
			
			static bool IsApproximately(float a, float b){
		    	return IsApproximately(a, b, 0.01f);
		    }
		     
		    static bool IsApproximately(float a, float b, float tolerance){
		   		return Mathf.Abs(a - b) < tolerance;
		    }

			//Convert a plane defined by 3 points to a plane defined by a vector and a point. 
			//The plane point is the middle of the triangle defined by the 3 points.
			public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC){
				planeNormal = Vector3.zero;
				planePoint = Vector3.zero;
		 
				//Make two vectors from the 3 input points, originating from point A
				Vector3 AB = pointB - pointA;
				Vector3 AC = pointC - pointA;
		 
				//Calculate the normal
				planeNormal = Vector3.Normalize(Vector3.Cross(AB, AC));
		 
				//Get the points in the middle AB and AC
				Vector3 middleAB = pointA + (AB / 2.0f);
				Vector3 middleAC = pointA + (AC / 2.0f);
		 
				//Get vectors from the middle of AB and AC to the point which is not on that line.
				Vector3 middleABtoC = pointC - middleAB;
				Vector3 middleACtoB = pointB - middleAC;
		 
				//Calculate the intersection between the two lines. This will be the center 
				//of the triangle defined by the 3 points.
				//We could use LineLineIntersection instead of ClosestPointsOnTwoLines but due to rounding errors 
				//this sometimes doesn't work.
				Vector3 temp;
				ClosestPointsOnTwoLines(out planePoint, out temp, middleAB, middleABtoC, middleAC, middleACtoB);
			}
			
			//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
			//to each other. This function finds those two points. If the lines are not parallel, the function 
			//outputs true, otherwise false.
			public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){
		 
				closestPointLine1 = Vector3.zero;
				closestPointLine2 = Vector3.zero;
		 
				float a = Vector3.Dot(lineVec1, lineVec1);
				float b = Vector3.Dot(lineVec1, lineVec2);
				float e = Vector3.Dot(lineVec2, lineVec2);
		 
				float d = a*e - b*b;
		 
				//lines are not parallel
				if(d != 0.0f){
		 
					Vector3 r = linePoint1 - linePoint2;
					float c = Vector3.Dot(lineVec1, r);
					float f = Vector3.Dot(lineVec2, r);
		 
					float s = (b*f - c*e) / d;
					float t = (a*f - c*b) / d;
		 
					closestPointLine1 = linePoint1 + lineVec1 * s;
					closestPointLine2 = linePoint2 + lineVec2 * t;
		 
					return true;
				}
		 
				else{
					return false;
				}
			}
			
			//create a vector of direction "vector" with length "size"
			public static Vector3 SetVectorLength(Vector3 vector, float size){
		 
				//normalize the vector
				Vector3 vectorNormalized = Vector3.Normalize(vector);
		 
				//scale the vector
				return vectorNormalized *= size;
			}
			
//			public bool Contains2D(ref Vector2 p){
//				return Contains2D_Do(ref p);
//			}
			public bool Contains2D(ref Vector2 p){
				if(Vector2.SqrMagnitude(p-poly2D[0]) > MaxDistanceSq){
					return false; 
				}
//				if(Vector2.Distance(p,P1) > MaxDistance){ return false; }
//				if(poly2D.Length != 3){ return false; }
				
				Vector2 x1 = default(Vector2);
				Vector2 x2 = default(Vector2);
				Vector2 oldPoint = default(Vector2);
				Vector2 newPoint = default(Vector2);
				bool inside = false;
				
				inside = false;
				oldPoint = new Vector2(poly2D[3 - 1].x, poly2D[3 - 1].y);
				for (int i = 0; i < 3; i++){
					newPoint = new Vector2(poly2D[i].x, poly2D[i].y);
					if (newPoint.x > oldPoint.x){
						x1 = oldPoint;
						x2 = newPoint;
					}else{
						x1 = newPoint;
						x2 = oldPoint;
					}
					if ((newPoint.x < p.x) == (p.x <= oldPoint.x) && (p.y - x1.y)*(x2.x - x1.x) < (x2.y - x1.y)*(p.x - x1.x)){
						inside = !inside;
					}
					oldPoint = newPoint;
				}
				return inside;
			}
			
			
			public bool Contains2D(ref Vector3 p){
				Vector2 tVect = new Vector2(p.x,p.z);
				return Contains2D(ref tVect);
			}

			public bool Near(ref Vector3 tVect, out Vector3 TheNearVect){
				if(Vector3.SqrMagnitude(tVect-P1) > MaxDistanceSq){ 
//				if(Vector3.Distance(tVect,P1) > MaxDistance){ 
					TheNearVect = default(Vector3);
					return false; 
				}
			
//				if(Vector3.Distance(tVect,P1) < NearDist){
				if(Vector3.SqrMagnitude(tVect-P1) < NearDistSQ){
					TheNearVect = P1;
					return true;
				}
//				if(Vector3.Distance(tVect,P2) < NearDist){
				if(Vector3.SqrMagnitude(tVect-P2) < NearDistSQ){
					TheNearVect = P2;
					return true;
				}
//				if(Vector3.Distance(tVect,P3) < NearDist){
				if(Vector3.SqrMagnitude(tVect-P3) < NearDistSQ){
					TheNearVect = P3;
					return true;
				}
				TheNearVect = default(Vector3);
				return false;	
			}
			
			public string ToStringGSD(){
				return ("P1:" + P1.ToString() + " P2:" + P2.ToString() + " P3:" + P3.ToString());
			}
		}
		
		public class Construction2DRect{
				public Vector2 P1,P2,P3,P4;	
				const float NearDist = 0.15f;
				const float NearDistSQ = 0.0225f;
				Vector2[] poly;
				public float MaxDistance = 200f;
				public float MaxDistanceSQ = 200f;
				public float Height = 0f;
				public float MinI = 0f;
				public float MaxI = 0f;
				
				static bool IsApproximately(float a, float b){
			    	return IsApproximately(a, b, 0.01f);
			    }
			     
			    static bool IsApproximately(float a, float b, float tolerance){
			   		return Mathf.Abs(a - b) < tolerance;
			    }
			
				public Construction2DRect(Vector2 _P1,Vector2 _P2,Vector2 _P3,Vector2 _P4,float tHeight = 0f){
					Construction2DRect_Do(ref _P1,ref _P2,ref _P3,ref _P4,ref tHeight);
				}
				private void Construction2DRect_Do(ref Vector2 _P1,ref Vector2 _P2,ref Vector2 _P3,ref Vector2 _P4,ref float tHeight){
					P1 = _P1;
					P2 = _P2;
					P3 = _P3;
					P4 = _P4;
					Height = tHeight;
				
					if(IsApproximately(P1.x,P2.x,0.0001f)){
						P2.x += 0.0002f;
					}
					if(IsApproximately(P1.x,P3.x,0.0001f)){
						P3.x += 0.0002f;
					}
					if(IsApproximately(P1.x,P4.x,0.0001f)){
						P4.x += 0.0002f;
					}
					if(IsApproximately(P2.x,P3.x,0.0001f)){
						P3.x += 0.0002f;
					}
					if(IsApproximately(P2.x,P4.x,0.0001f)){
						P4.x += 0.0002f;
					}
					if(IsApproximately(P3.x,P4.x,0.0001f)){
						P4.x += 0.0002f;
					}
				
					if(IsApproximately(P1.y,P2.y,0.0001f)){
						P2.y += 0.0002f;
					}
					if(IsApproximately(P1.y,P3.y,0.0001f)){
						P3.y += 0.0002f;
					}
					if(IsApproximately(P1.y,P4.y,0.0001f)){
						P4.y += 0.0002f;
					}
					if(IsApproximately(P2.y,P3.y,0.0001f)){
						P3.y += 0.0002f;
					}
					if(IsApproximately(P2.y,P4.y,0.0001f)){
						P4.y += 0.0002f;
					}
					if(IsApproximately(P3.y,P4.y,0.0001f)){
						P4.y += 0.0002f;
					}
				
					//Find two with smallest x, etc		
					float[] tX = new float[4];
					float[] tY = new float[4];

					tX[0] = P1.x;
					tX[1] = P2.x;
					tX[2] = P3.x;
					tX[3] = P4.x;
					
					tY[0] = P1.y;
					tY[1] = P2.y;
					tY[2] = P3.y;
					tY[3] = P4.y;

					float MinX1,MinX2;
					bool bIgnoreP1,bIgnoreP2,bIgnoreP3,bIgnoreP4;
					bIgnoreP1 = bIgnoreP2 = bIgnoreP3 = bIgnoreP4 = false;
					
					//Get top two minimum X
					MinX1 = Mathf.Min(tX);
					tX = new float[3];
					int tCounter = 0;
					if(!IsApproximately(MinX1,P1.x,0.0001f)){
						tX[tCounter] = P1.x; tCounter +=1;
					}
					if(!IsApproximately(MinX1,P2.x,0.0001f)){
						tX[tCounter] = P2.x; tCounter +=1;
					}
					if(!IsApproximately(MinX1,P3.x,0.0001f)){
						tX[tCounter] = P3.x; tCounter +=1;
					}
					if(!IsApproximately(MinX1,P4.x,0.0001f)){
						tX[tCounter] = P4.x; tCounter +=1;
					}
					MinX2 = Mathf.Min(tX);
				
					Vector2 xMin1 = default(Vector2);
					Vector2 xMin2 = default(Vector2);
					if(IsApproximately(MinX1,P1.x,0.0001f)){
						xMin1 = P1;
						bIgnoreP1 = true;
					}else if(IsApproximately(MinX1,P2.x,0.0001f)){
						xMin1 = P2;
						bIgnoreP2 = true;
					}else if(IsApproximately(MinX1,P3.x,0.0001f)){
						xMin1 = P3;
						bIgnoreP3 = true;
					}else if(IsApproximately(MinX1,P4.x,0.0001f)){
						xMin1 = P4;
						bIgnoreP4 = true;
					}
					
					if(IsApproximately(MinX2,P1.x,0.0001f)){
						xMin2 = P1;
						bIgnoreP1 = true;
					}else if(IsApproximately(MinX2,P2.x,0.0001f)){
						xMin2 = P2;
						bIgnoreP2 = true;
					}else if(IsApproximately(MinX2,P3.x,0.0001f)){
						xMin2 = P3;
						bIgnoreP3 = true;
					}else if(IsApproximately(MinX2,P4.x,0.0001f)){
						xMin2 = P4;
						bIgnoreP4 = true;
					}
				
					Vector2 TopLeft = default(Vector2);
					Vector2 BottomLeft = default(Vector2);
					if(xMin1.y > xMin2.y){
						TopLeft = xMin1;
						BottomLeft = xMin2;
					}else{
						TopLeft = xMin2;
						BottomLeft = xMin1;
					}
	
					Vector2 xMax1 = default(Vector2);
					Vector2 xMax2 = default(Vector2);
					bool bXmax1 = false;
					if(!bIgnoreP1){
						xMax1 = P1;
						bXmax1 = true;
					}
					if(!bIgnoreP2){
						if(bXmax1){
							xMax2 = P2;
						}else{
							xMax1 = P2;
							bXmax1 = true;
						}
					}
					if(!bIgnoreP3){
						if(bXmax1){
							xMax2 = P3;
						}else{
							xMax1 = P3;
							bXmax1 = true;
						}
					}
					if(!bIgnoreP4){
						if(bXmax1){
							xMax2 = P4;
						}else{
							xMax1 = P4;
							bXmax1 = true;
						}
					}
	
					Vector2 TopRight = default(Vector2);
					Vector2 BottomRight = default(Vector2);
					if(xMax1.y > xMax2.y){
						TopRight = xMax1;
						BottomRight = xMax2;
					}else{
						TopRight = xMax2;
						BottomRight = xMax1;
					}

					P1 = BottomLeft;
					P2 = BottomRight;
					P3 = TopRight;
					P4 = TopLeft;
				
					poly = new Vector2[4];
					poly[0] = P1;
					poly[1] = P2;
					poly[2] = P3;
					poly[3] = P4;
				
					float[] tMaxes = new float[6];
					tMaxes[0] = Vector2.Distance(P1,P2);
					tMaxes[1] = Vector2.Distance(P1,P3);
					tMaxes[2] = Vector2.Distance(P1,P4);
					tMaxes[3] = Vector2.Distance(P2,P3);
					tMaxes[4] = Vector2.Distance(P2,P4);
					tMaxes[5] = Vector2.Distance(P3,P4);
					MaxDistance = Mathf.Max(tMaxes) * 1.5f;
				
					float[] tMaxesSQ = new float[6];
					tMaxesSQ[0] = Vector2.SqrMagnitude(P1-P2);
					tMaxesSQ[1] = Vector2.SqrMagnitude(P1-P3);
					tMaxesSQ[2] = Vector2.SqrMagnitude(P1-P4);
					tMaxesSQ[3] = Vector2.SqrMagnitude(P2-P3);
					tMaxesSQ[4] = Vector2.SqrMagnitude(P2-P4);
					tMaxesSQ[5] = Vector2.SqrMagnitude(P3-P4);
					MaxDistanceSQ = Mathf.Max(tMaxesSQ) * 1.5f;
				}
			
			

				Vector2 x1 = default(Vector2);
				Vector2 x2 = default(Vector2);
				Vector2 oldPoint = default(Vector2);
				Vector2 newPoint = default(Vector2);
				bool inside = false;
			
//				public bool Contains(ref Vector2 p){
//					return Contains_Do(ref p);
//				}
				public bool Contains(ref Vector2 p){
//					if(Vector2.Distance(p,P1) > MaxDistance){ return false; }
					if(Vector2.SqrMagnitude(p-P1) > MaxDistanceSQ){ return false; }
//					if(poly.Length != 4){ return false; }
				
					inside = false;
					oldPoint = new Vector2(poly[4 - 1].x, poly[4 - 1].y);
					for (int i = 0; i < 4; i++){
						newPoint = new Vector2(poly[i].x, poly[i].y);
						if (newPoint.x > oldPoint.x){
							x1 = oldPoint;
							x2 = newPoint;
						}else{
							x1 = newPoint;
							x2 = oldPoint;
						}
						if ((newPoint.x < p.x) == (p.x <= oldPoint.x) && (p.y - x1.y)*(x2.x - x1.x) < (x2.y - x1.y)*(p.x - x1.x)){
							inside = !inside;
						}
						oldPoint = newPoint;
					}
					return inside;
				}
	
				public bool Near(ref Vector2 tVect, out Vector2 TheNearVect){
					if(Vector2.SqrMagnitude(tVect-P1) > MaxDistanceSQ){
//					if(Vector2.Distance(tVect,P1) > MaxDistance){ 
						TheNearVect = default(Vector2);
						return false; 
					}
				
					if(Vector2.SqrMagnitude(tVect-P1) < NearDistSQ){
//					if(Vector2.Distance(tVect,P1) < NearDist){
						TheNearVect = P1;
						return true;
					}
					if(Vector2.SqrMagnitude(tVect-P2) < NearDistSQ){
//					if(Vector2.Distance(tVect,P2) < NearDist){
						TheNearVect = P2;
						return true;
					}
					if(Vector2.SqrMagnitude(tVect-P3) < NearDistSQ){
//					if(Vector2.Distance(tVect,P3) < NearDist){
						TheNearVect = P3;
						return true;
					}
					if(Vector2.SqrMagnitude(tVect-P4) < NearDistSQ){
//					if(Vector2.Distance(tVect,P4) < NearDist){
						TheNearVect = P4;
						return true;
					}
					TheNearVect = default(Vector2);
					return false;	
				}
			
				public string ToStringGSD(){
					return ("P1:" + P1.ToString() + " P2:" + P2.ToString() + " P3:" + P3.ToString() + " P4:" + P4.ToString());
				}
		}

		public static GSD.Threaded.GSDRoadCreationT.RoadTerrainInfo[] GetRoadTerrainInfos(){
			Object[] tTerrainsObj = GameObject.FindObjectsOfType(typeof(Terrain));
			GSD.Threaded.GSDRoadCreationT.RoadTerrainInfo tInfo;
			List<GSD.Threaded.GSDRoadCreationT.RoadTerrainInfo> tInfos = new List<GSD.Threaded.GSDRoadCreationT.RoadTerrainInfo>();
			foreach(Terrain tTerrain in tTerrainsObj){	
				tInfo = new GSD.Threaded.GSDRoadCreationT.RoadTerrainInfo();
				tInfo.GSDID = tTerrain.transform.gameObject.GetComponent<GSDTerrain>().GSDID;
				tInfo.tBounds = new Rect(tTerrain.transform.position.x,tTerrain.transform.position.z,tTerrain.terrainData.size.x,tTerrain.terrainData.size.z);
				tInfo.hmWidth = tTerrain.terrainData.heightmapWidth;
				tInfo.hmHeight = tTerrain.terrainData.heightmapHeight;
				tInfo.tPos = tTerrain.transform.position;
				tInfo.tSize = tTerrain.terrainData.size;
				tInfo.heights = tTerrain.terrainData.GetHeights(0,0,tInfo.hmWidth,tInfo.hmHeight);
				tInfos.Add(tInfo);
			}
			GSD.Threaded.GSDRoadCreationT.RoadTerrainInfo[] fInfos = new GSD.Threaded.GSDRoadCreationT.RoadTerrainInfo[tInfos.Count];
			int fInfosLength = fInfos.Length;
			for(int i=0;i<fInfosLength;i++){
				fInfos[i] = tInfos[i];		
			}
			tInfos = null;
			return fInfos;
		}
		
	
		// RenderQueue provides ID's for Unity render queues. These can be applied to sub-shader tags,
	    // but it's easier to just set material.renderQueue. Static class instead of enum because these
	    // are int's, so this way client code doesn't need to use typecasting.
	    //
	    // From the documentation:
	    // For special uses in-between queues can be used. Internally each queue is represented
	    // by integer index; Background is 1000, Geometry is 2000, Transparent is 3000 and
	    // Overlay is 4000.
	    //
	    // NOTE: Keep these in numerical order for ease of understanding. Use plurals for start of
	    // a group of layers.
	    public static class RenderQueue{
		    public const int Background = 1000;
		     
		    // Mid-ground.
		    public const int ParallaxLayers = Background + 100; // +1, 2, 3, ... for additional layers
		     
		    // Lines on the ground.
		    public const int GroundLines = Background + 200;
		     
		    public const int Tracks = GroundLines + 0;
		    public const int Routes = GroundLines + 1;
		    public const int IndicatorRings = GroundLines + 2;
			public const int Road = GroundLines + 3;
		     
		    public const int Geometry = 2000;
		     
		     
		    public const int Transparent = 3000;
		     
		    // Lines on the screen. (Over world, but under GUI.)
		    public const int ScreenLines = Transparent + 100;
		     
		    public const int Overlay = 4000;
	    }
		
		
		public static void SaveNodeObjects(ref Splination.SplinatedMeshMaker[] tSplinatedObjects,ref EdgeObjects.EdgeObjectMaker[] tEdgeObjects,ref WizardObject WO){
			SaveNodeObjects_DO(ref tSplinatedObjects, ref tEdgeObjects, ref WO);
		}
		private static void SaveNodeObjects_DO(ref Splination.SplinatedMeshMaker[] tSplinatedObjects,ref EdgeObjects.EdgeObjectMaker[] tEdgeObjects,ref WizardObject WO){
			int sCount = tSplinatedObjects.Length;
			int eCount = tEdgeObjects.Length;
			//Splinated objects first:
			Splination.SplinatedMeshMaker SMM = null;
			GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			string tPath = xPath + "B/" + WO.FileName + ".gsd";
			if(WO.bIsDefault){
				tPath = xPath + "B/W/" + WO.FileName + ".gsd";	
			}
			StringBuilder builder = new StringBuilder(32768);
			
			//Wizard object:
			builder.Append(WO.ConvertToString());
			builder.Append(FileSepString);

			for(int i=0;i<sCount;i++){
				SMM = tSplinatedObjects[i];
				builder.Append(SMM.ConvertToString());
				builder.Append(FileSepString);
			}
			
			EdgeObjects.EdgeObjectMaker EOM = null;
			for(int i=0;i<eCount;i++){
				EOM = tEdgeObjects[i];
				builder.Append(EOM.ConvertToString());
				builder.Append(FileSepString);
			}
			
			#if UNITY_WEBPLAYER
			
			#else
				System.IO.File.WriteAllText(tPath,builder.ToString());
			#endif
		}
		
		public static void LoadNodeObjects(string tFileName, GSDSplineN tNode, bool bIsDefault = false, bool bIsBridge = false){
			#if UNITY_WEBPLAYER
			return;
            #else

			string tPath = "";
			GSDRootUtil.Dir_GetLibrary_CheckSpecialDirs();
			string xPath = GSDRootUtil.Dir_GetLibrary();
			if(bIsDefault){
				tPath = xPath + "B/W/" + tFileName + ".gsd";
			}else{
				tPath = xPath + "B/" + tFileName + ".gsd";
			}

	        string tData = System.IO.File.ReadAllText(tPath);
			string[] tSep = new string[2];
            tSep[0] = FileSepString;
            tSep[1] = FileSepStringCRLF;
			string[] tSplit = tData.Split(tSep,System.StringSplitOptions.RemoveEmptyEntries);

			Splination.SplinatedMeshMaker SMM = null;
			Splination.SplinatedMeshMaker.SplinatedMeshLibraryMaker SLM = null;
			EdgeObjects.EdgeObjectMaker EOM = null;
			EdgeObjects.EdgeObjectMaker.EdgeObjectLibraryMaker ELM = null;
			int tSplitCount = tSplit.Length;

			for(int i=0;i<tSplitCount;i++){
				SLM = null;
				SLM = Splination.SplinatedMeshMaker.SLMFromData(tSplit[i]);
				if(SLM != null){
					SMM = tNode.AddSplinatedObject();
					SMM.LoadFromLibraryBulk(ref SLM);
					SMM.bToggle = false;
					if(bIsBridge && tNode.bIsBridgeStart && tNode.bIsBridgeMatched && tNode.BridgeCounterpartNode != null){
						SMM.StartTime = tNode.tTime;
						SMM.EndTime = tNode.BridgeCounterpartNode.tTime;
						SMM.StartPos = tNode.GSDSpline.GetSplineValue(SMM.StartTime);
						SMM.EndPos = tNode.GSDSpline.GetSplineValue(SMM.EndTime);
					}
					continue;
				}
				
				ELM = null;
				ELM = EdgeObjects.EdgeObjectMaker.ELMFromData(tSplit[i]);
				if(ELM != null){
					EOM = tNode.AddEdgeObject();
					EOM.LoadFromLibraryBulk(ref ELM);
					EOM.bToggle = false;
					if(!EOM.bSingle && bIsBridge && tNode.bIsBridgeStart && tNode.bIsBridgeMatched && tNode.BridgeCounterpartNode != null){
						EOM.StartTime = tNode.tTime;
						EOM.EndTime = tNode.BridgeCounterpartNode.tTime;
						EOM.StartPos = tNode.GSDSpline.GetSplineValue(EOM.StartTime);
						EOM.EndPos = tNode.GSDSpline.GetSplineValue(EOM.EndTime);
					}else if(EOM.bSingle && bIsBridge && tNode.BridgeCounterpartNode != null && tNode.bIsBridgeStart){
						float tDist = (EOM.SingleOnlyBridgePercent * (tNode.BridgeCounterpartNode.tDist - tNode.tDist) + tNode.tDist); 
						EOM.SinglePosition = tNode.GSDSpline.TranslateDistBasedToParam(tDist);
						EOM.StartPos = tNode.GSDSpline.GetSplineValue(EOM.SinglePosition);
						EOM.EndPos = tNode.GSDSpline.GetSplineValue(EOM.SinglePosition);
					}
					continue;
				}
			}
			
			tNode.SetupSplinatedMeshes();
			tNode.SetupEdgeObjects();

            #endif
        }
    
		#region "Splat maps"
		public static byte[] MakeSplatMap(Terrain tTerrain, Color tBG, Color tFG, int tWidth, int tHeight, float SplatWidth, bool bSkipBridge, bool bSkipTunnel, string xRoadUID = ""){
			return MakeSplatMapDo(tTerrain,tBG,tFG,tWidth,tHeight,SplatWidth,bSkipBridge,bSkipTunnel,xRoadUID);
		}
		
		private static byte[] MakeSplatMapDo(Terrain tTerrain, Color tBG, Color tFG, int tWidth, int tHeight, float SplatWidth, bool bSkipBridge, bool bSkipTunnel, string xRoadUID){
			Texture2D tTexture = new Texture2D(tWidth,tHeight,TextureFormat.RGB24,false);
			
			//Set background color:
			Color[] tColorsBG = new Color[tWidth*tHeight];
			int tBGCount = tColorsBG.Length;
			for(int i=0;i<tBGCount;i++){
				tColorsBG[i] = tBG;	
			}
			tTexture.SetPixels(0,0,tWidth,tHeight,tColorsBG);
			tColorsBG = null;
			
			Object[] tRoads = null;
			if(xRoadUID != ""){
				tRoads = new Object[1];
				Object[] bRoads = GameObject.FindObjectsOfType(typeof(GSDRoad));
				foreach(GSDRoad fRoad in bRoads){
					if(string.CompareOrdinal(fRoad.UID,xRoadUID) == 0){
						tRoads[0] = fRoad;
						break;
					}
				}
			}else{
				tRoads = GameObject.FindObjectsOfType(typeof(GSDRoad));
			}
			Vector3 tPos = tTerrain.transform.position;
			Vector3 tSize = tTerrain.terrainData.size;
			foreach(GSDRoad tRoad in tRoads){
				GSDSplineC tSpline = tRoad.GSDSpline;
				int tCount = tSpline.RoadDefKeysArray.Length;	
				
				Vector3 POS1 = default(Vector3);
				Vector3 POS2 = default(Vector3);
				
				Vector3 tVect = default(Vector3);
				Vector3 tVect2 = default(Vector3);
				Vector3 lVect1 = default(Vector3);
				Vector3 lVect2 = default(Vector3);
				Vector3 rVect1 = default(Vector3);
				Vector3 rVect2 = default(Vector3);
				
				int x1,y1;
				int[] tX = new int[4];
				int[] tY = new int[4];
				int MinX = -1;
				int MaxX = -1;
				int MinY = -1;
				int MaxY = -1;
				int xDiff = -1;
				int yDiff = -1;
				float p1 = 0f;
				float p2 = 0f;
				bool bXBad = false;
				bool bYBad = false;
				for(int i=0;i<(tCount-1);i++){
					bXBad = false;
					bYBad = false;
					p1 = tSpline.TranslateInverseParamToFloat(tSpline.RoadDefKeysArray[i]);
					p2 = tSpline.TranslateInverseParamToFloat(tSpline.RoadDefKeysArray[i+1]);
					
					//Skip bridges:
					if(bSkipBridge){
						if(tSpline.IsInBridgeTerrain(p1)){
							continue;
						}
					}
					
					//Skip tunnels:
					if(bSkipTunnel){
						if(tSpline.IsInTunnelTerrain(p1)){
							continue;
						}
					}
					
					tSpline.GetSplineValue_Both(p1,out tVect, out POS1);
					tSpline.GetSplineValue_Both(p2,out tVect2, out POS2);
					lVect1 = (tVect + new Vector3(SplatWidth*-POS1.normalized.z,0,SplatWidth*POS1.normalized.x));
					rVect1 = (tVect + new Vector3(SplatWidth*POS1.normalized.z,0,SplatWidth*-POS1.normalized.x));
					lVect2 = (tVect2 + new Vector3(SplatWidth*-POS2.normalized.z,0,SplatWidth*POS2.normalized.x));
					rVect2 = (tVect2 + new Vector3(SplatWidth*POS2.normalized.z,0,SplatWidth*-POS2.normalized.x));
					
					TranslateWorldVectToCustom(tWidth,tHeight,lVect1,ref tPos,ref tSize,out x1, out y1);
					tX[0] = x1;
					tY[0] = y1;
					TranslateWorldVectToCustom(tWidth,tHeight,rVect1,ref tPos,ref tSize,out x1, out y1);
					tX[1] = x1;
					tY[1] = y1;
					TranslateWorldVectToCustom(tWidth,tHeight,lVect2,ref tPos,ref tSize,out x1, out y1);
					tX[2] = x1;
					tY[2] = y1;
					TranslateWorldVectToCustom(tWidth,tHeight,rVect2,ref tPos,ref tSize,out x1, out y1);
					tX[3] = x1;
					tY[3] = y1;
					
					MinX = Mathf.Min(tX);
					MaxX = Mathf.Max(tX);
					MinY = Mathf.Min(tY);
					MaxY = Mathf.Max(tY);
					
					
					if(MinX < 0){ MinX = 0; bXBad = true; }
					if(MaxX < 0){ MaxX = 0; bXBad = true; }
					if(MinY < 0){ MinY = 0; bYBad = true; }
					if(MaxY < 0){ MaxY = 0; bYBad = true; }
					
					if(MinX > (tWidth-1)){ MinX = (tWidth-1); bXBad = true; }
					if(MaxX > (tWidth-1)){ MaxX = (tWidth-1); bXBad = true; }
					if(MinY > (tHeight-1)){ MinY = (tHeight-1); bYBad = true; }
					if(MaxY > (tHeight-1)){ MaxY = (tHeight-1); bYBad = true; }
					
					if(bXBad && bYBad){ continue; }
					
					xDiff = MaxX-MinX;
					yDiff = MaxY-MinY;
					
					Color[] tColors = new Color[xDiff*yDiff];
					int cCount = tColors.Length;
					for(int j=0;j<cCount;j++){
						tColors[j] = tFG;
					}
					
					if(xDiff > 0 && yDiff > 0){
						tTexture.SetPixels(MinX,MinY,xDiff,yDiff,tColors);
					}
				}
			}
			
			tTexture.Apply();
			byte[] tBytes = tTexture.EncodeToPNG();
			Object.DestroyImmediate(tTexture);
			return tBytes;
		}
		
		
		private static void TranslateWorldVectToCustom(int tWidth, int tHeight, Vector3 tVect,ref Vector3 tPos,ref Vector3 tSize, out int x1, out int y1){
			//Get the normalized position of this game object relative to the terrain:
			tVect -= tPos;
	
			tVect.x = tVect.x / tSize.x;
			tVect.z = tVect.z / tSize.z;
					
			//Get the position of the terrain heightmap where this game object is:
			x1 = (int) (tVect.x * tWidth);
			y1 = (int) (tVect.z * tHeight);
		}
		#endregion
	
		#region "Wizard objects"
		public class WizardObject{
			public Texture2D Thumb;
			public string ThumbString;
			public string DisplayName;
			public string Desc;
			public bool bIsDefault;
			public bool bIsBridge;
			public string FileName;
			public string FullPath;
			public int sortID = 0;
			
			public string ConvertToString(){
				WizardObjectLibrary WOL = new WizardObjectLibrary();
				WOL.LoadFrom(this);
				return GSDRootUtil.GetString<WizardObjectLibrary>(WOL);
			}
			
			public void LoadDataFromWOL(WizardObjectLibrary WOL){
				ThumbString = WOL.ThumbString;
				DisplayName = WOL.DisplayName;
				Desc = WOL.Desc;
				bIsDefault = WOL.bIsDefault;
				FileName = WOL.FileName;
				bIsBridge = WOL.bIsBridge;
			}
	
			public static WizardObject LoadFromLibrary(string tPath){
				#if UNITY_WEBPLAYER
				return null;
                #else
				string tData = System.IO.File.ReadAllText(tPath);
				string[] tSep = new string[2];
                tSep[0] = FileSepString;
                tSep[1] = FileSepStringCRLF;
				string[] tSplit = tData.Split(tSep,System.StringSplitOptions.RemoveEmptyEntries);
				int tSplitCount = tSplit.Length;
				WizardObjectLibrary WOL = null;
				for(int i=0;i<tSplitCount;i++){
					WOL = WizardObject.WizardObjectLibrary.WOLFromData(tSplit[i]);
					if(WOL != null){
						WizardObject WO = new WizardObject();
						WO.LoadDataFromWOL(WOL);
						return WO;
					}
				}
				return null;
                #endif
            }

			[System.Serializable]
			public class WizardObjectLibrary{
				public string ThumbString;
				public string DisplayName;
				public string Desc;
				public bool bIsDefault;
				public bool bIsBridge;
				public string FileName;
				
				public void LoadFrom(WizardObject WO){
					ThumbString = WO.ThumbString;
					DisplayName = WO.DisplayName;
					Desc = WO.Desc;
					bIsDefault = WO.bIsDefault;
					FileName = WO.FileName;
					bIsBridge = WO.bIsBridge;
				}
				
				public static WizardObjectLibrary WOLFromData(string tData){
					try{
						WizardObjectLibrary WOL = (WizardObjectLibrary)GSDRootUtil.LoadData<WizardObjectLibrary>(ref tData);	
						return WOL;
					}catch{
						return null;	
					}
				}
			}
		}
		
		
		#endregion	
	}	

	public static class GSDIntersectionObjects{
		static bool IsApproximately(float a, float b){
	    	return IsApproximately(a, b, 0.01f);
	    }
	     
	    static bool IsApproximately(float a, float b, float tolerance){
	   		return Mathf.Abs(a - b) < tolerance;
	    }
		
		public static void CleanupIntersectionObjects(GameObject MasterGameObj){
			int mCount = MasterGameObj.transform.childCount;
			if(mCount == 0){ return; }
			List<GameObject> tObjtoDelete = new List<GameObject>();
			for(int i=0;i<mCount;i++){
				if(MasterGameObj.transform.GetChild(i).name.ToLower().Contains("stopsign")){
					tObjtoDelete.Add(MasterGameObj.transform.GetChild(i).gameObject);	
				}
				if(MasterGameObj.transform.GetChild(i).name.ToLower().Contains("trafficlight")){
					tObjtoDelete.Add(MasterGameObj.transform.GetChild(i).gameObject);	
				}
			}
			for(int i=(tObjtoDelete.Count-1);i>=0;i--){
				Object.DestroyImmediate(tObjtoDelete[i]);	
			}
		}

		#region "Stop Sign All Way"
		public static void CreateStopSignsAllWay(GameObject MasterGameObj, bool bIsRB = true){
			CreateStopSignsAllWay_Do(ref MasterGameObj, bIsRB);
		}
		private static void CreateStopSignsAllWay_Do(ref GameObject MasterGameObj, bool bIsRB){
			Object prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDSignStopAllway.prefab", typeof(GameObject));
	
			GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
			GSDSplineC tSpline = GSDRI.Node1.GSDSpline;
			
			GameObject tObj = null;
//			Vector3 xDir = default(Vector3);
			Vector3 tDir = default(Vector3);
//			float RoadWidth = tSpline.tRoad.RoadWidth();
//			float LaneWidth = tSpline.tRoad.opt_LaneWidth;
			float ShoulderWidth = tSpline.tRoad.opt_ShoulderWidth;
			
			//Cleanup:
			CleanupIntersectionObjects(MasterGameObj);
			
			float Mass = 100f;
			
			//Get four points:
			float DistFromCorner = (ShoulderWidth*0.45f);
			Vector3 tPosRR = default(Vector3);
			Vector3 tPosRL = default(Vector3);
			Vector3 tPosLR = default(Vector3);
			Vector3 tPosLL = default(Vector3);
			GetFourPoints(GSDRI, out tPosRR,out tPosRL,out tPosLL,out tPosLR,DistFromCorner);
			
			//RR:
			tSpline = GSDRI.Node1.GSDSpline;
			tObj = Object.Instantiate(prefab,Vector3.zero,Quaternion.identity) as GameObject;
//			xDir = (GSDRI.CornerRR - GSDRI.transform.position).normalized;
			tDir = StopSign_GetRot_RR(GSDRI,tSpline);
			tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f,180f,0f);
			if(bIsRB){
				Rigidbody RB = tObj.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = new Vector3(0f,-10f,0f);
			}
			tObj.transform.parent = MasterGameObj.transform;
			tObj.transform.position = tPosRR;
			tObj.name = "StopSignRR";
			if(GSDRI.IgnoreCorner == 0){ Object.DestroyImmediate(tObj); }
			
			//LL:
			tSpline = GSDRI.Node1.GSDSpline;
			tObj = Object.Instantiate(prefab,Vector3.zero,Quaternion.identity) as GameObject;
//			xDir = (GSDRI.CornerLL - GSDRI.transform.position).normalized;
			tDir = StopSign_GetRot_LL(GSDRI,tSpline);
			tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f,180f,0f);
			if(bIsRB){
				Rigidbody RB = tObj.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = new Vector3(0f,-10f,0f);
			}
			tObj.transform.parent = MasterGameObj.transform;
			tObj.transform.position = tPosLL;
			tObj.name = "StopSignLL";
			if(GSDRI.IgnoreCorner == 2){ Object.DestroyImmediate(tObj); }
			
			//RL:
			tSpline = GSDRI.Node2.GSDSpline;
			tObj = Object.Instantiate(prefab,Vector3.zero,Quaternion.identity) as GameObject;
//			xDir = (GSDRI.CornerRL - GSDRI.transform.position).normalized;
			tDir = StopSign_GetRot_RL(GSDRI,tSpline);
			tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f,180f,0f);
			if(bIsRB){
				Rigidbody RB = tObj.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = new Vector3(0f,-10f,0f);
			}
			tObj.transform.parent = MasterGameObj.transform;
			tObj.transform.position = tPosRL;
			tObj.name = "StopSignRL";
			if(GSDRI.IgnoreCorner == 1){ Object.DestroyImmediate(tObj); }
			
			//LR:
			tSpline = GSDRI.Node2.GSDSpline;
			tObj = Object.Instantiate(prefab,Vector3.zero,Quaternion.identity) as GameObject;
//			xDir = (GSDRI.CornerLR - GSDRI.transform.position).normalized;
			tDir = StopSign_GetRot_LR(GSDRI,tSpline);
			tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f,180f,0f);
			if(bIsRB){
				Rigidbody RB = tObj.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = new Vector3(0f,-10f,0f);
			}
			tObj.transform.parent = MasterGameObj.transform;
			tObj.transform.position = tPosLR;
			tObj.name = "StopSignLR";
			if(GSDRI.IgnoreCorner == 3){ Object.DestroyImmediate(tObj); }
		}
		
		private static Vector3 StopSign_GetRot_RR(GSDRoadIntersection GSDRI, GSDSplineC tSpline){
			float tDist = ((Vector3.Distance(GSDRI.CornerRL,GSDRI.CornerRR) / 2f) + (0.025f*Vector3.Distance(GSDRI.CornerLL,GSDRI.CornerRR))) / tSpline.distance;;
			float p = Mathf.Clamp(GSDRI.Node1.tTime-tDist,0f,1f);
			Vector3 POS = tSpline.GetSplineValue(p,true);
			return (POS*-1);
		}
		
		private static Vector3 StopSign_GetRot_LL(GSDRoadIntersection GSDRI, GSDSplineC tSpline){
			float tDist = ((Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerLL) / 2f) + (0.025f*Vector3.Distance(GSDRI.CornerLL,GSDRI.CornerRR))) / tSpline.distance;;
			float p = Mathf.Clamp(GSDRI.Node1.tTime+tDist,0f,1f);
			Vector3 POS = tSpline.GetSplineValue(p,true);
			return POS;
		}
		
		private static Vector3 StopSign_GetRot_RL(GSDRoadIntersection GSDRI, GSDSplineC tSpline){
			float tDist = ((Vector3.Distance(GSDRI.CornerLL,GSDRI.CornerRL) / 2f) + (0.025f*Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerRL))) / tSpline.distance;;
			float p = -1f;
			if(GSDRI.bFlipped){
				p = Mathf.Clamp(GSDRI.Node2.tTime-tDist,0f,1f);
			}else{
				p = Mathf.Clamp(GSDRI.Node2.tTime+tDist,0f,1f);
			}
			Vector3 POS = tSpline.GetSplineValue(p,true);
			//POS = Vector3.Cross(POS,Vector3.up);
			if(GSDRI.bFlipped){
				return (POS*-1);
			}else{
				return POS;
			}
		}
		
		private static Vector3 StopSign_GetRot_LR(GSDRoadIntersection GSDRI, GSDSplineC tSpline){
			float tDist = ((Vector3.Distance(GSDRI.CornerRR,GSDRI.CornerLR) / 2f) + (0.025f*Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerRL))) / tSpline.distance;;
			float p = -1f;
			if(GSDRI.bFlipped){
				p = Mathf.Clamp(GSDRI.Node2.tTime+tDist,0f,1f);
			}else{
				p = Mathf.Clamp(GSDRI.Node2.tTime-tDist,0f,1f);
			}
			Vector3 POS = tSpline.GetSplineValue(p,true);
			//POS = Vector3.Cross(POS,Vector3.up);
			if(GSDRI.bFlipped){
				return POS;
			}else{
				return (POS*-1);
			}
		}
		#endregion
		
		#region "Traffic light bases"
		public static void CreateTrafficLightBases(GameObject MasterGameObj,bool bIsTrafficLight1 = true){
			CreateTrafficLightBases_Do(ref MasterGameObj, bIsTrafficLight1);
		}
		private static void CreateTrafficLightBases_Do(ref GameObject MasterGameObj,bool bIsTrafficLight1){
			GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
			GSDSplineC tSpline = GSDRI.Node1.GSDSpline;
			bool bIsRB = true;
			
//			float RoadWidth = tSpline.tRoad.RoadWidth();
			float LaneWidth = tSpline.tRoad.opt_LaneWidth;
			float ShoulderWidth = tSpline.tRoad.opt_ShoulderWidth;
			
			int Lanes = tSpline.tRoad.opt_Lanes;
			int LanesHalf = Lanes / 2;
			float LanesForInter = -1;
			if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				LanesForInter = LanesHalf + 1f;
			}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				LanesForInter = LanesHalf + 1f;
			}else{
				LanesForInter = LanesHalf - 1 + 1f;
			}
			float DistFromCorner = (ShoulderWidth*0.45f);
			float TLDistance = (LanesForInter * LaneWidth) + DistFromCorner;

			GameObject tObjRR = null;
			GameObject tObjRL = null;
			GameObject tObjLL = null;
			GameObject tObjLR = null;
//			Vector3 xDir = default(Vector3);
			Vector3 tDir = default(Vector3);
			float Mass = 12500f;
			Vector3 COM = new Vector3(0f,0f,4f);
            Vector3 zeroVect = new Vector3(0f,0f,0f);
			Vector3 StartVec = default(Vector3);
			Vector3 EndVec = default(Vector3);
//			bool bContains = false;
//			MeshFilter MF = null;
//			Vector3[] tVerts = null;
			Rigidbody RB = null;
			
			//Get four points:
			Vector3 tPosRR = default(Vector3);
			Vector3 tPosRL = default(Vector3);
			Vector3 tPosLR = default(Vector3);
			Vector3 tPosLL = default(Vector3);
			GetFourPoints(GSDRI, out tPosRR,out tPosRL,out tPosLL,out tPosLR,DistFromCorner);

			//Cleanup:
			CleanupIntersectionObjects(MasterGameObj);
			
			float[] tempDistances = new float[4];
			tempDistances[0] = Vector3.Distance(GSDRI.CornerRL,GSDRI.CornerLL);
			tempDistances[1] = Vector3.Distance(GSDRI.CornerRL,GSDRI.CornerRR);
			tempDistances[2] = Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerLL);
			tempDistances[3] = Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerRR);
			float MaxDistanceStart = Mathf.Max(tempDistances);
			bool OrigPoleAlignment = GSDRI.bRegularPoleAlignment;

			//Node1:
			//RL:
			tObjRL = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//			xDir = (GSDRI.CornerRL - GSDRI.transform.position).normalized;
			tDir = TrafficLightBase_GetRot_RL(GSDRI,tSpline,DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
			tObjRL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
			tObjRL.transform.parent = MasterGameObj.transform;
			StartVec = tPosRL;
			EndVec = (tDir.normalized*TLDistance) + StartVec;
			if(!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec,EndVec)){ //Convert to regular alignment if necessary
				tObjRL.transform.parent = null;
				tDir = TrafficLightBase_GetRot_RL(GSDRI,tSpline,DistFromCorner,true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjRL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
				tObjRL.transform.parent = MasterGameObj.transform;	
			}else{
				GSDRI.bRegularPoleAlignment = true;
				if(tObjRL != null){ Object.DestroyImmediate(tObjRL); }
				tObjRL = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//				xDir = (GSDRI.CornerRL - GSDRI.transform.position).normalized;
				tDir = TrafficLightBase_GetRot_RL(GSDRI,tSpline,DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjRL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
				tObjRL.transform.parent = MasterGameObj.transform;
				StartVec = tPosRL;
				EndVec = (tDir.normalized*TLDistance) + StartVec;
				GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
			}	
			if(bIsRB){
				RB = tObjRL.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = COM;
				tObjRL.AddComponent<GSDRigidBody>();
			}
			tObjRL.transform.position = tPosRL;
			tObjRL.transform.name = "TrafficLightRL";
			//LR:
			tObjLR = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//			xDir = (GSDRI.CornerLR - GSDRI.transform.position).normalized;
			tDir = TrafficLightBase_GetRot_LR(GSDRI,tSpline,DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
			tObjLR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
			tObjLR.transform.parent = MasterGameObj.transform;
			StartVec = tPosLR;
			EndVec = (tDir.normalized*TLDistance) + StartVec;
			if(!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec,EndVec)){ //Convert to regular alignment if necessary
				tObjLR.transform.parent = null;
				tDir = TrafficLightBase_GetRot_LR(GSDRI,tSpline,DistFromCorner,true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjLR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
				tObjLR.transform.parent = MasterGameObj.transform;	
			}else{
				GSDRI.bRegularPoleAlignment = true;
				if(tObjLR != null){ Object.DestroyImmediate(tObjLR); }
				tObjLR = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//				xDir = (GSDRI.CornerLR - GSDRI.transform.position).normalized;
				tDir = TrafficLightBase_GetRot_LR(GSDRI,tSpline,DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjLR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
				tObjLR.transform.parent = MasterGameObj.transform;
				StartVec = tPosLR;
				EndVec = (tDir.normalized*TLDistance) + StartVec;	
				GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
			}
			if(bIsRB){
				RB = tObjLR.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = COM;
				tObjLR.AddComponent<GSDRigidBody>();
			}
			tObjLR.transform.position = tPosLR;
			tObjLR.transform.name = "TrafficLightLR";
			//Node2:
			//RR:
			tObjRR = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//			xDir = (GSDRI.CornerRR - GSDRI.transform.position).normalized;
			tDir = TrafficLightBase_GetRot_RR(GSDRI,tSpline,DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
			tObjRR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
			tObjRR.transform.parent = MasterGameObj.transform;
			StartVec = tPosRR;
			EndVec = (tDir.normalized*TLDistance) + StartVec;
			if(!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec,EndVec)){ //Convert to regular alignment if necessary
				tObjRR.transform.parent = null;
				tDir = TrafficLightBase_GetRot_RR(GSDRI,tSpline,DistFromCorner,true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjRR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,0f,0f);
				tObjRR.transform.parent = MasterGameObj.transform;	
			}else{
				GSDRI.bRegularPoleAlignment = true;
				if(tObjRR != null){ Object.DestroyImmediate(tObjRR); }
				tObjRR = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//				xDir = (GSDRI.CornerRR - GSDRI.transform.position).normalized;
				tDir = TrafficLightBase_GetRot_RR(GSDRI,tSpline,DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjRR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
				tObjRR.transform.parent = MasterGameObj.transform;
				StartVec = tPosRR;
				EndVec = (tDir.normalized*TLDistance) + StartVec;
				GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
			}
			if(bIsRB){
				RB = tObjRR.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = COM;
				tObjRR.AddComponent<GSDRigidBody>();
			}
			tObjRR.transform.position = tPosRR;
			tObjRR.transform.name = "TrafficLightRR";
			
			//LL:
			tObjLL = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//			xDir = (GSDRI.CornerLL - GSDRI.transform.position).normalized;
			tDir = TrafficLightBase_GetRot_LL(GSDRI,tSpline,DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
			tObjLL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
			tObjLL.transform.parent = MasterGameObj.transform;
			StartVec = tPosLL;
			EndVec = (tDir.normalized*TLDistance) + StartVec;
			if(!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec,EndVec)){ //Convert to regular alignment if necessary
				tObjLL.transform.parent = null;
				tDir = TrafficLightBase_GetRot_LL(GSDRI,tSpline,DistFromCorner,true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjLL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,0f,0f);
				tObjLL.transform.parent = MasterGameObj.transform;	
			}else{
				GSDRI.bRegularPoleAlignment = true;
				if(tObjLL != null){ Object.DestroyImmediate(tObjLL); }
				tObjLL = CreateTrafficLight(TLDistance,true,true,MaxDistanceStart,GSDRI.bTrafficPoleStreetLight,tSpline.tRoad.GSDRS.opt_bSaveMeshes);
//				xDir = (GSDRI.CornerLL - GSDRI.transform.position).normalized;
				tDir = TrafficLightBase_GetRot_LL(GSDRI,tSpline,DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
				tObjLL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f,-180f,0f);
				tObjLL.transform.parent = MasterGameObj.transform;
				StartVec = tPosLL;
				EndVec = (tDir.normalized*TLDistance) + StartVec;	
				GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
			}
			if(bIsRB){
				RB = tObjLL.AddComponent<Rigidbody>();
				RB.mass = Mass;
				RB.centerOfMass = COM;
				tObjLL.AddComponent<GSDRigidBody>();
			}
			tObjLL.transform.position = tPosLL;
			tObjLL.transform.name = "TrafficLightLL";
			
			//Create the actual lights:
			CreateTrafficLightMains(MasterGameObj,tObjRR,tObjRL,tObjLL,tObjLR);
		}
		
		private static bool CreateTrafficLightBase_IsInIntersection(GSDRoadIntersection GSDRI,ref Vector3 StartVec,ref Vector3 EndVec){
			return GSDRI.ContainsLine(StartVec,EndVec);
		}
		
		private static GameObject CreateTrafficLight(float tDistance, bool bIsTrafficLight1, bool bOptionalCollider, float xDistance, bool bLight, bool bSaveAsset){
			GameObject tObj = null;
			string tTrafficLightNumber = "1";
			if(!bIsTrafficLight1){
				tTrafficLightNumber = "2";	
			}
			
			bool bDoCustom = false;
			xDistance = Mathf.Ceil(xDistance);	//Round up to nearest whole F
			tDistance = Mathf.Ceil(tDistance); 	//Round up to nearest whole F
//			string assetName = "GSDInterTLB" + tTrafficLightNumber + "_" + tDistance.ToString("F0") + "_" + xDistance.ToString("F0") + ".prefab";
			string assetNameAsset = "GSDInterTLB" + tTrafficLightNumber + "_" + tDistance.ToString("F0") + "_" + xDistance.ToString("F0") + ".asset";
			string BackupFBX = "GSDInterTLB" + tTrafficLightNumber + ".FBX";
			float tMod = tDistance / 5f;
			float hMod = (tDistance / 10f) * 0.7f;
			float xMod = ((xDistance / 20f) + 2f) * 0.3334f;
			xMod = Mathf.Clamp(xMod,1f,20f);
			tMod = Mathf.Clamp(tMod,1f,20f);
			hMod = Mathf.Clamp(hMod,1f,20f);
			
			bool bXMod = false;
			if(!IsApproximately(xMod,1f,0.0001f)){ bXMod = true; }
			
			Mesh xMesh = (Mesh)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/TrafficLightBases/" + assetNameAsset, typeof(Mesh));
			if(xMesh == null){
				xMesh = (Mesh)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/TrafficLightBases/" + BackupFBX, typeof(Mesh));
				bDoCustom = true;
			}
			
			tObj = new GameObject("TempTrafficLight");
			MeshFilter MF = tObj.GetComponent<MeshFilter>(); if(MF == null){ MF = tObj.AddComponent<MeshFilter>(); }
			MeshRenderer MR = tObj.GetComponent<MeshRenderer>(); if(MR == null){ MR = tObj.AddComponent<MeshRenderer>(); }
			GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/Signs/GSDInterTLB" + tTrafficLightNumber + ".mat",MR);
			
			if(!bDoCustom){
				MF.sharedMesh = xMesh;	
			}
			
			float tempMaxHeight = 7.6f * hMod;
			float xValue = tempMaxHeight - 7.6f;
			if(bDoCustom){
				Mesh tMesh = new Mesh();
				tMesh.vertices = xMesh.vertices;
				tMesh.triangles = xMesh.triangles;
				tMesh.uv = xMesh.uv;
				tMesh.normals = xMesh.normals;
				tMesh.tangents = xMesh.tangents;
				MF.sharedMesh = tMesh;
				Vector3[] tVerts = tMesh.vertices;
				
				xValue = (xMod * 6f) - 6f;
				if((xMod * 6f) > (tempMaxHeight-1f)){
					xValue = (tempMaxHeight-1f)-6f;
				}
				
//				float tValue = 0f;
//				float hValue = 0f;
				bool bIgnoreMe = false;
				
				
				int mCount = tVerts.Length;
				Vector2[] uv = tMesh.uv;
//				List<int> tUVInts = new List<int>();
				for(int i=0;i<mCount;i++){
					bIgnoreMe= false;
					if(IsApproximately(tVerts[i].y,5f,0.01f)){
						tVerts[i].y = tDistance;
						if(uv[i].y > 3.5f){
							uv[i].y *= tMod;
						}
						bIgnoreMe = true;
					}
					if(!bIgnoreMe && tVerts[i].z > 7.5f){
						tVerts[i].z *= hMod;
						if(uv[i].y > 3.8f){
							uv[i].y *= hMod;
						}
					}
					
					if(bXMod && tVerts[i].z > 4.8f && tVerts[i].z < 6.2f){
						tVerts[i].z += xValue;
					}
				}
				tMesh.vertices = tVerts;
				tMesh.uv = uv;
				tMesh.RecalculateNormals();
				tMesh.RecalculateBounds();

				//Save:
				if(bSaveAsset){
					UnityEditor.AssetDatabase.CreateAsset(tMesh, "Assets/RoadArchitect/Mesh/RoadObj/Signs/TrafficLightBases/" + assetNameAsset);
				}
			}
			
			BoxCollider BC = tObj.AddComponent<BoxCollider>();
			float MaxHeight = MF.sharedMesh.vertices[447].z;
			BC.size = new Vector3(0.35f,0.35f,MaxHeight);
			BC.center = new Vector3(0f,0f,(MaxHeight/2f));
			
			if(bOptionalCollider){
				float MaxWidth = MF.sharedMesh.vertices[497].y;
				GameObject tObjCollider = new GameObject("col2");
				BC = tObjCollider.AddComponent<BoxCollider>();
				BC.size = new Vector3(0.175f,MaxWidth,0.175f);
				BC.center = new Vector3(0f,MaxWidth/2f,5.808f);
				tObjCollider.transform.parent = tObj.transform;
			}
			
			if(bLight){
				GameObject yObj = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDStreetLight_TrafficLight.prefab", typeof(GameObject));
				GameObject kObj = (GameObject)GameObject.Instantiate(yObj);
				kObj.transform.position = tObj.transform.position;
				kObj.transform.position += new Vector3(0f,0f,MaxHeight-7.6f);
				kObj.transform.parent = tObj.transform;
				kObj.transform.rotation = Quaternion.identity;
//				kObj.name = "StreetLight";
			}
			
			
			//Bounds calcs:
			MeshFilter[] tMeshes = tObj.GetComponents<MeshFilter>();	
			for(int i=0;i<tMeshes.Length;i++){ tMeshes[i].sharedMesh.RecalculateBounds(); }

			return tObj;
		}
	
		private static Vector3 TrafficLightBase_GetRot_RL(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false){
			Vector3 POS = default(Vector3);
			if(!GSDRI.bRegularPoleAlignment && !bOverrideRegular){
//				float tDist = ((Vector3.Distance(GSDRI.CornerRR,GSDRI.CornerRL) / 2f) + DistFromCorner) / tSpline.distance;;
				float p = Mathf.Clamp(GSDRI.Node1.tTime,0f,1f);
				POS = tSpline.GetSplineValue(p,true);
				POS = Vector3.Cross(POS,Vector3.up);
				return POS;
			}else{
				POS = GSDRI.CornerRL - GSDRI.CornerLL;
				return POS * -1;	
			}
		}
		
		private static Vector3 TrafficLightBase_GetRot_LR(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false){
			Vector3 POS = default(Vector3);
			if(!GSDRI.bRegularPoleAlignment && !bOverrideRegular){
//				float tDist = ((Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerLL) / 2f) + DistFromCorner) / tSpline.distance;;
				float p = Mathf.Clamp(GSDRI.Node1.tTime,0f,1f);
				POS = tSpline.GetSplineValue(p,true);
				POS = Vector3.Cross(POS,Vector3.up);
				return POS*-1;
			}else{
				POS = GSDRI.CornerRR - GSDRI.CornerLR;
				return POS;	
			}
		}
		
		private static Vector3 TrafficLightBase_GetRot_RR(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false){
			Vector3 POS = default(Vector3);
			if(!GSDRI.bRegularPoleAlignment && !bOverrideRegular){
//				float tDist = ((Vector3.Distance(GSDRI.CornerRR,GSDRI.CornerLR) / 2f) + DistFromCorner) / tSpline.distance;;
				float p = Mathf.Clamp(GSDRI.Node2.tTime,0f,1f);
				POS = tSpline.GetSplineValue(p,true);
				POS = Vector3.Cross(POS,Vector3.up); if(GSDRI.bFlipped){ POS = POS*-1; }
			}else{
				POS = GSDRI.CornerLL - GSDRI.CornerLR;
			}
			return POS;	
		}
		
		private static Vector3 TrafficLightBase_GetRot_LL(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false){
			Vector3 POS = default(Vector3);
			if(!GSDRI.bRegularPoleAlignment && !bOverrideRegular){
//				float tDist = ((Vector3.Distance(GSDRI.CornerLL,GSDRI.CornerRL) / 2f) + DistFromCorner) / tSpline.distance;;
				float p = Mathf.Clamp(GSDRI.Node2.tTime,0f,1f);
				POS = tSpline.GetSplineValue(p,true);
				POS = Vector3.Cross(POS,Vector3.up); if(GSDRI.bFlipped){ POS = POS*-1; }
			}else{
				POS = GSDRI.CornerRL - GSDRI.CornerRR;
			}
			return POS * -1;
		}
		#endregion
	
		#region "Traffic light mains"
		private static void CreateTrafficLightMains(GameObject MasterGameObj, GameObject tRR, GameObject tRL, GameObject tLL, GameObject tLR){
			GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
			GSDSplineC tSpline = GSDRI.Node1.GSDSpline;
			
			float tDist = (Vector3.Distance(GSDRI.CornerRL,GSDRI.CornerRR) / 2f) / tSpline.distance;
			Vector3 tan = tSpline.GetSplineValue(GSDRI.Node1.tTime + tDist,true);
			ProcessPole(MasterGameObj,tRL,tan * -1,1, Vector3.Distance(GSDRI.CornerRL,GSDRI.CornerRR));
			tDist = (Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerLL) / 2f) / tSpline.distance;
			tan = tSpline.GetSplineValue(GSDRI.Node1.tTime - tDist,true);
			ProcessPole(MasterGameObj,tLR,tan,3,Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerLL));
			
			
			float InterDist = Vector3.Distance(GSDRI.CornerRL,GSDRI.CornerLL);
			tDist = (InterDist / 2f) / tSpline.distance;
			tan = tSpline.GetSplineValue(GSDRI.Node1.tTime + tDist,true);
			
			float fTime1 = GSDRI.Node2.tTime + tDist;
			float fTime2neg = GSDRI.Node2.tTime - tDist;
			
			tSpline = GSDRI.Node2.GSDSpline;
			if(GSDRI.bFlipped){
				tan = tSpline.GetSplineValue(fTime1,true);
				ProcessPole(MasterGameObj,tRR,tan,0,InterDist);
				tan = tSpline.GetSplineValue(fTime2neg,true);
				ProcessPole(MasterGameObj,tLL,tan * -1,2,InterDist);
			}else{
				tan = tSpline.GetSplineValue(fTime2neg,true);
				ProcessPole(MasterGameObj,tRR,tan * -1,0,InterDist);
				tan = tSpline.GetSplineValue(fTime1,true);
				ProcessPole(MasterGameObj,tLL,tan,2,InterDist);
			}
			
			if(GSDRI.IgnoreCorner == 0){
				if(tRR != null){ Object.DestroyImmediate(tRR); }
			}else if(GSDRI.IgnoreCorner == 1){
				if(tRL != null){ Object.DestroyImmediate(tRL); }
			}else if(GSDRI.IgnoreCorner == 2){
				if(tLL != null){ Object.DestroyImmediate(tLL); }
			}else if(GSDRI.IgnoreCorner == 3){
				if(tLR != null){ Object.DestroyImmediate(tLR); }
			}
		}
		
		private static void ProcessPole(GameObject MasterGameObj, GameObject tObj, Vector3 tan, int tCorner, float InterDist){
			GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
			GSDSplineC tSpline = GSDRI.Node1.GSDSpline;
//			bool bIsRB = true;
		
//			float RoadWidth = tSpline.tRoad.RoadWidth();
			float LaneWidth = tSpline.tRoad.opt_LaneWidth;
			float ShoulderWidth = tSpline.tRoad.opt_ShoulderWidth;
			
			int Lanes = tSpline.tRoad.opt_Lanes;
			int LanesHalf = Lanes / 2;
			float LanesForInter = -1;
			if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				LanesForInter = LanesHalf + 1f;
			}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				LanesForInter = LanesHalf + 1f;
			}else{
				LanesForInter = LanesHalf;
			}
			float DistFromCorner = (ShoulderWidth*0.35f);
			float TLDistance = (LanesForInter * LaneWidth) + DistFromCorner;
			
			MeshFilter MF = tObj.GetComponent<MeshFilter>();
			Mesh tMesh = MF.sharedMesh;
			Vector3[] tVerts = tMesh.vertices;
			Vector3 StartVec = tVerts[520];
			Vector3 EndVec = tVerts[521];
			StartVec = (((EndVec-StartVec) * (DistFromCorner/TLDistance)) + StartVec);
			Vector3 tempR_Start = tVerts[399];
			Vector3 tempR_End = tVerts[396];
			Vector3 tLanePosR = ((tempR_End - tempR_Start)*0.95f) + tempR_Start;
			tLanePosR.z -= 1f;
			
			float SmallerDist = Vector3.Distance(StartVec,EndVec);
			
			//StartVec = Corner
			//2.5m = lane
			//7.5m = lane
			//12.5m = lane
			Vector3[] tLanePos = new Vector3[LanesHalf];
			for(int i=0;i<LanesHalf;i++){
				tLanePos[i] = (((EndVec-StartVec) * (((LaneWidth*0.5f)+(i*LaneWidth))/SmallerDist)) + StartVec);
			}
			Vector3 tLanePosL = default(Vector3);
			Vector3 tLanePosL_Sign = default(Vector3);
			
			if(GSDRI.bLeftTurnYieldOnGreen){
				tLanePosL = ((EndVec-StartVec) * ((SmallerDist-1.8f)/SmallerDist)) + StartVec;
				tLanePosL_Sign = ((EndVec-StartVec) * ((SmallerDist-3.2f)/SmallerDist)) + StartVec;
			}else{
				tLanePosL = ((EndVec-StartVec) * ((SmallerDist-2.5f)/SmallerDist)) + StartVec;
			}
			
			Vector3 tPos1 = default(Vector3);
			if(tCorner == 0){ //RR
				tPos1 = GSDRI.CornerLR;
			}else if(tCorner == 1){ //RL
				tPos1 = GSDRI.CornerRR;
			}else if(tCorner == 2){ //LL
				tPos1 = GSDRI.CornerRL;
			}else if(tCorner == 3){ //LR
				tPos1 = GSDRI.CornerLL;
			}
			
			int mCount = tLanePos.Length;
			float mDistance = -50000f;
			float tDistance = 0f;
			for(int i=0;i<mCount;i++){
				tDistance = Vector3.Distance(tObj.transform.TransformPoint(tLanePos[i]),tPos1);
				if(tDistance > mDistance){ mDistance = tDistance; }
			}
			tDistance = Vector3.Distance(tObj.transform.TransformPoint(tLanePosL),tPos1);
			if(tDistance > mDistance){ mDistance = tDistance; }
			tDistance = Vector3.Distance(tObj.transform.TransformPoint(tLanePosR),tPos1);
			if(tDistance > mDistance){ mDistance = tDistance; }
			
			float tScaleSense = ((200f-GSDRI.ScalingSense) / 200f) * 200f;
			tScaleSense = Mathf.Clamp(tScaleSense * 0.1f,0f,20f);
			float ScaleMod = ((mDistance/17f) + tScaleSense) * (1f/(tScaleSense+1f));
			if(IsApproximately(tScaleSense,20f,0.05f)){ ScaleMod = 1f; }
			ScaleMod = Mathf.Clamp(ScaleMod,1f,1.5f);
			Vector3 tScale = new Vector3(ScaleMod,ScaleMod,ScaleMod);
			bool bScale = true; if(IsApproximately(ScaleMod,1f,0.001f)){ bScale = false; }
			
			//Debug.Log (mDistance + " " + ScaleMod + " " + tScaleSense);
			
			GameObject tRight = null;
			GameObject tLeft = null;
			GameObject tLeft_Sign = null;
			Object prefab = null;
			
			MeshRenderer MR_Left = null;
			MeshRenderer MR_Right = null;
			MeshRenderer[] MR_Mains = new MeshRenderer[LanesHalf];
			int cCount = -1;
			if(GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
				if(GSDRI.bLeftTurnYieldOnGreen){
					prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightLeftYield.prefab", typeof(GameObject));
				}else{
					prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightLeft.prefab", typeof(GameObject));
				}
				tLeft = (GameObject)GameObject.Instantiate(prefab,Vector3.zero,Quaternion.identity);
				tLeft.transform.position = tObj.transform.TransformPoint(tLanePosL);
				tLeft.transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(0f,90f,0f);
				tLeft.transform.parent = tObj.transform;
				tLeft.transform.name = "LightLeft";
				
				cCount = tLeft.transform.childCount;
				for(int i=0;i<cCount;i++){
					if(tLeft.transform.GetChild(i).name.ToLower() == "lights"){
						MR_Left = tLeft.transform.GetChild(i).GetComponent<MeshRenderer>();
					}
				}
				
				if(bScale){ tLeft.transform.localScale = tScale; }
				
				if(GSDRI.bLeftTurnYieldOnGreen){
					prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDSignYieldOnGreen.prefab", typeof(GameObject));
					tLeft_Sign = (GameObject)GameObject.Instantiate(prefab,Vector3.zero,Quaternion.identity);
					tLeft_Sign.transform.position = tObj.transform.TransformPoint(tLanePosL_Sign);
					tLeft_Sign.transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(-90f,90f,0f);
					tLeft_Sign.transform.parent = tObj.transform;
					tLeft_Sign.transform.name = "SignYieldOnGreen";
					if(bScale){ tLeft_Sign.transform.localScale = tScale; }
				}
			}
			if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightRight.prefab", typeof(GameObject));
				tRight = (GameObject)GameObject.Instantiate(prefab,Vector3.zero,Quaternion.identity);
				tRight.transform.position = tObj.transform.TransformPoint(tLanePosR);
				tRight.transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(0f,90f,0f);
				tRight.transform.parent = tObj.transform;
				tRight.transform.name = "LightRight";
				if(bScale){ tRight.transform.localScale = tScale; }
				
				cCount = tRight.transform.childCount;
				for(int i=0;i<cCount;i++){
					if(tRight.transform.GetChild(i).name.ToLower() == "lights"){
						MR_Right = tRight.transform.GetChild(i).GetComponent<MeshRenderer>();
					}
				}
			}
			GameObject[] tLanes = new GameObject[LanesHalf];
			for(int i=0;i<LanesHalf;i++){
				prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightMain.prefab", typeof(GameObject));
				tLanes[i] = (GameObject)GameObject.Instantiate(prefab,Vector3.zero,Quaternion.identity);
				tLanes[i].transform.position = tObj.transform.TransformPoint(tLanePos[i]);
				tLanes[i].transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(0f,90f,0f);
				tLanes[i].transform.parent = tObj.transform;
				tLanes[i].transform.name = "Light" + i.ToString();
				if(bScale){ tLanes[i].transform.localScale = tScale; }
				
				cCount = tLanes[i].transform.childCount;
				for(int j=0;j<cCount;j++){
					if(tLanes[i].transform.GetChild(j).name.ToLower() == "lights"){
						MR_Mains[i] = tLanes[i].transform.GetChild(j).GetComponent<MeshRenderer>();
					}
				}
			}

			GSDTrafficLightController LM = new GSDTrafficLightController(ref tLeft,ref tRight,ref tLanes, ref MR_Left, ref MR_Right, ref MR_Mains);
			if(tCorner == 0){
				GSDRI.LightsRR = null;
				GSDRI.LightsRR = LM;
			}else if(tCorner == 1){
				GSDRI.LightsRL = null;
				GSDRI.LightsRL = LM;
			}else if(tCorner == 2){
				GSDRI.LightsLL = null;
				GSDRI.LightsLL = LM;
			}else if(tCorner == 3){
				GSDRI.LightsLR = null;
				GSDRI.LightsLR = LM;
			}
		}
		#endregion
		
		public static void GetFourPoints(GSDRoadIntersection GSDRI, out Vector3 tPosRR, out Vector3 tPosRL, out Vector3 tPosLL, out Vector3 tPosLR, float DistFromCorner){
			GetFourPoints_Do(ref GSDRI,out tPosRR,out tPosRL,out tPosLL,out tPosLR,DistFromCorner);
		}
		private static void GetFourPoints_Do(ref GSDRoadIntersection GSDRI, out Vector3 tPosRR, out Vector3 tPosRL, out Vector3 tPosLL, out Vector3 tPosLR, float DistFromCorner){
			//Get four points:
			float tPos1 = 0f;
			float tPos2 = 0f;
			Vector3 tTan1 = default(Vector3);
			Vector3 tTan2 = default(Vector3);
			float Node1Width = -1f;
			float Node2Width = -1f;
			Vector3 tVectRR = GSDRI.CornerRR;
			Vector3 tVectRL = GSDRI.CornerRL;
			Vector3 tVectLR = GSDRI.CornerLR;
			Vector3 tVectLL = GSDRI.CornerLL;
			Vector3 tDir = default(Vector3);
			float ShoulderWidth1 = GSDRI.Node1.GSDSpline.tRoad.opt_ShoulderWidth;
			float ShoulderWidth2 = GSDRI.Node2.GSDSpline.tRoad.opt_ShoulderWidth;
			
			if(!GSDRI.bFlipped){
				//RR:
				Node1Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime - Node1Width;
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
				tPos2 = GSDRI.Node2.tTime + Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosRR = tVectRR + (tDir * DistFromCorner);
				//RL:
				Node1Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime + Node1Width;
				if(GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){ tPos1 = GSDRI.Node1.tTime; }
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
				tPos2 = GSDRI.Node2.tTime + Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosRL = tVectRL + (tDir * DistFromCorner);
				//LL:
				Node1Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime + Node1Width;
				if(GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){ tPos1 = GSDRI.Node1.tTime; }
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
				tPos2 = GSDRI.Node2.tTime - Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosLL = tVectLL + (tDir * DistFromCorner);
				//LR:
				Node1Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime - Node1Width;
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
				tPos2 = GSDRI.Node2.tTime - Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosLR = tVectLR + (tDir * DistFromCorner);
			}else{
				//RR:
				Node1Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime - Node1Width;
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
				tPos2 = GSDRI.Node2.tTime - Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosRR = tVectRR + (tDir * DistFromCorner);
				//RL:
				Node1Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime + Node1Width;
				if(GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){ tPos1 = GSDRI.Node1.tTime; }
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
				tPos2 = GSDRI.Node2.tTime - Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosRL = tVectRL + (tDir * DistFromCorner);
				//LL:
				Node1Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime + Node1Width;
				if(GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){ tPos1 = GSDRI.Node1.tTime; }
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
				tPos2 = GSDRI.Node2.tTime + Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosLL = tVectLL + (tDir * DistFromCorner);
				//LR:
				Node1Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
				Node2Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
				tPos1 = GSDRI.Node1.tTime - Node1Width;
				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
				tPos2 = GSDRI.Node2.tTime + Node2Width;
				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
				tDir = (tTan1.normalized + tTan2.normalized).normalized;
				tPosLR = tVectLR + (tDir * DistFromCorner);
			}	
			tPosRR.y = GSDRI.SignHeight;
			tPosRL.y = GSDRI.SignHeight;
			tPosLL.y = GSDRI.SignHeight;
			tPosLR.y = GSDRI.SignHeight;
			
//			GameObject tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			tObj.transform.name = "temp22_RR";
//			tObj.transform.position = tPosRR;
//			tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			tObj.transform.name = "temp22_RL";
//			tObj.transform.position = tPosRL;
//			tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			tObj.transform.name = "temp22_LL";
//			tObj.transform.position = tPosLL;
//			tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			tObj.transform.name = "temp22_LR";
//			tObj.transform.position = tPosLR;
		}
	
	
//	
//		public static void GetOrigFour(GSDRoadIntersection GSDRI, out Vector3 tPosRR, out Vector3 tPosRL, out Vector3 tPosLL, out Vector3 tPosLR){
//			//Get four points:
//			float tPos1 = 0f;
//			float tPos2 = 0f;
//			Vector3 tTan1 = default(Vector3);
//			Vector3 tTan2 = default(Vector3);
//			float Node1Width = -1f;
//			float Node2Width = -1f;
//			Vector3 tDirRR = default(Vector3);
//			Vector3 tDirRL = default(Vector3);
//			Vector3 tDirLL = default(Vector3);
//			Vector3 tDirLR = default(Vector3);
//			float tAngleRR = 85f;
//			float tAngleRL = 85f;
//			float tAngleLL = 85f;
//			float tAngleLR = 85f;
//			float ShoulderWidth1 = GSDRI.Node1.GSDSpline.tRoad.opt_ShoulderWidth;
//			float ShoulderWidth2 = GSDRI.Node2.GSDSpline.tRoad.opt_ShoulderWidth;
//			Vector3 xPos1 = default(Vector3);
//			Vector3 xPos2 = default(Vector3);
//			
//			if(!GSDRI.bFlipped){
//				//RR:
//				Node1Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime - Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
//				tPos2 = GSDRI.Node2.tTime + Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tDirRR = (tTan1.normalized + tTan2.normalized).normalized;
//				//tAngleRR = Vector3.Angle(tTan1,tTan2);
//				tAngleRR = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//				//RL:
//				Node1Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime + Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
//				tPos2 = GSDRI.Node2.tTime + Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tDirRL = (tTan1.normalized + tTan2.normalized).normalized;
//				//tAngleRL = Vector3.Angle(tTan1,tTan2);
//				tAngleRL = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//				//LL:
//				Node1Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime + Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
//				tPos2 = GSDRI.Node2.tTime - Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tDirLL = (tTan1.normalized + tTan2.normalized).normalized;
//				//tAngleLL = Vector3.Angle(tTan1,tTan2);
//				tAngleLL = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//				//LR:
//				Node1Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime - Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
//				tPos2 = GSDRI.Node2.tTime - Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tDirLR = (tTan1.normalized + tTan2.normalized).normalized;
//				//tAngleLR = Vector3.Angle(tTan1,tTan2);
//				tAngleLR = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//			}else{
//				//RR:
//				Node1Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerRR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime - Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
//				tPos2 = GSDRI.Node2.tTime - Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
//				tDirRR = (tTan1.normalized + tTan2.normalized).normalized;
////				tAngleRR = Vector3.Angle(tTan1,tTan2);
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tAngleRR = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//				//RL:
//				Node1Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerRL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime + Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
//				tPos2 = GSDRI.Node2.tTime - Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true) * -1f;
//				tDirRL = (tTan1.normalized + tTan2.normalized).normalized;
////				tAngleRL = Vector3.Angle(tTan1,tTan2);
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tAngleRL = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//				//LL:
//				Node1Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerLL,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime + Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true);
//				tPos2 = GSDRI.Node2.tTime + Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
//				tDirLL = (tTan1.normalized + tTan2.normalized).normalized;
////				tAngleLL = Vector3.Angle(tTan1,tTan2);
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tAngleLL = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//				//LR:
//				Node1Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node1.pos) + ShoulderWidth1)/GSDRI.Node1.GSDSpline.distance;
//				Node2Width = (Vector3.Distance(GSDRI.CornerLR,GSDRI.Node2.pos) + ShoulderWidth2)/GSDRI.Node2.GSDSpline.distance;
//				tPos1 = GSDRI.Node1.tTime - Node1Width;
//				tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1,true) * -1f;
//				tPos2 = GSDRI.Node2.tTime + Node2Width;
//				tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2,true);
//				tDirLR = (tTan1.normalized + tTan2.normalized).normalized;
//				//tAngleLR = Vector3.Angle(tTan1,tTan2);
//				xPos1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1);
//				xPos2 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos2);
//				tAngleLR = Vector3.Angle(xPos1 - GSDRI.Node1.pos,xPos2 - GSDRI.Node1.pos);
//			}	
//			
//			//D = B*cos(angle/2)
//			float tWidth = GSDRI.Node1.GSDSpline.tRoad.opt_RoadWidth * 0.5f;
//			float tAngleRR_Opp = 180f - tAngleRR;
//			float tAngleRL_Opp = 180f - tAngleRL;
//			float tAngleLL_Opp = 180f - tAngleLL;
//			float tAngleLR_Opp = 180f - tAngleLR;
//
//			float tOffSetRR = tWidth*(Mathf.Cos((tAngleRR*0.5f)*Mathf.Deg2Rad));
//			float tOffSetRL = tWidth*(Mathf.Cos((tAngleRL*0.5f)*Mathf.Deg2Rad));
//			float tOffSetLL = tWidth*(Mathf.Cos((tAngleLL*0.5f)*Mathf.Deg2Rad));
//			float tOffSetLR = tWidth*(Mathf.Cos((tAngleLR*0.5f)*Mathf.Deg2Rad));
//			
//			float tOffSetRR_opp = tWidth*(Mathf.Cos((tAngleRR_Opp*0.5f)*Mathf.Deg2Rad));
//			float tOffSetRL_opp = tWidth*(Mathf.Cos((tAngleRL_Opp*0.5f)*Mathf.Deg2Rad));
//			float tOffSetLL_opp = tWidth*(Mathf.Cos((tAngleLL_Opp*0.5f)*Mathf.Deg2Rad));
//			float tOffSetLR_opp = tWidth*(Mathf.Cos((tAngleLR_Opp*0.5f)*Mathf.Deg2Rad));
//			
//			Vector3 tPos = GSDRI.Node1.pos;
//			
////			tOffSetRR *=2f;
////			tOffSetRL *=2f;
////			tOffSetLL *=2f;
////			tOffSetLR *=2f;
//			
//			tPosRR = tPos + (tDirRR * (tOffSetRR+tOffSetRR_opp));
//			tPosRL = tPos + (tDirRL * (tOffSetRL+tOffSetRL_opp));
//			tPosLL = tPos + (tDirLL * (tOffSetLL+tOffSetLL_opp));
//			tPosLR = tPos + (tDirLR * (tOffSetLR+tOffSetLR_opp));
//			
//			GameObject tObj = GameObject.Find("temp22_RR"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			tObj = GameObject.Find("temp22_RL"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			tObj = GameObject.Find("temp22_LL"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			tObj = GameObject.Find("temp22_LR"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			
//			GameObject tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosRR;
//			tCubeRR.transform.name = "temp22_RR";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			
//			tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosRL;
//			tCubeRR.transform.name = "temp22_RL";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			
//			tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosLL;
//			tCubeRR.transform.name = "temp22_LL";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			
//			tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosLR;
//			tCubeRR.transform.name = "temp22_LR";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//		}
//	
//		public static void GetCornerVectors_Test(GSDRoadIntersection GSDRI, out Vector3 tPosRR, out Vector3 tPosRL, out Vector3 tPosLL, out Vector3 tPosLR){
//			GSDSplineN tNode = null;
//			GSDSplineC tSpline = null;
//
//			tNode = GSDRI.Node1;
//			tSpline = tNode.GSDSpline;
//			float tOffset = tSpline.tRoad.opt_RoadWidth * 0.5f;
//			float tPos1 = tNode.tTime - (tOffset/tSpline.distance);
//			float tPos2 = tNode.tTime + (tOffset/tSpline.distance);
//			Vector3 tVect1 = tSpline.GetSplineValue(tPos1);	
//			Vector3 POS1 = tSpline.GetSplineValue(tPos1,true);
//			Vector3 tVect2 = tSpline.GetSplineValue(tPos2);	
//			Vector3 POS2 = tSpline.GetSplineValue(tPos2,true);
//			tPosRR = (tVect1 + new Vector3(5f*POS1.normalized.z,0,5f*-POS1.normalized.x));
//			tPosLR = (tVect1 + new Vector3(5f*-POS1.normalized.z,0,5f*POS1.normalized.x));
//			tPosRL = (tVect2 + new Vector3(5f*POS2.normalized.z,0,5f*-POS2.normalized.x));
//			tPosLL = (tVect2 + new Vector3(5f*-POS2.normalized.z,0,5f*POS2.normalized.x));
//			
//			tNode = GSDRI.Node2;
//			tSpline = tNode.GSDSpline;
//			tOffset = tSpline.tRoad.opt_RoadWidth * 0.5f;
//			tPos1 = tNode.tTime - (tOffset/tSpline.distance);
//			tPos2 = tNode.tTime + (tOffset/tSpline.distance);
//			tVect1 = tSpline.GetSplineValue(tPos1);	
//			POS1 = tSpline.GetSplineValue(tPos1,true);
//			tVect2 = tSpline.GetSplineValue(tPos2);	
//			POS2 = tSpline.GetSplineValue(tPos2,true);
//			Vector3 tPosRR2 = default(Vector3);
//			Vector3 tPosLR2 = default(Vector3);
//			Vector3 tPosRL2 = default(Vector3);
//			Vector3 tPosLL2 = default(Vector3);
//			
//			if(GSDRI.bFlipped){
//				tPosRL2 = (tVect1 + new Vector3(5f*POS1.normalized.z,0,5f*-POS1.normalized.x));
//				tPosRR2 = (tVect1 + new Vector3(5f*-POS1.normalized.z,0,5f*POS1.normalized.x));
//				tPosLL2 = (tVect2 + new Vector3(5f*POS2.normalized.z,0,5f*-POS2.normalized.x));
//				tPosLR2 = (tVect2 + new Vector3(5f*-POS2.normalized.z,0,5f*POS2.normalized.x));
//			}else{
//				tPosLR2 = (tVect1 + new Vector3(5f*POS1.normalized.z,0,5f*-POS1.normalized.x));
//				tPosLL2 = (tVect1 + new Vector3(5f*-POS1.normalized.z,0,5f*POS1.normalized.x));
//				tPosRR2 = (tVect2 + new Vector3(5f*POS2.normalized.z,0,5f*-POS2.normalized.x));
//				tPosRL2 = (tVect2 + new Vector3(5f*-POS2.normalized.z,0,5f*POS2.normalized.x));
//			}
//			
//			tPosRR = ((tPosRR-tPosRR2)*0.5f)+tPosRR;
//			tPosLR = ((tPosLR-tPosLR2)*0.5f)+tPosLR;
//			tPosRL = ((tPosRL-tPosRL2)*0.5f)+tPosRL;
//			tPosLL = ((tPosLL-tPosLL2)*0.5f)+tPosLL;
//			
//			
//						GameObject tObj = GameObject.Find("temp22_RR"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			tObj = GameObject.Find("temp22_RL"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			tObj = GameObject.Find("temp22_LL"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			tObj = GameObject.Find("temp22_LR"); if(tObj != null){ Object.DestroyImmediate(tObj); }
//			
//			GameObject tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosRR;
//			tCubeRR.transform.name = "temp22_RR";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			
//			tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosRL;
//			tCubeRR.transform.name = "temp22_RL";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			
//			tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosLL;
//			tCubeRR.transform.name = "temp22_LL";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//			
//			tCubeRR = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			tCubeRR.transform.position = tPosLR;
//			tCubeRR.transform.name = "temp22_LR";
//			tCubeRR.transform.localScale = new Vector3(0.2f,20f,0.2f);
//		}
//	
	}
	
	public static class GSDIntersections{
		public class iConstructionMaker{
			//Lanes:
			public List<Vector3> iBLane0L,iBLane0R;
			public List<Vector3> iBLane1L,iBLane1R;
			public List<Vector3> iBLane2L,iBLane2R;
			public List<Vector3> iBLane3L,iBLane3R;
			public List<Vector3> iFLane0L,iFLane0R;
			public List<Vector3> iFLane1L,iFLane1R;
			public List<Vector3> iFLane2L,iFLane2R;
			public List<Vector3> iFLane3L,iFLane3R;
			//Main plate:
			public List<Vector3> iBMainPlateL; public List<Vector3> iBMainPlateR;
			public List<Vector3> iFMainPlateL; public List<Vector3> iFMainPlateR;	
			//Front marker plates:
			public List<Vector3> iBMarkerPlateL; public List<Vector3> iBMarkerPlateR;
			public List<Vector3> iFMarkerPlateL; public List<Vector3> iFMarkerPlateR;
			
			public float tempconstruction_InterStart;
			public float tempconstruction_InterEnd;
			public bool tempconstruction_HasProcessed_Inter1 = false;
			public bool tempconstruction_HasProcessed_Inter2 = false;
			
			public List<Vector2> tempconstruction_R_RightTurn;
			public List<Vector2> tempconstruction_L_RightTurn;
			public List<Vector2> tempconstruction_R;
			public List<Vector2> tempconstruction_L; 
			
			public float tempconstruction_MinXR = 0f;
			public float tempconstruction_MaxXR = 0f;
			public float tempconstruction_MinXL = 0f;
			public float tempconstruction_MaxXL = 0f;
			
			public float tempconstruction_MinYR = 0f;
			public float tempconstruction_MaxYR = 0f;
			public float tempconstruction_MinYL = 0f;
			public float tempconstruction_MaxYL = 0f;
			
			public bool bBLane0Done = false;
			public bool bBLane1Done = false;
			public bool bBLane2Done = false;
			public bool bBLane3Done = false;
			public bool bFLane0Done = false;
			public bool bFLane1Done = false;
			public bool bFLane2Done = false;
			public bool bFLane3Done = false;
			
			public bool bBLane0Done_Final = false;
			public bool bBLane1Done_Final = false;
			public bool bBLane2Done_Final = false;
			public bool bBLane3Done_Final = false;
			public bool bFLane0Done_Final = false;
			public bool bFLane1Done_Final = false;
			public bool bFLane2Done_Final = false;
			public bool bFLane3Done_Final = false;
			
			public bool bBLane0Done_Final_ThisRound = false;
			public bool bBLane1Done_Final_ThisRound = false;
			public bool bBLane2Done_Final_ThisRound = false;
			public bool bBLane3Done_Final_ThisRound = false;
			public bool bFLane0Done_Final_ThisRound = false;
			public bool bFLane1Done_Final_ThisRound = false;
			public bool bFLane2Done_Final_ThisRound = false;
			public bool bFLane3Done_Final_ThisRound = false;
			
			public bool bFDone = false;
			public bool bBDone = false;
			
			public bool bIsFrontFirstRound = false;
			public bool bIsFrontFirstRoundTriggered = false;
			
			public Vector3 f0LAttempt = default(Vector3);
			public Vector3 f1LAttempt = default(Vector3);
			public Vector3 f2LAttempt = default(Vector3);
			public Vector3 f3LAttempt = default(Vector3);
			public Vector3 f0RAttempt = default(Vector3);
			public Vector3 f1RAttempt = default(Vector3);
			public Vector3 f2RAttempt = default(Vector3);
			public Vector3 f3RAttempt = default(Vector3);
			
			public bool bNode1RLTriggered = false;
			
			public bool bDepressDoneR = false;
			public bool bDepressDoneL = false;
			
			public bool bBackRRpassed = false;
			
			public List<Vector3> iBLane0_Real;
			
			public Vector3 ShoulderBL_Start = default(Vector3);
			public Vector3 ShoulderBR_Start = default(Vector3);
			public Vector3 ShoulderFL_Start = default(Vector3);
			public Vector3 ShoulderFR_Start = default(Vector3);
			
			public Vector3 ShoulderBL_End = default(Vector3);
			public Vector3 ShoulderBR_End = default(Vector3);
			public Vector3 ShoulderFL_End = default(Vector3);
			public Vector3 ShoulderFR_End = default(Vector3);
			
			public int ShoulderBL_StartIndex = -1;
			public int ShoulderBR_StartIndex = -1;
			public int ShoulderFL_StartIndex = -1;
			public int ShoulderFR_StartIndex = -1;
			
			public void Nullify(){
				
				//Intersection construction:
				if(iBLane0L != null){ iBLane0L.Clear(); iBLane0L = null; }
				if(iBLane0R != null){ iBLane0R.Clear(); iBLane0R = null; }
				if(iBLane1L != null){ iBLane1L.Clear(); iBLane1L = null; }
				if(iBLane1R != null){ iBLane1R.Clear(); iBLane1R = null; }
				if(iBLane2L != null){ iBLane2L.Clear(); iBLane2L = null; }
				if(iBLane2R != null){ iBLane2R.Clear(); iBLane2R = null; }
				if(iBLane3L != null){ iBLane3L.Clear(); iBLane3L = null; }
				if(iBLane3R != null){ iBLane3R.Clear(); iBLane3R = null; }
				if(iFLane0L != null){ iFLane0L.Clear(); iFLane0L = null; }
				if(iFLane0R != null){ iFLane0R.Clear(); iFLane0R = null; }
				if(iFLane1L != null){ iFLane1L.Clear(); iFLane1L = null; }
				if(iFLane1R != null){ iFLane1R.Clear(); iFLane1R = null; }
				if(iFLane2L != null){ iFLane2L.Clear(); iFLane2L = null; }
				if(iFLane2R != null){ iFLane2R.Clear(); iFLane2R = null; }
				if(iFLane3L != null){ iFLane3L.Clear(); iFLane3L = null; }
				if(iFLane3R != null){ iFLane3R.Clear(); iFLane3R = null; }	
				if(iBMainPlateL != null){ iBMainPlateL.Clear(); iBMainPlateL = null; }	
				if(iBMainPlateR != null){ iBMainPlateR.Clear(); iBMainPlateR = null; }	
				if(iFMainPlateL != null){ iFMainPlateL.Clear(); iFMainPlateL = null; }	
				if(iFMainPlateR != null){ iFMainPlateR.Clear(); iFMainPlateR = null; }	
				
				if(iBMarkerPlateL != null){ iBMarkerPlateL.Clear(); iBMarkerPlateL = null; }	
				if(iBMarkerPlateR != null){ iBMarkerPlateR.Clear(); iBMarkerPlateR = null; }	
				if(iFMarkerPlateL != null){ iFMarkerPlateL.Clear(); iFMarkerPlateL = null; }	
				if(iFMarkerPlateR != null){ iFMarkerPlateR.Clear(); iFMarkerPlateR = null; }
				
				if(tempconstruction_R != null){ tempconstruction_R = null; }
				if(tempconstruction_L != null){ tempconstruction_L = null; }
			}
			
			public iConstructionMaker(){
				Nullify();
				
				iBLane0_Real = new List<Vector3>();
				
				//Lanes:
				iBLane0L = new List<Vector3>();
				iBLane0R = new List<Vector3>();
				iBLane1L = new List<Vector3>();
				iBLane1R = new List<Vector3>();
				iBLane2L = new List<Vector3>();
				iBLane2R = new List<Vector3>();
				iBLane3L = new List<Vector3>();
				iBLane3R = new List<Vector3>();
				iFLane0L = new List<Vector3>();
				iFLane0R = new List<Vector3>();
				iFLane1L = new List<Vector3>();
				iFLane1R = new List<Vector3>();
				iFLane2L = new List<Vector3>();
				iFLane2R = new List<Vector3>();
				iFLane3L = new List<Vector3>();
				iFLane3R = new List<Vector3>();
				//Main plate:
				iBMainPlateL = new List<Vector3>();
				iBMainPlateR = new List<Vector3>();
				iFMainPlateL = new List<Vector3>();
				iFMainPlateR = new List<Vector3>();
				
				iBMarkerPlateL = new List<Vector3>(); 
				iBMarkerPlateR = new List<Vector3>();
				iFMarkerPlateL = new List<Vector3>();
				iFMarkerPlateR = new List<Vector3>();
				
				tempconstruction_HasProcessed_Inter1 = false;
				tempconstruction_HasProcessed_Inter2 = false;
				tempconstruction_MinXR = 20000000f;
				tempconstruction_MaxXR = 0f;
				tempconstruction_MinXL = 20000000f;
				tempconstruction_MaxXL = 0f;
				tempconstruction_MinYR = 20000000f;
				tempconstruction_MaxYR = 0f;
				tempconstruction_MinYL = 20000000f;
				tempconstruction_MaxYL = 0f;
				
				bBLane0Done = false;
				bBLane1Done = false;
				bBLane2Done = false;
				bBLane3Done = false;
				bFLane0Done = false;
				bFLane1Done = false;
				bFLane2Done = false;
				bFLane3Done = false;
			}
		}
		
		public static GameObject CreateIntersection(GSDSplineN tNode,GSDSplineN xNode){
			return CreateIntersection_Do(tNode,xNode);
		}
		private static GameObject CreateIntersection_Do(GSDSplineN tNode,GSDSplineN xNode){
			float RoadMod = 10f;
			GameObject SystemObj = tNode.transform.parent.parent.parent.gameObject;
			if(!SystemObj){ Debug.LogWarning("Could not find GSD road system master object."); return null; }
			GameObject InterMaster = null;
			int cCount = SystemObj.transform.childCount;
			for(int i=0;i<cCount;i++){
				if(SystemObj.transform.GetChild(i).transform.name.ToLower() == "intersections"){
					InterMaster=SystemObj.transform.GetChild(i).gameObject;
				}
			}
			if(!InterMaster){ 
				InterMaster = new GameObject("Intersections"); 
				InterMaster.transform.parent = SystemObj.transform;
			}
			if(!InterMaster){ 
				Debug.LogWarning("Could not find intersections master object for this road system."); 
				return null; 
			}
			cCount = InterMaster.transform.childCount;
			
			GameObject tObj = new GameObject("Inter" + (cCount+1).ToString());
			tObj.transform.parent = InterMaster.transform;
			GSDRoadIntersection GSDRI = tObj.AddComponent<GSDRoadIntersection>();
			GSDRI.IgnoreSide = -1;
			GSDRI.bFirstSpecial_First = false;
			GSDRI.bFirstSpecial_Last = false;
			GSDRI.bSecondSpecial_First = false;
			GSDRI.bSecondSpecial_Last = false;
			
			GSDSplineN tNode1 = null;
			GSDSplineN tNode2 = null;
			if(tNode.GSDSpline == xNode.GSDSpline){
				if(tNode.idOnSpline < xNode.idOnSpline){
					tNode1 = tNode;
					tNode2 = xNode;
				}else{
					tNode1 = xNode;
					tNode2 = tNode;
				}
			}else{
				tNode1 = tNode;
				tNode2 = xNode;
			}
			
			//If 3way, always add the single node as primary:
			if(tNode.bIsEndPoint){
				tNode1 = tNode;
				tNode2 = xNode;
			}else if(xNode.bIsEndPoint){
				tNode1 = xNode;
				tNode2 = tNode;
			}
			
			tNode1.Intersection_OtherNode = tNode2;
			tNode2.Intersection_OtherNode = tNode1;
			
			if(tNode1.bIsEndPoint || tNode2.bIsEndPoint){
				GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.ThreeWay;	
			}
			
			GSDSplineN zNode = null;
			if(tNode1.bIsEndPoint){
				bool bFirstNode = false;
				bool bAlreadyNode = false;
				if(tNode1.idOnSpline == 1 || tNode1.idOnSpline == 0){
					bFirstNode = true;
				}
				if(bFirstNode && tNode1.idOnSpline == 1 && tNode1.GSDSpline.mNodes[0].bSpecialEndNode_IsStart){
					bAlreadyNode = true;	
				}else if(!bFirstNode && tNode1.idOnSpline == tNode1.GSDSpline.GetNodeCount()-2 && tNode1.GSDSpline.mNodes[tNode1.GSDSpline.GetNodeCount()-1].bSpecialEndNode_IsEnd){
					bAlreadyNode = true;	
				}
				
				Vector3 tPos = default(Vector3);
				if(bFirstNode){
					tPos = ((tNode1.tangent * -1f).normalized * (tNode1.GSDSpline.tRoad.RoadWidth()*RoadMod)) + tNode1.pos;
				}else{
					tPos = (tNode1.GSDSpline.GetSplineValue(0.999f,true).normalized * (tNode1.GSDSpline.tRoad.RoadWidth()*RoadMod)) + tNode1.pos;
				}
				
				if(!bAlreadyNode){
					if(bFirstNode){
						zNode = GSD.Roads.GSDConstruction.InsertNode(tNode1.GSDSpline.tRoad,true,tPos,false,0,true,true);
						zNode.bSpecialEndNode_IsStart = true;
						zNode.bSpecialIntersection = true;
						zNode.tangent = tNode1.tangent;
					}else{
						zNode = GSD.Roads.GSDConstruction.CreateNode(tNode1.GSDSpline.tRoad,true,tPos,true);	
						zNode.bSpecialEndNode_IsEnd = true;
						zNode.bSpecialIntersection = true;
						zNode.tangent = tNode1.tangent;
					}
				}else{
					if(bFirstNode){
						tNode1.GSDSpline.mNodes[0].transform.position = tPos;
					}else{
						tNode1.GSDSpline.mNodes[tNode1.GSDSpline.GetNodeCount()-1].transform.position = tPos;
					}
				}
				if(bFirstNode){
					tNode1.GSDSpline.bSpecialStartControlNode = true;	
					GSDRI.bFirstSpecial_First = true;
				}else{
					tNode1.GSDSpline.bSpecialEndControlNode = true;
					GSDRI.bFirstSpecial_Last = true;
				}
				
			}else if(tNode2.bIsEndPoint){
				bool bFirstNode = false;
				bool bAlreadyNode = false;
				if(tNode2.idOnSpline == 1 || tNode2.idOnSpline == 0){
					bFirstNode = true;
				}
				if(bFirstNode && tNode2.idOnSpline == 1 && tNode2.GSDSpline.mNodes[0].bSpecialEndNode_IsStart){
					bAlreadyNode = true;	
				}else if(!bFirstNode && tNode2.idOnSpline == tNode2.GSDSpline.GetNodeCount()-2 && tNode2.GSDSpline.mNodes[tNode2.GSDSpline.GetNodeCount()-1].bSpecialEndNode_IsEnd){
					bAlreadyNode = true;	
				}
				
				Vector3 tPos = default(Vector3);
				if(bFirstNode){
					tPos = ((tNode2.tangent * -1f).normalized * (tNode2.GSDSpline.tRoad.RoadWidth()*RoadMod)) + tNode2.pos;
				}else{
					tPos = (tNode2.GSDSpline.GetSplineValue(0.999f,true).normalized * (tNode2.GSDSpline.tRoad.RoadWidth()*RoadMod)) + tNode2.pos;
				}
				
				if(!bAlreadyNode){
					if(bFirstNode){
						zNode = GSD.Roads.GSDConstruction.InsertNode(tNode2.GSDSpline.tRoad,true,tPos,false,0,true,true);
						zNode.bSpecialEndNode_IsStart = true;
						zNode.bSpecialIntersection = true;
						zNode.tangent = tNode2.tangent;
					}else{
						zNode = GSD.Roads.GSDConstruction.CreateNode(tNode2.GSDSpline.tRoad,true,tPos,true);	
						zNode.bSpecialEndNode_IsEnd = true;
						zNode.bSpecialIntersection = true;
						zNode.tangent = tNode2.tangent;
					}
				}else{
					if(bFirstNode){
						tNode2.GSDSpline.mNodes[0].transform.position = tPos;
					}else{
						tNode2.GSDSpline.mNodes[tNode2.GSDSpline.GetNodeCount()-1].transform.position = tPos;
					}
				}
				if(bFirstNode){
					tNode2.GSDSpline.bSpecialStartControlNode = true;	
					GSDRI.bSecondSpecial_First = true;
				}else{
					tNode2.GSDSpline.bSpecialEndControlNode = true;
					GSDRI.bSecondSpecial_Last = true;
				}
			}
			
			//Undo crap:
            UnityEditor.Undo.RegisterCreatedObjectUndo(tObj, "Created intersection");
            
			GSDRI.Setup(tNode1,tNode2);
			tObj.transform.position = tNode.transform.position;
			
			GSDRI.ResetMaterials_All();
			
//			if(GSDRI.bSameSpline){
//				GSDRI.Node1.GSDSpline.tRoad.UpdateRoad();
//			}else{
//				GSDRI.Node1.GSDSpline.tRoad.UpdateRoad();
//				GSDRI.Node2.GSDSpline.tRoad.UpdateRoad();
//			}
			
			tNode1.ToggleHideFlags(true);
			tNode2.ToggleHideFlags(true);
			
			if(GSDRI != null && GSDRI.Node1 != null && GSDRI.Node2 != null){
				if(!GSDRI.bSameSpline){
					GSDRI.Node1.GSDSpline.tRoad.PiggyBacks = new GSDSplineC[4];
					GSDRI.Node1.GSDSpline.tRoad.PiggyBacks[0] = GSDRI.Node2.GSDSpline;
					
					GSDRI.Node1.GSDSpline.tRoad.PiggyBacks[1] = GSDRI.Node1.GSDSpline;
					GSDRI.Node1.GSDSpline.tRoad.PiggyBacks[2] = GSDRI.Node2.GSDSpline;
					GSDRI.Node1.GSDSpline.tRoad.PiggyBacks[3] = GSDRI.Node1.GSDSpline;
//					GSDRI.Node1.GSDSpline.tRoad.PiggyBacks[4] = GSDRI.Node2.GSDSpline;
				}
				GSDRI.Node1.GSDSpline.tRoad.EditorUpdateMe = true;
			}
			
			return tObj;
		}
		
		public static Vector3[] GetCornerVectors_Test(GSDRoadIntersection GSDRI){
			Vector3[] tVects = new Vector3[4];
			GSDSplineN tNode;
			tNode = GSDRI.Node1;
			GSDSplineC tSpline = tNode.GSDSpline;
			
			//RR = Node1 - 5, Node2 + 5
			//RL = Node1 + 5, Node2 + 5
			//LL = Node1 + 5, Node2 - 5
			//LR = Node1 - 5, Node2 - 5

			float tOffset = 5f;
			float tPos1 = tNode.tTime - (tOffset/tSpline.distance);
			float tPos2 = tNode.tTime + (tOffset/tSpline.distance);
			Vector3 tVect1 = tSpline.GetSplineValue(tPos1);	
			Vector3 POS1 = tSpline.GetSplineValue(tPos1,true);
			Vector3 tVect2 = tSpline.GetSplineValue(tPos2);	
			Vector3 POS2 = tSpline.GetSplineValue(tPos2,true);
	
			tVects[0] = (tVect1 + new Vector3(5f*POS1.normalized.z,0,5f*-POS1.normalized.x));
			tVects[1] = (tVect1 + new Vector3(5f*-POS1.normalized.z,0,5f*POS1.normalized.x));
			tVects[2] = (tVect2 + new Vector3(5f*POS2.normalized.z,0,5f*-POS2.normalized.x));
			tVects[3] = (tVect2 + new Vector3(5f*-POS2.normalized.z,0,5f*POS2.normalized.x));
			
			return tVects;
		}
		
		#region "Old intersection"
		public static void CreateIntersection(GSDRoadIntersection GSDRI){
			//1. Overlap sphere to find all road objects within intersection:
			float iWidth = GSDRI.IntersectionWidth*1.25f;
			Collider[] tColliders = Physics.OverlapSphere(GSDRI.transform.position,iWidth);
			if(tColliders == null || tColliders.Length < 1){ return; }
			List<GSDRoad> tRoads = new List<GSDRoad>();
			foreach(Collider tCollider in tColliders){
				if(tCollider.transform.parent){
					GSDRoad tRoad = tCollider.transform.parent.GetComponent<GSDRoad>();
					if(tRoad){
						if(!tRoads.Contains(tRoad)){
							tRoads.Add(tRoad);
						}
					}
				}
			}
			
			//Flatten intersection area:
			float tHeight = -1f;
			FlattenIntersectionArea(ref tRoads,GSDRI,iWidth,out tHeight);
			
			//Create main intersection mesh:
			string tName = GSDRI.transform.name;
			Vector3[] tVects;
			CreateIntersectionMesh_Main(GSDRI,tHeight,out tVects,ref tName);
			
			//Now create the 4 text overlays:
			CreateIntersectionMesh_Outer(GSDRI,tVects,ref tName);
			
			//Update connected nodes:
			GSDNavigation.UpdateConnectedNodes();
			
			//Now initialize intersection objects:
			InitializeIntersectionObjects(GSDRI);
		}

		private static void FlattenIntersectionArea(ref List<GSDRoad> tRoads, GSDRoadIntersection GSDRI, float iWidth, out float tHeight){
			//Cycle through each road and get all mesh vertices that are within range:
			Vector3 tInterPos = GSDRI.transform.position;
			float tInterPosY = tInterPos.y;
			foreach(GSDRoad tRoad in tRoads){
				MeshFilter MF_Road = tRoad.MeshRoad.GetComponent<MeshFilter>();
				MeshFilter MF_Road_SR = tRoad.MeshShoR.GetComponent<MeshFilter>();
				MeshFilter MF_Road_SL = tRoad.MeshShoL.GetComponent<MeshFilter>();
				
				Mesh Road = MF_Road.sharedMesh;
				Mesh Road_SR = MF_Road_SR.sharedMesh;
				Mesh Road_SL = MF_Road_SL.sharedMesh;
				
				Vector3[] tVects = Road.vertices;
				Vector3[] tVects_SR = Road_SR.vertices;
				Vector3[] tVects_SL = Road_SL.vertices;
				int VertCount = Road.vertices.Length;
				bool bLeftToggle = true;
				for(int i=0;i<VertCount;i+=2){
					if(Vector3.Distance(tVects[i],tInterPos) < iWidth){
						tVects[i].y = tInterPosY;
						tVects[i+1].y = tInterPosY;
						if(bLeftToggle){
							//Left:
							tVects_SL[i+2].y = tInterPosY;
							tVects_SL[i+3].y = tInterPosY;
						}else{
							//Right:
							tVects_SR[i-2].y = tInterPosY;
							tVects_SR[i-1].y = tInterPosY;
						}
					}
					bLeftToggle = !bLeftToggle;
				}
				//Main road:
				Road.vertices = tVects;
				Road.RecalculateNormals();
				MF_Road.sharedMesh = Road;
				//Right shoulder:
				Road_SR.vertices = tVects_SR;
				Road_SR.RecalculateNormals();
				MF_Road_SR.sharedMesh = Road_SR;
				//Left shoulder:
				Road_SL.vertices = tVects_SL;
				Road_SL.RecalculateNormals();
				MF_Road_SL.sharedMesh = Road_SL;
			}
			tHeight = tInterPosY;
		}
		
		private static bool V3Equal(Vector3 a, Vector3 b){
    		return Vector3.SqrMagnitude(a - b) < 0.0001f;
    	}
		
		private static Vector3[] GetCornerVectors(GSDRoadIntersection GSDRI, bool bPrimary = true){
			Vector3[] tVects = new Vector3[4];
			GSDSplineN tNode;
			if(bPrimary){
				tNode = GSDRI.Node1;
			}else{
				tNode = GSDRI.Node2;
			}
			GSDSplineC tSpline = tNode.GSDSpline;
			
			float tOffset = 7f;
			float tPos1 = tNode.tTime - (tOffset/tSpline.distance);
			float tPos2 = tNode.tTime + (tOffset/tSpline.distance);
			Vector3 tVect1 = tSpline.GetSplineValue(tPos1);	
			Vector3 POS1 = tSpline.GetSplineValue(tPos1,true);
			Vector3 tVect2 = tSpline.GetSplineValue(tPos2);	
			Vector3 POS2 = tSpline.GetSplineValue(tPos2,true);
	
			tVects[0] = (tVect1 + new Vector3(5f*POS1.normalized.z,0,5f*-POS1.normalized.x));
			tVects[1] = (tVect1 + new Vector3(5f*-POS1.normalized.z,0,5f*POS1.normalized.x));
			tVects[2] = (tVect2 + new Vector3(5f*POS2.normalized.z,0,5f*-POS2.normalized.x));
			tVects[3] = (tVect2 + new Vector3(5f*-POS2.normalized.z,0,5f*POS2.normalized.x));
			
			return tVects;
		}
		
		private static Vector3[] GetExtendedVectors(GSDRoadIntersection GSDRI, bool bPrimary = true){
			Vector3[] tVects = new Vector3[4];
			GSDSplineN tNode;
			if(bPrimary){
				tNode = GSDRI.Node1;
			}else{
				tNode = GSDRI.Node2;
			}
			GSDSplineC tSpline = tNode.GSDSpline;
			Vector3 NodePos = tNode.transform.position;
			
			float tOffset = tNode.GSDSpline.tRoad.RoadWidth(); 
			float tOffset2 = tOffset*0.5f;
			float tPos1 = tNode.tTime - (tOffset/tSpline.distance);
			float tPos2 = tNode.tTime + (tOffset/tSpline.distance);
			Vector3 tVect1 = tSpline.GetSplineValue(tPos1);	
			Vector3 tVect2 = tSpline.GetSplineValue(tPos2);	
			
			//Enforce distance:
			int SpamGuard = 0;
			int SGMax = 50;
			while(Vector3.Distance(tVect1,NodePos) < tOffset && SpamGuard < SGMax){ SpamGuard+=1;
				tPos1-=(1f/tSpline.distance);
				tVect1 = tSpline.GetSplineValue(tPos1);	
			} SpamGuard = 0;
			while(Vector3.Distance(tVect1,NodePos) > (tOffset*1.2f) && SpamGuard < SGMax){ SpamGuard+=1;
				tPos1+=(0.25f/tSpline.distance);
				tVect1 = tSpline.GetSplineValue(tPos1);	
			} SpamGuard = 0;
			while(Vector3.Distance(tVect2,NodePos) < tOffset && SpamGuard < SGMax){ SpamGuard+=1;
				tPos2+=(1f/tSpline.distance);
				tVect2 = tSpline.GetSplineValue(tPos2);	
			} SpamGuard = 0;
			while(Vector3.Distance(tVect1,NodePos) > (tOffset*1.2f) && SpamGuard < SGMax){ SpamGuard+=1;
				tPos2-=(0.25f/tSpline.distance);
				tVect2 = tSpline.GetSplineValue(tPos2);	
			}
			
			Vector3 POS1 = tSpline.GetSplineValue(tPos1,true);
			Vector3 POS2 = tSpline.GetSplineValue(tPos2,true);
			
			tVects[0] = (tVect1 + new Vector3(tOffset2*POS1.normalized.z,0,tOffset2*-POS1.normalized.x));
			tVects[1] = (tVect1 + new Vector3(tOffset2*-POS1.normalized.z,0,tOffset2*POS1.normalized.x));
			tVects[2] = (tVect2 + new Vector3(tOffset2*POS2.normalized.z,0,tOffset2*-POS2.normalized.x));
			tVects[3] = (tVect2 + new Vector3(tOffset2*-POS2.normalized.z,0,tOffset2*POS2.normalized.x));
			
			return tVects;
		}
		
		//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
		//to each other. This function finds those two points. If the lines are not parallel, the function 
		//outputs true, otherwise false.
		private static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){
	 
			closestPointLine1 = Vector3.zero;
			closestPointLine2 = Vector3.zero;
	 
			float a = Vector3.Dot(lineVec1, lineVec1);
			float b = Vector3.Dot(lineVec1, lineVec2);
			float e = Vector3.Dot(lineVec2, lineVec2);
	 
			float d = a*e - b*b;
	 
			//lines are not parallel
			if(d != 0.0f){
	 
				Vector3 r = linePoint1 - linePoint2;
				float c = Vector3.Dot(lineVec1, r);
				float f = Vector3.Dot(lineVec2, r);
	 
				float s = (b*f - c*e) / d;
				float t = (a*f - c*b) / d;
	 
				closestPointLine1 = linePoint1 + lineVec1 * s;
				closestPointLine2 = linePoint2 + lineVec2 * t;
	 
				return true;
			}
	 
			else{
				return false;
			}
		}	
		
		
		private static void CreateIntersectionMesh_Main(GSDRoadIntersection GSDRI, float tHeight, out Vector3[] tVects,ref string tName){
			//Get four points:
			Vector3[] pVects = GetCornerVectors(GSDRI);
			Vector3[] sVects = GetCornerVectors(GSDRI,false);
			tVects = new Vector3[4];
			Vector3 oIntersection = new Vector3(0f,0f,0f);
			Vector3 oIntersection2 = new Vector3(0f,0f,0f);//Unused
			
//			bool bIntersection;
			ClosestPointsOnTwoLines(out oIntersection,out oIntersection2,pVects[0],(pVects[2]-pVects[0]),sVects[0],(sVects[2]-sVects[0]));
			tVects[0] = oIntersection;
			tVects[0].y = tHeight;
			
			ClosestPointsOnTwoLines(out oIntersection,out oIntersection2,pVects[0],(pVects[2]-pVects[0]),sVects[1],(sVects[3]-sVects[1]));
			tVects[1] = oIntersection;
			tVects[1].y = tHeight;
			
			ClosestPointsOnTwoLines(out oIntersection,out oIntersection2,pVects[1],(pVects[3]-pVects[1]),sVects[0],(sVects[2]-sVects[0]));
			tVects[2] = oIntersection;
			tVects[2].y = tHeight;
			
			ClosestPointsOnTwoLines(out oIntersection,out oIntersection2,pVects[1],(pVects[3]-pVects[1]),sVects[1],(sVects[3]-sVects[1]));
			tVects[3] = oIntersection;
			tVects[3].y = tHeight;
			
			CreateIntersectionMesh_MainInternal(tVects,GSDRI.transform.gameObject, ref tName);	
		}
		
		private static void CreateIntersectionMesh_MainInternal(Vector3[] tVerts, GameObject iObj,ref string tName){
			Mesh tMesh = new Mesh();
			int MVL = 4;
			int triCount = (int)(4f*1.5f);
			
//			GameObject tObj;
//			tObj = GameObject.Find("tInter1"); tObj.transform.position = tVerts[0];
//			tObj = GameObject.Find("tInter2"); tObj.transform.position = tVerts[1];
//			tObj = GameObject.Find("tInter3"); tObj.transform.position = tVerts[2];
//			tObj = GameObject.Find("tInter4"); tObj.transform.position = tVerts[3];
			
			for(int i=0;i<MVL;i++){
				tVerts[i] -= iObj.transform.position;	
			}
			tMesh.vertices = tVerts;
			tMesh.RecalculateBounds();

			int[] tri = new int[triCount];
			tri[0] = 0;
			tri[1] = 2;
			tri[2] = 1;
			tri[3] = 2;
			tri[4] = 3;
			tri[5] = 1;
			tMesh.triangles = tri;

			Vector3[] normals = new Vector3[MVL];
			for(int i=0;i<MVL;i++){
				normals[i] = -Vector3.forward;
			}
			tMesh.normals = normals;
			tMesh.RecalculateNormals();
			
			Vector2[] uv = new Vector2[MVL];
			uv[0] = new Vector2(0f,0f);
			uv[1] = new Vector2(1f,0f);
			uv[2] = new Vector2(0f,1f);
			uv[3] = new Vector2(1f,1f);
			tMesh.uv = uv;
			
			GSDRootUtil.ProcessTangents(ref tMesh);

			//Final processing:
			MeshFilter MF = iObj.GetComponent<MeshFilter>();
			if(!MF){ MF = iObj.AddComponent<MeshFilter>(); }
			MF.sharedMesh = tMesh;
//			MeshToFile(MF,"Assets/RoadArchitect/Mesh/Intersections/" + tName +".obj");
			
			MeshCollider MC = iObj.GetComponent<MeshCollider>();
			if(MC){ Object.DestroyImmediate(MC); }
			//if(!MC){ MC = iObj.AddComponent<MeshCollider>(); }
			//MC.sharedMesh = MF.sharedMesh;
			
			MeshRenderer MR = iObj.GetComponent<MeshRenderer>();
			if(!MR){ MR = iObj.AddComponent<MeshRenderer>(); }
            MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;            
            MR.receiveShadows = true;
			GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/GSDRoadIntersection.mat",MR);
		}
	
		private static void CreateIntersectionMesh_Outer(GSDRoadIntersection GSDRI,Vector3[] tVects, ref string tName){
			Vector3[] bVects1 = GetExtendedVectors(GSDRI);
			Vector3[] bVects2 = GetExtendedVectors(GSDRI,false);
			Vector3[] eVects = new Vector3[16];
			eVects[0] = tVects[1];
			eVects[1] = tVects[0];
			eVects[2] = bVects2[3];
			eVects[3] = bVects2[2];
			//
			eVects[4] = tVects[3];
			eVects[5] = tVects[1];
			eVects[6] = bVects1[3];
			eVects[7] = bVects1[2];
			//
			eVects[8] = tVects[2];
			eVects[9] = tVects[3];
			eVects[10] = bVects2[0];
			eVects[11] = bVects2[1];
			//
			eVects[12] = tVects[0];
			eVects[13] = tVects[2];
			eVects[14] = bVects1[0];
			eVects[15] = bVects1[1];

			int cCount = GSDRI.transform.childCount;
//			bool bOuter = false;
			GameObject tOuter = null;
			for(int i=0;i<cCount;i++){
				if(GSDRI.transform.GetChild(i).transform.name == "outer"){
					tOuter = GSDRI.transform.GetChild(i).transform.gameObject;
				}
			}
			if(!tOuter){
				tOuter = new GameObject("outer");
				tOuter.transform.parent = GSDRI.transform;
			}
			tOuter.transform.position = GSDRI.transform.position;

//			GameObject tObj;
//			tObj = GameObject.Find("tInter1"); tObj.transform.position = bVects2[0];
//			tObj = GameObject.Find("tInter2"); tObj.transform.position = bVects2[1];
//			tObj = GameObject.Find("tInter3"); tObj.transform.position = bVects2[2];
//			tObj = GameObject.Find("tInter4"); tObj.transform.position = bVects2[3];
			
			CreateIntersectionMesh_OuterInternal(eVects,tOuter,GSDRI.transform.position, ref tName);	
		}
		
		private static void CreateIntersectionMesh_OuterInternal(Vector3[] tVerts, GameObject iObj, Vector3 vOffset, ref string tName){
			Mesh tMesh = new Mesh();
			int MVL = 16;
			int triCount = (int)(16f*1.5f);
			
			for(int i=0;i<MVL;i+=4){
//				tVerts[i] += vOffset;
//				tVerts[i+1] += vOffset;
				tVerts[i+2] -= vOffset;
				tVerts[i+3] -= vOffset;
			}
			tMesh.vertices = tVerts;
			tMesh.RecalculateBounds();

			int[] tri = new int[triCount];
			int cTri = 0;
			for(int i=0;i<triCount;i+=4){
				if(i+3 >= MVL){
					break;	
				}
				tri[cTri] = i; cTri+=1;
				tri[cTri] = i+2; cTri+=1;
				tri[cTri] = i+1; cTri+=1;
				
				tri[cTri] = i+2; cTri+=1;
				tri[cTri] = i+3; cTri+=1;
				tri[cTri] = i+1; cTri+=1;
			}
			tMesh.triangles = tri;

			Vector3[] normals = new Vector3[MVL];
			for(int i=0;i<4;i++){
				normals[i] = -Vector3.forward;
			}
			tMesh.normals = normals;
			tMesh.RecalculateNormals();
			
			Vector2[] uv = new Vector2[MVL];
			for(int i=0;i<MVL;i+=4){
				uv[i] = new Vector2(1f,1f);
				uv[i+1] = new Vector2(0f,1f);
				uv[i+2] = new Vector2(1f,0f);
				uv[i+3] = new Vector2(0f,0f);
			}
			tMesh.uv = uv;
			
			GSDRootUtil.ProcessTangents(ref tMesh);

			//Final processing:
			MeshFilter MF = iObj.GetComponent<MeshFilter>();
			if(!MF){ MF = iObj.AddComponent<MeshFilter>(); }
			MF.sharedMesh = tMesh;
			
//			MeshToFile(MF,"Assets/RoadArchitect/Mesh/Intersections/" + tName +"-overlay.obj");

			MeshCollider MC = iObj.GetComponent<MeshCollider>();
			if(MC){ Object.DestroyImmediate(MC); }
//			if(!MC){ MC = iObj.AddComponent<MeshCollider>(); }
//			MC.sharedMesh = MF.sharedMesh;
			
			MeshRenderer MR = iObj.GetComponent<MeshRenderer>();
			if(!MR){ MR = iObj.AddComponent<MeshRenderer>(); }
            MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MR.receiveShadows = true;
			GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/GSDInterText.mat",MR);
		}

		#region "Intersection creation"
		private static void InitializeIntersectionObjects(GSDRoadIntersection tGSDRI){
			if(tGSDRI != null){
				InitializeIntersectionObjects_Internal(tGSDRI);
			}else{
				Object[] iObjects = GameObject.FindObjectsOfType(typeof(GSDRoadIntersection));
				//Add intersection components, if necessary:
				foreach (GSDRoadIntersection GSDRI in iObjects){
					InitializeIntersectionObjects_Internal(GSDRI);
				}
			}
		}
		
		private static void InitializeIntersectionObjects_Internal(GSDRoadIntersection GSDRI){			
			//1. Determine 3-way or 4-way. # of corners for 3-way: 2. 4-way = 4.
			if(GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){
				GSDRI.CornerPoints = new GSDRoadIntersection.CornerPositionMaker[2];
			}else{
				GSDRI.CornerPoints = new GSDRoadIntersection.CornerPositionMaker[4];
			}
			
			//Contains int IDs of connected nodes:			
			List<GSDSplineN> tList = new List<GSDSplineN>();
			//Get all connected nodes on intersection node1:
			int cCount = GSDRI.Node1.id_connected.Count;
			GSDSplineN tNode;
			for(int i=0;i<cCount;i++){
				//tNode = GetNodeByID(GSDRI.Node1.id_connected[i]);
				tNode = GSDRI.Node1.node_connected[i];
				if(!tList.Contains(tNode)){
					tList.Add(tNode);
				}
			}
			//Get all connected nodes on intersection node2:
			cCount = GSDRI.Node2.id_connected.Count;
			for(int i=0;i<cCount;i++){
				//tNode = GetNodeByID(GSDRI.Node2.id_connected[i]);
				tNode = GSDRI.Node2.node_connected[i];
				if(!tList.Contains(tNode)){
					tList.Add(tNode);
				}
			}
			//Declare connected nodes:
			GSDSplineN n1,n2,n3,n4;
			n1 = tList[0];
			n2 = tList[1];
			n3 = tList[2];
			n4 = null;
			if(tList.Count > 3){ n4 = tList[3]; }
			
			//Determine most relevant spline:
			GSDSplineC n1Spline = n1.GSDSpline;
			GSDSplineC n2Spline = n2.GSDSpline;
			GSDSplineC n3Spline = n3.GSDSpline;
			GSDSplineC n4Spline = null;
			if(n4 != null){ n4Spline = n4.GSDSpline; }
			
			//Get the point:
			Vector3 n1Point = GetFourCornerPoint(ref n1Spline,ref n1,GSDRI);
			Vector3 n2Point = GetFourCornerPoint(ref n2Spline,ref n2,GSDRI);
			Vector3 n3Point = GetFourCornerPoint(ref n3Spline,ref n3,GSDRI);
			Vector3 n4Point = new Vector3(0f,0f,0f);
			if(n4Spline != null){ n4Point = GetFourCornerPoint(ref n4Spline,ref n4,GSDRI); }
			
			//2. If 3 way, we know that 2 of the nodes have the same spline.
			if(1==1 && GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay){
				//Should be size 3:
				if(tList.Count != 3){
					Debug.LogError("InitializeIntersections hashset != 3 connected on three way intersection, real size: " + tList.Count + " on intersection: " + GSDRI.transform.name);
					return;
				}
				
				ProcessFourCorners(ref n1Point,ref n2Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n1Spline,n2Spline));
				ProcessFourCorners(ref n1Point,ref n3Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n1Spline,n3Spline));
				ProcessFourCorners(ref n2Point,ref n3Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n2Spline,n3Spline));

			}else if(GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.FourWay){
				//Should be size 3:
				if(tList.Count != 4){
					Debug.LogError("InitializeIntersections hashset != 4 connected on four way intersection, real size: " + tList.Count + " on intersection: " + GSDRI.transform.name);
					return;
				}
				
				ProcessFourCorners(ref n1Point,ref n2Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n1Spline,n2Spline));
				ProcessFourCorners(ref n1Point,ref n3Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n1Spline,n3Spline));
				ProcessFourCorners(ref n1Point,ref n4Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n1Spline,n4Spline));
				ProcessFourCorners(ref n2Point,ref n3Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n2Spline,n3Spline));
				ProcessFourCorners(ref n2Point,ref n4Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n2Spline,n4Spline));
				ProcessFourCorners(ref n3Point,ref n4Point,GSDRI.transform.gameObject, GetLongestSplineDistance(n3Spline,n4Spline));
			}
		}
		#endregion
		
		#region "Intersection creation helpers"
		private static float GetLongestSplineDistance(GSDSplineC s1, GSDSplineC s2){
			if(s1.distance > s2.distance){
				return s1.distance;	
			}else{
				return s2.distance;
			}
		}
		
		private static GSDSplineN GetNodeByID(int i){
			Object[] SplineNodeObjects = GameObject.FindObjectsOfType(typeof(GSDSplineN));
			foreach(GSDSplineN tNode in SplineNodeObjects){
				if(tNode.id == i){
					return tNode;	
				}
			}
			return null;
		}
		
		static Vector3 GetFourCornerPoint(ref GSDSplineC tSpline, ref GSDSplineN tNode, GSDRoadIntersection GSDRI){
			GSDSplineN iNode;
			if(tNode.node_connected.Contains(GSDRI.Node1)){
				iNode = GSDRI.Node1;	
			}else{
				iNode = GSDRI.Node2;
			}
			
			float Pos1 = tNode.tTime;
			float iPos = iNode.tTime;
			
			float tFloat = 0;
			float NewSplinePos = 0;
			if(iPos >= Pos1){
				tFloat = iPos - Pos1;
				tFloat = tFloat / 8;
				NewSplinePos = iPos - tFloat;
			}else{
				tFloat = Pos1 - iPos;
				tFloat = tFloat / 8;
				NewSplinePos = iPos + tFloat;
			}
			
			Vector3 tVect = new Vector3(0,0,0); 
			
			bool bDone = false;
			int spamguard = 0;
			float tDist = 0f;
			while(!bDone && spamguard < 20000){
				spamguard+=1;
				tVect = tSpline.GetSplineValue(NewSplinePos);
				tDist = Vector3.Distance(tVect,iNode.transform.position);
	
				if(tDist > 22f){
					//Get closer to intersection:
					if(iPos >= NewSplinePos){
						NewSplinePos += 0.001f;	
					}else{
						NewSplinePos -= 0.001f;
					}
				}else if(tDist < 20f){
					//Move away from intersection:
					if(iPos >= NewSplinePos){
						NewSplinePos -= 0.001f;	
					}else{
						NewSplinePos += 0.001f;
					}
				}else{
					bDone = true;	
				}
			}
			return tVect;
		}
		
		static void ProcessFourCorners(ref Vector3 n1,ref Vector3 n2,GameObject tIntersectionObject, float SplineDistance){
			float Side1,Side2,Side3;
			Side1 = Vector3.Distance(n1,n2);
			Side2 = Vector3.Distance(tIntersectionObject.transform.position,n1);
			Side3 = Vector3.Distance(tIntersectionObject.transform.position,n2);
			float tAngle = AngleOfTriangle(Side2,Side3,Side1);
			if(tAngle > 20f && tAngle < 140f){
				ProcessTwoCorners(ref tIntersectionObject,ref n1,ref n2,SplineDistance);
			}
		}
		
		static float AngleOfTriangle(float a, float b, float c){
		    float cAng = (a*a+b*b- c*c)/(2*a*b);
		    float rad = Mathf.Acos(cAng);
			float tFloat = Mathf.Rad2Deg * rad;
		    return tFloat;
	    }
	
		static void ProcessTwoCorners(ref GameObject tIntersectionObject, ref Vector3 n1, ref Vector3 n2, float SplineDistance){
			GameObject tCorner = GameObject.CreatePrimitive(PrimitiveType.Cube);
			tCorner.transform.localScale = new Vector3(0.5f,20f,0.5f);
			tCorner.name = "CornerPosition";
			tCorner.transform.parent = tIntersectionObject.transform;
			tCorner.layer = 0;
			
			//Calculate the angle:
			Vector3 v3BA = n2 - tIntersectionObject.transform.position;
			Vector3 v3BC = n1 - tIntersectionObject.transform.position;
			Vector3 axis = Vector3.Cross(v3BA, v3BC);
			float angle = Vector3.Angle(v3BA, v3BC);
			Vector3 v3 = Quaternion.AngleAxis(angle / 2.0f, axis) * v3BA;
			//Vector3 v3 = (((n2.transform.position + n1.transform.position)/2) - tIntersectionObject.transform.position);
			
			tCorner.transform.rotation = Quaternion.LookRotation(v3);
			
			float tStep = 1.25f / SplineDistance;
			bool bSuccess = MoveCorner(tStep,3f,ref tCorner,ref tIntersectionObject, v3);
			if(!bSuccess){
				Debug.Log ("not success");
				Object.DestroyImmediate(tCorner);	
			}
		}
		
		static bool MoveCorner(float tStep, float tDist,ref GameObject tCorner,ref GameObject tIntersectionObject, Vector3 v3){
			float tStart = 0.05f;
			bool bDone = false;
			int spamguard = 0;
			bool bHitRoad = false;
			Collider[] tCollider;
			while(!bDone){
				spamguard +=1;
				tCorner.transform.position = tIntersectionObject.transform.position + (v3 * tStart);
				
				if(Vector3.Distance(tCorner.transform.position,tIntersectionObject.transform.position) > 25f){
					Debug.Log ("too long");
					bDone = true;	
					return false;
				}
				if(spamguard > 80000){
					Debug.Log ("spamguard");
					bDone = true;
					return false;
				}
				
				//Cast sphere now
				bHitRoad = false;
				tCollider = Physics.OverlapSphere(tCorner.transform.position,tDist);
				if(tCollider == null || tCollider.Length < 1){
					tStart += tStep;
					continue;
				}else{
					for(int k=0;k<tCollider.Length;k++){
						if(tCollider[k].transform.name.ToLower().Contains("road")){
							bHitRoad = true;
							break;
						}
					}
				}
				
				if(bHitRoad){
					tStart += tStep;
					continue;
				}else{
					//Debug.Log ("Distance: " + Vector3.Distance(tCorner.transform.position,tIntersectionObject.transform.position));
					bDone = true;
					break;
				}
			}
			return true;
		}
		#endregion
		
		#endregion
	}
	
	public static class GSDNavigation{
		public static void UpdateConnectedNodes(){
			Init_ResetNavigationData();
			
			Object[] SplineObjects = GameObject.FindObjectsOfType(typeof(GSDSplineC));
			
			//Store connected spline nodes on each other:
			GSDSplineN xNode;
			foreach (GSDSplineC tSpline in SplineObjects) {
				int mNodeCount = tSpline.mNodes.Count;
				for(int i=0;i<mNodeCount;i++){
					xNode = tSpline.mNodes[i];
					//Add next node if not last node:
					if((i+1) < mNodeCount){
						xNode.id_connected.Add(tSpline.mNodes[i+1].id);
						xNode.node_connected.Add(tSpline.mNodes[i+1]);
					}
					//Add prev node if not first node:
					if(i > 0){
						xNode.id_connected.Add(tSpline.mNodes[i-1].id);	
						xNode.node_connected.Add(tSpline.mNodes[i-1]);
					}
				}
			} 
		}
		
		public static void Init_ResetNavigationData(){
			Object[] tSplines = GameObject.FindObjectsOfType(typeof(GSDSplineC));
			int SplineCount=0;
			int NodeCount=0;
			foreach(GSDSplineC tSpline in tSplines){
				SplineCount+=1;
				foreach(GSDSplineN tNode in tSpline.mNodes){
					NodeCount+=1;
					tNode.ResetNavigationData();
				}
				tSpline.ResetNavigationData();
			}
		}
	}
	
	#endif
}