using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
#endif
namespace GSD.Roads {
#if UNITY_EDITOR
    // Proper automation flow:
    // 1. Make sure opt_bAllowRoadUpdates in the scene's GSDRoadSystem is set to FALSE.
    // 2. Create your roads programmatically via CreateRoad_Programmatically (pass it the road, and then the points in a list)
    //      a. Optionally you can do it via CreateNode_Programmatically and InsertNode_Programmatically
    // 3. Call CreateIntersections_ProgrammaticallyForRoad for each road to create intersections automatically at intersection points.
    // 4. Set opt_bAllowRoadUpdates in the scene's GSDRoadSystem is set to TRUE.
    // 5. Call GSDRoadSystem.UpdateAllRoads();
    // 6. Call GSDRoadSystem.UpdateAllRoads(); after step #5 completes.
    //
    // See "GSDUnitTests.cs" for an example on automation (ignore unit test #3).


    public static class GSDRoadAutomation{
        /// <summary>
        /// Use this to create nodes via coding while in editor mode. Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates.
        /// </summary>
        /// <param name="RS">The road system to create nodes on.</param>
        /// <param name="NodeLocation">The location of the newly created node.</param>
        /// <returns></returns>
        public static GSDRoad CreateRoad_Programmatically(GSDRoadSystem GSDRS, ref List<Vector3> tLocs) {
            GameObject tRoadObj = GSDRS.AddRoad(false);
            GSDRoad tRoad = tRoadObj.GetComponent<GSDRoad>();

            int hCount = tLocs.Count;
            for (int i = 0; i < hCount; i++) {
                CreateNode_Programmatically(tRoad, tLocs[i]);
            }

            return tRoad;
        }


        /// <summary>
        /// Use this to create nodes via coding while in editor mode. Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates.
        /// </summary>
        /// <param name="RS">The road system to create nodes on.</param>
        /// <param name="NodeLocation">The location of the newly created node.</param>
        /// <returns></returns>
        public static GSDSplineN CreateNode_Programmatically(GSDRoad tRoad, Vector3 NodeLocation) {
            int SplineChildCount = tRoad.GSDSpline.transform.childCount;
			GameObject tNodeObj = new GameObject("Node" + (SplineChildCount+1).ToString());
			GSDSplineN tNode = tNodeObj.AddComponent<GSDSplineN>(); //Add the node component.

            //Set node location:
            if (NodeLocation.y < 0.03f) { NodeLocation.y = 0.03f; }     //Make sure it doesn't try to create a node below 0 height.
            tNodeObj.transform.position = NodeLocation;

            //Set the node's parent:
            tNodeObj.transform.parent = tRoad.GSDSplineObj.transform;     

            //Set the idOnSpline:
            tNode.idOnSpline = (SplineChildCount + 1);
            tNode.GSDSpline = tRoad.GSDSpline;

            //Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates
            tRoad.UpdateRoad();

            return tNode;
		}

        /// <summary>
        /// Use this to insert nodes via coding while in editor mode. Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates.
        /// </summary>
        /// <param name="RS">The road system to insert nodes in.</param>
        /// <param name="NodeLocation">The location of the newly inserted node.</param>
        /// <returns></returns>
		public static GSDSplineN InsertNode_Programmatically(GSDRoad RS, Vector3 NodeLocation){
			GameObject tNodeObj;
			Object[] tWorldNodeCount = GameObject.FindObjectsOfType(typeof(GSDSplineN));
            tNodeObj = new GameObject("Node" + tWorldNodeCount.Length.ToString());	
			
            //Set node location:
            if (NodeLocation.y < 0.03f) { NodeLocation.y = 0.03f; }     //Make sure it doesn't try to create a node below 0 height.
            tNodeObj.transform.position = NodeLocation;

            //Set the node's parent:
            tNodeObj.transform.parent = RS.GSDSplineObj.transform;    
			
			int cCount = RS.GSDSpline.mNodes.Count;

            //Get the closet param on spline:
			float tParam = RS.GSDSpline.GetClosestParam(NodeLocation, false, true);
			
			bool bEndInsert = false;
			bool bZeroInsert = false;
			int iStart = 0;
			if(GSDRootUtil.IsApproximately(tParam,0f,0.0001f)){
				bZeroInsert = true;
				iStart = 0;
            } else if (GSDRootUtil.IsApproximately(tParam, 1f, 0.0001f)) {
                //Inserted at end, switch to create node instead:
                Object.DestroyImmediate(tNodeObj);
                return CreateNode_Programmatically(RS, NodeLocation);
			}
			
            //Figure out where to insert the node:
			for(int i=0;i<cCount;i++){
				GSDSplineN xNode = RS.GSDSpline.mNodes[i];
				if(!bZeroInsert && !bEndInsert){
					if(tParam > xNode.tTime){
						iStart = xNode.idOnSpline+1;
					}
				}
			}
			for(int i=iStart;i<cCount;i++){
				RS.GSDSpline.mNodes[i].idOnSpline+=1;
			}
			
			GSDSplineN tNode = tNodeObj.AddComponent<GSDSplineN>();
			tNode.GSDSpline = RS.GSDSpline;
			tNode.idOnSpline = iStart;
            tNode.pos = NodeLocation;
			RS.GSDSpline.mNodes.Insert(iStart,tNode);

            //Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates
            RS.UpdateRoad();
			
			return tNode;
		}


