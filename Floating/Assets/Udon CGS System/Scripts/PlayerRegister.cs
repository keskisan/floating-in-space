
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerRegister : UdonSharpBehaviour
{
    public int maxPlayerCount;

    public PlayerInfoPrefab playerInfo;

    [HideInInspector] [UdonSynced] public int[] playerIds;

    [HideInInspector] public int playerId = -1; 

    void Start()
    {
        playerId = -1;

        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            playerIds = new int[maxPlayerCount];

            for (int i = 0; i < maxPlayerCount; i++)
            {
                playerIds[i] = -1;
            }

            playerIds[0] = Networking.LocalPlayer.playerId;
            playerId = 0;
            playerInfo.Register(playerId);
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Networking.LocalPlayer.IsOwner(gameObject)) return;

        //Check if not registered yet...
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (playerIds[i] == player.playerId) return;
        }


        //Register to the first empty slot
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (playerIds[i] == -1)
            {
                playerIds[i] = player.playerId;
                break;
            }
        }
        RequestSerialization();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (playerIds[i] == player.playerId) playerIds[i] = -1;
        }
    }

    public override void OnDeserialization()
    {
        //Get your playerId;

        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (playerIds[i] == Networking.LocalPlayer.playerId) playerId = i;
        }
        playerInfo.Register(playerId);
    }
}
