using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using GSD;
#endif
namespace GSD.Threaded{
	#if UNITY_EDITOR
	
	public static class GSDTerraformingT{
		public class TerrainBoundsMaker{
			public List<GSD.Roads.GSDRoadUtil.Construction3DTri> triList;
			public GSD.Roads.GSDRoadUtil.Construction2DRect tRect;
			public float MinI = 0f;
			public float MaxI = 1f;
		}
		
//		static Vector3 ProcessLineHeights_PrevVect = new Vector3(0f,0f,0f);
//		public static float ProcessLineHeights(GSDSplineC tSpline, ref Vector3 tVect, ref Vector3 POS, float tDistance, GSD.Roads.GSDTerraforming.TempTerrainData TTD, float PrevDesiredHeight){	
//			Vector3 ShoulderR_rVect = new Vector3(0f,0f,0f);
//			Vector3 ShoulderL_lVect = new Vector3(0f,0f,0f);
//
//			float DesiredHeight = ProcessLineHeights_GetDesiredHeight(tVect,ref TTD, ref tSpline);
//			float nResult = 0f;
//			bool bIntersection = tSpline.IsNearIntersection(ref tVect,ref nResult);
//			if(bIntersection){
//				if(nResult < tVect.y){
//					tVect.y = nResult;
//					DesiredHeight = ProcessLineHeights_GetDesiredHeight(tVect,ref TTD, ref tSpline);	
//				}
//			}
//
//			int x1 = 0,y1 = 0;
//			GetTempHeights_Coordinates(ref tVect,ref TTD,out x1,out y1);
//				
//			bool bOverride = false;
//			int StepMod = (int)(1 / TTD.HMRatio);
//			for(float i=tDistance;i>=1f;i-=StepMod){
//				ShoulderR_rVect = (tVect + new Vector3(i*POS.normalized.z,0,i*-POS.normalized.x));
//				GetTempHeights_Coordinates(ref ShoulderR_rVect,ref TTD,out x1,out y1);
//				if(TTD.heights[x1,y1] > DesiredHeight){
//					bOverride = true;	
//				}
//				if(bOverride || !TTD.tHeights[x1,y1]){
//					TTD.tHeights[x1,y1] = true;
//					TTD.cX[TTD.cI] = x1;
//					TTD.cY[TTD.cI] = y1;
//					TTD.cH[TTD.cI] = DesiredHeight;
//					TTD.oldH[TTD.cI] = TTD.heights[x1,y1];
//					TTD.cI += 1;
//				}
//				bOverride = false;
//	
//				ShoulderL_lVect = (tVect + new Vector3(i*-POS.normalized.z,0,i*POS.normalized.x));
//				GetTempHeights_Coordinates(ref ShoulderL_lVect,ref TTD,out x1,out y1);
//				if(TTD.heights[x1,y1] > DesiredHeight){
//					bOverride = true;	
//				}
//				if(bOverride || !TTD.tHeights[x1,y1]){
//					TTD.tHeights[x1,y1] = true;
//					TTD.cX[TTD.cI] = x1;
//					TTD.cY[TTD.cI] = y1;
//					TTD.cH[TTD.cI] = DesiredHeight;
//					TTD.oldH[TTD.cI] = TTD.heights[x1,y1];
//					TTD.cI += 1;
//				}
//				bOverride=false;
//			}
//
//			GetTempHeights_Coordinates(ref tVect,ref TTD,out x1,out y1);
//			if(TTD.heights[x1,y1] > DesiredHeight || (tVect.y < ProcessLineHeights_PrevVect.y)){
//				bOverride = true;	
//			}
//			if(bOverride || !TTD.tHeights[x1,y1]){
//				TTD.tHeights[x1,y1] = true;
//				TTD.cX[TTD.cI] = x1;
//				TTD.cY[TTD.cI] = y1;
//				if(tDistance > 15f && TTD.HMRatio > 0.24f){
//					TTD.cH[TTD.cI] = DesiredHeight-0.0002f;
//				}else{
//					TTD.cH[TTD.cI] = DesiredHeight;	
//				}
//				TTD.oldH[TTD.cI] = TTD.heights[x1,y1];
//				TTD.cI += 1;
//			}
//				
//			ProcessLineHeights_PrevVect = tVect;
//			return DesiredHeight;
//		}
//
//		private static float ProcessLineHeights_GetDesiredHeight(Vector3 tVect, ref GSD.Roads.GSDTerraforming.TempTerrainData TTD, ref GSDSplineC tSpline){
//			return ((((tVect - TTD.TerrainPos).y)-tSpline.tRoad.opt_TerrainSubtract_Alt) / TTD.TerrainSize.y);
//		}
		
		private static void GetTempHeights_Coordinates(ref Vector3 tVect,ref GSD.Roads.GSDTerraforming.TempTerrainData TTD, out int x, out int y){
			//Get the normalized position of this game object relative to the terrain:
			Vector3 tempCoord = (tVect - TTD.TerrainPos);
			
			Vector3 coord;
			coord.x = tempCoord.x / TTD.TerrainSize.x;
			coord.y = tempCoord.y / TTD.TerrainSize.y;
			coord.z = tempCoord.z / TTD.TerrainSize.z;
				
			//Get the position of the terrain heightmap where this game object is:
			y = (int) (coord.x * TTD.HM);
			x = (int) (coord.z * TTD.HM);
		}
		
		private static void GetTempDetails_Coordinates(ref Vector3 tVect,ref GSD.Roads.GSDTerraforming.TempTerrainData TTD, out int x, out int y){
			//Get the normalized position of this game object relative to the terrain:
			Vector3 tempCoord = (tVect - TTD.TerrainPos);
			
			Vector3 coord;
			coord.x = tempCoord.x / TTD.TerrainSize.x;
			coord.y = tempCoord.y / TTD.TerrainSize.y;
			coord.z = tempCoord.z / TTD.TerrainSize.z;
				
			//Get the position of the terrain heightmap where this game object is:
			y = (int) (coord.x * TTD.DetailMaxIndex);
			x = (int) (coord.z * TTD.DetailMaxIndex);
		}
		
		//Privatized for obfuscate:
		public static void DoRects(GSDSplineC tSpline, GSD.Roads.GSDTerraforming.TempTerrainData TTD){
			DoRectsDo(ref tSpline, ref TTD);
		}
		
		private static void DoRectsDo(ref GSDSplineC tSpline,ref GSD.Roads.GSDTerraforming.TempTerrainData TTD){
			float Sep = tSpline.tRoad.RoadWidth()*0.5f;
			float HeightSep = Sep + (tSpline.tRoad.opt_MatchHeightsDistance * 0.5f);
			List<TerrainBoundsMaker> TBMList = new List<TerrainBoundsMaker>();
//			List<GSD.Roads.GSDRoadUtil.Construction3DTri> triList = new List<GSD.Roads.GSDRoadUtil.Construction3DTri>();
			List<GSD.Roads.GSDRoadUtil.Construction2DRect> TreerectList = new List<GSD.Roads.GSDRoadUtil.Construction2DRect>();
			float tStep = tSpline.tRoad.opt_RoadDefinition / tSpline.distance;
//			tStep *= 0.5f;
			
			//Start initializing the loop. Convuluted to handle special control nodes, so roads don't get rendered where they aren't supposed to, while still preserving the proper curvature.
			float FinalMax = 1f;
			float StartMin = 0f;
			if(tSpline.bSpecialEndControlNode){	//If control node, start after the control node:
				FinalMax = tSpline.mNodes[tSpline.GetNodeCount()-2].tTime;
			}
			if(tSpline.bSpecialStartControlNode){	//If ends in control node, end construction before the control node:
				StartMin = tSpline.mNodes[1].tTime;
			}
//			bool bFinalEnd = false;
//			float RoadConnection_StartMin1 = StartMin;	//Storage of incremental start values for the road connection mesh construction at the end of this function.
//			float RoadConnection_FinalMax1 = FinalMax; 	//Storage of incremental end values for the road connection mesh construction at the end of this function.
			if(tSpline.bSpecialEndNode_IsStart_Delay){
				StartMin += (tSpline.SpecialEndNodeDelay_Start / tSpline.distance);	//If there's a start delay (in meters), delay the start of road construction: Due to special control nodes for road connections or 3 way intersections.
			}else if(tSpline.bSpecialEndNode_IsEnd_Delay){
				FinalMax -= (tSpline.SpecialEndNodeDelay_End / tSpline.distance);	//If there's a end delay (in meters), cut early the end of road construction: Due to special control nodes for road connections or 3 way intersections.
			}
//			float RoadConnection_StartMin2 = StartMin;	//Storage of incremental start values for the road connection mesh construction at the end of this function.
//			float RoadConnection_FinalMax2 = FinalMax; 	//Storage of incremental end values for the road connection mesh construction at the end of this function.
			
			FinalMax = FinalMax + tStep;
			
			Vector3 tVect1 = default(Vector3);
			Vector3 tVect2 = default(Vector3);
			Vector3 POS1 = default(Vector3);
			Vector3 POS2 = default(Vector3);
			if(FinalMax > 1f){ FinalMax = 1f; }

			float tNext = 0f;
			float fValue1,fValue2;
			float fValueMod = tSpline.tRoad.opt_RoadDefinition / tSpline.distance;
			bool bIsPastInter = false;
			float tIntStrength = 0f;
			float tIntStrength2 = 0f;
//			bool bMaxIntersection = false;
//			bool bFirstInterNode = false;
			GSDSplineN xNode = null;
			float tIntHeight = 0f;
			float tIntHeight2 = 0f;
			GSDRoadIntersection GSDRI = null;
			float T1SUB = 0f;
			float T2SUB = 0f;
			bool bIntStr1_Full = false;
			bool bIntStr1_FullPrev = false;
			bool bIntStr1_FullNext = false;
			bool bIntStr2_Full = false;
			bool bIntStr2_FullPrev = false;
			bool bIntStr2_FullNext = false;
			Vector3 tVect3 = default(Vector3);
//			bool bStarted = false;
//			bool T3Added = false;
			List<int[]> tXYs = new List<int[]>();
			float TreeClearDist = tSpline.tRoad.opt_ClearTreesDistance;
			if(TreeClearDist < tSpline.tRoad.RoadWidth()){ TreeClearDist = tSpline.tRoad.RoadWidth(); }
			GSD.Roads.GSDRoadUtil.Construction2DRect tRect = null;
			float tGrade = 0f;
			for(float i=StartMin;i<FinalMax;i+=tStep){
				if(tSpline.tRoad.opt_HeightModEnabled){
					if(i > 1f){ break; }
					tNext = i+tStep;
					if(tNext > 1f){ break; }
	
					tSpline.GetSplineValue_Both(i,out tVect1,out POS1);
	
					if(tNext <= 1f){
						tSpline.GetSplineValue_Both(tNext,out tVect2,out POS2);
					}else{
						tSpline.GetSplineValue_Both(1f,out tVect2,out POS2);
					}
					
					//Determine if intersection:
					bIsPastInter = false;	//If past intersection
					tIntStrength = tSpline.IntersectionStrength(ref tVect1,ref tIntHeight, ref GSDRI, ref bIsPastInter, ref i, ref xNode);
//					if(IsApproximately(tIntStrength,1f,0.001f) || tIntStrength > 1f){
//						bMaxIntersection = true;
//					}else{
//						bMaxIntersection = false;	
//					}	
//					bFirstInterNode = false;	
					
					tIntStrength2 = tSpline.IntersectionStrength(ref tVect2,ref tIntHeight2, ref GSDRI, ref bIsPastInter, ref i, ref xNode);
					if(tIntStrength2 > 1f){ tIntStrength2 = 1f; }
					
					T1SUB = tVect1.y;
					T2SUB = tVect2.y;
					
					if(tIntStrength > 1f){ tIntStrength = 1f; }
					if(tIntStrength >= 0f){// || IsApproximately(tIntStrength,0f,0.01f)){
						if(IsApproximately(tIntStrength,1f,0.01f)){ 
							T1SUB = tIntHeight;
							bIntStr1_Full = true; 
							bIntStr1_FullNext = false;
						}else{
						 	bIntStr1_Full = false;
							bIntStr1_FullNext = (tIntStrength2 >= 1f);
							if(!IsApproximately(tIntStrength,0f,0.01f)){ T1SUB = (tIntStrength*tIntHeight) + ((1-tIntStrength)*tVect1.y); }
	//						if(tIntStrength <= 0f){ T1SUB = (tIntStrength*tIntHeight) + ((1-tIntStrength)*tVect1.y); }
						}
	
						if((bIntStr1_Full && !bIntStr1_FullPrev) || (!bIntStr1_Full && bIntStr1_FullNext)){
							tGrade = tSpline.GetCurrentNode(i).GradeToPrevValue;
							if(tGrade < 0f){
								T1SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,(tGrade/20f)*-1f);
							}else{
								T1SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,tGrade/20f);
							}
							
//							if(tGrade < 0f){
//								T1SUB *= -1f;
//							}
						}else if(bIntStr1_Full && !bIntStr1_FullNext){
							tGrade = tSpline.GetCurrentNode(i).GradeToNextValue;
							if(tGrade < 0f){
								T1SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,(tGrade/20f)*-1f);
							}else{
								T1SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,tGrade/20f);
							}
//							if(tGrade < 0f){
//								T1SUB *= -1f;
//							}
						}else{
							T1SUB -= 0.02f;
						}
						bIntStr1_FullPrev = bIntStr1_Full;
					}
	
					if(tIntStrength2 >= 0f || IsApproximately(tIntStrength2,0f,0.01f)){
	//					if(!IsApproximately(tIntStrength,1f,0.01f)){ 
						if(IsApproximately(tIntStrength,1f,0.01f)){ 
							bIntStr2_Full = true; 
							T2SUB = tIntHeight2;
						}else{
						 	bIntStr2_Full = false;
							if(!IsApproximately(tIntStrength2,0f,0.01f)){ T2SUB = (tIntStrength2*tIntHeight) + ((1-tIntStrength2)*tVect2.y); }
	//						if(tIntStrength2 <= 0f){ T2SUB = (tIntStrength2*tIntHeight) + ((1-tIntStrength2)*tVect2.y); }
						}
	
						if((bIntStr2_Full && !bIntStr2_FullPrev)){
							tGrade = tSpline.GetCurrentNode(i).GradeToPrevValue;
							if(tGrade < 0f){
								T2SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,(tGrade/20f)*-1f);
							}else{
								T2SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,tGrade/20f);
							}
//							T2SUB -= tIntHeight2 - tVect2.y;
						}else if(bIntStr2_Full && !bIntStr2_FullNext){
							tGrade = tSpline.GetCurrentNode(i).GradeToNextValue;
							if(tGrade < 0f){
								T2SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,(tGrade/20f)*-1f);
							}else{
								T2SUB -= Mathf.Lerp(0.02f,GSDRI.GradeMod,tGrade/20f);
							}
//							if(tGrade < 0f){
//								T2SUB *= -1f;
//							}
//							T2SUB -= tIntHeight2 - tVect2.y;
						}else if(!bIntStr2_Full){
							if(tNext+tStep < 1f){
								tVect3 = tSpline.GetSplineValue(tNext+tStep,false);
								tIntStrength2 = tSpline.IntersectionStrength(ref tVect3,ref tIntHeight2, ref GSDRI, ref bIsPastInter, ref i, ref xNode);
							}else{
								tIntStrength2 = 0f;
							}
							
							if(tIntStrength2 >= 1f){
								T2SUB -= 0.06f;
							}else{
								T2SUB -= 0.02f;
							}
						}else{
							T2SUB -= 0.02f;	
						}
						bIntStr2_FullPrev = bIntStr2_Full;
					}
	
					fValue1=i - fValueMod;
					fValue2=i + fValueMod;
					if(fValue1 < 0){ fValue1 = 0; }
					if(fValue2 > 1){ fValue2 = 1; }

					tXYs.Add(CreateTris(fValue1,fValue2,ref tVect1,ref POS1,ref tVect2,ref POS2,Sep,ref TBMList,ref T1SUB, ref T2SUB, ref TTD, HeightSep));
					
					//Details and trees:
					tRect = SetDetailCoords(i,ref tVect1,ref POS1,ref tVect2, ref POS2,tSpline.tRoad.opt_ClearDetailsDistance,TreeClearDist, ref TTD, ref tSpline);
					if(tSpline.tRoad.opt_TreeModEnabled && tRect != null){
						TreerectList.Add(tRect);
					}
				}else{
					if(i > 1f){ break; }
					tNext = i+tStep;
					if(tNext > 1f){ break; }
	
					tSpline.GetSplineValue_Both(i,out tVect1,out POS1);
	
					if(tNext <= 1f){
						tSpline.GetSplineValue_Both(tNext,out tVect2,out POS2);
					}else{
						tSpline.GetSplineValue_Both(1f,out tVect2,out POS2);
					}
					
					//Details and trees:
					tRect = SetDetailCoords(i,ref tVect1,ref POS1,ref tVect2, ref POS2,tSpline.tRoad.opt_ClearDetailsDistance,TreeClearDist, ref TTD, ref tSpline);
					if(tSpline.tRoad.opt_TreeModEnabled && tRect != null){
						TreerectList.Add(tRect);
					}
				}
			}
			
			if(tSpline.tRoad.bProfiling){
                UnityEngine.Profiling.Profiler.BeginSample("DoRectsTree");	
			}
			if(tSpline.tRoad.opt_TreeModEnabled && TreerectList != null && TreerectList.Count > 0){
				int tCount = TTD.TreeSize;
				int jCount = TreerectList.Count;
				Vector3 tVect3D = default(Vector3);
				Vector2 tVect2D = default(Vector2);
				TreeInstance tTree;
				for(int i=0;i<tCount;i++){
					tTree = TTD.TreesCurrent[i];
					
					tVect3D = tTree.position;
					tVect3D.x *= TTD.TerrainSize.z;
					tVect3D.y *= TTD.TerrainSize.y;
					tVect3D.z *= TTD.TerrainSize.x;
					tVect3D += TTD.TerrainPos;
					tVect2D.x = tVect3D.x;
					tVect2D.y = tVect3D.z;

					for(int j=0;j<jCount;j++){
						if(TreerectList[j].Contains(ref tVect2D)){
							TTD.TreesOld.Add(TTD.TreesCurrent[i]);
							tTree = TTD.TreesCurrent[i];
							tTree.prototypeIndex = -2;
							TTD.TreesCurrent[i] = tTree;
							TTD.TreesI+=1;
							break;
						}
					}
				}
				TTD.TreesCurrent.RemoveAll(item => item.prototypeIndex < -1);
			}
			if(tSpline.tRoad.bProfiling){
                UnityEngine.Profiling.Profiler.EndSample();	
			}
			
			if(!tSpline.tRoad.opt_HeightModEnabled){
				return;	
			}

//			//Temp testing:
//			tSpline.mNodes[22].tTriList = new List<GSD.Roads.GSDRoadUtil.Construction3DTri>();
//			int tCount = triList.Count;
//			for(int i=0;i<tCount;i++){
//				tSpline.mNodes[22].tTriList.Add(triList[i]);	
//			}
//			tSpline.mNodes[22].tHMList = new List<Vector3>();
			

			float tFloat = -1f;
			Sep = tSpline.tRoad.RoadWidth()*1.5f;
			int k = 0;
			int[] tXY = null;
			int tXYsCount = tXYs.Count;
			bool bIsBridge = false;
			bool bIsTunnel = false;
			for(float i=StartMin;i<FinalMax;i+=tStep){
				if(TBMList.Count > 0){
					if(TBMList[0].MaxI < i){
						CleanupTris(i,ref TBMList);
					}
				}else{
					break;	
				}
				
				//If in bridg mode:
				if(tSpline.IsInBridgeTerrain(i)){
					bIsBridge = true;
				}else{
					bIsBridge = false;	
				}
				//If in tunnel mode:
				if(tSpline.IsInTunnelTerrain(i)){
					bIsTunnel = true;	
				}else{
					bIsTunnel = false;	
				}
				
				if(k < tXYsCount){
					tXY = tXYs[k];
					tFloat = ProcessCoordinateGrabber(ref i,ref tSpline,ref TTD, ref TBMList,ref tXY, bIsBridge, bIsTunnel);
					if(!IsApproximately(tFloat,0f,0.0001f)){
						tSpline.HeightHistory.Add(new KeyValuePair<float,float>(i,tFloat));
					}
				}else{
					break;	
				}
				k+=1;
			}
		}
		
		private static void CleanupTris(float CurrentI, ref List<TerrainBoundsMaker> tList){
			int mCount = tList.Count;
			int LastIndexToRemove = -1;
			for(int i=0;i<mCount;i++){
				if(tList[i].MaxI < CurrentI){
					LastIndexToRemove = i;
				}else{
					break;
				}
			}
			if(LastIndexToRemove >= 0){
				tList.RemoveRange(0,LastIndexToRemove);
			}
//			
//			mCount = rectList.Count;
//			LastIndexToRemove = -1;
//			for(int i=0;i<mCount;i++){
//				if(tList[i].MaxI < CurrentI){
//					LastIndexToRemove = i;
//				}else{
//					break;
//				}
//			}
//			if(LastIndexToRemove >= 0){
//				rectList.RemoveRange(0,LastIndexToRemove);
//			}
		}
		
		private static int[] CreateTris(float i, float i2,ref Vector3 tVect1,ref Vector3 POS1,ref Vector3 tVect2,ref Vector3 POS2, float Sep, ref List<TerrainBoundsMaker> tList, ref float T1SUB, ref float T2SUB,ref GSD.Roads.GSDTerraforming.TempTerrainData TTD, float HeightSep){
			Vector3 lVect1 = (tVect1 + new Vector3(Sep*-POS1.normalized.z,0,Sep*POS1.normalized.x));
			Vector3 rVect1 = (tVect1 + new Vector3(Sep*POS1.normalized.z,0,Sep*-POS1.normalized.x));
			Vector3 lVect2 = (tVect2 + new Vector3(Sep*-POS2.normalized.z,0,Sep*POS2.normalized.x));
			Vector3 rVect2 = (tVect2 + new Vector3(Sep*POS2.normalized.z,0,Sep*-POS2.normalized.x));
			
			lVect1.y = T1SUB;
			rVect1.y = T1SUB;
			lVect2.y = T2SUB;
			rVect2.y = T2SUB;
			
			TerrainBoundsMaker TBM = new TerrainBoundsMaker();
			TBM.triList = new List<GSD.Roads.GSDRoadUtil.Construction3DTri>();
			
			TBM.triList.Add(new GSD.Roads.GSDRoadUtil.Construction3DTri(lVect1,rVect1,lVect2,i,i2));
			TBM.triList.Add(new GSD.Roads.GSDRoadUtil.Construction3DTri(lVect2,rVect1,rVect2,i,i2));

			Vector3 lVect1far = (tVect1 + new Vector3(HeightSep*-POS1.normalized.z,0,HeightSep*POS1.normalized.x));
			Vector3 rVect1far = (tVect1 + new Vector3(HeightSep*POS1.normalized.z,0,HeightSep*-POS1.normalized.x));
			Vector3 lVect2far = (tVect2 + new Vector3(HeightSep*-POS2.normalized.z,0,HeightSep*POS2.normalized.x));
			Vector3 rVect2far = (tVect2 + new Vector3(HeightSep*POS2.normalized.z,0,HeightSep*-POS2.normalized.x));
			
			lVect1far.y = lVect1.y;
			lVect2far.y = lVect2.y;
			rVect1far.y = rVect1.y;
			rVect2far.y = rVect2.y;

			TBM.triList.Add(new GSD.Roads.GSDRoadUtil.Construction3DTri(lVect1far,lVect1,lVect2far,i,i2));
			TBM.triList.Add(new GSD.Roads.GSDRoadUtil.Construction3DTri(lVect2far,lVect1,lVect2,i,i2));
			TBM.triList.Add(new GSD.Roads.GSDRoadUtil.Construction3DTri(rVect1,rVect1far,rVect2,i,i2));
			TBM.triList.Add(new GSD.Roads.GSDRoadUtil.Construction3DTri(rVect2,rVect1far,rVect2far,i,i2));

			TBM.tRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(new Vector2(lVect1far.x,lVect1far.z),new Vector2(rVect1far.x,rVect1far.z),new Vector2(rVect2far.x,rVect2far.z),new Vector2(lVect2far.x,lVect2far.z),0f);
//			tRect.MinI = i;
//			tRect.MaxI = i2;
			
			TBM.MinI = i;
			TBM.MaxI = i2;
			
			tList.Add(TBM);
				
			int[] Xs = new int[4];
			int[] Ys = new int[4];
			
			int x1,y1;
			GetTempHeights_Coordinates(ref lVect1far,ref TTD,out x1, out y1);
			Xs[0] = x1;
			Ys[0] = y1;
			GetTempHeights_Coordinates(ref lVect2far,ref TTD,out x1, out y1);
			Xs[1] = x1;
			Ys[1] = y1;
			GetTempHeights_Coordinates(ref rVect1far,ref TTD,out x1, out y1);
			Xs[2] = x1;
			Ys[2] = y1;
			GetTempHeights_Coordinates(ref rVect2far,ref TTD,out x1, out y1);
			Xs[3] = x1;
			Ys[3] = y1;
		
			int Min = Mathf.Min(Xs);
			int Max = Mathf.Max(Xs);
			Xs[0] = Min-2;
			Xs[2] = Max+2;
			Min = Mathf.Min(Ys);
			Max = Mathf.Max(Ys);
			Xs[1] = Min-2;
			Xs[3] = Max+2;

			return Xs;
		}
		
		private static GSD.Roads.GSDRoadUtil.Construction2DRect SetDetailCoords(float param,ref Vector3 tVect1,ref Vector3 POS1,ref Vector3 tVect2,ref Vector3 POS2, float Sep, float TreeSep, ref GSD.Roads.GSDTerraforming.TempTerrainData TTD, ref GSDSplineC tSpline){
			Vector3 lVect1far = default(Vector3);
			Vector3 rVect1far = default(Vector3);
			Vector3 lVect2far = default(Vector3);
			Vector3 rVect2far = default(Vector3);
			
			bool bIsInBridge = tSpline.IsInBridgeTerrain(param);
			bool bIsInTunnel = tSpline.IsInTunnelTerrain(param);
			int x2,y2,x3,y3;
			GetTempHeights_Coordinates(ref tVect1,ref TTD,out x2,out y2);
			if(x2 >= TTD.HM){ x2=-1; }
			if(y2 >= TTD.HM){ y2=-1; }
			if(x2 < 0){ x2=-1; }
			if(y2 < 0){ y2=-1; }
			if(x2 == -1){ return null; }
			if(y2 == -1){ return null; }
			
			float tDiff1 = ((TTD.heights[x2,y2]*(float)TTD.TerrainSize.y) - tVect1.y);
			GetTempHeights_Coordinates(ref tVect2,ref TTD,out x3,out y3);
			if(x3 >= TTD.HM){ x3=-1; } 
			if(y3 >= TTD.HM){ y3=-1; }
			if(x3 < 0){ x3=-1; }
			if(y3 < 0){ y3=-1; }
			if(x3 == -1){ return null; }
			if(y3 == -1){ return null; }
			float tDiff2 = ((TTD.heights[x3,y3]*(float)TTD.TerrainSize.y) - tVect2.y);
			

			
			GSD.Roads.GSDRoadUtil.Construction2DRect tRect = null;
			if(tSpline.tRoad.opt_TreeModEnabled){
				bool bQuit = false;
				if(x2 == -1){ bQuit = true; }
				if(y2 == -1){ bQuit = true; }
				
				if(bIsInBridge && !bQuit){
					if(tDiff1 < 0f){ tDiff1 *= -1f; }
					if(tDiff2 < 0f){ tDiff2 *= -1f; }
					if(tDiff1 > tSpline.tRoad.opt_ClearTreesDistanceHeight){
						bQuit = true;
					}	
					if(tDiff2 > tSpline.tRoad.opt_ClearTreesDistanceHeight){
						bQuit = true;
					}
				}
				if(bIsInTunnel && !bQuit){
					if(tDiff1 < 0f){
						if((tDiff1*-1f) > tSpline.tRoad.opt_ClearTreesDistanceHeight){
							bQuit = true;
						}	
					}else{
						if(tDiff1 > (tSpline.tRoad.opt_ClearTreesDistanceHeight*0.1f)){
							bQuit = true;
						}	
					}
					if(tDiff2 < 0f){
						if((tDiff2*-1f) > tSpline.tRoad.opt_ClearTreesDistanceHeight){
							bQuit = true;
						}	
					}else{
						if(tDiff2 > (tSpline.tRoad.opt_ClearTreesDistanceHeight*0.1f)){
							bQuit = true;
						}	
					}	
				}

				if(!bQuit){
					TreeSep = TreeSep * 0.5f;
					lVect1far = (tVect1 + new Vector3(TreeSep*-POS1.normalized.z,0,TreeSep*POS1.normalized.x));
					rVect1far = (tVect1 + new Vector3(TreeSep*POS1.normalized.z,0,TreeSep*-POS1.normalized.x));
					lVect2far = (tVect2 + new Vector3(TreeSep*-POS2.normalized.z,0,TreeSep*POS2.normalized.x));
					rVect2far = (tVect2 + new Vector3(TreeSep*POS2.normalized.z,0,TreeSep*-POS2.normalized.x));
					tRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(new Vector2(lVect1far.x,lVect1far.z),new Vector2(rVect1far.x,rVect1far.z),new Vector2(rVect2far.x,rVect2far.z),new Vector2(lVect2far.x,lVect2far.z),0f);
				}
			}
			
			if(tSpline.tRoad.opt_DetailModEnabled){
				if(bIsInBridge || bIsInTunnel){
					if(tDiff1 < 0f){ tDiff1 *= -1f; }
					if(tDiff2 < 0f){ tDiff2 *= -1f; }
					
					bool bQuit = false;
					if(x2 == -1){ bQuit = true; }
					if(y2 == -1){ bQuit = true; }

					if(tDiff1 > tSpline.tRoad.opt_ClearDetailsDistanceHeight){
						bQuit = true;
					}
					if(tDiff2 > tSpline.tRoad.opt_ClearDetailsDistanceHeight){
						bQuit = true;
					}

					if(bQuit){
						return tRect;	
					}
				}
				
				Sep = Sep * 0.5f;
				
				lVect1far = (tVect1 + new Vector3(Sep*-POS1.normalized.z,0,Sep*POS1.normalized.x));
				rVect1far = (tVect1 + new Vector3(Sep*POS1.normalized.z,0,Sep*-POS1.normalized.x));
				lVect2far = (tVect2 + new Vector3(Sep*-POS2.normalized.z,0,Sep*POS2.normalized.x));
				rVect2far = (tVect2 + new Vector3(Sep*POS2.normalized.z,0,Sep*-POS2.normalized.x));
				
				int[] Xs = new int[4];
				int[] Ys = new int[4];
				
				int x1,y1;
				GetTempDetails_Coordinates(ref lVect1far,ref TTD,out x1, out y1);
				Xs[0] = x1;
				Ys[0] = y1;
				GetTempDetails_Coordinates(ref lVect2far,ref TTD,out x1, out y1);
				Xs[1] = x1;
				Ys[1] = y1;
				GetTempDetails_Coordinates(ref rVect1far,ref TTD,out x1, out y1);
				Xs[2] = x1;
				Ys[2] = y1;
				GetTempDetails_Coordinates(ref rVect2far,ref TTD,out x1, out y1);
				Xs[3] = x1;
				Ys[3] = y1;
//				
//				if(TTD.DetailLayersCount == 1 && x1 > 0 && y1 > 0){
//					Debug.Log(Xs[0]+","+Ys[0] + " " + Xs[1]+","+Ys[1]);
//				}
					
				int MinX = Mathf.Min(Xs);
				int MinY = Mathf.Min(Ys);
				int MaxX = Mathf.Max(Xs);
				int MaxY = Mathf.Max(Ys);
				
				if(MinX >= TTD.DetailMaxIndex){ MinX = TTD.DetailMaxIndex-1; }
				if(MinY >= TTD.DetailMaxIndex){ MinY = TTD.DetailMaxIndex-1; }
				if(MaxX >= TTD.DetailMaxIndex){ MaxX = TTD.DetailMaxIndex-1; }
				if(MaxY >= TTD.DetailMaxIndex){ MaxY = TTD.DetailMaxIndex-1; }
				
				if(MinX < 0){ MinX = 0; }
				if(MinY < 0){ MinY = 0; }
				if(MaxX < 0){ MaxX = 0; }
				if(MaxY < 0){ MaxY = 0; }

//				int DetailI = 0;
				if(tSpline.tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("Dorectsdetails"); }
				int tInt = 0;
				for(int i=MinX;i<=MaxX;i++){
					for(int k=MinY;k<=MaxY;k++){
						//Bitfield for identification:
						tInt = k; 
						tInt = tInt << 16;
       			 		tInt = tInt | (ushort)i;
						if(!TTD.DetailHasProcessed.Contains(tInt)){
//							for(int h=0;h<TTD.DetailLayersCount;h++){
//								if(TTD.DetailLayersSkip.Contains(h)){ continue; }
	//							if(!TTD.DetailHasProcessed[h][i,k]){// && TTD.DetailValues[h][i,k] > 0){
								
							TTD.MainDetailsX.Add((ushort)i);
							TTD.MainDetailsY.Add((ushort)k);
							
//								DetailI = TTD.DetailsI[h];
									
//								TTD.DetailsX[h].Add((ushort)i);
//								TTD.DetailsY[h].Add((ushort)k);
								
//								TTD.DetailsX[h][DetailI] = (ushort)i;	
//								TTD.DetailsY[h][DetailI] = (ushort)k;
//								TTD.OldDetailsValue[h][DetailI] = (ushort)TTD.DetailValues[h][i,k];
//								TTD.DetailValues[h][i,k] = 0;
								
//								TTD.DetailsI[h]+=1;
							
//							}
							TTD.DetailHasProcessed.Add(tInt);
						}
					}
				}
				if(tSpline.tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			}

			return tRect;
		}
		
		private static float ProcessCoordinateGrabber(ref float param, ref GSDSplineC tSpline, ref GSD.Roads.GSDTerraforming.TempTerrainData TTD, ref List<TerrainBoundsMaker> tList,ref int[] tXY, bool bIsBridge, bool bIsTunnel){	
			int MinX = tXY[0];
			int MinY = tXY[1];
			int MaxX = tXY[2];
			int MaxY = tXY[3];
			
			if(MinX >= TTD.TerrainMaxIndex){ MinX = TTD.TerrainMaxIndex-1; }
			if(MinY >= TTD.TerrainMaxIndex){ MinY = TTD.TerrainMaxIndex-1; }
			if(MaxX >= TTD.TerrainMaxIndex){ MaxX = TTD.TerrainMaxIndex-1; }
			if(MaxY >= TTD.TerrainMaxIndex){ MaxY = TTD.TerrainMaxIndex-1; }
			
			if(MinX < 0){ MinX = 0; }
			if(MinY < 0){ MinY = 0; }
			if(MaxX < 0){ MaxX = 0; }
			if(MaxY < 0){ MaxY = 0; }
			
			Vector3 xVect = default(Vector3);
			bool bAdjusted = false;
			float tHeight = -1f;
			float tReturnFloat = 0f;
//			int dX = 0;
//			int dY = 0;
//			int tdX = 0;
//			int tdY = 0;
//			bool bOneHit = false;
			
			for(int i=MinX;i<=MaxX;i++){
				for(int k=MinY;k<=MaxY;k++){
					if(TTD.tHeights[i,k] != true){
						if(TTD.cX.Length <= TTD.cI){ break; }
						
						xVect = ConvertTerrainCoordToWorldVect(i,k,TTD.heights[i,k],ref TTD);
						AdjustedTerrainVect_Tri(ref param, out bAdjusted,out tHeight,ref xVect,ref tList, bIsBridge, bIsTunnel);
						
						if(bAdjusted){
							tHeight-= tSpline.tRoad.opt_TerrainSubtract_Match;
							if(tHeight < 0f){ tHeight = 0f; }
							xVect.y = tHeight;
							tHeight = ((tHeight) / TTD.TerrainSize.y);
							
							//Set height values:
							TTD.tHeights[i,k] = true;
							TTD.cX[TTD.cI] = (ushort)i;
							TTD.cY[TTD.cI] = (ushort)k;
							TTD.oldH[TTD.cI] = TTD.heights[i,k];
							TTD.heights[i,k] = tHeight;
							TTD.cI+=1;

							tReturnFloat = xVect.y;
//							bOneHit = true;
						}
					}else{
						xVect = ConvertTerrainCoordToWorldVect(i,k,TTD.heights[i,k],ref TTD);
						AdjustedTerrainVect_Tri(ref param, out bAdjusted,out tHeight,ref xVect,ref tList, bIsBridge, bIsTunnel);
						
						if(bAdjusted){
							tHeight-= tSpline.tRoad.opt_TerrainSubtract_Match;
							if(tHeight < 0f){ tHeight = 0f; }
							tReturnFloat = tHeight;
//							bOneHit = true;
						}
					}
				}
			}
			
			if(bIsBridge && IsApproximately(tReturnFloat,0f,0.0001f)){
				tReturnFloat = tSpline.GetSplineValue(param,false).y;	
			}
			
			return tReturnFloat;
		}
		
		private static Vector3 ConvertTerrainCoordToWorldVect(int x, int y, float tHeight,ref GSD.Roads.GSDTerraforming.TempTerrainData TTD){
			//Get the normalized position of this game object relative to the terrain:
			float x1 = x / ((float)TTD.HM-1f);
			x1 = x1 * TTD.TerrainSize.x;
			
			float z1 = y / ((float)TTD.HM-1f);
			z1 = z1 * TTD.TerrainSize.z;

			float y1 = tHeight * TTD.TerrainSize.y;

			Vector3 xVect = new Vector3(z1,y1,x1);
			xVect += TTD.TerrainPos;
			
			return xVect;
		}

		private static void AdjustedTerrainVect_Tri(ref float param, out bool bAdjusted, out float tHeight,ref Vector3 xVect, ref List<TerrainBoundsMaker> tList, bool bIsBridge, bool bIsTunnel){
			float OrigHeight = xVect.y;
			int mCount = tList.Count;
			int tCount = 0;
			GSD.Roads.GSDRoadUtil.Construction3DTri tTri;
			TerrainBoundsMaker TBM;
			bAdjusted = false;
			tHeight = 0f;
			Vector2 t2D = new Vector2(xVect.x,xVect.z);
			for(int i=0;i<mCount;i++){
				TBM = tList[i];
				if(param < TBM.MinI){
					return;	
				}
				if(param > TBM.MaxI){
					continue;	
				}
				if(TBM.tRect.Contains(ref t2D)){
					tCount = TBM.triList.Count;
					for(int k=0;k<tCount;k++){
						tTri = TBM.triList[k];	
						if(tTri.Contains2D(ref t2D)){
							tHeight = tTri.LinePlaneIntersection(ref xVect).y;
							if(bIsBridge){
								if(OrigHeight > (tHeight-0.03f)){
									tHeight -= 0.03f;
									bAdjusted = true;
									return;
								}
							}else if(bIsTunnel){
								if(OrigHeight < (tHeight+0.03f)){
									tHeight += 0.03f;
									bAdjusted = true;
									return;
								}
							}else{
								bAdjusted = true;
								return;
							}
						}
					}
				}
			}
		}
		
		private static bool IsApproximately(float a, float b){
	    	return IsApproximately(a, b, 0.01f);
	    }
	     
	    private static bool IsApproximately(float a, float b, float tolerance){
	   		return Mathf.Abs(a - b) < tolerance;
	    }
	}

	public static class GSDRoadCreationT{
		#region "Road Prelim"
		//Privatized for obfuscate:
		public static void RoadJob_Prelim(ref GSDRoad tRoad){
			RoadJob_DoPrelim(ref tRoad);
		}
		private static void RoadJob_DoPrelim(ref GSDRoad tRoad){
			GSDSplineC tSpline = tRoad.GSDSpline;
			//Road,shoulder,ramp and lane widths:
			float RoadWidth = tRoad.RoadWidth();
			float ShoulderWidth = tRoad.opt_ShoulderWidth;
			float RoadSeperation = RoadWidth / 2f;
			float RoadSeperation_NoTurn = RoadWidth / 2f;
			float ShoulderSeperation = RoadSeperation + ShoulderWidth;
			float LaneWidth = tRoad.opt_LaneWidth;
			float RoadSep1Lane = (RoadSeperation + (LaneWidth*0.5f));
			float RoadSep2Lane = (RoadSeperation + (LaneWidth*1.5f));
			float ShoulderSep1Lane = (ShoulderSeperation + (LaneWidth*0.5f));
			float ShoulderSep2Lane = (ShoulderSeperation + (LaneWidth*1.5f));
			
			//Vector3 buffers used in construction:
			Vector3 rVect = default(Vector3);
			Vector3 lVect = default(Vector3);
			Vector3 ShoulderR_rVect = default(Vector3);
			Vector3 ShoulderR_lVect = default(Vector3);
			Vector3 ShoulderL_rVect = default(Vector3);
			Vector3 ShoulderL_lVect = default(Vector3);
			Vector3 RampR_R = default(Vector3);
			Vector3 RampR_L = default(Vector3);
			Vector3 RampL_R = default(Vector3);
			Vector3 RampL_L = default(Vector3);
			float ShoulderR_OuterAngle = 0f; if(ShoulderR_OuterAngle < 0f){ }
			float ShoulderL_OuterAngle = 0f; if(ShoulderL_OuterAngle < 0f){ }
//			Vector3 ShoulderR_OuterDirection = default(Vector3);
//			Vector3 ShoulderL_OuterDirection = default(Vector3);
			
			//Previous temp storage values:
			Vector3 tVect_Prev = default(Vector3);				if(tVect_Prev == default(Vector3)){ } //Prev step storage of road variable.
			Vector3 rVect_Prev = default(Vector3);				if(rVect_Prev == default(Vector3)){ }//Prev step storage of road variable.
			Vector3 lVect_Prev = default(Vector3);				if(lVect_Prev == default(Vector3)){ }//Prev step storage of road variable.
			Vector3 ShoulderR_PrevLVect = default(Vector3);		if(ShoulderR_PrevLVect == default(Vector3)){ }//Prev step storage of shoulder variable.
			Vector3 ShoulderL_PrevRVect = default(Vector3);		if(ShoulderL_PrevRVect == default(Vector3)){ }//Prev step storage of shoulder variable.
			Vector3 ShoulderR_PrevRVect = default(Vector3);		if(ShoulderR_PrevRVect == default(Vector3)){ }//Prev step storage of shoulder variable.
			Vector3 ShoulderL_PrevLVect = default(Vector3);		if(ShoulderL_PrevLVect == default(Vector3)){ }//Prev step storage of shoulder variable.
//			Vector3 ShoulderR_PrevRVect2 = default(Vector3);	//Prev storage of shoulder variable (2 step history).
//			Vector3 ShoulderL_PrevLVect2 = default(Vector3);	//Prev storage of shoulder variable (2 step history).
//			Vector3 ShoulderR_PrevRVect3 = default(Vector3);	//Prev storage of shoulder variable (3 step history).
//			Vector3 ShoulderL_PrevLVect3 = default(Vector3);	//Prev storage of shoulder variable (3 step history).
			Vector3 RampR_PrevR = default(Vector3);				if(RampR_PrevR == default(Vector3)){ }//Prev storage of ramp variables (outer shoulder).
			Vector3 RampR_PrevL = default(Vector3);				if(RampR_PrevL == default(Vector3)){ }//Prev storage of ramp variables (outer shoulder).
			Vector3 RampL_PrevR = default(Vector3);				if(RampL_PrevR == default(Vector3)){ }//Prev storage of ramp variables (outer shoulder).
			Vector3 RampL_PrevL = default(Vector3);				if(RampL_PrevL == default(Vector3)){ }//Prev storage of ramp variables (outer shoulder).
//			Vector3 ShoulderR_OuterDirectionPrev = default(Vector3);	//Prev storage of outer shoulder direction (euler).
//			Vector3 ShoulderL_OuterDirectionPrev = default(Vector3);	//Prev storage of outer shoulder direction (euler).
			
			//Height and angle variables, used to change certain parameters of road depending on past & future angle and height changes.
//			float tAngle = 0f;
//			float OrigStep = 0.06f;
			float Step = tRoad.opt_RoadDefinition / tSpline.distance;
//			float AngleStep = 5f;
			Vector3 tHeight0 = new Vector3(0f,0.1f,0f);
//			Vector3 tHeight2 = new Vector3(0f,0.15f,0f);
//			Vector3 tHeight1 = new Vector3(0f,0.2f,0f);
			float OuterShoulderWidthR = 0f;
			float OuterShoulderWidthL = 0f;
			float RampOuterWidthR = (OuterShoulderWidthR / 6f) + OuterShoulderWidthR;
			float RampOuterWidthL = (OuterShoulderWidthL / 6f) + OuterShoulderWidthL;
			Vector3 tVect = default(Vector3);
			Vector3 POS = default(Vector3);
			float TempY = 0f;
//			bool bTempYWasNegative = false;
//			Vector3 tY = new Vector3(0f,0f,0f);
			float tHeightAdded = 0f;		if(tHeightAdded < 0f){ }
//			float[] HeightChecks = new float[5];
			Vector3 gHeight = default(Vector3);
			
			//Bridge variables:
			bool bIsBridge = false;			if(bIsBridge == false){ }
			bool bTempbridge = false;		if(bTempbridge == false){ }
			bool bBridgeInitial = false;	if(bBridgeInitial == false){ }
			bool bBridgeLast = false;		if(bBridgeLast == false){ }
			float BridgeUpComing;
//			int BridgeLIndex;
//			int BridgeRIndex;
			
			//Tunnel variables:	
			bool bIsTunnel = false;			if(bIsTunnel == false){ }
			bool bTempTunnel = false;		if(bTempTunnel == false){ }
			bool bTunnelInitial = false; 	if(bTunnelInitial == false){ }
			bool bTunnelLast = false; 		if(bTunnelLast == false){ }
			float TunnelUpComing = 0f;		if(TunnelUpComing < 0f){ }
//			int TunnelLIndex;
//			int TunnelRIndex;

			//Intersection variables and buffers:
			float tIntHeight = 0f;			if(tIntHeight < 0f){ }
			float tIntStrength = 0f;		if(tIntStrength < 0f){ }
			float tIntStrength_temp = 0f; 	if(tIntStrength_temp < 0f){ }
//			float tIntDistCheck = 75f;
			GSDRoadIntersection GSDRI = null;
			bool bIsPastInter = false;
			bool bMaxIntersection = false;
			bool bWasPrevMaxInter = false;
			GSDSplineN xNode = null;
			float tInterSubtract = 4f;
			float tLastInterHeight = -4f;
			bool bOverrideRampR = false;
			bool bOverrideRampL = false;
			Vector3 RampR_Override = default(Vector3);
			Vector3 RampL_Override = default(Vector3);
			bool bFirstInterNode = false;
			bool bInter_PrevWasCorner = false; if(bInter_PrevWasCorner == false){ }
			bool bInter_CurreIsCorner = false;
			bool bInter_CurreIsCornerRR = false;
			bool bInter_CurreIsCornerRL = false;
			bool bInter_CurreIsCornerLL = false;
			bool bInter_CurreIsCornerLR = false;
			bool bInter_PrevWasCornerRR = false;
			bool bInter_PrevWasCornerRL = false;
			bool bInter_PrevWasCornerLL = false;
			bool bInter_PrevWasCornerLR = false;
			Vector3 iTemp_HeightVect = default(Vector3);
			Vector3 rVect_iTemp = default(Vector3);
			Vector3 lVect_iTemp = default(Vector3);
			Vector3 ShoulderR_R_iTemp = default(Vector3);
			Vector3 ShoulderL_L_iTemp = default(Vector3);
			Vector3 RampR_R_iTemp = default(Vector3);
			Vector3 RampR_L_iTemp = default(Vector3);
			Vector3 RampL_R_iTemp = default(Vector3);
			Vector3 RampL_L_iTemp = default(Vector3);
			Vector3 tempIVect_Prev = default(Vector3);
			Vector3 tempIVect = tVect;
			bool b0LAdded = false; if(b0LAdded == false){ }
			bool b1LAdded = false; if(b1LAdded == false){ }
			bool b2LAdded = false; if(b2LAdded == false){ }
			bool b3LAdded = false; if(b3LAdded == false){ }
			bool f0LAdded = false; if(f0LAdded == false){ }
			bool f1LAdded = false; if(f1LAdded == false){ }
			bool f2LAdded = false; if(f2LAdded == false){ }
			bool f3LAdded = false; if(f3LAdded == false){ }
			bool b0RAdded = false; if(b0RAdded == false){ }
			bool b1RAdded = false; if(b1RAdded == false){ }
			bool b2RAdded = false; if(b2RAdded == false){ }
			bool b3RAdded = false; if(b3RAdded == false){ }
			bool f0RAdded = false; if(f0RAdded == false){ }
			bool f1RAdded = false; if(f1RAdded == false){ }
			bool f2RAdded = false; if(f2RAdded == false){ }
			bool f3RAdded = false; if(f3RAdded == false){ }
			bool bInterTestAddAfterR = false; if(bInterTestAddAfterR == false){ }
			bool bInterTestAddAfterL = false; if(bInterTestAddAfterL == false){ }
//			Vector3 InterTestVect1 = default(Vector3);
//			Vector3 InterTestVect2 = default(Vector3);
//			Vector3 InterTestVect3 = default(Vector3);
//			Vector3 InterTestVect4 = default(Vector3);
			bool bShoulderSkipR = false;
			bool bShoulderSkipL = false;
			bool bShrinkRoadB = false;
			bool bShrinkRoadFNext = false;
			bool bShrinkRoadF = false;
			bool bIsNextInter = false;
			GSDSplineN cNode = null;
			int NodeID = -1;
			int NodeIDPrev = -1;
			int NodeCount = tSpline.GetNodeCount();
			bool bDynamicCut = false;
			float CullDistanceSQ = (3f * RoadWidth) * (3f * RoadWidth);
			float mCornerDist = 0f;
			Vector2 CornerRR = default(Vector2);		if(CornerRR == default(Vector2)){ }
			Vector2 CornerRL = default(Vector2);		if(CornerRL == default(Vector2)){ }
			Vector2 CornerLR = default(Vector2);		if(CornerLR == default(Vector2)){ }
			Vector2 CornerLL = default(Vector2);		if(CornerLL == default(Vector2)){ }
			Vector2 rVect2D = default(Vector2);			if(rVect2D == default(Vector2)){ }
			Vector2 lVect2D = default(Vector2);			if(lVect2D == default(Vector2)){ }
			Vector3 tempIVect_prev = default(Vector3);	if(tempIVect_prev == default(Vector3)){ }
			Vector3 POS_Next = default(Vector3);		if(POS_Next == default(Vector3)){ }
			Vector3 tVect_Next = default(Vector3);		if(tVect_Next == default(Vector3)){ }
			Vector3 rVect_Next = default(Vector3);		if(rVect_Next == default(Vector3)){ }
			Vector3 lVect_Next = default(Vector3);		if(lVect_Next == default(Vector3)){ }
			Vector3 xHeight = default(Vector3); 		if(xHeight == default(Vector3)){ }
			bool bLRtoRR = false;
			bool bLLtoLR = false;
			bool bLine = false;
			bool bImmuneR = false;
			bool bImmuneL = false;
			bool bSpecAddedL = false; if(bSpecAddedL == false){ }
			bool bSpecAddedR = false; if(bSpecAddedR == false){ }
			bool bTriggerInterAddition = false;
			bool bSpecialThreeWayIgnoreR = false;
			bool bSpecialThreeWayIgnoreL = false;
//			int eCount = -1;
//			int eIndex = -1;
//			int uCount = -1;
//			int uIndex = -1;
			float bMod1 = 1.75f;
			float bMod2 = 1.25f;
			float t2DDist = -1f;
			List<Vector3> vList = null;
			List<int> eList = null;
			float tParam2 = 0f;
			float tParam1 = 0f;
			bool bRecordShoulderForNormals = false;
			bool bRecordShoulderLForNormals = false;
			
			//Unused for now, for later partial construction methods:
			bool bInterseOn = tRoad.RCS.bInterseOn;
//			bool bBridgesOn = tRoad.RCS.bBridgesOn;
//			if(tRoad.RCS.bRoadOn){
				bInterseOn = true;
//			}	
			
			//Prelim intersection construction and profiling:
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("RoadJob_Prelim_Inter"); }
			if(bInterseOn){
				RoadJob_Prelim_Inter(ref tRoad);
			}
			
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			
			
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("RoadPrelimForLoop"); }
			
			//Road/shoulder cuts: Init necessary since a road cut is added for the last segment after this function:
			if(tRoad.opt_bRoadCuts || tRoad.opt_bDynamicCuts){
				tRoad.RCS.RoadCutNodes.Add(tSpline.mNodes[0]);
			}
			if(tRoad.opt_bShoulderCuts || tRoad.opt_bDynamicCuts){
				tRoad.RCS.ShoulderCutsLNodes.Add(tSpline.mNodes[0]);
				tRoad.RCS.ShoulderCutsRNodes.Add(tSpline.mNodes[0]);
			}
			
			//Start initializing the loop. Convuluted to handle special control nodes, so roads don't get rendered where they aren't supposed to, while still preserving the proper curvature.
			float FinalMax = 1f;
			float StartMin = 0f;
			if(tSpline.bSpecialEndControlNode){	//If control node, start after the control node:
				FinalMax = tSpline.mNodes[tSpline.GetNodeCount()-2].tTime;
			}
			if(tSpline.bSpecialStartControlNode){	//If ends in control node, end construction before the control node:
				StartMin = tSpline.mNodes[1].tTime;
			}
			bool bFinalEnd = false;
			float RoadConnection_StartMin1 = StartMin;	//Storage of incremental start values for the road connection mesh construction at the end of this function.
			float RoadConnection_FinalMax1 = FinalMax; 	//Storage of incremental end values for the road connection mesh construction at the end of this function.
			if(tSpline.bSpecialEndNode_IsStart_Delay){
				StartMin += (tSpline.SpecialEndNodeDelay_Start / tSpline.distance);	//If there's a start delay (in meters), delay the start of road construction: Due to special control nodes for road connections or 3 way intersections.
			}else if(tSpline.bSpecialEndNode_IsEnd_Delay){
				FinalMax -= (tSpline.SpecialEndNodeDelay_End / tSpline.distance);	//If there's a end delay (in meters), cut early the end of road construction: Due to special control nodes for road connections or 3 way intersections.
			}
