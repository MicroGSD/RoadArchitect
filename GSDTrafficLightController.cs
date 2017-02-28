using UnityEngine;
using System.Collections;

[System.Serializable]
public class GSDTrafficLightController{
	public GameObject LightLeftObj;
	public GameObject LightRightObj;
	public GameObject[] LightsObj;
	
	public MeshRenderer MR_Left;
	public MeshRenderer MR_Right;
	public MeshRenderer[] MR_MainsStorage;
	public MeshRenderer MR_Main;
	
	public Light LightLeft_R;
	public Light LightLeft_Y;
	public Light LightLeft_G;
	
	public Light LightRight_R;
	public Light LightRight_Y;
	public Light LightRight_G;
	
	public Light[] Lights_R;
	public Light[] Lights_Y;
	public Light[] Lights_G;
	
	//Enums for controller:
	public enum iLightControllerEnum {Regular,LeftTurn,MasterLeft1,MasterLeft2,Red}
	//Enums for actual lights:
	public enum iLightStatusEnum {Regular,LeftTurn,MasterLeft,Red,RightTurn}
	public enum iLightSubStatusEnum {Green,Yellow,Red}
	public enum iLightYieldSubStatusEnum {Green,Yellow,Red,YellowTurn,GreenTurn}
	public iLightStatusEnum iLightStatus = iLightStatusEnum.Red;
	public iLightSubStatusEnum iLightSubStatus = iLightSubStatusEnum.Green;
	
	bool bLeft = false;
	bool bRight = false;
	bool bMain = false;
	bool bUseSharedMaterial = false;
	bool bLeftTurnYieldOnGreen = true;
	bool bLightsEnabled = true;
	
	#region "Constructor"
	public GSDTrafficLightController(ref GameObject _LightLeft, ref GameObject _LightRight, ref GameObject[] _Lights, ref MeshRenderer _MR_Left, ref MeshRenderer _MR_Right, ref MeshRenderer[] MR_Mains){
		LightLeftObj = _LightLeft;
		LightRightObj = _LightRight;
		LightsObj = _Lights;
		
		MR_Left = _MR_Left;
		MR_Right = _MR_Right;
		MR_MainsStorage = MR_Mains;
		MR_Main = MR_Mains[0];
		
		Light[] tLights;
		if(LightLeftObj != null){
			tLights = LightLeftObj.transform.GetComponentsInChildren<Light>();
			foreach(Light tLight in tLights){
				if(tLight.transform.name.ToLower().Contains("redlight")){
					LightLeft_R = tLight;
				}
				if(tLight.transform.name.ToLower().Contains("yellowlight")){
					LightLeft_Y = tLight;
				}
				if(tLight.transform.name.ToLower().Contains("greenl")){
					LightLeft_G = tLight;
				}
			}
		}
		if(LightRightObj != null){
			tLights = LightRightObj.transform.GetComponentsInChildren<Light>();
			foreach(Light tLight in tLights){
				if(tLight.transform.name.ToLower().Contains("redlight")){
					LightRight_R = tLight;
				}
				if(tLight.transform.name.ToLower().Contains("yellowlight")){
					LightRight_Y = tLight;
				}
				if(tLight.transform.name.ToLower().Contains("greenl")){
					LightRight_G = tLight;
				}
			}
		}
		
		int mCount = LightsObj.Length;
		Lights_R = new Light[mCount];
		Lights_Y = new Light[mCount];
		Lights_G = new Light[mCount];
		for(int i=0;i<mCount;i++){
			tLights = LightsObj[i].transform.GetComponentsInChildren<Light>();
			foreach(Light tLight in tLights){
				if(tLight.transform.name.ToLower().Contains("redlight")){
					Lights_R[i] = tLight;
				}
				if(tLight.transform.name.ToLower().Contains("yellowlight")){
					Lights_Y[i] = tLight;
				}
				if(tLight.transform.name.ToLower().Contains("greenl")){
					Lights_G[i] = tLight;
				}
			}
		}
	}
	#endregion

	#region "Update"
	public void UpdateLights(iLightStatusEnum tLightStatus, iLightSubStatusEnum tLightSubStatus, bool _bLightsEnabled){
		bLightsEnabled = _bLightsEnabled;
		iLightStatus = tLightStatus;
		iLightSubStatus = tLightSubStatus;
		bUseSharedMaterial = false;
		switch(iLightStatus){
		case iLightStatusEnum.Regular:
			TriggerRegular();
			break;
		case iLightStatusEnum.LeftTurn:
			TriggerLeftTurn();
			break;
		case iLightStatusEnum.MasterLeft:
			TriggerMasterLeft();
			break;
		case iLightStatusEnum.Red:
			TriggerRed();
			break;
		case iLightStatusEnum.RightTurn:
			TriggerRightTurn();
			break;
		}
	}
	#endregion
	
