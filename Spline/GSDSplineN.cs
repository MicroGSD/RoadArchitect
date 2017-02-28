using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using GSD.Roads.Splination;
using GSD.Roads.EdgeObjects;
using GSD;
#endif
public class GSDSplineN : MonoBehaviour{
	#if UNITY_EDITOR
	
	public Vector3 pos;
	public Quaternion rot;
	public Vector3 tangent;
	public Vector2 EaseIO;
	public float tDist = 0f;
	public float tTime = 0f;
	public float NextTime = 1f;
	public Vector3 NextTan = default(Vector3);
	public float OldTime = 0f;
	public float SegmentDist = 0f;
	public string EditorDisplayString = "";
	
	public string tName = "Node-1";
	
	public bool tempTime = false; 
	public float tempSegmentTime = 0f;
	public float tempMinDistance = 5000f;
	public float tempMinTime = 0f;
	public bool bSpecialEndNode = false;
	public GSDSplineN SpecialNodeCounterpart = null;
	public GSDSplineN SpecialNodeCounterpart_Master = null;
	public GSDSplineN SpecialNodeCounterpart_Old = null;
	public bool bSpecialEndNode_IsStart = false;
	public bool bSpecialEndNode_IsEnd = false;
	public bool bSpecialIntersection = false;
	public bool bSpecialRoadConnPrimary = false;
	public bool bSpecialRoadConnPrimary_Old = false;
	public bool bRoadCut = false;
	public float MinSplination = 0f;
	public float MaxSplination = 1f;
	public bool bQuitGUI = false;
	
	public int idOnSpline = -1;
	public GSDSplineC GSDSpline;
	public bool bDestroyed = false;
	public string UID; //Unique ID
	public GSDSplineN Intersection_OtherNode;
	
	//Editor only:
	public bool bEditorSelected = false;
	public string GradeToNext;
	public string GradeToPrev;
	public float GradeToNextValue;
	public float GradeToPrevValue;
	public float bInitialRoadHeight = -1f;
	
	//Navigation:
	public bool bNeverIntersect = false;
	public bool bIsIntersection = false;
	public bool bIsEndPoint = false;
	public bool bIsMainPoint = false;
	public int id = 0;
	public int id_intersection_othernode = 0;
	public List<int> id_connected;
	public List<GSDSplineN> node_connected;
	public bool bDeleteMe = false;
	public float tempDistance = 0f;
	public int ExtraFCost = 0;
	public int gValue,hValue,fValue;
	public bool bIgnore = false;
	public bool opt_GizmosEnabled = true;
	
	//Tunnels:
	public bool bIsTunnel = false;
	public bool bIsTunnelStart = false;
	public bool bIsTunnelEnd = false;
	public bool bIsTunnelMatched = false;
	public GSDSplineN TunnelCounterpartNode = null;
	
	//Bridges:
	public bool bIsBridge = false;
	public bool bIsBridgeStart = false;
	public bool bIsBridgeEnd = false;
	public bool bIsBridgeMatched = false;
	public GSDSplineN BridgeCounterpartNode = null;

	public GSDRoadIntersection GSDRI = null;
	
	public GSD.Roads.GSDIntersections.iConstructionMaker iConstruction;

	#region "Edge Objects"
	public List<EdgeObjectMaker> EdgeObjects;

	public void SetupEdgeObjects(bool bCollect = true){
		if(EdgeObjects == null){ EdgeObjects = new List<EdgeObjectMaker>(); }
		int eCount = EdgeObjects.Count;
		EdgeObjectMaker EOM = null;
		for(int i=0;i<eCount;i++){
			EOM = EdgeObjects[i];
			EOM.tNode = this;
			EOM.Setup(bCollect);
		}
	}

	public EdgeObjectMaker AddEdgeObject(){
		EdgeObjectMaker EOM = new EdgeObjectMaker();
		EOM.tNode = this;
		EOM.SetDefaultTimes(bIsEndPoint,tTime,NextTime,idOnSpline,GSDSpline.distance);
		EOM.StartPos = GSDSpline.GetSplineValue(EOM.StartTime);
		EOM.EndPos = GSDSpline.GetSplineValue(EOM.EndTime);
		EdgeObjects.Add(EOM);
		return EOM;
	}
	
	public void EdgeObjectQuickAdd(string tName){
		EdgeObjectMaker EOM = AddEdgeObject();
		EOM.LoadFromLibrary(tName,true);
		EOM.SetDefaultTimes(bIsEndPoint,tTime,NextTime,idOnSpline,GSDSpline.distance);
		EOM.tNode = this;
		EOM.Setup();
	}
	
	public void RemoveEdgeObject(int tIndex = -1,bool bSkipUpdate = false){
		if(EdgeObjects == null){ return; }
		if(EdgeObjects.Count == 0){ return; }
		if(tIndex < 0){
			if(EdgeObjects.Count > 0){
				EdgeObjects[EdgeObjects.Count-1].ClearEOM();
				EdgeObjects.RemoveAt(EdgeObjects.Count-1);	
			}
		}else{
			if(EdgeObjects.Count > tIndex){
				EdgeObjects[tIndex].ClearEOM();
				EdgeObjects.RemoveAt(tIndex);
			}
		}
		if(!bSkipUpdate){
			SetupEdgeObjects();
		}
	}
	
	public void RemoveAllEdgeObjects(bool bSkipUpdate = false){
		int SpamCheck = 0;
		while(EdgeObjects.Count > 0 && SpamCheck < 1000){
			RemoveEdgeObject(-1,bSkipUpdate);
			SpamCheck+=1;
		}
	}
	
	public void CopyEdgeObject(int tIndex){
		EdgeObjectMaker EOM = EdgeObjects[tIndex].Copy();
		EdgeObjects.Add(EOM);
		SetupEdgeObjects();
	}
	
	public void EdgeObjectLoadFromLibrary(int i, string tName){
		if(EdgeObjects == null){ EdgeObjects = new List<EdgeObjectMaker>(); }
		int eCount = EdgeObjects.Count;
		if(i > -1 && i < eCount){
			EdgeObjectMaker EOM = EdgeObjects[i];
			EOM.LoadFromLibrary(tName);
			EOM.SetDefaultTimes(bIsEndPoint,tTime,NextTime,idOnSpline,GSDSpline.distance);
			EOM.tNode = this;
			EOM.Setup();
		}
	}
	