//			float RoadConnection_StartMin2 = StartMin;	//Storage of incremental start values for the road connection mesh construction at the end of this function.
//			float RoadConnection_FinalMax2 = FinalMax; 	//Storage of incremental end values for the road connection mesh construction at the end of this function.
			float i=StartMin;
			
//			int StartIndex = tSpline.GetClosestRoadDefIndex(StartMin,true,false);
//			int EndIndex = tSpline.GetClosestRoadDefIndex(FinalMax,false,true);
//			float cDist = 0f;
			bool kSkip = true;
			bool kSkipFinal = false;
			int kCount = 0;
			int vCount = kCount;
			int kFinalCount = tSpline.RoadDefKeysArray.Length;
			int spamcheckmax1 = 18000;
			int spamcheck1 = 0;
			
			if(IsApproximately(StartMin,0f,0.0001f)){
				kSkip = false;
			}
			if(IsApproximately(FinalMax,1f,0.0001f)){
				kSkipFinal = true;
			}
			
			//If startmin > 0 then kcount needs to start at proper road def
//			bool bStartMinEnabled = false;
			int StartMinIndex1 = 0;
			
			if(StartMin > 0f){
				kCount = tSpline.GetClosestRoadDefIndex(StartMin,true,false);
//				bStartMinEnabled = true;
				StartMinIndex1 = 1;
			}
			
			while(!bFinalEnd && spamcheck1 < spamcheckmax1){
				spamcheck1++;
				
				if(kSkip){
					i = StartMin;
					kSkip = false;	
				}else{
					if(kCount >= kFinalCount){
						i = FinalMax;
						if(kSkipFinal){ break; }
					}else{
						i = tSpline.TranslateInverseParamToFloat(tSpline.RoadDefKeysArray[kCount]);
						kCount+=1;
					}
				}
				
				if(i > 1f){ 
					break; 
				}
				if(i < 0f){ 
					i = 0f; 
				}
				
				if(IsApproximately(i,FinalMax,0.00001f)){
					bFinalEnd = true;
				}else if(i > FinalMax){
					if(tSpline.bSpecialEndControlNode){
						i = FinalMax;
						bFinalEnd = true;
					}else{
						bFinalEnd = true;
						break;	
					}
				}
				cNode = tSpline.GetCurrentNode(i);	//Set the current node.
				NodeID = cNode.idOnSpline;			//Set the current node ID.
				if(NodeID != NodeIDPrev && (tRoad.opt_bRoadCuts || tRoad.opt_bDynamicCuts)){	//If different than the previous node id, time to make a cut, if necessary:
					//Don't ever cut the first node, last node, intersection node, special control nodes, bridge nodes or bridge control nodes:
					if(NodeID > StartMinIndex1 && NodeID < (NodeCount-1) && !cNode.bIsIntersection && !cNode.bSpecialEndNode){ // && !cNode.bIsBridge_PreNode && !cNode.bIsBridge_PostNode){
						if(tRoad.opt_bDynamicCuts){
							bDynamicCut = cNode.bRoadCut;
						}else{
							bDynamicCut = true;	
						}
						
						if(bDynamicCut){
							tRoad.RCS.RoadCuts.Add(tRoad.RCS.RoadVectors.Count);			//Add the vector index to cut later.
							tRoad.RCS.RoadCutNodes.Add(cNode);								//Store the node which was at the beginning of this cut.	
						}
						if(tRoad.opt_bShoulderCuts && bDynamicCut){	//If option shoulder cuts is on.
							tRoad.RCS.ShoulderCutsL.Add(tRoad.RCS.ShoulderL_Vectors.Count);	//Add the vector index to cut later.
							tRoad.RCS.ShoulderCutsLNodes.Add(cNode);						//Store the node which was at the beginning of this cut.
							tRoad.RCS.ShoulderCutsR.Add(tRoad.RCS.ShoulderR_Vectors.Count);	//Add the vector index to cut later.
							tRoad.RCS.ShoulderCutsRNodes.Add(cNode);						//Store the node which was at the beginning of this cut.
						}
					}
				}
				if(NodeID != NodeIDPrev){
					if(tRoad.RCS.RoadVectors.Count > 0){
						cNode.bInitialRoadHeight = tRoad.RCS.RoadVectors[tRoad.RCS.RoadVectors.Count-1].y;
					}
				}
				NodeIDPrev = NodeID;				//Store the previous node ID for the next round. Done now with road cuts as far as this function is concerned.
				
				//Set all necessary intersection triggers to false:
				bInter_CurreIsCorner = false;
				bInter_CurreIsCornerRR = false;
				bInter_CurreIsCornerRL = false;
				bInter_CurreIsCornerLL = false;
				bInter_CurreIsCornerLR = false;
				b0LAdded = false;
				b1LAdded = false;
				b2LAdded = false;
				b3LAdded = false;
				f0LAdded = false;
				f1LAdded = false;
				f2LAdded = false;
				f3LAdded = false;
				b0RAdded = false;
				b1RAdded = false;
				b2RAdded = false;
				b3RAdded = false;
				f0RAdded = false;
				f1RAdded = false;
				f2RAdded = false;
				f3RAdded = false;
				bInterTestAddAfterR=false;
				bInterTestAddAfterL=false;
				bShoulderSkipR = false;
				bShoulderSkipL = false;
				bShrinkRoadB = false;
				bShrinkRoadF = false;
				bIsNextInter = false;
				if(bShrinkRoadFNext){
					bShrinkRoadFNext = false;
					bShrinkRoadF = true;	
				}
				bRecordShoulderForNormals = false;
				bRecordShoulderLForNormals = false;
				
				//Bridges: Note: This is convoluted due to need for triggers:
				bBridgeInitial = false;
				bBridgeLast = false;
				bTempbridge = tSpline.IsInBridge(i);
				if(!bIsBridge && bTempbridge){
					bIsBridge = true;
					bBridgeInitial = true;
				}else if(bIsBridge && !bTempbridge){
					bIsBridge = false;
				}
				//Check if this is the last bridge run for this bridge:
				if(bIsBridge){
					bTempbridge = tSpline.IsInBridge(i+Step);
					if(!bTempbridge){
						bBridgeLast = true;	
					}
				}
				
				//Tunnels: Note: This is convoluted due to need for triggers:
				bTunnelInitial = false;
				bTunnelLast = false;
				bTempTunnel = tSpline.IsInTunnel(i);
				if(!bIsTunnel && bTempTunnel){
					bIsTunnel = true;
					bTunnelInitial = true;
				}else if(bIsTunnel && !bTempTunnel){
					bIsTunnel = false;
				}
				//Check if this is the last Tunnel run for this Tunnel:
				if(bIsTunnel){
					bTempTunnel = tSpline.IsInTunnel(i+Step);
					if(!bTempTunnel){
						bTunnelLast = true;	
					}
				}
				
				//Master Vector3 for the current road construction location:
				tSpline.GetSplineValue_Both(i,out tVect,out POS);
				
//				Profiler.EndSample();
//				Profiler.BeginSample("Test2");
				
				//Detect downward or upward slope:
				TempY = POS.y;
//				bTempYWasNegative = false;
				if(TempY < 0f){
//					bTempYWasNegative = true;
					TempY *= -1f;	
				}
				if(tVect.y < 0f){
					tVect.y = 0f;	
				}
	
				//Determine if intersection:
				if(bInterseOn){
					bIsPastInter = false;	//If past intersection
					tIntStrength = tRoad.GSDSpline.IntersectionStrength(ref tVect,ref tIntHeight, ref GSDRI, ref bIsPastInter, ref i, ref xNode);
					bMaxIntersection = (tIntStrength >= 1f);	//1f strength = max intersection
					bFirstInterNode = false;	
				}
				
				//Outer widths:
				if(bMaxIntersection && bInterseOn){
					GSDRI.SignHeight = tIntHeight;
					xNode.iConstruction.bBLane0Done_Final_ThisRound = false;
					xNode.iConstruction.bBLane1Done_Final_ThisRound = false;
					xNode.iConstruction.bBLane2Done_Final_ThisRound = false;
					xNode.iConstruction.bBLane3Done_Final_ThisRound = false;
					xNode.iConstruction.bFLane0Done_Final_ThisRound = false;
					xNode.iConstruction.bFLane1Done_Final_ThisRound = false;
					xNode.iConstruction.bFLane2Done_Final_ThisRound = false;
					xNode.iConstruction.bFLane3Done_Final_ThisRound = false;
					xNode.iConstruction.bIsFrontFirstRound = false;
					
					if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
						OuterShoulderWidthR = ShoulderSeperation;
						OuterShoulderWidthL = ShoulderSeperation;
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
						OuterShoulderWidthR = ShoulderSep1Lane;
						OuterShoulderWidthL = ShoulderSep1Lane;
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						if(bIsPastInter){
							OuterShoulderWidthR = ShoulderSep1Lane;
							OuterShoulderWidthL = ShoulderSep2Lane;
						}else{
							OuterShoulderWidthR = ShoulderSep2Lane;
							OuterShoulderWidthL = ShoulderSep1Lane;
						}
					}
				}else{
					if(TempY < 0.5f || bIsBridge || bIsTunnel){
						OuterShoulderWidthR = ShoulderSeperation;
						OuterShoulderWidthL = ShoulderSeperation;
					}else{
						OuterShoulderWidthR = ShoulderSeperation + (TempY*0.05f);
						OuterShoulderWidthL = ShoulderSeperation + (TempY*0.05f);
					}
				}
				
				if(bIsBridge){ //No ramps for bridges:
					RampOuterWidthR = OuterShoulderWidthR;
					RampOuterWidthL = OuterShoulderWidthL;
				}else{
					RampOuterWidthR = (OuterShoulderWidthR / 4f) + OuterShoulderWidthR;
					RampOuterWidthL = (OuterShoulderWidthL / 4f) + OuterShoulderWidthL;
				}
				
				//The master outer road edges vector locations:
				if(bMaxIntersection && bInterseOn){	//If in maximum intersection, adjust road edge (also the shoulder inner edges):
					if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
						rVect = (tVect + new Vector3(RoadSeperation_NoTurn*POS.normalized.z,0,RoadSeperation_NoTurn*-POS.normalized.x));
						lVect = (tVect + new Vector3(RoadSeperation_NoTurn*-POS.normalized.z,0,RoadSeperation_NoTurn*POS.normalized.x));
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
						rVect = (tVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
						lVect = (tVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						if(bIsPastInter){
							rVect = (tVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
							lVect = (tVect + new Vector3(RoadSep2Lane*-POS.normalized.z,0,RoadSep2Lane*POS.normalized.x));;
						}else{
							rVect = (tVect + new Vector3(RoadSep2Lane*POS.normalized.z,0,RoadSep2Lane*-POS.normalized.x));
							lVect = (tVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
						}
					}else{
						rVect = (tVect + new Vector3(RoadSeperation*POS.normalized.z,0,RoadSeperation*-POS.normalized.x));
						lVect = (tVect + new Vector3(RoadSeperation*-POS.normalized.z,0,RoadSeperation*POS.normalized.x));
					}
				}else{
					//Typical road/shoulder inner edge location:
					rVect = (tVect + new Vector3(RoadSeperation*POS.normalized.z,0,RoadSeperation*-POS.normalized.x));
					lVect = (tVect + new Vector3(RoadSeperation*-POS.normalized.z,0,RoadSeperation*POS.normalized.x));
				}
				
				//Shoulder right vectors:
				ShoulderR_rVect = (tVect + new Vector3(OuterShoulderWidthR*POS.normalized.z,0,OuterShoulderWidthR*-POS.normalized.x));
				ShoulderR_lVect = rVect;	//Note that the shoulder inner edge is the same as the road edge vector.
				//Shoulder left vectors:
				ShoulderL_rVect = lVect;	//Note that the shoulder inner edge is the same as the road edge vector.
				ShoulderL_lVect = (tVect + new Vector3(OuterShoulderWidthL*-POS.normalized.z,0,OuterShoulderWidthL*POS.normalized.x));
				
//				Profiler.EndSample();
//				Profiler.BeginSample("Test3");
				
				//Now to start the main lane construction for the intersection:
				if(bMaxIntersection && bInterseOn){
//					if(kCount >= tSpline.RoadDefKeysArray.Length){
//						vCount = tSpline.RoadDefKeysArray.Length-1;
//					}else{
//						vCount = kCount-1;	
//					}
					vCount = kCount;
					
					tParam2 = tSpline.TranslateInverseParamToFloat(tSpline.RoadDefKeysArray[vCount]);
					float tInterStrNext = tRoad.GSDSpline.IntersectionStrength_Next(tSpline.GetSplineValue(tParam2,false));
					if(IsApproximately(tInterStrNext,1f,0.001f) || tInterStrNext > 1f){
						bIsNextInter = true;
					}else{
						bIsNextInter = false;			
					}
					
					if(string.Compare(xNode.UID,GSDRI.Node1.UID) == 0){
						bFirstInterNode = true;
					}else{
						bFirstInterNode = false;
					}

					tempIVect = tVect;
					if(bIsPastInter){
						bool bLLtoRL = bFirstInterNode;
						bool bRLtoRR = !bFirstInterNode;
						if(xNode.iConstruction.iFLane0L.Count == 0){
							xNode.iConstruction.bIsFrontFirstRound = true;
							xNode.iConstruction.bIsFrontFirstRoundTriggered = true;
							xNode.iConstruction.bFLane0Done_Final_ThisRound = true;
							xNode.iConstruction.bFLane1Done_Final_ThisRound = true;
							xNode.iConstruction.bFLane2Done_Final_ThisRound = true;
							xNode.iConstruction.bFLane3Done_Final_ThisRound = true;

							if(GSDRI.bFlipped && !bFirstInterNode){
								if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
									xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerLL_CornerLR[0],tIntHeight));
									xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
									xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
									xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerLL_CornerLR[2],tIntHeight));
									xNode.iConstruction.iFLane2L.Add(GVC(GSDRI.fCornerLL_CornerLR[2],tIntHeight));
									xNode.iConstruction.iFLane2R.Add(GVC(GSDRI.fCornerLL_CornerLR[3],tIntHeight));
									xNode.iConstruction.iFLane3L.Add(GVC(GSDRI.fCornerLL_CornerLR[3],tIntHeight));
									xNode.iConstruction.iFLane3R.Add(GVC(GSDRI.fCornerLL_CornerLR[4],tIntHeight));
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
									xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerLL_CornerLR[0],tIntHeight));
									xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
									xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
									xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerLL_CornerLR[2],tIntHeight));
									xNode.iConstruction.iFLane2L.Add(GVC(GSDRI.fCornerLL_CornerLR[2],tIntHeight));
									xNode.iConstruction.iFLane2R.Add(GVC(GSDRI.fCornerLL_CornerLR[3],tIntHeight));
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
									xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerLL_CornerLR[0],tIntHeight));
									xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
									xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
									xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerLL_CornerLR[2],tIntHeight));
								}	
							}else{
								if(bLLtoRL){
									if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
										xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerLL_CornerRL[4],tIntHeight));
										xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerLL_CornerRL[3],tIntHeight));
										xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerLL_CornerRL[3],tIntHeight));
										xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerLL_CornerRL[2],tIntHeight));
										xNode.iConstruction.iFLane2L.Add(GVC(GSDRI.fCornerLL_CornerRL[2],tIntHeight));
										xNode.iConstruction.iFLane2R.Add(GVC(GSDRI.fCornerLL_CornerRL[1],tIntHeight));
										xNode.iConstruction.iFLane3L.Add(GVC(GSDRI.fCornerLL_CornerRL[1],tIntHeight));
										xNode.iConstruction.iFLane3R.Add(GVC(GSDRI.fCornerLL_CornerRL[0],tIntHeight));
									}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
										xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerLL_CornerRL[3],tIntHeight));
										xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerLL_CornerRL[2],tIntHeight));
										xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerLL_CornerRL[2],tIntHeight));
										xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerLL_CornerRL[1],tIntHeight));
										xNode.iConstruction.iFLane2L.Add(GVC(GSDRI.fCornerLL_CornerRL[1],tIntHeight));
										xNode.iConstruction.iFLane2R.Add(GVC(GSDRI.fCornerLL_CornerRL[0],tIntHeight));
									}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
										xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerLL_CornerRL[2],tIntHeight));
										xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerLL_CornerRL[1],tIntHeight));
										xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerLL_CornerRL[1],tIntHeight));
										xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerLL_CornerRL[0],tIntHeight));
									}
								}else if(bRLtoRR){
									if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
										xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerRL_CornerRR[4],tIntHeight));
										xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerRL_CornerRR[3],tIntHeight));
										xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerRL_CornerRR[3],tIntHeight));
										xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
										xNode.iConstruction.iFLane2L.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
										xNode.iConstruction.iFLane2R.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
										xNode.iConstruction.iFLane3L.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
										xNode.iConstruction.iFLane3R.Add(GVC(GSDRI.fCornerRL_CornerRR[0],tIntHeight));
									}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
										xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerRL_CornerRR[3],tIntHeight));
										xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
										xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
										xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
										xNode.iConstruction.iFLane2L.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
										xNode.iConstruction.iFLane2R.Add(GVC(GSDRI.fCornerRL_CornerRR[0],tIntHeight));
									}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
										xNode.iConstruction.iFLane0L.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
										xNode.iConstruction.iFLane0R.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
										xNode.iConstruction.iFLane1L.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
										xNode.iConstruction.iFLane1R.Add(GVC(GSDRI.fCornerRL_CornerRR[0],tIntHeight));
									}
								}
							}
							
							xNode.iConstruction.ShoulderFR_End = xNode.iConstruction.iFLane0L[0];
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								xNode.iConstruction.ShoulderFL_End = xNode.iConstruction.iFLane3R[0];
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								xNode.iConstruction.ShoulderFL_End = xNode.iConstruction.iFLane2R[0];
							}else{
								xNode.iConstruction.ShoulderFL_End = xNode.iConstruction.iFLane1R[0];
							}
							xNode.iConstruction.ShoulderFL_StartIndex = tRoad.RCS.ShoulderL_Vectors.Count-2;
							xNode.iConstruction.ShoulderFR_StartIndex = tRoad.RCS.ShoulderR_Vectors.Count-2;
						}
						
						//Line 0:
						xNode.iConstruction.f0LAttempt = rVect;
						if(!xNode.iConstruction.bFLane0Done && !GSDRI.Contains(ref rVect)){
							xNode.iConstruction.iFLane0L.Add(GVC(rVect,tIntHeight)); f0LAdded = true;
						}
						
						//Line 1:
					//	if(f0LAdded){
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
							tempIVect = tVect;
							if(!xNode.iConstruction.bFLane1Done && !GSDRI.Contains(ref tempIVect) && !GSDRI.ContainsLine(tempIVect,rVect)){
								if(f0LAdded){ xNode.iConstruction.iFLane0R.Add(GVC(tempIVect,tIntHeight)); f0RAdded = true; }
								xNode.iConstruction.iFLane1L.Add(GVC(tempIVect,tIntHeight)); f1LAdded = true;
							}else{
								if(f0LAdded){ xNode.iConstruction.iFLane0L.RemoveAt(xNode.iConstruction.iFLane0L.Count-1); f0LAdded = false; }
							}
						}else{
							tempIVect = (tVect + new Vector3((LaneWidth*0.5f)*POS.normalized.z,0f,(LaneWidth*0.5f)*-POS.normalized.x));
							if(!xNode.iConstruction.bFLane1Done && !GSDRI.Contains(ref tempIVect) && !GSDRI.ContainsLine(tempIVect,rVect)){
								if(f0LAdded){ xNode.iConstruction.iFLane0R.Add(GVC(tempIVect,tIntHeight));f0RAdded = true; }
								xNode.iConstruction.iFLane1L.Add(GVC(tempIVect,tIntHeight)); f1LAdded = true;
							}else{
								if(f0LAdded){ xNode.iConstruction.iFLane0L.RemoveAt(xNode.iConstruction.iFLane0L.Count-1); f0LAdded = false; }
							}
						}
						//}
						xNode.iConstruction.f0RAttempt = tempIVect;
						xNode.iConstruction.f1LAttempt = tempIVect;
						
						//Line 2:
						//if(f1LAdded){
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
							tempIVect = lVect;
							if(!xNode.iConstruction.bFLane2Done && !GSDRI.Contains(ref tempIVect) && !GSDRI.ContainsLine(tempIVect,rVect)){
								if(f1LAdded){ xNode.iConstruction.iFLane1R.Add(GVC(tempIVect,tIntHeight)); f1RAdded = true; }
							}else{
								if(f1LAdded && xNode.iConstruction.iFLane1L.Count > 1){ xNode.iConstruction.iFLane1L.RemoveAt(xNode.iConstruction.iFLane1L.Count-1); f1LAdded = false; }
							}
						}else{
							tempIVect = (tVect + new Vector3((LaneWidth*0.5f)*-POS.normalized.z,0f,(LaneWidth*0.5f)*POS.normalized.x));
							tempIVect_prev = tempIVect;
							if(!xNode.iConstruction.bFLane2Done && !GSDRI.Contains(ref tempIVect) && !GSDRI.ContainsLine(tempIVect,rVect)){
								if(f1LAdded){ xNode.iConstruction.iFLane1R.Add(GVC(tempIVect,tIntHeight)); f1RAdded = true; }
								xNode.iConstruction.iFLane2L.Add(GVC(tempIVect,tIntHeight)); f2LAdded = true;
							}else{
								if(f1LAdded){ xNode.iConstruction.iFLane1L.RemoveAt(xNode.iConstruction.iFLane1L.Count-1); f1LAdded = false; f1RAdded=false; }
							}
						}
						//}
						xNode.iConstruction.f1RAttempt = tempIVect;
						xNode.iConstruction.f2LAttempt = tempIVect;
						
						//Line 3 / 4:
						//if(f2LAdded){
					
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
							tempIVect = (tVect + new Vector3(((LaneWidth*0.5f)+RoadSeperation)*-POS.normalized.z,0,((LaneWidth*0.5f)+RoadSeperation)*POS.normalized.x));
							if(!xNode.iConstruction.bFLane3Done && !GSDRI.Contains(ref tempIVect) && !GSDRI.ContainsLine(lVect,tempIVect)){
							
								xNode.iConstruction.iFLane3L.Add(GVC(tempIVect,tIntHeight));  f3LAdded = true;
								xNode.iConstruction.iFLane3R.Add(GVC(lVect,tIntHeight)); f3RAdded = true;
//								if(bIsNextInter && GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.FourWay){
									if(f2LAdded){ xNode.iConstruction.iFLane2R.Add(GVC(tempIVect,tIntHeight)); f2RAdded = true; }
//								}
							}else{
								if(f2LAdded){
									xNode.iConstruction.iFLane2L.RemoveAt(xNode.iConstruction.iFLane2L.Count-1); f2LAdded = false;
								}
							}
								
						}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
							tempIVect = (tVect + new Vector3(((LaneWidth*0.5f)+RoadSeperation)*-POS.normalized.z,0,((LaneWidth*0.5f)+RoadSeperation)*POS.normalized.x));
							if(f2LAdded && !GSDRI.Contains(ref tempIVect) && !GSDRI.ContainsLine(rVect,tempIVect)){
								xNode.iConstruction.iFLane2R.Add(GVC(tempIVect,tIntHeight)); f2RAdded = true;
							}else if(f2LAdded){
								xNode.iConstruction.iFLane2L.RemoveAt(xNode.iConstruction.iFLane2L.Count-1);  f2LAdded = false;
							}	
						}
				
					//	}
						xNode.iConstruction.f2RAttempt = tempIVect;
						xNode.iConstruction.f3LAttempt = tempIVect;
						xNode.iConstruction.f3RAttempt = lVect;
						
						if(!bIsNextInter && !xNode.iConstruction.bFDone){
//							xNode.iConstruction.bFDone = true;
							xNode.iConstruction.bFLane0Done = true;
							xNode.iConstruction.bFLane1Done = true;
							xNode.iConstruction.bFLane2Done = true;
							xNode.iConstruction.bFLane3Done = true;
							
							POS_Next = default(Vector3);
							tVect_Next = default(Vector3); 
							
							tParam1 = tSpline.TranslateInverseParamToFloat(tSpline.RoadDefKeysArray[kCount]);
							tSpline.GetSplineValue_Both(tParam1,out tVect_Next,out POS_Next);
							rVect_Next = (tVect_Next + new Vector3(RoadSeperation*POS_Next.normalized.z,0,RoadSeperation*-POS_Next.normalized.x));
							lVect_Next = (tVect_Next + new Vector3(RoadSeperation*-POS_Next.normalized.z,0,RoadSeperation*POS_Next.normalized.x));
							
							xNode.iConstruction.iFLane0L.Add(GVC(rVect_Next,tIntHeight));	
							xNode.iConstruction.iFLane0R.Add(GVC(tVect_Next,tIntHeight));
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								xNode.iConstruction.iFLane1L.Add(GVC(tVect_Next,tIntHeight));	
								if(tRoad.opt_Lanes == 2){
									xNode.iConstruction.iFLane1R.Add(GVC(((rVect_Next-lVect_Next)*0.475f)+lVect_Next,tIntHeight));
								}else if(tRoad.opt_Lanes == 4){
									xNode.iConstruction.iFLane1R.Add(GVC(((rVect_Next-lVect_Next)*0.488f)+lVect_Next,tIntHeight));
								}else if(tRoad.opt_Lanes == 6){
									xNode.iConstruction.iFLane1R.Add(GVC(((rVect_Next-lVect_Next)*0.492f)+lVect_Next,tIntHeight));
								}

								if(tRoad.opt_Lanes == 2){
									xNode.iConstruction.iFLane3L.Add(GVC(((rVect_Next-lVect_Next)*0.03f)+lVect_Next,tIntHeight));
								}else if(tRoad.opt_Lanes == 4){
									xNode.iConstruction.iFLane3L.Add(GVC(((rVect_Next-lVect_Next)*0.015f)+lVect_Next,tIntHeight));
								}else if(tRoad.opt_Lanes == 6){
									xNode.iConstruction.iFLane3L.Add(GVC(((rVect_Next-lVect_Next)*0.01f)+lVect_Next,tIntHeight));
								}
									
								xNode.iConstruction.iFLane3R.Add(GVC(lVect_Next,tIntHeight));
//								xNode.iConstruction.iFLane2L.Add(GVC(tVect_Next,tIntHeight));	
//								xNode.iConstruction.iFLane2R.Add(GVC(lVect_Next,tIntHeight));
								
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								xNode.iConstruction.iFLane1L.Add(GVC(tVect_Next,tIntHeight));	
								if(tRoad.opt_Lanes == 2){
									xNode.iConstruction.iFLane1R.Add(GVC(((rVect_Next-lVect_Next)*0.475f)+lVect_Next,tIntHeight));
								}else if(tRoad.opt_Lanes == 4){
									xNode.iConstruction.iFLane1R.Add(GVC(((rVect_Next-lVect_Next)*0.488f)+lVect_Next,tIntHeight));
								}else if(tRoad.opt_Lanes == 6){
									xNode.iConstruction.iFLane1R.Add(GVC(((rVect_Next-lVect_Next)*0.492f)+lVect_Next,tIntHeight));
								}
								xNode.iConstruction.iFLane2L.Add(GVC(tVect_Next,tIntHeight));	
								xNode.iConstruction.iFLane2R.Add(GVC(lVect_Next,tIntHeight));

							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
								xNode.iConstruction.iFLane1L.Add(GVC(tVect_Next,tIntHeight));	
								xNode.iConstruction.iFLane1R.Add(GVC(lVect_Next,tIntHeight));
							}
							bShrinkRoadFNext = true;
