using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject body;
    public GameObject scope;
    public GameObject clip;
    public GameObject muzzleFlash;

    private Animator weaponAnimator;
    private Animator flashAnimator;
    private void Awake()
    {
        weaponAnimator = GetComponent<Animator>();
        flashAnimator = muzzleFlash.GetComponent<Animator>();
    }

    public void shoot()
    {
        int random = UnityEngine.Random.Range(0, 4);
        weaponAnimator.SetTrigger("shoot");
        flashAnimator.SetInteger("random", random);
        flashAnimator.SetTrigger("shoot");
    }

}
