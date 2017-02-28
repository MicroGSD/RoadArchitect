using UnityEngine;

public class GSDRigidBody : MonoBehaviour{
	public float MinCollVelocity = 2f;
//	bool bIsForcedSleeping = false;
	Rigidbody RB;
//	bool bIgnoreRB = false;
	
	void Awake(){
		RB = transform.GetComponent<Rigidbody>();
        if (RB != null) {
            DestroyImmediate(RB);
        }
	}
	
//	void OnCollisionEnter(Collision collision) {
//		if(bIgnoreRB || !bIsForcedSleeping){ return; }
//		Debug.Log (collision.relativeVelocity.magnitude);
//		if(RB != null){
//	        if(collision.relativeVelocity.magnitude <= MinCollVelocity){ 
//				RB.Sleep();
//			}else{
//				//RB.isKinematic = false;
//				bIsForcedSleeping = false;
//				//RB.AddForce(collision.relativeVelocity*collision.relativeVelocity.magnitude*(RB.mass*0.3f));
//			}
//		}
//	}
//	
//	void OnCollisionExit(Collision collisionInfo) {
//		if(bIgnoreRB || !bIsForcedSleeping){ return; }
//       	if(bIsForcedSleeping && RB != null){
//			RB.Sleep();	
//		}
//    }
//	
//	float TimerMax = 0.1f;
//	float TimerNow = 0f;
//	void Update(){
//		if(bIsForcedSleeping){
//			TimerNow += Time.deltaTime;
//			if(TimerNow > TimerMax){
//				if(RB != null && !RB.IsSleeping()){
//					RB.Sleep();	
//				}
//				TimerNow = 0f;
//			}
//		}
//	}
}