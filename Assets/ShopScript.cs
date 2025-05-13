using System;
using System.IO.IsolatedStorage;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour
{
    public static Player player;

    [SerializeField] private WeaponObject[] SecondaryWeapons;
    [SerializeField] private WeaponObject[] ShotgunWeapons;
    [SerializeField] private WeaponObject[] SmgWeapons;
    [SerializeField] private WeaponObject[] RifleWeapons;
    [SerializeField] private WeaponObject[] MachinegunWeapons;
    [SerializeField] private WeaponObject[] SniperRifleWeapons;

    private WeaponObject[] AllWeapons;


    // List Variables
    private WeaponObject[] DisplayedWeapons = new WeaponObject[5];
    [SerializeField] private TextMeshProUGUI[] selectionTMP;
    [SerializeField] private Image[] silhuetteImages;

    // Specs Variables
    [SerializeField] private TextMeshProUGUI weaponNameTMP;
    [SerializeField] private TextMeshProUGUI weaponTriggerTypeTMP;
    [SerializeField] private TextMeshProUGUI weaponAmmoTMP;
    [SerializeField] private TextMeshProUGUI weaponPriceTMP;
    [SerializeField] private Image selectedImage;

    [SerializeField] private BrokenProgressBar[] specBars;
    [SerializeField] private CanvasGroup WeaponInfoCG;
    private Animation WeaponInfoAnim;

    private int weaponCount = 5; // Magic Number

    // UI Sounds
    [SerializeField] private AudioClip errorSFX;
    [SerializeField] private AudioClip hoverSFX;
    private void Awake()
    {
        AllWeapons = SecondaryWeapons.Concat(ShotgunWeapons).Concat(SmgWeapons).Concat(RifleWeapons).Concat(MachinegunWeapons).Concat(SniperRifleWeapons).ToArray();
        WeaponInfoAnim = selectedImage.GetComponent<Animation>();
    }

    private void Start()
    {
        SystemScript system = SystemScript.Instance;
        for (int i = 0; i < AllWeapons.Length; i++)
        {
            AllWeapons[i] = system.GetClonedWeaponByName(AllWeapons[i].weaponName);
        }
        SelectWeaponCategory(0);
    }

    public void SelectWeaponCategory(int index)
    {
        for (int i = index * weaponCount; i < (index * weaponCount) + weaponCount; i++)
        {
            int specsIndex = i - (index * weaponCount);
            DisplayedWeapons[specsIndex] = AllWeapons[i];
            selectionTMP[specsIndex].text = AllWeapons[i].weaponName;
            silhuetteImages[specsIndex].sprite = AllWeapons[i].silhuetteImage;
        }
    }

    public void HoverWeapon(int index)
    {
        SystemScript.Instance.PlaySystemSound(hoverSFX);
        WeaponInfoCG.alpha = 1f;
        WeaponInfoAnim.Stop();
        WeaponInfoAnim.Play();

        weaponNameTMP.text = DisplayedWeapons[index].weaponName;
        weaponTriggerTypeTMP.text = DisplayedWeapons[index].shootType.ToString();

        String maxAmmoTXT = "";
        if (DisplayedWeapons[index].storedAmmo >= 10000)
            maxAmmoTXT = "INFINITE";
        else
            maxAmmoTXT = DisplayedWeapons[index].storedAmmo.ToString();
        weaponAmmoTMP.text = DisplayedWeapons[index].ammoInClip + " / " + maxAmmoTXT;

        String priceTXT = "";
        if (DisplayedWeapons[index].price == 0)
            priceTXT = "PURCHASED";
        else
            priceTXT = DisplayedWeapons[index].price.ToString();
        weaponPriceTMP.text = priceTXT;

        selectedImage.sprite = DisplayedWeapons[index].wholeImage;

        specBars[0].SmoothChangeValue(DisplayedWeapons[index].UIDamage);
        specBars[1].SmoothChangeValue(DisplayedWeapons[index].UIFirerate);
        specBars[2].SmoothChangeValue(DisplayedWeapons[index].UIRecoil);
        specBars[3].SmoothChangeValue(DisplayedWeapons[index].UIPenetration);
        specBars[4].SmoothChangeValue(DisplayedWeapons[index].UIReloadSpeed);
    }

    public void UnHoverWeapon()
    {
        WeaponInfoCG.alpha = 0f;
    }

    public void BuyWeapon(int index)
    {
        SystemScript system = SystemScript.Instance;
        if (player.coins < DisplayedWeapons[index].price)
        {
            SystemScript.Instance.PlaySystemSound(errorSFX);
            return;
        }

        player.coins -= DisplayedWeapons[index].price;
        DisplayedWeapons[index].price = 0;

        int weaponID = system.GetWeaponID(DisplayedWeapons[index]);
        if (DisplayedWeapons[index].maxStoredAmmo >= 9999)
            player.EquipWeaponServerRpc(weaponID, 0);
        else
            player.EquipWeaponServerRpc(weaponID, 1);
    }

    private void OnEnable()
    {
        player.UIOpened.Value = true;
        player.crosshair.SetActive(false);
    }

    private void OnDisable()
    {
        player.UIOpened.Value = false;
        player.crosshair.SetActive(true);
    }
}

