#region "Imports"
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GSD;
#endregion
public class GSDSaveWindow : EditorWindow{
	public enum WindowTypeEnum{Extrusion,
		Edge, 
		BridgeWizard
	};
	WindowTypeEnum tWindowType = WindowTypeEnum.Extrusion;
	
	Texture2D temp2D = null;
	Texture2D temp2D_2 = null;
	string ThumbString = "";
	string Desc = "";
	string tFilename = "DefaultName";
	string tDisplayName = "DefaultName";
	string tDisplayName2 = "";
	string TitleText = "";
//	string tPath = "";
	bool bFileExists = false;
	bool bIsBridge = false;
//	bool bIsDefault = false;

	GSD.Roads.Splination.SplinatedMeshMaker[] tSMMs = null;
	GSD.Roads.EdgeObjects.EdgeObjectMaker[] tEOMs = null;
	const int titleLabelHeight = 20;
	
	string xPath = "";
	
    void OnGUI() {
		GUILayout.Space(4f);
		EditorGUILayout.LabelField(TitleText,EditorStyles.boldLabel);

		temp2D_2 = (Texture2D)EditorGUILayout.ObjectField("Square thumb (optional):",temp2D,typeof(Texture2D), false);
		if(temp2D_2 != temp2D){
			temp2D = temp2D_2;
			ThumbString = AssetDatabase.GetAssetPath(temp2D);
		}
		
		if(xPath.Length < 5){
			xPath = GSDRootUtil.Dir_GetLibrary();
		}
		
		EditorGUILayout.LabelField("Short description (optional):");
		Desc = EditorGUILayout.TextArea(Desc,GUILayout.Height(40f));
		tDisplayName2 = EditorGUILayout.TextField("Display name:",tDisplayName);
		if(string.Compare(tDisplayName2,tDisplayName) != 0){
			tDisplayName = tDisplayName2;	
			SanitizeFilename();
			
			if(tWindowType == WindowTypeEnum.Edge){
				
				
				if(System.IO.File.Exists(xPath + "EOM"+tFilename+".gsd")){
					bFileExists = true;
				}else{
					bFileExists = false;	
				}
			}else if(tWindowType == WindowTypeEnum.Extrusion){
				if(System.IO.File.Exists(xPath + "ESO"+tFilename+".gsd")){
					bFileExists = true;
				}else{
					bFileExists = false;	
				}
			}else{
				if(System.IO.File.Exists(xPath + "B/"+tFilename+".gsd")){
					bFileExists = true;
				}else{
					bFileExists = false;	
				}
			}
		}
		
		
		if(bFileExists){
			EditorGUILayout.LabelField("File exists already!", EditorStyles.miniLabel);
			if(tWindowType == WindowTypeEnum.Edge){
				EditorGUILayout.LabelField(xPath + "EOM"+tFilename + ".gsd", EditorStyles.miniLabel);
			}else if(tWindowType == WindowTypeEnum.Extrusion){
				EditorGUILayout.LabelField(xPath + "ESO"+tFilename + ".gsd", EditorStyles.miniLabel);
			}else{
				EditorGUILayout.LabelField(xPath + "B/"+tFilename + ".gsd", EditorStyles.miniLabel);	
			}
		}else{
			if(tWindowType == WindowTypeEnum.Edge){
				EditorGUILayout.LabelField(xPath + "EOM"+tFilename + ".gsd", EditorStyles.miniLabel);
			}else if(tWindowType == WindowTypeEnum.Extrusion){
				EditorGUILayout.LabelField(xPath + "ESO"+tFilename + ".gsd", EditorStyles.miniLabel);
			}else{
				EditorGUILayout.LabelField(xPath + "B/"+tFilename + ".gsd", EditorStyles.miniLabel);
			}
		}
		
		GUILayout.Space(4f);
		
		bIsBridge = EditorGUILayout.Toggle("Is bridge related:",bIsBridge);
//		GUILayout.Space(4f);
//		bIsDefault = EditorGUILayout.Toggle("Is GSD:",bIsDefault);
		GUILayout.Space(8f);
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Cancel")){
			Close ();
		}
		if(tWindowType == WindowTypeEnum.Extrusion){
			DoExtrusion();
		}else if(tWindowType == WindowTypeEnum.Edge){
			DoEdgeObject();
		}else if(tWindowType ==  WindowTypeEnum.BridgeWizard){
			DoBridge();
		}
		
