
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SwimmingSystemPrefab : UdonSharpBehaviour
{
    public GameObject underwaterBackground;
    public SwimmingHandPrefab leftHand;
    public SwimmingHandPrefab rightHand;
    public GliderPrefab glider;
    public PlayerInfoPrefab playerInfo;

    public int swimmingLayer = 24;

    public float staminaCost = 0.5f;
    public float noStaminaPowerMul = 0.1f;
    public float swimmingPower = 0.5f;
    public float underWaterDrag = 0.05f;

    public float hapticFrequency = 0.1f;
    public float hapticAmplitude = 0.5f;

    [HideInInspector] public bool underWater = false;

    private VRCPlayerApi localPlayer;
    
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }


    private void FixedUpdate()
    {
        transform.position = localPlayer.GetBonePosition(HumanBodyBones.Head);

        if (!underWater) return;

        HandForce(leftHand);
        HandForce(rightHand);

        DragForce();
    }

    private void HandForce(SwimmingHandPrefab hand)
    {
        if (!hand.pressed) return;

        Vector3 deltaPos = hand.currentPos - hand.prevPos;
        Vector3 deltaVel =  swimmingPower * deltaPos / Time.fixedDeltaTime;


        if (playerInfo.currentStamina <= 0.0f)
        {
            deltaVel *= noStaminaPowerMul;
        }

        playerInfo.CostStamina(staminaCost*deltaVel.magnitude);

        Vector3 playerVelocity = localPlayer.GetVelocity();
        playerVelocity -= deltaVel;
        localPlayer.SetVelocity(playerVelocity);
    }

    private void DragForce()
    {
        Vector3 playerVelocity = localPlayer.GetVelocity();
        float speed = playerVelocity.magnitude;

        float deceleration = underWaterDrag * speed * Time.fixedDeltaTime;
        float finalSpeed = Mathf.Max(0.0f, speed - deceleration);

        Vector3 finalVelocity = finalSpeed * playerVelocity.normalized;
        localPlayer.SetVelocity(finalVelocity);
    }

    private void EnableSwimming()
    {
        underWater = true;
        underwaterBackground.SetActive(true);
        glider.DisableGliding();
        localPlayer.SetGravityStrength(0);
        localPlayer.Immobilize(true);
    }

    private void DisableSwimming()
    {
        underWater = false;
        underwaterBackground.SetActive(false);
        localPlayer.SetGravityStrength(1);
        localPlayer.Immobilize(false);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other == null || other.gameObject == null) return;

        if(other.gameObject.layer == swimmingLayer)
        {
            EnableSwimming();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other == null || other.gameObject == null) return;

        if (other.gameObject.layer == swimmingLayer)
        {
            DisableSwimming();
        }
    }

}