//							bShrinkRoadF = true;
						}
						
					}else{
						bLRtoRR = bFirstInterNode;
						bLLtoLR = !bFirstInterNode;
						//B:
						//Line 0:
						tempIVect = lVect;
						bool bFirst123 = false;
						if(xNode.iConstruction.iBLane0R.Count == 0){
							xNode.iConstruction.iBLane0L.Add(lVect_Prev);	
							xNode.iConstruction.iBLane0R.Add(tVect_Prev);
							bShrinkRoadB = true;

							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								xNode.iConstruction.iBLane1L.Add(tVect_Prev);	
								xNode.iConstruction.iBLane1R.Add((tVect_Prev + new Vector3((LaneWidth*0.05f)*POS.normalized.z,0,(LaneWidth*0.05f)*-POS.normalized.x)));	
								xNode.iConstruction.iBLane3L.Add(((lVect_Prev-rVect_Prev)*0.03f)+rVect_Prev);	
								xNode.iConstruction.iBLane3R.Add(rVect_Prev);
								
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								xNode.iConstruction.iBLane1L.Add(tVect_Prev);	
								xNode.iConstruction.iBLane1R.Add((tVect_Prev + new Vector3((LaneWidth*0.05f)*POS.normalized.z,0,(LaneWidth*0.05f)*-POS.normalized.x)));	
								xNode.iConstruction.iBLane2L.Add(xNode.iConstruction.iBLane1R[0]);	
								xNode.iConstruction.iBLane2R.Add(rVect_Prev);
								
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
								xNode.iConstruction.iBLane1L.Add(tVect_Prev);	
								xNode.iConstruction.iBLane1R.Add(rVect_Prev);
							}
							
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								xNode.iConstruction.ShoulderBL_Start = xNode.iConstruction.iBLane0L[0];
								xNode.iConstruction.ShoulderBR_Start = xNode.iConstruction.iBLane3R[0];
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								xNode.iConstruction.ShoulderBL_Start = xNode.iConstruction.iBLane0L[0];
								xNode.iConstruction.ShoulderBR_Start = xNode.iConstruction.iBLane2R[0];
							}else{
								xNode.iConstruction.ShoulderBL_Start = xNode.iConstruction.iBLane0L[0];
								xNode.iConstruction.ShoulderBR_Start = xNode.iConstruction.iBLane1R[0];
							}
							
							xNode.iConstruction.ShoulderBL_StartIndex = tRoad.RCS.ShoulderL_Vectors.Count-2;
							xNode.iConstruction.ShoulderBR_StartIndex = tRoad.RCS.ShoulderR_Vectors.Count-2;
//							bFirst123 = true;
//							goto InterSkip;
						}
						
						bLine = false;
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
							bLine = !GSDRI.ContainsLine(tempIVect,(tVect + new Vector3((LaneWidth*0.5f)*-POS.normalized.z,0,(LaneWidth*0.5f)*POS.normalized.x)));
						}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
							bLine = !GSDRI.ContainsLine(tempIVect,(tVect + new Vector3((LaneWidth*0.5f)*-POS.normalized.z,0,(LaneWidth*0.5f)*POS.normalized.x)));
						}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
							bLine = !GSDRI.ContainsLine(lVect,tVect);
						}
						if(!xNode.iConstruction.bBLane0Done && !GSDRI.Contains(ref tempIVect) && bLine){
							xNode.iConstruction.iBLane0L.Add(GVC(tempIVect,tIntHeight)); b0LAdded = true;
						}else if(!xNode.iConstruction.bBLane0Done_Final){
							//Finalize lane 0:
							InterFinalizeiBLane0(ref xNode, ref GSDRI, ref tIntHeight, bLRtoRR, bLLtoLR,bFirstInterNode);
						}
						
						//Line 1:
						if(xNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
							if(xNode.iConstruction.iBLane0L.Count == 2){
								tempIVect = (tVect + new Vector3((LaneWidth*0.5f)*-POS.normalized.z,0,(LaneWidth*0.5f)*POS.normalized.x));
								xNode.iConstruction.iBLane0R.Add(GVC(tempIVect,tIntHeight));  		b0RAdded = true;	
							}
						}
						tempIVect_Prev = tempIVect;
						tempIVect = (tVect + new Vector3((LaneWidth*0.5f)*-POS.normalized.z,0,(LaneWidth*0.5f)*POS.normalized.x));
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){ tempIVect = tVect; }
						bLine = false;
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
							bLine = !GSDRI.ContainsLine(tempIVect,(tVect + new Vector3((LaneWidth*0.5f)*POS.normalized.z,0,(LaneWidth*0.5f)*-POS.normalized.x)));
						}if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){	
							bLine = !GSDRI.ContainsLine(tempIVect,rVect);
						}else{
							bLine = !GSDRI.ContainsLine(tempIVect,rVect);
						}
						tempIVect_Prev = tempIVect;
						if(b0LAdded && !xNode.iConstruction.bBLane1Done && !GSDRI.Contains(ref tempIVect) && bLine){
							if(b0LAdded && (xNode.iConstruction.iBLane0L.Count != 2 || GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane)){ 
								xNode.iConstruction.iBLane0R.Add(GVC(tempIVect,tIntHeight));   	b0RAdded = true;
							}
							xNode.iConstruction.iBLane1L.Add(GVC(tempIVect,tIntHeight));		b1LAdded = true;
						}else if(!xNode.iConstruction.bBLane1Done_Final){
							//Finalize lane 1:
							InterFinalizeiBLane1(ref xNode, ref GSDRI, ref tIntHeight, bLRtoRR, bLLtoLR, bFirstInterNode, ref b0LAdded, ref b1RAdded);
						}
						
						//Line 2:
						if(xNode.iConstruction.iBLane1R.Count == 0 && xNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
							xNode.iConstruction.iBLane1R.Add(GVC(tVect,tIntHeight));		b1RAdded = true;
							xNode.iConstruction.iBLane2L.Add(GVC(tVect,tIntHeight));		b2LAdded = true;
							b2LAdded = true;
						}else{
							tempIVect = (tVect + new Vector3((LaneWidth*0.5f)*POS.normalized.z,0,(LaneWidth*0.5f)*-POS.normalized.x));
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){ tempIVect = rVect; }
							if(b1LAdded){
								bLine = !GSDRI.ContainsLine(tempIVect,tempIVect_Prev);
							}else{
								bLine = !GSDRI.ContainsLine(tempIVect,rVect);	
							}
							if(!xNode.iConstruction.bBLane2Done && !GSDRI.Contains(ref tempIVect) && bLine){
								if(b1LAdded){ xNode.iConstruction.iBLane1R.Add(GVC(tempIVect,tIntHeight)); b1RAdded = true; }
								xNode.iConstruction.iBLane2L.Add(GVC(tempIVect,tIntHeight)); b2LAdded = true;
							}else if(!xNode.iConstruction.bBLane2Done_Final){
								InterFinalizeiBLane2(ref xNode, ref GSDRI, ref tIntHeight, bLRtoRR, bLLtoLR, bFirstInterNode, ref b2LAdded, ref b1LAdded, ref b0LAdded, ref b1RAdded);
							}
						}
						
						//Line 3 / 4:
						tempIVect = (tVect + new Vector3(((LaneWidth*0.5f)+RoadSeperation)*POS.normalized.z,0,((LaneWidth*0.5f)+RoadSeperation)*-POS.normalized.x));
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){ tempIVect = rVect; }
						if(!xNode.iConstruction.bBLane3Done && !GSDRI.ContainsLine(rVect,tempIVect) && !GSDRI.ContainsLine(rVect,lVect)){
							xNode.iConstruction.iBLane3L.Add(GVC(tempIVect,tIntHeight)); b3LAdded = true;
							xNode.iConstruction.iBLane3R.Add(GVC(rVect,tIntHeight)); b3RAdded = true;
							if(!bFirst123 && GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.FourWay){
								if(b2LAdded){ xNode.iConstruction.iBLane2R.Add(GVC(tempIVect,tIntHeight));	 b2RAdded = true; }
							}
						}else if(!xNode.iConstruction.bBLane3Done_Final){
							InterFinalizeiBLane3(ref xNode, ref GSDRI, ref tIntHeight, bLRtoRR, bLLtoLR, bFirstInterNode, ref b2LAdded, ref b1LAdded, ref b0LAdded, ref b1RAdded);
						}
						
					}
				}
				
//			InterSkip:
				
				if(!bIsBridge){
					BridgeUpComing = tRoad.GSDSpline.BridgeUpComing(i);
//					if(TempY < 0.5f){
//						gHeight = tHeight0;
//					}else if(TempY < 2f){
//						gHeight = tHeight2;
//					}else{
//						if(bTempYWasNegative){
//							tY = new Vector3(0f,(TempY*0.035f),0f);	
//						}
//						if(tY.y < tHeight1.y){
//							tY = tHeight1;	
//						}
//						gHeight = tY;
//					}
					if(BridgeUpComing < 0.2f){
						BridgeUpComing = 0.2f;	
					}
//					gHeight.y = gHeight.y * BridgeUpComing;
					
//					if(tRoad.opt_MatchTerrain){
						gHeight.y = 0f;
//					}
					
					lVect += gHeight;
					rVect += gHeight;
					ShoulderR_lVect += gHeight;
					ShoulderL_rVect += gHeight;	
					ShoulderL_lVect += gHeight;
					ShoulderR_rVect += gHeight;
					tHeightAdded = gHeight.y;
				}

				
				if(tIntStrength >= 1f){
					tVect.y -= tInterSubtract;
					tLastInterHeight = tVect.y;
					rVect.y -= tInterSubtract;
					lVect.y -= tInterSubtract;
					
					ShoulderL_rVect.y = tIntHeight;
					ShoulderR_lVect.y = tIntHeight;
					ShoulderR_rVect.y = tIntHeight;
					ShoulderL_lVect.y = tIntHeight; 
					
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref ShoulderL_rVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ ShoulderL_rVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*ShoulderL_rVect.y); }
//					
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref ShoulderR_lVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ ShoulderR_lVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*ShoulderR_lVect.y); }
//					
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref ShoulderR_rVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ ShoulderR_rVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*ShoulderR_rVect.y); }
//					
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref ShoulderL_lVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ ShoulderL_lVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*ShoulderL_lVect.y); }
				}else if(tIntStrength > 0f){
					
					rVect.y = (tIntStrength*tIntHeight) + ((1-tIntStrength)*rVect.y); 
					ShoulderR_lVect = rVect;
					lVect.y = (tIntStrength*tIntHeight) + ((1-tIntStrength)*lVect.y); 
					ShoulderL_rVect = lVect;
					ShoulderR_rVect.y = (tIntStrength*tIntHeight) + ((1-tIntStrength)*ShoulderR_rVect.y);
					ShoulderL_lVect.y = (tIntStrength*tIntHeight) + ((1-tIntStrength)*ShoulderL_lVect.y);
					
//					if(!Mathf.Approximately(tIntStrength,0f)){ tVect.y = (tIntStrength*tIntHeight) + ((1-tIntStrength)*tVect.y); }
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref rVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ rVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*rVect.y); ShoulderR_lVect = rVect; }
//					
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref lVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ lVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*lVect.y); ShoulderL_rVect = lVect; }
//					
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref ShoulderR_rVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ ShoulderR_rVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*ShoulderR_rVect.y); }
//					
//					tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref ShoulderL_lVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
//					if(!Mathf.Approximately(tIntStrength_temp,0f)){ ShoulderL_lVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*ShoulderL_lVect.y); }
				}

				//Ramp:
				RampR_L = ShoulderR_rVect;
				RampL_R = ShoulderL_lVect;
				if(bIsBridge){
					RampR_R = RampR_L;
					RampL_L = RampL_R;
				}else{
					RampR_R = (tVect + new Vector3(RampOuterWidthR*POS.normalized.z,0,RampOuterWidthR*-POS.normalized.x)) + gHeight;
					SetVectorHeight2(ref RampR_R,ref i, ref tSpline.HeightHistory, ref tSpline);
					RampR_R.y -= 0.35f;

					RampL_L = (tVect + new Vector3(RampOuterWidthL*-POS.normalized.z,0,RampOuterWidthL*POS.normalized.x)) + gHeight;
					SetVectorHeight2(ref RampL_L,ref i, ref tSpline.HeightHistory, ref tSpline);
					RampL_L.y -= 0.35f;
				}
				
				//Merge points to intersection corners if necessary:
				if(bMaxIntersection && !bIsBridge && !bIsTunnel && bInterseOn){
                    mCornerDist = tRoad.opt_RoadDefinition* 1.35f;
                    mCornerDist *= mCornerDist;

					CornerRR = new Vector2(GSDRI.CornerRR.x, GSDRI.CornerRR.z);
					CornerRL = new Vector2(GSDRI.CornerRL.x, GSDRI.CornerRL.z);
					CornerLR = new Vector2(GSDRI.CornerLR.x, GSDRI.CornerLR.z);
					CornerLL = new Vector2(GSDRI.CornerLL.x, GSDRI.CornerLL.z);
					rVect2D = new Vector2(rVect.x,rVect.z);
					lVect2D = new Vector2(lVect.x,lVect.z); 
					bOverrideRampR = false;
					bOverrideRampL = false;
					bImmuneR = false;
					bImmuneL = false;
					bMod1 = 1.75f;
					bMod2 = 1.25f;
					t2DDist = -1f;
					
					//Find equatable lane vect and move it too
//					eCount = -1;
//					eIndex = -1;
//					uCount = -1;
//					uIndex = -1;
					
					xHeight = new Vector3(0f,-0.1f,0f);
					bSpecAddedL = false;
					bSpecAddedR = false;
					
					if(bFirstInterNode){
						bSpecAddedL = (b0LAdded || f0LAdded);
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
							bSpecAddedR = (b1RAdded || f1LAdded);
						}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
							bSpecAddedR = (b2RAdded || f2LAdded);
						}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
							bSpecAddedR = (b3RAdded || f3LAdded);
						}
					}

                    float tempRoadDef = Mathf.Clamp(tRoad.opt_LaneWidth, 3f, 5f);

                    if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane) {
                        
                    } else if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane) {
                        
                    } else if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes) {
                        
                    }

					//RR:
					if(GSDRI.EvenAngle > 90f){ mCornerDist = tempRoadDef*bMod1; } else { mCornerDist = tempRoadDef*bMod2; }
					mCornerDist *= mCornerDist;
					t2DDist = Vector2.SqrMagnitude(CornerRR-rVect2D);
					if(t2DDist < mCornerDist){
						bImmuneR = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerRR = true;

						if(bFirstInterNode){
							vList = null;
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
								vList = xNode.iConstruction.iBLane1R;
								if(xNode.iConstruction.bBLane1Done_Final_ThisRound){ vList = null; }
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								vList = xNode.iConstruction.iBLane2R;
								if(xNode.iConstruction.bBLane2Done_Final_ThisRound){ vList = null; }
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								vList = xNode.iConstruction.iBLane3R;
								if(xNode.iConstruction.bBLane3Done_Final_ThisRound){ vList = null; }
							}
							
							eList = new List<int>();
							if(vList != null){
								for(int m=0;m<vList.Count;m++){
									if(Vector3.SqrMagnitude(vList[m]-ShoulderR_lVect) < 0.01f){
										if(!(IsApproximately(vList[m].x,GSDRI.CornerRR.x,0.01f) && IsApproximately(vList[m].z,GSDRI.CornerRR.z,0.01f))){
											eList.Add(m);									
										}
									}
								}
								for(int m=(eList.Count-1);m>=0;m--){
									vList.RemoveAt(eList[m]);	
								}
							}
							eList = null;
						}else{
							//2nd node can only come through RR as front with R
							vList = null;
							vList = xNode.iConstruction.iFLane0L;
							eList = new List<int>();
							if(vList != null){
								for(int m=1;m<vList.Count;m++){
									if(Vector3.SqrMagnitude(vList[m]-ShoulderR_lVect) < 0.01f){
										if(!(IsApproximately(vList[m].x,GSDRI.CornerRR.x,0.01f) && IsApproximately(vList[m].z,GSDRI.CornerRR.z,0.01f))){
											eList.Add(m);									
										}
									}
								}
								for(int m=(eList.Count-1);m>=0;m--){
									vList.RemoveAt(eList[m]);	
								}
							}
							eList = null;
						}
						
						ShoulderR_lVect = new Vector3(CornerRR.x,tIntHeight,CornerRR.y);
						ShoulderR_rVect = new Vector3(GSDRI.CornerRR_Outer.x,tIntHeight,GSDRI.CornerRR_Outer.z);
						RampR_Override = new Vector3(GSDRI.CornerRR_RampOuter.x,tIntHeight,GSDRI.CornerRR_RampOuter.z);
						bRecordShoulderForNormals = true;
					}else{
						t2DDist = Vector2.SqrMagnitude(CornerRR-lVect2D);
						if(t2DDist < mCornerDist){
							bImmuneL = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerRR = true;
							
							//2nd node can come in via left
							if(!bFirstInterNode){
								vList = null;
								vList = xNode.iConstruction.iBLane0L;
								if(xNode.iConstruction.bBLane0Done_Final_ThisRound){ vList = null; }
								eList = new List<int>();
								if(vList != null){
									for(int m=0;m<vList.Count;m++){
										if(Vector3.SqrMagnitude(vList[m]-ShoulderL_rVect) < 0.01f){
											if(!(IsApproximately(vList[m].x,GSDRI.CornerRR.x) && IsApproximately(vList[m].z,GSDRI.CornerRR.z))){
												eList.Add(m);
											}
										}
									}
									for(int m=(eList.Count-1);m>=0;m--){
										vList.RemoveAt(eList[m]);	
									}
								}
								eList = null;	
							}
							
							ShoulderL_rVect = new Vector3(CornerRR.x,tIntHeight,CornerRR.y);
							ShoulderL_lVect = new Vector3(GSDRI.CornerRR_Outer.x,tIntHeight,GSDRI.CornerRR_Outer.z);
							RampL_Override = new Vector3(GSDRI.CornerRR_RampOuter.x,tIntHeight,GSDRI.CornerRR_RampOuter.z);
							bRecordShoulderLForNormals = true;
						}
					}
					//RL:
					if(GSDRI.OddAngle > 90f){ mCornerDist = tempRoadDef*bMod1; } else { mCornerDist = tempRoadDef*bMod2; }
					mCornerDist *= mCornerDist;
					t2DDist = Vector2.SqrMagnitude(CornerRL-rVect2D);
					if(t2DDist < mCornerDist){
						bImmuneR = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerRL = true;
						
						if(bFirstInterNode){
							vList = null;
							vList = xNode.iConstruction.iFLane0L;
							eList = new List<int>();
							if(vList != null){
								for(int m=1;m<vList.Count;m++){
									if(Vector3.SqrMagnitude(vList[m]-ShoulderR_lVect) < 0.01f){
										if(!(IsApproximately(vList[m].x,GSDRI.CornerRL.x) && IsApproximately(vList[m].z,GSDRI.CornerRL.z))){
											eList.Add(m);									
										}
									}
								}
								for(int m=(eList.Count-1);m>=0;m--){
									vList.RemoveAt(eList[m]);	
								}
							}
							eList = null;
						}else{
							vList = null;
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
								vList = xNode.iConstruction.iBLane1R;
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								vList = xNode.iConstruction.iBLane2R;
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								vList = xNode.iConstruction.iBLane3R;
							}
							
							//Hitting RL from backside with second node:
							if(!bFirstInterNode){
								eList = new List<int>();
								if(vList != null){
									for(int m=0;m<vList.Count;m++){
										if(Vector3.SqrMagnitude(vList[m]-ShoulderR_lVect) < 0.01f){
											if(!(IsApproximately(vList[m].x,GSDRI.CornerRL.x) && IsApproximately(vList[m].z,GSDRI.CornerRL.z))){
												eList.Add(m);				
												if(m == vList.Count-1){
													if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
														b1RAdded = false;
													}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
														b2RAdded = false;
													}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
														b3RAdded = false;
													}	
												}
											}
										}
									}
									for(int m=(eList.Count-1);m>=0;m--){
										vList.RemoveAt(eList[m]);	
									}
								}
							}
							eList = null;	
						}
						
						ShoulderR_lVect = new Vector3(CornerRL.x,tIntHeight,CornerRL.y);
						ShoulderR_rVect = new Vector3(GSDRI.CornerRL_Outer.x,tIntHeight,GSDRI.CornerRL_Outer.z);
						RampR_Override = new Vector3(GSDRI.CornerRL_RampOuter.x,tIntHeight,GSDRI.CornerRL_RampOuter.z);
						bRecordShoulderForNormals = true;
					}else{
						t2DDist = Vector2.SqrMagnitude(CornerRL-lVect2D);
						if(t2DDist < mCornerDist){
							bImmuneL = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerRL = true;
							
							if(!bFirstInterNode){
								vList = null;
								if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
									vList = xNode.iConstruction.iFLane1R;
									if(xNode.iConstruction.bFLane1Done_Final_ThisRound){ vList = null; }
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
									vList = xNode.iConstruction.iFLane2R;
									if(xNode.iConstruction.bFLane2Done_Final_ThisRound){ vList = null; }
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
									vList = xNode.iConstruction.iFLane3R;
									if(xNode.iConstruction.bFLane3Done_Final_ThisRound){ vList = null; }
								}
								eList = new List<int>();
								if(vList != null){
									for(int m=1;m<vList.Count;m++){
										if(Vector3.SqrMagnitude(vList[m]-ShoulderL_rVect) < 0.01f){
											if(!(IsApproximately(vList[m].x,GSDRI.CornerRL.x) && IsApproximately(vList[m].z,GSDRI.CornerRL.z))){
												eList.Add(m);									
											}
										}
									}
									for(int m=(eList.Count-1);m>=0;m--){
										vList.RemoveAt(eList[m]);	
									}
								}
								eList = null;
							}
							
							ShoulderL_rVect = new Vector3(CornerRL.x,tIntHeight,CornerRL.y);
							ShoulderL_lVect = new Vector3(GSDRI.CornerRL_Outer.x,tIntHeight,GSDRI.CornerRL_Outer.z);
							RampL_Override = new Vector3(GSDRI.CornerRL_RampOuter.x,tIntHeight,GSDRI.CornerRL_RampOuter.z);
							bRecordShoulderLForNormals = true;
						}
					}
					//LR:
					if(GSDRI.OddAngle > 90f){ mCornerDist = tempRoadDef*bMod1; } else { mCornerDist = tempRoadDef*bMod2; }
					mCornerDist *= mCornerDist;
					t2DDist = Vector2.SqrMagnitude(CornerLR-rVect2D);
					if(t2DDist < mCornerDist){
						bImmuneR = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerLR = true;
						
						if(!bFirstInterNode){
							vList = null;
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
								vList = xNode.iConstruction.iBLane1R;
								if(xNode.iConstruction.bBLane1Done_Final_ThisRound){ vList = null; }
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								vList = xNode.iConstruction.iBLane2R;
								if(xNode.iConstruction.bBLane2Done_Final_ThisRound){ vList = null; }
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								vList = xNode.iConstruction.iBLane3R;
								if(xNode.iConstruction.bBLane3Done_Final_ThisRound){ vList = null; }
							}

							eList = new List<int>();
							if(vList != null){
								for(int m=0;m<vList.Count;m++){
									if(Vector3.SqrMagnitude(vList[m]-ShoulderR_lVect) < 0.01f){
										if(!(IsApproximately(vList[m].x,GSDRI.CornerLR.x) && IsApproximately(vList[m].z,GSDRI.CornerLR.z))){
											eList.Add(m);									
										}
									}
								}
								for(int m=(eList.Count-1);m>=0;m--){
									vList.RemoveAt(eList[m]);	
								}
							}
							eList = null;	
						}

						ShoulderR_lVect = new Vector3(CornerLR.x,tIntHeight,CornerLR.y);
						ShoulderR_rVect = new Vector3(GSDRI.CornerLR_Outer.x,tIntHeight,GSDRI.CornerLR_Outer.z);
						RampR_Override = new Vector3(GSDRI.CornerLR_RampOuter.x,tIntHeight,GSDRI.CornerLR_RampOuter.z);
						bRecordShoulderForNormals = true;
					}else{
						t2DDist = Vector2.SqrMagnitude(CornerLR-lVect2D);
						if(t2DDist < mCornerDist){
							bImmuneL = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerLR = true;
							
							if(bFirstInterNode){
								vList = null;
								vList = xNode.iConstruction.iBLane0L;
								if(xNode.iConstruction.bBLane0Done_Final_ThisRound){ vList = null; }
								eList = new List<int>();
								if(vList != null){
									for(int m=0;m<vList.Count;m++){
										if(Vector3.SqrMagnitude(vList[m]-ShoulderL_rVect) < 0.01f){
											if(!(IsApproximately(vList[m].x,GSDRI.CornerLR.x) && IsApproximately(vList[m].z,GSDRI.CornerLR.z))){
												eList.Add(m);									
											}
										}
									}
									for(int m=(eList.Count-1);m>=0;m--){
										vList.RemoveAt(eList[m]);	
									}
								}
								eList = null;
							}else{
								//2nd node can only come through LR as front with L
								vList = null;
								if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
									vList = xNode.iConstruction.iFLane1R;
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
									vList = xNode.iConstruction.iFLane2R;
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
									vList = xNode.iConstruction.iFLane3R;
								}
								eList = new List<int>();
								if(vList != null){
									for(int m=1;m<vList.Count;m++){
										if(Vector3.SqrMagnitude(vList[m]-ShoulderL_rVect) < 0.01f){
											if(!(IsApproximately(vList[m].x,GSDRI.CornerLR.x) && IsApproximately(vList[m].z,GSDRI.CornerLR.z))){
												eList.Add(m);									
											}
										}
									}
									for(int m=(eList.Count-1);m>=0;m--){
										vList.RemoveAt(eList[m]);	
									}
								}
								eList = null;	
							}
							
							ShoulderL_rVect = new Vector3(CornerLR.x,tIntHeight,CornerLR.y);
							ShoulderL_lVect = new Vector3(GSDRI.CornerLR_Outer.x,tIntHeight,GSDRI.CornerLR_Outer.z);
							RampL_Override = new Vector3(GSDRI.CornerLR_RampOuter.x,tIntHeight,GSDRI.CornerLR_RampOuter.z);
							bRecordShoulderLForNormals = true;
						}
					}
					//LL:
					if(GSDRI.EvenAngle > 90f){ mCornerDist = tempRoadDef*bMod1; } else { mCornerDist = tempRoadDef*bMod2; }
					mCornerDist *= mCornerDist;
					t2DDist = Vector2.SqrMagnitude(CornerLL-rVect2D);
					if(t2DDist < mCornerDist){
						bImmuneR = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerLL = true;
						
						
						if(!bFirstInterNode){
							vList = null;
							vList = xNode.iConstruction.iFLane0L;
							eList = new List<int>();
							if(vList != null){
								for(int m=1;m<vList.Count;m++){
									if(Vector3.SqrMagnitude(vList[m]-ShoulderR_lVect) < 0.01f){
										if(!(IsApproximately(vList[m].x,GSDRI.CornerLL.x) && IsApproximately(vList[m].z,GSDRI.CornerLL.z))){
											eList.Add(m);									
										}
									}
								}
								for(int m=(eList.Count-1);m>=0;m--){
									vList.RemoveAt(eList[m]);	
								}
							}
							eList = null;	
						}
						
						ShoulderR_lVect = new Vector3(CornerLL.x,tIntHeight,CornerLL.y);
						ShoulderR_rVect = new Vector3(GSDRI.CornerLL_Outer.x,tIntHeight,GSDRI.CornerLL_Outer.z);
						RampR_Override = new Vector3(GSDRI.CornerLL_RampOuter.x,tIntHeight,GSDRI.CornerLL_RampOuter.z);
						bRecordShoulderForNormals = true;
					}else{
						t2DDist = Vector2.SqrMagnitude(CornerLL-lVect2D);
						if(t2DDist < mCornerDist){
							bImmuneL = true; bInter_CurreIsCorner = true; bInter_CurreIsCornerLL = true;
							
							if(bFirstInterNode){
								vList = null;
								if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
									vList = xNode.iConstruction.iFLane1R;
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
									vList = xNode.iConstruction.iFLane2R;
								}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
									vList = xNode.iConstruction.iFLane3R;
								}
								eList = new List<int>();
								if(vList != null){
									for(int m=1;m<vList.Count;m++){
										if(Vector3.SqrMagnitude(vList[m]-ShoulderL_rVect) < 0.01f){
											if(!(IsApproximately(vList[m].x,GSDRI.CornerLL.x) && IsApproximately(vList[m].z,GSDRI.CornerLL.z))){
												eList.Add(m);									
											}
										}
									}
									for(int m=(eList.Count-1);m>=0;m--){
										vList.RemoveAt(eList[m]);	
									}
								}
								eList = null;		
							}else{
								vList = null;
								vList = xNode.iConstruction.iBLane0L;
								if(xNode.iConstruction.bBLane0Done_Final_ThisRound){ vList = null; }
								eList = new List<int>();
								if(vList != null){
									for(int m=0;m<vList.Count;m++){
										if(Vector3.SqrMagnitude(vList[m]-ShoulderL_rVect) < 0.01f){
											if(!(IsApproximately(vList[m].x,GSDRI.CornerLL.x) && IsApproximately(vList[m].z,GSDRI.CornerLL.z))){
												eList.Add(m);									
											}
										}
									}
									for(int m=(eList.Count-1);m>=0;m--){
										vList.RemoveAt(eList[m]);	
									}
								}
								eList = null;	
							}
							
							ShoulderL_rVect = new Vector3(CornerLL.x,tIntHeight,CornerLL.y);
							ShoulderL_lVect = new Vector3(GSDRI.CornerLL_Outer.x,tIntHeight,GSDRI.CornerLL_Outer.z);
							RampL_Override = new Vector3(GSDRI.CornerLL_RampOuter.x,tIntHeight,GSDRI.CornerLL_RampOuter.z);
							bRecordShoulderLForNormals = true;
						}
					}
					
					if(bImmuneR){
						bOverrideRampR = true;
						if(!tRoad.RCS.ImmuneVects.Contains(ShoulderR_lVect)){ tRoad.RCS.ImmuneVects.Add(ShoulderR_lVect); }
						if(!tRoad.RCS.ImmuneVects.Contains(ShoulderR_rVect)){ tRoad.RCS.ImmuneVects.Add(ShoulderR_rVect); }
					}
					if(bImmuneL){
						bOverrideRampL = true;
						if(!tRoad.RCS.ImmuneVects.Contains(ShoulderL_rVect)){ tRoad.RCS.ImmuneVects.Add(ShoulderL_rVect); }
						if(!tRoad.RCS.ImmuneVects.Contains(ShoulderL_lVect)){ tRoad.RCS.ImmuneVects.Add(ShoulderL_lVect); }
					}
				}
				
				if(bShrinkRoadB){
					
                    if (lVect_Prev != new Vector3(0f, 0f, 0f)) {
                        tRoad.RCS.RoadVectors.Add(lVect_Prev);
                        tRoad.RCS.RoadVectors.Add(lVect_Prev);
                        tRoad.RCS.RoadVectors.Add(lVect_Prev);
                        tRoad.RCS.RoadVectors.Add(lVect_Prev);
                    }
				}
				if(bShrinkRoadF){
                    if (lVect != new Vector3(0f, 0f, 0f)) {
                        tRoad.RCS.RoadVectors.Add(lVect);
                        tRoad.RCS.RoadVectors.Add(lVect);
                        tRoad.RCS.RoadVectors.Add(lVect);
                        tRoad.RCS.RoadVectors.Add(lVect);
                    }
				}
				
				tRoad.RCS.RoadVectors.Add(lVect); 
				tRoad.RCS.RoadVectors.Add(lVect);
				tRoad.RCS.RoadVectors.Add(rVect); 
				tRoad.RCS.RoadVectors.Add(rVect);

                

				//Add bounds for later removal:
				if(!bIsBridge && !bIsTunnel && bMaxIntersection && bWasPrevMaxInter && bInterseOn){
					bool bGoAhead = true;
					if(xNode.bIsEndPoint){
						if(xNode.idOnSpline == 1){
							if(i < xNode.tTime){
								bGoAhead = false;
							}
						}else{
							if(i > xNode.tTime){
								bGoAhead = false;
							}
						}
					}
					
					//Get this and prev lvect rvect rects:
					if((Vector3.SqrMagnitude(xNode.pos-tVect) < CullDistanceSQ) && bGoAhead){
							GSD.Roads.GSDRoadUtil.Construction2DRect vRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(
								new Vector2(lVect.x,lVect.z),
								new Vector2(rVect.x,rVect.z),
								new Vector2(lVect_Prev.x,lVect_Prev.z),
								new Vector2(rVect_Prev.x,rVect_Prev.z), 
								tLastInterHeight
								);
						
						tRoad.RCS.tIntersectionBounds.Add(vRect);
//						GameObject tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//						tObj.transform.position = lVect;
//						tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//						tObj.transform.name = "temp22";
//						
//						tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//						tObj.transform.position = rVect;
//						tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//						tObj.transform.name = "temp22";
					}
				}
				
				//Ramp construction:
				RampR_L = ShoulderR_rVect;
				RampL_R = ShoulderL_lVect;
				if(bIsBridge){
					RampR_R = RampR_L;
					RampL_L = RampL_R;
				}else{
					RampR_R = (tVect + new Vector3(RampOuterWidthR*POS.normalized.z,0,RampOuterWidthR*-POS.normalized.x)) + gHeight;;
					if(bOverrideRampR){ RampR_R = RampR_Override; }	//Overrides will come from intersection.
					SetVectorHeight2(ref RampR_R,ref i, ref tSpline.HeightHistory, ref tSpline);
					RampR_R.y -= 0.35f;

					RampL_L = (tVect + new Vector3(RampOuterWidthL*-POS.normalized.z,0,RampOuterWidthL*POS.normalized.x)) + gHeight;;
					if(bOverrideRampL){ RampL_L = RampL_Override; }	//Overrides will come from intersection.
					SetVectorHeight2(ref RampL_L,ref i, ref tSpline.HeightHistory, ref tSpline);
					RampL_L.y -= 0.35f;
					bOverrideRampR = false;
					bOverrideRampL = false;
				}
				
				//If necessary during intersection construction, sometimes an addition will be created inbetween intersection corner points.
				//This addition will create a dip between corner points to 100% ensure there is no shoulder visible on the roads between corner points.
				bTriggerInterAddition = false;
				if(bMaxIntersection && bInterseOn){
					if(bFirstInterNode){
						if((bInter_PrevWasCornerLR && bInter_CurreIsCornerLL) || (bInter_PrevWasCornerRR && bInter_CurreIsCornerRL)){	
							bTriggerInterAddition = true;
						}
					}else{
						if(!GSDRI.bFlipped){
							if((bInter_PrevWasCornerLL && bInter_CurreIsCornerRL) || (bInter_PrevWasCornerLR && bInter_CurreIsCornerRR) || (bInter_PrevWasCornerRR && bInter_CurreIsCornerLR)){
								bTriggerInterAddition = true;
							}
						}else{
							if((bInter_PrevWasCornerRR && bInter_CurreIsCornerLR) || (bInter_PrevWasCornerLR && bInter_CurreIsCornerRR) || (bInter_PrevWasCornerRL && bInter_CurreIsCornerLL)){
								bTriggerInterAddition = true;
							}
						}
					}
					
					if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						bTriggerInterAddition = false;	
					}
					
					//For 3-way intersections:
					bSpecialThreeWayIgnoreR = false;
					bSpecialThreeWayIgnoreL = false;
					if(GSDRI.IgnoreSide > -1){
						if(GSDRI.IgnoreSide == 0){
							//RR to RL:
							if(bFirstInterNode && (bInter_PrevWasCornerRR && bInter_CurreIsCornerRL)){
								bTriggerInterAddition = false;		
							}
						}else if(GSDRI.IgnoreSide == 1){
							//RL to LL:
							if(!bFirstInterNode && ((bInter_PrevWasCornerRL && bInter_CurreIsCornerLL) || (bInter_PrevWasCornerLL && bInter_CurreIsCornerRL))){
								//bTriggerInterAddition = false;	
								if(GSDRI.bFlipped){
									bSpecialThreeWayIgnoreR = true;	
								}else{
									bSpecialThreeWayIgnoreL = true;
								}
							}
						}else if(GSDRI.IgnoreSide == 2){
							//LL to LR:
							if(bFirstInterNode && (bInter_PrevWasCornerLR && bInter_CurreIsCornerLL)){
								bTriggerInterAddition = false;
							}
						}else if(GSDRI.IgnoreSide == 3){
							//LR to RR:
							if(!bFirstInterNode && ((bInter_PrevWasCornerRR && bInter_CurreIsCornerLR) || (bInter_PrevWasCornerLR && bInter_CurreIsCornerRR))){
								//bTriggerInterAddition = false;	
								if(GSDRI.bFlipped){
									bSpecialThreeWayIgnoreL = true;	
								}else{
									bSpecialThreeWayIgnoreR = true;
								}
							}
						}
					}
					
					if(bTriggerInterAddition){
						iTemp_HeightVect = new Vector3(0f,0f,0f);
						rVect_iTemp = (((rVect_Prev-rVect)*0.5f)+rVect) + iTemp_HeightVect;
						lVect_iTemp = (((lVect_Prev-lVect)*0.5f)+lVect) + iTemp_HeightVect;
						ShoulderR_R_iTemp = (((ShoulderR_PrevRVect-ShoulderR_rVect)*0.5f)+ShoulderR_rVect) + iTemp_HeightVect;
						ShoulderL_L_iTemp = (((ShoulderL_PrevLVect-ShoulderL_lVect)*0.5f)+ShoulderL_lVect) + iTemp_HeightVect;
						RampR_R_iTemp = (((RampR_PrevR-RampR_R)*0.5f)+RampR_R) + iTemp_HeightVect;
						RampR_L_iTemp = (((RampR_PrevL-RampR_L)*0.5f)+RampR_L) + iTemp_HeightVect;
						RampL_R_iTemp = (((RampL_PrevR-RampL_R)*0.5f)+RampL_R) + iTemp_HeightVect;
						RampL_L_iTemp = (((RampL_PrevL-RampL_L)*0.5f)+RampL_L) + iTemp_HeightVect;	
						
//						ShoulderL_L_iTemp = lVect_iTemp;
//						RampL_R_iTemp = lVect_iTemp;
//						RampL_L_iTemp = lVect_iTemp;
//					
//						ShoulderR_R_iTemp = rVect_iTemp;
//						RampR_R_iTemp = rVect_iTemp;
//						RampR_L_iTemp = rVect_iTemp;
					}
					
					if(bTriggerInterAddition && !(GSDRI.bFlipped && !bFirstInterNode)){
						if(bFirstInterNode){
							if((bInter_PrevWasCornerRR && bInter_CurreIsCornerRL && !bSpecialThreeWayIgnoreR)){
								//Right shoulder:
								tRoad.RCS.ShoulderR_Vectors.Add(rVect_iTemp); 
								tRoad.RCS.ShoulderR_Vectors.Add(rVect_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_R_iTemp); 
								tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_R_iTemp);
								//Ramps:
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_L_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_L_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_R_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_R_iTemp);
							}
							if((bInter_PrevWasCornerLR && bInter_CurreIsCornerLL && !bSpecialThreeWayIgnoreL)){	
								//Left shoulder:
								tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_L_iTemp); 
								tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_L_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(lVect_iTemp); 
								tRoad.RCS.ShoulderL_Vectors.Add(lVect_iTemp);
								//Ramp:
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_L_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_L_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_R_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_R_iTemp);
							}
						}else{
							if((bInter_PrevWasCornerLR && bInter_CurreIsCornerRR && !bSpecialThreeWayIgnoreR)){
								//Right shoulder:
								tRoad.RCS.ShoulderR_Vectors.Add(rVect_iTemp); 
								tRoad.RCS.ShoulderR_Vectors.Add(rVect_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_R_iTemp); 
								tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_R_iTemp);
								//Ramps:
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_L_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_L_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_R_iTemp);
								tRoad.RCS.ShoulderR_Vectors.Add(RampR_R_iTemp);
							}
							if((bInter_PrevWasCornerLL && bInter_CurreIsCornerRL && !bSpecialThreeWayIgnoreL)){	
								//Left shoulder:
								tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_L_iTemp); 
								tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_L_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(lVect_iTemp); 
								tRoad.RCS.ShoulderL_Vectors.Add(lVect_iTemp);
								//Ramp:
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_L_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_L_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_R_iTemp);
								tRoad.RCS.ShoulderL_Vectors.Add(RampL_R_iTemp);
							}
						}
					}else if(bTriggerInterAddition && (GSDRI.bFlipped && !bFirstInterNode)){
						if((bInter_PrevWasCornerRR && bInter_CurreIsCornerLR && !bSpecialThreeWayIgnoreL)){
							//Left shoulder:
							tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_L_iTemp); 
							tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_L_iTemp);
							tRoad.RCS.ShoulderL_Vectors.Add(lVect_iTemp); 
							tRoad.RCS.ShoulderL_Vectors.Add(lVect_iTemp);
							//Ramp:
							tRoad.RCS.ShoulderL_Vectors.Add(RampL_L_iTemp);
							tRoad.RCS.ShoulderL_Vectors.Add(RampL_L_iTemp);
							tRoad.RCS.ShoulderL_Vectors.Add(RampL_R_iTemp);
							tRoad.RCS.ShoulderL_Vectors.Add(RampL_R_iTemp);
						}
						if((bInter_PrevWasCornerRL && bInter_CurreIsCornerLL && !bSpecialThreeWayIgnoreR)){
							//Right shoulder:
							tRoad.RCS.ShoulderR_Vectors.Add(rVect_iTemp); 
							tRoad.RCS.ShoulderR_Vectors.Add(rVect_iTemp);
							tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_R_iTemp); 
							tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_R_iTemp);
							//Ramps:
							tRoad.RCS.ShoulderR_Vectors.Add(RampR_L_iTemp);
							tRoad.RCS.ShoulderR_Vectors.Add(RampR_L_iTemp);
							tRoad.RCS.ShoulderR_Vectors.Add(RampR_R_iTemp);
							tRoad.RCS.ShoulderR_Vectors.Add(RampR_R_iTemp);
						}
					}
				}
				
				
				
				
				//Right shoulder:
				if(!bShoulderSkipR){
					if(bRecordShoulderForNormals){
						tRoad.RCS.normals_ShoulderR_averageStartIndexes.Add(tRoad.RCS.ShoulderR_Vectors.Count);
					}
					
					tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_lVect); 
					tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_lVect);
					tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect); 
					tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
					tRoad.RCS.ShoulderR_Vectors.Add(RampR_L);
					tRoad.RCS.ShoulderR_Vectors.Add(RampR_L);
					tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
					tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
					
					//Double up to prevent normal errors from intersection subtraction:
					if(bImmuneR && bRecordShoulderForNormals){
						tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_lVect); 
						tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_lVect);
						tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect); 
						tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
						tRoad.RCS.ShoulderR_Vectors.Add(RampR_L);
						tRoad.RCS.ShoulderR_Vectors.Add(RampR_L);
						tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
						tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
					}
				}

				//Left shoulder:
				if(!bShoulderSkipL){
					if(bRecordShoulderLForNormals){
						tRoad.RCS.normals_ShoulderL_averageStartIndexes.Add(tRoad.RCS.ShoulderL_Vectors.Count);
					}
					tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect); 
					tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);
					tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_rVect); 
					tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_rVect);
					tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
					tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
					tRoad.RCS.ShoulderL_Vectors.Add(RampL_R);
					tRoad.RCS.ShoulderL_Vectors.Add(RampL_R);
					
					//Double up to prevent normal errors from intersection subtraction:
					if(bImmuneL && bRecordShoulderForNormals){
						tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect); 
						tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);
						tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_rVect); 
						tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_rVect);
						tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
						tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
						tRoad.RCS.ShoulderL_Vectors.Add(RampL_R);
						tRoad.RCS.ShoulderL_Vectors.Add(RampL_R);
					}
				}
				
				//Previous storage:
				tVect_Prev = tVect;
				rVect_Prev = rVect;
				lVect_Prev = lVect;
				ShoulderR_PrevLVect = ShoulderR_lVect;
				ShoulderL_PrevRVect = ShoulderL_rVect;
