using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Object", menuName = "ScriptableObjects/New Weapon Object")]
public class WeaponObject : ScriptableObject
{
    public GameObject prefab;
    public GameObject bulletObject;

    [Header("Weapon Attributes")]
    public int damage;
    public float firerate;
    public int penetration;
    public float reloadTime;

    public int maxAmmoInClip;
    public int ammoInClip;
    public int maxStoredAmmo;
    public int storedAmmo;

    [Header("Weapon Bullet Spread")]
    public float stability;
    public float recoil;
    public float maxAngle;
    public float minAngle;
    public int ammoBoxIncrement;

    [Header("Weapon Properties")]
    public ShootType shootType;
    public int burstCount;
    public float burstDelay;
    public int bulletCount;

    [Header("Screen Shake")]
    public float shakeTime;
    public float shakeAmplitude;
    public float shakeFrequency;
}

public enum ShootType
{
    Semi_Auto,
    Burst,
    Full_Auto
}
