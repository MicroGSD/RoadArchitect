#region "Imports"
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using GSD;
#endif
#endregion
public class GSDSplineC : MonoBehaviour{
	#if UNITY_EDITOR
	public List<GSDSplineN> mNodes = new List<GSDSplineN>();
	public GameObject mSplineRoot;
	public GSDRoad tRoad;
	public float distance = -1f;
	public Vector3[] CachedPoints;
	private const float CachedPointsSeperation = 1f;
	
	//Editor preview splines for add and insert:
	public GSDSplineF PreviewSpline;
	public GSDSplineI PreviewSplineInsert;
	
	//Nav data
	public float RoadWidth;
	public int Lanes;
	public List<int> id_connected;
	public int id=0;
	public string UID; //Unique ID
	public List<KeyValuePair<float,float>> BridgeParams;
	public List<KeyValuePair<float,float>> TunnelParams;
	public List<KeyValuePair<float,float>> HeightHistory;
	public int[] RoadDefKeysArray;
	public float[] RoadDefValuesArray;
	public double EditorOnly_LastNode_TimeSinceStartup = -1f;
	
	//Vars for intersections:
	const float MetersToCheck_NoTurnLane = 75f;
	const float MetersToCheck_NoTurnLaneSQ = 5625f;
	const float MetersToCheck_TurnLane = 125f;
	const float MetersToCheck_TurnLaneSQ = 15625f;
	const float MetersToCheck_BothTurnLane = 125f;
	const float MetersToCheck_BothTurnLaneSQ = 15625f;
	const bool bUseSQ = true;
	
	//Road connections and 3-way intersections:
	public bool bSpecialStartControlNode = false;
	public bool bSpecialEndControlNode = false;
	public bool bSpecialEndNode_IsStart_Delay = false;
	public bool bSpecialEndNode_IsEnd_Delay = false;
	public float SpecialEndNodeDelay_Start = 10f;
	public float SpecialEndNodeDelay_Start_Result = 10f;
	public float SpecialEndNodeDelay_End = 10f;
	public float SpecialEndNodeDelay_End_Result = 10f;
	public GSDSplineC SpecialEndNode_Start_OtherSpline = null;
	public GSDSplineC SpecialEndNode_End_OtherSpline = null;
	
	public Vector2 RoadV0 = default(Vector2);
	public Vector2 RoadV1 = default(Vector2);
	public Vector2 RoadV2 = default(Vector2);
	public Vector2 RoadV3 = default(Vector2);
	
	#region "Setup"
	public void Setup_Trigger(){
		#if UNITY_EDITOR
		if(!tRoad){ 
			if(mSplineRoot != null){
				tRoad = mSplineRoot.transform.parent.transform.gameObject.GetComponent<GSDRoad>(); 
			}
		}
		if(tRoad){ 
			tRoad.UpdateRoad(); 
		}	
		#endif
	}
	public void Setup(){
		#if UNITY_EDITOR
		//Don't setup if playing:
//		if(!Application.isEditor || (Application.isEditor && UnityEditor.EditorApplication.isPlaying)){ return; }
		
		//Setup unique ID:
		SetupUniqueIdentifier();
		
		//Set spline root:
		mSplineRoot = transform.gameObject;

		//Create spline nodes:
		GSDSplineN[] tNodesRaw = mSplineRoot.GetComponentsInChildren<GSDSplineN>();
		List<GSDSplineN> tList = new List<GSDSplineN>();
		int tNodesRawLength = tNodesRaw.Length;
		if(tNodesRawLength == 0){ return; }
		for(int i=0;i<tNodesRawLength;i++){
			if(tNodesRaw[i] != null){
				tNodesRaw[i].pos = tNodesRaw[i].transform.position;
				tList.Add(tNodesRaw[i]);
			}
		}
		tList.Sort(CompareListByName);
		//tList.Sort(delegate(GSDSplin i1, Item i2) { return i1.name.CompareTo(i2.name); });
		tNodesRaw = tList.ToArray();
		tList = null;
		Setup_Nodes(ref tNodesRaw);
		
		//Setup spline length, if more than 1 node:
		if(GetNodeCount() > 1){
//			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.BeginSample("SplineSetupLength"); }
			Setup_SplineLength();
//			if(tRoad.bProfiling){ UnityEngine.Profiling.Profiler.EndSample(); }
        }else if(GetNodeCount() == 1){
            mNodes[0].tTime = 0f;
        }
		
		#if UNITY_EDITOR
		//Setup preview spline:
		if(PreviewSpline == null){
			PreviewSpline = mSplineRoot.AddComponent<GSDSplineF>();	
			PreviewSpline.GSDSpline = this;
		}
		//Setup preview spline for insertion mode:
		if(PreviewSplineInsert == null){
			PreviewSplineInsert = mSplineRoot.AddComponent<GSDSplineI>();
			PreviewSplineInsert.GSDSpline = this;
		}
		#endif
		
		int mNodesCount = mNodes.Count;
		GSDSplineN tNode = null;
		Vector3[] pVects = new Vector3[mNodesCount+1];
		for(int i=0;i<mNodesCount;i++){
			tNode = mNodes[i];
			tNode.idOnSpline = i;
			tNode.bIsEndPoint = false;
			pVects[i] = tNode.pos;
		}
		pVects[pVects.Length-1] = new Vector3(0f,0f,0f);
		#if UNITY_EDITOR
		PreviewSpline.Setup(ref pVects);
		#endif
		
		RenameNodes();
		
		//Setup bridge params:
		if(BridgeParams != null){
			BridgeParams.Clear();
		}
		BridgeParams = new List<KeyValuePair<float, float>>();
		KeyValuePair<float,float> KVP;
		
		//Setup tunnel params:
		if(TunnelParams != null){
			TunnelParams.Clear();
		}
		TunnelParams = new List<KeyValuePair<float, float>>();
		
		if(mNodesCount > 1){
			if(bSpecialStartControlNode){
				mNodes[1].bIsEndPoint = true;
			}else{
				mNodes[0].bIsEndPoint = true;	
			}
			if(bSpecialEndControlNode){
				mNodes[mNodesCount-2].bIsEndPoint = true;	
			}else{
				mNodes[mNodesCount-1].bIsEndPoint = true;		
			}
        }else if(mNodesCount == 1){
            mNodes[0].bIsEndPoint = true;
            distance = 1;
        }
		
		float fStart = -1f;
        float fEnd = -1f;
        if(mNodesCount > 1){
            for(int i=0;i<mNodesCount;i++){
                tNode = mNodes[i];
				
				//Bridges:
				fStart = -1f;
				fEnd = -1f;
                if(tNode.bIsBridgeStart && !tNode.bIsTunnelStart){
                    fStart = tNode.tTime;
                    for (int j = i; j < mNodesCount; j++){
                        if (mNodes[j].bIsBridgeEnd){
                            fEnd = mNodes[j].tTime;
                            break;
                        }
                    }
					if(fEnd > 0f || GSDRootUtil.IsApproximately(fEnd,0f,0.0001f)){
	                    KVP = new KeyValuePair<float, float>(fStart, fEnd);
	                    BridgeParams.Add(KVP);
	                }
                }

				//Tunnels:
				fStart = -1f;
				fEnd = -1f;
				if(!tNode.bIsBridgeStart && tNode.bIsTunnelStart){
					fStart = tNode.tTime;
                    for (int j = i; j < mNodesCount; j++){
                        if (mNodes[j].bIsTunnelEnd){
                            fEnd = mNodes[j].tTime;
                            break;
                        }
                    }
					
					if(fEnd > 0f || GSDRootUtil.IsApproximately(fEnd,0f,0.0001f)){
	                    KVP = new KeyValuePair<float, float>(fStart, fEnd);
	                    TunnelParams.Add(KVP);
	                }
				}
				
                tNode.SetGradePercent(mNodesCount);
//                tNode.bIsEndPoint = false;
                tNode.tangent = GetSplineValue(mNodes[i].tTime, true);
                if(i<(mNodesCount - 1)){
                    tNode.NextTime = mNodes[i+1].tTime;
					tNode.NextTan = mNodes[i+1].tangent;
                }
            }
        }else if(mNodesCount == 1){
            mNodes[0].tangent = default(Vector3);
        }
		
		//Get bounds of road system:
		float[] tMaxEffects = new float[3];
		tMaxEffects[0] = tRoad.opt_MatchHeightsDistance;
		tMaxEffects[1] = tRoad.opt_ClearDetailsDistance;
		tMaxEffects[2] = tRoad.opt_ClearTreesDistance;
		float MaxEffectDistance = Mathf.Max(tMaxEffects)*2f; //Add min/max clear diff to bound checks
		int mCount1 = GetNodeCount();
		float[] tMinMaxX = new float[mCount1];
		float[] tMinMaxZ = new float[mCount1];
//		Vector3 tVect1 = default(Vector3);
		for(int i=0;i<mCount1;i++){
			tMinMaxX[i] = mNodes[i].pos.x;
			tMinMaxZ[i] = mNodes[i].pos.z;
		}
		float MinX = Mathf.Min(tMinMaxX) - MaxEffectDistance;	
		float MaxX = Mathf.Max(tMinMaxX) + MaxEffectDistance;
		float MinZ = Mathf.Min(tMinMaxZ) - MaxEffectDistance;
		float MaxZ = Mathf.Max(tMinMaxZ) + MaxEffectDistance;
		RoadV0 = new Vector3(MinX,MinZ);
		RoadV1 = new Vector3(MaxX,MinZ);
		RoadV2 = new Vector3(MaxX,MaxZ);
		RoadV3 = new Vector3(MinX,MaxZ);
		#endif
	}
	
