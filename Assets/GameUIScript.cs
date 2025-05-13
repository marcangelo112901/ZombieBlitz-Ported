using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIScript : MonoBehaviour
{
    public static Player player;

    [SerializeField] private TextMeshProUGUI WaveCounterTMP;

    [SerializeField] private TextMeshProUGUI WeaponNameTMP;
    [SerializeField] private Image WeaponImage;
    [SerializeField] private TextMeshProUGUI WeaponBulletTMP;
    [SerializeField] private GameObject Button1Up;
    [SerializeField] private GameObject Button1Down;
    [SerializeField] private GameObject Button2Up;
    [SerializeField] private GameObject Button2Down;
    [SerializeField] private GameObject Button3Up;
    [SerializeField] private GameObject Button3Down;
    [SerializeField] private BrokenProgressBar ReloadBar;

    [SerializeField] private TextMeshProUGUI ScoreTMP;
    [SerializeField] private TextMeshProUGUI CoinTMP;
    [SerializeField] private TextMeshProUGUI HealthTMP;

    private SystemScript system;

    private void Start()
    {
        system = SystemScript.Instance;
    }

    private void Update()
    {
        if (player == null || player.currentWeapon == null) return;

        WaveCounterTMP.text = system.waveNumber.ToString();

        WeaponNameTMP.text = player.currentWeapon.weaponName;
        WeaponImage.sprite = player.currentWeapon.wholeImage;

        string bulletTXT = "";
        if (player.currentWeapon.storedAmmo >= 9999)
            bulletTXT = player.currentWeapon.ammoInClip + " / UNLI";
        else
            bulletTXT = player.currentWeapon.ammoInClip + " / " + player.currentWeapon.storedAmmo;
        WeaponBulletTMP.text = bulletTXT;

        SwitchButton(player.weaponIndex.Value);

        ScoreTMP.text = system.score.ToString();
        CoinTMP.text = player.coins.ToString();
        HealthTMP.text = player.currentHP.Value.ToString();
        ChangeHealthTMPColor(player.currentHP.Value);

        float reloadValue = 1 - (player.reloadTimer / player.currentWeapon.reloadTime);
        if (reloadValue > 0.995f)
            ReloadBar.gameObject.SetActive(false);
        else
        {
            ReloadBar.currentValue = reloadValue;
            ReloadBar.gameObject.SetActive(true);
        }
    }

    private void ChangeHealthTMPColor(int value)
    {
        float Value = Mathf.Clamp01((float)value / 100);
        Color color = Color.Lerp(Color.red, Color.green, Value);
        HealthTMP.color = color;
    }

    private void SwitchButton(int index)
    {
        switch(index)
        {
            case 0:
                Button1Down.SetActive(true);
                Button1Up.SetActive(false);
                Button2Down.SetActive(false);
                Button2Up.SetActive(true);
                Button3Down.SetActive(false);
                Button3Up.SetActive(true);
                break;

            case 1:
                Button1Down.SetActive(false);
                Button1Up.SetActive(true);
                Button2Down.SetActive(true);
                Button2Up.SetActive(false);
                Button3Down.SetActive(false);
                Button3Up.SetActive(true);
                break;

            case 2:
                Button1Down.SetActive(false);
                Button1Up.SetActive(true);
                Button2Down.SetActive(false);
                Button2Up.SetActive(true);
                Button3Down.SetActive(true);
                Button3Up.SetActive(false);
                break;
        }
    }
}
