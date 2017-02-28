#region "Imports"
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion
public class GSDRoadIntersection : MonoBehaviour{
	#if UNITY_EDITOR
	public GSDSplineN Node1;
	public GSDSplineN Node2;
	
	public string Node1UID;
	public string Node2UID;
	
	public bool bSameSpline = false;
	public bool bDrawGizmo = true;
	public bool bSelected = false;
	public string tName = "";
	
	//Markers:
	
	public bool bUseDefaultMaterials = true;
	public Material MarkerCenter1 = null;
	public Material MarkerCenter2 = null;
	public Material MarkerCenter3 = null;
	public Material MarkerExt_Stretch1 = null;
	public Material MarkerExt_Stretch2 = null;
	public Material MarkerExt_Stretch3 = null;
	public Material MarkerExt_Tiled1 = null;
	public Material MarkerExt_Tiled2 = null;
	public Material MarkerExt_Tiled3 = null;
	
	public Material Lane0Mat1 = null;
	public Material Lane0Mat2 = null;
	public Material Lane1Mat1 = null;
	public Material Lane1Mat2 = null;
	public Material Lane2Mat1 = null;
	public Material Lane2Mat2 = null;
	public Material Lane3Mat1 = null;
	public Material Lane3Mat2 = null;
	
	public Material Lane1Mat1_Disabled = null;
	public Material Lane1Mat2_Disabled = null;
	public Material Lane1Mat1_DisabledActive = null;
	public Material Lane1Mat2_DisabledActive = null;
	public Material Lane2Mat1_Disabled = null;
	public Material Lane2Mat2_Disabled = null;
	public Material Lane2Mat1_DisabledActive = null;
	public Material Lane2Mat2_DisabledActive = null;
	public Material Lane2Mat1_DisabledActiveR = null;
	public Material Lane2Mat2_DisabledActiveR = null;
	public Material Lane3Mat1_Disabled = null;
	public Material Lane3Mat2_Disabled = null;
	
	public int IntersectionWidth = 10;	//Width of the largest of road connected
	public int Lanes;
	public enum IntersectionTypeEnum {ThreeWay,FourWay};
	public IntersectionTypeEnum iType = IntersectionTypeEnum.FourWay;
	public bool bNode2B_LeftTurnLane = true;
	public bool bNode2B_RightTurnLane = true;
	public bool bNode2F_LeftTurnLane = true;
	public bool bNode2F_RightTurnLane = true;
	#endif
	
	public enum iStopTypeEnum {StopSign_AllWay,TrafficLight1,None,TrafficLight2};
	public iStopTypeEnum iStopType = iStopTypeEnum.StopSign_AllWay;
	public bool bLightsEnabled = true;
	public bool bFlipped = false;
	public bool bLeftTurnYieldOnGreen = true;
	public RoadTypeEnum rType = RoadTypeEnum.NoTurnLane;
	public enum RoadTypeEnum {NoTurnLane,TurnLane,BothTurnLanes};
	public enum LightTypeEnum {Timed,Sensors};
	public LightTypeEnum lType = LightTypeEnum.Timed;
	
	#if UNITY_EDITOR
	public bool bRegularPoleAlignment = true;
	public bool bTrafficPoleStreetLight = true;
	public bool bTrafficLightGray = false;
	public float StreetLight_Range = 30f;
	public float StreetLight_Intensity = 1f;
	public Color StreetLight_Color = new Color(1f,0.7451f,0.27451f,1f);

	public float GradeMod = 0.375f;
	public float GradeModNegative = 0.75f;

	public int IgnoreSide = -1;
	public int IgnoreCorner = -1;
	public bool bFirstSpecial_First = false;
	public bool bFirstSpecial_Last = false;
	public bool bSecondSpecial_First = false;
	public bool bSecondSpecial_Last = false;

	public float ScalingSense = 3f;
	
	public class CornerPositionMaker{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 DirectionFromCenter;
	}
	public CornerPositionMaker[] CornerPoints;
	
	protected string UID; //Unique ID
	public void SetupUniqueIdentifier(){
		if(UID == null || UID.Length < 4){
			UID = System.Guid.NewGuid().ToString();}
	}
	
	public Vector3 CornerRR,CornerRR_Outer,CornerRR_RampOuter;
	public Vector3 CornerRL,CornerRL_Outer,CornerRL_RampOuter;
	public Vector3 CornerLR,CornerLR_Outer,CornerLR_RampOuter;
	public Vector3 CornerLL,CornerLL_Outer,CornerLL_RampOuter;
	
	public Vector2 CornerRR_2D;
	public Vector2 CornerRL_2D;
	public Vector2 CornerLR_2D;
	public Vector2 CornerLL_2D;
	
	public Vector3[] fCornerLR_CornerRR;
	public Vector3[] fCornerLL_CornerRL;
	public Vector3[] fCornerLL_CornerLR;
	public Vector3[] fCornerRL_CornerRR;
	