	private void SetupUniqueIdentifier(){
		if(UID == null || UID.Length < 4){
			UID = System.Guid.NewGuid().ToString();
		}
	}
	
	private void RenameNodes(){
		int mNodesCount = mNodes.Count;
		for(int i=0;i<mNodesCount;i++){
			GSDSplineN tNode = mNodes[i];
			tNode.name = "Node" + tNode.idOnSpline;	
			tNode.transform.gameObject.name = tNode.name;
			tNode.EditorDisplayString = tRoad.transform.name + "-" + tNode.name;
		}
	}
	
	private int CompareListByName(GSDSplineN i1, GSDSplineN i2){
		return i1.idOnSpline.CompareTo(i2.idOnSpline);
	}

	private void Setup_Nodes(ref GSDSplineN[] tNodesRaw){
		//Process nodes:
		int i=0;
		List<GSDSplineN> tNodes = new List<GSDSplineN>();
		int tNodesRawLength = tNodesRaw.Length;
		for(i=0;i<tNodesRawLength;i++){
			if(!tNodesRaw[i].bDestroyed){
				tNodes.Add(tNodesRaw[i]);
			}
		}
		
		mNodes.Clear();
		mNodes = new List<GSDSplineN>();
		GSDSplineN tNode;
		float step;
		Quaternion rot;
		bool bClosed = false;
		step = (bClosed) ? 1f / ((float)tNodes.Count) : 1f / ((float)(tNodes.Count - 1));
		int tNodesCount = tNodes.Count;
		for(i=0;i<tNodesCount;i++){
			tNode = tNodes[i];
			
			rot = Quaternion.identity;
			if(i != tNodes.Count - 1){
				if((tNodes[i+1].transform.position - tNode.transform.position) == Vector3.zero){
					rot = Quaternion.identity;
				}else{
					rot = Quaternion.LookRotation(tNodes[i+1].transform.position - tNode.transform.position, transform.up);	
				}
			}else if (bClosed){
				rot = Quaternion.LookRotation(tNodes[0].transform.position - tNode.transform.position, transform.up);
			}else{
				rot = Quaternion.identity;
			}
	
			tNode.Setup(tNode.transform.position,rot,new Vector2 (0f, 1f),step*((float)i),tNode.transform.gameObject.name);
			tNode.SetupUniqueIdentifier();
			mNodes.Add(tNode);
		}

		tNodes = null;
		tNodesRaw = null;
	}
	
	private void Setup_SplineLength(){
        int mNodeCount = mNodes.Count;

		//First lets get the general distance, node to node:
		mNodes[0].tTime = 0f;
		mNodes[mNodeCount-1].tTime = 1f;
		Vector3 tVect1 = new Vector3(0f,0f,0f);
		Vector3 tVect2 = new Vector3(0f,0f,0f);
		float mDistance = 0f;
		float mDistance_NoMod = 0f;
		for(int j=0;j<mNodeCount;j++){
			tVect2 = mNodes[j].pos;
			if(j>0){
				mDistance += Vector3.Distance(tVect1,tVect2);
			}
			tVect1 = tVect2;	
		}
		mDistance_NoMod = mDistance;
		mDistance = mDistance * 1.05f;
		float step = 0.5f / mDistance;
		
		//Get a slightly more accurate portrayal of the time:
		float tTime = 0f;
		for(int j=0;j<(mNodeCount-1);j++){
			tVect2 = mNodes[j].pos;
			if(j > 0){
				tTime += (Vector3.Distance(tVect1,tVect2) / mDistance_NoMod);
				mNodes[j].tTime = tTime;
			}
			tVect1 = tVect2;
		}

		//Using general distance and calculated step, get an accurate distance:
		float tDistance = 0f;
		Vector3 prevPos = mNodes[0].pos;
		Vector3 cPos = new Vector3(0f,0f,0f);
//		float hDistance = 0f;
		GSDSplineN tNode;
		
		prevPos = GetSplineValue(0f);
		for (float i=0f;i<1f;i+=step) {
			cPos = GetSplineValue(i);
//			if(float.IsNaN(cPos.x)){
//				int xsagfdsdgsd = 0;	
//			}
			tDistance += Vector3.Distance(cPos,prevPos);
			prevPos = cPos;
		}
		
		distance = tDistance;
		
		//Now get fine distance between nodes:
		
//		float tNanCheck = 0f;
		float newTotalDistance = 0f;
		step = 0.5f / distance;
		GSDSplineN PrevNode = null;
		GSDSplineN ThisNode = null;
		prevPos = GetSplineValue(0f,false);
		for(int j=1;j<(mNodeCount-1);j++){
			PrevNode = mNodes[j-1];
			ThisNode = mNodes[j];
			
			if(j==1){
				prevPos = GetSplineValue(PrevNode.tTime);
			}
			tDistance = 0.00001f;
			for (float i=PrevNode.tTime;i<ThisNode.tTime;i+=step) {
				cPos = GetSplineValue(i);
				if(!float.IsNaN(cPos.x)){
					if(float.IsNaN(prevPos.x)){
						prevPos = cPos;	
					}
					tDistance+=Vector3.Distance(cPos,prevPos);
					prevPos = cPos;
				}
			}
			ThisNode.tempSegmentTime = (tDistance / distance);
			newTotalDistance+=tDistance;
			ThisNode.tDist = newTotalDistance;
		}
		
		
		mNodes[0].tDist = 0f;
		PrevNode = mNodes[mNodeCount-2];
		ThisNode = mNodes[mNodeCount-1];
		tDistance = 0.00001f;
		for (float i=PrevNode.tTime;i<ThisNode.tTime;i+=step) {
			cPos = GetSplineValue(i,false);
			if(!float.IsNaN(cPos.x)){
				if(float.IsNaN(prevPos.x)){
					prevPos = cPos;	
				}
				tDistance+=Vector3.Distance(cPos,prevPos);
				prevPos = cPos;
			}
		}
		ThisNode.tempSegmentTime = (tDistance / distance);
		newTotalDistance+=tDistance;
		ThisNode.tDist = newTotalDistance;
		distance = newTotalDistance;
		
		
		tTime = 0f;
		for(int j=1;j<(mNodeCount-1);j++){
			tNode = mNodes[j];
			tTime += tNode.tempSegmentTime;
			tNode.OldTime = tNode.tTime;
			tNode.tTime = tTime;
			tNode.tangent = GetSplineValue_SkipOpt(tNode.tTime,true);
			tNode.transform.rotation = Quaternion.LookRotation(tNode.tangent);
		}
		mNodes[0].tangent = GetSplineValue_SkipOpt(0f,true);
		mNodes[mNodeCount-1].tangent = GetSplineValue_SkipOpt(1f,true);
		

		mNodes[0].tDist = 0f;
		
		step = distance/CachedPointsSeperation;
		int ArrayCount = (int)Mathf.Floor(step) + 2;
		CachedPoints = null;
		CachedPoints = new Vector3[ArrayCount];
		step = CachedPointsSeperation/distance;
		for (int j=1;j<(ArrayCount-1);j++) {
			CachedPoints[j] = GetSplineValue(step*j);
		}
		CachedPoints[0] = mNodes[0].pos;
		CachedPoints[ArrayCount-1] = mNodes[mNodeCount-1].pos;
		
		RoadDefCalcs();
	}
	#endregion
	
