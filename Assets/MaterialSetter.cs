using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [Range(0f, 1f)] public float DissolveLevel;
    public bool playParticle;
    private bool hasPlayed;
    private float prevDissolveLevel;
    private ParticleSystem particle;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particle = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        UpdateDissolveLevel();
        UpdateParticleBool();
    }

    private void UpdateParticleBool()
    {
        if (playParticle)
            if (!hasPlayed)
            {
                hasPlayed = true;
                particle.Play();
            }
    }

    public void UpdateDissolveLevel()
    {
        if (DissolveLevel != prevDissolveLevel)
        {
            spriteRenderer.material.SetFloat("_DissolveLevel", DissolveLevel);
            prevDissolveLevel = DissolveLevel;
        }
    }
}