		EditorGUILayout.EndHorizontal();
	}
	
	void DoExtrusion(){
		if(GUILayout.Button("Save extrusion")){
			SanitizeFilename();
			tSMMs[0].bIsBridge = bIsBridge;
			tSMMs[0].ThumbString = ThumbString;
			tSMMs[0].Desc = Desc;
			tSMMs[0].DisplayName = tDisplayName;
			tSMMs[0].SaveToLibrary(tFilename,false);
			Close();
		}
	}
	
	void DoEdgeObject(){
		if(GUILayout.Button("Save edge object")){
			SanitizeFilename();
			tEOMs[0].bIsBridge = bIsBridge;
			tEOMs[0].ThumbString = ThumbString;
			tEOMs[0].Desc = Desc;
			tEOMs[0].DisplayName = tDisplayName;
			tEOMs[0].SaveToLibrary(tFilename,false);
			Close();
		}
	}
	
	void DoBridge(){
		if(GUILayout.Button("Save group")){
			SanitizeFilename();
			GSD.Roads.GSDRoadUtil.WizardObject WO = new GSD.Roads.GSDRoadUtil.WizardObject();
			WO.ThumbString = ThumbString;
			WO.Desc = Desc;
			WO.DisplayName = tDisplayName;
			WO.FileName = tFilename;
			WO.bIsBridge = bIsBridge;
			WO.bIsDefault = false;

			GSD.Roads.GSDRoadUtil.SaveNodeObjects(ref tSMMs, ref tEOMs, ref WO);
			Close();
		}
	}
	
	void SanitizeFilename(){
		Regex rgx = new Regex("[^a-zA-Z0-9 -]");
		tFilename = rgx.Replace(tDisplayName, "");
		tFilename = tFilename.Replace(" ","-");
		tFilename = tFilename.Replace("_","-");
	}	

	#region "Init"
	public void Initialize(ref Rect tRect, WindowTypeEnum _tWindowType, GSDSplineN tNode, GSD.Roads.Splination.SplinatedMeshMaker SMM = null, GSD.Roads.EdgeObjects.EdgeObjectMaker EOM = null) {
		int Rheight = 300;
		int Rwidth = 360;
		float Rx = ((float)tRect.width/2f) - ((float)Rwidth/2f) + tRect.x;
		float Ry = ((float)tRect.height/2f) - ((float)Rheight/2f) + tRect.y;
		
		if(Rx < 0){ Rx = tRect.x; }
		if(Ry < 0){ Ry = tRect.y; }
		if(Rx > (tRect.width + tRect.x)){ Rx = tRect.x; }
		if(Ry > (tRect.height + tRect.y)){ Ry = tRect.y; }
		
		Rect fRect = new Rect(Rx,Ry,Rwidth,Rheight);

		if(fRect.width < 300){
			fRect.width = 300;
			fRect.x = tRect.x;
		}
		if(fRect.height < 300){
			fRect.height = 300;
			fRect.y = tRect.y;
		}

		position = fRect;
		tWindowType = _tWindowType;
        Show();
        titleContent.text = "Save";
		if(tWindowType == WindowTypeEnum.Extrusion){
			TitleText =  "Save extrusion";
			tSMMs = new GSD.Roads.Splination.SplinatedMeshMaker[1];
			tSMMs[0] = SMM;
			if(SMM != null){
				tFilename = SMM.tName;
				tDisplayName = tFilename;
			}
		}else if(tWindowType == WindowTypeEnum.Edge){
			TitleText =  "Save edge object";
			tEOMs = new GSD.Roads.EdgeObjects.EdgeObjectMaker[1];
			tEOMs[0] = EOM;
			if(EOM != null){
				tFilename = EOM.tName;
				tDisplayName = tFilename;
			}
		}else if(tWindowType ==  WindowTypeEnum.BridgeWizard){
			bIsBridge = true;
			tSMMs = tNode.SplinatedObjects.ToArray();
			tEOMs = tNode.EdgeObjects.ToArray();
			TitleText = "Save group";
			tFilename = "Group" + Random.Range(0,10000).ToString();
			tDisplayName = tFilename;
		}
		
		if(xPath.Length < 5){
			xPath = GSDRootUtil.Dir_GetLibrary();
		}
		
		if(tWindowType == WindowTypeEnum.Edge){
			if(System.IO.File.Exists(xPath + "EOM"+tFilename+".gsd")){
				bFileExists = true;
			}else{
				bFileExists = false;	
			}
		}else if(tWindowType == WindowTypeEnum.Extrusion){
			if(System.IO.File.Exists(xPath + "ESO"+tFilename+".gsd")){
				bFileExists = true;
			}else{
				bFileExists = false;	
			}
		}else{
			if(System.IO.File.Exists(xPath + "B/"+tFilename+".gsd")){
				bFileExists = true;
			}else{
				bFileExists = false;	
			}
		}
    }
	#endregion
}