using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    private SystemScript system;

    [SerializeField] private GameObject[] coins;
    [SerializeField] private GameObject[] ammoBox;
    [SerializeField] private GameObject[] legendaryItems;
    private GameObject[] ItemDatabase;

    private void Awake()
    {
        system = GetComponent<SystemScript>();
        ItemDatabase = coins.Concat(ammoBox).Concat(legendaryItems).ToArray();
    }

    public GameObject GetLoot(LootType lootType)
    {
        int chance = Random.Range(0, 1000);
        if (system.waveNumber < 5)
        {
            switch (lootType)
            {
                case LootType.common:
                    if (chance <= 1) { return legendaryWeapon(); }
                    else if (chance <= 20) { return ammoBox[0]; }
                    else if (chance <= 50) { return coins[1]; }
                    else { return coins[0]; }
                case LootType.rare:
                    if (chance <= 3) { return legendaryWeapon(); }
                    else if (chance <= 40) { return ammoBox[0]; }
                    else if (chance <= 200) { return coins[1]; }
                    else { return coins[0]; }
            }
        }
        else if (system.waveNumber < 10)
        {
            switch (lootType)
            {
                case LootType.common:
                    if (chance <= 5) { return legendaryWeapon(); }
                    else if (chance <= 60) { return ammoBox[0]; }
                    else if (chance <= 100) { return coins[1]; }
                    else { return coins[0]; }
                case LootType.rare:
                    if (chance <= 7) { return legendaryWeapon(); }
                    else if (chance <= 10) { return coins[2]; }
                    else if (chance <= 80) { return ammoBox[0]; }
                    else if (chance <= 400) { return coins[1]; }
                    else { return coins[0]; }
            }
        }
        else if (system.waveNumber < 15)
        {
            switch (lootType)
            {
                case LootType.common:
                    if (chance <= 7) { return legendaryWeapon(); }
                    else if (chance <= 10) { return coins[2]; }
                    else if (chance <= 80) { return ammoBox[0]; }
                    else if (chance <= 200) { return coins[1]; }
                    else { return coins[0]; }
                case LootType.rare:
                    if (chance <= 9) { return legendaryWeapon(); }
                    else if (chance <= 30) { return coins[2]; }
                    else if (chance <= 100) { return ammoBox[0]; }
                    else if (chance <= 700) { return coins[1]; }
                    else { return coins[0]; }
            }
        }
        else
        {
            switch (lootType)
            {
                case LootType.common:
                    if (chance <= 10) { return legendaryWeapon(); }
                    else if (chance <= 40) { return coins[2]; }
                    else if (chance <= 150) { return ammoBox[0]; }
                    else if (chance <= 400) { return coins[1]; }
                    else { return coins[0]; }
                case LootType.rare:
                    if (chance <= 15) { return legendaryWeapon(); }
                    else if (chance <= 70) { return coins[2]; }
                    else if (chance <= 200) { return ammoBox[0]; }
                    else { return coins[1]; }
            }
        }

        return null;
    }

    public GameObject GetLootByName(string name)
    {
        foreach (GameObject loot in ItemDatabase)
        {
            if (loot.name == name)
                return loot;
        }
        return null;
    }

    private GameObject legendaryWeapon()
    {
        int chance = Random.Range(0, legendaryItems.Length);
        return legendaryItems[chance];
    }
}
public enum LootType
{
    common,
    rare
}
