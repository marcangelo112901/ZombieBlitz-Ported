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
    public NetworkVariable<int> currentHP = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int damage;
    public float movementSpeed;

    public float foreswingTime;
    public float backswingTime;
    public float attackSpeed;
    public float attackRange;
    public float deathTime;

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
        if (!IsServer) return;

        currentHP.Value = maxHP;
    }

    public override void OnNetworkDespawn()
    {
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        Vector3 direction = agent.velocity.normalized;
        if (direction.x > 0f)
            flipX.Value = false;
        else if (direction.x < 0f)
            flipX.Value = true;

        spriteRenderer.flipX = flipX.Value;
        animator.SetBool("isMoving", isMoving.Value);
        if (!IsServer) return;
        AI();
    }

    private void AI()
    {
        attackTimer -= Time.fixedDeltaTime;
        switch(aiState)
        {
            case AIState.idle:
                targetPlayer = SearchNearestPlayer();
                agent.SetDestination(targetPlayer.transform.position);
                aiState = AIState.moving;
                isMoving.Value = true;
                break;

            case AIState.moving:
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
                    agent.SetDestination(targetPlayer.transform.position);
                    isMoving.Value = true;
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
                if (deathTime > 0f)
                {
                    deathTime -= Time.deltaTime;
                    return;
                }
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

}

public enum AIState
{
    idle,
    moving,
    attacking,
    dead
}
