using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using NUnit.Framework;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.UI;

public class Player : NetworkBehaviour
{
    private WeaponObject _currentWeapon;
    private Weapon weapon;
    private AudioManager audioManager;

    public int maxHP;
    public NetworkVariable<int> currentHP = new NetworkVariable<int>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float regenDelay = 5f;

    public GameObject crosshair;
    public GameObject weaponPivot;
    public float crosshairDistance = 2;
    public WeaponObject[] weapons;

    public float reloadTimer;
    public bool isReloading = false;

    public float shootTimer;
    public float burstDelay;
    public int burstCount;
    public bool isShooting = false;
    public Vector2 mousePosition;
    public float recoilAngle;

    [SerializeField] private AudioClip[] hitSounds;

    public NetworkVariable<float> mouseAngle = new NetworkVariable<float>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> isFiring = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> weaponIndex = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Server Only Variable
    public float regenTimer;
    public float healAccumulator;

    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            currentHP.Value = maxHP;

        CameraScript cameraScript = CameraScript.Instance;
        if (!IsOwner)
        {
            Destroy(GetComponent<AudioListener>());
            Destroy(crosshair);
            return;
        }
        cameraScript.setTrackingTargets(weaponPivot.transform, crosshair.transform);
    }

    void Update()
    {
        if (IsServer)
        {
            if (regenTimer > 0f)
                regenTimer -= Time.deltaTime;
            else
            {
                healAccumulator += (maxHP / 3f) * Time.deltaTime;

                if (healAccumulator > 1f)
                {
                    int wholeHP = Mathf.FloorToInt(healAccumulator);
                    healAccumulator -= wholeHP;
                    currentHP.Value = Mathf.Clamp(currentHP.Value + wholeHP, 0, maxHP);
                }
            }
        }

        RecoilUpdate();
        clickUpdate();
        weaponIndexCheck();

        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;

        if (burstDelay > 0f)
            burstDelay -= Time.deltaTime;

        angleUpdate();
        if (isReloading)
            reloadUpdate();
        else if (isShooting)
            shootUpdate();
        else
            triggerCheck();

    }

    public void RecoilUpdate()
    {
        if (!IsOwner) return;
        if (_currentWeapon == null)
        {
            recoilAngle = 0f;
            return;
        }

        recoilAngle = Mathf.Clamp(recoilAngle - _currentWeapon.stability * Time.deltaTime,
            _currentWeapon.minAngle, _currentWeapon.maxAngle);
    }

    public void triggerCheck()
    {
        currentWeapon = weapons[weaponIndex.Value];

        if (isFiring.Value && shootTimer <= 0f && burstDelay <= 0f)
            isShooting = true;
    }

    [ClientRpc]
    private void SpawnBulletClientRpc(float[] angles)
    {
        for (int i = 0; i < angles.Length; i++)
        {
            BulletScript bullet = Instantiate(_currentWeapon.bulletObject, transform.position, Quaternion.identity).GetComponent<BulletScript>();
            bullet.damage = _currentWeapon.damage;
            bullet.penetration = _currentWeapon.penetration;
            bullet.angle = angles[i];
            bullet.player = this;
        }
    }

    private void shootUpdate()
    {
        if (_currentWeapon.ammoInClip <= 0)
        {
            initiateReload();
            return;
        }

        if (burstCount == 0)
        {
            isShooting = false;
            burstDelay = _currentWeapon.burstDelay;
            burstCount = _currentWeapon.burstCount;
            return;
        }


        if (shootTimer > 0f || burstDelay > 0f) return;

        burstCount--;
        _currentWeapon.ammoInClip--;

        List<float> selectedAngles = new List<float>();
        for (int i = 0; i < _currentWeapon.bulletCount; i++)
            selectedAngles.Add(mouseAngle.Value + (Random.Range(-recoilAngle, recoilAngle) / 2));

        SpawnBulletClientRpc(selectedAngles.ToArray());
        recoilAngle = Mathf.Clamp(recoilAngle + _currentWeapon.recoil, _currentWeapon.minAngle, _currentWeapon.maxAngle);
        CameraScript.Instance.shakeCamera(_currentWeapon.shakeTime, _currentWeapon.shakeAmplitude, _currentWeapon.shakeFrequency);

        weapon.shoot();
        shootTimer = _currentWeapon.firerate;


      

    }

    private void clickUpdate()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            isFiring.Value = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isFiring.Value = false;
        }
    }

    private void angleUpdate()
    {
        if (mouseAngle.Value < 90 || mouseAngle.Value > 270)
            weaponPivot.transform.rotation = Quaternion.Euler(0, 0, mouseAngle.Value);
        else
            weaponPivot.transform.rotation = Quaternion.Euler(0, -180f, (mouseAngle.Value + 180f) * -1f);

        if (!IsOwner) return;
        if (!IsMouseInsideWindow()) return;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mousePos = Input.mousePosition;

        Vector2 direction = mousePos - screenCenter;
        mouseAngle.Value = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (mouseAngle.Value < 0) mouseAngle.Value += 360f;

        mousePosition = Camera.main.ScreenToWorldPoint(mousePos);
        crosshair.transform.position = mousePosition;
    }

    private void initiateReload()
    {
        if (reloadTimer <= 0f && isReloading == false)
        {
            reloadTimer = _currentWeapon.reloadTime;
            isReloading = true;
        }
    }

    private void reloadUpdate()
    {
        if (reloadTimer > 0f)
        {
            reloadTimer -= Time.deltaTime;
            return;
        }

        isReloading = false;
        if (_currentWeapon.storedAmmo >= _currentWeapon.maxAmmoInClip)
        {
            _currentWeapon.ammoInClip = _currentWeapon.maxAmmoInClip;
            _currentWeapon.storedAmmo -= _currentWeapon.maxAmmoInClip;
        }
        else if (_currentWeapon.storedAmmo < _currentWeapon.maxAmmoInClip)
        {
            _currentWeapon.ammoInClip = _currentWeapon.storedAmmo;
            _currentWeapon.storedAmmo = 0;
        }
    }

    bool IsMouseInsideWindow()
    {
        Vector3 pos = Input.mousePosition;
        return pos.x >= 0 && pos.x <= Screen.width &&
               pos.y >= 0 && pos.y <= Screen.height;
    }

    public WeaponObject currentWeapon
    {
        set
        {
            if (_currentWeapon != value)
            {
                _currentWeapon = value;
                if (weapon != null)
                    Destroy(weapon.gameObject);

                weapon = Instantiate(value.prefab, weaponPivot.transform).GetComponent<Weapon>();
                burstCount = _currentWeapon.burstCount;
            }
        }
    }


    public void weaponIndexCheck()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            weaponIndex.Value = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            weaponIndex.Value = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            weaponIndex.Value = 3;
    }

    public void DealDamage(int damage)
    {
        if (damage >= currentHP.Value)
            Dead();
        else
        {
            int num = Random.Range(0, hitSounds.Length);
            audioManager.playClip(hitSounds[num]);

            currentHP.Value -= damage;
            regenTimer = regenDelay;
            healAccumulator = 0f;
        }
    }

    private void Dead()
    {

    }
}
