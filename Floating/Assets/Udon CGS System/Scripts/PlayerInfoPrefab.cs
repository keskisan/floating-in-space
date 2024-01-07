
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerInfoPrefab : UdonSharpBehaviour
{
    private VRCPlayerApi localPlayer;

    const float SLOW_UPDATE_RATE = 0.5f;
    private float last_slow_update = 0;

    [HideInInspector] public float playerHeight;

    public float maxStamina = 100.0f;
    [HideInInspector] public float currentStamina = 100.0f;

    public float regenStaminaTime = 5.0f;
    [HideInInspector] private float currentRegenStaminaTime = 0.0f;
    public float regenStaminaSpeed = 20.0f;

    public GameObject staminaBar;
    public Text staminaText;

    public Transform staminaBarRotCenter;
    public float staminaBarHeight = 0.8f;

    [HideInInspector] public VRCStation climbingStation;
    [HideInInspector] public Rigidbody climbingRd;
    [HideInInspector] public CapsuleCollider climbingCollider;

    void Start()
    {
        climbingStation = null;
        localPlayer = Networking.LocalPlayer;
    }


    private void Update()
    {
        staminaBarRotCenter.position = Networking.LocalPlayer.GetPosition() + playerHeight * staminaBarHeight * Vector3.up;
        staminaBarRotCenter.rotation = Networking.LocalPlayer.GetRotation();

        if (Time.time - last_slow_update > SLOW_UPDATE_RATE)
        {
            last_slow_update = Time.time;
            SlowUpdate();
        }

        StaminaUpdate();
    }

    public void Register(int playerId)
    {
        if (playerId < 0) return;

        climbingStation = (VRCStation)GameObject.Find("ClimbingStation (" + playerId + ")").GetComponent(typeof(VRCStation));
        climbingRd = (Rigidbody)climbingStation.gameObject.GetComponent(typeof(Rigidbody));
        climbingCollider = (CapsuleCollider)climbingStation.gameObject.GetComponent(typeof(CapsuleCollider));
        Networking.SetOwner(localPlayer, climbingStation.gameObject);

        Debug.Log("Register + " + localPlayer.displayName + " -> " + climbingStation.name);
    }

    private void SlowUpdate()
    {
        playerHeight = GetAvatarHeight(localPlayer);
    }

    private float GetAvatarHeight(VRCPlayerApi player)
    {
        float height = 0;
        Vector3 postition1 = player.GetBonePosition(HumanBodyBones.Head);
        Vector3 postition2 = player.GetBonePosition(HumanBodyBones.Neck);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.Hips);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.RightFoot);
        height += (postition1 - postition2).magnitude;
        return height;
    }

    public void StaminaUpdate()
    {
        if(currentRegenStaminaTime <= 0)
        {
            currentStamina += regenStaminaSpeed * Time.deltaTime;
        }
        currentRegenStaminaTime -= Time.deltaTime;
        UpdateStaminaDisplay();
    }

    public void CostStamina(float amount)
    {
       currentStamina -= amount;
       currentRegenStaminaTime = regenStaminaTime;
    }

    public void UpdateStaminaDisplay()
    {
        if (currentStamina < 0) currentStamina = 0;
        if (currentStamina > maxStamina) currentStamina = maxStamina;

        staminaBar.transform.localScale = new Vector3(currentStamina / maxStamina, 1, 1);
        staminaText.text = ((int)(currentStamina)) + "/" + ((int)maxStamina);
    }

}