	public float OddAngle,EvenAngle;
	
	
	public bool CornerRR1 = false;
	public bool CornerRR2 = false;
	public bool CornerRL1 = false;
	public bool CornerRL2 = false;
	public bool CornerLR1 = false;
	public bool CornerLR2 = false;
	public bool CornerLL1 = false;
	public bool CornerLL2 = false;
	
	public float MaxInterDistance = 0f;
	public float MaxInterDistanceSQ = 0f;
	public float Height = 50000f;
	public float SignHeight = -2000f;
	#endif

	//Traffic lights:
	public GSDTrafficLightController LightsRR;
	public GSDTrafficLightController LightsRL;
	public GSDTrafficLightController LightsLL;
	public GSDTrafficLightController LightsLR;
	public float opt_FixedTime_RegularLightLength = 10f;
	public float opt_FixedTime_LeftTurnLightLength = 5f;
	public float opt_FixedTime_AllRedLightLength = 1f;
	public float opt_FixedTime_YellowLightLength = 2f;
	public bool opt_AutoUpdateIntersections = true;
	public List<GSDTrafficLightSequence> FixedTimeSequenceList;
	
	#if UNITY_EDITOR
	#region "Setup"
	public void Setup(GSDSplineN tNode, GSDSplineN xNode){
		if(tNode.GSDSpline == xNode.GSDSpline){
			bSameSpline = true;	
		}
		
		if(bSameSpline){
			if(tNode.idOnSpline < xNode.idOnSpline){
				Node1 = tNode;
				Node2 = xNode;
			}else{
				Node1 = xNode;
				Node2 = tNode;
			}
		}else{
			Node1 = tNode;
			Node2 = xNode;
		}
		
		Node1.Intersection_OtherNode = Node2;
		Node2.Intersection_OtherNode = Node1;
		
		Node1.ToggleHideFlags(true);
		Node2.ToggleHideFlags(true);
		
		Node1UID = Node1.UID;
		Node2UID = Node2.UID;
		Node1.bIsIntersection = true;
		Node2.bIsIntersection = true;
		Node1.GSDRI = this;
		Node2.GSDRI = this;
	}
	
	public void DeleteRelevantChildren(GSDSplineN tNode, string tString){
		int cCount = transform.childCount;
		for(int i=cCount-1;i>=0;i--){
			if(transform.GetChild(i).name.ToLower().Contains(tString.ToLower())){
				Object.DestroyImmediate(transform.GetChild(i).gameObject);	
			}else if(tNode == Node1){
				if(transform.GetChild(i).name.ToLower().Contains("centermarkers")){
					Object.DestroyImmediate(transform.GetChild(i).gameObject);	
				}
			}
		}
	}
	#endregion
	
	#region "Utility"
	public void UpdateRoads(){
		#if UNITY_EDITOR
		if(!bSameSpline){
			GSDSplineC[] tPiggys = new GSDSplineC[1];
			tPiggys[0] = Node2.GSDSpline;
			Node1.GSDSpline.tRoad.PiggyBacks = tPiggys;
			Node1.GSDSpline.Setup_Trigger();	
		}else{
			Node1.GSDSpline.Setup_Trigger();
		}
		#endif
	}
	
	GSD.Roads.GSDRoadUtil.Construction2DRect BoundsRect;
	public void ConstructBoundsRect(){
		BoundsRect = null;
		BoundsRect = new GSD.Roads.GSDRoadUtil.Construction2DRect(new Vector2(CornerRR.x,CornerRR.z),new Vector2(CornerRL.x,CornerRL.z),new Vector2(CornerLR.x,CornerLR.z),new Vector2(CornerLL.x,CornerLL.z));
	}
	
	public bool Contains(ref Vector3 tVect){
		Vector2 vVect = new Vector2(tVect.x,tVect.z);
		if(BoundsRect == null){ ConstructBoundsRect(); }
		return BoundsRect.Contains(ref vVect);
	}
	
	private bool ContainsLineOld(Vector3 tVect1, Vector3 tVect2, int LineDef = 30){
		int MaxDef = LineDef;
		float MaxDefF = (float)MaxDef;
		
		Vector3[] tVects = new Vector3[MaxDef];
		
		tVects[0] = tVect1;
		float mMod = 0f;
		float fcounter = 1f;
		for(int i=1;i<(MaxDef-1);i++){
			mMod = fcounter/MaxDefF;
			tVects[i] = ((tVect2 - tVect1)*mMod) + tVect1;
			fcounter+=1f;
		}
		tVects[MaxDef-1] = tVect2;

		Vector2 xVect = default(Vector2);
		for(int i=0;i<MaxDef;i++){
			xVect = new Vector2(tVects[i].x,tVects[i].z);
			if(BoundsRect.Contains(ref xVect)){
				return true;	
			}
		}
		return false;
	}
	
