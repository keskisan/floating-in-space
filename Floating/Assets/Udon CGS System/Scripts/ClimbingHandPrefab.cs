
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class ClimbingHandPrefab : UdonSharpBehaviour
{
    public bool isLeft;
    public PlayerInfoPrefab playerInfo;

    private VRCPlayerApi localPlayer;

    public int climbingLayer = 23;

    [HideInInspector] public bool isTouching = false;

    [HideInInspector] public Vector3 prevPos;
    [HideInInspector] public Vector3 currentPos;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;

        isTouching = false;
    }

    public void FixedUpdate()
    {
        prevPos = currentPos;
        Vector3 tempPos;

        if (isLeft) tempPos = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
        else tempPos = localPlayer.GetBonePosition(HumanBodyBones.RightHand);

        
        currentPos = tempPos - localPlayer.GetPosition();

        transform.position = tempPos;
        transform.localScale = playerInfo.playerHeight * Vector3.one;

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other == null || other.gameObject == null) return;

        if(other.gameObject.layer == climbingLayer)
        {
            isTouching = true;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other == null || other.gameObject == null) return;

        if (other.gameObject.layer == climbingLayer)
        {
            isTouching = true;
            Debug.Log("touching...");
        }

    }

    public void OnTriggerExit(Collider other)
    {
        if (other == null || other.gameObject == null) return;

        if (other.gameObject.layer == climbingLayer)
        {
            isTouching = false;
        }
    }
}
