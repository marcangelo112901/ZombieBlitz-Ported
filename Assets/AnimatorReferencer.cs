using Unity.VisualScripting;
using UnityEngine;

public class AnimatorReferencer : MonoBehaviour
{
    AudioManager audioManager;
    private void Awake()
    {
        Transform currentParent = transform.parent;
        while (currentParent != null)
        {
            currentParent.TryGetComponent<AudioManager>(out audioManager);
            if (audioManager != null)
                break;

            currentParent = currentParent.parent;
        }
    }

    public void PlayClip(AudioClip clip)
    {
        audioManager.playClip(clip);
    }
}
