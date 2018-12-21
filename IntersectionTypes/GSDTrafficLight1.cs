using UnityEngine;
using System.Collections;
using static GSD.GSDRootUtil;

namespace GSD.Roads{

    public class GSDTrafficLight1 : GSDIntersection
    {
        public GSDTrafficLight1() {
            IntersectionType = AllowedIntersectionTypes.TrafficLights;
            DisplayName = "Traffic Lights (US)";
        }
        public new static void CreateIntersection(GameObject MasterGameObj, bool bIsTrafficLight1 = true)
        {
            CreateTrafficLightBases_Do(ref MasterGameObj, bIsTrafficLight1);
        }
        private static void CreateTrafficLightBases_Do(ref GameObject MasterGameObj, bool bIsTrafficLight1)
        {
            GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
            GSDSplineC tSpline = GSDRI.Node1.GSDSpline;
            bool bIsRB = true;

            //			float RoadWidth = tSpline.tRoad.RoadWidth();
            float LaneWidth = tSpline.tRoad.opt_LaneWidth;
            float ShoulderWidth = tSpline.tRoad.opt_ShoulderWidth;

            int Lanes = tSpline.tRoad.opt_Lanes;
            int LanesHalf = Lanes / 2;
            float LanesForInter = -1;
            if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes)
            {
                LanesForInter = LanesHalf + 1f;
            }
            else if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane)
            {
                LanesForInter = LanesHalf + 1f;
            }
            else
            {
                LanesForInter = LanesHalf - 1 + 1f;
            }
            float DistFromCorner = (ShoulderWidth * 0.45f);
            float TLDistance = (LanesForInter * LaneWidth) + DistFromCorner;

            GameObject tObjRR = null;
            GameObject tObjRL = null;
            GameObject tObjLL = null;
            GameObject tObjLR = null;
            //			Vector3 xDir = default();
            Vector3 tDir = default;
            float Mass = 12500f;
            Vector3 COM = new Vector3(0f, 0f, 4f);
            Vector3 zeroVect = new Vector3(0f, 0f, 0f);
            Vector3 StartVec = default;
            Vector3 EndVec = default;
            //			bool bContains = false;
            //			MeshFilter MF = null;
            //			Vector3[] tVerts = null;
            Rigidbody RB = null;

            //Get four points:
            Vector3 tPosRR = default;
            Vector3 tPosRL = default;
            Vector3 tPosLR = default;
            Vector3 tPosLL = default;
            GetFourPoints(GSDRI, out tPosRR, out tPosRL, out tPosLL, out tPosLR, DistFromCorner);

            //Cleanup:
            CleanupIntersections(MasterGameObj);

            float[] tempDistances = new float[4];
            tempDistances[0] = Vector3.Distance(GSDRI.CornerRL, GSDRI.CornerLL);
            tempDistances[1] = Vector3.Distance(GSDRI.CornerRL, GSDRI.CornerRR);
            tempDistances[2] = Vector3.Distance(GSDRI.CornerLR, GSDRI.CornerLL);
            tempDistances[3] = Vector3.Distance(GSDRI.CornerLR, GSDRI.CornerRR);
            float MaxDistanceStart = Mathf.Max(tempDistances);
            bool OrigPoleAlignment = GSDRI.bRegularPoleAlignment;

