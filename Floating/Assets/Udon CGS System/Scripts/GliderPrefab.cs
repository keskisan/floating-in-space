
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class GliderPrefab : UdonSharpBehaviour
{
    public ClimbingSystemPrefab climbingSystem;
    public PlayerInfoPrefab playerInfo;
    public SwimmingSystemPrefab swimmingSystem;
    public AudioSource glidingSound;

    public float glidingHeight = 0.5f;
    public float glidingFallingRate = 1.0f;

    public float maxGlidingSpeed = 20.0f;

    public float glidingAcceleration = 10.0f;
    public float staminaPerSecond = 5.0f;

    private Vector3 playerVel;

    private VRCPlayerApi localPlayer;
    private bool gliding = false;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        playerVel = Vector3.down * glidingFallingRate;
    }

    private void FixedUpdate()
    {
        if (!localPlayer.IsPlayerGrounded()) playerInfo.CostStamina(0);
        if (!gliding) return;
        if (localPlayer.IsPlayerGrounded() || playerInfo.currentStamina == 0.0f)
        {
            DisableGliding();
            return;
        }

        MoveHorizontal();
        MoveVertical();
        MaxSpeedLimit();

        localPlayer.SetVelocity(playerVel);
        playerInfo.CostStamina(staminaPerSecond * Time.fixedDeltaTime);
        
    }



    private void EnableGliding()
    {
        gliding = true;
        playerVel = localPlayer.GetVelocity();
        playerVel.y = -glidingFallingRate;
        climbingSystem.DisableClimbing();
        localPlayer.SetGravityStrength(0);
        glidingSound.Play();
    }

    public void DisableGliding()
    {
        gliding = false;
        glidingSound.Stop();
        if(!swimmingSystem.underWater)localPlayer.SetGravityStrength(1);
    }

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        if (!args.boolValue) return;

        if (gliding)
        {
            DisableGliding();
            glidingSound.transform.position = localPlayer.GetPosition();
            return;
        }

        if (localPlayer.IsPlayerGrounded()) return;

        Vector3 playerPos = localPlayer.GetPosition();

        if(Physics.Raycast(playerPos, Vector3.down, glidingHeight) || swimmingSystem.underWater){
            return;
        }

        EnableGliding();
    }

    private void MaxSpeedLimit()
    {
        if(playerVel.magnitude > maxGlidingSpeed)
        {
            playerVel = maxGlidingSpeed * playerVel.normalized;
            playerVel.y = -glidingFallingRate;
        }
    }

    private void MoveHorizontal()
    {
        float value = Input.GetAxis("Horizontal");
        

        playerVel += localPlayer.GetRotation() * (value*Time.fixedDeltaTime*glidingAcceleration*Vector3.right);
    }

    private void MoveVertical()
    {
        float value = Input.GetAxis("Vertical");

        playerVel += localPlayer.GetRotation() * (value * Time.fixedDeltaTime * glidingAcceleration * Vector3.forward);
    }
}
