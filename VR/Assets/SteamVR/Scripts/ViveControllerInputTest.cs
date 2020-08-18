using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInputTest : MonoBehaviour {

    // Use this for initialization
    private SteamVR_TrackedObject trackedObj;
    // 2
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    // Update is called once per frame
    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();

    }
    void Update () {
        if (Controller.GetHairTriggerDown())
        {
            Debug.Log(gameObject.name + " Trigger Press");
        }
        if (Controller.GetHairTriggerUp())
        {
            Debug.Log(gameObject.name + " Trigger Release");
        }


    }
}
