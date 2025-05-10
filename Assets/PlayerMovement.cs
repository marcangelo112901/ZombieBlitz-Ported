using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Player player;
    public Animator playerAnimator;
    private SpriteRenderer spriteRenderer;

    public float moveSpeed = 5f;

    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> direction = new NetworkVariable<float>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        spriteRenderer = playerAnimator.transform.GetComponent<SpriteRenderer>();
        SystemScript.Instance.players.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
        rb.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        playerAnimator.SetBool("isMoving", isMoving.Value);
        playerAnimator.SetFloat("direction", direction.Value);
        if (player.mouseAngle.Value < 90 || player.mouseAngle.Value > 260)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;



        if (!IsOwner) return;

        if (moveDirection != Vector2.zero)
        {
            isMoving.Value = true;

            if (moveDirection.x > 0)
            {
                if (player.mouseAngle.Value < 90 || player.mouseAngle.Value > 270)
                    direction.Value = 1;
                else
                    direction.Value = -1;
            }
            else
            {
                if (player.mouseAngle.Value < 90 || player.mouseAngle.Value > 270)
                    direction.Value = -1;
                else
                    direction.Value = 1;
            }
        }
        else
        {
            isMoving.Value = false;
        }
    }
}
