using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private bool aboutToLoop = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.loop = aboutToLoop;
        audioSource.clip = clip;
        audioSource.Play();
        aboutToLoop = true;
    }
}