	public bool ContainsLine(Vector3 tVect1, Vector3 tVect2){
		Vector2 tVectStart = new Vector2(tVect1.x,tVect1.z);
		Vector2 tVectEnd = new Vector2(tVect2.x,tVect2.z);
		bool bIntersects = Intersects2D(ref tVectStart,ref tVectEnd,ref CornerRR_2D,ref CornerRL_2D);
		if(bIntersects){ return true; }
		bIntersects = Intersects2D(ref tVectStart,ref tVectEnd,ref CornerRL_2D,ref CornerLL_2D);
		if(bIntersects){ return true; }
		bIntersects = Intersects2D(ref tVectStart,ref tVectEnd,ref CornerLL_2D,ref CornerLR_2D);
		if(bIntersects){ return true; }
		bIntersects = Intersects2D(ref tVectStart,ref tVectEnd,ref CornerLR_2D,ref CornerRR_2D);
		return bIntersects;
	}
	
	// Returns true if the lines intersect, otherwise false. If the lines
    // intersect, intersectionPoint holds the intersection point.
    private static bool Intersects2D(ref Vector2 Line1S, ref Vector2 Line1E, ref Vector2 Line2S, ref Vector2 Line2E){
        float firstLineSlopeX, firstLineSlopeY, secondLineSlopeX, secondLineSlopeY;

        firstLineSlopeX = Line1E.x - Line1S.x;
        firstLineSlopeY = Line1E.y - Line1S.y;

        secondLineSlopeX = Line2E.x - Line2S.x;
        secondLineSlopeY = Line2E.y - Line2S.y;

        float s, t;
        s = (-firstLineSlopeY * (Line1S.x - Line2S.x) + firstLineSlopeX * (Line1S.y - Line2S.y)) / (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);
        t = (secondLineSlopeX * (Line1S.y - Line2S.y) - secondLineSlopeY * (Line1S.x - Line2S.x)) / (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);

        if (s >= 0 && s <= 1 && t >= 0 && t <= 1){
            return true;
        }
        return false; // No collision
    }
	
	#endregion
	
	#region "Gizmos"
	void OnDrawGizmos(){
		if(!bDrawGizmo){ return; }
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(transform.position+new Vector3(0f,5f,0f), new Vector3(2f,11f,2f));
	}
	#endregion
	#endif
	
	#region "Traffic light controlling"
	void Start(){
		LightsRR.Setup(bLeftTurnYieldOnGreen);
		LightsRL.Setup(bLeftTurnYieldOnGreen);
		LightsLL.Setup(bLeftTurnYieldOnGreen);
		LightsLR.Setup(bLeftTurnYieldOnGreen);
		if(lType == LightTypeEnum.Timed){
			CreateFixedSequence();
			FixedTime_Increment();
		}else{
			//Do your custom stuff
			//In GSD Traffic addon, this will include sensor mode.
		}
	}
	
	private void CreateFixedSequence(){
		GSDTrafficLightSequence SMaker = null; FixedTimeSequenceList = new List<GSDTrafficLightSequence>();
		if(rType != RoadTypeEnum.NoTurnLane){SMaker = new GSDTrafficLightSequence(true,	GSDTrafficLightController.iLightControllerEnum.LeftTurn,	GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_LeftTurnLightLength);		FixedTimeSequenceList.Add (SMaker); }
		if(rType != RoadTypeEnum.NoTurnLane){SMaker = new GSDTrafficLightSequence(true,	GSDTrafficLightController.iLightControllerEnum.LeftTurn,	GSDTrafficLightController.iLightSubStatusEnum.Yellow,	opt_FixedTime_YellowLightLength);		FixedTimeSequenceList.Add (SMaker); }
		SMaker = new GSDTrafficLightSequence(true,	GSDTrafficLightController.iLightControllerEnum.Red,			GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_AllRedLightLength);		FixedTimeSequenceList.Add (SMaker);
		SMaker = new GSDTrafficLightSequence(true,	GSDTrafficLightController.iLightControllerEnum.Regular,		GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_RegularLightLength);		FixedTimeSequenceList.Add (SMaker);
		SMaker = new GSDTrafficLightSequence(true,	GSDTrafficLightController.iLightControllerEnum.Regular,		GSDTrafficLightController.iLightSubStatusEnum.Yellow,	opt_FixedTime_YellowLightLength);		FixedTimeSequenceList.Add (SMaker);
		SMaker = new GSDTrafficLightSequence(true,	GSDTrafficLightController.iLightControllerEnum.Red,			GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_AllRedLightLength);		FixedTimeSequenceList.Add (SMaker);
		
		if(rType != RoadTypeEnum.NoTurnLane){SMaker = new GSDTrafficLightSequence(false,	GSDTrafficLightController.iLightControllerEnum.LeftTurn,	GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_LeftTurnLightLength);		FixedTimeSequenceList.Add (SMaker); }
		if(rType != RoadTypeEnum.NoTurnLane){SMaker = new GSDTrafficLightSequence(false,	GSDTrafficLightController.iLightControllerEnum.LeftTurn,	GSDTrafficLightController.iLightSubStatusEnum.Yellow,	opt_FixedTime_YellowLightLength);		FixedTimeSequenceList.Add (SMaker); }
		SMaker = new GSDTrafficLightSequence(true,	GSDTrafficLightController.iLightControllerEnum.Red,			GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_AllRedLightLength);		FixedTimeSequenceList.Add (SMaker);
		SMaker = new GSDTrafficLightSequence(false,	GSDTrafficLightController.iLightControllerEnum.Regular,		GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_RegularLightLength);		FixedTimeSequenceList.Add (SMaker);
		SMaker = new GSDTrafficLightSequence(false,	GSDTrafficLightController.iLightControllerEnum.Regular,		GSDTrafficLightController.iLightSubStatusEnum.Yellow,	opt_FixedTime_YellowLightLength);		FixedTimeSequenceList.Add (SMaker);
		SMaker = new GSDTrafficLightSequence(false,	GSDTrafficLightController.iLightControllerEnum.Red,			GSDTrafficLightController.iLightSubStatusEnum.Green,	opt_FixedTime_AllRedLightLength);		FixedTimeSequenceList.Add (SMaker);
	}