        /// <summary>
        /// Creates intersections where this road intersects with other roads.
        /// </summary>
        /// <param name="tRoad">The primary road to create intersections for.</param>
        /// <param name="iDefaultIntersectionType">Stop signs, traffic lights #1 (US) or traffic lights #2 (Euro). Defaults to none.</param>
        /// <param name="rType">Intersection type: No turn lane, left turn lane or both turn lanes. Defaults to no turn lane.</param>
        public static void CreateIntersections_ProgrammaticallyForRoad(GSDRoad tRoad, GSDRoadIntersection.iIntersectionTypeEnum iDefaultIntersectionType = GSDRoadIntersection.iIntersectionTypeEnum.None, GSDRoadIntersection.RoadTypeEnum rType = GSDRoadIntersection.RoadTypeEnum.NoTurnLane) {
            /*
            General logic:
             20m increments to gather collection of which roads intersect
                2m increments to find actual intersection point
                each 2m, primary road checks all intersecting array for an intersection.
             find intersection point
                if any intersections already within 75m or 100m, dont create intersection here
                check if nodes within 50m, if more than one just grab closest, and move  it to intersecting point
                if no node within 50m, add
             create intersection with above two nodes
            */

            Object[] GSDRoadObjs = Object.FindObjectsOfType<GSDRoad>();

            //20m increments to gather collection of which roads intersect
            List<GSDRoad> xRoads = new List<GSDRoad>();
            foreach (GSDRoad xRoad in GSDRoadObjs) {
                if (tRoad != xRoad) {
                    float EarlyDistanceCheckMeters = 10f;
                    float EarlyDistanceCheckThreshold = 50f;
                    bool EarlyDistanceFound = false;
                    float tRoadMod = EarlyDistanceCheckMeters / tRoad.GSDSpline.distance;
                    float xRoadMod = EarlyDistanceCheckMeters / xRoad.GSDSpline.distance;
                    Vector3 tVect1 = default(Vector3);
                    Vector3 tVect2 = default(Vector3);
                    for (float i = 0f; i < 1.0000001f; i += tRoadMod) {
                        tVect1 = tRoad.GSDSpline.GetSplineValue(i);
                        for (float x = 0f; x < 1.000001f; x += xRoadMod) {
                            tVect2 = xRoad.GSDSpline.GetSplineValue(x);
                            if (Vector3.Distance(tVect1, tVect2) < EarlyDistanceCheckThreshold) {
                                if (!xRoads.Contains(xRoad)) {
                                    xRoads.Add(xRoad);
                                }
                                EarlyDistanceFound = true;
                                break;
                            }
                        }
                        if (EarlyDistanceFound) { break; }
                    }
                }
            }

            //See if any end point nodes are on top of each other already since T might not intersect all the time.:
            List<KeyValuePair<GSDSplineN, GSDSplineN>> tKVP = new List<KeyValuePair<GSDSplineN, GSDSplineN>>();
            foreach (GSDRoad xRoad in xRoads) {
                foreach (GSDSplineN IntersectionNode1 in tRoad.GSDSpline.mNodes) {
                    if (IntersectionNode1.bIsIntersection || !IntersectionNode1.IsLegitimate()) { continue; }
                    foreach (GSDSplineN IntersectionNode2 in xRoad.GSDSpline.mNodes) {
                        if (IntersectionNode2.bIsIntersection || !IntersectionNode2.IsLegitimate()) { continue; }
                        if (IntersectionNode1.transform.position == IntersectionNode2.transform.position) {
                            //Only do T intersections and let the next algorithm handle the +, since T might not intersect all the time.
                            if (IntersectionNode1.bIsEndPoint || IntersectionNode2.bIsEndPoint) {
                                tKVP.Add(new KeyValuePair<GSDSplineN, GSDSplineN>(IntersectionNode1, IntersectionNode2));
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<GSDSplineN, GSDSplineN> KVP in tKVP) {
                //Now create the fucking intersection:
                GameObject tInter = GSD.Roads.GSDIntersections.CreateIntersection(KVP.Key, KVP.Value);
                GSDRoadIntersection GSDRI_JustCreated = tInter.GetComponent<GSDRoadIntersection>();
                GSDRI_JustCreated.iDefaultIntersectionType = iDefaultIntersectionType;
                GSDRI_JustCreated.rType = rType;
            }

            //Main algorithm: 2m increments to find actual intersection point:
            foreach (GSDRoad xRoad in xRoads) {
                if (tRoad != xRoad) {
                    //Debug.Log("Checking road: " + xRoad.transform.name);
                    float DistanceCheckMeters = 2f;
                    bool EarlyDistanceFound = false;
                    float tRoadMod = DistanceCheckMeters / tRoad.GSDSpline.distance;
                    float xRoadMod = DistanceCheckMeters / xRoad.GSDSpline.distance;
                    Vector3 tVect = default(Vector3);
                    Vector2 iVect1 = default(Vector2);
                    Vector2 iVect2 = default(Vector2);
                    Vector2 xVect1 = default(Vector2);
                    Vector2 xVect2 = default(Vector2);
                    Vector2 IntersectPoint2D = default(Vector2);
                    float i2 = 0f;
                    for (float i = 0f; i < 1.0000001f; i += tRoadMod) {
                        i2 = (i + tRoadMod);
                        if (i2 > 1f) { i2 = 1f; }
                        tVect = tRoad.GSDSpline.GetSplineValue(i);
                        iVect1 = new Vector2(tVect.x, tVect.z);
                        tVect = tRoad.GSDSpline.GetSplineValue(i2);
                        iVect2 = new Vector2(tVect.x, tVect.z);

                        float x2 = 0f;
                        for (float x = 0f; x < 1.000001f; x += xRoadMod) {
                            x2 = (x + xRoadMod);
                            if (x2 > 1f) { x2 = 1f; }
                            tVect = xRoad.GSDSpline.GetSplineValue(x);
                            xVect1 = new Vector2(tVect.x, tVect.z);
                            tVect = xRoad.GSDSpline.GetSplineValue(x2);
                            xVect2 = new Vector2(tVect.x, tVect.z);

                            //Now see if these two lines intersect:
                            if (GSD.GSDRootUtil.Intersects2D(ref iVect1,ref iVect2,ref xVect1,ref xVect2, out IntersectPoint2D)) {
                                //Get height of intersection on primary road:
                                float tHeight = 0f;
                                float hParam = tRoad.GSDSpline.GetClosestParam(new Vector3(IntersectPoint2D.x, 0f, IntersectPoint2D.y));
                                Vector3 hVect = tRoad.GSDSpline.GetSplineValue(hParam);
                                tHeight = hVect.y;

                                //if any intersections already within 75m or 100m, dont create intersection here
                                Object[] AllInterectionObjects = Object.FindObjectsOfType<GSDRoadIntersection>();
                                foreach (GSDRoadIntersection GSDRI in AllInterectionObjects) {
                                    if (Vector2.Distance(new Vector2(GSDRI.transform.position.x, GSDRI.transform.position.z), IntersectPoint2D) < 100f) {
                                        goto NoIntersectionCreation;
                                    }
                                }

                                GSDSplineN IntersectionNode1 = null;
                                GSDSplineN IntersectionNode2 = null;
                                Vector3 IntersectionPoint3D = new Vector3(IntersectPoint2D.x, tHeight, IntersectPoint2D.y);
                                //Debug.Log("Instersect found road: " + xRoad.transform.name + " at point: " + IntersectionPoint3D.ToString());

                                //Check primary road if any nodes are nearby and usable for intersection
                                foreach(GSDSplineN tNode in tRoad.GSDSpline.mNodes){
                                    if (tNode.IsLegitimate()) {
                                        if (Vector2.Distance(new Vector2(tNode.transform.position.x, tNode.transform.position.z), IntersectPoint2D) < 30f) {
                                            IntersectionNode1 = tNode;
                                            IntersectionNode1.transform.position = IntersectionPoint3D;
                                            IntersectionNode1.pos = IntersectionPoint3D;
                                            break;
                                        }
                                    }
                                }

                                //Check secondary road if any nodes are nearby and usable for intersection
                                foreach (GSDSplineN tNode in xRoad.GSDSpline.mNodes) {
                                    if (tNode.IsLegitimate()) {
                                        if (Vector2.Distance(new Vector2(tNode.transform.position.x, tNode.transform.position.z), IntersectPoint2D) < 30f) {
                                            IntersectionNode2 = tNode;
                                            IntersectionNode2.transform.position = IntersectionPoint3D;
                                            IntersectionNode2.pos = IntersectionPoint3D;
                                            break;
                                        }
                                    }
                                }

                                //Check if any of the nodes are null. If so, need to insert node. And maybe update it.
                                if (IntersectionNode1 == null) {
                                    IntersectionNode1 = InsertNode_Programmatically(tRoad, IntersectionPoint3D);
                                }
                                if (IntersectionNode2 == null) {
                                    IntersectionNode2 = InsertNode_Programmatically(xRoad, IntersectionPoint3D);
                                }

                                //Now create the fucking intersection:
                                GameObject tInter = GSD.Roads.GSDIntersections.CreateIntersection(IntersectionNode1, IntersectionNode2);
                                GSDRoadIntersection GSDRI_JustCreated = tInter.GetComponent<GSDRoadIntersection>();
                                GSDRI_JustCreated.iDefaultIntersectionType = iDefaultIntersectionType;
                                GSDRI_JustCreated.rType = rType;
                            }

                        NoIntersectionCreation:
                            //Gibberish to get rid of warnings:
                            int xxx = 1;
                            if (xxx == 1) { xxx = 2; }
                        }
                        if (EarlyDistanceFound) { break; }
                    }
                }
            }
        }
    }
#endif
}