	#region "Road definition cache and translation"
	private void RoadDefCalcs(){		
//		float tNanCheck = 0f;
		float tMod = Mathf.Lerp(0.05f,0.2f,distance/9000f);
		float step = tMod / distance;
		Vector3 cPos = GetSplineValue(0f);
		Vector3 prevPos = cPos;
		float tempDistanceModMax = tRoad.opt_RoadDefinition - step;
		float tempDistanceMod = 0f;
		float tempTotalDistance = 0f;
		float tempDistanceHolder = 0f;
		if(RoadDefKeysArray != null){ RoadDefKeysArray = null; }
		if(RoadDefValuesArray != null){ RoadDefValuesArray = null; }

		List<int> RoadDefKeys = new List<int>();
		List<float> RoadDefValues = new List<float>();
		
		RoadDefKeys.Add(0);
		RoadDefValues.Add(0f);
		
		for(float i=0f;i<1f;i+=step){
			cPos = GetSplineValue(i);
			tempDistanceHolder = Vector3.Distance(cPos,prevPos);
			tempTotalDistance += tempDistanceHolder;
			tempDistanceMod += tempDistanceHolder;
			if(tempDistanceMod > tempDistanceModMax){
				tempDistanceMod = 0f;
				RoadDefKeys.Add(TranslateParamToInt(i));
				RoadDefValues.Add(tempTotalDistance);
			}
			prevPos = cPos;
		}
		
		RoadDefKeysArray = RoadDefKeys.ToArray();
		RoadDefValuesArray = RoadDefValues.ToArray();
	}
	
	public int TranslateParamToInt(float f){
		return Mathf.Clamp((int)(f*10000000f),0,10000000);
	}
	public float TranslateInverseParamToFloat(int f){
		return Mathf.Clamp(((float)(float)f / 10000000f),0f,1f);
	}
	
	private void GetClosestRoadDefKeys(float tX, out int lo, out int hi, out int loIndex, out int hiIndex) {
		int x = TranslateParamToInt(tX);
		lo = -1;
		hi = RoadDefKeysArray.Length-1;
		
		int mid = -1;
		
	    while ((hi - lo) > 1) {
	        mid = Mathf.RoundToInt((lo + hi)/2);
	        if(RoadDefKeysArray[mid] <= x){
	            lo = mid;
	        }else{
	            hi = mid;
	        }
	    }
		
	    if(RoadDefKeysArray[lo] == x){ 
			hi = lo; 
		}
//		if(hi > RoadDefKeysArray.Length-1){ hi = RoadDefKeysArray.Length-1; }
		
		loIndex = lo;
		hiIndex = hi;
		lo = RoadDefKeysArray[lo];
		hi = RoadDefKeysArray[hi];
	}
	
	public int GetClosestRoadDefIndex(float tX, bool bRoundUp = false, bool bRoundDown = false) {
		int lo,hi,loIndex,hiIndex;
		
		GetClosestRoadDefKeys(tX, out lo, out hi, out loIndex,out hiIndex);
		
		int x = TranslateParamToInt(tX);
		
		if(bRoundUp){ return hiIndex; }
		if(bRoundDown){ return loIndex; }
		
		if((x - lo) > (hi - x)){
			return hiIndex;
		}else{
			return loIndex;
		}
	}
	
	private void GetClosestRoadDefValues(float tX, out float loF, out float hiF, out int loIndex, out int hiIndex) {
		int lo = -1;
		int hi = RoadDefValuesArray.Length-1;
		int mid = -1;
		
	    while ((hi - lo) > 1) {
	        mid = Mathf.RoundToInt((lo + hi)/2);
	        if(RoadDefValuesArray[mid] < tX || GSDRootUtil.IsApproximately(RoadDefValuesArray[mid],tX,0.02f)){
	            lo = mid;
	        }else{
	            hi = mid;
	        }
	    }

		if(GSDRootUtil.IsApproximately(RoadDefValuesArray[lo],tX,0.02f)){
			hi = lo; 
		}

		loIndex = lo;
		hiIndex = hi;
		loF = RoadDefValuesArray[lo];
		hiF = RoadDefValuesArray[hi];
	}
	
	public int GetClosestRoadDefValuesIndex(float tX, bool bRoundUp = false, bool bRoundDown = false) {
		float lo,hi;
		int loIndex,hiIndex;
		
		GetClosestRoadDefValues(tX, out lo, out hi, out loIndex,out hiIndex);
		
		if(bRoundUp){ return hiIndex; }
		if(bRoundDown){ return loIndex; }
		
		if((tX - lo) > (hi - tX)){
			return hiIndex;
		}else{
			return loIndex;
		}
	}
	
	public float TranslateDistBasedToParam(float mDist){
		int tIndex = GetClosestRoadDefValuesIndex(mDist,false,false);
		float tKey = TranslateInverseParamToFloat(RoadDefKeysArray[tIndex]);
		int tCount = RoadDefKeysArray.Length;
		float kDist = RoadDefValuesArray[tIndex];

		if(tIndex < (tCount-1)){
			if(mDist > kDist){
				float NextValue = RoadDefValuesArray[tIndex+1];
				float tDiff1 = (mDist - kDist) / (NextValue - kDist);
				tKey += (tDiff1 * (TranslateInverseParamToFloat(RoadDefKeysArray[tIndex+1])-tKey));
			}
		}
		if(tIndex > 0){
			if(mDist < kDist){
				float PrevValue = RoadDefValuesArray[tIndex-1];
				float tDiff1 = (mDist - PrevValue) / (kDist - PrevValue);
				tKey -= (tDiff1 * (tKey - TranslateInverseParamToFloat(RoadDefKeysArray[tIndex-1])));
			}
		}
		
		return tKey;
	}
	
	public float TranslateParamToDist(float param){
		int tIndex = GetClosestRoadDefIndex(param,false,false);
		float tKey = TranslateInverseParamToFloat(RoadDefKeysArray[tIndex]);
		int tCount = RoadDefKeysArray.Length;
		float kDist = RoadDefValuesArray[tIndex];
		float xDiff = kDist;
		
		if(tIndex < (tCount-1)){
			if(param > tKey){
				float NextValue = TranslateInverseParamToFloat(RoadDefKeysArray[tIndex+1]);
				float tDiff1 = (param - tKey) / (NextValue - tKey);
				xDiff += (tDiff1 * (RoadDefValuesArray[tIndex+1]-kDist));
			}
		}
		if(tIndex > 0){
			if(param < tKey){
				float PrevValue = TranslateInverseParamToFloat(RoadDefKeysArray[tIndex-1]);
				float tDiff1 = 1f-((param - PrevValue) / (tKey - PrevValue));
				xDiff -= (tDiff1 * (kDist - RoadDefValuesArray[tIndex-1]));
			}
		}
		
		return xDiff;
	}
	#endregion
	
	#region "Hermite math"
	/// <summary>
	/// Gets the spline value.
	/// </summary>
	/// <param name='f'>
	/// The relevant param (0-1) of the spline.
	/// </param>
	/// <param name='b'>
	/// True for is tangent, false (default) for vector3 position.
	/// </param>
	public Vector3 GetSplineValue(float f,bool b = false){
		int i;
		int idx = - 1;

        if (mNodes.Count == 0) { return default(Vector3); }
        if (mNodes.Count == 1) { return mNodes[0].pos; }

//		if(GSDRootUtil.IsApproximately(f,0f,0.00001f)){
//			if(b){
//				return mNodes[0].tangent;
//			}else{
//				return mNodes[0].pos;	
//			}
//		}else 
//		if(GSDRootUtil.IsApproximately(f,1f,0.00001f) || f > 1f){
//			if(b){
//				return mNodes[mNodes.Count-1].tangent;
//			}else{
//				return mNodes[mNodes.Count-1].pos;	
//			}
//		}else{
		//GSDRootUtil.IsApproximately(f,1f,0.00001f)
			for(i=0;i<mNodes.Count;i++){
				if(i == mNodes.Count-1){
					idx = i - 1;
					break;
				}
				if(mNodes[i].tTime > f){
					idx = i - 1;
					break;
				}
			}
			if(idx < 0){ idx = 0; }
//		}
		
		float param = (f - mNodes[idx].tTime) / (mNodes[idx + 1].tTime - mNodes[idx].tTime);
		param = GSDRootUtil.Ease(param, mNodes[idx].EaseIO.x, mNodes[idx].EaseIO.y);
		return GetHermiteInternal(idx, param, b);
	}
	
