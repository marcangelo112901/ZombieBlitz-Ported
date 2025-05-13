using Unity.Netcode;
using UnityEngine;

public class MeleeZombie : BaseEnemy
{
    protected override void Attack()
    {
        targetPlayer.TryGetComponent(out Player player);
        if (player)
            player.DealDamage(damage);
    }

    public override void OnDeath()
    {
        int chance = Random.Range(0, 100);
        if (chance < 50)
        {
            GameObject loot = SystemScript.Instance.lootSystem.GetLoot(LootType.common);
            GameObject droppedLoot = Instantiate(loot, transform.position, Quaternion.identity, null);
            if (droppedLoot.TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Spawn();
            }
        }

        UpdateScoreClientRpc();
    }

    [ClientRpc]
    private void UpdateScoreClientRpc()
    {
        SystemScript.Instance.score += score;
    }
}
