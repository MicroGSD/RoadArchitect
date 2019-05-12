#if UNITY_EDITOR
#region "Imports"
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using GSD;
//using System.Collections;                         // Unused
//using System.Collections.Generic;             // Unused
#endregion


public class GSDSaveWindow : EditorWindow
{
    public enum WindowTypeEnum
    {
        Extrusion,
        Edge,
        BridgeWizard
    };
    private WindowTypeEnum tWindowType = WindowTypeEnum.Extrusion;

    private Texture2D temp2D = null;
    private Texture2D temp2D_2 = null;
    private string ThumbString = "";
    private string Desc = "";
    [UnityEngine.Serialization.FormerlySerializedAs("tFilename")]
    private string fileName = "DefaultName";
    [UnityEngine.Serialization.FormerlySerializedAs("tDisplayName")]
    private string displayName = "DefaultName";
    [UnityEngine.Serialization.FormerlySerializedAs("tDisplayName2")]
    private string displayName2 = "";
    [UnityEngine.Serialization.FormerlySerializedAs("TitleText")]
    private string titleText = "";
    //	private string tPath = "";
    [UnityEngine.Serialization.FormerlySerializedAs("bFileExists")]
    private bool fileExists = false;
    [UnityEngine.Serialization.FormerlySerializedAs("bIsBridge")]
    private bool isBridge = false;
    //	private bool bIsDefault = false;

    private GSD.Roads.Splination.SplinatedMeshMaker[] tSMMs = null;
    private GSD.Roads.EdgeObjects.EdgeObjectMaker[] tEOMs = null;
    private const int titleLabelHeight = 20;

    private string xPath = "";


