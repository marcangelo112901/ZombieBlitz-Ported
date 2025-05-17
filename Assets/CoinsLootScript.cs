using Unity.Netcode;
using UnityEngine;

public class CoinLootScript : BaseLoot
{
    [SerializeField] private int coinValueMin;
    [SerializeField] private int coinValueMax;
    [SerializeField] private float coinShareRatio;
    [SerializeField] private float coinRatioPenaltyPerPlayer;



    public override void OnLoot(Player player, ClientRpcParams rpcParams)
    {
        AddSharedCoinsClientRpc(player.OwnerClientId);
    }

    [ClientRpc]
    private void AddSharedCoinsClientRpc(ulong ownerID)
    {
        if (SystemScript.player.OwnerClientId == ownerID)
        {
            int amount = Random.Range(coinValueMin, coinValueMax + 1);
            SystemScript.player.AddCoins(amount);
        }
        else
        {
            int sharedCoinMin = (int)(coinValueMin * (coinShareRatio - (coinRatioPenaltyPerPlayer * (SystemScript.Instance.players.Count - 2))));
            int sharedCoinMax = (int)(coinValueMax * (coinShareRatio - (coinRatioPenaltyPerPlayer * (SystemScript.Instance.players.Count - 2))));
            int amount = Random.Range(sharedCoinMin, sharedCoinMax + 1);
            SystemScript.player.AddCoins(amount);
        }
            
    }
}