//				ShoulderR_PrevRVect3 = ShoulderR_PrevRVect2;
//				ShoulderL_PrevLVect3 = ShoulderL_PrevLVect2;
//				ShoulderR_PrevRVect2 = ShoulderR_PrevRVect;
//				ShoulderL_PrevLVect2 = ShoulderL_PrevLVect;
				ShoulderR_PrevRVect = ShoulderR_rVect;
				ShoulderL_PrevLVect = ShoulderL_lVect;
				RampR_PrevR = RampR_R;
				RampR_PrevL = RampR_L;
				RampL_PrevR = RampL_R;
				RampL_PrevL = RampL_L;

				//Store more prev variables:
				bWasPrevMaxInter = bMaxIntersection;
				bInter_PrevWasCorner = bInter_CurreIsCorner;
				bInter_PrevWasCornerRR = bInter_CurreIsCornerRR;
				bInter_PrevWasCornerRL = bInter_CurreIsCornerRL;
				bInter_PrevWasCornerLL = bInter_CurreIsCornerLL;
				bInter_PrevWasCornerLR = bInter_CurreIsCornerLR;		
				
//				i+=Step;//Master step incrementer.
			}	 
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("RoadJob_Prelim_FinalizeInter"); }
			//Finalize intersection vectors:
			if(bInterseOn){
				RoadJob_Prelim_FinalizeInter(ref tRoad);
			}
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("RoadJob_Prelim_RoadConnections"); }
			//Creates road connections if necessary:
