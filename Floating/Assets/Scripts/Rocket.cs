
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class Rocket : UdonSharpBehaviour
{
    //[SerializeField]
    //RocketIndicator rocketIndicator;

    [SerializeField]
    ChairController chairController;

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    float velocity = 3f;

    [SerializeField]
    VRCPickup thisPickup;

    [SerializeField, Range(0f, 1f)]
    float rocketRotationInfluence = 0.2f;

    VRCPlayerApi localPlayer;

    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    /*private void Update()
    {
        if (chairController.LocalPlayerChair == null) return;
        Vector3 centreOfMass = chairController.LocalPlayerChair.climbingRd.centerOfMass + chairController.LocalPlayerChair.transform.position;
        //rocketIndicator.DrawLine(transform.position, centreOfMass);
    }*/

    public override void OnPickupUseDown()
    {
        if (chairController.LocalPlayerChair == null) return;
        Vector3 centreOfMass = chairController.LocalPlayerChair.climbingRd.centerOfMass + chairController.LocalPlayerChair.transform.position;
        Vector3 offSetPosition = Vector3.Lerp(transform.position, centreOfMass, rocketRotationInfluence);
        chairController.LocalPlayerChair.AddForceAtPoint(transform.forward * velocity, offSetPosition);
    }

    public void SpawnRocketinHand()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
    }
}