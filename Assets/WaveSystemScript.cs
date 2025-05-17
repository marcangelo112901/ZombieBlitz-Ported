using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using static Unity.Collections.Unicode;

public class WaveSystemScript : MonoBehaviour
{
    private NetworkManager networkManager;
    private SystemScript system;

    [SerializeField] private TilemapRenderer maxSprite;
    [SerializeField] private GameObject normalZombie;
    [SerializeField] private GameObject throwerZombie;
    [SerializeField] private GameObject fastZombie;

    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    [SerializeField] private float waveTimer = 30f;
    [SerializeField] private float spawnTimer = 1f;

    public float minimumSpawnRange = 20f;
    
    private void Awake()
    {
        minBounds = (Vector2)maxSprite.bounds.min;
        maxBounds = (Vector2)maxSprite.bounds.max;
    }
    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        system = GetComponent<SystemScript>();
    }

    private void Update()
    {
        if (!networkManager.IsServer) return;

        if (waveTimer > 0f)
         waveTimer -= Time.deltaTime;
        else
        {
            waveTimer = 30f; //<<<<<<<---------------- Magic Number
            system.waveNumber++;
        }

        if (spawnTimer > 0f)
            spawnTimer -= Time.deltaTime;
        else
        {
            spawnTimer = Random.Range(2f, 6f) * 1f;

            int chance = Random.Range(3, 6 + Mathf.Clamp(system.players.Count - 2, 0, int.MaxValue) + (int)Mathf.Floor(system.waveNumber / 5));
            for (int i = 0; i < chance; i++)
            {
                SpawnRandomZombie();
            }
        }
    }

    public void SpawnRandomZombie()
    {
        int chances = Random.Range(1, 1000);
        if (system.waveNumber < 5)
        {
            if (chances < 50) SpawnZombieRandomly(throwerZombie);
            else SpawnZombieRandomly(normalZombie);
        }
        else if (system.waveNumber < 10)
        {
            if (chances < 100) SpawnZombieRandomly(throwerZombie);
            else if (chances < 200) SpawnZombieRandomly(fastZombie);
            else SpawnZombieRandomly(normalZombie);
        }
        else if (system.waveNumber < 15)
        {
            if (chances < 200) SpawnZombieRandomly(throwerZombie);
            else if (chances < 400) SpawnZombieRandomly(fastZombie);
            else SpawnZombieRandomly(normalZombie);
        }
        else
        {
            if (chances < 300) SpawnZombieRandomly(throwerZombie);
            else if (chances < 600) SpawnZombieRandomly(fastZombie);
            else SpawnZombieRandomly(normalZombie);
        }
    }

    private void SpawnZombieRandomly(GameObject entity)
    {
        Vector2 chosenLocation = Vector2.zero;
        int attempts = 0;
        while (attempts < 100)
        {
            chosenLocation = new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));
            if (NavMesh.SamplePosition(chosenLocation, out NavMeshHit hit, 200f, NavMesh.AllAreas))
            {
                chosenLocation = hit.position;
            }

            bool isInside = false;
            for (int i = 0; i < system.players.Count; i++)
            {
                float distance = Vector2.Distance(chosenLocation, system.players[i].transform.position);
                if (distance < minimumSpawnRange)
                {
                    isInside = true;
                    break;
                }
            }

            if (isInside == false)
                break;

            attempts++;
        }
        


        GameObject spawnedEntity = Instantiate(entity, chosenLocation, Quaternion.identity, null);
        if (spawnedEntity.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn();
        }
    }

}