	public void GetSplineValue_Both(float f, out Vector3 tVect1, out Vector3 tVect2){
		int i;
		int idx = - 1;
		int mCount = GetNodeCount();
		
		if(f < 0f){ f = 0f; }
		if(f > 1f){ f = 1f; }

        if (mCount == 0) { 
            tVect1 = default(Vector3);
            tVect2 = default(Vector3);
            return;
        }
        if (mCount == 1) { 
			if(mNodes[0]){
            	tVect1 = mNodes[0].pos;
            	tVect2 = default(Vector3);
			}else{
				tVect1 = default(Vector3);
            	tVect2 = default(Vector3);
			}
			return;
        }
		
//		if(GSDRootUtil.IsApproximately(f,1f,0.0001f)){
//			tVect1 = mNodes[mNodes.Count-1].pos;	
//			tVect2 = mNodes[mNodes.Count-1].tangent;
//			return;
//		}
//		else if(GSDRootUtil.IsApproximately(f,0f,0.0001f)){
//			tVect1 = mNodes[0].pos;	
//			tVect2 = mNodes[0].tangent;
//			return;
//		}

		for(i=1;i<mCount;i++){
			if(i == mCount-1){
				idx = i - 1;
				break;
			}
			if(mNodes[i].tTime > f){
				idx = i - 1;
				break;
			}
		}
		if(idx < 0){ idx = 0; }

		float param = (f - mNodes[idx].tTime) / (mNodes[idx + 1].tTime - mNodes[idx].tTime);
		param = GSDRootUtil.Ease(param, mNodes[idx].EaseIO.x, mNodes[idx].EaseIO.y);
		
		tVect1 = GetHermiteInternal(idx, param, false);
		tVect2 = GetHermiteInternal(idx, param, true);
	}
	
	public Vector3 GetSplineValue_SkipOpt(float f,bool b = false){
		int i;
		int idx = - 1;

        if (mNodes.Count == 0) { return default(Vector3); }
        if (mNodes.Count == 1) { return mNodes[0].pos; }
		
		
//		if(GSDRootUtil.IsApproximately(f,0f,0.00001f)){
//			if(b){
//				return mNodes[0].tangent;
//			}else{
//				return mNodes[0].pos;	
//			}
//		}else 
//		if(GSDRootUtil.IsApproximately(f,1f,0.00001f) || f > 1f){
//			if(b){
//				return mNodes[mNodes.Count-1].tangent;
//			}else{
//				return mNodes[mNodes.Count-1].pos;	
//			}
//		}else{
			for(i=1;i<mNodes.Count;i++){
				if(i == mNodes.Count-1){
					idx = i - 1;
					break;
				}
				if(mNodes[i].tTime > f){
					idx = i - 1;
					break;
				}
			}
		if(idx < 0){ idx = 0; }
//			if(b && GSDRootUtil.IsApproximately(f,1f,0.00001f)){
//				idx = mNodes.Count-2;
//			}
//		}

		float param = (f - mNodes[idx].tTime) / (mNodes[idx + 1].tTime - mNodes[idx].tTime);
		param = GSDRootUtil.Ease(param, mNodes[idx].EaseIO.x, mNodes[idx].EaseIO.y);
		return GetHermiteInternal(idx, param, b);
	}
	
	public float GetClosestParam(Vector3 tVect, bool b20cmPrecision = false, bool b1MeterPrecision = false){
		return GetClosestParam_Do(ref tVect,b20cmPrecision,b1MeterPrecision);
	}
	private float GetClosestParam_Do(ref Vector3 tVect,bool b20cmPrecision,bool b1MeterPrecision){
		float Step1 = CachedPointsSeperation / distance; 	//5m to 1m	
		float Step2 = Step1*0.2f;		//20 cm		
		float Step3 = Step2*0.4f;		//8 cm 
		float Step4 = Step3*0.25f;		//2 cm
		float tMin=0f;
		float tMax=1f;
		float BestValue = -1f;
		float MaxStretch = 0.9f;
		Vector3 BestVect_p = new Vector3(0f,0f,0f);
		Vector3 BestVect_n = new Vector3(0f,0f,0f);

        if (mNodes.Count == 0) { return 0f; }
        if (mNodes.Count == 1) { return 1f; }
		 
		//Step 1: 1m 
		BestValue = GetClosestPoint_Helper(ref tVect,Step1,BestValue,tMin,tMax,ref BestVect_p,ref BestVect_n,true);
		if(b1MeterPrecision){ return BestValue; }
		//Step 2: 20cm 
		tMin = BestValue-(Step1*MaxStretch);
		tMax = BestValue+(Step1*MaxStretch);
		BestValue = GetClosestPoint_Helper(ref tVect,Step2,BestValue,tMin,tMax,ref BestVect_p,ref BestVect_n);
		if(b20cmPrecision){ return BestValue; }
		//Step 3: 8cm 
		tMin = BestValue-(Step2*MaxStretch);
		tMax = BestValue+(Step2*MaxStretch);
		BestValue = GetClosestPoint_Helper(ref tVect,Step3,BestValue,tMin,tMax,ref BestVect_p,ref BestVect_n);
		
		//Step 4: 2cm
		tMin = BestValue-(Step3*MaxStretch);
		tMax = BestValue+(Step3*MaxStretch);
		BestValue = GetClosestPoint_Helper(ref tVect,Step4,BestValue,tMin,tMax,ref BestVect_p,ref BestVect_n);
	
		return BestValue;
	}
	
	private float GetClosestPoint_Helper(ref Vector3 tVect, float tStep, float BestValue, float tMin, float tMax, ref Vector3 BestVect_p, ref Vector3 BestVect_n, bool bMeterCache = false){
		float mDistance = 5000f;
		float tDistance = 0f;
		Vector3 cVect = new Vector3(0f,0f,0f);
		Vector3 pVect = new Vector3(0f,0f,0f);
		bool bFirstLoopHappened = false;
		bool bSetBestValue = false;
		
		//Get lean for tmin/tmax:
		if(GetClosetPoint_MinMaxDirection(ref tVect,ref BestVect_p,ref BestVect_n)){
			tMax = BestValue;
		}else{
			tMin = BestValue;
		}
		
		tMin = Mathf.Clamp(tMin,0f,1f);
		tMax = Mathf.Clamp(tMax,0f,1f);
		
		if(bMeterCache){
			int CachedIndex = -1;
			int Step1 = 10;
			
			int CachedPointsLength = CachedPoints.Length;
			for(int j=0;j<CachedPointsLength;j+=Step1){
				cVect = CachedPoints[j];
				tDistance = Vector3.Distance(tVect,cVect);
				if(tDistance < mDistance){
					mDistance = tDistance;	
					CachedIndex = j;
				}
			}

			int jStart = (CachedIndex-Step1);
			if(jStart < 50){ jStart = 0; }
			int jEnd = (CachedIndex+Step1);
			if(jEnd > (CachedPointsLength)){ jEnd = CachedPointsLength; }
			for(int j=jStart;j<jEnd;j++){
				cVect = CachedPoints[j];
				if(bSetBestValue){ BestVect_n = cVect; bSetBestValue = false; }
				tDistance = Vector3.Distance(tVect,cVect);
				if(tDistance < mDistance){
					mDistance = tDistance;	
					if(!bFirstLoopHappened){
						BestVect_p = cVect;
					}else{
						BestVect_p = pVect;
					}
					CachedIndex = j;
					bSetBestValue = true;
					bFirstLoopHappened = true;
				}
				pVect = cVect;
			}
			
			BestValue = (CachedIndex / distance);
			
		}else{
			for(float i=tMin;i<=tMax;i+=tStep){
				cVect = GetSplineValue(i);
				if(bSetBestValue){ BestVect_n = cVect; bSetBestValue = false; }
				tDistance = Vector3.Distance(tVect,cVect);
				if(tDistance < mDistance){
					mDistance = tDistance;	
					BestValue = i;
					if(!bFirstLoopHappened){
						BestVect_p = cVect;
					}else{
						BestVect_p = pVect;
					}
					
					bSetBestValue = true;
					bFirstLoopHappened = true;
				}
				pVect = cVect;
			}
		}
		
		if(bSetBestValue){
			BestVect_n = cVect;
		}
		
		//Debug.Log ("returning: " + BestValue + " tmin:" + tMin + " tmax:" + tMax);
		return BestValue;
	}
	
