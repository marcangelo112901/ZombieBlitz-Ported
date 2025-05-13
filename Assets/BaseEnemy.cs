using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemy : NetworkBehaviour
{
    protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected NetworkAnimator networkAnimator;
    protected CircleCollider2D[] colliders;
    protected AudioSource audioSource;

    public int maxHP;
    public float hpPerWaveIncrement = 1f;
    public int score;
    public float scorePerWaveIncrement = 1f;

    public NetworkVariable<int> currentHP = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int damage;
    public float movementSpeed;

    public float foreswingTime;
    public float backswingTime;
    public float attackSpeed;
    public float attackRange;
    public float deathTime;
    private bool hasDropped = false;

    [SerializeField] protected float foreswingTimer;
    [SerializeField] protected float backswingTimer;
    [SerializeField] protected float attackTimer;

    [SerializeField] protected AIState aiState = AIState.idle;
    [SerializeField] protected bool hasAttacked = false;
    [SerializeField] protected GameObject targetPlayer;

    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] deathSounds;

    // Animator Synced Variables
    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> flipX = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        networkAnimator = animator.GetComponent<NetworkAnimator>();
        spriteRenderer = animator.GetComponent<SpriteRenderer>();
        colliders = GetComponents<CircleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        agent.speed = movementSpeed;
    }

    public override void OnNetworkSpawn()
    {
        SystemScript.Instance.enemies.Add(gameObject);
        maxHP = (int)(maxHP * Mathf.Pow(hpPerWaveIncrement, SystemScript.Instance.waveNumber - 1));
        score = (int)(score * Mathf.Pow(scorePerWaveIncrement, SystemScript.Instance.waveNumber - 1));
        if (!IsServer) return;

        currentHP.Value = maxHP;
    }

    public override void OnNetworkDespawn()
    {
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        spriteRenderer.flipX = flipX.Value;
        animator.SetBool("isMoving", isMoving.Value);
        if (!IsServer) return;
        Vector3 direction = agent.velocity.normalized;
        if (direction.x > 0f)
            flipX.Value = false;
        else if (direction.x < 0f)
            flipX.Value = true;
        AI();
    }

    private void AI()
    {
        attackTimer -= Time.fixedDeltaTime;
        switch(aiState)
        {
            case AIState.idle:
                targetPlayer = SearchNearestPlayer();
                if (targetPlayer != null)
                {
                    agent.SetDestination(targetPlayer.transform.position);
                    aiState = AIState.moving;
                    isMoving.Value = true;
                }
                break;

            case AIState.moving:
                if (targetPlayer == null)
                {
                    aiState = AIState.idle;
                    break;
                }
                float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);
                if (distance <= attackRange && attackTimer <= 0f)
                {
                    foreswingTimer = foreswingTime;
                    backswingTimer = foreswingTimer;
                    isMoving.Value = false;
                    networkAnimator.SetTrigger("Attack");
                    agent.ResetPath();
                    PlaySound("attack");
                    aiState = AIState.attacking;
                }
                else
                {
                    targetPlayer = SearchNearestPlayer();
                    if (targetPlayer != null)
                    {
                        agent.SetDestination(targetPlayer.transform.position);
                        isMoving.Value = true;
                    }
                }
                break;

            case AIState.attacking:
                if (foreswingTimer > 0f)
                {
                    foreswingTimer -= Time.fixedDeltaTime;
                    return;
                }

                if (hasAttacked == false)
                {
                    hasAttacked = true;
                    attackTimer = attackSpeed;
                    Attack();
                }

                if (backswingTimer > 0f)
                {
                    backswingTimer -= Time.fixedDeltaTime;
                    return;
                }

                hasAttacked = false;
                aiState = AIState.idle;
                break;

            case AIState.dead:
                if (hasDropped == false)
                {
                    hasDropped = true;
                    OnDeath();
                }

                if (deathTime > 0f)
                {
                    deathTime -= Time.deltaTime;
                    return;
                }
                SystemScript.Instance.enemies.Remove(gameObject);
                GetComponent<NetworkObject>().Despawn(true);
                break;
        }
    }

    private GameObject SearchNearestPlayer()
    {
        List<GameObject> players = SystemScript.Instance.players;
        GameObject nearestPlayer = null;

        for (int i = 0; i < players.Count; i++)
        {
            if (nearestPlayer == null)
            {
                nearestPlayer = players[i];
                continue;
            }

            float nearestDistance = Vector2.Distance(transform.position, nearestPlayer.transform.position);
            float currentDistance = Vector2.Distance(transform.position, players[i].transform.position);

            if (nearestDistance > currentDistance)
                nearestPlayer = players[i];
        }

        return nearestPlayer;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DealDamageServerRpc(int damage)
    {
        if (damage >= currentHP.Value)
        {
            RemoveCollidersClientRpc();
            networkAnimator.SetTrigger("Death");
            agent.ResetPath();
            aiState = AIState.dead;
        }
        else
        {
            currentHP.Value -= damage;
        }
    }

    [ClientRpc]
    public void RemoveCollidersClientRpc()
    {
        PlaySound("death");
        foreach (CircleCollider2D collider in colliders)
            collider.enabled = false;
    }

    protected abstract void Attack();

    protected void PlaySound(string sfxName)
    {
        if (sfxName == "attack")
        {
            int num = Random.Range(0, attackSounds.Length);
            audioSource.clip = attackSounds[num];
            audioSource.Play();
        }
        else if (sfxName == "death")
        {
            int num = Random.Range(0, deathSounds.Length);
            audioSource.clip = deathSounds[num];
            audioSource.Play();
        }
    }

    public abstract void OnDeath();

}

public enum AIState
{
    idle,
    moving,
    attacking,
    dead
}
