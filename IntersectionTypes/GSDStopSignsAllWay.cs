using UnityEngine;
using System.Collections;

namespace GSD.Roads{

    public class GSDStopSignsAllWay : GSDIntersection
    {
        public GSDStopSignsAllWay()
        {
            IntersectionType = AllowedIntersectionTypes.StopSigns;
            DisplayName = "Stop signs (US)";
        }

        public new static void CreateIntersection(GameObject MasterGameObj, bool bIsRB = true)
        {
            CreateStopSignsAllWay_Do(ref MasterGameObj, bIsRB);
        }
        private static void CreateStopSignsAllWay_Do(ref GameObject MasterGameObj, bool bIsRB)
        {
            Object prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDSignStopAllway.prefab", typeof(GameObject));

            GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
            GSDSplineC tSpline = GSDRI.Node1.GSDSpline;

            GameObject tObj = null;
            //			Vector3 xDir = default(Vector3);
            Vector3 tDir = default(Vector3);
            //			float RoadWidth = tSpline.tRoad.RoadWidth();
            //			float LaneWidth = tSpline.tRoad.opt_LaneWidth;
            float ShoulderWidth = tSpline.tRoad.opt_ShoulderWidth;

            //Cleanup:
            CleanupIntersections(MasterGameObj);

            float Mass = 100f;

            //Get four points:
            float DistFromCorner = (ShoulderWidth * 0.45f);
            Vector3 tPosRR = default(Vector3);
            Vector3 tPosRL = default(Vector3);
            Vector3 tPosLR = default(Vector3);
            Vector3 tPosLL = default(Vector3);
            GetFourPoints(GSDRI, out tPosRR, out tPosRL, out tPosLL, out tPosLR, DistFromCorner);

            //RR:
            tSpline = GSDRI.Node1.GSDSpline;
            tObj = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            //			xDir = (GSDRI.CornerRR - GSDRI.transform.position).normalized;
            tDir = StopSign_GetRot_RR(GSDRI, tSpline);
            tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f, 180f, 0f);
            if (bIsRB)
            {
                Rigidbody RB = tObj.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = new Vector3(0f, -10f, 0f);
            }
            tObj.transform.parent = MasterGameObj.transform;
            tObj.transform.position = tPosRR;
            tObj.name = "StopSignRR";
            if (GSDRI.IgnoreCorner == 0) { Object.DestroyImmediate(tObj); }

            //LL:
            tSpline = GSDRI.Node1.GSDSpline;
            tObj = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            //			xDir = (GSDRI.CornerLL - GSDRI.transform.position).normalized;
            tDir = StopSign_GetRot_LL(GSDRI, tSpline);
            tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f, 180f, 0f);
            if (bIsRB)
            {
                Rigidbody RB = tObj.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = new Vector3(0f, -10f, 0f);
            }
            tObj.transform.parent = MasterGameObj.transform;
            tObj.transform.position = tPosLL;
            tObj.name = "StopSignLL";
            if (GSDRI.IgnoreCorner == 2) { Object.DestroyImmediate(tObj); }

            //RL:
            tSpline = GSDRI.Node2.GSDSpline;
            tObj = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            //			xDir = (GSDRI.CornerRL - GSDRI.transform.position).normalized;
            tDir = StopSign_GetRot_RL(GSDRI, tSpline);
            tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f, 180f, 0f);
            if (bIsRB)
            {
                Rigidbody RB = tObj.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = new Vector3(0f, -10f, 0f);
            }
            tObj.transform.parent = MasterGameObj.transform;
            tObj.transform.position = tPosRL;
            tObj.name = "StopSignRL";
            if (GSDRI.IgnoreCorner == 1) { Object.DestroyImmediate(tObj); }

            //LR:
            tSpline = GSDRI.Node2.GSDSpline;
            tObj = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            //			xDir = (GSDRI.CornerLR - GSDRI.transform.position).normalized;
            tDir = StopSign_GetRot_LR(GSDRI, tSpline);
            tObj.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(0f, 180f, 0f);
            if (bIsRB)
            {
                Rigidbody RB = tObj.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = new Vector3(0f, -10f, 0f);
            }
            tObj.transform.parent = MasterGameObj.transform;
            tObj.transform.position = tPosLR;
            tObj.name = "StopSignLR";
            if (GSDRI.IgnoreCorner == 3) { Object.DestroyImmediate(tObj); }
        }

        private static Vector3 StopSign_GetRot_RR(GSDRoadIntersection GSDRI, GSDSplineC tSpline)
        {
            float tDist = ((Vector3.Distance(GSDRI.CornerRL, GSDRI.CornerRR) / 2f) + (0.025f * Vector3.Distance(GSDRI.CornerLL, GSDRI.CornerRR))) / tSpline.distance; ;
            float p = Mathf.Clamp(GSDRI.Node1.tTime - tDist, 0f, 1f);
            Vector3 POS = tSpline.GetSplineValue(p, true);
            return (POS * -1);
        }

        private static Vector3 StopSign_GetRot_LL(GSDRoadIntersection GSDRI, GSDSplineC tSpline)
        {
            float tDist = ((Vector3.Distance(GSDRI.CornerLR, GSDRI.CornerLL) / 2f) + (0.025f * Vector3.Distance(GSDRI.CornerLL, GSDRI.CornerRR))) / tSpline.distance; ;
            float p = Mathf.Clamp(GSDRI.Node1.tTime + tDist, 0f, 1f);
            Vector3 POS = tSpline.GetSplineValue(p, true);
            return POS;
        }

        private static Vector3 StopSign_GetRot_RL(GSDRoadIntersection GSDRI, GSDSplineC tSpline)
        {
            float tDist = ((Vector3.Distance(GSDRI.CornerLL, GSDRI.CornerRL) / 2f) + (0.025f * Vector3.Distance(GSDRI.CornerLR, GSDRI.CornerRL))) / tSpline.distance; ;
            float p = -1f;
            if (GSDRI.bFlipped)
            {
                p = Mathf.Clamp(GSDRI.Node2.tTime - tDist, 0f, 1f);
            }
            else
            {
                p = Mathf.Clamp(GSDRI.Node2.tTime + tDist, 0f, 1f);
            }
            Vector3 POS = tSpline.GetSplineValue(p, true);
            //POS = Vector3.Cross(POS,Vector3.up);
            if (GSDRI.bFlipped)
            {
                return (POS * -1);
            }
            else
            {
                return POS;
            }
        }

        private static Vector3 StopSign_GetRot_LR(GSDRoadIntersection GSDRI, GSDSplineC tSpline)
        {
            float tDist = ((Vector3.Distance(GSDRI.CornerRR, GSDRI.CornerLR) / 2f) + (0.025f * Vector3.Distance(GSDRI.CornerLR, GSDRI.CornerRL))) / tSpline.distance; ;
            float p = -1f;
            if (GSDRI.bFlipped)
            {
                p = Mathf.Clamp(GSDRI.Node2.tTime + tDist, 0f, 1f);
            }
            else
            {
                p = Mathf.Clamp(GSDRI.Node2.tTime - tDist, 0f, 1f);
            }
            Vector3 POS = tSpline.GetSplineValue(p, true);
            //POS = Vector3.Cross(POS,Vector3.up);
            if (GSDRI.bFlipped)
            {
                return POS;
            }
            else
            {
                return (POS * -1);
            }
        }
    }
}