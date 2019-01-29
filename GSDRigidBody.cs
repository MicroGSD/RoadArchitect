using UnityEngine;


public class GSDRigidBody : MonoBehaviour
{
    public float MinCollVelocity = 2f;
    //	bool bIsForcedSleeping = false;
    Rigidbody rigidbody;            // Formerly RB // FH 29.01.19
    //	bool bIgnoreRB = false;

    void Awake()
    {
        rigidbody = transform.GetComponent<Rigidbody>();
        if ( rigidbody != null )
        {
            DestroyImmediate( rigidbody );
        }
    }


    /*
    void OnCollisionEnter(Collision collision)
    {
        if ( bIgnoreRB || !bIsForcedSleeping )
        {
            return;
        }
        Debug.Log( collision.relativeVelocity.magnitude );
        if ( rigidbody != null )
        {
            if ( collision.relativeVelocity.magnitude <= MinCollVelocity )
            {
                rigidbody.Sleep();
            }
            else
            {
                //RB.isKinematic = false;
                bIsForcedSleeping = false;
                //RB.AddForce(collision.relativeVelocity*collision.relativeVelocity.magnitude*(RB.mass*0.3f));
            }
        }
    }


    void OnCollisionExit(Collision collisionInfo)
    {
        if ( bIgnoreRB || !bIsForcedSleeping )
        { return; }
        if ( bIsForcedSleeping && rigidbody != null )
        {
            rigidbody.Sleep();
        }
    }


    float TimerMax = 0.1f;
    float TimerNow = 0f;


    void Update()
    {
        if ( bIsForcedSleeping )
        {
            TimerNow += Time.deltaTime;
            if ( TimerNow > TimerMax )
            {
                if ( rigidbody != null && !rigidbody.IsSleeping() )
                {
                    rigidbody.Sleep();
                }
                TimerNow = 0f;
            }
        }
    }
    */
}