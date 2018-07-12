using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Xml; 
using System.Xml.Serialization; 
using System.IO; 
using System.Text;
#endif
namespace GSD{
	#if UNITY_EDITOR
	public static class GSDRootUtil{
		/// <summary>
		/// Smooths the input parameter t.
		/// If less than k1 ir greater than k2, it uses a sin.
		/// Between k1 and k2 it uses linear interp.
		/// </summary>
		public static float Ease (float t, float k1, float k2){
			float f;
			float s;
	
			f = k1 * 2 / Mathf.PI + k2 - k1 + (1.0f - k2) * 2 / Mathf.PI;
	
			if (t < k1) {
				s = k1 * (2 / Mathf.PI) * (Mathf.Sin ((t / k1) * Mathf.PI / 2 - Mathf.PI / 2) + 1);
			} else if (t < k2) {
				s = (2 * k1 / Mathf.PI + t - k1);
			} else {
				s = 2 * k1 / Mathf.PI + k2 - k1 + ((1 - k2) * (2 / Mathf.PI)) * Mathf.Sin (((t - k2) / (1.0f - k2)) * Mathf.PI / 2);
			}
	
			return (s / f);
		}

        /// <summary>
        /// Returns true if the lines intersect, otherwise false. 
        /// </summary>
        /// <param name="Line1S">Line 1 start.</param>
        /// <param name="Line1E">Line 1 end.</param>
        /// <param name="Line2S">Line 2 start.</param>
        /// <param name="Line2E">Line 2 end.</param>
        /// <param name="intersectionPoint">If the lines intersect, intersectionPoint holds the intersection point.</param>
        /// <returns></returns>
        public static bool Intersects2D(ref Vector2 Line1S, ref Vector2 Line1E, ref Vector2 Line2S, ref Vector2 Line2E, out Vector2 intersectionPoint) {
            float firstLineSlopeX, firstLineSlopeY, secondLineSlopeX, secondLineSlopeY;

            firstLineSlopeX = Line1E.x - Line1S.x;
            firstLineSlopeY = Line1E.y - Line1S.y;

            secondLineSlopeX = Line2E.x - Line2S.x;
            secondLineSlopeY = Line2E.y - Line2S.y;

            float s, t;
            s = (-firstLineSlopeY * (Line1S.x - Line2S.x) + firstLineSlopeX * (Line1S.y - Line2S.y)) / (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);
            t = (secondLineSlopeX * (Line1S.y - Line2S.y) - secondLineSlopeY * (Line1S.x - Line2S.x)) / (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1) {
                float intersectionPointX = Line1S.x + (t * firstLineSlopeX);
                float intersectionPointY = Line1S.y + (t * firstLineSlopeY);

                // Collision detected
                intersectionPoint = new Vector2(intersectionPointX, intersectionPointY);
                return true;
            }

            intersectionPoint = Vector2.zero;
            return false; // No collision
        }
			
		
		public static string GetPrefabString(GameObject tObj){
			string tString = "";
			#if UNITY_EDITOR
			if(tObj != null){
				tString = UnityEditor.AssetDatabase.GetAssetPath( tObj );
				if(tString == null || tString.Length < 1){
#if UNITY_2018_2_OR_NEWER
                    Object parentObject = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(tObj); 
#else
                    Object parentObject = UnityEditor.PrefabUtility.GetPrefabParent(tObj);
#endif
                    tString = UnityEditor.AssetDatabase.GetAssetPath(parentObject);
				}
			}
#endif
                    return tString;
		}
		
		#region "Float comparisons"
		public static bool IsApproximately (float a, float b){
			return IsApproximately (a, b, 0.01f);
		}
		     
		public static bool IsApproximately (float a, float b, float tolerance){
			return Mathf.Abs (a - b) < tolerance;
		}
		#endregion
		
		#region "XML"
		public static void CreateXML<T>(ref string tPath, object pObject){ 
			#if UNITY_WEBPLAYER
			return;
			#else
			string tData = SerializeObject<T>(ref pObject);
			StreamWriter writer; 
			FileInfo t = new FileInfo (tPath); 
			if(!t.Exists){ 
				writer = t.CreateText (); 
			}else{ 
				t.Delete(); 
				writer = t.CreateText (); 
			} 
			writer.Write(tData); 
			writer.Close(); 
			#endif	
		}
		
		public static string GetString<T>(object pObject){
			string tData = SerializeObject<T>(ref pObject);
			return tData;
		}
		 
		public static object LoadXML<T>(ref string tPath){ 
			#if UNITY_WEBPLAYER
			return null;
			#else
			StreamReader r = File.OpenText (tPath); 
			string _info = r.ReadToEnd (); 
			r.Close ();  
			object tObject = DeserializeObject<T>(_info);
			return tObject; 
			#endif
		} 
		
		public static object LoadData<T>(ref string _info){ 
			object tObject = DeserializeObject<T>(_info);
			return tObject; 
		} 
		
		public static void DeleteLibraryXML(string tName, bool bIsExtrusion){
			#if UNITY_WEBPLAYER
			return;
			#else
			string tPath;
			if(bIsExtrusion){
				tPath = Application.dataPath + "/RoadArchitect/Library/ESO" + tName + ".gsd";
			}else{
				tPath = Application.dataPath + "/RoadArchitect/Library/EOM" + tName + ".gsd";
			}
			if(File.Exists(tPath)){
				File.Delete(tPath);	
			}
			#endif
		}
		
