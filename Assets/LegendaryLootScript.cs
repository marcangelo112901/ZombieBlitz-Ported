using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class LegendaryLootScript : BaseLoot
{
    public WeaponObject legendaryWeapon;
    public override void OnLoot(Player player, ClientRpcParams rpcParams)
    {
        int weaponID = SystemScript.Instance.GetWeaponID(legendaryWeapon);
        player.EquipWeaponClientRpc(weaponID, 2);
    }

    public override void Awake()
    {
        base.Awake();
        legendaryWeapon = SystemScript.Instance.GetClonedWeaponByName(legendaryWeapon.weaponName);
    }
}
