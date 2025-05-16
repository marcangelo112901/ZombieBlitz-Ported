using Unity.Netcode;
using UnityEngine;

public abstract class BaseLoot : NetworkBehaviour
{
    private NetworkObject networkObject;

    public override void OnNetworkSpawn()
    {
        
    }

    public virtual void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    public override void OnNetworkDespawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsServer || !IsSpawned) return;

        collision.gameObject.TryGetComponent(out Player player);

        if (player == null) return;

        ulong targetClientId = player.OwnerClientId;

        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetClientId }
            }
        };

        OnLoot(player, rpcParams);
        networkObject.Despawn(true);
    }

    public abstract void OnLoot(Player player, ClientRpcParams rpcParams);

}
