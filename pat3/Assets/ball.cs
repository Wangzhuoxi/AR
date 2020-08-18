using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour {

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        // If the sphere has no Rigidbody component, add one to enable physics.
        /*   if (!this.GetComponent<Rigidbody>())
           {
               var rigidbody = this.gameObject.AddComponent<Rigidbody>();
               rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
           }*/
        this.GetComponent<Rigidbody>().useGravity = false;
        this.GetComponent<Rigidbody>().velocity=new Vector3(0,0,0);
        this.transform.position = new Vector3(0, 0, 0);
      


}
}
