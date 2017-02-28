#region "Imports"
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using GSD.Roads.Splination;
using GSD.Roads.EdgeObjects;
using GSD;
using System.IO;
using GSD.Roads;
#endregion
public class GSDWizard : EditorWindow{
	public enum WindowTypeEnum{Extrusion,
		Edge, 
		BridgeComplete, 
		Groups
	};
	public enum WindowTypeEnumShort{Extrusion,
		Edge, 
		Groups
	};
	private static string[] WindowTypeDescBridge = new string[]{
		"Extrusion items",
		"Edge objects",
		"Complete bridges",
		"Other groups"
	};
	private static string[] WindowTypeDesc = new string[]{
		"Extrusion items", 
		"Edge objects", 
		"Other groups"
	};
	WindowTypeEnum tWindowType = WindowTypeEnum.Extrusion;
	WindowTypeEnum xWindowType = WindowTypeEnum.Extrusion;
	WindowTypeEnumShort StWindowType = WindowTypeEnumShort.Extrusion;
	WindowTypeEnumShort SxWindowType = WindowTypeEnumShort.Extrusion;
	static string xPath = "";
	
	GUIStyle ThumbStyle;
	Vector2 scrollPos = new Vector2(0f,25f);
	GSDSplineN tNode = null;
	List<GSDRoadUtil.WizardObject> oList = null;
	bool bNoGUI = false;
	
    void OnGUI() {
		DoGUI();
    }

	private void DoGUI(){
		if(bNoGUI){ return; }
		if(oList == null){ Close(); return; }
		
		GUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal();

		if(tNode.bIsBridgeStart){
			xWindowType = (WindowTypeEnum)EditorGUILayout.Popup("Category: ",(int)tWindowType,WindowTypeDescBridge,GUILayout.Width(312f));		
		}else{
			
			if(xWindowType == WindowTypeEnum.Edge){
				SxWindowType = WindowTypeEnumShort.Edge;
			}else if(xWindowType == WindowTypeEnum.Extrusion){
				SxWindowType = WindowTypeEnumShort.Extrusion;
			}else{
				SxWindowType = WindowTypeEnumShort.Groups;
			}

			SxWindowType = (WindowTypeEnumShort)EditorGUILayout.Popup("Category: ",(int)StWindowType,WindowTypeDesc,GUILayout.Width(312f));		
			
			if(SxWindowType == WindowTypeEnumShort.Extrusion){
				xWindowType = WindowTypeEnum.Extrusion;
			}else if(SxWindowType == WindowTypeEnumShort.Edge){
				xWindowType = WindowTypeEnum.Edge;
			}else{
				xWindowType = WindowTypeEnum.Groups;
			}
			StWindowType = SxWindowType;
		}
		
		if(xWindowType != tWindowType){
			Initialize(xWindowType,tNode);
			EditorGUILayout.EndHorizontal();
			return;
		}
		
		
		
		EditorGUILayout.LabelField("");
		EditorGUILayout.LabelField("Single-click items to load",EditorStyles.boldLabel,GUILayout.Width(200f));
		
		
		
		EditorGUILayout.EndHorizontal();
		if(oList.Count == 0){ return; }
		int oCount = oList.Count;
		
		int WidthSpacing = 160;
		int HeightSpacing = 200;
		int HeightOffset = 30;
		int ScrollHeightOffset = 25;
		
		int xCount = 0;
		int yCount = 0;
		int yMod = Mathf.FloorToInt((float)position.width / 142f) - 1;
		
		int yMax = 0;
		if(yMod == 0){ 
			yMax = 1;
		}else{
			yMax = Mathf.CeilToInt((float)oCount/(float)yMod);
		}
		
		bool bScrolling = false;
		if((((yMax) * HeightSpacing)+25) > position.height){
			scrollPos = GUI.BeginScrollView(new Rect(0, 25,position.width-10, position.height-30), scrollPos,new Rect (0, 0, (yMod*WidthSpacing)+25, (((yMax)*HeightSpacing)+50)));
			bScrolling= true;
			HeightOffset=ScrollHeightOffset;
		}
		
		EditorGUILayout.BeginHorizontal();
		
		bool bClicked = false;
		for(int i=0;i<oCount;i++){
			if(i > 0){
				if(yMod == 0){
					EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); yCount+=1; xCount=0;
				}else{
					if(i % yMod == 0){ EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); yCount+=1; xCount=0; }
				}
			}
			
