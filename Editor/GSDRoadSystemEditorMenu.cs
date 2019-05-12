#if UNITY_EDITOR
#region "Imports"
using UnityEngine;
using UnityEditor;
#endregion


public class GSDRoadSystemEditorMenu : ScriptableObject
{
    /// <summary>
    /// Creates the road system.
    /// </summary>
    [MenuItem("Window/Road Architect/Create road system")]
    public static void CreateRoadSystem()
    {
        Object[] allRoadSystemObjects = GameObject.FindObjectsOfType(typeof(GSDRoadSystem));
        int nextCount = (allRoadSystemObjects.Length + 1);
        allRoadSystemObjects = null;

        GameObject newRoadSystemObject = new GameObject("RoadArchitectSystem" + nextCount.ToString());
        GSDRoadSystem newRoadSystem = newRoadSystemObject.AddComponent<GSDRoadSystem>();
        newRoadSystem.AddRoad(true);//Add road for new road system.

        GameObject masterIntersectionsObject = new GameObject("Intersections");
        masterIntersectionsObject.transform.parent = newRoadSystemObject.transform;
    }


    /// <summary>
    /// Add road to gameobject. Not sure if this is necessary.
    /// </summary>
	[MenuItem("Window/Road Architect/Add road")]
    public static void AddRoad()
    {
        Object[] allRoadSystemObjects = GameObject.FindObjectsOfType(typeof(GSDRoadSystem));
        if (allRoadSystemObjects != null && allRoadSystemObjects.Length == 0)
        {
            CreateRoadSystem();
            return;
        }
        else
        {
            GSDRoadSystem firstRoadSystem = (GSDRoadSystem) allRoadSystemObjects[0];
            Selection.activeGameObject = firstRoadSystem.AddRoad();
        }
    }


    /// <summary>
    /// Updates all roads. Used when things get out of sync.
    /// </summary>
    [MenuItem("Window/Road Architect/Update All Roads")]
    public static void UpdateAllRoads()
    {
        GSDRoad[] allRoadObjects = (GSDRoad[]) GameObject.FindObjectsOfType(typeof(GSDRoad));

        int roadCount = allRoadObjects.Length;

        GSDRoad singleRoad = null;
        GSDSplineC[] tPiggys = null;
        if (roadCount > 1)
        {
            tPiggys = new GSDSplineC[roadCount - 1];
        }

        for (int count = 0; count < roadCount; count++)
        {
            singleRoad = allRoadObjects[count];
            if (count > 0)
            {
                tPiggys[count - 1] = singleRoad.GSDSpline;
            }
        }

        singleRoad = allRoadObjects[0];
        if (tPiggys != null && tPiggys.Length > 0)
        {
            singleRoad.PiggyBacks = tPiggys;
        }
        singleRoad.UpdateRoad();
    }


    /// <summary>
    /// Show the help screen.
    /// </summary>
    [MenuItem("Window/Road Architect/Help")]
    public static void GSDRoadsHelp()
    {
        GSDHelpWindow helpWindow = EditorWindow.GetWindow<GSDHelpWindow>();
        helpWindow.Initialize();
    }


    /// <summary>
    /// WARNING: Only call this on an empty scene that has some terrains on it. MicroGSD LLC is not responsbile for data loss if this function is called by user.
    /// </summary>
    [MenuItem("Window/Road Architect/Testing/Run all unit tests (caution)")]
    public static void TestProgram()
    {
        GSD.Roads.GSDUnitTests.RoadArchitectUnitTests();
    }


    /// <summary>
    /// WARNING: Only call this on an empty scene that has some terrains on it. MicroGSD LLC is not responsbile for data loss if this function is called by user.
    /// </summary>
    [MenuItem("Window/Road Architect/Testing/Clean up tests (caution)")]
    public static void TestCleanup()
    {
        GSD.Roads.GSDUnitTests.CleanupTests();
    }



    /// <summary>
    /// Get code line count for RA project.
    /// </summary>
    [MenuItem("Window/Road Architect/Testing/Get line count of RA")]
    public static void testCodeCount()
    {
        string mainDir = Application.dataPath + "/RoadArchitect/";
        string[] files = System.IO.Directory.GetFiles(mainDir, "*.cs", System.IO.SearchOption.AllDirectories);
        int lineCount = 0;
        foreach (string file in files)
        {
            lineCount += System.IO.File.ReadAllLines(file).Length;
        }
        Debug.Log(string.Format("{0:n0}", lineCount) + " lines of code in Road Architect.");
    }


    [MenuItem("Window/Road Architect/Report a Bug")]
    public static void ReportBug()
    {
        Application.OpenURL("https://github.com/embeddedt/RoadArchitect/issues");
    }
}
#endif