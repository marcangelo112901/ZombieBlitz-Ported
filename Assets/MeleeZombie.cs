using UnityEngine;

public class MeleeZombie : BaseEnemy
{
    protected override void Attack()
    {
        targetPlayer.TryGetComponent(out Player player);
        if (player)
            player.DealDamage(damage);
    }
}