    private void OnGUI()
    {
        GUILayout.Space(4f);
        EditorGUILayout.LabelField(titleText, EditorStyles.boldLabel);

        temp2D_2 = (Texture2D) EditorGUILayout.ObjectField("Square thumb (optional):", temp2D, typeof(Texture2D), false);
        if (temp2D_2 != temp2D)
        {
            temp2D = temp2D_2;
            ThumbString = AssetDatabase.GetAssetPath(temp2D);
        }

        if (xPath.Length < 5)
        {
            xPath = GSDRootUtil.Dir_GetLibrary();
        }

        EditorGUILayout.LabelField("Short description (optional):");
        Desc = EditorGUILayout.TextArea(Desc, GUILayout.Height(40f));
        displayName2 = EditorGUILayout.TextField("Display name:", displayName);
        if (string.Compare(displayName2, displayName) != 0)
        {
            displayName = displayName2;
            SanitizeFilename();

            if (tWindowType == WindowTypeEnum.Edge)
            {


                if (System.IO.File.Exists(xPath + "EOM" + fileName + ".gsd"))
                {
                    fileExists = true;
                }
                else
                {
                    fileExists = false;
                }
            }
            else if (tWindowType == WindowTypeEnum.Extrusion)
            {
                if (System.IO.File.Exists(xPath + "ESO" + fileName + ".gsd"))
                {
                    fileExists = true;
                }
                else
                {
                    fileExists = false;
                }
            }
            else
            {
                if (System.IO.File.Exists(xPath + "B/" + fileName + ".gsd"))
                {
                    fileExists = true;
                }
                else
                {
                    fileExists = false;
                }
            }
        }


        if (fileExists)
        {
            EditorGUILayout.LabelField("File exists already!", EditorStyles.miniLabel);
            if (tWindowType == WindowTypeEnum.Edge)
            {
                EditorGUILayout.LabelField(xPath + "EOM" + fileName + ".gsd", EditorStyles.miniLabel);
            }
            else if (tWindowType == WindowTypeEnum.Extrusion)
            {
                EditorGUILayout.LabelField(xPath + "ESO" + fileName + ".gsd", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField(xPath + "B/" + fileName + ".gsd", EditorStyles.miniLabel);
            }
        }
        else
        {
            if (tWindowType == WindowTypeEnum.Edge)
            {
                EditorGUILayout.LabelField(xPath + "EOM" + fileName + ".gsd", EditorStyles.miniLabel);
            }
            else if (tWindowType == WindowTypeEnum.Extrusion)
            {
                EditorGUILayout.LabelField(xPath + "ESO" + fileName + ".gsd", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField(xPath + "B/" + fileName + ".gsd", EditorStyles.miniLabel);
            }
        }

        GUILayout.Space(4f);

        isBridge = EditorGUILayout.Toggle("Is bridge related:", isBridge);
        //		GUILayout.Space(4f);
        //		bIsDefault = EditorGUILayout.Toggle("Is GSD:",bIsDefault);
        GUILayout.Space(8f);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        if (tWindowType == WindowTypeEnum.Extrusion)
        {
            DoExtrusion();
        }
        else if (tWindowType == WindowTypeEnum.Edge)
        {
            DoEdgeObject();
        }
        else if (tWindowType == WindowTypeEnum.BridgeWizard)
        {
            DoBridge();
        }

        EditorGUILayout.EndHorizontal();
    }


    private void DoExtrusion()
    {
        if (GUILayout.Button("Save extrusion"))
        {
            SanitizeFilename();
            tSMMs[0].bIsBridge = isBridge;
            tSMMs[0].ThumbString = ThumbString;
            tSMMs[0].Desc = Desc;
            tSMMs[0].DisplayName = displayName;
            tSMMs[0].SaveToLibrary(fileName, false);
            Close();
        }
    }


    private void DoEdgeObject()
    {
        if (GUILayout.Button("Save edge object"))
        {
            SanitizeFilename();
            tEOMs[0].bIsBridge = isBridge;
            tEOMs[0].ThumbString = ThumbString;
            tEOMs[0].Desc = Desc;
            tEOMs[0].DisplayName = displayName;
            tEOMs[0].SaveToLibrary(fileName, false);
            Close();
        }
    }


    private void DoBridge()
    {
        if (GUILayout.Button("Save group"))
        {
            SanitizeFilename();
            GSD.Roads.GSDRoadUtil.WizardObject WO = new GSD.Roads.GSDRoadUtil.WizardObject();
            WO.ThumbString = ThumbString;
            WO.Desc = Desc;
            WO.DisplayName = displayName;
            WO.FileName = fileName;
            WO.bIsBridge = isBridge;
            WO.bIsDefault = false;

            GSD.Roads.GSDRoadUtil.SaveNodeObjects(ref tSMMs, ref tEOMs, ref WO);
            Close();
        }
    }


    private void SanitizeFilename()
    {
        Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        fileName = rgx.Replace(displayName, "");
        fileName = fileName.Replace(" ", "-");
        fileName = fileName.Replace("_", "-");
    }


    #region "Init"
    public void Initialize(ref Rect _rect, WindowTypeEnum _windowType, GSDSplineN _node, GSD.Roads.Splination.SplinatedMeshMaker _SMM = null, GSD.Roads.EdgeObjects.EdgeObjectMaker _EOM = null)
    {
        int Rheight = 300;
        int Rwidth = 360;
        float Rx = ((float) _rect.width / 2f) - ((float) Rwidth / 2f) + _rect.x;
        float Ry = ((float) _rect.height / 2f) - ((float) Rheight / 2f) + _rect.y;

        if (Rx < 0)
        {
            Rx = _rect.x;
        }
        if (Ry < 0)
        {
            Ry = _rect.y;
        }
        if (Rx > (_rect.width + _rect.x))
        {
            Rx = _rect.x;
        }
        if (Ry > (_rect.height + _rect.y))
        {
            Ry = _rect.y;
        }

        Rect fRect = new Rect(Rx, Ry, Rwidth, Rheight);

        if (fRect.width < 300)
        {
            fRect.width = 300;
            fRect.x = _rect.x;
        }
        if (fRect.height < 300)
        {
            fRect.height = 300;
            fRect.y = _rect.y;
        }

        position = fRect;
        tWindowType = _windowType;
        Show();
        titleContent.text = "Save";
        if (tWindowType == WindowTypeEnum.Extrusion)
        {
            titleText = "Save extrusion";
            tSMMs = new GSD.Roads.Splination.SplinatedMeshMaker[1];
            tSMMs[0] = _SMM;
            if (_SMM != null)
            {
                fileName = _SMM.tName;
                displayName = fileName;
            }
        }
        else if (tWindowType == WindowTypeEnum.Edge)
        {
            titleText = "Save edge object";
            tEOMs = new GSD.Roads.EdgeObjects.EdgeObjectMaker[1];
            tEOMs[0] = _EOM;
            if (_EOM != null)
            {
                fileName = _EOM.tName;
                displayName = fileName;
            }
        }
        else if (tWindowType == WindowTypeEnum.BridgeWizard)
        {
            isBridge = true;
            tSMMs = _node.SplinatedObjects.ToArray();
            tEOMs = _node.EdgeObjects.ToArray();
            titleText = "Save group";
            fileName = "Group" + Random.Range(0, 10000).ToString();
            displayName = fileName;
        }

        if (xPath.Length < 5)
        {
            xPath = GSDRootUtil.Dir_GetLibrary();
        }

        if (tWindowType == WindowTypeEnum.Edge)
        {
            if (System.IO.File.Exists(xPath + "EOM" + fileName + ".gsd"))
            {
                fileExists = true;
            }
            else
            {
                fileExists = false;
            }
        }
        else if (tWindowType == WindowTypeEnum.Extrusion)
        {
            if (System.IO.File.Exists(xPath + "ESO" + fileName + ".gsd"))
            {
                fileExists = true;
            }
            else
            {
                fileExists = false;
            }
        }
        else
        {
            if (System.IO.File.Exists(xPath + "B/" + fileName + ".gsd"))
            {
                fileExists = true;
            }
            else
            {
                fileExists = false;
            }
        }
    }
    #endregion
}
#endif