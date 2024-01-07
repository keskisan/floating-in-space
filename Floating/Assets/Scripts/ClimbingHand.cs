
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ClimbingHand : UdonSharpBehaviour
{
    public bool isLeft;

    private VRCPlayerApi localPlayer;

    [SerializeField]
    LayerMask layerMask;

    [SerializeField]
    float sphereCheckRadius = 0.5f;

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
        if (Physics.CheckSphere(transform.position, sphereCheckRadius, layerMask))
        {
            isTouching = true;
        }
        else
        {
            isTouching = false;
        }

        prevPos = currentPos;
        Vector3 tempPos;

        if (isLeft) tempPos = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
        else tempPos = localPlayer.GetBonePosition(HumanBodyBones.RightHand);


        currentPos = tempPos - localPlayer.GetPosition();

        transform.position = tempPos;
    }

   
}