	#region "Triggers"
	private void TriggerRegular(){
		if(bMain){
			MRChange(ref MR_Main,iLightSubStatus);
			LightChange(0,iLightSubStatus);
		}
		if(bLeft){
			if(bLeftTurnYieldOnGreen){
				if(iLightSubStatus == iLightSubStatusEnum.Green){
					MRChangeLeftYield(ref MR_Left,iLightYieldSubStatusEnum.Green);
				}else if(iLightSubStatus == iLightSubStatusEnum.Yellow){
					MRChangeLeftYield(ref MR_Left,iLightYieldSubStatusEnum.Yellow);
				}
			}else{
				MRChange(ref MR_Left,iLightSubStatusEnum.Red);
				LightChange(1,iLightSubStatusEnum.Red);
			}
		}
		if(bRight){
			MRChange(ref MR_Right,iLightSubStatusEnum.Red);
			LightChange(2,iLightSubStatusEnum.Red);
		}
	}
	
	private void TriggerLeftTurn(){
		if(bMain){
			MRChange(ref MR_Main,iLightSubStatusEnum.Red);
			LightChange(0,iLightSubStatusEnum.Red);
		}
		if(bLeft){
			if(bLeftTurnYieldOnGreen){
				if(iLightSubStatus == iLightSubStatusEnum.Green){
					MRChangeLeftYield(ref MR_Left,iLightYieldSubStatusEnum.GreenTurn);
				}else if(iLightSubStatus == iLightSubStatusEnum.Yellow){
					MRChangeLeftYield(ref MR_Left,iLightYieldSubStatusEnum.YellowTurn);
				}
				LightChange(1,iLightSubStatus);
			}else{
				MRChange(ref MR_Left,iLightSubStatus);
				LightChange(1,iLightSubStatus);
			}
		}
		if(bRight){
			MRChange(ref MR_Right,iLightSubStatusEnum.Red);
			LightChange(2,iLightSubStatusEnum.Red);
		}	
	}
	
	private void TriggerMasterLeft(){
		if(bMain){
			MRChange(ref MR_Main,iLightSubStatus);
			LightChange(0,iLightSubStatus);
		}
		if(bLeft){
			if(iLightSubStatus == iLightSubStatusEnum.Green){
				MRChangeLeftYield(ref MR_Left,iLightYieldSubStatusEnum.GreenTurn);
			}else if(iLightSubStatus == iLightSubStatusEnum.Yellow){
				MRChangeLeftYield(ref MR_Left,iLightYieldSubStatusEnum.YellowTurn);
			}
			LightChange(1,iLightSubStatus);
		}
		if(bRight){
			MRChange(ref MR_Right,iLightSubStatus);
			LightChange(2,iLightSubStatus);
		}
	}
	
	private void TriggerRightTurn(){
		if(bMain){
			MRChange(ref MR_Main,iLightSubStatusEnum.Red);
			LightChange(0,iLightSubStatusEnum.Red);
		}
		if(bLeft){
			MRChange(ref MR_Left,iLightSubStatusEnum.Red);
			LightChange(1,iLightSubStatusEnum.Red);
		}
		if(bRight){
			MRChange(ref MR_Right,iLightSubStatus);
			LightChange(2,iLightSubStatus);
		}
	}
	
	private void TriggerRed(){
		if(bMain){
			MRChange(ref MR_Main,iLightSubStatusEnum.Red);
			LightChange(0,iLightSubStatusEnum.Red);
		}
		if(bLeft){
			MRChange(ref MR_Left,iLightSubStatusEnum.Red);
			LightChange(1,iLightSubStatusEnum.Red);
		}
		if(bRight){
			MRChange(ref MR_Right,iLightSubStatusEnum.Red);
			LightChange(2,iLightSubStatusEnum.Red);
		}
	}
	#endregion
	