	private IEnumerator TrafficLightFixedUpdate(float tTime){
		yield return new WaitForSeconds(tTime);
		FixedTime_Increment();
	}

	int FixedTimeIndex = 0;
	private void FixedTime_Increment(){
		GSDTrafficLightSequence SMaker = FixedTimeSequenceList[FixedTimeIndex];
		FixedTimeIndex+=1;
		if(FixedTimeIndex > (FixedTimeSequenceList.Count-1)){ FixedTimeIndex = 0; }
		
		GSDTrafficLightController Lights1 = null;
		GSDTrafficLightController Lights2 = null;
		
		GSDTrafficLightController Lights_outer1 = null;
		GSDTrafficLightController Lights_outer2 = null;
		
		if(SMaker.bLightMasterPath1){
			Lights1 = LightsRL;	
			Lights2 = LightsLR;
		
			if(bFlipped){
				Lights_outer1 = LightsRR;	
				Lights_outer2 = LightsLL;
			}else{
				Lights_outer1 = LightsRR;	
				Lights_outer2 = LightsLL;
			}
		}else{
			if(bFlipped){
				Lights1 = LightsRR;	
				Lights2 = LightsLL;
			}else{
				Lights1 = LightsRR;	
				Lights2 = LightsLL;
			}
			
			Lights_outer1 = LightsRL;	
			Lights_outer2 = LightsLR;
		}

		GSDTrafficLightController.iLightControllerEnum LCE = SMaker.iLightController;
		GSDTrafficLightController.iLightSubStatusEnum LCESub = SMaker.iLightSubcontroller;
		
		if(LCE == GSDTrafficLightController.iLightControllerEnum.Regular){
			Lights1.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Regular,LCESub,bLightsEnabled);
			Lights2.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Regular,LCESub,bLightsEnabled);
			Lights_outer1.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Red,LCESub,bLightsEnabled);
			Lights_outer2.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Red,LCESub,bLightsEnabled);
		}else if(LCE == GSDTrafficLightController.iLightControllerEnum.LeftTurn){
			Lights1.UpdateLights(GSDTrafficLightController.iLightStatusEnum.LeftTurn,LCESub,bLightsEnabled);
			Lights2.UpdateLights(GSDTrafficLightController.iLightStatusEnum.LeftTurn,LCESub,bLightsEnabled);
			Lights_outer1.UpdateLights(GSDTrafficLightController.iLightStatusEnum.RightTurn,LCESub,bLightsEnabled);
			Lights_outer2.UpdateLights(GSDTrafficLightController.iLightStatusEnum.RightTurn,LCESub,bLightsEnabled);
		}else if(LCE == GSDTrafficLightController.iLightControllerEnum.Red){
			Lights1.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Red,LCESub,bLightsEnabled);
			Lights2.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Red,LCESub,bLightsEnabled);
			Lights_outer1.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Red,LCESub,bLightsEnabled);
			Lights_outer2.UpdateLights(GSDTrafficLightController.iLightStatusEnum.Red,LCESub,bLightsEnabled);
		}
		
