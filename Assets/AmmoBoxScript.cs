using Unity.Netcode;
using UnityEngine;

public class AmmoBoxScript : BaseLoot
{
    public override void OnLoot(Player player, ClientRpcParams rpcParams)
    {
        player.ReplenishBulletClientRpc(rpcParams);
    }
}