	//Returns true for tmin lean:
	private bool GetClosetPoint_MinMaxDirection(ref Vector3 tVect, ref Vector3 BestVect_p, ref Vector3 BestVect_n){
		float Distance1 = Vector3.Distance(tVect,BestVect_p);
		float Distance2 = Vector3.Distance(tVect,BestVect_n);
		
		if(Distance1 < Distance2){
			//tMin lean
			return true;
		}else{
			//tMax lean
			return false;
		}
	}
	
	private Vector3 GetHermiteInternal(int i, double t, bool bTangent = false){
		double t2,t3;
		float BL0,BL1,BL2,BL3,tension;
		
		if(!bTangent){
			t2 = t * t;
			t3 = t2 * t;
		}else{
			t2 = t * t;
			t = t * 2.0;
			t2 = t2 * 3.0;
			t3 = 0; //Necessary for compiler error.
		}
		
		//Vectors:
		Vector3 P0 = mNodes[NGI(i, NI[0])].pos;
		Vector3 P1 = mNodes[NGI(i, NI[1])].pos;
		Vector3 P2 = mNodes[NGI(i, NI[2])].pos;
		Vector3 P3 = mNodes[NGI(i, NI[3])].pos;

		//Tension:
		tension = 0.5f;
		
		
		
		//Tangents:
		Vector3 xVect1 = (P1-P2) * tension;
		Vector3 xVect2 = (P3-P0) * tension;
		float tMaxMag = tRoad.opt_MagnitudeThreshold;
	
		if(Vector3.Distance(P1,P3) > tMaxMag){
			if(xVect1.magnitude > tMaxMag){ xVect1 = Vector3.ClampMagnitude(xVect1,tMaxMag); }
			if(xVect2.magnitude > tMaxMag){ xVect2 = Vector3.ClampMagnitude(xVect2,tMaxMag); }
		}else if(Vector3.Distance(P0,P2) > tMaxMag){
			if(xVect1.magnitude > tMaxMag){ xVect1 = Vector3.ClampMagnitude(xVect1,tMaxMag); }
			if(xVect2.magnitude > tMaxMag){ xVect2 = Vector3.ClampMagnitude(xVect2,tMaxMag); }
		}
		

		if(!bTangent){
			BL0 = (float) (CM[ 0] * t3 + CM[ 1] * t2 + CM[ 2] * t + CM[ 3]);
			BL1 = (float) (CM[ 4] * t3 + CM[ 5] * t2 + CM[ 6] * t + CM[ 7]);
			BL2 = (float) (CM[ 8] * t3 + CM[ 9] * t2 + CM[10] * t + CM[11]);
			BL3 = (float) (CM[12] * t3 + CM[13] * t2 + CM[14] * t + CM[15]);	
		}else{
			BL0 = (float) (CM[ 0] * t2 + CM[ 1] * t + CM[ 2]);
			BL1 = (float) (CM[ 4] * t2 + CM[ 5] * t + CM[ 6]);
			BL2 = (float) (CM[ 8] * t2 + CM[ 9] * t + CM[10]);
			BL3 = (float) (CM[12] * t2 + CM[13] * t + CM[14]);
		}
		
		Vector3 tVect = BL0 * P0 + BL1 * P1 + BL2 * xVect1 + BL3 * xVect2;

		if(!bTangent){ if(tVect.y < 0f){ tVect.y = 0f; } }
		
		return tVect;
	}

	private static readonly double[] CM = new double[] {
		 2.0, -3.0,  0.0,  1.0,
		-2.0,  3.0,  0.0,  0.0,
		 1.0, -2.0,  1.0,  0.0,
		 1.0, -1.0,  0.0,  0.0
	};
	private static readonly int[] NI = new int[] { 0, 1, -1, 2 };
	
	private int NGI(int i, int o){
		int NGITI = i + o;
//		if(bClosed){
//			return (NGITI % mNodes.Count + mNodes.Count) % mNodes.Count;
//		}else{
			return Mathf.Clamp(NGITI, 0, mNodes.Count-1);
//		}
	}
	#endregion
	
	#region "Gizmos"
//	private const bool bGizmoDraw = true;
	private float GizmoDrawMeters = 1f;
	void OnDrawGizmosSelected (){
//		if(!bGizmoDraw){ return; }
		if(mNodes == null || mNodes.Count < 2){ return; }
		if(transform == null){ return; }
		float DistanceFromCam = Vector3.SqrMagnitude(Camera.current.transform.position-mNodes[0].transform.position);
		
		if(DistanceFromCam > 16777216f){
			return;	 
		}else if(DistanceFromCam > 4194304f){
			GizmoDrawMeters = 16f;
		}else if(DistanceFromCam > 1048576f){
			GizmoDrawMeters = 8f;
		}else if(DistanceFromCam > 262144f){
			GizmoDrawMeters = 4f;
		}else if(DistanceFromCam > 65536){
			GizmoDrawMeters = 1f;
		}else if(DistanceFromCam > 16384f){
			GizmoDrawMeters = 0.5f;
		}else{
			GizmoDrawMeters = 0.1f;
		}

		Vector3 prevPos = mNodes[0].pos;
		Vector3 tempVect = new Vector3(0f,0f,0f);
		float step = GizmoDrawMeters/distance;
		step = Mathf.Clamp(step,0f,1f);
		Gizmos.color = new Color (1f, 0f, 0f, 1f);
		float i=0f;
		Vector3 cPos;
		float tCheck = 0f;
		Vector3 camPos = Camera.current.transform.position;
		for(i=0f;i<=1f;i+=step){
			tCheck+=step;
			cPos = GetSplineValue(i);
			
			if(tCheck > 0.1f){
				DistanceFromCam = Vector3.SqrMagnitude(camPos-cPos);
				if(DistanceFromCam > 16777216f){
					return;	 
				}else if(DistanceFromCam > 4194304f){
					GizmoDrawMeters = 16f;
				}else if(DistanceFromCam > 1048576f){
					GizmoDrawMeters = 10f;
				}else if(DistanceFromCam > 262144f){
					GizmoDrawMeters = 4f;
				}else if(DistanceFromCam > 65536){
					GizmoDrawMeters = 1f;
				}else if(DistanceFromCam > 16384f){
					GizmoDrawMeters = 0.5f;
				}else{
					GizmoDrawMeters = 0.1f;
				}
				step = GizmoDrawMeters/distance;
				step = Mathf.Clamp(step,0f,1f);
				tCheck = 0f;	
			}
			
			Gizmos.DrawLine (prevPos + tempVect, cPos + tempVect);
			prevPos = cPos;
			if((i+step)>1f){
				cPos = GetSplineValue(1f);
				Gizmos.DrawLine (prevPos + tempVect, cPos + tempVect);
			}
			
		}
	}
	#endregion

	#region "Intersections"
	public bool IsNearIntersection(ref Vector3 tPos, ref float nResult){
		int mCount = GetNodeCount();
		GSDSplineN tNode;
        float MetersToCheck = 75f * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f));
		float tDist;
		for(int i=0;i<mCount;i++){
			tNode = mNodes[i];
			if(tNode.bIsIntersection){
				tNode.GSDRI.Height = tNode.pos.y;
				
				if(bUseSQ){
					tDist = Vector3.SqrMagnitude(tPos-tNode.pos);
				}
//				else{
//					tDist = Vector3.Distance(tPos,tNode.pos);
//				}
				
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
					if(bUseSQ){
                        MetersToCheck = MetersToCheck_NoTurnLaneSQ * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f)); ;
					}
//					else{
//						MetersToCheck = MetersToCheck_NoTurnLane;
//					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					if(bUseSQ){
                        MetersToCheck = MetersToCheck_TurnLaneSQ * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f)); ;
					}
//					else{
//						MetersToCheck = MetersToCheck_TurnLane;
//					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					if(bUseSQ){
                        MetersToCheck = MetersToCheck_BothTurnLaneSQ * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f)); ;
					}