	public void DetectInvalidEdgeObjects(){
		int mCount = EdgeObjects.Count;
		List<int> InvalidList = new List<int>();
		
		for(int i=0;i<mCount;i++){
			if(EdgeObjects[i].EdgeObject == null){
				InvalidList.Add(i);
			}
		}
		
		for(int i=(InvalidList.Count-1);i>=0;i--){
			RemoveEdgeObject(InvalidList[i],true);	
		}
		
		SetupEdgeObjects();
	}
#endregion
	
	#region "Extruded objects"
	public List<SplinatedMeshMaker> SplinatedObjects;
	
	public void SetupSplinatedMeshes(bool bCollect = true){
		if(SplinatedObjects == null){ SplinatedObjects = new List<SplinatedMeshMaker>(); }
		int eCount = SplinatedObjects.Count;
		SplinatedMeshMaker SMM = null;
		for(int i=0;i<eCount;i++){
			SMM = SplinatedObjects[i];
			SMM.Setup(true,bCollect);
		}
	}
	
	public int SplinatedMeshGetIndex(ref string UID){
		if(SplinatedObjects == null){ SplinatedObjects = new List<SplinatedMeshMaker>(); }
		int eCount = SplinatedObjects.Count;
		SplinatedMeshMaker SMM = null;
		for(int i=0;i<eCount;i++){
			SMM = SplinatedObjects[i];
			if(string.CompareOrdinal(SMM.UID,UID) == 0){
				return i;	
			}
		}
		return -1;
	}
	
	public void SetupSplinatedMesh(int i, bool bGetStrings = false){
		if(SplinatedObjects == null){ SplinatedObjects = new List<SplinatedMeshMaker>(); }
		int eCount = SplinatedObjects.Count;
		if(i > -1 && i < eCount){
			SplinatedMeshMaker SMM = SplinatedObjects[i];
			SMM.Setup(bGetStrings);
		}
	}
	
	public SplinatedMeshMaker AddSplinatedObject(){
        if (SplinatedObjects == null) { SplinatedObjects = new List<SplinatedMeshMaker>(); }
		SplinatedMeshMaker SMM = new SplinatedMeshMaker();
		SMM.Init(GSDSpline,this,transform);
		SplinatedObjects.Add(SMM);
		SMM.SetDefaultTimes(bIsEndPoint,tTime,NextTime,idOnSpline,GSDSpline.distance);
		SMM.StartPos = GSDSpline.GetSplineValue(SMM.StartTime);
		SMM.EndPos = GSDSpline.GetSplineValue(SMM.EndTime);
		return SMM;
	}
	
	public void SplinatedObjectQuickAdd(string tName){
		SplinatedMeshMaker SMM = AddSplinatedObject();
		SMM.LoadFromLibrary(tName,true);
		SMM.SetDefaultTimes(bIsEndPoint,tTime,NextTime,idOnSpline,GSDSpline.distance);
		SMM.Setup(true);
	}
	
	public void SplinatedObjectLoadFromLibrary(int i, string tName){
		if(SplinatedObjects == null){ SplinatedObjects = new List<SplinatedMeshMaker>(); }
		int eCount = SplinatedObjects.Count;
		if(i > -1 && i < eCount){
			SplinatedMeshMaker SMM = SplinatedObjects[i];
			SMM.SetDefaultTimes(bIsEndPoint,tTime,NextTime,idOnSpline,GSDSpline.distance);
			SMM.LoadFromLibrary(tName);
			SMM.Setup(true);
		}
	}
	
	public void CopySplinatedObject(ref SplinatedMeshMaker tSMM){
		SplinatedMeshMaker SMM = tSMM.Copy();
		SplinatedObjects.Add(SMM);
		SetupSplinatedMeshes();
	}
	
	public void RemoveSplinatedObject(int tIndex = -1,bool bSkipUpdate = false){
		if(SplinatedObjects == null){ return; }
		if(SplinatedObjects.Count == 0){ return; }
		SplinatedMeshMaker SMM = null;
		if(tIndex < 0){
			if(SplinatedObjects.Count > 0){
				SMM = SplinatedObjects[SplinatedObjects.Count-1];
				SMM.Kill();
				SplinatedObjects.RemoveAt(SplinatedObjects.Count-1);	
				SMM = null;
			}
		}else{
			if(SplinatedObjects.Count > tIndex){
				SMM = SplinatedObjects[tIndex];
				SMM.Kill();
				SplinatedObjects.RemoveAt(tIndex);
				SMM = null;
			}
		}
		if(!bSkipUpdate){
			SetupSplinatedMeshes();
		}
	}
	
	public void RemoveAllSplinatedObjects(bool bSkipUpdate = false){
		int SpamCheck = 0;
        if (SplinatedObjects != null) { 
		    while(SplinatedObjects.Count > 0 && SpamCheck < 1000){
			    RemoveSplinatedObject(-1,bSkipUpdate);
			    SpamCheck+=1;
		    }
        }
	}
	
	public void DetectInvalidSplinatedObjects(){
		int mCount = SplinatedObjects.Count;
		List<int> InvalidList = new List<int>();
		
		for(int i=0;i<mCount;i++){
			if(SplinatedObjects[i].Output == null){
				InvalidList.Add(i);
			}
		}
		
		for(int i=(InvalidList.Count-1);i>=0;i--){
			RemoveSplinatedObject(InvalidList[i],true);	
		}
		
		SetupSplinatedMeshes();
	}
	#endregion
	
	public void LoadWizardObjectsFromLibrary(string tFileName, bool _bIsDefault, bool _bIsBridge){
		if(_bIsBridge){
			RemoveAllSplinatedObjects(true);
			RemoveAllEdgeObjects(true);
		}
		GSD.Roads.GSDRoadUtil.LoadNodeObjects(tFileName,this,_bIsDefault,_bIsBridge);
	}

	public void Setup (Vector3 _p, Quaternion _q, Vector2 _io, float _tTime, string _name){
		if(!Application.isEditor){ return; }
		pos = _p;
		rot = _q;
		EaseIO = _io;
		tTime = _tTime;
		name = _name;
		if(EdgeObjects == null){ EdgeObjects = new List<EdgeObjectMaker>(); }
	}
	