//			float ExtraHeight = 0f;
//			float RampPercent = 0.2f;
			if(tSpline.bSpecialEndNode_IsStart_Delay){
				Vector3[] RoadConn_verts = new Vector3[4];

				RampR_R = tRoad.RCS.ShoulderR_Vectors[7];
				ShoulderR_rVect = tRoad.RCS.ShoulderR_Vectors[3];
				rVect = tRoad.RCS.ShoulderR_Vectors[0];
				
				tRoad.RCS.ShoulderR_Vectors.Insert(0,RampR_R);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,RampR_R);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,rVect);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,rVect);
				
				RampL_L = tRoad.RCS.ShoulderL_Vectors[4];
				ShoulderL_lVect = tRoad.RCS.ShoulderL_Vectors[0];
				lVect = tRoad.RCS.ShoulderL_Vectors[3];
				
				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,RampL_L);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,RampL_L);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,lVect);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,lVect);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect);

				RoadConn_verts[0] = lVect;
				RoadConn_verts[1] = rVect;
				tSpline.GetSplineValue_Both(RoadConnection_StartMin1,out tVect,out POS);
				RoadSeperation = tSpline.SpecialEndNodeDelay_Start_Result / 2f;
				rVect = (tVect + new Vector3(RoadSeperation*POS.normalized.z,0,RoadSeperation*-POS.normalized.x));
				lVect = (tVect + new Vector3(RoadSeperation*-POS.normalized.z,0,RoadSeperation*POS.normalized.x));
				ShoulderSeperation = RoadSeperation + ShoulderWidth;
				OuterShoulderWidthR = ShoulderSeperation;
				OuterShoulderWidthL = ShoulderSeperation;
				RampOuterWidthR = (OuterShoulderWidthR / 4f) + OuterShoulderWidthR;
				RampOuterWidthL = (OuterShoulderWidthL / 4f) + OuterShoulderWidthL;
				ShoulderR_rVect = (tVect + new Vector3(ShoulderSeperation*POS.normalized.z,0,ShoulderSeperation*-POS.normalized.x));
				ShoulderL_lVect = (tVect + new Vector3(ShoulderSeperation*-POS.normalized.z,0,ShoulderSeperation*POS.normalized.x));
				RampR_R = (tVect + new Vector3(RampOuterWidthR*POS.normalized.z,0,RampOuterWidthR*-POS.normalized.x));
				SetVectorHeight2(ref RampR_R,ref i, ref tSpline.HeightHistory, ref tSpline);
				RampR_R.y -= 0.45f;
				RampL_L = (tVect + new Vector3(RampOuterWidthL*-POS.normalized.z,0,RampOuterWidthL*POS.normalized.x));
				SetVectorHeight2(ref RampL_L,ref i, ref tSpline.HeightHistory, ref tSpline);
				RampL_L.y -= 0.45f;
				
			
				
				tRoad.RCS.ShoulderR_Vectors.Insert(0,RampR_R + tHeight0);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,RampR_R + tHeight0);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect + tHeight0);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect + tHeight0);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect + tHeight0);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,ShoulderR_rVect + tHeight0);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,rVect + tHeight0);
				tRoad.RCS.ShoulderR_Vectors.Insert(0,rVect + tHeight0);

				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect + tHeight0);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect + tHeight0);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,RampL_L + tHeight0);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,RampL_L + tHeight0);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,lVect + tHeight0);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,lVect + tHeight0);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect + tHeight0);
				tRoad.RCS.ShoulderL_Vectors.Insert(0,ShoulderL_lVect + tHeight0);

				RoadConn_verts[2] = lVect + tHeight0;
				RoadConn_verts[3] = rVect + tHeight0;
				//Tris:
				int[] RoadConn_tris = new int[6];
				RoadConn_tris[0] = 2;
				RoadConn_tris[1] = 0;
				RoadConn_tris[2] = 3;
				RoadConn_tris[3] = 0;
				RoadConn_tris[4] = 1;
				RoadConn_tris[5] = 3;

				Vector3[] RoadConn_normals = new Vector3[4];
				RoadConn_normals[0] = -Vector3.forward;
				RoadConn_normals[1] = -Vector3.forward;
				RoadConn_normals[2] = -Vector3.forward;
				RoadConn_normals[3] = -Vector3.forward;
				Vector2[] RoadConn_uv = new Vector2[4];
				float tMod1 = -1;
				float tMod2 = -1;
				
				if(tRoad.opt_Lanes == 2){
					tMod1 = 0.5f - (LaneWidth / tSpline.SpecialEndNodeDelay_Start_Result);
					tMod2 = 0.5f + (LaneWidth / tSpline.SpecialEndNodeDelay_Start_Result);
				}else if(tRoad.opt_Lanes == 4){
					tMod1 = 0.5f - ((LaneWidth*2f) / tSpline.SpecialEndNodeDelay_Start_Result);
					tMod2 = 0.5f + ((LaneWidth*2f) / tSpline.SpecialEndNodeDelay_Start_Result);
				}
				RoadConn_uv[0] = new Vector2(tMod1,0f);
				RoadConn_uv[1] = new Vector2(tMod2,0f);
				RoadConn_uv[2] = new Vector2(0f,1f);
				RoadConn_uv[3] = new Vector2(1f,1f);
				
				
				tRoad.RCS.RoadConnections_verts.Add(RoadConn_verts);
				tRoad.RCS.RoadConnections_tris.Add(RoadConn_tris);
				tRoad.RCS.RoadConnections_normals.Add(RoadConn_normals);
				tRoad.RCS.RoadConnections_uv.Add(RoadConn_uv);
			}else if(tSpline.bSpecialEndNode_IsEnd_Delay){
				Vector3[] RoadConn_verts = new Vector3[4];
				int rrCount = tRoad.RCS.ShoulderR_Vectors.Count;
				RampR_R = tRoad.RCS.ShoulderR_Vectors[rrCount-1];
				ShoulderR_rVect = tRoad.RCS.ShoulderR_Vectors[rrCount-3];
				rVect = tRoad.RCS.ShoulderR_Vectors[rrCount-7];
				
				//Right shoulder:
				tRoad.RCS.ShoulderR_Vectors.Add(rVect); 
				tRoad.RCS.ShoulderR_Vectors.Add(rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect); 
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
				tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
				
				rrCount = tRoad.RCS.ShoulderL_Vectors.Count;
				RampL_L = tRoad.RCS.ShoulderL_Vectors[rrCount-3];
				ShoulderL_lVect = tRoad.RCS.ShoulderL_Vectors[rrCount-1];
				lVect = tRoad.RCS.ShoulderL_Vectors[rrCount-5];
				
				//Left shoulder:
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect); 
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);
				tRoad.RCS.ShoulderL_Vectors.Add(lVect); 
				tRoad.RCS.ShoulderL_Vectors.Add(lVect);
				tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
				tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);
				
				RoadConn_verts[0] = lVect;
				RoadConn_verts[1] = rVect;
				tSpline.GetSplineValue_Both(RoadConnection_FinalMax1,out tVect,out POS);
				RoadSeperation = tSpline.SpecialEndNodeDelay_End_Result / 2f;
				rVect = (tVect + new Vector3(RoadSeperation*POS.normalized.z,0,RoadSeperation*-POS.normalized.x));
				lVect = (tVect + new Vector3(RoadSeperation*-POS.normalized.z,0,RoadSeperation*POS.normalized.x));
				ShoulderSeperation = RoadSeperation + ShoulderWidth;
				OuterShoulderWidthR = ShoulderSeperation;
				OuterShoulderWidthL = ShoulderSeperation;
				RampOuterWidthR = (OuterShoulderWidthR / 4f) + OuterShoulderWidthR;
				RampOuterWidthL = (OuterShoulderWidthL / 4f) + OuterShoulderWidthL;
				ShoulderR_rVect = (tVect + new Vector3(ShoulderSeperation*POS.normalized.z,0,ShoulderSeperation*-POS.normalized.x));
				ShoulderL_lVect = (tVect + new Vector3(ShoulderSeperation*-POS.normalized.z,0,ShoulderSeperation*POS.normalized.x));
				RampR_R = (tVect + new Vector3(RampOuterWidthR*POS.normalized.z,0,RampOuterWidthR*-POS.normalized.x));
				SetVectorHeight2(ref RampR_R,ref i, ref tSpline.HeightHistory, ref tSpline);
				RampR_R.y -= 0.35f;
				RampL_L = (tVect + new Vector3(RampOuterWidthL*-POS.normalized.z,0,RampOuterWidthL*POS.normalized.x));
				SetVectorHeight2(ref RampL_L,ref i, ref tSpline.HeightHistory, ref tSpline);
				RampL_L.y -= 0.35f;
				
				//Right shoulder:
				tRoad.RCS.ShoulderR_Vectors.Add(rVect); 
				tRoad.RCS.ShoulderR_Vectors.Add(rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect); 
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(ShoulderR_rVect);
				tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
				tRoad.RCS.ShoulderR_Vectors.Add(RampR_R);
				
				//Left shoulder:
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect); 
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);
				tRoad.RCS.ShoulderL_Vectors.Add(lVect); 
				tRoad.RCS.ShoulderL_Vectors.Add(lVect);
				tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
				tRoad.RCS.ShoulderL_Vectors.Add(RampL_L);
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);
				tRoad.RCS.ShoulderL_Vectors.Add(ShoulderL_lVect);

				RoadConn_verts[2] = lVect;
				RoadConn_verts[3] = rVect;
				//Tris:
				int[] RoadConn_tris = new int[6];
				RoadConn_tris[0] = 0;
				RoadConn_tris[1] = 2;
				RoadConn_tris[2] = 1;
				RoadConn_tris[3] = 2;
				RoadConn_tris[4] = 3;
				RoadConn_tris[5] = 1;
				
				Vector3[] RoadConn_normals = new Vector3[4];
				RoadConn_normals[0] = -Vector3.forward;
				RoadConn_normals[1] = -Vector3.forward;
				RoadConn_normals[2] = -Vector3.forward;
				RoadConn_normals[3] = -Vector3.forward;
				Vector2[] RoadConn_uv = new Vector2[4];
				float tMod = (RoadWidth / tSpline.SpecialEndNodeDelay_End_Result)/2f;
				RoadConn_uv[0] = new Vector2(tMod,0f);
				RoadConn_uv[1] = new Vector2(tMod*3f,0f);
				RoadConn_uv[2] = new Vector2(0f,1f);
				RoadConn_uv[3] = new Vector2(1f,1f);
				tRoad.RCS.RoadConnections_verts.Add(RoadConn_verts);
				tRoad.RCS.RoadConnections_tris.Add(RoadConn_tris);
				tRoad.RCS.RoadConnections_normals.Add(RoadConn_normals);
				tRoad.RCS.RoadConnections_uv.Add(RoadConn_uv);
			}
			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
		}	
		
		#region "Road prelim helpers"
		private static Vector3 GVC(Vector3 v1, float tHeight){
			return new Vector3(v1.x,tHeight,v1.z);		
		}
		
		/// <summary>
		/// Usage: tDir = forward dir of player. tVect = direction from player to enemy
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is vect in front the specified tDir tVect; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='tDir'>
		/// If set to <c>true</c> t dir.
		/// </param>
		/// <param name='tVect'>
		/// If set to <c>true</c> t vect.
		/// </param>
		private static bool IsVectInFront(Vector3 tDir, Vector3 tVect){
    		return (Vector3.Dot(tDir.normalized, tVect) > 0);
    	}
		
		private static Vector2 ConvertVect3_To_Vect2(Vector3 tVect){
			return new Vector2(tVect.x,tVect.z);	
		}
		
		private static void InterFinalizeiBLane0(ref GSDSplineN xNode, ref GSDRoadIntersection GSDRI, ref float tIntHeight, bool bLRtoRR, bool bLLtoLR, bool bFirstInterNode){
			if(xNode.iConstruction.bBLane0Done_Final){ return; }

			xNode.iConstruction.bBLane0Done = true;
			if(GSDRI.bFlipped && !bFirstInterNode){
				if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					xNode.iConstruction.iBLane0L.Add(GVC(GSDRI.fCornerRL_CornerRR[4],tIntHeight));
					xNode.iConstruction.iBLane0R.Add(GVC(GSDRI.fCornerRL_CornerRR[3],tIntHeight));
				}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					xNode.iConstruction.iBLane0L.Add(GVC(GSDRI.fCornerRL_CornerRR[3],tIntHeight));
					xNode.iConstruction.iBLane0R.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
				}else{
					xNode.iConstruction.iBLane0L.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
					xNode.iConstruction.iBLane0R.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
				}
			}else{
				if(bLRtoRR){
					xNode.iConstruction.iBLane0L.Add(GVC(GSDRI.fCornerLR_CornerRR[0],tIntHeight));
					xNode.iConstruction.iBLane0R.Add(GVC(GSDRI.fCornerLR_CornerRR[1],tIntHeight));
				}else if(bLLtoLR){
					xNode.iConstruction.iBLane0L.Add(GVC(GSDRI.fCornerLL_CornerLR[0],tIntHeight));
					xNode.iConstruction.iBLane0R.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
				}
			}
			xNode.iConstruction.bBLane0Done_Final = true;
			xNode.iConstruction.bBLane0Done_Final_ThisRound = true;	
		}
		
		private static void InterFinalizeiBLane1(ref GSDSplineN xNode, ref GSDRoadIntersection GSDRI, ref float tIntHeight, bool bLRtoRR, bool bLLtoLR, bool bFirstInterNode, ref bool b0LAdded, ref bool b1RAdded){
			if(xNode.iConstruction.bBLane1Done_Final){ return; }

			if(b0LAdded && !xNode.iConstruction.bBLane0Done_Final){ 
				xNode.iConstruction.iBLane0L.RemoveAt(xNode.iConstruction.iBLane0L.Count-1); 
				b0LAdded = false; 
				InterFinalizeiBLane0(ref xNode, ref GSDRI, ref tIntHeight, bLRtoRR, bLLtoLR,bFirstInterNode);
			}
			xNode.iConstruction.bBLane1Done = true;	
			xNode.iConstruction.bBLane0Done = true;
			
			if(GSDRI.bFlipped && !bFirstInterNode){
				if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					xNode.iConstruction.iBLane1L.Add(GVC(GSDRI.fCornerRL_CornerRR[3],tIntHeight));
					xNode.iConstruction.iBLane1R.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
				}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					xNode.iConstruction.iBLane1L.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
					xNode.iConstruction.iBLane1R.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
				}else{
					xNode.iConstruction.iBLane1L.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
					xNode.iConstruction.iBLane1R.Add(GVC(GSDRI.fCornerRL_CornerRR[0],tIntHeight)); //b1RAdded = true;
				}
			}else{
				if(bLRtoRR){
					xNode.iConstruction.iBLane1L.Add(GVC(GSDRI.fCornerLR_CornerRR[1],tIntHeight));
					xNode.iConstruction.iBLane1R.Add(GVC(GSDRI.fCornerLR_CornerRR[2],tIntHeight)); //b1RAdded = true;
				}else if(bLLtoLR){
					xNode.iConstruction.iBLane1L.Add(GVC(GSDRI.fCornerLL_CornerLR[1],tIntHeight));
					xNode.iConstruction.iBLane1R.Add(GVC(GSDRI.fCornerLL_CornerLR[2],tIntHeight)); //b1RAdded = true;
				}
			}
			xNode.iConstruction.bBLane1Done_Final = true;
			xNode.iConstruction.bBLane1Done_Final_ThisRound = true;
			
			if(bFirstInterNode && GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
				xNode.iConstruction.bBackRRpassed = true;	
			}
		}
		
		private static void InterFinalizeiBLane2(ref GSDSplineN xNode, ref GSDRoadIntersection GSDRI, ref float tIntHeight, bool bLRtoRR, bool bLLtoLR, bool bFirstInterNode, ref bool b2LAdded, ref bool b1LAdded, ref bool b0LAdded, ref bool b1RAdded){
			if(xNode.iConstruction.bBLane2Done_Final){ return; }
			
			if(b1LAdded && !xNode.iConstruction.bBLane1Done_Final){ 
				xNode.iConstruction.iBLane1L.RemoveAt(xNode.iConstruction.iBLane1L.Count-1); 
				b1LAdded = false; 
				InterFinalizeiBLane1(ref xNode, ref GSDRI, ref tIntHeight, bLRtoRR, bLLtoLR, bFirstInterNode, ref b0LAdded, ref b1RAdded);
			}
			xNode.iConstruction.bBLane1Done = true;
			xNode.iConstruction.bBLane2Done = true;	
			
			if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes || GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				if(GSDRI.bFlipped && !bFirstInterNode){
					if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						xNode.iConstruction.iBLane2L.Add(GVC(GSDRI.fCornerRL_CornerRR[2],tIntHeight));
						xNode.iConstruction.iBLane2R.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
						xNode.iConstruction.iBLane2L.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
						xNode.iConstruction.iBLane2R.Add(GVC(GSDRI.fCornerRL_CornerRR[0],tIntHeight));
					}
				}else{
					if(bLRtoRR){
						xNode.iConstruction.iBLane2L.Add(GVC(GSDRI.fCornerLR_CornerRR[2],tIntHeight));
						xNode.iConstruction.iBLane2R.Add(GVC(GSDRI.fCornerLR_CornerRR[3],tIntHeight));
					}else if(bLLtoLR){
						xNode.iConstruction.iBLane2L.Add(GVC(GSDRI.fCornerLL_CornerLR[2],tIntHeight));
						xNode.iConstruction.iBLane2R.Add(GVC(GSDRI.fCornerLL_CornerLR[3],tIntHeight));
					}
				}
			}
			xNode.iConstruction.bBLane2Done_Final = true;
			xNode.iConstruction.bBLane2Done_Final_ThisRound = true;
			
			if(bFirstInterNode && GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				xNode.iConstruction.bBackRRpassed = true;	
			}
		}
		
		private static void InterFinalizeiBLane3(ref GSDSplineN xNode, ref GSDRoadIntersection GSDRI, ref float tIntHeight, bool bLRtoRR, bool bLLtoLR, bool bFirstInterNode, ref bool b2LAdded, ref bool b1LAdded, ref bool b0LAdded, ref bool b1RAdded){
			if(b2LAdded && !xNode.iConstruction.bBLane2Done_Final){
				xNode.iConstruction.iBLane2L.RemoveAt(xNode.iConstruction.iBLane2L.Count-1);
				b2LAdded = false;
				InterFinalizeiBLane2(ref xNode, ref GSDRI, ref tIntHeight, bLRtoRR, bLLtoLR, bFirstInterNode, ref b2LAdded, ref b1LAdded, ref b0LAdded, ref b1RAdded);
			}
			xNode.iConstruction.bBLane2Done = true;
			xNode.iConstruction.bBLane3Done = true;	
			if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				if(GSDRI.bFlipped && !bFirstInterNode){
					xNode.iConstruction.iBLane3L.Add(GVC(GSDRI.fCornerRL_CornerRR[1],tIntHeight));
					xNode.iConstruction.iBLane3R.Add(GVC(GSDRI.fCornerRL_CornerRR[0],tIntHeight));
				}else{
					if(bLRtoRR){
						xNode.iConstruction.iBLane3L.Add(GVC(GSDRI.fCornerLR_CornerRR[3],tIntHeight));
						xNode.iConstruction.iBLane3R.Add(GVC(GSDRI.fCornerLR_CornerRR[4],tIntHeight));	
					}else if(bLLtoLR){
						xNode.iConstruction.iBLane3L.Add(GVC(GSDRI.fCornerLL_CornerLR[3],tIntHeight));	
						xNode.iConstruction.iBLane3R.Add(GVC(GSDRI.fCornerLL_CornerLR[4],tIntHeight));	
					}
				}
			}
			xNode.iConstruction.bBLane3Done_Final = true;
			xNode.iConstruction.bBLane3Done_Final_ThisRound = true;
			
			if(bFirstInterNode && GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				xNode.iConstruction.bBackRRpassed = true;	
			}
		}
		#endregion
		
		#endregion
		
		#region "Intersection Prelim"
		private static void RoadJob_Prelim_Inter(ref GSDRoad tRoad){
			GSDSplineC tSpline = tRoad.GSDSpline;
			float RoadWidth = tRoad.RoadWidth();
			float ShoulderWidth = tRoad.opt_ShoulderWidth;
			float RoadSeperation = RoadWidth / 2f;
			float RoadSeperation_NoTurn = RoadWidth / 2f;				
			float ShoulderSeperation = RoadSeperation + ShoulderWidth;	if(ShoulderSeperation < 0f){ }
			float LaneWidth = tRoad.opt_LaneWidth;
			float RoadSep1Lane = (RoadSeperation + (LaneWidth*0.5f));
			float RoadSep2Lane = (RoadSeperation + (LaneWidth*1.5f));
			Vector3 POS = default(Vector3);
			bool bIsPastInter = false;
			bool bOldMethod = false;
            //bool bCancel = false; if (bTempCancel) { }
            bool bTempCancel = false; if (bTempCancel) { }
			
			//If left collides with left, etc
			
			//This will speed up later calculations for intersection 4 corner construction:
			int mCount = tSpline.GetNodeCount();
			float PreInter_RoadWidthMod = 4.5f;
			if(!bOldMethod){
				PreInter_RoadWidthMod = 5.5f;
			}
			float preInterDistance = (tSpline.RoadWidth*PreInter_RoadWidthMod) / tSpline.distance;
			GSDSplineN iNode = null;
			for(int j=0;j<mCount;j++){
				bTempCancel = false;
				if(tSpline.mNodes[j].bIsIntersection){
					iNode = tSpline.mNodes[j];
					//First node set min / max float:
					if(iNode.iConstruction == null){ iNode.iConstruction = new GSD.Roads.GSDIntersections.iConstructionMaker(); }
					if(!iNode.iConstruction.tempconstruction_HasProcessed_Inter1){
						preInterDistance = (iNode.GSDSpline.RoadWidth*PreInter_RoadWidthMod) / iNode.GSDSpline.distance;
						iNode.iConstruction.tempconstruction_InterStart = iNode.tTime - preInterDistance;
					 	iNode.iConstruction.tempconstruction_InterEnd = iNode.tTime + preInterDistance;
						if(iNode.iConstruction.tempconstruction_InterStart > 1f){ iNode.iConstruction.tempconstruction_InterStart = 1f; }
						if(iNode.iConstruction.tempconstruction_InterStart < 0f){ iNode.iConstruction.tempconstruction_InterStart = 0f; }
						if(iNode.iConstruction.tempconstruction_InterEnd > 1f){ iNode.iConstruction.tempconstruction_InterEnd = 1f; }
						if(iNode.iConstruction.tempconstruction_InterEnd < 0f){ iNode.iConstruction.tempconstruction_InterEnd = 0f; }
						iNode.iConstruction.tempconstruction_HasProcessed_Inter1 = true;
					}
					
					if(string.Compare(iNode.UID,iNode.GSDRI.Node1.UID) == 0){
						iNode = iNode.GSDRI.Node2;
					}else{
						iNode = iNode.GSDRI.Node1;
					}

					//Grab other intersection node and set min / max float	
					try{
						if(!iNode.iConstruction.tempconstruction_HasProcessed_Inter1){
							preInterDistance = (iNode.GSDSpline.RoadWidth*PreInter_RoadWidthMod) / iNode.GSDSpline.distance;
							iNode.iConstruction.tempconstruction_InterStart = iNode.tTime - preInterDistance;
						 	iNode.iConstruction.tempconstruction_InterEnd = iNode.tTime + preInterDistance;
							if(iNode.iConstruction.tempconstruction_InterStart > 1f){ iNode.iConstruction.tempconstruction_InterStart = 1f; }
							if(iNode.iConstruction.tempconstruction_InterStart < 0f){ iNode.iConstruction.tempconstruction_InterStart = 0f; }
							if(iNode.iConstruction.tempconstruction_InterEnd > 1f){ iNode.iConstruction.tempconstruction_InterEnd = 1f; }
							if(iNode.iConstruction.tempconstruction_InterEnd < 0f){ iNode.iConstruction.tempconstruction_InterEnd = 0f; }
							iNode.iConstruction.tempconstruction_HasProcessed_Inter1 = true;
						}
					}catch{
						//Do nothing
					}
				}
			}
			
			//Now get the four points per intersection:
			GSDSplineN oNode1 = null;
			GSDSplineN oNode2 = null;
			float PreInterPrecision1 = -1f;
			float PreInterPrecision2 = -1f;
			Vector3 PreInterVect = default(Vector3);
			Vector3 PreInterVectR = default(Vector3);
			Vector3 PreInterVectR_RightTurn = default(Vector3);
			Vector3 PreInterVectL = default(Vector3);
			Vector3 PreInterVectL_RightTurn = default(Vector3);
			GSDRoadIntersection GSDRI = null;
			
			for(int j=0;j<mCount;j++){
				oNode1 = tSpline.mNodes[j];
				if(oNode1.bIsIntersection){
					oNode1 = oNode1.GSDRI.Node1;
					oNode2 = oNode1.GSDRI.Node2;
					if(bOldMethod){
						PreInterPrecision1 = 0.1f / oNode1.GSDSpline.distance;
						PreInterPrecision2 = 0.1f / oNode2.GSDSpline.distance;
					}else{
						PreInterPrecision1 = 4f / oNode1.GSDSpline.distance;
						PreInterPrecision2 = 4f / oNode2.GSDSpline.distance;
					}
					GSDRI = oNode1.GSDRI;
					try{
						if(oNode1.iConstruction.tempconstruction_HasProcessed_Inter2 && oNode2.iConstruction.tempconstruction_HasProcessed_Inter2){
							continue;
						}
					}catch{
						continue;	
					}
					GSDRI = oNode1.GSDRI;
					GSDRI.CornerRR1 = false;
					GSDRI.CornerRR2 = false;
					GSDRI.CornerRL1 = false;
					GSDRI.CornerRL2 = false;
					GSDRI.CornerLR1 = false;
					GSDRI.CornerLR2 = false;
					GSDRI.CornerLL1 = false;
					GSDRI.CornerLL2 = false;
					
					if(!oNode1.iConstruction.tempconstruction_HasProcessed_Inter2){
						oNode1.iConstruction.tempconstruction_R = new List<Vector2>();
						oNode1.iConstruction.tempconstruction_L = new List<Vector2>();
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
							oNode1.iConstruction.tempconstruction_R_RightTurn = new List<Vector2>();
							oNode1.iConstruction.tempconstruction_L_RightTurn = new List<Vector2>();
						}
						
						for(float i=oNode1.iConstruction.tempconstruction_InterStart;i<oNode1.iConstruction.tempconstruction_InterEnd;i+=PreInterPrecision1){
							oNode1.GSDSpline.GetSplineValue_Both(i,out PreInterVect,out POS);
							
							bIsPastInter = oNode1.GSDSpline.IntersectionIsPast(ref i,ref oNode1);
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								if(bIsPastInter){
									PreInterVectR = (PreInterVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
									PreInterVectL = (PreInterVect + new Vector3(RoadSep2Lane*-POS.normalized.z,0,RoadSep2Lane*POS.normalized.x));
								}else{
									PreInterVectR = (PreInterVect + new Vector3(RoadSep2Lane*POS.normalized.z,0,RoadSep2Lane*-POS.normalized.x));
									PreInterVectL = (PreInterVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
								}
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								PreInterVectR = (PreInterVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
								PreInterVectL = (PreInterVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
							}else{
								PreInterVectR = (PreInterVect + new Vector3(RoadSeperation_NoTurn*POS.normalized.z,0,RoadSeperation_NoTurn*-POS.normalized.x));
								PreInterVectL = (PreInterVect + new Vector3(RoadSeperation_NoTurn*-POS.normalized.z,0,RoadSeperation_NoTurn*POS.normalized.x));
							}
			
							oNode1.iConstruction.tempconstruction_R.Add(new Vector2(PreInterVectR.x,PreInterVectR.z));
							oNode1.iConstruction.tempconstruction_L.Add(new Vector2(PreInterVectL.x,PreInterVectL.z));
							
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								PreInterVectR_RightTurn = (PreInterVect + new Vector3(RoadSep2Lane*POS.normalized.z,0,RoadSep2Lane*-POS.normalized.x));
								oNode1.iConstruction.tempconstruction_R_RightTurn.Add(ConvertVect3_To_Vect2(PreInterVectR_RightTurn));
								
								PreInterVectL_RightTurn = (PreInterVect + new Vector3(RoadSep2Lane*-POS.normalized.z,0,RoadSep2Lane*POS.normalized.x));
								oNode1.iConstruction.tempconstruction_L_RightTurn.Add(ConvertVect3_To_Vect2(PreInterVectL_RightTurn));
							}
						}
					}
					
					//Process second node:
					if(oNode2.iConstruction == null){ oNode2.iConstruction = new GSD.Roads.GSDIntersections.iConstructionMaker(); }
					if(!oNode2.iConstruction.tempconstruction_HasProcessed_Inter2){
						oNode2.iConstruction.tempconstruction_R = new List<Vector2>();
						oNode2.iConstruction.tempconstruction_L = new List<Vector2>();
						if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
							oNode2.iConstruction.tempconstruction_R_RightTurn = new List<Vector2>();
							oNode2.iConstruction.tempconstruction_L_RightTurn = new List<Vector2>();
						}
						
						for(float i=oNode2.iConstruction.tempconstruction_InterStart;i<oNode2.iConstruction.tempconstruction_InterEnd;i+=PreInterPrecision2){
							oNode2.GSDSpline.GetSplineValue_Both(i,out PreInterVect,out POS);
							
							bIsPastInter = oNode2.GSDSpline.IntersectionIsPast(ref i,ref oNode2);
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								if(bIsPastInter){
									PreInterVectR = (PreInterVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
									PreInterVectL = (PreInterVect + new Vector3(RoadSep2Lane*-POS.normalized.z,0,RoadSep2Lane*POS.normalized.x));
								}else{
									PreInterVectR = (PreInterVect + new Vector3(RoadSep2Lane*POS.normalized.z,0,RoadSep2Lane*-POS.normalized.x));
									PreInterVectL = (PreInterVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
								}
							}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
								PreInterVectR = (PreInterVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
								PreInterVectL = (PreInterVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
							}else{
								PreInterVectR = (PreInterVect + new Vector3(RoadSeperation_NoTurn*POS.normalized.z,0,RoadSeperation_NoTurn*-POS.normalized.x));
								PreInterVectL = (PreInterVect + new Vector3(RoadSeperation_NoTurn*-POS.normalized.z,0,RoadSeperation_NoTurn*POS.normalized.x));
							}

							oNode2.iConstruction.tempconstruction_R.Add(new Vector2(PreInterVectR.x,PreInterVectR.z));
							oNode2.iConstruction.tempconstruction_L.Add(new Vector2(PreInterVectL.x,PreInterVectL.z));
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								PreInterVectR_RightTurn = (PreInterVect + new Vector3(RoadSep2Lane*POS.normalized.z,0,RoadSep2Lane*-POS.normalized.x));
								oNode2.iConstruction.tempconstruction_R_RightTurn.Add(ConvertVect3_To_Vect2(PreInterVectR_RightTurn));
								
								PreInterVectL_RightTurn = (PreInterVect + new Vector3(RoadSep2Lane*-POS.normalized.z,0,RoadSep2Lane*POS.normalized.x));
								oNode2.iConstruction.tempconstruction_L_RightTurn.Add(ConvertVect3_To_Vect2(PreInterVectL_RightTurn));
							}
						}
					}

	
					
					bool bFlipped = false;
					bool bFlippedSet = false;
					int hCount1 = oNode1.iConstruction.tempconstruction_R.Count;
					int hCount2 = oNode2.iConstruction.tempconstruction_R.Count;
					int N1RCount = oNode1.iConstruction.tempconstruction_R.Count;
					int N1LCount = oNode1.iConstruction.tempconstruction_L.Count;
					int N2RCount = oNode2.iConstruction.tempconstruction_R.Count;
					int N2LCount = oNode2.iConstruction.tempconstruction_L.Count;
					
					int[] tCounts = new int[4];
					tCounts[0] = N1RCount;
					tCounts[1] = N1LCount;
					tCounts[2] = N2RCount;
					tCounts[3] = N2LCount;
					
					//RR:
					int MaxCount = -1;
					MaxCount = Mathf.Max(N2RCount,N2LCount);
					for(int h=0;h<hCount1;h++){
						for(int k=0;k<MaxCount;k++){
							if(k < N2RCount){
								if(Vector2.Distance(oNode1.iConstruction.tempconstruction_R[h],oNode2.iConstruction.tempconstruction_R[k]) < tRoad.opt_RoadDefinition){
									bFlipped = false;
									bFlippedSet = true;
									break;
								}
							}
							if(k < N2LCount){
								if(Vector2.Distance(oNode1.iConstruction.tempconstruction_R[h],oNode2.iConstruction.tempconstruction_L[k]) < tRoad.opt_RoadDefinition){
									bFlipped = true;
									bFlippedSet = true;
									break;
								}
							}
						}
						if(bFlippedSet){ break; }
					}
					oNode1.GSDRI.bFlipped = bFlipped;
	
					
					//Three-way intersections lane specifics:
					GSDRI.bNode2B_LeftTurnLane = true;
					GSDRI.bNode2B_RightTurnLane = true;
					GSDRI.bNode2F_LeftTurnLane = true;
					GSDRI.bNode2F_RightTurnLane = true;
					
					//Three-way intersections:
					GSDRI.IgnoreSide = -1;
					GSDRI.IgnoreCorner = -1;
					GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.FourWay;	
					if(GSDRI.bFirstSpecial_First){
						GSDRI.IgnoreSide = 3;
						GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.ThreeWay;
						if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.StopSign_AllWay){
							GSDRI.IgnoreCorner = 0;
						}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
							GSDRI.IgnoreCorner = 1;	
						}
						
						if(!oNode1.GSDRI.bFlipped){
							GSDRI.bNode2F_LeftTurnLane = false;
							GSDRI.bNode2B_RightTurnLane = false;
						}else{
							GSDRI.bNode2B_LeftTurnLane = false;
							GSDRI.bNode2F_RightTurnLane = false;
						}
						
						
					}else if(GSDRI.bFirstSpecial_Last){
						GSDRI.IgnoreSide = 1;
						GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.ThreeWay;
						if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.StopSign_AllWay){
							GSDRI.IgnoreCorner = 2;
						}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
							GSDRI.IgnoreCorner = 3;	
						}
						
						if(!oNode1.GSDRI.bFlipped){
							GSDRI.bNode2B_LeftTurnLane = false;
							GSDRI.bNode2F_RightTurnLane = false;
						}else{
							GSDRI.bNode2F_LeftTurnLane = false;
							GSDRI.bNode2B_RightTurnLane = false;
						}
						
					}
					if(!bFlipped){
						if(GSDRI.bSecondSpecial_First){
							GSDRI.IgnoreSide = 2;
							GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.ThreeWay;
							if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.StopSign_AllWay){
								GSDRI.IgnoreCorner = 3;
							}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
								GSDRI.IgnoreCorner = 0;	
							}
							
							if(!oNode1.GSDRI.bFlipped){
								GSDRI.bNode2B_LeftTurnLane = false;
								GSDRI.bNode2F_RightTurnLane = false;
							}else{
								GSDRI.bNode2F_LeftTurnLane = false;
								GSDRI.bNode2B_RightTurnLane = false;
							}
							
						}else if(GSDRI.bSecondSpecial_Last){
							GSDRI.IgnoreSide = 0;
							GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.ThreeWay;
							if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.StopSign_AllWay){
								GSDRI.IgnoreCorner = 1;
							}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
								GSDRI.IgnoreCorner = 2;
							}
							
							if(!oNode1.GSDRI.bFlipped){
								GSDRI.bNode2B_LeftTurnLane = false;
								GSDRI.bNode2F_RightTurnLane = false;
							}else{
								GSDRI.bNode2F_LeftTurnLane = false;
								GSDRI.bNode2B_RightTurnLane = false;
							}
							
						}
					}else{
						if(GSDRI.bSecondSpecial_First){
							GSDRI.IgnoreSide = 0;
							GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.ThreeWay;
							if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.StopSign_AllWay){
								GSDRI.IgnoreCorner = 1;
							}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
								GSDRI.IgnoreCorner = 2;	
							}
							
							if(!oNode1.GSDRI.bFlipped){
								GSDRI.bNode2B_LeftTurnLane = false;
								GSDRI.bNode2F_RightTurnLane = false;
							}else{
								GSDRI.bNode2F_LeftTurnLane = false;
								GSDRI.bNode2B_RightTurnLane = false;
							}
							
						}else if(GSDRI.bSecondSpecial_Last){
							GSDRI.IgnoreSide = 2;
							GSDRI.iType = GSDRoadIntersection.IntersectionTypeEnum.ThreeWay;
							if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.StopSign_AllWay){
								GSDRI.IgnoreCorner = 3;
							}else if(GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight1 || GSDRI.iStopType == GSDRoadIntersection.iStopTypeEnum.TrafficLight2){
								GSDRI.IgnoreCorner = 0;	
							}
							
							if(!oNode1.GSDRI.bFlipped){
								GSDRI.bNode2B_LeftTurnLane = false;
								GSDRI.bNode2F_RightTurnLane = false;
							}else{
								GSDRI.bNode2F_LeftTurnLane = false;
								GSDRI.bNode2B_RightTurnLane = false;
							}
						}
					}
					
					//Find corners:
					Vector2 tFoundVectRR = default(Vector2);
					Vector2 tFoundVectRL = default(Vector2);
					Vector2 tFoundVectLR = default(Vector2);
					Vector2 tFoundVectLL = default(Vector2);
					if(!bOldMethod){
						//RR:
						if(!bFlipped){
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectRR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R_RightTurn, ref oNode2.iConstruction.tempconstruction_R);	
							}else{
								tFoundVectRR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R, ref oNode2.iConstruction.tempconstruction_R);
							}
						}else{
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectRR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R_RightTurn, ref oNode2.iConstruction.tempconstruction_L);
							}else{
								tFoundVectRR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R, ref oNode2.iConstruction.tempconstruction_L);
							}
						}
	
						//RL:
						if(!bFlipped){
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectRL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R, ref oNode2.iConstruction.tempconstruction_L_RightTurn);
							}else{
								tFoundVectRL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R, ref oNode2.iConstruction.tempconstruction_L);	
							}
						}else{
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectRL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R, ref oNode2.iConstruction.tempconstruction_R_RightTurn);
							}else{
								tFoundVectRL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_R, ref oNode2.iConstruction.tempconstruction_R);
							}
						}
	
						//LL:
						if(!bFlipped){
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectLL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L_RightTurn, ref oNode2.iConstruction.tempconstruction_L);
							}else{
								tFoundVectLL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L, ref oNode2.iConstruction.tempconstruction_L);
							}
						}else{
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectLL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L_RightTurn, ref oNode2.iConstruction.tempconstruction_R);
							}else{
								tFoundVectLL = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L, ref oNode2.iConstruction.tempconstruction_R);	
							}
						}
	
						//LR:
						if(!bFlipped){
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectLR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L, ref oNode2.iConstruction.tempconstruction_R_RightTurn);
							}else{
								tFoundVectLR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L, ref oNode2.iConstruction.tempconstruction_R);
							}
						}else{
							if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
								tFoundVectLR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L, ref oNode2.iConstruction.tempconstruction_L_RightTurn);
							}else{
								tFoundVectLR = IntersectionCornerCalc(ref oNode1.iConstruction.tempconstruction_L, ref oNode2.iConstruction.tempconstruction_L);	
							}
						}
					}else{
						//Now two lists of R and L on each intersection node, now match:
						float eDistanceRR = 5000f;
						float oDistanceRR = 0f;
						float eDistanceRL = 5000f;
						float oDistanceRL = 0f;
						float eDistanceLR = 5000f;
						float oDistanceLR = 0f;
						float eDistanceLL = 5000f;
						float oDistanceLL = 0f;
						bool bHasBeen1mRR = false;
						bool bHasBeen1mRL = false;
						bool bHasBeen1mLR = false;
						bool bHasBeen1mLL = false;
						bool bHasBeen1mRR_ignore = false;
						bool bHasBeen1mRL_ignore = false;
						bool bHasBeen1mLR_ignore = false;
						bool bHasBeen1mLL_ignore = false;
						bool bHasBeen1mRR_ignore_Max = false;
						bool bHasBeen1mRL_ignore_Max = false;
						bool bHasBeen1mLR_ignore_Max = false;
						bool bHasBeen1mLL_ignore_Max = false;
						float mMin = 0.2f;
						float mMax = 0.5f;
						
						MaxCount = Mathf.Max(tCounts);
						int MaxHCount = Mathf.Max(hCount1,hCount2);
						for(int h=0;h<MaxHCount;h++){
							bHasBeen1mRR = false;
							bHasBeen1mRL = false;
							bHasBeen1mLR = false;
							bHasBeen1mLL = false;
							bHasBeen1mRR_ignore = false;
							bHasBeen1mRL_ignore = false;
							bHasBeen1mLR_ignore = false;
							bHasBeen1mLL_ignore = false;
							for(int k=0;k<MaxCount;k++){
								if(!bFlipped){
									//RR:
									if(!bHasBeen1mRR_ignore_Max && !bHasBeen1mRR_ignore && (h < N1RCount && k < N2RCount)){
										oDistanceRR = Vector2.Distance(oNode1.iConstruction.tempconstruction_R[h],oNode2.iConstruction.tempconstruction_R[k]);
										if(oDistanceRR < eDistanceRR){
											eDistanceRR = oDistanceRR;
											tFoundVectRR = oNode1.iConstruction.tempconstruction_R[h]; //RR
											if(eDistanceRR < 0.07f){ bHasBeen1mRR_ignore_Max = true; }
										}
										if(oDistanceRR > mMax && bHasBeen1mRR){ bHasBeen1mRR_ignore = true; }
										if(oDistanceRR < mMin){ bHasBeen1mRR = true; }
									}
									//RL:
									if(!bHasBeen1mRL_ignore_Max && !bHasBeen1mRL_ignore && (h < N1RCount && k < N2LCount)){
										oDistanceRL = Vector2.Distance(oNode1.iConstruction.tempconstruction_R[h],oNode2.iConstruction.tempconstruction_L[k]);
										if(oDistanceRL < eDistanceRL){
											eDistanceRL = oDistanceRL;
											tFoundVectRL = oNode1.iConstruction.tempconstruction_R[h]; //RL
											if(eDistanceRL < 0.07f){ bHasBeen1mRL_ignore_Max = true; }
										}
										if(oDistanceRL > mMax && bHasBeen1mRL){ bHasBeen1mRL_ignore = true; }
										if(oDistanceRL < mMin){ bHasBeen1mRL = true; }
									}
									//LR:
									if(!bHasBeen1mLR_ignore_Max && !bHasBeen1mLR_ignore && (h < N1LCount && k < N2RCount)){
										oDistanceLR = Vector2.Distance(oNode1.iConstruction.tempconstruction_L[h],oNode2.iConstruction.tempconstruction_R[k]);
										if(oDistanceLR < eDistanceLR){
											eDistanceLR = oDistanceLR;
											tFoundVectLR = oNode1.iConstruction.tempconstruction_L[h]; //LR
											if(eDistanceLR < 0.07f){ bHasBeen1mLR_ignore_Max = true; }
										}
										if(oDistanceLR > mMax && bHasBeen1mLR){ bHasBeen1mLR_ignore = true; }
										if(oDistanceLR < mMin){ bHasBeen1mLR = true; }
									}
									//LL:
									if(!bHasBeen1mLL_ignore_Max && !bHasBeen1mLL_ignore && (h < N1LCount && k < N2LCount)){
										oDistanceLL = Vector2.Distance(oNode1.iConstruction.tempconstruction_L[h],oNode2.iConstruction.tempconstruction_L[k]);
										if(oDistanceLL < eDistanceLL){
											eDistanceLL = oDistanceLL;
											tFoundVectLL = oNode1.iConstruction.tempconstruction_L[h]; //LL
											if(eDistanceLL < 0.07f){ bHasBeen1mLL_ignore_Max = true; }
										}
										if(oDistanceLL > mMax && bHasBeen1mLL){ bHasBeen1mLL_ignore = true; }
										if(oDistanceLL < mMin){ bHasBeen1mLL = true; }
									}
								}else{
									//RR:
									if(!bHasBeen1mRR_ignore_Max && !bHasBeen1mRR_ignore && (h < N1RCount && k < N2LCount)){
										oDistanceRR = Vector2.Distance(oNode1.iConstruction.tempconstruction_R[h],oNode2.iConstruction.tempconstruction_L[k]);
										if(oDistanceRR < eDistanceRR){
											eDistanceRR = oDistanceRR;
											tFoundVectRR = oNode1.iConstruction.tempconstruction_R[h]; //RR
											if(eDistanceRR < 0.07f){ bHasBeen1mRR_ignore_Max = true; }
										}
										if(oDistanceRR > mMax && bHasBeen1mRR){ bHasBeen1mRR_ignore = true; }
										if(oDistanceRR < mMin){ bHasBeen1mRR = true; }
									}
									//RL:
									if(!bHasBeen1mRL_ignore_Max && !bHasBeen1mRL_ignore && (h < N1RCount && k < N2RCount)){
										oDistanceRL = Vector2.Distance(oNode1.iConstruction.tempconstruction_R[h],oNode2.iConstruction.tempconstruction_R[k]);
										if(oDistanceRL < eDistanceRL){
											eDistanceRL = oDistanceRL;
											tFoundVectRL = oNode1.iConstruction.tempconstruction_R[h]; //RL
											if(eDistanceRL < 0.07f){ bHasBeen1mRL_ignore_Max = true; }
										}
										if(oDistanceRL > mMax && bHasBeen1mRL){ bHasBeen1mRL_ignore = true; }
										if(oDistanceRL < mMin){ bHasBeen1mRL = true; }
									}
									//LR:
									if(!bHasBeen1mLR_ignore_Max && !bHasBeen1mLR_ignore && (h < N1LCount && k < N2LCount)){
										oDistanceLR = Vector2.Distance(oNode1.iConstruction.tempconstruction_L[h],oNode2.iConstruction.tempconstruction_L[k]);
										if(oDistanceLR < eDistanceLR){
											eDistanceLR = oDistanceLR;
											tFoundVectLR = oNode1.iConstruction.tempconstruction_L[h]; //LR
											if(eDistanceLR < 0.07f){ bHasBeen1mLR_ignore_Max = true; }
										}
										if(oDistanceLR > mMax && bHasBeen1mLR){ bHasBeen1mLR_ignore = true; }
										if(oDistanceLR < mMin){ bHasBeen1mLR = true; }
									}
									//LL:
									if(!bHasBeen1mLL_ignore_Max && !bHasBeen1mLL_ignore && (h < N1LCount && k < N2RCount)){
										oDistanceLL = Vector2.Distance(oNode1.iConstruction.tempconstruction_L[h],oNode2.iConstruction.tempconstruction_R[k]);
										if(oDistanceLL < eDistanceLL){
											eDistanceLL = oDistanceLL;
											tFoundVectLL = oNode1.iConstruction.tempconstruction_L[h]; //LL
											if(eDistanceLL < 0.07f){ bHasBeen1mLL_ignore_Max = true; }
										}
										if(oDistanceLL > mMax && bHasBeen1mLL){ bHasBeen1mLL_ignore = true; }
										if(oDistanceLL < mMin){ bHasBeen1mLL = true; }
									}
								}
							}
						}
					}

					oNode1.iConstruction.tempconstruction_HasProcessed_Inter2 = true;
					oNode2.iConstruction.tempconstruction_HasProcessed_Inter2 = true;
					
					Vector3 tVectRR = new Vector3(tFoundVectRR.x,0f,tFoundVectRR.y);
					Vector3 tVectRL = new Vector3(tFoundVectRL.x,0f,tFoundVectRL.y);
					Vector3 tVectLR = new Vector3(tFoundVectLR.x,0f,tFoundVectLR.y);
					Vector3 tVectLL = new Vector3(tFoundVectLL.x,0f,tFoundVectLL.y);
					
					oNode1.GSDRI.CornerRR = tVectRR;
					oNode1.GSDRI.CornerRL = tVectRL;
					oNode1.GSDRI.CornerLR = tVectLR;
					oNode1.GSDRI.CornerLL = tVectLL;
					
					float[] tMaxFloats = new float[4];
					tMaxFloats[0] = Vector3.Distance(((tVectRR-tVectRL)*0.5f)+tVectRL,oNode1.pos) * 1.25f;
					tMaxFloats[1] = Vector3.Distance(((tVectRR-tVectLR)*0.5f)+tVectLR,oNode1.pos) * 1.25f;
					tMaxFloats[2] = Vector3.Distance(((tVectRL-tVectLL)*0.5f)+tVectLL,oNode1.pos) * 1.25f;
					tMaxFloats[3] = Vector3.Distance(((tVectLR-tVectLL)*0.5f)+tVectLL,oNode1.pos) * 1.25f;
					GSDRI.MaxInterDistance = Mathf.Max(tMaxFloats);
					
					float[] tMaxFloatsSQ = new float[4];
					tMaxFloatsSQ[0] = Vector3.SqrMagnitude((((tVectRR-tVectRL)*0.5f)+tVectRL)-oNode1.pos) * 1.25f;
					tMaxFloatsSQ[1] = Vector3.SqrMagnitude((((tVectRR-tVectLR)*0.5f)+tVectLR)-oNode1.pos) * 1.25f;
					tMaxFloatsSQ[2] = Vector3.SqrMagnitude((((tVectRL-tVectLL)*0.5f)+tVectLL)-oNode1.pos) * 1.25f;
					tMaxFloatsSQ[3] = Vector3.SqrMagnitude((((tVectLR-tVectLL)*0.5f)+tVectLL)-oNode1.pos) * 1.25f;
					GSDRI.MaxInterDistanceSQ = Mathf.Max(tMaxFloatsSQ);

					float TotalLanes = (int)(RoadWidth/LaneWidth);
					float TotalLanesI = TotalLanes;
					float LanesPerSide = TotalLanes/2f;

					if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						TotalLanesI = TotalLanes + 2f;
						//Lower left to lower right: 
						GSDRI.fCornerLR_CornerRR = new Vector3[5];
						GSDRI.fCornerLR_CornerRR[0] = tVectLR;
						GSDRI.fCornerLR_CornerRR[1] = ((tVectRR - tVectLR)*(LanesPerSide/TotalLanesI))+tVectLR;
						GSDRI.fCornerLR_CornerRR[2] = ((tVectRR - tVectLR)*((LanesPerSide+1)/TotalLanesI))+tVectLR;
						GSDRI.fCornerLR_CornerRR[3] = ((tVectRR - tVectLR)*((LanesPerSide+1+LanesPerSide)/TotalLanesI))+tVectLR;
						GSDRI.fCornerLR_CornerRR[4] = tVectRR;
						//Upper right to lower right:
						GSDRI.fCornerRL_CornerRR = new Vector3[5];
						GSDRI.fCornerRL_CornerRR[0] = tVectRL;
						GSDRI.fCornerRL_CornerRR[1] = ((tVectRR - tVectRL)*(1/TotalLanesI))+tVectRL;
						GSDRI.fCornerRL_CornerRR[2] = ((tVectRR - tVectRL)*((LanesPerSide+1)/TotalLanesI))+tVectRL;
						GSDRI.fCornerRL_CornerRR[3] = ((tVectRR - tVectRL)*((LanesPerSide+2)/TotalLanesI))+tVectRL;
						GSDRI.fCornerRL_CornerRR[4] = tVectRR;
						//Upper left to upper right:
						GSDRI.fCornerLL_CornerRL = new Vector3[5];
						GSDRI.fCornerLL_CornerRL[0] = tVectLL;
						GSDRI.fCornerLL_CornerRL[1] = ((tVectRL - tVectLL)*(1/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerRL[2] = ((tVectRL - tVectLL)*((LanesPerSide+1)/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerRL[3] = ((tVectRL - tVectLL)*((LanesPerSide+2)/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerRL[4] = tVectRL;
						//Upper left to lower left:
						GSDRI.fCornerLL_CornerLR = new Vector3[5];
						GSDRI.fCornerLL_CornerLR[0] = tVectLL;
						GSDRI.fCornerLL_CornerLR[1] = ((tVectLR - tVectLL)*(LanesPerSide/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerLR[2] = ((tVectLR - tVectLL)*((LanesPerSide+1)/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerLR[3] = ((tVectLR - tVectLL)*((LanesPerSide+1+LanesPerSide)/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerLR[4] = tVectLR;
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
						TotalLanesI = TotalLanes + 1;
						//Lower left to lower right:
						GSDRI.fCornerLR_CornerRR = new Vector3[4];
						GSDRI.fCornerLR_CornerRR[0] = tVectLR;
						GSDRI.fCornerLR_CornerRR[1] = ((tVectRR - tVectLR)*(LanesPerSide/TotalLanesI))+tVectLR;
						GSDRI.fCornerLR_CornerRR[2] = ((tVectRR - tVectLR)*((LanesPerSide+1)/TotalLanesI))+tVectLR;
						GSDRI.fCornerLR_CornerRR[3] = tVectRR;
						//Upper right to lower right:
						GSDRI.fCornerRL_CornerRR = new Vector3[4];
						GSDRI.fCornerRL_CornerRR[0] = tVectRL;
						GSDRI.fCornerRL_CornerRR[1] = ((tVectRR - tVectRL)*(LanesPerSide/TotalLanesI))+tVectRL;
						GSDRI.fCornerRL_CornerRR[2] = ((tVectRR - tVectRL)*((LanesPerSide+1)/TotalLanesI))+tVectRL;
						GSDRI.fCornerRL_CornerRR[3] = tVectRR;
						//Upper left to upper right:
						GSDRI.fCornerLL_CornerRL = new Vector3[4];
						GSDRI.fCornerLL_CornerRL[0] = tVectLL;
						GSDRI.fCornerLL_CornerRL[1] = ((tVectRL - tVectLL)*(LanesPerSide/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerRL[2] = ((tVectRL - tVectLL)*((LanesPerSide+1)/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerRL[3] = tVectRL;
						//Upper left to lower left:
						GSDRI.fCornerLL_CornerLR = new Vector3[4];
						GSDRI.fCornerLL_CornerLR[0] = tVectLL;
						GSDRI.fCornerLL_CornerLR[1] = ((tVectLR - tVectLL)*(LanesPerSide/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerLR[2] = ((tVectLR - tVectLL)*((LanesPerSide+1)/TotalLanesI))+tVectLL;
						GSDRI.fCornerLL_CornerLR[3] = tVectLR;
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
						TotalLanesI = TotalLanes + 0;
						//Lower left to lower right:
						GSDRI.fCornerLR_CornerRR = new Vector3[3];
						GSDRI.fCornerLR_CornerRR[0] = tVectLR;
						GSDRI.fCornerLR_CornerRR[1] = ((tVectRR - tVectLR)*0.5f)+tVectLR;
						GSDRI.fCornerLR_CornerRR[2] = tVectRR;
						//Upper right to lower right:
						GSDRI.fCornerRL_CornerRR = new Vector3[3];
						GSDRI.fCornerRL_CornerRR[0] = tVectRL;
						GSDRI.fCornerRL_CornerRR[1] = ((tVectRR - tVectRL)*0.5f)+tVectRL;
						GSDRI.fCornerRL_CornerRR[2] = tVectRR;
						//Upper left to upper right:
						GSDRI.fCornerLL_CornerRL = new Vector3[3];
						GSDRI.fCornerLL_CornerRL[0] = tVectLL;
						GSDRI.fCornerLL_CornerRL[1] = ((tVectRL - tVectLL)*0.5f)+tVectLL;
						GSDRI.fCornerLL_CornerRL[2] = tVectRL;
						//Upper left to lower left:
						GSDRI.fCornerLL_CornerLR = new Vector3[3];
						GSDRI.fCornerLL_CornerLR[0] = tVectLL;
						GSDRI.fCornerLL_CornerLR[1] = ((tVectLR - tVectLL)*0.5f)+tVectLL;
						GSDRI.fCornerLL_CornerLR[2] = tVectLR;
					}

					//Use node1/node2 for angles instead
					float tShoulderWidth = ShoulderWidth * 1.75f;
					float tRampWidth = ShoulderWidth * 2f;
					
					oNode1.GSDRI.OddAngle = Vector3.Angle(GSDRI.Node2.tangent,GSDRI.Node1.tangent);
					oNode1.GSDRI.EvenAngle = 180f - Vector3.Angle(GSDRI.Node2.tangent,GSDRI.Node1.tangent);

				 	GSD.Roads.GSDIntersectionObjects.GetFourPoints(GSDRI,out GSDRI.CornerRR_Outer, out GSDRI.CornerRL_Outer, out GSDRI.CornerLL_Outer, out GSDRI.CornerLR_Outer, tShoulderWidth);
					GSD.Roads.GSDIntersectionObjects.GetFourPoints(GSDRI,out GSDRI.CornerRR_RampOuter, out GSDRI.CornerRL_RampOuter, out GSDRI.CornerLL_RampOuter, out GSDRI.CornerLR_RampOuter, tRampWidth);
					
					GSDRI.ConstructBoundsRect();
					GSDRI.CornerRR_2D = new Vector2(tVectRR.x,tVectRR.z);
					GSDRI.CornerRL_2D = new Vector2(tVectRL.x,tVectRL.z);
					GSDRI.CornerLL_2D = new Vector2(tVectLL.x,tVectLL.z);
					GSDRI.CornerLR_2D = new Vector2(tVectLR.x,tVectLR.z);
					
					if(!oNode1.GSDRI.bSameSpline){
						if(string.Compare(tRoad.GSDSpline.UID,oNode1.GSDSpline.tRoad.GSDSpline.UID) != 0){
							AddIntersectionBounds(ref oNode1.GSDSpline.tRoad, ref tRoad.RCS);
						}else if(string.Compare(tRoad.GSDSpline.UID,oNode2.GSDSpline.tRoad.GSDSpline.UID) != 0){
							AddIntersectionBounds(ref oNode2.GSDSpline.tRoad, ref tRoad.RCS);
						}
					}
				}
			}
		}
		
		private static Vector2 IntersectionCornerCalc(ref List<Vector2> PrimaryList,ref List<Vector2> SecondaryList){
			int PrimaryCount = PrimaryList.Count;
			int SecondaryCount = SecondaryList.Count;
			Vector2 t2D_Line1Start = default(Vector2);
			Vector2 t2D_Line1End = default(Vector2);
			Vector2 t2D_Line2Start = default(Vector2);
			Vector2 t2D_Line2End = default(Vector2);
			bool bDidIntersect = false;
			Vector2 tIntersectLocation = default(Vector2);
			for(int i=1;i<PrimaryCount;i++){
					bDidIntersect = false;
					t2D_Line1Start = PrimaryList[i-1];
					t2D_Line1End = PrimaryList[i];
					for(int k=1;k<SecondaryCount;k++){
						bDidIntersect = false;
						t2D_Line2Start = SecondaryList[k-1];
						t2D_Line2End = SecondaryList[k];
						bDidIntersect = GSDRootUtil.Intersects2D(ref t2D_Line1Start,ref t2D_Line1End,ref t2D_Line2Start,ref t2D_Line2End, out tIntersectLocation);
						if(bDidIntersect){ 
							return tIntersectLocation;	
						}
					}
				}
			return tIntersectLocation;
		}
		
		private static void AddIntersectionBounds(ref GSDRoad tRoad, ref GSD.Roads.RoadConstructorBufferMaker RCS){
			bool bIsBridge = false;
			bool bBridgeInitial = false; if(bBridgeInitial == false){ }
			bool bTempbridge = false;
			bool bBridgeLast = false; if(bBridgeLast == false){ }
			
			bool bIsTunnel = false;
			bool bTunnelInitial = false; if(bTunnelInitial == false){ }
			bool bTempTunnel = false;
			bool bTunnelLast = false; if(bTunnelLast == false){ }
			
			GSDRoadIntersection GSDRI = null;
			bool bIsPastInter = false;
			bool bMaxIntersection = false;
			bool bWasPrevMaxInter = false;
			Vector3 tVect = default(Vector3);
			Vector3 POS = default(Vector3);
			float tIntHeight = 0f;
			float tIntStrength = 0f;
			float tIntStrength_temp = 0f;
//			float tIntDistCheck = 75f;
			bool bFirstInterNode = false;
			Vector3 tVect_Prev = default(Vector3); 		if(tVect_Prev == default(Vector3)){ }
			Vector3 rVect_Prev = default(Vector3); 		if(rVect_Prev == default(Vector3)){ }
			Vector3 lVect_Prev = default(Vector3); 		if(lVect_Prev == default(Vector3)){ }
			Vector3 rVect = default(Vector3);			if(rVect == default(Vector3)){ }
			Vector3 lVect = default(Vector3);			if(lVect == default(Vector3)){ }
			Vector3 ShoulderR_rVect = default(Vector3);	if(ShoulderR_rVect == default(Vector3)){ }
			Vector3 ShoulderR_lVect = default(Vector3);	if(ShoulderR_lVect == default(Vector3)){ }
			Vector3 ShoulderL_rVect = default(Vector3);	if(ShoulderL_rVect == default(Vector3)){ }
			Vector3 ShoulderL_lVect = default(Vector3);	if(ShoulderL_lVect == default(Vector3)){ }
			
			Vector3 RampR_R = default(Vector3);
			Vector3 RampR_L = default(Vector3);
			Vector3 RampL_R = default(Vector3);
			Vector3 RampL_L = default(Vector3);
			float ShoulderR_OuterAngle = 0f;	if(ShoulderR_OuterAngle < 0f){ }
			float ShoulderL_OuterAngle = 0f;	if(ShoulderL_OuterAngle < 0f){ }
			Vector3 ShoulderR_PrevLVect = default(Vector3); if(ShoulderR_PrevLVect == default(Vector3)){ }
			Vector3 ShoulderL_PrevRVect = default(Vector3); if(ShoulderL_PrevRVect == default(Vector3)){ }
			Vector3 ShoulderR_PrevRVect = default(Vector3); if(ShoulderR_PrevRVect == default(Vector3)){ }
			Vector3 ShoulderL_PrevLVect = default(Vector3); if(ShoulderL_PrevLVect == default(Vector3)){ }
//			Vector3 ShoulderR_PrevRVect2 = default(Vector3);
//			Vector3 ShoulderL_PrevLVect2 = default(Vector3);
//			Vector3 ShoulderR_PrevRVect3 = default(Vector3);
//			Vector3 ShoulderL_PrevLVect3 = default(Vector3);
			Vector3 RampR_PrevR = default(Vector3); if(RampR_PrevR == default(Vector3)){ }
			Vector3 RampR_PrevL = default(Vector3); if(RampR_PrevL == default(Vector3)){ }
			Vector3 RampL_PrevR = default(Vector3); if(RampL_PrevR == default(Vector3)){ }
			Vector3 RampL_PrevL = default(Vector3); if(RampL_PrevL == default(Vector3)){ }
			GSDSplineC tSpline = tRoad.GSDSpline;
			//Road width:
			float RoadWidth = tRoad.RoadWidth();
			float ShoulderWidth = tRoad.opt_ShoulderWidth;
			float RoadSeperation = RoadWidth / 2f;
			float RoadSeperation_NoTurn = RoadWidth / 2f;
			float ShoulderSeperation = RoadSeperation + ShoulderWidth;
			float LaneWidth = tRoad.opt_LaneWidth;
			float RoadSep1Lane = (RoadSeperation + (LaneWidth*0.5f));
			float RoadSep2Lane = (RoadSeperation + (LaneWidth*1.5f));
			float ShoulderSep1Lane = (ShoulderSeperation + (LaneWidth*0.5f));		if(ShoulderSep1Lane < 0f){ }
			float ShoulderSep2Lane = (ShoulderSeperation + (LaneWidth*1.5f));		if(ShoulderSep2Lane < 0f){ }
			
//			float tAngle = 0f;
//			float OrigStep = 0.06f;
			float Step = tRoad.opt_RoadDefinition / tSpline.distance;
			
			GSDSplineN xNode = null;
			float tInterSubtract = 4f;
			float tLastInterHeight = -4f;
			
//			GameObject xObj = null;
//			xObj = GameObject.Find("temp22");
//			while(xObj != null){
//				Object.DestroyImmediate(xObj);
//				xObj = GameObject.Find("temp22");
//			}
//			xObj = GameObject.Find("temp23");
//			while(xObj != null){
//				Object.DestroyImmediate(xObj);
//				xObj = GameObject.Find("temp23");
//			}
//			xObj = GameObject.Find("temp22_RR");
//			while(xObj != null){
//				Object.DestroyImmediate(xObj);
//				xObj = GameObject.Find("temp22_RR");
//			}
//			xObj = GameObject.Find("temp22_RL");
//			while(xObj != null){
//				Object.DestroyImmediate(xObj);
//				xObj = GameObject.Find("temp22_RL");
//			}
//			xObj = GameObject.Find("temp22_LR");
//			while(xObj != null){
//				Object.DestroyImmediate(xObj);
//				xObj = GameObject.Find("temp22_LR");
//			}
//			xObj = GameObject.Find("temp22_LL");
//			while(xObj != null){
//				Object.DestroyImmediate(xObj);
//				xObj = GameObject.Find("temp22_LL");
//			}
			
			bool bFinalEnd = false;
			float i = 0f;
			
			float FinalMax = 1f;
			float StartMin = 0f;
			if(tSpline.bSpecialEndControlNode){
				FinalMax = tSpline.mNodes[tSpline.GetNodeCount()-2].tTime;
			}
			if(tSpline.bSpecialStartControlNode){
				StartMin = tSpline.mNodes[1].tTime;
			}
			
//			int StartIndex = tSpline.GetClosestRoadDefIndex(StartMin,true,false);
//			int EndIndex = tSpline.GetClosestRoadDefIndex(FinalMax,false,true);
//			float cDist = 0f;
			bool kSkip = true;
			bool kSkipFinal = false;
			int kCount = 0;
			int kFinalCount = tSpline.RoadDefKeysArray.Length;
			int spamcheckmax1 = 18000;
			int spamcheck1 = 0;
			
			if(IsApproximately(StartMin,0f,0.0001f)){
				kSkip = false;
			}
			if(IsApproximately(FinalMax,1f,0.0001f)){
				kSkipFinal = true;
			}
			
			while(!bFinalEnd && spamcheck1 < spamcheckmax1){
				spamcheck1++;
				
				if(kSkip){
					i = StartMin;
					kSkip = false;	
				}else{
					if(kCount >= kFinalCount){
						i = FinalMax;
						if(kSkipFinal){ break; }
					}else{
						i = tSpline.TranslateInverseParamToFloat(tSpline.RoadDefKeysArray[kCount]);
						kCount+=1;
					}
				}
				
				if(i > 1f){ break; }
				if(i < 0f){ i = 0f; }
				
				if(IsApproximately(i,FinalMax,0.00001f)){
					bFinalEnd = true;
				}else if(i > FinalMax){
					if(tSpline.bSpecialEndControlNode){
						i = FinalMax;
						bFinalEnd = true;
					}else{
						bFinalEnd = true;
						break;	
					}
				}

				tSpline.GetSplineValue_Both(i,out tVect,out POS);
				bIsPastInter = false;
				tIntStrength = tSpline.IntersectionStrength(ref tVect,ref tIntHeight, ref GSDRI, ref bIsPastInter, ref i, ref xNode);
				if(IsApproximately(tIntStrength,1f,0.001f) || tIntStrength > 1f){
					bMaxIntersection = true;
				}else{
					bMaxIntersection = false;	
				}	
				
				if(bMaxIntersection){
					if(string.Compare(xNode.UID,GSDRI.Node1.UID) == 0){
						bFirstInterNode = true;
					}else{
						bFirstInterNode = false;
					}
					
					//Convoluted for initial trigger:
					bBridgeInitial = false;
					bBridgeLast = false;
					bTempbridge = tSpline.IsInBridge(i);
					if(!bIsBridge && bTempbridge){
						bIsBridge = true;
						bBridgeInitial = true;
					}else if(bIsBridge && !bTempbridge){
						bIsBridge = false;
					}
					//Check if this is the last bridge run for this bridge:
					if(bIsBridge){
						bTempbridge = tSpline.IsInBridge(i+Step);
						if(!bTempbridge){
							bBridgeLast = true;	
						}
					}	
					
					
					//Convoluted for initial trigger:
					bTunnelInitial = false;
					bTunnelLast = false;
					bTempTunnel = tSpline.IsInTunnel(i);
					if(!bIsTunnel && bTempTunnel){
						bIsTunnel = true;
						bTunnelInitial = true;
					}else if(bIsTunnel && !bTempTunnel){
						bIsTunnel = false;
					}
					//Check if this is the last Tunnel run for this Tunnel:
					if(bIsTunnel){
						bTempTunnel = tSpline.IsInTunnel(i+Step);
						if(!bTempTunnel){
							bTunnelLast = true;	
						}
					}	
					
					if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
						rVect = (tVect + new Vector3(RoadSeperation_NoTurn*POS.normalized.z,0,RoadSeperation_NoTurn*-POS.normalized.x));
						lVect = (tVect + new Vector3(RoadSeperation_NoTurn*-POS.normalized.z,0,RoadSeperation_NoTurn*POS.normalized.x));
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
						rVect = (tVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
						lVect = (tVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						if(bIsPastInter){
							rVect = (tVect + new Vector3(RoadSep1Lane*POS.normalized.z,0,RoadSep1Lane*-POS.normalized.x));
							lVect = (tVect + new Vector3(RoadSep2Lane*-POS.normalized.z,0,RoadSep2Lane*POS.normalized.x));;
						}else{
							rVect = (tVect + new Vector3(RoadSep2Lane*POS.normalized.z,0,RoadSep2Lane*-POS.normalized.x));
							lVect = (tVect + new Vector3(RoadSep1Lane*-POS.normalized.z,0,RoadSep1Lane*POS.normalized.x));
						}
					}else{
						rVect = (tVect + new Vector3(RoadSeperation*POS.normalized.z,0,RoadSeperation*-POS.normalized.x));
						lVect = (tVect + new Vector3(RoadSeperation*-POS.normalized.z,0,RoadSeperation*POS.normalized.x));
					}

					if(tIntStrength >= 1f){
						tVect.y -= tInterSubtract;
						tLastInterHeight = tVect.y;
						rVect.y -= tInterSubtract;
						lVect.y -= tInterSubtract;
					}else{
						if(!IsApproximately(tIntStrength,0f,0.001f)){ tVect.y = (tIntStrength*tIntHeight) + ((1-tIntStrength)*tVect.y); }
						tIntStrength_temp = tRoad.GSDSpline.IntersectionStrength(ref rVect,ref tIntHeight, ref GSDRI,ref bIsPastInter,ref i, ref xNode);
						if(!IsApproximately(tIntStrength_temp,0f,0.001f)){ rVect.y = (tIntStrength_temp*tIntHeight) + ((1-tIntStrength_temp)*rVect.y); ShoulderR_lVect = rVect; }
					}
					
					//Add bounds for later removal:
					GSD.Roads.GSDRoadUtil.Construction2DRect vRect = null;
					if(!bIsBridge && !bIsTunnel && bMaxIntersection && bWasPrevMaxInter){
						bool bGoAhead = true;
						if(xNode.bIsEndPoint){
							if(xNode.idOnSpline == 1){
								if(i < xNode.tTime){
									bGoAhead = false;
								}
							}else{
								if(i > xNode.tTime){
									bGoAhead = false;
								}
							}
						}
						//Get this and prev lvect rvect rects:
						if(Vector3.Distance(xNode.pos,tVect) < (3f * RoadWidth) && bGoAhead){
							if(GSDRI.bFlipped && !bFirstInterNode){
								vRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(
									new Vector2(rVect.x,rVect.z),
									new Vector2(lVect.x,lVect.z),
									new Vector2(rVect_Prev.x,rVect_Prev.z), 
									new Vector2(lVect_Prev.x,lVect_Prev.z),
									tLastInterHeight
									);
							}else{
								 vRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(
									new Vector2(lVect.x,lVect.z),
									new Vector2(rVect.x,rVect.z),
									new Vector2(lVect_Prev.x,lVect_Prev.z),
									new Vector2(rVect_Prev.x,rVect_Prev.z), 
									tLastInterHeight
									);
							}
//							GameObject tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//							tObj.transform.position = lVect;
//							tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//							tObj.transform.name = "temp22";
//							
//							tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//							tObj.transform.position = rVect;
//							tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
//							tObj.transform.name = "temp22";
							
							RCS.tIntersectionBounds.Add(vRect);
						}
					}
				}
				
				bWasPrevMaxInter = bMaxIntersection;
				tVect_Prev = tVect;
				rVect_Prev = rVect;
				lVect_Prev = lVect;
				ShoulderR_PrevLVect = ShoulderR_lVect;
				ShoulderL_PrevRVect = ShoulderL_rVect;
//				ShoulderR_PrevRVect3 = ShoulderR_PrevRVect2;
//				ShoulderL_PrevLVect3 = ShoulderL_PrevLVect2;
//				ShoulderR_PrevRVect2 = ShoulderR_PrevRVect;
//				ShoulderL_PrevLVect2 = ShoulderL_PrevLVect;
				ShoulderR_PrevRVect = ShoulderR_rVect;
				ShoulderL_PrevLVect = ShoulderL_lVect;
				RampR_PrevR = RampR_R;
				RampR_PrevL = RampR_L;
				RampL_PrevR = RampL_R;
				RampL_PrevL = RampL_L;
//				i+=Step; 
			}
		}
		#endregion
		
		#region "Intersection Prelim Finalization"		
		private static void RoadJob_Prelim_FinalizeInter(ref GSDRoad tRoad){
			int mCount = tRoad.GSDSpline.GetNodeCount();
			GSDSplineN tNode;
			for(int i=0;i<mCount;i++){
				tNode = tRoad.GSDSpline.mNodes[i];
				if(tNode.bIsIntersection){
					Inter_OrganizeVertices(ref tNode,ref tRoad);
					tNode.iConstruction.Nullify();
					tNode.iConstruction = null;
				}
			}
		}
		
		private static bool Inter_OrganizeVerticesMatchEdges(ref List<Vector3> tList1,ref List<Vector3> tList2, bool bSkip1 = false, bool bSkipFirstListOne = false, bool bSkipBoth = false){
			List<Vector3> PrimaryList;
			List<Vector3> SecondaryList;
			
			List<Vector3> tList1New;
			List<Vector3> tList2New;

			if(bSkip1){
				if(bSkipBoth){
					tList1New = new List<Vector3>();
					tList2New = new List<Vector3>();
					for(int i=1;i<tList1.Count;i++){
						tList1New.Add(tList1[i]);
					}
					for(int i=1;i<tList2.Count;i++){
						tList2New.Add(tList2[i]);
					}
				}else{
					if(bSkipFirstListOne){
						tList1New = new List<Vector3>();
						for(int i=1;i<tList1.Count;i++){
							tList1New.Add(tList1[i]);
						}
						tList2New = tList2;
					}else{
						tList2New = new List<Vector3>();
						for(int i=1;i<tList2.Count;i++){
							tList2New.Add(tList2[i]);
						}
						tList1New = tList1;
					}
				}
			}else{
				tList1New = tList1;
				tList2New = tList2;
			}
			
			int tList1Count = tList1New.Count;
			int tList2Count = tList2New.Count;
			if(tList1Count == tList2Count){ return false; }
			
			if(tList1Count > tList2Count){
				PrimaryList = tList1New;
				SecondaryList = tList2New;
			}else{
				PrimaryList = tList2New;	
				SecondaryList = tList1New;
			}
			
			if(SecondaryList == null || SecondaryList.Count == 0){ return true; }
			SecondaryList.Clear();
			SecondaryList = null;
			SecondaryList = new List<Vector3>();
			for(int i=0;i<PrimaryList.Count;i++){
				SecondaryList.Add(PrimaryList[i]);	
			}
			
			
			if(tList1Count > tList2Count){
				tList2 = SecondaryList;
			}else{
				tList1 = SecondaryList;
			}
			
			return false;
		}
		
		private static void Inter_OrganizeVerticesMatchShoulder(ref List<Vector3> tShoulderList,ref List<Vector3> tToMatch, int StartI, ref Vector3 StartVec, ref Vector3 EndVect, float tHeight, bool bIsF = false){
//			return;
			List<Vector3> BackupList = new List<Vector3>();
			for(int i=0;i<tToMatch.Count;i++){
				BackupList.Add(tToMatch[i]);
			}
			Vector2 t2D = default(Vector2);
			Vector2 t2D_Start = ConvertVect3_To_Vect2(StartVec);
			Vector2 t2D_End = ConvertVect3_To_Vect2(EndVect);
			int RealStartID = -1;
			StartI = StartI - 30;
			if(StartI < 0){ StartI = 0; }
			for(int i=StartI;i<tShoulderList.Count;i++){
				t2D = ConvertVect3_To_Vect2(tShoulderList[i]);
//				if(t2D.x > 745f && t2D.x < 755f && t2D.y > 1240f && t2D.y < 1250f){
//					int agfsdajgsd = 1;	
//				}
				if(t2D == t2D_Start){
				//if(tShoulderList[i] == StartVec){
					RealStartID = i;
					break;
				}
			}

			tToMatch.Clear(); tToMatch = null;
			tToMatch = new List<Vector3>();
			
			int spamcounter = 0;
			bool bBackup = false;
			if(RealStartID == -1){
				bBackup = true;	
			}
			
			if(!bBackup){
				if(bIsF){
					for(int i=RealStartID;i>0;i-=8){
						t2D = ConvertVect3_To_Vect2(tShoulderList[i]);
						tToMatch.Add(tShoulderList[i]);
						if(t2D == t2D_End){
						//if(tShoulderList[i] == EndVect){
							break;
						}
						spamcounter+=1;
						if(spamcounter > 100){
							bBackup = true;
							break;	
						}
					}
				}else{
					for(int i=RealStartID;i<tShoulderList.Count;i+=8){
						t2D = ConvertVect3_To_Vect2(tShoulderList[i]);
						tToMatch.Add(tShoulderList[i]);
						if(t2D == t2D_End){
						//if(tShoulderList[i] == EndVect){
							break;
						}
						spamcounter+=1;
						if(spamcounter > 100){
							bBackup = true;
							break;	
						}
					}
				}
			}
////			
//			if(!bBackup){
//				for(int i=0;i<tToMatch.Count;i++){
//					tToMatch[i] = new Vector3(tToMatch[i].x,tHeight,tToMatch[i].z);
//				}
//			}
//			
//			//Backup if above fails:
//			if(bBackup){
//				tToMatch.Clear();
//				tToMatch = new List<Vector3>();
//				for(int i=0;i<BackupList.Count;i++){
//					tToMatch.Add(BackupList[i]);
//				}
//			}
		}
		
		private static void Inter_OrganizeVertices(ref GSDSplineN tNode, ref GSDRoad tRoad){
			GSD.Roads.GSDIntersections.iConstructionMaker iCon = tNode.iConstruction;
			GSDRoadIntersection GSDRI = tNode.GSDRI;
			
			//Skipping (3 ways):
			bool bSkipF = false;
			if(iCon.iFLane0L.Count == 0){
				bSkipF = true;
			}
			bool bSkipB = false;
			if(iCon.iBLane0L.Count == 0){
				bSkipB = true;
			}
			
			//Is primary node and is first node on a spline, meaning t junction: It does not have a B:
			if(tNode.idOnSpline == 0 && string.CompareOrdinal(GSDRI.Node1UID,tNode.UID) == 0){
				bSkipB = true;
			}
			//Is primary node and is last node on a spline, meaning t junction: It does not have a F:
			if(tNode.idOnSpline == (tNode.GSDSpline.GetNodeCount()-1) && string.CompareOrdinal(GSDRI.Node1UID,tNode.UID) == 0){
				bSkipF = true;
			}
			
			//Other node is t junction end node, meaning now we figure out which side we're on
			if(tNode.Intersection_OtherNode.idOnSpline == 0 || tNode.idOnSpline == (tNode.GSDSpline.GetNodeCount()-1)){
				
			}
			
			//Reverse all fronts:
			if(!bSkipF){
				iCon.iFLane0L.Reverse();
				iCon.iFLane0R.Reverse();
	
				iCon.iFLane1L.Reverse();
				iCon.iFLane2L.Reverse();
				iCon.iFLane3L.Reverse();
				iCon.iFLane1R.Reverse();
				iCon.iFLane2R.Reverse();
				iCon.iFLane3R.Reverse();
				
				if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					iCon.ShoulderFR_Start = iCon.iFLane0L[0];
					iCon.ShoulderFL_Start = iCon.iFLane3R[0];
				}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					iCon.ShoulderFR_Start = iCon.iFLane0L[0];
					iCon.ShoulderFL_Start = iCon.iFLane2R[0];
				}else{
					iCon.ShoulderFR_Start = iCon.iFLane0L[0];
					iCon.ShoulderFL_Start = iCon.iFLane1R[0];
				}
			}
			
			if(!bSkipB){
				if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					iCon.ShoulderBL_End = iCon.iBLane0L[iCon.iBLane0L.Count-1];
					iCon.ShoulderBR_End = iCon.iBLane3R[iCon.iBLane3R.Count-1];
				}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					iCon.ShoulderBL_End = iCon.iBLane0L[iCon.iBLane0L.Count-1];
					iCon.ShoulderBR_End = iCon.iBLane2R[iCon.iBLane2R.Count-1];
				}else{
					iCon.ShoulderBL_End = iCon.iBLane0L[iCon.iBLane0L.Count-1];
					iCon.ShoulderBR_End = iCon.iBLane1R[iCon.iBLane1R.Count-1];
				}
			}
			
			if(!bSkipB){
				Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderL_Vectors,ref iCon.iBLane0L,iCon.ShoulderBL_StartIndex,ref iCon.ShoulderBL_Start, ref iCon.ShoulderBL_End, GSDRI.Height);
				if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderR_Vectors,ref iCon.iBLane3R,iCon.ShoulderBR_StartIndex,ref iCon.ShoulderBR_Start, ref iCon.ShoulderBR_End, GSDRI.Height);
				}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderR_Vectors,ref iCon.iBLane2R,iCon.ShoulderBR_StartIndex,ref iCon.ShoulderBR_Start, ref iCon.ShoulderBR_End, GSDRI.Height);
				}else{
					Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderR_Vectors,ref iCon.iBLane1R,iCon.ShoulderBR_StartIndex,ref iCon.ShoulderBR_Start, ref iCon.ShoulderBR_End, GSDRI.Height);
				}
			}
			
			if(!bSkipF){
				Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderR_Vectors,ref iCon.iFLane0L,iCon.ShoulderFR_StartIndex,ref iCon.ShoulderFR_Start, ref iCon.ShoulderFR_End, GSDRI.Height,true);
				if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderL_Vectors,ref iCon.iFLane3R,iCon.ShoulderFL_StartIndex,ref iCon.ShoulderFL_Start, ref iCon.ShoulderFL_End, GSDRI.Height,true);
				}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderL_Vectors,ref iCon.iFLane2R,iCon.ShoulderFL_StartIndex,ref iCon.ShoulderFL_Start, ref iCon.ShoulderFL_End, GSDRI.Height,true);
				}else{
					Inter_OrganizeVerticesMatchShoulder(ref tRoad.RCS.ShoulderL_Vectors,ref iCon.iFLane1R,iCon.ShoulderFL_StartIndex,ref iCon.ShoulderFL_Start, ref iCon.ShoulderFL_End, GSDRI.Height,true);
				}
			}

			bool bError = false;
			string tWarning = "Intersection " + GSDRI.tName + " in road " + tRoad.tName + " at too extreme angle to process this intersection type. Reduce angle or reduce lane count.";
			
			if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
				if(!bSkipB){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iBLane0R, ref iCon.iBLane1L); if(bError){ Debug.Log(tWarning); } }
				if(!bSkipF){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iFLane0R, ref iCon.iFLane1L); if(bError){ Debug.Log(tWarning); } }
			}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				if(!bSkipB){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iBLane0R, ref iCon.iBLane1L); if(bError){ Debug.Log(tWarning); } }
				if(!bSkipF){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iFLane0R, ref iCon.iFLane1L); if(bError){ Debug.Log(tWarning); } }		
				
				if(!bSkipB){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iBLane1R, ref iCon.iBLane2L); if(bError){ Debug.Log(tWarning); } }
				if(!bSkipF){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iFLane1R, ref iCon.iFLane2L); if(bError){ Debug.Log(tWarning); } }	
			}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				if(!bSkipB){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iBLane0R, ref iCon.iBLane1L); if(bError){ Debug.Log(tWarning); } }
				if(!bSkipF){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iFLane0R, ref iCon.iFLane1L); if(bError){ Debug.Log(tWarning); } }	
				
				if(!bSkipB){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iBLane1R, ref iCon.iBLane2L,true,true); if(bError){ Debug.Log(tWarning); } }
				if(!bSkipF){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iFLane1R, ref iCon.iFLane2L,true,true); if(bError){ Debug.Log(tWarning); } }
				
