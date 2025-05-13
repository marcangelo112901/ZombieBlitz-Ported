using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Object", menuName = "ScriptableObjects/New Weapon Object")]
public class WeaponObject : ScriptableObject
{
    public GameObject prefab;
    public GameObject bulletObject;
    public AudioClip shootClip;

    [Header("Weapon Attributes")]
    public string weaponName;
    public string description;
    public int damage;
    public float firerate;
    public int penetration;
    public float reloadTime;

    public int maxAmmoInClip;
    public int ammoInClip;
    public int maxStoredAmmo;
    public int storedAmmo;

    public bool explosive;

    [Header("Weapon Bullet Spread")]
    public float stability;
    public float recoil;
    public float maxAngle;
    public float minAngle;
    public int ammoBoxIncrement;

    [Header("Weapon Properties")]
    public ShootType shootType;
    public int burstCount = 1;
    public float burstDelay;
    public int bulletCount = 1;

    [Header("Screen Shake")]
    public float shakeTime;
    public float shakeAmplitude;
    public float shakeFrequency;

    public int price;

    [Header("UI Variables")]
    public float UIDamage;
    public float UIFirerate;
    public float UIRecoil;
    public float UIPenetration;
    public float UIReloadSpeed;

    public Sprite wholeImage;
    public Sprite silhuetteImage;
}

public enum ShootType
{
    Semi_Auto,
    Burst,
    Full_Auto,
    Pump_Action,
    Bolt_Action
}
