
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PalmMenu : UdonSharpBehaviour
{
    [SerializeField]
    ChairController chairController;

    VRCPlayerApi localPlayer;
    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    private void Update()
    {
        transform.position = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
        transform.rotation = localPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
    }

    public void SpawnRocket()
    {
        if (chairController.LocalPlayerChair != null)
        {
            chairController.LocalPlayerChair.rocket.SpawnRocketinHand();
        }  
    }
}
