using UnityEngine;

public class TImeBomb : MonoBehaviour
{
    public float timer;

    private void Update()
    {
        if (timer > 0f)
            timer -= Time.deltaTime;
        else
            Destroy(gameObject);
    }
}
