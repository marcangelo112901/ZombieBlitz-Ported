using Unity.Netcode;
using UnityEngine;

public class RangeZombie : BaseEnemy
{
    [SerializeField] private GameObject rockProjectile;

    public override void OnDeath()
    {
        int chance = Random.Range(0, 100);
        if (chance < 50)
        {
            GameObject loot = SystemScript.Instance.lootSystem.GetLoot(LootType.rare);
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
        SystemScript.Instance.score += score.Value;
    }

    protected override void Attack() // server called function
    {
        Vector2 vDirection = (transform.position - targetPlayer.transform.position).normalized;
        float angle = Mathf.Atan2(vDirection.y, vDirection.x) * Mathf.Rad2Deg;
        ThrowRockClientRpc(angle);
    }

    [ClientRpc]
    private void ThrowRockClientRpc(float angle)
    {
        GameObject thrownObject = Instantiate(rockProjectile, transform.position, Quaternion.identity, null);
        if (thrownObject.TryGetComponent(out BulletScript bullet))
        {
            bullet.damage = damage;
            bullet.penetration = 1;
            bullet.angle = angle - 180f;
        }
    }
}
