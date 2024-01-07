
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BasicMovements : UdonSharpBehaviour
{
    public float jumpImpulse = 3.0f;
    public float walkSpeed = 2.0f;
    public float runSpeed = 4.0f;
    public float strafeSpeed = 2.0f;

    void Start()
    {
        Networking.LocalPlayer.SetStrafeSpeed(strafeSpeed);
        Networking.LocalPlayer.SetRunSpeed(runSpeed);
        Networking.LocalPlayer.SetWalkSpeed(walkSpeed);
        Networking.LocalPlayer.SetJumpImpulse(jumpImpulse);
    }
}
