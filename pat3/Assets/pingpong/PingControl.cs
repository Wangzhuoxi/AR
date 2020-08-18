using UnityEngine;

public class PingControl : MonoBehaviour
{
    public GameObject ball; // Called by GazeGestureManager when the user performs a Select gesture
    public void OnSelect()
    {
        // If the sphere has no Rigidbody component, add one to enable physics.
       
            ball.GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, 0.0f);
        transform.localPosition = new Vector3(0.0f, 0.15f, 0.0f);

    }
}