	private void MRChange(ref MeshRenderer MR, iLightSubStatusEnum iLSSE){
		if(bUseSharedMaterial){
			if(iLSSE == iLightSubStatusEnum.Green){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0.667f,0f);
			}else if(iLSSE == iLightSubStatusEnum.Yellow){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0.334f,0f);
			}else if(iLSSE == iLightSubStatusEnum.Red){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0f,0f);
			}
		}else{
			if(iLSSE == iLightSubStatusEnum.Green){
				MR.material.mainTextureOffset = new Vector2(0.667f,0f);
			}else if(iLSSE == iLightSubStatusEnum.Yellow){
				MR.material.mainTextureOffset = new Vector2(0.334f,0f);
			}else if(iLSSE == iLightSubStatusEnum.Red){
				MR.material.mainTextureOffset = new Vector2(0f,0f);
			}
		}
	}
	
	private void MRChangeLeftYield(ref MeshRenderer MR, iLightYieldSubStatusEnum iLYSSE){
		if(bUseSharedMaterial){
			if(iLYSSE == iLightYieldSubStatusEnum.Green){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0.667f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.Yellow){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0.334f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.Red){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.YellowTurn){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0.6f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.GreenTurn){
				MR.sharedMaterial.mainTextureOffset = new Vector2(0.8f,0f);
			}
		}else{
			if(iLYSSE == iLightYieldSubStatusEnum.Green){
				MR.material.mainTextureOffset = new Vector2(0.4f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.Yellow){
				MR.material.mainTextureOffset = new Vector2(0.2f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.Red){
				MR.material.mainTextureOffset = new Vector2(0f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.YellowTurn){
				MR.material.mainTextureOffset = new Vector2(0.6f,0f);
			}else if(iLYSSE == iLightYieldSubStatusEnum.GreenTurn){
				MR.material.mainTextureOffset = new Vector2(0.8f,0f);
			}
		}
	}
	
	private void LightChange(int tIndex, iLightSubStatusEnum iLSSE){
		if(!bLightsEnabled){ 
			int mCount = MR_MainsStorage.Length;
			for(int i=0;i<mCount;i++){
				Lights_R[i].enabled = false;
				Lights_Y[i].enabled = false;
				Lights_G[i].enabled = false;
			}
			if(LightLeft_R != null){ LightLeft_R.enabled = false; }
			if(LightLeft_Y != null){ LightLeft_Y.enabled = false; }
			if(LightLeft_G != null){ LightLeft_G.enabled = false; }
			if(LightRight_R != null){ LightRight_R.enabled = false; }
			if(LightRight_Y != null){ LightRight_Y.enabled = false; }
			if(LightRight_G != null){ LightRight_G.enabled = false; }
			return; 
		}
		
		if(tIndex == 0){
			//Main:
			int mCount = MR_MainsStorage.Length;
			for(int i=0;i<mCount;i++){
				LightChangeHelper(ref Lights_R[i],ref Lights_Y[i],ref Lights_G[i],iLSSE);
			}
		}else if(tIndex == 1){
			//Left:
			LightChangeHelper(ref LightLeft_R,ref LightLeft_Y,ref LightLeft_G,iLSSE);
		}else if(tIndex == 2){
			//Right:
			LightChangeHelper(ref LightRight_R,ref LightRight_Y,ref LightRight_G,iLSSE);
		}
	}
	
	private void LightChangeHelper(ref Light tRed, ref Light tYellow, ref Light tGreen, iLightSubStatusEnum iLSSE){
		if(iLSSE == iLightSubStatusEnum.Green){
			tRed.enabled = false;
			tYellow.enabled = false;
			tGreen.enabled = true;
		}else if(iLSSE == iLightSubStatusEnum.Yellow){
			tRed.enabled = false;
			tYellow.enabled = true;
			tGreen.enabled = false;
		}else if(iLSSE == iLightSubStatusEnum.Red){
			tRed.enabled = true;
			tYellow.enabled = false;
			tGreen.enabled = false;
		}
	}
	
	#region "Setup"
	public void Setup(bool bLeftYield){
		SetupObject(MR_Left);
		SetupObject(MR_Right);
		SetupMainObjects();
		bLeft = (MR_Left != null);
		bRight = (MR_Right != null);
		bMain = (MR_Main != null);
		bLeftTurnYieldOnGreen = bLeftYield;
	}	
			
	private void SetupMainObjects(){
		if(MR_Main == null){ return; }
		int mCount = MR_MainsStorage.Length;
		if(mCount == 0){ return; }
		SetupObject(MR_Main);
		if(mCount > 1){
			for(int i=1;i<mCount;i++){
				if(bUseSharedMaterial){
					MR_MainsStorage[i].sharedMaterial = MR_Main.sharedMaterial;
				}else{
					MR_MainsStorage[i].materials = new Material[1];
					MR_MainsStorage[i].materials[0] = MR_Main.materials[0];
				}
			}
		}
	}
	
	private void SetupObject(MeshRenderer MR){
		if(MR != null){ MR.material = MR.material; }
	}
	#endregion
}

public class GSDTrafficLightSequence{
	public bool bLightMasterPath1 = true;
	public GSDTrafficLightController.iLightControllerEnum iLightController = GSDTrafficLightController.iLightControllerEnum.Regular;
	public GSDTrafficLightController.iLightSubStatusEnum iLightSubcontroller = GSDTrafficLightController.iLightSubStatusEnum.Green;
	public float tTime = 10f;
	
	public GSDTrafficLightSequence(bool bPath1, GSDTrafficLightController.iLightControllerEnum tLightController, GSDTrafficLightController.iLightSubStatusEnum tLightSubcontroller, float xTime){
		bLightMasterPath1 = bPath1;
		iLightController = tLightController;
		iLightSubcontroller = tLightSubcontroller;
		tTime = xTime;
	}
	
	public string ToStringGSD(){
		return "Path1: " + bLightMasterPath1 + " iLightController: " + iLightController.ToString() + " iLightSubcontroller: " + iLightSubcontroller.ToString() + " tTime: " + tTime.ToString("0F");
	}
}