		private static string SerializeObject<T>(ref object pObject){ 
			string XmlizedString = null; 
			MemoryStream memoryStream = new MemoryStream ();
			XmlSerializer xs = new XmlSerializer(typeof(T)); 
			XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8); 
			xs.Serialize (xmlTextWriter, pObject); 
			memoryStream = (MemoryStream)xmlTextWriter.BaseStream; 
			XmlizedString = UTF8ByteArrayToString (memoryStream.ToArray ()); 
			return XmlizedString;
		} 
		
		private static object DeserializeObject<T>(string pXmlizedString){ 
			XmlSerializer xs = new XmlSerializer (typeof(T)); 
			MemoryStream memoryStream = new MemoryStream (StringToUTF8ByteArray (pXmlizedString)); 
			return xs.Deserialize (memoryStream); 
		} 
		
		private static string UTF8ByteArrayToString (byte[] characters){      
			UTF8Encoding encoding = new UTF8Encoding (); 
			string constructedString = encoding.GetString (characters); 
			return (constructedString); 
		}
		 
		private static byte[] StringToUTF8ByteArray (string pXmlString){ 
			UTF8Encoding encoding = new UTF8Encoding (); 
			byte[] byteArray = encoding.GetBytes (pXmlString); 
			return byteArray; 
		} 
		#endregion
		
		#region "Mesh tangents"
		//Thread safe because local scope and by val params
		public static Vector4[] ProcessTangents(int[] tris, Vector3[] normals, Vector2[] uvs, Vector3[] verts){
			int MVL = verts.Length;
			if(MVL == 0){ return new Vector4[0]; }
			int triangleCount = tris.Length;// mesh.triangles.Length / 3;
			Vector3[] tan1 = new Vector3[MVL];
			Vector3[] tan2 = new Vector3[MVL];
			Vector4[] tangents = new Vector4[MVL];
			int i1,i2,i3;
			Vector3 v1,v2,v3;
			Vector2 w1,w2,w3;
			float x1,x2,y1,y2,z1,z2,s1,s2,t1,t2,r;
			Vector3 sdir,tdir;
			float div = 0f;
			for(int a = 0; a < triangleCount; a+=3){
				i1 = tris[a+0];
				i2 = tris[a+1];
				i3 = tris[a+2];
				 
				v1 = verts[i1];
				v2 = verts[i2];
				v3 = verts[i3];

				w1 = uvs[i1];
				w2 = uvs[i2];
				w3 = uvs[i3];
				 
				x1 = v2.x - v1.x;
				x2 = v3.x - v1.x;
				y1 = v2.y - v1.y;
				y2 = v3.y - v1.y;
				z1 = v2.z - v1.z;
				z2 = v3.z - v1.z;
				 
				s1 = w2.x - w1.x;
				s2 = w3.x - w1.x;
				t1 = w2.y - w1.y;
				t2 = w3.y - w1.y;
				 
//				r = 1.0f / (s1 * t2 - s2 * t1);
				div = (s1 * t2 - s2 * t1);
				r = div == 0.0f ? 0.0f : 1.0f / div;
				 
				sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
				 
				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;
				 
				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}
			
			Vector3 n,t;//,tmp;
			for (int i=0;i<MVL;i++){
				n = normals[i];
				t = tan1[i];
				
				Vector3.OrthoNormalize(ref n, ref t);
				tangents[i].x = t.x;
				tangents[i].y = t.y;
				tangents[i].z = t.z;
				tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
				
//				tmp = (t - n * Vector3.Dot(n, t)).normalized;
//				tangents[i] = new Vector4(tmp.x, tmp.y, tmp.z);
//				tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
			}
			
			return tangents;
		}
		
		public static void ProcessTangents(ref Mesh tMesh){
			Vector3[] tVerts = tMesh.vertices;
			Vector2[] tUV = tMesh.uv;
			Vector3[] tNormals = tMesh.normals;
			int[] tTris = tMesh.triangles;

			tMesh.tangents = ProcessTangents(tTris,tNormals,tUV,tVerts);
		}	
		#endregion
	
		#region "Default directory for library etc"
		public static string Dir_GetBase(){
			return Application.dataPath.Replace("/Assets","/GSD/");
		}
		public static string Dir_GetTH(){
			string xPath = Dir_GetBase() + "TH/";
			if(!Directory.Exists(xPath)){
				Directory.CreateDirectory(xPath);
			}
			return xPath;
		}
		
		public static string Dir_GetLibraryBase(){
			return Application.dataPath + "/RoadArchitect/Editor/Library/";
		}
		public static string Dir_GetLibrary(){
			string xPath = Dir_GetLibraryBase();
			if(!Directory.Exists(xPath)){
				Directory.CreateDirectory(xPath);
			}
			return xPath;
		}
		
		public static void Dir_GetLibrary_CheckSpecialDirs(){
			string xPath = Dir_GetLibraryBase() + "Q/";
			if(!Directory.Exists(xPath)){
				Directory.CreateDirectory(xPath);
			}
			xPath = Dir_GetLibraryBase() + "W/";
			if(!Directory.Exists(xPath)){
				Directory.CreateDirectory(xPath);
			}
			xPath = Dir_GetLibraryBase() + "B/";
			if(!Directory.Exists(xPath)){
				Directory.CreateDirectory(xPath);
			}
			xPath = Dir_GetLibraryBase() + "B/W/";
			if(!Directory.Exists(xPath)){
				Directory.CreateDirectory(xPath);
			}
		}
		#endregion
		
		public static void ForceCollection(bool bWait = false){
			#if UNITY_EDITOR
			System.GC.Collect();
			if(bWait){
				System.GC.WaitForPendingFinalizers();
			}
			Resources.UnloadUnusedAssets();
			#endif
		}
	}
	#endif
}