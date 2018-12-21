using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSD.Roads{

    public abstract class GSDIntersection
    {
        public enum AllowedIntersectionTypes { StopSigns, TrafficLights, Other };
        static int _IntersectionType;
        public AllowedIntersectionTypes IntersectionType
        {
            get
            {
                return (AllowedIntersectionTypes)_IntersectionType;
            }
            set
            {
                _IntersectionType = (int)value;
            }
        }
        public string DisplayName = "";

        public static void CreateIntersection(GameObject MasterGameObj, bool bIsRB = true)
        {
            Debug.LogError("Method CreateIntersection not implemented. Do not use!");
            throw new System.NotImplementedException();
        }

        static bool IsApproximately(float a, float b)
        {
            return IsApproximately(a, b, 0.01f);
        }

        static bool IsApproximately(float a, float b, float tolerance)
        {
            return Mathf.Abs(a - b) < tolerance;
        }

        public static void CleanupIntersections(GameObject MasterGameObj)
        {
            int mCount = MasterGameObj.transform.childCount;
            if (mCount == 0) { return; }
            List<GameObject> tObjtoDelete = new List<GameObject>();
            for (int i = 0; i < mCount; i++)
            {
                if (MasterGameObj.transform.GetChild(i).name.ToLower().Contains("stopsign"))
                {
                    tObjtoDelete.Add(MasterGameObj.transform.GetChild(i).gameObject);
                }
                if (MasterGameObj.transform.GetChild(i).name.ToLower().Contains("trafficlight"))
                {
                    tObjtoDelete.Add(MasterGameObj.transform.GetChild(i).gameObject);
                }
            }
            for (int i = (tObjtoDelete.Count - 1); i >= 0; i--)
            {
                Object.DestroyImmediate(tObjtoDelete[i]);
            }
        }

        public static void GetFourPoints(GSDRoadIntersection GSDRI, out Vector3 tPosRR, out Vector3 tPosRL, out Vector3 tPosLL, out Vector3 tPosLR, float DistFromCorner)
        {
            GetFourPoints_Do(ref GSDRI, out tPosRR, out tPosRL, out tPosLL, out tPosLR, DistFromCorner);
        }

        static void GetFourPoints_Do(ref GSDRoadIntersection GSDRI, out Vector3 tPosRR, out Vector3 tPosRL, out Vector3 tPosLL, out Vector3 tPosLR, float DistFromCorner)
        {
            //Get four points:
            float tPos1 = 0f;
            float tPos2 = 0f;
            Vector3 tTan1 = default(Vector3);
            Vector3 tTan2 = default(Vector3);
            float Node1Width = -1f;
            float Node2Width = -1f;
            Vector3 tVectRR = GSDRI.CornerRR;
            Vector3 tVectRL = GSDRI.CornerRL;
            Vector3 tVectLR = GSDRI.CornerLR;
            Vector3 tVectLL = GSDRI.CornerLL;
            Vector3 tDir = default(Vector3);
            float ShoulderWidth1 = GSDRI.Node1.GSDSpline.tRoad.opt_ShoulderWidth;
            float ShoulderWidth2 = GSDRI.Node2.GSDSpline.tRoad.opt_ShoulderWidth;

            if (!GSDRI.bFlipped)
            {
                //RR:
                Node1Width = (Vector3.Distance(GSDRI.CornerRR, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerRR, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime - Node1Width;
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true) * -1f;
                tPos2 = GSDRI.Node2.tTime + Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true);
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosRR = tVectRR + (tDir * DistFromCorner);
                //RL:
                Node1Width = (Vector3.Distance(GSDRI.CornerRL, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerRL, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime + Node1Width;
                if (GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay) { tPos1 = GSDRI.Node1.tTime; }
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true);
                tPos2 = GSDRI.Node2.tTime + Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true);
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosRL = tVectRL + (tDir * DistFromCorner);
                //LL:
                Node1Width = (Vector3.Distance(GSDRI.CornerLL, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerLL, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime + Node1Width;
                if (GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay) { tPos1 = GSDRI.Node1.tTime; }
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true);
                tPos2 = GSDRI.Node2.tTime - Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true) * -1f;
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosLL = tVectLL + (tDir * DistFromCorner);
                //LR:
                Node1Width = (Vector3.Distance(GSDRI.CornerLR, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerLR, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime - Node1Width;
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true) * -1f;
                tPos2 = GSDRI.Node2.tTime - Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true) * -1f;
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosLR = tVectLR + (tDir * DistFromCorner);
            }
            else
            {
                //RR:
                Node1Width = (Vector3.Distance(GSDRI.CornerRR, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerRR, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime - Node1Width;
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true) * -1f;
                tPos2 = GSDRI.Node2.tTime - Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true) * -1f;
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosRR = tVectRR + (tDir * DistFromCorner);
                //RL:
                Node1Width = (Vector3.Distance(GSDRI.CornerRL, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerRL, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime + Node1Width;
                if (GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay) { tPos1 = GSDRI.Node1.tTime; }
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true);
                tPos2 = GSDRI.Node2.tTime - Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true) * -1f;
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosRL = tVectRL + (tDir * DistFromCorner);
                //LL:
                Node1Width = (Vector3.Distance(GSDRI.CornerLL, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerLL, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime + Node1Width;
                if (GSDRI.iType == GSDRoadIntersection.IntersectionTypeEnum.ThreeWay) { tPos1 = GSDRI.Node1.tTime; }
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true);
                tPos2 = GSDRI.Node2.tTime + Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true);
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosLL = tVectLL + (tDir * DistFromCorner);
                //LR:
                Node1Width = (Vector3.Distance(GSDRI.CornerLR, GSDRI.Node1.pos) + ShoulderWidth1) / GSDRI.Node1.GSDSpline.distance;
                Node2Width = (Vector3.Distance(GSDRI.CornerLR, GSDRI.Node2.pos) + ShoulderWidth2) / GSDRI.Node2.GSDSpline.distance;
                tPos1 = GSDRI.Node1.tTime - Node1Width;
                tTan1 = GSDRI.Node1.GSDSpline.GetSplineValue(tPos1, true) * -1f;
                tPos2 = GSDRI.Node2.tTime + Node2Width;
                tTan2 = GSDRI.Node2.GSDSpline.GetSplineValue(tPos2, true);
                tDir = (tTan1.normalized + tTan2.normalized).normalized;
                tPosLR = tVectLR + (tDir * DistFromCorner);
            }
            tPosRR.y = GSDRI.SignHeight;
            tPosRL.y = GSDRI.SignHeight;
            tPosLL.y = GSDRI.SignHeight;
            tPosLR.y = GSDRI.SignHeight;

            //			GameObject tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
            //			tObj.transform.name = "temp22_RR";
            //			tObj.transform.position = tPosRR;
            //			tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
            //			tObj.transform.name = "temp22_RL";
            //			tObj.transform.position = tPosRL;
            //			tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
            //			tObj.transform.name = "temp22_LL";
            //			tObj.transform.position = tPosLL;
            //			tObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //			tObj.transform.localScale = new Vector3(0.2f,20f,0.2f);
            //			tObj.transform.name = "temp22_LR";
            //			tObj.transform.position = tPosLR;
        }

    }

}