//		Debug.Log ("Starting: " + SMaker.ToString());
		StartCoroutine(TrafficLightFixedUpdate(SMaker.tTime));
	}
	#endregion
	
	#if UNITY_EDITOR
	#region "Materials"
	public void ResetMaterials_All(){
		ResetMaterials_Center(false);
		ResetMaterials_Ext_Stretched(false);
		ResetMaterials_Ext_Tiled(false);
		ResetMaterials_Lanes(false);
		UpdateMaterials();
	}
	
	public void ResetMaterials_Center(bool bUpdate = true){
		string tLanes = "-2L";
		Lanes = Node1.GSDSpline.tRoad.opt_Lanes;
		if(Lanes == 4){
			tLanes = "-4L";
		}else if(Lanes == 6){
			tLanes = "-6L";
		}
		if(iType == IntersectionTypeEnum.ThreeWay){
			tLanes += "-3";
			if(Node1.idOnSpline < 2 || Node2.idOnSpline < 2){
//			if(bFirstSpecial_First || bFirstSpecial_Last){	//Reverse if from node 0
				tLanes += "-crev";	//stands for "Center Reversed"	
//			}
			}
		}
		
		if(rType == RoadTypeEnum.BothTurnLanes){
		 	MarkerCenter1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterCenter-Both" + tLanes + ".mat");
			MarkerCenter2 = null;
			MarkerCenter3 = null;
		}else if(rType == RoadTypeEnum.TurnLane){
			MarkerCenter1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterCenter-Left" + tLanes + ".mat");
			MarkerCenter2 = null;
			MarkerCenter3 = null;
		}else if(rType == RoadTypeEnum.NoTurnLane){
			MarkerCenter1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterCenter-None" + tLanes + ".mat");
			MarkerCenter2 = null;
			MarkerCenter3 = null;
		}
		if(bUpdate){ UpdateMaterials(); }
	}
	public void ResetMaterials_Ext_Stretched(bool bUpdate = true){
		string tLanes = "-2L";
		Lanes = Node1.GSDSpline.tRoad.opt_Lanes;
		if(Lanes == 4){
			tLanes = "-4L";
		}else if(Lanes == 6){
			tLanes = "-6L";
		}
		
		if(rType == RoadTypeEnum.BothTurnLanes){
			MarkerExt_Stretch1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterStretch-Both" + tLanes + ".mat");
			MarkerExt_Stretch2 = null;
			MarkerExt_Stretch3 = null;
		}else if(rType == RoadTypeEnum.TurnLane){
			MarkerExt_Stretch1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterStretch-Left" + tLanes + ".mat");
			MarkerExt_Stretch2 = null;
			MarkerExt_Stretch3 = null;
		}else if(rType == RoadTypeEnum.NoTurnLane){
			MarkerExt_Stretch1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterStretch-None" + tLanes + ".mat");
			MarkerExt_Stretch2 = null;
			MarkerExt_Stretch3 = null;
		}
		if(bUpdate){ UpdateMaterials(); }
	}
	
	public void ResetMaterials_Ext_Tiled(bool bUpdate = true){
		if(rType == RoadTypeEnum.BothTurnLanes){
			MarkerExt_Tiled1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDRoad1.mat");
			MarkerExt_Tiled2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadDetailOverlay1.mat");
			MarkerExt_Tiled3 = null;
		}else if(rType == RoadTypeEnum.TurnLane){
			MarkerExt_Tiled1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDRoad1.mat");
			MarkerExt_Tiled2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadDetailOverlay1.mat");
			MarkerExt_Tiled3 = null;
		}else if(rType == RoadTypeEnum.NoTurnLane){
			MarkerExt_Tiled1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/GSDRoad1.mat");
			MarkerExt_Tiled2 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDRoadDetailOverlay1.mat");
			MarkerExt_Tiled3 = null;
		}
		if(bUpdate){ UpdateMaterials(); }
	}

	public void ResetMaterials_Lanes(bool bUpdate = true){
		string tLanes = "";
		Lanes = Node1.GSDSpline.tRoad.opt_Lanes;
		if(Lanes == 4){
			tLanes = "-4L";
		}else if(Lanes == 6){
			tLanes = "-6L";
		}
	
		if(iType == IntersectionTypeEnum.ThreeWay){
			Lane1Mat1_Disabled = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabled.mat");
			Lane1Mat2_Disabled = null;
			if(rType == RoadTypeEnum.BothTurnLanes){
				Lane1Mat1_DisabledActive = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuterRR.mat");
				Lane1Mat2_DisabledActive = null;
				Lane2Mat1_Disabled = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledR.mat");
				Lane2Mat2_Disabled = null;
			}else{
				Lane2Mat1_Disabled = null;
				Lane2Mat2_Disabled = null;
				Lane2Mat1_DisabledActive = null;
				Lane2Mat2_DisabledActive = null;
			}
			Lane2Mat1_DisabledActive = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuter" + tLanes + ".mat");
			Lane2Mat2_DisabledActive = null;
			if(rType == RoadTypeEnum.BothTurnLanes){
				Lane2Mat1_DisabledActiveR = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledOuterR.mat");
				Lane2Mat2_DisabledActiveR = null;
				Lane3Mat1_Disabled = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterLaneDisabledR.mat");
				Lane3Mat2_Disabled = null;
			}else{
				Lane2Mat1_DisabledActiveR = null;
				Lane2Mat2_DisabledActiveR = null;
				Lane3Mat1_Disabled = null;
				Lane3Mat2_Disabled = null;
			}
		}else{
			Lane1Mat1_Disabled = null;
			Lane1Mat2_Disabled = null;
			Lane2Mat1_Disabled = null;
			Lane2Mat2_Disabled = null;
			Lane2Mat1_DisabledActive = null;
			Lane2Mat2_DisabledActive = null;
			Lane2Mat1_DisabledActiveR = null;
			Lane2Mat2_DisabledActiveR = null;
			Lane3Mat1_Disabled = null;
			Lane3Mat2_Disabled = null;
		}
		
		if(rType == RoadTypeEnum.BothTurnLanes){
			Lane0Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterWhiteLYellowR" + tLanes + ".mat");
			Lane0Mat2 = null;
			Lane1Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterYellowLWhiteR.mat");
			Lane1Mat2 = null;
			Lane2Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR" + tLanes + ".mat");
			Lane2Mat2 = null;
			Lane3Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR.mat");
			Lane3Mat2 = null;
		}else if(rType == RoadTypeEnum.TurnLane){
			Lane0Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterWhiteLYellowR" + tLanes + ".mat");
			Lane0Mat2 = null;
			Lane1Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterYellowLWhiteR.mat");
			Lane1Mat2 = null;
			Lane2Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterWhiteR" + tLanes + ".mat");
			Lane2Mat2 = null;
			Lane3Mat1 = null;
			Lane3Mat2 = null;
		}else if(rType == RoadTypeEnum.NoTurnLane){
			Lane0Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterWhiteLYellowR" + tLanes + ".mat");
			Lane0Mat2 = null;
			Lane1Mat1 = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Markers/GSDInterYellowLWhiteR" + tLanes + ".mat");
			Lane1Mat2 = null;
			Lane2Mat1 = null;
			Lane2Mat2 = null;
			Lane3Mat1 = null;
			Lane3Mat2 = null;
		}
		
		if(bUpdate){ UpdateMaterials(); }
	}
	
	public void UpdateMaterials(){
		UpdateMaterials_Do();
	}
	private void UpdateMaterials_Do(){
		int cCount = transform.childCount;
		List<MeshRenderer> MR_Ext_Stretch = new List<MeshRenderer>();
		List<MeshRenderer> MR_Ext_Tiled = new List<MeshRenderer>();
		MeshRenderer MR_Center = null;
		List<MeshRenderer> MR_Lane0 = new List<MeshRenderer>();
		List<MeshRenderer> MR_Lane1 = new List<MeshRenderer>();
		List<MeshRenderer> MR_Lane2 = new List<MeshRenderer>();
		List<MeshRenderer> MR_Lane3 = new List<MeshRenderer>();
		List<MeshRenderer> MR_LaneD1 = new List<MeshRenderer>();
		List<MeshRenderer> MR_LaneD3 = new List<MeshRenderer>();
		List<MeshRenderer> MR_LaneDA2 = new List<MeshRenderer>();
		List<MeshRenderer> MR_LaneDAR2 = new List<MeshRenderer>();
		List<MeshRenderer> MR_LaneD2 = new List<MeshRenderer>();
		List<MeshRenderer> MR_LaneDA1 = new List<MeshRenderer>();
		
		string tTransName = "";
		for(int i=0;i<cCount;i++){
			tTransName = transform.GetChild(i).name.ToLower();
			if(tTransName.Contains("-stretchext")){
				MR_Ext_Stretch.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
			}
			if(tTransName.Contains("-tiledext")){
				MR_Ext_Tiled.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
			}
			if(tTransName.Contains("centermarkers")){
				MR_Center = transform.GetChild(i).GetComponent<MeshRenderer>(); continue;
			}
			if(tTransName.Contains("lane0")){
				MR_Lane0.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
			}
			if(tTransName.Contains("lane1")){
				MR_Lane1.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
			}
			if(tTransName.Contains("lane2")){
				MR_Lane2.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
			}
			if(tTransName.Contains("lane3")){
				MR_Lane3.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
			}
			if(iType == IntersectionTypeEnum.ThreeWay){
				if(tTransName.Contains("laned1")){
					MR_LaneD1.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
				}
				if(tTransName.Contains("laned3")){
					MR_LaneD3.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
				}
				if(tTransName.Contains("laneda2")){
					MR_LaneDA2.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
				}
				if(tTransName.Contains("lanedar2")){
					MR_LaneDAR2.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
				}
				if(tTransName.Contains("laned2")){
					MR_LaneD2.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
				}
				if(tTransName.Contains("laneda1")){
					MR_LaneDA1.Add(transform.GetChild(i).GetComponent<MeshRenderer>()); continue;
				}
			}
		}
		
		if(MR_Ext_Stretch != null && MR_Ext_Stretch.Count > 0){
			int MarkerExtStretchCounter = 0;
			if(MarkerExt_Stretch1 != null){
				MarkerExtStretchCounter+=1;
				if(MarkerExt_Stretch2 != null){
					MarkerExtStretchCounter+=1;
					if(MarkerExt_Stretch3 != null){
						MarkerExtStretchCounter+=1;
					}
				}
			}
			Material[] MarkerExtStretchMats = new Material[MarkerExtStretchCounter];
			for(int i=0;i<MarkerExtStretchCounter;i++){
				if(i==0){
					MarkerExtStretchMats[i] = MarkerExt_Stretch1;
				}else if(i == 1){
					MarkerExtStretchMats[i] = MarkerExt_Stretch2;
				}else if(i == 2){
					MarkerExtStretchMats[i] = MarkerExt_Stretch3;
				}
			}
			for(int i=0;i<MR_Ext_Stretch.Count;i++){
				MR_Ext_Stretch[i].materials = MarkerExtStretchMats;
			}
		}
		
		if(MR_Ext_Tiled != null && MR_Ext_Tiled.Count > 0){
			int MarkerExtTiledCounter = 0;
			if(MarkerExt_Tiled1 != null){
				MarkerExtTiledCounter+=1;
				if(MarkerExt_Tiled2 != null){
					MarkerExtTiledCounter+=1;
					if(MarkerExt_Tiled3 != null){
						MarkerExtTiledCounter+=1;
					}
				}
			}
			Material[] MarkerExtTiledMats = new Material[MarkerExtTiledCounter];
			for(int i=0;i<MarkerExtTiledCounter;i++){
				if(i==0){
					MarkerExtTiledMats[i] = MarkerExt_Tiled1;
				}else if(i == 1){
					MarkerExtTiledMats[i] = MarkerExt_Tiled2;
				}else if(i == 2){
					MarkerExtTiledMats[i] = MarkerExt_Tiled3;
				}
			}
			for(int i=0;i<MR_Ext_Tiled.Count;i++){
				MR_Ext_Tiled[i].materials = MarkerExtTiledMats;
			}
		}
		
		if(MR_Center != null){
			int CenterCounter = 0;
			if(MarkerCenter1 != null){
				CenterCounter+=1;
				if(MarkerCenter2 != null){
					CenterCounter+=1;
					if(MarkerCenter3 != null){
						CenterCounter+=1;
					}
				}
			}
			Material[] CenterMats = new Material[CenterCounter];
			for(int i=0;i<CenterCounter;i++){
				if(i==0){
					CenterMats[i] = MarkerCenter1;
				}else if(i == 1){
					CenterMats[i] = MarkerCenter2;
				}else if(i == 2){
					CenterMats[i] = MarkerCenter3;
				}
			}
			MR_Center.materials = CenterMats;
		}
		
		int LaneCounter = 0;
		if(MR_Lane0 != null && MR_Lane0.Count > 0){
			LaneCounter = 0;
			if(Lane0Mat1 != null){
				LaneCounter+=1;
				if(Lane0Mat2 != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane0Mats = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane0Mats[i] = Lane0Mat1;
				}else if(i == 1){
					Lane0Mats[i] = Lane0Mat2;
				}
			}
			for(int i=0;i<MR_Lane0.Count;i++){
				MR_Lane0[i].materials = Lane0Mats;
			}
		}
		
		if(MR_Lane1 != null && MR_Lane1.Count > 0){
			LaneCounter = 0;
			if(Lane1Mat1 != null){
				LaneCounter+=1;
				if(Lane1Mat2 != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane1Mats = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane1Mats[i] = Lane1Mat1;
				}else if(i == 1){
					Lane1Mats[i] = Lane1Mat2;
				}
			}
			for(int i=0;i<MR_Lane1.Count;i++){
				MR_Lane1[i].materials = Lane1Mats;
			}
		}
		
		if(MR_Lane2 != null && MR_Lane2.Count > 0){
			LaneCounter = 0;
			if(Lane2Mat1 != null){
				LaneCounter+=1;
				if(Lane2Mat2 != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane2Mats = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane2Mats[i] = Lane2Mat1;
				}else if(i == 1){
					Lane2Mats[i] = Lane2Mat2;
				}
			}
			for(int i=0;i<MR_Lane2.Count;i++){
				MR_Lane2[i].materials = Lane2Mats;
			}
		}
		
		if(MR_Lane3 != null && MR_Lane3.Count > 0){
			LaneCounter = 0;
			if(Lane3Mat1 != null){
				LaneCounter+=1;
				if(Lane3Mat2 != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane3Mats = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane3Mats[i] = Lane3Mat1;
				}else if(i == 1){
					Lane3Mats[i] = Lane3Mat2;
				}
			}
			for(int i=0;i<MR_Lane3.Count;i++){
				MR_Lane3[i].materials = Lane3Mats;
			}
		}
		
		if(MR_LaneD1 != null && MR_LaneD1.Count > 0){
			LaneCounter = 0;
			if(Lane1Mat1_Disabled != null){
				LaneCounter+=1;
				if(Lane1Mat2_Disabled != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane1Mats_Disabled = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane1Mats_Disabled[i] = Lane1Mat1_Disabled;
				}else if(i == 1){
					Lane1Mats_Disabled[i] = Lane1Mat2_Disabled;
				}
			}
			for(int i=0;i<MR_LaneD1.Count;i++){
				MR_LaneD1[i].materials = Lane1Mats_Disabled;
			}
		}
		
		if(MR_LaneD3 != null && MR_LaneD3.Count > 0){
			LaneCounter = 0;
			if(Lane3Mat1_Disabled != null){
				LaneCounter+=1;
				if(Lane3Mat2_Disabled != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane3Mats_Disabled = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane3Mats_Disabled[i] = Lane3Mat1_Disabled;
				}else if(i == 1){
					Lane3Mats_Disabled[i] = Lane3Mat2_Disabled;
				}
			}
			for(int i=0;i<MR_LaneD3.Count;i++){
				MR_LaneD3[i].materials = Lane3Mats_Disabled;
			}
		}
		
		if(MR_LaneDA2 != null && MR_LaneDA2.Count > 0){
			LaneCounter = 0;
			if(Lane2Mat1_DisabledActive != null){
				LaneCounter+=1;
				if(Lane2Mat2_DisabledActive != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane2Mats_DisabledActive = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane2Mats_DisabledActive[i] = Lane2Mat1_DisabledActive;
				}else if(i == 1){
					Lane2Mats_DisabledActive[i] = Lane2Mat2_DisabledActive;
				}
			}
			for(int i=0;i<MR_LaneDA2.Count;i++){
				MR_LaneDA2[i].materials = Lane2Mats_DisabledActive;
			}
		}
		
		if(MR_LaneDAR2 != null && MR_LaneDAR2.Count > 0){
			LaneCounter = 0;
			if(Lane2Mat1_DisabledActiveR != null){
				LaneCounter+=1;
				if(Lane2Mat2_DisabledActiveR != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane2Mats_DisabledActiveR = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane2Mats_DisabledActiveR[i] = Lane2Mat1_DisabledActiveR;
				}else if(i == 1){
					Lane2Mats_DisabledActiveR[i] = Lane2Mat2_DisabledActiveR;
				}
			}
			for(int i=0;i<MR_LaneDAR2.Count;i++){
				MR_LaneDAR2[i].materials = Lane2Mats_DisabledActiveR;
			}
		}
		
		if(MR_LaneD2 != null && MR_LaneD2.Count > 0){
			LaneCounter = 0;
			if(Lane2Mat1_Disabled != null){
				LaneCounter+=1;
				if(Lane2Mat2_Disabled != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane2Mats_Disabled = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane2Mats_Disabled[i] = Lane2Mat1_Disabled;
				}else if(i == 1){
					Lane2Mats_Disabled[i] = Lane2Mat2_Disabled;
				}
			}
			for(int i=0;i<MR_LaneD2.Count;i++){
				MR_LaneD2[i].materials = Lane2Mats_Disabled;
			}
		}
		
		
		if(MR_LaneDA1 != null && MR_LaneDA1.Count > 0){
			LaneCounter = 0;
			if(Lane1Mat1_DisabledActive != null){
				LaneCounter+=1;
				if(Lane1Mat2_DisabledActive != null){
					LaneCounter+=1;
				}
			}
			Material[] Lane1Mats_DisabledActive = new Material[LaneCounter];
			for(int i=0;i<LaneCounter;i++){
				if(i==0){
					Lane1Mats_DisabledActive[i] = Lane1Mat1_DisabledActive;
				}else if(i == 1){
					Lane1Mats_DisabledActive[i] = Lane1Mat2_DisabledActive;
				}
			}
			for(int i=0;i<MR_LaneDA1.Count;i++){
				MR_LaneDA1[i].materials = Lane1Mats_DisabledActive;
			}
		}
	}
	#endregion
	
	public void ToggleTrafficLightPoleColor(){
		Material TrafficLightMaterial = null;
		if(bTrafficLightGray){
			TrafficLightMaterial = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Signs/GSDInterTLB2.mat");	
		}else{
			TrafficLightMaterial = GSD.Roads.GSDRoadUtilityEditor.GiveMaterial("Assets/RoadArchitect/Materials/Signs/GSDInterTLB1.mat");	
		}
		int cCount = transform.childCount;
		string tName = "";
		MeshRenderer MR = null;
		Material[] tMats = new Material[1];
		tMats[0] = TrafficLightMaterial;
		for(int i=0;i<cCount;i++){
			tName = transform.GetChild(i).name.ToLower();
			if(tName.Contains("trafficlight")){
				MR = transform.GetChild(i).GetComponent<MeshRenderer>();
				MR.materials = tMats;
			}
		}
	}
	
	public void TogglePointLights(bool _bLightsEnabled){
		bLightsEnabled = _bLightsEnabled;
		int cCount = transform.childCount;
		Light[] fLights = null;
		Transform tTrans = null;
		for(int i=0;i<cCount;i++){
			if(transform.GetChild(i).name.ToLower().Contains("trafficlight")){
				tTrans = transform.GetChild(i);
				int kCount = tTrans.childCount;
				for(int k=0;k<kCount;k++){
					if(tTrans.GetChild(k).name.ToLower().Contains("streetlight")){
						fLights = tTrans.GetChild(k).GetComponentsInChildren<Light>();
						if(fLights != null){
							for(int j=0;j<fLights.Length;j++){
								fLights[j].enabled = bLightsEnabled;
								fLights[j].range = StreetLight_Range;
								fLights[j].intensity = StreetLight_Intensity;
								fLights[j].color = StreetLight_Color;
							}
						}
						fLights = null;
						break;
					}
				}
			}
		}
	}
	
	public void ResetStreetLightSettings(){
		StreetLight_Range = 30f;
		StreetLight_Intensity = 1f;
		StreetLight_Color = new Color(1f,0.7451f,0.27451f,1f);
		TogglePointLights(bLightsEnabled);
	}
	
	#endif
}