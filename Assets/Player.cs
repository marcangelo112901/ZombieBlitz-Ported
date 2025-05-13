using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
    public WeaponObject[] weapons = new WeaponObject[3];
    public int coins;

    public float reloadTimer;
    public bool isReloading = false;

    public float shootTimer;
    public float burstDelay;
    public int burstCount;
    public bool isShooting = false;
    public Vector2 mousePosition;
    public float recoilAngle;
    public bool isDead = false;
    public float deathTimer = 5f;

    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip getCoinSound;
    [SerializeField] private AudioClip replenishAmmoSound;
    [SerializeField] private AudioClip equipSound;
    [SerializeField] private AudioClip[] reloadSounds;
    [HideInInspector]
    public NetworkVariable<bool> UIOpened = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
        Debug.Log(SystemScript.Instance);
        weapons[0] = SystemScript.Instance.GetClonedWeaponByName(weapons[0].weaponName);
        SystemScript.Instance.players.Add(gameObject);
        if (IsServer)
            currentHP.Value = maxHP;

        CameraScript cameraScript = CameraScript.Instance;
        if (!IsOwner)
        {
            Destroy(GetComponent<AudioListener>());
            Destroy(crosshair);
            return;
        }
        ShopScript.player = this;
        GameUIScript.player = this;
        SystemScript.player = this;

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

        CheckWindowKey();
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

        if (!IsOwner) return;
        if (isFiring.Value && shootTimer <= 0f && burstDelay <= 0f)
            isShooting = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc(float[] angles)
    {
        SpawnBulletClientRpc(angles);
    }

    [ClientRpc]
    private void SpawnBulletClientRpc(float[] angles)
    {
        audioManager.playClip(_currentWeapon.shootClip);
        weapon.shoot();
        for (int i = 0; i < angles.Length; i++)
        {
            BulletScript bullet = Instantiate(_currentWeapon.bulletObject, transform.position, Quaternion.identity).GetComponent<BulletScript>();
            bullet.damage = _currentWeapon.damage;
            bullet.penetration = _currentWeapon.penetration;
            bullet.angle = angles[i];
            bullet.explosive = _currentWeapon.explosive;
            bullet.player = this;
        }
    }

    private void shootUpdate()
    {
        if (!IsOwner) return;
        if (_currentWeapon.ammoInClip <= 0)
        {
            if (weaponIndex.Value == 2 && _currentWeapon.storedAmmo <= 0)
            {
                weapons[2] = null;

                if (weapons[1] != null)
                    weaponIndex.Value = 1;
                else
                    weaponIndex.Value = 0;
                isShooting = false;
            }
            else if (_currentWeapon.storedAmmo > 0)
                initiateReload();
            else
                isShooting = false;
        }
        else
        {

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

            SpawnBulletServerRpc(selectedAngles.ToArray());
            recoilAngle = Mathf.Clamp(recoilAngle + _currentWeapon.recoil, _currentWeapon.minAngle, _currentWeapon.maxAngle);
            CameraScript.Instance.shakeCamera(_currentWeapon.shakeTime, _currentWeapon.shakeAmplitude, _currentWeapon.shakeFrequency);

            shootTimer = _currentWeapon.firerate;
        }

    }

    private void clickUpdate()
    {
        if (!IsOwner) return;

        if (weaponPivot.activeSelf == false)
        {
            isFiring.Value = false;
            return;
        }

        if (SystemScript.Instance.isShooting == true)
        {
            if (UIOpened.Value) return;
            isFiring.Value = true;
        }

        if (SystemScript.Instance.isShooting == false)
        {
            isFiring.Value = false;
        }
    }

    private void angleUpdate()
    {
        if (UIOpened.Value) return;
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
            SystemScript.Instance.PlaySystemSound(reloadSounds[0]);
            reloadTimer = _currentWeapon.reloadTime;
            isReloading = true;
        }
    }

    private void reloadUpdate()
    {
        if (IsOwner)
        {
            if (reloadTimer > 0f)
            {
                reloadTimer -= Time.deltaTime;
                return;
            }

            SystemScript.Instance.PlaySystemSound(reloadSounds[1]);
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
        get
        {
            return _currentWeapon;
        }
    }


    public void weaponIndexCheck()
    {
        if (!IsOwner) return;
        if (UIOpened.Value) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            weaponIndex.Value = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && weapons[1] != null)
            weaponIndex.Value = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && weapons[2] != null)
            weaponIndex.Value = 2;

        if (Input.GetKeyDown(KeyCode.R))
            if (_currentWeapon.ammoInClip < _currentWeapon.maxAmmoInClip)
                initiateReload();
    }

    public void DealDamage(int damage) // Called in server
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
        if (isDead == false)
        {
            healAccumulator = 0f;
            regenTimer = float.PositiveInfinity;
            currentHP.Value = 0;
            DeadClientRpc();
        }
    }

    [ClientRpc]
    public void DeadClientRpc()
    {
        isDead = true;
        SystemScript.Instance.players.Remove(gameObject);
        weaponPivot.SetActive(false);
        GetComponent<PlayerMovement>().playerAnimator.SetTrigger("death");
        if (SystemScript.Instance.players.Count == 0)
            GameOver();

        if (IsOwner)
        {
            crosshair.SetActive(false);
            Destroy(GetComponent<AudioListener>());
            CameraScript.Instance.cCamera.Follow = null;
            CameraScript.Instance.gameObject.AddComponent<AudioListener>();
        }
    }

    private void GameOver()
    {
        Cursor.visible = true;
        SystemScript system = SystemScript.Instance;
        system.gameoverAnimation.Play("Gameover Animation");
        system.ScoreTMP.text = "SCORE: " + system.score + "   WAVE: " + system.waveNumber;
        system.musicSource.Stop();
        system.audioManager.playClip(system.GameoverClips[0]);
        StartCoroutine(system.DelayedSound());
    }

    [ClientRpc]
    public void ReplenishBulletClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (weapons[1] != null)
            weapons[1].storedAmmo = Mathf.Clamp(weapons[1].storedAmmo + weapons[1].ammoBoxIncrement, 0, weapons[1].maxStoredAmmo);

        if (IsOwner)
            SystemScript.Instance.PlaySystemSound(replenishAmmoSound);
    }

    [ClientRpc]
    public void AddCoinsClientRpc(int amount, ClientRpcParams clientRpcParams = default)
    {
        SystemScript.Instance.PlaySystemSound(getCoinSound);
        coins += amount;
    }
    
    [ClientRpc]
    public void EquipWeaponClientRpc(int weaponID, int index)
    {
        SystemScript system = SystemScript.Instance;
        weapons[index] = Instantiate(system.GetWeaponObject(weaponID));
        if (IsOwner)
            SystemScript.Instance.PlaySystemSound(equipSound);
    }

    [ServerRpc]
    public void EquipWeaponServerRpc(int weaponID, int index)
    {
        EquipWeaponClientRpc(weaponID, index);
    }

    public void ExitWindowSetActive(bool boolean)
    {
        if (boolean)
        {
            UIOpened.Value = true;
            SystemScript.Instance.ExitWindow.SetActive(true);
            Cursor.visible = true;
        }
        else
        {
            UIOpened.Value = false;
            SystemScript.Instance.ExitWindow.SetActive(false);
            Cursor.visible = false;
        }
    }

    public void ShopWindowSetActive(bool boolean)
    {
        if (boolean)
        {
            UIOpened.Value = true;
            SystemScript.Instance.ShopWindow.SetActive(true);
            Cursor.visible = true;
        }
        else
        {
            UIOpened.Value = false;
            SystemScript.Instance.ShopWindow.SetActive(false);
            Cursor.visible = false;
        }
    }

    private void CheckWindowKey()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIOpened.Value)
            {
                ExitWindowSetActive(false);
                ShopWindowSetActive(false);
            }
            else
            {
                ExitWindowSetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.B) && !UIOpened.Value)
            ShopWindowSetActive(true);

    }
}
