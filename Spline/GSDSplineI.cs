#region "Imports"
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using GSD;
#endif
#endregion
public class GSDSplineI : MonoBehaviour{
	#if UNITY_EDITOR
	public class GSDSplineIN{
		public Vector3 pos;
		public Quaternion rot;
		public Vector3 tangent;
		public Vector2 EaseIO;
		public float tTime = 0f;
		public float OldTime = 0f;
		
		public string name = "Node-1";
		
		public bool tempTime = false;
		
		public float tempSegmentTime = 0f;
		public float tempMinDistance = 5000f;
		public float tempMinTime = 0f;
		
		public int idOnSpline;
		public GSDSplineC GSDSpline;
		public bool bDestroyed = false;
		public bool bPreviewNode = false;
		
		public void Setup (Vector3 _p, Quaternion _q, Vector2 _io, float _tTime, string _name){
			pos = _p;
			rot = _q;
			EaseIO = _io;
			tTime = _tTime;
			name = _name;
		}
	}
	
	public int tCount = 0;
	public List<GSDSplineIN> mNodes = new List<GSDSplineIN> ();
	public bool bClosed = false;
	public float distance = -1f;
	public Vector3 MousePos = new Vector3(0f,0f,0f);
	public GSDSplineC GSDSpline;
	public GSDSplineIN ActionNode;
	
	public void DetermineInsertNodes(){
		int iCount = GSDSpline.mNodes.Count;
		if(iCount < 2){ return; }
		GSDSplineIN tNode;
		GSDSplineN xNode;
		mNodes.Clear();
		float tParam = GSDSpline.GetClosestParam(MousePos,false,true);
		bool bEndInsert = false;
		bool bZeroInsert = false;
		int iStart = 0;
		if(Mathf.Approximately(tParam,0f)){
			bZeroInsert = true;
			iStart = 0;
		}else if(Mathf.Approximately(tParam,1f)){
			bEndInsert = true;
		}
		
		for(int i=0;i<iCount;i++){
			xNode = GSDSpline.mNodes[i];
			tNode = new GSDSplineIN();
			tNode.pos = xNode.pos;
			tNode.idOnSpline = xNode.idOnSpline;
			tNode.tTime = xNode.tTime;
			if(!bZeroInsert && !bEndInsert){
				if(tParam > tNode.tTime){
					iStart = tNode.idOnSpline+1;
				}
			}
			mNodes.Add(tNode);
		}	

		mNodes.Sort(CompareListByName);
		int cCount = mNodes.Count;
		if(bEndInsert){
			iStart = cCount;
		}else{
			for(int i=iStart;i<cCount;i++){
				mNodes[i].idOnSpline+=1;
			}
		}
		tNode = new GSDSplineIN();
		tNode.pos = MousePos;
		tNode.idOnSpline = iStart;
		tNode.tTime = tParam;
		tNode.bPreviewNode = true;
		if(bEndInsert){
			mNodes.Add(tNode);
		}else{
			mNodes.Insert(iStart,tNode);
		}
		Setup_SplineLength();
		ActionNode = tNode;
	}
	
	private int CompareListByName(GSDSplineIN i1, GSDSplineIN i2){
		return i1.idOnSpline.CompareTo(i2.idOnSpline);
	}
	
	public void UpdateActionNode(){
		if(ActionNode != null){ 
			ActionNode.pos = MousePos;
		}
		DetermineInsertNodes();
	}	
	