	public void SetupUniqueIdentifier(){
		if(UID == null || UID.Length < 4){
			UID = System.Guid.NewGuid().ToString();}
	}
	
	#region "Gizmos"
//	private void TerrainDebugging(){
//			GSD.Roads.GSDRoadUtil.Construction3DTri tTri = null;
//			Vector3 tHeightVec = new Vector3(0f,10f,0f);
//			if(tTriList != null){
//				
//
////				bool bOddToggle = false;
////				for(int i=0;i<tTriList.Count;i+=2){
//////					if(i < 210){ continue; }
//////					if(i > 230){ break; }
////					if(i < 1330){ continue; }
////					if(i > 1510 || i > (tTriList.Count-10)){ break; }
////					tTri = tTriList[i];
////					
////					if(bOddToggle){
////						Gizmos.color = new Color(0f,1f,1f,1f);
////					}else{
////						Gizmos.color = new Color(1f,1f,0f,1f);
////					}
////					
////					Gizmos.DrawLine(tTri.P1,tTri.P2);
////					Gizmos.DrawLine(tTri.P1,tTri.P3);
////					Gizmos.DrawLine(tTri.P2,tTri.P3);
//////					Gizmos.color = new Color(0f,1f,0f,1f);
//////					Gizmos.DrawLine(tTri.pMiddle,(tTri.normal*100f)+tTri.pMiddle);
////
////					
////					
////					if(bOddToggle){
////						Gizmos.color = new Color(0f,1f,1f,1f);
////					}else{
////						Gizmos.color = new Color(1f,1f,0f,1f);
////					}
////					
////					Gizmos.DrawLine(tTri.P1+tHeightVec,tTri.P2+tHeightVec);
////					Gizmos.DrawLine(tTri.P1+tHeightVec,tTri.P3+tHeightVec);
////					Gizmos.DrawLine(tTri.P3+tHeightVec,tTri.P2+tHeightVec);
////					Gizmos.color = new Color(0f,1f,0f,1f);
////					Gizmos.DrawLine(tTri.pMiddle+tHeightVec,(tTri.normal*100f)+tTri.pMiddle+tHeightVec);
////					
//////					
////					
////					tTri = tTriList[i+1];
////					if(bOddToggle){
////						Gizmos.color = new Color(0f,1f,1f,1f);
////					}else{
////						Gizmos.color = new Color(1f,1f,0f,1f);
////					}
////					
////					Gizmos.DrawLine(tTri.P1,tTri.P2);
////					Gizmos.DrawLine(tTri.P1,tTri.P3);
////					Gizmos.DrawLine(tTri.P2,tTri.P3);
////					
////					if(bOddToggle){
////						Gizmos.color = new Color(0f,1f,1f,1f);
////					}else{
////						Gizmos.color = new Color(1f,1f,0f,1f);
////					}
////					
////					Gizmos.DrawLine(tTri.P1+tHeightVec,tTri.P2+tHeightVec);
////					Gizmos.DrawLine(tTri.P1+tHeightVec,tTri.P3+tHeightVec);
////					Gizmos.DrawLine(tTri.P3+tHeightVec,tTri.P2+tHeightVec);
////					Gizmos.color = new Color(0f,1f,0f,1f);
////					Gizmos.DrawLine(tTri.pMiddle+tHeightVec,(tTri.normal*100f)+tTri.pMiddle+tHeightVec);
////					
////	
////					
////					bOddToggle = !bOddToggle;
//////					Gizmos.DrawCube(tRectList[i].pMiddle,new Vector3(1f,0.5f,1f));
////				}
//				
//				
////				Gizmos.color = new Color(0f,1f,0f,1f);
////				Terrain tTerrain = GameObject.Find("Terrain").GetComponent<Terrain>();
////				Vector3 HMVect = default(Vector3);
////				float tHeight = 0f;
////				for(int i=0;i<tHMList.Count;i++){
////					Gizmos.color = new Color(0f,1f,0f,1f);
////					Gizmos.DrawCube(tHMList[i] + new Vector3(0f,1f,0f),new Vector3(0.1f,2f,0.1f));
////				
////					if(tTerrain){
////						tHeight = tTerrain.SampleHeight(tHMList[i]) + tTerrain.transform.position.y;
////						
////						if(tHeight != tHMList[i].y){
////							HMVect = new Vector3(tHMList[i].x,tHeight,tHMList[i].z);
////							
////							if(Mathf.Approximately(9.584141f,tHMList[i].y)){
////								int sdagsdgsd =1;	
////							}
////							
////							Gizmos.color = new Color(0f,0f,1f,1f);
////							Gizmos.DrawWireCube(HMVect + new Vector3(0f,1f,0f),new Vector3(0.15f,2f,0.15f));
////						}
////					}
////				}
//			}
//			
////			Vector3 P1 = new Vector3(480.7f,50f,144.8f);
////			Vector3 P2 = new Vector3(517.3f,60f,128.9f);
////			Vector3 P3 = new Vector3(518.8f,60f,132.7f);
////			Vector3 P4 = new Vector3(481.3f,50f,146.4f);
////			
////			
////			Gizmos.color = new Color(1f,0f,0f,1f);
////			Gizmos.DrawCube(P1,new Vector3(1f,1f,1f));
////			Gizmos.DrawCube(P2,new Vector3(1f,1f,1f));
////			Gizmos.DrawCube(P3,new Vector3(1f,1f,1f));
////			Gizmos.DrawCube(P4,new Vector3(1f,1f,1f));
////			
////			tRect = new GSD.Roads.GSDRoadUtil.Construction3DRect(P1,P2,P3,P4);
////
////			Gizmos.color = new Color(0f,0f,1f,1f);
////			Gizmos.DrawCube(tRect.pMiddle,new Vector3(0.1f,0.1f,0.1f));
////			
////			//This creates normalized line:
////			Gizmos.color = new Color(0f,1f,0f,1f);
////			Vector3 tVect2 = (tRect.normal.normalized*100f)+tRect.pMiddle;
////			Gizmos.DrawLine(tRect.pMiddle,tVect2);
////			
////			//This creates emulated terrain point and straight up line:
////			Gizmos.color = new Color(0f,1f,1f,1f);
////			Vector3 F1 = new Vector3(500f,0f,138.5f);
////			Gizmos.DrawLine(F1,F1+ new Vector3(0f,100f,0f));
////			
////			//This creates diagonal line of plane.
////			Gizmos.color = new Color(1f,0f,1f,1f);
////			Gizmos.DrawLine(P1,P3);
////			Gizmos.DrawLine(P2,P4);
////			
////			//This creates intersection point:
////			Vector3 tPos = default(Vector3);
////			LinePlaneIntersection(out tPos,F1,Vector3.up,tRect.normal.normalized,tRect.pMiddle);
////			Gizmos.color = new Color(1f,1f,0f,1f);
////			Gizmos.DrawCube(tPos,new Vector3(0.3f,0.3f,0.3f));
//	}
//	
	public List<GSD.Roads.GSDRoadUtil.Construction3DTri> tTriList;
	public List<Vector3> tHMList;
	public bool bGizmoDrawIntersectionHighlight = false;
	