//				if(!bSkipB){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iBLane2R, ref iCon.iBLane3L,true,false); if(bError){ Debug.Log(tWarning); } }
//				if(!bSkipF){ bError = Inter_OrganizeVerticesMatchEdges(ref iCon.iFLane2R, ref iCon.iFLane3L,true,false); if(bError){ Debug.Log(tWarning); } }
			}

			//Back main plate left:
			int mCount = -1;
			if(!bSkipB){ 
				mCount = iCon.iBLane0L.Count;
				for(int m=0;m<mCount;m++){
					iCon.iBMainPlateL.Add(iCon.iBLane0L[m]);
				}
			}
			//Front main plate left:
			if(!bSkipF){
				mCount = iCon.iFLane0L.Count;
				for(int m=0;m<mCount;m++){
					iCon.iFMainPlateL.Add(iCon.iFLane0L[m]);
				}
			}
			
			//Back main plate right:
			if(!bSkipB){ 
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
					mCount = iCon.iBLane1R.Count;
					for(int m=0;m<mCount;m++){
						iCon.iBMainPlateR.Add(iCon.iBLane1R[m]);
					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					mCount = iCon.iBLane2R.Count;
					for(int m=0;m<mCount;m++){
						iCon.iBMainPlateR.Add(iCon.iBLane2R[m]);
					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					mCount = iCon.iBLane3R.Count;
					for(int m=0;m<mCount;m++){
						iCon.iBMainPlateR.Add(iCon.iBLane3R[m]);
					}
				}
			}
			
			//Front main plate right:
			if(!bSkipF){
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
					mCount = iCon.iFLane1R.Count;
					for(int m=0;m<mCount;m++){
						iCon.iFMainPlateR.Add(iCon.iFLane1R[m]);
					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					mCount = iCon.iFLane2R.Count;
					for(int m=0;m<mCount;m++){
						iCon.iFMainPlateR.Add(iCon.iFLane2R[m]);
					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					mCount = iCon.iFLane3R.Count;
					for(int m=0;m<mCount;m++){
						iCon.iFMainPlateR.Add(iCon.iFLane3R[m]);
					}
				}
			}
			
			mCount = tRoad.RCS.RoadVectors.Count;
//			float mDistance = 0.05f;
			Vector3 tVect = default(Vector3);
			
			bool biBLane0L = (iCon.iBLane0L.Count > 0);				if(biBLane0L == false){ }
			bool biBLane0R = (iCon.iBLane0R.Count > 0);				if(biBLane0R == false){ }
			bool biBMainPlateL = (iCon.iBMainPlateL.Count > 0);		if(biBMainPlateL == false){ }
			bool biBMainPlateR = (iCon.iBMainPlateR.Count > 0);		if(biBMainPlateR == false){ }
			bool biFLane0L = (iCon.iFLane0L.Count > 0);				if(biFLane0L == false){ }
			bool biFLane0R = (iCon.iFLane0R.Count > 0);				if(biFLane0R == false){ }
			bool biFMainPlateL = (iCon.iFMainPlateL.Count > 0);		if(biFMainPlateL == false){ }
			bool biFMainPlateR = (iCon.iFMainPlateR.Count > 0);		if(biFMainPlateR == false){ }
			bool biBLane2L = (iCon.iBLane2L.Count > 0);				if(biBLane2L == false){ }
			bool biBLane2R = (iCon.iBLane2R.Count > 0);				if(biBLane2R == false){ }
			bool biFLane2L = (iCon.iFLane2L.Count > 0);				if(biFLane2L == false){ }
			bool biFLane2R = (iCon.iFLane2R.Count > 0);				if(biFLane2R == false){ }
			bool biBLane3L = (iCon.iBLane3L.Count > 0);				if(biBLane3L == false){ }
			bool biBLane3R = (iCon.iBLane3R.Count > 0);				if(biBLane3R == false){ }
			bool biFLane3L = (iCon.iFLane3L.Count > 0);				if(biFLane3L == false){ }
			bool biFLane3R = (iCon.iFLane3R.Count > 0);				if(biFLane3R == false){ }

			mCount = tRoad.RCS.RoadVectors.Count;
			int cCount = tRoad.GSDSpline.GetNodeCount();
			int tStartI = 0;
			int tEndI = mCount;
			//Start and end the next loop after this one later for opt:
			if(cCount > 2){
				if(!tRoad.GSDSpline.mNodes[0].bIsIntersection && !tRoad.GSDSpline.mNodes[1].bIsIntersection){
					for(int i=2;i<cCount;i++){
						if(tRoad.GSDSpline.mNodes[i].bIsIntersection){
							if(i-2 >= 1){
								tStartI = (int)(tRoad.GSDSpline.mNodes[i-2].tTime * mCount);
							}
							break;
						}
					}
				}
			}
			if(cCount > 3){
				if(!tRoad.GSDSpline.mNodes[cCount-1].bIsIntersection && !tRoad.GSDSpline.mNodes[cCount-2].bIsIntersection){
					for(int i=(cCount-3);i>=0;i--){
						if(tRoad.GSDSpline.mNodes[i].bIsIntersection){
							if(i+2 < cCount){
								tEndI = (int)(tRoad.GSDSpline.mNodes[i+2].tTime * mCount);
							}
							break;
						}
					}
				}
			}
			
			if(tStartI > 0){
				if(tStartI % 2 != 0){
					tStartI += 1;	
				}
			}
			if(tStartI > mCount){ tStartI=mCount-4; }
			if(tStartI < 0){ tStartI = 0; }
			if(tEndI < mCount){
				if(tEndI % 2 != 0){
					tEndI += 1;
				}
			}
			if(tEndI > mCount){ tEndI=mCount-4; }
			if(tEndI < 0){ tEndI = 0; }
			
			for(int i=tStartI;i<tEndI;i+=2){
				tVect = tRoad.RCS.RoadVectors[i];
				for(int j=0;j<1;j++){
					if(biBLane0L && Vector3.SqrMagnitude(tVect-iCon.iBLane0L[j]) < 0.01f && !bSkipB){
						iCon.iBLane0L[j] = tVect;
					}
					if(biBMainPlateL && Vector3.SqrMagnitude(tVect-iCon.iBMainPlateL[j]) < 0.01f && !bSkipB){
						iCon.iBMainPlateL[j] = tVect;
					}
					if(biBMainPlateR && Vector3.SqrMagnitude(tVect-iCon.iBMainPlateR[j]) < 0.01f && !bSkipB){
						iCon.iBMainPlateR[j] = tVect;
					}
					if(biFLane0L && Vector3.SqrMagnitude(tVect-iCon.iFLane0L[j]) < 0.01f && !bSkipF){
						iCon.iFLane0L[j] = tVect;
					}
					if(biFMainPlateL && Vector3.SqrMagnitude(tVect-iCon.iFMainPlateL[j]) < 0.01f && !bSkipF){
						iCon.iFMainPlateL[j] = tVect;
					}
					if(biFMainPlateR && Vector3.SqrMagnitude(tVect-iCon.iFMainPlateR[j]) < 0.01f && !bSkipF){
						iCon.iFMainPlateR[j] = tVect;
					}
					if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						if(biBLane3L && Vector3.SqrMagnitude(tVect-iCon.iBLane3L[j]) < 0.01f && !bSkipB){
							iCon.iBLane3L[j] = tVect;
						}
						if(biBLane3R && Vector3.SqrMagnitude(tVect-iCon.iBLane3R[j]) < 0.01f && !bSkipB){
							iCon.iBLane3R[j] = tVect;
						}
						if(biFLane3L && Vector3.SqrMagnitude(tVect-iCon.iFLane3L[j]) < 0.01f && !bSkipF){
							iCon.iFLane3L[j] = tVect;
						}
						if(biFLane3R && Vector3.SqrMagnitude(tVect-iCon.iFLane3R[j]) < 0.01f && !bSkipF){
							iCon.iFLane3R[j] = tVect;
						}
					}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
						if(biBLane2L && Vector3.SqrMagnitude(tVect-iCon.iBLane2L[j]) < 0.01f && !bSkipB){
							iCon.iBLane2L[j] = tVect;
						}
						if(biBLane2R && Vector3.SqrMagnitude(tVect-iCon.iBLane2R[j]) < 0.01f && !bSkipB){
							iCon.iBLane2R[j] = tVect;
						}
						if(biFLane2L && Vector3.SqrMagnitude(tVect-iCon.iFLane2L[j]) < 0.01f && !bSkipF){
							iCon.iFLane2L[j] = tVect;
						}
						if(biFLane2R && Vector3.SqrMagnitude(tVect-iCon.iFLane2R[j]) < 0.01f && !bSkipF){
							iCon.iFLane2R[j] = tVect;
						}
					}
				}
			}
		
//			float b0 = -1f;
//			float f0 = -1f;
//			
//			if(!bSkipB){ b0 = iCon.iBMainPlateL[0].y; }
//			if(!bSkipF){ f0 = iCon.iFMainPlateL[0].y; }
//			
//			if(iCon.iBLane0R == null || iCon.iBLane0R.Count == 0){
//				bSkipB = true;	
//			}
			if(iCon.iBMainPlateR == null || iCon.iBMainPlateR.Count == 0){
				bSkipB = true;	
			}
			if(iCon.iBMainPlateL == null || iCon.iBMainPlateL.Count == 0){
				bSkipB = true;	
			}
			
			if(!bSkipB){ iCon.iBLane0R[0] = ((iCon.iBMainPlateR[0]-iCon.iBMainPlateL[0])*0.5f+iCon.iBMainPlateL[0]); }
			if(!bSkipF){ iCon.iFLane0R[0] = ((iCon.iFMainPlateR[0]-iCon.iFMainPlateL[0])*0.5f+iCon.iFMainPlateL[0]); }
			
//			if(tNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){ 
				if(!bSkipB){ 
					iCon.iBLane1L[0] = iCon.iBLane0R[0];
					iCon.iBLane1R[0] = new Vector3(iCon.iBLane1R[0].x,iCon.iBLane1L[0].y,iCon.iBLane1R[0].z);
				}
			
				if(!bSkipF){ 
					iCon.iFLane1L[0] = iCon.iFLane0R[0];
					iCon.iFLane1R[0] = new Vector3(iCon.iFLane1R[0].x,iCon.iFLane1L[0].y,iCon.iFLane1R[0].z);
				}
//			}
			
			if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				if(!bSkipB){ iCon.iBLane3L[0] = new Vector3(iCon.iBLane3L[0].x,iCon.iBLane3R[0].y,iCon.iBLane3L[0].z); }
				if(!bSkipF){ iCon.iFLane3L[0] = new Vector3(iCon.iFLane3L[0].x,iCon.iFLane3R[0].y,iCon.iFLane3L[0].z); }
			}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				if(!bSkipB){ iCon.iBLane2L[0] = new Vector3(iCon.iBLane2L[0].x,iCon.iBLane2R[0].y,iCon.iBLane2L[0].z); }
				if(!bSkipF){ iCon.iFLane2L[0] = new Vector3(iCon.iFLane2L[0].x,iCon.iFLane2R[0].y,iCon.iFLane2L[0].z); }
			}
			
			List<Vector3> iBLane0 = null;
			List<Vector3> iBLane1 = null;
			List<Vector3> iBLane2 = null;
			List<Vector3> iBLane3 = null;
			if(!bSkipB){ 
				iBLane0 = InterVertices(iCon.iBLane0L,iCon.iBLane0R, tNode.GSDRI.Height);
				iBLane1 = InterVertices(iCon.iBLane1L,iCon.iBLane1R, tNode.GSDRI.Height);
				if(tNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){ iBLane2 = InterVertices(iCon.iBLane2L,iCon.iBLane2R, tNode.GSDRI.Height); }
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){ iBLane3 = InterVertices(iCon.iBLane3L,iCon.iBLane3R, tNode.GSDRI.Height); }
			}
			
			//Front lanes:
			List<Vector3> iFLane0 = null;
			List<Vector3> iFLane1 = null;
			List<Vector3> iFLane2 = null;
			List<Vector3> iFLane3 = null;
			if(!bSkipF){ 
				iFLane0 = InterVertices(iCon.iFLane0L,iCon.iFLane0R, tNode.GSDRI.Height);
				iFLane1 = InterVertices(iCon.iFLane1L,iCon.iFLane1R, tNode.GSDRI.Height);
				if(tNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){ iFLane2 = InterVertices(iCon.iFLane2L,iCon.iFLane2R, tNode.GSDRI.Height); }
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){ iFLane3 = InterVertices(iCon.iFLane3L,iCon.iFLane3R, tNode.GSDRI.Height); }
			}
				
			//Main plates:
			List<Vector3> iBMainPlate = null;
			List<Vector3> iFMainPlate = null;
			if(!bSkipB){ 
				iBMainPlate = InterVertices(iCon.iBMainPlateL,iCon.iBMainPlateR, tNode.GSDRI.Height);
			}
			if(!bSkipF){ 
				iFMainPlate = InterVertices(iCon.iFMainPlateL,iCon.iFMainPlateR, tNode.GSDRI.Height);
			}
//			//Marker plates:
//			List<Vector3> iBMarkerPlate = InterVertices(iCon.iBMarkerPlateL,iCon.iBMarkerPlateR, tNode.GSDRI.Height);
//			List<Vector3> iFMarkerPlate = InterVertices(iCon.iFMarkerPlateL,iCon.iFMarkerPlateR, tNode.GSDRI.Height);
//			
			//Now add these to RCS:
			if(!bSkipB){ 
				tRoad.RCS.iBLane0s.Add(iBLane0.ToArray());
				tRoad.RCS.iBLane0s_tID.Add(GSDRI);
				tRoad.RCS.iBLane0s_nID.Add(tNode);
				tRoad.RCS.iBLane1s.Add(iBLane1.ToArray());
				tRoad.RCS.iBLane1s_tID.Add(GSDRI);
				tRoad.RCS.iBLane1s_nID.Add(tNode);
				if(tNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
                    if (iBLane2 != null) { 
					    tRoad.RCS.iBLane2s.Add(iBLane2.ToArray()); 
					    tRoad.RCS.iBLane2s_tID.Add(GSDRI);
					    tRoad.RCS.iBLane2s_nID.Add(tNode);
                    }
				}
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){ 
					tRoad.RCS.iBLane3s.Add(iBLane3.ToArray()); 
					tRoad.RCS.iBLane3s_tID.Add(GSDRI);
					tRoad.RCS.iBLane3s_nID.Add(tNode);
				}
			}
			//Front lanes:
			if(!bSkipF){ 
				tRoad.RCS.iFLane0s.Add(iFLane0.ToArray());
				tRoad.RCS.iFLane0s_tID.Add(GSDRI);
				tRoad.RCS.iFLane0s_nID.Add(tNode);
				tRoad.RCS.iFLane1s.Add(iFLane1.ToArray());
				tRoad.RCS.iFLane1s_tID.Add(GSDRI);
				tRoad.RCS.iFLane1s_nID.Add(tNode);
				if(tNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){ 
					tRoad.RCS.iFLane2s.Add(iFLane2.ToArray()); 
					tRoad.RCS.iFLane2s_tID.Add(GSDRI);
					tRoad.RCS.iFLane2s_nID.Add(tNode);
				}
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){ 
					tRoad.RCS.iFLane3s.Add(iFLane3.ToArray()); 
					tRoad.RCS.iFLane3s_tID.Add(GSDRI);
					tRoad.RCS.iFLane3s_nID.Add(tNode);
				}
			}
			//Main plates:
			if(iBMainPlate != null && !bSkipB){ 
				tRoad.RCS.iBMainPlates.Add(iBMainPlate.ToArray()); 
				tRoad.RCS.iBMainPlates_tID.Add(GSDRI);
				tRoad.RCS.iBMainPlates_nID.Add(tNode);
			}
			if(iFMainPlate != null && !bSkipF){ 
				tRoad.RCS.iFMainPlates.Add(iFMainPlate.ToArray()); 
				tRoad.RCS.iFMainPlates_tID.Add(GSDRI);
				tRoad.RCS.iFMainPlates_nID.Add(tNode);
			}
