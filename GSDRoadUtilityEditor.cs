using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GSD.Roads{
	#if UNITY_EDITOR
	public static class GSDRoadUtilityEditor{
		public static void SetRoadMaterial(string tPath, MeshRenderer MR, string tPath2 = ""){
			Material tMat2; 
			
			Material[] tMats;
			Material tMat = (Material)AssetDatabase.LoadAssetAtPath(tPath, typeof(Material));
			if(tPath2.Length > 0){
				tMats = new Material[2];
				tMats[0] = tMat;
				tMat2 = (Material)AssetDatabase.LoadAssetAtPath(tPath2, typeof(Material));
				tMats[1] = tMat2;
			}else{
				tMats = new Material[1];
				tMats[0] = tMat;
			}
			
			MR.sharedMaterials = tMats;
		}
		
		public static Material GiveMaterial(string tPath){
			return (Material)AssetDatabase.LoadAssetAtPath(tPath, typeof(Material));
		}
		
		public static PhysicMaterial GivePhysicsMaterial(string tPath){
			return (PhysicMaterial)AssetDatabase.LoadAssetAtPath(tPath, typeof(PhysicMaterial));
		}
	}
	#endif
}