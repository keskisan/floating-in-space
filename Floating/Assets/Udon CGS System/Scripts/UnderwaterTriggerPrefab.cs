
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UnderwaterTriggerPrefab : UdonSharpBehaviour
{
    public Renderer underwaterRenderer;

    private VRCPlayerApi localPlayer;

    void Start()
    {
        underwaterRenderer.enabled = false;
        localPlayer = Networking.LocalPlayer;
    }

    public void Update()
    {
        //underwaterRenderer.enabled = localPlayer.GetBonePosition(HumanBodyBones.Head).y < -1.0f;
    }
}
 