//			//Marker plates:
//			tRoad.RCS.iBMarkerPlates.Add(iBMarkerPlate.ToArray());
//			tRoad.RCS.iFMarkerPlates.Add(iFMarkerPlate.ToArray());
//			tRoad.RCS.IntersectionTypes.Add((int)tNode.GSDRI.rType);
			
			if(tNode.GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
				if(!bSkipB){ tRoad.RCS.iBLane1s_IsMiddleLane.Add(true);	}
				if(!bSkipF){ tRoad.RCS.iFLane1s_IsMiddleLane.Add(true);	}
			}else{
				if(!bSkipB){ tRoad.RCS.iBLane1s_IsMiddleLane.Add(false); }
				if(!bSkipF){ tRoad.RCS.iFLane1s_IsMiddleLane.Add(false); }
			}
		}
	
		private static bool IsVecSame(ref Vector3 tVect1,Vector3 tVect2){
			return ((Vector3.SqrMagnitude(tVect1-tVect2) < 0.01f));	
		}
		
		private static List<Vector3> InterVertices(List<Vector3> tL, List<Vector3> tR, float tHeight){
			if(tL.Count == 0 || tR.Count == 0){ return null;	}
			
			List<Vector3> tList = new List<Vector3>();
			int tCountL = tL.Count;
			int tCountR = tR.Count;
			int spamcheck = 0;
			
			while(tCountL < tCountR && spamcheck < 5000){
				tL.Add(tL[tCountL-1]);
				tCountL = tL.Count;
				spamcheck+=1;
			}
			
			spamcheck=0;
			while(tCountR < tCountL && spamcheck < 5000){
				tR.Add(tR[tCountR-1]);
				tCountR = tR.Count;
				spamcheck+=1;
			}
			
			if(spamcheck > 4000){
				Debug.LogWarning("spamcheck InterVertices");
			}
			
			int tCount = Mathf.Max(tCountL,tCountR);
			for(int i=0;i<tCount;i++){
				tList.Add(tL[i]);	
				tList.Add(tL[i]);
				tList.Add(tR[i]);	
				tList.Add(tR[i]);
			}
			return tList;
		}
		#endregion
		
		/// <summary>
		/// Handles most triangles and normals construction. In certain scenarios for efficiency reasons UV might also be processed.
		/// </summary>
		/// <param name='RCS'>
		/// The road construction buffer, by reference.
		/// </param>/
		public static void RoadJob1(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			//Triangles and normals:
//			if(RCS.tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("ProcessRoad_IntersectionCleanup"); }
			if(RCS.bInterseOn){ ProcessRoad_IntersectionCleanup(ref RCS); }
//			if(RCS.tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
			
			ProcessRoad_Tris_Bulk(ref RCS);
			
			RCS.tris_ShoulderR = ProcessRoad_Tris_Shoulder(RCS.ShoulderR_Vectors.Count);
			RCS.tris_ShoulderL = ProcessRoad_Tris_Shoulder(RCS.ShoulderL_Vectors.Count);
			if(RCS.tRoad.opt_bShoulderCuts || RCS.tRoad.opt_bDynamicCuts){
				ProcessRoad_Tris_ShoulderCutsR(ref RCS);
				ProcessRoad_Tris_ShoulderCutsL(ref RCS);
			}
			
			ProcessRoad_Normals_Bulk(ref RCS);
			ProcessRoad_Normals_Shoulders(ref RCS);
		}
		
		/// <summary>
		/// Handles most UV and tangent construction. Some scenarios might involve triangles and normals or lack UV construction for efficiency reasons.
		/// </summary>
		/// <param name='RCS'>
		/// The road construction buffer, by reference.
		/// </param>
		public static void RoadJob2(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			//Bridge UV is processed with tris and normals.
			
			//For one big road mesh:
			if(RCS.bRoadOn){
			    if(!RCS.tMeshSkip){ RCS.uv = ProcessRoad_UVs(RCS.RoadVectors.ToArray()); }
				if(!RCS.tMesh_SRSkip){ RCS.uv_SR = ProcessRoad_UVs_Shoulder(RCS.ShoulderR_Vectors.ToArray()); }
				if(!RCS.tMesh_SLSkip){ RCS.uv_SL = ProcessRoad_UVs_Shoulder(RCS.ShoulderL_Vectors.ToArray()); }
				
				//UVs for pavement:
				if(!RCS.tMeshSkip){ 
					int vCount = RCS.RoadVectors.Count;
					RCS.uv2 = new Vector2[vCount];
					for(int i=0;i<vCount;i++){
						RCS.uv2[i] = new Vector2(RCS.RoadVectors[i].x*0.2f,RCS.RoadVectors[i].z*0.2f);
					}
				}
			}
			
			//For road cuts:
			if(RCS.tRoad.opt_bRoadCuts || RCS.tRoad.opt_bDynamicCuts){
				ProcessRoad_UVs_RoadCuts(ref RCS);	
				int cCount = RCS.cut_RoadVectors.Count;
				for(int i=0;i<cCount;i++){
					RCS.cut_tangents.Add(GSDRootUtil.ProcessTangents(RCS.cut_tris[i], RCS.cut_normals[i], RCS.cut_uv[i], RCS.cut_RoadVectors[i].ToArray()));
					RCS.cut_tangents_world.Add(GSDRootUtil.ProcessTangents(RCS.cut_tris[i], RCS.cut_normals[i], RCS.cut_uv_world[i], RCS.cut_RoadVectors[i].ToArray()));
				}
			}
			if(RCS.tRoad.opt_bShoulderCuts || RCS.tRoad.opt_bDynamicCuts){
				int rCount = RCS.cut_ShoulderR_Vectors.Count;
				for(int i=0;i<rCount;i++){
					ProcessRoad_UVs_ShoulderCut(ref RCS,false,i);
					RCS.cut_tangents_SR.Add(GSDRootUtil.ProcessTangents(RCS.cut_tris_ShoulderR[i], RCS.cut_normals_ShoulderR[i], RCS.cut_uv_SR[i], RCS.cut_ShoulderR_Vectors[i].ToArray()));
					RCS.cut_tangents_SR_world.Add(GSDRootUtil.ProcessTangents(RCS.cut_tris_ShoulderR[i], RCS.cut_normals_ShoulderR[i], RCS.cut_uv_SR_world[i], RCS.cut_ShoulderR_Vectors[i].ToArray()));
				}
				int lCount = RCS.cut_ShoulderL_Vectors.Count;
				for(int i=0;i<lCount;i++){
					ProcessRoad_UVs_ShoulderCut(ref RCS,true,i);
					RCS.cut_tangents_SL.Add(GSDRootUtil.ProcessTangents(RCS.cut_tris_ShoulderL[i], RCS.cut_normals_ShoulderL[i], RCS.cut_uv_SL[i], RCS.cut_ShoulderL_Vectors[i].ToArray()));
					RCS.cut_tangents_SL_world.Add(GSDRootUtil.ProcessTangents(RCS.cut_tris_ShoulderL[i], RCS.cut_normals_ShoulderL[i], RCS.cut_uv_SL_world[i], RCS.cut_ShoulderL_Vectors[i].ToArray()));
				}
			}
			if(RCS.bInterseOn){
				ProcessRoad_UVs_Intersections(ref RCS);
			}
			
//						throw new System.Exception("FFFFFFFF");
			
			if(RCS.bRoadOn){
				if(!RCS.tMeshSkip){ RCS.tangents = GSDRootUtil.ProcessTangents(RCS.tris, RCS.normals, RCS.uv, RCS.RoadVectors.ToArray()); }
				if(!RCS.tMeshSkip){ RCS.tangents2 = GSDRootUtil.ProcessTangents(RCS.tris, RCS.normals, RCS.uv2, RCS.RoadVectors.ToArray()); }
				if(!RCS.tMesh_SRSkip){ RCS.tangents_SR = GSDRootUtil.ProcessTangents(RCS.tris_ShoulderR, RCS.normals_ShoulderR, RCS.uv_SR, RCS.ShoulderR_Vectors.ToArray()); }
				if(!RCS.tMesh_SLSkip){ RCS.tangents_SL = GSDRootUtil.ProcessTangents(RCS.tris_ShoulderL, RCS.normals_ShoulderL, RCS.uv_SL, RCS.ShoulderL_Vectors.ToArray()); }
				for(int i=0;i<RCS.tMesh_RoadConnections.Count;i++){
					RCS.RoadConnections_tangents.Add(GSDRootUtil.ProcessTangents(RCS.RoadConnections_tris[i], RCS.RoadConnections_normals[i], RCS.RoadConnections_uv[i], RCS.RoadConnections_verts[i]));
				}
			}

			if(RCS.bInterseOn){
				//Back lanes:
				int vCount = RCS.iBLane0s.Count;
				for(int i=0;i<vCount;i++){
				 	RCS.iBLane0s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iBLane0s_tris[i],RCS.iBLane0s_normals[i],RCS.iBLane0s_uv[i],RCS.iBLane0s[i]));
				}
				vCount = RCS.iBLane1s.Count;
				for(int i=0;i<vCount;i++){
					RCS.iBLane1s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iBLane1s_tris[i],RCS.iBLane1s_normals[i],RCS.iBLane1s_uv[i],RCS.iBLane1s[i]));
				}
				vCount = RCS.iBLane2s.Count;
				for(int i=0;i<vCount;i++){
					RCS.iBLane2s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iBLane2s_tris[i],RCS.iBLane2s_normals[i],RCS.iBLane2s_uv[i],RCS.iBLane2s[i]));
				}
				vCount = RCS.iBLane3s.Count;
				for(int i=0;i<vCount;i++){
					RCS.iBLane3s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iBLane3s_tris[i],RCS.iBLane3s_normals[i],RCS.iBLane3s_uv[i],RCS.iBLane3s[i]));
				}
				//Front lanes:
				vCount = RCS.iFLane0s.Count;
				for(int i=0;i<vCount;i++){
					RCS.iFLane0s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iFLane0s_tris[i],RCS.iFLane0s_normals[i],RCS.iFLane0s_uv[i],RCS.iFLane0s[i]));
				}
				vCount = RCS.iFLane1s.Count;
				for(int i=0;i<vCount;i++){
					RCS.iFLane1s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iFLane1s_tris[i],RCS.iFLane1s_normals[i],RCS.iFLane1s_uv[i],RCS.iFLane1s[i]));
				}
				vCount = RCS.iFLane2s.Count;
				for(int i=0;i<vCount;i++){
					RCS.iFLane2s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iFLane2s_tris[i],RCS.iFLane2s_normals[i],RCS.iFLane2s_uv[i],RCS.iFLane2s[i]));
				}
				vCount = RCS.iFLane3s.Count;
				for(int i=0;i<vCount;i++){
					RCS.iFLane3s_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iFLane3s_tris[i],RCS.iFLane3s_normals[i],RCS.iFLane3s_uv[i],RCS.iFLane3s[i]));
				}
				//Main plates:
				vCount = RCS.iBMainPlates.Count;
				for(int i=0;i<vCount;i++){
					RCS.iBMainPlates_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iBMainPlates_tris[i],RCS.iBMainPlates_normals[i],RCS.iBMainPlates_uv[i],RCS.iBMainPlates[i]));
				}
				vCount = RCS.iBMainPlates.Count;
				for(int i=0;i<vCount;i++){
					RCS.iBMainPlates_tangents2.Add(GSDRootUtil.ProcessTangents(RCS.iBMainPlates_tris[i],RCS.iBMainPlates_normals[i],RCS.iBMainPlates_uv2[i],RCS.iBMainPlates[i]));
				}
				vCount = RCS.iFMainPlates.Count;
				for(int i=0;i<vCount;i++){
					RCS.iFMainPlates_tangents.Add(GSDRootUtil.ProcessTangents(RCS.iFMainPlates_tris[i],RCS.iFMainPlates_normals[i],RCS.iFMainPlates_uv[i],RCS.iFMainPlates[i]));
				}
				vCount = RCS.iFMainPlates.Count;
				for(int i=0;i<vCount;i++){
					RCS.iFMainPlates_tangents2.Add(GSDRootUtil.ProcessTangents(RCS.iFMainPlates_tris[i],RCS.iFMainPlates_normals[i],RCS.iFMainPlates_uv2[i],RCS.iFMainPlates[i]));
				}
			}
		}
		
		#region "Intersection Cleanup"
		private static void ProcessRoad_IntersectionCleanup(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			List<GSD.Roads.GSDRoadUtil.Construction2DRect> tList = RCS.tIntersectionBounds;
			int mCount = tList.Count;
			RCS.ShoulderR_Vectors = ProcessRoad_IntersectionCleanup_Helper(ref RCS.ShoulderR_Vectors,ref tList,mCount,ref RCS.ImmuneVects);
			RCS.ShoulderL_Vectors = ProcessRoad_IntersectionCleanup_Helper(ref RCS.ShoulderL_Vectors,ref tList,mCount,ref RCS.ImmuneVects);
		}
		
		private static List<Vector3> ProcessRoad_IntersectionCleanup_Helper(ref List<Vector3> tVects, ref List<GSD.Roads.GSDRoadUtil.Construction2DRect> tList, int mCount, ref HashSet<Vector3> ImmuneVects){
			GSD.Roads.GSDRoadUtil.Construction2DRect tRect = null;
			int MVL = tVects.Count;
			//Vector3 tVect = default(Vector3);
			Vector2 Vect2D = default(Vector2);
			Vector2 tNearVect = default(Vector2);
			float tMax2 = 2000f;
			float tMax2SQ = 0f;
//			GameObject tObj = GameObject.Find("Inter1");
//			Vector2 tObj2D = ConvertVect3_To_Vect2(tObj.transform.position);
//			int fCount = 0;
//			bool bTempNow = false;
			for(int i=0;i<mCount;i++){
				tRect = tList[i];
				tMax2 = tRect.MaxDistance * 1.5f;
				tMax2SQ = (tMax2*tMax2);
				
//				Debug.Log (tRect.ToString());
				
				for(int j=0;j<MVL;j++){
					Vect2D.x = tVects[j].x;
					Vect2D.y = tVects[j].z;
	
					if(Vector2.SqrMagnitude(Vect2D-tRect.P1) > tMax2SQ){
						j+=32;
						continue;
					}
					
//					if(Vector2.Distance(Vect2D,tObj2D) < 20f && (j % 16 == 0)){
//						fCount+=1;
//						GameObject xObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//							xObj.transform.localScale = new Vector3(0.05f,40f,0.05f);
//							xObj.transform.position = tVects[j];
//							xObj.name = "temp22";
//					}
					
//					bTempNow = false;
					if(tRect.Contains(ref Vect2D)){
						if(ImmuneVects.Contains(tVects[j])){ continue; }
//						if(Vect2D == tRect.P1){
//							continue;
//						}else if(Vect2D == tRect.P2){
//							continue;
//						}else if(Vect2D == tRect.P3){
//							continue;
//						}else if(Vect2D == tRect.P4){
//							continue;
//						}
						

//						if(Mathf.Approximately(tVects[j].x,303.1898f)){
//							GameObject hObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//							hObj.transform.localScale = new Vector3(0.05f,40f,0.05f);
//							hObj.transform.position = tVects[j];
//							hObj.name = "temp23";
//							bTempNow = true;
//							Debug.Log (tVects[j]);
//						}
						
						//Calling near when it shouldn't ?
						if(tRect.Near(ref Vect2D,out tNearVect)){	//If near the rect, set it equal
							tVects[j] = new Vector3(tNearVect.x,tVects[j].y,tNearVect.y);
						}else{
							tVects[j] = new Vector3(tVects[j].x,tRect.Height,tVects[j].z);
						}
                    
                            
                        
             

						//ImmuneVects.Add(tVects[j]);
						
//						if(bTempNow){
//							GameObject xObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//							xObj.transform.localScale = new Vector3(0.05f,40f,0.05f);
//							xObj.transform.position = tVects[j];
//							xObj.name = "temp22";
//							Debug.Log ("to: " + tVects[j]);
//						}
					}
				}
			}
//			Debug.Log ("Fcount: " + fCount);
			
			return tVects;
		}
		#endregion
		
		#region "Tris"
		private static void ProcessRoad_Tris_Bulk(ref GSD.Roads.RoadConstructorBufferMaker RCS){//, ref Mesh tShoulderR, ref Mesh tShoulderL){
			//Next come the triangles. Since we want two triangles, each defined by three integers, the triangles array will have six elements in total. 
			//Remembering the clockwise rule for ordering the corners, the lower left triangle will use 0, 2, 1 as its corner indices, while the upper right one will use 2, 3, 1. 
			
			RCS.tris = ProcessRoad_Tris_Bulk_Helper(RCS.RoadVectors.Count);
			if(RCS.tRoad.opt_bRoadCuts || RCS.tRoad.opt_bDynamicCuts){
				ProcessRoad_Tris_RoadCuts(ref RCS);	
			}

			if(RCS.bInterseOn){
				//For intersection parts:
				//Back lanes:
				ProcessRoad_Tris_iProcessor(ref RCS.iBLane0s_tris,ref RCS.iBLane0s);
				ProcessRoad_Tris_iProcessor(ref RCS.iBLane1s_tris,ref RCS.iBLane1s);
				ProcessRoad_Tris_iProcessor(ref RCS.iBLane2s_tris,ref RCS.iBLane2s);
				ProcessRoad_Tris_iProcessor(ref RCS.iBLane3s_tris,ref RCS.iBLane3s);
				//Front lanes:
				ProcessRoad_Tris_iProcessor(ref RCS.iFLane0s_tris,ref RCS.iFLane0s);
				ProcessRoad_Tris_iProcessor(ref RCS.iFLane1s_tris,ref RCS.iFLane1s);
				ProcessRoad_Tris_iProcessor(ref RCS.iFLane2s_tris,ref RCS.iFLane2s);
				ProcessRoad_Tris_iProcessor(ref RCS.iFLane3s_tris,ref RCS.iFLane3s);
				//Main plates:
				ProcessRoad_Tris_iProcessor(ref RCS.iBMainPlates_tris,ref RCS.iBMainPlates);
				ProcessRoad_Tris_iProcessor(ref RCS.iFMainPlates_tris,ref RCS.iFMainPlates);
			}
		}

		private static int[] ProcessRoad_Tris_Bulk_Helper(int MVL){
			int TriCount = 0;
			int x1,x2,x3;
			int xCount = (int)(MVL*0.25f*6)-6;
//			if(xCount < 0){ xCount = 0; }
			int[] tri = new int[xCount];
			for(int i=0;i<MVL;i+=4){ 
				if(i+4 == MVL){ break; }
				
				x1 = i;
				x2 = i+4;
				x3 = i+2;
				
				tri[TriCount] = x1;	TriCount+=1;
				tri[TriCount] = x2;	TriCount+=1;
				tri[TriCount] = x3;	TriCount+=1;
					
				x1 = i+4;
				x2 = i+6;
				x3 = i+2;
	
				tri[TriCount] = x1;	TriCount+=1;
				tri[TriCount] = x2;	TriCount+=1;
				tri[TriCount] = x3;	TriCount+=1;
			}
			return tri;
		}
		
		private static void ProcessRoad_Tris_RoadCuts(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			//Road cuts aren't working right for the special nodes on cuts
			int cCount = RCS.RoadCuts.Count;
			int PrevRoadCutIndex = 0;
			int CurrentRoadCutIndex = 0;
			List<List<Vector3>> tVects = new List<List<Vector3>>();
			List<Vector3> tVectListSingle = null;
			Vector3 xVect = default(Vector3);
			for(int j=0;j<cCount;j++){
				CurrentRoadCutIndex = RCS.RoadCuts[j];
				tVectListSingle = new List<Vector3>();
				RCS.cut_RoadVectorsHome.Add(RCS.RoadVectors[PrevRoadCutIndex]);
				xVect = RCS.RoadVectors[PrevRoadCutIndex];
				for(int i=PrevRoadCutIndex;i<CurrentRoadCutIndex;i++){
					tVectListSingle.Add(RCS.RoadVectors[i]-xVect);
				}
				tVects.Add(tVectListSingle);
				PrevRoadCutIndex = CurrentRoadCutIndex-4;
				if(PrevRoadCutIndex < 0){ PrevRoadCutIndex = 0; }
			}
			int mMax = RCS.RoadVectors.Count;
			tVectListSingle = new List<Vector3>();
			RCS.cut_RoadVectorsHome.Add(RCS.RoadVectors[PrevRoadCutIndex]);
			xVect = RCS.RoadVectors[PrevRoadCutIndex];
			for(int i=PrevRoadCutIndex;i<mMax;i++){
				tVectListSingle.Add(RCS.RoadVectors[i]-xVect);
			}
			tVects.Add(tVectListSingle);

			int vCount = tVects.Count;
			List<int[]> tTris = new List<int[]>();
			for(int i=0;i<vCount;i++){
				int[] tTriSingle = ProcessRoad_Tris_Bulk_Helper(tVects[i].Count);
				tTris.Add(tTriSingle);
			}

			RCS.cut_RoadVectors = tVects;
			RCS.cut_tris = tTris;
		}
		
		private static void ProcessRoad_Tris_ShoulderCutsR(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			int cCount = RCS.ShoulderCutsR.Count;
			int PrevRoadCutIndex = 0;
			int CurrentRoadCutIndex = 0;
			List<List<Vector3>> tVects = new List<List<Vector3>>();
			List<Vector3> tVectListSingle = null;
			Vector3 xVect = default(Vector3);
			for(int j=0;j<cCount;j++){
				CurrentRoadCutIndex = RCS.ShoulderCutsR[j];
				tVectListSingle = new List<Vector3>();
				RCS.cut_ShoulderR_VectorsHome.Add(RCS.ShoulderR_Vectors[PrevRoadCutIndex]);
				xVect = RCS.ShoulderR_Vectors[PrevRoadCutIndex];
				for(int i=PrevRoadCutIndex;i<CurrentRoadCutIndex;i++){
					tVectListSingle.Add(RCS.ShoulderR_Vectors[i]-xVect);
				}
				tVects.Add(tVectListSingle);
				PrevRoadCutIndex = CurrentRoadCutIndex-8;
				if(PrevRoadCutIndex < 0){ PrevRoadCutIndex = 0; }
			}
			int mMax = RCS.ShoulderR_Vectors.Count;
			tVectListSingle = new List<Vector3>();
			RCS.cut_ShoulderR_VectorsHome.Add(RCS.ShoulderR_Vectors[PrevRoadCutIndex]);
			xVect = RCS.ShoulderR_Vectors[PrevRoadCutIndex];
			for(int i=PrevRoadCutIndex;i<mMax;i++){
				tVectListSingle.Add(RCS.ShoulderR_Vectors[i]-xVect);
			}
			tVects.Add(tVectListSingle);

			int vCount = tVects.Count;
			List<int[]> tTris = new List<int[]>();
			for(int i=0;i<vCount;i++){
				int[] tTriSingle = ProcessRoad_Tris_Shoulder(tVects[i].Count);
				tTris.Add(tTriSingle);
			}

			RCS.cut_ShoulderR_Vectors = tVects;
			RCS.cut_tris_ShoulderR = tTris;
		}
		
		private static void ProcessRoad_Tris_ShoulderCutsL(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			int cCount = RCS.ShoulderCutsL.Count;
			int PrevRoadCutIndex = 0;
			int CurrentRoadCutIndex = 0;
			List<List<Vector3>> tVects = new List<List<Vector3>>();
			List<Vector3> tVectListSingle = null;
			Vector3 xVect = default(Vector3);
			for(int j=0;j<cCount;j++){
				CurrentRoadCutIndex = RCS.ShoulderCutsR[j];
				tVectListSingle = new List<Vector3>();
				RCS.cut_ShoulderL_VectorsHome.Add(RCS.ShoulderL_Vectors[PrevRoadCutIndex]);
				xVect = RCS.ShoulderL_Vectors[PrevRoadCutIndex];
				for(int i=PrevRoadCutIndex;i<CurrentRoadCutIndex;i++){
					tVectListSingle.Add(RCS.ShoulderL_Vectors[i]-xVect);
				}
				tVects.Add(tVectListSingle);
				PrevRoadCutIndex = CurrentRoadCutIndex-8;
				if(PrevRoadCutIndex < 0){ PrevRoadCutIndex = 0; }
			}
			int mMax = RCS.ShoulderL_Vectors.Count;
			tVectListSingle = new List<Vector3>();
			RCS.cut_ShoulderL_VectorsHome.Add(RCS.ShoulderL_Vectors[PrevRoadCutIndex]);
			xVect = RCS.ShoulderL_Vectors[PrevRoadCutIndex];
			for(int i=PrevRoadCutIndex;i<mMax;i++){
				tVectListSingle.Add(RCS.ShoulderL_Vectors[i]-xVect);
			}
			tVects.Add(tVectListSingle);

			int vCount = tVects.Count;
			List<int[]> tTris = new List<int[]>();
			for(int i=0;i<vCount;i++){
				int[] tTriSingle = ProcessRoad_Tris_Shoulder(tVects[i].Count);
				tTris.Add(tTriSingle);
			}

			RCS.cut_ShoulderL_Vectors = tVects;
			RCS.cut_tris_ShoulderL = tTris;
		}
		
		private static int[] ProcessRoad_Tris_Shoulder(int MVL){
			int TriCount = 0;
			int x1,x2,x3;
			int xCount = (int)((MVL/2)*0.25f*6)-6;
			if(xCount < 0){ xCount = 0; }
			xCount = xCount * 2;
			
			int[] tri = new int[xCount];
			for(int i=0;i<MVL;i+=8){ 
				if(i+8 == MVL){ break; }
				
				x1 = i;
				x2 = i+8;
				x3 = i+2;
				
				tri[TriCount] = x1;	TriCount+=1;
				tri[TriCount] = x2;	TriCount+=1;
				tri[TriCount] = x3;	TriCount+=1;
					
				x1 = i+8;
				x2 = i+10;
				x3 = i+2;
	
				tri[TriCount] = x1;	TriCount+=1;
				tri[TriCount] = x2;	TriCount+=1;
				tri[TriCount] = x3;	TriCount+=1;

				x1 = i+4;
				x2 = i+12;
				x3 = i+6;
				
				tri[TriCount] = x1;	TriCount+=1;
				tri[TriCount] = x2;	TriCount+=1;
				tri[TriCount] = x3;	TriCount+=1;
					
				x1 = i+12;
				x2 = i+14;
				x3 = i+6;
	
				tri[TriCount] = x1;	TriCount+=1;
				tri[TriCount] = x2;	TriCount+=1;
				tri[TriCount] = x3;	TriCount+=1;
			}
			return tri;
		}
		
		//For intersection parts:
		private static void ProcessRoad_Tris_iProcessor(ref List<int[]> TriList, ref List<Vector3[]> VertexList){
			if(TriList == null){ TriList = new List<int[]>(); }
			int vListCount = VertexList.Count;
			int[] tris;
			for(int i=0;i<vListCount;i++){
				tris = ProcessRoad_Tris_Bulk_Helper(VertexList[i].Length);
				TriList.Add(tris);
			}
		}
		#endregion
		
		#region "Normals"
		private static void ProcessRoad_Normals_Bulk(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			//A mesh with just the vertices and triangles set up will be visible in the editor but will not look very convincing since it is not correctly shaded without the normals. 
			//The normals for the flat plane are very simple - they are all identical and point in the negative Z direction in the plane's local space. 
			//With the normals added, the plane will be correctly shaded but remember that you need a light in the scene to see the effect. 
			//Bridge normals are processed at same time as tris.
			int MVL = RCS.RoadVectors.Count;
			Vector3[] normals = new Vector3[MVL];
//			Vector3 tVect = -Vector3.forward;
//			for(int i=0;i<MVL;i++){
//				normals[i] = tVect;
//			}
			RCS.normals = normals;
			
			//Road cuts normals:
			if(RCS.tRoad.opt_bRoadCuts || RCS.tRoad.opt_bDynamicCuts){
				ProcessRoad_Normals_RoadCuts(ref RCS);	
			}
			if(RCS.tRoad.opt_bShoulderCuts || RCS.tRoad.opt_bDynamicCuts){
				ProcessRoad_Normals_ShoulderCutsR(ref RCS);	
				ProcessRoad_Normals_ShoulderCutsL(ref RCS);	
			}

			//Intersection normals:
			if(RCS.bInterseOn){
				//For intersection parts:
				//Back lanes:
				ProcessRoad_Normals_iProcessor(ref RCS.iBLane0s_normals,ref RCS.iBLane0s);
				ProcessRoad_Normals_iProcessor(ref RCS.iBLane1s_normals,ref RCS.iBLane1s);
				ProcessRoad_Normals_iProcessor(ref RCS.iBLane2s_normals,ref RCS.iBLane2s);
				ProcessRoad_Normals_iProcessor(ref RCS.iBLane3s_normals,ref RCS.iBLane3s);
				//Front lanes:
				ProcessRoad_Normals_iProcessor(ref RCS.iFLane0s_normals,ref RCS.iFLane0s);
				ProcessRoad_Normals_iProcessor(ref RCS.iFLane1s_normals,ref RCS.iFLane1s);
				ProcessRoad_Normals_iProcessor(ref RCS.iFLane2s_normals,ref RCS.iFLane2s);
				ProcessRoad_Normals_iProcessor(ref RCS.iFLane3s_normals,ref RCS.iFLane3s);
				//Main plates:
				ProcessRoad_Normals_iProcessor(ref RCS.iBMainPlates_normals,ref RCS.iBMainPlates);
				ProcessRoad_Normals_iProcessor(ref RCS.iFMainPlates_normals,ref RCS.iFMainPlates);
				//Marker plates:
				ProcessRoad_Normals_iProcessor(ref RCS.iBMarkerPlates_normals,ref RCS.iBMarkerPlates);
				ProcessRoad_Normals_iProcessor(ref RCS.iFMarkerPlates_normals,ref RCS.iFMarkerPlates);
			}
		}
		
		private static void ProcessRoad_Normals_RoadCuts(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			int cCount = RCS.cut_RoadVectors.Count;
			for(int j=0;j<cCount;j++){
				int MVL = RCS.cut_RoadVectors[j].Count;
				Vector3[] normals = new Vector3[MVL];
//				Vector3 tVect = -Vector3.forward;
//				for(int i=0;i<MVL;i++){
//					normals[i] = tVect;
//				}
				RCS.cut_normals.Add(normals);
			}
		}
		
		private static void ProcessRoad_Normals_ShoulderCutsR(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			int cCount = RCS.cut_ShoulderR_Vectors.Count;
			for(int j=0;j<cCount;j++){
				int MVL = RCS.cut_ShoulderR_Vectors[j].Count;
				Vector3[] normals = new Vector3[MVL];
//				Vector3 tVect = -Vector3.forward;
//				for(int i=0;i<MVL;i++){
//					normals[i] = tVect;
//				}
				RCS.cut_normals_ShoulderR.Add(normals);
			}
		}
		
		private static void ProcessRoad_Normals_ShoulderCutsL(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			int cCount = RCS.cut_ShoulderL_Vectors.Count;
			for(int j=0;j<cCount;j++){
				int MVL = RCS.cut_ShoulderL_Vectors[j].Count;
				Vector3[] normals = new Vector3[MVL];
//				Vector3 tVect = -Vector3.forward;
//				for(int i=0;i<MVL;i++){
//					normals[i] = tVect;
//				}
				RCS.cut_normals_ShoulderL.Add(normals);
			}
		}
		
		private static void ProcessRoad_Normals_Shoulders(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			//A mesh with just the vertices and triangles set up will be visible in the editor but will not look very convincing since it is not correctly shaded without the normals. 
			//The normals for the flat plane are very simple - they are all identical and point in the negative Z direction in the plane's local space. 
			//With the normals added, the plane will be correctly shaded but remember that you need a light in the scene to see the effect. 
			int MVL = RCS.ShoulderL_Vectors.Count;
			Vector3[] normals = new Vector3[MVL];
//			Vector3 tVect = -Vector3.forward;
//			for(int i=0;i<MVL;i++){
//				normals[i] = tVect;
//			}
			RCS.normals_ShoulderL = normals;
			//Right:
			MVL = RCS.ShoulderR_Vectors.Count;
			normals = new Vector3[MVL];
//			tVect = -Vector3.forward;
//			for(int i=0;i<MVL;i++){
//				normals[i] = tVect;
//			}
			RCS.normals_ShoulderR = normals;
		}
		
		//For intersection parts:
		private static void ProcessRoad_Normals_iProcessor(ref List<Vector3[]> NormalList, ref List<Vector3[]> VertexList){
			if(NormalList == null){ NormalList = new List<Vector3[]>(); }
			int vListCount = VertexList.Count;
			Vector3[] normals;
			int MVL = -1;
//			Vector3 tVect = -Vector3.forward;
			for(int i=0;i<vListCount;i++){
				MVL = VertexList[i].Length;
				normals = new Vector3[MVL];
//				for(int j=0;j<MVL;j++){
//					normals[j] = tVect;
//				}
				NormalList.Add(normals);
			}
		}
		#endregion
		
		#region "UVs"
		private static Vector2[] ProcessRoad_UVs(Vector3[] tVerts){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
			bool bOddToggle = true;
			float tDistance= 0f;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0f;
			float tDistanceRightSum = 0f;
			float tDistanceSum = 0f;

			while(i+6 < MVL){
				tDistance = Vector3.Distance(tVerts[i],tVerts[i+4]);
				tDistance = tDistance / 5f;
				uv[i] = new Vector2(0f, tDistanceSum);
				uv[i+2] = new Vector2(1f, tDistanceSum);
				uv[i+4] = new Vector2(0f, tDistance+tDistanceSum);
				uv[i+6] = new Vector2(1f, tDistance+tDistanceSum);	
				
				//Last segment needs adjusted due to double vertices:
				if((i+7) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}
				
				if(bOddToggle){
					i+=5;	
				}else{
					i+=3;
				}
				
				tDistanceLeftSum+=tDistanceLeft;
				tDistanceRightSum+=tDistanceRight;
				tDistanceSum+=tDistance;
				bOddToggle = !bOddToggle;
			}
			return uv;
		}
	
		private static void ProcessRoad_UVs_RoadCuts(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			
			int cCount = RCS.cut_RoadVectors.Count;
			float tDistance= 0f;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0f;
			float tDistanceRightSum = 0f;
			float tDistanceSum = 0f;
			for(int j=0;j<cCount;j++){
				Vector3[] tVerts = RCS.cut_RoadVectors[j].ToArray();
				int MVL = tVerts.Length;
				Vector2[] uv = new Vector2[MVL];
				Vector2[] uv_world = new Vector2[MVL];
				int i=0;
				bool bOddToggle = true;
				while(i+6 < MVL){
					tDistance = Vector3.Distance(tVerts[i],tVerts[i+4]);
					tDistance = tDistance / 5f;
					uv[i] = new Vector2(0f, tDistanceSum);
					uv[i+2] = new Vector2(1f, tDistanceSum);
					uv[i+4] = new Vector2(0f, tDistance+tDistanceSum);
					uv[i+6] = new Vector2(1f, tDistance+tDistanceSum);	
					
					//Last segment needs adjusted due to double vertices:
					if((i+7) == MVL){
						if(bOddToggle){
							//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
							uv[MVL-3] = uv[i+4];
							uv[MVL-1] = uv[i+6];
						}else{
							//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
							uv[MVL-4] = uv[i+4];
							uv[MVL-2] = uv[i+6];
						}
					}
					
					if(bOddToggle){
						i+=5;	
					}else{
						i+=3;
					}
					
					tDistanceLeftSum+=tDistanceLeft;
					tDistanceRightSum+=tDistanceRight;
					tDistanceSum+=tDistance;
					bOddToggle = !bOddToggle;
				}
				for(i=0;i<MVL;i++){
					uv_world[i] = new Vector2(tVerts[i].x*0.2f,tVerts[i].z*0.2f);
				}
				RCS.cut_uv_world.Add(uv_world);
				RCS.cut_uv.Add(uv);
			}
		}
		
		private static Vector2[] ProcessRoad_UVs_Shoulder(Vector3[] tVerts){
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
//			bool bOddToggle = true;
//			float tDistance= 0f;
//			float tDistanceLeft = 0f;
//			float tDistanceRight = 0f;
//			float tDistanceLeftSum = 0f;
//			float tDistanceRightSum = 0f;
//			float tDistanceSum = 0f;
//			float xDistance = 0f;
//			float rDistance1 = 0f;
//			float rDistance2 = 0f;
//			float fDistance = Vector3.Distance(tVerts[0],tVerts[2]);
			
			
			for(i=0;i<MVL;i++){
				uv[i] = new Vector2(tVerts[i].x*0.2f,tVerts[i].z*0.2f);
			}
			return uv;
			
//			while(i+8 < MVL){
//				tDistanceLeft = Vector3.Distance(tVerts[i],tVerts[i+8]);
//				tDistanceRight = Vector3.Distance(tVerts[i+2],tVerts[i+10]);
//				
//				tDistance = tDistance / 5f;
//				tDistanceLeft = tDistanceLeft / 5f;
//				tDistanceRight = tDistanceRight / 5f;
//				
//				uv[i] = new Vector2(0f, tDistanceSum);
//				uv[i+2] = new Vector2(1f, tDistanceSum);
//				uv[i+8] = new Vector2(0f, tDistance+tDistanceSum);
//				uv[i+10] = new Vector2(1f, tDistance+tDistanceSum);	
//				
//				rDistance1 = (Vector3.Distance(tVerts[i+4],tVerts[i+6]));
//				rDistance2 = (Vector3.Distance(tVerts[i+12],tVerts[i+14]));
//					
//				if(!bIsLeft){
//					uv[i+4] = new Vector2(1f, tDistanceSum);
//					xDistance = (rDistance1 / fDistance) + 1f;
//					uv[i+6] = new Vector2(xDistance, tDistanceSum);
//					uv[i+12] = new Vector2(1f, tDistance+tDistanceSum);
//					xDistance = (rDistance2 / fDistance) + 1f;
//					uv[i+14] = new Vector2(xDistance, tDistance+tDistanceSum);
//				}else{
//					xDistance = (rDistance1 / fDistance);
//					uv[i+4] = new Vector2(-xDistance, tDistanceSum);
//					uv[i+6] = new Vector2(0f, tDistanceSum);
//					xDistance = (rDistance2 / fDistance);
//					uv[i+12] = new Vector2(-xDistance, tDistance+tDistanceSum);
//					uv[i+14] = new Vector2(0f, tDistance+tDistanceSum);
//				}
//				
//				uv[i] = new Vector2(0f, tDistanceLeftSum);
//				uv[i+2] = new Vector2(1f, tDistanceRightSum);
//				uv[i+8] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
//				uv[i+10] = new Vector2(1f, tDistanceRight+tDistanceRightSum);	
//
//				uv[i] = new Vector2(tVerts[i].x/5f,tVerts[i].z/5f);
//				uv[i+2] = new Vector2(tVerts[i+2].x/5f,tVerts[i+2].z/5f);
//				uv[i+8] = new Vector2(tVerts[i+8].x/5f,tVerts[i+8].z/5f);
//				uv[i+10] = new Vector2(tVerts[i+10].x/5f,tVerts[i+10].z/5f);
//				
//				
//				rDistance1 = (Vector3.Distance(tVerts[i+4],tVerts[i+6]));
//				rDistance2 = (Vector3.Distance(tVerts[i+12],tVerts[i+14]));
//					
//				if(!bIsLeft){
//					uv[i+4] = new Vector2(1f, tDistanceRightSum);
//					xDistance = (rDistance1 / fDistance) + 1f;
//					uv[i+6] = new Vector2(xDistance, tDistanceRightSum);
//					uv[i+12] = new Vector2(1f, tDistanceRight+tDistanceRightSum);
//					xDistance = (rDistance2 / fDistance) + 1f;
//					uv[i+14] = new Vector2(xDistance, tDistanceRight+tDistanceRightSum);
//				}else{
//					xDistance = (rDistance1 / fDistance);
//					uv[i+4] = new Vector2(-xDistance, tDistanceLeftSum);
//					uv[i+6] = new Vector2(0f, tDistanceLeftSum);
//					xDistance = (rDistance2 / fDistance);
//					uv[i+12] = new Vector2(-xDistance, tDistanceLeft+tDistanceLeftSum);
//					uv[i+14] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
//				}
//				
//				uv[i+4] = new Vector2(tVerts[i+4].x/5f,tVerts[i+4].z/5f);
//				uv[i+6] = new Vector2(tVerts[i+6].x/5f,(tVerts[i+6].z/5f));
//				uv[i+12] = new Vector2(tVerts[i+12].x/5f,tVerts[i+12].z/5f);
//				uv[i+14] = new Vector2(tVerts[i+14].x/5f,(tVerts[i+14].z/5f));
//				
//				//Last segment needs adjusted due to double vertices:
//				if((i+11) == MVL){
//					if(bOddToggle){
//						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
//						uv[MVL-3] = uv[i+4];
//						uv[MVL-1] = uv[i+6];
//					}else{
//						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
//						uv[MVL-4] = uv[i+4];
//						uv[MVL-2] = uv[i+6];
//					}
//				}
//				
//				if(bOddToggle){
//					i+=9;	
//				}else{
//					i+=7;
//				}
//				
//				tDistanceLeftSum+=tDistanceLeft;
//				tDistanceRightSum+=tDistanceRight;
//				tDistanceSum+=tDistance;
//				bOddToggle = !bOddToggle;
//			}
//			return uv;
		}
		
		private static void ProcessRoad_UVs_ShoulderCut(ref GSD.Roads.RoadConstructorBufferMaker RCS, bool bIsLeft, int j){
			int i=0;
			Vector3[] tVerts;
			if(bIsLeft){
				tVerts = RCS.cut_ShoulderL_Vectors[j].ToArray();
			}else{
				tVerts = RCS.cut_ShoulderR_Vectors[j].ToArray();
			}
			int MVL = tVerts.Length;
			
			//World:
			Vector2[] uv_world = new Vector2[MVL];
			for(i=0;i<MVL;i++){
				uv_world[i] = new Vector2(tVerts[i].x*0.2f,tVerts[i].z*0.2f);
			}
			if(bIsLeft){
				RCS.cut_uv_SL_world.Add(uv_world);	
			}else{
				RCS.cut_uv_SR_world.Add(uv_world);
			}
			
			//Marks:
			float tDistance= 0f;
			float tDistanceSum = 0f;
			Vector2[] uv = new Vector2[MVL];
			float rDistance1 = 0f;
			float rDistance2 = 0f;
			bool bOddToggle = true;
			float fDistance = Vector3.Distance(tVerts[0],tVerts[2]);
			float xDistance = 0f;
			i=0;
			float TheOne = RCS.tRoad.opt_ShoulderWidth /  RCS.tRoad.opt_RoadDefinition;
			while(i+8 < MVL){
				tDistance = Vector3.Distance(tVerts[i],tVerts[i+8]) * 0.2f;
				
				uv[i] = new Vector2(0f, tDistanceSum);
				uv[i+2] = new Vector2(TheOne, tDistanceSum);
				uv[i+8] = new Vector2(0f, tDistance+tDistanceSum);
				uv[i+10] = new Vector2(TheOne, tDistance+tDistanceSum);	
				
				rDistance1 = (Vector3.Distance(tVerts[i+4],tVerts[i+6]));
				rDistance2 = (Vector3.Distance(tVerts[i+12],tVerts[i+14]));

				if(!bIsLeft){
					//Right
					//8	   10   12   14
					//0		2	 4	  6
					//0f   1f	1f	  X
					
					xDistance = TheOne + (rDistance1 / fDistance);
					uv[i+4] = uv[i+2];
					uv[i+6] = new Vector2(xDistance, tDistanceSum);
					
					xDistance = TheOne + (rDistance2 / fDistance);
					uv[i+12] = uv[i+10];
					uv[i+14] = new Vector2(xDistance, tDistance+tDistanceSum);
				}else{
					//Left:
					//12,13	   14,15    8,9    10,11
					//4,5		6,7		0,1		2,3	
					//0f-X	     0f	 	 0f		1f
					
					xDistance = 0f - (rDistance1 / fDistance);
					uv[i+4] = new Vector2(xDistance, tDistanceSum);
					uv[i+6] = uv[i];
					xDistance = 0f - (rDistance2 / fDistance);
					uv[i+12] = new Vector2(xDistance, tDistance+tDistanceSum);
					uv[i+14] = uv[i+8];
				}

				//Last segment needs adjusted due to double vertices:
				if((i+11) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}
				
				if(bOddToggle){
					i+=9;	
				}else{
					i+=7;
				}
				
				tDistanceSum+=tDistance;
				bOddToggle = !bOddToggle;
			}
			
			if(bIsLeft){
				RCS.cut_uv_SL.Add(uv);	
			}else{
				RCS.cut_uv_SR.Add(uv);
			}
		}
		
		#region "Intersection UV"
		private static void ProcessRoad_UVs_Intersections(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			int tCount = -1;

			//Lanes:
			tCount = RCS.iBLane0s.Count;
			for(int i=0;i<tCount;i++){
				RCS.iBLane0s_uv.Add(ProcessRoad_UVs_Intersection_Lane0(ref RCS,RCS.iBLane0s[i]));
			}
			tCount = RCS.iBLane1s.Count;
			for(int i=0;i<tCount;i++){
				if(RCS.iBLane1s_IsMiddleLane[i]){
					RCS.iBLane1s_uv.Add(ProcessRoad_UVs_Intersection_MiddleLane(ref RCS,RCS.iBLane1s[i]));	
				}else{
					RCS.iBLane1s_uv.Add(ProcessRoad_UVs_Intersection_FullLane(ref RCS,RCS.iBLane1s[i]));	
				}
			}
			tCount = RCS.iBLane2s.Count;
			for(int i=0;i<tCount;i++){
				RCS.iBLane2s_uv.Add(ProcessRoad_UVs_Intersection_FullLane(ref RCS,RCS.iBLane2s[i]));
			}
			tCount = RCS.iBLane3s.Count;
			for(int i=0;i<tCount;i++){
				RCS.iBLane3s_uv.Add(ProcessRoad_UVs_Intersection_Lane4(ref RCS,RCS.iBLane3s[i]));
			}
			
			//Lanes:
			tCount = RCS.iFLane0s.Count;
			for(int i=0;i<tCount;i++){
				RCS.iFLane0s_uv.Add(ProcessRoad_UVs_Intersection_Lane0(ref RCS,RCS.iFLane0s[i]));
			}
			tCount = RCS.iFLane1s.Count;
			for(int i=0;i<tCount;i++){
				if(RCS.iFLane1s_IsMiddleLane[i]){
					RCS.iFLane1s_uv.Add(ProcessRoad_UVs_Intersection_MiddleLane(ref RCS,RCS.iFLane1s[i]));	
				}else{
					RCS.iFLane1s_uv.Add(ProcessRoad_UVs_Intersection_FullLane(ref RCS,RCS.iFLane1s[i]));	
				}
			}
			tCount = RCS.iFLane2s.Count;
			for(int i=0;i<tCount;i++){
				RCS.iFLane2s_uv.Add(ProcessRoad_UVs_Intersection_FullLane(ref RCS,RCS.iFLane2s[i]));
			}
			tCount = RCS.iFLane3s.Count;
			for(int i=0;i<tCount;i++){
				RCS.iFLane3s_uv.Add(ProcessRoad_UVs_Intersection_Lane4(ref RCS,RCS.iFLane3s[i]));
			}

			//Main plates:
			tCount = RCS.iBMainPlates.Count;
			for(int i=0;i<tCount;i++){
				RCS.iBMainPlates_uv.Add(ProcessRoad_UVs_Intersection_MainPlate(ref RCS,RCS.iBMainPlates[i]));
			}
			tCount = RCS.iFMainPlates.Count;
			for(int i=0;i<tCount;i++){
				RCS.iFMainPlates_uv.Add(ProcessRoad_UVs_Intersection_MainPlate(ref RCS,RCS.iFMainPlates[i]));
			}
			tCount = RCS.iBMainPlates.Count;
			for(int i=0;i<tCount;i++){
				RCS.iBMainPlates_uv2.Add(ProcessRoad_UVs_Intersection_MainPlate2(ref RCS,RCS.iBMainPlates[i],RCS.iBMainPlates_tID[i]));
			}
			tCount = RCS.iFMainPlates.Count;
			for(int i=0;i<tCount;i++){
				RCS.iFMainPlates_uv2.Add(ProcessRoad_UVs_Intersection_MainPlate2(ref RCS,RCS.iFMainPlates[i],RCS.iFMainPlates_tID[i]));
			}
			
			//Marker plates:
			tCount = RCS.iBMarkerPlates.Count;
			for(int i=0;i<tCount;i++){
				RCS.iBMarkerPlates_uv.Add(ProcessRoad_UVs_Intersection_MarkerPlate(ref RCS,RCS.iBMarkerPlates[i]));
			}
			tCount = RCS.iFMarkerPlates.Count;
			for(int i=0;i<tCount;i++){
				RCS.iFMarkerPlates_uv.Add(ProcessRoad_UVs_Intersection_MarkerPlate(ref RCS,RCS.iFMarkerPlates[i]));
			}
		}
		
		private static Vector2[] ProcessRoad_UVs_Intersection_FullLane(ref GSD.Roads.RoadConstructorBufferMaker RCS, Vector3[] tVerts){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
			bool bOddToggle = true;
			float tDistance= 0f;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0f;
			float tDistanceRightSum = 0f;
			float tDistanceSum = 0f;

			while(i+6 < MVL){
				tDistance = Vector3.Distance(tVerts[i],tVerts[i+4]);
				tDistance = tDistance / 5f;
				uv[i] = new Vector2(0f, tDistanceSum);
				uv[i+2] = new Vector2(1f, tDistanceSum);
				uv[i+4] = new Vector2(0f, tDistance+tDistanceSum);
				uv[i+6] = new Vector2(1f, tDistance+tDistanceSum);	
				
				//Last segment needs adjusted due to double vertices:
				if((i+7) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}
				
				if(bOddToggle){
					i+=5;	
				}else{
					i+=3;
				}
				
				tDistanceLeftSum+=tDistanceLeft;
				tDistanceRightSum+=tDistanceRight;
				tDistanceSum+=tDistance;
				bOddToggle = !bOddToggle;
			}
			
			return uv;
		}
		
		private static Vector2[] ProcessRoad_UVs_Intersection_Lane4(ref GSD.Roads.RoadConstructorBufferMaker RCS, Vector3[] tVerts){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
			bool bOddToggle = true;
			float tDistance= 0f;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0f;
			float tDistanceRightSum = 0f;
			float tDistanceSum = 0f;

			while(i+6 < MVL){
				tDistance = Vector3.Distance(tVerts[i],tVerts[i+4]);
				tDistance = tDistance / 5f;
				
				
				if(i==0){
					uv[i] = new Vector2(0.94f, tDistanceSum);
					uv[i+2] = new Vector2(1f, tDistanceSum);
					uv[i+4] = new Vector2(0f, tDistance+tDistanceSum);
					uv[i+6] = new Vector2(1f, tDistance+tDistanceSum);	
				}else{
					uv[i] = new Vector2(0f, tDistanceSum);
					uv[i+2] = new Vector2(1f, tDistanceSum);
					uv[i+4] = new Vector2(0f, tDistance+tDistanceSum);
					uv[i+6] = new Vector2(1f, tDistance+tDistanceSum);	
				}
				
				//Last segment needs adjusted due to double vertices:
				if((i+7) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}	

				if(bOddToggle){
					i+=5;	
				}else{
					i+=3;
				}
				
				tDistanceLeftSum+=tDistanceLeft;
				tDistanceRightSum+=tDistanceRight;
				tDistanceSum+=tDistance;
				bOddToggle = !bOddToggle;
			}
			
			return uv;
		}
	
		private static Vector2[] ProcessRoad_UVs_Intersection_MiddleLane(ref GSD.Roads.RoadConstructorBufferMaker RCS, Vector3[] tVerts){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
			bool bOddToggle = true;
			float tDistance= 0f;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0f;
			float tDistanceRightSum = 0f;
			float tDistanceSum = 0f;

			while(i+6 < MVL){
				tDistance = Vector3.Distance(tVerts[i],tVerts[i+4]);
				tDistance = tDistance / 5f;
				
				
				if(i==0){
					uv[i] = new Vector2(0f, tDistanceSum);
					uv[i+2] = new Vector2(0.05f, tDistanceSum);
					uv[i+4] = new Vector2(0f, tDistance+tDistanceSum);
					uv[i+6] = new Vector2(1f, tDistance+tDistanceSum);	
				}else{
					uv[i] = new Vector2(0f, tDistanceSum);
					uv[i+2] = new Vector2(1f, tDistanceSum);
					uv[i+4] = new Vector2(0f, tDistance+tDistanceSum);
					uv[i+6] = new Vector2(1f, tDistance+tDistanceSum);	
				}
				
				//Last segment needs adjusted due to double vertices:
				if((i+7) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}
				
				if(bOddToggle){
					i+=5;	
				}else{
					i+=3;
				}
				
				tDistanceLeftSum+=tDistanceLeft;
				tDistanceRightSum+=tDistanceRight;
				tDistanceSum+=tDistance;
				bOddToggle = !bOddToggle;
			}
			return uv;
		}
		
		private static Vector2[] ProcessRoad_UVs_Intersection_Lane0(ref GSD.Roads.RoadConstructorBufferMaker RCS, Vector3[] tVerts){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
			bool bOddToggle = true;
			float tDistance= 0f;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0f;
			float tDistanceRightSum = 0f;
			float tDistanceSum = 0f;

			while(i+6 < MVL){
				tDistanceLeft = Vector3.Distance(tVerts[i],tVerts[i+4]);
				tDistanceRight = Vector3.Distance(tVerts[i+2],tVerts[i+6]);
				
				tDistanceLeft = tDistanceLeft / 5f;
				tDistanceRight = tDistanceRight / 5f;
				
				//Below is for uniform
//				if(i==0){
//					uv[i] = new Vector2(0.5f, tDistanceLeftSum);
//					uv[i+2] = new Vector2(1.5f, tDistanceRightSum);
//					uv[i+4] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
//					uv[i+6] = new Vector2(1.5f, tDistanceRight+tDistanceRightSum);
//				}else{
//					uv[i] = new Vector2(0f, tDistanceLeftSum);
//					uv[i+2] = new Vector2(1f, tDistanceRightSum);
//					uv[i+4] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
//					uv[i+6] = new Vector2(1f, tDistanceRight+tDistanceRightSum);
//				}
				
				//Stretched:
				uv[i] = new Vector2(0f, tDistanceLeftSum);
				uv[i+2] = new Vector2(1f, tDistanceRightSum);
				uv[i+4] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
				uv[i+6] = new Vector2(1f, tDistanceRight+tDistanceRightSum);
				
				//Last segment needs adjusted due to double vertices:
				if((i+7) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}
				
				if(bOddToggle){
					i+=5;	
				}else{
					i+=3;
				}
				
				tDistanceLeftSum+=tDistanceLeft;
				tDistanceRightSum+=tDistanceRight;
				tDistanceSum+=tDistance;
				bOddToggle = !bOddToggle;
			}
			return uv;
		}
		
		private static Vector2[] ProcessRoad_UVs_Intersection_MarkerPlate(ref GSD.Roads.RoadConstructorBufferMaker RCS, Vector3[] tVerts){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
			bool bOddToggle = true;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0.1f;
			float tDistanceRightSum = 0.1f;
			
			float mDistanceL = Vector3.Distance(tVerts[i],tVerts[tVerts.Length-3]);
			float mDistanceR = Vector3.Distance(tVerts[i+2],tVerts[tVerts.Length-1]);
			
			while(i+6 < MVL){
				tDistanceLeft = Vector3.Distance(tVerts[i],tVerts[i+4]);
				tDistanceRight = Vector3.Distance(tVerts[i+2],tVerts[i+6]);
				
				tDistanceLeft = tDistanceLeft / mDistanceL;
				tDistanceRight = tDistanceRight / mDistanceR;
			
				uv[i] = new Vector2(0f, tDistanceLeftSum);
				uv[i+2] = new Vector2(1f, tDistanceRightSum);
				uv[i+4] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
				uv[i+6] = new Vector2(1f, tDistanceRight+tDistanceRightSum);
				
				//Last segment needs adjusted due to double vertices:
				if((i+7) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}
				
				if(bOddToggle){
					i+=5;	
				}else{
					i+=3;
				}
				
				tDistanceLeftSum+=tDistanceLeft;
				tDistanceRightSum+=tDistanceRight;
				bOddToggle = !bOddToggle;
			}
			return uv;
		}
		
		private static Vector2[] ProcessRoad_UVs_Intersection_MainPlate(ref GSD.Roads.RoadConstructorBufferMaker RCS, Vector3[] tVerts){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
//			bool bOddToggle = true;
//			float tDistance= 0f;
//			float tDistanceLeft = 0f;
//			float tDistanceRight = 0f;
//			float tDistanceLeftSum = 0f;
//			float tDistanceRightSum = 0f;
//			float tDistanceSum = 0f;
//			float DistRepresent = 5f;

//			float mDistanceL = Vector3.Distance(tVerts[i],tVerts[tVerts.Length-3]);
//			float mDistanceR = Vector3.Distance(tVerts[i+2],tVerts[tVerts.Length-1]);
			
			for(i=0;i<MVL;i++){
				uv[i] = new Vector2(tVerts[i].x*0.2f, tVerts[i].z*0.2f);
			}
			return uv;
			
//			while(i+6 < MVL){
//				tDistanceLeft = Vector3.Distance(tVerts[i],tVerts[i+4]);
//				tDistanceRight = Vector3.Distance(tVerts[i+2],tVerts[i+6]);
//				
//				tDistanceLeft = tDistanceLeft / 5f;
//				tDistanceRight = tDistanceRight / 5f;
//				
////				if(i==0){
////					uv[i] = new Vector2(0.25f, tDistanceLeftSum);
////					uv[i+2] = new Vector2(1.25f, tDistanceRightSum);
////					uv[i+4] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
////					uv[i+6] = new Vector2(2f, tDistanceRight+tDistanceRightSum);
////				}else{
////					uv[i] = new Vector2(0f, tDistanceLeftSum);
////					uv[i+2] = new Vector2(2f, tDistanceRightSum);
////					uv[i+4] = new Vector2(0f, tDistanceLeft+tDistanceLeftSum);
////					uv[i+6] = new Vector2(2f, tDistanceRight+tDistanceRightSum);
////				}
//				
//				uv[i] = new Vector2(tVerts[i].x/5f, tVerts[i].z/5f);
//				uv[i+2] = new Vector2(tVerts[i+2].x/5f, tVerts[i+2].z/5f);
//				uv[i+4] = new Vector2(tVerts[i+4].x/5f, tVerts[i+4].z/5f);
//				uv[i+6] = new Vector2(tVerts[i+6].x/5f, tVerts[i+6].z/5f);
//
//				//Last segment needs adjusted due to double vertices:
//				if((i+7) == MVL){
//					if(bOddToggle){
//						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
//						uv[MVL-3] = uv[i+4];
//						uv[MVL-1] = uv[i+6];
//					}else{
//						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
//						uv[MVL-4] = uv[i+4];
//						uv[MVL-2] = uv[i+6];
//					}
//				}
//				
//				if(bOddToggle){
//					i+=5;	
//				}else{
//					i+=3;
//				}
//				
//				tDistanceLeftSum+=tDistanceLeft;
//				tDistanceRightSum+=tDistanceRight;
//				//tDistanceSum+=tDistance;
//				bOddToggle = !bOddToggle;
//			}
//			return uv;
		}
		
		private static Vector2[] ProcessRoad_UVs_Intersection_MainPlate2(ref GSD.Roads.RoadConstructorBufferMaker RCS, Vector3[] tVerts, GSDRoadIntersection GSDRI){
			//Finally, adding texture coordinates to the mesh will enable it to display a material correctly. 
			//Assuming we want to show the whole image across the plane, the UV values will all be 0 or 1, corresponding to the corners of the texture. 
			//int MVL = tMesh.vertices.Length;
			int MVL = tVerts.Length;
			Vector2[] uv = new Vector2[MVL];
			int i=0;
			bool bOddToggle = true;
//			float tDistance= 0f;
			float tDistanceLeft = 0f;
			float tDistanceRight = 0f;
			float tDistanceLeftSum = 0f;
			float tDistanceRightSum = 0f;
//			float tDistanceSum = 0f;
//			float DistRepresent = 5f;

			float mDistanceL = Vector3.Distance(tVerts[i+4],tVerts[tVerts.Length-3]);
			float mDistanceR = Vector3.Distance(tVerts[i+6],tVerts[tVerts.Length-1]);
			mDistanceL = mDistanceL * 1.125f;
			mDistanceR = mDistanceR * 1.125f;
			
//			int bHitMaxL = 0;
//			int bHitMaxR = 0;
			
			float tAdd1;
			float tAdd2;
			float tAdd3;
			float tAdd4;
			
			float RoadWidth = RCS.tRoad.RoadWidth();
			float LaneWidth = RCS.tRoad.opt_LaneWidth;
			float iWidth = -1;
			if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
				iWidth = RoadWidth + (LaneWidth*2f);
			}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
				iWidth = RoadWidth + (LaneWidth*1f);
			}else{
				iWidth = RoadWidth;
			}
			 
			
			while(i+6 < MVL){
				if(i==0){
					
					if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
						
						//(Lane width / 2)/roadwidth
						//1-((lanewidth / 2)/roadwidth)
						
						
						
						uv[i] = new Vector2((LaneWidth * 0.5f)/iWidth, 0f);
						uv[i+2] = new Vector2(1f-(((LaneWidth * 0.5f) + LaneWidth)/iWidth), 0f);
						//Debug.Log (GSDRI.tName + " " + uv[i+2].x);
						uv[i+4] = new Vector2(0f, 0.125f);
						uv[i+6] = new Vector2(1f, 0.125f);
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
						uv[i] = new Vector2((LaneWidth * 0.5f)/iWidth, 0f);
						uv[i+2] = new Vector2(1f-((LaneWidth * 0.5f)/iWidth), 0f);
						uv[i+4] = new Vector2(0f, 0.125f);
						uv[i+6] = new Vector2(1f, 0.125f);
					}else if(GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
						uv[i] = new Vector2(0f, 0f);
						uv[i+2] = new Vector2(1f, 0f);
						uv[i+4] = new Vector2(0f, 0.125f);
						uv[i+6] = new Vector2(1f, 0.125f);	
					}
					tDistanceLeft = 0.125f;
					tDistanceRight = 0.125f;
				}else{
					tDistanceLeft = Vector3.Distance(tVerts[i],tVerts[i+4]);
					tDistanceRight = Vector3.Distance(tVerts[i+2],tVerts[i+6]);
					tDistanceLeft = tDistanceLeft / mDistanceL;
					tDistanceRight = tDistanceRight / mDistanceR;
					
					
//					if(bHitMaxL > 0 || (tDistanceLeftSum+tDistanceLeft) > 1f){ 
//						tDistanceLeftSum = 0.998f + (0.0001f*bHitMaxL); 
//						tDistanceLeft = 0.001f; 
//						bHitMaxL+=1;
//					}
//					if(bHitMaxR > 0 || (tDistanceRightSum+tDistanceRight) > 1f){ 
//						tDistanceRightSum = 0.998f + (0.0001f*bHitMaxR); 
//						tDistanceRight = 0.001f;
//						bHitMaxR+=1;
//					}
				
					tAdd1 = tDistanceLeftSum; 					if(tAdd1 > 1f){ tAdd1 = 1f; }
					tAdd2 = tDistanceRightSum; 					if(tAdd2 > 1f){ tAdd2 = 1f; }
					tAdd3 = tDistanceLeft+tDistanceLeftSum; 	if(tAdd3 > 1f){ tAdd3 = 1f; }
					tAdd4 = tDistanceRight+tDistanceRightSum; 	if(tAdd4 > 1f){ tAdd4 = 1f; }
					
					uv[i] = new Vector2(0f, tAdd1);
					uv[i+2] = new Vector2(1f, tAdd2);
					uv[i+4] = new Vector2(0f, tAdd3);
					uv[i+6] = new Vector2(1f, tAdd4);
					//Debug.Log (tAdd3 + " R:"+ tAdd4 + " RLoc: " + tVerts[i+6]);
				}
				
				
				
				//Debug.Log ("1.0 R:1.0 RLoc: " + tVerts[i+6]);
				
				//Last segment needs adjusted due to double vertices:
				if((i+7) == MVL){
					if(bOddToggle){
						//First set: Debug.Log ("+5:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));		
						uv[MVL-3] = uv[i+4];
						uv[MVL-1] = uv[i+6];
					}else{
						//Last set: Debug.Log ("+3:"+i+" "+(i+2)+" "+(i+4)+" "+(i+6));	
						uv[MVL-4] = uv[i+4];
						uv[MVL-2] = uv[i+6];
					}
				}
				
				
				
				if(bOddToggle){
					i+=5;	
					
					if(i+6 >= MVL){
						uv[i+4-5] = new Vector2(0f, 1f);
						uv[i+6-5] = new Vector2(1f, 1f);
					}
					
				}else{
					i+=3;
					
					if(i+6 >= MVL){
						uv[i+4-3] = new Vector2(0f, 1f);
						uv[i+6-3] = new Vector2(1f, 1f);
					}
				}
				
				
				
				tDistanceLeftSum+=tDistanceLeft;
				tDistanceRightSum+=tDistanceRight;
				//tDistanceSum+=tDistance;
				bOddToggle = !bOddToggle;
			}
			
