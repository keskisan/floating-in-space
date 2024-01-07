
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ChairController : UdonSharpBehaviour
{
    public ClimbingScript[] chairs;

    private VRCPlayerApi localPlayer;
    ClimbingScript _localPlayerChair = null;

    public ClimbingScript LocalPlayerChair
    {
        get
        {
            return _localPlayerChair;
        }
    }

    void Start()
    {
        localPlayer = Networking.LocalPlayer;

        AssignePlayersAChair();
    }


    private void AssignePlayersAChair()
    {
        if (Networking.IsMaster)
        {
            _localPlayerChair = chairs[0];
            return;
        }
        else
        {
            for (int i = 1; i < chairs.Length; i++) //all capsules except 0 that belongs to master that is unused
            {
                if (Networking.GetOwner(chairs[i].gameObject).isMaster)
                {
                    _localPlayerChair = chairs[i];
                    Networking.SetOwner(localPlayer, chairs[i].gameObject);
                    return;
                }
            }
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player) //master always has capsule 0 so new masters loose thier capsule
    {
        if (localPlayer.isMaster)
        {
            if (_localPlayerChair != chairs[0])
            {
                _localPlayerChair.SetOwnerPlayer();
                _localPlayerChair = chairs[0];
            }
        }
    }

    public bool IsWingsAssigned(ClimbingScript chair)
    {
        if (chair == chairs[0]) return true; //master
        else if (Networking.GetOwner(chair.gameObject).isMaster) return false; //object not owned by anyone
        else return true; //is owned by someone
    }

    public void FloatInSpace()
    {
        if (_localPlayerChair != null)
        {
            _localPlayerChair.FloatInSpace();
        }
    }
}
