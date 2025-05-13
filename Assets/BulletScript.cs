using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public int damage;
    public int penetration;
    public float bulletSpeed = 120f;
    public float angle;
    public bool explosive;
    
    private List<Transform> damaged = new List<Transform>();
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Transform Visuals;
    [SerializeField] private Transform Particles;
    [SerializeField] private Transform Explotion;
    [SerializeField] private GameObject SoundObject;
    [HideInInspector] public Player player;

    private void Start()
    {
        Visuals.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f);
        float distanceThisFrame = bulletSpeed * Time.deltaTime;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distanceThisFrame, hitMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (explosive == true)
            {
                if (player.IsOwner)
                {
                    List<GameObject> enemies = SystemScript.Instance.enemies;
                    float range = 9; //<<<<<<-------------------------------------------------------------------- Magic Number Alert

                    for (int ii = 0; ii < enemies.Count; ii++)
                    {
                        float distance = Vector2.Distance(transform.position, enemies[ii].transform.position);
                        if (range >= distance)
                        {
                            if (enemies[ii].transform.TryGetComponent(out BaseEnemy enemy))
                                enemy.DealDamageServerRpc(damage);
                        }
                            
                    }
                }

                DestroyBullet(hits[i].point);
            }
            else
            {
                if (hits[i].transform.gameObject.layer == LayerMask.NameToLayer("Obstacle")) DestroyBullet(hits[i].point);
                if (damaged.Contains(hits[i].transform)) continue;
                damaged.Add(hits[i].transform);
                penetration--;

                if (this.player != null && this.player.IsOwner)
                {
                    if (hits[i].transform.TryGetComponent(out BaseEnemy enemy))
                        enemy.DealDamageServerRpc(damage);

                }

                if (hits[i].transform.TryGetComponent(out Player player))
                {
                    if (player.IsServer)
                        player.DealDamage(damage);
                }

                if (penetration == 0) DestroyBullet(hits[i].point);
            }
        }

        transform.position += direction * distanceThisFrame;
    }

    private void DestroyBullet(Vector2 hitPoint)
    {
        if (Visuals != null)
        {
            Visuals.SetParent(null);
            Visuals.position = hitPoint + new Vector2(0f, 0.875f);
            if (TryGetComponent(out SpriteRenderer sprite))
                sprite.enabled = false;
            
        }

        if (Particles != null)
        {
            Particles.SetParent(null);
            Particles.position = hitPoint + new Vector2(0f, 0.875f);
            ParticleSystem particleSystem = Particles.GetComponent<ParticleSystem>();
            particleSystem.Stop();
            //var emission = particleSystem.emission;
            //emission.enabled = false;
        }

        if (Explotion != null)
        {
            Explotion.SetParent(null);
            Explotion.position = hitPoint + new Vector2(0f, 0.875f);
            ParticleSystem particleSystem = Explotion.GetComponent<ParticleSystem>();
            particleSystem.Play();
        }

        if (SoundObject != null)
        {
            Instantiate(SoundObject, null);
        }

        Destroy(gameObject);
    }
}