//					else{
//						MetersToCheck = MetersToCheck_BothTurnLane;
//					}
				}

				MetersToCheck *= 0.8f;
				if(tRoad.opt_Lanes == 4){
					MetersToCheck *= 1.25f;
				}else if(tRoad.opt_Lanes == 6){
					MetersToCheck *= 1.35f;
				}
				
				if(tDist <= MetersToCheck){
					nResult = tNode.pos.y;
					return true;
				}
			}
		}
		nResult = tPos.y;
		return false;
	}

	public float IntersectionStrength(ref Vector3 tPos, ref float nResult, ref GSDRoadIntersection tInter, ref bool bIsPast, ref float p, ref GSDSplineN fNode){
		int mCount = GetNodeCount();
		float tDist;
		GSDSplineN tNode;

        float MetersToCheck = 75f * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f));
		
		for(int i=0;i<mCount;i++){
			tNode = mNodes[i];
			if(tNode.bIsIntersection){
				tNode.GSDRI.Height = tNode.pos.y;
				GSDSplineN xNode;
				if(bUseSQ){
					tDist = Vector3.SqrMagnitude(tPos-tNode.pos);
				}
//				else{
//					tDist = Vector3.Distance(tPos,tNode.pos);
//				}
				
				if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.NoTurnLane){
					if(bUseSQ){
                        MetersToCheck = MetersToCheck_NoTurnLaneSQ * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f)); ;
					}
//					else{
//						MetersToCheck = MetersToCheck_NoTurnLane;
//					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane){
					if(bUseSQ){
                        MetersToCheck = MetersToCheck_TurnLaneSQ * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f)); ;
					}
