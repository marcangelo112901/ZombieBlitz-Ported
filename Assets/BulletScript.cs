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
    private List<Transform> damaged = new List<Transform>();
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Transform Visuals;
    [HideInInspector] public Player player;

    private void Update()
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f);
        float distanceThisFrame = bulletSpeed * Time.deltaTime;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distanceThisFrame, hitMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.gameObject.layer == LayerMask.NameToLayer("Obstacle")) DestroyBullet(hits[i].point);
            if (damaged.Contains(hits[i].transform)) continue;
            damaged.Add(hits[i].transform);
            penetration--;

            if (player.IsOwner)
            {
                hits[i].transform.TryGetComponent(out BaseEnemy enemy);
                if (enemy)
                    enemy.DealDamageServerRpc(damage);
            }

            if (penetration == 0) DestroyBullet(hits[i].point);
        }

        transform.position += direction * distanceThisFrame;
    }

    private void DestroyBullet(Vector2 hitPoint)
    {
        Visuals.parent = transform.parent;
        Visuals.position = hitPoint + new Vector2(0f, 0.96f);
        Destroy(gameObject);
    }
}
