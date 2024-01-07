
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class ClimbingSystemPrefab : UdonSharpBehaviour
{
    public ClimbingHandPrefab leftHand;
    public ClimbingHandPrefab rightHand;
    public PlayerInfoPrefab playerInfo;
    public GliderPrefab glider;
    public SwimmingSystemPrefab swimmingSystem;

    public AudioSource climbingSource;

    public float staminaCost = 10.0f;

    public float hapticDuration = 0.5f;
    public float hapticAmplitude = 0.2f;
    public float hapticFrequency = 2.0f;

    private VRCPlayerApi localPlayer;
    private ClimbingHandPrefab currentClimbingHand;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        currentClimbingHand = null;
    }

    void FixedUpdate()
    {
        if (currentClimbingHand == null || playerInfo.climbingStation == null)
        {
            return;
        }


        //NEW CODE
       /* if (!playerInfo.climbingStation.seated)
        {
            playerInfo.climbingStation.transform.position = localPlayer.GetPosition();
            playerInfo.climbingStation.transform.rotation = localPlayer.GetRotation();
        }*/


        if (playerInfo.currentStamina <= 0.0f)
        {
            DisableClimbing();
            return;
        }

        playerInfo.climbingCollider.center = Vector3.up * playerInfo.playerHeight;

        Vector3 deltaVec = currentClimbingHand.currentPos - currentClimbingHand.prevPos;

        playerInfo.CostStamina(deltaVec.magnitude * staminaCost);


        Vector3 handVelocity = deltaVec/Time.fixedDeltaTime;

        playerInfo.climbingRd.velocity = -handVelocity;
    }

    private void EnableClimbing()
    {
        glider.DisableGliding();

        playerInfo.climbingStation.transform.position = localPlayer.GetPosition();
        playerInfo.climbingStation.transform.rotation = localPlayer.GetRotation();
        playerInfo.climbingStation.UseStation(localPlayer);
    }

    public void DisableClimbing()
    {
        if (currentClimbingHand == null) return;
        currentClimbingHand = null;

        playerInfo.climbingStation.ExitStation(localPlayer);
        localPlayer.SetVelocity(playerInfo.climbingRd.velocity);
        playerInfo.climbingRd.velocity = Vector3.zero;
        playerInfo.climbingRd.transform.position = 30 * Vector3.down;
        if(!swimmingSystem.underWater)localPlayer.Immobilize(false);
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        if (playerInfo.climbingStation == null) return;

        bool leftHandClick = (args.handType == HandType.LEFT);

        if (value)
        {
            if (leftHandClick)
            {
                if (leftHand.isTouching)
                {
                    currentClimbingHand = leftHand;
                    localPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, hapticDuration, hapticAmplitude, hapticFrequency);
                    climbingSource.transform.position = leftHand.transform.position;
                    climbingSource.Play();
                    EnableClimbing();
                }
                
            }
            else
            {
                if (rightHand.isTouching)
                {
                    currentClimbingHand = rightHand;
                    localPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, hapticDuration, hapticAmplitude, hapticFrequency);
                    climbingSource.transform.position = rightHand.transform.position;
                    climbingSource.Play();
                    EnableClimbing();
                }
            }
        }

        else
        {
            if (leftHandClick)
            {
                if (currentClimbingHand == leftHand)
                {
                    DisableClimbing();
                }
            }
            else
            {
                if (currentClimbingHand == rightHand)
                {
                    DisableClimbing();
                }
            }
        }

    }
}
