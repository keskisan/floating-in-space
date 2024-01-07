
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class SwimmingHandPrefab : UdonSharpBehaviour
{
    public bool isLeft;
    public PlayerInfoPrefab playerInfo;
    public SwimmingSystemPrefab swimmingSystem;

    private VRCPlayerApi localPlayer;


    [HideInInspector] public Vector3 prevPos;
    [HideInInspector] public Vector3 currentPos;

    [HideInInspector]public bool pressed;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        pressed = false;
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

        if (pressed && swimmingSystem.underWater)
        {
            VRC_Pickup.PickupHand hand = VRC_Pickup.PickupHand.Right;
            if (isLeft) hand = VRC_Pickup.PickupHand.Left;

            localPlayer.PlayHapticEventInHand(hand, Time.fixedDeltaTime, swimmingSystem.hapticAmplitude, swimmingSystem.hapticFrequency);
        }
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        bool leftPressed = args.handType == HandType.LEFT;

        if((isLeft && leftPressed) || (!isLeft && !leftPressed))
        {
            pressed = value;
        }
    }

}