//					else{
//						MetersToCheck = MetersToCheck_TurnLane;
//					}
				}else if(tNode.GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes){
					if(bUseSQ){
                        MetersToCheck = MetersToCheck_BothTurnLaneSQ * ((tRoad.opt_LaneWidth / 5f) * (tRoad.opt_LaneWidth / 5f)); ;
					}
//					else{
//						MetersToCheck = MetersToCheck_BothTurnLane;
//					}
				}
				if(tRoad.opt_Lanes == 4){
					MetersToCheck *= 1.25f;
				}else if(tRoad.opt_Lanes == 6){
					MetersToCheck *= 1.35f;
				}
				
				if(tDist <= MetersToCheck){
					if(tNode.GSDRI.bSameSpline){
						if(tNode.GSDRI.Node1.UID != tNode.UID){
							xNode = tNode.GSDRI.Node1;
						}else{
							xNode = tNode.GSDRI.Node2;
						}
						
						float P1 = tNode.tTime - p; if(P1 < 0f){ P1 *= -1f; }
						float P2 = xNode.tTime - p; if(P2 < 0f){ P2 *= -1f; }
						
						if(P1 > P2){
							if(p > xNode.tTime){
								bIsPast = true;	
							}else{
								bIsPast = false;	
							}
							fNode = xNode;
						}else{
							if(p > tNode.tTime){
								bIsPast = true;	
							}else{
								bIsPast = false;	
							}
							fNode = tNode;
						}
					}else{
						if(p > tNode.tTime){
							bIsPast = true;	
						}else{
							bIsPast = false;	
						}
						fNode = tNode;
					}
					
					
					if(bUseSQ){
						tDist = Mathf.Sqrt(tDist);
						MetersToCheck = Mathf.Sqrt(MetersToCheck);
					}
					
					tInter = tNode.GSDRI;
					nResult = tNode.pos.y + 0.1f;
					tDist = 1f-(tDist / MetersToCheck);
					tDist = Mathf.Pow(tDist,3f) * 5f;
					if(tDist > 1f) tDist = 1f;
					if(tDist < 0f) tDist = 0f;
					return tDist;	
				}
			}
		}
		nResult = tPos.y;
		return 0f;
	}
	
	public float IntersectionStrength_Next(Vector3 tPos){
		float nResult = 0f;
		GSDRoadIntersection tInter = null;
		bool bIsPast = false;
		float p = 0f;
		GSDSplineN fNode = null;
		return IntersectionStrength(ref tPos, ref nResult, ref tInter, ref bIsPast, ref p, ref fNode);
	}
	
	public bool IntersectionIsPast(ref float p, ref GSDSplineN oNode){
//		int mCount = GetNodeCount();
//		bool bIsPast;
//		GSDSplineN tNode = null;
//		for(int i=0;i<mCount;i++){
//			tNode = mNodes[i];
//			if(tNode.bIsIntersection){
//				float P1 = tNode.GSDRI.Node1.tTime - p; if(P1 < 0f){ P1 *= -1f; }
//				float P2 = tNode.GSDRI.Node2.tTime - p; if(P2 < 0f){ P2 *= -1f; }
//				
//				if(P1 > P2){
//					if(p > tNode.GSDRI.Node2.tTime){
//						bIsPast = true;	
//					}else{
//						bIsPast = false;	
//					}
//				}else{
//					if(p > tNode.GSDRI.Node1.tTime){
//						bIsPast = true;	
//					}else{
//						bIsPast = false;	
//					}
//				}
//				return bIsPast;
//			}
//		}
//		return false;
		
		
		if(p < oNode.tTime){
			return false;	
		}else{
			return true;
		}
	}
	
	void DestroyIntersection(GSDSplineN tNode){
		if(tNode != null){
			if(tNode.bIsEndPoint){
				if(tNode.idOnSpline == 1 && tNode.GSDSpline.mNodes[0].bSpecialEndNode_IsStart){
					Object.DestroyImmediate(tNode.GSDSpline.mNodes[0].transform.gameObject);
					tNode.GSDSpline.bSpecialStartControlNode = false;
				}else if(tNode.idOnSpline == tNode.GSDSpline.GetNodeCount()-2 && tNode.GSDSpline.mNodes[tNode.GSDSpline.GetNodeCount()-1].bSpecialEndNode_IsEnd){
					Object.DestroyImmediate(tNode.GSDSpline.mNodes[tNode.GSDSpline.GetNodeCount()-1].transform.gameObject);
					tNode.GSDSpline.bSpecialEndControlNode = false;
				}
				
			}
			tNode.bIsIntersection = false;
			tNode.bSpecialIntersection = false;
		}
		if(tNode.Intersection_OtherNode != null){
			if(tNode.Intersection_OtherNode.bIsEndPoint){
				if(tNode.Intersection_OtherNode.idOnSpline == 1 && tNode.Intersection_OtherNode.GSDSpline.mNodes[0].bSpecialEndNode_IsStart){
					Object.DestroyImmediate(tNode.Intersection_OtherNode.GSDSpline.mNodes[0].transform.gameObject);
					tNode.Intersection_OtherNode.GSDSpline.bSpecialStartControlNode = false;
				}else if(tNode.Intersection_OtherNode.idOnSpline == tNode.Intersection_OtherNode.GSDSpline.GetNodeCount()-2 && tNode.Intersection_OtherNode.GSDSpline.mNodes[tNode.Intersection_OtherNode.GSDSpline.GetNodeCount()-1].bSpecialEndNode_IsEnd){
					Object.DestroyImmediate(tNode.Intersection_OtherNode.GSDSpline.mNodes[tNode.Intersection_OtherNode.GSDSpline.GetNodeCount()-1].transform.gameObject);
					tNode.Intersection_OtherNode.GSDSpline.bSpecialEndControlNode = false;
				}
			}
			tNode.Intersection_OtherNode.bIsIntersection = false;
			tNode.Intersection_OtherNode.bSpecialIntersection = false;
		}
		
		if(tNode != null && tNode.Intersection_OtherNode != null){
			if(tNode.GSDSpline != tNode.Intersection_OtherNode.GSDSpline){
				if(tNode != null){ tNode.GSDSpline.tRoad.bUpdateSpline = true; }
				if(tNode.Intersection_OtherNode != null){ tNode.Intersection_OtherNode.GSDSpline.tRoad.bUpdateSpline = true; }
			}else{
				tNode.GSDSpline.tRoad.bUpdateSpline = true;
			}
		}else if(tNode != null){
			tNode.GSDSpline.tRoad.bUpdateSpline = true;
		}
	}
	#endregion
	
	#region "Bridges"
	public bool IsInBridge(float p){
		KeyValuePair<float,float> KVP;
		if(BridgeParams == null){ return false; }
		int cCount = BridgeParams.Count;
		if(cCount < 1){ return false; }
		for(int i=0;i<cCount;i++){
			KVP = BridgeParams[i];
			if(GSDRootUtil.IsApproximately(KVP.Key,p,0.0001f) || GSDRootUtil.IsApproximately(KVP.Value,p,0.0001f)){
				return true;
			}
			if(p > KVP.Key && p < KVP.Value){
				return true;
			}
		}
		return false;
	}
	
	public float BridgeUpComing(float p){
		float tDist = 20f / distance;
		float OrigP = p;
		p+=tDist;
		KeyValuePair<float,float> KVP;
		if(BridgeParams == null){ return 1f; }
		int cCount = BridgeParams.Count;
		if(cCount < 1){ return 1f; }
		for(int i=0;i<cCount;i++){
			KVP = BridgeParams[i];
			
			if(GSDRootUtil.IsApproximately(KVP.Key,p,0.0001f) || GSDRootUtil.IsApproximately(KVP.Value,p,0.0001f)){
				return ((KVP.Key - OrigP)/tDist);
			}
			if(p > KVP.Key && p < KVP.Value){
				return ((KVP.Key - OrigP)/tDist);
			}
		}
		return 1f;
	}
	
	public bool IsInBridgeTerrain(float p){
		KeyValuePair<float,float> KVP;
		if(BridgeParams == null){ return false; }
		int cCount = BridgeParams.Count;
		if(cCount < 1){ return false; }
		for(int i=0;i<cCount;i++){
			KVP = BridgeParams[i];
			if(GSDRootUtil.IsApproximately(KVP.Key+(10f/distance),p,0.0001f) || GSDRootUtil.IsApproximately(KVP.Value-(10f/distance),p,0.0001f)){
				return true;
			}
			if(p > (KVP.Key+(10f/distance)) && p < (KVP.Value-(10f/distance))){
				return true;
			}
		}
		return false;
	}
	
	public float GetBridgeEnd(float p){
		KeyValuePair<float,float> KVP;
		if(BridgeParams == null){ return -1f; }
		int cCount = BridgeParams.Count;
		if(cCount < 1){ return -1f; }
		for(int i=0;i<cCount;i++){
			KVP = BridgeParams[i];
			if(p >= KVP.Key && p <= KVP.Value){
				return KVP.Value;
			}
		}
		return -1f;
	}
	#endregion
	
	#region "Tunnels"
	public bool IsInTunnel(float p){
		KeyValuePair<float,float> KVP;
		if(TunnelParams == null){ return false; }
		int cCount = TunnelParams.Count;
		if(cCount < 1){ return false; }
		for(int i=0;i<cCount;i++){
			KVP = TunnelParams[i];
			if(GSDRootUtil.IsApproximately(KVP.Key,p,0.0001f) || GSDRootUtil.IsApproximately(KVP.Value,p,0.0001f)){
				return true;
			}
			if(p > KVP.Key && p < KVP.Value){
				return true;
			}
		}
		return false;
	}
	
	public float TunnelUpComing(float p){
		float tDist = 20f / distance;
		float OrigP = p;
		p+=tDist;
		KeyValuePair<float,float> KVP;
		if(TunnelParams == null){ return 1f; }
		int cCount = TunnelParams.Count;
		if(cCount < 1){ return 1f; }
		for(int i=0;i<cCount;i++){
			KVP = TunnelParams[i];
			
			if(GSDRootUtil.IsApproximately(KVP.Key,p,0.0001f) || GSDRootUtil.IsApproximately(KVP.Value,p,0.0001f)){
				return ((KVP.Key - OrigP)/tDist);
			}
			if(p > KVP.Key && p < KVP.Value){
				return ((KVP.Key - OrigP)/tDist);
			}
		}
		return 1f;
	}
	
	public bool IsInTunnelTerrain(float p){
		KeyValuePair<float,float> KVP;
		if(TunnelParams == null){ return false; }
		int cCount = TunnelParams.Count;
		if(cCount < 1){ return false; }
		for(int i=0;i<cCount;i++){
			KVP = TunnelParams[i];
			if(GSDRootUtil.IsApproximately(KVP.Key+(10f/distance),p,0.0001f) || GSDRootUtil.IsApproximately(KVP.Value-(10f/distance),p,0.0001f)){
				return true;
			}
			if(p > (KVP.Key+(10f/distance)) && p < (KVP.Value-(10f/distance))){
				return true;
			}
		}
		return false;
	}
	
	public float GetTunnelEnd(float p){
		KeyValuePair<float,float> KVP;
		if(TunnelParams == null){ return -1f; }
		int cCount = TunnelParams.Count;
		if(cCount < 1){ return -1f; }
		for(int i=0;i<cCount;i++){
			KVP = TunnelParams[i];
			if(p >= KVP.Key && p <= KVP.Value){
				return KVP.Value;
			}
		}
		return -1f;
	}
	#endregion
	
	#region "Road connections"
	public void ActivateEndNodeConnection(GSDSplineN tNode1, GSDSplineN tNode2){
		ActivateEndNodeConnection_Do(tNode1,tNode2);
	}
	private void ActivateEndNodeConnection_Do(GSDSplineN tNode1, GSDSplineN tNode2){
		GSDSplineC xSpline = tNode2.GSDSpline;
		int xCount = xSpline.GetNodeCount();
		int mCount = GetNodeCount();
		//Don't allow connection with less than 3 nodes:
		if(mCount < 3 || xCount < 3){ return; }

		Vector3 tNode1_ExtraPos = default(Vector3);
		Vector3 tNode2_ExtraPos = default(Vector3);

		bool bFirstNode_Start = false;
//		bool bFirstNode_End = false;
		if(tNode1.idOnSpline == 0){
			bFirstNode_Start = true;
			tNode2_ExtraPos = mNodes[1].transform.position;
		}else{
//			bFirstNode_End = true;
			tNode2_ExtraPos = mNodes[mCount-2].transform.position;
		}
		
		bool bSecondNode_Start = false;
//		bool bSecondNode_End = false;
		if(tNode2.idOnSpline == 0){
			bSecondNode_Start = true;
			tNode1_ExtraPos = xSpline.mNodes[1].transform.position;
		}else{
//			bSecondNode_End = true;
			tNode1_ExtraPos = xSpline.mNodes[xCount-2].transform.position;
		}
		
		GSDSplineN NodeCreated1 = null;
		GSDSplineN NodeCreated2 = null;
		
		if(bFirstNode_Start){
			bSpecialStartControlNode = true;
			if(mNodes[0].bSpecialEndNode){
				mNodes[0].transform.position = tNode1_ExtraPos;
				mNodes[0].pos = tNode1_ExtraPos;
			}else{
				GSD.Roads.GSDConstruction.InsertNode(tRoad,true,tNode1_ExtraPos,false,0,true);
			}
			NodeCreated1 = mNodes[0];
		}else{
			bSpecialEndControlNode = true;
            GSDSplineN zNode1 = xSpline.GetLastNode_All();
            if (zNode1 != null && zNode1.bSpecialEndNode) {
                zNode1.transform.position = tNode1_ExtraPos;
                zNode1.pos = tNode1_ExtraPos;
			}else{
				GSD.Roads.GSDConstruction.CreateNode(tRoad,true,tNode1_ExtraPos);	
			}
			NodeCreated1 = GetLastNode_All();
		}
		
		if(bSecondNode_Start){
			xSpline.bSpecialStartControlNode = true;
			if(xSpline.mNodes[0].bSpecialEndNode){
				xSpline.mNodes[0].transform.position = tNode2_ExtraPos;
				xSpline.mNodes[0].pos = tNode2_ExtraPos;
			}else{
				GSD.Roads.GSDConstruction.InsertNode(xSpline.tRoad,true,tNode2_ExtraPos,false,0,true);
			}
			NodeCreated2 = xSpline.mNodes[0];
		}else{
			xSpline.bSpecialEndControlNode = true;
            GSDSplineN zNode2 = xSpline.GetLastNode_All();
            if (zNode2 != null && zNode2.bSpecialEndNode) {
                zNode2.transform.position = tNode2_ExtraPos;
                zNode2.pos = tNode2_ExtraPos;
			}else{
				GSD.Roads.GSDConstruction.CreateNode(xSpline.tRoad,true,tNode2_ExtraPos);	
			}
            NodeCreated2 = xSpline.GetLastNode_All();
		}
		
		NodeCreated1.bSpecialEndNode_IsStart = bFirstNode_Start;
		NodeCreated2.bSpecialEndNode_IsStart = bSecondNode_Start;
		NodeCreated1.bSpecialEndNode_IsEnd = !bFirstNode_Start;
		NodeCreated2.bSpecialEndNode_IsEnd = !bSecondNode_Start;
		NodeCreated1.SpecialNodeCounterpart = NodeCreated2;
		NodeCreated2.SpecialNodeCounterpart = NodeCreated1;
		
		float lWidth1 = tNode1.GSDSpline.tRoad.opt_LaneWidth;
		float lWidth2 = tNode2.GSDSpline.tRoad.opt_LaneWidth;
		float xWidth = Mathf.Max(lWidth1,lWidth2);
		
		float tDelay = 0f;
		if(tNode1.GSDSpline.tRoad.opt_Lanes > tNode2.GSDSpline.tRoad.opt_Lanes){
			tNode2.bSpecialRoadConnPrimary = true;
			NodeCreated2.bSpecialRoadConnPrimary = true;
			if(tNode2.GSDSpline.tRoad.opt_Lanes == 4){ xWidth *= 2f; }
			tDelay = (tNode1.GSDSpline.tRoad.opt_Lanes - tNode2.GSDSpline.tRoad.opt_Lanes) * xWidth;
			if(tDelay < 10f){ tDelay = 10f; }
			if(bSecondNode_Start){
				tNode2.GSDSpline.bSpecialEndNode_IsStart_Delay = true;
				tNode2.GSDSpline.SpecialEndNodeDelay_Start = tDelay;
				tNode2.GSDSpline.SpecialEndNodeDelay_Start_Result = tNode1.GSDSpline.tRoad.RoadWidth();
				tNode2.GSDSpline.SpecialEndNode_Start_OtherSpline = tNode1.GSDSpline;
			}else{
				tNode2.GSDSpline.bSpecialEndNode_IsEnd_Delay = true;
				tNode2.GSDSpline.SpecialEndNodeDelay_End = tDelay;
				tNode2.GSDSpline.SpecialEndNodeDelay_End_Result = tNode1.GSDSpline.tRoad.RoadWidth();
				tNode2.GSDSpline.SpecialEndNode_End_OtherSpline = tNode1.GSDSpline;
			}
		}else if(tNode2.GSDSpline.tRoad.opt_Lanes > tNode1.GSDSpline.tRoad.opt_Lanes){
			tNode1.bSpecialRoadConnPrimary = true;
			NodeCreated1.bSpecialRoadConnPrimary = true;
			if(tNode1.GSDSpline.tRoad.opt_Lanes == 4){ xWidth *= 2f; }
			tDelay = (tNode2.GSDSpline.tRoad.opt_Lanes - tNode1.GSDSpline.tRoad.opt_Lanes) * xWidth;
			if(tDelay < 10f){ tDelay = 10f; }
			if(bFirstNode_Start){
				tNode1.GSDSpline.bSpecialEndNode_IsStart_Delay = true;
				tNode1.GSDSpline.SpecialEndNodeDelay_Start = tDelay;
				tNode1.GSDSpline.SpecialEndNodeDelay_Start_Result = tNode2.GSDSpline.tRoad.RoadWidth();
				tNode1.GSDSpline.SpecialEndNode_Start_OtherSpline = tNode2.GSDSpline;
			}else{
				tNode1.GSDSpline.bSpecialEndNode_IsEnd_Delay = true;
				tNode1.GSDSpline.SpecialEndNodeDelay_End = tDelay;
				tNode1.GSDSpline.SpecialEndNodeDelay_End_Result = tNode2.GSDSpline.tRoad.RoadWidth();
				tNode1.GSDSpline.SpecialEndNode_End_OtherSpline = tNode2.GSDSpline;
			}
		}else{
			tNode1.bSpecialRoadConnPrimary = true;
			NodeCreated1.bSpecialRoadConnPrimary = true;
			tDelay = 0f;
			tNode1.GSDSpline.bSpecialEndNode_IsEnd_Delay = false;
			tNode1.GSDSpline.bSpecialEndNode_IsStart_Delay = false;
			tNode1.GSDSpline.SpecialEndNodeDelay_End = tDelay;
			tNode1.GSDSpline.SpecialEndNodeDelay_End_Result = tNode2.GSDSpline.tRoad.RoadWidth();
			tNode1.GSDSpline.SpecialEndNode_End_OtherSpline = tNode2.GSDSpline;
			tNode2.GSDSpline.bSpecialEndNode_IsEnd_Delay = false;
			tNode2.GSDSpline.bSpecialEndNode_IsStart_Delay = false;
			tNode2.GSDSpline.SpecialEndNodeDelay_End = tDelay;
			tNode2.GSDSpline.SpecialEndNodeDelay_End_Result = tNode1.GSDSpline.tRoad.RoadWidth();
			tNode2.GSDSpline.SpecialEndNode_End_OtherSpline = tNode1.GSDSpline;
		}
		
		tNode1.SpecialNodeCounterpart = NodeCreated1;
		tNode2.SpecialNodeCounterpart = NodeCreated2;
		NodeCreated1.SpecialNodeCounterpart_Master = tNode1;
		NodeCreated2.SpecialNodeCounterpart_Master = tNode2;
		
		NodeCreated1.ToggleHideFlags(true);
		NodeCreated2.ToggleHideFlags(true);
		
//		tNode1.GSDSpline.Setup_Trigger();
//		if(tNode1.GSDSpline != tNode2.GSDSpline){
//			tNode2.GSDSpline.Setup_Trigger();
//		}
		
		if(tNode1 != null && tNode2 != null){
			if(tNode1.GSDSpline != tNode2.GSDSpline){
				tNode1.GSDSpline.tRoad.PiggyBacks = new GSDSplineC[1];
				tNode1.GSDSpline.tRoad.PiggyBacks[0] = tNode2.GSDSpline;
			}
			tNode1.GSDSpline.tRoad.EditorUpdateMe = true;
		}
	}
	#endregion

	#region "General Util"
	public int GetNodeCount(){
        return mNodes.Count;
	}
	
	public int GetNodeCountNonNull(){
		int mCount = GetNodeCount();
		int tCount = 0;
		for(int i=0;i<mCount;i++){
			if(mNodes[i] != null){
				tCount+=1;	
				if(mNodes[i].bIsIntersection && mNodes[i].GSDRI == null){
					DestroyIntersection(mNodes[i]);
				}
			}
		}
        return tCount;
	}
	
	public bool CheckInvalidNodeCount(){
		int mCount = GetNodeCount();
		int tCount = 0;
		for(int i=0;i<mCount;i++){
			if(mNodes[i] != null){
				tCount+=1;	
				if(mNodes[i].bIsIntersection && mNodes[i].GSDRI == null){
					DestroyIntersection(mNodes[i]);
				}
			}else{
					
			}
		}
		if(tCount != mCount){
			return true;	
		}else{
			return false;	
		}
	}
	
	public GSDSplineN GetCurrentNode(float p){
		int mCount = GetNodeCount();
		GSDSplineN tNode = null;
		for(int i=0;i<mCount;i++){
			tNode = mNodes[i];
			if(tNode.tTime > p){
				tNode = mNodes[i-1];
				return tNode;
			}
		}
		return tNode;
	}
	
	public GSDSplineN GetLastLegitimateNode(){
		int mCount = GetNodeCount();
		GSDSplineN tNode = null;
		for(int i=(mCount-1);i>=0;i--){
			tNode = mNodes[i];
			if(tNode.IsLegitimate()){
				return tNode;	
			}
		}
		return null;
	}

    public GSDSplineN GetLastNode_All() {
        int mCount = GetNodeCount();
        int StartIndex = (mCount - 1);
        GSDSplineN tNode = null;

        int i = StartIndex;
        while (i >= 0) {
            if (i <= (mNodes.Count - 1)) {
                tNode = mNodes[i];
                if (tNode != null) {
                    return tNode;
                }
            }
            i -= 1;
        }
        return null;
    }
	
	public GSDSplineN GetPrevLegitimateNode(int tIndex){
		try{
			GSDSplineN tNode = null;
			for(int i=(tIndex-1);i>=0;i--){
				tNode = mNodes[i];
				if(tNode.IsLegitimateGrade()){
					return tNode;
				}
			}
			return null;
		}catch{
			return null;		
		}
	}
	
	public GSDSplineN GetNextLegitimateNode(int tIndex){
		GSDSplineN tNode = null;
		int mCount = GetNodeCount();
		for(int i=(tIndex+1);i<mCount;i++){
			tNode = mNodes[i];
			if(tNode.IsLegitimateGrade()){
				return tNode;
			}
		}
		return null;
	}
	
	public void ClearAllRoadCuts(){
		int mCount = GetNodeCount();
		for(int i=0;i<mCount;i++){
			mNodes[i].ClearCuts();	
		}
	}
	
	public void ResetNavigationData(){
		id_connected = null;
		id_connected = new List<int>();
	}
	#endregion
	
	#endif
	
	#region "Start"
	void Start(){
		#if UNITY_EDITOR
			CachedPoints = null;
		#else
			this.enabled = false;
		#endif
	}
	#endregion
}