            //Node1:
            //RL:
            tObjRL = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
            //			xDir = (GSDRI.CornerRL - GSDRI.transform.position).normalized;
            tDir = TrafficLightBase_GetRot_RL(GSDRI, tSpline, DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
            tObjRL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
            tObjRL.transform.parent = MasterGameObj.transform;
            StartVec = tPosRL;
            EndVec = (tDir.normalized * TLDistance) + StartVec;
            if (!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec, EndVec))
            { //Convert to regular alignment if necessary
                tObjRL.transform.parent = null;
                tDir = TrafficLightBase_GetRot_RL(GSDRI, tSpline, DistFromCorner, true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjRL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
                tObjRL.transform.parent = MasterGameObj.transform;
            }
            else
            {
                GSDRI.bRegularPoleAlignment = true;
                if (tObjRL != null) { Object.DestroyImmediate(tObjRL); }
                tObjRL = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
                //				xDir = (GSDRI.CornerRL - GSDRI.transform.position).normalized;
                tDir = TrafficLightBase_GetRot_RL(GSDRI, tSpline, DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjRL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
                tObjRL.transform.parent = MasterGameObj.transform;
                StartVec = tPosRL;
                EndVec = (tDir.normalized * TLDistance) + StartVec;
                GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
            }
            if (bIsRB)
            {
                RB = tObjRL.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = COM;
                tObjRL.AddComponent<GSDRigidBody>();
            }
            tObjRL.transform.position = tPosRL;
            tObjRL.transform.name = "TrafficLightRL";
            //LR:
            tObjLR = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
            //			xDir = (GSDRI.CornerLR - GSDRI.transform.position).normalized;
            tDir = TrafficLightBase_GetRot_LR(GSDRI, tSpline, DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
            tObjLR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
            tObjLR.transform.parent = MasterGameObj.transform;
            StartVec = tPosLR;
            EndVec = (tDir.normalized * TLDistance) + StartVec;
            if (!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec, EndVec))
            { //Convert to regular alignment if necessary
                tObjLR.transform.parent = null;
                tDir = TrafficLightBase_GetRot_LR(GSDRI, tSpline, DistFromCorner, true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjLR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
                tObjLR.transform.parent = MasterGameObj.transform;
            }
            else
            {
                GSDRI.bRegularPoleAlignment = true;
                if (tObjLR != null) { Object.DestroyImmediate(tObjLR); }
                tObjLR = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
                //				xDir = (GSDRI.CornerLR - GSDRI.transform.position).normalized;
                tDir = TrafficLightBase_GetRot_LR(GSDRI, tSpline, DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjLR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
                tObjLR.transform.parent = MasterGameObj.transform;
                StartVec = tPosLR;
                EndVec = (tDir.normalized * TLDistance) + StartVec;
                GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
            }
            if (bIsRB)
            {
                RB = tObjLR.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = COM;
                tObjLR.AddComponent<GSDRigidBody>();
            }
            tObjLR.transform.position = tPosLR;
            tObjLR.transform.name = "TrafficLightLR";
            //Node2:
            //RR:
            tObjRR = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
            //			xDir = (GSDRI.CornerRR - GSDRI.transform.position).normalized;
            tDir = TrafficLightBase_GetRot_RR(GSDRI, tSpline, DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
            tObjRR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
            tObjRR.transform.parent = MasterGameObj.transform;
            StartVec = tPosRR;
            EndVec = (tDir.normalized * TLDistance) + StartVec;
            if (!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec, EndVec))
            { //Convert to regular alignment if necessary
                tObjRR.transform.parent = null;
                tDir = TrafficLightBase_GetRot_RR(GSDRI, tSpline, DistFromCorner, true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjRR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, 0f, 0f);
                tObjRR.transform.parent = MasterGameObj.transform;
            }
            else
            {
                GSDRI.bRegularPoleAlignment = true;
                if (tObjRR != null) { Object.DestroyImmediate(tObjRR); }
                tObjRR = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
                //				xDir = (GSDRI.CornerRR - GSDRI.transform.position).normalized;
                tDir = TrafficLightBase_GetRot_RR(GSDRI, tSpline, DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjRR.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
                tObjRR.transform.parent = MasterGameObj.transform;
                StartVec = tPosRR;
                EndVec = (tDir.normalized * TLDistance) + StartVec;
                GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
            }
            if (bIsRB)
            {
                RB = tObjRR.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = COM;
                tObjRR.AddComponent<GSDRigidBody>();
            }
            tObjRR.transform.position = tPosRR;
            tObjRR.transform.name = "TrafficLightRR";

            //LL:
            tObjLL = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
            //			xDir = (GSDRI.CornerLL - GSDRI.transform.position).normalized;
            tDir = TrafficLightBase_GetRot_LL(GSDRI, tSpline, DistFromCorner);
            if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
            tObjLL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
            tObjLL.transform.parent = MasterGameObj.transform;
            StartVec = tPosLL;
            EndVec = (tDir.normalized * TLDistance) + StartVec;
            if (!GSDRI.bRegularPoleAlignment && GSDRI.ContainsLine(StartVec, EndVec))
            { //Convert to regular alignment if necessary
                tObjLL.transform.parent = null;
                tDir = TrafficLightBase_GetRot_LL(GSDRI, tSpline, DistFromCorner, true);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjLL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, 0f, 0f);
                tObjLL.transform.parent = MasterGameObj.transform;
            }
            else
            {
                GSDRI.bRegularPoleAlignment = true;
                if (tObjLL != null) { Object.DestroyImmediate(tObjLL); }
                tObjLL = CreateTrafficLight(TLDistance, true, true, MaxDistanceStart, GSDRI.bTrafficPoleStreetLight, tSpline.tRoad.GSDRS.opt_bSaveMeshes);
                //				xDir = (GSDRI.CornerLL - GSDRI.transform.position).normalized;
                tDir = TrafficLightBase_GetRot_LL(GSDRI, tSpline, DistFromCorner);
                if (tDir == zeroVect) { tDir = new Vector3(0f, 0.0001f, 0f); }
                tObjLL.transform.rotation = Quaternion.LookRotation(tDir) * Quaternion.Euler(-90f, -180f, 0f);
                tObjLL.transform.parent = MasterGameObj.transform;
                StartVec = tPosLL;
                EndVec = (tDir.normalized * TLDistance) + StartVec;
                GSDRI.bRegularPoleAlignment = OrigPoleAlignment;
            }
            if (bIsRB)
            {
                RB = tObjLL.AddComponent<Rigidbody>();
                RB.mass = Mass;
                RB.centerOfMass = COM;
                tObjLL.AddComponent<GSDRigidBody>();
            }
            tObjLL.transform.position = tPosLL;
            tObjLL.transform.name = "TrafficLightLL";

            //Create the actual lights:
            CreateTrafficLightMains(MasterGameObj, tObjRR, tObjRL, tObjLL, tObjLR);
        }

        private static bool CreateTrafficLightBase_IsInIntersection(GSDRoadIntersection GSDRI, ref Vector3 StartVec, ref Vector3 EndVec)
        {
            return GSDRI.ContainsLine(StartVec, EndVec);
        }

        private static GameObject CreateTrafficLight(float tDistance, bool bIsTrafficLight1, bool bOptionalCollider, float xDistance, bool bLight, bool bSaveAsset)
        {
            GameObject tObj = null;
            string tTrafficLightNumber = "1";
            if (!bIsTrafficLight1)
            {
                tTrafficLightNumber = "2";
            }

            bool bDoCustom = false;
            xDistance = Mathf.Ceil(xDistance);  //Round up to nearest whole F
            tDistance = Mathf.Ceil(tDistance);  //Round up to nearest whole F
                                                //			string assetName = "GSDInterTLB" + tTrafficLightNumber + "_" + tDistance.ToString("F0") + "_" + xDistance.ToString("F0") + ".prefab";
            string assetNameAsset = "GSDInterTLB" + tTrafficLightNumber + "_" + tDistance.ToString("F0") + "_" + xDistance.ToString("F0") + ".asset";
            string BackupFBX = "GSDInterTLB" + tTrafficLightNumber + ".FBX";
            float tMod = tDistance / 5f;
            float hMod = (tDistance / 10f) * 0.7f;
            float xMod = ((xDistance / 20f) + 2f) * 0.3334f;
            xMod = Mathf.Clamp(xMod, 1f, 20f);
            tMod = Mathf.Clamp(tMod, 1f, 20f);
            hMod = Mathf.Clamp(hMod, 1f, 20f);

            bool bXMod = false;
            if (!IsApproximately(xMod, 1f, 0.0001f)) { bXMod = true; }

            Mesh xMesh = (Mesh)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/TrafficLightBases/" + assetNameAsset, typeof(Mesh));
            if (xMesh == null)
            {
                xMesh = (Mesh)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/TrafficLightBases/" + BackupFBX, typeof(Mesh));
                bDoCustom = true;
            }

            tObj = new GameObject("TempTrafficLight");
            MeshFilter MF = tObj.GetComponent<MeshFilter>(); if (MF == null) { MF = tObj.AddComponent<MeshFilter>(); }
            MeshRenderer MR = tObj.GetComponent<MeshRenderer>(); if (MR == null) { MR = tObj.AddComponent<MeshRenderer>(); }
            GSD.Roads.GSDRoadUtilityEditor.SetRoadMaterial("Assets/RoadArchitect/Materials/Signs/GSDInterTLB" + tTrafficLightNumber + ".mat", MR);

            if (!bDoCustom)
            {
                MF.sharedMesh = xMesh;
            }

            float tempMaxHeight = 7.6f * hMod;
            float xValue = tempMaxHeight - 7.6f;
            if (bDoCustom)
            {
                Mesh tMesh = new Mesh();
                tMesh.vertices = xMesh.vertices;
                tMesh.triangles = xMesh.triangles;
                tMesh.uv = xMesh.uv;
                tMesh.normals = xMesh.normals;
                tMesh.tangents = xMesh.tangents;
                MF.sharedMesh = tMesh;
                Vector3[] tVerts = tMesh.vertices;

                xValue = (xMod * 6f) - 6f;
                if ((xMod * 6f) > (tempMaxHeight - 1f))
                {
                    xValue = (tempMaxHeight - 1f) - 6f;
                }

                //				float tValue = 0f;
                //				float hValue = 0f;
                bool bIgnoreMe = false;


                int mCount = tVerts.Length;
                Vector2[] uv = tMesh.uv;
                //				List<int> tUVInts = new List<int>();
                for (int i = 0; i < mCount; i++)
                {
                    bIgnoreMe = false;
                    if (IsApproximately(tVerts[i].y, 5f, 0.01f))
                    {
                        tVerts[i].y = tDistance;
                        if (uv[i].y > 3.5f)
                        {
                            uv[i].y *= tMod;
                        }
                        bIgnoreMe = true;
                    }
                    if (!bIgnoreMe && tVerts[i].z > 7.5f)
                    {
                        tVerts[i].z *= hMod;
                        if (uv[i].y > 3.8f)
                        {
                            uv[i].y *= hMod;
                        }
                    }

                    if (bXMod && tVerts[i].z > 4.8f && tVerts[i].z < 6.2f)
                    {
                        tVerts[i].z += xValue;
                    }
                }
                tMesh.vertices = tVerts;
                tMesh.uv = uv;
                tMesh.RecalculateNormals();
                tMesh.RecalculateBounds();

                //Save:
                if (bSaveAsset)
                {
                    UnityEditor.AssetDatabase.CreateAsset(tMesh, "Assets/RoadArchitect/Mesh/RoadObj/Signs/TrafficLightBases/" + assetNameAsset);
                }
            }

            BoxCollider BC = tObj.AddComponent<BoxCollider>();
            float MaxHeight = MF.sharedMesh.vertices[447].z;
            BC.size = new Vector3(0.35f, 0.35f, MaxHeight);
            BC.center = new Vector3(0f, 0f, (MaxHeight / 2f));

            if (bOptionalCollider)
            {
                float MaxWidth = MF.sharedMesh.vertices[497].y;
                GameObject tObjCollider = new GameObject("col2");
                BC = tObjCollider.AddComponent<BoxCollider>();
                BC.size = new Vector3(0.175f, MaxWidth, 0.175f);
                BC.center = new Vector3(0f, MaxWidth / 2f, 5.808f);
                tObjCollider.transform.parent = tObj.transform;
            }

            if (bLight)
            {
                GameObject yObj = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDStreetLight_TrafficLight.prefab", typeof(GameObject));
                GameObject kObj = (GameObject)GameObject.Instantiate(yObj);
                kObj.transform.position = tObj.transform.position;
                kObj.transform.position += new Vector3(0f, 0f, MaxHeight - 7.6f);
                kObj.transform.parent = tObj.transform;
                kObj.transform.rotation = Quaternion.identity;
                //				kObj.name = "StreetLight";
            }


            //Bounds calcs:
            MeshFilter[] tMeshes = tObj.GetComponents<MeshFilter>();
            for (int i = 0; i < tMeshes.Length; i++) { tMeshes[i].sharedMesh.RecalculateBounds(); }

            return tObj;
        }

        private static Vector3 TrafficLightBase_GetRot_RL(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false)
        {
            Vector3 POS = default;
            if (!GSDRI.bRegularPoleAlignment && !bOverrideRegular)
            {
                //				float tDist = ((Vector3.Distance(GSDRI.CornerRR,GSDRI.CornerRL) / 2f) + DistFromCorner) / tSpline.distance;;
                float p = Mathf.Clamp(GSDRI.Node1.tTime, 0f, 1f);
                POS = tSpline.GetSplineValue(p, true);
                POS = Vector3.Cross(POS, Vector3.up);
                return POS;
            }
            else
            {
                POS = GSDRI.CornerRL - GSDRI.CornerLL;
                return POS * -1;
            }
        }

        private static Vector3 TrafficLightBase_GetRot_LR(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false)
        {
            Vector3 POS = default;
            if (!GSDRI.bRegularPoleAlignment && !bOverrideRegular)
            {
                //				float tDist = ((Vector3.Distance(GSDRI.CornerLR,GSDRI.CornerLL) / 2f) + DistFromCorner) / tSpline.distance;;
                float p = Mathf.Clamp(GSDRI.Node1.tTime, 0f, 1f);
                POS = tSpline.GetSplineValue(p, true);
                POS = Vector3.Cross(POS, Vector3.up);
                return POS * -1;
            }
            else
            {
                POS = GSDRI.CornerRR - GSDRI.CornerLR;
                return POS;
            }
        }

        private static Vector3 TrafficLightBase_GetRot_RR(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false)
        {
            Vector3 POS = default;
            if (!GSDRI.bRegularPoleAlignment && !bOverrideRegular)
            {
                //				float tDist = ((Vector3.Distance(GSDRI.CornerRR,GSDRI.CornerLR) / 2f) + DistFromCorner) / tSpline.distance;;
                float p = Mathf.Clamp(GSDRI.Node2.tTime, 0f, 1f);
                POS = tSpline.GetSplineValue(p, true);
                POS = Vector3.Cross(POS, Vector3.up); if (GSDRI.bFlipped) { POS = POS * -1; }
            }
            else
            {
                POS = GSDRI.CornerLL - GSDRI.CornerLR;
            }
            return POS;
        }

        private static Vector3 TrafficLightBase_GetRot_LL(GSDRoadIntersection GSDRI, GSDSplineC tSpline, float DistFromCorner, bool bOverrideRegular = false)
        {
            Vector3 POS = default;
            if (!GSDRI.bRegularPoleAlignment && !bOverrideRegular)
            {
                //				float tDist = ((Vector3.Distance(GSDRI.CornerLL,GSDRI.CornerRL) / 2f) + DistFromCorner) / tSpline.distance;;
                float p = Mathf.Clamp(GSDRI.Node2.tTime, 0f, 1f);
                POS = tSpline.GetSplineValue(p, true);
                POS = Vector3.Cross(POS, Vector3.up); if (GSDRI.bFlipped) { POS = POS * -1; }
            }
            else
            {
                POS = GSDRI.CornerRL - GSDRI.CornerRR;
            }
            return POS * -1;
        }

        private static void CreateTrafficLightMains(GameObject MasterGameObj, GameObject tRR, GameObject tRL, GameObject tLL, GameObject tLR)
        {
            GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
            GSDSplineC tSpline = GSDRI.Node1.GSDSpline;

            float tDist = (Vector3.Distance(GSDRI.CornerRL, GSDRI.CornerRR) / 2f) / tSpline.distance;
            Vector3 tan = tSpline.GetSplineValue(GSDRI.Node1.tTime + tDist, true);
            ProcessPole(MasterGameObj, tRL, tan * -1, 1, Vector3.Distance(GSDRI.CornerRL, GSDRI.CornerRR));
            tDist = (Vector3.Distance(GSDRI.CornerLR, GSDRI.CornerLL) / 2f) / tSpline.distance;
            tan = tSpline.GetSplineValue(GSDRI.Node1.tTime - tDist, true);
            ProcessPole(MasterGameObj, tLR, tan, 3, Vector3.Distance(GSDRI.CornerLR, GSDRI.CornerLL));


            float InterDist = Vector3.Distance(GSDRI.CornerRL, GSDRI.CornerLL);
            tDist = (InterDist / 2f) / tSpline.distance;
            tan = tSpline.GetSplineValue(GSDRI.Node1.tTime + tDist, true);

            float fTime1 = GSDRI.Node2.tTime + tDist;
            float fTime2neg = GSDRI.Node2.tTime - tDist;

            tSpline = GSDRI.Node2.GSDSpline;
            if (GSDRI.bFlipped)
            {
                tan = tSpline.GetSplineValue(fTime1, true);
                ProcessPole(MasterGameObj, tRR, tan, 0, InterDist);
                tan = tSpline.GetSplineValue(fTime2neg, true);
                ProcessPole(MasterGameObj, tLL, tan * -1, 2, InterDist);
            }
            else
            {
                tan = tSpline.GetSplineValue(fTime2neg, true);
                ProcessPole(MasterGameObj, tRR, tan * -1, 0, InterDist);
                tan = tSpline.GetSplineValue(fTime1, true);
                ProcessPole(MasterGameObj, tLL, tan, 2, InterDist);
            }

            if (GSDRI.IgnoreCorner == 0)
            {
                if (tRR != null) { Object.DestroyImmediate(tRR); }
            }
            else if (GSDRI.IgnoreCorner == 1)
            {
                if (tRL != null) { Object.DestroyImmediate(tRL); }
            }
            else if (GSDRI.IgnoreCorner == 2)
            {
                if (tLL != null) { Object.DestroyImmediate(tLL); }
            }
            else if (GSDRI.IgnoreCorner == 3)
            {
                if (tLR != null) { Object.DestroyImmediate(tLR); }
            }
        }

        private static void ProcessPole(GameObject MasterGameObj, GameObject tObj, Vector3 tan, int tCorner, float InterDist)
        {
            GSDRoadIntersection GSDRI = MasterGameObj.GetComponent<GSDRoadIntersection>();
            GSDSplineC tSpline = GSDRI.Node1.GSDSpline;
            //			bool bIsRB = true;

            //			float RoadWidth = tSpline.tRoad.RoadWidth();
            float LaneWidth = tSpline.tRoad.opt_LaneWidth;
            float ShoulderWidth = tSpline.tRoad.opt_ShoulderWidth;

            int Lanes = tSpline.tRoad.opt_Lanes;
            int LanesHalf = Lanes / 2;
            float LanesForInter = -1;
            if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes)
            {
                LanesForInter = LanesHalf + 1f;
            }
            else if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.TurnLane)
            {
                LanesForInter = LanesHalf + 1f;
            }
            else
            {
                LanesForInter = LanesHalf;
            }
            float DistFromCorner = (ShoulderWidth * 0.35f);
            float TLDistance = (LanesForInter * LaneWidth) + DistFromCorner;

            MeshFilter MF = tObj.GetComponent<MeshFilter>();
            Mesh tMesh = MF.sharedMesh;
            Vector3[] tVerts = tMesh.vertices;
            Vector3 StartVec = tVerts[520];
            Vector3 EndVec = tVerts[521];
            StartVec = (((EndVec - StartVec) * (DistFromCorner / TLDistance)) + StartVec);
            Vector3 tempR_Start = tVerts[399];
            Vector3 tempR_End = tVerts[396];
            Vector3 tLanePosR = ((tempR_End - tempR_Start) * 0.95f) + tempR_Start;
            tLanePosR.z -= 1f;

            float SmallerDist = Vector3.Distance(StartVec, EndVec);

            //StartVec = Corner
            //2.5m = lane
            //7.5m = lane
            //12.5m = lane
            Vector3[] tLanePos = new Vector3[LanesHalf];
            for (int i = 0; i < LanesHalf; i++)
            {
                tLanePos[i] = (((EndVec - StartVec) * (((LaneWidth * 0.5f) + (i * LaneWidth)) / SmallerDist)) + StartVec);
            }
            Vector3 tLanePosL = default;
            Vector3 tLanePosL_Sign = default;

            if (GSDRI.bLeftTurnYieldOnGreen)
            {
                tLanePosL = ((EndVec - StartVec) * ((SmallerDist - 1.8f) / SmallerDist)) + StartVec;
                tLanePosL_Sign = ((EndVec - StartVec) * ((SmallerDist - 3.2f) / SmallerDist)) + StartVec;
            }
            else
            {
                tLanePosL = ((EndVec - StartVec) * ((SmallerDist - 2.5f) / SmallerDist)) + StartVec;
            }

            Vector3 tPos1 = default;
            if (tCorner == 0)
            { //RR
                tPos1 = GSDRI.CornerLR;
            }
            else if (tCorner == 1)
            { //RL
                tPos1 = GSDRI.CornerRR;
            }
            else if (tCorner == 2)
            { //LL
                tPos1 = GSDRI.CornerRL;
            }
            else if (tCorner == 3)
            { //LR
                tPos1 = GSDRI.CornerLL;
            }

            int mCount = tLanePos.Length;
            float mDistance = -50000f;
            float tDistance = 0f;
            for (int i = 0; i < mCount; i++)
            {
                tDistance = Vector3.Distance(tObj.transform.TransformPoint(tLanePos[i]), tPos1);
                if (tDistance > mDistance) { mDistance = tDistance; }
            }
            tDistance = Vector3.Distance(tObj.transform.TransformPoint(tLanePosL), tPos1);
            if (tDistance > mDistance) { mDistance = tDistance; }
            tDistance = Vector3.Distance(tObj.transform.TransformPoint(tLanePosR), tPos1);
            if (tDistance > mDistance) { mDistance = tDistance; }

            float tScaleSense = ((200f - GSDRI.ScalingSense) / 200f) * 200f;
            tScaleSense = Mathf.Clamp(tScaleSense * 0.1f, 0f, 20f);
            float ScaleMod = ((mDistance / 17f) + tScaleSense) * (1f / (tScaleSense + 1f));
            if (IsApproximately(tScaleSense, 20f, 0.05f)) { ScaleMod = 1f; }
            ScaleMod = Mathf.Clamp(ScaleMod, 1f, 1.5f);
            Vector3 tScale = new Vector3(ScaleMod, ScaleMod, ScaleMod);
            bool bScale = true; if (IsApproximately(ScaleMod, 1f, 0.001f)) { bScale = false; }

            //Debug.Log (mDistance + " " + ScaleMod + " " + tScaleSense);

            GameObject tRight = null;
            GameObject tLeft = null;
            GameObject tLeft_Sign = null;
            Object prefab = null;

            MeshRenderer MR_Left = null;
            MeshRenderer MR_Right = null;
            MeshRenderer[] MR_Mains = new MeshRenderer[LanesHalf];
            int cCount = -1;
            if (GSDRI.rType != GSDRoadIntersection.RoadTypeEnum.NoTurnLane)
            {
                if (GSDRI.bLeftTurnYieldOnGreen)
                {
                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightLeftYield.prefab", typeof(GameObject));
                }
                else
                {
                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightLeft.prefab", typeof(GameObject));
                }
                tLeft = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                tLeft.transform.position = tObj.transform.TransformPoint(tLanePosL);
                tLeft.transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(0f, 90f, 0f);
                tLeft.transform.parent = tObj.transform;
                tLeft.transform.name = "LightLeft";

                cCount = tLeft.transform.childCount;
                for (int i = 0; i < cCount; i++)
                {
                    if (tLeft.transform.GetChild(i).name.ToLower() == "lights")
                    {
                        MR_Left = tLeft.transform.GetChild(i).GetComponent<MeshRenderer>();
                    }
                }

                if (bScale) { tLeft.transform.localScale = tScale; }

                if (GSDRI.bLeftTurnYieldOnGreen)
                {
                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDSignYieldOnGreen.prefab", typeof(GameObject));
                    tLeft_Sign = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                    tLeft_Sign.transform.position = tObj.transform.TransformPoint(tLanePosL_Sign);
                    tLeft_Sign.transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(-90f, 90f, 0f);
                    tLeft_Sign.transform.parent = tObj.transform;
                    tLeft_Sign.transform.name = "SignYieldOnGreen";
                    if (bScale) { tLeft_Sign.transform.localScale = tScale; }
                }
            }
            if (GSDRI.rType == GSDRoadIntersection.RoadTypeEnum.BothTurnLanes)
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightRight.prefab", typeof(GameObject));
                tRight = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                tRight.transform.position = tObj.transform.TransformPoint(tLanePosR);
                tRight.transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(0f, 90f, 0f);
                tRight.transform.parent = tObj.transform;
                tRight.transform.name = "LightRight";
                if (bScale) { tRight.transform.localScale = tScale; }

                cCount = tRight.transform.childCount;
                for (int i = 0; i < cCount; i++)
                {
                    if (tRight.transform.GetChild(i).name.ToLower() == "lights")
                    {
                        MR_Right = tRight.transform.GetChild(i).GetComponent<MeshRenderer>();
                    }
                }
            }
            GameObject[] tLanes = new GameObject[LanesHalf];
            for (int i = 0; i < LanesHalf; i++)
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/RoadArchitect/Mesh/RoadObj/Signs/GSDTrafficLightMain.prefab", typeof(GameObject));
                tLanes[i] = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                tLanes[i].transform.position = tObj.transform.TransformPoint(tLanePos[i]);
                tLanes[i].transform.rotation = Quaternion.LookRotation(tan) * Quaternion.Euler(0f, 90f, 0f);
                tLanes[i].transform.parent = tObj.transform;
                tLanes[i].transform.name = "Light" + i.ToString();
                if (bScale) { tLanes[i].transform.localScale = tScale; }

                cCount = tLanes[i].transform.childCount;
                for (int j = 0; j < cCount; j++)
                {
                    if (tLanes[i].transform.GetChild(j).name.ToLower() == "lights")
                    {
                        MR_Mains[i] = tLanes[i].transform.GetChild(j).GetComponent<MeshRenderer>();
                    }
                }
            }

            GSDTrafficLightController LM = new GSDTrafficLightController(ref tLeft, ref tRight, ref tLanes, ref MR_Left, ref MR_Right, ref MR_Mains);
            if (tCorner == 0)
            {
                GSDRI.LightsRR = null;
                GSDRI.LightsRR = LM;
            }
            else if (tCorner == 1)
            {
                GSDRI.LightsRL = null;
                GSDRI.LightsRL = LM;
            }
            else if (tCorner == 2)
            {
                GSDRI.LightsLL = null;
                GSDRI.LightsLL = LM;
            }
            else if (tCorner == 3)
            {
                GSDRI.LightsLR = null;
                GSDRI.LightsLR = LM;
            }
        }

    }
}