//			uv[MVL-1].y = 1f;
//			uv[MVL-2].y = 1f;
//			uv[MVL-3].y = 1f;
//			uv[MVL-4].y = 1f;
			
			return uv;
		}
		#endregion
#endregion

		#region "Set vector heights"
		private static void SetVectorHeight2(ref Vector3 tWorldVect, ref float p, ref List<KeyValuePair<float,float>> tList, ref GSDSplineC tSpline){
			int mCount = tList.Count;
			int i=0;
			
			if(mCount < 1){ tWorldVect.y = 0f; return; }

			float cValue = 0f;
			for(i=0;i<(mCount-1);i++){
				if(p >= tList[i].Key && p < tList[i+1].Key){
					cValue = tList[i].Value;
					if(i > 3){
						if(tList[i-1].Value < cValue){
							cValue = tList[i-1].Value;
						}
						if(tList[i-2].Value < cValue){
							cValue = tList[i-2].Value;
						}
						if(tList[i-3].Value < cValue){
							cValue = tList[i-3].Value;
						}
					}
					if(i < (mCount-3)){
						if(tList[i+1].Value < cValue){
							cValue = tList[i+1].Value;
						}
						if(tList[i+2].Value < cValue){
							cValue = tList[i+2].Value;
						}
						if(tList[i+3].Value < cValue){
							cValue = tList[i+3].Value;
						}
					}
					break;
				}
			}

            //if(p > 0.95f && GSDRootUtil.IsApproximately(cValue,0f,0.001f)){
            //    float DeadValue = 0f;
            //    Vector3 tPos = tSpline.GetSplineValue(p,false);
            //    if(!tSpline.IsNearIntersection(ref tPos,ref DeadValue)){
            //        cValue = tList[tList.Count-1].Value;
            //    }
            //}

            //Zero protection: 
            if (GSDRootUtil.IsApproximately(cValue, 0f, 0.001f) && tWorldVect.y > 0f) {
                cValue = tWorldVect.y-0.35f;
            }

            tWorldVect.y = cValue;
		}
		#endregion
		
		private static bool IsApproximately(float a, float b){
	    	return IsApproximately(a, b, 0.01f);
	    }
	     
	   	private static bool IsApproximately(float a, float b, float tolerance){
	   		return Mathf.Abs(a - b) < tolerance;
	    }
		
		public class RoadTerrainInfo{
			public Rect tBounds;
			public int GSDID;
			public int hmWidth;
			public int hmHeight;
			public Vector3 tPos;
			public Vector3 tSize;
			public float[,] heights;
		}
	}
	
	#region "Threading core"
	public class GSDThreadedJob{
	    private bool m_IsDone = false;
	    private object m_Handle = new object();
	    private System.Threading.Thread m_Thread = null;
			
		public bool IsDone{
			get{
	    		bool tmp;
	    		lock (m_Handle){
	    			tmp = m_IsDone;
	    		}
	   			return tmp;
	    	}set{
	    		lock (m_Handle){
	    			m_IsDone = value;
	    		}
	   	 	}
	    }
	     
	    public virtual void Start(){
	    	m_Thread = new System.Threading.Thread(Run);
	    	m_Thread.Start();
	    }
			
	    public virtual void Abort(){
	    	m_Thread.Abort();
	    }
	     
	    protected virtual void ThreadFunction() { }
	     
	    protected virtual void OnFinished() { }
	     
	    public virtual bool Update(){
		    if (IsDone){
		    	OnFinished();
		    	return true;
		    }
	    	return false;
	    }
	    private void Run(){
	    	ThreadFunction();
	    	IsDone = true;
	    }
	}
		
	public class GSDJob : GSDThreadedJob{
		//public Vector3[] InData; // arbitary job data
		//public Vector3[] OutData; // arbitary job data
		
		protected override void ThreadFunction(){
			// Do your threaded task. DON'T use the Unity API here
			//for (int i = 0; i < 100000000; i++){
				//InData[i % InData.Length] += InData[(i+1) % InData.Length];
			//}
		}
		
		protected override void OnFinished(){
			// This is executed by the Unity main thread when the job is finished
			//for (int i = 0; i < InData.Length; i++){
				//Debug.Log("Results(" + i + "): " + InData[i]);
			//}
		}
	}
	#endregion
	
	public class TerrainCalcs : GSDThreadedJob{
		private object GSDm_Handle = new object();
		private List<GSD.Roads.GSDTerraforming.TempTerrainData> TTDList;
		private GSDSplineC tSpline;
		private GSDRoad tRoad;
		
		public void Setup(ref List<GSD.Roads.GSDTerraforming.TempTerrainData> _TTDList, GSDSplineC _tSpline, GSDRoad _tRoad){
			TTDList = _TTDList;
			tSpline = _tSpline;
			tRoad = _tRoad;
		}
		
		protected override void ThreadFunction(){
			float Step = (tRoad.opt_RoadDefinition*0.4f) / tSpline.distance;
			if(Step > 2f){ Step = 2f; }
			if(Step < 1f){ Step = 1f; }
//			float tDistance = tRoad.RoadWidth()*2f;

//			Vector3 tVect,POS;
			foreach(GSD.Roads.GSDTerraforming.TempTerrainData TTD in TTDList){
//				float PrevHeight = 0f;
//				float FinalMax = 1f;
//				float StartMin = 0f;
//				if(tSpline.bSpecialEndControlNode){
//					FinalMax = tSpline.mNodes[tSpline.GetNodeCount()-2].tTime;
//				}
//				if(tSpline.bSpecialStartControlNode){
//					StartMin = tSpline.mNodes[1].tTime;
//				}
				
//				if(tRoad.opt_MatchTerrain){
				try{
					GSDTerraformingT.DoRects(tSpline,TTD);
				}catch(System.Exception e){
					lock (GSDm_Handle){
						tRoad.bEditorError = true;
						tRoad.tError = e;
					}
					throw e;
				}
//				}else{
//					for(float i=StartMin;i<=FinalMax;i+=Step){
//						if(tSpline.IsInBridgeTerrain(i)){
//							float tFloat = tSpline.GetBridgeEnd(i);
//							if(IsApproximately(tFloat,1f,0.00001f) || tFloat > 1f){ continue; }
//							if(tFloat < 0f){ continue; }
//							i = tFloat;
//						}
//						tSpline.GetSplineValue_Both(i,out tVect,out POS);
//						PrevHeight = GSDTerraformingT.ProcessLineHeights(tSpline,ref tVect,ref POS,tDistance,TTD,PrevHeight);
//						tSpline.HeightHistory.Add(new KeyValuePair<float,float>(i,PrevHeight*TTD.TerrainSize.y));
//					}	
//					
//					for(int i=0;i<TTD.cI;i++){
//						TTD.heights[TTD.cX[i],TTD.cY[i]] = TTD.cH[i];
//					}
//				}
			}	
			tSpline.HeightHistory.Sort(Compare1);
			IsDone = true;
		}
		
		bool IsApproximately(float a, float b){
	    	return IsApproximately(a, b, 0.01f);
	    }
	     
	    bool IsApproximately(float a, float b, float tolerance){
	   		return Mathf.Abs(a - b) < tolerance;
	    }
		
		int Compare1(KeyValuePair<float, float> a, KeyValuePair<float,float> b){
			return a.Key.CompareTo(b.Key);
	    }
	}
	
	public static class TerrainCalcs_Static{
		public static void RunMe(ref List<GSD.Roads.GSDTerraforming.TempTerrainData> TTDList, GSDSplineC tSpline, GSDRoad tRoad){
			float Step = (tRoad.opt_RoadDefinition*0.4f) / tSpline.distance;
			if(Step > 2f){ Step = 2f; }
			if(Step < 1f){ Step = 1f; }
//			float tDistance = tRoad.RoadWidth()*2f;

//			Vector3 tVect,POS;
			
			foreach(GSD.Roads.GSDTerraforming.TempTerrainData TTD in TTDList){
//				float PrevHeight = 0f;
//				float FinalMax = 1f;
//				float StartMin = 0f;
//				if(tSpline.bSpecialEndControlNode){
//					FinalMax = tSpline.mNodes[tSpline.GetNodeCount()-2].tTime;
//				}
//				if(tSpline.bSpecialStartControlNode){
//					StartMin = tSpline.mNodes[1].tTime;
//				}
				
//				if(tRoad.opt_MatchTerrain){
				if(tRoad.bProfiling){
                    UnityEngine.Profiling.Profiler.BeginSample("DoRects");	
				}
					GSDTerraformingT.DoRects(tSpline,TTD);
				
				if(tRoad.bProfiling){
                    UnityEngine.Profiling.Profiler.EndSample();	
				}
//				}else{
//					for(float i=StartMin;i<=FinalMax;i+=Step){
//						if(tSpline.IsInBridgeTerrain(i)){
//							float tFloat = tSpline.GetBridgeEnd(i);
//							if(IsApproximately(tFloat,1f,0.00001f) || tFloat > 1f){ continue; }
//							if(tFloat < 0f){ continue; }
//							i = tFloat;
//						}
//						tSpline.GetSplineValue_Both(i,out tVect,out POS);
//						PrevHeight = GSDTerraformingT.ProcessLineHeights(tSpline,ref tVect,ref POS,tDistance,TTD,PrevHeight);
//						tSpline.HeightHistory.Add(new KeyValuePair<float,float>(i,PrevHeight*TTD.TerrainSize.y));
//					}	
//					
//					for(int i=0;i<TTD.cI;i++){
//						TTD.heights[TTD.cX[i],TTD.cY[i]] = TTD.cH[i];
//					}
//				}
			}	
			tSpline.HeightHistory.Sort(Compare1);
		}
		
		static bool IsApproximately(float a, float b){
	    	return IsApproximately(a, b, 0.01f);
	    }
	     
	    static bool IsApproximately(float a, float b, float tolerance){
	   		return Mathf.Abs(a - b) < tolerance;
	    }
		
		static int Compare1(KeyValuePair<float, float> a, KeyValuePair<float,float> b){
			return a.Key.CompareTo(b.Key);
	    }
	}

	public class RoadCalcs1 : GSDThreadedJob{
		private object GSDm_Handle = new object();
		private GSD.Roads.RoadConstructorBufferMaker RCS;
		private GSDRoad tRoad;
		
		public void Setup(ref GSD.Roads.RoadConstructorBufferMaker _RCS, ref GSDRoad _tRoad){
			RCS = _RCS;
			tRoad = _tRoad;
		}
		
		protected override void ThreadFunction(){
			try{
				GSDRoadCreationT.RoadJob_Prelim(ref tRoad);
				GSDRoadCreationT.RoadJob1(ref RCS);
			}catch(System.Exception e){
				lock (GSDm_Handle){
					tRoad.bEditorError = true;
					tRoad.tError = e;
				}
				throw e;
			}
		}
		
		public GSD.Roads.RoadConstructorBufferMaker GetRCS(){
			GSD.Roads.RoadConstructorBufferMaker tRCS;
			lock (GSDm_Handle){
	    			tRCS = RCS;
	    	}
			return tRCS;	
		}
	}
	
	public static class RoadCalcs1_static{
		public static void RunMe(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			GSDRoadCreationT.RoadJob1(ref RCS);
		}
	}
	
	public class RoadCalcs2 : GSDThreadedJob{
		private object GSDm_Handle = new object();
		private GSD.Roads.RoadConstructorBufferMaker RCS;
		public void Setup(ref GSD.Roads.RoadConstructorBufferMaker _RCS){
			RCS = _RCS;
		}
		
		protected override void ThreadFunction(){
			try{
				GSDRoadCreationT.RoadJob2(ref RCS); 
			}catch(System.Exception e){
				lock (GSDm_Handle){
					RCS.tRoad.bEditorError = true;
					RCS.tRoad.tError = e;
				}
			}
		}
		
		public GSD.Roads.RoadConstructorBufferMaker GetRCS(){
			GSD.Roads.RoadConstructorBufferMaker tRCS;
			lock (GSDm_Handle){
	    		tRCS = RCS;
	    	}
			return tRCS;	
		}
	}
	
	public static class RoadCalcs2_static{
		public static void RunMe(ref GSD.Roads.RoadConstructorBufferMaker RCS){
			GSDRoadCreationT.RoadJob2(ref RCS);
		}
	}

	#endif
}