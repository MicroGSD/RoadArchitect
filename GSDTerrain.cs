using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif
[ExecuteInEditMode]
public class GSDTerrain : MonoBehaviour{
	#if UNITY_EDITOR
	[SerializeField][HideInInspector]
	private int mGSDID = -1;
	
    public int GSDID{
        get{return mGSDID;}
        set {
			//Do nothing.
		}
    }
	
	[HideInInspector]
	public Terrain tTerrain;
	
	//Splat map:
	public int SplatResoWidth = 1024;
	public int SplatResoHeight = 1024;
	public Color SplatBackground = new Color(0f,0f,0f,1f);
	public Color SplatForeground = new Color(1f,1f,1f,1f);
	public float SplatWidth = 30f;
	public bool SplatSkipBridges = false;
	public bool SplatSkipTunnels = false;
	public bool SplatSingleRoad = false;
	public int SplatSingleChoiceIndex = 0;
	public string RoadSingleChoiceUID = "";

	void OnEnable(){
		CheckID();
		if(!tTerrain){ tTerrain = transform.gameObject.GetComponent<Terrain>(); }
	}

	public void CheckID(){
		if(Application.isEditor){
			if(mGSDID < 0){
				mGSDID = GetNewID();
			}
			if(!tTerrain){ tTerrain = transform.gameObject.GetComponent<Terrain>(); }
		}
	}
	private int GetNewID(){
		Object[] tTerrainObjs = GameObject.FindObjectsOfType(typeof(GSDTerrain));
		List<int> AllIDS = new List<int>();
		foreach(GSDTerrain TID in tTerrainObjs){
			if(TID.GSDID > 0){
				AllIDS.Add (TID.GSDID);	
			}
		}
		
		bool bNotDone = true;
		int SpamChecker = 0;
		int SpamCheckerMax = AllIDS.Count + 64;
		int tRand;
		while(bNotDone){
			if(SpamChecker > SpamCheckerMax){
				Debug.LogError("Failed to generate GSDTerrainID");
				break;	
			}
			tRand = Random.Range(1,2000000000);
			if(!AllIDS.Contains(tRand)){
				bNotDone = false;
				return tRand;
			}
			SpamChecker+=1;
		}
		
		return -1;
	}
	#endif

	void Start (){
		#if UNITY_EDITOR
		this.enabled = true;
		CheckID();
		if(!tTerrain){ tTerrain = transform.gameObject.GetComponent<Terrain>(); }
		#else
		this.enabled = false;
		#endif
	}
}