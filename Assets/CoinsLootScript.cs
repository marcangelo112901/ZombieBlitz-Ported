using Unity.Netcode;
using UnityEngine;

public class NewMonoBehaviourScript : BaseLoot
{
    [SerializeField] private int coinValueMin;
    [SerializeField] private int coinValueMax;

    public override void OnLoot(Player player, ClientRpcParams rpcParams)
    {
        int amount = Random.Range(coinValueMin, coinValueMax + 1);
        player.AddCoinsClientRpc(amount, rpcParams);
    }
}