			if(xCount == 0){
				bClicked = DoItem((xCount*WidthSpacing)+5,(yCount*HeightSpacing)+HeightOffset,i);
			}else{
				bClicked = DoItem(xCount*WidthSpacing,(yCount*HeightSpacing)+HeightOffset,i);
			}
			
			if(bClicked){
				if(tWindowType == WindowTypeEnum.Extrusion){
					GSD.Roads.Splination.SplinatedMeshMaker SMM = tNode.AddSplinatedObject();
					SMM.SetDefaultTimes(tNode.bIsEndPoint,tNode.tTime,tNode.NextTime,tNode.idOnSpline,tNode.GSDSpline.distance);
					SMM.LoadFromLibrary(oList[i].FileName,oList[i].bIsDefault);
					SMM.bIsGSD = oList[i].bIsDefault;
					SMM.Setup(true);
				}else if(tWindowType == WindowTypeEnum.Edge){
					GSD.Roads.EdgeObjects.EdgeObjectMaker EOM = tNode.AddEdgeObject();
					EOM.SetDefaultTimes(tNode.bIsEndPoint,tNode.tTime,tNode.NextTime,tNode.idOnSpline,tNode.GSDSpline.distance);
					EOM.LoadFromLibrary(oList[i].FileName,oList[i].bIsDefault);
					EOM.bIsGSD = oList[i].bIsDefault;
					EOM.Setup();
				}else if(tWindowType == WindowTypeEnum.Groups){
					tNode.LoadWizardObjectsFromLibrary(oList[i].FileName,oList[i].bIsDefault,oList[i].bIsBridge);
				}else if(tWindowType == WindowTypeEnum.BridgeComplete){
					tNode.LoadWizardObjectsFromLibrary(oList[i].FileName,oList[i].bIsDefault,oList[i].bIsBridge);
				}
				tNode.bQuitGUI = true;
				oList.Clear(); oList = null;
				EditorGUILayout.EndHorizontal();
				if(bScrolling){
					GUI.EndScrollView();
				}
				bNoGUI = true;
				Close();
				return;
			}
			xCount+=1;
			
		}
		EditorGUILayout.EndHorizontal();
		
		if(bScrolling){
			GUI.EndScrollView();
		}
	}
	
	bool DoItem(int x1, int y1, int i){
		if(oList[i].Thumb != null){
			if(GUI.Button(new Rect(x1,y1,132f,132f),oList[i].Thumb)){
				return true;	
			}
		}else{
			if(GUI.Button(new Rect(x1,y1,132f,132f),"No image")){
				return true;	
			}
		}

		GUI.Label(new Rect(x1,y1+132f,148f,20f),oList[i].DisplayName,EditorStyles.boldLabel);
		GUI.Label(new Rect(x1,y1+148f,148f,52f),oList[i].Desc,EditorStyles.miniLabel);
		
		return false;
	}
	
	#region "Init"
	public Rect xRect;
	public void Initialize(WindowTypeEnum _tWindowType, GSDSplineN _tNode) {
		if(xRect.width < 1f && xRect.height < 1f){
			xRect.x = 275f;
			xRect.y = 200f;
			xRect.width = 860f;
			xRect.height = 500f;
		}
		
		position = xRect;
		tWindowType = _tWindowType;
		tNode = _tNode;
		InitWindow();
		Show();
    }
	
	private void InitWindow(){
		if(oList != null){ oList.Clear(); oList = null; }
		oList = new List<GSDRoadUtil.WizardObject>();
		if(tWindowType == WindowTypeEnum.Extrusion){
            titleContent.text = "Extrusion";
			InitObjs();
		}else if(tWindowType == WindowTypeEnum.Edge){
            titleContent.text = "Edge objects";
			InitObjs();
		}else if(tWindowType ==  WindowTypeEnum.BridgeComplete){
            titleContent.text = "Bridges";
			InitGroups(true);
		}else if(tWindowType ==  WindowTypeEnum.Groups){
            titleContent.text = "Groups";
			InitGroups(false);
		}
		
    	ThumbStyle = new GUIStyle(GUI.skin.button);
    	ThumbStyle.contentOffset = new Vector2(0f,0f);
		ThumbStyle.border = new RectOffset(0,0,0,0);
		ThumbStyle.fixedHeight = 128f;
		ThumbStyle.fixedWidth = 128f;
		ThumbStyle.padding = new RectOffset(0,0,0,0);
		ThumbStyle.normal.background = null;
		ThumbStyle.hover.background = null;
		ThumbStyle.active.background = null;

		EditorStyles.label.wordWrap = true;
		EditorStyles.miniLabel.wordWrap = true;
		GUI.skin.label.wordWrap = true;
	}
	
	#region "Init complete bridges"
	private void InitGroups(bool bIsBridge){
		string[] tNames = null;
		string[] tPaths = null;
		//Load user custom ones first:
		GetGroupListing(out tNames, out tPaths, tNode.GSDSpline.tRoad.opt_Lanes, false);
		LoadGroupObjs(ref tNames, ref tPaths, bIsBridge);
		//Load GSD ones last:
		GetGroupListing(out tNames, out tPaths, tNode.GSDSpline.tRoad.opt_Lanes, true);
		LoadGroupObjs(ref tNames, ref tPaths, bIsBridge);
	}
	
	private void LoadGroupObjs(ref string[] tNames, ref string[] tPaths, bool bIsBridge){
		int tCount = tNames.Length;
		string tPath = "";
//		string tStringPath = "";
//		string tDesc = "";
//		string tDisplayName = "";
//		string ThumbString = "";
		for(int i=0;i<tCount;i++){
			GSDRoadUtil.WizardObject tO = GSDRoadUtil.WizardObject.LoadFromLibrary(tPaths[i]);
			if(tO == null){ continue; }
			if(tO.bIsBridge != bIsBridge){ continue; }
			try{
				tO.Thumb = (Texture2D)AssetDatabase.LoadAssetAtPath(tO.ThumbString,typeof(Texture2D)) as Texture2D;	
			}catch{
				tO.Thumb = null;
			}
			tO.FileName = tNames[i];
			tO.FullPath = tPath;
			
			if(tO.bIsDefault && bIsBridge){
				if(tO.DisplayName.Contains("SuspL") || tO.DisplayName.Contains("Large Suspension")){
					tO.DisplayName = "Large Suspension";
					tO.Desc = "Designed after the Golden Gate bridge. For lengths over 850m. Best results over 1300m.";
					tO.sortID = 11;
				}else if(tO.DisplayName.Contains("SuspS") || tO.DisplayName.Contains("Small Suspension")){
					tO.DisplayName = "Small Suspension";
					tO.Desc = "Similar style as the large with smaller pillars. For lengths under 725m.";
					tO.sortID = 10;
				}else if(tO.DisplayName.Contains("SemiArch1")){
					tO.DisplayName = "SemiArch 80 Degree";
					tO.Desc = "80 Degree arch. For lengths under 300m and small heights.";
					tO.sortID = 40;
				}else if(tO.DisplayName.Contains("SemiArch2")){ 
					tO.DisplayName = "SemiArch 80 Girder";
					tO.Desc = "80 Degree arch with girder style. For lengths under 300m and small heights.";
					tO.sortID = 41;
				}else if(tO.DisplayName.Contains("SemiArch3")){
					tO.DisplayName = "SemiArch 180 Degree";
					tO.Desc = "180 Degree arch. For lengths under 300m and small heights.";
					tO.sortID = 42;
				}else if(tO.DisplayName.Contains("Arch12m")){
					tO.DisplayName = "Arch 12m Beams";
					tO.Desc = "Full deck arch bridge with 12m beams. Good for any length.";
					tO.sortID = 0;
				}else if(tO.DisplayName.Contains("Arch24m")){
					tO.DisplayName = "Arch 24m Beams";
					tO.Desc = "Full deck arch bridge with 24m beams. Good for any length and non-small width roads.";
					tO.sortID = 1;
				}else if(tO.DisplayName.Contains("Arch48m")){
					tO.DisplayName = "Arch 48m Beams";
					tO.Desc = "Full deck arch bridge with 48m beams. Good for any length and large width roads.";
					tO.sortID = 3;
				}else if(tO.DisplayName.Contains("Grid")){
					tO.DisplayName = "Grid Steel";
					tO.Desc = "Grid based steel bridge. Good for any length depending on triangle counts.";
					tO.sortID = 30;
				}else if(tO.DisplayName.Contains("Steel")){
					tO.DisplayName = "Steel Beam";
					tO.Desc = "Standard steel beam bridge. Good for any length depending on triangle counts.";
					tO.sortID = 4;
				}else if(tO.DisplayName.Contains("Causeway1")){
					tO.DisplayName = "Causeway Federal";
					tO.Desc = "Standard federal highway bridge style. Good for any length depending on triangle counts.";
					tO.sortID = 5;
				}else if(tO.DisplayName.Contains("Causeway2")){
					tO.DisplayName = "Causeway Overpass";
					tO.Desc = "Overpass style. Good for any length depending on triangle counts.";
					tO.sortID = 8;
				}else if(tO.DisplayName.Contains("Causeway3")){
					tO.DisplayName = "Causeway Classic";
					tO.Desc = "Classic causeway style. Good for any length depending on triangle counts.";
					tO.sortID = 7;
				}else if(tO.DisplayName.Contains("Causeway4")){
					tO.DisplayName = "Causeway Highway";
					tO.Desc = "State highway style. Good for any length depending on triangle counts.";
					tO.sortID = 6;
				}
			}
			
			if(tO.bIsDefault && !bIsBridge){
				if(tO.DisplayName.Contains("GSDTunnel")){
					tO.DisplayName = "Tunnel";
					tO.Desc = "Designed after the Eisenhower tunnel.";
				}else if(tO.DisplayName.Contains("GSDGroup-WBeamLeftTurn")){
					tO.DisplayName = "Left turn wbeams";
					tO.Desc = "Contains wbeam and signs on right side of road for left turn.";
				}else if(tO.DisplayName.Contains("GSDGroup-KRailLights")){
					tO.DisplayName = "K-rail with lights";
					tO.Desc = "Center divider k-rail with double sided lights. Best used on mostly straight highway type roads.";
				}else if(tO.DisplayName.Contains("GSDGroup-Rumblestrips")){
					tO.DisplayName = "Rumblestrips x2";
					tO.Desc = "Rumble strips on both sides of the road.";
				}else if(tO.DisplayName.Contains("GSDGroup-Fancy1")){
					tO.DisplayName = "Fancy railing x2";
					tO.Desc = "Luxurious railing with lighting on both sides of the road.";
				}
			}
			
			oList.Add(tO);
		}
		oListSort();
	}
	
	public static void GetGroupListing(out string[] tNames, out string[] tPaths, int Lanes, bool bIsDefault = false){
		if(xPath.Length < 5){
			xPath = GSDRootUtil.Dir_GetLibrary();
		}

		string LaneText = "-2L";
		if(Lanes == 4){
			LaneText = "-4L";
		}else if(Lanes == 6){
			LaneText = "-6L";
		}
		
		tNames = null;
		tPaths = null;
		DirectoryInfo info;
		if(bIsDefault){
			info = new DirectoryInfo(xPath + "B/W/");
		}else{
			info = new DirectoryInfo(xPath + "B/");
		}

		FileInfo[] fileInfo = info.GetFiles();
		int tCount = 0;
		foreach(FileInfo tInfo in fileInfo){
			if(tInfo.Extension.ToLower().Contains("gsd")){
				if(!bIsDefault){
					tCount+=1;
				}else{
					if(tInfo.Name.Contains(LaneText)){
						tCount+=1;
					}
				}
			}
		}
		
		tNames = new string[tCount];
		tPaths = new string[tCount];
		tCount = 0;
		foreach(FileInfo tInfo in fileInfo){
			if(tInfo.Extension.ToLower().Contains("gsd")){
				if(!bIsDefault){
					tNames[tCount] = tInfo.Name.Replace(".gsd","");
					tPaths[tCount] = tInfo.FullName;
					tCount+=1;
				}else{
					if(tInfo.Name.Contains(LaneText)){
						tNames[tCount] = tInfo.Name.Replace(".gsd","");
						tPaths[tCount] = tInfo.FullName;
						tCount+=1;
					}
				}
			}
		}
	}
	#endregion
	
	#region "Init objs"
	private void InitObjs(){
		string[] tNames = null;
		string[] tPaths = null;
		//Load user custom ones first:
		if(tWindowType == WindowTypeEnum.Extrusion){
			SplinatedMeshMaker.GetLibraryFiles(out tNames, out tPaths, false);
		}else{
			EdgeObjectMaker.GetLibraryFiles(out tNames, out tPaths, false);
		}
		LoadObjs(ref tNames, ref tPaths, false);
		//Load GSD ones last:
		if(tWindowType == WindowTypeEnum.Extrusion){
			SplinatedMeshMaker.GetLibraryFiles(out tNames, out tPaths, true);
		}else{
			EdgeObjectMaker.GetLibraryFiles(out tNames, out tPaths, true);
		}
		LoadObjs(ref tNames, ref tPaths, true);
	}
	
	private void LoadObjs(ref string[] tNames, ref string[] tPaths, bool bIsDefault = false){
		int tCount = tNames.Length;
		string tPath = "";
		string tStringPath = "";
		string tDesc = "";
		string tDisplayName = "";
		string ThumbString = "";
		bool bIsBridge = false;
		for(int i=0;i<tCount;i++){
			bIsBridge = false;
			tPath = tPaths[i];
			
			if(tWindowType == WindowTypeEnum.Extrusion){
				SplinatedMeshMaker.SplinatedMeshLibraryMaker SLM = (SplinatedMeshMaker.SplinatedMeshLibraryMaker)GSDRootUtil.LoadXML<SplinatedMeshMaker.SplinatedMeshLibraryMaker>(ref tPath);
				if(SLM == null){ continue; }
				tStringPath = SLM.CurrentSplinationString;
				tDesc = SLM.Desc;
				tDisplayName = SLM.DisplayName;
				ThumbString = SLM.ThumbString;
				bIsBridge = SLM.bIsBridge;
			}else if(tWindowType == WindowTypeEnum.Edge){
				EdgeObjectMaker.EdgeObjectLibraryMaker ELM = (EdgeObjectMaker.EdgeObjectLibraryMaker)GSDRootUtil.LoadXML<EdgeObjectMaker.EdgeObjectLibraryMaker>(ref tPath);
				if(ELM == null){ continue; }
				tStringPath = ELM.EdgeObjectString;
				tDesc = ELM.Desc;
				tDisplayName = ELM.DisplayName;
				ThumbString = ELM.ThumbString;
				bIsBridge = ELM.bIsBridge;
			}
			
			//Don't continue if bridge pieces and this is not a bridge piece:
			if(tWindowType == WindowTypeEnum.Extrusion && bIsBridge){ continue; }
			
			GSDRoadUtil.WizardObject tO = new GSDRoadUtil.WizardObject();
			try{
				tO.Thumb = (Texture2D)AssetDatabase.LoadAssetAtPath(ThumbString,typeof(Texture2D)) as Texture2D;	
			}catch{
				tO.Thumb = null;
			}
			if(tO.Thumb == null){
				try{
					GameObject xObj = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(tStringPath,typeof(GameObject)) as GameObject;
					tO.Thumb = AssetPreview.GetAssetPreview(xObj);
				}catch{
					tO.Thumb = null;
				}
			}
			tO.DisplayName = tDisplayName;
			tO.FileName = tNames[i];
			tO.FullPath = tPath;
			tO.Desc = tDesc;
			tO.bIsDefault = bIsDefault;	

			if(bIsDefault && tWindowType == WindowTypeEnum.Edge){
				if(tO.DisplayName.Contains("GSDAtten")){
					tO.DisplayName = "Attenuator";
					tO.Desc = "Standard double WBeam with impact absorption.";
				}else if(tO.DisplayName.Contains("GSDGreenBlinder")){
					tO.DisplayName = "KRail Blinder";
					tO.Desc = "Best results when placed on KRail for KRail blinders.";
					tO.sortID = 5;
				}else if(tO.DisplayName.Contains("GSDRoadBarrelStatic")){
					tO.DisplayName = "Sand Barrel Static";
					tO.Desc = "One static sand barrel. Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDRoadBarrelRigid")){
					tO.DisplayName = "Sand Barrel Rigid";
					tO.Desc = "One rigid sand barrel. Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDRoadBarrel3Static")){
					tO.DisplayName = "Sand Barrels Static 3";
					tO.Desc = "Three static sand barrels in a line. Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDRoadBarrel3Rigid")){
					tO.DisplayName = "Sand Barrels Rigid 3";
					tO.Desc = "Three rigid sand barrels in a line. Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDRoadBarrel7Static")){
					tO.DisplayName = "Sand Barrels Static 7";
					tO.Desc = "Seven static sand barrels in standard formation. Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDRoadBarrel7Rigid")){
					tO.DisplayName = "Sand Barrel Rigid 7";
					tO.Desc = "Seven rigid sand barrels in standard formation. Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDRoadConBarrelStatic")){
					tO.DisplayName = "Con Barrels Static";
					tO.Desc = "Static road construction barrels.";
					tO.sortID = 3;
				}else if(tO.DisplayName.Contains("GSDRoadConBarrelRigid")){
					tO.DisplayName = "Con Barrels Rigid";
					tO.Desc = "Rigid road construction barrels.";
					tO.sortID = 3;
				}else if(tO.DisplayName.Contains("GSDTrafficConeStatic")){
					tO.DisplayName = "Traffic cones Static";
					tO.Desc = "Static traffic cones.";
					tO.sortID = 4;
				}else if(tO.DisplayName.Contains("GSDTrafficConeRigid")){
					tO.DisplayName = "Traffic cones Rigid";
					tO.Desc = "Rigid traffic cones.";
					tO.sortID = 4;
				}else if(tO.DisplayName.Contains("GSDRoadReflector")){
					tO.DisplayName = "Road reflectors";
					tO.Desc = "Placed one center line of road for center line reflection.";
					tO.sortID = 4;
				}else if(tO.DisplayName.Contains("GSDStopSign")){
					tO.DisplayName = "Stop sign";
					tO.Desc = "Standard specification non-interstate stop sign.";
				}else if(tO.DisplayName.Contains("GSDStreetLightSingle")){
					tO.DisplayName = "Streetlight Singlesided";
					tO.Desc = "Best used on side of roads.";
				}else if(tO.DisplayName.Contains("GSDStreetLightDouble")){
					tO.DisplayName = "Streetlight Doublesided";
					tO.Desc = "Best results when embedded in KRail in centerline of road.";
				}else if(tO.DisplayName.Contains("GSDWarningSign1")){
					tO.DisplayName = "Warning Sign #1";
					tO.Desc = "Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDWarningSign2")){
					tO.DisplayName = "Warning Sign #2";
					tO.Desc = "Best results when placed in front of railings or bridges.";
				}else if(tO.DisplayName.Contains("GSDSignRightTurnOnly")){
					tO.DisplayName = "Right turn only";
					tO.Desc = "Best results when placed near intersection right turn lane.";
					tO.sortID = 4;
				}
				
				else if(tO.DisplayName.Contains("GSDSign330")){
					tO.DisplayName = "Signs 330";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-330\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSign396")){
					tO.DisplayName = "Signs 396";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-396\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSign617-Small")){
					tO.DisplayName = "Signs 617 small";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-617\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSign617")){
					tO.DisplayName = "Signs 617";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-617\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSign861-Small")){
					tO.DisplayName = "Signs 861 small";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-861\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSign861")){
					tO.DisplayName = "Sign type 861";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-861\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSign988-Small")){
					tO.DisplayName = "Signs 988 small";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-988\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSign988")){
					tO.DisplayName = "Signs 988";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-988\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSignDiamond")){
					tO.DisplayName = "Signs diamond";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-diamond\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSignSquare-Small")){
					tO.DisplayName = "Signs square small";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-Square\" as the search term.";
					tO.sortID = 21;
				}else if(tO.DisplayName.Contains("GSDSignSquare")){
					tO.DisplayName = "Signs square";
					tO.Desc = "Interchangeable materials, use \"GSDFedSign-Square\" as the search term.";
					tO.sortID = 21;
				}
			}
			
			if(bIsDefault && tWindowType == WindowTypeEnum.Extrusion){
				if(tO.DisplayName.Contains("GSDKRail")){
					tO.DisplayName = "KRail";
					tO.Desc = "Federal spec cement KRailing (also known as Jersey Barriers). Variant with down ends.";
				}else if(tO.DisplayName.Contains("GSDKRailCurvedR")){
					tO.DisplayName = "KRail Curved Right";
					tO.Desc = "Federal spec cement KRailing (also known as Jersey Barriers). Variant with curved ends for right shoulder.";
				}else if(tO.DisplayName.Contains("GSDKRailCurvedL")){
					tO.DisplayName = "KRail Curved Left";
					tO.Desc = "Federal spec cement KRailing (also known as Jersey Barriers). Variant with curved ends for left shoulder.";
				}else if(tO.DisplayName.Contains("GSDWBeam1R")){
					tO.DisplayName = "WBeam Wood Right";
					tO.Desc = "Federal spec wooden pole WBeam railing. Best used as outer shoulder railing. Right shoulder.";
				}else if(tO.DisplayName.Contains("GSDWBeam1L")){
					tO.DisplayName = "WBeam Wood Left";
					tO.Desc = "Federal spec wooden pole WBeam railing. Best used as outer shoulder railing. Left shoulder.";
				}else if(tO.DisplayName.Contains("GSDWBeam2R")){
					tO.DisplayName = "WBeam Metal Right";
					tO.Desc = "Federal spec metal pole WBeam railing. Best used as outer shoulder railing. Right shoulder.";
				}else if(tO.DisplayName.Contains("GSDWBeam2L")){
					tO.DisplayName = "WBeam Metal Left";
					tO.Desc = "Federal spec metal pole WBeam railing. Best used as outer shoulder railing. Left shoulder.";
				}else if(tO.DisplayName.Contains("GSDRailing1")){
					tO.DisplayName = "Railing #1";
					tO.Desc = "Standard double square pole railing.";
				}else if(tO.DisplayName.Contains("GSDRailing2")){
					tO.DisplayName = "Railing #2";
					tO.Desc = "Standard concrete big block railing.";
				}else if(tO.DisplayName.Contains("GSDRailing3")){
					tO.DisplayName = "Railing #3";
					tO.Desc = "Standard four-strand metal railing.";
				}else if(tO.DisplayName.Contains("GSDRailing5")){
					tO.DisplayName = "Railing #5";
					tO.Desc = "Basic concrete railing with pylons.";
				}else if(tO.DisplayName.Contains("GSDRailing6")){
					tO.DisplayName = "Railing #6";
					tO.Desc = "Standard two-strand metal pole railing.";
				}else if(tO.DisplayName.Contains("GSDRailing7")){
					tO.DisplayName = "Railing #7";
					tO.Desc = "Rock-decorated concrete railing with pylons and double strand rusted look metal railing.";
				}else if(tO.DisplayName.Contains("GSDRailing8")){
					tO.DisplayName = "Railing #8";
					tO.Desc = "Rock-decorated concrete railing with standard single pole metal railing.";
                } else if (tO.DisplayName.Contains("GSDRailing9")) {
                    tO.DisplayName = "Railing #9";
                    tO.Desc = "Very low poly railing used for mobile.";
                } else if (tO.DisplayName.Contains("GSDSidewalk")) {
                    tO.DisplayName = "Sidewalk";
                    tO.Desc = "Sidewalk.";
				}else if(tO.DisplayName.Contains("GSDRumbleStrip")){
					tO.DisplayName = "Rumblestrip";
					tO.Desc = "State spec rumblestrip. For best results place several cm from road edge into shoulder.";
				}else if(tO.DisplayName.Contains("GSDRailing4R")){
					tO.DisplayName = "Railing #4 Right";
					tO.Desc = "Three bar angled pole railing. Right side of road.";
				}else if(tO.DisplayName.Contains("GSDRailing4L")){
					tO.DisplayName = "Railing #4 Left";
					tO.Desc = "Three bar angled pole railing. Left side of road.";
				}else if(tO.DisplayName.Contains("GSDRailing4-LightR")){
					tO.DisplayName = "Railing #4 Light Right";
					tO.Desc = "Three bar angled pole railing. Right side of road. Light version with fewer triangle count.";
				}else if(tO.DisplayName.Contains("GSDRailing4-LightL")){
					tO.DisplayName = "Railing #4 Light Left";
					tO.Desc = "Three bar angled pole railing. Left side of road. Light version with fewer triangle count.";
				}else if(tO.DisplayName.Contains("GSDRailingBase1")){
					tO.DisplayName = "Railing base #1";
					tO.Desc = "Use as a base on other railings to create more detail.";
				}else if(tO.DisplayName.Contains("GSDRailingBase2")){
					tO.DisplayName = "Railing base #2";
					tO.Desc = "Use as a base on other railings to create more detail.";
				}else if(tO.DisplayName.Contains("GSDCableBarrier-Light")){
					tO.DisplayName = "Cable barrier 10m";
					tO.Desc = "Cable barrier 10m light triangle version. Best used as center divider or as railing barriers.";
					tO.sortID = 20;
				}else if(tO.DisplayName.Contains("GSDCableBarrier")){
					tO.DisplayName = "Cable barrier 5m";
					tO.Desc = "Cable barrier 5m. Best used as center divider or as railing barriers.";
					tO.sortID = 20;
				}
			}
			
			oList.Add(tO);
		}
		oListSort();
	}
	
	void oListSort(){
		oList.Sort((GSDRoadUtil.WizardObject t1, GSDRoadUtil.WizardObject t2) => {
			if(t1.bIsDefault != t2.bIsDefault){
				return t1.bIsDefault.CompareTo(t2.bIsDefault);
			}else if(t1.sortID != t2.sortID){ 
				return t1.sortID.CompareTo(t2.sortID);
			}else{
				return t1.DisplayName.CompareTo(t2.DisplayName);
			}
		});	
	}
	#endregion
	#endregion
}