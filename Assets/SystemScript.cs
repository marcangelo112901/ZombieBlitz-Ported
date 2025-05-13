using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SystemScript : MonoBehaviour
{
    public static SystemScript Instance { private set; get; }
    public LootSystem lootSystem;
    public int waveNumber;
    public int score;
    public WeaponObject[] weaponDatabase;
    private Dictionary<int, WeaponObject> weaponDictionary = new Dictionary<int, WeaponObject>();
    private Dictionary<WeaponObject, int> weaponIDDictionary = new Dictionary<WeaponObject, int>();
    public List<GameObject> players;
    public List<GameObject> enemies;
    public static Player player;
    public AudioManager audioManager;
    public AudioSource musicSource;
    public bool isShooting = false;
    public bool gameStarted = false;

    [SerializeField] private Vector2 minSpawnBounds = new Vector2 (-10f, -10f);
    [SerializeField] private Vector2 maxSpawnBounds = new Vector2 (10f, 10f);

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private AudioClip[] StartingClips;

    // Gameover
    public Animation gameoverAnimation;
    public TextMeshProUGUI ScoreTMP;
    public AudioClip[] GameoverClips;

    // Windows
    public GameObject ExitWindow;
    public GameObject ShopWindow;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        lootSystem = GetComponent<LootSystem>();
        audioManager = GetComponent<AudioManager>();

        for (int i = 0; i < weaponDatabase.Length; i++)
        {
            WeaponObject newWeaponObject = Instantiate(weaponDatabase[i]);
            weaponDictionary.Add(i, newWeaponObject);
            weaponIDDictionary.Add(newWeaponObject, i);
        }
    }

    private void Start()
    {
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("GameScene"));
        //SceneManager.UnloadSceneAsync("MainMenuScene");
        musicSource.Play();
        audioManager.playClip(StartingClips[0]);
        audioManager.playClip(StartingClips[1]);
        Cursor.visible = false;

        if (!NetworkManager.Singleton.IsServer) return;

        foreach(var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Vector2 chosenLocation = new Vector2(
                Random.Range(minSpawnBounds.x, maxSpawnBounds.x),
                Random.Range(minSpawnBounds.y, maxSpawnBounds.y));

            if (NavMesh.SamplePosition(chosenLocation, out NavMeshHit hit, 50f, NavMesh.AllAreas))
            {
                chosenLocation = hit.position;
            }

            ulong clientId = client.ClientId;



            // Instantiate the player prefab
            GameObject playerObject = Instantiate(playerPrefab, chosenLocation, Quaternion.identity);

            // Spawn it with ownership assigned to the client
            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, false);
        }
    }


    public void OnDestroy()
    {
        Cursor.visible = true;
    }

    public WeaponObject GetWeaponObject(int ID)
    {
        return weaponDictionary[ID];
    }

    public int GetWeaponID(WeaponObject weapon)
    {
        return weaponIDDictionary[weapon];
    }

    public WeaponObject GetClonedWeaponByName(string name)
    {
        foreach (WeaponObject weapon in weaponDictionary.Values)
        {
            if (name == weapon.weaponName)
                return weapon;
        }
        return null;
    }

    public void PlaySystemSound(AudioClip clip)
    {
        audioManager.playClip(clip);
    }

    public void OnPointerDown()
    {
        isShooting = true;
    }

    public void OnPointerUp()
    {
        isShooting = false;
    }

    public IEnumerator DelayedSound()
    {
        yield return new WaitForSeconds(0.5f);
        int random = Random.Range(1, 3);
        audioManager.playClip(GameoverClips[random]);
    }

    public void BackToMainMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenuScene");
    }

    public void CloseExit()
    {
        player.ExitWindowSetActive(false);
    }
}

