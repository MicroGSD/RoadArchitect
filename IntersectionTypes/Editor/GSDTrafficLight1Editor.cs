using UnityEngine;
using UnityEditor;

public class GSDTrafficLight1Editor : ScriptableObject
{
    //public static void CustomEditor(GSDRoadIntersectionEditor _)
    //{
    //    //Option: Traffic light timing type:
    //    t_lType.enumValueIndex = (int)EditorGUILayout.Popup("Traffic light timing:", (int)tInter.lType, iTrafficLightSequenceTypeDesc);

    //    //Options: Traffic fixed light timings:
    //    if (tInter.lType == GSDRoadIntersection.LightTypeEnum.Timed)
    //    {
    //        EditorGUILayout.LabelField("Traffic light fixed time lengths (in seconds):");
    //        EditorGUILayout.BeginHorizontal();
    //        t_opt_FixedTime_RegularLightLength.floatValue = EditorGUILayout.Slider("Green length: ", tInter.opt_FixedTime_RegularLightLength, 0.1f, 180f);
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f))) { t_opt_FixedTime_RegularLightLength.floatValue = 30f; }
    //        EditorGUILayout.EndHorizontal();

    //        if (tInter.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane)
    //        {
    //            EditorGUILayout.BeginHorizontal();
    //            t_opt_FixedTime_LeftTurnLightLength.floatValue = EditorGUILayout.Slider("Left turn only length: ", tInter.opt_FixedTime_LeftTurnLightLength, 0.1f, 180f);
    //            if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f))) { t_opt_FixedTime_LeftTurnLightLength.floatValue = 10f; }
    //            EditorGUILayout.EndHorizontal();
    //        }

    //        EditorGUILayout.BeginHorizontal();
    //        t_opt_FixedTime_AllRedLightLength.floatValue = EditorGUILayout.Slider("All red length: ", tInter.opt_FixedTime_AllRedLightLength, 0.1f, 180f);
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f))) { t_opt_FixedTime_AllRedLightLength.floatValue = 1f; }
    //        EditorGUILayout.EndHorizontal();

    //        EditorGUILayout.BeginHorizontal();
    //        t_opt_FixedTime_YellowLightLength.floatValue = EditorGUILayout.Slider("Yellow light length: ", tInter.opt_FixedTime_YellowLightLength, 0.1f, 180f);
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f))) { t_opt_FixedTime_YellowLightLength.floatValue = 3f; }
    //        EditorGUILayout.EndHorizontal();
    //    }


    //    //Option: Traffic light poles:
    //    Line();
    //    EditorGUILayout.LabelField("Traffic light poles:");
    //    t_bTrafficPoleStreetLight.boolValue = EditorGUILayout.Toggle("Street lights: ", tInter.bTrafficPoleStreetLight);

    //    //Option: Traffic light pole gray color:
    //    t_bTrafficLightGray.boolValue = EditorGUILayout.Toggle("Gray color: ", tInter.bTrafficLightGray);

    //    //Option: Normal pole alignment:
    //    t_bRegularPoleAlignment.boolValue = EditorGUILayout.Toggle("Normal pole alignment: ", tInter.bRegularPoleAlignment);
    //    bShowTLPole = EditorGUILayout.Foldout(bShowTLPole, status);
    //    if (bShowTLPole)
    //    {
    //        EditorGUILayout.BeginVertical("box");
    //        EditorGUILayout.LabelField("Street lights: If checked, attaches street lights to each intersection pole. Point lights optional and can be manipulated in the next option segment.");
    //        GUILayout.Space(4f);
    //        EditorGUILayout.LabelField("Gray color: If checked, sets the traffic light pole base materials to gray galvanized steel. If unchecked the material used is black metal paint.");
    //        GUILayout.Space(4f);
    //        EditorGUILayout.LabelField("Normal pole alignment: Recommended to keep this option on unless abnormal twisting of light objects is occurring. Turn this option off if the roads immediately surrounding your intersection are curved at extreme angles and cause irregular twisting of the traffic light objects. Turning this option off will attempt to align the poles perpendicular to the adjacent relevant road without any part existing over the main intersection bounds.");
    //        EditorGUILayout.EndVertical();
    //    }

    //    //Option: Point lights enabled:
    //    Line();
    //    EditorGUILayout.BeginHorizontal();
    //    EditorGUILayout.LabelField("Point lights:");
    //    EditorGUILayout.EndHorizontal();
    //    t_bLightsEnabled.boolValue = EditorGUILayout.Toggle("  Point lights enabled: ", tInter.bLightsEnabled);

    //    //Options: Street point light options:
    //    if (tInter.bTrafficPoleStreetLight)
    //    {
    //        //Option: Street light range:
    //        EditorGUILayout.BeginHorizontal();
    //        t_StreetLight_Range.floatValue = EditorGUILayout.Slider("  Street light range: ", tInter.StreetLight_Range, 1f, 128f);
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f)))
    //        {
    //            t_StreetLight_Range.floatValue = 30f;
    //        }
    //        EditorGUILayout.EndHorizontal();

    //        //Option: Street light intensity:
    //        EditorGUILayout.BeginHorizontal();
    //        t_StreetLight_Intensity.floatValue = EditorGUILayout.Slider("  Street light intensity: ", tInter.StreetLight_Intensity, 0f, 8f);
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f)))
    //        {
    //            t_StreetLight_Intensity.floatValue = 1f;
    //        }
    //        EditorGUILayout.EndHorizontal();

    //        //Option: Street light color:
    //        EditorGUILayout.BeginHorizontal();
    //        t_StreetLight_Color.colorValue = EditorGUILayout.ColorField("  Street light color: ", tInter.StreetLight_Color);
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f)))
    //        {
    //            t_StreetLight_Color.colorValue = new Color(1f, 0.7451f, 0.27451f, 1f); ;
    //        }
    //        EditorGUILayout.EndHorizontal();
    //    }
    //    bShowLightHelp = EditorGUILayout.Foldout(bShowLightHelp, status);
    //    if (bShowLightHelp)
    //    {
    //        EditorGUILayout.BeginVertical("box");
    //        EditorGUILayout.LabelField("Point lights: Enabled means that point lights for the traffic lights (and street lights, if enabled) will be turned on. This is accessible via script \"bLightsEnabled\"");

    //        GUILayout.Space(4f);
    //        EditorGUILayout.LabelField("If street pole lights enabled: Street light range, intensity and color: These settings directly correlate to the standard point light settings.");

    //        GUILayout.Space(4f);
    //        EditorGUILayout.BeginHorizontal();
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f)))
    //        {

    //        }
    //        EditorGUILayout.LabelField(" = Resets settings to default.");
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.EndVertical();
    //    }
    //    Line();

    //    //Option: Traffic light scaling sensitivity:
    //    EditorGUILayout.LabelField("Traffic light scaling sensitivity: *Does not auto-update");
    //    EditorGUILayout.BeginHorizontal();
    //    t_ScalingSense.floatValue = EditorGUILayout.Slider(tInter.ScalingSense, 0f, 200f);
    //    if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f)))
    //    {
    //        t_ScalingSense.floatValue = 170f;
    //    }
    //    EditorGUILayout.EndHorizontal(); GUILayout.Space(4f);
    //    EditorGUILayout.BeginHorizontal();
    //    bShowTLSense = EditorGUILayout.Foldout(bShowTLSense, status);
    //    if (GUILayout.Button("Manually update intersection", EditorStyles.miniButton, GUILayout.Width(170f)))
    //    {
    //        TriggerRoadUpdate(true);
    //    }
    //    EditorGUILayout.EndHorizontal();
    //    if (bShowTLSense)
    //    {
    //        EditorGUILayout.BeginVertical("box");
    //        EditorGUILayout.LabelField("Increasing this value will increase the scaling sensitivity relative to the size of the intersection. Higher scaling value = bigger traffic lights at further distances. Default value is 170.");
    //        GUILayout.Space(4f);
    //        EditorGUILayout.BeginHorizontal();
    //        if (GUILayout.Button(btnRefreshText, GSDImageButton, GUILayout.Width(16f)))
    //        {

    //        }
    //        EditorGUILayout.LabelField(" = Resets settings to default.");
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.EndVertical();
    //    }
    //    GUILayout.Space(4f);
    //}
}