	#region "Gizmos"
	public bool bGizmoDraw = false;
	private float GizmoDrawMeters = 1f;
	void OnDrawGizmos (){
		if(!bGizmoDraw){ return; }
		if(ActionNode == null){ return; }
		if(mNodes == null || mNodes.Count < 2){ return; }
		//Debug.Log ("lawl2");
		//mNodes[mNodes.Count-1].pos = MousePos;
		//Debug.Log ("lawl23");
		//Setup_SplineLength();
		
		float DistanceFromCam = Vector3.Distance(Camera.current.transform.position, mNodes[0].pos);
		
		if(DistanceFromCam > 2048){
			return;	 
		}else if(DistanceFromCam > 1024){
			GizmoDrawMeters = 32f;
		}else if(DistanceFromCam > 512){
			GizmoDrawMeters = 16f;
		}else if(DistanceFromCam > 256){
			GizmoDrawMeters = 8f;
		}else if(DistanceFromCam > 128){
			GizmoDrawMeters = 2f;
		}else if(DistanceFromCam > 64){
			GizmoDrawMeters = 0.5f;
		}else{
			GizmoDrawMeters = 0.1f;
		}
		GizmoDrawMeters = 0.1f;
		
		Vector3 prevPos = mNodes[0].pos;
		Vector3 tempVect = new Vector3(0f,0f,0f);
		//GizmoDrawMeters = 40f;
		float step = GizmoDrawMeters/distance;
		step = Mathf.Clamp(step,0f,1f);
		Gizmos.color = new Color (0f, 0f, 1f, 1f);
		float i=0f;
		Vector3 cPos;
		
		float startI = 0f;
		float endI = 1f;
		if(ActionNode.idOnSpline > 3){
			startI = mNodes[ActionNode.idOnSpline-2].tTime;	
		}
		if(ActionNode.idOnSpline < (mNodes.Count-3)){
			endI = mNodes[ActionNode.idOnSpline+2].tTime;
		}

		prevPos = GetSplineValue(startI);
		for(i=startI;i<=endI;i+=step){
			cPos = GetSplineValue(i);
			Gizmos.DrawLine (prevPos + tempVect, cPos + tempVect);
			prevPos = cPos;
			if((i+step)>1f){
				cPos = GetSplineValue(1f);
				Gizmos.DrawLine (prevPos + tempVect, cPos + tempVect);
			}
		}
	}
	#endregion

	#region "Setup"
	private void Setup_SplineLength(){
		//First lets get the general distance, node to node:
		mNodes[0].tTime = 0f;
		mNodes[mNodes.Count-1].tTime = 1f;
		Vector3 tVect1 = new Vector3(0f,0f,0f);
		Vector3 tVect2 = new Vector3(0f,0f,0f);
		float mDistance = 0f;
		float mDistance_NoMod = 0f;
		for(int j=0;j<mNodes.Count;j++){
			tVect2 = mNodes[j].pos;
			if(j>0){
				mDistance += Vector3.Distance(tVect1,tVect2);
			}
			tVect1 = tVect2;	
		}
		mDistance_NoMod = mDistance;
		mDistance = mDistance * 1.05f;
//		float step = 0.1f / mDistance;
		
		//Get a slightly more accurate portrayal of the time:
		float tTime = 0f;
		for(int j=0;j<(mNodes.Count-1);j++){
			tVect2 = mNodes[j].pos;
			if(j > 0){
				tTime += (Vector3.Distance(tVect1,tVect2) / mDistance_NoMod);
				mNodes[j].tTime = tTime;
			}
			tVect1 = tVect2;
		}
		distance = mDistance_NoMod;
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
			for(i=1;i<mNodes.Count;i++){
				if(i == mNodes.Count-1){
					idx = i - 1;
					break;
				}
				if(mNodes[i].tTime >= f){
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
				if(mNodes[i].tTime >= f){
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
		tension = 0.5f;	// 0.5 equivale a catmull-rom
		
		//Tangents:
		P2 = (P1-P2) * tension;
		P3 = (P3-P0) * tension;
		
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

		return BL0 * P0 + BL1 * P1 + BL2 * P2 + BL3 * P3;
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
		if(bClosed){
			return (NGITI % mNodes.Count + mNodes.Count) % mNodes.Count;
		}else{
			return Mathf.Clamp(NGITI, 0, mNodes.Count-1);
		}
	}
	#endregion
	
	public int GetNodeCount(){
		return mNodes.Count;
	}
	
	#endif

	#region "Start"
	void Start(){
		#if UNITY_EDITOR
			//Do nothing.
		#else
			this.enabled = false;
		#endif
	}
	#endregion
}