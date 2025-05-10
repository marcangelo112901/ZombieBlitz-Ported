using UnityEngine;

public class CrosshairScript : MonoBehaviour
{
    [SerializeField] private float angle;
    [SerializeField] private float distance;
    [SerializeField] private float angleDistance;

    public Transform[] lines;
    public Player player;

    public float initialGap = 2;
    public Vector2 lineSize = new Vector2(1f, 3f);

    private void Update()
    {
        angle = player.recoilAngle / 2;
        distance = Vector2.Distance(player.transform.position, transform.position);
        angleDistance = distance * Mathf.Tan(angle * Mathf.Deg2Rad);

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].localPosition = (initialGap + angleDistance) * lines[i].right;
            lines[i].localScale = lineSize;
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].localPosition = (initialGap + angleDistance) * lines[i].right;
            lines[i].localScale = lineSize;
        }
    }
}