	void OnDrawGizmos (){ 
		if(GSDSpline == null){ return; }
		if(GSDSpline.tRoad == null){ return; }
		if(!GSDSpline.tRoad.opt_GizmosEnabled){ return; }
		if(bIgnore){ return; }
		if(bIsIntersection){ return; }
		if(bSpecialEndNode){ return; }
		if(bSpecialEndNode_IsEnd || bSpecialEndNode_IsStart){ return; }
		if(bGizmoDrawIntersectionHighlight && !bSpecialEndNode && bIsIntersection){
			Gizmos.color = GSDSpline.tRoad.Color_NodeInter;
			Gizmos.DrawCube(transform.position+new Vector3(0f,2f,0f), new Vector3(32f,4f,32f));
			return;
		}
		if(bSpecialRoadConnPrimary){
			Gizmos.color = GSDSpline.tRoad.Color_NodeConnColor;
			Gizmos.DrawCube(transform.position+new Vector3(0f,7.5f,0f), new Vector3(5f,15f,5f));
		}else{
			Gizmos.color = GSDSpline.tRoad.Color_NodeDefaultColor;
			Gizmos.DrawCube(transform.position+new Vector3(0f,6f,0f), new Vector3(2f,11f,2f));
		}
	}

	void OnDrawGizmosSelected(){
		if(!GSDSpline.tRoad.opt_GizmosEnabled){ return; }
		if(bIgnore){ return; }
		if(bIsIntersection){ return; }
		if(bSpecialEndNode){ return; }
		if(bSpecialEndNode_IsEnd || bSpecialEndNode_IsStart){ return; }
		if(bGizmoDrawIntersectionHighlight && !bSpecialEndNode && bIsIntersection){
			Gizmos.color = new Color(0f,1f,0f,0.6f);
			Gizmos.DrawCube(transform.position+new Vector3(0f,2f,0f), new Vector3(32f,4f,32f));
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(transform.position+new Vector3(0f,6.25f,0f), new Vector3(3.5f,12.5f,3.5f));
	}
	#endregion

	#region "Grade"
	public void SetGradePercent(int mCount){
		Vector3 P1 = default(Vector3);
		Vector3 P2 = default(Vector3);
		float tDist;
		float tGrade;
		bool bIsNone = false;

        if (mCount < 2)
        {
            GradeToPrev = "NA";	
            GradeToNext = "NA";
			GradeToNextValue = 0f;
			GradeToPrevValue = 0f;
        }
		
//		if(idOnSpline == 4){
//			int kasfdjsakf = 1;	
//		}

        if (!bIsEndPoint && !bSpecialEndNode && mCount > 1 && ((idOnSpline+1) < GSDSpline.mNodes.Count))
        {
			P1 = pos;
			P2 = GSDSpline.mNodes[idOnSpline+1].pos;
			
			if(bIsNone){
				tGrade = 0f;
			}else{
				tDist = Vector2.Distance(new Vector2(P1.x,P1.z),new Vector2(P2.x,P2.z));
				tGrade = ((P2.y-P1.y)/tDist) * 100;	
			}
			GradeToNextValue = tGrade;
			GradeToNext = tGrade.ToString("0.##\\%");
		}else{
			GradeToNext = "NA";
			GradeToNextValue = 0f;
		}
		
		if(idOnSpline > 0 && !bSpecialEndNode && mCount > 1 && ((idOnSpline-1) >= 0)){
			P1 = pos;
			P2 = GSDSpline.mNodes[idOnSpline-1].pos;

			if(bIsNone){
				tGrade = 0f;
			}else{
				tDist = Vector2.Distance(new Vector2(P1.x,P1.z),new Vector2(P2.x,P2.z));
				tGrade = ((P2.y-P1.y)/tDist) * 100;	
			}
			GradeToPrevValue = tGrade;
			GradeToPrev = tGrade.ToString("0.##\\%");
		}else{
			GradeToPrev = "NA";	
			GradeToPrevValue = 0f;
		}
	}

	public Vector3 FilterMaxGradeHeight(Vector3 tPos, out float MinY, out float MaxY){
		Vector3 tVect = tPos;
		tVect.y = pos.y;
		float CurrentDistance = Vector2.Distance(new Vector2(pos.x,pos.z),new Vector2(tPos.x,tPos.z));
//		float CurrentDistance2 = Vector3.Distance(pos,tVect);
//		float CurrentYDiff = tPos.y - pos.y;
//		float CurrentGrade = CurrentYDiff/CurrentDistance;
		//Get max/min grade height position for this currrent tDist distance:
		MaxY = (GSDSpline.tRoad.opt_MaxGrade * CurrentDistance) + pos.y;
		MinY = pos.y - (GSDSpline.tRoad.opt_MaxGrade * CurrentDistance);
		
		//(tPos.y-pos.y)/CurrentDistance
		
		
//		float DifferenceFromMax = -1;
		if(tPos.y > MaxY){
//			DifferenceFromMax = tPos.y - MaximumY;
			tPos.y = MaxY;
		}
		if(tPos.y < MinY){
//			DifferenceFromMax = MinimumY - tPos.y;
			tPos.y = MinY;
		}
		
		return tPos;
	}
	
	public void EnsureGradeValidity(int iStart = -1, bool bIsAddToEnd = false){
		if(GSDSpline == null){ return; }
		GSDSplineN PrevNode = null; 
		GSDSplineN NextNode = null;
		
		if(bIsAddToEnd && GSDSpline.GetNodeCount() > 0){
			PrevNode = GSDSpline.mNodes[GSDSpline.GetNodeCount()-1];
		}else{
			if(iStart == -1){
				PrevNode = GSDSpline.GetPrevLegitimateNode(idOnSpline);
			}else{
				PrevNode = GSDSpline.GetPrevLegitimateNode(iStart);
			}
		}
		if(PrevNode == null){ return; }
		Vector3 tVect = transform.position;

		float tMinY1 = 0f;
		float tMinY2 = 0f;
		float tMaxY1 = 0f;
		float tMaxY2 = 0f;
		if(PrevNode != null){
			PrevNode.FilterMaxGradeHeight(tVect,out tMinY1, out tMaxY1);
		}
		if(NextNode != null){
			NextNode.FilterMaxGradeHeight(tVect,out tMinY2, out tMaxY2);
		}
		
		bool bPrevNodeGood = false;
		bool bNextNodeGood = false;
		
		if(tVect.y > tMinY1 && tVect.y < tMaxY1){
			bPrevNodeGood = true;
		}
		if(tVect.y > tMinY2 && tVect.y < tMaxY2){
			bNextNodeGood = true;
		}
		
		if(!bPrevNodeGood && !bNextNodeGood && PrevNode != null && NextNode != null){
			float tMaxY3 = Mathf.Min(tMaxY1,tMaxY2);
			float tMinY3 = Mathf.Max(tMinY1,tMinY2);
			if(tVect.y < tMinY3){
				tVect.y = tMinY3;
			}else if(tVect.y > tMaxY3){
				tVect.y = tMaxY3;
			}
		}else{
			if(!bPrevNodeGood && PrevNode != null){
				if(tVect.y < tMinY1){
					tVect.y = tMinY1;
				}else if(tVect.y > tMaxY1){
					tVect.y = tMaxY1;
				}
			}else if(!bNextNodeGood && NextNode != null){
				if(tVect.y < tMinY2){
					tVect.y = tMinY2;
				}else if(tVect.y > tMaxY2){
					tVect.y = tMaxY2;
				}
			}
		}
	
		transform.position = tVect;
	}
	#endregion

	#region "Util"
	public void ResetNavigationData(){
		id_connected = null;
		id_connected = new List<int>();
		node_connected = null;
		node_connected = new List<GSDSplineN>();
	}
	
	public void BreakConnection(){
		GSDSplineN tNode2 = SpecialNodeCounterpart;
				
		if(bSpecialEndNode_IsStart){
			GSDSpline.bSpecialStartControlNode = false;
			GSDSpline.bSpecialEndNode_IsStart_Delay = false;
		}else if(bSpecialEndNode_IsEnd){
			GSDSpline.bSpecialEndControlNode = false;
			GSDSpline.bSpecialEndNode_IsEnd_Delay = false;
		}
		if(tNode2.bSpecialEndNode_IsStart){
			tNode2.GSDSpline.bSpecialStartControlNode = false;
			tNode2.GSDSpline.bSpecialEndNode_IsStart_Delay = false;
		}else if(tNode2.bSpecialEndNode_IsEnd){
			tNode2.GSDSpline.bSpecialEndControlNode = false;
			tNode2.GSDSpline.bSpecialEndNode_IsEnd_Delay = false;
		}
		
		SpecialNodeCounterpart = null;
		bSpecialEndNode = false;
		bSpecialEndNode_IsEnd = false;
		bSpecialEndNode_IsStart = false;
		bSpecialRoadConnPrimary = false;
		tNode2.SpecialNodeCounterpart = null;
		tNode2.bSpecialEndNode = false;
		tNode2.bSpecialEndNode_IsEnd = false;
		tNode2.bSpecialEndNode_IsStart = false;
		tNode2.bSpecialRoadConnPrimary = false;
		
		tNode2.SpecialNodeCounterpart_Master.bSpecialRoadConnPrimary = false;
		SpecialNodeCounterpart_Master.bSpecialRoadConnPrimary = false;
		
		Object.DestroyImmediate(tNode2.transform.gameObject);
		Object.DestroyImmediate(transform.gameObject);
	}
	
	public void SetupSplinationLimits(){
		//Disallowed nodes:
		if(!CanSplinate()){
			MinSplination = tTime;
			MaxSplination = tTime;
			return;
		}
		
		//Figure out min splination:
		GSDSplineN tNode = null;
		MinSplination = tTime;
		for(int i=idOnSpline;i>=0;i--){
			tNode = GSDSpline.mNodes[i];
			if(tNode.CanSplinate()){
				MinSplination = tNode.tTime;
			}else{
				break;	
			}
		}
		
		//Figure out max splination:
		MaxSplination = tTime;
		int mCount = GSDSpline.GetNodeCount();
		for(int i=idOnSpline;i<mCount;i++){
			tNode = GSDSpline.mNodes[i];
			if(tNode.CanSplinate()){
				MaxSplination = tNode.tTime;
			}else{
				break;	
			}
		}
	}
	
	public bool CanSplinate(){
		if(bIsIntersection || bSpecialEndNode){// || bIsBridge_PreNode || bIsBridge_PostNode){
			return false;
		}else{
			return true;
		}
	}
	
	public bool IsLegitimate(){
		if(bIsIntersection || bSpecialEndNode){// || bIsBridge_PreNode || bIsBridge_PostNode){
			return false;
		}else{
			return true;
		}
	}
	
	public bool IsLegitimateGrade(){
		if(bSpecialEndNode){// || bIsBridge_PreNode || bIsBridge_PostNode){
			return false;
		}else{
			return true;
		}
	}
	#endregion

	#region "Cut materials storage and setting"
	public GameObject RoadCut_world = null;
	public GameObject ShoulderCutR_world = null;
	public GameObject ShoulderCutL_world = null;
	public Material[] RoadCut_world_Mats;
	public Material[] ShoulderCutR_world_Mats;
	public Material[] ShoulderCutL_world_Mats;
	
	public GameObject RoadCut_marker = null;
	public GameObject ShoulderCutR_marker = null;
	public GameObject ShoulderCutL_marker = null;
	public Material[] RoadCut_marker_Mats;
	public Material[] ShoulderCutR_marker_Mats;
	public Material[] ShoulderCutL_marker_Mats;
	
	public PhysicMaterial RoadCut_PhysicMat;
	public PhysicMaterial ShoulderCutR_PhysicMat;
	public PhysicMaterial ShoulderCutL_PhysicMat;
	
	/// <summary>
	/// Stores the cut materials. For use in UpdateCuts(). See UpdateCuts() in this code file for further description of this system.
	/// </summary>
	public void StoreCuts(){
		//Buffers:
		MeshRenderer MR = null;
		MeshCollider MC = null;
		
		//World cuts first:
		if(RoadCut_world != null){
			MR = RoadCut_world.GetComponent<MeshRenderer>();
			MC = RoadCut_world.GetComponent<MeshCollider>();
			if(MR != null){ RoadCut_world_Mats = MR.sharedMaterials; }
			if(MC != null){ RoadCut_PhysicMat = MC.material; }
			RoadCut_world = null;//Nullify reference only
		}
		if(ShoulderCutR_world != null){
			MR = ShoulderCutR_world.GetComponent<MeshRenderer>();
			MC = ShoulderCutR_world.GetComponent<MeshCollider>();
			if(MR != null){ ShoulderCutR_world_Mats = MR.sharedMaterials; }
			if(MC != null){ ShoulderCutR_PhysicMat = MC.material; }
			ShoulderCutR_world = null;
		}
		if(ShoulderCutL_world != null){
			MR = ShoulderCutL_world.GetComponent<MeshRenderer>();
			MC = ShoulderCutL_world.GetComponent<MeshCollider>();
			if(MR != null){ ShoulderCutL_world_Mats = MR.sharedMaterials; }
			if(MC != null){ ShoulderCutL_PhysicMat = MC.material; }
			ShoulderCutL_world = null;
		}
		//Markers:
		if(RoadCut_marker != null){
			MR = RoadCut_marker.GetComponent<MeshRenderer>();
			if(MR != null){ RoadCut_marker_Mats = MR.sharedMaterials; }
			RoadCut_marker = null;
		}
		if(ShoulderCutR_marker != null){
			MR = ShoulderCutR_marker.GetComponent<MeshRenderer>();
			if(MR != null){ ShoulderCutR_marker_Mats = MR.sharedMaterials; }
			ShoulderCutR_marker = null;
		}
		if(ShoulderCutL_marker != null){
			MR = ShoulderCutL_marker.GetComponent<MeshRenderer>();
			if(MR != null){ ShoulderCutL_marker_Mats = MR.sharedMaterials; }
			ShoulderCutL_marker = null;
		}
	}
	/// <summary>
	/// Updates the cut materials. Called upon creation of the cuts to set the newly cut materials to previously set materials.
	/// For instance, if the user set a material on a road cut, and then regenerated the road, this function will apply the mats that the user applied.
	/// </summary>
	public void UpdateCuts(){
		//Buffers:
		MeshRenderer MR = null;
		MeshCollider MC = null;
		
		//World:
		if(RoadCut_world_Mats != null && RoadCut_world_Mats.Length > 0 && RoadCut_world){
			MR = RoadCut_world.GetComponent<MeshRenderer>();
			if(!MR){ RoadCut_world.AddComponent<MeshRenderer>(); }
			if(MR != null){ MR.materials = RoadCut_world_Mats; }
		}
		if(RoadCut_PhysicMat != null && RoadCut_world){
			MC = RoadCut_world.GetComponent<MeshCollider>();
			if(MC != null){ MC.material = RoadCut_PhysicMat; }
		}
		
		if(ShoulderCutR_world_Mats != null && ShoulderCutR_world_Mats.Length > 0 && ShoulderCutR_world){
			MR = ShoulderCutR_world.GetComponent<MeshRenderer>();
			if(!MR){ ShoulderCutR_world.AddComponent<MeshRenderer>(); }
			if(MR != null){MR.materials = ShoulderCutR_world_Mats; }
		}
		if(ShoulderCutR_PhysicMat != null && ShoulderCutR_world){
			MC = ShoulderCutR_world.GetComponent<MeshCollider>();
			if(MC != null){ MC.material = ShoulderCutR_PhysicMat; }
		}
		
		if(ShoulderCutL_world_Mats != null && ShoulderCutL_world_Mats.Length > 0 && ShoulderCutL_world){
			MR = ShoulderCutL_world.GetComponent<MeshRenderer>();
			if(!MR){ ShoulderCutL_world.AddComponent<MeshRenderer>(); }
			if(MR != null){MR.materials = ShoulderCutL_world_Mats; }
		}
		if(ShoulderCutL_PhysicMat != null && ShoulderCutL_world){
			MC = ShoulderCutL_world.GetComponent<MeshCollider>();
			if(MC != null){ MC.material = ShoulderCutL_PhysicMat; }
		}
		
		//Markers:
		if(RoadCut_marker_Mats != null && RoadCut_marker_Mats.Length > 0 && RoadCut_marker){
			MR = RoadCut_marker.GetComponent<MeshRenderer>();
			if(!MR){ RoadCut_marker.AddComponent<MeshRenderer>(); }
			if(MR != null){ MR.materials = RoadCut_marker_Mats; }
		}
		if(ShoulderCutR_marker_Mats != null && ShoulderCutR_marker_Mats.Length > 0 && ShoulderCutR_marker){
			MR = ShoulderCutR_marker.GetComponent<MeshRenderer>();
			if(!MR){ ShoulderCutR_marker.AddComponent<MeshRenderer>(); }
			if(MR != null){MR.materials = ShoulderCutR_marker_Mats; }
		}
		if(ShoulderCutL_marker_Mats != null && ShoulderCutL_marker_Mats.Length > 0 && ShoulderCutL_marker){
			MR = ShoulderCutL_marker.GetComponent<MeshRenderer>();
			if(!MR){ ShoulderCutL_marker.AddComponent<MeshRenderer>(); }
			if(MR != null){MR.materials = ShoulderCutL_marker_Mats; }
		}
		
		if(RoadCut_marker != null){
			MR = RoadCut_marker.GetComponent<MeshRenderer>();
			if(MR == null || MR.sharedMaterial == null){
				Object.DestroyImmediate(RoadCut_marker);
			}
		}
		if(ShoulderCutR_marker != null){
			MR = ShoulderCutR_marker.GetComponent<MeshRenderer>();
			if(MR == null || MR.sharedMaterial == null){
				Object.DestroyImmediate(ShoulderCutR_marker);
			}
		}
		if(ShoulderCutL_marker != null){
			MR = ShoulderCutL_marker.GetComponent<MeshRenderer>();
			if(MR == null || MR.sharedMaterial == null){
				Object.DestroyImmediate(ShoulderCutL_marker);
			}
		}
	}
	
	/// <summary>
	/// Clears the cut materials. Called when user hits button on road editor inspector gui.
	/// </summary>
	public void ClearCuts(){
		RoadCut_world_Mats = null;
		ShoulderCutR_world_Mats = null;
		ShoulderCutL_world_Mats = null;
		RoadCut_marker_Mats = null;
		ShoulderCutR_marker_Mats = null;
		ShoulderCutL_marker_Mats = null;
		RoadCut_PhysicMat = null;
		ShoulderCutR_PhysicMat = null;
		ShoulderCutL_PhysicMat = null;
	}
#endregion

	#region "Bridges"
	public void BridgeToggleStart(){
		//If switching to end, find associated bridge 
		if(bIsBridgeStart){ 
			BridgeStart(); 
		}else{
			BridgeDestroy();	
		}
	}
	
	public void BridgeToggleEnd(){
		//If switching to end, find associated bridge 
		if(bIsBridgeEnd){ 
			int mCount = GSDSpline.GetNodeCount();
			GSDSplineN tNode = null;
			for(int i=1;i<(mCount-1);i++){
				tNode = GSDSpline.mNodes[i];
				if(tNode.bIsBridgeStart && !tNode.bIsBridgeMatched){
					tNode.BridgeToggleStart();
					if(tNode.bIsBridgeMatched && tNode.BridgeCounterpartNode == this){ return; }
				}
			}
		}else{
			BridgeDestroy();	
		}
	}
	
	private void BridgeStart(){
		//Cycle through nodes until you find another end or another start (in this case, no creation, encountered another bridge):
		int EndID = idOnSpline + 1;
		int mCount = GSDSpline.GetNodeCount();
		if(bIsEndPoint){ 
			//Attempted to make end point node a bridge node:
			bIsBridgeStart = false;
			return;
		}
		if(EndID >= mCount){
			//Attempted to make last node a bridge node:
			bIsBridgeStart = false;
			return;
		}else if(idOnSpline == 0){
			//Attempted to make first node a bridge node:
			bIsBridgeStart = false;
			return;
		}
		
		bIsBridgeMatched = false;
		BridgeCounterpartNode = null;
		int StartI = idOnSpline+1;
		GSDSplineN tNode = null;
		for(int i=StartI;i<mCount;i++){
			tNode = GSDSpline.mNodes[i];
			if(tNode.bIsIntersection){
				//Encountered intersection. End search.
				return;	
			}
			if(tNode.bIsBridgeStart){
				//Encountered another bridge. Return:
				return;
			}
			if(tNode.bIgnore){
				continue;	
			}
			if(tNode.bIsBridgeEnd){
				bIsBridgeMatched = true;
				tNode.bIsBridgeMatched = true;
				BridgeCounterpartNode = tNode;
				tNode.BridgeCounterpartNode = this;
				GSDSpline.Setup_Trigger();
				return;
			}
		}
	}	
	
	private void BridgeDestroy(){
		if(BridgeCounterpartNode != null){
			BridgeCounterpartNode.BridgeResetValues();	
		}
		BridgeResetValues();
		GSDSpline.Setup_Trigger();
	}

	public void BridgeResetValues(){
		bIsBridge = false;
		bIsBridgeStart = false;
		bIsBridgeEnd = false;
		bIsBridgeMatched = false;
		BridgeCounterpartNode = null;
	}
	
	public bool CanBridgeStart(){
		if(bIsBridgeStart){ return true; }
		if(bIsBridgeEnd){ return false; }
		if(bIsEndPoint){ return false; }
		
		int mCount = GSDSpline.GetNodeCount();
		
		if(idOnSpline > 0){
			if(GSDSpline.mNodes[idOnSpline-1].bIsBridgeStart){
				return false;	
			}
		}
		
		if(idOnSpline < (mCount-1)){
			if(GSDSpline.mNodes[idOnSpline+1].bIsBridgeStart){
				return false;	
			}
			
			if(GSDSpline.bSpecialEndControlNode){
				if((mCount-3 > 0) && idOnSpline == mCount-3){
					return false;	
				}
			}else{
				if((mCount-2 > 0) &&idOnSpline == mCount-2){
					return false;	
				}
			}
		}
		
		if(GSDSpline.IsInBridge(tTime)){
			return false;	
		}
		
		return true;
	}
	
	public bool CanBridgeEnd(){
		if(bIsBridgeEnd){ return true; }
		if(bIsBridgeStart){ return false; }
		if(bIsEndPoint){ return false; }
		
		int mCount = GSDSpline.GetNodeCount();
		
		if(idOnSpline < (mCount-1)){
			if(GSDSpline.bSpecialStartControlNode){
				if(idOnSpline == 2){
					return false;	
				}
			}else{
				if(idOnSpline == 1){
					return false;	
				}
			}
		}

		for(int i=idOnSpline;i>=0;i--){
			if(GSDSpline.mNodes[i].bIsBridgeStart){
				if(!GSDSpline.mNodes[i].bIsBridgeMatched){
					return true;
				}else{
					return false;	
				}
			}
		}
		
		return false;
	}
	#endregion
	
	#region "Tunnels"
	public void TunnelToggleStart(){
		//If switching to end, find associated Tunnel 
		if(bIsTunnelStart){ 
			TunnelStart(); 
		}else{
			TunnelDestroy();	
		}
	}
	
	public void TunnelToggleEnd(){
		//If switching to end, find associated Tunnel 
		if(bIsTunnelEnd){ 
			int mCount = GSDSpline.GetNodeCount();
			GSDSplineN tNode = null;
			for(int i=1;i<(mCount-1);i++){
				tNode = GSDSpline.mNodes[i];
				if(tNode.bIsTunnelStart && !tNode.bIsTunnelMatched){
					tNode.TunnelToggleStart();
					if(tNode.bIsTunnelMatched && tNode.TunnelCounterpartNode == this){ return; }
				}
			}
		}else{
			TunnelDestroy();	
		}
	}
	
	private void TunnelStart(){
		//Cycle through nodes until you find another end or another start (in this case, no creation, encountered another Tunnel):
		int EndID = idOnSpline + 1;
		int mCount = GSDSpline.GetNodeCount();
		if(bIsEndPoint){ 
			//Attempted to make end point node a Tunnel node:
			bIsTunnelStart = false;
			return;
		}
		if(EndID >= mCount){
			//Attempted to make last node a Tunnel node:
			bIsTunnelStart = false;
			return;
		}else if(idOnSpline == 0){
			//Attempted to make first node a Tunnel node:
			bIsTunnelStart = false;
			return;
		}
		
		bIsTunnelMatched = false;
		TunnelCounterpartNode = null;
		int StartI = idOnSpline+1;
		GSDSplineN tNode = null;
		for(int i=StartI;i<mCount;i++){
			tNode = GSDSpline.mNodes[i];
			if(tNode.bIsIntersection){
				//Encountered intersection. End search.
				return;	
			}
			if(tNode.bIsTunnelStart){
				//Encountered another Tunnel. Return:
				return;
			}
			if(tNode.bIgnore){
				continue;	
			}
			if(tNode.bIsTunnelEnd){
				bIsTunnelMatched = true;
				tNode.bIsTunnelMatched = true;
				TunnelCounterpartNode = tNode;
				tNode.TunnelCounterpartNode = this;
				GSDSpline.Setup_Trigger();
				return;
			}
		}
	}	
	
	private void TunnelDestroy(){
		if(TunnelCounterpartNode != null){
			TunnelCounterpartNode.TunnelResetValues();	
		}
		TunnelResetValues();
		GSDSpline.Setup_Trigger();
	}

	public void TunnelResetValues(){
		bIsTunnel = false;
		bIsTunnelStart = false;
		bIsTunnelEnd = false;
		bIsTunnelMatched = false;
		TunnelCounterpartNode = null;
	}
	
	public bool CanTunnelStart(){
		if(bIsTunnelStart){ return true; }
		if(bIsTunnelEnd){ return false; }
		if(bIsEndPoint){ return false; }
		
		int mCount = GSDSpline.GetNodeCount();
		
		if(idOnSpline > 0){
			if(GSDSpline.mNodes[idOnSpline-1].bIsTunnelStart){
				return false;	
			}
		}
		
		if(idOnSpline < (mCount-1)){
			if(GSDSpline.mNodes[idOnSpline+1].bIsTunnelStart){
				return false;	
			}
			
			if(GSDSpline.bSpecialEndControlNode){
				if((mCount-3 > 0) && idOnSpline == mCount-3){
					return false;	
				}
			}else{
				if((mCount-2 > 0) &&idOnSpline == mCount-2){
					return false;	
				}
			}
		}
		
		if(GSDSpline.IsInTunnel(tTime)){
			return false;	
		}
		
		return true;
	}
	
	public bool CanTunnelEnd(){
		if(bIsTunnelEnd){ return true; }
		if(bIsTunnelStart){ return false; }
		if(bIsEndPoint){ return false; }
		
		int mCount = GSDSpline.GetNodeCount();
		
		if(idOnSpline < (mCount-1)){
			if(GSDSpline.bSpecialStartControlNode){
				if(idOnSpline == 2){
					return false;	
				}
			}else{
				if(idOnSpline == 1){
					return false;	
				}
			}
		}

		for(int i=idOnSpline;i>=0;i--){
			if(GSDSpline.mNodes[i].bIsTunnelStart){
				if(!GSDSpline.mNodes[i].bIsTunnelMatched){
					return true;
				}else{
					return false;	
				}
			}
		}
		
		return false;
	}
	#endregion
	
	#region "Is straight line to next node"
	public bool IsStraight(){
		int id1 = idOnSpline-1;
		int id2 = idOnSpline+1;
		int id3 = idOnSpline+2;
		int mCount = GSDSpline.GetNodeCount();
		
		if(id1 > -1 && id1 < mCount){
			if(!IsApproxTwoThirds(ref pos,GSDSpline.mNodes[id1].pos)){
				return false;
			}
		}
		
		if(id2 > -1 && id2 < mCount){
			if(!IsApproxTwoThirds(ref pos,GSDSpline.mNodes[id2].pos)){
				return false;
			}
		}
		
		if(id3 > -1 && id3 < mCount){
			if(!IsApproxTwoThirds(ref pos,GSDSpline.mNodes[id3].pos)){
				return false;
			}
		}
		
		return true;
	}
			
	private static bool IsApproxTwoThirds(ref Vector3 V1, Vector3 V2){
		int cCount = 0;
		if(GSDRootUtil.IsApproximately(V1.x,V2.x,0.02f)){
			cCount+=1;
		}
		if(GSDRootUtil.IsApproximately(V1.y,V2.y,0.02f)){
			cCount+=1;
		}
		if(GSDRootUtil.IsApproximately(V1.z,V2.z,0.02f)){
			cCount+=1;
		}
	
		if(cCount == 2){
			return true;
		}else{
			return false;
		}
	}
	#endregion
	#endif
	
	
	void Start(){
		#if UNITY_EDITOR
			//Do nothing.
		#else
			this.enabled = false;
		#endif
	}
	
	public void ToggleHideFlags(bool bIsHidden){
		if(bIsHidden){	
			this.hideFlags = HideFlags.HideInHierarchy;
			transform.hideFlags = HideFlags.HideInHierarchy;
		}else{
			this.hideFlags = HideFlags.None;
			transform.hideFlags = HideFlags.None;
        }
	}
}